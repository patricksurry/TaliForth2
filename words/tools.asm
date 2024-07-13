; ## BYE ( -- ) "Break"
; ## "bye"  tested  ANS tools ext
        ; """https://forth-standard.org/standard/tools/BYE"""
xt_bye:
w_bye:
                ; Use the kernel_bye routine provided in the platform
                ; file.  For simulators, this is traditionally just a
                ; brk instruction, but platforms with another OS can
                ; arrange to jump back into that OS.  This routine
                ; does not return.
                jmp kernel_bye
z_bye:



; ## DOT_S ( -- ) "Print content of Data Stack"
; ## ".s"  tested  ANS tools
        ; """https://forth-standard.org/standard/tools/DotS
        ; Print content of Data Stack non-distructively. We follow the format
        ; of Gforth and print the number of elements first in brackets,
        ; followed by the Data Stack content (if any).
        ;
        ; Since this is for humans, we don't have to worry about speed.
        ; """

xt_dot_s:
w_dot_s:
                jsr w_depth    ; ( -- u )

                ; Print stack depth in brackets
                lda #'<'
                jsr emit_a

                ; We keep a copy of the number of the things on the stack
                ; to use as a counter later down. This assumes that there
                ; are less than 255 elements on the stack
                lda 0,x
                pha

                ; print unsigned number without the trailing space
                dex             ; DUP
                dex
                sta 0,x
                stz 1,x

                jsr print_u

                lda #'>'
                jsr emit_a
                lda #AscSP      ; ASCII for SPACE
                jsr emit_a

                inx
                inx

                ; There will be lots of cases where the stack is empty. If that
                ; is so, get out of here quickly
                cpx #dsp0
                beq _done

                ; We have at least one element on the stack. The depth of the
                ; stack is on the stack, we can use it as a counter. We go
                ; from bottom to top
                ply

                lda #dsp0-1     ; go up one to avoid garbage
                sta tmp3
                stz tmp3+1      ; must be zero page on the 65c02
_loop:
                dex
                dex

                lda (tmp3)
                sta 1,x
                dec tmp3

                lda (tmp3)
                sta 0,x
                dec tmp3
                phy

                jsr w_dot

                ply
                dey
                bne _loop

                pha             ; dummy to balance stack
_done:
                pla
z_dot_s:        rts



; ## DUMP ( addr u -- ) "Display a memory region"
; ## "dump"  tested  ANS tools
        ; """https://forth-standard.org/standard/tools/DUMP
        ;
        ; DUMP's exact output is defined as "implementation dependent".
        ; This is in assembler because it is
        ; useful for testing and development, so we want to have it work
        ; as soon as possible. Uses TMP2
        ; """

xt_dump:
                jsr underflow_2
w_dump:
_row:
                ; how many characters to print this row?
                ldy #16
                lda 1,x
                bne +

                lda 0,x
                beq _done

                cmp #16
                bcs +
                tay
+
                jsr w_cr

                ; print address number
                ; and store in TMP2 as the base address for the row
                lda 3,x
                sta tmp2+1
                jsr byte_to_ascii
                lda 2,x
                sta tmp2
                jsr byte_to_ascii

                jsr w_space
                jsr w_space

                ; temporarily trash addr for loop counter
                sty 2,x         ; temporary storage for loop counter
                stz 3,x         ; flag for pass = 0 (bytes) or -1 (ascii)
_pass:
                ldy #0
_loop:
                ; dump the contents
                lda (tmp2),y
                bit 3,x
                bmi _ascii

                jsr byte_to_ascii
                jsr w_space
                bra _nextbyte
_ascii:
                ; Handle ASCII printing
                jsr is_printable
                bcs +
                lda #'.'                 ; Print dot if not printable
+
                jsr emit_a
_nextbyte:
                ; extra space after eight bytes
                cpy #7
                bne +
                jsr w_space
+
                iny
                tya
                cmp 2,x
                bne _loop

                lda 3,x
                bmi +                   ; done both passes?
                jsr nextpass
                bra _pass
