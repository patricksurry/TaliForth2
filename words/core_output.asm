; Core Forth words for formatted output

; Tali Forth 2 for the 65c02
; Scot W. Stevenson <scot.stevenson@gmail.com>
; Sam Colwell
; Patrick Surry
; First version: 19. Jan 2014
; This version: 21. Apr 2024


; ## AT_XY ( m n -- ) "Move cursor to position given"
; ## "at-xy"  auto  ANS facility
        ; """https://forth-standard.org/standard/facility/AT-XY
        ; On an ANSI compatible terminal, place cursor at row n column m.
        ; ANSI code is ESC[<n+1>;<m+1>H
        ;
        ; Do not use U. to print the numbers because the
        ; trailing space will not work with xterm
        ; """
xt_at_xy:
                jsr underflow_2
w_at_xy:
                ; Save the BASE and change to decimal as the ANSI escape code
                ; values need to be in decimal.
                lda base
                pha
                lda #10
                sta base

                lda #AscESC
                jsr emit_a
                lda #'['
                jsr emit_a
                jsr w_one_plus ; AT-XY is zero based, but ANSI is 1 based
                jsr print_u
                lda #';'
                jsr emit_a
                jsr w_one_plus ; AT-XY is zero based, but ANSI is 1 based
                jsr print_u
                lda #'H'
                jsr emit_a

                ; Restore the base
                pla
                sta base

z_at_xy:        rts



; ## CR ( -- ) "Print a line feed"
; ## "cr"  auto  ANS core
        ; """https://forth-standard.org/standard/core/CR"""
xt_cr:
w_cr:
.if "cr" in TALI_OPTION_CR_EOL
                lda #AscCR
                jsr emit_a
.endif
.if "lf" in TALI_OPTION_CR_EOL
                lda #AscLF
                jsr emit_a
.endif
z_cr:           rts



; ## DOT ( u -- ) "Print TOS"
; ## "."  auto  ANS core
        ; """https://forth-standard.org/standard/core/d"""

xt_dot:
                jsr underflow_1
w_dot:
                jsr w_dup                      ; ( n n )
                jsr w_abs                      ; ( n u )
                jsr w_zero                     ; ( n u 0 )
                jsr w_less_number_sign         ; ( n u 0 )
                jsr w_number_sign_s            ; ( n ud )
                jsr w_rot                      ; ( ud n )
                jsr w_sign                     ; ( ud )
                jsr w_number_sign_greater      ; ( addr u )
                jsr w_type
                jsr w_space

z_dot:          rts



; ## DOT_PAREN ( -- ) "Print input up to close paren .( comment )"
; ## ".("  auto  ANS core
        ; """http://forth-standard.org/standard/core/Dotp"""

xt_dot_paren:
w_dot_paren:
                ; Put a right paren on the stack.
                dex
                dex
                lda #41     ; Right parenthesis
                sta 0,x
                stz 1,x

                jsr w_parse
                jsr w_type

z_dot_paren:    rts



