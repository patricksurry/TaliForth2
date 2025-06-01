; Core Forth compilation routines

; Tali Forth 2 for the 65c02
; Scot W. Stevenson <scot.stevenson@gmail.com>
; Sam Colwell
; Patrick Surry
; First version: 1. Jan 2014
; This version: 11. May 2024


; ## BRACKET_CHAR ( "c" -- ) "Compile character"
; ## "[char]"  auto  ANS core
        ; """https://forth-standard.org/standard/core/BracketCHAR
        ; Compile the ASCII value of a character as a literal. This is an
        ; immediate, compile-only word.
        ;
        ; A definition given in
        ; http://forth-standard.org/standard/implement is
        ; : [CHAR]  CHAR POSTPONE LITERAL ; IMMEDIATE
        ; """
xt_bracket_char:
w_bracket_char:
                jsr w_char
                jsr w_literal
z_bracket_char: rts



; ## BRACKET_TICK ( -- ) "Store xt of following word during compilation"
; ## "[']"  auto  ANS core
        ; """https://forth-standard.org/standard/core/BracketTick"""
xt_bracket_tick:
w_bracket_tick:
                jsr w_tick
                jsr w_literal
z_bracket_tick: rts



; ## COLON ( "name" -- ) "Start compilation of a new word"
; ## ":"  auto  ANS core
        ; """https://forth-standard.org/standard/core/Colon
        ;
        ; Use the CREATE routine and fill in the rest by hand.
        ; """
xt_colon:
w_colon:
                ; If we're already in the compile state, complain and quit
                lda state
                ora state+1
                beq +

                lda #err_state
                jmp error
+
                ; switch to compile state
                dec state
                dec state+1

                ; Set bit 6 in status to tell ";" and RECURSE this is a normal word
                ; and bit 7 to tell CREATE not to warn on duplicate name.
                ; Also set bit 4 to initially flag as allow-native
                lda #%11010000
                tsb status

                ; Save cp in WORKWORD so that ";" can add it to the dictionary later.
                ; Otherwise FIND-NAME etc could find a half-finished word when
                ; looking in the Dictionary.
                lda cp
                sta workword
                lda cp+1
                sta workword+1

                ldy #0                  ; Tell CREATE we want neither CFA nor dictionary update
                jsr create_common

z_colon:        rts



; ## COLON_NONAME ( -- ) "Start compilation of a new word""
; ## ":NONAME"  auto  ANS core
        ; """https://forth-standard.org/standard/core/ColonNONAME
        ; Compile a word with no nt.  ";" will put its xt on the stack.
        ; """
xt_colon_noname:
w_colon_noname:
                ; If we're already in the compile state, complain
                ; and quit
                lda state
                ora state+1
                beq +

                lda #err_state
                jmp error
+
                ; switch to compile state
                dec state
                dec state+1

                ; Clear bit 6 in status to tell ";" and RECURSE this is
                ; a :NONAME word.
                lda #%01000000
                trb status

                ; Put cp (the xt for this word) in WORKWORD. The flag above
                ; lets both ";" and RECURSE know that is is an xt instead of an
                ; nt and they will modify their behavior.
                lda cp
                sta workword
                lda cp+1
                sta workword+1
z_colon_noname:        rts



; The user-visible word COMPILE, is defined here along with various
; supporting cmpl_xxx routines.  These generate 65c02 assembler
; instructions from Forth code.  This is a good place to start
; reading if you want to understand how that process works.
;
; We can compile words inline or as JSR calls based on the nc-limit
; variable:  COMPILE, has internal entry points cmpl_by_limit,
; cmpl_inline and cmpl_as_call.  Inline compilation copies the
; source code for a word between xt_<word> and <z_word>,
; optionally removing the initial stack depth check.
; For some words we also discard leading and trailing stack
; juggling based on the ST flag.
;
; A great way to understand what's going on is to write simple
; Forth words and use SEE to disassemble them.  Try different
; values for nc-limit threshold and the strip-underflow flag.
; For example:
;
;       32 nc-limit !
;       true strip-underflow !
;       : fib 0 1 rot 0 ?do over + swap loop drop ;
;       see fib
;
; Forth uses only two branching constructs, an unconditional jump
; and a conditional 0branch.  TaliForth doesn't expose 0BRANCH as
; a user word but see cmpl_jump, cmpl_jump_later, cmpl_jump_tos,
; cmpl_0branch_tos and cmpl_0branch_later.  The xxx_later variants
; let us compile forward references where we need to come back
; and fill in the branch address after we've reached the target.
;
; The conditional zero_branch_runtime is also here.  It has some
; subtlety to support both inline and subroutine forms.
; It's factored into two parts, ztest_runtime and zbranch_runtime.
; Several looping conditional (LOOP, +LOOP, ?DO) implement custom
; tests followed by zbranch_runtime when compiled as subroutines.
; The inline forms are typically much simpler since they can use
; 65c02 jmp and bxx branch opcodes directly.

