xt_dump:  ; ( addr n )
                jsr underflow_2
w_dump:
                jsr w_cr

                lda 2,x                 ; copy addr to tmp1 for y-indexing
                and #$f8                ; mask off low bits to start from multiple of 8
                sta tmp2
                lda 3,x
                sta tmp2+1

                lda 2,x
                and #7
                sta tmp1                ; index of first byte this row

                clc
                adc 0,x
                cmp #8
                bcs _max

                ldy 1,x
                beq +
_max:
                lda #8
+
                cmp tmp1
                beq _done               ; if first == last, we're done

                sta tmp1+1              ; index of last byte this row

                ; show current address
                lda 3,x
                ldy 2,x
                jsr word_to_ascii
                jsr w_space

                lda #%0100_0000
                sta tmptos              ; pass counter $40 -> $80 -> 0
_pass:
                ldy #0
                jsr w_space
_loop:
                bit tmptos

                cpy tmp1
                bcc _skip
                cpy tmp1+1
                bcs _skip

                lda (tmp2),y
                bvc _chr

                jsr byte_to_ascii       ; show byte value
                bra _spnext
_chr:
                jsr is_printable        ; show ascii char
                bcs +
                lda #'.'                ; use dot if not printable
+
                jsr emit_a
                bra _next
_skip:
                bvc _spnext
                jsr w_space
                jsr w_space
_spnext:
                jsr w_space
_next:
                iny
                cpy #8
                bne _loop

                asl tmptos
                bne _pass

                dex
                dex
                lda tmp1+1
                sbc tmp1                ; C=1 from asl
                stz 1,x
                sta 0,x
                jsr w_slash_string

                bra w_dump

_done:
                inx
                inx
                inx
                inx
z_dump:
                rts
