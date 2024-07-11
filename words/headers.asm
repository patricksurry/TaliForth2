; Dictionary Headers for Tali Forth 2
; Scot W. Stevenson <scot.stevenson@gmail.com>
; Updated by Sam Colwell
; First version: 05. Dec 2016 (Liara Forth)
; This version: 01. Jan 2023

; Dictionary headers are kept separately from the code, which allows various
; tricks in the code. We roughly follow the Gforth terminology: The Execution
; Token (xt) is the address of the first byte of a word's code that can be, uh,
; executed; the Name Token (nt) is a pointer to the beginning of the word's
; header in the Dictionary.   Conceptually the header looks like this:

;   nt_word --> +----------------+
;               | Status flags   |
;               +----------------+
;               | Name length    |
;               +----------------+
;               | Last header    | -> nt_last_word
;               +----------------+
;               | Start of code  | -> xt_word
;               +----------------+
;               | Length of code | -> z_word - xt_word
;               +----------------+
;               | Name string    | (name length bytes, not zero terminated)
;               +----------------+
;
; In practice we use variable sizes for several of the fields so that
; each header is between 4-8 bytes plus the word's name, with most headers
; taking only four bytes.  The variable size fields are indicated in the status byte
; along with other flags defined in definitions.asm:
;
; Status flags byte
;
;  (msb)  7    6    5    4    3    2    1    0  (lsb)
;       +----+----+----+----+----+----+----+----+
;       | 0  | NN | AN | IM | CO | DC | LC | FP |
;       +----+----+----+----+----+----+----+----+
;
; Flag bits:
;
;       FP - Far previous NT (LSB/MSB not just LSB within previous page)
;       LC - Long code (two byte vs one byte length for native compile)
;       DC - Disjoint code (two byte pointer to xt rather than adjoining header)
;
;       CO - Compile Only
;       IM - Immediate Word
;       NN - Never Native Compile (must always be called by JSR)
;       AN - Always Native Compile (may not be called by JSR)
;
;       The NN and AN flags are intrepreted together like this:
;
;            NN  AN
;           +---+---+
;           | 0 | 0 |  -- : Normal word called by JSR (non-native) or inlined (native)
;           | 1 | 0 |  NN : Word can only be called by JSR (never native)
;           | 0 | 1 |  AN : Word can only be inlined (always native)
;           | 1 | 1 |  ST : Normal word with return stack juggling that
;           +---+---+       must be removed when inlining (R>, R@, >R etc)
;
; Note there is currently one bit unused.
;
; By default, all words can be natively compiled (compiled inline) or
; as a subroutine jump target; the system decides which variant to use based on
; a threshold the user can set. Tali detects users words that can't be inlined,
; typically because they contain non-relocatable `jmp` instructions, and flags
; them as NN.  The user can override the default flags using the `always-native`
; or `never-native` words, for example when using raw assembly.
;
;
; The actual header implementation looks like this:
;
;   nt_last --> +----------------+
;               :                :
;               :                :
;                     ...
;
;   nt_word --> +----------------+
;               |  Status flags  |   See description below
;               |    (1 byte)    |
;               +----------------+
;               |  Name length   |   Max name length 31 (5 bits)
;               |   (1 byte)     |   Top three bits are currently reserved
;               +----------------+
;               |    Last NT     |   FP=0: nt_last = nt_word - offset
;               |  (1|2 bytes)   |   FP=1: nt_last = 2 byte pointer
;               +----------------+
;               | Start of code  |   DC=0: xt_word = nt_end
;               |  (0|2 bytes)   |   DC=1: xt_word = 2 byte pointer
;               +----------------+
;               |   Code size    |   LC=0: One byte code length
;               |  (1|2 bytes)   |   LC=1: Two byte code length
;               +----------------+
;               |  Name string   |   Name is 7-bit lower case ascii
;               :                :
;               :                :   Note string is not zero-terminated
;    nt_end --> +----------------+
;                       ...
;   xt_word --> +----------------+
;               |   Word body    |   When DC=0 the code body adjoins the header
;               :  (65c02 code)  :
;               :                :
;    z_word --> +----------------+
;
;
; We can calculate the actual header length from the flag bits FP, LC and DC
; using simple assembly:
;
;       lda flags       ; start with status flags in the accumulator
;       and #DC+LC+FP   ; mask the header length bits
;       lsr             ; shift FP to carry flag, A = 2*DC + LC
;       adc #4          ; header length is 4 bytes + 2*DC + LC + FP
;
; To simplify header creation, reduce errors and allow for future changes
; we use the #nt_header macro to generate the actual header bytes for each word.
; Usually word NAME is implemented by code between `xt_name` and `z_name`.
; These headers are generated with just `#nt_header name`.  When a word's name
; doesn't match its start and end labels simply pass the name string
; as the optional second argument, like `#nt_header m_star_slash, "m*/"`.
; If the word needs any of the CO, IM, AN, NN flags, pass both the name
; and flags like `#nt_header myword, "myword", IM+CO`.
; The macro automatically chains words together in the dictionary using
; the `last_nt` compiler variable.  When your word list is complete
; you can capture the head of the dictionary (the latest NT header)
; by assigning from it, like `dictionary_start = last_nt`.
; Then start a new wordlist by resetting `last_nt := 0`.


