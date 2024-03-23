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


; ## CS_FETCH ( addr -- sc ) "Get a byte with sign extension from address"
; ## "cs@"
xt_cs_fetch:
                jsr underflow_1

                ldy #0      ; assume msb is zero
                lda (0,x)
                sta 0,x
                bpl _plus
                dey         ; extend sign if byte is negative
_plus:          tya
                sta 1,x
z_cs_fetch:     rts


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
        bra jsr_blkrw

; ## blk_read ( blk buf -- ) "read a 1024-byte block from blk to buf"
; ## "blk-read"  tested ad hoc
xt_blk_read:
        ldy #1
jsr_blkrw:
        jsr blkrw
        inx             ; free stack
        inx
        inx
        inx
z_blk_write:
z_blk_read:
        rts


blkrw:      ; ( blk buf -- blk buf ) ; Y = 1/2 for r/w
        lda 0,x
        sta $c014       ; buffer
        lda 1,x
        sta $c015
        lda 2,x
        sta $c012       ; blk number
        lda 3,x
        sta $c013
        sty $c010       ; go
        rts


; blk-write-n ( blk addr n ) loop over blk_write n times (n <= 64)
xt_blk_write_n:
        ldy #2
        bra blk_rw_n

; blk-read-n ( blk addr n ) loop over blk_read n times (n <= 64)
xt_blk_read_n:
        ldy #1
blk_rw_n:
        sty tmp1+1      ; 1=read, 2=write

        lda 0,x
        sta tmp1        ; block count (unsigned byte)
        inx             ; remove n from stack
        inx
        cmp #0          ; any blocks to read?
        beq _cleanup

_loop:
        ldy tmp1+1
        jsr blkrw

        lda #4          ; addr += 1024 = $400
        clc
        adc 1,x
        sta 1,x

        inc 2,x         ; blk += 1
        bne +
        inc 3,x
+
        dec tmp1        ; n--
        bne _loop

_cleanup:
        inx             ; clear stack
        inx
        inx
        inx
z_blk_read_n:
z_blk_write_n:
        rts


; ## shutdown ( -- ) "exit the matrix aka c65"
; ## "shutdown"  tested ad hoc
xt_shutdown:
        lda #$ff
        sta $c010       ; that's all folks...
z_shutdown:
        rts


; linkz decode 4 byte packed representation into 3 words
; ( link-addr -- dest' verb cond' )
;
;           addr+3          addr+2             addr+1           addr+0
;    +-----------------+-----------------+-----------------+-----------------+
;    | 7 6 5 4 3 2 1 0 | 7 6 5 4 3 2 1 0 | 7 6 5 4 3 2 1 0 | 7 6 5 4 3 2 1 0 |
;    +------+-----+----+-----------------+--------------+--+-----------------+
;    | . . .|  cf | dt |     dest        |     cobj     |          verb      |
;    +------+-----+----+-----------------+--------------+--+-----------------+
;             1,x   5,x      4,x               0,x       3,x       2,x
xt_decode_link:
        lda 0,x         ; copy addr to tmp1
        sta tmp1
        lda 1,x
        sta tmp1+1

        dex             ; make space for cond' @ 0-1, verb @ 2-3, dest at 4-5
        dex
        dex
        dex

        ldy #0
        lda (tmp1),y
        sta 2,x         ; verb lo
        iny
        lda (tmp1),y
        lsr
        sta 0,x         ; cond lo
        lda #0
        rol
        sta 3,x         ; verb hi
        iny
        lda (tmp1),y
        sta 4,x         ; dest lo
        iny
        lda (tmp1),y
        tay
        and #3
        sta 5,x         ; dest hi
        tya
        lsr
        lsr
        sta 1,x         ; cond hi
z_decode_link:
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
