; Tali Forth 2 for the 65c02
; Scot W. Stevenson <scot.stevenson@gmail.com>
; Sam Colwell
; Patrick Surry
; First version: 19. Jan 2014 (Tali Forth 1)
; This version: 21. Apr 2024 (Version 1.1)

; This is the main file for Tali Forth 2


; These assignments are "weak" and will only assign if the label
; does not have anything assigned to it.  The user can override these
; defaults by assigning values in their platform file before
; including this file.

; Assemble all words unless overridden in the platform file.
TALI_OPTIONAL_WORDS :?= [ "ed", "editor", "ramdrive", "block", "environment?", "assembler", "wordlist" ]

; Default line ending is line feed.
TALI_OPTION_CR_EOL :?= [ "lf" ]

; Default to verbose strings
TALI_OPTION_TERSE :?= 0

; Default to ctrl-n/p accept history
TALI_OPTION_HISTORY :?= 1

; Optional hardware/simulator architecture name for customization
TALI_ARCH :?= ""

; Label used to calculate UNUSED based on the hardware configuration in platform/
code0:

; Entry point for Tali Forth after kernel hardware setup
forth:

.include "words/all.asm"           ; Native Forth words. Starts with COLD
.include "definitions.asm"      ; Top-level definitions, memory map
                                ; included here to put relocatable tables after native words

; High-level Forth words, see forth_code/README.md
forth_words_start:
.if ! TALI_OPTION_TERSE         ; omit startup strings if terse
.binary "forth_words.asc"
.endif
forth_words_end:

; User-defined Forth words, see forth_code/README.md
user_words_start:
.binary "user_words.asc"
user_words_end:

.include "words/headers.asm"          ; Headers of native words
.include "strings.asm"          ; Strings, including error messages


; =====================================================================
; LOW LEVEL HELPER FUNCTIONS

; Push the accumulator to TOS
; This only saves a byte but improves readability
; This routine is also used as a template by the assembler "push-a" word
push_a_tos:  ; ( -- A )
                dex
                dex
                sta 0,x
                stz 1,x
z_push_a_tos:
                rts

push_upvar_tos:
        ; """Write addr of user page variable with offset A to TOS"""
                dex
                dex
                clc
                adc up
                sta 0,x
                lda up+1
                bcc +
                ina
+
                sta 1,x
                rts

byte_to_ascii:
        ; """Convert byte in A to two ASCII hex digits and EMIT them"""
                pha
                lsr             ; convert high nibble first
                lsr
                lsr
                lsr
                jsr _nibble_to_ascii
                pla

                ; fall through to _nibble_to_ascii

_nibble_to_ascii:
        ; """Private helper function for byte_to_ascii: Print lower nibble
        ; of A and and EMIT it. This does the actual work.
        ; """
                and #$F
                ora #'0'
                cmp #'9'+1
                bcc +
                adc #6

+               jmp emit_a

                rts


ascii_to_byte:
        ; convert two ascii hex digit characters in Y (hi) and A (lo) to a byte,
        ; returning the result in A with C=0 on success, C=1 if invalid.
        ; Uses tmpdsp.
        phx
        ldx base
        phx
        ldx #16                 ; parsing hex digits
        stx base
        jsr ascii_to_digit      ; lower nibble
        bcs _done
        sta tmpdsp
        tya
        jsr ascii_to_digit      ; high nibble
        bcs _done
        asl                     ; $0-$f on success so shifts leave C=0
        asl
        asl
        asl
        ora tmpdsp              ; combine with lower nibble
_done:
        plx
        stx base                ; restore original base
        plx
        rts


ascii_to_digit:
        ; convert A from ASCII character [0-9A-Za-z] to a digit in the
        ; current base, with C=0 on success or C=1 with invalid char
        cmp #$40
        bcs _hi

        sbc #'0'-1              ; < '0' => large
        cmp #10                 ; C=1 if >= 10 (error)
        bcc _chk
        bcs _done               ; invalid
