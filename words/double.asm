; ## D_MINUS ( d d -- d ) "Subtract two double-celled numbers"
; ## "d-"  auto  ANS double
        ; """https://forth-standard.org/standard/double/DMinus"""
xt_d_minus:
                jsr underflow_4 ; two double numbers
w_d_minus:
                sec

                lda 6,x         ; LSB of lower word
                sbc 2,x
                sta 6,x

                lda 7,x         ; MSB of lower word
                sbc 3,x
                sta 7,x

                lda 4,x         ; LSB of upper word
                sbc 0,x
                sta 4,x

                lda 5,x         ; MSB of upper word
                sbc 1,x
                sta 5,x

                inx
                inx
                inx
                inx

z_d_minus:      rts


; ## D_PLUS ( d d -- d ) "Add two double-celled numbers"
; ## "d+"  auto  ANS double
        ; """https://forth-standard.org/standard/double/DPlus"""
xt_d_plus:
                jsr underflow_4 ; two double numbers
w_d_plus:
                clc
                lda 2,x         ; LSB of lower word
                adc 6,x
                sta 6,x

                lda 3,x         ; MSB of lower word
                adc 7,x
                sta 7,x

                lda 0,x         ; LSB of upper word
                adc 4,x
                sta 4,x

                lda 1,x         ; MSB of upper word
                adc 5,x
                sta 5,x

                inx
                inx
                inx
                inx

z_d_plus:       rts


; ## D_TO_S ( d -- n ) "Convert a double number to single"
; ## "d>s"  auto  ANS double
        ; """https://forth-standard.org/standard/double/DtoS
        ; Though this is basically just DROP, we keep it
        ; separate so we can test for underflow
        ; """

xt_d_to_s:
                jsr underflow_2
w_d_to_s:
                inx
                inx

z_d_to_s:       rts



; ## DABS ( d -- d ) "Return the absolute value of a double"
; ## "dabs"  auto  ANS double
        ; """https://forth-standard.org/standard/double/DABS"""

xt_dabs:
                jsr underflow_2 ; double number
w_dabs:
                lda 1,x         ; MSB of high cell
                bpl _done       ; positive, we get off light

                ; negative, calculate 0 - d
                ldy #0
                sec

                tya
                sbc 2,x         ; LSB of low cell
                sta 2,x

                tya
                sbc 3,x         ; MSB of low cell
                sta 3,x

                tya
                sbc 0,x         ; LSB of high cell
                sta 0,x

                tya
                sbc 1,x         ; MSB of high cell
                sta 1,x
_done:
z_dabs:         rts



; ## DNEGATE ( d -- d ) "Negate double cell number"
; ## "dnegate"  auto  ANS double
        ; """https://forth-standard.org/standard/double/DNEGATE"""
xt_dnegate:
                jsr underflow_2 ; double number
w_dnegate:
     		ldy #0
                sec

                tya
                sbc 2,x         ; LSB of low cell
                sta 2,x

                tya
                sbc 3,x         ; MSB of low cell
                sta 3,x

                tya
                sbc 0,x         ; LSB of high cell
                sta 0,x

                tya
                sbc 1,x         ; MSB of high cell
                sta 1,x

z_dnegate:      rts



; ## D_DOT ( d -- ) "Print double"
; ## "d."  auto  ANS double
        ; """http://forth-standard.org/standard/double/Dd"""
        ;
        ; From the Forth code:
        ; : D. TUCK DABS <# #S ROT SIGN #> TYPE SPACE ;
        ; """

xt_d_dot:
                jsr underflow_2
w_d_dot:
                jsr w_tuck
                jsr w_dabs
                jsr w_less_number_sign
                jsr w_number_sign_s
                jsr w_rot
                jsr w_sign
                jsr w_number_sign_greater
                jsr w_type
                jsr w_space

z_d_dot:        rts


; ## D_DOT_R ( d u -- ) "Print double right-justified u wide"
; ## "d.r"  auto  ANS double
        ; """http://forth-standard.org/standard/double/DDotR"""
        ; Based on the Forth code
        ;  : D.R >R TUCK DABS <# #S ROT SIGN #> R> OVER - SPACES TYPE ;
        ; """

xt_d_dot_r:
                jsr underflow_3
w_d_dot_r:
                ; From the forth code:
                jsr w_to_r
                jsr w_tuck
                jsr w_dabs
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

z_d_dot_r:      rts


; ## M_STAR_SLASH ( d1 n1 n2 -- d2 ) "Multiply d1 by n1 and divide by n2.  Note n2 may be negative."
; ## "m*/"  auto  ANS double
        ; """https://forth-standard.org/standard/double/MTimesDiv"""
        ; From All About FORTH, MVP-Forth, public domain,
        ; from this forth code which is modified slightly for Tali2:
        ; DDUP XOR SWAP ABS >R SWAP ABS >R OVER XOR ROT ROT DABS
        ; SWAP R@ UM* ROT R> UM* ROT 0 D+ R@ UM/MOD ROT ROT R> UM/MOD
        ; SWAP DROP SWAP ROT 0< if dnegate then
xt_m_star_slash:
                jsr underflow_4
