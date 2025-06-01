; Core Forth words related to stack management

; Tali Forth 2 for the 65c02
; Scot W. Stevenson <scot.stevenson@gmail.com>
; Sam Colwell
; Patrick Surry
; First version: 19. Jan 2014
; This version: 21. Apr 2024


; ## BL ( -- c ) "Push ASCII value of SPACE to stack"
; ## "bl"  auto  ANS core
        ; """https://forth-standard.org/standard/core/BL"""
xt_bl:
w_bl:
                dex
                dex
                lda #AscSP
                sta 0,x
                stz 1,x

z_bl:           rts



; ## DEPTH ( -- u ) "Get number of cells (not bytes) used by stack"
; ## "depth"  auto  ANS core
        ; """https://forth-standard.org/standard/core/DEPTH"""
xt_depth:
w_depth:
                lda #dsp0
                stx tmpdsp
                sec
                sbc tmpdsp

                ; divide by two because each cell is two bytes
                lsr

                dex
                dex
                sta 0,x
                stz 1,x

z_depth:        rts



; ## DROP ( u -- ) "Pop top entry on Data Stack"
; ## "drop"  auto  ANS core
        ; """https://forth-standard.org/standard/core/DROP"""
xt_drop:
                jsr underflow_1
w_drop:
                inx
                inx

z_drop:         rts



; ## DUP ( u -- u u ) "Duplicate TOS"
; ## "dup"  auto  ANS core
        ; """https://forth-standard.org/standard/core/DUP"""
xt_dup:
                jsr underflow_1
w_dup:
                dex
                dex

                lda 2,x         ; LSB
                sta 0,x
                lda 3,x         ; MSB
                sta 1,x

z_dup:          rts



; ## NIP ( b a -- a ) "Delete NOS"
; ## "nip"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/NIP"""
xt_nip:
                jsr underflow_2
w_nip:
                lda 0,x         ; LSB
                sta 2,x
                lda 1,x         ; MSB
                sta 3,x

                inx
                inx

z_nip:          rts



; ## OVER ( b a -- b a b ) "Copy NOS to TOS"
; ## "over"  auto  ANS core
        ; """https://forth-standard.org/standard/core/OVER"""
xt_over:
                jsr underflow_2
w_over:
                dex
                dex

                lda 4,x         ; LSB
                sta 0,x
                lda 5,x         ; MSB
                sta 1,x

z_over:         rts



; ## PICK ( n n u -- n n n ) "Move element u of the stack to TOS"
; ## "pick"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/PICK
        ; Take the u-th element out of the stack and put it on TOS,
        ; overwriting the original TOS. 0 PICK is equivalent to DUP, 1 PICK to
        ; OVER. Note that using PICK is considered poor coding form. Also note
        ; that FIG Forth has a different behavior for PICK than ANS Forth.
        ; """

xt_pick:
w_pick:
                ; Checking for underflow is difficult because it depends on
                ; which element we want to grab. We could probably figure
                ; something out, but it wouldn't work with underflow stripping
                ; Since using PICK is considered poor form anyway, we just
                ; leave it as it is
                asl 0,x         ; we assume u < 128 (stack is small)
                txa
                adc 0,x
                tay

                lda 0002,y
                sta 0,x
                lda 0003,y
                sta 1,x

z_pick:         rts



; ## R_FETCH ( -- n ) "Get copy of top of Return Stack"
; ## "r@"  auto  ANS core
        ; """https://forth-standard.org/standard/core/RFetch
        ; This word is Compile Only in Tali Forth, though Gforth has it
        ; work normally as well
        ;
        ; An alternative way to write this word
        ; would be to access the elements on the stack directly like 2R@
        ; does, these versions should be compared at some point.
        ; """
xt_r_fetch:
w_r_fetch:
                ; --- START FOR JSR (save return address + 1) ---

                pla                     ; LSB
                ply                     ; MSB
                inc a
                sta tmp1                ; LSB
                bne +
                iny
+
                sty tmp1+1              ; MSB

                ; --- START FOR NATIVE COMPILE (via ST flag) ---

                ; get the actual top of Return Stack
                dex
                dex

                ply             ; LSB
                sty 0,x
                pla             ; MSB
                sta 1,x

                ; now we have to put that value back
                pha
                phy

                ; --- CUT FOR NATIVE COMPILE ---

z_r_fetch:      jmp (tmp1)




; ## R_FROM ( -- n )(R: n --) "Move top of Return Stack to TOS"
; ## "r>"  auto  ANS core
        ; """https://forth-standard.org/standard/core/Rfrom
        ; Move Top of Return Stack to Top of Data Stack.
        ;
        ; We have to move
        ; the RTS address out of the way first. This word is handled
        ; differently for native and and subroutine compilation, see COMPILE,
        ; This is a compile-only word
        ; """
xt_r_from:
w_r_from:
                ; --- START FOR JSR (save return address + 1) ---

                pla                     ; LSB
                ply                     ; MSB
                inc a
                sta tmp1                ; LSB
                bne +
                iny
+
                sty tmp1+1              ; MSB

                ; --- START FOR NATIVE COMPILE (via ST flag) ---

                dex
                dex

                ; now we can access the actual data

                pla             ; LSB
                sta 0,x
                pla             ; MSB
                sta 1,x

                ; --- CUT FOR NATIVE COMPILE ---

z_r_from:       jmp (tmp1)



; ## ROT ( a b c -- b c a ) "Rotate first three stack entries downwards"
; ## "rot"  auto  ANS core
        ; """https://forth-standard.org/standard/core/ROT
        ; Remember "R for 'Revolution'" - the bottom entry comes out
        ; on top!
        ; """

xt_rot:
                jsr underflow_3
w_rot:
                ldy 5,x         ; MSB first
                lda 3,x
                sta 5,x
                lda 1,x
                sta 3,x
                sty 1,x

                ldy 4,x         ; LSB next
                lda 2,x
                sta 4,x
                lda 0,x
                sta 2,x
                sty 0,x

z_rot:          rts



; ## SWAP ( b a -- a b ) "Exchange TOS and NOS"
; ## "swap"  auto  ANS core
        ; """https://forth-standard.org/standard/core/SWAP"""
