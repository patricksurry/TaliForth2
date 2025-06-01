; Core Forth word routines
; Tali Forth 2 for the 65c02
; Scot W. Stevenson <scot.stevenson@gmail.com>
; Sam Colwell
; Patrick Surry
; First version: 19. Jan 2014
; This version: 21. Apr 2024


; ## ACTION_OF ( "name" -- xt ) "Get named deferred word's xt"
; ## "action-of"  auto  ANS core ext
        ; """http://forth-standard.org/standard/core/ACTION-OF"""
xt_action_of:
w_action_of:
                ; This is a state aware word with differet behavior
                ; when used while compiling vs interpreting.
                ; Check STATE
                lda state
                ora state+1
                beq _interpreting

                ; Run ['] to compile the xt of the next word
                ; as a literal.
                jsr w_bracket_tick

                ; Postpone DEFER@ by compiling a JSR to it.
                ldy #>w_defer_fetch
                lda #<w_defer_fetch
                jsr cmpl_subroutine
                bra _done

_interpreting:
                jsr w_tick
                jsr w_defer_fetch

_done:
z_action_of:           rts



; ## BASE ( -- addr ) "Push address of radix base to stack"
; ## "base"  auto  ANS core
        ; """https://forth-standard.org/standard/core/BASE
        ; The ANS Forth standard sees the base up to 36, so we can cheat and
        ; ingore the MSB
        ; """
xt_base:
w_base:
                dex
                dex
                lda #<base
                sta 0,x         ; LSB
                stz 1,x         ; MSB is always 0

z_base:         rts



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



; ## BUFFER_COLON ( u "<name>" -- ; -- addr ) "Create an uninitialized buffer"
; ## "buffer:"  auto  ANS core ext
                ; """https://forth-standard.org/standard/core/BUFFERColon
                ; Create a buffer of size u that puts its address on the stack
                ; when its name is used.
                ; """
xt_buffer_colon:
w_buffer_colon:
                jsr w_create            ; will report default PFA size of 2 in SEE
                jsr w_allot
z_buffer_colon: rts



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



; ## CONSTANT ( n "name" -- ) "Define a constant"
; ## "constant"  auto  ANS core
        ; """https://forth-standard.org/standard/core/CONSTANT
        ;
        ; Forth equivalent is  CREATE , DOES> @  but we do
        ; more in assembler and let CREATE do the heavy lifting.
        ; See http://www.bradrodriguez.com/papers/moving3.htm for
        ; a primer on how this works in various Forths. This is the
        ; same code as VALUE in our case.
        ; """
xt_value:
xt_constant:
                jsr underflow_1
w_value:
w_constant:

            	; Use create but with DOCONST for constants.
                lda #2
                sta tmpdsp              ; 2 byte PFA
                lda #<doconst           ; LSB of DOCONST
                ldy #>doconst           ; MSB of DOCONST
                jsr create_common

                ; Now we save the constant number itself in the next cell
                jsr w_comma
z_value:
z_constant:     rts


doconst:
        ; """Execute a CONSTANT: Push the data in the first two bytes of
        ; the Data Field onto the Data Stack
        ; """
                dex             ; make room for constant
                dex

                ; The value we need is stored in the two bytes after the
                ; JSR return address, which in turn is what is on top of
                ; the Return Stack
                pla             ; LSB of return address
                sta tmp1
                pla             ; MSB of return address
                sta tmp1+1

                ; Start LDY with 1 instead of 0 because of how JSR stores
                ; the return address on the 65c02
                ldy #1
                lda (tmp1),y
                sta 0,x
                iny
                lda (tmp1),y
                sta 1,x

                ; This takes us back to the original caller, not the
                ; DOCONST caller
                rts



; ## COUNT ( c-addr -- addr u ) "Convert character string to normal format"
; ## "count"  auto  ANS core
        ; """https://forth-standard.org/standard/core/COUNT
        ; Convert old-style character string to address-length pair. Note
        ; that the length of the string c-addr is stored in character length
        ; (8 bit), not cell length (16 bit). This is rarely used these days,
        ; though COUNT can also be used to step through a string character by
        ; character.
        ; """
xt_count:
                jsr underflow_1
w_count:
                lda (0,x)       ; Get number of characters (255 max)
                tay

                ; move start address up by one
                inc 0,x         ; LSB
                bne +
                inc 1,x         ; MSB

                ; save number of characters to stack
+               tya
                dex
                dex
                sta 0,x         ; LSB
                stz 1,x         ; MSB, always zero

z_count:        rts



; ## CREATE ( "name" -- ) "Create Dictionary entry for 'name'"
; ## "create"  auto  ANS core
        ; """https://forth-standard.org/standard/core/CREATE
        ;
        ; See the drawing in headers.asm for details on the header
        ; """
xt_create:
w_create:
                ; Several routines build new words using create_common.
                ; They'll pass the CFA in A/Y, with Y=0 indicating no CFA.
                ; When Y is non-zero, tmpdsp should contain the planned
                ; PFA size so we can adjust the word length for SEE.
                ; Note that we're only responsible for allocating the header
                ; space. The caller will allocate and populate the PFA itself.
                lda #2                  ; default 2 byte PFA for variable
                sta tmpdsp
create_dovar:
                ldy #>dovar
                lda #<dovar
create_common:
                ; save the CFA
                dex
                dex
                sta 0,x
                sty 1,x                 ; ( cfa )

                ; get string
                jsr w_parse_name        ; ( cfa addr u )

                ; We want a length between 1 and 31.  We could allow 1-32
                ; and store length-1 but it doesn't seem worth the hassle.
                ; Complain and quit if it's empty.  Shorten it if too long.
                lda 1,x
                bne _too_long

                lda 0,x
                bne +

                lda #err_noname
                jmp error
+
                cmp #32
                bcc +

_too_long:
                ; The name is too long - silently shorten to 31 chars
                lda #31
                sta 0,x
                stz 1,x
+
                ; Check to see if this name already exists.
                jsr w_two_dup           ; ( cfa addr u addr u )
                jsr w_find_name         ; ( cfa addr u flag ) (non-zero nt as flag)

                inx                     ; pre-drop flag (nt) from find-name.
                inx

                lda $fe,x
                ora $ff,x
                beq _new_name           ; We haven't seen this one before.

                ; This name already exists.  See if we are supposed to print
                ; the message for it.

                ; Check bit 7
                bit status
                bpl _redefined_name     ; Bit 7 is zero, so print the message.

                ; We aren't supposed to print the redefined message ourselves,
                ; but we should indicate that it is redefined (for ; to print
                ; later).
                lda #$80                ; Set bit 7 to indicate dup
                tsb status
                bra _process_name

_redefined_name:
                ; Print the message that the name is redefined.
                lda #str_redefined
                jsr print_string_no_lf

                jsr w_two_dup           ; ( cfa addr u addr u )
                jsr w_type
                jsr w_space

                bra _process_name

_new_name:
                lda #$80                ; Clear status bit 7 to indicate new word.
                trb status

_process_name:
                ; ( cfa addr u )

                ; We need to decide on the flexible sizes in the header before
                ; we know how much memory to allot.  We'll always generate adjoining
                ; code so DC=0.  We can check nt - last_nt to see if we can just store
                ; the LSB (FP=0) or we need LSB/MSB (FP=1).  The LC (long code)
                ; flag is harder since we don't know the code length until after we've
                ; written the header and finish code generation.  Catch-22?
                ; Luckily we have an out.  We'll optimistically assume the generated
                ; code is < 256 bytes (LC=0) which is usually true.  Once compilation
                ; is done, `;' will check if the word is flagged as never native (NN).
                ; If so we'll create a new header after the code, wasting a few bytes.
                ; Otherwise the code is relocatable so we'll shift it up by a byte
                ; to make room for the two byte length.

                ; Get the CURRENT dictionary pointer.
                jsr current_to_dp

                ; Remember the first free byte of memory as the start of
                ; the header for the new word: tmp1 = cp.
                ; Calculate offset from dp at the same time.
                lda cp
                sta tmp1
                clc                     ; we want cp - dp - 1 so clc for the -1
                sbc dp

                lda cp+1
                sta tmp1+1
                sbc dp+1                ; A has MSB of cp - dp - 1

                ; we can skip MSB if cp-dp-1 is 0

                beq +                   ; if A is 0 we can use a single byte offset
                lda #FP                 ; otherwise we'll need a two byte pointer
+
                ; Finish determining the status flag byte in A.

                ; Most of the words CREATE'd with DOXXX CFA's must currently be
                ; called via JSR (i.e. never native) since they are compiled like
                ; `jsr doxxx + data` and expect to extract their data and then rts
                ; to the parent caller.  (Note: it might be possible to inline these
                ; if they were instead compiled like `<push address-of-data> + rts + data`
                ; so there'd only be one instance of data and fewer jsr levels.)

                ; Although many words CREATE'd without a CFA can be compiled natively
                ; we don't know for sure until we've seen whether they contain things
                ; like looping constructs with non-relocatable JMPs.

                ; Long story short, we flag everything as NN here, but then revert when
                ; possible in ";".
                ora #NN

                ; Words defined by CREATE are marked in the header as
                ; having a Code Field Area (CFA), which is a bit tricky for
                ; Subroutine Threaded Code (STC). We do this so >BODY works
                ; correctly with DOES> and CREATE. See the discussion at
                ; http://forum.6502.org/viewtopic.php?f=9&t=5182 for details

                ; ( cfa addr u )

                ldy 5,x                 ; check MSB of CFA
                beq +                   ; 0 means no CFA, don't set HC

                ora #HC                 ; otherwise set the HC bit
+
                ; Now start writing the header byte-by-byte

                ; HEADER BYTE 0: status flags byte
                jsr cmpl_a
                lsr                     ; FP -> C tells us 1 or 2 byte last nt

                ; HEADER BYTE 1: length of name
                lda 0,x
                jsr cmpl_a

                ; HEADER BYTE 2 or 2,3: last nt
                ; We always write the LSB
                lda dp                  ; LSB of prev header is in dp
                jsr cmpl_a              ; note cmpl_a doesn't affect carry

                ; If C=FP=1 we write the MSB, otherwise we'll infer it later
                bcc +                   ; FP=0, skip the MSB
                lda dp+1                ; otherwise MSB of dp
                jsr cmpl_a
+
                ; Interlude: Point start of dictionary (DP) at our new header (old CP)
                ; and update the CURRENT wordlist with the new DP
                ; unless it's a ":" word with no CFA which ";" will add to dictionary later
                lda 5,x                 ; has cfa?
                beq +

                lda tmp1
                sta dp
                lda tmp1+1
                sta dp+1

                jsr dp_to_current
+
                ; We always write code adjacent to header (DC=0) so skip the xt field

                ; HEADER BYTE 3 or 4: Length of code
                ; If there's no CFA this is zero since we have no code yet,
                ; otherwise it's three bytes for the subroutine call we'll compile below
                ; along with the size of the parameter field area (PFA) from tmpdsp
                lda 5,x                 ; has CFA?
                beq +                   ; leave A=0

                clc
                lda #3                  ; otherwise 3 plus the size of the PFA area
                adc tmpdsp              ; add PFA size, assume no carry
+
                jsr cmpl_a

                ; HEADER BYTE 4 or 5 onward: Name string
                ; We have ( cfa addr u ) and will compile bytes
                ; by hand so we can translate to lowercase

                ldy 0,x                 ; Y = name length
                inx                     ; drop name length
                inx                     ; ( cfa addr )
_name_loop:
                lda (0,x)               ; get next character of name

                ; Make sure it goes into the dictionary in lower case.
                cmp #'Z'+1
                bcs +
                cmp #'A'
                bcc +

                ora #$20                ; uppercase to lowercase
+
                jsr cmpl_a
                dey
                beq _end

                inc 0,x                 ; increment string address
                bne _name_loop

                inc 1,x
                bra _name_loop

_end:
                inx                     ; drop address leaving ( cfa )
                inx

                ; After the name string comes the code field, starting at the
                ; current xt of this word, which for CREATE is a subroutine call
                ; to DOVAR.  Other words use different subroutines or omit the CFA.

                ldy 1,x                 ; check MSB
                beq +

                lda 0,x
                jsr cmpl_subroutine     ; Add the CFA jsr to Y/A
+
                ; And we're done. Drop CFA
                inx
                inx

z_create:       rts


dovar:
        ; """Execute a variable: Push the address of the first bytes of
        ; the Data Field onto the stack. This is called with JSR so we
        ; can pick up the address of the calling variable off the 65c02's
        ; stack. The final RTS takes us to the original caller of the
        ; routine that itself called DOVAR. This is the default
        ; routine installed with CREATE.
        ; """
                ; Pull the return address off the machine's stack, adding
                ; one because of the way the 65c02 handles subroutines
                ply             ; LSB
                pla             ; MSB
                iny
                bne +
                ina
+
                dex
                dex

                sta 1,x
                tya
                sta 0,x

                rts



; ## DECIMAL ( -- ) "Change radix base to decimal"
; ## "decimal"  auto  ANS core
        ; """https://forth-standard.org/standard/core/DECIMAL"""
xt_decimal:
w_decimal:
                lda #10
                sta base
                stz base+1              ; paranoid

z_decimal:      rts



; ## DEFER ( "name" -- ) "Create a placeholder for words by name"
; ## "defer"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/DEFER
        ; Reserve an name that can be linked to various xt by IS.
        ;
        ; The ANS reference implementation is
        ;       CREATE ['] ABORT , DOES> @ EXECUTE ;
        ; But we use this routine as a low-level word so things go faster

xt_defer:
w_defer:
                ; we want CREATE but with DODEFER as the CFA
                lda #2
                sta tmpdsp      ; 2 byte PFA
                lda #<dodefer   ; LSB
                ldy #>dodefer   ; MSB
                jsr create_common

                ; DODEFER executes the next address it finds after
                ; its call. As default, we include the error
                ; "Defer not defined"
                lda #<defer_error
                ldy #>defer_error
                jsr cmpl_word

z_defer:        rts


defer_error:
                ; """Error routine for undefined DEFER: Complain and abort"""
                lda #err_defer
                jmp error


dodefer:
        ; """Execute a DEFER statement at runtime: Execute the address we
        ; find after the caller in the Data Field
        ; """
                ; The xt we need is stored in the two bytes after the JSR
                ; return address, which is what is on top of the Return
                ; Stack. So all we have to do is replace our return jump
                ; with what we find there
                pla             ; LSB
                sta tmp1
                pla             ; MSB
                sta tmp1+1

                ldy #1
                lda (tmp1),y
                sta tmp2
                iny
                lda (tmp1),y
                sta tmp2+1

                jmp (tmp2)      ; This is actually a jump to the new target



; ## DEFER_FETCH ( xt1 -- xt2 ) "Get the current XT for a deferred word"
; ## "defer@"  auto  ANS core ext
        ; """http://forth-standard.org/standard/core/DEFERFetch"""

xt_defer_fetch:
                jsr underflow_1
w_defer_fetch:
                jsr w_to_body
                jsr w_fetch
z_defer_fetch:  rts



; ## DEFER_STORE ( xt2 x1 -- ) "Set xt1 to execute xt2"
; ## "defer!"  auto  ANS core ext
        ; """http://forth-standard.org/standard/core/DEFERStore"""

xt_defer_store:
                jsr underflow_2
w_defer_store:
                jsr w_to_body
                jsr w_store
z_defer_store:  rts



; ## DOES ( -- ) "Add payload when defining new words"
; ## "does>"  auto  ANS core
        ; """https://forth-standard.org/standard/core/DOES
        ; Create the payload for defining new defining words. See
        ; http://www.bradrodriguez.com/papers/moving3.htm and
        ; the Developer Guide in the manual for a discussion of
        ; DOES>'s internal workings. This uses tmp1 and tmp2.
        ; """

xt_does:
w_does:
                ; compile a subroutine jump to runtime of DOES>
                ldy #>does_runtime
                lda #<does_runtime
                jsr cmpl_subroutine

                ; compile a subroutine jump to DODOES. In traditional
                ; terms, this is the Code Field Area (CFA) of the new
                ; word
                ldy #>dodoes
                lda #<dodoes
                jsr cmpl_subroutine

z_does:         rts


does_runtime:
        ; """Runtime portion of DOES>. This replaces the subroutine jump
        ; to DOVAR that CREATE automatically encodes by a jump to the
        ; address that contains a subroutine jump to DODOES. We don't
        ; jump to DODOES directly because we need to work our magic with
        ; the return addresses. This routine is also known as "(DOES)" in
        ; other Forths
        ; """

                ; CREATE has also already modified the DP to point to the new
                ; word. We have no idea which instructions followed the CREATE
                ; command if there is a DOES> so the CP could point anywhere
                ; by now.

                jsr current_to_dp       ; Grab the DP from the CURRENT wordlist.
                lda dp
                sta tmp1
                lda dp+1
                sta tmp1+1
                jsr nt_to_xt            ; nt in tmp1 to xt in y/a
                sta tmp1                ; xt in tmp2
                sty tmp1+1

                pla                     ; LSB of return address
                ply                     ; MSB

                ina                     ; increment to point at PFA
                bne +
                iny
+
                phy

                ; Replace the DOVAR address with our own
                ldy #1                  ; xt points at jsr lsb/msb
                sta (tmp1),y
                iny
                pla
                sta (tmp1),y

                ; Since we removed the return address that brought us here, we
                ; go back to whatever the main routine was. Otherwise, we
                ; smash into the subroutine jump to DODOES.
                rts


dodoes:
        ; """Execute the runtime portion of DOES>. See DOES> and
        ; docs/create-does.txt for details and
        ; http://www.bradrodriguez.com/papers/moving3.htm
        ; """
                ; typically called like
                ;       : foo CREATE 0 , DOES> forth code ;
                ;
                ; which makes a defining word `foo` like
                ;
                ; xt_foo:
                ;    jsr w_create
                ;    jsr w_zero                 ; initialize PFA with 0
                ;    jsr w_comma
                ;    jsr does_runtime           ; point CFA to _doit and return
                ; _doit:
                ;    jsr dodoes
                ; _action:
                ;    <compiled forth code>
                ;    rts
                ;
                ; so defining `foo someword` will first create something like:
                ;
                ; someword:
                ;       jsr dovar       ; code field area (CFA)
                ;       .word 0         ; parameter field area (PFA)
                ;
                ; and then does_runtime will convert it into:
                ;
                ; xt_someword:
                ;       jsr _doit
                ;       .word 0         ; PFA
                ;
                ; so finally invoking `someword` from some caller will
                ; do `jsr xt_someword` followed by `jsr _doit` which in turn
                ; triggers `jsr dodoes`, resulting in a return stack like:
                ;
                ;       (R: caller-1 PFA-1 _action-1 )
                ;
                ; our job here is to put the PFA address on the data stack
                ; and jump to _action, eventually returning to caller, ie.
                ;
                ;       (R: caller-1 PFA-1 _action-1 -- caller-1 )
                ;       ( -- PFA )
                ;       jmp _action

                ; Assumes the address of the CFA of the original defining word
                ; (say, CONSTANT) is on the top of the Return Stack. Save it
                ; for a later jump, adding one byte because of the way the
                ; 6502 works
                ply             ; LSB
                pla             ; MSB
                iny
                bne +
                ina
+
                sty tmp2
                sta tmp2+1

                ; Next on the Return Stack should be the address of the PFA of
                ; the calling defined word (say, the name of whatever constant we
                ; just defined). Move this to the Data Stack, again adding one.
                dex
                dex

                ply
                pla
                iny
                bne +
                ina
+
                sty 0,x         ; LSB
                sta 1,x         ; MSB

                ; This leaves the return address from the original main routine
                ; on top of the Return Stack. We leave that untouched and jump
                ; to the special code of the defining word. It's RTS instruction
                ; will take us back to the main routine
                jmp (tmp2)



; ## FIND ( caddr -- addr 0 | xt 1 | xt -1 ) "Find word in Dictionary"
; ## "find"  auto  ANS core
        ; """https://forth-standard.org/standard/core/FIND
        ; Included for backwards compatibility only, because it still
        ; can be found in so may examples. It should, however, be replaced
        ; by FIND-NAME. Counted string either returns address with a FALSE
        ; flag if not found in the Dictionary, or the xt with a flag to
        ; indicate if this is immediate or not. FIND is a wrapper around
        ; FIND-NAME, we get this all over with as quickly as possible. See
        ; https://www.complang.tuwien.ac.at/forth/gforth/Docs-html/Word-Lists.html
        ; https://www.complang.tuwien.ac.at/forth/gforth/Docs-html/Name-token.html
        ; """

xt_find:
                jsr underflow_1
w_find:
                jsr w_dup		; dup caddr in case conversion fails

                ; Convert ancient-type counted string address to
                ; modern format
                jsr w_count            ; ( caddr -- addr u )
                jsr w_find_name        ; ( addr u -- nt | 0 )

                ; ( caddr nt | 0 )

                lda 0,x
                ora 1,x
                beq _done               ; Not found, just return ( caddr 0 )

                ; We arrive here with ( caddr nt ). Now we have to
                ; convert the return values to FIND's format

                ; First check the status flag @ nt
                ldy #1                  ; assume immediate, returning 1
                lda (0,x)              ; check status flag byte
                and #IM                 ; is IM set?
                bne +
                ldy #$ff                ; not immediate, return -1
+
                phy                     ; stash the 1 or -1

                jsr w_name_to_int       ; ( nt -- xt )
                jsr w_swap

                ; ( xt caddr )
                pla                     ; result 1 or -1

                sta 0,x
                bmi +                   ; for -1 we store $ff twice
                dec a                   ; for 1 we store 1 and then 0
+
                sta 1,x
_done:
z_find:         rts



; ## HEX ( -- ) "Change base radix to hexadecimal"
; ## "hex"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/HEX"""
xt_hex:
w_hex:
                lda #16
                sta base
                stz base+1              ; paranoid

z_hex:          rts



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



; ## IS ( xt "name" -- ) or (C: "name" ) "Set named word to execute xt"
; ## "is"  auto  ANS core ext
        ; """http://forth-standard.org/standard/core/IS"""

xt_is:
w_is:
                ; This is a state aware word with differet behavior
                ; when used while compiling vs interpreting.
                ; Check STATE
                lda state
                ora state+1
                beq _interpreting

                ; Run ['] to compile the xt of the next word as a literal.
                jsr w_bracket_tick

                ; Postpone DEFER! by compiling a JSR to it.
                ldy #>w_defer_store
                lda #<w_defer_store
                jsr cmpl_subroutine

                bra _done

_interpreting:
                jsr w_tick
                jsr w_defer_store
_done:
z_is:           rts



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



; ## LITERAL ( n -- ) "Store TOS to be pushed on stack during runtime"
; ## "literal"  auto  ANS core
        ; """https://forth-standard.org/standard/core/LITERAL
        ; Compile-only word to store TOS so that it is pushed on stack
        ; during runtime. This is a immediate, compile-only word. At runtime,
        ; it works by calling literal_runtime by compling JSR LITERAL_RT.
        ;
        ; Note the cmpl_ routines use TMPTOS
        ; """
xt_literal:
                jsr underflow_1
w_literal:
                lda #template_push_tos_size
                jsr check_nc_limit
                bcc _inline

                ldy #>literal_runtime
                lda #<literal_runtime
                jsr cmpl_subroutine

                ; Compile the value that is to be pushed on the Stack during
                ; runtime
                jsr w_comma
                bra z_literal

_inline:
                ; we'll need the MSB (if non-zero) and LSB to fill in the template
                ; which we set up on the stack in reverse order
                ; first we need the STZ/STY opcode for the end of the template
                ldy #$94        ; STY opcode
                lda 1,x         ; MSB
                bne +
                ldy #$74        ; STZ opcode
+               phy

                lda 0,x         ; LSB
                pha

                ; if MSB is non-zero, stack it, otherwise skip first two bytes of template
                ldy #2
                lda 1,x         ; MSB
                beq _copy
                ldy #0
                pha

_copy:          lda template_push_tos,y
                cmp #$ff        ; is it a placeholder?
                bne +
                pla
+               jsr cmpl_a
                iny
                cpy #template_push_tos_size
                bne _copy

                inx             ; drop the literal
                inx

z_literal:      rts

template_push_tos:
                ldy #$ff        ; we'll omit this if MSB is zero
                lda #$ff
                dex
                dex
                sta 0,x
                .byte $ff, 1    ; this will become either sty 1,x or stz 1,x
template_push_tos_size = * - template_push_tos


literal_runtime:
                ; During runtime, we push the value following this word back
                ; on the Data Stack. The subroutine jump that brought us
                ; here put the address to return to on the Return Stack -
                ; this points to the data we need to get. This routine is
                ; also called (LITERAL) in some Forths
                dex
                dex

            	; The 65c02 stores <RETURN-ADDRESS>-1 on the Return Stack,
                ; so we are actually popping the address-1 of the literal
                pla             ; LSB
                sta tmp1
                pla             ; MSB
                sta tmp1+1

                ; Fetch the actual literal value and push it on Data stack
                ldy #1
                lda (tmp1),y    ; LSB
                sta 0,x
                iny
                lda (tmp1),y    ; MSB
                sta 1,x

                ; Adjust return address and push back on the Return Stack
                tya
                clc
                adc tmp1
                tay
                lda tmp1+1
                adc #0
                pha
                phy

                rts



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



; ## S_BACKSLASH_QUOTE ( "string" -- )( -- addr u ) "Store string in memory"
; ## "s\""  auto  ANS core
        ; """https://forth-standard.org/standard/core/Seq
        ; Store address and length of string given, returning ( addr u ).
        ; ANS core claims this is compile-only, but the file set expands it
        ; to be interpreted, so it is a state-sensitive word, which in theory
        ; are evil. We follow general usage. This is just like S" except
        ; that it allows for some special escaped characters.
        ; """

xt_s_backslash_quote:
w_s_backslash_quote:
                ; tmp2 will be used to determine if we are handling
                ; escaped characters or not. In this case, we are,
                ; so set it to $FF (the upper byte will be used to
                ; determine if we just had a \ and the next character
                ; needs to be modifed as an escaped character).
                lda #$FF
                sta tmp2
                stz tmp2+1

                ; Now that the flag is set, jump into s_quote to process
                ; the string.
                jsr s_quote_start
z_s_backslash_quote:
                rts



; ## S_QUOTE ( "string" -- )( -- addr u ) "Store string in memory"
; ## "s""  auto  ANS core
        ; """https://forth-standard.org/standard/core/Sq
        ; Store address and length of string given, returning ( addr u ).
        ; ANS core claims this is compile-only, but the file set expands it
        ; to be interpreted, so it is a state-sensitive word, which in theory
        ; are evil. We follow general usage.
        ;
        ; Can also be realized as
        ;     : S" [CHAR] " PARSE POSTPONE SLITERAL ; IMMEDIATE
        ; but it is used so much we want it in code.
        ; """

xt_s_quote:
w_s_quote:
                ; tmp2 will be used to determine if we are handling
                ; escaped characters or not.  In this case, we are
                ; not, so set it to zero.  (cf S_BACKSLASH_QUOTE)
                stz tmp2
                stz tmp2+1

s_quote_start:
                ; S" has undefined interpretation semantics in the CORE word set, but
                ; the FILE wordset permits it provided "no standard words other than S"
                ; ... [should overwrite the] interpreted string"
                ; (see https://forth-standard.org/standard/file/Sq for the details).
                ; One approach would be to reserve a fixed buffer of at least 80
                ; bytes somewhere outside the dictionary, like we do for command history
                ; or the block buffer.  The alternative adopted here is to always
                ; allocate space for the string in the dictionary.  This means
                ; that every interactive use of S" allocates space you won't get back
                ; (without MARKER or the like) but has the big advantage that strings
                ; stay where you put them and don't get overwritten by other operations.

                ; We will save a bit of space when interpreting by writing the string
                ; literal directly HERE.  When we're compiling we'll use SLITERAL
                ; which needs a five byte prologue (jsr sliteral_runtime / .word length)
                ; so we'll leave space for that.

                lda state               ; check whether we're interpreting (0) or compiling (-1)
                ora state+1             ; paranoid

                pha                     ; save zero / nonzero for post-processing
                beq _interpreting       ; just write string directly

                ; we're compiling, so reserve just enough space for SLITERAL to later
                ; add the prologue before the string data

                clc
                lda cp
                adc #5                  ; reserve five bytes for the prologue (see below)
                sta cp
                bcc +
                inc cp+1
+
_interpreting:
                ; Now we'll compile the string bytes into the dictionary
                ; But first remember the address where we started

                jsr w_here              ; ( addr )

_savechars_loop:
                ; Start saving the string into the dictionary up to the
                ; ending double quote. First, check to see if the input
                ; buffer is empty.
                lda toin+1              ; MSB
                cmp ciblen+1
                bcc _input_fine         ; unsigned comparison

                lda toin                ; LSB
                cmp ciblen
                bcc _input_fine

                ; Input buffer is empty. Refill it. Refill calls accept,
                ; which uses tmp2 and tmp3. Save and restore them.
                lda tmp2
                pha
                lda tmp2+1
                pha
                lda tmp3    ; Only tmp3 used, so don't bother with tmp3+1
                pha

                jsr w_refill           ; ( -- f )

                pla
                sta tmp3
                pla
                sta tmp2+1
                pla
                sta tmp2

                ; Check result of refill.
                lda 0,x
                ora 1,x
                bne _refill_ok

                ; Something when wrong with refill.
                lda #err_refill
                jmp error

_refill_ok:
                ; Remove the refill flag from the data stack.
                inx
                inx

                ; For refill success, jump back up to the empty check, just in
                ; case refill gave us an empty buffer (eg. empty/blank line of
                ; input)
                bra _savechars_loop

_input_fine:
                ; There should be at least one valid char to use.
                ; Calculate it's address at CIB+TOIN into tmp1
                lda cib
                clc
                adc toin        ; LSB
                sta tmp1
                lda cib+1
                adc toin+1      ; MSB
                sta tmp1+1

                ; Get the character
                lda (tmp1)

                ; Check to see if we are handling escaped characters.
                bit tmp2
                bmi _handle_escapes    ; Only checking bit 7
                jmp _regular_char

_handle_escapes:
                ; We are handling escaped characters.  See if we have
                ; already seen the backslash.
                bit tmp2+1
                bmi _escaped
                jmp _not_escaped

_escaped:

                ; We have seen a backslash (previous character). Check to see if
                ; we are in the middle of a \x sequence (bit 6 of tmp2+1 will
                ; be clear in that case )
                bvs _check_esc_chars

                ; We are in the middle of a \x sequence. Check to see if we
                ; are on the first or second digit.
                lda #1
                bit tmp2+1
                bne _esc_x_second_digit

                ; First digit.
                inc tmp2+1  ; Adjust flag for second digit next time.
                lda (tmp1)  ; Get the char and stash it.
                pha
                jmp _next_character

_esc_x_second_digit:
                ; We are on the second hex digit of a \x sequence. Clear the
                ; escaped character flag (because we are handling it right
                ; here)
                stz tmp2+1
                lda (tmp1)
                ply                     ; recover first of pair
                jsr ascii_to_byte       ; TODO we're ignoring possible C=1 error

                bra _save_character

_check_esc_chars:
                ; Clear the escaped character flag (because we are
                ; handling it right here)
                stz tmp2+1

                ; is it character a-z ?
                cmp #'a'
                bmi _check_esc_quote
                cmp #'z'+1
                bpl _check_esc_quote
                ; check translation table
                tay
                lda escape_tr_table - 'a',y   ; fake base address to index with a-z directly
                bne _esc_replace
                tya                     ; revert if no translation
                bra _check_esc_quote

_esc_replace:   bpl _save_character     ; simple replacement
                ; handle specials with hi bit set (NUL and CR/LF)
                and #$7F                ; clear hi bit
                beq _save_character     ; NUL we can just output
                jsr cmpl_a              ; else output first char (CR)
                lda #10                 ; followed by LF
                bra _save_character

_check_esc_quote:
                cmp #'"'
                beq _save_character

                cmp #'x'
                bne _check_esc_backslash

                ; This one is difficult. We need to get the next TWO
                ; characters (which might require a refill in the middle)
                ; and combine them as two hex digits. We do this by
                ; clearing bit 6 of tmp2+1 to indicate we are in a digit
                ; and using bit 0 to keep track of which digit we are on.
                lda #%10111110        ; Clear bits 6 and 0
                sta tmp2+1
                bra _next_character

_check_esc_backslash:
                cmp #'\'
                bne _regular_char
                bra _save_character

_not_escaped:
                ; Check for the backslash to see if we should escape
                ; the next char.
                cmp #'\'
                bne _regular_char

                ; We found a backslash.  Don't save anyhing, but set
                ; a flag (in tmp2+1) to handle the next char. We don't
                ; try to get the next char here as it may require a
                ; refill of the input buffer.
                lda #$FF
                sta tmp2+1
                bra _next_character

_regular_char:
                ; Check if the current character is the end of the string.
                cmp #'"'
                beq _found_string_end

_save_character:
                ; If we didn't reach the end of the string, compile this
                ; character into the dictionary
                jsr cmpl_a

_next_character:
                ; Move on to the next character.
                inc toin
                bne _savechars_loop_longjump
                inc toin+1

_savechars_loop_longjump:
                jmp _savechars_loop

_found_string_end:
                ; Use up the delimiter.
                inc toin
                bne +
                inc toin+1
+
                ; Finally we've compiled all the string data into the dictionary
                ; We still have the start address and need the string length

                ; ( addr )
                jsr w_here
                jsr w_over
                jsr w_minus    ; HERE - addr gives string length
                ; ( addr u )

                ; What happens next depends on the state (which is bad, but
                ; that's the way it works at the moment). If we are
                ; interpreting (state=0), we're done because we've saved the string
                ; to a buffer.  (In fact we've over-delivered by compiling the string
                ; to permanent storage in the dictionary!)

                ; If we're compiling, we need to turn the string into an SLITERAL.
                ; We'll just rewind the CP to where it was when we started -
                ; five bytes before the string we've written - and let sliteral
                ; work its magic.  It'll write the five byte prologue and copy
                ; the string data onto itself (a no-op) while re-allocating the space.

                pla                     ; fetch the state flag (0 = interpret)
                beq _done

                sec                     ; rewind the CP to addr-5
                lda 2,x
                sbc #5
                sta cp
                lda 3,x
                sbc #0
                sta cp+1

                ; write the prologue, "copy" the string and reallocate the space
                jsr w_sliteral         ; ( addr u -- )

_done:
z_s_quote:      rts



escape_tr_table:
    ; 26 character translation for simple escapes
    ; 0 indicates no translation, hi bit indicates special
    .byte   7               ; a -> BEL (ASCII value 7)
    .byte   8               ; b -> Backspace (ASCII value 8)
    .byte   0,0             ; c, d no escape
    .byte   27              ; e -> ESC (ASCII value 27)
    .byte   12              ; f -> FF (ASCII value 12)
    .byte   0,0,0,0,0       ; g,h,i,j,k
    .byte   10              ; l -> LF (ASCII value 10)
    .byte   13+128          ; m -> CR/LF pair (ASCII values 13, 10)
    ; n has configurable behavior which we hard-code in the table
.if "cr" in TALI_OPTION_CR_EOL
.if "lf" in TALI_OPTION_CR_EOL
    .byte   13+128          ; n behaves like m --> cr/lf
.else
    .byte   13              ; n behaves like r --> cr
.endif
.else
    .byte   10              ; n behaves like l --> lf
.endif
    .byte   0,0             ; o,p
    .byte   34              ; q -> Double quote (ASCII value 34)
    .byte   13              ; r ->  CR (ASCII value 13)
    .byte   0               ; s
    .byte   9               ; t -> Horizontal TAB (ASCII value 9)
    .byte   0               ; u
    .byte   11              ; v -> Vertical TAB (ASCII value 11)
    .byte   0,0,0           ; w,x,y   (x is a special case later)
    .byte   0+128           ; z -> NULL (ASCII value 0)



; ## S_TO_D ( u -- d ) "Convert single cell number to double cell"
; ## "s>d"  auto  ANS core
        ; """https://forth-standard.org/standard/core/StoD"""

xt_s_to_d:
                jsr underflow_1
w_s_to_d:
                dex
                dex
                stz 0,x
                stz 1,x

                lda 3,x
                bpl _done

                ; negative, extend sign
                dec 0,x
                dec 1,x
_done:
z_s_to_d:       rts



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



; ## TICK ( "name" -- xt ) "Return a word's execution token (xt)"
; ## "'"  auto  ANS core
        ; """https://forth-standard.org/standard/core/Tick"""

xt_tick:
w_tick:
                jsr w_parse_name       ; ( -- addr u )

                ; if we got a zero, there was a problem getting the
                ; name of the word
                lda 0,x
                ora 1,x
                bne +

                lda #err_noname
                jmp error
+
                jsr w_find_name        ; ( addr u -- nt )

                ; If we didn't find the word in the Dictionary, abort
                lda 0,x
                ora 1,x
                bne +

                lda #err_syntax
                jmp error
+
                jsr w_name_to_int      ; ( nt -- xt )

z_tick:         rts



; ## TO ( n "name" -- ) or ( "name" ) "Change a value"
; ## "to"  auto  ANS core ext
        ; """https://forth-standard.org/standard/core/TO
        ; Gives a new value to a, uh, VALUE.
        ;
        ; One possible Forth
        ; implementation is  ' >BODY !  but given the problems we have
        ; with >BODY on STC Forths, we do this the hard way. Since
        ; Tali Forth uses the same code for CONSTANTs and VALUEs, you
        ; could use this to redefine a CONSTANT, but that is a no-no.
        ;
        ; Note that the standard has different behaviors for TO depending
        ; on the state (https://forth-standard.org/standard/core/TO).
        ; This makes TO state-dependent (which is bad) and also rather
        ; complex (see the Gforth implementation for comparison). This
        ; word may not be natively compiled and must be immediate. Frankly,
        ; it would have made more sense to have two words for this.
        ; """

xt_to:
w_to:
                ; One way or the other, we need the xt of the word after this
                ; one. At this point, we don't know if we are interpreted or
                ; compile, so we don't know if there is a value n on the stack,
                ; so we can't do an underflow check yet
                jsr w_tick             ; ( [n] xt )

                ; The PFA (DFA in this case) is three bytes down,
                ; after the jump to DOCONST
                lda 0,x                 ; LSB
                clc
                adc #3
                sta tmp1
                lda 1,x                 ; MSB
                adc #0                  ; we just want the carry
                sta tmp1+1

                ; Now check which state we are in
                lda state
                ora state+1
                beq _interpret

                ; Compiling, so we arrive with just ( xt ) on the stack.
                ; We need to generate code that writes a number
                ; from TOS to the address in tmp1
                ; i.e. LITERAL tmp1 !

                lda tmp1            ; replace TOS with tmp1
                sta 0,x
                lda tmp1+1
                sta 1,x

                jsr w_literal      ; generate the runtime for LITERAL tmp1

                ldy #>w_store      ; write the runtime for !
                lda #<w_store
                jsr cmpl_subroutine

                bra _done

_interpret:
                ; We're interpreting, so we arrive here with ( n xt )
                ; on the stack. This is an annoying place to put
                ; the underflow check because we can't
                ; automatically strip it out
                jsr underflow_2

                inx
                inx                     ; leaving just ( n )

                ; We skip over the jump to DOCONST and store the number
                ; in the Program Field Area (PDF, in this case more a
                ; Data Field Area
                lda 0,x
                sta (tmp1)              ; LSB

                ldy #1
                lda 1,x                 ; MSB
                sta (tmp1),y            ; fall through to common

                inx                     ; DROP
                inx
_done:
z_to:           rts



; ## TO_BODY ( xt -- addr ) "Return a word's Code Field Area (CFA)"
; ## ">body"  auto  ANS core
        ; """https://forth-standard.org/standard/core/toBODY
        ; Given a word's execution token (xt), return the address of the
        ; start of that word's parameter field (PFA). This is defined as the
        ; address that HERE would return right after CREATE.
        ;
        ; This is a difficult word for STC Forths, because most words
        ; don't actually have a Code Field Area (CFA) to skip.

        ; We solve this with a header flag in CREATE, "has CFA" (HC),
        ; so >BODY knows to skip the CFA jsr like DOVAR, DOCONST, or DODOES
        ; """

xt_to_body:
                jsr underflow_1
w_to_body:
                ; Ideally, xt already points to the CFA. We just need to check
                ; the HC flag for special cases
                jsr w_dup              ; ( xt xt )
                jsr w_int_to_name      ; ( xt nt )

                ; The status flags byte is @ NT
                lda (0,x)               ; get status byte
                and #HC
                beq _no_cfa

                ; We've got a DOVAR, DOCONST, DODEFER, DODOES or whatever,
                ; so we add three to xt, which is NOS
                clc
                lda 2,x         ; LSB
                adc #3
                sta 2,x
                bcc _no_cfa
                inc 3,x         ; MSB
_no_cfa:
                inx             ; get rid of the nt
                inx
z_to_body:      rts



; ## VALUE ( n "name" -- ) "Define a value"
; ## "value"  auto  ANS core
        ; """https://forth-standard.org/standard/core/VALUE
        ;
        ; This is a dummy header for the WORDLIST. The actual code is
        ; identical to that of CONSTANT
        ; """



; ## VARIABLE ( "name" -- ) "Define a variable"
; ## "variable"  auto  ANS core
        ; """https://forth-standard.org/standard/core/VARIABLE
        ; There are various Forth definitions for this word, such as
        ; `CREATE 1 CELLS ALLOT`  or  `CREATE 0 ,`  We use a variant of the
        ; second one so the variable is initialized to zero
        ; """
xt_variable:
w_variable:
                ; we let CREATE do the heavy lifting
                jsr w_create

                ; initialize the value to zero
                lda #0
                jsr cmpl_a
                jsr cmpl_a

z_variable:     rts



; END
