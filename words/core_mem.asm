; Core Forth words for memory and string handling

; Tali Forth 2 for the 65c02
; Scot W. Stevenson <scot.stevenson@gmail.com>
; Sam Colwell
; Patrick Surry
; First version: 19. Jan 2014
; This version: 21. Apr 2024


; ## ALIGN ( -- ) "Make sure CP is aligned on word size"
; ## "align"  auto  ANS core
        ; """https://forth-standard.org/standard/core/ALIGN
        ; On a 8-bit machine, this does nothing. ALIGNED uses this
        ; routine as well, and also does nothing
        ; """



; ## ALIGNED ( addr -- addr ) "Return the first aligned address"
; ## "aligned"  auto  ANS core
        ; """https://forth-standard.org/standard/core/ALIGNED"""
xt_align:
xt_aligned:
w_align:
w_aligned:
z_align:
z_aligned:
                rts             ; stripped out during native compile



; ## ALLOT ( n -- ) "Reserve or release memory"
; ## "allot"  auto  ANS core
        ; """https://forth-standard.org/standard/core/ALLOT
        ; Reserve a certain number of bytes (not cells) or release them.
        ; If n = 0, do nothing. If n is negative, release n bytes, but only
        ; to the beginning of the Dictionary. If n is positive (the most
        ; common case), reserve n bytes, but not past the end of the
        ; Dictionary. See http://forth-standard.org/standard/core/ALLOT
        ; """
xt_allot:
                jsr underflow_1
w_allot:
                ; Releasing memory is going to be a very rare operation,
                ; so we check for it at the beginning and try to make
                ; the most common case as fast as possible
                lda 1,x
                bmi _release

                ; Common case: We are reserving memory, not releasing it
                clc
                lda cp
                adc 0,x
                sta cp

                lda cp+1
                adc 1,x
                sta cp+1

                ; Wait, did we just grant more space than we have? This is
                ; a check we only do here, not for other situations like cmpl_a
                ; where smaller amounts are reserved.
                ldy #<cp_end
                cpy cp
                lda #>cp_end
                sbc cp+1
                bcs _done               ; we're fine.

                ; Oops, that was too much, we're beyond the end of
                ; legal Dictionary RAM. Reduce to max memory and report
                ; an error
                sty cp                  ; still #<cp_end
                lda #>cp_end
                sta cp+1

                lda #err_allot
                jmp error

_release:
   		; The ANS standard doesn't really say what to do if too much
                ; memory is freed ("negatively alloted"). In fact, there isn't
                ; even an official test. Gforth is little help either. The good
                ; news is, this is going to be a rare case. We want to use as
                ; few bytes as possible.

                ; What we do is let the user free anything up to the beginning
                ; of the RAM area assigned to the Dicionary (CP0), but at
                ; their own risk. This means that the Dictionary pointer DP
                ; might end up pointing to garbage. However, an attempt to
                ; free more than RAM than CP0 will lead to CP being set to CP0,
                ; the DP pointing to the last word in RAM (should be DROP) and
                ; an error message.

                ; We arrive here with ( n ) which is negative. First step,
                ; subtract the number TOS from the CP for a new CP
                dex
                dex
                lda cp
                sta 0,x
                lda cp+1
                sta 1,x

                jsr w_plus                     ; new CP is now TOS

                ; Second step, see if we've gone too far. We compare the new
                ; CP on TOS (which, if we've really screwed up, might be
                ; negative) with CP0. This is a signed comparison
                dex
                dex                             ; new CP now NOS
                lda #<cp0
                sta 0,x
                lda #>cp0
                sta 1,x                         ; CP0 is TOS

                jsr compare_16bit               ; still ( CP CP0 )

                ; If CP (NOS) is smaller than CP0 (TOS), we're in trouble.
                ; This means we want Z=1 or N=1
                beq _nega_done
                bmi _nega_done

                ; Yep, we're in trouble. Set CP to CP0, set DP to the first
                ; word in ROM (should be DROP), and abort with an error
                lda #<cp0
                sta cp
                lda #>cp0
                sta cp+1

                lda #<dictionary_start
                sta dp
                lda #>dictionary_start
                sta dp+1

                lda #err_negallot
                jmp error

_nega_done:
                ; Save new CP, which is NOS
                lda 2,x
                sta cp
                lda 3,x
                sta cp+1

                inx
                inx                     ; drop through to _done
_done:
                inx
                inx
