

; woozy temps
txt_repeat  = txt_tmp

; unwrap temps
txt_col     = txt_tmp
txt_row     = txt_tmp+1


txt_putc:       ; (A) -> nil
    ; write a chr to output buffer and inc position
        sta (txt_outz)
        inc txt_outz
        bne _done
        inc txt_outz+1
_done:  rts


txt_inc_strz:
        inc txt_strz    ; inc pointer
        bne txt_rts
txt_inc_strz1:
        inc txt_strz+1
txt_rts:   rts

txt_iny_strz:
        iny
        bne txt_rts
        beq txt_inc_strz1


txt_wrap:   ; (txt_strz) -> nil
    ; replace selected whitespace chrs with newline / formfeed
    ; to give nicer wrapping on a small screen
    ; modifies string at txt_strz in place

        ; txt_strz points at current char, with screen coord (txt_col, txt_row)
        stz txt_col
        stz txt_row

_scan:  ldy #0          ; seek forward within row for next natural break
        ldx txt_col     ; y tracks char offset, x tracks updated txt_col

        bit txt_rts     ; set overflow with RTS = #$60 (first pass V=1, second V=0)

_skip:  cpx #SCR_WIDTH      ; skip past sequence of ws (first pass) then non-ws (second pass)
        bpl _eol
        lda (txt_strz),y
        beq _end        ; end of string?
        cmp #' '+1      ; is char whitespace (' ' or below)
        bvc _chk2       ; second pass?
        bmi _cont       ; continue first pass on ws
        clv             ; else start second pass
        bra _skip

_chk2:  bmi _adv        ; end second pass on ws
                        ;TODO handle tab which could bump col more than one
_cont:  inx             ; update col and offset
        iny
        bne _skip       ;TODO what happens with y overflow?

        ; advance natural break point
_adv:   tya         ; add offset y to txt_strz
        clc
        adc txt_strz
        sta txt_strz
        bcc _chknl
        inc txt_strz+1
_chknl: lda (txt_strz)  ; if curr chr is NL, force a break
        cmp #AscCR
        beq _eol
        cmp #AscLF
        beq _eol
        stx txt_col     ; update current natural break
        bra _scan       ; keep looking

_eol:   stz txt_col     ; reset txt_col, increment txt_row
        ldx #AscLF      ; normally insert LF, but FF at end of page
        lda txt_row
        ina
        cmp #SCR_HEIGHT
        bne _nopg
        lda #0
        ldx #AscFF      ; new page insert FF
_nopg:  sta txt_row     ; current txt_row
        phx             ; stash brk chr
        lda (txt_strz)
        cmp #' '+1
        bmi _soft

        ; hard break, skip ahead y and carry on
        plx         ; discard brk chr
        ldx #0
        bra _adv    ; advance breakpoint with offset y, at column x=0

_soft:  pla         ; insert our break char and skip past it
        sta (txt_strz)
        jsr txt_inc_strz
        bra _scan

_end:   rts


txt_unwoozy:                ; (txt_strz, txt_outz) -> nil
    ; undo woozy prep for dizzy
        ldy #0
        ldx #0              ; shift state, 0 = none, 1 = capitalize, 2 = all caps
        lda #1
        sta txt_repeat      ; repeat count for output

_loop:  lda (txt_strz),y
        beq _done
        cmp #$0d            ; $b,c: set shift status
        bpl _out
        cmp #$0b
        bmi _nobc
        sbc #$0a
        tax                 ; save shift state
        bra _next

_nobc:  cmp #$09            ; $3-8: rle next char
        bpl _out
        cmp #$03
        bmi _out
        sta txt_repeat
        bra _next

_out:   cmp #'A'
        bmi _notuc
        cmp #'Z'+1
        bpl _notuc
        ora #%0010_0000     ; lowercase
        pha
        lda #' '            ; add a space
        jsr _repeat_putc
        pla