+
                ; Done with one line, increment address and decrement count
                sec
                lda 0,x
                sbc 2,x
                sta 0,x
                bcs +
                dec 1,x
+
                clc
                lda tmp2+1
                sta 3,x
                lda tmp2
                adc 2,x
                sta 2,x
                bcc +
                inc 3,x
+
                bra _row                ; new row

_done:
                jsr w_cr
                inx
                inx
                inx
                inx

z_dump:         rts


nextpass:
                dec 3,x                 ; set flag to -1 for second pass

                ; add spaces for alignment
                ; we want 1 + 3*(16-Y) + (1 if Y<8)

                ; 16-A = 16 + (255-A) + 1 - 256

                tya                     ; 16-A = 16 + (255-A) + 1 - 256
                eor #$ff
                sec
                adc #16

                sta tmpdsp
                asl                     ; 2*(16-Y), leaving C=0
                adc tmpdsp              ; 3*(16-Y)

                cpy #8                  ; + 1 if Y < 8
                bcs +
                ina
+
                tay
-
                jsr w_space
                dey
                bpl -                   ; Y...0 inclusive
                rts


; ## QUESTION ( addr -- ) "Print content of a variable"
; ## "?"  tested  ANS tools
        ; """https://forth-standard.org/standard/tools/q
        ;
        ; Only used interactively. Since humans are so slow, we
        ; save size and just go for the subroutine jumps
        ; """
xt_question:
w_question:
                ; FETCH takes care of underflow check
                jsr w_fetch
                jsr w_dot

z_question:     rts



; ## SEE ( "name" -- ) "Print information about a Forth word"
; ## "see" tested  ANS tools
        ; """https://forth-standard.org/standard/tools/SEE
        ; SEE takes the name of a word and prints its name token (nt),
        ; execution token (xt), size in bytes, flags used, and then dumps the
        ; code and disassembles it.
        ; """

xt_see:
w_see:
                jsr w_parse_name       ; ( addr u )
                jsr w_find_name        ; ( nt | 0 )

                ; If we got back a zero we don't know that word and so we quit
                ; with an error
                lda 0,x
                ora 1,x
                bne +

                lda #err_noname
                jmp error
+
                jsr w_cr

                ; We have a legal word, so let's get serious. Save the current
                ; number base and use hexadecimal instead.
                lda base
                pha
                jsr w_hex

                lda #str_see_nt
                jsr print_string_no_lf

                jsr w_dup              ; ( nt nt )
                jsr w_u_dot
                jsr w_space            ; ( nt )

                jsr w_dup              ; ( nt nt )
                jsr w_name_to_int      ; ( nt xt )

                lda #str_see_xt
                jsr print_string_no_lf

                jsr w_dup              ; ( nt xt xt )
                jsr w_u_dot
                jsr w_cr               ; ( nt xt )

                ; Show flag values from the status byte along with
                ; several calculated (synthetic) flag values
                jsr w_over              ; ( nt xt nt )
                jsr w_one_plus          ; ( nt xt nt+1 )
                lda (0, x)
                sta 0,x                 ; stash status flag byte
                stz 1,x                 ; placeholder for synthetic flags

                ; calculate synthetic flags in reverse order to template
                and #ST                 ; calculate ST flag
                cmp #ST
                beq +                   ; C=1 when ST set
                clc
+
                rol 1,x                 ; add to flag byte

                jsr w_over
                jsr has_uf_check        ; C=1 when UF set
                rol 1,x                 ; add to flag byte

                lda #N_FLAGS            ; count off status byte flags
                sta tmptos

                ; use a high-bit terminated template string to show flag names
                ; and insert flag values at placeholders marked by ascii zeros
                lda #<see_flags_template
                sta tmp3                ; LSB
                lda #>see_flags_template
                sta tmp3+1              ; MSB

                ldy #0                  ; index the string
_loop:
                lda (tmp3),y            ; next char in template
                bpl +                   ; end of string?

                ldy #$ff                ; flag end of loop
                and #$7f                ; clear high bit of A to get last character
