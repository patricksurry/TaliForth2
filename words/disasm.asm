; Disassembler for Tali Forth 2
; Scot W. Stevenson <scot.stevenson@gmail.com>
; Updated by Sam Colwell and Patrick Surry
; First version: 28. Apr 2018
; This version: 06. Apr 2024

; This is the default disassembler for Tali Forth 2. Use by passing
; the address and length of the block of memory to be disassembled:
;
;       disasm ( addr x -- )

; The underflow checking is handled by the word's stub in native_words.asm, see
; there for more information.

; The code is disassembled in Simpler Assembler Notation (SAN), because that
; is, uh, simpler. See the documentation and https://github.com/scotws/SAN for
; more information. Because disassemblers are used interactively with slow
; humans, we don't care that much about speed and put the emphasis at being
; small.

; Uses: tmp3, tmp2, tmp1 (xt_u_dot_r uses xt_type which uses tmp1)
;       scratch (used for handling literals and JSRs)


; ## DISASM ( addr u -- ) "Disassemble a block of memory"
; ## "disasm"  tested  Tali Forth
        ; """Convert a segment of memory to assembler output. This
        ; word is vectored so people can add their own disassembler.
        ; Natively, this produces Simpler Assembly Notation (SAN)
        ; code, see the section on The Disassembler in the manual and
        ; the file disassembler.asm for more details.
        ; """
xt_disasm:
                jsr underflow_2

                jsr disassembler

z_disasm:       rts



disassembler:
                jsr xt_cr       ; ( addr u )
_byte_loop:
                ; Print address at start of the line. Note we use whatever
                ; number base the user has
                jsr xt_over     ; ( addr u addr )
                jsr xt_u_dot    ; ( addr u )
                jsr xt_space

                ; We use the opcode value as the offset in the oc_index_table.
                ; We have 256 entries, each two bytes long, so we can't just
                ; use an index with Y. We use tmp2 for this.
                lda #<oc_index_table
                sta tmp2
                lda #>oc_index_table
                sta tmp2+1

                lda (2,x)       ; get opcode that addr points to
                sta scratch     ; Save opcode

                asl             ; multiply by two for offset
                bcc +
                inc tmp2+1      ; we're on second page
+
                tay             ; use Y as the index

                ; Get address of the entry in the opcode table. We put it
                ; in tmp3 and push a copy of it to the stack to be able to
                ; print the opcode later
                lda (tmp2),y    ; LSB
                sta tmp3
                pha

                iny

                lda (tmp2),y    ; MSB
                sta tmp3+1
                pha

                ; The first byte is the "lengths byte" which is coded so
                ; that bits 7 to 6 are the length of the instruction (1 to
                ; 3 bytes) and 2 to 0 are the length of the mnemonic.
                lda (tmp3)
                tay                     ; save copy of lengths byte

                ; Since this is Simpler Assembler Notation (SAN) in a Forth
                ; system, we want to print any operand before we print the
                ; mnemonic ('1000 sta' instead of 'sta 1000'). This allows us
                ; to copy and paste directly from the disassembler to the
                ; assembler.

                ; What happens next depends on the length of the instruction in
                ; bytes:

                ;   1 byte:  OPC          -->          OPC  bit sequence: %01
                ;   2 bytes: OPC LSB      -->    0 LSB OPC  bit sequence: %10
                ;   3 bytes: OPC LSB MSB  -->  MSB LSB OPC  bit sequence: %11

                ; We can distinguish between the first case, where there is
                ; only the mnemonic, and the second and third cases, where we
                ; have an operand. We do this by use of the bit sequence in
                ; bits 7 and 6.
                bpl _no_operand         ; bit 7 clear, single-byte instruction

                ; We have an operand. Prepare the Data Stack
                jsr xt_zero             ; ( addr u 0 ) ZERO does not use Y

                ; Because of the glory of a little endian CPU, we can start
                ; with the next byte regardless if this is a one or two byte
                ; operand, because we'll need the LSB one way or the other.
                ; We have a copy of the opcode on the stack, so we can now move
                ; to the next byte
                inc 4,x
                bne +
                inc 5,x                 ; ( addr+1 u 0 )