; ## DOT_QUOTE ( "string" -- ) "Print string from compiled word"
; ## ".""  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/Dotq
        ; Compile string that is printed during run time. ANS Forth wants
        ; this to be compile-only, even though everybody and their friend
        ; uses it for everything. We follow the book here, and recommend
        ; `.(` for general printing.
        ; """

xt_dot_quote:
w_dot_quote:
                ; we let S" do the heavy lifting. Since we're in
                ; compile mode, it will save the string and reproduce it
                ; during runtime
                jsr w_s_quote

                ; We then let TYPE do the actual printing
                ldy #>w_type
                lda #<w_type
                jsr cmpl_subroutine

z_dot_quote:    rts



; ## DOT_R ( n u -- ) "Print NOS as unsigned number with TOS width"
; ## ".r"  tested  ANS core ext
        ; """https://forth-standard.org/standard/core/DotR
        ;
        ; Based on the Forth code
        ;  : .R  >R DUP ABS 0 <# #S ROT SIGN #> R> OVER - SPACES TYPE ;
        ; """

xt_dot_r:
                jsr underflow_2
w_dot_r:
                jsr w_to_r              ; ( n ) (R: u )
                jsr w_dup
                jsr w_abs
                jsr w_zero              ; ( n |n| 0 ) (R: u )
                jsr w_less_number_sign
                jsr w_number_sign_s
                jsr w_rot
                jsr w_sign
                jsr w_number_sign_greater
                jsr w_r_from
                jsr w_over
                jsr w_minus
                jsr w_spaces
                jsr w_type

z_dot_r:        rts



; ## EMIT ( char -- ) "Print character to current output"
; ## "emit"  auto  ANS core
        ; """https://forth-standard.org/standard/core/EMIT
        ; Run-time default for EMIT. The user can revector this by changing
        ; the value of the OUTPUT variable. We ignore the MSB completely, and
        ; do not check to see if we have been given a valid ASCII character.
        ; Don't make this native compile.
        ; """

xt_emit:
                jsr underflow_1
w_emit:
                lda 0,x
                inx
                inx

emit_a:
        ; We frequently want to print the character in A without fooling
        ; around with the Data Stack. This is emit_a's job, which still
        ; allows the output to be vectored. Call it with JSR as you
        ; would XT_EMIT
                jmp (output)            ; JSR/RTS

z_emit:         ; never reached



; ## HOLD ( char -- ) "Insert character at current output"
; ## "hold"  auto  ANS core
        ; """https://forth-standard.org/standard/core/HOLD
        ; Insert a character at the current position of a pictured numeric
        ; output string on
        ; https://github.com/philburk/pforth/blob/master/fth/numberio.fth
        ;
        ; Forth code is : HOLD  -1 HLD +!  HLD @ C! ;  We use the the internal
        ; variable tohold instead of HLD.
        ; """
xt_hold:
                jsr underflow_1
w_hold:
                lda tohold
                bne +
                dec tohold+1
+
                dec tohold

                lda 0,x
                sta (tohold)
                inx
                inx

z_hold:         rts



; ## LESS_NUMBER_SIGN ( -- ) "Start number conversion"
; ## "<#"  auto  ANS core
        ; """https://forth-standard.org/standard/core/num-start
        ; Start the process to create pictured numeric output.
        ;
        ; The new
        ; string is constructed from back to front, saving the new character
        ; at the beginning of the output string. Since we use PAD as a
        ; starting address and work backward (!), the string is constructed
        ; in the space between the end of the Dictionary (as defined by CP)
        ; and the PAD. This allows us to satisfy the ANS Forth condition that
        ; programs don't fool around with the PAD but still use its address.
        ; Based on pForth
        ; http://pforth.googlecode.com/svn/trunk/fth/numberio.fth
        ; pForth is in the pubic domain. Forth is : <# PAD HLD ! ; we use the
        ; internal variable tohold instead of HLD.
        ; """
xt_less_number_sign:
w_less_number_sign:
                jsr w_pad      ; ( addr )

                lda 0,x
                sta tohold
                lda 1,x
                sta tohold+1

                inx
                inx

z_less_number_sign:
                rts



; ## NUMBER_SIGN ( ud -- ud ) "Add character to pictured output string"
; ## "#"  auto  ANS core
        ; """https://forth-standard.org/standard/core/num
        ; Add one char to the beginning of the pictured output string.
        ;
        ; Based on
        ; https://github.com/philburk/pforth/blob/master/fth/numberio.fth
        ; Forth code  BASE @ UD/MOD ROT 9 OVER < IF 7 + THEN [CHAR] 0 + HOLD ;
        ; """
xt_number_sign:
                jsr underflow_2         ; double number
w_number_sign:
                ; The following is based on the ancient Forth word UD/MOD, which in
                ; various Forths (including Gforth) lives on under the hood,
                ; even though it's not an ANS standard word, it doesn't appear
                ; in the docs, it's only used here, and there are no tests for
                ; it. This is why we got rid of it. We'll be converting this
                ; mess to something more sane in the long run.

                ; Imagine we have the double word ud = 2^16 u + v which we want to
                ; write as qd m + r for some base m.  Let u = qu m + ru, with qu, ru
                ; caculated in forth via `u 0 m um/mod`, meaning that
                ; ud = 2^16 qu m + (2^16 ru + v).  Now write the "remainder" term
                ; 2^16 ru + v as qv m + rv, again calculating qv, rv
                ; in forth via `v ru m um/mod`.  (We know the quotient won't overflow
                ; a single word because the high word ru < m.)  This leaves
                ; ud = 2^16 qu m + qv m + rv = (2^16 qu + qv) m + rv
                ; so that qd is the double word (qv, qu) and r is rv.

                ; If, as is often the case, the most signficant word u is zero
                ; then qu = ru = 0 and we can skip the first pass and
                ; simply calculate v 0 m um/mod immediately.

                dex                     ; inline w_zero
                dex
                stz 0,x
                stz 1,x

                ; use msb of base as a flag to loop twice
                ; (we assume below base <= 36 so this is safe)
                inc base+1

                lda 2,x                 ; if msw is 0 we can skip the first pass
                ora 3,x
                beq _skip               ; enter with ( v 0 0 -rot -- 0 v 0 )

_loop:
                ; ( v u 0 ) on first pass, then ( qu v ru ) on second pass
                dex                     ; inline `base @`
                dex
                lda base                ; base <= 36
                sta 0,x
                stz 1,x
                jsr w_um_slash_mod      ; ( v u 0 base -- v ru qu )
_skip:          jsr w_not_rot           ; ( qu v ru )
                lsr base+1              ; 1 => 0 + C=1 => 0 + C=0
                bcs _loop               ; run two passes

                ; the second pass calculates:
                ; base @ ( qu v ru base )
                ; um/mod ( qu rv qv )
                ; -rot   ( qv qu rv ) aka ( ud rem )

                ; Convert the number that is left over to an ASCII character.
                ; We use a string lookup for speed (assumes base <= 36).

                lda 0,x
                tay
                lda alpha36,y           ; upper case 0-9A-Z
                sta 0,x
                stz 1,x                 ; paranoid; now ( ud char )

                jsr w_hold

z_number_sign:
                rts



; ## NUMBER_SIGN_GREATER ( d -- addr u ) "Finish pictured number conversion"
; ## "#>"  auto  ANS core
        ; """https://forth-standard.org/standard/core/num-end
        ; Finish conversion of pictured number string, putting address and
        ; length on the Data Stack.
        ;
        ; Original Fort is  2DROP HLD @ PAD OVER -
        ; Based on
        ; https://github.com/philburk/pforth/blob/master/fth/numberio.fth
        ; """
xt_number_sign_greater:
                jsr underflow_2         ; double number
w_number_sign_greater:
                ; The start address lives in tohold
                lda tohold
                sta 0,x         ; LSB of tohold
                sta 2,x
                lda tohold+1
                sta 1,x         ; MSB of addr
                sta 3,x         ; ( addr addr )

                ; The length of the string is pad - addr
                jsr w_pad      ; ( addr addr pad )

                sec
                lda 0,x         ; LSB of pad address
                sbc 2,x
                sta 2,x

                lda 1,x         ; MSB, which should always be zero
                sbc 3,x
                sta 3,x         ; ( addr u pad )

                inx
                inx

z_number_sign_greater:
                rts



; ## NUMBER_SIGN_S ( d -- addr u ) "Completely convert pictured output"
; ## "#s"  auto  ANS core
        ; """https://forth-standard.org/standard/core/numS
        ; Completely convert number for pictured numerical output.
        ;
        ; Based on
        ; https://github.com/philburk/pforth/blob/master/fth/system.fth
        ; Original Forth code  BEGIN # 2DUP OR 0= UNTIL
        ; """

xt_number_sign_s:
                jsr underflow_2
w_number_sign_s:
_loop:
                ; convert a single number ("#")
                jsr w_number_sign

                ; stop when double-celled number in TOS is zero:
                lda 0,x
                ora 1,x
                ora 2,x
                ora 3,x
                bne _loop

z_number_sign_s:
                rts



; ## PAGE ( -- ) "Clear the screen"
; ## "page"  auto  ANS facility
        ; """https://forth-standard.org/standard/facility/PAGE
        ; Clears a page if supported by ANS terminal codes. This is
        ; Clear Screen ("ESC[2J") plus moving the cursor to the top
        ; left of the screen
        ; """
xt_page:
w_page:
                lda #AscESC
                jsr emit_a
                lda #'['
                jsr emit_a
                lda #'2'
                jsr emit_a
                lda #'J'
                jsr emit_a

                ; move cursor to top left of screen
                jsr w_zero
                jsr w_zero
                jsr w_at_xy

z_page:         rts



; ## SIGN ( n -- ) "Add minus to pictured output"
; ## "sign"  auto  ANS core
        ; """https://forth-standard.org/standard/core/SIGN
        ;
        ; Code based on
        ; http://pforth.googlecode.com/svn/trunk/fth/numberio.fth
        ; Original Forth code is   0< IF ASCII - HOLD THEN
        ; """

xt_sign:
                jsr underflow_1
w_sign:
                lda 1,x         ; check MSB of TOS
                bmi _minus

                inx
                inx
                bra _done
_minus:
                lda #'-'
                sta 0,x         ; overwrite TOS
                stz 1,x         ; paranoid

                jsr w_hold
_done:
z_sign:         rts



; ## SPACE ( -- ) "Print a single space"
; ## "space"  auto  ANS core
        ; """https://forth-standard.org/standard/core/SPACE"""
