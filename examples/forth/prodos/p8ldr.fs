TF
: p8find ( -- addr|0 )
	6 2 do
		blkbuffer dup i p8read
		$23 - begin ( addr )
			$27 + dup $ff and $ff <> if
				dup c@ $29 = \ typ|n
				over 1+ 9 s" PRODOS.FS" compare 0=
				and ( addr t|f )
			else 0 swap ( 0 t ) then
		until
		?dup if unloop exit then
	loop 0 ;
: blk@ blkbuffer + dup c@ swap $100 + c@ 8 lshift or ;
: p8load ( addr n+1 key -- addr u )
	blkbuffer swap p8read 1- 0 tuck
	do 2dup + i blk@ p8read $200 + loop ;
:noname p8find dup if $11 + 2@ $2000 -rot p8load evaluate* then ;

