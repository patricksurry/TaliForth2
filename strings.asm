; List of Strings for Tali Forth 2
; Scot W. Stevenson <scot.stevenson@gmail.com>
; First version: 01. Apr 2016 (for Liara Forth)
; This version: 28. Dec 2018

; This file is included by taliforth.asm

; ## GENERAL STRINGS

; All general strings must be zero-terminated, names start with "s_",
; aliases with "str_"

str_ok             =  0
str_compile        =  1
str_redefined      =  2
str_wid_forth      =  3
str_abc_lower      =  4                     ; note unused
str_abc_upper      =  5
.if "wordlist" in TALI_OPTIONAL_WORDS
str_wid_editor     =  6
str_wid_assembler  =  7
str_wid_root       =  8
.endif
.if "disassembler" in TALI_OPTIONAL_WORDS
str_see_flags      =  9
str_see_nt         = 10
str_see_xt         = 11
str_see_size       = 12
str_disasm_lit     = 13
str_disasm_sdc     = 14
str_disasm_bra     = 15
.endif

; Since we can't fit a 16-bit address in a register, we use indexes as offsets
; to tables as error and string numbers.
string_table:
        .word s_ok, s_compiled, s_redefined, s_wid_forth, s_abc_lower ; 0-4
        .word s_abc_upper, s_wid_editor, s_wid_asm, s_wid_root        ; 5-8
.if "disassembler" in TALI_OPTIONAL_WORDS
        .word s_see_flags, s_see_nt, s_see_xt, s_see_size             ; 9-12
        .word s_disasm_lit, s_disasm_sdc, s_disasm_bra                ; 13-15
.endif

s_ok:         .text " ok", 0         ; note space at beginning
s_compiled:   .text " compiled", 0   ; note space at beginning
s_redefined:  .text "redefined ", 0  ; note space at end

s_abc_lower:
.if len(TALI_OPTIONAL_WORDS) > 0
    .text "0123456789abcdefghijklmnopqrstuvwxyz"
.endif
s_abc_upper:  .text "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"

.if "wordlist" in TALI_OPTIONAL_WORDS
s_wid_asm:    .text "Assembler ", 0  ; Wordlist ID 2, note space at end
s_wid_editor: .text "Editor ", 0     ; Wordlist ID 1, note space at end
s_wid_forth:  .text "Forth ", 0      ; Wordlist ID 0, note space at end
s_wid_root:   .text "Root ", 0       ; Wordlist ID 3, note space at end
.else
s_wid_asm:
s_wid_editor:
s_wid_forth:
s_wid_root:
.endif

.if "disassembler" in TALI_OPTIONAL_WORDS
s_see_flags:  .text "flags (CO AN IM NN UF HC): ", 0
s_see_nt:     .text "nt: ", 0
s_see_xt:     .text "xt: ", 0
s_see_size:   .text "size (decimal): ", 0

s_disasm_lit: .text "LITERAL ", 0
s_disasm_sdc: .text "STACK DEPTH CHECK", 0
s_disasm_bra: .text "BRANCH ",0
.endif

; ## ERROR STRINGS

; All error strings must be zero-terminated, all names start with "es_",
; aliases with "err_". If the string texts are changed, the test suite must be
; as well

err_allot        = 0
err_badsource    = 1
err_compileonly  = 2
err_defer        = 3
err_divzero      = 4
err_noname       = 5
err_refill       = 6
err_state        = 7
err_syntax       = 8
err_underflow    = 9
err_negallot     = 10
err_wordlist     = 11
err_blockwords   = 12
err_returnstack  = 13

error_table:
        .word es_allot, es_badsource, es_compileonly, es_defer  ;  0-3
        .word es_divzero, es_noname, es_refill, es_state        ;  4-7
        .word es_syntax, es_underflow, es_negallot, es_wordlist ;  8-11
        .word es_blockwords, es_returnstack                     ; 12-13

.if len(TALI_OPTIONAL_WORDS) > 0
es_allot:       .text "ALLOT using all available memory", 0
es_badsource:   .text "Illegal SOURCE-ID during REFILL", 0
es_compileonly: .text "Interpreting a compile-only word", 0
es_defer:       .text "DEFERed word not defined yet", 0
es_divzero:     .text "Division by zero", 0
es_noname:      .text "Parsing failure", 0
es_refill:      .text "QUIT could not get input (REFILL returned -1)", 0
es_state:       .text "Already in compile mode", 0
es_syntax:      .text "Undefined word", 0
es_underflow:   .text "Data stack underflow", 0
es_negallot:    .text "Max memory freed with ALLOT", 0
es_wordlist:    .text "No wordlists available", 0
es_blockwords:  .text "Please assign vectors BLOCK-READ-VECTOR and BLOCK-WRITE-VECTOR",0
es_returnstack: .text "Return stack:", 0
.else
es_allot:       .text "EALLT", 0
es_badsource:   .text "ESRC", 0
es_compileonly: .text "ECMPL", 0
es_defer:       .text "EDEFR", 0
es_divzero:     .text "EDIV0", 0
es_noname:      .text "ENAME", 0
es_refill:      .text "EREFL", 0
es_state:       .text "ESTAT", 0
es_syntax:      .text "ESNTX", 0
es_underflow:   .text "EUNDR", 0
es_negallot:    .text "ENALT", 0
es_wordlist:    .text "", 0
es_blockwords:  .text "",0
es_returnstack: .text "RS", 0
.endif

.if "environment?" in TALI_OPTIONAL_WORDS
; ## ENVIRONMENT STRINGS

; These are used by the ENVIRONMENT? word and stored in the old string format:
; Length byte first, then the string itself that is not rpt. not
; zero-terminated. Note these are uppercase by ANS defintion. All start with
; "envs_".

; These return a single-cell number
envs_cs:        .text 15, "/COUNTED-STRING"
envs_hold:      .text 5, "/HOLD"
envs_pad:       .text 4, "/PAD"
envs_aub:       .text 17, "ADDRESS-UNIT-BITS"
envs_floored:   .text 7, "FLOORED"
envs_max_char:  .text 8, "MAX-CHAR"
envs_max_n:     .text 5, "MAX-N"
envs_max_u:     .text 5, "MAX-U"
envs_rsc:       .text 18, "RETURN-STACK-CELLS"
envs_sc:        .text 11, "STACK-CELLS"
envs_wl:        .text 9, "WORDLISTS"

; These return a double-cell number
envs_max_d:     .text 5, "MAX-D"
envs_max_ud:    .text 6, "MAX-UD"
.endif

; END