z_allot:
                rts



; ## C_COMMA ( c -- ) "Store one byte/char in the Dictionary"
; ## "c,"  auto  ANS core
        ; """https://forth-standard.org/standard/core/CComma"""
xt_c_comma:
                jsr underflow_1
w_c_comma:
                lda 0,x
                jsr cmpl_a

                inx
                inx

z_c_comma:      rts



; ## C_FETCH ( addr -- c ) "Get a character/byte from given address"
; ## "c@"  auto  ANS core
        ; """https://forth-standard.org/standard/core/CFetch"""
xt_c_fetch:
                jsr underflow_1
w_c_fetch:
                lda (0,x)
                sta 0,x
                stz 1,x         ; Ignore LSB

z_c_fetch:      rts



; ## C_STORE ( c addr -- ) "Store character at address given"
; ## "c!"  auto  ANS core
        ; """https://forth-standard.org/standard/core/CStore"""
xt_c_store:
                jsr underflow_2
w_c_store:
                lda 2,x
                sta (0,x)

                inx
                inx
                inx
                inx

z_c_store:      rts



; ## CELL_PLUS ( u -- u ) "Add cell size in bytes"
; ## "cell+"  auto  ANS core
        ; """https://forth-standard.org/standard/core/CELLPlus
        ; Add the number of bytes ("address units") that one cell needs.
        ; Since this is an 8 bit machine with 16 bit cells, we add two bytes.
        ; """
xt_cell_plus:
                jsr underflow_1
w_cell_plus:
                inc 0,x
                bne +
                inc 1,x
+
                inc 0,x
                bne _done
                inc 1,x
_done:
z_cell_plus:    rts



; ## CELLS ( u -- u ) "Convert cells to size in bytes"
; ## "cells"  auto  ANS core
        ; """https://forth-standard.org/standard/core/CELLS
        ;
        ; Dummy entry for the CELLS word, the code is the same as for
        ; 2*, which is where the header directs us to
        ; """



; ## CHAR_PLUS ( addr -- addr+1 ) "Add the size of a character unit to address"
; ## "char+"  auto  ANS core
        ; """https://forth-standard.org/standard/core/CHARPlus
        ;
        ; This is a dummy entry, the code is shared with ONE_PLUS
        ; """



; ## CHARS ( n -- n ) "Number of bytes that n chars need"
; ## "chars"  auto  ANS core
        ; """https://forth-standard.org/standard/core/CHARS
        ; Return how many address units n chars are. Since this is an 8 bit
        ; machine, this does absolutely nothing and is included for
        ; compatibility with other Forth versions
        ; """
xt_chars:
                ; Checking for underflow seems a bit stupid because this
                ; routine does nothing on this machine. However, the user
                ; should be warned that there is something wrong with the
                ; code if this occurs.
                jsr underflow_1
w_chars:
z_chars:        rts



; ## COMMA ( n -- ) "Allot and store one cell in memory"
; ## ","  auto  ANS core
        ; """https://forth-standard.org/standard/core/Comma
        ; Store TOS at current place in memory.
        ;
        ; Since this an eight-bit machine, we can ignore all alignment issues.
        ; """
xt_comma:
                jsr underflow_1
w_comma:
                ldy #2
_twice:         lda 0,x
                jsr cmpl_a
                inx
                dey
                bne _twice

z_comma:        rts



; ## BLANK ( addr u -- ) "Fill memory region with spaces"
; ## "blank"  auto  ANS string
        ; """https://forth-standard.org/standard/string/BLANK"""
xt_blank:
                jsr underflow_2
w_blank:
                dex
                dex
                lda #AscSP
                sta 0,x
                stz 1,x
                bra w_fill     ; skip over code for ERASE



