; Core Forth words for control flow


; ## ABORT_QUOTE ( "string" -- ) "If flag TOS is true, ABORT with message"
; ## "abort""  tested  ANS core
        ; """https://forth-standard.org/standard/core/ABORTq
        ; Abort and print a string.
        ; """

xt_abort_quote:
w_abort_quote:
                ; save the string
                jsr w_s_quote          ; S"

                ; compile run-time part
                ldy #>abort_quote_runtime
                lda #<abort_quote_runtime
                jsr cmpl_subroutine     ; may not be JMP as JSR/RTS

z_abort_quote:  rts


abort_quote_runtime:
        ; """Runtime aspect of ABORT_QUOTE"""
                ; We arrive here with ( f addr u )
                lda 4,x
                ora 5,x
                beq _done       ; if FALSE, we're done

                ; We're true, so print string and ABORT. We follow Gforth
                ; in going to a new line after the string
                jsr w_type
                jsr w_cr
                jmp w_abort    ; not JSR, so never come back
_done:
                ; Drop three entries from the Data Stack
                txa
                clc
                adc #6
                tax

                rts



; ## AGAIN ( addr -- ) "Code backwards branch to address left by BEGIN"
; ## "again"  tested  ANS core ext
        ; """https://forth-standard.org/standard/core/AGAIN"""
xt_again:
                jsr underflow_1
w_again:
                ; Compile a JMP back to TOS address.
                jsr cmpl_jump_tos

z_again:        rts



; ## BEGIN ( -- addr ) "Mark entry point for loop"
; ## "begin"  auto  ANS core
        ; """https://forth-standard.org/standard/core/BEGIN
        ;
        ; This is a dummy header, BEGIN shares the actual code with HERE
        ; It could as well be coded in Forth as
        ;       : BEGIN HERE ; IMMEDIATE COMPILE-ONLY
        ; """



; ## CASE (C: -- 0) ( -- ) "Conditional flow control"
; ## "case"  auto  ANS core ext
        ; """http://forth-standard.org/standard/core/CASE
        ;
        ; This is a dummy header, CASE shares the actual code with ZERO.
        ; """



; ## QUESTION_DO (C: -- ) ( limit start -- ) "Conditional loop start"
; ## "?do"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/qDO"""
xt_question_do:
w_question_do:
                ; ?DO shares most of its code with DO.
                ; But first compile its runtime.
                dex
                dex
                lda #<question_do_runtime
                sta 0,x
                lda #>question_do_runtime
                sta 1,x
                jsr w_dup              ; xt and xt' are the same
                dex
                dex
                lda #question_do_runtime_size
                sta 0,x
                stz 1,x
                jsr cmpl_by_limit
                bcc _native

                ; for subroutine compile, write placeholder for jmp-target and save its address
                jsr w_here
                jsr w_zero
                jsr w_comma
                bra do_common

_native:
                ; for native compile, add the placeholder jump, saving its address
                jsr cmpl_jump_later
                bra do_common


; ## DO (C: -- ) ( limit start -- )  "Start a loop"
; ## "do"  auto  ANS core
        ; """https://forth-standard.org/standard/core/DO
        ;
        ; Compile-time part of DO. Could be realized in Forth as
        ;       : DO POSTPONE (DO) HERE ; IMMEDIATE COMPILE-ONLY
        ; but we do it in assembler for speed. See
        ; the Control Flow section of the manual for details.
        ;
        ; This is never native compile. Don't check for a stack underflow
        ; """

xt_do:
w_do:
                jsr w_zero             ; push 0 TOS