; The #nt_header macro is used to generate the physical header representation
nt_header .macro label, name="", flags=0
        ; It might be nicer to pass the name string instead of the bare label
        ; but it seems hard to generate a symbol from a string in 64tass
        ; see https://sourceforge.net/p/tass64/feature-requests/23/

    ; temp string with explicit name or stringified label
    _s := \name ? \name : str(.\label)

_nt:                    ; remember start of header to update last_nt pointer
    _fp := _nt < last_nt || _nt - last_nt > 256 ? FP : 0
    _sz := z_\label - xt_\label
    _lc := _sz > 255 ? LC : 0
    _dc := _nt_end != xt_\label ? DC : 0

    .byte \flags | _fp | _lc | _dc       ; status flags byte
    .byte len(_s)       ; length of word string, max 31
.if _fp
    .word last_nt       ; previous Dictionary header, 0000 signals start
.else
   .byte <last_nt
.endif
.if _dc
    .word xt_\label     ; start of code block (xt of this word)
.endif
.if _lc
    .word _sz          ; code size up to but not including final RTS
.else
    .byte _sz
.endif
    .text _s            ; word name, always lower case, not zero-terminated
_nt_end = *

last_nt ::= _nt         ; update to this header
.endmacro

; last_nt tracks the previous header, and is reset after each wordlist
last_nt := 0


; FORTH-WORDLIST

; Each wordlist is organized as a linked list with the head of the list stored
; in the `cold_user_table` table called `wordlists_offset`.  The main Forth
; dictionary is linked at `dictionary_start`.  Whenever we search for a word,
; for example via `find-name`, we begin at the head and walk through the linked
; list until we find the word or find a header whose previous NT pointer is 0.
; This means it's more efficient to have frequently used words towards the head
; of the list (later in memory).  In the main dictionary the first word we search
; (last in memory) is DROP and the last word we search (earliest in memory) is BYE.
; Other words are sorted with the more common ones later in memory so they are
; found earlier. Anything to do with output comes later (further up) because
; speed is less important when a human is involved.

; The initial skeleton of this list was automatically generated by a script
; in the tools folder and then sorted by hand.

#nt_header bye
#nt_header cold

.if "ed" in TALI_OPTIONAL_WORDS
#nt_header ed, "ed:", NN                                ; ed6502
.endif

.if "wordlist" in TALI_OPTIONAL_WORDS
    .if "editor" in TALI_OPTIONAL_WORDS
#nt_header editor_wordlist, "editor-wordlist"           ; shares code with ONE
    .endif

    .if "assembler" in TALI_OPTIONAL_WORDS