; ## ERASE ( addr u -- ) "Fill memory region with zeros"
; ## "erase"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/ERASE
        ; Note that ERASE works with "address" units
        ; (bytes), not cells.
        ; """

xt_erase:
                jsr underflow_2
w_erase:
                dex
                dex
                stz 0,x
                stz 1,x

                ; fall through to FILL
                bra w_fill

; ## FILL ( addr u char -- ) "Fill a memory region with a character"
; ## "fill"  auto  ANS core
        ; """https://forth-standard.org/standard/core/FILL
        ; Fill u bytes of memory with char starting at addr. Note that
        ; this works on bytes, not on cells. On an 8-bit machine such as the
        ; 65c02, this is a serious pain in the rear. It is not defined what
        ; happens when we reach the end of the address space
        ; """
xt_fill:
                jsr underflow_3
w_fill:
                ; We use tmp1 to hold the address
                lda 4,x         ; LSB
                sta tmp1
                lda 5,x
                sta tmp1+1

                ; We use tmp2 to hold the counter
                lda 2,x
                sta tmp2
                lda 3,x
                sta tmp2+1

                ; We use Y to hold the character
                lda 0,x
                tay
_loop:
                ; Unfortunately, we also need to make sure that we don't
                ; write further than the end of the RAM. So RAM_END must
                ; be larger or equal to the current address
                lda #>ram_end           ; MSB
                cmp tmp1+1
                bcc _done               ; RAM_END < TMP1, so leave
                bne _check_counter      ; RAM_END is not smaller and not equal

                lda #<ram_end           ; LSB, because MSBs were equal
                cmp tmp1
                bcc _done               ; RAM_END < TMP1, so leave

_check_counter:
                ; See if our counter has reached zero
                lda tmp2
                ora tmp2+1
                beq _done

                ; We're not in ROM and we still have stuff on the counter, so
                ; let's actually do what we came here to do
                tya
                sta (tmp1)

                ; Adjust the counter
                lda tmp2
                bne +
                dec tmp2+1
+               dec tmp2

                ; Next address
                inc tmp1
                bne _loop
                inc tmp1+1

                bra _loop

_done:
                ; Drop three cells off the Data Stack. This uses one byte
                ; less than six times INX
                txa
                clc
                adc #6
                tax
z_blank:
z_erase:
z_fill:         rts



; ## FETCH ( addr -- n ) "Push cell content from memory to stack"
; ## "@"  auto  ANS core
        ; """https://forth-standard.org/standard/core/Fetch"""
xt_fetch:
                jsr underflow_1
w_fetch:
                lda (0,x)               ; LSB
                tay
                inc 0,x
                bne +
                inc 1,x
+
                lda (0,x)               ; MSB
                sta 1,x
                sty 0,x

z_fetch:        rts



; ## HERE ( -- addr ) "Put Compiler Pointer on Data Stack"
; ## "here"  auto  ANS core
        ; """https://forth-standard.org/standard/core/HERE
        ; This code is also used by the assembler directive ARROW
        ; ("->") though as immediate
        ; and by HERE as an immediate compile word"""
xt_here:
xt_begin:
xt_asm_arrow:
w_here:
w_begin:
w_asm_arrow:
                dex
                dex
                lda cp
                sta 0,x
                lda cp+1
                sta 1,x

z_here:
z_begin:
z_asm_arrow:
                rts