xt_swap:
                jsr underflow_2
w_swap:
                lda 0,x         ; LSB
                ldy 2,x
                sta 2,x
                sty 0,x

                lda 1,x         ; MSB
                ldy 3,x
                sta 3,x
                sty 1,x

z_swap:         rts



; ## TO_R ( n -- )(R: -- n) "Push TOS to the Return Stack"
; ## ">r"  auto  ANS core
        ; """https://forth-standard.org/standard/core/toR
        ; This word is handled differently for native and for
        ; subroutine coding, see `COMPILE,`. This is a complile-only
        ; word.
        ; """
xt_to_r:
                ; we can't avoid underflow check here due to the stack prologue
w_to_r:
                ; --- START FOR JSR (save return address + 1) ---

                pla                     ; LSB
                ply                     ; MSB
                inc a
                sta tmp1                ; LSB
                bne +
                iny
+
                sty tmp1+1              ; MSB

                ; --- START FOR NATIVE COMPILE (via ST flag) ---

                ; We check for underflow in the second step, so we can
                ; strip off the stack thrashing for native compiling first

                jsr underflow_1

                ; now we can do the actual work
                lda 1,x         ; MSB
                pha
                lda 0,x         ; LSB
                pha

                inx
                inx

                ; --- CUT FOR NATIVE COMPILE ---

z_to_r:         jmp (tmp1)



; ## TUCK ( b a -- a b a ) "Copy TOS below NOS"
; ## "tuck"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/TUCK"""
xt_tuck:
                jsr underflow_2
w_tuck:
                dex
                dex

                ldy 4,x         ; LSB
                lda 2,x
                sta 4,x
                sty 2,x
                sta 0,x

                ldy 5,x         ; MSB
                lda 3,x
                sta 5,x
                sty 3,x         ; bba
                sta 1,x         ; baa

z_tuck:         rts



; ## TWO_DROP ( n n -- ) "Drop TOS and NOS"
; ## "2drop"  auto  ANS core
        ; """https://forth-standard.org/standard/core/TwoDROP"""
xt_two_drop:
                jsr underflow_2
w_two_drop:
                inx
                inx
                inx
                inx

z_two_drop:     rts



; ## TWO_DUP ( a b -- a b a b ) "Duplicate first two stack elements"
; ## "2dup"  auto  ANS core
        ; """https://forth-standard.org/standard/core/TwoDUP"""
xt_two_dup:
                jsr underflow_2
w_two_dup:
                dex
                dex
                dex
                dex

                lda 4,x         ; TOS
                sta 0,x
                lda 5,x
                sta 1,x

                lda 6,x         ; NOS
                sta 2,x
                lda 7,x
                sta 3,x

z_two_dup:      rts



; ## TWO_FETCH ( addr -- n1 n2 ) "Fetch the cell pair n1 n2 stored at addr"
; ## "2@"  auto  ANS core
        ; """https://forth-standard.org/standard/core/TwoFetch
        ; Note n2 stored at addr and n1 in the next cell -- in our case,
        ; the next two bytes. This is equvalent to  `DUP CELL+ @ SWAP @`
        ; """
xt_two_fetch:
                jsr underflow_1
w_two_fetch:
                lda 0,x
                sta tmp1
                ldy 1,x
                sty tmp1+1

                dex             ; reuse one stack element
                dex

                lda (tmp1)      ; copy LSB
                sta 0,x
                ldy #1          ; copy next
                lda (tmp1),y
                sta 1,x
                iny             ; copy next
                lda (tmp1),y
                sta 2,x
                iny             ; copy next
                lda (tmp1),y
                sta 3,x

z_two_fetch:    rts



; ## TWO_OVER ( d1 d2 -- d1 d2 d1 ) "Copy double word NOS to TOS"
; ## "2over"  auto  ANS core
        ; """https://forth-standard.org/standard/core/TwoOVER"""