do_common:
                ; the stack is ( 0 | jmp-target ) depending
                ; on whether we arrived from DO or ?DO

                ; The word LEAVE can be used to exit LOOP/+LOOP from
                ; anywhere inside the loop body, zero or more times.
                ; We'll use loopleave as the head of a linked list
                ; which points at the latest LEAVE address
                ; that we need to patch.  The xt_leave compilation
                ; will link backward to any prior LEAVE.
                ; To handle nested loops we stack the current value
                ; of loopleave here and restore it in xt_loop
                ; after we write any chained jumps for the current loop

                ; save current loopleave in case we're nested
                dex
                dex
                lda loopleave
                sta 0,x
                lda loopleave+1
                sta 1,x

                ; For now there is no LEAVE addr to patch, which we
                ; flag with MSB=0 (zero page) which is never a compilation target
                stz loopleave+1

                ; compile runtime part of DO.
                ; do this as a subroutine since it only happens once and is a big chunk of code
                ldy #>do_runtime
                lda #<do_runtime
                jsr cmpl_subroutine

                ; Now we're ready for the loop body.  We also push HERE
                ; to the Data Stack so LOOP/+LOOP knows where to repeat back
                ; ( qdo-skip old-loopleave repeat-addr )

                jmp w_here
z_question_do:
z_do:


question_do_runtime:
        ; """This is called (?DO) in some Forths. See the explanation at
        ; do_runtime for the background on this design
        ; """
                ; if TOS == NOS we skip the loop and drop the limits
                lda 0,x
                cmp 2,x
                bne _begin
                lda 1,x
                cmp 3,x
                bne _begin
                inx                     ; drop loop limits and skip
                inx
                inx
                inx
question_do_runtime_size = * - question_do_runtime
                ; for native compilation we stop here and ?DO will tack on a JMP <skip-addr>
                ; for subroutine compile we set up A for zbranch_runtime
                lda #0
                .byte $2c               ; BIT llhh to hide the lda #1
_begin:         lda #1
                jmp zbranch_runtime



do_runtime:
        ; """Runtime routine for DO loop. Note that ANS loops quit when the
        ; boundary of limit-1 and limit is reached, a different mechanism than
        ; the FIG Forth loop (you can see which version you have by running
        ; a loop with start and limit as the same value, for instance
        ; 0 0 DO -- these will walk through the number space). We use a
        ; "fudge factor" for the limit that makes the Overflow Flag trip when
        ; it is reached; see http://forum.6502.org/viewtopic.php?f=9&t=2026
        ; for further discussion of this. The source given there for
        ; this idea is Laxen & Perry F83. -- This routine is called (DO)
        ; in some Forths.
        ; """
                ldy loopctrl
                bmi +                   ; is this the first LCB?
                lda loopidx0            ; no, write cached LSB
                sta loopindex,y         ; back to loopindex in the LCB
+
                iny                     ; Reserve 4 bytes for next LCB
                iny
                iny
                iny
                sty loopctrl            ; Udpate LCB stack pointer

                ; data stack has ( limit index -- )
                ;
                ; We're going to calculate adjusted loop bounds
                ; and store the values in the current LCB:
                ;
                ;   loopfufa = $8000 - limit
                ;   loopindex = loopfufa + index
                ;
                ; The idea is that once we've incremented this adjusted
                ; index at least limit-index times we'll get:
                ;
                ;   loopindex' = $8000 - limit + index + (limit - index)
                ;              = $8000
                ;
                ; which will trigger LOOP's overflow test

                sec
                lda #0
                sbc 2,x             ; LSB of limit
                sta loopfufa,y      ; write to loop control block
                lda #$80
                sbc 3,x             ; MSB of limit
                sta loopfufa+1,y

                ; Second step: index is FUFA plus original index
                clc
                lda 0,x             ; LSB of original index
                adc loopfufa,y
                sta loopidx0        ; write LSB to cache not LCB
                lda 1,x             ; MSB of orginal index
                adc loopfufa+1,y
                sta loopindex+1,y

                inx                 ; clean up the stack
                inx
                inx
                inx

                rts



; ## ELSE (C: orig -- orig' ) ( -- ) "Conditional flow control"
; ## "else"  auto  ANS core
        ; """http://forth-standard.org/standard/core/ELSE
        ;
        ; The code is shared with ENDOF and most of THEN
        ; """

xt_else:
xt_endof:
                jsr underflow_1
w_else:
w_endof:
                ; Add an unconditional branch with target filled in later
                jsr cmpl_jump_later

                ; stash the branch target for later
                ; and then calculate the forward branch from orig
                jsr w_swap              ; ( target orig )
                bra w_then              ; fall through to then

xt_then:
                jsr underflow_1