_notuc: cpx #0
        beq _putc
        cmp #'a'
        bmi _notlc
        cmp #'z'+1
        bpl _notlc
        and #%0101_1111     ; capitalize
        cpx #2              ; all caps?
        beq _putc
_notlc: ldx #0              ; else end shift

_putc:  jsr _repeat_putc

_next:  jsr txt_iny_strz
        bra _loop

_done:  jsr txt_putc        ; store the nul terminator
        rts

_repeat_putc:
_rpt:   jsr txt_putc
        dec txt_repeat
        bne _rpt
        inc txt_repeat      ; reset to 1
        rts


txt_undizzy:                ; (txt_strz, txt_digrams, txt_outz) -> nil
    ; uncompress a zero-terminated dizzy string at txt_strz using txt_digrams lookup
    ; writes uncompressed data to txt_outz (including the terminator)
    ; outz is left pointing past the terminator

        ldx #0              ; track stack depth
_nextz: lda (txt_strz)      ; get encoded char
_chk7:  bmi _subst          ; is it a digraph (bit 7 set)?
        beq _done           ; if 0 we're done
        jsr txt_putc
_stk:   cpx #0              ; any stacked items?
        beq _cont
        dex
        pla                 ; pop latest
        bra _chk7

_subst: sec
        rol                 ; index*2+1 for second char in digram
        tay
        lda (txt_digrams),y
        inx                 ; track stack depth
        pha                 ; stack the second char
        dey
        lda (txt_digrams),y ; fetch the first char of the digram
        bra _chk7           ; keep going

_cont:  jsr txt_inc_strz
        bra _nextz

_done:  jsr txt_putc         ; store the terminator
        rts


.if TEST

test_buf = $400

PUTC:   sta $f001            ; pymon character output, or could write buffer etc
        rts

test_start:

test_undizzy:
        lda #<test_digrams
        sta txt_digrams
        lda #>test_digrams
        sta txt_digrams+1

        ; undizzy: dzy -> buf
        lda #<test_dzy
        sta txt_strz
        lda #>test_dzy
        sta txt_strz+1

        lda #<test_buf
        sta txt_outz
        lda #>test_buf
        sta txt_outz+1

        jsr txt_undizzy

        ; unwoozy: buf -> dzy

        lda #<test_buf
        sta txt_strz
        lda #>test_buf
        sta txt_strz+1

        lda #<test_dzy
        sta txt_outz
        lda #>test_dzy
        sta txt_outz+1

        jsr txt_unwoozy

        ; wrap: dzy

        lda #<test_dzy
        sta txt_strz
        lda #>test_dzy
        sta txt_strz+1

        jsr txt_wrap        ; wrap it

        lda #<test_dzy
        sta txt_strz
        lda #>test_dzy
        sta txt_strz+1

        ldy #0              ; print it
_loop:  lda (txt_strz),y
        beq _done
        cmp #AscFF
        bne _putc
        lda #AscLF
        jsr PUTC
        lda #'*'
        jsr PUTC
        lda #LF
_putc:  jsr PUTC
        iny
        bne _loop
        inc txt_strz+1
        bra _loop
_done:  brk


    .data