; ## MARKER ( "name" -- ) "Create a deletion boundary"
; ## "marker"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/MARKER
        ; This word replaces FORGET in earlier Forths. Old entries are not
        ; actually deleted, but merely overwritten by restoring CP and DP.
        ; Run the named word at a later time to restore all of the wordlists
        ; to their state when the word was created with marker.  Any words
        ; created after the marker (including the marker) will be forgotten.
        ;
        ; To do this, we want to end up with a run-time component
        ; that reverts to the original CP, DP  and wordlist state:
        ;
        ;       jsr marker_runtime
        ;       <Original CP MSB>
        ;       <Original CP LSB>
        ;       <Original DP MSB> ( for CURRENT wordlist )
        ;       <Original DP LSB>
        ;       ; USER variables with wordlist state:
        ;       <CURRENT> (byte variable)
        ;       <All wordlists> (currently 12) (cell array)
        ;       <#ORDER> (byte variable)
        ;       <All search order> (currently 9) (byte array)
        ;
        ; This code uses tmp1 and tmp2
        ; """

xt_marker:
w_marker:
                ; Before we do anything, we need to save CP, which
                ; after all is the whole point of this operation. CREATE
                ; uses tmp1 and tmp2, so we take the speed hit and push stuff
                ; to the stack
                jsr current_to_dp

                lda dp
                pha
                lda dp+1
                pha

                lda cp
                pha
                lda cp+1
                pha

                ; we want CREATE but with marker_runtime as the CFA
                lda #4 + marker_end_offset - marker_start_offset
                sta tmpdsp              ; PFA size in bytes
                lda #<marker_runtime
                ldy #>marker_runtime
                jsr create_common

                ; Write the payload bytes

                ; Add original CP
                ply                     ; MSB
                pla                     ; LSB
                jsr cmpl_word

                ; Add original DP
                ply                     ; MSB
                pla                     ; LSB
                jsr cmpl_word

                ; Add the user variables for the wordlists and search order.
                ; We're compiling them in byte order.
                ldy #marker_start_offset
-
                lda (up),y
                jsr cmpl_a
                iny
                cpy #marker_end_offset
                bne -

z_marker:       rts


marker_runtime:
        ; """Restore Dictionary and memory (DP and CP) along with other
        ; user state to where they were when marker was defined.
        ; This is called as a CFA followed by the payload data in the PFA, so
        ; the return address when we arrive here points to PFA-1
        ; """

                ; Get the address of the payload off the stack,
                ; increasing by one because of the RTS mechanics
                pla
                sta tmp1        ; LSB of address
                pla
                sta tmp1+1      ; MSB of address

                ldy #1          ; start at 1 due to RTS mechanics

                ; CP was stored first
                lda (tmp1),y
                sta cp
                iny
                lda (tmp1),y
                sta cp+1

                ; Next was DP
                iny
                lda (tmp1),y
                sta dp
                iny
                lda (tmp1),y
                sta dp+1

                ; We've consumed the first four bytes and now come the user vars.
                ; It's slightly tricky since we can only index indirectly with y
                ; so we'd like to copy from (tmp1),y to (up),y
                ; But currently tmp1 + 5 corresponds to up + marker_start_offset
                ; So we'll adjust tmp1 so that tmp1' + marker_start_offset == tmp1 + 5
                ; meaning that tmp1' = tmp1 - (marker_start_offset - 5).  Phew.
        .cerror marker_start_offset < 5, "MARKER assumes marker_start_offset >= 5"
                sec
                lda tmp1
                sbc #marker_start_offset - 5
                sta tmp1
                bcs +
                dec tmp1+1
+
                ; Restore previous wordlist state
                ldy #marker_start_offset
-
                ; Copy from the dictionary back on top of the wordlists
                ; and search order.
                lda (tmp1),y
                sta (up),y
                iny
                cpy #marker_end_offset
                bne -

                jsr dp_to_current       ; Move the CURRENT DP back.

                ; The return instruction takes us back to the original caller
                rts



; ## MOVE ( addr1 addr2 u -- ) "Copy bytes"
; ## "move"  auto  ANS core
        ; """https://forth-standard.org/standard/core/MOVE
        ; Copy u "address units" from addr1 to addr2. Since our address
        ; units are bytes, this is just a front-end for CMOVE and CMOVE>. This
        ; is actually the only one of these three words that is in the CORE
        ; set.
        ;
        ; This word must not be natively compiled.
        ; """

xt_move:
                jsr underflow_3
w_move:
                ; compare MSB first
                lda 3,x                 ; MSB of addr2
                cmp 5,x                 ; MSB of addr1
                beq _lsb                ; wasn't helpful, move to LSB
                bcs _to_move_up         ; we want CMOVE>

                jmp w_cmove            ; JSR/RTS

_lsb:
                ; MSB were equal, so do the whole thing over with LSB
                lda 2,x                 ; LSB of addr2
                cmp 4,x                 ; LSB of addr1
                beq _equal              ; LSB is equal as well
                bcs _to_move_up         ; we want CMOVE>

                jmp w_cmove            ; JSR/RTS

_to_move_up:
                jmp w_cmove_up         ; JSR/RTS
_equal:
                ; drop three entries from Data Stack
                txa
                clc
                adc #6
                tax

z_move:         rts



; ## PAD ( -- addr ) "Return address of user scratchpad"
; ## "pad"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/PAD
        ; Return address to a temporary area in free memory for user. Must
        ; be at least 84 bytes in size (says ANS). It is located relative to
        ; the compile area pointer (CP) and therefore varies in position.
        ; This area is reserved for the user and not used by the system
        ; """
xt_pad:
w_pad:
                dex
                dex

                lda cp
                clc
                adc #padoffset  ; assumes padoffset one byte in size
                sta 0,x

                lda cp+1
                adc #0          ; only need carry
                sta 1,x

z_pad:          rts



; ## S_BACKSLASH_QUOTE ( "string" -- )( -- addr u ) "Store string in memory"
; ## "s\""  auto  ANS core
        ; """https://forth-standard.org/standard/core/Seq
        ; Store address and length of string given, returning ( addr u ).
        ; ANS core claims this is compile-only, but the file set expands it
        ; to be interpreted, so it is a state-sensitive word, which in theory
        ; are evil. We follow general usage. This is just like S" except
        ; that it allows for some special escaped characters.
        ; """