w_then:
                ; (C: orig -- ) ( -- )

                ; This is a compile-time word that writes the target address
                ; of an earlier forward branch.  For example xt_if writes
                ; zbranch <placeholder> which wants to skip forward to here
                ; if the condition is false.  The orig argument on the stack
                ; is the address of the placeholder which needs to point here.
                ;
                ; Note this is also used by several other words that write
                ; a forward branch, like xt_else and xt_while.

                ; Just stuff HERE in for the branch address back
                ; at the IF or ELSE (origination address is on stack).
                jsr w_here
                jsr w_swap
                jsr w_store

z_else:
z_endof:
z_then:         rts



; ## ENDCASE (C: case-sys -- ) ( x -- ) "Conditional flow control"
; ## "endcase"  auto  ANS core ext
        ; """http://forth-standard.org/standard/core/ENDCASE"""

xt_endcase:
                jsr underflow_1
w_endcase:
                ; Postpone DROP to remove the item
                ; being checked.
                ldy #>w_drop
                lda #<w_drop
                jsr cmpl_subroutine

                ; There are a number of address (of branches that need their
                ; jump addressed filled in with the address of right here).
                ; Keep calling THEN to deal with them until we reach the
                ; 0 that CASE put on the stack at the beginning.
_endcase_loop:
                ; Check for 0 on the stack.
                lda 0,x
                ora 1,x
                beq _done

                jsr w_then
                bra _endcase_loop
_done:
                ; Remove the 0 from the stack.
                inx
                inx
z_endcase:      rts



; ## ENDOF (C: case-sys1 of-sys1-- case-sys2) ( -- ) "Conditional flow control"
; ## "endof"  auto  ANS core ext
        ; """http://forth-standard.org/standard/core/ENDOF
        ; This is a dummy entry, the code is shared with ELSE
        ; """



; ## EXECUTE ( xt -- ) "Jump to word based on execution token"
; ## "execute"  auto  ANS core
        ; """https://forth-standard.org/standard/core/EXECUTE"""
xt_execute:
                jsr underflow_1
w_execute:
                jsr doexecute   ; do not combine to JMP (native coding)

z_execute:      rts

doexecute:
                lda 0,x
                sta ip
                lda 1,x
                sta ip+1

                inx
                inx

                ; we don't need a RTS here because we highjack the RTS of
                ; the word we're calling to get back to xt_execute
                jmp (ip)

; end of doexecute



; ## EXIT ( -- ) "Return control to the calling word immediately"
; ## "exit"  auto  ANS core
        ; """https://forth-standard.org/standard/core/EXIT
        ; If we're in a loop, user should UNLOOP first to clean up
        ; any loop control. This should be natively compiled.
        ; """

xt_exit:
w_exit:
                rts             ; keep before z_exit
z_exit:                         ; never reached



; ## I ( -- n )  "Copy loop counter to stack"
; ## "i"  auto  ANS core
        ; """https://forth-standard.org/standard/core/I
        ; See definitions.asm and the Control Flow section of the manual.
        ;
        ; This word can be native compiled or not since
        ; it no longer depends on the return stack.
        ; """

xt_i:
w_i:
                dex
                dex

                ; The fudged index and offset are stored in the current
                ; loop control block, with loopidx0 cached in zp

                ldy loopctrl
                sec
                lda loopidx0        ; cached LSB of loopindex
                sbc loopfufa,y
                sta 0,x
                lda loopindex+1,y
                sbc loopfufa+1,y
                sta 1,x

z_i:            rts



; ## IF (C: -- orig) (flag -- ) "Conditional flow control"
; ## "if"  auto  ANS core
        ; """http://forth-standard.org/standard/core/IF"""

xt_if:
w_if:
                jsr cmpl_0branch_later
z_if:           rts



; ## J ( -- n ) "Copy second loop counter to stack"
; ## "j"  auto  ANS core
        ; """https://forth-standard.org/standard/core/J
        ; Copy second loop counter from Return Stack to stack. Note we use
        ; a fudge factor for loop control; see the Control Flow section of
        ; the manual for more details.
        ;
        ; This can be native compiled or not since it no longer uses the stack
        ; """

