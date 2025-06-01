; Core Forth words related to 16-bit integer aritmetic and bit manipulation

; Tali Forth 2 for the 65c02
; Scot W. Stevenson <scot.stevenson@gmail.com>
; Sam Colwell
; Patrick Surry
; First version: 19. Jan 2014
; This version: 21. Apr 2024


compare_16bit:
        ; """Compare TOS/NOS and return results in form of the 65c02 flags
        ; Adapted from Leventhal "6502 Assembly Language Subroutines", see
        ; also http://www.6502.org/tutorials/compare_beyond.html
        ; For signed numbers, Z signals equality and N which number is larger:
        ;       if TOS = NOS: Z=1 and N=0
        ;       if TOS > NOS: Z=0 and N=0
        ;       if TOS < NOS: Z=0 and N=1
        ; For unsigned numbers, Z signals equality and C which number is larger:
        ;       if TOS = NOS: Z=1 and N=0
        ;       if TOS > NOS: Z=0 and C=1
        ;       if TOS < NOS: Z=0 and C=0
        ; Compared to the book routine, WORD1 (MINUED) is TOS
        ;                               WORD2 (SUBTRAHEND) is NOS
        ; """
                ; Compare LSB first to set the carry flag
                lda 0,x                 ; LSB of TOS
                cmp 2,x                 ; LSB of NOS
                beq _equal

                ; LSBs are not equal, compare MSB
                lda 1,x                 ; MSB of TOS
                sbc 3,x                 ; MSB of NOS
                bvs _overflow
                bra _not_equal
_equal:
                ; Low bytes are equal, so we compare high bytes
                lda 1,x                 ; MSB of TOS
                sbc 3,x                 ; MSB of NOS
                bvc _done
_overflow:
                ; Handle overflow because we use signed numbers
                eor #$80                ; complement negative flag
_not_equal:
                ora #1                  ; set Z=0 since we're not equal
_done:
                rts


; ## ABS ( n -- u ) "Return absolute value of a number"
; ## "abs"  auto  ANS core
        ; """https://forth-standard.org/standard/core/ABS
        ; Return the absolute value of a number.
        ; """
xt_abs:
                jsr underflow_1
w_abs:
                lda 1,x
                bpl _done       ; positive number, easy money!

                ; negative: calculate 0 - n
                sec
                lda #0
                sbc 0,x         ; LSB
                sta 0,x

                lda #0          ; MSB
                sbc 1,x
                sta 1,x

_done:
z_abs:          rts



; ## AND ( n n -- n ) "Logically AND TOS and NOS"
; ## "and"  auto  ANS core
        ; """https://forth-standard.org/standard/core/AND"""
xt_and:
                jsr underflow_2
w_and:
                lda 0,x
                and 2,x
                sta 2,x

                lda 1,x
                and 3,x
                sta 3,x

                inx
                inx

z_and:          rts



; ## EQUAL ( n n -- f ) "See if TOS and NOS are equal"
; ## "="  auto  ANS core
        ; """https://forth-standard.org/standard/core/Equal"""

xt_equal:
                jsr underflow_2
w_equal:
                ldy #0                  ; default not-equal (false)

                lda 0,x                 ; LSB
                cmp 2,x
                bne _not_equal

                lda 1,x                 ; MSB
                cmp 3,x
                bne _not_equal

                dey                     ; equal, set to true

_not_equal:     sty 2,x
                sty 3,x

                inx
                inx

z_equal:        rts



