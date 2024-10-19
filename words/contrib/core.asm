;----------------------------------------------------------------------
; general helper words
;----------------------------------------------------------------------

#nt_header le, "<="
xt_le:
        jsr underflow_2
w_le:
        jsr w_greater_than
        lda 0,x
        eor #$ff
        sta 0,x
        sta 1,x
z_le:
        rts

#nt_header ge, ">="
xt_ge:
        jsr underflow_2
w_ge:
        jsr w_less_than
        lda 0,x
        eor #$ff
        sta 0,x
        sta 1,x
z_ge:
        rts