+
                lda 2,x
                bne +
                dec 3,x
+
                dec 2,x                 ; ( addr+1 u-1 0 )

                lda (4,x)
                sta 0,x                 ; LSB of operand ( addr+1 u-1 LSB )
                sta scratch+1           ; Save a copy in the scratch buffer

                ; We still have a copy of the lengths byte in Y, which we use
                ; to see if we have a one-byte operand (and are done already)
                ; or a two-byte operand
                tya                     ; retrieve copy of lengths byte
                rol                     ; shift bit 6 to bit 7
                bpl _print_operand

                ; We have a three-byte instruction, so we need to get the MSB
                ; of the operand. Move to the next byte
                inc 4,x
                bne +
                inc 5,x                 ; ( addr+2 u-1 LSB )
+
                lda 2,x
                bne +
                dec 3,x
+
                dec 2,x                 ; ( addr+2 u-2 LSB )

                lda (4,x)
                sta 1,x                 ; MSB of operand ( addr+2 u-2 opr )
                sta scratch+2           ; Save a copy in the scratch buffer

                ; fall through to _print_operand

_print_operand:

                ; We arrive here with the lengths byte in Y, the address of the
                ; opcode table entry for the instruction on the stack ( addr+n
                ; u-n opr). We want the output to be nicely formatted in
                ; columns, so we use U.R. The maximal width of the number in
                ; decimal on an 16-bit addressed machine is five characters
                dex
                dex
                lda #5
                sta 0,x
                stz 1,x                 ; ( addr+n u-n opr 5 )

                jsr xt_u_dot_r          ; U.R ( addr+n u-n )

                bra _print_mnemonic

_no_operand:
                ; We arrive here with the opcode table address on the stack,
                ; the lengths byte in Y and ( addr u ). Since we want to have
                ; a nicely formatted output, we need to indent the mnemonic by
                ; five spaces.
                dex
                dex
                lda #5
                sta 0,x
                stz 1,x                 ; ( addr u 5 )

                jsr xt_spaces           ; ( addr u )

                ; fall through to _print_mnemonic

_print_mnemonic:
                ; We arrive here with the opcode table address on the stack and
                ; ( addr u | addr+n u-n ). Time to print the mnemonic.
                jsr xt_space

                dex
                dex                     ; ( addr u ? )
                pla                     ; MSB
                sta 1,x                 ; ( addr u MSB )
                pla                     ; LSB
                sta 0,x                 ; ( addr u addr-o )

                jsr xt_count            ; ( addr u addr-o u-o )

                ; The length of the mnemnonic string is in bits 2 to 0
                stz 1,x                 ; paranoid
                lda 0,x
                and #%00000111          ; ( addr u addr-o u-o )
                sta 0,x

                jsr xt_type             ; ( addr u )

                ; Handle JSR by printing name of function, if available.
                ; scratch has opcode ($20 for JSR)
                ; scratch+1 and scratch+2 have address if it's a JSR.
                lda scratch
                cmp #OpJSR
                bne _not_jsr

                ; It's a JSR.  Print 5 spaces as an offset.
                dex
                dex
                lda #5
                sta 0,x
                stz 1,x
                jsr xt_spaces

                ldy #(_end_handlers - _special_handlers - 4)
_check_handler: lda _special_handlers,y
                cmp scratch+1
                bne _next_handler
                lda _special_handlers+1,y
                cmp scratch+2
                beq _run_handler
_next_handler:  dey
                dey
                dey
                dey
                bpl _check_handler

_not_special:
                ; Try the generic JSR handler, which will use the target of the
                ; JSR as an XT and print the name if it exists.
                jsr disasm_jsr
                jmp _printing_done

_run_handler:
                lda _special_handlers+2,y
                sta scratch+3
                lda _special_handlers+3,y
                sta scratch+4
                jsr _dispatch_handler
                jmp _printing_done

_dispatch_handler:
                jmp (scratch+3)

; Special handlers
_special_handlers:
    .word literal_runtime,      disasm_literal
    .word sliteral_runtime,     disasm_sliteral
    .word ztest_runtime,        disasm_0test
    .word do_runtime,           disasm_do
