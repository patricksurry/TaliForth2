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


; ## UNPACK ( u -- lo hi ) "unpack uint16 to lo and hi bytes"
; ## "unpack"  tested ad hoc
xt_unpack:
                dex
                dex
                lda 3,x     ; get hi byte
                sta 0,x     ; push to stack
                stz 1,x
                stz 3,x     ; zero hi byte leaving lo

z_unpack:       rts


; ## PACK ( lo hi  -- u ) "pack two char vals to uint16"
; ## "pack"  tested ad hoc
xt_pack:
                lda 0,x     ; pop hi byte
                inx
                inx
                sta 1,x     ; insert it alongside lo byte

z_pack:         rts


; ## BYTE_EXTEND ( c -- s ) "sign extend signed char to signed word"
; ## "-b-"  tested ad hoc
xt_byte_extend:
                lda 0,x
                bpl z_byte_extend
                lda #$ff
                sta 1,x
z_byte_extend:
                rts


; ## ASCIIZ> ( c-addr -- addr u ) "count a zero-terminated string; uses tmp1"
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


; ## blk_write ( blk buf -- ) "write a 1024-byte block from buf to blk"
; ## "blk-write"  tested ad hoc
xt_blk_write:
        ldy #2
        bra blk_rw

; ## blk_read ( blk buf -- ) "read a 1024-byte block from blk to buf"
; ## "blk-read"  tested ad hoc
xt_blk_read:
        ldy #1
blk_rw:
        lda 0,x
        sta $c014       ; buffer
        lda 1,x
        sta $c015
        lda 2,x
        sta $c012       ; blk number
        lda 3,x
        sta $c013
        inx             ; free stack
        inx
        inx
        inx
        sty $c010       ; go
z_blk_write:
z_blk_read:
        rts


; ## shutdown ( -- ) "exit the matrix aka c65"
; ## "shutdown"  tested ad hoc
xt_shutdown:
        lda #$ff
        sta $c010       ; that's all folks...
z_shutdown:
        rts


; ## typez ( strz digrams -- ) "emit a wrapped dizzy+woozy encoded string"
; ## "typez"  tested ad hoc
xt_typez:
        lda 0,x
        sta txt_digrams
        lda 1,x
        sta txt_digrams+1

        lda 2,x
        sta txt_strz
        lda 3,x
        sta txt_strz+1

        phx
        jsr txt_typez
        plx
        inx
        inx
        inx
        inx

z_typez:
        rts