#nt_header assembler_wordlist, "assembler-wordlist"     ; shares code with TWO
    .endif

#nt_header forth
#nt_header order
#nt_header to_order, ">order"
#nt_header previous
#nt_header also
#nt_header only
#nt_header forth_wordlist, "forth-wordlist"             ; shares code with ZERO
.endif

.if "wordlist" in TALI_OPTIONAL_WORDS
#nt_header root_wordlist, "root-wordlist"
#nt_header get_order, "get-order"
#nt_header set_order, "set-order"
#nt_header get_current, "get-current"
#nt_header set_current, "set-current"
#nt_header search_wordlist, "search-wordlist"
#nt_header wordlist
#nt_header definitions
.endif

.if "block" in TALI_OPTIONAL_WORDS

    .if "ramdrive" in TALI_OPTIONAL_WORDS
#nt_header block_ramdrive_init, "block-ramdrive-init"
    .endif
   .if TALI_ARCH == "c65"
#nt_header block_c65_init, "block-c65-init"
   .endif
   .if "editor" in TALI_OPTIONAL_WORDS
#nt_header list
   .endif

#nt_header thru
#nt_header load
#nt_header flush
#nt_header empty_buffers, "empty-buffers"
#nt_header buffer
#nt_header update
#nt_header block
#nt_header save_buffers, "save-buffers"
#nt_header block_read_vector, "block-read-vector", NN
#nt_header block_read, "block-read", NN
#nt_header block_write_vector, "block-write-vector", NN
#nt_header block_write, "block-write", NN
#nt_header blk, "blk", NN
#nt_header scr, "scr", NN
#nt_header blkbuffer
#nt_header buffblocknum
#nt_header buffstatus
.endif

.if "environment?" in TALI_OPTIONAL_WORDS
#nt_header environment_q, "environment?"
.endif

.if "disassembler" in TALI_OPTIONAL_WORDS
#nt_header disasm
.endif