_end_handlers:


_not_jsr:
                ; See if the instruction is a jump (instruction still in A)
                ; (Strings start with a jump over the data.)
                cmp #OpJMP
                bne _not_jmp

                ; We have a branch.  See if it's a string by looking for
                ; a JSR sliteral_runtime at the jump target address.
                ; The target address is in scratch+1 and scratch+2
                ; Use scratch+3 and scratch+4 here as we need to move
                ; the pointer.
                lda scratch+1   ; Copy the pointer.
                sta scratch+3
                lda scratch+2
                sta scratch+4

                ; Get the first byte at the jmp target address.
                lda (scratch+3)

                cmp #OpJSR ; check for JSR
                bne _printing_done
                ; Next byte
                inc scratch+3
                bne +
                inc scratch+4
+
                ; Check for string literal runtime
                lda (scratch+3)

                cmp #<sliteral_runtime
                bne _printing_done
                ; Next byte
                inc scratch+3
                bne +
                inc scratch+4
+
                lda (scratch+3)

                cmp #>sliteral_runtime
                bne _printing_done

                ; It's a string literal jump.
                jsr disasm_sliteral_jump
                jmp _printing_done

_not_jmp:
                ; is it a native branch instruction with one byte relative addressing?
                ; opcodes are bra: $80 and bxx: %xxx1 0000
                ; if so we'll display the branch target address

                ; destructive test on opcode in A
                cmp #OpBRA            ; is it bra?
                beq _is_rel
                and #$1F
                eor #$10            ; do bottom five bits match xxx10000 ?
                bne _printing_done
_is_rel:
                ; treat opr as signed byte and add to addr following operand: (addr+1) + 1
                ; scratch+1 contains the operand (offset), stack has (addr+1 u-1)
                ldy #'v'            ; we'll indicate branch forward or back with v or ^
                dex
                dex
                stz 1,x
                lda scratch+1
                sta 0,x
                bpl +
                dec 1,x             ; for negative offsets extend the sign bit so add works out
                ldy #'^'            ; it's a backward branch
+               sec                 ; start counting from address after opcode
                adc 4,x
                sta 0,x
                lda 1,x
                adc 5,x
                sta 1,x

                phy                 ; save the direction indicator

                dex
                dex
                lda #9
                sta 0,x
                stz 1,x
                jsr xt_u_dot_r      ; print the destination with 5 leading spaces

                lda #AscSp          ; print space and branch direction indicator
                jsr emit_a
                pla
                jsr emit_a

_printing_done:
                jsr xt_cr

                ; Housekeeping: Next byte
                inc 2,x
                bne +
                inc 3,x                 ; ( addr+1 u )
+
                jsr xt_one_minus        ; ( addr+1 u-1 )

                lda 0,x                 ; All done?
                ora 1,x
                beq _done

                lda 1,x                 ; Catch mid-instruction ranges
                bmi _done

                jmp _byte_loop          ; out of range for BRA
_done:
                ; Clean up and leave
                jmp xt_two_drop         ; JSR/RTS

; Handlers for various special disassembled instructions:
; String literal handler (both for inline strings and sliteral)
disasm_sliteral_jump:
                ; If we get here, we are at the jump for a constant string.
                ; Strings are compiled into the dictionary like so:
                ;           jmp a
                ;           <string data bytes>
                ;  a -->    jsr sliteral_runtime
                ;           <string address>
                ;           <string length>
                ;
                ; We have ( addr n ) on the stack where addr is the last
                ; byte of the address a in the above jmp instruction.
                ; Address a is in scratch+1 scratch+2.

                ; Determine the distance of the jump so we end on the byte
                ; just before the JSR (sets us up for SLITERAL on next loop)
                jsr xt_swap
                dex
                dex
                lda scratch+1
                sta 0,x
                lda scratch+2
                sta 1,x
                jsr xt_swap
                jsr xt_minus
                jsr xt_one_minus
                ; (n jump_distance)
                ; Subtract the jump distance from the bytes left.
                jsr xt_minus
                ; ( new_n )
                ; Move to one byte before the target address
                dex
                dex
                lda scratch+1
                sta 0,x
                lda scratch+2
                sta 1,x
                jsr xt_one_minus
                jsr xt_swap ; ( new_addr new_n )
                rts

