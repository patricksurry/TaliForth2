; ## D_MINUS ( d d -- d ) "Subtract two double-celled numbers"
; ## "d-"  auto  ANS double
        ; """https://forth-standard.org/standard/double/DMinus"""
xt_d_minus:
                jsr underflow_4 ; two double numbers

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

                inx
                inx

z_d_to_s:       rts



; ## DABS ( d -- d ) "Return the absolute value of a double"
; ## "dabs"  auto  ANS double
        ; """https://forth-standard.org/standard/double/DABS"""

xt_dabs:
                jsr underflow_2 ; double number

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
; ## "d."  tested  ANS double
        ; """http://forth-standard.org/standard/double/Dd"""
        ;
        ; From the Forth code:
        ; : D. TUCK DABS <# #S ROT SIGN #> TYPE SPACE ;
        ; """

xt_d_dot:
                jsr underflow_2

                jsr xt_tuck
                jsr xt_dabs
                jsr xt_less_number_sign
                jsr xt_number_sign_s
                jsr xt_rot
                jsr xt_sign
                jsr xt_number_sign_greater
                jsr xt_type
                jsr xt_space

z_d_dot:        rts


; ## D_DOT_R ( d u -- ) "Print double right-justified u wide"
; ## "d.r"  tested  ANS double
        ; """http://forth-standard.org/standard/double/DDotR"""
        ; Based on the Forth code
        ;  : D.R >R TUCK DABS <# #S ROT SIGN #> R> OVER - SPACES TYPE ;
        ; """

xt_d_dot_r:
                jsr underflow_3
                ; From the forth code:
                jsr xt_to_r
                jsr xt_tuck
                jsr xt_dabs
                jsr xt_less_number_sign
                jsr xt_number_sign_s
                jsr xt_rot
                jsr xt_sign
                jsr xt_number_sign_greater
                jsr xt_r_from
                jsr xt_over
                jsr xt_minus
                jsr xt_spaces
                jsr xt_type

z_d_dot_r:      rts



; ## TWO_CONSTANT (C: d "name" -- ) ( -- d) "Create a constant for a double word"
; ## "2constant"  auto  ANS double
        ; """https://forth-standard.org/standard/double/TwoCONSTANT
        ;
        ; Based on the Forth code
        ; : 2CONSTANT ( D -- )  CREATE SWAP , , DOES> DUP @ SWAP CELL+ @ ;
        ; """
xt_two_constant:
                jsr underflow_2

                jsr xt_create
                jsr xt_swap
                jsr xt_comma
                jsr xt_comma

                jsr does_runtime    ; does> turns into these two routines.
                jsr dodoes

                jsr xt_dup
                jsr xt_fetch
                jsr xt_swap
                jsr xt_cell_plus
                jsr xt_fetch

z_two_constant: rts


; ## TWO_LITERAL (C: d -- ) ( -- d) "Compile a literal double word"
; ## "2literal"  auto  ANS double
        ; """https://forth-standard.org/standard/double/TwoLITERAL"""
        ; Shares code with xt_sliteral for compiling a double word
        ; """
xt_two_literal:
                jsr underflow_2 ; double number

                lda #template_push_tos_size
                asl
                jsr check_nc_limit
                bcs _no_inline

                jsr xt_swap
                jsr xt_literal
                jmp xt_literal

_no_inline:
                jsr cmpl_two_literal

z_two_literal:  rts



; ## TWO_VARIABLE ( "name" -- ) "Create a variable for a double word"
; ## "2variable"  auto  ANS double
        ; """https://forth-standard.org/standard/double/TwoVARIABLE
        ; The variable is not initialized to zero.
        ;
        ; This can be realized in Forth as either
        ; CREATE 2 CELLS ALLOT  or just  CREATE 0 , 0 ,
        ; """
xt_two_variable:
                ; We just let CRATE and ALLOT do the heavy lifting
                jsr xt_create

                dex
                dex
                lda #4
                sta 0,x
                stz 1,x

                jsr xt_allot

z_two_variable: rts



; ## UD_DOT ( d -- ) "Print double as unsigned"
; ## "ud."  auto  Tali double
        ;
        ; """Based on the Forth code  : UD. <# #S #> TYPE SPACE ;
        ; """
xt_ud_dot:
                jsr underflow_2 ; double number

                jsr xt_less_number_sign
                jsr xt_number_sign_s
                jsr xt_number_sign_greater
                jsr xt_type
                jsr xt_space

z_ud_dot:        rts


; ## UD_DOT_R ( d u -- ) "Print unsigned double right-justified u wide"
; ## "ud.r"  auto  Tali double
        ;
        ; """Based on the Forth code : UD.R  >R <# #S #> R> OVER - SPACES TYPE ;
        ; """
xt_ud_dot_r:
                jsr underflow_3

                jsr xt_to_r
                jsr xt_less_number_sign
                jsr xt_number_sign_s
                jsr xt_number_sign_greater
                jsr xt_r_from
                jsr xt_over
                jsr xt_minus
                jsr xt_spaces
                jsr xt_type

z_ud_dot_r:      rts
