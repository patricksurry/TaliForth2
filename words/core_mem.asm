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
