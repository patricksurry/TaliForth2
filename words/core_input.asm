; Core Forth words for input

; Tali Forth 2 for the 65c02
; Scot W. Stevenson <scot.stevenson@gmail.com>
; Sam Colwell
; Patrick Surry
; First version: 19. Jan 2014
; This version: 21. Apr 2024


; ## ACCEPT ( addr n -- n ) "Receive a string of characters from the keyboard"
; ## "accept"  auto  ANS core
        ; """https://forth-standard.org/standard/core/ACCEPT
        ; Receive a string of at most n1 characters, placing them at
        ; addr. Return the actual number of characters as n2. Characters
        ; are echoed as they are received. ACCEPT is called by REFILL in
        ; modern Forths.
        ; """
xt_accept:
                jsr underflow_2
w_accept:
                ; Abort if we were asked to receive 0 chars
                lda 0,x
                ora 1,x
                bne _not_zero

                inx
                inx
                stz 0,x
                stz 1,x

                jmp accept_done

_not_zero:
                lda 0,x         ; number of chars to get in tmp2 ...
                sta tmp2
                stz tmp2+1      ; ... but we only accept max 255 chars

                lda 2,x         ; address of buffer is NOS, to tmp1
                sta tmp1
                lda 3,x
                sta tmp1+1

                inx
                inx

                ldy #0

                ; Select the next history buffer. Clear bit 3 first (so overflow
                ; from bit 2 to 3 is OK)
                lda status
                and #$F7

                ; Increment the buffer number (overflow from 7 to 0 OK)
                ina

                ; Set bit 3 for detecting if CTRL-n has been pressed the first
                ; time. This bit will be cleared on the first CTRL-n or CTRL-p
                ; received and won't be used to calculate the history buffer
                ; offset.
                ora #%00001000
                sta status

accept_loop:
                ; Out of the box, py65mon catches some CTRL sequences such as
                ; CTRL-c. We also don't need to check for CTRL-l because a
                ; vt100 terminal clears the screen automatically.

                ; This is the internal version of KEY without all the mucking
                ; about with the Data Stack while still using the input vector
                jsr key_a

                ; We quit on both line feed and carriage return
                cmp #AscLF
                beq _eol
                cmp #AscCR
                beq _eol

                ; BACKSPACE and DEL do the same thing for the moment
                cmp #AscBS
                beq _backspace
                cmp #AscDEL     ; (CTRL-h)
                beq _backspace

.if TALI_OPTION_HISTORY
                ; Check for CTRL-p and CTRL-n to recall input history
                cmp #AscCP
                beq _ctrl_p
                cmp #AscCN
                beq _ctrl_n
.endif
                ; That's enough for now. Save and echo character.
                sta (tmp1),y
                iny

                ; EMIT_A sidesteps all the fooling around with the Data Stack
                jsr emit_a

                cpy tmp2        ; reached character limit?
                bne accept_loop       ; fall through if buffer limit reached
                bra _buffer_full

_eol:
                jsr w_space    ; print final space

_buffer_full:
                ; REFILL updates ciblen and toin, we don't need to do it here
                sty 0,x         ; Y contains number of chars accepted already
                stz 1,x         ; we only accept 256 chars

                jmp accept_done

_backspace:
                ; Handle backspace and delete kex, which currently do the same
                ; thing
                cpy #0          ; buffer empty?
                bne +

                lda #AscBELL    ; complain and don't delete beyond the start of line
                jsr emit_a
                iny
+
                dey
                lda #AscBS      ; move back one
                jsr emit_a
                lda #AscSP      ; print a space (rubout)
                jsr emit_a
                lda #AscBS      ; move back over space
                jsr emit_a

                bra accept_loop

.if TALI_OPTION_HISTORY
_ctrl_p:
                ; CTRL-p was pressed. Recall the previous input buffer.

                ; Select the previous buffer
                lda status

                ; Check for 0 (need to wrap back to 7)
                and #7
                bne _ctrl_p_dec

                ; We need to wrap back to 7.
                lda status
                ora #7
                sta status
                bra _recall_history

_ctrl_p_dec:
                ; It's safe to decrement the buffer index directly.
                dec status
                bra _recall_history

