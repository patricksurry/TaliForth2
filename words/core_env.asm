.if "environment?" in TALI_OPTIONAL_WORDS
; ## ENVIRONMENT_Q  ( addr u -- 0 | i*x true )  "Return system information"
; ## "environment?"  auto  ANS core
        ; """https://forth-standard.org/standard/core/ENVIRONMENTq
        ;
        ; By ANS definition, we use upper-case strings here, see the
        ; string file for details. This can be realized as a high-level
        ; Forth word as
        ;
        ; : STRING_OF POSTPONE 2OVER POSTPONE COMPARE POSTPONE 0=
        ;    POSTPONE IF POSTPONE 2DROP ; IMMEDIATE COMPILE-ONLY
        ; HEX
        ; : ENVIRONMENT? ( C-ADDR U -- FALSE | I*X TRUE )
        ; CASE
        ; S" /COUNTED-STRING"    STRING_OF    FF TRUE ENDOF
        ; S" /HOLD"              STRING_OF    FF TRUE ENDOF
        ; S" /PAD"               STRING_OF    54 TRUE ENDOF ( 84 DECIMAL )
        ; S" ADDRESS-UNIT-BITS"  STRING_OF     8 TRUE ENDOF
        ; S" FLOORED"            STRING_OF FALSE TRUE ENDOF ( WE HAVE SYMMETRIC )
        ; S" MAX-CHAR"           STRING_OF   255 TRUE ENDOF
        ; S" MAX-D"              STRING_OF
                                     ; 7FFFFFFF. TRUE ENDOF
        ; S" MAX-N"              STRING_OF  7FFF TRUE ENDOF
        ; S" MAX-U"              STRING_OF  FFFF TRUE ENDOF
        ; S" MAX-UD"             STRING_OF
                                     ; FFFFFFFF. TRUE ENDOF
        ; S" RETURN-STACK-CELLS" STRING_OF    80 TRUE ENDOF
        ; S" STACK-CELLS"        STRING_OF    20 TRUE ENDOF ( FROM DEFINITIONS.ASM )
        ; ( DEFAULT ) 2DROP FALSE FALSE ( ONE FALSE WILL DROPPED BY ENDCASE )
        ; ENDCASE ;
        ;
        ; but that uses lots of memory and increases the start up time. This
        ; word is rarely used so we can try to keep it short at the expense
        ; of speed.
        ; """
xt_environment_q:
                jsr underflow_1
w_environment_q:
                ; This code is table-driven: We walk through the list of
                ; strings until we find one that matches, and then we take
                ; the equivalent data from the results table. This is made
                ; a bit harder by the fact that some of these return a
                ; double-cell number and some a single-cell one.


                ldy #0                  ; index for table

_table_loop:
                ; See if this is the last entry.
                cpy #env_table_end - env_table_single
                beq _table_done

                ; We arrived here with the address of the string to be checked
                ; on the stack. We make a copy. Index is in Y
                jsr w_two_dup          ; ( addr u addr u ) 2DUP does not use Y

                ; We do our work on the TOS to speed things up
                dex
                dex                     ; ( addr u addr u ? )

                ; Get address of string to check from table
                lda env_table_single,y
                sta 0,x
                iny
                lda env_table_single,y
                sta 1,x                 ; ( addr u addr u addr-s )
                iny

                ; Calculate length using difference from next pointer
                dex
                dex
                lda env_table_single,y
                sta 0,x
                lda env_table_single+1,y
                sta 1,x
                jsr w_over
                jsr w_minus            ; ( addr u addr u addr-s u-s )

                ; Compare the strings (surprisingly w_compare doesn't use Y)
                jsr w_compare           ; ( addr u f )

                ; Pre-drop the flag before we branch
                inx                     ; DROP, now ( addr u )
                inx

                ; If we found a match (flag is zero -- COMPARE is weird
                ; that way), fall through to return the result
                lda $fe,x
                ora $ff,x
                bne _table_loop         ; Not a match, so try next string

                ; We arrive here with ( addr u ) after finding a match
                ; Y contains the index of the match + 2.
                dey                     ; go back to index we had
                dey

                cpy #env_table_double - env_table_single
                bcs _double_result

                ; Single-cell result
                lda env_results_single,y
                sta 2,x
                lda env_results_single+1,y
                sta 3,x                 ; ( res u )

                bra _set_flag

_double_result:
                ; This is a double-celled result, which means we have to
                ; fool around with the index some more. We also need a
                ; further cell on the stack
                dex                     ; ( addr u ? )
                dex

                ; To get the index for the double-cell words,
                ; we subtract the table offset and multiply by two
                ; since we have four bytes per entry but Y increments by 2
                tya
                sec
                sbc #(env_table_double - env_table_single)
                asl
                tay

                lda env_results_double,y
                sta 2,x
                lda env_results_double+1,y
                sta 3,x                 ; ( res u ? )
                lda env_results_double+2,y
                sta 4,x
                lda env_results_double+3,y
                sta 5,x                 ; ( res res ? )

                ; fall through to _set_flag
_set_flag:
                lda #$ff
                bra _done

_table_done:
                ; We're done checking all the entries.
                ; We arrive here with ( addr u )
                ; Drop one entry to leave space for flag ( ? )
                inx
                inx
                lda #0                  ; flag failure and fall through

_done:
                ; Set the flag to either ffff or 0000 leaving
                ; ( res true ) or ( dres dres true ) or just ( false )
                sta 0,x
                sta 1,x

z_environment_q:
                rts


; Tables for ENVIRONMENT?. We use two separate ones, one for the single-cell
; results and one for the double-celled results. The strings themselves
; are defined consecutively in strings.asm so that we can calculate
; length as the difference in offsets.

env_table_single:
        .word envs_cs, envs_hold, envs_pad, envs_aub, envs_floored
        .word envs_max_char, envs_max_n, envs_max_u, envs_rsc
        .word envs_sc, envs_wl
env_table_double:
        .word envs_max_d, envs_max_ud
env_table_end:
        .word envs_eot                  ; pointer beyond last string


env_results_single:
        .word $00FF     ; /COUNTED-STRING
        .word $00FF     ; /HOLD
        .word $0054     ; /PAD (this is 84 decimal)
        .word $0008     ; ADDRESS-UNIT-BITS (keep "$" to avoid octal!)
        .word 0000      ; FLOORED ("FALSE", we have symmetric)
        .word $00FF     ; MAX-CHAR
        .word $7FFF     ; MAX-N
        .word $FFFF     ; MAX-U
        .word $0080     ; RETURN-STACK-CELLS
        .word $0020     ; STACK-CELLS (from definitions.asm)
        .word $0009     ; WORDLISTS

env_results_double:
        .word $7FFF, $FFFF      ; MAX-D
        .word $FFFF, $FFFF      ; MAX-UD
.endif



; END