; this could be exposed as a forth word but currently isn't
compile_nt_comma:  ; ( nt -- )
        ; compile, looks up the nt from the xt which is very slow
        ; if we already have the nt, we can get things rolling faster

        jsr w_dup                       ; ( nt nt )
        jsr w_name_to_int               ; ( nt xt )
        jsr w_dup                       ; ( nt xt xt )
        jsr w_rot                       ; ( xt xt nt )
        bra compile_comma_common


; ## COMPILE_COMMA ( xt -- ) "Compile xt"
; ## "compile,"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/COMPILEComma
        ; Compile the given xt in the current word definition. It is an
        ; error if we are not in the compile state. Because we are using
        ; subroutine threading, we can't use , (COMMA) to compile new words
        ; the traditional way. By default, native compiled is allowed, unless
        ; there is a NN (Never Native) flag associated. If not, we use the
        ; value NC_LIMIT (from definitions.asm) to decide if the code
        ; is too large to be natively coded: If the size is larger than
        ; NC_LIMIT, we silently use subroutine coding. If the AN (Always
        ; Native) flag is set, the word is always natively compiled.
        ; """
xt_compile_comma:
                jsr underflow_1
w_compile_comma:
                ; See if this is an Always Native (AN) word by checking the
                ; AN flag. We need nt for this.
                jsr w_dup               ; keep an unadjusted copy of xt
                jsr w_dup               ; plus one to convert to nt

                ; Nb. this reverse lookup from xt => nt is expensive so it's
                ; better to use the compile_comma_common entrypoint below
                ; if the nt is already available, e.g. while interpreting
                jsr w_int_to_name
                ; ( xt xt nt )

                ; Does this xt even have a valid (non-zero) nt?
                lda 0,x
                ora 1,x
                beq cmpl_as_call        ; No nt so unknown size; must compile as a JSR

compile_comma_common:
                ; ( xt xt nt )

                ; Otherwise investigate the nt
                lda (0,x)               ; get status flags byte @ NT

                sta tmp3                ; and keep a copy
                and #AN+NN              ; check if never native (NN)
                cmp #NN                 ; NN=1, AN=0?  i.e. not ST=AN+AN
                beq cmpl_as_call

                ; ( xt xt nt )          ; maybe native, let's check
                jsr w_wordsize
                ; ( xt xt u )

                ; --- SPECIAL CASE 1: PREVENT RETURN STACK THRASHING ---

                lda tmp3
                and #ST                 ; Check the Stack Thrash flag (ST=NN+AN)
                cmp #ST
                bne _check_uf

_strip_sz = 10  ; skip the standard 10 byte header which saves return address + 1 to tmp1

                ; Start later: xt += sz
                clc
                lda 2,x
                adc #_strip_sz
                sta 2,x
                bcc +
                inc 3,x                 ; we just care about the carry
+
                ; Quit earlier: u -= sz
                sec
                lda 0,x
                sbc #_strip_sz
                sta 0,x
                bcs +
                dec 1,x                 ; we just care about the borrow
+
                ; ( xt xt+sz u-sz )

                ; --- SPECIAL CASE 2: REMOVE UNDERFLOW CHECKING ---
_check_uf:
                ; The user can choose to remove the unterflow testing in those
                ; words that have the UF flag. This shortens the word by
                ; 3 bytes if there is no underflow.

                ; Does the user want to strip underflow checks?
                ldy #uf_strip_offset
                lda (up),y
                iny
                ora (up),y
                beq _check_limit

                jsr w_over
                jsr has_uf_check
                bcc _check_limit        ; not an underflow check

                ; Ready to remove the 3 byte underflow check.

                ; Start later: xt += 3
                clc
                lda 2,x
                adc #3
                sta 2,x
                bcc +
                inc 3,x                  ; we just care about the carry
+
                ; End earlier: u -= 3
                sec
                lda 0,x
                sbc #3
                sta 0,x
                bcs +
                dec 1,x                  ; we just care about the borrow
+
_check_limit:
                ; --- END OF SPECIAL CASES ---
                ; ( xt xt' u )

                lda tmp3
                and #AN+NN              ; check Always Native (AN) bit
                cmp #AN                 ; AN=1, NN=0?  (i.e. not ST=AN+NN)
                beq cmpl_inline         ; always natively compile

cmpl_by_limit:
                ; Compile either inline or as subroutine depending on
                ; whether native code size <= user limit
                ; Returns C=0 if native, C=1 if subroutine
                ; ( xt xt' u )
                ldy #nc_limit_offset+1
                lda 1,x                 ; MSB of word size
                cmp (up),y              ; user-defined limit MSB
                bcc cmpl_inline         ; borrow (C=0) means size < limit
                bne cmpl_as_call        ; else non-zero means size > limit

                ; Check the wordsize LSB against the user-defined limit.
                dey
                lda (up),y              ; user-defined limit LSB
                cmp 0,x
                bcs cmpl_inline         ; not bigger, so good to go

cmpl_as_call:
        ; Compile xt as a subroutine call, return with C=0
        ; Stack is either ( xt xt nt ) or ( xt xt' u )
        ; If the word has stack juggling, we need to use original xt
        ; otherwise use the middle value to respect strip-underflow.
                lda tmp3
                and #ST
                bne +
                jsr w_drop              ; no stack juggling, use middle (xt or xt')
                jsr w_nip
                bra _cmpl
+
                jsr w_two_drop          ; stack juggling, must use first (xt)
_cmpl:
                ; ( jsr_address -- )
                lda #OpJSR
                jsr cmpl_a
                jsr w_comma
                sec
                rts

cmpl_inline:
        ; compile inline, returning C=1
                ; ( xt xt' u -- )
                jsr w_here
                jsr w_swap
                ; ( xt xt' cp u -- )
                jsr w_dup
                jsr w_allot            ; allocate space for the word
                ; Enough of this, let's move those bytes already!
                ; ( xt xt' cp u ) on the stack at this point
                jsr w_move
                jsr w_drop             ; drop original xt
                clc
z_compile_comma:
                rts


has_uf_check:
                ; Check if the addr TOS points an underflow check,
                ; consuming TOS and returning C=1 (true) or C=0 (false)
                ; ( addr -- )

                ; Does addr point at a JSR?
                lda (0,x)               ; fetch byte @ addr
                cmp #OpJSR
                bne _not_uf             ; not a JSR

                ; Is address between underflow_1 ... underflow_4 ?
                ; We can check 0 <= addr - underflow_1 <= underflow_4 - underflow_1 < 256
                jsr w_one_plus
                jsr w_fetch             ; get JSR address to TOS
                lda 0,x                 ; LSB of jsr address
                sec
                sbc #<underflow_1
                tay                     ; stash LSB of result and finish subtraction
                lda 1,x                 ; MSB of jsr address
                sbc #>underflow_1
                bne _not_uf             ; MSB of result must be zero

                cpy #(underflow_4-underflow_1+1)
                bcs _not_uf             ; LSB is too big

                sec                     ; C=1 means it is an UF check
                .byte OpBITzp           ; mask the clc, with no effect on carry
_not_uf:        clc                     ; C=0 means it isn't a UF check
                inx                     ; clean up stack
                inx
                rts


; =====================================================================
; COMPILE WORDS, JUMPS and SUBROUTINE JUMPS INTO CODE

; These routines compile instructions such as "jsr w_words" into a word
; at compile time so they are available at run time. Words that use this
; routine may not be natively compiled. We use "cmpl" as not to confuse these
; routines with the COMPILE, word.  Always call this with a subroutine jump.
; This means combining JSR/RTS to JMP in those cases is not going to work. To
; use, load the LSB of the address in A and the MSB in Y. You can remember
; which comes first by thinking of the song "Young Americans" ("YA") by David
; Bowie.
;
;               ldy #>addr      ; MSB   ; "Young"
;               lda #<addr      ; LSB   ; "Americans"
;               jsr cmpl_subroutine
;
; We have have various utility routines here for compiling a word in Y/A
; and a single byte in A.

;TODO for all of these we could potentially avoid jmp (and NN) and
; use BRA instead.  jump_later is a bit harder since we need to remember NN state
; in case something else changed it

cmpl_jump_later:
    ; compile a jump to be filled in later with dummy address <MSB=Y/LSB=??>
    ; leaving address of the JMP target TOS
                dex
                dex
                lda cp+1
                sta 1,x
                lda cp
                inc a
                sta 0,x
                bne cmpl_jump
                inc 1,x
                bra cmpl_jump

cmpl_jump_tos:
                ; compile a jump to the address at TOS, consuming it
                lda 0,x         ; set up for cmpl_jump Y/A
                ldy 1,x
                inx
                inx

cmpl_jump:
                ; This is the entry point to compile JMP <ADDR=Y/A>
                pha             ; save LSB of address
                lda #%00010000  ; unset bit 4 to flag as never-native
                trb status
                lda #OpJMP      ; load opcode for JMP
                bra +

cmpl_subroutine:
                ; This is the entry point to compile JSR <ADDR=Y/A>
                pha             ; save LSB of address
                lda #OpJSR      ; load opcode for JSR and fall through

+
                ; At this point, A contains the opcode to be compiled,
                ; the LSB of the address is on the 65c02 stack, and the MSB of
                ; the address is in Y
                jsr cmpl_a      ; compile opcode
                pla             ; retrieve address LSB; fall thru to cmpl_word
cmpl_word:
                ; This is the entry point to compile a word in Y/A (little-endian)
                jsr cmpl_a      ; compile LSB of address
                tya             ; fall thru for MSB
cmpl_a:
                ; This is the entry point to compile a single byte which
                ; is passed in A. The built-in assembler assumes that this
                ; routine does not modify Y.
                sta (cp)
                inc cp
                bne _done
                inc cp+1
_done:
                rts



check_nc_limit:
        ; compare A > 0 to nc-limit, setting C=0 if A <= nc-limit (native compile ok)
                pha
                sec
                ldy #nc_limit_offset+1
                lda (up),y              ; if MSB non zero we're good, leave with C=0
                beq +
                clc
+
                pla
                bcc _done
                dea                     ; simplify test to A-1 < nc-limit
                dey
                cmp (up),y              ; A-1 < LSB leaves C=0, else C=1
                ina                     ; restore A, preserves carry
_done:
                rts


cmpl_0branch_later:
        ; compile a 0BRANCH where we don't know the target yet
        ; leaves pointer to the target on TOS
                jsr w_zero             ; dummy placeholder, which forces long jmp in native version
                jsr cmpl_0branch_tos    ; generate native or subroutine branch code
                jsr w_here             ; either way the target address is two bytes before here
                sec
                lda 0,x
                sbc #2
                sta 0,x
                bcs +
                dec 1,x
+
                rts


cmpl_0branch_tos:
                ; compare A > 0 to nc-limit, setting C=0 if A <= nc-limit (should native compile)

                lda #ztest_runtime_size+5       ; typical size of inline form
                jsr check_nc_limit              ; returns C=0 if we should native compile
                bcc _inline

                ; non-native, just generate a call with two-byte address payload

                ldy #>zero_branch_runtime
                lda #<zero_branch_runtime
                jsr cmpl_subroutine             ; call the 0branch runtime

                jmp w_comma                    ; add the payload and return

_inline:
                ; inline the test code
                ldy #0
-
                lda ztest_runtime,y
                jsr cmpl_a
                iny
                cpy #ztest_runtime_size
                bne -

                ; now we'll compile the branch to test the zero flag
                ; first check if we can use a short relative branch or need a long jmp
                ; the short form 'beq target' will work if addr - (here + 2) fits in a signed byte

                lda 0,x
                ora 1,x
                beq _long               ; always use the long form if target is 0

                ; ( addr )
                jsr w_dup
                jsr w_here
                clc
                lda #2
                adc 0,x
                sta 0,x
                bcc +
                inc 1,x
+
                jsr w_minus
                ; ( addr offset )
                ; offset is a signed byte if LSB bit 7 is 0 and MSB is 0 or bit 7 is 1 and MSB is #ff
                inx             ; pre-drop offset and use wraparound indexing to preserve flags
                inx
                lda $ff,x
                tay             ; Y=MSB of offset
                lda $fe,x       ; A=LSB, setting N flag to bit 7
                bmi _minus
                cpy #0          ; if LSB is positive we need MSB = 0
                bra +
_minus:         cpy #$ff        ; if LSB is negative we need MSB = ff
+               bne _long

                ; short relative branch will work!  all we need is code like:
                ;
                ;       beq target      ; relative branch if nearby target
                ;
                lda #OpBEQ
                jsr cmpl_a
                lda $fe,x
                inx             ; drop the original address we used to calc offset
                inx
                jmp cmpl_a

_long:
                ; too far (or unknown) so emit code like:
                ;
                ;       bne +3
                ;       jmp target

                lda #OpBNE
                jsr cmpl_a
                lda #3
                jsr cmpl_a
                jmp cmpl_jump_tos


; =====================================================================
; 0BRANCH runtime
;
; TaliForth doesn't expose 0BRANCH directly as a word, but implements
; all conditional branching with this runtime.  It's broken into
; two parts: ztest_runtime checks if TOS is zero,
; and zbranch_runtime then conditionally branches to a target address.
; This allows the looping constructs LOOP, +LOOP and ?DO to implement
; their own custom tests and reuse the zbranch_runtime for branching.
;
; Native compilation is very straightforward: we inline a few bytes
; from ztest_runtime and tack on a BEQ <target> or BNE +3/JMP <target>
; to implement the branch.  Non-native compilation generates
; JSR zero_branch_runtime / .word <target> so the runtime can
; use its own return address to read <target> and either return to
; that address or simply continue beyond the <target> word.
; This is obviously much slower but sometimes space is more
; important than speed.

zero_branch_runtime:

ztest_runtime:
        ; Drop TOS of stack setting Z flag, for optimizing short branches (see xt_then)
                inx
                inx
                lda $FE,x           ; wraparound so inx doesn't wreck Z status
                ora $FF,x
        ; The inline form ends here and is follwed by a native beq or bne / jmp
ztest_runtime_size = * - ztest_runtime

zbranch_runtime:
        ; The subroutine continues here, and is also used as an alternate entry point
        ; by various conditional looping constructs
        ; If A=0 we branch to the address following the jsr that landed here
        ; otherwise skip past that address and continue
                ply
                sty tmp1
                ply
                sty tmp1+1

                tay             ; test if A = 0 which tells us whether to branch
                bne _nobranch   ; the usual case is to repeat, so fall thru

                ; Flag is FALSE (0) so we take the jump to the address given in
                ; the next two bytes. However, the address points to the last
                ; byte of the JSR instruction, not to the next byte afterwards
                ldy #1
                lda (tmp1),y
                pha                     ; stash the LSB until we've read the MSB too
                iny
                lda (tmp1),y
                sta tmp1+1              ; update tmp1 with our branch target
                pla
                sta tmp1
_jmp:
                ; However we got here, tmp1 has the address to jump to.
                jmp (tmp1)

_nobranch:
                ; no branch, continue past the address bytes
                clc
                lda tmp1        ; LSB
                adc #3          ; skip two bytes plus the extra for jsr/rts behavior
                sta tmp1
                bcc _jmp

                inc tmp1+1
                bra _jmp



; ## COMPILE_ONLY ( -- ) "Mark most recent word as COMPILE-ONLY"
; ## "compile-only"  tested  Tali Forth
        ; """Set the Compile Only flag (CO) of the most recently defined
        ; word.
        ;
        ; The alternative way to do this is to define a word
        ; ?COMPILE that makes sure  we're in compile mode
        ; """
xt_compile_only:
w_compile_only:
                jsr current_to_dp
                lda (dp)        ; status flags are @ NT
                ora #CO        ; make sure bit 7 is set
                sta (dp)

z_compile_only: rts



; ## IMMEDIATE ( -- ) "Mark most recent word as IMMEDIATE"
; ## "immediate"  auto  ANS core
        ; """https://forth-standard.org/standard/core/IMMEDIATE
        ; Make sure the most recently defined word is immediate. Will only
        ; affect the last word in the dictionary. Note that if the word is
        ; defined in ROM, this will have no affect, but will not produce an
        ; error message.
        ; """
xt_immediate:
w_immediate:
                jsr current_to_dp
                lda (dp)        ; status flags are first header byte
                ora #IM         ; ensure IM bit is set
                sta (dp)

z_immediate:    rts



; ## LEFT_BRACKET ( -- ) "Enter interpretation state"
; ## "["  auto  ANS core
        ; """https://forth-standard.org/standard/core/Bracket
        ; This is an immediate and compile-only word
        ; """
xt_left_bracket:
w_left_bracket:
                stz state
                stz state+1

z_left_bracket: rts



; ## POSTPONE ( -- ) "Change IMMEDIATE status (it's complicated)"
; ## "postpone"  auto   ANS core
        ; """https://forth-standard.org/standard/core/POSTPONE
        ; Add the compilation behavior of a word to a new word at
        ; compile time. If the word that follows it is immediate, include
        ; it so that it will be compiled when the word being defined is
        ; itself used for a new word. Tricky, but very useful.
        ;
        ; Because POSTPONE expects a word (not an xt) in the input stream (not
        ; on the Data Stack). This means we cannot build words with
        ; "jsr w_postpone, jsr <word>" directly.
        ; """

xt_postpone:
w_postpone:
                jsr w_parse_name               ; ( -- addr n )

                ; if there was no word provided, complain and quit
                lda 0,x
                ora 1,x
                bne +

                lda #err_noname
                jmp error
+
                jsr w_find_name                 ; ( -- nt | 0 )

                ; if word not in Dictionary, complain and quit
                bne +
                lda #err_noname
                jmp error

+
                ; Grab status flag byte from NT
                lda (0,x)
                and #IM                         ; check Immediate status flag
                beq _not_immediate

                ; We're immediate, so instead of executing it right now, we
                ; compile it. nt is TOS, so this is easy.
                jsr compile_nt_comma
                bra _done

_not_immediate:
                ; This is not an immediate word, so we enact "deferred
                ; compilation" by including ' <NAME> COMPILE-NT, which we do by
                ; compiling the literal xt, and a subroutine jump to COMPILE-NT,
                jsr w_literal                   ; ( nt -- )

                ; Last, compile COMPILE,
                ldy #>compile_nt_comma
                lda #<compile_nt_comma
                jsr cmpl_subroutine
_done:
z_postpone:     rts



; ## RIGHT_BRACKET ( -- ) "Enter the compile state"
; ## "]"  auto  ANS core
        ; """https://forth-standard.org/standard/right-bracket
        ; This is an immediate word.
        ; """
xt_right_bracket:
w_right_bracket:
                lda #$FF
                sta state
                sta state+1
z_right_bracket:
                rts



; ## SEMICOLON ( -- ) or ( -- xt ) for :noname "End compilation of new word"
; ## ";"  auto  ANS core
        ; """https://forth-standard.org/standard/core/Semi
        ; End the compilation of a new word into the Dictionary.
        ;
        ; When we enter, WORKWORD is pointing to the nt of this word in the
        ; Dictionary, DP to the previous word, and CP to the next free byte.
        ; See more details in create_common which sets the stage for us.
        ;
        ; A Forth definition would be (see "Starting Forth"):
        ; : POSTPONE EXIT  REVEAL POSTPONE ; [ ; IMMEDIATE  Following the
        ; practice of Gforth, we warn here if a word has been redefined.
        ; """

xt_semicolon:
w_semicolon:
                dex
                dex
                lda workword
                sta 0,x
                lda workword+1
                sta 1,x                 ; ( xt|nt )

                ; Check if this is a : word or a :NONAME word.
                bit status              ; check bit 6 (overflow flag)
                bvs _colonword

                ; This is a :NONAME word - just put an RTS on the end and
                ; leave workword (xt) on the stack.
                lda #OpRTS
                jsr cmpl_a

                bra _semicolon_done

_colonword:
                ; ( nt )

                ; if status bit 4 is still 1, we didn't compile any never-native
                ; code so we can safely clear the NN flag
                lda #%00010000
                and status
                beq +
                lda (workword)
                and #255-NN
                sta (workword)
+
                ; Calculate code size by subtracting xt from CP.
                dex
                dex
                lda cp
                sta 0,x
                lda cp+1
                sta 1,x                 ; ( nt cp )

                jsr w_swap              ; ( cp nt )
                jsr w_name_to_int       ; ( cp xt )
                jsr w_minus             ; ( cp-xt )

                ; We've optimistically saved only one byte for the code size
                ; in the header.  If the code is too big we have work to do...

                lda 1,x
                beq _setsz              ; one byte size is OK
.if !TALI_OPTION_TERSE
                jsr fixup_long_word
                bcs +                   ; C=1 means fixup already added RTS
.else
                ; we currently only use the word size for SEE so in the
                ; minimal case we'll just call the length 255,
                ; make the word NN, and move on...
                stz 1,x
                lda #$ff
                sta 0,x
                lda (workword)
                ora #NN
                sta (workword)
.endif
_setsz:
                ; Compile the closing RTS instruction
                lda #OpRTS
                jsr cmpl_a
+
                ; ( codesize )

                ; Use header status flags to calculate offset to code size
                lda (workword)          ; Fetch status flags
                and #DC+FP
                lsr                     ; A=0 or 2 with FP in carry
                adc #3
                tay

                lda 0,x                 ; LSB of code size
                sta (workword),y        ; write LSB
                lda 1,x
                beq +

                iny                     ; write MSB only if non-zero
                sta (workword),y
+
                inx                     ; drop codesize
                inx

                ; Before we formally add the word to the Dictionary, we
                ; check to see if it is already present, and if yes, we
                ; warn the user.

                ; See if word already in Dictionary.
                ; (STATUS bit 7 will be high as CREATE already
                ;  checked for us.)
                bit status
                bpl _new_word   ; Bit 7 is clear = new word

                ; This word is already in the Dictionary, so we print a
                ; warning to the user.

                ; Start by putting nt on the stack, using WORKWORD.
                ; Note LATESTNT won't work since we haven't added the
                ; new word to the Dictionary yet
                dex
                dex
                lda workword
                sta 0,x
                lda workword+1
                sta 1,x

                jsr w_name_to_string    ; ( nt -- addr u )

                lda #str_redefined      ; address of string "redefined"
                jsr print_string_no_lf

                ; Now we print the offending word.
                jsr w_type
                jsr w_space

                ; Clear bit 7 of status (so future words will print message
                ; by defaut)
                lda #%10000000
                trb status

_new_word:
                ; Let's get this over with. Save beginning of our word
                ; as new last word in the Dictionary
                lda workword
                sta dp
                lda workword+1
                sta dp+1
                jsr dp_to_current       ; Save the updated DP to the
                                        ; CURRENT wordlist.
_semicolon_done:
                ; Word definition complete. Return compile flag to zero
                ; to return to interpret mode
                stz state
                stz state+1

z_semicolon:    rts


.if !TALI_OPTION_TERSE
fixup_long_word:
        ; Handle word with more than 256 bytes of code.  Our header is too
        ; small by one byte since we now need a two byte code length field.
        ; We've got two options:
        ;
        ; - if the word is relocatable (no NN) then we can shuffle
        ;   the code up one byte to make room for the extra size byte.
        ;
        ; - if the word is NN then we instead move the header itself,
        ;   writing the bigger one immediately after the code.  This
        ;   wastes the original header bytes but is a rare case.

                ; In both cases we need to allocate an extra byte after the code.
                ; For the shuffle case this is a dummy that gets overwritten
                ; when we move up by one.  For the new header case this is
                ; the actual RTS after the original code body.

                lda #OpRTS
                jsr cmpl_a

                ; Either way we'll need the word's name (pointer and lengt)
                dex
                dex
                lda workword
                sta 0,x
                lda workword+1
                sta 1,x
                jsr w_name_to_string

                ; ( codesize nameptr namelen )

                lda (workword)
                and #NN

                bne _mvhdr              ; NN so we'll need a new header

                ; we'll shuffle the name string and code up one byte
                ; nameptr is the start of the block we want to move,
                ; and the number of bytes is namelen + codesize

                ; ( codesize nameptr namelen )
                jsr w_swap
                jsr w_dup
                jsr w_one_plus
                jsr w_rot
                ; ( codesize nameptr nameptr+1 namelen )
                clc
                lda 6,x
                adc 0,x
                sta 0,x
                lda 7,x
                adc 1,x
                sta 1,x
                ; ( codesize nameptr nameptr+1 codesize+namelen )
                jsr w_cmove_up
                ; ( codesize )

                lda (workword)
                ora #LC
                sta (workword)          ; update the flag bit to indicate two-byte code size

                clc                     ; we still need to add the final RTS
                rts

_mvhdr:
                ; moving the header means back to the drawing board
                ; we'll need two bytes each for prev NT (FP=1),
                ; code size (LC=1) and code pointer (DC=1)
                ; which means an eight byte header, plus the name string

                ; keep a copy of the current header pointer
                lda workword
                sta tmp1
                lda workword+1
                sta tmp1+1

                ; the new header will land at CP, after the RTS we wrote above
                lda cp
                sta workword
                lda cp+1
                sta workword+1

                ; ( codesize nameptr namelen )

                ; allocate namelen + 8 bytes for the new header
                dex
                dex
                clc
                lda 2,x
                adc #8                  ; full header size
                sta 0,x
                stz 1,x                 ; no MSB since name length <32
                jsr w_allot

                ; Now fill in the new header
                ldy #0                  ; nt+0
                lda (tmp1),y
                ora #FP+LC+DC           ; need long form for everything
                sta (workword),y        ; status byte
                iny                     ; nt+1
                lda (tmp1),y
                sta (workword),y        ; name length

                jsr nt_to_xt            ; get XT from tmp1 as Y=MSB, A=LSB
                phy
                ldy #4                  ; nt+4
                sta (workword),y        ; XT LSB
                pla
                iny                     ; nt+5
                sta (workword),y        ; XT MSB

                jsr nt_to_nt            ; rewrite tmp1 as prev NT
                ldy #2                  ; nt+2
                lda tmp1
                sta (workword),y
                iny                     ; nt+3
                lda tmp1+1
                sta (workword),y

                ; finally copy the name string
                ; ( codesize nameptr namelen )
                dex
                dex
                clc
                lda workword
                adc #8                  ; offset to name in new header
                sta 0,x
                lda workword+1
                adc #0
                sta 1,x
                jsr w_swap
                ; ( codesize nameptr newnameptr namelen )
                jsr w_cmove_up

                sec                     ; we already have the RTS
                rts
.endif



; ## STATE ( -- addr ) "Return the address of compilation state flag"
; ## "state"  auto  ANS core
        ; """https://forth-standard.org/standard/core/STATE
        ; STATE is true when in compilation state, false otherwise. Note
        ; we do not return the state itself, but only the address where
        ; it lives. The state should not be changed directly by the user; see
        ; http://forth.sourceforge.net/standard/dpans/dpans6.htm#6.1.2250
        ; """
xt_state:
w_state:
                dex
                dex
                lda #<state
                sta 0,x
                lda #>state
                sta 1,x

z_state:        rts