_ctrl_n:
                ; CTRL-n was pressed. Recall the next input buffer. Select
                ; the next buffer Check bit 3. If it's set, this is the first
                ; time CTRL-n has been pressed and we should select the CURRENT
                ; history buffer.
                lda #$8
                bit status
                bne _recall_history

                ; This isn't the first time CTRL-n has been pressed, select the
                ; next history buffer. Clear bit 3 first (so overflow is OK)
                lda status
                and #$F7

                ; Increment the buffer number (overflow from 7 to 0 OK)
               ina

                ; Bit 3 (if it got set by going from buffer 7 to 0) will
                ; be cleared below.
                sta status

                ; Falls through to _recall_history

_recall_history:
                ; Clear bit 3 (first time ctrl-n recall) bit in status
                lda #%00001000
                trb status

                jsr accept_total_recall

                ; tmp3 now has the address of the previous history buffer.
                ; First byte of buffer is length. Clear the line by sending
                ; CR, Y spaces, then CR.
                lda #AscCR
                jsr emit_a

input_clear:
                cpy #0
                beq input_cleared

                lda #AscSP
                jsr emit_a
                dey
                bra input_clear

input_cleared:
                lda #AscCR
                jsr emit_a

                ; Save the history length byte into histinfo+1
                ; ldy #0        ; Y is already 0 by clearing the line.
                lda (tmp3),y
                sta status+1

                ; Increment the tmp3 pointer so we can use ,y addressing
                ; on both tmp1 (the input buffer) and tmp3 (the history
                ; buffer)
                inc tmp3
                bne +           ; Increment the upper byte on carry.
                inc tmp3+1
+
                ; Copy the history buffer into the input buffer,
                ; sending the characters to the output as we go.
                lda #AscCR
                jsr emit_a

_history_loop:
                ; See if we have reached the end of the history buffer.
                cpy status+1
                bne +
                jmp accept_loop       ; Needs a long jump
+
                ; See if we have reached the end of the input buffer.
                ; (only comparing to lower byte as we currently limit
                ; to 255 characters max)
                cpy tmp2
                beq _hist_filled_buffer

                ; Copy a character and echo.
                lda (tmp3),y
                sta (tmp1),y
                jsr emit_a

                ; Move to the next character.
                iny
                bra _history_loop

_hist_filled_buffer:
                ; We don't want a history recall to EOL our buffer,
                ; so back up one character and return to editing.
                dey
                jmp accept_loop

accept_done:
                ; Copy the input buffer into the currently
                ; selected history buffer.
                jsr accept_total_recall
                sta status+1

                ; Also save it in the first buffer byte.
                ldy #0
                sta (tmp3),y

                ; Move path the count to the data bytes
                inc tmp3
                bne +           ; Increment the upper byte on carry.
                inc tmp3+1
+
                ; Copy the characters from the input buffer to the
                ; history buffer.

_save_history_loop:
                cpy status+1
                beq _save_history_done

                lda (tmp1),y
                sta (tmp3),y
                iny
                bra _save_history_loop

_save_history_done:
.else
accept_done:            ; nothing to do if we're not saving history
.endif

z_accept:
                rts

.if TALI_OPTION_HISTORY
accept_total_recall:
        ; """Internal subroutine for ACCEPT that recalls history entry"""

                ; Generate the address of the buffer in tmp3. Start with the
                ; base address.
                lda #<hist_buff
                sta tmp3
                lda #>hist_buff
                sta tmp3+1

                ; This is a bit annoying as some bits go into each byte.
                ; .....xxx gets put into address like ......xx x.......
                lda status
                ror
                and #3
                clc
                adc tmp3+1
                sta tmp3+1

                lda status
                ror             ; Rotate through carry into msb.
                ror
                and #$80
                clc
                adc tmp3
                sta tmp3
                bcc +           ; Increment the upper byte on carry.
                inc tmp3+1
+
                ; Save the current length of the input buffer in
                ; histinfo+1 temporarily.  Reduce to 127 if larger.
                tya
                cmp #$80
                bcc +
                lda #$7F
+
                rts
.endif



; ## BACKSLASH ( -- ) "Ignore rest of line"
; ## "\"  auto  ANS block ext
        ; """https://forth-standard.org/standard/block/bs"""
