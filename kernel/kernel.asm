TEST    = 0                     ; compile unit tests?

SCR_WIDTH   = 20                ; sreen width, < 256
SCR_HEIGHT  = 4

AscFF   = $0f  ; form feed

kernel_zp   = zpage_end + 1

rand16      = kernel_zp
txt_strz    = kernel_zp + 2     ; input zero-terminated string
txt_outz    = kernel_zp + 4     ; output buffer for zero-terminated string
txt_digrams = kernel_zp + 6     ; digram lookup table (128 2-byte pairs)
txt_tmp     = kernel_zp + 8

    .include "util.asm"
    .include "txt.asm"
    .include "words.asm"
