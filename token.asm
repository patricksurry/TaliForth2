tp = $fe                ; zero page storage for token pointer
token_table = $400      ; $200 double-page aligned bytes

.comment
hex
bc0f constant token_initialize
bbde constant xt_token_interpret
bc4a constant xt_token_find

token_initialize execute

here .s
20 c, xt_token_interpret ,
10 c, 6 c,
' dup xt_token_find execute c,
' 1+ xt_token_find execute c,
' * xt_token_find execute c,
0 c,
60 c,

\ 0856  20 DE BB 10 06 41 11 12  00 60

execute


.8d41						xt_dup:
.9810						xt_one_plus:
.a112						xt_star:

.endcomment

xt_token_interpret:
    ; Interpret a sequence of tokens following our call
    ; We can use the stacked return address as the initial token pointer
    ; since it points to the last byte of the jsr here, and we use ++tp semantics

                pla
                sta tp          ; lsb
                pla
                sta tp+1        ; msb
                jsr token_loop
                lda tp+1        ; return to the byte after the token stream
                pha
                lda tp
                pha
                rts

token_loop:     jsr token_handler
                bra token_loop

token_handler_rom   .logical cp_end - size(token_handler_rom)
token_handler:
    ; this will be relocated to RAM
                inc tp
                bne +
                inc tp+1
+               lda (tp)
                beq _break

                asl
                bcs _high

                sta _low_jump+1
_low_jump:      jmp (token_table)

_high:          sta _high_jump+1
_high_jump:     jmp (token_table + 256)

_break:         pla             ; return to xt_interpret
                pla
                rts
.endlogical


token_initialize:
                ldy #size(token_handler_rom) - 1
_relocate:      lda token_handler_rom,y
                sta token_handler,y
                dey
                bpl _relocate

                ; $200 token_table erase
                dex
                dex
                dex
                dex
                stz 0,x
                lda #2
                sta 1,x
                lda #<token_table
                sta 2,x
                lda #>token_table
                sta 3,x
                jsr xt_erase

                ; set up specials as words $00, $10, $20, ...
                ; we space them out to avoid clustering for dynamic hash
                phx

                ldx #sz_token_special-2
_init:          txa
                asl
                asl
                asl
                asl
                tay
                lda token_special,x
                sta token_table,y
                lda token_special+1,x
                sta token_table+1,y
                dex
                dex
                bpl _init

                plx

                rts


xt_token_find:
        ; ( xt -- token )

        ; First check if we have a custom translation
        ; TODO handle separately? eg to get following bytes

                ldy #sz_token_translation
                lda 0,x
_xlate_loop:
                cmp token_translation-3,y   ; LSB match?
                bne _xlate_next
                lda 1,x
                cmp token_translation-2,y   ; MSB match?
                bne _xlate_miss
                lda token_translation-1,y   ; token
                bra _done

_xlate_miss:
                lda 0,x
_xlate_next:
                dey
                dey
                dey
                bne _xlate_loop

        ; Otherwise look in the token table, hashed on LSB

        ; use temp var as base for top or bottom of lookup
                lda #<token_table
                sta tmp1
                lda #>token_table
                sta tmp1+1

        ; we're hashing by LSB so form the initial index
                lda 0,x
                asl
                tay
                bcc _check_slot
                inc tmp1+1              ; hi page of jump table

_check_slot:    iny
                lda (tmp1),y            ; slot MSB=0 indicates empty
                beq _empty_slot

                cmp 1,x                 ; does slot MSB match xt ?
                bne _next_slot

                lda 0,x
                dey
                cmp (tmp1),y            ; does LSB also match?
                beq _found

                iny
_next_slot:     iny
                bne _check_slot
                lda tmp1+1
                eor #1                  ; flip hi/lo page
                sta tmp1+1
                bra _check_slot

    ; add word to the table and fall through to _found
    ; y points to MSB of slot
_empty_slot:    lda 1,x
                sta (tmp1),y
                dey
                lda 0,x
                sta (tmp1),y

    ; calculate the token index, Y pointing at LSB of slot
_found:         lsr tmp1+1      ; get hi/lo page bit into carry
                tya             ; other bits come from half of y
                ror             ; halve y setting bit 7 from hi/lo

_done:          sta 0,x         ; replace xt with token
                stz 1,x
                rts


; list of words that have custom translations
token_translation:
    .word literal_runtime
    .byte $20               ; tok_literal2
sz_token_translation = (* - token_translation)

token_special:
    .word $ffff             ; $00 - dummy to avoid reuse
    .word tok_literal1      ; $10
    .word tok_literal2      ; $20
    .word tok_literal4      ; $40
sz_token_special = * - token_special


; copy 1, 2 or 4 bytes as one or two words to the forth data stack
tok_literal4:   jsr tok_literal2
tok_literal2:   sec
                .byte $24   ; bit zp to skip next byte
tok_literal1:   clc
tok_literal:    dex
                dex
                inc tp
                bne +
                inc tp+1
+               lda (tp)
                sta 0,x
                bcs _msb
                stz 1,x
                rts
_msb:
                inc tp
                bne +
                inc tp+1
+               lda (tp)
                sta 1,x
                rts