xt_backslash:
w_backslash:
                ; Check BLK to see if we are interpreting a block
                ldy #blk_offset
                lda (up),y
                iny
                ora (up),y
                beq backslash_not_block

                ; We are in a block.  Move toin to next multiple of 64.

                ; First, however, we have to see if we are at an exact
                ; multiple of 64+1, which happens when a \ is at the end
                ; of a line (in which case we do nothing).  We also have
                ; to check for exact multiple of 64, which will happen with
                ; a backslash at the very end of a block.
                lda toin
                and #$3F
                beq z_backslash
                cmp #$1
                beq z_backslash

                ; Not at the end of the line (beginning of next line,
                ; after parsing the \, technically), so move to the
                ; next line.
                lda toin
                and #$C0        ; Clear lower bits to move to beginning of line.

                clc             ; Add $40 (64 decimal) to move to next line.
                adc #$40
                sta toin
                bcc z_backslash
                inc toin+1
                bra z_backslash

backslash_not_block:
                lda ciblen
                sta toin
                lda ciblen+1
                sta toin+1

z_backslash:    rts



; ## CHAR ( "c" -- u ) "Convert character to ASCII value"
; ## "char"  auto  ANS core
        ; """https://forth-standard.org/standard/core/CHAR"""
xt_char:
w_char:
                ; get character from string, returns ( addr u )
                jsr w_parse_name

                ; if we got back a zero, we have a problem
                lda 0,x
                ora 1,x
                bne _not_empty

                lda #err_noname
                jmp error

_not_empty:
                inx             ; drop number of characters, leave addr
                inx
                lda (0,x)       ; get character (equivalent to C@)
                sta 0,x
                stz 1,x         ; MSB is always zero

z_char:         rts



; This is a special jsr target to skip the zeroing of BLK at the beginning
; of evaluate.  It's used by LOAD to allow setting BLK while the block is
; being evaluated.  Evaluate's normal behavior is to zero BLK.
load_evaluate:
                ; Set a flag (using tmp1) to not zero BLK
                lda #$FF
                sta tmp1
                bra load_evaluate_start

; ## EVALUATE ( addr u -- ) "Execute a string"
; ## "evaluate"  auto  ANS core
        ; """https://forth-standard.org/standard/core/EVALUATE
        ; Set SOURCE-ID to -1, make addr u the input source, set >IN to zero.
        ; After processing the line, revert to old input source. We use this
        ; to compile high-level Forth words and user-defined words during
        ; start up and cold boot. In contrast to ACCEPT, we need to, uh,
        ; accept more than 255 characters here, even though it's a pain in
        ; the 8-bit.
        ; """

xt_evaluate:
                jsr underflow_2
w_evaluate:
                ; Clear the flag to zero BLK.  Only LOAD will set the flag,
                ; and will set the block number.
                stz tmp1

                ; If u is zero (which can happen a lot for the user-defined
                ; words), just leave again
                lda 0,x
                ora 1,x
                bne evaluate_got_work

                inx
                inx
                inx
                inx

                bra evaluate_done

; Special entry point for LOAD to bypass the zeroing of BLK.
load_evaluate_start:
evaluate_got_work:
                ; Save the current value of BLK on the return stack.
                ldy #blk_offset+1
                lda (up),y
                pha
                dey
                lda (up),y
                pha

                ; See if we should zero BLK.
                lda tmp1
                bne _nozero

                ; Set BLK to zero.
                ; lda #0        ; A is already zero from loading tmp1
                sta (up),y
                iny
                sta (up),y

_nozero:
                ; Save the input state to the Return Stack
                jsr w_input_to_r

                ; set SOURCE-ID to -1
                lda #$FF
                sta insrc
                sta insrc+1

                ; set >IN to zero
                stz toin
                stz toin+1

                ; move TOS and NOS to input buffers
                lda 0,x
                sta ciblen
                lda 1,x
                sta ciblen+1

                lda 2,x
                sta cib
                lda 3,x
                sta cib+1

                inx             ; A clean stack is a clean mind
                inx
                inx
                inx

                jsr interpret   ; ( -- )

                ; restore variables
                jsr w_r_to_input

                ; Restore BLK from the return stack.
                ldy #blk_offset
                pla
                sta (up),y
                iny
                pla
                sta (up),y

evaluate_done:
z_evaluate:     rts



; ## KEY ( -- char ) "Get one character from the input"
; ## "key"  tested  ANS core
xt_key:
w_key:
        ; """https://forth-standard.org/standard/core/KEY
        ; Get a single character of input from the vectored
        ; input without echoing.
        ; """
                jsr key_a               ; returns char in A

                dex
                dex
                sta 0,x
                stz 1,x

z_key:          rts