; ## FALSE ( -- f ) "Push flag FALSE to Data Stack"
; ## "false"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/FALSE
        ;
        ; This is a dummy header, FALSE shares the actual code with ZERO.
        ; """



; ## FM_SLASH_MOD ( d n1  -- rem n2 ) "Floored signed division"
; ## "fm/mod"  auto  ANS core
        ; """https://forth-standard.org/standard/core/FMDivMOD
        ; Note that by default, Tali Forth uses SM/REM for most things.
        ;
        ; There are various ways to realize this. We follow EForth with
        ;    DUP 0< DUP >R  IF NEGATE >R DNEGATE R> THEN >R DUP
        ;    0<  IF R@ + THEN  R> UM/MOD R> IF SWAP NEGATE SWAP THEN
        ; See (http://www.forth.org/eforth.html). However you can also
        ; go FM/MOD via SM/REM (http://www.figuk.plus.com/build/arith.htm):
        ;     DUP >R  SM/REM DUP 0< IF SWAP R> + SWAP 1+ ELSE  R> DROP THEN
        ; """

xt_fm_slash_mod:
                jsr underflow_3
w_fm_slash_mod:
                ; if sign of n1 is negative, negate both n1 and d
                stz tmp2        ; default: n is positive
                lda 1,x         ; MSB of n1
                bpl _check_d

                inc tmp2        ; set flag to negative for n1
                jsr w_negate    ; NEGATE
                inx
                inx             ; pretend to push to stack
                jsr w_dnegate   ; DNEGATE
                dex
                dex
_check_d:
                ; If d is negative, add n1 to high cell of d
                lda 3,x         ; MSB of high word of d
                bpl _multiply

                clc
                lda 0,x         ; LSB of n1
                adc 2,x         ; LSB of dh
                sta 2,x

                lda 1,x         ; MSB of n1
                adc 3,x         ; MSB of dh
                sta 3,x

_multiply:
                jsr w_um_slash_mod     ; ( d n1 -- rem n2 )

                ; if n was negative, negate the result
                lda tmp2
                beq _done

                inx             ; pretend that we SWAP
                inx
                jsr w_negate
                dex
                dex
_done:
z_fm_slash_mod: rts



; ## GREATER_THAN ( n n -- f ) "See if NOS is greater than TOS"
; ## ">"  auto  ANS core
        ; """https://forth-standard.org/standard/core/more"""

xt_greater_than:
                jsr underflow_2
w_greater_than:
                ldy #0          ; default false
                jsr compare_16bit

                ; for signed numbers, NOS>TOS gives us Z=0 and N=1
                beq _false
                bpl _false

                ; true
                dey
_false:
                tya

                inx
                inx
                sta 0,x
                sta 1,x

z_greater_than: rts



; ## INVERT ( n -- n ) "Complement of TOS"
; ## "invert"  auto  ANS core
        ; """https://forth-standard.org/standard/core/INVERT"""
xt_invert:
                jsr underflow_1
w_invert:
                lda #$FF
                eor 0,x         ; LSB
                sta 0,x

                lda #$FF
                eor 1,x         ; MSB
                sta 1,x

z_invert:       rts



; ## LESS_THAN ( n m -- f ) "Return true if NOS < TOS"
; ## "<"  auto  ANS core
        ; """https://forth-standard.org/standard/core/less"""

xt_less_than:
                jsr underflow_2
w_less_than:
                ldy #0          ; default false
                jsr compare_16bit

                ; for signed numbers, NOS < TOS if Z=0 and N=0
                beq _false
                bmi _false

                ; true
                dey
_false:
                tya

                inx
                inx
                sta 0,x
                sta 1,x

z_less_than:    rts



; ## LSHIFT ( x u -- u ) "Shift TOS left"
; ## "lshift"  auto  ANS core
        ; """https://forth-standard.org/standard/core/LSHIFT"""

xt_lshift:
                jsr underflow_2
w_lshift:
                ; max shift 16 times
                lda 0,x
                and #%00001111
                beq _done

                tay

_loop:
                asl 2,x
                rol 3,x
                dey
                bne _loop

_done:
                inx
                inx

z_lshift:       rts



; ## M_STAR ( n n -- d ) "16 * 16 --> 32"
; ## "m*"  auto  ANS core
        ; """https://forth-standard.org/standard/core/MTimes
        ; Multiply two 16 bit numbers, producing a 32 bit result. All
        ; values are signed. Adapted from FIG Forth for Tali Forth.
        ;
        ; The original Forth is : M* OVER OVER XOR >R ABS SWAP ABS UM* R> D+- ;
        ; with  : D+- O< IF DNEGATE THEN ;
        ; """

xt_m_star:
                jsr underflow_2
w_m_star:
                ; figure out the sign
                lda 1,x         ; MSB of n1
                eor 3,x         ; MSB of n2

                ; UM* uses all kinds of temporary variables so we don't
                ; risk a conflict but just take the cycle hit and push
                ; this to the stack
                pha

                ; get the absolute value of both numbers so we can feed
                ; them to UM*, which does the real work
                jsr w_abs
                inx             ; temporarily drop TOS
                inx
                jsr w_abs
                dex             ; recover TOS
                dex

                jsr w_um_star          ; ( d )

                ; handle the sign
                pla
                bpl _done

                jsr w_dnegate
_done:
z_m_star:       rts



; ## MAX ( n n -- n ) "Keep larger of two numbers"
; ## "max"  auto  ANS core
        ; """https://forth-standard.org/standard/core/MAX
        ; Compare TOS and NOS and keep which one is larger. Adapted from
        ; Lance A. Leventhal "6502 Assembly Language Subroutines". Negative
        ; Flag indicates which number is larger. See also
        ; http://6502.org/tutorials/compare_instructions.html and
        ; http://www.righto.com/2012/12/the-6502-overflow-flag-explained.html
        ; """

xt_max:
                jsr underflow_2
w_max:
                ; Compare LSB. We do this first to set the carry flag
                lda 0,x         ; LSB of TOS
                cmp 2,x         ; LSB of NOS, this sets the carry

                lda 1,x         ; MSB of TOS
                sbc 3,x         ; MSB of NOS
                bvc _no_overflow

                ; handle overflow, because we use signed numbers
                eor #$80        ; complement negative flag

_no_overflow:
                ; if negative, NOS is larger and needs to be kept
                bmi _keep_nos

                ; move TOS to NOS
                lda 0,x
                sta 2,x
                lda 1,x
                sta 3,x

_keep_nos:
                inx
                inx

z_max:          rts



; ## MIN ( n n -- n ) "Keep smaller of two numbers"
; ## "min"  auto  ANS core
        ; """https://forth-standard.org/standard/core/MIN
        ; Adapted from Lance A. Leventhal "6502 Assembly Language
        ; Subroutines." Negative Flag indicateds which number is larger. See
        ; http://www.righto.com/2012/12/the-6502-overflow-flag-explained.html
        ; """

xt_min:
                jsr underflow_2
w_min:
                ; compare LSB. We do this first to set the carry flag
                lda 0,x         ; LSB of TOS
                cmp 2,x         ; LSB of NOS, this sets carry

                lda 1,x         ; MSB of TOS
                sbc 3,x         ; MSB of NOS
                bvc _no_overflow

                ; handle overflow because we use signed numbers
                eor #$80

_no_overflow:
                ; if negative, NOS is larger and needs to be dumped
                bpl _keep_nos

                ; move TOS to NOS
                lda 0,x
                sta 2,x
                lda 1,x
                sta 3,x

_keep_nos:
                inx
                inx

z_min:          rts



; ## MINUS ( n n -- n ) "Subtract TOS from NOS"
; ## "-"  auto  ANS core
        ; """https://forth-standard.org/standard/core/Minus"""
xt_minus:
                jsr underflow_2
w_minus:
                sec
                lda 2,x         ; LSB
                sbc 0,x
                sta 2,x

                lda 3,x         ; MSB
                sbc 1,x
                sta 3,x

                inx
                inx

z_minus:        rts



; ## MOD ( n1 n2 -- n ) "Divide NOS by TOS and return the remainder"
; ## "mod"  auto  ANS core
        ; """https://forth-standard.org/standard/core/MOD
        ;
        ; The Forth definition of this word is  : MOD /MOD DROP ;
        ; so we just jump to xt_slash_mod and dump the actual result.
        ; """
xt_mod:
                jsr underflow_2
w_mod:
                jsr w_slash_mod

                inx             ; DROP
                inx
z_mod:
                rts



; ## NEGATE ( n -- n ) "Two's complement"
; ## "negate"  auto  ANS core
        ; """https://forth-standard.org/standard/core/NEGATE"""
xt_negate:
                jsr underflow_1
w_negate:
        	lda #0
                sec
                sbc 0,x         ; LSB
                sta 0,x

                lda #0
                sbc 1,x         ; MSB
                sta 1,x

z_negate:       rts



; ## NOT_EQUALS ( n m -- f ) "Return a true flag if TOS != NOS"
; ## "<>"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/ne
        ;
        ; This is just a variant of EQUAL, we code it separately
        ; for speed.
        ; """

xt_not_equals:
                jsr underflow_2
w_not_equals:
                ldy #$ff                 ; default not-equal (true)

                lda 0,x                 ; LSB
                cmp 2,x
                bne _done

                ; LSB is equal
                lda 1,x                 ; MSB
                cmp 3,x
                bne _done

                iny                     ; actually equal (false)
_done:
                inx
                inx
                sty 0,x
                sty 1,x

z_not_equals:   rts



; ## ONE_MINUS ( u -- u-1 ) "Decrease TOS by one"
; ## "1-"  auto  ANS core
        ; """https://forth-standard.org/standard/core/OneMinus"""

xt_one_minus:
                jsr underflow_1
w_one_minus:
                lda 0,x
                bne +
                dec 1,x
+
                dec 0,x

z_one_minus:    rts



; ## ONE_PLUS ( u -- u+1 ) "Increase TOS by one"
; ## "1+"  auto  ANS core
        ; """https://forth-standard.org/standard/core/OnePlus
        ;
        ; Code is shared with CHAR-PLUS
        ; """

xt_char_plus:
xt_one_plus:
                jsr underflow_1
w_char_plus:
w_one_plus:
                inc 0,x
                bne _done
                inc 1,x

_done:
z_char_plus:
z_one_plus:     rts



; ## OR ( m n -- n ) "Logically OR TOS and NOS"
; ## "or"  auto  ANS core
        ; """https://forth-standard.org/standard/core/OR"
xt_or:
                jsr underflow_2
w_or:
                lda 0,x
                ora 2,x
                sta 2,x

                lda 1,x
                ora 3,x
                sta 3,x

                inx
                inx

z_or:           rts



; ## PLUS ( n n -- n ) "Add TOS and NOS"
; ## "+"  auto  ANS core
        ; """https://forth-standard.org/standard/core/Plus"""
xt_plus:
                jsr underflow_2
w_plus:
                clc
                lda 0,x         ; LSB
                adc 2,x
                sta 2,x

                lda 1,x         ; MSB. No CLC, conserve carry bit
                adc 3,x
                sta 3,x

                inx
                inx

z_plus:         rts



; ## PLUS_STORE ( n addr -- ) "Add number to value at given address"
; ## "+!"  auto  ANS core
        ; """https://forth-standard.org/standard/core/PlusStore"""
xt_plus_store:
                jsr underflow_2
w_plus_store:
                clc
                lda (0,x)       ; fetch LSB at addr
                adc 2,x
                sta (0,x)

                inc 0,x         ; addr++
                bne +
                inc 1,x
+
                lda (0,x)       ; fetch MSB
                adc 3,x
                sta (0,x)

                inx
                inx
                inx
                inx

z_plus_store:   rts



; ## RSHIFT ( x u -- x ) "Shift TOS to the right"
; ## "rshift"  auto  ANS core
        ; """https://forth-standard.org/standard/core/RSHIFT"""
xt_rshift:
                jsr underflow_2
w_rshift:
                ; We shift maximal by 16 bits, mask everything else
                lda 0,x
                and #%00001111
                beq _done               ; if 0 shifts, quit

                tay                     ; we could optimize y >= 8 but prob not worth it
_loop:
                lsr 3,x
                ror 2,x
                dey
                bne _loop
_done:
                inx
                inx

z_rshift:       rts



; ## SLASH ( n1 n2 -- n ) "Divide NOS by TOS"
; ## "/"  auto  ANS core
        ; """https://forth-standard.org/standard/core/Div
        ;
        ; Forth code is either  >R S>D R> FM/MOD SWAP DROP
        ; or >R S>D R> SM/REM SWAP DROP -- we use SM/REM in Tali Forth.
        ; This code is currently unoptimized. This code without the SLASH
        ; DROP at the end is /MOD, so we share the code as far as possible.
        ; """

xt_slash:
                jsr underflow_2
w_slash:
                ; With all the multiplication going on, it would be hard to
                ; make sure that one of our temporary variables is not
                ; overwritten. We make sure that doesn't happen by taking the
                ; hit of pushing the flag to the 65c02's stack
                lda #0
                bra slashmod_common

xt_slash_mod:
                jsr underflow_2
w_slash_mod:
                ; Note that /MOD accesses this code
                lda #$FF                ; falls through to _common

slashmod_common:
                pha
                ; rather than >R S>D R> we'll do ( n1 n2 -- d1 n2 ) inline

                lda 0,x                 ; dup but drop leaving ( n1 -- ) with [ ? n2 ] in the wings
                sta $fe,x
                lda 1,x
                sta $ff,x
                inx
                inx
                jsr w_s_to_d            ; sign extend and then recover n2
                dex
                dex

                jsr w_sm_slash_rem      ; SM/REM leaving ( rem quo )

                ; Check flag with SLASH=0, SLASH_MOD=$ff
                pla
                bne _done

                jsr w_nip               ; SLASH discards the remainer
_done:
z_slash_mod:
z_slash:        rts


; ## SLASH_MOD ( n1 n2 -- n3 n4 ) "Divide NOS by TOS with a remainder"
; ## "/mod"  auto  ANS core
        ; """https://forth-standard.org/standard/core/DivMOD
        ;
        ; This is a dummy entry, the actual code is shared with SLASH
        ; """



; ## SM_SLASH_REM ( d n1 -- n2 n3 ) "Symmetric signed division"
; ## "sm/rem"  auto  ANS core
        ; """https://forth-standard.org/standard/core/SMDivREM
        ; Symmetric signed division. Compare FM/MOD. Based on F-PC 3.6
        ; by Ulrich Hoffmann. See http://www.xlerb.de/uho/ansi.seq
        ;
        ; Forth:
        ; OVER >R 2DUP XOR 0< >R ABS >R DABS R> UM/MOD R> ?NEGATE SWAP
        ; R> ?NEGATE SWAP
        ; """

xt_sm_slash_rem:
                jsr underflow_3 ; contains double number
w_sm_slash_rem:
                ; push MSB of high cell of d to Data Stack so we can check
                ; its sign later
                lda 3,x
                pha

                ; XOR the MSB of the high cell of d and n1 so we figure out
                ; its sign later as well
                lda 1,x
                eor 3,x
                pha

                ; Prepare division by getting absolute of n1 and d
                jsr w_abs
                inx             ; pretend we pushed n1 to R
                inx

                jsr w_dabs
                dex
                dex

                jsr w_um_slash_mod     ; UM/MOD

                ; if the XOR compiled above is negative, negate the
                ; quotient (n3)
                pla
                bpl +
                jsr w_negate
+
                ; if d was negative, negate the remainder (n2)
                pla
                bpl _done

                inx             ; pretend we pushed quotient to R
                inx
                jsr w_negate
                dex
                dex

_done:
z_sm_slash_rem: rts



; ## STAR ( n n -- n ) "16*16 --> 16 "
; ## "*"  auto  ANS core
        ; """https://forth-standard.org/standard/core/Times
        ; Multiply two signed 16 bit numbers, returning a 16 bit result.
        ;
        ; This is nothing  more than UM* DROP
        ; """

xt_star:
                jsr underflow_2
w_star:
                jsr w_um_star
                inx
                inx

z_star:         rts



; ## STAR_SLASH  ( n1 n2 n3 -- n4 ) "n1 * n2 / n3 -->  n"
; ## "*/"  auto  ANS core
        ; """https://forth-standard.org/standard/core/TimesDiv
        ; Multiply n1 by n2 and divide by n3, returning the result
        ; without a remainder. This is */MOD without the mod.
        ;
        ; This word
        ; can be defined in Forth as : */  */MOD SWAP DROP ; which is
        ; pretty much what we do here
        ; """
xt_star_slash:
                jsr underflow_3
w_star_slash:
                jsr w_star_slash_mod
                jsr w_swap
                inx
                inx
z_star_slash:
                rts


; ## STAR_SLASH_MOD  ( n1 n2 n3 -- n4 n5 ) "n1 * n2 / n3 --> n-mod n"
; ## "*/mod"  auto  ANS core
        ; """https://forth-standard.org/standard/core/TimesDivMOD
        ; Multiply n1 by n2 producing the intermediate double-cell result d.
        ; Divide d by n3 producing the single-cell remainder n4 and the
        ; single-cell quotient n5.
        ;
        ; In Forth, this is
        ; : */MOD  >R M* >R SM/REM ;  Note that */ accesses this routine.
        ; """
xt_star_slash_mod:
                jsr underflow_3
w_star_slash_mod:
                inx                     ; pretend to push to stack
                inx
                jsr w_m_star            ; doesn't use further stack space
                dex
                dex
                jsr w_sm_slash_rem

z_star_slash_mod:
                rts



; ## TRUE ( -- f ) "Push TRUE flag to Data Stack"
; ## "true"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/TRUE"""
xt_true:
w_true:
                dex
                dex
                lda #$FF
                sta 0,x
                sta 1,x

z_true:         rts



; ## TWO_SLASH ( n -- n ) "Divide TOS by two"
; ## "2/"  auto  ANS core
        ; """https://forth-standard.org/standard/core/TwoDiv"""
xt_two_slash:
                jsr underflow_1
w_two_slash:
                ; We can't just LSR the LSB and ROR the MSB because that
                ; would do bad things to the sign
                lda 1,x
                asl                     ; save the sign
                ror 1,x
                ror 0,x

z_two_slash:    rts



; ## TWO_STAR ( n -- n ) "Multiply TOS by two"
; ## "2*"  auto  ANS core
        ; """https://forth-standard.org/standard/core/TwoTimes
        ;
        ; Also used for CELLS
        ; """
xt_two_star:
xt_cells:
                jsr underflow_1
w_two_star:
w_cells:
                asl 0,x
                rol 1,x
z_cells:
z_two_star:     rts



; ## U_GREATER_THAN ( n m -- f ) "Return true if NOS > TOS (unsigned)"
; ## "u>"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/Umore"""
xt_u_greater_than:
                jsr underflow_2
w_u_greater_than:
                lda 0,x
                cmp 2,x
                lda 1,x
                sbc 3,x
                inx
                inx

                lda #0
                adc #$FF
                sta 0,x         ; store flag
                sta 1,x

z_u_greater_than:    rts



; ## U_LESS_THAN ( n m -- f ) "Return true if NOS < TOS (unsigned)"
; ## "u<"  auto  ANS core
        ; """https://forth-standard.org/standard/core/Uless"""
xt_u_less_than:
                jsr underflow_2
w_u_less_than:
                lda 2,x
                cmp 0,x
                lda 3,x
                sbc 1,x
                inx
                inx

                lda #0
                adc #$FF
                sta 0,x         ; store flag
                sta 1,x

z_u_less_than:    rts



; ## UM_SLASH_MOD ( ud u -- ur u ) "32/16 -> 16 division"
; ## "um/mod"  auto  ANS core
        ; """https://forth-standard.org/standard/core/UMDivMOD
        ; Divide double cell number by single cell number, returning the
        ; quotient as TOS and any remainder as NOS. All numbers are unsigned.
        ; This is the basic division operation all others use. Based on FIG
        ; Forth code, modified by Garth Wilson, see
        ; http://6502.org/source/integers/ummodfix/ummodfix.htm
        ;
        ; This uses tmpdsp but otherwise works in place
        ; """

xt_um_slash_mod:
                jsr underflow_3
w_um_slash_mod:
                ; catch division by zero
                lda 0,x
                ora 1,x
                bne _not_zero

                lda #err_divzero
                jmp error

                ; note we don't check for the overflow condition that occurs
                ; when the divisor is less than the high word of the dividend,
                ; ie. when the quotient would be more than 16 bits

                ; During the main part of the routine we have the following
                ; stack layout.  We're essentially doing binary long division
                ; (see https://en.wikipedia.org/wiki/Binary_number#Division).
                ; At each step we check whether the divisor fits into the
                ; top word of the dividend, while rolling the dividend left one bit,
                ; and rolling our result bits in from the right.
                ; Eventually we're left with the remainder TOS and quotient NOS:
                ;
                ;       +-----------+-----------+-----------+
                ;       |    TOS    |    NOS    |    3OS    |
                ;       | 0,x | 1,x | 2,x | 3,x | 4,x | 5,x |
                ;       +-----+-----+-----+-----+-----+-----+
                ;       |  divisor  |       dividend        |
                ;       | ulo   uhi | ud2   ud3   ud0   ud1 |
                ;       +-----------+-----------+-----------+
                ;                   | remainder | quotient  |
                ;                   | rlo   rhi | qlo   qhi |
                ;                   +-----------+-----------+
                ;
                ; Finally we do DROP, SWAP leaving the desired result:
                ;
                ;       +-----------+-----------+
                ;       |    TOS    |    NOS    |
                ;       | 0,x | 1,x | 2,x | 3,x |
                ;       +-----+-----+-----+-----+
                ;       | quotient  | remainder |
                ;       | qlo   qhi | rlo   rhi |
                ;       +-----------+-----------+

_not_zero:
                ; We loop 17 times
                ldy #17

                ; because we're often dividing a word that's been
                ; extended to a double via S>D, it's worth doing a
                ; fast pre-loop until we see a non-zero high dividend

                lda 2,x                 ; is high part of dividend zero?
                ora 3,x
                bne _loop               ; nope, carry on...

_while_zero:    rol 4,x                 ; roll the bottom word
                rol 5,x
                dey
                beq _done
                bcc _while_zero         ; until we get a high bit

                rol 2,x                 ; enter the bit into the high part
                bra _maybe              ; start the real work

_loop:
                ; rotate low cell of dividend one bit left (LSB)
                ; entering the last result bit from the carry
                ; NB. the arbitrary bit on pass one is discarded on step 17
                rol 4,x
                rol 5,x

                ; loop control
                dey
                beq _done

                ; rotate high cell of dividend one bit left (MSB)
                rol 2,x
                rol 3,x

                ; Garth's original routine explicitly stores
                ; the carry (bit 17) in a temp and uses an
                ; extended version of the _maybe branch here.
                ; While that saves some code, this routine is
                ; so heavily used that it seems worth unfolding
                ; the C=0 and C=1 for speed and avoid the temp storage

                bcc _maybe      ; hi bit set?

                ; bit 17 aka carry is set, so divisor will definitely go
                lda 2,x
                sbc 0,x
                sta 2,x

                lda 3,x
                sbc 1,x
                sta 3,x

                sec             ; result bit is 1
                bra _loop

_maybe:
                ; otherwise we need to check if divisor "goes", i.e.
                ; is no larger than the high word of dividend, by actually
                ; doing the subtraction and checking the resulting carry

                ; start with the MSB so we can short-circuit early

                sec
                lda 3,x         ; check if we need borrow on MSB
                sbc 1,x
                bcc _loop       ; if we do, divisor won't go, result bit is C=0

                ina
                sta tmpdsp      ; stash msb+1 to simplify upcoming borrow test

                lda 2,x         ; find difference of LSB
                sbc 0,x         ; note carry is already set
                bcs _ok         ; if C=1, we're good to go

                dec tmpdsp      ; need to borrow from the MSB
                beq _loop       ; failing if it was 0 (ie. msb+1 was 1), leaving C=0

                sec             ; otherwise we're good, so ensure C=1
_ok:
                sta 2,x         ; update the LSB of dividend
                lda tmpdsp      ; recover stashed MSB
                dea             ; undo our +1 adjustment
                sta 3,x         ; update MSB of dividend

                bra _loop       ; continue with result bit C=1
_done:
                inx             ; drop the divisor
                inx

                jsr w_swap      ; swap to return ( rem quo )

z_um_slash_mod: rts



; ## UM_STAR ( u u -- ud ) "Multiply 16 x 16 -> 32"
; ## "um*"  auto  ANS core
        ; """https://forth-standard.org/standard/core/UMTimes
        ; Multiply two unsigned 16 bit numbers, producing a 32 bit result.
        ; Old Forth versions such as FIG Forth call this U*
        ;
        ; This is based on modified FIG Forth code by Dr. Jefyll, see
        ; http://forum.6502.org/viewtopic.php?f=9&t=689 for a detailed
        ; discussion and some great explanatory diagrams.
        ;
        ; We don't use the system scratch pad (SYSPAD) for temp
        ; storage because >NUMBER uses it as well, but instead tmp1 to
        ; tmp3 (tmp1 is N in the original code, tmp1+1 is N+1, etc).
        ;
        ; There's a lengthy discussion of alternative 6502 multiply
        ; algorithms at http://forum.6502.org/viewtopic.php?f=2&t=7451
        ; with performance compared at https://github.com/TobyLobster/multiply_test
        ; However those performance figures are averaged over all possible
        ; inputs values uniformly.   In practical applications smaller inputs
        ; are much more likely, especially zero, so it's worth a little more
        ; average expensive in size and cycles to optimize for these cases.
        ;
        ; Also note that although multiplication is symmetrical,
        ; typically algorithm performance isn't.  For example, since we sum
        ; a shifted copy of the RHS each time we find a one bit in the LHS
        ; then it's usually faster to have the larger number on the right and
        ; smaller number on the left.
        ; """

xt_um_star:
                jsr underflow_2
w_um_star:
                ; When we write "123 45 um*" to calculate the product a * b = d
                ; then TOS is the RHS (b) and NOS is the LHS (a) and our
                ; calculation looks like this on the stack:
                ;
                ;           +-----------+-----------+
                ;           |    TOS    |    NOS    |
                ;           | 0,x | 1,x | 2,x | 3,x |
                ;           +-----+-----+-----+-----+
                ; Input:    | blo   bhi | alo   ahi |    we move b-1 to tmp2 and use a in place
                ;           +-----------+-----------+
                ; Output:   | dhlo dhhi   dllo dlhi |    NUXI order d2 d3 d0 d1
                ;           +-----------------------+
                ;              ^    ^
                ;              |    +---- cached in ACC/tmp1+1
                ;              +--------- cached in tmp1


                ; set tmp2 to RHS-1 to eliminate clc inside the loop
                ; at the same time check for quick exit if RHS=0
                lda 0,x         ; copy TOS-1 to tmp2
                clc             ; subtract the extra one
                sbc #0          ; leaves C=1 unless LSB was zero
                sta tmp2

                lda 1,x
                sbc #0          ; leaves C=1 unless both bytes were zero
                bcc _tos_zero   ; is TOS aka RHS zero?
                sta tmp2+1

                lda #0
                sta tmp1        ; initialize dhlo/dhhi = $0000 in <tmp1, acc>
                stx tmp3        ; tracks when to exit from outer loop
                dex
                dex

_outer_loop:
                ; We loop over LHS bits in two passes, once for the low byte
                ; and then for the high byte.  Each time we use a LHS bit
                ; we roll it out from the least significant bit, and roll
                ; in a bit of the result to the most significant bit.  Once
                ; we've done this eight times the RHS byte has been replaced
                ; by the output byte.
                ; We don't explicitly test for LHS=0 but the skip8 shortcut
                ; deals with it fairly quickly.

                ; On entry A has the low byte of tmp1

                ldy #8          ; inner loop counter, looping over LHS bits
                lsr 4,x         ; think "2,x" the first time and "3,x" the next
                bcs +
                beq _skip8      ; shortcut if all bits in this byte were zero
_inner_loop:
                bcc _no_add
+
                sta tmp1+1      ; add a copy of LHS-1 + C=1 to tmp1
                lda tmp1
                adc tmp2        ; save time, don't CLC
                sta tmp1
                lda tmp1+1
                adc tmp2+1

_no_add:
                ror
                ror tmp1
                ror 4,x         ; first "2,x" then "3,x"

                dey
                bne _inner_loop ; done eight bits?
_next8:
                inx
                cpx tmp3
                bne _outer_loop ; go back for eight more shifts?

                ; all done, store high word of result
                sta 1,x
                lda tmp1
                sta 0,x
                bra _done

_skip8:
                ldy tmp1         ; 0 => A => tmp1 => 4,x
                sty 4,x
                sta tmp1
                lda #0
                bra _next8

_tos_zero:
                stz 2,x         ; just set the other result bytes to zero
                stz 3,x
_done:
z_um_star:      rts



; ## WITHIN ( n1 n2 n3 -- ) "Test n1 within range [n2, n3) or outwith [n3, n2)"
; ## "within"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/WITHIN
        ;
        ; This an assembler version of the ANS Forth implementation
        ; at https://forth-standard.org/standard/core/WITHIN which is
        ; OVER - >R - R> U<  note there is an alternative high-level version
        ; ROT TUCK > -ROT > INVERT AND
        ; """"
xt_within:
                jsr underflow_3
w_within:
                jsr w_over              ; ( n1 n2 n3 n2 )
                jsr w_minus             ; ( n1 n2 n3-n2 )
                inx                     ; pretend to push n3-n2 to return stack
                inx
                jsr w_minus             ; ( n1-n2 ) with ( n2 n3-n2 ) past end of stack
                dex                     ; nip the overhang leaving ( n1-n2 n3-n2 )
                dex
                lda $fe,x
                sta 0,x
                lda $ff,x
                sta 1,x
                jsr w_u_less_than       ; ( f )

z_within:       rts



; ## XOR ( n n -- n ) "Logically XOR TOS and NOS"
; ## "xor"  auto  ANS core
        ; """https://forth-standard.org/standard/core/XOR"""
xt_xor:
                jsr underflow_2
w_xor:
                lda 0,x
                eor 2,x
                sta 2,x

                lda 1,x
                eor 3,x
                sta 3,x

                inx
                inx

z_xor:          rts



; ## ZERO_EQUAL ( n -- f ) "Check if TOS is zero"
; ## "0="  auto  ANS core
        ; """https://forth-standard.org/standard/core/ZeroEqual"""

xt_zero_equal:
                jsr underflow_1
w_zero_equal:
                lda 0,x
                ora 1,x
                beq _zero       ; if 0, A is inverse of the TRUE (-1) we want
                lda #$FF        ; else set A inverse of the FALSE (0) we want
_zero:
                eor #$FF        ; now just invert:
                sta 0,x
                sta 1,x

z_zero_equal:   rts



; ## ZERO_GREATER ( n -- f ) "Return a TRUE flag if TOS is positive"
; ## "0>"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/Zeromore"""

xt_zero_greater:
                jsr underflow_1
w_zero_greater:
                ldy #0          ; Default is FALSE (TOS is negative)

                lda 1,x         ; MSB
                bmi _done       ; TOS is negative, keep FLASE
                ora 0,x
                beq _done       ; TOS is zero, keep FALSE

                dey             ; TOS is postive, make true
_done:
                tya
                sta 0,x
                sta 1,x

z_zero_greater: rts



; ## ZERO_LESS ( n -- f ) "Return a TRUE flag if TOS negative"
; ## "0<"  auto  ANS core
        ; """https://forth-standard.org/standard/core/Zeroless"""

xt_zero_less:
                jsr underflow_1
w_zero_less:
                ldy #0          ; Default is FALSE (TOS positive)

                lda 1,x         ; MSB
                bpl _done       ; TOS is positive, so keep FALSE

                dey             ; TOS is negative, make TRUE
_done:
                tya
                sta 0,x
                sta 1,x

z_zero_less:    rts



; ## ZERO_UNEQUAL ( m -- f ) "Return TRUE flag if not zero"
; ## "0<>"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/Zerone"""

xt_zero_unequal:
                jsr underflow_1
w_zero_unequal:
                lda 0,x
                ora 1,x
                beq _zero
                lda #$FF
_zero:
                sta 0,x
                sta 1,x

z_zero_unequal: rts



; END