xt_j:
w_j:
                dex                 ; make space on the stack
                dex

                ; subtract four to get the enclosing LCB offset
                lda loopctrl
                sec
                sbc #4
                tay
                sec
                lda loopindex,y
                sbc loopfufa,y
                sta 0,x
                lda loopindex+1,y
                sbc loopfufa+1,y
                sta 1,x
z_j:            rts



; ## LEAVE ( -- ) "Leave DO/LOOP construct"
; ## "leave"  auto  ANS core
        ; """https://forth-standard.org/standard/core/LEAVE
        ; Note that this does not work with anything but a DO/LOOP in
        ; contrast to other versions such as discussed at
        ; http://blogs.msdn.com/b/ashleyf/archive/2011/02/06/loopty-do-i-loop.aspx
        ;
        ;       : LEAVE POSTPONE BRANCH HERE SWAP 0 , ; IMMEDIATE COMPILE-ONLY
        ; See definitions.asm and the Control Flow section in the manual
        ; for details of how this works.
        ; This must be native compile and not IMMEDIATE
        ; """

xt_leave:
w_leave:
                ; LEAVE will eventually jump forward to the unloop.
                ; We don't know where that is at compile time
                ; so we'll write a JMP to be patched later.
                ; Since LEAVE is allowed multiple times we'll
                ; use the JMP placeholder address to keep a linked list
                ; of all LEAVE addresses to update, headed by loopleave

                lda loopleave
                ldy loopleave+1
                jsr cmpl_jump   ; emit the JMP chaining prior leave address

                ; set head of the list to point to our placeholder
                sec
                lda cp
                sbc #2
                sta loopleave
                lda cp+1
                bcs +
                dea
+               sta loopleave+1

z_leave:
                rts



; ## LOOP ( -- ) "Finish loop construct"
; ## "loop"  auto  ANS core
        ; """https://forth-standard.org/standard/core/LOOP
        ; Compile-time part of LOOP. This is specialized to
        ; increment by one.
        ;
        ; In Forth, this is
        ;       : LOOP  POSTPONE 1 POSTPONE (+LOOP) , POSTPONE UNLOOP ;
        ;       IMMEDIATE ; COMPILE-ONLY
        ; """
xt_loop:
w_loop:
                ; Compile LOOP-specific runtime
                dex
                dex
                dex
                dex
                lda #<loop_runtime
                sta 2,x
                lda #>loop_runtime
                sta 3,x
                lda #loop_runtime_size
                sta 0,x
                stz 1,x

                ; Now compile the runtime shared with +LOOP
                bra loop_common



; ## PLUS_LOOP ( -- ) "Finish loop construct"
; ## "+loop"  auto  ANS core
        ; """https://forth-standard.org/standard/core/PlusLOOP
        ;
        ; Compile-time part of +LOOP, also used for LOOP. Is usually
        ;       : +LOOP POSTPONE (+LOOP) , POSTPONE UNLOOP ; IMMEDIATE
        ;       COMPILE-ONLY
        ; in Forth. LOOP uses this routine as well. We jump here with the
        ; address for looping as TOS and the address for aborting the loop
        ; (LEAVE) as the second double-byte entry on the Return Stack (see
        ; DO and the Control Flow section of the manual for details).
        ; """

xt_plus_loop:
w_plus_loop:
                ; Compile +LOOP-specific runtime
                dex
                dex
                dex
                dex
                lda #<plus_loop_runtime
                sta 2,x
                lda #>plus_loop_runtime
                sta 3,x
                lda #plus_loop_runtime_size
                sta 0,x
                stz 1,x

                ; fall through to shared runtime

loop_common:
                jsr w_over
                jsr w_swap             ; xt and xt' are the same
                ; ( xt xt u )
                jsr cmpl_by_limit

                ; The address we need to loop back to is TOS
                ; ( qdo-skip old-loopleave repeat-addr )

                bcc _native

                ; if non-native, just write repeat-addr as payload after the call
                jsr w_comma
                bra +

_native:
                ; if native, write the JMP repeat-addr after either loop runtime
                jsr cmpl_jump_tos
+

                ; any LEAVE words want to jmp to the unloop we'll write here
                ; so follow the linked list and update them
                lda loopleave+1         ; MSB=0 means we're done
                beq _noleave