key_a:
        ; The 65c02 doesn't have a JSR (ADDR,X) instruction like the
        ; 65816, so we have to fake the indirect jump to vector it.
        ; This is depressingly slow. We use this routine internally
        ; to avoid manipulating the Data Stack when we just want a
        ; character
                jmp (input)             ; JSR/RTS



; ## KEY? ( -- char ) "Return true if a character is available"
; ## "key?"  tested  ANS core
xt_keyq:
w_keyq:
        ; """https://forth-standard.org/standard/core/KEYq
        ; Check if a key is available from the vectored havekey.
        ; Use KEY to fetch it.
        ; """
                ldy #0
                jsr keyq_a
                beq +           ; A=0 => Y=0
                dey             ; A<>0 => Y=#$ff
+
                dex
                dex
                sty 0,x         ; store either $0000 or $ffff
                sty 1,x

z_keyq:         rts

keyq_a:         jmp (havekey)



; ## PAREN ( -- ) "Discard input up to close paren ( comment )"
; ## "("  auto  ANS core
        ; """http://forth-standard.org/standard/core/p"""

xt_paren:
w_paren:
                ; Put a right paren on the stack.
                dex
                dex
                lda #41     ; Right parenthesis
                sta 0,x
                stz 1,x

                ; Call parse.
                jsr w_parse

                ; Throw away the result.
                inx
                inx
                inx
                inx

z_paren:        rts



