
;----------------------------------------------------------------------
; string helpers
;----------------------------------------------------------------------

; ## TOLOWER ( addr u -- addr u ) "convert ascii to lower case in place; uses tmp1"
; ## "tolower"  tested ad hoc
#nt_header tolower
xt_tolower:
                jsr underflow_2
w_tolower:
                jsr w_two_dup
_loop:
                lda 0,x
                ora 1,x
                beq _done

                lda (2,x)
                cmp #'A'
                bcc +
                cmp #'Z'+1
                bcs +
                ora #$20
                sta (2,x)
+
                jsr slash_string_1
                bra _loop
_done:
                jsr w_two_drop

z_tolower:      rts


; ## ASCIIZ> ( c-addr -- addr u ) "count a zero-terminated string; uses tmp1"
; ## "asciiz"  tested ad hoc
#nt_header asciiz, "asciiz>"
xt_asciiz:
        jsr underflow_1
w_asciiz:
        lda 0,x
        sta tmp1
        lda 1,x
        sta tmp1+1
        pha             ; save original high byte
        dex             ; push uint16 len
        dex

        ldy #0
-
        lda (tmp1),y
        beq +
        iny
        bne -
        inc tmp1+1
        bra -
+
        tya
        sta 0,x         ; low byte of len
        pla             ; starting page
        tay
        clc             ; subtract one more
        sbc tmp1+1      ; page_start - page_end - 1
        eor #$ff        ; 255 - (page_start - page_end - 1)
        sta 1,x         ; # of pages
        sty tmp1+1      ; reset original addr
z_asciiz:
        rts
