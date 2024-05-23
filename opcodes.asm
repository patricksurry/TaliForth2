
; =========================================================
oc_index_table:
        ; Lookup table for the instruction data (length of instruction in
        ; bytes, length of mnemonic in bytes, mnemonic string). This is used by
        ; the assembler as well.

        ; Opcodes 00-0F
        .word oc00, oc01, oc__, oc__, oc04, oc05, oc06, oc07
        .word oc08, oc09, oc0A, oc__, oc0C, oc0D, oc0E, oc0F

        ; Opcodes 10-1F
        .word oc10, oc11, oc12, oc__, oc14, oc15, oc16, oc17
        .word oc18, oc19, oc1A, oc__, oc1C, oc1D, oc1E, oc1F

        ; Opcodes 20-2F
        .word oc20, oc21, oc__, oc__, oc24, oc25, oc26, oc27
        .word oc28, oc29, oc2A, oc__, oc2C, oc2D, oc2E, oc2F

        ; Opcodes 30-3F
        .word oc30, oc31, oc32, oc__, oc34, oc35, oc36, oc37
        .word oc38, oc39, oc3A, oc__, oc3C, oc3D, oc3E, oc3F

        ; Opcodes 40-4F
        .word oc40, oc41, oc__, oc__, oc__, oc45, oc46, oc47
        .word oc48, oc49, oc4A, oc__, oc4C, oc4D, oc4E, oc4F

        ; Opcodes 50-5F
        .word oc50, oc51, oc52, oc__, oc__, oc55, oc56, oc57
        .word oc58, oc59, oc5A, oc__, oc__, oc5D, oc5E, oc5F

        ; Opcodes 60-6F
        .word oc60, oc61, oc__, oc__, oc64, oc65, oc66, oc67
        .word oc68, oc69, oc6A, oc__, oc6C, oc6D, oc6E, oc6F

        ; Opcodes 70-7F
        .word oc70, oc71, oc72, oc__, oc74, oc75, oc76, oc77
        .word oc78, oc79, oc7A, oc__, oc7C, oc7D, oc7E, oc7F

        ; Opcodes 80-8F
        .word oc80, oc81, oc__, oc__, oc84, oc85, oc86, oc87
        .word oc88, oc89, oc8A, oc__, oc8C, oc8D, oc8E, oc8F

        ; Opcodes 90-9F
        .word oc90, oc91, oc92, oc__, oc94, oc95, oc96, oc97
        .word oc98, oc99, oc9A, oc__, oc9C, oc9D, oc9E, oc9F

        ; Opcodes A0-AF
        .word ocA0, ocA1, ocA2, oc__, ocA4, ocA5, ocA6, ocA7
        .word ocA8, ocA9, ocAA, oc__, ocAC, ocAD, ocAE, ocAF

        ; Opcodes B0-BF
        .word ocB0, ocB1, ocB2, oc__, ocB4, ocB5, ocB6, ocB7
        .word ocB8, ocB9, ocBA, oc__, ocBC, ocBD, ocBE, ocBF

        ; Opcodes C0-CF
        .word ocC0, ocC1, oc__, oc__, ocC4, ocC5, ocC6, ocC7
        .word ocC8, ocC9, ocCA, oc__, ocCC, ocCD, ocCE, ocCF

        ; Opcodes D0-DF
        .word ocD0, ocD1, ocD2, oc__, oc__, ocD5, ocD6, ocD7
        .word ocD8, ocD9, ocDA, oc__, oc__, ocDD, ocDE, ocDF

        ; Opcodes E0-EF
        .word ocE0, ocE1, oc__, oc__, ocE4, ocE5, ocE6, ocE7
        .word ocE8, ocE9, ocEA, oc__, ocEC, ocED, ocEE, ocEF

        ; Opcodes F0-FF
        .word ocF0, ocF1, ocF2, oc__, oc__, ocF5, ocF6, ocF7
        .word ocF8, ocF9, ocFA, oc__, oc__, ocFD, ocFE, ocFF


; =========================================================
        ; Opcode data table for the disassember, which is also used by the
        ; assembler. Each entry starts with a "lengths byte":

        ;       bit 7-6:  Length of instruction in bytes (1 to 3 for the 65c02)
        ;       bit 5-3:  unused
        ;       bit 2-0:  Length of mnemonic in chars (3 to 7)

        ; To convert a line in this table to a Forth string of the mnemonic,
        ; use the COUNT word on the address of the lengths byte to get
        ; ( addr u ) and then mask all but the bits 2-0 of the TOS.

        ; To make debugging easier, we keep the raw numbers for the lengths of
        ; the instruction and mnemonics and let the assembler do the math
        ; required to shift and add. The actual mnemonic string follows after
        ; and is not zero terminated because we have the length in bits 2 to 0.

	oc00:	.text 2*64+3, "brk"              ; enforce the signature byte
	oc01:	.text 2*64+7, "ora.zxi"
