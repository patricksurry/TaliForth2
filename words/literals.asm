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
; All of the entrypoints share the flexible push_inline_pictured routine

cmpl_call_inline_literal:
        ; Generate code that calls the literal
        ;
        ;       jsr cmpl_call_inline_literal    ; compile "JSR target"
        ;       .word target

                lda #%00000111
                ldy #2
                bra push_inline_pictured_pp

push_inline_sliteral:
        ; Put a string literal on the stack.  Supported by DISASM
        ; Note the length and actual text follow the JSR.
        ; The values ( addr length ) are pushed to the data stack
        ; with addr pointing to the literal "<text>"
        ;
        ;       jsr push_inline_sliteral
        ;       .word <length>
        ;       .text "<text>"

                lda #%00010011
                ldy #1
                bra push_inline_pictured_pp

push_inline_2literal:
        ; Put a double-word literal on the stack.  Supported by DISASM
        ; The in-memory byte order matches the stack order, i.e. bytes N,U,X,I
        ; following the JSR become the Forth double word UNIX with UN TOS and IX NOS
        ; each stored in little endian order, i.e. N,U, I,X
        ;
        ;       jsr push_inline_2literal
        ;       .word TOS
        ;       .word NOS

                lda #%00011111
                bra push_inline_pictured

push_inline_3literal:
        ; Only used internally, add three words to the stack
        ;
        ;       jsr push_inline_2literal
        ;       .word TOS
        ;       .word NOS
        ;       .word 3OS

                lda #%01111111
                bra push_inline_pictured

push_inline_addru_literal:
        ; In word implementations we often need to stack (addr u) with u < 256
        ; This saves a byte vs push_inline_2literal.
        ; Not used in generated code so not supported by DISASM
        ;
        ;       jsr push_inline_addru_literal   ; stack (addr u)
        ;       .byte u         ; TOS
        ;       .word addr      ; NOS

                lda #%00011101
                bra push_inline_pictured

push_inline_literal:
        ; Put a word literal on the stack.  Supported by DISASM
        ;
        ;       jsr push_inline_literal
        ;       .word TOS

                lda #%00000111
                bra push_inline_pictured

push_inline_bliteral:
        ; Put a byte literal on the stack with MSB=0.  Supported by DISASM
        ;       jsr push_inline_bliteral
        ;       .byte TOS

                lda #%00000101

push_inline_pictured:
        ; Copies 1-4 bytes following the calling JSR to the data stack.
        ; The accumulator contains flags defining the mapping from bytes to stack values.
        ;
        ;       msb                  lsb
        ;        0  p  p  p  p  p  p  p
        ;
        ; Bits 0-6 contain a one-terminated stack picture (reading lsb to msb)
        ; which maps payload bytes to stack bytes.  Bit 7 must be 0 (unused).
        ; For one stack entry the picture is 00000/xx where / shows the 1 terminator
        ; and the two xx bits map the payload byte(s) to the data stack.
        ; A one bit copies a payload byte, a zero bit writes a zero.
        ; For two stack entries the picture is 000/xxxx where / is the 1 terminator.
        ; Parameter bytes are copied to the data stack in memory order, i.e. the
        ; first byte after the JSR will be nearest TOS (lowest in data stack memory).
        ;
        ; The alternate push_inline_pictured_pp entry point supports
        ; various payload post-processing actions as well as indirect payloads.
        ; This is controlled by the Y register.  Setting the sign bit of Y (bit 7)
        ; means the caller is responsible for pointing tmp1 to one byte before the payload,
        ; and that the routine will just return to caller not following the payload.
        ; The lower seven bits of Y select a 1-indexed post-processing routine
        ; from the literal_postprocessors table.
        ;
        ; Examples:
        ;       00000/11 consumes two parameter bytes as the LSB/MSB of one stack word
        ;       00000/01 consumes one parameter byte as the LSB of TOS, with MSB=0
        ;       000/0101 consumes two single bytes as the LSB of two words on the data stack
        ;       000/0011 creates an empty NOS and consumes two parameter bytes as LSB/MSB of TOS

                ldy #0
push_inline_pictured_pp:
                sty tmptos+1    ; 1-indexed post-processing routine (0 for none), sign bit if indirect
                sta tmptos      ; masked picture

                bit tmptos+1
                bmi +           ; if indirect, caller is responsible for setting tmp1/+1

                ply             ; LSB of return address
                sty tmp1
                ply             ; MSB
                sty tmp1+1
+
                dex             ; add one stack entry
                dex
                cmp #%00010000  ; maybe add a second?
                bcc +
                dex
                dex
                cmp #%01000000  ; and even a third?
                bcc +
                dex
                dex
+
                phx             ; save stack pointer
                dex             ; initial decrement so we can pre-increment
_loop:
                inx             ; next data stack byte to fill
                lsr tmptos      ; fetch next picture bit to carry
                beq _done       ; if picture is now empty, we found the terminator
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

                lda tmptos+1    ; any post-processing to do?
                asl
                beq pp_done_plx

                tax
                jmp (literal_postprocessors-2,x)   ; 1-based indexing
pp_done_plx:
                plx             ; restore the stack pointer
pp_done:
                lda tmptos+1
                bmi _indirect   ; indirect version returns to caller

                lda tmp1+1      ; otherwise continue past the payload
                pha
                lda tmp1
                pha
_indirect:
                rts



literal_postprocessors:
        ; post-processing handlers
        ; if data stack access is needed, routine should PLX and jump to pp_done
        ; otherwise return to pp_done_plx
                .word pp_literal_string         ; index 1
                .word pp_literal_cmpl_call      ; index 2

pp_literal_cmpl_call:
                plx
                jsr cmpl_call_tos
                bra pp_done

pp_literal_string:
                plx
                ; put the string address, tmp1+1, into NOS
                ldy tmp1+1
                lda tmp1
                inc a
                bne +
                iny
        +
                sta 2,x
                sty 3,x

                ; add TOS to return past payload
                clc
                lda tmp1
                adc 0,x
                sta tmp1
                lda tmp1+1
                adc 1,x
                sta tmp1+1
                bra pp_done


; TODO rework CREATE + PFA