#nt_header see, "see", NN
#nt_header buffer_colon, "buffer:"
#nt_header useraddr
#nt_header action_of, "action-of", IM
#nt_header is, "is", IM
#nt_header defer_store, "defer!"
#nt_header defer_fetch, "defer@"
#nt_header endcase, "endcase", IM+CO+NN
#nt_header endof, "endof", IM+CO+NN             ; shares code with ELSE
#nt_header of, "of", IM+CO+NN
#nt_header case, "case", IM+CO+NN               ; shares code with ZERO
#nt_header while, "while", IM+CO+NN
#nt_header until, "until", IM+CO+NN
#nt_header repeat, "repeat", IM+CO+NN
#nt_header else, "else", IM+CO+NN
#nt_header then, "then", IM+CO+NN
#nt_header if, "if", IM+CO+NN
#nt_header dot_paren, ".(", IM
#nt_header paren, "(", IM
#nt_header word
#nt_header find
#nt_header search, "search", NN
#nt_header compare
#nt_header dot_s, ".s"
#nt_header dump
#nt_header bell
#nt_header align
#nt_header aligned
#nt_header wordsize
#nt_header words
#nt_header djb2
#nt_header marker, "marker", IM
#nt_header at_xy, "at-xy"
#nt_header page
#nt_header cr
#nt_header havekey
#nt_header input
#nt_header output
#nt_header sign
#nt_header hold
#nt_header number_sign_greater, "#>"
#nt_header number_sign_s, "#s"
#nt_header number_sign, "#"
#nt_header less_number_sign, "<#"
#nt_header to_in, ">in"
#nt_header within
#nt_header hexstore
#nt_header cleave
#nt_header pad
#nt_header cmove
#nt_header cmove_up, "cmove>"
#nt_header move, "move", NN
#nt_header backslash, "\", IM
#nt_header star_slash, "*/"
#nt_header star_slash_mod, "*/mod"
#nt_header mod
#nt_header slash_mod, "/mod"
#nt_header slash, "/"
#nt_header fm_slash_mod, "fm/mod"
#nt_header sm_slash_rem, "sm/rem"
#nt_header um_slash_mod, "um/mod"
#nt_header star, "*"
#nt_header um_star, "um*"
#nt_header m_star, "m*"
#nt_header count
#nt_header decimal
#nt_header hex
#nt_header to_number, ">number"
#nt_header number
#nt_header digit_question, "digit?"
#nt_header base
#nt_header evaluate
#nt_header state
#nt_header again, "again", CO+IM
#nt_header begin, "begin", CO+IM
#nt_header quit
#nt_header recurse, "recurse", CO+IM
#nt_header leave, "leave", CO+IM
#nt_header unloop, "unloop", CO
#nt_header exit, "exit", AN+CO
#nt_header plus_loop, "+loop", CO+IM
#nt_header loop, "loop", CO+IM
#nt_header j, "j", CO
#nt_header i, "i", CO
#nt_header question_do, "?do", CO+IM+NN
#nt_header do, "do", CO+IM+NN
#nt_header abort_quote, 'abort"', CO+IM+NN
#nt_header abort
#nt_header strip_underflow, "strip-underflow", NN
#nt_header nc_limit, "nc-limit", NN
#nt_header allow_native, "allow-native"
#nt_header always_native, "always-native"
#nt_header never_native, "never-native"
#nt_header compile_only, "compile-only"
#nt_header immediate
#nt_header postpone, "postpone", IM+CO
#nt_header s_backslash_quote, 's\"', IM
#nt_header s_quote, 's"', IM+NN
#nt_header dot_quote, '."', CO+IM
#nt_header sliteral, "sliteral", CO+IM
#nt_header literal, "literal", IM+CO
#nt_header right_bracket, "]", IM
#nt_header left_bracket, "[", IM+CO
#nt_header compile_comma, "compile,", NN
#nt_header colon_noname, ":noname"
#nt_header semicolon, ";", CO+IM
#nt_header colon, ":"
#nt_header source_id, "source-id"
#nt_header source
#nt_header execute_parsing, "execute-parsing"
#nt_header parse
#nt_header parse_name, "parse-name", NN
#nt_header latestnt
#nt_header latestxt
#nt_header defer
#nt_header to_body, ">body"
#nt_header name_to_string, "name>string"
#nt_header int_to_name, "int>name"
#nt_header name_to_int, "name>int"
#nt_header bracket_tick, "[']", CO+IM
#nt_header tick, "'"
#nt_header find_name, "find-name"
#nt_header fill
#nt_header blank
#nt_header erase
#nt_header d_plus, "d+"
#nt_header d_minus, "d-"
#nt_header d_to_s, "d>s"
#nt_header s_to_d, "s>d"
#nt_header to, "to", NN+IM
#nt_header value                        ; same code as CONSTANT
#nt_header constant
#nt_header variable
#nt_header does, "does>", CO+IM
#nt_header create
#nt_header allot
#nt_header keyq, "key?"
#nt_header key
#nt_header depth
#nt_header unused
#nt_header r_to_input, "r>input", NN
#nt_header input_to_r, "input>r", NN
#nt_header accept, "accept", NN
#nt_header refill
#nt_header slash_string, "/string"
#nt_header minus_leading, "-leading"
#nt_header minus_trailing, "-trailing"
#nt_header bl
#nt_header spaces
#nt_header bounds
#nt_header c_comma, "c,"
#nt_header dnegate
#nt_header negate
#nt_header invert
#nt_header two_to_r, "2>r", CO+ST       ; native skips stack juggling
#nt_header two_r_from, "2r>", CO+ST     ; native skips stack juggling
#nt_header two_r_fetch, "2r@", CO+ST    ; native skips stack juggling
#nt_header two_literal, "2literal", IM
#nt_header two_constant, "2constant"
#nt_header two_variable, "2variable"
#nt_header two_fetch, "2@"
#nt_header two_store, "2!"
#nt_header two_over, "2over"
#nt_header two_swap, "2swap"
#nt_header two_drop, "2drop"
#nt_header max
#nt_header min
#nt_header zero_less, "0<"
#nt_header zero_greater, "0>"
#nt_header zero_unequal, "0<>"
#nt_header zero_equal, "0="
#nt_header greater_than, ">"
#nt_header u_greater_than, "u>"
#nt_header u_less_than, "u<"
#nt_header less_than, "<"
#nt_header not_equals, "<>"
#nt_header equal, "="
#nt_header here
#nt_header cell_plus, "cell+"
#nt_header cells                ; same as 2*
#nt_header chars                ; no-op (underflow check) during compile
#nt_header char_plus, "char+"
#nt_header bracket_char, "[char]", CO+IM
#nt_header char
#nt_header pick                 ; underflow check is complicated, leave off here
#nt_header lshift
#nt_header rshift
#nt_header xor
#nt_header or
#nt_header and
#nt_header dabs
#nt_header abs
#nt_header two_slash, "2/"
#nt_header two_star, "2*"
#nt_header one_plus, "1+"
#nt_header one_minus, "1-"
#nt_header minus, "-"
#nt_header plus, "+"
#nt_header question_dup, "?dup"
#nt_header two_dup, "2dup"
#nt_header space
#nt_header true
#nt_header false
#nt_header question, "?"
#nt_header ud_dot_r, "ud.r"
#nt_header ud_dot, "ud."
#nt_header m_star_slash, "m*/"
#nt_header d_dot_r, "d.r"
#nt_header d_dot, "d."
#nt_header dot_r, ".r"
#nt_header u_dot_r, "u.r"
#nt_header u_dot, "u."
#nt_header dot, "."
#nt_header type
#nt_header emit, "emit", NN
#nt_header execute
#nt_header two, "2"
#nt_header one, "1"
#nt_header zero, "0"
#nt_header plus_store, "+!"
#nt_header c_store, "c!"
#nt_header c_fetch, "c@"
#nt_header comma, ","
#nt_header tuck
#nt_header not_rot, "-rot"
#nt_header rot
#nt_header nip
#nt_header r_fetch, "r@", CO+ST ; native skips stack juggling
#nt_header r_from, "r>", CO+ST  ; native skips stack juggling
#nt_header to_r, ">r", CO+ST    ; native skips stack juggling
#nt_header over
#nt_header fetch, "@"
#nt_header store, "!"
#nt_header swap
#nt_header dup
#nt_header drop                 ; DROP is always the first native word in the Dictionary