test_digrams:
        .byte $68, $65, $72, $65, $6f, $75, $54, $80, $69, $6e, $73, $74, $84, $67, $6e, $64
        .byte $69, $74, $6c, $6c, $49, $6e, $65, $72, $61, $72, $2e, $0b, $4f, $66, $0b, $79
        .byte $8f, $82, $65, $73, $6f, $72, $49, $73, $59, $82, $6f, $6e, $6f, $6d, $54, $6f
        .byte $61, $6e, $6f, $77, $6c, $65, $61, $73, $76, $65, $61, $74, $74, $80, $41, $81
        .byte $0b, $9e, $65, $6e, $42, $65, $67, $65, $61, $89, $65, $64, $41, $87, $54, $68
        .byte $90, $9f, $69, $64, $74, $68, $65, $81, $73, $61, $61, $64, $52, $6f, $69, $63
        .byte $9b, $ac, $6c, $79, $63, $6b, $27, $81, $41, $4c, $65, $74, $50, $b0, $6c, $6f
        .byte $69, $73, $67, $68, $4f, $6e, $43, $98, $90, $b3, $41, $74, $49, $74, $65, $ad
        .byte $88, $74, $88, $68, $75, $74, $61, $6d, $6f, $74, $a8, $8a, $8d, $83, $57, $c1
        .byte $69, $85, $4d, $61, $53, $74, $41, $6e, $72, $6f, $81, $93, $57, $68, $45, $87
        .byte $8e, $83, $69, $72, $76, $8b, $48, $ab, $63, $74, $ae, $96, $65, $85, $61, $9c
        .byte $61, $79, $53, $65, $20, $22, $61, $6c, $61, $85, $69, $95, $6b, $65, $72, $61
        .byte $8a, $83, $46, $72, $45, $78, $b6, $a3, $27, $74, $72, $82, $c0, $9a, $55, $70
        .byte $2c, $41, $52, $65, $a0, $cd, $72, $79, $97, $83, $41, $53, $6c, $64, $e1, $96
        .byte $75, $81, $a9, $65, $63, $65, $57, $d6, $b9, $74, $69, $f4, $bc, $8a, $0b, $64
        .byte $43, $68, $6e, $74, $50, $88, $96, $65, $98, $74, $4f, $c2, $44, $69, $9d, $65
test_dzy:
        .byte $0b, $73, $fb, $77, $80, $81, $4e, $65, $8c, $62, $79, $93, $0b, $43, $6f, $b7
        .byte $73, $ac, $6c, $0b, $43, $d7, $2c, $57, $80, $81, $4f, $9e, $72, $73, $48, $d7
        .byte $46, $82, $87, $46, $92, $74, $75, $6e, $91, $8a, $54, $81, $9b, $f0, $a6, $47
        .byte $6f, $ee, $2c, $a7, $82, $b9, $be, $93, $52, $75, $6d, $6f, $81, $64, $a7, $9d
        .byte $53, $fb, $ce, $6f, $45, $f9, $8b, $9f, $4e, $65, $d2, $d9, $a1, $41, $67, $61
        .byte $84, $8d, $c9, $67, $af, $93, $53, $61, $a9, $97, $57, $92, $6b, $e0, $43, $d7
        .byte $8d, $49, $57, $69, $89, $a2, $94, $72, $45, $79, $91, $a6, $48, $61, $87, $73
        .byte $8d, $fe, $81, $d4, $4d, $65, $c7, $43, $96, $6d, $61, $87, $73, $8e, $20, $31
        .byte $4f, $72, $20, $32, $57, $92, $64, $73, $8d, $49, $53, $68, $82, $ee, $57, $8c
        .byte $6e, $94, $a7, $9d, $0b, $49, $4c, $6f, $6f, $6b, $bd, $ba, $b1, $83, $46, $d1
        .byte $85, $46, $69, $9c, $4c, $b5, $74, $8b, $73, $8e, $45, $61, $63, $68, $57, $92
        .byte $64, $2c, $53, $6f, $94, $27, $89, $48, $d7, $97, $45, $f9, $8b, $da, $0b, $6e
        .byte $92, $9e, $dc, $22, $41, $73, $da, $6e, $65, $22, $97, $44, $c8, $86, $75, $b8
        .byte $68, $be, $ef, $da, $0b, $6e, $92, $aa, $22, $2e, $20, $28, $0b, $73, $68, $82
        .byte $ee, $94, $47, $b5, $ca, $75, $b2, $2c, $54, $79, $70, $65, $da, $80, $6c, $70
        .byte $22, $46, $92, $53, $fb, $47, $a1, $8b, $db, $48, $84, $74, $73, $29, $2e, $00

.endif