xt_s_backslash_quote:
w_s_backslash_quote:
                ; tmp2 will be used to determine if we are handling
                ; escaped characters or not. In this case, we are,
                ; so set it to $FF (the upper byte will be used to
                ; determine if we just had a \ and the next character
                ; needs to be modifed as an escaped character).
                lda #$FF
                sta tmp2
                stz tmp2+1

                ; Now that the flag is set, jump into s_quote to process
                ; the string.
                jsr s_quote_start
z_s_backslash_quote:
                rts



; ## S_QUOTE ( "string" -- )( -- addr u ) "Store string in memory"
; ## "s""  auto  ANS core
        ; """https://forth-standard.org/standard/core/Sq
        ; Store address and length of string given, returning ( addr u ).
        ; ANS core claims this is compile-only, but the file set expands it
        ; to be interpreted, so it is a state-sensitive word, which in theory
        ; are evil. We follow general usage.
        ;
        ; Can also be realized as
        ;     : S" [CHAR] " PARSE POSTPONE SLITERAL ; IMMEDIATE
        ; but it is used so much we want it in code.
        ; """

xt_s_quote:
w_s_quote:
                ; tmp2 will be used to determine if we are handling
                ; escaped characters or not.  In this case, we are
                ; not, so set it to zero.  (cf S_BACKSLASH_QUOTE)
                stz tmp2
                stz tmp2+1