dictionary_start = last_nt
last_nt := 0

; END of FORTH-WORDLIST


; ROOT-WORDLIST
        ; This is a short wordlist that has just the words needed to
        ; set the wordlists. These words are also included in the
        ; FORTH-WORDLIST as well.

.if "wordlist" in TALI_OPTIONAL_WORDS
#nt_header set_order, "set-order"
#nt_header forth
#nt_header forth_wordlist, "forth-wordlist"     ; shares code with ZERO
#nt_header words
.endif

root_dictionary_start = last_nt
last_nt := 0

; END of ROOT-WORDLIST


; EDITOR-WORDLIST

.if "editor" in TALI_OPTIONAL_WORDS && "block" in TALI_OPTIONAL_WORDS
#nt_header editor_o, "o"
#nt_header editor_line, "line"
#nt_header editor_l, "l"
#nt_header editor_el, "el"
#nt_header editor_erase_screen, "erase-screen"
#nt_header editor_enter_screen, "enter-screen"
.endif

editor_dictionary_start = last_nt
last_nt := 0

; END of EDITOR-WORDLIST


; ASSEMBLER-WORDLIST

; Labels for the opcodes have the format "nt_asm_<OPC>" where a futher
; underscore replaces any dot present in the SAN mnemonic. The hash sign for
; immediate addressing is replaced by an "h" (for example, the label code for
; "lda.#" is "xt_adm_lda_h"). All opcodes are immediate.

