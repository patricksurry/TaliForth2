\ configure a device with 512 byte blocks at the start of blockfile

$f010       constant blkio
$10         constant p8dvc          \ device #1 << 4

: p8read ( addr blk )
    [ blkio 2 + ] literal ! [ blkio 4 + ] literal ! [ p8dvc 1 or ] literal blkio c! ;

: p8boot ( -- )
    blkbuffer dup 0 p8read      \ read boot sector (TODO: should verify)
    $200 evaluate* execute      \ get xt for loader and run it
;

\ p8init
9  blkio 2 +  !   p8dvc 7 or  blkio  c!
