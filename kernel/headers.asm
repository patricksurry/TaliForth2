nt_random:
        .byte 6, 0
        .word nt_tolower, xt_random, z_random
        .text "random"

nt_tolower:
        .byte 7, 0
        .word nt_asciiz, xt_tolower, z_tolower
        .text "tolower"

nt_asciiz:
        .byte 7, 0
        .word nt_unpack, xt_asciiz, z_asciiz
        .text "asciiz>"

nt_unpack:
        .byte 6, 0
        .word nt_pack, xt_unpack, z_unpack
        .text "unpack"

nt_pack:
        .byte 4, 0
        .word nt_typez, xt_pack, z_pack
        .text "pack"

nt_typez:
        .byte 5, 0
        .word +, xt_typez, z_typez
        .text "typez"
+
