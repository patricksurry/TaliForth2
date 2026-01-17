\ Test code adapted from TaliForth
\ These can be run in batch using a command like:
\  cat examples/forth/emu65c02.fs examples/forth/emu65c02_test.fs | tools/c65/c65 -r taliforth-c65.bin > results.txt

hex

variable actual-depth   \ stack record
create actual-results  20 cells allot


\ Empty stack: handles underflowed stack too
: empty-stack ( ... -- )
   depth ?dup if
      dup 0< if
         negate 0 do 0 loop
      else
         0 do drop loop
      then
   then ;

\ Print the previous test's actual results. Added by SamCo 2018-05
: show-results ( -- )
   cr s"  ACTUAL RESULT: { " type
   actual-depth @ 0 ?do
      actual-results
      actual-depth @ i - 1- \ Print them in reverse order to match test.
      cells + @ .
   loop
   s" }" type ;

\ Display an error message followed by the line that had the error
: error  \ ( C-ADDR U -- )
   type source type \ display line corresponding to error
   empty-stack      \ throw away every thing else
   show-results ;   \ added by SamCo to show what actually happened

\ Syntactic sugar
: T{  ( -- ) ;

\ Record depth and content of stack
: ->  ( ... -- )
   depth dup actual-depth !  \ record depth
   ?dup if                   \ if there is something on stack ...
      0 do
         actual-results i cells + !
      loop                   \ ... save it
   then ;

\ Compare stack (expected) contents with saved (actual) contents
: }T  ( ... -- )
   depth actual-depth @ = if     \ if depths match
      depth ?dup if              \ if there is something on the stack
         0 do                    \ for each stack item
            actual-results i cells + @  \ compare actual with expected
            <> if
               cr s" INCORRECT RESULT: " error leave
            then
         loop
      then
   else                          \ depth mismatch
      cr s" WRONG NUMBER OF RESULTS: " error
   then ;

\ Tests extracted from https://github.com/SingleStepTests/ProcessorTests/tree/main/wdc65c02
\ This is a random sampling of 16 tests per opcode, avoiding tests that access the
\ same emulated 12-bit memory location via distinct 16-bit synonyms

( 00 )
$663e >PC $5b >S $64 >A $87 >X $d3 >Y $23 >P $1fdc $aa >M $663e $00 >M $663f $28 >M $6640 $75 >M $fffe $dc >M $ffff $1f >M
T{ op PC S A X Y P -> $1fdc $58 $64 $87 $d3 $27 }T T{ $0159 M $015a M $015b M $1fdc M $663e M $663f M $6640 M $fffe M $ffff M -> $33 $40 $66 $aa $00 $28 $75 $dc $1f }T
$3331 >PC $ed >S $02 >A $1a >X $01 >Y $a6 >P $3331 $00 >M $3332 $e4 >M $3333 $7a >M $422b $73 >M $fffe $2b >M $ffff $42 >M
T{ op PC S A X Y P -> $422b $ea $02 $1a $01 $a6 }T T{ $01eb M $01ec M $01ed M $3331 M $3332 M $3333 M $422b M $fffe M $ffff M -> $b6 $33 $33 $00 $e4 $7a $73 $2b $42 }T
$d309 >PC $a2 >S $43 >A $d9 >X $72 >Y $a3 >P $c6f6 $b9 >M $d309 $00 >M $d30a $40 >M $d30b $5a >M $fffe $f6 >M $ffff $c6 >M
T{ op PC S A X Y P -> $c6f6 $9f $43 $d9 $72 $a7 }T T{ $01a0 M $01a1 M $01a2 M $c6f6 M $d309 M $d30a M $d30b M $fffe M $ffff M -> $b3 $0b $d3 $b9 $00 $40 $5a $f6 $c6 }T
$d081 >PC $e8 >S $17 >A $ff >X $a4 >Y $a3 >P $ad92 $bf >M $d081 $00 >M $d082 $66 >M $d083 $34 >M $fffe $92 >M $ffff $ad >M
T{ op PC S A X Y P -> $ad92 $e5 $17 $ff $a4 $a7 }T T{ $01e6 M $01e7 M $01e8 M $ad92 M $d081 M $d082 M $d083 M $fffe M $ffff M -> $b3 $83 $d0 $bf $00 $66 $34 $92 $ad }T
$c7a0 >PC $6d >S $7b >A $8e >X $cc >Y $64 >P $315b $ff >M $c7a0 $00 >M $c7a1 $7e >M $c7a2 $54 >M $fffe $5b >M $ffff $31 >M
T{ op PC S A X Y P -> $315b $6a $7b $8e $cc $64 }T T{ $016b M $016c M $016d M $315b M $c7a0 M $c7a1 M $c7a2 M $fffe M $ffff M -> $74 $a2 $c7 $ff $00 $7e $54 $5b $31 }T
$3de2 >PC $75 >S $b0 >A $3c >X $32 >Y $e6 >P $3de2 $00 >M $3de3 $4c >M $3de4 $4e >M $6573 $91 >M $fffe $73 >M $ffff $65 >M
T{ op PC S A X Y P -> $6573 $72 $b0 $3c $32 $e6 }T T{ $0173 M $0174 M $0175 M $3de2 M $3de3 M $3de4 M $6573 M $fffe M $ffff M -> $f6 $e4 $3d $00 $4c $4e $91 $73 $65 }T
$7bd2 >PC $16 >S $ba >A $71 >X $f5 >Y $26 >P $7868 $33 >M $7bd2 $00 >M $7bd3 $b3 >M $7bd4 $04 >M $fffe $68 >M $ffff $78 >M
T{ op PC S A X Y P -> $7868 $13 $ba $71 $f5 $26 }T T{ $0114 M $0115 M $0116 M $7868 M $7bd2 M $7bd3 M $7bd4 M $fffe M $ffff M -> $36 $d4 $7b $33 $00 $b3 $04 $68 $78 }T
$22d8 >PC $64 >S $23 >A $03 >X $88 >Y $e2 >P $22d8 $00 >M $22d9 $b8 >M $22da $cb >M $caa3 $f1 >M $fffe $a3 >M $ffff $ca >M
T{ op PC S A X Y P -> $caa3 $61 $23 $03 $88 $e6 }T T{ $0162 M $0163 M $0164 M $22d8 M $22d9 M $22da M $caa3 M $fffe M $ffff M -> $f2 $da $22 $00 $b8 $cb $f1 $a3 $ca }T
$733e >PC $33 >S $c2 >A $00 >X $c4 >Y $e0 >P $2ae7 $08 >M $733e $00 >M $733f $15 >M $7340 $a2 >M $fffe $e7 >M $ffff $2a >M
T{ op PC S A X Y P -> $2ae7 $30 $c2 $00 $c4 $e4 }T T{ $0131 M $0132 M $0133 M $2ae7 M $733e M $733f M $7340 M $fffe M $ffff M -> $f0 $40 $73 $08 $00 $15 $a2 $e7 $2a }T
$eda1 >PC $96 >S $79 >A $dc >X $7b >Y $66 >P $2409 $6f >M $eda1 $00 >M $eda2 $c5 >M $eda3 $6e >M $fffe $09 >M $ffff $24 >M
T{ op PC S A X Y P -> $2409 $93 $79 $dc $7b $66 }T T{ $0194 M $0195 M $0196 M $2409 M $eda1 M $eda2 M $eda3 M $fffe M $ffff M -> $76 $a3 $ed $6f $00 $c5 $6e $09 $24 }T
$2001 >PC $d1 >S $69 >A $db >X $e2 >Y $23 >P $2001 $00 >M $2002 $31 >M $2003 $b0 >M $2712 $a5 >M $fffe $12 >M $ffff $27 >M
T{ op PC S A X Y P -> $2712 $ce $69 $db $e2 $27 }T T{ $01cf M $01d0 M $01d1 M $2001 M $2002 M $2003 M $2712 M $fffe M $ffff M -> $33 $03 $20 $00 $31 $b0 $a5 $12 $27 }T
$36c8 >PC $34 >S $43 >A $af >X $2b >Y $25 >P $36c8 $00 >M $36c9 $fa >M $36ca $13 >M $72dc $6f >M $fffe $dc >M $ffff $72 >M
T{ op PC S A X Y P -> $72dc $31 $43 $af $2b $25 }T T{ $0132 M $0133 M $0134 M $36c8 M $36c9 M $36ca M $72dc M $fffe M $ffff M -> $35 $ca $36 $00 $fa $13 $6f $dc $72 }T
$f8fe >PC $24 >S $af >A $45 >X $5e >Y $a2 >P $bf30 $82 >M $f8fe $00 >M $f8ff $69 >M $f900 $0d >M $fffe $30 >M $ffff $bf >M
T{ op PC S A X Y P -> $bf30 $21 $af $45 $5e $a6 }T T{ $0122 M $0123 M $0124 M $bf30 M $f8fe M $f8ff M $f900 M $fffe M $ffff M -> $b2 $00 $f9 $82 $00 $69 $0d $30 $bf }T
$aae9 >PC $0a >S $0f >A $de >X $39 >Y $60 >P $5835 $6a >M $aae9 $00 >M $aaea $f4 >M $aaeb $8c >M $fffe $35 >M $ffff $58 >M
T{ op PC S A X Y P -> $5835 $07 $0f $de $39 $64 }T T{ $0108 M $0109 M $010a M $5835 M $aae9 M $aaea M $aaeb M $fffe M $ffff M -> $70 $eb $aa $6a $00 $f4 $8c $35 $58 }T
$b825 >PC $71 >S $1c >A $c9 >X $18 >Y $65 >P $5de9 $25 >M $b825 $00 >M $b826 $ff >M $b827 $25 >M $fffe $e9 >M $ffff $5d >M
T{ op PC S A X Y P -> $5de9 $6e $1c $c9 $18 $65 }T T{ $016f M $0170 M $0171 M $5de9 M $b825 M $b826 M $b827 M $fffe M $ffff M -> $75 $27 $b8 $25 $00 $ff $25 $e9 $5d }T
$4097 >PC $ef >S $f3 >A $a1 >X $43 >Y $a3 >P $4097 $00 >M $4098 $18 >M $4099 $0c >M $917a $35 >M $fffe $7a >M $ffff $91 >M
T{ op PC S A X Y P -> $917a $ec $f3 $a1 $43 $a7 }T T{ $01ed M $01ee M $01ef M $4097 M $4098 M $4099 M $917a M $fffe M $ffff M -> $b3 $99 $40 $00 $18 $0c $35 $7a $91 }T
( 01 )
$a719 >PC $77 >S $c4 >A $de >X $00 >Y $e5 >P $000d $0c >M $00eb $49 >M $00ec $dd >M $a719 $01 >M $a71a $0d >M $a71b $85 >M $dd49 $71 >M
T{ op PC S A X Y P -> $a71b $77 $f5 $de $00 $e5 }T T{ $000d M $00eb M $00ec M $a719 M $a71a M $a71b M $dd49 M -> $0c $49 $dd $01 $0d $85 $71 }T
$cad3 >PC $20 >S $1c >A $c4 >X $69 >Y $a7 >P $0084 $5c >M $0085 $54 >M $00c0 $84 >M $545c $f3 >M $cad3 $01 >M $cad4 $c0 >M $cad5 $fa >M
T{ op PC S A X Y P -> $cad5 $20 $ff $c4 $69 $a5 }T T{ $0084 M $0085 M $00c0 M $545c M $cad3 M $cad4 M $cad5 M -> $5c $54 $84 $f3 $01 $c0 $fa }T
$7806 >PC $d0 >S $50 >A $46 >X $3d >Y $a3 >P $0026 $9a >M $006c $c9 >M $006d $25 >M $25c9 $e1 >M $7806 $01 >M $7807 $26 >M $7808 $55 >M
T{ op PC S A X Y P -> $7808 $d0 $f1 $46 $3d $a1 }T T{ $0026 M $006c M $006d M $25c9 M $7806 M $7807 M $7808 M -> $9a $c9 $25 $e1 $01 $26 $55 }T
$843f >PC $2c >S $05 >A $5d >X $ab >Y $62 >P $0013 $56 >M $0070 $c2 >M $0071 $31 >M $31c2 $d1 >M $843f $01 >M $8440 $13 >M $8441 $d0 >M
T{ op PC S A X Y P -> $8441 $2c $d5 $5d $ab $e0 }T T{ $0013 M $0070 M $0071 M $31c2 M $843f M $8440 M $8441 M -> $56 $c2 $31 $d1 $01 $13 $d0 }T
$ddf4 >PC $cd >S $cf >A $26 >X $43 >Y $63 >P $0057 $d9 >M $007d $ff >M $007e $be >M $beff $6f >M $ddf4 $01 >M $ddf5 $57 >M $ddf6 $7b >M
T{ op PC S A X Y P -> $ddf6 $cd $ef $26 $43 $e1 }T T{ $0057 M $007d M $007e M $beff M $ddf4 M $ddf5 M $ddf6 M -> $d9 $ff $be $6f $01 $57 $7b }T
$e7b2 >PC $a0 >S $07 >A $f9 >X $b9 >Y $a6 >P $006e $d4 >M $006f $e9 >M $0075 $28 >M $e7b2 $01 >M $e7b3 $75 >M $e7b4 $72 >M $e9d4 $11 >M
T{ op PC S A X Y P -> $e7b4 $a0 $17 $f9 $b9 $24 }T T{ $006e M $006f M $0075 M $e7b2 M $e7b3 M $e7b4 M $e9d4 M -> $d4 $e9 $28 $01 $75 $72 $11 }T
$ec80 >PC $4b >S $3f >A $0e >X $21 >Y $22 >P $0006 $20 >M $0014 $93 >M $0015 $64 >M $6493 $78 >M $ec80 $01 >M $ec81 $06 >M $ec82 $25 >M
T{ op PC S A X Y P -> $ec82 $4b $7f $0e $21 $20 }T T{ $0006 M $0014 M $0015 M $6493 M $ec80 M $ec81 M $ec82 M -> $20 $93 $64 $78 $01 $06 $25 }T
$8938 >PC $4c >S $dd >A $05 >X $22 >Y $e1 >P $005c $2d >M $0061 $8f >M $0062 $87 >M $878f $d3 >M $8938 $01 >M $8939 $5c >M $893a $35 >M
T{ op PC S A X Y P -> $893a $4c $df $05 $22 $e1 }T T{ $005c M $0061 M $0062 M $878f M $8938 M $8939 M $893a M -> $2d $8f $87 $d3 $01 $5c $35 }T
$7881 >PC $46 >S $54 >A $da >X $66 >Y $a2 >P $0035 $4c >M $0036 $37 >M $005b $32 >M $374c $54 >M $7881 $01 >M $7882 $5b >M $7883 $dc >M
T{ op PC S A X Y P -> $7883 $46 $54 $da $66 $20 }T T{ $0035 M $0036 M $005b M $374c M $7881 M $7882 M $7883 M -> $4c $37 $32 $54 $01 $5b $dc }T
$6f84 >PC $4f >S $cd >A $28 >X $92 >Y $63 >P $008e $bd >M $00b6 $5e >M $00b7 $7d >M $6f84 $01 >M $6f85 $8e >M $6f86 $72 >M $7d5e $90 >M
T{ op PC S A X Y P -> $6f86 $4f $dd $28 $92 $e1 }T T{ $008e M $00b6 M $00b7 M $6f84 M $6f85 M $6f86 M $7d5e M -> $bd $5e $7d $01 $8e $72 $90 }T
$b140 >PC $55 >S $88 >A $c2 >X $f8 >Y $e4 >P $0019 $1a >M $00db $66 >M $00dc $a7 >M $a766 $db >M $b140 $01 >M $b141 $19 >M $b142 $2b >M
T{ op PC S A X Y P -> $b142 $55 $db $c2 $f8 $e4 }T T{ $0019 M $00db M $00dc M $a766 M $b140 M $b141 M $b142 M -> $1a $66 $a7 $db $01 $19 $2b }T
$3dec >PC $ef >S $b5 >A $2b >X $0b >Y $66 >P $006e $d7 >M $0099 $fd >M $009a $f3 >M $3dec $01 >M $3ded $6e >M $3dee $3c >M $f3fd $eb >M
T{ op PC S A X Y P -> $3dee $ef $ff $2b $0b $e4 }T T{ $006e M $0099 M $009a M $3dec M $3ded M $3dee M $f3fd M -> $d7 $fd $f3 $01 $6e $3c $eb }T
$b41a >PC $a3 >S $1a >A $a3 >X $fa >Y $22 >P $0020 $9f >M $00c3 $70 >M $00c4 $bd >M $b41a $01 >M $b41b $20 >M $b41c $b6 >M $bd70 $0a >M
T{ op PC S A X Y P -> $b41c $a3 $1a $a3 $fa $20 }T T{ $0020 M $00c3 M $00c4 M $b41a M $b41b M $b41c M $bd70 M -> $9f $70 $bd $01 $20 $b6 $0a }T
$46b4 >PC $43 >S $23 >A $a7 >X $0f >Y $a3 >P $0008 $e0 >M $0009 $e7 >M $0061 $91 >M $46b4 $01 >M $46b5 $61 >M $46b6 $d7 >M $e7e0 $b4 >M
T{ op PC S A X Y P -> $46b6 $43 $b7 $a7 $0f $a1 }T T{ $0008 M $0009 M $0061 M $46b4 M $46b5 M $46b6 M $e7e0 M -> $e0 $e7 $91 $01 $61 $d7 $b4 }T
$f1db >PC $2d >S $00 >A $48 >X $d4 >Y $22 >P $000a $05 >M $000b $73 >M $00c2 $70 >M $7305 $b2 >M $f1db $01 >M $f1dc $c2 >M $f1dd $02 >M
T{ op PC S A X Y P -> $f1dd $2d $b2 $48 $d4 $a0 }T T{ $000a M $000b M $00c2 M $7305 M $f1db M $f1dc M $f1dd M -> $05 $73 $70 $b2 $01 $c2 $02 }T
$0f4f >PC $3c >S $68 >A $b9 >X $10 >Y $65 >P $0034 $5c >M $00ed $72 >M $00ee $45 >M $0f4f $01 >M $0f50 $34 >M $0f51 $14 >M $4572 $b9 >M
T{ op PC S A X Y P -> $0f51 $3c $f9 $b9 $10 $e5 }T T{ $0034 M $00ed M $00ee M $0f4f M $0f50 M $0f51 M $4572 M -> $5c $72 $45 $01 $34 $14 $b9 }T
( 02 )
$282a >PC $14 >S $76 >A $0a >X $ab >Y $a5 >P $282a $02 >M $282b $55 >M $282c $2c >M
T{ op PC S A X Y P -> $282c $14 $76 $0a $ab $a5 }T T{ $282a M $282b M $282c M -> $02 $55 $2c }T
$2a33 >PC $e9 >S $42 >A $6f >X $22 >Y $23 >P $2a33 $02 >M $2a34 $6f >M $2a35 $94 >M
T{ op PC S A X Y P -> $2a35 $e9 $42 $6f $22 $23 }T T{ $2a33 M $2a34 M $2a35 M -> $02 $6f $94 }T
$02d1 >PC $27 >S $8f >A $50 >X $cf >Y $e7 >P $02d1 $02 >M $02d2 $76 >M $02d3 $69 >M
T{ op PC S A X Y P -> $02d3 $27 $8f $50 $cf $e7 }T T{ $02d1 M $02d2 M $02d3 M -> $02 $76 $69 }T
$40b2 >PC $35 >S $02 >A $e6 >X $51 >Y $62 >P $40b2 $02 >M $40b3 $b6 >M $40b4 $fa >M
T{ op PC S A X Y P -> $40b4 $35 $02 $e6 $51 $62 }T T{ $40b2 M $40b3 M $40b4 M -> $02 $b6 $fa }T
$4660 >PC $ea >S $c6 >A $13 >X $76 >Y $21 >P $4660 $02 >M $4661 $58 >M $4662 $d3 >M
T{ op PC S A X Y P -> $4662 $ea $c6 $13 $76 $21 }T T{ $4660 M $4661 M $4662 M -> $02 $58 $d3 }T
$06d6 >PC $fa >S $df >A $46 >X $94 >Y $67 >P $06d6 $02 >M $06d7 $3a >M $06d8 $57 >M
T{ op PC S A X Y P -> $06d8 $fa $df $46 $94 $67 }T T{ $06d6 M $06d7 M $06d8 M -> $02 $3a $57 }T
$7cb0 >PC $ec >S $51 >A $5d >X $1c >Y $e7 >P $7cb0 $02 >M $7cb1 $9d >M $7cb2 $2e >M
T{ op PC S A X Y P -> $7cb2 $ec $51 $5d $1c $e7 }T T{ $7cb0 M $7cb1 M $7cb2 M -> $02 $9d $2e }T
$31c1 >PC $12 >S $0a >A $19 >X $5a >Y $a3 >P $31c1 $02 >M $31c2 $a8 >M $31c3 $39 >M
T{ op PC S A X Y P -> $31c3 $12 $0a $19 $5a $a3 }T T{ $31c1 M $31c2 M $31c3 M -> $02 $a8 $39 }T
$cfd7 >PC $00 >S $33 >A $ca >X $86 >Y $67 >P $cfd7 $02 >M $cfd8 $b0 >M $cfd9 $0f >M
T{ op PC S A X Y P -> $cfd9 $00 $33 $ca $86 $67 }T T{ $cfd7 M $cfd8 M $cfd9 M -> $02 $b0 $0f }T
$611e >PC $1a >S $1b >A $fb >X $d9 >Y $a5 >P $611e $02 >M $611f $a0 >M $6120 $a1 >M
T{ op PC S A X Y P -> $6120 $1a $1b $fb $d9 $a5 }T T{ $611e M $611f M $6120 M -> $02 $a0 $a1 }T
$0e2f >PC $00 >S $0f >A $d4 >X $18 >Y $a5 >P $0e2f $02 >M $0e30 $38 >M $0e31 $cc >M
T{ op PC S A X Y P -> $0e31 $00 $0f $d4 $18 $a5 }T T{ $0e2f M $0e30 M $0e31 M -> $02 $38 $cc }T
$09dd >PC $f7 >S $34 >A $95 >X $65 >Y $a6 >P $09dd $02 >M $09de $54 >M $09df $ac >M
T{ op PC S A X Y P -> $09df $f7 $34 $95 $65 $a6 }T T{ $09dd M $09de M $09df M -> $02 $54 $ac }T
$aeaa >PC $63 >S $2b >A $7d >X $3d >Y $26 >P $aeaa $02 >M $aeab $7b >M $aeac $16 >M
T{ op PC S A X Y P -> $aeac $63 $2b $7d $3d $26 }T T{ $aeaa M $aeab M $aeac M -> $02 $7b $16 }T
$3aae >PC $76 >S $4a >A $86 >X $1a >Y $27 >P $3aae $02 >M $3aaf $f2 >M $3ab0 $4b >M
T{ op PC S A X Y P -> $3ab0 $76 $4a $86 $1a $27 }T T{ $3aae M $3aaf M $3ab0 M -> $02 $f2 $4b }T
$fc52 >PC $e8 >S $c5 >A $58 >X $f1 >Y $24 >P $fc52 $02 >M $fc53 $c3 >M $fc54 $71 >M
T{ op PC S A X Y P -> $fc54 $e8 $c5 $58 $f1 $24 }T T{ $fc52 M $fc53 M $fc54 M -> $02 $c3 $71 }T
$2b84 >PC $78 >S $8e >A $81 >X $a0 >Y $60 >P $2b84 $02 >M $2b85 $cd >M $2b86 $d8 >M
T{ op PC S A X Y P -> $2b86 $78 $8e $81 $a0 $60 }T T{ $2b84 M $2b85 M $2b86 M -> $02 $cd $d8 }T
( 03 )
$ea5f >PC $5b >S $08 >A $34 >X $cd >Y $27 >P $ea5f $03 >M $ea60 $5f >M $ea61 $f9 >M
T{ op PC S A X Y P -> $ea60 $5b $08 $34 $cd $27 }T T{ $ea5f M $ea60 M $ea61 M -> $03 $5f $f9 }T
$9376 >PC $8b >S $28 >A $72 >X $95 >Y $63 >P $9376 $03 >M $9377 $5a >M $9378 $ab >M
T{ op PC S A X Y P -> $9377 $8b $28 $72 $95 $63 }T T{ $9376 M $9377 M $9378 M -> $03 $5a $ab }T
$09df >PC $3d >S $96 >A $34 >X $f8 >Y $64 >P $09df $03 >M $09e0 $53 >M $09e1 $50 >M
T{ op PC S A X Y P -> $09e0 $3d $96 $34 $f8 $64 }T T{ $09df M $09e0 M $09e1 M -> $03 $53 $50 }T
$1c87 >PC $fa >S $95 >A $6d >X $cb >Y $67 >P $1c87 $03 >M $1c88 $5e >M $1c89 $50 >M
T{ op PC S A X Y P -> $1c88 $fa $95 $6d $cb $67 }T T{ $1c87 M $1c88 M $1c89 M -> $03 $5e $50 }T
$4bc2 >PC $88 >S $fa >A $6a >X $63 >Y $e3 >P $4bc2 $03 >M $4bc3 $06 >M $4bc4 $06 >M
T{ op PC S A X Y P -> $4bc3 $88 $fa $6a $63 $e3 }T T{ $4bc2 M $4bc3 M $4bc4 M -> $03 $06 $06 }T
$1d10 >PC $3f >S $89 >A $a7 >X $f7 >Y $24 >P $1d10 $03 >M $1d11 $a3 >M $1d12 $0d >M
T{ op PC S A X Y P -> $1d11 $3f $89 $a7 $f7 $24 }T T{ $1d10 M $1d11 M $1d12 M -> $03 $a3 $0d }T
$3ad3 >PC $05 >S $e3 >A $f5 >X $c8 >Y $22 >P $3ad3 $03 >M $3ad4 $10 >M $3ad5 $75 >M
T{ op PC S A X Y P -> $3ad4 $05 $e3 $f5 $c8 $22 }T T{ $3ad3 M $3ad4 M $3ad5 M -> $03 $10 $75 }T
$6b04 >PC $09 >S $26 >A $dd >X $55 >Y $64 >P $6b04 $03 >M $6b05 $b8 >M $6b06 $ac >M
T{ op PC S A X Y P -> $6b05 $09 $26 $dd $55 $64 }T T{ $6b04 M $6b05 M $6b06 M -> $03 $b8 $ac }T
$a9c6 >PC $b3 >S $a8 >A $47 >X $e7 >Y $60 >P $a9c6 $03 >M $a9c7 $84 >M $a9c8 $a6 >M
T{ op PC S A X Y P -> $a9c7 $b3 $a8 $47 $e7 $60 }T T{ $a9c6 M $a9c7 M $a9c8 M -> $03 $84 $a6 }T
$1382 >PC $06 >S $af >A $0e >X $98 >Y $e1 >P $1382 $03 >M $1383 $cd >M $1384 $78 >M
T{ op PC S A X Y P -> $1383 $06 $af $0e $98 $e1 }T T{ $1382 M $1383 M $1384 M -> $03 $cd $78 }T
$f09d >PC $73 >S $ae >A $84 >X $b6 >Y $a4 >P $f09d $03 >M $f09e $2c >M $f09f $05 >M
T{ op PC S A X Y P -> $f09e $73 $ae $84 $b6 $a4 }T T{ $f09d M $f09e M $f09f M -> $03 $2c $05 }T
$23f1 >PC $58 >S $3e >A $37 >X $79 >Y $26 >P $23f1 $03 >M $23f2 $95 >M $23f3 $2e >M
T{ op PC S A X Y P -> $23f2 $58 $3e $37 $79 $26 }T T{ $23f1 M $23f2 M $23f3 M -> $03 $95 $2e }T
$5270 >PC $cf >S $51 >A $46 >X $c6 >Y $67 >P $5270 $03 >M $5271 $f8 >M $5272 $4c >M
T{ op PC S A X Y P -> $5271 $cf $51 $46 $c6 $67 }T T{ $5270 M $5271 M $5272 M -> $03 $f8 $4c }T
$0498 >PC $de >S $db >A $3d >X $be >Y $e2 >P $0498 $03 >M $0499 $63 >M $049a $67 >M
T{ op PC S A X Y P -> $0499 $de $db $3d $be $e2 }T T{ $0498 M $0499 M $049a M -> $03 $63 $67 }T
$befa >PC $48 >S $05 >A $0d >X $7f >Y $61 >P $befa $03 >M $befb $7c >M $befc $4f >M
T{ op PC S A X Y P -> $befb $48 $05 $0d $7f $61 }T T{ $befa M $befb M $befc M -> $03 $7c $4f }T
$e31d >PC $7e >S $82 >A $2e >X $bc >Y $23 >P $e31d $03 >M $e31e $92 >M $e31f $e2 >M
T{ op PC S A X Y P -> $e31e $7e $82 $2e $bc $23 }T T{ $e31d M $e31e M $e31f M -> $03 $92 $e2 }T
( 04 )
$bf10 >PC $ba >S $0a >A $af >X $2f >Y $a2 >P $0020 $b5 >M $bf10 $04 >M $bf11 $20 >M $bf12 $a0 >M
T{ op PC S A X Y P -> $bf12 $ba $0a $af $2f $a2 }T T{ $0020 M $bf10 M $bf11 M $bf12 M -> $bf $04 $20 $a0 }T
$78ff >PC $f7 >S $43 >A $7f >X $18 >Y $66 >P $0095 $23 >M $78ff $04 >M $7900 $95 >M $7901 $03 >M
T{ op PC S A X Y P -> $7901 $f7 $43 $7f $18 $64 }T T{ $0095 M $78ff M $7900 M $7901 M -> $63 $04 $95 $03 }T
$c287 >PC $e8 >S $d6 >A $25 >X $bd >Y $22 >P $0008 $a0 >M $c287 $04 >M $c288 $08 >M $c289 $3f >M
T{ op PC S A X Y P -> $c289 $e8 $d6 $25 $bd $20 }T T{ $0008 M $c287 M $c288 M $c289 M -> $f6 $04 $08 $3f }T
$05f6 >PC $6b >S $72 >A $02 >X $86 >Y $a1 >P $0084 $80 >M $05f6 $04 >M $05f7 $84 >M $05f8 $1a >M
T{ op PC S A X Y P -> $05f8 $6b $72 $02 $86 $a3 }T T{ $0084 M $05f6 M $05f7 M $05f8 M -> $f2 $04 $84 $1a }T
$7104 >PC $73 >S $bb >A $5b >X $06 >Y $e0 >P $008e $2c >M $7104 $04 >M $7105 $8e >M $7106 $3e >M
T{ op PC S A X Y P -> $7106 $73 $bb $5b $06 $e0 }T T{ $008e M $7104 M $7105 M $7106 M -> $bf $04 $8e $3e }T
$2152 >PC $97 >S $52 >A $eb >X $e6 >Y $24 >P $000e $89 >M $2152 $04 >M $2153 $0e >M $2154 $b4 >M
T{ op PC S A X Y P -> $2154 $97 $52 $eb $e6 $26 }T T{ $000e M $2152 M $2153 M $2154 M -> $db $04 $0e $b4 }T
$2072 >PC $e4 >S $b7 >A $b9 >X $c6 >Y $20 >P $0093 $92 >M $2072 $04 >M $2073 $93 >M $2074 $45 >M
T{ op PC S A X Y P -> $2074 $e4 $b7 $b9 $c6 $20 }T T{ $0093 M $2072 M $2073 M $2074 M -> $b7 $04 $93 $45 }T
$d341 >PC $a9 >S $01 >A $b2 >X $9b >Y $a6 >P $00c8 $05 >M $d341 $04 >M $d342 $c8 >M $d343 $80 >M
T{ op PC S A X Y P -> $d343 $a9 $01 $b2 $9b $a4 }T T{ $00c8 M $d341 M $d342 M $d343 M -> $05 $04 $c8 $80 }T
$511f >PC $b5 >S $86 >A $83 >X $79 >Y $64 >P $004f $53 >M $511f $04 >M $5120 $4f >M $5121 $8a >M
T{ op PC S A X Y P -> $5121 $b5 $86 $83 $79 $64 }T T{ $004f M $511f M $5120 M $5121 M -> $d7 $04 $4f $8a }T
$a440 >PC $c2 >S $43 >A $69 >X $11 >Y $25 >P $00e2 $e2 >M $a440 $04 >M $a441 $e2 >M $a442 $18 >M
T{ op PC S A X Y P -> $a442 $c2 $43 $69 $11 $25 }T T{ $00e2 M $a440 M $a441 M $a442 M -> $e3 $04 $e2 $18 }T
$3021 >PC $04 >S $5b >A $49 >X $d7 >Y $a3 >P $00c8 $d7 >M $3021 $04 >M $3022 $c8 >M $3023 $2e >M
T{ op PC S A X Y P -> $3023 $04 $5b $49 $d7 $a1 }T T{ $00c8 M $3021 M $3022 M $3023 M -> $df $04 $c8 $2e }T
$c69f >PC $1a >S $e2 >A $06 >X $43 >Y $61 >P $00e9 $49 >M $c69f $04 >M $c6a0 $e9 >M $c6a1 $9f >M
T{ op PC S A X Y P -> $c6a1 $1a $e2 $06 $43 $61 }T T{ $00e9 M $c69f M $c6a0 M $c6a1 M -> $eb $04 $e9 $9f }T
$bbe9 >PC $9c >S $5c >A $08 >X $e8 >Y $23 >P $007d $ec >M $bbe9 $04 >M $bbea $7d >M $bbeb $0a >M
T{ op PC S A X Y P -> $bbeb $9c $5c $08 $e8 $21 }T T{ $007d M $bbe9 M $bbea M $bbeb M -> $fc $04 $7d $0a }T
$b88c >PC $06 >S $ea >A $68 >X $59 >Y $e7 >P $0008 $01 >M $b88c $04 >M $b88d $08 >M $b88e $11 >M
T{ op PC S A X Y P -> $b88e $06 $ea $68 $59 $e7 }T T{ $0008 M $b88c M $b88d M $b88e M -> $eb $04 $08 $11 }T
$ef9e >PC $fa >S $3d >A $6b >X $67 >Y $a0 >P $00e6 $56 >M $ef9e $04 >M $ef9f $e6 >M $efa0 $ca >M
T{ op PC S A X Y P -> $efa0 $fa $3d $6b $67 $a0 }T T{ $00e6 M $ef9e M $ef9f M $efa0 M -> $7f $04 $e6 $ca }T
$f7a5 >PC $80 >S $ad >A $61 >X $29 >Y $26 >P $002b $26 >M $f7a5 $04 >M $f7a6 $2b >M $f7a7 $79 >M
T{ op PC S A X Y P -> $f7a7 $80 $ad $61 $29 $24 }T T{ $002b M $f7a5 M $f7a6 M $f7a7 M -> $af $04 $2b $79 }T
( 05 )
$3998 >PC $8d >S $c4 >A $d7 >X $97 >Y $e7 >P $00ae $c1 >M $3998 $05 >M $3999 $ae >M $399a $ca >M
T{ op PC S A X Y P -> $399a $8d $c5 $d7 $97 $e5 }T T{ $00ae M $3998 M $3999 M $399a M -> $c1 $05 $ae $ca }T
$5ba9 >PC $36 >S $a9 >A $48 >X $37 >Y $a0 >P $00af $ea >M $5ba9 $05 >M $5baa $af >M $5bab $92 >M
T{ op PC S A X Y P -> $5bab $36 $eb $48 $37 $a0 }T T{ $00af M $5ba9 M $5baa M $5bab M -> $ea $05 $af $92 }T
$d36a >PC $fb >S $f0 >A $95 >X $fe >Y $e3 >P $004e $8d >M $d36a $05 >M $d36b $4e >M $d36c $a8 >M
T{ op PC S A X Y P -> $d36c $fb $fd $95 $fe $e1 }T T{ $004e M $d36a M $d36b M $d36c M -> $8d $05 $4e $a8 }T
$8257 >PC $cc >S $1f >A $9d >X $0c >Y $e2 >P $00e7 $86 >M $8257 $05 >M $8258 $e7 >M $8259 $5a >M
T{ op PC S A X Y P -> $8259 $cc $9f $9d $0c $e0 }T T{ $00e7 M $8257 M $8258 M $8259 M -> $86 $05 $e7 $5a }T
$4d18 >PC $68 >S $ec >A $ac >X $42 >Y $a6 >P $00b7 $f5 >M $4d18 $05 >M $4d19 $b7 >M $4d1a $4e >M
T{ op PC S A X Y P -> $4d1a $68 $fd $ac $42 $a4 }T T{ $00b7 M $4d18 M $4d19 M $4d1a M -> $f5 $05 $b7 $4e }T
$bcb7 >PC $d8 >S $cb >A $53 >X $33 >Y $22 >P $007f $36 >M $bcb7 $05 >M $bcb8 $7f >M $bcb9 $4b >M
T{ op PC S A X Y P -> $bcb9 $d8 $ff $53 $33 $a0 }T T{ $007f M $bcb7 M $bcb8 M $bcb9 M -> $36 $05 $7f $4b }T
$ef1d >PC $6a >S $69 >A $0b >X $2a >Y $63 >P $0067 $fa >M $ef1d $05 >M $ef1e $67 >M $ef1f $02 >M
T{ op PC S A X Y P -> $ef1f $6a $fb $0b $2a $e1 }T T{ $0067 M $ef1d M $ef1e M $ef1f M -> $fa $05 $67 $02 }T
$d990 >PC $cb >S $1d >A $b1 >X $1c >Y $25 >P $0023 $b1 >M $d990 $05 >M $d991 $23 >M $d992 $b1 >M
T{ op PC S A X Y P -> $d992 $cb $bd $b1 $1c $a5 }T T{ $0023 M $d990 M $d991 M $d992 M -> $b1 $05 $23 $b1 }T
$81f3 >PC $3f >S $fb >A $79 >X $50 >Y $25 >P $00a0 $ed >M $81f3 $05 >M $81f4 $a0 >M $81f5 $c7 >M
T{ op PC S A X Y P -> $81f5 $3f $ff $79 $50 $a5 }T T{ $00a0 M $81f3 M $81f4 M $81f5 M -> $ed $05 $a0 $c7 }T
$469d >PC $1a >S $c9 >A $5c >X $f4 >Y $27 >P $0080 $33 >M $469d $05 >M $469e $80 >M $469f $e3 >M
T{ op PC S A X Y P -> $469f $1a $fb $5c $f4 $a5 }T T{ $0080 M $469d M $469e M $469f M -> $33 $05 $80 $e3 }T
$5b14 >PC $67 >S $2b >A $be >X $ed >Y $e6 >P $00ec $56 >M $5b14 $05 >M $5b15 $ec >M $5b16 $85 >M
T{ op PC S A X Y P -> $5b16 $67 $7f $be $ed $64 }T T{ $00ec M $5b14 M $5b15 M $5b16 M -> $56 $05 $ec $85 }T
$671f >PC $28 >S $e3 >A $e5 >X $8f >Y $21 >P $0016 $f6 >M $671f $05 >M $6720 $16 >M $6721 $33 >M
T{ op PC S A X Y P -> $6721 $28 $f7 $e5 $8f $a1 }T T{ $0016 M $671f M $6720 M $6721 M -> $f6 $05 $16 $33 }T
$3931 >PC $c0 >S $ff >A $09 >X $a0 >Y $a0 >P $00e5 $16 >M $3931 $05 >M $3932 $e5 >M $3933 $e1 >M
T{ op PC S A X Y P -> $3933 $c0 $ff $09 $a0 $a0 }T T{ $00e5 M $3931 M $3932 M $3933 M -> $16 $05 $e5 $e1 }T
$f080 >PC $b8 >S $91 >A $1f >X $c5 >Y $a5 >P $0068 $2e >M $f080 $05 >M $f081 $68 >M $f082 $36 >M
T{ op PC S A X Y P -> $f082 $b8 $bf $1f $c5 $a5 }T T{ $0068 M $f080 M $f081 M $f082 M -> $2e $05 $68 $36 }T
$6199 >PC $11 >S $de >A $83 >X $a2 >Y $a7 >P $0027 $21 >M $6199 $05 >M $619a $27 >M $619b $e0 >M
T{ op PC S A X Y P -> $619b $11 $ff $83 $a2 $a5 }T T{ $0027 M $6199 M $619a M $619b M -> $21 $05 $27 $e0 }T
$4c0f >PC $ef >S $3a >A $30 >X $4e >Y $27 >P $0083 $52 >M $4c0f $05 >M $4c10 $83 >M $4c11 $bf >M
T{ op PC S A X Y P -> $4c11 $ef $7a $30 $4e $25 }T T{ $0083 M $4c0f M $4c10 M $4c11 M -> $52 $05 $83 $bf }T
( 06 )
$3528 >PC $4a >S $c1 >A $02 >X $b7 >Y $26 >P $0090 $d4 >M $3528 $06 >M $3529 $90 >M $352a $bb >M
T{ op PC S A X Y P -> $352a $4a $c1 $02 $b7 $a5 }T T{ $0090 M $3528 M $3529 M $352a M -> $a8 $06 $90 $bb }T
$832d >PC $ce >S $0b >A $c7 >X $0c >Y $a5 >P $00cb $be >M $832d $06 >M $832e $cb >M $832f $d0 >M
T{ op PC S A X Y P -> $832f $ce $0b $c7 $0c $25 }T T{ $00cb M $832d M $832e M $832f M -> $7c $06 $cb $d0 }T
$b8ed >PC $b7 >S $74 >A $a5 >X $24 >Y $e0 >P $00d5 $0f >M $b8ed $06 >M $b8ee $d5 >M $b8ef $46 >M
T{ op PC S A X Y P -> $b8ef $b7 $74 $a5 $24 $60 }T T{ $00d5 M $b8ed M $b8ee M $b8ef M -> $1e $06 $d5 $46 }T
$b663 >PC $ce >S $b7 >A $3a >X $cb >Y $65 >P $007f $b9 >M $b663 $06 >M $b664 $7f >M $b665 $c0 >M
T{ op PC S A X Y P -> $b665 $ce $b7 $3a $cb $65 }T T{ $007f M $b663 M $b664 M $b665 M -> $72 $06 $7f $c0 }T
$79c9 >PC $5c >S $fd >A $7e >X $81 >Y $20 >P $00a1 $0a >M $79c9 $06 >M $79ca $a1 >M $79cb $83 >M
T{ op PC S A X Y P -> $79cb $5c $fd $7e $81 $20 }T T{ $00a1 M $79c9 M $79ca M $79cb M -> $14 $06 $a1 $83 }T
$5e2e >PC $37 >S $27 >A $ea >X $03 >Y $a1 >P $006a $a3 >M $5e2e $06 >M $5e2f $6a >M $5e30 $32 >M
T{ op PC S A X Y P -> $5e30 $37 $27 $ea $03 $21 }T T{ $006a M $5e2e M $5e2f M $5e30 M -> $46 $06 $6a $32 }T
$282e >PC $b3 >S $6d >A $45 >X $76 >Y $62 >P $00ea $a1 >M $282e $06 >M $282f $ea >M $2830 $a2 >M
T{ op PC S A X Y P -> $2830 $b3 $6d $45 $76 $61 }T T{ $00ea M $282e M $282f M $2830 M -> $42 $06 $ea $a2 }T
$b8fc >PC $d6 >S $d2 >A $43 >X $7c >Y $65 >P $0010 $4f >M $b8fc $06 >M $b8fd $10 >M $b8fe $69 >M
T{ op PC S A X Y P -> $b8fe $d6 $d2 $43 $7c $e4 }T T{ $0010 M $b8fc M $b8fd M $b8fe M -> $9e $06 $10 $69 }T
$da3a >PC $f7 >S $87 >A $a2 >X $55 >Y $61 >P $0036 $c6 >M $da3a $06 >M $da3b $36 >M $da3c $03 >M
T{ op PC S A X Y P -> $da3c $f7 $87 $a2 $55 $e1 }T T{ $0036 M $da3a M $da3b M $da3c M -> $8c $06 $36 $03 }T
$a511 >PC $8d >S $34 >A $70 >X $60 >Y $e1 >P $00d6 $08 >M $a511 $06 >M $a512 $d6 >M $a513 $d6 >M
T{ op PC S A X Y P -> $a513 $8d $34 $70 $60 $60 }T T{ $00d6 M $a511 M $a512 M $a513 M -> $10 $06 $d6 $d6 }T
$6508 >PC $65 >S $2a >A $f1 >X $90 >Y $61 >P $00b4 $a3 >M $6508 $06 >M $6509 $b4 >M $650a $ff >M
T{ op PC S A X Y P -> $650a $65 $2a $f1 $90 $61 }T T{ $00b4 M $6508 M $6509 M $650a M -> $46 $06 $b4 $ff }T
$7d24 >PC $41 >S $8f >A $a7 >X $c0 >Y $e4 >P $0054 $d3 >M $7d24 $06 >M $7d25 $54 >M $7d26 $12 >M
T{ op PC S A X Y P -> $7d26 $41 $8f $a7 $c0 $e5 }T T{ $0054 M $7d24 M $7d25 M $7d26 M -> $a6 $06 $54 $12 }T
$2979 >PC $a4 >S $82 >A $d4 >X $a4 >Y $e2 >P $00c6 $1f >M $2979 $06 >M $297a $c6 >M $297b $51 >M
T{ op PC S A X Y P -> $297b $a4 $82 $d4 $a4 $60 }T T{ $00c6 M $2979 M $297a M $297b M -> $3e $06 $c6 $51 }T
$3fce >PC $18 >S $13 >A $23 >X $15 >Y $a0 >P $0060 $f7 >M $3fce $06 >M $3fcf $60 >M $3fd0 $5d >M
T{ op PC S A X Y P -> $3fd0 $18 $13 $23 $15 $a1 }T T{ $0060 M $3fce M $3fcf M $3fd0 M -> $ee $06 $60 $5d }T
$3ea0 >PC $63 >S $51 >A $da >X $96 >Y $a1 >P $008e $6f >M $3ea0 $06 >M $3ea1 $8e >M $3ea2 $87 >M
T{ op PC S A X Y P -> $3ea2 $63 $51 $da $96 $a0 }T T{ $008e M $3ea0 M $3ea1 M $3ea2 M -> $de $06 $8e $87 }T
$d0c5 >PC $78 >S $bc >A $ca >X $e2 >Y $a1 >P $004d $82 >M $d0c5 $06 >M $d0c6 $4d >M $d0c7 $ee >M
T{ op PC S A X Y P -> $d0c7 $78 $bc $ca $e2 $21 }T T{ $004d M $d0c5 M $d0c6 M $d0c7 M -> $04 $06 $4d $ee }T
( 07 )
$be9d >PC $e6 >S $44 >A $46 >X $33 >Y $20 >P $009d $eb >M $be9d $07 >M $be9e $9d >M $be9f $d4 >M
T{ op PC S A X Y P -> $be9f $e6 $44 $46 $33 $20 }T T{ $009d M $be9d M $be9e M $be9f M -> $ea $07 $9d $d4 }T
$4c85 >PC $81 >S $b2 >A $e7 >X $b6 >Y $22 >P $00fd $94 >M $4c85 $07 >M $4c86 $fd >M $4c87 $02 >M
T{ op PC S A X Y P -> $4c87 $81 $b2 $e7 $b6 $22 }T T{ $00fd M $4c85 M $4c86 M $4c87 M -> $94 $07 $fd $02 }T
$7616 >PC $5a >S $a4 >A $96 >X $3a >Y $a5 >P $0041 $d0 >M $7616 $07 >M $7617 $41 >M $7618 $10 >M
T{ op PC S A X Y P -> $7618 $5a $a4 $96 $3a $a5 }T T{ $0041 M $7616 M $7617 M $7618 M -> $d0 $07 $41 $10 }T
$6013 >PC $57 >S $7e >A $22 >X $ee >Y $e2 >P $007f $d9 >M $6013 $07 >M $6014 $7f >M $6015 $c0 >M
T{ op PC S A X Y P -> $6015 $57 $7e $22 $ee $e2 }T T{ $007f M $6013 M $6014 M $6015 M -> $d8 $07 $7f $c0 }T
$7d59 >PC $03 >S $38 >A $53 >X $ee >Y $65 >P $00cd $e3 >M $7d59 $07 >M $7d5a $cd >M $7d5b $ee >M
T{ op PC S A X Y P -> $7d5b $03 $38 $53 $ee $65 }T T{ $00cd M $7d59 M $7d5a M $7d5b M -> $e2 $07 $cd $ee }T
$2ff7 >PC $d8 >S $f9 >A $09 >X $85 >Y $65 >P $008a $2a >M $2ff7 $07 >M $2ff8 $8a >M $2ff9 $75 >M
T{ op PC S A X Y P -> $2ff9 $d8 $f9 $09 $85 $65 }T T{ $008a M $2ff7 M $2ff8 M $2ff9 M -> $2a $07 $8a $75 }T
$121e >PC $44 >S $99 >A $e0 >X $fc >Y $26 >P $00da $da >M $121e $07 >M $121f $da >M $1220 $a1 >M
T{ op PC S A X Y P -> $1220 $44 $99 $e0 $fc $26 }T T{ $00da M $121e M $121f M $1220 M -> $da $07 $da $a1 }T
$5d54 >PC $47 >S $bf >A $86 >X $90 >Y $60 >P $00a3 $c0 >M $5d54 $07 >M $5d55 $a3 >M $5d56 $c0 >M
T{ op PC S A X Y P -> $5d56 $47 $bf $86 $90 $60 }T T{ $00a3 M $5d54 M $5d55 M $5d56 M -> $c0 $07 $a3 $c0 }T
$72f3 >PC $59 >S $01 >A $b7 >X $cb >Y $e6 >P $0035 $45 >M $72f3 $07 >M $72f4 $35 >M $72f5 $c8 >M
T{ op PC S A X Y P -> $72f5 $59 $01 $b7 $cb $e6 }T T{ $0035 M $72f3 M $72f4 M $72f5 M -> $44 $07 $35 $c8 }T
$506d >PC $b3 >S $89 >A $1c >X $5b >Y $a1 >P $005f $08 >M $506d $07 >M $506e $5f >M $506f $c3 >M
T{ op PC S A X Y P -> $506f $b3 $89 $1c $5b $a1 }T T{ $005f M $506d M $506e M $506f M -> $08 $07 $5f $c3 }T
$f928 >PC $fa >S $03 >A $49 >X $25 >Y $a7 >P $005e $27 >M $f928 $07 >M $f929 $5e >M $f92a $ec >M
T{ op PC S A X Y P -> $f92a $fa $03 $49 $25 $a7 }T T{ $005e M $f928 M $f929 M $f92a M -> $26 $07 $5e $ec }T
$bc58 >PC $35 >S $79 >A $54 >X $7b >Y $e5 >P $00ea $05 >M $bc58 $07 >M $bc59 $ea >M $bc5a $cd >M
T{ op PC S A X Y P -> $bc5a $35 $79 $54 $7b $e5 }T T{ $00ea M $bc58 M $bc59 M $bc5a M -> $04 $07 $ea $cd }T
$5452 >PC $1f >S $4d >A $29 >X $4d >Y $e0 >P $009e $2c >M $5452 $07 >M $5453 $9e >M $5454 $53 >M
T{ op PC S A X Y P -> $5454 $1f $4d $29 $4d $e0 }T T{ $009e M $5452 M $5453 M $5454 M -> $2c $07 $9e $53 }T
$a8c1 >PC $70 >S $51 >A $9e >X $b9 >Y $25 >P $00af $80 >M $a8c1 $07 >M $a8c2 $af >M $a8c3 $bf >M
T{ op PC S A X Y P -> $a8c3 $70 $51 $9e $b9 $25 }T T{ $00af M $a8c1 M $a8c2 M $a8c3 M -> $80 $07 $af $bf }T
$9acd >PC $3f >S $8b >A $5a >X $97 >Y $e3 >P $00e1 $84 >M $9acd $07 >M $9ace $e1 >M $9acf $af >M
T{ op PC S A X Y P -> $9acf $3f $8b $5a $97 $e3 }T T{ $00e1 M $9acd M $9ace M $9acf M -> $84 $07 $e1 $af }T
$b5e3 >PC $07 >S $e3 >A $76 >X $3e >Y $21 >P $0037 $c8 >M $b5e3 $07 >M $b5e4 $37 >M $b5e5 $41 >M
T{ op PC S A X Y P -> $b5e5 $07 $e3 $76 $3e $21 }T T{ $0037 M $b5e3 M $b5e4 M $b5e5 M -> $c8 $07 $37 $41 }T
( 08 )
$e250 >PC $38 >S $47 >A $da >X $c2 >Y $60 >P $e250 $08 >M $e251 $26 >M $e252 $42 >M
T{ op PC S A X Y P -> $e251 $37 $47 $da $c2 $60 }T T{ $0138 M $e250 M $e251 M $e252 M -> $70 $08 $26 $42 }T
$e5b5 >PC $c5 >S $b0 >A $64 >X $ef >Y $67 >P $e5b5 $08 >M $e5b6 $ce >M $e5b7 $a4 >M
T{ op PC S A X Y P -> $e5b6 $c4 $b0 $64 $ef $67 }T T{ $01c5 M $e5b5 M $e5b6 M $e5b7 M -> $77 $08 $ce $a4 }T
$ed24 >PC $09 >S $36 >A $42 >X $09 >Y $e7 >P $ed24 $08 >M $ed25 $81 >M $ed26 $a6 >M
T{ op PC S A X Y P -> $ed25 $08 $36 $42 $09 $e7 }T T{ $0109 M $ed24 M $ed25 M $ed26 M -> $f7 $08 $81 $a6 }T
$dfe4 >PC $96 >S $3a >A $5c >X $81 >Y $20 >P $dfe4 $08 >M $dfe5 $6d >M $dfe6 $3b >M
T{ op PC S A X Y P -> $dfe5 $95 $3a $5c $81 $20 }T T{ $0196 M $dfe4 M $dfe5 M $dfe6 M -> $30 $08 $6d $3b }T
$941f >PC $30 >S $99 >A $23 >X $b7 >Y $26 >P $941f $08 >M $9420 $8d >M $9421 $6d >M
T{ op PC S A X Y P -> $9420 $2f $99 $23 $b7 $26 }T T{ $0130 M $941f M $9420 M $9421 M -> $36 $08 $8d $6d }T
$9282 >PC $6a >S $1f >A $70 >X $b9 >Y $67 >P $9282 $08 >M $9283 $10 >M $9284 $fd >M
T{ op PC S A X Y P -> $9283 $69 $1f $70 $b9 $67 }T T{ $016a M $9282 M $9283 M $9284 M -> $77 $08 $10 $fd }T
$d684 >PC $f0 >S $5a >A $94 >X $91 >Y $23 >P $d684 $08 >M $d685 $76 >M $d686 $af >M
T{ op PC S A X Y P -> $d685 $ef $5a $94 $91 $23 }T T{ $01f0 M $d684 M $d685 M $d686 M -> $33 $08 $76 $af }T
$6a46 >PC $37 >S $bd >A $ff >X $99 >Y $a6 >P $6a46 $08 >M $6a47 $61 >M $6a48 $79 >M
T{ op PC S A X Y P -> $6a47 $36 $bd $ff $99 $a6 }T T{ $0137 M $6a46 M $6a47 M $6a48 M -> $b6 $08 $61 $79 }T
$433d >PC $84 >S $5e >A $4a >X $26 >Y $e0 >P $433d $08 >M $433e $ff >M $433f $84 >M
T{ op PC S A X Y P -> $433e $83 $5e $4a $26 $e0 }T T{ $0184 M $433d M $433e M $433f M -> $f0 $08 $ff $84 }T
$c8f8 >PC $7a >S $b7 >A $a7 >X $5a >Y $63 >P $c8f8 $08 >M $c8f9 $7f >M $c8fa $c2 >M
T{ op PC S A X Y P -> $c8f9 $79 $b7 $a7 $5a $63 }T T{ $017a M $c8f8 M $c8f9 M $c8fa M -> $73 $08 $7f $c2 }T
$92c9 >PC $b5 >S $e3 >A $18 >X $96 >Y $a1 >P $92c9 $08 >M $92ca $2c >M $92cb $d3 >M
T{ op PC S A X Y P -> $92ca $b4 $e3 $18 $96 $a1 }T T{ $01b5 M $92c9 M $92ca M $92cb M -> $b1 $08 $2c $d3 }T
$dc61 >PC $cb >S $b6 >A $87 >X $68 >Y $e2 >P $dc61 $08 >M $dc62 $c0 >M $dc63 $7e >M
T{ op PC S A X Y P -> $dc62 $ca $b6 $87 $68 $e2 }T T{ $01cb M $dc61 M $dc62 M $dc63 M -> $f2 $08 $c0 $7e }T
$cd5b >PC $ad >S $e9 >A $20 >X $8d >Y $67 >P $cd5b $08 >M $cd5c $06 >M $cd5d $e6 >M
T{ op PC S A X Y P -> $cd5c $ac $e9 $20 $8d $67 }T T{ $01ad M $cd5b M $cd5c M $cd5d M -> $77 $08 $06 $e6 }T
$d7b2 >PC $26 >S $5f >A $26 >X $bb >Y $e3 >P $d7b2 $08 >M $d7b3 $43 >M $d7b4 $e1 >M
T{ op PC S A X Y P -> $d7b3 $25 $5f $26 $bb $e3 }T T{ $0126 M $d7b2 M $d7b3 M $d7b4 M -> $f3 $08 $43 $e1 }T
$77d6 >PC $04 >S $d7 >A $5a >X $75 >Y $62 >P $77d6 $08 >M $77d7 $18 >M $77d8 $7b >M
T{ op PC S A X Y P -> $77d7 $03 $d7 $5a $75 $62 }T T{ $0104 M $77d6 M $77d7 M $77d8 M -> $72 $08 $18 $7b }T
$c238 >PC $8a >S $1a >A $40 >X $19 >Y $23 >P $c238 $08 >M $c239 $1b >M $c23a $36 >M
T{ op PC S A X Y P -> $c239 $89 $1a $40 $19 $23 }T T{ $018a M $c238 M $c239 M $c23a M -> $33 $08 $1b $36 }T
( 09 )
$cdf4 >PC $58 >S $64 >A $f0 >X $25 >Y $a1 >P $cdf4 $09 >M $cdf5 $97 >M $cdf6 $d4 >M
T{ op PC S A X Y P -> $cdf6 $58 $f7 $f0 $25 $a1 }T T{ $cdf4 M $cdf5 M $cdf6 M -> $09 $97 $d4 }T
$8323 >PC $fc >S $b4 >A $fe >X $00 >Y $66 >P $8323 $09 >M $8324 $62 >M $8325 $37 >M
T{ op PC S A X Y P -> $8325 $fc $f6 $fe $00 $e4 }T T{ $8323 M $8324 M $8325 M -> $09 $62 $37 }T
$ffbd >PC $2d >S $6d >A $b8 >X $b2 >Y $65 >P $ffbd $09 >M $ffbe $e9 >M $ffbf $87 >M
T{ op PC S A X Y P -> $ffbf $2d $ed $b8 $b2 $e5 }T T{ $ffbd M $ffbe M $ffbf M -> $09 $e9 $87 }T
$f678 >PC $9f >S $da >A $c3 >X $97 >Y $25 >P $f678 $09 >M $f679 $ac >M $f67a $da >M
T{ op PC S A X Y P -> $f67a $9f $fe $c3 $97 $a5 }T T{ $f678 M $f679 M $f67a M -> $09 $ac $da }T
$9342 >PC $98 >S $47 >A $8a >X $44 >Y $62 >P $9342 $09 >M $9343 $3c >M $9344 $ab >M
T{ op PC S A X Y P -> $9344 $98 $7f $8a $44 $60 }T T{ $9342 M $9343 M $9344 M -> $09 $3c $ab }T
$62e7 >PC $54 >S $ff >A $8f >X $8a >Y $a6 >P $62e7 $09 >M $62e8 $ab >M $62e9 $b2 >M
T{ op PC S A X Y P -> $62e9 $54 $ff $8f $8a $a4 }T T{ $62e7 M $62e8 M $62e9 M -> $09 $ab $b2 }T
$eb1f >PC $8c >S $f6 >A $7e >X $12 >Y $e6 >P $eb1f $09 >M $eb20 $f7 >M $eb21 $63 >M
T{ op PC S A X Y P -> $eb21 $8c $f7 $7e $12 $e4 }T T{ $eb1f M $eb20 M $eb21 M -> $09 $f7 $63 }T
$cfa7 >PC $1b >S $df >A $3f >X $3e >Y $a1 >P $cfa7 $09 >M $cfa8 $a2 >M $cfa9 $78 >M
T{ op PC S A X Y P -> $cfa9 $1b $ff $3f $3e $a1 }T T{ $cfa7 M $cfa8 M $cfa9 M -> $09 $a2 $78 }T
$a325 >PC $d1 >S $ad >A $08 >X $88 >Y $66 >P $a325 $09 >M $a326 $13 >M $a327 $1f >M
T{ op PC S A X Y P -> $a327 $d1 $bf $08 $88 $e4 }T T{ $a325 M $a326 M $a327 M -> $09 $13 $1f }T
$8efd >PC $b8 >S $e5 >A $18 >X $c4 >Y $67 >P $8efd $09 >M $8efe $ed >M $8eff $0f >M
T{ op PC S A X Y P -> $8eff $b8 $ed $18 $c4 $e5 }T T{ $8efd M $8efe M $8eff M -> $09 $ed $0f }T
$401f >PC $f8 >S $f9 >A $8c >X $76 >Y $a2 >P $401f $09 >M $4020 $b9 >M $4021 $6d >M
T{ op PC S A X Y P -> $4021 $f8 $f9 $8c $76 $a0 }T T{ $401f M $4020 M $4021 M -> $09 $b9 $6d }T
$153d >PC $ef >S $c9 >A $89 >X $19 >Y $60 >P $153d $09 >M $153e $aa >M $153f $97 >M
T{ op PC S A X Y P -> $153f $ef $eb $89 $19 $e0 }T T{ $153d M $153e M $153f M -> $09 $aa $97 }T
$db2a >PC $20 >S $3c >A $0e >X $03 >Y $66 >P $db2a $09 >M $db2b $12 >M $db2c $e6 >M
T{ op PC S A X Y P -> $db2c $20 $3e $0e $03 $64 }T T{ $db2a M $db2b M $db2c M -> $09 $12 $e6 }T
$86db >PC $7c >S $16 >A $27 >X $f1 >Y $61 >P $86db $09 >M $86dc $bc >M $86dd $8d >M
T{ op PC S A X Y P -> $86dd $7c $be $27 $f1 $e1 }T T{ $86db M $86dc M $86dd M -> $09 $bc $8d }T
$be93 >PC $83 >S $76 >A $df >X $03 >Y $63 >P $be93 $09 >M $be94 $b3 >M $be95 $9d >M
T{ op PC S A X Y P -> $be95 $83 $f7 $df $03 $e1 }T T{ $be93 M $be94 M $be95 M -> $09 $b3 $9d }T
$09db >PC $7e >S $a2 >A $91 >X $49 >Y $e5 >P $09db $09 >M $09dc $23 >M $09dd $be >M
T{ op PC S A X Y P -> $09dd $7e $a3 $91 $49 $e5 }T T{ $09db M $09dc M $09dd M -> $09 $23 $be }T
( 0a )
$4cd7 >PC $a8 >S $f2 >A $22 >X $51 >Y $e4 >P $4cd7 $0a >M $4cd8 $b8 >M $4cd9 $0b >M
T{ op PC S A X Y P -> $4cd8 $a8 $e4 $22 $51 $e5 }T T{ $4cd7 M $4cd8 M $4cd9 M -> $0a $b8 $0b }T
$6749 >PC $cf >S $ad >A $a6 >X $ee >Y $26 >P $6749 $0a >M $674a $ba >M $674b $ae >M
T{ op PC S A X Y P -> $674a $cf $5a $a6 $ee $25 }T T{ $6749 M $674a M $674b M -> $0a $ba $ae }T
$2826 >PC $a1 >S $af >A $3b >X $47 >Y $a4 >P $2826 $0a >M $2827 $e4 >M $2828 $24 >M
T{ op PC S A X Y P -> $2827 $a1 $5e $3b $47 $25 }T T{ $2826 M $2827 M $2828 M -> $0a $e4 $24 }T
$52ad >PC $0b >S $3d >A $a5 >X $a0 >Y $e0 >P $52ad $0a >M $52ae $e0 >M $52af $fe >M
T{ op PC S A X Y P -> $52ae $0b $7a $a5 $a0 $60 }T T{ $52ad M $52ae M $52af M -> $0a $e0 $fe }T
$f987 >PC $10 >S $fb >A $3c >X $1a >Y $a4 >P $f987 $0a >M $f988 $59 >M $f989 $6a >M
T{ op PC S A X Y P -> $f988 $10 $f6 $3c $1a $a5 }T T{ $f987 M $f988 M $f989 M -> $0a $59 $6a }T
$344f >PC $83 >S $6b >A $c7 >X $50 >Y $a0 >P $344f $0a >M $3450 $1d >M $3451 $d3 >M
T{ op PC S A X Y P -> $3450 $83 $d6 $c7 $50 $a0 }T T{ $344f M $3450 M $3451 M -> $0a $1d $d3 }T
$b89f >PC $3a >S $c6 >A $8f >X $98 >Y $e4 >P $b89f $0a >M $b8a0 $68 >M $b8a1 $71 >M
T{ op PC S A X Y P -> $b8a0 $3a $8c $8f $98 $e5 }T T{ $b89f M $b8a0 M $b8a1 M -> $0a $68 $71 }T
$ef5c >PC $f4 >S $26 >A $ad >X $39 >Y $e1 >P $ef5c $0a >M $ef5d $ae >M $ef5e $f8 >M
T{ op PC S A X Y P -> $ef5d $f4 $4c $ad $39 $60 }T T{ $ef5c M $ef5d M $ef5e M -> $0a $ae $f8 }T
$3c93 >PC $e8 >S $90 >A $3b >X $23 >Y $a1 >P $3c93 $0a >M $3c94 $14 >M $3c95 $db >M
T{ op PC S A X Y P -> $3c94 $e8 $20 $3b $23 $21 }T T{ $3c93 M $3c94 M $3c95 M -> $0a $14 $db }T
$0305 >PC $85 >S $76 >A $9b >X $be >Y $67 >P $0305 $0a >M $0306 $80 >M $0307 $d5 >M
T{ op PC S A X Y P -> $0306 $85 $ec $9b $be $e4 }T T{ $0305 M $0306 M $0307 M -> $0a $80 $d5 }T
$4a1c >PC $c9 >S $cb >A $d5 >X $71 >Y $e3 >P $4a1c $0a >M $4a1d $55 >M $4a1e $14 >M
T{ op PC S A X Y P -> $4a1d $c9 $96 $d5 $71 $e1 }T T{ $4a1c M $4a1d M $4a1e M -> $0a $55 $14 }T
$896a >PC $a9 >S $ad >A $f0 >X $7a >Y $e6 >P $896a $0a >M $896b $c8 >M $896c $27 >M
T{ op PC S A X Y P -> $896b $a9 $5a $f0 $7a $65 }T T{ $896a M $896b M $896c M -> $0a $c8 $27 }T
$82d8 >PC $fa >S $95 >A $27 >X $23 >Y $a5 >P $82d8 $0a >M $82d9 $ac >M $82da $1a >M
T{ op PC S A X Y P -> $82d9 $fa $2a $27 $23 $25 }T T{ $82d8 M $82d9 M $82da M -> $0a $ac $1a }T
$a0f5 >PC $04 >S $5b >A $f0 >X $64 >Y $e1 >P $a0f5 $0a >M $a0f6 $c8 >M $a0f7 $17 >M
T{ op PC S A X Y P -> $a0f6 $04 $b6 $f0 $64 $e0 }T T{ $a0f5 M $a0f6 M $a0f7 M -> $0a $c8 $17 }T
$dd29 >PC $ec >S $b5 >A $11 >X $55 >Y $a6 >P $dd29 $0a >M $dd2a $d0 >M $dd2b $18 >M
T{ op PC S A X Y P -> $dd2a $ec $6a $11 $55 $25 }T T{ $dd29 M $dd2a M $dd2b M -> $0a $d0 $18 }T
$9baf >PC $75 >S $a1 >A $17 >X $08 >Y $21 >P $9baf $0a >M $9bb0 $e3 >M $9bb1 $fb >M
T{ op PC S A X Y P -> $9bb0 $75 $42 $17 $08 $21 }T T{ $9baf M $9bb0 M $9bb1 M -> $0a $e3 $fb }T
( 0b )
$62d5 >PC $99 >S $97 >A $38 >X $6e >Y $24 >P $62d5 $0b >M $62d6 $f5 >M $62d7 $e4 >M
T{ op PC S A X Y P -> $62d6 $99 $97 $38 $6e $24 }T T{ $62d5 M $62d6 M $62d7 M -> $0b $f5 $e4 }T
$23ab >PC $27 >S $5c >A $2b >X $07 >Y $e7 >P $23ab $0b >M $23ac $86 >M $23ad $9d >M
T{ op PC S A X Y P -> $23ac $27 $5c $2b $07 $e7 }T T{ $23ab M $23ac M $23ad M -> $0b $86 $9d }T
$55a2 >PC $6b >S $25 >A $d9 >X $34 >Y $e2 >P $55a2 $0b >M $55a3 $72 >M $55a4 $d9 >M
T{ op PC S A X Y P -> $55a3 $6b $25 $d9 $34 $e2 }T T{ $55a2 M $55a3 M $55a4 M -> $0b $72 $d9 }T
$56c3 >PC $cd >S $24 >A $88 >X $d8 >Y $62 >P $56c3 $0b >M $56c4 $49 >M $56c5 $30 >M
T{ op PC S A X Y P -> $56c4 $cd $24 $88 $d8 $62 }T T{ $56c3 M $56c4 M $56c5 M -> $0b $49 $30 }T
$b7a6 >PC $f9 >S $96 >A $30 >X $c7 >Y $e2 >P $b7a6 $0b >M $b7a7 $b1 >M $b7a8 $c0 >M
T{ op PC S A X Y P -> $b7a7 $f9 $96 $30 $c7 $e2 }T T{ $b7a6 M $b7a7 M $b7a8 M -> $0b $b1 $c0 }T
$6abf >PC $4b >S $e0 >A $bf >X $21 >Y $25 >P $6abf $0b >M $6ac0 $ae >M $6ac1 $48 >M
T{ op PC S A X Y P -> $6ac0 $4b $e0 $bf $21 $25 }T T{ $6abf M $6ac0 M $6ac1 M -> $0b $ae $48 }T
$c80b >PC $c7 >S $9f >A $18 >X $e3 >Y $e4 >P $c80b $0b >M $c80c $b2 >M $c80d $49 >M
T{ op PC S A X Y P -> $c80c $c7 $9f $18 $e3 $e4 }T T{ $c80b M $c80c M $c80d M -> $0b $b2 $49 }T
$4d99 >PC $a0 >S $8c >A $8f >X $59 >Y $21 >P $4d99 $0b >M $4d9a $c7 >M $4d9b $61 >M
T{ op PC S A X Y P -> $4d9a $a0 $8c $8f $59 $21 }T T{ $4d99 M $4d9a M $4d9b M -> $0b $c7 $61 }T
$0606 >PC $f8 >S $6f >A $d8 >X $c7 >Y $66 >P $0606 $0b >M $0607 $d5 >M $0608 $38 >M
T{ op PC S A X Y P -> $0607 $f8 $6f $d8 $c7 $66 }T T{ $0606 M $0607 M $0608 M -> $0b $d5 $38 }T
$2c3e >PC $e7 >S $c6 >A $35 >X $52 >Y $e4 >P $2c3e $0b >M $2c3f $9a >M $2c40 $bc >M
T{ op PC S A X Y P -> $2c3f $e7 $c6 $35 $52 $e4 }T T{ $2c3e M $2c3f M $2c40 M -> $0b $9a $bc }T
$0dd7 >PC $01 >S $d9 >A $27 >X $58 >Y $a7 >P $0dd7 $0b >M $0dd8 $3f >M $0dd9 $0f >M
T{ op PC S A X Y P -> $0dd8 $01 $d9 $27 $58 $a7 }T T{ $0dd7 M $0dd8 M $0dd9 M -> $0b $3f $0f }T
$92cb >PC $7e >S $46 >A $2b >X $98 >Y $e7 >P $92cb $0b >M $92cc $14 >M $92cd $88 >M
T{ op PC S A X Y P -> $92cc $7e $46 $2b $98 $e7 }T T{ $92cb M $92cc M $92cd M -> $0b $14 $88 }T
$17c3 >PC $ed >S $55 >A $2a >X $ca >Y $21 >P $17c3 $0b >M $17c4 $d8 >M $17c5 $32 >M
T{ op PC S A X Y P -> $17c4 $ed $55 $2a $ca $21 }T T{ $17c3 M $17c4 M $17c5 M -> $0b $d8 $32 }T
$2196 >PC $01 >S $89 >A $f2 >X $ff >Y $65 >P $2196 $0b >M $2197 $09 >M $2198 $c6 >M
T{ op PC S A X Y P -> $2197 $01 $89 $f2 $ff $65 }T T{ $2196 M $2197 M $2198 M -> $0b $09 $c6 }T
$6850 >PC $3e >S $2f >A $5c >X $d3 >Y $e0 >P $6850 $0b >M $6851 $7e >M $6852 $e1 >M
T{ op PC S A X Y P -> $6851 $3e $2f $5c $d3 $e0 }T T{ $6850 M $6851 M $6852 M -> $0b $7e $e1 }T
$fe75 >PC $db >S $3e >A $ea >X $a3 >Y $e5 >P $fe75 $0b >M $fe76 $96 >M $fe77 $b6 >M
T{ op PC S A X Y P -> $fe76 $db $3e $ea $a3 $e5 }T T{ $fe75 M $fe76 M $fe77 M -> $0b $96 $b6 }T
( 0c )
$12ad >PC $ed >S $8a >A $cd >X $32 >Y $a3 >P $12ad $0c >M $12ae $02 >M $12af $38 >M $12b0 $57 >M $3802 $85 >M
T{ op PC S A X Y P -> $12b0 $ed $8a $cd $32 $a1 }T T{ $12ad M $12ae M $12af M $12b0 M $3802 M -> $0c $02 $38 $57 $8f }T
$9cd7 >PC $1d >S $01 >A $0c >X $08 >Y $20 >P $9cd7 $0c >M $9cd8 $98 >M $9cd9 $9d >M $9cda $0f >M $9d98 $55 >M
T{ op PC S A X Y P -> $9cda $1d $01 $0c $08 $20 }T T{ $9cd7 M $9cd8 M $9cd9 M $9cda M $9d98 M -> $0c $98 $9d $0f $55 }T
$b6dc >PC $3d >S $56 >A $bf >X $e7 >Y $23 >P $68af $d3 >M $b6dc $0c >M $b6dd $af >M $b6de $68 >M $b6df $6b >M
T{ op PC S A X Y P -> $b6df $3d $56 $bf $e7 $21 }T T{ $68af M $b6dc M $b6dd M $b6de M $b6df M -> $d7 $0c $af $68 $6b }T
$8ad1 >PC $9d >S $69 >A $25 >X $a0 >Y $63 >P $3770 $f2 >M $8ad1 $0c >M $8ad2 $70 >M $8ad3 $37 >M $8ad4 $7a >M
T{ op PC S A X Y P -> $8ad4 $9d $69 $25 $a0 $61 }T T{ $3770 M $8ad1 M $8ad2 M $8ad3 M $8ad4 M -> $fb $0c $70 $37 $7a }T
$f103 >PC $e0 >S $8c >A $a5 >X $a2 >Y $67 >P $6d42 $91 >M $f103 $0c >M $f104 $42 >M $f105 $6d >M $f106 $86 >M
T{ op PC S A X Y P -> $f106 $e0 $8c $a5 $a2 $65 }T T{ $6d42 M $f103 M $f104 M $f105 M $f106 M -> $9d $0c $42 $6d $86 }T
$988f >PC $13 >S $69 >A $b6 >X $8e >Y $e3 >P $988f $0c >M $9890 $c1 >M $9891 $e6 >M $9892 $47 >M $e6c1 $7a >M
T{ op PC S A X Y P -> $9892 $13 $69 $b6 $8e $e1 }T T{ $988f M $9890 M $9891 M $9892 M $e6c1 M -> $0c $c1 $e6 $47 $7b }T
$942c >PC $b5 >S $ee >A $9a >X $2a >Y $27 >P $85f9 $c2 >M $942c $0c >M $942d $f9 >M $942e $85 >M $942f $43 >M
T{ op PC S A X Y P -> $942f $b5 $ee $9a $2a $25 }T T{ $85f9 M $942c M $942d M $942e M $942f M -> $ee $0c $f9 $85 $43 }T
$d32e >PC $f7 >S $4e >A $11 >X $f8 >Y $24 >P $d32e $0c >M $d32f $90 >M $d330 $d3 >M $d331 $fd >M $d390 $5b >M
T{ op PC S A X Y P -> $d331 $f7 $4e $11 $f8 $24 }T T{ $d32e M $d32f M $d330 M $d331 M $d390 M -> $0c $90 $d3 $fd $5f }T
$999b >PC $f2 >S $b7 >A $ef >X $ac >Y $e0 >P $00cc $e2 >M $999b $0c >M $999c $cc >M $999d $00 >M $999e $80 >M
T{ op PC S A X Y P -> $999e $f2 $b7 $ef $ac $e0 }T T{ $00cc M $999b M $999c M $999d M $999e M -> $f7 $0c $cc $00 $80 }T
$8a6f >PC $9b >S $ed >A $d9 >X $f1 >Y $65 >P $498f $24 >M $8a6f $0c >M $8a70 $8f >M $8a71 $49 >M $8a72 $2a >M
T{ op PC S A X Y P -> $8a72 $9b $ed $d9 $f1 $65 }T T{ $498f M $8a6f M $8a70 M $8a71 M $8a72 M -> $ed $0c $8f $49 $2a }T
$5da1 >PC $e4 >S $05 >A $f5 >X $8d >Y $66 >P $5da1 $0c >M $5da2 $ae >M $5da3 $d6 >M $5da4 $74 >M $d6ae $83 >M
T{ op PC S A X Y P -> $5da4 $e4 $05 $f5 $8d $64 }T T{ $5da1 M $5da2 M $5da3 M $5da4 M $d6ae M -> $0c $ae $d6 $74 $87 }T
$5f59 >PC $cb >S $6e >A $66 >X $35 >Y $21 >P $5f59 $0c >M $5f5a $05 >M $5f5b $74 >M $5f5c $3d >M $7405 $c3 >M
T{ op PC S A X Y P -> $5f5c $cb $6e $66 $35 $21 }T T{ $5f59 M $5f5a M $5f5b M $5f5c M $7405 M -> $0c $05 $74 $3d $ef }T
$8003 >PC $e5 >S $83 >A $59 >X $71 >Y $66 >P $30eb $95 >M $8003 $0c >M $8004 $eb >M $8005 $30 >M $8006 $28 >M
T{ op PC S A X Y P -> $8006 $e5 $83 $59 $71 $64 }T T{ $30eb M $8003 M $8004 M $8005 M $8006 M -> $97 $0c $eb $30 $28 }T
$35a0 >PC $0b >S $61 >A $9d >X $e2 >Y $61 >P $35a0 $0c >M $35a1 $70 >M $35a2 $7c >M $35a3 $6d >M $7c70 $cb >M
T{ op PC S A X Y P -> $35a3 $0b $61 $9d $e2 $61 }T T{ $35a0 M $35a1 M $35a2 M $35a3 M $7c70 M -> $0c $70 $7c $6d $eb }T
$7462 >PC $b0 >S $4b >A $eb >X $d4 >Y $20 >P $29ee $09 >M $7462 $0c >M $7463 $ee >M $7464 $29 >M $7465 $03 >M
T{ op PC S A X Y P -> $7465 $b0 $4b $eb $d4 $20 }T T{ $29ee M $7462 M $7463 M $7464 M $7465 M -> $4b $0c $ee $29 $03 }T
$2437 >PC $ef >S $9c >A $17 >X $c9 >Y $a1 >P $2437 $0c >M $2438 $09 >M $2439 $98 >M $243a $27 >M $9809 $12 >M
T{ op PC S A X Y P -> $243a $ef $9c $17 $c9 $a1 }T T{ $2437 M $2438 M $2439 M $243a M $9809 M -> $0c $09 $98 $27 $9e }T
( 0d )
$ef5e >PC $4c >S $a8 >A $2f >X $68 >Y $61 >P $e31e $e7 >M $ef5e $0d >M $ef5f $1e >M $ef60 $e3 >M $ef61 $3b >M
T{ op PC S A X Y P -> $ef61 $4c $ef $2f $68 $e1 }T T{ $e31e M $ef5e M $ef5f M $ef60 M $ef61 M -> $e7 $0d $1e $e3 $3b }T
$efe1 >PC $8a >S $d1 >A $e9 >X $de >Y $e1 >P $cd22 $92 >M $efe1 $0d >M $efe2 $22 >M $efe3 $cd >M $efe4 $63 >M
T{ op PC S A X Y P -> $efe4 $8a $d3 $e9 $de $e1 }T T{ $cd22 M $efe1 M $efe2 M $efe3 M $efe4 M -> $92 $0d $22 $cd $63 }T
$b05e >PC $33 >S $c3 >A $fe >X $88 >Y $a3 >P $15c6 $48 >M $b05e $0d >M $b05f $c6 >M $b060 $15 >M $b061 $d8 >M
T{ op PC S A X Y P -> $b061 $33 $cb $fe $88 $a1 }T T{ $15c6 M $b05e M $b05f M $b060 M $b061 M -> $48 $0d $c6 $15 $d8 }T
$4ca2 >PC $4d >S $d9 >A $48 >X $21 >Y $64 >P $13a5 $4e >M $4ca2 $0d >M $4ca3 $a5 >M $4ca4 $13 >M $4ca5 $0c >M
T{ op PC S A X Y P -> $4ca5 $4d $df $48 $21 $e4 }T T{ $13a5 M $4ca2 M $4ca3 M $4ca4 M $4ca5 M -> $4e $0d $a5 $13 $0c }T
$ea5c >PC $da >S $e0 >A $ef >X $98 >Y $27 >P $ea5c $0d >M $ea5d $10 >M $ea5e $fb >M $ea5f $83 >M $fb10 $11 >M
T{ op PC S A X Y P -> $ea5f $da $f1 $ef $98 $a5 }T T{ $ea5c M $ea5d M $ea5e M $ea5f M $fb10 M -> $0d $10 $fb $83 $11 }T
$f090 >PC $56 >S $87 >A $9d >X $6c >Y $26 >P $9af9 $0c >M $f090 $0d >M $f091 $f9 >M $f092 $9a >M $f093 $02 >M
T{ op PC S A X Y P -> $f093 $56 $8f $9d $6c $a4 }T T{ $9af9 M $f090 M $f091 M $f092 M $f093 M -> $0c $0d $f9 $9a $02 }T
$a388 >PC $aa >S $31 >A $6b >X $d0 >Y $27 >P $a388 $0d >M $a389 $f6 >M $a38a $f5 >M $a38b $e2 >M $f5f6 $3d >M
T{ op PC S A X Y P -> $a38b $aa $3d $6b $d0 $25 }T T{ $a388 M $a389 M $a38a M $a38b M $f5f6 M -> $0d $f6 $f5 $e2 $3d }T
$a8a8 >PC $8c >S $dc >A $35 >X $84 >Y $20 >P $a8a8 $0d >M $a8a9 $79 >M $a8aa $df >M $a8ab $05 >M $df79 $f6 >M
T{ op PC S A X Y P -> $a8ab $8c $fe $35 $84 $a0 }T T{ $a8a8 M $a8a9 M $a8aa M $a8ab M $df79 M -> $0d $79 $df $05 $f6 }T
$a702 >PC $4e >S $a8 >A $0d >X $2d >Y $66 >P $a59c $ec >M $a702 $0d >M $a703 $9c >M $a704 $a5 >M $a705 $ee >M
T{ op PC S A X Y P -> $a705 $4e $ec $0d $2d $e4 }T T{ $a59c M $a702 M $a703 M $a704 M $a705 M -> $ec $0d $9c $a5 $ee }T
$1f3b >PC $b6 >S $dc >A $8f >X $12 >Y $e5 >P $1f3b $0d >M $1f3c $8e >M $1f3d $79 >M $1f3e $49 >M $798e $94 >M
T{ op PC S A X Y P -> $1f3e $b6 $dc $8f $12 $e5 }T T{ $1f3b M $1f3c M $1f3d M $1f3e M $798e M -> $0d $8e $79 $49 $94 }T
$1d57 >PC $1c >S $86 >A $47 >X $48 >Y $23 >P $1d57 $0d >M $1d58 $0b >M $1d59 $3d >M $1d5a $cb >M $3d0b $f3 >M
T{ op PC S A X Y P -> $1d5a $1c $f7 $47 $48 $a1 }T T{ $1d57 M $1d58 M $1d59 M $1d5a M $3d0b M -> $0d $0b $3d $cb $f3 }T
$f1f0 >PC $92 >S $0f >A $69 >X $55 >Y $65 >P $57ae $35 >M $f1f0 $0d >M $f1f1 $ae >M $f1f2 $57 >M $f1f3 $54 >M
T{ op PC S A X Y P -> $f1f3 $92 $3f $69 $55 $65 }T T{ $57ae M $f1f0 M $f1f1 M $f1f2 M $f1f3 M -> $35 $0d $ae $57 $54 }T
$d47b >PC $58 >S $5e >A $01 >X $ce >Y $24 >P $a927 $3b >M $d47b $0d >M $d47c $27 >M $d47d $a9 >M $d47e $bd >M
T{ op PC S A X Y P -> $d47e $58 $7f $01 $ce $24 }T T{ $a927 M $d47b M $d47c M $d47d M $d47e M -> $3b $0d $27 $a9 $bd }T
$6d3a >PC $a7 >S $bb >A $8a >X $d0 >Y $e6 >P $6d3a $0d >M $6d3b $8a >M $6d3c $ca >M $6d3d $43 >M $ca8a $f4 >M
T{ op PC S A X Y P -> $6d3d $a7 $ff $8a $d0 $e4 }T T{ $6d3a M $6d3b M $6d3c M $6d3d M $ca8a M -> $0d $8a $ca $43 $f4 }T
$aa6d >PC $42 >S $5a >A $c8 >X $1e >Y $66 >P $aa6d $0d >M $aa6e $bc >M $aa6f $fa >M $aa70 $e5 >M $fabc $d0 >M
T{ op PC S A X Y P -> $aa70 $42 $da $c8 $1e $e4 }T T{ $aa6d M $aa6e M $aa6f M $aa70 M $fabc M -> $0d $bc $fa $e5 $d0 }T
$ca3b >PC $aa >S $54 >A $cf >X $c5 >Y $27 >P $1ac1 $ee >M $ca3b $0d >M $ca3c $c1 >M $ca3d $1a >M $ca3e $f1 >M
T{ op PC S A X Y P -> $ca3e $aa $fe $cf $c5 $a5 }T T{ $1ac1 M $ca3b M $ca3c M $ca3d M $ca3e M -> $ee $0d $c1 $1a $f1 }T
( 0e )
$95a5 >PC $23 >S $6f >A $21 >X $32 >Y $66 >P $95a5 $0e >M $95a6 $18 >M $95a7 $ca >M $95a8 $e2 >M $ca18 $64 >M
T{ op PC S A X Y P -> $95a8 $23 $6f $21 $32 $e4 }T T{ $95a5 M $95a6 M $95a7 M $95a8 M $ca18 M -> $0e $18 $ca $e2 $c8 }T
$38c7 >PC $2e >S $b7 >A $26 >X $99 >Y $63 >P $38c7 $0e >M $38c8 $0a >M $38c9 $97 >M $38ca $c5 >M $970a $ee >M
T{ op PC S A X Y P -> $38ca $2e $b7 $26 $99 $e1 }T T{ $38c7 M $38c8 M $38c9 M $38ca M $970a M -> $0e $0a $97 $c5 $dc }T
$410f >PC $64 >S $d7 >A $26 >X $45 >Y $e5 >P $410f $0e >M $4110 $f9 >M $4111 $e3 >M $4112 $a4 >M $e3f9 $ee >M
T{ op PC S A X Y P -> $4112 $64 $d7 $26 $45 $e5 }T T{ $410f M $4110 M $4111 M $4112 M $e3f9 M -> $0e $f9 $e3 $a4 $dc }T
$f3ea >PC $a5 >S $7c >A $3f >X $08 >Y $63 >P $c0cc $a7 >M $f3ea $0e >M $f3eb $cc >M $f3ec $c0 >M $f3ed $ed >M
T{ op PC S A X Y P -> $f3ed $a5 $7c $3f $08 $61 }T T{ $c0cc M $f3ea M $f3eb M $f3ec M $f3ed M -> $4e $0e $cc $c0 $ed }T
$5b0c >PC $01 >S $b8 >A $ee >X $d0 >Y $e7 >P $4771 $a3 >M $5b0c $0e >M $5b0d $71 >M $5b0e $47 >M $5b0f $08 >M
T{ op PC S A X Y P -> $5b0f $01 $b8 $ee $d0 $65 }T T{ $4771 M $5b0c M $5b0d M $5b0e M $5b0f M -> $46 $0e $71 $47 $08 }T
$ea24 >PC $d9 >S $de >A $24 >X $3e >Y $e3 >P $c510 $7f >M $ea24 $0e >M $ea25 $10 >M $ea26 $c5 >M $ea27 $8e >M
T{ op PC S A X Y P -> $ea27 $d9 $de $24 $3e $e0 }T T{ $c510 M $ea24 M $ea25 M $ea26 M $ea27 M -> $fe $0e $10 $c5 $8e }T
$4313 >PC $3d >S $7c >A $ae >X $ac >Y $67 >P $16db $a2 >M $4313 $0e >M $4314 $db >M $4315 $16 >M $4316 $d0 >M
T{ op PC S A X Y P -> $4316 $3d $7c $ae $ac $65 }T T{ $16db M $4313 M $4314 M $4315 M $4316 M -> $44 $0e $db $16 $d0 }T
$f833 >PC $79 >S $07 >A $0d >X $f4 >Y $a4 >P $ad7e $3a >M $f833 $0e >M $f834 $7e >M $f835 $ad >M $f836 $c1 >M
T{ op PC S A X Y P -> $f836 $79 $07 $0d $f4 $24 }T T{ $ad7e M $f833 M $f834 M $f835 M $f836 M -> $74 $0e $7e $ad $c1 }T
$e7e0 >PC $09 >S $59 >A $71 >X $b3 >Y $e3 >P $5a06 $b0 >M $e7e0 $0e >M $e7e1 $06 >M $e7e2 $5a >M $e7e3 $d8 >M
T{ op PC S A X Y P -> $e7e3 $09 $59 $71 $b3 $61 }T T{ $5a06 M $e7e0 M $e7e1 M $e7e2 M $e7e3 M -> $60 $0e $06 $5a $d8 }T
$a5b1 >PC $a6 >S $5a >A $ca >X $bc >Y $a2 >P $7d39 $3d >M $a5b1 $0e >M $a5b2 $39 >M $a5b3 $7d >M $a5b4 $73 >M
T{ op PC S A X Y P -> $a5b4 $a6 $5a $ca $bc $20 }T T{ $7d39 M $a5b1 M $a5b2 M $a5b3 M $a5b4 M -> $7a $0e $39 $7d $73 }T
$0077 >PC $b5 >S $46 >A $ec >X $f9 >Y $62 >P $0077 $0e >M $0078 $b9 >M $0079 $b4 >M $007a $e2 >M $b4b9 $15 >M
T{ op PC S A X Y P -> $007a $b5 $46 $ec $f9 $60 }T T{ $0077 M $0078 M $0079 M $007a M $b4b9 M -> $0e $b9 $b4 $e2 $2a }T
$cd07 >PC $2b >S $32 >A $36 >X $76 >Y $a6 >P $0af3 $ba >M $cd07 $0e >M $cd08 $f3 >M $cd09 $0a >M $cd0a $57 >M
T{ op PC S A X Y P -> $cd0a $2b $32 $36 $76 $25 }T T{ $0af3 M $cd07 M $cd08 M $cd09 M $cd0a M -> $74 $0e $f3 $0a $57 }T
$97c8 >PC $47 >S $dd >A $bd >X $13 >Y $a1 >P $97c8 $0e >M $97c9 $e1 >M $97ca $a8 >M $97cb $3e >M $a8e1 $38 >M
T{ op PC S A X Y P -> $97cb $47 $dd $bd $13 $20 }T T{ $97c8 M $97c9 M $97ca M $97cb M $a8e1 M -> $0e $e1 $a8 $3e $70 }T
$32c7 >PC $a9 >S $12 >A $67 >X $f4 >Y $65 >P $32c7 $0e >M $32c8 $91 >M $32c9 $e1 >M $32ca $22 >M $e191 $34 >M
T{ op PC S A X Y P -> $32ca $a9 $12 $67 $f4 $64 }T T{ $32c7 M $32c8 M $32c9 M $32ca M $e191 M -> $0e $91 $e1 $22 $68 }T
$3dbf >PC $8d >S $3c >A $99 >X $ff >Y $61 >P $2bde $e3 >M $3dbf $0e >M $3dc0 $de >M $3dc1 $2b >M $3dc2 $de >M
T{ op PC S A X Y P -> $3dc2 $8d $3c $99 $ff $e1 }T T{ $2bde M $3dbf M $3dc0 M $3dc1 M $3dc2 M -> $c6 $0e $de $2b $de }T
$5e27 >PC $d2 >S $a8 >A $16 >X $12 >Y $a3 >P $5e27 $0e >M $5e28 $bc >M $5e29 $ab >M $5e2a $66 >M $abbc $3e >M
T{ op PC S A X Y P -> $5e2a $d2 $a8 $16 $12 $20 }T T{ $5e27 M $5e28 M $5e29 M $5e2a M $abbc M -> $0e $bc $ab $66 $7c }T
( 0f )
$df3f >PC $9e >S $44 >A $71 >X $31 >Y $e3 >P $0052 $a5 >M $df3f $0f >M $df40 $52 >M $df41 $9d >M $df42 $db >M $dfdf $67 >M
T{ op PC S A X Y P -> $df42 $9e $44 $71 $31 $e3 }T T{ $0052 M $df3f M $df40 M $df41 M $df42 M $dfdf M -> $a5 $0f $52 $9d $db $67 }T
$96c6 >PC $bc >S $cc >A $42 >X $a2 >Y $e7 >P $0044 $b9 >M $96c3 $fd >M $96c6 $0f >M $96c7 $44 >M $96c8 $fa >M $96c9 $9e >M
T{ op PC S A X Y P -> $96c9 $bc $cc $42 $a2 $e7 }T T{ $0044 M $96c3 M $96c6 M $96c7 M $96c8 M $96c9 M -> $b9 $fd $0f $44 $fa $9e }T
$59dc >PC $55 >S $05 >A $41 >X $52 >Y $e7 >P $00b3 $41 >M $5994 $46 >M $59dc $0f >M $59dd $b3 >M $59de $b5 >M $59df $7d >M
T{ op PC S A X Y P -> $59df $55 $05 $41 $52 $e7 }T T{ $00b3 M $5994 M $59dc M $59dd M $59de M $59df M -> $41 $46 $0f $b3 $b5 $7d }T
$277d >PC $d7 >S $21 >A $83 >X $57 >Y $63 >P $00c2 $1b >M $271b $b6 >M $277d $0f >M $277e $c2 >M $277f $9b >M $2780 $b7 >M
T{ op PC S A X Y P -> $2780 $d7 $21 $83 $57 $63 }T T{ $00c2 M $271b M $277d M $277e M $277f M $2780 M -> $1b $b6 $0f $c2 $9b $b7 }T
$a268 >PC $a0 >S $60 >A $92 >X $ff >Y $61 >P $00f8 $d4 >M $a268 $0f >M $a269 $f8 >M $a26a $70 >M $a2db $12 >M
T{ op PC S A X Y P -> $a2db $a0 $60 $92 $ff $61 }T T{ $00f8 M $a268 M $a269 M $a26a M $a2db M -> $d4 $0f $f8 $70 $12 }T
$d523 >PC $7e >S $9e >A $9d >X $c7 >Y $23 >P $008e $cf >M $d523 $0f >M $d524 $8e >M $d525 $fe >M $d526 $d6 >M
T{ op PC S A X Y P -> $d526 $7e $9e $9d $c7 $23 }T T{ $008e M $d523 M $d524 M $d525 M $d526 M -> $cf $0f $8e $fe $d6 }T
$4a40 >PC $e9 >S $6a >A $ce >X $20 >Y $e6 >P $0067 $f6 >M $49d6 $4a >M $4a40 $0f >M $4a41 $67 >M $4a42 $93 >M $4ad6 $a1 >M
T{ op PC S A X Y P -> $49d6 $e9 $6a $ce $20 $e6 }T T{ $0067 M $49d6 M $4a40 M $4a41 M $4a42 M $4ad6 M -> $f6 $4a $0f $67 $93 $a1 }T
$b192 >PC $03 >S $f5 >A $8b >X $3a >Y $e5 >P $0011 $f0 >M $b114 $a8 >M $b192 $0f >M $b193 $11 >M $b194 $7f >M $b214 $25 >M
T{ op PC S A X Y P -> $b214 $03 $f5 $8b $3a $e5 }T T{ $0011 M $b114 M $b192 M $b193 M $b194 M $b214 M -> $f0 $a8 $0f $11 $7f $25 }T
$81ba >PC $6e >S $c9 >A $85 >X $ac >Y $62 >P $00fd $a2 >M $812b $f0 >M $81ba $0f >M $81bb $fd >M $81bc $6e >M $822b $1c >M
T{ op PC S A X Y P -> $822b $6e $c9 $85 $ac $62 }T T{ $00fd M $812b M $81ba M $81bb M $81bc M $822b M -> $a2 $f0 $0f $fd $6e $1c }T
$b9ac >PC $3d >S $ea >A $76 >X $16 >Y $e4 >P $004d $4d >M $b95b $c3 >M $b9ac $0f >M $b9ad $4d >M $b9ae $ac >M $b9af $d6 >M
T{ op PC S A X Y P -> $b9af $3d $ea $76 $16 $e4 }T T{ $004d M $b95b M $b9ac M $b9ad M $b9ae M $b9af M -> $4d $c3 $0f $4d $ac $d6 }T
$65b3 >PC $6c >S $c0 >A $15 >X $48 >Y $e2 >P $0081 $cd >M $654b $c6 >M $65b3 $0f >M $65b4 $81 >M $65b5 $95 >M $65b6 $65 >M
T{ op PC S A X Y P -> $65b6 $6c $c0 $15 $48 $e2 }T T{ $0081 M $654b M $65b3 M $65b4 M $65b5 M $65b6 M -> $cd $c6 $0f $81 $95 $65 }T
$9cfe >PC $90 >S $fc >A $7a >X $b1 >Y $60 >P $0083 $29 >M $9cfe $0f >M $9cff $83 >M $9d00 $fd >M $9d01 $b1 >M $9dfe $ea >M
T{ op PC S A X Y P -> $9d01 $90 $fc $7a $b1 $60 }T T{ $0083 M $9cfe M $9cff M $9d00 M $9d01 M $9dfe M -> $29 $0f $83 $fd $b1 $ea }T
$efd3 >PC $41 >S $c6 >A $16 >X $38 >Y $62 >P $00a1 $bd >M $efb7 $91 >M $efd3 $0f >M $efd4 $a1 >M $efd5 $e1 >M $efd6 $41 >M
T{ op PC S A X Y P -> $efd6 $41 $c6 $16 $38 $62 }T T{ $00a1 M $efb7 M $efd3 M $efd4 M $efd5 M $efd6 M -> $bd $91 $0f $a1 $e1 $41 }T
$4a12 >PC $4e >S $89 >A $b1 >X $7a >Y $e1 >P $0069 $d8 >M $4a12 $0f >M $4a13 $69 >M $4a14 $31 >M $4a46 $6f >M
T{ op PC S A X Y P -> $4a46 $4e $89 $b1 $7a $e1 }T T{ $0069 M $4a12 M $4a13 M $4a14 M $4a46 M -> $d8 $0f $69 $31 $6f }T
$6635 >PC $cf >S $9b >A $21 >X $22 >Y $e1 >P $0059 $1d >M $6615 $af >M $6635 $0f >M $6636 $59 >M $6637 $dd >M $6638 $1f >M
T{ op PC S A X Y P -> $6638 $cf $9b $21 $22 $e1 }T T{ $0059 M $6615 M $6635 M $6636 M $6637 M $6638 M -> $1d $af $0f $59 $dd $1f }T
$7afe >PC $66 >S $a0 >A $09 >X $9b >Y $a3 >P $008b $2e >M $7afe $0f >M $7aff $8b >M $7b00 $12 >M $7b13 $99 >M
T{ op PC S A X Y P -> $7b13 $66 $a0 $09 $9b $a3 }T T{ $008b M $7afe M $7aff M $7b00 M $7b13 M -> $2e $0f $8b $12 $99 }T
( 10 )
$d650 >PC $a4 >S $94 >A $ed >X $e4 >Y $26 >P $d650 $10 >M $d651 $6a >M $d652 $ae >M $d6bc $40 >M
T{ op PC S A X Y P -> $d6bc $a4 $94 $ed $e4 $26 }T T{ $d650 M $d651 M $d652 M $d6bc M -> $10 $6a $ae $40 }T
$0ec3 >PC $e8 >S $94 >A $13 >X $3d >Y $e7 >P $0ec3 $10 >M $0ec4 $2b >M $0ec5 $81 >M
T{ op PC S A X Y P -> $0ec5 $e8 $94 $13 $3d $e7 }T T{ $0ec3 M $0ec4 M $0ec5 M -> $10 $2b $81 }T
$9947 >PC $d0 >S $18 >A $62 >X $97 >Y $e6 >P $9947 $10 >M $9948 $9f >M $9949 $35 >M
T{ op PC S A X Y P -> $9949 $d0 $18 $62 $97 $e6 }T T{ $9947 M $9948 M $9949 M -> $10 $9f $35 }T
$5bc1 >PC $8b >S $74 >A $ba >X $0c >Y $e6 >P $5bc1 $10 >M $5bc2 $7f >M $5bc3 $ec >M
T{ op PC S A X Y P -> $5bc3 $8b $74 $ba $0c $e6 }T T{ $5bc1 M $5bc2 M $5bc3 M -> $10 $7f $ec }T
$ad06 >PC $fb >S $88 >A $00 >X $b3 >Y $a1 >P $ad06 $10 >M $ad07 $d4 >M $ad08 $63 >M
T{ op PC S A X Y P -> $ad08 $fb $88 $00 $b3 $a1 }T T{ $ad06 M $ad07 M $ad08 M -> $10 $d4 $63 }T
$9708 >PC $d1 >S $c0 >A $3c >X $dd >Y $20 >P $9708 $10 >M $9709 $7d >M $970a $7d >M $9787 $57 >M
T{ op PC S A X Y P -> $9787 $d1 $c0 $3c $dd $20 }T T{ $9708 M $9709 M $970a M $9787 M -> $10 $7d $7d $57 }T
$91c1 >PC $b0 >S $e4 >A $be >X $2e >Y $64 >P $9146 $3e >M $91c1 $10 >M $91c2 $83 >M $91c3 $c5 >M
T{ op PC S A X Y P -> $9146 $b0 $e4 $be $2e $64 }T T{ $9146 M $91c1 M $91c2 M $91c3 M -> $3e $10 $83 $c5 }T
$1790 >PC $89 >S $da >A $12 >X $31 >Y $a6 >P $1790 $10 >M $1791 $25 >M $1792 $60 >M
T{ op PC S A X Y P -> $1792 $89 $da $12 $31 $a6 }T T{ $1790 M $1791 M $1792 M -> $10 $25 $60 }T
$b97b >PC $17 >S $86 >A $98 >X $0e >Y $21 >P $b97b $10 >M $b97c $23 >M $b97d $87 >M $b9a0 $a4 >M
T{ op PC S A X Y P -> $b9a0 $17 $86 $98 $0e $21 }T T{ $b97b M $b97c M $b97d M $b9a0 M -> $10 $23 $87 $a4 }T
$b621 >PC $dd >S $1b >A $39 >X $0b >Y $27 >P $b621 $10 >M $b622 $6e >M $b623 $29 >M $b691 $b1 >M
T{ op PC S A X Y P -> $b691 $dd $1b $39 $0b $27 }T T{ $b621 M $b622 M $b623 M $b691 M -> $10 $6e $29 $b1 }T
$d6dc >PC $cd >S $7e >A $b4 >X $bf >Y $a5 >P $d6dc $10 >M $d6dd $8e >M $d6de $a9 >M
T{ op PC S A X Y P -> $d6de $cd $7e $b4 $bf $a5 }T T{ $d6dc M $d6dd M $d6de M -> $10 $8e $a9 }T
$4ae1 >PC $a5 >S $78 >A $64 >X $8f >Y $61 >P $4a64 $9d >M $4ae1 $10 >M $4ae2 $81 >M $4ae3 $39 >M
T{ op PC S A X Y P -> $4a64 $a5 $78 $64 $8f $61 }T T{ $4a64 M $4ae1 M $4ae2 M $4ae3 M -> $9d $10 $81 $39 }T
$6d88 >PC $55 >S $58 >A $e5 >X $9a >Y $65 >P $6d88 $10 >M $6d89 $6d >M $6d8a $c8 >M $6df7 $28 >M
T{ op PC S A X Y P -> $6df7 $55 $58 $e5 $9a $65 }T T{ $6d88 M $6d89 M $6d8a M $6df7 M -> $10 $6d $c8 $28 }T
$33d8 >PC $f6 >S $b4 >A $f8 >X $b4 >Y $a0 >P $33d8 $10 >M $33d9 $d9 >M $33da $4f >M
T{ op PC S A X Y P -> $33da $f6 $b4 $f8 $b4 $a0 }T T{ $33d8 M $33d9 M $33da M -> $10 $d9 $4f }T
$3e9f >PC $0e >S $8a >A $dd >X $be >Y $62 >P $3e5b $39 >M $3e9f $10 >M $3ea0 $ba >M $3ea1 $ac >M
T{ op PC S A X Y P -> $3e5b $0e $8a $dd $be $62 }T T{ $3e5b M $3e9f M $3ea0 M $3ea1 M -> $39 $10 $ba $ac }T
$7177 >PC $a0 >S $7b >A $ee >X $42 >Y $24 >P $7177 $10 >M $7178 $20 >M $7179 $ae >M $7199 $6c >M
T{ op PC S A X Y P -> $7199 $a0 $7b $ee $42 $24 }T T{ $7177 M $7178 M $7179 M $7199 M -> $10 $20 $ae $6c }T
( 11 )
$13d9 >PC $24 >S $e5 >A $bf >X $ff >Y $60 >P $0043 $46 >M $0044 $ec >M $13d9 $11 >M $13da $43 >M $13db $a6 >M $ed45 $05 >M
T{ op PC S A X Y P -> $13db $24 $e5 $bf $ff $e0 }T T{ $0043 M $0044 M $13d9 M $13da M $13db M $ed45 M -> $46 $ec $11 $43 $a6 $05 }T
$7471 >PC $6d >S $0a >A $6c >X $77 >Y $a0 >P $003f $f7 >M $0040 $02 >M $036e $7d >M $7471 $11 >M $7472 $3f >M $7473 $55 >M
T{ op PC S A X Y P -> $7473 $6d $7f $6c $77 $20 }T T{ $003f M $0040 M $036e M $7471 M $7472 M $7473 M -> $f7 $02 $7d $11 $3f $55 }T
$e03e >PC $e7 >S $ea >A $72 >X $2c >Y $a5 >P $00d9 $24 >M $00da $b1 >M $b150 $f7 >M $e03e $11 >M $e03f $d9 >M $e040 $54 >M
T{ op PC S A X Y P -> $e040 $e7 $ff $72 $2c $a5 }T T{ $00d9 M $00da M $b150 M $e03e M $e03f M $e040 M -> $24 $b1 $f7 $11 $d9 $54 }T
$e0b9 >PC $4c >S $16 >A $e9 >X $59 >Y $20 >P $00d3 $4d >M $00d4 $ea >M $e0b9 $11 >M $e0ba $d3 >M $e0bb $a6 >M $eaa6 $e3 >M
T{ op PC S A X Y P -> $e0bb $4c $f7 $e9 $59 $a0 }T T{ $00d3 M $00d4 M $e0b9 M $e0ba M $e0bb M $eaa6 M -> $4d $ea $11 $d3 $a6 $e3 }T
$bb19 >PC $d0 >S $68 >A $1a >X $e6 >Y $22 >P $001d $72 >M $001e $58 >M $5958 $bb >M $bb19 $11 >M $bb1a $1d >M $bb1b $2f >M
T{ op PC S A X Y P -> $bb1b $d0 $fb $1a $e6 $a0 }T T{ $001d M $001e M $5958 M $bb19 M $bb1a M $bb1b M -> $72 $58 $bb $11 $1d $2f }T
$56d1 >PC $a3 >S $44 >A $b5 >X $a8 >Y $22 >P $00ca $0e >M $00cb $77 >M $56d1 $11 >M $56d2 $ca >M $56d3 $a6 >M $77b6 $88 >M
T{ op PC S A X Y P -> $56d3 $a3 $cc $b5 $a8 $a0 }T T{ $00ca M $00cb M $56d1 M $56d2 M $56d3 M $77b6 M -> $0e $77 $11 $ca $a6 $88 }T
$7751 >PC $d0 >S $c2 >A $9f >X $42 >Y $a3 >P $004b $83 >M $004c $0a >M $0ac5 $6a >M $7751 $11 >M $7752 $4b >M $7753 $3f >M
T{ op PC S A X Y P -> $7753 $d0 $ea $9f $42 $a1 }T T{ $004b M $004c M $0ac5 M $7751 M $7752 M $7753 M -> $83 $0a $6a $11 $4b $3f }T
$5276 >PC $94 >S $24 >A $09 >X $9d >Y $24 >P $00b7 $75 >M $00b8 $79 >M $5276 $11 >M $5277 $b7 >M $5278 $66 >M $7a12 $a0 >M
T{ op PC S A X Y P -> $5278 $94 $a4 $09 $9d $a4 }T T{ $00b7 M $00b8 M $5276 M $5277 M $5278 M $7a12 M -> $75 $79 $11 $b7 $66 $a0 }T
$fe34 >PC $62 >S $27 >A $75 >X $c4 >Y $a3 >P $0073 $c2 >M $0074 $2a >M $2b86 $8d >M $fe34 $11 >M $fe35 $73 >M $fe36 $e8 >M
T{ op PC S A X Y P -> $fe36 $62 $af $75 $c4 $a1 }T T{ $0073 M $0074 M $2b86 M $fe34 M $fe35 M $fe36 M -> $c2 $2a $8d $11 $73 $e8 }T
$671e >PC $ec >S $67 >A $88 >X $73 >Y $e3 >P $0067 $c7 >M $0068 $3b >M $3c3a $77 >M $671e $11 >M $671f $67 >M $6720 $28 >M
T{ op PC S A X Y P -> $6720 $ec $77 $88 $73 $61 }T T{ $0067 M $0068 M $3c3a M $671e M $671f M $6720 M -> $c7 $3b $77 $11 $67 $28 }T
$9ffb >PC $56 >S $dd >A $3c >X $47 >Y $21 >P $000e $9c >M $000f $2f >M $2fe3 $d3 >M $9ffb $11 >M $9ffc $0e >M $9ffd $d5 >M
T{ op PC S A X Y P -> $9ffd $56 $df $3c $47 $a1 }T T{ $000e M $000f M $2fe3 M $9ffb M $9ffc M $9ffd M -> $9c $2f $d3 $11 $0e $d5 }T
$d560 >PC $e5 >S $14 >A $d5 >X $fc >Y $e2 >P $0007 $94 >M $0008 $92 >M $9390 $6a >M $d560 $11 >M $d561 $07 >M $d562 $6c >M
T{ op PC S A X Y P -> $d562 $e5 $7e $d5 $fc $60 }T T{ $0007 M $0008 M $9390 M $d560 M $d561 M $d562 M -> $94 $92 $6a $11 $07 $6c }T
$022a >PC $66 >S $10 >A $e7 >X $ed >Y $24 >P $001a $4c >M $001b $1d >M $022a $11 >M $022b $1a >M $022c $c7 >M $1e39 $33 >M
T{ op PC S A X Y P -> $022c $66 $33 $e7 $ed $24 }T T{ $001a M $001b M $022a M $022b M $022c M $1e39 M -> $4c $1d $11 $1a $c7 $33 }T
$d8d8 >PC $4f >S $fe >A $93 >X $f9 >Y $23 >P $009d $38 >M $009e $9b >M $9c31 $79 >M $d8d8 $11 >M $d8d9 $9d >M $d8da $58 >M
T{ op PC S A X Y P -> $d8da $4f $ff $93 $f9 $a1 }T T{ $009d M $009e M $9c31 M $d8d8 M $d8d9 M $d8da M -> $38 $9b $79 $11 $9d $58 }T
$94c9 >PC $d5 >S $9b >A $ec >X $32 >Y $a2 >P $00d7 $4a >M $00d8 $c5 >M $94c9 $11 >M $94ca $d7 >M $94cb $6e >M $c57c $4c >M
T{ op PC S A X Y P -> $94cb $d5 $df $ec $32 $a0 }T T{ $00d7 M $00d8 M $94c9 M $94ca M $94cb M $c57c M -> $4a $c5 $11 $d7 $6e $4c }T
$d09f >PC $86 >S $2b >A $f8 >X $7a >Y $a4 >P $00fb $4b >M $00fc $18 >M $18c5 $22 >M $d09f $11 >M $d0a0 $fb >M $d0a1 $8b >M
T{ op PC S A X Y P -> $d0a1 $86 $2b $f8 $7a $24 }T T{ $00fb M $00fc M $18c5 M $d09f M $d0a0 M $d0a1 M -> $4b $18 $22 $11 $fb $8b }T
( 12 )
$069f >PC $1e >S $f9 >A $52 >X $8b >Y $a2 >P $00c9 $90 >M $00ca $f0 >M $069f $12 >M $06a0 $c9 >M $06a1 $ac >M $f090 $8f >M
T{ op PC S A X Y P -> $06a1 $1e $ff $52 $8b $a0 }T T{ $00c9 M $00ca M $069f M $06a0 M $06a1 M $f090 M -> $90 $f0 $12 $c9 $ac $8f }T
$319e >PC $17 >S $4a >A $3e >X $26 >Y $a5 >P $000b $f3 >M $000c $a0 >M $319e $12 >M $319f $0b >M $31a0 $b2 >M $a0f3 $ec >M
T{ op PC S A X Y P -> $31a0 $17 $ee $3e $26 $a5 }T T{ $000b M $000c M $319e M $319f M $31a0 M $a0f3 M -> $f3 $a0 $12 $0b $b2 $ec }T
$dcda >PC $7a >S $6c >A $31 >X $ed >Y $20 >P $0066 $66 >M $0067 $fb >M $dcda $12 >M $dcdb $66 >M $dcdc $02 >M $fb66 $52 >M
T{ op PC S A X Y P -> $dcdc $7a $7e $31 $ed $20 }T T{ $0066 M $0067 M $dcda M $dcdb M $dcdc M $fb66 M -> $66 $fb $12 $66 $02 $52 }T
$7add >PC $db >S $e3 >A $92 >X $ec >Y $a1 >P $0067 $01 >M $0068 $a1 >M $7add $12 >M $7ade $67 >M $7adf $65 >M $a101 $6e >M
T{ op PC S A X Y P -> $7adf $db $ef $92 $ec $a1 }T T{ $0067 M $0068 M $7add M $7ade M $7adf M $a101 M -> $01 $a1 $12 $67 $65 $6e }T
$4084 >PC $f1 >S $1b >A $5f >X $b7 >Y $61 >P $00cc $a3 >M $00cd $3a >M $3aa3 $d4 >M $4084 $12 >M $4085 $cc >M $4086 $14 >M
T{ op PC S A X Y P -> $4086 $f1 $df $5f $b7 $e1 }T T{ $00cc M $00cd M $3aa3 M $4084 M $4085 M $4086 M -> $a3 $3a $d4 $12 $cc $14 }T
$c657 >PC $22 >S $10 >A $d7 >X $a0 >Y $a2 >P $0002 $e1 >M $0003 $60 >M $60e1 $a1 >M $c657 $12 >M $c658 $02 >M $c659 $29 >M
T{ op PC S A X Y P -> $c659 $22 $b1 $d7 $a0 $a0 }T T{ $0002 M $0003 M $60e1 M $c657 M $c658 M $c659 M -> $e1 $60 $a1 $12 $02 $29 }T
$cd44 >PC $71 >S $d2 >A $19 >X $33 >Y $65 >P $0076 $17 >M $0077 $09 >M $0917 $23 >M $cd44 $12 >M $cd45 $76 >M $cd46 $78 >M
T{ op PC S A X Y P -> $cd46 $71 $f3 $19 $33 $e5 }T T{ $0076 M $0077 M $0917 M $cd44 M $cd45 M $cd46 M -> $17 $09 $23 $12 $76 $78 }T
$272a >PC $74 >S $57 >A $99 >X $28 >Y $a5 >P $00a0 $6f >M $00a1 $a9 >M $272a $12 >M $272b $a0 >M $272c $b4 >M $a96f $a4 >M
T{ op PC S A X Y P -> $272c $74 $f7 $99 $28 $a5 }T T{ $00a0 M $00a1 M $272a M $272b M $272c M $a96f M -> $6f $a9 $12 $a0 $b4 $a4 }T
$f33f >PC $f5 >S $e9 >A $c1 >X $c6 >Y $e7 >P $0014 $cc >M $0015 $8d >M $8dcc $e7 >M $f33f $12 >M $f340 $14 >M $f341 $10 >M
T{ op PC S A X Y P -> $f341 $f5 $ef $c1 $c6 $e5 }T T{ $0014 M $0015 M $8dcc M $f33f M $f340 M $f341 M -> $cc $8d $e7 $12 $14 $10 }T
$58a5 >PC $0f >S $e1 >A $b2 >X $9f >Y $a3 >P $006d $5c >M $006e $de >M $58a5 $12 >M $58a6 $6d >M $58a7 $d9 >M $de5c $21 >M
T{ op PC S A X Y P -> $58a7 $0f $e1 $b2 $9f $a1 }T T{ $006d M $006e M $58a5 M $58a6 M $58a7 M $de5c M -> $5c $de $12 $6d $d9 $21 }T
$753a >PC $97 >S $eb >A $0f >X $f2 >Y $27 >P $009b $17 >M $009c $b5 >M $753a $12 >M $753b $9b >M $753c $61 >M $b517 $b3 >M
T{ op PC S A X Y P -> $753c $97 $fb $0f $f2 $a5 }T T{ $009b M $009c M $753a M $753b M $753c M $b517 M -> $17 $b5 $12 $9b $61 $b3 }T
$fabd >PC $a3 >S $91 >A $e5 >X $0c >Y $e2 >P $00a4 $6f >M $00a5 $f4 >M $f46f $e7 >M $fabd $12 >M $fabe $a4 >M $fabf $d3 >M
T{ op PC S A X Y P -> $fabf $a3 $f7 $e5 $0c $e0 }T T{ $00a4 M $00a5 M $f46f M $fabd M $fabe M $fabf M -> $6f $f4 $e7 $12 $a4 $d3 }T
$1fea >PC $51 >S $96 >A $12 >X $95 >Y $22 >P $0012 $c0 >M $0013 $db >M $1fea $12 >M $1feb $12 >M $1fec $01 >M $dbc0 $46 >M
T{ op PC S A X Y P -> $1fec $51 $d6 $12 $95 $a0 }T T{ $0012 M $0013 M $1fea M $1feb M $1fec M $dbc0 M -> $c0 $db $12 $12 $01 $46 }T
$2748 >PC $7a >S $c6 >A $05 >X $3b >Y $a7 >P $0021 $a9 >M $0022 $09 >M $09a9 $46 >M $2748 $12 >M $2749 $21 >M $274a $ae >M
T{ op PC S A X Y P -> $274a $7a $c6 $05 $3b $a5 }T T{ $0021 M $0022 M $09a9 M $2748 M $2749 M $274a M -> $a9 $09 $46 $12 $21 $ae }T
$9e41 >PC $b2 >S $83 >A $f6 >X $84 >Y $62 >P $00f9 $30 >M $00fa $3f >M $3f30 $03 >M $9e41 $12 >M $9e42 $f9 >M $9e43 $b9 >M
T{ op PC S A X Y P -> $9e43 $b2 $83 $f6 $84 $e0 }T T{ $00f9 M $00fa M $3f30 M $9e41 M $9e42 M $9e43 M -> $30 $3f $03 $12 $f9 $b9 }T
$9cc4 >PC $64 >S $12 >A $e7 >X $6c >Y $a2 >P $00a4 $06 >M $00a5 $8a >M $8a06 $49 >M $9cc4 $12 >M $9cc5 $a4 >M $9cc6 $25 >M
T{ op PC S A X Y P -> $9cc6 $64 $5b $e7 $6c $20 }T T{ $00a4 M $00a5 M $8a06 M $9cc4 M $9cc5 M $9cc6 M -> $06 $8a $49 $12 $a4 $25 }T
( 13 )
$7a0c >PC $5a >S $d8 >A $06 >X $94 >Y $e4 >P $7a0c $13 >M $7a0d $d0 >M $7a0e $4d >M
T{ op PC S A X Y P -> $7a0d $5a $d8 $06 $94 $e4 }T T{ $7a0c M $7a0d M $7a0e M -> $13 $d0 $4d }T
$b2a1 >PC $49 >S $b8 >A $fe >X $7b >Y $e7 >P $b2a1 $13 >M $b2a2 $37 >M $b2a3 $35 >M
T{ op PC S A X Y P -> $b2a2 $49 $b8 $fe $7b $e7 }T T{ $b2a1 M $b2a2 M $b2a3 M -> $13 $37 $35 }T
$9937 >PC $0c >S $f4 >A $ff >X $39 >Y $21 >P $9937 $13 >M $9938 $37 >M $9939 $2a >M
T{ op PC S A X Y P -> $9938 $0c $f4 $ff $39 $21 }T T{ $9937 M $9938 M $9939 M -> $13 $37 $2a }T
$8715 >PC $77 >S $2a >A $e5 >X $ff >Y $61 >P $8715 $13 >M $8716 $56 >M $8717 $0d >M
T{ op PC S A X Y P -> $8716 $77 $2a $e5 $ff $61 }T T{ $8715 M $8716 M $8717 M -> $13 $56 $0d }T
$61ae >PC $e1 >S $d4 >A $6d >X $90 >Y $26 >P $61ae $13 >M $61af $35 >M $61b0 $f1 >M
T{ op PC S A X Y P -> $61af $e1 $d4 $6d $90 $26 }T T{ $61ae M $61af M $61b0 M -> $13 $35 $f1 }T
$515e >PC $82 >S $dc >A $be >X $84 >Y $a6 >P $515e $13 >M $515f $fb >M $5160 $cc >M
T{ op PC S A X Y P -> $515f $82 $dc $be $84 $a6 }T T{ $515e M $515f M $5160 M -> $13 $fb $cc }T
$1d5f >PC $3e >S $2a >A $1f >X $4c >Y $a6 >P $1d5f $13 >M $1d60 $64 >M $1d61 $aa >M
T{ op PC S A X Y P -> $1d60 $3e $2a $1f $4c $a6 }T T{ $1d5f M $1d60 M $1d61 M -> $13 $64 $aa }T
$1b42 >PC $6e >S $26 >A $73 >X $37 >Y $20 >P $1b42 $13 >M $1b43 $4b >M $1b44 $18 >M
T{ op PC S A X Y P -> $1b43 $6e $26 $73 $37 $20 }T T{ $1b42 M $1b43 M $1b44 M -> $13 $4b $18 }T
$bd03 >PC $ab >S $f1 >A $ef >X $f9 >Y $e0 >P $bd03 $13 >M $bd04 $92 >M $bd05 $a6 >M
T{ op PC S A X Y P -> $bd04 $ab $f1 $ef $f9 $e0 }T T{ $bd03 M $bd04 M $bd05 M -> $13 $92 $a6 }T
$6048 >PC $b4 >S $76 >A $05 >X $8b >Y $a0 >P $6048 $13 >M $6049 $5f >M $604a $f9 >M
T{ op PC S A X Y P -> $6049 $b4 $76 $05 $8b $a0 }T T{ $6048 M $6049 M $604a M -> $13 $5f $f9 }T
$8731 >PC $9f >S $0c >A $da >X $1b >Y $e7 >P $8731 $13 >M $8732 $73 >M $8733 $bc >M
T{ op PC S A X Y P -> $8732 $9f $0c $da $1b $e7 }T T{ $8731 M $8732 M $8733 M -> $13 $73 $bc }T
$015b >PC $62 >S $49 >A $b8 >X $27 >Y $a0 >P $015b $13 >M $015c $e6 >M $015d $b2 >M
T{ op PC S A X Y P -> $015c $62 $49 $b8 $27 $a0 }T T{ $015b M $015c M $015d M -> $13 $e6 $b2 }T
$082e >PC $e3 >S $ba >A $ca >X $13 >Y $22 >P $082e $13 >M $082f $89 >M $0830 $97 >M
T{ op PC S A X Y P -> $082f $e3 $ba $ca $13 $22 }T T{ $082e M $082f M $0830 M -> $13 $89 $97 }T
$190e >PC $f3 >S $8e >A $8b >X $02 >Y $20 >P $190e $13 >M $190f $0e >M $1910 $1f >M
T{ op PC S A X Y P -> $190f $f3 $8e $8b $02 $20 }T T{ $190e M $190f M $1910 M -> $13 $0e $1f }T
$029f >PC $68 >S $63 >A $ad >X $d6 >Y $a7 >P $029f $13 >M $02a0 $df >M $02a1 $19 >M
T{ op PC S A X Y P -> $02a0 $68 $63 $ad $d6 $a7 }T T{ $029f M $02a0 M $02a1 M -> $13 $df $19 }T
$dfbf >PC $27 >S $56 >A $d3 >X $13 >Y $61 >P $dfbf $13 >M $dfc0 $5f >M $dfc1 $b7 >M
T{ op PC S A X Y P -> $dfc0 $27 $56 $d3 $13 $61 }T T{ $dfbf M $dfc0 M $dfc1 M -> $13 $5f $b7 }T
( 14 )
$15ea >PC $40 >S $5f >A $3c >X $23 >Y $a4 >P $00b8 $40 >M $15ea $14 >M $15eb $b8 >M $15ec $ef >M
T{ op PC S A X Y P -> $15ec $40 $5f $3c $23 $a4 }T T{ $00b8 M $15ea M $15eb M $15ec M -> $00 $14 $b8 $ef }T
$d4d5 >PC $ea >S $0d >A $11 >X $21 >Y $a0 >P $00ef $03 >M $d4d5 $14 >M $d4d6 $ef >M $d4d7 $a9 >M
T{ op PC S A X Y P -> $d4d7 $ea $0d $11 $21 $a0 }T T{ $00ef M $d4d5 M $d4d6 M $d4d7 M -> $02 $14 $ef $a9 }T
$041d >PC $cc >S $b1 >A $e4 >X $ab >Y $a4 >P $007f $73 >M $041d $14 >M $041e $7f >M $041f $95 >M
T{ op PC S A X Y P -> $041f $cc $b1 $e4 $ab $a4 }T T{ $007f M $041d M $041e M $041f M -> $42 $14 $7f $95 }T
$9c7f >PC $59 >S $79 >A $6b >X $c2 >Y $e5 >P $0054 $af >M $9c7f $14 >M $9c80 $54 >M $9c81 $a4 >M
T{ op PC S A X Y P -> $9c81 $59 $79 $6b $c2 $e5 }T T{ $0054 M $9c7f M $9c80 M $9c81 M -> $86 $14 $54 $a4 }T
$5166 >PC $70 >S $cd >A $e5 >X $4a >Y $e6 >P $001a $c1 >M $5166 $14 >M $5167 $1a >M $5168 $5f >M
T{ op PC S A X Y P -> $5168 $70 $cd $e5 $4a $e4 }T T{ $001a M $5166 M $5167 M $5168 M -> $00 $14 $1a $5f }T
$306e >PC $00 >S $92 >A $cf >X $8e >Y $24 >P $0067 $e6 >M $306e $14 >M $306f $67 >M $3070 $5d >M
T{ op PC S A X Y P -> $3070 $00 $92 $cf $8e $24 }T T{ $0067 M $306e M $306f M $3070 M -> $64 $14 $67 $5d }T
$d2d4 >PC $1b >S $d8 >A $9b >X $42 >Y $e2 >P $00be $73 >M $d2d4 $14 >M $d2d5 $be >M $d2d6 $7b >M
T{ op PC S A X Y P -> $d2d6 $1b $d8 $9b $42 $e0 }T T{ $00be M $d2d4 M $d2d5 M $d2d6 M -> $23 $14 $be $7b }T
$6074 >PC $2b >S $57 >A $de >X $8e >Y $21 >P $0024 $e1 >M $6074 $14 >M $6075 $24 >M $6076 $d2 >M
T{ op PC S A X Y P -> $6076 $2b $57 $de $8e $21 }T T{ $0024 M $6074 M $6075 M $6076 M -> $a0 $14 $24 $d2 }T
$dffa >PC $b9 >S $37 >A $fb >X $c6 >Y $61 >P $00c2 $7d >M $dffa $14 >M $dffb $c2 >M $dffc $ff >M
T{ op PC S A X Y P -> $dffc $b9 $37 $fb $c6 $61 }T T{ $00c2 M $dffa M $dffb M $dffc M -> $48 $14 $c2 $ff }T
$0e50 >PC $c3 >S $11 >A $c1 >X $2a >Y $a2 >P $00aa $ff >M $0e50 $14 >M $0e51 $aa >M $0e52 $87 >M
T{ op PC S A X Y P -> $0e52 $c3 $11 $c1 $2a $a0 }T T{ $00aa M $0e50 M $0e51 M $0e52 M -> $ee $14 $aa $87 }T
$1754 >PC $98 >S $ac >A $df >X $77 >Y $67 >P $0042 $fe >M $1754 $14 >M $1755 $42 >M $1756 $a6 >M
T{ op PC S A X Y P -> $1756 $98 $ac $df $77 $65 }T T{ $0042 M $1754 M $1755 M $1756 M -> $52 $14 $42 $a6 }T
$96d3 >PC $79 >S $15 >A $93 >X $22 >Y $61 >P $00ce $8b >M $96d3 $14 >M $96d4 $ce >M $96d5 $6e >M
T{ op PC S A X Y P -> $96d5 $79 $15 $93 $22 $61 }T T{ $00ce M $96d3 M $96d4 M $96d5 M -> $8a $14 $ce $6e }T
$703a >PC $96 >S $ac >A $ae >X $72 >Y $27 >P $0098 $22 >M $703a $14 >M $703b $98 >M $703c $36 >M
T{ op PC S A X Y P -> $703c $96 $ac $ae $72 $25 }T T{ $0098 M $703a M $703b M $703c M -> $02 $14 $98 $36 }T
$3c83 >PC $86 >S $d8 >A $d1 >X $40 >Y $67 >P $00f6 $80 >M $3c83 $14 >M $3c84 $f6 >M $3c85 $ff >M
T{ op PC S A X Y P -> $3c85 $86 $d8 $d1 $40 $65 }T T{ $00f6 M $3c83 M $3c84 M $3c85 M -> $00 $14 $f6 $ff }T
$26e6 >PC $02 >S $c0 >A $e2 >X $b5 >Y $a2 >P $009b $0b >M $26e6 $14 >M $26e7 $9b >M $26e8 $66 >M
T{ op PC S A X Y P -> $26e8 $02 $c0 $e2 $b5 $a2 }T T{ $009b M $26e6 M $26e7 M $26e8 M -> $0b $14 $9b $66 }T
$c6b4 >PC $0a >S $72 >A $66 >X $ef >Y $a4 >P $0087 $82 >M $c6b4 $14 >M $c6b5 $87 >M $c6b6 $39 >M
T{ op PC S A X Y P -> $c6b6 $0a $72 $66 $ef $a4 }T T{ $0087 M $c6b4 M $c6b5 M $c6b6 M -> $80 $14 $87 $39 }T
( 15 )
$4a78 >PC $3f >S $b7 >A $23 >X $5f >Y $21 >P $0038 $93 >M $005b $ac >M $4a78 $15 >M $4a79 $38 >M $4a7a $e3 >M
T{ op PC S A X Y P -> $4a7a $3f $bf $23 $5f $a1 }T T{ $0038 M $005b M $4a78 M $4a79 M $4a7a M -> $93 $ac $15 $38 $e3 }T
$75ac >PC $ea >S $e6 >A $a5 >X $6a >Y $63 >P $0024 $48 >M $00c9 $86 >M $75ac $15 >M $75ad $24 >M $75ae $06 >M
T{ op PC S A X Y P -> $75ae $ea $e6 $a5 $6a $e1 }T T{ $0024 M $00c9 M $75ac M $75ad M $75ae M -> $48 $86 $15 $24 $06 }T
$0b48 >PC $d8 >S $29 >A $57 >X $3b >Y $a0 >P $0017 $9c >M $006e $88 >M $0b48 $15 >M $0b49 $17 >M $0b4a $9f >M
T{ op PC S A X Y P -> $0b4a $d8 $a9 $57 $3b $a0 }T T{ $0017 M $006e M $0b48 M $0b49 M $0b4a M -> $9c $88 $15 $17 $9f }T
$f2b7 >PC $a1 >S $3a >A $72 >X $d8 >Y $24 >P $0061 $18 >M $00ef $ce >M $f2b7 $15 >M $f2b8 $ef >M $f2b9 $7d >M
T{ op PC S A X Y P -> $f2b9 $a1 $3a $72 $d8 $24 }T T{ $0061 M $00ef M $f2b7 M $f2b8 M $f2b9 M -> $18 $ce $15 $ef $7d }T
$5a03 >PC $0f >S $79 >A $38 >X $fd >Y $65 >P $00b7 $a0 >M $00ef $8d >M $5a03 $15 >M $5a04 $b7 >M $5a05 $1d >M
T{ op PC S A X Y P -> $5a05 $0f $fd $38 $fd $e5 }T T{ $00b7 M $00ef M $5a03 M $5a04 M $5a05 M -> $a0 $8d $15 $b7 $1d }T
$d5a3 >PC $fd >S $5b >A $c0 >X $d1 >Y $65 >P $00ab $74 >M $00eb $66 >M $d5a3 $15 >M $d5a4 $eb >M $d5a5 $b0 >M
T{ op PC S A X Y P -> $d5a5 $fd $7f $c0 $d1 $65 }T T{ $00ab M $00eb M $d5a3 M $d5a4 M $d5a5 M -> $74 $66 $15 $eb $b0 }T
$64d0 >PC $b4 >S $71 >A $f7 >X $37 >Y $62 >P $00c6 $eb >M $00cf $7b >M $64d0 $15 >M $64d1 $cf >M $64d2 $04 >M
T{ op PC S A X Y P -> $64d2 $b4 $fb $f7 $37 $e0 }T T{ $00c6 M $00cf M $64d0 M $64d1 M $64d2 M -> $eb $7b $15 $cf $04 }T
$9415 >PC $7a >S $94 >A $f3 >X $14 >Y $e5 >P $0017 $9c >M $0024 $7b >M $9415 $15 >M $9416 $24 >M $9417 $38 >M
T{ op PC S A X Y P -> $9417 $7a $9c $f3 $14 $e5 }T T{ $0017 M $0024 M $9415 M $9416 M $9417 M -> $9c $7b $15 $24 $38 }T
$732f >PC $38 >S $c4 >A $06 >X $2a >Y $a6 >P $00a8 $3c >M $00ae $4b >M $732f $15 >M $7330 $a8 >M $7331 $ac >M
T{ op PC S A X Y P -> $7331 $38 $cf $06 $2a $a4 }T T{ $00a8 M $00ae M $732f M $7330 M $7331 M -> $3c $4b $15 $a8 $ac }T
$710c >PC $32 >S $82 >A $1d >X $6c >Y $62 >P $0027 $b9 >M $0044 $38 >M $710c $15 >M $710d $27 >M $710e $29 >M
T{ op PC S A X Y P -> $710e $32 $ba $1d $6c $e0 }T T{ $0027 M $0044 M $710c M $710d M $710e M -> $b9 $38 $15 $27 $29 }T
$c2c5 >PC $c8 >S $90 >A $66 >X $0c >Y $e4 >P $0040 $0d >M $00a6 $31 >M $c2c5 $15 >M $c2c6 $40 >M $c2c7 $68 >M
T{ op PC S A X Y P -> $c2c7 $c8 $b1 $66 $0c $e4 }T T{ $0040 M $00a6 M $c2c5 M $c2c6 M $c2c7 M -> $0d $31 $15 $40 $68 }T
$b373 >PC $89 >S $35 >A $ab >X $29 >Y $63 >P $0032 $63 >M $00dd $63 >M $b373 $15 >M $b374 $32 >M $b375 $65 >M
T{ op PC S A X Y P -> $b375 $89 $77 $ab $29 $61 }T T{ $0032 M $00dd M $b373 M $b374 M $b375 M -> $63 $63 $15 $32 $65 }T
$9f91 >PC $49 >S $10 >A $10 >X $be >Y $e2 >P $00d1 $ec >M $00e1 $0c >M $9f91 $15 >M $9f92 $d1 >M $9f93 $c6 >M
T{ op PC S A X Y P -> $9f93 $49 $1c $10 $be $60 }T T{ $00d1 M $00e1 M $9f91 M $9f92 M $9f93 M -> $ec $0c $15 $d1 $c6 }T
$f0ca >PC $5f >S $28 >A $20 >X $36 >Y $e3 >P $009b $01 >M $00bb $7b >M $f0ca $15 >M $f0cb $9b >M $f0cc $b1 >M
T{ op PC S A X Y P -> $f0cc $5f $7b $20 $36 $61 }T T{ $009b M $00bb M $f0ca M $f0cb M $f0cc M -> $01 $7b $15 $9b $b1 }T
$52d0 >PC $fa >S $f4 >A $53 >X $4b >Y $e7 >P $0009 $b7 >M $005c $c8 >M $52d0 $15 >M $52d1 $09 >M $52d2 $54 >M
T{ op PC S A X Y P -> $52d2 $fa $fc $53 $4b $e5 }T T{ $0009 M $005c M $52d0 M $52d1 M $52d2 M -> $b7 $c8 $15 $09 $54 }T
$ca78 >PC $66 >S $4e >A $2d >X $ec >Y $e7 >P $0003 $ca >M $0030 $ca >M $ca78 $15 >M $ca79 $03 >M $ca7a $51 >M
T{ op PC S A X Y P -> $ca7a $66 $ce $2d $ec $e5 }T T{ $0003 M $0030 M $ca78 M $ca79 M $ca7a M -> $ca $ca $15 $03 $51 }T
( 16 )
$6711 >PC $7d >S $16 >A $2b >X $cb >Y $63 >P $0001 $82 >M $00d6 $49 >M $6711 $16 >M $6712 $d6 >M $6713 $39 >M
T{ op PC S A X Y P -> $6713 $7d $16 $2b $cb $61 }T T{ $0001 M $00d6 M $6711 M $6712 M $6713 M -> $04 $49 $16 $d6 $39 }T
$2493 >PC $bf >S $ee >A $00 >X $6a >Y $23 >P $0091 $f8 >M $2493 $16 >M $2494 $91 >M $2495 $63 >M
T{ op PC S A X Y P -> $2495 $bf $ee $00 $6a $a1 }T T{ $0091 M $2493 M $2494 M $2495 M -> $f0 $16 $91 $63 }T
$7302 >PC $f2 >S $a8 >A $26 >X $6a >Y $62 >P $0050 $25 >M $0076 $b3 >M $7302 $16 >M $7303 $50 >M $7304 $7d >M
T{ op PC S A X Y P -> $7304 $f2 $a8 $26 $6a $61 }T T{ $0050 M $0076 M $7302 M $7303 M $7304 M -> $25 $66 $16 $50 $7d }T
$f82a >PC $17 >S $6f >A $73 >X $7f >Y $a7 >P $0026 $44 >M $0099 $e1 >M $f82a $16 >M $f82b $26 >M $f82c $5a >M
T{ op PC S A X Y P -> $f82c $17 $6f $73 $7f $a5 }T T{ $0026 M $0099 M $f82a M $f82b M $f82c M -> $44 $c2 $16 $26 $5a }T
$6fcc >PC $39 >S $54 >A $6d >X $22 >Y $24 >P $0029 $81 >M $0096 $2b >M $6fcc $16 >M $6fcd $29 >M $6fce $58 >M
T{ op PC S A X Y P -> $6fce $39 $54 $6d $22 $24 }T T{ $0029 M $0096 M $6fcc M $6fcd M $6fce M -> $81 $56 $16 $29 $58 }T
$9485 >PC $f3 >S $35 >A $3a >X $65 >Y $20 >P $0001 $48 >M $003b $ab >M $9485 $16 >M $9486 $01 >M $9487 $f5 >M
T{ op PC S A X Y P -> $9487 $f3 $35 $3a $65 $21 }T T{ $0001 M $003b M $9485 M $9486 M $9487 M -> $48 $56 $16 $01 $f5 }T
$9801 >PC $42 >S $0a >A $df >X $5f >Y $27 >P $008a $a2 >M $00ab $b7 >M $9801 $16 >M $9802 $ab >M $9803 $7b >M
T{ op PC S A X Y P -> $9803 $42 $0a $df $5f $25 }T T{ $008a M $00ab M $9801 M $9802 M $9803 M -> $44 $b7 $16 $ab $7b }T
$f08e >PC $bd >S $d0 >A $74 >X $bf >Y $e5 >P $000e $62 >M $0082 $f9 >M $f08e $16 >M $f08f $0e >M $f090 $b0 >M
T{ op PC S A X Y P -> $f090 $bd $d0 $74 $bf $e5 }T T{ $000e M $0082 M $f08e M $f08f M $f090 M -> $62 $f2 $16 $0e $b0 }T
$c864 >PC $bb >S $6c >A $0d >X $63 >Y $27 >P $0062 $7b >M $006f $02 >M $c864 $16 >M $c865 $62 >M $c866 $06 >M
T{ op PC S A X Y P -> $c866 $bb $6c $0d $63 $24 }T T{ $0062 M $006f M $c864 M $c865 M $c866 M -> $7b $04 $16 $62 $06 }T
$a975 >PC $32 >S $81 >A $69 >X $a6 >Y $27 >P $0042 $ea >M $00d9 $93 >M $a975 $16 >M $a976 $d9 >M $a977 $65 >M
T{ op PC S A X Y P -> $a977 $32 $81 $69 $a6 $a5 }T T{ $0042 M $00d9 M $a975 M $a976 M $a977 M -> $d4 $93 $16 $d9 $65 }T
$d667 >PC $70 >S $91 >A $6c >X $57 >Y $e6 >P $0069 $e6 >M $00d5 $33 >M $d667 $16 >M $d668 $69 >M $d669 $f4 >M
T{ op PC S A X Y P -> $d669 $70 $91 $6c $57 $64 }T T{ $0069 M $00d5 M $d667 M $d668 M $d669 M -> $e6 $66 $16 $69 $f4 }T
$b5d6 >PC $c4 >S $09 >A $b6 >X $9c >Y $27 >P $0010 $95 >M $00c6 $e8 >M $b5d6 $16 >M $b5d7 $10 >M $b5d8 $b0 >M
T{ op PC S A X Y P -> $b5d8 $c4 $09 $b6 $9c $a5 }T T{ $0010 M $00c6 M $b5d6 M $b5d7 M $b5d8 M -> $95 $d0 $16 $10 $b0 }T
$0922 >PC $dd >S $b9 >A $30 >X $de >Y $a2 >P $006c $16 >M $009c $26 >M $0922 $16 >M $0923 $6c >M $0924 $c8 >M
T{ op PC S A X Y P -> $0924 $dd $b9 $30 $de $20 }T T{ $006c M $009c M $0922 M $0923 M $0924 M -> $16 $4c $16 $6c $c8 }T
$27c1 >PC $56 >S $86 >A $d0 >X $01 >Y $a7 >P $0075 $19 >M $00a5 $c1 >M $27c1 $16 >M $27c2 $a5 >M $27c3 $43 >M
T{ op PC S A X Y P -> $27c3 $56 $86 $d0 $01 $24 }T T{ $0075 M $00a5 M $27c1 M $27c2 M $27c3 M -> $32 $c1 $16 $a5 $43 }T
$aa5a >PC $b6 >S $5c >A $a9 >X $99 >Y $a0 >P $0025 $c7 >M $007c $eb >M $aa5a $16 >M $aa5b $7c >M $aa5c $0c >M
T{ op PC S A X Y P -> $aa5c $b6 $5c $a9 $99 $a1 }T T{ $0025 M $007c M $aa5a M $aa5b M $aa5c M -> $8e $eb $16 $7c $0c }T
$ee76 >PC $28 >S $fa >A $89 >X $f0 >Y $65 >P $003f $9b >M $00c8 $7f >M $ee76 $16 >M $ee77 $3f >M $ee78 $6d >M
T{ op PC S A X Y P -> $ee78 $28 $fa $89 $f0 $e4 }T T{ $003f M $00c8 M $ee76 M $ee77 M $ee78 M -> $9b $fe $16 $3f $6d }T
( 17 )
$b1a9 >PC $d6 >S $ac >A $3e >X $49 >Y $64 >P $0010 $73 >M $b1a9 $17 >M $b1aa $10 >M $b1ab $0c >M
T{ op PC S A X Y P -> $b1ab $d6 $ac $3e $49 $64 }T T{ $0010 M $b1a9 M $b1aa M $b1ab M -> $71 $17 $10 $0c }T
$73f2 >PC $1b >S $10 >A $52 >X $f8 >Y $e7 >P $00f0 $1d >M $73f2 $17 >M $73f3 $f0 >M $73f4 $db >M
T{ op PC S A X Y P -> $73f4 $1b $10 $52 $f8 $e7 }T T{ $00f0 M $73f2 M $73f3 M $73f4 M -> $1d $17 $f0 $db }T
$951d >PC $13 >S $fe >A $b5 >X $94 >Y $e4 >P $0064 $b0 >M $951d $17 >M $951e $64 >M $951f $a0 >M
T{ op PC S A X Y P -> $951f $13 $fe $b5 $94 $e4 }T T{ $0064 M $951d M $951e M $951f M -> $b0 $17 $64 $a0 }T
$bec8 >PC $4e >S $2b >A $77 >X $85 >Y $26 >P $00e9 $ce >M $bec8 $17 >M $bec9 $e9 >M $beca $4d >M
T{ op PC S A X Y P -> $beca $4e $2b $77 $85 $26 }T T{ $00e9 M $bec8 M $bec9 M $beca M -> $cc $17 $e9 $4d }T
$e32c >PC $fb >S $87 >A $ee >X $3c >Y $65 >P $00e4 $58 >M $e32c $17 >M $e32d $e4 >M $e32e $cd >M
T{ op PC S A X Y P -> $e32e $fb $87 $ee $3c $65 }T T{ $00e4 M $e32c M $e32d M $e32e M -> $58 $17 $e4 $cd }T
$056c >PC $cf >S $58 >A $0c >X $89 >Y $e4 >P $002c $57 >M $056c $17 >M $056d $2c >M $056e $73 >M
T{ op PC S A X Y P -> $056e $cf $58 $0c $89 $e4 }T T{ $002c M $056c M $056d M $056e M -> $55 $17 $2c $73 }T
$27df >PC $a9 >S $7a >A $ef >X $92 >Y $67 >P $00b4 $92 >M $27df $17 >M $27e0 $b4 >M $27e1 $93 >M
T{ op PC S A X Y P -> $27e1 $a9 $7a $ef $92 $67 }T T{ $00b4 M $27df M $27e0 M $27e1 M -> $90 $17 $b4 $93 }T
$0d7c >PC $77 >S $b4 >A $84 >X $6c >Y $25 >P $000a $8b >M $0d7c $17 >M $0d7d $0a >M $0d7e $99 >M
T{ op PC S A X Y P -> $0d7e $77 $b4 $84 $6c $25 }T T{ $000a M $0d7c M $0d7d M $0d7e M -> $89 $17 $0a $99 }T
$4aba >PC $ff >S $9b >A $28 >X $92 >Y $e5 >P $0042 $43 >M $4aba $17 >M $4abb $42 >M $4abc $89 >M
T{ op PC S A X Y P -> $4abc $ff $9b $28 $92 $e5 }T T{ $0042 M $4aba M $4abb M $4abc M -> $41 $17 $42 $89 }T
$6580 >PC $2d >S $d2 >A $3c >X $b0 >Y $24 >P $000f $73 >M $6580 $17 >M $6581 $0f >M $6582 $ae >M
T{ op PC S A X Y P -> $6582 $2d $d2 $3c $b0 $24 }T T{ $000f M $6580 M $6581 M $6582 M -> $71 $17 $0f $ae }T
$1368 >PC $d5 >S $7d >A $63 >X $25 >Y $25 >P $0032 $3a >M $1368 $17 >M $1369 $32 >M $136a $31 >M
T{ op PC S A X Y P -> $136a $d5 $7d $63 $25 $25 }T T{ $0032 M $1368 M $1369 M $136a M -> $38 $17 $32 $31 }T
$5cf6 >PC $ce >S $41 >A $d2 >X $27 >Y $a5 >P $0084 $42 >M $5cf6 $17 >M $5cf7 $84 >M $5cf8 $1f >M
T{ op PC S A X Y P -> $5cf8 $ce $41 $d2 $27 $a5 }T T{ $0084 M $5cf6 M $5cf7 M $5cf8 M -> $40 $17 $84 $1f }T
$1999 >PC $8b >S $7d >A $14 >X $41 >Y $67 >P $00e5 $65 >M $1999 $17 >M $199a $e5 >M $199b $ba >M
T{ op PC S A X Y P -> $199b $8b $7d $14 $41 $67 }T T{ $00e5 M $1999 M $199a M $199b M -> $65 $17 $e5 $ba }T
$cf80 >PC $74 >S $02 >A $6e >X $71 >Y $26 >P $0034 $d2 >M $cf80 $17 >M $cf81 $34 >M $cf82 $0e >M
T{ op PC S A X Y P -> $cf82 $74 $02 $6e $71 $26 }T T{ $0034 M $cf80 M $cf81 M $cf82 M -> $d0 $17 $34 $0e }T
$f9e1 >PC $03 >S $a7 >A $f0 >X $77 >Y $25 >P $0041 $6d >M $f9e1 $17 >M $f9e2 $41 >M $f9e3 $be >M
T{ op PC S A X Y P -> $f9e3 $03 $a7 $f0 $77 $25 }T T{ $0041 M $f9e1 M $f9e2 M $f9e3 M -> $6d $17 $41 $be }T
$ce80 >PC $27 >S $a5 >A $14 >X $7c >Y $23 >P $008d $87 >M $ce80 $17 >M $ce81 $8d >M $ce82 $c8 >M
T{ op PC S A X Y P -> $ce82 $27 $a5 $14 $7c $23 }T T{ $008d M $ce80 M $ce81 M $ce82 M -> $85 $17 $8d $c8 }T
( 18 )
$21d7 >PC $fd >S $46 >A $44 >X $75 >Y $66 >P $21d7 $18 >M $21d8 $75 >M $21d9 $74 >M
T{ op PC S A X Y P -> $21d8 $fd $46 $44 $75 $66 }T T{ $21d7 M $21d8 M $21d9 M -> $18 $75 $74 }T
$7cdc >PC $98 >S $a9 >A $b8 >X $da >Y $65 >P $7cdc $18 >M $7cdd $f9 >M $7cde $ba >M
T{ op PC S A X Y P -> $7cdd $98 $a9 $b8 $da $64 }T T{ $7cdc M $7cdd M $7cde M -> $18 $f9 $ba }T
$a1df >PC $22 >S $74 >A $4f >X $29 >Y $62 >P $a1df $18 >M $a1e0 $8d >M $a1e1 $26 >M
T{ op PC S A X Y P -> $a1e0 $22 $74 $4f $29 $62 }T T{ $a1df M $a1e0 M $a1e1 M -> $18 $8d $26 }T
$7b84 >PC $c4 >S $50 >A $d4 >X $a8 >Y $a3 >P $7b84 $18 >M $7b85 $55 >M $7b86 $12 >M
T{ op PC S A X Y P -> $7b85 $c4 $50 $d4 $a8 $a2 }T T{ $7b84 M $7b85 M $7b86 M -> $18 $55 $12 }T
$404d >PC $3d >S $03 >A $a9 >X $40 >Y $22 >P $404d $18 >M $404e $e9 >M $404f $73 >M
T{ op PC S A X Y P -> $404e $3d $03 $a9 $40 $22 }T T{ $404d M $404e M $404f M -> $18 $e9 $73 }T
$6bfe >PC $5a >S $84 >A $d6 >X $c6 >Y $e4 >P $6bfe $18 >M $6bff $4a >M $6c00 $75 >M
T{ op PC S A X Y P -> $6bff $5a $84 $d6 $c6 $e4 }T T{ $6bfe M $6bff M $6c00 M -> $18 $4a $75 }T
$60b7 >PC $6e >S $d7 >A $7d >X $a9 >Y $a2 >P $60b7 $18 >M $60b8 $8f >M $60b9 $43 >M
T{ op PC S A X Y P -> $60b8 $6e $d7 $7d $a9 $a2 }T T{ $60b7 M $60b8 M $60b9 M -> $18 $8f $43 }T
$80e0 >PC $f1 >S $1a >A $e1 >X $04 >Y $a5 >P $80e0 $18 >M $80e1 $0a >M $80e2 $c1 >M
T{ op PC S A X Y P -> $80e1 $f1 $1a $e1 $04 $a4 }T T{ $80e0 M $80e1 M $80e2 M -> $18 $0a $c1 }T
$1ad7 >PC $06 >S $d2 >A $4b >X $f1 >Y $e4 >P $1ad7 $18 >M $1ad8 $9d >M $1ad9 $5a >M
T{ op PC S A X Y P -> $1ad8 $06 $d2 $4b $f1 $e4 }T T{ $1ad7 M $1ad8 M $1ad9 M -> $18 $9d $5a }T
$dad9 >PC $e7 >S $8d >A $46 >X $18 >Y $65 >P $dad9 $18 >M $dada $af >M $dadb $f5 >M
T{ op PC S A X Y P -> $dada $e7 $8d $46 $18 $64 }T T{ $dad9 M $dada M $dadb M -> $18 $af $f5 }T
$9cb3 >PC $90 >S $1a >A $cb >X $d2 >Y $67 >P $9cb3 $18 >M $9cb4 $ec >M $9cb5 $35 >M
T{ op PC S A X Y P -> $9cb4 $90 $1a $cb $d2 $66 }T T{ $9cb3 M $9cb4 M $9cb5 M -> $18 $ec $35 }T
$f6de >PC $1a >S $62 >A $b7 >X $22 >Y $61 >P $f6de $18 >M $f6df $fd >M $f6e0 $fb >M
T{ op PC S A X Y P -> $f6df $1a $62 $b7 $22 $60 }T T{ $f6de M $f6df M $f6e0 M -> $18 $fd $fb }T
$821b >PC $f0 >S $07 >A $c2 >X $a4 >Y $e2 >P $821b $18 >M $821c $a4 >M $821d $76 >M
T{ op PC S A X Y P -> $821c $f0 $07 $c2 $a4 $e2 }T T{ $821b M $821c M $821d M -> $18 $a4 $76 }T
$4df6 >PC $0e >S $cd >A $56 >X $94 >Y $64 >P $4df6 $18 >M $4df7 $41 >M $4df8 $bf >M
T{ op PC S A X Y P -> $4df7 $0e $cd $56 $94 $64 }T T{ $4df6 M $4df7 M $4df8 M -> $18 $41 $bf }T
$dea5 >PC $1c >S $d2 >A $c7 >X $f8 >Y $24 >P $dea5 $18 >M $dea6 $a0 >M $dea7 $77 >M
T{ op PC S A X Y P -> $dea6 $1c $d2 $c7 $f8 $24 }T T{ $dea5 M $dea6 M $dea7 M -> $18 $a0 $77 }T
$420b >PC $7a >S $a0 >A $8c >X $24 >Y $a3 >P $420b $18 >M $420c $19 >M $420d $f0 >M
T{ op PC S A X Y P -> $420c $7a $a0 $8c $24 $a2 }T T{ $420b M $420c M $420d M -> $18 $19 $f0 }T
( 19 )
$57c5 >PC $5b >S $e2 >A $ac >X $89 >Y $e5 >P $1363 $25 >M $57c5 $19 >M $57c6 $da >M $57c7 $12 >M $57c8 $35 >M
T{ op PC S A X Y P -> $57c8 $5b $e7 $ac $89 $e5 }T T{ $1363 M $57c5 M $57c6 M $57c7 M $57c8 M -> $25 $19 $da $12 $35 }T
$886b >PC $aa >S $b0 >A $7e >X $8f >Y $26 >P $886b $19 >M $886c $f5 >M $886d $e3 >M $886e $fa >M $e484 $97 >M
T{ op PC S A X Y P -> $886e $aa $b7 $7e $8f $a4 }T T{ $886b M $886c M $886d M $886e M $e484 M -> $19 $f5 $e3 $fa $97 }T
$5a3c >PC $7a >S $76 >A $e9 >X $92 >Y $a6 >P $5a3c $19 >M $5a3d $dd >M $5a3e $97 >M $5a3f $19 >M $986f $f9 >M
T{ op PC S A X Y P -> $5a3f $7a $ff $e9 $92 $a4 }T T{ $5a3c M $5a3d M $5a3e M $5a3f M $986f M -> $19 $dd $97 $19 $f9 }T
$1b39 >PC $d4 >S $62 >A $f4 >X $d1 >Y $e0 >P $1b39 $19 >M $1b3a $b7 >M $1b3b $d9 >M $1b3c $ee >M $da88 $99 >M
T{ op PC S A X Y P -> $1b3c $d4 $fb $f4 $d1 $e0 }T T{ $1b39 M $1b3a M $1b3b M $1b3c M $da88 M -> $19 $b7 $d9 $ee $99 }T
$1f45 >PC $a5 >S $b7 >A $5b >X $1e >Y $21 >P $1f45 $19 >M $1f46 $e3 >M $1f47 $a2 >M $1f48 $ad >M $a301 $5c >M
T{ op PC S A X Y P -> $1f48 $a5 $ff $5b $1e $a1 }T T{ $1f45 M $1f46 M $1f47 M $1f48 M $a301 M -> $19 $e3 $a2 $ad $5c }T
$3c67 >PC $d3 >S $27 >A $89 >X $13 >Y $65 >P $07d9 $78 >M $3c67 $19 >M $3c68 $c6 >M $3c69 $07 >M $3c6a $5b >M
T{ op PC S A X Y P -> $3c6a $d3 $7f $89 $13 $65 }T T{ $07d9 M $3c67 M $3c68 M $3c69 M $3c6a M -> $78 $19 $c6 $07 $5b }T
$4d3d >PC $aa >S $de >A $0b >X $9e >Y $a4 >P $4d3d $19 >M $4d3e $37 >M $4d3f $fe >M $4d40 $8d >M $fed5 $fb >M
T{ op PC S A X Y P -> $4d40 $aa $ff $0b $9e $a4 }T T{ $4d3d M $4d3e M $4d3f M $4d40 M $fed5 M -> $19 $37 $fe $8d $fb }T
$0683 >PC $4f >S $88 >A $c7 >X $f2 >Y $e7 >P $0683 $19 >M $0684 $f5 >M $0685 $c1 >M $0686 $1c >M $c2e7 $75 >M
T{ op PC S A X Y P -> $0686 $4f $fd $c7 $f2 $e5 }T T{ $0683 M $0684 M $0685 M $0686 M $c2e7 M -> $19 $f5 $c1 $1c $75 }T
$c860 >PC $e4 >S $a4 >A $d6 >X $96 >Y $62 >P $9839 $cc >M $c860 $19 >M $c861 $a3 >M $c862 $97 >M $c863 $c8 >M
T{ op PC S A X Y P -> $c863 $e4 $ec $d6 $96 $e0 }T T{ $9839 M $c860 M $c861 M $c862 M $c863 M -> $cc $19 $a3 $97 $c8 }T
$1e68 >PC $ef >S $15 >A $a1 >X $da >Y $64 >P $1e68 $19 >M $1e69 $e4 >M $1e6a $bf >M $1e6b $1d >M $c0be $a0 >M
T{ op PC S A X Y P -> $1e6b $ef $b5 $a1 $da $e4 }T T{ $1e68 M $1e69 M $1e6a M $1e6b M $c0be M -> $19 $e4 $bf $1d $a0 }T
$f5b3 >PC $57 >S $6d >A $e8 >X $d3 >Y $a7 >P $e786 $ea >M $f5b3 $19 >M $f5b4 $b3 >M $f5b5 $e6 >M $f5b6 $37 >M
T{ op PC S A X Y P -> $f5b6 $57 $ef $e8 $d3 $a5 }T T{ $e786 M $f5b3 M $f5b4 M $f5b5 M $f5b6 M -> $ea $19 $b3 $e6 $37 }T
$02e4 >PC $94 >S $2f >A $24 >X $2b >Y $a2 >P $02e4 $19 >M $02e5 $d4 >M $02e6 $4a >M $02e7 $89 >M $4aff $da >M
T{ op PC S A X Y P -> $02e7 $94 $ff $24 $2b $a0 }T T{ $02e4 M $02e5 M $02e6 M $02e7 M $4aff M -> $19 $d4 $4a $89 $da }T
$4a3b >PC $11 >S $00 >A $7d >X $5b >Y $23 >P $4a3b $19 >M $4a3c $c6 >M $4a3d $99 >M $4a3e $de >M $9a21 $a7 >M
T{ op PC S A X Y P -> $4a3e $11 $a7 $7d $5b $a1 }T T{ $4a3b M $4a3c M $4a3d M $4a3e M $9a21 M -> $19 $c6 $99 $de $a7 }T
$cf8a >PC $ca >S $44 >A $2f >X $1b >Y $e6 >P $cf8a $19 >M $cf8b $6e >M $cf8c $d7 >M $cf8d $97 >M $d789 $4b >M
T{ op PC S A X Y P -> $cf8d $ca $4f $2f $1b $64 }T T{ $cf8a M $cf8b M $cf8c M $cf8d M $d789 M -> $19 $6e $d7 $97 $4b }T
$9fbc >PC $80 >S $34 >A $9a >X $51 >Y $20 >P $6d13 $34 >M $9fbc $19 >M $9fbd $c2 >M $9fbe $6c >M $9fbf $81 >M
T{ op PC S A X Y P -> $9fbf $80 $34 $9a $51 $20 }T T{ $6d13 M $9fbc M $9fbd M $9fbe M $9fbf M -> $34 $19 $c2 $6c $81 }T
$d3d0 >PC $31 >S $2f >A $48 >X $e3 >Y $67 >P $d3d0 $19 >M $d3d1 $ad >M $d3d2 $d9 >M $d3d3 $70 >M $da90 $b7 >M
T{ op PC S A X Y P -> $d3d3 $31 $bf $48 $e3 $e5 }T T{ $d3d0 M $d3d1 M $d3d2 M $d3d3 M $da90 M -> $19 $ad $d9 $70 $b7 }T
( 1a )
$6882 >PC $3f >S $84 >A $d6 >X $02 >Y $27 >P $6882 $1a >M $6883 $ee >M $6884 $bd >M
T{ op PC S A X Y P -> $6883 $3f $85 $d6 $02 $a5 }T T{ $6882 M $6883 M $6884 M -> $1a $ee $bd }T
$29e9 >PC $4f >S $e3 >A $86 >X $ea >Y $60 >P $29e9 $1a >M $29ea $30 >M $29eb $b9 >M
T{ op PC S A X Y P -> $29ea $4f $e4 $86 $ea $e0 }T T{ $29e9 M $29ea M $29eb M -> $1a $30 $b9 }T
$4671 >PC $81 >S $a5 >A $8f >X $37 >Y $e1 >P $4671 $1a >M $4672 $20 >M $4673 $58 >M
T{ op PC S A X Y P -> $4672 $81 $a6 $8f $37 $e1 }T T{ $4671 M $4672 M $4673 M -> $1a $20 $58 }T
$cf45 >PC $2f >S $62 >A $3b >X $8d >Y $65 >P $cf45 $1a >M $cf46 $8a >M $cf47 $d5 >M
T{ op PC S A X Y P -> $cf46 $2f $63 $3b $8d $65 }T T{ $cf45 M $cf46 M $cf47 M -> $1a $8a $d5 }T
$d67e >PC $bb >S $1e >A $bc >X $f8 >Y $a2 >P $d67e $1a >M $d67f $75 >M $d680 $01 >M
T{ op PC S A X Y P -> $d67f $bb $1f $bc $f8 $20 }T T{ $d67e M $d67f M $d680 M -> $1a $75 $01 }T
$a030 >PC $ad >S $00 >A $95 >X $ba >Y $e1 >P $a030 $1a >M $a031 $74 >M $a032 $f5 >M
T{ op PC S A X Y P -> $a031 $ad $01 $95 $ba $61 }T T{ $a030 M $a031 M $a032 M -> $1a $74 $f5 }T
$6e5d >PC $ac >S $d2 >A $ab >X $df >Y $e6 >P $6e5d $1a >M $6e5e $09 >M $6e5f $00 >M
T{ op PC S A X Y P -> $6e5e $ac $d3 $ab $df $e4 }T T{ $6e5d M $6e5e M $6e5f M -> $1a $09 $00 }T
$fc22 >PC $4a >S $19 >A $85 >X $eb >Y $63 >P $fc22 $1a >M $fc23 $59 >M $fc24 $af >M
T{ op PC S A X Y P -> $fc23 $4a $1a $85 $eb $61 }T T{ $fc22 M $fc23 M $fc24 M -> $1a $59 $af }T
$55b1 >PC $67 >S $a6 >A $35 >X $40 >Y $64 >P $55b1 $1a >M $55b2 $ef >M $55b3 $a9 >M
T{ op PC S A X Y P -> $55b2 $67 $a7 $35 $40 $e4 }T T{ $55b1 M $55b2 M $55b3 M -> $1a $ef $a9 }T
$5826 >PC $87 >S $65 >A $d5 >X $80 >Y $e4 >P $5826 $1a >M $5827 $37 >M $5828 $a8 >M
T{ op PC S A X Y P -> $5827 $87 $66 $d5 $80 $64 }T T{ $5826 M $5827 M $5828 M -> $1a $37 $a8 }T
$3c19 >PC $7c >S $dc >A $2a >X $d4 >Y $e4 >P $3c19 $1a >M $3c1a $5d >M $3c1b $20 >M
T{ op PC S A X Y P -> $3c1a $7c $dd $2a $d4 $e4 }T T{ $3c19 M $3c1a M $3c1b M -> $1a $5d $20 }T
$7633 >PC $56 >S $52 >A $ac >X $3f >Y $62 >P $7633 $1a >M $7634 $f4 >M $7635 $bf >M
T{ op PC S A X Y P -> $7634 $56 $53 $ac $3f $60 }T T{ $7633 M $7634 M $7635 M -> $1a $f4 $bf }T
$dd2c >PC $ac >S $ff >A $32 >X $2a >Y $24 >P $dd2c $1a >M $dd2d $4c >M $dd2e $1c >M
T{ op PC S A X Y P -> $dd2d $ac $00 $32 $2a $26 }T T{ $dd2c M $dd2d M $dd2e M -> $1a $4c $1c }T
$8df3 >PC $93 >S $69 >A $14 >X $49 >Y $a5 >P $8df3 $1a >M $8df4 $89 >M $8df5 $a4 >M
T{ op PC S A X Y P -> $8df4 $93 $6a $14 $49 $25 }T T{ $8df3 M $8df4 M $8df5 M -> $1a $89 $a4 }T
$4f01 >PC $39 >S $5b >A $12 >X $46 >Y $a6 >P $4f01 $1a >M $4f02 $a3 >M $4f03 $8a >M
T{ op PC S A X Y P -> $4f02 $39 $5c $12 $46 $24 }T T{ $4f01 M $4f02 M $4f03 M -> $1a $a3 $8a }T
$976d >PC $64 >S $ec >A $51 >X $38 >Y $20 >P $976d $1a >M $976e $f6 >M $976f $1d >M
T{ op PC S A X Y P -> $976e $64 $ed $51 $38 $a0 }T T{ $976d M $976e M $976f M -> $1a $f6 $1d }T
( 1b )
$73cb >PC $86 >S $f1 >A $38 >X $e3 >Y $25 >P $73cb $1b >M $73cc $73 >M $73cd $22 >M
T{ op PC S A X Y P -> $73cc $86 $f1 $38 $e3 $25 }T T{ $73cb M $73cc M $73cd M -> $1b $73 $22 }T
$7c05 >PC $a2 >S $c0 >A $3a >X $40 >Y $e3 >P $7c05 $1b >M $7c06 $9a >M $7c07 $70 >M
T{ op PC S A X Y P -> $7c06 $a2 $c0 $3a $40 $e3 }T T{ $7c05 M $7c06 M $7c07 M -> $1b $9a $70 }T
$edd9 >PC $2b >S $0a >A $ec >X $fe >Y $65 >P $edd9 $1b >M $edda $27 >M $eddb $92 >M
T{ op PC S A X Y P -> $edda $2b $0a $ec $fe $65 }T T{ $edd9 M $edda M $eddb M -> $1b $27 $92 }T
$7ed0 >PC $ba >S $c5 >A $a7 >X $c2 >Y $60 >P $7ed0 $1b >M $7ed1 $2b >M $7ed2 $57 >M
T{ op PC S A X Y P -> $7ed1 $ba $c5 $a7 $c2 $60 }T T{ $7ed0 M $7ed1 M $7ed2 M -> $1b $2b $57 }T
$ddb1 >PC $92 >S $8a >A $08 >X $95 >Y $e5 >P $ddb1 $1b >M $ddb2 $e4 >M $ddb3 $97 >M
T{ op PC S A X Y P -> $ddb2 $92 $8a $08 $95 $e5 }T T{ $ddb1 M $ddb2 M $ddb3 M -> $1b $e4 $97 }T
$3a3e >PC $01 >S $55 >A $43 >X $0e >Y $60 >P $3a3e $1b >M $3a3f $0e >M $3a40 $17 >M
T{ op PC S A X Y P -> $3a3f $01 $55 $43 $0e $60 }T T{ $3a3e M $3a3f M $3a40 M -> $1b $0e $17 }T
$c6b8 >PC $1a >S $7e >A $5f >X $c2 >Y $a5 >P $c6b8 $1b >M $c6b9 $db >M $c6ba $fe >M
T{ op PC S A X Y P -> $c6b9 $1a $7e $5f $c2 $a5 }T T{ $c6b8 M $c6b9 M $c6ba M -> $1b $db $fe }T
$845c >PC $8b >S $25 >A $ba >X $d8 >Y $60 >P $845c $1b >M $845d $6d >M $845e $84 >M
T{ op PC S A X Y P -> $845d $8b $25 $ba $d8 $60 }T T{ $845c M $845d M $845e M -> $1b $6d $84 }T
$7d2a >PC $7a >S $88 >A $16 >X $90 >Y $62 >P $7d2a $1b >M $7d2b $d0 >M $7d2c $04 >M
T{ op PC S A X Y P -> $7d2b $7a $88 $16 $90 $62 }T T{ $7d2a M $7d2b M $7d2c M -> $1b $d0 $04 }T
$3280 >PC $a4 >S $8b >A $c9 >X $7f >Y $e2 >P $3280 $1b >M $3281 $4d >M $3282 $e0 >M
T{ op PC S A X Y P -> $3281 $a4 $8b $c9 $7f $e2 }T T{ $3280 M $3281 M $3282 M -> $1b $4d $e0 }T
$557e >PC $33 >S $64 >A $77 >X $9d >Y $e7 >P $557e $1b >M $557f $76 >M $5580 $c9 >M
T{ op PC S A X Y P -> $557f $33 $64 $77 $9d $e7 }T T{ $557e M $557f M $5580 M -> $1b $76 $c9 }T
$ad7f >PC $63 >S $80 >A $7b >X $57 >Y $60 >P $ad7f $1b >M $ad80 $de >M $ad81 $4c >M
T{ op PC S A X Y P -> $ad80 $63 $80 $7b $57 $60 }T T{ $ad7f M $ad80 M $ad81 M -> $1b $de $4c }T
$344e >PC $06 >S $1e >A $85 >X $87 >Y $e2 >P $344e $1b >M $344f $75 >M $3450 $32 >M
T{ op PC S A X Y P -> $344f $06 $1e $85 $87 $e2 }T T{ $344e M $344f M $3450 M -> $1b $75 $32 }T
$329a >PC $3b >S $eb >A $5e >X $a7 >Y $65 >P $329a $1b >M $329b $f1 >M $329c $15 >M
T{ op PC S A X Y P -> $329b $3b $eb $5e $a7 $65 }T T{ $329a M $329b M $329c M -> $1b $f1 $15 }T
$a0af >PC $91 >S $fa >A $38 >X $73 >Y $24 >P $a0af $1b >M $a0b0 $a0 >M $a0b1 $80 >M
T{ op PC S A X Y P -> $a0b0 $91 $fa $38 $73 $24 }T T{ $a0af M $a0b0 M $a0b1 M -> $1b $a0 $80 }T
$3322 >PC $48 >S $c1 >A $43 >X $4b >Y $a4 >P $3322 $1b >M $3323 $50 >M $3324 $42 >M
T{ op PC S A X Y P -> $3323 $48 $c1 $43 $4b $a4 }T T{ $3322 M $3323 M $3324 M -> $1b $50 $42 }T
( 1c )
$3bfa >PC $3d >S $2f >A $d2 >X $90 >Y $67 >P $3bfa $1c >M $3bfb $b4 >M $3bfc $98 >M $3bfd $f5 >M $98b4 $48 >M
T{ op PC S A X Y P -> $3bfd $3d $2f $d2 $90 $65 }T T{ $3bfa M $3bfb M $3bfc M $3bfd M $98b4 M -> $1c $b4 $98 $f5 $40 }T
$a799 >PC $c8 >S $8c >A $5d >X $e6 >Y $22 >P $06ff $cb >M $a799 $1c >M $a79a $ff >M $a79b $06 >M $a79c $ac >M
T{ op PC S A X Y P -> $a79c $c8 $8c $5d $e6 $20 }T T{ $06ff M $a799 M $a79a M $a79b M $a79c M -> $43 $1c $ff $06 $ac }T
$f7c6 >PC $b3 >S $08 >A $d8 >X $49 >Y $20 >P $f7c6 $1c >M $f7c7 $80 >M $f7c8 $f9 >M $f7c9 $94 >M $f980 $6c >M
T{ op PC S A X Y P -> $f7c9 $b3 $08 $d8 $49 $20 }T T{ $f7c6 M $f7c7 M $f7c8 M $f7c9 M $f980 M -> $1c $80 $f9 $94 $64 }T
$d3a5 >PC $d2 >S $d1 >A $e0 >X $06 >Y $26 >P $d3a5 $1c >M $d3a6 $a1 >M $d3a7 $d9 >M $d3a8 $c1 >M $d9a1 $5e >M
T{ op PC S A X Y P -> $d3a8 $d2 $d1 $e0 $06 $24 }T T{ $d3a5 M $d3a6 M $d3a7 M $d3a8 M $d9a1 M -> $1c $a1 $d9 $c1 $0e }T
$a465 >PC $56 >S $4e >A $1c >X $b3 >Y $65 >P $8722 $a4 >M $a465 $1c >M $a466 $22 >M $a467 $87 >M $a468 $96 >M
T{ op PC S A X Y P -> $a468 $56 $4e $1c $b3 $65 }T T{ $8722 M $a465 M $a466 M $a467 M $a468 M -> $a0 $1c $22 $87 $96 }T
$fe3e >PC $07 >S $15 >A $5d >X $ce >Y $67 >P $ad98 $2b >M $fe3e $1c >M $fe3f $98 >M $fe40 $ad >M $fe41 $f5 >M
T{ op PC S A X Y P -> $fe41 $07 $15 $5d $ce $65 }T T{ $ad98 M $fe3e M $fe3f M $fe40 M $fe41 M -> $2a $1c $98 $ad $f5 }T
$4ba3 >PC $c0 >S $ba >A $c0 >X $8c >Y $e1 >P $4ba3 $1c >M $4ba4 $ef >M $4ba5 $ce >M $4ba6 $7f >M $ceef $3b >M
T{ op PC S A X Y P -> $4ba6 $c0 $ba $c0 $8c $e1 }T T{ $4ba3 M $4ba4 M $4ba5 M $4ba6 M $ceef M -> $1c $ef $ce $7f $01 }T
$1496 >PC $74 >S $6a >A $cb >X $ec >Y $21 >P $1496 $1c >M $1497 $da >M $1498 $e2 >M $1499 $6c >M $e2da $d7 >M
T{ op PC S A X Y P -> $1499 $74 $6a $cb $ec $21 }T T{ $1496 M $1497 M $1498 M $1499 M $e2da M -> $1c $da $e2 $6c $95 }T
$7dc3 >PC $89 >S $92 >A $08 >X $e2 >Y $e7 >P $7dc3 $1c >M $7dc4 $0d >M $7dc5 $eb >M $7dc6 $d0 >M $eb0d $9a >M
T{ op PC S A X Y P -> $7dc6 $89 $92 $08 $e2 $e5 }T T{ $7dc3 M $7dc4 M $7dc5 M $7dc6 M $eb0d M -> $1c $0d $eb $d0 $08 }T
$f472 >PC $a1 >S $af >A $db >X $98 >Y $24 >P $db15 $6c >M $f472 $1c >M $f473 $15 >M $f474 $db >M $f475 $74 >M
T{ op PC S A X Y P -> $f475 $a1 $af $db $98 $24 }T T{ $db15 M $f472 M $f473 M $f474 M $f475 M -> $40 $1c $15 $db $74 }T
$492f >PC $69 >S $ca >A $dd >X $53 >Y $62 >P $39c5 $0b >M $492f $1c >M $4930 $c5 >M $4931 $39 >M $4932 $a0 >M
T{ op PC S A X Y P -> $4932 $69 $ca $dd $53 $60 }T T{ $39c5 M $492f M $4930 M $4931 M $4932 M -> $01 $1c $c5 $39 $a0 }T
$3e7a >PC $a6 >S $51 >A $53 >X $60 >Y $20 >P $3334 $3c >M $3e7a $1c >M $3e7b $34 >M $3e7c $33 >M $3e7d $de >M
T{ op PC S A X Y P -> $3e7d $a6 $51 $53 $60 $20 }T T{ $3334 M $3e7a M $3e7b M $3e7c M $3e7d M -> $2c $1c $34 $33 $de }T
$cc1a >PC $96 >S $a6 >A $14 >X $92 >Y $a6 >P $5157 $4e >M $cc1a $1c >M $cc1b $57 >M $cc1c $51 >M $cc1d $e1 >M
T{ op PC S A X Y P -> $cc1d $96 $a6 $14 $92 $a4 }T T{ $5157 M $cc1a M $cc1b M $cc1c M $cc1d M -> $48 $1c $57 $51 $e1 }T
$03f7 >PC $95 >S $0c >A $10 >X $96 >Y $25 >P $03f7 $1c >M $03f8 $3c >M $03f9 $ca >M $03fa $93 >M $ca3c $49 >M
T{ op PC S A X Y P -> $03fa $95 $0c $10 $96 $25 }T T{ $03f7 M $03f8 M $03f9 M $03fa M $ca3c M -> $1c $3c $ca $93 $41 }T
$5228 >PC $66 >S $85 >A $eb >X $32 >Y $27 >P $5228 $1c >M $5229 $1c >M $522a $c1 >M $522b $e5 >M $c11c $04 >M
T{ op PC S A X Y P -> $522b $66 $85 $eb $32 $25 }T T{ $5228 M $5229 M $522a M $522b M $c11c M -> $1c $1c $c1 $e5 $00 }T
$ee69 >PC $e0 >S $47 >A $85 >X $8a >Y $64 >P $ebfd $f2 >M $ee69 $1c >M $ee6a $fd >M $ee6b $eb >M $ee6c $c0 >M
T{ op PC S A X Y P -> $ee6c $e0 $47 $85 $8a $64 }T T{ $ebfd M $ee69 M $ee6a M $ee6b M $ee6c M -> $b0 $1c $fd $eb $c0 }T
( 1d )
$d29e >PC $f9 >S $02 >A $82 >X $4f >Y $22 >P $7b02 $25 >M $d29e $1d >M $d29f $80 >M $d2a0 $7a >M $d2a1 $a6 >M
T{ op PC S A X Y P -> $d2a1 $f9 $27 $82 $4f $20 }T T{ $7b02 M $d29e M $d29f M $d2a0 M $d2a1 M -> $25 $1d $80 $7a $a6 }T
$0ade >PC $09 >S $aa >A $8f >X $c1 >Y $62 >P $0ade $1d >M $0adf $dc >M $0ae0 $ed >M $0ae1 $31 >M $ee6b $d6 >M
T{ op PC S A X Y P -> $0ae1 $09 $fe $8f $c1 $e0 }T T{ $0ade M $0adf M $0ae0 M $0ae1 M $ee6b M -> $1d $dc $ed $31 $d6 }T
$846c >PC $6c >S $7c >A $7a >X $c0 >Y $e0 >P $0eba $82 >M $846c $1d >M $846d $40 >M $846e $0e >M $846f $0e >M
T{ op PC S A X Y P -> $846f $6c $fe $7a $c0 $e0 }T T{ $0eba M $846c M $846d M $846e M $846f M -> $82 $1d $40 $0e $0e }T
$cf47 >PC $37 >S $7a >A $60 >X $75 >Y $24 >P $abcb $09 >M $cf47 $1d >M $cf48 $6b >M $cf49 $ab >M $cf4a $41 >M
T{ op PC S A X Y P -> $cf4a $37 $7b $60 $75 $24 }T T{ $abcb M $cf47 M $cf48 M $cf49 M $cf4a M -> $09 $1d $6b $ab $41 }T
$b25e >PC $da >S $d3 >A $e2 >X $c1 >Y $e6 >P $3327 $87 >M $b25e $1d >M $b25f $45 >M $b260 $32 >M $b261 $bf >M
T{ op PC S A X Y P -> $b261 $da $d7 $e2 $c1 $e4 }T T{ $3327 M $b25e M $b25f M $b260 M $b261 M -> $87 $1d $45 $32 $bf }T
$3f2d >PC $6a >S $cf >A $b3 >X $45 >Y $e2 >P $1875 $f6 >M $3f2d $1d >M $3f2e $c2 >M $3f2f $17 >M $3f30 $75 >M
T{ op PC S A X Y P -> $3f30 $6a $ff $b3 $45 $e0 }T T{ $1875 M $3f2d M $3f2e M $3f2f M $3f30 M -> $f6 $1d $c2 $17 $75 }T
$8089 >PC $99 >S $d5 >A $8d >X $94 >Y $e1 >P $7f35 $9a >M $8089 $1d >M $808a $a8 >M $808b $7e >M $808c $d8 >M
T{ op PC S A X Y P -> $808c $99 $df $8d $94 $e1 }T T{ $7f35 M $8089 M $808a M $808b M $808c M -> $9a $1d $a8 $7e $d8 }T
$784e >PC $48 >S $c7 >A $e6 >X $f7 >Y $e4 >P $670f $64 >M $784e $1d >M $784f $29 >M $7850 $66 >M $7851 $0c >M
T{ op PC S A X Y P -> $7851 $48 $e7 $e6 $f7 $e4 }T T{ $670f M $784e M $784f M $7850 M $7851 M -> $64 $1d $29 $66 $0c }T
$2479 >PC $67 >S $19 >A $c3 >X $ce >Y $62 >P $2479 $1d >M $247a $ff >M $247b $3d >M $247c $4a >M $3ec2 $ae >M
T{ op PC S A X Y P -> $247c $67 $bf $c3 $ce $e0 }T T{ $2479 M $247a M $247b M $247c M $3ec2 M -> $1d $ff $3d $4a $ae }T
$859b >PC $3a >S $ae >A $76 >X $6f >Y $e2 >P $1f61 $63 >M $859b $1d >M $859c $eb >M $859d $1e >M $859e $05 >M
T{ op PC S A X Y P -> $859e $3a $ef $76 $6f $e0 }T T{ $1f61 M $859b M $859c M $859d M $859e M -> $63 $1d $eb $1e $05 }T
$a724 >PC $5a >S $a7 >A $c6 >X $63 >Y $e5 >P $a724 $1d >M $a725 $ec >M $a726 $bd >M $a727 $db >M $beb2 $0f >M
T{ op PC S A X Y P -> $a727 $5a $af $c6 $63 $e5 }T T{ $a724 M $a725 M $a726 M $a727 M $beb2 M -> $1d $ec $bd $db $0f }T
$1ded >PC $30 >S $61 >A $0e >X $35 >Y $e0 >P $1a86 $8c >M $1ded $1d >M $1dee $78 >M $1def $1a >M $1df0 $2f >M
T{ op PC S A X Y P -> $1df0 $30 $ed $0e $35 $e0 }T T{ $1a86 M $1ded M $1dee M $1def M $1df0 M -> $8c $1d $78 $1a $2f }T
$ec81 >PC $0a >S $22 >A $66 >X $5d >Y $26 >P $ec81 $1d >M $ec82 $b8 >M $ec83 $ed >M $ec84 $a6 >M $ee1e $d7 >M
T{ op PC S A X Y P -> $ec84 $0a $f7 $66 $5d $a4 }T T{ $ec81 M $ec82 M $ec83 M $ec84 M $ee1e M -> $1d $b8 $ed $a6 $d7 }T
$7136 >PC $78 >S $3f >A $6c >X $16 >Y $24 >P $7136 $1d >M $7137 $c0 >M $7138 $75 >M $7139 $66 >M $762c $2b >M
T{ op PC S A X Y P -> $7139 $78 $3f $6c $16 $24 }T T{ $7136 M $7137 M $7138 M $7139 M $762c M -> $1d $c0 $75 $66 $2b }T
$dc69 >PC $96 >S $6c >A $ac >X $3e >Y $a3 >P $54be $1d >M $dc69 $1d >M $dc6a $12 >M $dc6b $54 >M $dc6c $9e >M
T{ op PC S A X Y P -> $dc6c $96 $7d $ac $3e $21 }T T{ $54be M $dc69 M $dc6a M $dc6b M $dc6c M -> $1d $1d $12 $54 $9e }T
$817d >PC $65 >S $d5 >A $18 >X $c2 >Y $60 >P $2250 $78 >M $817d $1d >M $817e $38 >M $817f $22 >M $8180 $ee >M
T{ op PC S A X Y P -> $8180 $65 $fd $18 $c2 $e0 }T T{ $2250 M $817d M $817e M $817f M $8180 M -> $78 $1d $38 $22 $ee }T
( 1e )
$c2da >PC $92 >S $ea >A $f5 >X $f9 >Y $a5 >P $0254 $e1 >M $c2da $1e >M $c2db $5f >M $c2dc $01 >M $c2dd $32 >M
T{ op PC S A X Y P -> $c2dd $92 $ea $f5 $f9 $a5 }T T{ $0254 M $c2da M $c2db M $c2dc M $c2dd M -> $c2 $1e $5f $01 $32 }T
$3de0 >PC $33 >S $6a >A $ac >X $84 >Y $e2 >P $3de0 $1e >M $3de1 $dd >M $3de2 $fd >M $3de3 $86 >M $fe89 $10 >M
T{ op PC S A X Y P -> $3de3 $33 $6a $ac $84 $60 }T T{ $3de0 M $3de1 M $3de2 M $3de3 M $fe89 M -> $1e $dd $fd $86 $20 }T
$92e3 >PC $e8 >S $dc >A $7f >X $68 >Y $21 >P $382e $8c >M $92e3 $1e >M $92e4 $af >M $92e5 $37 >M $92e6 $d4 >M
T{ op PC S A X Y P -> $92e6 $e8 $dc $7f $68 $21 }T T{ $382e M $92e3 M $92e4 M $92e5 M $92e6 M -> $18 $1e $af $37 $d4 }T
$796b >PC $dc >S $61 >A $27 >X $10 >Y $66 >P $796b $1e >M $796c $93 >M $796d $97 >M $796e $1c >M $97ba $7e >M
T{ op PC S A X Y P -> $796e $dc $61 $27 $10 $e4 }T T{ $796b M $796c M $796d M $796e M $97ba M -> $1e $93 $97 $1c $fc }T
$0184 >PC $ca >S $55 >A $05 >X $19 >Y $e1 >P $0184 $1e >M $0185 $2a >M $0186 $9b >M $0187 $0d >M $9b2f $11 >M
T{ op PC S A X Y P -> $0187 $ca $55 $05 $19 $60 }T T{ $0184 M $0185 M $0186 M $0187 M $9b2f M -> $1e $2a $9b $0d $22 }T
$2b8d >PC $7f >S $96 >A $f8 >X $23 >Y $20 >P $2b8d $1e >M $2b8e $78 >M $2b8f $b8 >M $2b90 $d7 >M $b970 $9d >M
T{ op PC S A X Y P -> $2b90 $7f $96 $f8 $23 $21 }T T{ $2b8d M $2b8e M $2b8f M $2b90 M $b970 M -> $1e $78 $b8 $d7 $3a }T
$4613 >PC $b4 >S $3e >A $ea >X $8e >Y $e0 >P $2709 $46 >M $4613 $1e >M $4614 $1f >M $4615 $26 >M $4616 $d9 >M
T{ op PC S A X Y P -> $4616 $b4 $3e $ea $8e $e0 }T T{ $2709 M $4613 M $4614 M $4615 M $4616 M -> $8c $1e $1f $26 $d9 }T
$c184 >PC $59 >S $4c >A $c1 >X $df >Y $a6 >P $3202 $72 >M $c184 $1e >M $c185 $41 >M $c186 $31 >M $c187 $2c >M
T{ op PC S A X Y P -> $c187 $59 $4c $c1 $df $a4 }T T{ $3202 M $c184 M $c185 M $c186 M $c187 M -> $e4 $1e $41 $31 $2c }T
$a926 >PC $08 >S $c5 >A $c3 >X $33 >Y $a0 >P $a926 $1e >M $a927 $b7 >M $a928 $b5 >M $a929 $d8 >M $b67a $df >M
T{ op PC S A X Y P -> $a929 $08 $c5 $c3 $33 $a1 }T T{ $a926 M $a927 M $a928 M $a929 M $b67a M -> $1e $b7 $b5 $d8 $be }T
$c16d >PC $48 >S $19 >A $fb >X $05 >Y $27 >P $3de0 $ec >M $c16d $1e >M $c16e $e5 >M $c16f $3c >M $c170 $0a >M
T{ op PC S A X Y P -> $c170 $48 $19 $fb $05 $a5 }T T{ $3de0 M $c16d M $c16e M $c16f M $c170 M -> $d8 $1e $e5 $3c $0a }T
$4e6b >PC $5a >S $6c >A $db >X $a8 >Y $61 >P $238e $49 >M $4e6b $1e >M $4e6c $b3 >M $4e6d $22 >M $4e6e $80 >M
T{ op PC S A X Y P -> $4e6e $5a $6c $db $a8 $e0 }T T{ $238e M $4e6b M $4e6c M $4e6d M $4e6e M -> $92 $1e $b3 $22 $80 }T
$335a >PC $12 >S $81 >A $15 >X $2a >Y $e6 >P $335a $1e >M $335b $2a >M $335c $6d >M $335d $8d >M $6d3f $4b >M
T{ op PC S A X Y P -> $335d $12 $81 $15 $2a $e4 }T T{ $335a M $335b M $335c M $335d M $6d3f M -> $1e $2a $6d $8d $96 }T
$d6e0 >PC $27 >S $92 >A $f8 >X $ea >Y $a5 >P $836e $88 >M $d6e0 $1e >M $d6e1 $76 >M $d6e2 $82 >M $d6e3 $db >M
T{ op PC S A X Y P -> $d6e3 $27 $92 $f8 $ea $25 }T T{ $836e M $d6e0 M $d6e1 M $d6e2 M $d6e3 M -> $10 $1e $76 $82 $db }T
$6407 >PC $c7 >S $02 >A $c0 >X $90 >Y $23 >P $54a5 $4a >M $6407 $1e >M $6408 $e5 >M $6409 $53 >M $640a $04 >M
T{ op PC S A X Y P -> $640a $c7 $02 $c0 $90 $a0 }T T{ $54a5 M $6407 M $6408 M $6409 M $640a M -> $94 $1e $e5 $53 $04 }T
$cdd2 >PC $65 >S $c6 >A $60 >X $28 >Y $e0 >P $1ba9 $45 >M $cdd2 $1e >M $cdd3 $49 >M $cdd4 $1b >M $cdd5 $4d >M
T{ op PC S A X Y P -> $cdd5 $65 $c6 $60 $28 $e0 }T T{ $1ba9 M $cdd2 M $cdd3 M $cdd4 M $cdd5 M -> $8a $1e $49 $1b $4d }T
$9837 >PC $ae >S $bc >A $a9 >X $40 >Y $a3 >P $37ec $0b >M $9837 $1e >M $9838 $43 >M $9839 $37 >M $983a $22 >M
T{ op PC S A X Y P -> $983a $ae $bc $a9 $40 $20 }T T{ $37ec M $9837 M $9838 M $9839 M $983a M -> $16 $1e $43 $37 $22 }T
( 1f )
$863a >PC $8d >S $89 >A $d3 >X $10 >Y $e4 >P $005d $b6 >M $863a $1f >M $863b $5d >M $863c $18 >M $863d $48 >M $8655 $bb >M
T{ op PC S A X Y P -> $863d $8d $89 $d3 $10 $e4 }T T{ $005d M $863a M $863b M $863c M $863d M $8655 M -> $b6 $1f $5d $18 $48 $bb }T
$2cc6 >PC $6c >S $67 >A $ac >X $98 >Y $25 >P $0053 $f4 >M $2cc6 $1f >M $2cc7 $53 >M $2cc8 $19 >M $2ce2 $24 >M
T{ op PC S A X Y P -> $2ce2 $6c $67 $ac $98 $25 }T T{ $0053 M $2cc6 M $2cc7 M $2cc8 M $2ce2 M -> $f4 $1f $53 $19 $24 }T
$2356 >PC $77 >S $c7 >A $06 >X $58 >Y $23 >P $0088 $6b >M $2356 $1f >M $2357 $88 >M $2358 $9f >M $2359 $ac >M $23f8 $36 >M
T{ op PC S A X Y P -> $2359 $77 $c7 $06 $58 $23 }T T{ $0088 M $2356 M $2357 M $2358 M $2359 M $23f8 M -> $6b $1f $88 $9f $ac $36 }T
$b915 >PC $10 >S $1e >A $d2 >X $b6 >Y $a5 >P $00f5 $f7 >M $b915 $1f >M $b916 $f5 >M $b917 $0b >M $b918 $fe >M $b923 $4c >M
T{ op PC S A X Y P -> $b918 $10 $1e $d2 $b6 $a5 }T T{ $00f5 M $b915 M $b916 M $b917 M $b918 M $b923 M -> $f7 $1f $f5 $0b $fe $4c }T
$dd35 >PC $14 >S $5e >A $3a >X $62 >Y $20 >P $0016 $61 >M $dcc2 $e8 >M $dd35 $1f >M $dd36 $16 >M $dd37 $8a >M $ddc2 $fc >M
T{ op PC S A X Y P -> $dcc2 $14 $5e $3a $62 $20 }T T{ $0016 M $dcc2 M $dd35 M $dd36 M $dd37 M $ddc2 M -> $61 $e8 $1f $16 $8a $fc }T
$2de1 >PC $df >S $f8 >A $c1 >X $7e >Y $a5 >P $00cd $bf >M $2d35 $49 >M $2de1 $1f >M $2de2 $cd >M $2de3 $51 >M $2de4 $02 >M
T{ op PC S A X Y P -> $2de4 $df $f8 $c1 $7e $a5 }T T{ $00cd M $2d35 M $2de1 M $2de2 M $2de3 M $2de4 M -> $bf $49 $1f $cd $51 $02 }T
$130d >PC $d5 >S $63 >A $2d >X $b7 >Y $21 >P $003a $0f >M $130d $1f >M $130e $3a >M $130f $30 >M $1310 $0e >M $1340 $a3 >M
T{ op PC S A X Y P -> $1310 $d5 $63 $2d $b7 $21 }T T{ $003a M $130d M $130e M $130f M $1310 M $1340 M -> $0f $1f $3a $30 $0e $a3 }T
$06cb >PC $dd >S $c5 >A $c2 >X $29 >Y $23 >P $005a $1c >M $0629 $65 >M $06cb $1f >M $06cc $5a >M $06cd $5b >M $0729 $9b >M
T{ op PC S A X Y P -> $0729 $dd $c5 $c2 $29 $23 }T T{ $005a M $0629 M $06cb M $06cc M $06cd M $0729 M -> $1c $65 $1f $5a $5b $9b }T
$5371 >PC $18 >S $2b >A $e5 >X $57 >Y $27 >P $0083 $c5 >M $52f5 $7e >M $5371 $1f >M $5372 $83 >M $5373 $81 >M $53f5 $e0 >M
T{ op PC S A X Y P -> $52f5 $18 $2b $e5 $57 $27 }T T{ $0083 M $52f5 M $5371 M $5372 M $5373 M $53f5 M -> $c5 $7e $1f $83 $81 $e0 }T
$a03f >PC $2d >S $46 >A $0c >X $17 >Y $66 >P $0066 $e7 >M $a03f $1f >M $a040 $66 >M $a041 $38 >M $a042 $a1 >M $a07a $62 >M
T{ op PC S A X Y P -> $a042 $2d $46 $0c $17 $66 }T T{ $0066 M $a03f M $a040 M $a041 M $a042 M $a07a M -> $e7 $1f $66 $38 $a1 $62 }T
$3adb >PC $7a >S $68 >A $fd >X $ee >Y $27 >P $003f $13 >M $3ac5 $0f >M $3adb $1f >M $3adc $3f >M $3add $e7 >M $3ade $dd >M
T{ op PC S A X Y P -> $3ade $7a $68 $fd $ee $27 }T T{ $003f M $3ac5 M $3adb M $3adc M $3add M $3ade M -> $13 $0f $1f $3f $e7 $dd }T
$6a60 >PC $f2 >S $77 >A $17 >X $09 >Y $67 >P $0024 $6f >M $6a60 $1f >M $6a61 $24 >M $6a62 $2b >M $6a63 $8e >M $6a8e $06 >M
T{ op PC S A X Y P -> $6a63 $f2 $77 $17 $09 $67 }T T{ $0024 M $6a60 M $6a61 M $6a62 M $6a63 M $6a8e M -> $6f $1f $24 $2b $8e $06 }T
$942d >PC $7f >S $ae >A $8f >X $7c >Y $61 >P $009a $f1 >M $942d $1f >M $942e $9a >M $942f $3d >M $946d $ea >M
T{ op PC S A X Y P -> $946d $7f $ae $8f $7c $61 }T T{ $009a M $942d M $942e M $942f M $946d M -> $f1 $1f $9a $3d $ea }T
$c9e3 >PC $1e >S $aa >A $04 >X $6c >Y $a0 >P $002a $fd >M $c9bc $c7 >M $c9e3 $1f >M $c9e4 $2a >M $c9e5 $d6 >M
T{ op PC S A X Y P -> $c9bc $1e $aa $04 $6c $a0 }T T{ $002a M $c9bc M $c9e3 M $c9e4 M $c9e5 M -> $fd $c7 $1f $2a $d6 }T
$0c95 >PC $94 >S $36 >A $c1 >X $be >Y $21 >P $00aa $f0 >M $0c95 $1f >M $0c96 $aa >M $0c97 $5a >M $0cf2 $5b >M
T{ op PC S A X Y P -> $0cf2 $94 $36 $c1 $be $21 }T T{ $00aa M $0c95 M $0c96 M $0c97 M $0cf2 M -> $f0 $1f $aa $5a $5b }T
$d5f9 >PC $a3 >S $78 >A $4a >X $97 >Y $e2 >P $0063 $b2 >M $d5f8 $89 >M $d5f9 $1f >M $d5fa $63 >M $d5fb $fc >M $d5fc $8d >M
T{ op PC S A X Y P -> $d5fc $a3 $78 $4a $97 $e2 }T T{ $0063 M $d5f8 M $d5f9 M $d5fa M $d5fb M $d5fc M -> $b2 $89 $1f $63 $fc $8d }T
( 20 )
$5c68 >PC $20 >S $d6 >A $f7 >X $7c >Y $27 >P $0120 $48 >M $5c68 $20 >M $5c69 $7e >M $5c6a $83 >M $837e $0a >M
T{ op PC S A X Y P -> $837e $1e $d6 $f7 $7c $27 }T T{ $011f M $0120 M $5c68 M $5c69 M $5c6a M $837e M -> $6a $5c $20 $7e $83 $0a }T
$3521 >PC $48 >S $63 >A $bb >X $36 >Y $a5 >P $0148 $14 >M $1d74 $4c >M $3521 $20 >M $3522 $74 >M $3523 $1d >M
T{ op PC S A X Y P -> $1d74 $46 $63 $bb $36 $a5 }T T{ $0147 M $0148 M $1d74 M $3521 M $3522 M $3523 M -> $23 $35 $4c $20 $74 $1d }T
$53a9 >PC $fa >S $e6 >A $d1 >X $84 >Y $62 >P $01fa $92 >M $53a9 $20 >M $53aa $a8 >M $53ab $59 >M $59a8 $4a >M
T{ op PC S A X Y P -> $59a8 $f8 $e6 $d1 $84 $62 }T T{ $01f9 M $01fa M $53a9 M $53aa M $53ab M $59a8 M -> $ab $53 $20 $a8 $59 $4a }T
$f090 >PC $3f >S $f3 >A $24 >X $bb >Y $24 >P $013f $7c >M $4e78 $6e >M $f090 $20 >M $f091 $78 >M $f092 $4e >M
T{ op PC S A X Y P -> $4e78 $3d $f3 $24 $bb $24 }T T{ $013e M $013f M $4e78 M $f090 M $f091 M $f092 M -> $92 $f0 $6e $20 $78 $4e }T
$849d >PC $3f >S $40 >A $90 >X $69 >Y $24 >P $013f $11 >M $849d $20 >M $849e $28 >M $849f $d7 >M $d728 $de >M
T{ op PC S A X Y P -> $d728 $3d $40 $90 $69 $24 }T T{ $013e M $013f M $849d M $849e M $849f M $d728 M -> $9f $84 $20 $28 $d7 $de }T
$c1ac >PC $2e >S $79 >A $20 >X $59 >Y $a0 >P $012e $b2 >M $748a $4d >M $c1ac $20 >M $c1ad $8a >M $c1ae $74 >M
T{ op PC S A X Y P -> $748a $2c $79 $20 $59 $a0 }T T{ $012d M $012e M $748a M $c1ac M $c1ad M $c1ae M -> $ae $c1 $4d $20 $8a $74 }T
$eb9a >PC $3c >S $25 >A $49 >X $94 >Y $a1 >P $013c $ba >M $dc19 $19 >M $eb9a $20 >M $eb9b $19 >M $eb9c $dc >M
T{ op PC S A X Y P -> $dc19 $3a $25 $49 $94 $a1 }T T{ $013b M $013c M $dc19 M $eb9a M $eb9b M $eb9c M -> $9c $eb $19 $20 $19 $dc }T
$3a6b >PC $49 >S $45 >A $ca >X $da >Y $a4 >P $0149 $a8 >M $2ae0 $db >M $3a6b $20 >M $3a6c $e0 >M $3a6d $2a >M
T{ op PC S A X Y P -> $2ae0 $47 $45 $ca $da $a4 }T T{ $0148 M $0149 M $2ae0 M $3a6b M $3a6c M $3a6d M -> $6d $3a $db $20 $e0 $2a }T
$82c3 >PC $de >S $2f >A $1d >X $90 >Y $a2 >P $01de $41 >M $82c3 $20 >M $82c4 $8f >M $82c5 $da >M $da8f $70 >M
T{ op PC S A X Y P -> $da8f $dc $2f $1d $90 $a2 }T T{ $01dd M $01de M $82c3 M $82c4 M $82c5 M $da8f M -> $c5 $82 $20 $8f $da $70 }T
$d97d >PC $a5 >S $f4 >A $ca >X $63 >Y $e2 >P $01a5 $5c >M $1dab $7a >M $d97d $20 >M $d97e $ab >M $d97f $1d >M
T{ op PC S A X Y P -> $1dab $a3 $f4 $ca $63 $e2 }T T{ $01a4 M $01a5 M $1dab M $d97d M $d97e M $d97f M -> $7f $d9 $7a $20 $ab $1d }T
$debb >PC $49 >S $11 >A $a9 >X $69 >Y $23 >P $0149 $80 >M $8e1c $96 >M $debb $20 >M $debc $1c >M $debd $8e >M
T{ op PC S A X Y P -> $8e1c $47 $11 $a9 $69 $23 }T T{ $0148 M $0149 M $8e1c M $debb M $debc M $debd M -> $bd $de $96 $20 $1c $8e }T
$a471 >PC $c6 >S $47 >A $34 >X $43 >Y $e0 >P $01c6 $5b >M $2199 $8d >M $a471 $20 >M $a472 $99 >M $a473 $21 >M
T{ op PC S A X Y P -> $2199 $c4 $47 $34 $43 $e0 }T T{ $01c5 M $01c6 M $2199 M $a471 M $a472 M $a473 M -> $73 $a4 $8d $20 $99 $21 }T
$3e36 >PC $5a >S $1d >A $ed >X $0d >Y $a7 >P $015a $9d >M $3e36 $20 >M $3e37 $89 >M $3e38 $ae >M $ae89 $67 >M
T{ op PC S A X Y P -> $ae89 $58 $1d $ed $0d $a7 }T T{ $0159 M $015a M $3e36 M $3e37 M $3e38 M $ae89 M -> $38 $3e $20 $89 $ae $67 }T
$d725 >PC $37 >S $28 >A $e0 >X $18 >Y $e1 >P $0137 $94 >M $4747 $b0 >M $d725 $20 >M $d726 $47 >M $d727 $47 >M
T{ op PC S A X Y P -> $4747 $35 $28 $e0 $18 $e1 }T T{ $0136 M $0137 M $4747 M $d725 M $d726 M $d727 M -> $27 $d7 $b0 $20 $47 $47 }T
$ea1d >PC $5d >S $f1 >A $42 >X $0f >Y $61 >P $015d $88 >M $ae53 $73 >M $ea1d $20 >M $ea1e $53 >M $ea1f $ae >M
T{ op PC S A X Y P -> $ae53 $5b $f1 $42 $0f $61 }T T{ $015c M $015d M $ae53 M $ea1d M $ea1e M $ea1f M -> $1f $ea $73 $20 $53 $ae }T
$40ea >PC $0d >S $76 >A $33 >X $6f >Y $26 >P $010d $c4 >M $40ea $20 >M $40eb $c4 >M $40ec $88 >M $88c4 $0b >M
T{ op PC S A X Y P -> $88c4 $0b $76 $33 $6f $26 }T T{ $010c M $010d M $40ea M $40eb M $40ec M $88c4 M -> $ec $40 $20 $c4 $88 $0b }T
( 21 )
$aadf >PC $c9 >S $7b >A $07 >X $53 >Y $a3 >P $0067 $58 >M $006e $32 >M $006f $30 >M $3032 $19 >M $aadf $21 >M $aae0 $67 >M $aae1 $4d >M
T{ op PC S A X Y P -> $aae1 $c9 $19 $07 $53 $21 }T T{ $0067 M $006e M $006f M $3032 M $aadf M $aae0 M $aae1 M -> $58 $32 $30 $19 $21 $67 $4d }T
$adf2 >PC $25 >S $96 >A $ad >X $d2 >Y $a0 >P $0030 $11 >M $0031 $35 >M $0083 $ee >M $3511 $7a >M $adf2 $21 >M $adf3 $83 >M $adf4 $f2 >M
T{ op PC S A X Y P -> $adf4 $25 $12 $ad $d2 $20 }T T{ $0030 M $0031 M $0083 M $3511 M $adf2 M $adf3 M $adf4 M -> $11 $35 $ee $7a $21 $83 $f2 }T
$412c >PC $e3 >S $b9 >A $07 >X $5b >Y $64 >P $0018 $33 >M $001f $82 >M $0020 $6f >M $412c $21 >M $412d $18 >M $412e $2c >M $6f82 $bd >M
T{ op PC S A X Y P -> $412e $e3 $b9 $07 $5b $e4 }T T{ $0018 M $001f M $0020 M $412c M $412d M $412e M $6f82 M -> $33 $82 $6f $21 $18 $2c $bd }T
$186b >PC $f5 >S $b1 >A $45 >X $d0 >Y $e1 >P $0091 $e7 >M $00d6 $5b >M $00d7 $7b >M $186b $21 >M $186c $91 >M $186d $c0 >M $7b5b $9d >M
T{ op PC S A X Y P -> $186d $f5 $91 $45 $d0 $e1 }T T{ $0091 M $00d6 M $00d7 M $186b M $186c M $186d M $7b5b M -> $e7 $5b $7b $21 $91 $c0 $9d }T
$afb7 >PC $14 >S $82 >A $df >X $16 >Y $22 >P $00bf $c4 >M $00c0 $ea >M $00e0 $5b >M $afb7 $21 >M $afb8 $e0 >M $afb9 $16 >M $eac4 $35 >M
T{ op PC S A X Y P -> $afb9 $14 $00 $df $16 $22 }T T{ $00bf M $00c0 M $00e0 M $afb7 M $afb8 M $afb9 M $eac4 M -> $c4 $ea $5b $21 $e0 $16 $35 }T
$c55e >PC $7e >S $d1 >A $eb >X $06 >Y $a3 >P $00ac $3a >M $00ad $89 >M $00c1 $87 >M $893a $b5 >M $c55e $21 >M $c55f $c1 >M $c560 $ad >M
T{ op PC S A X Y P -> $c560 $7e $91 $eb $06 $a1 }T T{ $00ac M $00ad M $00c1 M $893a M $c55e M $c55f M $c560 M -> $3a $89 $87 $b5 $21 $c1 $ad }T
$2bf7 >PC $e9 >S $b9 >A $b0 >X $51 >Y $22 >P $0058 $09 >M $0059 $dc >M $00a8 $73 >M $2bf7 $21 >M $2bf8 $a8 >M $2bf9 $82 >M $dc09 $3d >M
T{ op PC S A X Y P -> $2bf9 $e9 $39 $b0 $51 $20 }T T{ $0058 M $0059 M $00a8 M $2bf7 M $2bf8 M $2bf9 M $dc09 M -> $09 $dc $73 $21 $a8 $82 $3d }T
$3fd9 >PC $8a >S $be >A $f6 >X $1f >Y $23 >P $0049 $47 >M $004a $0a >M $0053 $bb >M $0a47 $6a >M $3fd9 $21 >M $3fda $53 >M $3fdb $5a >M
T{ op PC S A X Y P -> $3fdb $8a $2a $f6 $1f $21 }T T{ $0049 M $004a M $0053 M $0a47 M $3fd9 M $3fda M $3fdb M -> $47 $0a $bb $6a $21 $53 $5a }T
$9744 >PC $fc >S $57 >A $18 >X $3b >Y $a6 >P $00b1 $ff >M $00c9 $42 >M $00ca $b8 >M $9744 $21 >M $9745 $b1 >M $9746 $6f >M $b842 $0a >M
T{ op PC S A X Y P -> $9746 $fc $02 $18 $3b $24 }T T{ $00b1 M $00c9 M $00ca M $9744 M $9745 M $9746 M $b842 M -> $ff $42 $b8 $21 $b1 $6f $0a }T
$0efc >PC $6e >S $19 >A $73 >X $69 >Y $66 >P $0032 $61 >M $0033 $0a >M $00bf $38 >M $0a61 $96 >M $0efc $21 >M $0efd $bf >M $0efe $e2 >M
T{ op PC S A X Y P -> $0efe $6e $10 $73 $69 $64 }T T{ $0032 M $0033 M $00bf M $0a61 M $0efc M $0efd M $0efe M -> $61 $0a $38 $96 $21 $bf $e2 }T
$216e >PC $f1 >S $1c >A $78 >X $56 >Y $a6 >P $0071 $4d >M $00e9 $04 >M $00ea $d9 >M $216e $21 >M $216f $71 >M $2170 $f9 >M $d904 $3b >M
T{ op PC S A X Y P -> $2170 $f1 $18 $78 $56 $24 }T T{ $0071 M $00e9 M $00ea M $216e M $216f M $2170 M $d904 M -> $4d $04 $d9 $21 $71 $f9 $3b }T
$98f1 >PC $b1 >S $5d >A $39 >X $f6 >Y $20 >P $0020 $4e >M $0059 $3c >M $005a $97 >M $973c $03 >M $98f1 $21 >M $98f2 $20 >M $98f3 $72 >M
T{ op PC S A X Y P -> $98f3 $b1 $01 $39 $f6 $20 }T T{ $0020 M $0059 M $005a M $973c M $98f1 M $98f2 M $98f3 M -> $4e $3c $97 $03 $21 $20 $72 }T
$46b4 >PC $23 >S $42 >A $6d >X $c6 >Y $a7 >P $005d $12 >M $005e $68 >M $00f0 $66 >M $46b4 $21 >M $46b5 $f0 >M $46b6 $b8 >M $6812 $cc >M
T{ op PC S A X Y P -> $46b6 $23 $40 $6d $c6 $25 }T T{ $005d M $005e M $00f0 M $46b4 M $46b5 M $46b6 M $6812 M -> $12 $68 $66 $21 $f0 $b8 $cc }T
$7aa4 >PC $1f >S $95 >A $c0 >X $ee >Y $e2 >P $0062 $92 >M $0063 $8e >M $00a2 $1e >M $7aa4 $21 >M $7aa5 $a2 >M $7aa6 $f6 >M $8e92 $12 >M
T{ op PC S A X Y P -> $7aa6 $1f $10 $c0 $ee $60 }T T{ $0062 M $0063 M $00a2 M $7aa4 M $7aa5 M $7aa6 M $8e92 M -> $92 $8e $1e $21 $a2 $f6 $12 }T
$2be8 >PC $10 >S $f3 >A $8c >X $48 >Y $61 >P $0037 $0d >M $00c3 $b6 >M $00c4 $67 >M $2be8 $21 >M $2be9 $37 >M $2bea $79 >M $67b6 $c3 >M
T{ op PC S A X Y P -> $2bea $10 $c3 $8c $48 $e1 }T T{ $0037 M $00c3 M $00c4 M $2be8 M $2be9 M $2bea M $67b6 M -> $0d $b6 $67 $21 $37 $79 $c3 }T
$6e11 >PC $fa >S $de >A $4b >X $44 >Y $a4 >P $0049 $44 >M $004a $f0 >M $00fe $1b >M $6e11 $21 >M $6e12 $fe >M $6e13 $d8 >M $f044 $db >M
T{ op PC S A X Y P -> $6e13 $fa $da $4b $44 $a4 }T T{ $0049 M $004a M $00fe M $6e11 M $6e12 M $6e13 M $f044 M -> $44 $f0 $1b $21 $fe $d8 $db }T
( 22 )
$7bdf >PC $8f >S $6c >A $ee >X $19 >Y $27 >P $7bdf $22 >M $7be0 $85 >M $7be1 $1c >M
T{ op PC S A X Y P -> $7be1 $8f $6c $ee $19 $27 }T T{ $7bdf M $7be0 M $7be1 M -> $22 $85 $1c }T
$955d >PC $98 >S $87 >A $9b >X $b6 >Y $e7 >P $955d $22 >M $955e $96 >M $955f $2f >M
T{ op PC S A X Y P -> $955f $98 $87 $9b $b6 $e7 }T T{ $955d M $955e M $955f M -> $22 $96 $2f }T
$39eb >PC $4d >S $d0 >A $35 >X $e6 >Y $62 >P $39eb $22 >M $39ec $e3 >M $39ed $84 >M
T{ op PC S A X Y P -> $39ed $4d $d0 $35 $e6 $62 }T T{ $39eb M $39ec M $39ed M -> $22 $e3 $84 }T
$558d >PC $e8 >S $4f >A $7e >X $32 >Y $e6 >P $558d $22 >M $558e $67 >M $558f $ad >M
T{ op PC S A X Y P -> $558f $e8 $4f $7e $32 $e6 }T T{ $558d M $558e M $558f M -> $22 $67 $ad }T
$64f9 >PC $1c >S $6d >A $bf >X $d0 >Y $e6 >P $64f9 $22 >M $64fa $d5 >M $64fb $c5 >M
T{ op PC S A X Y P -> $64fb $1c $6d $bf $d0 $e6 }T T{ $64f9 M $64fa M $64fb M -> $22 $d5 $c5 }T
$c2d4 >PC $04 >S $9c >A $b9 >X $3b >Y $e7 >P $c2d4 $22 >M $c2d5 $66 >M $c2d6 $ca >M
T{ op PC S A X Y P -> $c2d6 $04 $9c $b9 $3b $e7 }T T{ $c2d4 M $c2d5 M $c2d6 M -> $22 $66 $ca }T
$a022 >PC $9f >S $cb >A $16 >X $5c >Y $e6 >P $a022 $22 >M $a023 $68 >M $a024 $51 >M
T{ op PC S A X Y P -> $a024 $9f $cb $16 $5c $e6 }T T{ $a022 M $a023 M $a024 M -> $22 $68 $51 }T
$85aa >PC $f7 >S $81 >A $87 >X $80 >Y $a5 >P $85aa $22 >M $85ab $b6 >M $85ac $5f >M
T{ op PC S A X Y P -> $85ac $f7 $81 $87 $80 $a5 }T T{ $85aa M $85ab M $85ac M -> $22 $b6 $5f }T
$34d0 >PC $3c >S $91 >A $76 >X $84 >Y $62 >P $34d0 $22 >M $34d1 $3a >M $34d2 $a2 >M
T{ op PC S A X Y P -> $34d2 $3c $91 $76 $84 $62 }T T{ $34d0 M $34d1 M $34d2 M -> $22 $3a $a2 }T
$f2e9 >PC $46 >S $bb >A $4b >X $6a >Y $62 >P $f2e9 $22 >M $f2ea $1a >M $f2eb $d7 >M
T{ op PC S A X Y P -> $f2eb $46 $bb $4b $6a $62 }T T{ $f2e9 M $f2ea M $f2eb M -> $22 $1a $d7 }T
$3b8e >PC $15 >S $2b >A $27 >X $38 >Y $62 >P $3b8e $22 >M $3b8f $a9 >M $3b90 $82 >M
T{ op PC S A X Y P -> $3b90 $15 $2b $27 $38 $62 }T T{ $3b8e M $3b8f M $3b90 M -> $22 $a9 $82 }T
$1b3f >PC $6d >S $d8 >A $43 >X $47 >Y $e7 >P $1b3f $22 >M $1b40 $b0 >M $1b41 $c0 >M
T{ op PC S A X Y P -> $1b41 $6d $d8 $43 $47 $e7 }T T{ $1b3f M $1b40 M $1b41 M -> $22 $b0 $c0 }T
$999a >PC $ce >S $e6 >A $9e >X $04 >Y $a2 >P $999a $22 >M $999b $03 >M $999c $25 >M
T{ op PC S A X Y P -> $999c $ce $e6 $9e $04 $a2 }T T{ $999a M $999b M $999c M -> $22 $03 $25 }T
$e429 >PC $82 >S $05 >A $85 >X $25 >Y $27 >P $e429 $22 >M $e42a $6d >M $e42b $52 >M
T{ op PC S A X Y P -> $e42b $82 $05 $85 $25 $27 }T T{ $e429 M $e42a M $e42b M -> $22 $6d $52 }T
$baea >PC $80 >S $26 >A $9c >X $f4 >Y $a5 >P $baea $22 >M $baeb $46 >M $baec $49 >M
T{ op PC S A X Y P -> $baec $80 $26 $9c $f4 $a5 }T T{ $baea M $baeb M $baec M -> $22 $46 $49 }T
$32e7 >PC $63 >S $44 >A $46 >X $d7 >Y $61 >P $32e7 $22 >M $32e8 $de >M $32e9 $ce >M
T{ op PC S A X Y P -> $32e9 $63 $44 $46 $d7 $61 }T T{ $32e7 M $32e8 M $32e9 M -> $22 $de $ce }T
( 23 )
$5cce >PC $da >S $9f >A $da >X $b6 >Y $60 >P $5cce $23 >M $5ccf $3a >M $5cd0 $af >M
T{ op PC S A X Y P -> $5ccf $da $9f $da $b6 $60 }T T{ $5cce M $5ccf M $5cd0 M -> $23 $3a $af }T
$f670 >PC $40 >S $0c >A $ff >X $30 >Y $60 >P $f670 $23 >M $f671 $e1 >M $f672 $07 >M
T{ op PC S A X Y P -> $f671 $40 $0c $ff $30 $60 }T T{ $f670 M $f671 M $f672 M -> $23 $e1 $07 }T
$9d5e >PC $22 >S $b4 >A $78 >X $d2 >Y $e3 >P $9d5e $23 >M $9d5f $92 >M $9d60 $f2 >M
T{ op PC S A X Y P -> $9d5f $22 $b4 $78 $d2 $e3 }T T{ $9d5e M $9d5f M $9d60 M -> $23 $92 $f2 }T
$9633 >PC $63 >S $9c >A $0b >X $6f >Y $e3 >P $9633 $23 >M $9634 $f5 >M $9635 $f3 >M
T{ op PC S A X Y P -> $9634 $63 $9c $0b $6f $e3 }T T{ $9633 M $9634 M $9635 M -> $23 $f5 $f3 }T
$65a4 >PC $ba >S $7e >A $bc >X $c5 >Y $a5 >P $65a4 $23 >M $65a5 $b2 >M $65a6 $2c >M
T{ op PC S A X Y P -> $65a5 $ba $7e $bc $c5 $a5 }T T{ $65a4 M $65a5 M $65a6 M -> $23 $b2 $2c }T
$9f89 >PC $50 >S $b9 >A $ce >X $b0 >Y $a3 >P $9f89 $23 >M $9f8a $75 >M $9f8b $e0 >M
T{ op PC S A X Y P -> $9f8a $50 $b9 $ce $b0 $a3 }T T{ $9f89 M $9f8a M $9f8b M -> $23 $75 $e0 }T
$d923 >PC $89 >S $e9 >A $1b >X $3a >Y $62 >P $d923 $23 >M $d924 $8d >M $d925 $2c >M
T{ op PC S A X Y P -> $d924 $89 $e9 $1b $3a $62 }T T{ $d923 M $d924 M $d925 M -> $23 $8d $2c }T
$dcdc >PC $dc >S $b0 >A $0c >X $ea >Y $27 >P $dcdc $23 >M $dcdd $f3 >M $dcde $51 >M
T{ op PC S A X Y P -> $dcdd $dc $b0 $0c $ea $27 }T T{ $dcdc M $dcdd M $dcde M -> $23 $f3 $51 }T
$ced6 >PC $51 >S $9e >A $3a >X $f6 >Y $26 >P $ced6 $23 >M $ced7 $fb >M $ced8 $b4 >M
T{ op PC S A X Y P -> $ced7 $51 $9e $3a $f6 $26 }T T{ $ced6 M $ced7 M $ced8 M -> $23 $fb $b4 }T
$7d7b >PC $28 >S $78 >A $0e >X $1b >Y $e3 >P $7d7b $23 >M $7d7c $a4 >M $7d7d $de >M
T{ op PC S A X Y P -> $7d7c $28 $78 $0e $1b $e3 }T T{ $7d7b M $7d7c M $7d7d M -> $23 $a4 $de }T
$9d05 >PC $4e >S $50 >A $af >X $0d >Y $a6 >P $9d05 $23 >M $9d06 $80 >M $9d07 $92 >M
T{ op PC S A X Y P -> $9d06 $4e $50 $af $0d $a6 }T T{ $9d05 M $9d06 M $9d07 M -> $23 $80 $92 }T
$56a4 >PC $fa >S $64 >A $3e >X $d6 >Y $67 >P $56a4 $23 >M $56a5 $1f >M $56a6 $e5 >M
T{ op PC S A X Y P -> $56a5 $fa $64 $3e $d6 $67 }T T{ $56a4 M $56a5 M $56a6 M -> $23 $1f $e5 }T
$9c22 >PC $d5 >S $b9 >A $f9 >X $e8 >Y $66 >P $9c22 $23 >M $9c23 $5e >M $9c24 $6c >M
T{ op PC S A X Y P -> $9c23 $d5 $b9 $f9 $e8 $66 }T T{ $9c22 M $9c23 M $9c24 M -> $23 $5e $6c }T
$e45c >PC $5d >S $36 >A $de >X $d4 >Y $27 >P $e45c $23 >M $e45d $f4 >M $e45e $72 >M
T{ op PC S A X Y P -> $e45d $5d $36 $de $d4 $27 }T T{ $e45c M $e45d M $e45e M -> $23 $f4 $72 }T
$cc39 >PC $27 >S $cb >A $40 >X $0a >Y $64 >P $cc39 $23 >M $cc3a $15 >M $cc3b $45 >M
T{ op PC S A X Y P -> $cc3a $27 $cb $40 $0a $64 }T T{ $cc39 M $cc3a M $cc3b M -> $23 $15 $45 }T
$9dec >PC $7b >S $8f >A $2d >X $03 >Y $65 >P $9dec $23 >M $9ded $32 >M $9dee $ea >M
T{ op PC S A X Y P -> $9ded $7b $8f $2d $03 $65 }T T{ $9dec M $9ded M $9dee M -> $23 $32 $ea }T
( 24 )
$4bef >PC $7e >S $7a >A $95 >X $5e >Y $a1 >P $0053 $9a >M $4bef $24 >M $4bf0 $53 >M $4bf1 $72 >M
T{ op PC S A X Y P -> $4bf1 $7e $7a $95 $5e $a1 }T T{ $0053 M $4bef M $4bf0 M $4bf1 M -> $9a $24 $53 $72 }T
$7fa2 >PC $91 >S $11 >A $4e >X $34 >Y $66 >P $00ea $20 >M $7fa2 $24 >M $7fa3 $ea >M $7fa4 $3a >M
T{ op PC S A X Y P -> $7fa4 $91 $11 $4e $34 $26 }T T{ $00ea M $7fa2 M $7fa3 M $7fa4 M -> $20 $24 $ea $3a }T
$586c >PC $b0 >S $46 >A $fa >X $e1 >Y $e6 >P $009b $b3 >M $586c $24 >M $586d $9b >M $586e $9c >M
T{ op PC S A X Y P -> $586e $b0 $46 $fa $e1 $a4 }T T{ $009b M $586c M $586d M $586e M -> $b3 $24 $9b $9c }T
$bdab >PC $93 >S $50 >A $6a >X $00 >Y $63 >P $0033 $64 >M $bdab $24 >M $bdac $33 >M $bdad $4b >M
T{ op PC S A X Y P -> $bdad $93 $50 $6a $00 $61 }T T{ $0033 M $bdab M $bdac M $bdad M -> $64 $24 $33 $4b }T
$811c >PC $9e >S $84 >A $e4 >X $d5 >Y $67 >P $00dd $5d >M $811c $24 >M $811d $dd >M $811e $d7 >M
T{ op PC S A X Y P -> $811e $9e $84 $e4 $d5 $65 }T T{ $00dd M $811c M $811d M $811e M -> $5d $24 $dd $d7 }T
$0384 >PC $46 >S $4d >A $8a >X $d7 >Y $e7 >P $009c $28 >M $0384 $24 >M $0385 $9c >M $0386 $00 >M
T{ op PC S A X Y P -> $0386 $46 $4d $8a $d7 $25 }T T{ $009c M $0384 M $0385 M $0386 M -> $28 $24 $9c $00 }T
$5f9c >PC $92 >S $f9 >A $32 >X $a1 >Y $20 >P $0085 $43 >M $5f9c $24 >M $5f9d $85 >M $5f9e $55 >M
T{ op PC S A X Y P -> $5f9e $92 $f9 $32 $a1 $60 }T T{ $0085 M $5f9c M $5f9d M $5f9e M -> $43 $24 $85 $55 }T
$89fd >PC $50 >S $ea >A $16 >X $09 >Y $27 >P $00e5 $0c >M $89fd $24 >M $89fe $e5 >M $89ff $c5 >M
T{ op PC S A X Y P -> $89ff $50 $ea $16 $09 $25 }T T{ $00e5 M $89fd M $89fe M $89ff M -> $0c $24 $e5 $c5 }T
$bd9b >PC $19 >S $32 >A $ae >X $5b >Y $25 >P $0017 $f2 >M $bd9b $24 >M $bd9c $17 >M $bd9d $31 >M
T{ op PC S A X Y P -> $bd9d $19 $32 $ae $5b $e5 }T T{ $0017 M $bd9b M $bd9c M $bd9d M -> $f2 $24 $17 $31 }T
$4df1 >PC $f4 >S $26 >A $68 >X $47 >Y $21 >P $00ae $a3 >M $4df1 $24 >M $4df2 $ae >M $4df3 $6a >M
T{ op PC S A X Y P -> $4df3 $f4 $26 $68 $47 $a1 }T T{ $00ae M $4df1 M $4df2 M $4df3 M -> $a3 $24 $ae $6a }T
$33dc >PC $c1 >S $1f >A $18 >X $e5 >Y $23 >P $0024 $94 >M $33dc $24 >M $33dd $24 >M $33de $74 >M
T{ op PC S A X Y P -> $33de $c1 $1f $18 $e5 $a1 }T T{ $0024 M $33dc M $33dd M $33de M -> $94 $24 $24 $74 }T
$0fbd >PC $8e >S $cf >A $d7 >X $1e >Y $a4 >P $007a $48 >M $0fbd $24 >M $0fbe $7a >M $0fbf $49 >M
T{ op PC S A X Y P -> $0fbf $8e $cf $d7 $1e $64 }T T{ $007a M $0fbd M $0fbe M $0fbf M -> $48 $24 $7a $49 }T
$7579 >PC $f6 >S $3e >A $fc >X $27 >Y $23 >P $00b7 $d8 >M $7579 $24 >M $757a $b7 >M $757b $63 >M
T{ op PC S A X Y P -> $757b $f6 $3e $fc $27 $e1 }T T{ $00b7 M $7579 M $757a M $757b M -> $d8 $24 $b7 $63 }T
$d0dd >PC $5e >S $f8 >A $41 >X $8b >Y $67 >P $003d $67 >M $d0dd $24 >M $d0de $3d >M $d0df $86 >M
T{ op PC S A X Y P -> $d0df $5e $f8 $41 $8b $65 }T T{ $003d M $d0dd M $d0de M $d0df M -> $67 $24 $3d $86 }T
$4cc1 >PC $0d >S $9f >A $b4 >X $2a >Y $a0 >P $000a $05 >M $4cc1 $24 >M $4cc2 $0a >M $4cc3 $93 >M
T{ op PC S A X Y P -> $4cc3 $0d $9f $b4 $2a $20 }T T{ $000a M $4cc1 M $4cc2 M $4cc3 M -> $05 $24 $0a $93 }T
$6f3c >PC $9d >S $fe >A $f1 >X $b2 >Y $a6 >P $00ce $eb >M $6f3c $24 >M $6f3d $ce >M $6f3e $12 >M
T{ op PC S A X Y P -> $6f3e $9d $fe $f1 $b2 $e4 }T T{ $00ce M $6f3c M $6f3d M $6f3e M -> $eb $24 $ce $12 }T
( 25 )
$07be >PC $74 >S $c6 >A $4c >X $1e >Y $e4 >P $00a5 $2b >M $07be $25 >M $07bf $a5 >M $07c0 $38 >M
T{ op PC S A X Y P -> $07c0 $74 $02 $4c $1e $64 }T T{ $00a5 M $07be M $07bf M $07c0 M -> $2b $25 $a5 $38 }T
$5c96 >PC $47 >S $4e >A $58 >X $cf >Y $60 >P $00ae $cd >M $5c96 $25 >M $5c97 $ae >M $5c98 $6c >M
T{ op PC S A X Y P -> $5c98 $47 $4c $58 $cf $60 }T T{ $00ae M $5c96 M $5c97 M $5c98 M -> $cd $25 $ae $6c }T
$e3d2 >PC $7a >S $0a >A $4b >X $8a >Y $27 >P $0095 $a8 >M $e3d2 $25 >M $e3d3 $95 >M $e3d4 $ee >M
T{ op PC S A X Y P -> $e3d4 $7a $08 $4b $8a $25 }T T{ $0095 M $e3d2 M $e3d3 M $e3d4 M -> $a8 $25 $95 $ee }T
$9b1b >PC $93 >S $b9 >A $11 >X $55 >Y $e1 >P $0018 $b6 >M $9b1b $25 >M $9b1c $18 >M $9b1d $7f >M
T{ op PC S A X Y P -> $9b1d $93 $b0 $11 $55 $e1 }T T{ $0018 M $9b1b M $9b1c M $9b1d M -> $b6 $25 $18 $7f }T
$f8d3 >PC $0c >S $7a >A $93 >X $50 >Y $63 >P $00f5 $a3 >M $f8d3 $25 >M $f8d4 $f5 >M $f8d5 $cb >M
T{ op PC S A X Y P -> $f8d5 $0c $22 $93 $50 $61 }T T{ $00f5 M $f8d3 M $f8d4 M $f8d5 M -> $a3 $25 $f5 $cb }T
$18b1 >PC $43 >S $af >A $59 >X $a6 >Y $62 >P $0049 $aa >M $18b1 $25 >M $18b2 $49 >M $18b3 $d3 >M
T{ op PC S A X Y P -> $18b3 $43 $aa $59 $a6 $e0 }T T{ $0049 M $18b1 M $18b2 M $18b3 M -> $aa $25 $49 $d3 }T
$9b99 >PC $2e >S $3f >A $e9 >X $99 >Y $67 >P $0095 $e5 >M $9b99 $25 >M $9b9a $95 >M $9b9b $84 >M
T{ op PC S A X Y P -> $9b9b $2e $25 $e9 $99 $65 }T T{ $0095 M $9b99 M $9b9a M $9b9b M -> $e5 $25 $95 $84 }T
$f1f6 >PC $cd >S $0a >A $60 >X $5f >Y $20 >P $0068 $58 >M $f1f6 $25 >M $f1f7 $68 >M $f1f8 $69 >M
T{ op PC S A X Y P -> $f1f8 $cd $08 $60 $5f $20 }T T{ $0068 M $f1f6 M $f1f7 M $f1f8 M -> $58 $25 $68 $69 }T
$e889 >PC $0f >S $35 >A $0b >X $81 >Y $62 >P $0073 $8b >M $e889 $25 >M $e88a $73 >M $e88b $72 >M
T{ op PC S A X Y P -> $e88b $0f $01 $0b $81 $60 }T T{ $0073 M $e889 M $e88a M $e88b M -> $8b $25 $73 $72 }T
$e1b5 >PC $4c >S $69 >A $f4 >X $21 >Y $67 >P $009b $27 >M $e1b5 $25 >M $e1b6 $9b >M $e1b7 $a6 >M
T{ op PC S A X Y P -> $e1b7 $4c $21 $f4 $21 $65 }T T{ $009b M $e1b5 M $e1b6 M $e1b7 M -> $27 $25 $9b $a6 }T
$e117 >PC $9a >S $66 >A $2a >X $e1 >Y $a4 >P $008e $83 >M $e117 $25 >M $e118 $8e >M $e119 $97 >M
T{ op PC S A X Y P -> $e119 $9a $02 $2a $e1 $24 }T T{ $008e M $e117 M $e118 M $e119 M -> $83 $25 $8e $97 }T
$8885 >PC $27 >S $a6 >A $0c >X $ff >Y $61 >P $006c $50 >M $8885 $25 >M $8886 $6c >M $8887 $a1 >M
T{ op PC S A X Y P -> $8887 $27 $00 $0c $ff $63 }T T{ $006c M $8885 M $8886 M $8887 M -> $50 $25 $6c $a1 }T
$7904 >PC $4c >S $86 >A $03 >X $b1 >Y $22 >P $004f $75 >M $7904 $25 >M $7905 $4f >M $7906 $ff >M
T{ op PC S A X Y P -> $7906 $4c $04 $03 $b1 $20 }T T{ $004f M $7904 M $7905 M $7906 M -> $75 $25 $4f $ff }T
$3aed >PC $83 >S $ea >A $9b >X $30 >Y $66 >P $0025 $82 >M $3aed $25 >M $3aee $25 >M $3aef $cb >M
T{ op PC S A X Y P -> $3aef $83 $82 $9b $30 $e4 }T T{ $0025 M $3aed M $3aee M $3aef M -> $82 $25 $25 $cb }T
$f049 >PC $0d >S $b1 >A $d1 >X $91 >Y $63 >P $00b9 $0f >M $f049 $25 >M $f04a $b9 >M $f04b $c2 >M
T{ op PC S A X Y P -> $f04b $0d $01 $d1 $91 $61 }T T{ $00b9 M $f049 M $f04a M $f04b M -> $0f $25 $b9 $c2 }T
$517c >PC $ba >S $a0 >A $29 >X $46 >Y $27 >P $0046 $5d >M $517c $25 >M $517d $46 >M $517e $4a >M
T{ op PC S A X Y P -> $517e $ba $00 $29 $46 $27 }T T{ $0046 M $517c M $517d M $517e M -> $5d $25 $46 $4a }T
( 26 )
$5e86 >PC $ad >S $f1 >A $1e >X $56 >Y $a7 >P $0019 $5b >M $5e86 $26 >M $5e87 $19 >M $5e88 $9d >M
T{ op PC S A X Y P -> $5e88 $ad $f1 $1e $56 $a4 }T T{ $0019 M $5e86 M $5e87 M $5e88 M -> $b7 $26 $19 $9d }T
$0768 >PC $85 >S $d7 >A $fc >X $01 >Y $e2 >P $0057 $82 >M $0768 $26 >M $0769 $57 >M $076a $cc >M
T{ op PC S A X Y P -> $076a $85 $d7 $fc $01 $61 }T T{ $0057 M $0768 M $0769 M $076a M -> $04 $26 $57 $cc }T
$dcf9 >PC $da >S $a2 >A $08 >X $36 >Y $60 >P $0016 $69 >M $dcf9 $26 >M $dcfa $16 >M $dcfb $87 >M
T{ op PC S A X Y P -> $dcfb $da $a2 $08 $36 $e0 }T T{ $0016 M $dcf9 M $dcfa M $dcfb M -> $d2 $26 $16 $87 }T
$f775 >PC $8a >S $c9 >A $3d >X $f6 >Y $e4 >P $0060 $59 >M $f775 $26 >M $f776 $60 >M $f777 $31 >M
T{ op PC S A X Y P -> $f777 $8a $c9 $3d $f6 $e4 }T T{ $0060 M $f775 M $f776 M $f777 M -> $b2 $26 $60 $31 }T
$6056 >PC $14 >S $d4 >A $a8 >X $f4 >Y $63 >P $0022 $02 >M $6056 $26 >M $6057 $22 >M $6058 $00 >M
T{ op PC S A X Y P -> $6058 $14 $d4 $a8 $f4 $60 }T T{ $0022 M $6056 M $6057 M $6058 M -> $05 $26 $22 $00 }T
$8c5d >PC $a1 >S $70 >A $f1 >X $c0 >Y $64 >P $001e $63 >M $8c5d $26 >M $8c5e $1e >M $8c5f $3b >M
T{ op PC S A X Y P -> $8c5f $a1 $70 $f1 $c0 $e4 }T T{ $001e M $8c5d M $8c5e M $8c5f M -> $c6 $26 $1e $3b }T
$feef >PC $ff >S $1e >A $72 >X $b5 >Y $62 >P $005c $34 >M $feef $26 >M $fef0 $5c >M $fef1 $6c >M
T{ op PC S A X Y P -> $fef1 $ff $1e $72 $b5 $60 }T T{ $005c M $feef M $fef0 M $fef1 M -> $68 $26 $5c $6c }T
$df7f >PC $3a >S $88 >A $67 >X $da >Y $20 >P $00f7 $95 >M $df7f $26 >M $df80 $f7 >M $df81 $08 >M
T{ op PC S A X Y P -> $df81 $3a $88 $67 $da $21 }T T{ $00f7 M $df7f M $df80 M $df81 M -> $2a $26 $f7 $08 }T
$0418 >PC $c2 >S $b1 >A $62 >X $f0 >Y $63 >P $006a $44 >M $0418 $26 >M $0419 $6a >M $041a $7e >M
T{ op PC S A X Y P -> $041a $c2 $b1 $62 $f0 $e0 }T T{ $006a M $0418 M $0419 M $041a M -> $89 $26 $6a $7e }T
$bd92 >PC $f3 >S $c0 >A $14 >X $62 >Y $67 >P $00e9 $74 >M $bd92 $26 >M $bd93 $e9 >M $bd94 $9b >M
T{ op PC S A X Y P -> $bd94 $f3 $c0 $14 $62 $e4 }T T{ $00e9 M $bd92 M $bd93 M $bd94 M -> $e9 $26 $e9 $9b }T
$d6b7 >PC $74 >S $ee >A $82 >X $53 >Y $a7 >P $007e $0e >M $d6b7 $26 >M $d6b8 $7e >M $d6b9 $ea >M
T{ op PC S A X Y P -> $d6b9 $74 $ee $82 $53 $24 }T T{ $007e M $d6b7 M $d6b8 M $d6b9 M -> $1d $26 $7e $ea }T
$6488 >PC $9a >S $d0 >A $79 >X $03 >Y $e7 >P $00a0 $50 >M $6488 $26 >M $6489 $a0 >M $648a $16 >M
T{ op PC S A X Y P -> $648a $9a $d0 $79 $03 $e4 }T T{ $00a0 M $6488 M $6489 M $648a M -> $a1 $26 $a0 $16 }T
$3c2d >PC $c8 >S $23 >A $11 >X $c2 >Y $23 >P $006a $0d >M $3c2d $26 >M $3c2e $6a >M $3c2f $37 >M
T{ op PC S A X Y P -> $3c2f $c8 $23 $11 $c2 $20 }T T{ $006a M $3c2d M $3c2e M $3c2f M -> $1b $26 $6a $37 }T
$3b21 >PC $3c >S $d0 >A $8e >X $60 >Y $e3 >P $0067 $be >M $3b21 $26 >M $3b22 $67 >M $3b23 $43 >M
T{ op PC S A X Y P -> $3b23 $3c $d0 $8e $60 $61 }T T{ $0067 M $3b21 M $3b22 M $3b23 M -> $7d $26 $67 $43 }T
$f25a >PC $61 >S $cb >A $09 >X $9d >Y $a0 >P $001b $8a >M $f25a $26 >M $f25b $1b >M $f25c $a8 >M
T{ op PC S A X Y P -> $f25c $61 $cb $09 $9d $21 }T T{ $001b M $f25a M $f25b M $f25c M -> $14 $26 $1b $a8 }T
$42a1 >PC $1d >S $1a >A $6b >X $d7 >Y $64 >P $0040 $18 >M $42a1 $26 >M $42a2 $40 >M $42a3 $f8 >M
T{ op PC S A X Y P -> $42a3 $1d $1a $6b $d7 $64 }T T{ $0040 M $42a1 M $42a2 M $42a3 M -> $30 $26 $40 $f8 }T
( 27 )
$c610 >PC $29 >S $38 >A $10 >X $dc >Y $e5 >P $005f $da >M $c610 $27 >M $c611 $5f >M $c612 $ee >M
T{ op PC S A X Y P -> $c612 $29 $38 $10 $dc $e5 }T T{ $005f M $c610 M $c611 M $c612 M -> $da $27 $5f $ee }T
$c865 >PC $a1 >S $e9 >A $da >X $c0 >Y $25 >P $0034 $d4 >M $c865 $27 >M $c866 $34 >M $c867 $a1 >M
T{ op PC S A X Y P -> $c867 $a1 $e9 $da $c0 $25 }T T{ $0034 M $c865 M $c866 M $c867 M -> $d0 $27 $34 $a1 }T
$a253 >PC $4e >S $f7 >A $4e >X $45 >Y $a5 >P $00e8 $31 >M $a253 $27 >M $a254 $e8 >M $a255 $7d >M
T{ op PC S A X Y P -> $a255 $4e $f7 $4e $45 $a5 }T T{ $00e8 M $a253 M $a254 M $a255 M -> $31 $27 $e8 $7d }T
$4544 >PC $2a >S $ba >A $19 >X $0c >Y $e1 >P $00ac $17 >M $4544 $27 >M $4545 $ac >M $4546 $d4 >M
T{ op PC S A X Y P -> $4546 $2a $ba $19 $0c $e1 }T T{ $00ac M $4544 M $4545 M $4546 M -> $13 $27 $ac $d4 }T
$32fa >PC $50 >S $4c >A $e5 >X $04 >Y $66 >P $0085 $65 >M $32fa $27 >M $32fb $85 >M $32fc $2a >M
T{ op PC S A X Y P -> $32fc $50 $4c $e5 $04 $66 }T T{ $0085 M $32fa M $32fb M $32fc M -> $61 $27 $85 $2a }T
$658e >PC $de >S $d1 >A $1a >X $41 >Y $23 >P $00c7 $6f >M $658e $27 >M $658f $c7 >M $6590 $8e >M
T{ op PC S A X Y P -> $6590 $de $d1 $1a $41 $23 }T T{ $00c7 M $658e M $658f M $6590 M -> $6b $27 $c7 $8e }T
$ceca >PC $97 >S $fd >A $89 >X $81 >Y $23 >P $0017 $8b >M $ceca $27 >M $cecb $17 >M $cecc $42 >M
T{ op PC S A X Y P -> $cecc $97 $fd $89 $81 $23 }T T{ $0017 M $ceca M $cecb M $cecc M -> $8b $27 $17 $42 }T
$f630 >PC $e8 >S $e4 >A $33 >X $59 >Y $a0 >P $0072 $71 >M $f630 $27 >M $f631 $72 >M $f632 $3f >M
T{ op PC S A X Y P -> $f632 $e8 $e4 $33 $59 $a0 }T T{ $0072 M $f630 M $f631 M $f632 M -> $71 $27 $72 $3f }T
$d9bb >PC $64 >S $8a >A $ac >X $45 >Y $65 >P $009d $da >M $d9bb $27 >M $d9bc $9d >M $d9bd $f7 >M
T{ op PC S A X Y P -> $d9bd $64 $8a $ac $45 $65 }T T{ $009d M $d9bb M $d9bc M $d9bd M -> $da $27 $9d $f7 }T
$3a44 >PC $7e >S $0d >A $00 >X $9e >Y $a2 >P $00c1 $e3 >M $3a44 $27 >M $3a45 $c1 >M $3a46 $bf >M
T{ op PC S A X Y P -> $3a46 $7e $0d $00 $9e $a2 }T T{ $00c1 M $3a44 M $3a45 M $3a46 M -> $e3 $27 $c1 $bf }T
$4197 >PC $e2 >S $bf >A $9c >X $97 >Y $26 >P $006a $dc >M $4197 $27 >M $4198 $6a >M $4199 $8a >M
T{ op PC S A X Y P -> $4199 $e2 $bf $9c $97 $26 }T T{ $006a M $4197 M $4198 M $4199 M -> $d8 $27 $6a $8a }T
$0d21 >PC $4c >S $ef >A $19 >X $6d >Y $60 >P $00f4 $93 >M $0d21 $27 >M $0d22 $f4 >M $0d23 $92 >M
T{ op PC S A X Y P -> $0d23 $4c $ef $19 $6d $60 }T T{ $00f4 M $0d21 M $0d22 M $0d23 M -> $93 $27 $f4 $92 }T
$feb0 >PC $d3 >S $2c >A $b1 >X $41 >Y $21 >P $0088 $4e >M $feb0 $27 >M $feb1 $88 >M $feb2 $1d >M
T{ op PC S A X Y P -> $feb2 $d3 $2c $b1 $41 $21 }T T{ $0088 M $feb0 M $feb1 M $feb2 M -> $4a $27 $88 $1d }T
$21e4 >PC $45 >S $1e >A $00 >X $d3 >Y $a4 >P $003e $9d >M $21e4 $27 >M $21e5 $3e >M $21e6 $23 >M
T{ op PC S A X Y P -> $21e6 $45 $1e $00 $d3 $a4 }T T{ $003e M $21e4 M $21e5 M $21e6 M -> $99 $27 $3e $23 }T
$0a17 >PC $5c >S $9b >A $f1 >X $b0 >Y $25 >P $00a3 $b0 >M $0a17 $27 >M $0a18 $a3 >M $0a19 $ed >M
T{ op PC S A X Y P -> $0a19 $5c $9b $f1 $b0 $25 }T T{ $00a3 M $0a17 M $0a18 M $0a19 M -> $b0 $27 $a3 $ed }T
$287e >PC $17 >S $a0 >A $90 >X $3e >Y $a7 >P $008e $d3 >M $287e $27 >M $287f $8e >M $2880 $d2 >M
T{ op PC S A X Y P -> $2880 $17 $a0 $90 $3e $a7 }T T{ $008e M $287e M $287f M $2880 M -> $d3 $27 $8e $d2 }T
( 28 )
$a98b >PC $a2 >S $ac >A $e0 >X $d4 >Y $65 >P $01a2 $fa >M $01a3 $ae >M $a98b $28 >M $a98c $72 >M $a98d $03 >M
T{ op PC S A X Y P -> $a98c $a3 $ac $e0 $d4 $ae }T T{ $01a2 M $01a3 M $a98b M $a98c M $a98d M -> $fa $ae $28 $72 $03 }T
$2792 >PC $ee >S $7e >A $ff >X $b4 >Y $20 >P $01ee $13 >M $01ef $a6 >M $2792 $28 >M $2793 $37 >M $2794 $cf >M
T{ op PC S A X Y P -> $2793 $ef $7e $ff $b4 $a6 }T T{ $01ee M $01ef M $2792 M $2793 M $2794 M -> $13 $a6 $28 $37 $cf }T
$447d >PC $16 >S $e4 >A $5b >X $46 >Y $61 >P $0116 $62 >M $0117 $0e >M $447d $28 >M $447e $2d >M $447f $c3 >M
T{ op PC S A X Y P -> $447e $17 $e4 $5b $46 $2e }T T{ $0116 M $0117 M $447d M $447e M $447f M -> $62 $0e $28 $2d $c3 }T
$796f >PC $78 >S $4e >A $70 >X $02 >Y $e0 >P $0178 $6f >M $0179 $aa >M $796f $28 >M $7970 $37 >M $7971 $90 >M
T{ op PC S A X Y P -> $7970 $79 $4e $70 $02 $aa }T T{ $0178 M $0179 M $796f M $7970 M $7971 M -> $6f $aa $28 $37 $90 }T
$ebcc >PC $b5 >S $22 >A $44 >X $d6 >Y $a7 >P $01b5 $61 >M $01b6 $e8 >M $ebcc $28 >M $ebcd $d5 >M $ebce $2f >M
T{ op PC S A X Y P -> $ebcd $b6 $22 $44 $d6 $e8 }T T{ $01b5 M $01b6 M $ebcc M $ebcd M $ebce M -> $61 $e8 $28 $d5 $2f }T
$a6c9 >PC $b8 >S $40 >A $08 >X $34 >Y $26 >P $01b8 $68 >M $01b9 $c9 >M $a6c9 $28 >M $a6ca $b2 >M $a6cb $b1 >M
T{ op PC S A X Y P -> $a6ca $b9 $40 $08 $34 $e9 }T T{ $01b8 M $01b9 M $a6c9 M $a6ca M $a6cb M -> $68 $c9 $28 $b2 $b1 }T
$f1e0 >PC $98 >S $9c >A $64 >X $dc >Y $a1 >P $0198 $82 >M $0199 $ab >M $f1e0 $28 >M $f1e1 $96 >M $f1e2 $ea >M
T{ op PC S A X Y P -> $f1e1 $99 $9c $64 $dc $ab }T T{ $0198 M $0199 M $f1e0 M $f1e1 M $f1e2 M -> $82 $ab $28 $96 $ea }T
$4a21 >PC $77 >S $c5 >A $73 >X $5c >Y $22 >P $0177 $7b >M $0178 $ee >M $4a21 $28 >M $4a22 $1c >M $4a23 $6d >M
T{ op PC S A X Y P -> $4a22 $78 $c5 $73 $5c $ee }T T{ $0177 M $0178 M $4a21 M $4a22 M $4a23 M -> $7b $ee $28 $1c $6d }T
$c3b1 >PC $15 >S $dc >A $01 >X $bb >Y $a2 >P $0115 $c0 >M $0116 $ff >M $c3b1 $28 >M $c3b2 $a7 >M $c3b3 $38 >M
T{ op PC S A X Y P -> $c3b2 $16 $dc $01 $bb $ef }T T{ $0115 M $0116 M $c3b1 M $c3b2 M $c3b3 M -> $c0 $ff $28 $a7 $38 }T
$0570 >PC $96 >S $21 >A $f9 >X $08 >Y $22 >P $0196 $39 >M $0197 $53 >M $0570 $28 >M $0571 $73 >M $0572 $89 >M
T{ op PC S A X Y P -> $0571 $97 $21 $f9 $08 $63 }T T{ $0196 M $0197 M $0570 M $0571 M $0572 M -> $39 $53 $28 $73 $89 }T
$8f55 >PC $f5 >S $0d >A $49 >X $94 >Y $63 >P $01f5 $a5 >M $01f6 $f9 >M $8f55 $28 >M $8f56 $6e >M $8f57 $58 >M
T{ op PC S A X Y P -> $8f56 $f6 $0d $49 $94 $e9 }T T{ $01f5 M $01f6 M $8f55 M $8f56 M $8f57 M -> $a5 $f9 $28 $6e $58 }T
$b04d >PC $8d >S $b6 >A $4d >X $3c >Y $e2 >P $018d $12 >M $018e $80 >M $b04d $28 >M $b04e $dd >M $b04f $e8 >M
T{ op PC S A X Y P -> $b04e $8e $b6 $4d $3c $a0 }T T{ $018d M $018e M $b04d M $b04e M $b04f M -> $12 $80 $28 $dd $e8 }T
$6f38 >PC $3d >S $5a >A $94 >X $b1 >Y $20 >P $013d $cc >M $013e $92 >M $6f38 $28 >M $6f39 $fa >M $6f3a $74 >M
T{ op PC S A X Y P -> $6f39 $3e $5a $94 $b1 $a2 }T T{ $013d M $013e M $6f38 M $6f39 M $6f3a M -> $cc $92 $28 $fa $74 }T
$a85a >PC $32 >S $69 >A $6d >X $a1 >Y $a0 >P $0132 $44 >M $0133 $8e >M $a85a $28 >M $a85b $a5 >M $a85c $b1 >M
T{ op PC S A X Y P -> $a85b $33 $69 $6d $a1 $ae }T T{ $0132 M $0133 M $a85a M $a85b M $a85c M -> $44 $8e $28 $a5 $b1 }T
$f8a4 >PC $07 >S $14 >A $a1 >X $bc >Y $e4 >P $0107 $ed >M $0108 $38 >M $f8a4 $28 >M $f8a5 $e7 >M $f8a6 $92 >M
T{ op PC S A X Y P -> $f8a5 $08 $14 $a1 $bc $28 }T T{ $0107 M $0108 M $f8a4 M $f8a5 M $f8a6 M -> $ed $38 $28 $e7 $92 }T
$d1e7 >PC $44 >S $22 >A $35 >X $4e >Y $67 >P $0144 $74 >M $0145 $4b >M $d1e7 $28 >M $d1e8 $27 >M $d1e9 $6e >M
T{ op PC S A X Y P -> $d1e8 $45 $22 $35 $4e $6b }T T{ $0144 M $0145 M $d1e7 M $d1e8 M $d1e9 M -> $74 $4b $28 $27 $6e }T
( 29 )
$3447 >PC $cf >S $3d >A $52 >X $58 >Y $a2 >P $3447 $29 >M $3448 $df >M $3449 $ce >M
T{ op PC S A X Y P -> $3449 $cf $1d $52 $58 $20 }T T{ $3447 M $3448 M $3449 M -> $29 $df $ce }T
$c3a8 >PC $72 >S $41 >A $2a >X $9c >Y $e2 >P $c3a8 $29 >M $c3a9 $53 >M $c3aa $e1 >M
T{ op PC S A X Y P -> $c3aa $72 $41 $2a $9c $60 }T T{ $c3a8 M $c3a9 M $c3aa M -> $29 $53 $e1 }T
$bba8 >PC $c7 >S $7d >A $9b >X $2e >Y $e6 >P $bba8 $29 >M $bba9 $2a >M $bbaa $e6 >M
T{ op PC S A X Y P -> $bbaa $c7 $28 $9b $2e $64 }T T{ $bba8 M $bba9 M $bbaa M -> $29 $2a $e6 }T
$336e >PC $26 >S $0b >A $01 >X $5a >Y $e0 >P $336e $29 >M $336f $65 >M $3370 $28 >M
T{ op PC S A X Y P -> $3370 $26 $01 $01 $5a $60 }T T{ $336e M $336f M $3370 M -> $29 $65 $28 }T
$e70e >PC $53 >S $9d >A $9a >X $e1 >Y $25 >P $e70e $29 >M $e70f $61 >M $e710 $dc >M
T{ op PC S A X Y P -> $e710 $53 $01 $9a $e1 $25 }T T{ $e70e M $e70f M $e710 M -> $29 $61 $dc }T
$5a76 >PC $65 >S $5e >A $60 >X $8b >Y $60 >P $5a76 $29 >M $5a77 $de >M $5a78 $0e >M
T{ op PC S A X Y P -> $5a78 $65 $5e $60 $8b $60 }T T{ $5a76 M $5a77 M $5a78 M -> $29 $de $0e }T
$362f >PC $b4 >S $5a >A $c7 >X $89 >Y $e5 >P $362f $29 >M $3630 $9f >M $3631 $88 >M
T{ op PC S A X Y P -> $3631 $b4 $1a $c7 $89 $65 }T T{ $362f M $3630 M $3631 M -> $29 $9f $88 }T
$39a6 >PC $91 >S $bf >A $7f >X $03 >Y $e6 >P $39a6 $29 >M $39a7 $3e >M $39a8 $44 >M
T{ op PC S A X Y P -> $39a8 $91 $3e $7f $03 $64 }T T{ $39a6 M $39a7 M $39a8 M -> $29 $3e $44 }T
$9135 >PC $e7 >S $2c >A $c6 >X $d4 >Y $a2 >P $9135 $29 >M $9136 $c3 >M $9137 $3a >M
T{ op PC S A X Y P -> $9137 $e7 $00 $c6 $d4 $22 }T T{ $9135 M $9136 M $9137 M -> $29 $c3 $3a }T
$e9d7 >PC $dd >S $18 >A $ab >X $9d >Y $67 >P $e9d7 $29 >M $e9d8 $87 >M $e9d9 $a8 >M
T{ op PC S A X Y P -> $e9d9 $dd $00 $ab $9d $67 }T T{ $e9d7 M $e9d8 M $e9d9 M -> $29 $87 $a8 }T
$c094 >PC $58 >S $bc >A $89 >X $5d >Y $e6 >P $c094 $29 >M $c095 $f3 >M $c096 $06 >M
T{ op PC S A X Y P -> $c096 $58 $b0 $89 $5d $e4 }T T{ $c094 M $c095 M $c096 M -> $29 $f3 $06 }T
$b363 >PC $7d >S $86 >A $3a >X $39 >Y $e4 >P $b363 $29 >M $b364 $c6 >M $b365 $4c >M
T{ op PC S A X Y P -> $b365 $7d $86 $3a $39 $e4 }T T{ $b363 M $b364 M $b365 M -> $29 $c6 $4c }T
$906a >PC $05 >S $78 >A $06 >X $ed >Y $67 >P $906a $29 >M $906b $b3 >M $906c $b8 >M
T{ op PC S A X Y P -> $906c $05 $30 $06 $ed $65 }T T{ $906a M $906b M $906c M -> $29 $b3 $b8 }T
$5570 >PC $82 >S $82 >A $79 >X $dd >Y $26 >P $5570 $29 >M $5571 $84 >M $5572 $5d >M
T{ op PC S A X Y P -> $5572 $82 $80 $79 $dd $a4 }T T{ $5570 M $5571 M $5572 M -> $29 $84 $5d }T
$c6b0 >PC $47 >S $d6 >A $b0 >X $33 >Y $65 >P $c6b0 $29 >M $c6b1 $9b >M $c6b2 $b1 >M
T{ op PC S A X Y P -> $c6b2 $47 $92 $b0 $33 $e5 }T T{ $c6b0 M $c6b1 M $c6b2 M -> $29 $9b $b1 }T
$9aa4 >PC $2a >S $5b >A $e1 >X $f0 >Y $e5 >P $9aa4 $29 >M $9aa5 $a0 >M $9aa6 $bf >M
T{ op PC S A X Y P -> $9aa6 $2a $00 $e1 $f0 $67 }T T{ $9aa4 M $9aa5 M $9aa6 M -> $29 $a0 $bf }T
( 2a )
$97f3 >PC $dd >S $c8 >A $3d >X $66 >Y $e7 >P $97f3 $2a >M $97f4 $42 >M $97f5 $31 >M
T{ op PC S A X Y P -> $97f4 $dd $91 $3d $66 $e5 }T T{ $97f3 M $97f4 M $97f5 M -> $2a $42 $31 }T
$7111 >PC $86 >S $13 >A $da >X $2f >Y $e2 >P $7111 $2a >M $7112 $a3 >M $7113 $cc >M
T{ op PC S A X Y P -> $7112 $86 $26 $da $2f $60 }T T{ $7111 M $7112 M $7113 M -> $2a $a3 $cc }T
$64fb >PC $c0 >S $66 >A $dc >X $11 >Y $a1 >P $64fb $2a >M $64fc $97 >M $64fd $60 >M
T{ op PC S A X Y P -> $64fc $c0 $cd $dc $11 $a0 }T T{ $64fb M $64fc M $64fd M -> $2a $97 $60 }T
$9541 >PC $2e >S $2e >A $2c >X $da >Y $27 >P $9541 $2a >M $9542 $6f >M $9543 $4d >M
T{ op PC S A X Y P -> $9542 $2e $5d $2c $da $24 }T T{ $9541 M $9542 M $9543 M -> $2a $6f $4d }T
$ac41 >PC $55 >S $25 >A $04 >X $75 >Y $62 >P $ac41 $2a >M $ac42 $6b >M $ac43 $ea >M
T{ op PC S A X Y P -> $ac42 $55 $4a $04 $75 $60 }T T{ $ac41 M $ac42 M $ac43 M -> $2a $6b $ea }T
$0e2c >PC $9b >S $6b >A $61 >X $b3 >Y $a4 >P $0e2c $2a >M $0e2d $a7 >M $0e2e $09 >M
T{ op PC S A X Y P -> $0e2d $9b $d6 $61 $b3 $a4 }T T{ $0e2c M $0e2d M $0e2e M -> $2a $a7 $09 }T
$23a5 >PC $15 >S $75 >A $f1 >X $f9 >Y $62 >P $23a5 $2a >M $23a6 $ef >M $23a7 $56 >M
T{ op PC S A X Y P -> $23a6 $15 $ea $f1 $f9 $e0 }T T{ $23a5 M $23a6 M $23a7 M -> $2a $ef $56 }T
$d785 >PC $66 >S $fb >A $cf >X $4e >Y $a2 >P $d785 $2a >M $d786 $ad >M $d787 $9b >M
T{ op PC S A X Y P -> $d786 $66 $f6 $cf $4e $a1 }T T{ $d785 M $d786 M $d787 M -> $2a $ad $9b }T
$4cf2 >PC $e8 >S $c5 >A $b7 >X $29 >Y $65 >P $4cf2 $2a >M $4cf3 $fa >M $4cf4 $02 >M
T{ op PC S A X Y P -> $4cf3 $e8 $8b $b7 $29 $e5 }T T{ $4cf2 M $4cf3 M $4cf4 M -> $2a $fa $02 }T
$abe7 >PC $8a >S $4d >A $b6 >X $5e >Y $a1 >P $abe7 $2a >M $abe8 $e5 >M $abe9 $7a >M
T{ op PC S A X Y P -> $abe8 $8a $9b $b6 $5e $a0 }T T{ $abe7 M $abe8 M $abe9 M -> $2a $e5 $7a }T
$83ec >PC $a0 >S $08 >A $9a >X $09 >Y $a2 >P $83ec $2a >M $83ed $01 >M $83ee $59 >M
T{ op PC S A X Y P -> $83ed $a0 $10 $9a $09 $20 }T T{ $83ec M $83ed M $83ee M -> $2a $01 $59 }T
$8f8d >PC $a9 >S $cf >A $2e >X $74 >Y $63 >P $8f8d $2a >M $8f8e $bd >M $8f8f $d1 >M
T{ op PC S A X Y P -> $8f8e $a9 $9f $2e $74 $e1 }T T{ $8f8d M $8f8e M $8f8f M -> $2a $bd $d1 }T
$1c05 >PC $50 >S $b6 >A $97 >X $65 >Y $61 >P $1c05 $2a >M $1c06 $e1 >M $1c07 $c0 >M
T{ op PC S A X Y P -> $1c06 $50 $6d $97 $65 $61 }T T{ $1c05 M $1c06 M $1c07 M -> $2a $e1 $c0 }T
$39ff >PC $a2 >S $04 >A $9a >X $f8 >Y $65 >P $39ff $2a >M $3a00 $b2 >M $3a01 $0a >M
T{ op PC S A X Y P -> $3a00 $a2 $09 $9a $f8 $64 }T T{ $39ff M $3a00 M $3a01 M -> $2a $b2 $0a }T
$d343 >PC $08 >S $9e >A $fb >X $78 >Y $25 >P $d343 $2a >M $d344 $f6 >M $d345 $b2 >M
T{ op PC S A X Y P -> $d344 $08 $3d $fb $78 $25 }T T{ $d343 M $d344 M $d345 M -> $2a $f6 $b2 }T
$b83b >PC $7e >S $66 >A $73 >X $f2 >Y $65 >P $b83b $2a >M $b83c $0e >M $b83d $f3 >M
T{ op PC S A X Y P -> $b83c $7e $cd $73 $f2 $e4 }T T{ $b83b M $b83c M $b83d M -> $2a $0e $f3 }T
( 2b )
$00d3 >PC $be >S $a9 >A $45 >X $91 >Y $22 >P $00d3 $2b >M $00d4 $ef >M $00d5 $90 >M
T{ op PC S A X Y P -> $00d4 $be $a9 $45 $91 $22 }T T{ $00d3 M $00d4 M $00d5 M -> $2b $ef $90 }T
$abaa >PC $0a >S $35 >A $bf >X $9f >Y $65 >P $abaa $2b >M $abab $f4 >M $abac $3f >M
T{ op PC S A X Y P -> $abab $0a $35 $bf $9f $65 }T T{ $abaa M $abab M $abac M -> $2b $f4 $3f }T
$09df >PC $23 >S $f1 >A $12 >X $43 >Y $60 >P $09df $2b >M $09e0 $ca >M $09e1 $50 >M
T{ op PC S A X Y P -> $09e0 $23 $f1 $12 $43 $60 }T T{ $09df M $09e0 M $09e1 M -> $2b $ca $50 }T
$a6da >PC $75 >S $39 >A $b8 >X $f5 >Y $e4 >P $a6da $2b >M $a6db $57 >M $a6dc $d1 >M
T{ op PC S A X Y P -> $a6db $75 $39 $b8 $f5 $e4 }T T{ $a6da M $a6db M $a6dc M -> $2b $57 $d1 }T
$5618 >PC $c3 >S $2d >A $22 >X $72 >Y $a3 >P $5618 $2b >M $5619 $f6 >M $561a $8b >M
T{ op PC S A X Y P -> $5619 $c3 $2d $22 $72 $a3 }T T{ $5618 M $5619 M $561a M -> $2b $f6 $8b }T
$cab8 >PC $49 >S $b4 >A $8b >X $82 >Y $67 >P $cab8 $2b >M $cab9 $ab >M $caba $86 >M
T{ op PC S A X Y P -> $cab9 $49 $b4 $8b $82 $67 }T T{ $cab8 M $cab9 M $caba M -> $2b $ab $86 }T
$624a >PC $6f >S $26 >A $b7 >X $fc >Y $66 >P $624a $2b >M $624b $5f >M $624c $f0 >M
T{ op PC S A X Y P -> $624b $6f $26 $b7 $fc $66 }T T{ $624a M $624b M $624c M -> $2b $5f $f0 }T
$a727 >PC $b4 >S $8b >A $1e >X $93 >Y $e4 >P $a727 $2b >M $a728 $7b >M $a729 $06 >M
T{ op PC S A X Y P -> $a728 $b4 $8b $1e $93 $e4 }T T{ $a727 M $a728 M $a729 M -> $2b $7b $06 }T
$cc6d >PC $42 >S $62 >A $4c >X $ef >Y $a1 >P $cc6d $2b >M $cc6e $1e >M $cc6f $93 >M
T{ op PC S A X Y P -> $cc6e $42 $62 $4c $ef $a1 }T T{ $cc6d M $cc6e M $cc6f M -> $2b $1e $93 }T
$3b6a >PC $cf >S $bd >A $54 >X $75 >Y $e2 >P $3b6a $2b >M $3b6b $73 >M $3b6c $98 >M
T{ op PC S A X Y P -> $3b6b $cf $bd $54 $75 $e2 }T T{ $3b6a M $3b6b M $3b6c M -> $2b $73 $98 }T
$0ce9 >PC $0e >S $e0 >A $92 >X $65 >Y $66 >P $0ce9 $2b >M $0cea $ac >M $0ceb $f0 >M
T{ op PC S A X Y P -> $0cea $0e $e0 $92 $65 $66 }T T{ $0ce9 M $0cea M $0ceb M -> $2b $ac $f0 }T
$3c96 >PC $9a >S $ea >A $72 >X $13 >Y $22 >P $3c96 $2b >M $3c97 $92 >M $3c98 $9c >M
T{ op PC S A X Y P -> $3c97 $9a $ea $72 $13 $22 }T T{ $3c96 M $3c97 M $3c98 M -> $2b $92 $9c }T
$c636 >PC $3e >S $0f >A $08 >X $f9 >Y $25 >P $c636 $2b >M $c637 $81 >M $c638 $af >M
T{ op PC S A X Y P -> $c637 $3e $0f $08 $f9 $25 }T T{ $c636 M $c637 M $c638 M -> $2b $81 $af }T
$7d16 >PC $1c >S $03 >A $2f >X $ea >Y $a6 >P $7d16 $2b >M $7d17 $d4 >M $7d18 $a0 >M
T{ op PC S A X Y P -> $7d17 $1c $03 $2f $ea $a6 }T T{ $7d16 M $7d17 M $7d18 M -> $2b $d4 $a0 }T
$65c5 >PC $94 >S $2f >A $b8 >X $61 >Y $a2 >P $65c5 $2b >M $65c6 $d3 >M $65c7 $8f >M
T{ op PC S A X Y P -> $65c6 $94 $2f $b8 $61 $a2 }T T{ $65c5 M $65c6 M $65c7 M -> $2b $d3 $8f }T
$38cc >PC $42 >S $c9 >A $d5 >X $a4 >Y $a5 >P $38cc $2b >M $38cd $b6 >M $38ce $c4 >M
T{ op PC S A X Y P -> $38cd $42 $c9 $d5 $a4 $a5 }T T{ $38cc M $38cd M $38ce M -> $2b $b6 $c4 }T
( 2c )
$e044 >PC $6f >S $dd >A $a5 >X $9e >Y $a5 >P $57c4 $ad >M $e044 $2c >M $e045 $c4 >M $e046 $57 >M $e047 $da >M
T{ op PC S A X Y P -> $e047 $6f $dd $a5 $9e $a5 }T T{ $57c4 M $e044 M $e045 M $e046 M $e047 M -> $ad $2c $c4 $57 $da }T
$5951 >PC $b9 >S $7f >A $d6 >X $81 >Y $a6 >P $0b46 $3d >M $5951 $2c >M $5952 $46 >M $5953 $0b >M $5954 $22 >M
T{ op PC S A X Y P -> $5954 $b9 $7f $d6 $81 $24 }T T{ $0b46 M $5951 M $5952 M $5953 M $5954 M -> $3d $2c $46 $0b $22 }T
$6dea >PC $4c >S $84 >A $0e >X $d6 >Y $61 >P $6dea $2c >M $6deb $99 >M $6dec $be >M $6ded $cd >M $be99 $96 >M
T{ op PC S A X Y P -> $6ded $4c $84 $0e $d6 $a1 }T T{ $6dea M $6deb M $6dec M $6ded M $be99 M -> $2c $99 $be $cd $96 }T
$840f >PC $bd >S $d9 >A $83 >X $85 >Y $a0 >P $17fd $12 >M $840f $2c >M $8410 $fd >M $8411 $17 >M $8412 $65 >M
T{ op PC S A X Y P -> $8412 $bd $d9 $83 $85 $20 }T T{ $17fd M $840f M $8410 M $8411 M $8412 M -> $12 $2c $fd $17 $65 }T
$d1c9 >PC $62 >S $7d >A $50 >X $60 >Y $20 >P $0f08 $a9 >M $d1c9 $2c >M $d1ca $08 >M $d1cb $0f >M $d1cc $6c >M
T{ op PC S A X Y P -> $d1cc $62 $7d $50 $60 $a0 }T T{ $0f08 M $d1c9 M $d1ca M $d1cb M $d1cc M -> $a9 $2c $08 $0f $6c }T
$a954 >PC $08 >S $e3 >A $12 >X $7d >Y $e5 >P $a954 $2c >M $a955 $fc >M $a956 $aa >M $a957 $fe >M $aafc $f0 >M
T{ op PC S A X Y P -> $a957 $08 $e3 $12 $7d $e5 }T T{ $a954 M $a955 M $a956 M $a957 M $aafc M -> $2c $fc $aa $fe $f0 }T
$138c >PC $be >S $52 >A $bf >X $74 >Y $a7 >P $138c $2c >M $138d $ee >M $138e $a6 >M $138f $27 >M $a6ee $f4 >M
T{ op PC S A X Y P -> $138f $be $52 $bf $74 $e5 }T T{ $138c M $138d M $138e M $138f M $a6ee M -> $2c $ee $a6 $27 $f4 }T
$2103 >PC $fc >S $51 >A $66 >X $15 >Y $62 >P $2103 $2c >M $2104 $34 >M $2105 $d1 >M $2106 $ec >M $d134 $8b >M
T{ op PC S A X Y P -> $2106 $fc $51 $66 $15 $a0 }T T{ $2103 M $2104 M $2105 M $2106 M $d134 M -> $2c $34 $d1 $ec $8b }T
$fe8b >PC $e6 >S $01 >A $b2 >X $50 >Y $64 >P $9dca $93 >M $fe8b $2c >M $fe8c $ca >M $fe8d $9d >M $fe8e $8c >M
T{ op PC S A X Y P -> $fe8e $e6 $01 $b2 $50 $a4 }T T{ $9dca M $fe8b M $fe8c M $fe8d M $fe8e M -> $93 $2c $ca $9d $8c }T
$337b >PC $47 >S $0b >A $2c >X $52 >Y $a7 >P $2141 $d8 >M $337b $2c >M $337c $41 >M $337d $21 >M $337e $1e >M
T{ op PC S A X Y P -> $337e $47 $0b $2c $52 $e5 }T T{ $2141 M $337b M $337c M $337d M $337e M -> $d8 $2c $41 $21 $1e }T
$382b >PC $e5 >S $84 >A $23 >X $ed >Y $e5 >P $1737 $cb >M $382b $2c >M $382c $37 >M $382d $17 >M $382e $37 >M
T{ op PC S A X Y P -> $382e $e5 $84 $23 $ed $e5 }T T{ $1737 M $382b M $382c M $382d M $382e M -> $cb $2c $37 $17 $37 }T
$c8f1 >PC $e6 >S $c4 >A $c6 >X $37 >Y $a4 >P $7430 $51 >M $c8f1 $2c >M $c8f2 $30 >M $c8f3 $74 >M $c8f4 $c6 >M
T{ op PC S A X Y P -> $c8f4 $e6 $c4 $c6 $37 $64 }T T{ $7430 M $c8f1 M $c8f2 M $c8f3 M $c8f4 M -> $51 $2c $30 $74 $c6 }T
$1704 >PC $81 >S $4b >A $78 >X $68 >Y $23 >P $1704 $2c >M $1705 $0f >M $1706 $d8 >M $1707 $9c >M $d80f $fc >M
T{ op PC S A X Y P -> $1707 $81 $4b $78 $68 $e1 }T T{ $1704 M $1705 M $1706 M $1707 M $d80f M -> $2c $0f $d8 $9c $fc }T
$27a4 >PC $27 >S $8c >A $3b >X $1e >Y $a5 >P $27a4 $2c >M $27a5 $9d >M $27a6 $7f >M $27a7 $37 >M $7f9d $bf >M
T{ op PC S A X Y P -> $27a7 $27 $8c $3b $1e $a5 }T T{ $27a4 M $27a5 M $27a6 M $27a7 M $7f9d M -> $2c $9d $7f $37 $bf }T
$ced8 >PC $88 >S $a5 >A $fd >X $72 >Y $21 >P $b250 $76 >M $ced8 $2c >M $ced9 $50 >M $ceda $b2 >M $cedb $8e >M
T{ op PC S A X Y P -> $cedb $88 $a5 $fd $72 $61 }T T{ $b250 M $ced8 M $ced9 M $ceda M $cedb M -> $76 $2c $50 $b2 $8e }T
$6083 >PC $eb >S $8c >A $7b >X $1b >Y $21 >P $3c91 $2f >M $6083 $2c >M $6084 $91 >M $6085 $3c >M $6086 $97 >M
T{ op PC S A X Y P -> $6086 $eb $8c $7b $1b $21 }T T{ $3c91 M $6083 M $6084 M $6085 M $6086 M -> $2f $2c $91 $3c $97 }T
( 2d )
$6f88 >PC $1e >S $33 >A $21 >X $4b >Y $27 >P $6f88 $2d >M $6f89 $77 >M $6f8a $a0 >M $6f8b $0d >M $a077 $ea >M
T{ op PC S A X Y P -> $6f8b $1e $22 $21 $4b $25 }T T{ $6f88 M $6f89 M $6f8a M $6f8b M $a077 M -> $2d $77 $a0 $0d $ea }T
$ed6f >PC $ca >S $69 >A $b5 >X $b0 >Y $63 >P $8c58 $b4 >M $ed6f $2d >M $ed70 $58 >M $ed71 $8c >M $ed72 $65 >M
T{ op PC S A X Y P -> $ed72 $ca $20 $b5 $b0 $61 }T T{ $8c58 M $ed6f M $ed70 M $ed71 M $ed72 M -> $b4 $2d $58 $8c $65 }T
$0ef1 >PC $93 >S $45 >A $8c >X $b8 >Y $63 >P $0ef1 $2d >M $0ef2 $60 >M $0ef3 $ef >M $0ef4 $d6 >M $ef60 $13 >M
T{ op PC S A X Y P -> $0ef4 $93 $01 $8c $b8 $61 }T T{ $0ef1 M $0ef2 M $0ef3 M $0ef4 M $ef60 M -> $2d $60 $ef $d6 $13 }T
$2233 >PC $86 >S $6d >A $61 >X $d3 >Y $25 >P $2233 $2d >M $2234 $0f >M $2235 $27 >M $2236 $59 >M $270f $a5 >M
T{ op PC S A X Y P -> $2236 $86 $25 $61 $d3 $25 }T T{ $2233 M $2234 M $2235 M $2236 M $270f M -> $2d $0f $27 $59 $a5 }T
$7e8c >PC $39 >S $b4 >A $1a >X $f6 >Y $a6 >P $7e8c $2d >M $7e8d $f5 >M $7e8e $f7 >M $7e8f $ba >M $f7f5 $b8 >M
T{ op PC S A X Y P -> $7e8f $39 $b0 $1a $f6 $a4 }T T{ $7e8c M $7e8d M $7e8e M $7e8f M $f7f5 M -> $2d $f5 $f7 $ba $b8 }T
$a51e >PC $a0 >S $b8 >A $fb >X $0e >Y $e7 >P $5184 $45 >M $a51e $2d >M $a51f $84 >M $a520 $51 >M $a521 $ab >M
T{ op PC S A X Y P -> $a521 $a0 $00 $fb $0e $67 }T T{ $5184 M $a51e M $a51f M $a520 M $a521 M -> $45 $2d $84 $51 $ab }T
$addb >PC $ea >S $49 >A $fa >X $b7 >Y $24 >P $0b0b $6a >M $addb $2d >M $addc $0b >M $addd $0b >M $adde $c9 >M
T{ op PC S A X Y P -> $adde $ea $48 $fa $b7 $24 }T T{ $0b0b M $addb M $addc M $addd M $adde M -> $6a $2d $0b $0b $c9 }T
$44e0 >PC $ea >S $01 >A $f4 >X $12 >Y $e6 >P $44e0 $2d >M $44e1 $73 >M $44e2 $b0 >M $44e3 $1e >M $b073 $e4 >M
T{ op PC S A X Y P -> $44e3 $ea $00 $f4 $12 $66 }T T{ $44e0 M $44e1 M $44e2 M $44e3 M $b073 M -> $2d $73 $b0 $1e $e4 }T
$f8cb >PC $b3 >S $b6 >A $0a >X $50 >Y $64 >P $757a $79 >M $f8cb $2d >M $f8cc $7a >M $f8cd $75 >M $f8ce $da >M
T{ op PC S A X Y P -> $f8ce $b3 $30 $0a $50 $64 }T T{ $757a M $f8cb M $f8cc M $f8cd M $f8ce M -> $79 $2d $7a $75 $da }T
$ecb2 >PC $8a >S $b0 >A $49 >X $3a >Y $62 >P $2c9a $74 >M $ecb2 $2d >M $ecb3 $9a >M $ecb4 $2c >M $ecb5 $a4 >M
T{ op PC S A X Y P -> $ecb5 $8a $30 $49 $3a $60 }T T{ $2c9a M $ecb2 M $ecb3 M $ecb4 M $ecb5 M -> $74 $2d $9a $2c $a4 }T
$ae54 >PC $7b >S $ae >A $da >X $c7 >Y $24 >P $9945 $fa >M $ae54 $2d >M $ae55 $45 >M $ae56 $99 >M $ae57 $66 >M
T{ op PC S A X Y P -> $ae57 $7b $aa $da $c7 $a4 }T T{ $9945 M $ae54 M $ae55 M $ae56 M $ae57 M -> $fa $2d $45 $99 $66 }T
$b852 >PC $6b >S $f1 >A $ab >X $33 >Y $21 >P $17ab $a7 >M $b852 $2d >M $b853 $ab >M $b854 $17 >M $b855 $54 >M
T{ op PC S A X Y P -> $b855 $6b $a1 $ab $33 $a1 }T T{ $17ab M $b852 M $b853 M $b854 M $b855 M -> $a7 $2d $ab $17 $54 }T
$224d >PC $06 >S $10 >A $3d >X $76 >Y $a7 >P $224d $2d >M $224e $4c >M $224f $96 >M $2250 $94 >M $964c $d1 >M
T{ op PC S A X Y P -> $2250 $06 $10 $3d $76 $25 }T T{ $224d M $224e M $224f M $2250 M $964c M -> $2d $4c $96 $94 $d1 }T
$d784 >PC $15 >S $8f >A $25 >X $e0 >Y $63 >P $33a9 $bb >M $d784 $2d >M $d785 $a9 >M $d786 $33 >M $d787 $0a >M
T{ op PC S A X Y P -> $d787 $15 $8b $25 $e0 $e1 }T T{ $33a9 M $d784 M $d785 M $d786 M $d787 M -> $bb $2d $a9 $33 $0a }T
$6994 >PC $43 >S $72 >A $c3 >X $31 >Y $22 >P $6994 $2d >M $6995 $5c >M $6996 $a9 >M $6997 $3a >M $a95c $7a >M
T{ op PC S A X Y P -> $6997 $43 $72 $c3 $31 $20 }T T{ $6994 M $6995 M $6996 M $6997 M $a95c M -> $2d $5c $a9 $3a $7a }T
$a526 >PC $15 >S $b4 >A $d4 >X $ff >Y $a0 >P $02c1 $cb >M $a526 $2d >M $a527 $c1 >M $a528 $02 >M $a529 $fa >M
T{ op PC S A X Y P -> $a529 $15 $80 $d4 $ff $a0 }T T{ $02c1 M $a526 M $a527 M $a528 M $a529 M -> $cb $2d $c1 $02 $fa }T
( 2e )
$e98f >PC $5e >S $01 >A $3d >X $a3 >Y $e6 >P $913e $ae >M $e98f $2e >M $e990 $3e >M $e991 $91 >M $e992 $49 >M
T{ op PC S A X Y P -> $e992 $5e $01 $3d $a3 $65 }T T{ $913e M $e98f M $e990 M $e991 M $e992 M -> $5c $2e $3e $91 $49 }T
$5f41 >PC $16 >S $81 >A $2d >X $e7 >Y $62 >P $5f41 $2e >M $5f42 $c5 >M $5f43 $f4 >M $5f44 $3d >M $f4c5 $0e >M
T{ op PC S A X Y P -> $5f44 $16 $81 $2d $e7 $60 }T T{ $5f41 M $5f42 M $5f43 M $5f44 M $f4c5 M -> $2e $c5 $f4 $3d $1c }T
$f358 >PC $5b >S $2e >A $fd >X $b8 >Y $a3 >P $a08e $02 >M $f358 $2e >M $f359 $8e >M $f35a $a0 >M $f35b $05 >M
T{ op PC S A X Y P -> $f35b $5b $2e $fd $b8 $20 }T T{ $a08e M $f358 M $f359 M $f35a M $f35b M -> $05 $2e $8e $a0 $05 }T
$5566 >PC $e9 >S $4d >A $53 >X $0e >Y $22 >P $5566 $2e >M $5567 $9d >M $5568 $8e >M $5569 $5e >M $8e9d $3e >M
T{ op PC S A X Y P -> $5569 $e9 $4d $53 $0e $20 }T T{ $5566 M $5567 M $5568 M $5569 M $8e9d M -> $2e $9d $8e $5e $7c }T
$55ca >PC $45 >S $b0 >A $0c >X $a2 >Y $e6 >P $55ca $2e >M $55cb $d7 >M $55cc $8a >M $55cd $30 >M $8ad7 $87 >M
T{ op PC S A X Y P -> $55cd $45 $b0 $0c $a2 $65 }T T{ $55ca M $55cb M $55cc M $55cd M $8ad7 M -> $2e $d7 $8a $30 $0e }T
$9b2b >PC $a6 >S $a5 >A $b9 >X $94 >Y $e2 >P $9800 $5a >M $9b2b $2e >M $9b2c $00 >M $9b2d $98 >M $9b2e $b9 >M
T{ op PC S A X Y P -> $9b2e $a6 $a5 $b9 $94 $e0 }T T{ $9800 M $9b2b M $9b2c M $9b2d M $9b2e M -> $b4 $2e $00 $98 $b9 }T
$b842 >PC $75 >S $ac >A $98 >X $63 >Y $e7 >P $4ffa $8d >M $b842 $2e >M $b843 $fa >M $b844 $4f >M $b845 $71 >M
T{ op PC S A X Y P -> $b845 $75 $ac $98 $63 $65 }T T{ $4ffa M $b842 M $b843 M $b844 M $b845 M -> $1b $2e $fa $4f $71 }T
$ca01 >PC $f5 >S $a4 >A $04 >X $27 >Y $22 >P $1c1f $95 >M $ca01 $2e >M $ca02 $1f >M $ca03 $1c >M $ca04 $17 >M
T{ op PC S A X Y P -> $ca04 $f5 $a4 $04 $27 $21 }T T{ $1c1f M $ca01 M $ca02 M $ca03 M $ca04 M -> $2a $2e $1f $1c $17 }T
$470d >PC $fa >S $f5 >A $76 >X $fd >Y $a1 >P $470d $2e >M $470e $35 >M $470f $9f >M $4710 $0d >M $9f35 $2e >M
T{ op PC S A X Y P -> $4710 $fa $f5 $76 $fd $20 }T T{ $470d M $470e M $470f M $4710 M $9f35 M -> $2e $35 $9f $0d $5d }T
$3622 >PC $ad >S $47 >A $1e >X $41 >Y $a3 >P $3622 $2e >M $3623 $5e >M $3624 $f4 >M $3625 $b5 >M $f45e $32 >M
T{ op PC S A X Y P -> $3625 $ad $47 $1e $41 $20 }T T{ $3622 M $3623 M $3624 M $3625 M $f45e M -> $2e $5e $f4 $b5 $65 }T
$c4a6 >PC $d4 >S $c9 >A $d6 >X $ec >Y $e3 >P $5c68 $69 >M $c4a6 $2e >M $c4a7 $68 >M $c4a8 $5c >M $c4a9 $e8 >M
T{ op PC S A X Y P -> $c4a9 $d4 $c9 $d6 $ec $e0 }T T{ $5c68 M $c4a6 M $c4a7 M $c4a8 M $c4a9 M -> $d3 $2e $68 $5c $e8 }T
$3814 >PC $b8 >S $1b >A $cd >X $8e >Y $a0 >P $2772 $4b >M $3814 $2e >M $3815 $72 >M $3816 $27 >M $3817 $71 >M
T{ op PC S A X Y P -> $3817 $b8 $1b $cd $8e $a0 }T T{ $2772 M $3814 M $3815 M $3816 M $3817 M -> $96 $2e $72 $27 $71 }T
$f484 >PC $d0 >S $ed >A $99 >X $b8 >Y $65 >P $ccdb $f6 >M $f484 $2e >M $f485 $db >M $f486 $cc >M $f487 $7d >M
T{ op PC S A X Y P -> $f487 $d0 $ed $99 $b8 $e5 }T T{ $ccdb M $f484 M $f485 M $f486 M $f487 M -> $ed $2e $db $cc $7d }T
$dd2f >PC $09 >S $a4 >A $24 >X $04 >Y $66 >P $a88e $1e >M $dd2f $2e >M $dd30 $8e >M $dd31 $a8 >M $dd32 $34 >M
T{ op PC S A X Y P -> $dd32 $09 $a4 $24 $04 $64 }T T{ $a88e M $dd2f M $dd30 M $dd31 M $dd32 M -> $3c $2e $8e $a8 $34 }T
$1fc1 >PC $37 >S $28 >A $59 >X $72 >Y $e7 >P $1fc1 $2e >M $1fc2 $3b >M $1fc3 $bf >M $1fc4 $67 >M $bf3b $57 >M
T{ op PC S A X Y P -> $1fc4 $37 $28 $59 $72 $e4 }T T{ $1fc1 M $1fc2 M $1fc3 M $1fc4 M $bf3b M -> $2e $3b $bf $67 $af }T
$d370 >PC $32 >S $3a >A $63 >X $08 >Y $22 >P $142e $cc >M $d370 $2e >M $d371 $2e >M $d372 $14 >M $d373 $89 >M
T{ op PC S A X Y P -> $d373 $32 $3a $63 $08 $a1 }T T{ $142e M $d370 M $d371 M $d372 M $d373 M -> $98 $2e $2e $14 $89 }T
( 2f )
$3959 >PC $49 >S $d4 >A $d3 >X $2f >Y $a7 >P $000a $ed >M $3958 $12 >M $3959 $2f >M $395a $0a >M $395b $fc >M $395c $9f >M
T{ op PC S A X Y P -> $395c $49 $d4 $d3 $2f $a7 }T T{ $000a M $3958 M $3959 M $395a M $395b M $395c M -> $ed $12 $2f $0a $fc $9f }T
$8a7d >PC $ab >S $14 >A $ad >X $db >Y $66 >P $008e $73 >M $8a7d $2f >M $8a7e $8e >M $8a7f $49 >M $8ac9 $03 >M
T{ op PC S A X Y P -> $8ac9 $ab $14 $ad $db $66 }T T{ $008e M $8a7d M $8a7e M $8a7f M $8ac9 M -> $73 $2f $8e $49 $03 }T
$55b7 >PC $b6 >S $62 >A $45 >X $48 >Y $21 >P $00cf $65 >M $55b7 $2f >M $55b8 $cf >M $55b9 $26 >M $55ba $54 >M $55e0 $a3 >M
T{ op PC S A X Y P -> $55ba $b6 $62 $45 $48 $21 }T T{ $00cf M $55b7 M $55b8 M $55b9 M $55ba M $55e0 M -> $65 $2f $cf $26 $54 $a3 }T
$cbad >PC $e1 >S $ec >A $61 >X $0a >Y $a1 >P $0081 $50 >M $cb35 $3b >M $cbad $2f >M $cbae $81 >M $cbaf $85 >M
T{ op PC S A X Y P -> $cb35 $e1 $ec $61 $0a $a1 }T T{ $0081 M $cb35 M $cbad M $cbae M $cbaf M -> $50 $3b $2f $81 $85 }T
$ce22 >PC $b0 >S $40 >A $a8 >X $76 >Y $63 >P $0008 $9c >M $ce22 $2f >M $ce23 $08 >M $ce24 $06 >M $ce25 $30 >M $ce2b $59 >M
T{ op PC S A X Y P -> $ce25 $b0 $40 $a8 $76 $63 }T T{ $0008 M $ce22 M $ce23 M $ce24 M $ce25 M $ce2b M -> $9c $2f $08 $06 $30 $59 }T
$2256 >PC $59 >S $d0 >A $5e >X $eb >Y $a5 >P $0001 $40 >M $21da $dd >M $2256 $2f >M $2257 $01 >M $2258 $81 >M $22da $16 >M
T{ op PC S A X Y P -> $21da $59 $d0 $5e $eb $a5 }T T{ $0001 M $21da M $2256 M $2257 M $2258 M $22da M -> $40 $dd $2f $01 $81 $16 }T
$aa09 >PC $82 >S $05 >A $7f >X $7f >Y $24 >P $00fe $13 >M $aa09 $2f >M $aa0a $fe >M $aa0b $74 >M $aa80 $0c >M
T{ op PC S A X Y P -> $aa80 $82 $05 $7f $7f $24 }T T{ $00fe M $aa09 M $aa0a M $aa0b M $aa80 M -> $13 $2f $fe $74 $0c }T
$c73b >PC $9d >S $fa >A $ac >X $74 >Y $a5 >P $0087 $0c >M $c73b $2f >M $c73c $87 >M $c73d $01 >M $c73e $3b >M $c73f $c1 >M
T{ op PC S A X Y P -> $c73e $9d $fa $ac $74 $a5 }T T{ $0087 M $c73b M $c73c M $c73d M $c73e M $c73f M -> $0c $2f $87 $01 $3b $c1 }T
$ef33 >PC $f8 >S $0e >A $78 >X $de >Y $a7 >P $002f $15 >M $ef33 $2f >M $ef34 $2f >M $ef35 $71 >M $ef36 $83 >M $efa7 $07 >M
T{ op PC S A X Y P -> $ef36 $f8 $0e $78 $de $a7 }T T{ $002f M $ef33 M $ef34 M $ef35 M $ef36 M $efa7 M -> $15 $2f $2f $71 $83 $07 }T
$bedb >PC $34 >S $42 >A $c9 >X $01 >Y $e1 >P $0010 $ae >M $be80 $77 >M $bedb $2f >M $bedc $10 >M $bedd $a2 >M $bede $b7 >M
T{ op PC S A X Y P -> $bede $34 $42 $c9 $01 $e1 }T T{ $0010 M $be80 M $bedb M $bedc M $bedd M $bede M -> $ae $77 $2f $10 $a2 $b7 }T
$95dd >PC $cc >S $8a >A $bd >X $df >Y $a6 >P $0094 $5c >M $9534 $f9 >M $95dd $2f >M $95de $94 >M $95df $54 >M $95e0 $79 >M
T{ op PC S A X Y P -> $95e0 $cc $8a $bd $df $a6 }T T{ $0094 M $9534 M $95dd M $95de M $95df M $95e0 M -> $5c $f9 $2f $94 $54 $79 }T
$4230 >PC $08 >S $07 >A $2c >X $1e >Y $20 >P $00d4 $63 >M $4230 $2f >M $4231 $d4 >M $4232 $3d >M $4270 $b9 >M
T{ op PC S A X Y P -> $4270 $08 $07 $2c $1e $20 }T T{ $00d4 M $4230 M $4231 M $4232 M $4270 M -> $63 $2f $d4 $3d $b9 }T
$0cd3 >PC $e2 >S $1e >A $b5 >X $01 >Y $22 >P $00e8 $cf >M $0ca3 $4b >M $0cd3 $2f >M $0cd4 $e8 >M $0cd5 $cd >M $0cd6 $eb >M
T{ op PC S A X Y P -> $0cd6 $e2 $1e $b5 $01 $22 }T T{ $00e8 M $0ca3 M $0cd3 M $0cd4 M $0cd5 M $0cd6 M -> $cf $4b $2f $e8 $cd $eb }T
$d913 >PC $1a >S $95 >A $44 >X $4e >Y $e4 >P $00fe $10 >M $d913 $2f >M $d914 $fe >M $d915 $37 >M $d94d $88 >M
T{ op PC S A X Y P -> $d94d $1a $95 $44 $4e $e4 }T T{ $00fe M $d913 M $d914 M $d915 M $d94d M -> $10 $2f $fe $37 $88 }T
$8ee0 >PC $c9 >S $c0 >A $2c >X $c0 >Y $e7 >P $00cf $20 >M $8e99 $81 >M $8ee0 $2f >M $8ee1 $cf >M $8ee2 $b6 >M
T{ op PC S A X Y P -> $8e99 $c9 $c0 $2c $c0 $e7 }T T{ $00cf M $8e99 M $8ee0 M $8ee1 M $8ee2 M -> $20 $81 $2f $cf $b6 }T
$b550 >PC $30 >S $90 >A $b1 >X $26 >Y $a7 >P $0021 $b7 >M $b527 $7e >M $b550 $2f >M $b551 $21 >M $b552 $d4 >M $b553 $ec >M
T{ op PC S A X Y P -> $b553 $30 $90 $b1 $26 $a7 }T T{ $0021 M $b527 M $b550 M $b551 M $b552 M $b553 M -> $b7 $7e $2f $21 $d4 $ec }T
( 30 )
$1656 >PC $0e >S $fc >A $52 >X $72 >Y $a1 >P $1656 $30 >M $1657 $5b >M $1658 $aa >M $16b3 $3f >M
T{ op PC S A X Y P -> $16b3 $0e $fc $52 $72 $a1 }T T{ $1656 M $1657 M $1658 M $16b3 M -> $30 $5b $aa $3f }T
$a276 >PC $c6 >S $d4 >A $3f >X $c7 >Y $24 >P $a276 $30 >M $a277 $c8 >M $a278 $a0 >M
T{ op PC S A X Y P -> $a278 $c6 $d4 $3f $c7 $24 }T T{ $a276 M $a277 M $a278 M -> $30 $c8 $a0 }T
$564d >PC $13 >S $1f >A $07 >X $3a >Y $61 >P $564d $30 >M $564e $de >M $564f $36 >M
T{ op PC S A X Y P -> $564f $13 $1f $07 $3a $61 }T T{ $564d M $564e M $564f M -> $30 $de $36 }T
$1fbb >PC $95 >S $a8 >A $c5 >X $b8 >Y $e5 >P $1f48 $8a >M $1fbb $30 >M $1fbc $8b >M $1fbd $0b >M
T{ op PC S A X Y P -> $1f48 $95 $a8 $c5 $b8 $e5 }T T{ $1f48 M $1fbb M $1fbc M $1fbd M -> $8a $30 $8b $0b }T
$b545 >PC $c2 >S $fa >A $38 >X $5d >Y $e2 >P $b545 $30 >M $b546 $3b >M $b547 $2b >M $b582 $b7 >M
T{ op PC S A X Y P -> $b582 $c2 $fa $38 $5d $e2 }T T{ $b545 M $b546 M $b547 M $b582 M -> $30 $3b $2b $b7 }T
$02ae >PC $f3 >S $c6 >A $4b >X $6c >Y $a6 >P $0220 $52 >M $02ae $30 >M $02af $70 >M $02b0 $c3 >M $0320 $95 >M
T{ op PC S A X Y P -> $0320 $f3 $c6 $4b $6c $a6 }T T{ $0220 M $02ae M $02af M $02b0 M $0320 M -> $52 $30 $70 $c3 $95 }T
$c08b >PC $17 >S $a1 >A $c4 >X $de >Y $e6 >P $c08b $30 >M $c08c $26 >M $c08d $5c >M $c0b3 $e6 >M
T{ op PC S A X Y P -> $c0b3 $17 $a1 $c4 $de $e6 }T T{ $c08b M $c08c M $c08d M $c0b3 M -> $30 $26 $5c $e6 }T
$9efb >PC $d5 >S $e8 >A $3a >X $c0 >Y $66 >P $9efb $30 >M $9efc $71 >M $9efd $80 >M
T{ op PC S A X Y P -> $9efd $d5 $e8 $3a $c0 $66 }T T{ $9efb M $9efc M $9efd M -> $30 $71 $80 }T
$79a6 >PC $b0 >S $75 >A $c4 >X $55 >Y $67 >P $79a6 $30 >M $79a7 $b7 >M $79a8 $86 >M
T{ op PC S A X Y P -> $79a8 $b0 $75 $c4 $55 $67 }T T{ $79a6 M $79a7 M $79a8 M -> $30 $b7 $86 }T
$6ef2 >PC $6a >S $7d >A $10 >X $bb >Y $67 >P $6ef2 $30 >M $6ef3 $4d >M $6ef4 $0a >M
T{ op PC S A X Y P -> $6ef4 $6a $7d $10 $bb $67 }T T{ $6ef2 M $6ef3 M $6ef4 M -> $30 $4d $0a }T
$6cbc >PC $ef >S $c0 >A $75 >X $a1 >Y $a3 >P $6c5d $92 >M $6cbc $30 >M $6cbd $9f >M $6cbe $42 >M
T{ op PC S A X Y P -> $6c5d $ef $c0 $75 $a1 $a3 }T T{ $6c5d M $6cbc M $6cbd M $6cbe M -> $92 $30 $9f $42 }T
$fecc >PC $9b >S $e2 >A $ce >X $1d >Y $e6 >P $fe9a $a8 >M $fecc $30 >M $fecd $cc >M $fece $cb >M
T{ op PC S A X Y P -> $fe9a $9b $e2 $ce $1d $e6 }T T{ $fe9a M $fecc M $fecd M $fece M -> $a8 $30 $cc $cb }T
$8211 >PC $ac >S $c0 >A $42 >X $73 >Y $a3 >P $8211 $30 >M $8212 $0e >M $8213 $96 >M $8221 $17 >M
T{ op PC S A X Y P -> $8221 $ac $c0 $42 $73 $a3 }T T{ $8211 M $8212 M $8213 M $8221 M -> $30 $0e $96 $17 }T
$f9e1 >PC $f3 >S $62 >A $74 >X $e8 >Y $67 >P $f9e1 $30 >M $f9e2 $27 >M $f9e3 $cb >M
T{ op PC S A X Y P -> $f9e3 $f3 $62 $74 $e8 $67 }T T{ $f9e1 M $f9e2 M $f9e3 M -> $30 $27 $cb }T
$1a84 >PC $ad >S $04 >A $c3 >X $73 >Y $22 >P $1a84 $30 >M $1a85 $fe >M $1a86 $63 >M
T{ op PC S A X Y P -> $1a86 $ad $04 $c3 $73 $22 }T T{ $1a84 M $1a85 M $1a86 M -> $30 $fe $63 }T
$16a7 >PC $75 >S $33 >A $2a >X $ba >Y $e1 >P $1650 $21 >M $16a7 $30 >M $16a8 $a7 >M $16a9 $50 >M
T{ op PC S A X Y P -> $1650 $75 $33 $2a $ba $e1 }T T{ $1650 M $16a7 M $16a8 M $16a9 M -> $21 $30 $a7 $50 }T
( 31 )
$fab8 >PC $7c >S $17 >A $7c >X $f7 >Y $e6 >P $0057 $80 >M $0058 $6d >M $6e77 $37 >M $fab8 $31 >M $fab9 $57 >M $faba $5c >M
T{ op PC S A X Y P -> $faba $7c $17 $7c $f7 $64 }T T{ $0057 M $0058 M $6e77 M $fab8 M $fab9 M $faba M -> $80 $6d $37 $31 $57 $5c }T
$2758 >PC $7e >S $c2 >A $99 >X $d6 >Y $20 >P $00d4 $30 >M $00d5 $13 >M $1406 $aa >M $2758 $31 >M $2759 $d4 >M $275a $24 >M
T{ op PC S A X Y P -> $275a $7e $82 $99 $d6 $a0 }T T{ $00d4 M $00d5 M $1406 M $2758 M $2759 M $275a M -> $30 $13 $aa $31 $d4 $24 }T
$b32a >PC $09 >S $2b >A $b0 >X $b8 >Y $e1 >P $003f $3d >M $0040 $54 >M $54f5 $1f >M $b32a $31 >M $b32b $3f >M $b32c $e7 >M
T{ op PC S A X Y P -> $b32c $09 $0b $b0 $b8 $61 }T T{ $003f M $0040 M $54f5 M $b32a M $b32b M $b32c M -> $3d $54 $1f $31 $3f $e7 }T
$dfc6 >PC $7b >S $4f >A $2d >X $93 >Y $26 >P $00a1 $15 >M $00a2 $13 >M $13a8 $08 >M $dfc6 $31 >M $dfc7 $a1 >M $dfc8 $cd >M
T{ op PC S A X Y P -> $dfc8 $7b $08 $2d $93 $24 }T T{ $00a1 M $00a2 M $13a8 M $dfc6 M $dfc7 M $dfc8 M -> $15 $13 $08 $31 $a1 $cd }T
$2a31 >PC $40 >S $a7 >A $f0 >X $5b >Y $60 >P $0019 $1b >M $001a $a4 >M $2a31 $31 >M $2a32 $19 >M $2a33 $a0 >M $a476 $56 >M
T{ op PC S A X Y P -> $2a33 $40 $06 $f0 $5b $60 }T T{ $0019 M $001a M $2a31 M $2a32 M $2a33 M $a476 M -> $1b $a4 $31 $19 $a0 $56 }T
$ce7f >PC $eb >S $73 >A $e3 >X $b8 >Y $20 >P $0096 $26 >M $0097 $38 >M $38de $30 >M $ce7f $31 >M $ce80 $96 >M $ce81 $e7 >M
T{ op PC S A X Y P -> $ce81 $eb $30 $e3 $b8 $20 }T T{ $0096 M $0097 M $38de M $ce7f M $ce80 M $ce81 M -> $26 $38 $30 $31 $96 $e7 }T
$f384 >PC $a1 >S $dc >A $07 >X $cb >Y $e5 >P $007c $78 >M $007d $92 >M $9343 $a0 >M $f384 $31 >M $f385 $7c >M $f386 $90 >M
T{ op PC S A X Y P -> $f386 $a1 $80 $07 $cb $e5 }T T{ $007c M $007d M $9343 M $f384 M $f385 M $f386 M -> $78 $92 $a0 $31 $7c $90 }T
$974d >PC $f7 >S $e1 >A $67 >X $3f >Y $e5 >P $002e $9f >M $002f $95 >M $95de $8f >M $974d $31 >M $974e $2e >M $974f $4a >M
T{ op PC S A X Y P -> $974f $f7 $81 $67 $3f $e5 }T T{ $002e M $002f M $95de M $974d M $974e M $974f M -> $9f $95 $8f $31 $2e $4a }T
$71b7 >PC $c9 >S $6f >A $65 >X $ac >Y $23 >P $00ad $bb >M $00ae $4d >M $4e67 $dd >M $71b7 $31 >M $71b8 $ad >M $71b9 $a3 >M
T{ op PC S A X Y P -> $71b9 $c9 $4d $65 $ac $21 }T T{ $00ad M $00ae M $4e67 M $71b7 M $71b8 M $71b9 M -> $bb $4d $dd $31 $ad $a3 }T
$814b >PC $51 >S $94 >A $9f >X $cc >Y $a3 >P $0060 $2a >M $0061 $e6 >M $814b $31 >M $814c $60 >M $814d $99 >M $e6f6 $5d >M
T{ op PC S A X Y P -> $814d $51 $14 $9f $cc $21 }T T{ $0060 M $0061 M $814b M $814c M $814d M $e6f6 M -> $2a $e6 $31 $60 $99 $5d }T
$7581 >PC $87 >S $22 >A $70 >X $4a >Y $e0 >P $00ac $1e >M $00ad $c1 >M $7581 $31 >M $7582 $ac >M $7583 $fd >M $c168 $fc >M
T{ op PC S A X Y P -> $7583 $87 $20 $70 $4a $60 }T T{ $00ac M $00ad M $7581 M $7582 M $7583 M $c168 M -> $1e $c1 $31 $ac $fd $fc }T
$adc9 >PC $47 >S $61 >A $4b >X $79 >Y $e0 >P $0031 $36 >M $0032 $73 >M $73af $58 >M $adc9 $31 >M $adca $31 >M $adcb $e4 >M
T{ op PC S A X Y P -> $adcb $47 $40 $4b $79 $60 }T T{ $0031 M $0032 M $73af M $adc9 M $adca M $adcb M -> $36 $73 $58 $31 $31 $e4 }T
$af0c >PC $f2 >S $2b >A $22 >X $e0 >Y $64 >P $0092 $fb >M $0093 $bd >M $af0c $31 >M $af0d $92 >M $af0e $cb >M $bedb $5e >M
T{ op PC S A X Y P -> $af0e $f2 $0a $22 $e0 $64 }T T{ $0092 M $0093 M $af0c M $af0d M $af0e M $bedb M -> $fb $bd $31 $92 $cb $5e }T
$5fb7 >PC $6a >S $be >A $ba >X $2b >Y $27 >P $002c $45 >M $002d $ec >M $5fb7 $31 >M $5fb8 $2c >M $5fb9 $70 >M $ec70 $8f >M
T{ op PC S A X Y P -> $5fb9 $6a $8e $ba $2b $a5 }T T{ $002c M $002d M $5fb7 M $5fb8 M $5fb9 M $ec70 M -> $45 $ec $31 $2c $70 $8f }T
$ffc0 >PC $60 >S $16 >A $a7 >X $5e >Y $23 >P $005e $12 >M $005f $cc >M $cc70 $28 >M $ffc0 $31 >M $ffc1 $5e >M $ffc2 $00 >M
T{ op PC S A X Y P -> $ffc2 $60 $00 $a7 $5e $23 }T T{ $005e M $005f M $cc70 M $ffc0 M $ffc1 M $ffc2 M -> $12 $cc $28 $31 $5e $00 }T
$627d >PC $2f >S $fc >A $a9 >X $0b >Y $24 >P $000a $31 >M $000b $e0 >M $627d $31 >M $627e $0a >M $627f $68 >M $e03c $37 >M
T{ op PC S A X Y P -> $627f $2f $34 $a9 $0b $24 }T T{ $000a M $000b M $627d M $627e M $627f M $e03c M -> $31 $e0 $31 $0a $68 $37 }T
( 32 )
$ad17 >PC $36 >S $69 >A $25 >X $6c >Y $64 >P $0012 $92 >M $0013 $a6 >M $a692 $b0 >M $ad17 $32 >M $ad18 $12 >M $ad19 $12 >M
T{ op PC S A X Y P -> $ad19 $36 $20 $25 $6c $64 }T T{ $0012 M $0013 M $a692 M $ad17 M $ad18 M $ad19 M -> $92 $a6 $b0 $32 $12 $12 }T
$df2e >PC $e6 >S $7c >A $a4 >X $a5 >Y $26 >P $0087 $f6 >M $0088 $eb >M $df2e $32 >M $df2f $87 >M $df30 $2c >M $ebf6 $16 >M
T{ op PC S A X Y P -> $df30 $e6 $14 $a4 $a5 $24 }T T{ $0087 M $0088 M $df2e M $df2f M $df30 M $ebf6 M -> $f6 $eb $32 $87 $2c $16 }T
$8c5b >PC $59 >S $95 >A $6a >X $8d >Y $a5 >P $0040 $17 >M $0041 $a1 >M $8c5b $32 >M $8c5c $40 >M $8c5d $2e >M $a117 $d8 >M
T{ op PC S A X Y P -> $8c5d $59 $90 $6a $8d $a5 }T T{ $0040 M $0041 M $8c5b M $8c5c M $8c5d M $a117 M -> $17 $a1 $32 $40 $2e $d8 }T
$eb1f >PC $14 >S $5f >A $60 >X $41 >Y $e4 >P $001d $c6 >M $001e $c2 >M $c2c6 $67 >M $eb1f $32 >M $eb20 $1d >M $eb21 $2d >M
T{ op PC S A X Y P -> $eb21 $14 $47 $60 $41 $64 }T T{ $001d M $001e M $c2c6 M $eb1f M $eb20 M $eb21 M -> $c6 $c2 $67 $32 $1d $2d }T
$24b5 >PC $38 >S $e5 >A $a7 >X $bd >Y $a1 >P $0080 $55 >M $0081 $73 >M $24b5 $32 >M $24b6 $80 >M $24b7 $b1 >M $7355 $33 >M
T{ op PC S A X Y P -> $24b7 $38 $21 $a7 $bd $21 }T T{ $0080 M $0081 M $24b5 M $24b6 M $24b7 M $7355 M -> $55 $73 $32 $80 $b1 $33 }T
$9790 >PC $e5 >S $66 >A $ed >X $34 >Y $25 >P $0086 $a3 >M $0087 $08 >M $08a3 $b9 >M $9790 $32 >M $9791 $86 >M $9792 $b1 >M
T{ op PC S A X Y P -> $9792 $e5 $20 $ed $34 $25 }T T{ $0086 M $0087 M $08a3 M $9790 M $9791 M $9792 M -> $a3 $08 $b9 $32 $86 $b1 }T
$dfd1 >PC $b4 >S $e3 >A $35 >X $f4 >Y $63 >P $0075 $a5 >M $0076 $0c >M $0ca5 $f3 >M $dfd1 $32 >M $dfd2 $75 >M $dfd3 $fb >M
T{ op PC S A X Y P -> $dfd3 $b4 $e3 $35 $f4 $e1 }T T{ $0075 M $0076 M $0ca5 M $dfd1 M $dfd2 M $dfd3 M -> $a5 $0c $f3 $32 $75 $fb }T
$db71 >PC $a3 >S $59 >A $5f >X $56 >Y $e1 >P $006e $b9 >M $006f $27 >M $27b9 $f1 >M $db71 $32 >M $db72 $6e >M $db73 $a8 >M
T{ op PC S A X Y P -> $db73 $a3 $51 $5f $56 $61 }T T{ $006e M $006f M $27b9 M $db71 M $db72 M $db73 M -> $b9 $27 $f1 $32 $6e $a8 }T
$993c >PC $df >S $82 >A $84 >X $34 >Y $e6 >P $00d8 $dd >M $00d9 $59 >M $59dd $3a >M $993c $32 >M $993d $d8 >M $993e $41 >M
T{ op PC S A X Y P -> $993e $df $02 $84 $34 $64 }T T{ $00d8 M $00d9 M $59dd M $993c M $993d M $993e M -> $dd $59 $3a $32 $d8 $41 }T
$2c0b >PC $ea >S $42 >A $8d >X $79 >Y $a1 >P $0043 $c8 >M $0044 $18 >M $18c8 $6d >M $2c0b $32 >M $2c0c $43 >M $2c0d $32 >M
T{ op PC S A X Y P -> $2c0d $ea $40 $8d $79 $21 }T T{ $0043 M $0044 M $18c8 M $2c0b M $2c0c M $2c0d M -> $c8 $18 $6d $32 $43 $32 }T
$9122 >PC $81 >S $09 >A $c2 >X $a3 >Y $e7 >P $001f $da >M $0020 $af >M $9122 $32 >M $9123 $1f >M $9124 $19 >M $afda $c6 >M
T{ op PC S A X Y P -> $9124 $81 $00 $c2 $a3 $67 }T T{ $001f M $0020 M $9122 M $9123 M $9124 M $afda M -> $da $af $32 $1f $19 $c6 }T
$789a >PC $bc >S $0d >A $ea >X $87 >Y $26 >P $0097 $a8 >M $0098 $4d >M $4da8 $f7 >M $789a $32 >M $789b $97 >M $789c $b8 >M
T{ op PC S A X Y P -> $789c $bc $05 $ea $87 $24 }T T{ $0097 M $0098 M $4da8 M $789a M $789b M $789c M -> $a8 $4d $f7 $32 $97 $b8 }T
$b5a6 >PC $c3 >S $f0 >A $1c >X $9c >Y $25 >P $001b $11 >M $001c $b8 >M $b5a6 $32 >M $b5a7 $1b >M $b5a8 $7b >M $b811 $00 >M
T{ op PC S A X Y P -> $b5a8 $c3 $00 $1c $9c $27 }T T{ $001b M $001c M $b5a6 M $b5a7 M $b5a8 M $b811 M -> $11 $b8 $32 $1b $7b $00 }T
$d2d3 >PC $87 >S $4a >A $39 >X $1a >Y $e2 >P $00c2 $d7 >M $00c3 $a5 >M $a5d7 $01 >M $d2d3 $32 >M $d2d4 $c2 >M $d2d5 $21 >M
T{ op PC S A X Y P -> $d2d5 $87 $00 $39 $1a $62 }T T{ $00c2 M $00c3 M $a5d7 M $d2d3 M $d2d4 M $d2d5 M -> $d7 $a5 $01 $32 $c2 $21 }T
$8186 >PC $07 >S $a9 >A $81 >X $47 >Y $63 >P $000a $82 >M $000b $5f >M $5f82 $94 >M $8186 $32 >M $8187 $0a >M $8188 $30 >M
T{ op PC S A X Y P -> $8188 $07 $80 $81 $47 $e1 }T T{ $000a M $000b M $5f82 M $8186 M $8187 M $8188 M -> $82 $5f $94 $32 $0a $30 }T
$dad0 >PC $d6 >S $65 >A $6e >X $e3 >Y $27 >P $00fc $26 >M $00fd $ab >M $ab26 $f9 >M $dad0 $32 >M $dad1 $fc >M $dad2 $0f >M
T{ op PC S A X Y P -> $dad2 $d6 $61 $6e $e3 $25 }T T{ $00fc M $00fd M $ab26 M $dad0 M $dad1 M $dad2 M -> $26 $ab $f9 $32 $fc $0f }T
( 33 )
$1f5d >PC $4b >S $f4 >A $10 >X $6a >Y $26 >P $1f5d $33 >M $1f5e $6b >M $1f5f $f4 >M
T{ op PC S A X Y P -> $1f5e $4b $f4 $10 $6a $26 }T T{ $1f5d M $1f5e M $1f5f M -> $33 $6b $f4 }T
$27ec >PC $18 >S $2e >A $09 >X $4a >Y $e3 >P $27ec $33 >M $27ed $0b >M $27ee $d8 >M
T{ op PC S A X Y P -> $27ed $18 $2e $09 $4a $e3 }T T{ $27ec M $27ed M $27ee M -> $33 $0b $d8 }T
$cf54 >PC $77 >S $a4 >A $2f >X $4b >Y $25 >P $cf54 $33 >M $cf55 $77 >M $cf56 $2b >M
T{ op PC S A X Y P -> $cf55 $77 $a4 $2f $4b $25 }T T{ $cf54 M $cf55 M $cf56 M -> $33 $77 $2b }T
$bce5 >PC $0e >S $b7 >A $7b >X $92 >Y $64 >P $bce5 $33 >M $bce6 $bf >M $bce7 $0a >M
T{ op PC S A X Y P -> $bce6 $0e $b7 $7b $92 $64 }T T{ $bce5 M $bce6 M $bce7 M -> $33 $bf $0a }T
$9a8f >PC $69 >S $fc >A $47 >X $33 >Y $66 >P $9a8f $33 >M $9a90 $00 >M $9a91 $6c >M
T{ op PC S A X Y P -> $9a90 $69 $fc $47 $33 $66 }T T{ $9a8f M $9a90 M $9a91 M -> $33 $00 $6c }T
$27ec >PC $6c >S $e7 >A $48 >X $49 >Y $24 >P $27ec $33 >M $27ed $6e >M $27ee $3e >M
T{ op PC S A X Y P -> $27ed $6c $e7 $48 $49 $24 }T T{ $27ec M $27ed M $27ee M -> $33 $6e $3e }T
$4621 >PC $df >S $c3 >A $a5 >X $16 >Y $67 >P $4621 $33 >M $4622 $08 >M $4623 $a0 >M
T{ op PC S A X Y P -> $4622 $df $c3 $a5 $16 $67 }T T{ $4621 M $4622 M $4623 M -> $33 $08 $a0 }T
$b9c1 >PC $81 >S $26 >A $c4 >X $dd >Y $e2 >P $b9c1 $33 >M $b9c2 $ee >M $b9c3 $e1 >M
T{ op PC S A X Y P -> $b9c2 $81 $26 $c4 $dd $e2 }T T{ $b9c1 M $b9c2 M $b9c3 M -> $33 $ee $e1 }T
$af79 >PC $1e >S $6a >A $bf >X $e5 >Y $61 >P $af79 $33 >M $af7a $95 >M $af7b $70 >M
T{ op PC S A X Y P -> $af7a $1e $6a $bf $e5 $61 }T T{ $af79 M $af7a M $af7b M -> $33 $95 $70 }T
$cfda >PC $81 >S $c7 >A $44 >X $e5 >Y $61 >P $cfda $33 >M $cfdb $3c >M $cfdc $6f >M
T{ op PC S A X Y P -> $cfdb $81 $c7 $44 $e5 $61 }T T{ $cfda M $cfdb M $cfdc M -> $33 $3c $6f }T
$34f0 >PC $7e >S $64 >A $44 >X $b0 >Y $67 >P $34f0 $33 >M $34f1 $93 >M $34f2 $8c >M
T{ op PC S A X Y P -> $34f1 $7e $64 $44 $b0 $67 }T T{ $34f0 M $34f1 M $34f2 M -> $33 $93 $8c }T
$a845 >PC $c2 >S $1c >A $73 >X $95 >Y $e5 >P $a845 $33 >M $a846 $8c >M $a847 $6a >M
T{ op PC S A X Y P -> $a846 $c2 $1c $73 $95 $e5 }T T{ $a845 M $a846 M $a847 M -> $33 $8c $6a }T
$384c >PC $5b >S $7e >A $5e >X $31 >Y $21 >P $384c $33 >M $384d $f0 >M $384e $43 >M
T{ op PC S A X Y P -> $384d $5b $7e $5e $31 $21 }T T{ $384c M $384d M $384e M -> $33 $f0 $43 }T
$c109 >PC $3d >S $3d >A $ce >X $22 >Y $27 >P $c109 $33 >M $c10a $25 >M $c10b $37 >M
T{ op PC S A X Y P -> $c10a $3d $3d $ce $22 $27 }T T{ $c109 M $c10a M $c10b M -> $33 $25 $37 }T
$511f >PC $cc >S $42 >A $d4 >X $bb >Y $67 >P $511f $33 >M $5120 $55 >M $5121 $b4 >M
T{ op PC S A X Y P -> $5120 $cc $42 $d4 $bb $67 }T T{ $511f M $5120 M $5121 M -> $33 $55 $b4 }T
$3a71 >PC $1e >S $11 >A $1e >X $e8 >Y $e1 >P $3a71 $33 >M $3a72 $b4 >M $3a73 $22 >M
T{ op PC S A X Y P -> $3a72 $1e $11 $1e $e8 $e1 }T T{ $3a71 M $3a72 M $3a73 M -> $33 $b4 $22 }T
( 34 )
$f98d >PC $08 >S $b6 >A $85 >X $60 >Y $60 >P $0036 $48 >M $00b1 $e5 >M $f98d $34 >M $f98e $b1 >M $f98f $da >M
T{ op PC S A X Y P -> $f98f $08 $b6 $85 $60 $62 }T T{ $0036 M $00b1 M $f98d M $f98e M $f98f M -> $48 $e5 $34 $b1 $da }T
$3384 >PC $29 >S $ef >A $26 >X $84 >Y $e6 >P $0002 $61 >M $0028 $cb >M $3384 $34 >M $3385 $02 >M $3386 $d3 >M
T{ op PC S A X Y P -> $3386 $29 $ef $26 $84 $e4 }T T{ $0002 M $0028 M $3384 M $3385 M $3386 M -> $61 $cb $34 $02 $d3 }T
$9db3 >PC $d2 >S $fa >A $67 >X $b0 >Y $e4 >P $0060 $e9 >M $00c7 $c0 >M $9db3 $34 >M $9db4 $60 >M $9db5 $04 >M
T{ op PC S A X Y P -> $9db5 $d2 $fa $67 $b0 $e4 }T T{ $0060 M $00c7 M $9db3 M $9db4 M $9db5 M -> $e9 $c0 $34 $60 $04 }T
$ccc6 >PC $92 >S $66 >A $a0 >X $ca >Y $66 >P $008e $9c >M $00ee $36 >M $ccc6 $34 >M $ccc7 $ee >M $ccc8 $d3 >M
T{ op PC S A X Y P -> $ccc8 $92 $66 $a0 $ca $a4 }T T{ $008e M $00ee M $ccc6 M $ccc7 M $ccc8 M -> $9c $36 $34 $ee $d3 }T
$97a4 >PC $70 >S $25 >A $1e >X $31 >Y $a4 >P $00de $df >M $00fc $70 >M $97a4 $34 >M $97a5 $de >M $97a6 $1c >M
T{ op PC S A X Y P -> $97a6 $70 $25 $1e $31 $64 }T T{ $00de M $00fc M $97a4 M $97a5 M $97a6 M -> $df $70 $34 $de $1c }T
$10f2 >PC $4e >S $c4 >A $cf >X $36 >Y $62 >P $0013 $0f >M $00e2 $b8 >M $10f2 $34 >M $10f3 $13 >M $10f4 $6e >M
T{ op PC S A X Y P -> $10f4 $4e $c4 $cf $36 $a0 }T T{ $0013 M $00e2 M $10f2 M $10f3 M $10f4 M -> $0f $b8 $34 $13 $6e }T
$faff >PC $f3 >S $46 >A $a3 >X $3b >Y $e4 >P $004e $14 >M $00ab $30 >M $faff $34 >M $fb00 $ab >M $fb01 $9a >M
T{ op PC S A X Y P -> $fb01 $f3 $46 $a3 $3b $24 }T T{ $004e M $00ab M $faff M $fb00 M $fb01 M -> $14 $30 $34 $ab $9a }T
$8962 >PC $b5 >S $82 >A $33 >X $70 >Y $25 >P $0052 $bd >M $0085 $25 >M $8962 $34 >M $8963 $52 >M $8964 $95 >M
T{ op PC S A X Y P -> $8964 $b5 $82 $33 $70 $27 }T T{ $0052 M $0085 M $8962 M $8963 M $8964 M -> $bd $25 $34 $52 $95 }T
$db3c >PC $ed >S $a6 >A $0f >X $0c >Y $a6 >P $009c $54 >M $00ab $b0 >M $db3c $34 >M $db3d $9c >M $db3e $95 >M
T{ op PC S A X Y P -> $db3e $ed $a6 $0f $0c $a4 }T T{ $009c M $00ab M $db3c M $db3d M $db3e M -> $54 $b0 $34 $9c $95 }T
$d1bb >PC $6a >S $71 >A $16 >X $d6 >Y $62 >P $00e7 $df >M $00fd $ef >M $d1bb $34 >M $d1bc $e7 >M $d1bd $c2 >M
T{ op PC S A X Y P -> $d1bd $6a $71 $16 $d6 $e0 }T T{ $00e7 M $00fd M $d1bb M $d1bc M $d1bd M -> $df $ef $34 $e7 $c2 }T
$cc1d >PC $ee >S $ab >A $1f >X $50 >Y $e1 >P $0079 $97 >M $0098 $59 >M $cc1d $34 >M $cc1e $79 >M $cc1f $e0 >M
T{ op PC S A X Y P -> $cc1f $ee $ab $1f $50 $61 }T T{ $0079 M $0098 M $cc1d M $cc1e M $cc1f M -> $97 $59 $34 $79 $e0 }T
$8d53 >PC $32 >S $f6 >A $1a >X $2f >Y $e7 >P $007b $5c >M $0095 $b0 >M $8d53 $34 >M $8d54 $7b >M $8d55 $ed >M
T{ op PC S A X Y P -> $8d55 $32 $f6 $1a $2f $a5 }T T{ $007b M $0095 M $8d53 M $8d54 M $8d55 M -> $5c $b0 $34 $7b $ed }T
$f8ea >PC $07 >S $3e >A $d9 >X $f5 >Y $a7 >P $002c $88 >M $0053 $7c >M $f8ea $34 >M $f8eb $53 >M $f8ec $be >M
T{ op PC S A X Y P -> $f8ec $07 $3e $d9 $f5 $a5 }T T{ $002c M $0053 M $f8ea M $f8eb M $f8ec M -> $88 $7c $34 $53 $be }T
$f8c8 >PC $dd >S $59 >A $af >X $19 >Y $e1 >P $000a $90 >M $005b $26 >M $f8c8 $34 >M $f8c9 $5b >M $f8ca $31 >M
T{ op PC S A X Y P -> $f8ca $dd $59 $af $19 $a1 }T T{ $000a M $005b M $f8c8 M $f8c9 M $f8ca M -> $90 $26 $34 $5b $31 }T
$d969 >PC $11 >S $ed >A $de >X $20 >Y $a1 >P $00ae $90 >M $00d0 $c4 >M $d969 $34 >M $d96a $d0 >M $d96b $85 >M
T{ op PC S A X Y P -> $d96b $11 $ed $de $20 $a1 }T T{ $00ae M $00d0 M $d969 M $d96a M $d96b M -> $90 $c4 $34 $d0 $85 }T
$2f8f >PC $13 >S $d4 >A $7f >X $dc >Y $e1 >P $0062 $88 >M $00e3 $0d >M $2f8f $34 >M $2f90 $e3 >M $2f91 $6b >M
T{ op PC S A X Y P -> $2f91 $13 $d4 $7f $dc $a1 }T T{ $0062 M $00e3 M $2f8f M $2f90 M $2f91 M -> $88 $0d $34 $e3 $6b }T
( 35 )
$3ad8 >PC $58 >S $29 >A $cf >X $d4 >Y $e1 >P $0002 $74 >M $0033 $48 >M $3ad8 $35 >M $3ad9 $33 >M $3ada $59 >M
T{ op PC S A X Y P -> $3ada $58 $20 $cf $d4 $61 }T T{ $0002 M $0033 M $3ad8 M $3ad9 M $3ada M -> $74 $48 $35 $33 $59 }T
$c550 >PC $ab >S $a0 >A $51 >X $e9 >Y $a1 >P $006e $14 >M $00bf $0a >M $c550 $35 >M $c551 $6e >M $c552 $ba >M
T{ op PC S A X Y P -> $c552 $ab $00 $51 $e9 $23 }T T{ $006e M $00bf M $c550 M $c551 M $c552 M -> $14 $0a $35 $6e $ba }T
$df3a >PC $ce >S $04 >A $a6 >X $ea >Y $e6 >P $001d $22 >M $0077 $69 >M $df3a $35 >M $df3b $77 >M $df3c $7c >M
T{ op PC S A X Y P -> $df3c $ce $00 $a6 $ea $66 }T T{ $001d M $0077 M $df3a M $df3b M $df3c M -> $22 $69 $35 $77 $7c }T
$79fd >PC $4c >S $19 >A $b1 >X $ff >Y $a5 >P $0042 $2c >M $0091 $70 >M $79fd $35 >M $79fe $91 >M $79ff $32 >M
T{ op PC S A X Y P -> $79ff $4c $08 $b1 $ff $25 }T T{ $0042 M $0091 M $79fd M $79fe M $79ff M -> $2c $70 $35 $91 $32 }T
$ba16 >PC $6e >S $23 >A $12 >X $f9 >Y $21 >P $00df $a4 >M $00f1 $54 >M $ba16 $35 >M $ba17 $df >M $ba18 $e1 >M
T{ op PC S A X Y P -> $ba18 $6e $00 $12 $f9 $23 }T T{ $00df M $00f1 M $ba16 M $ba17 M $ba18 M -> $a4 $54 $35 $df $e1 }T
$1ea8 >PC $1d >S $ca >A $46 >X $da >Y $66 >P $0033 $6b >M $00ed $a2 >M $1ea8 $35 >M $1ea9 $ed >M $1eaa $e5 >M
T{ op PC S A X Y P -> $1eaa $1d $4a $46 $da $64 }T T{ $0033 M $00ed M $1ea8 M $1ea9 M $1eaa M -> $6b $a2 $35 $ed $e5 }T
$7433 >PC $44 >S $02 >A $9e >X $f3 >Y $a7 >P $0002 $05 >M $00a0 $09 >M $7433 $35 >M $7434 $02 >M $7435 $6d >M
T{ op PC S A X Y P -> $7435 $44 $00 $9e $f3 $27 }T T{ $0002 M $00a0 M $7433 M $7434 M $7435 M -> $05 $09 $35 $02 $6d }T
$563d >PC $bf >S $a8 >A $fc >X $42 >Y $a6 >P $0051 $56 >M $0055 $58 >M $563d $35 >M $563e $55 >M $563f $50 >M
T{ op PC S A X Y P -> $563f $bf $00 $fc $42 $26 }T T{ $0051 M $0055 M $563d M $563e M $563f M -> $56 $58 $35 $55 $50 }T
$acda >PC $71 >S $38 >A $0c >X $ab >Y $e4 >P $00e5 $77 >M $00f1 $15 >M $acda $35 >M $acdb $e5 >M $acdc $f9 >M
T{ op PC S A X Y P -> $acdc $71 $10 $0c $ab $64 }T T{ $00e5 M $00f1 M $acda M $acdb M $acdc M -> $77 $15 $35 $e5 $f9 }T
$c712 >PC $19 >S $5c >A $97 >X $1b >Y $a2 >P $0047 $9f >M $00de $e4 >M $c712 $35 >M $c713 $47 >M $c714 $e6 >M
T{ op PC S A X Y P -> $c714 $19 $44 $97 $1b $20 }T T{ $0047 M $00de M $c712 M $c713 M $c714 M -> $9f $e4 $35 $47 $e6 }T
$8567 >PC $9d >S $a3 >A $3d >X $d2 >Y $66 >P $0049 $fd >M $0086 $de >M $8567 $35 >M $8568 $49 >M $8569 $f8 >M
T{ op PC S A X Y P -> $8569 $9d $82 $3d $d2 $e4 }T T{ $0049 M $0086 M $8567 M $8568 M $8569 M -> $fd $de $35 $49 $f8 }T
$2ec0 >PC $fd >S $f1 >A $6e >X $e8 >Y $e0 >P $0079 $ee >M $00e7 $a4 >M $2ec0 $35 >M $2ec1 $79 >M $2ec2 $b3 >M
T{ op PC S A X Y P -> $2ec2 $fd $a0 $6e $e8 $e0 }T T{ $0079 M $00e7 M $2ec0 M $2ec1 M $2ec2 M -> $ee $a4 $35 $79 $b3 }T
$b8be >PC $9d >S $e5 >A $f4 >X $46 >Y $26 >P $00bd $e0 >M $00c9 $42 >M $b8be $35 >M $b8bf $c9 >M $b8c0 $a4 >M
T{ op PC S A X Y P -> $b8c0 $9d $e0 $f4 $46 $a4 }T T{ $00bd M $00c9 M $b8be M $b8bf M $b8c0 M -> $e0 $42 $35 $c9 $a4 }T
$42d3 >PC $0a >S $5e >A $45 >X $41 >Y $e3 >P $0072 $53 >M $00b7 $95 >M $42d3 $35 >M $42d4 $72 >M $42d5 $a1 >M
T{ op PC S A X Y P -> $42d5 $0a $14 $45 $41 $61 }T T{ $0072 M $00b7 M $42d3 M $42d4 M $42d5 M -> $53 $95 $35 $72 $a1 }T
$e022 >PC $39 >S $77 >A $70 >X $68 >Y $21 >P $0052 $f0 >M $00c2 $d3 >M $e022 $35 >M $e023 $52 >M $e024 $33 >M
T{ op PC S A X Y P -> $e024 $39 $53 $70 $68 $21 }T T{ $0052 M $00c2 M $e022 M $e023 M $e024 M -> $f0 $d3 $35 $52 $33 }T
$f911 >PC $95 >S $a1 >A $7c >X $ec >Y $67 >P $0072 $d1 >M $00f6 $61 >M $f911 $35 >M $f912 $f6 >M $f913 $33 >M
T{ op PC S A X Y P -> $f913 $95 $81 $7c $ec $e5 }T T{ $0072 M $00f6 M $f911 M $f912 M $f913 M -> $d1 $61 $35 $f6 $33 }T
( 36 )
$7ab0 >PC $a3 >S $69 >A $c0 >X $37 >Y $21 >P $0089 $af >M $00c9 $f4 >M $7ab0 $36 >M $7ab1 $c9 >M $7ab2 $d9 >M
T{ op PC S A X Y P -> $7ab2 $a3 $69 $c0 $37 $21 }T T{ $0089 M $00c9 M $7ab0 M $7ab1 M $7ab2 M -> $5f $f4 $36 $c9 $d9 }T
$be0d >PC $7c >S $4d >A $62 >X $33 >Y $e6 >P $006e $14 >M $00d0 $fc >M $be0d $36 >M $be0e $6e >M $be0f $eb >M
T{ op PC S A X Y P -> $be0f $7c $4d $62 $33 $e5 }T T{ $006e M $00d0 M $be0d M $be0e M $be0f M -> $14 $f8 $36 $6e $eb }T
$1c43 >PC $3c >S $66 >A $e3 >X $2d >Y $e6 >P $0097 $f5 >M $00b4 $02 >M $1c43 $36 >M $1c44 $b4 >M $1c45 $3b >M
T{ op PC S A X Y P -> $1c45 $3c $66 $e3 $2d $e5 }T T{ $0097 M $00b4 M $1c43 M $1c44 M $1c45 M -> $ea $02 $36 $b4 $3b }T
$46df >PC $80 >S $09 >A $e9 >X $73 >Y $a3 >P $006f $9e >M $0086 $43 >M $46df $36 >M $46e0 $86 >M $46e1 $6a >M
T{ op PC S A X Y P -> $46e1 $80 $09 $e9 $73 $21 }T T{ $006f M $0086 M $46df M $46e0 M $46e1 M -> $3d $43 $36 $86 $6a }T
$c486 >PC $92 >S $85 >A $74 >X $e1 >Y $e6 >P $0059 $25 >M $00e5 $b7 >M $c486 $36 >M $c487 $e5 >M $c488 $32 >M
T{ op PC S A X Y P -> $c488 $92 $85 $74 $e1 $64 }T T{ $0059 M $00e5 M $c486 M $c487 M $c488 M -> $4a $b7 $36 $e5 $32 }T
$c2b5 >PC $23 >S $75 >A $fb >X $32 >Y $e6 >P $00e3 $01 >M $00e8 $50 >M $c2b5 $36 >M $c2b6 $e8 >M $c2b7 $eb >M
T{ op PC S A X Y P -> $c2b7 $23 $75 $fb $32 $64 }T T{ $00e3 M $00e8 M $c2b5 M $c2b6 M $c2b7 M -> $02 $50 $36 $e8 $eb }T
$1c21 >PC $00 >S $c8 >A $ae >X $91 >Y $21 >P $0046 $d8 >M $00f4 $52 >M $1c21 $36 >M $1c22 $46 >M $1c23 $e2 >M
T{ op PC S A X Y P -> $1c23 $00 $c8 $ae $91 $a0 }T T{ $0046 M $00f4 M $1c21 M $1c22 M $1c23 M -> $d8 $a5 $36 $46 $e2 }T
$5cf9 >PC $e0 >S $a6 >A $f8 >X $e5 >Y $a6 >P $0045 $49 >M $004d $cc >M $5cf9 $36 >M $5cfa $4d >M $5cfb $ea >M
T{ op PC S A X Y P -> $5cfb $e0 $a6 $f8 $e5 $a4 }T T{ $0045 M $004d M $5cf9 M $5cfa M $5cfb M -> $92 $cc $36 $4d $ea }T
$7257 >PC $75 >S $e6 >A $94 >X $6e >Y $63 >P $0062 $9d >M $00f6 $c7 >M $7257 $36 >M $7258 $62 >M $7259 $a9 >M
T{ op PC S A X Y P -> $7259 $75 $e6 $94 $6e $e1 }T T{ $0062 M $00f6 M $7257 M $7258 M $7259 M -> $9d $8f $36 $62 $a9 }T
$abb6 >PC $8b >S $9f >A $a9 >X $93 >Y $e2 >P $0002 $e9 >M $00ab $f8 >M $abb6 $36 >M $abb7 $02 >M $abb8 $10 >M
T{ op PC S A X Y P -> $abb8 $8b $9f $a9 $93 $e1 }T T{ $0002 M $00ab M $abb6 M $abb7 M $abb8 M -> $e9 $f0 $36 $02 $10 }T
$e2bf >PC $71 >S $3e >A $06 >X $3b >Y $20 >P $008e $5b >M $0094 $8b >M $e2bf $36 >M $e2c0 $8e >M $e2c1 $92 >M
T{ op PC S A X Y P -> $e2c1 $71 $3e $06 $3b $21 }T T{ $008e M $0094 M $e2bf M $e2c0 M $e2c1 M -> $5b $16 $36 $8e $92 }T
$bbbb >PC $b3 >S $15 >A $b1 >X $de >Y $e0 >P $0010 $00 >M $00c1 $59 >M $bbbb $36 >M $bbbc $10 >M $bbbd $37 >M
T{ op PC S A X Y P -> $bbbd $b3 $15 $b1 $de $e0 }T T{ $0010 M $00c1 M $bbbb M $bbbc M $bbbd M -> $00 $b2 $36 $10 $37 }T
$8eb2 >PC $c5 >S $10 >A $24 >X $12 >Y $a2 >P $0032 $a8 >M $0056 $0c >M $8eb2 $36 >M $8eb3 $32 >M $8eb4 $5a >M
T{ op PC S A X Y P -> $8eb4 $c5 $10 $24 $12 $20 }T T{ $0032 M $0056 M $8eb2 M $8eb3 M $8eb4 M -> $a8 $18 $36 $32 $5a }T
$7b1e >PC $ae >S $e4 >A $8b >X $47 >Y $22 >P $0021 $93 >M $00ac $81 >M $7b1e $36 >M $7b1f $21 >M $7b20 $80 >M
T{ op PC S A X Y P -> $7b20 $ae $e4 $8b $47 $21 }T T{ $0021 M $00ac M $7b1e M $7b1f M $7b20 M -> $93 $02 $36 $21 $80 }T
$2168 >PC $c8 >S $73 >A $d8 >X $c9 >Y $60 >P $0025 $ea >M $004d $7d >M $2168 $36 >M $2169 $4d >M $216a $f8 >M
T{ op PC S A X Y P -> $216a $c8 $73 $d8 $c9 $e1 }T T{ $0025 M $004d M $2168 M $2169 M $216a M -> $d4 $7d $36 $4d $f8 }T
$0f0d >PC $e8 >S $b0 >A $ae >X $b1 >Y $61 >P $0017 $b8 >M $0069 $ec >M $0f0d $36 >M $0f0e $69 >M $0f0f $b7 >M
T{ op PC S A X Y P -> $0f0f $e8 $b0 $ae $b1 $61 }T T{ $0017 M $0069 M $0f0d M $0f0e M $0f0f M -> $71 $ec $36 $69 $b7 }T
( 37 )
$9ef6 >PC $86 >S $93 >A $64 >X $a5 >Y $27 >P $00d9 $ad >M $9ef6 $37 >M $9ef7 $d9 >M $9ef8 $16 >M
T{ op PC S A X Y P -> $9ef8 $86 $93 $64 $a5 $27 }T T{ $00d9 M $9ef6 M $9ef7 M $9ef8 M -> $a5 $37 $d9 $16 }T
$16e2 >PC $fa >S $ab >A $17 >X $c9 >Y $67 >P $0056 $7b >M $16e2 $37 >M $16e3 $56 >M $16e4 $0f >M
T{ op PC S A X Y P -> $16e4 $fa $ab $17 $c9 $67 }T T{ $0056 M $16e2 M $16e3 M $16e4 M -> $73 $37 $56 $0f }T
$ee18 >PC $ac >S $44 >A $d0 >X $88 >Y $e3 >P $0096 $ca >M $ee18 $37 >M $ee19 $96 >M $ee1a $1f >M
T{ op PC S A X Y P -> $ee1a $ac $44 $d0 $88 $e3 }T T{ $0096 M $ee18 M $ee19 M $ee1a M -> $c2 $37 $96 $1f }T
$4afb >PC $44 >S $b1 >A $1c >X $bb >Y $a6 >P $00b6 $33 >M $4afb $37 >M $4afc $b6 >M $4afd $f7 >M
T{ op PC S A X Y P -> $4afd $44 $b1 $1c $bb $a6 }T T{ $00b6 M $4afb M $4afc M $4afd M -> $33 $37 $b6 $f7 }T
$9acd >PC $a7 >S $38 >A $ca >X $c6 >Y $e0 >P $00fa $7c >M $9acd $37 >M $9ace $fa >M $9acf $0c >M
T{ op PC S A X Y P -> $9acf $a7 $38 $ca $c6 $e0 }T T{ $00fa M $9acd M $9ace M $9acf M -> $74 $37 $fa $0c }T
$0190 >PC $7d >S $4a >A $68 >X $54 >Y $61 >P $0020 $9c >M $0190 $37 >M $0191 $20 >M $0192 $8d >M
T{ op PC S A X Y P -> $0192 $7d $4a $68 $54 $61 }T T{ $0020 M $0190 M $0191 M $0192 M -> $94 $37 $20 $8d }T
$6ab6 >PC $e6 >S $7b >A $8c >X $03 >Y $63 >P $0094 $5c >M $6ab6 $37 >M $6ab7 $94 >M $6ab8 $53 >M
T{ op PC S A X Y P -> $6ab8 $e6 $7b $8c $03 $63 }T T{ $0094 M $6ab6 M $6ab7 M $6ab8 M -> $54 $37 $94 $53 }T
$27c3 >PC $31 >S $cc >A $8c >X $a7 >Y $a1 >P $009b $44 >M $27c3 $37 >M $27c4 $9b >M $27c5 $64 >M
T{ op PC S A X Y P -> $27c5 $31 $cc $8c $a7 $a1 }T T{ $009b M $27c3 M $27c4 M $27c5 M -> $44 $37 $9b $64 }T
$a93e >PC $f1 >S $20 >A $d0 >X $f7 >Y $24 >P $0030 $11 >M $a93e $37 >M $a93f $30 >M $a940 $56 >M
T{ op PC S A X Y P -> $a940 $f1 $20 $d0 $f7 $24 }T T{ $0030 M $a93e M $a93f M $a940 M -> $11 $37 $30 $56 }T
$1f5a >PC $7e >S $dc >A $ff >X $cf >Y $63 >P $0078 $de >M $1f5a $37 >M $1f5b $78 >M $1f5c $11 >M
T{ op PC S A X Y P -> $1f5c $7e $dc $ff $cf $63 }T T{ $0078 M $1f5a M $1f5b M $1f5c M -> $d6 $37 $78 $11 }T
$afc9 >PC $a8 >S $52 >A $5d >X $45 >Y $61 >P $003a $24 >M $afc9 $37 >M $afca $3a >M $afcb $27 >M
T{ op PC S A X Y P -> $afcb $a8 $52 $5d $45 $61 }T T{ $003a M $afc9 M $afca M $afcb M -> $24 $37 $3a $27 }T
$83bf >PC $70 >S $8b >A $3b >X $c6 >Y $64 >P $0056 $cc >M $83bf $37 >M $83c0 $56 >M $83c1 $60 >M
T{ op PC S A X Y P -> $83c1 $70 $8b $3b $c6 $64 }T T{ $0056 M $83bf M $83c0 M $83c1 M -> $c4 $37 $56 $60 }T
$e548 >PC $74 >S $80 >A $0e >X $e0 >Y $a1 >P $007a $7e >M $e548 $37 >M $e549 $7a >M $e54a $50 >M
T{ op PC S A X Y P -> $e54a $74 $80 $0e $e0 $a1 }T T{ $007a M $e548 M $e549 M $e54a M -> $76 $37 $7a $50 }T
$1650 >PC $9f >S $f7 >A $a2 >X $f4 >Y $65 >P $00bb $10 >M $1650 $37 >M $1651 $bb >M $1652 $c7 >M
T{ op PC S A X Y P -> $1652 $9f $f7 $a2 $f4 $65 }T T{ $00bb M $1650 M $1651 M $1652 M -> $10 $37 $bb $c7 }T
$7f77 >PC $ce >S $6e >A $4f >X $da >Y $e1 >P $00c1 $a6 >M $7f77 $37 >M $7f78 $c1 >M $7f79 $68 >M
T{ op PC S A X Y P -> $7f79 $ce $6e $4f $da $e1 }T T{ $00c1 M $7f77 M $7f78 M $7f79 M -> $a6 $37 $c1 $68 }T
$940c >PC $f3 >S $bb >A $a5 >X $a4 >Y $e7 >P $00a4 $46 >M $940c $37 >M $940d $a4 >M $940e $1a >M
T{ op PC S A X Y P -> $940e $f3 $bb $a5 $a4 $e7 }T T{ $00a4 M $940c M $940d M $940e M -> $46 $37 $a4 $1a }T
( 38 )
$93cc >PC $24 >S $2a >A $ba >X $31 >Y $e7 >P $93cc $38 >M $93cd $be >M $93ce $c1 >M
T{ op PC S A X Y P -> $93cd $24 $2a $ba $31 $e7 }T T{ $93cc M $93cd M $93ce M -> $38 $be $c1 }T
$3fba >PC $c2 >S $55 >A $9a >X $02 >Y $e5 >P $3fba $38 >M $3fbb $d3 >M $3fbc $25 >M
T{ op PC S A X Y P -> $3fbb $c2 $55 $9a $02 $e5 }T T{ $3fba M $3fbb M $3fbc M -> $38 $d3 $25 }T
$d8eb >PC $25 >S $27 >A $b1 >X $e4 >Y $25 >P $d8eb $38 >M $d8ec $0f >M $d8ed $b3 >M
T{ op PC S A X Y P -> $d8ec $25 $27 $b1 $e4 $25 }T T{ $d8eb M $d8ec M $d8ed M -> $38 $0f $b3 }T
$ab44 >PC $86 >S $71 >A $cd >X $ad >Y $24 >P $ab44 $38 >M $ab45 $03 >M $ab46 $34 >M
T{ op PC S A X Y P -> $ab45 $86 $71 $cd $ad $25 }T T{ $ab44 M $ab45 M $ab46 M -> $38 $03 $34 }T
$24ac >PC $e8 >S $ff >A $99 >X $62 >Y $e3 >P $24ac $38 >M $24ad $ba >M $24ae $69 >M
T{ op PC S A X Y P -> $24ad $e8 $ff $99 $62 $e3 }T T{ $24ac M $24ad M $24ae M -> $38 $ba $69 }T
$a5d9 >PC $05 >S $0e >A $96 >X $23 >Y $a5 >P $a5d9 $38 >M $a5da $90 >M $a5db $39 >M
T{ op PC S A X Y P -> $a5da $05 $0e $96 $23 $a5 }T T{ $a5d9 M $a5da M $a5db M -> $38 $90 $39 }T
$42ac >PC $d8 >S $3f >A $0e >X $9e >Y $e7 >P $42ac $38 >M $42ad $bc >M $42ae $5e >M
T{ op PC S A X Y P -> $42ad $d8 $3f $0e $9e $e7 }T T{ $42ac M $42ad M $42ae M -> $38 $bc $5e }T
$d6c0 >PC $7c >S $91 >A $c1 >X $55 >Y $e0 >P $d6c0 $38 >M $d6c1 $5b >M $d6c2 $98 >M
T{ op PC S A X Y P -> $d6c1 $7c $91 $c1 $55 $e1 }T T{ $d6c0 M $d6c1 M $d6c2 M -> $38 $5b $98 }T
$e3d4 >PC $be >S $89 >A $d4 >X $99 >Y $e6 >P $e3d4 $38 >M $e3d5 $a1 >M $e3d6 $78 >M
T{ op PC S A X Y P -> $e3d5 $be $89 $d4 $99 $e7 }T T{ $e3d4 M $e3d5 M $e3d6 M -> $38 $a1 $78 }T
$737a >PC $35 >S $29 >A $97 >X $70 >Y $25 >P $737a $38 >M $737b $49 >M $737c $89 >M
T{ op PC S A X Y P -> $737b $35 $29 $97 $70 $25 }T T{ $737a M $737b M $737c M -> $38 $49 $89 }T
$a4cc >PC $53 >S $56 >A $91 >X $4a >Y $65 >P $a4cc $38 >M $a4cd $bd >M $a4ce $53 >M
T{ op PC S A X Y P -> $a4cd $53 $56 $91 $4a $65 }T T{ $a4cc M $a4cd M $a4ce M -> $38 $bd $53 }T
$2089 >PC $f6 >S $d1 >A $b4 >X $cb >Y $a6 >P $2089 $38 >M $208a $20 >M $208b $b8 >M
T{ op PC S A X Y P -> $208a $f6 $d1 $b4 $cb $a7 }T T{ $2089 M $208a M $208b M -> $38 $20 $b8 }T
$c446 >PC $fe >S $77 >A $b3 >X $b0 >Y $66 >P $c446 $38 >M $c447 $69 >M $c448 $dc >M
T{ op PC S A X Y P -> $c447 $fe $77 $b3 $b0 $67 }T T{ $c446 M $c447 M $c448 M -> $38 $69 $dc }T
$7ca1 >PC $fb >S $7f >A $9c >X $cd >Y $27 >P $7ca1 $38 >M $7ca2 $c5 >M $7ca3 $62 >M
T{ op PC S A X Y P -> $7ca2 $fb $7f $9c $cd $27 }T T{ $7ca1 M $7ca2 M $7ca3 M -> $38 $c5 $62 }T
$9914 >PC $69 >S $f9 >A $77 >X $2b >Y $e0 >P $9914 $38 >M $9915 $fc >M $9916 $7a >M
T{ op PC S A X Y P -> $9915 $69 $f9 $77 $2b $e1 }T T{ $9914 M $9915 M $9916 M -> $38 $fc $7a }T
$ede4 >PC $15 >S $7d >A $d7 >X $ac >Y $21 >P $ede4 $38 >M $ede5 $d3 >M $ede6 $07 >M
T{ op PC S A X Y P -> $ede5 $15 $7d $d7 $ac $21 }T T{ $ede4 M $ede5 M $ede6 M -> $38 $d3 $07 }T
( 39 )
$28bb >PC $39 >S $72 >A $6e >X $1b >Y $65 >P $28bb $39 >M $28bc $33 >M $28bd $58 >M $28be $0b >M $584e $3d >M
T{ op PC S A X Y P -> $28be $39 $30 $6e $1b $65 }T T{ $28bb M $28bc M $28bd M $28be M $584e M -> $39 $33 $58 $0b $3d }T
$2143 >PC $30 >S $3a >A $ae >X $fa >Y $61 >P $2143 $39 >M $2144 $3e >M $2145 $42 >M $2146 $ea >M $4338 $67 >M
T{ op PC S A X Y P -> $2146 $30 $22 $ae $fa $61 }T T{ $2143 M $2144 M $2145 M $2146 M $4338 M -> $39 $3e $42 $ea $67 }T
$e14a >PC $da >S $5a >A $f5 >X $8d >Y $26 >P $81b3 $db >M $e14a $39 >M $e14b $26 >M $e14c $81 >M $e14d $22 >M
T{ op PC S A X Y P -> $e14d $da $5a $f5 $8d $24 }T T{ $81b3 M $e14a M $e14b M $e14c M $e14d M -> $db $39 $26 $81 $22 }T
$4798 >PC $67 >S $78 >A $ac >X $75 >Y $22 >P $4798 $39 >M $4799 $ef >M $479a $fd >M $479b $3a >M $fe64 $e3 >M
T{ op PC S A X Y P -> $479b $67 $60 $ac $75 $20 }T T{ $4798 M $4799 M $479a M $479b M $fe64 M -> $39 $ef $fd $3a $e3 }T
$41d7 >PC $95 >S $fb >A $c8 >X $74 >Y $e5 >P $41d7 $39 >M $41d8 $e8 >M $41d9 $5a >M $41da $47 >M $5b5c $c4 >M
T{ op PC S A X Y P -> $41da $95 $c0 $c8 $74 $e5 }T T{ $41d7 M $41d8 M $41d9 M $41da M $5b5c M -> $39 $e8 $5a $47 $c4 }T
$b087 >PC $e1 >S $7a >A $6f >X $e9 >Y $21 >P $5a69 $c1 >M $b087 $39 >M $b088 $80 >M $b089 $59 >M $b08a $ec >M
T{ op PC S A X Y P -> $b08a $e1 $40 $6f $e9 $21 }T T{ $5a69 M $b087 M $b088 M $b089 M $b08a M -> $c1 $39 $80 $59 $ec }T
$ef7a >PC $9c >S $78 >A $c0 >X $52 >Y $65 >P $acc0 $7d >M $ef7a $39 >M $ef7b $6e >M $ef7c $ac >M $ef7d $c8 >M
T{ op PC S A X Y P -> $ef7d $9c $78 $c0 $52 $65 }T T{ $acc0 M $ef7a M $ef7b M $ef7c M $ef7d M -> $7d $39 $6e $ac $c8 }T
$f0c8 >PC $24 >S $bb >A $04 >X $c8 >Y $e3 >P $431c $3f >M $f0c8 $39 >M $f0c9 $54 >M $f0ca $42 >M $f0cb $4a >M
T{ op PC S A X Y P -> $f0cb $24 $3b $04 $c8 $61 }T T{ $431c M $f0c8 M $f0c9 M $f0ca M $f0cb M -> $3f $39 $54 $42 $4a }T
$60d3 >PC $93 >S $b5 >A $5b >X $e8 >Y $a2 >P $60d3 $39 >M $60d4 $d5 >M $60d5 $66 >M $60d6 $08 >M $67bd $c9 >M
T{ op PC S A X Y P -> $60d6 $93 $81 $5b $e8 $a0 }T T{ $60d3 M $60d4 M $60d5 M $60d6 M $67bd M -> $39 $d5 $66 $08 $c9 }T
$e832 >PC $e3 >S $7c >A $53 >X $12 >Y $a4 >P $72c2 $85 >M $e832 $39 >M $e833 $b0 >M $e834 $72 >M $e835 $b7 >M
T{ op PC S A X Y P -> $e835 $e3 $04 $53 $12 $24 }T T{ $72c2 M $e832 M $e833 M $e834 M $e835 M -> $85 $39 $b0 $72 $b7 }T
$22c9 >PC $6b >S $a3 >A $32 >X $31 >Y $a5 >P $22c9 $39 >M $22ca $c0 >M $22cb $95 >M $22cc $62 >M $95f1 $28 >M
T{ op PC S A X Y P -> $22cc $6b $20 $32 $31 $25 }T T{ $22c9 M $22ca M $22cb M $22cc M $95f1 M -> $39 $c0 $95 $62 $28 }T
$a4ef >PC $05 >S $25 >A $f8 >X $d9 >Y $a6 >P $0e62 $d3 >M $a4ef $39 >M $a4f0 $89 >M $a4f1 $0d >M $a4f2 $48 >M
T{ op PC S A X Y P -> $a4f2 $05 $01 $f8 $d9 $24 }T T{ $0e62 M $a4ef M $a4f0 M $a4f1 M $a4f2 M -> $d3 $39 $89 $0d $48 }T
$25d2 >PC $b2 >S $3c >A $45 >X $b5 >Y $e1 >P $25d2 $39 >M $25d3 $fd >M $25d4 $c7 >M $25d5 $0b >M $c8b2 $ad >M
T{ op PC S A X Y P -> $25d5 $b2 $2c $45 $b5 $61 }T T{ $25d2 M $25d3 M $25d4 M $25d5 M $c8b2 M -> $39 $fd $c7 $0b $ad }T
$df59 >PC $ff >S $39 >A $c6 >X $0b >Y $27 >P $d22e $e3 >M $df59 $39 >M $df5a $23 >M $df5b $d2 >M $df5c $24 >M
T{ op PC S A X Y P -> $df5c $ff $21 $c6 $0b $25 }T T{ $d22e M $df59 M $df5a M $df5b M $df5c M -> $e3 $39 $23 $d2 $24 }T
$c04f >PC $8c >S $cf >A $10 >X $ea >Y $a5 >P $a287 $e0 >M $c04f $39 >M $c050 $9d >M $c051 $a1 >M $c052 $09 >M
T{ op PC S A X Y P -> $c052 $8c $c0 $10 $ea $a5 }T T{ $a287 M $c04f M $c050 M $c051 M $c052 M -> $e0 $39 $9d $a1 $09 }T
$58e7 >PC $df >S $2c >A $c3 >X $b8 >Y $a6 >P $1319 $7d >M $58e7 $39 >M $58e8 $61 >M $58e9 $12 >M $58ea $82 >M
T{ op PC S A X Y P -> $58ea $df $2c $c3 $b8 $24 }T T{ $1319 M $58e7 M $58e8 M $58e9 M $58ea M -> $7d $39 $61 $12 $82 }T
( 3a )
$426e >PC $72 >S $04 >A $31 >X $3d >Y $e2 >P $426e $3a >M $426f $e5 >M $4270 $e2 >M
T{ op PC S A X Y P -> $426f $72 $03 $31 $3d $60 }T T{ $426e M $426f M $4270 M -> $3a $e5 $e2 }T
$7ab8 >PC $68 >S $d7 >A $3c >X $a0 >Y $23 >P $7ab8 $3a >M $7ab9 $1b >M $7aba $59 >M
T{ op PC S A X Y P -> $7ab9 $68 $d6 $3c $a0 $a1 }T T{ $7ab8 M $7ab9 M $7aba M -> $3a $1b $59 }T
$a979 >PC $3b >S $39 >A $c9 >X $87 >Y $a5 >P $a979 $3a >M $a97a $22 >M $a97b $17 >M
T{ op PC S A X Y P -> $a97a $3b $38 $c9 $87 $25 }T T{ $a979 M $a97a M $a97b M -> $3a $22 $17 }T
$7e23 >PC $81 >S $66 >A $69 >X $fc >Y $65 >P $7e23 $3a >M $7e24 $96 >M $7e25 $54 >M
T{ op PC S A X Y P -> $7e24 $81 $65 $69 $fc $65 }T T{ $7e23 M $7e24 M $7e25 M -> $3a $96 $54 }T
$fa28 >PC $a9 >S $e4 >A $54 >X $9f >Y $a2 >P $fa28 $3a >M $fa29 $75 >M $fa2a $94 >M
T{ op PC S A X Y P -> $fa29 $a9 $e3 $54 $9f $a0 }T T{ $fa28 M $fa29 M $fa2a M -> $3a $75 $94 }T
$8f03 >PC $ab >S $62 >A $ec >X $b6 >Y $e6 >P $8f03 $3a >M $8f04 $6b >M $8f05 $5b >M
T{ op PC S A X Y P -> $8f04 $ab $61 $ec $b6 $64 }T T{ $8f03 M $8f04 M $8f05 M -> $3a $6b $5b }T
$7172 >PC $d7 >S $d3 >A $ba >X $fd >Y $21 >P $7172 $3a >M $7173 $9f >M $7174 $36 >M
T{ op PC S A X Y P -> $7173 $d7 $d2 $ba $fd $a1 }T T{ $7172 M $7173 M $7174 M -> $3a $9f $36 }T
$296e >PC $5e >S $0a >A $1e >X $b0 >Y $e1 >P $296e $3a >M $296f $54 >M $2970 $9d >M
T{ op PC S A X Y P -> $296f $5e $09 $1e $b0 $61 }T T{ $296e M $296f M $2970 M -> $3a $54 $9d }T
$368b >PC $f7 >S $96 >A $d8 >X $79 >Y $e4 >P $368b $3a >M $368c $9a >M $368d $49 >M
T{ op PC S A X Y P -> $368c $f7 $95 $d8 $79 $e4 }T T{ $368b M $368c M $368d M -> $3a $9a $49 }T
$135c >PC $e9 >S $ea >A $c0 >X $cc >Y $21 >P $135c $3a >M $135d $e6 >M $135e $1c >M
T{ op PC S A X Y P -> $135d $e9 $e9 $c0 $cc $a1 }T T{ $135c M $135d M $135e M -> $3a $e6 $1c }T
$a6b5 >PC $e6 >S $fa >A $0b >X $a2 >Y $25 >P $a6b5 $3a >M $a6b6 $fe >M $a6b7 $68 >M
T{ op PC S A X Y P -> $a6b6 $e6 $f9 $0b $a2 $a5 }T T{ $a6b5 M $a6b6 M $a6b7 M -> $3a $fe $68 }T
$f5c3 >PC $7d >S $8a >A $87 >X $04 >Y $26 >P $f5c3 $3a >M $f5c4 $1e >M $f5c5 $4a >M
T{ op PC S A X Y P -> $f5c4 $7d $89 $87 $04 $a4 }T T{ $f5c3 M $f5c4 M $f5c5 M -> $3a $1e $4a }T
$9714 >PC $d3 >S $3b >A $bf >X $4e >Y $63 >P $9714 $3a >M $9715 $34 >M $9716 $b2 >M
T{ op PC S A X Y P -> $9715 $d3 $3a $bf $4e $61 }T T{ $9714 M $9715 M $9716 M -> $3a $34 $b2 }T
$86d6 >PC $d8 >S $68 >A $54 >X $79 >Y $66 >P $86d6 $3a >M $86d7 $b2 >M $86d8 $f8 >M
T{ op PC S A X Y P -> $86d7 $d8 $67 $54 $79 $64 }T T{ $86d6 M $86d7 M $86d8 M -> $3a $b2 $f8 }T
$5ed8 >PC $0e >S $1b >A $83 >X $7b >Y $a1 >P $5ed8 $3a >M $5ed9 $92 >M $5eda $72 >M
T{ op PC S A X Y P -> $5ed9 $0e $1a $83 $7b $21 }T T{ $5ed8 M $5ed9 M $5eda M -> $3a $92 $72 }T
$9943 >PC $d9 >S $76 >A $e4 >X $6c >Y $23 >P $9943 $3a >M $9944 $63 >M $9945 $b3 >M
T{ op PC S A X Y P -> $9944 $d9 $75 $e4 $6c $21 }T T{ $9943 M $9944 M $9945 M -> $3a $63 $b3 }T
( 3b )
$41ea >PC $d8 >S $3c >A $97 >X $f9 >Y $e3 >P $41ea $3b >M $41eb $89 >M $41ec $80 >M
T{ op PC S A X Y P -> $41eb $d8 $3c $97 $f9 $e3 }T T{ $41ea M $41eb M $41ec M -> $3b $89 $80 }T
$ea1d >PC $e4 >S $43 >A $72 >X $87 >Y $26 >P $ea1d $3b >M $ea1e $f8 >M $ea1f $e6 >M
T{ op PC S A X Y P -> $ea1e $e4 $43 $72 $87 $26 }T T{ $ea1d M $ea1e M $ea1f M -> $3b $f8 $e6 }T
$e939 >PC $1e >S $79 >A $f4 >X $0a >Y $a7 >P $e939 $3b >M $e93a $e7 >M $e93b $78 >M
T{ op PC S A X Y P -> $e93a $1e $79 $f4 $0a $a7 }T T{ $e939 M $e93a M $e93b M -> $3b $e7 $78 }T
$1410 >PC $0c >S $0b >A $f7 >X $79 >Y $e6 >P $1410 $3b >M $1411 $ff >M $1412 $3d >M
T{ op PC S A X Y P -> $1411 $0c $0b $f7 $79 $e6 }T T{ $1410 M $1411 M $1412 M -> $3b $ff $3d }T
$bfac >PC $09 >S $a5 >A $c6 >X $73 >Y $e0 >P $bfac $3b >M $bfad $b0 >M $bfae $f4 >M
T{ op PC S A X Y P -> $bfad $09 $a5 $c6 $73 $e0 }T T{ $bfac M $bfad M $bfae M -> $3b $b0 $f4 }T
$efbb >PC $af >S $43 >A $8a >X $7e >Y $66 >P $efbb $3b >M $efbc $ff >M $efbd $f1 >M
T{ op PC S A X Y P -> $efbc $af $43 $8a $7e $66 }T T{ $efbb M $efbc M $efbd M -> $3b $ff $f1 }T
$d7c8 >PC $04 >S $8f >A $65 >X $d2 >Y $a6 >P $d7c8 $3b >M $d7c9 $b4 >M $d7ca $f3 >M
T{ op PC S A X Y P -> $d7c9 $04 $8f $65 $d2 $a6 }T T{ $d7c8 M $d7c9 M $d7ca M -> $3b $b4 $f3 }T
$7a0b >PC $f7 >S $d2 >A $3d >X $23 >Y $67 >P $7a0b $3b >M $7a0c $1f >M $7a0d $a6 >M
T{ op PC S A X Y P -> $7a0c $f7 $d2 $3d $23 $67 }T T{ $7a0b M $7a0c M $7a0d M -> $3b $1f $a6 }T
$14ae >PC $96 >S $54 >A $fa >X $d9 >Y $63 >P $14ae $3b >M $14af $a5 >M $14b0 $f9 >M
T{ op PC S A X Y P -> $14af $96 $54 $fa $d9 $63 }T T{ $14ae M $14af M $14b0 M -> $3b $a5 $f9 }T
$b1ea >PC $79 >S $90 >A $16 >X $76 >Y $a1 >P $b1ea $3b >M $b1eb $48 >M $b1ec $69 >M
T{ op PC S A X Y P -> $b1eb $79 $90 $16 $76 $a1 }T T{ $b1ea M $b1eb M $b1ec M -> $3b $48 $69 }T
$f039 >PC $d6 >S $e2 >A $cc >X $a4 >Y $65 >P $f039 $3b >M $f03a $86 >M $f03b $6c >M
T{ op PC S A X Y P -> $f03a $d6 $e2 $cc $a4 $65 }T T{ $f039 M $f03a M $f03b M -> $3b $86 $6c }T
$78f5 >PC $7b >S $d9 >A $e3 >X $d1 >Y $a4 >P $78f5 $3b >M $78f6 $60 >M $78f7 $11 >M
T{ op PC S A X Y P -> $78f6 $7b $d9 $e3 $d1 $a4 }T T{ $78f5 M $78f6 M $78f7 M -> $3b $60 $11 }T
$6496 >PC $03 >S $89 >A $e9 >X $45 >Y $21 >P $6496 $3b >M $6497 $93 >M $6498 $99 >M
T{ op PC S A X Y P -> $6497 $03 $89 $e9 $45 $21 }T T{ $6496 M $6497 M $6498 M -> $3b $93 $99 }T
$6272 >PC $6b >S $27 >A $aa >X $66 >Y $27 >P $6272 $3b >M $6273 $b3 >M $6274 $d4 >M
T{ op PC S A X Y P -> $6273 $6b $27 $aa $66 $27 }T T{ $6272 M $6273 M $6274 M -> $3b $b3 $d4 }T
$7138 >PC $2a >S $1f >A $e9 >X $a2 >Y $a2 >P $7138 $3b >M $7139 $64 >M $713a $4f >M
T{ op PC S A X Y P -> $7139 $2a $1f $e9 $a2 $a2 }T T{ $7138 M $7139 M $713a M -> $3b $64 $4f }T
$58cb >PC $14 >S $ea >A $ca >X $8d >Y $a4 >P $58cb $3b >M $58cc $10 >M $58cd $64 >M
T{ op PC S A X Y P -> $58cc $14 $ea $ca $8d $a4 }T T{ $58cb M $58cc M $58cd M -> $3b $10 $64 }T
( 3c )
$d90f >PC $a6 >S $e3 >A $6c >X $7c >Y $e4 >P $8d13 $53 >M $d90f $3c >M $d910 $a7 >M $d911 $8c >M $d912 $9d >M
T{ op PC S A X Y P -> $d912 $a6 $e3 $6c $7c $64 }T T{ $8d13 M $d90f M $d910 M $d911 M $d912 M -> $53 $3c $a7 $8c $9d }T
$46ba >PC $b6 >S $de >A $c2 >X $d9 >Y $62 >P $46ba $3c >M $46bb $c4 >M $46bc $f3 >M $46bd $05 >M $f486 $e6 >M
T{ op PC S A X Y P -> $46bd $b6 $de $c2 $d9 $e0 }T T{ $46ba M $46bb M $46bc M $46bd M $f486 M -> $3c $c4 $f3 $05 $e6 }T
$1ec3 >PC $05 >S $aa >A $bd >X $41 >Y $24 >P $1ec3 $3c >M $1ec4 $02 >M $1ec5 $45 >M $1ec6 $eb >M $45bf $3c >M
T{ op PC S A X Y P -> $1ec6 $05 $aa $bd $41 $24 }T T{ $1ec3 M $1ec4 M $1ec5 M $1ec6 M $45bf M -> $3c $02 $45 $eb $3c }T
$38e1 >PC $b8 >S $c8 >A $37 >X $30 >Y $e1 >P $38e1 $3c >M $38e2 $d9 >M $38e3 $b1 >M $38e4 $c4 >M $b210 $9f >M
T{ op PC S A X Y P -> $38e4 $b8 $c8 $37 $30 $a1 }T T{ $38e1 M $38e2 M $38e3 M $38e4 M $b210 M -> $3c $d9 $b1 $c4 $9f }T
$3a2f >PC $0f >S $33 >A $66 >X $76 >Y $a1 >P $3a2f $3c >M $3a30 $1c >M $3a31 $65 >M $3a32 $d7 >M $6582 $76 >M
T{ op PC S A X Y P -> $3a32 $0f $33 $66 $76 $61 }T T{ $3a2f M $3a30 M $3a31 M $3a32 M $6582 M -> $3c $1c $65 $d7 $76 }T
$a8f1 >PC $d7 >S $7a >A $f4 >X $58 >Y $65 >P $6171 $d7 >M $a8f1 $3c >M $a8f2 $7d >M $a8f3 $60 >M $a8f4 $98 >M
T{ op PC S A X Y P -> $a8f4 $d7 $7a $f4 $58 $e5 }T T{ $6171 M $a8f1 M $a8f2 M $a8f3 M $a8f4 M -> $d7 $3c $7d $60 $98 }T
$7227 >PC $16 >S $8c >A $77 >X $7e >Y $23 >P $496b $09 >M $7227 $3c >M $7228 $f4 >M $7229 $48 >M $722a $13 >M
T{ op PC S A X Y P -> $722a $16 $8c $77 $7e $21 }T T{ $496b M $7227 M $7228 M $7229 M $722a M -> $09 $3c $f4 $48 $13 }T
$55d6 >PC $7e >S $9b >A $12 >X $c6 >Y $60 >P $55d6 $3c >M $55d7 $b3 >M $55d8 $c0 >M $55d9 $96 >M $c0c5 $f8 >M
T{ op PC S A X Y P -> $55d9 $7e $9b $12 $c6 $e0 }T T{ $55d6 M $55d7 M $55d8 M $55d9 M $c0c5 M -> $3c $b3 $c0 $96 $f8 }T
$c593 >PC $72 >S $94 >A $ab >X $28 >Y $62 >P $c593 $3c >M $c594 $b5 >M $c595 $c6 >M $c596 $07 >M $c760 $52 >M
T{ op PC S A X Y P -> $c596 $72 $94 $ab $28 $60 }T T{ $c593 M $c594 M $c595 M $c596 M $c760 M -> $3c $b5 $c6 $07 $52 }T
$3347 >PC $43 >S $39 >A $03 >X $2b >Y $e5 >P $3347 $3c >M $3348 $9d >M $3349 $9a >M $334a $3f >M $9aa0 $e8 >M
T{ op PC S A X Y P -> $334a $43 $39 $03 $2b $e5 }T T{ $3347 M $3348 M $3349 M $334a M $9aa0 M -> $3c $9d $9a $3f $e8 }T
$3cb3 >PC $9a >S $f5 >A $a2 >X $2f >Y $e5 >P $3cb3 $3c >M $3cb4 $b4 >M $3cb5 $4d >M $3cb6 $a1 >M $4e56 $21 >M
T{ op PC S A X Y P -> $3cb6 $9a $f5 $a2 $2f $25 }T T{ $3cb3 M $3cb4 M $3cb5 M $3cb6 M $4e56 M -> $3c $b4 $4d $a1 $21 }T
$ae1b >PC $15 >S $a7 >A $63 >X $33 >Y $e0 >P $ae1b $3c >M $ae1c $a4 >M $ae1d $f6 >M $ae1e $74 >M $f707 $67 >M
T{ op PC S A X Y P -> $ae1e $15 $a7 $63 $33 $60 }T T{ $ae1b M $ae1c M $ae1d M $ae1e M $f707 M -> $3c $a4 $f6 $74 $67 }T
$c21b >PC $9b >S $4c >A $23 >X $85 >Y $e1 >P $c21b $3c >M $c21c $91 >M $c21d $da >M $c21e $6e >M $dab4 $8a >M
T{ op PC S A X Y P -> $c21e $9b $4c $23 $85 $a1 }T T{ $c21b M $c21c M $c21d M $c21e M $dab4 M -> $3c $91 $da $6e $8a }T
$4205 >PC $66 >S $a9 >A $38 >X $c7 >Y $a3 >P $4205 $3c >M $4206 $93 >M $4207 $50 >M $4208 $43 >M $50cb $ff >M
T{ op PC S A X Y P -> $4208 $66 $a9 $38 $c7 $e1 }T T{ $4205 M $4206 M $4207 M $4208 M $50cb M -> $3c $93 $50 $43 $ff }T
$0402 >PC $f9 >S $d7 >A $9d >X $a9 >Y $a2 >P $0402 $3c >M $0403 $bb >M $0404 $b6 >M $0405 $8c >M $b758 $f3 >M
T{ op PC S A X Y P -> $0405 $f9 $d7 $9d $a9 $e0 }T T{ $0402 M $0403 M $0404 M $0405 M $b758 M -> $3c $bb $b6 $8c $f3 }T
$fa9e >PC $39 >S $c1 >A $26 >X $6a >Y $a0 >P $61dc $43 >M $fa9e $3c >M $fa9f $b6 >M $faa0 $61 >M $faa1 $a3 >M
T{ op PC S A X Y P -> $faa1 $39 $c1 $26 $6a $60 }T T{ $61dc M $fa9e M $fa9f M $faa0 M $faa1 M -> $43 $3c $b6 $61 $a3 }T
( 3d )
$f80f >PC $36 >S $49 >A $a0 >X $a9 >Y $25 >P $e329 $a4 >M $f80f $3d >M $f810 $89 >M $f811 $e2 >M $f812 $2e >M
T{ op PC S A X Y P -> $f812 $36 $00 $a0 $a9 $27 }T T{ $e329 M $f80f M $f810 M $f811 M $f812 M -> $a4 $3d $89 $e2 $2e }T
$1ad2 >PC $16 >S $2e >A $a4 >X $3a >Y $e2 >P $1ad2 $3d >M $1ad3 $8e >M $1ad4 $ce >M $1ad5 $22 >M $cf32 $4e >M
T{ op PC S A X Y P -> $1ad5 $16 $0e $a4 $3a $60 }T T{ $1ad2 M $1ad3 M $1ad4 M $1ad5 M $cf32 M -> $3d $8e $ce $22 $4e }T
$e31d >PC $16 >S $45 >A $41 >X $9a >Y $22 >P $4dcb $9e >M $e31d $3d >M $e31e $8a >M $e31f $4d >M $e320 $d7 >M
T{ op PC S A X Y P -> $e320 $16 $04 $41 $9a $20 }T T{ $4dcb M $e31d M $e31e M $e31f M $e320 M -> $9e $3d $8a $4d $d7 }T
$5a82 >PC $2e >S $d4 >A $e1 >X $65 >Y $20 >P $5a82 $3d >M $5a83 $0b >M $5a84 $87 >M $5a85 $5e >M $87ec $2b >M
T{ op PC S A X Y P -> $5a85 $2e $00 $e1 $65 $22 }T T{ $5a82 M $5a83 M $5a84 M $5a85 M $87ec M -> $3d $0b $87 $5e $2b }T
$045c >PC $d9 >S $47 >A $43 >X $70 >Y $60 >P $045c $3d >M $045d $b2 >M $045e $ef >M $045f $d3 >M $eff5 $2e >M
T{ op PC S A X Y P -> $045f $d9 $06 $43 $70 $60 }T T{ $045c M $045d M $045e M $045f M $eff5 M -> $3d $b2 $ef $d3 $2e }T
$c754 >PC $9f >S $5c >A $72 >X $d6 >Y $e5 >P $322f $8e >M $c754 $3d >M $c755 $bd >M $c756 $31 >M $c757 $86 >M
T{ op PC S A X Y P -> $c757 $9f $0c $72 $d6 $65 }T T{ $322f M $c754 M $c755 M $c756 M $c757 M -> $8e $3d $bd $31 $86 }T
$69f1 >PC $c7 >S $95 >A $70 >X $38 >Y $24 >P $181f $3c >M $69f1 $3d >M $69f2 $af >M $69f3 $17 >M $69f4 $f5 >M
T{ op PC S A X Y P -> $69f4 $c7 $14 $70 $38 $24 }T T{ $181f M $69f1 M $69f2 M $69f3 M $69f4 M -> $3c $3d $af $17 $f5 }T
$70c6 >PC $97 >S $d5 >A $ed >X $78 >Y $27 >P $70c6 $3d >M $70c7 $27 >M $70c8 $e4 >M $70c9 $4b >M $e514 $3a >M
T{ op PC S A X Y P -> $70c9 $97 $10 $ed $78 $25 }T T{ $70c6 M $70c7 M $70c8 M $70c9 M $e514 M -> $3d $27 $e4 $4b $3a }T
$21f5 >PC $52 >S $20 >A $8b >X $07 >Y $60 >P $0a33 $e1 >M $21f5 $3d >M $21f6 $a8 >M $21f7 $09 >M $21f8 $ba >M
T{ op PC S A X Y P -> $21f8 $52 $20 $8b $07 $60 }T T{ $0a33 M $21f5 M $21f6 M $21f7 M $21f8 M -> $e1 $3d $a8 $09 $ba }T
$7330 >PC $9b >S $24 >A $cf >X $d2 >Y $66 >P $4828 $ab >M $7330 $3d >M $7331 $59 >M $7332 $47 >M $7333 $97 >M
T{ op PC S A X Y P -> $7333 $9b $20 $cf $d2 $64 }T T{ $4828 M $7330 M $7331 M $7332 M $7333 M -> $ab $3d $59 $47 $97 }T
$101b >PC $1c >S $e0 >A $85 >X $fc >Y $22 >P $101b $3d >M $101c $ad >M $101d $be >M $101e $79 >M $bf32 $51 >M
T{ op PC S A X Y P -> $101e $1c $40 $85 $fc $20 }T T{ $101b M $101c M $101d M $101e M $bf32 M -> $3d $ad $be $79 $51 }T
$3630 >PC $f0 >S $d1 >A $85 >X $51 >Y $64 >P $3630 $3d >M $3631 $9a >M $3632 $9b >M $3633 $68 >M $9c1f $00 >M
T{ op PC S A X Y P -> $3633 $f0 $00 $85 $51 $66 }T T{ $3630 M $3631 M $3632 M $3633 M $9c1f M -> $3d $9a $9b $68 $00 }T
$d46e >PC $8a >S $b5 >A $ba >X $ba >Y $a6 >P $7b8d $5c >M $d46e $3d >M $d46f $d3 >M $d470 $7a >M $d471 $93 >M
T{ op PC S A X Y P -> $d471 $8a $14 $ba $ba $24 }T T{ $7b8d M $d46e M $d46f M $d470 M $d471 M -> $5c $3d $d3 $7a $93 }T
$abc9 >PC $65 >S $62 >A $75 >X $7d >Y $e2 >P $2630 $07 >M $abc9 $3d >M $abca $bb >M $abcb $25 >M $abcc $fe >M
T{ op PC S A X Y P -> $abcc $65 $02 $75 $7d $60 }T T{ $2630 M $abc9 M $abca M $abcb M $abcc M -> $07 $3d $bb $25 $fe }T
$aa7b >PC $e6 >S $f0 >A $f9 >X $ac >Y $e6 >P $8e81 $a1 >M $aa7b $3d >M $aa7c $88 >M $aa7d $8d >M $aa7e $bb >M
T{ op PC S A X Y P -> $aa7e $e6 $a0 $f9 $ac $e4 }T T{ $8e81 M $aa7b M $aa7c M $aa7d M $aa7e M -> $a1 $3d $88 $8d $bb }T
$fb97 >PC $47 >S $60 >A $c4 >X $c5 >Y $66 >P $be56 $9e >M $fb97 $3d >M $fb98 $92 >M $fb99 $bd >M $fb9a $44 >M
T{ op PC S A X Y P -> $fb9a $47 $00 $c4 $c5 $66 }T T{ $be56 M $fb97 M $fb98 M $fb99 M $fb9a M -> $9e $3d $92 $bd $44 }T
( 3e )
$46ab >PC $1e >S $76 >A $fa >X $a4 >Y $67 >P $46ab $3e >M $46ac $0a >M $46ad $f4 >M $46ae $b0 >M $f504 $49 >M
T{ op PC S A X Y P -> $46ae $1e $76 $fa $a4 $e4 }T T{ $46ab M $46ac M $46ad M $46ae M $f504 M -> $3e $0a $f4 $b0 $93 }T
$7397 >PC $6c >S $d1 >A $5a >X $b4 >Y $a3 >P $7397 $3e >M $7398 $24 >M $7399 $f2 >M $739a $59 >M $f27e $e6 >M
T{ op PC S A X Y P -> $739a $6c $d1 $5a $b4 $a1 }T T{ $7397 M $7398 M $7399 M $739a M $f27e M -> $3e $24 $f2 $59 $cd }T
$d658 >PC $00 >S $05 >A $34 >X $08 >Y $e7 >P $1a05 $a1 >M $d658 $3e >M $d659 $d1 >M $d65a $19 >M $d65b $4f >M
T{ op PC S A X Y P -> $d65b $00 $05 $34 $08 $65 }T T{ $1a05 M $d658 M $d659 M $d65a M $d65b M -> $43 $3e $d1 $19 $4f }T
$0f41 >PC $80 >S $18 >A $c8 >X $21 >Y $20 >P $0f41 $3e >M $0f42 $e6 >M $0f43 $da >M $0f44 $29 >M $dbae $4a >M
T{ op PC S A X Y P -> $0f44 $80 $18 $c8 $21 $a0 }T T{ $0f41 M $0f42 M $0f43 M $0f44 M $dbae M -> $3e $e6 $da $29 $94 }T
$abf8 >PC $fa >S $c6 >A $0e >X $e2 >Y $64 >P $abf8 $3e >M $abf9 $49 >M $abfa $e4 >M $abfb $3f >M $e457 $95 >M
T{ op PC S A X Y P -> $abfb $fa $c6 $0e $e2 $65 }T T{ $abf8 M $abf9 M $abfa M $abfb M $e457 M -> $3e $49 $e4 $3f $2a }T
$89c7 >PC $0e >S $4b >A $61 >X $6a >Y $63 >P $1085 $84 >M $89c7 $3e >M $89c8 $24 >M $89c9 $10 >M $89ca $e8 >M
T{ op PC S A X Y P -> $89ca $0e $4b $61 $6a $61 }T T{ $1085 M $89c7 M $89c8 M $89c9 M $89ca M -> $09 $3e $24 $10 $e8 }T
$6707 >PC $f5 >S $af >A $50 >X $33 >Y $23 >P $6707 $3e >M $6708 $8c >M $6709 $97 >M $670a $25 >M $97dc $34 >M
T{ op PC S A X Y P -> $670a $f5 $af $50 $33 $20 }T T{ $6707 M $6708 M $6709 M $670a M $97dc M -> $3e $8c $97 $25 $69 }T
$3881 >PC $e5 >S $ee >A $00 >X $fd >Y $63 >P $3881 $3e >M $3882 $9c >M $3883 $a4 >M $3884 $69 >M $a49c $3b >M
T{ op PC S A X Y P -> $3884 $e5 $ee $00 $fd $60 }T T{ $3881 M $3882 M $3883 M $3884 M $a49c M -> $3e $9c $a4 $69 $77 }T
$5b80 >PC $09 >S $04 >A $34 >X $89 >Y $67 >P $5b80 $3e >M $5b81 $97 >M $5b82 $70 >M $5b83 $d7 >M $70cb $72 >M
T{ op PC S A X Y P -> $5b83 $09 $04 $34 $89 $e4 }T T{ $5b80 M $5b81 M $5b82 M $5b83 M $70cb M -> $3e $97 $70 $d7 $e5 }T
$8bd8 >PC $d1 >S $00 >A $98 >X $c7 >Y $24 >P $8bd8 $3e >M $8bd9 $e5 >M $8bda $a9 >M $8bdb $fa >M $aa7d $c3 >M
T{ op PC S A X Y P -> $8bdb $d1 $00 $98 $c7 $a5 }T T{ $8bd8 M $8bd9 M $8bda M $8bdb M $aa7d M -> $3e $e5 $a9 $fa $86 }T
$39d0 >PC $9c >S $d9 >A $e8 >X $eb >Y $24 >P $39d0 $3e >M $39d1 $db >M $39d2 $4d >M $39d3 $5a >M $4ec3 $7b >M
T{ op PC S A X Y P -> $39d3 $9c $d9 $e8 $eb $a4 }T T{ $39d0 M $39d1 M $39d2 M $39d3 M $4ec3 M -> $3e $db $4d $5a $f6 }T
$1add >PC $44 >S $6d >A $4d >X $71 >Y $e7 >P $1add $3e >M $1ade $77 >M $1adf $e9 >M $1ae0 $8c >M $e9c4 $af >M
T{ op PC S A X Y P -> $1ae0 $44 $6d $4d $71 $65 }T T{ $1add M $1ade M $1adf M $1ae0 M $e9c4 M -> $3e $77 $e9 $8c $5f }T
$b3fd >PC $2d >S $8c >A $71 >X $85 >Y $21 >P $0a25 $3a >M $b3fd $3e >M $b3fe $b4 >M $b3ff $09 >M $b400 $1c >M
T{ op PC S A X Y P -> $b400 $2d $8c $71 $85 $20 }T T{ $0a25 M $b3fd M $b3fe M $b3ff M $b400 M -> $75 $3e $b4 $09 $1c }T
$5283 >PC $2f >S $60 >A $7c >X $7b >Y $e7 >P $1484 $a8 >M $5283 $3e >M $5284 $08 >M $5285 $14 >M $5286 $bf >M
T{ op PC S A X Y P -> $5286 $2f $60 $7c $7b $65 }T T{ $1484 M $5283 M $5284 M $5285 M $5286 M -> $51 $3e $08 $14 $bf }T
$e35a >PC $35 >S $ec >A $71 >X $3f >Y $e1 >P $5884 $44 >M $e35a $3e >M $e35b $13 >M $e35c $58 >M $e35d $74 >M
T{ op PC S A X Y P -> $e35d $35 $ec $71 $3f $e0 }T T{ $5884 M $e35a M $e35b M $e35c M $e35d M -> $89 $3e $13 $58 $74 }T
$c9f5 >PC $87 >S $ac >A $52 >X $82 >Y $a4 >P $9aa3 $95 >M $c9f5 $3e >M $c9f6 $51 >M $c9f7 $9a >M $c9f8 $5a >M
T{ op PC S A X Y P -> $c9f8 $87 $ac $52 $82 $25 }T T{ $9aa3 M $c9f5 M $c9f6 M $c9f7 M $c9f8 M -> $2a $3e $51 $9a $5a }T
( 3f )
$91ac >PC $e8 >S $04 >A $cd >X $fb >Y $61 >P $00a0 $19 >M $914e $e9 >M $91ac $3f >M $91ad $a0 >M $91ae $9f >M $91af $b4 >M
T{ op PC S A X Y P -> $91af $e8 $04 $cd $fb $61 }T T{ $00a0 M $914e M $91ac M $91ad M $91ae M $91af M -> $19 $e9 $3f $a0 $9f $b4 }T
$5b77 >PC $0c >S $1a >A $6c >X $90 >Y $e7 >P $006d $42 >M $5b74 $57 >M $5b77 $3f >M $5b78 $6d >M $5b79 $fa >M
T{ op PC S A X Y P -> $5b74 $0c $1a $6c $90 $e7 }T T{ $006d M $5b74 M $5b77 M $5b78 M $5b79 M -> $42 $57 $3f $6d $fa }T
$a808 >PC $68 >S $a9 >A $9d >X $82 >Y $e1 >P $007b $e6 >M $a7b1 $e2 >M $a808 $3f >M $a809 $7b >M $a80a $a6 >M $a8b1 $60 >M
T{ op PC S A X Y P -> $a7b1 $68 $a9 $9d $82 $e1 }T T{ $007b M $a7b1 M $a808 M $a809 M $a80a M $a8b1 M -> $e6 $e2 $3f $7b $a6 $60 }T
$7c90 >PC $25 >S $48 >A $08 >X $3d >Y $62 >P $005a $9d >M $7c90 $3f >M $7c91 $5a >M $7c92 $2d >M $7c93 $79 >M $7cc0 $17 >M
T{ op PC S A X Y P -> $7c93 $25 $48 $08 $3d $62 }T T{ $005a M $7c90 M $7c91 M $7c92 M $7c93 M $7cc0 M -> $9d $3f $5a $2d $79 $17 }T
$0594 >PC $2e >S $ed >A $1a >X $0e >Y $64 >P $003f $0e >M $0594 $3f >M $0595 $3f >M $0596 $20 >M $0597 $8e >M $05b7 $ce >M
T{ op PC S A X Y P -> $0597 $2e $ed $1a $0e $64 }T T{ $003f M $0594 M $0595 M $0596 M $0597 M $05b7 M -> $0e $3f $3f $20 $8e $ce }T
$1dd4 >PC $42 >S $b0 >A $30 >X $2c >Y $e1 >P $0024 $6a >M $1dd4 $3f >M $1dd5 $24 >M $1dd6 $08 >M $1dd7 $12 >M $1ddf $4f >M
T{ op PC S A X Y P -> $1dd7 $42 $b0 $30 $2c $e1 }T T{ $0024 M $1dd4 M $1dd5 M $1dd6 M $1dd7 M $1ddf M -> $6a $3f $24 $08 $12 $4f }T
$c86d >PC $49 >S $05 >A $5c >X $35 >Y $26 >P $0004 $e4 >M $c86d $3f >M $c86e $04 >M $c86f $5d >M $c8cd $2f >M
T{ op PC S A X Y P -> $c8cd $49 $05 $5c $35 $26 }T T{ $0004 M $c86d M $c86e M $c86f M $c8cd M -> $e4 $3f $04 $5d $2f }T
$9e34 >PC $a3 >S $c0 >A $83 >X $88 >Y $a6 >P $00c3 $98 >M $9e34 $3f >M $9e35 $c3 >M $9e36 $14 >M $9e37 $bf >M $9e4b $81 >M
T{ op PC S A X Y P -> $9e37 $a3 $c0 $83 $88 $a6 }T T{ $00c3 M $9e34 M $9e35 M $9e36 M $9e37 M $9e4b M -> $98 $3f $c3 $14 $bf $81 }T
$35dc >PC $ec >S $71 >A $a6 >X $75 >Y $25 >P $00b9 $62 >M $35dc $3f >M $35dd $b9 >M $35de $00 >M $35df $15 >M
T{ op PC S A X Y P -> $35df $ec $71 $a6 $75 $25 }T T{ $00b9 M $35dc M $35dd M $35de M $35df M -> $62 $3f $b9 $00 $15 }T
$c99d >PC $09 >S $fd >A $ce >X $44 >Y $60 >P $003d $71 >M $c95a $c6 >M $c99d $3f >M $c99e $3d >M $c99f $ba >M
T{ op PC S A X Y P -> $c95a $09 $fd $ce $44 $60 }T T{ $003d M $c95a M $c99d M $c99e M $c99f M -> $71 $c6 $3f $3d $ba }T
$e7f0 >PC $68 >S $7d >A $e4 >X $48 >Y $61 >P $001b $0d >M $e745 $cf >M $e7f0 $3f >M $e7f1 $1b >M $e7f2 $52 >M $e7f3 $8c >M
T{ op PC S A X Y P -> $e7f3 $68 $7d $e4 $48 $61 }T T{ $001b M $e745 M $e7f0 M $e7f1 M $e7f2 M $e7f3 M -> $0d $cf $3f $1b $52 $8c }T
$4f7e >PC $18 >S $58 >A $22 >X $75 >Y $62 >P $0049 $c9 >M $4f7d $0f >M $4f7e $3f >M $4f7f $49 >M $4f80 $fc >M $4f81 $16 >M
T{ op PC S A X Y P -> $4f81 $18 $58 $22 $75 $62 }T T{ $0049 M $4f7d M $4f7e M $4f7f M $4f80 M $4f81 M -> $c9 $0f $3f $49 $fc $16 }T
$57cd >PC $49 >S $63 >A $05 >X $38 >Y $a3 >P $0000 $84 >M $57cd $3f >M $57ce $00 >M $57cf $2b >M $57fb $af >M
T{ op PC S A X Y P -> $57fb $49 $63 $05 $38 $a3 }T T{ $0000 M $57cd M $57ce M $57cf M $57fb M -> $84 $3f $00 $2b $af }T
$62b6 >PC $fe >S $b8 >A $1a >X $14 >Y $61 >P $0062 $e5 >M $627e $1f >M $62b6 $3f >M $62b7 $62 >M $62b8 $c5 >M
T{ op PC S A X Y P -> $627e $fe $b8 $1a $14 $61 }T T{ $0062 M $627e M $62b6 M $62b7 M $62b8 M -> $e5 $1f $3f $62 $c5 }T
$796f >PC $b8 >S $2e >A $fd >X $9e >Y $a5 >P $00fb $8d >M $796f $3f >M $7970 $fb >M $7971 $22 >M $7972 $1c >M $7994 $24 >M
T{ op PC S A X Y P -> $7972 $b8 $2e $fd $9e $a5 }T T{ $00fb M $796f M $7970 M $7971 M $7972 M $7994 M -> $8d $3f $fb $22 $1c $24 }T
$27e7 >PC $0c >S $2d >A $87 >X $09 >Y $a7 >P $0072 $07 >M $27d4 $ee >M $27e7 $3f >M $27e8 $72 >M $27e9 $ea >M
T{ op PC S A X Y P -> $27d4 $0c $2d $87 $09 $a7 }T T{ $0072 M $27d4 M $27e7 M $27e8 M $27e9 M -> $07 $ee $3f $72 $ea }T
( 40 )
$a747 >PC $59 >S $0c >A $37 >X $73 >Y $21 >P $0159 $b5 >M $015a $61 >M $015b $3f >M $015c $76 >M $763f $f5 >M $a747 $40 >M $a748 $67 >M $a749 $77 >M
T{ op PC S A X Y P -> $763f $5c $0c $37 $73 $61 }T T{ $0159 M $015a M $015b M $015c M $763f M $a747 M $a748 M $a749 M -> $b5 $61 $3f $76 $f5 $40 $67 $77 }T
$1e78 >PC $69 >S $47 >A $0d >X $60 >Y $e0 >P $0169 $af >M $016a $27 >M $016b $e7 >M $016c $37 >M $1e78 $40 >M $1e79 $9c >M $1e7a $d7 >M $37e7 $75 >M
T{ op PC S A X Y P -> $37e7 $6c $47 $0d $60 $27 }T T{ $0169 M $016a M $016b M $016c M $1e78 M $1e79 M $1e7a M $37e7 M -> $af $27 $e7 $37 $40 $9c $d7 $75 }T
$865f >PC $7a >S $9f >A $e8 >X $73 >Y $23 >P $017a $a7 >M $017b $c6 >M $017c $96 >M $017d $11 >M $1196 $4c >M $865f $40 >M $8660 $8b >M $8661 $48 >M
T{ op PC S A X Y P -> $1196 $7d $9f $e8 $73 $e6 }T T{ $017a M $017b M $017c M $017d M $1196 M $865f M $8660 M $8661 M -> $a7 $c6 $96 $11 $4c $40 $8b $48 }T
$4ad8 >PC $b3 >S $48 >A $96 >X $d0 >Y $23 >P $01b3 $6d >M $01b4 $7e >M $01b5 $b0 >M $01b6 $24 >M $24b0 $bf >M $4ad8 $40 >M $4ad9 $55 >M $4ada $23 >M
T{ op PC S A X Y P -> $24b0 $b6 $48 $96 $d0 $6e }T T{ $01b3 M $01b4 M $01b5 M $01b6 M $24b0 M $4ad8 M $4ad9 M $4ada M -> $6d $7e $b0 $24 $bf $40 $55 $23 }T
$3b4f >PC $4b >S $61 >A $a4 >X $91 >Y $63 >P $014b $a5 >M $014c $62 >M $014d $af >M $014e $7e >M $3b4f $40 >M $3b50 $fa >M $3b51 $25 >M $7eaf $3f >M
T{ op PC S A X Y P -> $7eaf $4e $61 $a4 $91 $62 }T T{ $014b M $014c M $014d M $014e M $3b4f M $3b50 M $3b51 M $7eaf M -> $a5 $62 $af $7e $40 $fa $25 $3f }T
$5c5e >PC $ae >S $86 >A $12 >X $aa >Y $e7 >P $01ae $e8 >M $01af $89 >M $01b0 $a1 >M $01b1 $af >M $5c5e $40 >M $5c5f $a6 >M $5c60 $53 >M $afa1 $51 >M
T{ op PC S A X Y P -> $afa1 $b1 $86 $12 $aa $a9 }T T{ $01ae M $01af M $01b0 M $01b1 M $5c5e M $5c5f M $5c60 M $afa1 M -> $e8 $89 $a1 $af $40 $a6 $53 $51 }T
$063c >PC $88 >S $2f >A $41 >X $b1 >Y $64 >P $0188 $2e >M $0189 $5e >M $018a $b6 >M $018b $7d >M $063c $40 >M $063d $9d >M $063e $05 >M $7db6 $e0 >M
T{ op PC S A X Y P -> $7db6 $8b $2f $41 $b1 $6e }T T{ $0188 M $0189 M $018a M $018b M $063c M $063d M $063e M $7db6 M -> $2e $5e $b6 $7d $40 $9d $05 $e0 }T
$205f >PC $38 >S $24 >A $98 >X $bb >Y $20 >P $0138 $4b >M $0139 $03 >M $013a $c0 >M $013b $f1 >M $205f $40 >M $2060 $b4 >M $2061 $aa >M $f1c0 $61 >M
T{ op PC S A X Y P -> $f1c0 $3b $24 $98 $bb $23 }T T{ $0138 M $0139 M $013a M $013b M $205f M $2060 M $2061 M $f1c0 M -> $4b $03 $c0 $f1 $40 $b4 $aa $61 }T
$a2c0 >PC $a1 >S $b4 >A $97 >X $e7 >Y $e1 >P $01a1 $a2 >M $01a2 $de >M $01a3 $1c >M $01a4 $33 >M $331c $3e >M $a2c0 $40 >M $a2c1 $94 >M $a2c2 $79 >M
T{ op PC S A X Y P -> $331c $a4 $b4 $97 $e7 $ee }T T{ $01a1 M $01a2 M $01a3 M $01a4 M $331c M $a2c0 M $a2c1 M $a2c2 M -> $a2 $de $1c $33 $3e $40 $94 $79 }T
$113c >PC $28 >S $bc >A $4e >X $7a >Y $e3 >P $0128 $6b >M $0129 $8c >M $012a $0a >M $012b $35 >M $113c $40 >M $113d $d2 >M $113e $39 >M $350a $b2 >M
T{ op PC S A X Y P -> $350a $2b $bc $4e $7a $ac }T T{ $0128 M $0129 M $012a M $012b M $113c M $113d M $113e M $350a M -> $6b $8c $0a $35 $40 $d2 $39 $b2 }T
$39ba >PC $65 >S $31 >A $93 >X $0f >Y $a2 >P $0165 $b9 >M $0166 $e4 >M $0167 $12 >M $0168 $03 >M $0312 $a4 >M $39ba $40 >M $39bb $81 >M $39bc $46 >M
T{ op PC S A X Y P -> $0312 $68 $31 $93 $0f $e4 }T T{ $0165 M $0166 M $0167 M $0168 M $0312 M $39ba M $39bb M $39bc M -> $b9 $e4 $12 $03 $a4 $40 $81 $46 }T
$e20f >PC $b1 >S $5a >A $99 >X $c0 >Y $64 >P $01b1 $67 >M $01b2 $0b >M $01b3 $de >M $01b4 $75 >M $75de $d9 >M $e20f $40 >M $e210 $ce >M $e211 $34 >M
T{ op PC S A X Y P -> $75de $b4 $5a $99 $c0 $2b }T T{ $01b1 M $01b2 M $01b3 M $01b4 M $75de M $e20f M $e210 M $e211 M -> $67 $0b $de $75 $d9 $40 $ce $34 }T
$6d59 >PC $f1 >S $1e >A $34 >X $a4 >Y $a0 >P $01f1 $73 >M $01f2 $f2 >M $01f3 $ed >M $01f4 $5c >M $5ced $71 >M $6d59 $40 >M $6d5a $c7 >M $6d5b $b7 >M
T{ op PC S A X Y P -> $5ced $f4 $1e $34 $a4 $e2 }T T{ $01f1 M $01f2 M $01f3 M $01f4 M $5ced M $6d59 M $6d5a M $6d5b M -> $73 $f2 $ed $5c $71 $40 $c7 $b7 }T
$cb78 >PC $dc >S $83 >A $1d >X $b2 >Y $a6 >P $01dc $45 >M $01dd $a4 >M $01de $2c >M $01df $cd >M $cb78 $40 >M $cb79 $b5 >M $cb7a $83 >M $cd2c $6d >M
T{ op PC S A X Y P -> $cd2c $df $83 $1d $b2 $a4 }T T{ $01dc M $01dd M $01de M $01df M $cb78 M $cb79 M $cb7a M $cd2c M -> $45 $a4 $2c $cd $40 $b5 $83 $6d }T
$0b12 >PC $ba >S $0b >A $8c >X $30 >Y $61 >P $01ba $1b >M $01bb $99 >M $01bc $ed >M $01bd $cf >M $0b12 $40 >M $0b13 $70 >M $0b14 $52 >M $cfed $65 >M
T{ op PC S A X Y P -> $cfed $bd $0b $8c $30 $a9 }T T{ $01ba M $01bb M $01bc M $01bd M $0b12 M $0b13 M $0b14 M $cfed M -> $1b $99 $ed $cf $40 $70 $52 $65 }T
$f88e >PC $9b >S $8f >A $e9 >X $24 >Y $a0 >P $019b $ed >M $019c $40 >M $019d $e0 >M $019e $cf >M $cfe0 $e5 >M $f88e $40 >M $f88f $90 >M $f890 $36 >M
T{ op PC S A X Y P -> $cfe0 $9e $8f $e9 $24 $60 }T T{ $019b M $019c M $019d M $019e M $cfe0 M $f88e M $f88f M $f890 M -> $ed $40 $e0 $cf $e5 $40 $90 $36 }T
( 41 )
$9977 >PC $b1 >S $f0 >A $5c >X $ea >Y $a4 >P $0091 $93 >M $00ed $27 >M $00ee $bf >M $9977 $41 >M $9978 $91 >M $9979 $ca >M $bf27 $a5 >M
T{ op PC S A X Y P -> $9979 $b1 $55 $5c $ea $24 }T T{ $0091 M $00ed M $00ee M $9977 M $9978 M $9979 M $bf27 M -> $93 $27 $bf $41 $91 $ca $a5 }T
$71bd >PC $f6 >S $e1 >A $96 >X $29 >Y $a3 >P $0002 $08 >M $0003 $18 >M $006c $46 >M $1808 $e3 >M $71bd $41 >M $71be $6c >M $71bf $10 >M
T{ op PC S A X Y P -> $71bf $f6 $02 $96 $29 $21 }T T{ $0002 M $0003 M $006c M $1808 M $71bd M $71be M $71bf M -> $08 $18 $46 $e3 $41 $6c $10 }T
$679f >PC $49 >S $a7 >A $f3 >X $e5 >Y $63 >P $00db $38 >M $00dc $6e >M $00e8 $01 >M $679f $41 >M $67a0 $e8 >M $67a1 $b1 >M $6e38 $b9 >M
T{ op PC S A X Y P -> $67a1 $49 $1e $f3 $e5 $61 }T T{ $00db M $00dc M $00e8 M $679f M $67a0 M $67a1 M $6e38 M -> $38 $6e $01 $41 $e8 $b1 $b9 }T
$cb55 >PC $98 >S $8b >A $be >X $35 >Y $60 >P $0026 $dd >M $0027 $dc >M $0068 $85 >M $cb55 $41 >M $cb56 $68 >M $cb57 $29 >M $dcdd $d8 >M
T{ op PC S A X Y P -> $cb57 $98 $53 $be $35 $60 }T T{ $0026 M $0027 M $0068 M $cb55 M $cb56 M $cb57 M $dcdd M -> $dd $dc $85 $41 $68 $29 $d8 }T
$f2ca >PC $b8 >S $14 >A $06 >X $cd >Y $63 >P $0069 $47 >M $006f $38 >M $0070 $f4 >M $f2ca $41 >M $f2cb $69 >M $f2cc $91 >M $f438 $ea >M
T{ op PC S A X Y P -> $f2cc $b8 $fe $06 $cd $e1 }T T{ $0069 M $006f M $0070 M $f2ca M $f2cb M $f2cc M $f438 M -> $47 $38 $f4 $41 $69 $91 $ea }T
$85b6 >PC $1f >S $ff >A $d2 >X $a0 >Y $a7 >P $0018 $bc >M $0019 $33 >M $0046 $18 >M $33bc $81 >M $85b6 $41 >M $85b7 $46 >M $85b8 $45 >M
T{ op PC S A X Y P -> $85b8 $1f $7e $d2 $a0 $25 }T T{ $0018 M $0019 M $0046 M $33bc M $85b6 M $85b7 M $85b8 M -> $bc $33 $18 $81 $41 $46 $45 }T
$1119 >PC $13 >S $90 >A $4f >X $46 >Y $21 >P $00a0 $8d >M $00ef $4b >M $00f0 $51 >M $1119 $41 >M $111a $a0 >M $111b $a3 >M $514b $1d >M
T{ op PC S A X Y P -> $111b $13 $8d $4f $46 $a1 }T T{ $00a0 M $00ef M $00f0 M $1119 M $111a M $111b M $514b M -> $8d $4b $51 $41 $a0 $a3 $1d }T
$770b >PC $9a >S $28 >A $98 >X $e8 >Y $61 >P $004a $0a >M $004b $09 >M $00b2 $e1 >M $090a $8c >M $770b $41 >M $770c $b2 >M $770d $50 >M
T{ op PC S A X Y P -> $770d $9a $a4 $98 $e8 $e1 }T T{ $004a M $004b M $00b2 M $090a M $770b M $770c M $770d M -> $0a $09 $e1 $8c $41 $b2 $50 }T
$15a7 >PC $a1 >S $0d >A $c6 >X $51 >Y $a3 >P $0097 $f4 >M $0098 $47 >M $00d1 $73 >M $15a7 $41 >M $15a8 $d1 >M $15a9 $55 >M $47f4 $cd >M
T{ op PC S A X Y P -> $15a9 $a1 $c0 $c6 $51 $a1 }T T{ $0097 M $0098 M $00d1 M $15a7 M $15a8 M $15a9 M $47f4 M -> $f4 $47 $73 $41 $d1 $55 $cd }T
$69b0 >PC $62 >S $e8 >A $67 >X $99 >Y $a1 >P $0066 $c1 >M $00cd $34 >M $00ce $ed >M $69b0 $41 >M $69b1 $66 >M $69b2 $c6 >M $ed34 $40 >M
T{ op PC S A X Y P -> $69b2 $62 $a8 $67 $99 $a1 }T T{ $0066 M $00cd M $00ce M $69b0 M $69b1 M $69b2 M $ed34 M -> $c1 $34 $ed $41 $66 $c6 $40 }T
$e4e1 >PC $70 >S $34 >A $d1 >X $12 >Y $63 >P $003a $8a >M $003b $41 >M $0069 $45 >M $418a $40 >M $e4e1 $41 >M $e4e2 $69 >M $e4e3 $1e >M
T{ op PC S A X Y P -> $e4e3 $70 $74 $d1 $12 $61 }T T{ $003a M $003b M $0069 M $418a M $e4e1 M $e4e2 M $e4e3 M -> $8a $41 $45 $40 $41 $69 $1e }T
$6646 >PC $16 >S $9e >A $f6 >X $b5 >Y $60 >P $006d $49 >M $006e $6c >M $0077 $fb >M $6646 $41 >M $6647 $77 >M $6648 $61 >M $6c49 $8d >M
T{ op PC S A X Y P -> $6648 $16 $13 $f6 $b5 $60 }T T{ $006d M $006e M $0077 M $6646 M $6647 M $6648 M $6c49 M -> $49 $6c $fb $41 $77 $61 $8d }T
$2890 >PC $ee >S $d2 >A $a2 >X $9b >Y $65 >P $007d $ec >M $007e $68 >M $00db $64 >M $2890 $41 >M $2891 $db >M $2892 $32 >M $68ec $53 >M
T{ op PC S A X Y P -> $2892 $ee $81 $a2 $9b $e5 }T T{ $007d M $007e M $00db M $2890 M $2891 M $2892 M $68ec M -> $ec $68 $64 $41 $db $32 $53 }T
$4bd0 >PC $5e >S $77 >A $0a >X $96 >Y $e7 >P $000b $1a >M $0015 $a9 >M $0016 $22 >M $22a9 $ba >M $4bd0 $41 >M $4bd1 $0b >M $4bd2 $10 >M
T{ op PC S A X Y P -> $4bd2 $5e $cd $0a $96 $e5 }T T{ $000b M $0015 M $0016 M $22a9 M $4bd0 M $4bd1 M $4bd2 M -> $1a $a9 $22 $ba $41 $0b $10 }T
$e51e >PC $6f >S $94 >A $78 >X $0c >Y $21 >P $0002 $7d >M $007a $27 >M $007b $cd >M $cd27 $a3 >M $e51e $41 >M $e51f $02 >M $e520 $e0 >M
T{ op PC S A X Y P -> $e520 $6f $37 $78 $0c $21 }T T{ $0002 M $007a M $007b M $cd27 M $e51e M $e51f M $e520 M -> $7d $27 $cd $a3 $41 $02 $e0 }T
$4323 >PC $18 >S $2b >A $bb >X $62 >Y $e1 >P $004b $c4 >M $004c $b6 >M $0090 $6b >M $4323 $41 >M $4324 $90 >M $4325 $4d >M $b6c4 $b7 >M
T{ op PC S A X Y P -> $4325 $18 $9c $bb $62 $e1 }T T{ $004b M $004c M $0090 M $4323 M $4324 M $4325 M $b6c4 M -> $c4 $b6 $6b $41 $90 $4d $b7 }T
( 42 )
$1e86 >PC $c8 >S $fa >A $c1 >X $0b >Y $65 >P $1e86 $42 >M $1e87 $cd >M $1e88 $46 >M
T{ op PC S A X Y P -> $1e88 $c8 $fa $c1 $0b $65 }T T{ $1e86 M $1e87 M $1e88 M -> $42 $cd $46 }T
$317a >PC $0e >S $b5 >A $6e >X $a9 >Y $65 >P $317a $42 >M $317b $ad >M $317c $35 >M
T{ op PC S A X Y P -> $317c $0e $b5 $6e $a9 $65 }T T{ $317a M $317b M $317c M -> $42 $ad $35 }T
$b024 >PC $1f >S $c0 >A $ec >X $0f >Y $a4 >P $b024 $42 >M $b025 $23 >M $b026 $dd >M
T{ op PC S A X Y P -> $b026 $1f $c0 $ec $0f $a4 }T T{ $b024 M $b025 M $b026 M -> $42 $23 $dd }T
$31fa >PC $72 >S $6f >A $ec >X $65 >Y $20 >P $31fa $42 >M $31fb $69 >M $31fc $23 >M
T{ op PC S A X Y P -> $31fc $72 $6f $ec $65 $20 }T T{ $31fa M $31fb M $31fc M -> $42 $69 $23 }T
$1f95 >PC $de >S $fd >A $9d >X $eb >Y $20 >P $1f95 $42 >M $1f96 $be >M $1f97 $f1 >M
T{ op PC S A X Y P -> $1f97 $de $fd $9d $eb $20 }T T{ $1f95 M $1f96 M $1f97 M -> $42 $be $f1 }T
$6937 >PC $a1 >S $bd >A $e0 >X $ba >Y $60 >P $6937 $42 >M $6938 $aa >M $6939 $d9 >M
T{ op PC S A X Y P -> $6939 $a1 $bd $e0 $ba $60 }T T{ $6937 M $6938 M $6939 M -> $42 $aa $d9 }T
$1c9e >PC $81 >S $0e >A $06 >X $1f >Y $25 >P $1c9e $42 >M $1c9f $17 >M $1ca0 $77 >M
T{ op PC S A X Y P -> $1ca0 $81 $0e $06 $1f $25 }T T{ $1c9e M $1c9f M $1ca0 M -> $42 $17 $77 }T
$e365 >PC $9c >S $6e >A $02 >X $22 >Y $a3 >P $e365 $42 >M $e366 $ce >M $e367 $2b >M
T{ op PC S A X Y P -> $e367 $9c $6e $02 $22 $a3 }T T{ $e365 M $e366 M $e367 M -> $42 $ce $2b }T
$a0d8 >PC $09 >S $be >A $e7 >X $e9 >Y $20 >P $a0d8 $42 >M $a0d9 $ff >M $a0da $81 >M
T{ op PC S A X Y P -> $a0da $09 $be $e7 $e9 $20 }T T{ $a0d8 M $a0d9 M $a0da M -> $42 $ff $81 }T
$7c03 >PC $97 >S $a9 >A $0c >X $1d >Y $67 >P $7c03 $42 >M $7c04 $c8 >M $7c05 $1c >M
T{ op PC S A X Y P -> $7c05 $97 $a9 $0c $1d $67 }T T{ $7c03 M $7c04 M $7c05 M -> $42 $c8 $1c }T
$0f90 >PC $66 >S $7c >A $cd >X $24 >Y $60 >P $0f90 $42 >M $0f91 $8f >M $0f92 $12 >M
T{ op PC S A X Y P -> $0f92 $66 $7c $cd $24 $60 }T T{ $0f90 M $0f91 M $0f92 M -> $42 $8f $12 }T
$1f0a >PC $25 >S $d0 >A $06 >X $06 >Y $63 >P $1f0a $42 >M $1f0b $a1 >M $1f0c $8d >M
T{ op PC S A X Y P -> $1f0c $25 $d0 $06 $06 $63 }T T{ $1f0a M $1f0b M $1f0c M -> $42 $a1 $8d }T
$c6c7 >PC $85 >S $68 >A $23 >X $82 >Y $e1 >P $c6c7 $42 >M $c6c8 $42 >M $c6c9 $d0 >M
T{ op PC S A X Y P -> $c6c9 $85 $68 $23 $82 $e1 }T T{ $c6c7 M $c6c8 M $c6c9 M -> $42 $42 $d0 }T
$b867 >PC $20 >S $e3 >A $ab >X $8d >Y $a5 >P $b867 $42 >M $b868 $c6 >M $b869 $56 >M
T{ op PC S A X Y P -> $b869 $20 $e3 $ab $8d $a5 }T T{ $b867 M $b868 M $b869 M -> $42 $c6 $56 }T
$a005 >PC $62 >S $8a >A $a7 >X $0d >Y $e6 >P $a005 $42 >M $a006 $07 >M $a007 $94 >M
T{ op PC S A X Y P -> $a007 $62 $8a $a7 $0d $e6 }T T{ $a005 M $a006 M $a007 M -> $42 $07 $94 }T
$02fd >PC $14 >S $dd >A $a4 >X $50 >Y $63 >P $02fd $42 >M $02fe $15 >M $02ff $39 >M
T{ op PC S A X Y P -> $02ff $14 $dd $a4 $50 $63 }T T{ $02fd M $02fe M $02ff M -> $42 $15 $39 }T
( 43 )
$4e9a >PC $31 >S $1f >A $6d >X $dc >Y $a5 >P $4e9a $43 >M $4e9b $92 >M $4e9c $c4 >M
T{ op PC S A X Y P -> $4e9b $31 $1f $6d $dc $a5 }T T{ $4e9a M $4e9b M $4e9c M -> $43 $92 $c4 }T
$c4dd >PC $37 >S $f1 >A $09 >X $7b >Y $21 >P $c4dd $43 >M $c4de $69 >M $c4df $0b >M
T{ op PC S A X Y P -> $c4de $37 $f1 $09 $7b $21 }T T{ $c4dd M $c4de M $c4df M -> $43 $69 $0b }T
$df2c >PC $39 >S $5c >A $58 >X $63 >Y $a4 >P $df2c $43 >M $df2d $05 >M $df2e $c2 >M
T{ op PC S A X Y P -> $df2d $39 $5c $58 $63 $a4 }T T{ $df2c M $df2d M $df2e M -> $43 $05 $c2 }T
$67e4 >PC $67 >S $0d >A $a6 >X $ab >Y $63 >P $67e4 $43 >M $67e5 $70 >M $67e6 $4c >M
T{ op PC S A X Y P -> $67e5 $67 $0d $a6 $ab $63 }T T{ $67e4 M $67e5 M $67e6 M -> $43 $70 $4c }T
$e091 >PC $59 >S $62 >A $2b >X $34 >Y $27 >P $e091 $43 >M $e092 $d8 >M $e093 $1d >M
T{ op PC S A X Y P -> $e092 $59 $62 $2b $34 $27 }T T{ $e091 M $e092 M $e093 M -> $43 $d8 $1d }T
$bbe5 >PC $f6 >S $5b >A $38 >X $2c >Y $e5 >P $bbe5 $43 >M $bbe6 $4a >M $bbe7 $90 >M
T{ op PC S A X Y P -> $bbe6 $f6 $5b $38 $2c $e5 }T T{ $bbe5 M $bbe6 M $bbe7 M -> $43 $4a $90 }T
$3b2e >PC $4b >S $ef >A $b8 >X $6f >Y $a5 >P $3b2e $43 >M $3b2f $35 >M $3b30 $05 >M
T{ op PC S A X Y P -> $3b2f $4b $ef $b8 $6f $a5 }T T{ $3b2e M $3b2f M $3b30 M -> $43 $35 $05 }T
$8e56 >PC $fd >S $d0 >A $01 >X $28 >Y $27 >P $8e56 $43 >M $8e57 $e4 >M $8e58 $a9 >M
T{ op PC S A X Y P -> $8e57 $fd $d0 $01 $28 $27 }T T{ $8e56 M $8e57 M $8e58 M -> $43 $e4 $a9 }T
$1777 >PC $85 >S $8d >A $97 >X $12 >Y $e2 >P $1777 $43 >M $1778 $71 >M $1779 $b4 >M
T{ op PC S A X Y P -> $1778 $85 $8d $97 $12 $e2 }T T{ $1777 M $1778 M $1779 M -> $43 $71 $b4 }T
$80f4 >PC $4b >S $2a >A $78 >X $c5 >Y $e1 >P $80f4 $43 >M $80f5 $cf >M $80f6 $91 >M
T{ op PC S A X Y P -> $80f5 $4b $2a $78 $c5 $e1 }T T{ $80f4 M $80f5 M $80f6 M -> $43 $cf $91 }T
$83f3 >PC $cc >S $25 >A $9e >X $10 >Y $63 >P $83f3 $43 >M $83f4 $e7 >M $83f5 $92 >M
T{ op PC S A X Y P -> $83f4 $cc $25 $9e $10 $63 }T T{ $83f3 M $83f4 M $83f5 M -> $43 $e7 $92 }T
$4272 >PC $a1 >S $80 >A $0a >X $d6 >Y $e2 >P $4272 $43 >M $4273 $6f >M $4274 $40 >M
T{ op PC S A X Y P -> $4273 $a1 $80 $0a $d6 $e2 }T T{ $4272 M $4273 M $4274 M -> $43 $6f $40 }T
$c573 >PC $b1 >S $1c >A $a3 >X $b7 >Y $20 >P $c573 $43 >M $c574 $a5 >M $c575 $99 >M
T{ op PC S A X Y P -> $c574 $b1 $1c $a3 $b7 $20 }T T{ $c573 M $c574 M $c575 M -> $43 $a5 $99 }T
$f573 >PC $d5 >S $9b >A $00 >X $01 >Y $60 >P $f573 $43 >M $f574 $2c >M $f575 $a0 >M
T{ op PC S A X Y P -> $f574 $d5 $9b $00 $01 $60 }T T{ $f573 M $f574 M $f575 M -> $43 $2c $a0 }T
$8f4a >PC $48 >S $26 >A $6a >X $76 >Y $23 >P $8f4a $43 >M $8f4b $ac >M $8f4c $2d >M
T{ op PC S A X Y P -> $8f4b $48 $26 $6a $76 $23 }T T{ $8f4a M $8f4b M $8f4c M -> $43 $ac $2d }T
$cf79 >PC $df >S $9f >A $bd >X $74 >Y $66 >P $cf79 $43 >M $cf7a $84 >M $cf7b $d4 >M
T{ op PC S A X Y P -> $cf7a $df $9f $bd $74 $66 }T T{ $cf79 M $cf7a M $cf7b M -> $43 $84 $d4 }T
( 44 )
$a432 >PC $3e >S $76 >A $5e >X $4c >Y $24 >P $00ad $9b >M $a432 $44 >M $a433 $ad >M $a434 $04 >M
T{ op PC S A X Y P -> $a434 $3e $76 $5e $4c $24 }T T{ $00ad M $a432 M $a433 M $a434 M -> $9b $44 $ad $04 }T
$754d >PC $67 >S $f6 >A $f0 >X $6e >Y $21 >P $0054 $75 >M $754d $44 >M $754e $54 >M $754f $bb >M
T{ op PC S A X Y P -> $754f $67 $f6 $f0 $6e $21 }T T{ $0054 M $754d M $754e M $754f M -> $75 $44 $54 $bb }T
$38ec >PC $87 >S $de >A $09 >X $3b >Y $e7 >P $002a $01 >M $38ec $44 >M $38ed $2a >M $38ee $68 >M
T{ op PC S A X Y P -> $38ee $87 $de $09 $3b $e7 }T T{ $002a M $38ec M $38ed M $38ee M -> $01 $44 $2a $68 }T
$8ba3 >PC $51 >S $99 >A $37 >X $3c >Y $a5 >P $0031 $4e >M $8ba3 $44 >M $8ba4 $31 >M $8ba5 $1e >M
T{ op PC S A X Y P -> $8ba5 $51 $99 $37 $3c $a5 }T T{ $0031 M $8ba3 M $8ba4 M $8ba5 M -> $4e $44 $31 $1e }T
$6a60 >PC $99 >S $2d >A $aa >X $75 >Y $e4 >P $00fb $b3 >M $6a60 $44 >M $6a61 $fb >M $6a62 $2c >M
T{ op PC S A X Y P -> $6a62 $99 $2d $aa $75 $e4 }T T{ $00fb M $6a60 M $6a61 M $6a62 M -> $b3 $44 $fb $2c }T
$d355 >PC $ba >S $63 >A $98 >X $eb >Y $26 >P $0035 $72 >M $d355 $44 >M $d356 $35 >M $d357 $ca >M
T{ op PC S A X Y P -> $d357 $ba $63 $98 $eb $26 }T T{ $0035 M $d355 M $d356 M $d357 M -> $72 $44 $35 $ca }T
$7f4d >PC $50 >S $cf >A $92 >X $2e >Y $e5 >P $00ee $8a >M $7f4d $44 >M $7f4e $ee >M $7f4f $ca >M
T{ op PC S A X Y P -> $7f4f $50 $cf $92 $2e $e5 }T T{ $00ee M $7f4d M $7f4e M $7f4f M -> $8a $44 $ee $ca }T
$187f >PC $d4 >S $f7 >A $3f >X $b8 >Y $20 >P $0048 $c8 >M $187f $44 >M $1880 $48 >M $1881 $a9 >M
T{ op PC S A X Y P -> $1881 $d4 $f7 $3f $b8 $20 }T T{ $0048 M $187f M $1880 M $1881 M -> $c8 $44 $48 $a9 }T
$477e >PC $d6 >S $7a >A $00 >X $88 >Y $a3 >P $00e4 $ad >M $477e $44 >M $477f $e4 >M $4780 $7c >M
T{ op PC S A X Y P -> $4780 $d6 $7a $00 $88 $a3 }T T{ $00e4 M $477e M $477f M $4780 M -> $ad $44 $e4 $7c }T
$833e >PC $54 >S $d8 >A $0f >X $87 >Y $a1 >P $004c $30 >M $833e $44 >M $833f $4c >M $8340 $72 >M
T{ op PC S A X Y P -> $8340 $54 $d8 $0f $87 $a1 }T T{ $004c M $833e M $833f M $8340 M -> $30 $44 $4c $72 }T
$9939 >PC $e6 >S $36 >A $d5 >X $4c >Y $27 >P $004d $8c >M $9939 $44 >M $993a $4d >M $993b $2f >M
T{ op PC S A X Y P -> $993b $e6 $36 $d5 $4c $27 }T T{ $004d M $9939 M $993a M $993b M -> $8c $44 $4d $2f }T
$e522 >PC $76 >S $d5 >A $9f >X $ca >Y $66 >P $0079 $a8 >M $e522 $44 >M $e523 $79 >M $e524 $54 >M
T{ op PC S A X Y P -> $e524 $76 $d5 $9f $ca $66 }T T{ $0079 M $e522 M $e523 M $e524 M -> $a8 $44 $79 $54 }T
$ed4f >PC $9b >S $28 >A $05 >X $a9 >Y $26 >P $006a $9b >M $ed4f $44 >M $ed50 $6a >M $ed51 $bf >M
T{ op PC S A X Y P -> $ed51 $9b $28 $05 $a9 $26 }T T{ $006a M $ed4f M $ed50 M $ed51 M -> $9b $44 $6a $bf }T
$32f2 >PC $78 >S $26 >A $d9 >X $a9 >Y $66 >P $00cd $8c >M $32f2 $44 >M $32f3 $cd >M $32f4 $16 >M
T{ op PC S A X Y P -> $32f4 $78 $26 $d9 $a9 $66 }T T{ $00cd M $32f2 M $32f3 M $32f4 M -> $8c $44 $cd $16 }T
$d311 >PC $96 >S $c3 >A $2b >X $44 >Y $27 >P $00d0 $5f >M $d311 $44 >M $d312 $d0 >M $d313 $7b >M
T{ op PC S A X Y P -> $d313 $96 $c3 $2b $44 $27 }T T{ $00d0 M $d311 M $d312 M $d313 M -> $5f $44 $d0 $7b }T
$2745 >PC $f3 >S $3d >A $e9 >X $52 >Y $a1 >P $0002 $8c >M $2745 $44 >M $2746 $02 >M $2747 $03 >M
T{ op PC S A X Y P -> $2747 $f3 $3d $e9 $52 $a1 }T T{ $0002 M $2745 M $2746 M $2747 M -> $8c $44 $02 $03 }T
( 45 )
$4ad6 >PC $f6 >S $90 >A $81 >X $6f >Y $e6 >P $0020 $c1 >M $4ad6 $45 >M $4ad7 $20 >M $4ad8 $bb >M
T{ op PC S A X Y P -> $4ad8 $f6 $51 $81 $6f $64 }T T{ $0020 M $4ad6 M $4ad7 M $4ad8 M -> $c1 $45 $20 $bb }T
$6659 >PC $10 >S $9e >A $40 >X $6b >Y $a0 >P $00ac $21 >M $6659 $45 >M $665a $ac >M $665b $5a >M
T{ op PC S A X Y P -> $665b $10 $bf $40 $6b $a0 }T T{ $00ac M $6659 M $665a M $665b M -> $21 $45 $ac $5a }T
$b07a >PC $93 >S $ee >A $2b >X $f6 >Y $e2 >P $0093 $1f >M $b07a $45 >M $b07b $93 >M $b07c $cd >M
T{ op PC S A X Y P -> $b07c $93 $f1 $2b $f6 $e0 }T T{ $0093 M $b07a M $b07b M $b07c M -> $1f $45 $93 $cd }T
$8b79 >PC $25 >S $ae >A $dd >X $1a >Y $e3 >P $0098 $eb >M $8b79 $45 >M $8b7a $98 >M $8b7b $ea >M
T{ op PC S A X Y P -> $8b7b $25 $45 $dd $1a $61 }T T{ $0098 M $8b79 M $8b7a M $8b7b M -> $eb $45 $98 $ea }T
$0fc2 >PC $ae >S $4e >A $d6 >X $f3 >Y $a0 >P $00bd $fb >M $0fc2 $45 >M $0fc3 $bd >M $0fc4 $aa >M
T{ op PC S A X Y P -> $0fc4 $ae $b5 $d6 $f3 $a0 }T T{ $00bd M $0fc2 M $0fc3 M $0fc4 M -> $fb $45 $bd $aa }T
$a65b >PC $43 >S $f3 >A $8f >X $70 >Y $21 >P $00cf $04 >M $a65b $45 >M $a65c $cf >M $a65d $87 >M
T{ op PC S A X Y P -> $a65d $43 $f7 $8f $70 $a1 }T T{ $00cf M $a65b M $a65c M $a65d M -> $04 $45 $cf $87 }T
$fc17 >PC $f5 >S $22 >A $a1 >X $36 >Y $25 >P $0058 $63 >M $fc17 $45 >M $fc18 $58 >M $fc19 $db >M
T{ op PC S A X Y P -> $fc19 $f5 $41 $a1 $36 $25 }T T{ $0058 M $fc17 M $fc18 M $fc19 M -> $63 $45 $58 $db }T
$66c4 >PC $d9 >S $0c >A $a6 >X $66 >Y $e0 >P $0066 $39 >M $66c4 $45 >M $66c5 $66 >M $66c6 $af >M
T{ op PC S A X Y P -> $66c6 $d9 $35 $a6 $66 $60 }T T{ $0066 M $66c4 M $66c5 M $66c6 M -> $39 $45 $66 $af }T
$18ef >PC $a5 >S $e7 >A $b7 >X $2d >Y $67 >P $00af $a3 >M $18ef $45 >M $18f0 $af >M $18f1 $8e >M
T{ op PC S A X Y P -> $18f1 $a5 $44 $b7 $2d $65 }T T{ $00af M $18ef M $18f0 M $18f1 M -> $a3 $45 $af $8e }T
$c882 >PC $e7 >S $dd >A $65 >X $fb >Y $21 >P $00fc $21 >M $c882 $45 >M $c883 $fc >M $c884 $d7 >M
T{ op PC S A X Y P -> $c884 $e7 $fc $65 $fb $a1 }T T{ $00fc M $c882 M $c883 M $c884 M -> $21 $45 $fc $d7 }T
$61dd >PC $90 >S $1a >A $b1 >X $f4 >Y $e0 >P $00de $c8 >M $61dd $45 >M $61de $de >M $61df $c7 >M
T{ op PC S A X Y P -> $61df $90 $d2 $b1 $f4 $e0 }T T{ $00de M $61dd M $61de M $61df M -> $c8 $45 $de $c7 }T
$9ad9 >PC $4d >S $fb >A $bd >X $fb >Y $e6 >P $00dd $42 >M $9ad9 $45 >M $9ada $dd >M $9adb $5c >M
T{ op PC S A X Y P -> $9adb $4d $b9 $bd $fb $e4 }T T{ $00dd M $9ad9 M $9ada M $9adb M -> $42 $45 $dd $5c }T
$4c3f >PC $d7 >S $80 >A $1f >X $05 >Y $a7 >P $0050 $16 >M $4c3f $45 >M $4c40 $50 >M $4c41 $29 >M
T{ op PC S A X Y P -> $4c41 $d7 $96 $1f $05 $a5 }T T{ $0050 M $4c3f M $4c40 M $4c41 M -> $16 $45 $50 $29 }T
$a1c9 >PC $2c >S $b3 >A $c3 >X $85 >Y $e0 >P $00c1 $9c >M $a1c9 $45 >M $a1ca $c1 >M $a1cb $7a >M
T{ op PC S A X Y P -> $a1cb $2c $2f $c3 $85 $60 }T T{ $00c1 M $a1c9 M $a1ca M $a1cb M -> $9c $45 $c1 $7a }T
$c31f >PC $0b >S $24 >A $3c >X $f3 >Y $24 >P $001e $81 >M $c31f $45 >M $c320 $1e >M $c321 $89 >M
T{ op PC S A X Y P -> $c321 $0b $a5 $3c $f3 $a4 }T T{ $001e M $c31f M $c320 M $c321 M -> $81 $45 $1e $89 }T
$139a >PC $c1 >S $a9 >A $87 >X $07 >Y $a6 >P $0034 $3a >M $139a $45 >M $139b $34 >M $139c $a6 >M
T{ op PC S A X Y P -> $139c $c1 $93 $87 $07 $a4 }T T{ $0034 M $139a M $139b M $139c M -> $3a $45 $34 $a6 }T
( 46 )
$b17e >PC $de >S $c5 >A $96 >X $e1 >Y $67 >P $00e5 $4b >M $b17e $46 >M $b17f $e5 >M $b180 $88 >M
T{ op PC S A X Y P -> $b180 $de $c5 $96 $e1 $65 }T T{ $00e5 M $b17e M $b17f M $b180 M -> $25 $46 $e5 $88 }T
$fab0 >PC $43 >S $7e >A $90 >X $b8 >Y $a6 >P $003f $cb >M $fab0 $46 >M $fab1 $3f >M $fab2 $bf >M
T{ op PC S A X Y P -> $fab2 $43 $7e $90 $b8 $25 }T T{ $003f M $fab0 M $fab1 M $fab2 M -> $65 $46 $3f $bf }T
$bd2f >PC $b7 >S $85 >A $cc >X $02 >Y $e0 >P $008c $c0 >M $bd2f $46 >M $bd30 $8c >M $bd31 $bb >M
T{ op PC S A X Y P -> $bd31 $b7 $85 $cc $02 $60 }T T{ $008c M $bd2f M $bd30 M $bd31 M -> $60 $46 $8c $bb }T
$2f80 >PC $d2 >S $88 >A $f6 >X $90 >Y $e5 >P $00b3 $d9 >M $2f80 $46 >M $2f81 $b3 >M $2f82 $03 >M
T{ op PC S A X Y P -> $2f82 $d2 $88 $f6 $90 $65 }T T{ $00b3 M $2f80 M $2f81 M $2f82 M -> $6c $46 $b3 $03 }T
$efa7 >PC $f0 >S $ac >A $ce >X $8b >Y $e7 >P $003c $ff >M $efa7 $46 >M $efa8 $3c >M $efa9 $1c >M
T{ op PC S A X Y P -> $efa9 $f0 $ac $ce $8b $65 }T T{ $003c M $efa7 M $efa8 M $efa9 M -> $7f $46 $3c $1c }T
$377e >PC $9d >S $f8 >A $f9 >X $08 >Y $e2 >P $0091 $6b >M $377e $46 >M $377f $91 >M $3780 $ae >M
T{ op PC S A X Y P -> $3780 $9d $f8 $f9 $08 $61 }T T{ $0091 M $377e M $377f M $3780 M -> $35 $46 $91 $ae }T
$20dc >PC $5f >S $32 >A $ac >X $8c >Y $a0 >P $0080 $e5 >M $20dc $46 >M $20dd $80 >M $20de $fd >M
T{ op PC S A X Y P -> $20de $5f $32 $ac $8c $21 }T T{ $0080 M $20dc M $20dd M $20de M -> $72 $46 $80 $fd }T
$ff8e >PC $4e >S $07 >A $87 >X $42 >Y $24 >P $003b $10 >M $ff8e $46 >M $ff8f $3b >M $ff90 $54 >M
T{ op PC S A X Y P -> $ff90 $4e $07 $87 $42 $24 }T T{ $003b M $ff8e M $ff8f M $ff90 M -> $08 $46 $3b $54 }T
$c4ab >PC $cf >S $4f >A $b1 >X $43 >Y $a5 >P $00ac $47 >M $c4ab $46 >M $c4ac $ac >M $c4ad $21 >M
T{ op PC S A X Y P -> $c4ad $cf $4f $b1 $43 $25 }T T{ $00ac M $c4ab M $c4ac M $c4ad M -> $23 $46 $ac $21 }T
$dab4 >PC $57 >S $60 >A $38 >X $bd >Y $e0 >P $00d2 $1b >M $dab4 $46 >M $dab5 $d2 >M $dab6 $a3 >M
T{ op PC S A X Y P -> $dab6 $57 $60 $38 $bd $61 }T T{ $00d2 M $dab4 M $dab5 M $dab6 M -> $0d $46 $d2 $a3 }T
$b02a >PC $cc >S $4b >A $05 >X $1c >Y $a1 >P $009b $9f >M $b02a $46 >M $b02b $9b >M $b02c $69 >M
T{ op PC S A X Y P -> $b02c $cc $4b $05 $1c $21 }T T{ $009b M $b02a M $b02b M $b02c M -> $4f $46 $9b $69 }T
$6a35 >PC $4a >S $07 >A $f7 >X $95 >Y $a1 >P $0060 $08 >M $6a35 $46 >M $6a36 $60 >M $6a37 $63 >M
T{ op PC S A X Y P -> $6a37 $4a $07 $f7 $95 $20 }T T{ $0060 M $6a35 M $6a36 M $6a37 M -> $04 $46 $60 $63 }T
$c886 >PC $86 >S $30 >A $29 >X $80 >Y $a6 >P $00be $3e >M $c886 $46 >M $c887 $be >M $c888 $c7 >M
T{ op PC S A X Y P -> $c888 $86 $30 $29 $80 $24 }T T{ $00be M $c886 M $c887 M $c888 M -> $1f $46 $be $c7 }T
$ced7 >PC $b1 >S $7f >A $18 >X $5a >Y $66 >P $00ba $74 >M $ced7 $46 >M $ced8 $ba >M $ced9 $33 >M
T{ op PC S A X Y P -> $ced9 $b1 $7f $18 $5a $64 }T T{ $00ba M $ced7 M $ced8 M $ced9 M -> $3a $46 $ba $33 }T
$900b >PC $86 >S $0e >A $7d >X $2e >Y $60 >P $0044 $12 >M $900b $46 >M $900c $44 >M $900d $6f >M
T{ op PC S A X Y P -> $900d $86 $0e $7d $2e $60 }T T{ $0044 M $900b M $900c M $900d M -> $09 $46 $44 $6f }T
$a2a8 >PC $93 >S $8f >A $93 >X $d8 >Y $26 >P $008b $ad >M $a2a8 $46 >M $a2a9 $8b >M $a2aa $f1 >M
T{ op PC S A X Y P -> $a2aa $93 $8f $93 $d8 $25 }T T{ $008b M $a2a8 M $a2a9 M $a2aa M -> $56 $46 $8b $f1 }T
( 47 )
$f9f2 >PC $61 >S $3d >A $1b >X $9a >Y $a4 >P $0089 $ab >M $f9f2 $47 >M $f9f3 $89 >M $f9f4 $32 >M
T{ op PC S A X Y P -> $f9f4 $61 $3d $1b $9a $a4 }T T{ $0089 M $f9f2 M $f9f3 M $f9f4 M -> $ab $47 $89 $32 }T
$c15f >PC $69 >S $83 >A $71 >X $fe >Y $26 >P $0011 $1e >M $c15f $47 >M $c160 $11 >M $c161 $75 >M
T{ op PC S A X Y P -> $c161 $69 $83 $71 $fe $26 }T T{ $0011 M $c15f M $c160 M $c161 M -> $0e $47 $11 $75 }T
$d1e4 >PC $23 >S $10 >A $75 >X $99 >Y $66 >P $00da $98 >M $d1e4 $47 >M $d1e5 $da >M $d1e6 $84 >M
T{ op PC S A X Y P -> $d1e6 $23 $10 $75 $99 $66 }T T{ $00da M $d1e4 M $d1e5 M $d1e6 M -> $88 $47 $da $84 }T
$ef33 >PC $7d >S $1f >A $42 >X $e9 >Y $27 >P $00b4 $6a >M $ef33 $47 >M $ef34 $b4 >M $ef35 $79 >M
T{ op PC S A X Y P -> $ef35 $7d $1f $42 $e9 $27 }T T{ $00b4 M $ef33 M $ef34 M $ef35 M -> $6a $47 $b4 $79 }T
$8c0b >PC $da >S $13 >A $90 >X $30 >Y $e2 >P $008d $a5 >M $8c0b $47 >M $8c0c $8d >M $8c0d $f4 >M
T{ op PC S A X Y P -> $8c0d $da $13 $90 $30 $e2 }T T{ $008d M $8c0b M $8c0c M $8c0d M -> $a5 $47 $8d $f4 }T
$a233 >PC $e1 >S $03 >A $b7 >X $fd >Y $20 >P $0069 $fb >M $a233 $47 >M $a234 $69 >M $a235 $19 >M
T{ op PC S A X Y P -> $a235 $e1 $03 $b7 $fd $20 }T T{ $0069 M $a233 M $a234 M $a235 M -> $eb $47 $69 $19 }T
$c551 >PC $1c >S $d9 >A $06 >X $79 >Y $a7 >P $00a8 $64 >M $c551 $47 >M $c552 $a8 >M $c553 $63 >M
T{ op PC S A X Y P -> $c553 $1c $d9 $06 $79 $a7 }T T{ $00a8 M $c551 M $c552 M $c553 M -> $64 $47 $a8 $63 }T
$53f1 >PC $0c >S $69 >A $46 >X $8d >Y $a4 >P $00a1 $73 >M $53f1 $47 >M $53f2 $a1 >M $53f3 $97 >M
T{ op PC S A X Y P -> $53f3 $0c $69 $46 $8d $a4 }T T{ $00a1 M $53f1 M $53f2 M $53f3 M -> $63 $47 $a1 $97 }T
$55b5 >PC $50 >S $a2 >A $6c >X $06 >Y $20 >P $00c9 $94 >M $55b5 $47 >M $55b6 $c9 >M $55b7 $43 >M
T{ op PC S A X Y P -> $55b7 $50 $a2 $6c $06 $20 }T T{ $00c9 M $55b5 M $55b6 M $55b7 M -> $84 $47 $c9 $43 }T
$ba13 >PC $36 >S $a7 >A $fa >X $0d >Y $a2 >P $00f6 $8d >M $ba13 $47 >M $ba14 $f6 >M $ba15 $3c >M
T{ op PC S A X Y P -> $ba15 $36 $a7 $fa $0d $a2 }T T{ $00f6 M $ba13 M $ba14 M $ba15 M -> $8d $47 $f6 $3c }T
$9a1c >PC $30 >S $a9 >A $46 >X $68 >Y $22 >P $00e6 $3c >M $9a1c $47 >M $9a1d $e6 >M $9a1e $34 >M
T{ op PC S A X Y P -> $9a1e $30 $a9 $46 $68 $22 }T T{ $00e6 M $9a1c M $9a1d M $9a1e M -> $2c $47 $e6 $34 }T
$aa10 >PC $fb >S $a5 >A $67 >X $70 >Y $e6 >P $00d4 $ef >M $aa10 $47 >M $aa11 $d4 >M $aa12 $37 >M
T{ op PC S A X Y P -> $aa12 $fb $a5 $67 $70 $e6 }T T{ $00d4 M $aa10 M $aa11 M $aa12 M -> $ef $47 $d4 $37 }T
$bae5 >PC $dd >S $be >A $fa >X $0d >Y $a6 >P $00e1 $0f >M $bae5 $47 >M $bae6 $e1 >M $bae7 $42 >M
T{ op PC S A X Y P -> $bae7 $dd $be $fa $0d $a6 }T T{ $00e1 M $bae5 M $bae6 M $bae7 M -> $0f $47 $e1 $42 }T
$1236 >PC $b2 >S $c9 >A $aa >X $87 >Y $a3 >P $00b4 $8d >M $1236 $47 >M $1237 $b4 >M $1238 $a1 >M
T{ op PC S A X Y P -> $1238 $b2 $c9 $aa $87 $a3 }T T{ $00b4 M $1236 M $1237 M $1238 M -> $8d $47 $b4 $a1 }T
$5118 >PC $b5 >S $d0 >A $35 >X $e4 >Y $a7 >P $0055 $33 >M $5118 $47 >M $5119 $55 >M $511a $53 >M
T{ op PC S A X Y P -> $511a $b5 $d0 $35 $e4 $a7 }T T{ $0055 M $5118 M $5119 M $511a M -> $23 $47 $55 $53 }T
$737f >PC $0d >S $75 >A $9c >X $85 >Y $60 >P $00d6 $72 >M $737f $47 >M $7380 $d6 >M $7381 $c6 >M
T{ op PC S A X Y P -> $7381 $0d $75 $9c $85 $60 }T T{ $00d6 M $737f M $7380 M $7381 M -> $62 $47 $d6 $c6 }T
( 48 )
$e1b6 >PC $ee >S $75 >A $e7 >X $5a >Y $a6 >P $e1b6 $48 >M $e1b7 $7a >M $e1b8 $cc >M
T{ op PC S A X Y P -> $e1b7 $ed $75 $e7 $5a $a6 }T T{ $01ee M $e1b6 M $e1b7 M $e1b8 M -> $75 $48 $7a $cc }T
$638d >PC $44 >S $df >A $30 >X $23 >Y $63 >P $638d $48 >M $638e $ef >M $638f $11 >M
T{ op PC S A X Y P -> $638e $43 $df $30 $23 $63 }T T{ $0144 M $638d M $638e M $638f M -> $df $48 $ef $11 }T
$90ba >PC $6b >S $69 >A $4b >X $40 >Y $e1 >P $90ba $48 >M $90bb $9a >M $90bc $25 >M
T{ op PC S A X Y P -> $90bb $6a $69 $4b $40 $e1 }T T{ $016b M $90ba M $90bb M $90bc M -> $69 $48 $9a $25 }T
$2444 >PC $45 >S $68 >A $1b >X $1e >Y $67 >P $2444 $48 >M $2445 $83 >M $2446 $85 >M
T{ op PC S A X Y P -> $2445 $44 $68 $1b $1e $67 }T T{ $0145 M $2444 M $2445 M $2446 M -> $68 $48 $83 $85 }T
$a2f7 >PC $28 >S $d9 >A $22 >X $83 >Y $22 >P $a2f7 $48 >M $a2f8 $b7 >M $a2f9 $71 >M
T{ op PC S A X Y P -> $a2f8 $27 $d9 $22 $83 $22 }T T{ $0128 M $a2f7 M $a2f8 M $a2f9 M -> $d9 $48 $b7 $71 }T
$becc >PC $3f >S $c8 >A $6b >X $b2 >Y $a2 >P $becc $48 >M $becd $2d >M $bece $70 >M
T{ op PC S A X Y P -> $becd $3e $c8 $6b $b2 $a2 }T T{ $013f M $becc M $becd M $bece M -> $c8 $48 $2d $70 }T
$0ba8 >PC $c6 >S $43 >A $0a >X $57 >Y $22 >P $0ba8 $48 >M $0ba9 $19 >M $0baa $6e >M
T{ op PC S A X Y P -> $0ba9 $c5 $43 $0a $57 $22 }T T{ $01c6 M $0ba8 M $0ba9 M $0baa M -> $43 $48 $19 $6e }T
$4301 >PC $a5 >S $fa >A $0c >X $4a >Y $66 >P $4301 $48 >M $4302 $d8 >M $4303 $17 >M
T{ op PC S A X Y P -> $4302 $a4 $fa $0c $4a $66 }T T{ $01a5 M $4301 M $4302 M $4303 M -> $fa $48 $d8 $17 }T
$fb59 >PC $0c >S $8d >A $5a >X $5a >Y $a2 >P $fb59 $48 >M $fb5a $3f >M $fb5b $0d >M
T{ op PC S A X Y P -> $fb5a $0b $8d $5a $5a $a2 }T T{ $010c M $fb59 M $fb5a M $fb5b M -> $8d $48 $3f $0d }T
$0ecb >PC $32 >S $56 >A $a6 >X $ac >Y $e4 >P $0ecb $48 >M $0ecc $cc >M $0ecd $23 >M
T{ op PC S A X Y P -> $0ecc $31 $56 $a6 $ac $e4 }T T{ $0132 M $0ecb M $0ecc M $0ecd M -> $56 $48 $cc $23 }T
$2bba >PC $9f >S $17 >A $ca >X $50 >Y $66 >P $2bba $48 >M $2bbb $e7 >M $2bbc $2b >M
T{ op PC S A X Y P -> $2bbb $9e $17 $ca $50 $66 }T T{ $019f M $2bba M $2bbb M $2bbc M -> $17 $48 $e7 $2b }T
$b3e3 >PC $88 >S $18 >A $01 >X $b4 >Y $e3 >P $b3e3 $48 >M $b3e4 $2d >M $b3e5 $44 >M
T{ op PC S A X Y P -> $b3e4 $87 $18 $01 $b4 $e3 }T T{ $0188 M $b3e3 M $b3e4 M $b3e5 M -> $18 $48 $2d $44 }T
$3080 >PC $94 >S $21 >A $09 >X $1d >Y $20 >P $3080 $48 >M $3081 $ce >M $3082 $c5 >M
T{ op PC S A X Y P -> $3081 $93 $21 $09 $1d $20 }T T{ $0194 M $3080 M $3081 M $3082 M -> $21 $48 $ce $c5 }T
$285e >PC $36 >S $2a >A $33 >X $1e >Y $a3 >P $285e $48 >M $285f $2a >M $2860 $14 >M
T{ op PC S A X Y P -> $285f $35 $2a $33 $1e $a3 }T T{ $0136 M $285e M $285f M $2860 M -> $2a $48 $2a $14 }T
$1484 >PC $28 >S $49 >A $7b >X $6b >Y $a2 >P $1484 $48 >M $1485 $22 >M $1486 $c3 >M
T{ op PC S A X Y P -> $1485 $27 $49 $7b $6b $a2 }T T{ $0128 M $1484 M $1485 M $1486 M -> $49 $48 $22 $c3 }T
$0af2 >PC $f8 >S $8f >A $5e >X $1a >Y $63 >P $0af2 $48 >M $0af3 $9a >M $0af4 $10 >M
T{ op PC S A X Y P -> $0af3 $f7 $8f $5e $1a $63 }T T{ $01f8 M $0af2 M $0af3 M $0af4 M -> $8f $48 $9a $10 }T
( 49 )
$6db1 >PC $af >S $54 >A $03 >X $ec >Y $e0 >P $6db1 $49 >M $6db2 $36 >M $6db3 $30 >M
T{ op PC S A X Y P -> $6db3 $af $62 $03 $ec $60 }T T{ $6db1 M $6db2 M $6db3 M -> $49 $36 $30 }T
$a787 >PC $82 >S $ad >A $1f >X $71 >Y $60 >P $a787 $49 >M $a788 $00 >M $a789 $ed >M
T{ op PC S A X Y P -> $a789 $82 $ad $1f $71 $e0 }T T{ $a787 M $a788 M $a789 M -> $49 $00 $ed }T
$8dd1 >PC $6c >S $98 >A $1d >X $96 >Y $e6 >P $8dd1 $49 >M $8dd2 $c8 >M $8dd3 $b0 >M
T{ op PC S A X Y P -> $8dd3 $6c $50 $1d $96 $64 }T T{ $8dd1 M $8dd2 M $8dd3 M -> $49 $c8 $b0 }T
$5080 >PC $45 >S $bd >A $0e >X $7c >Y $65 >P $5080 $49 >M $5081 $03 >M $5082 $ce >M
T{ op PC S A X Y P -> $5082 $45 $be $0e $7c $e5 }T T{ $5080 M $5081 M $5082 M -> $49 $03 $ce }T
$ffcd >PC $a8 >S $80 >A $09 >X $84 >Y $21 >P $ffcd $49 >M $ffce $d6 >M $ffcf $8a >M
T{ op PC S A X Y P -> $ffcf $a8 $56 $09 $84 $21 }T T{ $ffcd M $ffce M $ffcf M -> $49 $d6 $8a }T
$435b >PC $0e >S $97 >A $fb >X $e5 >Y $a3 >P $435b $49 >M $435c $e4 >M $435d $a2 >M
T{ op PC S A X Y P -> $435d $0e $73 $fb $e5 $21 }T T{ $435b M $435c M $435d M -> $49 $e4 $a2 }T
$1ece >PC $4f >S $b8 >A $4b >X $4b >Y $62 >P $1ece $49 >M $1ecf $2d >M $1ed0 $d0 >M
T{ op PC S A X Y P -> $1ed0 $4f $95 $4b $4b $e0 }T T{ $1ece M $1ecf M $1ed0 M -> $49 $2d $d0 }T
$2e0b >PC $57 >S $db >A $51 >X $30 >Y $e4 >P $2e0b $49 >M $2e0c $bb >M $2e0d $e4 >M
T{ op PC S A X Y P -> $2e0d $57 $60 $51 $30 $64 }T T{ $2e0b M $2e0c M $2e0d M -> $49 $bb $e4 }T
$9ea9 >PC $55 >S $c8 >A $f3 >X $e5 >Y $a5 >P $9ea9 $49 >M $9eaa $1c >M $9eab $d3 >M
T{ op PC S A X Y P -> $9eab $55 $d4 $f3 $e5 $a5 }T T{ $9ea9 M $9eaa M $9eab M -> $49 $1c $d3 }T
$8598 >PC $58 >S $35 >A $2b >X $31 >Y $a2 >P $8598 $49 >M $8599 $a8 >M $859a $26 >M
T{ op PC S A X Y P -> $859a $58 $9d $2b $31 $a0 }T T{ $8598 M $8599 M $859a M -> $49 $a8 $26 }T
$e23e >PC $3a >S $80 >A $b9 >X $16 >Y $e5 >P $e23e $49 >M $e23f $e1 >M $e240 $32 >M
T{ op PC S A X Y P -> $e240 $3a $61 $b9 $16 $65 }T T{ $e23e M $e23f M $e240 M -> $49 $e1 $32 }T
$cf47 >PC $75 >S $ae >A $40 >X $64 >Y $e0 >P $cf47 $49 >M $cf48 $cf >M $cf49 $87 >M
T{ op PC S A X Y P -> $cf49 $75 $61 $40 $64 $60 }T T{ $cf47 M $cf48 M $cf49 M -> $49 $cf $87 }T
$b938 >PC $f8 >S $ef >A $c8 >X $bc >Y $e2 >P $b938 $49 >M $b939 $14 >M $b93a $e9 >M
T{ op PC S A X Y P -> $b93a $f8 $fb $c8 $bc $e0 }T T{ $b938 M $b939 M $b93a M -> $49 $14 $e9 }T
$ff9e >PC $3c >S $62 >A $f4 >X $fe >Y $e4 >P $ff9e $49 >M $ff9f $cd >M $ffa0 $c9 >M
T{ op PC S A X Y P -> $ffa0 $3c $af $f4 $fe $e4 }T T{ $ff9e M $ff9f M $ffa0 M -> $49 $cd $c9 }T
$3aea >PC $e4 >S $71 >A $f2 >X $9c >Y $e2 >P $3aea $49 >M $3aeb $29 >M $3aec $33 >M
T{ op PC S A X Y P -> $3aec $e4 $58 $f2 $9c $60 }T T{ $3aea M $3aeb M $3aec M -> $49 $29 $33 }T
$9cc5 >PC $0b >S $95 >A $6a >X $f8 >Y $21 >P $9cc5 $49 >M $9cc6 $95 >M $9cc7 $87 >M
T{ op PC S A X Y P -> $9cc7 $0b $00 $6a $f8 $23 }T T{ $9cc5 M $9cc6 M $9cc7 M -> $49 $95 $87 }T
( 4a )
$d088 >PC $d7 >S $4a >A $25 >X $27 >Y $a4 >P $d088 $4a >M $d089 $93 >M $d08a $fe >M
T{ op PC S A X Y P -> $d089 $d7 $25 $25 $27 $24 }T T{ $d088 M $d089 M $d08a M -> $4a $93 $fe }T
$f347 >PC $ed >S $1f >A $d9 >X $23 >Y $e7 >P $f347 $4a >M $f348 $85 >M $f349 $7f >M
T{ op PC S A X Y P -> $f348 $ed $0f $d9 $23 $65 }T T{ $f347 M $f348 M $f349 M -> $4a $85 $7f }T
$fdfc >PC $84 >S $fc >A $b4 >X $c6 >Y $e6 >P $fdfc $4a >M $fdfd $c3 >M $fdfe $f8 >M
T{ op PC S A X Y P -> $fdfd $84 $7e $b4 $c6 $64 }T T{ $fdfc M $fdfd M $fdfe M -> $4a $c3 $f8 }T
$c4e4 >PC $c0 >S $a7 >A $09 >X $3c >Y $63 >P $c4e4 $4a >M $c4e5 $b6 >M $c4e6 $9c >M
T{ op PC S A X Y P -> $c4e5 $c0 $53 $09 $3c $61 }T T{ $c4e4 M $c4e5 M $c4e6 M -> $4a $b6 $9c }T
$c31e >PC $fd >S $f2 >A $8b >X $b5 >Y $64 >P $c31e $4a >M $c31f $14 >M $c320 $f7 >M
T{ op PC S A X Y P -> $c31f $fd $79 $8b $b5 $64 }T T{ $c31e M $c31f M $c320 M -> $4a $14 $f7 }T
$e24b >PC $4d >S $ae >A $86 >X $12 >Y $67 >P $e24b $4a >M $e24c $4b >M $e24d $18 >M
T{ op PC S A X Y P -> $e24c $4d $57 $86 $12 $64 }T T{ $e24b M $e24c M $e24d M -> $4a $4b $18 }T
$abcd >PC $7b >S $f0 >A $2b >X $e2 >Y $e0 >P $abcd $4a >M $abce $4e >M $abcf $f7 >M
T{ op PC S A X Y P -> $abce $7b $78 $2b $e2 $60 }T T{ $abcd M $abce M $abcf M -> $4a $4e $f7 }T
$7e06 >PC $46 >S $8d >A $7f >X $e8 >Y $a2 >P $7e06 $4a >M $7e07 $99 >M $7e08 $8c >M
T{ op PC S A X Y P -> $7e07 $46 $46 $7f $e8 $21 }T T{ $7e06 M $7e07 M $7e08 M -> $4a $99 $8c }T
$a5ec >PC $c2 >S $d9 >A $c0 >X $da >Y $e1 >P $a5ec $4a >M $a5ed $cc >M $a5ee $5a >M
T{ op PC S A X Y P -> $a5ed $c2 $6c $c0 $da $61 }T T{ $a5ec M $a5ed M $a5ee M -> $4a $cc $5a }T
$e448 >PC $0d >S $7e >A $dd >X $36 >Y $20 >P $e448 $4a >M $e449 $2b >M $e44a $dc >M
T{ op PC S A X Y P -> $e449 $0d $3f $dd $36 $20 }T T{ $e448 M $e449 M $e44a M -> $4a $2b $dc }T
$6a49 >PC $b2 >S $09 >A $f6 >X $5f >Y $a1 >P $6a49 $4a >M $6a4a $83 >M $6a4b $dc >M
T{ op PC S A X Y P -> $6a4a $b2 $04 $f6 $5f $21 }T T{ $6a49 M $6a4a M $6a4b M -> $4a $83 $dc }T
$a093 >PC $e9 >S $3a >A $39 >X $e3 >Y $e3 >P $a093 $4a >M $a094 $5c >M $a095 $29 >M
T{ op PC S A X Y P -> $a094 $e9 $1d $39 $e3 $60 }T T{ $a093 M $a094 M $a095 M -> $4a $5c $29 }T
$8762 >PC $10 >S $b2 >A $fc >X $c7 >Y $63 >P $8762 $4a >M $8763 $85 >M $8764 $94 >M
T{ op PC S A X Y P -> $8763 $10 $59 $fc $c7 $60 }T T{ $8762 M $8763 M $8764 M -> $4a $85 $94 }T
$25ba >PC $83 >S $a0 >A $db >X $05 >Y $60 >P $25ba $4a >M $25bb $4c >M $25bc $db >M
T{ op PC S A X Y P -> $25bb $83 $50 $db $05 $60 }T T{ $25ba M $25bb M $25bc M -> $4a $4c $db }T
$54d7 >PC $e8 >S $02 >A $2f >X $b3 >Y $e2 >P $54d7 $4a >M $54d8 $19 >M $54d9 $b0 >M
T{ op PC S A X Y P -> $54d8 $e8 $01 $2f $b3 $60 }T T{ $54d7 M $54d8 M $54d9 M -> $4a $19 $b0 }T
$c1e7 >PC $4c >S $f6 >A $cc >X $7b >Y $20 >P $c1e7 $4a >M $c1e8 $12 >M $c1e9 $5e >M
T{ op PC S A X Y P -> $c1e8 $4c $7b $cc $7b $20 }T T{ $c1e7 M $c1e8 M $c1e9 M -> $4a $12 $5e }T
( 4b )
$6e41 >PC $cd >S $df >A $61 >X $33 >Y $67 >P $6e41 $4b >M $6e42 $15 >M $6e43 $63 >M
T{ op PC S A X Y P -> $6e42 $cd $df $61 $33 $67 }T T{ $6e41 M $6e42 M $6e43 M -> $4b $15 $63 }T
$5c91 >PC $2f >S $8b >A $52 >X $f1 >Y $22 >P $5c91 $4b >M $5c92 $fc >M $5c93 $c4 >M
T{ op PC S A X Y P -> $5c92 $2f $8b $52 $f1 $22 }T T{ $5c91 M $5c92 M $5c93 M -> $4b $fc $c4 }T
$1134 >PC $fc >S $07 >A $80 >X $bb >Y $a3 >P $1134 $4b >M $1135 $d6 >M $1136 $db >M
T{ op PC S A X Y P -> $1135 $fc $07 $80 $bb $a3 }T T{ $1134 M $1135 M $1136 M -> $4b $d6 $db }T
$d37c >PC $3c >S $4f >A $fe >X $1e >Y $e5 >P $d37c $4b >M $d37d $0e >M $d37e $68 >M
T{ op PC S A X Y P -> $d37d $3c $4f $fe $1e $e5 }T T{ $d37c M $d37d M $d37e M -> $4b $0e $68 }T
$fb83 >PC $40 >S $de >A $fb >X $9b >Y $a0 >P $fb83 $4b >M $fb84 $18 >M $fb85 $ce >M
T{ op PC S A X Y P -> $fb84 $40 $de $fb $9b $a0 }T T{ $fb83 M $fb84 M $fb85 M -> $4b $18 $ce }T
$af9f >PC $55 >S $37 >A $6a >X $cf >Y $61 >P $af9f $4b >M $afa0 $35 >M $afa1 $03 >M
T{ op PC S A X Y P -> $afa0 $55 $37 $6a $cf $61 }T T{ $af9f M $afa0 M $afa1 M -> $4b $35 $03 }T
$d91b >PC $a0 >S $0b >A $bb >X $32 >Y $a4 >P $d91b $4b >M $d91c $2e >M $d91d $a7 >M
T{ op PC S A X Y P -> $d91c $a0 $0b $bb $32 $a4 }T T{ $d91b M $d91c M $d91d M -> $4b $2e $a7 }T
$7310 >PC $bd >S $2f >A $09 >X $8a >Y $20 >P $7310 $4b >M $7311 $49 >M $7312 $5a >M
T{ op PC S A X Y P -> $7311 $bd $2f $09 $8a $20 }T T{ $7310 M $7311 M $7312 M -> $4b $49 $5a }T
$3a09 >PC $64 >S $ef >A $06 >X $43 >Y $23 >P $3a09 $4b >M $3a0a $2e >M $3a0b $3f >M
T{ op PC S A X Y P -> $3a0a $64 $ef $06 $43 $23 }T T{ $3a09 M $3a0a M $3a0b M -> $4b $2e $3f }T
$862f >PC $77 >S $80 >A $f2 >X $27 >Y $20 >P $862f $4b >M $8630 $59 >M $8631 $67 >M
T{ op PC S A X Y P -> $8630 $77 $80 $f2 $27 $20 }T T{ $862f M $8630 M $8631 M -> $4b $59 $67 }T
$1faf >PC $47 >S $fd >A $91 >X $2d >Y $a1 >P $1faf $4b >M $1fb0 $1a >M $1fb1 $b9 >M
T{ op PC S A X Y P -> $1fb0 $47 $fd $91 $2d $a1 }T T{ $1faf M $1fb0 M $1fb1 M -> $4b $1a $b9 }T
$30cd >PC $0b >S $3d >A $db >X $b6 >Y $61 >P $30cd $4b >M $30ce $ac >M $30cf $ae >M
T{ op PC S A X Y P -> $30ce $0b $3d $db $b6 $61 }T T{ $30cd M $30ce M $30cf M -> $4b $ac $ae }T
$4c16 >PC $22 >S $68 >A $20 >X $e2 >Y $e7 >P $4c16 $4b >M $4c17 $7b >M $4c18 $97 >M
T{ op PC S A X Y P -> $4c17 $22 $68 $20 $e2 $e7 }T T{ $4c16 M $4c17 M $4c18 M -> $4b $7b $97 }T
$eb06 >PC $f0 >S $4c >A $53 >X $d6 >Y $e1 >P $eb06 $4b >M $eb07 $6c >M $eb08 $0a >M
T{ op PC S A X Y P -> $eb07 $f0 $4c $53 $d6 $e1 }T T{ $eb06 M $eb07 M $eb08 M -> $4b $6c $0a }T
$c61b >PC $90 >S $48 >A $97 >X $92 >Y $e1 >P $c61b $4b >M $c61c $ca >M $c61d $a8 >M
T{ op PC S A X Y P -> $c61c $90 $48 $97 $92 $e1 }T T{ $c61b M $c61c M $c61d M -> $4b $ca $a8 }T
$d277 >PC $bb >S $09 >A $36 >X $6b >Y $e6 >P $d277 $4b >M $d278 $3f >M $d279 $3b >M
T{ op PC S A X Y P -> $d278 $bb $09 $36 $6b $e6 }T T{ $d277 M $d278 M $d279 M -> $4b $3f $3b }T
( 4c )
$e274 >PC $00 >S $93 >A $c3 >X $20 >Y $a3 >P $37bd $ea >M $e274 $4c >M $e275 $bd >M $e276 $37 >M
T{ op PC S A X Y P -> $37bd $00 $93 $c3 $20 $a3 }T T{ $37bd M $e274 M $e275 M $e276 M -> $ea $4c $bd $37 }T
$ea32 >PC $c8 >S $04 >A $24 >X $6f >Y $23 >P $d014 $8a >M $ea32 $4c >M $ea33 $14 >M $ea34 $d0 >M
T{ op PC S A X Y P -> $d014 $c8 $04 $24 $6f $23 }T T{ $d014 M $ea32 M $ea33 M $ea34 M -> $8a $4c $14 $d0 }T
$57ad >PC $29 >S $54 >A $00 >X $d0 >Y $25 >P $57ad $4c >M $57ae $23 >M $57af $eb >M $eb23 $b7 >M
T{ op PC S A X Y P -> $eb23 $29 $54 $00 $d0 $25 }T T{ $57ad M $57ae M $57af M $eb23 M -> $4c $23 $eb $b7 }T
$a3ce >PC $bc >S $0c >A $99 >X $30 >Y $67 >P $4194 $76 >M $a3ce $4c >M $a3cf $94 >M $a3d0 $41 >M
T{ op PC S A X Y P -> $4194 $bc $0c $99 $30 $67 }T T{ $4194 M $a3ce M $a3cf M $a3d0 M -> $76 $4c $94 $41 }T
$adfb >PC $1b >S $1b >A $f6 >X $d3 >Y $e0 >P $adfb $4c >M $adfc $d7 >M $adfd $bf >M $bfd7 $c0 >M
T{ op PC S A X Y P -> $bfd7 $1b $1b $f6 $d3 $e0 }T T{ $adfb M $adfc M $adfd M $bfd7 M -> $4c $d7 $bf $c0 }T
$b905 >PC $7c >S $f3 >A $4b >X $d9 >Y $e6 >P $066e $dd >M $b905 $4c >M $b906 $6e >M $b907 $06 >M
T{ op PC S A X Y P -> $066e $7c $f3 $4b $d9 $e6 }T T{ $066e M $b905 M $b906 M $b907 M -> $dd $4c $6e $06 }T
$ea1b >PC $a8 >S $32 >A $67 >X $4b >Y $25 >P $cd9f $b7 >M $ea1b $4c >M $ea1c $9f >M $ea1d $cd >M
T{ op PC S A X Y P -> $cd9f $a8 $32 $67 $4b $25 }T T{ $cd9f M $ea1b M $ea1c M $ea1d M -> $b7 $4c $9f $cd }T
$6987 >PC $17 >S $e1 >A $84 >X $b5 >Y $27 >P $0436 $5c >M $6987 $4c >M $6988 $36 >M $6989 $04 >M
T{ op PC S A X Y P -> $0436 $17 $e1 $84 $b5 $27 }T T{ $0436 M $6987 M $6988 M $6989 M -> $5c $4c $36 $04 }T
$3819 >PC $a3 >S $bd >A $82 >X $a0 >Y $24 >P $3819 $4c >M $381a $ee >M $381b $f9 >M $f9ee $7e >M
T{ op PC S A X Y P -> $f9ee $a3 $bd $82 $a0 $24 }T T{ $3819 M $381a M $381b M $f9ee M -> $4c $ee $f9 $7e }T
$ce80 >PC $7e >S $7f >A $00 >X $d6 >Y $66 >P $07ec $a9 >M $ce80 $4c >M $ce81 $ec >M $ce82 $07 >M
T{ op PC S A X Y P -> $07ec $7e $7f $00 $d6 $66 }T T{ $07ec M $ce80 M $ce81 M $ce82 M -> $a9 $4c $ec $07 }T
$9874 >PC $34 >S $98 >A $db >X $3d >Y $a7 >P $9874 $4c >M $9875 $c8 >M $9876 $b7 >M $b7c8 $e2 >M
T{ op PC S A X Y P -> $b7c8 $34 $98 $db $3d $a7 }T T{ $9874 M $9875 M $9876 M $b7c8 M -> $4c $c8 $b7 $e2 }T
$1467 >PC $28 >S $69 >A $fb >X $40 >Y $25 >P $1467 $4c >M $1468 $7e >M $1469 $35 >M $357e $b1 >M
T{ op PC S A X Y P -> $357e $28 $69 $fb $40 $25 }T T{ $1467 M $1468 M $1469 M $357e M -> $4c $7e $35 $b1 }T
$6fce >PC $1c >S $6c >A $71 >X $1c >Y $e0 >P $6fce $4c >M $6fcf $8c >M $6fd0 $b9 >M $b98c $8d >M
T{ op PC S A X Y P -> $b98c $1c $6c $71 $1c $e0 }T T{ $6fce M $6fcf M $6fd0 M $b98c M -> $4c $8c $b9 $8d }T
$e3d8 >PC $1e >S $80 >A $93 >X $cd >Y $26 >P $a402 $65 >M $e3d8 $4c >M $e3d9 $02 >M $e3da $a4 >M
T{ op PC S A X Y P -> $a402 $1e $80 $93 $cd $26 }T T{ $a402 M $e3d8 M $e3d9 M $e3da M -> $65 $4c $02 $a4 }T
$1ea0 >PC $e5 >S $d3 >A $70 >X $4e >Y $61 >P $1ea0 $4c >M $1ea1 $af >M $1ea2 $d9 >M $d9af $db >M
T{ op PC S A X Y P -> $d9af $e5 $d3 $70 $4e $61 }T T{ $1ea0 M $1ea1 M $1ea2 M $d9af M -> $4c $af $d9 $db }T
$d617 >PC $5e >S $7e >A $51 >X $41 >Y $a7 >P $337e $d4 >M $d617 $4c >M $d618 $7e >M $d619 $33 >M
T{ op PC S A X Y P -> $337e $5e $7e $51 $41 $a7 }T T{ $337e M $d617 M $d618 M $d619 M -> $d4 $4c $7e $33 }T
( 4d )
$368d >PC $d3 >S $04 >A $5f >X $9c >Y $a3 >P $368d $4d >M $368e $b2 >M $368f $b5 >M $3690 $97 >M $b5b2 $e6 >M
T{ op PC S A X Y P -> $3690 $d3 $e2 $5f $9c $a1 }T T{ $368d M $368e M $368f M $3690 M $b5b2 M -> $4d $b2 $b5 $97 $e6 }T
$09ad >PC $91 >S $0e >A $04 >X $a0 >Y $24 >P $09ad $4d >M $09ae $fe >M $09af $68 >M $09b0 $6e >M $68fe $07 >M
T{ op PC S A X Y P -> $09b0 $91 $09 $04 $a0 $24 }T T{ $09ad M $09ae M $09af M $09b0 M $68fe M -> $4d $fe $68 $6e $07 }T
$2d99 >PC $08 >S $0f >A $b9 >X $1c >Y $e4 >P $2d99 $4d >M $2d9a $19 >M $2d9b $c2 >M $2d9c $9d >M $c219 $c2 >M
T{ op PC S A X Y P -> $2d9c $08 $cd $b9 $1c $e4 }T T{ $2d99 M $2d9a M $2d9b M $2d9c M $c219 M -> $4d $19 $c2 $9d $c2 }T
$65d3 >PC $17 >S $a9 >A $d8 >X $c8 >Y $64 >P $5f0f $0e >M $65d3 $4d >M $65d4 $0f >M $65d5 $5f >M $65d6 $bb >M
T{ op PC S A X Y P -> $65d6 $17 $a7 $d8 $c8 $e4 }T T{ $5f0f M $65d3 M $65d4 M $65d5 M $65d6 M -> $0e $4d $0f $5f $bb }T
$66c8 >PC $78 >S $5b >A $18 >X $ed >Y $a0 >P $66c8 $4d >M $66c9 $6f >M $66ca $ab >M $66cb $ab >M $ab6f $d5 >M
T{ op PC S A X Y P -> $66cb $78 $8e $18 $ed $a0 }T T{ $66c8 M $66c9 M $66ca M $66cb M $ab6f M -> $4d $6f $ab $ab $d5 }T
$f82c >PC $48 >S $ec >A $5b >X $89 >Y $26 >P $7576 $bd >M $f82c $4d >M $f82d $76 >M $f82e $75 >M $f82f $16 >M
T{ op PC S A X Y P -> $f82f $48 $51 $5b $89 $24 }T T{ $7576 M $f82c M $f82d M $f82e M $f82f M -> $bd $4d $76 $75 $16 }T
$790d >PC $98 >S $05 >A $12 >X $dd >Y $21 >P $5dfc $c4 >M $790d $4d >M $790e $fc >M $790f $5d >M $7910 $9f >M
T{ op PC S A X Y P -> $7910 $98 $c1 $12 $dd $a1 }T T{ $5dfc M $790d M $790e M $790f M $7910 M -> $c4 $4d $fc $5d $9f }T
$24ca >PC $42 >S $5d >A $fd >X $97 >Y $21 >P $103d $da >M $24ca $4d >M $24cb $3d >M $24cc $10 >M $24cd $b5 >M
T{ op PC S A X Y P -> $24cd $42 $87 $fd $97 $a1 }T T{ $103d M $24ca M $24cb M $24cc M $24cd M -> $da $4d $3d $10 $b5 }T
$31da >PC $fc >S $01 >A $35 >X $00 >Y $a6 >P $31da $4d >M $31db $ee >M $31dc $67 >M $31dd $60 >M $67ee $94 >M
T{ op PC S A X Y P -> $31dd $fc $95 $35 $00 $a4 }T T{ $31da M $31db M $31dc M $31dd M $67ee M -> $4d $ee $67 $60 $94 }T
$39b1 >PC $a8 >S $8d >A $d9 >X $6a >Y $e5 >P $39b1 $4d >M $39b2 $d9 >M $39b3 $ad >M $39b4 $4e >M $add9 $11 >M
T{ op PC S A X Y P -> $39b4 $a8 $9c $d9 $6a $e5 }T T{ $39b1 M $39b2 M $39b3 M $39b4 M $add9 M -> $4d $d9 $ad $4e $11 }T
$8bb7 >PC $67 >S $f3 >A $97 >X $df >Y $e6 >P $472c $b4 >M $8bb7 $4d >M $8bb8 $2c >M $8bb9 $47 >M $8bba $78 >M
T{ op PC S A X Y P -> $8bba $67 $47 $97 $df $64 }T T{ $472c M $8bb7 M $8bb8 M $8bb9 M $8bba M -> $b4 $4d $2c $47 $78 }T
$26bf >PC $3a >S $94 >A $e7 >X $6e >Y $e1 >P $26bf $4d >M $26c0 $fa >M $26c1 $ad >M $26c2 $55 >M $adfa $d9 >M
T{ op PC S A X Y P -> $26c2 $3a $4d $e7 $6e $61 }T T{ $26bf M $26c0 M $26c1 M $26c2 M $adfa M -> $4d $fa $ad $55 $d9 }T
$7375 >PC $a9 >S $95 >A $ea >X $45 >Y $e6 >P $2be9 $bf >M $7375 $4d >M $7376 $e9 >M $7377 $2b >M $7378 $96 >M
T{ op PC S A X Y P -> $7378 $a9 $2a $ea $45 $64 }T T{ $2be9 M $7375 M $7376 M $7377 M $7378 M -> $bf $4d $e9 $2b $96 }T
$7b7d >PC $d3 >S $c1 >A $29 >X $fb >Y $21 >P $7b7d $4d >M $7b7e $1e >M $7b7f $82 >M $7b80 $23 >M $821e $d6 >M
T{ op PC S A X Y P -> $7b80 $d3 $17 $29 $fb $21 }T T{ $7b7d M $7b7e M $7b7f M $7b80 M $821e M -> $4d $1e $82 $23 $d6 }T
$3d17 >PC $b9 >S $76 >A $08 >X $cc >Y $60 >P $3ac6 $30 >M $3d17 $4d >M $3d18 $c6 >M $3d19 $3a >M $3d1a $e5 >M
T{ op PC S A X Y P -> $3d1a $b9 $46 $08 $cc $60 }T T{ $3ac6 M $3d17 M $3d18 M $3d19 M $3d1a M -> $30 $4d $c6 $3a $e5 }T
$d0c8 >PC $85 >S $a4 >A $22 >X $4f >Y $e5 >P $c45a $b1 >M $d0c8 $4d >M $d0c9 $5a >M $d0ca $c4 >M $d0cb $23 >M
T{ op PC S A X Y P -> $d0cb $85 $15 $22 $4f $65 }T T{ $c45a M $d0c8 M $d0c9 M $d0ca M $d0cb M -> $b1 $4d $5a $c4 $23 }T
( 4e )
$ba2c >PC $e0 >S $03 >A $d6 >X $b5 >Y $60 >P $46bd $35 >M $ba2c $4e >M $ba2d $bd >M $ba2e $46 >M $ba2f $62 >M
T{ op PC S A X Y P -> $ba2f $e0 $03 $d6 $b5 $61 }T T{ $46bd M $ba2c M $ba2d M $ba2e M $ba2f M -> $1a $4e $bd $46 $62 }T
$95b7 >PC $80 >S $ae >A $be >X $c2 >Y $66 >P $85e7 $43 >M $95b7 $4e >M $95b8 $e7 >M $95b9 $85 >M $95ba $f1 >M
T{ op PC S A X Y P -> $95ba $80 $ae $be $c2 $65 }T T{ $85e7 M $95b7 M $95b8 M $95b9 M $95ba M -> $21 $4e $e7 $85 $f1 }T
$7574 >PC $13 >S $2f >A $09 >X $e1 >Y $60 >P $36f5 $2e >M $7574 $4e >M $7575 $f5 >M $7576 $36 >M $7577 $4c >M
T{ op PC S A X Y P -> $7577 $13 $2f $09 $e1 $60 }T T{ $36f5 M $7574 M $7575 M $7576 M $7577 M -> $17 $4e $f5 $36 $4c }T
$35f7 >PC $6a >S $2e >A $71 >X $36 >Y $e6 >P $35f7 $4e >M $35f8 $f2 >M $35f9 $e6 >M $35fa $38 >M $e6f2 $4f >M
T{ op PC S A X Y P -> $35fa $6a $2e $71 $36 $65 }T T{ $35f7 M $35f8 M $35f9 M $35fa M $e6f2 M -> $4e $f2 $e6 $38 $27 }T
$48b7 >PC $a7 >S $49 >A $da >X $8b >Y $a6 >P $390a $98 >M $48b7 $4e >M $48b8 $0a >M $48b9 $39 >M $48ba $6f >M
T{ op PC S A X Y P -> $48ba $a7 $49 $da $8b $24 }T T{ $390a M $48b7 M $48b8 M $48b9 M $48ba M -> $4c $4e $0a $39 $6f }T
$154d >PC $f1 >S $62 >A $75 >X $2b >Y $67 >P $154d $4e >M $154e $e6 >M $154f $e9 >M $1550 $73 >M $e9e6 $58 >M
T{ op PC S A X Y P -> $1550 $f1 $62 $75 $2b $64 }T T{ $154d M $154e M $154f M $1550 M $e9e6 M -> $4e $e6 $e9 $73 $2c }T
$6f47 >PC $51 >S $71 >A $8d >X $16 >Y $e6 >P $13fd $5d >M $6f47 $4e >M $6f48 $fd >M $6f49 $13 >M $6f4a $51 >M
T{ op PC S A X Y P -> $6f4a $51 $71 $8d $16 $65 }T T{ $13fd M $6f47 M $6f48 M $6f49 M $6f4a M -> $2e $4e $fd $13 $51 }T
$632c >PC $f3 >S $e7 >A $db >X $ef >Y $a7 >P $632c $4e >M $632d $17 >M $632e $90 >M $632f $6e >M $9017 $e2 >M
T{ op PC S A X Y P -> $632f $f3 $e7 $db $ef $24 }T T{ $632c M $632d M $632e M $632f M $9017 M -> $4e $17 $90 $6e $71 }T
$c8b2 >PC $3d >S $37 >A $e1 >X $80 >Y $e2 >P $c8b2 $4e >M $c8b3 $63 >M $c8b4 $e4 >M $c8b5 $6c >M $e463 $80 >M
T{ op PC S A X Y P -> $c8b5 $3d $37 $e1 $80 $60 }T T{ $c8b2 M $c8b3 M $c8b4 M $c8b5 M $e463 M -> $4e $63 $e4 $6c $40 }T
$fb58 >PC $15 >S $e5 >A $68 >X $98 >Y $a3 >P $b5d6 $86 >M $fb58 $4e >M $fb59 $d6 >M $fb5a $b5 >M $fb5b $fc >M
T{ op PC S A X Y P -> $fb5b $15 $e5 $68 $98 $20 }T T{ $b5d6 M $fb58 M $fb59 M $fb5a M $fb5b M -> $43 $4e $d6 $b5 $fc }T
$2d7b >PC $6e >S $9b >A $ad >X $ec >Y $61 >P $2d7b $4e >M $2d7c $72 >M $2d7d $c2 >M $2d7e $06 >M $c272 $f2 >M
T{ op PC S A X Y P -> $2d7e $6e $9b $ad $ec $60 }T T{ $2d7b M $2d7c M $2d7d M $2d7e M $c272 M -> $4e $72 $c2 $06 $79 }T
$594c >PC $6d >S $29 >A $c5 >X $19 >Y $22 >P $594c $4e >M $594d $cc >M $594e $e2 >M $594f $c0 >M $e2cc $62 >M
T{ op PC S A X Y P -> $594f $6d $29 $c5 $19 $20 }T T{ $594c M $594d M $594e M $594f M $e2cc M -> $4e $cc $e2 $c0 $31 }T
$df81 >PC $04 >S $c0 >A $5f >X $37 >Y $a7 >P $9b26 $5a >M $df81 $4e >M $df82 $26 >M $df83 $9b >M $df84 $99 >M
T{ op PC S A X Y P -> $df84 $04 $c0 $5f $37 $24 }T T{ $9b26 M $df81 M $df82 M $df83 M $df84 M -> $2d $4e $26 $9b $99 }T
$f6f6 >PC $56 >S $c2 >A $6e >X $1d >Y $a7 >P $382b $78 >M $f6f6 $4e >M $f6f7 $2b >M $f6f8 $38 >M $f6f9 $47 >M
T{ op PC S A X Y P -> $f6f9 $56 $c2 $6e $1d $24 }T T{ $382b M $f6f6 M $f6f7 M $f6f8 M $f6f9 M -> $3c $4e $2b $38 $47 }T
$7065 >PC $60 >S $0c >A $15 >X $88 >Y $67 >P $7065 $4e >M $7066 $89 >M $7067 $f0 >M $7068 $7a >M $f089 $53 >M
T{ op PC S A X Y P -> $7068 $60 $0c $15 $88 $65 }T T{ $7065 M $7066 M $7067 M $7068 M $f089 M -> $4e $89 $f0 $7a $29 }T
$2d6c >PC $81 >S $bd >A $f9 >X $27 >Y $e1 >P $14fd $16 >M $2d6c $4e >M $2d6d $fd >M $2d6e $14 >M $2d6f $1a >M
T{ op PC S A X Y P -> $2d6f $81 $bd $f9 $27 $60 }T T{ $14fd M $2d6c M $2d6d M $2d6e M $2d6f M -> $0b $4e $fd $14 $1a }T
( 4f )
$6eac >PC $68 >S $52 >A $76 >X $bd >Y $a4 >P $004b $94 >M $6e41 $7e >M $6eac $4f >M $6ead $4b >M $6eae $92 >M $6eaf $9a >M
T{ op PC S A X Y P -> $6eaf $68 $52 $76 $bd $a4 }T T{ $004b M $6e41 M $6eac M $6ead M $6eae M $6eaf M -> $94 $7e $4f $4b $92 $9a }T
$109f >PC $b4 >S $03 >A $a7 >X $a7 >Y $a6 >P $0044 $a0 >M $108b $1a >M $109f $4f >M $10a0 $44 >M $10a1 $e9 >M
T{ op PC S A X Y P -> $108b $b4 $03 $a7 $a7 $a6 }T T{ $0044 M $108b M $109f M $10a0 M $10a1 M -> $a0 $1a $4f $44 $e9 }T
$05e9 >PC $c0 >S $48 >A $f3 >X $e1 >Y $26 >P $0015 $75 >M $0527 $18 >M $05e9 $4f >M $05ea $15 >M $05eb $3b >M $05ec $7f >M
T{ op PC S A X Y P -> $05ec $c0 $48 $f3 $e1 $26 }T T{ $0015 M $0527 M $05e9 M $05ea M $05eb M $05ec M -> $75 $18 $4f $15 $3b $7f }T
$82db >PC $f0 >S $71 >A $08 >X $19 >Y $e3 >P $0001 $20 >M $821f $5d >M $82db $4f >M $82dc $01 >M $82dd $41 >M $831f $41 >M
T{ op PC S A X Y P -> $831f $f0 $71 $08 $19 $e3 }T T{ $0001 M $821f M $82db M $82dc M $82dd M $831f M -> $20 $5d $4f $01 $41 $41 }T
$437a >PC $94 >S $c1 >A $f4 >X $b3 >Y $26 >P $00cc $c3 >M $437a $4f >M $437b $cc >M $437c $7c >M $43f9 $13 >M
T{ op PC S A X Y P -> $43f9 $94 $c1 $f4 $b3 $26 }T T{ $00cc M $437a M $437b M $437c M $43f9 M -> $c3 $4f $cc $7c $13 }T
$f29b >PC $4f >S $ad >A $b1 >X $1d >Y $60 >P $00e6 $bc >M $f289 $67 >M $f29b $4f >M $f29c $e6 >M $f29d $eb >M $f29e $e7 >M
T{ op PC S A X Y P -> $f29e $4f $ad $b1 $1d $60 }T T{ $00e6 M $f289 M $f29b M $f29c M $f29d M $f29e M -> $bc $67 $4f $e6 $eb $e7 }T
$ea1d >PC $8c >S $94 >A $cc >X $39 >Y $25 >P $0025 $89 >M $e9ee $87 >M $ea1d $4f >M $ea1e $25 >M $ea1f $ce >M $eaee $84 >M
T{ op PC S A X Y P -> $e9ee $8c $94 $cc $39 $25 }T T{ $0025 M $e9ee M $ea1d M $ea1e M $ea1f M $eaee M -> $89 $87 $4f $25 $ce $84 }T
$154d >PC $bf >S $f2 >A $8e >X $38 >Y $a4 >P $00ad $7b >M $151e $8b >M $154d $4f >M $154e $ad >M $154f $ce >M $1550 $d8 >M
T{ op PC S A X Y P -> $1550 $bf $f2 $8e $38 $a4 }T T{ $00ad M $151e M $154d M $154e M $154f M $1550 M -> $7b $8b $4f $ad $ce $d8 }T
$0420 >PC $b1 >S $d2 >A $7d >X $3d >Y $20 >P $00b9 $c1 >M $0420 $4f >M $0421 $b9 >M $0422 $55 >M $0478 $29 >M
T{ op PC S A X Y P -> $0478 $b1 $d2 $7d $3d $20 }T T{ $00b9 M $0420 M $0421 M $0422 M $0478 M -> $c1 $4f $b9 $55 $29 }T
$ee0f >PC $d6 >S $4b >A $34 >X $c6 >Y $a0 >P $002e $88 >M $ee0f $4f >M $ee10 $2e >M $ee11 $55 >M $ee67 $40 >M
T{ op PC S A X Y P -> $ee67 $d6 $4b $34 $c6 $a0 }T T{ $002e M $ee0f M $ee10 M $ee11 M $ee67 M -> $88 $4f $2e $55 $40 }T
$948d >PC $7d >S $b0 >A $83 >X $4e >Y $62 >P $0007 $7e >M $9411 $ba >M $948d $4f >M $948e $07 >M $948f $81 >M $9490 $e5 >M
T{ op PC S A X Y P -> $9490 $7d $b0 $83 $4e $62 }T T{ $0007 M $9411 M $948d M $948e M $948f M $9490 M -> $7e $ba $4f $07 $81 $e5 }T
$a11c >PC $5c >S $d4 >A $08 >X $6d >Y $21 >P $00b9 $d9 >M $a11c $4f >M $a11d $b9 >M $a11e $ce >M $a11f $e2 >M $a1ed $cd >M
T{ op PC S A X Y P -> $a11f $5c $d4 $08 $6d $21 }T T{ $00b9 M $a11c M $a11d M $a11e M $a11f M $a1ed M -> $d9 $4f $b9 $ce $e2 $cd }T
$a1e5 >PC $e2 >S $14 >A $8b >X $c0 >Y $a0 >P $0076 $58 >M $a143 $e8 >M $a1e5 $4f >M $a1e6 $76 >M $a1e7 $5b >M $a1e8 $bf >M
T{ op PC S A X Y P -> $a1e8 $e2 $14 $8b $c0 $a0 }T T{ $0076 M $a143 M $a1e5 M $a1e6 M $a1e7 M $a1e8 M -> $58 $e8 $4f $76 $5b $bf }T
$58b5 >PC $99 >S $ca >A $6e >X $9b >Y $e3 >P $0088 $f0 >M $58a4 $f0 >M $58b5 $4f >M $58b6 $88 >M $58b7 $ec >M $58b8 $91 >M
T{ op PC S A X Y P -> $58b8 $99 $ca $6e $9b $e3 }T T{ $0088 M $58a4 M $58b5 M $58b6 M $58b7 M $58b8 M -> $f0 $f0 $4f $88 $ec $91 }T
$5e7d >PC $99 >S $80 >A $d0 >X $8c >Y $a7 >P $0047 $20 >M $5e7d $4f >M $5e7e $47 >M $5e7f $7b >M $5efb $92 >M
T{ op PC S A X Y P -> $5efb $99 $80 $d0 $8c $a7 }T T{ $0047 M $5e7d M $5e7e M $5e7f M $5efb M -> $20 $4f $47 $7b $92 }T
$c66a >PC $3b >S $7d >A $da >X $78 >Y $20 >P $0073 $1a >M $c63d $6c >M $c66a $4f >M $c66b $73 >M $c66c $d0 >M $c66d $e0 >M
T{ op PC S A X Y P -> $c66d $3b $7d $da $78 $20 }T T{ $0073 M $c63d M $c66a M $c66b M $c66c M $c66d M -> $1a $6c $4f $73 $d0 $e0 }T
( 50 )
$b05b >PC $30 >S $6d >A $49 >X $02 >Y $22 >P $b05b $50 >M $b05c $3f >M $b05d $0c >M $b09c $5d >M
T{ op PC S A X Y P -> $b09c $30 $6d $49 $02 $22 }T T{ $b05b M $b05c M $b05d M $b09c M -> $50 $3f $0c $5d }T
$dcc7 >PC $d1 >S $dc >A $9b >X $74 >Y $63 >P $dcc7 $50 >M $dcc8 $fa >M $dcc9 $70 >M
T{ op PC S A X Y P -> $dcc9 $d1 $dc $9b $74 $63 }T T{ $dcc7 M $dcc8 M $dcc9 M -> $50 $fa $70 }T
$948e >PC $f0 >S $67 >A $7c >X $a2 >Y $67 >P $948e $50 >M $948f $38 >M $9490 $7d >M
T{ op PC S A X Y P -> $9490 $f0 $67 $7c $a2 $67 }T T{ $948e M $948f M $9490 M -> $50 $38 $7d }T
$6cfe >PC $6f >S $dc >A $50 >X $43 >Y $25 >P $6cd0 $a3 >M $6cfe $50 >M $6cff $d0 >M $6d00 $20 >M $6dd0 $1c >M
T{ op PC S A X Y P -> $6cd0 $6f $dc $50 $43 $25 }T T{ $6cd0 M $6cfe M $6cff M $6d00 M $6dd0 M -> $a3 $50 $d0 $20 $1c }T
$e709 >PC $cc >S $1d >A $71 >X $35 >Y $24 >P $e6ef $ef >M $e709 $50 >M $e70a $e4 >M $e70b $c5 >M $e7ef $6f >M
T{ op PC S A X Y P -> $e6ef $cc $1d $71 $35 $24 }T T{ $e6ef M $e709 M $e70a M $e70b M $e7ef M -> $ef $50 $e4 $c5 $6f }T
$12ef >PC $84 >S $72 >A $30 >X $85 >Y $65 >P $12ef $50 >M $12f0 $c3 >M $12f1 $d2 >M
T{ op PC S A X Y P -> $12f1 $84 $72 $30 $85 $65 }T T{ $12ef M $12f0 M $12f1 M -> $50 $c3 $d2 }T
$f34e >PC $6b >S $56 >A $8e >X $a6 >Y $26 >P $f34e $50 >M $f34f $33 >M $f350 $85 >M $f383 $1f >M
T{ op PC S A X Y P -> $f383 $6b $56 $8e $a6 $26 }T T{ $f34e M $f34f M $f350 M $f383 M -> $50 $33 $85 $1f }T
$1b03 >PC $1d >S $c4 >A $c9 >X $cc >Y $e0 >P $1b03 $50 >M $1b04 $4f >M $1b05 $1f >M
T{ op PC S A X Y P -> $1b05 $1d $c4 $c9 $cc $e0 }T T{ $1b03 M $1b04 M $1b05 M -> $50 $4f $1f }T
$106f >PC $d9 >S $09 >A $dc >X $10 >Y $a3 >P $100d $60 >M $106f $50 >M $1070 $9c >M $1071 $18 >M
T{ op PC S A X Y P -> $100d $d9 $09 $dc $10 $a3 }T T{ $100d M $106f M $1070 M $1071 M -> $60 $50 $9c $18 }T
$5f64 >PC $a0 >S $f8 >A $ee >X $99 >Y $e5 >P $5f64 $50 >M $5f65 $33 >M $5f66 $9f >M
T{ op PC S A X Y P -> $5f66 $a0 $f8 $ee $99 $e5 }T T{ $5f64 M $5f65 M $5f66 M -> $50 $33 $9f }T
$0cff >PC $34 >S $5c >A $4f >X $48 >Y $e6 >P $0cff $50 >M $0d00 $de >M $0d01 $64 >M
T{ op PC S A X Y P -> $0d01 $34 $5c $4f $48 $e6 }T T{ $0cff M $0d00 M $0d01 M -> $50 $de $64 }T
$e0d3 >PC $b8 >S $a6 >A $c1 >X $c4 >Y $62 >P $e0d3 $50 >M $e0d4 $94 >M $e0d5 $ea >M
T{ op PC S A X Y P -> $e0d5 $b8 $a6 $c1 $c4 $62 }T T{ $e0d3 M $e0d4 M $e0d5 M -> $50 $94 $ea }T
$67ba >PC $0f >S $c3 >A $7b >X $a9 >Y $e2 >P $67ba $50 >M $67bb $79 >M $67bc $e3 >M
T{ op PC S A X Y P -> $67bc $0f $c3 $7b $a9 $e2 }T T{ $67ba M $67bb M $67bc M -> $50 $79 $e3 }T
$7504 >PC $d0 >S $c5 >A $b9 >X $58 >Y $e2 >P $7504 $50 >M $7505 $1e >M $7506 $d1 >M
T{ op PC S A X Y P -> $7506 $d0 $c5 $b9 $58 $e2 }T T{ $7504 M $7505 M $7506 M -> $50 $1e $d1 }T
$00a5 >PC $e2 >S $55 >A $47 >X $de >Y $60 >P $00a5 $50 >M $00a6 $70 >M $00a7 $9d >M
T{ op PC S A X Y P -> $00a7 $e2 $55 $47 $de $60 }T T{ $00a5 M $00a6 M $00a7 M -> $50 $70 $9d }T
$09b1 >PC $71 >S $3d >A $b6 >X $7b >Y $a3 >P $09b1 $50 >M $09b2 $2c >M $09b3 $26 >M $09df $d8 >M
T{ op PC S A X Y P -> $09df $71 $3d $b6 $7b $a3 }T T{ $09b1 M $09b2 M $09b3 M $09df M -> $50 $2c $26 $d8 }T
( 51 )
$4c39 >PC $23 >S $d3 >A $24 >X $8e >Y $a6 >P $0037 $aa >M $0038 $11 >M $1238 $44 >M $4c39 $51 >M $4c3a $37 >M $4c3b $8c >M
T{ op PC S A X Y P -> $4c3b $23 $97 $24 $8e $a4 }T T{ $0037 M $0038 M $1238 M $4c39 M $4c3a M $4c3b M -> $aa $11 $44 $51 $37 $8c }T
$6729 >PC $bc >S $09 >A $71 >X $25 >Y $e3 >P $005f $ac >M $0060 $99 >M $6729 $51 >M $672a $5f >M $672b $c1 >M $99d1 $3b >M
T{ op PC S A X Y P -> $672b $bc $32 $71 $25 $61 }T T{ $005f M $0060 M $6729 M $672a M $672b M $99d1 M -> $ac $99 $51 $5f $c1 $3b }T
$3249 >PC $dd >S $30 >A $b4 >X $49 >Y $e7 >P $001c $27 >M $001d $cc >M $3249 $51 >M $324a $1c >M $324b $c5 >M $cc70 $d2 >M
T{ op PC S A X Y P -> $324b $dd $e2 $b4 $49 $e5 }T T{ $001c M $001d M $3249 M $324a M $324b M $cc70 M -> $27 $cc $51 $1c $c5 $d2 }T
$54c2 >PC $92 >S $68 >A $f7 >X $eb >Y $23 >P $0060 $92 >M $0061 $89 >M $54c2 $51 >M $54c3 $60 >M $54c4 $c1 >M $8a7d $99 >M
T{ op PC S A X Y P -> $54c4 $92 $f1 $f7 $eb $a1 }T T{ $0060 M $0061 M $54c2 M $54c3 M $54c4 M $8a7d M -> $92 $89 $51 $60 $c1 $99 }T
$a8fd >PC $2f >S $42 >A $68 >X $aa >Y $26 >P $001c $a6 >M $001d $b0 >M $a8fd $51 >M $a8fe $1c >M $a8ff $23 >M $b150 $fb >M
T{ op PC S A X Y P -> $a8ff $2f $b9 $68 $aa $a4 }T T{ $001c M $001d M $a8fd M $a8fe M $a8ff M $b150 M -> $a6 $b0 $51 $1c $23 $fb }T
$a7b8 >PC $47 >S $77 >A $7e >X $f1 >Y $63 >P $005d $53 >M $005e $01 >M $0244 $8a >M $a7b8 $51 >M $a7b9 $5d >M $a7ba $0f >M
T{ op PC S A X Y P -> $a7ba $47 $fd $7e $f1 $e1 }T T{ $005d M $005e M $0244 M $a7b8 M $a7b9 M $a7ba M -> $53 $01 $8a $51 $5d $0f }T
$777e >PC $44 >S $bf >A $6c >X $d2 >Y $21 >P $00b1 $27 >M $00b2 $a9 >M $777e $51 >M $777f $b1 >M $7780 $11 >M $a9f9 $e4 >M
T{ op PC S A X Y P -> $7780 $44 $5b $6c $d2 $21 }T T{ $00b1 M $00b2 M $777e M $777f M $7780 M $a9f9 M -> $27 $a9 $51 $b1 $11 $e4 }T
$ef6e >PC $af >S $9b >A $2a >X $d3 >Y $e5 >P $0023 $da >M $0024 $9f >M $a0ad $04 >M $ef6e $51 >M $ef6f $23 >M $ef70 $1a >M
T{ op PC S A X Y P -> $ef70 $af $9f $2a $d3 $e5 }T T{ $0023 M $0024 M $a0ad M $ef6e M $ef6f M $ef70 M -> $da $9f $04 $51 $23 $1a }T
$336c >PC $07 >S $b4 >A $73 >X $df >Y $27 >P $004f $47 >M $0050 $38 >M $336c $51 >M $336d $4f >M $336e $61 >M $3926 $77 >M
T{ op PC S A X Y P -> $336e $07 $c3 $73 $df $a5 }T T{ $004f M $0050 M $336c M $336d M $336e M $3926 M -> $47 $38 $51 $4f $61 $77 }T
$5033 >PC $78 >S $52 >A $4f >X $2f >Y $a1 >P $0049 $78 >M $004a $cd >M $5033 $51 >M $5034 $49 >M $5035 $a7 >M $cda7 $e9 >M
T{ op PC S A X Y P -> $5035 $78 $bb $4f $2f $a1 }T T{ $0049 M $004a M $5033 M $5034 M $5035 M $cda7 M -> $78 $cd $51 $49 $a7 $e9 }T
$bbd1 >PC $e2 >S $63 >A $1d >X $ff >Y $e3 >P $00cf $f1 >M $00d0 $75 >M $76f0 $45 >M $bbd1 $51 >M $bbd2 $cf >M $bbd3 $cf >M
T{ op PC S A X Y P -> $bbd3 $e2 $26 $1d $ff $61 }T T{ $00cf M $00d0 M $76f0 M $bbd1 M $bbd2 M $bbd3 M -> $f1 $75 $45 $51 $cf $cf }T
$947d >PC $c9 >S $4f >A $3b >X $45 >Y $25 >P $005e $57 >M $005f $fb >M $947d $51 >M $947e $5e >M $947f $92 >M $fb9c $da >M
T{ op PC S A X Y P -> $947f $c9 $95 $3b $45 $a5 }T T{ $005e M $005f M $947d M $947e M $947f M $fb9c M -> $57 $fb $51 $5e $92 $da }T
$6216 >PC $af >S $4d >A $12 >X $5e >Y $a5 >P $0015 $9d >M $0016 $df >M $6216 $51 >M $6217 $15 >M $6218 $a9 >M $dffb $89 >M
T{ op PC S A X Y P -> $6218 $af $c4 $12 $5e $a5 }T T{ $0015 M $0016 M $6216 M $6217 M $6218 M $dffb M -> $9d $df $51 $15 $a9 $89 }T
$00ed >PC $86 >S $c6 >A $2e >X $09 >Y $e7 >P $0018 $7a >M $0019 $29 >M $00ed $51 >M $00ee $18 >M $00ef $f5 >M $2983 $31 >M
T{ op PC S A X Y P -> $00ef $86 $f7 $2e $09 $e5 }T T{ $0018 M $0019 M $00ed M $00ee M $00ef M $2983 M -> $7a $29 $51 $18 $f5 $31 }T
$a905 >PC $9f >S $c0 >A $d1 >X $d5 >Y $25 >P $0008 $c6 >M $0009 $e9 >M $a905 $51 >M $a906 $08 >M $a907 $ea >M $ea9b $00 >M
T{ op PC S A X Y P -> $a907 $9f $c0 $d1 $d5 $a5 }T T{ $0008 M $0009 M $a905 M $a906 M $a907 M $ea9b M -> $c6 $e9 $51 $08 $ea $00 }T
$424d >PC $17 >S $a6 >A $f1 >X $4e >Y $26 >P $0013 $be >M $0014 $b7 >M $424d $51 >M $424e $13 >M $424f $01 >M $b80c $cb >M
T{ op PC S A X Y P -> $424f $17 $6d $f1 $4e $24 }T T{ $0013 M $0014 M $424d M $424e M $424f M $b80c M -> $be $b7 $51 $13 $01 $cb }T
( 52 )
$256e >PC $31 >S $17 >A $14 >X $65 >Y $e5 >P $00f6 $b2 >M $00f7 $4b >M $256e $52 >M $256f $f6 >M $2570 $6a >M $4bb2 $3c >M
T{ op PC S A X Y P -> $2570 $31 $2b $14 $65 $65 }T T{ $00f6 M $00f7 M $256e M $256f M $2570 M $4bb2 M -> $b2 $4b $52 $f6 $6a $3c }T
$93ed >PC $18 >S $b1 >A $fa >X $30 >Y $26 >P $0024 $91 >M $0025 $3d >M $3d91 $cc >M $93ed $52 >M $93ee $24 >M $93ef $ac >M
T{ op PC S A X Y P -> $93ef $18 $7d $fa $30 $24 }T T{ $0024 M $0025 M $3d91 M $93ed M $93ee M $93ef M -> $91 $3d $cc $52 $24 $ac }T
$3daf >PC $ed >S $e1 >A $f4 >X $eb >Y $a7 >P $0040 $f3 >M $0041 $cd >M $3daf $52 >M $3db0 $40 >M $3db1 $8c >M $cdf3 $05 >M
T{ op PC S A X Y P -> $3db1 $ed $e4 $f4 $eb $a5 }T T{ $0040 M $0041 M $3daf M $3db0 M $3db1 M $cdf3 M -> $f3 $cd $52 $40 $8c $05 }T
$7950 >PC $12 >S $45 >A $16 >X $49 >Y $23 >P $0091 $33 >M $0092 $9c >M $7950 $52 >M $7951 $91 >M $7952 $c9 >M $9c33 $7f >M
T{ op PC S A X Y P -> $7952 $12 $3a $16 $49 $21 }T T{ $0091 M $0092 M $7950 M $7951 M $7952 M $9c33 M -> $33 $9c $52 $91 $c9 $7f }T
$736e >PC $94 >S $c9 >A $2b >X $f5 >Y $64 >P $0040 $b7 >M $0041 $ea >M $736e $52 >M $736f $40 >M $7370 $cc >M $eab7 $74 >M
T{ op PC S A X Y P -> $7370 $94 $bd $2b $f5 $e4 }T T{ $0040 M $0041 M $736e M $736f M $7370 M $eab7 M -> $b7 $ea $52 $40 $cc $74 }T
$0bea >PC $ab >S $d1 >A $e8 >X $80 >Y $64 >P $00f2 $65 >M $00f3 $b2 >M $0bea $52 >M $0beb $f2 >M $0bec $a7 >M $b265 $82 >M
T{ op PC S A X Y P -> $0bec $ab $53 $e8 $80 $64 }T T{ $00f2 M $00f3 M $0bea M $0beb M $0bec M $b265 M -> $65 $b2 $52 $f2 $a7 $82 }T
$320a >PC $c0 >S $32 >A $62 >X $ec >Y $65 >P $0001 $05 >M $0002 $5e >M $320a $52 >M $320b $01 >M $320c $3c >M $5e05 $3d >M
T{ op PC S A X Y P -> $320c $c0 $0f $62 $ec $65 }T T{ $0001 M $0002 M $320a M $320b M $320c M $5e05 M -> $05 $5e $52 $01 $3c $3d }T
$1e60 >PC $94 >S $5a >A $50 >X $02 >Y $e7 >P $0017 $b7 >M $0018 $aa >M $1e60 $52 >M $1e61 $17 >M $1e62 $ef >M $aab7 $52 >M
T{ op PC S A X Y P -> $1e62 $94 $08 $50 $02 $65 }T T{ $0017 M $0018 M $1e60 M $1e61 M $1e62 M $aab7 M -> $b7 $aa $52 $17 $ef $52 }T
$1322 >PC $f5 >S $51 >A $fb >X $c6 >Y $e3 >P $009d $fc >M $009e $9d >M $1322 $52 >M $1323 $9d >M $1324 $37 >M $9dfc $12 >M
T{ op PC S A X Y P -> $1324 $f5 $43 $fb $c6 $61 }T T{ $009d M $009e M $1322 M $1323 M $1324 M $9dfc M -> $fc $9d $52 $9d $37 $12 }T
$0be8 >PC $b3 >S $ff >A $af >X $34 >Y $64 >P $0027 $e4 >M $0028 $a7 >M $0be8 $52 >M $0be9 $27 >M $0bea $4f >M $a7e4 $91 >M
T{ op PC S A X Y P -> $0bea $b3 $6e $af $34 $64 }T T{ $0027 M $0028 M $0be8 M $0be9 M $0bea M $a7e4 M -> $e4 $a7 $52 $27 $4f $91 }T
$48db >PC $9e >S $44 >A $0e >X $af >Y $64 >P $0070 $ef >M $0071 $89 >M $48db $52 >M $48dc $70 >M $48dd $99 >M $89ef $c0 >M
T{ op PC S A X Y P -> $48dd $9e $84 $0e $af $e4 }T T{ $0070 M $0071 M $48db M $48dc M $48dd M $89ef M -> $ef $89 $52 $70 $99 $c0 }T
$dd65 >PC $7d >S $94 >A $9a >X $d0 >Y $a7 >P $0099 $17 >M $009a $e9 >M $dd65 $52 >M $dd66 $99 >M $dd67 $bd >M $e917 $66 >M
T{ op PC S A X Y P -> $dd67 $7d $f2 $9a $d0 $a5 }T T{ $0099 M $009a M $dd65 M $dd66 M $dd67 M $e917 M -> $17 $e9 $52 $99 $bd $66 }T
$f28f >PC $d5 >S $24 >A $58 >X $af >Y $a3 >P $00f0 $72 >M $00f1 $fc >M $f28f $52 >M $f290 $f0 >M $f291 $d8 >M $fc72 $5e >M
T{ op PC S A X Y P -> $f291 $d5 $7a $58 $af $21 }T T{ $00f0 M $00f1 M $f28f M $f290 M $f291 M $fc72 M -> $72 $fc $52 $f0 $d8 $5e }T
$6d1b >PC $28 >S $1e >A $0b >X $a5 >Y $23 >P $002f $ea >M $0030 $ed >M $6d1b $52 >M $6d1c $2f >M $6d1d $a8 >M $edea $37 >M
T{ op PC S A X Y P -> $6d1d $28 $29 $0b $a5 $21 }T T{ $002f M $0030 M $6d1b M $6d1c M $6d1d M $edea M -> $ea $ed $52 $2f $a8 $37 }T
$821c >PC $cb >S $b5 >A $a7 >X $7a >Y $25 >P $001a $cf >M $001b $ec >M $821c $52 >M $821d $1a >M $821e $56 >M $eccf $fc >M
T{ op PC S A X Y P -> $821e $cb $49 $a7 $7a $25 }T T{ $001a M $001b M $821c M $821d M $821e M $eccf M -> $cf $ec $52 $1a $56 $fc }T
$b815 >PC $8d >S $f6 >A $60 >X $8f >Y $a6 >P $00af $64 >M $00b0 $4a >M $4a64 $b4 >M $b815 $52 >M $b816 $af >M $b817 $86 >M
T{ op PC S A X Y P -> $b817 $8d $42 $60 $8f $24 }T T{ $00af M $00b0 M $4a64 M $b815 M $b816 M $b817 M -> $64 $4a $b4 $52 $af $86 }T
( 53 )
$17b5 >PC $41 >S $5f >A $94 >X $d8 >Y $e2 >P $17b5 $53 >M $17b6 $b7 >M $17b7 $47 >M
T{ op PC S A X Y P -> $17b6 $41 $5f $94 $d8 $e2 }T T{ $17b5 M $17b6 M $17b7 M -> $53 $b7 $47 }T
$fcb6 >PC $61 >S $48 >A $58 >X $f7 >Y $a1 >P $fcb6 $53 >M $fcb7 $2b >M $fcb8 $54 >M
T{ op PC S A X Y P -> $fcb7 $61 $48 $58 $f7 $a1 }T T{ $fcb6 M $fcb7 M $fcb8 M -> $53 $2b $54 }T
$3830 >PC $6d >S $2c >A $cc >X $7d >Y $a6 >P $3830 $53 >M $3831 $6e >M $3832 $ad >M
T{ op PC S A X Y P -> $3831 $6d $2c $cc $7d $a6 }T T{ $3830 M $3831 M $3832 M -> $53 $6e $ad }T
$b5f3 >PC $ab >S $83 >A $36 >X $02 >Y $e7 >P $b5f3 $53 >M $b5f4 $e1 >M $b5f5 $e3 >M
T{ op PC S A X Y P -> $b5f4 $ab $83 $36 $02 $e7 }T T{ $b5f3 M $b5f4 M $b5f5 M -> $53 $e1 $e3 }T
$fb97 >PC $92 >S $a3 >A $1f >X $10 >Y $a2 >P $fb97 $53 >M $fb98 $5f >M $fb99 $a3 >M
T{ op PC S A X Y P -> $fb98 $92 $a3 $1f $10 $a2 }T T{ $fb97 M $fb98 M $fb99 M -> $53 $5f $a3 }T
$1252 >PC $21 >S $30 >A $c7 >X $62 >Y $64 >P $1252 $53 >M $1253 $ba >M $1254 $44 >M
T{ op PC S A X Y P -> $1253 $21 $30 $c7 $62 $64 }T T{ $1252 M $1253 M $1254 M -> $53 $ba $44 }T
$83a9 >PC $65 >S $52 >A $59 >X $c4 >Y $e3 >P $83a9 $53 >M $83aa $67 >M $83ab $0f >M
T{ op PC S A X Y P -> $83aa $65 $52 $59 $c4 $e3 }T T{ $83a9 M $83aa M $83ab M -> $53 $67 $0f }T
$db8e >PC $82 >S $10 >A $b5 >X $2c >Y $24 >P $db8e $53 >M $db8f $3f >M $db90 $cd >M
T{ op PC S A X Y P -> $db8f $82 $10 $b5 $2c $24 }T T{ $db8e M $db8f M $db90 M -> $53 $3f $cd }T
$bfb0 >PC $15 >S $a5 >A $9e >X $43 >Y $26 >P $bfb0 $53 >M $bfb1 $d6 >M $bfb2 $48 >M
T{ op PC S A X Y P -> $bfb1 $15 $a5 $9e $43 $26 }T T{ $bfb0 M $bfb1 M $bfb2 M -> $53 $d6 $48 }T
$3e35 >PC $e3 >S $fe >A $ae >X $d4 >Y $26 >P $3e35 $53 >M $3e36 $1d >M $3e37 $ee >M
T{ op PC S A X Y P -> $3e36 $e3 $fe $ae $d4 $26 }T T{ $3e35 M $3e36 M $3e37 M -> $53 $1d $ee }T
$3de9 >PC $13 >S $8e >A $a9 >X $df >Y $a5 >P $3de9 $53 >M $3dea $09 >M $3deb $18 >M
T{ op PC S A X Y P -> $3dea $13 $8e $a9 $df $a5 }T T{ $3de9 M $3dea M $3deb M -> $53 $09 $18 }T
$7bb4 >PC $45 >S $59 >A $5b >X $bc >Y $e6 >P $7bb4 $53 >M $7bb5 $ef >M $7bb6 $dd >M
T{ op PC S A X Y P -> $7bb5 $45 $59 $5b $bc $e6 }T T{ $7bb4 M $7bb5 M $7bb6 M -> $53 $ef $dd }T
$2316 >PC $46 >S $cd >A $20 >X $fe >Y $23 >P $2316 $53 >M $2317 $4b >M $2318 $ef >M
T{ op PC S A X Y P -> $2317 $46 $cd $20 $fe $23 }T T{ $2316 M $2317 M $2318 M -> $53 $4b $ef }T
$476e >PC $bd >S $47 >A $0f >X $46 >Y $e7 >P $476e $53 >M $476f $5b >M $4770 $c8 >M
T{ op PC S A X Y P -> $476f $bd $47 $0f $46 $e7 }T T{ $476e M $476f M $4770 M -> $53 $5b $c8 }T
$6c04 >PC $58 >S $9f >A $52 >X $ed >Y $a5 >P $6c04 $53 >M $6c05 $8a >M $6c06 $59 >M
T{ op PC S A X Y P -> $6c05 $58 $9f $52 $ed $a5 }T T{ $6c04 M $6c05 M $6c06 M -> $53 $8a $59 }T
$9128 >PC $cf >S $8c >A $40 >X $a9 >Y $e4 >P $9128 $53 >M $9129 $10 >M $912a $1c >M
T{ op PC S A X Y P -> $9129 $cf $8c $40 $a9 $e4 }T T{ $9128 M $9129 M $912a M -> $53 $10 $1c }T
( 54 )
$2811 >PC $bf >S $b2 >A $9a >X $93 >Y $67 >P $0025 $2f >M $008b $e0 >M $2811 $54 >M $2812 $8b >M $2813 $37 >M
T{ op PC S A X Y P -> $2813 $bf $b2 $9a $93 $67 }T T{ $0025 M $008b M $2811 M $2812 M $2813 M -> $2f $e0 $54 $8b $37 }T
$087b >PC $71 >S $c5 >A $75 >X $5b >Y $65 >P $0017 $58 >M $00a2 $de >M $087b $54 >M $087c $a2 >M $087d $5f >M
T{ op PC S A X Y P -> $087d $71 $c5 $75 $5b $65 }T T{ $0017 M $00a2 M $087b M $087c M $087d M -> $58 $de $54 $a2 $5f }T
$1da5 >PC $54 >S $5a >A $d0 >X $8b >Y $27 >P $0059 $73 >M $0089 $70 >M $1da5 $54 >M $1da6 $89 >M $1da7 $e1 >M
T{ op PC S A X Y P -> $1da7 $54 $5a $d0 $8b $27 }T T{ $0059 M $0089 M $1da5 M $1da6 M $1da7 M -> $73 $70 $54 $89 $e1 }T
$5bcf >PC $52 >S $ad >A $9f >X $7e >Y $a4 >P $0020 $ff >M $00bf $8b >M $5bcf $54 >M $5bd0 $20 >M $5bd1 $8a >M
T{ op PC S A X Y P -> $5bd1 $52 $ad $9f $7e $a4 }T T{ $0020 M $00bf M $5bcf M $5bd0 M $5bd1 M -> $ff $8b $54 $20 $8a }T
$fd00 >PC $3d >S $47 >A $d5 >X $9d >Y $65 >P $0071 $72 >M $009c $09 >M $fd00 $54 >M $fd01 $9c >M $fd02 $80 >M
T{ op PC S A X Y P -> $fd02 $3d $47 $d5 $9d $65 }T T{ $0071 M $009c M $fd00 M $fd01 M $fd02 M -> $72 $09 $54 $9c $80 }T
$19a1 >PC $73 >S $20 >A $a3 >X $fc >Y $a3 >P $0052 $5d >M $00af $97 >M $19a1 $54 >M $19a2 $af >M $19a3 $a8 >M
T{ op PC S A X Y P -> $19a3 $73 $20 $a3 $fc $a3 }T T{ $0052 M $00af M $19a1 M $19a2 M $19a3 M -> $5d $97 $54 $af $a8 }T
$5b86 >PC $87 >S $be >A $fe >X $33 >Y $20 >P $00a8 $b2 >M $00aa $9f >M $5b86 $54 >M $5b87 $aa >M $5b88 $7c >M
T{ op PC S A X Y P -> $5b88 $87 $be $fe $33 $20 }T T{ $00a8 M $00aa M $5b86 M $5b87 M $5b88 M -> $b2 $9f $54 $aa $7c }T
$b477 >PC $3a >S $e3 >A $59 >X $3e >Y $25 >P $001c $67 >M $0075 $a4 >M $b477 $54 >M $b478 $1c >M $b479 $06 >M
T{ op PC S A X Y P -> $b479 $3a $e3 $59 $3e $25 }T T{ $001c M $0075 M $b477 M $b478 M $b479 M -> $67 $a4 $54 $1c $06 }T
$9917 >PC $ce >S $78 >A $f9 >X $0d >Y $e3 >P $0040 $bf >M $0047 $86 >M $9917 $54 >M $9918 $47 >M $9919 $19 >M
T{ op PC S A X Y P -> $9919 $ce $78 $f9 $0d $e3 }T T{ $0040 M $0047 M $9917 M $9918 M $9919 M -> $bf $86 $54 $47 $19 }T
$b66b >PC $76 >S $f2 >A $40 >X $a0 >Y $a6 >P $0022 $b8 >M $0062 $40 >M $b66b $54 >M $b66c $22 >M $b66d $cd >M
T{ op PC S A X Y P -> $b66d $76 $f2 $40 $a0 $a6 }T T{ $0022 M $0062 M $b66b M $b66c M $b66d M -> $b8 $40 $54 $22 $cd }T
$b283 >PC $73 >S $6a >A $39 >X $e1 >Y $e4 >P $005a $31 >M $0093 $6a >M $b283 $54 >M $b284 $5a >M $b285 $fa >M
T{ op PC S A X Y P -> $b285 $73 $6a $39 $e1 $e4 }T T{ $005a M $0093 M $b283 M $b284 M $b285 M -> $31 $6a $54 $5a $fa }T
$10b1 >PC $6d >S $53 >A $b2 >X $3c >Y $e1 >P $0069 $a8 >M $00b7 $34 >M $10b1 $54 >M $10b2 $b7 >M $10b3 $f8 >M
T{ op PC S A X Y P -> $10b3 $6d $53 $b2 $3c $e1 }T T{ $0069 M $00b7 M $10b1 M $10b2 M $10b3 M -> $a8 $34 $54 $b7 $f8 }T
$8c0b >PC $91 >S $88 >A $24 >X $c1 >Y $a4 >P $005c $e9 >M $0080 $39 >M $8c0b $54 >M $8c0c $5c >M $8c0d $fa >M
T{ op PC S A X Y P -> $8c0d $91 $88 $24 $c1 $a4 }T T{ $005c M $0080 M $8c0b M $8c0c M $8c0d M -> $e9 $39 $54 $5c $fa }T
$ec7c >PC $0c >S $57 >A $2d >X $05 >Y $66 >P $0010 $09 >M $00e3 $11 >M $ec7c $54 >M $ec7d $e3 >M $ec7e $9d >M
T{ op PC S A X Y P -> $ec7e $0c $57 $2d $05 $66 }T T{ $0010 M $00e3 M $ec7c M $ec7d M $ec7e M -> $09 $11 $54 $e3 $9d }T
$ab94 >PC $bd >S $44 >A $9f >X $f5 >Y $62 >P $0077 $9e >M $00d8 $64 >M $ab94 $54 >M $ab95 $d8 >M $ab96 $55 >M
T{ op PC S A X Y P -> $ab96 $bd $44 $9f $f5 $62 }T T{ $0077 M $00d8 M $ab94 M $ab95 M $ab96 M -> $9e $64 $54 $d8 $55 }T
$eeb4 >PC $3f >S $0f >A $ac >X $28 >Y $25 >P $0074 $23 >M $00c8 $af >M $eeb4 $54 >M $eeb5 $c8 >M $eeb6 $2e >M
T{ op PC S A X Y P -> $eeb6 $3f $0f $ac $28 $25 }T T{ $0074 M $00c8 M $eeb4 M $eeb5 M $eeb6 M -> $23 $af $54 $c8 $2e }T
( 55 )
$2dd5 >PC $07 >S $3e >A $e6 >X $29 >Y $25 >P $000b $c2 >M $0025 $ab >M $2dd5 $55 >M $2dd6 $25 >M $2dd7 $a4 >M
T{ op PC S A X Y P -> $2dd7 $07 $fc $e6 $29 $a5 }T T{ $000b M $0025 M $2dd5 M $2dd6 M $2dd7 M -> $c2 $ab $55 $25 $a4 }T
$e2a4 >PC $04 >S $7b >A $e9 >X $f4 >Y $a1 >P $0043 $02 >M $005a $65 >M $e2a4 $55 >M $e2a5 $5a >M $e2a6 $fe >M
T{ op PC S A X Y P -> $e2a6 $04 $79 $e9 $f4 $21 }T T{ $0043 M $005a M $e2a4 M $e2a5 M $e2a6 M -> $02 $65 $55 $5a $fe }T
$1263 >PC $bd >S $03 >A $74 >X $32 >Y $e1 >P $0064 $7e >M $00f0 $57 >M $1263 $55 >M $1264 $f0 >M $1265 $09 >M
T{ op PC S A X Y P -> $1265 $bd $7d $74 $32 $61 }T T{ $0064 M $00f0 M $1263 M $1264 M $1265 M -> $7e $57 $55 $f0 $09 }T
$a080 >PC $c1 >S $76 >A $7a >X $2d >Y $a6 >P $0026 $bd >M $00ac $4d >M $a080 $55 >M $a081 $ac >M $a082 $93 >M
T{ op PC S A X Y P -> $a082 $c1 $cb $7a $2d $a4 }T T{ $0026 M $00ac M $a080 M $a081 M $a082 M -> $bd $4d $55 $ac $93 }T
$97a9 >PC $b3 >S $17 >A $9e >X $45 >Y $a3 >P $0056 $68 >M $00f4 $cc >M $97a9 $55 >M $97aa $56 >M $97ab $55 >M
T{ op PC S A X Y P -> $97ab $b3 $db $9e $45 $a1 }T T{ $0056 M $00f4 M $97a9 M $97aa M $97ab M -> $68 $cc $55 $56 $55 }T
$1c1c >PC $9f >S $8e >A $9d >X $a3 >Y $a1 >P $0011 $c5 >M $0074 $bd >M $1c1c $55 >M $1c1d $74 >M $1c1e $47 >M
T{ op PC S A X Y P -> $1c1e $9f $4b $9d $a3 $21 }T T{ $0011 M $0074 M $1c1c M $1c1d M $1c1e M -> $c5 $bd $55 $74 $47 }T
$25a8 >PC $e1 >S $a8 >A $17 >X $32 >Y $20 >P $0006 $6d >M $001d $70 >M $25a8 $55 >M $25a9 $06 >M $25aa $a7 >M
T{ op PC S A X Y P -> $25aa $e1 $d8 $17 $32 $a0 }T T{ $0006 M $001d M $25a8 M $25a9 M $25aa M -> $6d $70 $55 $06 $a7 }T
$5e14 >PC $a7 >S $20 >A $a9 >X $a6 >Y $e4 >P $0047 $e2 >M $00f0 $77 >M $5e14 $55 >M $5e15 $47 >M $5e16 $48 >M
T{ op PC S A X Y P -> $5e16 $a7 $57 $a9 $a6 $64 }T T{ $0047 M $00f0 M $5e14 M $5e15 M $5e16 M -> $e2 $77 $55 $47 $48 }T
$8c37 >PC $e6 >S $d7 >A $61 >X $88 >Y $26 >P $0018 $69 >M $00b7 $e0 >M $8c37 $55 >M $8c38 $b7 >M $8c39 $49 >M
T{ op PC S A X Y P -> $8c39 $e6 $be $61 $88 $a4 }T T{ $0018 M $00b7 M $8c37 M $8c38 M $8c39 M -> $69 $e0 $55 $b7 $49 }T
$1d99 >PC $44 >S $96 >A $e9 >X $c9 >Y $64 >P $004b $ec >M $0062 $f8 >M $1d99 $55 >M $1d9a $62 >M $1d9b $0e >M
T{ op PC S A X Y P -> $1d9b $44 $7a $e9 $c9 $64 }T T{ $004b M $0062 M $1d99 M $1d9a M $1d9b M -> $ec $f8 $55 $62 $0e }T
$f9fc >PC $d0 >S $50 >A $94 >X $ff >Y $25 >P $000e $9a >M $007a $b2 >M $f9fc $55 >M $f9fd $7a >M $f9fe $39 >M
T{ op PC S A X Y P -> $f9fe $d0 $ca $94 $ff $a5 }T T{ $000e M $007a M $f9fc M $f9fd M $f9fe M -> $9a $b2 $55 $7a $39 }T
$19ad >PC $f5 >S $99 >A $0b >X $1a >Y $e7 >P $0018 $83 >M $0023 $9e >M $19ad $55 >M $19ae $18 >M $19af $95 >M
T{ op PC S A X Y P -> $19af $f5 $07 $0b $1a $65 }T T{ $0018 M $0023 M $19ad M $19ae M $19af M -> $83 $9e $55 $18 $95 }T
$bd41 >PC $44 >S $6f >A $03 >X $88 >Y $65 >P $0076 $61 >M $0079 $0e >M $bd41 $55 >M $bd42 $76 >M $bd43 $47 >M
T{ op PC S A X Y P -> $bd43 $44 $61 $03 $88 $65 }T T{ $0076 M $0079 M $bd41 M $bd42 M $bd43 M -> $61 $0e $55 $76 $47 }T
$2fb5 >PC $1d >S $52 >A $ef >X $07 >Y $66 >P $00e1 $74 >M $00f2 $cc >M $2fb5 $55 >M $2fb6 $f2 >M $2fb7 $5b >M
T{ op PC S A X Y P -> $2fb7 $1d $26 $ef $07 $64 }T T{ $00e1 M $00f2 M $2fb5 M $2fb6 M $2fb7 M -> $74 $cc $55 $f2 $5b }T
$fe7b >PC $28 >S $a5 >A $dd >X $f6 >Y $61 >P $006f $fd >M $0092 $ac >M $fe7b $55 >M $fe7c $92 >M $fe7d $da >M
T{ op PC S A X Y P -> $fe7d $28 $58 $dd $f6 $61 }T T{ $006f M $0092 M $fe7b M $fe7c M $fe7d M -> $fd $ac $55 $92 $da }T
$c4cd >PC $f1 >S $b2 >A $06 >X $8a >Y $a6 >P $00f2 $f7 >M $00f8 $a1 >M $c4cd $55 >M $c4ce $f2 >M $c4cf $a2 >M
T{ op PC S A X Y P -> $c4cf $f1 $13 $06 $8a $24 }T T{ $00f2 M $00f8 M $c4cd M $c4ce M $c4cf M -> $f7 $a1 $55 $f2 $a2 }T
( 56 )
$6b24 >PC $5b >S $f9 >A $f5 >X $b8 >Y $27 >P $0066 $f5 >M $0071 $39 >M $6b24 $56 >M $6b25 $71 >M $6b26 $bf >M
T{ op PC S A X Y P -> $6b26 $5b $f9 $f5 $b8 $25 }T T{ $0066 M $0071 M $6b24 M $6b25 M $6b26 M -> $7a $39 $56 $71 $bf }T
$ca4e >PC $3f >S $43 >A $d0 >X $a8 >Y $66 >P $0023 $cc >M $0053 $9b >M $ca4e $56 >M $ca4f $53 >M $ca50 $e9 >M
T{ op PC S A X Y P -> $ca50 $3f $43 $d0 $a8 $64 }T T{ $0023 M $0053 M $ca4e M $ca4f M $ca50 M -> $66 $9b $56 $53 $e9 }T
$4c3a >PC $46 >S $b0 >A $a1 >X $53 >Y $26 >P $0090 $13 >M $00ef $ad >M $4c3a $56 >M $4c3b $ef >M $4c3c $1f >M
T{ op PC S A X Y P -> $4c3c $46 $b0 $a1 $53 $25 }T T{ $0090 M $00ef M $4c3a M $4c3b M $4c3c M -> $09 $ad $56 $ef $1f }T
$eb2f >PC $c0 >S $1e >A $54 >X $81 >Y $a2 >P $003b $9b >M $008f $14 >M $eb2f $56 >M $eb30 $3b >M $eb31 $7a >M
T{ op PC S A X Y P -> $eb31 $c0 $1e $54 $81 $20 }T T{ $003b M $008f M $eb2f M $eb30 M $eb31 M -> $9b $0a $56 $3b $7a }T
$dff4 >PC $86 >S $d5 >A $88 >X $fb >Y $67 >P $0043 $4b >M $00cb $10 >M $dff4 $56 >M $dff5 $43 >M $dff6 $51 >M
T{ op PC S A X Y P -> $dff6 $86 $d5 $88 $fb $64 }T T{ $0043 M $00cb M $dff4 M $dff5 M $dff6 M -> $4b $08 $56 $43 $51 }T
$0379 >PC $c8 >S $52 >A $5b >X $70 >Y $24 >P $0024 $bc >M $007f $bf >M $0379 $56 >M $037a $24 >M $037b $a0 >M
T{ op PC S A X Y P -> $037b $c8 $52 $5b $70 $25 }T T{ $0024 M $007f M $0379 M $037a M $037b M -> $bc $5f $56 $24 $a0 }T
$7a08 >PC $20 >S $42 >A $c4 >X $6c >Y $22 >P $00a7 $96 >M $00e3 $2e >M $7a08 $56 >M $7a09 $e3 >M $7a0a $92 >M
T{ op PC S A X Y P -> $7a0a $20 $42 $c4 $6c $20 }T T{ $00a7 M $00e3 M $7a08 M $7a09 M $7a0a M -> $4b $2e $56 $e3 $92 }T
$043b >PC $9e >S $dd >A $04 >X $6c >Y $e7 >P $00a4 $90 >M $00a8 $92 >M $043b $56 >M $043c $a4 >M $043d $47 >M
T{ op PC S A X Y P -> $043d $9e $dd $04 $6c $64 }T T{ $00a4 M $00a8 M $043b M $043c M $043d M -> $90 $49 $56 $a4 $47 }T
$4604 >PC $7f >S $8f >A $5b >X $71 >Y $65 >P $002a $81 >M $00cf $54 >M $4604 $56 >M $4605 $cf >M $4606 $a7 >M
T{ op PC S A X Y P -> $4606 $7f $8f $5b $71 $65 }T T{ $002a M $00cf M $4604 M $4605 M $4606 M -> $40 $54 $56 $cf $a7 }T
$2e8d >PC $a5 >S $a0 >A $d6 >X $a4 >Y $e5 >P $0028 $7f >M $0052 $8e >M $2e8d $56 >M $2e8e $52 >M $2e8f $2a >M
T{ op PC S A X Y P -> $2e8f $a5 $a0 $d6 $a4 $65 }T T{ $0028 M $0052 M $2e8d M $2e8e M $2e8f M -> $3f $8e $56 $52 $2a }T
$997c >PC $c1 >S $53 >A $dc >X $59 >Y $60 >P $00a6 $4b >M $00ca $2d >M $997c $56 >M $997d $ca >M $997e $ac >M
T{ op PC S A X Y P -> $997e $c1 $53 $dc $59 $61 }T T{ $00a6 M $00ca M $997c M $997d M $997e M -> $25 $2d $56 $ca $ac }T
$faa6 >PC $ba >S $bb >A $67 >X $5f >Y $64 >P $000f $03 >M $0076 $8a >M $faa6 $56 >M $faa7 $0f >M $faa8 $40 >M
T{ op PC S A X Y P -> $faa8 $ba $bb $67 $5f $64 }T T{ $000f M $0076 M $faa6 M $faa7 M $faa8 M -> $03 $45 $56 $0f $40 }T
$ab55 >PC $69 >S $7d >A $99 >X $a9 >Y $a0 >P $0046 $1b >M $00ad $8e >M $ab55 $56 >M $ab56 $ad >M $ab57 $a8 >M
T{ op PC S A X Y P -> $ab57 $69 $7d $99 $a9 $21 }T T{ $0046 M $00ad M $ab55 M $ab56 M $ab57 M -> $0d $8e $56 $ad $a8 }T
$352a >PC $4e >S $27 >A $d5 >X $df >Y $27 >P $001b $c2 >M $0046 $54 >M $352a $56 >M $352b $46 >M $352c $59 >M
T{ op PC S A X Y P -> $352c $4e $27 $d5 $df $24 }T T{ $001b M $0046 M $352a M $352b M $352c M -> $61 $54 $56 $46 $59 }T
$16fb >PC $20 >S $35 >A $cb >X $59 >Y $21 >P $0014 $b6 >M $00df $83 >M $16fb $56 >M $16fc $14 >M $16fd $b0 >M
T{ op PC S A X Y P -> $16fd $20 $35 $cb $59 $21 }T T{ $0014 M $00df M $16fb M $16fc M $16fd M -> $b6 $41 $56 $14 $b0 }T
$30a5 >PC $cf >S $70 >A $10 >X $89 >Y $26 >P $001f $70 >M $002f $da >M $30a5 $56 >M $30a6 $1f >M $30a7 $97 >M
T{ op PC S A X Y P -> $30a7 $cf $70 $10 $89 $24 }T T{ $001f M $002f M $30a5 M $30a6 M $30a7 M -> $70 $6d $56 $1f $97 }T
( 57 )
$2df5 >PC $c5 >S $87 >A $fb >X $15 >Y $66 >P $00d1 $e3 >M $2df5 $57 >M $2df6 $d1 >M $2df7 $36 >M
T{ op PC S A X Y P -> $2df7 $c5 $87 $fb $15 $66 }T T{ $00d1 M $2df5 M $2df6 M $2df7 M -> $c3 $57 $d1 $36 }T
$8daa >PC $90 >S $a6 >A $a1 >X $be >Y $27 >P $009e $ef >M $8daa $57 >M $8dab $9e >M $8dac $79 >M
T{ op PC S A X Y P -> $8dac $90 $a6 $a1 $be $27 }T T{ $009e M $8daa M $8dab M $8dac M -> $cf $57 $9e $79 }T
$8717 >PC $5b >S $b8 >A $39 >X $c3 >Y $e5 >P $00a4 $ee >M $8717 $57 >M $8718 $a4 >M $8719 $33 >M
T{ op PC S A X Y P -> $8719 $5b $b8 $39 $c3 $e5 }T T{ $00a4 M $8717 M $8718 M $8719 M -> $ce $57 $a4 $33 }T
$c2ed >PC $28 >S $7a >A $bf >X $e9 >Y $a6 >P $0050 $54 >M $c2ed $57 >M $c2ee $50 >M $c2ef $35 >M
T{ op PC S A X Y P -> $c2ef $28 $7a $bf $e9 $a6 }T T{ $0050 M $c2ed M $c2ee M $c2ef M -> $54 $57 $50 $35 }T
$115b >PC $f1 >S $e3 >A $0f >X $8d >Y $60 >P $005f $5d >M $115b $57 >M $115c $5f >M $115d $e9 >M
T{ op PC S A X Y P -> $115d $f1 $e3 $0f $8d $60 }T T{ $005f M $115b M $115c M $115d M -> $5d $57 $5f $e9 }T
$478e >PC $cf >S $6c >A $54 >X $75 >Y $e7 >P $0078 $3f >M $478e $57 >M $478f $78 >M $4790 $23 >M
T{ op PC S A X Y P -> $4790 $cf $6c $54 $75 $e7 }T T{ $0078 M $478e M $478f M $4790 M -> $1f $57 $78 $23 }T
$85da >PC $ae >S $ac >A $f4 >X $d6 >Y $e2 >P $0055 $6b >M $85da $57 >M $85db $55 >M $85dc $72 >M
T{ op PC S A X Y P -> $85dc $ae $ac $f4 $d6 $e2 }T T{ $0055 M $85da M $85db M $85dc M -> $4b $57 $55 $72 }T
$fbd3 >PC $17 >S $8c >A $16 >X $01 >Y $a6 >P $00c4 $44 >M $fbd3 $57 >M $fbd4 $c4 >M $fbd5 $f8 >M
T{ op PC S A X Y P -> $fbd5 $17 $8c $16 $01 $a6 }T T{ $00c4 M $fbd3 M $fbd4 M $fbd5 M -> $44 $57 $c4 $f8 }T
$dc4c >PC $f1 >S $4c >A $58 >X $ba >Y $e1 >P $004e $e3 >M $dc4c $57 >M $dc4d $4e >M $dc4e $6e >M
T{ op PC S A X Y P -> $dc4e $f1 $4c $58 $ba $e1 }T T{ $004e M $dc4c M $dc4d M $dc4e M -> $c3 $57 $4e $6e }T
$5b1b >PC $ee >S $21 >A $12 >X $4c >Y $67 >P $003e $78 >M $5b1b $57 >M $5b1c $3e >M $5b1d $33 >M
T{ op PC S A X Y P -> $5b1d $ee $21 $12 $4c $67 }T T{ $003e M $5b1b M $5b1c M $5b1d M -> $58 $57 $3e $33 }T
$28b3 >PC $30 >S $eb >A $4c >X $fe >Y $24 >P $0099 $df >M $28b3 $57 >M $28b4 $99 >M $28b5 $73 >M
T{ op PC S A X Y P -> $28b5 $30 $eb $4c $fe $24 }T T{ $0099 M $28b3 M $28b4 M $28b5 M -> $df $57 $99 $73 }T
$3445 >PC $b9 >S $17 >A $a0 >X $3d >Y $25 >P $0044 $63 >M $3445 $57 >M $3446 $44 >M $3447 $35 >M
T{ op PC S A X Y P -> $3447 $b9 $17 $a0 $3d $25 }T T{ $0044 M $3445 M $3446 M $3447 M -> $43 $57 $44 $35 }T
$36e1 >PC $14 >S $5e >A $d9 >X $4e >Y $e1 >P $00b5 $1b >M $36e1 $57 >M $36e2 $b5 >M $36e3 $93 >M
T{ op PC S A X Y P -> $36e3 $14 $5e $d9 $4e $e1 }T T{ $00b5 M $36e1 M $36e2 M $36e3 M -> $1b $57 $b5 $93 }T
$2462 >PC $47 >S $da >A $3a >X $09 >Y $e1 >P $000a $26 >M $2462 $57 >M $2463 $0a >M $2464 $ff >M
T{ op PC S A X Y P -> $2464 $47 $da $3a $09 $e1 }T T{ $000a M $2462 M $2463 M $2464 M -> $06 $57 $0a $ff }T
$af62 >PC $f8 >S $37 >A $f0 >X $ad >Y $a4 >P $0083 $7e >M $af62 $57 >M $af63 $83 >M $af64 $b2 >M
T{ op PC S A X Y P -> $af64 $f8 $37 $f0 $ad $a4 }T T{ $0083 M $af62 M $af63 M $af64 M -> $5e $57 $83 $b2 }T
$6dc0 >PC $35 >S $86 >A $b1 >X $53 >Y $e4 >P $007d $d4 >M $6dc0 $57 >M $6dc1 $7d >M $6dc2 $ea >M
T{ op PC S A X Y P -> $6dc2 $35 $86 $b1 $53 $e4 }T T{ $007d M $6dc0 M $6dc1 M $6dc2 M -> $d4 $57 $7d $ea }T
( 58 )
$712d >PC $7a >S $fd >A $a6 >X $3c >Y $a1 >P $712d $58 >M $712e $9d >M $712f $05 >M
T{ op PC S A X Y P -> $712e $7a $fd $a6 $3c $a1 }T T{ $712d M $712e M $712f M -> $58 $9d $05 }T
$29de >PC $01 >S $48 >A $73 >X $be >Y $23 >P $29de $58 >M $29df $e9 >M $29e0 $5f >M
T{ op PC S A X Y P -> $29df $01 $48 $73 $be $23 }T T{ $29de M $29df M $29e0 M -> $58 $e9 $5f }T
$140c >PC $61 >S $23 >A $f9 >X $93 >Y $e4 >P $140c $58 >M $140d $5a >M $140e $90 >M
T{ op PC S A X Y P -> $140d $61 $23 $f9 $93 $e0 }T T{ $140c M $140d M $140e M -> $58 $5a $90 }T
$decd >PC $3a >S $0e >A $d0 >X $53 >Y $e1 >P $decd $58 >M $dece $30 >M $decf $c5 >M
T{ op PC S A X Y P -> $dece $3a $0e $d0 $53 $e1 }T T{ $decd M $dece M $decf M -> $58 $30 $c5 }T
$c18b >PC $0f >S $39 >A $79 >X $ed >Y $e2 >P $c18b $58 >M $c18c $2d >M $c18d $93 >M
T{ op PC S A X Y P -> $c18c $0f $39 $79 $ed $e2 }T T{ $c18b M $c18c M $c18d M -> $58 $2d $93 }T
$1dfc >PC $e7 >S $37 >A $fb >X $14 >Y $e1 >P $1dfc $58 >M $1dfd $1a >M $1dfe $0f >M
T{ op PC S A X Y P -> $1dfd $e7 $37 $fb $14 $e1 }T T{ $1dfc M $1dfd M $1dfe M -> $58 $1a $0f }T
$aa20 >PC $9f >S $81 >A $fa >X $35 >Y $66 >P $aa20 $58 >M $aa21 $55 >M $aa22 $75 >M
T{ op PC S A X Y P -> $aa21 $9f $81 $fa $35 $62 }T T{ $aa20 M $aa21 M $aa22 M -> $58 $55 $75 }T
$7190 >PC $20 >S $56 >A $d3 >X $5c >Y $61 >P $7190 $58 >M $7191 $c1 >M $7192 $45 >M
T{ op PC S A X Y P -> $7191 $20 $56 $d3 $5c $61 }T T{ $7190 M $7191 M $7192 M -> $58 $c1 $45 }T
$f825 >PC $c1 >S $b5 >A $f4 >X $9f >Y $26 >P $f825 $58 >M $f826 $9a >M $f827 $f0 >M
T{ op PC S A X Y P -> $f826 $c1 $b5 $f4 $9f $22 }T T{ $f825 M $f826 M $f827 M -> $58 $9a $f0 }T
$82bd >PC $a0 >S $ab >A $44 >X $e2 >Y $66 >P $82bd $58 >M $82be $76 >M $82bf $3b >M
T{ op PC S A X Y P -> $82be $a0 $ab $44 $e2 $62 }T T{ $82bd M $82be M $82bf M -> $58 $76 $3b }T
$6243 >PC $2f >S $13 >A $9e >X $9a >Y $24 >P $6243 $58 >M $6244 $d3 >M $6245 $50 >M
T{ op PC S A X Y P -> $6244 $2f $13 $9e $9a $20 }T T{ $6243 M $6244 M $6245 M -> $58 $d3 $50 }T
$5ae7 >PC $18 >S $b7 >A $b0 >X $1c >Y $e7 >P $5ae7 $58 >M $5ae8 $8d >M $5ae9 $19 >M
T{ op PC S A X Y P -> $5ae8 $18 $b7 $b0 $1c $e3 }T T{ $5ae7 M $5ae8 M $5ae9 M -> $58 $8d $19 }T
$72e0 >PC $b3 >S $e3 >A $6f >X $57 >Y $a3 >P $72e0 $58 >M $72e1 $44 >M $72e2 $41 >M
T{ op PC S A X Y P -> $72e1 $b3 $e3 $6f $57 $a3 }T T{ $72e0 M $72e1 M $72e2 M -> $58 $44 $41 }T
$c7e4 >PC $b4 >S $0d >A $4b >X $73 >Y $26 >P $c7e4 $58 >M $c7e5 $cd >M $c7e6 $00 >M
T{ op PC S A X Y P -> $c7e5 $b4 $0d $4b $73 $22 }T T{ $c7e4 M $c7e5 M $c7e6 M -> $58 $cd $00 }T
$aa39 >PC $29 >S $0d >A $f2 >X $a0 >Y $a1 >P $aa39 $58 >M $aa3a $cc >M $aa3b $47 >M
T{ op PC S A X Y P -> $aa3a $29 $0d $f2 $a0 $a1 }T T{ $aa39 M $aa3a M $aa3b M -> $58 $cc $47 }T
$2111 >PC $42 >S $d9 >A $96 >X $85 >Y $e1 >P $2111 $58 >M $2112 $d4 >M $2113 $9c >M
T{ op PC S A X Y P -> $2112 $42 $d9 $96 $85 $e1 }T T{ $2111 M $2112 M $2113 M -> $58 $d4 $9c }T
( 59 )
$5c03 >PC $29 >S $a7 >A $f7 >X $78 >Y $a7 >P $4db1 $2f >M $5c03 $59 >M $5c04 $39 >M $5c05 $4d >M $5c06 $74 >M
T{ op PC S A X Y P -> $5c06 $29 $88 $f7 $78 $a5 }T T{ $4db1 M $5c03 M $5c04 M $5c05 M $5c06 M -> $2f $59 $39 $4d $74 }T
$bde0 >PC $3b >S $ae >A $d8 >X $31 >Y $e0 >P $75bb $e9 >M $bde0 $59 >M $bde1 $8a >M $bde2 $75 >M $bde3 $57 >M
T{ op PC S A X Y P -> $bde3 $3b $47 $d8 $31 $60 }T T{ $75bb M $bde0 M $bde1 M $bde2 M $bde3 M -> $e9 $59 $8a $75 $57 }T
$ba55 >PC $6d >S $55 >A $e3 >X $04 >Y $e5 >P $ab8d $a7 >M $ba55 $59 >M $ba56 $89 >M $ba57 $ab >M $ba58 $91 >M
T{ op PC S A X Y P -> $ba58 $6d $f2 $e3 $04 $e5 }T T{ $ab8d M $ba55 M $ba56 M $ba57 M $ba58 M -> $a7 $59 $89 $ab $91 }T
$28cc >PC $00 >S $b2 >A $b7 >X $68 >Y $e3 >P $106b $72 >M $28cc $59 >M $28cd $03 >M $28ce $10 >M $28cf $b7 >M
T{ op PC S A X Y P -> $28cf $00 $c0 $b7 $68 $e1 }T T{ $106b M $28cc M $28cd M $28ce M $28cf M -> $72 $59 $03 $10 $b7 }T
$c528 >PC $91 >S $99 >A $82 >X $dd >Y $a3 >P $c528 $59 >M $c529 $c6 >M $c52a $fc >M $c52b $84 >M $fda3 $0c >M
T{ op PC S A X Y P -> $c52b $91 $95 $82 $dd $a1 }T T{ $c528 M $c529 M $c52a M $c52b M $fda3 M -> $59 $c6 $fc $84 $0c }T
$6940 >PC $36 >S $fc >A $14 >X $cc >Y $20 >P $6940 $59 >M $6941 $d5 >M $6942 $be >M $6943 $5a >M $bfa1 $f1 >M
T{ op PC S A X Y P -> $6943 $36 $0d $14 $cc $20 }T T{ $6940 M $6941 M $6942 M $6943 M $bfa1 M -> $59 $d5 $be $5a $f1 }T
$b29b >PC $bc >S $51 >A $bf >X $2f >Y $a4 >P $b29b $59 >M $b29c $96 >M $b29d $f9 >M $b29e $62 >M $f9c5 $3c >M
T{ op PC S A X Y P -> $b29e $bc $6d $bf $2f $24 }T T{ $b29b M $b29c M $b29d M $b29e M $f9c5 M -> $59 $96 $f9 $62 $3c }T
$e4e1 >PC $20 >S $c5 >A $0e >X $c4 >Y $23 >P $b15d $e1 >M $e4e1 $59 >M $e4e2 $99 >M $e4e3 $b0 >M $e4e4 $56 >M
T{ op PC S A X Y P -> $e4e4 $20 $24 $0e $c4 $21 }T T{ $b15d M $e4e1 M $e4e2 M $e4e3 M $e4e4 M -> $e1 $59 $99 $b0 $56 }T
$228f >PC $df >S $a7 >A $6d >X $c7 >Y $21 >P $228f $59 >M $2290 $e4 >M $2291 $c4 >M $2292 $2f >M $c5ab $8e >M
T{ op PC S A X Y P -> $2292 $df $29 $6d $c7 $21 }T T{ $228f M $2290 M $2291 M $2292 M $c5ab M -> $59 $e4 $c4 $2f $8e }T
$9b70 >PC $92 >S $14 >A $66 >X $ac >Y $60 >P $550a $6f >M $9b70 $59 >M $9b71 $5e >M $9b72 $54 >M $9b73 $2d >M
T{ op PC S A X Y P -> $9b73 $92 $7b $66 $ac $60 }T T{ $550a M $9b70 M $9b71 M $9b72 M $9b73 M -> $6f $59 $5e $54 $2d }T
$6515 >PC $9e >S $75 >A $09 >X $4b >Y $e0 >P $6515 $59 >M $6516 $fa >M $6517 $f3 >M $6518 $db >M $f445 $8f >M
T{ op PC S A X Y P -> $6518 $9e $fa $09 $4b $e0 }T T{ $6515 M $6516 M $6517 M $6518 M $f445 M -> $59 $fa $f3 $db $8f }T
$97d1 >PC $52 >S $5e >A $68 >X $46 >Y $65 >P $97d1 $59 >M $97d2 $45 >M $97d3 $c0 >M $97d4 $df >M $c08b $68 >M
T{ op PC S A X Y P -> $97d4 $52 $36 $68 $46 $65 }T T{ $97d1 M $97d2 M $97d3 M $97d4 M $c08b M -> $59 $45 $c0 $df $68 }T
$30e9 >PC $f9 >S $00 >A $1f >X $c1 >Y $e6 >P $30e9 $59 >M $30ea $6c >M $30eb $d8 >M $30ec $ca >M $d92d $1c >M
T{ op PC S A X Y P -> $30ec $f9 $1c $1f $c1 $64 }T T{ $30e9 M $30ea M $30eb M $30ec M $d92d M -> $59 $6c $d8 $ca $1c }T
$22f4 >PC $c9 >S $aa >A $43 >X $f6 >Y $62 >P $22f4 $59 >M $22f5 $d0 >M $22f6 $7e >M $22f7 $c2 >M $7fc6 $8c >M
T{ op PC S A X Y P -> $22f7 $c9 $26 $43 $f6 $60 }T T{ $22f4 M $22f5 M $22f6 M $22f7 M $7fc6 M -> $59 $d0 $7e $c2 $8c }T
$bfc5 >PC $af >S $eb >A $58 >X $0b >Y $24 >P $0cd7 $af >M $bfc5 $59 >M $bfc6 $cc >M $bfc7 $0c >M $bfc8 $41 >M
T{ op PC S A X Y P -> $bfc8 $af $44 $58 $0b $24 }T T{ $0cd7 M $bfc5 M $bfc6 M $bfc7 M $bfc8 M -> $af $59 $cc $0c $41 }T
$6041 >PC $b6 >S $1d >A $c9 >X $7f >Y $63 >P $6041 $59 >M $6042 $b1 >M $6043 $9e >M $6044 $7b >M $9f30 $17 >M
T{ op PC S A X Y P -> $6044 $b6 $0a $c9 $7f $61 }T T{ $6041 M $6042 M $6043 M $6044 M $9f30 M -> $59 $b1 $9e $7b $17 }T
( 5a )
$61bc >PC $cb >S $f4 >A $8a >X $21 >Y $a7 >P $61bc $5a >M $61bd $96 >M $61be $10 >M
T{ op PC S A X Y P -> $61bd $ca $f4 $8a $21 $a7 }T T{ $01cb M $61bc M $61bd M $61be M -> $21 $5a $96 $10 }T
$0247 >PC $2c >S $8a >A $74 >X $2f >Y $65 >P $0247 $5a >M $0248 $1a >M $0249 $15 >M
T{ op PC S A X Y P -> $0248 $2b $8a $74 $2f $65 }T T{ $012c M $0247 M $0248 M $0249 M -> $2f $5a $1a $15 }T
$f2fc >PC $90 >S $82 >A $27 >X $85 >Y $20 >P $f2fc $5a >M $f2fd $10 >M $f2fe $06 >M
T{ op PC S A X Y P -> $f2fd $8f $82 $27 $85 $20 }T T{ $0190 M $f2fc M $f2fd M $f2fe M -> $85 $5a $10 $06 }T
$3626 >PC $2d >S $e0 >A $cb >X $a5 >Y $67 >P $3626 $5a >M $3627 $eb >M $3628 $79 >M
T{ op PC S A X Y P -> $3627 $2c $e0 $cb $a5 $67 }T T{ $012d M $3626 M $3627 M $3628 M -> $a5 $5a $eb $79 }T
$a76e >PC $7a >S $d8 >A $2e >X $79 >Y $22 >P $a76e $5a >M $a76f $63 >M $a770 $6a >M
T{ op PC S A X Y P -> $a76f $79 $d8 $2e $79 $22 }T T{ $017a M $a76e M $a76f M $a770 M -> $79 $5a $63 $6a }T
$d24c >PC $00 >S $4a >A $3a >X $70 >Y $20 >P $d24c $5a >M $d24d $50 >M $d24e $0d >M
T{ op PC S A X Y P -> $d24d $ff $4a $3a $70 $20 }T T{ $0100 M $d24c M $d24d M $d24e M -> $70 $5a $50 $0d }T
$e6ea >PC $8a >S $1e >A $db >X $2b >Y $a3 >P $e6ea $5a >M $e6eb $c1 >M $e6ec $b3 >M
T{ op PC S A X Y P -> $e6eb $89 $1e $db $2b $a3 }T T{ $018a M $e6ea M $e6eb M $e6ec M -> $2b $5a $c1 $b3 }T
$d388 >PC $92 >S $84 >A $c4 >X $13 >Y $e4 >P $d388 $5a >M $d389 $50 >M $d38a $ee >M
T{ op PC S A X Y P -> $d389 $91 $84 $c4 $13 $e4 }T T{ $0192 M $d388 M $d389 M $d38a M -> $13 $5a $50 $ee }T
$f57f >PC $62 >S $50 >A $e6 >X $cd >Y $66 >P $f57f $5a >M $f580 $39 >M $f581 $61 >M
T{ op PC S A X Y P -> $f580 $61 $50 $e6 $cd $66 }T T{ $0162 M $f57f M $f580 M $f581 M -> $cd $5a $39 $61 }T
$7674 >PC $a2 >S $ab >A $ea >X $49 >Y $a0 >P $7674 $5a >M $7675 $e4 >M $7676 $87 >M
T{ op PC S A X Y P -> $7675 $a1 $ab $ea $49 $a0 }T T{ $01a2 M $7674 M $7675 M $7676 M -> $49 $5a $e4 $87 }T
$1851 >PC $1b >S $21 >A $f0 >X $a5 >Y $e2 >P $1851 $5a >M $1852 $8e >M $1853 $63 >M
T{ op PC S A X Y P -> $1852 $1a $21 $f0 $a5 $e2 }T T{ $011b M $1851 M $1852 M $1853 M -> $a5 $5a $8e $63 }T
$68a8 >PC $2e >S $d2 >A $7a >X $a3 >Y $a3 >P $68a8 $5a >M $68a9 $75 >M $68aa $25 >M
T{ op PC S A X Y P -> $68a9 $2d $d2 $7a $a3 $a3 }T T{ $012e M $68a8 M $68a9 M $68aa M -> $a3 $5a $75 $25 }T
$e834 >PC $3b >S $2a >A $43 >X $ff >Y $66 >P $e834 $5a >M $e835 $c2 >M $e836 $11 >M
T{ op PC S A X Y P -> $e835 $3a $2a $43 $ff $66 }T T{ $013b M $e834 M $e835 M $e836 M -> $ff $5a $c2 $11 }T
$8b70 >PC $5c >S $6d >A $5b >X $b3 >Y $e0 >P $8b70 $5a >M $8b71 $3a >M $8b72 $a6 >M
T{ op PC S A X Y P -> $8b71 $5b $6d $5b $b3 $e0 }T T{ $015c M $8b70 M $8b71 M $8b72 M -> $b3 $5a $3a $a6 }T
$4830 >PC $78 >S $c8 >A $e6 >X $d1 >Y $21 >P $4830 $5a >M $4831 $08 >M $4832 $b4 >M
T{ op PC S A X Y P -> $4831 $77 $c8 $e6 $d1 $21 }T T{ $0178 M $4830 M $4831 M $4832 M -> $d1 $5a $08 $b4 }T
$9262 >PC $90 >S $94 >A $8c >X $d0 >Y $e6 >P $9262 $5a >M $9263 $62 >M $9264 $96 >M
T{ op PC S A X Y P -> $9263 $8f $94 $8c $d0 $e6 }T T{ $0190 M $9262 M $9263 M $9264 M -> $d0 $5a $62 $96 }T
( 5b )
$1ebe >PC $13 >S $a7 >A $18 >X $3e >Y $e3 >P $1ebe $5b >M $1ebf $bd >M $1ec0 $4a >M
T{ op PC S A X Y P -> $1ebf $13 $a7 $18 $3e $e3 }T T{ $1ebe M $1ebf M $1ec0 M -> $5b $bd $4a }T
$5a0d >PC $e3 >S $90 >A $c0 >X $e7 >Y $63 >P $5a0d $5b >M $5a0e $89 >M $5a0f $38 >M
T{ op PC S A X Y P -> $5a0e $e3 $90 $c0 $e7 $63 }T T{ $5a0d M $5a0e M $5a0f M -> $5b $89 $38 }T
$697d >PC $99 >S $f3 >A $dc >X $bf >Y $64 >P $697d $5b >M $697e $52 >M $697f $3b >M
T{ op PC S A X Y P -> $697e $99 $f3 $dc $bf $64 }T T{ $697d M $697e M $697f M -> $5b $52 $3b }T
$f9d5 >PC $c3 >S $a5 >A $21 >X $08 >Y $22 >P $f9d5 $5b >M $f9d6 $af >M $f9d7 $27 >M
T{ op PC S A X Y P -> $f9d6 $c3 $a5 $21 $08 $22 }T T{ $f9d5 M $f9d6 M $f9d7 M -> $5b $af $27 }T
$0f31 >PC $96 >S $5b >A $62 >X $88 >Y $a3 >P $0f31 $5b >M $0f32 $24 >M $0f33 $1f >M
T{ op PC S A X Y P -> $0f32 $96 $5b $62 $88 $a3 }T T{ $0f31 M $0f32 M $0f33 M -> $5b $24 $1f }T
$faa3 >PC $7c >S $d7 >A $46 >X $da >Y $25 >P $faa3 $5b >M $faa4 $44 >M $faa5 $db >M
T{ op PC S A X Y P -> $faa4 $7c $d7 $46 $da $25 }T T{ $faa3 M $faa4 M $faa5 M -> $5b $44 $db }T
$dd9d >PC $90 >S $5d >A $1c >X $de >Y $61 >P $dd9d $5b >M $dd9e $8f >M $dd9f $74 >M
T{ op PC S A X Y P -> $dd9e $90 $5d $1c $de $61 }T T{ $dd9d M $dd9e M $dd9f M -> $5b $8f $74 }T
$3f16 >PC $36 >S $98 >A $ce >X $55 >Y $62 >P $3f16 $5b >M $3f17 $be >M $3f18 $a2 >M
T{ op PC S A X Y P -> $3f17 $36 $98 $ce $55 $62 }T T{ $3f16 M $3f17 M $3f18 M -> $5b $be $a2 }T
$aa2e >PC $1f >S $5d >A $be >X $e4 >Y $64 >P $aa2e $5b >M $aa2f $e0 >M $aa30 $6b >M
T{ op PC S A X Y P -> $aa2f $1f $5d $be $e4 $64 }T T{ $aa2e M $aa2f M $aa30 M -> $5b $e0 $6b }T
$3572 >PC $a1 >S $63 >A $a4 >X $77 >Y $e1 >P $3572 $5b >M $3573 $7b >M $3574 $18 >M
T{ op PC S A X Y P -> $3573 $a1 $63 $a4 $77 $e1 }T T{ $3572 M $3573 M $3574 M -> $5b $7b $18 }T
$a7a9 >PC $0b >S $69 >A $fc >X $e1 >Y $61 >P $a7a9 $5b >M $a7aa $11 >M $a7ab $2d >M
T{ op PC S A X Y P -> $a7aa $0b $69 $fc $e1 $61 }T T{ $a7a9 M $a7aa M $a7ab M -> $5b $11 $2d }T
$ff10 >PC $21 >S $b9 >A $5e >X $8e >Y $e3 >P $ff10 $5b >M $ff11 $72 >M $ff12 $95 >M
T{ op PC S A X Y P -> $ff11 $21 $b9 $5e $8e $e3 }T T{ $ff10 M $ff11 M $ff12 M -> $5b $72 $95 }T
$2c06 >PC $30 >S $0b >A $9b >X $bc >Y $e0 >P $2c06 $5b >M $2c07 $e4 >M $2c08 $05 >M
T{ op PC S A X Y P -> $2c07 $30 $0b $9b $bc $e0 }T T{ $2c06 M $2c07 M $2c08 M -> $5b $e4 $05 }T
$545f >PC $c7 >S $10 >A $3d >X $93 >Y $62 >P $545f $5b >M $5460 $b4 >M $5461 $61 >M
T{ op PC S A X Y P -> $5460 $c7 $10 $3d $93 $62 }T T{ $545f M $5460 M $5461 M -> $5b $b4 $61 }T
$1650 >PC $bd >S $77 >A $c9 >X $9a >Y $e0 >P $1650 $5b >M $1651 $0c >M $1652 $c0 >M
T{ op PC S A X Y P -> $1651 $bd $77 $c9 $9a $e0 }T T{ $1650 M $1651 M $1652 M -> $5b $0c $c0 }T
$8849 >PC $8e >S $a4 >A $9d >X $43 >Y $a0 >P $8849 $5b >M $884a $25 >M $884b $e4 >M
T{ op PC S A X Y P -> $884a $8e $a4 $9d $43 $a0 }T T{ $8849 M $884a M $884b M -> $5b $25 $e4 }T
( 5c )
$8237 >PC $1d >S $6f >A $63 >X $57 >Y $64 >P $8237 $5c >M $8238 $d4 >M $8239 $ee >M $823a $8b >M
T{ op PC S A X Y P -> $823a $1d $6f $63 $57 $64 }T T{ $8237 M $8238 M $8239 M $823a M -> $5c $d4 $ee $8b }T
$d2f8 >PC $a6 >S $e0 >A $ab >X $04 >Y $e6 >P $d2f8 $5c >M $d2f9 $65 >M $d2fa $fb >M $d2fb $20 >M
T{ op PC S A X Y P -> $d2fb $a6 $e0 $ab $04 $e6 }T T{ $d2f8 M $d2f9 M $d2fa M $d2fb M -> $5c $65 $fb $20 }T
$3821 >PC $65 >S $ac >A $a0 >X $9c >Y $e6 >P $3821 $5c >M $3822 $7e >M $3823 $63 >M $3824 $3f >M
T{ op PC S A X Y P -> $3824 $65 $ac $a0 $9c $e6 }T T{ $3821 M $3822 M $3823 M $3824 M -> $5c $7e $63 $3f }T
$24ce >PC $12 >S $79 >A $a0 >X $68 >Y $63 >P $24ce $5c >M $24cf $c4 >M $24d0 $3a >M $24d1 $bd >M
T{ op PC S A X Y P -> $24d1 $12 $79 $a0 $68 $63 }T T{ $24ce M $24cf M $24d0 M $24d1 M -> $5c $c4 $3a $bd }T
$a41a >PC $ed >S $88 >A $79 >X $e6 >Y $20 >P $a41a $5c >M $a41b $8b >M $a41c $61 >M $a41d $e1 >M
T{ op PC S A X Y P -> $a41d $ed $88 $79 $e6 $20 }T T{ $a41a M $a41b M $a41c M $a41d M -> $5c $8b $61 $e1 }T
$05f1 >PC $d5 >S $66 >A $75 >X $4f >Y $65 >P $05f1 $5c >M $05f2 $a5 >M $05f3 $f9 >M $05f4 $41 >M
T{ op PC S A X Y P -> $05f4 $d5 $66 $75 $4f $65 }T T{ $05f1 M $05f2 M $05f3 M $05f4 M -> $5c $a5 $f9 $41 }T
$1580 >PC $f4 >S $e5 >A $10 >X $a3 >Y $24 >P $1580 $5c >M $1581 $f6 >M $1582 $aa >M $1583 $a7 >M
T{ op PC S A X Y P -> $1583 $f4 $e5 $10 $a3 $24 }T T{ $1580 M $1581 M $1582 M $1583 M -> $5c $f6 $aa $a7 }T
$b0ec >PC $ce >S $15 >A $59 >X $44 >Y $e7 >P $b0ec $5c >M $b0ed $c0 >M $b0ee $58 >M $b0ef $1a >M
T{ op PC S A X Y P -> $b0ef $ce $15 $59 $44 $e7 }T T{ $b0ec M $b0ed M $b0ee M $b0ef M -> $5c $c0 $58 $1a }T
$f216 >PC $ea >S $e9 >A $54 >X $c0 >Y $60 >P $f216 $5c >M $f217 $d3 >M $f218 $2e >M $f219 $26 >M
T{ op PC S A X Y P -> $f219 $ea $e9 $54 $c0 $60 }T T{ $f216 M $f217 M $f218 M $f219 M -> $5c $d3 $2e $26 }T
$a552 >PC $3c >S $3c >A $cf >X $dc >Y $20 >P $a552 $5c >M $a553 $03 >M $a554 $88 >M $a555 $95 >M
T{ op PC S A X Y P -> $a555 $3c $3c $cf $dc $20 }T T{ $a552 M $a553 M $a554 M $a555 M -> $5c $03 $88 $95 }T
$a41c >PC $3d >S $11 >A $18 >X $a0 >Y $a3 >P $a41c $5c >M $a41d $ee >M $a41e $d2 >M $a41f $1c >M
T{ op PC S A X Y P -> $a41f $3d $11 $18 $a0 $a3 }T T{ $a41c M $a41d M $a41e M $a41f M -> $5c $ee $d2 $1c }T
$9cdf >PC $d2 >S $1b >A $4a >X $9b >Y $67 >P $9cdf $5c >M $9ce0 $b2 >M $9ce1 $94 >M $9ce2 $b8 >M
T{ op PC S A X Y P -> $9ce2 $d2 $1b $4a $9b $67 }T T{ $9cdf M $9ce0 M $9ce1 M $9ce2 M -> $5c $b2 $94 $b8 }T
$dd3a >PC $16 >S $ba >A $c2 >X $8a >Y $63 >P $dd3a $5c >M $dd3b $76 >M $dd3c $79 >M $dd3d $89 >M
T{ op PC S A X Y P -> $dd3d $16 $ba $c2 $8a $63 }T T{ $dd3a M $dd3b M $dd3c M $dd3d M -> $5c $76 $79 $89 }T
$680f >PC $8b >S $5c >A $59 >X $ac >Y $63 >P $680f $5c >M $6810 $b6 >M $6811 $c0 >M $6812 $6e >M
T{ op PC S A X Y P -> $6812 $8b $5c $59 $ac $63 }T T{ $680f M $6810 M $6811 M $6812 M -> $5c $b6 $c0 $6e }T
$aee2 >PC $5e >S $f0 >A $22 >X $17 >Y $a2 >P $aee2 $5c >M $aee3 $73 >M $aee4 $61 >M $aee5 $25 >M
T{ op PC S A X Y P -> $aee5 $5e $f0 $22 $17 $a2 }T T{ $aee2 M $aee3 M $aee4 M $aee5 M -> $5c $73 $61 $25 }T
$1aa4 >PC $38 >S $5f >A $99 >X $ba >Y $25 >P $1aa4 $5c >M $1aa5 $02 >M $1aa6 $dd >M $1aa7 $4b >M
T{ op PC S A X Y P -> $1aa7 $38 $5f $99 $ba $25 }T T{ $1aa4 M $1aa5 M $1aa6 M $1aa7 M -> $5c $02 $dd $4b }T
( 5d )
$be24 >PC $3a >S $d7 >A $55 >X $dd >Y $e2 >P $7297 $3a >M $be24 $5d >M $be25 $42 >M $be26 $72 >M $be27 $1b >M
T{ op PC S A X Y P -> $be27 $3a $ed $55 $dd $e0 }T T{ $7297 M $be24 M $be25 M $be26 M $be27 M -> $3a $5d $42 $72 $1b }T
$2681 >PC $c2 >S $f0 >A $de >X $5f >Y $a1 >P $2681 $5d >M $2682 $c8 >M $2683 $53 >M $2684 $b9 >M $54a6 $7d >M
T{ op PC S A X Y P -> $2684 $c2 $8d $de $5f $a1 }T T{ $2681 M $2682 M $2683 M $2684 M $54a6 M -> $5d $c8 $53 $b9 $7d }T
$8ec1 >PC $14 >S $5c >A $18 >X $fe >Y $60 >P $8ec1 $5d >M $8ec2 $6e >M $8ec3 $cc >M $8ec4 $da >M $cc86 $b2 >M
T{ op PC S A X Y P -> $8ec4 $14 $ee $18 $fe $e0 }T T{ $8ec1 M $8ec2 M $8ec3 M $8ec4 M $cc86 M -> $5d $6e $cc $da $b2 }T
$efc9 >PC $c9 >S $f4 >A $8a >X $f5 >Y $65 >P $a015 $84 >M $efc9 $5d >M $efca $8b >M $efcb $9f >M $efcc $ea >M
T{ op PC S A X Y P -> $efcc $c9 $70 $8a $f5 $65 }T T{ $a015 M $efc9 M $efca M $efcb M $efcc M -> $84 $5d $8b $9f $ea }T
$83b7 >PC $92 >S $50 >A $fa >X $82 >Y $22 >P $83b7 $5d >M $83b8 $ac >M $83b9 $cf >M $83ba $2c >M $d0a6 $13 >M
T{ op PC S A X Y P -> $83ba $92 $43 $fa $82 $20 }T T{ $83b7 M $83b8 M $83b9 M $83ba M $d0a6 M -> $5d $ac $cf $2c $13 }T
$9065 >PC $0c >S $37 >A $7e >X $ed >Y $62 >P $6f56 $72 >M $9065 $5d >M $9066 $d8 >M $9067 $6e >M $9068 $b1 >M
T{ op PC S A X Y P -> $9068 $0c $45 $7e $ed $60 }T T{ $6f56 M $9065 M $9066 M $9067 M $9068 M -> $72 $5d $d8 $6e $b1 }T
$8034 >PC $97 >S $4d >A $1c >X $b8 >Y $27 >P $2d32 $09 >M $8034 $5d >M $8035 $16 >M $8036 $2d >M $8037 $9e >M
T{ op PC S A X Y P -> $8037 $97 $44 $1c $b8 $25 }T T{ $2d32 M $8034 M $8035 M $8036 M $8037 M -> $09 $5d $16 $2d $9e }T
$1f24 >PC $55 >S $e9 >A $a2 >X $46 >Y $a4 >P $1f24 $5d >M $1f25 $09 >M $1f26 $4f >M $1f27 $78 >M $4fab $d0 >M
T{ op PC S A X Y P -> $1f27 $55 $39 $a2 $46 $24 }T T{ $1f24 M $1f25 M $1f26 M $1f27 M $4fab M -> $5d $09 $4f $78 $d0 }T
$cb24 >PC $ff >S $a3 >A $79 >X $9b >Y $e5 >P $9fcd $37 >M $cb24 $5d >M $cb25 $54 >M $cb26 $9f >M $cb27 $76 >M
T{ op PC S A X Y P -> $cb27 $ff $94 $79 $9b $e5 }T T{ $9fcd M $cb24 M $cb25 M $cb26 M $cb27 M -> $37 $5d $54 $9f $76 }T
$3879 >PC $9e >S $28 >A $81 >X $d4 >Y $60 >P $3879 $5d >M $387a $ad >M $387b $db >M $387c $aa >M $dc2e $af >M
T{ op PC S A X Y P -> $387c $9e $87 $81 $d4 $e0 }T T{ $3879 M $387a M $387b M $387c M $dc2e M -> $5d $ad $db $aa $af }T
$c297 >PC $7b >S $72 >A $d1 >X $68 >Y $66 >P $c297 $5d >M $c298 $2f >M $c299 $fb >M $c29a $75 >M $fc00 $fd >M
T{ op PC S A X Y P -> $c29a $7b $8f $d1 $68 $e4 }T T{ $c297 M $c298 M $c299 M $c29a M $fc00 M -> $5d $2f $fb $75 $fd }T
$970f >PC $0e >S $4c >A $98 >X $91 >Y $a6 >P $6001 $5d >M $970f $5d >M $9710 $69 >M $9711 $5f >M $9712 $ae >M
T{ op PC S A X Y P -> $9712 $0e $11 $98 $91 $24 }T T{ $6001 M $970f M $9710 M $9711 M $9712 M -> $5d $5d $69 $5f $ae }T
$49c3 >PC $0c >S $73 >A $8d >X $b0 >Y $e4 >P $0b65 $0e >M $49c3 $5d >M $49c4 $d8 >M $49c5 $0a >M $49c6 $1b >M
T{ op PC S A X Y P -> $49c6 $0c $7d $8d $b0 $64 }T T{ $0b65 M $49c3 M $49c4 M $49c5 M $49c6 M -> $0e $5d $d8 $0a $1b }T
$c5d6 >PC $81 >S $32 >A $43 >X $dd >Y $e2 >P $c5d6 $5d >M $c5d7 $93 >M $c5d8 $e2 >M $c5d9 $63 >M $e2d6 $d7 >M
T{ op PC S A X Y P -> $c5d9 $81 $e5 $43 $dd $e0 }T T{ $c5d6 M $c5d7 M $c5d8 M $c5d9 M $e2d6 M -> $5d $93 $e2 $63 $d7 }T
$a12c >PC $4b >S $01 >A $19 >X $99 >Y $22 >P $7f8d $5d >M $a12c $5d >M $a12d $74 >M $a12e $7f >M $a12f $96 >M
T{ op PC S A X Y P -> $a12f $4b $5c $19 $99 $20 }T T{ $7f8d M $a12c M $a12d M $a12e M $a12f M -> $5d $5d $74 $7f $96 }T
$1eb1 >PC $4d >S $91 >A $61 >X $20 >Y $e4 >P $1eb1 $5d >M $1eb2 $78 >M $1eb3 $ac >M $1eb4 $91 >M $acd9 $12 >M
T{ op PC S A X Y P -> $1eb4 $4d $83 $61 $20 $e4 }T T{ $1eb1 M $1eb2 M $1eb3 M $1eb4 M $acd9 M -> $5d $78 $ac $91 $12 }T
( 5e )
$1646 >PC $cc >S $13 >A $17 >X $ab >Y $a2 >P $1646 $5e >M $1647 $02 >M $1648 $af >M $1649 $f9 >M $af19 $2c >M
T{ op PC S A X Y P -> $1649 $cc $13 $17 $ab $20 }T T{ $1646 M $1647 M $1648 M $1649 M $af19 M -> $5e $02 $af $f9 $16 }T
$fc45 >PC $24 >S $7a >A $1b >X $a4 >Y $a4 >P $34dd $97 >M $fc45 $5e >M $fc46 $c2 >M $fc47 $34 >M $fc48 $76 >M
T{ op PC S A X Y P -> $fc48 $24 $7a $1b $a4 $25 }T T{ $34dd M $fc45 M $fc46 M $fc47 M $fc48 M -> $4b $5e $c2 $34 $76 }T
$0434 >PC $0c >S $dc >A $dc >X $4f >Y $27 >P $0434 $5e >M $0435 $4a >M $0436 $b5 >M $0437 $56 >M $b626 $d4 >M
T{ op PC S A X Y P -> $0437 $0c $dc $dc $4f $24 }T T{ $0434 M $0435 M $0436 M $0437 M $b626 M -> $5e $4a $b5 $56 $6a }T
$54c8 >PC $55 >S $f0 >A $a3 >X $c8 >Y $66 >P $2cc5 $ca >M $54c8 $5e >M $54c9 $22 >M $54ca $2c >M $54cb $70 >M
T{ op PC S A X Y P -> $54cb $55 $f0 $a3 $c8 $64 }T T{ $2cc5 M $54c8 M $54c9 M $54ca M $54cb M -> $65 $5e $22 $2c $70 }T
$b595 >PC $5d >S $50 >A $1e >X $6c >Y $63 >P $97ae $d6 >M $b595 $5e >M $b596 $90 >M $b597 $97 >M $b598 $48 >M
T{ op PC S A X Y P -> $b598 $5d $50 $1e $6c $60 }T T{ $97ae M $b595 M $b596 M $b597 M $b598 M -> $6b $5e $90 $97 $48 }T
$4f7e >PC $7c >S $a7 >A $95 >X $f6 >Y $23 >P $4f7e $5e >M $4f7f $8c >M $4f80 $a0 >M $4f81 $e6 >M $a121 $78 >M
T{ op PC S A X Y P -> $4f81 $7c $a7 $95 $f6 $20 }T T{ $4f7e M $4f7f M $4f80 M $4f81 M $a121 M -> $5e $8c $a0 $e6 $3c }T
$e706 >PC $30 >S $6e >A $58 >X $ee >Y $24 >P $180a $5d >M $e706 $5e >M $e707 $b2 >M $e708 $17 >M $e709 $29 >M
T{ op PC S A X Y P -> $e709 $30 $6e $58 $ee $25 }T T{ $180a M $e706 M $e707 M $e708 M $e709 M -> $2e $5e $b2 $17 $29 }T
$d318 >PC $f3 >S $ce >A $95 >X $13 >Y $a4 >P $04d4 $6c >M $d318 $5e >M $d319 $3f >M $d31a $04 >M $d31b $91 >M
T{ op PC S A X Y P -> $d31b $f3 $ce $95 $13 $24 }T T{ $04d4 M $d318 M $d319 M $d31a M $d31b M -> $36 $5e $3f $04 $91 }T
$70a2 >PC $61 >S $c7 >A $4d >X $32 >Y $27 >P $70a2 $5e >M $70a3 $ea >M $70a4 $ee >M $70a5 $67 >M $ef37 $d6 >M
T{ op PC S A X Y P -> $70a5 $61 $c7 $4d $32 $24 }T T{ $70a2 M $70a3 M $70a4 M $70a5 M $ef37 M -> $5e $ea $ee $67 $6b }T
$942c >PC $00 >S $af >A $6e >X $db >Y $e3 >P $69c7 $95 >M $942c $5e >M $942d $59 >M $942e $69 >M $942f $17 >M
T{ op PC S A X Y P -> $942f $00 $af $6e $db $61 }T T{ $69c7 M $942c M $942d M $942e M $942f M -> $4a $5e $59 $69 $17 }T
$b175 >PC $33 >S $a3 >A $77 >X $31 >Y $65 >P $b175 $5e >M $b176 $23 >M $b177 $b8 >M $b178 $29 >M $b89a $dc >M
T{ op PC S A X Y P -> $b178 $33 $a3 $77 $31 $64 }T T{ $b175 M $b176 M $b177 M $b178 M $b89a M -> $5e $23 $b8 $29 $6e }T
$7012 >PC $93 >S $8f >A $bd >X $31 >Y $a6 >P $40a6 $15 >M $7012 $5e >M $7013 $e9 >M $7014 $3f >M $7015 $9b >M
T{ op PC S A X Y P -> $7015 $93 $8f $bd $31 $25 }T T{ $40a6 M $7012 M $7013 M $7014 M $7015 M -> $0a $5e $e9 $3f $9b }T
$1d33 >PC $d4 >S $89 >A $8d >X $36 >Y $26 >P $1d33 $5e >M $1d34 $58 >M $1d35 $a2 >M $1d36 $87 >M $a2e5 $1a >M
T{ op PC S A X Y P -> $1d36 $d4 $89 $8d $36 $24 }T T{ $1d33 M $1d34 M $1d35 M $1d36 M $a2e5 M -> $5e $58 $a2 $87 $0d }T
$9eef >PC $a4 >S $31 >A $2b >X $fa >Y $a1 >P $9eef $5e >M $9ef0 $3d >M $9ef1 $ae >M $9ef2 $55 >M $ae68 $c7 >M
T{ op PC S A X Y P -> $9ef2 $a4 $31 $2b $fa $21 }T T{ $9eef M $9ef0 M $9ef1 M $9ef2 M $ae68 M -> $5e $3d $ae $55 $63 }T
$1b9e >PC $80 >S $50 >A $8b >X $c0 >Y $27 >P $0048 $8e >M $1b9e $5e >M $1b9f $bd >M $1ba0 $ff >M $1ba1 $e9 >M
T{ op PC S A X Y P -> $1ba1 $80 $50 $8b $c0 $24 }T T{ $0048 M $1b9e M $1b9f M $1ba0 M $1ba1 M -> $47 $5e $bd $ff $e9 }T
$54f5 >PC $7b >S $7d >A $f9 >X $cb >Y $e2 >P $54f5 $5e >M $54f6 $96 >M $54f7 $a5 >M $54f8 $b3 >M $a68f $80 >M
T{ op PC S A X Y P -> $54f8 $7b $7d $f9 $cb $60 }T T{ $54f5 M $54f6 M $54f7 M $54f8 M $a68f M -> $5e $96 $a5 $b3 $40 }T
( 5f )
$60ea >PC $03 >S $90 >A $e2 >X $ca >Y $26 >P $00af $8f >M $60a7 $f0 >M $60ea $5f >M $60eb $af >M $60ec $ba >M
T{ op PC S A X Y P -> $60a7 $03 $90 $e2 $ca $26 }T T{ $00af M $60a7 M $60ea M $60eb M $60ec M -> $8f $f0 $5f $af $ba }T
$2768 >PC $97 >S $59 >A $64 >X $bd >Y $a1 >P $006e $6e >M $274b $b2 >M $2768 $5f >M $2769 $6e >M $276a $e0 >M $276b $ba >M
T{ op PC S A X Y P -> $276b $97 $59 $64 $bd $a1 }T T{ $006e M $274b M $2768 M $2769 M $276a M $276b M -> $6e $b2 $5f $6e $e0 $ba }T
$9719 >PC $35 >S $67 >A $82 >X $e2 >Y $24 >P $00fc $44 >M $96e4 $8f >M $9719 $5f >M $971a $fc >M $971b $c8 >M $97e4 $07 >M
T{ op PC S A X Y P -> $96e4 $35 $67 $82 $e2 $24 }T T{ $00fc M $96e4 M $9719 M $971a M $971b M $97e4 M -> $44 $8f $5f $fc $c8 $07 }T
$56d9 >PC $ca >S $d8 >A $41 >X $ff >Y $23 >P $007a $e2 >M $5636 $22 >M $56d9 $5f >M $56da $7a >M $56db $5a >M $56dc $9b >M
T{ op PC S A X Y P -> $56dc $ca $d8 $41 $ff $23 }T T{ $007a M $5636 M $56d9 M $56da M $56db M $56dc M -> $e2 $22 $5f $7a $5a $9b }T
$ec29 >PC $15 >S $d3 >A $46 >X $24 >Y $23 >P $00b7 $98 >M $ebb1 $7e >M $ec29 $5f >M $ec2a $b7 >M $ec2b $85 >M $ecb1 $e7 >M
T{ op PC S A X Y P -> $ebb1 $15 $d3 $46 $24 $23 }T T{ $00b7 M $ebb1 M $ec29 M $ec2a M $ec2b M $ecb1 M -> $98 $7e $5f $b7 $85 $e7 }T
$21ac >PC $0c >S $21 >A $4b >X $91 >Y $27 >P $005a $4e >M $2147 $a9 >M $21ac $5f >M $21ad $5a >M $21ae $98 >M
T{ op PC S A X Y P -> $2147 $0c $21 $4b $91 $27 }T T{ $005a M $2147 M $21ac M $21ad M $21ae M -> $4e $a9 $5f $5a $98 }T
$d7cc >PC $dd >S $2a >A $44 >X $c6 >Y $a3 >P $00bf $6a >M $d7cc $5f >M $d7cd $bf >M $d7ce $2d >M $d7cf $46 >M $d7fc $15 >M
T{ op PC S A X Y P -> $d7cf $dd $2a $44 $c6 $a3 }T T{ $00bf M $d7cc M $d7cd M $d7ce M $d7cf M $d7fc M -> $6a $5f $bf $2d $46 $15 }T
$2b81 >PC $e4 >S $45 >A $9a >X $26 >Y $a4 >P $00bd $58 >M $2b81 $5f >M $2b82 $bd >M $2b83 $57 >M $2bdb $69 >M
T{ op PC S A X Y P -> $2bdb $e4 $45 $9a $26 $a4 }T T{ $00bd M $2b81 M $2b82 M $2b83 M $2bdb M -> $58 $5f $bd $57 $69 }T
$447f >PC $31 >S $d6 >A $f8 >X $6c >Y $a6 >P $0075 $d9 >M $447f $5f >M $4480 $75 >M $4481 $5e >M $44e0 $bc >M
T{ op PC S A X Y P -> $44e0 $31 $d6 $f8 $6c $a6 }T T{ $0075 M $447f M $4480 M $4481 M $44e0 M -> $d9 $5f $75 $5e $bc }T
$a95a >PC $4b >S $0e >A $24 >X $4d >Y $20 >P $0055 $34 >M $a95a $5f >M $a95b $55 >M $a95c $69 >M $a95d $ee >M $a9c6 $b9 >M
T{ op PC S A X Y P -> $a95d $4b $0e $24 $4d $20 }T T{ $0055 M $a95a M $a95b M $a95c M $a95d M $a9c6 M -> $34 $5f $55 $69 $ee $b9 }T
$99f2 >PC $20 >S $eb >A $3e >X $aa >Y $a1 >P $00bd $d5 >M $99d3 $d8 >M $99f2 $5f >M $99f3 $bd >M $99f4 $de >M
T{ op PC S A X Y P -> $99d3 $20 $eb $3e $aa $a1 }T T{ $00bd M $99d3 M $99f2 M $99f3 M $99f4 M -> $d5 $d8 $5f $bd $de }T
$0a05 >PC $4d >S $b6 >A $50 >X $d4 >Y $e6 >P $00a5 $cb >M $099c $05 >M $0a05 $5f >M $0a06 $a5 >M $0a07 $94 >M $0a9c $7f >M
T{ op PC S A X Y P -> $099c $4d $b6 $50 $d4 $e6 }T T{ $00a5 M $099c M $0a05 M $0a06 M $0a07 M $0a9c M -> $cb $05 $5f $a5 $94 $7f }T
$0d80 >PC $64 >S $4a >A $7b >X $7c >Y $e7 >P $0075 $48 >M $0d80 $5f >M $0d81 $75 >M $0d82 $35 >M $0db8 $0b >M
T{ op PC S A X Y P -> $0db8 $64 $4a $7b $7c $e7 }T T{ $0075 M $0d80 M $0d81 M $0d82 M $0db8 M -> $48 $5f $75 $35 $0b }T
$536b >PC $e8 >S $4a >A $a5 >X $28 >Y $e0 >P $0083 $58 >M $536b $5f >M $536c $83 >M $536d $09 >M $5377 $ea >M
T{ op PC S A X Y P -> $5377 $e8 $4a $a5 $28 $e0 }T T{ $0083 M $536b M $536c M $536d M $5377 M -> $58 $5f $83 $09 $ea }T
$8c95 >PC $9e >S $b7 >A $28 >X $87 >Y $66 >P $00e5 $4d >M $8c1a $f4 >M $8c95 $5f >M $8c96 $e5 >M $8c97 $82 >M
T{ op PC S A X Y P -> $8c1a $9e $b7 $28 $87 $66 }T T{ $00e5 M $8c1a M $8c95 M $8c96 M $8c97 M -> $4d $f4 $5f $e5 $82 }T
$3c02 >PC $80 >S $7b >A $21 >X $99 >Y $20 >P $0086 $8e >M $3c02 $5f >M $3c03 $86 >M $3c04 $7f >M $3c84 $37 >M
T{ op PC S A X Y P -> $3c84 $80 $7b $21 $99 $20 }T T{ $0086 M $3c02 M $3c03 M $3c04 M $3c84 M -> $8e $5f $86 $7f $37 }T
( 60 )
$cdaf >PC $1f >S $be >A $d4 >X $f1 >Y $e7 >P $011f $49 >M $0120 $62 >M $0121 $64 >M $6462 $89 >M $6463 $59 >M $cdaf $60 >M $cdb0 $81 >M $cdb1 $d7 >M
T{ op PC S A X Y P -> $6463 $21 $be $d4 $f1 $e7 }T T{ $011f M $0120 M $0121 M $6462 M $6463 M $cdaf M $cdb0 M $cdb1 M -> $49 $62 $64 $89 $59 $60 $81 $d7 }T
$8ae7 >PC $44 >S $a9 >A $26 >X $34 >Y $a0 >P $0144 $c9 >M $0145 $4d >M $0146 $5e >M $5e4d $ed >M $5e4e $5a >M $8ae7 $60 >M $8ae8 $c8 >M $8ae9 $fb >M
T{ op PC S A X Y P -> $5e4e $46 $a9 $26 $34 $a0 }T T{ $0144 M $0145 M $0146 M $5e4d M $5e4e M $8ae7 M $8ae8 M $8ae9 M -> $c9 $4d $5e $ed $5a $60 $c8 $fb }T
$f07c >PC $85 >S $f1 >A $2b >X $75 >Y $25 >P $0185 $a4 >M $0186 $a8 >M $0187 $2f >M $2fa8 $14 >M $2fa9 $c8 >M $f07c $60 >M $f07d $4a >M $f07e $6d >M
T{ op PC S A X Y P -> $2fa9 $87 $f1 $2b $75 $25 }T T{ $0185 M $0186 M $0187 M $2fa8 M $2fa9 M $f07c M $f07d M $f07e M -> $a4 $a8 $2f $14 $c8 $60 $4a $6d }T
$f865 >PC $52 >S $24 >A $0f >X $24 >Y $e5 >P $0152 $48 >M $0153 $e0 >M $0154 $d2 >M $d2e0 $06 >M $d2e1 $e0 >M $f865 $60 >M $f866 $46 >M $f867 $e1 >M
T{ op PC S A X Y P -> $d2e1 $54 $24 $0f $24 $e5 }T T{ $0152 M $0153 M $0154 M $d2e0 M $d2e1 M $f865 M $f866 M $f867 M -> $48 $e0 $d2 $06 $e0 $60 $46 $e1 }T
$b8b4 >PC $6d >S $61 >A $57 >X $17 >Y $23 >P $016d $0f >M $016e $32 >M $016f $56 >M $5632 $8c >M $5633 $3a >M $b8b4 $60 >M $b8b5 $b8 >M $b8b6 $12 >M
T{ op PC S A X Y P -> $5633 $6f $61 $57 $17 $23 }T T{ $016d M $016e M $016f M $5632 M $5633 M $b8b4 M $b8b5 M $b8b6 M -> $0f $32 $56 $8c $3a $60 $b8 $12 }T
$bfdc >PC $66 >S $fa >A $48 >X $b8 >Y $e5 >P $0166 $45 >M $0167 $7c >M $0168 $8e >M $8e7c $c4 >M $8e7d $49 >M $bfdc $60 >M $bfdd $02 >M $bfde $2a >M
T{ op PC S A X Y P -> $8e7d $68 $fa $48 $b8 $e5 }T T{ $0166 M $0167 M $0168 M $8e7c M $8e7d M $bfdc M $bfdd M $bfde M -> $45 $7c $8e $c4 $49 $60 $02 $2a }T
$26ab >PC $fe >S $f7 >A $4f >X $20 >Y $20 >P $0100 $b1 >M $01fe $14 >M $01ff $55 >M $26ab $60 >M $26ac $3f >M $26ad $d5 >M $b155 $bb >M $b156 $ac >M
T{ op PC S A X Y P -> $b156 $00 $f7 $4f $20 $20 }T T{ $0100 M $01fe M $01ff M $26ab M $26ac M $26ad M $b155 M $b156 M -> $b1 $14 $55 $60 $3f $d5 $bb $ac }T
$ec1a >PC $55 >S $26 >A $c5 >X $d0 >Y $62 >P $0155 $82 >M $0156 $22 >M $0157 $67 >M $6722 $1e >M $6723 $a6 >M $ec1a $60 >M $ec1b $22 >M $ec1c $a8 >M
T{ op PC S A X Y P -> $6723 $57 $26 $c5 $d0 $62 }T T{ $0155 M $0156 M $0157 M $6722 M $6723 M $ec1a M $ec1b M $ec1c M -> $82 $22 $67 $1e $a6 $60 $22 $a8 }T
$255b >PC $a9 >S $c6 >A $fd >X $b8 >Y $e1 >P $01a9 $02 >M $01aa $c5 >M $01ab $8a >M $255b $60 >M $255c $09 >M $255d $46 >M $8ac5 $6b >M $8ac6 $03 >M
T{ op PC S A X Y P -> $8ac6 $ab $c6 $fd $b8 $e1 }T T{ $01a9 M $01aa M $01ab M $255b M $255c M $255d M $8ac5 M $8ac6 M -> $02 $c5 $8a $60 $09 $46 $6b $03 }T
$2b77 >PC $db >S $bc >A $0c >X $f3 >Y $a2 >P $01db $a3 >M $01dc $0c >M $01dd $56 >M $2b77 $60 >M $2b78 $6c >M $2b79 $0d >M $560c $5c >M $560d $e0 >M
T{ op PC S A X Y P -> $560d $dd $bc $0c $f3 $a2 }T T{ $01db M $01dc M $01dd M $2b77 M $2b78 M $2b79 M $560c M $560d M -> $a3 $0c $56 $60 $6c $0d $5c $e0 }T
$9dbd >PC $e8 >S $b4 >A $1a >X $3a >Y $a7 >P $01e8 $3a >M $01e9 $35 >M $01ea $79 >M $7935 $1c >M $7936 $fc >M $9dbd $60 >M $9dbe $21 >M $9dbf $92 >M
T{ op PC S A X Y P -> $7936 $ea $b4 $1a $3a $a7 }T T{ $01e8 M $01e9 M $01ea M $7935 M $7936 M $9dbd M $9dbe M $9dbf M -> $3a $35 $79 $1c $fc $60 $21 $92 }T
$a8f4 >PC $fc >S $8e >A $35 >X $b1 >Y $e6 >P $01fc $62 >M $01fd $0e >M $01fe $ac >M $a8f4 $60 >M $a8f5 $67 >M $a8f6 $05 >M $ac0e $f3 >M $ac0f $ab >M
T{ op PC S A X Y P -> $ac0f $fe $8e $35 $b1 $e6 }T T{ $01fc M $01fd M $01fe M $a8f4 M $a8f5 M $a8f6 M $ac0e M $ac0f M -> $62 $0e $ac $60 $67 $05 $f3 $ab }T
$2a67 >PC $d0 >S $58 >A $a1 >X $f3 >Y $60 >P $01d0 $e8 >M $01d1 $b2 >M $01d2 $a3 >M $2a67 $60 >M $2a68 $26 >M $2a69 $0f >M $a3b2 $1e >M $a3b3 $31 >M
T{ op PC S A X Y P -> $a3b3 $d2 $58 $a1 $f3 $60 }T T{ $01d0 M $01d1 M $01d2 M $2a67 M $2a68 M $2a69 M $a3b2 M $a3b3 M -> $e8 $b2 $a3 $60 $26 $0f $1e $31 }T
$32d9 >PC $a2 >S $a7 >A $7c >X $e8 >Y $a5 >P $01a2 $68 >M $01a3 $f7 >M $01a4 $e6 >M $32d9 $60 >M $32da $e9 >M $32db $77 >M $e6f7 $62 >M $e6f8 $35 >M
T{ op PC S A X Y P -> $e6f8 $a4 $a7 $7c $e8 $a5 }T T{ $01a2 M $01a3 M $01a4 M $32d9 M $32da M $32db M $e6f7 M $e6f8 M -> $68 $f7 $e6 $60 $e9 $77 $62 $35 }T
$fa41 >PC $38 >S $6f >A $03 >X $bb >Y $a1 >P $0138 $84 >M $0139 $35 >M $013a $40 >M $4035 $6b >M $4036 $4d >M $fa41 $60 >M $fa42 $05 >M $fa43 $a4 >M
T{ op PC S A X Y P -> $4036 $3a $6f $03 $bb $a1 }T T{ $0138 M $0139 M $013a M $4035 M $4036 M $fa41 M $fa42 M $fa43 M -> $84 $35 $40 $6b $4d $60 $05 $a4 }T
$1b78 >PC $d1 >S $24 >A $b0 >X $54 >Y $67 >P $01d1 $f5 >M $01d2 $b1 >M $01d3 $e7 >M $1b78 $60 >M $1b79 $18 >M $1b7a $11 >M $e7b1 $ad >M $e7b2 $47 >M
T{ op PC S A X Y P -> $e7b2 $d3 $24 $b0 $54 $67 }T T{ $01d1 M $01d2 M $01d3 M $1b78 M $1b79 M $1b7a M $e7b1 M $e7b2 M -> $f5 $b1 $e7 $60 $18 $11 $ad $47 }T
( 61 )
$67f2 >PC $66 >S $99 >A $18 >X $37 >Y $63 >P $0017 $f0 >M $002f $0d >M $0030 $52 >M $520d $82 >M $67f2 $61 >M $67f3 $17 >M $67f4 $18 >M
T{ op PC S A X Y P -> $67f4 $66 $1c $18 $37 $61 }T T{ $0017 M $002f M $0030 M $520d M $67f2 M $67f3 M $67f4 M -> $f0 $0d $52 $82 $61 $17 $18 }T
$e220 >PC $81 >S $46 >A $25 >X $58 >Y $64 >P $007f $69 >M $00a4 $23 >M $00a5 $28 >M $2823 $37 >M $e220 $61 >M $e221 $7f >M $e222 $7e >M
T{ op PC S A X Y P -> $e222 $81 $7d $25 $58 $24 }T T{ $007f M $00a4 M $00a5 M $2823 M $e220 M $e221 M $e222 M -> $69 $23 $28 $37 $61 $7f $7e }T
$1183 >PC $7d >S $e1 >A $52 >X $2e >Y $e5 >P $008d $2e >M $00df $f5 >M $00e0 $03 >M $03f5 $86 >M $1183 $61 >M $1184 $8d >M $1185 $c1 >M
T{ op PC S A X Y P -> $1185 $7d $68 $52 $2e $65 }T T{ $008d M $00df M $00e0 M $03f5 M $1183 M $1184 M $1185 M -> $2e $f5 $03 $86 $61 $8d $c1 }T
$ea8b >PC $bf >S $a5 >A $4e >X $70 >Y $a4 >P $001f $ed >M $0020 $55 >M $00d1 $6c >M $55ed $13 >M $ea8b $61 >M $ea8c $d1 >M $ea8d $50 >M
T{ op PC S A X Y P -> $ea8d $bf $b8 $4e $70 $a4 }T T{ $001f M $0020 M $00d1 M $55ed M $ea8b M $ea8c M $ea8d M -> $ed $55 $6c $13 $61 $d1 $50 }T
$71bf >PC $ad >S $90 >A $b6 >X $e5 >Y $62 >P $0071 $66 >M $0072 $38 >M $00bb $3d >M $3866 $c9 >M $71bf $61 >M $71c0 $bb >M $71c1 $97 >M
T{ op PC S A X Y P -> $71c1 $ad $59 $b6 $e5 $61 }T T{ $0071 M $0072 M $00bb M $3866 M $71bf M $71c0 M $71c1 M -> $66 $38 $3d $c9 $61 $bb $97 }T
$a043 >PC $e7 >S $58 >A $8c >X $54 >Y $a3 >P $007b $2f >M $007c $fa >M $00ef $65 >M $a043 $61 >M $a044 $ef >M $a045 $c1 >M $fa2f $db >M
T{ op PC S A X Y P -> $a045 $e7 $34 $8c $54 $21 }T T{ $007b M $007c M $00ef M $a043 M $a044 M $a045 M $fa2f M -> $2f $fa $65 $61 $ef $c1 $db }T
$3e69 >PC $e5 >S $c7 >A $98 >X $04 >Y $22 >P $0071 $e8 >M $0072 $4b >M $00d9 $84 >M $3e69 $61 >M $3e6a $d9 >M $3e6b $0b >M $4be8 $76 >M
T{ op PC S A X Y P -> $3e6b $e5 $3d $98 $04 $21 }T T{ $0071 M $0072 M $00d9 M $3e69 M $3e6a M $3e6b M $4be8 M -> $e8 $4b $84 $61 $d9 $0b $76 }T
$9327 >PC $39 >S $ab >A $c3 >X $3c >Y $27 >P $0043 $5d >M $0044 $ee >M $0080 $24 >M $9327 $61 >M $9328 $80 >M $9329 $d2 >M $ee5d $9b >M
T{ op PC S A X Y P -> $9329 $39 $47 $c3 $3c $65 }T T{ $0043 M $0044 M $0080 M $9327 M $9328 M $9329 M $ee5d M -> $5d $ee $24 $61 $80 $d2 $9b }T
$e032 >PC $7f >S $52 >A $b0 >X $36 >Y $61 >P $0051 $bb >M $0052 $05 >M $00a1 $3b >M $05bb $02 >M $e032 $61 >M $e033 $a1 >M $e034 $98 >M
T{ op PC S A X Y P -> $e034 $7f $55 $b0 $36 $20 }T T{ $0051 M $0052 M $00a1 M $05bb M $e032 M $e033 M $e034 M -> $bb $05 $3b $02 $61 $a1 $98 }T
$cc0b >PC $04 >S $b7 >A $c9 >X $80 >Y $e5 >P $0032 $23 >M $0033 $42 >M $0069 $c7 >M $4223 $ca >M $cc0b $61 >M $cc0c $69 >M $cc0d $96 >M
T{ op PC S A X Y P -> $cc0d $04 $82 $c9 $80 $a5 }T T{ $0032 M $0033 M $0069 M $4223 M $cc0b M $cc0c M $cc0d M -> $23 $42 $c7 $ca $61 $69 $96 }T
$b6e5 >PC $bd >S $5d >A $94 >X $5f >Y $e0 >P $004e $87 >M $004f $a0 >M $00ba $73 >M $a087 $d2 >M $b6e5 $61 >M $b6e6 $ba >M $b6e7 $58 >M
T{ op PC S A X Y P -> $b6e7 $bd $2f $94 $5f $21 }T T{ $004e M $004f M $00ba M $a087 M $b6e5 M $b6e6 M $b6e7 M -> $87 $a0 $73 $d2 $61 $ba $58 }T
$ab99 >PC $8e >S $16 >A $3d >X $5f >Y $e0 >P $0080 $d1 >M $00bd $54 >M $00be $0e >M $0e54 $b5 >M $ab99 $61 >M $ab9a $80 >M $ab9b $20 >M
T{ op PC S A X Y P -> $ab9b $8e $cb $3d $5f $a0 }T T{ $0080 M $00bd M $00be M $0e54 M $ab99 M $ab9a M $ab9b M -> $d1 $54 $0e $b5 $61 $80 $20 }T
$c7cf >PC $bb >S $ff >A $5b >X $c2 >Y $63 >P $0008 $33 >M $0063 $9b >M $0064 $4a >M $4a9b $d9 >M $c7cf $61 >M $c7d0 $08 >M $c7d1 $be >M
T{ op PC S A X Y P -> $c7d1 $bb $d9 $5b $c2 $a1 }T T{ $0008 M $0063 M $0064 M $4a9b M $c7cf M $c7d0 M $c7d1 M -> $33 $9b $4a $d9 $61 $08 $be }T
$e89e >PC $c6 >S $84 >A $92 >X $cd >Y $a4 >P $0064 $70 >M $00f6 $3a >M $00f7 $29 >M $293a $c0 >M $e89e $61 >M $e89f $64 >M $e8a0 $3d >M
T{ op PC S A X Y P -> $e8a0 $c6 $44 $92 $cd $65 }T T{ $0064 M $00f6 M $00f7 M $293a M $e89e M $e89f M $e8a0 M -> $70 $3a $29 $c0 $61 $64 $3d }T
$94c9 >PC $3b >S $64 >A $92 >X $22 >Y $e7 >P $002d $90 >M $002e $60 >M $009b $ef >M $6090 $c2 >M $94c9 $61 >M $94ca $9b >M $94cb $dd >M
T{ op PC S A X Y P -> $94cb $3b $27 $92 $22 $25 }T T{ $002d M $002e M $009b M $6090 M $94c9 M $94ca M $94cb M -> $90 $60 $ef $c2 $61 $9b $dd }T
$c8b5 >PC $f0 >S $49 >A $eb >X $56 >Y $25 >P $0013 $8a >M $0014 $00 >M $0028 $b0 >M $008a $c7 >M $c8b5 $61 >M $c8b6 $28 >M $c8b7 $f7 >M
T{ op PC S A X Y P -> $c8b7 $f0 $11 $eb $56 $25 }T T{ $0013 M $0014 M $0028 M $008a M $c8b5 M $c8b6 M $c8b7 M -> $8a $00 $b0 $c7 $61 $28 $f7 }T
( 62 )
$9fcc >PC $0c >S $05 >A $f4 >X $f1 >Y $65 >P $9fcc $62 >M $9fcd $6e >M $9fce $d5 >M
T{ op PC S A X Y P -> $9fce $0c $05 $f4 $f1 $65 }T T{ $9fcc M $9fcd M $9fce M -> $62 $6e $d5 }T
$cc13 >PC $96 >S $b3 >A $38 >X $1c >Y $60 >P $cc13 $62 >M $cc14 $d9 >M $cc15 $9f >M
T{ op PC S A X Y P -> $cc15 $96 $b3 $38 $1c $60 }T T{ $cc13 M $cc14 M $cc15 M -> $62 $d9 $9f }T
$a202 >PC $56 >S $ae >A $ac >X $7a >Y $24 >P $a202 $62 >M $a203 $f6 >M $a204 $31 >M
T{ op PC S A X Y P -> $a204 $56 $ae $ac $7a $24 }T T{ $a202 M $a203 M $a204 M -> $62 $f6 $31 }T
$bea2 >PC $62 >S $ae >A $c0 >X $32 >Y $a5 >P $bea2 $62 >M $bea3 $73 >M $bea4 $44 >M
T{ op PC S A X Y P -> $bea4 $62 $ae $c0 $32 $a5 }T T{ $bea2 M $bea3 M $bea4 M -> $62 $73 $44 }T
$724d >PC $b8 >S $1d >A $c1 >X $a1 >Y $20 >P $724d $62 >M $724e $cc >M $724f $a9 >M
T{ op PC S A X Y P -> $724f $b8 $1d $c1 $a1 $20 }T T{ $724d M $724e M $724f M -> $62 $cc $a9 }T
$d10f >PC $61 >S $ef >A $f6 >X $b8 >Y $e7 >P $d10f $62 >M $d110 $55 >M $d111 $d9 >M
T{ op PC S A X Y P -> $d111 $61 $ef $f6 $b8 $e7 }T T{ $d10f M $d110 M $d111 M -> $62 $55 $d9 }T
$90e3 >PC $04 >S $c4 >A $7e >X $e3 >Y $61 >P $90e3 $62 >M $90e4 $c3 >M $90e5 $8e >M
T{ op PC S A X Y P -> $90e5 $04 $c4 $7e $e3 $61 }T T{ $90e3 M $90e4 M $90e5 M -> $62 $c3 $8e }T
$9f43 >PC $99 >S $ba >A $6b >X $8a >Y $e0 >P $9f43 $62 >M $9f44 $5a >M $9f45 $02 >M
T{ op PC S A X Y P -> $9f45 $99 $ba $6b $8a $e0 }T T{ $9f43 M $9f44 M $9f45 M -> $62 $5a $02 }T
$320b >PC $4a >S $27 >A $75 >X $38 >Y $a1 >P $320b $62 >M $320c $e7 >M $320d $15 >M
T{ op PC S A X Y P -> $320d $4a $27 $75 $38 $a1 }T T{ $320b M $320c M $320d M -> $62 $e7 $15 }T
$8151 >PC $33 >S $c2 >A $4b >X $8b >Y $e6 >P $8151 $62 >M $8152 $04 >M $8153 $2a >M
T{ op PC S A X Y P -> $8153 $33 $c2 $4b $8b $e6 }T T{ $8151 M $8152 M $8153 M -> $62 $04 $2a }T
$c6b1 >PC $22 >S $05 >A $e1 >X $f4 >Y $24 >P $c6b1 $62 >M $c6b2 $ab >M $c6b3 $e0 >M
T{ op PC S A X Y P -> $c6b3 $22 $05 $e1 $f4 $24 }T T{ $c6b1 M $c6b2 M $c6b3 M -> $62 $ab $e0 }T
$6aae >PC $b7 >S $53 >A $3d >X $a3 >Y $a7 >P $6aae $62 >M $6aaf $bf >M $6ab0 $02 >M
T{ op PC S A X Y P -> $6ab0 $b7 $53 $3d $a3 $a7 }T T{ $6aae M $6aaf M $6ab0 M -> $62 $bf $02 }T
$3e32 >PC $54 >S $9a >A $3a >X $74 >Y $a2 >P $3e32 $62 >M $3e33 $94 >M $3e34 $3c >M
T{ op PC S A X Y P -> $3e34 $54 $9a $3a $74 $a2 }T T{ $3e32 M $3e33 M $3e34 M -> $62 $94 $3c }T
$30dd >PC $f5 >S $2d >A $58 >X $dc >Y $66 >P $30dd $62 >M $30de $06 >M $30df $84 >M
T{ op PC S A X Y P -> $30df $f5 $2d $58 $dc $66 }T T{ $30dd M $30de M $30df M -> $62 $06 $84 }T
$aacf >PC $34 >S $fa >A $82 >X $1d >Y $67 >P $aacf $62 >M $aad0 $43 >M $aad1 $93 >M
T{ op PC S A X Y P -> $aad1 $34 $fa $82 $1d $67 }T T{ $aacf M $aad0 M $aad1 M -> $62 $43 $93 }T
$a373 >PC $f5 >S $0b >A $82 >X $a3 >Y $23 >P $a373 $62 >M $a374 $a2 >M $a375 $b5 >M
T{ op PC S A X Y P -> $a375 $f5 $0b $82 $a3 $23 }T T{ $a373 M $a374 M $a375 M -> $62 $a2 $b5 }T
( 63 )
$b153 >PC $60 >S $28 >A $7a >X $fa >Y $25 >P $b153 $63 >M $b154 $f0 >M $b155 $b9 >M
T{ op PC S A X Y P -> $b154 $60 $28 $7a $fa $25 }T T{ $b153 M $b154 M $b155 M -> $63 $f0 $b9 }T
$049d >PC $96 >S $81 >A $c0 >X $b2 >Y $e3 >P $049d $63 >M $049e $b4 >M $049f $86 >M
T{ op PC S A X Y P -> $049e $96 $81 $c0 $b2 $e3 }T T{ $049d M $049e M $049f M -> $63 $b4 $86 }T
$e7db >PC $6f >S $0b >A $8b >X $76 >Y $a7 >P $e7db $63 >M $e7dc $19 >M $e7dd $02 >M
T{ op PC S A X Y P -> $e7dc $6f $0b $8b $76 $a7 }T T{ $e7db M $e7dc M $e7dd M -> $63 $19 $02 }T
$c30f >PC $19 >S $a1 >A $94 >X $96 >Y $a2 >P $c30f $63 >M $c310 $2c >M $c311 $b7 >M
T{ op PC S A X Y P -> $c310 $19 $a1 $94 $96 $a2 }T T{ $c30f M $c310 M $c311 M -> $63 $2c $b7 }T
$a631 >PC $9b >S $89 >A $8e >X $26 >Y $27 >P $a631 $63 >M $a632 $45 >M $a633 $55 >M
T{ op PC S A X Y P -> $a632 $9b $89 $8e $26 $27 }T T{ $a631 M $a632 M $a633 M -> $63 $45 $55 }T
$f948 >PC $81 >S $28 >A $50 >X $c6 >Y $67 >P $f948 $63 >M $f949 $ea >M $f94a $2d >M
T{ op PC S A X Y P -> $f949 $81 $28 $50 $c6 $67 }T T{ $f948 M $f949 M $f94a M -> $63 $ea $2d }T
$b0aa >PC $f9 >S $c7 >A $87 >X $91 >Y $67 >P $b0aa $63 >M $b0ab $b6 >M $b0ac $b8 >M
T{ op PC S A X Y P -> $b0ab $f9 $c7 $87 $91 $67 }T T{ $b0aa M $b0ab M $b0ac M -> $63 $b6 $b8 }T
$149f >PC $4c >S $d0 >A $11 >X $a4 >Y $a7 >P $149f $63 >M $14a0 $54 >M $14a1 $fe >M
T{ op PC S A X Y P -> $14a0 $4c $d0 $11 $a4 $a7 }T T{ $149f M $14a0 M $14a1 M -> $63 $54 $fe }T
$2beb >PC $9a >S $a6 >A $5a >X $b1 >Y $61 >P $2beb $63 >M $2bec $d8 >M $2bed $c0 >M
T{ op PC S A X Y P -> $2bec $9a $a6 $5a $b1 $61 }T T{ $2beb M $2bec M $2bed M -> $63 $d8 $c0 }T
$ecb3 >PC $de >S $41 >A $79 >X $6b >Y $63 >P $ecb3 $63 >M $ecb4 $66 >M $ecb5 $d0 >M
T{ op PC S A X Y P -> $ecb4 $de $41 $79 $6b $63 }T T{ $ecb3 M $ecb4 M $ecb5 M -> $63 $66 $d0 }T
$edeb >PC $db >S $2e >A $a8 >X $2d >Y $21 >P $edeb $63 >M $edec $7e >M $eded $24 >M
T{ op PC S A X Y P -> $edec $db $2e $a8 $2d $21 }T T{ $edeb M $edec M $eded M -> $63 $7e $24 }T
$971b >PC $24 >S $2e >A $7a >X $35 >Y $e5 >P $971b $63 >M $971c $73 >M $971d $47 >M
T{ op PC S A X Y P -> $971c $24 $2e $7a $35 $e5 }T T{ $971b M $971c M $971d M -> $63 $73 $47 }T
$840e >PC $5c >S $a5 >A $81 >X $70 >Y $23 >P $840e $63 >M $840f $9f >M $8410 $24 >M
T{ op PC S A X Y P -> $840f $5c $a5 $81 $70 $23 }T T{ $840e M $840f M $8410 M -> $63 $9f $24 }T
$e079 >PC $8b >S $6e >A $53 >X $f4 >Y $e7 >P $e079 $63 >M $e07a $70 >M $e07b $36 >M
T{ op PC S A X Y P -> $e07a $8b $6e $53 $f4 $e7 }T T{ $e079 M $e07a M $e07b M -> $63 $70 $36 }T
$ee32 >PC $79 >S $01 >A $bd >X $35 >Y $a3 >P $ee32 $63 >M $ee33 $71 >M $ee34 $0c >M
T{ op PC S A X Y P -> $ee33 $79 $01 $bd $35 $a3 }T T{ $ee32 M $ee33 M $ee34 M -> $63 $71 $0c }T
$ffea >PC $96 >S $8f >A $b0 >X $e8 >Y $67 >P $ffea $63 >M $ffeb $8e >M $ffec $9e >M
T{ op PC S A X Y P -> $ffeb $96 $8f $b0 $e8 $67 }T T{ $ffea M $ffeb M $ffec M -> $63 $8e $9e }T
( 64 )
$63be >PC $28 >S $36 >A $57 >X $00 >Y $20 >P $63be $64 >M $63bf $36 >M $63c0 $b5 >M
T{ op PC S A X Y P -> $63c0 $28 $36 $57 $00 $20 }T T{ $0036 M $63be M $63bf M $63c0 M -> $00 $64 $36 $b5 }T
$3d37 >PC $82 >S $7a >A $fd >X $9b >Y $66 >P $3d37 $64 >M $3d38 $3b >M $3d39 $53 >M
T{ op PC S A X Y P -> $3d39 $82 $7a $fd $9b $66 }T T{ $003b M $3d37 M $3d38 M $3d39 M -> $00 $64 $3b $53 }T
$86da >PC $bb >S $1f >A $c5 >X $b6 >Y $e1 >P $86da $64 >M $86db $bd >M $86dc $4a >M
T{ op PC S A X Y P -> $86dc $bb $1f $c5 $b6 $e1 }T T{ $00bd M $86da M $86db M $86dc M -> $00 $64 $bd $4a }T
$2776 >PC $5d >S $22 >A $f7 >X $10 >Y $e6 >P $2776 $64 >M $2777 $e5 >M $2778 $88 >M
T{ op PC S A X Y P -> $2778 $5d $22 $f7 $10 $e6 }T T{ $00e5 M $2776 M $2777 M $2778 M -> $00 $64 $e5 $88 }T
$1c16 >PC $0a >S $c2 >A $fe >X $a0 >Y $e0 >P $1c16 $64 >M $1c17 $8f >M $1c18 $e5 >M
T{ op PC S A X Y P -> $1c18 $0a $c2 $fe $a0 $e0 }T T{ $008f M $1c16 M $1c17 M $1c18 M -> $00 $64 $8f $e5 }T
$7fa4 >PC $d3 >S $40 >A $3e >X $d3 >Y $e1 >P $7fa4 $64 >M $7fa5 $cf >M $7fa6 $39 >M
T{ op PC S A X Y P -> $7fa6 $d3 $40 $3e $d3 $e1 }T T{ $00cf M $7fa4 M $7fa5 M $7fa6 M -> $00 $64 $cf $39 }T
$2dda >PC $40 >S $32 >A $90 >X $63 >Y $a1 >P $2dda $64 >M $2ddb $0e >M $2ddc $2d >M
T{ op PC S A X Y P -> $2ddc $40 $32 $90 $63 $a1 }T T{ $000e M $2dda M $2ddb M $2ddc M -> $00 $64 $0e $2d }T
$7072 >PC $34 >S $23 >A $bf >X $20 >Y $a2 >P $7072 $64 >M $7073 $39 >M $7074 $e7 >M
T{ op PC S A X Y P -> $7074 $34 $23 $bf $20 $a2 }T T{ $0039 M $7072 M $7073 M $7074 M -> $00 $64 $39 $e7 }T
$71df >PC $39 >S $96 >A $b9 >X $ba >Y $a7 >P $71df $64 >M $71e0 $7d >M $71e1 $f6 >M
T{ op PC S A X Y P -> $71e1 $39 $96 $b9 $ba $a7 }T T{ $007d M $71df M $71e0 M $71e1 M -> $00 $64 $7d $f6 }T
$d087 >PC $6f >S $cc >A $ed >X $c9 >Y $a7 >P $d087 $64 >M $d088 $10 >M $d089 $e9 >M
T{ op PC S A X Y P -> $d089 $6f $cc $ed $c9 $a7 }T T{ $0010 M $d087 M $d088 M $d089 M -> $00 $64 $10 $e9 }T
$0915 >PC $a6 >S $fe >A $47 >X $03 >Y $e0 >P $0915 $64 >M $0916 $dc >M $0917 $0f >M
T{ op PC S A X Y P -> $0917 $a6 $fe $47 $03 $e0 }T T{ $00dc M $0915 M $0916 M $0917 M -> $00 $64 $dc $0f }T
$9b05 >PC $5d >S $f5 >A $83 >X $0f >Y $a4 >P $9b05 $64 >M $9b06 $65 >M $9b07 $be >M
T{ op PC S A X Y P -> $9b07 $5d $f5 $83 $0f $a4 }T T{ $0065 M $9b05 M $9b06 M $9b07 M -> $00 $64 $65 $be }T
$d664 >PC $05 >S $5e >A $f9 >X $56 >Y $e4 >P $d664 $64 >M $d665 $ed >M $d666 $c3 >M
T{ op PC S A X Y P -> $d666 $05 $5e $f9 $56 $e4 }T T{ $00ed M $d664 M $d665 M $d666 M -> $00 $64 $ed $c3 }T
$a31b >PC $13 >S $ab >A $1d >X $56 >Y $e6 >P $a31b $64 >M $a31c $71 >M $a31d $7a >M
T{ op PC S A X Y P -> $a31d $13 $ab $1d $56 $e6 }T T{ $0071 M $a31b M $a31c M $a31d M -> $00 $64 $71 $7a }T
$a9a4 >PC $29 >S $ee >A $63 >X $0a >Y $60 >P $a9a4 $64 >M $a9a5 $b9 >M $a9a6 $12 >M
T{ op PC S A X Y P -> $a9a6 $29 $ee $63 $0a $60 }T T{ $00b9 M $a9a4 M $a9a5 M $a9a6 M -> $00 $64 $b9 $12 }T
$d6cd >PC $f8 >S $62 >A $e5 >X $a4 >Y $63 >P $d6cd $64 >M $d6ce $62 >M $d6cf $39 >M
T{ op PC S A X Y P -> $d6cf $f8 $62 $e5 $a4 $63 }T T{ $0062 M $d6cd M $d6ce M $d6cf M -> $00 $64 $62 $39 }T
( 65 )
$4032 >PC $b8 >S $71 >A $c5 >X $bd >Y $67 >P $00fe $8c >M $4032 $65 >M $4033 $fe >M $4034 $cd >M
T{ op PC S A X Y P -> $4034 $b8 $fe $c5 $bd $a4 }T T{ $00fe M $4032 M $4033 M $4034 M -> $8c $65 $fe $cd }T
$3d85 >PC $d3 >S $a0 >A $a5 >X $d3 >Y $21 >P $00aa $73 >M $3d85 $65 >M $3d86 $aa >M $3d87 $94 >M
T{ op PC S A X Y P -> $3d87 $d3 $14 $a5 $d3 $21 }T T{ $00aa M $3d85 M $3d86 M $3d87 M -> $73 $65 $aa $94 }T
$1a3c >PC $f7 >S $ad >A $04 >X $79 >Y $a3 >P $00e0 $79 >M $1a3c $65 >M $1a3d $e0 >M $1a3e $af >M
T{ op PC S A X Y P -> $1a3e $f7 $27 $04 $79 $21 }T T{ $00e0 M $1a3c M $1a3d M $1a3e M -> $79 $65 $e0 $af }T
$9b0f >PC $e3 >S $0e >A $5b >X $91 >Y $62 >P $005f $df >M $9b0f $65 >M $9b10 $5f >M $9b11 $26 >M
T{ op PC S A X Y P -> $9b11 $e3 $ed $5b $91 $a0 }T T{ $005f M $9b0f M $9b10 M $9b11 M -> $df $65 $5f $26 }T
$3d3a >PC $de >S $5c >A $ea >X $d7 >Y $21 >P $0088 $15 >M $3d3a $65 >M $3d3b $88 >M $3d3c $17 >M
T{ op PC S A X Y P -> $3d3c $de $72 $ea $d7 $20 }T T{ $0088 M $3d3a M $3d3b M $3d3c M -> $15 $65 $88 $17 }T
$a961 >PC $82 >S $a9 >A $a5 >X $c0 >Y $21 >P $00c1 $93 >M $a961 $65 >M $a962 $c1 >M $a963 $79 >M
T{ op PC S A X Y P -> $a963 $82 $3d $a5 $c0 $61 }T T{ $00c1 M $a961 M $a962 M $a963 M -> $93 $65 $c1 $79 }T
$657f >PC $63 >S $ce >A $4d >X $16 >Y $e7 >P $00de $d8 >M $657f $65 >M $6580 $de >M $6581 $de >M
T{ op PC S A X Y P -> $6581 $63 $a7 $4d $16 $a5 }T T{ $00de M $657f M $6580 M $6581 M -> $d8 $65 $de $de }T
$851b >PC $11 >S $96 >A $3f >X $02 >Y $25 >P $0067 $f5 >M $851b $65 >M $851c $67 >M $851d $f3 >M
T{ op PC S A X Y P -> $851d $11 $8c $3f $02 $a5 }T T{ $0067 M $851b M $851c M $851d M -> $f5 $65 $67 $f3 }T
$7cac >PC $53 >S $f8 >A $7b >X $e2 >Y $22 >P $002b $25 >M $7cac $65 >M $7cad $2b >M $7cae $9c >M
T{ op PC S A X Y P -> $7cae $53 $1d $7b $e2 $21 }T T{ $002b M $7cac M $7cad M $7cae M -> $25 $65 $2b $9c }T
$d4cc >PC $b6 >S $5a >A $fe >X $1e >Y $22 >P $00e1 $4d >M $d4cc $65 >M $d4cd $e1 >M $d4ce $17 >M
T{ op PC S A X Y P -> $d4ce $b6 $a7 $fe $1e $e0 }T T{ $00e1 M $d4cc M $d4cd M $d4ce M -> $4d $65 $e1 $17 }T
$ca9b >PC $7a >S $49 >A $4c >X $c1 >Y $20 >P $00b2 $a9 >M $ca9b $65 >M $ca9c $b2 >M $ca9d $22 >M
T{ op PC S A X Y P -> $ca9d $7a $f2 $4c $c1 $a0 }T T{ $00b2 M $ca9b M $ca9c M $ca9d M -> $a9 $65 $b2 $22 }T
$19df >PC $e1 >S $87 >A $9c >X $94 >Y $21 >P $0016 $ab >M $19df $65 >M $19e0 $16 >M $19e1 $6b >M
T{ op PC S A X Y P -> $19e1 $e1 $33 $9c $94 $61 }T T{ $0016 M $19df M $19e0 M $19e1 M -> $ab $65 $16 $6b }T
$d091 >PC $e7 >S $67 >A $d9 >X $66 >Y $66 >P $001d $5b >M $d091 $65 >M $d092 $1d >M $d093 $7d >M
T{ op PC S A X Y P -> $d093 $e7 $c2 $d9 $66 $e4 }T T{ $001d M $d091 M $d092 M $d093 M -> $5b $65 $1d $7d }T
$0515 >PC $27 >S $61 >A $cc >X $5d >Y $27 >P $002e $3f >M $0515 $65 >M $0516 $2e >M $0517 $18 >M
T{ op PC S A X Y P -> $0517 $27 $a1 $cc $5d $e4 }T T{ $002e M $0515 M $0516 M $0517 M -> $3f $65 $2e $18 }T
$6cd8 >PC $ac >S $da >A $ae >X $1a >Y $64 >P $00dc $39 >M $6cd8 $65 >M $6cd9 $dc >M $6cda $98 >M
T{ op PC S A X Y P -> $6cda $ac $13 $ae $1a $25 }T T{ $00dc M $6cd8 M $6cd9 M $6cda M -> $39 $65 $dc $98 }T
$b1a1 >PC $b1 >S $9c >A $77 >X $0d >Y $62 >P $000e $d6 >M $b1a1 $65 >M $b1a2 $0e >M $b1a3 $58 >M
T{ op PC S A X Y P -> $b1a3 $b1 $72 $77 $0d $61 }T T{ $000e M $b1a1 M $b1a2 M $b1a3 M -> $d6 $65 $0e $58 }T
( 66 )
$e7e4 >PC $32 >S $a3 >A $6e >X $ee >Y $a0 >P $00fa $2d >M $e7e4 $66 >M $e7e5 $fa >M $e7e6 $e0 >M
T{ op PC S A X Y P -> $e7e6 $32 $a3 $6e $ee $21 }T T{ $00fa M $e7e4 M $e7e5 M $e7e6 M -> $16 $66 $fa $e0 }T
$108e >PC $ec >S $53 >A $7d >X $1d >Y $a4 >P $00e6 $7c >M $108e $66 >M $108f $e6 >M $1090 $c2 >M
T{ op PC S A X Y P -> $1090 $ec $53 $7d $1d $24 }T T{ $00e6 M $108e M $108f M $1090 M -> $3e $66 $e6 $c2 }T
$9126 >PC $73 >S $9b >A $a0 >X $54 >Y $21 >P $00b0 $1d >M $9126 $66 >M $9127 $b0 >M $9128 $17 >M
T{ op PC S A X Y P -> $9128 $73 $9b $a0 $54 $a1 }T T{ $00b0 M $9126 M $9127 M $9128 M -> $8e $66 $b0 $17 }T
$1730 >PC $ce >S $ab >A $5f >X $4f >Y $e6 >P $004e $bd >M $1730 $66 >M $1731 $4e >M $1732 $20 >M
T{ op PC S A X Y P -> $1732 $ce $ab $5f $4f $65 }T T{ $004e M $1730 M $1731 M $1732 M -> $5e $66 $4e $20 }T
$6ea2 >PC $90 >S $a5 >A $41 >X $9a >Y $27 >P $0043 $9f >M $6ea2 $66 >M $6ea3 $43 >M $6ea4 $0d >M
T{ op PC S A X Y P -> $6ea4 $90 $a5 $41 $9a $a5 }T T{ $0043 M $6ea2 M $6ea3 M $6ea4 M -> $cf $66 $43 $0d }T
$1395 >PC $70 >S $dc >A $66 >X $db >Y $63 >P $00f0 $90 >M $1395 $66 >M $1396 $f0 >M $1397 $fd >M
T{ op PC S A X Y P -> $1397 $70 $dc $66 $db $e0 }T T{ $00f0 M $1395 M $1396 M $1397 M -> $c8 $66 $f0 $fd }T
$f1eb >PC $b0 >S $3d >A $94 >X $d8 >Y $a3 >P $007d $a6 >M $f1eb $66 >M $f1ec $7d >M $f1ed $40 >M
T{ op PC S A X Y P -> $f1ed $b0 $3d $94 $d8 $a0 }T T{ $007d M $f1eb M $f1ec M $f1ed M -> $d3 $66 $7d $40 }T
$72f7 >PC $2c >S $7e >A $77 >X $40 >Y $20 >P $00e6 $16 >M $72f7 $66 >M $72f8 $e6 >M $72f9 $3d >M
T{ op PC S A X Y P -> $72f9 $2c $7e $77 $40 $20 }T T{ $00e6 M $72f7 M $72f8 M $72f9 M -> $0b $66 $e6 $3d }T
$0ee0 >PC $47 >S $52 >A $f5 >X $40 >Y $21 >P $0042 $fa >M $0ee0 $66 >M $0ee1 $42 >M $0ee2 $a5 >M
T{ op PC S A X Y P -> $0ee2 $47 $52 $f5 $40 $a0 }T T{ $0042 M $0ee0 M $0ee1 M $0ee2 M -> $fd $66 $42 $a5 }T
$d651 >PC $82 >S $96 >A $0b >X $31 >Y $a6 >P $006a $03 >M $d651 $66 >M $d652 $6a >M $d653 $74 >M
T{ op PC S A X Y P -> $d653 $82 $96 $0b $31 $25 }T T{ $006a M $d651 M $d652 M $d653 M -> $01 $66 $6a $74 }T
$f707 >PC $57 >S $a7 >A $84 >X $2c >Y $27 >P $001c $84 >M $f707 $66 >M $f708 $1c >M $f709 $98 >M
T{ op PC S A X Y P -> $f709 $57 $a7 $84 $2c $a4 }T T{ $001c M $f707 M $f708 M $f709 M -> $c2 $66 $1c $98 }T
$9219 >PC $0d >S $2b >A $75 >X $bc >Y $22 >P $004d $59 >M $9219 $66 >M $921a $4d >M $921b $3b >M
T{ op PC S A X Y P -> $921b $0d $2b $75 $bc $21 }T T{ $004d M $9219 M $921a M $921b M -> $2c $66 $4d $3b }T
$f5ff >PC $ce >S $42 >A $b3 >X $7f >Y $e7 >P $0012 $34 >M $f5ff $66 >M $f600 $12 >M $f601 $18 >M
T{ op PC S A X Y P -> $f601 $ce $42 $b3 $7f $e4 }T T{ $0012 M $f5ff M $f600 M $f601 M -> $9a $66 $12 $18 }T
$78a7 >PC $14 >S $76 >A $64 >X $0d >Y $e2 >P $0072 $8d >M $78a7 $66 >M $78a8 $72 >M $78a9 $58 >M
T{ op PC S A X Y P -> $78a9 $14 $76 $64 $0d $61 }T T{ $0072 M $78a7 M $78a8 M $78a9 M -> $46 $66 $72 $58 }T
$ffae >PC $e3 >S $7b >A $3c >X $e9 >Y $a2 >P $0095 $f0 >M $ffae $66 >M $ffaf $95 >M $ffb0 $10 >M
T{ op PC S A X Y P -> $ffb0 $e3 $7b $3c $e9 $20 }T T{ $0095 M $ffae M $ffaf M $ffb0 M -> $78 $66 $95 $10 }T
$94a3 >PC $2b >S $cf >A $b8 >X $d2 >Y $63 >P $00b9 $82 >M $94a3 $66 >M $94a4 $b9 >M $94a5 $7c >M
T{ op PC S A X Y P -> $94a5 $2b $cf $b8 $d2 $e0 }T T{ $00b9 M $94a3 M $94a4 M $94a5 M -> $c1 $66 $b9 $7c }T
( 67 )
$b639 >PC $37 >S $8d >A $47 >X $61 >Y $a4 >P $00a6 $54 >M $b639 $67 >M $b63a $a6 >M $b63b $c8 >M
T{ op PC S A X Y P -> $b63b $37 $8d $47 $61 $a4 }T T{ $00a6 M $b639 M $b63a M $b63b M -> $14 $67 $a6 $c8 }T
$eaeb >PC $9d >S $96 >A $24 >X $6c >Y $e3 >P $0026 $b8 >M $eaeb $67 >M $eaec $26 >M $eaed $c1 >M
T{ op PC S A X Y P -> $eaed $9d $96 $24 $6c $e3 }T T{ $0026 M $eaeb M $eaec M $eaed M -> $b8 $67 $26 $c1 }T
$8491 >PC $74 >S $68 >A $5c >X $a5 >Y $62 >P $0025 $7e >M $8491 $67 >M $8492 $25 >M $8493 $59 >M
T{ op PC S A X Y P -> $8493 $74 $68 $5c $a5 $62 }T T{ $0025 M $8491 M $8492 M $8493 M -> $3e $67 $25 $59 }T
$c6d3 >PC $86 >S $3d >A $ea >X $10 >Y $e5 >P $00e4 $71 >M $c6d3 $67 >M $c6d4 $e4 >M $c6d5 $e8 >M
T{ op PC S A X Y P -> $c6d5 $86 $3d $ea $10 $e5 }T T{ $00e4 M $c6d3 M $c6d4 M $c6d5 M -> $31 $67 $e4 $e8 }T
$d04a >PC $54 >S $ec >A $f5 >X $67 >Y $64 >P $002d $c1 >M $d04a $67 >M $d04b $2d >M $d04c $17 >M
T{ op PC S A X Y P -> $d04c $54 $ec $f5 $67 $64 }T T{ $002d M $d04a M $d04b M $d04c M -> $81 $67 $2d $17 }T
$b50a >PC $8c >S $97 >A $bb >X $a3 >Y $a0 >P $0035 $8d >M $b50a $67 >M $b50b $35 >M $b50c $86 >M
T{ op PC S A X Y P -> $b50c $8c $97 $bb $a3 $a0 }T T{ $0035 M $b50a M $b50b M $b50c M -> $8d $67 $35 $86 }T
$99be >PC $6d >S $f9 >A $8d >X $c1 >Y $24 >P $00ca $2a >M $99be $67 >M $99bf $ca >M $99c0 $94 >M
T{ op PC S A X Y P -> $99c0 $6d $f9 $8d $c1 $24 }T T{ $00ca M $99be M $99bf M $99c0 M -> $2a $67 $ca $94 }T
$94ee >PC $93 >S $a5 >A $30 >X $cc >Y $e5 >P $0074 $0c >M $94ee $67 >M $94ef $74 >M $94f0 $9d >M
T{ op PC S A X Y P -> $94f0 $93 $a5 $30 $cc $e5 }T T{ $0074 M $94ee M $94ef M $94f0 M -> $0c $67 $74 $9d }T
$d198 >PC $0d >S $16 >A $82 >X $26 >Y $63 >P $0093 $b2 >M $d198 $67 >M $d199 $93 >M $d19a $59 >M
T{ op PC S A X Y P -> $d19a $0d $16 $82 $26 $63 }T T{ $0093 M $d198 M $d199 M $d19a M -> $b2 $67 $93 $59 }T
$59d8 >PC $e5 >S $5e >A $d0 >X $50 >Y $20 >P $006d $94 >M $59d8 $67 >M $59d9 $6d >M $59da $2c >M
T{ op PC S A X Y P -> $59da $e5 $5e $d0 $50 $20 }T T{ $006d M $59d8 M $59d9 M $59da M -> $94 $67 $6d $2c }T
$2914 >PC $27 >S $a7 >A $2c >X $b9 >Y $23 >P $0056 $3b >M $2914 $67 >M $2915 $56 >M $2916 $8d >M
T{ op PC S A X Y P -> $2916 $27 $a7 $2c $b9 $23 }T T{ $0056 M $2914 M $2915 M $2916 M -> $3b $67 $56 $8d }T
$56c4 >PC $9d >S $82 >A $33 >X $58 >Y $67 >P $008c $fd >M $56c4 $67 >M $56c5 $8c >M $56c6 $07 >M
T{ op PC S A X Y P -> $56c6 $9d $82 $33 $58 $67 }T T{ $008c M $56c4 M $56c5 M $56c6 M -> $bd $67 $8c $07 }T
$0de5 >PC $72 >S $10 >A $59 >X $89 >Y $61 >P $00c4 $3b >M $0de5 $67 >M $0de6 $c4 >M $0de7 $37 >M
T{ op PC S A X Y P -> $0de7 $72 $10 $59 $89 $61 }T T{ $00c4 M $0de5 M $0de6 M $0de7 M -> $3b $67 $c4 $37 }T
$994d >PC $be >S $c9 >A $cd >X $22 >Y $a1 >P $0006 $7b >M $994d $67 >M $994e $06 >M $994f $ee >M
T{ op PC S A X Y P -> $994f $be $c9 $cd $22 $a1 }T T{ $0006 M $994d M $994e M $994f M -> $3b $67 $06 $ee }T
$7e62 >PC $2a >S $63 >A $b2 >X $f3 >Y $a1 >P $003a $fb >M $7e62 $67 >M $7e63 $3a >M $7e64 $24 >M
T{ op PC S A X Y P -> $7e64 $2a $63 $b2 $f3 $a1 }T T{ $003a M $7e62 M $7e63 M $7e64 M -> $bb $67 $3a $24 }T
$f04a >PC $9b >S $5c >A $43 >X $4e >Y $e6 >P $0035 $36 >M $f04a $67 >M $f04b $35 >M $f04c $45 >M
T{ op PC S A X Y P -> $f04c $9b $5c $43 $4e $e6 }T T{ $0035 M $f04a M $f04b M $f04c M -> $36 $67 $35 $45 }T
( 68 )
$d020 >PC $f2 >S $31 >A $18 >X $1e >Y $26 >P $01f2 $77 >M $01f3 $81 >M $d020 $68 >M $d021 $34 >M $d022 $ff >M
T{ op PC S A X Y P -> $d021 $f3 $81 $18 $1e $a4 }T T{ $01f2 M $01f3 M $d020 M $d021 M $d022 M -> $77 $81 $68 $34 $ff }T
$e2a3 >PC $a0 >S $b5 >A $ea >X $bb >Y $24 >P $01a0 $8d >M $01a1 $16 >M $e2a3 $68 >M $e2a4 $8d >M $e2a5 $f5 >M
T{ op PC S A X Y P -> $e2a4 $a1 $16 $ea $bb $24 }T T{ $01a0 M $01a1 M $e2a3 M $e2a4 M $e2a5 M -> $8d $16 $68 $8d $f5 }T
$aaf6 >PC $5b >S $d1 >A $cf >X $6b >Y $a3 >P $015b $2b >M $015c $ec >M $aaf6 $68 >M $aaf7 $36 >M $aaf8 $ab >M
T{ op PC S A X Y P -> $aaf7 $5c $ec $cf $6b $a1 }T T{ $015b M $015c M $aaf6 M $aaf7 M $aaf8 M -> $2b $ec $68 $36 $ab }T
$75c3 >PC $bb >S $96 >A $29 >X $a6 >Y $e6 >P $01bb $67 >M $01bc $2c >M $75c3 $68 >M $75c4 $dc >M $75c5 $6d >M
T{ op PC S A X Y P -> $75c4 $bc $2c $29 $a6 $64 }T T{ $01bb M $01bc M $75c3 M $75c4 M $75c5 M -> $67 $2c $68 $dc $6d }T
$504f >PC $78 >S $7a >A $24 >X $53 >Y $e5 >P $0178 $3d >M $0179 $4d >M $504f $68 >M $5050 $43 >M $5051 $8b >M
T{ op PC S A X Y P -> $5050 $79 $4d $24 $53 $65 }T T{ $0178 M $0179 M $504f M $5050 M $5051 M -> $3d $4d $68 $43 $8b }T
$eff3 >PC $83 >S $58 >A $08 >X $8b >Y $27 >P $0183 $03 >M $0184 $4b >M $eff3 $68 >M $eff4 $81 >M $eff5 $43 >M
T{ op PC S A X Y P -> $eff4 $84 $4b $08 $8b $25 }T T{ $0183 M $0184 M $eff3 M $eff4 M $eff5 M -> $03 $4b $68 $81 $43 }T
$7f6d >PC $bf >S $11 >A $6e >X $b5 >Y $65 >P $01bf $4f >M $01c0 $35 >M $7f6d $68 >M $7f6e $aa >M $7f6f $a9 >M
T{ op PC S A X Y P -> $7f6e $c0 $35 $6e $b5 $65 }T T{ $01bf M $01c0 M $7f6d M $7f6e M $7f6f M -> $4f $35 $68 $aa $a9 }T
$e7d2 >PC $6b >S $74 >A $72 >X $d9 >Y $20 >P $016b $29 >M $016c $8f >M $e7d2 $68 >M $e7d3 $f0 >M $e7d4 $29 >M
T{ op PC S A X Y P -> $e7d3 $6c $8f $72 $d9 $a0 }T T{ $016b M $016c M $e7d2 M $e7d3 M $e7d4 M -> $29 $8f $68 $f0 $29 }T
$596f >PC $9c >S $ef >A $70 >X $e5 >Y $61 >P $019c $8c >M $019d $e3 >M $596f $68 >M $5970 $89 >M $5971 $3d >M
T{ op PC S A X Y P -> $5970 $9d $e3 $70 $e5 $e1 }T T{ $019c M $019d M $596f M $5970 M $5971 M -> $8c $e3 $68 $89 $3d }T
$1fa9 >PC $27 >S $d2 >A $e0 >X $3a >Y $24 >P $0127 $79 >M $0128 $9f >M $1fa9 $68 >M $1faa $7e >M $1fab $af >M
T{ op PC S A X Y P -> $1faa $28 $9f $e0 $3a $a4 }T T{ $0127 M $0128 M $1fa9 M $1faa M $1fab M -> $79 $9f $68 $7e $af }T
$edb0 >PC $e1 >S $fc >A $af >X $d2 >Y $a7 >P $01e1 $f1 >M $01e2 $aa >M $edb0 $68 >M $edb1 $18 >M $edb2 $d4 >M
T{ op PC S A X Y P -> $edb1 $e2 $aa $af $d2 $a5 }T T{ $01e1 M $01e2 M $edb0 M $edb1 M $edb2 M -> $f1 $aa $68 $18 $d4 }T
$ed58 >PC $6d >S $ea >A $35 >X $2e >Y $63 >P $016d $42 >M $016e $05 >M $ed58 $68 >M $ed59 $36 >M $ed5a $b8 >M
T{ op PC S A X Y P -> $ed59 $6e $05 $35 $2e $61 }T T{ $016d M $016e M $ed58 M $ed59 M $ed5a M -> $42 $05 $68 $36 $b8 }T
$ee20 >PC $52 >S $3e >A $fa >X $23 >Y $25 >P $0152 $90 >M $0153 $d8 >M $ee20 $68 >M $ee21 $cb >M $ee22 $da >M
T{ op PC S A X Y P -> $ee21 $53 $d8 $fa $23 $a5 }T T{ $0152 M $0153 M $ee20 M $ee21 M $ee22 M -> $90 $d8 $68 $cb $da }T
$6df5 >PC $50 >S $1d >A $41 >X $14 >Y $a1 >P $0150 $eb >M $0151 $4c >M $6df5 $68 >M $6df6 $32 >M $6df7 $55 >M
T{ op PC S A X Y P -> $6df6 $51 $4c $41 $14 $21 }T T{ $0150 M $0151 M $6df5 M $6df6 M $6df7 M -> $eb $4c $68 $32 $55 }T
$e7a8 >PC $f8 >S $d2 >A $57 >X $d5 >Y $e4 >P $01f8 $31 >M $01f9 $c9 >M $e7a8 $68 >M $e7a9 $81 >M $e7aa $5b >M
T{ op PC S A X Y P -> $e7a9 $f9 $c9 $57 $d5 $e4 }T T{ $01f8 M $01f9 M $e7a8 M $e7a9 M $e7aa M -> $31 $c9 $68 $81 $5b }T
$3509 >PC $38 >S $fd >A $86 >X $2c >Y $e7 >P $0138 $1b >M $0139 $92 >M $3509 $68 >M $350a $e3 >M $350b $62 >M
T{ op PC S A X Y P -> $350a $39 $92 $86 $2c $e5 }T T{ $0138 M $0139 M $3509 M $350a M $350b M -> $1b $92 $68 $e3 $62 }T
( 69 )
$0f42 >PC $43 >S $af >A $4d >X $5e >Y $26 >P $0f42 $69 >M $0f43 $53 >M $0f44 $e0 >M
T{ op PC S A X Y P -> $0f44 $43 $02 $4d $5e $25 }T T{ $0f42 M $0f43 M $0f44 M -> $69 $53 $e0 }T
$a30e >PC $63 >S $15 >A $29 >X $51 >Y $e6 >P $a30e $69 >M $a30f $0f >M $a310 $31 >M
T{ op PC S A X Y P -> $a310 $63 $24 $29 $51 $24 }T T{ $a30e M $a30f M $a310 M -> $69 $0f $31 }T
$88ad >PC $6a >S $3c >A $98 >X $fb >Y $67 >P $88ad $69 >M $88ae $53 >M $88af $c6 >M
T{ op PC S A X Y P -> $88af $6a $90 $98 $fb $e4 }T T{ $88ad M $88ae M $88af M -> $69 $53 $c6 }T
$c5d3 >PC $be >S $52 >A $e5 >X $4f >Y $65 >P $c5d3 $69 >M $c5d4 $51 >M $c5d5 $6d >M
T{ op PC S A X Y P -> $c5d5 $be $a4 $e5 $4f $e4 }T T{ $c5d3 M $c5d4 M $c5d5 M -> $69 $51 $6d }T
$b098 >PC $ea >S $aa >A $04 >X $11 >Y $61 >P $b098 $69 >M $b099 $56 >M $b09a $c1 >M
T{ op PC S A X Y P -> $b09a $ea $01 $04 $11 $21 }T T{ $b098 M $b099 M $b09a M -> $69 $56 $c1 }T
$4cc6 >PC $8a >S $f2 >A $5f >X $81 >Y $65 >P $4cc6 $69 >M $4cc7 $5f >M $4cc8 $f3 >M
T{ op PC S A X Y P -> $4cc8 $8a $52 $5f $81 $25 }T T{ $4cc6 M $4cc7 M $4cc8 M -> $69 $5f $f3 }T
$b8ce >PC $bd >S $6e >A $b1 >X $54 >Y $e3 >P $b8ce $69 >M $b8cf $5d >M $b8d0 $97 >M
T{ op PC S A X Y P -> $b8d0 $bd $cc $b1 $54 $e0 }T T{ $b8ce M $b8cf M $b8d0 M -> $69 $5d $97 }T
$299a >PC $b9 >S $35 >A $9e >X $c1 >Y $61 >P $299a $69 >M $299b $02 >M $299c $74 >M
T{ op PC S A X Y P -> $299c $b9 $38 $9e $c1 $20 }T T{ $299a M $299b M $299c M -> $69 $02 $74 }T
$b8cc >PC $6a >S $fc >A $15 >X $3f >Y $e6 >P $b8cc $69 >M $b8cd $85 >M $b8ce $41 >M
T{ op PC S A X Y P -> $b8ce $6a $81 $15 $3f $a5 }T T{ $b8cc M $b8cd M $b8ce M -> $69 $85 $41 }T
$a77c >PC $10 >S $ba >A $c7 >X $6d >Y $e2 >P $a77c $69 >M $a77d $fb >M $a77e $ab >M
T{ op PC S A X Y P -> $a77e $10 $b5 $c7 $6d $a1 }T T{ $a77c M $a77d M $a77e M -> $69 $fb $ab }T
$41ca >PC $d0 >S $e3 >A $ef >X $db >Y $e4 >P $41ca $69 >M $41cb $15 >M $41cc $88 >M
T{ op PC S A X Y P -> $41cc $d0 $f8 $ef $db $a4 }T T{ $41ca M $41cb M $41cc M -> $69 $15 $88 }T
$e430 >PC $56 >S $1b >A $e2 >X $1a >Y $21 >P $e430 $69 >M $e431 $90 >M $e432 $97 >M
T{ op PC S A X Y P -> $e432 $56 $ac $e2 $1a $a0 }T T{ $e430 M $e431 M $e432 M -> $69 $90 $97 }T
$86c0 >PC $18 >S $f7 >A $fc >X $7d >Y $20 >P $86c0 $69 >M $86c1 $c2 >M $86c2 $7c >M
T{ op PC S A X Y P -> $86c2 $18 $b9 $fc $7d $a1 }T T{ $86c0 M $86c1 M $86c2 M -> $69 $c2 $7c }T
$3340 >PC $78 >S $86 >A $19 >X $58 >Y $a2 >P $3340 $69 >M $3341 $ce >M $3342 $80 >M
T{ op PC S A X Y P -> $3342 $78 $54 $19 $58 $61 }T T{ $3340 M $3341 M $3342 M -> $69 $ce $80 }T
$96e7 >PC $a4 >S $b9 >A $31 >X $9f >Y $60 >P $96e7 $69 >M $96e8 $e9 >M $96e9 $c5 >M
T{ op PC S A X Y P -> $96e9 $a4 $a2 $31 $9f $a1 }T T{ $96e7 M $96e8 M $96e9 M -> $69 $e9 $c5 }T
$1e8d >PC $57 >S $8d >A $68 >X $fe >Y $20 >P $1e8d $69 >M $1e8e $99 >M $1e8f $c4 >M
T{ op PC S A X Y P -> $1e8f $57 $26 $68 $fe $61 }T T{ $1e8d M $1e8e M $1e8f M -> $69 $99 $c4 }T
( 6a )
$647a >PC $91 >S $1b >A $07 >X $89 >Y $66 >P $647a $6a >M $647b $df >M $647c $4d >M
T{ op PC S A X Y P -> $647b $91 $0d $07 $89 $65 }T T{ $647a M $647b M $647c M -> $6a $df $4d }T
$a5c9 >PC $29 >S $4b >A $d7 >X $e7 >Y $a1 >P $a5c9 $6a >M $a5ca $df >M $a5cb $ba >M
T{ op PC S A X Y P -> $a5ca $29 $a5 $d7 $e7 $a1 }T T{ $a5c9 M $a5ca M $a5cb M -> $6a $df $ba }T
$42d6 >PC $ca >S $52 >A $a4 >X $a3 >Y $61 >P $42d6 $6a >M $42d7 $f2 >M $42d8 $8d >M
T{ op PC S A X Y P -> $42d7 $ca $a9 $a4 $a3 $e0 }T T{ $42d6 M $42d7 M $42d8 M -> $6a $f2 $8d }T
$be5e >PC $64 >S $71 >A $80 >X $ab >Y $a6 >P $be5e $6a >M $be5f $7f >M $be60 $f4 >M
T{ op PC S A X Y P -> $be5f $64 $38 $80 $ab $25 }T T{ $be5e M $be5f M $be60 M -> $6a $7f $f4 }T
$c6ad >PC $70 >S $50 >A $05 >X $c4 >Y $a3 >P $c6ad $6a >M $c6ae $05 >M $c6af $a3 >M
T{ op PC S A X Y P -> $c6ae $70 $a8 $05 $c4 $a0 }T T{ $c6ad M $c6ae M $c6af M -> $6a $05 $a3 }T
$0e35 >PC $fc >S $64 >A $ad >X $dc >Y $a3 >P $0e35 $6a >M $0e36 $9a >M $0e37 $be >M
T{ op PC S A X Y P -> $0e36 $fc $b2 $ad $dc $a0 }T T{ $0e35 M $0e36 M $0e37 M -> $6a $9a $be }T
$1b14 >PC $02 >S $e4 >A $95 >X $4d >Y $27 >P $1b14 $6a >M $1b15 $50 >M $1b16 $e4 >M
T{ op PC S A X Y P -> $1b15 $02 $f2 $95 $4d $a4 }T T{ $1b14 M $1b15 M $1b16 M -> $6a $50 $e4 }T
$f60b >PC $08 >S $1d >A $4c >X $bb >Y $24 >P $f60b $6a >M $f60c $20 >M $f60d $a9 >M
T{ op PC S A X Y P -> $f60c $08 $0e $4c $bb $25 }T T{ $f60b M $f60c M $f60d M -> $6a $20 $a9 }T
$b961 >PC $53 >S $4a >A $36 >X $73 >Y $e0 >P $b961 $6a >M $b962 $5e >M $b963 $49 >M
T{ op PC S A X Y P -> $b962 $53 $25 $36 $73 $60 }T T{ $b961 M $b962 M $b963 M -> $6a $5e $49 }T
$845c >PC $0b >S $61 >A $26 >X $15 >Y $22 >P $845c $6a >M $845d $bb >M $845e $db >M
T{ op PC S A X Y P -> $845d $0b $30 $26 $15 $21 }T T{ $845c M $845d M $845e M -> $6a $bb $db }T
$6f51 >PC $0f >S $01 >A $0f >X $66 >Y $20 >P $6f51 $6a >M $6f52 $3f >M $6f53 $24 >M
T{ op PC S A X Y P -> $6f52 $0f $00 $0f $66 $23 }T T{ $6f51 M $6f52 M $6f53 M -> $6a $3f $24 }T
$9fe6 >PC $cb >S $44 >A $ec >X $20 >Y $64 >P $9fe6 $6a >M $9fe7 $6b >M $9fe8 $36 >M
T{ op PC S A X Y P -> $9fe7 $cb $22 $ec $20 $64 }T T{ $9fe6 M $9fe7 M $9fe8 M -> $6a $6b $36 }T
$38bd >PC $5b >S $3a >A $c4 >X $8a >Y $a5 >P $38bd $6a >M $38be $19 >M $38bf $d4 >M
T{ op PC S A X Y P -> $38be $5b $9d $c4 $8a $a4 }T T{ $38bd M $38be M $38bf M -> $6a $19 $d4 }T
$e149 >PC $3d >S $72 >A $e2 >X $7d >Y $e5 >P $e149 $6a >M $e14a $9c >M $e14b $e2 >M
T{ op PC S A X Y P -> $e14a $3d $b9 $e2 $7d $e4 }T T{ $e149 M $e14a M $e14b M -> $6a $9c $e2 }T
$2699 >PC $96 >S $c0 >A $93 >X $ee >Y $25 >P $2699 $6a >M $269a $80 >M $269b $b5 >M
T{ op PC S A X Y P -> $269a $96 $e0 $93 $ee $a4 }T T{ $2699 M $269a M $269b M -> $6a $80 $b5 }T
$dfab >PC $52 >S $f9 >A $fc >X $65 >Y $66 >P $dfab $6a >M $dfac $11 >M $dfad $3c >M
T{ op PC S A X Y P -> $dfac $52 $7c $fc $65 $65 }T T{ $dfab M $dfac M $dfad M -> $6a $11 $3c }T
( 6b )
$6391 >PC $cf >S $6d >A $fe >X $37 >Y $e2 >P $6391 $6b >M $6392 $d9 >M $6393 $28 >M
T{ op PC S A X Y P -> $6392 $cf $6d $fe $37 $e2 }T T{ $6391 M $6392 M $6393 M -> $6b $d9 $28 }T
$5323 >PC $2f >S $df >A $2d >X $41 >Y $25 >P $5323 $6b >M $5324 $09 >M $5325 $97 >M
T{ op PC S A X Y P -> $5324 $2f $df $2d $41 $25 }T T{ $5323 M $5324 M $5325 M -> $6b $09 $97 }T
$43db >PC $1c >S $70 >A $c0 >X $fd >Y $64 >P $43db $6b >M $43dc $30 >M $43dd $fc >M
T{ op PC S A X Y P -> $43dc $1c $70 $c0 $fd $64 }T T{ $43db M $43dc M $43dd M -> $6b $30 $fc }T
$3e88 >PC $26 >S $d5 >A $f6 >X $7e >Y $a0 >P $3e88 $6b >M $3e89 $44 >M $3e8a $b7 >M
T{ op PC S A X Y P -> $3e89 $26 $d5 $f6 $7e $a0 }T T{ $3e88 M $3e89 M $3e8a M -> $6b $44 $b7 }T
$6e63 >PC $b5 >S $72 >A $0b >X $58 >Y $65 >P $6e63 $6b >M $6e64 $4b >M $6e65 $6b >M
T{ op PC S A X Y P -> $6e64 $b5 $72 $0b $58 $65 }T T{ $6e63 M $6e64 M $6e65 M -> $6b $4b $6b }T
$5d45 >PC $5e >S $da >A $1c >X $20 >Y $a2 >P $5d45 $6b >M $5d46 $e5 >M $5d47 $06 >M
T{ op PC S A X Y P -> $5d46 $5e $da $1c $20 $a2 }T T{ $5d45 M $5d46 M $5d47 M -> $6b $e5 $06 }T
$91e5 >PC $90 >S $6d >A $f5 >X $6e >Y $a2 >P $91e5 $6b >M $91e6 $5d >M $91e7 $a6 >M
T{ op PC S A X Y P -> $91e6 $90 $6d $f5 $6e $a2 }T T{ $91e5 M $91e6 M $91e7 M -> $6b $5d $a6 }T
$8169 >PC $a8 >S $ca >A $e0 >X $a9 >Y $e5 >P $8169 $6b >M $816a $f9 >M $816b $1a >M
T{ op PC S A X Y P -> $816a $a8 $ca $e0 $a9 $e5 }T T{ $8169 M $816a M $816b M -> $6b $f9 $1a }T
$1b5d >PC $3a >S $65 >A $67 >X $90 >Y $a1 >P $1b5d $6b >M $1b5e $8a >M $1b5f $c3 >M
T{ op PC S A X Y P -> $1b5e $3a $65 $67 $90 $a1 }T T{ $1b5d M $1b5e M $1b5f M -> $6b $8a $c3 }T
$2287 >PC $a0 >S $07 >A $d1 >X $8a >Y $a0 >P $2287 $6b >M $2288 $98 >M $2289 $c9 >M
T{ op PC S A X Y P -> $2288 $a0 $07 $d1 $8a $a0 }T T{ $2287 M $2288 M $2289 M -> $6b $98 $c9 }T
$94e1 >PC $48 >S $b7 >A $c0 >X $7f >Y $a3 >P $94e1 $6b >M $94e2 $0a >M $94e3 $02 >M
T{ op PC S A X Y P -> $94e2 $48 $b7 $c0 $7f $a3 }T T{ $94e1 M $94e2 M $94e3 M -> $6b $0a $02 }T
$fdac >PC $3f >S $3a >A $ae >X $8f >Y $e1 >P $fdac $6b >M $fdad $55 >M $fdae $64 >M
T{ op PC S A X Y P -> $fdad $3f $3a $ae $8f $e1 }T T{ $fdac M $fdad M $fdae M -> $6b $55 $64 }T
$4838 >PC $59 >S $4f >A $56 >X $af >Y $e0 >P $4838 $6b >M $4839 $62 >M $483a $3b >M
T{ op PC S A X Y P -> $4839 $59 $4f $56 $af $e0 }T T{ $4838 M $4839 M $483a M -> $6b $62 $3b }T
$2586 >PC $ec >S $88 >A $64 >X $10 >Y $66 >P $2586 $6b >M $2587 $9c >M $2588 $ba >M
T{ op PC S A X Y P -> $2587 $ec $88 $64 $10 $66 }T T{ $2586 M $2587 M $2588 M -> $6b $9c $ba }T
$2b70 >PC $e0 >S $4c >A $8c >X $fd >Y $20 >P $2b70 $6b >M $2b71 $03 >M $2b72 $fc >M
T{ op PC S A X Y P -> $2b71 $e0 $4c $8c $fd $20 }T T{ $2b70 M $2b71 M $2b72 M -> $6b $03 $fc }T
$9263 >PC $fa >S $b9 >A $4a >X $55 >Y $e3 >P $9263 $6b >M $9264 $11 >M $9265 $87 >M
T{ op PC S A X Y P -> $9264 $fa $b9 $4a $55 $e3 }T T{ $9263 M $9264 M $9265 M -> $6b $11 $87 }T
( 6c )
$21e9 >PC $0c >S $a8 >A $c4 >X $30 >Y $20 >P $21e9 $6c >M $21ea $33 >M $21eb $f9 >M $2c6b $25 >M $f933 $6b >M $f934 $2c >M
T{ op PC S A X Y P -> $2c6b $0c $a8 $c4 $30 $20 }T T{ $21e9 M $21ea M $21eb M $2c6b M $f933 M $f934 M -> $6c $33 $f9 $25 $6b $2c }T
$b2e9 >PC $46 >S $03 >A $7f >X $71 >Y $a3 >P $2663 $3f >M $b2e9 $6c >M $b2ea $27 >M $b2eb $ff >M $ff27 $63 >M $ff28 $26 >M
T{ op PC S A X Y P -> $2663 $46 $03 $7f $71 $a3 }T T{ $2663 M $b2e9 M $b2ea M $b2eb M $ff27 M $ff28 M -> $3f $6c $27 $ff $63 $26 }T
$d11b >PC $fc >S $63 >A $60 >X $d1 >Y $26 >P $4a68 $25 >M $4a69 $b8 >M $b825 $b7 >M $d11b $6c >M $d11c $68 >M $d11d $4a >M
T{ op PC S A X Y P -> $b825 $fc $63 $60 $d1 $26 }T T{ $4a68 M $4a69 M $b825 M $d11b M $d11c M $d11d M -> $25 $b8 $b7 $6c $68 $4a }T
$0592 >PC $f9 >S $a1 >A $ed >X $6c >Y $23 >P $0592 $6c >M $0593 $dc >M $0594 $b3 >M $b3dc $e1 >M $b3dd $b9 >M $b9e1 $75 >M
T{ op PC S A X Y P -> $b9e1 $f9 $a1 $ed $6c $23 }T T{ $0592 M $0593 M $0594 M $b3dc M $b3dd M $b9e1 M -> $6c $dc $b3 $e1 $b9 $75 }T
$19a4 >PC $0a >S $b0 >A $44 >X $3d >Y $a4 >P $0c1d $ec >M $0c1e $73 >M $19a4 $6c >M $19a5 $1d >M $19a6 $0c >M $73ec $92 >M
T{ op PC S A X Y P -> $73ec $0a $b0 $44 $3d $a4 }T T{ $0c1d M $0c1e M $19a4 M $19a5 M $19a6 M $73ec M -> $ec $73 $6c $1d $0c $92 }T
$e2e9 >PC $af >S $50 >A $77 >X $9b >Y $25 >P $6457 $5f >M $6458 $dd >M $dd5f $af >M $e2e9 $6c >M $e2ea $57 >M $e2eb $64 >M
T{ op PC S A X Y P -> $dd5f $af $50 $77 $9b $25 }T T{ $6457 M $6458 M $dd5f M $e2e9 M $e2ea M $e2eb M -> $5f $dd $af $6c $57 $64 }T
$bae5 >PC $dd >S $64 >A $a7 >X $d3 >Y $62 >P $1dfb $a7 >M $1dfc $ac >M $aca7 $35 >M $bae5 $6c >M $bae6 $fb >M $bae7 $1d >M
T{ op PC S A X Y P -> $aca7 $dd $64 $a7 $d3 $62 }T T{ $1dfb M $1dfc M $aca7 M $bae5 M $bae6 M $bae7 M -> $a7 $ac $35 $6c $fb $1d }T
$abca >PC $87 >S $3b >A $e5 >X $d0 >Y $24 >P $19be $2b >M $abca $6c >M $abcb $1b >M $abcc $e7 >M $e71b $be >M $e71c $19 >M
T{ op PC S A X Y P -> $19be $87 $3b $e5 $d0 $24 }T T{ $19be M $abca M $abcb M $abcc M $e71b M $e71c M -> $2b $6c $1b $e7 $be $19 }T
$7277 >PC $0a >S $b8 >A $5e >X $41 >Y $e1 >P $7277 $6c >M $7278 $36 >M $7279 $78 >M $7836 $aa >M $7837 $de >M $deaa $00 >M
T{ op PC S A X Y P -> $deaa $0a $b8 $5e $41 $e1 }T T{ $7277 M $7278 M $7279 M $7836 M $7837 M $deaa M -> $6c $36 $78 $aa $de $00 }T
$ab03 >PC $7f >S $ff >A $36 >X $f0 >Y $e4 >P $ab03 $6c >M $ab04 $fd >M $ab05 $ea >M $cf5f $36 >M $eafd $5f >M $eafe $cf >M
T{ op PC S A X Y P -> $cf5f $7f $ff $36 $f0 $e4 }T T{ $ab03 M $ab04 M $ab05 M $cf5f M $eafd M $eafe M -> $6c $fd $ea $36 $5f $cf }T
$5b80 >PC $66 >S $bc >A $0b >X $b6 >Y $60 >P $3e7f $f6 >M $3e80 $79 >M $5b80 $6c >M $5b81 $7f >M $5b82 $3e >M $79f6 $8e >M
T{ op PC S A X Y P -> $79f6 $66 $bc $0b $b6 $60 }T T{ $3e7f M $3e80 M $5b80 M $5b81 M $5b82 M $79f6 M -> $f6 $79 $6c $7f $3e $8e }T
$0b3b >PC $f5 >S $05 >A $90 >X $70 >Y $e2 >P $0b3b $6c >M $0b3c $65 >M $0b3d $f3 >M $41af $b4 >M $f365 $af >M $f366 $41 >M
T{ op PC S A X Y P -> $41af $f5 $05 $90 $70 $e2 }T T{ $0b3b M $0b3c M $0b3d M $41af M $f365 M $f366 M -> $6c $65 $f3 $b4 $af $41 }T
$73c5 >PC $d8 >S $4a >A $cf >X $62 >Y $64 >P $554f $30 >M $73c5 $6c >M $73c6 $ed >M $73c7 $d8 >M $d8ed $4f >M $d8ee $55 >M
T{ op PC S A X Y P -> $554f $d8 $4a $cf $62 $64 }T T{ $554f M $73c5 M $73c6 M $73c7 M $d8ed M $d8ee M -> $30 $6c $ed $d8 $4f $55 }T
$6bb1 >PC $f1 >S $1f >A $60 >X $5a >Y $e6 >P $6bb1 $6c >M $6bb2 $29 >M $6bb3 $b7 >M $6ea4 $a0 >M $b729 $a4 >M $b72a $6e >M
T{ op PC S A X Y P -> $6ea4 $f1 $1f $60 $5a $e6 }T T{ $6bb1 M $6bb2 M $6bb3 M $6ea4 M $b729 M $b72a M -> $6c $29 $b7 $a0 $a4 $6e }T
$12ad >PC $13 >S $8b >A $11 >X $75 >Y $a2 >P $12ad $6c >M $12ae $d4 >M $12af $83 >M $6823 $2c >M $83d4 $23 >M $83d5 $68 >M
T{ op PC S A X Y P -> $6823 $13 $8b $11 $75 $a2 }T T{ $12ad M $12ae M $12af M $6823 M $83d4 M $83d5 M -> $6c $d4 $83 $2c $23 $68 }T
$4945 >PC $50 >S $8e >A $ff >X $c3 >Y $65 >P $05ca $02 >M $05cb $b3 >M $4945 $6c >M $4946 $ca >M $4947 $05 >M $b302 $ef >M
T{ op PC S A X Y P -> $b302 $50 $8e $ff $c3 $65 }T T{ $05ca M $05cb M $4945 M $4946 M $4947 M $b302 M -> $02 $b3 $6c $ca $05 $ef }T
( 6d )
$14c9 >PC $57 >S $cf >A $8c >X $a1 >Y $26 >P $14c9 $6d >M $14ca $af >M $14cb $d6 >M $14cc $91 >M $d6af $ec >M
T{ op PC S A X Y P -> $14cc $57 $bb $8c $a1 $a5 }T T{ $14c9 M $14ca M $14cb M $14cc M $d6af M -> $6d $af $d6 $91 $ec }T
$3e55 >PC $0d >S $a8 >A $b9 >X $da >Y $e6 >P $3e55 $6d >M $3e56 $c1 >M $3e57 $a7 >M $3e58 $7e >M $a7c1 $5c >M
T{ op PC S A X Y P -> $3e58 $0d $04 $b9 $da $25 }T T{ $3e55 M $3e56 M $3e57 M $3e58 M $a7c1 M -> $6d $c1 $a7 $7e $5c }T
$c5ee >PC $a0 >S $2d >A $ee >X $8c >Y $67 >P $2280 $39 >M $c5ee $6d >M $c5ef $80 >M $c5f0 $22 >M $c5f1 $51 >M
T{ op PC S A X Y P -> $c5f1 $a0 $67 $ee $8c $24 }T T{ $2280 M $c5ee M $c5ef M $c5f0 M $c5f1 M -> $39 $6d $80 $22 $51 }T
$f5c7 >PC $4c >S $f9 >A $1d >X $66 >Y $60 >P $812c $3e >M $f5c7 $6d >M $f5c8 $2c >M $f5c9 $81 >M $f5ca $e8 >M
T{ op PC S A X Y P -> $f5ca $4c $37 $1d $66 $21 }T T{ $812c M $f5c7 M $f5c8 M $f5c9 M $f5ca M -> $3e $6d $2c $81 $e8 }T
$59d9 >PC $35 >S $7e >A $7c >X $89 >Y $a5 >P $59d9 $6d >M $59da $f9 >M $59db $72 >M $59dc $e6 >M $72f9 $e3 >M
T{ op PC S A X Y P -> $59dc $35 $62 $7c $89 $25 }T T{ $59d9 M $59da M $59db M $59dc M $72f9 M -> $6d $f9 $72 $e6 $e3 }T
$12d5 >PC $63 >S $de >A $3a >X $9e >Y $a2 >P $12d5 $6d >M $12d6 $23 >M $12d7 $c9 >M $12d8 $20 >M $c923 $6c >M
T{ op PC S A X Y P -> $12d8 $63 $4a $3a $9e $21 }T T{ $12d5 M $12d6 M $12d7 M $12d8 M $c923 M -> $6d $23 $c9 $20 $6c }T
$47e4 >PC $97 >S $61 >A $68 >X $e7 >Y $a6 >P $47e4 $6d >M $47e5 $1a >M $47e6 $d6 >M $47e7 $b7 >M $d61a $64 >M
T{ op PC S A X Y P -> $47e7 $97 $c5 $68 $e7 $e4 }T T{ $47e4 M $47e5 M $47e6 M $47e7 M $d61a M -> $6d $1a $d6 $b7 $64 }T
$d726 >PC $f1 >S $87 >A $fb >X $99 >Y $61 >P $b22a $56 >M $d726 $6d >M $d727 $2a >M $d728 $b2 >M $d729 $ab >M
T{ op PC S A X Y P -> $d729 $f1 $de $fb $99 $a0 }T T{ $b22a M $d726 M $d727 M $d728 M $d729 M -> $56 $6d $2a $b2 $ab }T
$ffd4 >PC $56 >S $b2 >A $bf >X $d9 >Y $60 >P $cb70 $a7 >M $ffd4 $6d >M $ffd5 $70 >M $ffd6 $cb >M $ffd7 $29 >M
T{ op PC S A X Y P -> $ffd7 $56 $59 $bf $d9 $61 }T T{ $cb70 M $ffd4 M $ffd5 M $ffd6 M $ffd7 M -> $a7 $6d $70 $cb $29 }T
$aa3d >PC $9b >S $df >A $5c >X $78 >Y $21 >P $3231 $36 >M $aa3d $6d >M $aa3e $31 >M $aa3f $32 >M $aa40 $ba >M
T{ op PC S A X Y P -> $aa40 $9b $16 $5c $78 $21 }T T{ $3231 M $aa3d M $aa3e M $aa3f M $aa40 M -> $36 $6d $31 $32 $ba }T
$3756 >PC $4e >S $29 >A $51 >X $6a >Y $63 >P $3756 $6d >M $3757 $83 >M $3758 $88 >M $3759 $7c >M $8883 $c5 >M
T{ op PC S A X Y P -> $3759 $4e $ef $51 $6a $a0 }T T{ $3756 M $3757 M $3758 M $3759 M $8883 M -> $6d $83 $88 $7c $c5 }T
$b065 >PC $0b >S $44 >A $d0 >X $d1 >Y $25 >P $b065 $6d >M $b066 $c7 >M $b067 $ca >M $b068 $8a >M $cac7 $da >M
T{ op PC S A X Y P -> $b068 $0b $1f $d0 $d1 $25 }T T{ $b065 M $b066 M $b067 M $b068 M $cac7 M -> $6d $c7 $ca $8a $da }T
$88dd >PC $30 >S $1e >A $81 >X $fe >Y $27 >P $88dd $6d >M $88de $c7 >M $88df $d5 >M $88e0 $ed >M $d5c7 $b2 >M
T{ op PC S A X Y P -> $88e0 $30 $d1 $81 $fe $a4 }T T{ $88dd M $88de M $88df M $88e0 M $d5c7 M -> $6d $c7 $d5 $ed $b2 }T
$fcf0 >PC $4c >S $1e >A $4a >X $cd >Y $67 >P $6a3b $69 >M $fcf0 $6d >M $fcf1 $3b >M $fcf2 $6a >M $fcf3 $be >M
T{ op PC S A X Y P -> $fcf3 $4c $88 $4a $cd $e4 }T T{ $6a3b M $fcf0 M $fcf1 M $fcf2 M $fcf3 M -> $69 $6d $3b $6a $be }T
$0324 >PC $2b >S $ee >A $e4 >X $d5 >Y $63 >P $0324 $6d >M $0325 $44 >M $0326 $d6 >M $0327 $30 >M $d644 $b6 >M
T{ op PC S A X Y P -> $0327 $2b $a5 $e4 $d5 $a1 }T T{ $0324 M $0325 M $0326 M $0327 M $d644 M -> $6d $44 $d6 $30 $b6 }T
$4a65 >PC $20 >S $5e >A $a2 >X $9e >Y $e0 >P $4a65 $6d >M $4a66 $b7 >M $4a67 $4e >M $4a68 $4e >M $4eb7 $de >M
T{ op PC S A X Y P -> $4a68 $20 $3c $a2 $9e $21 }T T{ $4a65 M $4a66 M $4a67 M $4a68 M $4eb7 M -> $6d $b7 $4e $4e $de }T
( 6e )
$7f2d >PC $85 >S $4b >A $57 >X $3e >Y $a4 >P $7f2d $6e >M $7f2e $43 >M $7f2f $7f >M $7f30 $6f >M $7f43 $1e >M
T{ op PC S A X Y P -> $7f30 $85 $4b $57 $3e $24 }T T{ $7f2d M $7f2e M $7f2f M $7f30 M $7f43 M -> $6e $43 $7f $6f $0f }T
$a587 >PC $4d >S $38 >A $ee >X $4b >Y $27 >P $a587 $6e >M $a588 $d6 >M $a589 $ed >M $a58a $96 >M $edd6 $dc >M
T{ op PC S A X Y P -> $a58a $4d $38 $ee $4b $a4 }T T{ $a587 M $a588 M $a589 M $a58a M $edd6 M -> $6e $d6 $ed $96 $ee }T
$3ffc >PC $ff >S $9b >A $09 >X $ec >Y $e3 >P $3ffc $6e >M $3ffd $25 >M $3ffe $85 >M $3fff $14 >M $8525 $46 >M
T{ op PC S A X Y P -> $3fff $ff $9b $09 $ec $e0 }T T{ $3ffc M $3ffd M $3ffe M $3fff M $8525 M -> $6e $25 $85 $14 $a3 }T
$a2fe >PC $ad >S $89 >A $b5 >X $b4 >Y $e4 >P $2c7e $1a >M $a2fe $6e >M $a2ff $7e >M $a300 $2c >M $a301 $7f >M
T{ op PC S A X Y P -> $a301 $ad $89 $b5 $b4 $64 }T T{ $2c7e M $a2fe M $a2ff M $a300 M $a301 M -> $0d $6e $7e $2c $7f }T
$9005 >PC $ec >S $b2 >A $1d >X $67 >Y $a0 >P $9005 $6e >M $9006 $c4 >M $9007 $bc >M $9008 $ad >M $bcc4 $18 >M
T{ op PC S A X Y P -> $9008 $ec $b2 $1d $67 $20 }T T{ $9005 M $9006 M $9007 M $9008 M $bcc4 M -> $6e $c4 $bc $ad $0c }T
$7150 >PC $b9 >S $dc >A $b5 >X $aa >Y $e4 >P $445d $99 >M $7150 $6e >M $7151 $5d >M $7152 $44 >M $7153 $5d >M
T{ op PC S A X Y P -> $7153 $b9 $dc $b5 $aa $65 }T T{ $445d M $7150 M $7151 M $7152 M $7153 M -> $4c $6e $5d $44 $5d }T
$6714 >PC $d7 >S $60 >A $4a >X $63 >Y $e0 >P $6714 $6e >M $6715 $1b >M $6716 $7b >M $6717 $21 >M $7b1b $7e >M
T{ op PC S A X Y P -> $6717 $d7 $60 $4a $63 $60 }T T{ $6714 M $6715 M $6716 M $6717 M $7b1b M -> $6e $1b $7b $21 $3f }T
$8758 >PC $32 >S $04 >A $ae >X $3f >Y $a0 >P $4084 $50 >M $8758 $6e >M $8759 $84 >M $875a $40 >M $875b $51 >M
T{ op PC S A X Y P -> $875b $32 $04 $ae $3f $20 }T T{ $4084 M $8758 M $8759 M $875a M $875b M -> $28 $6e $84 $40 $51 }T
$d883 >PC $5b >S $a8 >A $b7 >X $9e >Y $e6 >P $319a $8b >M $d883 $6e >M $d884 $9a >M $d885 $31 >M $d886 $df >M
T{ op PC S A X Y P -> $d886 $5b $a8 $b7 $9e $65 }T T{ $319a M $d883 M $d884 M $d885 M $d886 M -> $45 $6e $9a $31 $df }T
$747b >PC $77 >S $90 >A $e9 >X $d8 >Y $e5 >P $21a6 $93 >M $747b $6e >M $747c $a6 >M $747d $21 >M $747e $3f >M
T{ op PC S A X Y P -> $747e $77 $90 $e9 $d8 $e5 }T T{ $21a6 M $747b M $747c M $747d M $747e M -> $c9 $6e $a6 $21 $3f }T
$cd9e >PC $64 >S $a7 >A $3a >X $a3 >Y $23 >P $35b4 $9d >M $cd9e $6e >M $cd9f $b4 >M $cda0 $35 >M $cda1 $e1 >M
T{ op PC S A X Y P -> $cda1 $64 $a7 $3a $a3 $a1 }T T{ $35b4 M $cd9e M $cd9f M $cda0 M $cda1 M -> $ce $6e $b4 $35 $e1 }T
$0997 >PC $0c >S $7f >A $d8 >X $db >Y $a4 >P $0997 $6e >M $0998 $da >M $0999 $58 >M $099a $d6 >M $58da $f1 >M
T{ op PC S A X Y P -> $099a $0c $7f $d8 $db $25 }T T{ $0997 M $0998 M $0999 M $099a M $58da M -> $6e $da $58 $d6 $78 }T
$34e5 >PC $bb >S $1c >A $fc >X $b8 >Y $23 >P $34e5 $6e >M $34e6 $75 >M $34e7 $8a >M $34e8 $d6 >M $8a75 $0a >M
T{ op PC S A X Y P -> $34e8 $bb $1c $fc $b8 $a0 }T T{ $34e5 M $34e6 M $34e7 M $34e8 M $8a75 M -> $6e $75 $8a $d6 $85 }T
$ba12 >PC $7b >S $8c >A $84 >X $58 >Y $25 >P $20da $22 >M $ba12 $6e >M $ba13 $da >M $ba14 $20 >M $ba15 $ab >M
T{ op PC S A X Y P -> $ba15 $7b $8c $84 $58 $a4 }T T{ $20da M $ba12 M $ba13 M $ba14 M $ba15 M -> $91 $6e $da $20 $ab }T
$9336 >PC $f5 >S $da >A $1a >X $51 >Y $21 >P $9336 $6e >M $9337 $24 >M $9338 $b4 >M $9339 $d1 >M $b424 $77 >M
T{ op PC S A X Y P -> $9339 $f5 $da $1a $51 $a1 }T T{ $9336 M $9337 M $9338 M $9339 M $b424 M -> $6e $24 $b4 $d1 $bb }T
$de92 >PC $f2 >S $25 >A $9d >X $5d >Y $61 >P $3a75 $37 >M $de92 $6e >M $de93 $75 >M $de94 $3a >M $de95 $59 >M
T{ op PC S A X Y P -> $de95 $f2 $25 $9d $5d $e1 }T T{ $3a75 M $de92 M $de93 M $de94 M $de95 M -> $9b $6e $75 $3a $59 }T
( 6f )
$2fcd >PC $fb >S $45 >A $df >X $4a >Y $a6 >P $0089 $17 >M $2f12 $a3 >M $2fcd $6f >M $2fce $89 >M $2fcf $42 >M $3012 $a8 >M
T{ op PC S A X Y P -> $3012 $fb $45 $df $4a $a6 }T T{ $0089 M $2f12 M $2fcd M $2fce M $2fcf M $3012 M -> $17 $a3 $6f $89 $42 $a8 }T
$51e9 >PC $04 >S $ab >A $84 >X $5f >Y $e4 >P $007a $73 >M $51e9 $6f >M $51ea $7a >M $51eb $0b >M $51ec $9c >M $51f7 $4c >M
T{ op PC S A X Y P -> $51ec $04 $ab $84 $5f $e4 }T T{ $007a M $51e9 M $51ea M $51eb M $51ec M $51f7 M -> $73 $6f $7a $0b $9c $4c }T
$2a50 >PC $da >S $53 >A $84 >X $cb >Y $a3 >P $00ca $d9 >M $2a50 $6f >M $2a51 $ca >M $2a52 $9c >M $2a53 $d3 >M $2aef $03 >M
T{ op PC S A X Y P -> $2a53 $da $53 $84 $cb $a3 }T T{ $00ca M $2a50 M $2a51 M $2a52 M $2a53 M $2aef M -> $d9 $6f $ca $9c $d3 $03 }T
$2a62 >PC $33 >S $fa >A $6b >X $50 >Y $a6 >P $0021 $a8 >M $2a62 $6f >M $2a63 $21 >M $2a64 $57 >M $2abc $50 >M
T{ op PC S A X Y P -> $2abc $33 $fa $6b $50 $a6 }T T{ $0021 M $2a62 M $2a63 M $2a64 M $2abc M -> $a8 $6f $21 $57 $50 }T
$8a9e >PC $44 >S $8b >A $ce >X $12 >Y $e1 >P $00e4 $61 >M $8a9e $6f >M $8a9f $e4 >M $8aa0 $10 >M $8aa1 $0f >M $8ab1 $02 >M
T{ op PC S A X Y P -> $8aa1 $44 $8b $ce $12 $e1 }T T{ $00e4 M $8a9e M $8a9f M $8aa0 M $8aa1 M $8ab1 M -> $61 $6f $e4 $10 $0f $02 }T
$f0a2 >PC $1a >S $00 >A $1c >X $9b >Y $63 >P $00e4 $d3 >M $f010 $fe >M $f0a2 $6f >M $f0a3 $e4 >M $f0a4 $6b >M $f0a5 $10 >M
T{ op PC S A X Y P -> $f0a5 $1a $00 $1c $9b $63 }T T{ $00e4 M $f010 M $f0a2 M $f0a3 M $f0a4 M $f0a5 M -> $d3 $fe $6f $e4 $6b $10 }T
$e487 >PC $62 >S $90 >A $b7 >X $d5 >Y $e0 >P $00a4 $f2 >M $e451 $68 >M $e487 $6f >M $e488 $a4 >M $e489 $c7 >M $e48a $5d >M
T{ op PC S A X Y P -> $e48a $62 $90 $b7 $d5 $e0 }T T{ $00a4 M $e451 M $e487 M $e488 M $e489 M $e48a M -> $f2 $68 $6f $a4 $c7 $5d }T
$9255 >PC $38 >S $91 >A $b8 >X $34 >Y $a3 >P $00a0 $ba >M $91da $f3 >M $9255 $6f >M $9256 $a0 >M $9257 $82 >M $92da $04 >M
T{ op PC S A X Y P -> $91da $38 $91 $b8 $34 $a3 }T T{ $00a0 M $91da M $9255 M $9256 M $9257 M $92da M -> $ba $f3 $6f $a0 $82 $04 }T
$f5fe >PC $00 >S $a4 >A $39 >X $dd >Y $25 >P $009d $93 >M $f5e2 $fa >M $f5fe $6f >M $f5ff $9d >M $f600 $e1 >M $f6e2 $ac >M
T{ op PC S A X Y P -> $f5e2 $00 $a4 $39 $dd $25 }T T{ $009d M $f5e2 M $f5fe M $f5ff M $f600 M $f6e2 M -> $93 $fa $6f $9d $e1 $ac }T
$255c >PC $3f >S $67 >A $77 >X $31 >Y $a3 >P $0083 $92 >M $255c $6f >M $255d $83 >M $255e $06 >M $2565 $9e >M
T{ op PC S A X Y P -> $2565 $3f $67 $77 $31 $a3 }T T{ $0083 M $255c M $255d M $255e M $2565 M -> $92 $6f $83 $06 $9e }T
$6912 >PC $95 >S $d3 >A $6e >X $3f >Y $62 >P $00e5 $59 >M $6912 $6f >M $6913 $e5 >M $6914 $ba >M $6915 $95 >M $69cf $a3 >M
T{ op PC S A X Y P -> $6915 $95 $d3 $6e $3f $62 }T T{ $00e5 M $6912 M $6913 M $6914 M $6915 M $69cf M -> $59 $6f $e5 $ba $95 $a3 }T
$822c >PC $16 >S $29 >A $2d >X $5a >Y $20 >P $00ff $c9 >M $822c $6f >M $822d $ff >M $822e $6d >M $822f $f5 >M $829c $ae >M
T{ op PC S A X Y P -> $822f $16 $29 $2d $5a $20 }T T{ $00ff M $822c M $822d M $822e M $822f M $829c M -> $c9 $6f $ff $6d $f5 $ae }T
$4bf2 >PC $b0 >S $eb >A $2b >X $c8 >Y $a0 >P $008e $7c >M $4b5e $a9 >M $4bf2 $6f >M $4bf3 $8e >M $4bf4 $69 >M $4bf5 $b3 >M
T{ op PC S A X Y P -> $4bf5 $b0 $eb $2b $c8 $a0 }T T{ $008e M $4b5e M $4bf2 M $4bf3 M $4bf4 M $4bf5 M -> $7c $a9 $6f $8e $69 $b3 }T
$e77d >PC $6a >S $6b >A $ad >X $b4 >Y $e5 >P $0018 $b2 >M $e77d $6f >M $e77e $18 >M $e77f $72 >M $e7f2 $2a >M
T{ op PC S A X Y P -> $e7f2 $6a $6b $ad $b4 $e5 }T T{ $0018 M $e77d M $e77e M $e77f M $e7f2 M -> $b2 $6f $18 $72 $2a }T
$4084 >PC $10 >S $c7 >A $f6 >X $95 >Y $a6 >P $006c $f1 >M $4055 $8b >M $4084 $6f >M $4085 $6c >M $4086 $ce >M $4087 $12 >M
T{ op PC S A X Y P -> $4087 $10 $c7 $f6 $95 $a6 }T T{ $006c M $4055 M $4084 M $4085 M $4086 M $4087 M -> $f1 $8b $6f $6c $ce $12 }T
$aee1 >PC $c6 >S $f0 >A $3d >X $ca >Y $a2 >P $00ac $e4 >M $ae77 $be >M $aee1 $6f >M $aee2 $ac >M $aee3 $93 >M $aee4 $7f >M
T{ op PC S A X Y P -> $aee4 $c6 $f0 $3d $ca $a2 }T T{ $00ac M $ae77 M $aee1 M $aee2 M $aee3 M $aee4 M -> $e4 $be $6f $ac $93 $7f }T
( 70 )
$e4aa >PC $7f >S $52 >A $b3 >X $8b >Y $e3 >P $e4aa $70 >M $e4ab $4f >M $e4ac $5b >M $e4fb $ef >M
T{ op PC S A X Y P -> $e4fb $7f $52 $b3 $8b $e3 }T T{ $e4aa M $e4ab M $e4ac M $e4fb M -> $70 $4f $5b $ef }T
$e457 >PC $ec >S $13 >A $4c >X $d6 >Y $62 >P $e3f5 $f7 >M $e457 $70 >M $e458 $9c >M $e459 $2f >M $e4f5 $0e >M
T{ op PC S A X Y P -> $e3f5 $ec $13 $4c $d6 $62 }T T{ $e3f5 M $e457 M $e458 M $e459 M $e4f5 M -> $f7 $70 $9c $2f $0e }T
$4a55 >PC $27 >S $e4 >A $10 >X $16 >Y $a3 >P $4a55 $70 >M $4a56 $0a >M $4a57 $48 >M
T{ op PC S A X Y P -> $4a57 $27 $e4 $10 $16 $a3 }T T{ $4a55 M $4a56 M $4a57 M -> $70 $0a $48 }T
$67da >PC $05 >S $69 >A $be >X $f2 >Y $e6 >P $670f $44 >M $67da $70 >M $67db $33 >M $67dc $e4 >M $680f $cc >M
T{ op PC S A X Y P -> $680f $05 $69 $be $f2 $e6 }T T{ $670f M $67da M $67db M $67dc M $680f M -> $44 $70 $33 $e4 $cc }T
$b376 >PC $d0 >S $cf >A $13 >X $90 >Y $a4 >P $b376 $70 >M $b377 $4f >M $b378 $3c >M
T{ op PC S A X Y P -> $b378 $d0 $cf $13 $90 $a4 }T T{ $b376 M $b377 M $b378 M -> $70 $4f $3c }T
$0975 >PC $48 >S $9b >A $9d >X $fb >Y $24 >P $0975 $70 >M $0976 $27 >M $0977 $69 >M
T{ op PC S A X Y P -> $0977 $48 $9b $9d $fb $24 }T T{ $0975 M $0976 M $0977 M -> $70 $27 $69 }T
$af07 >PC $db >S $a5 >A $ba >X $2e >Y $21 >P $af07 $70 >M $af08 $d7 >M $af09 $5d >M
T{ op PC S A X Y P -> $af09 $db $a5 $ba $2e $21 }T T{ $af07 M $af08 M $af09 M -> $70 $d7 $5d }T
$b3c1 >PC $b2 >S $13 >A $87 >X $fe >Y $65 >P $b301 $c5 >M $b3c1 $70 >M $b3c2 $3e >M $b3c3 $d0 >M $b401 $22 >M
T{ op PC S A X Y P -> $b401 $b2 $13 $87 $fe $65 }T T{ $b301 M $b3c1 M $b3c2 M $b3c3 M $b401 M -> $c5 $70 $3e $d0 $22 }T
$f0a4 >PC $45 >S $12 >A $a7 >X $58 >Y $a6 >P $f0a4 $70 >M $f0a5 $12 >M $f0a6 $04 >M
T{ op PC S A X Y P -> $f0a6 $45 $12 $a7 $58 $a6 }T T{ $f0a4 M $f0a5 M $f0a6 M -> $70 $12 $04 }T
$fb68 >PC $aa >S $6b >A $32 >X $b6 >Y $22 >P $fb68 $70 >M $fb69 $a3 >M $fb6a $60 >M
T{ op PC S A X Y P -> $fb6a $aa $6b $32 $b6 $22 }T T{ $fb68 M $fb69 M $fb6a M -> $70 $a3 $60 }T
$4552 >PC $b2 >S $51 >A $2b >X $9e >Y $25 >P $4552 $70 >M $4553 $f0 >M $4554 $04 >M
T{ op PC S A X Y P -> $4554 $b2 $51 $2b $9e $25 }T T{ $4552 M $4553 M $4554 M -> $70 $f0 $04 }T
$a1ac >PC $26 >S $38 >A $d6 >X $69 >Y $e2 >P $a1ac $70 >M $a1ad $3a >M $a1ae $88 >M $a1e8 $f4 >M
T{ op PC S A X Y P -> $a1e8 $26 $38 $d6 $69 $e2 }T T{ $a1ac M $a1ad M $a1ae M $a1e8 M -> $70 $3a $88 $f4 }T
$d142 >PC $d2 >S $ef >A $fe >X $7c >Y $20 >P $d142 $70 >M $d143 $68 >M $d144 $60 >M
T{ op PC S A X Y P -> $d144 $d2 $ef $fe $7c $20 }T T{ $d142 M $d143 M $d144 M -> $70 $68 $60 }T
$9192 >PC $81 >S $ab >A $a3 >X $de >Y $24 >P $9192 $70 >M $9193 $61 >M $9194 $b2 >M
T{ op PC S A X Y P -> $9194 $81 $ab $a3 $de $24 }T T{ $9192 M $9193 M $9194 M -> $70 $61 $b2 }T
$0288 >PC $6c >S $25 >A $22 >X $f0 >Y $24 >P $0288 $70 >M $0289 $22 >M $028a $c4 >M
T{ op PC S A X Y P -> $028a $6c $25 $22 $f0 $24 }T T{ $0288 M $0289 M $028a M -> $70 $22 $c4 }T
$14d2 >PC $a2 >S $47 >A $dd >X $32 >Y $a0 >P $14d2 $70 >M $14d3 $93 >M $14d4 $1d >M
T{ op PC S A X Y P -> $14d4 $a2 $47 $dd $32 $a0 }T T{ $14d2 M $14d3 M $14d4 M -> $70 $93 $1d }T
( 71 )
$0af6 >PC $54 >S $98 >A $1d >X $5c >Y $24 >P $00ef $4b >M $00f0 $a8 >M $0af6 $71 >M $0af7 $ef >M $0af8 $84 >M $a8a7 $91 >M
T{ op PC S A X Y P -> $0af8 $54 $29 $1d $5c $65 }T T{ $00ef M $00f0 M $0af6 M $0af7 M $0af8 M $a8a7 M -> $4b $a8 $71 $ef $84 $91 }T
$8409 >PC $8b >S $7c >A $70 >X $75 >Y $a5 >P $00a9 $97 >M $00aa $ab >M $8409 $71 >M $840a $a9 >M $840b $a2 >M $ac0c $67 >M
T{ op PC S A X Y P -> $840b $8b $e4 $70 $75 $e4 }T T{ $00a9 M $00aa M $8409 M $840a M $840b M $ac0c M -> $97 $ab $71 $a9 $a2 $67 }T
$698f >PC $b2 >S $dd >A $dd >X $1f >Y $60 >P $0008 $4a >M $0009 $6c >M $698f $71 >M $6990 $08 >M $6991 $1e >M $6c69 $d9 >M
T{ op PC S A X Y P -> $6991 $b2 $b6 $dd $1f $a1 }T T{ $0008 M $0009 M $698f M $6990 M $6991 M $6c69 M -> $4a $6c $71 $08 $1e $d9 }T
$d5b3 >PC $ec >S $b4 >A $47 >X $67 >Y $60 >P $0030 $c8 >M $0031 $ca >M $cb2f $92 >M $d5b3 $71 >M $d5b4 $30 >M $d5b5 $fe >M
T{ op PC S A X Y P -> $d5b5 $ec $46 $47 $67 $61 }T T{ $0030 M $0031 M $cb2f M $d5b3 M $d5b4 M $d5b5 M -> $c8 $ca $92 $71 $30 $fe }T
$d301 >PC $46 >S $02 >A $64 >X $ac >Y $64 >P $0042 $56 >M $0043 $c4 >M $c502 $c7 >M $d301 $71 >M $d302 $42 >M $d303 $a4 >M
T{ op PC S A X Y P -> $d303 $46 $c9 $64 $ac $a4 }T T{ $0042 M $0043 M $c502 M $d301 M $d302 M $d303 M -> $56 $c4 $c7 $71 $42 $a4 }T
$98c1 >PC $35 >S $54 >A $fa >X $d4 >Y $e5 >P $0002 $20 >M $0003 $a6 >M $98c1 $71 >M $98c2 $02 >M $98c3 $71 >M $a6f4 $7e >M
T{ op PC S A X Y P -> $98c3 $35 $d3 $fa $d4 $e4 }T T{ $0002 M $0003 M $98c1 M $98c2 M $98c3 M $a6f4 M -> $20 $a6 $71 $02 $71 $7e }T
$0b89 >PC $3a >S $a0 >A $c1 >X $da >Y $a4 >P $00bf $78 >M $00c0 $3b >M $0b89 $71 >M $0b8a $bf >M $0b8b $e8 >M $3c52 $9e >M
T{ op PC S A X Y P -> $0b8b $3a $3e $c1 $da $65 }T T{ $00bf M $00c0 M $0b89 M $0b8a M $0b8b M $3c52 M -> $78 $3b $71 $bf $e8 $9e }T
$d330 >PC $b9 >S $15 >A $62 >X $39 >Y $67 >P $0062 $27 >M $0063 $44 >M $4460 $0a >M $d330 $71 >M $d331 $62 >M $d332 $71 >M
T{ op PC S A X Y P -> $d332 $b9 $20 $62 $39 $24 }T T{ $0062 M $0063 M $4460 M $d330 M $d331 M $d332 M -> $27 $44 $0a $71 $62 $71 }T
$2b77 >PC $29 >S $71 >A $40 >X $af >Y $e6 >P $007e $24 >M $007f $58 >M $2b77 $71 >M $2b78 $7e >M $2b79 $be >M $58d3 $09 >M
T{ op PC S A X Y P -> $2b79 $29 $7a $40 $af $24 }T T{ $007e M $007f M $2b77 M $2b78 M $2b79 M $58d3 M -> $24 $58 $71 $7e $be $09 }T
$752c >PC $49 >S $5b >A $fb >X $45 >Y $e4 >P $0033 $04 >M $0034 $c2 >M $752c $71 >M $752d $33 >M $752e $fe >M $c249 $25 >M
T{ op PC S A X Y P -> $752e $49 $80 $fb $45 $e4 }T T{ $0033 M $0034 M $752c M $752d M $752e M $c249 M -> $04 $c2 $71 $33 $fe $25 }T
$5aaf >PC $39 >S $9e >A $72 >X $4c >Y $a5 >P $0089 $06 >M $008a $30 >M $3052 $2a >M $5aaf $71 >M $5ab0 $89 >M $5ab1 $1b >M
T{ op PC S A X Y P -> $5ab1 $39 $c9 $72 $4c $a4 }T T{ $0089 M $008a M $3052 M $5aaf M $5ab0 M $5ab1 M -> $06 $30 $2a $71 $89 $1b }T
$5fd9 >PC $11 >S $ad >A $63 >X $06 >Y $e5 >P $00ab $7c >M $00ac $2c >M $2c82 $b3 >M $5fd9 $71 >M $5fda $ab >M $5fdb $17 >M
T{ op PC S A X Y P -> $5fdb $11 $61 $63 $06 $65 }T T{ $00ab M $00ac M $2c82 M $5fd9 M $5fda M $5fdb M -> $7c $2c $b3 $71 $ab $17 }T
$6a96 >PC $b3 >S $a1 >A $1e >X $d5 >Y $a7 >P $00fa $f8 >M $00fb $e0 >M $6a96 $71 >M $6a97 $fa >M $6a98 $fa >M $e1cd $a9 >M
T{ op PC S A X Y P -> $6a98 $b3 $4b $1e $d5 $65 }T T{ $00fa M $00fb M $6a96 M $6a97 M $6a98 M $e1cd M -> $f8 $e0 $71 $fa $fa $a9 }T
$d2bf >PC $a1 >S $a3 >A $fe >X $9b >Y $e0 >P $00b6 $88 >M $00b7 $9f >M $a023 $42 >M $d2bf $71 >M $d2c0 $b6 >M $d2c1 $70 >M
T{ op PC S A X Y P -> $d2c1 $a1 $e5 $fe $9b $a0 }T T{ $00b6 M $00b7 M $a023 M $d2bf M $d2c0 M $d2c1 M -> $88 $9f $42 $71 $b6 $70 }T
$07f9 >PC $b2 >S $73 >A $8e >X $ef >Y $26 >P $001f $70 >M $0020 $cd >M $07f9 $71 >M $07fa $1f >M $07fb $02 >M $ce5f $82 >M
T{ op PC S A X Y P -> $07fb $b2 $f5 $8e $ef $a4 }T T{ $001f M $0020 M $07f9 M $07fa M $07fb M $ce5f M -> $70 $cd $71 $1f $02 $82 }T
$824d >PC $9a >S $b8 >A $e0 >X $72 >Y $a5 >P $0052 $4e >M $0053 $53 >M $53c0 $ab >M $824d $71 >M $824e $52 >M $824f $ad >M
T{ op PC S A X Y P -> $824f $9a $64 $e0 $72 $65 }T T{ $0052 M $0053 M $53c0 M $824d M $824e M $824f M -> $4e $53 $ab $71 $52 $ad }T
( 72 )
$7906 >PC $7b >S $72 >A $1c >X $02 >Y $e3 >P $0021 $f5 >M $0022 $14 >M $14f5 $5b >M $7906 $72 >M $7907 $21 >M $7908 $d3 >M
T{ op PC S A X Y P -> $7908 $7b $ce $1c $02 $e0 }T T{ $0021 M $0022 M $14f5 M $7906 M $7907 M $7908 M -> $f5 $14 $5b $72 $21 $d3 }T
$ebb4 >PC $cc >S $01 >A $e0 >X $2c >Y $27 >P $000d $4a >M $000e $cb >M $cb4a $4c >M $ebb4 $72 >M $ebb5 $0d >M $ebb6 $06 >M
T{ op PC S A X Y P -> $ebb6 $cc $4e $e0 $2c $24 }T T{ $000d M $000e M $cb4a M $ebb4 M $ebb5 M $ebb6 M -> $4a $cb $4c $72 $0d $06 }T
$90bf >PC $a9 >S $ba >A $9a >X $6f >Y $a3 >P $0045 $f1 >M $0046 $24 >M $24f1 $a0 >M $90bf $72 >M $90c0 $45 >M $90c1 $be >M
T{ op PC S A X Y P -> $90c1 $a9 $5b $9a $6f $61 }T T{ $0045 M $0046 M $24f1 M $90bf M $90c0 M $90c1 M -> $f1 $24 $a0 $72 $45 $be }T
$d63c >PC $34 >S $61 >A $f6 >X $2e >Y $61 >P $0034 $17 >M $0035 $b5 >M $b517 $53 >M $d63c $72 >M $d63d $34 >M $d63e $ee >M
T{ op PC S A X Y P -> $d63e $34 $b5 $f6 $2e $e0 }T T{ $0034 M $0035 M $b517 M $d63c M $d63d M $d63e M -> $17 $b5 $53 $72 $34 $ee }T
$f3b9 >PC $78 >S $e5 >A $94 >X $0d >Y $26 >P $0028 $4e >M $0029 $e2 >M $e24e $6f >M $f3b9 $72 >M $f3ba $28 >M $f3bb $78 >M
T{ op PC S A X Y P -> $f3bb $78 $54 $94 $0d $25 }T T{ $0028 M $0029 M $e24e M $f3b9 M $f3ba M $f3bb M -> $4e $e2 $6f $72 $28 $78 }T
$298a >PC $13 >S $04 >A $ac >X $6d >Y $26 >P $00df $e8 >M $00e0 $c4 >M $298a $72 >M $298b $df >M $298c $9b >M $c4e8 $75 >M
T{ op PC S A X Y P -> $298c $13 $79 $ac $6d $24 }T T{ $00df M $00e0 M $298a M $298b M $298c M $c4e8 M -> $e8 $c4 $72 $df $9b $75 }T
$ecea >PC $65 >S $81 >A $f5 >X $b6 >Y $e6 >P $00d8 $41 >M $00d9 $9f >M $9f41 $b6 >M $ecea $72 >M $eceb $d8 >M $ecec $3e >M
T{ op PC S A X Y P -> $ecec $65 $37 $f5 $b6 $65 }T T{ $00d8 M $00d9 M $9f41 M $ecea M $eceb M $ecec M -> $41 $9f $b6 $72 $d8 $3e }T
$8403 >PC $12 >S $13 >A $bd >X $43 >Y $e2 >P $0090 $5f >M $0091 $50 >M $505f $18 >M $8403 $72 >M $8404 $90 >M $8405 $f5 >M
T{ op PC S A X Y P -> $8405 $12 $2b $bd $43 $20 }T T{ $0090 M $0091 M $505f M $8403 M $8404 M $8405 M -> $5f $50 $18 $72 $90 $f5 }T
$e9aa >PC $bd >S $b1 >A $8c >X $69 >Y $a3 >P $0081 $1f >M $0082 $34 >M $341f $64 >M $e9aa $72 >M $e9ab $81 >M $e9ac $22 >M
T{ op PC S A X Y P -> $e9ac $bd $16 $8c $69 $21 }T T{ $0081 M $0082 M $341f M $e9aa M $e9ab M $e9ac M -> $1f $34 $64 $72 $81 $22 }T
$77bb >PC $d5 >S $63 >A $7b >X $e1 >Y $24 >P $00a1 $d9 >M $00a2 $a7 >M $77bb $72 >M $77bc $a1 >M $77bd $15 >M $a7d9 $d9 >M
T{ op PC S A X Y P -> $77bd $d5 $3c $7b $e1 $25 }T T{ $00a1 M $00a2 M $77bb M $77bc M $77bd M $a7d9 M -> $d9 $a7 $72 $a1 $15 $d9 }T
$386e >PC $5a >S $4b >A $ec >X $b3 >Y $a4 >P $006a $03 >M $006b $18 >M $1803 $e4 >M $386e $72 >M $386f $6a >M $3870 $38 >M
T{ op PC S A X Y P -> $3870 $5a $2f $ec $b3 $25 }T T{ $006a M $006b M $1803 M $386e M $386f M $3870 M -> $03 $18 $e4 $72 $6a $38 }T
$0e52 >PC $fc >S $73 >A $1b >X $21 >Y $67 >P $009f $6c >M $00a0 $ee >M $0e52 $72 >M $0e53 $9f >M $0e54 $af >M $ee6c $4b >M
T{ op PC S A X Y P -> $0e54 $fc $bf $1b $21 $e4 }T T{ $009f M $00a0 M $0e52 M $0e53 M $0e54 M $ee6c M -> $6c $ee $72 $9f $af $4b }T
$a950 >PC $ef >S $52 >A $15 >X $96 >Y $62 >P $00bb $14 >M $00bc $3d >M $3d14 $de >M $a950 $72 >M $a951 $bb >M $a952 $31 >M
T{ op PC S A X Y P -> $a952 $ef $30 $15 $96 $21 }T T{ $00bb M $00bc M $3d14 M $a950 M $a951 M $a952 M -> $14 $3d $de $72 $bb $31 }T
$8fa2 >PC $92 >S $f6 >A $b2 >X $db >Y $a1 >P $00ab $36 >M $00ac $0c >M $0c36 $33 >M $8fa2 $72 >M $8fa3 $ab >M $8fa4 $aa >M
T{ op PC S A X Y P -> $8fa4 $92 $2a $b2 $db $21 }T T{ $00ab M $00ac M $0c36 M $8fa2 M $8fa3 M $8fa4 M -> $36 $0c $33 $72 $ab $aa }T
$fb86 >PC $47 >S $4e >A $3d >X $cd >Y $a2 >P $0031 $bd >M $0032 $7e >M $7ebd $57 >M $fb86 $72 >M $fb87 $31 >M $fb88 $00 >M
T{ op PC S A X Y P -> $fb88 $47 $a5 $3d $cd $e0 }T T{ $0031 M $0032 M $7ebd M $fb86 M $fb87 M $fb88 M -> $bd $7e $57 $72 $31 $00 }T
$7a8e >PC $e6 >S $30 >A $c8 >X $04 >Y $e4 >P $00a9 $af >M $00aa $71 >M $71af $cf >M $7a8e $72 >M $7a8f $a9 >M $7a90 $9b >M
T{ op PC S A X Y P -> $7a90 $e6 $ff $c8 $04 $a4 }T T{ $00a9 M $00aa M $71af M $7a8e M $7a8f M $7a90 M -> $af $71 $cf $72 $a9 $9b }T
( 73 )
$78e2 >PC $23 >S $90 >A $e0 >X $68 >Y $64 >P $78e2 $73 >M $78e3 $27 >M $78e4 $b6 >M
T{ op PC S A X Y P -> $78e3 $23 $90 $e0 $68 $64 }T T{ $78e2 M $78e3 M $78e4 M -> $73 $27 $b6 }T
$85a6 >PC $eb >S $bd >A $97 >X $c9 >Y $62 >P $85a6 $73 >M $85a7 $8d >M $85a8 $3c >M
T{ op PC S A X Y P -> $85a7 $eb $bd $97 $c9 $62 }T T{ $85a6 M $85a7 M $85a8 M -> $73 $8d $3c }T
$f107 >PC $a2 >S $f9 >A $7c >X $19 >Y $67 >P $f107 $73 >M $f108 $bc >M $f109 $7e >M
T{ op PC S A X Y P -> $f108 $a2 $f9 $7c $19 $67 }T T{ $f107 M $f108 M $f109 M -> $73 $bc $7e }T
$2e91 >PC $a6 >S $df >A $79 >X $e0 >Y $27 >P $2e91 $73 >M $2e92 $4b >M $2e93 $e0 >M
T{ op PC S A X Y P -> $2e92 $a6 $df $79 $e0 $27 }T T{ $2e91 M $2e92 M $2e93 M -> $73 $4b $e0 }T
$6eaf >PC $78 >S $f3 >A $96 >X $12 >Y $e5 >P $6eaf $73 >M $6eb0 $c5 >M $6eb1 $f7 >M
T{ op PC S A X Y P -> $6eb0 $78 $f3 $96 $12 $e5 }T T{ $6eaf M $6eb0 M $6eb1 M -> $73 $c5 $f7 }T
$452a >PC $9f >S $db >A $05 >X $9a >Y $61 >P $452a $73 >M $452b $36 >M $452c $89 >M
T{ op PC S A X Y P -> $452b $9f $db $05 $9a $61 }T T{ $452a M $452b M $452c M -> $73 $36 $89 }T
$ab0f >PC $f2 >S $37 >A $9e >X $ba >Y $a7 >P $ab0f $73 >M $ab10 $03 >M $ab11 $7d >M
T{ op PC S A X Y P -> $ab10 $f2 $37 $9e $ba $a7 }T T{ $ab0f M $ab10 M $ab11 M -> $73 $03 $7d }T
$84c5 >PC $da >S $5b >A $ed >X $80 >Y $e6 >P $84c5 $73 >M $84c6 $7c >M $84c7 $1a >M
T{ op PC S A X Y P -> $84c6 $da $5b $ed $80 $e6 }T T{ $84c5 M $84c6 M $84c7 M -> $73 $7c $1a }T
$87bb >PC $d1 >S $92 >A $fa >X $b0 >Y $e1 >P $87bb $73 >M $87bc $ad >M $87bd $76 >M
T{ op PC S A X Y P -> $87bc $d1 $92 $fa $b0 $e1 }T T{ $87bb M $87bc M $87bd M -> $73 $ad $76 }T
$0673 >PC $09 >S $b6 >A $18 >X $a3 >Y $22 >P $0673 $73 >M $0674 $c2 >M $0675 $84 >M
T{ op PC S A X Y P -> $0674 $09 $b6 $18 $a3 $22 }T T{ $0673 M $0674 M $0675 M -> $73 $c2 $84 }T
$8ba8 >PC $07 >S $96 >A $e6 >X $45 >Y $e5 >P $8ba8 $73 >M $8ba9 $86 >M $8baa $c4 >M
T{ op PC S A X Y P -> $8ba9 $07 $96 $e6 $45 $e5 }T T{ $8ba8 M $8ba9 M $8baa M -> $73 $86 $c4 }T
$1fd4 >PC $f1 >S $92 >A $b0 >X $66 >Y $64 >P $1fd4 $73 >M $1fd5 $78 >M $1fd6 $96 >M
T{ op PC S A X Y P -> $1fd5 $f1 $92 $b0 $66 $64 }T T{ $1fd4 M $1fd5 M $1fd6 M -> $73 $78 $96 }T
$4a75 >PC $09 >S $81 >A $35 >X $ef >Y $a0 >P $4a75 $73 >M $4a76 $51 >M $4a77 $58 >M
T{ op PC S A X Y P -> $4a76 $09 $81 $35 $ef $a0 }T T{ $4a75 M $4a76 M $4a77 M -> $73 $51 $58 }T
$bd1d >PC $6f >S $7e >A $52 >X $0f >Y $60 >P $bd1d $73 >M $bd1e $c1 >M $bd1f $2f >M
T{ op PC S A X Y P -> $bd1e $6f $7e $52 $0f $60 }T T{ $bd1d M $bd1e M $bd1f M -> $73 $c1 $2f }T
$2c0f >PC $0f >S $9e >A $7d >X $55 >Y $25 >P $2c0f $73 >M $2c10 $d3 >M $2c11 $7f >M
T{ op PC S A X Y P -> $2c10 $0f $9e $7d $55 $25 }T T{ $2c0f M $2c10 M $2c11 M -> $73 $d3 $7f }T
$c17b >PC $33 >S $fd >A $57 >X $c5 >Y $20 >P $c17b $73 >M $c17c $23 >M $c17d $5d >M
T{ op PC S A X Y P -> $c17c $33 $fd $57 $c5 $20 }T T{ $c17b M $c17c M $c17d M -> $73 $23 $5d }T
( 74 )
$def2 >PC $b3 >S $89 >A $77 >X $ce >Y $20 >P $00b2 $f4 >M $def2 $74 >M $def3 $b2 >M $def4 $80 >M
T{ op PC S A X Y P -> $def4 $b3 $89 $77 $ce $20 }T T{ $0029 M $00b2 M $def2 M $def3 M $def4 M -> $00 $f4 $74 $b2 $80 }T
$1c25 >PC $01 >S $65 >A $c0 >X $44 >Y $e1 >P $0079 $d2 >M $1c25 $74 >M $1c26 $79 >M $1c27 $f8 >M
T{ op PC S A X Y P -> $1c27 $01 $65 $c0 $44 $e1 }T T{ $0039 M $0079 M $1c25 M $1c26 M $1c27 M -> $00 $d2 $74 $79 $f8 }T
$8be3 >PC $21 >S $e1 >A $fc >X $39 >Y $a5 >P $00d0 $22 >M $8be3 $74 >M $8be4 $d0 >M $8be5 $95 >M
T{ op PC S A X Y P -> $8be5 $21 $e1 $fc $39 $a5 }T T{ $00cc M $00d0 M $8be3 M $8be4 M $8be5 M -> $00 $22 $74 $d0 $95 }T
$2f02 >PC $2a >S $53 >A $b1 >X $22 >Y $e2 >P $0088 $68 >M $2f02 $74 >M $2f03 $88 >M $2f04 $94 >M
T{ op PC S A X Y P -> $2f04 $2a $53 $b1 $22 $e2 }T T{ $0039 M $0088 M $2f02 M $2f03 M $2f04 M -> $00 $68 $74 $88 $94 }T
$bd85 >PC $c1 >S $51 >A $69 >X $15 >Y $e6 >P $0080 $aa >M $bd85 $74 >M $bd86 $80 >M $bd87 $67 >M
T{ op PC S A X Y P -> $bd87 $c1 $51 $69 $15 $e6 }T T{ $0080 M $00e9 M $bd85 M $bd86 M $bd87 M -> $aa $00 $74 $80 $67 }T
$e576 >PC $e7 >S $46 >A $2e >X $0e >Y $23 >P $0044 $c7 >M $e576 $74 >M $e577 $44 >M $e578 $76 >M
T{ op PC S A X Y P -> $e578 $e7 $46 $2e $0e $23 }T T{ $0044 M $0072 M $e576 M $e577 M $e578 M -> $c7 $00 $74 $44 $76 }T
$0882 >PC $ee >S $f5 >A $e3 >X $c1 >Y $a2 >P $001a $89 >M $0882 $74 >M $0883 $1a >M $0884 $90 >M
T{ op PC S A X Y P -> $0884 $ee $f5 $e3 $c1 $a2 }T T{ $001a M $00fd M $0882 M $0883 M $0884 M -> $89 $00 $74 $1a $90 }T
$6303 >PC $af >S $89 >A $c2 >X $6d >Y $a6 >P $0048 $c3 >M $6303 $74 >M $6304 $48 >M $6305 $17 >M
T{ op PC S A X Y P -> $6305 $af $89 $c2 $6d $a6 }T T{ $000a M $0048 M $6303 M $6304 M $6305 M -> $00 $c3 $74 $48 $17 }T
$2e5e >PC $65 >S $f0 >A $db >X $ab >Y $e4 >P $00bc $a7 >M $2e5e $74 >M $2e5f $bc >M $2e60 $06 >M
T{ op PC S A X Y P -> $2e60 $65 $f0 $db $ab $e4 }T T{ $0097 M $00bc M $2e5e M $2e5f M $2e60 M -> $00 $a7 $74 $bc $06 }T
$a149 >PC $13 >S $14 >A $40 >X $ab >Y $a3 >P $0004 $c5 >M $a149 $74 >M $a14a $04 >M $a14b $9f >M
T{ op PC S A X Y P -> $a14b $13 $14 $40 $ab $a3 }T T{ $0004 M $0044 M $a149 M $a14a M $a14b M -> $c5 $00 $74 $04 $9f }T
$4781 >PC $f4 >S $64 >A $6b >X $51 >Y $a3 >P $002c $13 >M $4781 $74 >M $4782 $2c >M $4783 $95 >M
T{ op PC S A X Y P -> $4783 $f4 $64 $6b $51 $a3 }T T{ $002c M $0097 M $4781 M $4782 M $4783 M -> $13 $00 $74 $2c $95 }T
$9513 >PC $a1 >S $65 >A $57 >X $d7 >Y $e1 >P $0037 $fb >M $9513 $74 >M $9514 $37 >M $9515 $4f >M
T{ op PC S A X Y P -> $9515 $a1 $65 $57 $d7 $e1 }T T{ $0037 M $008e M $9513 M $9514 M $9515 M -> $fb $00 $74 $37 $4f }T
$691f >PC $04 >S $a2 >A $db >X $7f >Y $62 >P $0053 $eb >M $691f $74 >M $6920 $53 >M $6921 $79 >M
T{ op PC S A X Y P -> $6921 $04 $a2 $db $7f $62 }T T{ $002e M $0053 M $691f M $6920 M $6921 M -> $00 $eb $74 $53 $79 }T
$2059 >PC $c7 >S $82 >A $5f >X $0a >Y $e2 >P $003e $a6 >M $2059 $74 >M $205a $3e >M $205b $94 >M
T{ op PC S A X Y P -> $205b $c7 $82 $5f $0a $e2 }T T{ $003e M $009d M $2059 M $205a M $205b M -> $a6 $00 $74 $3e $94 }T
$a6da >PC $81 >S $11 >A $4e >X $38 >Y $e6 >P $001c $de >M $a6da $74 >M $a6db $1c >M $a6dc $86 >M
T{ op PC S A X Y P -> $a6dc $81 $11 $4e $38 $e6 }T T{ $001c M $006a M $a6da M $a6db M $a6dc M -> $de $00 $74 $1c $86 }T
$da57 >PC $33 >S $ba >A $1c >X $ae >Y $62 >P $00cf $75 >M $da57 $74 >M $da58 $cf >M $da59 $5a >M
T{ op PC S A X Y P -> $da59 $33 $ba $1c $ae $62 }T T{ $00cf M $00eb M $da57 M $da58 M $da59 M -> $75 $00 $74 $cf $5a }T
( 75 )
$493c >PC $7f >S $df >A $4b >X $e9 >Y $a4 >P $00ad $b5 >M $00f8 $2b >M $493c $75 >M $493d $ad >M $493e $c9 >M
T{ op PC S A X Y P -> $493e $7f $0a $4b $e9 $25 }T T{ $00ad M $00f8 M $493c M $493d M $493e M -> $b5 $2b $75 $ad $c9 }T
$ea63 >PC $89 >S $b9 >A $f6 >X $88 >Y $22 >P $006d $da >M $0077 $32 >M $ea63 $75 >M $ea64 $77 >M $ea65 $5f >M
T{ op PC S A X Y P -> $ea65 $89 $93 $f6 $88 $a1 }T T{ $006d M $0077 M $ea63 M $ea64 M $ea65 M -> $da $32 $75 $77 $5f }T
$8a42 >PC $70 >S $78 >A $a7 >X $50 >Y $61 >P $0081 $a5 >M $00da $f3 >M $8a42 $75 >M $8a43 $da >M $8a44 $3e >M
T{ op PC S A X Y P -> $8a44 $70 $1e $a7 $50 $21 }T T{ $0081 M $00da M $8a42 M $8a43 M $8a44 M -> $a5 $f3 $75 $da $3e }T
$b841 >PC $86 >S $b9 >A $a6 >X $97 >Y $60 >P $00a2 $57 >M $00fc $1e >M $b841 $75 >M $b842 $fc >M $b843 $e4 >M
T{ op PC S A X Y P -> $b843 $86 $10 $a6 $97 $21 }T T{ $00a2 M $00fc M $b841 M $b842 M $b843 M -> $57 $1e $75 $fc $e4 }T
$ee5e >PC $b7 >S $75 >A $96 >X $57 >Y $e2 >P $0050 $2e >M $00ba $86 >M $ee5e $75 >M $ee5f $ba >M $ee60 $10 >M
T{ op PC S A X Y P -> $ee60 $b7 $a3 $96 $57 $e0 }T T{ $0050 M $00ba M $ee5e M $ee5f M $ee60 M -> $2e $86 $75 $ba $10 }T
$9a61 >PC $7a >S $73 >A $94 >X $d0 >Y $a7 >P $0011 $43 >M $007d $3d >M $9a61 $75 >M $9a62 $7d >M $9a63 $37 >M
T{ op PC S A X Y P -> $9a63 $7a $b7 $94 $d0 $e4 }T T{ $0011 M $007d M $9a61 M $9a62 M $9a63 M -> $43 $3d $75 $7d $37 }T
$201e >PC $27 >S $7f >A $f3 >X $1d >Y $e0 >P $0095 $4d >M $00a2 $0f >M $201e $75 >M $201f $a2 >M $2020 $95 >M
T{ op PC S A X Y P -> $2020 $27 $cc $f3 $1d $e0 }T T{ $0095 M $00a2 M $201e M $201f M $2020 M -> $4d $0f $75 $a2 $95 }T
$f865 >PC $e8 >S $ce >A $88 >X $9b >Y $26 >P $0073 $bd >M $00eb $7b >M $f865 $75 >M $f866 $eb >M $f867 $f5 >M
T{ op PC S A X Y P -> $f867 $e8 $8b $88 $9b $a5 }T T{ $0073 M $00eb M $f865 M $f866 M $f867 M -> $bd $7b $75 $eb $f5 }T
$ca93 >PC $37 >S $ce >A $65 >X $ca >Y $63 >P $0090 $63 >M $00f5 $14 >M $ca93 $75 >M $ca94 $90 >M $ca95 $36 >M
T{ op PC S A X Y P -> $ca95 $37 $e3 $65 $ca $a0 }T T{ $0090 M $00f5 M $ca93 M $ca94 M $ca95 M -> $63 $14 $75 $90 $36 }T
$e69e >PC $64 >S $24 >A $96 >X $b3 >Y $20 >P $0021 $f6 >M $00b7 $f3 >M $e69e $75 >M $e69f $21 >M $e6a0 $f6 >M
T{ op PC S A X Y P -> $e6a0 $64 $17 $96 $b3 $21 }T T{ $0021 M $00b7 M $e69e M $e69f M $e6a0 M -> $f6 $f3 $75 $21 $f6 }T
$a98d >PC $a1 >S $32 >A $c3 >X $3c >Y $e1 >P $0001 $72 >M $00c4 $3e >M $a98d $75 >M $a98e $01 >M $a98f $8f >M
T{ op PC S A X Y P -> $a98f $a1 $71 $c3 $3c $20 }T T{ $0001 M $00c4 M $a98d M $a98e M $a98f M -> $72 $3e $75 $01 $8f }T
$30aa >PC $98 >S $2e >A $4e >X $01 >Y $a7 >P $008e $88 >M $00dc $22 >M $30aa $75 >M $30ab $8e >M $30ac $39 >M
T{ op PC S A X Y P -> $30ac $98 $51 $4e $01 $24 }T T{ $008e M $00dc M $30aa M $30ab M $30ac M -> $88 $22 $75 $8e $39 }T
$0df0 >PC $03 >S $39 >A $63 >X $b0 >Y $e2 >P $007a $38 >M $00dd $7f >M $0df0 $75 >M $0df1 $7a >M $0df2 $bf >M
T{ op PC S A X Y P -> $0df2 $03 $b8 $63 $b0 $e0 }T T{ $007a M $00dd M $0df0 M $0df1 M $0df2 M -> $38 $7f $75 $7a $bf }T
$558f >PC $68 >S $74 >A $43 >X $10 >Y $a2 >P $001a $a9 >M $00d7 $20 >M $558f $75 >M $5590 $d7 >M $5591 $2c >M
T{ op PC S A X Y P -> $5591 $68 $1d $43 $10 $21 }T T{ $001a M $00d7 M $558f M $5590 M $5591 M -> $a9 $20 $75 $d7 $2c }T
$08f5 >PC $22 >S $82 >A $53 >X $da >Y $63 >P $0067 $01 >M $00ba $5f >M $08f5 $75 >M $08f6 $67 >M $08f7 $84 >M
T{ op PC S A X Y P -> $08f7 $22 $e2 $53 $da $a0 }T T{ $0067 M $00ba M $08f5 M $08f6 M $08f7 M -> $01 $5f $75 $67 $84 }T
$2a10 >PC $d8 >S $e8 >A $d1 >X $03 >Y $a7 >P $00ad $d2 >M $00dc $29 >M $2a10 $75 >M $2a11 $dc >M $2a12 $15 >M
T{ op PC S A X Y P -> $2a12 $d8 $bb $d1 $03 $a5 }T T{ $00ad M $00dc M $2a10 M $2a11 M $2a12 M -> $d2 $29 $75 $dc $15 }T
( 76 )
$4861 >PC $64 >S $91 >A $3e >X $f5 >Y $62 >P $0015 $ff >M $0053 $d0 >M $4861 $76 >M $4862 $15 >M $4863 $6f >M
T{ op PC S A X Y P -> $4863 $64 $91 $3e $f5 $60 }T T{ $0015 M $0053 M $4861 M $4862 M $4863 M -> $ff $68 $76 $15 $6f }T
$791e >PC $c8 >S $9d >A $5c >X $83 >Y $e0 >P $002a $88 >M $00ce $55 >M $791e $76 >M $791f $ce >M $7920 $7f >M
T{ op PC S A X Y P -> $7920 $c8 $9d $5c $83 $60 }T T{ $002a M $00ce M $791e M $791f M $7920 M -> $44 $55 $76 $ce $7f }T
$8a8b >PC $e8 >S $69 >A $25 >X $ab >Y $21 >P $004a $b4 >M $006f $6a >M $8a8b $76 >M $8a8c $4a >M $8a8d $b9 >M
T{ op PC S A X Y P -> $8a8d $e8 $69 $25 $ab $a0 }T T{ $004a M $006f M $8a8b M $8a8c M $8a8d M -> $b4 $b5 $76 $4a $b9 }T
$c02f >PC $2c >S $22 >A $23 >X $fc >Y $21 >P $003f $e4 >M $0062 $9e >M $c02f $76 >M $c030 $3f >M $c031 $13 >M
T{ op PC S A X Y P -> $c031 $2c $22 $23 $fc $a0 }T T{ $003f M $0062 M $c02f M $c030 M $c031 M -> $e4 $cf $76 $3f $13 }T
$b568 >PC $4a >S $dc >A $bf >X $b8 >Y $e0 >P $003d $73 >M $00fc $b4 >M $b568 $76 >M $b569 $3d >M $b56a $47 >M
T{ op PC S A X Y P -> $b56a $4a $dc $bf $b8 $60 }T T{ $003d M $00fc M $b568 M $b569 M $b56a M -> $73 $5a $76 $3d $47 }T
$dbb4 >PC $7d >S $50 >A $b8 >X $57 >Y $26 >P $004a $e0 >M $0092 $d5 >M $dbb4 $76 >M $dbb5 $92 >M $dbb6 $63 >M
T{ op PC S A X Y P -> $dbb6 $7d $50 $b8 $57 $24 }T T{ $004a M $0092 M $dbb4 M $dbb5 M $dbb6 M -> $70 $d5 $76 $92 $63 }T
$aa11 >PC $d3 >S $98 >A $da >X $2e >Y $65 >P $004d $1f >M $0073 $34 >M $aa11 $76 >M $aa12 $73 >M $aa13 $61 >M
T{ op PC S A X Y P -> $aa13 $d3 $98 $da $2e $e5 }T T{ $004d M $0073 M $aa11 M $aa12 M $aa13 M -> $8f $34 $76 $73 $61 }T
$4b9e >PC $c9 >S $f1 >A $c3 >X $ca >Y $24 >P $0079 $42 >M $00b6 $0a >M $4b9e $76 >M $4b9f $b6 >M $4ba0 $cf >M
T{ op PC S A X Y P -> $4ba0 $c9 $f1 $c3 $ca $24 }T T{ $0079 M $00b6 M $4b9e M $4b9f M $4ba0 M -> $21 $0a $76 $b6 $cf }T
$8ac7 >PC $e8 >S $d9 >A $8e >X $97 >Y $27 >P $001c $77 >M $00aa $8e >M $8ac7 $76 >M $8ac8 $1c >M $8ac9 $b5 >M
T{ op PC S A X Y P -> $8ac9 $e8 $d9 $8e $97 $a4 }T T{ $001c M $00aa M $8ac7 M $8ac8 M $8ac9 M -> $77 $c7 $76 $1c $b5 }T
$e3eb >PC $ab >S $d6 >A $d9 >X $1f >Y $a5 >P $0013 $90 >M $00ec $61 >M $e3eb $76 >M $e3ec $13 >M $e3ed $21 >M
T{ op PC S A X Y P -> $e3ed $ab $d6 $d9 $1f $a5 }T T{ $0013 M $00ec M $e3eb M $e3ec M $e3ed M -> $90 $b0 $76 $13 $21 }T
$f6b7 >PC $25 >S $c6 >A $39 >X $08 >Y $27 >P $002d $35 >M $0066 $af >M $f6b7 $76 >M $f6b8 $2d >M $f6b9 $07 >M
T{ op PC S A X Y P -> $f6b9 $25 $c6 $39 $08 $a5 }T T{ $002d M $0066 M $f6b7 M $f6b8 M $f6b9 M -> $35 $d7 $76 $2d $07 }T
$4d42 >PC $51 >S $a9 >A $c1 >X $ad >Y $23 >P $009b $76 >M $00da $97 >M $4d42 $76 >M $4d43 $da >M $4d44 $ec >M
T{ op PC S A X Y P -> $4d44 $51 $a9 $c1 $ad $a0 }T T{ $009b M $00da M $4d42 M $4d43 M $4d44 M -> $bb $97 $76 $da $ec }T
$c7f9 >PC $44 >S $76 >A $af >X $26 >Y $a5 >P $001e $df >M $006f $5e >M $c7f9 $76 >M $c7fa $6f >M $c7fb $f2 >M
T{ op PC S A X Y P -> $c7fb $44 $76 $af $26 $a5 }T T{ $001e M $006f M $c7f9 M $c7fa M $c7fb M -> $ef $5e $76 $6f $f2 }T
$f0d4 >PC $49 >S $3e >A $15 >X $f4 >Y $22 >P $0077 $65 >M $008c $02 >M $f0d4 $76 >M $f0d5 $77 >M $f0d6 $da >M
T{ op PC S A X Y P -> $f0d6 $49 $3e $15 $f4 $20 }T T{ $0077 M $008c M $f0d4 M $f0d5 M $f0d6 M -> $65 $01 $76 $77 $da }T
$42f9 >PC $d5 >S $dd >A $69 >X $d1 >Y $a0 >P $0009 $58 >M $00a0 $61 >M $42f9 $76 >M $42fa $a0 >M $42fb $38 >M
T{ op PC S A X Y P -> $42fb $d5 $dd $69 $d1 $20 }T T{ $0009 M $00a0 M $42f9 M $42fa M $42fb M -> $2c $61 $76 $a0 $38 }T
$6bd2 >PC $17 >S $d4 >A $e7 >X $fc >Y $63 >P $0011 $9d >M $00f8 $c9 >M $6bd2 $76 >M $6bd3 $11 >M $6bd4 $dd >M
T{ op PC S A X Y P -> $6bd4 $17 $d4 $e7 $fc $e1 }T T{ $0011 M $00f8 M $6bd2 M $6bd3 M $6bd4 M -> $9d $e4 $76 $11 $dd }T
( 77 )
$6a0a >PC $5c >S $10 >A $9b >X $62 >Y $23 >P $0095 $d4 >M $6a0a $77 >M $6a0b $95 >M $6a0c $46 >M
T{ op PC S A X Y P -> $6a0c $5c $10 $9b $62 $23 }T T{ $0095 M $6a0a M $6a0b M $6a0c M -> $54 $77 $95 $46 }T
$f020 >PC $2b >S $28 >A $a2 >X $18 >Y $a2 >P $00b9 $a0 >M $f020 $77 >M $f021 $b9 >M $f022 $2c >M
T{ op PC S A X Y P -> $f022 $2b $28 $a2 $18 $a2 }T T{ $00b9 M $f020 M $f021 M $f022 M -> $20 $77 $b9 $2c }T
$5e9e >PC $ca >S $50 >A $32 >X $dc >Y $27 >P $00a0 $8e >M $5e9e $77 >M $5e9f $a0 >M $5ea0 $45 >M
T{ op PC S A X Y P -> $5ea0 $ca $50 $32 $dc $27 }T T{ $00a0 M $5e9e M $5e9f M $5ea0 M -> $0e $77 $a0 $45 }T
$bbf7 >PC $13 >S $14 >A $e4 >X $51 >Y $64 >P $00a4 $85 >M $bbf7 $77 >M $bbf8 $a4 >M $bbf9 $e0 >M
T{ op PC S A X Y P -> $bbf9 $13 $14 $e4 $51 $64 }T T{ $00a4 M $bbf7 M $bbf8 M $bbf9 M -> $05 $77 $a4 $e0 }T
$ac43 >PC $cb >S $c4 >A $20 >X $29 >Y $22 >P $0099 $da >M $ac43 $77 >M $ac44 $99 >M $ac45 $fb >M
T{ op PC S A X Y P -> $ac45 $cb $c4 $20 $29 $22 }T T{ $0099 M $ac43 M $ac44 M $ac45 M -> $5a $77 $99 $fb }T
$23e3 >PC $22 >S $11 >A $5a >X $f1 >Y $64 >P $00b0 $52 >M $23e3 $77 >M $23e4 $b0 >M $23e5 $bb >M
T{ op PC S A X Y P -> $23e5 $22 $11 $5a $f1 $64 }T T{ $00b0 M $23e3 M $23e4 M $23e5 M -> $52 $77 $b0 $bb }T
$53a0 >PC $ec >S $ab >A $a0 >X $df >Y $e3 >P $00d3 $88 >M $53a0 $77 >M $53a1 $d3 >M $53a2 $fb >M
T{ op PC S A X Y P -> $53a2 $ec $ab $a0 $df $e3 }T T{ $00d3 M $53a0 M $53a1 M $53a2 M -> $08 $77 $d3 $fb }T
$47a4 >PC $bb >S $2e >A $50 >X $09 >Y $60 >P $005e $4e >M $47a4 $77 >M $47a5 $5e >M $47a6 $38 >M
T{ op PC S A X Y P -> $47a6 $bb $2e $50 $09 $60 }T T{ $005e M $47a4 M $47a5 M $47a6 M -> $4e $77 $5e $38 }T
$701f >PC $53 >S $aa >A $84 >X $86 >Y $a4 >P $0087 $d8 >M $701f $77 >M $7020 $87 >M $7021 $7b >M
T{ op PC S A X Y P -> $7021 $53 $aa $84 $86 $a4 }T T{ $0087 M $701f M $7020 M $7021 M -> $58 $77 $87 $7b }T
$8b4c >PC $48 >S $e2 >A $72 >X $7d >Y $a2 >P $00bf $fe >M $8b4c $77 >M $8b4d $bf >M $8b4e $45 >M
T{ op PC S A X Y P -> $8b4e $48 $e2 $72 $7d $a2 }T T{ $00bf M $8b4c M $8b4d M $8b4e M -> $7e $77 $bf $45 }T
$d013 >PC $41 >S $a8 >A $61 >X $07 >Y $a5 >P $00b4 $98 >M $d013 $77 >M $d014 $b4 >M $d015 $5d >M
T{ op PC S A X Y P -> $d015 $41 $a8 $61 $07 $a5 }T T{ $00b4 M $d013 M $d014 M $d015 M -> $18 $77 $b4 $5d }T
$dcd1 >PC $b3 >S $21 >A $cd >X $af >Y $60 >P $0026 $c0 >M $dcd1 $77 >M $dcd2 $26 >M $dcd3 $84 >M
T{ op PC S A X Y P -> $dcd3 $b3 $21 $cd $af $60 }T T{ $0026 M $dcd1 M $dcd2 M $dcd3 M -> $40 $77 $26 $84 }T
$eab6 >PC $2b >S $9c >A $06 >X $9c >Y $a4 >P $0076 $64 >M $eab6 $77 >M $eab7 $76 >M $eab8 $b8 >M
T{ op PC S A X Y P -> $eab8 $2b $9c $06 $9c $a4 }T T{ $0076 M $eab6 M $eab7 M $eab8 M -> $64 $77 $76 $b8 }T
$1882 >PC $3c >S $2c >A $1f >X $ea >Y $a7 >P $00a7 $c6 >M $1882 $77 >M $1883 $a7 >M $1884 $7b >M
T{ op PC S A X Y P -> $1884 $3c $2c $1f $ea $a7 }T T{ $00a7 M $1882 M $1883 M $1884 M -> $46 $77 $a7 $7b }T
$d7a0 >PC $b4 >S $63 >A $7a >X $8b >Y $23 >P $00e6 $48 >M $d7a0 $77 >M $d7a1 $e6 >M $d7a2 $92 >M
T{ op PC S A X Y P -> $d7a2 $b4 $63 $7a $8b $23 }T T{ $00e6 M $d7a0 M $d7a1 M $d7a2 M -> $48 $77 $e6 $92 }T
$21e5 >PC $ff >S $56 >A $9e >X $03 >Y $a3 >P $003a $ba >M $21e5 $77 >M $21e6 $3a >M $21e7 $61 >M
T{ op PC S A X Y P -> $21e7 $ff $56 $9e $03 $a3 }T T{ $003a M $21e5 M $21e6 M $21e7 M -> $3a $77 $3a $61 }T
( 78 )
$193d >PC $f8 >S $87 >A $32 >X $d1 >Y $a6 >P $193d $78 >M $193e $16 >M $193f $56 >M
T{ op PC S A X Y P -> $193e $f8 $87 $32 $d1 $a6 }T T{ $193d M $193e M $193f M -> $78 $16 $56 }T
$1d3a >PC $74 >S $ec >A $c4 >X $c2 >Y $25 >P $1d3a $78 >M $1d3b $fe >M $1d3c $e9 >M
T{ op PC S A X Y P -> $1d3b $74 $ec $c4 $c2 $25 }T T{ $1d3a M $1d3b M $1d3c M -> $78 $fe $e9 }T
$7847 >PC $d8 >S $a5 >A $64 >X $3d >Y $27 >P $7847 $78 >M $7848 $f9 >M $7849 $1d >M
T{ op PC S A X Y P -> $7848 $d8 $a5 $64 $3d $27 }T T{ $7847 M $7848 M $7849 M -> $78 $f9 $1d }T
$364f >PC $7c >S $7e >A $75 >X $df >Y $64 >P $364f $78 >M $3650 $e3 >M $3651 $f8 >M
T{ op PC S A X Y P -> $3650 $7c $7e $75 $df $64 }T T{ $364f M $3650 M $3651 M -> $78 $e3 $f8 }T
$58c3 >PC $71 >S $92 >A $2b >X $8d >Y $26 >P $58c3 $78 >M $58c4 $4c >M $58c5 $c7 >M
T{ op PC S A X Y P -> $58c4 $71 $92 $2b $8d $26 }T T{ $58c3 M $58c4 M $58c5 M -> $78 $4c $c7 }T
$43f4 >PC $c4 >S $7f >A $97 >X $86 >Y $60 >P $43f4 $78 >M $43f5 $8b >M $43f6 $71 >M
T{ op PC S A X Y P -> $43f5 $c4 $7f $97 $86 $64 }T T{ $43f4 M $43f5 M $43f6 M -> $78 $8b $71 }T
$6a5b >PC $35 >S $77 >A $70 >X $0e >Y $e5 >P $6a5b $78 >M $6a5c $37 >M $6a5d $b7 >M
T{ op PC S A X Y P -> $6a5c $35 $77 $70 $0e $e5 }T T{ $6a5b M $6a5c M $6a5d M -> $78 $37 $b7 }T
$947c >PC $a6 >S $75 >A $db >X $8d >Y $65 >P $947c $78 >M $947d $30 >M $947e $12 >M
T{ op PC S A X Y P -> $947d $a6 $75 $db $8d $65 }T T{ $947c M $947d M $947e M -> $78 $30 $12 }T
$43f1 >PC $7b >S $d5 >A $65 >X $73 >Y $e0 >P $43f1 $78 >M $43f2 $11 >M $43f3 $d0 >M
T{ op PC S A X Y P -> $43f2 $7b $d5 $65 $73 $e4 }T T{ $43f1 M $43f2 M $43f3 M -> $78 $11 $d0 }T
$4dfb >PC $ad >S $8e >A $6a >X $79 >Y $24 >P $4dfb $78 >M $4dfc $36 >M $4dfd $1a >M
T{ op PC S A X Y P -> $4dfc $ad $8e $6a $79 $24 }T T{ $4dfb M $4dfc M $4dfd M -> $78 $36 $1a }T
$d0f0 >PC $a7 >S $ce >A $c7 >X $0c >Y $65 >P $d0f0 $78 >M $d0f1 $9e >M $d0f2 $21 >M
T{ op PC S A X Y P -> $d0f1 $a7 $ce $c7 $0c $65 }T T{ $d0f0 M $d0f1 M $d0f2 M -> $78 $9e $21 }T
$d794 >PC $f3 >S $2b >A $ae >X $4e >Y $27 >P $d794 $78 >M $d795 $f9 >M $d796 $0a >M
T{ op PC S A X Y P -> $d795 $f3 $2b $ae $4e $27 }T T{ $d794 M $d795 M $d796 M -> $78 $f9 $0a }T
$458d >PC $05 >S $0a >A $c1 >X $44 >Y $a4 >P $458d $78 >M $458e $13 >M $458f $26 >M
T{ op PC S A X Y P -> $458e $05 $0a $c1 $44 $a4 }T T{ $458d M $458e M $458f M -> $78 $13 $26 }T
$4fd4 >PC $0b >S $fb >A $aa >X $19 >Y $e7 >P $4fd4 $78 >M $4fd5 $1d >M $4fd6 $79 >M
T{ op PC S A X Y P -> $4fd5 $0b $fb $aa $19 $e7 }T T{ $4fd4 M $4fd5 M $4fd6 M -> $78 $1d $79 }T
$80c5 >PC $f8 >S $f0 >A $3a >X $26 >Y $62 >P $80c5 $78 >M $80c6 $9d >M $80c7 $56 >M
T{ op PC S A X Y P -> $80c6 $f8 $f0 $3a $26 $66 }T T{ $80c5 M $80c6 M $80c7 M -> $78 $9d $56 }T
$083b >PC $a2 >S $6e >A $dd >X $df >Y $22 >P $083b $78 >M $083c $32 >M $083d $31 >M
T{ op PC S A X Y P -> $083c $a2 $6e $dd $df $26 }T T{ $083b M $083c M $083d M -> $78 $32 $31 }T
( 79 )
$c5ad >PC $da >S $de >A $b7 >X $da >Y $65 >P $c5ad $79 >M $c5ae $82 >M $c5af $cc >M $c5b0 $00 >M $cd5c $ad >M
T{ op PC S A X Y P -> $c5b0 $da $8c $b7 $da $a5 }T T{ $c5ad M $c5ae M $c5af M $c5b0 M $cd5c M -> $79 $82 $cc $00 $ad }T
$32fd >PC $fe >S $94 >A $36 >X $3b >Y $e6 >P $2f00 $b6 >M $32fd $79 >M $32fe $c5 >M $32ff $2e >M $3300 $c4 >M
T{ op PC S A X Y P -> $3300 $fe $4a $36 $3b $65 }T T{ $2f00 M $32fd M $32fe M $32ff M $3300 M -> $b6 $79 $c5 $2e $c4 }T
$a75a >PC $d9 >S $6c >A $09 >X $55 >Y $a5 >P $0e90 $93 >M $a75a $79 >M $a75b $3b >M $a75c $0e >M $a75d $ac >M
T{ op PC S A X Y P -> $a75d $d9 $00 $09 $55 $27 }T T{ $0e90 M $a75a M $a75b M $a75c M $a75d M -> $93 $79 $3b $0e $ac }T
$c0de >PC $ef >S $83 >A $bd >X $ef >Y $a7 >P $1bf4 $b6 >M $c0de $79 >M $c0df $05 >M $c0e0 $1b >M $c0e1 $99 >M
T{ op PC S A X Y P -> $c0e1 $ef $3a $bd $ef $65 }T T{ $1bf4 M $c0de M $c0df M $c0e0 M $c0e1 M -> $b6 $79 $05 $1b $99 }T
$68ef >PC $02 >S $eb >A $16 >X $91 >Y $26 >P $68ef $79 >M $68f0 $4f >M $68f1 $b6 >M $68f2 $74 >M $b6e0 $a6 >M
T{ op PC S A X Y P -> $68f2 $02 $91 $16 $91 $a5 }T T{ $68ef M $68f0 M $68f1 M $68f2 M $b6e0 M -> $79 $4f $b6 $74 $a6 }T
$1628 >PC $d2 >S $66 >A $cd >X $33 >Y $e6 >P $1628 $79 >M $1629 $09 >M $162a $f9 >M $162b $ad >M $f93c $12 >M
T{ op PC S A X Y P -> $162b $d2 $78 $cd $33 $24 }T T{ $1628 M $1629 M $162a M $162b M $f93c M -> $79 $09 $f9 $ad $12 }T
$2817 >PC $f4 >S $03 >A $2d >X $f2 >Y $63 >P $2817 $79 >M $2818 $2e >M $2819 $6a >M $281a $8c >M $6b20 $d6 >M
T{ op PC S A X Y P -> $281a $f4 $da $2d $f2 $a0 }T T{ $2817 M $2818 M $2819 M $281a M $6b20 M -> $79 $2e $6a $8c $d6 }T
$5389 >PC $ac >S $e1 >A $91 >X $32 >Y $62 >P $5389 $79 >M $538a $a6 >M $538b $ae >M $538c $f2 >M $aed8 $e0 >M
T{ op PC S A X Y P -> $538c $ac $c1 $91 $32 $a1 }T T{ $5389 M $538a M $538b M $538c M $aed8 M -> $79 $a6 $ae $f2 $e0 }T
$e00d >PC $85 >S $48 >A $42 >X $8a >Y $e5 >P $00b9 $29 >M $e00d $79 >M $e00e $2f >M $e00f $00 >M $e010 $71 >M
T{ op PC S A X Y P -> $e010 $85 $72 $42 $8a $24 }T T{ $00b9 M $e00d M $e00e M $e00f M $e010 M -> $29 $79 $2f $00 $71 }T
$280a >PC $bc >S $11 >A $45 >X $40 >Y $22 >P $280a $79 >M $280b $6d >M $280c $b1 >M $280d $08 >M $b1ad $39 >M
T{ op PC S A X Y P -> $280d $bc $4a $45 $40 $20 }T T{ $280a M $280b M $280c M $280d M $b1ad M -> $79 $6d $b1 $08 $39 }T
$b274 >PC $32 >S $bb >A $23 >X $f4 >Y $27 >P $b274 $79 >M $b275 $d0 >M $b276 $d9 >M $b277 $9a >M $dac4 $c7 >M
T{ op PC S A X Y P -> $b277 $32 $83 $23 $f4 $a5 }T T{ $b274 M $b275 M $b276 M $b277 M $dac4 M -> $79 $d0 $d9 $9a $c7 }T
$070b >PC $2b >S $9c >A $40 >X $39 >Y $e0 >P $070b $79 >M $070c $24 >M $070d $94 >M $070e $52 >M $945d $b8 >M
T{ op PC S A X Y P -> $070e $2b $54 $40 $39 $61 }T T{ $070b M $070c M $070d M $070e M $945d M -> $79 $24 $94 $52 $b8 }T
$2319 >PC $52 >S $57 >A $06 >X $45 >Y $a7 >P $2319 $79 >M $231a $32 >M $231b $51 >M $231c $a6 >M $5177 $dd >M
T{ op PC S A X Y P -> $231c $52 $35 $06 $45 $25 }T T{ $2319 M $231a M $231b M $231c M $5177 M -> $79 $32 $51 $a6 $dd }T
$6117 >PC $7c >S $d1 >A $12 >X $41 >Y $e0 >P $6117 $79 >M $6118 $01 >M $6119 $76 >M $611a $00 >M $7642 $65 >M
T{ op PC S A X Y P -> $611a $7c $36 $12 $41 $21 }T T{ $6117 M $6118 M $6119 M $611a M $7642 M -> $79 $01 $76 $00 $65 }T
$5967 >PC $b1 >S $1c >A $50 >X $fa >Y $66 >P $5967 $79 >M $5968 $34 >M $5969 $64 >M $596a $0a >M $652e $09 >M
T{ op PC S A X Y P -> $596a $b1 $25 $50 $fa $24 }T T{ $5967 M $5968 M $5969 M $596a M $652e M -> $79 $34 $64 $0a $09 }T
$5fb7 >PC $d2 >S $d9 >A $ca >X $ec >Y $60 >P $5fb7 $79 >M $5fb8 $fb >M $5fb9 $86 >M $5fba $57 >M $87e7 $0a >M
T{ op PC S A X Y P -> $5fba $d2 $e3 $ca $ec $a0 }T T{ $5fb7 M $5fb8 M $5fb9 M $5fba M $87e7 M -> $79 $fb $86 $57 $0a }T
( 7a )
$a244 >PC $27 >S $f8 >A $f9 >X $d3 >Y $a6 >P $0127 $1d >M $0128 $5b >M $a244 $7a >M $a245 $e3 >M $a246 $8a >M
T{ op PC S A X Y P -> $a245 $28 $f8 $f9 $5b $24 }T T{ $0127 M $0128 M $a244 M $a245 M $a246 M -> $1d $5b $7a $e3 $8a }T
$b9c1 >PC $c8 >S $42 >A $9d >X $c5 >Y $66 >P $01c8 $78 >M $01c9 $b7 >M $b9c1 $7a >M $b9c2 $1e >M $b9c3 $4a >M
T{ op PC S A X Y P -> $b9c2 $c9 $42 $9d $b7 $e4 }T T{ $01c8 M $01c9 M $b9c1 M $b9c2 M $b9c3 M -> $78 $b7 $7a $1e $4a }T
$f7fd >PC $f0 >S $3f >A $eb >X $57 >Y $e5 >P $01f0 $70 >M $01f1 $f9 >M $f7fd $7a >M $f7fe $2a >M $f7ff $32 >M
T{ op PC S A X Y P -> $f7fe $f1 $3f $eb $f9 $e5 }T T{ $01f0 M $01f1 M $f7fd M $f7fe M $f7ff M -> $70 $f9 $7a $2a $32 }T
$861c >PC $9d >S $46 >A $ad >X $c8 >Y $e6 >P $019d $e3 >M $019e $45 >M $861c $7a >M $861d $70 >M $861e $96 >M
T{ op PC S A X Y P -> $861d $9e $46 $ad $45 $64 }T T{ $019d M $019e M $861c M $861d M $861e M -> $e3 $45 $7a $70 $96 }T
$582e >PC $d8 >S $f7 >A $fe >X $98 >Y $64 >P $01d8 $60 >M $01d9 $d9 >M $582e $7a >M $582f $b8 >M $5830 $79 >M
T{ op PC S A X Y P -> $582f $d9 $f7 $fe $d9 $e4 }T T{ $01d8 M $01d9 M $582e M $582f M $5830 M -> $60 $d9 $7a $b8 $79 }T
$9e76 >PC $e4 >S $6c >A $23 >X $44 >Y $23 >P $01e4 $f2 >M $01e5 $f0 >M $9e76 $7a >M $9e77 $9d >M $9e78 $0a >M
T{ op PC S A X Y P -> $9e77 $e5 $6c $23 $f0 $a1 }T T{ $01e4 M $01e5 M $9e76 M $9e77 M $9e78 M -> $f2 $f0 $7a $9d $0a }T
$99e2 >PC $70 >S $73 >A $8b >X $13 >Y $60 >P $0170 $14 >M $0171 $e5 >M $99e2 $7a >M $99e3 $dc >M $99e4 $e0 >M
T{ op PC S A X Y P -> $99e3 $71 $73 $8b $e5 $e0 }T T{ $0170 M $0171 M $99e2 M $99e3 M $99e4 M -> $14 $e5 $7a $dc $e0 }T
$40d8 >PC $89 >S $b5 >A $76 >X $f2 >Y $a1 >P $0189 $4e >M $018a $42 >M $40d8 $7a >M $40d9 $4d >M $40da $02 >M
T{ op PC S A X Y P -> $40d9 $8a $b5 $76 $42 $21 }T T{ $0189 M $018a M $40d8 M $40d9 M $40da M -> $4e $42 $7a $4d $02 }T
$8591 >PC $73 >S $25 >A $0d >X $39 >Y $63 >P $0173 $96 >M $0174 $f9 >M $8591 $7a >M $8592 $21 >M $8593 $bd >M
T{ op PC S A X Y P -> $8592 $74 $25 $0d $f9 $e1 }T T{ $0173 M $0174 M $8591 M $8592 M $8593 M -> $96 $f9 $7a $21 $bd }T
$dfbf >PC $af >S $c5 >A $bf >X $f2 >Y $27 >P $01af $65 >M $01b0 $6c >M $dfbf $7a >M $dfc0 $52 >M $dfc1 $09 >M
T{ op PC S A X Y P -> $dfc0 $b0 $c5 $bf $6c $25 }T T{ $01af M $01b0 M $dfbf M $dfc0 M $dfc1 M -> $65 $6c $7a $52 $09 }T
$caad >PC $eb >S $f0 >A $29 >X $47 >Y $a0 >P $01eb $3c >M $01ec $f1 >M $caad $7a >M $caae $b4 >M $caaf $8d >M
T{ op PC S A X Y P -> $caae $ec $f0 $29 $f1 $a0 }T T{ $01eb M $01ec M $caad M $caae M $caaf M -> $3c $f1 $7a $b4 $8d }T
$ce8b >PC $66 >S $69 >A $05 >X $10 >Y $64 >P $0166 $14 >M $0167 $df >M $ce8b $7a >M $ce8c $ce >M $ce8d $f0 >M
T{ op PC S A X Y P -> $ce8c $67 $69 $05 $df $e4 }T T{ $0166 M $0167 M $ce8b M $ce8c M $ce8d M -> $14 $df $7a $ce $f0 }T
$26b4 >PC $36 >S $c6 >A $64 >X $f6 >Y $e4 >P $0136 $77 >M $0137 $09 >M $26b4 $7a >M $26b5 $c8 >M $26b6 $72 >M
T{ op PC S A X Y P -> $26b5 $37 $c6 $64 $09 $64 }T T{ $0136 M $0137 M $26b4 M $26b5 M $26b6 M -> $77 $09 $7a $c8 $72 }T
$7a18 >PC $12 >S $7d >A $f4 >X $f8 >Y $27 >P $0112 $1a >M $0113 $35 >M $7a18 $7a >M $7a19 $f8 >M $7a1a $a4 >M
T{ op PC S A X Y P -> $7a19 $13 $7d $f4 $35 $25 }T T{ $0112 M $0113 M $7a18 M $7a19 M $7a1a M -> $1a $35 $7a $f8 $a4 }T
$74a4 >PC $88 >S $24 >A $88 >X $37 >Y $e7 >P $0188 $bb >M $0189 $55 >M $74a4 $7a >M $74a5 $41 >M $74a6 $12 >M
T{ op PC S A X Y P -> $74a5 $89 $24 $88 $55 $65 }T T{ $0188 M $0189 M $74a4 M $74a5 M $74a6 M -> $bb $55 $7a $41 $12 }T
$3981 >PC $80 >S $19 >A $a1 >X $a8 >Y $a6 >P $0180 $85 >M $0181 $f9 >M $3981 $7a >M $3982 $ec >M $3983 $fc >M
T{ op PC S A X Y P -> $3982 $81 $19 $a1 $f9 $a4 }T T{ $0180 M $0181 M $3981 M $3982 M $3983 M -> $85 $f9 $7a $ec $fc }T
( 7b )
$aff6 >PC $85 >S $b4 >A $67 >X $8c >Y $63 >P $aff6 $7b >M $aff7 $96 >M $aff8 $a1 >M
T{ op PC S A X Y P -> $aff7 $85 $b4 $67 $8c $63 }T T{ $aff6 M $aff7 M $aff8 M -> $7b $96 $a1 }T
$7aef >PC $44 >S $3b >A $5f >X $23 >Y $e1 >P $7aef $7b >M $7af0 $3a >M $7af1 $bf >M
T{ op PC S A X Y P -> $7af0 $44 $3b $5f $23 $e1 }T T{ $7aef M $7af0 M $7af1 M -> $7b $3a $bf }T
$c29d >PC $68 >S $d5 >A $5b >X $67 >Y $e4 >P $c29d $7b >M $c29e $1e >M $c29f $47 >M
T{ op PC S A X Y P -> $c29e $68 $d5 $5b $67 $e4 }T T{ $c29d M $c29e M $c29f M -> $7b $1e $47 }T
$b0fb >PC $4b >S $af >A $f9 >X $5d >Y $25 >P $b0fb $7b >M $b0fc $5b >M $b0fd $f6 >M
T{ op PC S A X Y P -> $b0fc $4b $af $f9 $5d $25 }T T{ $b0fb M $b0fc M $b0fd M -> $7b $5b $f6 }T
$35b3 >PC $13 >S $b8 >A $dc >X $2e >Y $64 >P $35b3 $7b >M $35b4 $a7 >M $35b5 $28 >M
T{ op PC S A X Y P -> $35b4 $13 $b8 $dc $2e $64 }T T{ $35b3 M $35b4 M $35b5 M -> $7b $a7 $28 }T
$9a9f >PC $c0 >S $b5 >A $15 >X $ce >Y $23 >P $9a9f $7b >M $9aa0 $20 >M $9aa1 $1a >M
T{ op PC S A X Y P -> $9aa0 $c0 $b5 $15 $ce $23 }T T{ $9a9f M $9aa0 M $9aa1 M -> $7b $20 $1a }T
$bd5f >PC $8a >S $d8 >A $83 >X $32 >Y $e6 >P $bd5f $7b >M $bd60 $7f >M $bd61 $aa >M
T{ op PC S A X Y P -> $bd60 $8a $d8 $83 $32 $e6 }T T{ $bd5f M $bd60 M $bd61 M -> $7b $7f $aa }T
$39e4 >PC $df >S $f0 >A $15 >X $09 >Y $20 >P $39e4 $7b >M $39e5 $80 >M $39e6 $cb >M
T{ op PC S A X Y P -> $39e5 $df $f0 $15 $09 $20 }T T{ $39e4 M $39e5 M $39e6 M -> $7b $80 $cb }T
$0f87 >PC $22 >S $6f >A $d4 >X $b6 >Y $a3 >P $0f87 $7b >M $0f88 $04 >M $0f89 $50 >M
T{ op PC S A X Y P -> $0f88 $22 $6f $d4 $b6 $a3 }T T{ $0f87 M $0f88 M $0f89 M -> $7b $04 $50 }T
$356b >PC $3f >S $ed >A $60 >X $37 >Y $a3 >P $356b $7b >M $356c $93 >M $356d $94 >M
T{ op PC S A X Y P -> $356c $3f $ed $60 $37 $a3 }T T{ $356b M $356c M $356d M -> $7b $93 $94 }T
$66ef >PC $a7 >S $61 >A $65 >X $73 >Y $e1 >P $66ef $7b >M $66f0 $55 >M $66f1 $64 >M
T{ op PC S A X Y P -> $66f0 $a7 $61 $65 $73 $e1 }T T{ $66ef M $66f0 M $66f1 M -> $7b $55 $64 }T
$cd1a >PC $61 >S $13 >A $48 >X $e9 >Y $a2 >P $cd1a $7b >M $cd1b $f0 >M $cd1c $6d >M
T{ op PC S A X Y P -> $cd1b $61 $13 $48 $e9 $a2 }T T{ $cd1a M $cd1b M $cd1c M -> $7b $f0 $6d }T
$3674 >PC $52 >S $1c >A $5b >X $f7 >Y $e6 >P $3674 $7b >M $3675 $bf >M $3676 $b4 >M
T{ op PC S A X Y P -> $3675 $52 $1c $5b $f7 $e6 }T T{ $3674 M $3675 M $3676 M -> $7b $bf $b4 }T
$0795 >PC $e8 >S $ac >A $4a >X $a7 >Y $e0 >P $0795 $7b >M $0796 $ab >M $0797 $cf >M
T{ op PC S A X Y P -> $0796 $e8 $ac $4a $a7 $e0 }T T{ $0795 M $0796 M $0797 M -> $7b $ab $cf }T
$2e7a >PC $03 >S $d2 >A $bf >X $3f >Y $25 >P $2e7a $7b >M $2e7b $e0 >M $2e7c $8c >M
T{ op PC S A X Y P -> $2e7b $03 $d2 $bf $3f $25 }T T{ $2e7a M $2e7b M $2e7c M -> $7b $e0 $8c }T
$d8a9 >PC $13 >S $a8 >A $e3 >X $44 >Y $27 >P $d8a9 $7b >M $d8aa $a4 >M $d8ab $ca >M
T{ op PC S A X Y P -> $d8aa $13 $a8 $e3 $44 $27 }T T{ $d8a9 M $d8aa M $d8ab M -> $7b $a4 $ca }T
( 7c )
$a7b1 >PC $b1 >S $d5 >A $98 >X $07 >Y $60 >P $a7b1 $7c >M $a7b2 $3e >M $a7b3 $eb >M $c3b0 $85 >M $ebd6 $b0 >M $ebd7 $c3 >M
T{ op PC S A X Y P -> $c3b0 $b1 $d5 $98 $07 $60 }T T{ $a7b1 M $a7b2 M $a7b3 M $c3b0 M $ebd6 M $ebd7 M -> $7c $3e $eb $85 $b0 $c3 }T
$772c >PC $ee >S $7c >A $ef >X $ec >Y $27 >P $2c18 $3a >M $2c19 $3b >M $3b3a $b9 >M $772c $7c >M $772d $29 >M $772e $2b >M
T{ op PC S A X Y P -> $3b3a $ee $7c $ef $ec $27 }T T{ $2c18 M $2c19 M $3b3a M $772c M $772d M $772e M -> $3a $3b $b9 $7c $29 $2b }T
$c964 >PC $58 >S $1f >A $30 >X $27 >Y $e7 >P $902f $d4 >M $a709 $2f >M $a70a $90 >M $c964 $7c >M $c965 $d9 >M $c966 $a6 >M
T{ op PC S A X Y P -> $902f $58 $1f $30 $27 $e7 }T T{ $902f M $a709 M $a70a M $c964 M $c965 M $c966 M -> $d4 $2f $90 $7c $d9 $a6 }T
$75b2 >PC $c1 >S $e3 >A $2e >X $e9 >Y $a3 >P $2b5b $92 >M $2b5c $e1 >M $75b2 $7c >M $75b3 $2d >M $75b4 $2b >M $e192 $85 >M
T{ op PC S A X Y P -> $e192 $c1 $e3 $2e $e9 $a3 }T T{ $2b5b M $2b5c M $75b2 M $75b3 M $75b4 M $e192 M -> $92 $e1 $7c $2d $2b $85 }T
$157d >PC $9b >S $a6 >A $6d >X $48 >Y $e0 >P $157d $7c >M $157e $15 >M $157f $e4 >M $e482 $bf >M $e483 $f0 >M $f0bf $77 >M
T{ op PC S A X Y P -> $f0bf $9b $a6 $6d $48 $e0 }T T{ $157d M $157e M $157f M $e482 M $e483 M $f0bf M -> $7c $15 $e4 $bf $f0 $77 }T
$1ca5 >PC $d7 >S $8e >A $eb >X $5b >Y $67 >P $1ca5 $7c >M $1ca6 $b6 >M $1ca7 $aa >M $644a $4c >M $aba1 $4a >M $aba2 $64 >M
T{ op PC S A X Y P -> $644a $d7 $8e $eb $5b $67 }T T{ $1ca5 M $1ca6 M $1ca7 M $644a M $aba1 M $aba2 M -> $7c $b6 $aa $4c $4a $64 }T
$936f >PC $dd >S $fb >A $b7 >X $30 >Y $a6 >P $4db6 $ee >M $4db7 $ee >M $936f $7c >M $9370 $ff >M $9371 $4c >M $eeee $09 >M
T{ op PC S A X Y P -> $eeee $dd $fb $b7 $30 $a6 }T T{ $4db6 M $4db7 M $936f M $9370 M $9371 M $eeee M -> $ee $ee $7c $ff $4c $09 }T
$cd70 >PC $7e >S $d1 >A $80 >X $19 >Y $26 >P $3100 $b6 >M $959b $00 >M $959c $31 >M $cd70 $7c >M $cd71 $1b >M $cd72 $95 >M
T{ op PC S A X Y P -> $3100 $7e $d1 $80 $19 $26 }T T{ $3100 M $959b M $959c M $cd70 M $cd71 M $cd72 M -> $b6 $00 $31 $7c $1b $95 }T
$d117 >PC $1e >S $5c >A $28 >X $b1 >Y $e1 >P $60e6 $c1 >M $cb62 $e6 >M $cb63 $60 >M $d117 $7c >M $d118 $3a >M $d119 $cb >M
T{ op PC S A X Y P -> $60e6 $1e $5c $28 $b1 $e1 }T T{ $60e6 M $cb62 M $cb63 M $d117 M $d118 M $d119 M -> $c1 $e6 $60 $7c $3a $cb }T
$0d82 >PC $4b >S $2b >A $48 >X $84 >Y $66 >P $0d82 $7c >M $0d83 $7c >M $0d84 $c3 >M $c3c4 $26 >M $c3c5 $f9 >M $f926 $be >M
T{ op PC S A X Y P -> $f926 $4b $2b $48 $84 $66 }T T{ $0d82 M $0d83 M $0d84 M $c3c4 M $c3c5 M $f926 M -> $7c $7c $c3 $26 $f9 $be }T
$6a8e >PC $08 >S $ed >A $c1 >X $b6 >Y $63 >P $6556 $5d >M $6a8e $7c >M $6a8f $b2 >M $6a90 $6b >M $6c73 $56 >M $6c74 $65 >M
T{ op PC S A X Y P -> $6556 $08 $ed $c1 $b6 $63 }T T{ $6556 M $6a8e M $6a8f M $6a90 M $6c73 M $6c74 M -> $5d $7c $b2 $6b $56 $65 }T
$efd3 >PC $fa >S $78 >A $f1 >X $f0 >Y $a2 >P $0d43 $87 >M $0d44 $35 >M $3587 $51 >M $efd3 $7c >M $efd4 $52 >M $efd5 $0c >M
T{ op PC S A X Y P -> $3587 $fa $78 $f1 $f0 $a2 }T T{ $0d43 M $0d44 M $3587 M $efd3 M $efd4 M $efd5 M -> $87 $35 $51 $7c $52 $0c }T
$fa35 >PC $ab >S $6b >A $a3 >X $6a >Y $e0 >P $d235 $0c >M $d506 $35 >M $d507 $d2 >M $fa35 $7c >M $fa36 $63 >M $fa37 $d4 >M
T{ op PC S A X Y P -> $d235 $ab $6b $a3 $6a $e0 }T T{ $d235 M $d506 M $d507 M $fa35 M $fa36 M $fa37 M -> $0c $35 $d2 $7c $63 $d4 }T
$25d0 >PC $c2 >S $8a >A $f2 >X $ea >Y $64 >P $25d0 $7c >M $25d1 $54 >M $25d2 $78 >M $7946 $0e >M $7947 $c4 >M $c40e $e7 >M
T{ op PC S A X Y P -> $c40e $c2 $8a $f2 $ea $64 }T T{ $25d0 M $25d1 M $25d2 M $7946 M $7947 M $c40e M -> $7c $54 $78 $0e $c4 $e7 }T
$b911 >PC $bf >S $53 >A $eb >X $a2 >Y $e1 >P $b911 $7c >M $b912 $69 >M $b913 $c4 >M $bae5 $80 >M $c554 $e5 >M $c555 $ba >M
T{ op PC S A X Y P -> $bae5 $bf $53 $eb $a2 $e1 }T T{ $b911 M $b912 M $b913 M $bae5 M $c554 M $c555 M -> $7c $69 $c4 $80 $e5 $ba }T
$933b >PC $83 >S $d6 >A $51 >X $50 >Y $a7 >P $084c $f4 >M $933b $7c >M $933c $bc >M $933d $ac >M $ad0d $4c >M $ad0e $08 >M
T{ op PC S A X Y P -> $084c $83 $d6 $51 $50 $a7 }T T{ $084c M $933b M $933c M $933d M $ad0d M $ad0e M -> $f4 $7c $bc $ac $4c $08 }T
( 7d )
$2624 >PC $ba >S $c4 >A $28 >X $4e >Y $a4 >P $2624 $7d >M $2625 $fc >M $2626 $44 >M $2627 $ea >M $4524 $c1 >M
T{ op PC S A X Y P -> $2627 $ba $85 $28 $4e $a5 }T T{ $2624 M $2625 M $2626 M $2627 M $4524 M -> $7d $fc $44 $ea $c1 }T
$653e >PC $e5 >S $e2 >A $ff >X $d6 >Y $a2 >P $653e $7d >M $653f $b4 >M $6540 $96 >M $6541 $cf >M $97b3 $1d >M
T{ op PC S A X Y P -> $6541 $e5 $ff $ff $d6 $a0 }T T{ $653e M $653f M $6540 M $6541 M $97b3 M -> $7d $b4 $96 $cf $1d }T
$fbde >PC $04 >S $00 >A $aa >X $2a >Y $e4 >P $74f3 $4f >M $fbde $7d >M $fbdf $49 >M $fbe0 $74 >M $fbe1 $35 >M
T{ op PC S A X Y P -> $fbe1 $04 $4f $aa $2a $24 }T T{ $74f3 M $fbde M $fbdf M $fbe0 M $fbe1 M -> $4f $7d $49 $74 $35 }T
$156b >PC $2f >S $54 >A $17 >X $e9 >Y $a7 >P $156b $7d >M $156c $6d >M $156d $e4 >M $156e $b4 >M $e484 $f7 >M
T{ op PC S A X Y P -> $156e $2f $4c $17 $e9 $25 }T T{ $156b M $156c M $156d M $156e M $e484 M -> $7d $6d $e4 $b4 $f7 }T
$4a86 >PC $76 >S $ac >A $31 >X $93 >Y $e6 >P $4a86 $7d >M $4a87 $74 >M $4a88 $71 >M $4a89 $f6 >M $71a5 $41 >M
T{ op PC S A X Y P -> $4a89 $76 $ed $31 $93 $a4 }T T{ $4a86 M $4a87 M $4a88 M $4a89 M $71a5 M -> $7d $74 $71 $f6 $41 }T
$648a >PC $e2 >S $6a >A $83 >X $b2 >Y $66 >P $648a $7d >M $648b $1f >M $648c $99 >M $648d $a6 >M $99a2 $ab >M
T{ op PC S A X Y P -> $648d $e2 $15 $83 $b2 $25 }T T{ $648a M $648b M $648c M $648d M $99a2 M -> $7d $1f $99 $a6 $ab }T
$5733 >PC $d2 >S $d9 >A $48 >X $41 >Y $22 >P $5733 $7d >M $5734 $55 >M $5735 $cb >M $5736 $d4 >M $cb9d $56 >M
T{ op PC S A X Y P -> $5736 $d2 $2f $48 $41 $21 }T T{ $5733 M $5734 M $5735 M $5736 M $cb9d M -> $7d $55 $cb $d4 $56 }T
$7d0f >PC $2d >S $18 >A $08 >X $f1 >Y $e1 >P $6766 $3e >M $7d0f $7d >M $7d10 $5e >M $7d11 $67 >M $7d12 $ef >M
T{ op PC S A X Y P -> $7d12 $2d $57 $08 $f1 $20 }T T{ $6766 M $7d0f M $7d10 M $7d11 M $7d12 M -> $3e $7d $5e $67 $ef }T
$8195 >PC $7c >S $43 >A $c5 >X $b4 >Y $a0 >P $8195 $7d >M $8196 $90 >M $8197 $a6 >M $8198 $a5 >M $a755 $c4 >M
T{ op PC S A X Y P -> $8198 $7c $07 $c5 $b4 $21 }T T{ $8195 M $8196 M $8197 M $8198 M $a755 M -> $7d $90 $a6 $a5 $c4 }T
$b5cb >PC $8d >S $f8 >A $1b >X $25 >Y $e7 >P $1f40 $3a >M $b5cb $7d >M $b5cc $25 >M $b5cd $1f >M $b5ce $ff >M
T{ op PC S A X Y P -> $b5ce $8d $33 $1b $25 $25 }T T{ $1f40 M $b5cb M $b5cc M $b5cd M $b5ce M -> $3a $7d $25 $1f $ff }T
$1283 >PC $e9 >S $d7 >A $6b >X $b0 >Y $a5 >P $1283 $7d >M $1284 $b2 >M $1285 $3c >M $1286 $31 >M $3d1d $55 >M
T{ op PC S A X Y P -> $1286 $e9 $2d $6b $b0 $25 }T T{ $1283 M $1284 M $1285 M $1286 M $3d1d M -> $7d $b2 $3c $31 $55 }T
$334b >PC $68 >S $1b >A $ed >X $12 >Y $a6 >P $01df $ad >M $334b $7d >M $334c $f2 >M $334d $00 >M $334e $24 >M
T{ op PC S A X Y P -> $334e $68 $c8 $ed $12 $a4 }T T{ $01df M $334b M $334c M $334d M $334e M -> $ad $7d $f2 $00 $24 }T
$9ea0 >PC $2d >S $57 >A $6a >X $a9 >Y $67 >P $7aa1 $49 >M $9ea0 $7d >M $9ea1 $37 >M $9ea2 $7a >M $9ea3 $91 >M
T{ op PC S A X Y P -> $9ea3 $2d $a1 $6a $a9 $e4 }T T{ $7aa1 M $9ea0 M $9ea1 M $9ea2 M $9ea3 M -> $49 $7d $37 $7a $91 }T
$7ed9 >PC $23 >S $a8 >A $6c >X $3c >Y $e6 >P $243a $f4 >M $7ed9 $7d >M $7eda $ce >M $7edb $23 >M $7edc $d6 >M
T{ op PC S A X Y P -> $7edc $23 $9c $6c $3c $a5 }T T{ $243a M $7ed9 M $7eda M $7edb M $7edc M -> $f4 $7d $ce $23 $d6 }T
$fb1d >PC $bb >S $e4 >A $8a >X $e8 >Y $21 >P $f3ec $0a >M $fb1d $7d >M $fb1e $62 >M $fb1f $f3 >M $fb20 $4f >M
T{ op PC S A X Y P -> $fb20 $bb $ef $8a $e8 $a0 }T T{ $f3ec M $fb1d M $fb1e M $fb1f M $fb20 M -> $0a $7d $62 $f3 $4f }T
$5595 >PC $5b >S $9d >A $31 >X $61 >Y $a3 >P $042a $80 >M $5595 $7d >M $5596 $f9 >M $5597 $03 >M $5598 $06 >M
T{ op PC S A X Y P -> $5598 $5b $1e $31 $61 $61 }T T{ $042a M $5595 M $5596 M $5597 M $5598 M -> $80 $7d $f9 $03 $06 }T
( 7e )
$a4d8 >PC $5a >S $8b >A $32 >X $6b >Y $62 >P $791e $37 >M $a4d8 $7e >M $a4d9 $ec >M $a4da $78 >M $a4db $b3 >M
T{ op PC S A X Y P -> $a4db $5a $8b $32 $6b $61 }T T{ $791e M $a4d8 M $a4d9 M $a4da M $a4db M -> $1b $7e $ec $78 $b3 }T
$5490 >PC $67 >S $7c >A $4c >X $29 >Y $21 >P $2c50 $6d >M $5490 $7e >M $5491 $04 >M $5492 $2c >M $5493 $bd >M
T{ op PC S A X Y P -> $5493 $67 $7c $4c $29 $a1 }T T{ $2c50 M $5490 M $5491 M $5492 M $5493 M -> $b6 $7e $04 $2c $bd }T
$c429 >PC $44 >S $54 >A $89 >X $77 >Y $e4 >P $c429 $7e >M $c42a $57 >M $c42b $e7 >M $c42c $7b >M $e7e0 $e0 >M
T{ op PC S A X Y P -> $c42c $44 $54 $89 $77 $64 }T T{ $c429 M $c42a M $c42b M $c42c M $e7e0 M -> $7e $57 $e7 $7b $70 }T
$8055 >PC $94 >S $6f >A $0f >X $ff >Y $a3 >P $8055 $7e >M $8056 $31 >M $8057 $c8 >M $8058 $4f >M $c840 $b8 >M
T{ op PC S A X Y P -> $8058 $94 $6f $0f $ff $a0 }T T{ $8055 M $8056 M $8057 M $8058 M $c840 M -> $7e $31 $c8 $4f $dc }T
$4338 >PC $3e >S $b2 >A $fe >X $b5 >Y $63 >P $4338 $7e >M $4339 $c2 >M $433a $b6 >M $433b $33 >M $b7c0 $53 >M
T{ op PC S A X Y P -> $433b $3e $b2 $fe $b5 $e1 }T T{ $4338 M $4339 M $433a M $433b M $b7c0 M -> $7e $c2 $b6 $33 $a9 }T
$d5f8 >PC $0e >S $cb >A $87 >X $b7 >Y $a3 >P $28f1 $0d >M $d5f8 $7e >M $d5f9 $6a >M $d5fa $28 >M $d5fb $ca >M
T{ op PC S A X Y P -> $d5fb $0e $cb $87 $b7 $a1 }T T{ $28f1 M $d5f8 M $d5f9 M $d5fa M $d5fb M -> $86 $7e $6a $28 $ca }T
$63e3 >PC $17 >S $34 >A $da >X $9d >Y $e6 >P $59d2 $b4 >M $63e3 $7e >M $63e4 $f8 >M $63e5 $58 >M $63e6 $e6 >M
T{ op PC S A X Y P -> $63e6 $17 $34 $da $9d $64 }T T{ $59d2 M $63e3 M $63e4 M $63e5 M $63e6 M -> $5a $7e $f8 $58 $e6 }T
$32c0 >PC $78 >S $05 >A $3a >X $ff >Y $64 >P $32c0 $7e >M $32c1 $45 >M $32c2 $3a >M $32c3 $d3 >M $3a7f $bf >M
T{ op PC S A X Y P -> $32c3 $78 $05 $3a $ff $65 }T T{ $32c0 M $32c1 M $32c2 M $32c3 M $3a7f M -> $7e $45 $3a $d3 $5f }T
$a0bd >PC $25 >S $2a >A $f4 >X $ca >Y $26 >P $64d9 $59 >M $a0bd $7e >M $a0be $e5 >M $a0bf $63 >M $a0c0 $42 >M
T{ op PC S A X Y P -> $a0c0 $25 $2a $f4 $ca $25 }T T{ $64d9 M $a0bd M $a0be M $a0bf M $a0c0 M -> $2c $7e $e5 $63 $42 }T
$63fe >PC $18 >S $81 >A $87 >X $0d >Y $66 >P $63fe $7e >M $63ff $12 >M $6400 $b7 >M $6401 $f6 >M $b799 $b6 >M
T{ op PC S A X Y P -> $6401 $18 $81 $87 $0d $64 }T T{ $63fe M $63ff M $6400 M $6401 M $b799 M -> $7e $12 $b7 $f6 $5b }T
$57ae >PC $c2 >S $ae >A $60 >X $ac >Y $67 >P $57ae $7e >M $57af $bd >M $57b0 $65 >M $57b1 $77 >M $661d $a4 >M
T{ op PC S A X Y P -> $57b1 $c2 $ae $60 $ac $e4 }T T{ $57ae M $57af M $57b0 M $57b1 M $661d M -> $7e $bd $65 $77 $d2 }T
$b160 >PC $e3 >S $1d >A $3b >X $00 >Y $a7 >P $8686 $75 >M $b160 $7e >M $b161 $4b >M $b162 $86 >M $b163 $2c >M
T{ op PC S A X Y P -> $b163 $e3 $1d $3b $00 $a5 }T T{ $8686 M $b160 M $b161 M $b162 M $b163 M -> $ba $7e $4b $86 $2c }T
$55f0 >PC $5c >S $38 >A $98 >X $ac >Y $a5 >P $55f0 $7e >M $55f1 $71 >M $55f2 $f5 >M $55f3 $2d >M $f609 $af >M
T{ op PC S A X Y P -> $55f3 $5c $38 $98 $ac $a5 }T T{ $55f0 M $55f1 M $55f2 M $55f3 M $f609 M -> $7e $71 $f5 $2d $d7 }T
$3c6a >PC $2c >S $ef >A $a1 >X $3d >Y $61 >P $3c6a $7e >M $3c6b $a3 >M $3c6c $87 >M $3c6d $bf >M $8844 $bd >M
T{ op PC S A X Y P -> $3c6d $2c $ef $a1 $3d $e1 }T T{ $3c6a M $3c6b M $3c6c M $3c6d M $8844 M -> $7e $a3 $87 $bf $de }T
$fd5e >PC $e4 >S $67 >A $40 >X $6c >Y $21 >P $8523 $d2 >M $fd5e $7e >M $fd5f $e3 >M $fd60 $84 >M $fd61 $90 >M
T{ op PC S A X Y P -> $fd61 $e4 $67 $40 $6c $a0 }T T{ $8523 M $fd5e M $fd5f M $fd60 M $fd61 M -> $e9 $7e $e3 $84 $90 }T
$392c >PC $f0 >S $a6 >A $c3 >X $78 >Y $e5 >P $392c $7e >M $392d $8d >M $392e $9e >M $392f $15 >M $9f50 $90 >M
T{ op PC S A X Y P -> $392f $f0 $a6 $c3 $78 $e4 }T T{ $392c M $392d M $392e M $392f M $9f50 M -> $7e $8d $9e $15 $c8 }T
( 7f )
$a882 >PC $9e >S $c4 >A $1e >X $18 >Y $63 >P $00ba $95 >M $a866 $46 >M $a882 $7f >M $a883 $ba >M $a884 $e1 >M $a885 $68 >M
T{ op PC S A X Y P -> $a885 $9e $c4 $1e $18 $63 }T T{ $00ba M $a866 M $a882 M $a883 M $a884 M $a885 M -> $95 $46 $7f $ba $e1 $68 }T
$9f52 >PC $53 >S $98 >A $8d >X $ab >Y $61 >P $009f $f6 >M $9f52 $7f >M $9f53 $9f >M $9f54 $9b >M $9f55 $16 >M $9ff0 $f2 >M
T{ op PC S A X Y P -> $9f55 $53 $98 $8d $ab $61 }T T{ $009f M $9f52 M $9f53 M $9f54 M $9f55 M $9ff0 M -> $f6 $7f $9f $9b $16 $f2 }T
$d6c5 >PC $3b >S $49 >A $d2 >X $48 >Y $27 >P $00df $f2 >M $d6c5 $7f >M $d6c6 $df >M $d6c7 $21 >M $d6c8 $68 >M $d6e9 $4a >M
T{ op PC S A X Y P -> $d6c8 $3b $49 $d2 $48 $27 }T T{ $00df M $d6c5 M $d6c6 M $d6c7 M $d6c8 M $d6e9 M -> $f2 $7f $df $21 $68 $4a }T
$c639 >PC $ce >S $22 >A $e1 >X $bf >Y $e5 >P $009d $93 >M $c639 $7f >M $c63a $9d >M $c63b $49 >M $c63c $11 >M $c685 $a6 >M
T{ op PC S A X Y P -> $c63c $ce $22 $e1 $bf $e5 }T T{ $009d M $c639 M $c63a M $c63b M $c63c M $c685 M -> $93 $7f $9d $49 $11 $a6 }T
$bac8 >PC $4d >S $cf >A $54 >X $84 >Y $a1 >P $00dc $48 >M $bac8 $7f >M $bac9 $dc >M $baca $0f >M $bada $7b >M
T{ op PC S A X Y P -> $bada $4d $cf $54 $84 $a1 }T T{ $00dc M $bac8 M $bac9 M $baca M $bada M -> $48 $7f $dc $0f $7b }T
$2156 >PC $bd >S $7d >A $68 >X $48 >Y $60 >P $00af $c1 >M $2156 $7f >M $2157 $af >M $2158 $2c >M $2159 $c4 >M $2185 $ad >M
T{ op PC S A X Y P -> $2159 $bd $7d $68 $48 $60 }T T{ $00af M $2156 M $2157 M $2158 M $2159 M $2185 M -> $c1 $7f $af $2c $c4 $ad }T
$9d73 >PC $6f >S $01 >A $21 >X $6d >Y $67 >P $0095 $e4 >M $9d10 $f3 >M $9d73 $7f >M $9d74 $95 >M $9d75 $9a >M $9d76 $c9 >M
T{ op PC S A X Y P -> $9d76 $6f $01 $21 $6d $67 }T T{ $0095 M $9d10 M $9d73 M $9d74 M $9d75 M $9d76 M -> $e4 $f3 $7f $95 $9a $c9 }T
$d93f >PC $6e >S $51 >A $7d >X $f4 >Y $60 >P $0027 $93 >M $d93f $7f >M $d940 $27 >M $d941 $11 >M $d942 $10 >M $d953 $7e >M
T{ op PC S A X Y P -> $d942 $6e $51 $7d $f4 $60 }T T{ $0027 M $d93f M $d940 M $d941 M $d942 M $d953 M -> $93 $7f $27 $11 $10 $7e }T
$9065 >PC $b3 >S $4d >A $e9 >X $0a >Y $66 >P $00cb $46 >M $904a $23 >M $9065 $7f >M $9066 $cb >M $9067 $e2 >M
T{ op PC S A X Y P -> $904a $b3 $4d $e9 $0a $66 }T T{ $00cb M $904a M $9065 M $9066 M $9067 M -> $46 $23 $7f $cb $e2 }T
$0672 >PC $78 >S $65 >A $2e >X $b8 >Y $a2 >P $0094 $2f >M $0606 $6c >M $0672 $7f >M $0673 $94 >M $0674 $91 >M
T{ op PC S A X Y P -> $0606 $78 $65 $2e $b8 $a2 }T T{ $0094 M $0606 M $0672 M $0673 M $0674 M -> $2f $6c $7f $94 $91 }T
$6553 >PC $bf >S $fc >A $82 >X $f1 >Y $20 >P $0089 $7d >M $6513 $9a >M $6553 $7f >M $6554 $89 >M $6555 $bd >M
T{ op PC S A X Y P -> $6513 $bf $fc $82 $f1 $20 }T T{ $0089 M $6513 M $6553 M $6554 M $6555 M -> $7d $9a $7f $89 $bd }T
$b4db >PC $3b >S $23 >A $26 >X $a3 >Y $65 >P $0077 $36 >M $b41b $e3 >M $b4db $7f >M $b4dc $77 >M $b4dd $3d >M $b51b $ec >M
T{ op PC S A X Y P -> $b51b $3b $23 $26 $a3 $65 }T T{ $0077 M $b41b M $b4db M $b4dc M $b4dd M $b51b M -> $36 $e3 $7f $77 $3d $ec }T
$a742 >PC $83 >S $04 >A $55 >X $e4 >Y $e5 >P $0036 $de >M $a73e $4b >M $a742 $7f >M $a743 $36 >M $a744 $f9 >M $a745 $a6 >M
T{ op PC S A X Y P -> $a745 $83 $04 $55 $e4 $e5 }T T{ $0036 M $a73e M $a742 M $a743 M $a744 M $a745 M -> $de $4b $7f $36 $f9 $a6 }T
$9d7c >PC $43 >S $96 >A $0b >X $8d >Y $63 >P $00f3 $3f >M $9d09 $9a >M $9d7c $7f >M $9d7d $f3 >M $9d7e $8a >M
T{ op PC S A X Y P -> $9d09 $43 $96 $0b $8d $63 }T T{ $00f3 M $9d09 M $9d7c M $9d7d M $9d7e M -> $3f $9a $7f $f3 $8a }T
$fe58 >PC $df >S $a4 >A $d4 >X $4d >Y $e2 >P $00e3 $cd >M $fe58 $7f >M $fe59 $e3 >M $fe5a $59 >M $fe5b $c8 >M $feb4 $09 >M
T{ op PC S A X Y P -> $fe5b $df $a4 $d4 $4d $e2 }T T{ $00e3 M $fe58 M $fe59 M $fe5a M $fe5b M $feb4 M -> $cd $7f $e3 $59 $c8 $09 }T
$9e7b >PC $ab >S $d7 >A $5c >X $3c >Y $22 >P $006b $5d >M $9e2b $21 >M $9e7b $7f >M $9e7c $6b >M $9e7d $ad >M
T{ op PC S A X Y P -> $9e2b $ab $d7 $5c $3c $22 }T T{ $006b M $9e2b M $9e7b M $9e7c M $9e7d M -> $5d $21 $7f $6b $ad }T
( 80 )
$f145 >PC $9e >S $28 >A $7a >X $c2 >Y $25 >P $f145 $80 >M $f146 $11 >M $f147 $2c >M $f158 $08 >M
T{ op PC S A X Y P -> $f158 $9e $28 $7a $c2 $25 }T T{ $f145 M $f146 M $f147 M $f158 M -> $80 $11 $2c $08 }T
$05d6 >PC $8e >S $cd >A $d5 >X $99 >Y $e0 >P $05c9 $99 >M $05d6 $80 >M $05d7 $f1 >M $05d8 $99 >M
T{ op PC S A X Y P -> $05c9 $8e $cd $d5 $99 $e0 }T T{ $05c9 M $05d6 M $05d7 M $05d8 M -> $99 $80 $f1 $99 }T
$58da >PC $50 >S $c8 >A $9e >X $e6 >Y $66 >P $5842 $bb >M $58da $80 >M $58db $66 >M $58dc $c8 >M $5942 $cb >M
T{ op PC S A X Y P -> $5942 $50 $c8 $9e $e6 $66 }T T{ $5842 M $58da M $58db M $58dc M $5942 M -> $bb $80 $66 $c8 $cb }T
$cb71 >PC $93 >S $9a >A $25 >X $89 >Y $64 >P $cb6f $f8 >M $cb71 $80 >M $cb72 $fc >M $cb73 $0a >M
T{ op PC S A X Y P -> $cb6f $93 $9a $25 $89 $64 }T T{ $cb6f M $cb71 M $cb72 M $cb73 M -> $f8 $80 $fc $0a }T
$9500 >PC $62 >S $f3 >A $9b >X $7b >Y $a1 >P $9500 $80 >M $9501 $4b >M $9502 $1e >M $954d $b7 >M
T{ op PC S A X Y P -> $954d $62 $f3 $9b $7b $a1 }T T{ $9500 M $9501 M $9502 M $954d M -> $80 $4b $1e $b7 }T
$5c47 >PC $66 >S $a0 >A $45 >X $be >Y $a1 >P $5c18 $b5 >M $5c47 $80 >M $5c48 $cf >M $5c49 $a3 >M
T{ op PC S A X Y P -> $5c18 $66 $a0 $45 $be $a1 }T T{ $5c18 M $5c47 M $5c48 M $5c49 M -> $b5 $80 $cf $a3 }T
$1a49 >PC $a1 >S $28 >A $45 >X $f0 >Y $e0 >P $1a49 $80 >M $1a4a $46 >M $1a4b $99 >M $1a91 $0c >M
T{ op PC S A X Y P -> $1a91 $a1 $28 $45 $f0 $e0 }T T{ $1a49 M $1a4a M $1a4b M $1a91 M -> $80 $46 $99 $0c }T
$7e4a >PC $db >S $99 >A $c5 >X $01 >Y $63 >P $7df9 $aa >M $7e4a $80 >M $7e4b $ad >M $7e4c $08 >M $7ef9 $02 >M
T{ op PC S A X Y P -> $7df9 $db $99 $c5 $01 $63 }T T{ $7df9 M $7e4a M $7e4b M $7e4c M $7ef9 M -> $aa $80 $ad $08 $02 }T
$0c7a >PC $d6 >S $3b >A $b9 >X $98 >Y $e3 >P $0c7a $80 >M $0c7b $37 >M $0c7c $2b >M $0cb3 $56 >M
T{ op PC S A X Y P -> $0cb3 $d6 $3b $b9 $98 $e3 }T T{ $0c7a M $0c7b M $0c7c M $0cb3 M -> $80 $37 $2b $56 }T
$5864 >PC $94 >S $98 >A $01 >X $6b >Y $a4 >P $57fa $39 >M $5864 $80 >M $5865 $94 >M $5866 $f0 >M $58fa $95 >M
T{ op PC S A X Y P -> $57fa $94 $98 $01 $6b $a4 }T T{ $57fa M $5864 M $5865 M $5866 M $58fa M -> $39 $80 $94 $f0 $95 }T
$1c7d >PC $24 >S $f7 >A $12 >X $68 >Y $25 >P $1c4f $08 >M $1c7d $80 >M $1c7e $d0 >M $1c7f $17 >M
T{ op PC S A X Y P -> $1c4f $24 $f7 $12 $68 $25 }T T{ $1c4f M $1c7d M $1c7e M $1c7f M -> $08 $80 $d0 $17 }T
$6241 >PC $b4 >S $33 >A $d4 >X $fa >Y $25 >P $623a $47 >M $6241 $80 >M $6242 $f7 >M $6243 $d5 >M
T{ op PC S A X Y P -> $623a $b4 $33 $d4 $fa $25 }T T{ $623a M $6241 M $6242 M $6243 M -> $47 $80 $f7 $d5 }T
$d222 >PC $66 >S $28 >A $fa >X $1e >Y $62 >P $d1c4 $68 >M $d222 $80 >M $d223 $a0 >M $d224 $ee >M $d2c4 $44 >M
T{ op PC S A X Y P -> $d1c4 $66 $28 $fa $1e $62 }T T{ $d1c4 M $d222 M $d223 M $d224 M $d2c4 M -> $68 $80 $a0 $ee $44 }T
$7e4a >PC $44 >S $80 >A $e3 >X $49 >Y $27 >P $7e45 $dc >M $7e4a $80 >M $7e4b $f9 >M $7e4c $f1 >M
T{ op PC S A X Y P -> $7e45 $44 $80 $e3 $49 $27 }T T{ $7e45 M $7e4a M $7e4b M $7e4c M -> $dc $80 $f9 $f1 }T
$c3e2 >PC $95 >S $5b >A $0b >X $e3 >Y $65 >P $c3e2 $80 >M $c3e3 $18 >M $c3e4 $0a >M $c3fc $fb >M
T{ op PC S A X Y P -> $c3fc $95 $5b $0b $e3 $65 }T T{ $c3e2 M $c3e3 M $c3e4 M $c3fc M -> $80 $18 $0a $fb }T
$0a54 >PC $11 >S $40 >A $4e >X $b6 >Y $63 >P $0a33 $17 >M $0a54 $80 >M $0a55 $dd >M $0a56 $ff >M
T{ op PC S A X Y P -> $0a33 $11 $40 $4e $b6 $63 }T T{ $0a33 M $0a54 M $0a55 M $0a56 M -> $17 $80 $dd $ff }T
( 81 )
$9ef2 >PC $45 >S $9d >A $8c >X $34 >Y $a0 >P $0064 $c8 >M $00f0 $02 >M $00f1 $4d >M $9ef2 $81 >M $9ef3 $64 >M $9ef4 $a2 >M
T{ op PC S A X Y P -> $9ef4 $45 $9d $8c $34 $a0 }T T{ $0064 M $00f0 M $00f1 M $4d02 M $9ef2 M $9ef3 M $9ef4 M -> $c8 $02 $4d $9d $81 $64 $a2 }T
$9277 >PC $de >S $f1 >A $b7 >X $46 >Y $27 >P $002b $fb >M $002c $5c >M $0074 $17 >M $9277 $81 >M $9278 $74 >M $9279 $46 >M
T{ op PC S A X Y P -> $9279 $de $f1 $b7 $46 $27 }T T{ $002b M $002c M $0074 M $5cfb M $9277 M $9278 M $9279 M -> $fb $5c $17 $f1 $81 $74 $46 }T
$efea >PC $a9 >S $c0 >A $26 >X $64 >Y $e6 >P $002e $9c >M $0054 $a2 >M $0055 $86 >M $efea $81 >M $efeb $2e >M $efec $a0 >M
T{ op PC S A X Y P -> $efec $a9 $c0 $26 $64 $e6 }T T{ $002e M $0054 M $0055 M $86a2 M $efea M $efeb M $efec M -> $9c $a2 $86 $c0 $81 $2e $a0 }T
$499d >PC $97 >S $8b >A $41 >X $00 >Y $67 >P $001c $b0 >M $001d $c6 >M $00db $5c >M $499d $81 >M $499e $db >M $499f $77 >M
T{ op PC S A X Y P -> $499f $97 $8b $41 $00 $67 }T T{ $001c M $001d M $00db M $499d M $499e M $499f M $c6b0 M -> $b0 $c6 $5c $81 $db $77 $8b }T
$efb4 >PC $ed >S $d5 >A $b8 >X $5f >Y $a2 >P $005d $a3 >M $005e $9d >M $00a5 $42 >M $efb4 $81 >M $efb5 $a5 >M $efb6 $98 >M
T{ op PC S A X Y P -> $efb6 $ed $d5 $b8 $5f $a2 }T T{ $005d M $005e M $00a5 M $9da3 M $efb4 M $efb5 M $efb6 M -> $a3 $9d $42 $d5 $81 $a5 $98 }T
$fa83 >PC $38 >S $a9 >A $49 >X $17 >Y $62 >P $0028 $f5 >M $0029 $4c >M $00df $59 >M $fa83 $81 >M $fa84 $df >M $fa85 $2e >M
T{ op PC S A X Y P -> $fa85 $38 $a9 $49 $17 $62 }T T{ $0028 M $0029 M $00df M $4cf5 M $fa83 M $fa84 M $fa85 M -> $f5 $4c $59 $a9 $81 $df $2e }T
$558e >PC $d6 >S $56 >A $df >X $e0 >Y $a0 >P $00be $dd >M $00bf $15 >M $00df $b2 >M $558e $81 >M $558f $df >M $5590 $82 >M
T{ op PC S A X Y P -> $5590 $d6 $56 $df $e0 $a0 }T T{ $00be M $00bf M $00df M $15dd M $558e M $558f M $5590 M -> $dd $15 $b2 $56 $81 $df $82 }T
$14cc >PC $0b >S $80 >A $27 >X $25 >Y $a6 >P $0006 $46 >M $002d $60 >M $002e $e3 >M $14cc $81 >M $14cd $06 >M $14ce $7a >M
T{ op PC S A X Y P -> $14ce $0b $80 $27 $25 $a6 }T T{ $0006 M $002d M $002e M $14cc M $14cd M $14ce M $e360 M -> $46 $60 $e3 $81 $06 $7a $80 }T
$bd24 >PC $54 >S $33 >A $76 >X $51 >Y $a1 >P $0055 $c8 >M $0056 $02 >M $00df $4c >M $bd24 $81 >M $bd25 $df >M $bd26 $e9 >M
T{ op PC S A X Y P -> $bd26 $54 $33 $76 $51 $a1 }T T{ $0055 M $0056 M $00df M $02c8 M $bd24 M $bd25 M $bd26 M -> $c8 $02 $4c $33 $81 $df $e9 }T
$bf54 >PC $98 >S $b7 >A $0c >X $da >Y $63 >P $00a4 $20 >M $00b0 $f4 >M $00b1 $bc >M $bf54 $81 >M $bf55 $a4 >M $bf56 $dd >M
T{ op PC S A X Y P -> $bf56 $98 $b7 $0c $da $63 }T T{ $00a4 M $00b0 M $00b1 M $bcf4 M $bf54 M $bf55 M $bf56 M -> $20 $f4 $bc $b7 $81 $a4 $dd }T
$bce3 >PC $f7 >S $59 >A $ec >X $b1 >Y $21 >P $0007 $d6 >M $00f3 $b8 >M $00f4 $e8 >M $bce3 $81 >M $bce4 $07 >M $bce5 $9b >M
T{ op PC S A X Y P -> $bce5 $f7 $59 $ec $b1 $21 }T T{ $0007 M $00f3 M $00f4 M $bce3 M $bce4 M $bce5 M $e8b8 M -> $d6 $b8 $e8 $81 $07 $9b $59 }T
$d976 >PC $4e >S $05 >A $41 >X $cf >Y $e7 >P $00ac $a1 >M $00ed $22 >M $00ee $e3 >M $d976 $81 >M $d977 $ac >M $d978 $9a >M
T{ op PC S A X Y P -> $d978 $4e $05 $41 $cf $e7 }T T{ $00ac M $00ed M $00ee M $d976 M $d977 M $d978 M $e322 M -> $a1 $22 $e3 $81 $ac $9a $05 }T
$8cb8 >PC $98 >S $1a >A $2d >X $d9 >Y $e1 >P $00c8 $4a >M $00f5 $53 >M $00f6 $92 >M $8cb8 $81 >M $8cb9 $c8 >M $8cba $dc >M
T{ op PC S A X Y P -> $8cba $98 $1a $2d $d9 $e1 }T T{ $00c8 M $00f5 M $00f6 M $8cb8 M $8cb9 M $8cba M $9253 M -> $4a $53 $92 $81 $c8 $dc $1a }T
$2ee1 >PC $4c >S $10 >A $a0 >X $7b >Y $66 >P $003b $21 >M $003c $4f >M $009b $8e >M $2ee1 $81 >M $2ee2 $9b >M $2ee3 $7e >M
T{ op PC S A X Y P -> $2ee3 $4c $10 $a0 $7b $66 }T T{ $003b M $003c M $009b M $2ee1 M $2ee2 M $2ee3 M $4f21 M -> $21 $4f $8e $81 $9b $7e $10 }T
$3afa >PC $5e >S $91 >A $45 >X $e9 >Y $a0 >P $005e $f1 >M $00a3 $f4 >M $00a4 $44 >M $3afa $81 >M $3afb $5e >M $3afc $15 >M
T{ op PC S A X Y P -> $3afc $5e $91 $45 $e9 $a0 }T T{ $005e M $00a3 M $00a4 M $3afa M $3afb M $3afc M $44f4 M -> $f1 $f4 $44 $81 $5e $15 $91 }T
$8be3 >PC $84 >S $90 >A $01 >X $6f >Y $e7 >P $00e3 $8e >M $00e4 $ad >M $00e5 $75 >M $8be3 $81 >M $8be4 $e3 >M $8be5 $86 >M
T{ op PC S A X Y P -> $8be5 $84 $90 $01 $6f $e7 }T T{ $00e3 M $00e4 M $00e5 M $75ad M $8be3 M $8be4 M $8be5 M -> $8e $ad $75 $90 $81 $e3 $86 }T
( 82 )
$2737 >PC $db >S $da >A $9a >X $f0 >Y $a6 >P $2737 $82 >M $2738 $69 >M $2739 $20 >M
T{ op PC S A X Y P -> $2739 $db $da $9a $f0 $a6 }T T{ $2737 M $2738 M $2739 M -> $82 $69 $20 }T
$ec79 >PC $a1 >S $6d >A $ef >X $76 >Y $25 >P $ec79 $82 >M $ec7a $75 >M $ec7b $5b >M
T{ op PC S A X Y P -> $ec7b $a1 $6d $ef $76 $25 }T T{ $ec79 M $ec7a M $ec7b M -> $82 $75 $5b }T
$5f23 >PC $1e >S $a8 >A $82 >X $63 >Y $e3 >P $5f23 $82 >M $5f24 $42 >M $5f25 $7c >M
T{ op PC S A X Y P -> $5f25 $1e $a8 $82 $63 $e3 }T T{ $5f23 M $5f24 M $5f25 M -> $82 $42 $7c }T
$c303 >PC $16 >S $18 >A $b2 >X $06 >Y $27 >P $c303 $82 >M $c304 $55 >M $c305 $26 >M
T{ op PC S A X Y P -> $c305 $16 $18 $b2 $06 $27 }T T{ $c303 M $c304 M $c305 M -> $82 $55 $26 }T
$228e >PC $26 >S $0b >A $02 >X $c1 >Y $e1 >P $228e $82 >M $228f $9d >M $2290 $74 >M
T{ op PC S A X Y P -> $2290 $26 $0b $02 $c1 $e1 }T T{ $228e M $228f M $2290 M -> $82 $9d $74 }T
$d01d >PC $33 >S $8f >A $55 >X $fe >Y $22 >P $d01d $82 >M $d01e $34 >M $d01f $f2 >M
T{ op PC S A X Y P -> $d01f $33 $8f $55 $fe $22 }T T{ $d01d M $d01e M $d01f M -> $82 $34 $f2 }T
$d74f >PC $3f >S $d1 >A $9a >X $e3 >Y $23 >P $d74f $82 >M $d750 $6a >M $d751 $46 >M
T{ op PC S A X Y P -> $d751 $3f $d1 $9a $e3 $23 }T T{ $d74f M $d750 M $d751 M -> $82 $6a $46 }T
$0e3b >PC $a4 >S $95 >A $83 >X $e3 >Y $60 >P $0e3b $82 >M $0e3c $cc >M $0e3d $1f >M
T{ op PC S A X Y P -> $0e3d $a4 $95 $83 $e3 $60 }T T{ $0e3b M $0e3c M $0e3d M -> $82 $cc $1f }T
$b744 >PC $62 >S $96 >A $c6 >X $de >Y $21 >P $b744 $82 >M $b745 $8e >M $b746 $be >M
T{ op PC S A X Y P -> $b746 $62 $96 $c6 $de $21 }T T{ $b744 M $b745 M $b746 M -> $82 $8e $be }T
$7d9b >PC $55 >S $58 >A $44 >X $05 >Y $a6 >P $7d9b $82 >M $7d9c $0f >M $7d9d $10 >M
T{ op PC S A X Y P -> $7d9d $55 $58 $44 $05 $a6 }T T{ $7d9b M $7d9c M $7d9d M -> $82 $0f $10 }T
$fd9f >PC $a8 >S $7b >A $88 >X $c7 >Y $25 >P $fd9f $82 >M $fda0 $a2 >M $fda1 $e8 >M
T{ op PC S A X Y P -> $fda1 $a8 $7b $88 $c7 $25 }T T{ $fd9f M $fda0 M $fda1 M -> $82 $a2 $e8 }T
$de4e >PC $4c >S $04 >A $64 >X $69 >Y $e2 >P $de4e $82 >M $de4f $1d >M $de50 $69 >M
T{ op PC S A X Y P -> $de50 $4c $04 $64 $69 $e2 }T T{ $de4e M $de4f M $de50 M -> $82 $1d $69 }T
$a60f >PC $be >S $a4 >A $ad >X $0a >Y $e0 >P $a60f $82 >M $a610 $c7 >M $a611 $5a >M
T{ op PC S A X Y P -> $a611 $be $a4 $ad $0a $e0 }T T{ $a60f M $a610 M $a611 M -> $82 $c7 $5a }T
$d785 >PC $c5 >S $62 >A $06 >X $e5 >Y $a5 >P $d785 $82 >M $d786 $a5 >M $d787 $c7 >M
T{ op PC S A X Y P -> $d787 $c5 $62 $06 $e5 $a5 }T T{ $d785 M $d786 M $d787 M -> $82 $a5 $c7 }T
$d5de >PC $ee >S $5a >A $94 >X $45 >Y $a3 >P $d5de $82 >M $d5df $82 >M $d5e0 $af >M
T{ op PC S A X Y P -> $d5e0 $ee $5a $94 $45 $a3 }T T{ $d5de M $d5df M $d5e0 M -> $82 $82 $af }T
$bb8f >PC $46 >S $b9 >A $95 >X $c1 >Y $24 >P $bb8f $82 >M $bb90 $33 >M $bb91 $f7 >M
T{ op PC S A X Y P -> $bb91 $46 $b9 $95 $c1 $24 }T T{ $bb8f M $bb90 M $bb91 M -> $82 $33 $f7 }T
( 83 )
$bcff >PC $b8 >S $c5 >A $f6 >X $7a >Y $a0 >P $bcff $83 >M $bd00 $21 >M $bd01 $59 >M
T{ op PC S A X Y P -> $bd00 $b8 $c5 $f6 $7a $a0 }T T{ $bcff M $bd00 M $bd01 M -> $83 $21 $59 }T
$62b0 >PC $3d >S $82 >A $b1 >X $ee >Y $a6 >P $62b0 $83 >M $62b1 $d7 >M $62b2 $1a >M
T{ op PC S A X Y P -> $62b1 $3d $82 $b1 $ee $a6 }T T{ $62b0 M $62b1 M $62b2 M -> $83 $d7 $1a }T
$041b >PC $53 >S $24 >A $2b >X $df >Y $a5 >P $041b $83 >M $041c $cb >M $041d $d1 >M
T{ op PC S A X Y P -> $041c $53 $24 $2b $df $a5 }T T{ $041b M $041c M $041d M -> $83 $cb $d1 }T
$9edc >PC $2b >S $26 >A $04 >X $65 >Y $60 >P $9edc $83 >M $9edd $77 >M $9ede $ae >M
T{ op PC S A X Y P -> $9edd $2b $26 $04 $65 $60 }T T{ $9edc M $9edd M $9ede M -> $83 $77 $ae }T
$40db >PC $7e >S $46 >A $a6 >X $8b >Y $25 >P $40db $83 >M $40dc $fa >M $40dd $62 >M
T{ op PC S A X Y P -> $40dc $7e $46 $a6 $8b $25 }T T{ $40db M $40dc M $40dd M -> $83 $fa $62 }T
$ebc3 >PC $93 >S $00 >A $b5 >X $93 >Y $67 >P $ebc3 $83 >M $ebc4 $9f >M $ebc5 $6a >M
T{ op PC S A X Y P -> $ebc4 $93 $00 $b5 $93 $67 }T T{ $ebc3 M $ebc4 M $ebc5 M -> $83 $9f $6a }T
$84dc >PC $60 >S $73 >A $0c >X $4a >Y $66 >P $84dc $83 >M $84dd $45 >M $84de $d3 >M
T{ op PC S A X Y P -> $84dd $60 $73 $0c $4a $66 }T T{ $84dc M $84dd M $84de M -> $83 $45 $d3 }T
$7322 >PC $f0 >S $fe >A $0a >X $05 >Y $a0 >P $7322 $83 >M $7323 $26 >M $7324 $2d >M
T{ op PC S A X Y P -> $7323 $f0 $fe $0a $05 $a0 }T T{ $7322 M $7323 M $7324 M -> $83 $26 $2d }T
$6bdf >PC $24 >S $e7 >A $ea >X $85 >Y $21 >P $6bdf $83 >M $6be0 $a2 >M $6be1 $1a >M
T{ op PC S A X Y P -> $6be0 $24 $e7 $ea $85 $21 }T T{ $6bdf M $6be0 M $6be1 M -> $83 $a2 $1a }T
$5fc3 >PC $7a >S $61 >A $5c >X $a8 >Y $e3 >P $5fc3 $83 >M $5fc4 $c5 >M $5fc5 $73 >M
T{ op PC S A X Y P -> $5fc4 $7a $61 $5c $a8 $e3 }T T{ $5fc3 M $5fc4 M $5fc5 M -> $83 $c5 $73 }T
$c514 >PC $39 >S $bb >A $7b >X $0d >Y $63 >P $c514 $83 >M $c515 $42 >M $c516 $b6 >M
T{ op PC S A X Y P -> $c515 $39 $bb $7b $0d $63 }T T{ $c514 M $c515 M $c516 M -> $83 $42 $b6 }T
$c4a3 >PC $2c >S $91 >A $c8 >X $a0 >Y $a1 >P $c4a3 $83 >M $c4a4 $56 >M $c4a5 $e7 >M
T{ op PC S A X Y P -> $c4a4 $2c $91 $c8 $a0 $a1 }T T{ $c4a3 M $c4a4 M $c4a5 M -> $83 $56 $e7 }T
$408f >PC $35 >S $10 >A $b5 >X $42 >Y $66 >P $408f $83 >M $4090 $74 >M $4091 $6f >M
T{ op PC S A X Y P -> $4090 $35 $10 $b5 $42 $66 }T T{ $408f M $4090 M $4091 M -> $83 $74 $6f }T
$1ddc >PC $91 >S $32 >A $2b >X $db >Y $a6 >P $1ddc $83 >M $1ddd $ac >M $1dde $5c >M
T{ op PC S A X Y P -> $1ddd $91 $32 $2b $db $a6 }T T{ $1ddc M $1ddd M $1dde M -> $83 $ac $5c }T
$755d >PC $df >S $3f >A $fb >X $d2 >Y $21 >P $755d $83 >M $755e $b8 >M $755f $77 >M
T{ op PC S A X Y P -> $755e $df $3f $fb $d2 $21 }T T{ $755d M $755e M $755f M -> $83 $b8 $77 }T
$8834 >PC $c0 >S $d5 >A $fc >X $a8 >Y $e2 >P $8834 $83 >M $8835 $53 >M $8836 $71 >M
T{ op PC S A X Y P -> $8835 $c0 $d5 $fc $a8 $e2 }T T{ $8834 M $8835 M $8836 M -> $83 $53 $71 }T
( 84 )
$6248 >PC $9c >S $36 >A $1d >X $31 >Y $a7 >P $6248 $84 >M $6249 $0f >M $624a $2a >M
T{ op PC S A X Y P -> $624a $9c $36 $1d $31 $a7 }T T{ $000f M $6248 M $6249 M $624a M -> $31 $84 $0f $2a }T
$1ac2 >PC $40 >S $83 >A $9c >X $0c >Y $e5 >P $1ac2 $84 >M $1ac3 $55 >M $1ac4 $c8 >M
T{ op PC S A X Y P -> $1ac4 $40 $83 $9c $0c $e5 }T T{ $0055 M $1ac2 M $1ac3 M $1ac4 M -> $0c $84 $55 $c8 }T
$2654 >PC $6f >S $cc >A $7e >X $24 >Y $67 >P $2654 $84 >M $2655 $75 >M $2656 $0c >M
T{ op PC S A X Y P -> $2656 $6f $cc $7e $24 $67 }T T{ $0075 M $2654 M $2655 M $2656 M -> $24 $84 $75 $0c }T
$68cc >PC $d4 >S $b9 >A $fc >X $c5 >Y $63 >P $68cc $84 >M $68cd $b2 >M $68ce $ca >M
T{ op PC S A X Y P -> $68ce $d4 $b9 $fc $c5 $63 }T T{ $00b2 M $68cc M $68cd M $68ce M -> $c5 $84 $b2 $ca }T
$0adf >PC $80 >S $41 >A $d5 >X $4a >Y $66 >P $0adf $84 >M $0ae0 $b4 >M $0ae1 $03 >M
T{ op PC S A X Y P -> $0ae1 $80 $41 $d5 $4a $66 }T T{ $00b4 M $0adf M $0ae0 M $0ae1 M -> $4a $84 $b4 $03 }T
$2f24 >PC $09 >S $8b >A $8c >X $88 >Y $a3 >P $2f24 $84 >M $2f25 $f2 >M $2f26 $a4 >M
T{ op PC S A X Y P -> $2f26 $09 $8b $8c $88 $a3 }T T{ $00f2 M $2f24 M $2f25 M $2f26 M -> $88 $84 $f2 $a4 }T
$6075 >PC $5f >S $74 >A $70 >X $07 >Y $22 >P $6075 $84 >M $6076 $8d >M $6077 $4b >M
T{ op PC S A X Y P -> $6077 $5f $74 $70 $07 $22 }T T{ $008d M $6075 M $6076 M $6077 M -> $07 $84 $8d $4b }T
$47a2 >PC $15 >S $7b >A $4d >X $dc >Y $61 >P $47a2 $84 >M $47a3 $11 >M $47a4 $89 >M
T{ op PC S A X Y P -> $47a4 $15 $7b $4d $dc $61 }T T{ $0011 M $47a2 M $47a3 M $47a4 M -> $dc $84 $11 $89 }T
$a3ef >PC $51 >S $38 >A $24 >X $e8 >Y $a5 >P $a3ef $84 >M $a3f0 $4c >M $a3f1 $cb >M
T{ op PC S A X Y P -> $a3f1 $51 $38 $24 $e8 $a5 }T T{ $004c M $a3ef M $a3f0 M $a3f1 M -> $e8 $84 $4c $cb }T
$4e38 >PC $d5 >S $1f >A $75 >X $79 >Y $a3 >P $4e38 $84 >M $4e39 $14 >M $4e3a $9f >M
T{ op PC S A X Y P -> $4e3a $d5 $1f $75 $79 $a3 }T T{ $0014 M $4e38 M $4e39 M $4e3a M -> $79 $84 $14 $9f }T
$ce3b >PC $cd >S $ec >A $87 >X $e2 >Y $e2 >P $ce3b $84 >M $ce3c $f0 >M $ce3d $1e >M
T{ op PC S A X Y P -> $ce3d $cd $ec $87 $e2 $e2 }T T{ $00f0 M $ce3b M $ce3c M $ce3d M -> $e2 $84 $f0 $1e }T
$acca >PC $01 >S $36 >A $73 >X $0f >Y $25 >P $acca $84 >M $accb $ce >M $accc $1b >M
T{ op PC S A X Y P -> $accc $01 $36 $73 $0f $25 }T T{ $00ce M $acca M $accb M $accc M -> $0f $84 $ce $1b }T
$43a6 >PC $36 >S $0c >A $eb >X $a0 >Y $24 >P $43a6 $84 >M $43a7 $e3 >M $43a8 $31 >M
T{ op PC S A X Y P -> $43a8 $36 $0c $eb $a0 $24 }T T{ $00e3 M $43a6 M $43a7 M $43a8 M -> $a0 $84 $e3 $31 }T
$a741 >PC $2e >S $6d >A $26 >X $72 >Y $25 >P $a741 $84 >M $a742 $0d >M $a743 $09 >M
T{ op PC S A X Y P -> $a743 $2e $6d $26 $72 $25 }T T{ $000d M $a741 M $a742 M $a743 M -> $72 $84 $0d $09 }T
$5b77 >PC $be >S $30 >A $4f >X $99 >Y $e4 >P $5b77 $84 >M $5b78 $47 >M $5b79 $42 >M
T{ op PC S A X Y P -> $5b79 $be $30 $4f $99 $e4 }T T{ $0047 M $5b77 M $5b78 M $5b79 M -> $99 $84 $47 $42 }T
$f6d1 >PC $18 >S $49 >A $ad >X $b6 >Y $e7 >P $f6d1 $84 >M $f6d2 $b8 >M $f6d3 $e9 >M
T{ op PC S A X Y P -> $f6d3 $18 $49 $ad $b6 $e7 }T T{ $00b8 M $f6d1 M $f6d2 M $f6d3 M -> $b6 $84 $b8 $e9 }T
( 85 )
$6f51 >PC $10 >S $f8 >A $65 >X $4e >Y $25 >P $6f51 $85 >M $6f52 $b8 >M $6f53 $8a >M
T{ op PC S A X Y P -> $6f53 $10 $f8 $65 $4e $25 }T T{ $00b8 M $6f51 M $6f52 M $6f53 M -> $f8 $85 $b8 $8a }T
$9271 >PC $76 >S $10 >A $74 >X $53 >Y $a7 >P $9271 $85 >M $9272 $9b >M $9273 $d0 >M
T{ op PC S A X Y P -> $9273 $76 $10 $74 $53 $a7 }T T{ $009b M $9271 M $9272 M $9273 M -> $10 $85 $9b $d0 }T
$e782 >PC $d9 >S $48 >A $0c >X $d3 >Y $61 >P $e782 $85 >M $e783 $f7 >M $e784 $e8 >M
T{ op PC S A X Y P -> $e784 $d9 $48 $0c $d3 $61 }T T{ $00f7 M $e782 M $e783 M $e784 M -> $48 $85 $f7 $e8 }T
$742d >PC $0b >S $e7 >A $b0 >X $6c >Y $60 >P $742d $85 >M $742e $82 >M $742f $12 >M
T{ op PC S A X Y P -> $742f $0b $e7 $b0 $6c $60 }T T{ $0082 M $742d M $742e M $742f M -> $e7 $85 $82 $12 }T
$7fa4 >PC $4f >S $e0 >A $12 >X $45 >Y $e5 >P $7fa4 $85 >M $7fa5 $3f >M $7fa6 $48 >M
T{ op PC S A X Y P -> $7fa6 $4f $e0 $12 $45 $e5 }T T{ $003f M $7fa4 M $7fa5 M $7fa6 M -> $e0 $85 $3f $48 }T
$b78b >PC $d8 >S $47 >A $5a >X $b5 >Y $20 >P $b78b $85 >M $b78c $a8 >M $b78d $02 >M
T{ op PC S A X Y P -> $b78d $d8 $47 $5a $b5 $20 }T T{ $00a8 M $b78b M $b78c M $b78d M -> $47 $85 $a8 $02 }T
$f2e0 >PC $9c >S $3e >A $b6 >X $c8 >Y $e2 >P $f2e0 $85 >M $f2e1 $1b >M $f2e2 $1d >M
T{ op PC S A X Y P -> $f2e2 $9c $3e $b6 $c8 $e2 }T T{ $001b M $f2e0 M $f2e1 M $f2e2 M -> $3e $85 $1b $1d }T
$138a >PC $70 >S $fd >A $36 >X $ae >Y $24 >P $138a $85 >M $138b $25 >M $138c $55 >M
T{ op PC S A X Y P -> $138c $70 $fd $36 $ae $24 }T T{ $0025 M $138a M $138b M $138c M -> $fd $85 $25 $55 }T
$9eac >PC $2e >S $08 >A $2f >X $3e >Y $e5 >P $9eac $85 >M $9ead $eb >M $9eae $20 >M
T{ op PC S A X Y P -> $9eae $2e $08 $2f $3e $e5 }T T{ $00eb M $9eac M $9ead M $9eae M -> $08 $85 $eb $20 }T
$4ea2 >PC $4e >S $89 >A $c8 >X $c2 >Y $24 >P $4ea2 $85 >M $4ea3 $f1 >M $4ea4 $4c >M
T{ op PC S A X Y P -> $4ea4 $4e $89 $c8 $c2 $24 }T T{ $00f1 M $4ea2 M $4ea3 M $4ea4 M -> $89 $85 $f1 $4c }T
$4c76 >PC $88 >S $bf >A $04 >X $73 >Y $66 >P $4c76 $85 >M $4c77 $4d >M $4c78 $6f >M
T{ op PC S A X Y P -> $4c78 $88 $bf $04 $73 $66 }T T{ $004d M $4c76 M $4c77 M $4c78 M -> $bf $85 $4d $6f }T
$580c >PC $d2 >S $28 >A $90 >X $f2 >Y $22 >P $580c $85 >M $580d $1c >M $580e $0d >M
T{ op PC S A X Y P -> $580e $d2 $28 $90 $f2 $22 }T T{ $001c M $580c M $580d M $580e M -> $28 $85 $1c $0d }T
$1081 >PC $d0 >S $5b >A $7c >X $a3 >Y $a5 >P $1081 $85 >M $1082 $ff >M $1083 $3d >M
T{ op PC S A X Y P -> $1083 $d0 $5b $7c $a3 $a5 }T T{ $00ff M $1081 M $1082 M $1083 M -> $5b $85 $ff $3d }T
$7405 >PC $ae >S $ea >A $f4 >X $72 >Y $a3 >P $7405 $85 >M $7406 $c2 >M $7407 $a7 >M
T{ op PC S A X Y P -> $7407 $ae $ea $f4 $72 $a3 }T T{ $00c2 M $7405 M $7406 M $7407 M -> $ea $85 $c2 $a7 }T
$97d8 >PC $da >S $27 >A $62 >X $c3 >Y $e6 >P $97d8 $85 >M $97d9 $68 >M $97da $60 >M
T{ op PC S A X Y P -> $97da $da $27 $62 $c3 $e6 }T T{ $0068 M $97d8 M $97d9 M $97da M -> $27 $85 $68 $60 }T
$187e >PC $8d >S $ff >A $23 >X $0d >Y $61 >P $187e $85 >M $187f $05 >M $1880 $c4 >M
T{ op PC S A X Y P -> $1880 $8d $ff $23 $0d $61 }T T{ $0005 M $187e M $187f M $1880 M -> $ff $85 $05 $c4 }T
( 86 )
$e8b8 >PC $1d >S $37 >A $66 >X $ad >Y $26 >P $e8b8 $86 >M $e8b9 $9e >M $e8ba $77 >M
T{ op PC S A X Y P -> $e8ba $1d $37 $66 $ad $26 }T T{ $009e M $e8b8 M $e8b9 M $e8ba M -> $66 $86 $9e $77 }T
$8357 >PC $ad >S $29 >A $11 >X $ef >Y $25 >P $8357 $86 >M $8358 $6f >M $8359 $f4 >M
T{ op PC S A X Y P -> $8359 $ad $29 $11 $ef $25 }T T{ $006f M $8357 M $8358 M $8359 M -> $11 $86 $6f $f4 }T
$77e4 >PC $d8 >S $b3 >A $73 >X $2b >Y $20 >P $77e4 $86 >M $77e5 $25 >M $77e6 $bb >M
T{ op PC S A X Y P -> $77e6 $d8 $b3 $73 $2b $20 }T T{ $0025 M $77e4 M $77e5 M $77e6 M -> $73 $86 $25 $bb }T
$2981 >PC $d0 >S $39 >A $29 >X $b1 >Y $64 >P $2981 $86 >M $2982 $05 >M $2983 $2a >M
T{ op PC S A X Y P -> $2983 $d0 $39 $29 $b1 $64 }T T{ $0005 M $2981 M $2982 M $2983 M -> $29 $86 $05 $2a }T
$65b0 >PC $a7 >S $1d >A $52 >X $76 >Y $24 >P $65b0 $86 >M $65b1 $a9 >M $65b2 $b4 >M
T{ op PC S A X Y P -> $65b2 $a7 $1d $52 $76 $24 }T T{ $00a9 M $65b0 M $65b1 M $65b2 M -> $52 $86 $a9 $b4 }T
$25f5 >PC $2c >S $1c >A $95 >X $77 >Y $e2 >P $25f5 $86 >M $25f6 $bb >M $25f7 $cc >M
T{ op PC S A X Y P -> $25f7 $2c $1c $95 $77 $e2 }T T{ $00bb M $25f5 M $25f6 M $25f7 M -> $95 $86 $bb $cc }T
$45bb >PC $5a >S $39 >A $a9 >X $69 >Y $64 >P $45bb $86 >M $45bc $61 >M $45bd $ed >M
T{ op PC S A X Y P -> $45bd $5a $39 $a9 $69 $64 }T T{ $0061 M $45bb M $45bc M $45bd M -> $a9 $86 $61 $ed }T
$1664 >PC $29 >S $db >A $44 >X $e4 >Y $64 >P $1664 $86 >M $1665 $20 >M $1666 $36 >M
T{ op PC S A X Y P -> $1666 $29 $db $44 $e4 $64 }T T{ $0020 M $1664 M $1665 M $1666 M -> $44 $86 $20 $36 }T
$021d >PC $89 >S $00 >A $b9 >X $ea >Y $e1 >P $021d $86 >M $021e $59 >M $021f $8f >M
T{ op PC S A X Y P -> $021f $89 $00 $b9 $ea $e1 }T T{ $0059 M $021d M $021e M $021f M -> $b9 $86 $59 $8f }T
$f3b8 >PC $69 >S $48 >A $6b >X $c4 >Y $65 >P $f3b8 $86 >M $f3b9 $7b >M $f3ba $d3 >M
T{ op PC S A X Y P -> $f3ba $69 $48 $6b $c4 $65 }T T{ $007b M $f3b8 M $f3b9 M $f3ba M -> $6b $86 $7b $d3 }T
$94e9 >PC $68 >S $5e >A $f0 >X $a2 >Y $20 >P $94e9 $86 >M $94ea $fd >M $94eb $ce >M
T{ op PC S A X Y P -> $94eb $68 $5e $f0 $a2 $20 }T T{ $00fd M $94e9 M $94ea M $94eb M -> $f0 $86 $fd $ce }T
$b147 >PC $52 >S $79 >A $75 >X $e2 >Y $21 >P $b147 $86 >M $b148 $fa >M $b149 $60 >M
T{ op PC S A X Y P -> $b149 $52 $79 $75 $e2 $21 }T T{ $00fa M $b147 M $b148 M $b149 M -> $75 $86 $fa $60 }T
$023d >PC $75 >S $9a >A $c9 >X $d3 >Y $a7 >P $023d $86 >M $023e $fd >M $023f $d1 >M
T{ op PC S A X Y P -> $023f $75 $9a $c9 $d3 $a7 }T T{ $00fd M $023d M $023e M $023f M -> $c9 $86 $fd $d1 }T
$87a4 >PC $e9 >S $ba >A $7f >X $8b >Y $22 >P $87a4 $86 >M $87a5 $99 >M $87a6 $b5 >M
T{ op PC S A X Y P -> $87a6 $e9 $ba $7f $8b $22 }T T{ $0099 M $87a4 M $87a5 M $87a6 M -> $7f $86 $99 $b5 }T
$005d >PC $91 >S $6c >A $5a >X $7b >Y $a7 >P $005d $86 >M $005e $b0 >M $005f $e6 >M
T{ op PC S A X Y P -> $005f $91 $6c $5a $7b $a7 }T T{ $005d M $005e M $005f M $00b0 M -> $86 $b0 $e6 $5a }T
$933f >PC $05 >S $48 >A $a0 >X $eb >Y $a5 >P $933f $86 >M $9340 $28 >M $9341 $ab >M
T{ op PC S A X Y P -> $9341 $05 $48 $a0 $eb $a5 }T T{ $0028 M $933f M $9340 M $9341 M -> $a0 $86 $28 $ab }T
( 87 )
$dcc6 >PC $26 >S $bc >A $6a >X $3a >Y $e3 >P $007d $67 >M $dcc6 $87 >M $dcc7 $7d >M $dcc8 $f5 >M
T{ op PC S A X Y P -> $dcc8 $26 $bc $6a $3a $e3 }T T{ $007d M $dcc6 M $dcc7 M $dcc8 M -> $67 $87 $7d $f5 }T
$4b08 >PC $70 >S $52 >A $96 >X $a5 >Y $e0 >P $003b $21 >M $4b08 $87 >M $4b09 $3b >M $4b0a $46 >M
T{ op PC S A X Y P -> $4b0a $70 $52 $96 $a5 $e0 }T T{ $003b M $4b08 M $4b09 M $4b0a M -> $21 $87 $3b $46 }T
$132f >PC $bf >S $62 >A $4f >X $ce >Y $a1 >P $0094 $79 >M $132f $87 >M $1330 $94 >M $1331 $41 >M
T{ op PC S A X Y P -> $1331 $bf $62 $4f $ce $a1 }T T{ $0094 M $132f M $1330 M $1331 M -> $79 $87 $94 $41 }T
$b2a7 >PC $b1 >S $46 >A $78 >X $4d >Y $24 >P $0003 $8f >M $b2a7 $87 >M $b2a8 $03 >M $b2a9 $75 >M
T{ op PC S A X Y P -> $b2a9 $b1 $46 $78 $4d $24 }T T{ $0003 M $b2a7 M $b2a8 M $b2a9 M -> $8f $87 $03 $75 }T
$b78a >PC $23 >S $11 >A $70 >X $58 >Y $21 >P $004f $6c >M $b78a $87 >M $b78b $4f >M $b78c $7f >M
T{ op PC S A X Y P -> $b78c $23 $11 $70 $58 $21 }T T{ $004f M $b78a M $b78b M $b78c M -> $6d $87 $4f $7f }T
$543c >PC $e0 >S $bb >A $60 >X $ee >Y $26 >P $00c2 $ed >M $543c $87 >M $543d $c2 >M $543e $4a >M
T{ op PC S A X Y P -> $543e $e0 $bb $60 $ee $26 }T T{ $00c2 M $543c M $543d M $543e M -> $ed $87 $c2 $4a }T
$a16b >PC $1b >S $2d >A $7c >X $83 >Y $a5 >P $00af $e8 >M $a16b $87 >M $a16c $af >M $a16d $f8 >M
T{ op PC S A X Y P -> $a16d $1b $2d $7c $83 $a5 }T T{ $00af M $a16b M $a16c M $a16d M -> $e9 $87 $af $f8 }T
$b416 >PC $eb >S $3c >A $d2 >X $20 >Y $62 >P $0004 $8f >M $b416 $87 >M $b417 $04 >M $b418 $26 >M
T{ op PC S A X Y P -> $b418 $eb $3c $d2 $20 $62 }T T{ $0004 M $b416 M $b417 M $b418 M -> $8f $87 $04 $26 }T
$0602 >PC $1c >S $41 >A $69 >X $e0 >Y $e5 >P $0065 $26 >M $0602 $87 >M $0603 $65 >M $0604 $15 >M
T{ op PC S A X Y P -> $0604 $1c $41 $69 $e0 $e5 }T T{ $0065 M $0602 M $0603 M $0604 M -> $27 $87 $65 $15 }T
$334d >PC $f4 >S $66 >A $97 >X $99 >Y $a7 >P $00be $ca >M $334d $87 >M $334e $be >M $334f $de >M
T{ op PC S A X Y P -> $334f $f4 $66 $97 $99 $a7 }T T{ $00be M $334d M $334e M $334f M -> $cb $87 $be $de }T
$f143 >PC $ba >S $34 >A $43 >X $d7 >Y $65 >P $00ca $13 >M $f143 $87 >M $f144 $ca >M $f145 $65 >M
T{ op PC S A X Y P -> $f145 $ba $34 $43 $d7 $65 }T T{ $00ca M $f143 M $f144 M $f145 M -> $13 $87 $ca $65 }T
$783c >PC $92 >S $85 >A $c0 >X $e5 >Y $e1 >P $0094 $67 >M $783c $87 >M $783d $94 >M $783e $df >M
T{ op PC S A X Y P -> $783e $92 $85 $c0 $e5 $e1 }T T{ $0094 M $783c M $783d M $783e M -> $67 $87 $94 $df }T
$b1b8 >PC $e2 >S $dd >A $f7 >X $bd >Y $e4 >P $0037 $cc >M $b1b8 $87 >M $b1b9 $37 >M $b1ba $88 >M
T{ op PC S A X Y P -> $b1ba $e2 $dd $f7 $bd $e4 }T T{ $0037 M $b1b8 M $b1b9 M $b1ba M -> $cd $87 $37 $88 }T
$f0d5 >PC $69 >S $38 >A $52 >X $2b >Y $a0 >P $005e $31 >M $f0d5 $87 >M $f0d6 $5e >M $f0d7 $cb >M
T{ op PC S A X Y P -> $f0d7 $69 $38 $52 $2b $a0 }T T{ $005e M $f0d5 M $f0d6 M $f0d7 M -> $31 $87 $5e $cb }T
$9b43 >PC $b2 >S $b1 >A $d9 >X $5e >Y $64 >P $00fb $cd >M $9b43 $87 >M $9b44 $fb >M $9b45 $80 >M
T{ op PC S A X Y P -> $9b45 $b2 $b1 $d9 $5e $64 }T T{ $00fb M $9b43 M $9b44 M $9b45 M -> $cd $87 $fb $80 }T
$e1a3 >PC $65 >S $7d >A $77 >X $74 >Y $65 >P $00fb $76 >M $e1a3 $87 >M $e1a4 $fb >M $e1a5 $98 >M
T{ op PC S A X Y P -> $e1a5 $65 $7d $77 $74 $65 }T T{ $00fb M $e1a3 M $e1a4 M $e1a5 M -> $77 $87 $fb $98 }T
( 88 )
$1b17 >PC $7b >S $08 >A $3b >X $ff >Y $e1 >P $1b17 $88 >M $1b18 $87 >M $1b19 $d6 >M
T{ op PC S A X Y P -> $1b18 $7b $08 $3b $fe $e1 }T T{ $1b17 M $1b18 M $1b19 M -> $88 $87 $d6 }T
$9968 >PC $44 >S $24 >A $6b >X $6a >Y $a5 >P $9968 $88 >M $9969 $87 >M $996a $15 >M
T{ op PC S A X Y P -> $9969 $44 $24 $6b $69 $25 }T T{ $9968 M $9969 M $996a M -> $88 $87 $15 }T
$fc6e >PC $3d >S $5c >A $e8 >X $c1 >Y $25 >P $fc6e $88 >M $fc6f $04 >M $fc70 $7e >M
T{ op PC S A X Y P -> $fc6f $3d $5c $e8 $c0 $a5 }T T{ $fc6e M $fc6f M $fc70 M -> $88 $04 $7e }T
$a25f >PC $83 >S $5a >A $2b >X $00 >Y $64 >P $a25f $88 >M $a260 $67 >M $a261 $95 >M
T{ op PC S A X Y P -> $a260 $83 $5a $2b $ff $e4 }T T{ $a25f M $a260 M $a261 M -> $88 $67 $95 }T
$ff7b >PC $82 >S $00 >A $21 >X $79 >Y $24 >P $ff7b $88 >M $ff7c $62 >M $ff7d $fc >M
T{ op PC S A X Y P -> $ff7c $82 $00 $21 $78 $24 }T T{ $ff7b M $ff7c M $ff7d M -> $88 $62 $fc }T
$598f >PC $39 >S $7d >A $c0 >X $51 >Y $65 >P $598f $88 >M $5990 $d9 >M $5991 $cd >M
T{ op PC S A X Y P -> $5990 $39 $7d $c0 $50 $65 }T T{ $598f M $5990 M $5991 M -> $88 $d9 $cd }T
$408e >PC $b7 >S $bf >A $c9 >X $bb >Y $25 >P $408e $88 >M $408f $eb >M $4090 $50 >M
T{ op PC S A X Y P -> $408f $b7 $bf $c9 $ba $a5 }T T{ $408e M $408f M $4090 M -> $88 $eb $50 }T
$b370 >PC $c5 >S $43 >A $b0 >X $f4 >Y $60 >P $b370 $88 >M $b371 $20 >M $b372 $17 >M
T{ op PC S A X Y P -> $b371 $c5 $43 $b0 $f3 $e0 }T T{ $b370 M $b371 M $b372 M -> $88 $20 $17 }T
$ee18 >PC $d2 >S $49 >A $fe >X $ac >Y $27 >P $ee18 $88 >M $ee19 $3f >M $ee1a $b7 >M
T{ op PC S A X Y P -> $ee19 $d2 $49 $fe $ab $a5 }T T{ $ee18 M $ee19 M $ee1a M -> $88 $3f $b7 }T
$2661 >PC $de >S $27 >A $42 >X $82 >Y $24 >P $2661 $88 >M $2662 $a5 >M $2663 $3e >M
T{ op PC S A X Y P -> $2662 $de $27 $42 $81 $a4 }T T{ $2661 M $2662 M $2663 M -> $88 $a5 $3e }T
$cfc7 >PC $04 >S $15 >A $68 >X $0b >Y $a4 >P $cfc7 $88 >M $cfc8 $cb >M $cfc9 $21 >M
T{ op PC S A X Y P -> $cfc8 $04 $15 $68 $0a $24 }T T{ $cfc7 M $cfc8 M $cfc9 M -> $88 $cb $21 }T
$72f6 >PC $ca >S $21 >A $d8 >X $2e >Y $a3 >P $72f6 $88 >M $72f7 $28 >M $72f8 $19 >M
T{ op PC S A X Y P -> $72f7 $ca $21 $d8 $2d $21 }T T{ $72f6 M $72f7 M $72f8 M -> $88 $28 $19 }T
$6524 >PC $40 >S $5c >A $c3 >X $5f >Y $21 >P $6524 $88 >M $6525 $c3 >M $6526 $58 >M
T{ op PC S A X Y P -> $6525 $40 $5c $c3 $5e $21 }T T{ $6524 M $6525 M $6526 M -> $88 $c3 $58 }T
$9d83 >PC $b6 >S $bf >A $3f >X $6b >Y $65 >P $9d83 $88 >M $9d84 $26 >M $9d85 $80 >M
T{ op PC S A X Y P -> $9d84 $b6 $bf $3f $6a $65 }T T{ $9d83 M $9d84 M $9d85 M -> $88 $26 $80 }T
$f8c7 >PC $e8 >S $07 >A $12 >X $33 >Y $62 >P $f8c7 $88 >M $f8c8 $ed >M $f8c9 $d7 >M
T{ op PC S A X Y P -> $f8c8 $e8 $07 $12 $32 $60 }T T{ $f8c7 M $f8c8 M $f8c9 M -> $88 $ed $d7 }T
$564d >PC $43 >S $41 >A $d1 >X $82 >Y $25 >P $564d $88 >M $564e $83 >M $564f $93 >M
T{ op PC S A X Y P -> $564e $43 $41 $d1 $81 $a5 }T T{ $564d M $564e M $564f M -> $88 $83 $93 }T
( 89 )
$90cf >PC $f2 >S $f7 >A $b0 >X $05 >Y $63 >P $90cf $89 >M $90d0 $a1 >M $90d1 $fd >M
T{ op PC S A X Y P -> $90d1 $f2 $f7 $b0 $05 $61 }T T{ $90cf M $90d0 M $90d1 M -> $89 $a1 $fd }T
$e7d1 >PC $52 >S $4c >A $86 >X $f4 >Y $e2 >P $e7d1 $89 >M $e7d2 $fa >M $e7d3 $eb >M
T{ op PC S A X Y P -> $e7d3 $52 $4c $86 $f4 $e0 }T T{ $e7d1 M $e7d2 M $e7d3 M -> $89 $fa $eb }T
$0397 >PC $c3 >S $de >A $6d >X $28 >Y $a7 >P $0397 $89 >M $0398 $ed >M $0399 $79 >M
T{ op PC S A X Y P -> $0399 $c3 $de $6d $28 $a5 }T T{ $0397 M $0398 M $0399 M -> $89 $ed $79 }T
$5296 >PC $45 >S $56 >A $98 >X $1d >Y $e5 >P $5296 $89 >M $5297 $07 >M $5298 $47 >M
T{ op PC S A X Y P -> $5298 $45 $56 $98 $1d $e5 }T T{ $5296 M $5297 M $5298 M -> $89 $07 $47 }T
$c3ed >PC $3b >S $12 >A $70 >X $07 >Y $a5 >P $c3ed $89 >M $c3ee $55 >M $c3ef $37 >M
T{ op PC S A X Y P -> $c3ef $3b $12 $70 $07 $a5 }T T{ $c3ed M $c3ee M $c3ef M -> $89 $55 $37 }T
$2ecb >PC $2f >S $4b >A $02 >X $3b >Y $a4 >P $2ecb $89 >M $2ecc $65 >M $2ecd $6b >M
T{ op PC S A X Y P -> $2ecd $2f $4b $02 $3b $a4 }T T{ $2ecb M $2ecc M $2ecd M -> $89 $65 $6b }T
$928c >PC $2a >S $6f >A $f8 >X $5e >Y $23 >P $928c $89 >M $928d $65 >M $928e $ac >M
T{ op PC S A X Y P -> $928e $2a $6f $f8 $5e $21 }T T{ $928c M $928d M $928e M -> $89 $65 $ac }T
$4cba >PC $4c >S $e5 >A $92 >X $8d >Y $22 >P $4cba $89 >M $4cbb $f4 >M $4cbc $ac >M
T{ op PC S A X Y P -> $4cbc $4c $e5 $92 $8d $20 }T T{ $4cba M $4cbb M $4cbc M -> $89 $f4 $ac }T
$549b >PC $88 >S $e0 >A $d7 >X $59 >Y $e1 >P $549b $89 >M $549c $4c >M $549d $43 >M
T{ op PC S A X Y P -> $549d $88 $e0 $d7 $59 $e1 }T T{ $549b M $549c M $549d M -> $89 $4c $43 }T
$497d >PC $b7 >S $5e >A $39 >X $91 >Y $20 >P $497d $89 >M $497e $9b >M $497f $dd >M
T{ op PC S A X Y P -> $497f $b7 $5e $39 $91 $20 }T T{ $497d M $497e M $497f M -> $89 $9b $dd }T
$bee9 >PC $df >S $49 >A $f8 >X $82 >Y $a6 >P $bee9 $89 >M $beea $63 >M $beeb $af >M
T{ op PC S A X Y P -> $beeb $df $49 $f8 $82 $a4 }T T{ $bee9 M $beea M $beeb M -> $89 $63 $af }T
$1a75 >PC $a5 >S $bd >A $0d >X $2a >Y $e2 >P $1a75 $89 >M $1a76 $42 >M $1a77 $e0 >M
T{ op PC S A X Y P -> $1a77 $a5 $bd $0d $2a $e2 }T T{ $1a75 M $1a76 M $1a77 M -> $89 $42 $e0 }T
$58e2 >PC $1b >S $20 >A $0b >X $39 >Y $27 >P $58e2 $89 >M $58e3 $11 >M $58e4 $92 >M
T{ op PC S A X Y P -> $58e4 $1b $20 $0b $39 $27 }T T{ $58e2 M $58e3 M $58e4 M -> $89 $11 $92 }T
$4ed1 >PC $0a >S $53 >A $94 >X $41 >Y $60 >P $4ed1 $89 >M $4ed2 $52 >M $4ed3 $82 >M
T{ op PC S A X Y P -> $4ed3 $0a $53 $94 $41 $60 }T T{ $4ed1 M $4ed2 M $4ed3 M -> $89 $52 $82 }T
$8da9 >PC $ae >S $88 >A $dc >X $f6 >Y $e5 >P $8da9 $89 >M $8daa $36 >M $8dab $f8 >M
T{ op PC S A X Y P -> $8dab $ae $88 $dc $f6 $e7 }T T{ $8da9 M $8daa M $8dab M -> $89 $36 $f8 }T
$4b22 >PC $cf >S $36 >A $97 >X $ff >Y $62 >P $4b22 $89 >M $4b23 $3d >M $4b24 $b6 >M
T{ op PC S A X Y P -> $4b24 $cf $36 $97 $ff $60 }T T{ $4b22 M $4b23 M $4b24 M -> $89 $3d $b6 }T
( 8a )
$c054 >PC $96 >S $7e >A $74 >X $81 >Y $62 >P $c054 $8a >M $c055 $4f >M $c056 $6d >M
T{ op PC S A X Y P -> $c055 $96 $74 $74 $81 $60 }T T{ $c054 M $c055 M $c056 M -> $8a $4f $6d }T
$5448 >PC $78 >S $c4 >A $0e >X $65 >Y $60 >P $5448 $8a >M $5449 $da >M $544a $73 >M
T{ op PC S A X Y P -> $5449 $78 $0e $0e $65 $60 }T T{ $5448 M $5449 M $544a M -> $8a $da $73 }T
$bc8e >PC $cc >S $48 >A $95 >X $2d >Y $27 >P $bc8e $8a >M $bc8f $99 >M $bc90 $7d >M
T{ op PC S A X Y P -> $bc8f $cc $95 $95 $2d $a5 }T T{ $bc8e M $bc8f M $bc90 M -> $8a $99 $7d }T
$8f93 >PC $2d >S $fa >A $2e >X $c2 >Y $a3 >P $8f93 $8a >M $8f94 $6d >M $8f95 $f4 >M
T{ op PC S A X Y P -> $8f94 $2d $2e $2e $c2 $21 }T T{ $8f93 M $8f94 M $8f95 M -> $8a $6d $f4 }T
$620b >PC $6b >S $34 >A $0c >X $e1 >Y $a1 >P $620b $8a >M $620c $87 >M $620d $e4 >M
T{ op PC S A X Y P -> $620c $6b $0c $0c $e1 $21 }T T{ $620b M $620c M $620d M -> $8a $87 $e4 }T
$7d26 >PC $42 >S $b7 >A $04 >X $84 >Y $67 >P $7d26 $8a >M $7d27 $7e >M $7d28 $c2 >M
T{ op PC S A X Y P -> $7d27 $42 $04 $04 $84 $65 }T T{ $7d26 M $7d27 M $7d28 M -> $8a $7e $c2 }T
$8e45 >PC $fb >S $ac >A $5a >X $c9 >Y $a2 >P $8e45 $8a >M $8e46 $cc >M $8e47 $36 >M
T{ op PC S A X Y P -> $8e46 $fb $5a $5a $c9 $20 }T T{ $8e45 M $8e46 M $8e47 M -> $8a $cc $36 }T
$55b8 >PC $e9 >S $5a >A $6d >X $14 >Y $63 >P $55b8 $8a >M $55b9 $d2 >M $55ba $9c >M
T{ op PC S A X Y P -> $55b9 $e9 $6d $6d $14 $61 }T T{ $55b8 M $55b9 M $55ba M -> $8a $d2 $9c }T
$baee >PC $24 >S $36 >A $a8 >X $86 >Y $63 >P $baee $8a >M $baef $f9 >M $baf0 $3c >M
T{ op PC S A X Y P -> $baef $24 $a8 $a8 $86 $e1 }T T{ $baee M $baef M $baf0 M -> $8a $f9 $3c }T
$5aa9 >PC $d7 >S $78 >A $4b >X $49 >Y $60 >P $5aa9 $8a >M $5aaa $c2 >M $5aab $e7 >M
T{ op PC S A X Y P -> $5aaa $d7 $4b $4b $49 $60 }T T{ $5aa9 M $5aaa M $5aab M -> $8a $c2 $e7 }T
$e428 >PC $c8 >S $43 >A $a8 >X $f2 >Y $e2 >P $e428 $8a >M $e429 $6c >M $e42a $19 >M
T{ op PC S A X Y P -> $e429 $c8 $a8 $a8 $f2 $e0 }T T{ $e428 M $e429 M $e42a M -> $8a $6c $19 }T
$ce06 >PC $ba >S $f2 >A $23 >X $56 >Y $e2 >P $ce06 $8a >M $ce07 $02 >M $ce08 $cb >M
T{ op PC S A X Y P -> $ce07 $ba $23 $23 $56 $60 }T T{ $ce06 M $ce07 M $ce08 M -> $8a $02 $cb }T
$8d9c >PC $03 >S $5b >A $a0 >X $2d >Y $24 >P $8d9c $8a >M $8d9d $82 >M $8d9e $c0 >M
T{ op PC S A X Y P -> $8d9d $03 $a0 $a0 $2d $a4 }T T{ $8d9c M $8d9d M $8d9e M -> $8a $82 $c0 }T
$da22 >PC $a9 >S $00 >A $c9 >X $50 >Y $a7 >P $da22 $8a >M $da23 $40 >M $da24 $c7 >M
T{ op PC S A X Y P -> $da23 $a9 $c9 $c9 $50 $a5 }T T{ $da22 M $da23 M $da24 M -> $8a $40 $c7 }T
$ea57 >PC $be >S $86 >A $61 >X $8b >Y $21 >P $ea57 $8a >M $ea58 $75 >M $ea59 $ee >M
T{ op PC S A X Y P -> $ea58 $be $61 $61 $8b $21 }T T{ $ea57 M $ea58 M $ea59 M -> $8a $75 $ee }T
$419f >PC $bf >S $55 >A $c8 >X $b7 >Y $a4 >P $419f $8a >M $41a0 $67 >M $41a1 $50 >M
T{ op PC S A X Y P -> $41a0 $bf $c8 $c8 $b7 $a4 }T T{ $419f M $41a0 M $41a1 M -> $8a $67 $50 }T
( 8b )
$4aff >PC $22 >S $a3 >A $d5 >X $8d >Y $a2 >P $4aff $8b >M $4b00 $c2 >M $4b01 $19 >M
T{ op PC S A X Y P -> $4b00 $22 $a3 $d5 $8d $a2 }T T{ $4aff M $4b00 M $4b01 M -> $8b $c2 $19 }T
$f95c >PC $69 >S $83 >A $f6 >X $16 >Y $a3 >P $f95c $8b >M $f95d $50 >M $f95e $82 >M
T{ op PC S A X Y P -> $f95d $69 $83 $f6 $16 $a3 }T T{ $f95c M $f95d M $f95e M -> $8b $50 $82 }T
$e7a7 >PC $3e >S $2a >A $d4 >X $3b >Y $27 >P $e7a7 $8b >M $e7a8 $56 >M $e7a9 $33 >M
T{ op PC S A X Y P -> $e7a8 $3e $2a $d4 $3b $27 }T T{ $e7a7 M $e7a8 M $e7a9 M -> $8b $56 $33 }T
$4a60 >PC $6e >S $5a >A $a6 >X $ff >Y $20 >P $4a60 $8b >M $4a61 $ca >M $4a62 $c2 >M
T{ op PC S A X Y P -> $4a61 $6e $5a $a6 $ff $20 }T T{ $4a60 M $4a61 M $4a62 M -> $8b $ca $c2 }T
$55d0 >PC $d0 >S $60 >A $70 >X $c7 >Y $63 >P $55d0 $8b >M $55d1 $93 >M $55d2 $ac >M
T{ op PC S A X Y P -> $55d1 $d0 $60 $70 $c7 $63 }T T{ $55d0 M $55d1 M $55d2 M -> $8b $93 $ac }T
$c864 >PC $54 >S $9c >A $38 >X $e9 >Y $63 >P $c864 $8b >M $c865 $9c >M $c866 $81 >M
T{ op PC S A X Y P -> $c865 $54 $9c $38 $e9 $63 }T T{ $c864 M $c865 M $c866 M -> $8b $9c $81 }T
$6f33 >PC $72 >S $7e >A $a5 >X $0e >Y $e2 >P $6f33 $8b >M $6f34 $f4 >M $6f35 $cd >M
T{ op PC S A X Y P -> $6f34 $72 $7e $a5 $0e $e2 }T T{ $6f33 M $6f34 M $6f35 M -> $8b $f4 $cd }T
$d139 >PC $f2 >S $6c >A $53 >X $be >Y $22 >P $d139 $8b >M $d13a $51 >M $d13b $eb >M
T{ op PC S A X Y P -> $d13a $f2 $6c $53 $be $22 }T T{ $d139 M $d13a M $d13b M -> $8b $51 $eb }T
$562f >PC $d3 >S $1e >A $0a >X $47 >Y $62 >P $562f $8b >M $5630 $f3 >M $5631 $56 >M
T{ op PC S A X Y P -> $5630 $d3 $1e $0a $47 $62 }T T{ $562f M $5630 M $5631 M -> $8b $f3 $56 }T
$5eaf >PC $09 >S $9e >A $08 >X $19 >Y $21 >P $5eaf $8b >M $5eb0 $8a >M $5eb1 $ac >M
T{ op PC S A X Y P -> $5eb0 $09 $9e $08 $19 $21 }T T{ $5eaf M $5eb0 M $5eb1 M -> $8b $8a $ac }T
$2dd4 >PC $3d >S $ae >A $d8 >X $17 >Y $a6 >P $2dd4 $8b >M $2dd5 $6b >M $2dd6 $de >M
T{ op PC S A X Y P -> $2dd5 $3d $ae $d8 $17 $a6 }T T{ $2dd4 M $2dd5 M $2dd6 M -> $8b $6b $de }T
$1602 >PC $17 >S $58 >A $de >X $2a >Y $20 >P $1602 $8b >M $1603 $42 >M $1604 $c6 >M
T{ op PC S A X Y P -> $1603 $17 $58 $de $2a $20 }T T{ $1602 M $1603 M $1604 M -> $8b $42 $c6 }T
$5192 >PC $d3 >S $8d >A $75 >X $95 >Y $64 >P $5192 $8b >M $5193 $03 >M $5194 $18 >M
T{ op PC S A X Y P -> $5193 $d3 $8d $75 $95 $64 }T T{ $5192 M $5193 M $5194 M -> $8b $03 $18 }T
$8d67 >PC $f4 >S $ca >A $f8 >X $0d >Y $61 >P $8d67 $8b >M $8d68 $59 >M $8d69 $d4 >M
T{ op PC S A X Y P -> $8d68 $f4 $ca $f8 $0d $61 }T T{ $8d67 M $8d68 M $8d69 M -> $8b $59 $d4 }T
$78f3 >PC $79 >S $2c >A $57 >X $80 >Y $e2 >P $78f3 $8b >M $78f4 $4b >M $78f5 $94 >M
T{ op PC S A X Y P -> $78f4 $79 $2c $57 $80 $e2 }T T{ $78f3 M $78f4 M $78f5 M -> $8b $4b $94 }T
$0a84 >PC $21 >S $9f >A $70 >X $91 >Y $a4 >P $0a84 $8b >M $0a85 $b2 >M $0a86 $16 >M
T{ op PC S A X Y P -> $0a85 $21 $9f $70 $91 $a4 }T T{ $0a84 M $0a85 M $0a86 M -> $8b $b2 $16 }T
( 8c )
$b33d >PC $34 >S $10 >A $a2 >X $bc >Y $a3 >P $b33d $8c >M $b33e $89 >M $b33f $92 >M $b340 $b1 >M
T{ op PC S A X Y P -> $b340 $34 $10 $a2 $bc $a3 }T T{ $9289 M $b33d M $b33e M $b33f M $b340 M -> $bc $8c $89 $92 $b1 }T
$71d3 >PC $9d >S $b8 >A $27 >X $9d >Y $60 >P $71d3 $8c >M $71d4 $23 >M $71d5 $ca >M $71d6 $8f >M
T{ op PC S A X Y P -> $71d6 $9d $b8 $27 $9d $60 }T T{ $71d3 M $71d4 M $71d5 M $71d6 M $ca23 M -> $8c $23 $ca $8f $9d }T
$cfed >PC $35 >S $67 >A $c6 >X $26 >Y $a1 >P $cfed $8c >M $cfee $b0 >M $cfef $59 >M $cff0 $84 >M
T{ op PC S A X Y P -> $cff0 $35 $67 $c6 $26 $a1 }T T{ $59b0 M $cfed M $cfee M $cfef M $cff0 M -> $26 $8c $b0 $59 $84 }T
$c734 >PC $ae >S $ea >A $5c >X $34 >Y $a3 >P $c734 $8c >M $c735 $f2 >M $c736 $be >M $c737 $e2 >M
T{ op PC S A X Y P -> $c737 $ae $ea $5c $34 $a3 }T T{ $bef2 M $c734 M $c735 M $c736 M $c737 M -> $34 $8c $f2 $be $e2 }T
$2e6a >PC $1d >S $25 >A $46 >X $21 >Y $e7 >P $2e6a $8c >M $2e6b $dd >M $2e6c $a0 >M $2e6d $8b >M
T{ op PC S A X Y P -> $2e6d $1d $25 $46 $21 $e7 }T T{ $2e6a M $2e6b M $2e6c M $2e6d M $a0dd M -> $8c $dd $a0 $8b $21 }T
$4eba >PC $96 >S $53 >A $b3 >X $e1 >Y $a6 >P $4eba $8c >M $4ebb $f1 >M $4ebc $58 >M $4ebd $90 >M
T{ op PC S A X Y P -> $4ebd $96 $53 $b3 $e1 $a6 }T T{ $4eba M $4ebb M $4ebc M $4ebd M $58f1 M -> $8c $f1 $58 $90 $e1 }T
$4b10 >PC $26 >S $cd >A $b3 >X $6c >Y $25 >P $4b10 $8c >M $4b11 $ef >M $4b12 $a0 >M $4b13 $9e >M
T{ op PC S A X Y P -> $4b13 $26 $cd $b3 $6c $25 }T T{ $4b10 M $4b11 M $4b12 M $4b13 M $a0ef M -> $8c $ef $a0 $9e $6c }T
$c38c >PC $92 >S $13 >A $77 >X $1e >Y $67 >P $c38c $8c >M $c38d $b8 >M $c38e $f2 >M $c38f $41 >M
T{ op PC S A X Y P -> $c38f $92 $13 $77 $1e $67 }T T{ $c38c M $c38d M $c38e M $c38f M $f2b8 M -> $8c $b8 $f2 $41 $1e }T
$2216 >PC $a9 >S $7e >A $15 >X $79 >Y $a6 >P $2216 $8c >M $2217 $36 >M $2218 $31 >M $2219 $79 >M
T{ op PC S A X Y P -> $2219 $a9 $7e $15 $79 $a6 }T T{ $2216 M $2217 M $2218 M $2219 M $3136 M -> $8c $36 $31 $79 $79 }T
$c1e8 >PC $f0 >S $6f >A $ee >X $33 >Y $23 >P $c1e8 $8c >M $c1e9 $93 >M $c1ea $d0 >M $c1eb $4a >M
T{ op PC S A X Y P -> $c1eb $f0 $6f $ee $33 $23 }T T{ $c1e8 M $c1e9 M $c1ea M $c1eb M $d093 M -> $8c $93 $d0 $4a $33 }T
$3a79 >PC $68 >S $22 >A $de >X $96 >Y $a3 >P $3a79 $8c >M $3a7a $75 >M $3a7b $af >M $3a7c $37 >M
T{ op PC S A X Y P -> $3a7c $68 $22 $de $96 $a3 }T T{ $3a79 M $3a7a M $3a7b M $3a7c M $af75 M -> $8c $75 $af $37 $96 }T
$55ca >PC $a8 >S $94 >A $c8 >X $73 >Y $27 >P $55ca $8c >M $55cb $dd >M $55cc $9c >M $55cd $ea >M
T{ op PC S A X Y P -> $55cd $a8 $94 $c8 $73 $27 }T T{ $55ca M $55cb M $55cc M $55cd M $9cdd M -> $8c $dd $9c $ea $73 }T
$3daa >PC $74 >S $0c >A $5a >X $6a >Y $e5 >P $3daa $8c >M $3dab $a7 >M $3dac $e9 >M $3dad $32 >M
T{ op PC S A X Y P -> $3dad $74 $0c $5a $6a $e5 }T T{ $3daa M $3dab M $3dac M $3dad M $e9a7 M -> $8c $a7 $e9 $32 $6a }T
$65a7 >PC $7b >S $c5 >A $49 >X $0c >Y $e7 >P $65a7 $8c >M $65a8 $75 >M $65a9 $36 >M $65aa $d7 >M
T{ op PC S A X Y P -> $65aa $7b $c5 $49 $0c $e7 }T T{ $3675 M $65a7 M $65a8 M $65a9 M $65aa M -> $0c $8c $75 $36 $d7 }T
$d6cb >PC $f3 >S $30 >A $d5 >X $da >Y $27 >P $d6cb $8c >M $d6cc $fa >M $d6cd $c3 >M $d6ce $5d >M
T{ op PC S A X Y P -> $d6ce $f3 $30 $d5 $da $27 }T T{ $c3fa M $d6cb M $d6cc M $d6cd M $d6ce M -> $da $8c $fa $c3 $5d }T
$6fc8 >PC $b9 >S $0d >A $9a >X $ae >Y $e1 >P $6fc8 $8c >M $6fc9 $56 >M $6fca $35 >M $6fcb $5c >M
T{ op PC S A X Y P -> $6fcb $b9 $0d $9a $ae $e1 }T T{ $3556 M $6fc8 M $6fc9 M $6fca M $6fcb M -> $ae $8c $56 $35 $5c }T
( 8d )
$7e4d >PC $ff >S $b2 >A $61 >X $de >Y $e1 >P $7e4d $8d >M $7e4e $56 >M $7e4f $e2 >M $7e50 $fa >M
T{ op PC S A X Y P -> $7e50 $ff $b2 $61 $de $e1 }T T{ $7e4d M $7e4e M $7e4f M $7e50 M $e256 M -> $8d $56 $e2 $fa $b2 }T
$60b2 >PC $98 >S $3c >A $34 >X $c0 >Y $67 >P $60b2 $8d >M $60b3 $05 >M $60b4 $1b >M $60b5 $fb >M
T{ op PC S A X Y P -> $60b5 $98 $3c $34 $c0 $67 }T T{ $1b05 M $60b2 M $60b3 M $60b4 M $60b5 M -> $3c $8d $05 $1b $fb }T
$72bd >PC $00 >S $00 >A $7a >X $1c >Y $67 >P $72bd $8d >M $72be $de >M $72bf $83 >M $72c0 $bd >M
T{ op PC S A X Y P -> $72c0 $00 $00 $7a $1c $67 }T T{ $72bd M $72be M $72bf M $72c0 M $83de M -> $8d $de $83 $bd $00 }T
$6819 >PC $87 >S $8d >A $de >X $da >Y $a5 >P $6819 $8d >M $681a $1e >M $681b $50 >M $681c $a1 >M
T{ op PC S A X Y P -> $681c $87 $8d $de $da $a5 }T T{ $501e M $6819 M $681a M $681b M $681c M -> $8d $8d $1e $50 $a1 }T
$2bda >PC $d3 >S $a5 >A $8b >X $d9 >Y $21 >P $2bda $8d >M $2bdb $69 >M $2bdc $01 >M $2bdd $e2 >M
T{ op PC S A X Y P -> $2bdd $d3 $a5 $8b $d9 $21 }T T{ $0169 M $2bda M $2bdb M $2bdc M $2bdd M -> $a5 $8d $69 $01 $e2 }T
$6cfd >PC $fe >S $78 >A $7b >X $6a >Y $e5 >P $6cfd $8d >M $6cfe $2f >M $6cff $67 >M $6d00 $ad >M
T{ op PC S A X Y P -> $6d00 $fe $78 $7b $6a $e5 }T T{ $672f M $6cfd M $6cfe M $6cff M $6d00 M -> $78 $8d $2f $67 $ad }T
$56c5 >PC $a8 >S $01 >A $84 >X $c3 >Y $e6 >P $56c5 $8d >M $56c6 $21 >M $56c7 $20 >M $56c8 $77 >M
T{ op PC S A X Y P -> $56c8 $a8 $01 $84 $c3 $e6 }T T{ $2021 M $56c5 M $56c6 M $56c7 M $56c8 M -> $01 $8d $21 $20 $77 }T
$c50d >PC $8a >S $2c >A $bd >X $58 >Y $25 >P $c50d $8d >M $c50e $b5 >M $c50f $8c >M $c510 $f7 >M
T{ op PC S A X Y P -> $c510 $8a $2c $bd $58 $25 }T T{ $8cb5 M $c50d M $c50e M $c50f M $c510 M -> $2c $8d $b5 $8c $f7 }T
$44ea >PC $29 >S $b7 >A $df >X $42 >Y $a3 >P $44ea $8d >M $44eb $94 >M $44ec $6f >M $44ed $3e >M
T{ op PC S A X Y P -> $44ed $29 $b7 $df $42 $a3 }T T{ $44ea M $44eb M $44ec M $44ed M $6f94 M -> $8d $94 $6f $3e $b7 }T
$9e0a >PC $cb >S $0a >A $72 >X $02 >Y $a7 >P $9e0a $8d >M $9e0b $a1 >M $9e0c $1f >M $9e0d $b0 >M
T{ op PC S A X Y P -> $9e0d $cb $0a $72 $02 $a7 }T T{ $1fa1 M $9e0a M $9e0b M $9e0c M $9e0d M -> $0a $8d $a1 $1f $b0 }T
$02c0 >PC $b3 >S $72 >A $0e >X $23 >Y $23 >P $02c0 $8d >M $02c1 $c1 >M $02c2 $4f >M $02c3 $0f >M
T{ op PC S A X Y P -> $02c3 $b3 $72 $0e $23 $23 }T T{ $02c0 M $02c1 M $02c2 M $02c3 M $4fc1 M -> $8d $c1 $4f $0f $72 }T
$734d >PC $4c >S $48 >A $1c >X $26 >Y $23 >P $734d $8d >M $734e $84 >M $734f $07 >M $7350 $67 >M
T{ op PC S A X Y P -> $7350 $4c $48 $1c $26 $23 }T T{ $0784 M $734d M $734e M $734f M $7350 M -> $48 $8d $84 $07 $67 }T
$5b4d >PC $90 >S $45 >A $84 >X $07 >Y $a6 >P $5b4d $8d >M $5b4e $e9 >M $5b4f $cd >M $5b50 $4c >M
T{ op PC S A X Y P -> $5b50 $90 $45 $84 $07 $a6 }T T{ $5b4d M $5b4e M $5b4f M $5b50 M $cde9 M -> $8d $e9 $cd $4c $45 }T
$52c8 >PC $cc >S $d4 >A $c8 >X $68 >Y $e3 >P $52c8 $8d >M $52c9 $51 >M $52ca $46 >M $52cb $fc >M
T{ op PC S A X Y P -> $52cb $cc $d4 $c8 $68 $e3 }T T{ $4651 M $52c8 M $52c9 M $52ca M $52cb M -> $d4 $8d $51 $46 $fc }T
$0843 >PC $6d >S $0c >A $fb >X $86 >Y $a2 >P $0843 $8d >M $0844 $b2 >M $0845 $52 >M $0846 $3f >M
T{ op PC S A X Y P -> $0846 $6d $0c $fb $86 $a2 }T T{ $0843 M $0844 M $0845 M $0846 M $52b2 M -> $8d $b2 $52 $3f $0c }T
$4d2e >PC $95 >S $e8 >A $bc >X $a8 >Y $a7 >P $4d2e $8d >M $4d2f $bd >M $4d30 $40 >M $4d31 $bc >M
T{ op PC S A X Y P -> $4d31 $95 $e8 $bc $a8 $a7 }T T{ $40bd M $4d2e M $4d2f M $4d30 M $4d31 M -> $e8 $8d $bd $40 $bc }T
( 8e )
$2252 >PC $6d >S $08 >A $45 >X $94 >Y $e2 >P $2252 $8e >M $2253 $c1 >M $2254 $53 >M $2255 $9d >M
T{ op PC S A X Y P -> $2255 $6d $08 $45 $94 $e2 }T T{ $2252 M $2253 M $2254 M $2255 M $53c1 M -> $8e $c1 $53 $9d $45 }T
$8c7b >PC $94 >S $50 >A $35 >X $30 >Y $65 >P $8c7b $8e >M $8c7c $66 >M $8c7d $6e >M $8c7e $fb >M
T{ op PC S A X Y P -> $8c7e $94 $50 $35 $30 $65 }T T{ $6e66 M $8c7b M $8c7c M $8c7d M $8c7e M -> $35 $8e $66 $6e $fb }T
$efe1 >PC $46 >S $6c >A $36 >X $93 >Y $64 >P $efe1 $8e >M $efe2 $61 >M $efe3 $e7 >M $efe4 $2e >M
T{ op PC S A X Y P -> $efe4 $46 $6c $36 $93 $64 }T T{ $e761 M $efe1 M $efe2 M $efe3 M $efe4 M -> $36 $8e $61 $e7 $2e }T
$2575 >PC $7e >S $62 >A $32 >X $49 >Y $a7 >P $2575 $8e >M $2576 $a5 >M $2577 $47 >M $2578 $9b >M
T{ op PC S A X Y P -> $2578 $7e $62 $32 $49 $a7 }T T{ $2575 M $2576 M $2577 M $2578 M $47a5 M -> $8e $a5 $47 $9b $32 }T
$b46d >PC $ac >S $b4 >A $4e >X $df >Y $e0 >P $b46d $8e >M $b46e $28 >M $b46f $dc >M $b470 $06 >M
T{ op PC S A X Y P -> $b470 $ac $b4 $4e $df $e0 }T T{ $b46d M $b46e M $b46f M $b470 M $dc28 M -> $8e $28 $dc $06 $4e }T
$1646 >PC $41 >S $71 >A $22 >X $22 >Y $e1 >P $1646 $8e >M $1647 $cf >M $1648 $d2 >M $1649 $8d >M
T{ op PC S A X Y P -> $1649 $41 $71 $22 $22 $e1 }T T{ $1646 M $1647 M $1648 M $1649 M $d2cf M -> $8e $cf $d2 $8d $22 }T
$a3d2 >PC $b4 >S $64 >A $b6 >X $e5 >Y $23 >P $a3d2 $8e >M $a3d3 $ce >M $a3d4 $14 >M $a3d5 $5a >M
T{ op PC S A X Y P -> $a3d5 $b4 $64 $b6 $e5 $23 }T T{ $14ce M $a3d2 M $a3d3 M $a3d4 M $a3d5 M -> $b6 $8e $ce $14 $5a }T
$525a >PC $56 >S $0a >A $52 >X $10 >Y $65 >P $525a $8e >M $525b $12 >M $525c $f0 >M $525d $4b >M
T{ op PC S A X Y P -> $525d $56 $0a $52 $10 $65 }T T{ $525a M $525b M $525c M $525d M $f012 M -> $8e $12 $f0 $4b $52 }T
$1c36 >PC $d4 >S $93 >A $e2 >X $be >Y $e2 >P $1c36 $8e >M $1c37 $77 >M $1c38 $8d >M $1c39 $b5 >M
T{ op PC S A X Y P -> $1c39 $d4 $93 $e2 $be $e2 }T T{ $1c36 M $1c37 M $1c38 M $1c39 M $8d77 M -> $8e $77 $8d $b5 $e2 }T
$108c >PC $33 >S $86 >A $3b >X $15 >Y $a7 >P $108c $8e >M $108d $cd >M $108e $28 >M $108f $79 >M
T{ op PC S A X Y P -> $108f $33 $86 $3b $15 $a7 }T T{ $108c M $108d M $108e M $108f M $28cd M -> $8e $cd $28 $79 $3b }T
$f1d1 >PC $26 >S $38 >A $9d >X $36 >Y $e2 >P $f1d1 $8e >M $f1d2 $89 >M $f1d3 $90 >M $f1d4 $da >M
T{ op PC S A X Y P -> $f1d4 $26 $38 $9d $36 $e2 }T T{ $9089 M $f1d1 M $f1d2 M $f1d3 M $f1d4 M -> $9d $8e $89 $90 $da }T
$7453 >PC $d1 >S $50 >A $21 >X $a9 >Y $e1 >P $7453 $8e >M $7454 $fe >M $7455 $09 >M $7456 $b8 >M
T{ op PC S A X Y P -> $7456 $d1 $50 $21 $a9 $e1 }T T{ $09fe M $7453 M $7454 M $7455 M $7456 M -> $21 $8e $fe $09 $b8 }T
$6c85 >PC $45 >S $db >A $a8 >X $42 >Y $e5 >P $6c85 $8e >M $6c86 $ff >M $6c87 $74 >M $6c88 $ad >M
T{ op PC S A X Y P -> $6c88 $45 $db $a8 $42 $e5 }T T{ $6c85 M $6c86 M $6c87 M $6c88 M $74ff M -> $8e $ff $74 $ad $a8 }T
$2c78 >PC $6c >S $b8 >A $1f >X $95 >Y $61 >P $2c78 $8e >M $2c79 $51 >M $2c7a $ba >M $2c7b $c6 >M
T{ op PC S A X Y P -> $2c7b $6c $b8 $1f $95 $61 }T T{ $2c78 M $2c79 M $2c7a M $2c7b M $ba51 M -> $8e $51 $ba $c6 $1f }T
$cfb7 >PC $21 >S $68 >A $fd >X $c3 >Y $24 >P $cfb7 $8e >M $cfb8 $c5 >M $cfb9 $3f >M $cfba $de >M
T{ op PC S A X Y P -> $cfba $21 $68 $fd $c3 $24 }T T{ $3fc5 M $cfb7 M $cfb8 M $cfb9 M $cfba M -> $fd $8e $c5 $3f $de }T
$cb5c >PC $62 >S $f6 >A $43 >X $48 >Y $62 >P $cb5c $8e >M $cb5d $bd >M $cb5e $ec >M $cb5f $0b >M
T{ op PC S A X Y P -> $cb5f $62 $f6 $43 $48 $62 }T T{ $cb5c M $cb5d M $cb5e M $cb5f M $ecbd M -> $8e $bd $ec $0b $43 }T
( 8f )
$dea8 >PC $9b >S $2d >A $e4 >X $46 >Y $e0 >P $0034 $44 >M $de72 $5e >M $dea8 $8f >M $dea9 $34 >M $deaa $c7 >M $deab $7b >M
T{ op PC S A X Y P -> $deab $9b $2d $e4 $46 $e0 }T T{ $0034 M $de72 M $dea8 M $dea9 M $deaa M $deab M -> $44 $5e $8f $34 $c7 $7b }T
$0270 >PC $66 >S $25 >A $0e >X $dd >Y $21 >P $00dd $59 >M $020a $98 >M $0270 $8f >M $0271 $dd >M $0272 $97 >M
T{ op PC S A X Y P -> $020a $66 $25 $0e $dd $21 }T T{ $00dd M $020a M $0270 M $0271 M $0272 M -> $59 $98 $8f $dd $97 }T
$dc47 >PC $e4 >S $c9 >A $82 >X $5e >Y $e2 >P $0095 $bd >M $dc3e $07 >M $dc47 $8f >M $dc48 $95 >M $dc49 $f4 >M
T{ op PC S A X Y P -> $dc3e $e4 $c9 $82 $5e $e2 }T T{ $0095 M $dc3e M $dc47 M $dc48 M $dc49 M -> $bd $07 $8f $95 $f4 }T
$d69c >PC $81 >S $60 >A $a3 >X $83 >Y $65 >P $000a $36 >M $d689 $96 >M $d69c $8f >M $d69d $0a >M $d69e $ea >M $d69f $2a >M
T{ op PC S A X Y P -> $d69f $81 $60 $a3 $83 $65 }T T{ $000a M $d689 M $d69c M $d69d M $d69e M $d69f M -> $36 $96 $8f $0a $ea $2a }T
$7dd0 >PC $2d >S $c3 >A $8f >X $80 >Y $21 >P $001b $0e >M $7dd0 $8f >M $7dd1 $1b >M $7dd2 $2a >M $7dd3 $42 >M $7dfd $3e >M
T{ op PC S A X Y P -> $7dd3 $2d $c3 $8f $80 $21 }T T{ $001b M $7dd0 M $7dd1 M $7dd2 M $7dd3 M $7dfd M -> $0e $8f $1b $2a $42 $3e }T
$a784 >PC $a8 >S $dc >A $13 >X $fe >Y $21 >P $0074 $43 >M $a729 $a2 >M $a784 $8f >M $a785 $74 >M $a786 $a2 >M
T{ op PC S A X Y P -> $a729 $a8 $dc $13 $fe $21 }T T{ $0074 M $a729 M $a784 M $a785 M $a786 M -> $43 $a2 $8f $74 $a2 }T
$3538 >PC $2c >S $30 >A $9b >X $b2 >Y $62 >P $00db $a8 >M $3538 $8f >M $3539 $db >M $353a $92 >M $353b $78 >M $35cd $ad >M
T{ op PC S A X Y P -> $353b $2c $30 $9b $b2 $62 }T T{ $00db M $3538 M $3539 M $353a M $353b M $35cd M -> $a8 $8f $db $92 $78 $ad }T
$9d27 >PC $a9 >S $ee >A $5a >X $38 >Y $e5 >P $006a $c5 >M $9d1e $8a >M $9d27 $8f >M $9d28 $6a >M $9d29 $f4 >M
T{ op PC S A X Y P -> $9d1e $a9 $ee $5a $38 $e5 }T T{ $006a M $9d1e M $9d27 M $9d28 M $9d29 M -> $c5 $8a $8f $6a $f4 }T
$7c33 >PC $a3 >S $49 >A $c7 >X $d1 >Y $a6 >P $00cc $ac >M $7c33 $8f >M $7c34 $cc >M $7c35 $c7 >M $7c36 $45 >M $7cfd $3e >M
T{ op PC S A X Y P -> $7c36 $a3 $49 $c7 $d1 $a6 }T T{ $00cc M $7c33 M $7c34 M $7c35 M $7c36 M $7cfd M -> $ac $8f $cc $c7 $45 $3e }T
$1ba1 >PC $64 >S $8f >A $97 >X $45 >Y $60 >P $0056 $9a >M $1b8b $ba >M $1ba1 $8f >M $1ba2 $56 >M $1ba3 $e7 >M $1ba4 $30 >M
T{ op PC S A X Y P -> $1ba4 $64 $8f $97 $45 $60 }T T{ $0056 M $1b8b M $1ba1 M $1ba2 M $1ba3 M $1ba4 M -> $9a $ba $8f $56 $e7 $30 }T
$f347 >PC $c0 >S $5a >A $2c >X $6e >Y $25 >P $009b $d5 >M $f347 $8f >M $f348 $9b >M $f349 $18 >M $f362 $ec >M
T{ op PC S A X Y P -> $f362 $c0 $5a $2c $6e $25 }T T{ $009b M $f347 M $f348 M $f349 M $f362 M -> $d5 $8f $9b $18 $ec }T
$fa26 >PC $7e >S $33 >A $62 >X $c7 >Y $24 >P $00fa $c6 >M $fa26 $8f >M $fa27 $fa >M $fa28 $4a >M $fa29 $f7 >M $fa73 $f4 >M
T{ op PC S A X Y P -> $fa29 $7e $33 $62 $c7 $24 }T T{ $00fa M $fa26 M $fa27 M $fa28 M $fa29 M $fa73 M -> $c6 $8f $fa $4a $f7 $f4 }T
$3398 >PC $84 >S $d9 >A $61 >X $02 >Y $21 >P $0004 $5b >M $3353 $5e >M $3398 $8f >M $3399 $04 >M $339a $b8 >M
T{ op PC S A X Y P -> $3353 $84 $d9 $61 $02 $21 }T T{ $0004 M $3353 M $3398 M $3399 M $339a M -> $5b $5e $8f $04 $b8 }T
$941d >PC $2a >S $57 >A $bc >X $f7 >Y $e6 >P $0086 $fe >M $941d $8f >M $941e $86 >M $941f $c2 >M $9420 $2b >M $94e2 $55 >M
T{ op PC S A X Y P -> $9420 $2a $57 $bc $f7 $e6 }T T{ $0086 M $941d M $941e M $941f M $9420 M $94e2 M -> $fe $8f $86 $c2 $2b $55 }T
$1f40 >PC $b6 >S $1c >A $e3 >X $ef >Y $a7 >P $0064 $03 >M $1f40 $8f >M $1f41 $64 >M $1f42 $49 >M $1f8c $d8 >M
T{ op PC S A X Y P -> $1f8c $b6 $1c $e3 $ef $a7 }T T{ $0064 M $1f40 M $1f41 M $1f42 M $1f8c M -> $03 $8f $64 $49 $d8 }T
$cbb4 >PC $cf >S $f5 >A $23 >X $74 >Y $e5 >P $00eb $9c >M $cb5f $10 >M $cbb4 $8f >M $cbb5 $eb >M $cbb6 $a8 >M $cbb7 $80 >M
T{ op PC S A X Y P -> $cbb7 $cf $f5 $23 $74 $e5 }T T{ $00eb M $cb5f M $cbb4 M $cbb5 M $cbb6 M $cbb7 M -> $9c $10 $8f $eb $a8 $80 }T
( 90 )
$af18 >PC $35 >S $f7 >A $5c >X $d8 >Y $24 >P $aea0 $9d >M $af18 $90 >M $af19 $86 >M $af1a $f2 >M $afa0 $ee >M
T{ op PC S A X Y P -> $aea0 $35 $f7 $5c $d8 $24 }T T{ $aea0 M $af18 M $af19 M $af1a M $afa0 M -> $9d $90 $86 $f2 $ee }T
$203c >PC $93 >S $bd >A $f5 >X $d5 >Y $21 >P $203c $90 >M $203d $b4 >M $203e $2f >M
T{ op PC S A X Y P -> $203e $93 $bd $f5 $d5 $21 }T T{ $203c M $203d M $203e M -> $90 $b4 $2f }T
$c6b6 >PC $c0 >S $48 >A $6f >X $dc >Y $a4 >P $c659 $ec >M $c6b6 $90 >M $c6b7 $a1 >M $c6b8 $f1 >M
T{ op PC S A X Y P -> $c659 $c0 $48 $6f $dc $a4 }T T{ $c659 M $c6b6 M $c6b7 M $c6b8 M -> $ec $90 $a1 $f1 }T
$603d >PC $b3 >S $ec >A $8e >X $f2 >Y $63 >P $603d $90 >M $603e $c7 >M $603f $12 >M
T{ op PC S A X Y P -> $603f $b3 $ec $8e $f2 $63 }T T{ $603d M $603e M $603f M -> $90 $c7 $12 }T
$1791 >PC $82 >S $0d >A $64 >X $07 >Y $61 >P $1791 $90 >M $1792 $f3 >M $1793 $0e >M
T{ op PC S A X Y P -> $1793 $82 $0d $64 $07 $61 }T T{ $1791 M $1792 M $1793 M -> $90 $f3 $0e }T
$1974 >PC $1e >S $92 >A $72 >X $16 >Y $63 >P $1974 $90 >M $1975 $41 >M $1976 $cc >M
T{ op PC S A X Y P -> $1976 $1e $92 $72 $16 $63 }T T{ $1974 M $1975 M $1976 M -> $90 $41 $cc }T
$d9be >PC $91 >S $fd >A $d2 >X $5a >Y $a1 >P $d9be $90 >M $d9bf $14 >M $d9c0 $30 >M
T{ op PC S A X Y P -> $d9c0 $91 $fd $d2 $5a $a1 }T T{ $d9be M $d9bf M $d9c0 M -> $90 $14 $30 }T
$790a >PC $38 >S $57 >A $a6 >X $36 >Y $22 >P $790a $90 >M $790b $55 >M $790c $db >M $7961 $61 >M
T{ op PC S A X Y P -> $7961 $38 $57 $a6 $36 $22 }T T{ $790a M $790b M $790c M $7961 M -> $90 $55 $db $61 }T
$55cc >PC $3b >S $be >A $9a >X $ec >Y $67 >P $55cc $90 >M $55cd $b6 >M $55ce $11 >M
T{ op PC S A X Y P -> $55ce $3b $be $9a $ec $67 }T T{ $55cc M $55cd M $55ce M -> $90 $b6 $11 }T
$5afb >PC $cb >S $71 >A $f5 >X $4e >Y $22 >P $5a0a $51 >M $5afb $90 >M $5afc $0d >M $5afd $6d >M $5b0a $47 >M
T{ op PC S A X Y P -> $5b0a $cb $71 $f5 $4e $22 }T T{ $5a0a M $5afb M $5afc M $5afd M $5b0a M -> $51 $90 $0d $6d $47 }T
$139e >PC $7d >S $de >A $a7 >X $47 >Y $66 >P $139e $90 >M $139f $3e >M $13a0 $1a >M $13de $0f >M
T{ op PC S A X Y P -> $13de $7d $de $a7 $47 $66 }T T{ $139e M $139f M $13a0 M $13de M -> $90 $3e $1a $0f }T
$3925 >PC $b2 >S $25 >A $5d >X $99 >Y $65 >P $3925 $90 >M $3926 $cd >M $3927 $5a >M
T{ op PC S A X Y P -> $3927 $b2 $25 $5d $99 $65 }T T{ $3925 M $3926 M $3927 M -> $90 $cd $5a }T
$6664 >PC $3e >S $3a >A $e5 >X $d2 >Y $e6 >P $6664 $90 >M $6665 $17 >M $6666 $30 >M $667d $88 >M
T{ op PC S A X Y P -> $667d $3e $3a $e5 $d2 $e6 }T T{ $6664 M $6665 M $6666 M $667d M -> $90 $17 $30 $88 }T
$1f2e >PC $6f >S $6f >A $97 >X $64 >Y $64 >P $1f2e $90 >M $1f2f $78 >M $1f30 $5a >M $1fa8 $23 >M
T{ op PC S A X Y P -> $1fa8 $6f $6f $97 $64 $64 }T T{ $1f2e M $1f2f M $1f30 M $1fa8 M -> $90 $78 $5a $23 }T
$edba >PC $ef >S $5f >A $21 >X $41 >Y $24 >P $ed6b $4d >M $edba $90 >M $edbb $af >M $edbc $f7 >M
T{ op PC S A X Y P -> $ed6b $ef $5f $21 $41 $24 }T T{ $ed6b M $edba M $edbb M $edbc M -> $4d $90 $af $f7 }T
$8b25 >PC $bd >S $65 >A $85 >X $36 >Y $a4 >P $8aea $77 >M $8b25 $90 >M $8b26 $c3 >M $8b27 $14 >M $8bea $27 >M
T{ op PC S A X Y P -> $8aea $bd $65 $85 $36 $a4 }T T{ $8aea M $8b25 M $8b26 M $8b27 M $8bea M -> $77 $90 $c3 $14 $27 }T
( 91 )
$117f >PC $b2 >S $27 >A $ce >X $2b >Y $22 >P $004a $c0 >M $004b $68 >M $117f $91 >M $1180 $4a >M $1181 $47 >M
T{ op PC S A X Y P -> $1181 $b2 $27 $ce $2b $22 }T T{ $004a M $004b M $117f M $1180 M $1181 M $68eb M -> $c0 $68 $91 $4a $47 $27 }T
$cf6b >PC $e2 >S $2e >A $c7 >X $8f >Y $60 >P $008a $d6 >M $008b $c2 >M $cf6b $91 >M $cf6c $8a >M $cf6d $85 >M
T{ op PC S A X Y P -> $cf6d $e2 $2e $c7 $8f $60 }T T{ $008a M $008b M $c365 M $cf6b M $cf6c M $cf6d M -> $d6 $c2 $2e $91 $8a $85 }T
$ed5d >PC $56 >S $c6 >A $79 >X $fd >Y $26 >P $0087 $5d >M $0088 $79 >M $ed5d $91 >M $ed5e $87 >M $ed5f $c2 >M
T{ op PC S A X Y P -> $ed5f $56 $c6 $79 $fd $26 }T T{ $0087 M $0088 M $7a5a M $ed5d M $ed5e M $ed5f M -> $5d $79 $c6 $91 $87 $c2 }T
$9394 >PC $f7 >S $3a >A $0a >X $5d >Y $20 >P $0025 $cc >M $0026 $78 >M $9394 $91 >M $9395 $25 >M $9396 $2a >M
T{ op PC S A X Y P -> $9396 $f7 $3a $0a $5d $20 }T T{ $0025 M $0026 M $7929 M $9394 M $9395 M $9396 M -> $cc $78 $3a $91 $25 $2a }T
$5a45 >PC $c7 >S $33 >A $18 >X $81 >Y $23 >P $00f0 $d6 >M $00f1 $2b >M $5a45 $91 >M $5a46 $f0 >M $5a47 $93 >M
T{ op PC S A X Y P -> $5a47 $c7 $33 $18 $81 $23 }T T{ $00f0 M $00f1 M $2c57 M $5a45 M $5a46 M $5a47 M -> $d6 $2b $33 $91 $f0 $93 }T
$c2c7 >PC $6f >S $fd >A $53 >X $72 >Y $64 >P $0037 $b2 >M $0038 $aa >M $c2c7 $91 >M $c2c8 $37 >M $c2c9 $9a >M
T{ op PC S A X Y P -> $c2c9 $6f $fd $53 $72 $64 }T T{ $0037 M $0038 M $ab24 M $c2c7 M $c2c8 M $c2c9 M -> $b2 $aa $fd $91 $37 $9a }T
$9831 >PC $00 >S $98 >A $2f >X $94 >Y $a3 >P $005a $1f >M $005b $b2 >M $9831 $91 >M $9832 $5a >M $9833 $97 >M
T{ op PC S A X Y P -> $9833 $00 $98 $2f $94 $a3 }T T{ $005a M $005b M $9831 M $9832 M $9833 M $b2b3 M -> $1f $b2 $91 $5a $97 $98 }T
$2dc3 >PC $5a >S $e6 >A $76 >X $47 >Y $e0 >P $0061 $a8 >M $0062 $cc >M $2dc3 $91 >M $2dc4 $61 >M $2dc5 $f1 >M
T{ op PC S A X Y P -> $2dc5 $5a $e6 $76 $47 $e0 }T T{ $0061 M $0062 M $2dc3 M $2dc4 M $2dc5 M $ccef M -> $a8 $cc $91 $61 $f1 $e6 }T
$3986 >PC $d3 >S $90 >A $d6 >X $eb >Y $e5 >P $00fc $d1 >M $00fd $15 >M $3986 $91 >M $3987 $fc >M $3988 $bb >M
T{ op PC S A X Y P -> $3988 $d3 $90 $d6 $eb $e5 }T T{ $00fc M $00fd M $16bc M $3986 M $3987 M $3988 M -> $d1 $15 $90 $91 $fc $bb }T
$a3f1 >PC $a6 >S $85 >A $42 >X $8a >Y $a2 >P $0017 $69 >M $0018 $29 >M $a3f1 $91 >M $a3f2 $17 >M $a3f3 $ba >M
T{ op PC S A X Y P -> $a3f3 $a6 $85 $42 $8a $a2 }T T{ $0017 M $0018 M $29f3 M $a3f1 M $a3f2 M $a3f3 M -> $69 $29 $85 $91 $17 $ba }T
$9641 >PC $fe >S $90 >A $11 >X $bc >Y $e2 >P $005f $e4 >M $0060 $57 >M $9641 $91 >M $9642 $5f >M $9643 $f3 >M
T{ op PC S A X Y P -> $9643 $fe $90 $11 $bc $e2 }T T{ $005f M $0060 M $58a0 M $9641 M $9642 M $9643 M -> $e4 $57 $90 $91 $5f $f3 }T
$bd6a >PC $6e >S $38 >A $a4 >X $d8 >Y $e0 >P $00bf $f8 >M $00c0 $8b >M $bd6a $91 >M $bd6b $bf >M $bd6c $ed >M
T{ op PC S A X Y P -> $bd6c $6e $38 $a4 $d8 $e0 }T T{ $00bf M $00c0 M $8cd0 M $bd6a M $bd6b M $bd6c M -> $f8 $8b $38 $91 $bf $ed }T
$677a >PC $c3 >S $b4 >A $3e >X $b0 >Y $63 >P $00e6 $a6 >M $00e7 $82 >M $677a $91 >M $677b $e6 >M $677c $a3 >M
T{ op PC S A X Y P -> $677c $c3 $b4 $3e $b0 $63 }T T{ $00e6 M $00e7 M $677a M $677b M $677c M $8356 M -> $a6 $82 $91 $e6 $a3 $b4 }T
$4116 >PC $7f >S $0f >A $78 >X $78 >Y $e2 >P $00d9 $a9 >M $00da $75 >M $4116 $91 >M $4117 $d9 >M $4118 $18 >M
T{ op PC S A X Y P -> $4118 $7f $0f $78 $78 $e2 }T T{ $00d9 M $00da M $4116 M $4117 M $4118 M $7621 M -> $a9 $75 $91 $d9 $18 $0f }T
$2c15 >PC $a2 >S $56 >A $b9 >X $a6 >Y $67 >P $00db $06 >M $00dc $e1 >M $2c15 $91 >M $2c16 $db >M $2c17 $14 >M
T{ op PC S A X Y P -> $2c17 $a2 $56 $b9 $a6 $67 }T T{ $00db M $00dc M $2c15 M $2c16 M $2c17 M $e1ac M -> $06 $e1 $91 $db $14 $56 }T
$8cf7 >PC $51 >S $2a >A $25 >X $d3 >Y $64 >P $005a $ad >M $005b $fa >M $8cf7 $91 >M $8cf8 $5a >M $8cf9 $6d >M
T{ op PC S A X Y P -> $8cf9 $51 $2a $25 $d3 $64 }T T{ $005a M $005b M $8cf7 M $8cf8 M $8cf9 M $fb80 M -> $ad $fa $91 $5a $6d $2a }T
( 92 )
$b6ae >PC $f9 >S $bc >A $d3 >X $5a >Y $a1 >P $0025 $b4 >M $0026 $47 >M $b6ae $92 >M $b6af $25 >M $b6b0 $0d >M
T{ op PC S A X Y P -> $b6b0 $f9 $bc $d3 $5a $a1 }T T{ $0025 M $0026 M $47b4 M $b6ae M $b6af M $b6b0 M -> $b4 $47 $bc $92 $25 $0d }T
$0bb0 >PC $c9 >S $68 >A $aa >X $3a >Y $e0 >P $0057 $f9 >M $0058 $2c >M $0bb0 $92 >M $0bb1 $57 >M $0bb2 $36 >M
T{ op PC S A X Y P -> $0bb2 $c9 $68 $aa $3a $e0 }T T{ $0057 M $0058 M $0bb0 M $0bb1 M $0bb2 M $2cf9 M -> $f9 $2c $92 $57 $36 $68 }T
$d2a1 >PC $e1 >S $fe >A $5f >X $72 >Y $60 >P $0071 $d7 >M $0072 $7c >M $d2a1 $92 >M $d2a2 $71 >M $d2a3 $72 >M
T{ op PC S A X Y P -> $d2a3 $e1 $fe $5f $72 $60 }T T{ $0071 M $0072 M $7cd7 M $d2a1 M $d2a2 M $d2a3 M -> $d7 $7c $fe $92 $71 $72 }T
$7986 >PC $d6 >S $61 >A $ce >X $a9 >Y $60 >P $0059 $0a >M $005a $4b >M $7986 $92 >M $7987 $59 >M $7988 $13 >M
T{ op PC S A X Y P -> $7988 $d6 $61 $ce $a9 $60 }T T{ $0059 M $005a M $4b0a M $7986 M $7987 M $7988 M -> $0a $4b $61 $92 $59 $13 }T
$589f >PC $7d >S $88 >A $1c >X $28 >Y $e7 >P $00e8 $ab >M $00e9 $e2 >M $589f $92 >M $58a0 $e8 >M $58a1 $31 >M
T{ op PC S A X Y P -> $58a1 $7d $88 $1c $28 $e7 }T T{ $00e8 M $00e9 M $589f M $58a0 M $58a1 M $e2ab M -> $ab $e2 $92 $e8 $31 $88 }T
$4c5f >PC $7f >S $a9 >A $4e >X $a5 >Y $e4 >P $001a $70 >M $001b $68 >M $4c5f $92 >M $4c60 $1a >M $4c61 $ae >M
T{ op PC S A X Y P -> $4c61 $7f $a9 $4e $a5 $e4 }T T{ $001a M $001b M $4c5f M $4c60 M $4c61 M $6870 M -> $70 $68 $92 $1a $ae $a9 }T
$fcd0 >PC $b8 >S $75 >A $68 >X $7c >Y $a1 >P $0078 $18 >M $0079 $84 >M $fcd0 $92 >M $fcd1 $78 >M $fcd2 $c2 >M
T{ op PC S A X Y P -> $fcd2 $b8 $75 $68 $7c $a1 }T T{ $0078 M $0079 M $8418 M $fcd0 M $fcd1 M $fcd2 M -> $18 $84 $75 $92 $78 $c2 }T
$45f7 >PC $57 >S $42 >A $9f >X $59 >Y $61 >P $008e $e8 >M $008f $fa >M $45f7 $92 >M $45f8 $8e >M $45f9 $52 >M
T{ op PC S A X Y P -> $45f9 $57 $42 $9f $59 $61 }T T{ $008e M $008f M $45f7 M $45f8 M $45f9 M $fae8 M -> $e8 $fa $92 $8e $52 $42 }T
$d27d >PC $80 >S $aa >A $1c >X $94 >Y $26 >P $00d8 $75 >M $00d9 $44 >M $d27d $92 >M $d27e $d8 >M $d27f $85 >M
T{ op PC S A X Y P -> $d27f $80 $aa $1c $94 $26 }T T{ $00d8 M $00d9 M $4475 M $d27d M $d27e M $d27f M -> $75 $44 $aa $92 $d8 $85 }T
$a321 >PC $49 >S $05 >A $2b >X $0e >Y $23 >P $00e6 $9d >M $00e7 $f6 >M $a321 $92 >M $a322 $e6 >M $a323 $92 >M
T{ op PC S A X Y P -> $a323 $49 $05 $2b $0e $23 }T T{ $00e6 M $00e7 M $a321 M $a322 M $a323 M $f69d M -> $9d $f6 $92 $e6 $92 $05 }T
$ed2a >PC $5d >S $17 >A $d8 >X $fc >Y $e5 >P $0063 $68 >M $0064 $3c >M $ed2a $92 >M $ed2b $63 >M $ed2c $60 >M
T{ op PC S A X Y P -> $ed2c $5d $17 $d8 $fc $e5 }T T{ $0063 M $0064 M $3c68 M $ed2a M $ed2b M $ed2c M -> $68 $3c $17 $92 $63 $60 }T
$428e >PC $ba >S $61 >A $2a >X $3c >Y $e7 >P $0002 $b1 >M $0003 $b5 >M $428e $92 >M $428f $02 >M $4290 $82 >M
T{ op PC S A X Y P -> $4290 $ba $61 $2a $3c $e7 }T T{ $0002 M $0003 M $428e M $428f M $4290 M $b5b1 M -> $b1 $b5 $92 $02 $82 $61 }T
$3242 >PC $cd >S $09 >A $0c >X $c5 >Y $a4 >P $008b $64 >M $008c $85 >M $3242 $92 >M $3243 $8b >M $3244 $cb >M
T{ op PC S A X Y P -> $3244 $cd $09 $0c $c5 $a4 }T T{ $008b M $008c M $3242 M $3243 M $3244 M $8564 M -> $64 $85 $92 $8b $cb $09 }T
$f784 >PC $8c >S $3c >A $c2 >X $1c >Y $25 >P $00a6 $6f >M $00a7 $73 >M $f784 $92 >M $f785 $a6 >M $f786 $7a >M
T{ op PC S A X Y P -> $f786 $8c $3c $c2 $1c $25 }T T{ $00a6 M $00a7 M $736f M $f784 M $f785 M $f786 M -> $6f $73 $3c $92 $a6 $7a }T
$6cbf >PC $77 >S $92 >A $3d >X $ae >Y $20 >P $00d5 $04 >M $00d6 $00 >M $6cbf $92 >M $6cc0 $d5 >M $6cc1 $e9 >M
T{ op PC S A X Y P -> $6cc1 $77 $92 $3d $ae $20 }T T{ $0004 M $00d5 M $00d6 M $6cbf M $6cc0 M $6cc1 M -> $92 $04 $00 $92 $d5 $e9 }T
$1c4e >PC $13 >S $db >A $e3 >X $f3 >Y $65 >P $002f $35 >M $0030 $74 >M $1c4e $92 >M $1c4f $2f >M $1c50 $06 >M
T{ op PC S A X Y P -> $1c50 $13 $db $e3 $f3 $65 }T T{ $002f M $0030 M $1c4e M $1c4f M $1c50 M $7435 M -> $35 $74 $92 $2f $06 $db }T
( 93 )
$53bd >PC $60 >S $87 >A $d7 >X $f4 >Y $62 >P $53bd $93 >M $53be $9e >M $53bf $27 >M
T{ op PC S A X Y P -> $53be $60 $87 $d7 $f4 $62 }T T{ $53bd M $53be M $53bf M -> $93 $9e $27 }T
$243d >PC $11 >S $eb >A $89 >X $5f >Y $21 >P $243d $93 >M $243e $6c >M $243f $79 >M
T{ op PC S A X Y P -> $243e $11 $eb $89 $5f $21 }T T{ $243d M $243e M $243f M -> $93 $6c $79 }T
$cd41 >PC $46 >S $f1 >A $ab >X $af >Y $27 >P $cd41 $93 >M $cd42 $d9 >M $cd43 $e8 >M
T{ op PC S A X Y P -> $cd42 $46 $f1 $ab $af $27 }T T{ $cd41 M $cd42 M $cd43 M -> $93 $d9 $e8 }T
$4d13 >PC $46 >S $56 >A $52 >X $76 >Y $a1 >P $4d13 $93 >M $4d14 $c0 >M $4d15 $20 >M
T{ op PC S A X Y P -> $4d14 $46 $56 $52 $76 $a1 }T T{ $4d13 M $4d14 M $4d15 M -> $93 $c0 $20 }T
$2a4d >PC $1c >S $cc >A $c2 >X $10 >Y $a6 >P $2a4d $93 >M $2a4e $48 >M $2a4f $20 >M
T{ op PC S A X Y P -> $2a4e $1c $cc $c2 $10 $a6 }T T{ $2a4d M $2a4e M $2a4f M -> $93 $48 $20 }T
$ba5d >PC $20 >S $2c >A $dc >X $ea >Y $a7 >P $ba5d $93 >M $ba5e $ce >M $ba5f $93 >M
T{ op PC S A X Y P -> $ba5e $20 $2c $dc $ea $a7 }T T{ $ba5d M $ba5e M $ba5f M -> $93 $ce $93 }T
$b370 >PC $8d >S $51 >A $42 >X $59 >Y $27 >P $b370 $93 >M $b371 $67 >M $b372 $7e >M
T{ op PC S A X Y P -> $b371 $8d $51 $42 $59 $27 }T T{ $b370 M $b371 M $b372 M -> $93 $67 $7e }T
$284f >PC $90 >S $fb >A $67 >X $da >Y $e4 >P $284f $93 >M $2850 $ba >M $2851 $98 >M
T{ op PC S A X Y P -> $2850 $90 $fb $67 $da $e4 }T T{ $284f M $2850 M $2851 M -> $93 $ba $98 }T
$27d3 >PC $6a >S $3e >A $63 >X $b4 >Y $a0 >P $27d3 $93 >M $27d4 $ed >M $27d5 $72 >M
T{ op PC S A X Y P -> $27d4 $6a $3e $63 $b4 $a0 }T T{ $27d3 M $27d4 M $27d5 M -> $93 $ed $72 }T
$eff6 >PC $fb >S $92 >A $e7 >X $e2 >Y $21 >P $eff6 $93 >M $eff7 $27 >M $eff8 $a5 >M
T{ op PC S A X Y P -> $eff7 $fb $92 $e7 $e2 $21 }T T{ $eff6 M $eff7 M $eff8 M -> $93 $27 $a5 }T
$e3b2 >PC $1c >S $c7 >A $86 >X $cf >Y $e6 >P $e3b2 $93 >M $e3b3 $1e >M $e3b4 $50 >M
T{ op PC S A X Y P -> $e3b3 $1c $c7 $86 $cf $e6 }T T{ $e3b2 M $e3b3 M $e3b4 M -> $93 $1e $50 }T
$b3b4 >PC $27 >S $20 >A $01 >X $d0 >Y $a4 >P $b3b4 $93 >M $b3b5 $72 >M $b3b6 $31 >M
T{ op PC S A X Y P -> $b3b5 $27 $20 $01 $d0 $a4 }T T{ $b3b4 M $b3b5 M $b3b6 M -> $93 $72 $31 }T
$b4ca >PC $67 >S $b0 >A $18 >X $2d >Y $66 >P $b4ca $93 >M $b4cb $5f >M $b4cc $b8 >M
T{ op PC S A X Y P -> $b4cb $67 $b0 $18 $2d $66 }T T{ $b4ca M $b4cb M $b4cc M -> $93 $5f $b8 }T
$f727 >PC $02 >S $39 >A $20 >X $99 >Y $e5 >P $f727 $93 >M $f728 $51 >M $f729 $f7 >M
T{ op PC S A X Y P -> $f728 $02 $39 $20 $99 $e5 }T T{ $f727 M $f728 M $f729 M -> $93 $51 $f7 }T
$20da >PC $a2 >S $43 >A $c3 >X $38 >Y $20 >P $20da $93 >M $20db $f0 >M $20dc $ba >M
T{ op PC S A X Y P -> $20db $a2 $43 $c3 $38 $20 }T T{ $20da M $20db M $20dc M -> $93 $f0 $ba }T
$c966 >PC $63 >S $3f >A $db >X $36 >Y $a5 >P $c966 $93 >M $c967 $37 >M $c968 $2d >M
T{ op PC S A X Y P -> $c967 $63 $3f $db $36 $a5 }T T{ $c966 M $c967 M $c968 M -> $93 $37 $2d }T
( 94 )
$c3c8 >PC $4b >S $d3 >A $b2 >X $ac >Y $a0 >P $0059 $e8 >M $c3c8 $94 >M $c3c9 $59 >M $c3ca $6e >M
T{ op PC S A X Y P -> $c3ca $4b $d3 $b2 $ac $a0 }T T{ $000b M $0059 M $c3c8 M $c3c9 M $c3ca M -> $ac $e8 $94 $59 $6e }T
$a832 >PC $78 >S $bf >A $8a >X $4e >Y $65 >P $0081 $4f >M $a832 $94 >M $a833 $81 >M $a834 $3b >M
T{ op PC S A X Y P -> $a834 $78 $bf $8a $4e $65 }T T{ $000b M $0081 M $a832 M $a833 M $a834 M -> $4e $4f $94 $81 $3b }T
$fb61 >PC $08 >S $ea >A $93 >X $fe >Y $27 >P $0091 $33 >M $fb61 $94 >M $fb62 $91 >M $fb63 $cb >M
T{ op PC S A X Y P -> $fb63 $08 $ea $93 $fe $27 }T T{ $0024 M $0091 M $fb61 M $fb62 M $fb63 M -> $fe $33 $94 $91 $cb }T
$72a5 >PC $29 >S $15 >A $34 >X $5c >Y $a2 >P $0084 $86 >M $72a5 $94 >M $72a6 $84 >M $72a7 $96 >M
T{ op PC S A X Y P -> $72a7 $29 $15 $34 $5c $a2 }T T{ $0084 M $00b8 M $72a5 M $72a6 M $72a7 M -> $86 $5c $94 $84 $96 }T
$0e13 >PC $ac >S $23 >A $80 >X $15 >Y $e6 >P $00ed $37 >M $0e13 $94 >M $0e14 $ed >M $0e15 $d3 >M
T{ op PC S A X Y P -> $0e15 $ac $23 $80 $15 $e6 }T T{ $006d M $00ed M $0e13 M $0e14 M $0e15 M -> $15 $37 $94 $ed $d3 }T
$8d0b >PC $33 >S $5b >A $da >X $32 >Y $e3 >P $0091 $c7 >M $8d0b $94 >M $8d0c $91 >M $8d0d $87 >M
T{ op PC S A X Y P -> $8d0d $33 $5b $da $32 $e3 }T T{ $006b M $0091 M $8d0b M $8d0c M $8d0d M -> $32 $c7 $94 $91 $87 }T
$e229 >PC $ff >S $35 >A $aa >X $ad >Y $26 >P $00e2 $28 >M $e229 $94 >M $e22a $e2 >M $e22b $09 >M
T{ op PC S A X Y P -> $e22b $ff $35 $aa $ad $26 }T T{ $008c M $00e2 M $e229 M $e22a M $e22b M -> $ad $28 $94 $e2 $09 }T
$9df8 >PC $a7 >S $25 >A $32 >X $48 >Y $63 >P $00a4 $0f >M $9df8 $94 >M $9df9 $a4 >M $9dfa $c1 >M
T{ op PC S A X Y P -> $9dfa $a7 $25 $32 $48 $63 }T T{ $00a4 M $00d6 M $9df8 M $9df9 M $9dfa M -> $0f $48 $94 $a4 $c1 }T
$9e04 >PC $a3 >S $b5 >A $e8 >X $fd >Y $a7 >P $009e $3a >M $9e04 $94 >M $9e05 $9e >M $9e06 $4e >M
T{ op PC S A X Y P -> $9e06 $a3 $b5 $e8 $fd $a7 }T T{ $0086 M $009e M $9e04 M $9e05 M $9e06 M -> $fd $3a $94 $9e $4e }T
$1ddf >PC $91 >S $b5 >A $9a >X $ba >Y $24 >P $00d0 $8e >M $1ddf $94 >M $1de0 $d0 >M $1de1 $15 >M
T{ op PC S A X Y P -> $1de1 $91 $b5 $9a $ba $24 }T T{ $006a M $00d0 M $1ddf M $1de0 M $1de1 M -> $ba $8e $94 $d0 $15 }T
$fb20 >PC $af >S $14 >A $cb >X $8d >Y $65 >P $00be $9b >M $fb20 $94 >M $fb21 $be >M $fb22 $ed >M
T{ op PC S A X Y P -> $fb22 $af $14 $cb $8d $65 }T T{ $0089 M $00be M $fb20 M $fb21 M $fb22 M -> $8d $9b $94 $be $ed }T
$84a2 >PC $36 >S $1b >A $ec >X $e1 >Y $25 >P $00a8 $e2 >M $84a2 $94 >M $84a3 $a8 >M $84a4 $68 >M
T{ op PC S A X Y P -> $84a4 $36 $1b $ec $e1 $25 }T T{ $0094 M $00a8 M $84a2 M $84a3 M $84a4 M -> $e1 $e2 $94 $a8 $68 }T
$1d21 >PC $ad >S $f6 >A $c0 >X $16 >Y $25 >P $0027 $10 >M $1d21 $94 >M $1d22 $27 >M $1d23 $f0 >M
T{ op PC S A X Y P -> $1d23 $ad $f6 $c0 $16 $25 }T T{ $0027 M $00e7 M $1d21 M $1d22 M $1d23 M -> $10 $16 $94 $27 $f0 }T
$ddd1 >PC $ed >S $23 >A $4c >X $a7 >Y $61 >P $00e1 $08 >M $ddd1 $94 >M $ddd2 $e1 >M $ddd3 $29 >M
T{ op PC S A X Y P -> $ddd3 $ed $23 $4c $a7 $61 }T T{ $002d M $00e1 M $ddd1 M $ddd2 M $ddd3 M -> $a7 $08 $94 $e1 $29 }T
$977c >PC $48 >S $fb >A $cb >X $d9 >Y $61 >P $006f $6c >M $977c $94 >M $977d $6f >M $977e $3e >M
T{ op PC S A X Y P -> $977e $48 $fb $cb $d9 $61 }T T{ $003a M $006f M $977c M $977d M $977e M -> $d9 $6c $94 $6f $3e }T
$188d >PC $cb >S $2e >A $e9 >X $c6 >Y $62 >P $00a6 $dc >M $188d $94 >M $188e $a6 >M $188f $aa >M
T{ op PC S A X Y P -> $188f $cb $2e $e9 $c6 $62 }T T{ $008f M $00a6 M $188d M $188e M $188f M -> $c6 $dc $94 $a6 $aa }T
( 95 )
$355d >PC $fa >S $35 >A $5e >X $bf >Y $e3 >P $0075 $34 >M $355d $95 >M $355e $75 >M $355f $a9 >M
T{ op PC S A X Y P -> $355f $fa $35 $5e $bf $e3 }T T{ $0075 M $00d3 M $355d M $355e M $355f M -> $34 $35 $95 $75 $a9 }T
$55bc >PC $6e >S $2d >A $34 >X $87 >Y $65 >P $0005 $3d >M $55bc $95 >M $55bd $05 >M $55be $56 >M
T{ op PC S A X Y P -> $55be $6e $2d $34 $87 $65 }T T{ $0005 M $0039 M $55bc M $55bd M $55be M -> $3d $2d $95 $05 $56 }T
$34e4 >PC $b0 >S $97 >A $29 >X $29 >Y $a1 >P $0086 $5e >M $34e4 $95 >M $34e5 $86 >M $34e6 $63 >M
T{ op PC S A X Y P -> $34e6 $b0 $97 $29 $29 $a1 }T T{ $0086 M $00af M $34e4 M $34e5 M $34e6 M -> $5e $97 $95 $86 $63 }T
$fe1e >PC $6b >S $6e >A $cf >X $0c >Y $66 >P $0074 $79 >M $fe1e $95 >M $fe1f $74 >M $fe20 $db >M
T{ op PC S A X Y P -> $fe20 $6b $6e $cf $0c $66 }T T{ $0043 M $0074 M $fe1e M $fe1f M $fe20 M -> $6e $79 $95 $74 $db }T
$7aa8 >PC $4e >S $1a >A $ba >X $7d >Y $a2 >P $003a $d1 >M $7aa8 $95 >M $7aa9 $3a >M $7aaa $26 >M
T{ op PC S A X Y P -> $7aaa $4e $1a $ba $7d $a2 }T T{ $003a M $00f4 M $7aa8 M $7aa9 M $7aaa M -> $d1 $1a $95 $3a $26 }T
$52f9 >PC $99 >S $5b >A $fa >X $ae >Y $a1 >P $0047 $e1 >M $52f9 $95 >M $52fa $47 >M $52fb $08 >M
T{ op PC S A X Y P -> $52fb $99 $5b $fa $ae $a1 }T T{ $0041 M $0047 M $52f9 M $52fa M $52fb M -> $5b $e1 $95 $47 $08 }T
$08cf >PC $a9 >S $58 >A $c1 >X $27 >Y $e3 >P $005e $f0 >M $08cf $95 >M $08d0 $5e >M $08d1 $30 >M
T{ op PC S A X Y P -> $08d1 $a9 $58 $c1 $27 $e3 }T T{ $001f M $005e M $08cf M $08d0 M $08d1 M -> $58 $f0 $95 $5e $30 }T
$3e59 >PC $ee >S $ad >A $72 >X $cf >Y $20 >P $0081 $84 >M $3e59 $95 >M $3e5a $81 >M $3e5b $18 >M
T{ op PC S A X Y P -> $3e5b $ee $ad $72 $cf $20 }T T{ $0081 M $00f3 M $3e59 M $3e5a M $3e5b M -> $84 $ad $95 $81 $18 }T
$82db >PC $09 >S $71 >A $21 >X $61 >Y $e7 >P $008d $ec >M $82db $95 >M $82dc $8d >M $82dd $c3 >M
T{ op PC S A X Y P -> $82dd $09 $71 $21 $61 $e7 }T T{ $008d M $00ae M $82db M $82dc M $82dd M -> $ec $71 $95 $8d $c3 }T
$8a91 >PC $2d >S $e1 >A $c0 >X $b7 >Y $23 >P $0097 $33 >M $8a91 $95 >M $8a92 $97 >M $8a93 $80 >M
T{ op PC S A X Y P -> $8a93 $2d $e1 $c0 $b7 $23 }T T{ $0057 M $0097 M $8a91 M $8a92 M $8a93 M -> $e1 $33 $95 $97 $80 }T
$da5a >PC $52 >S $90 >A $55 >X $95 >Y $a2 >P $00f8 $e5 >M $da5a $95 >M $da5b $f8 >M $da5c $49 >M
T{ op PC S A X Y P -> $da5c $52 $90 $55 $95 $a2 }T T{ $004d M $00f8 M $da5a M $da5b M $da5c M -> $90 $e5 $95 $f8 $49 }T
$d5e8 >PC $51 >S $97 >A $d8 >X $3f >Y $23 >P $007e $98 >M $d5e8 $95 >M $d5e9 $7e >M $d5ea $7c >M
T{ op PC S A X Y P -> $d5ea $51 $97 $d8 $3f $23 }T T{ $0056 M $007e M $d5e8 M $d5e9 M $d5ea M -> $97 $98 $95 $7e $7c }T
$c6c7 >PC $8d >S $a4 >A $0e >X $90 >Y $26 >P $00c1 $28 >M $c6c7 $95 >M $c6c8 $c1 >M $c6c9 $48 >M
T{ op PC S A X Y P -> $c6c9 $8d $a4 $0e $90 $26 }T T{ $00c1 M $00cf M $c6c7 M $c6c8 M $c6c9 M -> $28 $a4 $95 $c1 $48 }T
$cf69 >PC $cb >S $1d >A $7b >X $a0 >Y $a3 >P $006f $d0 >M $cf69 $95 >M $cf6a $6f >M $cf6b $a5 >M
T{ op PC S A X Y P -> $cf6b $cb $1d $7b $a0 $a3 }T T{ $006f M $00ea M $cf69 M $cf6a M $cf6b M -> $d0 $1d $95 $6f $a5 }T
$5426 >PC $82 >S $97 >A $a6 >X $bd >Y $a3 >P $0016 $28 >M $5426 $95 >M $5427 $16 >M $5428 $7c >M
T{ op PC S A X Y P -> $5428 $82 $97 $a6 $bd $a3 }T T{ $0016 M $00bc M $5426 M $5427 M $5428 M -> $28 $97 $95 $16 $7c }T
$491c >PC $31 >S $64 >A $8a >X $19 >Y $22 >P $00cd $7b >M $491c $95 >M $491d $cd >M $491e $5c >M
T{ op PC S A X Y P -> $491e $31 $64 $8a $19 $22 }T T{ $0057 M $00cd M $491c M $491d M $491e M -> $64 $7b $95 $cd $5c }T
( 96 )
$dcb9 >PC $3c >S $e2 >A $ff >X $1f >Y $27 >P $0077 $8c >M $dcb9 $96 >M $dcba $77 >M $dcbb $74 >M
T{ op PC S A X Y P -> $dcbb $3c $e2 $ff $1f $27 }T T{ $0077 M $0096 M $dcb9 M $dcba M $dcbb M -> $8c $ff $96 $77 $74 }T
$9119 >PC $33 >S $08 >A $63 >X $83 >Y $26 >P $0004 $b7 >M $9119 $96 >M $911a $04 >M $911b $ad >M
T{ op PC S A X Y P -> $911b $33 $08 $63 $83 $26 }T T{ $0004 M $0087 M $9119 M $911a M $911b M -> $b7 $63 $96 $04 $ad }T
$01a2 >PC $71 >S $ae >A $87 >X $c3 >Y $63 >P $00ec $c6 >M $01a2 $96 >M $01a3 $ec >M $01a4 $e3 >M
T{ op PC S A X Y P -> $01a4 $71 $ae $87 $c3 $63 }T T{ $00af M $00ec M $01a2 M $01a3 M $01a4 M -> $87 $c6 $96 $ec $e3 }T
$4f26 >PC $bc >S $ff >A $b5 >X $d7 >Y $22 >P $005a $6b >M $4f26 $96 >M $4f27 $5a >M $4f28 $64 >M
T{ op PC S A X Y P -> $4f28 $bc $ff $b5 $d7 $22 }T T{ $0031 M $005a M $4f26 M $4f27 M $4f28 M -> $b5 $6b $96 $5a $64 }T
$0169 >PC $85 >S $f6 >A $91 >X $e5 >Y $60 >P $0074 $e9 >M $0169 $96 >M $016a $74 >M $016b $3c >M
T{ op PC S A X Y P -> $016b $85 $f6 $91 $e5 $60 }T T{ $0059 M $0074 M $0169 M $016a M $016b M -> $91 $e9 $96 $74 $3c }T
$18c2 >PC $3c >S $9e >A $0a >X $7d >Y $e7 >P $00ac $42 >M $18c2 $96 >M $18c3 $ac >M $18c4 $04 >M
T{ op PC S A X Y P -> $18c4 $3c $9e $0a $7d $e7 }T T{ $0029 M $00ac M $18c2 M $18c3 M $18c4 M -> $0a $42 $96 $ac $04 }T
$8fa3 >PC $da >S $0a >A $eb >X $b4 >Y $60 >P $007a $35 >M $8fa3 $96 >M $8fa4 $7a >M $8fa5 $72 >M
T{ op PC S A X Y P -> $8fa5 $da $0a $eb $b4 $60 }T T{ $002e M $007a M $8fa3 M $8fa4 M $8fa5 M -> $eb $35 $96 $7a $72 }T
$56a5 >PC $eb >S $03 >A $b5 >X $d1 >Y $67 >P $0069 $46 >M $56a5 $96 >M $56a6 $69 >M $56a7 $9c >M
T{ op PC S A X Y P -> $56a7 $eb $03 $b5 $d1 $67 }T T{ $003a M $0069 M $56a5 M $56a6 M $56a7 M -> $b5 $46 $96 $69 $9c }T
$c523 >PC $7c >S $a5 >A $98 >X $ef >Y $a1 >P $0096 $e0 >M $c523 $96 >M $c524 $96 >M $c525 $45 >M
T{ op PC S A X Y P -> $c525 $7c $a5 $98 $ef $a1 }T T{ $0085 M $0096 M $c523 M $c524 M $c525 M -> $98 $e0 $96 $96 $45 }T
$86dc >PC $ed >S $3e >A $d3 >X $5e >Y $24 >P $001c $a9 >M $86dc $96 >M $86dd $1c >M $86de $38 >M
T{ op PC S A X Y P -> $86de $ed $3e $d3 $5e $24 }T T{ $001c M $007a M $86dc M $86dd M $86de M -> $a9 $d3 $96 $1c $38 }T
$96c5 >PC $c5 >S $e9 >A $16 >X $5f >Y $a6 >P $001b $58 >M $96c5 $96 >M $96c6 $1b >M $96c7 $d9 >M
T{ op PC S A X Y P -> $96c7 $c5 $e9 $16 $5f $a6 }T T{ $001b M $007a M $96c5 M $96c6 M $96c7 M -> $58 $16 $96 $1b $d9 }T
$4ab7 >PC $8c >S $ed >A $a8 >X $74 >Y $a7 >P $00f5 $d8 >M $4ab7 $96 >M $4ab8 $f5 >M $4ab9 $ff >M
T{ op PC S A X Y P -> $4ab9 $8c $ed $a8 $74 $a7 }T T{ $0069 M $00f5 M $4ab7 M $4ab8 M $4ab9 M -> $a8 $d8 $96 $f5 $ff }T
$77cb >PC $0c >S $7b >A $2f >X $f2 >Y $a1 >P $0013 $0c >M $77cb $96 >M $77cc $13 >M $77cd $4b >M
T{ op PC S A X Y P -> $77cd $0c $7b $2f $f2 $a1 }T T{ $0005 M $0013 M $77cb M $77cc M $77cd M -> $2f $0c $96 $13 $4b }T
$c40c >PC $dc >S $87 >A $dc >X $67 >Y $63 >P $004a $ec >M $c40c $96 >M $c40d $4a >M $c40e $83 >M
T{ op PC S A X Y P -> $c40e $dc $87 $dc $67 $63 }T T{ $004a M $00b1 M $c40c M $c40d M $c40e M -> $ec $dc $96 $4a $83 }T
$7a75 >PC $f8 >S $a6 >A $d9 >X $ff >Y $23 >P $0017 $cf >M $7a75 $96 >M $7a76 $17 >M $7a77 $09 >M
T{ op PC S A X Y P -> $7a77 $f8 $a6 $d9 $ff $23 }T T{ $0016 M $0017 M $7a75 M $7a76 M $7a77 M -> $d9 $cf $96 $17 $09 }T
$047f >PC $3d >S $48 >A $13 >X $7e >Y $20 >P $007d $06 >M $047f $96 >M $0480 $7d >M $0481 $c4 >M
T{ op PC S A X Y P -> $0481 $3d $48 $13 $7e $20 }T T{ $007d M $00fb M $047f M $0480 M $0481 M -> $06 $13 $96 $7d $c4 }T
( 97 )
$0553 >PC $2a >S $32 >A $82 >X $fc >Y $e5 >P $00db $6b >M $0553 $97 >M $0554 $db >M $0555 $1b >M
T{ op PC S A X Y P -> $0555 $2a $32 $82 $fc $e5 }T T{ $00db M $0553 M $0554 M $0555 M -> $6b $97 $db $1b }T
$9589 >PC $41 >S $4a >A $26 >X $03 >Y $20 >P $00c1 $e4 >M $9589 $97 >M $958a $c1 >M $958b $f1 >M
T{ op PC S A X Y P -> $958b $41 $4a $26 $03 $20 }T T{ $00c1 M $9589 M $958a M $958b M -> $e6 $97 $c1 $f1 }T
$5036 >PC $51 >S $10 >A $1a >X $1f >Y $65 >P $00a4 $9f >M $5036 $97 >M $5037 $a4 >M $5038 $30 >M
T{ op PC S A X Y P -> $5038 $51 $10 $1a $1f $65 }T T{ $00a4 M $5036 M $5037 M $5038 M -> $9f $97 $a4 $30 }T
$0d1f >PC $15 >S $ac >A $2c >X $23 >Y $e4 >P $00d0 $a1 >M $0d1f $97 >M $0d20 $d0 >M $0d21 $c0 >M
T{ op PC S A X Y P -> $0d21 $15 $ac $2c $23 $e4 }T T{ $00d0 M $0d1f M $0d20 M $0d21 M -> $a3 $97 $d0 $c0 }T
$42b8 >PC $54 >S $68 >A $d1 >X $8e >Y $21 >P $00b7 $63 >M $42b8 $97 >M $42b9 $b7 >M $42ba $31 >M
T{ op PC S A X Y P -> $42ba $54 $68 $d1 $8e $21 }T T{ $00b7 M $42b8 M $42b9 M $42ba M -> $63 $97 $b7 $31 }T
$0f38 >PC $a2 >S $ac >A $79 >X $3b >Y $a1 >P $0032 $c7 >M $0f38 $97 >M $0f39 $32 >M $0f3a $2b >M
T{ op PC S A X Y P -> $0f3a $a2 $ac $79 $3b $a1 }T T{ $0032 M $0f38 M $0f39 M $0f3a M -> $c7 $97 $32 $2b }T
$ae11 >PC $ee >S $7b >A $08 >X $48 >Y $25 >P $0018 $d4 >M $ae11 $97 >M $ae12 $18 >M $ae13 $8f >M
T{ op PC S A X Y P -> $ae13 $ee $7b $08 $48 $25 }T T{ $0018 M $ae11 M $ae12 M $ae13 M -> $d6 $97 $18 $8f }T
$4b0c >PC $e1 >S $c0 >A $20 >X $8c >Y $a3 >P $0068 $20 >M $4b0c $97 >M $4b0d $68 >M $4b0e $dc >M
T{ op PC S A X Y P -> $4b0e $e1 $c0 $20 $8c $a3 }T T{ $0068 M $4b0c M $4b0d M $4b0e M -> $22 $97 $68 $dc }T
$7090 >PC $b3 >S $59 >A $40 >X $79 >Y $22 >P $0022 $5e >M $7090 $97 >M $7091 $22 >M $7092 $fb >M
T{ op PC S A X Y P -> $7092 $b3 $59 $40 $79 $22 }T T{ $0022 M $7090 M $7091 M $7092 M -> $5e $97 $22 $fb }T
$ce5e >PC $12 >S $73 >A $76 >X $a5 >Y $67 >P $00f4 $4b >M $ce5e $97 >M $ce5f $f4 >M $ce60 $52 >M
T{ op PC S A X Y P -> $ce60 $12 $73 $76 $a5 $67 }T T{ $00f4 M $ce5e M $ce5f M $ce60 M -> $4b $97 $f4 $52 }T
$406f >PC $ae >S $0d >A $57 >X $3a >Y $a7 >P $0039 $c7 >M $406f $97 >M $4070 $39 >M $4071 $73 >M
T{ op PC S A X Y P -> $4071 $ae $0d $57 $3a $a7 }T T{ $0039 M $406f M $4070 M $4071 M -> $c7 $97 $39 $73 }T
$0624 >PC $07 >S $28 >A $8d >X $d6 >Y $a0 >P $00b4 $6b >M $0624 $97 >M $0625 $b4 >M $0626 $a4 >M
T{ op PC S A X Y P -> $0626 $07 $28 $8d $d6 $a0 }T T{ $00b4 M $0624 M $0625 M $0626 M -> $6b $97 $b4 $a4 }T
$702b >PC $38 >S $ff >A $9d >X $d4 >Y $a0 >P $001c $2b >M $702b $97 >M $702c $1c >M $702d $65 >M
T{ op PC S A X Y P -> $702d $38 $ff $9d $d4 $a0 }T T{ $001c M $702b M $702c M $702d M -> $2b $97 $1c $65 }T
$c3d1 >PC $ee >S $6a >A $08 >X $1f >Y $a2 >P $0063 $68 >M $c3d1 $97 >M $c3d2 $63 >M $c3d3 $09 >M
T{ op PC S A X Y P -> $c3d3 $ee $6a $08 $1f $a2 }T T{ $0063 M $c3d1 M $c3d2 M $c3d3 M -> $6a $97 $63 $09 }T
$1faf >PC $ed >S $ae >A $e1 >X $ce >Y $a2 >P $003e $0a >M $1faf $97 >M $1fb0 $3e >M $1fb1 $71 >M
T{ op PC S A X Y P -> $1fb1 $ed $ae $e1 $ce $a2 }T T{ $003e M $1faf M $1fb0 M $1fb1 M -> $0a $97 $3e $71 }T
$630c >PC $3d >S $39 >A $f2 >X $61 >Y $e0 >P $005b $2a >M $630c $97 >M $630d $5b >M $630e $c6 >M
T{ op PC S A X Y P -> $630e $3d $39 $f2 $61 $e0 }T T{ $005b M $630c M $630d M $630e M -> $2a $97 $5b $c6 }T
( 98 )
$7be9 >PC $3d >S $9b >A $2b >X $75 >Y $60 >P $7be9 $98 >M $7bea $6d >M $7beb $71 >M
T{ op PC S A X Y P -> $7bea $3d $75 $2b $75 $60 }T T{ $7be9 M $7bea M $7beb M -> $98 $6d $71 }T
$51c5 >PC $be >S $50 >A $cd >X $6b >Y $e1 >P $51c5 $98 >M $51c6 $f2 >M $51c7 $e5 >M
T{ op PC S A X Y P -> $51c6 $be $6b $cd $6b $61 }T T{ $51c5 M $51c6 M $51c7 M -> $98 $f2 $e5 }T
$0fcb >PC $a3 >S $f8 >A $c1 >X $e8 >Y $26 >P $0fcb $98 >M $0fcc $65 >M $0fcd $93 >M
T{ op PC S A X Y P -> $0fcc $a3 $e8 $c1 $e8 $a4 }T T{ $0fcb M $0fcc M $0fcd M -> $98 $65 $93 }T
$2881 >PC $0b >S $52 >A $c1 >X $ba >Y $e2 >P $2881 $98 >M $2882 $b6 >M $2883 $f2 >M
T{ op PC S A X Y P -> $2882 $0b $ba $c1 $ba $e0 }T T{ $2881 M $2882 M $2883 M -> $98 $b6 $f2 }T
$0e12 >PC $5e >S $86 >A $49 >X $f0 >Y $61 >P $0e12 $98 >M $0e13 $b9 >M $0e14 $95 >M
T{ op PC S A X Y P -> $0e13 $5e $f0 $49 $f0 $e1 }T T{ $0e12 M $0e13 M $0e14 M -> $98 $b9 $95 }T
$16c3 >PC $20 >S $57 >A $61 >X $a2 >Y $25 >P $16c3 $98 >M $16c4 $70 >M $16c5 $67 >M
T{ op PC S A X Y P -> $16c4 $20 $a2 $61 $a2 $a5 }T T{ $16c3 M $16c4 M $16c5 M -> $98 $70 $67 }T
$5495 >PC $3e >S $5e >A $22 >X $3a >Y $23 >P $5495 $98 >M $5496 $b7 >M $5497 $64 >M
T{ op PC S A X Y P -> $5496 $3e $3a $22 $3a $21 }T T{ $5495 M $5496 M $5497 M -> $98 $b7 $64 }T
$5450 >PC $5c >S $f4 >A $85 >X $a9 >Y $a1 >P $5450 $98 >M $5451 $72 >M $5452 $21 >M
T{ op PC S A X Y P -> $5451 $5c $a9 $85 $a9 $a1 }T T{ $5450 M $5451 M $5452 M -> $98 $72 $21 }T
$a135 >PC $49 >S $a7 >A $63 >X $b2 >Y $60 >P $a135 $98 >M $a136 $3d >M $a137 $dd >M
T{ op PC S A X Y P -> $a136 $49 $b2 $63 $b2 $e0 }T T{ $a135 M $a136 M $a137 M -> $98 $3d $dd }T
$773e >PC $5f >S $42 >A $f1 >X $04 >Y $21 >P $773e $98 >M $773f $2c >M $7740 $ab >M
T{ op PC S A X Y P -> $773f $5f $04 $f1 $04 $21 }T T{ $773e M $773f M $7740 M -> $98 $2c $ab }T
$a5cc >PC $79 >S $00 >A $7c >X $82 >Y $e6 >P $a5cc $98 >M $a5cd $28 >M $a5ce $0e >M
T{ op PC S A X Y P -> $a5cd $79 $82 $7c $82 $e4 }T T{ $a5cc M $a5cd M $a5ce M -> $98 $28 $0e }T
$012e >PC $cf >S $3b >A $33 >X $33 >Y $a5 >P $012e $98 >M $012f $d4 >M $0130 $5f >M
T{ op PC S A X Y P -> $012f $cf $33 $33 $33 $25 }T T{ $012e M $012f M $0130 M -> $98 $d4 $5f }T
$4c89 >PC $f7 >S $5d >A $e2 >X $2b >Y $24 >P $4c89 $98 >M $4c8a $41 >M $4c8b $aa >M
T{ op PC S A X Y P -> $4c8a $f7 $2b $e2 $2b $24 }T T{ $4c89 M $4c8a M $4c8b M -> $98 $41 $aa }T
$0abd >PC $6e >S $d2 >A $2c >X $94 >Y $a3 >P $0abd $98 >M $0abe $1e >M $0abf $c1 >M
T{ op PC S A X Y P -> $0abe $6e $94 $2c $94 $a1 }T T{ $0abd M $0abe M $0abf M -> $98 $1e $c1 }T
$e386 >PC $b8 >S $e5 >A $39 >X $1c >Y $e2 >P $e386 $98 >M $e387 $7d >M $e388 $5b >M
T{ op PC S A X Y P -> $e387 $b8 $1c $39 $1c $60 }T T{ $e386 M $e387 M $e388 M -> $98 $7d $5b }T
$5147 >PC $4e >S $de >A $9e >X $aa >Y $a4 >P $5147 $98 >M $5148 $f5 >M $5149 $da >M
T{ op PC S A X Y P -> $5148 $4e $aa $9e $aa $a4 }T T{ $5147 M $5148 M $5149 M -> $98 $f5 $da }T
( 99 )
$38c9 >PC $d4 >S $88 >A $59 >X $3c >Y $a4 >P $38c9 $99 >M $38ca $15 >M $38cb $58 >M $38cc $22 >M
T{ op PC S A X Y P -> $38cc $d4 $88 $59 $3c $a4 }T T{ $38c9 M $38ca M $38cb M $38cc M $5851 M -> $99 $15 $58 $22 $88 }T
$1a40 >PC $1d >S $91 >A $8b >X $a1 >Y $26 >P $1a40 $99 >M $1a41 $7e >M $1a42 $ed >M $1a43 $c1 >M
T{ op PC S A X Y P -> $1a43 $1d $91 $8b $a1 $26 }T T{ $1a40 M $1a41 M $1a42 M $1a43 M $ee1f M -> $99 $7e $ed $c1 $91 }T
$1894 >PC $60 >S $7a >A $7b >X $8d >Y $e6 >P $1894 $99 >M $1895 $08 >M $1896 $0e >M $1897 $dd >M
T{ op PC S A X Y P -> $1897 $60 $7a $7b $8d $e6 }T T{ $0e95 M $1894 M $1895 M $1896 M $1897 M -> $7a $99 $08 $0e $dd }T
$5861 >PC $94 >S $ea >A $0c >X $f7 >Y $60 >P $5861 $99 >M $5862 $df >M $5863 $d8 >M $5864 $bd >M
T{ op PC S A X Y P -> $5864 $94 $ea $0c $f7 $60 }T T{ $5861 M $5862 M $5863 M $5864 M $d9d6 M -> $99 $df $d8 $bd $ea }T
$977b >PC $3e >S $16 >A $07 >X $ea >Y $20 >P $977b $99 >M $977c $18 >M $977d $68 >M $977e $7a >M
T{ op PC S A X Y P -> $977e $3e $16 $07 $ea $20 }T T{ $6902 M $977b M $977c M $977d M $977e M -> $16 $99 $18 $68 $7a }T
$9725 >PC $ec >S $5b >A $86 >X $c4 >Y $a4 >P $9725 $99 >M $9726 $d5 >M $9727 $d8 >M $9728 $02 >M
T{ op PC S A X Y P -> $9728 $ec $5b $86 $c4 $a4 }T T{ $9725 M $9726 M $9727 M $9728 M $d999 M -> $99 $d5 $d8 $02 $5b }T
$c273 >PC $31 >S $22 >A $9e >X $32 >Y $65 >P $c273 $99 >M $c274 $c4 >M $c275 $77 >M $c276 $d7 >M
T{ op PC S A X Y P -> $c276 $31 $22 $9e $32 $65 }T T{ $77f6 M $c273 M $c274 M $c275 M $c276 M -> $22 $99 $c4 $77 $d7 }T
$1513 >PC $fe >S $61 >A $0b >X $06 >Y $26 >P $1513 $99 >M $1514 $38 >M $1515 $cd >M $1516 $70 >M
T{ op PC S A X Y P -> $1516 $fe $61 $0b $06 $26 }T T{ $1513 M $1514 M $1515 M $1516 M $cd3e M -> $99 $38 $cd $70 $61 }T
$3cd1 >PC $0b >S $08 >A $85 >X $13 >Y $a3 >P $3cd1 $99 >M $3cd2 $75 >M $3cd3 $ca >M $3cd4 $1e >M
T{ op PC S A X Y P -> $3cd4 $0b $08 $85 $13 $a3 }T T{ $3cd1 M $3cd2 M $3cd3 M $3cd4 M $ca88 M -> $99 $75 $ca $1e $08 }T
$5c1a >PC $68 >S $f2 >A $72 >X $a9 >Y $e7 >P $5c1a $99 >M $5c1b $c9 >M $5c1c $37 >M $5c1d $63 >M
T{ op PC S A X Y P -> $5c1d $68 $f2 $72 $a9 $e7 }T T{ $3872 M $5c1a M $5c1b M $5c1c M $5c1d M -> $f2 $99 $c9 $37 $63 }T
$3870 >PC $a1 >S $c7 >A $b1 >X $47 >Y $61 >P $3870 $99 >M $3871 $ed >M $3872 $a7 >M $3873 $77 >M
T{ op PC S A X Y P -> $3873 $a1 $c7 $b1 $47 $61 }T T{ $3870 M $3871 M $3872 M $3873 M $a834 M -> $99 $ed $a7 $77 $c7 }T
$d703 >PC $23 >S $96 >A $9d >X $72 >Y $e5 >P $d703 $99 >M $d704 $b5 >M $d705 $5c >M $d706 $ec >M
T{ op PC S A X Y P -> $d706 $23 $96 $9d $72 $e5 }T T{ $5d27 M $d703 M $d704 M $d705 M $d706 M -> $96 $99 $b5 $5c $ec }T
$2aef >PC $63 >S $c7 >A $de >X $86 >Y $25 >P $2aef $99 >M $2af0 $20 >M $2af1 $98 >M $2af2 $4e >M
T{ op PC S A X Y P -> $2af2 $63 $c7 $de $86 $25 }T T{ $2aef M $2af0 M $2af1 M $2af2 M $98a6 M -> $99 $20 $98 $4e $c7 }T
$fbb5 >PC $c8 >S $ec >A $79 >X $4a >Y $62 >P $fbb5 $99 >M $fbb6 $75 >M $fbb7 $06 >M $fbb8 $28 >M
T{ op PC S A X Y P -> $fbb8 $c8 $ec $79 $4a $62 }T T{ $06bf M $fbb5 M $fbb6 M $fbb7 M $fbb8 M -> $ec $99 $75 $06 $28 }T
$3ad1 >PC $25 >S $54 >A $bc >X $4b >Y $e0 >P $3ad1 $99 >M $3ad2 $81 >M $3ad3 $1a >M $3ad4 $4e >M
T{ op PC S A X Y P -> $3ad4 $25 $54 $bc $4b $e0 }T T{ $1acc M $3ad1 M $3ad2 M $3ad3 M $3ad4 M -> $54 $99 $81 $1a $4e }T
$0c29 >PC $e3 >S $cc >A $9d >X $17 >Y $e4 >P $0c29 $99 >M $0c2a $ef >M $0c2b $99 >M $0c2c $e9 >M
T{ op PC S A X Y P -> $0c2c $e3 $cc $9d $17 $e4 }T T{ $0c29 M $0c2a M $0c2b M $0c2c M $9a06 M -> $99 $ef $99 $e9 $cc }T
( 9a )
$822e >PC $d0 >S $02 >A $29 >X $1a >Y $65 >P $822e $9a >M $822f $d8 >M $8230 $b6 >M
T{ op PC S A X Y P -> $822f $29 $02 $29 $1a $65 }T T{ $822e M $822f M $8230 M -> $9a $d8 $b6 }T
$a1a6 >PC $30 >S $0c >A $44 >X $e2 >Y $a7 >P $a1a6 $9a >M $a1a7 $2f >M $a1a8 $7e >M
T{ op PC S A X Y P -> $a1a7 $44 $0c $44 $e2 $a7 }T T{ $a1a6 M $a1a7 M $a1a8 M -> $9a $2f $7e }T
$91e8 >PC $3f >S $de >A $d0 >X $51 >Y $a0 >P $91e8 $9a >M $91e9 $fc >M $91ea $63 >M
T{ op PC S A X Y P -> $91e9 $d0 $de $d0 $51 $a0 }T T{ $91e8 M $91e9 M $91ea M -> $9a $fc $63 }T
$ec7f >PC $11 >S $38 >A $91 >X $8c >Y $a0 >P $ec7f $9a >M $ec80 $bc >M $ec81 $cc >M
T{ op PC S A X Y P -> $ec80 $91 $38 $91 $8c $a0 }T T{ $ec7f M $ec80 M $ec81 M -> $9a $bc $cc }T
$5d63 >PC $ea >S $7f >A $df >X $a7 >Y $e0 >P $5d63 $9a >M $5d64 $eb >M $5d65 $72 >M
T{ op PC S A X Y P -> $5d64 $df $7f $df $a7 $e0 }T T{ $5d63 M $5d64 M $5d65 M -> $9a $eb $72 }T
$2edd >PC $84 >S $70 >A $81 >X $12 >Y $a2 >P $2edd $9a >M $2ede $1e >M $2edf $6a >M
T{ op PC S A X Y P -> $2ede $81 $70 $81 $12 $a2 }T T{ $2edd M $2ede M $2edf M -> $9a $1e $6a }T
$13fa >PC $fa >S $a6 >A $5b >X $3b >Y $64 >P $13fa $9a >M $13fb $a0 >M $13fc $29 >M
T{ op PC S A X Y P -> $13fb $5b $a6 $5b $3b $64 }T T{ $13fa M $13fb M $13fc M -> $9a $a0 $29 }T
$30bf >PC $6d >S $5c >A $3d >X $d0 >Y $21 >P $30bf $9a >M $30c0 $e2 >M $30c1 $61 >M
T{ op PC S A X Y P -> $30c0 $3d $5c $3d $d0 $21 }T T{ $30bf M $30c0 M $30c1 M -> $9a $e2 $61 }T
$44e6 >PC $98 >S $72 >A $0b >X $9d >Y $27 >P $44e6 $9a >M $44e7 $42 >M $44e8 $f9 >M
T{ op PC S A X Y P -> $44e7 $0b $72 $0b $9d $27 }T T{ $44e6 M $44e7 M $44e8 M -> $9a $42 $f9 }T
$725a >PC $06 >S $94 >A $f8 >X $46 >Y $e1 >P $725a $9a >M $725b $7c >M $725c $b6 >M
T{ op PC S A X Y P -> $725b $f8 $94 $f8 $46 $e1 }T T{ $725a M $725b M $725c M -> $9a $7c $b6 }T
$62a3 >PC $6d >S $1a >A $05 >X $4b >Y $62 >P $62a3 $9a >M $62a4 $13 >M $62a5 $4b >M
T{ op PC S A X Y P -> $62a4 $05 $1a $05 $4b $62 }T T{ $62a3 M $62a4 M $62a5 M -> $9a $13 $4b }T
$0400 >PC $43 >S $84 >A $69 >X $e8 >Y $21 >P $0400 $9a >M $0401 $aa >M $0402 $bd >M
T{ op PC S A X Y P -> $0401 $69 $84 $69 $e8 $21 }T T{ $0400 M $0401 M $0402 M -> $9a $aa $bd }T
$f552 >PC $e4 >S $98 >A $86 >X $8b >Y $a7 >P $f552 $9a >M $f553 $c0 >M $f554 $28 >M
T{ op PC S A X Y P -> $f553 $86 $98 $86 $8b $a7 }T T{ $f552 M $f553 M $f554 M -> $9a $c0 $28 }T
$f80b >PC $96 >S $29 >A $92 >X $a5 >Y $a2 >P $f80b $9a >M $f80c $b9 >M $f80d $49 >M
T{ op PC S A X Y P -> $f80c $92 $29 $92 $a5 $a2 }T T{ $f80b M $f80c M $f80d M -> $9a $b9 $49 }T
$be0c >PC $18 >S $66 >A $2a >X $25 >Y $22 >P $be0c $9a >M $be0d $98 >M $be0e $e5 >M
T{ op PC S A X Y P -> $be0d $2a $66 $2a $25 $22 }T T{ $be0c M $be0d M $be0e M -> $9a $98 $e5 }T
$6dcd >PC $f1 >S $af >A $d2 >X $17 >Y $61 >P $6dcd $9a >M $6dce $23 >M $6dcf $c3 >M
T{ op PC S A X Y P -> $6dce $d2 $af $d2 $17 $61 }T T{ $6dcd M $6dce M $6dcf M -> $9a $23 $c3 }T
( 9b )
$450b >PC $e9 >S $a4 >A $aa >X $53 >Y $e7 >P $450b $9b >M $450c $75 >M $450d $8e >M
T{ op PC S A X Y P -> $450c $e9 $a4 $aa $53 $e7 }T T{ $450b M $450c M $450d M -> $9b $75 $8e }T
$dde8 >PC $bc >S $42 >A $9e >X $2e >Y $a2 >P $dde8 $9b >M $dde9 $0b >M $ddea $85 >M
T{ op PC S A X Y P -> $dde9 $bc $42 $9e $2e $a2 }T T{ $dde8 M $dde9 M $ddea M -> $9b $0b $85 }T
$84fd >PC $33 >S $4c >A $85 >X $40 >Y $67 >P $84fd $9b >M $84fe $20 >M $84ff $0f >M
T{ op PC S A X Y P -> $84fe $33 $4c $85 $40 $67 }T T{ $84fd M $84fe M $84ff M -> $9b $20 $0f }T
$35cb >PC $f8 >S $b1 >A $37 >X $42 >Y $a2 >P $35cb $9b >M $35cc $98 >M $35cd $a5 >M
T{ op PC S A X Y P -> $35cc $f8 $b1 $37 $42 $a2 }T T{ $35cb M $35cc M $35cd M -> $9b $98 $a5 }T
$909a >PC $3b >S $68 >A $98 >X $7b >Y $a6 >P $909a $9b >M $909b $60 >M $909c $3f >M
T{ op PC S A X Y P -> $909b $3b $68 $98 $7b $a6 }T T{ $909a M $909b M $909c M -> $9b $60 $3f }T
$d0a7 >PC $16 >S $dd >A $98 >X $ca >Y $27 >P $d0a7 $9b >M $d0a8 $b5 >M $d0a9 $12 >M
T{ op PC S A X Y P -> $d0a8 $16 $dd $98 $ca $27 }T T{ $d0a7 M $d0a8 M $d0a9 M -> $9b $b5 $12 }T
$84cb >PC $1f >S $15 >A $74 >X $cd >Y $67 >P $84cb $9b >M $84cc $5f >M $84cd $4e >M
T{ op PC S A X Y P -> $84cc $1f $15 $74 $cd $67 }T T{ $84cb M $84cc M $84cd M -> $9b $5f $4e }T
$cba8 >PC $1a >S $a3 >A $89 >X $ee >Y $66 >P $cba8 $9b >M $cba9 $3c >M $cbaa $08 >M
T{ op PC S A X Y P -> $cba9 $1a $a3 $89 $ee $66 }T T{ $cba8 M $cba9 M $cbaa M -> $9b $3c $08 }T
$12b0 >PC $1e >S $cc >A $e8 >X $38 >Y $a2 >P $12b0 $9b >M $12b1 $17 >M $12b2 $a0 >M
T{ op PC S A X Y P -> $12b1 $1e $cc $e8 $38 $a2 }T T{ $12b0 M $12b1 M $12b2 M -> $9b $17 $a0 }T
$e0d5 >PC $51 >S $5c >A $fb >X $dd >Y $a7 >P $e0d5 $9b >M $e0d6 $02 >M $e0d7 $77 >M
T{ op PC S A X Y P -> $e0d6 $51 $5c $fb $dd $a7 }T T{ $e0d5 M $e0d6 M $e0d7 M -> $9b $02 $77 }T
$82bf >PC $03 >S $31 >A $b6 >X $58 >Y $25 >P $82bf $9b >M $82c0 $d8 >M $82c1 $8d >M
T{ op PC S A X Y P -> $82c0 $03 $31 $b6 $58 $25 }T T{ $82bf M $82c0 M $82c1 M -> $9b $d8 $8d }T
$9d62 >PC $96 >S $06 >A $97 >X $a9 >Y $27 >P $9d62 $9b >M $9d63 $df >M $9d64 $11 >M
T{ op PC S A X Y P -> $9d63 $96 $06 $97 $a9 $27 }T T{ $9d62 M $9d63 M $9d64 M -> $9b $df $11 }T
$fb1e >PC $5a >S $00 >A $35 >X $13 >Y $21 >P $fb1e $9b >M $fb1f $72 >M $fb20 $18 >M
T{ op PC S A X Y P -> $fb1f $5a $00 $35 $13 $21 }T T{ $fb1e M $fb1f M $fb20 M -> $9b $72 $18 }T
$0e9d >PC $8f >S $12 >A $e3 >X $aa >Y $a0 >P $0e9d $9b >M $0e9e $eb >M $0e9f $36 >M
T{ op PC S A X Y P -> $0e9e $8f $12 $e3 $aa $a0 }T T{ $0e9d M $0e9e M $0e9f M -> $9b $eb $36 }T
$5182 >PC $31 >S $34 >A $6e >X $94 >Y $a5 >P $5182 $9b >M $5183 $4d >M $5184 $4d >M
T{ op PC S A X Y P -> $5183 $31 $34 $6e $94 $a5 }T T{ $5182 M $5183 M $5184 M -> $9b $4d $4d }T
$1b26 >PC $92 >S $17 >A $e9 >X $70 >Y $25 >P $1b26 $9b >M $1b27 $66 >M $1b28 $9a >M
T{ op PC S A X Y P -> $1b27 $92 $17 $e9 $70 $25 }T T{ $1b26 M $1b27 M $1b28 M -> $9b $66 $9a }T
( 9c )
$3259 >PC $11 >S $9e >A $84 >X $63 >Y $26 >P $3259 $9c >M $325a $a4 >M $325b $2a >M $325c $b4 >M
T{ op PC S A X Y P -> $325c $11 $9e $84 $63 $26 }T T{ $2aa4 M $3259 M $325a M $325b M $325c M -> $00 $9c $a4 $2a $b4 }T
$cdad >PC $30 >S $f9 >A $88 >X $ee >Y $21 >P $cdad $9c >M $cdae $97 >M $cdaf $a6 >M $cdb0 $ed >M
T{ op PC S A X Y P -> $cdb0 $30 $f9 $88 $ee $21 }T T{ $a697 M $cdad M $cdae M $cdaf M $cdb0 M -> $00 $9c $97 $a6 $ed }T
$4be3 >PC $55 >S $a0 >A $04 >X $9e >Y $e5 >P $4be3 $9c >M $4be4 $89 >M $4be5 $d3 >M $4be6 $81 >M
T{ op PC S A X Y P -> $4be6 $55 $a0 $04 $9e $e5 }T T{ $4be3 M $4be4 M $4be5 M $4be6 M $d389 M -> $9c $89 $d3 $81 $00 }T
$bd98 >PC $94 >S $73 >A $47 >X $38 >Y $a2 >P $bd98 $9c >M $bd99 $6d >M $bd9a $cd >M $bd9b $06 >M
T{ op PC S A X Y P -> $bd9b $94 $73 $47 $38 $a2 }T T{ $bd98 M $bd99 M $bd9a M $bd9b M $cd6d M -> $9c $6d $cd $06 $00 }T
$0efd >PC $f9 >S $dd >A $b3 >X $3b >Y $a7 >P $0efd $9c >M $0efe $5b >M $0eff $0b >M $0f00 $a0 >M
T{ op PC S A X Y P -> $0f00 $f9 $dd $b3 $3b $a7 }T T{ $0b5b M $0efd M $0efe M $0eff M $0f00 M -> $00 $9c $5b $0b $a0 }T
$c4a9 >PC $1a >S $c1 >A $31 >X $7b >Y $24 >P $c4a9 $9c >M $c4aa $35 >M $c4ab $c1 >M $c4ac $ff >M
T{ op PC S A X Y P -> $c4ac $1a $c1 $31 $7b $24 }T T{ $c135 M $c4a9 M $c4aa M $c4ab M $c4ac M -> $00 $9c $35 $c1 $ff }T
$340a >PC $78 >S $e6 >A $5c >X $a7 >Y $a4 >P $340a $9c >M $340b $d4 >M $340c $c5 >M $340d $74 >M
T{ op PC S A X Y P -> $340d $78 $e6 $5c $a7 $a4 }T T{ $340a M $340b M $340c M $340d M $c5d4 M -> $9c $d4 $c5 $74 $00 }T
$aa71 >PC $b0 >S $04 >A $82 >X $c9 >Y $a7 >P $aa71 $9c >M $aa72 $0c >M $aa73 $a6 >M $aa74 $c5 >M
T{ op PC S A X Y P -> $aa74 $b0 $04 $82 $c9 $a7 }T T{ $a60c M $aa71 M $aa72 M $aa73 M $aa74 M -> $00 $9c $0c $a6 $c5 }T
$491a >PC $34 >S $34 >A $f1 >X $ca >Y $a3 >P $491a $9c >M $491b $5f >M $491c $c6 >M $491d $28 >M
T{ op PC S A X Y P -> $491d $34 $34 $f1 $ca $a3 }T T{ $491a M $491b M $491c M $491d M $c65f M -> $9c $5f $c6 $28 $00 }T
$a763 >PC $36 >S $e6 >A $23 >X $ac >Y $a6 >P $a763 $9c >M $a764 $1e >M $a765 $c3 >M $a766 $06 >M
T{ op PC S A X Y P -> $a766 $36 $e6 $23 $ac $a6 }T T{ $a763 M $a764 M $a765 M $a766 M $c31e M -> $9c $1e $c3 $06 $00 }T
$013e >PC $e0 >S $87 >A $83 >X $5a >Y $62 >P $013e $9c >M $013f $22 >M $0140 $4c >M $0141 $89 >M
T{ op PC S A X Y P -> $0141 $e0 $87 $83 $5a $62 }T T{ $013e M $013f M $0140 M $0141 M $4c22 M -> $9c $22 $4c $89 $00 }T
$0582 >PC $28 >S $98 >A $22 >X $b1 >Y $e3 >P $0582 $9c >M $0583 $6c >M $0584 $74 >M $0585 $48 >M
T{ op PC S A X Y P -> $0585 $28 $98 $22 $b1 $e3 }T T{ $0582 M $0583 M $0584 M $0585 M $746c M -> $9c $6c $74 $48 $00 }T
$4867 >PC $ec >S $17 >A $3a >X $66 >Y $21 >P $4867 $9c >M $4868 $26 >M $4869 $c0 >M $486a $6c >M
T{ op PC S A X Y P -> $486a $ec $17 $3a $66 $21 }T T{ $4867 M $4868 M $4869 M $486a M $c026 M -> $9c $26 $c0 $6c $00 }T
$02af >PC $bc >S $43 >A $d4 >X $61 >Y $25 >P $02af $9c >M $02b0 $15 >M $02b1 $6c >M $02b2 $69 >M
T{ op PC S A X Y P -> $02b2 $bc $43 $d4 $61 $25 }T T{ $02af M $02b0 M $02b1 M $02b2 M $6c15 M -> $9c $15 $6c $69 $00 }T
$432e >PC $f4 >S $43 >A $f0 >X $cd >Y $24 >P $432e $9c >M $432f $2b >M $4330 $08 >M $4331 $8c >M
T{ op PC S A X Y P -> $4331 $f4 $43 $f0 $cd $24 }T T{ $082b M $432e M $432f M $4330 M $4331 M -> $00 $9c $2b $08 $8c }T
$8b0e >PC $a5 >S $aa >A $8d >X $1f >Y $a4 >P $8b0e $9c >M $8b0f $47 >M $8b10 $b7 >M $8b11 $78 >M
T{ op PC S A X Y P -> $8b11 $a5 $aa $8d $1f $a4 }T T{ $8b0e M $8b0f M $8b10 M $8b11 M $b747 M -> $9c $47 $b7 $78 $00 }T
( 9d )
$7992 >PC $11 >S $4e >A $4f >X $78 >Y $20 >P $7992 $9d >M $7993 $ae >M $7994 $23 >M $7995 $b5 >M
T{ op PC S A X Y P -> $7995 $11 $4e $4f $78 $20 }T T{ $23fd M $7992 M $7993 M $7994 M $7995 M -> $4e $9d $ae $23 $b5 }T
$fee2 >PC $e3 >S $83 >A $ef >X $0a >Y $22 >P $fee2 $9d >M $fee3 $63 >M $fee4 $81 >M $fee5 $83 >M
T{ op PC S A X Y P -> $fee5 $e3 $83 $ef $0a $22 }T T{ $8252 M $fee2 M $fee3 M $fee4 M $fee5 M -> $83 $9d $63 $81 $83 }T
$1392 >PC $aa >S $45 >A $0a >X $23 >Y $64 >P $1392 $9d >M $1393 $b1 >M $1394 $13 >M $1395 $6e >M
T{ op PC S A X Y P -> $1395 $aa $45 $0a $23 $64 }T T{ $1392 M $1393 M $1394 M $1395 M $13bb M -> $9d $b1 $13 $6e $45 }T
$9243 >PC $8c >S $ce >A $40 >X $9f >Y $23 >P $9243 $9d >M $9244 $d0 >M $9245 $d7 >M $9246 $6a >M
T{ op PC S A X Y P -> $9246 $8c $ce $40 $9f $23 }T T{ $9243 M $9244 M $9245 M $9246 M $d810 M -> $9d $d0 $d7 $6a $ce }T
$c77d >PC $9f >S $70 >A $ec >X $70 >Y $e2 >P $c77d $9d >M $c77e $9b >M $c77f $90 >M $c780 $a3 >M
T{ op PC S A X Y P -> $c780 $9f $70 $ec $70 $e2 }T T{ $9187 M $c77d M $c77e M $c77f M $c780 M -> $70 $9d $9b $90 $a3 }T
$602c >PC $aa >S $64 >A $fd >X $52 >Y $25 >P $602c $9d >M $602d $a5 >M $602e $4d >M $602f $f8 >M
T{ op PC S A X Y P -> $602f $aa $64 $fd $52 $25 }T T{ $4ea2 M $602c M $602d M $602e M $602f M -> $64 $9d $a5 $4d $f8 }T
$83cd >PC $10 >S $4b >A $64 >X $37 >Y $21 >P $83cd $9d >M $83ce $cd >M $83cf $b8 >M $83d0 $65 >M
T{ op PC S A X Y P -> $83d0 $10 $4b $64 $37 $21 }T T{ $83cd M $83ce M $83cf M $83d0 M $b931 M -> $9d $cd $b8 $65 $4b }T
$61f7 >PC $26 >S $b7 >A $11 >X $65 >Y $25 >P $61f7 $9d >M $61f8 $83 >M $61f9 $5d >M $61fa $d7 >M
T{ op PC S A X Y P -> $61fa $26 $b7 $11 $65 $25 }T T{ $5d94 M $61f7 M $61f8 M $61f9 M $61fa M -> $b7 $9d $83 $5d $d7 }T
$4368 >PC $18 >S $22 >A $7e >X $f0 >Y $e4 >P $4368 $9d >M $4369 $64 >M $436a $8f >M $436b $12 >M
T{ op PC S A X Y P -> $436b $18 $22 $7e $f0 $e4 }T T{ $4368 M $4369 M $436a M $436b M $8fe2 M -> $9d $64 $8f $12 $22 }T
$d6ad >PC $1c >S $e6 >A $11 >X $7f >Y $63 >P $d6ad $9d >M $d6ae $d6 >M $d6af $24 >M $d6b0 $9e >M
T{ op PC S A X Y P -> $d6b0 $1c $e6 $11 $7f $63 }T T{ $24e7 M $d6ad M $d6ae M $d6af M $d6b0 M -> $e6 $9d $d6 $24 $9e }T
$d591 >PC $f3 >S $6a >A $a9 >X $0c >Y $e6 >P $d591 $9d >M $d592 $03 >M $d593 $cd >M $d594 $ed >M
T{ op PC S A X Y P -> $d594 $f3 $6a $a9 $0c $e6 }T T{ $cdac M $d591 M $d592 M $d593 M $d594 M -> $6a $9d $03 $cd $ed }T
$c0b0 >PC $f1 >S $16 >A $7d >X $9b >Y $e0 >P $c0b0 $9d >M $c0b1 $33 >M $c0b2 $1d >M $c0b3 $e3 >M
T{ op PC S A X Y P -> $c0b3 $f1 $16 $7d $9b $e0 }T T{ $1db0 M $c0b0 M $c0b1 M $c0b2 M $c0b3 M -> $16 $9d $33 $1d $e3 }T
$63ba >PC $dd >S $fd >A $89 >X $2d >Y $24 >P $63ba $9d >M $63bb $d6 >M $63bc $0f >M $63bd $1c >M
T{ op PC S A X Y P -> $63bd $dd $fd $89 $2d $24 }T T{ $105f M $63ba M $63bb M $63bc M $63bd M -> $fd $9d $d6 $0f $1c }T
$3c33 >PC $56 >S $ed >A $5a >X $90 >Y $e3 >P $3c33 $9d >M $3c34 $0d >M $3c35 $51 >M $3c36 $b9 >M
T{ op PC S A X Y P -> $3c36 $56 $ed $5a $90 $e3 }T T{ $3c33 M $3c34 M $3c35 M $3c36 M $5167 M -> $9d $0d $51 $b9 $ed }T
$091a >PC $ab >S $d5 >A $b7 >X $be >Y $21 >P $091a $9d >M $091b $27 >M $091c $a6 >M $091d $24 >M
T{ op PC S A X Y P -> $091d $ab $d5 $b7 $be $21 }T T{ $091a M $091b M $091c M $091d M $a6de M -> $9d $27 $a6 $24 $d5 }T
$ea30 >PC $09 >S $48 >A $2d >X $06 >Y $26 >P $ea30 $9d >M $ea31 $8e >M $ea32 $84 >M $ea33 $2a >M
T{ op PC S A X Y P -> $ea33 $09 $48 $2d $06 $26 }T T{ $84bb M $ea30 M $ea31 M $ea32 M $ea33 M -> $48 $9d $8e $84 $2a }T
( 9e )
$16ac >PC $14 >S $00 >A $b2 >X $0a >Y $20 >P $16ac $9e >M $16ad $ad >M $16ae $25 >M $16af $ab >M
T{ op PC S A X Y P -> $16af $14 $00 $b2 $0a $20 }T T{ $16ac M $16ad M $16ae M $16af M $265f M -> $9e $ad $25 $ab $00 }T
$f7d7 >PC $b9 >S $7f >A $d9 >X $ec >Y $e6 >P $f7d7 $9e >M $f7d8 $59 >M $f7d9 $b7 >M $f7da $b1 >M
T{ op PC S A X Y P -> $f7da $b9 $7f $d9 $ec $e6 }T T{ $b832 M $f7d7 M $f7d8 M $f7d9 M $f7da M -> $00 $9e $59 $b7 $b1 }T
$95ef >PC $6b >S $d9 >A $ff >X $28 >Y $a4 >P $95ef $9e >M $95f0 $5e >M $95f1 $93 >M $95f2 $64 >M
T{ op PC S A X Y P -> $95f2 $6b $d9 $ff $28 $a4 }T T{ $945d M $95ef M $95f0 M $95f1 M $95f2 M -> $00 $9e $5e $93 $64 }T
$1e8b >PC $38 >S $6c >A $ca >X $52 >Y $61 >P $1e8b $9e >M $1e8c $ea >M $1e8d $9a >M $1e8e $6e >M
T{ op PC S A X Y P -> $1e8e $38 $6c $ca $52 $61 }T T{ $1e8b M $1e8c M $1e8d M $1e8e M $9bb4 M -> $9e $ea $9a $6e $00 }T
$4c6e >PC $b3 >S $c2 >A $fc >X $9e >Y $21 >P $4c6e $9e >M $4c6f $06 >M $4c70 $76 >M $4c71 $6c >M
T{ op PC S A X Y P -> $4c71 $b3 $c2 $fc $9e $21 }T T{ $4c6e M $4c6f M $4c70 M $4c71 M $7702 M -> $9e $06 $76 $6c $00 }T
$bbf0 >PC $70 >S $d2 >A $64 >X $67 >Y $26 >P $bbf0 $9e >M $bbf1 $21 >M $bbf2 $e8 >M $bbf3 $fe >M
T{ op PC S A X Y P -> $bbf3 $70 $d2 $64 $67 $26 }T T{ $bbf0 M $bbf1 M $bbf2 M $bbf3 M $e885 M -> $9e $21 $e8 $fe $00 }T
$247e >PC $76 >S $07 >A $c3 >X $7e >Y $a6 >P $247e $9e >M $247f $58 >M $2480 $22 >M $2481 $9b >M
T{ op PC S A X Y P -> $2481 $76 $07 $c3 $7e $a6 }T T{ $231b M $247e M $247f M $2480 M $2481 M -> $00 $9e $58 $22 $9b }T
$1034 >PC $53 >S $aa >A $5a >X $66 >Y $27 >P $1034 $9e >M $1035 $59 >M $1036 $ba >M $1037 $e7 >M
T{ op PC S A X Y P -> $1037 $53 $aa $5a $66 $27 }T T{ $1034 M $1035 M $1036 M $1037 M $bab3 M -> $9e $59 $ba $e7 $00 }T
$005f >PC $a1 >S $48 >A $09 >X $3f >Y $26 >P $005f $9e >M $0060 $5d >M $0061 $a8 >M $0062 $d5 >M
T{ op PC S A X Y P -> $0062 $a1 $48 $09 $3f $26 }T T{ $005f M $0060 M $0061 M $0062 M $a866 M -> $9e $5d $a8 $d5 $00 }T
$a1fc >PC $02 >S $ce >A $2e >X $c3 >Y $23 >P $a1fc $9e >M $a1fd $07 >M $a1fe $bb >M $a1ff $fd >M
T{ op PC S A X Y P -> $a1ff $02 $ce $2e $c3 $23 }T T{ $a1fc M $a1fd M $a1fe M $a1ff M $bb35 M -> $9e $07 $bb $fd $00 }T
$06ed >PC $51 >S $5d >A $3a >X $21 >Y $e7 >P $06ed $9e >M $06ee $4d >M $06ef $1d >M $06f0 $e1 >M
T{ op PC S A X Y P -> $06f0 $51 $5d $3a $21 $e7 }T T{ $06ed M $06ee M $06ef M $06f0 M $1d87 M -> $9e $4d $1d $e1 $00 }T
$46b5 >PC $9e >S $d2 >A $31 >X $c5 >Y $61 >P $46b5 $9e >M $46b6 $34 >M $46b7 $d5 >M $46b8 $69 >M
T{ op PC S A X Y P -> $46b8 $9e $d2 $31 $c5 $61 }T T{ $46b5 M $46b6 M $46b7 M $46b8 M $d565 M -> $9e $34 $d5 $69 $00 }T
$477d >PC $92 >S $de >A $a9 >X $f4 >Y $26 >P $477d $9e >M $477e $52 >M $477f $2d >M $4780 $56 >M
T{ op PC S A X Y P -> $4780 $92 $de $a9 $f4 $26 }T T{ $2dfb M $477d M $477e M $477f M $4780 M -> $00 $9e $52 $2d $56 }T
$721a >PC $82 >S $b4 >A $40 >X $bd >Y $a2 >P $721a $9e >M $721b $e8 >M $721c $88 >M $721d $1d >M
T{ op PC S A X Y P -> $721d $82 $b4 $40 $bd $a2 }T T{ $721a M $721b M $721c M $721d M $8928 M -> $9e $e8 $88 $1d $00 }T
$6477 >PC $1c >S $92 >A $59 >X $67 >Y $a7 >P $6477 $9e >M $6478 $78 >M $6479 $71 >M $647a $9f >M
T{ op PC S A X Y P -> $647a $1c $92 $59 $67 $a7 }T T{ $6477 M $6478 M $6479 M $647a M $71d1 M -> $9e $78 $71 $9f $00 }T
$4d9a >PC $a7 >S $3c >A $d9 >X $fc >Y $26 >P $4d9a $9e >M $4d9b $9a >M $4d9c $81 >M $4d9d $1a >M
T{ op PC S A X Y P -> $4d9d $a7 $3c $d9 $fc $26 }T T{ $4d9a M $4d9b M $4d9c M $4d9d M $8273 M -> $9e $9a $81 $1a $00 }T
( 9f )
$a61c >PC $10 >S $01 >A $75 >X $c7 >Y $e3 >P $0037 $5e >M $a61c $9f >M $a61d $37 >M $a61e $0e >M $a62d $fb >M
T{ op PC S A X Y P -> $a62d $10 $01 $75 $c7 $e3 }T T{ $0037 M $a61c M $a61d M $a61e M $a62d M -> $5e $9f $37 $0e $fb }T
$4f8e >PC $e0 >S $da >A $ad >X $6a >Y $67 >P $0052 $0c >M $4f81 $d2 >M $4f8e $9f >M $4f8f $52 >M $4f90 $f0 >M $4f91 $cb >M
T{ op PC S A X Y P -> $4f91 $e0 $da $ad $6a $67 }T T{ $0052 M $4f81 M $4f8e M $4f8f M $4f90 M $4f91 M -> $0c $d2 $9f $52 $f0 $cb }T
$826f >PC $1f >S $76 >A $14 >X $5f >Y $61 >P $00cd $ea >M $8238 $1a >M $826f $9f >M $8270 $cd >M $8271 $c6 >M
T{ op PC S A X Y P -> $8238 $1f $76 $14 $5f $61 }T T{ $00cd M $8238 M $826f M $8270 M $8271 M -> $ea $1a $9f $cd $c6 }T
$794b >PC $7d >S $b0 >A $dc >X $b1 >Y $a0 >P $006e $7d >M $793b $a5 >M $794b $9f >M $794c $6e >M $794d $ed >M $794e $70 >M
T{ op PC S A X Y P -> $794e $7d $b0 $dc $b1 $a0 }T T{ $006e M $793b M $794b M $794c M $794d M $794e M -> $7d $a5 $9f $6e $ed $70 }T
$95ed >PC $fc >S $ae >A $da >X $7f >Y $65 >P $0054 $bc >M $9512 $2c >M $95ed $9f >M $95ee $54 >M $95ef $22 >M $95f0 $14 >M
T{ op PC S A X Y P -> $95f0 $fc $ae $da $7f $65 }T T{ $0054 M $9512 M $95ed M $95ee M $95ef M $95f0 M -> $bc $2c $9f $54 $22 $14 }T
$40ac >PC $b2 >S $7f >A $a2 >X $46 >Y $62 >P $008d $49 >M $40ac $9f >M $40ad $8d >M $40ae $43 >M $40af $5c >M $40f2 $94 >M
T{ op PC S A X Y P -> $40af $b2 $7f $a2 $46 $62 }T T{ $008d M $40ac M $40ad M $40ae M $40af M $40f2 M -> $49 $9f $8d $43 $5c $94 }T
$6867 >PC $b1 >S $6a >A $9b >X $c5 >Y $a5 >P $0022 $42 >M $681c $a3 >M $6867 $9f >M $6868 $22 >M $6869 $b2 >M
T{ op PC S A X Y P -> $681c $b1 $6a $9b $c5 $a5 }T T{ $0022 M $681c M $6867 M $6868 M $6869 M -> $42 $a3 $9f $22 $b2 }T
$e58c >PC $02 >S $8d >A $f0 >X $da >Y $e0 >P $0053 $c7 >M $e588 $71 >M $e58c $9f >M $e58d $53 >M $e58e $f9 >M
T{ op PC S A X Y P -> $e588 $02 $8d $f0 $da $e0 }T T{ $0053 M $e588 M $e58c M $e58d M $e58e M -> $c7 $71 $9f $53 $f9 }T
$674f >PC $81 >S $61 >A $0b >X $e1 >Y $21 >P $00cc $a2 >M $6724 $74 >M $674f $9f >M $6750 $cc >M $6751 $d2 >M
T{ op PC S A X Y P -> $6724 $81 $61 $0b $e1 $21 }T T{ $00cc M $6724 M $674f M $6750 M $6751 M -> $a2 $74 $9f $cc $d2 }T
$96b5 >PC $6b >S $68 >A $3d >X $9b >Y $a5 >P $0031 $4e >M $964a $14 >M $96b5 $9f >M $96b6 $31 >M $96b7 $92 >M
T{ op PC S A X Y P -> $964a $6b $68 $3d $9b $a5 }T T{ $0031 M $964a M $96b5 M $96b6 M $96b7 M -> $4e $14 $9f $31 $92 }T
$4eba >PC $5d >S $8a >A $5c >X $bb >Y $a7 >P $0082 $68 >M $4e14 $a5 >M $4eba $9f >M $4ebb $82 >M $4ebc $57 >M $4ebd $fd >M
T{ op PC S A X Y P -> $4ebd $5d $8a $5c $bb $a7 }T T{ $0082 M $4e14 M $4eba M $4ebb M $4ebc M $4ebd M -> $68 $a5 $9f $82 $57 $fd }T
$8644 >PC $e6 >S $db >A $2d >X $85 >Y $25 >P $00e8 $7f >M $85ec $72 >M $8644 $9f >M $8645 $e8 >M $8646 $a5 >M $86ec $f3 >M
T{ op PC S A X Y P -> $85ec $e6 $db $2d $85 $25 }T T{ $00e8 M $85ec M $8644 M $8645 M $8646 M $86ec M -> $7f $72 $9f $e8 $a5 $f3 }T
$072e >PC $41 >S $e8 >A $f5 >X $37 >Y $a3 >P $00a1 $e3 >M $072e $9f >M $072f $a1 >M $0730 $0e >M $073f $94 >M
T{ op PC S A X Y P -> $073f $41 $e8 $f5 $37 $a3 }T T{ $00a1 M $072e M $072f M $0730 M $073f M -> $e3 $9f $a1 $0e $94 }T
$f855 >PC $45 >S $26 >A $89 >X $c8 >Y $66 >P $009e $88 >M $f855 $9f >M $f856 $9e >M $f857 $04 >M $f858 $33 >M $f85c $93 >M
T{ op PC S A X Y P -> $f858 $45 $26 $89 $c8 $66 }T T{ $009e M $f855 M $f856 M $f857 M $f858 M $f85c M -> $88 $9f $9e $04 $33 $93 }T
$a5d4 >PC $3f >S $7e >A $f2 >X $c4 >Y $66 >P $00aa $94 >M $a560 $5b >M $a5d4 $9f >M $a5d5 $aa >M $a5d6 $89 >M $a5d7 $e3 >M
T{ op PC S A X Y P -> $a5d7 $3f $7e $f2 $c4 $66 }T T{ $00aa M $a560 M $a5d4 M $a5d5 M $a5d6 M $a5d7 M -> $94 $5b $9f $aa $89 $e3 }T
$092c >PC $69 >S $64 >A $45 >X $4f >Y $e0 >P $0050 $6a >M $08e5 $7e >M $092c $9f >M $092d $50 >M $092e $b6 >M $09e5 $45 >M
T{ op PC S A X Y P -> $08e5 $69 $64 $45 $4f $e0 }T T{ $0050 M $08e5 M $092c M $092d M $092e M $09e5 M -> $6a $7e $9f $50 $b6 $45 }T
( a0 )
$03ab >PC $d6 >S $ab >A $0c >X $a6 >Y $23 >P $03ab $a0 >M $03ac $0f >M $03ad $ad >M
T{ op PC S A X Y P -> $03ad $d6 $ab $0c $0f $21 }T T{ $03ab M $03ac M $03ad M -> $a0 $0f $ad }T
$4079 >PC $3d >S $37 >A $3e >X $d8 >Y $65 >P $4079 $a0 >M $407a $69 >M $407b $e3 >M
T{ op PC S A X Y P -> $407b $3d $37 $3e $69 $65 }T T{ $4079 M $407a M $407b M -> $a0 $69 $e3 }T
$19a7 >PC $33 >S $47 >A $f6 >X $77 >Y $27 >P $19a7 $a0 >M $19a8 $21 >M $19a9 $e0 >M
T{ op PC S A X Y P -> $19a9 $33 $47 $f6 $21 $25 }T T{ $19a7 M $19a8 M $19a9 M -> $a0 $21 $e0 }T
$a2d9 >PC $e4 >S $ed >A $1e >X $9c >Y $23 >P $a2d9 $a0 >M $a2da $24 >M $a2db $ad >M
T{ op PC S A X Y P -> $a2db $e4 $ed $1e $24 $21 }T T{ $a2d9 M $a2da M $a2db M -> $a0 $24 $ad }T
$ee68 >PC $a1 >S $ef >A $15 >X $c6 >Y $60 >P $ee68 $a0 >M $ee69 $d4 >M $ee6a $02 >M
T{ op PC S A X Y P -> $ee6a $a1 $ef $15 $d4 $e0 }T T{ $ee68 M $ee69 M $ee6a M -> $a0 $d4 $02 }T
$dc12 >PC $c0 >S $6a >A $0e >X $7a >Y $64 >P $dc12 $a0 >M $dc13 $fb >M $dc14 $b7 >M
T{ op PC S A X Y P -> $dc14 $c0 $6a $0e $fb $e4 }T T{ $dc12 M $dc13 M $dc14 M -> $a0 $fb $b7 }T
$37cd >PC $a0 >S $f2 >A $b1 >X $cd >Y $22 >P $37cd $a0 >M $37ce $35 >M $37cf $4a >M
T{ op PC S A X Y P -> $37cf $a0 $f2 $b1 $35 $20 }T T{ $37cd M $37ce M $37cf M -> $a0 $35 $4a }T
$b2be >PC $ae >S $5a >A $81 >X $a8 >Y $a5 >P $b2be $a0 >M $b2bf $3f >M $b2c0 $27 >M
T{ op PC S A X Y P -> $b2c0 $ae $5a $81 $3f $25 }T T{ $b2be M $b2bf M $b2c0 M -> $a0 $3f $27 }T
$1d4a >PC $26 >S $d4 >A $65 >X $25 >Y $66 >P $1d4a $a0 >M $1d4b $89 >M $1d4c $68 >M
T{ op PC S A X Y P -> $1d4c $26 $d4 $65 $89 $e4 }T T{ $1d4a M $1d4b M $1d4c M -> $a0 $89 $68 }T
$fec4 >PC $bf >S $3b >A $07 >X $d1 >Y $e3 >P $fec4 $a0 >M $fec5 $71 >M $fec6 $65 >M
T{ op PC S A X Y P -> $fec6 $bf $3b $07 $71 $61 }T T{ $fec4 M $fec5 M $fec6 M -> $a0 $71 $65 }T
$b23a >PC $18 >S $63 >A $77 >X $b4 >Y $20 >P $b23a $a0 >M $b23b $dc >M $b23c $2c >M
T{ op PC S A X Y P -> $b23c $18 $63 $77 $dc $a0 }T T{ $b23a M $b23b M $b23c M -> $a0 $dc $2c }T
$9a5f >PC $be >S $40 >A $69 >X $d1 >Y $61 >P $9a5f $a0 >M $9a60 $ba >M $9a61 $bd >M
T{ op PC S A X Y P -> $9a61 $be $40 $69 $ba $e1 }T T{ $9a5f M $9a60 M $9a61 M -> $a0 $ba $bd }T
$3ed1 >PC $7c >S $ec >A $fe >X $62 >Y $21 >P $3ed1 $a0 >M $3ed2 $98 >M $3ed3 $5a >M
T{ op PC S A X Y P -> $3ed3 $7c $ec $fe $98 $a1 }T T{ $3ed1 M $3ed2 M $3ed3 M -> $a0 $98 $5a }T
$3e66 >PC $72 >S $18 >A $99 >X $bc >Y $25 >P $3e66 $a0 >M $3e67 $05 >M $3e68 $9c >M
T{ op PC S A X Y P -> $3e68 $72 $18 $99 $05 $25 }T T{ $3e66 M $3e67 M $3e68 M -> $a0 $05 $9c }T
$d077 >PC $45 >S $30 >A $73 >X $f4 >Y $20 >P $d077 $a0 >M $d078 $cd >M $d079 $b9 >M
T{ op PC S A X Y P -> $d079 $45 $30 $73 $cd $a0 }T T{ $d077 M $d078 M $d079 M -> $a0 $cd $b9 }T
$18f8 >PC $e1 >S $a1 >A $e4 >X $0f >Y $63 >P $18f8 $a0 >M $18f9 $d9 >M $18fa $be >M
T{ op PC S A X Y P -> $18fa $e1 $a1 $e4 $d9 $e1 }T T{ $18f8 M $18f9 M $18fa M -> $a0 $d9 $be }T
( a1 )
$8a21 >PC $2e >S $91 >A $2a >X $f6 >Y $a2 >P $00ce $16 >M $00f8 $1f >M $00f9 $3b >M $3b1f $c4 >M $8a21 $a1 >M $8a22 $ce >M $8a23 $f5 >M
T{ op PC S A X Y P -> $8a23 $2e $c4 $2a $f6 $a0 }T T{ $00ce M $00f8 M $00f9 M $3b1f M $8a21 M $8a22 M $8a23 M -> $16 $1f $3b $c4 $a1 $ce $f5 }T
$cd0a >PC $b0 >S $97 >A $a0 >X $31 >Y $66 >P $001c $38 >M $00bc $7e >M $00bd $74 >M $747e $34 >M $cd0a $a1 >M $cd0b $1c >M $cd0c $5e >M
T{ op PC S A X Y P -> $cd0c $b0 $34 $a0 $31 $64 }T T{ $001c M $00bc M $00bd M $747e M $cd0a M $cd0b M $cd0c M -> $38 $7e $74 $34 $a1 $1c $5e }T
$dd3b >PC $47 >S $65 >A $3f >X $55 >Y $a6 >P $0081 $e7 >M $00c0 $9f >M $00c1 $92 >M $929f $74 >M $dd3b $a1 >M $dd3c $81 >M $dd3d $6a >M
T{ op PC S A X Y P -> $dd3d $47 $74 $3f $55 $24 }T T{ $0081 M $00c0 M $00c1 M $929f M $dd3b M $dd3c M $dd3d M -> $e7 $9f $92 $74 $a1 $81 $6a }T
$dcb0 >PC $98 >S $81 >A $5f >X $ae >Y $21 >P $0044 $ee >M $00a3 $94 >M $00a4 $38 >M $3894 $11 >M $dcb0 $a1 >M $dcb1 $44 >M $dcb2 $1b >M
T{ op PC S A X Y P -> $dcb2 $98 $11 $5f $ae $21 }T T{ $0044 M $00a3 M $00a4 M $3894 M $dcb0 M $dcb1 M $dcb2 M -> $ee $94 $38 $11 $a1 $44 $1b }T
$0e1b >PC $20 >S $8e >A $fd >X $e1 >Y $21 >P $0068 $24 >M $0069 $51 >M $006b $72 >M $0e1b $a1 >M $0e1c $6b >M $0e1d $25 >M $5124 $d4 >M
T{ op PC S A X Y P -> $0e1d $20 $d4 $fd $e1 $a1 }T T{ $0068 M $0069 M $006b M $0e1b M $0e1c M $0e1d M $5124 M -> $24 $51 $72 $a1 $6b $25 $d4 }T
$0e7f >PC $b2 >S $c9 >A $b4 >X $29 >Y $61 >P $008b $ab >M $008c $84 >M $00d7 $ef >M $0e7f $a1 >M $0e80 $d7 >M $0e81 $6e >M $84ab $33 >M
T{ op PC S A X Y P -> $0e81 $b2 $33 $b4 $29 $61 }T T{ $008b M $008c M $00d7 M $0e7f M $0e80 M $0e81 M $84ab M -> $ab $84 $ef $a1 $d7 $6e $33 }T
$e1be >PC $c5 >S $f6 >A $a5 >X $72 >Y $e4 >P $002b $01 >M $00d0 $63 >M $00d1 $8b >M $8b63 $a6 >M $e1be $a1 >M $e1bf $2b >M $e1c0 $26 >M
T{ op PC S A X Y P -> $e1c0 $c5 $a6 $a5 $72 $e4 }T T{ $002b M $00d0 M $00d1 M $8b63 M $e1be M $e1bf M $e1c0 M -> $01 $63 $8b $a6 $a1 $2b $26 }T
$89a3 >PC $9d >S $26 >A $54 >X $a8 >Y $60 >P $007b $2e >M $00cf $c1 >M $00d0 $fd >M $89a3 $a1 >M $89a4 $7b >M $89a5 $88 >M $fdc1 $78 >M
T{ op PC S A X Y P -> $89a5 $9d $78 $54 $a8 $60 }T T{ $007b M $00cf M $00d0 M $89a3 M $89a4 M $89a5 M $fdc1 M -> $2e $c1 $fd $a1 $7b $88 $78 }T
$cafb >PC $b1 >S $f6 >A $e8 >X $83 >Y $a6 >P $00d7 $22 >M $00d8 $db >M $00ef $e8 >M $cafb $a1 >M $cafc $ef >M $cafd $40 >M $db22 $02 >M
T{ op PC S A X Y P -> $cafd $b1 $02 $e8 $83 $24 }T T{ $00d7 M $00d8 M $00ef M $cafb M $cafc M $cafd M $db22 M -> $22 $db $e8 $a1 $ef $40 $02 }T
$3342 >PC $0e >S $7a >A $b2 >X $59 >Y $60 >P $003e $ff >M $003f $85 >M $008c $63 >M $3342 $a1 >M $3343 $8c >M $3344 $fc >M $85ff $ec >M
T{ op PC S A X Y P -> $3344 $0e $ec $b2 $59 $e0 }T T{ $003e M $003f M $008c M $3342 M $3343 M $3344 M $85ff M -> $ff $85 $63 $a1 $8c $fc $ec }T
$38b0 >PC $b2 >S $70 >A $35 >X $d4 >Y $a2 >P $009a $99 >M $00cf $e0 >M $00d0 $13 >M $13e0 $a8 >M $38b0 $a1 >M $38b1 $9a >M $38b2 $75 >M
T{ op PC S A X Y P -> $38b2 $b2 $a8 $35 $d4 $a0 }T T{ $009a M $00cf M $00d0 M $13e0 M $38b0 M $38b1 M $38b2 M -> $99 $e0 $13 $a8 $a1 $9a $75 }T
$d6ab >PC $6a >S $ee >A $02 >X $f8 >Y $e5 >P $00fa $ad >M $00fc $59 >M $00fd $d6 >M $d659 $cd >M $d6ab $a1 >M $d6ac $fa >M $d6ad $76 >M
T{ op PC S A X Y P -> $d6ad $6a $cd $02 $f8 $e5 }T T{ $00fa M $00fc M $00fd M $d659 M $d6ab M $d6ac M $d6ad M -> $ad $59 $d6 $cd $a1 $fa $76 }T
$143f >PC $a4 >S $d2 >A $d5 >X $ec >Y $e0 >P $00b1 $00 >M $00b2 $4a >M $00dc $e2 >M $143f $a1 >M $1440 $dc >M $1441 $7f >M $4a00 $b2 >M
T{ op PC S A X Y P -> $1441 $a4 $b2 $d5 $ec $e0 }T T{ $00b1 M $00b2 M $00dc M $143f M $1440 M $1441 M $4a00 M -> $00 $4a $e2 $a1 $dc $7f $b2 }T
$6c74 >PC $a3 >S $eb >A $49 >X $46 >Y $61 >P $001b $b4 >M $0064 $3d >M $0065 $82 >M $6c74 $a1 >M $6c75 $1b >M $6c76 $63 >M $823d $73 >M
T{ op PC S A X Y P -> $6c76 $a3 $73 $49 $46 $61 }T T{ $001b M $0064 M $0065 M $6c74 M $6c75 M $6c76 M $823d M -> $b4 $3d $82 $a1 $1b $63 $73 }T
$3ab9 >PC $1f >S $66 >A $cc >X $04 >Y $26 >P $00c8 $84 >M $00c9 $88 >M $00fc $27 >M $3ab9 $a1 >M $3aba $fc >M $3abb $fe >M $8884 $25 >M
T{ op PC S A X Y P -> $3abb $1f $25 $cc $04 $24 }T T{ $00c8 M $00c9 M $00fc M $3ab9 M $3aba M $3abb M $8884 M -> $84 $88 $27 $a1 $fc $fe $25 }T
$20c8 >PC $19 >S $da >A $a1 >X $f1 >Y $26 >P $0031 $c9 >M $00d2 $77 >M $00d3 $ce >M $20c8 $a1 >M $20c9 $31 >M $20ca $5c >M $ce77 $58 >M
T{ op PC S A X Y P -> $20ca $19 $58 $a1 $f1 $24 }T T{ $0031 M $00d2 M $00d3 M $20c8 M $20c9 M $20ca M $ce77 M -> $c9 $77 $ce $a1 $31 $5c $58 }T
( a2 )
$954f >PC $a5 >S $27 >A $2b >X $10 >Y $a1 >P $954f $a2 >M $9550 $ea >M $9551 $68 >M
T{ op PC S A X Y P -> $9551 $a5 $27 $ea $10 $a1 }T T{ $954f M $9550 M $9551 M -> $a2 $ea $68 }T
$f3b2 >PC $c5 >S $76 >A $3b >X $70 >Y $26 >P $f3b2 $a2 >M $f3b3 $8d >M $f3b4 $ab >M
T{ op PC S A X Y P -> $f3b4 $c5 $76 $8d $70 $a4 }T T{ $f3b2 M $f3b3 M $f3b4 M -> $a2 $8d $ab }T
$ed0c >PC $cc >S $2f >A $b2 >X $1d >Y $e5 >P $ed0c $a2 >M $ed0d $07 >M $ed0e $c8 >M
T{ op PC S A X Y P -> $ed0e $cc $2f $07 $1d $65 }T T{ $ed0c M $ed0d M $ed0e M -> $a2 $07 $c8 }T
$1976 >PC $01 >S $ba >A $af >X $d4 >Y $21 >P $1976 $a2 >M $1977 $4d >M $1978 $77 >M
T{ op PC S A X Y P -> $1978 $01 $ba $4d $d4 $21 }T T{ $1976 M $1977 M $1978 M -> $a2 $4d $77 }T
$8c16 >PC $39 >S $92 >A $d6 >X $ca >Y $e4 >P $8c16 $a2 >M $8c17 $69 >M $8c18 $3f >M
T{ op PC S A X Y P -> $8c18 $39 $92 $69 $ca $64 }T T{ $8c16 M $8c17 M $8c18 M -> $a2 $69 $3f }T
$a954 >PC $d6 >S $57 >A $35 >X $24 >Y $a5 >P $a954 $a2 >M $a955 $9a >M $a956 $73 >M
T{ op PC S A X Y P -> $a956 $d6 $57 $9a $24 $a5 }T T{ $a954 M $a955 M $a956 M -> $a2 $9a $73 }T
$e281 >PC $db >S $ab >A $98 >X $77 >Y $64 >P $e281 $a2 >M $e282 $b2 >M $e283 $42 >M
T{ op PC S A X Y P -> $e283 $db $ab $b2 $77 $e4 }T T{ $e281 M $e282 M $e283 M -> $a2 $b2 $42 }T
$c472 >PC $c6 >S $58 >A $f0 >X $f2 >Y $e0 >P $c472 $a2 >M $c473 $90 >M $c474 $14 >M
T{ op PC S A X Y P -> $c474 $c6 $58 $90 $f2 $e0 }T T{ $c472 M $c473 M $c474 M -> $a2 $90 $14 }T
$7af5 >PC $89 >S $e6 >A $af >X $01 >Y $22 >P $7af5 $a2 >M $7af6 $49 >M $7af7 $52 >M
T{ op PC S A X Y P -> $7af7 $89 $e6 $49 $01 $20 }T T{ $7af5 M $7af6 M $7af7 M -> $a2 $49 $52 }T
$8d08 >PC $95 >S $32 >A $fb >X $ca >Y $a1 >P $8d08 $a2 >M $8d09 $be >M $8d0a $3d >M
T{ op PC S A X Y P -> $8d0a $95 $32 $be $ca $a1 }T T{ $8d08 M $8d09 M $8d0a M -> $a2 $be $3d }T
$59b3 >PC $97 >S $10 >A $57 >X $00 >Y $e5 >P $59b3 $a2 >M $59b4 $f5 >M $59b5 $88 >M
T{ op PC S A X Y P -> $59b5 $97 $10 $f5 $00 $e5 }T T{ $59b3 M $59b4 M $59b5 M -> $a2 $f5 $88 }T
$d843 >PC $bc >S $09 >A $b4 >X $00 >Y $e3 >P $d843 $a2 >M $d844 $cb >M $d845 $fb >M
T{ op PC S A X Y P -> $d845 $bc $09 $cb $00 $e1 }T T{ $d843 M $d844 M $d845 M -> $a2 $cb $fb }T
$d4a7 >PC $49 >S $ae >A $54 >X $77 >Y $62 >P $d4a7 $a2 >M $d4a8 $5b >M $d4a9 $53 >M
T{ op PC S A X Y P -> $d4a9 $49 $ae $5b $77 $60 }T T{ $d4a7 M $d4a8 M $d4a9 M -> $a2 $5b $53 }T
$3b1b >PC $b4 >S $a3 >A $de >X $b7 >Y $e0 >P $3b1b $a2 >M $3b1c $16 >M $3b1d $b4 >M
T{ op PC S A X Y P -> $3b1d $b4 $a3 $16 $b7 $60 }T T{ $3b1b M $3b1c M $3b1d M -> $a2 $16 $b4 }T
$aeed >PC $7a >S $08 >A $55 >X $1e >Y $e0 >P $aeed $a2 >M $aeee $21 >M $aeef $27 >M
T{ op PC S A X Y P -> $aeef $7a $08 $21 $1e $60 }T T{ $aeed M $aeee M $aeef M -> $a2 $21 $27 }T
$0a51 >PC $8c >S $07 >A $96 >X $2e >Y $20 >P $0a51 $a2 >M $0a52 $3b >M $0a53 $66 >M
T{ op PC S A X Y P -> $0a53 $8c $07 $3b $2e $20 }T T{ $0a51 M $0a52 M $0a53 M -> $a2 $3b $66 }T
( a3 )
$4646 >PC $73 >S $7a >A $8b >X $3e >Y $20 >P $4646 $a3 >M $4647 $8c >M $4648 $0a >M
T{ op PC S A X Y P -> $4647 $73 $7a $8b $3e $20 }T T{ $4646 M $4647 M $4648 M -> $a3 $8c $0a }T
$a448 >PC $07 >S $99 >A $a0 >X $56 >Y $e3 >P $a448 $a3 >M $a449 $b9 >M $a44a $1b >M
T{ op PC S A X Y P -> $a449 $07 $99 $a0 $56 $e3 }T T{ $a448 M $a449 M $a44a M -> $a3 $b9 $1b }T
$9111 >PC $f6 >S $ee >A $70 >X $52 >Y $66 >P $9111 $a3 >M $9112 $91 >M $9113 $41 >M
T{ op PC S A X Y P -> $9112 $f6 $ee $70 $52 $66 }T T{ $9111 M $9112 M $9113 M -> $a3 $91 $41 }T
$516a >PC $39 >S $23 >A $d0 >X $fb >Y $e7 >P $516a $a3 >M $516b $7d >M $516c $b9 >M
T{ op PC S A X Y P -> $516b $39 $23 $d0 $fb $e7 }T T{ $516a M $516b M $516c M -> $a3 $7d $b9 }T
$cdbb >PC $52 >S $f7 >A $80 >X $57 >Y $a6 >P $cdbb $a3 >M $cdbc $79 >M $cdbd $54 >M
T{ op PC S A X Y P -> $cdbc $52 $f7 $80 $57 $a6 }T T{ $cdbb M $cdbc M $cdbd M -> $a3 $79 $54 }T
$f890 >PC $cc >S $43 >A $7d >X $84 >Y $a0 >P $f890 $a3 >M $f891 $49 >M $f892 $4d >M
T{ op PC S A X Y P -> $f891 $cc $43 $7d $84 $a0 }T T{ $f890 M $f891 M $f892 M -> $a3 $49 $4d }T
$9320 >PC $1b >S $16 >A $41 >X $9a >Y $62 >P $9320 $a3 >M $9321 $f8 >M $9322 $ef >M
T{ op PC S A X Y P -> $9321 $1b $16 $41 $9a $62 }T T{ $9320 M $9321 M $9322 M -> $a3 $f8 $ef }T
$506a >PC $76 >S $44 >A $1c >X $24 >Y $a2 >P $506a $a3 >M $506b $b7 >M $506c $49 >M
T{ op PC S A X Y P -> $506b $76 $44 $1c $24 $a2 }T T{ $506a M $506b M $506c M -> $a3 $b7 $49 }T
$aa36 >PC $22 >S $05 >A $7b >X $7a >Y $a2 >P $aa36 $a3 >M $aa37 $c6 >M $aa38 $dd >M
T{ op PC S A X Y P -> $aa37 $22 $05 $7b $7a $a2 }T T{ $aa36 M $aa37 M $aa38 M -> $a3 $c6 $dd }T
$6338 >PC $e8 >S $b4 >A $c1 >X $b2 >Y $20 >P $6338 $a3 >M $6339 $6d >M $633a $08 >M
T{ op PC S A X Y P -> $6339 $e8 $b4 $c1 $b2 $20 }T T{ $6338 M $6339 M $633a M -> $a3 $6d $08 }T
$2b71 >PC $e1 >S $28 >A $27 >X $c2 >Y $a3 >P $2b71 $a3 >M $2b72 $c6 >M $2b73 $62 >M
T{ op PC S A X Y P -> $2b72 $e1 $28 $27 $c2 $a3 }T T{ $2b71 M $2b72 M $2b73 M -> $a3 $c6 $62 }T
$31b6 >PC $a5 >S $b3 >A $e5 >X $2c >Y $64 >P $31b6 $a3 >M $31b7 $f6 >M $31b8 $50 >M
T{ op PC S A X Y P -> $31b7 $a5 $b3 $e5 $2c $64 }T T{ $31b6 M $31b7 M $31b8 M -> $a3 $f6 $50 }T
$98fa >PC $0a >S $49 >A $13 >X $2c >Y $a6 >P $98fa $a3 >M $98fb $55 >M $98fc $de >M
T{ op PC S A X Y P -> $98fb $0a $49 $13 $2c $a6 }T T{ $98fa M $98fb M $98fc M -> $a3 $55 $de }T
$ba0c >PC $7e >S $95 >A $ce >X $a0 >Y $e5 >P $ba0c $a3 >M $ba0d $38 >M $ba0e $ea >M
T{ op PC S A X Y P -> $ba0d $7e $95 $ce $a0 $e5 }T T{ $ba0c M $ba0d M $ba0e M -> $a3 $38 $ea }T
$42b5 >PC $05 >S $12 >A $12 >X $70 >Y $22 >P $42b5 $a3 >M $42b6 $58 >M $42b7 $87 >M
T{ op PC S A X Y P -> $42b6 $05 $12 $12 $70 $22 }T T{ $42b5 M $42b6 M $42b7 M -> $a3 $58 $87 }T
$8d25 >PC $97 >S $ca >A $72 >X $51 >Y $67 >P $8d25 $a3 >M $8d26 $a4 >M $8d27 $e1 >M
T{ op PC S A X Y P -> $8d26 $97 $ca $72 $51 $67 }T T{ $8d25 M $8d26 M $8d27 M -> $a3 $a4 $e1 }T
( a4 )
$627c >PC $9a >S $0e >A $73 >X $aa >Y $67 >P $0091 $ff >M $627c $a4 >M $627d $91 >M $627e $5b >M
T{ op PC S A X Y P -> $627e $9a $0e $73 $ff $e5 }T T{ $0091 M $627c M $627d M $627e M -> $ff $a4 $91 $5b }T
$0be0 >PC $9a >S $13 >A $e5 >X $b0 >Y $61 >P $0080 $ee >M $0be0 $a4 >M $0be1 $80 >M $0be2 $0c >M
T{ op PC S A X Y P -> $0be2 $9a $13 $e5 $ee $e1 }T T{ $0080 M $0be0 M $0be1 M $0be2 M -> $ee $a4 $80 $0c }T
$fb90 >PC $24 >S $80 >A $e3 >X $eb >Y $60 >P $002e $63 >M $fb90 $a4 >M $fb91 $2e >M $fb92 $d9 >M
T{ op PC S A X Y P -> $fb92 $24 $80 $e3 $63 $60 }T T{ $002e M $fb90 M $fb91 M $fb92 M -> $63 $a4 $2e $d9 }T
$10a9 >PC $c8 >S $b7 >A $e7 >X $de >Y $a4 >P $0078 $5c >M $10a9 $a4 >M $10aa $78 >M $10ab $e0 >M
T{ op PC S A X Y P -> $10ab $c8 $b7 $e7 $5c $24 }T T{ $0078 M $10a9 M $10aa M $10ab M -> $5c $a4 $78 $e0 }T
$1a0d >PC $38 >S $d9 >A $e0 >X $a3 >Y $a2 >P $0058 $7a >M $1a0d $a4 >M $1a0e $58 >M $1a0f $d4 >M
T{ op PC S A X Y P -> $1a0f $38 $d9 $e0 $7a $20 }T T{ $0058 M $1a0d M $1a0e M $1a0f M -> $7a $a4 $58 $d4 }T
$51a8 >PC $b2 >S $37 >A $f0 >X $f9 >Y $60 >P $0015 $62 >M $51a8 $a4 >M $51a9 $15 >M $51aa $80 >M
T{ op PC S A X Y P -> $51aa $b2 $37 $f0 $62 $60 }T T{ $0015 M $51a8 M $51a9 M $51aa M -> $62 $a4 $15 $80 }T
$00f8 >PC $29 >S $83 >A $91 >X $de >Y $a6 >P $009e $53 >M $00f8 $a4 >M $00f9 $9e >M $00fa $82 >M
T{ op PC S A X Y P -> $00fa $29 $83 $91 $53 $24 }T T{ $009e M $00f8 M $00f9 M $00fa M -> $53 $a4 $9e $82 }T
$4d53 >PC $67 >S $d1 >A $dd >X $99 >Y $e1 >P $00d4 $32 >M $4d53 $a4 >M $4d54 $d4 >M $4d55 $b4 >M
T{ op PC S A X Y P -> $4d55 $67 $d1 $dd $32 $61 }T T{ $00d4 M $4d53 M $4d54 M $4d55 M -> $32 $a4 $d4 $b4 }T
$8171 >PC $cc >S $83 >A $3f >X $1c >Y $e0 >P $00c6 $3d >M $8171 $a4 >M $8172 $c6 >M $8173 $c5 >M
T{ op PC S A X Y P -> $8173 $cc $83 $3f $3d $60 }T T{ $00c6 M $8171 M $8172 M $8173 M -> $3d $a4 $c6 $c5 }T
$630f >PC $6e >S $cd >A $c7 >X $0c >Y $24 >P $00e2 $ac >M $630f $a4 >M $6310 $e2 >M $6311 $52 >M
T{ op PC S A X Y P -> $6311 $6e $cd $c7 $ac $a4 }T T{ $00e2 M $630f M $6310 M $6311 M -> $ac $a4 $e2 $52 }T
$1fb8 >PC $46 >S $b5 >A $c7 >X $05 >Y $a2 >P $00f3 $f0 >M $1fb8 $a4 >M $1fb9 $f3 >M $1fba $8a >M
T{ op PC S A X Y P -> $1fba $46 $b5 $c7 $f0 $a0 }T T{ $00f3 M $1fb8 M $1fb9 M $1fba M -> $f0 $a4 $f3 $8a }T
$e9cc >PC $25 >S $35 >A $9f >X $6a >Y $a1 >P $004a $1d >M $e9cc $a4 >M $e9cd $4a >M $e9ce $62 >M
T{ op PC S A X Y P -> $e9ce $25 $35 $9f $1d $21 }T T{ $004a M $e9cc M $e9cd M $e9ce M -> $1d $a4 $4a $62 }T
$83b0 >PC $92 >S $2e >A $57 >X $f9 >Y $67 >P $00a6 $25 >M $83b0 $a4 >M $83b1 $a6 >M $83b2 $ea >M
T{ op PC S A X Y P -> $83b2 $92 $2e $57 $25 $65 }T T{ $00a6 M $83b0 M $83b1 M $83b2 M -> $25 $a4 $a6 $ea }T
$eb9f >PC $e9 >S $5b >A $b8 >X $67 >Y $60 >P $0088 $1a >M $eb9f $a4 >M $eba0 $88 >M $eba1 $44 >M
T{ op PC S A X Y P -> $eba1 $e9 $5b $b8 $1a $60 }T T{ $0088 M $eb9f M $eba0 M $eba1 M -> $1a $a4 $88 $44 }T
$b2ac >PC $c6 >S $06 >A $7b >X $ac >Y $26 >P $00c1 $f2 >M $b2ac $a4 >M $b2ad $c1 >M $b2ae $42 >M
T{ op PC S A X Y P -> $b2ae $c6 $06 $7b $f2 $a4 }T T{ $00c1 M $b2ac M $b2ad M $b2ae M -> $f2 $a4 $c1 $42 }T
$20ae >PC $b8 >S $08 >A $cb >X $d8 >Y $e7 >P $00c5 $a4 >M $20ae $a4 >M $20af $c5 >M $20b0 $ec >M
T{ op PC S A X Y P -> $20b0 $b8 $08 $cb $a4 $e5 }T T{ $00c5 M $20ae M $20af M $20b0 M -> $a4 $a4 $c5 $ec }T
( a5 )
$e68e >PC $8b >S $08 >A $4f >X $a9 >Y $22 >P $00eb $4b >M $e68e $a5 >M $e68f $eb >M $e690 $d8 >M
T{ op PC S A X Y P -> $e690 $8b $4b $4f $a9 $20 }T T{ $00eb M $e68e M $e68f M $e690 M -> $4b $a5 $eb $d8 }T
$3bba >PC $f4 >S $e2 >A $cd >X $a0 >Y $25 >P $0043 $e6 >M $3bba $a5 >M $3bbb $43 >M $3bbc $ab >M
T{ op PC S A X Y P -> $3bbc $f4 $e6 $cd $a0 $a5 }T T{ $0043 M $3bba M $3bbb M $3bbc M -> $e6 $a5 $43 $ab }T
$5dcc >PC $c2 >S $51 >A $a5 >X $37 >Y $a4 >P $0037 $95 >M $5dcc $a5 >M $5dcd $37 >M $5dce $43 >M
T{ op PC S A X Y P -> $5dce $c2 $95 $a5 $37 $a4 }T T{ $0037 M $5dcc M $5dcd M $5dce M -> $95 $a5 $37 $43 }T
$9ad2 >PC $a3 >S $54 >A $09 >X $06 >Y $22 >P $0013 $2b >M $9ad2 $a5 >M $9ad3 $13 >M $9ad4 $3e >M
T{ op PC S A X Y P -> $9ad4 $a3 $2b $09 $06 $20 }T T{ $0013 M $9ad2 M $9ad3 M $9ad4 M -> $2b $a5 $13 $3e }T
$56b6 >PC $fc >S $27 >A $1d >X $9c >Y $26 >P $0033 $41 >M $56b6 $a5 >M $56b7 $33 >M $56b8 $2d >M
T{ op PC S A X Y P -> $56b8 $fc $41 $1d $9c $24 }T T{ $0033 M $56b6 M $56b7 M $56b8 M -> $41 $a5 $33 $2d }T
$b164 >PC $50 >S $8e >A $06 >X $7e >Y $a0 >P $0062 $0c >M $b164 $a5 >M $b165 $62 >M $b166 $95 >M
T{ op PC S A X Y P -> $b166 $50 $0c $06 $7e $20 }T T{ $0062 M $b164 M $b165 M $b166 M -> $0c $a5 $62 $95 }T
$02a4 >PC $a3 >S $c2 >A $f0 >X $b9 >Y $66 >P $0099 $f3 >M $02a4 $a5 >M $02a5 $99 >M $02a6 $45 >M
T{ op PC S A X Y P -> $02a6 $a3 $f3 $f0 $b9 $e4 }T T{ $0099 M $02a4 M $02a5 M $02a6 M -> $f3 $a5 $99 $45 }T
$eb84 >PC $85 >S $c8 >A $d5 >X $a2 >Y $e4 >P $0072 $41 >M $eb84 $a5 >M $eb85 $72 >M $eb86 $99 >M
T{ op PC S A X Y P -> $eb86 $85 $41 $d5 $a2 $64 }T T{ $0072 M $eb84 M $eb85 M $eb86 M -> $41 $a5 $72 $99 }T
$344e >PC $a3 >S $92 >A $c2 >X $9d >Y $e3 >P $00c0 $7d >M $344e $a5 >M $344f $c0 >M $3450 $08 >M
T{ op PC S A X Y P -> $3450 $a3 $7d $c2 $9d $61 }T T{ $00c0 M $344e M $344f M $3450 M -> $7d $a5 $c0 $08 }T
$9dad >PC $51 >S $c2 >A $6b >X $f1 >Y $e4 >P $00c7 $37 >M $9dad $a5 >M $9dae $c7 >M $9daf $6b >M
T{ op PC S A X Y P -> $9daf $51 $37 $6b $f1 $64 }T T{ $00c7 M $9dad M $9dae M $9daf M -> $37 $a5 $c7 $6b }T
$de17 >PC $a3 >S $5b >A $9f >X $60 >Y $a3 >P $00b5 $28 >M $de17 $a5 >M $de18 $b5 >M $de19 $57 >M
T{ op PC S A X Y P -> $de19 $a3 $28 $9f $60 $21 }T T{ $00b5 M $de17 M $de18 M $de19 M -> $28 $a5 $b5 $57 }T
$8810 >PC $ce >S $da >A $ff >X $fc >Y $a4 >P $0007 $c6 >M $8810 $a5 >M $8811 $07 >M $8812 $ca >M
T{ op PC S A X Y P -> $8812 $ce $c6 $ff $fc $a4 }T T{ $0007 M $8810 M $8811 M $8812 M -> $c6 $a5 $07 $ca }T
$3f1d >PC $9a >S $55 >A $02 >X $0b >Y $a4 >P $00b3 $64 >M $3f1d $a5 >M $3f1e $b3 >M $3f1f $55 >M
T{ op PC S A X Y P -> $3f1f $9a $64 $02 $0b $24 }T T{ $00b3 M $3f1d M $3f1e M $3f1f M -> $64 $a5 $b3 $55 }T
$b22f >PC $67 >S $65 >A $c0 >X $8c >Y $e5 >P $00e6 $b7 >M $b22f $a5 >M $b230 $e6 >M $b231 $62 >M
T{ op PC S A X Y P -> $b231 $67 $b7 $c0 $8c $e5 }T T{ $00e6 M $b22f M $b230 M $b231 M -> $b7 $a5 $e6 $62 }T
$f0ed >PC $d8 >S $bb >A $59 >X $83 >Y $e0 >P $006d $07 >M $f0ed $a5 >M $f0ee $6d >M $f0ef $15 >M
T{ op PC S A X Y P -> $f0ef $d8 $07 $59 $83 $60 }T T{ $006d M $f0ed M $f0ee M $f0ef M -> $07 $a5 $6d $15 }T
$d928 >PC $6f >S $d0 >A $3d >X $ae >Y $66 >P $0025 $1c >M $d928 $a5 >M $d929 $25 >M $d92a $5e >M
T{ op PC S A X Y P -> $d92a $6f $1c $3d $ae $64 }T T{ $0025 M $d928 M $d929 M $d92a M -> $1c $a5 $25 $5e }T
( a6 )
$991c >PC $44 >S $be >A $64 >X $77 >Y $a7 >P $003d $dd >M $991c $a6 >M $991d $3d >M $991e $5b >M
T{ op PC S A X Y P -> $991e $44 $be $dd $77 $a5 }T T{ $003d M $991c M $991d M $991e M -> $dd $a6 $3d $5b }T
$45e2 >PC $3f >S $5f >A $d0 >X $4d >Y $26 >P $0099 $18 >M $45e2 $a6 >M $45e3 $99 >M $45e4 $d3 >M
T{ op PC S A X Y P -> $45e4 $3f $5f $18 $4d $24 }T T{ $0099 M $45e2 M $45e3 M $45e4 M -> $18 $a6 $99 $d3 }T
$b2b8 >PC $20 >S $03 >A $3e >X $73 >Y $65 >P $0057 $0a >M $b2b8 $a6 >M $b2b9 $57 >M $b2ba $a7 >M
T{ op PC S A X Y P -> $b2ba $20 $03 $0a $73 $65 }T T{ $0057 M $b2b8 M $b2b9 M $b2ba M -> $0a $a6 $57 $a7 }T
$8c37 >PC $c8 >S $b1 >A $27 >X $ed >Y $a7 >P $0070 $d7 >M $8c37 $a6 >M $8c38 $70 >M $8c39 $0c >M
T{ op PC S A X Y P -> $8c39 $c8 $b1 $d7 $ed $a5 }T T{ $0070 M $8c37 M $8c38 M $8c39 M -> $d7 $a6 $70 $0c }T
$8d6d >PC $59 >S $d9 >A $cd >X $b2 >Y $63 >P $00c0 $3f >M $8d6d $a6 >M $8d6e $c0 >M $8d6f $8f >M
T{ op PC S A X Y P -> $8d6f $59 $d9 $3f $b2 $61 }T T{ $00c0 M $8d6d M $8d6e M $8d6f M -> $3f $a6 $c0 $8f }T
$12da >PC $c9 >S $e8 >A $66 >X $9d >Y $26 >P $005e $74 >M $12da $a6 >M $12db $5e >M $12dc $26 >M
T{ op PC S A X Y P -> $12dc $c9 $e8 $74 $9d $24 }T T{ $005e M $12da M $12db M $12dc M -> $74 $a6 $5e $26 }T
$a373 >PC $73 >S $e6 >A $b3 >X $a7 >Y $62 >P $003b $4a >M $a373 $a6 >M $a374 $3b >M $a375 $30 >M
T{ op PC S A X Y P -> $a375 $73 $e6 $4a $a7 $60 }T T{ $003b M $a373 M $a374 M $a375 M -> $4a $a6 $3b $30 }T
$e329 >PC $da >S $66 >A $8c >X $1d >Y $e0 >P $0089 $c2 >M $e329 $a6 >M $e32a $89 >M $e32b $a6 >M
T{ op PC S A X Y P -> $e32b $da $66 $c2 $1d $e0 }T T{ $0089 M $e329 M $e32a M $e32b M -> $c2 $a6 $89 $a6 }T
$1011 >PC $7c >S $c2 >A $d3 >X $0a >Y $a2 >P $00b3 $94 >M $1011 $a6 >M $1012 $b3 >M $1013 $07 >M
T{ op PC S A X Y P -> $1013 $7c $c2 $94 $0a $a0 }T T{ $00b3 M $1011 M $1012 M $1013 M -> $94 $a6 $b3 $07 }T
$c072 >PC $9a >S $75 >A $ac >X $9d >Y $e0 >P $0025 $a4 >M $c072 $a6 >M $c073 $25 >M $c074 $96 >M
T{ op PC S A X Y P -> $c074 $9a $75 $a4 $9d $e0 }T T{ $0025 M $c072 M $c073 M $c074 M -> $a4 $a6 $25 $96 }T
$3fbd >PC $1b >S $f8 >A $3b >X $0e >Y $20 >P $00e9 $fc >M $3fbd $a6 >M $3fbe $e9 >M $3fbf $0c >M
T{ op PC S A X Y P -> $3fbf $1b $f8 $fc $0e $a0 }T T{ $00e9 M $3fbd M $3fbe M $3fbf M -> $fc $a6 $e9 $0c }T
$02c7 >PC $d8 >S $de >A $d8 >X $7b >Y $63 >P $00da $b0 >M $02c7 $a6 >M $02c8 $da >M $02c9 $6f >M
T{ op PC S A X Y P -> $02c9 $d8 $de $b0 $7b $e1 }T T{ $00da M $02c7 M $02c8 M $02c9 M -> $b0 $a6 $da $6f }T
$41c0 >PC $e9 >S $3e >A $e6 >X $07 >Y $e2 >P $00f7 $37 >M $41c0 $a6 >M $41c1 $f7 >M $41c2 $fd >M
T{ op PC S A X Y P -> $41c2 $e9 $3e $37 $07 $60 }T T{ $00f7 M $41c0 M $41c1 M $41c2 M -> $37 $a6 $f7 $fd }T
$c324 >PC $27 >S $e5 >A $2b >X $7b >Y $60 >P $008a $89 >M $c324 $a6 >M $c325 $8a >M $c326 $c1 >M
T{ op PC S A X Y P -> $c326 $27 $e5 $89 $7b $e0 }T T{ $008a M $c324 M $c325 M $c326 M -> $89 $a6 $8a $c1 }T
$498c >PC $9c >S $d7 >A $61 >X $43 >Y $62 >P $0021 $66 >M $498c $a6 >M $498d $21 >M $498e $8b >M
T{ op PC S A X Y P -> $498e $9c $d7 $66 $43 $60 }T T{ $0021 M $498c M $498d M $498e M -> $66 $a6 $21 $8b }T
$dada >PC $0a >S $2b >A $f8 >X $c8 >Y $a0 >P $009e $1d >M $dada $a6 >M $dadb $9e >M $dadc $53 >M
T{ op PC S A X Y P -> $dadc $0a $2b $1d $c8 $20 }T T{ $009e M $dada M $dadb M $dadc M -> $1d $a6 $9e $53 }T
( a7 )
$d181 >PC $88 >S $93 >A $51 >X $67 >Y $61 >P $0057 $3e >M $d181 $a7 >M $d182 $57 >M $d183 $01 >M
T{ op PC S A X Y P -> $d183 $88 $93 $51 $67 $61 }T T{ $0057 M $d181 M $d182 M $d183 M -> $3e $a7 $57 $01 }T
$c962 >PC $db >S $35 >A $b0 >X $86 >Y $e5 >P $0084 $0e >M $c962 $a7 >M $c963 $84 >M $c964 $e7 >M
T{ op PC S A X Y P -> $c964 $db $35 $b0 $86 $e5 }T T{ $0084 M $c962 M $c963 M $c964 M -> $0e $a7 $84 $e7 }T
$b355 >PC $d6 >S $22 >A $4a >X $35 >Y $a3 >P $0044 $23 >M $b355 $a7 >M $b356 $44 >M $b357 $69 >M
T{ op PC S A X Y P -> $b357 $d6 $22 $4a $35 $a3 }T T{ $0044 M $b355 M $b356 M $b357 M -> $27 $a7 $44 $69 }T
$798e >PC $08 >S $34 >A $15 >X $85 >Y $20 >P $006c $d1 >M $798e $a7 >M $798f $6c >M $7990 $cd >M
T{ op PC S A X Y P -> $7990 $08 $34 $15 $85 $20 }T T{ $006c M $798e M $798f M $7990 M -> $d5 $a7 $6c $cd }T
$8651 >PC $b1 >S $5b >A $74 >X $e0 >Y $e4 >P $0041 $c4 >M $8651 $a7 >M $8652 $41 >M $8653 $82 >M
T{ op PC S A X Y P -> $8653 $b1 $5b $74 $e0 $e4 }T T{ $0041 M $8651 M $8652 M $8653 M -> $c4 $a7 $41 $82 }T
$71b5 >PC $dc >S $08 >A $d4 >X $61 >Y $a6 >P $0041 $f7 >M $71b5 $a7 >M $71b6 $41 >M $71b7 $ef >M
T{ op PC S A X Y P -> $71b7 $dc $08 $d4 $61 $a6 }T T{ $0041 M $71b5 M $71b6 M $71b7 M -> $f7 $a7 $41 $ef }T
$6531 >PC $96 >S $4c >A $15 >X $31 >Y $a1 >P $0087 $fe >M $6531 $a7 >M $6532 $87 >M $6533 $e0 >M
T{ op PC S A X Y P -> $6533 $96 $4c $15 $31 $a1 }T T{ $0087 M $6531 M $6532 M $6533 M -> $fe $a7 $87 $e0 }T
$82ac >PC $dc >S $f6 >A $2b >X $1d >Y $67 >P $00d1 $ff >M $82ac $a7 >M $82ad $d1 >M $82ae $44 >M
T{ op PC S A X Y P -> $82ae $dc $f6 $2b $1d $67 }T T{ $00d1 M $82ac M $82ad M $82ae M -> $ff $a7 $d1 $44 }T
$c49d >PC $73 >S $97 >A $aa >X $9e >Y $a7 >P $0026 $c1 >M $c49d $a7 >M $c49e $26 >M $c49f $a0 >M
T{ op PC S A X Y P -> $c49f $73 $97 $aa $9e $a7 }T T{ $0026 M $c49d M $c49e M $c49f M -> $c5 $a7 $26 $a0 }T
$ded2 >PC $5b >S $5b >A $68 >X $49 >Y $e1 >P $00ca $4c >M $ded2 $a7 >M $ded3 $ca >M $ded4 $38 >M
T{ op PC S A X Y P -> $ded4 $5b $5b $68 $49 $e1 }T T{ $00ca M $ded2 M $ded3 M $ded4 M -> $4c $a7 $ca $38 }T
$2262 >PC $7c >S $08 >A $3d >X $2f >Y $e4 >P $00e6 $a8 >M $2262 $a7 >M $2263 $e6 >M $2264 $c6 >M
T{ op PC S A X Y P -> $2264 $7c $08 $3d $2f $e4 }T T{ $00e6 M $2262 M $2263 M $2264 M -> $ac $a7 $e6 $c6 }T
$820f >PC $34 >S $ab >A $2a >X $7f >Y $67 >P $000e $2a >M $820f $a7 >M $8210 $0e >M $8211 $be >M
T{ op PC S A X Y P -> $8211 $34 $ab $2a $7f $67 }T T{ $000e M $820f M $8210 M $8211 M -> $2e $a7 $0e $be }T
$909d >PC $82 >S $97 >A $33 >X $08 >Y $e2 >P $00bb $b7 >M $909d $a7 >M $909e $bb >M $909f $0a >M
T{ op PC S A X Y P -> $909f $82 $97 $33 $08 $e2 }T T{ $00bb M $909d M $909e M $909f M -> $b7 $a7 $bb $0a }T
$11a9 >PC $94 >S $72 >A $40 >X $93 >Y $64 >P $0028 $0a >M $11a9 $a7 >M $11aa $28 >M $11ab $4a >M
T{ op PC S A X Y P -> $11ab $94 $72 $40 $93 $64 }T T{ $0028 M $11a9 M $11aa M $11ab M -> $0e $a7 $28 $4a }T
$2ca8 >PC $94 >S $22 >A $dc >X $9c >Y $a2 >P $0098 $32 >M $2ca8 $a7 >M $2ca9 $98 >M $2caa $87 >M
T{ op PC S A X Y P -> $2caa $94 $22 $dc $9c $a2 }T T{ $0098 M $2ca8 M $2ca9 M $2caa M -> $36 $a7 $98 $87 }T
$8da8 >PC $ad >S $27 >A $46 >X $f6 >Y $e1 >P $00ce $f9 >M $8da8 $a7 >M $8da9 $ce >M $8daa $06 >M
T{ op PC S A X Y P -> $8daa $ad $27 $46 $f6 $e1 }T T{ $00ce M $8da8 M $8da9 M $8daa M -> $fd $a7 $ce $06 }T
( a8 )
$cf1a >PC $12 >S $ef >A $f9 >X $73 >Y $26 >P $cf1a $a8 >M $cf1b $b9 >M $cf1c $e7 >M
T{ op PC S A X Y P -> $cf1b $12 $ef $f9 $ef $a4 }T T{ $cf1a M $cf1b M $cf1c M -> $a8 $b9 $e7 }T
$66b2 >PC $0a >S $51 >A $fc >X $9d >Y $a7 >P $66b2 $a8 >M $66b3 $60 >M $66b4 $08 >M
T{ op PC S A X Y P -> $66b3 $0a $51 $fc $51 $25 }T T{ $66b2 M $66b3 M $66b4 M -> $a8 $60 $08 }T
$40e0 >PC $cf >S $f2 >A $d3 >X $80 >Y $a2 >P $40e0 $a8 >M $40e1 $aa >M $40e2 $18 >M
T{ op PC S A X Y P -> $40e1 $cf $f2 $d3 $f2 $a0 }T T{ $40e0 M $40e1 M $40e2 M -> $a8 $aa $18 }T
$55f2 >PC $2c >S $27 >A $e1 >X $6a >Y $a3 >P $55f2 $a8 >M $55f3 $4e >M $55f4 $90 >M
T{ op PC S A X Y P -> $55f3 $2c $27 $e1 $27 $21 }T T{ $55f2 M $55f3 M $55f4 M -> $a8 $4e $90 }T
$9099 >PC $50 >S $0c >A $fa >X $d3 >Y $e6 >P $9099 $a8 >M $909a $38 >M $909b $b4 >M
T{ op PC S A X Y P -> $909a $50 $0c $fa $0c $64 }T T{ $9099 M $909a M $909b M -> $a8 $38 $b4 }T
$1e43 >PC $d1 >S $a4 >A $f5 >X $33 >Y $65 >P $1e43 $a8 >M $1e44 $d2 >M $1e45 $44 >M
T{ op PC S A X Y P -> $1e44 $d1 $a4 $f5 $a4 $e5 }T T{ $1e43 M $1e44 M $1e45 M -> $a8 $d2 $44 }T
$f9dd >PC $41 >S $df >A $ac >X $25 >Y $a4 >P $f9dd $a8 >M $f9de $db >M $f9df $02 >M
T{ op PC S A X Y P -> $f9de $41 $df $ac $df $a4 }T T{ $f9dd M $f9de M $f9df M -> $a8 $db $02 }T
$09f6 >PC $3f >S $88 >A $67 >X $40 >Y $20 >P $09f6 $a8 >M $09f7 $79 >M $09f8 $b7 >M
T{ op PC S A X Y P -> $09f7 $3f $88 $67 $88 $a0 }T T{ $09f6 M $09f7 M $09f8 M -> $a8 $79 $b7 }T
$838e >PC $ac >S $bd >A $74 >X $4c >Y $23 >P $838e $a8 >M $838f $9d >M $8390 $d7 >M
T{ op PC S A X Y P -> $838f $ac $bd $74 $bd $a1 }T T{ $838e M $838f M $8390 M -> $a8 $9d $d7 }T
$ece6 >PC $a0 >S $81 >A $c8 >X $74 >Y $a6 >P $ece6 $a8 >M $ece7 $e5 >M $ece8 $21 >M
T{ op PC S A X Y P -> $ece7 $a0 $81 $c8 $81 $a4 }T T{ $ece6 M $ece7 M $ece8 M -> $a8 $e5 $21 }T
$47df >PC $61 >S $1e >A $38 >X $8c >Y $20 >P $47df $a8 >M $47e0 $30 >M $47e1 $73 >M
T{ op PC S A X Y P -> $47e0 $61 $1e $38 $1e $20 }T T{ $47df M $47e0 M $47e1 M -> $a8 $30 $73 }T
$ec3a >PC $0d >S $61 >A $3c >X $c8 >Y $a4 >P $ec3a $a8 >M $ec3b $28 >M $ec3c $ea >M
T{ op PC S A X Y P -> $ec3b $0d $61 $3c $61 $24 }T T{ $ec3a M $ec3b M $ec3c M -> $a8 $28 $ea }T
$ee6e >PC $39 >S $71 >A $68 >X $f1 >Y $24 >P $ee6e $a8 >M $ee6f $ff >M $ee70 $6d >M
T{ op PC S A X Y P -> $ee6f $39 $71 $68 $71 $24 }T T{ $ee6e M $ee6f M $ee70 M -> $a8 $ff $6d }T
$1890 >PC $b2 >S $17 >A $e8 >X $aa >Y $66 >P $1890 $a8 >M $1891 $fb >M $1892 $75 >M
T{ op PC S A X Y P -> $1891 $b2 $17 $e8 $17 $64 }T T{ $1890 M $1891 M $1892 M -> $a8 $fb $75 }T
$f93e >PC $04 >S $40 >A $54 >X $26 >Y $66 >P $f93e $a8 >M $f93f $a0 >M $f940 $ea >M
T{ op PC S A X Y P -> $f93f $04 $40 $54 $40 $64 }T T{ $f93e M $f93f M $f940 M -> $a8 $a0 $ea }T
$ac93 >PC $05 >S $7f >A $81 >X $5c >Y $63 >P $ac93 $a8 >M $ac94 $98 >M $ac95 $54 >M
T{ op PC S A X Y P -> $ac94 $05 $7f $81 $7f $61 }T T{ $ac93 M $ac94 M $ac95 M -> $a8 $98 $54 }T
( a9 )
$6e17 >PC $df >S $af >A $35 >X $df >Y $20 >P $6e17 $a9 >M $6e18 $47 >M $6e19 $e1 >M
T{ op PC S A X Y P -> $6e19 $df $47 $35 $df $20 }T T{ $6e17 M $6e18 M $6e19 M -> $a9 $47 $e1 }T
$bcad >PC $b5 >S $37 >A $c4 >X $2a >Y $67 >P $bcad $a9 >M $bcae $e2 >M $bcaf $36 >M
T{ op PC S A X Y P -> $bcaf $b5 $e2 $c4 $2a $e5 }T T{ $bcad M $bcae M $bcaf M -> $a9 $e2 $36 }T
$69ee >PC $23 >S $39 >A $81 >X $12 >Y $e3 >P $69ee $a9 >M $69ef $cb >M $69f0 $75 >M
T{ op PC S A X Y P -> $69f0 $23 $cb $81 $12 $e1 }T T{ $69ee M $69ef M $69f0 M -> $a9 $cb $75 }T
$aef8 >PC $61 >S $3b >A $6d >X $1d >Y $a3 >P $aef8 $a9 >M $aef9 $e3 >M $aefa $d0 >M
T{ op PC S A X Y P -> $aefa $61 $e3 $6d $1d $a1 }T T{ $aef8 M $aef9 M $aefa M -> $a9 $e3 $d0 }T
$7331 >PC $7b >S $fe >A $44 >X $33 >Y $25 >P $7331 $a9 >M $7332 $6d >M $7333 $0d >M
T{ op PC S A X Y P -> $7333 $7b $6d $44 $33 $25 }T T{ $7331 M $7332 M $7333 M -> $a9 $6d $0d }T
$7bdf >PC $50 >S $d5 >A $f3 >X $05 >Y $60 >P $7bdf $a9 >M $7be0 $bb >M $7be1 $9b >M
T{ op PC S A X Y P -> $7be1 $50 $bb $f3 $05 $e0 }T T{ $7bdf M $7be0 M $7be1 M -> $a9 $bb $9b }T
$6e82 >PC $15 >S $22 >A $00 >X $cc >Y $26 >P $6e82 $a9 >M $6e83 $97 >M $6e84 $da >M
T{ op PC S A X Y P -> $6e84 $15 $97 $00 $cc $a4 }T T{ $6e82 M $6e83 M $6e84 M -> $a9 $97 $da }T
$b6a5 >PC $4d >S $41 >A $2e >X $11 >Y $67 >P $b6a5 $a9 >M $b6a6 $45 >M $b6a7 $ab >M
T{ op PC S A X Y P -> $b6a7 $4d $45 $2e $11 $65 }T T{ $b6a5 M $b6a6 M $b6a7 M -> $a9 $45 $ab }T
$1579 >PC $7b >S $99 >A $78 >X $88 >Y $e6 >P $1579 $a9 >M $157a $3f >M $157b $01 >M
T{ op PC S A X Y P -> $157b $7b $3f $78 $88 $64 }T T{ $1579 M $157a M $157b M -> $a9 $3f $01 }T
$f77f >PC $32 >S $a9 >A $75 >X $63 >Y $26 >P $f77f $a9 >M $f780 $f4 >M $f781 $5c >M
T{ op PC S A X Y P -> $f781 $32 $f4 $75 $63 $a4 }T T{ $f77f M $f780 M $f781 M -> $a9 $f4 $5c }T
$1b21 >PC $7e >S $85 >A $46 >X $f4 >Y $26 >P $1b21 $a9 >M $1b22 $b8 >M $1b23 $85 >M
T{ op PC S A X Y P -> $1b23 $7e $b8 $46 $f4 $a4 }T T{ $1b21 M $1b22 M $1b23 M -> $a9 $b8 $85 }T
$2b23 >PC $ee >S $8e >A $30 >X $51 >Y $61 >P $2b23 $a9 >M $2b24 $4f >M $2b25 $30 >M
T{ op PC S A X Y P -> $2b25 $ee $4f $30 $51 $61 }T T{ $2b23 M $2b24 M $2b25 M -> $a9 $4f $30 }T
$2e89 >PC $01 >S $fd >A $98 >X $6b >Y $25 >P $2e89 $a9 >M $2e8a $5a >M $2e8b $9c >M
T{ op PC S A X Y P -> $2e8b $01 $5a $98 $6b $25 }T T{ $2e89 M $2e8a M $2e8b M -> $a9 $5a $9c }T
$8968 >PC $ba >S $34 >A $b3 >X $07 >Y $a1 >P $8968 $a9 >M $8969 $54 >M $896a $3a >M
T{ op PC S A X Y P -> $896a $ba $54 $b3 $07 $21 }T T{ $8968 M $8969 M $896a M -> $a9 $54 $3a }T
$f2b6 >PC $57 >S $59 >A $39 >X $dc >Y $20 >P $f2b6 $a9 >M $f2b7 $e0 >M $f2b8 $1b >M
T{ op PC S A X Y P -> $f2b8 $57 $e0 $39 $dc $a0 }T T{ $f2b6 M $f2b7 M $f2b8 M -> $a9 $e0 $1b }T
$745e >PC $8e >S $3e >A $76 >X $85 >Y $23 >P $745e $a9 >M $745f $9d >M $7460 $97 >M
T{ op PC S A X Y P -> $7460 $8e $9d $76 $85 $a1 }T T{ $745e M $745f M $7460 M -> $a9 $9d $97 }T
( aa )
$c9d7 >PC $30 >S $c2 >A $3a >X $65 >Y $23 >P $c9d7 $aa >M $c9d8 $74 >M $c9d9 $e5 >M
T{ op PC S A X Y P -> $c9d8 $30 $c2 $c2 $65 $a1 }T T{ $c9d7 M $c9d8 M $c9d9 M -> $aa $74 $e5 }T
$fe93 >PC $05 >S $99 >A $43 >X $91 >Y $23 >P $fe93 $aa >M $fe94 $9c >M $fe95 $94 >M
T{ op PC S A X Y P -> $fe94 $05 $99 $99 $91 $a1 }T T{ $fe93 M $fe94 M $fe95 M -> $aa $9c $94 }T
$4653 >PC $18 >S $1d >A $12 >X $bf >Y $64 >P $4653 $aa >M $4654 $45 >M $4655 $a0 >M
T{ op PC S A X Y P -> $4654 $18 $1d $1d $bf $64 }T T{ $4653 M $4654 M $4655 M -> $aa $45 $a0 }T
$9e63 >PC $37 >S $6e >A $f8 >X $34 >Y $67 >P $9e63 $aa >M $9e64 $fe >M $9e65 $40 >M
T{ op PC S A X Y P -> $9e64 $37 $6e $6e $34 $65 }T T{ $9e63 M $9e64 M $9e65 M -> $aa $fe $40 }T
$66ee >PC $01 >S $3d >A $b3 >X $7b >Y $a4 >P $66ee $aa >M $66ef $88 >M $66f0 $de >M
T{ op PC S A X Y P -> $66ef $01 $3d $3d $7b $24 }T T{ $66ee M $66ef M $66f0 M -> $aa $88 $de }T
$a97b >PC $ce >S $f9 >A $cb >X $28 >Y $a0 >P $a97b $aa >M $a97c $0d >M $a97d $63 >M
T{ op PC S A X Y P -> $a97c $ce $f9 $f9 $28 $a0 }T T{ $a97b M $a97c M $a97d M -> $aa $0d $63 }T
$7c19 >PC $e9 >S $c8 >A $28 >X $e6 >Y $23 >P $7c19 $aa >M $7c1a $9d >M $7c1b $18 >M
T{ op PC S A X Y P -> $7c1a $e9 $c8 $c8 $e6 $a1 }T T{ $7c19 M $7c1a M $7c1b M -> $aa $9d $18 }T
$5160 >PC $8b >S $95 >A $6c >X $4f >Y $e2 >P $5160 $aa >M $5161 $1c >M $5162 $9c >M
T{ op PC S A X Y P -> $5161 $8b $95 $95 $4f $e0 }T T{ $5160 M $5161 M $5162 M -> $aa $1c $9c }T
$c97d >PC $3f >S $8c >A $31 >X $e4 >Y $e2 >P $c97d $aa >M $c97e $c9 >M $c97f $40 >M
T{ op PC S A X Y P -> $c97e $3f $8c $8c $e4 $e0 }T T{ $c97d M $c97e M $c97f M -> $aa $c9 $40 }T
$3fc2 >PC $73 >S $71 >A $e7 >X $97 >Y $66 >P $3fc2 $aa >M $3fc3 $8c >M $3fc4 $ab >M
T{ op PC S A X Y P -> $3fc3 $73 $71 $71 $97 $64 }T T{ $3fc2 M $3fc3 M $3fc4 M -> $aa $8c $ab }T
$8081 >PC $47 >S $73 >A $bf >X $d1 >Y $26 >P $8081 $aa >M $8082 $a1 >M $8083 $46 >M
T{ op PC S A X Y P -> $8082 $47 $73 $73 $d1 $24 }T T{ $8081 M $8082 M $8083 M -> $aa $a1 $46 }T
$c66a >PC $b9 >S $d0 >A $7b >X $11 >Y $a2 >P $c66a $aa >M $c66b $24 >M $c66c $2e >M
T{ op PC S A X Y P -> $c66b $b9 $d0 $d0 $11 $a0 }T T{ $c66a M $c66b M $c66c M -> $aa $24 $2e }T
$26b5 >PC $7d >S $9a >A $1d >X $56 >Y $e5 >P $26b5 $aa >M $26b6 $11 >M $26b7 $2e >M
T{ op PC S A X Y P -> $26b6 $7d $9a $9a $56 $e5 }T T{ $26b5 M $26b6 M $26b7 M -> $aa $11 $2e }T
$4a84 >PC $25 >S $34 >A $19 >X $85 >Y $67 >P $4a84 $aa >M $4a85 $19 >M $4a86 $ec >M
T{ op PC S A X Y P -> $4a85 $25 $34 $34 $85 $65 }T T{ $4a84 M $4a85 M $4a86 M -> $aa $19 $ec }T
$5155 >PC $27 >S $b8 >A $4b >X $2d >Y $26 >P $5155 $aa >M $5156 $83 >M $5157 $22 >M
T{ op PC S A X Y P -> $5156 $27 $b8 $b8 $2d $a4 }T T{ $5155 M $5156 M $5157 M -> $aa $83 $22 }T
$c472 >PC $30 >S $f8 >A $48 >X $89 >Y $65 >P $c472 $aa >M $c473 $07 >M $c474 $8b >M
T{ op PC S A X Y P -> $c473 $30 $f8 $f8 $89 $e5 }T T{ $c472 M $c473 M $c474 M -> $aa $07 $8b }T
( ab )
$f9d3 >PC $e1 >S $23 >A $87 >X $cf >Y $22 >P $f9d3 $ab >M $f9d4 $88 >M $f9d5 $0b >M
T{ op PC S A X Y P -> $f9d4 $e1 $23 $87 $cf $22 }T T{ $f9d3 M $f9d4 M $f9d5 M -> $ab $88 $0b }T
$e280 >PC $96 >S $c5 >A $96 >X $5b >Y $a7 >P $e280 $ab >M $e281 $3e >M $e282 $ab >M
T{ op PC S A X Y P -> $e281 $96 $c5 $96 $5b $a7 }T T{ $e280 M $e281 M $e282 M -> $ab $3e $ab }T
$29dc >PC $18 >S $21 >A $ff >X $7c >Y $64 >P $29dc $ab >M $29dd $a6 >M $29de $51 >M
T{ op PC S A X Y P -> $29dd $18 $21 $ff $7c $64 }T T{ $29dc M $29dd M $29de M -> $ab $a6 $51 }T
$1118 >PC $3a >S $08 >A $3a >X $e8 >Y $22 >P $1118 $ab >M $1119 $e2 >M $111a $df >M
T{ op PC S A X Y P -> $1119 $3a $08 $3a $e8 $22 }T T{ $1118 M $1119 M $111a M -> $ab $e2 $df }T
$e86a >PC $3f >S $e6 >A $26 >X $10 >Y $e1 >P $e86a $ab >M $e86b $f8 >M $e86c $7f >M
T{ op PC S A X Y P -> $e86b $3f $e6 $26 $10 $e1 }T T{ $e86a M $e86b M $e86c M -> $ab $f8 $7f }T
$f15d >PC $39 >S $7e >A $ab >X $70 >Y $60 >P $f15d $ab >M $f15e $41 >M $f15f $e1 >M
T{ op PC S A X Y P -> $f15e $39 $7e $ab $70 $60 }T T{ $f15d M $f15e M $f15f M -> $ab $41 $e1 }T
$eff5 >PC $d3 >S $ff >A $75 >X $35 >Y $a6 >P $eff5 $ab >M $eff6 $a1 >M $eff7 $10 >M
T{ op PC S A X Y P -> $eff6 $d3 $ff $75 $35 $a6 }T T{ $eff5 M $eff6 M $eff7 M -> $ab $a1 $10 }T
$93ae >PC $de >S $88 >A $76 >X $e4 >Y $e6 >P $93ae $ab >M $93af $b4 >M $93b0 $32 >M
T{ op PC S A X Y P -> $93af $de $88 $76 $e4 $e6 }T T{ $93ae M $93af M $93b0 M -> $ab $b4 $32 }T
$738f >PC $fa >S $fe >A $75 >X $22 >Y $a2 >P $738f $ab >M $7390 $d5 >M $7391 $dc >M
T{ op PC S A X Y P -> $7390 $fa $fe $75 $22 $a2 }T T{ $738f M $7390 M $7391 M -> $ab $d5 $dc }T
$11f9 >PC $ef >S $c6 >A $7f >X $45 >Y $25 >P $11f9 $ab >M $11fa $e2 >M $11fb $ae >M
T{ op PC S A X Y P -> $11fa $ef $c6 $7f $45 $25 }T T{ $11f9 M $11fa M $11fb M -> $ab $e2 $ae }T
$0b02 >PC $c6 >S $90 >A $37 >X $e4 >Y $e5 >P $0b02 $ab >M $0b03 $97 >M $0b04 $6a >M
T{ op PC S A X Y P -> $0b03 $c6 $90 $37 $e4 $e5 }T T{ $0b02 M $0b03 M $0b04 M -> $ab $97 $6a }T
$4372 >PC $24 >S $72 >A $03 >X $5f >Y $e2 >P $4372 $ab >M $4373 $13 >M $4374 $ae >M
T{ op PC S A X Y P -> $4373 $24 $72 $03 $5f $e2 }T T{ $4372 M $4373 M $4374 M -> $ab $13 $ae }T
$acda >PC $b6 >S $47 >A $7e >X $ae >Y $a7 >P $acda $ab >M $acdb $7c >M $acdc $e8 >M
T{ op PC S A X Y P -> $acdb $b6 $47 $7e $ae $a7 }T T{ $acda M $acdb M $acdc M -> $ab $7c $e8 }T
$7636 >PC $4f >S $cb >A $fd >X $86 >Y $e2 >P $7636 $ab >M $7637 $82 >M $7638 $4b >M
T{ op PC S A X Y P -> $7637 $4f $cb $fd $86 $e2 }T T{ $7636 M $7637 M $7638 M -> $ab $82 $4b }T
$b17a >PC $0d >S $ca >A $68 >X $b6 >Y $a4 >P $b17a $ab >M $b17b $d6 >M $b17c $12 >M
T{ op PC S A X Y P -> $b17b $0d $ca $68 $b6 $a4 }T T{ $b17a M $b17b M $b17c M -> $ab $d6 $12 }T
$e372 >PC $d2 >S $82 >A $68 >X $4d >Y $63 >P $e372 $ab >M $e373 $a5 >M $e374 $0a >M
T{ op PC S A X Y P -> $e373 $d2 $82 $68 $4d $63 }T T{ $e372 M $e373 M $e374 M -> $ab $a5 $0a }T
( ac )
$8085 >PC $2f >S $b8 >A $3d >X $86 >Y $21 >P $0058 $dd >M $8085 $ac >M $8086 $58 >M $8087 $00 >M $8088 $5f >M
T{ op PC S A X Y P -> $8088 $2f $b8 $3d $dd $a1 }T T{ $0058 M $8085 M $8086 M $8087 M $8088 M -> $dd $ac $58 $00 $5f }T
$da56 >PC $ca >S $6e >A $aa >X $82 >Y $61 >P $2ad7 $7e >M $da56 $ac >M $da57 $d7 >M $da58 $2a >M $da59 $76 >M
T{ op PC S A X Y P -> $da59 $ca $6e $aa $7e $61 }T T{ $2ad7 M $da56 M $da57 M $da58 M $da59 M -> $7e $ac $d7 $2a $76 }T
$2b1f >PC $d3 >S $ac >A $59 >X $31 >Y $67 >P $2b1f $ac >M $2b20 $da >M $2b21 $68 >M $2b22 $b3 >M $68da $6c >M
T{ op PC S A X Y P -> $2b22 $d3 $ac $59 $6c $65 }T T{ $2b1f M $2b20 M $2b21 M $2b22 M $68da M -> $ac $da $68 $b3 $6c }T
$2b2d >PC $4f >S $86 >A $93 >X $f2 >Y $27 >P $2b2d $ac >M $2b2e $e7 >M $2b2f $e4 >M $2b30 $6a >M $e4e7 $b2 >M
T{ op PC S A X Y P -> $2b30 $4f $86 $93 $b2 $a5 }T T{ $2b2d M $2b2e M $2b2f M $2b30 M $e4e7 M -> $ac $e7 $e4 $6a $b2 }T
$bd6c >PC $a6 >S $1b >A $be >X $ad >Y $64 >P $bd6c $ac >M $bd6d $51 >M $bd6e $c5 >M $bd6f $96 >M $c551 $cc >M
T{ op PC S A X Y P -> $bd6f $a6 $1b $be $cc $e4 }T T{ $bd6c M $bd6d M $bd6e M $bd6f M $c551 M -> $ac $51 $c5 $96 $cc }T
$20b4 >PC $85 >S $b6 >A $be >X $91 >Y $a6 >P $20b4 $ac >M $20b5 $d2 >M $20b6 $31 >M $20b7 $2f >M $31d2 $5d >M
T{ op PC S A X Y P -> $20b7 $85 $b6 $be $5d $24 }T T{ $20b4 M $20b5 M $20b6 M $20b7 M $31d2 M -> $ac $d2 $31 $2f $5d }T
$1102 >PC $c3 >S $aa >A $1b >X $cc >Y $26 >P $1102 $ac >M $1103 $95 >M $1104 $59 >M $1105 $da >M $5995 $6a >M
T{ op PC S A X Y P -> $1105 $c3 $aa $1b $6a $24 }T T{ $1102 M $1103 M $1104 M $1105 M $5995 M -> $ac $95 $59 $da $6a }T
$4351 >PC $38 >S $97 >A $4b >X $8b >Y $21 >P $1e38 $cf >M $4351 $ac >M $4352 $38 >M $4353 $1e >M $4354 $9b >M
T{ op PC S A X Y P -> $4354 $38 $97 $4b $cf $a1 }T T{ $1e38 M $4351 M $4352 M $4353 M $4354 M -> $cf $ac $38 $1e $9b }T
$1aa7 >PC $c8 >S $14 >A $69 >X $3c >Y $24 >P $1aa7 $ac >M $1aa8 $78 >M $1aa9 $a4 >M $1aaa $11 >M $a478 $de >M
T{ op PC S A X Y P -> $1aaa $c8 $14 $69 $de $a4 }T T{ $1aa7 M $1aa8 M $1aa9 M $1aaa M $a478 M -> $ac $78 $a4 $11 $de }T
$6165 >PC $59 >S $55 >A $27 >X $29 >Y $22 >P $6165 $ac >M $6166 $7a >M $6167 $b3 >M $6168 $37 >M $b37a $37 >M
T{ op PC S A X Y P -> $6168 $59 $55 $27 $37 $20 }T T{ $6165 M $6166 M $6167 M $6168 M $b37a M -> $ac $7a $b3 $37 $37 }T
$2c32 >PC $dd >S $e6 >A $55 >X $c9 >Y $e0 >P $188d $cf >M $2c32 $ac >M $2c33 $8d >M $2c34 $18 >M $2c35 $67 >M
T{ op PC S A X Y P -> $2c35 $dd $e6 $55 $cf $e0 }T T{ $188d M $2c32 M $2c33 M $2c34 M $2c35 M -> $cf $ac $8d $18 $67 }T
$23ec >PC $42 >S $6c >A $ad >X $17 >Y $66 >P $23ec $ac >M $23ed $c0 >M $23ee $49 >M $23ef $cb >M $49c0 $8a >M
T{ op PC S A X Y P -> $23ef $42 $6c $ad $8a $e4 }T T{ $23ec M $23ed M $23ee M $23ef M $49c0 M -> $ac $c0 $49 $cb $8a }T
$8ffa >PC $73 >S $30 >A $1a >X $ad >Y $64 >P $8ffa $ac >M $8ffb $22 >M $8ffc $95 >M $8ffd $3c >M $9522 $de >M
T{ op PC S A X Y P -> $8ffd $73 $30 $1a $de $e4 }T T{ $8ffa M $8ffb M $8ffc M $8ffd M $9522 M -> $ac $22 $95 $3c $de }T
$9afb >PC $47 >S $ba >A $11 >X $74 >Y $e4 >P $29aa $d1 >M $9afb $ac >M $9afc $aa >M $9afd $29 >M $9afe $40 >M
T{ op PC S A X Y P -> $9afe $47 $ba $11 $d1 $e4 }T T{ $29aa M $9afb M $9afc M $9afd M $9afe M -> $d1 $ac $aa $29 $40 }T
$a653 >PC $b8 >S $43 >A $63 >X $24 >Y $62 >P $914d $f6 >M $a653 $ac >M $a654 $4d >M $a655 $91 >M $a656 $3d >M
T{ op PC S A X Y P -> $a656 $b8 $43 $63 $f6 $e0 }T T{ $914d M $a653 M $a654 M $a655 M $a656 M -> $f6 $ac $4d $91 $3d }T
$5813 >PC $bd >S $38 >A $a9 >X $3a >Y $24 >P $3f8e $d1 >M $5813 $ac >M $5814 $8e >M $5815 $3f >M $5816 $a9 >M
T{ op PC S A X Y P -> $5816 $bd $38 $a9 $d1 $a4 }T T{ $3f8e M $5813 M $5814 M $5815 M $5816 M -> $d1 $ac $8e $3f $a9 }T
( ad )
$9778 >PC $0c >S $c9 >A $e6 >X $e2 >Y $27 >P $59f0 $af >M $9778 $ad >M $9779 $f0 >M $977a $59 >M $977b $9e >M
T{ op PC S A X Y P -> $977b $0c $af $e6 $e2 $a5 }T T{ $59f0 M $9778 M $9779 M $977a M $977b M -> $af $ad $f0 $59 $9e }T
$fc08 >PC $23 >S $d1 >A $3d >X $08 >Y $e4 >P $ef9f $ce >M $fc08 $ad >M $fc09 $9f >M $fc0a $ef >M $fc0b $66 >M
T{ op PC S A X Y P -> $fc0b $23 $ce $3d $08 $e4 }T T{ $ef9f M $fc08 M $fc09 M $fc0a M $fc0b M -> $ce $ad $9f $ef $66 }T
$65fc >PC $3c >S $f3 >A $84 >X $f5 >Y $62 >P $2b46 $29 >M $65fc $ad >M $65fd $46 >M $65fe $2b >M $65ff $93 >M
T{ op PC S A X Y P -> $65ff $3c $29 $84 $f5 $60 }T T{ $2b46 M $65fc M $65fd M $65fe M $65ff M -> $29 $ad $46 $2b $93 }T
$1bad >PC $17 >S $5c >A $02 >X $a2 >Y $23 >P $1bad $ad >M $1bae $dd >M $1baf $be >M $1bb0 $23 >M $bedd $57 >M
T{ op PC S A X Y P -> $1bb0 $17 $57 $02 $a2 $21 }T T{ $1bad M $1bae M $1baf M $1bb0 M $bedd M -> $ad $dd $be $23 $57 }T
$d75c >PC $c4 >S $2b >A $b6 >X $e8 >Y $e6 >P $d75c $ad >M $d75d $a6 >M $d75e $f3 >M $d75f $ad >M $f3a6 $f4 >M
T{ op PC S A X Y P -> $d75f $c4 $f4 $b6 $e8 $e4 }T T{ $d75c M $d75d M $d75e M $d75f M $f3a6 M -> $ad $a6 $f3 $ad $f4 }T
$b50e >PC $b5 >S $d3 >A $59 >X $b4 >Y $27 >P $b04a $0a >M $b50e $ad >M $b50f $4a >M $b510 $b0 >M $b511 $21 >M
T{ op PC S A X Y P -> $b511 $b5 $0a $59 $b4 $25 }T T{ $b04a M $b50e M $b50f M $b510 M $b511 M -> $0a $ad $4a $b0 $21 }T
$8a2a >PC $07 >S $b8 >A $84 >X $56 >Y $e6 >P $8a2a $ad >M $8a2b $7d >M $8a2c $bc >M $8a2d $32 >M $bc7d $5b >M
T{ op PC S A X Y P -> $8a2d $07 $5b $84 $56 $64 }T T{ $8a2a M $8a2b M $8a2c M $8a2d M $bc7d M -> $ad $7d $bc $32 $5b }T
$60b2 >PC $42 >S $2e >A $c3 >X $88 >Y $66 >P $2cfb $86 >M $60b2 $ad >M $60b3 $fb >M $60b4 $2c >M $60b5 $21 >M
T{ op PC S A X Y P -> $60b5 $42 $86 $c3 $88 $e4 }T T{ $2cfb M $60b2 M $60b3 M $60b4 M $60b5 M -> $86 $ad $fb $2c $21 }T
$dba9 >PC $cc >S $10 >A $f3 >X $a8 >Y $61 >P $dba9 $ad >M $dbaa $40 >M $dbab $dd >M $dbac $1c >M $dd40 $c8 >M
T{ op PC S A X Y P -> $dbac $cc $c8 $f3 $a8 $e1 }T T{ $dba9 M $dbaa M $dbab M $dbac M $dd40 M -> $ad $40 $dd $1c $c8 }T
$b8a2 >PC $08 >S $74 >A $8a >X $96 >Y $60 >P $b8a2 $ad >M $b8a3 $33 >M $b8a4 $f6 >M $b8a5 $d0 >M $f633 $e0 >M
T{ op PC S A X Y P -> $b8a5 $08 $e0 $8a $96 $e0 }T T{ $b8a2 M $b8a3 M $b8a4 M $b8a5 M $f633 M -> $ad $33 $f6 $d0 $e0 }T
$6e8f >PC $c3 >S $53 >A $a4 >X $a4 >Y $a1 >P $55a3 $c7 >M $6e8f $ad >M $6e90 $a3 >M $6e91 $55 >M $6e92 $7a >M
T{ op PC S A X Y P -> $6e92 $c3 $c7 $a4 $a4 $a1 }T T{ $55a3 M $6e8f M $6e90 M $6e91 M $6e92 M -> $c7 $ad $a3 $55 $7a }T
$e43e >PC $67 >S $bb >A $85 >X $7a >Y $a3 >P $817b $68 >M $e43e $ad >M $e43f $7b >M $e440 $81 >M $e441 $50 >M
T{ op PC S A X Y P -> $e441 $67 $68 $85 $7a $21 }T T{ $817b M $e43e M $e43f M $e440 M $e441 M -> $68 $ad $7b $81 $50 }T
$4fd2 >PC $cd >S $fb >A $9d >X $14 >Y $61 >P $4fd2 $ad >M $4fd3 $3f >M $4fd4 $b8 >M $4fd5 $04 >M $b83f $9d >M
T{ op PC S A X Y P -> $4fd5 $cd $9d $9d $14 $e1 }T T{ $4fd2 M $4fd3 M $4fd4 M $4fd5 M $b83f M -> $ad $3f $b8 $04 $9d }T
$e513 >PC $78 >S $1f >A $8c >X $f9 >Y $a1 >P $e513 $ad >M $e514 $eb >M $e515 $ee >M $e516 $3e >M $eeeb $17 >M
T{ op PC S A X Y P -> $e516 $78 $17 $8c $f9 $21 }T T{ $e513 M $e514 M $e515 M $e516 M $eeeb M -> $ad $eb $ee $3e $17 }T
$af57 >PC $f4 >S $87 >A $9b >X $f0 >Y $23 >P $19f5 $cc >M $af57 $ad >M $af58 $f5 >M $af59 $19 >M $af5a $0a >M
T{ op PC S A X Y P -> $af5a $f4 $cc $9b $f0 $a1 }T T{ $19f5 M $af57 M $af58 M $af59 M $af5a M -> $cc $ad $f5 $19 $0a }T
$8fb4 >PC $b6 >S $f8 >A $2a >X $88 >Y $a0 >P $773f $a5 >M $8fb4 $ad >M $8fb5 $3f >M $8fb6 $77 >M $8fb7 $56 >M
T{ op PC S A X Y P -> $8fb7 $b6 $a5 $2a $88 $a0 }T T{ $773f M $8fb4 M $8fb5 M $8fb6 M $8fb7 M -> $a5 $ad $3f $77 $56 }T
( ae )
$eb9c >PC $f5 >S $ef >A $ce >X $6f >Y $26 >P $eb9c $ae >M $eb9d $5e >M $eb9e $f0 >M $eb9f $14 >M $f05e $bb >M
T{ op PC S A X Y P -> $eb9f $f5 $ef $bb $6f $a4 }T T{ $eb9c M $eb9d M $eb9e M $eb9f M $f05e M -> $ae $5e $f0 $14 $bb }T
$22c4 >PC $05 >S $ef >A $7b >X $11 >Y $e2 >P $22c4 $ae >M $22c5 $3e >M $22c6 $5f >M $22c7 $ca >M $5f3e $f8 >M
T{ op PC S A X Y P -> $22c7 $05 $ef $f8 $11 $e0 }T T{ $22c4 M $22c5 M $22c6 M $22c7 M $5f3e M -> $ae $3e $5f $ca $f8 }T
$69f0 >PC $56 >S $c6 >A $2e >X $70 >Y $e2 >P $69f0 $ae >M $69f1 $69 >M $69f2 $85 >M $69f3 $6d >M $8569 $fc >M
T{ op PC S A X Y P -> $69f3 $56 $c6 $fc $70 $e0 }T T{ $69f0 M $69f1 M $69f2 M $69f3 M $8569 M -> $ae $69 $85 $6d $fc }T
$0bd7 >PC $f8 >S $b9 >A $1c >X $01 >Y $e0 >P $0bd7 $ae >M $0bd8 $0c >M $0bd9 $e7 >M $0bda $1d >M $e70c $5c >M
T{ op PC S A X Y P -> $0bda $f8 $b9 $5c $01 $60 }T T{ $0bd7 M $0bd8 M $0bd9 M $0bda M $e70c M -> $ae $0c $e7 $1d $5c }T
$a272 >PC $11 >S $ac >A $da >X $5d >Y $e7 >P $4a19 $9e >M $a272 $ae >M $a273 $19 >M $a274 $4a >M $a275 $4c >M
T{ op PC S A X Y P -> $a275 $11 $ac $9e $5d $e5 }T T{ $4a19 M $a272 M $a273 M $a274 M $a275 M -> $9e $ae $19 $4a $4c }T
$ff52 >PC $76 >S $6e >A $18 >X $67 >Y $e5 >P $9074 $cc >M $ff52 $ae >M $ff53 $74 >M $ff54 $90 >M $ff55 $83 >M
T{ op PC S A X Y P -> $ff55 $76 $6e $cc $67 $e5 }T T{ $9074 M $ff52 M $ff53 M $ff54 M $ff55 M -> $cc $ae $74 $90 $83 }T
$5372 >PC $a7 >S $f1 >A $7f >X $5a >Y $63 >P $5372 $ae >M $5373 $cc >M $5374 $fe >M $5375 $0c >M $fecc $e8 >M
T{ op PC S A X Y P -> $5375 $a7 $f1 $e8 $5a $e1 }T T{ $5372 M $5373 M $5374 M $5375 M $fecc M -> $ae $cc $fe $0c $e8 }T
$7ca9 >PC $1f >S $f1 >A $19 >X $6d >Y $e5 >P $7ca9 $ae >M $7caa $d5 >M $7cab $86 >M $7cac $5c >M $86d5 $b1 >M
T{ op PC S A X Y P -> $7cac $1f $f1 $b1 $6d $e5 }T T{ $7ca9 M $7caa M $7cab M $7cac M $86d5 M -> $ae $d5 $86 $5c $b1 }T
$8d2c >PC $91 >S $db >A $89 >X $6d >Y $20 >P $5925 $09 >M $8d2c $ae >M $8d2d $25 >M $8d2e $59 >M $8d2f $28 >M
T{ op PC S A X Y P -> $8d2f $91 $db $09 $6d $20 }T T{ $5925 M $8d2c M $8d2d M $8d2e M $8d2f M -> $09 $ae $25 $59 $28 }T
$e979 >PC $1f >S $a0 >A $a5 >X $e9 >Y $67 >P $00a6 $a7 >M $e979 $ae >M $e97a $a6 >M $e97b $00 >M $e97c $fb >M
T{ op PC S A X Y P -> $e97c $1f $a0 $a7 $e9 $e5 }T T{ $00a6 M $e979 M $e97a M $e97b M $e97c M -> $a7 $ae $a6 $00 $fb }T
$0122 >PC $ac >S $7b >A $fa >X $d7 >Y $60 >P $0122 $ae >M $0123 $fd >M $0124 $cf >M $0125 $95 >M $cffd $b1 >M
T{ op PC S A X Y P -> $0125 $ac $7b $b1 $d7 $e0 }T T{ $0122 M $0123 M $0124 M $0125 M $cffd M -> $ae $fd $cf $95 $b1 }T
$d3c9 >PC $7c >S $c1 >A $2f >X $06 >Y $e0 >P $38cc $a9 >M $d3c9 $ae >M $d3ca $cc >M $d3cb $38 >M $d3cc $62 >M
T{ op PC S A X Y P -> $d3cc $7c $c1 $a9 $06 $e0 }T T{ $38cc M $d3c9 M $d3ca M $d3cb M $d3cc M -> $a9 $ae $cc $38 $62 }T
$416a >PC $a4 >S $85 >A $6e >X $df >Y $64 >P $416a $ae >M $416b $d5 >M $416c $f4 >M $416d $ee >M $f4d5 $e0 >M
T{ op PC S A X Y P -> $416d $a4 $85 $e0 $df $e4 }T T{ $416a M $416b M $416c M $416d M $f4d5 M -> $ae $d5 $f4 $ee $e0 }T
$9e38 >PC $7f >S $f7 >A $06 >X $f6 >Y $a5 >P $32f0 $53 >M $9e38 $ae >M $9e39 $f0 >M $9e3a $32 >M $9e3b $c8 >M
T{ op PC S A X Y P -> $9e3b $7f $f7 $53 $f6 $25 }T T{ $32f0 M $9e38 M $9e39 M $9e3a M $9e3b M -> $53 $ae $f0 $32 $c8 }T
$3aae >PC $ec >S $5b >A $70 >X $63 >Y $e6 >P $3aae $ae >M $3aaf $15 >M $3ab0 $fe >M $3ab1 $63 >M $fe15 $33 >M
T{ op PC S A X Y P -> $3ab1 $ec $5b $33 $63 $64 }T T{ $3aae M $3aaf M $3ab0 M $3ab1 M $fe15 M -> $ae $15 $fe $63 $33 }T
$4521 >PC $8c >S $0b >A $80 >X $e7 >Y $62 >P $4521 $ae >M $4522 $8e >M $4523 $55 >M $4524 $26 >M $558e $29 >M
T{ op PC S A X Y P -> $4524 $8c $0b $29 $e7 $60 }T T{ $4521 M $4522 M $4523 M $4524 M $558e M -> $ae $8e $55 $26 $29 }T
( af )
$1c3b >PC $8f >S $60 >A $14 >X $fe >Y $63 >P $003f $dc >M $1c3b $af >M $1c3c $3f >M $1c3d $23 >M $1c61 $0b >M
T{ op PC S A X Y P -> $1c61 $8f $60 $14 $fe $63 }T T{ $003f M $1c3b M $1c3c M $1c3d M $1c61 M -> $dc $af $3f $23 $0b }T
$5eb5 >PC $21 >S $fa >A $75 >X $bf >Y $e4 >P $0028 $26 >M $5e18 $30 >M $5eb5 $af >M $5eb6 $28 >M $5eb7 $60 >M $5f18 $63 >M
T{ op PC S A X Y P -> $5f18 $21 $fa $75 $bf $e4 }T T{ $0028 M $5e18 M $5eb5 M $5eb6 M $5eb7 M $5f18 M -> $26 $30 $af $28 $60 $63 }T
$8c1d >PC $1e >S $ca >A $29 >X $30 >Y $a4 >P $0094 $47 >M $8bfd $b9 >M $8c1d $af >M $8c1e $94 >M $8c1f $dd >M $8cfd $c3 >M
T{ op PC S A X Y P -> $8bfd $1e $ca $29 $30 $a4 }T T{ $0094 M $8bfd M $8c1d M $8c1e M $8c1f M $8cfd M -> $47 $b9 $af $94 $dd $c3 }T
$7e16 >PC $ea >S $28 >A $af >X $5c >Y $20 >P $008e $49 >M $7e16 $af >M $7e17 $8e >M $7e18 $db >M $7e19 $66 >M $7ef4 $f8 >M
T{ op PC S A X Y P -> $7e19 $ea $28 $af $5c $20 }T T{ $008e M $7e16 M $7e17 M $7e18 M $7e19 M $7ef4 M -> $49 $af $8e $db $66 $f8 }T
$af20 >PC $45 >S $1a >A $3d >X $27 >Y $25 >P $000b $33 >M $af20 $af >M $af21 $0b >M $af22 $4a >M $af23 $52 >M $af6d $09 >M
T{ op PC S A X Y P -> $af23 $45 $1a $3d $27 $25 }T T{ $000b M $af20 M $af21 M $af22 M $af23 M $af6d M -> $33 $af $0b $4a $52 $09 }T
$d480 >PC $27 >S $83 >A $83 >X $ff >Y $23 >P $0004 $6c >M $d480 $af >M $d481 $04 >M $d482 $21 >M $d4a4 $09 >M
T{ op PC S A X Y P -> $d4a4 $27 $83 $83 $ff $23 }T T{ $0004 M $d480 M $d481 M $d482 M $d4a4 M -> $6c $af $04 $21 $09 }T
$306c >PC $ca >S $34 >A $8e >X $e2 >Y $67 >P $00d0 $cc >M $302c $f7 >M $306c $af >M $306d $d0 >M $306e $bd >M
T{ op PC S A X Y P -> $302c $ca $34 $8e $e2 $67 }T T{ $00d0 M $302c M $306c M $306d M $306e M -> $cc $f7 $af $d0 $bd }T
$21dd >PC $97 >S $84 >A $77 >X $56 >Y $66 >P $00cc $87 >M $21dd $af >M $21de $cc >M $21df $1c >M $21fc $94 >M
T{ op PC S A X Y P -> $21fc $97 $84 $77 $56 $66 }T T{ $00cc M $21dd M $21de M $21df M $21fc M -> $87 $af $cc $1c $94 }T
$bff5 >PC $1a >S $b7 >A $7e >X $b8 >Y $61 >P $0014 $40 >M $bfb7 $61 >M $bff5 $af >M $bff6 $14 >M $bff7 $bf >M $bff8 $cb >M
T{ op PC S A X Y P -> $bff8 $1a $b7 $7e $b8 $61 }T T{ $0014 M $bfb7 M $bff5 M $bff6 M $bff7 M $bff8 M -> $40 $61 $af $14 $bf $cb }T
$88e2 >PC $28 >S $c0 >A $1b >X $3b >Y $a6 >P $00fb $32 >M $880b $b4 >M $88e2 $af >M $88e3 $fb >M $88e4 $26 >M $88e5 $e8 >M
T{ op PC S A X Y P -> $88e5 $28 $c0 $1b $3b $a6 }T T{ $00fb M $880b M $88e2 M $88e3 M $88e4 M $88e5 M -> $32 $b4 $af $fb $26 $e8 }T
$d70c >PC $06 >S $3a >A $fc >X $0a >Y $e1 >P $0079 $00 >M $d70c $af >M $d70d $79 >M $d70e $ef >M $d70f $35 >M $d7fe $75 >M
T{ op PC S A X Y P -> $d70f $06 $3a $fc $0a $e1 }T T{ $0079 M $d70c M $d70d M $d70e M $d70f M $d7fe M -> $00 $af $79 $ef $35 $75 }T
$fd07 >PC $8c >S $09 >A $23 >X $5d >Y $e2 >P $0025 $b0 >M $fd07 $af >M $fd08 $25 >M $fd09 $2a >M $fd0a $3e >M $fd34 $4a >M
T{ op PC S A X Y P -> $fd0a $8c $09 $23 $5d $e2 }T T{ $0025 M $fd07 M $fd08 M $fd09 M $fd0a M $fd34 M -> $b0 $af $25 $2a $3e $4a }T
$b1bf >PC $a8 >S $d8 >A $1b >X $3c >Y $e1 >P $006d $c4 >M $b1bf $af >M $b1c0 $6d >M $b1c1 $26 >M $b1e8 $82 >M
T{ op PC S A X Y P -> $b1e8 $a8 $d8 $1b $3c $e1 }T T{ $006d M $b1bf M $b1c0 M $b1c1 M $b1e8 M -> $c4 $af $6d $26 $82 }T
$01ad >PC $bd >S $72 >A $4f >X $54 >Y $a6 >P $001d $4c >M $0131 $d7 >M $01ad $af >M $01ae $1d >M $01af $81 >M
T{ op PC S A X Y P -> $0131 $bd $72 $4f $54 $a6 }T T{ $001d M $0131 M $01ad M $01ae M $01af M -> $4c $d7 $af $1d $81 }T
$c6a3 >PC $dd >S $81 >A $a1 >X $50 >Y $25 >P $0044 $91 >M $c634 $46 >M $c6a3 $af >M $c6a4 $44 >M $c6a5 $8e >M $c6a6 $17 >M
T{ op PC S A X Y P -> $c6a6 $dd $81 $a1 $50 $25 }T T{ $0044 M $c634 M $c6a3 M $c6a4 M $c6a5 M $c6a6 M -> $91 $46 $af $44 $8e $17 }T
$4626 >PC $07 >S $f5 >A $43 >X $bf >Y $62 >P $00dd $cb >M $4626 $af >M $4627 $dd >M $4628 $8a >M $4629 $cf >M $46b3 $02 >M
T{ op PC S A X Y P -> $4629 $07 $f5 $43 $bf $62 }T T{ $00dd M $4626 M $4627 M $4628 M $4629 M $46b3 M -> $cb $af $dd $8a $cf $02 }T
( b0 )
$9711 >PC $16 >S $b9 >A $8e >X $fb >Y $26 >P $9711 $b0 >M $9712 $03 >M $9713 $94 >M
T{ op PC S A X Y P -> $9713 $16 $b9 $8e $fb $26 }T T{ $9711 M $9712 M $9713 M -> $b0 $03 $94 }T
$49a0 >PC $8a >S $98 >A $c1 >X $af >Y $22 >P $49a0 $b0 >M $49a1 $1c >M $49a2 $3b >M
T{ op PC S A X Y P -> $49a2 $8a $98 $c1 $af $22 }T T{ $49a0 M $49a1 M $49a2 M -> $b0 $1c $3b }T
$c0f8 >PC $a7 >S $0a >A $dc >X $f5 >Y $a5 >P $c0a0 $aa >M $c0f8 $b0 >M $c0f9 $a6 >M $c0fa $d1 >M
T{ op PC S A X Y P -> $c0a0 $a7 $0a $dc $f5 $a5 }T T{ $c0a0 M $c0f8 M $c0f9 M $c0fa M -> $aa $b0 $a6 $d1 }T
$e50b >PC $5f >S $82 >A $83 >X $a5 >Y $e1 >P $e50b $b0 >M $e50c $26 >M $e50d $95 >M $e533 $61 >M
T{ op PC S A X Y P -> $e533 $5f $82 $83 $a5 $e1 }T T{ $e50b M $e50c M $e50d M $e533 M -> $b0 $26 $95 $61 }T
$1ce0 >PC $99 >S $a4 >A $59 >X $89 >Y $a3 >P $1c89 $e4 >M $1ce0 $b0 >M $1ce1 $a7 >M $1ce2 $5e >M
T{ op PC S A X Y P -> $1c89 $99 $a4 $59 $89 $a3 }T T{ $1c89 M $1ce0 M $1ce1 M $1ce2 M -> $e4 $b0 $a7 $5e }T
$6a8f >PC $b7 >S $dd >A $3e >X $db >Y $e7 >P $6a8f $b0 >M $6a90 $49 >M $6a91 $94 >M $6ada $35 >M
T{ op PC S A X Y P -> $6ada $b7 $dd $3e $db $e7 }T T{ $6a8f M $6a90 M $6a91 M $6ada M -> $b0 $49 $94 $35 }T
$0b30 >PC $ec >S $ba >A $54 >X $0b >Y $a7 >P $0ae9 $85 >M $0b30 $b0 >M $0b31 $b7 >M $0b32 $ed >M $0be9 $42 >M
T{ op PC S A X Y P -> $0ae9 $ec $ba $54 $0b $a7 }T T{ $0ae9 M $0b30 M $0b31 M $0b32 M $0be9 M -> $85 $b0 $b7 $ed $42 }T
$651e >PC $a6 >S $5d >A $01 >X $c3 >Y $a2 >P $651e $b0 >M $651f $02 >M $6520 $23 >M
T{ op PC S A X Y P -> $6520 $a6 $5d $01 $c3 $a2 }T T{ $651e M $651f M $6520 M -> $b0 $02 $23 }T
$901a >PC $fb >S $85 >A $11 >X $e3 >Y $26 >P $901a $b0 >M $901b $b6 >M $901c $1f >M
T{ op PC S A X Y P -> $901c $fb $85 $11 $e3 $26 }T T{ $901a M $901b M $901c M -> $b0 $b6 $1f }T
$bd6e >PC $ed >S $cc >A $10 >X $65 >Y $a6 >P $bd6e $b0 >M $bd6f $ba >M $bd70 $1b >M
T{ op PC S A X Y P -> $bd70 $ed $cc $10 $65 $a6 }T T{ $bd6e M $bd6f M $bd70 M -> $b0 $ba $1b }T
$217a >PC $37 >S $59 >A $17 >X $9c >Y $a6 >P $217a $b0 >M $217b $d7 >M $217c $e4 >M
T{ op PC S A X Y P -> $217c $37 $59 $17 $9c $a6 }T T{ $217a M $217b M $217c M -> $b0 $d7 $e4 }T
$2b2a >PC $07 >S $23 >A $4e >X $4d >Y $22 >P $2b2a $b0 >M $2b2b $3d >M $2b2c $8a >M
T{ op PC S A X Y P -> $2b2c $07 $23 $4e $4d $22 }T T{ $2b2a M $2b2b M $2b2c M -> $b0 $3d $8a }T
$de25 >PC $06 >S $0d >A $da >X $5a >Y $66 >P $de25 $b0 >M $de26 $f2 >M $de27 $2a >M
T{ op PC S A X Y P -> $de27 $06 $0d $da $5a $66 }T T{ $de25 M $de26 M $de27 M -> $b0 $f2 $2a }T
$8bd3 >PC $1c >S $f9 >A $b5 >X $36 >Y $26 >P $8bd3 $b0 >M $8bd4 $e8 >M $8bd5 $65 >M
T{ op PC S A X Y P -> $8bd5 $1c $f9 $b5 $36 $26 }T T{ $8bd3 M $8bd4 M $8bd5 M -> $b0 $e8 $65 }T
$bcde >PC $9f >S $46 >A $3a >X $ac >Y $a6 >P $bcde $b0 >M $bcdf $7b >M $bce0 $6c >M
T{ op PC S A X Y P -> $bce0 $9f $46 $3a $ac $a6 }T T{ $bcde M $bcdf M $bce0 M -> $b0 $7b $6c }T
$7ccf >PC $14 >S $df >A $f8 >X $cb >Y $e1 >P $7ccf $b0 >M $7cd0 $04 >M $7cd1 $92 >M $7cd5 $f6 >M
T{ op PC S A X Y P -> $7cd5 $14 $df $f8 $cb $e1 }T T{ $7ccf M $7cd0 M $7cd1 M $7cd5 M -> $b0 $04 $92 $f6 }T
( b1 )
$e868 >PC $50 >S $30 >A $93 >X $ab >Y $21 >P $0086 $44 >M $0087 $7f >M $7fef $b5 >M $e868 $b1 >M $e869 $86 >M $e86a $9b >M
T{ op PC S A X Y P -> $e86a $50 $b5 $93 $ab $a1 }T T{ $0086 M $0087 M $7fef M $e868 M $e869 M $e86a M -> $44 $7f $b5 $b1 $86 $9b }T
$b668 >PC $1c >S $70 >A $19 >X $71 >Y $23 >P $002f $81 >M $0030 $10 >M $10f2 $7d >M $b668 $b1 >M $b669 $2f >M $b66a $cf >M
T{ op PC S A X Y P -> $b66a $1c $7d $19 $71 $21 }T T{ $002f M $0030 M $10f2 M $b668 M $b669 M $b66a M -> $81 $10 $7d $b1 $2f $cf }T
$f5bc >PC $83 >S $b7 >A $ce >X $24 >Y $a4 >P $00f9 $77 >M $00fa $07 >M $079b $d2 >M $f5bc $b1 >M $f5bd $f9 >M $f5be $38 >M
T{ op PC S A X Y P -> $f5be $83 $d2 $ce $24 $a4 }T T{ $00f9 M $00fa M $079b M $f5bc M $f5bd M $f5be M -> $77 $07 $d2 $b1 $f9 $38 }T
$234f >PC $d7 >S $00 >A $61 >X $d8 >Y $e7 >P $007b $34 >M $007c $fa >M $234f $b1 >M $2350 $7b >M $2351 $75 >M $fb0c $e2 >M
T{ op PC S A X Y P -> $2351 $d7 $e2 $61 $d8 $e5 }T T{ $007b M $007c M $234f M $2350 M $2351 M $fb0c M -> $34 $fa $b1 $7b $75 $e2 }T
$4a47 >PC $ce >S $c6 >A $9b >X $e2 >Y $26 >P $0015 $43 >M $0016 $6c >M $4a47 $b1 >M $4a48 $15 >M $4a49 $d6 >M $6d25 $8f >M
T{ op PC S A X Y P -> $4a49 $ce $8f $9b $e2 $a4 }T T{ $0015 M $0016 M $4a47 M $4a48 M $4a49 M $6d25 M -> $43 $6c $b1 $15 $d6 $8f }T
$34cd >PC $1b >S $e6 >A $2c >X $cc >Y $e0 >P $0075 $36 >M $0076 $38 >M $34cd $b1 >M $34ce $75 >M $34cf $5f >M $3902 $64 >M
T{ op PC S A X Y P -> $34cf $1b $64 $2c $cc $60 }T T{ $0075 M $0076 M $34cd M $34ce M $34cf M $3902 M -> $36 $38 $b1 $75 $5f $64 }T
$421c >PC $54 >S $89 >A $4e >X $d1 >Y $26 >P $00ad $e9 >M $00ae $dc >M $421c $b1 >M $421d $ad >M $421e $25 >M $ddba $fb >M
T{ op PC S A X Y P -> $421e $54 $fb $4e $d1 $a4 }T T{ $00ad M $00ae M $421c M $421d M $421e M $ddba M -> $e9 $dc $b1 $ad $25 $fb }T
$563d >PC $8b >S $42 >A $92 >X $23 >Y $66 >P $00b6 $d6 >M $00b7 $db >M $563d $b1 >M $563e $b6 >M $563f $40 >M $dbf9 $c9 >M
T{ op PC S A X Y P -> $563f $8b $c9 $92 $23 $e4 }T T{ $00b6 M $00b7 M $563d M $563e M $563f M $dbf9 M -> $d6 $db $b1 $b6 $40 $c9 }T
$5ce7 >PC $a3 >S $5d >A $02 >X $de >Y $e7 >P $00f7 $7e >M $00f8 $3f >M $405c $6e >M $5ce7 $b1 >M $5ce8 $f7 >M $5ce9 $fc >M
T{ op PC S A X Y P -> $5ce9 $a3 $6e $02 $de $65 }T T{ $00f7 M $00f8 M $405c M $5ce7 M $5ce8 M $5ce9 M -> $7e $3f $6e $b1 $f7 $fc }T
$2fba >PC $7b >S $e1 >A $13 >X $4c >Y $62 >P $00e1 $08 >M $00e2 $63 >M $2fba $b1 >M $2fbb $e1 >M $2fbc $7e >M $6354 $6e >M
T{ op PC S A X Y P -> $2fbc $7b $6e $13 $4c $60 }T T{ $00e1 M $00e2 M $2fba M $2fbb M $2fbc M $6354 M -> $08 $63 $b1 $e1 $7e $6e }T
$f405 >PC $ee >S $fb >A $1f >X $4a >Y $27 >P $00e5 $4f >M $00e6 $a8 >M $a899 $2c >M $f405 $b1 >M $f406 $e5 >M $f407 $a6 >M
T{ op PC S A X Y P -> $f407 $ee $2c $1f $4a $25 }T T{ $00e5 M $00e6 M $a899 M $f405 M $f406 M $f407 M -> $4f $a8 $2c $b1 $e5 $a6 }T
$14b4 >PC $76 >S $39 >A $0d >X $68 >Y $61 >P $008b $36 >M $008c $59 >M $14b4 $b1 >M $14b5 $8b >M $14b6 $ff >M $599e $20 >M
T{ op PC S A X Y P -> $14b6 $76 $20 $0d $68 $61 }T T{ $008b M $008c M $14b4 M $14b5 M $14b6 M $599e M -> $36 $59 $b1 $8b $ff $20 }T
$a9a2 >PC $83 >S $cd >A $19 >X $c5 >Y $22 >P $00bd $e4 >M $00be $88 >M $89a9 $ce >M $a9a2 $b1 >M $a9a3 $bd >M $a9a4 $5f >M
T{ op PC S A X Y P -> $a9a4 $83 $ce $19 $c5 $a0 }T T{ $00bd M $00be M $89a9 M $a9a2 M $a9a3 M $a9a4 M -> $e4 $88 $ce $b1 $bd $5f }T
$ecb8 >PC $63 >S $1d >A $bd >X $e4 >Y $62 >P $0009 $21 >M $000a $9c >M $9d05 $d2 >M $ecb8 $b1 >M $ecb9 $09 >M $ecba $32 >M
T{ op PC S A X Y P -> $ecba $63 $d2 $bd $e4 $e0 }T T{ $0009 M $000a M $9d05 M $ecb8 M $ecb9 M $ecba M -> $21 $9c $d2 $b1 $09 $32 }T
$137f >PC $ab >S $cf >A $e4 >X $37 >Y $67 >P $00fa $da >M $00fb $02 >M $0311 $b9 >M $137f $b1 >M $1380 $fa >M $1381 $53 >M
T{ op PC S A X Y P -> $1381 $ab $b9 $e4 $37 $e5 }T T{ $00fa M $00fb M $0311 M $137f M $1380 M $1381 M -> $da $02 $b9 $b1 $fa $53 }T
$c611 >PC $1f >S $1e >A $da >X $ad >Y $a7 >P $008c $9e >M $008d $48 >M $494b $c3 >M $c611 $b1 >M $c612 $8c >M $c613 $d9 >M
T{ op PC S A X Y P -> $c613 $1f $c3 $da $ad $a5 }T T{ $008c M $008d M $494b M $c611 M $c612 M $c613 M -> $9e $48 $c3 $b1 $8c $d9 }T
( b2 )
$24d3 >PC $17 >S $7c >A $79 >X $36 >Y $a0 >P $0019 $cb >M $001a $87 >M $24d3 $b2 >M $24d4 $19 >M $24d5 $13 >M $87cb $16 >M
T{ op PC S A X Y P -> $24d5 $17 $16 $79 $36 $20 }T T{ $0019 M $001a M $24d3 M $24d4 M $24d5 M $87cb M -> $cb $87 $b2 $19 $13 $16 }T
$29b3 >PC $4a >S $79 >A $b6 >X $12 >Y $a0 >P $003d $0a >M $003e $69 >M $29b3 $b2 >M $29b4 $3d >M $29b5 $23 >M $690a $73 >M
T{ op PC S A X Y P -> $29b5 $4a $73 $b6 $12 $20 }T T{ $003d M $003e M $29b3 M $29b4 M $29b5 M $690a M -> $0a $69 $b2 $3d $23 $73 }T
$86ab >PC $07 >S $fd >A $58 >X $ae >Y $e4 >P $0039 $58 >M $003a $58 >M $5858 $da >M $86ab $b2 >M $86ac $39 >M $86ad $c3 >M
T{ op PC S A X Y P -> $86ad $07 $da $58 $ae $e4 }T T{ $0039 M $003a M $5858 M $86ab M $86ac M $86ad M -> $58 $58 $da $b2 $39 $c3 }T
$d120 >PC $eb >S $7f >A $d6 >X $15 >Y $e1 >P $00b9 $db >M $00ba $85 >M $85db $41 >M $d120 $b2 >M $d121 $b9 >M $d122 $22 >M
T{ op PC S A X Y P -> $d122 $eb $41 $d6 $15 $61 }T T{ $00b9 M $00ba M $85db M $d120 M $d121 M $d122 M -> $db $85 $41 $b2 $b9 $22 }T
$f25c >PC $0e >S $c3 >A $2a >X $e7 >Y $67 >P $00ef $5e >M $00f0 $ea >M $ea5e $74 >M $f25c $b2 >M $f25d $ef >M $f25e $4c >M
T{ op PC S A X Y P -> $f25e $0e $74 $2a $e7 $65 }T T{ $00ef M $00f0 M $ea5e M $f25c M $f25d M $f25e M -> $5e $ea $74 $b2 $ef $4c }T
$3395 >PC $5f >S $32 >A $04 >X $9b >Y $e3 >P $004f $2e >M $0050 $2d >M $2d2e $4f >M $3395 $b2 >M $3396 $4f >M $3397 $f9 >M
T{ op PC S A X Y P -> $3397 $5f $4f $04 $9b $61 }T T{ $004f M $0050 M $2d2e M $3395 M $3396 M $3397 M -> $2e $2d $4f $b2 $4f $f9 }T
$c270 >PC $79 >S $cb >A $b7 >X $21 >Y $66 >P $0035 $36 >M $0036 $df >M $c270 $b2 >M $c271 $35 >M $c272 $d6 >M $df36 $ef >M
T{ op PC S A X Y P -> $c272 $79 $ef $b7 $21 $e4 }T T{ $0035 M $0036 M $c270 M $c271 M $c272 M $df36 M -> $36 $df $b2 $35 $d6 $ef }T
$9deb >PC $fe >S $17 >A $8c >X $c5 >Y $20 >P $0072 $4e >M $0073 $94 >M $944e $91 >M $9deb $b2 >M $9dec $72 >M $9ded $72 >M
T{ op PC S A X Y P -> $9ded $fe $91 $8c $c5 $a0 }T T{ $0072 M $0073 M $944e M $9deb M $9dec M $9ded M -> $4e $94 $91 $b2 $72 $72 }T
$4eb3 >PC $cc >S $71 >A $13 >X $58 >Y $a1 >P $00e0 $9d >M $00e1 $9d >M $4eb3 $b2 >M $4eb4 $e0 >M $4eb5 $11 >M $9d9d $0a >M
T{ op PC S A X Y P -> $4eb5 $cc $0a $13 $58 $21 }T T{ $00e0 M $00e1 M $4eb3 M $4eb4 M $4eb5 M $9d9d M -> $9d $9d $b2 $e0 $11 $0a }T
$88ee >PC $e5 >S $bc >A $f2 >X $e2 >Y $65 >P $0007 $5e >M $0008 $fd >M $88ee $b2 >M $88ef $07 >M $88f0 $df >M $fd5e $bb >M
T{ op PC S A X Y P -> $88f0 $e5 $bb $f2 $e2 $e5 }T T{ $0007 M $0008 M $88ee M $88ef M $88f0 M $fd5e M -> $5e $fd $b2 $07 $df $bb }T
$78ad >PC $d0 >S $e5 >A $32 >X $f3 >Y $67 >P $00cd $b6 >M $00ce $a6 >M $78ad $b2 >M $78ae $cd >M $78af $ec >M $a6b6 $e4 >M
T{ op PC S A X Y P -> $78af $d0 $e4 $32 $f3 $e5 }T T{ $00cd M $00ce M $78ad M $78ae M $78af M $a6b6 M -> $b6 $a6 $b2 $cd $ec $e4 }T
$7385 >PC $0c >S $47 >A $b2 >X $f9 >Y $e7 >P $0001 $b6 >M $0002 $89 >M $7385 $b2 >M $7386 $01 >M $7387 $23 >M $89b6 $05 >M
T{ op PC S A X Y P -> $7387 $0c $05 $b2 $f9 $65 }T T{ $0001 M $0002 M $7385 M $7386 M $7387 M $89b6 M -> $b6 $89 $b2 $01 $23 $05 }T
$0744 >PC $7f >S $9e >A $1a >X $69 >Y $66 >P $00c4 $32 >M $00c5 $97 >M $0744 $b2 >M $0745 $c4 >M $0746 $18 >M $9732 $3a >M
T{ op PC S A X Y P -> $0746 $7f $3a $1a $69 $64 }T T{ $00c4 M $00c5 M $0744 M $0745 M $0746 M $9732 M -> $32 $97 $b2 $c4 $18 $3a }T
$7ee0 >PC $83 >S $c1 >A $cf >X $81 >Y $61 >P $00c6 $92 >M $00c7 $48 >M $4892 $d5 >M $7ee0 $b2 >M $7ee1 $c6 >M $7ee2 $4c >M
T{ op PC S A X Y P -> $7ee2 $83 $d5 $cf $81 $e1 }T T{ $00c6 M $00c7 M $4892 M $7ee0 M $7ee1 M $7ee2 M -> $92 $48 $d5 $b2 $c6 $4c }T
$76a6 >PC $21 >S $46 >A $ae >X $6f >Y $a7 >P $000d $c2 >M $000e $77 >M $76a6 $b2 >M $76a7 $0d >M $76a8 $d9 >M $77c2 $d3 >M
T{ op PC S A X Y P -> $76a8 $21 $d3 $ae $6f $a5 }T T{ $000d M $000e M $76a6 M $76a7 M $76a8 M $77c2 M -> $c2 $77 $b2 $0d $d9 $d3 }T
$9cfb >PC $7f >S $5d >A $a7 >X $8c >Y $24 >P $00d1 $82 >M $00d2 $89 >M $8982 $ab >M $9cfb $b2 >M $9cfc $d1 >M $9cfd $c4 >M
T{ op PC S A X Y P -> $9cfd $7f $ab $a7 $8c $a4 }T T{ $00d1 M $00d2 M $8982 M $9cfb M $9cfc M $9cfd M -> $82 $89 $ab $b2 $d1 $c4 }T
( b3 )
$145b >PC $fe >S $e4 >A $d7 >X $b5 >Y $60 >P $145b $b3 >M $145c $88 >M $145d $ee >M
T{ op PC S A X Y P -> $145c $fe $e4 $d7 $b5 $60 }T T{ $145b M $145c M $145d M -> $b3 $88 $ee }T
$bc64 >PC $f0 >S $2c >A $97 >X $26 >Y $27 >P $bc64 $b3 >M $bc65 $ca >M $bc66 $44 >M
T{ op PC S A X Y P -> $bc65 $f0 $2c $97 $26 $27 }T T{ $bc64 M $bc65 M $bc66 M -> $b3 $ca $44 }T
$11c0 >PC $f4 >S $30 >A $51 >X $ef >Y $e1 >P $11c0 $b3 >M $11c1 $33 >M $11c2 $ca >M
T{ op PC S A X Y P -> $11c1 $f4 $30 $51 $ef $e1 }T T{ $11c0 M $11c1 M $11c2 M -> $b3 $33 $ca }T
$f76f >PC $dc >S $f3 >A $e0 >X $7a >Y $22 >P $f76f $b3 >M $f770 $93 >M $f771 $92 >M
T{ op PC S A X Y P -> $f770 $dc $f3 $e0 $7a $22 }T T{ $f76f M $f770 M $f771 M -> $b3 $93 $92 }T
$c56b >PC $7f >S $d5 >A $4b >X $5d >Y $a6 >P $c56b $b3 >M $c56c $db >M $c56d $3e >M
T{ op PC S A X Y P -> $c56c $7f $d5 $4b $5d $a6 }T T{ $c56b M $c56c M $c56d M -> $b3 $db $3e }T
$a087 >PC $f7 >S $15 >A $9a >X $21 >Y $a1 >P $a087 $b3 >M $a088 $33 >M $a089 $6e >M
T{ op PC S A X Y P -> $a088 $f7 $15 $9a $21 $a1 }T T{ $a087 M $a088 M $a089 M -> $b3 $33 $6e }T
$204a >PC $64 >S $51 >A $d0 >X $45 >Y $a6 >P $204a $b3 >M $204b $35 >M $204c $c7 >M
T{ op PC S A X Y P -> $204b $64 $51 $d0 $45 $a6 }T T{ $204a M $204b M $204c M -> $b3 $35 $c7 }T
$a93a >PC $de >S $ad >A $a3 >X $62 >Y $e7 >P $a93a $b3 >M $a93b $5e >M $a93c $81 >M
T{ op PC S A X Y P -> $a93b $de $ad $a3 $62 $e7 }T T{ $a93a M $a93b M $a93c M -> $b3 $5e $81 }T
$c141 >PC $fe >S $2d >A $d0 >X $51 >Y $26 >P $c141 $b3 >M $c142 $ec >M $c143 $a9 >M
T{ op PC S A X Y P -> $c142 $fe $2d $d0 $51 $26 }T T{ $c141 M $c142 M $c143 M -> $b3 $ec $a9 }T
$a4d6 >PC $97 >S $1f >A $13 >X $b1 >Y $60 >P $a4d6 $b3 >M $a4d7 $38 >M $a4d8 $a8 >M
T{ op PC S A X Y P -> $a4d7 $97 $1f $13 $b1 $60 }T T{ $a4d6 M $a4d7 M $a4d8 M -> $b3 $38 $a8 }T
$661f >PC $cd >S $91 >A $7f >X $1c >Y $a5 >P $661f $b3 >M $6620 $0f >M $6621 $22 >M
T{ op PC S A X Y P -> $6620 $cd $91 $7f $1c $a5 }T T{ $661f M $6620 M $6621 M -> $b3 $0f $22 }T
$a2a0 >PC $a1 >S $be >A $08 >X $3e >Y $60 >P $a2a0 $b3 >M $a2a1 $fa >M $a2a2 $96 >M
T{ op PC S A X Y P -> $a2a1 $a1 $be $08 $3e $60 }T T{ $a2a0 M $a2a1 M $a2a2 M -> $b3 $fa $96 }T
$5302 >PC $73 >S $ca >A $26 >X $5c >Y $60 >P $5302 $b3 >M $5303 $a9 >M $5304 $02 >M
T{ op PC S A X Y P -> $5303 $73 $ca $26 $5c $60 }T T{ $5302 M $5303 M $5304 M -> $b3 $a9 $02 }T
$857d >PC $4d >S $e0 >A $0a >X $fd >Y $62 >P $857d $b3 >M $857e $0e >M $857f $18 >M
T{ op PC S A X Y P -> $857e $4d $e0 $0a $fd $62 }T T{ $857d M $857e M $857f M -> $b3 $0e $18 }T
$07fa >PC $61 >S $db >A $bd >X $35 >Y $26 >P $07fa $b3 >M $07fb $ce >M $07fc $da >M
T{ op PC S A X Y P -> $07fb $61 $db $bd $35 $26 }T T{ $07fa M $07fb M $07fc M -> $b3 $ce $da }T
$7a63 >PC $28 >S $0b >A $2f >X $98 >Y $e4 >P $7a63 $b3 >M $7a64 $e5 >M $7a65 $57 >M
T{ op PC S A X Y P -> $7a64 $28 $0b $2f $98 $e4 }T T{ $7a63 M $7a64 M $7a65 M -> $b3 $e5 $57 }T
( b4 )
$0dc0 >PC $3f >S $50 >A $c6 >X $63 >Y $a2 >P $009e $fa >M $00d8 $f3 >M $0dc0 $b4 >M $0dc1 $d8 >M $0dc2 $47 >M
T{ op PC S A X Y P -> $0dc2 $3f $50 $c6 $fa $a0 }T T{ $009e M $00d8 M $0dc0 M $0dc1 M $0dc2 M -> $fa $f3 $b4 $d8 $47 }T
$0501 >PC $44 >S $67 >A $2d >X $16 >Y $61 >P $0059 $ca >M $0086 $78 >M $0501 $b4 >M $0502 $59 >M $0503 $d2 >M
T{ op PC S A X Y P -> $0503 $44 $67 $2d $78 $61 }T T{ $0059 M $0086 M $0501 M $0502 M $0503 M -> $ca $78 $b4 $59 $d2 }T
$2bed >PC $6e >S $fa >A $af >X $bf >Y $a0 >P $00ae $cd >M $00ff $03 >M $2bed $b4 >M $2bee $ff >M $2bef $1b >M
T{ op PC S A X Y P -> $2bef $6e $fa $af $cd $a0 }T T{ $00ae M $00ff M $2bed M $2bee M $2bef M -> $cd $03 $b4 $ff $1b }T
$a0f4 >PC $52 >S $d6 >A $32 >X $64 >Y $60 >P $004a $a8 >M $007c $99 >M $a0f4 $b4 >M $a0f5 $4a >M $a0f6 $12 >M
T{ op PC S A X Y P -> $a0f6 $52 $d6 $32 $99 $e0 }T T{ $004a M $007c M $a0f4 M $a0f5 M $a0f6 M -> $a8 $99 $b4 $4a $12 }T
$1ef6 >PC $53 >S $77 >A $ca >X $7f >Y $64 >P $008d $18 >M $00c3 $f0 >M $1ef6 $b4 >M $1ef7 $c3 >M $1ef8 $0e >M
T{ op PC S A X Y P -> $1ef8 $53 $77 $ca $18 $64 }T T{ $008d M $00c3 M $1ef6 M $1ef7 M $1ef8 M -> $18 $f0 $b4 $c3 $0e }T
$2637 >PC $3e >S $2c >A $bd >X $d3 >Y $20 >P $0066 $50 >M $00a9 $71 >M $2637 $b4 >M $2638 $a9 >M $2639 $bd >M
T{ op PC S A X Y P -> $2639 $3e $2c $bd $50 $20 }T T{ $0066 M $00a9 M $2637 M $2638 M $2639 M -> $50 $71 $b4 $a9 $bd }T
$02ef >PC $8f >S $9e >A $fc >X $d9 >Y $21 >P $006d $0c >M $0071 $d3 >M $02ef $b4 >M $02f0 $71 >M $02f1 $17 >M
T{ op PC S A X Y P -> $02f1 $8f $9e $fc $0c $21 }T T{ $006d M $0071 M $02ef M $02f0 M $02f1 M -> $0c $d3 $b4 $71 $17 }T
$957a >PC $ec >S $e6 >A $bb >X $d3 >Y $65 >P $0060 $7f >M $00a5 $ae >M $957a $b4 >M $957b $a5 >M $957c $a8 >M
T{ op PC S A X Y P -> $957c $ec $e6 $bb $7f $65 }T T{ $0060 M $00a5 M $957a M $957b M $957c M -> $7f $ae $b4 $a5 $a8 }T
$47b3 >PC $f9 >S $0e >A $9b >X $8d >Y $a3 >P $0027 $2d >M $00c2 $31 >M $47b3 $b4 >M $47b4 $27 >M $47b5 $60 >M
T{ op PC S A X Y P -> $47b5 $f9 $0e $9b $31 $21 }T T{ $0027 M $00c2 M $47b3 M $47b4 M $47b5 M -> $2d $31 $b4 $27 $60 }T
$eab2 >PC $f4 >S $65 >A $eb >X $85 >Y $a3 >P $007e $6a >M $0093 $33 >M $eab2 $b4 >M $eab3 $93 >M $eab4 $1d >M
T{ op PC S A X Y P -> $eab4 $f4 $65 $eb $6a $21 }T T{ $007e M $0093 M $eab2 M $eab3 M $eab4 M -> $6a $33 $b4 $93 $1d }T
$90fc >PC $b0 >S $3c >A $08 >X $c0 >Y $a0 >P $0079 $60 >M $0081 $d0 >M $90fc $b4 >M $90fd $79 >M $90fe $81 >M
T{ op PC S A X Y P -> $90fe $b0 $3c $08 $d0 $a0 }T T{ $0079 M $0081 M $90fc M $90fd M $90fe M -> $60 $d0 $b4 $79 $81 }T
$94e2 >PC $39 >S $f9 >A $c1 >X $c9 >Y $27 >P $005a $f2 >M $0099 $c0 >M $94e2 $b4 >M $94e3 $99 >M $94e4 $75 >M
T{ op PC S A X Y P -> $94e4 $39 $f9 $c1 $f2 $a5 }T T{ $005a M $0099 M $94e2 M $94e3 M $94e4 M -> $f2 $c0 $b4 $99 $75 }T
$2708 >PC $20 >S $03 >A $62 >X $fc >Y $24 >P $0061 $98 >M $00ff $3c >M $2708 $b4 >M $2709 $ff >M $270a $ff >M
T{ op PC S A X Y P -> $270a $20 $03 $62 $98 $a4 }T T{ $0061 M $00ff M $2708 M $2709 M $270a M -> $98 $3c $b4 $ff $ff }T
$ee4d >PC $b1 >S $27 >A $30 >X $17 >Y $20 >P $000c $9d >M $00dc $cc >M $ee4d $b4 >M $ee4e $dc >M $ee4f $e9 >M
T{ op PC S A X Y P -> $ee4f $b1 $27 $30 $9d $a0 }T T{ $000c M $00dc M $ee4d M $ee4e M $ee4f M -> $9d $cc $b4 $dc $e9 }T
$4322 >PC $9c >S $a0 >A $ef >X $0f >Y $65 >P $0063 $c5 >M $0074 $fc >M $4322 $b4 >M $4323 $74 >M $4324 $f5 >M
T{ op PC S A X Y P -> $4324 $9c $a0 $ef $c5 $e5 }T T{ $0063 M $0074 M $4322 M $4323 M $4324 M -> $c5 $fc $b4 $74 $f5 }T
$f040 >PC $1a >S $76 >A $ee >X $a9 >Y $21 >P $0052 $87 >M $0064 $1d >M $f040 $b4 >M $f041 $64 >M $f042 $b9 >M
T{ op PC S A X Y P -> $f042 $1a $76 $ee $87 $a1 }T T{ $0052 M $0064 M $f040 M $f041 M $f042 M -> $87 $1d $b4 $64 $b9 }T
( b5 )
$8128 >PC $e0 >S $f5 >A $27 >X $14 >Y $e6 >P $0018 $e0 >M $003f $15 >M $8128 $b5 >M $8129 $18 >M $812a $26 >M
T{ op PC S A X Y P -> $812a $e0 $15 $27 $14 $64 }T T{ $0018 M $003f M $8128 M $8129 M $812a M -> $e0 $15 $b5 $18 $26 }T
$aca4 >PC $fb >S $90 >A $4a >X $bc >Y $e0 >P $0094 $6c >M $00de $d8 >M $aca4 $b5 >M $aca5 $94 >M $aca6 $44 >M
T{ op PC S A X Y P -> $aca6 $fb $d8 $4a $bc $e0 }T T{ $0094 M $00de M $aca4 M $aca5 M $aca6 M -> $6c $d8 $b5 $94 $44 }T
$0e6c >PC $d7 >S $d6 >A $e4 >X $a9 >Y $a5 >P $0000 $7b >M $001c $b4 >M $0e6c $b5 >M $0e6d $1c >M $0e6e $96 >M
T{ op PC S A X Y P -> $0e6e $d7 $7b $e4 $a9 $25 }T T{ $0000 M $001c M $0e6c M $0e6d M $0e6e M -> $7b $b4 $b5 $1c $96 }T
$0777 >PC $66 >S $a2 >A $e6 >X $34 >Y $21 >P $000b $af >M $0025 $2b >M $0777 $b5 >M $0778 $25 >M $0779 $c3 >M
T{ op PC S A X Y P -> $0779 $66 $af $e6 $34 $a1 }T T{ $000b M $0025 M $0777 M $0778 M $0779 M -> $af $2b $b5 $25 $c3 }T
$55d3 >PC $ca >S $18 >A $0d >X $c0 >Y $62 >P $000c $9e >M $0019 $59 >M $55d3 $b5 >M $55d4 $0c >M $55d5 $8e >M
T{ op PC S A X Y P -> $55d5 $ca $59 $0d $c0 $60 }T T{ $000c M $0019 M $55d3 M $55d4 M $55d5 M -> $9e $59 $b5 $0c $8e }T
$b60a >PC $07 >S $60 >A $f2 >X $87 >Y $64 >P $004c $51 >M $005a $29 >M $b60a $b5 >M $b60b $5a >M $b60c $14 >M
T{ op PC S A X Y P -> $b60c $07 $51 $f2 $87 $64 }T T{ $004c M $005a M $b60a M $b60b M $b60c M -> $51 $29 $b5 $5a $14 }T
$8317 >PC $2b >S $ef >A $79 >X $57 >Y $e6 >P $0075 $ec >M $00fc $0c >M $8317 $b5 >M $8318 $fc >M $8319 $32 >M
T{ op PC S A X Y P -> $8319 $2b $ec $79 $57 $e4 }T T{ $0075 M $00fc M $8317 M $8318 M $8319 M -> $ec $0c $b5 $fc $32 }T
$e540 >PC $7b >S $10 >A $d5 >X $ff >Y $e7 >P $008f $1d >M $00ba $65 >M $e540 $b5 >M $e541 $ba >M $e542 $49 >M
T{ op PC S A X Y P -> $e542 $7b $1d $d5 $ff $65 }T T{ $008f M $00ba M $e540 M $e541 M $e542 M -> $1d $65 $b5 $ba $49 }T
$c2aa >PC $75 >S $97 >A $93 >X $43 >Y $60 >P $0041 $60 >M $00d4 $08 >M $c2aa $b5 >M $c2ab $41 >M $c2ac $d3 >M
T{ op PC S A X Y P -> $c2ac $75 $08 $93 $43 $60 }T T{ $0041 M $00d4 M $c2aa M $c2ab M $c2ac M -> $60 $08 $b5 $41 $d3 }T
$6865 >PC $76 >S $29 >A $4e >X $88 >Y $e0 >P $0032 $b2 >M $0080 $a6 >M $6865 $b5 >M $6866 $32 >M $6867 $f1 >M
T{ op PC S A X Y P -> $6867 $76 $a6 $4e $88 $e0 }T T{ $0032 M $0080 M $6865 M $6866 M $6867 M -> $b2 $a6 $b5 $32 $f1 }T
$6a45 >PC $6e >S $e1 >A $8b >X $ba >Y $61 >P $0049 $54 >M $00d4 $64 >M $6a45 $b5 >M $6a46 $49 >M $6a47 $f8 >M
T{ op PC S A X Y P -> $6a47 $6e $64 $8b $ba $61 }T T{ $0049 M $00d4 M $6a45 M $6a46 M $6a47 M -> $54 $64 $b5 $49 $f8 }T
$8a48 >PC $58 >S $db >A $99 >X $01 >Y $22 >P $005a $71 >M $00c1 $8e >M $8a48 $b5 >M $8a49 $c1 >M $8a4a $4d >M
T{ op PC S A X Y P -> $8a4a $58 $71 $99 $01 $20 }T T{ $005a M $00c1 M $8a48 M $8a49 M $8a4a M -> $71 $8e $b5 $c1 $4d }T
$e8fb >PC $22 >S $1c >A $d9 >X $d9 >Y $a0 >P $001b $46 >M $0042 $28 >M $e8fb $b5 >M $e8fc $42 >M $e8fd $9f >M
T{ op PC S A X Y P -> $e8fd $22 $46 $d9 $d9 $20 }T T{ $001b M $0042 M $e8fb M $e8fc M $e8fd M -> $46 $28 $b5 $42 $9f }T
$8794 >PC $04 >S $f6 >A $e3 >X $70 >Y $26 >P $0002 $d7 >M $001f $e5 >M $8794 $b5 >M $8795 $1f >M $8796 $f5 >M
T{ op PC S A X Y P -> $8796 $04 $d7 $e3 $70 $a4 }T T{ $0002 M $001f M $8794 M $8795 M $8796 M -> $d7 $e5 $b5 $1f $f5 }T
$b40e >PC $75 >S $c0 >A $07 >X $f7 >Y $a6 >P $00e3 $dd >M $00ea $d0 >M $b40e $b5 >M $b40f $e3 >M $b410 $7a >M
T{ op PC S A X Y P -> $b410 $75 $d0 $07 $f7 $a4 }T T{ $00e3 M $00ea M $b40e M $b40f M $b410 M -> $dd $d0 $b5 $e3 $7a }T
$7ec0 >PC $65 >S $06 >A $a8 >X $b2 >Y $67 >P $0091 $fa >M $00e9 $2f >M $7ec0 $b5 >M $7ec1 $e9 >M $7ec2 $8c >M
T{ op PC S A X Y P -> $7ec2 $65 $fa $a8 $b2 $e5 }T T{ $0091 M $00e9 M $7ec0 M $7ec1 M $7ec2 M -> $fa $2f $b5 $e9 $8c }T
( b6 )
$b9de >PC $44 >S $5b >A $b8 >X $01 >Y $20 >P $00e9 $73 >M $00ea $19 >M $b9de $b6 >M $b9df $e9 >M $b9e0 $dd >M
T{ op PC S A X Y P -> $b9e0 $44 $5b $19 $01 $20 }T T{ $00e9 M $00ea M $b9de M $b9df M $b9e0 M -> $73 $19 $b6 $e9 $dd }T
$6e36 >PC $83 >S $21 >A $21 >X $11 >Y $63 >P $0001 $0c >M $00f0 $df >M $6e36 $b6 >M $6e37 $f0 >M $6e38 $9a >M
T{ op PC S A X Y P -> $6e38 $83 $21 $0c $11 $61 }T T{ $0001 M $00f0 M $6e36 M $6e37 M $6e38 M -> $0c $df $b6 $f0 $9a }T
$1701 >PC $31 >S $b0 >A $9a >X $95 >Y $66 >P $0061 $e9 >M $00f6 $7a >M $1701 $b6 >M $1702 $61 >M $1703 $94 >M
T{ op PC S A X Y P -> $1703 $31 $b0 $7a $95 $64 }T T{ $0061 M $00f6 M $1701 M $1702 M $1703 M -> $e9 $7a $b6 $61 $94 }T
$8f0c >PC $7f >S $29 >A $7d >X $46 >Y $e1 >P $00b4 $fc >M $00fa $c5 >M $8f0c $b6 >M $8f0d $b4 >M $8f0e $e6 >M
T{ op PC S A X Y P -> $8f0e $7f $29 $c5 $46 $e1 }T T{ $00b4 M $00fa M $8f0c M $8f0d M $8f0e M -> $fc $c5 $b6 $b4 $e6 }T
$70d6 >PC $3d >S $9b >A $75 >X $f7 >Y $26 >P $003c $9f >M $0045 $88 >M $70d6 $b6 >M $70d7 $45 >M $70d8 $c5 >M
T{ op PC S A X Y P -> $70d8 $3d $9b $9f $f7 $a4 }T T{ $003c M $0045 M $70d6 M $70d7 M $70d8 M -> $9f $88 $b6 $45 $c5 }T
$af2e >PC $a2 >S $af >A $0d >X $b8 >Y $22 >P $0013 $8e >M $00cb $62 >M $af2e $b6 >M $af2f $13 >M $af30 $0a >M
T{ op PC S A X Y P -> $af30 $a2 $af $62 $b8 $20 }T T{ $0013 M $00cb M $af2e M $af2f M $af30 M -> $8e $62 $b6 $13 $0a }T
$f2b9 >PC $37 >S $e5 >A $60 >X $d6 >Y $a0 >P $000e $17 >M $00e4 $80 >M $f2b9 $b6 >M $f2ba $0e >M $f2bb $a3 >M
T{ op PC S A X Y P -> $f2bb $37 $e5 $80 $d6 $a0 }T T{ $000e M $00e4 M $f2b9 M $f2ba M $f2bb M -> $17 $80 $b6 $0e $a3 }T
$f716 >PC $f3 >S $a4 >A $ff >X $63 >Y $62 >P $0000 $d8 >M $0063 $94 >M $f716 $b6 >M $f717 $00 >M $f718 $05 >M
T{ op PC S A X Y P -> $f718 $f3 $a4 $94 $63 $e0 }T T{ $0000 M $0063 M $f716 M $f717 M $f718 M -> $d8 $94 $b6 $00 $05 }T
$ed71 >PC $77 >S $31 >A $23 >X $88 >Y $24 >P $0052 $9a >M $00ca $e2 >M $ed71 $b6 >M $ed72 $ca >M $ed73 $d9 >M
T{ op PC S A X Y P -> $ed73 $77 $31 $9a $88 $a4 }T T{ $0052 M $00ca M $ed71 M $ed72 M $ed73 M -> $9a $e2 $b6 $ca $d9 }T
$74fa >PC $86 >S $df >A $73 >X $c3 >Y $26 >P $0012 $8e >M $00d5 $ca >M $74fa $b6 >M $74fb $12 >M $74fc $33 >M
T{ op PC S A X Y P -> $74fc $86 $df $ca $c3 $a4 }T T{ $0012 M $00d5 M $74fa M $74fb M $74fc M -> $8e $ca $b6 $12 $33 }T
$5556 >PC $f7 >S $b1 >A $67 >X $d2 >Y $63 >P $00bb $32 >M $00e9 $8e >M $5556 $b6 >M $5557 $e9 >M $5558 $60 >M
T{ op PC S A X Y P -> $5558 $f7 $b1 $32 $d2 $61 }T T{ $00bb M $00e9 M $5556 M $5557 M $5558 M -> $32 $8e $b6 $e9 $60 }T
$e786 >PC $ed >S $d9 >A $84 >X $1e >Y $a4 >P $001a $29 >M $0038 $1c >M $e786 $b6 >M $e787 $1a >M $e788 $64 >M
T{ op PC S A X Y P -> $e788 $ed $d9 $1c $1e $24 }T T{ $001a M $0038 M $e786 M $e787 M $e788 M -> $29 $1c $b6 $1a $64 }T
$cc55 >PC $d7 >S $08 >A $f1 >X $46 >Y $a5 >P $00a7 $b9 >M $00ed $fb >M $cc55 $b6 >M $cc56 $a7 >M $cc57 $b5 >M
T{ op PC S A X Y P -> $cc57 $d7 $08 $fb $46 $a5 }T T{ $00a7 M $00ed M $cc55 M $cc56 M $cc57 M -> $b9 $fb $b6 $a7 $b5 }T
$28e2 >PC $45 >S $ce >A $61 >X $41 >Y $a5 >P $0011 $d5 >M $0052 $dc >M $28e2 $b6 >M $28e3 $11 >M $28e4 $3c >M
T{ op PC S A X Y P -> $28e4 $45 $ce $dc $41 $a5 }T T{ $0011 M $0052 M $28e2 M $28e3 M $28e4 M -> $d5 $dc $b6 $11 $3c }T
$122c >PC $b8 >S $3e >A $c4 >X $d5 >Y $61 >P $0032 $25 >M $005d $43 >M $122c $b6 >M $122d $5d >M $122e $7e >M
T{ op PC S A X Y P -> $122e $b8 $3e $25 $d5 $61 }T T{ $0032 M $005d M $122c M $122d M $122e M -> $25 $43 $b6 $5d $7e }T
$861e >PC $34 >S $d5 >A $07 >X $77 >Y $21 >P $0087 $27 >M $00fe $95 >M $861e $b6 >M $861f $87 >M $8620 $a3 >M
T{ op PC S A X Y P -> $8620 $34 $d5 $95 $77 $a1 }T T{ $0087 M $00fe M $861e M $861f M $8620 M -> $27 $95 $b6 $87 $a3 }T
( b7 )
$e907 >PC $c1 >S $aa >A $6a >X $16 >Y $65 >P $00aa $58 >M $e907 $b7 >M $e908 $aa >M $e909 $60 >M
T{ op PC S A X Y P -> $e909 $c1 $aa $6a $16 $65 }T T{ $00aa M $e907 M $e908 M $e909 M -> $58 $b7 $aa $60 }T
$9c00 >PC $95 >S $c7 >A $23 >X $1e >Y $20 >P $00dc $b6 >M $9c00 $b7 >M $9c01 $dc >M $9c02 $40 >M
T{ op PC S A X Y P -> $9c02 $95 $c7 $23 $1e $20 }T T{ $00dc M $9c00 M $9c01 M $9c02 M -> $be $b7 $dc $40 }T
$698e >PC $10 >S $ec >A $ba >X $28 >Y $e4 >P $00c2 $cb >M $698e $b7 >M $698f $c2 >M $6990 $9e >M
T{ op PC S A X Y P -> $6990 $10 $ec $ba $28 $e4 }T T{ $00c2 M $698e M $698f M $6990 M -> $cb $b7 $c2 $9e }T
$131d >PC $57 >S $9d >A $9e >X $41 >Y $a3 >P $0082 $1b >M $131d $b7 >M $131e $82 >M $131f $ee >M
T{ op PC S A X Y P -> $131f $57 $9d $9e $41 $a3 }T T{ $0082 M $131d M $131e M $131f M -> $1b $b7 $82 $ee }T
$dbd7 >PC $f6 >S $41 >A $5a >X $6d >Y $e1 >P $003f $5f >M $dbd7 $b7 >M $dbd8 $3f >M $dbd9 $78 >M
T{ op PC S A X Y P -> $dbd9 $f6 $41 $5a $6d $e1 }T T{ $003f M $dbd7 M $dbd8 M $dbd9 M -> $5f $b7 $3f $78 }T
$97fa >PC $8e >S $5d >A $6d >X $14 >Y $60 >P $0076 $5a >M $97fa $b7 >M $97fb $76 >M $97fc $7e >M
T{ op PC S A X Y P -> $97fc $8e $5d $6d $14 $60 }T T{ $0076 M $97fa M $97fb M $97fc M -> $5a $b7 $76 $7e }T
$0a49 >PC $18 >S $dc >A $ea >X $b9 >Y $e2 >P $00fc $4e >M $0a49 $b7 >M $0a4a $fc >M $0a4b $6e >M
T{ op PC S A X Y P -> $0a4b $18 $dc $ea $b9 $e2 }T T{ $00fc M $0a49 M $0a4a M $0a4b M -> $4e $b7 $fc $6e }T
$8904 >PC $8a >S $81 >A $65 >X $24 >Y $20 >P $008b $d5 >M $8904 $b7 >M $8905 $8b >M $8906 $fe >M
T{ op PC S A X Y P -> $8906 $8a $81 $65 $24 $20 }T T{ $008b M $8904 M $8905 M $8906 M -> $dd $b7 $8b $fe }T
$48b0 >PC $36 >S $46 >A $b5 >X $d6 >Y $e2 >P $00c0 $16 >M $48b0 $b7 >M $48b1 $c0 >M $48b2 $ad >M
T{ op PC S A X Y P -> $48b2 $36 $46 $b5 $d6 $e2 }T T{ $00c0 M $48b0 M $48b1 M $48b2 M -> $1e $b7 $c0 $ad }T
$c164 >PC $78 >S $b5 >A $bd >X $f2 >Y $e6 >P $00e6 $fb >M $c164 $b7 >M $c165 $e6 >M $c166 $47 >M
T{ op PC S A X Y P -> $c166 $78 $b5 $bd $f2 $e6 }T T{ $00e6 M $c164 M $c165 M $c166 M -> $fb $b7 $e6 $47 }T
$3053 >PC $2b >S $70 >A $53 >X $82 >Y $e3 >P $001f $c9 >M $3053 $b7 >M $3054 $1f >M $3055 $0e >M
T{ op PC S A X Y P -> $3055 $2b $70 $53 $82 $e3 }T T{ $001f M $3053 M $3054 M $3055 M -> $c9 $b7 $1f $0e }T
$829a >PC $fc >S $ee >A $05 >X $9f >Y $a7 >P $0079 $5d >M $829a $b7 >M $829b $79 >M $829c $45 >M
T{ op PC S A X Y P -> $829c $fc $ee $05 $9f $a7 }T T{ $0079 M $829a M $829b M $829c M -> $5d $b7 $79 $45 }T
$3982 >PC $91 >S $99 >A $3c >X $88 >Y $63 >P $0013 $22 >M $3982 $b7 >M $3983 $13 >M $3984 $79 >M
T{ op PC S A X Y P -> $3984 $91 $99 $3c $88 $63 }T T{ $0013 M $3982 M $3983 M $3984 M -> $2a $b7 $13 $79 }T
$a123 >PC $9c >S $89 >A $8a >X $c0 >Y $a6 >P $00a3 $01 >M $a123 $b7 >M $a124 $a3 >M $a125 $32 >M
T{ op PC S A X Y P -> $a125 $9c $89 $8a $c0 $a6 }T T{ $00a3 M $a123 M $a124 M $a125 M -> $09 $b7 $a3 $32 }T
$7c7f >PC $15 >S $89 >A $5a >X $19 >Y $a4 >P $006f $ff >M $7c7f $b7 >M $7c80 $6f >M $7c81 $c5 >M
T{ op PC S A X Y P -> $7c81 $15 $89 $5a $19 $a4 }T T{ $006f M $7c7f M $7c80 M $7c81 M -> $ff $b7 $6f $c5 }T
$129c >PC $b1 >S $8f >A $ba >X $c1 >Y $a7 >P $00cf $05 >M $129c $b7 >M $129d $cf >M $129e $2e >M
T{ op PC S A X Y P -> $129e $b1 $8f $ba $c1 $a7 }T T{ $00cf M $129c M $129d M $129e M -> $0d $b7 $cf $2e }T
( b8 )
$fee3 >PC $15 >S $db >A $48 >X $ba >Y $63 >P $fee3 $b8 >M $fee4 $92 >M $fee5 $4f >M
T{ op PC S A X Y P -> $fee4 $15 $db $48 $ba $23 }T T{ $fee3 M $fee4 M $fee5 M -> $b8 $92 $4f }T
$cc56 >PC $d9 >S $55 >A $b9 >X $34 >Y $e0 >P $cc56 $b8 >M $cc57 $99 >M $cc58 $1a >M
T{ op PC S A X Y P -> $cc57 $d9 $55 $b9 $34 $a0 }T T{ $cc56 M $cc57 M $cc58 M -> $b8 $99 $1a }T
$5bbf >PC $85 >S $d8 >A $68 >X $80 >Y $a4 >P $5bbf $b8 >M $5bc0 $5d >M $5bc1 $15 >M
T{ op PC S A X Y P -> $5bc0 $85 $d8 $68 $80 $a4 }T T{ $5bbf M $5bc0 M $5bc1 M -> $b8 $5d $15 }T
$f58a >PC $4c >S $18 >A $ca >X $b6 >Y $a5 >P $f58a $b8 >M $f58b $8b >M $f58c $76 >M
T{ op PC S A X Y P -> $f58b $4c $18 $ca $b6 $a5 }T T{ $f58a M $f58b M $f58c M -> $b8 $8b $76 }T
$32e1 >PC $4e >S $d2 >A $53 >X $7f >Y $62 >P $32e1 $b8 >M $32e2 $76 >M $32e3 $03 >M
T{ op PC S A X Y P -> $32e2 $4e $d2 $53 $7f $22 }T T{ $32e1 M $32e2 M $32e3 M -> $b8 $76 $03 }T
$f36a >PC $32 >S $27 >A $4c >X $e8 >Y $60 >P $f36a $b8 >M $f36b $ab >M $f36c $81 >M
T{ op PC S A X Y P -> $f36b $32 $27 $4c $e8 $20 }T T{ $f36a M $f36b M $f36c M -> $b8 $ab $81 }T
$10b5 >PC $8c >S $7c >A $80 >X $a5 >Y $65 >P $10b5 $b8 >M $10b6 $5e >M $10b7 $19 >M
T{ op PC S A X Y P -> $10b6 $8c $7c $80 $a5 $25 }T T{ $10b5 M $10b6 M $10b7 M -> $b8 $5e $19 }T
$601a >PC $3b >S $21 >A $8d >X $ee >Y $63 >P $601a $b8 >M $601b $07 >M $601c $52 >M
T{ op PC S A X Y P -> $601b $3b $21 $8d $ee $23 }T T{ $601a M $601b M $601c M -> $b8 $07 $52 }T
$2d22 >PC $c4 >S $67 >A $55 >X $4b >Y $e6 >P $2d22 $b8 >M $2d23 $30 >M $2d24 $d2 >M
T{ op PC S A X Y P -> $2d23 $c4 $67 $55 $4b $a6 }T T{ $2d22 M $2d23 M $2d24 M -> $b8 $30 $d2 }T
$c385 >PC $a3 >S $7d >A $83 >X $4c >Y $e7 >P $c385 $b8 >M $c386 $07 >M $c387 $11 >M
T{ op PC S A X Y P -> $c386 $a3 $7d $83 $4c $a7 }T T{ $c385 M $c386 M $c387 M -> $b8 $07 $11 }T
$2f16 >PC $c3 >S $99 >A $f9 >X $29 >Y $e0 >P $2f16 $b8 >M $2f17 $e6 >M $2f18 $84 >M
T{ op PC S A X Y P -> $2f17 $c3 $99 $f9 $29 $a0 }T T{ $2f16 M $2f17 M $2f18 M -> $b8 $e6 $84 }T
$1621 >PC $c6 >S $c8 >A $a3 >X $de >Y $a2 >P $1621 $b8 >M $1622 $71 >M $1623 $27 >M
T{ op PC S A X Y P -> $1622 $c6 $c8 $a3 $de $a2 }T T{ $1621 M $1622 M $1623 M -> $b8 $71 $27 }T
$cc71 >PC $f2 >S $c5 >A $82 >X $d6 >Y $a6 >P $cc71 $b8 >M $cc72 $65 >M $cc73 $c6 >M
T{ op PC S A X Y P -> $cc72 $f2 $c5 $82 $d6 $a6 }T T{ $cc71 M $cc72 M $cc73 M -> $b8 $65 $c6 }T
$71c2 >PC $08 >S $8b >A $cd >X $68 >Y $67 >P $71c2 $b8 >M $71c3 $67 >M $71c4 $84 >M
T{ op PC S A X Y P -> $71c3 $08 $8b $cd $68 $27 }T T{ $71c2 M $71c3 M $71c4 M -> $b8 $67 $84 }T
$b407 >PC $60 >S $65 >A $01 >X $95 >Y $e2 >P $b407 $b8 >M $b408 $9a >M $b409 $2b >M
T{ op PC S A X Y P -> $b408 $60 $65 $01 $95 $a2 }T T{ $b407 M $b408 M $b409 M -> $b8 $9a $2b }T
$ea13 >PC $e4 >S $f5 >A $86 >X $a5 >Y $26 >P $ea13 $b8 >M $ea14 $97 >M $ea15 $ed >M
T{ op PC S A X Y P -> $ea14 $e4 $f5 $86 $a5 $26 }T T{ $ea13 M $ea14 M $ea15 M -> $b8 $97 $ed }T
( b9 )
$60fa >PC $1c >S $52 >A $7a >X $92 >Y $e3 >P $55f2 $65 >M $60fa $b9 >M $60fb $60 >M $60fc $55 >M $60fd $9e >M
T{ op PC S A X Y P -> $60fd $1c $65 $7a $92 $61 }T T{ $55f2 M $60fa M $60fb M $60fc M $60fd M -> $65 $b9 $60 $55 $9e }T
$7650 >PC $15 >S $91 >A $0b >X $1c >Y $e3 >P $1969 $69 >M $7650 $b9 >M $7651 $4d >M $7652 $19 >M $7653 $ad >M
T{ op PC S A X Y P -> $7653 $15 $69 $0b $1c $61 }T T{ $1969 M $7650 M $7651 M $7652 M $7653 M -> $69 $b9 $4d $19 $ad }T
$ddb6 >PC $76 >S $75 >A $55 >X $67 >Y $67 >P $ce39 $d3 >M $ddb6 $b9 >M $ddb7 $d2 >M $ddb8 $cd >M $ddb9 $c5 >M
T{ op PC S A X Y P -> $ddb9 $76 $d3 $55 $67 $e5 }T T{ $ce39 M $ddb6 M $ddb7 M $ddb8 M $ddb9 M -> $d3 $b9 $d2 $cd $c5 }T
$586d >PC $b7 >S $a4 >A $c8 >X $c8 >Y $66 >P $098e $05 >M $586d $b9 >M $586e $c6 >M $586f $08 >M $5870 $aa >M
T{ op PC S A X Y P -> $5870 $b7 $05 $c8 $c8 $64 }T T{ $098e M $586d M $586e M $586f M $5870 M -> $05 $b9 $c6 $08 $aa }T
$c27f >PC $7a >S $35 >A $78 >X $4e >Y $a3 >P $ac3d $30 >M $c27f $b9 >M $c280 $ef >M $c281 $ab >M $c282 $4f >M
T{ op PC S A X Y P -> $c282 $7a $30 $78 $4e $21 }T T{ $ac3d M $c27f M $c280 M $c281 M $c282 M -> $30 $b9 $ef $ab $4f }T
$2c92 >PC $9c >S $97 >A $c0 >X $89 >Y $26 >P $2c92 $b9 >M $2c93 $0f >M $2c94 $9a >M $2c95 $04 >M $9a98 $50 >M
T{ op PC S A X Y P -> $2c95 $9c $50 $c0 $89 $24 }T T{ $2c92 M $2c93 M $2c94 M $2c95 M $9a98 M -> $b9 $0f $9a $04 $50 }T
$a577 >PC $27 >S $9e >A $2e >X $3e >Y $66 >P $5b60 $88 >M $a577 $b9 >M $a578 $22 >M $a579 $5b >M $a57a $be >M
T{ op PC S A X Y P -> $a57a $27 $88 $2e $3e $e4 }T T{ $5b60 M $a577 M $a578 M $a579 M $a57a M -> $88 $b9 $22 $5b $be }T
$6216 >PC $ba >S $71 >A $b1 >X $4e >Y $a7 >P $6216 $b9 >M $6217 $cb >M $6218 $73 >M $6219 $56 >M $7419 $22 >M
T{ op PC S A X Y P -> $6219 $ba $22 $b1 $4e $25 }T T{ $6216 M $6217 M $6218 M $6219 M $7419 M -> $b9 $cb $73 $56 $22 }T
$d216 >PC $0f >S $4a >A $b9 >X $c8 >Y $a4 >P $7c86 $38 >M $d216 $b9 >M $d217 $be >M $d218 $7b >M $d219 $0c >M
T{ op PC S A X Y P -> $d219 $0f $38 $b9 $c8 $24 }T T{ $7c86 M $d216 M $d217 M $d218 M $d219 M -> $38 $b9 $be $7b $0c }T
$d607 >PC $b1 >S $53 >A $51 >X $32 >Y $e4 >P $d607 $b9 >M $d608 $68 >M $d609 $f8 >M $d60a $82 >M $f89a $76 >M
T{ op PC S A X Y P -> $d60a $b1 $76 $51 $32 $64 }T T{ $d607 M $d608 M $d609 M $d60a M $f89a M -> $b9 $68 $f8 $82 $76 }T
$93cd >PC $b4 >S $da >A $31 >X $62 >Y $e1 >P $09f4 $2b >M $93cd $b9 >M $93ce $92 >M $93cf $09 >M $93d0 $57 >M
T{ op PC S A X Y P -> $93d0 $b4 $2b $31 $62 $61 }T T{ $09f4 M $93cd M $93ce M $93cf M $93d0 M -> $2b $b9 $92 $09 $57 }T
$87e0 >PC $32 >S $8e >A $a7 >X $da >Y $a7 >P $87e0 $b9 >M $87e1 $ef >M $87e2 $df >M $87e3 $10 >M $e0c9 $bc >M
T{ op PC S A X Y P -> $87e3 $32 $bc $a7 $da $a5 }T T{ $87e0 M $87e1 M $87e2 M $87e3 M $e0c9 M -> $b9 $ef $df $10 $bc }T
$f7f0 >PC $d7 >S $fc >A $40 >X $49 >Y $e7 >P $7898 $20 >M $f7f0 $b9 >M $f7f1 $4f >M $f7f2 $78 >M $f7f3 $3b >M
T{ op PC S A X Y P -> $f7f3 $d7 $20 $40 $49 $65 }T T{ $7898 M $f7f0 M $f7f1 M $f7f2 M $f7f3 M -> $20 $b9 $4f $78 $3b }T
$276d >PC $5a >S $d6 >A $9d >X $10 >Y $20 >P $276d $b9 >M $276e $ed >M $276f $9c >M $2770 $dd >M $9cfd $c9 >M
T{ op PC S A X Y P -> $2770 $5a $c9 $9d $10 $a0 }T T{ $276d M $276e M $276f M $2770 M $9cfd M -> $b9 $ed $9c $dd $c9 }T
$b5eb >PC $8d >S $2d >A $0d >X $4d >Y $a3 >P $b5eb $b9 >M $b5ec $73 >M $b5ed $f5 >M $b5ee $07 >M $f5c0 $bb >M
T{ op PC S A X Y P -> $b5ee $8d $bb $0d $4d $a1 }T T{ $b5eb M $b5ec M $b5ed M $b5ee M $f5c0 M -> $b9 $73 $f5 $07 $bb }T
$800e >PC $67 >S $b0 >A $78 >X $bd >Y $a5 >P $2b70 $05 >M $800e $b9 >M $800f $b3 >M $8010 $2a >M $8011 $aa >M
T{ op PC S A X Y P -> $8011 $67 $05 $78 $bd $25 }T T{ $2b70 M $800e M $800f M $8010 M $8011 M -> $05 $b9 $b3 $2a $aa }T
( ba )
$49a4 >PC $20 >S $7b >A $5a >X $d7 >Y $22 >P $49a4 $ba >M $49a5 $97 >M $49a6 $d6 >M
T{ op PC S A X Y P -> $49a5 $20 $7b $20 $d7 $20 }T T{ $49a4 M $49a5 M $49a6 M -> $ba $97 $d6 }T
$27c6 >PC $86 >S $7f >A $b4 >X $b6 >Y $e6 >P $27c6 $ba >M $27c7 $b4 >M $27c8 $00 >M
T{ op PC S A X Y P -> $27c7 $86 $7f $86 $b6 $e4 }T T{ $27c6 M $27c7 M $27c8 M -> $ba $b4 $00 }T
$b135 >PC $b1 >S $ef >A $36 >X $e8 >Y $21 >P $b135 $ba >M $b136 $6f >M $b137 $55 >M
T{ op PC S A X Y P -> $b136 $b1 $ef $b1 $e8 $a1 }T T{ $b135 M $b136 M $b137 M -> $ba $6f $55 }T
$db6f >PC $97 >S $70 >A $30 >X $2b >Y $24 >P $db6f $ba >M $db70 $cf >M $db71 $fc >M
T{ op PC S A X Y P -> $db70 $97 $70 $97 $2b $a4 }T T{ $db6f M $db70 M $db71 M -> $ba $cf $fc }T
$247d >PC $da >S $c9 >A $75 >X $44 >Y $e5 >P $247d $ba >M $247e $30 >M $247f $37 >M
T{ op PC S A X Y P -> $247e $da $c9 $da $44 $e5 }T T{ $247d M $247e M $247f M -> $ba $30 $37 }T
$26f9 >PC $63 >S $61 >A $78 >X $2d >Y $e1 >P $26f9 $ba >M $26fa $db >M $26fb $a9 >M
T{ op PC S A X Y P -> $26fa $63 $61 $63 $2d $61 }T T{ $26f9 M $26fa M $26fb M -> $ba $db $a9 }T
$ea5e >PC $5d >S $26 >A $9f >X $9e >Y $a0 >P $ea5e $ba >M $ea5f $fc >M $ea60 $36 >M
T{ op PC S A X Y P -> $ea5f $5d $26 $5d $9e $20 }T T{ $ea5e M $ea5f M $ea60 M -> $ba $fc $36 }T
$3ffb >PC $2c >S $df >A $e3 >X $42 >Y $e4 >P $3ffb $ba >M $3ffc $d3 >M $3ffd $a9 >M
T{ op PC S A X Y P -> $3ffc $2c $df $2c $42 $64 }T T{ $3ffb M $3ffc M $3ffd M -> $ba $d3 $a9 }T
$cb4d >PC $26 >S $6b >A $33 >X $e4 >Y $a0 >P $cb4d $ba >M $cb4e $f2 >M $cb4f $77 >M
T{ op PC S A X Y P -> $cb4e $26 $6b $26 $e4 $20 }T T{ $cb4d M $cb4e M $cb4f M -> $ba $f2 $77 }T
$df0d >PC $48 >S $5f >A $4f >X $59 >Y $25 >P $df0d $ba >M $df0e $bf >M $df0f $f2 >M
T{ op PC S A X Y P -> $df0e $48 $5f $48 $59 $25 }T T{ $df0d M $df0e M $df0f M -> $ba $bf $f2 }T
$bb0b >PC $5e >S $5b >A $0e >X $d4 >Y $21 >P $bb0b $ba >M $bb0c $fa >M $bb0d $cc >M
T{ op PC S A X Y P -> $bb0c $5e $5b $5e $d4 $21 }T T{ $bb0b M $bb0c M $bb0d M -> $ba $fa $cc }T
$8a69 >PC $d4 >S $a3 >A $24 >X $ba >Y $64 >P $8a69 $ba >M $8a6a $92 >M $8a6b $5e >M
T{ op PC S A X Y P -> $8a6a $d4 $a3 $d4 $ba $e4 }T T{ $8a69 M $8a6a M $8a6b M -> $ba $92 $5e }T
$c5e4 >PC $1b >S $11 >A $b1 >X $9c >Y $21 >P $c5e4 $ba >M $c5e5 $85 >M $c5e6 $f8 >M
T{ op PC S A X Y P -> $c5e5 $1b $11 $1b $9c $21 }T T{ $c5e4 M $c5e5 M $c5e6 M -> $ba $85 $f8 }T
$4c6d >PC $61 >S $c7 >A $50 >X $e9 >Y $a3 >P $4c6d $ba >M $4c6e $ac >M $4c6f $a3 >M
T{ op PC S A X Y P -> $4c6e $61 $c7 $61 $e9 $21 }T T{ $4c6d M $4c6e M $4c6f M -> $ba $ac $a3 }T
$5ae7 >PC $ca >S $25 >A $17 >X $79 >Y $24 >P $5ae7 $ba >M $5ae8 $96 >M $5ae9 $45 >M
T{ op PC S A X Y P -> $5ae8 $ca $25 $ca $79 $a4 }T T{ $5ae7 M $5ae8 M $5ae9 M -> $ba $96 $45 }T
$8291 >PC $6d >S $ed >A $e4 >X $08 >Y $a4 >P $8291 $ba >M $8292 $d1 >M $8293 $d8 >M
T{ op PC S A X Y P -> $8292 $6d $ed $6d $08 $24 }T T{ $8291 M $8292 M $8293 M -> $ba $d1 $d8 }T
( bb )
$802c >PC $6b >S $a6 >A $0e >X $7a >Y $26 >P $802c $bb >M $802d $21 >M $802e $b5 >M
T{ op PC S A X Y P -> $802d $6b $a6 $0e $7a $26 }T T{ $802c M $802d M $802e M -> $bb $21 $b5 }T
$4b7c >PC $53 >S $c2 >A $6d >X $ad >Y $26 >P $4b7c $bb >M $4b7d $4d >M $4b7e $3f >M
T{ op PC S A X Y P -> $4b7d $53 $c2 $6d $ad $26 }T T{ $4b7c M $4b7d M $4b7e M -> $bb $4d $3f }T
$c582 >PC $db >S $47 >A $0c >X $37 >Y $a2 >P $c582 $bb >M $c583 $4c >M $c584 $7a >M
T{ op PC S A X Y P -> $c583 $db $47 $0c $37 $a2 }T T{ $c582 M $c583 M $c584 M -> $bb $4c $7a }T
$eba3 >PC $5b >S $94 >A $a1 >X $48 >Y $a4 >P $eba3 $bb >M $eba4 $02 >M $eba5 $55 >M
T{ op PC S A X Y P -> $eba4 $5b $94 $a1 $48 $a4 }T T{ $eba3 M $eba4 M $eba5 M -> $bb $02 $55 }T
$ce17 >PC $2c >S $f1 >A $4d >X $55 >Y $a1 >P $ce17 $bb >M $ce18 $5c >M $ce19 $d0 >M
T{ op PC S A X Y P -> $ce18 $2c $f1 $4d $55 $a1 }T T{ $ce17 M $ce18 M $ce19 M -> $bb $5c $d0 }T
$901d >PC $38 >S $bd >A $66 >X $e4 >Y $61 >P $901d $bb >M $901e $ed >M $901f $0c >M
T{ op PC S A X Y P -> $901e $38 $bd $66 $e4 $61 }T T{ $901d M $901e M $901f M -> $bb $ed $0c }T
$1681 >PC $4e >S $81 >A $3c >X $77 >Y $a2 >P $1681 $bb >M $1682 $ae >M $1683 $35 >M
T{ op PC S A X Y P -> $1682 $4e $81 $3c $77 $a2 }T T{ $1681 M $1682 M $1683 M -> $bb $ae $35 }T
$03b5 >PC $c0 >S $6d >A $9a >X $58 >Y $a6 >P $03b5 $bb >M $03b6 $35 >M $03b7 $5a >M
T{ op PC S A X Y P -> $03b6 $c0 $6d $9a $58 $a6 }T T{ $03b5 M $03b6 M $03b7 M -> $bb $35 $5a }T
$bb1e >PC $fb >S $9c >A $c8 >X $fa >Y $e2 >P $bb1e $bb >M $bb1f $39 >M $bb20 $de >M
T{ op PC S A X Y P -> $bb1f $fb $9c $c8 $fa $e2 }T T{ $bb1e M $bb1f M $bb20 M -> $bb $39 $de }T
$aac5 >PC $5c >S $f8 >A $d8 >X $9f >Y $a0 >P $aac5 $bb >M $aac6 $8f >M $aac7 $8d >M
T{ op PC S A X Y P -> $aac6 $5c $f8 $d8 $9f $a0 }T T{ $aac5 M $aac6 M $aac7 M -> $bb $8f $8d }T
$8b47 >PC $9d >S $66 >A $e6 >X $30 >Y $64 >P $8b47 $bb >M $8b48 $99 >M $8b49 $88 >M
T{ op PC S A X Y P -> $8b48 $9d $66 $e6 $30 $64 }T T{ $8b47 M $8b48 M $8b49 M -> $bb $99 $88 }T
$e995 >PC $1e >S $33 >A $18 >X $10 >Y $e3 >P $e995 $bb >M $e996 $4f >M $e997 $27 >M
T{ op PC S A X Y P -> $e996 $1e $33 $18 $10 $e3 }T T{ $e995 M $e996 M $e997 M -> $bb $4f $27 }T
$7612 >PC $26 >S $cb >A $91 >X $17 >Y $27 >P $7612 $bb >M $7613 $34 >M $7614 $c3 >M
T{ op PC S A X Y P -> $7613 $26 $cb $91 $17 $27 }T T{ $7612 M $7613 M $7614 M -> $bb $34 $c3 }T
$4678 >PC $f0 >S $fe >A $cb >X $c8 >Y $25 >P $4678 $bb >M $4679 $e2 >M $467a $d4 >M
T{ op PC S A X Y P -> $4679 $f0 $fe $cb $c8 $25 }T T{ $4678 M $4679 M $467a M -> $bb $e2 $d4 }T
$096a >PC $f2 >S $a9 >A $3f >X $6c >Y $60 >P $096a $bb >M $096b $41 >M $096c $40 >M
T{ op PC S A X Y P -> $096b $f2 $a9 $3f $6c $60 }T T{ $096a M $096b M $096c M -> $bb $41 $40 }T
$c149 >PC $a8 >S $5a >A $59 >X $45 >Y $20 >P $c149 $bb >M $c14a $31 >M $c14b $37 >M
T{ op PC S A X Y P -> $c14a $a8 $5a $59 $45 $20 }T T{ $c149 M $c14a M $c14b M -> $bb $31 $37 }T
( bc )
$d26b >PC $86 >S $a6 >A $a6 >X $43 >Y $64 >P $3598 $96 >M $d26b $bc >M $d26c $f2 >M $d26d $34 >M $d26e $f1 >M
T{ op PC S A X Y P -> $d26e $86 $a6 $a6 $96 $e4 }T T{ $3598 M $d26b M $d26c M $d26d M $d26e M -> $96 $bc $f2 $34 $f1 }T
$b04d >PC $27 >S $b6 >A $57 >X $07 >Y $a7 >P $5323 $ea >M $b04d $bc >M $b04e $cc >M $b04f $52 >M $b050 $70 >M
T{ op PC S A X Y P -> $b050 $27 $b6 $57 $ea $a5 }T T{ $5323 M $b04d M $b04e M $b04f M $b050 M -> $ea $bc $cc $52 $70 }T
$b249 >PC $1e >S $95 >A $9a >X $bd >Y $22 >P $a1b5 $30 >M $b249 $bc >M $b24a $1b >M $b24b $a1 >M $b24c $fd >M
T{ op PC S A X Y P -> $b24c $1e $95 $9a $30 $20 }T T{ $a1b5 M $b249 M $b24a M $b24b M $b24c M -> $30 $bc $1b $a1 $fd }T
$082f >PC $cd >S $9e >A $4f >X $31 >Y $e5 >P $082f $bc >M $0830 $b9 >M $0831 $b9 >M $0832 $9a >M $ba08 $93 >M
T{ op PC S A X Y P -> $0832 $cd $9e $4f $93 $e5 }T T{ $082f M $0830 M $0831 M $0832 M $ba08 M -> $bc $b9 $b9 $9a $93 }T
$e671 >PC $5d >S $4e >A $10 >X $de >Y $a7 >P $34e6 $a8 >M $e671 $bc >M $e672 $d6 >M $e673 $34 >M $e674 $2b >M
T{ op PC S A X Y P -> $e674 $5d $4e $10 $a8 $a5 }T T{ $34e6 M $e671 M $e672 M $e673 M $e674 M -> $a8 $bc $d6 $34 $2b }T
$9e1d >PC $b3 >S $db >A $7e >X $71 >Y $e0 >P $9e1d $bc >M $9e1e $ff >M $9e1f $d7 >M $9e20 $99 >M $d87d $12 >M
T{ op PC S A X Y P -> $9e20 $b3 $db $7e $12 $60 }T T{ $9e1d M $9e1e M $9e1f M $9e20 M $d87d M -> $bc $ff $d7 $99 $12 }T
$7456 >PC $a4 >S $66 >A $c8 >X $73 >Y $26 >P $2390 $c2 >M $7456 $bc >M $7457 $c8 >M $7458 $22 >M $7459 $2f >M
T{ op PC S A X Y P -> $7459 $a4 $66 $c8 $c2 $a4 }T T{ $2390 M $7456 M $7457 M $7458 M $7459 M -> $c2 $bc $c8 $22 $2f }T
$b268 >PC $08 >S $e2 >A $56 >X $3d >Y $26 >P $8f1c $21 >M $b268 $bc >M $b269 $c6 >M $b26a $8e >M $b26b $03 >M
T{ op PC S A X Y P -> $b26b $08 $e2 $56 $21 $24 }T T{ $8f1c M $b268 M $b269 M $b26a M $b26b M -> $21 $bc $c6 $8e $03 }T
$cda8 >PC $87 >S $b6 >A $3e >X $9a >Y $22 >P $1aa9 $d9 >M $cda8 $bc >M $cda9 $6b >M $cdaa $1a >M $cdab $fa >M
T{ op PC S A X Y P -> $cdab $87 $b6 $3e $d9 $a0 }T T{ $1aa9 M $cda8 M $cda9 M $cdaa M $cdab M -> $d9 $bc $6b $1a $fa }T
$fd1c >PC $32 >S $1c >A $f7 >X $a0 >Y $e4 >P $26a9 $ee >M $fd1c $bc >M $fd1d $b2 >M $fd1e $25 >M $fd1f $9a >M
T{ op PC S A X Y P -> $fd1f $32 $1c $f7 $ee $e4 }T T{ $26a9 M $fd1c M $fd1d M $fd1e M $fd1f M -> $ee $bc $b2 $25 $9a }T
$7d3c >PC $96 >S $43 >A $73 >X $c4 >Y $26 >P $726c $df >M $7d3c $bc >M $7d3d $f9 >M $7d3e $71 >M $7d3f $3b >M
T{ op PC S A X Y P -> $7d3f $96 $43 $73 $df $a4 }T T{ $726c M $7d3c M $7d3d M $7d3e M $7d3f M -> $df $bc $f9 $71 $3b }T
$ab2f >PC $c5 >S $19 >A $35 >X $0b >Y $e3 >P $9aea $a0 >M $ab2f $bc >M $ab30 $b5 >M $ab31 $9a >M $ab32 $c0 >M
T{ op PC S A X Y P -> $ab32 $c5 $19 $35 $a0 $e1 }T T{ $9aea M $ab2f M $ab30 M $ab31 M $ab32 M -> $a0 $bc $b5 $9a $c0 }T
$ee2e >PC $cb >S $c4 >A $7c >X $94 >Y $64 >P $79ac $bd >M $ee2e $bc >M $ee2f $30 >M $ee30 $79 >M $ee31 $07 >M
T{ op PC S A X Y P -> $ee31 $cb $c4 $7c $bd $e4 }T T{ $79ac M $ee2e M $ee2f M $ee30 M $ee31 M -> $bd $bc $30 $79 $07 }T
$4f77 >PC $78 >S $00 >A $33 >X $31 >Y $22 >P $4f77 $bc >M $4f78 $c5 >M $4f79 $78 >M $4f7a $3c >M $78f8 $a6 >M
T{ op PC S A X Y P -> $4f7a $78 $00 $33 $a6 $a0 }T T{ $4f77 M $4f78 M $4f79 M $4f7a M $78f8 M -> $bc $c5 $78 $3c $a6 }T
$b184 >PC $24 >S $d1 >A $7f >X $d3 >Y $26 >P $7b84 $e8 >M $b184 $bc >M $b185 $05 >M $b186 $7b >M $b187 $5a >M
T{ op PC S A X Y P -> $b187 $24 $d1 $7f $e8 $a4 }T T{ $7b84 M $b184 M $b185 M $b186 M $b187 M -> $e8 $bc $05 $7b $5a }T
$d744 >PC $1d >S $08 >A $54 >X $6f >Y $e1 >P $34c4 $6a >M $d744 $bc >M $d745 $70 >M $d746 $34 >M $d747 $e7 >M
T{ op PC S A X Y P -> $d747 $1d $08 $54 $6a $61 }T T{ $34c4 M $d744 M $d745 M $d746 M $d747 M -> $6a $bc $70 $34 $e7 }T
( bd )
$30c5 >PC $73 >S $77 >A $98 >X $28 >Y $e5 >P $30c5 $bd >M $30c6 $e9 >M $30c7 $61 >M $30c8 $ed >M $6281 $fc >M
T{ op PC S A X Y P -> $30c8 $73 $fc $98 $28 $e5 }T T{ $30c5 M $30c6 M $30c7 M $30c8 M $6281 M -> $bd $e9 $61 $ed $fc }T
$2c64 >PC $50 >S $33 >A $1e >X $b9 >Y $e6 >P $2c64 $bd >M $2c65 $84 >M $2c66 $8c >M $2c67 $fe >M $8ca2 $32 >M
T{ op PC S A X Y P -> $2c67 $50 $32 $1e $b9 $64 }T T{ $2c64 M $2c65 M $2c66 M $2c67 M $8ca2 M -> $bd $84 $8c $fe $32 }T
$b7e7 >PC $b9 >S $e6 >A $89 >X $a9 >Y $e3 >P $62b4 $56 >M $b7e7 $bd >M $b7e8 $2b >M $b7e9 $62 >M $b7ea $6d >M
T{ op PC S A X Y P -> $b7ea $b9 $56 $89 $a9 $61 }T T{ $62b4 M $b7e7 M $b7e8 M $b7e9 M $b7ea M -> $56 $bd $2b $62 $6d }T
$30ae >PC $47 >S $13 >A $a5 >X $7d >Y $26 >P $30ae $bd >M $30af $b0 >M $30b0 $ac >M $30b1 $42 >M $ad55 $4e >M
T{ op PC S A X Y P -> $30b1 $47 $4e $a5 $7d $24 }T T{ $30ae M $30af M $30b0 M $30b1 M $ad55 M -> $bd $b0 $ac $42 $4e }T
$bd18 >PC $63 >S $e9 >A $14 >X $7b >Y $e6 >P $bd18 $bd >M $bd19 $0a >M $bd1a $bf >M $bd1b $f1 >M $bf1e $0b >M
T{ op PC S A X Y P -> $bd1b $63 $0b $14 $7b $64 }T T{ $bd18 M $bd19 M $bd1a M $bd1b M $bf1e M -> $bd $0a $bf $f1 $0b }T
$9850 >PC $a8 >S $5c >A $d7 >X $33 >Y $60 >P $9850 $bd >M $9851 $77 >M $9852 $b5 >M $9853 $92 >M $b64e $15 >M
T{ op PC S A X Y P -> $9853 $a8 $15 $d7 $33 $60 }T T{ $9850 M $9851 M $9852 M $9853 M $b64e M -> $bd $77 $b5 $92 $15 }T
$b2c7 >PC $e4 >S $08 >A $3e >X $c5 >Y $20 >P $4e0d $60 >M $b2c7 $bd >M $b2c8 $cf >M $b2c9 $4d >M $b2ca $9a >M
T{ op PC S A X Y P -> $b2ca $e4 $60 $3e $c5 $20 }T T{ $4e0d M $b2c7 M $b2c8 M $b2c9 M $b2ca M -> $60 $bd $cf $4d $9a }T
$a844 >PC $f0 >S $d4 >A $5c >X $f9 >Y $e4 >P $a844 $bd >M $a845 $bd >M $a846 $ee >M $a847 $79 >M $ef19 $5f >M
T{ op PC S A X Y P -> $a847 $f0 $5f $5c $f9 $64 }T T{ $a844 M $a845 M $a846 M $a847 M $ef19 M -> $bd $bd $ee $79 $5f }T
$db2e >PC $c6 >S $06 >A $f8 >X $45 >Y $e7 >P $3a86 $c7 >M $db2e $bd >M $db2f $8e >M $db30 $39 >M $db31 $85 >M
T{ op PC S A X Y P -> $db31 $c6 $c7 $f8 $45 $e5 }T T{ $3a86 M $db2e M $db2f M $db30 M $db31 M -> $c7 $bd $8e $39 $85 }T
$a43b >PC $30 >S $d3 >A $29 >X $ba >Y $67 >P $5ed4 $fb >M $a43b $bd >M $a43c $ab >M $a43d $5e >M $a43e $14 >M
T{ op PC S A X Y P -> $a43e $30 $fb $29 $ba $e5 }T T{ $5ed4 M $a43b M $a43c M $a43d M $a43e M -> $fb $bd $ab $5e $14 }T
$345d >PC $bd >S $83 >A $9c >X $1f >Y $a5 >P $345d $bd >M $345e $52 >M $345f $bb >M $3460 $85 >M $bbee $ed >M
T{ op PC S A X Y P -> $3460 $bd $ed $9c $1f $a5 }T T{ $345d M $345e M $345f M $3460 M $bbee M -> $bd $52 $bb $85 $ed }T
$ef05 >PC $71 >S $91 >A $11 >X $6c >Y $25 >P $d0fe $af >M $ef05 $bd >M $ef06 $ed >M $ef07 $d0 >M $ef08 $ac >M
T{ op PC S A X Y P -> $ef08 $71 $af $11 $6c $a5 }T T{ $d0fe M $ef05 M $ef06 M $ef07 M $ef08 M -> $af $bd $ed $d0 $ac }T
$9211 >PC $bf >S $08 >A $78 >X $b5 >Y $a2 >P $9211 $bd >M $9212 $b5 >M $9213 $a2 >M $9214 $60 >M $a32d $de >M
T{ op PC S A X Y P -> $9214 $bf $de $78 $b5 $a0 }T T{ $9211 M $9212 M $9213 M $9214 M $a32d M -> $bd $b5 $a2 $60 $de }T
$dd21 >PC $85 >S $e2 >A $2f >X $0a >Y $26 >P $acf6 $4f >M $dd21 $bd >M $dd22 $c7 >M $dd23 $ac >M $dd24 $80 >M
T{ op PC S A X Y P -> $dd24 $85 $4f $2f $0a $24 }T T{ $acf6 M $dd21 M $dd22 M $dd23 M $dd24 M -> $4f $bd $c7 $ac $80 }T
$d75b >PC $cb >S $e2 >A $78 >X $1d >Y $62 >P $53c6 $17 >M $d75b $bd >M $d75c $4e >M $d75d $53 >M $d75e $26 >M
T{ op PC S A X Y P -> $d75e $cb $17 $78 $1d $60 }T T{ $53c6 M $d75b M $d75c M $d75d M $d75e M -> $17 $bd $4e $53 $26 }T
$42ed >PC $df >S $e4 >A $fb >X $2a >Y $e5 >P $42ed $bd >M $42ee $8b >M $42ef $58 >M $42f0 $9b >M $5986 $0c >M
T{ op PC S A X Y P -> $42f0 $df $0c $fb $2a $65 }T T{ $42ed M $42ee M $42ef M $42f0 M $5986 M -> $bd $8b $58 $9b $0c }T
( be )
$4cc2 >PC $7f >S $3a >A $6b >X $fa >Y $a3 >P $4cc2 $be >M $4cc3 $9b >M $4cc4 $bd >M $4cc5 $bc >M $be95 $0a >M
T{ op PC S A X Y P -> $4cc5 $7f $3a $0a $fa $21 }T T{ $4cc2 M $4cc3 M $4cc4 M $4cc5 M $be95 M -> $be $9b $bd $bc $0a }T
$72d5 >PC $fb >S $ed >A $b9 >X $eb >Y $a3 >P $547a $2a >M $72d5 $be >M $72d6 $8f >M $72d7 $53 >M $72d8 $de >M
T{ op PC S A X Y P -> $72d8 $fb $ed $2a $eb $21 }T T{ $547a M $72d5 M $72d6 M $72d7 M $72d8 M -> $2a $be $8f $53 $de }T
$daf6 >PC $f1 >S $52 >A $32 >X $eb >Y $66 >P $3cdf $b3 >M $daf6 $be >M $daf7 $f4 >M $daf8 $3b >M $daf9 $c8 >M
T{ op PC S A X Y P -> $daf9 $f1 $52 $b3 $eb $e4 }T T{ $3cdf M $daf6 M $daf7 M $daf8 M $daf9 M -> $b3 $be $f4 $3b $c8 }T
$c88d >PC $a3 >S $ff >A $9b >X $a7 >Y $a5 >P $c88d $be >M $c88e $cb >M $c88f $f7 >M $c890 $e3 >M $f872 $27 >M
T{ op PC S A X Y P -> $c890 $a3 $ff $27 $a7 $25 }T T{ $c88d M $c88e M $c88f M $c890 M $f872 M -> $be $cb $f7 $e3 $27 }T
$297f >PC $9f >S $88 >A $5a >X $5a >Y $63 >P $297f $be >M $2980 $7c >M $2981 $ac >M $2982 $ac >M $acd6 $93 >M
T{ op PC S A X Y P -> $2982 $9f $88 $93 $5a $e1 }T T{ $297f M $2980 M $2981 M $2982 M $acd6 M -> $be $7c $ac $ac $93 }T
$b827 >PC $42 >S $f3 >A $e6 >X $40 >Y $a1 >P $14e4 $78 >M $b827 $be >M $b828 $a4 >M $b829 $14 >M $b82a $74 >M
T{ op PC S A X Y P -> $b82a $42 $f3 $78 $40 $21 }T T{ $14e4 M $b827 M $b828 M $b829 M $b82a M -> $78 $be $a4 $14 $74 }T
$008c >PC $b5 >S $75 >A $be >X $f0 >Y $61 >P $008c $be >M $008d $ec >M $008e $31 >M $008f $4c >M $32dc $ae >M
T{ op PC S A X Y P -> $008f $b5 $75 $ae $f0 $e1 }T T{ $008c M $008d M $008e M $008f M $32dc M -> $be $ec $31 $4c $ae }T
$b339 >PC $f0 >S $71 >A $94 >X $59 >Y $60 >P $b339 $be >M $b33a $7f >M $b33b $d8 >M $b33c $37 >M $d8d8 $54 >M
T{ op PC S A X Y P -> $b33c $f0 $71 $54 $59 $60 }T T{ $b339 M $b33a M $b33b M $b33c M $d8d8 M -> $be $7f $d8 $37 $54 }T
$0290 >PC $67 >S $b5 >A $3e >X $fd >Y $63 >P $0290 $be >M $0291 $3f >M $0292 $e6 >M $0293 $b6 >M $e73c $d9 >M
T{ op PC S A X Y P -> $0293 $67 $b5 $d9 $fd $e1 }T T{ $0290 M $0291 M $0292 M $0293 M $e73c M -> $be $3f $e6 $b6 $d9 }T
$67db >PC $94 >S $ae >A $32 >X $69 >Y $a4 >P $67db $be >M $67dc $65 >M $67dd $98 >M $67de $89 >M $98ce $7e >M
T{ op PC S A X Y P -> $67de $94 $ae $7e $69 $24 }T T{ $67db M $67dc M $67dd M $67de M $98ce M -> $be $65 $98 $89 $7e }T
$5268 >PC $38 >S $c8 >A $f7 >X $83 >Y $a2 >P $5268 $be >M $5269 $e5 >M $526a $cb >M $526b $72 >M $cc68 $68 >M
T{ op PC S A X Y P -> $526b $38 $c8 $68 $83 $20 }T T{ $5268 M $5269 M $526a M $526b M $cc68 M -> $be $e5 $cb $72 $68 }T
$e4d9 >PC $0f >S $d9 >A $83 >X $54 >Y $26 >P $8260 $b1 >M $e4d9 $be >M $e4da $0c >M $e4db $82 >M $e4dc $71 >M
T{ op PC S A X Y P -> $e4dc $0f $d9 $b1 $54 $a4 }T T{ $8260 M $e4d9 M $e4da M $e4db M $e4dc M -> $b1 $be $0c $82 $71 }T
$cc6d >PC $91 >S $04 >A $e3 >X $8c >Y $65 >P $536e $00 >M $cc6d $be >M $cc6e $e2 >M $cc6f $52 >M $cc70 $dd >M
T{ op PC S A X Y P -> $cc70 $91 $04 $00 $8c $67 }T T{ $536e M $cc6d M $cc6e M $cc6f M $cc70 M -> $00 $be $e2 $52 $dd }T
$f644 >PC $b9 >S $ef >A $d9 >X $09 >Y $60 >P $bc11 $35 >M $f644 $be >M $f645 $08 >M $f646 $bc >M $f647 $85 >M
T{ op PC S A X Y P -> $f647 $b9 $ef $35 $09 $60 }T T{ $bc11 M $f644 M $f645 M $f646 M $f647 M -> $35 $be $08 $bc $85 }T
$2746 >PC $2c >S $90 >A $02 >X $56 >Y $65 >P $2746 $be >M $2747 $6a >M $2748 $da >M $2749 $0e >M $dac0 $07 >M
T{ op PC S A X Y P -> $2749 $2c $90 $07 $56 $65 }T T{ $2746 M $2747 M $2748 M $2749 M $dac0 M -> $be $6a $da $0e $07 }T
$d3d7 >PC $da >S $1a >A $b8 >X $72 >Y $67 >P $5b8c $63 >M $d3d7 $be >M $d3d8 $1a >M $d3d9 $5b >M $d3da $c5 >M
T{ op PC S A X Y P -> $d3da $da $1a $63 $72 $65 }T T{ $5b8c M $d3d7 M $d3d8 M $d3d9 M $d3da M -> $63 $be $1a $5b $c5 }T
( bf )
$a178 >PC $d5 >S $79 >A $d6 >X $c1 >Y $a5 >P $00bd $ab >M $a172 $d1 >M $a178 $bf >M $a179 $bd >M $a17a $f7 >M
T{ op PC S A X Y P -> $a172 $d5 $79 $d6 $c1 $a5 }T T{ $00bd M $a172 M $a178 M $a179 M $a17a M -> $ab $d1 $bf $bd $f7 }T
$35fc >PC $ce >S $fc >A $62 >X $e2 >Y $a2 >P $00cd $36 >M $3512 $1f >M $35fc $bf >M $35fd $cd >M $35fe $13 >M $35ff $2e >M
T{ op PC S A X Y P -> $35ff $ce $fc $62 $e2 $a2 }T T{ $00cd M $3512 M $35fc M $35fd M $35fe M $35ff M -> $36 $1f $bf $cd $13 $2e }T
$a24c >PC $a6 >S $17 >A $45 >X $ae >Y $e4 >P $00dc $b8 >M $a20b $88 >M $a24c $bf >M $a24d $dc >M $a24e $bc >M
T{ op PC S A X Y P -> $a20b $a6 $17 $45 $ae $e4 }T T{ $00dc M $a20b M $a24c M $a24d M $a24e M -> $b8 $88 $bf $dc $bc }T
$12e4 >PC $03 >S $fe >A $86 >X $4c >Y $67 >P $0003 $9f >M $126e $e8 >M $12e4 $bf >M $12e5 $03 >M $12e6 $87 >M
T{ op PC S A X Y P -> $126e $03 $fe $86 $4c $67 }T T{ $0003 M $126e M $12e4 M $12e5 M $12e6 M -> $9f $e8 $bf $03 $87 }T
$009b >PC $3f >S $a3 >A $13 >X $9e >Y $a1 >P $001e $5a >M $009b $bf >M $009c $1e >M $009d $61 >M $00ff $73 >M
T{ op PC S A X Y P -> $00ff $3f $a3 $13 $9e $a1 }T T{ $001e M $009b M $009c M $009d M $00ff M -> $5a $bf $1e $61 $73 }T
$302c >PC $c7 >S $45 >A $d7 >X $68 >Y $64 >P $00f3 $43 >M $302c $bf >M $302d $f3 >M $302e $a8 >M $302f $70 >M $30d7 $6e >M
T{ op PC S A X Y P -> $302f $c7 $45 $d7 $68 $64 }T T{ $00f3 M $302c M $302d M $302e M $302f M $30d7 M -> $43 $bf $f3 $a8 $70 $6e }T
$114a >PC $d0 >S $9a >A $98 >X $eb >Y $a6 >P $0022 $49 >M $114a $bf >M $114b $22 >M $114c $74 >M $11c1 $39 >M
T{ op PC S A X Y P -> $11c1 $d0 $9a $98 $eb $a6 }T T{ $0022 M $114a M $114b M $114c M $11c1 M -> $49 $bf $22 $74 $39 }T
$e402 >PC $11 >S $fb >A $70 >X $23 >Y $a6 >P $0083 $cf >M $e402 $bf >M $e403 $83 >M $e404 $2d >M $e432 $bb >M
T{ op PC S A X Y P -> $e432 $11 $fb $70 $23 $a6 }T T{ $0083 M $e402 M $e403 M $e404 M $e432 M -> $cf $bf $83 $2d $bb }T
$0fc6 >PC $da >S $97 >A $2d >X $81 >Y $a4 >P $0082 $37 >M $0f72 $af >M $0fc6 $bf >M $0fc7 $82 >M $0fc8 $a9 >M $0fc9 $9b >M
T{ op PC S A X Y P -> $0fc9 $da $97 $2d $81 $a4 }T T{ $0082 M $0f72 M $0fc6 M $0fc7 M $0fc8 M $0fc9 M -> $37 $af $bf $82 $a9 $9b }T
$c361 >PC $67 >S $09 >A $51 >X $87 >Y $21 >P $0025 $29 >M $c361 $bf >M $c362 $25 >M $c363 $76 >M $c3da $d8 >M
T{ op PC S A X Y P -> $c3da $67 $09 $51 $87 $21 }T T{ $0025 M $c361 M $c362 M $c363 M $c3da M -> $29 $bf $25 $76 $d8 }T
$2487 >PC $40 >S $80 >A $76 >X $08 >Y $e3 >P $0009 $ae >M $247f $bb >M $2487 $bf >M $2488 $09 >M $2489 $f5 >M
T{ op PC S A X Y P -> $247f $40 $80 $76 $08 $e3 }T T{ $0009 M $247f M $2487 M $2488 M $2489 M -> $ae $bb $bf $09 $f5 }T
$1fa1 >PC $34 >S $aa >A $74 >X $1d >Y $e0 >P $0016 $1a >M $1fa1 $bf >M $1fa2 $16 >M $1fa3 $40 >M $1fe4 $d6 >M
T{ op PC S A X Y P -> $1fe4 $34 $aa $74 $1d $e0 }T T{ $0016 M $1fa1 M $1fa2 M $1fa3 M $1fe4 M -> $1a $bf $16 $40 $d6 }T
$2365 >PC $39 >S $5b >A $ff >X $f0 >Y $a1 >P $0096 $4b >M $230e $54 >M $2365 $bf >M $2366 $96 >M $2367 $a6 >M
T{ op PC S A X Y P -> $230e $39 $5b $ff $f0 $a1 }T T{ $0096 M $230e M $2365 M $2366 M $2367 M -> $4b $54 $bf $96 $a6 }T
$1f29 >PC $03 >S $b6 >A $cc >X $ba >Y $62 >P $0082 $7d >M $1f29 $bf >M $1f2a $82 >M $1f2b $63 >M $1f8f $5a >M
T{ op PC S A X Y P -> $1f8f $03 $b6 $cc $ba $62 }T T{ $0082 M $1f29 M $1f2a M $1f2b M $1f8f M -> $7d $bf $82 $63 $5a }T
$8e6b >PC $bd >S $66 >A $cc >X $89 >Y $25 >P $0038 $62 >M $8e6b $bf >M $8e6c $38 >M $8e6d $2f >M $8e6e $e1 >M $8e9d $8a >M
T{ op PC S A X Y P -> $8e6e $bd $66 $cc $89 $25 }T T{ $0038 M $8e6b M $8e6c M $8e6d M $8e6e M $8e9d M -> $62 $bf $38 $2f $e1 $8a }T
$29a0 >PC $23 >S $ce >A $07 >X $c4 >Y $26 >P $0000 $53 >M $29a0 $bf >M $29a1 $00 >M $29a2 $2a >M $29a3 $c0 >M $29cd $cb >M
T{ op PC S A X Y P -> $29a3 $23 $ce $07 $c4 $26 }T T{ $0000 M $29a0 M $29a1 M $29a2 M $29a3 M $29cd M -> $53 $bf $00 $2a $c0 $cb }T
( c0 )
$f5c0 >PC $6c >S $f6 >A $f5 >X $56 >Y $a2 >P $f5c0 $c0 >M $f5c1 $0c >M $f5c2 $40 >M
T{ op PC S A X Y P -> $f5c2 $6c $f6 $f5 $56 $21 }T T{ $f5c0 M $f5c1 M $f5c2 M -> $c0 $0c $40 }T
$91cf >PC $3c >S $06 >A $0b >X $47 >Y $a6 >P $91cf $c0 >M $91d0 $09 >M $91d1 $15 >M
T{ op PC S A X Y P -> $91d1 $3c $06 $0b $47 $25 }T T{ $91cf M $91d0 M $91d1 M -> $c0 $09 $15 }T
$8d55 >PC $c2 >S $b1 >A $2b >X $37 >Y $22 >P $8d55 $c0 >M $8d56 $7b >M $8d57 $ed >M
T{ op PC S A X Y P -> $8d57 $c2 $b1 $2b $37 $a0 }T T{ $8d55 M $8d56 M $8d57 M -> $c0 $7b $ed }T
$6c2e >PC $6e >S $7e >A $b3 >X $f0 >Y $27 >P $6c2e $c0 >M $6c2f $54 >M $6c30 $f5 >M
T{ op PC S A X Y P -> $6c30 $6e $7e $b3 $f0 $a5 }T T{ $6c2e M $6c2f M $6c30 M -> $c0 $54 $f5 }T
$6929 >PC $6c >S $a7 >A $f1 >X $c0 >Y $a5 >P $6929 $c0 >M $692a $b3 >M $692b $7d >M
T{ op PC S A X Y P -> $692b $6c $a7 $f1 $c0 $25 }T T{ $6929 M $692a M $692b M -> $c0 $b3 $7d }T
$d01d >PC $49 >S $c7 >A $95 >X $0e >Y $a7 >P $d01d $c0 >M $d01e $3c >M $d01f $07 >M
T{ op PC S A X Y P -> $d01f $49 $c7 $95 $0e $a4 }T T{ $d01d M $d01e M $d01f M -> $c0 $3c $07 }T
$1a3d >PC $04 >S $1b >A $f2 >X $87 >Y $e2 >P $1a3d $c0 >M $1a3e $46 >M $1a3f $93 >M
T{ op PC S A X Y P -> $1a3f $04 $1b $f2 $87 $61 }T T{ $1a3d M $1a3e M $1a3f M -> $c0 $46 $93 }T
$31c6 >PC $a5 >S $d9 >A $59 >X $2e >Y $66 >P $31c6 $c0 >M $31c7 $cf >M $31c8 $af >M
T{ op PC S A X Y P -> $31c8 $a5 $d9 $59 $2e $64 }T T{ $31c6 M $31c7 M $31c8 M -> $c0 $cf $af }T
$24f9 >PC $ae >S $04 >A $26 >X $7f >Y $e3 >P $24f9 $c0 >M $24fa $d4 >M $24fb $76 >M
T{ op PC S A X Y P -> $24fb $ae $04 $26 $7f $e0 }T T{ $24f9 M $24fa M $24fb M -> $c0 $d4 $76 }T
$9395 >PC $57 >S $cc >A $c1 >X $15 >Y $26 >P $9395 $c0 >M $9396 $40 >M $9397 $d4 >M
T{ op PC S A X Y P -> $9397 $57 $cc $c1 $15 $a4 }T T{ $9395 M $9396 M $9397 M -> $c0 $40 $d4 }T
$0c58 >PC $fb >S $4a >A $49 >X $82 >Y $64 >P $0c58 $c0 >M $0c59 $a0 >M $0c5a $e7 >M
T{ op PC S A X Y P -> $0c5a $fb $4a $49 $82 $e4 }T T{ $0c58 M $0c59 M $0c5a M -> $c0 $a0 $e7 }T
$d348 >PC $c0 >S $80 >A $ec >X $c0 >Y $a5 >P $d348 $c0 >M $d349 $66 >M $d34a $42 >M
T{ op PC S A X Y P -> $d34a $c0 $80 $ec $c0 $25 }T T{ $d348 M $d349 M $d34a M -> $c0 $66 $42 }T
$b9f3 >PC $13 >S $a9 >A $cd >X $df >Y $e1 >P $b9f3 $c0 >M $b9f4 $df >M $b9f5 $8b >M
T{ op PC S A X Y P -> $b9f5 $13 $a9 $cd $df $63 }T T{ $b9f3 M $b9f4 M $b9f5 M -> $c0 $df $8b }T
$121f >PC $1d >S $4c >A $03 >X $ee >Y $e2 >P $121f $c0 >M $1220 $83 >M $1221 $25 >M
T{ op PC S A X Y P -> $1221 $1d $4c $03 $ee $61 }T T{ $121f M $1220 M $1221 M -> $c0 $83 $25 }T
$6a4d >PC $e0 >S $1b >A $fb >X $0d >Y $21 >P $6a4d $c0 >M $6a4e $c1 >M $6a4f $5a >M
T{ op PC S A X Y P -> $6a4f $e0 $1b $fb $0d $20 }T T{ $6a4d M $6a4e M $6a4f M -> $c0 $c1 $5a }T
$6e02 >PC $6d >S $a6 >A $df >X $f3 >Y $a5 >P $6e02 $c0 >M $6e03 $a3 >M $6e04 $ff >M
T{ op PC S A X Y P -> $6e04 $6d $a6 $df $f3 $25 }T T{ $6e02 M $6e03 M $6e04 M -> $c0 $a3 $ff }T
( c1 )
$2912 >PC $96 >S $4a >A $d7 >X $c3 >Y $20 >P $0013 $a1 >M $00ea $9a >M $00eb $50 >M $2912 $c1 >M $2913 $13 >M $2914 $69 >M $509a $19 >M
T{ op PC S A X Y P -> $2914 $96 $4a $d7 $c3 $21 }T T{ $0013 M $00ea M $00eb M $2912 M $2913 M $2914 M $509a M -> $a1 $9a $50 $c1 $13 $69 $19 }T
$b777 >PC $2b >S $85 >A $7f >X $ca >Y $e7 >P $0066 $95 >M $0067 $7f >M $00e7 $a2 >M $7f95 $f0 >M $b777 $c1 >M $b778 $e7 >M $b779 $8d >M
T{ op PC S A X Y P -> $b779 $2b $85 $7f $ca $e4 }T T{ $0066 M $0067 M $00e7 M $7f95 M $b777 M $b778 M $b779 M -> $95 $7f $a2 $f0 $c1 $e7 $8d }T
$f55b >PC $49 >S $39 >A $9f >X $df >Y $a2 >P $003c $8f >M $00db $cf >M $00dc $a2 >M $a2cf $b3 >M $f55b $c1 >M $f55c $3c >M $f55d $c5 >M
T{ op PC S A X Y P -> $f55d $49 $39 $9f $df $a0 }T T{ $003c M $00db M $00dc M $a2cf M $f55b M $f55c M $f55d M -> $8f $cf $a2 $b3 $c1 $3c $c5 }T
$f2c3 >PC $c2 >S $2d >A $18 >X $c6 >Y $a7 >P $00e6 $95 >M $00fe $aa >M $00ff $57 >M $57aa $e6 >M $f2c3 $c1 >M $f2c4 $e6 >M $f2c5 $1a >M
T{ op PC S A X Y P -> $f2c5 $c2 $2d $18 $c6 $24 }T T{ $00e6 M $00fe M $00ff M $57aa M $f2c3 M $f2c4 M $f2c5 M -> $95 $aa $57 $e6 $c1 $e6 $1a }T
$8c16 >PC $86 >S $f7 >A $e9 >X $e2 >Y $23 >P $0093 $93 >M $0094 $ae >M $00aa $8f >M $8c16 $c1 >M $8c17 $aa >M $8c18 $4c >M $ae93 $98 >M
T{ op PC S A X Y P -> $8c18 $86 $f7 $e9 $e2 $21 }T T{ $0093 M $0094 M $00aa M $8c16 M $8c17 M $8c18 M $ae93 M -> $93 $ae $8f $c1 $aa $4c $98 }T
$ead9 >PC $24 >S $07 >A $8f >X $41 >Y $26 >P $0079 $8f >M $007a $74 >M $00ea $1f >M $748f $ac >M $ead9 $c1 >M $eada $ea >M $eadb $20 >M
T{ op PC S A X Y P -> $eadb $24 $07 $8f $41 $24 }T T{ $0079 M $007a M $00ea M $748f M $ead9 M $eada M $eadb M -> $8f $74 $1f $ac $c1 $ea $20 }T
$d396 >PC $66 >S $98 >A $ca >X $99 >Y $61 >P $004a $99 >M $004b $a8 >M $0080 $d5 >M $a899 $9a >M $d396 $c1 >M $d397 $80 >M $d398 $ee >M
T{ op PC S A X Y P -> $d398 $66 $98 $ca $99 $e0 }T T{ $004a M $004b M $0080 M $a899 M $d396 M $d397 M $d398 M -> $99 $a8 $d5 $9a $c1 $80 $ee }T
$b186 >PC $f9 >S $fd >A $b1 >X $f7 >Y $22 >P $0048 $cc >M $0049 $84 >M $0097 $a0 >M $84cc $c0 >M $b186 $c1 >M $b187 $97 >M $b188 $8d >M
T{ op PC S A X Y P -> $b188 $f9 $fd $b1 $f7 $21 }T T{ $0048 M $0049 M $0097 M $84cc M $b186 M $b187 M $b188 M -> $cc $84 $a0 $c0 $c1 $97 $8d }T
$9809 >PC $17 >S $75 >A $9c >X $1c >Y $e4 >P $0088 $e7 >M $0089 $4e >M $00ec $cb >M $4ee7 $ec >M $9809 $c1 >M $980a $ec >M $980b $c1 >M
T{ op PC S A X Y P -> $980b $17 $75 $9c $1c $e4 }T T{ $0088 M $0089 M $00ec M $4ee7 M $9809 M $980a M $980b M -> $e7 $4e $cb $ec $c1 $ec $c1 }T
$a6ed >PC $41 >S $a4 >A $5a >X $ec >Y $e7 >P $0028 $70 >M $0082 $88 >M $0083 $7a >M $7a88 $a3 >M $a6ed $c1 >M $a6ee $28 >M $a6ef $81 >M
T{ op PC S A X Y P -> $a6ef $41 $a4 $5a $ec $65 }T T{ $0028 M $0082 M $0083 M $7a88 M $a6ed M $a6ee M $a6ef M -> $70 $88 $7a $a3 $c1 $28 $81 }T
$ce80 >PC $00 >S $a5 >A $02 >X $02 >Y $e7 >P $00f3 $cb >M $00f5 $53 >M $00f6 $32 >M $3253 $73 >M $ce80 $c1 >M $ce81 $f3 >M $ce82 $4c >M
T{ op PC S A X Y P -> $ce82 $00 $a5 $02 $02 $65 }T T{ $00f3 M $00f5 M $00f6 M $3253 M $ce80 M $ce81 M $ce82 M -> $cb $53 $32 $73 $c1 $f3 $4c }T
$fd1a >PC $33 >S $0c >A $d0 >X $7d >Y $60 >P $006b $db >M $006c $bc >M $009b $e5 >M $bcdb $96 >M $fd1a $c1 >M $fd1b $9b >M $fd1c $c7 >M
T{ op PC S A X Y P -> $fd1c $33 $0c $d0 $7d $60 }T T{ $006b M $006c M $009b M $bcdb M $fd1a M $fd1b M $fd1c M -> $db $bc $e5 $96 $c1 $9b $c7 }T
$7971 >PC $27 >S $5a >A $09 >X $4b >Y $e2 >P $006a $f8 >M $0073 $e9 >M $0074 $de >M $7971 $c1 >M $7972 $6a >M $7973 $30 >M $dee9 $d5 >M
T{ op PC S A X Y P -> $7973 $27 $5a $09 $4b $e0 }T T{ $006a M $0073 M $0074 M $7971 M $7972 M $7973 M $dee9 M -> $f8 $e9 $de $c1 $6a $30 $d5 }T
$12ef >PC $ac >S $71 >A $5a >X $d4 >Y $e6 >P $0001 $d1 >M $005b $92 >M $005c $82 >M $12ef $c1 >M $12f0 $01 >M $12f1 $a0 >M $8292 $08 >M
T{ op PC S A X Y P -> $12f1 $ac $71 $5a $d4 $65 }T T{ $0001 M $005b M $005c M $12ef M $12f0 M $12f1 M $8292 M -> $d1 $92 $82 $c1 $01 $a0 $08 }T
$9e93 >PC $3b >S $9b >A $55 >X $bd >Y $66 >P $000d $e4 >M $000e $ad >M $00b8 $9d >M $9e93 $c1 >M $9e94 $b8 >M $9e95 $4d >M $ade4 $84 >M
T{ op PC S A X Y P -> $9e95 $3b $9b $55 $bd $65 }T T{ $000d M $000e M $00b8 M $9e93 M $9e94 M $9e95 M $ade4 M -> $e4 $ad $9d $c1 $b8 $4d $84 }T
$7409 >PC $52 >S $ac >A $45 >X $5c >Y $a2 >P $001d $ff >M $0062 $5f >M $0063 $59 >M $595f $44 >M $7409 $c1 >M $740a $1d >M $740b $a3 >M
T{ op PC S A X Y P -> $740b $52 $ac $45 $5c $21 }T T{ $001d M $0062 M $0063 M $595f M $7409 M $740a M $740b M -> $ff $5f $59 $44 $c1 $1d $a3 }T
( c2 )
$66fd >PC $65 >S $5f >A $7e >X $52 >Y $e7 >P $66fd $c2 >M $66fe $d2 >M $66ff $4f >M
T{ op PC S A X Y P -> $66ff $65 $5f $7e $52 $e7 }T T{ $66fd M $66fe M $66ff M -> $c2 $d2 $4f }T
$f533 >PC $c4 >S $93 >A $3d >X $65 >Y $66 >P $f533 $c2 >M $f534 $78 >M $f535 $09 >M
T{ op PC S A X Y P -> $f535 $c4 $93 $3d $65 $66 }T T{ $f533 M $f534 M $f535 M -> $c2 $78 $09 }T
$f6be >PC $d4 >S $7e >A $26 >X $0e >Y $64 >P $f6be $c2 >M $f6bf $ae >M $f6c0 $5d >M
T{ op PC S A X Y P -> $f6c0 $d4 $7e $26 $0e $64 }T T{ $f6be M $f6bf M $f6c0 M -> $c2 $ae $5d }T
$a234 >PC $c5 >S $33 >A $4d >X $c2 >Y $27 >P $a234 $c2 >M $a235 $50 >M $a236 $91 >M
T{ op PC S A X Y P -> $a236 $c5 $33 $4d $c2 $27 }T T{ $a234 M $a235 M $a236 M -> $c2 $50 $91 }T
$dedf >PC $d7 >S $f2 >A $13 >X $ff >Y $e7 >P $dedf $c2 >M $dee0 $5f >M $dee1 $3c >M
T{ op PC S A X Y P -> $dee1 $d7 $f2 $13 $ff $e7 }T T{ $dedf M $dee0 M $dee1 M -> $c2 $5f $3c }T
$b322 >PC $f2 >S $b7 >A $e0 >X $b4 >Y $a4 >P $b322 $c2 >M $b323 $81 >M $b324 $da >M
T{ op PC S A X Y P -> $b324 $f2 $b7 $e0 $b4 $a4 }T T{ $b322 M $b323 M $b324 M -> $c2 $81 $da }T
$10e1 >PC $e1 >S $3a >A $aa >X $55 >Y $e6 >P $10e1 $c2 >M $10e2 $4b >M $10e3 $47 >M
T{ op PC S A X Y P -> $10e3 $e1 $3a $aa $55 $e6 }T T{ $10e1 M $10e2 M $10e3 M -> $c2 $4b $47 }T
$d254 >PC $0a >S $02 >A $f6 >X $2b >Y $22 >P $d254 $c2 >M $d255 $2b >M $d256 $e0 >M
T{ op PC S A X Y P -> $d256 $0a $02 $f6 $2b $22 }T T{ $d254 M $d255 M $d256 M -> $c2 $2b $e0 }T
$fdf9 >PC $f9 >S $25 >A $81 >X $2a >Y $a6 >P $fdf9 $c2 >M $fdfa $0b >M $fdfb $9a >M
T{ op PC S A X Y P -> $fdfb $f9 $25 $81 $2a $a6 }T T{ $fdf9 M $fdfa M $fdfb M -> $c2 $0b $9a }T
$e4b1 >PC $1c >S $7d >A $22 >X $b8 >Y $67 >P $e4b1 $c2 >M $e4b2 $f8 >M $e4b3 $f5 >M
T{ op PC S A X Y P -> $e4b3 $1c $7d $22 $b8 $67 }T T{ $e4b1 M $e4b2 M $e4b3 M -> $c2 $f8 $f5 }T
$ac63 >PC $67 >S $cc >A $e9 >X $40 >Y $e3 >P $ac63 $c2 >M $ac64 $e9 >M $ac65 $77 >M
T{ op PC S A X Y P -> $ac65 $67 $cc $e9 $40 $e3 }T T{ $ac63 M $ac64 M $ac65 M -> $c2 $e9 $77 }T
$68d5 >PC $0b >S $15 >A $cc >X $75 >Y $22 >P $68d5 $c2 >M $68d6 $d0 >M $68d7 $e6 >M
T{ op PC S A X Y P -> $68d7 $0b $15 $cc $75 $22 }T T{ $68d5 M $68d6 M $68d7 M -> $c2 $d0 $e6 }T
$cc2a >PC $b7 >S $33 >A $f3 >X $88 >Y $26 >P $cc2a $c2 >M $cc2b $e2 >M $cc2c $68 >M
T{ op PC S A X Y P -> $cc2c $b7 $33 $f3 $88 $26 }T T{ $cc2a M $cc2b M $cc2c M -> $c2 $e2 $68 }T
$0fdb >PC $10 >S $19 >A $42 >X $79 >Y $23 >P $0fdb $c2 >M $0fdc $0b >M $0fdd $ff >M
T{ op PC S A X Y P -> $0fdd $10 $19 $42 $79 $23 }T T{ $0fdb M $0fdc M $0fdd M -> $c2 $0b $ff }T
$730b >PC $a4 >S $70 >A $3d >X $c6 >Y $a0 >P $730b $c2 >M $730c $42 >M $730d $22 >M
T{ op PC S A X Y P -> $730d $a4 $70 $3d $c6 $a0 }T T{ $730b M $730c M $730d M -> $c2 $42 $22 }T
$9d49 >PC $9c >S $75 >A $50 >X $44 >Y $a7 >P $9d49 $c2 >M $9d4a $ac >M $9d4b $d6 >M
T{ op PC S A X Y P -> $9d4b $9c $75 $50 $44 $a7 }T T{ $9d49 M $9d4a M $9d4b M -> $c2 $ac $d6 }T
( c3 )
$6006 >PC $65 >S $12 >A $41 >X $8e >Y $64 >P $6006 $c3 >M $6007 $2c >M $6008 $cc >M
T{ op PC S A X Y P -> $6007 $65 $12 $41 $8e $64 }T T{ $6006 M $6007 M $6008 M -> $c3 $2c $cc }T
$f796 >PC $c0 >S $69 >A $ba >X $8c >Y $26 >P $f796 $c3 >M $f797 $c6 >M $f798 $7e >M
T{ op PC S A X Y P -> $f797 $c0 $69 $ba $8c $26 }T T{ $f796 M $f797 M $f798 M -> $c3 $c6 $7e }T
$403c >PC $09 >S $8e >A $ef >X $25 >Y $a3 >P $403c $c3 >M $403d $27 >M $403e $c3 >M
T{ op PC S A X Y P -> $403d $09 $8e $ef $25 $a3 }T T{ $403c M $403d M $403e M -> $c3 $27 $c3 }T
$e36c >PC $6b >S $40 >A $1c >X $08 >Y $23 >P $e36c $c3 >M $e36d $4f >M $e36e $9a >M
T{ op PC S A X Y P -> $e36d $6b $40 $1c $08 $23 }T T{ $e36c M $e36d M $e36e M -> $c3 $4f $9a }T
$1fa2 >PC $62 >S $b2 >A $fe >X $b3 >Y $67 >P $1fa2 $c3 >M $1fa3 $87 >M $1fa4 $42 >M
T{ op PC S A X Y P -> $1fa3 $62 $b2 $fe $b3 $67 }T T{ $1fa2 M $1fa3 M $1fa4 M -> $c3 $87 $42 }T
$f144 >PC $bf >S $e5 >A $01 >X $88 >Y $61 >P $f144 $c3 >M $f145 $48 >M $f146 $47 >M
T{ op PC S A X Y P -> $f145 $bf $e5 $01 $88 $61 }T T{ $f144 M $f145 M $f146 M -> $c3 $48 $47 }T
$753d >PC $4e >S $e9 >A $41 >X $c8 >Y $e3 >P $753d $c3 >M $753e $8d >M $753f $de >M
T{ op PC S A X Y P -> $753e $4e $e9 $41 $c8 $e3 }T T{ $753d M $753e M $753f M -> $c3 $8d $de }T
$6c7f >PC $8f >S $bc >A $25 >X $cd >Y $24 >P $6c7f $c3 >M $6c80 $bb >M $6c81 $f7 >M
T{ op PC S A X Y P -> $6c80 $8f $bc $25 $cd $24 }T T{ $6c7f M $6c80 M $6c81 M -> $c3 $bb $f7 }T
$c410 >PC $54 >S $b2 >A $13 >X $a8 >Y $e5 >P $c410 $c3 >M $c411 $ce >M $c412 $33 >M
T{ op PC S A X Y P -> $c411 $54 $b2 $13 $a8 $e5 }T T{ $c410 M $c411 M $c412 M -> $c3 $ce $33 }T
$d64b >PC $52 >S $96 >A $1c >X $d3 >Y $a5 >P $d64b $c3 >M $d64c $b8 >M $d64d $44 >M
T{ op PC S A X Y P -> $d64c $52 $96 $1c $d3 $a5 }T T{ $d64b M $d64c M $d64d M -> $c3 $b8 $44 }T
$bc1b >PC $6e >S $5e >A $74 >X $4d >Y $60 >P $bc1b $c3 >M $bc1c $9d >M $bc1d $41 >M
T{ op PC S A X Y P -> $bc1c $6e $5e $74 $4d $60 }T T{ $bc1b M $bc1c M $bc1d M -> $c3 $9d $41 }T
$4238 >PC $aa >S $8f >A $3b >X $e1 >Y $e6 >P $4238 $c3 >M $4239 $46 >M $423a $3e >M
T{ op PC S A X Y P -> $4239 $aa $8f $3b $e1 $e6 }T T{ $4238 M $4239 M $423a M -> $c3 $46 $3e }T
$f92c >PC $e7 >S $98 >A $8e >X $fd >Y $60 >P $f92c $c3 >M $f92d $43 >M $f92e $b9 >M
T{ op PC S A X Y P -> $f92d $e7 $98 $8e $fd $60 }T T{ $f92c M $f92d M $f92e M -> $c3 $43 $b9 }T
$a593 >PC $62 >S $80 >A $be >X $d2 >Y $27 >P $a593 $c3 >M $a594 $eb >M $a595 $9b >M
T{ op PC S A X Y P -> $a594 $62 $80 $be $d2 $27 }T T{ $a593 M $a594 M $a595 M -> $c3 $eb $9b }T
$8c03 >PC $cf >S $60 >A $b4 >X $ce >Y $21 >P $8c03 $c3 >M $8c04 $4e >M $8c05 $6a >M
T{ op PC S A X Y P -> $8c04 $cf $60 $b4 $ce $21 }T T{ $8c03 M $8c04 M $8c05 M -> $c3 $4e $6a }T
$edfc >PC $a0 >S $ae >A $a8 >X $b5 >Y $21 >P $edfc $c3 >M $edfd $39 >M $edfe $dc >M
T{ op PC S A X Y P -> $edfd $a0 $ae $a8 $b5 $21 }T T{ $edfc M $edfd M $edfe M -> $c3 $39 $dc }T
( c4 )
$29e5 >PC $a5 >S $4c >A $48 >X $e6 >Y $a3 >P $00ba $bb >M $29e5 $c4 >M $29e6 $ba >M $29e7 $19 >M
T{ op PC S A X Y P -> $29e7 $a5 $4c $48 $e6 $21 }T T{ $00ba M $29e5 M $29e6 M $29e7 M -> $bb $c4 $ba $19 }T
$d8c4 >PC $fa >S $36 >A $1c >X $b3 >Y $62 >P $005d $93 >M $d8c4 $c4 >M $d8c5 $5d >M $d8c6 $bf >M
T{ op PC S A X Y P -> $d8c6 $fa $36 $1c $b3 $61 }T T{ $005d M $d8c4 M $d8c5 M $d8c6 M -> $93 $c4 $5d $bf }T
$b288 >PC $dc >S $f3 >A $f3 >X $af >Y $e1 >P $00d7 $7e >M $b288 $c4 >M $b289 $d7 >M $b28a $eb >M
T{ op PC S A X Y P -> $b28a $dc $f3 $f3 $af $61 }T T{ $00d7 M $b288 M $b289 M $b28a M -> $7e $c4 $d7 $eb }T
$9711 >PC $eb >S $2a >A $7e >X $01 >Y $a3 >P $00f9 $64 >M $9711 $c4 >M $9712 $f9 >M $9713 $17 >M
T{ op PC S A X Y P -> $9713 $eb $2a $7e $01 $a0 }T T{ $00f9 M $9711 M $9712 M $9713 M -> $64 $c4 $f9 $17 }T
$47f0 >PC $a5 >S $e8 >A $6d >X $dd >Y $23 >P $006e $72 >M $47f0 $c4 >M $47f1 $6e >M $47f2 $00 >M
T{ op PC S A X Y P -> $47f2 $a5 $e8 $6d $dd $21 }T T{ $006e M $47f0 M $47f1 M $47f2 M -> $72 $c4 $6e $00 }T
$adff >PC $b8 >S $43 >A $78 >X $42 >Y $e2 >P $0074 $43 >M $adff $c4 >M $ae00 $74 >M $ae01 $1a >M
T{ op PC S A X Y P -> $ae01 $b8 $43 $78 $42 $e0 }T T{ $0074 M $adff M $ae00 M $ae01 M -> $43 $c4 $74 $1a }T
$1925 >PC $43 >S $b1 >A $e6 >X $cd >Y $e3 >P $00fe $58 >M $1925 $c4 >M $1926 $fe >M $1927 $da >M
T{ op PC S A X Y P -> $1927 $43 $b1 $e6 $cd $61 }T T{ $00fe M $1925 M $1926 M $1927 M -> $58 $c4 $fe $da }T
$3152 >PC $6d >S $75 >A $12 >X $fa >Y $27 >P $000e $4c >M $3152 $c4 >M $3153 $0e >M $3154 $e4 >M
T{ op PC S A X Y P -> $3154 $6d $75 $12 $fa $a5 }T T{ $000e M $3152 M $3153 M $3154 M -> $4c $c4 $0e $e4 }T
$5710 >PC $54 >S $12 >A $c7 >X $91 >Y $a1 >P $0087 $f7 >M $5710 $c4 >M $5711 $87 >M $5712 $43 >M
T{ op PC S A X Y P -> $5712 $54 $12 $c7 $91 $a0 }T T{ $0087 M $5710 M $5711 M $5712 M -> $f7 $c4 $87 $43 }T
$aa94 >PC $d8 >S $51 >A $08 >X $b2 >Y $e3 >P $005b $10 >M $aa94 $c4 >M $aa95 $5b >M $aa96 $82 >M
T{ op PC S A X Y P -> $aa96 $d8 $51 $08 $b2 $e1 }T T{ $005b M $aa94 M $aa95 M $aa96 M -> $10 $c4 $5b $82 }T
$e2ca >PC $5e >S $6b >A $0d >X $65 >Y $a7 >P $00ce $87 >M $e2ca $c4 >M $e2cb $ce >M $e2cc $61 >M
T{ op PC S A X Y P -> $e2cc $5e $6b $0d $65 $a4 }T T{ $00ce M $e2ca M $e2cb M $e2cc M -> $87 $c4 $ce $61 }T
$b8d8 >PC $3b >S $69 >A $c9 >X $ad >Y $a6 >P $00d7 $27 >M $b8d8 $c4 >M $b8d9 $d7 >M $b8da $59 >M
T{ op PC S A X Y P -> $b8da $3b $69 $c9 $ad $a5 }T T{ $00d7 M $b8d8 M $b8d9 M $b8da M -> $27 $c4 $d7 $59 }T
$78f1 >PC $0c >S $32 >A $68 >X $84 >Y $e3 >P $0060 $33 >M $78f1 $c4 >M $78f2 $60 >M $78f3 $98 >M
T{ op PC S A X Y P -> $78f3 $0c $32 $68 $84 $61 }T T{ $0060 M $78f1 M $78f2 M $78f3 M -> $33 $c4 $60 $98 }T
$0b23 >PC $bc >S $3a >A $3f >X $0e >Y $62 >P $004d $ce >M $0b23 $c4 >M $0b24 $4d >M $0b25 $1a >M
T{ op PC S A X Y P -> $0b25 $bc $3a $3f $0e $60 }T T{ $004d M $0b23 M $0b24 M $0b25 M -> $ce $c4 $4d $1a }T
$3b02 >PC $ff >S $68 >A $36 >X $14 >Y $a2 >P $00a9 $ef >M $3b02 $c4 >M $3b03 $a9 >M $3b04 $6f >M
T{ op PC S A X Y P -> $3b04 $ff $68 $36 $14 $20 }T T{ $00a9 M $3b02 M $3b03 M $3b04 M -> $ef $c4 $a9 $6f }T
$f907 >PC $e1 >S $f6 >A $37 >X $d6 >Y $66 >P $00ac $0c >M $f907 $c4 >M $f908 $ac >M $f909 $e5 >M
T{ op PC S A X Y P -> $f909 $e1 $f6 $37 $d6 $e5 }T T{ $00ac M $f907 M $f908 M $f909 M -> $0c $c4 $ac $e5 }T
( c5 )
$7272 >PC $bc >S $07 >A $78 >X $29 >Y $21 >P $0089 $85 >M $7272 $c5 >M $7273 $89 >M $7274 $ce >M
T{ op PC S A X Y P -> $7274 $bc $07 $78 $29 $a0 }T T{ $0089 M $7272 M $7273 M $7274 M -> $85 $c5 $89 $ce }T
$fa82 >PC $b5 >S $c3 >A $a0 >X $13 >Y $e1 >P $00fc $55 >M $fa82 $c5 >M $fa83 $fc >M $fa84 $b3 >M
T{ op PC S A X Y P -> $fa84 $b5 $c3 $a0 $13 $61 }T T{ $00fc M $fa82 M $fa83 M $fa84 M -> $55 $c5 $fc $b3 }T
$dda7 >PC $cb >S $f8 >A $b0 >X $e3 >Y $a7 >P $0030 $32 >M $dda7 $c5 >M $dda8 $30 >M $dda9 $49 >M
T{ op PC S A X Y P -> $dda9 $cb $f8 $b0 $e3 $a5 }T T{ $0030 M $dda7 M $dda8 M $dda9 M -> $32 $c5 $30 $49 }T
$5cd3 >PC $80 >S $75 >A $c5 >X $c4 >Y $a6 >P $0031 $1a >M $5cd3 $c5 >M $5cd4 $31 >M $5cd5 $39 >M
T{ op PC S A X Y P -> $5cd5 $80 $75 $c5 $c4 $25 }T T{ $0031 M $5cd3 M $5cd4 M $5cd5 M -> $1a $c5 $31 $39 }T
$07e0 >PC $e7 >S $d5 >A $e4 >X $8f >Y $e3 >P $0095 $c2 >M $07e0 $c5 >M $07e1 $95 >M $07e2 $e6 >M
T{ op PC S A X Y P -> $07e2 $e7 $d5 $e4 $8f $61 }T T{ $0095 M $07e0 M $07e1 M $07e2 M -> $c2 $c5 $95 $e6 }T
$958d >PC $72 >S $d7 >A $d6 >X $0a >Y $27 >P $0075 $bb >M $958d $c5 >M $958e $75 >M $958f $dc >M
T{ op PC S A X Y P -> $958f $72 $d7 $d6 $0a $25 }T T{ $0075 M $958d M $958e M $958f M -> $bb $c5 $75 $dc }T
$4bd0 >PC $e0 >S $b6 >A $29 >X $86 >Y $27 >P $006e $a4 >M $4bd0 $c5 >M $4bd1 $6e >M $4bd2 $b3 >M
T{ op PC S A X Y P -> $4bd2 $e0 $b6 $29 $86 $25 }T T{ $006e M $4bd0 M $4bd1 M $4bd2 M -> $a4 $c5 $6e $b3 }T
$9515 >PC $52 >S $8b >A $06 >X $c3 >Y $64 >P $0008 $d3 >M $9515 $c5 >M $9516 $08 >M $9517 $17 >M
T{ op PC S A X Y P -> $9517 $52 $8b $06 $c3 $e4 }T T{ $0008 M $9515 M $9516 M $9517 M -> $d3 $c5 $08 $17 }T
$908f >PC $6c >S $34 >A $b7 >X $59 >Y $24 >P $0018 $13 >M $908f $c5 >M $9090 $18 >M $9091 $e3 >M
T{ op PC S A X Y P -> $9091 $6c $34 $b7 $59 $25 }T T{ $0018 M $908f M $9090 M $9091 M -> $13 $c5 $18 $e3 }T
$08a2 >PC $c0 >S $dd >A $31 >X $3b >Y $e4 >P $005e $80 >M $08a2 $c5 >M $08a3 $5e >M $08a4 $49 >M
T{ op PC S A X Y P -> $08a4 $c0 $dd $31 $3b $65 }T T{ $005e M $08a2 M $08a3 M $08a4 M -> $80 $c5 $5e $49 }T
$8531 >PC $1b >S $21 >A $b8 >X $13 >Y $e5 >P $000e $30 >M $8531 $c5 >M $8532 $0e >M $8533 $60 >M
T{ op PC S A X Y P -> $8533 $1b $21 $b8 $13 $e4 }T T{ $000e M $8531 M $8532 M $8533 M -> $30 $c5 $0e $60 }T
$9ad4 >PC $5d >S $d8 >A $a6 >X $38 >Y $a3 >P $00a3 $e3 >M $9ad4 $c5 >M $9ad5 $a3 >M $9ad6 $bd >M
T{ op PC S A X Y P -> $9ad6 $5d $d8 $a6 $38 $a0 }T T{ $00a3 M $9ad4 M $9ad5 M $9ad6 M -> $e3 $c5 $a3 $bd }T
$d66a >PC $08 >S $af >A $85 >X $d7 >Y $60 >P $009a $19 >M $d66a $c5 >M $d66b $9a >M $d66c $61 >M
T{ op PC S A X Y P -> $d66c $08 $af $85 $d7 $e1 }T T{ $009a M $d66a M $d66b M $d66c M -> $19 $c5 $9a $61 }T
$2284 >PC $af >S $12 >A $10 >X $b1 >Y $a1 >P $000d $5f >M $2284 $c5 >M $2285 $0d >M $2286 $1f >M
T{ op PC S A X Y P -> $2286 $af $12 $10 $b1 $a0 }T T{ $000d M $2284 M $2285 M $2286 M -> $5f $c5 $0d $1f }T
$5b20 >PC $8f >S $a1 >A $70 >X $a7 >Y $e4 >P $0009 $55 >M $5b20 $c5 >M $5b21 $09 >M $5b22 $22 >M
T{ op PC S A X Y P -> $5b22 $8f $a1 $70 $a7 $65 }T T{ $0009 M $5b20 M $5b21 M $5b22 M -> $55 $c5 $09 $22 }T
$0252 >PC $f1 >S $9d >A $f9 >X $51 >Y $e7 >P $003f $07 >M $0252 $c5 >M $0253 $3f >M $0254 $ca >M
T{ op PC S A X Y P -> $0254 $f1 $9d $f9 $51 $e5 }T T{ $003f M $0252 M $0253 M $0254 M -> $07 $c5 $3f $ca }T
( c6 )
$100f >PC $82 >S $ed >A $a4 >X $7e >Y $65 >P $00c2 $51 >M $100f $c6 >M $1010 $c2 >M $1011 $1c >M
T{ op PC S A X Y P -> $1011 $82 $ed $a4 $7e $65 }T T{ $00c2 M $100f M $1010 M $1011 M -> $50 $c6 $c2 $1c }T
$11f4 >PC $6e >S $fa >A $3e >X $c3 >Y $e3 >P $0059 $25 >M $11f4 $c6 >M $11f5 $59 >M $11f6 $10 >M
T{ op PC S A X Y P -> $11f6 $6e $fa $3e $c3 $61 }T T{ $0059 M $11f4 M $11f5 M $11f6 M -> $24 $c6 $59 $10 }T
$776b >PC $eb >S $a3 >A $e5 >X $3e >Y $62 >P $0026 $69 >M $776b $c6 >M $776c $26 >M $776d $35 >M
T{ op PC S A X Y P -> $776d $eb $a3 $e5 $3e $60 }T T{ $0026 M $776b M $776c M $776d M -> $68 $c6 $26 $35 }T
$7546 >PC $16 >S $34 >A $89 >X $ee >Y $a1 >P $004f $74 >M $7546 $c6 >M $7547 $4f >M $7548 $1c >M
T{ op PC S A X Y P -> $7548 $16 $34 $89 $ee $21 }T T{ $004f M $7546 M $7547 M $7548 M -> $73 $c6 $4f $1c }T
$cec7 >PC $8a >S $60 >A $d3 >X $5d >Y $62 >P $00a3 $c1 >M $cec7 $c6 >M $cec8 $a3 >M $cec9 $0c >M
T{ op PC S A X Y P -> $cec9 $8a $60 $d3 $5d $e0 }T T{ $00a3 M $cec7 M $cec8 M $cec9 M -> $c0 $c6 $a3 $0c }T
$9742 >PC $9c >S $ac >A $4f >X $4d >Y $24 >P $0037 $9b >M $9742 $c6 >M $9743 $37 >M $9744 $a6 >M
T{ op PC S A X Y P -> $9744 $9c $ac $4f $4d $a4 }T T{ $0037 M $9742 M $9743 M $9744 M -> $9a $c6 $37 $a6 }T
$3c45 >PC $26 >S $76 >A $48 >X $d9 >Y $e3 >P $0040 $29 >M $3c45 $c6 >M $3c46 $40 >M $3c47 $a1 >M
T{ op PC S A X Y P -> $3c47 $26 $76 $48 $d9 $61 }T T{ $0040 M $3c45 M $3c46 M $3c47 M -> $28 $c6 $40 $a1 }T
$6d13 >PC $e0 >S $47 >A $cd >X $84 >Y $65 >P $00ce $42 >M $6d13 $c6 >M $6d14 $ce >M $6d15 $3d >M
T{ op PC S A X Y P -> $6d15 $e0 $47 $cd $84 $65 }T T{ $00ce M $6d13 M $6d14 M $6d15 M -> $41 $c6 $ce $3d }T
$f54d >PC $d8 >S $72 >A $24 >X $71 >Y $e2 >P $00ee $cd >M $f54d $c6 >M $f54e $ee >M $f54f $11 >M
T{ op PC S A X Y P -> $f54f $d8 $72 $24 $71 $e0 }T T{ $00ee M $f54d M $f54e M $f54f M -> $cc $c6 $ee $11 }T
$b72b >PC $d4 >S $01 >A $a7 >X $a9 >Y $a3 >P $00f1 $15 >M $b72b $c6 >M $b72c $f1 >M $b72d $80 >M
T{ op PC S A X Y P -> $b72d $d4 $01 $a7 $a9 $21 }T T{ $00f1 M $b72b M $b72c M $b72d M -> $14 $c6 $f1 $80 }T
$5465 >PC $64 >S $1f >A $5d >X $76 >Y $e4 >P $0070 $f0 >M $5465 $c6 >M $5466 $70 >M $5467 $9e >M
T{ op PC S A X Y P -> $5467 $64 $1f $5d $76 $e4 }T T{ $0070 M $5465 M $5466 M $5467 M -> $ef $c6 $70 $9e }T
$c7b9 >PC $9f >S $1a >A $27 >X $3c >Y $a2 >P $00d3 $5f >M $c7b9 $c6 >M $c7ba $d3 >M $c7bb $51 >M
T{ op PC S A X Y P -> $c7bb $9f $1a $27 $3c $20 }T T{ $00d3 M $c7b9 M $c7ba M $c7bb M -> $5e $c6 $d3 $51 }T
$9229 >PC $e7 >S $46 >A $c3 >X $c2 >Y $62 >P $00b1 $b3 >M $9229 $c6 >M $922a $b1 >M $922b $eb >M
T{ op PC S A X Y P -> $922b $e7 $46 $c3 $c2 $e0 }T T{ $00b1 M $9229 M $922a M $922b M -> $b2 $c6 $b1 $eb }T
$78fd >PC $49 >S $ee >A $2b >X $eb >Y $27 >P $00e2 $72 >M $78fd $c6 >M $78fe $e2 >M $78ff $bb >M
T{ op PC S A X Y P -> $78ff $49 $ee $2b $eb $25 }T T{ $00e2 M $78fd M $78fe M $78ff M -> $71 $c6 $e2 $bb }T
$1ece >PC $9e >S $6e >A $e6 >X $a8 >Y $60 >P $0061 $89 >M $1ece $c6 >M $1ecf $61 >M $1ed0 $fe >M
T{ op PC S A X Y P -> $1ed0 $9e $6e $e6 $a8 $e0 }T T{ $0061 M $1ece M $1ecf M $1ed0 M -> $88 $c6 $61 $fe }T
$7e05 >PC $9d >S $55 >A $d8 >X $a3 >Y $21 >P $0085 $b3 >M $7e05 $c6 >M $7e06 $85 >M $7e07 $de >M
T{ op PC S A X Y P -> $7e07 $9d $55 $d8 $a3 $a1 }T T{ $0085 M $7e05 M $7e06 M $7e07 M -> $b2 $c6 $85 $de }T
( c7 )
$4493 >PC $ac >S $ad >A $ca >X $24 >Y $25 >P $00be $88 >M $4493 $c7 >M $4494 $be >M $4495 $d4 >M
T{ op PC S A X Y P -> $4495 $ac $ad $ca $24 $25 }T T{ $00be M $4493 M $4494 M $4495 M -> $98 $c7 $be $d4 }T
$6572 >PC $3b >S $6f >A $bd >X $ec >Y $22 >P $0080 $7b >M $6572 $c7 >M $6573 $80 >M $6574 $b1 >M
T{ op PC S A X Y P -> $6574 $3b $6f $bd $ec $22 }T T{ $0080 M $6572 M $6573 M $6574 M -> $7b $c7 $80 $b1 }T
$5db5 >PC $16 >S $8c >A $90 >X $95 >Y $a1 >P $00b4 $74 >M $5db5 $c7 >M $5db6 $b4 >M $5db7 $b0 >M
T{ op PC S A X Y P -> $5db7 $16 $8c $90 $95 $a1 }T T{ $00b4 M $5db5 M $5db6 M $5db7 M -> $74 $c7 $b4 $b0 }T
$19fd >PC $86 >S $f7 >A $59 >X $90 >Y $a3 >P $0003 $17 >M $19fd $c7 >M $19fe $03 >M $19ff $71 >M
T{ op PC S A X Y P -> $19ff $86 $f7 $59 $90 $a3 }T T{ $0003 M $19fd M $19fe M $19ff M -> $17 $c7 $03 $71 }T
$2172 >PC $4a >S $ba >A $04 >X $13 >Y $a7 >P $0015 $5c >M $2172 $c7 >M $2173 $15 >M $2174 $ad >M
T{ op PC S A X Y P -> $2174 $4a $ba $04 $13 $a7 }T T{ $0015 M $2172 M $2173 M $2174 M -> $5c $c7 $15 $ad }T
$fca8 >PC $fe >S $e5 >A $c1 >X $ec >Y $23 >P $00b7 $64 >M $fca8 $c7 >M $fca9 $b7 >M $fcaa $ce >M
T{ op PC S A X Y P -> $fcaa $fe $e5 $c1 $ec $23 }T T{ $00b7 M $fca8 M $fca9 M $fcaa M -> $74 $c7 $b7 $ce }T
$2331 >PC $72 >S $70 >A $03 >X $1c >Y $e4 >P $0010 $c6 >M $2331 $c7 >M $2332 $10 >M $2333 $8a >M
T{ op PC S A X Y P -> $2333 $72 $70 $03 $1c $e4 }T T{ $0010 M $2331 M $2332 M $2333 M -> $d6 $c7 $10 $8a }T
$355c >PC $d9 >S $04 >A $b3 >X $14 >Y $a1 >P $003b $40 >M $355c $c7 >M $355d $3b >M $355e $f6 >M
T{ op PC S A X Y P -> $355e $d9 $04 $b3 $14 $a1 }T T{ $003b M $355c M $355d M $355e M -> $50 $c7 $3b $f6 }T
$8219 >PC $55 >S $71 >A $2b >X $a7 >Y $25 >P $00a4 $56 >M $8219 $c7 >M $821a $a4 >M $821b $36 >M
T{ op PC S A X Y P -> $821b $55 $71 $2b $a7 $25 }T T{ $00a4 M $8219 M $821a M $821b M -> $56 $c7 $a4 $36 }T
$b527 >PC $55 >S $90 >A $7a >X $16 >Y $e4 >P $00f4 $cd >M $b527 $c7 >M $b528 $f4 >M $b529 $97 >M
T{ op PC S A X Y P -> $b529 $55 $90 $7a $16 $e4 }T T{ $00f4 M $b527 M $b528 M $b529 M -> $dd $c7 $f4 $97 }T
$a404 >PC $38 >S $b5 >A $ea >X $4f >Y $a0 >P $000f $25 >M $a404 $c7 >M $a405 $0f >M $a406 $b5 >M
T{ op PC S A X Y P -> $a406 $38 $b5 $ea $4f $a0 }T T{ $000f M $a404 M $a405 M $a406 M -> $35 $c7 $0f $b5 }T
$d85d >PC $ab >S $ee >A $52 >X $8c >Y $a3 >P $005a $d6 >M $d85d $c7 >M $d85e $5a >M $d85f $f2 >M
T{ op PC S A X Y P -> $d85f $ab $ee $52 $8c $a3 }T T{ $005a M $d85d M $d85e M $d85f M -> $d6 $c7 $5a $f2 }T
$58ca >PC $9e >S $c8 >A $dc >X $d7 >Y $61 >P $0061 $7c >M $58ca $c7 >M $58cb $61 >M $58cc $2d >M
T{ op PC S A X Y P -> $58cc $9e $c8 $dc $d7 $61 }T T{ $0061 M $58ca M $58cb M $58cc M -> $7c $c7 $61 $2d }T
$2221 >PC $48 >S $e2 >A $6a >X $80 >Y $61 >P $007c $4f >M $2221 $c7 >M $2222 $7c >M $2223 $ce >M
T{ op PC S A X Y P -> $2223 $48 $e2 $6a $80 $61 }T T{ $007c M $2221 M $2222 M $2223 M -> $5f $c7 $7c $ce }T
$9289 >PC $e8 >S $81 >A $eb >X $00 >Y $66 >P $00b7 $1f >M $9289 $c7 >M $928a $b7 >M $928b $f7 >M
T{ op PC S A X Y P -> $928b $e8 $81 $eb $00 $66 }T T{ $00b7 M $9289 M $928a M $928b M -> $1f $c7 $b7 $f7 }T
$ea4e >PC $55 >S $ee >A $72 >X $1c >Y $a3 >P $00d0 $ea >M $ea4e $c7 >M $ea4f $d0 >M $ea50 $8f >M
T{ op PC S A X Y P -> $ea50 $55 $ee $72 $1c $a3 }T T{ $00d0 M $ea4e M $ea4f M $ea50 M -> $fa $c7 $d0 $8f }T
( c8 )
$f889 >PC $bd >S $bb >A $1c >X $e2 >Y $a0 >P $f889 $c8 >M $f88a $03 >M $f88b $5b >M
T{ op PC S A X Y P -> $f88a $bd $bb $1c $e3 $a0 }T T{ $f889 M $f88a M $f88b M -> $c8 $03 $5b }T
$dde6 >PC $41 >S $5e >A $c5 >X $fd >Y $e5 >P $dde6 $c8 >M $dde7 $33 >M $dde8 $b3 >M
T{ op PC S A X Y P -> $dde7 $41 $5e $c5 $fe $e5 }T T{ $dde6 M $dde7 M $dde8 M -> $c8 $33 $b3 }T
$c1b2 >PC $1b >S $f1 >A $cd >X $74 >Y $66 >P $c1b2 $c8 >M $c1b3 $25 >M $c1b4 $29 >M
T{ op PC S A X Y P -> $c1b3 $1b $f1 $cd $75 $64 }T T{ $c1b2 M $c1b3 M $c1b4 M -> $c8 $25 $29 }T
$f229 >PC $01 >S $9c >A $90 >X $4f >Y $e0 >P $f229 $c8 >M $f22a $bc >M $f22b $68 >M
T{ op PC S A X Y P -> $f22a $01 $9c $90 $50 $60 }T T{ $f229 M $f22a M $f22b M -> $c8 $bc $68 }T
$3e34 >PC $7b >S $1a >A $6d >X $19 >Y $e5 >P $3e34 $c8 >M $3e35 $37 >M $3e36 $b9 >M
T{ op PC S A X Y P -> $3e35 $7b $1a $6d $1a $65 }T T{ $3e34 M $3e35 M $3e36 M -> $c8 $37 $b9 }T
$84c6 >PC $2d >S $7b >A $50 >X $de >Y $67 >P $84c6 $c8 >M $84c7 $6e >M $84c8 $06 >M
T{ op PC S A X Y P -> $84c7 $2d $7b $50 $df $e5 }T T{ $84c6 M $84c7 M $84c8 M -> $c8 $6e $06 }T
$ad3c >PC $58 >S $5a >A $c9 >X $55 >Y $e4 >P $ad3c $c8 >M $ad3d $9f >M $ad3e $73 >M
T{ op PC S A X Y P -> $ad3d $58 $5a $c9 $56 $64 }T T{ $ad3c M $ad3d M $ad3e M -> $c8 $9f $73 }T
$4e0c >PC $6c >S $7c >A $6c >X $2c >Y $a7 >P $4e0c $c8 >M $4e0d $5b >M $4e0e $db >M
T{ op PC S A X Y P -> $4e0d $6c $7c $6c $2d $25 }T T{ $4e0c M $4e0d M $4e0e M -> $c8 $5b $db }T
$671f >PC $00 >S $0f >A $df >X $79 >Y $22 >P $671f $c8 >M $6720 $c8 >M $6721 $6e >M
T{ op PC S A X Y P -> $6720 $00 $0f $df $7a $20 }T T{ $671f M $6720 M $6721 M -> $c8 $c8 $6e }T
$c5e9 >PC $c5 >S $1a >A $98 >X $29 >Y $22 >P $c5e9 $c8 >M $c5ea $15 >M $c5eb $17 >M
T{ op PC S A X Y P -> $c5ea $c5 $1a $98 $2a $20 }T T{ $c5e9 M $c5ea M $c5eb M -> $c8 $15 $17 }T
$103a >PC $84 >S $5b >A $22 >X $7a >Y $67 >P $103a $c8 >M $103b $a3 >M $103c $fe >M
T{ op PC S A X Y P -> $103b $84 $5b $22 $7b $65 }T T{ $103a M $103b M $103c M -> $c8 $a3 $fe }T
$5c5b >PC $3a >S $bc >A $81 >X $c7 >Y $a3 >P $5c5b $c8 >M $5c5c $99 >M $5c5d $42 >M
T{ op PC S A X Y P -> $5c5c $3a $bc $81 $c8 $a1 }T T{ $5c5b M $5c5c M $5c5d M -> $c8 $99 $42 }T
$c4e6 >PC $57 >S $f0 >A $c3 >X $fe >Y $e0 >P $c4e6 $c8 >M $c4e7 $c5 >M $c4e8 $99 >M
T{ op PC S A X Y P -> $c4e7 $57 $f0 $c3 $ff $e0 }T T{ $c4e6 M $c4e7 M $c4e8 M -> $c8 $c5 $99 }T
$3aa6 >PC $a8 >S $d1 >A $48 >X $e2 >Y $a1 >P $3aa6 $c8 >M $3aa7 $d0 >M $3aa8 $05 >M
T{ op PC S A X Y P -> $3aa7 $a8 $d1 $48 $e3 $a1 }T T{ $3aa6 M $3aa7 M $3aa8 M -> $c8 $d0 $05 }T
$c416 >PC $7c >S $fd >A $7c >X $50 >Y $a6 >P $c416 $c8 >M $c417 $9c >M $c418 $ab >M
T{ op PC S A X Y P -> $c417 $7c $fd $7c $51 $24 }T T{ $c416 M $c417 M $c418 M -> $c8 $9c $ab }T
$b0cb >PC $ff >S $a4 >A $2f >X $ac >Y $e4 >P $b0cb $c8 >M $b0cc $b2 >M $b0cd $c5 >M
T{ op PC S A X Y P -> $b0cc $ff $a4 $2f $ad $e4 }T T{ $b0cb M $b0cc M $b0cd M -> $c8 $b2 $c5 }T
( c9 )
$d6dc >PC $b3 >S $d3 >A $d9 >X $e8 >Y $a5 >P $d6dc $c9 >M $d6dd $20 >M $d6de $ae >M
T{ op PC S A X Y P -> $d6de $b3 $d3 $d9 $e8 $a5 }T T{ $d6dc M $d6dd M $d6de M -> $c9 $20 $ae }T
$fd72 >PC $70 >S $17 >A $6a >X $cc >Y $e1 >P $fd72 $c9 >M $fd73 $a1 >M $fd74 $a9 >M
T{ op PC S A X Y P -> $fd74 $70 $17 $6a $cc $60 }T T{ $fd72 M $fd73 M $fd74 M -> $c9 $a1 $a9 }T
$082e >PC $3e >S $af >A $6e >X $b7 >Y $27 >P $082e $c9 >M $082f $18 >M $0830 $b2 >M
T{ op PC S A X Y P -> $0830 $3e $af $6e $b7 $a5 }T T{ $082e M $082f M $0830 M -> $c9 $18 $b2 }T
$b1d7 >PC $fd >S $76 >A $dd >X $40 >Y $21 >P $b1d7 $c9 >M $b1d8 $bf >M $b1d9 $5e >M
T{ op PC S A X Y P -> $b1d9 $fd $76 $dd $40 $a0 }T T{ $b1d7 M $b1d8 M $b1d9 M -> $c9 $bf $5e }T
$85e2 >PC $bf >S $07 >A $81 >X $2f >Y $23 >P $85e2 $c9 >M $85e3 $08 >M $85e4 $32 >M
T{ op PC S A X Y P -> $85e4 $bf $07 $81 $2f $a0 }T T{ $85e2 M $85e3 M $85e4 M -> $c9 $08 $32 }T
$7f86 >PC $fb >S $9e >A $99 >X $7f >Y $24 >P $7f86 $c9 >M $7f87 $e6 >M $7f88 $b1 >M
T{ op PC S A X Y P -> $7f88 $fb $9e $99 $7f $a4 }T T{ $7f86 M $7f87 M $7f88 M -> $c9 $e6 $b1 }T
$2568 >PC $a2 >S $dd >A $00 >X $b4 >Y $e4 >P $2568 $c9 >M $2569 $98 >M $256a $c3 >M
T{ op PC S A X Y P -> $256a $a2 $dd $00 $b4 $65 }T T{ $2568 M $2569 M $256a M -> $c9 $98 $c3 }T
$b587 >PC $79 >S $5b >A $ae >X $80 >Y $e1 >P $b587 $c9 >M $b588 $4b >M $b589 $34 >M
T{ op PC S A X Y P -> $b589 $79 $5b $ae $80 $61 }T T{ $b587 M $b588 M $b589 M -> $c9 $4b $34 }T
$5a9a >PC $e8 >S $b8 >A $8d >X $b2 >Y $66 >P $5a9a $c9 >M $5a9b $41 >M $5a9c $0d >M
T{ op PC S A X Y P -> $5a9c $e8 $b8 $8d $b2 $65 }T T{ $5a9a M $5a9b M $5a9c M -> $c9 $41 $0d }T
$505a >PC $0d >S $d7 >A $cd >X $1a >Y $24 >P $505a $c9 >M $505b $e4 >M $505c $74 >M
T{ op PC S A X Y P -> $505c $0d $d7 $cd $1a $a4 }T T{ $505a M $505b M $505c M -> $c9 $e4 $74 }T
$f4bd >PC $88 >S $2a >A $1e >X $e7 >Y $61 >P $f4bd $c9 >M $f4be $d5 >M $f4bf $a7 >M
T{ op PC S A X Y P -> $f4bf $88 $2a $1e $e7 $60 }T T{ $f4bd M $f4be M $f4bf M -> $c9 $d5 $a7 }T
$a5e6 >PC $21 >S $d7 >A $43 >X $be >Y $a6 >P $a5e6 $c9 >M $a5e7 $90 >M $a5e8 $2e >M
T{ op PC S A X Y P -> $a5e8 $21 $d7 $43 $be $25 }T T{ $a5e6 M $a5e7 M $a5e8 M -> $c9 $90 $2e }T
$1052 >PC $56 >S $28 >A $30 >X $f3 >Y $26 >P $1052 $c9 >M $1053 $ed >M $1054 $d4 >M
T{ op PC S A X Y P -> $1054 $56 $28 $30 $f3 $24 }T T{ $1052 M $1053 M $1054 M -> $c9 $ed $d4 }T
$23c9 >PC $e7 >S $1f >A $fa >X $a8 >Y $66 >P $23c9 $c9 >M $23ca $0d >M $23cb $c3 >M
T{ op PC S A X Y P -> $23cb $e7 $1f $fa $a8 $65 }T T{ $23c9 M $23ca M $23cb M -> $c9 $0d $c3 }T
$ff00 >PC $9e >S $74 >A $ff >X $f1 >Y $20 >P $ff00 $c9 >M $ff01 $62 >M $ff02 $31 >M
T{ op PC S A X Y P -> $ff02 $9e $74 $ff $f1 $21 }T T{ $ff00 M $ff01 M $ff02 M -> $c9 $62 $31 }T
$7385 >PC $5f >S $0a >A $a1 >X $8b >Y $a4 >P $7385 $c9 >M $7386 $ef >M $7387 $28 >M
T{ op PC S A X Y P -> $7387 $5f $0a $a1 $8b $24 }T T{ $7385 M $7386 M $7387 M -> $c9 $ef $28 }T
( ca )
$e9eb >PC $88 >S $a0 >A $d9 >X $b0 >Y $e7 >P $e9eb $ca >M $e9ec $e2 >M $e9ed $d2 >M
T{ op PC S A X Y P -> $e9ec $88 $a0 $d8 $b0 $e5 }T T{ $e9eb M $e9ec M $e9ed M -> $ca $e2 $d2 }T
$eba9 >PC $d0 >S $1b >A $ee >X $48 >Y $e2 >P $eba9 $ca >M $ebaa $2e >M $ebab $8a >M
T{ op PC S A X Y P -> $ebaa $d0 $1b $ed $48 $e0 }T T{ $eba9 M $ebaa M $ebab M -> $ca $2e $8a }T
$d087 >PC $45 >S $dc >A $86 >X $dc >Y $24 >P $d087 $ca >M $d088 $c2 >M $d089 $ac >M
T{ op PC S A X Y P -> $d088 $45 $dc $85 $dc $a4 }T T{ $d087 M $d088 M $d089 M -> $ca $c2 $ac }T
$7e93 >PC $3f >S $ab >A $fe >X $c5 >Y $65 >P $7e93 $ca >M $7e94 $79 >M $7e95 $01 >M
T{ op PC S A X Y P -> $7e94 $3f $ab $fd $c5 $e5 }T T{ $7e93 M $7e94 M $7e95 M -> $ca $79 $01 }T
$a427 >PC $00 >S $30 >A $1c >X $b4 >Y $61 >P $a427 $ca >M $a428 $f3 >M $a429 $10 >M
T{ op PC S A X Y P -> $a428 $00 $30 $1b $b4 $61 }T T{ $a427 M $a428 M $a429 M -> $ca $f3 $10 }T
$86a9 >PC $ba >S $63 >A $64 >X $4b >Y $20 >P $86a9 $ca >M $86aa $9e >M $86ab $6d >M
T{ op PC S A X Y P -> $86aa $ba $63 $63 $4b $20 }T T{ $86a9 M $86aa M $86ab M -> $ca $9e $6d }T
$2d78 >PC $ec >S $32 >A $cb >X $77 >Y $a4 >P $2d78 $ca >M $2d79 $69 >M $2d7a $74 >M
T{ op PC S A X Y P -> $2d79 $ec $32 $ca $77 $a4 }T T{ $2d78 M $2d79 M $2d7a M -> $ca $69 $74 }T
$c507 >PC $98 >S $27 >A $14 >X $1e >Y $e3 >P $c507 $ca >M $c508 $b1 >M $c509 $e8 >M
T{ op PC S A X Y P -> $c508 $98 $27 $13 $1e $61 }T T{ $c507 M $c508 M $c509 M -> $ca $b1 $e8 }T
$3790 >PC $f7 >S $be >A $1a >X $4e >Y $25 >P $3790 $ca >M $3791 $36 >M $3792 $6d >M
T{ op PC S A X Y P -> $3791 $f7 $be $19 $4e $25 }T T{ $3790 M $3791 M $3792 M -> $ca $36 $6d }T
$afce >PC $93 >S $df >A $d7 >X $16 >Y $20 >P $afce $ca >M $afcf $75 >M $afd0 $2f >M
T{ op PC S A X Y P -> $afcf $93 $df $d6 $16 $a0 }T T{ $afce M $afcf M $afd0 M -> $ca $75 $2f }T
$c2e7 >PC $c5 >S $37 >A $f2 >X $f0 >Y $e1 >P $c2e7 $ca >M $c2e8 $e8 >M $c2e9 $74 >M
T{ op PC S A X Y P -> $c2e8 $c5 $37 $f1 $f0 $e1 }T T{ $c2e7 M $c2e8 M $c2e9 M -> $ca $e8 $74 }T
$021f >PC $6d >S $c6 >A $c3 >X $f6 >Y $a6 >P $021f $ca >M $0220 $ee >M $0221 $87 >M
T{ op PC S A X Y P -> $0220 $6d $c6 $c2 $f6 $a4 }T T{ $021f M $0220 M $0221 M -> $ca $ee $87 }T
$8cdb >PC $5d >S $3a >A $a0 >X $6e >Y $25 >P $8cdb $ca >M $8cdc $2b >M $8cdd $30 >M
T{ op PC S A X Y P -> $8cdc $5d $3a $9f $6e $a5 }T T{ $8cdb M $8cdc M $8cdd M -> $ca $2b $30 }T
$5cd5 >PC $f0 >S $10 >A $fe >X $3c >Y $a5 >P $5cd5 $ca >M $5cd6 $7e >M $5cd7 $10 >M
T{ op PC S A X Y P -> $5cd6 $f0 $10 $fd $3c $a5 }T T{ $5cd5 M $5cd6 M $5cd7 M -> $ca $7e $10 }T
$f5a8 >PC $6d >S $61 >A $48 >X $63 >Y $61 >P $f5a8 $ca >M $f5a9 $8d >M $f5aa $06 >M
T{ op PC S A X Y P -> $f5a9 $6d $61 $47 $63 $61 }T T{ $f5a8 M $f5a9 M $f5aa M -> $ca $8d $06 }T
$376f >PC $1f >S $b3 >A $7c >X $f0 >Y $60 >P $376f $ca >M $3770 $db >M $3771 $27 >M
T{ op PC S A X Y P -> $3770 $1f $b3 $7b $f0 $60 }T T{ $376f M $3770 M $3771 M -> $ca $db $27 }T
( cb )
\ Skipping invalid json wdc65c02/v1/cb.json
( cc )
$368b >PC $00 >S $45 >A $81 >X $c5 >Y $e2 >P $2fe9 $4d >M $368b $cc >M $368c $e9 >M $368d $2f >M $368e $2b >M
T{ op PC S A X Y P -> $368e $00 $45 $81 $c5 $61 }T T{ $2fe9 M $368b M $368c M $368d M $368e M -> $4d $cc $e9 $2f $2b }T
$0e61 >PC $bb >S $33 >A $33 >X $61 >Y $24 >P $0e61 $cc >M $0e62 $a6 >M $0e63 $62 >M $0e64 $ca >M $62a6 $e5 >M
T{ op PC S A X Y P -> $0e64 $bb $33 $33 $61 $24 }T T{ $0e61 M $0e62 M $0e63 M $0e64 M $62a6 M -> $cc $a6 $62 $ca $e5 }T
$cb11 >PC $06 >S $3d >A $25 >X $52 >Y $23 >P $9d82 $3c >M $cb11 $cc >M $cb12 $82 >M $cb13 $9d >M $cb14 $af >M
T{ op PC S A X Y P -> $cb14 $06 $3d $25 $52 $21 }T T{ $9d82 M $cb11 M $cb12 M $cb13 M $cb14 M -> $3c $cc $82 $9d $af }T
$217c >PC $a4 >S $d9 >A $88 >X $22 >Y $23 >P $217c $cc >M $217d $2a >M $217e $2a >M $217f $92 >M $2a2a $5c >M
T{ op PC S A X Y P -> $217f $a4 $d9 $88 $22 $a0 }T T{ $217c M $217d M $217e M $217f M $2a2a M -> $cc $2a $2a $92 $5c }T
$3026 >PC $c2 >S $ba >A $70 >X $92 >Y $a6 >P $3026 $cc >M $3027 $b6 >M $3028 $c4 >M $3029 $34 >M $c4b6 $95 >M
T{ op PC S A X Y P -> $3029 $c2 $ba $70 $92 $a4 }T T{ $3026 M $3027 M $3028 M $3029 M $c4b6 M -> $cc $b6 $c4 $34 $95 }T
$b718 >PC $83 >S $47 >A $e1 >X $49 >Y $e5 >P $b718 $cc >M $b719 $f5 >M $b71a $f2 >M $b71b $11 >M $f2f5 $fc >M
T{ op PC S A X Y P -> $b71b $83 $47 $e1 $49 $64 }T T{ $b718 M $b719 M $b71a M $b71b M $f2f5 M -> $cc $f5 $f2 $11 $fc }T
$8cb9 >PC $f7 >S $31 >A $08 >X $9b >Y $a6 >P $3ea6 $6c >M $8cb9 $cc >M $8cba $a6 >M $8cbb $3e >M $8cbc $bb >M
T{ op PC S A X Y P -> $8cbc $f7 $31 $08 $9b $25 }T T{ $3ea6 M $8cb9 M $8cba M $8cbb M $8cbc M -> $6c $cc $a6 $3e $bb }T
$7ebd >PC $f6 >S $8c >A $e7 >X $b6 >Y $a4 >P $7ebd $cc >M $7ebe $b1 >M $7ebf $ab >M $7ec0 $c6 >M $abb1 $60 >M
T{ op PC S A X Y P -> $7ec0 $f6 $8c $e7 $b6 $25 }T T{ $7ebd M $7ebe M $7ebf M $7ec0 M $abb1 M -> $cc $b1 $ab $c6 $60 }T
$0c92 >PC $81 >S $f7 >A $5b >X $65 >Y $66 >P $0c92 $cc >M $0c93 $94 >M $0c94 $df >M $0c95 $be >M $df94 $4a >M
T{ op PC S A X Y P -> $0c95 $81 $f7 $5b $65 $65 }T T{ $0c92 M $0c93 M $0c94 M $0c95 M $df94 M -> $cc $94 $df $be $4a }T
$caf4 >PC $87 >S $dc >A $e7 >X $bd >Y $67 >P $9bc7 $e3 >M $caf4 $cc >M $caf5 $c7 >M $caf6 $9b >M $caf7 $b6 >M
T{ op PC S A X Y P -> $caf7 $87 $dc $e7 $bd $e4 }T T{ $9bc7 M $caf4 M $caf5 M $caf6 M $caf7 M -> $e3 $cc $c7 $9b $b6 }T
$0f70 >PC $1c >S $7f >A $5b >X $36 >Y $a6 >P $0f70 $cc >M $0f71 $90 >M $0f72 $d7 >M $0f73 $aa >M $d790 $37 >M
T{ op PC S A X Y P -> $0f73 $1c $7f $5b $36 $a4 }T T{ $0f70 M $0f71 M $0f72 M $0f73 M $d790 M -> $cc $90 $d7 $aa $37 }T
$64c3 >PC $f1 >S $d6 >A $f1 >X $c2 >Y $62 >P $64c3 $cc >M $64c4 $7a >M $64c5 $e7 >M $64c6 $45 >M $e77a $ed >M
T{ op PC S A X Y P -> $64c6 $f1 $d6 $f1 $c2 $e0 }T T{ $64c3 M $64c4 M $64c5 M $64c6 M $e77a M -> $cc $7a $e7 $45 $ed }T
$ae08 >PC $04 >S $c5 >A $df >X $57 >Y $64 >P $3e7f $d3 >M $ae08 $cc >M $ae09 $7f >M $ae0a $3e >M $ae0b $a1 >M
T{ op PC S A X Y P -> $ae0b $04 $c5 $df $57 $e4 }T T{ $3e7f M $ae08 M $ae09 M $ae0a M $ae0b M -> $d3 $cc $7f $3e $a1 }T
$16d6 >PC $77 >S $f5 >A $45 >X $3c >Y $64 >P $16d6 $cc >M $16d7 $93 >M $16d8 $18 >M $16d9 $0e >M $1893 $04 >M
T{ op PC S A X Y P -> $16d9 $77 $f5 $45 $3c $65 }T T{ $16d6 M $16d7 M $16d8 M $16d9 M $1893 M -> $cc $93 $18 $0e $04 }T
$d51a >PC $ac >S $8d >A $ab >X $12 >Y $e3 >P $a7d5 $ed >M $d51a $cc >M $d51b $d5 >M $d51c $a7 >M $d51d $d5 >M
T{ op PC S A X Y P -> $d51d $ac $8d $ab $12 $60 }T T{ $a7d5 M $d51a M $d51b M $d51c M $d51d M -> $ed $cc $d5 $a7 $d5 }T
$2daa >PC $e1 >S $9e >A $69 >X $9c >Y $67 >P $2daa $cc >M $2dab $e5 >M $2dac $d6 >M $2dad $e2 >M $d6e5 $da >M
T{ op PC S A X Y P -> $2dad $e1 $9e $69 $9c $e4 }T T{ $2daa M $2dab M $2dac M $2dad M $d6e5 M -> $cc $e5 $d6 $e2 $da }T
( cd )
$a162 >PC $09 >S $ac >A $2d >X $f8 >Y $e1 >P $a162 $cd >M $a163 $32 >M $a164 $a4 >M $a165 $16 >M $a432 $e3 >M
T{ op PC S A X Y P -> $a165 $09 $ac $2d $f8 $e0 }T T{ $a162 M $a163 M $a164 M $a165 M $a432 M -> $cd $32 $a4 $16 $e3 }T
$9dc6 >PC $0d >S $d5 >A $b4 >X $b1 >Y $61 >P $9dc6 $cd >M $9dc7 $4e >M $9dc8 $f1 >M $9dc9 $85 >M $f14e $86 >M
T{ op PC S A X Y P -> $9dc9 $0d $d5 $b4 $b1 $61 }T T{ $9dc6 M $9dc7 M $9dc8 M $9dc9 M $f14e M -> $cd $4e $f1 $85 $86 }T
$d048 >PC $77 >S $91 >A $bd >X $34 >Y $a1 >P $94db $7c >M $d048 $cd >M $d049 $db >M $d04a $94 >M $d04b $ec >M
T{ op PC S A X Y P -> $d04b $77 $91 $bd $34 $21 }T T{ $94db M $d048 M $d049 M $d04a M $d04b M -> $7c $cd $db $94 $ec }T
$817d >PC $19 >S $12 >A $a3 >X $42 >Y $21 >P $817d $cd >M $817e $35 >M $817f $d6 >M $8180 $02 >M $d635 $15 >M
T{ op PC S A X Y P -> $8180 $19 $12 $a3 $42 $a0 }T T{ $817d M $817e M $817f M $8180 M $d635 M -> $cd $35 $d6 $02 $15 }T
$505c >PC $b5 >S $4c >A $f1 >X $54 >Y $23 >P $505c $cd >M $505d $a7 >M $505e $c6 >M $505f $5f >M $c6a7 $65 >M
T{ op PC S A X Y P -> $505f $b5 $4c $f1 $54 $a0 }T T{ $505c M $505d M $505e M $505f M $c6a7 M -> $cd $a7 $c6 $5f $65 }T
$183d >PC $0e >S $1a >A $aa >X $ab >Y $a2 >P $183d $cd >M $183e $f6 >M $183f $42 >M $1840 $50 >M $42f6 $71 >M
T{ op PC S A X Y P -> $1840 $0e $1a $aa $ab $a0 }T T{ $183d M $183e M $183f M $1840 M $42f6 M -> $cd $f6 $42 $50 $71 }T
$a034 >PC $8d >S $5a >A $07 >X $50 >Y $61 >P $6ddf $d4 >M $a034 $cd >M $a035 $df >M $a036 $6d >M $a037 $91 >M
T{ op PC S A X Y P -> $a037 $8d $5a $07 $50 $e0 }T T{ $6ddf M $a034 M $a035 M $a036 M $a037 M -> $d4 $cd $df $6d $91 }T
$2478 >PC $22 >S $2d >A $23 >X $44 >Y $a1 >P $2478 $cd >M $2479 $7a >M $247a $6f >M $247b $fa >M $6f7a $f1 >M
T{ op PC S A X Y P -> $247b $22 $2d $23 $44 $20 }T T{ $2478 M $2479 M $247a M $247b M $6f7a M -> $cd $7a $6f $fa $f1 }T
$5d69 >PC $49 >S $82 >A $c0 >X $63 >Y $a4 >P $5d69 $cd >M $5d6a $3e >M $5d6b $9f >M $5d6c $dd >M $9f3e $46 >M
T{ op PC S A X Y P -> $5d6c $49 $82 $c0 $63 $25 }T T{ $5d69 M $5d6a M $5d6b M $5d6c M $9f3e M -> $cd $3e $9f $dd $46 }T
$7d16 >PC $92 >S $c9 >A $db >X $b2 >Y $e0 >P $7d16 $cd >M $7d17 $05 >M $7d18 $9e >M $7d19 $93 >M $9e05 $90 >M
T{ op PC S A X Y P -> $7d19 $92 $c9 $db $b2 $61 }T T{ $7d16 M $7d17 M $7d18 M $7d19 M $9e05 M -> $cd $05 $9e $93 $90 }T
$fd94 >PC $f7 >S $8d >A $b6 >X $32 >Y $67 >P $c3de $3f >M $fd94 $cd >M $fd95 $de >M $fd96 $c3 >M $fd97 $6e >M
T{ op PC S A X Y P -> $fd97 $f7 $8d $b6 $32 $65 }T T{ $c3de M $fd94 M $fd95 M $fd96 M $fd97 M -> $3f $cd $de $c3 $6e }T
$982a >PC $42 >S $8c >A $13 >X $4a >Y $e7 >P $7228 $6c >M $982a $cd >M $982b $28 >M $982c $72 >M $982d $83 >M
T{ op PC S A X Y P -> $982d $42 $8c $13 $4a $65 }T T{ $7228 M $982a M $982b M $982c M $982d M -> $6c $cd $28 $72 $83 }T
$6908 >PC $57 >S $df >A $a3 >X $19 >Y $65 >P $6908 $cd >M $6909 $d6 >M $690a $bb >M $690b $f2 >M $bbd6 $40 >M
T{ op PC S A X Y P -> $690b $57 $df $a3 $19 $e5 }T T{ $6908 M $6909 M $690a M $690b M $bbd6 M -> $cd $d6 $bb $f2 $40 }T
$b9bb >PC $98 >S $be >A $c6 >X $1f >Y $23 >P $b9bb $cd >M $b9bc $13 >M $b9bd $bf >M $b9be $77 >M $bf13 $c1 >M
T{ op PC S A X Y P -> $b9be $98 $be $c6 $1f $a0 }T T{ $b9bb M $b9bc M $b9bd M $b9be M $bf13 M -> $cd $13 $bf $77 $c1 }T
$5eba >PC $18 >S $0a >A $7a >X $8c >Y $26 >P $5eba $cd >M $5ebb $89 >M $5ebc $ee >M $5ebd $4e >M $ee89 $1d >M
T{ op PC S A X Y P -> $5ebd $18 $0a $7a $8c $a4 }T T{ $5eba M $5ebb M $5ebc M $5ebd M $ee89 M -> $cd $89 $ee $4e $1d }T
$6918 >PC $ae >S $f6 >A $58 >X $c1 >Y $e0 >P $3a12 $00 >M $6918 $cd >M $6919 $12 >M $691a $3a >M $691b $b3 >M
T{ op PC S A X Y P -> $691b $ae $f6 $58 $c1 $e1 }T T{ $3a12 M $6918 M $6919 M $691a M $691b M -> $00 $cd $12 $3a $b3 }T
( ce )
$c5a9 >PC $b8 >S $9f >A $c9 >X $92 >Y $61 >P $c5a9 $ce >M $c5aa $40 >M $c5ab $fb >M $c5ac $eb >M $fb40 $4d >M
T{ op PC S A X Y P -> $c5ac $b8 $9f $c9 $92 $61 }T T{ $c5a9 M $c5aa M $c5ab M $c5ac M $fb40 M -> $ce $40 $fb $eb $4c }T
$1a26 >PC $27 >S $8d >A $0e >X $00 >Y $65 >P $1a26 $ce >M $1a27 $44 >M $1a28 $6e >M $1a29 $b5 >M $6e44 $36 >M
T{ op PC S A X Y P -> $1a29 $27 $8d $0e $00 $65 }T T{ $1a26 M $1a27 M $1a28 M $1a29 M $6e44 M -> $ce $44 $6e $b5 $35 }T
$d058 >PC $96 >S $4a >A $8f >X $12 >Y $a7 >P $bffa $51 >M $d058 $ce >M $d059 $fa >M $d05a $bf >M $d05b $52 >M
T{ op PC S A X Y P -> $d05b $96 $4a $8f $12 $25 }T T{ $bffa M $d058 M $d059 M $d05a M $d05b M -> $50 $ce $fa $bf $52 }T
$1998 >PC $f9 >S $c0 >A $fb >X $91 >Y $62 >P $1998 $ce >M $1999 $f7 >M $199a $3a >M $199b $04 >M $3af7 $61 >M
T{ op PC S A X Y P -> $199b $f9 $c0 $fb $91 $60 }T T{ $1998 M $1999 M $199a M $199b M $3af7 M -> $ce $f7 $3a $04 $60 }T
$f177 >PC $bd >S $3f >A $17 >X $16 >Y $a4 >P $9790 $41 >M $f177 $ce >M $f178 $90 >M $f179 $97 >M $f17a $7f >M
T{ op PC S A X Y P -> $f17a $bd $3f $17 $16 $24 }T T{ $9790 M $f177 M $f178 M $f179 M $f17a M -> $40 $ce $90 $97 $7f }T
$805c >PC $c8 >S $96 >A $3b >X $c2 >Y $20 >P $53b2 $bb >M $805c $ce >M $805d $b2 >M $805e $53 >M $805f $52 >M
T{ op PC S A X Y P -> $805f $c8 $96 $3b $c2 $a0 }T T{ $53b2 M $805c M $805d M $805e M $805f M -> $ba $ce $b2 $53 $52 }T
$3b46 >PC $bd >S $02 >A $19 >X $b8 >Y $66 >P $3b46 $ce >M $3b47 $6d >M $3b48 $62 >M $3b49 $3c >M $626d $ff >M
T{ op PC S A X Y P -> $3b49 $bd $02 $19 $b8 $e4 }T T{ $3b46 M $3b47 M $3b48 M $3b49 M $626d M -> $ce $6d $62 $3c $fe }T
$9001 >PC $94 >S $f1 >A $c4 >X $50 >Y $65 >P $9001 $ce >M $9002 $70 >M $9003 $a6 >M $9004 $d1 >M $a670 $0d >M
T{ op PC S A X Y P -> $9004 $94 $f1 $c4 $50 $65 }T T{ $9001 M $9002 M $9003 M $9004 M $a670 M -> $ce $70 $a6 $d1 $0c }T
$4a21 >PC $41 >S $22 >A $c9 >X $d5 >Y $a0 >P $4a21 $ce >M $4a22 $c2 >M $4a23 $a6 >M $4a24 $57 >M $a6c2 $15 >M
T{ op PC S A X Y P -> $4a24 $41 $22 $c9 $d5 $20 }T T{ $4a21 M $4a22 M $4a23 M $4a24 M $a6c2 M -> $ce $c2 $a6 $57 $14 }T
$ce6f >PC $14 >S $f8 >A $cf >X $45 >Y $e5 >P $8b86 $a3 >M $ce6f $ce >M $ce70 $86 >M $ce71 $8b >M $ce72 $5e >M
T{ op PC S A X Y P -> $ce72 $14 $f8 $cf $45 $e5 }T T{ $8b86 M $ce6f M $ce70 M $ce71 M $ce72 M -> $a2 $ce $86 $8b $5e }T
$dd9a >PC $e5 >S $46 >A $7d >X $ee >Y $e1 >P $2249 $5b >M $dd9a $ce >M $dd9b $49 >M $dd9c $22 >M $dd9d $ee >M
T{ op PC S A X Y P -> $dd9d $e5 $46 $7d $ee $61 }T T{ $2249 M $dd9a M $dd9b M $dd9c M $dd9d M -> $5a $ce $49 $22 $ee }T
$2329 >PC $83 >S $61 >A $bd >X $11 >Y $a3 >P $2329 $ce >M $232a $7d >M $232b $8e >M $232c $93 >M $8e7d $f3 >M
T{ op PC S A X Y P -> $232c $83 $61 $bd $11 $a1 }T T{ $2329 M $232a M $232b M $232c M $8e7d M -> $ce $7d $8e $93 $f2 }T
$6691 >PC $b5 >S $a6 >A $82 >X $4d >Y $64 >P $6691 $ce >M $6692 $12 >M $6693 $c2 >M $6694 $c0 >M $c212 $a6 >M
T{ op PC S A X Y P -> $6694 $b5 $a6 $82 $4d $e4 }T T{ $6691 M $6692 M $6693 M $6694 M $c212 M -> $ce $12 $c2 $c0 $a5 }T
$1b59 >PC $b5 >S $97 >A $67 >X $d2 >Y $27 >P $02f6 $e3 >M $1b59 $ce >M $1b5a $f6 >M $1b5b $02 >M $1b5c $da >M
T{ op PC S A X Y P -> $1b5c $b5 $97 $67 $d2 $a5 }T T{ $02f6 M $1b59 M $1b5a M $1b5b M $1b5c M -> $e2 $ce $f6 $02 $da }T
$5c56 >PC $ab >S $0f >A $df >X $47 >Y $e3 >P $5219 $14 >M $5c56 $ce >M $5c57 $19 >M $5c58 $52 >M $5c59 $1b >M
T{ op PC S A X Y P -> $5c59 $ab $0f $df $47 $61 }T T{ $5219 M $5c56 M $5c57 M $5c58 M $5c59 M -> $13 $ce $19 $52 $1b }T
$b79d >PC $35 >S $45 >A $1c >X $cf >Y $e2 >P $3bfa $86 >M $b79d $ce >M $b79e $fa >M $b79f $3b >M $b7a0 $c4 >M
T{ op PC S A X Y P -> $b7a0 $35 $45 $1c $cf $e0 }T T{ $3bfa M $b79d M $b79e M $b79f M $b7a0 M -> $85 $ce $fa $3b $c4 }T
( cf )
$ce9e >PC $a4 >S $ce >A $85 >X $f9 >Y $a5 >P $002d $8c >M $ce9e $cf >M $ce9f $2d >M $cea0 $5a >M $cea1 $e7 >M $cefb $e2 >M
T{ op PC S A X Y P -> $cea1 $a4 $ce $85 $f9 $a5 }T T{ $002d M $ce9e M $ce9f M $cea0 M $cea1 M $cefb M -> $8c $cf $2d $5a $e7 $e2 }T
$9404 >PC $7f >S $66 >A $5e >X $46 >Y $a5 >P $0026 $1c >M $9397 $f6 >M $9404 $cf >M $9405 $26 >M $9406 $90 >M $9497 $67 >M
T{ op PC S A X Y P -> $9397 $7f $66 $5e $46 $a5 }T T{ $0026 M $9397 M $9404 M $9405 M $9406 M $9497 M -> $1c $f6 $cf $26 $90 $67 }T
$cb6c >PC $27 >S $1c >A $bc >X $b5 >Y $61 >P $00fa $c5 >M $cb6c $cf >M $cb6d $fa >M $cb6e $58 >M $cb6f $2e >M $cbc7 $af >M
T{ op PC S A X Y P -> $cb6f $27 $1c $bc $b5 $61 }T T{ $00fa M $cb6c M $cb6d M $cb6e M $cb6f M $cbc7 M -> $c5 $cf $fa $58 $2e $af }T
$10e0 >PC $6e >S $84 >A $75 >X $f0 >Y $e4 >P $00f4 $fa >M $1022 $92 >M $10e0 $cf >M $10e1 $f4 >M $10e2 $3f >M $1122 $07 >M
T{ op PC S A X Y P -> $1122 $6e $84 $75 $f0 $e4 }T T{ $00f4 M $1022 M $10e0 M $10e1 M $10e2 M $1122 M -> $fa $92 $cf $f4 $3f $07 }T
$4dd5 >PC $4b >S $ff >A $19 >X $b0 >Y $62 >P $008a $7d >M $4dd5 $cf >M $4dd6 $8a >M $4dd7 $22 >M $4dfa $e9 >M
T{ op PC S A X Y P -> $4dfa $4b $ff $19 $b0 $62 }T T{ $008a M $4dd5 M $4dd6 M $4dd7 M $4dfa M -> $7d $cf $8a $22 $e9 }T
$5311 >PC $5e >S $ce >A $37 >X $82 >Y $27 >P $0041 $1c >M $5311 $cf >M $5312 $41 >M $5313 $08 >M $531c $7a >M
T{ op PC S A X Y P -> $531c $5e $ce $37 $82 $27 }T T{ $0041 M $5311 M $5312 M $5313 M $531c M -> $1c $cf $41 $08 $7a }T
$46f2 >PC $b7 >S $1f >A $c3 >X $ed >Y $a2 >P $004b $c7 >M $469a $90 >M $46f2 $cf >M $46f3 $4b >M $46f4 $a5 >M $46f5 $ca >M
T{ op PC S A X Y P -> $46f5 $b7 $1f $c3 $ed $a2 }T T{ $004b M $469a M $46f2 M $46f3 M $46f4 M $46f5 M -> $c7 $90 $cf $4b $a5 $ca }T
$a30d >PC $43 >S $44 >A $50 >X $96 >Y $21 >P $00f3 $7c >M $a30d $cf >M $a30e $f3 >M $a30f $69 >M $a379 $be >M
T{ op PC S A X Y P -> $a379 $43 $44 $50 $96 $21 }T T{ $00f3 M $a30d M $a30e M $a30f M $a379 M -> $7c $cf $f3 $69 $be }T
$4077 >PC $4b >S $a1 >A $14 >X $21 >Y $62 >P $00ed $36 >M $4013 $52 >M $4077 $cf >M $4078 $ed >M $4079 $99 >M
T{ op PC S A X Y P -> $4013 $4b $a1 $14 $21 $62 }T T{ $00ed M $4013 M $4077 M $4078 M $4079 M -> $36 $52 $cf $ed $99 }T
$4393 >PC $3a >S $95 >A $6d >X $2d >Y $22 >P $00b1 $e5 >M $4393 $cf >M $4394 $b1 >M $4395 $2b >M $4396 $a4 >M $43c1 $f3 >M
T{ op PC S A X Y P -> $4396 $3a $95 $6d $2d $22 }T T{ $00b1 M $4393 M $4394 M $4395 M $4396 M $43c1 M -> $e5 $cf $b1 $2b $a4 $f3 }T
$a697 >PC $74 >S $c5 >A $e3 >X $43 >Y $62 >P $00dd $71 >M $a697 $cf >M $a698 $dd >M $a699 $62 >M $a6fc $d9 >M
T{ op PC S A X Y P -> $a6fc $74 $c5 $e3 $43 $62 }T T{ $00dd M $a697 M $a698 M $a699 M $a6fc M -> $71 $cf $dd $62 $d9 }T
$b715 >PC $19 >S $27 >A $60 >X $b5 >Y $e6 >P $004b $b8 >M $b6c4 $fd >M $b715 $cf >M $b716 $4b >M $b717 $ac >M $b7c4 $38 >M
T{ op PC S A X Y P -> $b6c4 $19 $27 $60 $b5 $e6 }T T{ $004b M $b6c4 M $b715 M $b716 M $b717 M $b7c4 M -> $b8 $fd $cf $4b $ac $38 }T
$7f1f >PC $05 >S $91 >A $c5 >X $e6 >Y $64 >P $0017 $24 >M $7f1f $cf >M $7f20 $17 >M $7f21 $d9 >M $7f22 $6f >M $7ffb $f0 >M
T{ op PC S A X Y P -> $7f22 $05 $91 $c5 $e6 $64 }T T{ $0017 M $7f1f M $7f20 M $7f21 M $7f22 M $7ffb M -> $24 $cf $17 $d9 $6f $f0 }T
$a59a >PC $e8 >S $ba >A $5c >X $22 >Y $e7 >P $0062 $18 >M $a537 $11 >M $a59a $cf >M $a59b $62 >M $a59c $9a >M
T{ op PC S A X Y P -> $a537 $e8 $ba $5c $22 $e7 }T T{ $0062 M $a537 M $a59a M $a59b M $a59c M -> $18 $11 $cf $62 $9a }T
$636b >PC $21 >S $c5 >A $28 >X $16 >Y $20 >P $00ae $bc >M $636b $cf >M $636c $ae >M $636d $44 >M $63b2 $08 >M
T{ op PC S A X Y P -> $63b2 $21 $c5 $28 $16 $20 }T T{ $00ae M $636b M $636c M $636d M $63b2 M -> $bc $cf $ae $44 $08 }T
$1332 >PC $00 >S $e7 >A $97 >X $d2 >Y $60 >P $0065 $d6 >M $1332 $cf >M $1333 $65 >M $1334 $6c >M $13a1 $ae >M
T{ op PC S A X Y P -> $13a1 $00 $e7 $97 $d2 $60 }T T{ $0065 M $1332 M $1333 M $1334 M $13a1 M -> $d6 $cf $65 $6c $ae }T
( d0 )
$dc4e >PC $46 >S $f2 >A $eb >X $1d >Y $63 >P $dc4e $d0 >M $dc4f $51 >M $dc50 $f2 >M
T{ op PC S A X Y P -> $dc50 $46 $f2 $eb $1d $63 }T T{ $dc4e M $dc4f M $dc50 M -> $d0 $51 $f2 }T
$bc7e >PC $bb >S $fd >A $fb >X $30 >Y $a0 >P $bc7e $d0 >M $bc7f $16 >M $bc80 $eb >M $bc96 $46 >M
T{ op PC S A X Y P -> $bc96 $bb $fd $fb $30 $a0 }T T{ $bc7e M $bc7f M $bc80 M $bc96 M -> $d0 $16 $eb $46 }T
$d2d3 >PC $05 >S $2f >A $ec >X $99 >Y $25 >P $d2d3 $d0 >M $d2d4 $ff >M $d2d5 $56 >M
T{ op PC S A X Y P -> $d2d4 $05 $2f $ec $99 $25 }T T{ $d2d3 M $d2d4 M $d2d5 M -> $d0 $ff $56 }T
$37ac >PC $e3 >S $d2 >A $dd >X $50 >Y $61 >P $37ac $d0 >M $37ad $41 >M $37ae $a7 >M $37ef $ee >M
T{ op PC S A X Y P -> $37ef $e3 $d2 $dd $50 $61 }T T{ $37ac M $37ad M $37ae M $37ef M -> $d0 $41 $a7 $ee }T
$a4e5 >PC $bb >S $32 >A $64 >X $9f >Y $27 >P $a4e5 $d0 >M $a4e6 $6a >M $a4e7 $d1 >M
T{ op PC S A X Y P -> $a4e7 $bb $32 $64 $9f $27 }T T{ $a4e5 M $a4e6 M $a4e7 M -> $d0 $6a $d1 }T
$d3d0 >PC $d6 >S $12 >A $e1 >X $70 >Y $64 >P $d327 $44 >M $d3d0 $d0 >M $d3d1 $55 >M $d3d2 $0b >M $d427 $da >M
T{ op PC S A X Y P -> $d427 $d6 $12 $e1 $70 $64 }T T{ $d327 M $d3d0 M $d3d1 M $d3d2 M $d427 M -> $44 $d0 $55 $0b $da }T
$5c79 >PC $d3 >S $f2 >A $c6 >X $3b >Y $64 >P $5c6a $ef >M $5c79 $d0 >M $5c7a $ef >M $5c7b $f8 >M
T{ op PC S A X Y P -> $5c6a $d3 $f2 $c6 $3b $64 }T T{ $5c6a M $5c79 M $5c7a M $5c7b M -> $ef $d0 $ef $f8 }T
$7295 >PC $16 >S $fb >A $88 >X $d4 >Y $e1 >P $7295 $d0 >M $7296 $65 >M $7297 $f2 >M $72fc $c9 >M
T{ op PC S A X Y P -> $72fc $16 $fb $88 $d4 $e1 }T T{ $7295 M $7296 M $7297 M $72fc M -> $d0 $65 $f2 $c9 }T
$5d54 >PC $fc >S $9e >A $67 >X $ee >Y $e6 >P $5d54 $d0 >M $5d55 $4f >M $5d56 $00 >M
T{ op PC S A X Y P -> $5d56 $fc $9e $67 $ee $e6 }T T{ $5d54 M $5d55 M $5d56 M -> $d0 $4f $00 }T
$92b2 >PC $10 >S $0c >A $3e >X $0e >Y $e1 >P $922f $92 >M $92b2 $d0 >M $92b3 $7b >M $92b4 $69 >M $932f $5e >M
T{ op PC S A X Y P -> $932f $10 $0c $3e $0e $e1 }T T{ $922f M $92b2 M $92b3 M $92b4 M $932f M -> $92 $d0 $7b $69 $5e }T
$e38f >PC $f4 >S $c3 >A $ac >X $2c >Y $e2 >P $e38f $d0 >M $e390 $55 >M $e391 $24 >M
T{ op PC S A X Y P -> $e391 $f4 $c3 $ac $2c $e2 }T T{ $e38f M $e390 M $e391 M -> $d0 $55 $24 }T
$e1dd >PC $10 >S $82 >A $b6 >X $0f >Y $60 >P $e1c8 $ad >M $e1dd $d0 >M $e1de $e9 >M $e1df $92 >M
T{ op PC S A X Y P -> $e1c8 $10 $82 $b6 $0f $60 }T T{ $e1c8 M $e1dd M $e1de M $e1df M -> $ad $d0 $e9 $92 }T
$7878 >PC $0e >S $49 >A $d8 >X $b7 >Y $64 >P $7878 $d0 >M $7879 $47 >M $787a $0b >M $78c1 $3d >M
T{ op PC S A X Y P -> $78c1 $0e $49 $d8 $b7 $64 }T T{ $7878 M $7879 M $787a M $78c1 M -> $d0 $47 $0b $3d }T
$9bf7 >PC $ed >S $4e >A $d8 >X $0e >Y $62 >P $9bf7 $d0 >M $9bf8 $64 >M $9bf9 $04 >M
T{ op PC S A X Y P -> $9bf9 $ed $4e $d8 $0e $62 }T T{ $9bf7 M $9bf8 M $9bf9 M -> $d0 $64 $04 }T
$08c1 >PC $41 >S $88 >A $11 >X $6e >Y $60 >P $08c1 $d0 >M $08c2 $04 >M $08c3 $e2 >M $08c7 $aa >M
T{ op PC S A X Y P -> $08c7 $41 $88 $11 $6e $60 }T T{ $08c1 M $08c2 M $08c3 M $08c7 M -> $d0 $04 $e2 $aa }T
$be19 >PC $62 >S $2a >A $5a >X $04 >Y $a0 >P $be19 $d0 >M $be1a $43 >M $be1b $66 >M $be5e $aa >M
T{ op PC S A X Y P -> $be5e $62 $2a $5a $04 $a0 }T T{ $be19 M $be1a M $be1b M $be5e M -> $d0 $43 $66 $aa }T
( d1 )
$367e >PC $12 >S $be >A $a8 >X $ac >Y $63 >P $00f8 $2d >M $00f9 $96 >M $367e $d1 >M $367f $f8 >M $3680 $12 >M $96d9 $c8 >M
T{ op PC S A X Y P -> $3680 $12 $be $a8 $ac $e0 }T T{ $00f8 M $00f9 M $367e M $367f M $3680 M $96d9 M -> $2d $96 $d1 $f8 $12 $c8 }T
$1459 >PC $fa >S $52 >A $dd >X $30 >Y $e7 >P $00f2 $b3 >M $00f3 $c5 >M $1459 $d1 >M $145a $f2 >M $145b $a8 >M $c5e3 $2d >M
T{ op PC S A X Y P -> $145b $fa $52 $dd $30 $65 }T T{ $00f2 M $00f3 M $1459 M $145a M $145b M $c5e3 M -> $b3 $c5 $d1 $f2 $a8 $2d }T
$9f20 >PC $64 >S $26 >A $68 >X $e0 >Y $64 >P $0057 $a3 >M $0058 $ec >M $9f20 $d1 >M $9f21 $57 >M $9f22 $8d >M $ed83 $21 >M
T{ op PC S A X Y P -> $9f22 $64 $26 $68 $e0 $65 }T T{ $0057 M $0058 M $9f20 M $9f21 M $9f22 M $ed83 M -> $a3 $ec $d1 $57 $8d $21 }T
$656a >PC $82 >S $6e >A $66 >X $7f >Y $a6 >P $009e $7b >M $009f $51 >M $51fa $67 >M $656a $d1 >M $656b $9e >M $656c $1d >M
T{ op PC S A X Y P -> $656c $82 $6e $66 $7f $25 }T T{ $009e M $009f M $51fa M $656a M $656b M $656c M -> $7b $51 $67 $d1 $9e $1d }T
$865c >PC $ab >S $b8 >A $e8 >X $8f >Y $e5 >P $00b8 $e6 >M $00b9 $eb >M $865c $d1 >M $865d $b8 >M $865e $9a >M $ec75 $ac >M
T{ op PC S A X Y P -> $865e $ab $b8 $e8 $8f $65 }T T{ $00b8 M $00b9 M $865c M $865d M $865e M $ec75 M -> $e6 $eb $d1 $b8 $9a $ac }T
$0ca4 >PC $c2 >S $ec >A $05 >X $4d >Y $24 >P $001f $77 >M $0020 $9e >M $0ca4 $d1 >M $0ca5 $1f >M $0ca6 $d9 >M $9ec4 $4f >M
T{ op PC S A X Y P -> $0ca6 $c2 $ec $05 $4d $a5 }T T{ $001f M $0020 M $0ca4 M $0ca5 M $0ca6 M $9ec4 M -> $77 $9e $d1 $1f $d9 $4f }T
$2cc2 >PC $08 >S $a9 >A $fe >X $68 >Y $a0 >P $00a8 $6a >M $00a9 $65 >M $2cc2 $d1 >M $2cc3 $a8 >M $2cc4 $a6 >M $65d2 $ff >M
T{ op PC S A X Y P -> $2cc4 $08 $a9 $fe $68 $a0 }T T{ $00a8 M $00a9 M $2cc2 M $2cc3 M $2cc4 M $65d2 M -> $6a $65 $d1 $a8 $a6 $ff }T
$3099 >PC $5e >S $c6 >A $f6 >X $df >Y $20 >P $003d $7c >M $003e $c1 >M $3099 $d1 >M $309a $3d >M $309b $7a >M $c25b $93 >M
T{ op PC S A X Y P -> $309b $5e $c6 $f6 $df $21 }T T{ $003d M $003e M $3099 M $309a M $309b M $c25b M -> $7c $c1 $d1 $3d $7a $93 }T
$e51c >PC $10 >S $85 >A $8b >X $96 >Y $e0 >P $00a2 $cd >M $00a3 $d5 >M $d663 $2e >M $e51c $d1 >M $e51d $a2 >M $e51e $33 >M
T{ op PC S A X Y P -> $e51e $10 $85 $8b $96 $61 }T T{ $00a2 M $00a3 M $d663 M $e51c M $e51d M $e51e M -> $cd $d5 $2e $d1 $a2 $33 }T
$f2d4 >PC $cb >S $19 >A $3e >X $1e >Y $60 >P $00f1 $17 >M $00f2 $1c >M $1c35 $30 >M $f2d4 $d1 >M $f2d5 $f1 >M $f2d6 $54 >M
T{ op PC S A X Y P -> $f2d6 $cb $19 $3e $1e $e0 }T T{ $00f1 M $00f2 M $1c35 M $f2d4 M $f2d5 M $f2d6 M -> $17 $1c $30 $d1 $f1 $54 }T
$e984 >PC $73 >S $7e >A $47 >X $9e >Y $a4 >P $0019 $17 >M $001a $ef >M $e984 $d1 >M $e985 $19 >M $e986 $b7 >M $efb5 $eb >M
T{ op PC S A X Y P -> $e986 $73 $7e $47 $9e $a4 }T T{ $0019 M $001a M $e984 M $e985 M $e986 M $efb5 M -> $17 $ef $d1 $19 $b7 $eb }T
$8ace >PC $0d >S $f5 >A $93 >X $ec >Y $a5 >P $0016 $c2 >M $0017 $e5 >M $8ace $d1 >M $8acf $16 >M $8ad0 $10 >M $e6ae $3f >M
T{ op PC S A X Y P -> $8ad0 $0d $f5 $93 $ec $a5 }T T{ $0016 M $0017 M $8ace M $8acf M $8ad0 M $e6ae M -> $c2 $e5 $d1 $16 $10 $3f }T
$8dc9 >PC $f1 >S $17 >A $18 >X $57 >Y $e1 >P $00be $08 >M $00bf $26 >M $265f $0f >M $8dc9 $d1 >M $8dca $be >M $8dcb $94 >M
T{ op PC S A X Y P -> $8dcb $f1 $17 $18 $57 $61 }T T{ $00be M $00bf M $265f M $8dc9 M $8dca M $8dcb M -> $08 $26 $0f $d1 $be $94 }T
$ea71 >PC $55 >S $e8 >A $9d >X $76 >Y $e0 >P $0018 $53 >M $0019 $9c >M $9cc9 $b3 >M $ea71 $d1 >M $ea72 $18 >M $ea73 $78 >M
T{ op PC S A X Y P -> $ea73 $55 $e8 $9d $76 $61 }T T{ $0018 M $0019 M $9cc9 M $ea71 M $ea72 M $ea73 M -> $53 $9c $b3 $d1 $18 $78 }T
$8d4c >PC $89 >S $e5 >A $3f >X $bb >Y $20 >P $003d $73 >M $003e $6f >M $702e $24 >M $8d4c $d1 >M $8d4d $3d >M $8d4e $40 >M
T{ op PC S A X Y P -> $8d4e $89 $e5 $3f $bb $a1 }T T{ $003d M $003e M $702e M $8d4c M $8d4d M $8d4e M -> $73 $6f $24 $d1 $3d $40 }T
$b764 >PC $a4 >S $6b >A $c4 >X $4b >Y $23 >P $0092 $c6 >M $0093 $18 >M $1911 $6b >M $b764 $d1 >M $b765 $92 >M $b766 $de >M
T{ op PC S A X Y P -> $b766 $a4 $6b $c4 $4b $23 }T T{ $0092 M $0093 M $1911 M $b764 M $b765 M $b766 M -> $c6 $18 $6b $d1 $92 $de }T
( d2 )
$0057 >PC $c2 >S $5b >A $5a >X $66 >Y $22 >P $0057 $d2 >M $0058 $6d >M $0059 $34 >M $006d $d2 >M $006e $58 >M $58d2 $7e >M
T{ op PC S A X Y P -> $0059 $c2 $5b $5a $66 $a0 }T T{ $0057 M $0058 M $0059 M $006d M $006e M $58d2 M -> $d2 $6d $34 $d2 $58 $7e }T
$12cc >PC $2d >S $da >A $5b >X $ae >Y $e7 >P $00f5 $82 >M $00f6 $90 >M $12cc $d2 >M $12cd $f5 >M $12ce $a0 >M $9082 $8f >M
T{ op PC S A X Y P -> $12ce $2d $da $5b $ae $65 }T T{ $00f5 M $00f6 M $12cc M $12cd M $12ce M $9082 M -> $82 $90 $d2 $f5 $a0 $8f }T
$0e84 >PC $56 >S $02 >A $c2 >X $4c >Y $a5 >P $00c0 $6e >M $00c1 $f0 >M $0e84 $d2 >M $0e85 $c0 >M $0e86 $74 >M $f06e $35 >M
T{ op PC S A X Y P -> $0e86 $56 $02 $c2 $4c $a4 }T T{ $00c0 M $00c1 M $0e84 M $0e85 M $0e86 M $f06e M -> $6e $f0 $d2 $c0 $74 $35 }T
$ea2c >PC $32 >S $bb >A $11 >X $e1 >Y $60 >P $005c $aa >M $005d $11 >M $11aa $94 >M $ea2c $d2 >M $ea2d $5c >M $ea2e $c6 >M
T{ op PC S A X Y P -> $ea2e $32 $bb $11 $e1 $61 }T T{ $005c M $005d M $11aa M $ea2c M $ea2d M $ea2e M -> $aa $11 $94 $d2 $5c $c6 }T
$d7d8 >PC $a2 >S $a3 >A $ef >X $08 >Y $e2 >P $00ea $0b >M $00eb $ef >M $d7d8 $d2 >M $d7d9 $ea >M $d7da $2d >M $ef0b $6b >M
T{ op PC S A X Y P -> $d7da $a2 $a3 $ef $08 $61 }T T{ $00ea M $00eb M $d7d8 M $d7d9 M $d7da M $ef0b M -> $0b $ef $d2 $ea $2d $6b }T
$3ac1 >PC $e4 >S $cd >A $61 >X $f4 >Y $66 >P $0001 $82 >M $0002 $a7 >M $3ac1 $d2 >M $3ac2 $01 >M $3ac3 $da >M $a782 $93 >M
T{ op PC S A X Y P -> $3ac3 $e4 $cd $61 $f4 $65 }T T{ $0001 M $0002 M $3ac1 M $3ac2 M $3ac3 M $a782 M -> $82 $a7 $d2 $01 $da $93 }T
$8347 >PC $bc >S $55 >A $27 >X $d0 >Y $a6 >P $00dc $a0 >M $00dd $53 >M $53a0 $f5 >M $8347 $d2 >M $8348 $dc >M $8349 $aa >M
T{ op PC S A X Y P -> $8349 $bc $55 $27 $d0 $24 }T T{ $00dc M $00dd M $53a0 M $8347 M $8348 M $8349 M -> $a0 $53 $f5 $d2 $dc $aa }T
$af14 >PC $97 >S $27 >A $b6 >X $22 >Y $a1 >P $00d3 $c9 >M $00d4 $62 >M $62c9 $06 >M $af14 $d2 >M $af15 $d3 >M $af16 $f6 >M
T{ op PC S A X Y P -> $af16 $97 $27 $b6 $22 $21 }T T{ $00d3 M $00d4 M $62c9 M $af14 M $af15 M $af16 M -> $c9 $62 $06 $d2 $d3 $f6 }T
$ef90 >PC $76 >S $28 >A $f5 >X $ef >Y $a4 >P $0026 $38 >M $0027 $21 >M $2138 $69 >M $ef90 $d2 >M $ef91 $26 >M $ef92 $b8 >M
T{ op PC S A X Y P -> $ef92 $76 $28 $f5 $ef $a4 }T T{ $0026 M $0027 M $2138 M $ef90 M $ef91 M $ef92 M -> $38 $21 $69 $d2 $26 $b8 }T
$0b19 >PC $48 >S $a9 >A $0e >X $10 >Y $e4 >P $0096 $f5 >M $0097 $43 >M $0b19 $d2 >M $0b1a $96 >M $0b1b $f3 >M $43f5 $ea >M
T{ op PC S A X Y P -> $0b1b $48 $a9 $0e $10 $e4 }T T{ $0096 M $0097 M $0b19 M $0b1a M $0b1b M $43f5 M -> $f5 $43 $d2 $96 $f3 $ea }T
$e894 >PC $95 >S $c9 >A $c0 >X $ec >Y $a2 >P $002a $c9 >M $002b $5f >M $5fc9 $31 >M $e894 $d2 >M $e895 $2a >M $e896 $9e >M
T{ op PC S A X Y P -> $e896 $95 $c9 $c0 $ec $a1 }T T{ $002a M $002b M $5fc9 M $e894 M $e895 M $e896 M -> $c9 $5f $31 $d2 $2a $9e }T
$2ed7 >PC $d4 >S $dd >A $96 >X $d8 >Y $24 >P $00d2 $db >M $00d3 $39 >M $2ed7 $d2 >M $2ed8 $d2 >M $2ed9 $1d >M $39db $52 >M
T{ op PC S A X Y P -> $2ed9 $d4 $dd $96 $d8 $a5 }T T{ $00d2 M $00d3 M $2ed7 M $2ed8 M $2ed9 M $39db M -> $db $39 $d2 $d2 $1d $52 }T
$501e >PC $ae >S $0a >A $78 >X $45 >Y $e6 >P $005c $c8 >M $005d $09 >M $09c8 $44 >M $501e $d2 >M $501f $5c >M $5020 $c1 >M
T{ op PC S A X Y P -> $5020 $ae $0a $78 $45 $e4 }T T{ $005c M $005d M $09c8 M $501e M $501f M $5020 M -> $c8 $09 $44 $d2 $5c $c1 }T
$37d7 >PC $06 >S $47 >A $b7 >X $3c >Y $a1 >P $0013 $33 >M $0014 $05 >M $0533 $77 >M $37d7 $d2 >M $37d8 $13 >M $37d9 $4f >M
T{ op PC S A X Y P -> $37d9 $06 $47 $b7 $3c $a0 }T T{ $0013 M $0014 M $0533 M $37d7 M $37d8 M $37d9 M -> $33 $05 $77 $d2 $13 $4f }T
$b9aa >PC $ba >S $55 >A $b5 >X $5e >Y $24 >P $00d6 $0f >M $00d7 $41 >M $410f $b9 >M $b9aa $d2 >M $b9ab $d6 >M $b9ac $d5 >M
T{ op PC S A X Y P -> $b9ac $ba $55 $b5 $5e $a4 }T T{ $00d6 M $00d7 M $410f M $b9aa M $b9ab M $b9ac M -> $0f $41 $b9 $d2 $d6 $d5 }T
$ad50 >PC $5c >S $38 >A $a8 >X $65 >Y $e3 >P $00c8 $27 >M $00c9 $90 >M $9027 $20 >M $ad50 $d2 >M $ad51 $c8 >M $ad52 $ac >M
T{ op PC S A X Y P -> $ad52 $5c $38 $a8 $65 $61 }T T{ $00c8 M $00c9 M $9027 M $ad50 M $ad51 M $ad52 M -> $27 $90 $20 $d2 $c8 $ac }T
( d3 )
$a7ac >PC $3c >S $0d >A $ac >X $91 >Y $64 >P $a7ac $d3 >M $a7ad $79 >M $a7ae $92 >M
T{ op PC S A X Y P -> $a7ad $3c $0d $ac $91 $64 }T T{ $a7ac M $a7ad M $a7ae M -> $d3 $79 $92 }T
$10e2 >PC $88 >S $0c >A $6e >X $4b >Y $64 >P $10e2 $d3 >M $10e3 $d0 >M $10e4 $10 >M
T{ op PC S A X Y P -> $10e3 $88 $0c $6e $4b $64 }T T{ $10e2 M $10e3 M $10e4 M -> $d3 $d0 $10 }T
$38cb >PC $94 >S $32 >A $58 >X $1a >Y $67 >P $38cb $d3 >M $38cc $71 >M $38cd $a6 >M
T{ op PC S A X Y P -> $38cc $94 $32 $58 $1a $67 }T T{ $38cb M $38cc M $38cd M -> $d3 $71 $a6 }T
$c284 >PC $17 >S $4e >A $a0 >X $29 >Y $65 >P $c284 $d3 >M $c285 $d7 >M $c286 $58 >M
T{ op PC S A X Y P -> $c285 $17 $4e $a0 $29 $65 }T T{ $c284 M $c285 M $c286 M -> $d3 $d7 $58 }T
$c02a >PC $6c >S $0a >A $94 >X $b0 >Y $60 >P $c02a $d3 >M $c02b $c7 >M $c02c $22 >M
T{ op PC S A X Y P -> $c02b $6c $0a $94 $b0 $60 }T T{ $c02a M $c02b M $c02c M -> $d3 $c7 $22 }T
$efac >PC $76 >S $46 >A $0f >X $f1 >Y $63 >P $efac $d3 >M $efad $2b >M $efae $88 >M
T{ op PC S A X Y P -> $efad $76 $46 $0f $f1 $63 }T T{ $efac M $efad M $efae M -> $d3 $2b $88 }T
$2a0d >PC $57 >S $44 >A $59 >X $d1 >Y $a3 >P $2a0d $d3 >M $2a0e $78 >M $2a0f $c5 >M
T{ op PC S A X Y P -> $2a0e $57 $44 $59 $d1 $a3 }T T{ $2a0d M $2a0e M $2a0f M -> $d3 $78 $c5 }T
$8631 >PC $ae >S $18 >A $dc >X $5c >Y $a7 >P $8631 $d3 >M $8632 $7c >M $8633 $31 >M
T{ op PC S A X Y P -> $8632 $ae $18 $dc $5c $a7 }T T{ $8631 M $8632 M $8633 M -> $d3 $7c $31 }T
$55fb >PC $7f >S $06 >A $0f >X $14 >Y $a4 >P $55fb $d3 >M $55fc $46 >M $55fd $e3 >M
T{ op PC S A X Y P -> $55fc $7f $06 $0f $14 $a4 }T T{ $55fb M $55fc M $55fd M -> $d3 $46 $e3 }T
$5429 >PC $b5 >S $46 >A $2b >X $90 >Y $e0 >P $5429 $d3 >M $542a $e0 >M $542b $b6 >M
T{ op PC S A X Y P -> $542a $b5 $46 $2b $90 $e0 }T T{ $5429 M $542a M $542b M -> $d3 $e0 $b6 }T
$6e77 >PC $c2 >S $61 >A $03 >X $f7 >Y $24 >P $6e77 $d3 >M $6e78 $b8 >M $6e79 $ed >M
T{ op PC S A X Y P -> $6e78 $c2 $61 $03 $f7 $24 }T T{ $6e77 M $6e78 M $6e79 M -> $d3 $b8 $ed }T
$4616 >PC $72 >S $45 >A $4c >X $8d >Y $26 >P $4616 $d3 >M $4617 $d2 >M $4618 $c9 >M
T{ op PC S A X Y P -> $4617 $72 $45 $4c $8d $26 }T T{ $4616 M $4617 M $4618 M -> $d3 $d2 $c9 }T
$4714 >PC $6c >S $2f >A $11 >X $da >Y $e5 >P $4714 $d3 >M $4715 $56 >M $4716 $6b >M
T{ op PC S A X Y P -> $4715 $6c $2f $11 $da $e5 }T T{ $4714 M $4715 M $4716 M -> $d3 $56 $6b }T
$3255 >PC $fe >S $5f >A $c4 >X $66 >Y $63 >P $3255 $d3 >M $3256 $3a >M $3257 $42 >M
T{ op PC S A X Y P -> $3256 $fe $5f $c4 $66 $63 }T T{ $3255 M $3256 M $3257 M -> $d3 $3a $42 }T
$0106 >PC $cf >S $09 >A $4e >X $df >Y $e2 >P $0106 $d3 >M $0107 $7b >M $0108 $1c >M
T{ op PC S A X Y P -> $0107 $cf $09 $4e $df $e2 }T T{ $0106 M $0107 M $0108 M -> $d3 $7b $1c }T
$e702 >PC $d3 >S $14 >A $5c >X $d2 >Y $67 >P $e702 $d3 >M $e703 $4e >M $e704 $72 >M
T{ op PC S A X Y P -> $e703 $d3 $14 $5c $d2 $67 }T T{ $e702 M $e703 M $e704 M -> $d3 $4e $72 }T
( d4 )
$18ee >PC $ab >S $2d >A $48 >X $54 >Y $e3 >P $00a5 $6b >M $00ed $ff >M $18ee $d4 >M $18ef $a5 >M $18f0 $32 >M
T{ op PC S A X Y P -> $18f0 $ab $2d $48 $54 $e3 }T T{ $00a5 M $00ed M $18ee M $18ef M $18f0 M -> $6b $ff $d4 $a5 $32 }T
$6bf7 >PC $10 >S $70 >A $eb >X $13 >Y $e5 >P $006e $35 >M $0083 $ca >M $6bf7 $d4 >M $6bf8 $83 >M $6bf9 $39 >M
T{ op PC S A X Y P -> $6bf9 $10 $70 $eb $13 $e5 }T T{ $006e M $0083 M $6bf7 M $6bf8 M $6bf9 M -> $35 $ca $d4 $83 $39 }T
$c630 >PC $60 >S $ed >A $30 >X $ef >Y $a7 >P $0026 $72 >M $0056 $94 >M $c630 $d4 >M $c631 $26 >M $c632 $f2 >M
T{ op PC S A X Y P -> $c632 $60 $ed $30 $ef $a7 }T T{ $0026 M $0056 M $c630 M $c631 M $c632 M -> $72 $94 $d4 $26 $f2 }T
$3ca2 >PC $0d >S $5d >A $22 >X $94 >Y $61 >P $0001 $8a >M $00df $c8 >M $3ca2 $d4 >M $3ca3 $df >M $3ca4 $cd >M
T{ op PC S A X Y P -> $3ca4 $0d $5d $22 $94 $61 }T T{ $0001 M $00df M $3ca2 M $3ca3 M $3ca4 M -> $8a $c8 $d4 $df $cd }T
$2232 >PC $11 >S $38 >A $fd >X $1d >Y $27 >P $00d7 $f3 >M $00da $18 >M $2232 $d4 >M $2233 $da >M $2234 $ec >M
T{ op PC S A X Y P -> $2234 $11 $38 $fd $1d $27 }T T{ $00d7 M $00da M $2232 M $2233 M $2234 M -> $f3 $18 $d4 $da $ec }T
$84d1 >PC $b1 >S $38 >A $7a >X $39 >Y $a7 >P $0076 $3d >M $00f0 $ff >M $84d1 $d4 >M $84d2 $76 >M $84d3 $aa >M
T{ op PC S A X Y P -> $84d3 $b1 $38 $7a $39 $a7 }T T{ $0076 M $00f0 M $84d1 M $84d2 M $84d3 M -> $3d $ff $d4 $76 $aa }T
$b0ed >PC $91 >S $df >A $50 >X $5f >Y $a0 >P $000a $4e >M $00ba $da >M $b0ed $d4 >M $b0ee $ba >M $b0ef $ce >M
T{ op PC S A X Y P -> $b0ef $91 $df $50 $5f $a0 }T T{ $000a M $00ba M $b0ed M $b0ee M $b0ef M -> $4e $da $d4 $ba $ce }T
$6a32 >PC $1a >S $42 >A $1f >X $49 >Y $27 >P $00da $cf >M $00f9 $88 >M $6a32 $d4 >M $6a33 $da >M $6a34 $20 >M
T{ op PC S A X Y P -> $6a34 $1a $42 $1f $49 $27 }T T{ $00da M $00f9 M $6a32 M $6a33 M $6a34 M -> $cf $88 $d4 $da $20 }T
$1eba >PC $07 >S $0b >A $16 >X $c8 >Y $e5 >P $003e $80 >M $0054 $43 >M $1eba $d4 >M $1ebb $3e >M $1ebc $f5 >M
T{ op PC S A X Y P -> $1ebc $07 $0b $16 $c8 $e5 }T T{ $003e M $0054 M $1eba M $1ebb M $1ebc M -> $80 $43 $d4 $3e $f5 }T
$9821 >PC $d6 >S $f2 >A $da >X $06 >Y $26 >P $00a9 $25 >M $00cf $42 >M $9821 $d4 >M $9822 $cf >M $9823 $62 >M
T{ op PC S A X Y P -> $9823 $d6 $f2 $da $06 $26 }T T{ $00a9 M $00cf M $9821 M $9822 M $9823 M -> $25 $42 $d4 $cf $62 }T
$fea4 >PC $e2 >S $f8 >A $b8 >X $03 >Y $a3 >P $0063 $2d >M $00ab $83 >M $fea4 $d4 >M $fea5 $ab >M $fea6 $46 >M
T{ op PC S A X Y P -> $fea6 $e2 $f8 $b8 $03 $a3 }T T{ $0063 M $00ab M $fea4 M $fea5 M $fea6 M -> $2d $83 $d4 $ab $46 }T
$fbfc >PC $f0 >S $67 >A $05 >X $a5 >Y $24 >P $0042 $0c >M $0047 $57 >M $fbfc $d4 >M $fbfd $42 >M $fbfe $76 >M
T{ op PC S A X Y P -> $fbfe $f0 $67 $05 $a5 $24 }T T{ $0042 M $0047 M $fbfc M $fbfd M $fbfe M -> $0c $57 $d4 $42 $76 }T
$3a24 >PC $94 >S $26 >A $fa >X $9b >Y $a1 >P $00e3 $fa >M $00e9 $14 >M $3a24 $d4 >M $3a25 $e9 >M $3a26 $f4 >M
T{ op PC S A X Y P -> $3a26 $94 $26 $fa $9b $a1 }T T{ $00e3 M $00e9 M $3a24 M $3a25 M $3a26 M -> $fa $14 $d4 $e9 $f4 }T
$5a46 >PC $96 >S $e2 >A $e0 >X $80 >Y $e3 >P $00bf $2b >M $00df $cb >M $5a46 $d4 >M $5a47 $df >M $5a48 $0a >M
T{ op PC S A X Y P -> $5a48 $96 $e2 $e0 $80 $e3 }T T{ $00bf M $00df M $5a46 M $5a47 M $5a48 M -> $2b $cb $d4 $df $0a }T
$eac9 >PC $1b >S $45 >A $5e >X $b1 >Y $e0 >P $0015 $a6 >M $0073 $a7 >M $eac9 $d4 >M $eaca $15 >M $eacb $47 >M
T{ op PC S A X Y P -> $eacb $1b $45 $5e $b1 $e0 }T T{ $0015 M $0073 M $eac9 M $eaca M $eacb M -> $a6 $a7 $d4 $15 $47 }T
$512d >PC $e9 >S $27 >A $17 >X $34 >Y $60 >P $00b7 $c0 >M $00ce $89 >M $512d $d4 >M $512e $b7 >M $512f $00 >M
T{ op PC S A X Y P -> $512f $e9 $27 $17 $34 $60 }T T{ $00b7 M $00ce M $512d M $512e M $512f M -> $c0 $89 $d4 $b7 $00 }T
( d5 )
$1e4f >PC $0c >S $d1 >A $74 >X $b4 >Y $21 >P $0065 $37 >M $00d9 $c3 >M $1e4f $d5 >M $1e50 $65 >M $1e51 $96 >M
T{ op PC S A X Y P -> $1e51 $0c $d1 $74 $b4 $21 }T T{ $0065 M $00d9 M $1e4f M $1e50 M $1e51 M -> $37 $c3 $d5 $65 $96 }T
$1a8e >PC $ba >S $af >A $4c >X $e5 >Y $65 >P $0038 $24 >M $00ec $e3 >M $1a8e $d5 >M $1a8f $ec >M $1a90 $44 >M
T{ op PC S A X Y P -> $1a90 $ba $af $4c $e5 $e5 }T T{ $0038 M $00ec M $1a8e M $1a8f M $1a90 M -> $24 $e3 $d5 $ec $44 }T
$4f63 >PC $02 >S $7e >A $b8 >X $bf >Y $60 >P $0006 $1d >M $004e $a7 >M $4f63 $d5 >M $4f64 $4e >M $4f65 $42 >M
T{ op PC S A X Y P -> $4f65 $02 $7e $b8 $bf $61 }T T{ $0006 M $004e M $4f63 M $4f64 M $4f65 M -> $1d $a7 $d5 $4e $42 }T
$2c86 >PC $8c >S $c1 >A $99 >X $a6 >Y $e7 >P $006d $1c >M $00d4 $8a >M $2c86 $d5 >M $2c87 $d4 >M $2c88 $62 >M
T{ op PC S A X Y P -> $2c88 $8c $c1 $99 $a6 $e5 }T T{ $006d M $00d4 M $2c86 M $2c87 M $2c88 M -> $1c $8a $d5 $d4 $62 }T
$6818 >PC $a6 >S $60 >A $e4 >X $43 >Y $e1 >P $0034 $cb >M $0050 $0b >M $6818 $d5 >M $6819 $50 >M $681a $29 >M
T{ op PC S A X Y P -> $681a $a6 $60 $e4 $43 $e0 }T T{ $0034 M $0050 M $6818 M $6819 M $681a M -> $cb $0b $d5 $50 $29 }T
$7033 >PC $4e >S $ac >A $03 >X $5b >Y $23 >P $002d $99 >M $0030 $50 >M $7033 $d5 >M $7034 $2d >M $7035 $a4 >M
T{ op PC S A X Y P -> $7035 $4e $ac $03 $5b $21 }T T{ $002d M $0030 M $7033 M $7034 M $7035 M -> $99 $50 $d5 $2d $a4 }T
$6dd5 >PC $f0 >S $9e >A $28 >X $ab >Y $27 >P $005a $81 >M $0082 $c3 >M $6dd5 $d5 >M $6dd6 $5a >M $6dd7 $8b >M
T{ op PC S A X Y P -> $6dd7 $f0 $9e $28 $ab $a4 }T T{ $005a M $0082 M $6dd5 M $6dd6 M $6dd7 M -> $81 $c3 $d5 $5a $8b }T
$a4db >PC $04 >S $86 >A $99 >X $77 >Y $e5 >P $002e $e9 >M $0095 $07 >M $a4db $d5 >M $a4dc $95 >M $a4dd $f1 >M
T{ op PC S A X Y P -> $a4dd $04 $86 $99 $77 $e4 }T T{ $002e M $0095 M $a4db M $a4dc M $a4dd M -> $e9 $07 $d5 $95 $f1 }T
$1449 >PC $c7 >S $19 >A $e9 >X $58 >Y $e1 >P $0002 $97 >M $0019 $60 >M $1449 $d5 >M $144a $19 >M $144b $3f >M
T{ op PC S A X Y P -> $144b $c7 $19 $e9 $58 $e0 }T T{ $0002 M $0019 M $1449 M $144a M $144b M -> $97 $60 $d5 $19 $3f }T
$740e >PC $42 >S $e5 >A $a3 >X $7c >Y $a7 >P $0028 $86 >M $00cb $b7 >M $740e $d5 >M $740f $28 >M $7410 $bb >M
T{ op PC S A X Y P -> $7410 $42 $e5 $a3 $7c $25 }T T{ $0028 M $00cb M $740e M $740f M $7410 M -> $86 $b7 $d5 $28 $bb }T
$6797 >PC $0a >S $74 >A $8c >X $05 >Y $61 >P $003d $91 >M $00c9 $29 >M $6797 $d5 >M $6798 $3d >M $6799 $09 >M
T{ op PC S A X Y P -> $6799 $0a $74 $8c $05 $61 }T T{ $003d M $00c9 M $6797 M $6798 M $6799 M -> $91 $29 $d5 $3d $09 }T
$0d30 >PC $03 >S $c2 >A $78 >X $d2 >Y $a4 >P $0060 $72 >M $00d8 $20 >M $0d30 $d5 >M $0d31 $60 >M $0d32 $01 >M
T{ op PC S A X Y P -> $0d32 $03 $c2 $78 $d2 $a5 }T T{ $0060 M $00d8 M $0d30 M $0d31 M $0d32 M -> $72 $20 $d5 $60 $01 }T
$475d >PC $8f >S $53 >A $e1 >X $ac >Y $60 >P $0032 $b9 >M $0051 $69 >M $475d $d5 >M $475e $51 >M $475f $25 >M
T{ op PC S A X Y P -> $475f $8f $53 $e1 $ac $e0 }T T{ $0032 M $0051 M $475d M $475e M $475f M -> $b9 $69 $d5 $51 $25 }T
$3238 >PC $f7 >S $44 >A $fb >X $46 >Y $62 >P $00bb $1c >M $00c0 $6a >M $3238 $d5 >M $3239 $c0 >M $323a $26 >M
T{ op PC S A X Y P -> $323a $f7 $44 $fb $46 $61 }T T{ $00bb M $00c0 M $3238 M $3239 M $323a M -> $1c $6a $d5 $c0 $26 }T
$e477 >PC $70 >S $e9 >A $29 >X $b7 >Y $a0 >P $0065 $85 >M $008e $46 >M $e477 $d5 >M $e478 $65 >M $e479 $74 >M
T{ op PC S A X Y P -> $e479 $70 $e9 $29 $b7 $a1 }T T{ $0065 M $008e M $e477 M $e478 M $e479 M -> $85 $46 $d5 $65 $74 }T
$a771 >PC $49 >S $1a >A $ec >X $3a >Y $e7 >P $0044 $8c >M $0058 $7f >M $a771 $d5 >M $a772 $58 >M $a773 $03 >M
T{ op PC S A X Y P -> $a773 $49 $1a $ec $3a $e4 }T T{ $0044 M $0058 M $a771 M $a772 M $a773 M -> $8c $7f $d5 $58 $03 }T
( d6 )
$630a >PC $73 >S $d2 >A $f0 >X $75 >Y $61 >P $00b7 $e0 >M $00c7 $cf >M $630a $d6 >M $630b $c7 >M $630c $b2 >M
T{ op PC S A X Y P -> $630c $73 $d2 $f0 $75 $e1 }T T{ $00b7 M $00c7 M $630a M $630b M $630c M -> $df $cf $d6 $c7 $b2 }T
$4d73 >PC $f7 >S $d0 >A $e0 >X $76 >Y $63 >P $009b $d2 >M $00bb $d0 >M $4d73 $d6 >M $4d74 $bb >M $4d75 $18 >M
T{ op PC S A X Y P -> $4d75 $f7 $d0 $e0 $76 $e1 }T T{ $009b M $00bb M $4d73 M $4d74 M $4d75 M -> $d1 $d0 $d6 $bb $18 }T
$b1ca >PC $80 >S $3e >A $19 >X $c9 >Y $e2 >P $0011 $43 >M $002a $17 >M $b1ca $d6 >M $b1cb $11 >M $b1cc $f2 >M
T{ op PC S A X Y P -> $b1cc $80 $3e $19 $c9 $60 }T T{ $0011 M $002a M $b1ca M $b1cb M $b1cc M -> $43 $16 $d6 $11 $f2 }T
$2978 >PC $30 >S $6b >A $37 >X $b9 >Y $26 >P $007b $de >M $00b2 $df >M $2978 $d6 >M $2979 $7b >M $297a $db >M
T{ op PC S A X Y P -> $297a $30 $6b $37 $b9 $a4 }T T{ $007b M $00b2 M $2978 M $2979 M $297a M -> $de $de $d6 $7b $db }T
$65bf >PC $7a >S $cd >A $89 >X $d8 >Y $65 >P $0040 $29 >M $00c9 $78 >M $65bf $d6 >M $65c0 $40 >M $65c1 $65 >M
T{ op PC S A X Y P -> $65c1 $7a $cd $89 $d8 $65 }T T{ $0040 M $00c9 M $65bf M $65c0 M $65c1 M -> $29 $77 $d6 $40 $65 }T
$c4c5 >PC $8f >S $d5 >A $a0 >X $46 >Y $a2 >P $0001 $b1 >M $00a1 $98 >M $c4c5 $d6 >M $c4c6 $01 >M $c4c7 $68 >M
T{ op PC S A X Y P -> $c4c7 $8f $d5 $a0 $46 $a0 }T T{ $0001 M $00a1 M $c4c5 M $c4c6 M $c4c7 M -> $b1 $97 $d6 $01 $68 }T
$b433 >PC $84 >S $94 >A $3a >X $9c >Y $62 >P $0042 $81 >M $007c $6b >M $b433 $d6 >M $b434 $42 >M $b435 $bf >M
T{ op PC S A X Y P -> $b435 $84 $94 $3a $9c $60 }T T{ $0042 M $007c M $b433 M $b434 M $b435 M -> $81 $6a $d6 $42 $bf }T
$7151 >PC $49 >S $06 >A $8c >X $ba >Y $a5 >P $0018 $4d >M $008c $27 >M $7151 $d6 >M $7152 $8c >M $7153 $5a >M
T{ op PC S A X Y P -> $7153 $49 $06 $8c $ba $25 }T T{ $0018 M $008c M $7151 M $7152 M $7153 M -> $4c $27 $d6 $8c $5a }T
$569b >PC $0b >S $bd >A $5a >X $66 >Y $e1 >P $0043 $ed >M $00e9 $c8 >M $569b $d6 >M $569c $e9 >M $569d $b5 >M
T{ op PC S A X Y P -> $569d $0b $bd $5a $66 $e1 }T T{ $0043 M $00e9 M $569b M $569c M $569d M -> $ec $c8 $d6 $e9 $b5 }T
$0fed >PC $dc >S $9c >A $d3 >X $2d >Y $a4 >P $004c $67 >M $0079 $65 >M $0fed $d6 >M $0fee $79 >M $0fef $3b >M
T{ op PC S A X Y P -> $0fef $dc $9c $d3 $2d $24 }T T{ $004c M $0079 M $0fed M $0fee M $0fef M -> $66 $65 $d6 $79 $3b }T
$0fe0 >PC $52 >S $e2 >A $a3 >X $61 >Y $e4 >P $0025 $93 >M $00c8 $19 >M $0fe0 $d6 >M $0fe1 $25 >M $0fe2 $6a >M
T{ op PC S A X Y P -> $0fe2 $52 $e2 $a3 $61 $64 }T T{ $0025 M $00c8 M $0fe0 M $0fe1 M $0fe2 M -> $93 $18 $d6 $25 $6a }T
$c4e5 >PC $0d >S $56 >A $7f >X $8b >Y $66 >P $005f $52 >M $00de $e4 >M $c4e5 $d6 >M $c4e6 $5f >M $c4e7 $2b >M
T{ op PC S A X Y P -> $c4e7 $0d $56 $7f $8b $e4 }T T{ $005f M $00de M $c4e5 M $c4e6 M $c4e7 M -> $52 $e3 $d6 $5f $2b }T
$e60a >PC $31 >S $e4 >A $3c >X $16 >Y $64 >P $0023 $fb >M $00e7 $07 >M $e60a $d6 >M $e60b $e7 >M $e60c $2e >M
T{ op PC S A X Y P -> $e60c $31 $e4 $3c $16 $e4 }T T{ $0023 M $00e7 M $e60a M $e60b M $e60c M -> $fa $07 $d6 $e7 $2e }T
$fa33 >PC $90 >S $7b >A $82 >X $79 >Y $22 >P $0043 $e2 >M $00c5 $3e >M $fa33 $d6 >M $fa34 $43 >M $fa35 $19 >M
T{ op PC S A X Y P -> $fa35 $90 $7b $82 $79 $20 }T T{ $0043 M $00c5 M $fa33 M $fa34 M $fa35 M -> $e2 $3d $d6 $43 $19 }T
$d53e >PC $b4 >S $f9 >A $21 >X $85 >Y $a1 >P $0093 $f6 >M $00b4 $58 >M $d53e $d6 >M $d53f $93 >M $d540 $f2 >M
T{ op PC S A X Y P -> $d540 $b4 $f9 $21 $85 $21 }T T{ $0093 M $00b4 M $d53e M $d53f M $d540 M -> $f6 $57 $d6 $93 $f2 }T
$ef02 >PC $75 >S $d3 >A $46 >X $28 >Y $66 >P $0063 $51 >M $00a9 $0f >M $ef02 $d6 >M $ef03 $63 >M $ef04 $92 >M
T{ op PC S A X Y P -> $ef04 $75 $d3 $46 $28 $64 }T T{ $0063 M $00a9 M $ef02 M $ef03 M $ef04 M -> $51 $0e $d6 $63 $92 }T
( d7 )
$e270 >PC $14 >S $f1 >A $73 >X $7a >Y $a5 >P $00ef $84 >M $e270 $d7 >M $e271 $ef >M $e272 $03 >M
T{ op PC S A X Y P -> $e272 $14 $f1 $73 $7a $a5 }T T{ $00ef M $e270 M $e271 M $e272 M -> $a4 $d7 $ef $03 }T
$ca52 >PC $47 >S $59 >A $80 >X $6f >Y $a7 >P $008d $ae >M $ca52 $d7 >M $ca53 $8d >M $ca54 $aa >M
T{ op PC S A X Y P -> $ca54 $47 $59 $80 $6f $a7 }T T{ $008d M $ca52 M $ca53 M $ca54 M -> $ae $d7 $8d $aa }T
$b490 >PC $a5 >S $8f >A $77 >X $70 >Y $22 >P $0092 $49 >M $b490 $d7 >M $b491 $92 >M $b492 $b9 >M
T{ op PC S A X Y P -> $b492 $a5 $8f $77 $70 $22 }T T{ $0092 M $b490 M $b491 M $b492 M -> $69 $d7 $92 $b9 }T
$c31e >PC $ca >S $9f >A $e9 >X $c3 >Y $62 >P $00a0 $b0 >M $c31e $d7 >M $c31f $a0 >M $c320 $fc >M
T{ op PC S A X Y P -> $c320 $ca $9f $e9 $c3 $62 }T T{ $00a0 M $c31e M $c31f M $c320 M -> $b0 $d7 $a0 $fc }T
$e081 >PC $16 >S $5a >A $4a >X $ab >Y $66 >P $0015 $48 >M $e081 $d7 >M $e082 $15 >M $e083 $e7 >M
T{ op PC S A X Y P -> $e083 $16 $5a $4a $ab $66 }T T{ $0015 M $e081 M $e082 M $e083 M -> $68 $d7 $15 $e7 }T
$62a5 >PC $90 >S $a9 >A $e6 >X $a0 >Y $66 >P $00ec $11 >M $62a5 $d7 >M $62a6 $ec >M $62a7 $72 >M
T{ op PC S A X Y P -> $62a7 $90 $a9 $e6 $a0 $66 }T T{ $00ec M $62a5 M $62a6 M $62a7 M -> $31 $d7 $ec $72 }T
$a06d >PC $90 >S $98 >A $b9 >X $94 >Y $27 >P $00e8 $b0 >M $a06d $d7 >M $a06e $e8 >M $a06f $19 >M
T{ op PC S A X Y P -> $a06f $90 $98 $b9 $94 $27 }T T{ $00e8 M $a06d M $a06e M $a06f M -> $b0 $d7 $e8 $19 }T
$f12b >PC $80 >S $e3 >A $7f >X $52 >Y $a6 >P $00ff $83 >M $f12b $d7 >M $f12c $ff >M $f12d $42 >M
T{ op PC S A X Y P -> $f12d $80 $e3 $7f $52 $a6 }T T{ $00ff M $f12b M $f12c M $f12d M -> $a3 $d7 $ff $42 }T
$0e4f >PC $5b >S $1a >A $ca >X $28 >Y $60 >P $0015 $66 >M $0e4f $d7 >M $0e50 $15 >M $0e51 $62 >M
T{ op PC S A X Y P -> $0e51 $5b $1a $ca $28 $60 }T T{ $0015 M $0e4f M $0e50 M $0e51 M -> $66 $d7 $15 $62 }T
$db38 >PC $88 >S $10 >A $c7 >X $52 >Y $61 >P $0020 $55 >M $db38 $d7 >M $db39 $20 >M $db3a $5f >M
T{ op PC S A X Y P -> $db3a $88 $10 $c7 $52 $61 }T T{ $0020 M $db38 M $db39 M $db3a M -> $75 $d7 $20 $5f }T
$c239 >PC $05 >S $0e >A $b8 >X $e1 >Y $e1 >P $008e $72 >M $c239 $d7 >M $c23a $8e >M $c23b $bc >M
T{ op PC S A X Y P -> $c23b $05 $0e $b8 $e1 $e1 }T T{ $008e M $c239 M $c23a M $c23b M -> $72 $d7 $8e $bc }T
$f0d7 >PC $c9 >S $49 >A $d4 >X $34 >Y $e1 >P $000f $7a >M $f0d7 $d7 >M $f0d8 $0f >M $f0d9 $3c >M
T{ op PC S A X Y P -> $f0d9 $c9 $49 $d4 $34 $e1 }T T{ $000f M $f0d7 M $f0d8 M $f0d9 M -> $7a $d7 $0f $3c }T
$174f >PC $cd >S $19 >A $2a >X $d7 >Y $67 >P $00ed $11 >M $174f $d7 >M $1750 $ed >M $1751 $c3 >M
T{ op PC S A X Y P -> $1751 $cd $19 $2a $d7 $67 }T T{ $00ed M $174f M $1750 M $1751 M -> $31 $d7 $ed $c3 }T
$ed4e >PC $16 >S $e4 >A $9b >X $64 >Y $65 >P $00f5 $91 >M $ed4e $d7 >M $ed4f $f5 >M $ed50 $e2 >M
T{ op PC S A X Y P -> $ed50 $16 $e4 $9b $64 $65 }T T{ $00f5 M $ed4e M $ed4f M $ed50 M -> $b1 $d7 $f5 $e2 }T
$c88e >PC $50 >S $1a >A $ff >X $26 >Y $a3 >P $0036 $05 >M $c88e $d7 >M $c88f $36 >M $c890 $b3 >M
T{ op PC S A X Y P -> $c890 $50 $1a $ff $26 $a3 }T T{ $0036 M $c88e M $c88f M $c890 M -> $25 $d7 $36 $b3 }T
$780f >PC $ac >S $25 >A $02 >X $f9 >Y $63 >P $003e $6c >M $780f $d7 >M $7810 $3e >M $7811 $b4 >M
T{ op PC S A X Y P -> $7811 $ac $25 $02 $f9 $63 }T T{ $003e M $780f M $7810 M $7811 M -> $6c $d7 $3e $b4 }T
( d8 )
$892b >PC $65 >S $0f >A $f0 >X $a7 >Y $65 >P $892b $d8 >M $892c $07 >M $892d $08 >M
T{ op PC S A X Y P -> $892c $65 $0f $f0 $a7 $65 }T T{ $892b M $892c M $892d M -> $d8 $07 $08 }T
$6f9d >PC $fd >S $f9 >A $d8 >X $3d >Y $a3 >P $6f9d $d8 >M $6f9e $a5 >M $6f9f $14 >M
T{ op PC S A X Y P -> $6f9e $fd $f9 $d8 $3d $a3 }T T{ $6f9d M $6f9e M $6f9f M -> $d8 $a5 $14 }T
$b43b >PC $90 >S $b9 >A $cc >X $00 >Y $a0 >P $b43b $d8 >M $b43c $a5 >M $b43d $66 >M
T{ op PC S A X Y P -> $b43c $90 $b9 $cc $00 $a0 }T T{ $b43b M $b43c M $b43d M -> $d8 $a5 $66 }T
$7902 >PC $bf >S $a1 >A $3e >X $61 >Y $63 >P $7902 $d8 >M $7903 $82 >M $7904 $52 >M
T{ op PC S A X Y P -> $7903 $bf $a1 $3e $61 $63 }T T{ $7902 M $7903 M $7904 M -> $d8 $82 $52 }T
$9b00 >PC $3e >S $2f >A $5f >X $bf >Y $61 >P $9b00 $d8 >M $9b01 $c2 >M $9b02 $51 >M
T{ op PC S A X Y P -> $9b01 $3e $2f $5f $bf $61 }T T{ $9b00 M $9b01 M $9b02 M -> $d8 $c2 $51 }T
$d777 >PC $d4 >S $86 >A $40 >X $f4 >Y $e0 >P $d777 $d8 >M $d778 $72 >M $d779 $1b >M
T{ op PC S A X Y P -> $d778 $d4 $86 $40 $f4 $e0 }T T{ $d777 M $d778 M $d779 M -> $d8 $72 $1b }T
$9b2d >PC $b3 >S $d8 >A $28 >X $71 >Y $e5 >P $9b2d $d8 >M $9b2e $cf >M $9b2f $8b >M
T{ op PC S A X Y P -> $9b2e $b3 $d8 $28 $71 $e5 }T T{ $9b2d M $9b2e M $9b2f M -> $d8 $cf $8b }T
$132f >PC $04 >S $7c >A $e7 >X $40 >Y $20 >P $132f $d8 >M $1330 $12 >M $1331 $c8 >M
T{ op PC S A X Y P -> $1330 $04 $7c $e7 $40 $20 }T T{ $132f M $1330 M $1331 M -> $d8 $12 $c8 }T
$afab >PC $19 >S $ad >A $de >X $0b >Y $e2 >P $afab $d8 >M $afac $1b >M $afad $05 >M
T{ op PC S A X Y P -> $afac $19 $ad $de $0b $e2 }T T{ $afab M $afac M $afad M -> $d8 $1b $05 }T
$d781 >PC $38 >S $a2 >A $c7 >X $f2 >Y $21 >P $d781 $d8 >M $d782 $da >M $d783 $a4 >M
T{ op PC S A X Y P -> $d782 $38 $a2 $c7 $f2 $21 }T T{ $d781 M $d782 M $d783 M -> $d8 $da $a4 }T
$57fb >PC $6a >S $2e >A $a9 >X $0b >Y $a6 >P $57fb $d8 >M $57fc $a9 >M $57fd $23 >M
T{ op PC S A X Y P -> $57fc $6a $2e $a9 $0b $a6 }T T{ $57fb M $57fc M $57fd M -> $d8 $a9 $23 }T
$8c77 >PC $7e >S $d4 >A $0e >X $5a >Y $e3 >P $8c77 $d8 >M $8c78 $71 >M $8c79 $d2 >M
T{ op PC S A X Y P -> $8c78 $7e $d4 $0e $5a $e3 }T T{ $8c77 M $8c78 M $8c79 M -> $d8 $71 $d2 }T
$02c5 >PC $4d >S $20 >A $0c >X $a8 >Y $27 >P $02c5 $d8 >M $02c6 $df >M $02c7 $40 >M
T{ op PC S A X Y P -> $02c6 $4d $20 $0c $a8 $27 }T T{ $02c5 M $02c6 M $02c7 M -> $d8 $df $40 }T
$87bd >PC $f2 >S $27 >A $15 >X $3e >Y $62 >P $87bd $d8 >M $87be $29 >M $87bf $03 >M
T{ op PC S A X Y P -> $87be $f2 $27 $15 $3e $62 }T T{ $87bd M $87be M $87bf M -> $d8 $29 $03 }T
$ff01 >PC $29 >S $59 >A $57 >X $87 >Y $25 >P $ff01 $d8 >M $ff02 $d5 >M $ff03 $68 >M
T{ op PC S A X Y P -> $ff02 $29 $59 $57 $87 $25 }T T{ $ff01 M $ff02 M $ff03 M -> $d8 $d5 $68 }T
$0d46 >PC $c7 >S $6c >A $ed >X $96 >Y $65 >P $0d46 $d8 >M $0d47 $1e >M $0d48 $23 >M
T{ op PC S A X Y P -> $0d47 $c7 $6c $ed $96 $65 }T T{ $0d46 M $0d47 M $0d48 M -> $d8 $1e $23 }T
( d9 )
$ceb9 >PC $53 >S $f8 >A $c9 >X $5e >Y $a1 >P $81e8 $6b >M $ceb9 $d9 >M $ceba $8a >M $cebb $81 >M $cebc $65 >M
T{ op PC S A X Y P -> $cebc $53 $f8 $c9 $5e $a1 }T T{ $81e8 M $ceb9 M $ceba M $cebb M $cebc M -> $6b $d9 $8a $81 $65 }T
$b28a >PC $35 >S $8b >A $74 >X $3a >Y $a1 >P $b28a $d9 >M $b28b $1e >M $b28c $d7 >M $b28d $1a >M $d758 $e2 >M
T{ op PC S A X Y P -> $b28d $35 $8b $74 $3a $a0 }T T{ $b28a M $b28b M $b28c M $b28d M $d758 M -> $d9 $1e $d7 $1a $e2 }T
$6e7e >PC $fa >S $3e >A $c9 >X $bd >Y $20 >P $6e7e $d9 >M $6e7f $4f >M $6e80 $eb >M $6e81 $cf >M $ec0c $3e >M
T{ op PC S A X Y P -> $6e81 $fa $3e $c9 $bd $23 }T T{ $6e7e M $6e7f M $6e80 M $6e81 M $ec0c M -> $d9 $4f $eb $cf $3e }T
$e6de >PC $4b >S $7d >A $9c >X $87 >Y $65 >P $e6de $d9 >M $e6df $95 >M $e6e0 $f5 >M $e6e1 $9f >M $f61c $9d >M
T{ op PC S A X Y P -> $e6e1 $4b $7d $9c $87 $e4 }T T{ $e6de M $e6df M $e6e0 M $e6e1 M $f61c M -> $d9 $95 $f5 $9f $9d }T
$9d35 >PC $0d >S $f8 >A $af >X $96 >Y $e0 >P $3f66 $f2 >M $9d35 $d9 >M $9d36 $d0 >M $9d37 $3e >M $9d38 $2d >M
T{ op PC S A X Y P -> $9d38 $0d $f8 $af $96 $61 }T T{ $3f66 M $9d35 M $9d36 M $9d37 M $9d38 M -> $f2 $d9 $d0 $3e $2d }T
$5a3e >PC $f7 >S $a2 >A $c8 >X $ae >Y $20 >P $5a3e $d9 >M $5a3f $96 >M $5a40 $af >M $5a41 $74 >M $b044 $af >M
T{ op PC S A X Y P -> $5a41 $f7 $a2 $c8 $ae $a0 }T T{ $5a3e M $5a3f M $5a40 M $5a41 M $b044 M -> $d9 $96 $af $74 $af }T
$6746 >PC $57 >S $d7 >A $b7 >X $1a >Y $24 >P $4d9a $45 >M $6746 $d9 >M $6747 $80 >M $6748 $4d >M $6749 $d7 >M
T{ op PC S A X Y P -> $6749 $57 $d7 $b7 $1a $a5 }T T{ $4d9a M $6746 M $6747 M $6748 M $6749 M -> $45 $d9 $80 $4d $d7 }T
$fe04 >PC $fc >S $4a >A $89 >X $1f >Y $60 >P $3a72 $a6 >M $fe04 $d9 >M $fe05 $53 >M $fe06 $3a >M $fe07 $29 >M
T{ op PC S A X Y P -> $fe07 $fc $4a $89 $1f $e0 }T T{ $3a72 M $fe04 M $fe05 M $fe06 M $fe07 M -> $a6 $d9 $53 $3a $29 }T
$4a9e >PC $03 >S $37 >A $a3 >X $7e >Y $27 >P $4a9e $d9 >M $4a9f $20 >M $4aa0 $76 >M $4aa1 $9f >M $769e $6b >M
T{ op PC S A X Y P -> $4aa1 $03 $37 $a3 $7e $a4 }T T{ $4a9e M $4a9f M $4aa0 M $4aa1 M $769e M -> $d9 $20 $76 $9f $6b }T
$abb8 >PC $3b >S $24 >A $47 >X $29 >Y $23 >P $abb8 $d9 >M $abb9 $a1 >M $abba $d3 >M $abbb $54 >M $d3ca $7c >M
T{ op PC S A X Y P -> $abbb $3b $24 $47 $29 $a0 }T T{ $abb8 M $abb9 M $abba M $abbb M $d3ca M -> $d9 $a1 $d3 $54 $7c }T
$b64c >PC $6d >S $61 >A $27 >X $53 >Y $e1 >P $4646 $dc >M $b64c $d9 >M $b64d $f3 >M $b64e $45 >M $b64f $5e >M
T{ op PC S A X Y P -> $b64f $6d $61 $27 $53 $e0 }T T{ $4646 M $b64c M $b64d M $b64e M $b64f M -> $dc $d9 $f3 $45 $5e }T
$7aac >PC $f1 >S $22 >A $0e >X $84 >Y $63 >P $7aac $d9 >M $7aad $d0 >M $7aae $d9 >M $7aaf $97 >M $da54 $29 >M
T{ op PC S A X Y P -> $7aaf $f1 $22 $0e $84 $e0 }T T{ $7aac M $7aad M $7aae M $7aaf M $da54 M -> $d9 $d0 $d9 $97 $29 }T
$5565 >PC $6a >S $40 >A $a1 >X $1d >Y $a7 >P $5565 $d9 >M $5566 $66 >M $5567 $97 >M $5568 $c7 >M $9783 $fe >M
T{ op PC S A X Y P -> $5568 $6a $40 $a1 $1d $24 }T T{ $5565 M $5566 M $5567 M $5568 M $9783 M -> $d9 $66 $97 $c7 $fe }T
$c1d0 >PC $24 >S $ee >A $0d >X $2b >Y $66 >P $9552 $be >M $c1d0 $d9 >M $c1d1 $27 >M $c1d2 $95 >M $c1d3 $51 >M
T{ op PC S A X Y P -> $c1d3 $24 $ee $0d $2b $65 }T T{ $9552 M $c1d0 M $c1d1 M $c1d2 M $c1d3 M -> $be $d9 $27 $95 $51 }T
$c402 >PC $ac >S $87 >A $91 >X $39 >Y $61 >P $c402 $d9 >M $c403 $ef >M $c404 $e7 >M $c405 $25 >M $e828 $d8 >M
T{ op PC S A X Y P -> $c405 $ac $87 $91 $39 $e0 }T T{ $c402 M $c403 M $c404 M $c405 M $e828 M -> $d9 $ef $e7 $25 $d8 }T
$3d9c >PC $7c >S $a0 >A $11 >X $96 >Y $a2 >P $3d9c $d9 >M $3d9d $6e >M $3d9e $d2 >M $3d9f $e5 >M $d304 $17 >M
T{ op PC S A X Y P -> $3d9f $7c $a0 $11 $96 $a1 }T T{ $3d9c M $3d9d M $3d9e M $3d9f M $d304 M -> $d9 $6e $d2 $e5 $17 }T
( da )
$6203 >PC $29 >S $6c >A $94 >X $90 >Y $21 >P $6203 $da >M $6204 $8b >M $6205 $c5 >M
T{ op PC S A X Y P -> $6204 $28 $6c $94 $90 $21 }T T{ $0129 M $6203 M $6204 M $6205 M -> $94 $da $8b $c5 }T
$6f21 >PC $eb >S $e9 >A $18 >X $24 >Y $a7 >P $6f21 $da >M $6f22 $1f >M $6f23 $06 >M
T{ op PC S A X Y P -> $6f22 $ea $e9 $18 $24 $a7 }T T{ $01eb M $6f21 M $6f22 M $6f23 M -> $18 $da $1f $06 }T
$966a >PC $51 >S $ff >A $62 >X $52 >Y $e3 >P $966a $da >M $966b $22 >M $966c $61 >M
T{ op PC S A X Y P -> $966b $50 $ff $62 $52 $e3 }T T{ $0151 M $966a M $966b M $966c M -> $62 $da $22 $61 }T
$c1c1 >PC $f9 >S $15 >A $6e >X $db >Y $24 >P $c1c1 $da >M $c1c2 $53 >M $c1c3 $1d >M
T{ op PC S A X Y P -> $c1c2 $f8 $15 $6e $db $24 }T T{ $01f9 M $c1c1 M $c1c2 M $c1c3 M -> $6e $da $53 $1d }T
$6478 >PC $76 >S $1b >A $b0 >X $a8 >Y $e4 >P $6478 $da >M $6479 $d7 >M $647a $ec >M
T{ op PC S A X Y P -> $6479 $75 $1b $b0 $a8 $e4 }T T{ $0176 M $6478 M $6479 M $647a M -> $b0 $da $d7 $ec }T
$26fc >PC $6b >S $97 >A $91 >X $14 >Y $a0 >P $26fc $da >M $26fd $10 >M $26fe $27 >M
T{ op PC S A X Y P -> $26fd $6a $97 $91 $14 $a0 }T T{ $016b M $26fc M $26fd M $26fe M -> $91 $da $10 $27 }T
$7be7 >PC $8e >S $67 >A $c0 >X $90 >Y $a5 >P $7be7 $da >M $7be8 $8c >M $7be9 $30 >M
T{ op PC S A X Y P -> $7be8 $8d $67 $c0 $90 $a5 }T T{ $018e M $7be7 M $7be8 M $7be9 M -> $c0 $da $8c $30 }T
$18f7 >PC $af >S $13 >A $b7 >X $b0 >Y $22 >P $18f7 $da >M $18f8 $53 >M $18f9 $31 >M
T{ op PC S A X Y P -> $18f8 $ae $13 $b7 $b0 $22 }T T{ $01af M $18f7 M $18f8 M $18f9 M -> $b7 $da $53 $31 }T
$fbd8 >PC $7f >S $de >A $d3 >X $b2 >Y $21 >P $fbd8 $da >M $fbd9 $b4 >M $fbda $b2 >M
T{ op PC S A X Y P -> $fbd9 $7e $de $d3 $b2 $21 }T T{ $017f M $fbd8 M $fbd9 M $fbda M -> $d3 $da $b4 $b2 }T
$f3ff >PC $9a >S $72 >A $3b >X $5c >Y $e5 >P $f3ff $da >M $f400 $22 >M $f401 $48 >M
T{ op PC S A X Y P -> $f400 $99 $72 $3b $5c $e5 }T T{ $019a M $f3ff M $f400 M $f401 M -> $3b $da $22 $48 }T
$5b51 >PC $f1 >S $15 >A $f3 >X $bb >Y $61 >P $5b51 $da >M $5b52 $72 >M $5b53 $c2 >M
T{ op PC S A X Y P -> $5b52 $f0 $15 $f3 $bb $61 }T T{ $01f1 M $5b51 M $5b52 M $5b53 M -> $f3 $da $72 $c2 }T
$1c90 >PC $ec >S $36 >A $a0 >X $9b >Y $66 >P $1c90 $da >M $1c91 $0b >M $1c92 $52 >M
T{ op PC S A X Y P -> $1c91 $eb $36 $a0 $9b $66 }T T{ $01ec M $1c90 M $1c91 M $1c92 M -> $a0 $da $0b $52 }T
$c44e >PC $e7 >S $11 >A $f8 >X $c5 >Y $a2 >P $c44e $da >M $c44f $6c >M $c450 $86 >M
T{ op PC S A X Y P -> $c44f $e6 $11 $f8 $c5 $a2 }T T{ $01e7 M $c44e M $c44f M $c450 M -> $f8 $da $6c $86 }T
$c915 >PC $70 >S $fb >A $7c >X $5e >Y $a0 >P $c915 $da >M $c916 $36 >M $c917 $3e >M
T{ op PC S A X Y P -> $c916 $6f $fb $7c $5e $a0 }T T{ $0170 M $c915 M $c916 M $c917 M -> $7c $da $36 $3e }T
$3634 >PC $0e >S $94 >A $8c >X $89 >Y $65 >P $3634 $da >M $3635 $3d >M $3636 $85 >M
T{ op PC S A X Y P -> $3635 $0d $94 $8c $89 $65 }T T{ $010e M $3634 M $3635 M $3636 M -> $8c $da $3d $85 }T
$a0b6 >PC $4c >S $30 >A $6b >X $d9 >Y $20 >P $a0b6 $da >M $a0b7 $04 >M $a0b8 $de >M
T{ op PC S A X Y P -> $a0b7 $4b $30 $6b $d9 $20 }T T{ $014c M $a0b6 M $a0b7 M $a0b8 M -> $6b $da $04 $de }T
( db )
\ Skipping invalid json wdc65c02/v1/db.json
( dc )
$2779 >PC $75 >S $08 >A $89 >X $34 >Y $a1 >P $2779 $dc >M $277a $b4 >M $277b $94 >M $277c $af >M
T{ op PC S A X Y P -> $277c $75 $08 $89 $34 $a1 }T T{ $2779 M $277a M $277b M $277c M -> $dc $b4 $94 $af }T
$bb11 >PC $6b >S $fe >A $72 >X $39 >Y $a3 >P $bb11 $dc >M $bb12 $0b >M $bb13 $16 >M $bb14 $cc >M
T{ op PC S A X Y P -> $bb14 $6b $fe $72 $39 $a3 }T T{ $bb11 M $bb12 M $bb13 M $bb14 M -> $dc $0b $16 $cc }T
$83b5 >PC $76 >S $2a >A $ea >X $35 >Y $65 >P $83b5 $dc >M $83b6 $e0 >M $83b7 $73 >M $83b8 $ab >M
T{ op PC S A X Y P -> $83b8 $76 $2a $ea $35 $65 }T T{ $83b5 M $83b6 M $83b7 M $83b8 M -> $dc $e0 $73 $ab }T
$3b1a >PC $39 >S $e4 >A $b2 >X $e8 >Y $a1 >P $3b1a $dc >M $3b1b $a7 >M $3b1c $5d >M $3b1d $d9 >M
T{ op PC S A X Y P -> $3b1d $39 $e4 $b2 $e8 $a1 }T T{ $3b1a M $3b1b M $3b1c M $3b1d M -> $dc $a7 $5d $d9 }T
$be69 >PC $44 >S $26 >A $c6 >X $f0 >Y $60 >P $be69 $dc >M $be6a $50 >M $be6b $12 >M $be6c $80 >M
T{ op PC S A X Y P -> $be6c $44 $26 $c6 $f0 $60 }T T{ $be69 M $be6a M $be6b M $be6c M -> $dc $50 $12 $80 }T
$c872 >PC $ec >S $bd >A $bb >X $c5 >Y $21 >P $c872 $dc >M $c873 $16 >M $c874 $40 >M $c875 $28 >M
T{ op PC S A X Y P -> $c875 $ec $bd $bb $c5 $21 }T T{ $c872 M $c873 M $c874 M $c875 M -> $dc $16 $40 $28 }T
$a8e8 >PC $cd >S $d2 >A $08 >X $9f >Y $a3 >P $a8e8 $dc >M $a8e9 $b8 >M $a8ea $50 >M $a8eb $cf >M
T{ op PC S A X Y P -> $a8eb $cd $d2 $08 $9f $a3 }T T{ $a8e8 M $a8e9 M $a8ea M $a8eb M -> $dc $b8 $50 $cf }T
$af36 >PC $1f >S $10 >A $e2 >X $b3 >Y $25 >P $af36 $dc >M $af37 $0f >M $af38 $18 >M $af39 $86 >M
T{ op PC S A X Y P -> $af39 $1f $10 $e2 $b3 $25 }T T{ $af36 M $af37 M $af38 M $af39 M -> $dc $0f $18 $86 }T
$8b3c >PC $4b >S $75 >A $b9 >X $ef >Y $a2 >P $8b3c $dc >M $8b3d $b4 >M $8b3e $9f >M $8b3f $b5 >M
T{ op PC S A X Y P -> $8b3f $4b $75 $b9 $ef $a2 }T T{ $8b3c M $8b3d M $8b3e M $8b3f M -> $dc $b4 $9f $b5 }T
$0d11 >PC $6b >S $a0 >A $9b >X $6f >Y $20 >P $0d11 $dc >M $0d12 $32 >M $0d13 $b1 >M $0d14 $a6 >M
T{ op PC S A X Y P -> $0d14 $6b $a0 $9b $6f $20 }T T{ $0d11 M $0d12 M $0d13 M $0d14 M -> $dc $32 $b1 $a6 }T
$3275 >PC $de >S $34 >A $90 >X $48 >Y $61 >P $3275 $dc >M $3276 $19 >M $3277 $20 >M $3278 $db >M
T{ op PC S A X Y P -> $3278 $de $34 $90 $48 $61 }T T{ $3275 M $3276 M $3277 M $3278 M -> $dc $19 $20 $db }T
$8a7f >PC $51 >S $64 >A $9e >X $2a >Y $62 >P $8a7f $dc >M $8a80 $b1 >M $8a81 $38 >M $8a82 $78 >M
T{ op PC S A X Y P -> $8a82 $51 $64 $9e $2a $62 }T T{ $8a7f M $8a80 M $8a81 M $8a82 M -> $dc $b1 $38 $78 }T
$0811 >PC $84 >S $0f >A $d7 >X $e7 >Y $a7 >P $0811 $dc >M $0812 $78 >M $0813 $41 >M $0814 $c8 >M
T{ op PC S A X Y P -> $0814 $84 $0f $d7 $e7 $a7 }T T{ $0811 M $0812 M $0813 M $0814 M -> $dc $78 $41 $c8 }T
$1494 >PC $d8 >S $83 >A $c4 >X $ad >Y $a4 >P $1494 $dc >M $1495 $7b >M $1496 $fb >M $1497 $90 >M
T{ op PC S A X Y P -> $1497 $d8 $83 $c4 $ad $a4 }T T{ $1494 M $1495 M $1496 M $1497 M -> $dc $7b $fb $90 }T
$6f79 >PC $43 >S $59 >A $15 >X $51 >Y $66 >P $6f79 $dc >M $6f7a $67 >M $6f7b $2e >M $6f7c $c6 >M
T{ op PC S A X Y P -> $6f7c $43 $59 $15 $51 $66 }T T{ $6f79 M $6f7a M $6f7b M $6f7c M -> $dc $67 $2e $c6 }T
$9352 >PC $a1 >S $95 >A $f9 >X $ac >Y $64 >P $9352 $dc >M $9353 $ba >M $9354 $b0 >M $9355 $1f >M
T{ op PC S A X Y P -> $9355 $a1 $95 $f9 $ac $64 }T T{ $9352 M $9353 M $9354 M $9355 M -> $dc $ba $b0 $1f }T
( dd )
$5a2c >PC $bd >S $17 >A $60 >X $93 >Y $e4 >P $52b6 $23 >M $5a2c $dd >M $5a2d $56 >M $5a2e $52 >M $5a2f $4d >M
T{ op PC S A X Y P -> $5a2f $bd $17 $60 $93 $e4 }T T{ $52b6 M $5a2c M $5a2d M $5a2e M $5a2f M -> $23 $dd $56 $52 $4d }T
$2a16 >PC $bd >S $f8 >A $a4 >X $80 >Y $24 >P $2a16 $dd >M $2a17 $5b >M $2a18 $99 >M $2a19 $a6 >M $99ff $b5 >M
T{ op PC S A X Y P -> $2a19 $bd $f8 $a4 $80 $25 }T T{ $2a16 M $2a17 M $2a18 M $2a19 M $99ff M -> $dd $5b $99 $a6 $b5 }T
$2de1 >PC $c3 >S $3f >A $7e >X $af >Y $e3 >P $29d4 $b0 >M $2de1 $dd >M $2de2 $56 >M $2de3 $29 >M $2de4 $eb >M
T{ op PC S A X Y P -> $2de4 $c3 $3f $7e $af $e0 }T T{ $29d4 M $2de1 M $2de2 M $2de3 M $2de4 M -> $b0 $dd $56 $29 $eb }T
$5c6c >PC $65 >S $d6 >A $ee >X $57 >Y $61 >P $00fa $85 >M $5c6c $dd >M $5c6d $0c >M $5c6e $00 >M $5c6f $70 >M
T{ op PC S A X Y P -> $5c6f $65 $d6 $ee $57 $61 }T T{ $00fa M $5c6c M $5c6d M $5c6e M $5c6f M -> $85 $dd $0c $00 $70 }T
$3791 >PC $56 >S $b0 >A $e8 >X $f1 >Y $e7 >P $3791 $dd >M $3792 $4c >M $3793 $e3 >M $3794 $14 >M $e434 $32 >M
T{ op PC S A X Y P -> $3794 $56 $b0 $e8 $f1 $65 }T T{ $3791 M $3792 M $3793 M $3794 M $e434 M -> $dd $4c $e3 $14 $32 }T
$9874 >PC $6a >S $77 >A $17 >X $cb >Y $a4 >P $9874 $dd >M $9875 $93 >M $9876 $d1 >M $9877 $42 >M $d1aa $9d >M
T{ op PC S A X Y P -> $9877 $6a $77 $17 $cb $a4 }T T{ $9874 M $9875 M $9876 M $9877 M $d1aa M -> $dd $93 $d1 $42 $9d }T
$9da2 >PC $83 >S $62 >A $75 >X $4c >Y $20 >P $60c9 $5b >M $9da2 $dd >M $9da3 $54 >M $9da4 $60 >M $9da5 $2a >M
T{ op PC S A X Y P -> $9da5 $83 $62 $75 $4c $21 }T T{ $60c9 M $9da2 M $9da3 M $9da4 M $9da5 M -> $5b $dd $54 $60 $2a }T
$265c >PC $a1 >S $cb >A $25 >X $7e >Y $21 >P $265c $dd >M $265d $c4 >M $265e $2c >M $265f $9d >M $2ce9 $d7 >M
T{ op PC S A X Y P -> $265f $a1 $cb $25 $7e $a0 }T T{ $265c M $265d M $265e M $265f M $2ce9 M -> $dd $c4 $2c $9d $d7 }T
$11f5 >PC $a0 >S $55 >A $53 >X $47 >Y $66 >P $11f5 $dd >M $11f6 $41 >M $11f7 $39 >M $11f8 $f9 >M $3994 $55 >M
T{ op PC S A X Y P -> $11f8 $a0 $55 $53 $47 $67 }T T{ $11f5 M $11f6 M $11f7 M $11f8 M $3994 M -> $dd $41 $39 $f9 $55 }T
$d6dc >PC $ad >S $01 >A $f7 >X $8c >Y $e2 >P $d6dc $dd >M $d6dd $00 >M $d6de $ef >M $d6df $ed >M $eff7 $1a >M
T{ op PC S A X Y P -> $d6df $ad $01 $f7 $8c $e0 }T T{ $d6dc M $d6dd M $d6de M $d6df M $eff7 M -> $dd $00 $ef $ed $1a }T
$345c >PC $73 >S $f9 >A $20 >X $79 >Y $a6 >P $345c $dd >M $345d $95 >M $345e $84 >M $345f $02 >M $84b5 $b9 >M
T{ op PC S A X Y P -> $345f $73 $f9 $20 $79 $25 }T T{ $345c M $345d M $345e M $345f M $84b5 M -> $dd $95 $84 $02 $b9 }T
$a3c5 >PC $1a >S $b0 >A $be >X $70 >Y $65 >P $28bc $0a >M $a3c5 $dd >M $a3c6 $fe >M $a3c7 $27 >M $a3c8 $6a >M
T{ op PC S A X Y P -> $a3c8 $1a $b0 $be $70 $e5 }T T{ $28bc M $a3c5 M $a3c6 M $a3c7 M $a3c8 M -> $0a $dd $fe $27 $6a }T
$34a8 >PC $4c >S $e8 >A $cb >X $0b >Y $23 >P $34a8 $dd >M $34a9 $95 >M $34aa $f8 >M $34ab $7a >M $f960 $70 >M
T{ op PC S A X Y P -> $34ab $4c $e8 $cb $0b $21 }T T{ $34a8 M $34a9 M $34aa M $34ab M $f960 M -> $dd $95 $f8 $7a $70 }T
$9b1a >PC $6b >S $24 >A $7a >X $2d >Y $21 >P $46d0 $7e >M $9b1a $dd >M $9b1b $56 >M $9b1c $46 >M $9b1d $3e >M
T{ op PC S A X Y P -> $9b1d $6b $24 $7a $2d $a0 }T T{ $46d0 M $9b1a M $9b1b M $9b1c M $9b1d M -> $7e $dd $56 $46 $3e }T
$c598 >PC $b0 >S $e2 >A $08 >X $4c >Y $25 >P $c598 $dd >M $c599 $0b >M $c59a $d0 >M $c59b $83 >M $d013 $a0 >M
T{ op PC S A X Y P -> $c59b $b0 $e2 $08 $4c $25 }T T{ $c598 M $c599 M $c59a M $c59b M $d013 M -> $dd $0b $d0 $83 $a0 }T
$d24a >PC $a8 >S $63 >A $94 >X $93 >Y $26 >P $100d $9c >M $d24a $dd >M $d24b $79 >M $d24c $0f >M $d24d $ee >M
T{ op PC S A X Y P -> $d24d $a8 $63 $94 $93 $a4 }T T{ $100d M $d24a M $d24b M $d24c M $d24d M -> $9c $dd $79 $0f $ee }T
( de )
$675d >PC $ad >S $c6 >A $c2 >X $1b >Y $a2 >P $675d $de >M $675e $62 >M $675f $95 >M $6760 $81 >M $9624 $b3 >M
T{ op PC S A X Y P -> $6760 $ad $c6 $c2 $1b $a0 }T T{ $675d M $675e M $675f M $6760 M $9624 M -> $de $62 $95 $81 $b2 }T
$cd04 >PC $7f >S $79 >A $6b >X $f9 >Y $66 >P $3c4c $66 >M $cd04 $de >M $cd05 $e1 >M $cd06 $3b >M $cd07 $be >M
T{ op PC S A X Y P -> $cd07 $7f $79 $6b $f9 $64 }T T{ $3c4c M $cd04 M $cd05 M $cd06 M $cd07 M -> $65 $de $e1 $3b $be }T
$5064 >PC $bc >S $8c >A $ae >X $c8 >Y $e6 >P $2429 $21 >M $5064 $de >M $5065 $7b >M $5066 $23 >M $5067 $e6 >M
T{ op PC S A X Y P -> $5067 $bc $8c $ae $c8 $64 }T T{ $2429 M $5064 M $5065 M $5066 M $5067 M -> $20 $de $7b $23 $e6 }T
$42e1 >PC $d6 >S $8d >A $66 >X $2a >Y $61 >P $0acb $e7 >M $42e1 $de >M $42e2 $65 >M $42e3 $0a >M $42e4 $4f >M
T{ op PC S A X Y P -> $42e4 $d6 $8d $66 $2a $e1 }T T{ $0acb M $42e1 M $42e2 M $42e3 M $42e4 M -> $e6 $de $65 $0a $4f }T
$5f4f >PC $9c >S $de >A $1b >X $67 >Y $66 >P $52e7 $49 >M $5f4f $de >M $5f50 $cc >M $5f51 $52 >M $5f52 $2e >M
T{ op PC S A X Y P -> $5f52 $9c $de $1b $67 $64 }T T{ $52e7 M $5f4f M $5f50 M $5f51 M $5f52 M -> $48 $de $cc $52 $2e }T
$17cc >PC $c1 >S $90 >A $a3 >X $bf >Y $a1 >P $17cc $de >M $17cd $1d >M $17ce $ee >M $17cf $05 >M $eec0 $e4 >M
T{ op PC S A X Y P -> $17cf $c1 $90 $a3 $bf $a1 }T T{ $17cc M $17cd M $17ce M $17cf M $eec0 M -> $de $1d $ee $05 $e3 }T
$0cc3 >PC $06 >S $35 >A $e6 >X $78 >Y $65 >P $0cc3 $de >M $0cc4 $31 >M $0cc5 $87 >M $0cc6 $c8 >M $8817 $48 >M
T{ op PC S A X Y P -> $0cc6 $06 $35 $e6 $78 $65 }T T{ $0cc3 M $0cc4 M $0cc5 M $0cc6 M $8817 M -> $de $31 $87 $c8 $47 }T
$ab96 >PC $66 >S $26 >A $84 >X $28 >Y $65 >P $2abb $98 >M $ab96 $de >M $ab97 $37 >M $ab98 $2a >M $ab99 $55 >M
T{ op PC S A X Y P -> $ab99 $66 $26 $84 $28 $e5 }T T{ $2abb M $ab96 M $ab97 M $ab98 M $ab99 M -> $97 $de $37 $2a $55 }T
$7ff5 >PC $de >S $10 >A $0d >X $42 >Y $a2 >P $3ef8 $57 >M $7ff5 $de >M $7ff6 $eb >M $7ff7 $3e >M $7ff8 $35 >M
T{ op PC S A X Y P -> $7ff8 $de $10 $0d $42 $20 }T T{ $3ef8 M $7ff5 M $7ff6 M $7ff7 M $7ff8 M -> $56 $de $eb $3e $35 }T
$8311 >PC $91 >S $57 >A $a9 >X $92 >Y $a4 >P $0c95 $83 >M $8311 $de >M $8312 $ec >M $8313 $0b >M $8314 $11 >M
T{ op PC S A X Y P -> $8314 $91 $57 $a9 $92 $a4 }T T{ $0c95 M $8311 M $8312 M $8313 M $8314 M -> $82 $de $ec $0b $11 }T
$410f >PC $f7 >S $7a >A $c0 >X $93 >Y $a6 >P $410f $de >M $4110 $ae >M $4111 $6c >M $4112 $22 >M $6d6e $e5 >M
T{ op PC S A X Y P -> $4112 $f7 $7a $c0 $93 $a4 }T T{ $410f M $4110 M $4111 M $4112 M $6d6e M -> $de $ae $6c $22 $e4 }T
$fc42 >PC $70 >S $1a >A $81 >X $d1 >Y $e7 >P $5ae1 $77 >M $fc42 $de >M $fc43 $60 >M $fc44 $5a >M $fc45 $b9 >M
T{ op PC S A X Y P -> $fc45 $70 $1a $81 $d1 $65 }T T{ $5ae1 M $fc42 M $fc43 M $fc44 M $fc45 M -> $76 $de $60 $5a $b9 }T
$d6f6 >PC $02 >S $ec >A $b7 >X $8e >Y $a6 >P $5c58 $e1 >M $d6f6 $de >M $d6f7 $a1 >M $d6f8 $5b >M $d6f9 $84 >M
T{ op PC S A X Y P -> $d6f9 $02 $ec $b7 $8e $a4 }T T{ $5c58 M $d6f6 M $d6f7 M $d6f8 M $d6f9 M -> $e0 $de $a1 $5b $84 }T
$e31b >PC $21 >S $e4 >A $41 >X $5e >Y $e0 >P $e31b $de >M $e31c $b1 >M $e31d $ee >M $e31e $30 >M $eef2 $dc >M
T{ op PC S A X Y P -> $e31e $21 $e4 $41 $5e $e0 }T T{ $e31b M $e31c M $e31d M $e31e M $eef2 M -> $de $b1 $ee $30 $db }T
$c701 >PC $28 >S $3d >A $a1 >X $41 >Y $20 >P $ba20 $58 >M $c701 $de >M $c702 $7f >M $c703 $b9 >M $c704 $ad >M
T{ op PC S A X Y P -> $c704 $28 $3d $a1 $41 $20 }T T{ $ba20 M $c701 M $c702 M $c703 M $c704 M -> $57 $de $7f $b9 $ad }T
$b525 >PC $01 >S $2a >A $3c >X $f2 >Y $62 >P $7b6f $e2 >M $b525 $de >M $b526 $33 >M $b527 $7b >M $b528 $e8 >M
T{ op PC S A X Y P -> $b528 $01 $2a $3c $f2 $e0 }T T{ $7b6f M $b525 M $b526 M $b527 M $b528 M -> $e1 $de $33 $7b $e8 }T
( df )
$0f85 >PC $71 >S $96 >A $63 >X $1d >Y $61 >P $0020 $e9 >M $0f85 $df >M $0f86 $20 >M $0f87 $73 >M $0ffb $59 >M
T{ op PC S A X Y P -> $0ffb $71 $96 $63 $1d $61 }T T{ $0020 M $0f85 M $0f86 M $0f87 M $0ffb M -> $e9 $df $20 $73 $59 }T
$fbb1 >PC $bf >S $0b >A $ba >X $b7 >Y $60 >P $00d7 $74 >M $fb88 $32 >M $fbb1 $df >M $fbb2 $d7 >M $fbb3 $d4 >M
T{ op PC S A X Y P -> $fb88 $bf $0b $ba $b7 $60 }T T{ $00d7 M $fb88 M $fbb1 M $fbb2 M $fbb3 M -> $74 $32 $df $d7 $d4 }T
$1ae2 >PC $42 >S $9a >A $42 >X $3b >Y $22 >P $007d $4f >M $1ae2 $df >M $1ae3 $7d >M $1ae4 $fd >M $1ae5 $73 >M
T{ op PC S A X Y P -> $1ae5 $42 $9a $42 $3b $22 }T T{ $007d M $1ae2 M $1ae3 M $1ae4 M $1ae5 M -> $4f $df $7d $fd $73 }T
$e85e >PC $22 >S $ae >A $38 >X $a3 >Y $e4 >P $00bd $70 >M $e82c $f0 >M $e85e $df >M $e85f $bd >M $e860 $cb >M
T{ op PC S A X Y P -> $e82c $22 $ae $38 $a3 $e4 }T T{ $00bd M $e82c M $e85e M $e85f M $e860 M -> $70 $f0 $df $bd $cb }T
$e283 >PC $27 >S $38 >A $a6 >X $f0 >Y $23 >P $0045 $7f >M $e254 $08 >M $e283 $df >M $e284 $45 >M $e285 $ce >M
T{ op PC S A X Y P -> $e254 $27 $38 $a6 $f0 $23 }T T{ $0045 M $e254 M $e283 M $e284 M $e285 M -> $7f $08 $df $45 $ce }T
$a496 >PC $49 >S $29 >A $06 >X $6a >Y $66 >P $00e2 $47 >M $a496 $df >M $a497 $e2 >M $a498 $39 >M $a499 $88 >M $a4d2 $10 >M
T{ op PC S A X Y P -> $a499 $49 $29 $06 $6a $66 }T T{ $00e2 M $a496 M $a497 M $a498 M $a499 M $a4d2 M -> $47 $df $e2 $39 $88 $10 }T
$8de6 >PC $f2 >S $80 >A $42 >X $91 >Y $26 >P $00fd $91 >M $8d43 $15 >M $8de6 $df >M $8de7 $fd >M $8de8 $5a >M $8de9 $2d >M
T{ op PC S A X Y P -> $8de9 $f2 $80 $42 $91 $26 }T T{ $00fd M $8d43 M $8de6 M $8de7 M $8de8 M $8de9 M -> $91 $15 $df $fd $5a $2d }T
$30d6 >PC $87 >S $c8 >A $ab >X $f5 >Y $25 >P $0012 $d1 >M $3050 $fc >M $30d6 $df >M $30d7 $12 >M $30d8 $77 >M $30d9 $a7 >M
T{ op PC S A X Y P -> $30d9 $87 $c8 $ab $f5 $25 }T T{ $0012 M $3050 M $30d6 M $30d7 M $30d8 M $30d9 M -> $d1 $fc $df $12 $77 $a7 }T
$4e53 >PC $a8 >S $0d >A $74 >X $0e >Y $67 >P $0026 $b6 >M $4e2b $70 >M $4e53 $df >M $4e54 $26 >M $4e55 $d5 >M
T{ op PC S A X Y P -> $4e2b $a8 $0d $74 $0e $67 }T T{ $0026 M $4e2b M $4e53 M $4e54 M $4e55 M -> $b6 $70 $df $26 $d5 }T
$7ead >PC $5a >S $2b >A $ee >X $11 >Y $a3 >P $005a $f3 >M $7ead $df >M $7eae $5a >M $7eaf $4a >M $7efa $4b >M
T{ op PC S A X Y P -> $7efa $5a $2b $ee $11 $a3 }T T{ $005a M $7ead M $7eae M $7eaf M $7efa M -> $f3 $df $5a $4a $4b }T
$befe >PC $60 >S $90 >A $a7 >X $bd >Y $27 >P $0063 $d7 >M $befe $df >M $beff $63 >M $bf00 $30 >M $bf01 $bc >M $bf31 $d6 >M
T{ op PC S A X Y P -> $bf01 $60 $90 $a7 $bd $27 }T T{ $0063 M $befe M $beff M $bf00 M $bf01 M $bf31 M -> $d7 $df $63 $30 $bc $d6 }T
$e1e7 >PC $97 >S $64 >A $b6 >X $2d >Y $a1 >P $00ff $90 >M $e183 $f3 >M $e1e7 $df >M $e1e8 $ff >M $e1e9 $99 >M $e1ea $a1 >M
T{ op PC S A X Y P -> $e1ea $97 $64 $b6 $2d $a1 }T T{ $00ff M $e183 M $e1e7 M $e1e8 M $e1e9 M $e1ea M -> $90 $f3 $df $ff $99 $a1 }T
$6466 >PC $2d >S $c8 >A $d8 >X $dc >Y $60 >P $00e1 $f9 >M $6421 $61 >M $6466 $df >M $6467 $e1 >M $6468 $b8 >M
T{ op PC S A X Y P -> $6421 $2d $c8 $d8 $dc $60 }T T{ $00e1 M $6421 M $6466 M $6467 M $6468 M -> $f9 $61 $df $e1 $b8 }T
$b814 >PC $15 >S $31 >A $43 >X $41 >Y $25 >P $0054 $ac >M $b814 $df >M $b815 $54 >M $b816 $6b >M $b882 $cd >M
T{ op PC S A X Y P -> $b882 $15 $31 $43 $41 $25 }T T{ $0054 M $b814 M $b815 M $b816 M $b882 M -> $ac $df $54 $6b $cd }T
$e289 >PC $76 >S $20 >A $03 >X $fc >Y $22 >P $00c9 $50 >M $e289 $df >M $e28a $c9 >M $e28b $60 >M $e28c $99 >M $e2ec $0d >M
T{ op PC S A X Y P -> $e28c $76 $20 $03 $fc $22 }T T{ $00c9 M $e289 M $e28a M $e28b M $e28c M $e2ec M -> $50 $df $c9 $60 $99 $0d }T
$64e0 >PC $a5 >S $91 >A $18 >X $5e >Y $60 >P $000e $2f >M $64e0 $df >M $64e1 $0e >M $64e2 $ff >M
T{ op PC S A X Y P -> $64e2 $a5 $91 $18 $5e $60 }T T{ $000e M $64e0 M $64e1 M $64e2 M -> $2f $df $0e $ff }T
( e0 )
$8ff0 >PC $ea >S $71 >A $62 >X $f7 >Y $e1 >P $8ff0 $e0 >M $8ff1 $84 >M $8ff2 $7d >M
T{ op PC S A X Y P -> $8ff2 $ea $71 $62 $f7 $e0 }T T{ $8ff0 M $8ff1 M $8ff2 M -> $e0 $84 $7d }T
$4f36 >PC $42 >S $f8 >A $33 >X $8f >Y $a5 >P $4f36 $e0 >M $4f37 $9e >M $4f38 $eb >M
T{ op PC S A X Y P -> $4f38 $42 $f8 $33 $8f $a4 }T T{ $4f36 M $4f37 M $4f38 M -> $e0 $9e $eb }T
$993e >PC $2b >S $95 >A $ff >X $2d >Y $e5 >P $993e $e0 >M $993f $2b >M $9940 $f7 >M
T{ op PC S A X Y P -> $9940 $2b $95 $ff $2d $e5 }T T{ $993e M $993f M $9940 M -> $e0 $2b $f7 }T
$1601 >PC $00 >S $61 >A $58 >X $3b >Y $23 >P $1601 $e0 >M $1602 $2f >M $1603 $c4 >M
T{ op PC S A X Y P -> $1603 $00 $61 $58 $3b $21 }T T{ $1601 M $1602 M $1603 M -> $e0 $2f $c4 }T
$fa43 >PC $36 >S $b5 >A $9e >X $49 >Y $a4 >P $fa43 $e0 >M $fa44 $c4 >M $fa45 $bd >M
T{ op PC S A X Y P -> $fa45 $36 $b5 $9e $49 $a4 }T T{ $fa43 M $fa44 M $fa45 M -> $e0 $c4 $bd }T
$99e6 >PC $a1 >S $8a >A $23 >X $6e >Y $21 >P $99e6 $e0 >M $99e7 $98 >M $99e8 $65 >M
T{ op PC S A X Y P -> $99e8 $a1 $8a $23 $6e $a0 }T T{ $99e6 M $99e7 M $99e8 M -> $e0 $98 $65 }T
$6dba >PC $0e >S $31 >A $b3 >X $9a >Y $61 >P $6dba $e0 >M $6dbb $47 >M $6dbc $c0 >M
T{ op PC S A X Y P -> $6dbc $0e $31 $b3 $9a $61 }T T{ $6dba M $6dbb M $6dbc M -> $e0 $47 $c0 }T
$08d7 >PC $a4 >S $71 >A $09 >X $d2 >Y $a4 >P $08d7 $e0 >M $08d8 $35 >M $08d9 $d0 >M
T{ op PC S A X Y P -> $08d9 $a4 $71 $09 $d2 $a4 }T T{ $08d7 M $08d8 M $08d9 M -> $e0 $35 $d0 }T
$ee8f >PC $ac >S $2d >A $77 >X $0a >Y $60 >P $ee8f $e0 >M $ee90 $02 >M $ee91 $03 >M
T{ op PC S A X Y P -> $ee91 $ac $2d $77 $0a $61 }T T{ $ee8f M $ee90 M $ee91 M -> $e0 $02 $03 }T
$bd6b >PC $04 >S $d1 >A $e0 >X $1e >Y $64 >P $bd6b $e0 >M $bd6c $a2 >M $bd6d $9b >M
T{ op PC S A X Y P -> $bd6d $04 $d1 $e0 $1e $65 }T T{ $bd6b M $bd6c M $bd6d M -> $e0 $a2 $9b }T
$3ff0 >PC $b5 >S $61 >A $af >X $53 >Y $e5 >P $3ff0 $e0 >M $3ff1 $29 >M $3ff2 $66 >M
T{ op PC S A X Y P -> $3ff2 $b5 $61 $af $53 $e5 }T T{ $3ff0 M $3ff1 M $3ff2 M -> $e0 $29 $66 }T
$ae9d >PC $1b >S $a1 >A $ba >X $27 >Y $63 >P $ae9d $e0 >M $ae9e $2d >M $ae9f $a9 >M
T{ op PC S A X Y P -> $ae9f $1b $a1 $ba $27 $e1 }T T{ $ae9d M $ae9e M $ae9f M -> $e0 $2d $a9 }T
$a104 >PC $b9 >S $40 >A $82 >X $d4 >Y $e6 >P $a104 $e0 >M $a105 $60 >M $a106 $9b >M
T{ op PC S A X Y P -> $a106 $b9 $40 $82 $d4 $65 }T T{ $a104 M $a105 M $a106 M -> $e0 $60 $9b }T
$2d30 >PC $d4 >S $75 >A $1f >X $b4 >Y $61 >P $2d30 $e0 >M $2d31 $4e >M $2d32 $77 >M
T{ op PC S A X Y P -> $2d32 $d4 $75 $1f $b4 $e0 }T T{ $2d30 M $2d31 M $2d32 M -> $e0 $4e $77 }T
$355a >PC $a8 >S $bf >A $1e >X $b0 >Y $a4 >P $355a $e0 >M $355b $92 >M $355c $d5 >M
T{ op PC S A X Y P -> $355c $a8 $bf $1e $b0 $a4 }T T{ $355a M $355b M $355c M -> $e0 $92 $d5 }T
$2b56 >PC $04 >S $41 >A $2c >X $6d >Y $a3 >P $2b56 $e0 >M $2b57 $40 >M $2b58 $55 >M
T{ op PC S A X Y P -> $2b58 $04 $41 $2c $6d $a0 }T T{ $2b56 M $2b57 M $2b58 M -> $e0 $40 $55 }T
( e1 )
$92df >PC $28 >S $c5 >A $a9 >X $d7 >Y $33 >P $0082 $46 >M $0083 $88 >M $00d9 $28 >M $8846 $a1 >M $92df $e1 >M $92e0 $d9 >M
T{ op PC S A X Y P -> $92e1 $28 $24 $a9 $d7 $31 }T T{ $0082 M $0083 M $00d9 M $8846 M $92df M $92e0 M -> $46 $88 $28 $a1 $e1 $d9 }T
$8170 >PC $77 >S $97 >A $a1 >X $33 >Y $f1 >P $009a $58 >M $009b $85 >M $00f9 $a1 >M $8170 $e1 >M $8171 $f9 >M $8558 $d2 >M
T{ op PC S A X Y P -> $8172 $77 $c5 $a1 $33 $b0 }T T{ $009a M $009b M $00f9 M $8170 M $8171 M $8558 M -> $58 $85 $a1 $e1 $f9 $d2 }T
$0305 >PC $0e >S $be >A $e2 >X $fe >Y $f5 >P $005f $37 >M $0060 $1e >M $007d $16 >M $0305 $e1 >M $0306 $7d >M $1e37 $d3 >M
T{ op PC S A X Y P -> $0307 $0e $eb $e2 $fe $b4 }T T{ $005f M $0060 M $007d M $0305 M $0306 M $1e37 M -> $37 $1e $16 $e1 $7d $d3 }T
$ba1c >PC $88 >S $ce >A $93 >X $66 >Y $77 >P $001d $5b >M $001e $9e >M $008a $24 >M $9e5b $77 >M $ba1c $e1 >M $ba1d $8a >M
T{ op PC S A X Y P -> $ba1e $88 $57 $93 $66 $75 }T T{ $001d M $001e M $008a M $9e5b M $ba1c M $ba1d M -> $5b $9e $24 $77 $e1 $8a }T
$7cce >PC $d4 >S $21 >A $64 >X $3f >Y $32 >P $0021 $42 >M $0022 $89 >M $00bd $f6 >M $7cce $e1 >M $7ccf $bd >M $8942 $ed >M
T{ op PC S A X Y P -> $7cd0 $d4 $33 $64 $3f $30 }T T{ $0021 M $0022 M $00bd M $7cce M $7ccf M $8942 M -> $42 $89 $f6 $e1 $bd $ed }T
$82db >PC $54 >S $04 >A $6f >X $4e >Y $74 >P $0077 $75 >M $00e6 $f1 >M $00e7 $0f >M $0ff1 $9c >M $82db $e1 >M $82dc $77 >M
T{ op PC S A X Y P -> $82dd $54 $67 $6f $4e $34 }T T{ $0077 M $00e6 M $00e7 M $0ff1 M $82db M $82dc M -> $75 $f1 $0f $9c $e1 $77 }T
$b18e >PC $a1 >S $cc >A $b7 >X $47 >Y $77 >P $0003 $cc >M $0004 $16 >M $004c $fb >M $16cc $c2 >M $b18e $e1 >M $b18f $4c >M
T{ op PC S A X Y P -> $b190 $a1 $0a $b7 $47 $35 }T T{ $0003 M $0004 M $004c M $16cc M $b18e M $b18f M -> $cc $16 $fb $c2 $e1 $4c }T
$c9fe >PC $de >S $ae >A $d2 >X $58 >Y $37 >P $0071 $f4 >M $0072 $00 >M $009f $56 >M $00f4 $45 >M $c9fe $e1 >M $c9ff $9f >M
T{ op PC S A X Y P -> $ca00 $de $69 $d2 $58 $75 }T T{ $0071 M $0072 M $009f M $00f4 M $c9fe M $c9ff M -> $f4 $00 $56 $45 $e1 $9f }T
$c299 >PC $0f >S $9d >A $ca >X $97 >Y $f2 >P $0057 $5c >M $0058 $b2 >M $008d $e2 >M $b25c $1a >M $c299 $e1 >M $c29a $8d >M
T{ op PC S A X Y P -> $c29b $0f $82 $ca $97 $b1 }T T{ $0057 M $0058 M $008d M $b25c M $c299 M $c29a M -> $5c $b2 $e2 $1a $e1 $8d }T
$38d5 >PC $3b >S $17 >A $68 >X $88 >Y $76 >P $005e $83 >M $00c6 $0b >M $00c7 $59 >M $38d5 $e1 >M $38d6 $5e >M $590b $39 >M
T{ op PC S A X Y P -> $38d7 $3b $dd $68 $88 $b4 }T T{ $005e M $00c6 M $00c7 M $38d5 M $38d6 M $590b M -> $83 $0b $59 $e1 $5e $39 }T
$c83e >PC $52 >S $3f >A $00 >X $9d >Y $f5 >P $00a1 $7b >M $00a2 $5a >M $5a7b $ca >M $c83e $e1 >M $c83f $a1 >M
T{ op PC S A X Y P -> $c840 $52 $75 $00 $9d $34 }T T{ $00a1 M $00a2 M $5a7b M $c83e M $c83f M -> $7b $5a $ca $e1 $a1 }T
$cb9e >PC $8d >S $e7 >A $53 >X $a2 >Y $76 >P $0005 $ef >M $0058 $ce >M $0059 $0c >M $0cce $fb >M $cb9e $e1 >M $cb9f $05 >M
T{ op PC S A X Y P -> $cba0 $8d $eb $53 $a2 $b4 }T T{ $0005 M $0058 M $0059 M $0cce M $cb9e M $cb9f M -> $ef $ce $0c $fb $e1 $05 }T
$75a5 >PC $97 >S $0e >A $e9 >X $76 >Y $f6 >P $00da $8f >M $00db $c2 >M $00f1 $45 >M $75a5 $e1 >M $75a6 $f1 >M $c28f $a7 >M
T{ op PC S A X Y P -> $75a7 $97 $66 $e9 $76 $34 }T T{ $00da M $00db M $00f1 M $75a5 M $75a6 M $c28f M -> $8f $c2 $45 $e1 $f1 $a7 }T
$378f >PC $d8 >S $65 >A $8a >X $b0 >Y $b3 >P $005f $be >M $00e9 $04 >M $00ea $16 >M $1604 $fc >M $378f $e1 >M $3790 $5f >M
T{ op PC S A X Y P -> $3791 $d8 $69 $8a $b0 $30 }T T{ $005f M $00e9 M $00ea M $1604 M $378f M $3790 M -> $be $04 $16 $fc $e1 $5f }T
$880e >PC $58 >S $53 >A $58 >X $1f >Y $71 >P $0008 $fc >M $0060 $02 >M $0061 $b3 >M $880e $e1 >M $880f $08 >M $b302 $74 >M
T{ op PC S A X Y P -> $8810 $58 $df $58 $1f $b0 }T T{ $0008 M $0060 M $0061 M $880e M $880f M $b302 M -> $fc $02 $b3 $e1 $08 $74 }T
$4cc8 >PC $ee >S $ee >A $fb >X $a2 >Y $33 >P $001a $3b >M $001b $df >M $001f $ae >M $4cc8 $e1 >M $4cc9 $1f >M $df3b $d1 >M
T{ op PC S A X Y P -> $4cca $ee $1d $fb $a2 $31 }T T{ $001a M $001b M $001f M $4cc8 M $4cc9 M $df3b M -> $3b $df $ae $e1 $1f $d1 }T
( e2 )
$61cf >PC $6e >S $b0 >A $31 >X $99 >Y $a2 >P $61cf $e2 >M $61d0 $ed >M $61d1 $24 >M
T{ op PC S A X Y P -> $61d1 $6e $b0 $31 $99 $a2 }T T{ $61cf M $61d0 M $61d1 M -> $e2 $ed $24 }T
$c9da >PC $cb >S $51 >A $1c >X $73 >Y $e4 >P $c9da $e2 >M $c9db $be >M $c9dc $99 >M
T{ op PC S A X Y P -> $c9dc $cb $51 $1c $73 $e4 }T T{ $c9da M $c9db M $c9dc M -> $e2 $be $99 }T
$59e3 >PC $8f >S $ea >A $60 >X $ed >Y $a1 >P $59e3 $e2 >M $59e4 $8b >M $59e5 $bf >M
T{ op PC S A X Y P -> $59e5 $8f $ea $60 $ed $a1 }T T{ $59e3 M $59e4 M $59e5 M -> $e2 $8b $bf }T
$d46c >PC $2a >S $bf >A $bc >X $b6 >Y $a2 >P $d46c $e2 >M $d46d $93 >M $d46e $97 >M
T{ op PC S A X Y P -> $d46e $2a $bf $bc $b6 $a2 }T T{ $d46c M $d46d M $d46e M -> $e2 $93 $97 }T
$8bb3 >PC $2f >S $1b >A $29 >X $61 >Y $e0 >P $8bb3 $e2 >M $8bb4 $e4 >M $8bb5 $c3 >M
T{ op PC S A X Y P -> $8bb5 $2f $1b $29 $61 $e0 }T T{ $8bb3 M $8bb4 M $8bb5 M -> $e2 $e4 $c3 }T
$c76d >PC $4f >S $ed >A $96 >X $4d >Y $a0 >P $c76d $e2 >M $c76e $d8 >M $c76f $b4 >M
T{ op PC S A X Y P -> $c76f $4f $ed $96 $4d $a0 }T T{ $c76d M $c76e M $c76f M -> $e2 $d8 $b4 }T
$2fb8 >PC $19 >S $59 >A $be >X $12 >Y $65 >P $2fb8 $e2 >M $2fb9 $c4 >M $2fba $b4 >M
T{ op PC S A X Y P -> $2fba $19 $59 $be $12 $65 }T T{ $2fb8 M $2fb9 M $2fba M -> $e2 $c4 $b4 }T
$96f2 >PC $ec >S $cc >A $05 >X $65 >Y $23 >P $96f2 $e2 >M $96f3 $7b >M $96f4 $20 >M
T{ op PC S A X Y P -> $96f4 $ec $cc $05 $65 $23 }T T{ $96f2 M $96f3 M $96f4 M -> $e2 $7b $20 }T
$ff3f >PC $ff >S $fb >A $8e >X $76 >Y $24 >P $ff3f $e2 >M $ff40 $11 >M $ff41 $b0 >M
T{ op PC S A X Y P -> $ff41 $ff $fb $8e $76 $24 }T T{ $ff3f M $ff40 M $ff41 M -> $e2 $11 $b0 }T
$0c54 >PC $12 >S $80 >A $af >X $37 >Y $65 >P $0c54 $e2 >M $0c55 $73 >M $0c56 $17 >M
T{ op PC S A X Y P -> $0c56 $12 $80 $af $37 $65 }T T{ $0c54 M $0c55 M $0c56 M -> $e2 $73 $17 }T
$79a6 >PC $a2 >S $0c >A $2e >X $9c >Y $24 >P $79a6 $e2 >M $79a7 $63 >M $79a8 $fc >M
T{ op PC S A X Y P -> $79a8 $a2 $0c $2e $9c $24 }T T{ $79a6 M $79a7 M $79a8 M -> $e2 $63 $fc }T
$a4cd >PC $3d >S $a2 >A $18 >X $69 >Y $e4 >P $a4cd $e2 >M $a4ce $c8 >M $a4cf $e8 >M
T{ op PC S A X Y P -> $a4cf $3d $a2 $18 $69 $e4 }T T{ $a4cd M $a4ce M $a4cf M -> $e2 $c8 $e8 }T
$3c2a >PC $3f >S $30 >A $ab >X $3c >Y $61 >P $3c2a $e2 >M $3c2b $a3 >M $3c2c $af >M
T{ op PC S A X Y P -> $3c2c $3f $30 $ab $3c $61 }T T{ $3c2a M $3c2b M $3c2c M -> $e2 $a3 $af }T
$891c >PC $c2 >S $b1 >A $7e >X $17 >Y $26 >P $891c $e2 >M $891d $9a >M $891e $c0 >M
T{ op PC S A X Y P -> $891e $c2 $b1 $7e $17 $26 }T T{ $891c M $891d M $891e M -> $e2 $9a $c0 }T
$a1b1 >PC $8b >S $57 >A $14 >X $97 >Y $e0 >P $a1b1 $e2 >M $a1b2 $32 >M $a1b3 $d1 >M
T{ op PC S A X Y P -> $a1b3 $8b $57 $14 $97 $e0 }T T{ $a1b1 M $a1b2 M $a1b3 M -> $e2 $32 $d1 }T
$fbe8 >PC $c3 >S $44 >A $2c >X $61 >Y $65 >P $fbe8 $e2 >M $fbe9 $09 >M $fbea $bc >M
T{ op PC S A X Y P -> $fbea $c3 $44 $2c $61 $65 }T T{ $fbe8 M $fbe9 M $fbea M -> $e2 $09 $bc }T
( e3 )
$3dcf >PC $30 >S $14 >A $6c >X $c7 >Y $64 >P $3dcf $e3 >M $3dd0 $2a >M $3dd1 $bc >M
T{ op PC S A X Y P -> $3dd0 $30 $14 $6c $c7 $64 }T T{ $3dcf M $3dd0 M $3dd1 M -> $e3 $2a $bc }T
$cad9 >PC $a3 >S $ee >A $21 >X $65 >Y $e7 >P $cad9 $e3 >M $cada $51 >M $cadb $1f >M
T{ op PC S A X Y P -> $cada $a3 $ee $21 $65 $e7 }T T{ $cad9 M $cada M $cadb M -> $e3 $51 $1f }T
$bc9a >PC $99 >S $21 >A $f2 >X $ec >Y $e0 >P $bc9a $e3 >M $bc9b $7d >M $bc9c $6b >M
T{ op PC S A X Y P -> $bc9b $99 $21 $f2 $ec $e0 }T T{ $bc9a M $bc9b M $bc9c M -> $e3 $7d $6b }T
$d1aa >PC $fd >S $78 >A $86 >X $ef >Y $27 >P $d1aa $e3 >M $d1ab $db >M $d1ac $41 >M
T{ op PC S A X Y P -> $d1ab $fd $78 $86 $ef $27 }T T{ $d1aa M $d1ab M $d1ac M -> $e3 $db $41 }T
$6660 >PC $e6 >S $b5 >A $28 >X $97 >Y $e6 >P $6660 $e3 >M $6661 $c3 >M $6662 $3a >M
T{ op PC S A X Y P -> $6661 $e6 $b5 $28 $97 $e6 }T T{ $6660 M $6661 M $6662 M -> $e3 $c3 $3a }T
$3a41 >PC $e5 >S $cb >A $93 >X $cd >Y $66 >P $3a41 $e3 >M $3a42 $ca >M $3a43 $f2 >M
T{ op PC S A X Y P -> $3a42 $e5 $cb $93 $cd $66 }T T{ $3a41 M $3a42 M $3a43 M -> $e3 $ca $f2 }T
$d0ee >PC $6f >S $d0 >A $42 >X $53 >Y $e4 >P $d0ee $e3 >M $d0ef $74 >M $d0f0 $04 >M
T{ op PC S A X Y P -> $d0ef $6f $d0 $42 $53 $e4 }T T{ $d0ee M $d0ef M $d0f0 M -> $e3 $74 $04 }T
$28d4 >PC $26 >S $3b >A $cf >X $bd >Y $a6 >P $28d4 $e3 >M $28d5 $12 >M $28d6 $d3 >M
T{ op PC S A X Y P -> $28d5 $26 $3b $cf $bd $a6 }T T{ $28d4 M $28d5 M $28d6 M -> $e3 $12 $d3 }T
$5be4 >PC $1f >S $e6 >A $df >X $96 >Y $63 >P $5be4 $e3 >M $5be5 $c2 >M $5be6 $4e >M
T{ op PC S A X Y P -> $5be5 $1f $e6 $df $96 $63 }T T{ $5be4 M $5be5 M $5be6 M -> $e3 $c2 $4e }T
$337b >PC $76 >S $15 >A $65 >X $eb >Y $a4 >P $337b $e3 >M $337c $84 >M $337d $d4 >M
T{ op PC S A X Y P -> $337c $76 $15 $65 $eb $a4 }T T{ $337b M $337c M $337d M -> $e3 $84 $d4 }T
$3a05 >PC $dc >S $40 >A $fb >X $c4 >Y $e2 >P $3a05 $e3 >M $3a06 $d6 >M $3a07 $55 >M
T{ op PC S A X Y P -> $3a06 $dc $40 $fb $c4 $e2 }T T{ $3a05 M $3a06 M $3a07 M -> $e3 $d6 $55 }T
$1cd9 >PC $c5 >S $f7 >A $d2 >X $18 >Y $27 >P $1cd9 $e3 >M $1cda $bc >M $1cdb $db >M
T{ op PC S A X Y P -> $1cda $c5 $f7 $d2 $18 $27 }T T{ $1cd9 M $1cda M $1cdb M -> $e3 $bc $db }T
$851c >PC $d6 >S $8e >A $e2 >X $40 >Y $a2 >P $851c $e3 >M $851d $6f >M $851e $3c >M
T{ op PC S A X Y P -> $851d $d6 $8e $e2 $40 $a2 }T T{ $851c M $851d M $851e M -> $e3 $6f $3c }T
$062b >PC $da >S $15 >A $80 >X $7f >Y $a1 >P $062b $e3 >M $062c $50 >M $062d $8f >M
T{ op PC S A X Y P -> $062c $da $15 $80 $7f $a1 }T T{ $062b M $062c M $062d M -> $e3 $50 $8f }T
$261e >PC $68 >S $56 >A $29 >X $b5 >Y $e6 >P $261e $e3 >M $261f $c6 >M $2620 $99 >M
T{ op PC S A X Y P -> $261f $68 $56 $29 $b5 $e6 }T T{ $261e M $261f M $2620 M -> $e3 $c6 $99 }T
$db3b >PC $13 >S $05 >A $5c >X $ad >Y $a1 >P $db3b $e3 >M $db3c $d0 >M $db3d $df >M
T{ op PC S A X Y P -> $db3c $13 $05 $5c $ad $a1 }T T{ $db3b M $db3c M $db3d M -> $e3 $d0 $df }T
( e4 )
$370f >PC $f0 >S $57 >A $22 >X $f4 >Y $a3 >P $0028 $23 >M $370f $e4 >M $3710 $28 >M $3711 $a9 >M
T{ op PC S A X Y P -> $3711 $f0 $57 $22 $f4 $a0 }T T{ $0028 M $370f M $3710 M $3711 M -> $23 $e4 $28 $a9 }T
$c19b >PC $69 >S $13 >A $86 >X $c0 >Y $27 >P $00be $58 >M $c19b $e4 >M $c19c $be >M $c19d $8e >M
T{ op PC S A X Y P -> $c19d $69 $13 $86 $c0 $25 }T T{ $00be M $c19b M $c19c M $c19d M -> $58 $e4 $be $8e }T
$0ac9 >PC $c4 >S $c8 >A $bd >X $90 >Y $27 >P $0042 $bd >M $0ac9 $e4 >M $0aca $42 >M $0acb $2a >M
T{ op PC S A X Y P -> $0acb $c4 $c8 $bd $90 $27 }T T{ $0042 M $0ac9 M $0aca M $0acb M -> $bd $e4 $42 $2a }T
$e961 >PC $14 >S $c8 >A $a7 >X $a8 >Y $a1 >P $00ae $a2 >M $e961 $e4 >M $e962 $ae >M $e963 $f4 >M
T{ op PC S A X Y P -> $e963 $14 $c8 $a7 $a8 $21 }T T{ $00ae M $e961 M $e962 M $e963 M -> $a2 $e4 $ae $f4 }T
$7da5 >PC $46 >S $e5 >A $ec >X $4c >Y $26 >P $00e9 $db >M $7da5 $e4 >M $7da6 $e9 >M $7da7 $c6 >M
T{ op PC S A X Y P -> $7da7 $46 $e5 $ec $4c $25 }T T{ $00e9 M $7da5 M $7da6 M $7da7 M -> $db $e4 $e9 $c6 }T
$f3e2 >PC $61 >S $99 >A $1e >X $32 >Y $e6 >P $000f $8c >M $f3e2 $e4 >M $f3e3 $0f >M $f3e4 $b0 >M
T{ op PC S A X Y P -> $f3e4 $61 $99 $1e $32 $e4 }T T{ $000f M $f3e2 M $f3e3 M $f3e4 M -> $8c $e4 $0f $b0 }T
$750f >PC $c0 >S $28 >A $e1 >X $38 >Y $67 >P $0011 $37 >M $750f $e4 >M $7510 $11 >M $7511 $ef >M
T{ op PC S A X Y P -> $7511 $c0 $28 $e1 $38 $e5 }T T{ $0011 M $750f M $7510 M $7511 M -> $37 $e4 $11 $ef }T
$6abc >PC $6c >S $33 >A $fc >X $6c >Y $64 >P $0001 $1a >M $6abc $e4 >M $6abd $01 >M $6abe $26 >M
T{ op PC S A X Y P -> $6abe $6c $33 $fc $6c $e5 }T T{ $0001 M $6abc M $6abd M $6abe M -> $1a $e4 $01 $26 }T
$41c8 >PC $4b >S $61 >A $39 >X $10 >Y $62 >P $0025 $9b >M $41c8 $e4 >M $41c9 $25 >M $41ca $18 >M
T{ op PC S A X Y P -> $41ca $4b $61 $39 $10 $e0 }T T{ $0025 M $41c8 M $41c9 M $41ca M -> $9b $e4 $25 $18 }T
$b467 >PC $47 >S $b9 >A $99 >X $64 >Y $a3 >P $00c9 $42 >M $b467 $e4 >M $b468 $c9 >M $b469 $da >M
T{ op PC S A X Y P -> $b469 $47 $b9 $99 $64 $21 }T T{ $00c9 M $b467 M $b468 M $b469 M -> $42 $e4 $c9 $da }T
$641e >PC $45 >S $63 >A $62 >X $e2 >Y $e2 >P $006a $b2 >M $641e $e4 >M $641f $6a >M $6420 $8b >M
T{ op PC S A X Y P -> $6420 $45 $63 $62 $e2 $e0 }T T{ $006a M $641e M $641f M $6420 M -> $b2 $e4 $6a $8b }T
$ace4 >PC $ca >S $1f >A $77 >X $a2 >Y $e3 >P $0056 $ac >M $ace4 $e4 >M $ace5 $56 >M $ace6 $56 >M
T{ op PC S A X Y P -> $ace6 $ca $1f $77 $a2 $e0 }T T{ $0056 M $ace4 M $ace5 M $ace6 M -> $ac $e4 $56 $56 }T
$b9d6 >PC $31 >S $b3 >A $05 >X $76 >Y $a1 >P $00a0 $0e >M $b9d6 $e4 >M $b9d7 $a0 >M $b9d8 $41 >M
T{ op PC S A X Y P -> $b9d8 $31 $b3 $05 $76 $a0 }T T{ $00a0 M $b9d6 M $b9d7 M $b9d8 M -> $0e $e4 $a0 $41 }T
$b400 >PC $62 >S $8d >A $47 >X $2d >Y $24 >P $00ff $df >M $b400 $e4 >M $b401 $ff >M $b402 $36 >M
T{ op PC S A X Y P -> $b402 $62 $8d $47 $2d $24 }T T{ $00ff M $b400 M $b401 M $b402 M -> $df $e4 $ff $36 }T
$b4fb >PC $f0 >S $d1 >A $82 >X $45 >Y $63 >P $00f8 $9a >M $b4fb $e4 >M $b4fc $f8 >M $b4fd $a0 >M
T{ op PC S A X Y P -> $b4fd $f0 $d1 $82 $45 $e0 }T T{ $00f8 M $b4fb M $b4fc M $b4fd M -> $9a $e4 $f8 $a0 }T
$d6ce >PC $f7 >S $e1 >A $e7 >X $de >Y $a6 >P $00f1 $d5 >M $d6ce $e4 >M $d6cf $f1 >M $d6d0 $b9 >M
T{ op PC S A X Y P -> $d6d0 $f7 $e1 $e7 $de $25 }T T{ $00f1 M $d6ce M $d6cf M $d6d0 M -> $d5 $e4 $f1 $b9 }T
( e5 )
$a19f >PC $ea >S $75 >A $26 >X $c5 >Y $71 >P $007d $6e >M $a19f $e5 >M $a1a0 $7d >M
T{ op PC S A X Y P -> $a1a1 $ea $07 $26 $c5 $31 }T T{ $007d M $a19f M $a1a0 M -> $6e $e5 $7d }T
$b076 >PC $67 >S $01 >A $33 >X $55 >Y $b7 >P $0056 $37 >M $b076 $e5 >M $b077 $56 >M
T{ op PC S A X Y P -> $b078 $67 $ca $33 $55 $b4 }T T{ $0056 M $b076 M $b077 M -> $37 $e5 $56 }T
$3085 >PC $b8 >S $3c >A $43 >X $e5 >Y $f1 >P $0022 $54 >M $3085 $e5 >M $3086 $22 >M
T{ op PC S A X Y P -> $3087 $b8 $e8 $43 $e5 $b0 }T T{ $0022 M $3085 M $3086 M -> $54 $e5 $22 }T
$4afc >PC $1a >S $94 >A $0c >X $bf >Y $f2 >P $0055 $79 >M $4afc $e5 >M $4afd $55 >M
T{ op PC S A X Y P -> $4afe $1a $1a $0c $bf $71 }T T{ $0055 M $4afc M $4afd M -> $79 $e5 $55 }T
$ffec >PC $17 >S $98 >A $81 >X $8d >Y $b7 >P $0002 $7f >M $ffec $e5 >M $ffed $02 >M
T{ op PC S A X Y P -> $ffee $17 $19 $81 $8d $75 }T T{ $0002 M $ffec M $ffed M -> $7f $e5 $02 }T
$fb83 >PC $17 >S $6a >A $b8 >X $1a >Y $37 >P $00d1 $06 >M $fb83 $e5 >M $fb84 $d1 >M
T{ op PC S A X Y P -> $fb85 $17 $64 $b8 $1a $35 }T T{ $00d1 M $fb83 M $fb84 M -> $06 $e5 $d1 }T
$429c >PC $73 >S $e6 >A $c4 >X $c8 >Y $70 >P $00ca $b0 >M $429c $e5 >M $429d $ca >M
T{ op PC S A X Y P -> $429e $73 $35 $c4 $c8 $31 }T T{ $00ca M $429c M $429d M -> $b0 $e5 $ca }T
$4024 >PC $82 >S $48 >A $e4 >X $af >Y $f2 >P $007e $fe >M $4024 $e5 >M $4025 $7e >M
T{ op PC S A X Y P -> $4026 $82 $49 $e4 $af $30 }T T{ $007e M $4024 M $4025 M -> $fe $e5 $7e }T
$598d >PC $2a >S $2b >A $37 >X $b0 >Y $b4 >P $0054 $1d >M $598d $e5 >M $598e $54 >M
T{ op PC S A X Y P -> $598f $2a $0d $37 $b0 $35 }T T{ $0054 M $598d M $598e M -> $1d $e5 $54 }T
$2c6c >PC $b0 >S $c8 >A $98 >X $e9 >Y $33 >P $0084 $bb >M $2c6c $e5 >M $2c6d $84 >M
T{ op PC S A X Y P -> $2c6e $b0 $0d $98 $e9 $31 }T T{ $0084 M $2c6c M $2c6d M -> $bb $e5 $84 }T
$39ca >PC $ef >S $c1 >A $8e >X $0c >Y $b0 >P $0016 $4c >M $39ca $e5 >M $39cb $16 >M
T{ op PC S A X Y P -> $39cc $ef $74 $8e $0c $71 }T T{ $0016 M $39ca M $39cb M -> $4c $e5 $16 }T
$278c >PC $2e >S $a2 >A $5d >X $e2 >Y $f3 >P $00d6 $7b >M $278c $e5 >M $278d $d6 >M
T{ op PC S A X Y P -> $278e $2e $27 $5d $e2 $71 }T T{ $00d6 M $278c M $278d M -> $7b $e5 $d6 }T
$2e0f >PC $34 >S $38 >A $24 >X $d0 >Y $f3 >P $0028 $a0 >M $2e0f $e5 >M $2e10 $28 >M
T{ op PC S A X Y P -> $2e11 $34 $98 $24 $d0 $f0 }T T{ $0028 M $2e0f M $2e10 M -> $a0 $e5 $28 }T
$3438 >PC $16 >S $88 >A $97 >X $fc >Y $f3 >P $008a $2f >M $3438 $e5 >M $3439 $8a >M
T{ op PC S A X Y P -> $343a $16 $59 $97 $fc $71 }T T{ $008a M $3438 M $3439 M -> $2f $e5 $8a }T
$d933 >PC $2e >S $3c >A $dd >X $c2 >Y $37 >P $005e $3c >M $d933 $e5 >M $d934 $5e >M
T{ op PC S A X Y P -> $d935 $2e $00 $dd $c2 $37 }T T{ $005e M $d933 M $d934 M -> $3c $e5 $5e }T
$f69c >PC $6f >S $34 >A $d6 >X $f0 >Y $b5 >P $0033 $d2 >M $f69c $e5 >M $f69d $33 >M
T{ op PC S A X Y P -> $f69e $6f $62 $d6 $f0 $34 }T T{ $0033 M $f69c M $f69d M -> $d2 $e5 $33 }T
( e6 )
$032e >PC $96 >S $32 >A $a6 >X $62 >Y $67 >P $0068 $00 >M $032e $e6 >M $032f $68 >M $0330 $1e >M
T{ op PC S A X Y P -> $0330 $96 $32 $a6 $62 $65 }T T{ $0068 M $032e M $032f M $0330 M -> $01 $e6 $68 $1e }T
$d1ad >PC $56 >S $4b >A $23 >X $a4 >Y $e1 >P $0012 $1f >M $d1ad $e6 >M $d1ae $12 >M $d1af $70 >M
T{ op PC S A X Y P -> $d1af $56 $4b $23 $a4 $61 }T T{ $0012 M $d1ad M $d1ae M $d1af M -> $20 $e6 $12 $70 }T
$6747 >PC $5c >S $2b >A $c4 >X $ed >Y $a5 >P $003c $71 >M $6747 $e6 >M $6748 $3c >M $6749 $ae >M
T{ op PC S A X Y P -> $6749 $5c $2b $c4 $ed $25 }T T{ $003c M $6747 M $6748 M $6749 M -> $72 $e6 $3c $ae }T
$5b06 >PC $39 >S $03 >A $da >X $c2 >Y $e4 >P $00fc $0b >M $5b06 $e6 >M $5b07 $fc >M $5b08 $bc >M
T{ op PC S A X Y P -> $5b08 $39 $03 $da $c2 $64 }T T{ $00fc M $5b06 M $5b07 M $5b08 M -> $0c $e6 $fc $bc }T
$9150 >PC $50 >S $1a >A $44 >X $c5 >Y $20 >P $009c $3f >M $9150 $e6 >M $9151 $9c >M $9152 $d4 >M
T{ op PC S A X Y P -> $9152 $50 $1a $44 $c5 $20 }T T{ $009c M $9150 M $9151 M $9152 M -> $40 $e6 $9c $d4 }T
$57d7 >PC $4c >S $81 >A $ab >X $1b >Y $e5 >P $0059 $a0 >M $57d7 $e6 >M $57d8 $59 >M $57d9 $e7 >M
T{ op PC S A X Y P -> $57d9 $4c $81 $ab $1b $e5 }T T{ $0059 M $57d7 M $57d8 M $57d9 M -> $a1 $e6 $59 $e7 }T
$0bc0 >PC $a1 >S $fb >A $72 >X $5a >Y $24 >P $00ed $56 >M $0bc0 $e6 >M $0bc1 $ed >M $0bc2 $cf >M
T{ op PC S A X Y P -> $0bc2 $a1 $fb $72 $5a $24 }T T{ $00ed M $0bc0 M $0bc1 M $0bc2 M -> $57 $e6 $ed $cf }T
$ea7d >PC $ad >S $9f >A $0e >X $78 >Y $26 >P $000a $e7 >M $ea7d $e6 >M $ea7e $0a >M $ea7f $63 >M
T{ op PC S A X Y P -> $ea7f $ad $9f $0e $78 $a4 }T T{ $000a M $ea7d M $ea7e M $ea7f M -> $e8 $e6 $0a $63 }T
$7fdf >PC $3c >S $bf >A $6c >X $ff >Y $61 >P $0038 $db >M $7fdf $e6 >M $7fe0 $38 >M $7fe1 $fe >M
T{ op PC S A X Y P -> $7fe1 $3c $bf $6c $ff $e1 }T T{ $0038 M $7fdf M $7fe0 M $7fe1 M -> $dc $e6 $38 $fe }T
$fb3f >PC $47 >S $1c >A $5a >X $77 >Y $a5 >P $00a0 $11 >M $fb3f $e6 >M $fb40 $a0 >M $fb41 $41 >M
T{ op PC S A X Y P -> $fb41 $47 $1c $5a $77 $25 }T T{ $00a0 M $fb3f M $fb40 M $fb41 M -> $12 $e6 $a0 $41 }T
$913c >PC $bf >S $45 >A $37 >X $61 >Y $24 >P $00af $81 >M $913c $e6 >M $913d $af >M $913e $9c >M
T{ op PC S A X Y P -> $913e $bf $45 $37 $61 $a4 }T T{ $00af M $913c M $913d M $913e M -> $82 $e6 $af $9c }T
$c727 >PC $bb >S $f9 >A $90 >X $db >Y $e5 >P $00ab $3b >M $c727 $e6 >M $c728 $ab >M $c729 $ab >M
T{ op PC S A X Y P -> $c729 $bb $f9 $90 $db $65 }T T{ $00ab M $c727 M $c728 M $c729 M -> $3c $e6 $ab $ab }T
$ddf7 >PC $1f >S $7e >A $b9 >X $bf >Y $24 >P $008f $40 >M $ddf7 $e6 >M $ddf8 $8f >M $ddf9 $b4 >M
T{ op PC S A X Y P -> $ddf9 $1f $7e $b9 $bf $24 }T T{ $008f M $ddf7 M $ddf8 M $ddf9 M -> $41 $e6 $8f $b4 }T
$8b97 >PC $24 >S $cb >A $a5 >X $f3 >Y $e5 >P $00e1 $a4 >M $8b97 $e6 >M $8b98 $e1 >M $8b99 $35 >M
T{ op PC S A X Y P -> $8b99 $24 $cb $a5 $f3 $e5 }T T{ $00e1 M $8b97 M $8b98 M $8b99 M -> $a5 $e6 $e1 $35 }T
$28e0 >PC $d0 >S $d4 >A $9c >X $31 >Y $e4 >P $0071 $e3 >M $28e0 $e6 >M $28e1 $71 >M $28e2 $55 >M
T{ op PC S A X Y P -> $28e2 $d0 $d4 $9c $31 $e4 }T T{ $0071 M $28e0 M $28e1 M $28e2 M -> $e4 $e6 $71 $55 }T
$5a9b >PC $f4 >S $72 >A $83 >X $fb >Y $22 >P $000f $85 >M $5a9b $e6 >M $5a9c $0f >M $5a9d $d7 >M
T{ op PC S A X Y P -> $5a9d $f4 $72 $83 $fb $a0 }T T{ $000f M $5a9b M $5a9c M $5a9d M -> $86 $e6 $0f $d7 }T
( e7 )
$b72d >PC $4a >S $3e >A $df >X $ce >Y $a6 >P $0006 $3b >M $b72d $e7 >M $b72e $06 >M $b72f $ad >M
T{ op PC S A X Y P -> $b72f $4a $3e $df $ce $a6 }T T{ $0006 M $b72d M $b72e M $b72f M -> $7b $e7 $06 $ad }T
$d5a6 >PC $50 >S $86 >A $b9 >X $1a >Y $24 >P $0088 $f2 >M $d5a6 $e7 >M $d5a7 $88 >M $d5a8 $28 >M
T{ op PC S A X Y P -> $d5a8 $50 $86 $b9 $1a $24 }T T{ $0088 M $d5a6 M $d5a7 M $d5a8 M -> $f2 $e7 $88 $28 }T
$9589 >PC $cf >S $d9 >A $35 >X $46 >Y $e4 >P $00f0 $83 >M $9589 $e7 >M $958a $f0 >M $958b $88 >M
T{ op PC S A X Y P -> $958b $cf $d9 $35 $46 $e4 }T T{ $00f0 M $9589 M $958a M $958b M -> $c3 $e7 $f0 $88 }T
$5c9d >PC $d1 >S $5a >A $31 >X $27 >Y $a0 >P $0011 $08 >M $5c9d $e7 >M $5c9e $11 >M $5c9f $8a >M
T{ op PC S A X Y P -> $5c9f $d1 $5a $31 $27 $a0 }T T{ $0011 M $5c9d M $5c9e M $5c9f M -> $48 $e7 $11 $8a }T
$0ead >PC $1e >S $dc >A $20 >X $24 >Y $a0 >P $00bc $d9 >M $0ead $e7 >M $0eae $bc >M $0eaf $18 >M
T{ op PC S A X Y P -> $0eaf $1e $dc $20 $24 $a0 }T T{ $00bc M $0ead M $0eae M $0eaf M -> $d9 $e7 $bc $18 }T
$d5b4 >PC $04 >S $eb >A $95 >X $f5 >Y $67 >P $0040 $3a >M $d5b4 $e7 >M $d5b5 $40 >M $d5b6 $95 >M
T{ op PC S A X Y P -> $d5b6 $04 $eb $95 $f5 $67 }T T{ $0040 M $d5b4 M $d5b5 M $d5b6 M -> $7a $e7 $40 $95 }T
$4166 >PC $33 >S $5f >A $14 >X $ab >Y $23 >P $000d $ef >M $4166 $e7 >M $4167 $0d >M $4168 $77 >M
T{ op PC S A X Y P -> $4168 $33 $5f $14 $ab $23 }T T{ $000d M $4166 M $4167 M $4168 M -> $ef $e7 $0d $77 }T
$08b3 >PC $26 >S $88 >A $a0 >X $8b >Y $60 >P $006a $08 >M $08b3 $e7 >M $08b4 $6a >M $08b5 $a1 >M
T{ op PC S A X Y P -> $08b5 $26 $88 $a0 $8b $60 }T T{ $006a M $08b3 M $08b4 M $08b5 M -> $48 $e7 $6a $a1 }T
$5157 >PC $73 >S $e9 >A $98 >X $66 >Y $a6 >P $003b $47 >M $5157 $e7 >M $5158 $3b >M $5159 $93 >M
T{ op PC S A X Y P -> $5159 $73 $e9 $98 $66 $a6 }T T{ $003b M $5157 M $5158 M $5159 M -> $47 $e7 $3b $93 }T
$1cdd >PC $59 >S $65 >A $2b >X $f7 >Y $a2 >P $00da $bc >M $1cdd $e7 >M $1cde $da >M $1cdf $1e >M
T{ op PC S A X Y P -> $1cdf $59 $65 $2b $f7 $a2 }T T{ $00da M $1cdd M $1cde M $1cdf M -> $fc $e7 $da $1e }T
$4ea9 >PC $16 >S $4b >A $51 >X $39 >Y $e1 >P $005b $7f >M $4ea9 $e7 >M $4eaa $5b >M $4eab $6c >M
T{ op PC S A X Y P -> $4eab $16 $4b $51 $39 $e1 }T T{ $005b M $4ea9 M $4eaa M $4eab M -> $7f $e7 $5b $6c }T
$b7b2 >PC $0d >S $26 >A $66 >X $b9 >Y $62 >P $0051 $b4 >M $b7b2 $e7 >M $b7b3 $51 >M $b7b4 $de >M
T{ op PC S A X Y P -> $b7b4 $0d $26 $66 $b9 $62 }T T{ $0051 M $b7b2 M $b7b3 M $b7b4 M -> $f4 $e7 $51 $de }T
$3e3c >PC $a5 >S $ba >A $8c >X $dd >Y $e0 >P $00b8 $2e >M $3e3c $e7 >M $3e3d $b8 >M $3e3e $a7 >M
T{ op PC S A X Y P -> $3e3e $a5 $ba $8c $dd $e0 }T T{ $00b8 M $3e3c M $3e3d M $3e3e M -> $6e $e7 $b8 $a7 }T
$50b9 >PC $f9 >S $9b >A $d2 >X $f2 >Y $63 >P $0047 $7b >M $50b9 $e7 >M $50ba $47 >M $50bb $dd >M
T{ op PC S A X Y P -> $50bb $f9 $9b $d2 $f2 $63 }T T{ $0047 M $50b9 M $50ba M $50bb M -> $7b $e7 $47 $dd }T
$604a >PC $6d >S $22 >A $70 >X $04 >Y $20 >P $00da $7e >M $604a $e7 >M $604b $da >M $604c $c3 >M
T{ op PC S A X Y P -> $604c $6d $22 $70 $04 $20 }T T{ $00da M $604a M $604b M $604c M -> $7e $e7 $da $c3 }T
$8994 >PC $ee >S $1d >A $eb >X $e6 >Y $21 >P $0074 $f1 >M $8994 $e7 >M $8995 $74 >M $8996 $dd >M
T{ op PC S A X Y P -> $8996 $ee $1d $eb $e6 $21 }T T{ $0074 M $8994 M $8995 M $8996 M -> $f1 $e7 $74 $dd }T
( e8 )
$dbee >PC $81 >S $3c >A $04 >X $98 >Y $67 >P $dbee $e8 >M $dbef $cc >M $dbf0 $40 >M
T{ op PC S A X Y P -> $dbef $81 $3c $05 $98 $65 }T T{ $dbee M $dbef M $dbf0 M -> $e8 $cc $40 }T
$ab03 >PC $b5 >S $1b >A $e9 >X $10 >Y $24 >P $ab03 $e8 >M $ab04 $dd >M $ab05 $a4 >M
T{ op PC S A X Y P -> $ab04 $b5 $1b $ea $10 $a4 }T T{ $ab03 M $ab04 M $ab05 M -> $e8 $dd $a4 }T
$17fa >PC $84 >S $62 >A $8a >X $29 >Y $23 >P $17fa $e8 >M $17fb $e8 >M $17fc $9e >M
T{ op PC S A X Y P -> $17fb $84 $62 $8b $29 $a1 }T T{ $17fa M $17fb M $17fc M -> $e8 $e8 $9e }T
$fb77 >PC $46 >S $c9 >A $2b >X $a4 >Y $e4 >P $fb77 $e8 >M $fb78 $b2 >M $fb79 $34 >M
T{ op PC S A X Y P -> $fb78 $46 $c9 $2c $a4 $64 }T T{ $fb77 M $fb78 M $fb79 M -> $e8 $b2 $34 }T
$02ad >PC $d0 >S $c7 >A $ca >X $8c >Y $27 >P $02ad $e8 >M $02ae $18 >M $02af $f9 >M
T{ op PC S A X Y P -> $02ae $d0 $c7 $cb $8c $a5 }T T{ $02ad M $02ae M $02af M -> $e8 $18 $f9 }T
$32ac >PC $ff >S $8b >A $af >X $99 >Y $e7 >P $32ac $e8 >M $32ad $09 >M $32ae $64 >M
T{ op PC S A X Y P -> $32ad $ff $8b $b0 $99 $e5 }T T{ $32ac M $32ad M $32ae M -> $e8 $09 $64 }T
$a2c3 >PC $8b >S $97 >A $4a >X $87 >Y $21 >P $a2c3 $e8 >M $a2c4 $40 >M $a2c5 $d0 >M
T{ op PC S A X Y P -> $a2c4 $8b $97 $4b $87 $21 }T T{ $a2c3 M $a2c4 M $a2c5 M -> $e8 $40 $d0 }T
$1607 >PC $80 >S $43 >A $06 >X $ec >Y $62 >P $1607 $e8 >M $1608 $71 >M $1609 $47 >M
T{ op PC S A X Y P -> $1608 $80 $43 $07 $ec $60 }T T{ $1607 M $1608 M $1609 M -> $e8 $71 $47 }T
$7197 >PC $58 >S $11 >A $09 >X $b2 >Y $66 >P $7197 $e8 >M $7198 $50 >M $7199 $b1 >M
T{ op PC S A X Y P -> $7198 $58 $11 $0a $b2 $64 }T T{ $7197 M $7198 M $7199 M -> $e8 $50 $b1 }T
$5d5c >PC $cd >S $cc >A $d1 >X $e8 >Y $64 >P $5d5c $e8 >M $5d5d $7d >M $5d5e $92 >M
T{ op PC S A X Y P -> $5d5d $cd $cc $d2 $e8 $e4 }T T{ $5d5c M $5d5d M $5d5e M -> $e8 $7d $92 }T
$5edf >PC $30 >S $67 >A $8c >X $91 >Y $e2 >P $5edf $e8 >M $5ee0 $4f >M $5ee1 $d2 >M
T{ op PC S A X Y P -> $5ee0 $30 $67 $8d $91 $e0 }T T{ $5edf M $5ee0 M $5ee1 M -> $e8 $4f $d2 }T
$8a2c >PC $3d >S $c7 >A $cd >X $76 >Y $24 >P $8a2c $e8 >M $8a2d $8e >M $8a2e $37 >M
T{ op PC S A X Y P -> $8a2d $3d $c7 $ce $76 $a4 }T T{ $8a2c M $8a2d M $8a2e M -> $e8 $8e $37 }T
$2cc9 >PC $0d >S $76 >A $f4 >X $ed >Y $a3 >P $2cc9 $e8 >M $2cca $05 >M $2ccb $d8 >M
T{ op PC S A X Y P -> $2cca $0d $76 $f5 $ed $a1 }T T{ $2cc9 M $2cca M $2ccb M -> $e8 $05 $d8 }T
$f273 >PC $96 >S $b1 >A $4d >X $53 >Y $63 >P $f273 $e8 >M $f274 $26 >M $f275 $39 >M
T{ op PC S A X Y P -> $f274 $96 $b1 $4e $53 $61 }T T{ $f273 M $f274 M $f275 M -> $e8 $26 $39 }T
$e180 >PC $25 >S $14 >A $49 >X $88 >Y $21 >P $e180 $e8 >M $e181 $0a >M $e182 $4e >M
T{ op PC S A X Y P -> $e181 $25 $14 $4a $88 $21 }T T{ $e180 M $e181 M $e182 M -> $e8 $0a $4e }T
$504f >PC $fe >S $c4 >A $b1 >X $34 >Y $62 >P $504f $e8 >M $5050 $1e >M $5051 $92 >M
T{ op PC S A X Y P -> $5050 $fe $c4 $b2 $34 $e0 }T T{ $504f M $5050 M $5051 M -> $e8 $1e $92 }T
( e9 )
$2b38 >PC $b4 >S $1b >A $89 >X $a0 >Y $f5 >P $2b38 $e9 >M $2b39 $51 >M
T{ op PC S A X Y P -> $2b3a $b4 $ca $89 $a0 $b4 }T T{ $2b38 M $2b39 M -> $e9 $51 }T
$e9b5 >PC $92 >S $85 >A $8c >X $d5 >Y $34 >P $e9b5 $e9 >M $e9b6 $0f >M
T{ op PC S A X Y P -> $e9b7 $92 $75 $8c $d5 $75 }T T{ $e9b5 M $e9b6 M -> $e9 $0f }T
$e35d >PC $af >S $cd >A $98 >X $b3 >Y $72 >P $e35d $e9 >M $e35e $f2 >M
T{ op PC S A X Y P -> $e35f $af $da $98 $b3 $b0 }T T{ $e35d M $e35e M -> $e9 $f2 }T
$e49d >PC $06 >S $21 >A $e2 >X $1f >Y $b2 >P $e49d $e9 >M $e49e $6c >M
T{ op PC S A X Y P -> $e49f $06 $b4 $e2 $1f $b0 }T T{ $e49d M $e49e M -> $e9 $6c }T
$d242 >PC $6f >S $96 >A $df >X $ce >Y $b1 >P $d242 $e9 >M $d243 $17 >M
T{ op PC S A X Y P -> $d244 $6f $7f $df $ce $71 }T T{ $d242 M $d243 M -> $e9 $17 }T
$5db9 >PC $c1 >S $14 >A $52 >X $8a >Y $f5 >P $5db9 $e9 >M $5dba $74 >M
T{ op PC S A X Y P -> $5dbb $c1 $a0 $52 $8a $b4 }T T{ $5db9 M $5dba M -> $e9 $74 }T
$3ae9 >PC $c2 >S $b7 >A $9b >X $29 >Y $f3 >P $3ae9 $e9 >M $3aea $21 >M
T{ op PC S A X Y P -> $3aeb $c2 $96 $9b $29 $b1 }T T{ $3ae9 M $3aea M -> $e9 $21 }T
$56be >PC $8c >S $8a >A $c0 >X $68 >Y $76 >P $56be $e9 >M $56bf $12 >M
T{ op PC S A X Y P -> $56c0 $8c $77 $c0 $68 $75 }T T{ $56be M $56bf M -> $e9 $12 }T
$018c >PC $69 >S $c9 >A $5f >X $39 >Y $31 >P $018c $e9 >M $018d $4a >M
T{ op PC S A X Y P -> $018e $69 $7f $5f $39 $71 }T T{ $018c M $018d M -> $e9 $4a }T
$0226 >PC $81 >S $90 >A $35 >X $4d >Y $f2 >P $0226 $e9 >M $0227 $d2 >M
T{ op PC S A X Y P -> $0228 $81 $bd $35 $4d $b0 }T T{ $0226 M $0227 M -> $e9 $d2 }T
$221d >PC $de >S $9c >A $15 >X $ed >Y $b0 >P $221d $e9 >M $221e $a8 >M
T{ op PC S A X Y P -> $221f $de $f3 $15 $ed $b0 }T T{ $221d M $221e M -> $e9 $a8 }T
$33c3 >PC $01 >S $2a >A $9f >X $28 >Y $76 >P $33c3 $e9 >M $33c4 $bf >M
T{ op PC S A X Y P -> $33c5 $01 $6a $9f $28 $34 }T T{ $33c3 M $33c4 M -> $e9 $bf }T
$9504 >PC $cf >S $e0 >A $39 >X $de >Y $b6 >P $9504 $e9 >M $9505 $53 >M
T{ op PC S A X Y P -> $9506 $cf $8c $39 $de $b5 }T T{ $9504 M $9505 M -> $e9 $53 }T
$0d4e >PC $51 >S $2a >A $76 >X $92 >Y $34 >P $0d4e $e9 >M $0d4f $87 >M
T{ op PC S A X Y P -> $0d50 $51 $a2 $76 $92 $f4 }T T{ $0d4e M $0d4f M -> $e9 $87 }T
$9817 >PC $9b >S $2c >A $40 >X $33 >Y $72 >P $9817 $e9 >M $9818 $a8 >M
T{ op PC S A X Y P -> $9819 $9b $83 $40 $33 $f0 }T T{ $9817 M $9818 M -> $e9 $a8 }T
$3d0e >PC $60 >S $f9 >A $8c >X $46 >Y $71 >P $3d0e $e9 >M $3d0f $7a >M
T{ op PC S A X Y P -> $3d10 $60 $7f $8c $46 $71 }T T{ $3d0e M $3d0f M -> $e9 $7a }T
( ea )
$e427 >PC $b9 >S $e1 >A $ee >X $34 >Y $63 >P $e427 $ea >M $e428 $2f >M $e429 $65 >M
T{ op PC S A X Y P -> $e428 $b9 $e1 $ee $34 $63 }T T{ $e427 M $e428 M $e429 M -> $ea $2f $65 }T
$774d >PC $e6 >S $4a >A $31 >X $c7 >Y $65 >P $774d $ea >M $774e $95 >M $774f $a4 >M
T{ op PC S A X Y P -> $774e $e6 $4a $31 $c7 $65 }T T{ $774d M $774e M $774f M -> $ea $95 $a4 }T
$c563 >PC $c1 >S $18 >A $0f >X $19 >Y $e0 >P $c563 $ea >M $c564 $e1 >M $c565 $21 >M
T{ op PC S A X Y P -> $c564 $c1 $18 $0f $19 $e0 }T T{ $c563 M $c564 M $c565 M -> $ea $e1 $21 }T
$0cee >PC $9f >S $42 >A $4c >X $83 >Y $e0 >P $0cee $ea >M $0cef $ee >M $0cf0 $13 >M
T{ op PC S A X Y P -> $0cef $9f $42 $4c $83 $e0 }T T{ $0cee M $0cef M $0cf0 M -> $ea $ee $13 }T
$b69c >PC $98 >S $14 >A $2c >X $37 >Y $65 >P $b69c $ea >M $b69d $f1 >M $b69e $2b >M
T{ op PC S A X Y P -> $b69d $98 $14 $2c $37 $65 }T T{ $b69c M $b69d M $b69e M -> $ea $f1 $2b }T
$8c17 >PC $01 >S $cd >A $1d >X $1d >Y $a0 >P $8c17 $ea >M $8c18 $cc >M $8c19 $91 >M
T{ op PC S A X Y P -> $8c18 $01 $cd $1d $1d $a0 }T T{ $8c17 M $8c18 M $8c19 M -> $ea $cc $91 }T
$0409 >PC $84 >S $74 >A $0f >X $57 >Y $a7 >P $0409 $ea >M $040a $47 >M $040b $69 >M
T{ op PC S A X Y P -> $040a $84 $74 $0f $57 $a7 }T T{ $0409 M $040a M $040b M -> $ea $47 $69 }T
$0ef1 >PC $b1 >S $96 >A $a3 >X $4b >Y $e6 >P $0ef1 $ea >M $0ef2 $61 >M $0ef3 $b0 >M
T{ op PC S A X Y P -> $0ef2 $b1 $96 $a3 $4b $e6 }T T{ $0ef1 M $0ef2 M $0ef3 M -> $ea $61 $b0 }T
$4551 >PC $13 >S $02 >A $71 >X $18 >Y $e0 >P $4551 $ea >M $4552 $ed >M $4553 $95 >M
T{ op PC S A X Y P -> $4552 $13 $02 $71 $18 $e0 }T T{ $4551 M $4552 M $4553 M -> $ea $ed $95 }T
$ac9c >PC $41 >S $1b >A $31 >X $36 >Y $e4 >P $ac9c $ea >M $ac9d $f7 >M $ac9e $85 >M
T{ op PC S A X Y P -> $ac9d $41 $1b $31 $36 $e4 }T T{ $ac9c M $ac9d M $ac9e M -> $ea $f7 $85 }T
$8d9f >PC $3c >S $b2 >A $85 >X $ea >Y $65 >P $8d9f $ea >M $8da0 $f0 >M $8da1 $28 >M
T{ op PC S A X Y P -> $8da0 $3c $b2 $85 $ea $65 }T T{ $8d9f M $8da0 M $8da1 M -> $ea $f0 $28 }T
$5b13 >PC $97 >S $07 >A $e7 >X $e2 >Y $e0 >P $5b13 $ea >M $5b14 $96 >M $5b15 $a4 >M
T{ op PC S A X Y P -> $5b14 $97 $07 $e7 $e2 $e0 }T T{ $5b13 M $5b14 M $5b15 M -> $ea $96 $a4 }T
$d3f1 >PC $1b >S $04 >A $b0 >X $1a >Y $65 >P $d3f1 $ea >M $d3f2 $aa >M $d3f3 $68 >M
T{ op PC S A X Y P -> $d3f2 $1b $04 $b0 $1a $65 }T T{ $d3f1 M $d3f2 M $d3f3 M -> $ea $aa $68 }T
$2c4e >PC $84 >S $c1 >A $08 >X $62 >Y $a1 >P $2c4e $ea >M $2c4f $48 >M $2c50 $d2 >M
T{ op PC S A X Y P -> $2c4f $84 $c1 $08 $62 $a1 }T T{ $2c4e M $2c4f M $2c50 M -> $ea $48 $d2 }T
$13a3 >PC $33 >S $f1 >A $cd >X $b9 >Y $64 >P $13a3 $ea >M $13a4 $9c >M $13a5 $55 >M
T{ op PC S A X Y P -> $13a4 $33 $f1 $cd $b9 $64 }T T{ $13a3 M $13a4 M $13a5 M -> $ea $9c $55 }T
$0ebb >PC $99 >S $ed >A $95 >X $be >Y $e3 >P $0ebb $ea >M $0ebc $86 >M $0ebd $7a >M
T{ op PC S A X Y P -> $0ebc $99 $ed $95 $be $e3 }T T{ $0ebb M $0ebc M $0ebd M -> $ea $86 $7a }T
( eb )
$3c8e >PC $c1 >S $a5 >A $04 >X $ba >Y $64 >P $3c8e $eb >M $3c8f $46 >M $3c90 $0e >M
T{ op PC S A X Y P -> $3c8f $c1 $a5 $04 $ba $64 }T T{ $3c8e M $3c8f M $3c90 M -> $eb $46 $0e }T
$35f7 >PC $61 >S $52 >A $27 >X $d1 >Y $62 >P $35f7 $eb >M $35f8 $61 >M $35f9 $3d >M
T{ op PC S A X Y P -> $35f8 $61 $52 $27 $d1 $62 }T T{ $35f7 M $35f8 M $35f9 M -> $eb $61 $3d }T
$ea17 >PC $29 >S $29 >A $f0 >X $78 >Y $a0 >P $ea17 $eb >M $ea18 $2d >M $ea19 $37 >M
T{ op PC S A X Y P -> $ea18 $29 $29 $f0 $78 $a0 }T T{ $ea17 M $ea18 M $ea19 M -> $eb $2d $37 }T
$9f20 >PC $b8 >S $7d >A $86 >X $02 >Y $e2 >P $9f20 $eb >M $9f21 $91 >M $9f22 $e6 >M
T{ op PC S A X Y P -> $9f21 $b8 $7d $86 $02 $e2 }T T{ $9f20 M $9f21 M $9f22 M -> $eb $91 $e6 }T
$53dc >PC $5e >S $49 >A $93 >X $80 >Y $a7 >P $53dc $eb >M $53dd $03 >M $53de $9b >M
T{ op PC S A X Y P -> $53dd $5e $49 $93 $80 $a7 }T T{ $53dc M $53dd M $53de M -> $eb $03 $9b }T
$2b1b >PC $25 >S $1c >A $3c >X $22 >Y $22 >P $2b1b $eb >M $2b1c $09 >M $2b1d $bd >M
T{ op PC S A X Y P -> $2b1c $25 $1c $3c $22 $22 }T T{ $2b1b M $2b1c M $2b1d M -> $eb $09 $bd }T
$8abb >PC $0d >S $7c >A $c0 >X $e3 >Y $a3 >P $8abb $eb >M $8abc $c3 >M $8abd $d3 >M
T{ op PC S A X Y P -> $8abc $0d $7c $c0 $e3 $a3 }T T{ $8abb M $8abc M $8abd M -> $eb $c3 $d3 }T
$a426 >PC $dd >S $fc >A $35 >X $f1 >Y $65 >P $a426 $eb >M $a427 $99 >M $a428 $bf >M
T{ op PC S A X Y P -> $a427 $dd $fc $35 $f1 $65 }T T{ $a426 M $a427 M $a428 M -> $eb $99 $bf }T
$d3e4 >PC $51 >S $d6 >A $bf >X $05 >Y $61 >P $d3e4 $eb >M $d3e5 $aa >M $d3e6 $f1 >M
T{ op PC S A X Y P -> $d3e5 $51 $d6 $bf $05 $61 }T T{ $d3e4 M $d3e5 M $d3e6 M -> $eb $aa $f1 }T
$2405 >PC $21 >S $07 >A $d4 >X $b6 >Y $23 >P $2405 $eb >M $2406 $68 >M $2407 $9b >M
T{ op PC S A X Y P -> $2406 $21 $07 $d4 $b6 $23 }T T{ $2405 M $2406 M $2407 M -> $eb $68 $9b }T
$e179 >PC $d4 >S $af >A $43 >X $99 >Y $23 >P $e179 $eb >M $e17a $59 >M $e17b $fb >M
T{ op PC S A X Y P -> $e17a $d4 $af $43 $99 $23 }T T{ $e179 M $e17a M $e17b M -> $eb $59 $fb }T
$8c5b >PC $2a >S $a8 >A $63 >X $b7 >Y $22 >P $8c5b $eb >M $8c5c $ec >M $8c5d $42 >M
T{ op PC S A X Y P -> $8c5c $2a $a8 $63 $b7 $22 }T T{ $8c5b M $8c5c M $8c5d M -> $eb $ec $42 }T
$d51d >PC $77 >S $53 >A $fc >X $48 >Y $a5 >P $d51d $eb >M $d51e $87 >M $d51f $15 >M
T{ op PC S A X Y P -> $d51e $77 $53 $fc $48 $a5 }T T{ $d51d M $d51e M $d51f M -> $eb $87 $15 }T
$3b24 >PC $c4 >S $1e >A $2a >X $dd >Y $a2 >P $3b24 $eb >M $3b25 $cc >M $3b26 $4b >M
T{ op PC S A X Y P -> $3b25 $c4 $1e $2a $dd $a2 }T T{ $3b24 M $3b25 M $3b26 M -> $eb $cc $4b }T
$1dbf >PC $86 >S $82 >A $93 >X $1b >Y $e7 >P $1dbf $eb >M $1dc0 $7a >M $1dc1 $9d >M
T{ op PC S A X Y P -> $1dc0 $86 $82 $93 $1b $e7 }T T{ $1dbf M $1dc0 M $1dc1 M -> $eb $7a $9d }T
$f2ab >PC $33 >S $96 >A $cb >X $8a >Y $e1 >P $f2ab $eb >M $f2ac $06 >M $f2ad $93 >M
T{ op PC S A X Y P -> $f2ac $33 $96 $cb $8a $e1 }T T{ $f2ab M $f2ac M $f2ad M -> $eb $06 $93 }T
( ec )
$b4c1 >PC $9c >S $d3 >A $d4 >X $dd >Y $e3 >P $b4c1 $ec >M $b4c2 $39 >M $b4c3 $b8 >M $b4c4 $32 >M $b839 $90 >M
T{ op PC S A X Y P -> $b4c4 $9c $d3 $d4 $dd $61 }T T{ $b4c1 M $b4c2 M $b4c3 M $b4c4 M $b839 M -> $ec $39 $b8 $32 $90 }T
$35fa >PC $a4 >S $33 >A $33 >X $7e >Y $23 >P $1dc7 $d0 >M $35fa $ec >M $35fb $c7 >M $35fc $1d >M $35fd $d6 >M
T{ op PC S A X Y P -> $35fd $a4 $33 $33 $7e $20 }T T{ $1dc7 M $35fa M $35fb M $35fc M $35fd M -> $d0 $ec $c7 $1d $d6 }T
$11c2 >PC $8d >S $7a >A $25 >X $74 >Y $20 >P $11c2 $ec >M $11c3 $2d >M $11c4 $6f >M $11c5 $2b >M $6f2d $9d >M
T{ op PC S A X Y P -> $11c5 $8d $7a $25 $74 $a0 }T T{ $11c2 M $11c3 M $11c4 M $11c5 M $6f2d M -> $ec $2d $6f $2b $9d }T
$bf5e >PC $de >S $f3 >A $c2 >X $b5 >Y $e6 >P $1e7d $e6 >M $bf5e $ec >M $bf5f $7d >M $bf60 $1e >M $bf61 $6b >M
T{ op PC S A X Y P -> $bf61 $de $f3 $c2 $b5 $e4 }T T{ $1e7d M $bf5e M $bf5f M $bf60 M $bf61 M -> $e6 $ec $7d $1e $6b }T
$bc13 >PC $0e >S $37 >A $90 >X $9c >Y $65 >P $bc13 $ec >M $bc14 $13 >M $bc15 $e8 >M $bc16 $11 >M $e813 $38 >M
T{ op PC S A X Y P -> $bc16 $0e $37 $90 $9c $65 }T T{ $bc13 M $bc14 M $bc15 M $bc16 M $e813 M -> $ec $13 $e8 $11 $38 }T
$d0bb >PC $2b >S $9d >A $2f >X $58 >Y $64 >P $ce5f $b2 >M $d0bb $ec >M $d0bc $5f >M $d0bd $ce >M $d0be $cd >M
T{ op PC S A X Y P -> $d0be $2b $9d $2f $58 $64 }T T{ $ce5f M $d0bb M $d0bc M $d0bd M $d0be M -> $b2 $ec $5f $ce $cd }T
$523d >PC $4a >S $23 >A $7e >X $8a >Y $21 >P $523d $ec >M $523e $45 >M $523f $ba >M $5240 $74 >M $ba45 $31 >M
T{ op PC S A X Y P -> $5240 $4a $23 $7e $8a $21 }T T{ $523d M $523e M $523f M $5240 M $ba45 M -> $ec $45 $ba $74 $31 }T
$df69 >PC $71 >S $0c >A $bc >X $65 >Y $a7 >P $2fa0 $7b >M $df69 $ec >M $df6a $a0 >M $df6b $2f >M $df6c $d1 >M
T{ op PC S A X Y P -> $df6c $71 $0c $bc $65 $25 }T T{ $2fa0 M $df69 M $df6a M $df6b M $df6c M -> $7b $ec $a0 $2f $d1 }T
$653d >PC $06 >S $d3 >A $39 >X $cc >Y $a5 >P $653d $ec >M $653e $9b >M $653f $f3 >M $6540 $96 >M $f39b $3f >M
T{ op PC S A X Y P -> $6540 $06 $d3 $39 $cc $a4 }T T{ $653d M $653e M $653f M $6540 M $f39b M -> $ec $9b $f3 $96 $3f }T
$5434 >PC $ff >S $ac >A $71 >X $87 >Y $20 >P $5434 $ec >M $5435 $ff >M $5436 $57 >M $5437 $59 >M $57ff $f2 >M
T{ op PC S A X Y P -> $5437 $ff $ac $71 $87 $20 }T T{ $5434 M $5435 M $5436 M $5437 M $57ff M -> $ec $ff $57 $59 $f2 }T
$bfbc >PC $7a >S $ec >A $86 >X $51 >Y $65 >P $99d8 $e1 >M $bfbc $ec >M $bfbd $d8 >M $bfbe $99 >M $bfbf $ca >M
T{ op PC S A X Y P -> $bfbf $7a $ec $86 $51 $e4 }T T{ $99d8 M $bfbc M $bfbd M $bfbe M $bfbf M -> $e1 $ec $d8 $99 $ca }T
$dbcc >PC $6e >S $a2 >A $d6 >X $11 >Y $e5 >P $dbcc $ec >M $dbcd $5b >M $dbce $eb >M $dbcf $c0 >M $eb5b $ba >M
T{ op PC S A X Y P -> $dbcf $6e $a2 $d6 $11 $65 }T T{ $dbcc M $dbcd M $dbce M $dbcf M $eb5b M -> $ec $5b $eb $c0 $ba }T
$2853 >PC $ad >S $4b >A $99 >X $12 >Y $a3 >P $2853 $ec >M $2854 $b0 >M $2855 $3d >M $2856 $e3 >M $3db0 $7a >M
T{ op PC S A X Y P -> $2856 $ad $4b $99 $12 $21 }T T{ $2853 M $2854 M $2855 M $2856 M $3db0 M -> $ec $b0 $3d $e3 $7a }T
$4423 >PC $c1 >S $ac >A $5d >X $7a >Y $61 >P $2468 $18 >M $4423 $ec >M $4424 $68 >M $4425 $24 >M $4426 $88 >M
T{ op PC S A X Y P -> $4426 $c1 $ac $5d $7a $61 }T T{ $2468 M $4423 M $4424 M $4425 M $4426 M -> $18 $ec $68 $24 $88 }T
$2102 >PC $3e >S $e0 >A $e7 >X $86 >Y $24 >P $2102 $ec >M $2103 $d8 >M $2104 $c3 >M $2105 $ef >M $c3d8 $4d >M
T{ op PC S A X Y P -> $2105 $3e $e0 $e7 $86 $a5 }T T{ $2102 M $2103 M $2104 M $2105 M $c3d8 M -> $ec $d8 $c3 $ef $4d }T
$3b94 >PC $aa >S $0f >A $38 >X $b2 >Y $62 >P $3739 $2b >M $3b94 $ec >M $3b95 $39 >M $3b96 $37 >M $3b97 $d9 >M
T{ op PC S A X Y P -> $3b97 $aa $0f $38 $b2 $61 }T T{ $3739 M $3b94 M $3b95 M $3b96 M $3b97 M -> $2b $ec $39 $37 $d9 }T
( ed )
$20f0 >PC $c6 >S $ca >A $0a >X $48 >Y $f3 >P $146e $ed >M $20f0 $ed >M $20f1 $6e >M $20f2 $14 >M
T{ op PC S A X Y P -> $20f3 $c6 $dd $0a $48 $b0 }T T{ $146e M $20f0 M $20f1 M $20f2 M -> $ed $ed $6e $14 }T
$4535 >PC $fe >S $e9 >A $6d >X $c3 >Y $37 >P $4535 $ed >M $4536 $b1 >M $4537 $ad >M $adb1 $3e >M
T{ op PC S A X Y P -> $4538 $fe $ab $6d $c3 $b5 }T T{ $4535 M $4536 M $4537 M $adb1 M -> $ed $b1 $ad $3e }T
$3423 >PC $ea >S $45 >A $54 >X $d7 >Y $77 >P $3423 $ed >M $3424 $b1 >M $3425 $67 >M $67b1 $c0 >M
T{ op PC S A X Y P -> $3426 $ea $85 $54 $d7 $f4 }T T{ $3423 M $3424 M $3425 M $67b1 M -> $ed $b1 $67 $c0 }T
$9277 >PC $88 >S $ac >A $c7 >X $22 >Y $76 >P $3047 $d9 >M $9277 $ed >M $9278 $47 >M $9279 $30 >M
T{ op PC S A X Y P -> $927a $88 $d2 $c7 $22 $b4 }T T{ $3047 M $9277 M $9278 M $9279 M -> $d9 $ed $47 $30 }T
$e1b5 >PC $59 >S $75 >A $c0 >X $44 >Y $f6 >P $11a4 $19 >M $e1b5 $ed >M $e1b6 $a4 >M $e1b7 $11 >M
T{ op PC S A X Y P -> $e1b8 $59 $5b $c0 $44 $35 }T T{ $11a4 M $e1b5 M $e1b6 M $e1b7 M -> $19 $ed $a4 $11 }T
$15fe >PC $c7 >S $47 >A $c6 >X $21 >Y $36 >P $15fe $ed >M $15ff $bb >M $1600 $45 >M $45bb $17 >M
T{ op PC S A X Y P -> $1601 $c7 $2f $c6 $21 $35 }T T{ $15fe M $15ff M $1600 M $45bb M -> $ed $bb $45 $17 }T
$8f41 >PC $4f >S $d7 >A $a8 >X $17 >Y $f3 >P $333f $df >M $8f41 $ed >M $8f42 $3f >M $8f43 $33 >M
T{ op PC S A X Y P -> $8f44 $4f $f8 $a8 $17 $b0 }T T{ $333f M $8f41 M $8f42 M $8f43 M -> $df $ed $3f $33 }T
$4ecc >PC $7a >S $ff >A $83 >X $11 >Y $b1 >P $2c7d $b3 >M $4ecc $ed >M $4ecd $7d >M $4ece $2c >M
T{ op PC S A X Y P -> $4ecf $7a $4c $83 $11 $31 }T T{ $2c7d M $4ecc M $4ecd M $4ece M -> $b3 $ed $7d $2c }T
$f291 >PC $18 >S $20 >A $a1 >X $4b >Y $b4 >P $2cf3 $2d >M $f291 $ed >M $f292 $f3 >M $f293 $2c >M
T{ op PC S A X Y P -> $f294 $18 $f2 $a1 $4b $b4 }T T{ $2cf3 M $f291 M $f292 M $f293 M -> $2d $ed $f3 $2c }T
$cda6 >PC $0a >S $69 >A $80 >X $86 >Y $b1 >P $cda6 $ed >M $cda7 $80 >M $cda8 $e4 >M $e480 $54 >M
T{ op PC S A X Y P -> $cda9 $0a $15 $80 $86 $31 }T T{ $cda6 M $cda7 M $cda8 M $e480 M -> $ed $80 $e4 $54 }T
$6be4 >PC $73 >S $40 >A $b1 >X $60 >Y $32 >P $6be4 $ed >M $6be5 $bd >M $6be6 $6d >M $6dbd $96 >M
T{ op PC S A X Y P -> $6be7 $73 $a9 $b1 $60 $f0 }T T{ $6be4 M $6be5 M $6be6 M $6dbd M -> $ed $bd $6d $96 }T
$20c4 >PC $8c >S $3b >A $42 >X $95 >Y $33 >P $14ff $a8 >M $20c4 $ed >M $20c5 $ff >M $20c6 $14 >M
T{ op PC S A X Y P -> $20c7 $8c $93 $42 $95 $f0 }T T{ $14ff M $20c4 M $20c5 M $20c6 M -> $a8 $ed $ff $14 }T
$e1a4 >PC $5b >S $29 >A $76 >X $e1 >Y $77 >P $3040 $72 >M $e1a4 $ed >M $e1a5 $40 >M $e1a6 $30 >M
T{ op PC S A X Y P -> $e1a7 $5b $b7 $76 $e1 $b4 }T T{ $3040 M $e1a4 M $e1a5 M $e1a6 M -> $72 $ed $40 $30 }T
$f0c4 >PC $cc >S $ff >A $bc >X $b6 >Y $b7 >P $8181 $58 >M $f0c4 $ed >M $f0c5 $81 >M $f0c6 $81 >M
T{ op PC S A X Y P -> $f0c7 $cc $a7 $bc $b6 $b5 }T T{ $8181 M $f0c4 M $f0c5 M $f0c6 M -> $58 $ed $81 $81 }T
$abf7 >PC $a6 >S $28 >A $f4 >X $1f >Y $b0 >P $3857 $ea >M $abf7 $ed >M $abf8 $57 >M $abf9 $38 >M
T{ op PC S A X Y P -> $abfa $a6 $3d $f4 $1f $30 }T T{ $3857 M $abf7 M $abf8 M $abf9 M -> $ea $ed $57 $38 }T
$1ac1 >PC $6b >S $f2 >A $9a >X $f9 >Y $70 >P $1ac1 $ed >M $1ac2 $4d >M $1ac3 $bc >M $bc4d $21 >M
T{ op PC S A X Y P -> $1ac4 $6b $d0 $9a $f9 $b1 }T T{ $1ac1 M $1ac2 M $1ac3 M $bc4d M -> $ed $4d $bc $21 }T
( ee )
$9add >PC $1d >S $33 >A $c4 >X $c6 >Y $e3 >P $04c5 $c6 >M $9add $ee >M $9ade $c5 >M $9adf $04 >M $9ae0 $da >M
T{ op PC S A X Y P -> $9ae0 $1d $33 $c4 $c6 $e1 }T T{ $04c5 M $9add M $9ade M $9adf M $9ae0 M -> $c7 $ee $c5 $04 $da }T
$6a6f >PC $75 >S $0d >A $f1 >X $c3 >Y $22 >P $6a6f $ee >M $6a70 $e3 >M $6a71 $7e >M $6a72 $52 >M $7ee3 $d6 >M
T{ op PC S A X Y P -> $6a72 $75 $0d $f1 $c3 $a0 }T T{ $6a6f M $6a70 M $6a71 M $6a72 M $7ee3 M -> $ee $e3 $7e $52 $d7 }T
$a5af >PC $04 >S $c4 >A $b4 >X $37 >Y $a2 >P $21ee $89 >M $a5af $ee >M $a5b0 $ee >M $a5b1 $21 >M $a5b2 $0a >M
T{ op PC S A X Y P -> $a5b2 $04 $c4 $b4 $37 $a0 }T T{ $21ee M $a5af M $a5b0 M $a5b1 M $a5b2 M -> $8a $ee $ee $21 $0a }T
$103b >PC $43 >S $68 >A $56 >X $df >Y $e3 >P $103b $ee >M $103c $17 >M $103d $b5 >M $103e $21 >M $b517 $24 >M
T{ op PC S A X Y P -> $103e $43 $68 $56 $df $61 }T T{ $103b M $103c M $103d M $103e M $b517 M -> $ee $17 $b5 $21 $25 }T
$f582 >PC $31 >S $8a >A $22 >X $0e >Y $60 >P $5b95 $cc >M $f582 $ee >M $f583 $95 >M $f584 $5b >M $f585 $e1 >M
T{ op PC S A X Y P -> $f585 $31 $8a $22 $0e $e0 }T T{ $5b95 M $f582 M $f583 M $f584 M $f585 M -> $cd $ee $95 $5b $e1 }T
$2faa >PC $80 >S $2c >A $3f >X $d4 >Y $67 >P $2faa $ee >M $2fab $4d >M $2fac $3b >M $2fad $70 >M $3b4d $47 >M
T{ op PC S A X Y P -> $2fad $80 $2c $3f $d4 $65 }T T{ $2faa M $2fab M $2fac M $2fad M $3b4d M -> $ee $4d $3b $70 $48 }T
$be97 >PC $20 >S $96 >A $f9 >X $44 >Y $22 >P $bdbd $14 >M $be97 $ee >M $be98 $bd >M $be99 $bd >M $be9a $68 >M
T{ op PC S A X Y P -> $be9a $20 $96 $f9 $44 $20 }T T{ $bdbd M $be97 M $be98 M $be99 M $be9a M -> $15 $ee $bd $bd $68 }T
$b979 >PC $8b >S $d1 >A $02 >X $91 >Y $e4 >P $3df2 $e6 >M $b979 $ee >M $b97a $f2 >M $b97b $3d >M $b97c $d9 >M
T{ op PC S A X Y P -> $b97c $8b $d1 $02 $91 $e4 }T T{ $3df2 M $b979 M $b97a M $b97b M $b97c M -> $e7 $ee $f2 $3d $d9 }T
$64c9 >PC $42 >S $9b >A $05 >X $fa >Y $61 >P $3887 $97 >M $64c9 $ee >M $64ca $87 >M $64cb $38 >M $64cc $93 >M
T{ op PC S A X Y P -> $64cc $42 $9b $05 $fa $e1 }T T{ $3887 M $64c9 M $64ca M $64cb M $64cc M -> $98 $ee $87 $38 $93 }T
$b595 >PC $3d >S $df >A $ea >X $9b >Y $a1 >P $8557 $75 >M $b595 $ee >M $b596 $57 >M $b597 $85 >M $b598 $70 >M
T{ op PC S A X Y P -> $b598 $3d $df $ea $9b $21 }T T{ $8557 M $b595 M $b596 M $b597 M $b598 M -> $76 $ee $57 $85 $70 }T
$6352 >PC $28 >S $7d >A $af >X $44 >Y $e4 >P $06d7 $fc >M $6352 $ee >M $6353 $d7 >M $6354 $06 >M $6355 $75 >M
T{ op PC S A X Y P -> $6355 $28 $7d $af $44 $e4 }T T{ $06d7 M $6352 M $6353 M $6354 M $6355 M -> $fd $ee $d7 $06 $75 }T
$e60f >PC $ce >S $70 >A $c4 >X $b8 >Y $67 >P $8f78 $e1 >M $e60f $ee >M $e610 $78 >M $e611 $8f >M $e612 $cd >M
T{ op PC S A X Y P -> $e612 $ce $70 $c4 $b8 $e5 }T T{ $8f78 M $e60f M $e610 M $e611 M $e612 M -> $e2 $ee $78 $8f $cd }T
$8db0 >PC $c2 >S $52 >A $a4 >X $d9 >Y $65 >P $1d92 $c8 >M $8db0 $ee >M $8db1 $92 >M $8db2 $1d >M $8db3 $55 >M
T{ op PC S A X Y P -> $8db3 $c2 $52 $a4 $d9 $e5 }T T{ $1d92 M $8db0 M $8db1 M $8db2 M $8db3 M -> $c9 $ee $92 $1d $55 }T
$94eb >PC $1c >S $0a >A $07 >X $82 >Y $24 >P $94eb $ee >M $94ec $5c >M $94ed $e1 >M $94ee $d1 >M $e15c $9b >M
T{ op PC S A X Y P -> $94ee $1c $0a $07 $82 $a4 }T T{ $94eb M $94ec M $94ed M $94ee M $e15c M -> $ee $5c $e1 $d1 $9c }T
$90c1 >PC $10 >S $72 >A $6f >X $69 >Y $e4 >P $90c1 $ee >M $90c2 $ef >M $90c3 $b0 >M $90c4 $1f >M $b0ef $40 >M
T{ op PC S A X Y P -> $90c4 $10 $72 $6f $69 $64 }T T{ $90c1 M $90c2 M $90c3 M $90c4 M $b0ef M -> $ee $ef $b0 $1f $41 }T
$e926 >PC $e3 >S $a1 >A $41 >X $7b >Y $21 >P $4869 $c4 >M $e926 $ee >M $e927 $69 >M $e928 $48 >M $e929 $d3 >M
T{ op PC S A X Y P -> $e929 $e3 $a1 $41 $7b $a1 }T T{ $4869 M $e926 M $e927 M $e928 M $e929 M -> $c5 $ee $69 $48 $d3 }T
( ef )
$881d >PC $30 >S $96 >A $48 >X $94 >Y $a6 >P $00ac $e3 >M $881d $ef >M $881e $ac >M $881f $37 >M $8857 $f1 >M
T{ op PC S A X Y P -> $8857 $30 $96 $48 $94 $a6 }T T{ $00ac M $881d M $881e M $881f M $8857 M -> $e3 $ef $ac $37 $f1 }T
$382c >PC $8d >S $83 >A $dd >X $94 >Y $a4 >P $00b3 $27 >M $382c $ef >M $382d $b3 >M $382e $3a >M $382f $0b >M $3869 $b0 >M
T{ op PC S A X Y P -> $382f $8d $83 $dd $94 $a4 }T T{ $00b3 M $382c M $382d M $382e M $382f M $3869 M -> $27 $ef $b3 $3a $0b $b0 }T
$7235 >PC $d3 >S $0a >A $60 >X $e5 >Y $67 >P $0044 $d7 >M $7235 $ef >M $7236 $44 >M $7237 $6b >M $72a3 $80 >M
T{ op PC S A X Y P -> $72a3 $d3 $0a $60 $e5 $67 }T T{ $0044 M $7235 M $7236 M $7237 M $72a3 M -> $d7 $ef $44 $6b $80 }T
$46f5 >PC $10 >S $57 >A $16 >X $19 >Y $26 >P $00c0 $1c >M $4607 $2b >M $46f5 $ef >M $46f6 $c0 >M $46f7 $0f >M $46f8 $82 >M
T{ op PC S A X Y P -> $46f8 $10 $57 $16 $19 $26 }T T{ $00c0 M $4607 M $46f5 M $46f6 M $46f7 M $46f8 M -> $1c $2b $ef $c0 $0f $82 }T
$c34f >PC $cc >S $9e >A $cf >X $0b >Y $61 >P $001c $ff >M $c34f $ef >M $c350 $1c >M $c351 $13 >M $c365 $17 >M
T{ op PC S A X Y P -> $c365 $cc $9e $cf $0b $61 }T T{ $001c M $c34f M $c350 M $c351 M $c365 M -> $ff $ef $1c $13 $17 }T
$cd7e >PC $43 >S $b0 >A $af >X $c0 >Y $62 >P $004a $bf >M $cd6d $39 >M $cd7e $ef >M $cd7f $4a >M $cd80 $ec >M $cd81 $d5 >M
T{ op PC S A X Y P -> $cd81 $43 $b0 $af $c0 $62 }T T{ $004a M $cd6d M $cd7e M $cd7f M $cd80 M $cd81 M -> $bf $39 $ef $4a $ec $d5 }T
$aff2 >PC $99 >S $7e >A $53 >X $f7 >Y $21 >P $00dc $47 >M $af7d $4f >M $aff2 $ef >M $aff3 $dc >M $aff4 $88 >M
T{ op PC S A X Y P -> $af7d $99 $7e $53 $f7 $21 }T T{ $00dc M $af7d M $aff2 M $aff3 M $aff4 M -> $47 $4f $ef $dc $88 }T
$1ab7 >PC $83 >S $a1 >A $26 >X $ab >Y $a7 >P $0055 $31 >M $1a50 $a3 >M $1ab7 $ef >M $1ab8 $55 >M $1ab9 $96 >M $1aba $94 >M
T{ op PC S A X Y P -> $1aba $83 $a1 $26 $ab $a7 }T T{ $0055 M $1a50 M $1ab7 M $1ab8 M $1ab9 M $1aba M -> $31 $a3 $ef $55 $96 $94 }T
$fa89 >PC $8b >S $ca >A $7e >X $1f >Y $62 >P $00a5 $b2 >M $fa24 $54 >M $fa89 $ef >M $fa8a $a5 >M $fa8b $98 >M $fa8c $9a >M
T{ op PC S A X Y P -> $fa8c $8b $ca $7e $1f $62 }T T{ $00a5 M $fa24 M $fa89 M $fa8a M $fa8b M $fa8c M -> $b2 $54 $ef $a5 $98 $9a }T
$e36f >PC $48 >S $31 >A $75 >X $2b >Y $20 >P $00cf $3f >M $e36f $ef >M $e370 $cf >M $e371 $4b >M $e372 $cc >M $e3bd $51 >M
T{ op PC S A X Y P -> $e372 $48 $31 $75 $2b $20 }T T{ $00cf M $e36f M $e370 M $e371 M $e372 M $e3bd M -> $3f $ef $cf $4b $cc $51 }T
$c64f >PC $e6 >S $c0 >A $a1 >X $a3 >Y $27 >P $0097 $36 >M $c64f $ef >M $c650 $97 >M $c651 $3e >M $c652 $b6 >M $c690 $0c >M
T{ op PC S A X Y P -> $c652 $e6 $c0 $a1 $a3 $27 }T T{ $0097 M $c64f M $c650 M $c651 M $c652 M $c690 M -> $36 $ef $97 $3e $b6 $0c }T
$8220 >PC $d3 >S $68 >A $a3 >X $7c >Y $24 >P $0055 $e8 >M $81c0 $26 >M $8220 $ef >M $8221 $55 >M $8222 $9d >M $82c0 $cc >M
T{ op PC S A X Y P -> $81c0 $d3 $68 $a3 $7c $24 }T T{ $0055 M $81c0 M $8220 M $8221 M $8222 M $82c0 M -> $e8 $26 $ef $55 $9d $cc }T
$c467 >PC $5c >S $c9 >A $26 >X $0f >Y $e1 >P $0028 $9b >M $c467 $ef >M $c468 $28 >M $c469 $6d >M $c46a $d1 >M $c4d7 $6c >M
T{ op PC S A X Y P -> $c46a $5c $c9 $26 $0f $e1 }T T{ $0028 M $c467 M $c468 M $c469 M $c46a M $c4d7 M -> $9b $ef $28 $6d $d1 $6c }T
$0a8a >PC $43 >S $fe >A $a9 >X $f1 >Y $e3 >P $008f $14 >M $0a2d $44 >M $0a8a $ef >M $0a8b $8f >M $0a8c $a0 >M $0a8d $7d >M
T{ op PC S A X Y P -> $0a8d $43 $fe $a9 $f1 $e3 }T T{ $008f M $0a2d M $0a8a M $0a8b M $0a8c M $0a8d M -> $14 $44 $ef $8f $a0 $7d }T
$f7b7 >PC $09 >S $9b >A $04 >X $95 >Y $e4 >P $001a $fe >M $f75e $66 >M $f7b7 $ef >M $f7b8 $1a >M $f7b9 $a4 >M
T{ op PC S A X Y P -> $f75e $09 $9b $04 $95 $e4 }T T{ $001a M $f75e M $f7b7 M $f7b8 M $f7b9 M -> $fe $66 $ef $1a $a4 }T
$1482 >PC $30 >S $9a >A $aa >X $69 >Y $64 >P $00c0 $8e >M $1476 $7b >M $1482 $ef >M $1483 $c0 >M $1484 $f1 >M $1485 $65 >M
T{ op PC S A X Y P -> $1485 $30 $9a $aa $69 $64 }T T{ $00c0 M $1476 M $1482 M $1483 M $1484 M $1485 M -> $8e $7b $ef $c0 $f1 $65 }T
( f0 )
$7ba2 >PC $f4 >S $16 >A $8c >X $47 >Y $a5 >P $7ba2 $f0 >M $7ba3 $e1 >M $7ba4 $43 >M
T{ op PC S A X Y P -> $7ba4 $f4 $16 $8c $47 $a5 }T T{ $7ba2 M $7ba3 M $7ba4 M -> $f0 $e1 $43 }T
$d9bc >PC $f9 >S $f8 >A $78 >X $00 >Y $63 >P $d99d $ad >M $d9bc $f0 >M $d9bd $df >M $d9be $1a >M
T{ op PC S A X Y P -> $d99d $f9 $f8 $78 $00 $63 }T T{ $d99d M $d9bc M $d9bd M $d9be M -> $ad $f0 $df $1a }T
$fdc3 >PC $6f >S $3c >A $22 >X $6e >Y $66 >P $fd3a $41 >M $fdc3 $f0 >M $fdc4 $75 >M $fdc5 $6f >M $fe3a $1d >M
T{ op PC S A X Y P -> $fe3a $6f $3c $22 $6e $66 }T T{ $fd3a M $fdc3 M $fdc4 M $fdc5 M $fe3a M -> $41 $f0 $75 $6f $1d }T
$fe9b >PC $6f >S $ef >A $27 >X $7f >Y $20 >P $fe9b $f0 >M $fe9c $af >M $fe9d $f9 >M
T{ op PC S A X Y P -> $fe9d $6f $ef $27 $7f $20 }T T{ $fe9b M $fe9c M $fe9d M -> $f0 $af $f9 }T
$d19f >PC $19 >S $56 >A $2e >X $b1 >Y $a2 >P $d15e $ef >M $d19f $f0 >M $d1a0 $bd >M $d1a1 $c1 >M
T{ op PC S A X Y P -> $d15e $19 $56 $2e $b1 $a2 }T T{ $d15e M $d19f M $d1a0 M $d1a1 M -> $ef $f0 $bd $c1 }T
$b486 >PC $9a >S $31 >A $a3 >X $bf >Y $20 >P $b486 $f0 >M $b487 $47 >M $b488 $e1 >M
T{ op PC S A X Y P -> $b488 $9a $31 $a3 $bf $20 }T T{ $b486 M $b487 M $b488 M -> $f0 $47 $e1 }T
$c7d4 >PC $45 >S $6c >A $89 >X $50 >Y $65 >P $c7d4 $f0 >M $c7d5 $39 >M $c7d6 $bd >M
T{ op PC S A X Y P -> $c7d6 $45 $6c $89 $50 $65 }T T{ $c7d4 M $c7d5 M $c7d6 M -> $f0 $39 $bd }T
$480e >PC $bf >S $39 >A $b9 >X $4b >Y $a1 >P $480e $f0 >M $480f $ea >M $4810 $90 >M
T{ op PC S A X Y P -> $4810 $bf $39 $b9 $4b $a1 }T T{ $480e M $480f M $4810 M -> $f0 $ea $90 }T
$f731 >PC $0d >S $e8 >A $ec >X $1e >Y $22 >P $f731 $f0 >M $f732 $26 >M $f733 $ad >M $f759 $f5 >M
T{ op PC S A X Y P -> $f759 $0d $e8 $ec $1e $22 }T T{ $f731 M $f732 M $f733 M $f759 M -> $f0 $26 $ad $f5 }T
$a99c >PC $be >S $34 >A $87 >X $4d >Y $e7 >P $a97a $85 >M $a99c $f0 >M $a99d $dc >M $a99e $96 >M
T{ op PC S A X Y P -> $a97a $be $34 $87 $4d $e7 }T T{ $a97a M $a99c M $a99d M $a99e M -> $85 $f0 $dc $96 }T
$1296 >PC $a5 >S $5c >A $03 >X $aa >Y $66 >P $1296 $f0 >M $1297 $0b >M $1298 $fe >M $12a3 $43 >M
T{ op PC S A X Y P -> $12a3 $a5 $5c $03 $aa $66 }T T{ $1296 M $1297 M $1298 M $12a3 M -> $f0 $0b $fe $43 }T
$f3c5 >PC $2f >S $4f >A $26 >X $b1 >Y $a6 >P $f330 $8b >M $f3c5 $f0 >M $f3c6 $69 >M $f3c7 $d5 >M $f430 $67 >M
T{ op PC S A X Y P -> $f430 $2f $4f $26 $b1 $a6 }T T{ $f330 M $f3c5 M $f3c6 M $f3c7 M $f430 M -> $8b $f0 $69 $d5 $67 }T
$a513 >PC $ad >S $21 >A $53 >X $4b >Y $25 >P $a513 $f0 >M $a514 $9e >M $a515 $26 >M
T{ op PC S A X Y P -> $a515 $ad $21 $53 $4b $25 }T T{ $a513 M $a514 M $a515 M -> $f0 $9e $26 }T
$a623 >PC $f3 >S $1c >A $ae >X $30 >Y $67 >P $a5f5 $a7 >M $a623 $f0 >M $a624 $d0 >M $a625 $f0 >M $a6f5 $ac >M
T{ op PC S A X Y P -> $a5f5 $f3 $1c $ae $30 $67 }T T{ $a5f5 M $a623 M $a624 M $a625 M $a6f5 M -> $a7 $f0 $d0 $f0 $ac }T
$1daf >PC $51 >S $6d >A $b1 >X $f2 >Y $a6 >P $1daf $f0 >M $1db0 $45 >M $1db1 $97 >M $1df6 $f5 >M
T{ op PC S A X Y P -> $1df6 $51 $6d $b1 $f2 $a6 }T T{ $1daf M $1db0 M $1db1 M $1df6 M -> $f0 $45 $97 $f5 }T
$4ed0 >PC $ed >S $3a >A $db >X $d9 >Y $e1 >P $4ed0 $f0 >M $4ed1 $f0 >M $4ed2 $ea >M
T{ op PC S A X Y P -> $4ed2 $ed $3a $db $d9 $e1 }T T{ $4ed0 M $4ed1 M $4ed2 M -> $f0 $f0 $ea }T
( f1 )
$c5a8 >PC $4b >S $56 >A $ef >X $e0 >Y $70 >P $00ac $ea >M $00ad $99 >M $9aca $5b >M $c5a8 $f1 >M $c5a9 $ac >M
T{ op PC S A X Y P -> $c5aa $4b $fa $ef $e0 $b0 }T T{ $00ac M $00ad M $9aca M $c5a8 M $c5a9 M -> $ea $99 $5b $f1 $ac }T
$81b0 >PC $ee >S $24 >A $79 >X $29 >Y $b5 >P $000a $86 >M $000b $2b >M $2baf $4b >M $81b0 $f1 >M $81b1 $0a >M
T{ op PC S A X Y P -> $81b2 $ee $d9 $79 $29 $b4 }T T{ $000a M $000b M $2baf M $81b0 M $81b1 M -> $86 $2b $4b $f1 $0a }T
$237d >PC $7b >S $08 >A $be >X $81 >Y $b4 >P $00ce $10 >M $00cf $2e >M $237d $f1 >M $237e $ce >M $2e91 $52 >M
T{ op PC S A X Y P -> $237f $7b $b5 $be $81 $b4 }T T{ $00ce M $00cf M $237d M $237e M $2e91 M -> $10 $2e $f1 $ce $52 }T
$5cfe >PC $c3 >S $55 >A $2e >X $69 >Y $33 >P $004b $3c >M $004c $38 >M $38a5 $25 >M $5cfe $f1 >M $5cff $4b >M
T{ op PC S A X Y P -> $5d00 $c3 $30 $2e $69 $31 }T T{ $004b M $004c M $38a5 M $5cfe M $5cff M -> $3c $38 $25 $f1 $4b }T
$1f6e >PC $df >S $3c >A $98 >X $a1 >Y $b1 >P $00c7 $5f >M $00c8 $1e >M $1f00 $73 >M $1f6e $f1 >M $1f6f $c7 >M
T{ op PC S A X Y P -> $1f70 $df $c9 $98 $a1 $b0 }T T{ $00c7 M $00c8 M $1f00 M $1f6e M $1f6f M -> $5f $1e $73 $f1 $c7 }T
$065b >PC $24 >S $b0 >A $86 >X $b9 >Y $b4 >P $0094 $d2 >M $0095 $b3 >M $065b $f1 >M $065c $94 >M $b48b $08 >M
T{ op PC S A X Y P -> $065d $24 $a7 $86 $b9 $b5 }T T{ $0094 M $0095 M $065b M $065c M $b48b M -> $d2 $b3 $f1 $94 $08 }T
$397c >PC $cb >S $a7 >A $fe >X $92 >Y $32 >P $00e0 $5c >M $00e1 $22 >M $22ee $a7 >M $397c $f1 >M $397d $e0 >M
T{ op PC S A X Y P -> $397e $cb $ff $fe $92 $b0 }T T{ $00e0 M $00e1 M $22ee M $397c M $397d M -> $5c $22 $a7 $f1 $e0 }T
$566c >PC $9e >S $29 >A $85 >X $7b >Y $75 >P $007b $f7 >M $007c $f7 >M $566c $f1 >M $566d $7b >M $f872 $fd >M
T{ op PC S A X Y P -> $566e $9e $2c $85 $7b $34 }T T{ $007b M $007c M $566c M $566d M $f872 M -> $f7 $f7 $f1 $7b $fd }T
$4ec3 >PC $e5 >S $92 >A $a8 >X $2c >Y $f4 >P $0010 $11 >M $0011 $3d >M $3d3d $b0 >M $4ec3 $f1 >M $4ec4 $10 >M
T{ op PC S A X Y P -> $4ec5 $e5 $e1 $a8 $2c $b4 }T T{ $0010 M $0011 M $3d3d M $4ec3 M $4ec4 M -> $11 $3d $b0 $f1 $10 }T
$6a03 >PC $54 >S $ef >A $fa >X $5b >Y $31 >P $00be $6a >M $00bf $0c >M $0cc5 $dc >M $6a03 $f1 >M $6a04 $be >M
T{ op PC S A X Y P -> $6a05 $54 $13 $fa $5b $31 }T T{ $00be M $00bf M $0cc5 M $6a03 M $6a04 M -> $6a $0c $dc $f1 $be }T
$46ff >PC $10 >S $1b >A $8b >X $be >Y $f6 >P $0053 $a2 >M $0054 $42 >M $4360 $3b >M $46ff $f1 >M $4700 $53 >M
T{ op PC S A X Y P -> $4701 $10 $df $8b $be $b4 }T T{ $0053 M $0054 M $4360 M $46ff M $4700 M -> $a2 $42 $3b $f1 $53 }T
$b777 >PC $1a >S $39 >A $3e >X $a5 >Y $b5 >P $0059 $99 >M $005a $1a >M $1b3e $17 >M $b777 $f1 >M $b778 $59 >M
T{ op PC S A X Y P -> $b779 $1a $22 $3e $a5 $35 }T T{ $0059 M $005a M $1b3e M $b777 M $b778 M -> $99 $1a $17 $f1 $59 }T
$b4fa >PC $e0 >S $1f >A $2f >X $63 >Y $72 >P $0046 $a0 >M $0047 $57 >M $5803 $1d >M $b4fa $f1 >M $b4fb $46 >M
T{ op PC S A X Y P -> $b4fc $e0 $01 $2f $63 $31 }T T{ $0046 M $0047 M $5803 M $b4fa M $b4fb M -> $a0 $57 $1d $f1 $46 }T
$e6a9 >PC $a2 >S $81 >A $71 >X $b0 >Y $b5 >P $002b $46 >M $002c $b4 >M $b4f6 $72 >M $e6a9 $f1 >M $e6aa $2b >M
T{ op PC S A X Y P -> $e6ab $a2 $0f $71 $b0 $75 }T T{ $002b M $002c M $b4f6 M $e6a9 M $e6aa M -> $46 $b4 $72 $f1 $2b }T
$4c16 >PC $23 >S $34 >A $6c >X $78 >Y $30 >P $001a $8a >M $001b $67 >M $4c16 $f1 >M $4c17 $1a >M $6802 $88 >M
T{ op PC S A X Y P -> $4c18 $23 $ab $6c $78 $f0 }T T{ $001a M $001b M $4c16 M $4c17 M $6802 M -> $8a $67 $f1 $1a $88 }T
$53c0 >PC $e1 >S $51 >A $bc >X $cb >Y $b4 >P $001d $e5 >M $001e $18 >M $19b0 $38 >M $53c0 $f1 >M $53c1 $1d >M
T{ op PC S A X Y P -> $53c2 $e1 $18 $bc $cb $35 }T T{ $001d M $001e M $19b0 M $53c0 M $53c1 M -> $e5 $18 $38 $f1 $1d }T
( f2 )
$06fe >PC $5b >S $fe >A $23 >X $60 >Y $76 >P $000a $30 >M $000b $a8 >M $06fe $f2 >M $06ff $0a >M $a830 $91 >M
T{ op PC S A X Y P -> $0700 $5b $6c $23 $60 $35 }T T{ $000a M $000b M $06fe M $06ff M $a830 M -> $30 $a8 $f2 $0a $91 }T
$e335 >PC $40 >S $65 >A $33 >X $5e >Y $33 >P $009a $47 >M $009b $73 >M $7347 $ed >M $e335 $f2 >M $e336 $9a >M
T{ op PC S A X Y P -> $e337 $40 $78 $33 $5e $30 }T T{ $009a M $009b M $7347 M $e335 M $e336 M -> $47 $73 $ed $f2 $9a }T
$c344 >PC $8b >S $2a >A $be >X $53 >Y $34 >P $0074 $91 >M $0075 $8a >M $8a91 $46 >M $c344 $f2 >M $c345 $74 >M
T{ op PC S A X Y P -> $c346 $8b $e3 $be $53 $b4 }T T{ $0074 M $0075 M $8a91 M $c344 M $c345 M -> $91 $8a $46 $f2 $74 }T
$b8e4 >PC $7b >S $79 >A $fd >X $b1 >Y $f7 >P $00cf $16 >M $00d0 $51 >M $5116 $23 >M $b8e4 $f2 >M $b8e5 $cf >M
T{ op PC S A X Y P -> $b8e6 $7b $56 $fd $b1 $35 }T T{ $00cf M $00d0 M $5116 M $b8e4 M $b8e5 M -> $16 $51 $23 $f2 $cf }T
$78e9 >PC $6d >S $0a >A $9e >X $c7 >Y $31 >P $0065 $c0 >M $0066 $34 >M $34c0 $ca >M $78e9 $f2 >M $78ea $65 >M
T{ op PC S A X Y P -> $78eb $6d $40 $9e $c7 $30 }T T{ $0065 M $0066 M $34c0 M $78e9 M $78ea M -> $c0 $34 $ca $f2 $65 }T
$1e93 >PC $d4 >S $e5 >A $a9 >X $f6 >Y $f3 >P $00a5 $d3 >M $00a6 $b9 >M $1e93 $f2 >M $1e94 $a5 >M $b9d3 $87 >M
T{ op PC S A X Y P -> $1e95 $d4 $5e $a9 $f6 $31 }T T{ $00a5 M $00a6 M $1e93 M $1e94 M $b9d3 M -> $d3 $b9 $f2 $a5 $87 }T
$5557 >PC $7d >S $a5 >A $72 >X $10 >Y $b4 >P $00a1 $e5 >M $00a2 $a3 >M $5557 $f2 >M $5558 $a1 >M $a3e5 $a6 >M
T{ op PC S A X Y P -> $5559 $7d $fe $72 $10 $b4 }T T{ $00a1 M $00a2 M $5557 M $5558 M $a3e5 M -> $e5 $a3 $f2 $a1 $a6 }T
$98b5 >PC $14 >S $a0 >A $e7 >X $d6 >Y $f2 >P $00b9 $b5 >M $00ba $bb >M $98b5 $f2 >M $98b6 $b9 >M $bbb5 $b5 >M
T{ op PC S A X Y P -> $98b7 $14 $ea $e7 $d6 $b0 }T T{ $00b9 M $00ba M $98b5 M $98b6 M $bbb5 M -> $b5 $bb $f2 $b9 $b5 }T
$fea6 >PC $c3 >S $fc >A $97 >X $28 >Y $73 >P $0014 $d4 >M $0015 $e5 >M $e5d4 $84 >M $fea6 $f2 >M $fea7 $14 >M
T{ op PC S A X Y P -> $fea8 $c3 $78 $97 $28 $31 }T T{ $0014 M $0015 M $e5d4 M $fea6 M $fea7 M -> $d4 $e5 $84 $f2 $14 }T
$26ce >PC $be >S $32 >A $33 >X $54 >Y $f7 >P $0011 $12 >M $0012 $df >M $26ce $f2 >M $26cf $11 >M $df12 $26 >M
T{ op PC S A X Y P -> $26d0 $be $0c $33 $54 $35 }T T{ $0011 M $0012 M $26ce M $26cf M $df12 M -> $12 $df $f2 $11 $26 }T
$cb66 >PC $1d >S $0e >A $e0 >X $da >Y $75 >P $001e $6a >M $001f $66 >M $666a $dc >M $cb66 $f2 >M $cb67 $1e >M
T{ op PC S A X Y P -> $cb68 $1d $32 $e0 $da $34 }T T{ $001e M $001f M $666a M $cb66 M $cb67 M -> $6a $66 $dc $f2 $1e }T
$42bc >PC $7f >S $7d >A $0a >X $66 >Y $b1 >P $0090 $e4 >M $0091 $12 >M $12e4 $a6 >M $42bc $f2 >M $42bd $90 >M
T{ op PC S A X Y P -> $42be $7f $d7 $0a $66 $f0 }T T{ $0090 M $0091 M $12e4 M $42bc M $42bd M -> $e4 $12 $a6 $f2 $90 }T
$a49a >PC $5d >S $8a >A $55 >X $6a >Y $33 >P $0069 $29 >M $006a $51 >M $5129 $e9 >M $a49a $f2 >M $a49b $69 >M
T{ op PC S A X Y P -> $a49c $5d $a1 $55 $6a $b0 }T T{ $0069 M $006a M $5129 M $a49a M $a49b M -> $29 $51 $e9 $f2 $69 }T
$ef3a >PC $bc >S $a4 >A $c8 >X $11 >Y $f4 >P $0057 $bb >M $0058 $25 >M $25bb $4c >M $ef3a $f2 >M $ef3b $57 >M
T{ op PC S A X Y P -> $ef3c $bc $57 $c8 $11 $75 }T T{ $0057 M $0058 M $25bb M $ef3a M $ef3b M -> $bb $25 $4c $f2 $57 }T
$1296 >PC $7b >S $67 >A $3a >X $bd >Y $f6 >P $007e $ab >M $007f $b4 >M $1296 $f2 >M $1297 $7e >M $b4ab $9a >M
T{ op PC S A X Y P -> $1298 $7b $cc $3a $bd $f4 }T T{ $007e M $007f M $1296 M $1297 M $b4ab M -> $ab $b4 $f2 $7e $9a }T
$5a8d >PC $b0 >S $55 >A $70 >X $e6 >Y $f2 >P $00c3 $61 >M $00c4 $65 >M $5a8d $f2 >M $5a8e $c3 >M $6561 $85 >M
T{ op PC S A X Y P -> $5a8f $b0 $cf $70 $e6 $f0 }T T{ $00c3 M $00c4 M $5a8d M $5a8e M $6561 M -> $61 $65 $f2 $c3 $85 }T
( f3 )
$43ee >PC $50 >S $54 >A $9a >X $4e >Y $e4 >P $43ee $f3 >M $43ef $d9 >M $43f0 $69 >M
T{ op PC S A X Y P -> $43ef $50 $54 $9a $4e $e4 }T T{ $43ee M $43ef M $43f0 M -> $f3 $d9 $69 }T
$1bc4 >PC $05 >S $42 >A $51 >X $0e >Y $66 >P $1bc4 $f3 >M $1bc5 $08 >M $1bc6 $5f >M
T{ op PC S A X Y P -> $1bc5 $05 $42 $51 $0e $66 }T T{ $1bc4 M $1bc5 M $1bc6 M -> $f3 $08 $5f }T
$e92c >PC $3a >S $0d >A $f1 >X $92 >Y $65 >P $e92c $f3 >M $e92d $8a >M $e92e $84 >M
T{ op PC S A X Y P -> $e92d $3a $0d $f1 $92 $65 }T T{ $e92c M $e92d M $e92e M -> $f3 $8a $84 }T
$a1ea >PC $0c >S $5b >A $7e >X $cd >Y $e2 >P $a1ea $f3 >M $a1eb $d2 >M $a1ec $16 >M
T{ op PC S A X Y P -> $a1eb $0c $5b $7e $cd $e2 }T T{ $a1ea M $a1eb M $a1ec M -> $f3 $d2 $16 }T
$da2c >PC $a7 >S $20 >A $78 >X $c3 >Y $66 >P $da2c $f3 >M $da2d $7d >M $da2e $3b >M
T{ op PC S A X Y P -> $da2d $a7 $20 $78 $c3 $66 }T T{ $da2c M $da2d M $da2e M -> $f3 $7d $3b }T
$9fe7 >PC $11 >S $cd >A $04 >X $3c >Y $e3 >P $9fe7 $f3 >M $9fe8 $5b >M $9fe9 $3b >M
T{ op PC S A X Y P -> $9fe8 $11 $cd $04 $3c $e3 }T T{ $9fe7 M $9fe8 M $9fe9 M -> $f3 $5b $3b }T
$4c43 >PC $55 >S $93 >A $d6 >X $05 >Y $a7 >P $4c43 $f3 >M $4c44 $31 >M $4c45 $14 >M
T{ op PC S A X Y P -> $4c44 $55 $93 $d6 $05 $a7 }T T{ $4c43 M $4c44 M $4c45 M -> $f3 $31 $14 }T
$6177 >PC $be >S $5d >A $19 >X $ad >Y $e4 >P $6177 $f3 >M $6178 $e7 >M $6179 $5a >M
T{ op PC S A X Y P -> $6178 $be $5d $19 $ad $e4 }T T{ $6177 M $6178 M $6179 M -> $f3 $e7 $5a }T
$0c9e >PC $25 >S $52 >A $a2 >X $cc >Y $a0 >P $0c9e $f3 >M $0c9f $80 >M $0ca0 $90 >M
T{ op PC S A X Y P -> $0c9f $25 $52 $a2 $cc $a0 }T T{ $0c9e M $0c9f M $0ca0 M -> $f3 $80 $90 }T
$089d >PC $24 >S $2d >A $37 >X $1c >Y $22 >P $089d $f3 >M $089e $43 >M $089f $d6 >M
T{ op PC S A X Y P -> $089e $24 $2d $37 $1c $22 }T T{ $089d M $089e M $089f M -> $f3 $43 $d6 }T
$f521 >PC $28 >S $a4 >A $ea >X $7e >Y $26 >P $f521 $f3 >M $f522 $e5 >M $f523 $e7 >M
T{ op PC S A X Y P -> $f522 $28 $a4 $ea $7e $26 }T T{ $f521 M $f522 M $f523 M -> $f3 $e5 $e7 }T
$d219 >PC $a1 >S $6a >A $bc >X $74 >Y $e6 >P $d219 $f3 >M $d21a $4a >M $d21b $30 >M
T{ op PC S A X Y P -> $d21a $a1 $6a $bc $74 $e6 }T T{ $d219 M $d21a M $d21b M -> $f3 $4a $30 }T
$3b77 >PC $b1 >S $00 >A $6b >X $1a >Y $e5 >P $3b77 $f3 >M $3b78 $93 >M $3b79 $18 >M
T{ op PC S A X Y P -> $3b78 $b1 $00 $6b $1a $e5 }T T{ $3b77 M $3b78 M $3b79 M -> $f3 $93 $18 }T
$6c9d >PC $89 >S $a6 >A $1d >X $31 >Y $e2 >P $6c9d $f3 >M $6c9e $47 >M $6c9f $c5 >M
T{ op PC S A X Y P -> $6c9e $89 $a6 $1d $31 $e2 }T T{ $6c9d M $6c9e M $6c9f M -> $f3 $47 $c5 }T
$a737 >PC $40 >S $0d >A $95 >X $27 >Y $62 >P $a737 $f3 >M $a738 $87 >M $a739 $3f >M
T{ op PC S A X Y P -> $a738 $40 $0d $95 $27 $62 }T T{ $a737 M $a738 M $a739 M -> $f3 $87 $3f }T
$5888 >PC $05 >S $15 >A $e8 >X $3c >Y $e3 >P $5888 $f3 >M $5889 $14 >M $588a $70 >M
T{ op PC S A X Y P -> $5889 $05 $15 $e8 $3c $e3 }T T{ $5888 M $5889 M $588a M -> $f3 $14 $70 }T
( f4 )
$a9e3 >PC $5f >S $d5 >A $dd >X $ba >Y $e1 >P $000d $1c >M $0030 $03 >M $a9e3 $f4 >M $a9e4 $30 >M $a9e5 $27 >M
T{ op PC S A X Y P -> $a9e5 $5f $d5 $dd $ba $e1 }T T{ $000d M $0030 M $a9e3 M $a9e4 M $a9e5 M -> $1c $03 $f4 $30 $27 }T
$c05e >PC $35 >S $d5 >A $b5 >X $86 >Y $a1 >P $002d $67 >M $0078 $bf >M $c05e $f4 >M $c05f $78 >M $c060 $81 >M
T{ op PC S A X Y P -> $c060 $35 $d5 $b5 $86 $a1 }T T{ $002d M $0078 M $c05e M $c05f M $c060 M -> $67 $bf $f4 $78 $81 }T
$b5d2 >PC $32 >S $0b >A $15 >X $0c >Y $e7 >P $0054 $96 >M $0069 $fa >M $b5d2 $f4 >M $b5d3 $54 >M $b5d4 $79 >M
T{ op PC S A X Y P -> $b5d4 $32 $0b $15 $0c $e7 }T T{ $0054 M $0069 M $b5d2 M $b5d3 M $b5d4 M -> $96 $fa $f4 $54 $79 }T
$ab59 >PC $68 >S $ea >A $84 >X $19 >Y $22 >P $0010 $a0 >M $0094 $15 >M $ab59 $f4 >M $ab5a $10 >M $ab5b $30 >M
T{ op PC S A X Y P -> $ab5b $68 $ea $84 $19 $22 }T T{ $0010 M $0094 M $ab59 M $ab5a M $ab5b M -> $a0 $15 $f4 $10 $30 }T
$ae49 >PC $12 >S $11 >A $4b >X $b7 >Y $e2 >P $003f $df >M $00f4 $05 >M $ae49 $f4 >M $ae4a $f4 >M $ae4b $1b >M
T{ op PC S A X Y P -> $ae4b $12 $11 $4b $b7 $e2 }T T{ $003f M $00f4 M $ae49 M $ae4a M $ae4b M -> $df $05 $f4 $f4 $1b }T
$dc57 >PC $f7 >S $da >A $59 >X $e8 >Y $a0 >P $0020 $a8 >M $00c7 $6e >M $dc57 $f4 >M $dc58 $c7 >M $dc59 $d7 >M
T{ op PC S A X Y P -> $dc59 $f7 $da $59 $e8 $a0 }T T{ $0020 M $00c7 M $dc57 M $dc58 M $dc59 M -> $a8 $6e $f4 $c7 $d7 }T
$b77b >PC $62 >S $09 >A $1b >X $64 >Y $e4 >P $0009 $87 >M $00ee $1c >M $b77b $f4 >M $b77c $ee >M $b77d $3e >M
T{ op PC S A X Y P -> $b77d $62 $09 $1b $64 $e4 }T T{ $0009 M $00ee M $b77b M $b77c M $b77d M -> $87 $1c $f4 $ee $3e }T
$5547 >PC $b7 >S $b5 >A $84 >X $1b >Y $a4 >P $0071 $44 >M $00ed $c8 >M $5547 $f4 >M $5548 $ed >M $5549 $7d >M
T{ op PC S A X Y P -> $5549 $b7 $b5 $84 $1b $a4 }T T{ $0071 M $00ed M $5547 M $5548 M $5549 M -> $44 $c8 $f4 $ed $7d }T
$69ef >PC $23 >S $6f >A $73 >X $fb >Y $60 >P $008c $a5 >M $00ff $4c >M $69ef $f4 >M $69f0 $8c >M $69f1 $2e >M
T{ op PC S A X Y P -> $69f1 $23 $6f $73 $fb $60 }T T{ $008c M $00ff M $69ef M $69f0 M $69f1 M -> $a5 $4c $f4 $8c $2e }T
$943b >PC $bc >S $44 >A $81 >X $93 >Y $a5 >P $0048 $8a >M $00c7 $8f >M $943b $f4 >M $943c $c7 >M $943d $ff >M
T{ op PC S A X Y P -> $943d $bc $44 $81 $93 $a5 }T T{ $0048 M $00c7 M $943b M $943c M $943d M -> $8a $8f $f4 $c7 $ff }T
$779c >PC $53 >S $b6 >A $e4 >X $36 >Y $a5 >P $000e $7c >M $002a $78 >M $779c $f4 >M $779d $2a >M $779e $ff >M
T{ op PC S A X Y P -> $779e $53 $b6 $e4 $36 $a5 }T T{ $000e M $002a M $779c M $779d M $779e M -> $7c $78 $f4 $2a $ff }T
$cee6 >PC $d1 >S $70 >A $2c >X $c5 >Y $24 >P $0039 $d7 >M $0065 $92 >M $cee6 $f4 >M $cee7 $39 >M $cee8 $bb >M
T{ op PC S A X Y P -> $cee8 $d1 $70 $2c $c5 $24 }T T{ $0039 M $0065 M $cee6 M $cee7 M $cee8 M -> $d7 $92 $f4 $39 $bb }T
$22d6 >PC $6a >S $ee >A $4c >X $64 >Y $e6 >P $0085 $cd >M $00d1 $57 >M $22d6 $f4 >M $22d7 $85 >M $22d8 $b0 >M
T{ op PC S A X Y P -> $22d8 $6a $ee $4c $64 $e6 }T T{ $0085 M $00d1 M $22d6 M $22d7 M $22d8 M -> $cd $57 $f4 $85 $b0 }T
$f16e >PC $cc >S $39 >A $1d >X $da >Y $63 >P $001d $21 >M $003a $d9 >M $f16e $f4 >M $f16f $1d >M $f170 $f2 >M
T{ op PC S A X Y P -> $f170 $cc $39 $1d $da $63 }T T{ $001d M $003a M $f16e M $f16f M $f170 M -> $21 $d9 $f4 $1d $f2 }T
$e15c >PC $9d >S $cc >A $eb >X $c1 >Y $e2 >P $0032 $e1 >M $0047 $ae >M $e15c $f4 >M $e15d $47 >M $e15e $74 >M
T{ op PC S A X Y P -> $e15e $9d $cc $eb $c1 $e2 }T T{ $0032 M $0047 M $e15c M $e15d M $e15e M -> $e1 $ae $f4 $47 $74 }T
$2c7a >PC $c2 >S $37 >A $56 >X $ea >Y $e5 >P $0038 $84 >M $008e $e5 >M $2c7a $f4 >M $2c7b $38 >M $2c7c $99 >M
T{ op PC S A X Y P -> $2c7c $c2 $37 $56 $ea $e5 }T T{ $0038 M $008e M $2c7a M $2c7b M $2c7c M -> $84 $e5 $f4 $38 $99 }T
( f5 )
$6b04 >PC $ef >S $4e >A $9a >X $72 >Y $70 >P $000c $23 >M $0072 $30 >M $6b04 $f5 >M $6b05 $72 >M
T{ op PC S A X Y P -> $6b06 $ef $2a $9a $72 $31 }T T{ $000c M $0072 M $6b04 M $6b05 M -> $23 $30 $f5 $72 }T
$6eb7 >PC $96 >S $35 >A $74 >X $b5 >Y $77 >P $007e $24 >M $00f2 $a2 >M $6eb7 $f5 >M $6eb8 $7e >M
T{ op PC S A X Y P -> $6eb9 $96 $93 $74 $b5 $f4 }T T{ $007e M $00f2 M $6eb7 M $6eb8 M -> $24 $a2 $f5 $7e }T
$719a >PC $69 >S $3d >A $2b >X $f3 >Y $b3 >P $009b $84 >M $00c6 $38 >M $719a $f5 >M $719b $9b >M
T{ op PC S A X Y P -> $719c $69 $05 $2b $f3 $31 }T T{ $009b M $00c6 M $719a M $719b M -> $84 $38 $f5 $9b }T
$b7f2 >PC $74 >S $c8 >A $55 >X $8d >Y $31 >P $002d $e1 >M $0082 $28 >M $b7f2 $f5 >M $b7f3 $2d >M
T{ op PC S A X Y P -> $b7f4 $74 $a0 $55 $8d $b1 }T T{ $002d M $0082 M $b7f2 M $b7f3 M -> $e1 $28 $f5 $2d }T
$f7e3 >PC $82 >S $31 >A $a6 >X $13 >Y $b3 >P $001a $44 >M $00c0 $a6 >M $f7e3 $f5 >M $f7e4 $1a >M
T{ op PC S A X Y P -> $f7e5 $82 $8b $a6 $13 $f0 }T T{ $001a M $00c0 M $f7e3 M $f7e4 M -> $44 $a6 $f5 $1a }T
$fa12 >PC $cf >S $f5 >A $27 >X $05 >Y $36 >P $0031 $39 >M $0058 $cb >M $fa12 $f5 >M $fa13 $31 >M
T{ op PC S A X Y P -> $fa14 $cf $29 $27 $05 $35 }T T{ $0031 M $0058 M $fa12 M $fa13 M -> $39 $cb $f5 $31 }T
$072b >PC $2c >S $cd >A $7e >X $25 >Y $b7 >P $0046 $bf >M $00c8 $6f >M $072b $f5 >M $072c $c8 >M
T{ op PC S A X Y P -> $072d $2c $0e $7e $25 $35 }T T{ $0046 M $00c8 M $072b M $072c M -> $bf $6f $f5 $c8 }T
$cc15 >PC $68 >S $7a >A $fd >X $76 >Y $34 >P $0078 $50 >M $007b $9e >M $cc15 $f5 >M $cc16 $7b >M
T{ op PC S A X Y P -> $cc17 $68 $29 $fd $76 $35 }T T{ $0078 M $007b M $cc15 M $cc16 M -> $50 $9e $f5 $7b }T
$519f >PC $e1 >S $d3 >A $08 >X $f5 >Y $32 >P $00a3 $3e >M $00ab $b2 >M $519f $f5 >M $51a0 $a3 >M
T{ op PC S A X Y P -> $51a1 $e1 $20 $08 $f5 $31 }T T{ $00a3 M $00ab M $519f M $51a0 M -> $3e $b2 $f5 $a3 }T
$c7bc >PC $05 >S $72 >A $11 >X $b0 >Y $31 >P $00cd $22 >M $00de $93 >M $c7bc $f5 >M $c7bd $cd >M
T{ op PC S A X Y P -> $c7be $05 $df $11 $b0 $f0 }T T{ $00cd M $00de M $c7bc M $c7bd M -> $22 $93 $f5 $cd }T
$4f00 >PC $e4 >S $32 >A $64 >X $d5 >Y $30 >P $0027 $95 >M $008b $82 >M $4f00 $f5 >M $4f01 $27 >M
T{ op PC S A X Y P -> $4f02 $e4 $af $64 $d5 $f0 }T T{ $0027 M $008b M $4f00 M $4f01 M -> $95 $82 $f5 $27 }T
$8930 >PC $c4 >S $51 >A $89 >X $ff >Y $f3 >P $0058 $15 >M $00cf $44 >M $8930 $f5 >M $8931 $cf >M
T{ op PC S A X Y P -> $8932 $c4 $3c $89 $ff $31 }T T{ $0058 M $00cf M $8930 M $8931 M -> $15 $44 $f5 $cf }T
$7420 >PC $93 >S $e0 >A $bc >X $6f >Y $32 >P $001f $27 >M $0063 $80 >M $7420 $f5 >M $7421 $63 >M
T{ op PC S A X Y P -> $7422 $93 $b8 $bc $6f $b1 }T T{ $001f M $0063 M $7420 M $7421 M -> $27 $80 $f5 $63 }T
$9077 >PC $ec >S $6f >A $06 >X $73 >Y $f4 >P $00b6 $ae >M $00bc $ee >M $9077 $f5 >M $9078 $b6 >M
T{ op PC S A X Y P -> $9079 $ec $80 $06 $73 $f4 }T T{ $00b6 M $00bc M $9077 M $9078 M -> $ae $ee $f5 $b6 }T
$b44a >PC $3b >S $ab >A $a8 >X $c8 >Y $b5 >P $002b $b5 >M $00d3 $9c >M $b44a $f5 >M $b44b $2b >M
T{ op PC S A X Y P -> $b44c $3b $0f $a8 $c8 $35 }T T{ $002b M $00d3 M $b44a M $b44b M -> $b5 $9c $f5 $2b }T
$a17a >PC $95 >S $20 >A $31 >X $3e >Y $b6 >P $004b $4b >M $007c $a5 >M $a17a $f5 >M $a17b $4b >M
T{ op PC S A X Y P -> $a17c $95 $7a $31 $3e $34 }T T{ $004b M $007c M $a17a M $a17b M -> $4b $a5 $f5 $4b }T
( f6 )
$7d4d >PC $66 >S $96 >A $b3 >X $79 >Y $e3 >P $0056 $8a >M $00a3 $c5 >M $7d4d $f6 >M $7d4e $a3 >M $7d4f $1b >M
T{ op PC S A X Y P -> $7d4f $66 $96 $b3 $79 $e1 }T T{ $0056 M $00a3 M $7d4d M $7d4e M $7d4f M -> $8b $c5 $f6 $a3 $1b }T
$2bde >PC $20 >S $e3 >A $9d >X $9c >Y $a6 >P $0006 $8e >M $0069 $90 >M $2bde $f6 >M $2bdf $69 >M $2be0 $e9 >M
T{ op PC S A X Y P -> $2be0 $20 $e3 $9d $9c $a4 }T T{ $0006 M $0069 M $2bde M $2bdf M $2be0 M -> $8f $90 $f6 $69 $e9 }T
$3cc5 >PC $06 >S $d5 >A $ce >X $66 >Y $e4 >P $0056 $b4 >M $0088 $fb >M $3cc5 $f6 >M $3cc6 $88 >M $3cc7 $7c >M
T{ op PC S A X Y P -> $3cc7 $06 $d5 $ce $66 $e4 }T T{ $0056 M $0088 M $3cc5 M $3cc6 M $3cc7 M -> $b5 $fb $f6 $88 $7c }T
$d399 >PC $bd >S $41 >A $39 >X $d1 >Y $e4 >P $0009 $0b >M $0042 $2b >M $d399 $f6 >M $d39a $09 >M $d39b $2b >M
T{ op PC S A X Y P -> $d39b $bd $41 $39 $d1 $64 }T T{ $0009 M $0042 M $d399 M $d39a M $d39b M -> $0b $2c $f6 $09 $2b }T
$8f1e >PC $0c >S $21 >A $77 >X $c4 >Y $67 >P $0067 $22 >M $00de $32 >M $8f1e $f6 >M $8f1f $67 >M $8f20 $83 >M
T{ op PC S A X Y P -> $8f20 $0c $21 $77 $c4 $65 }T T{ $0067 M $00de M $8f1e M $8f1f M $8f20 M -> $22 $33 $f6 $67 $83 }T
$d939 >PC $79 >S $dc >A $6d >X $5e >Y $e2 >P $0038 $6d >M $00a5 $1a >M $d939 $f6 >M $d93a $38 >M $d93b $ff >M
T{ op PC S A X Y P -> $d93b $79 $dc $6d $5e $60 }T T{ $0038 M $00a5 M $d939 M $d93a M $d93b M -> $6d $1b $f6 $38 $ff }T
$6a2a >PC $84 >S $b7 >A $0d >X $00 >Y $a5 >P $0068 $39 >M $0075 $2e >M $6a2a $f6 >M $6a2b $68 >M $6a2c $11 >M
T{ op PC S A X Y P -> $6a2c $84 $b7 $0d $00 $25 }T T{ $0068 M $0075 M $6a2a M $6a2b M $6a2c M -> $39 $2f $f6 $68 $11 }T
$05f9 >PC $7d >S $1a >A $86 >X $ef >Y $26 >P $0014 $5a >M $008e $50 >M $05f9 $f6 >M $05fa $8e >M $05fb $36 >M
T{ op PC S A X Y P -> $05fb $7d $1a $86 $ef $24 }T T{ $0014 M $008e M $05f9 M $05fa M $05fb M -> $5b $50 $f6 $8e $36 }T
$7eb4 >PC $dd >S $69 >A $ad >X $ea >Y $a2 >P $00a7 $41 >M $00fa $57 >M $7eb4 $f6 >M $7eb5 $fa >M $7eb6 $0f >M
T{ op PC S A X Y P -> $7eb6 $dd $69 $ad $ea $20 }T T{ $00a7 M $00fa M $7eb4 M $7eb5 M $7eb6 M -> $42 $57 $f6 $fa $0f }T
$029d >PC $1a >S $54 >A $1a >X $78 >Y $22 >P $0056 $a7 >M $0070 $43 >M $029d $f6 >M $029e $56 >M $029f $59 >M
T{ op PC S A X Y P -> $029f $1a $54 $1a $78 $20 }T T{ $0056 M $0070 M $029d M $029e M $029f M -> $a7 $44 $f6 $56 $59 }T
$199f >PC $b8 >S $2d >A $de >X $67 >Y $64 >P $0006 $93 >M $0028 $79 >M $199f $f6 >M $19a0 $28 >M $19a1 $69 >M
T{ op PC S A X Y P -> $19a1 $b8 $2d $de $67 $e4 }T T{ $0006 M $0028 M $199f M $19a0 M $19a1 M -> $94 $79 $f6 $28 $69 }T
$ae8b >PC $a7 >S $e2 >A $84 >X $cf >Y $23 >P $0079 $2c >M $00f5 $dc >M $ae8b $f6 >M $ae8c $f5 >M $ae8d $d0 >M
T{ op PC S A X Y P -> $ae8d $a7 $e2 $84 $cf $21 }T T{ $0079 M $00f5 M $ae8b M $ae8c M $ae8d M -> $2d $dc $f6 $f5 $d0 }T
$1bd6 >PC $b7 >S $13 >A $78 >X $21 >Y $e3 >P $0018 $ed >M $0090 $6c >M $1bd6 $f6 >M $1bd7 $18 >M $1bd8 $03 >M
T{ op PC S A X Y P -> $1bd8 $b7 $13 $78 $21 $61 }T T{ $0018 M $0090 M $1bd6 M $1bd7 M $1bd8 M -> $ed $6d $f6 $18 $03 }T
$43b2 >PC $81 >S $27 >A $e3 >X $98 >Y $e3 >P $0096 $4a >M $00b3 $40 >M $43b2 $f6 >M $43b3 $b3 >M $43b4 $92 >M
T{ op PC S A X Y P -> $43b4 $81 $27 $e3 $98 $61 }T T{ $0096 M $00b3 M $43b2 M $43b3 M $43b4 M -> $4b $40 $f6 $b3 $92 }T
$380d >PC $bc >S $3c >A $40 >X $7a >Y $26 >P $0016 $55 >M $0056 $7f >M $380d $f6 >M $380e $16 >M $380f $54 >M
T{ op PC S A X Y P -> $380f $bc $3c $40 $7a $a4 }T T{ $0016 M $0056 M $380d M $380e M $380f M -> $55 $80 $f6 $16 $54 }T
$d3b8 >PC $54 >S $ef >A $93 >X $d1 >Y $67 >P $0009 $a0 >M $0076 $a0 >M $d3b8 $f6 >M $d3b9 $76 >M $d3ba $f8 >M
T{ op PC S A X Y P -> $d3ba $54 $ef $93 $d1 $e5 }T T{ $0009 M $0076 M $d3b8 M $d3b9 M $d3ba M -> $a1 $a0 $f6 $76 $f8 }T
( f7 )
$d3f4 >PC $35 >S $26 >A $dd >X $0e >Y $60 >P $009e $72 >M $d3f4 $f7 >M $d3f5 $9e >M $d3f6 $f6 >M
T{ op PC S A X Y P -> $d3f6 $35 $26 $dd $0e $60 }T T{ $009e M $d3f4 M $d3f5 M $d3f6 M -> $f2 $f7 $9e $f6 }T
$c924 >PC $5c >S $55 >A $38 >X $74 >Y $a7 >P $004c $5f >M $c924 $f7 >M $c925 $4c >M $c926 $c4 >M
T{ op PC S A X Y P -> $c926 $5c $55 $38 $74 $a7 }T T{ $004c M $c924 M $c925 M $c926 M -> $df $f7 $4c $c4 }T
$5b32 >PC $c4 >S $5e >A $d1 >X $0e >Y $e3 >P $0061 $64 >M $5b32 $f7 >M $5b33 $61 >M $5b34 $92 >M
T{ op PC S A X Y P -> $5b34 $c4 $5e $d1 $0e $e3 }T T{ $0061 M $5b32 M $5b33 M $5b34 M -> $e4 $f7 $61 $92 }T
$b88c >PC $e8 >S $a6 >A $47 >X $71 >Y $e4 >P $00c9 $49 >M $b88c $f7 >M $b88d $c9 >M $b88e $49 >M
T{ op PC S A X Y P -> $b88e $e8 $a6 $47 $71 $e4 }T T{ $00c9 M $b88c M $b88d M $b88e M -> $c9 $f7 $c9 $49 }T
$9d9f >PC $c3 >S $85 >A $7a >X $7b >Y $e2 >P $002e $77 >M $9d9f $f7 >M $9da0 $2e >M $9da1 $6c >M
T{ op PC S A X Y P -> $9da1 $c3 $85 $7a $7b $e2 }T T{ $002e M $9d9f M $9da0 M $9da1 M -> $f7 $f7 $2e $6c }T
$208f >PC $63 >S $45 >A $d4 >X $09 >Y $67 >P $00e0 $97 >M $208f $f7 >M $2090 $e0 >M $2091 $bc >M
T{ op PC S A X Y P -> $2091 $63 $45 $d4 $09 $67 }T T{ $00e0 M $208f M $2090 M $2091 M -> $97 $f7 $e0 $bc }T
$4a79 >PC $87 >S $f2 >A $97 >X $0d >Y $22 >P $0083 $8f >M $4a79 $f7 >M $4a7a $83 >M $4a7b $53 >M
T{ op PC S A X Y P -> $4a7b $87 $f2 $97 $0d $22 }T T{ $0083 M $4a79 M $4a7a M $4a7b M -> $8f $f7 $83 $53 }T
$7ad0 >PC $0a >S $4b >A $9f >X $46 >Y $a4 >P $0042 $50 >M $7ad0 $f7 >M $7ad1 $42 >M $7ad2 $74 >M
T{ op PC S A X Y P -> $7ad2 $0a $4b $9f $46 $a4 }T T{ $0042 M $7ad0 M $7ad1 M $7ad2 M -> $d0 $f7 $42 $74 }T
$55ff >PC $29 >S $2a >A $d3 >X $3b >Y $e3 >P $00c2 $f3 >M $55ff $f7 >M $5600 $c2 >M $5601 $ec >M
T{ op PC S A X Y P -> $5601 $29 $2a $d3 $3b $e3 }T T{ $00c2 M $55ff M $5600 M $5601 M -> $f3 $f7 $c2 $ec }T
$5ccd >PC $14 >S $ff >A $57 >X $89 >Y $e5 >P $0027 $a5 >M $5ccd $f7 >M $5cce $27 >M $5ccf $2e >M
T{ op PC S A X Y P -> $5ccf $14 $ff $57 $89 $e5 }T T{ $0027 M $5ccd M $5cce M $5ccf M -> $a5 $f7 $27 $2e }T
$85a5 >PC $a7 >S $4b >A $7c >X $4d >Y $a3 >P $0061 $7f >M $85a5 $f7 >M $85a6 $61 >M $85a7 $e1 >M
T{ op PC S A X Y P -> $85a7 $a7 $4b $7c $4d $a3 }T T{ $0061 M $85a5 M $85a6 M $85a7 M -> $ff $f7 $61 $e1 }T
$9ffd >PC $41 >S $cb >A $d6 >X $d6 >Y $60 >P $0013 $c4 >M $9ffd $f7 >M $9ffe $13 >M $9fff $0b >M
T{ op PC S A X Y P -> $9fff $41 $cb $d6 $d6 $60 }T T{ $0013 M $9ffd M $9ffe M $9fff M -> $c4 $f7 $13 $0b }T
$12ae >PC $bf >S $0b >A $04 >X $45 >Y $e5 >P $0001 $21 >M $12ae $f7 >M $12af $01 >M $12b0 $7e >M
T{ op PC S A X Y P -> $12b0 $bf $0b $04 $45 $e5 }T T{ $0001 M $12ae M $12af M $12b0 M -> $a1 $f7 $01 $7e }T
$1024 >PC $9f >S $ac >A $85 >X $05 >Y $a2 >P $00f1 $42 >M $1024 $f7 >M $1025 $f1 >M $1026 $f4 >M
T{ op PC S A X Y P -> $1026 $9f $ac $85 $05 $a2 }T T{ $00f1 M $1024 M $1025 M $1026 M -> $c2 $f7 $f1 $f4 }T
$f21c >PC $c4 >S $4a >A $b0 >X $3f >Y $a1 >P $00dc $e4 >M $f21c $f7 >M $f21d $dc >M $f21e $ab >M
T{ op PC S A X Y P -> $f21e $c4 $4a $b0 $3f $a1 }T T{ $00dc M $f21c M $f21d M $f21e M -> $e4 $f7 $dc $ab }T
$6f0d >PC $f7 >S $93 >A $cf >X $e5 >Y $26 >P $0020 $66 >M $6f0d $f7 >M $6f0e $20 >M $6f0f $f2 >M
T{ op PC S A X Y P -> $6f0f $f7 $93 $cf $e5 $26 }T T{ $0020 M $6f0d M $6f0e M $6f0f M -> $e6 $f7 $20 $f2 }T
( f8 )
$65c7 >PC $e8 >S $aa >A $34 >X $11 >Y $e5 >P $65c7 $f8 >M $65c8 $35 >M $65c9 $e6 >M
T{ op PC S A X Y P -> $65c8 $e8 $aa $34 $11 $ed }T T{ $65c7 M $65c8 M $65c9 M -> $f8 $35 $e6 }T
$b141 >PC $e2 >S $0b >A $2f >X $80 >Y $62 >P $b141 $f8 >M $b142 $41 >M $b143 $77 >M
T{ op PC S A X Y P -> $b142 $e2 $0b $2f $80 $6a }T T{ $b141 M $b142 M $b143 M -> $f8 $41 $77 }T
$f400 >PC $ad >S $8d >A $8b >X $8d >Y $e1 >P $f400 $f8 >M $f401 $4c >M $f402 $b8 >M
T{ op PC S A X Y P -> $f401 $ad $8d $8b $8d $e9 }T T{ $f400 M $f401 M $f402 M -> $f8 $4c $b8 }T
$fc6e >PC $9a >S $c1 >A $9f >X $ee >Y $25 >P $fc6e $f8 >M $fc6f $37 >M $fc70 $0e >M
T{ op PC S A X Y P -> $fc6f $9a $c1 $9f $ee $2d }T T{ $fc6e M $fc6f M $fc70 M -> $f8 $37 $0e }T
$e21a >PC $9f >S $3f >A $ac >X $c8 >Y $25 >P $e21a $f8 >M $e21b $e6 >M $e21c $f3 >M
T{ op PC S A X Y P -> $e21b $9f $3f $ac $c8 $2d }T T{ $e21a M $e21b M $e21c M -> $f8 $e6 $f3 }T
$583a >PC $7b >S $81 >A $6e >X $f6 >Y $e4 >P $583a $f8 >M $583b $96 >M $583c $f0 >M
T{ op PC S A X Y P -> $583b $7b $81 $6e $f6 $ec }T T{ $583a M $583b M $583c M -> $f8 $96 $f0 }T
$e1bc >PC $c6 >S $a7 >A $e4 >X $42 >Y $20 >P $e1bc $f8 >M $e1bd $dd >M $e1be $63 >M
T{ op PC S A X Y P -> $e1bd $c6 $a7 $e4 $42 $28 }T T{ $e1bc M $e1bd M $e1be M -> $f8 $dd $63 }T
$39f1 >PC $1d >S $f4 >A $f4 >X $1e >Y $e6 >P $39f1 $f8 >M $39f2 $9d >M $39f3 $7a >M
T{ op PC S A X Y P -> $39f2 $1d $f4 $f4 $1e $ee }T T{ $39f1 M $39f2 M $39f3 M -> $f8 $9d $7a }T
$4758 >PC $04 >S $6a >A $1c >X $e5 >Y $a0 >P $4758 $f8 >M $4759 $8b >M $475a $f2 >M
T{ op PC S A X Y P -> $4759 $04 $6a $1c $e5 $a8 }T T{ $4758 M $4759 M $475a M -> $f8 $8b $f2 }T
$6668 >PC $f8 >S $12 >A $6b >X $9e >Y $e3 >P $6668 $f8 >M $6669 $c8 >M $666a $cc >M
T{ op PC S A X Y P -> $6669 $f8 $12 $6b $9e $eb }T T{ $6668 M $6669 M $666a M -> $f8 $c8 $cc }T
$0c32 >PC $60 >S $5d >A $4e >X $50 >Y $26 >P $0c32 $f8 >M $0c33 $1c >M $0c34 $d2 >M
T{ op PC S A X Y P -> $0c33 $60 $5d $4e $50 $2e }T T{ $0c32 M $0c33 M $0c34 M -> $f8 $1c $d2 }T
$4c96 >PC $67 >S $8f >A $4b >X $06 >Y $a7 >P $4c96 $f8 >M $4c97 $1a >M $4c98 $73 >M
T{ op PC S A X Y P -> $4c97 $67 $8f $4b $06 $af }T T{ $4c96 M $4c97 M $4c98 M -> $f8 $1a $73 }T
$43d2 >PC $00 >S $0b >A $5a >X $34 >Y $64 >P $43d2 $f8 >M $43d3 $06 >M $43d4 $3c >M
T{ op PC S A X Y P -> $43d3 $00 $0b $5a $34 $6c }T T{ $43d2 M $43d3 M $43d4 M -> $f8 $06 $3c }T
$775f >PC $41 >S $20 >A $91 >X $fa >Y $60 >P $775f $f8 >M $7760 $80 >M $7761 $b9 >M
T{ op PC S A X Y P -> $7760 $41 $20 $91 $fa $68 }T T{ $775f M $7760 M $7761 M -> $f8 $80 $b9 }T
$6fd1 >PC $07 >S $6f >A $d2 >X $b1 >Y $62 >P $6fd1 $f8 >M $6fd2 $22 >M $6fd3 $ca >M
T{ op PC S A X Y P -> $6fd2 $07 $6f $d2 $b1 $6a }T T{ $6fd1 M $6fd2 M $6fd3 M -> $f8 $22 $ca }T
$84a0 >PC $59 >S $04 >A $78 >X $fc >Y $66 >P $84a0 $f8 >M $84a1 $dc >M $84a2 $54 >M
T{ op PC S A X Y P -> $84a1 $59 $04 $78 $fc $6e }T T{ $84a0 M $84a1 M $84a2 M -> $f8 $dc $54 }T
( f9 )
$05ff >PC $8c >S $94 >A $7c >X $af >Y $74 >P $05ff $f9 >M $0600 $fd >M $0601 $13 >M $14ac $bf >M
T{ op PC S A X Y P -> $0602 $8c $d4 $7c $af $b4 }T T{ $05ff M $0600 M $0601 M $14ac M -> $f9 $fd $13 $bf }T
$3570 >PC $52 >S $7c >A $bf >X $52 >Y $f1 >P $3570 $f9 >M $3571 $68 >M $3572 $4d >M $4dba $27 >M
T{ op PC S A X Y P -> $3573 $52 $55 $bf $52 $31 }T T{ $3570 M $3571 M $3572 M $4dba M -> $f9 $68 $4d $27 }T
$f5c0 >PC $27 >S $68 >A $c1 >X $3b >Y $73 >P $abd3 $23 >M $f5c0 $f9 >M $f5c1 $98 >M $f5c2 $ab >M
T{ op PC S A X Y P -> $f5c3 $27 $45 $c1 $3b $31 }T T{ $abd3 M $f5c0 M $f5c1 M $f5c2 M -> $23 $f9 $98 $ab }T
$81a6 >PC $43 >S $51 >A $60 >X $f9 >Y $f3 >P $81a6 $f9 >M $81a7 $5d >M $81a8 $95 >M $9656 $2c >M
T{ op PC S A X Y P -> $81a9 $43 $25 $60 $f9 $31 }T T{ $81a6 M $81a7 M $81a8 M $9656 M -> $f9 $5d $95 $2c }T
$7f6d >PC $6a >S $0a >A $4d >X $63 >Y $b6 >P $3509 $62 >M $7f6d $f9 >M $7f6e $a6 >M $7f6f $34 >M
T{ op PC S A X Y P -> $7f70 $6a $a7 $4d $63 $b4 }T T{ $3509 M $7f6d M $7f6e M $7f6f M -> $62 $f9 $a6 $34 }T
$0387 >PC $6b >S $f8 >A $fd >X $c4 >Y $36 >P $0387 $f9 >M $0388 $b6 >M $0389 $40 >M $417a $6b >M
T{ op PC S A X Y P -> $038a $6b $8c $fd $c4 $b5 }T T{ $0387 M $0388 M $0389 M $417a M -> $f9 $b6 $40 $6b }T
$2e24 >PC $63 >S $32 >A $a9 >X $8b >Y $30 >P $2e24 $f9 >M $2e25 $2c >M $2e26 $2f >M $2fb7 $89 >M
T{ op PC S A X Y P -> $2e27 $63 $a8 $a9 $8b $f0 }T T{ $2e24 M $2e25 M $2e26 M $2fb7 M -> $f9 $2c $2f $89 }T
$b0c7 >PC $fe >S $54 >A $88 >X $2e >Y $f7 >P $8ff3 $5e >M $b0c7 $f9 >M $b0c8 $c5 >M $b0c9 $8f >M
T{ op PC S A X Y P -> $b0ca $fe $f6 $88 $2e $b4 }T T{ $8ff3 M $b0c7 M $b0c8 M $b0c9 M -> $5e $f9 $c5 $8f }T
$c933 >PC $78 >S $5a >A $f2 >X $b1 >Y $33 >P $74d2 $5d >M $c933 $f9 >M $c934 $21 >M $c935 $74 >M
T{ op PC S A X Y P -> $c936 $78 $fd $f2 $b1 $b0 }T T{ $74d2 M $c933 M $c934 M $c935 M -> $5d $f9 $21 $74 }T
$0f8a >PC $15 >S $a3 >A $e7 >X $94 >Y $32 >P $0f8a $f9 >M $0f8b $98 >M $0f8c $d9 >M $da2c $21 >M
T{ op PC S A X Y P -> $0f8d $15 $81 $e7 $94 $b1 }T T{ $0f8a M $0f8b M $0f8c M $da2c M -> $f9 $98 $d9 $21 }T
$5db6 >PC $9d >S $ee >A $08 >X $56 >Y $34 >P $5db6 $f9 >M $5db7 $c5 >M $5db8 $a3 >M $a41b $48 >M
T{ op PC S A X Y P -> $5db9 $9d $a5 $08 $56 $b5 }T T{ $5db6 M $5db7 M $5db8 M $a41b M -> $f9 $c5 $a3 $48 }T
$b819 >PC $ea >S $14 >A $dc >X $95 >Y $f2 >P $3993 $90 >M $b819 $f9 >M $b81a $fe >M $b81b $38 >M
T{ op PC S A X Y P -> $b81c $ea $83 $dc $95 $f0 }T T{ $3993 M $b819 M $b81a M $b81b M -> $90 $f9 $fe $38 }T
$ec75 >PC $b4 >S $d1 >A $a8 >X $96 >Y $74 >P $2a03 $cb >M $ec75 $f9 >M $ec76 $6d >M $ec77 $29 >M
T{ op PC S A X Y P -> $ec78 $b4 $05 $a8 $96 $35 }T T{ $2a03 M $ec75 M $ec76 M $ec77 M -> $cb $f9 $6d $29 }T
$f341 >PC $ef >S $d9 >A $06 >X $e9 >Y $35 >P $66e8 $59 >M $f341 $f9 >M $f342 $ff >M $f343 $65 >M
T{ op PC S A X Y P -> $f344 $ef $80 $06 $e9 $b5 }T T{ $66e8 M $f341 M $f342 M $f343 M -> $59 $f9 $ff $65 }T
$03c2 >PC $48 >S $3e >A $b4 >X $c2 >Y $b2 >P $03c2 $f9 >M $03c3 $ca >M $03c4 $d5 >M $d68c $76 >M
T{ op PC S A X Y P -> $03c5 $48 $c7 $b4 $c2 $b0 }T T{ $03c2 M $03c3 M $03c4 M $d68c M -> $f9 $ca $d5 $76 }T
$a14e >PC $8f >S $94 >A $54 >X $55 >Y $f0 >P $16bd $d4 >M $a14e $f9 >M $a14f $68 >M $a150 $16 >M
T{ op PC S A X Y P -> $a151 $8f $bf $54 $55 $b0 }T T{ $16bd M $a14e M $a14f M $a150 M -> $d4 $f9 $68 $16 }T
( fa )
$aecc >PC $ce >S $67 >A $17 >X $33 >Y $22 >P $01ce $68 >M $01cf $98 >M $aecc $fa >M $aecd $1a >M $aece $13 >M
T{ op PC S A X Y P -> $aecd $cf $67 $98 $33 $a0 }T T{ $01ce M $01cf M $aecc M $aecd M $aece M -> $68 $98 $fa $1a $13 }T
$d8a1 >PC $91 >S $f1 >A $79 >X $80 >Y $a1 >P $0191 $b9 >M $0192 $74 >M $d8a1 $fa >M $d8a2 $cb >M $d8a3 $ec >M
T{ op PC S A X Y P -> $d8a2 $92 $f1 $74 $80 $21 }T T{ $0191 M $0192 M $d8a1 M $d8a2 M $d8a3 M -> $b9 $74 $fa $cb $ec }T
$da13 >PC $fe >S $ba >A $2a >X $e8 >Y $20 >P $01fe $d9 >M $01ff $bd >M $da13 $fa >M $da14 $6d >M $da15 $7c >M
T{ op PC S A X Y P -> $da14 $ff $ba $bd $e8 $a0 }T T{ $01fe M $01ff M $da13 M $da14 M $da15 M -> $d9 $bd $fa $6d $7c }T
$b8d5 >PC $50 >S $be >A $89 >X $26 >Y $e7 >P $0150 $56 >M $0151 $f8 >M $b8d5 $fa >M $b8d6 $d2 >M $b8d7 $ac >M
T{ op PC S A X Y P -> $b8d6 $51 $be $f8 $26 $e5 }T T{ $0150 M $0151 M $b8d5 M $b8d6 M $b8d7 M -> $56 $f8 $fa $d2 $ac }T
$f289 >PC $80 >S $1b >A $b0 >X $eb >Y $23 >P $0180 $d9 >M $0181 $c7 >M $f289 $fa >M $f28a $e1 >M $f28b $f1 >M
T{ op PC S A X Y P -> $f28a $81 $1b $c7 $eb $a1 }T T{ $0180 M $0181 M $f289 M $f28a M $f28b M -> $d9 $c7 $fa $e1 $f1 }T
$7ff3 >PC $06 >S $a7 >A $84 >X $a3 >Y $66 >P $0106 $28 >M $0107 $2c >M $7ff3 $fa >M $7ff4 $98 >M $7ff5 $5c >M
T{ op PC S A X Y P -> $7ff4 $07 $a7 $2c $a3 $64 }T T{ $0106 M $0107 M $7ff3 M $7ff4 M $7ff5 M -> $28 $2c $fa $98 $5c }T
$19dd >PC $63 >S $56 >A $ea >X $ad >Y $e5 >P $0163 $7e >M $0164 $10 >M $19dd $fa >M $19de $1b >M $19df $fb >M
T{ op PC S A X Y P -> $19de $64 $56 $10 $ad $65 }T T{ $0163 M $0164 M $19dd M $19de M $19df M -> $7e $10 $fa $1b $fb }T
$f8e7 >PC $74 >S $d4 >A $17 >X $9e >Y $62 >P $0174 $b9 >M $0175 $12 >M $f8e7 $fa >M $f8e8 $3b >M $f8e9 $bf >M
T{ op PC S A X Y P -> $f8e8 $75 $d4 $12 $9e $60 }T T{ $0174 M $0175 M $f8e7 M $f8e8 M $f8e9 M -> $b9 $12 $fa $3b $bf }T
$45be >PC $79 >S $a4 >A $0d >X $cb >Y $64 >P $0179 $4a >M $017a $ac >M $45be $fa >M $45bf $d8 >M $45c0 $17 >M
T{ op PC S A X Y P -> $45bf $7a $a4 $ac $cb $e4 }T T{ $0179 M $017a M $45be M $45bf M $45c0 M -> $4a $ac $fa $d8 $17 }T
$2961 >PC $af >S $42 >A $86 >X $1e >Y $63 >P $01af $0f >M $01b0 $d3 >M $2961 $fa >M $2962 $96 >M $2963 $45 >M
T{ op PC S A X Y P -> $2962 $b0 $42 $d3 $1e $e1 }T T{ $01af M $01b0 M $2961 M $2962 M $2963 M -> $0f $d3 $fa $96 $45 }T
$42f5 >PC $f4 >S $4d >A $c1 >X $dd >Y $e6 >P $01f4 $c9 >M $01f5 $a0 >M $42f5 $fa >M $42f6 $ec >M $42f7 $99 >M
T{ op PC S A X Y P -> $42f6 $f5 $4d $a0 $dd $e4 }T T{ $01f4 M $01f5 M $42f5 M $42f6 M $42f7 M -> $c9 $a0 $fa $ec $99 }T
$3427 >PC $0e >S $69 >A $94 >X $ce >Y $20 >P $010e $91 >M $010f $d0 >M $3427 $fa >M $3428 $29 >M $3429 $bc >M
T{ op PC S A X Y P -> $3428 $0f $69 $d0 $ce $a0 }T T{ $010e M $010f M $3427 M $3428 M $3429 M -> $91 $d0 $fa $29 $bc }T
$3292 >PC $99 >S $b5 >A $db >X $ed >Y $a5 >P $0199 $96 >M $019a $b2 >M $3292 $fa >M $3293 $90 >M $3294 $fa >M
T{ op PC S A X Y P -> $3293 $9a $b5 $b2 $ed $a5 }T T{ $0199 M $019a M $3292 M $3293 M $3294 M -> $96 $b2 $fa $90 $fa }T
$22d7 >PC $d0 >S $85 >A $00 >X $79 >Y $e2 >P $01d0 $08 >M $01d1 $4a >M $22d7 $fa >M $22d8 $b1 >M $22d9 $62 >M
T{ op PC S A X Y P -> $22d8 $d1 $85 $4a $79 $60 }T T{ $01d0 M $01d1 M $22d7 M $22d8 M $22d9 M -> $08 $4a $fa $b1 $62 }T
$01b0 >PC $55 >S $6e >A $4f >X $4f >Y $60 >P $0155 $23 >M $0156 $39 >M $01b0 $fa >M $01b1 $47 >M $01b2 $b6 >M
T{ op PC S A X Y P -> $01b1 $56 $6e $39 $4f $60 }T T{ $0155 M $0156 M $01b0 M $01b1 M $01b2 M -> $23 $39 $fa $47 $b6 }T
$c965 >PC $79 >S $00 >A $3e >X $f8 >Y $a3 >P $0179 $dd >M $017a $99 >M $c965 $fa >M $c966 $11 >M $c967 $76 >M
T{ op PC S A X Y P -> $c966 $7a $00 $99 $f8 $a1 }T T{ $0179 M $017a M $c965 M $c966 M $c967 M -> $dd $99 $fa $11 $76 }T
( fb )
$3595 >PC $3c >S $e3 >A $f1 >X $f2 >Y $22 >P $3595 $fb >M $3596 $e4 >M $3597 $90 >M
T{ op PC S A X Y P -> $3596 $3c $e3 $f1 $f2 $22 }T T{ $3595 M $3596 M $3597 M -> $fb $e4 $90 }T
$5304 >PC $9f >S $97 >A $5c >X $46 >Y $65 >P $5304 $fb >M $5305 $82 >M $5306 $83 >M
T{ op PC S A X Y P -> $5305 $9f $97 $5c $46 $65 }T T{ $5304 M $5305 M $5306 M -> $fb $82 $83 }T
$b779 >PC $cc >S $ca >A $53 >X $64 >Y $a1 >P $b779 $fb >M $b77a $9d >M $b77b $34 >M
T{ op PC S A X Y P -> $b77a $cc $ca $53 $64 $a1 }T T{ $b779 M $b77a M $b77b M -> $fb $9d $34 }T
$3306 >PC $f0 >S $2c >A $27 >X $0e >Y $27 >P $3306 $fb >M $3307 $45 >M $3308 $d0 >M
T{ op PC S A X Y P -> $3307 $f0 $2c $27 $0e $27 }T T{ $3306 M $3307 M $3308 M -> $fb $45 $d0 }T
$b709 >PC $ac >S $92 >A $b6 >X $ab >Y $63 >P $b709 $fb >M $b70a $6b >M $b70b $7a >M
T{ op PC S A X Y P -> $b70a $ac $92 $b6 $ab $63 }T T{ $b709 M $b70a M $b70b M -> $fb $6b $7a }T
$2992 >PC $5a >S $3d >A $89 >X $2b >Y $a3 >P $2992 $fb >M $2993 $e8 >M $2994 $d6 >M
T{ op PC S A X Y P -> $2993 $5a $3d $89 $2b $a3 }T T{ $2992 M $2993 M $2994 M -> $fb $e8 $d6 }T
$2435 >PC $89 >S $0c >A $53 >X $d8 >Y $a3 >P $2435 $fb >M $2436 $97 >M $2437 $6b >M
T{ op PC S A X Y P -> $2436 $89 $0c $53 $d8 $a3 }T T{ $2435 M $2436 M $2437 M -> $fb $97 $6b }T
$d3a3 >PC $25 >S $27 >A $d9 >X $fb >Y $a5 >P $d3a3 $fb >M $d3a4 $e3 >M $d3a5 $e8 >M
T{ op PC S A X Y P -> $d3a4 $25 $27 $d9 $fb $a5 }T T{ $d3a3 M $d3a4 M $d3a5 M -> $fb $e3 $e8 }T
$8763 >PC $b0 >S $22 >A $1a >X $09 >Y $e7 >P $8763 $fb >M $8764 $02 >M $8765 $b5 >M
T{ op PC S A X Y P -> $8764 $b0 $22 $1a $09 $e7 }T T{ $8763 M $8764 M $8765 M -> $fb $02 $b5 }T
$3fc9 >PC $c9 >S $e0 >A $56 >X $3e >Y $27 >P $3fc9 $fb >M $3fca $c8 >M $3fcb $33 >M
T{ op PC S A X Y P -> $3fca $c9 $e0 $56 $3e $27 }T T{ $3fc9 M $3fca M $3fcb M -> $fb $c8 $33 }T
$6c98 >PC $c1 >S $78 >A $6d >X $a3 >Y $23 >P $6c98 $fb >M $6c99 $d4 >M $6c9a $95 >M
T{ op PC S A X Y P -> $6c99 $c1 $78 $6d $a3 $23 }T T{ $6c98 M $6c99 M $6c9a M -> $fb $d4 $95 }T
$ff0e >PC $a2 >S $93 >A $f7 >X $80 >Y $62 >P $ff0e $fb >M $ff0f $b1 >M $ff10 $12 >M
T{ op PC S A X Y P -> $ff0f $a2 $93 $f7 $80 $62 }T T{ $ff0e M $ff0f M $ff10 M -> $fb $b1 $12 }T
$f9ea >PC $72 >S $a7 >A $25 >X $84 >Y $60 >P $f9ea $fb >M $f9eb $11 >M $f9ec $ea >M
T{ op PC S A X Y P -> $f9eb $72 $a7 $25 $84 $60 }T T{ $f9ea M $f9eb M $f9ec M -> $fb $11 $ea }T
$c48f >PC $0a >S $e9 >A $43 >X $0e >Y $a7 >P $c48f $fb >M $c490 $1b >M $c491 $5b >M
T{ op PC S A X Y P -> $c490 $0a $e9 $43 $0e $a7 }T T{ $c48f M $c490 M $c491 M -> $fb $1b $5b }T
$19fc >PC $f8 >S $9b >A $52 >X $ec >Y $a4 >P $19fc $fb >M $19fd $59 >M $19fe $e6 >M
T{ op PC S A X Y P -> $19fd $f8 $9b $52 $ec $a4 }T T{ $19fc M $19fd M $19fe M -> $fb $59 $e6 }T
$4cd9 >PC $47 >S $9f >A $05 >X $dd >Y $a5 >P $4cd9 $fb >M $4cda $88 >M $4cdb $02 >M
T{ op PC S A X Y P -> $4cda $47 $9f $05 $dd $a5 }T T{ $4cd9 M $4cda M $4cdb M -> $fb $88 $02 }T
( fc )
$4b8e >PC $f3 >S $88 >A $20 >X $42 >Y $24 >P $4b8e $fc >M $4b8f $28 >M $4b90 $98 >M $4b91 $99 >M
T{ op PC S A X Y P -> $4b91 $f3 $88 $20 $42 $24 }T T{ $4b8e M $4b8f M $4b90 M $4b91 M -> $fc $28 $98 $99 }T
$86e5 >PC $ee >S $d6 >A $fd >X $2d >Y $23 >P $86e5 $fc >M $86e6 $c3 >M $86e7 $d0 >M $86e8 $35 >M
T{ op PC S A X Y P -> $86e8 $ee $d6 $fd $2d $23 }T T{ $86e5 M $86e6 M $86e7 M $86e8 M -> $fc $c3 $d0 $35 }T
$dcb9 >PC $7b >S $72 >A $b8 >X $d6 >Y $65 >P $dcb9 $fc >M $dcba $1b >M $dcbb $f5 >M $dcbc $4d >M
T{ op PC S A X Y P -> $dcbc $7b $72 $b8 $d6 $65 }T T{ $dcb9 M $dcba M $dcbb M $dcbc M -> $fc $1b $f5 $4d }T
$78a5 >PC $93 >S $2a >A $f5 >X $00 >Y $21 >P $78a5 $fc >M $78a6 $c4 >M $78a7 $70 >M $78a8 $ac >M
T{ op PC S A X Y P -> $78a8 $93 $2a $f5 $00 $21 }T T{ $78a5 M $78a6 M $78a7 M $78a8 M -> $fc $c4 $70 $ac }T
$e982 >PC $f0 >S $b5 >A $2e >X $18 >Y $e5 >P $e982 $fc >M $e983 $90 >M $e984 $32 >M $e985 $64 >M
T{ op PC S A X Y P -> $e985 $f0 $b5 $2e $18 $e5 }T T{ $e982 M $e983 M $e984 M $e985 M -> $fc $90 $32 $64 }T
$8532 >PC $c2 >S $2b >A $a7 >X $81 >Y $e7 >P $8532 $fc >M $8533 $79 >M $8534 $12 >M $8535 $df >M
T{ op PC S A X Y P -> $8535 $c2 $2b $a7 $81 $e7 }T T{ $8532 M $8533 M $8534 M $8535 M -> $fc $79 $12 $df }T
$8329 >PC $20 >S $2e >A $61 >X $a9 >Y $62 >P $8329 $fc >M $832a $6c >M $832b $ec >M $832c $8c >M
T{ op PC S A X Y P -> $832c $20 $2e $61 $a9 $62 }T T{ $8329 M $832a M $832b M $832c M -> $fc $6c $ec $8c }T
$ccaa >PC $5b >S $86 >A $0d >X $8d >Y $e5 >P $ccaa $fc >M $ccab $98 >M $ccac $22 >M $ccad $e5 >M
T{ op PC S A X Y P -> $ccad $5b $86 $0d $8d $e5 }T T{ $ccaa M $ccab M $ccac M $ccad M -> $fc $98 $22 $e5 }T
$088b >PC $b5 >S $29 >A $44 >X $22 >Y $27 >P $088b $fc >M $088c $3b >M $088d $2f >M $088e $08 >M
T{ op PC S A X Y P -> $088e $b5 $29 $44 $22 $27 }T T{ $088b M $088c M $088d M $088e M -> $fc $3b $2f $08 }T
$1fc9 >PC $d6 >S $13 >A $8d >X $b4 >Y $24 >P $1fc9 $fc >M $1fca $66 >M $1fcb $34 >M $1fcc $e6 >M
T{ op PC S A X Y P -> $1fcc $d6 $13 $8d $b4 $24 }T T{ $1fc9 M $1fca M $1fcb M $1fcc M -> $fc $66 $34 $e6 }T
$2d76 >PC $2f >S $e6 >A $ee >X $61 >Y $63 >P $2d76 $fc >M $2d77 $6c >M $2d78 $15 >M $2d79 $ba >M
T{ op PC S A X Y P -> $2d79 $2f $e6 $ee $61 $63 }T T{ $2d76 M $2d77 M $2d78 M $2d79 M -> $fc $6c $15 $ba }T
$d6e7 >PC $02 >S $a4 >A $61 >X $46 >Y $26 >P $d6e7 $fc >M $d6e8 $36 >M $d6e9 $f5 >M $d6ea $95 >M
T{ op PC S A X Y P -> $d6ea $02 $a4 $61 $46 $26 }T T{ $d6e7 M $d6e8 M $d6e9 M $d6ea M -> $fc $36 $f5 $95 }T
$bb55 >PC $64 >S $e3 >A $88 >X $ef >Y $20 >P $bb55 $fc >M $bb56 $fa >M $bb57 $9e >M $bb58 $23 >M
T{ op PC S A X Y P -> $bb58 $64 $e3 $88 $ef $20 }T T{ $bb55 M $bb56 M $bb57 M $bb58 M -> $fc $fa $9e $23 }T
$07a0 >PC $27 >S $1b >A $d3 >X $82 >Y $21 >P $07a0 $fc >M $07a1 $ad >M $07a2 $30 >M $07a3 $44 >M
T{ op PC S A X Y P -> $07a3 $27 $1b $d3 $82 $21 }T T{ $07a0 M $07a1 M $07a2 M $07a3 M -> $fc $ad $30 $44 }T
$71d0 >PC $67 >S $bc >A $23 >X $ea >Y $22 >P $71d0 $fc >M $71d1 $e2 >M $71d2 $81 >M $71d3 $b4 >M
T{ op PC S A X Y P -> $71d3 $67 $bc $23 $ea $22 }T T{ $71d0 M $71d1 M $71d2 M $71d3 M -> $fc $e2 $81 $b4 }T
$f4d9 >PC $5e >S $3d >A $50 >X $46 >Y $e5 >P $f4d9 $fc >M $f4da $be >M $f4db $26 >M $f4dc $7c >M
T{ op PC S A X Y P -> $f4dc $5e $3d $50 $46 $e5 }T T{ $f4d9 M $f4da M $f4db M $f4dc M -> $fc $be $26 $7c }T
( fd )
$9e04 >PC $ee >S $fe >A $e4 >X $59 >Y $73 >P $77a8 $5e >M $9e04 $fd >M $9e05 $c4 >M $9e06 $76 >M
T{ op PC S A X Y P -> $9e07 $ee $a0 $e4 $59 $b1 }T T{ $77a8 M $9e04 M $9e05 M $9e06 M -> $5e $fd $c4 $76 }T
$8995 >PC $d8 >S $0c >A $b9 >X $c7 >Y $f5 >P $74f8 $ae >M $8995 $fd >M $8996 $3f >M $8997 $74 >M
T{ op PC S A X Y P -> $8998 $d8 $5e $b9 $c7 $34 }T T{ $74f8 M $8995 M $8996 M $8997 M -> $ae $fd $3f $74 }T
$fb9d >PC $07 >S $dc >A $75 >X $a0 >Y $30 >P $abcd $65 >M $fb9d $fd >M $fb9e $58 >M $fb9f $ab >M
T{ op PC S A X Y P -> $fba0 $07 $76 $75 $a0 $71 }T T{ $abcd M $fb9d M $fb9e M $fb9f M -> $65 $fd $58 $ab }T
$5d08 >PC $0b >S $27 >A $15 >X $75 >Y $74 >P $1c47 $bb >M $5d08 $fd >M $5d09 $32 >M $5d0a $1c >M
T{ op PC S A X Y P -> $5d0b $0b $6b $15 $75 $34 }T T{ $1c47 M $5d08 M $5d09 M $5d0a M -> $bb $fd $32 $1c }T
$799f >PC $9e >S $6a >A $59 >X $f9 >Y $f0 >P $4772 $5f >M $799f $fd >M $79a0 $19 >M $79a1 $47 >M
T{ op PC S A X Y P -> $79a2 $9e $0a $59 $f9 $31 }T T{ $4772 M $799f M $79a0 M $79a1 M -> $5f $fd $19 $47 }T
$6a87 >PC $24 >S $1b >A $3b >X $cc >Y $f0 >P $6a87 $fd >M $6a88 $45 >M $6a89 $dc >M $dc80 $4d >M
T{ op PC S A X Y P -> $6a8a $24 $cd $3b $cc $b0 }T T{ $6a87 M $6a88 M $6a89 M $dc80 M -> $fd $45 $dc $4d }T
$a16a >PC $7d >S $d1 >A $d9 >X $81 >Y $36 >P $a16a $fd >M $a16b $9b >M $a16c $d5 >M $d674 $d9 >M
T{ op PC S A X Y P -> $a16d $7d $f7 $d9 $81 $b4 }T T{ $a16a M $a16b M $a16c M $d674 M -> $fd $9b $d5 $d9 }T
$4073 >PC $bb >S $91 >A $e8 >X $8c >Y $32 >P $4073 $fd >M $4074 $28 >M $4075 $a0 >M $a110 $ad >M
T{ op PC S A X Y P -> $4076 $bb $e3 $e8 $8c $b0 }T T{ $4073 M $4074 M $4075 M $a110 M -> $fd $28 $a0 $ad }T
$ce8d >PC $6f >S $13 >A $e9 >X $1b >Y $b6 >P $5b2b $d5 >M $ce8d $fd >M $ce8e $42 >M $ce8f $5a >M
T{ op PC S A X Y P -> $ce90 $6f $3d $e9 $1b $34 }T T{ $5b2b M $ce8d M $ce8e M $ce8f M -> $d5 $fd $42 $5a }T
$1ecf >PC $c7 >S $2f >A $74 >X $58 >Y $77 >P $15ac $42 >M $1ecf $fd >M $1ed0 $38 >M $1ed1 $15 >M
T{ op PC S A X Y P -> $1ed2 $c7 $ed $74 $58 $b4 }T T{ $15ac M $1ecf M $1ed0 M $1ed1 M -> $42 $fd $38 $15 }T
$a36b >PC $6f >S $51 >A $d0 >X $db >Y $71 >P $76cb $78 >M $a36b $fd >M $a36c $fb >M $a36d $75 >M
T{ op PC S A X Y P -> $a36e $6f $d9 $d0 $db $b0 }T T{ $76cb M $a36b M $a36c M $a36d M -> $78 $fd $fb $75 }T
$5b6c >PC $d7 >S $05 >A $93 >X $7c >Y $b1 >P $3dc7 $0f >M $5b6c $fd >M $5b6d $34 >M $5b6e $3d >M
T{ op PC S A X Y P -> $5b6f $d7 $f6 $93 $7c $b0 }T T{ $3dc7 M $5b6c M $5b6d M $5b6e M -> $0f $fd $34 $3d }T
$bdb7 >PC $ab >S $07 >A $75 >X $c6 >Y $74 >P $6e4f $9c >M $bdb7 $fd >M $bdb8 $da >M $bdb9 $6d >M
T{ op PC S A X Y P -> $bdba $ab $6a $75 $c6 $34 }T T{ $6e4f M $bdb7 M $bdb8 M $bdb9 M -> $9c $fd $da $6d }T
$d007 >PC $88 >S $34 >A $c4 >X $6d >Y $71 >P $d007 $fd >M $d008 $1c >M $d009 $fe >M $fee0 $5f >M
T{ op PC S A X Y P -> $d00a $88 $d5 $c4 $6d $b0 }T T{ $d007 M $d008 M $d009 M $fee0 M -> $fd $1c $fe $5f }T
$f010 >PC $1b >S $1a >A $f0 >X $e0 >Y $f1 >P $d88b $d1 >M $f010 $fd >M $f011 $9b >M $f012 $d7 >M
T{ op PC S A X Y P -> $f013 $1b $49 $f0 $e0 $30 }T T{ $d88b M $f010 M $f011 M $f012 M -> $d1 $fd $9b $d7 }T
$ce96 >PC $39 >S $8c >A $4f >X $86 >Y $f1 >P $4e8b $c9 >M $ce96 $fd >M $ce97 $3c >M $ce98 $4e >M
T{ op PC S A X Y P -> $ce99 $39 $c3 $4f $86 $b0 }T T{ $4e8b M $ce96 M $ce97 M $ce98 M -> $c9 $fd $3c $4e }T
( fe )
$5632 >PC $cd >S $20 >A $55 >X $72 >Y $a1 >P $3021 $7e >M $5632 $fe >M $5633 $cc >M $5634 $2f >M $5635 $4f >M
T{ op PC S A X Y P -> $5635 $cd $20 $55 $72 $21 }T T{ $3021 M $5632 M $5633 M $5634 M $5635 M -> $7f $fe $cc $2f $4f }T
$85b6 >PC $6b >S $0b >A $f3 >X $b7 >Y $e7 >P $05a5 $a0 >M $85b6 $fe >M $85b7 $b2 >M $85b8 $04 >M $85b9 $67 >M
T{ op PC S A X Y P -> $85b9 $6b $0b $f3 $b7 $e5 }T T{ $05a5 M $85b6 M $85b7 M $85b8 M $85b9 M -> $a1 $fe $b2 $04 $67 }T
$fe3a >PC $fc >S $55 >A $8f >X $96 >Y $a1 >P $2596 $6e >M $fe3a $fe >M $fe3b $07 >M $fe3c $25 >M $fe3d $cc >M
T{ op PC S A X Y P -> $fe3d $fc $55 $8f $96 $21 }T T{ $2596 M $fe3a M $fe3b M $fe3c M $fe3d M -> $6f $fe $07 $25 $cc }T
$cd6c >PC $82 >S $ac >A $dc >X $d2 >Y $a6 >P $29b0 $07 >M $cd6c $fe >M $cd6d $d4 >M $cd6e $28 >M $cd6f $6f >M
T{ op PC S A X Y P -> $cd6f $82 $ac $dc $d2 $24 }T T{ $29b0 M $cd6c M $cd6d M $cd6e M $cd6f M -> $08 $fe $d4 $28 $6f }T
$7aea >PC $06 >S $50 >A $92 >X $53 >Y $e5 >P $7aea $fe >M $7aeb $1c >M $7aec $a8 >M $7aed $aa >M $a8ae $b3 >M
T{ op PC S A X Y P -> $7aed $06 $50 $92 $53 $e5 }T T{ $7aea M $7aeb M $7aec M $7aed M $a8ae M -> $fe $1c $a8 $aa $b4 }T
$e45f >PC $cc >S $cb >A $11 >X $70 >Y $e4 >P $517c $20 >M $e45f $fe >M $e460 $6b >M $e461 $51 >M $e462 $16 >M
T{ op PC S A X Y P -> $e462 $cc $cb $11 $70 $64 }T T{ $517c M $e45f M $e460 M $e461 M $e462 M -> $21 $fe $6b $51 $16 }T
$b184 >PC $59 >S $b5 >A $2d >X $f7 >Y $62 >P $9122 $30 >M $b184 $fe >M $b185 $f5 >M $b186 $90 >M $b187 $4e >M
T{ op PC S A X Y P -> $b187 $59 $b5 $2d $f7 $60 }T T{ $9122 M $b184 M $b185 M $b186 M $b187 M -> $31 $fe $f5 $90 $4e }T
$e752 >PC $de >S $ac >A $b3 >X $b3 >Y $64 >P $d0f9 $b0 >M $e752 $fe >M $e753 $46 >M $e754 $d0 >M $e755 $aa >M
T{ op PC S A X Y P -> $e755 $de $ac $b3 $b3 $e4 }T T{ $d0f9 M $e752 M $e753 M $e754 M $e755 M -> $b1 $fe $46 $d0 $aa }T
$e2c4 >PC $20 >S $37 >A $c8 >X $26 >Y $62 >P $e2c4 $fe >M $e2c5 $63 >M $e2c6 $e4 >M $e2c7 $5c >M $e52b $2a >M
T{ op PC S A X Y P -> $e2c7 $20 $37 $c8 $26 $60 }T T{ $e2c4 M $e2c5 M $e2c6 M $e2c7 M $e52b M -> $fe $63 $e4 $5c $2b }T
$f7bc >PC $f9 >S $55 >A $0b >X $68 >Y $22 >P $ab90 $87 >M $f7bc $fe >M $f7bd $85 >M $f7be $ab >M $f7bf $ad >M
T{ op PC S A X Y P -> $f7bf $f9 $55 $0b $68 $a0 }T T{ $ab90 M $f7bc M $f7bd M $f7be M $f7bf M -> $88 $fe $85 $ab $ad }T
$d27c >PC $e6 >S $8b >A $bb >X $bc >Y $60 >P $0a8b $1b >M $d27c $fe >M $d27d $d0 >M $d27e $09 >M $d27f $a9 >M
T{ op PC S A X Y P -> $d27f $e6 $8b $bb $bc $60 }T T{ $0a8b M $d27c M $d27d M $d27e M $d27f M -> $1c $fe $d0 $09 $a9 }T
$bec2 >PC $31 >S $d4 >A $c4 >X $39 >Y $26 >P $21f8 $6d >M $bec2 $fe >M $bec3 $34 >M $bec4 $21 >M $bec5 $e0 >M
T{ op PC S A X Y P -> $bec5 $31 $d4 $c4 $39 $24 }T T{ $21f8 M $bec2 M $bec3 M $bec4 M $bec5 M -> $6e $fe $34 $21 $e0 }T
$d61d >PC $63 >S $b9 >A $88 >X $8f >Y $e5 >P $3431 $63 >M $d61d $fe >M $d61e $a9 >M $d61f $33 >M $d620 $de >M
T{ op PC S A X Y P -> $d620 $63 $b9 $88 $8f $65 }T T{ $3431 M $d61d M $d61e M $d61f M $d620 M -> $64 $fe $a9 $33 $de }T
$a918 >PC $47 >S $8a >A $8c >X $76 >Y $a1 >P $8002 $47 >M $a918 $fe >M $a919 $76 >M $a91a $7f >M $a91b $2e >M
T{ op PC S A X Y P -> $a91b $47 $8a $8c $76 $21 }T T{ $8002 M $a918 M $a919 M $a91a M $a91b M -> $48 $fe $76 $7f $2e }T
$06f0 >PC $2a >S $5a >A $f6 >X $25 >Y $e3 >P $06f0 $fe >M $06f1 $06 >M $06f2 $e1 >M $06f3 $d5 >M $e1fc $b5 >M
T{ op PC S A X Y P -> $06f3 $2a $5a $f6 $25 $e1 }T T{ $06f0 M $06f1 M $06f2 M $06f3 M $e1fc M -> $fe $06 $e1 $d5 $b6 }T
$b7a9 >PC $1f >S $40 >A $8f >X $0a >Y $a5 >P $2c28 $3d >M $b7a9 $fe >M $b7aa $99 >M $b7ab $2b >M $b7ac $7b >M
T{ op PC S A X Y P -> $b7ac $1f $40 $8f $0a $25 }T T{ $2c28 M $b7a9 M $b7aa M $b7ab M $b7ac M -> $3e $fe $99 $2b $7b }T
( ff )
$6496 >PC $41 >S $03 >A $29 >X $da >Y $a3 >P $00ae $9b >M $6496 $ff >M $6497 $ae >M $6498 $30 >M $64c9 $52 >M
T{ op PC S A X Y P -> $64c9 $41 $03 $29 $da $a3 }T T{ $00ae M $6496 M $6497 M $6498 M $64c9 M -> $9b $ff $ae $30 $52 }T
$bf6a >PC $6b >S $4d >A $32 >X $11 >Y $e2 >P $00e2 $f2 >M $bf3c $e4 >M $bf6a $ff >M $bf6b $e2 >M $bf6c $cf >M
T{ op PC S A X Y P -> $bf3c $6b $4d $32 $11 $e2 }T T{ $00e2 M $bf3c M $bf6a M $bf6b M $bf6c M -> $f2 $e4 $ff $e2 $cf }T
$c603 >PC $62 >S $74 >A $3e >X $c0 >Y $67 >P $00de $f0 >M $c59e $b0 >M $c603 $ff >M $c604 $de >M $c605 $98 >M $c69e $94 >M
T{ op PC S A X Y P -> $c59e $62 $74 $3e $c0 $67 }T T{ $00de M $c59e M $c603 M $c604 M $c605 M $c69e M -> $f0 $b0 $ff $de $98 $94 }T
$7766 >PC $9a >S $95 >A $67 >X $ca >Y $67 >P $002d $d1 >M $774b $95 >M $7766 $ff >M $7767 $2d >M $7768 $e2 >M
T{ op PC S A X Y P -> $774b $9a $95 $67 $ca $67 }T T{ $002d M $774b M $7766 M $7767 M $7768 M -> $d1 $95 $ff $2d $e2 }T
$1be1 >PC $25 >S $90 >A $17 >X $40 >Y $a5 >P $0023 $7c >M $1b0f $14 >M $1be1 $ff >M $1be2 $23 >M $1be3 $2b >M $1be4 $9b >M
T{ op PC S A X Y P -> $1be4 $25 $90 $17 $40 $a5 }T T{ $0023 M $1b0f M $1be1 M $1be2 M $1be3 M $1be4 M -> $7c $14 $ff $23 $2b $9b }T
$25f6 >PC $70 >S $46 >A $46 >X $b9 >Y $20 >P $0003 $b0 >M $2579 $9e >M $25f6 $ff >M $25f7 $03 >M $25f8 $80 >M
T{ op PC S A X Y P -> $2579 $70 $46 $46 $b9 $20 }T T{ $0003 M $2579 M $25f6 M $25f7 M $25f8 M -> $b0 $9e $ff $03 $80 }T
$44d3 >PC $95 >S $db >A $86 >X $d8 >Y $25 >P $00ca $66 >M $4401 $fe >M $44d3 $ff >M $44d4 $ca >M $44d5 $2b >M $44d6 $60 >M
T{ op PC S A X Y P -> $44d6 $95 $db $86 $d8 $25 }T T{ $00ca M $4401 M $44d3 M $44d4 M $44d5 M $44d6 M -> $66 $fe $ff $ca $2b $60 }T
$1abe >PC $2d >S $2a >A $a2 >X $f3 >Y $25 >P $00c1 $d2 >M $1abe $ff >M $1abf $c1 >M $1ac0 $18 >M $1ad9 $48 >M
T{ op PC S A X Y P -> $1ad9 $2d $2a $a2 $f3 $25 }T T{ $00c1 M $1abe M $1abf M $1ac0 M $1ad9 M -> $d2 $ff $c1 $18 $48 }T
$feee >PC $3f >S $7d >A $ac >X $4f >Y $60 >P $00bf $94 >M $fe72 $44 >M $feee $ff >M $feef $bf >M $fef0 $81 >M
T{ op PC S A X Y P -> $fe72 $3f $7d $ac $4f $60 }T T{ $00bf M $fe72 M $feee M $feef M $fef0 M -> $94 $44 $ff $bf $81 }T
$0c1d >PC $f6 >S $f6 >A $cf >X $0a >Y $e0 >P $00a7 $72 >M $0c1d $ff >M $0c1e $a7 >M $0c1f $47 >M $0c20 $e9 >M $0c67 $c6 >M
T{ op PC S A X Y P -> $0c20 $f6 $f6 $cf $0a $e0 }T T{ $00a7 M $0c1d M $0c1e M $0c1f M $0c20 M $0c67 M -> $72 $ff $a7 $47 $e9 $c6 }T
$fcb2 >PC $dc >S $61 >A $e5 >X $45 >Y $e4 >P $0080 $10 >M $fcb2 $ff >M $fcb3 $80 >M $fcb4 $36 >M $fcb5 $9a >M $fceb $98 >M
T{ op PC S A X Y P -> $fcb5 $dc $61 $e5 $45 $e4 }T T{ $0080 M $fcb2 M $fcb3 M $fcb4 M $fcb5 M $fceb M -> $10 $ff $80 $36 $9a $98 }T
$5c50 >PC $4a >S $37 >A $79 >X $d7 >Y $27 >P $0093 $55 >M $5c50 $ff >M $5c51 $93 >M $5c52 $7f >M $5c53 $2e >M $5cd2 $4d >M
T{ op PC S A X Y P -> $5c53 $4a $37 $79 $d7 $27 }T T{ $0093 M $5c50 M $5c51 M $5c52 M $5c53 M $5cd2 M -> $55 $ff $93 $7f $2e $4d }T
$6374 >PC $ea >S $fa >A $81 >X $a7 >Y $25 >P $007b $9c >M $6374 $ff >M $6375 $7b >M $6376 $51 >M $63c8 $c4 >M
T{ op PC S A X Y P -> $63c8 $ea $fa $81 $a7 $25 }T T{ $007b M $6374 M $6375 M $6376 M $63c8 M -> $9c $ff $7b $51 $c4 }T
$d9f8 >PC $81 >S $07 >A $64 >X $47 >Y $64 >P $00fa $a4 >M $d94e $ea >M $d9f8 $ff >M $d9f9 $fa >M $d9fa $53 >M $da4e $6f >M
T{ op PC S A X Y P -> $da4e $81 $07 $64 $47 $64 }T T{ $00fa M $d94e M $d9f8 M $d9f9 M $d9fa M $da4e M -> $a4 $ea $ff $fa $53 $6f }T
$f034 >PC $01 >S $9d >A $8b >X $4c >Y $e0 >P $008d $8d >M $f034 $ff >M $f035 $8d >M $f036 $1a >M $f051 $18 >M
T{ op PC S A X Y P -> $f051 $01 $9d $8b $4c $e0 }T T{ $008d M $f034 M $f035 M $f036 M $f051 M -> $8d $ff $8d $1a $18 }T
$1e21 >PC $c5 >S $e6 >A $a4 >X $26 >Y $a7 >P $00a9 $0e >M $1e21 $ff >M $1e22 $a9 >M $1e23 $94 >M $1e24 $f2 >M $1eb8 $5e >M
T{ op PC S A X Y P -> $1e24 $c5 $e6 $a4 $26 $a7 }T T{ $00a9 M $1e21 M $1e22 M $1e23 M $1e24 M $1eb8 M -> $0e $ff $a9 $94 $f2 $5e }T


bye
