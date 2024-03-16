TEST        = 0                 ; compile unit tests?

SCR_WIDTH   = 40                ; sreen width, < 256
SCR_HEIGHT  = 8

AscFF       = $0f               ; form feed

kernel_zp   = zpage_end + 1

rand16      = kernel_zp
txt_strz    = kernel_zp + 2     ; input zero-terminated string
txt_outz    = kernel_zp + 4     ; output buffer for zero-terminated string
txt_digrams = kernel_zp + 6     ; digram lookup table (128 2-byte pairs)

; unwrap temps
txt_col     = kernel_zp + 8
txt_row     = kernel_zp + 9
wrp_col     = kernel_zp + $a
wrp_flg     = kernel_zp + $b

; woozy temps
txt_repeat  = kernel_zp + $c
txt_shift   = kernel_zp + $d
txt_chr     = kernel_zp + $e

; dizzy temp
txt_stack   = kernel_zp + $f

cb_head     = kernel_zp + $10
cb_tail     = kernel_zp + $14

    .include "util.asm"
    .include "txt.asm"
    .include "words.asm"