w_m_star_slash:
                ; DDUP XOR SWAP ABS >R SWAP ABS >R OVER XOR ROT ROT DABS
                jsr w_two_dup
                jsr w_xor
                jsr w_swap
                jsr w_abs
                jsr w_to_r
                jsr w_swap
                jsr w_abs
                jsr w_to_r
                jsr w_over
                jsr w_xor
                jsr w_not_rot          ; rot rot
                jsr w_dabs

                ; SWAP R@ UM* ROT R> UM* ROT 0 D+ R@ UM/MOD ROT ROT R> UM/MOD
                jsr w_swap
                jsr w_r_fetch
                jsr w_um_star
                jsr w_rot
                jsr w_r_from
                jsr w_um_star
                jsr w_rot
                jsr w_zero
                jsr w_d_plus
                jsr w_r_fetch
                jsr w_um_slash_mod
                jsr w_not_rot          ; rot rot
                jsr w_r_from
                jsr w_um_slash_mod

                ; SWAP DROP SWAP ROT 0< if dnegate then ;
                jsr w_swap
                jsr w_drop
                jsr w_swap
                jsr w_rot
                inx                     ; drop TOS
                inx
                lda $fe,x               ; but keep MSB
                bpl z_m_star_slash      ; ... 0< if ...
                jsr w_dnegate

z_m_star_slash: rts


; ## TWO_CONSTANT (C: d "name" -- ) ( -- d) "Create a constant for a double word"
; ## "2constant"  auto  ANS double
        ; """https://forth-standard.org/standard/double/TwoCONSTANT
        ;
        ; Based on the Forth code
        ; : 2CONSTANT ( D -- )  CREATE SWAP , , DOES> DUP @ SWAP CELL+ @ ;
        ; """
xt_two_constant:
                jsr underflow_2
w_two_constant:
                jsr w_create
                jsr w_comma
                jsr w_comma

                jsr does_runtime    ; does> turns into these two routines.
                jsr dodoes

                jsr w_two_fetch
z_two_constant: rts


; ## TWO_LITERAL (C: d -- ) ( -- d) "Compile a literal double word"
; ## "2literal"  auto  ANS double
        ; """https://forth-standard.org/standard/double/TwoLITERAL"""
        ; """
xt_two_literal:
                jsr underflow_2 ; double number
w_two_literal:
                lda #2 * template_push_tos_size
                jsr check_nc_limit
                bcs _no_inline

                jsr w_swap
                jsr w_literal
                jmp w_literal

_no_inline:
                ; Compile a subroutine jump that copies the four following
                ; bytes to the stack, in the same order.  For example
                ; a four byte double word written from MSB to LSB as UNIX
                ; is represented on the stack as TOS: .byte N,U and NOS: X,I
                ; so we'll generate code like:
                ;       jsr two_literal_runtime
                ;       .byte N, U, X, I

                ldy #>two_literal_runtime
                lda #<two_literal_runtime
                jsr cmpl_subroutine

                ldy #4
-
                lda 0,x         ; move four bytes from the stack to cp
                jsr cmpl_a
                inx
                dey
                bne -

z_two_literal:  rts


two_literal_runtime:
        ; """Run time behavior of 2LITERAL, which stacks two words
        ; following the jsr to the stack in the same memory order.
        ; For example if we have
        ;
        ;       jsr two_literal_runtime
        ;       .byte N, U, X, I
        ;
        ; Then on return the stack will contain the word N,U TOS
        ; and the word X, I NOS.  This matches the order expected
        ; for double words and as specified for 2@, 2!
        ; see https://forth-standard.org/standard/core/TwoFetch
        ; """

                pla             ; LSB of address
                sta tmp1
                ply             ; MSB of address
                sty tmp1+1

                clc             ; add four to the return address
                adc #4
                bcc +
                iny
+
                phy             ; and re-stack
                pha

                ldy #4
-
                lda (tmp1),y    ; copy trailing four bytes to the stack
                dex
                sta 0,x
                dey
                bne -

                rts


; ## TWO_VARIABLE ( "name" -- ) "Create a variable for a double word"
; ## "2variable"  auto  ANS double
        ; """https://forth-standard.org/standard/double/TwoVARIABLE
        ; The variable is not initialized to zero.
        ;
        ; This can be realized in Forth as either
        ; CREATE 2 CELLS ALLOT  or just  CREATE 0 , 0 ,
        ; """
xt_two_variable:
                ; We just let CREATE and ALLOT do the heavy lifting
                jsr w_create
w_two_variable:
                dex
                dex
                lda #4
                sta 0,x
                stz 1,x

                jsr w_allot

z_two_variable: rts



; ## UD_DOT ( d -- ) "Print double as unsigned"
; ## "ud."  auto  Tali double
        ;
        ; """Based on the Forth code  : UD. <# #S #> TYPE SPACE ;
        ; """
xt_ud_dot:
                jsr underflow_2 ; double number
w_ud_dot:
                jsr w_less_number_sign
                jsr w_number_sign_s
                jsr w_number_sign_greater
                jsr w_type
                jsr w_space

z_ud_dot:        rts


; ## UD_DOT_R ( d u -- ) "Print unsigned double right-justified u wide"
; ## "ud.r"  auto  Tali double
        ;
        ; """Based on the Forth code : UD.R  >R <# #S #> R> OVER - SPACES TYPE ;
        ; """
xt_ud_dot_r:
                jsr underflow_3
w_ud_dot_r:
                jsr w_to_r
                jsr w_less_number_sign
                jsr w_number_sign_s
                jsr w_number_sign_greater
                jsr w_r_from
                jsr w_over
                jsr w_minus
                jsr w_spaces
                jsr w_type

z_ud_dot_r:      rts
