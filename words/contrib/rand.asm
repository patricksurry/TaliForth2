; ## RANDOM ( -- n ) "Return a non-zero random word"
; ## "random"  tested ad hoc
#nt_header random
xt_random:
w_random:
        jsr rng_798
        dex
        dex
        lda rand16
        sta 0,x
        lda rand16+1
        sta 1,x
z_random:
        rts


; ## RANDINT( n -- k ) "Return random unsigned k in [0, n) without modulo bias"
; ## "randint"  tested ad hoc
#nt_header randint
xt_randint:
        jsr underflow_1
w_randint:
        txa                 ; set up stack for initial division
        sec
        sbc #6
        tax
        lda #$ff
        sta 5,x
        sta 4,x
        stz 3,x
        stz 2,x
        lda 7,x
        sta 1,x
        lda 6,x
        sta 0,x
        ; ( n {$ffff 0} n )
        jsr w_um_slash_mod         ; ( ud u -- rem quo )
        ; ( n rem quo )
_retry:
        jsr w_nip
        jsr w_over
        jsr w_random
        jsr w_one_minus            ; random is non-zero, so -1
        ; ( n quo n rand0 )
        jsr w_zero
        jsr w_rot
        ; ( n quo {rand0 0} n )
        ; use /mod to get the candidate remainder, but discard
        ; if the quotient rand0 // n == $ffff // n since not all
        ; potential results are equally represented at the tail end
        jsr w_um_slash_mod
        ; ( n quo rem quo' )
        jsr w_rot
        jsr w_tuck
        ; ( n rem quo quo' quo )
        inx                 ; 2drop and compare
        inx
        inx
        inx
        lda $fc,x
        cmp $fe,x
        bne _done
        lda $fd,x
        cmp $ff,x
        bne _done
        bra _retry
_done:
        ; ( n k quo )
        inx
        inx
        inx
        inx
        lda $fe,x
        sta 0,x
        lda $ff,x
        sta 1,x
z_randint:
        rts