; String literal handler
disasm_sliteral:
                lda #'S'
                jsr emit_a ; Print S before LITERAL so it becomes SLITERAL
                lda #str_disasm_lit     ; "LITERAL "
                jsr print_string_no_lf

                ; ( addr u ) address of last byte of JSR address and bytes left on the stack.
                ; We need to print the two values just after addr and move along two bytes
                ; for each value.
                jsr xt_swap             ; switch to (u addr)
                jsr xt_one_plus

                jsr xt_dup
                jsr xt_fetch
                jsr xt_u_dot            ; Print the address of the string
                ; Move along two bytes (already moved address one) to skip over the constant.
                jsr xt_two
                jsr xt_plus

                jsr xt_dup
                jsr xt_question         ; Print the length of the string
                ; Move along to the very last byte of the data.
                jsr xt_one_plus

                ; ( u addr+4 )
                ; Fix up the number of bytes left.
                jsr xt_swap            ; ( addr+4 u )
                dex
                dex
                lda #4
                sta 0,x
                stz 1,x
                jsr xt_minus            ; ( addr+4 u-4 )
                rts

disasm_0test:
                lda #str_disasm_0test
                jsr print_string_no_lf
                jmp emit_a

; DO handler
disasm_do:
                lda #'D'
                jsr emit_a
                lda #'O'
                jmp emit_a

; Literal handler
disasm_literal:
                lda #str_disasm_lit
                jsr print_string_no_lf ; "LITERAL "
disasm_print_literal:
                ; ( addr u ) address of last byte of JSR and bytes left on the stack.
                ; We need to print the value just after the address and move along two bytes.
                jsr xt_swap ; switch to (u addr)
                jsr xt_one_plus

                jsr xt_dup
                jsr xt_question ; Print the value at the address
                ; Move along two bytes (already moved address one) to skip over the constant.
                jsr xt_one_plus
                jsr xt_swap ; (addr+2 u)
                jsr xt_one_minus
                jsr xt_one_minus ; (addr+2 u-2)
                rts

; JSR handler
disasm_jsr:
                ; The address of the JSR is in scratch+1 and scratch+2.
                ; The current stack is already ( addr u ) where addr is the address of the last byte of
                ; the JSR target address, and we want to leave it like that so moving on to the next byte
                ; works properly.
                ; Put the target address on the stack and see if it's an XT.
                dex
                dex
                lda scratch+1
                sta 0,x
                lda scratch+2
                sta 1,x
                ; ( xt )
                jsr xt_int_to_name
                ; int>name returns zero if we just don't know.
                lda 0,x
                ora 1,x
                beq _disasm_no_nt
                ; We now have a name token ( nt ) on the stack.
                ; Change it into the name and print it.
                jsr xt_name_to_string
                jsr xt_type
                rts

_disasm_no_nt:
                jsr xt_drop ; the 0 indicating no name token
                ; See if the address is between underflow_1 and underflow_4,
                ; inclusive.
                dex
                dex
                lda scratch+1
                sta 0,x
                lda scratch+2
                sta 1,x
                ; ( jsr_address )
                ; Compare to lower underflow address
                dex
                dex
                lda #<underflow_1
                sta 0,x
                lda #>underflow_1
                sta 1,x
                jsr compare_16bit
                beq _disasm_jsr_uflow_check_upper
                bcs _disasm_jsr_unknown
_disasm_jsr_uflow_check_upper:
                ; Compare to upper underflow addresses
                lda #<underflow_4
                sta 0,x
                lda #>underflow_4
                sta 1,x
                jsr compare_16bit
                beq _disasm_jsr_soc
                bcc _disasm_jsr_unknown
_disasm_jsr_soc:
                ; It's an underflow check.
                lda #str_disasm_sdc
                jsr print_string_no_lf  ; "STACK DEPTH CHECK"
_disasm_jsr_unknown:
                jsr xt_two_drop
                rts


; used to calculate size of assembled disassembler code
disassembler_end:
