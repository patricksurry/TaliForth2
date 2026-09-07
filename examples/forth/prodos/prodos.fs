\ read-only PRODOS FS

2           constant volblk
blkbuffer dup
            constant p8buf0         \ used for metadata blocks
    $200 +  constant p8buf1         \ used for data blocks

            variable cwd
            volblk cwd !

: p8read0 ( addr blk|0 -- )
    \ read block to addr, where 0 means empty fill
    ?dup if p8read else $200 erase then
;
: p8read0[] ( u i addr blk|0 -- )
    \ read index i:i+u of a blk to addr via the data buffer
    p8buf1 swap p8read0
    ( u i addr )
    -rot p8buf1 + -rot
    ( buf+i addr u )
    cmove
;

: entryt ( entry -- type )      c@ 4 rshift ;
: isdir? ( entry -- t|f )       entryt 8 and 0<> ;
: entry$ ( entry -- addr u )    dup 1+ swap c@ $f and ;
: entry@ ( entry -- keyblk )    $11 + @ ;
: entry# ( entry -- ud )
    dup isdir? if
        $21 + @ 0               \ file count, as ud
    else
        dup $15 + @ swap $17 + c@   \ 3 bytes in little endian order
    then
;

: entry> ( entry -- entry'|0 )
    \ advance to next header entry, assuming page aligned blocks
    \ the odd entry size makes it easy to tell when we wrap since
    \ the LSB of each entry is unique, with last + $27 having LSB $ff.
    \ (in reverse, the first entry at $04 wraps down to LSB $dd)
    $27 + dup $ff and $ff = if
        ( $..ff )   \ entry would wrap so grab next block
        $fe00 and dup 2 + @ ?dup if
            ( addr0 blk ) over swap p8read 4 +
        else
            ( addr0 ) \ no next block
            drop 0
        then
    then
    ( entry'|0 )
;

\ provided by loader
\ : blk@ ( i -- blk )
\     \ get block number from index block
\     \ note that the low bytes are in bytes 0-127 of the index block,
\     \ and corresponding high bytes are in bytes 128-255
\     p8buf0 + dup
\     c@ swap $100 + c@ 8 lshift or
\ ;

: dirhdr ( dirblk -- dirhdr )
    \ read the first block of the directory to metadata buffer,
    \ returning the directory header entry
    p8buf0 dup rot p8read0 4 +
;

: dir-find ( dirblk addr u -- entry|0 )
    \ find entry for name in the directory at dirblk

    2>r dirhdr
    begin
        entry> dup if
            ( entry )
            dup entryt if
                \ only check active entries
                dup entry$ 2r@ compare 0=
                ( entry ?match)
            else
                false
            then
            ( entry ?match )
        else
            true
            ( 0 true )      \ exit for end of directory
        then
    until
    2r> 2drop
;

\ fopen, fseek and fnextblk provide a block iterator for ProDOS data files

variable _ftyp      \ 1, 2 or 3
variable _fkey      \ the key block for a file
variable _floc      \ current block offset from start of file

: offset/mod ( ud -- #blks #bytes )
    \ convert a double word file offset to block number and byte offset
    \ the low 9 bits give the offset, and the high 15 bits (3 bytes total) give the block number
    over 9 rshift swap 7 lshift or
    swap $1ff and
;

: fseek ( ud -- blk #bytes )
    \ seek to given position returning data block and offset within it
    offset/mod swap _floc !

     _fkey @  _ftyp @
    begin
        ( blk typ )
        1- ?dup while
        _floc @
        ( blk typ loc )
        \ fetch hi or low byte of index word
        over 2 = if 8 rshift else $ff and then
        ( blk typ i )
        rot p8buf0 swap p8read0 blk@ swap
        ( blk' typ )
    repeat
    swap
;

: fnextblk ( -- blk )
    \ advance to next block
    _floc @  1+
    dup $ff and ?dup if
        \ incrementing lower byte so we already have index block
        ( loc loclo )
        swap _floc !  blk@
    else
        \ wrapping to next index block (i.e. level 3), so reset
        ( loc )
        fseek
    then
;

: fopen ( entry -- )
    \ prepare to iterate on a data file from entry
    dup entry@ _fkey !  entryt _ftyp !
;

: fread ( addr u ud -- addr u )
    \ read u>0 bytes to addr from file offset ud

    fseek 2over 2>r

    ( addr u blk i ) ( R: addr u )

    \ partial block to start?
    ?dup if
        $200 over -  r@ min
        ( .. blk i k  R: addr u )
        dup 2r@ rot /string
        ( .. blk i k addr' u' ) ( R: addr u )
        2r> drop -rot 2>r
        ( .. blk i k addr ) ( R: addr' u' )
        2swap -rot
        ( .. k i addr blk ) ( R: addr' u' )
        p8read0[]
        fnextblk
        ( .. blk' ) ( R: addr' u' )
    then

    \ full blocks
    begin
        r@ $1ff u> while
        2r> over swap $200 /string 2>r swap
        ( .. addr blk ) ( R: addr' u' )
        p8read0
        fnextblk
        ( .. blk' ) ( R: addr' u' )
    repeat

    \ partial block to end?
    2r> ?dup if
        ( .. blk addr u )
        0 2swap swap
        ( .. k i addr blk )
        p8read0[]
    else
        2drop
    then
    ( addr u )
;


: ?entry. ( entry -- )
    \ show a line for active entry
    dup entryt ?dup if
        \ only show active entries
        9 emit . space dup entry$ type 9 emit entry# ud. cr
    else
        drop
    then
;

: >entry ( dirhdr blk -- entry' )
    \ kludge a directory header into self-referential file entry
    \ so that entryt is a directory and entry@ points to self
    over $11 + !     \ write self block number for entry@ into the served area
;

: dir. ( dirhdr -- )
    \ show a directory listing from a volume or subdirectory key block
    \ note this stomps the directory buffer
    \ for example: volblk dirhdr dir.

    cr dup entry# ud. ." files in " dup entry$ type
    cr begin
        entry> ?dup while
        dup ?entry.
    repeat
;

: uc ( c -- C )
    dup
    [char] a [ char z 1+ ] literal within
    $20 and xor
;

: upper ( addr u -- )
    bounds ?do i c@ uc i c! loop
;

: path-find ( addr u -- entry|0 )
    2dup upper
    over c@ [char] / = if
        1 /string volblk
    else
        cwd @
    then
    ( addr u blk )
    over 0= if
        \ if path is empty, kludge a file entry that points to self
        >r 2drop r@ dirhdr r> >entry exit
    then

    begin
        >r [char] / split r> -rot dir-find
        ( addr' u' entry|0 )
        \ if more path, i.e. u' > 0, check entry is directory
        over if dup isdir? and then
        \ if either u' or entry is 0 we're done
        ( addr' u' entry|0 )
        over 0<> over 0<> and while
        entry@
    repeat
    nip nip
;

: path-entry ( "path" -- entry )
    parse-name path-find ?dup 0= abort" Not found"
;

: file-entry ( "path" -- entry )
    path-entry dup isdir? abort" Not a file"
;

: ls ( "path" -- )
    path-entry dup isdir? if
        entry@ dirhdr dir.
    else
        cr ?entry.
    then
;

: cd ( "path" -- )
    path-entry dup isdir? 0= abort" Not a directory"
    entry@ cwd !
;


: load-file ( addr "name" -- addr u )
    file-entry
    ( addr entry )
    dup fopen entry# drop 0. fread
;

variable eom                    \ top of memory for include
unused here + eom !

: include ( "name" -- )
    file-entry
    ( entry )
    dup fopen entry# drop       \ assume size <64K
    ( u )
    eom @ dup >r over -         \ stash eom then reduce by u
    ( u addr  R: eom )
    dup eom ! swap              \ save new eom below included file
    ( addr u )
    0. fread evaluate*          \ read and evaluate file contents
    r> eom !                    \ restore eom
;