.if "assembler" in TALI_OPTIONAL_WORDS
#nt_header asm_adc_h, "adc.#", IM+NN
#nt_header asm_adc_x, "adc.x", IM+NN
#nt_header asm_adc_y, "adc.y", IM+NN
#nt_header asm_adc_z, "adc.z", IM+NN
#nt_header asm_adc_zi, "adc.zi", IM+NN
#nt_header asm_adc_ziy, "adc.ziy", IM+NN
#nt_header asm_adc_zx, "adc.zx", IM+NN
#nt_header asm_adc_zxi, "adc.zxi", IM+NN
#nt_header asm_and, "and.", IM+NN               ; not "and" because of conflicts with Forth word
#nt_header asm_and_h, "and.#", IM+NN
#nt_header asm_and_x, "and.x", IM+NN
#nt_header asm_and_y, "and.y", IM+NN
#nt_header asm_and_z, "and.z", IM+NN
#nt_header asm_and_zi, "and.zi", IM+NN
#nt_header asm_and_ziy, "and.ziy", IM+NN
#nt_header asm_and_zx, "and.zx", IM+NN
#nt_header asm_and_zxi, "and.zxi", IM+NN
#nt_header asm_asl, "asl", IM+NN
#nt_header asm_asl_a, "asl.a", IM+NN
#nt_header asm_asl_x, "asl.x", IM+NN
#nt_header asm_asl_z, "asl.z", IM+NN
#nt_header asm_asl_zx, "asl.zx", IM+NN
#nt_header asm_bcc, "bcc", IM+NN
#nt_header asm_bcs, "bcs", IM+NN
#nt_header asm_beq, "beq", IM+NN
#nt_header asm_bit, "bit", IM+NN
#nt_header asm_bit_h, "bit.#", IM+NN
#nt_header asm_bit_x, "bit.x", IM+NN
#nt_header asm_bit_z, "bit.z", IM+NN
#nt_header asm_bit_zx, "bit.zx", IM+NN
#nt_header asm_bmi, "bmi", IM+NN
#nt_header asm_bne, "bne", IM+NN
#nt_header asm_bpl, "bpl", IM+NN
#nt_header asm_bra, "bra", IM+NN
#nt_header asm_brk, "brk", IM+NN
#nt_header asm_bvc, "bvc", IM+NN
#nt_header asm_bvs, "bvs", IM+NN
#nt_header asm_clc, "clc", IM+NN
#nt_header asm_cld, "cld", IM+NN
#nt_header asm_cli, "cli", IM+NN
#nt_header asm_clv, "clv", IM+NN
#nt_header asm_cmp, "cmp", IM+NN
#nt_header asm_cmp_h, "cmp.#", IM+NN
#nt_header asm_cmp_x, "cmp.x", IM+NN
#nt_header asm_cmp_y, "cmp.y", IM+NN
#nt_header asm_cmp_z, "cmp.z", IM+NN
#nt_header asm_cmp_zi, "cmp.zi", IM+NN
#nt_header asm_cmp_ziy, "cmp.ziy", IM+NN
#nt_header asm_cmp_zx, "cmp.zx", IM+NN
#nt_header asm_cmp_zxi, "cmp.zxi", IM+NN
#nt_header asm_cpx, "cpx", IM+NN
#nt_header asm_cpx_h, "cpx.#", IM+NN
#nt_header asm_cpx_z, "cpx.z", IM+NN
#nt_header asm_cpy, "cpy", IM+NN
#nt_header asm_cpy_h, "cpy.#", IM+NN
#nt_header asm_cpy_z, "cpy.z", IM+NN
#nt_header asm_dec, "dec", IM+NN
#nt_header asm_dec_a, "dec.a", IM+NN
#nt_header asm_dec_x, "dec.x", IM+NN
#nt_header asm_dec_z, "dec.z", IM+NN
#nt_header asm_dec_zx, "dec.zx", IM+NN
#nt_header asm_dex, "dex", IM+NN
#nt_header asm_dey, "dey", IM+NN
#nt_header asm_eor, "eor", IM+NN
#nt_header asm_eor_h, "eor.#", IM+NN
#nt_header asm_eor_x, "eor.x", IM+NN
#nt_header asm_eor_y, "eor.y", IM+NN
#nt_header asm_eor_z, "eor.z", IM+NN
#nt_header asm_eor_zi, "eor.zi", IM+NN
#nt_header asm_eor_ziy, "eor.ziy", IM+NN
#nt_header asm_eor_zx, "eor.zx", IM+NN
#nt_header asm_eor_zxi, "eor.zxi", IM+NN
#nt_header asm_inc, "inc", IM+NN
#nt_header asm_inc_a, "inc.a", IM+NN
#nt_header asm_inc_x, "inc.x", IM+NN
#nt_header asm_inc_z, "inc.z", IM+NN
#nt_header asm_inc_zx, "inc.zx", IM+NN
#nt_header asm_inx, "inx", IM+NN
#nt_header asm_iny, "iny", IM+NN
#nt_header asm_jmp, "jmp", IM+NN
#nt_header asm_jmp_i, "jmp.i", IM+NN
#nt_header asm_jmp_xi, "jmp.xi", IM+NN
#nt_header asm_jsr, "jsr", IM+NN
#nt_header asm_lda, "lda", IM+NN
#nt_header asm_lda_h, "lda.#", IM+NN
#nt_header asm_lda_x, "lda.x", IM+NN
#nt_header asm_lda_y, "lda.y", IM+NN
#nt_header asm_lda_z, "lda.z", IM+NN
#nt_header asm_lda_zi, "lda.zi", IM+NN
#nt_header asm_lda_ziy, "lda.ziy", IM+NN
#nt_header asm_lda_zx, "lda.zx", IM+NN
#nt_header asm_lda_zxi, "lda.zxi", IM+NN
#nt_header asm_ldx, "ldx", IM+NN
#nt_header asm_ldx_h, "ldx.#", IM+NN
#nt_header asm_ldx_y, "ldx.y", IM+NN
#nt_header asm_ldx_z, "ldx.z", IM+NN
#nt_header asm_ldx_zy, "ldx.zy", IM+NN
#nt_header asm_ldy, "ldy", IM+NN
#nt_header asm_ldy_h, "ldy.#", IM+NN
#nt_header asm_ldy_x, "ldy.x", IM+NN
#nt_header asm_ldy_z, "ldy.z", IM+NN
#nt_header asm_ldy_zx, "ldy.zx", IM+NN
#nt_header asm_lsr, "lsr", IM+NN
#nt_header asm_lsr_a, "lsr.a", IM+NN
#nt_header asm_lsr_x, "lsr.x", IM+NN
#nt_header asm_lsr_z, "lsr.z", IM+NN
#nt_header asm_lsr_zx, "lsr.zx", IM+NN
#nt_header asm_nop, "nop", IM+NN
#nt_header asm_ora, "ora", IM+NN
#nt_header asm_ora_h, "ora.#", IM+NN
#nt_header asm_ora_x, "ora.x", IM+NN
#nt_header asm_ora_y, "ora.y", IM+NN
#nt_header asm_ora_z, "ora.z", IM+NN
#nt_header asm_ora_zi, "ora.zi", IM+NN
#nt_header asm_ora_ziy, "ora.ziy", IM+NN
#nt_header asm_ora_zx, "ora.zx", IM+NN
#nt_header asm_ora_zxi, "ora.zxi", IM+NN
#nt_header asm_pha, "pha", IM+NN
#nt_header asm_php, "php", IM+NN
#nt_header asm_phx, "phx", IM+NN
#nt_header asm_phy, "phy", IM+NN
#nt_header asm_pla, "pla", IM+NN
#nt_header asm_plp, "plp", IM+NN
#nt_header asm_plx, "plx", IM+NN
#nt_header asm_ply, "ply", IM+NN
#nt_header asm_rol, "rol", IM+NN
#nt_header asm_rol_a, "rol.a", IM+NN
#nt_header asm_rol_x, "rol.x", IM+NN
#nt_header asm_rol_z, "rol.z", IM+NN
#nt_header asm_rol_zx, "rol.zx", IM+NN
#nt_header asm_ror, "ror", IM+NN
#nt_header asm_ror_a, "ror.a", IM+NN
#nt_header asm_ror_x, "ror.x", IM+NN
#nt_header asm_ror_z, "ror.z", IM+NN
#nt_header asm_ror_zx, "ror.zx", IM+NN
#nt_header asm_rti, "rti", IM+NN
#nt_header asm_rts, "rts", IM+NN
#nt_header asm_sbc, "sbc", IM+NN
#nt_header asm_sbc_h, "sbc.#", IM+NN
#nt_header asm_sbc_x, "sbc.x", IM+NN
#nt_header asm_sbc_y, "sbc.y", IM+NN
#nt_header asm_sbc_z, "sbc.z", IM+NN
#nt_header asm_sbc_zi, "sbc.zi", IM+NN
#nt_header asm_sbc_ziy, "sbc.ziy", IM+NN
#nt_header asm_sbc_zx, "sbc.zx", IM+NN
#nt_header asm_sbc_zxi, "sbc.zxi", IM+NN
#nt_header asm_sec, "sec", IM+NN
#nt_header asm_sed, "sed", IM+NN
#nt_header asm_sei, "sei", IM+NN
#nt_header asm_sta, "sta", IM+NN
#nt_header asm_sta_x, "sta.x", IM+NN
#nt_header asm_sta_y, "sta.y", IM+NN
#nt_header asm_sta_z, "sta.z", IM+NN
#nt_header asm_sta_zi, "sta.zi", IM+NN
#nt_header asm_sta_ziy, "sta.ziy", IM+NN
#nt_header asm_sta_zx, "sta.zx", IM+NN
#nt_header asm_sta_zxi, "sta.zxi", IM+NN
#nt_header asm_stx, "stx", IM+NN
#nt_header asm_stx_z, "stx.z", IM+NN
#nt_header asm_stx_zy, "stx.zy", IM+NN
#nt_header asm_sty, "sty", IM+NN
#nt_header asm_sty_z, "sty.z", IM+NN
#nt_header asm_sty_zx, "sty.zx", IM+NN
#nt_header asm_stz, "stz", IM+NN
#nt_header asm_stz_x, "stz.x", IM+NN
#nt_header asm_stz_z, "stz.z", IM+NN
#nt_header asm_stz_zx, "stz.zx", IM+NN
#nt_header asm_tax, "tax", IM+NN
#nt_header asm_tay, "tay", IM+NN
#nt_header asm_trb, "trb", IM+NN
#nt_header asm_trb_z, "trb.z", IM+NN
#nt_header asm_tsb, "tsb", IM+NN
#nt_header asm_tsb_z, "tsb.z", IM+NN
#nt_header asm_tsx, "tsx", IM+NN
#nt_header asm_txa, "txa", IM+NN
#nt_header asm_txs, "txs", IM+NN
#nt_header asm_tya, "tya", IM+NN

; Assembler pseudo-instructions, directives and macros

#nt_header asm_arrow, "-->", IM         ; uses same code as HERE, but immediate
#nt_header asm_back_jump, "<j", IM      ; syntactic sugar, does nothing
#nt_header asm_back_branch, "<b", IM
#nt_header asm_push_a, "push-a", IM+NN
.endif

assembler_dictionary_start = last_nt
last_nt := 0

; END of ASSEMBLER-WORDLIST

; END
