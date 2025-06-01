; Low-level Forth word routines
; Tali Forth 2 for the 65c02
; Scot W. Stevenson <scot.stevenson@gmail.com>
; Sam Colwell
; Patrick Surry
; First version: 19. Jan 2014
; This version: 21. Apr 2024

; The words are grouped similarly to https://forth-standard.org/standard/words
; Each group of words is ordered alphabetically by the names of their XT symbol,
; not their strings (so "!" is sorted as "STORE"). However, we start off with COLD,
; ABORT, and QUIT as the natural start sequence. Other word groups are included below.
; Each word has two special status lines that begins with "; ## " which
; allows auto-generation of the WORDLIST.md file and other entries in the docs folder.
; Status entries are:

;       TBA --> fragment --> coded --> tested --> auto

; "Auto" means that the word is automatically tested by the test suite (good),
; "tested" means that it was tested by hand in some way (okay), "coded" means
; it hasn't been tested at all (bad). See the test suite for more details.


; ## COLD ( -- ) "Reset the Forth system"
; ## "cold"  tested  Tali Forth
;       """Reset the Forth system. Does not restart the kernel,
;       use the 65c02 reset for that. Flows into ABORT.
;       """
xt_cold:
w_cold:
                cld

                ; Initialize 65c02 stack (Return Stack)
                ldx #rsp0
                txs

                ; Clear Data Stack. This is repeated in ABORT, but this way we
                ; can load high-level words with EVALUATE
                ldx #dsp0

                ; Set the OUTPUT vector to the default kernel_putc
                ; We do this really early so we can print error messages
                ; during debugging
                lda #<kernel_putc
                sta output
                lda #>kernel_putc
                sta output+1

                ; Load all of the important zero page variables from ROM
                ldy #cold_zp_table_end-cold_zp_table-1

_load_zp_loop:
                ; This loop loads them back to front.
                lda cold_zp_table,y
                sta user0,y
                dey
                bpl _load_zp_loop           ; <128 bytes so safe to loop until y<0

                ; Initialize the user variables.
                ldy #cold_user_table_end-cold_user_table-1
_load_user_vars_loop:
                ; Like the zero page variables, these are initialized
                ; back to front.
                lda cold_user_table,y
                sta (up),y
                dey
                bpl _load_user_vars_loop    ; again we have <128 bytes so bpl is safe

                jsr w_cr

                ; Define high-level words in forth_words.asc via EVALUATE,
                ; followed by any user-defined words from user_words.asc.
                ; These are stored sequentially in ROM so we can evaluate them together.
                ; If you have neither, this section can be commented out.
                dex
                dex
                dex
                dex

                ; start address goes NOS
                lda #<forth_words_start
                sta 2,x
                lda #>forth_words_start
                sta 3,x

                ; length goes TOS; let the assembler do the math
                lda #<(user_words_end-forth_words_start)
                sta 0,x
                lda #>(user_words_end-forth_words_start)
                sta 1,x

                jsr w_evaluate

.if TALI_OPTION_HISTORY
                ; Initialize all of the history buffers by putting a zero in
                ; each length byte.
                stz hist_buff
                stz hist_buff+$80
                stz hist_buff+$100
                stz hist_buff+$180
                stz hist_buff+$200
                stz hist_buff+$280
                stz hist_buff+$300
                stz hist_buff+$380
.endif
                ; fall through to ABORT


; ## ABORT ( -- ) "Reset the Data Stack and restart the CLI"
; ## "abort"  tested  ANS core
        ; """https://forth-standard.org/standard/core/ABORT
        ; Clear Data Stack and continue into QUIT. We can jump here via
        ; subroutine if we want to because we are going to reset the 65c02's
        ; stack pointer (the Return Stack) anyway during QUIT. Note we don't
        ; actually delete the stuff on the Data Stack.
        ; """
xt_abort:
w_abort:
                ldx #dsp0

                ; fall through to QUIT


; ## QUIT ( -- ) "Reset the input and get new input"
; ## "quit"  tested  ANS core
        ; """https://forth-standard.org/standard/core/QUIT
        ; Rest the input and start command loop
        ; """
xt_quit:
w_quit:
                ; Clear the Return Stack. This is a little screwed up
                ; because the 65c02 can only set the Return Stack via X,
                ; which is our Data Stack pointer. The ANS specification
                ; demands, however, that ABORT reset the Data Stack pointer
                txa             ; Save the DSP that we just defined
                ldx #rsp0
                txs
                tax             ; Restore the DSP. Dude, seriously.

                ; make sure instruction pointer is empty
                stz ip
                stz ip+1

                ; SOURCE-ID is zero (keyboard input)
                stz insrc
                stz insrc+1

                ; BLK is zero
                lda #0
                ldy #blk_offset
                sta (up),y
                iny
                sta (up),y

                ; initialize loopctrl to indicate no active loop
                ; see definitions.asm
                lda #(256-4)
                sta loopctrl

                ; STATE is zero (interpret, not compile)
                stz state
                stz state+1
_get_line:
                lda #<buffer0   ; input buffer, this is paranoid
                sta cib
                lda #>buffer0
                sta cib+1

                ; Size of current input buffer (CIB) is zero
                stz ciblen
                stz ciblen+1

                ; Accept a line from the current import source. This is how
                ; modern Forths do it.
                jsr w_refill           ; ( -- f )

                ; Test flag: LSB of TOS
                lda 0,x
                bne _success

                ; If REFILL returned a FALSE flag, something went wrong and we
                ; need to print an error message and reset the machine. We
                ; don't need to save TOS because we're going to clobber it
                ; anyway when we go back to ABORT.
                lda #err_refill
                jmp error

