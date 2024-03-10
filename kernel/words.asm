; ## RANDOM ( -- n ) "Return a random word"
; ## "random"  tested ad hoc
xt_random:
        jsr rng_798
        dex
        dex
        lda rand16
        sta 0,x
        lda rand16+1
        sta 1,x
z_random:
        rts


; ## TOLOWER ( addr u -- addr u ) "convert ascii to lower case in place; uses tmp1"
; ## "tolower"  tested ad hoc
xt_tolower:
                ; we'll work backwards, using addr in tmp1
                lda 2,x         ; copy addr to tmp1
                sta tmp1
                lda 1,x         ; stash # of pages
                pha
                clc
                adc 3,x         ; and add to addr
                sta tmp1+1

                lda 0,x         ; get starting offset
                tay

_tolower_loop:  dey
                cpy #$ff        ; wrapped?
                bne +
                lda 1,x
                beq _tolower_done
                dec 1,x         ; next page
                dec tmp1+1
+
                lda (tmp1),y
                cmp #'A'
                bmi _tolower_loop
                cmp #'Z'+1
                bpl _tolower_loop
                ora #$20        ; lower case
                sta (tmp1),y
                bra _tolower_loop

_tolower_done:  pla
                sta 1,x

z_tolower:      rts

; ## unpack ( u -- lo hi ) "unpack uint16 to lo and hi bytes"
; ## "unpack"  tested ad hoc
xt_unpack:
                dex
                dex
                lda 3,x     ; get hi byte
                sta 0,x     ; push to stack
                stz 1,x
                stz 3,x     ; zero hi byte leaving lo

z_unpack:       rts

; ## pack ( lo hi  -- u ) "pack two char vals to uint16"
; ## "pack"  tested ad hoc
xt_pack:
                lda 0,x     ; pop hi byte
                inx
                inx
                sta 1,x     ; insert it alongside lo byte

z_pack:         rts

; ## asciiz> ( c-addr -- addr u ) "count a zero-terminated string; uses tmp1"
; ## "asciiz"  tested ad hoc
xt_asciiz:
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

; ## DECODE ( strz digrams outz -- addr n ) "decode a dizzy+woozy encoded string"
; ## "decode"  tested ad hoc

xt_decode:
        lda 0,x
        sta txt_outz
        lda 1,x
        sta txt_outz+1

        lda 2,x
        sta txt_digrams
        lda 3,x
        sta txt_digrams+1

        lda 4,x
        sta txt_strz
        lda 5,x
        sta txt_strz+1

        phx
        jsr txt_undizzy
        plx

        lda 0,x             ; undizzy output becomes woozy input
        sta txt_strz
        lda 1,x
        sta txt_strz+1

        inx                 ; drop
        inx

        ; txt_outz points to free space where we'll output
        lda txt_outz
        sta 2,x
        lda txt_outz+1
        sta 3,x

        phx
        jsr txt_unwoozy
        plx

        lda txt_outz    ; new pointer including terminator
        sta 0,x
        lda txt_outz+1
        sta 1,x

        jsr xt_over     ; n = p2 - p1 - 1
        jsr xt_minus
        jsr xt_one_minus

z_decode:
        rts
