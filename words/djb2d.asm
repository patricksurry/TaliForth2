        .cpu "65c02"
        .enc "none"

djb_hash = scratch+4
djb_sptr = tmptos
djb_len = tmpdsp


xt_djb2d:   ; ( addr n -- dhash )
        jsr underflow_2

w_djb2d:
        lda 2,x
        sta djb_sptr
        lda 3,x
        sta djb_sptr+1

        inc 1,x         ; increment page count for zero test

        sec             ; initialize hash on first pass
        lda 0,x         ; is there a partial page?
        sta djb_len
        beq +
-
        jsr djb2d0      ; leaves djb_len as 0 for full pages
+
        clc             ; continue current hash on further passes
        dec 1,x         ; completed full or partial page
        bne -           ; next page

        ; NUXI order means the result <h3 h2 h1 h0> appears on the stack as

        ;   0,x  1,x  2,x  3,x
        ;    h2   h3   h0   h1

        lda djb_hash    ; return value
        sta 2,x
        lda djb_hash+1
        sta 3,x
        lda djb_hash+2
        sta 0,x
        lda djb_hash+3
        sta 1,x

z_djb2d:
        rts


djb2d0:
        bcc djb2_cont   ; initialize if C=1
djb2d:
            ; implements the DJB2 32-bit hash (see http://www.cse.yorku.ca/~oz/hash.html)
            ; according to the recurrence:
            ;
            ;   hash(-1) = 0
            ;   hash(i) = hash(i - 1) * 65599 + str[i]
            ;
            ; the magic constant 65599 is equal to 2^6 + 2^16 - 1 which simplifies the calculation
            ;
            ; this inner routine assumes the string is in (sptr) with length in 0,x
            ; where length=0 is treated as 256, with the current hash value in hash[0..3]


            ; hex s" fubar" djb2d ud. => $846e4964 or reversed 'rabuf'  => $110E7364

        stz djb_hash
        stz djb_hash+1
        stz djb_hash+2
        stz djb_hash+3

djb2_cont:
        ldy #0                  ; string index
loop:
        ; calculate hash - str[y] - hash << 16
        ; as a four byte subtraction <h3 h2 h1 h0> - <h1 h0 00 ch>
        ; making a copy of hash as we go
        sec
        lda djb_hash+0
        sta scratch+0
        sbc (djb_sptr),y
        sta djb_hash+0

        lda djb_hash+1
        sta scratch+1
        sbc #0
        sta djb_hash+1

        lda djb_hash+2
        sta scratch+2
        sbc scratch+0
        sta djb_hash+2

        lda djb_hash+3
        sta scratch+3
        sbc scratch+1
        sta djb_hash+3

        ; calculate scratch >>= 2, which leaves <s2 s1 s0 a> = hash<<6
        lda #2              ; stop bit leaves C=1 after second pass
-
        lsr scratch+3
        ror scratch+2
        ror scratch+1
        ror scratch+0
        ror a
        bcc -

        ; calculate hash << 6 - (hash - ch - hash << 16)
        ; as the four byte subtraction <h3 h2 h1 h0> = <s2 s1 s1 s0 a> - <h3 h2 h1 h0>
        ; leaving our desired result: hash << 16 + hash << 6 - hash + ch

        sbc djb_hash+0
        sta djb_hash+0
        lda scratch+0
        sbc djb_hash+1
        sta djb_hash+1
        lda scratch+1
        sbc djb_hash+2
        sta djb_hash+2
        lda scratch+2
        sbc djb_hash+3
        sta djb_hash+3

        iny
        dec djb_len
        bne loop

        rts