_success:
                ; Assume we have successfully accepted a string of input from
                ; a source, with address cib and length of input in ciblen. We
                ; arrive here still with the TRUE flag from REFILL as TOS
                inx                     ; drop
                inx

                ; Main compile/execute routine
                jsr interpret

                ; Test for Data Stack underflow. Tali Forth does not check for
                ; overflow because it is so rare
                cpx #dsp0
                beq _stack_ok
                bcc _stack_ok           ; DSP must always be smaller than DSP0

                jmp underflow_error

_stack_ok:
                ; Display system prompt if all went well. If we're interpreting,
                ; this is " ok", if we're compiling, it's " compiled". Note
                ; space at beginning of the string.
                lda state
                beq _print

                lda #1                  ; number for "compile" string
_print:
                jsr print_string_no_lf
                jsr w_cr

                ; Awesome line, everybody! Now get the next one.
                bra _get_line

z_cold:
z_abort:
z_quit:         ; no RTS required


interpret:
        ; """Core routine for the interpreter called by EVALUATE and QUIT.
        ; Process one line only. Assumes that the address of name is in
        ; cib and the length of the whole input line string is in ciblen
        ; """
                ; Normally we would use PARSE here with the SPACE character as
                ; a parameter (PARSE replaces WORD in modern Forths). However,
                ; Gforth's PARSE-NAME makes more sense as it uses spaces as
                ; delimiters per default and skips any leading spaces, which
                ; PARSE doesn't
_loop:
                jsr w_parse_name       ; ( "string" -- addr u )

                ; If PARSE-NAME returns 0 (empty line), no characters were left
                ; in the line and we need to go get a new line
                lda 0,x
                ora 1,x
                beq _line_done

                ; Go to FIND-NAME to see if this is a word we know. We have to
                ; make a copy of the address in case it isn't a word we know and
                ; we have to go see if it is a number
                jsr w_two_dup          ; ( addr u -- addr u addr u )
                jsr w_find_name        ; ( addr u addr u -- addr u nt|0 )

                ; A zero signals that we didn't find a word in the Dictionary
                lda 0,x
                ora 1,x
                bne _got_name_token

                ; We didn't get any nt we know of, so let's see if this is
                ; a number.
                inx                     ; ( addr u 0 -- addr u )
                inx

                ; If the number conversion doesn't work, NUMBER will do the
                ; complaining for us
                jsr w_number           ; ( addr u -- u|d )

                ; Otherwise, if we're interpreting, we're done
                lda state
                beq _loop

                ; We're compiling, so there is a bit more work.  Check
                ; status bit 5 to see if it's a single or double-cell
                ; number.
                lda #%00100000
                bit status
                bne _double_number

                jsr w_literal
                ; That was so much fun, let's do it again!
                bra _loop

_double_number:
                ; It's a double cell number.
                jsr w_two_literal
                bra _loop

_got_name_token:
                ; We have a known word's nt TOS and need to calculate its xt

                ; We arrive here with ( addr u nt ), so we NIP twice
                lda 0,x
                sta 4,x
                lda 1,x
                sta 5,x

                inx
                inx
                inx
                inx                     ; ( nt )

                ; Whether interpreting or compiling we'll need to check the
                ; status byte at nt so let's save it now
                lda (0,x)
                pha

                ; See if we are in interpret or compile mode, 0 is interpret
                lda state
                lsr                     ; C=1 for compile, 0 for interpret
                pla                     ; A=flags
                bcs _compile

                ; We are interpreting, so EXECUTE the xt that is TOS. First,
                ; though, see if this isn't a compile-only word, which would be
                ; illegal.
                and #CO                 ; mask everything but Compile Only bit
                bne _compileonly

_interpret:
                ; We JSR to EXECUTE instead of calling the xt directly because
                ; the RTS of the word we're executing will bring us back here,
                ; skipping EXECUTE completely during RTS. If we were to execute
                ; xt directly, we have to fool around with the Return Stack
                ; instead, which is actually slightly slower
                jsr w_name_to_int      ; ( nt - xt )
                jsr w_execute

                ; That's quite enough for this word, let's get the next one
                bra _loop

_compileonly:
                lda #err_compileonly
                jmp error

_compile:
                ; We're compiling! However, we need to see if this is an
                ; IMMEDIATE word, which would mean we execute it right now even
                ; during compilation mode. Fortunately, we saved the header flags
                ; so life is easier.
                and #IM                 ; Mask all but IM bit
                bne _interpret          ; IMMEDIATE word, execute right now

                ; Compile the word into the Dictionary using nt entry for COMPILE,
                jsr compile_nt_comma
                bra _loop

_line_done:
                ; drop stuff from PARSE_NAME
                inx
                inx
                inx
                inx

                rts


.include "core.asm"
.include "core_juggle.asm"
.include "core_mem.asm"
.include "core_math.asm"
.include "core_flow.asm"
.include "core_input.asm"
.include "core_output.asm"
.include "core_env.asm"
.include "compile.asm"
.include "tools.asm"
.include "tali.asm"
.include "double.asm"
.include "string.asm"
.if "assembler" in TALI_OPTIONAL_WORDS || "disassembler" in TALI_OPTIONAL_WORDS
    .include "assembler.asm"
.endif
.if "disassembler" in TALI_OPTIONAL_WORDS
    .include "disasm.asm"
.endif
.if "ed" in TALI_OPTIONAL_WORDS
    .include "ed.asm"        ; Line-based editor ed6502
.endif
.if "block" in TALI_OPTIONAL_WORDS
    .include "block.asm"
    .if "editor" in TALI_OPTIONAL_WORDS
        .include "editor.asm"
    .endif
.endif
.if "wordlist" in TALI_OPTIONAL_WORDS
    .include "wordlist.asm"
.endif
