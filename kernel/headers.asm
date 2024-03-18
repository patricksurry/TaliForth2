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
        .word nt_blk_read, xt_asciiz, z_asciiz
        .text "asciiz>"

nt_blk_read:
        .byte 8, 0
        .word nt_blk_write, xt_blk_read, z_blk_read
        .text "blk-read"

nt_blk_write:
        .byte 9, 0
        .word nt_shutdown, xt_blk_write, z_blk_write
        .text "blk-write"

nt_shutdown:
        .byte 8, 0
        .word nt_unpack, xt_shutdown, z_shutdown
        .text "shutdown"

nt_unpack:
        .byte 6, 0
        .word nt_pack, xt_unpack, z_unpack
        .text "unpack"

nt_pack:
        .byte 4, 0
        .word nt_byte_extend, xt_pack, z_pack
        .text "pack"

nt_byte_extend:
        .byte 3, 0
        .word nt_typez, xt_byte_extend, z_byte_extend
        .text "-b-"

nt_typez:
        .byte 5, 0
        .word +, xt_typez, z_typez
        .text "typez"
+