xt_space:
w_space:
                lda #AscSP
                jsr emit_a

z_space:        rts



; ## SPACES ( u -- ) "Print a number of spaces"
; ## "spaces"  auto  ANS core
        ; """https://forth-standard.org/standard/core/SPACES"""

xt_spaces:
                jsr underflow_1
w_spaces:
                lda 1,x         ; ANS says this word takes a signed value
                bmi _done       ; but prints no spaces for negative values.

                ldy 0,x
                beq _msb
_loop:                          ; loop to zero out LSB
                lda #AscSP
                jsr emit_a      ; user routine preserves X and Y
                dey
                bne _loop       ; Y is zero on exit so looping again emits 256 more spaces
_msb:
                dec 1,x         ; when decrementing MSB goes negative, it was zero so we're done
                bpl _loop       ; otherwise emit another 256 spaces

_done:          inx
                inx
z_spaces:       rts



; ## TYPE ( addr u -- ) "Print string"
; ## "type"  auto  ANS core
        ; """https://forth-standard.org/standard/core/TYPE
        ; Works through EMIT to allow OUTPUT revectoring.
        ; """

xt_type:
                jsr underflow_2
w_type:
                ; Save the starting address into tmp1
                lda 2,x
                sta tmp1
                lda 3,x
                sta tmp1+1