;      (oc02)
;      (oc03)
    oc04:   .text 2*64+5, "tsb.z"
	oc05:	.text 2*64+5, "ora.z"
	oc06:	.text 2*64+5, "asl.z"
	oc07:	.text 2*64+6, "rmb0.z"
	oc08:	.text 1*64+3, "php"
	oc09:	.text 2*64+5, "ora.#"
	oc0A:	.text 1*64+5, "asl.a"
;      (oc0B)
	oc0C:	.text 3*64+3, "tsb"
	oc0D:	.text 3*64+3, "ora"
	oc0E:	.text 3*64+3, "asl"
	oc0F:	.text 3*64+4, "bbr0"

	oc10:	.text 2*64+3, "bpl"
	oc11:	.text 2*64+7, "ora.ziy"
	oc12:	.text 2*64+6, "ora.zi"
;      (oc13:)
	oc14:	.text 2*64+5, "trb.z"
	oc15:	.text 2*64+6, "ora.zx"
	oc16:	.text 2*64+6, "asl.zx"
	oc17:	.text 2*64+6, "rmb1.z"
	oc18:	.text 1*64+3, "clc"
	oc19:	.text 3*64+5, "ora.y"
	oc1A:	.text 1*64+5, "inc.a"
;      (oc1B:)
	oc1C:	.text 3*64+3, "trb"
	oc1D:	.text 3*64+5, "ora.x"
	oc1E:	.text 3*64+5, "asl.x"
	oc1F:	.text 3*64+4, "bbr1"

	oc20:	.text 3*64+3, "jsr"
	oc21:	.text 2*64+7, "and.zxi"
;      (oc22:)
;      (oc23:)
	oc24:	.text 2*64+5, "bit.z"
	oc25:	.text 2*64+5, "and.z"
	oc26:	.text 2*64+5, "rol.z"
	oc27:	.text 2*64+6, "rmb2.z"
	oc28:	.text 1*64+3, "plp"
	oc29:	.text 2*64+5, "and.#"
	oc2A:	.text 1*64+5, "rol.a"
;      (oc2B:)
	oc2C:	.text 3*64+3, "bit"
	oc2D:	.text 3*64+4, "and."
	oc2E:	.text 3*64+3, "rol"
	oc2F:	.text 3*64+4, "bbr2"

	oc30:	.text 2*64+3, "bmi"
	oc31:	.text 2*64+7, "and.ziy"
	oc32:	.text 2*64+6, "and.zi"
;      (oc33:)
	oc34:	.text 2*64+7, "bit.zxi"
	oc35:	.text 2*64+6, "and.zx"
	oc36:	.text 2*64+6, "rol.zx"
	oc37:	.text 2*64+6, "rmb3.z"
	oc38:	.text 1*64+3, "sec"
	oc39:	.text 3*64+5, "and.y"
	oc3A:	.text 1*64+5, "dec.a"
;      (oc3B:)
	oc3C:	.text 3*64+5, "bit.x"
	oc3D:	.text 3*64+5, "and.x"
	oc3E:	.text 3*64+5, "rol.x"
	oc3F:	.text 3*64+4, "bbr3"

	oc40:	.text 1*64+3, "rti"
	oc41:	.text 2*64+7, "eor.zxi"
;      (oc42:)
;      (oc43:)
;      (oc44:)
	oc45:	.text 2*64+5, "eor.z"
	oc46:	.text 2*64+5, "lsr.z"
	oc47:	.text 2*64+6, "rbm4.z"
	oc48:	.text 1*64+3, "pha"
	oc49:	.text 2*64+5, "eor.#"
	oc4A:	.text 1*64+5, "lsr.a"
;      (oc4B:)
	oc4C:	.text 3*64+3, "jmp"
	oc4D:	.text 3*64+3, "eor"
	oc4E:	.text 3*64+3, "lsr"
	oc4F:	.text 3*64+4, "bbr4"

	oc50:	.text 2*64+3, "bvc"
	oc51:	.text 2*64+7, "eor.ziy"
	oc52:	.text 2*64+6, "eor.zi"
