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

push_inline_sliteral:
        ; Put a string literal on the stack.  Supported by DISASM
        ; Note the length and actual text follow the JSR.
        ; The values ( addr length ) are pushed to the data stack
        ; with addr pointing to the literal "<text>"
        ;
        ;       jsr push_inline_sliteral
        ;       .word <length>
        ;       .text "<text>"

                lda #%10001111
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

                lda #%00111111
                bra push_inline_pictured

push_inline_addru_literal:
        ; In word implementations we often need to stack (addr u) with u < 256
        ; This saves a byte vs push_inline_2literal.
        ; Not used in generated code so not supported by DISASM
        ;
        ;       jsr push_inline_addru_literal   ; stack (addr u)
        ;       .byte u         ; TOS
        ;       .word addr      ; NOS

                lda #%00111011
                bra push_inline_pictured

push_inline_literal:
        ; Put a word literal on the stack.  Supported by DISASM
        ;
        ;       jsr push_inline_literal
        ;       .word TOS

                lda #%00001110
                bra push_inline_pictured

push_inline_bliteral:
        ; Put a byte literal on the stack with MSB=0.  Supported by DISASM
        ;       jsr push_inline_bliteral
        ;       .byte TOS

                lda #%00001010

push_inline_pictured:
        ; Copies 1-4 bytes following the calling JSR to the data stack.
        ; The accumulator contains flags defining the mapping from bytes to stack values.
        ;
        ;       msb                  lsb
        ;        N  V  p  p  p  p  p  n
        ;
        ; The lsb (bit 0) creates either one (0) or two (1) new stack entries.
        ; Bits 1-5 are a one-terminated stack picture mapping payload bytes to stack bytes.
        ; For one stack entry the picture is 00/xx where / is the 1 terminator
        ; and the two xx bits map the payload byte(s) to the data stack.
        ; A one bit copies a payload byte, a zero bit writes a zero.
        ; For two stack entries the picture is /xxxx where / is the 1 terminator.
        ; Parameter bytes are copied to the data stack in memory order, i.e. the
        ; first byte after the JSR will be nearest TOS (lowest in data stack memory).
        ; Examples:
        ;       NV00/110 consumes two parameter bytes as the LSB/MSB of one stack word
        ;       NV00/010 consumes one parameter byte as the LSB of TOS, with MSB=0
        ;       NV/01011 consumes two single bytes as the LSB of two words on the data stack
        ;       NV/00111 creates an empty NOS and consumes two parameter bytes as LSB/MSB of TOS
        ;       NV00/111 creates undef NOS and consumes two parameter bytes as LSB/MSB of TOS
        ; The two high bits (N and V) control post-processing:
        ;       N = 1 indicates a string payload, which writes addr to NOS and returns to addr+TOS
        ;       V = 1 means return to caller's caller rather than continuing past payload
        ;             which is useful for DISASM or other post-processing like cmpl_call_inline_literal

                ply             ; LSB of address
                sty tmp1
                ply             ; MSB of address
                sty tmp1+1

push_pictured_common:
                sta tmptos+1    ; save for flag bits
                and #%00111111
                sta tmptos      ; save masked picture

                dex             ; add one stack entry
                dex
                lsr tmptos      ; conditionally add a second?
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
                plx             ; reset stack pointer

                bit tmptos+1    ; check N+V flag bits
                bpl _return     ; is it a string?

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

                bit tmptos+1    ; restore V status
_return:
                bvs _indirect
                lda tmp1+1
                pha
                lda tmp1
                pha
_indirect:
                rts             ; continue execution past the payload



cmpl_call_inline_literal:
        ; Generate code that calls the literal
        ;
        ;       jsr cmpl_call_inline_literal    ; compile "JSR target"
        ;       .word target

                ; set up the payload address
                ply             ; <payload-1>
                sty tmp1
                ply             ; MSB of address
                sty tmp1+1

                ; like push_inline_literal, but returns to caller (us) instead of (tmp1)
                lda #%01001110
                jsr push_pictured_common
                jsr cmpl_call_tos
                ldy tmp1+1
                phy
                ldy tmp1
                phy
                rts



; TODO post-processing:

; just jmp(tmp1)
; cmpl_call_tos / jmp(tmp1)
; string / jmp(tmp1)
; create / jmp(tmp1) - tho it's used so put back to stack?
; indirect / don't put back