; ## PARSE_NAME ( "name" -- addr u ) "Parse the input"
; ## "parse-name"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/PARSE-NAME
        ; Find next word in input string, skipping leading whitespace. This is
        ; a special form of PARSE and drops through to that word. See PARSE
        ; for more detail. We use this word internally for the interpreter
        ; because it is a lot easier to use. Reference implementations at
        ; http://forth-standard.org/standard/core/PARSE-NAME and
        ; http://www.forth200x.org/reference-implementations/parse-name.fs
        ; Roughly, the word is comparable to BL WORD COUNT. -- Note that
        ; though the ANS standard talks about skipping "spaces", whitespace
        ; is actually perfectly legal (see for example
        ; http://forth-standard.org/standard/usage#subsubsection.3.4.1.1).
        ; Otherwise, PARSE-NAME chokes on tabs.
        ;
        ; Uses tmp1, tmp2
        ; """

xt_parse_name:
w_parse_name:
                ; To enable the compilation of the high-level Forth words
                ; in forth-words.asm and user-words.asm at boot time,
                ; PARSE-NAME and PARSE must be able to deal with 16-bit string
                ; lengths. This is a pain on an 8-bit machine. The pointer
                ; to the current location is in toin (>IN). We need to check,
                ; worst case, the characters from cib+toin to cib+ciblen, and
                ; we can't just use Y as an index.

                ; The counter is CIBLEN-TOIN and stored in tmp1
                lda ciblen              ; LSB of counter
                sec
                sbc toin
                sta tmp1
                lda ciblen+1            ; MSB
                sbc toin+1
                sta tmp1+1

                ; Check the result for zero (TOIN is equal to CIBLEN)
                lda tmp1
                ora tmp1+1
                beq _empty_line

                ; We walk through the characters starting at CIB+TOIN, so we
                ; save a temp version of that in tmp2
                lda cib
                clc
                adc toin
                sta tmp2                ; LSB of first character
                lda cib+1
                adc toin+1
                sta tmp2+1              ; MSB

_skip_loop:
                lda (tmp2)              ; work copy of cib
                jsr is_whitespace
                bcc _char_found

                ; Char is still whitespace, continue
                inc tmp2
                bne +
                inc tmp2+1
+
                ; Adjust counter
                lda tmp1
                bne +
                dec tmp1+1
+               dec tmp1

                lda tmp1
                ora tmp1+1
                bne _skip_loop          ; fall through if empty line

_empty_line:
                ; Neither the ANS Forth nor the Gforth documentation say
                ; what to return as an address if a string with only
                ; spaces is given. For speed reasons, we just return junk
                ; NOS, with the TOS zero as per standard
                dex
                dex
                dex
                dex

                stz 0,x                 ; TOS is zero
                stz 1,x

                jmp z_parse_name        ; skip over PARSE

_char_found:
                ; We arrive here with tmp2 pointing to the first non-space
                ; character. This is where the word really starts, so
                ; we use it to calculate the new >IN by subtracting
                lda tmp2
                sec
                sbc cib
                sta toin
                lda tmp2+1
                sbc cib+1
                sta toin+1

                ; prepare Data Stack for PARSE by adding space
                ; as the delimiter
                dex
                dex

                lda #AscSP
                sta 0,x
                stz 1,x                 ; paranoid, now ( "name" c )

                bra w_parse             ; fall through to parse, skipping underflow


; ## PARSE ( "name" c -- addr u ) "Parse input with delimiter character"
; ## "parse"  tested  ANS core ext
        ; """https://forth-standard.org/standard/core/PARSE
        ; Find word in input string delimited by character given. Do not
        ; skip leading delimiters -- this is the main difference to PARSE-NAME.
        ; PARSE and PARSE-NAME replace WORD in modern systems. ANS discussion
        ; http://www.forth200x.org/documents/html3/rationale.html#rat:core:PARSE
        ;
        ;
        ;     cib  cib+toin   cib+ciblen
        ;      v      v            v
        ;     |###################|
        ;
        ;     |------>|  toin (>IN)
        ;     |------------------->|  ciblen
        ;
        ; The input string is stored starting at the address in the Current
        ; Input Buffer (CIB), the length of which is in CIBLEN. While searching
        ; for the delimiter, TOIN (>IN) points to the where we currently are.
        ; Since PARSE does not skip leading delimiters, we assume we are on a
        ; useful string if there are any characters at all. As with
        ; PARSE-NAME, we must be able to handle strings with a length of
        ; 16-bit for EVALUATE, which is a pain on an 8-bit machine.
        ; """

xt_parse:
                jsr underflow_1
w_parse:
                ; If the input buffer is empty, we just return
                lda ciblen
                ora ciblen+1
                beq _abort_parse

                ; If the pointer >IN is larger or equal to the length of
                ; the input buffer (CIBLEN), the line is done. Put
                ; differently, we only continue if >IN is smaller than
                ; CIBLEN
                lda toin+1              ; MSB
                cmp ciblen+1
                bcc _go_parse           ; unsigned comparison

                lda toin                ; LSB
                cmp ciblen
                bcc _go_parse

_abort_parse:
                ; Sorry, this line is over
                dex
                dex
                stz 0,x
                stz 1,x

                bra _done
_go_parse:
                ; We actually have work to do. Save the delimiter in
                ; tmptos.
                lda 0,x
                sta tmptos

                ; We can now prepare the Data Stack for the return value
                dex
                dex

                ; tmp1 is CIB+TOIN, the beginning of the current string
                ; tmp2 is initially the same as tmp1, then the work index
                ; tmp3 is CIB+CIBLEN, one char past the end of the string

                ; Calculate the beginning of the string, which is also the
                ; address to return
                lda cib
                clc
                adc toin        ; LSB
                sta tmp1
                sta tmp2
                sta 2,x

                lda cib+1
                adc toin+1      ; MSB
                sta tmp1+1
                sta tmp2+1
                sta 3,x

                ; Calculate the address where the input buffer ends plus 1, so
                ; we can compare it with TOIN, which is an index
                lda cib
                clc
                adc ciblen
                sta tmp3
                lda cib+1
                adc ciblen+1
                sta tmp3+1

                ; Initialize the offset we use to adjust EOL or found delimiter
                stz tmptos+1
_loop:
                ; If we are at the end of the string, quit
                lda tmp2
                cmp tmp3
                bne _not_empty

                lda tmp2+1
                cmp tmp3+1
                beq _eol
_not_empty:
                ; We have to do this the hard way. In fact, it's really
                ; hard since if we are dealing with a SPACE, the standard
                ; wants us to skip all whitespace, not just spaces. Otherwise,
                ; Tali would choke on tabs between words. For details, see
                ; http://forth-standard.org/standard/file#subsection.11.3.5
                ; In theory, we could make this faster by defining a delimiter
                ; that is 00 as the sign that we skip all whitespace, thereby
                ; avoiding having to test every time. However, somebody,
                ; somewhere might want to parse a zero-delimited list. Since
                ; any byte value could be chosen for that, we just test for
                ; a space every single time for the moment.
                lda (tmp2)

                ldy tmptos
                cpy #AscSP
                bne _not_whitespace

                ; The delimiter is a space, so we're looking for all
                ; whitespace
                jsr is_whitespace
                bcc _not_whitespace
                bra _found_delimiter

_not_whitespace:
                ; The delimiter is not a space, so we're looking for
                ; whatever it is
                cmp tmptos
                beq _found_delimiter

                ; Not a delimiter, next character
                inc tmp2
                bne _loop
                inc tmp2+1
                bra _loop

_found_delimiter:
                ; Increase the offset: If we've found a delimiter, we want
                ; TOIN to point to the character after it, not the delimiter
                ; itself
                inc tmptos+1
_eol:
                ; The length of the new string is tmp2-tmp1
                lda tmp2
                sec
                sbc tmp1
                sta 0,x

                lda tmp2+1
                sbc tmp1+1
                sta 1,x

                ; The new offset is tmp2-cib
                lda tmp2
                sec
                sbc cib
                sta toin
                lda tmp2+1
                sbc cib+1
                sta toin+1

                ; Add in the delimiter
                lda toin
                clc
                adc tmptos+1
                sta toin
                bcc +
                inc toin+1
+
_done:
z_parse_name:
z_parse:        rts



; ## REFILL ( -- f ) "Refill the input buffer"
; ## "refill"  tested  ANS core ext
        ; """https://forth-standard.org/standard/core/REFILL
        ; Attempt to fill the input buffer from the input source, returning
        ; a true flag if successful. When the input source is the user input
        ; device, attempt to receive input into the terminal input buffer. If
        ; successful, make the result the input buffer, set >IN to zero, and
        ; return true. Receipt of a line containing no characters is considered
        ; successful. If there is no input available from the current input
        ; source, return false. When the input source is a string from EVALUATE,
        ; return false and perform no other action." See
        ; https://www.complang.tuwien.ac.at/forth/gforth/Docs-html/The-Input-Stream.html
        ; and Conklin & Rather p. 156. Note we don't have to care about blocks
        ; because REFILL is never used on blocks - Tali is able to evaluate the
        ; entire block as a 1024 byte string.
        ; """"

xt_refill:
w_refill:
                ; Get input source from SOURCE-ID. This is an
                ; optimized version of a subroutine jump to SOURCE-ID
                lda insrc               ; cheat: We only check LSB
                bne _src_not_kbd

                ; SOURCE-ID of zero means we're getting stuff from the keyboard
                ; with ACCEPT, which wants the address of the current input
                ; buffer NOS and the max number of characters to accept TOS
                dex
                dex
                dex
                dex

                lda cib                 ; address of CIB is NOS
                sta 2,x
                lda cib+1
                sta 3,x

                stz ciblen              ; go in with empty buffer
                stz ciblen+1

                lda #bsize              ; max number of chars is TOS
                sta 0,x
                stz 1,x                 ; cheat: We only accept max 255

                jsr w_accept           ; ( addr n1 -- n2)

                ; ACCEPT returns the number of characters accepted, which
                ; belong in CIBLEN
                lda 0,x
                sta ciblen
                lda 1,x
                sta ciblen+1            ; though we only accept 255 chars

                ; make >IN point to beginning of buffer
                stz toin
                stz toin+1

                lda #$FF                ; overwrite with TRUE flag
                sta 0,x
                sta 1,x

                bra _done

_src_not_kbd:
                ; If SOURCE-ID doesn't return a zero, it must be a string in
                ; memory or a file (remember, no blocks in this version).
                ; If source is a string, we were given the flag -1 ($FFFF)
                ina
                bne _src_not_string

                ; Simply return FALSE flag as per specification
                dex
                dex
                stz 0,x
                stz 1,x

                bra z_refill

_src_not_string:
                ; Since we don't have blocks, this must mean that we are trying
                ; to read from a file. However, we don't have files yet, so we
                ; report an error and jump to ABORT.
                lda #err_badsource
                jmp error
_done:
z_refill:       rts



; ## SOURCE ( -- addr u ) "Return location and size of input buffer""
; ## "source"  auto  ANS core
        ; """https://forth-standard.org/standard/core/SOURCE"""
xt_source:
w_source:
                ; add address
                dex
                dex
                lda cib
                sta 0,x
                lda cib+1
                sta 1,x

                ; add size
                dex
                dex
                lda ciblen
                sta 0,x
                lda ciblen+1
                sta 1,x

z_source:       rts



; ## SOURCE_ID ( -- n ) "Return source identifier"
; ## "source-id"  tested  ANS core ext
        ; """https://forth-standard.org/standard/core/SOURCE-ID Identify the
        ; input source unless it is a block (s. Conklin & Rather p. 156). This
        ; will give the input source: 0 is keyboard, -1 ($FFFF) is character
        ; string, and a text file gives the fileid.
        ; """
xt_source_id:
w_source_id:
                dex
                dex

                lda insrc
                sta 0,x
                lda insrc+1
                sta 1,x

z_source_id:    rts



; ## TO_IN ( -- addr ) "Return address of the input pointer"
; ## ">in"  auto  ANS core
xt_to_in:
w_to_in:
                dex
                dex

                lda #<toin
                sta 0,x
                lda #>toin      ; paranoid, should be zero
                sta 1,x

z_to_in:        rts



; ## TO_NUMBER ( ud addr u -- ud addr u ) "Convert a number"
; ## ">number"  auto  ANS core
        ; """https://forth-standard.org/standard/core/toNUMBER
        ; Convert a string to a double number. Logic here is based on the
        ; routine by Phil Burk of the same name in pForth, see
        ; https://github.com/philburk/pforth/blob/master/fth/numberio.fth
        ; for the original Forth code. We arrive here from NUMBER which has
        ; made sure that we don't have to deal with a sign and we don't have
        ; to deal with a dot as a last character that signalizes double -
        ; this should be a pure number string.
        ;
        ; This routine calls UM*, which uses tmp1, tmp2 and tmp3, so we
        ; cannot access any of those.
        ;
        ; For the math routine, we move the inputs to the scratchpad to
        ; avoid having to fool around with the Data Stack.
        ;
        ;     +-----+-----+-----+-----+-----+-----+-----+-----+
        ;     |   UD-LO   |   UD-HI   |  N  : ZF  | UD-HI-LO  |
        ;     |           |           |     :     |           |
        ;     |  S    S+1 | S+2   S+3 | S+4 : S+5 | S+6   S+7 |
        ;     +-----+-----+-----+-----+-----+-----+-----+-----+
        ;
        ; The math routine works by converting one character to its
        ; numerical value (N) via DIGIT? and storing it in S+4 for
        ; the moment. We then multiply the UD-HI value with the radix
        ; (from BASE) using UM*, which returns a double-cell result. We
        ; discard the high cell of that result (UD-HI-HI) and store the
        ; low cell (UD-HI-LO) in S+6 for now. -- The second part is
        ; multiplying UD-LO with the radix. The high cell (UD-LO-HI)
        ; gets put in S+2, the low cell (HD-LO-LO) in S. We then use
        ; a version of D+ to add ( S S+2 ) and ( S+4 S+6) together,
        ; storing the result back in S and S+2, before we start another
        ; round with it as the new UD-LO and UD-HI.
        ; For the first several digits, UD-HI will be zero, so we
        ; save some time by tracking whether UD-HI is zero in S+5.
        ; """

xt_to_number:
                jsr underflow_4
w_to_number:
                ; Fill the scratchpad. We arrive with ( ud-lo ud-hi addr u ).
                ; After this step, the original ud-lo and ud-hi will still be on
                ; the Data Stack, but will be ignored and later overwritten
                ; If >NUMBER is called by NUMBER, these should be all zeros
                lda 6,x         ; ud-lo LSB
                sta scratch
                lda 7,x         ; ud-lo MSB
                sta scratch+1

                lda 4,x         ; ud-hi LSB
                sta scratch+2
                lda 5,x         ; ud-hi MSB
                sta scratch+3
                ora scratch+2
                sta scratch+5   ; flag to track ud-hi zero

                stz scratch+6   ; zero out ud-hi-lo in case we're skipping
                stz scratch+7

                dex             ; make space on the stack
                dex
                dex
                dex

_loop:
                ; Fetch one character from current address
                lda (6,x)
                jsr ascii_to_digit
                bcs _done       ; bad digit

                ; Conversion was successful. We arrive here with
                ; ( ud-lo ud-hi addr u ? ? ) and can start the math routine

                ; Save the digit, n.  Note the MSB is always zero
                sta scratch+4

                lda scratch+5   ; if UD-HI is still zero...
                beq _skip       ; ... we can skip the first step here

                ; Now multiply ud-hi (the one in the scratchpad, not the
                ; original one on the Data Stack) with the radix from BASE.
                ; We can clobber TOS and NOS because we saved n
                ; The multiply is faster with the smaller base on the left (NOS)
                lda scratch+2
                sta 0,x         ; TOS
                lda scratch+3
                sta 1,x

                lda base
                sta 2,x         ; NOS
                stz 3,x         ; ( ud-lo ud-hi addr u base ud-hi )

                ; UM* returns a double-celled number
                jsr w_um_star   ; ( ud-lo ud-hi addr u ud-hi-lo ud-hi-hi )

                ; Move ud-hi-lo to safety
                lda 2,x         ; ud-hi-lo
                sta scratch+6
                lda 3,x
                sta scratch+7

_skip:
                ; Now we multiply ud-lo, overwriting NOS, TOS
                ; Again put the smaller base on the left (NOS)
                lda scratch
                sta 0,x
                lda scratch+1
                sta 1,x         ; ( ud-lo ud-hi addr u ? ud-lo )

                lda base
                sta 2,x
                stz 3,x         ; ( ud-lo ud-hi addr u base ud-lo )

                jsr w_um_star   ; ( ud-lo ud-hi addr u ud-lo-lo ud-lo-hi )

                ; We add ud-lo and n, as well as ud-hi and ud-hi-lo,
                ; both in the scratch pad
                clc
                lda 2,x         ; ud-lo LSB
                adc scratch+4   ; n LSB
                sta scratch     ; this is the new ud-lo
                lda 3,x         ; ud-lo MSB
                adc #0          ; MSB of digit is 0
                sta scratch+1

                lda 0,x         ; ud-hi LSB
                adc scratch+6
                sta scratch+2   ; this is the new ud-hi
                lda 1,x         ; MSB
                adc scratch+7
                sta scratch+3

                ora scratch+2
                ora scratch+5
                sta scratch+5   ; update our ud-hi zero flag

                ; One character down. Increment address
                inc 6,x
                bne +
                inc 7,x
+
                ; Decrease counter (< 256)
                dec 4,x
                bne _loop

_done:
                ; Counter has reached zero or we have an error. In both
                ; cases, we clean up the Data Stack and return. Regular end is
                ; ( ud-lo ud-hi addr u ud-lo )
                inx
                inx
                inx
                inx

                ; ( ud-lo ud-hi addr u )

                ; The new ud-lo and ud-hi are still on the scratch pad
                lda scratch     ; new ud-lo
                sta 6,x
                lda scratch+1
                sta 7,x

                lda scratch+2
                sta 4,x
                lda scratch+3
                sta 5,x

z_to_number:    rts



; ## WORD ( char "name " -- caddr ) "Parse input stream"
; ## "word"  auto  ANS core
        ; """https://forth-standard.org/standard/core/WORD
        ; Obsolete parsing word included for backwards compatibility only.
        ; Do not use this, use `PARSE` or `PARSE-NAME`. Skips leading delimiters
        ; and copies word to storage area for a maximum size of 255 bytes.
        ; Returns the result as a counted string (requires COUNT to convert
        ; to modern format), and inserts a space after the string. See "Forth
        ; Programmer's Handbook" 3rd edition p. 159 and
        ; http://www.forth200x.org/documents/html/rationale.html#rat:core:PARSE
        ; for discussions of why you shouldn't be using WORD anymore.
        ;
        ; Forth
        ; would be   PARSE DUP BUFFER1 C! OUTPUT 1+ SWAP MOVE BUFFER1
        ; We only allow input of 255 chars. Seriously, use PARSE-NAME.
        ; """

xt_word:
                jsr underflow_1
w_word:
                ; Skip over leading delimiters - this is like PARSE-NAME,
                ; but unlike PARSE
                ldy toin                ; >IN
_loop:
                cpy ciblen              ; quit if end of input
                beq _found_char
                lda (cib),y
                cmp 0,x                 ; ASCII of delimiter
                bne _found_char

                iny
                bra _loop
_found_char:
                ; Save index of where word starts
                sty toin

                ; The real work is done by parse
                jsr w_parse            ; Returns ( addr u )

                ; Convert the modern ( addr u ) string format to obsolete
                ; ( caddr ) format. We just do this in the Dictionary
                lda 0,x
                sta (cp)                ; Save length of string
                pha                     ; Keep copy of length for later

                jsr w_dup              ; ( addr u u )
                lda cp
                clc
                adc #1
                sta 2,x                 ; LSB of CP
                lda cp+1
                adc #0
                sta 3,x                 ; ( addr cp+1 u )

                jsr w_move

                ; Return caddr
                dex
                dex
                lda cp
                sta 0,x
                lda cp+1
                sta 1,x

                ; Adjust CP
                pla                     ; length of string
                clc
                adc cp
                sta cp
                bcc z_word
                inc cp+1
z_word:         rts



; END