_next:
                ; stash current LEAVE addr which links to
                ; the previous one (if any) and replace it with HERE
                ldy #1
                lda (loopleave),y
                pha
                lda cp+1
                sta (loopleave),y
                dey
                lda (loopleave),y
                pha
                lda cp
                sta (loopleave),y

                ; follow the chain backward
                pla
                sta loopleave
                pla
                sta loopleave+1
                bne _next
_noleave:
                ; restore loopleave in case we were nested
                lda 0,x
                sta loopleave
                lda 1,x
                sta loopleave+1

                ; reuse TOS

                ; Clean up the loop params by appending unloop
                lda #<nt_unloop
                sta 0,x
                lda #>nt_unloop
                sta 1,x
                jsr compile_nt_comma    ; use the faster entry with the NT

                ; Finally we're left with qdo-skip which either
                ; points at ?DO's "skip the loop" jmp address,
                ; wanting to skip past this whole mess to CP=HERE,
                ; or has MSB=0 from DO which we can just ignore
                lda 1,x                 ; MSB=0 means DO so nothing to do
                beq +
                jsr w_here
                jsr w_swap
                jmp w_store             ; write here as ?DO jmp target and return

+               inx                     ; drop the ignored word for DO
                inx
z_loop:
z_plus_loop:    rts


loop_runtime:
        ; """Runtime compile for LOOP when stepping by one.
        ; This must always be native compiled.
        ; """
                ; do_runtime has set up the loop control block with
                ; loopindex:    $8000-limit+index
                ; loopfufa:     $8000-limit

                ; so we need to increment loopindex and
                ; and look for overflow as explained in do_runtime

                inc loopidx0            ; increment the LSB of loopindex
                bne _repeat             ; avoid expensive test most of the time

                ; we might be done so need to inc and check the MSB

                ldy loopctrl
                ; for the +1 case we can increment MSB and test for #$80
                ; unlike the +LOOP case where we use V to flag crossing #$80
                lda loopindex+1,y
                ina
                cmp #$80
                beq _done
                sta loopindex+1,y
loop_runtime_size = * - loop_runtime
_repeat:
                ; for native compilation we stop here and LOOP/+LOOP will tack on a JMP <repeat-addr>
                ; for subroutine compile we set up A for zbranch_runtime
                lda #0
                .byte $2c               ; BIT llhh to hide the lda #1
_done:          lda #1
                jmp zbranch_runtime


plus_loop_runtime:
        ; """Runtime compile for +LOOP when we have an arbitrary step.
        ; See below for loop_runtime when step=1. Note we use a fudge factor for
        ; loop control so we can test with the Overflow Flag. See
        ; the Control Flow section of the manual for details.
        ; The step value is TOS in the loop.
        ; This must always be native compiled.
        ; In some Forths, this is a separate word called (+LOOP) or (LOOP)
        ; """

                clc
                lda 0,x                 ; LSB of step
                adc loopidx0
                sta loopidx0

                inx                     ; dump step from TOS before MSB test
                inx                     ; since we might skip it
                lda $FF,x               ; MSB of step since 1,x == -1,x+2
                bne _chkv               ; if it's non-zero we have to check
                bcc _repeat             ; but if 0 and no carry, we're good

_chkv:          clv
                ldy loopctrl            ; get LCB offset
                adc loopindex+1,y       ; MSB of index
                sta loopindex+1,y       ; put MSB of index back on stack

                ; If V flag is set, we're done looping and continue
                ; after the +LOOP instruction
                bvs _done               ; skip over JMP instruction
plus_loop_runtime_size = * - plus_loop_runtime
_repeat:
                ; for native compilation we stop here and LOOP/+LOOP will tack on a JMP <repeat-addr>
                ; for subroutine compile we set up A and continue with zbranch_runtime
                lda #0
                .byte $2c               ; BIT llhh to hide the lda #1
_done:          lda #1
                jmp zbranch_runtime



; ## OF (C: -- of-sys) (x1 x2 -- |x1) "Conditional flow control"
; ## "of"  auto  ANS core ext
        ; """http://forth-standard.org/standard/core/OF"""