s_quote_start:
                ; S" has undefined interpretation semantics in the CORE word set, but
                ; the FILE wordset permits it provided "no standard words other than S"
                ; ... [should overwrite the] interpreted string"
                ; (see https://forth-standard.org/standard/file/Sq for the details).
                ; One approach would be to reserve a fixed buffer of at least 80
                ; bytes somewhere outside the dictionary, like we do for command history
                ; or the block buffer.  The alternative adopted here is to always
                ; allocate space for the string in the dictionary.  This means
                ; that every interactive use of S" allocates space you won't get back
                ; (without MARKER or the like) but has the big advantage that strings
                ; stay where you put them and don't get overwritten by other operations.

                ; We will save a bit of space when interpreting by writing the string
                ; literal directly HERE.  When we're compiling we'll use SLITERAL
                ; which needs a five byte prologue (jsr sliteral_runtime / .word length)
                ; so we'll leave space for that.

                lda state               ; check whether we're interpreting (0) or compiling (-1)
                ora state+1             ; paranoid

                pha                     ; save zero / nonzero for post-processing
                beq _interpreting       ; just write string directly

                ; we're compiling, so reserve just enough space for SLITERAL to later
                ; add the prologue before the string data

                clc
                lda cp
                adc #5                  ; reserve five bytes for the prologue (see below)
                sta cp
                bcc +
                inc cp+1
+
_interpreting:
                ; Now we'll compile the string bytes into the dictionary
                ; But first remember the address where we started

                jsr w_here              ; ( addr )

_savechars_loop:
                ; Start saving the string into the dictionary up to the
                ; ending double quote. First, check to see if the input
                ; buffer is empty.
                lda toin+1              ; MSB
                cmp ciblen+1
                bcc _input_fine         ; unsigned comparison

                lda toin                ; LSB
                cmp ciblen
                bcc _input_fine

                ; Input buffer is empty. Refill it. Refill calls accept,
                ; which uses tmp2 and tmp3. Save and restore them.
                lda tmp2
                pha
                lda tmp2+1
                pha
                lda tmp3    ; Only tmp3 used, so don't bother with tmp3+1
                pha

                jsr w_refill           ; ( -- f )

                pla
                sta tmp3
                pla
                sta tmp2+1
                pla
                sta tmp2

                ; Check result of refill.
                lda 0,x
                ora 1,x
                bne _refill_ok

                ; Something when wrong with refill.
                lda #err_refill
                jmp error

_refill_ok:
                ; Remove the refill flag from the data stack.
                inx
                inx

                ; For refill success, jump back up to the empty check, just in
                ; case refill gave us an empty buffer (eg. empty/blank line of
                ; input)
                bra _savechars_loop

_input_fine:
                ; There should be at least one valid char to use.
                ; Calculate it's address at CIB+TOIN into tmp1
                lda cib
                clc
                adc toin        ; LSB
                sta tmp1
                lda cib+1
                adc toin+1      ; MSB
                sta tmp1+1

                ; Get the character
                lda (tmp1)

                ; Check to see if we are handling escaped characters.
                bit tmp2
                bmi _handle_escapes    ; Only checking bit 7
                jmp _regular_char

_handle_escapes:
                ; We are handling escaped characters.  See if we have
                ; already seen the backslash.
                bit tmp2+1
                bmi _escaped
                jmp _not_escaped

_escaped:

                ; We have seen a backslash (previous character). Check to see if
                ; we are in the middle of a \x sequence (bit 6 of tmp2+1 will
                ; be clear in that case )
                bvs _check_esc_chars

                ; We are in the middle of a \x sequence. Check to see if we
                ; are on the first or second digit.
                lda #1
                bit tmp2+1
                bne _esc_x_second_digit

                ; First digit.
                inc tmp2+1  ; Adjust flag for second digit next time.
                lda (tmp1)  ; Get the char and stash it.
                pha
                jmp _next_character

_esc_x_second_digit:
                ; We are on the second hex digit of a \x sequence. Clear the
                ; escaped character flag (because we are handling it right
                ; here)
                stz tmp2+1
                lda (tmp1)
                ply                     ; recover first of pair
                jsr ascii_to_byte       ; TODO we're ignoring possible C=1 error

                bra _save_character

_check_esc_chars:
                ; Clear the escaped character flag (because we are
                ; handling it right here)
                stz tmp2+1

                ; is it character a-z ?
                cmp #'a'
                bmi _check_esc_quote
                cmp #'z'+1
                bpl _check_esc_quote
                ; check translation table
                tay
                lda escape_tr_table - 'a',y   ; fake base address to index with a-z directly
                bne _esc_replace
                tya                     ; revert if no translation
                bra _check_esc_quote

_esc_replace:   bpl _save_character     ; simple replacement
                ; handle specials with hi bit set (NUL and CR/LF)
                and #$7F                ; clear hi bit
                beq _save_character     ; NUL we can just output
                jsr cmpl_a              ; else output first char (CR)
                lda #10                 ; followed by LF
                bra _save_character

_check_esc_quote:
                cmp #'"'
                beq _save_character

                cmp #'x'
                bne _check_esc_backslash

                ; This one is difficult. We need to get the next TWO
                ; characters (which might require a refill in the middle)
                ; and combine them as two hex digits. We do this by
                ; clearing bit 6 of tmp2+1 to indicate we are in a digit
                ; and using bit 0 to keep track of which digit we are on.
                lda #%10111110        ; Clear bits 6 and 0
                sta tmp2+1
                bra _next_character

_check_esc_backslash:
                cmp #'\'
                bne _regular_char
                bra _save_character

_not_escaped:
                ; Check for the backslash to see if we should escape
                ; the next char.
                cmp #'\'
                bne _regular_char

                ; We found a backslash.  Don't save anyhing, but set
                ; a flag (in tmp2+1) to handle the next char. We don't
                ; try to get the next char here as it may require a
                ; refill of the input buffer.
                lda #$FF
                sta tmp2+1
                bra _next_character

_regular_char:
                ; Check if the current character is the end of the string.
                cmp #'"'
                beq _found_string_end

_save_character:
                ; If we didn't reach the end of the string, compile this
                ; character into the dictionary
                jsr cmpl_a

_next_character:
                ; Move on to the next character.
                inc toin
                bne _savechars_loop_longjump
                inc toin+1

_savechars_loop_longjump:
                jmp _savechars_loop

_found_string_end:
                ; Use up the delimiter.
                inc toin
                bne +
                inc toin+1
+
                ; Finally we've compiled all the string data into the dictionary
                ; We still have the start address and need the string length

                ; ( addr )
                jsr w_here
                jsr w_over
                jsr w_minus    ; HERE - addr gives string length
                ; ( addr u )

                ; What happens next depends on the state (which is bad, but
                ; that's the way it works at the moment). If we are
                ; interpreting (state=0), we're done because we've saved the string
                ; to a buffer.  (In fact we've over-delivered by compiling the string
                ; to permanent storage in the dictionary!)

                ; If we're compiling, we need to turn the string into an SLITERAL.
                ; We'll just rewind the CP to where it was when we started -
                ; five bytes before the string we've written - and let sliteral
                ; work its magic.  It'll write the five byte prologue and copy
                ; the string data onto itself (a no-op) while re-allocating the space.

                pla                     ; fetch the state flag (0 = interpret)
                beq _done

                sec                     ; rewind the CP to addr-5
                lda 2,x
                sbc #5
                sta cp
                lda 3,x
                sbc #0
                sta cp+1

                ; write the prologue, "copy" the string and reallocate the space
                jsr w_sliteral         ; ( addr u -- )

_done:
z_s_quote:      rts



escape_tr_table:
    ; 26 character translation for simple escapes
    ; 0 indicates no translation, hi bit indicates special
    .byte   7               ; a -> BEL (ASCII value 7)
    .byte   8               ; b -> Backspace (ASCII value 8)
    .byte   0,0             ; c, d no escape
    .byte   27              ; e -> ESC (ASCII value 27)
    .byte   12              ; f -> FF (ASCII value 12)
    .byte   0,0,0,0,0       ; g,h,i,j,k
    .byte   10              ; l -> LF (ASCII value 10)
    .byte   13+128          ; m -> CR/LF pair (ASCII values 13, 10)
    ; n has configurable behavior which we hard-code in the table
.if "cr" in TALI_OPTION_CR_EOL
.if "lf" in TALI_OPTION_CR_EOL
    .byte   13+128          ; n behaves like m --> cr/lf
.else
    .byte   13              ; n behaves like r --> cr
.endif
.else
    .byte   10              ; n behaves like l --> lf
.endif
    .byte   0,0             ; o,p
    .byte   34              ; q -> Double quote (ASCII value 34)
    .byte   13              ; r ->  CR (ASCII value 13)
    .byte   0               ; s
    .byte   9               ; t -> Horizontal TAB (ASCII value 9)
    .byte   0               ; u
    .byte   11              ; v -> Vertical TAB (ASCII value 11)
    .byte   0,0,0           ; w,x,y   (x is a special case later)
    .byte   0+128           ; z -> NULL (ASCII value 0)



; ## S_TO_D ( u -- d ) "Convert single cell number to double cell"
; ## "s>d"  auto  ANS core
        ; """https://forth-standard.org/standard/core/StoD"""

xt_s_to_d:
                jsr underflow_1
w_s_to_d:
                dex
                dex
                stz 0,x
                stz 1,x

                lda 3,x
                bpl _done

                ; negative, extend sign
                dec 0,x
                dec 1,x
_done:
z_s_to_d:       rts



; ## STORE ( n addr -- ) "Store TOS in memory"
; ## "!"  auto  ANS core
        ; """https://forth-standard.org/standard/core/Store"""
xt_store:
                jsr underflow_2
w_store:
                lda 2,x         ; LSB
                sta (0,x)

                inc 0,x
                bne +
                inc 1,x
+
                lda 3,x         ; MSB
                sta (0,x)

                inx             ; 2DROP
                inx
                inx
                inx

z_store:        rts



; ## TWO_STORE ( n1 n2 addr -- ) "Store two numbers at given address"
; ## "2!"  auto  ANS core
        ; """https://forth-standard.org/standard/core/TwoStore
        ; Stores so n2 goes to addr and n1 to the next consecutive cell.
        ; Is equivalent to  `SWAP OVER ! CELL+ !`
        ; """
xt_two_store:
                jsr underflow_3
w_two_store:
                lda 0,x
                sta tmp1
                ldy 1,x
                sty tmp1+1

                inx
                inx

                lda 0,x         ; copy MSB
                sta (tmp1)
                lda 1,x         ; copy next
                ldy #1
                sta (tmp1),y
                lda 2,x         ; copy next
                iny
                sta (tmp1),y
                lda 3,x         ; copy MSB
                iny
                sta (tmp1),y

                inx             ; 2DROP
                inx
                inx
                inx

z_two_store:    rts



; ## UNUSED ( -- u ) "Return size of space available to Dictionary"
; ## "unused"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/UNUSED
        ; UNUSED does not include the ACCEPT history buffers. Total RAM
        ; should be HERE + UNUSED + <history buffer size>, the last of which
        ; defaults to $400
        ; """
xt_unused:
w_unused:
                dex
                dex

                lda #<cp_end
                sec
                sbc cp
                sta 0,x

                lda #>cp_end
                sbc cp+1
                sta 1,x

z_unused:       rts



; END