_loop:
                ; done if length is zero
                lda 0,x
                ora 1,x
                beq _done

                ; Send the current character
                lda (tmp1)
                jsr emit_a      ; avoids stack foolery

                ; Move the address along (in tmp1)
                inc tmp1
                bne +
                inc tmp1+1
+
                ; Reduce the count (on the data stack)
                lda 0,x
                bne +
                dec 1,x
+
                dec 0,x

                bra _loop
_done:
                inx
                inx
                inx
                inx

z_type:         rts



; ## U_DOT ( u -- ) "Print TOS as unsigned number"
; ## "u."  tested  ANS core
        ; """https://forth-standard.org/standard/core/Ud
        ;
        ; This is : U. 0 <# #S #> TYPE SPACE ; in Forth
        ; We use the internal assembler function print_u followed
        ; by a single space
        ; """
xt_u_dot:
                jsr underflow_1
w_u_dot:
                jsr print_u
                lda #AscSP
                jsr emit_a

z_u_dot:        rts


; ## U_DOT_R ( u u -- ) "Print NOS as unsigned number right-justified with TOS width"
; ## "u.r"  tested  ANS core ext
        ; """https://forth-standard.org/standard/core/UDotR"""
xt_u_dot_r:
                jsr underflow_2
w_u_dot_r:
                jsr w_to_r
                jsr w_zero
                jsr w_less_number_sign
                jsr w_number_sign_s
                jsr w_number_sign_greater
                jsr w_r_from
                jsr w_over
                jsr w_minus
                jsr w_spaces
                jsr w_type

z_u_dot_r:      rts



; END