+
                bne _emit               ; flag placeholder?

                jsr w_space             ; print <space>, <flag>, <space>

                dec tmptos
                bmi _synthetic          ; more core status flags?
                lsr 0,x                 ; shift next flag bit into carry
                bra +
_synthetic:
                lsr 1,x                 ; show synthetic flags after core ones
+
                lda #'0'                ; convert C=0/1 into '0' or '1'
                adc #0
                jsr emit_a              ; write the flag digit

                lda #' '                ; fall through and add trailing space
_emit:
                jsr emit_a

                iny
                bne _loop

                jsr w_cr

                inx
                inx                     ; ( nt xt )

                ; Figure out the size
                lda #str_see_size
                jsr print_string_no_lf

                jsr w_swap             ; ( xt nt )
                jsr w_wordsize         ; ( xt u )
                jsr w_dup              ; ( xt u u ) for DUMP and DISASM
                jsr w_decimal
                jsr w_u_dot            ; ( xt u )
                jsr w_hex
                jsr w_cr

                ; Dump hex and disassemble
.if "disassembler" in TALI_OPTIONAL_WORDS
                jsr w_two_dup          ; ( xt u xt u )
.endif
                jsr w_dump
.if "disassembler" in TALI_OPTIONAL_WORDS
                jsr w_disasm
.endif
                pla
                sta base

z_see:          rts



; ## WORDS ( -- ) "Print known words from Dictionary"
; ## "words"  tested  ANS tools
        ; """https://forth-standard.org/standard/tools/WORDS
        ; This is pretty much only used at the command line so we can
        ; be slow and try to save space.
        ; """

xt_words:
w_words:
                ; we follow Gforth by starting on the next
                ; line
                jsr w_cr

                ; We pretty-format the output by inserting a line break
                ; before the end of the line. We can get away with pushing
                ; the counter to the stack because this is usually an
                ; interactive word and speed is not that important
                lda #0
                pha

                ; Set up for traversing the wordlist search order.
                dex                     ; Make room on the stack for
                dex                     ; a dictionary pointer.
                stz tmp3                ; Start at the beginning of
                                        ; the search order.
_wordlist_loop:
                ldy #num_order_offset   ; Check against byte variable #ORDER.
                lda tmp3
                cmp (up),y              ; See if we are done.
                bne _have_wordlist

                ; We ran out of wordlists to search.
                bra _words_done

_have_wordlist:
                ; start with last word in Dictionary
                ; Get the current wordlist id
                clc                     ; Index into byte array SEARCH-ORDER.
                adc #search_order_offset
                tay
                lda (up),y              ; Get the index into array WORDLISTS

                ; Get the DP for that wordlist.
                asl                     ; Turn offset into cells offset.
                clc
                adc #wordlists_offset
                tay
                lda (up),y              ; Save the DP for this wordlist
                sta 0,x                 ; on the stack. ( nt )
                iny
                lda (up),y
                sta 1,x

_loop:
                jsr w_dup              ; ( nt nt )
                jsr w_name_to_string   ; ( nt addr u )

                ; Insert line break if we're about to go past the end of the
                ; line
                pla
                clc
                adc 0,x
                ina                     ; don't forget the space between words
                cmp #MAX_LINE_LENGTH    ; usually 79
                bcc +

                jsr w_cr

                lda 0,x                 ; After going to next line, start
                ina                     ; with length of this word.
+
                pha
                jsr w_type             ; ( nt )

                lda #AscSP
                jsr emit_a

                ; get next word, which begins two down
                jsr w_one_plus         ; 1+
                jsr w_one_plus         ; 1+
                jsr w_fetch            ; @ ( nt+1 )

                ; if next address is zero, we're done
                lda 0,x
                ora 1,x
                bne _loop

                ; Move on to the next wordlist in the search order.
                inc tmp3
                bra _wordlist_loop

_words_done:
                pla                     ; dump counter

                inx
                inx

z_words:        rts
