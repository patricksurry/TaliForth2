\ ------------------------------------------------------------------------
testing tools words: .s ? dump name>string see state words

\ Test for BYE not implemented

\ TYPE tests
T{ s" five by five" 2dup capture-output type restore-output compare -> 0 }T

\ .S tests
T{ capture-output .s restore-output s" <0> " compare -> 0 }T
T{ 1 2 3 capture-output .s restore-output s" <3> 1 2 3 " compare -> 1 2 3 0 }T
T{ hex $12345678. capture-output .s restore-output s" <2> 5678 1234 " compare decimal -> $12345678. 0 }T

\ ? tests
variable life  42 life !
T{ capture-output life ? restore-output s" 42 " compare -> 0 }T

\ WORDS should print a bunch of output including a common sequence
T{ capture-output words restore-output nip 1024 2048 within -> true }T
T{ capture-output words restore-output s" drop dup swap" search -rot 2drop -> true }T

\ DUMP tests

:noname blkbuffer 32 bounds do i i c! loop ; execute
T{ blkbuffer 0 capture-output dump restore-output s\" \n0400  " compare -> 0 }T
T{ blkbuffer 3 capture-output dump restore-output s\" \n0400  00 01 02  ..." compare -> 0 }T
T{ blkbuffer 3 capture-output dump restore-output s\" \n0400  00 01 02  ..." compare -> 0 }T
T{ blkbuffer 11 capture-output dump restore-output s\" \n0400  00 01 02 03 04 05 06 07  08 09 0A  ........ ..." compare -> 0 }T
T{ blkbuffer 32 capture-output dump restore-output s\" \n0400  00 01 02 03 04 05 06 07  08 09 0A 0B 0C 0D 0E 0F  ........ ........\n0410  10 11 12 13 14 15 16 17  18 19 1A 1B 1C 1D 1E 1F  ........ ........\n0420  " compare -> 0 }T



\ compare two strings ignoring masked characters from the second string
\ for example 'abcdEf' would compare equal to 'abcd*f' using * as the mask
\ returns 0 if the strings match, -1 if their lengths differ,
\ and otherwise the 1-based index of first mismatch

: compare-masked ( addr1 n1 addr2 n2 ch -- 0 | <>0 )
    >r rot over <> if
        2drop drop r> drop -1 exit
    then
    \ ( addr1 addr2 n ) (R: ch )
    0 do
        \ ( addr1 addr2 )
        over i + c@ over i + c@
        dup r@ <> -rot <> and if
            2drop r> drop i 1+ unloop exit
        then
    loop
    2drop r> drop 0
;

: see-/mod-output s\" \n
nt: ****  xt: **** \n
flags: CO 0 IM 0 AN 0 NN 0 HC 0 | UF 1 ST 0 \n
size (decimal): 26 \n
\n
****  20 ** ** A9 FF 48 20 **  ** 20 ** ** 20 ** ** 20   .**.H * * ** ** \n
****  ** ** 68 D0 05 20 ** **  E8 E8  **h..  .*..\n
\n
****   **** jsr     2 STACK DEPTH CHECK\n
****     FF lda.#\n
****        pha\n
****   **** jsr     >r\n
****   **** jsr     s>d\n
****   **** jsr     r>\n
****   **** jsr     sm/rem\n
****        pla\n
****      5 bne     **** v\n
****   **** jsr     swap\n
****        inx\n
****        inx\n"
;

\ these tests are a little fiddly since the width of some fields can vary based on the base
\ and the starting offset.  HEX is better, but with DECIMAL in disasm it can be sensitive.
\ to debug, add 2DUP DUMP after restore-output and after see-/mod and compare in results.txt
T{ capture-output see /mod restore-output see-/mod-output char * compare-masked -> 0 }T

\ CASE has CO+IM+NN flags

: see-case-output s\" \n
nt: ****  xt: **** \n
flags: CO 1 IM 1 AN 0 NN 1 HC 0 | UF 0 ST 0 \n
size (decimal): 6 \n
\n
****  CA CA 74 00 74 01  ..t.t.\n
\n
****        dex\n
****        dex\n
****      0 stz.zx\n
****      1 stz.zx\n"
;
T{ capture-output see case restore-output see-case-output char * compare-masked -> 0 }T

\ EXIT has AN flag

: see-exit-output s\" \n
nt: ****  xt: **** \n
flags: CO 1 IM 0 AN 1 NN 0 HC 0 | UF 0 ST 0 \n
size (decimal): 1 \n
\n
****  60  `\n
\n
****        rts\n"
;
T{ capture-output see exit restore-output see-exit-output char * compare-masked -> 0 }T

nc-limit @
0 nc-limit !
: disasm-test
    10 0 do $12345678. 2drop s" banana" 2drop i 1 and if leave then loop
    0 0 ?do -1 +loop
;
nc-limit !

: disasm-test-output s\" \n
*****  ***** jsr     LITERAL 10 \n
*****  ***** jsr     0\n
*****  ***** jsr     DO \n
*****  ***** jsr     2LITERAL 305419896 \n
*****  ***** jsr     2drop\n
*****  ***** jmp\n
*****  ***** jsr     SLITERAL ***** 6 \n
*****  ***** jsr     2drop\n
*****  ***** jsr     i\n
*****  ***** jsr     1\n
*****  ***** jsr     and\n
*****  ***** jsr     0BRANCH ***** \n
*****  ***** jmp\n
*****  ***** jsr     LOOP ***** \n
*****  ***** jsr     unloop\n
*****  ***** jsr     0\n
*****  ***** jsr     0\n
*****  ***** jsr     ?DO ***** \n
*****  ***** jsr     DO \n
*****  ***** jsr     LITERAL -1 \n
*****  ***** jsr     +LOOP ***** \n
*****  ***** jsr     unloop\n"
;

T{
    ' disasm-test dup int>name wordsize
    capture-output disasm restore-output 2dup dump
    disasm-test-output 2dup dump char * compare-masked -> 0 }T