xt_two_over:
                jsr underflow_4
w_two_over:
                dex
                dex
                dex
                dex

                lda 8,x
                sta 0,x

                lda 9,x
                sta 1,x

                lda 10,x
                sta 2,x

                lda 11,x
                sta 3,x

z_two_over:     rts



; ## TWO_R_FETCH ( -- n n ) "Copy top two entries from Return Stack"
; ## "2r@"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/TwoRFetch
        ;
        ; This is R> R> 2DUP >R >R SWAP but we can do it a lot faster in
        ; assembler. We use trickery to access the elements on the Return
        ; Stack instead of pulling the return address first and storing
        ; it somewhere else like for 2R> and 2>R. In this version, we leave
        ; it as Never Native; at some point, we should compare versions to
        ; see if an Always Native version would be better
        ; """
xt_two_r_fetch:
w_two_r_fetch:
                ; --- START FOR JSR (save return address + 1) ---

                pla                     ; LSB
                ply                     ; MSB
                inc a
                sta tmp1                ; LSB
                bne +
                iny
+
                sty tmp1+1              ; MSB

                ; --- START FOR NATIVE COMPILE (via ST flag) ---

                ; copy four bytes from return stack to the data stack

                txa             ; arrange for Y = SP; X -= 4
                tsx
                phx             ; 65c02 has no TXY, so do it the hard way
                ply
                sec
                sbc #4
                tax

                lda $101,y
                sta 0,x
                lda $102,y
                sta 1,x
                lda $103,y
                sta 2,x
                lda $104,y
                sta 3,x

                ; --- CUT FOR NATIVE COMPILE ---

z_two_r_fetch:  jmp (tmp1)



; ## TWO_R_FROM ( -- n1 n2 ) (R: n1 n2 -- ) "Pull two cells from Return Stack"
; ## "2r>"  auto  ANS core ext
	    ; """https://forth-standard.org/standard/core/TwoRfrom
        ; Pull top two entries from Return Stack.
        ;
        ; Is the same as
        ; R> R> SWAP. As with R>, the problem with the is word is that
        ; the top value on the ReturnStack for a STC Forth is the
        ; return address, which we need to get out of the way first.
        ; Native compile needs to be handled as a special case.
        ; """
xt_two_r_from:
w_two_r_from:
                ; --- START FOR JSR (save return address + 1) ---

                pla
                ply                     ; MSB
                inc a
                sta tmp1                ; LSB
                bne +
                iny
+
                sty tmp1+1              ; MSB

                ; --- START FOR NATIVE COMPILE (via ST flag) ---

                ; In theory, we should test for underflow on the Return
                ; Stack. However, given the traffic there with an STC
                ; Forth, that's probably not really useful

		; make room on stack
                dex
                dex
                dex
                dex

                pla                     ; LSB
                sta 0,x
                pla                     ; MSB
                sta 1,x

                pla                     ; LSB
                sta 2,x
                pla                     ; MSB
                sta 3,x

                ; --- CUT FOR NATIVE COMPILE ---

z_two_r_from:   jmp (tmp1)



; ## TWO_SWAP ( n1 n2 n3 n4 -- n3 n4 n1 n1 ) "Exchange two double words"
; ## "2swap"  auto  ANS core
        ; """https://forth-standard.org/standard/core/TwoSWAP"""
xt_two_swap:
                jsr underflow_4
w_two_swap:
                ; 0 <-> 4
                lda 0,x
                ldy 4,x
                sta 4,x
                sty 0,x

                ; 1 <-> 5
                lda 1,x
                ldy 5,x
                sta 5,x
                sty 1,x

                ; 2 <-> 6
                lda 2,x
                ldy 6,x
                sta 6,x
                sty 2,x

                ; 3 <-> 7
                lda 3,x
                ldy 7,x
                sta 7,x
                sty 3,x

z_two_swap:     rts



; ## TWO_TO_R ( n1 n2 -- )(R: -- n1 n2 "Push top two entries to Return Stack"
; ## "2>r"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/TwotoR
        ; Push top two entries to Return Stack.
        ;
        ; The same as SWAP >R >R
        ; except that if we jumped here, the return address will be in the
        ; way. May not be natively compiled unless we're clever and use
        ; special routines.
        ; """
xt_two_to_r:
                ; we can't avoid the underflow check here due to the stack prologue
w_two_to_r:
                ; --- START FOR JSR (save return address + 1) ---

                pla                     ; LSB
                ply                     ; MSB
                inc a
                sta tmp1                ; LSB
                bne +
                iny
+
                sty tmp1+1              ; MSB

                ; --- START FOR NATIVE COMPILE (via ST flag) ---

                jsr underflow_2

                ; now we can move the data
                lda 3,x         ; MSB
                pha
                lda 2,x         ; LSB
                pha

                ; now we can move the data
                lda 1,x         ; MSB
                pha
                lda 0,x         ; LSB
                pha

                inx
                inx
                inx
                inx

                ; --- CUT FOR NATIVE COMPILE ---

z_two_to_r:     jmp (tmp1)



; END
