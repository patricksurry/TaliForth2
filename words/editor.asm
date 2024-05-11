; ==========================================================
; EDITOR words

; This routine is used by both enter-screen and erase-screen
; to get a buffer for the given screen number and set SCR to
; the given screen number.  This word is not in the dictionary.
xt_editor_screen_helper:
                jsr xt_dup
                jsr xt_scr
                jsr xt_store
                jsr xt_buffer
z_editor_screen_helper:
                rts


; ## EDITOR_ENTER_SCREEN ( scr# -- ) "Enter all lines for given screen"
; ## "enter-screen"  auto  Tali Editor

xt_editor_enter_screen:
                ; Set the variable SCR and get a buffer for the
                ; given screen number.
                jsr xt_editor_screen_helper

                ; Drop the buffer address.
                jsr xt_drop

                ; Overwrite the lines one at a time.
                stz ed_head
_prompt_loop:
                ; Put the current line number on the stack.
                dex
                dex
                lda ed_head
                sta 0,x
                stz 1,x

                ; Use the O word to prompt for overwrite.
                jsr xt_editor_o

                ; Move on to the next line.
                inc ed_head
                lda #16
                cmp ed_head
                bne _prompt_loop

z_editor_enter_screen:
                rts



; ## EDITOR_ERASE_SCREEN ( scr# -- ) "Erase all lines for given screen"
; ## "erase-screen"  tested  Tali Editor
xt_editor_erase_screen:
                ; Set the variable SCR and get a buffer for the
                ; given screen number.
                jsr xt_editor_screen_helper

                ; Put 1024 (chars/screen) on stack.
                dex
                dex
                stz 0,x
                lda #4          ; 4 in MSB makes 1024 ($400).
                sta 1,x

                ; Erase the entire block (fill with spaces).
                jsr xt_blank

                ; Mark buffer as updated.
                jsr xt_update

z_editor_erase_screen:
                rts



; ## EDITOR_EL ( line# -- ) "Erase the given line number"
; ## "el"  tested  Tali Editor
xt_editor_el:
                ; Turn the line number into buffer offset.
                ; This also loads the block into the buffer if it's
                ; not there for some reason.
                jsr xt_editor_line

                ; Put 64 (# of chars/line) on the stack.
                dex
                dex
                lda #64
                sta 0,x
                stz 1,x

                ; Fill with spaces.
                jsr xt_blank

                ; Mark buffer as updated.
                jsr xt_update

z_editor_el:    rts



; ## EDITOR_L ( -- ) "List the current screen"
; ## "l"  tested  Tali Editor
; note "l" is used by LIST in the block words

xt_editor_l:
                ; Load the current screen
                dex             ; Put SCR on the stack.
                dex
                ldy #scr_offset
                lda (up),y
                sta 0,x
                iny
                lda (up),y
                sta 1,x
                jsr xt_block    ; Get the current screen.

                jsr xt_cr

                ; Print the screen number.
                ; We're using sliteral, so we need to set up the
                ; appropriate data structure (see sliteral)
                bra _after_screen_msg

_screen_msg:
                .text "Screen #"

_after_screen_msg:
                jsr sliteral_runtime
                .word _screen_msg, _after_screen_msg-_screen_msg

                jsr xt_type

                ; Put the screen number and printed size for u.r on the stack.
                jsr xt_scr
                jsr xt_fetch
                dex
                dex
                lda #4          ; four spaces
                sta 0,x
                stz 1,x
                jsr xt_u_dot_r

                ; The address of the buffer is currently on the stack.
                ; Print 64 chars at a time. TYPE uses tmp1, so we'll
                ; keep track of the line number in tmp3.
                stz tmp3

_line_loop:
                jsr xt_cr

                ; Print the line number (2-space fixed width)
                dex
                dex
                dex
                dex
                stz 3,x
                lda tmp3
                sta 2,x
                stz 1,x
                lda #2
                sta 0,x
                jsr xt_u_dot_r
                jsr xt_space

                ; Print one line using the address on the stack.
                jsr xt_dup
                dex
                dex
                lda #64
                sta 0,x
                stz 1,x
                jsr xt_type

                ; Add 64 to the address on the stack to move to the next line.
                clc
                lda #64
                adc 0,x
                sta 0,x
                bcc +
                inc 1,x
+
                ; Increment the line number (held in tmp3)
                inc tmp3

                ; See if we are done.
                lda tmp3
                cmp #16
                bne _line_loop

                jsr xt_cr
                ; Drop the address on the stack.
                inx
                inx

z_editor_l:            rts



; ## EDITOR_LINE ( line# -- c-addr ) "Turn a line number into address in current screen"
; ## "line"  tested  Tali Editor

xt_editor_line:
                jsr underflow_1

                ; Multiply the TOS by 64 (chars/line) to compute offset.
                ldy #6          ; *64 is same as left shift 6 times.
_shift_tos_left:
                asl 0,x         ; Shift TOS to the left
                rol 1,x         ; ROL brings MSb from lower byte.
                dey
                bne _shift_tos_left
                ; Load the current screen into a buffer
                ; and get the buffer address
                jsr xt_scr
                jsr xt_fetch
                jsr xt_block

                ; Add the offset to the buffer base address.
                jsr xt_plus

z_editor_line:  rts



; ## EDITOR_O ( line# -- ) "Overwrite the given line"
; ## "o"  tested  Tali Editor
xt_editor_o:
                ; Print prompt
                jsr xt_cr
                jsr xt_dup
                jsr xt_two
                jsr xt_u_dot_r
                jsr xt_space
                lda #'*'
                jsr emit_a
                jsr xt_space

                ; Accept new input (directly into the buffer)
                jsr xt_editor_line
                jsr xt_dup      ; Save a copy of the line address for later.
                dex
                dex
                lda #64         ; chars/line
                sta 0,x
                stz 1,x
                jsr xt_accept

                ; Fill the rest with spaces.
                ; Stack is currently ( line_address numchars_from_accept )
                jsr xt_dup
                jsr xt_not_rote ; -rot
                jsr xt_plus
                dex
                dex
                lda #64         ; chars/line
                sta 0,x
                stz 1,x
                jsr xt_rot
                jsr xt_minus
                jsr xt_blank

                ; Mark buffer as updated.
                jsr xt_update

z_editor_o:     rts
