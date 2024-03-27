; Definitions for Tali Forth 2
; Scot W. Stevenson <scot.stevenson@gmail.com>
; First version: 01. Apr 2016 (Liara Forth)
; This version: 29. Jan 2020

; This file is included by taliforth.asm. These are the general
; definitions; platform-specific definitions such as the
; memory map are kept in the platform folder.

; TaliForth reserves the first part of zero page ($0 - zpage_end) which is
; configured by zpage_end in platform/platform-*.asm and normally 128 bytes.
; The rest of zero page is free for kernel/external use (zpage_end+1 - $ff)
; TaliForth usage is as follows:
;   zero page variables: 30 words = 60 bytes ($0000-$0038)
;   Forth Data Stack: 128 - 60 - 8 = 60 bytes or 30 words
;   Data Stack floodplain: 8 bytes after stack (to avoid catastrophic underflow)

; ZERO PAGE ADDRESSES/VARIABLES

; These are kept at the top of Zero Page, with the most important variables at
; the top because the Data Stack grows towards this area from dsp0: If there is
; an overflow, the lower, less important variables will be clobbered first,
; giving the system a chance to recover. In other words, they are part of the
; overflow floodplain.

; This table defines all of the zero page variables along with their initial
; values except the uninitialized temporaries at the end.  This table
; is relocated to zero page by COLD.

dsp0      = zpage_end-7    ; initial Data Stack Pointer
turnkey   = zpage_end-1    ; word to resume in pre-compiled image

cold_zp_table:
        .logical user0              ; make labels refer to relocated address

cp:         .word cp0+256+1024      ; Compiler Pointer
                                    ; moved to make room for user vars and block buffer
dp:         .word dictionary_start  ; Dictionary Pointer
workword:   .word 0                 ; nt (not xt!) of word being compiled, except in
                                    ; a :NONAME declared word (see status)

; The four variables insrc, cib, ciblen, and toin must stay together in this
; sequence for the words INPUT>R and R>INPUT to work correctly.

insrc:      .word 0                 ; input source for SOURCE-ID (0 for keyboard)
cib:        .word buffer0           ; address of current input buffer
ciblen:     .word 0                 ; length of current input buffer
toin:       .word 0                 ; pointer to CIB (>IN in Forth)

ip:         .word 0                 ; Instruction Pointer (current xt)
output:     .word kernel_putc       ; vector for EMIT
input:      .word kernel_getc       ; vector for KEY
havekey:    .word 0                 ; vector for KEY?
state:      .word 0                 ; STATE: -1 compile, 0 interpret
base:       .word 10                ; number radix, default decimal
nc_limit:   .word 20                ; byte limit for Native Compile size
uf_strip:   .word 0                 ; flag to strip underflow detection code (0 off)
up:         .word cp0               ; Forth user vars at start of available RAM
status:     .word 0                 ; internal status used by : :NONAME ; ACCEPT
        ; Bit 7 = Redefined word message postpone
        ;         When set before calling CREATE, it will
        ;         not print the "redefined xxxx" message if
        ;         the word exists. Instead, this bit will
        ;         be reused and after CREATE has run, it will
        ;         be set if the word was redefined and 0 if
        ;         not. This bit should be 0 when not in use.
        ; Bit 6 = 1 for normal ":" definitions
        ;         WORKWORD contains nt of word being compiled
        ;       = 0 for :NONAME definitions
        ;         WORKWORD contains xt of word being compiled
        ; Bit 5 = 1 for NUMBER returning a double word
        ;       = 0 for NUMBER returning a single word
        ; Bit 3 = 1 makes CTRL-n recall current history
        ;       = 0 CTRL-n recalls previous history
        ; Bit 2 = Current history buffer msb
        ; Bit 1 = Current history buffer (0-7, wraps)
        ; Bit 0 = Current history buffer lsb
        ;
        ; status+1 is used by ACCEPT to hold history lengths.

; The remaining ZP variables are uninitialized temporaries.

    .virtual
tmp1:       .word ?         ; temporary storage [address hard-coded in tests/ed.fs]
tmp2:       .word ?         ; temporary storage
tmp3:       .word ?         ; temporary storage (especially for print)
tmpdsp:     .word ?         ; temporary DSP (X) storage (two bytes)
tmptos:     .word ?         ; temporary TOS storage
editor1:    .word ?         ; temporary for editors
editor2:    .word ?         ; temporary for editors
editor3:    .word ?         ; temporary for editors
tohold:     .word ?         ; pointer for formatted output
scratch:    .word ?,?,?,?   ; 8 byte scratchpad (see UM/MOD)
    .endvirtual

    .endlogical
cold_zp_table_end:


; RAM System Variables

; This table defines the initial values for forth user variables above zero page,
; which are relocated to 'up' (defaulting to cp0) by COLD.

cold_user_table:
    .logical 0          ; make labels here offsets that we can add to 'up'

; Block variables
blk_offset:             .word 0         ; BLK
scr_offset:             .word 0         ; SCR

; Wordlists

max_wordlists = 12    ; Maximum number of wordlists supported (4 built-in, 8 user wordlists)

current_offset:         .byte 0         ; CURRENT = FORTH-WORDLIST (compilation wordlist)
num_wordlists_offset:   .byte 4         ; #WORDLISTS (FORTH EDITOR ASSEMBLER ROOT)

wordlists_offset:
    .word dictionary_start              ; FORTH-WORDLIST
    .word editor_dictionary_start       ; EDITOR-WORDLIST
    .word assembler_dictionary_start    ; ASSEMBLER-WORDLIST
    .word root_dictionary_start         ; ROOT-WORDLIST
    .word 0,0,0,0,0,0,0,0               ; Space for 8 User wordlists

num_order_offset:       .byte 1         ; #ORDER (Number of wordlists in search order)
search_order_offset:
    .byte 0,0,0,0,0,0,0,0,0             ; SEARCH-ORDER (9 bytes to keep offsets even)

; Buffer variables

blkbuffer_offset:       .word cp0+256   ; Address of buffer (right after USER vars)
buffblocknum_offset:    .word 0         ; Block number current in buffer
buffstatus_offset:      .word 0         ; Buffer status (bit 0 = used, bit 1 = dirty) (not in use)

; Block I/O vectors

blockread_offset:       .word xt_block_word_error   ; Vector to block reading routine
blockwrite_offset:      .word xt_block_word_error   ; Vector to block writing routine
    .endlogical
cold_user_table_end:


; ASCII CHARACTERS
AscCC   = $03  ; break (CTRL-c)
AscBELL = $07  ; bell sound
AscBS   = $08  ; backspace
AscLF   = $0a  ; line feed
AscCR   = $0d  ; carriage return
AscESC  = $1b  ; escape
AscSP   = $20  ; space
AscDEL  = $7f  ; delete (CTRL-h)
AscCP   = $10  ; CTRL-p (used to recall previous input history)
AscCN   = $0e  ; CTRL-n (used to recall next input history)

; DICTIONARY FLAGS
; The first two bits are currently unused
CO = 1  ; Compile Only
AN = 2  ; Always Native Compile
IM = 4  ; Immediate Word
NN = 8  ; Never Native Compile
UF = 16 ; Includes Underflow Check (RESERVED)
HC = 32 ; Word has Code Field Area (CFA)


; VARIOUS
MAX_LINE_LENGTH  = 79      ; assumes 80 character lines

; END