_hi:
        and #$1f                ; mask to accept uc & lc
        beq _done               ; @ or ` is error (C=1 from above)
        adc #8                  ; A=1 => 1+8+1=10
_chk
        cmp base                ; set C=1 if not < base
_done:
        rts


; =====================================================================
; HEADER HELPER FUNCTIONS
;
; These functions help navigate the variable size headers.
; Currently nt_to_nt is a subroutine that's shared by several callers.
; In the current test suite this is called about 2M times
; in find_nt_by_name (during parsing), and another 100K times
; in find_nt_by_xt (mostly by compile,).
; Inlining nt_to_nt in the find_nt_by_name variant would save about 25M cycles
; (about 14% of the test suite cycles) but this is only a compile time expense
; so might not be worth it for "real" forth code.
; An alternative would be a simple cache of recently used words.

nt_to_nt:
                ; If the header structure goes off the rails we can get hung up here
                ; and start looping through non-NT addresses.  A simple safety check is
                ; to watch (tmp1)+1 for a length byte >= 32.  For example:
                ;
                ;       ldy #1
                ;       lda (tmp1),y
                ;       cmp #32
                ;       bcc +
                ;       lda #str_see_nt
                ;       jsr print_string_no_lf
                ;       lda #$0a
                ;       jsr kernel_putc
                ; -     bra -
                ; +

                ; nt_to_nt updates tmp1 to point to the previous NT header,
                ; setting tmp1=0 and Z=1 when we've reached the end of the list
                lda (tmp1)              ; check the flag bits
                lsr                     ; FP => carry

                ldy #2
                lda (tmp1),y            ; get the LSB
                bcc _no_msb             ; is there a MSB?

                ; It's a two byte pointer, with A as LSB
                pha                     ; stash LSB since we can't update tmp1 yet
                iny
                lda (tmp1),y            ; fetch MSB
                sta tmp1+1              ; update MSB
                pla
                bra _finish

_no_msb:
                ; New MSB is same as current one if new LSB < current one
                ; else current MSB-1 if new LSB >= current one
                cmp tmp1                ; C=1 when new LSB >= current one
                bcc _finish             ; leave current MSB alone
                dec tmp1+1

_finish:
                ; write LSB and set Z flag
                sta tmp1
                ora tmp1+1

                rts


nt_to_xt:
        ; Given a valid nt in tmp1 (unchanged), return its xt in Y/A

                lda (tmp1)              ; DC flag tells us pointer (1) or adjoining (0)
                lsr                     ; FP -> carry bit
                bit #DC>>1              ; is DC set (leaves carry unchanged)?
                beq _adjoint

                ; the explicit xt pointer is at offset 3 (when FP=0) or 4 (when FP=1)
                ldy #3
                bcc +
                iny
+
                lda (tmp1),y            ; fetch LSB of xt
                pha
                iny
                lda (tmp1),y            ; fetch MSB
                tay
                pla
                rts
_adjoint:
                ; otherwise calculate nt + header + name length
                and #(DC+LC)>>1         ; mask length bits
                adc #4                  ; add along with FP in carry

                ldy #1                  ; add name length byte
                adc (tmp1),y            ; carry already clear and stays clear

                adc tmp1                ; add to nt
                ldy tmp1+1
                bcc +
                iny                     ; maybe update MSB
+
                rts


find_nt_by_name:
        ; """Given a string on the stack ( addr  n ) with n at most 31
        ; and tmp1 pointing at an NT header, search each
        ; linked header looking for a matching name.
        ;
        ; On success tmp1 points at the matching NT, with A nonzero and Z=0.
        ; On failure tmp1 is 0, A=0 and Z=1.
        ; Stomps tmp2.  The stack is unchanged.
        ; """

                lda tmp1                ; Start by checking if initial NT is zero
                ora tmp1+1
                beq _done

_loop:
                ; first quick test: Are strings the same length?
                ldy #1                  ; length is at header offset 1
                lda (tmp1),y
                cmp 0,x
                beq _maybe

_next_nt:
                jsr nt_to_nt

                bne _loop        ; A=0 means failure, otherwise try again
                bra _done

_maybe:
                ; second quick test: could first characters be equal?
                ; Use header status flags to calculate offset to name (header size)
                lda (tmp1)              ; Fetch status flags
                and #DC+LC+FP
                lsr
                adc #4
                tay

                lda (tmp1),y            ; first character of candidate
                eor (2,x)               ; flag any mismatched bits
                and #%11011111          ; but ignore upper/lower case bit
                bne _next_nt            ; definitely not equal if any bits differ

                ; Same length and probably same first character
                ; (though we still have to check properly).
                ; Suck it up and compare all characters. We go
                ; from back to front, because words like CELLS and CELL+ would
                ; take longer otherwise.

                sty tmptos              ; stash header length, the name offset
                sec
                lda 2,x                 ; Copy mystery string addr - Y to tmp2
                sbc tmptos
                sta tmp2
                lda 3,x
                sbc #0
                sta tmp2+1

                clc
                lda 0,x                 ; string length
                sta tmptos+1            ; our loop counter
                adc tmptos              ; add offset
                tay
                dey

_next_char:
                lda (tmp2),y            ; last char of mystery string
                ; Lowercase the incoming charcter.
                cmp #'Z'+1
                bcs _check_char
                cmp #'A'
                bcc _check_char

                ; Convert uppercase letter to lowercase.
                ora #$20

_check_char:
                cmp (tmp1),y            ; last char of word we're testing against
                bne _next_nt

                dey
                dec tmptos+1
                bne _next_char

                ; fall through on success with non-zero result
                lda #$ff

_done:
                rts


find_nt_by_xt:
        ; Given ( xt ) on the stack and tmp1 pointing to an NT header
        ; search each linked header looking for a matching xt

                lda tmp1                ; Start by checking if initial NT is zero
                ora tmp1+1
                beq _done               ; failure with A=0, Z=1

_loop:
                jsr nt_to_xt            ; nt in tmp1 to xt in y/a

                ; ( xt )
                cmp 0,x                 ; does LSB match?
                bne _next_nt
                tya
                cmp 1,x                 ; does MSB match?
                bne _next_nt

                lda #$ff                ; non-zero result for success
                bra _done

_next_nt:
                jsr nt_to_nt
                bne _loop               ; A=0 means failure, otherwise try again

_done:
                rts



current_to_dp:
        ; """Look up the current (compilation) dictionary pointer
        ; in the wordlist set and put it into the dp zero-page
        ; variable. Uses A and Y.
        ; """
                ; Determine which wordlist is current
                ldy #current_offset
                lda (up),y      ; current is a byte variable
                asl             ; turn it into an offset (in cells)

                ; Get the dictionary pointer for that wordlist.
                clc
                adc #wordlists_offset   ; add offset to wordlists base.
                tay
                lda (up),y              ; get the dp for that wordlist.
                sta dp
                iny
                lda (up),y
                sta dp+1

                rts


dp_to_current:
        ; """Look up which wordlist is current and update its pointer
        ; with the value in dp. Uses A and Y.
        ; """
                ; Determine which wordlist is current
                ldy #current_offset
                lda (up),y      ; current is a byte variable
                asl             ; turn it into an offset (in cells)

                ; Get the dictionary pointer for that wordlist.
                clc
                adc #wordlists_offset   ; add offset to wordlists base.
                tay
                lda dp
                sta (up),y              ; get the dp for that wordlist.
                iny
                lda dp+1
                sta (up),y

                rts


is_printable:
        ; """Given a character in A, check if it is a printable ASCII
        ; character in the range from $20 to $7E inclusive. Returns the
        ; result in the Carry Flag: 0 (clear) is not printable, 1 (set)
        ; is printable. Keeps A. See
        ; http://www.obelisk.me.uk/6502/algorithms.html for a
        ; discussion of various ways to do this
                cmp #AscSP              ; $20
                bcc _done
                cmp #$7F + 1             ; '~'
                bcs _failed

                sec
                .byte OpBITzp
_failed:
                clc
_done:
                rts


is_whitespace:
        ; """Given a character in A, check if it is a whitespace
        ; character, that is, an ASCII value from 0 to 32 (where
        ; 32 is SPACE). Returns the result in the Carry Flag:
        ; 0 (clear) is no, it isn't whitespace, while 1 (set) means
        ; that it is whitespace. See PARSE and PARSE-NAME for
        ; a discussion of the uses. Does not change A or Y.
                cmp #00         ; explicit comparison to leave Y untouched
                bcc _done

                cmp #AscSP+1
                bcs _failed

                sec
                bra _done
_failed:
                clc
_done:
                rts


; Underflow tests. We jump to the label with the number of cells (not: bytes)
; required for the word. This routine flows into the generic error handling
; code
underflow_1:
        ; """Make sure we have at least one cell on the Data Stack"""
                cpx #dsp0-1
                bpl underflow_error
                rts
underflow_2:
        ; """Make sure we have at least two cells on the Data Stack"""
                cpx #dsp0-3
                bpl underflow_error
                rts
underflow_3:
        ; """Make sure we have at least three cells on the Data Stack"""
                cpx #dsp0-5
                bpl underflow_error
                rts
underflow_4:
        ; """Make sure we have at least four cells on the Data Stack"""
                cpx #dsp0-7
                bpl underflow_error
                rts

underflow_error:
                ; Entry for COLD/ABORT/QUIT
                lda #err_underflow      ; fall through to error

error:
        ; """Given the error number in a, display the error and call abort. Uses tmp3.
        ; """
                pha                     ; save error
                jsr print_error
                jsr w_cr
                pla
                cmp #err_underflow      ; should we display return stack?
                bne _no_underflow

                lda #err_returnstack
                jsr print_error

                ; dump return stack from SP...$1FF to help debug source of underflow
                ; the data stack pointer in X is already corrupted so safe to reuse here
                tsx
-
                inx
                beq +
                jsr w_space
                lda $100,x
                jsr byte_to_ascii
                bra -
+
                jsr w_cr

_no_underflow:
                jmp w_abort            ; no jsr, as we clobber return stack

; =====================================================================
; PRINTING ROUTINES

; print_string_no_lf prints a high-bit terminated string
; from string_table (see strings.asm) indexed by the accumulator,
; with no trailing line ending.

; print_error does likewise, with A indexing error_table

; print_common provides a lower-level alternative for error
; handling and anything else that provides the address of a
; high-bit terminated string directly in tmp3. These routines assume that
; printing should be more concerned with size than speed, because anything to
; do with humans reading text is going to be slow.

print_string_no_lf:
        ; """Given the number of a zero-terminated string in A, print it to the
        ; current output without adding a LF. Uses Y and tmp3 by falling
        ; through to print_common
        ; """
                ; Get the entry from the string table
                asl
                tay
                lda string_table,y
                sta tmp3                ; LSB
                lda string_table+1,y
                sta tmp3+1              ; MSB

                ; fall through to print_common
print_common:
        ; """Common print routine used by both printing routines.
        ; Assumes tmp3 points to a high-bit terminated string.
        ; Uses Y.
        ; """
                ldy #0
_loop:
                lda (tmp3),y
                bpl +                           ; strings are high-bit terminated

                and #$7f                        ; last character, clear high bit
                ldy #$ff                        ; flag end of loop
+
                jsr emit_a                      ; allows vectoring via output
                iny
                bne _loop

                rts


print_error:
        ; """Given the error number in a, print the associated error string. Uses tmp3.
        ; """
                asl
                tay
                lda error_table,y
                sta tmp3                        ; LSB
                iny
                lda error_table,y
                sta tmp3+1                      ; MSB

                bra print_common


print_u:
        ; """basic printing routine used by higher-level constructs,
        ; the equivalent of the forth word  0 <# #s #> type  which is
        ; basically u. without the space at the end. used for various
        ; outputs
        ; """
                jsr w_zero                     ; 0
                jsr w_less_number_sign         ; <#
                jsr w_number_sign_s            ; #S
                jsr w_number_sign_greater      ; #>
                jmp w_type                     ; JSR/RTS because never compiled

.weak
kernel_kbhit .proc
                lda #1
                rts
.endproc
.endweak

code_end:

; END
