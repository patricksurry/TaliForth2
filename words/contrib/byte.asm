
;----------------------------------------------------------------------
; byte manipulation
;----------------------------------------------------------------------

; ## UNPACK ( u -- lo hi ) "unpack uint16 to lo and hi bytes"
; ## "unpack"  tested ad hoc
#nt_header unpack
xt_unpack:
                jsr underflow_1
w_unpack:
                dex
                dex
                lda 3,x     ; get hi byte
                sta 0,x     ; push to stack
                stz 1,x
                stz 3,x     ; zero hi byte leaving lo

z_unpack:       rts


; ## PACK ( lo hi  -- u ) "pack two char vals to uint16"
; ## "pack"  tested ad hoc
#nt_header pack
xt_pack:
                jsr underflow_2
w_pack:
                lda 0,x     ; pop hi byte
                inx
                inx
                sta 1,x     ; insert it alongside lo byte

z_pack:         rts


; ## CS_FETCH ( addr -- sc ) "Get a byte with sign extension from address"
; ## "cs@"
#nt_header cs_fetch, "cs@"
xt_cs_fetch:
                jsr underflow_1
w_cs_fetch:
                ldy #0      ; assume msb is zero
                lda (0,x)
                sta 0,x
                bpl _plus
                dey         ; extend sign if byte is negative
_plus:          tya
                sta 1,x
z_cs_fetch:     rts