xt_of:
w_of:
                ; Check if value is equal to this case.
                ; Postpone over (eg. compile a jsr to it)
                ldy #>w_over
                lda #<w_over
                jsr cmpl_subroutine

                ; Postpone = (EQUAL), that is, compile a jsr to it
                ldy #>w_equal
                lda #<w_equal
                jsr cmpl_subroutine

                jsr w_if

                ; If it's true, consume the original value.
                ; Postpone DROP (eg. compile a jsr to it)
                ldy #>w_drop
                lda #<w_drop
                jsr cmpl_subroutine

z_of:           rts



; ## QUESTION_DUP ( n -- 0 | n n ) "Duplicate TOS non-zero"
; ## "?dup"  auto  ANS core
        ; """https://forth-standard.org/standard/core/qDUP"""

xt_question_dup:
                jsr underflow_1
w_question_dup:
                ; Check if TOS is zero
                lda 0,x
                ora 1,x
                beq _done

                ; not zero, duplicate
                dex
                dex
                lda 2,x
                sta 0,x
                lda 3,x
                sta 1,x
_done:
z_question_dup: rts



; ## RECURSE ( -- ) "Copy recursive call to word being defined"
; ## "recurse"  auto  ANS core
        ; """https://forth-standard.org/standard/core/RECURSE
        ;
        ; This word may not be natively compiled
        ; """

xt_recurse:
w_recurse:
                ; The whole routine amounts to compiling a reference to
                ; the word that is being compiled. WORKWORD contains either
                ; the nt (if : started the word) or the xt (if :NONNAME
                ; started the word). Status bit 6 tells us which.

                lda workword
                ldy workword+1

                bit status                      ; status bit 6 => V flag
                bvc _got_xt

                ; we have a bit more work to get nt -> xt
                sta tmp1
                sty tmp1+1
                jsr nt_to_xt                    ; nt in tmp1 to y/a

_got_xt:
                jsr cmpl_subroutine             ; JSR <Y/A>

z_recurse:      rts



; ## REPEAT (C: orig dest -- ) ( -- ) "Loop flow control"
; ## "repeat"  auto  ANS core
        ; """http://forth-standard.org/standard/core/REPEAT"""

xt_repeat:
                jsr underflow_2
w_repeat:
                ; Code the jump back to begin
                jsr w_again

                ; Stuff HERE in for the branch address left by WHILE
                ; to get out of the loop
                jmp w_then
z_repeat:



; ## THEN (C: orig -- ) ( -- ) "Conditional flow control"
; ## "then"  auto  ANS core
        ; """http://forth-standard.org/standard/core/THEN
        ; This is a dummy entry, the code is shared with xt_else
        ; """



; ## UNLOOP ( -- ) "Drop current loop control block"
; ## "unloop"  auto  ANS core
        ; """https://forth-standard.org/standard/core/UNLOOP"""
xt_unloop:
w_unloop:
                ; This is used as an epliogue to each LOOP/+LOOP
                ; as well as prior to EXIT'ng a loop
                ; We need to drop the current loop control block
                ; and restore the cached loopidx0 of the prior loop, if any

                ldy loopctrl
                dey
                dey
                dey
                dey
                sty loopctrl
                bmi z_unloop            ; no active loops?

                lda loopindex,y         ; else re-cache the LSB of loopindex
                sta loopidx0

z_unloop:       rts



; ## UNTIL (C: dest -- ) ( -- ) "Loop flow control"
; ## "until"  auto  ANS core
        ; """http://forth-standard.org/standard/core/UNTIL"""
xt_until:
                jsr underflow_1
w_until:
                ; The address to loop back to is on the stack.
                jsr cmpl_0branch_tos

z_until:        rts



; ## WHILE ( C: dest -- orig dest ) ( x -- ) "Loop flow control"
; ## "while"  auto  ANS core
        ; """http://forth-standard.org/standard/core/WHILE"""
xt_while:
                jsr underflow_1
w_while:
                jsr cmpl_0branch_later          ; branch to location we'll determine later
                ; tuck the address of the branch placeholder under the repeat address left by begin
                jsr w_swap
                ; ( branch-target repeat-target )

z_while:        rts



; END
