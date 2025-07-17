; Support for inline data payloads.
;
; We often need to put constant (literal) data values onto TaliForth's data stack.
; When small is more important than fast these inline words let us write code like:
;
;               jsr push_inline_literal         ; puts 1234 on the data stack ...
;               .word 1234
;               jsr w_plus                      ; ... continues past the payload
;
; These are used extensively in implementing forth words where we don't care
; so much about speed (e.g. user interaction, compilation).
; They're also used to compile Forth literals into compact non-native assembly.
;
; All of the entrypoints share the flexible push_inline_pictured routine.
; The accumulator contains a bit pattern that pictures how to map payload byte(s)
; following the original JSR onto new data stack entries.  The six most significant bits
; are read left to right with a one bit copying a payload byte and a zero byte
; inserting a zero byte into the corresponding stack slot.  The two least
; significant bits count how many stack entries are mapped in the high bits.
; For example a pattern like 1011xx10 would add two stack entries, and
; read the four high bits, consuming three payload bytes.  The first byte becomes
; the LSB of TOS (with MSB 0), the next two bytes form NOS.
; Normally the routine returns to the instruction following the payload bytes,
; but can be called indirectly where the caller is responsible for providing the
; payload address, and the routine then returns to the caller.
; Postprocessing can also be performed after the stack entries are created.
;
;                        msb                  lsb
;                    A =   p   p   p   p   p   p   n   n
;                        |  TOS  |  NOS  |  3OS  |  0-3  |
;                        | lo hi | lo hi | lo hi |
;               JSR pushp  .word [ .word [ .word ]]   ; call with inline parameters
;               LDA ...                               ; continues after parameters

cmpl_call_inline_literal:
        ; Generate code that calls the literal
        ;
        ;       jsr cmpl_call_inline_literal    ; compile "JSR target"
        ;       .word target

                ldy #1                          ; post-processing routine
                lda #%11_0000_01
                bra push_inline_pictured_pp

push_inline_literal:
        ; Put a word literal on the stack.  Supported by DISASM
        ;
        ;       jsr push_inline_literal
        ;       .word TOS

                lda #%11_0000_01
                bra push_inline_pictured

push_inline_sliteral:
        ; Put a string literal on the stack.  Supported by DISASM
        ; Note the length and actual text follow the JSR.
        ; The values ( addr length ) are pushed to the data stack
        ; with addr pointing to the literal "<text>"
        ;
        ;       jsr push_inline_sliteral
        ;       .word <length>
        ;       .text "<text>"

                lda #%1100_11_10
                bra push_inline_pictured

push_inline_2literal:
        ; Put a double-word literal on the stack.  Supported by DISASM
        ; The in-memory byte order matches the stack order, i.e. bytes N,U,X,I
        ; following the JSR become the Forth double word UNIX with UN TOS and IX NOS
        ; each stored in little endian order, i.e. N,U, I,X
        ;
        ;       jsr push_inline_2literal
        ;       .word TOS
        ;       .word NOS

                lda #%1111_00_10
                bra push_inline_pictured

push_inline_3literal:
        ; Only used internally, add three words to the stack
        ;
        ;       jsr push_inline_2literal
        ;       .word TOS
        ;       .word NOS
        ;       .word 3OS

                lda #%111111_11
                bra push_inline_pictured

push_inline_addru_literal:
        ; In word implementations we often need to stack (addr u) with u < 256
        ; This saves a byte vs push_inline_2literal.
        ; Not used in generated code so not supported by DISASM
        ;
        ;       jsr push_inline_addru_literal   ; stack (addr u)
        ;       .byte u         ; TOS
        ;       .word addr      ; NOS

                lda #%1011_00_10
                bra push_inline_pictured

push_inline_bliteral:
        ; Put a byte literal on the stack with MSB=0.  Supported by DISASM
        ;       jsr push_inline_bliteral
        ;       .byte TOS

                lda #%10_0000_01

push_inline_pictured:
        ; Note The alternate push_inline_pictured_pp entry point supports
        ; various payload post-processing actions as well as indirect payloads.
        ; This is controlled by the Y register.  Setting the sign bit of Y (bit 7)
        ; means the caller is responsible for pointing tmp1 one byte before the payload,
        ; and that we should just return to caller rather than following the payload.
        ; The lower seven bits of Y select a 1-indexed post-processing routine
        ; from the literal_postprocessors table.

                ldy #0
push_inline_pictured_pp:
                sta tmptos      ; masked picture
                and #3          ; count of new stack entries
                asl             ; *2 to get length of picture bits
                sta tmptos+1    ; temporarily write so we can add to DS
                txa
                sec
                sbc tmptos+1    ; extend data stack by a byte for each picture bit
                tax             ; update data stack pointer

                tya             ; check sign bit of Y arg (1 means indirect)
                bmi +           ; if indirect, caller already set up tmp1/+1

                pla             ; LSB of return address
                sta tmp1
                pla             ; MSB
                sta tmp1+1
+
                lda tmptos+1    ; fetch the picture length
                sty tmptos+1    ; overwrite with postprocessing info
                tay

                phx             ; save stack pointer
                dex
_loop:
                dey
                bmi _done
                inx

                asl tmptos      ; fetch next picture bit to carry
                bcs _copy       ; a one bit means copy a value
                stz 0,x         ; otherwise add a zero byte
                bra _loop
_copy:
                inc tmp1        ; inc data pointer to next param byte
                bne +
                inc tmp1+1
+
                lda (tmp1)      ; copy a payload byte to the stack
                sta 0,x
                bra _loop

_done:
                ; after the loop tmp1 points to the last parameter byte
                plx             ; restore the stack pointer

                lda tmptos
                cmp #%1110_0000
                bne _postprocess

                ; sliteral is special since NOS needs to point at string
                ; and then we need to advance past the string itself

                ; currently tmp1 points one byte before the string
                ; so put tmp1+1 into NOS
                ldy tmp1+1
                lda tmp1
                inc a
                bne +
                iny
        +
                sta 2,x
                sty 3,x

                ; advance past string data by adding string length in TOS
                clc
                lda tmp1
                adc 0,x
                sta tmp1
                lda tmp1+1
                adc 1,x
                sta tmp1+1

_postprocess:
                lda tmptos+1    ; any post-processing to do?
                bmi _indirect   ; indirect version returns to caller

                ldy tmp1+1      ; normally we'll return past the payload
                phy
                ldy tmp1
                phy
_indirect:

                asl             ; shift out indirect? flag while multiplying by 2
                beq +           ; no post-processing

                ; set up stack to RTS into post-processing routine
                ; which will then return to caller or indirect as needd
                tay
                lda literal_postprocessors-1,y
                pha
                lda literal_postprocessors-2,y
                pha
+
                rts



literal_postprocessors:
        ; post-processing handlers
        ; if data stack access is needed, routine should PLX and jump to pp_done
        ; otherwise return to pp_done_plx
                .word cmpl_call_tos-1      ; index 1



; TODO rework CREATE + PFA
; don't check for zero in literal (comment why not)

; set C=1 for indirect (where to store?)
; set Y=MSB for post-processing, with LSB in tmptos+1