;      (oc53:)
;      (oc54:)
	oc55:	.text 2*64+6, "eor.zx"
	oc56:	.text 2*64+6, "lsr.zx"
	oc57:	.text 2*64+6, "rbm5.z"
	oc58:	.text 1*64+3, "cli"
	oc59:	.text 3*64+5, "eor.y"
	oc5A:	.text 1*64+3, "phy"
;      (oc5B:)
;      (oc5C:)
	oc5D:	.text 3*64+5, "eor.x"
	oc5E:	.text 3*64+5, "lsr.x"
	oc5F:	.text 3*64+4, "bbr5"

	oc60:	.text 1*64+3, "rts"
	oc61:	.text 2*64+7, "adc.zxi"
;      (oc62:)
;      (oc63:)
	oc64:	.text 2*64+5, "stz.z"
	oc65:	.text 2*64+5, "adc.z"
	oc66:	.text 2*64+5, "ror.z"
	oc67:	.text 2*64+6, "rmb6.z"
	oc68:	.text 1*64+3, "pla"
	oc69:	.text 2*64+5, "adc.#"
	oc6A:	.text 1*64+5, "ror.a"
;      (oc6B:)
	oc6C:	.text 3*64+5, "jmp.i"
	oc6D:	.text 3*64+3, "adc"
	oc6E:	.text 3*64+3, "ror"
	oc6F:	.text 3*64+4, "bbr6"

	oc70:	.text 2*64+3, "bvs"
	oc71:	.text 2*64+7, "adc.ziy"
	oc72:	.text 2*64+6, "adc.zi"
;      (oc73:)
	oc74:	.text 2*64+6, "stz.zx"
	oc75:	.text 2*64+6, "adc.zx"
	oc76:	.text 2*64+6, "ror.zx"
	oc77:	.text 2*64+6, "rmb7.z"
	oc78:	.text 1*64+3, "sei"
	oc79:	.text 3*64+5, "adc.y"
	oc7A:	.text 1*64+3, "ply"
;      (oc7B:)
	oc7C:	.text 3*64+6, "jmp.xi"
	oc7D:	.text 3*64+5, "adc.x"
	oc7E:	.text 3*64+5, "ror.x"
	oc7F:	.text 3*64+4, "bbr7"

	oc80:	.text 2*64+3, "bra"
	oc81:	.text 2*64+7, "sta.zxi"
;      (oc82:)
;      (oc83:)
	oc84:	.text 2*64+5, "sty.z"
	oc85:	.text 2*64+5, "sta.z"
	oc86:	.text 2*64+5, "stx.z"
	oc87:	.text 2*64+6, "smb0.z"
	oc88:	.text 1*64+3, "dey"
	oc89:	.text 2*64+5, "bit.#"
	oc8A:	.text 1*64+3, "txa"
;      (oc8B:)
	oc8C:	.text 3*64+3, "sty"
	oc8D:	.text 3*64+3, "sta"
	oc8E:	.text 3*64+3, "stx"
	oc8F:	.text 3*64+4, "bbs0"

	oc90:	.text 2*64+3, "bcc"
	oc91:	.text 2*64+7, "sta.ziy"
	oc92:	.text 2*64+6, "sta.zi"
;      (oc93:)
	oc94:	.text 2*64+6, "sty.zx"
	oc95:	.text 2*64+6, "sta.zx"
	oc96:	.text 2*64+6, "stx.zy"
	oc97:	.text 2*64+6, "smb1.z"
	oc98:	.text 1*64+3, "tya"
	oc99:	.text 3*64+5, "sta.y"
	oc9A:	.text 1*64+3, "txs"
;      (oc9B:)
	oc9C:	.text 3*64+3, "stz"
	oc9D:	.text 3*64+5, "sta.x"
	oc9E:	.text 3*64+5, "stz.x"
	oc9F:	.text 3*64+4, "bbs1"

	ocA0:	.text 2*64+5, "ldy.#"
	ocA1:	.text 2*64+7, "lda.zxi"
	ocA2:	.text 2*64+5, "ldx.#"
;      (ocA3:)
	ocA4:	.text 2*64+5, "ldy.z"
	ocA5:	.text 2*64+5, "lda.z"
	ocA6:	.text 2*64+5, "ldx.z"
	ocA7:	.text 2*64+6, "smb2.z"
	ocA8:	.text 1*64+3, "tay"
	ocA9:	.text 2*64+5, "lda.#"
	ocAA:	.text 1*64+3, "tax"
;      (ocAB:)
	ocAC:	.text 3*64+3, "ldy"
	ocAD:	.text 3*64+3, "lda"
	ocAE:	.text 3*64+3, "ldx"
	ocAF:	.text 3*64+4, "bbs2"

	ocB0:	.text 2*64+3, "bcs"
	ocB1:	.text 2*64+7, "lda.ziy"
	ocB2:	.text 2*64+6, "lda.zi"
;      (ocB3:)
	ocB4:	.text 2*64+6, "ldy.zx"
	ocB5:	.text 2*64+6, "lda.zx"
	ocB6:	.text 2*64+6, "ldx.zy"
	ocB7:	.text 2*64+6, "smb3.z"
	ocB8:	.text 1*64+3, "clv"
	ocB9:	.text 3*64+5, "lda.y"
	ocBA:	.text 1*64+3, "tsx"
;      (ocBB:)
	ocBC:	.text 3*64+5, "ldy.x"
	ocBD:	.text 3*64+5, "lda.x"
	ocBE:	.text 3*64+5, "ldx.y"
	ocBF:	.text 3*64+4, "bbs4"

	ocC0:	.text 2*64+5, "cpy.#"
	ocC1:	.text 2*64+7, "cmp.zxi"
;      (ocC2:)
;      (ocC3:)
	ocC4:	.text 2*64+5, "cpy.z"
	ocC5:	.text 2*64+5, "cmp.z"
	ocC6:	.text 2*64+5, "dec.z"
	ocC7:	.text 2*64+6, "smb4.z"
	ocC8:	.text 1*64+3, "iny"
	ocC9:	.text 2*64+5, "cmp.#"
	ocCA:	.text 1*64+3, "dex"
;      (ocCB:)
	ocCC:	.text 3*64+3, "cpy"
	ocCD:	.text 3*64+3, "cmp"
	ocCE:	.text 3*64+3, "dec"
	ocCF:	.text 3*64+4, "bbs4"

	ocD0:	.text 2*64+3, "bne"
	ocD1:	.text 2*64+7, "cmp.ziy"
	ocD2:	.text 2*64+6, "cmp.zi"
;      (ocD3:)
;      (ocD4:)
	ocD5:	.text 2*64+6, "cmp.zx"
	ocD6:	.text 2*64+6, "dec.zx"
	ocD7:	.text 2*64+6, "smb5.z"
	ocD8:	.text 1*64+3, "cld"
	ocD9:	.text 3*64+5, "cmp.y"
	ocDA:	.text 1*64+3, "phx"
;      (ocDB:)
;      (ocDC:)
	ocDD:	.text 3*64+5, "cmp.x"
	ocDE:	.text 3*64+5, "dec.x"
	ocDF:	.text 3*64+4, "bbs5"

	ocE0:	.text 2*64+5, "cpx.#"
	ocE1:	.text 2*64+7, "sbc.zxi"
;      (ocE2:)
;      (ocE3:)
	ocE4:	.text 2*64+5, "cpx.z"
	ocE5:	.text 2*64+5, "sbc.z"
	ocE6:	.text 2*64+5, "inc.z"
	ocE7:	.text 2*64+6, "smb6.z"
	ocE8:	.text 1*64+3, "inx"
	ocE9:	.text 2*64+5, "sbc.#"
	ocEA:	.text 1*64+3, "nop"
;      (ocEB:)
	ocEC:	.text 3*64+3, "cpx"
	ocED:	.text 3*64+3, "sbc"
	ocEE:	.text 3*64+3, "inc"
	ocEF:	.text 3*64+4, "bbs6"

	ocF0:	.text 2*64+3, "beq"
	ocF1:	.text 2*64+7, "sbc.ziy"
	ocF2:	.text 2*64+6, "sbc.zi"
;      (ocF3:)
;      (ocF4:)
	ocF5:	.text 2*64+6, "sbc.zx"
	ocF6:	.text 2*64+6, "inc.zx"
	ocF7:	.text 2*64+6, "smb7.z"
	ocF8:	.text 1*64+3, "sed"
	ocF9:	.text 3*64+5, "sbc.y"
	ocFA:	.text 1*64+3, "plx"
;      (ocFB:)
;      (ocFC:)
	ocFD:	.text 3*64+5, "sbc.x"
	ocFE:	.text 3*64+5, "inc.x"
	ocFF:	.text 3*64+4, "bbs7"

        ; Common routine for opcodes that are not supported by the 65c02
	oc__:	.text 1, "?"
