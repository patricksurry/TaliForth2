\ 65c02 inception: emulate the 65c02 CPU with TaliForth running on a 65c02...
\ This is illustrated by emulating TaliForth's own assembly code for UM*
\ Also tested via https://github.com/SingleStepTests/ProcessorTests/tree/main/wdc65c02

\ Note:  STP and WAI are treated as NOP, and decimal mode is not implemented

\ Since TaliForth is running in a 64K memory space we restrict the emulator
\ memory to a smaller memory footprint by ignoring high address bits.
\ For example setting MBITS to 12 gives 2^12 = 4K bytes of emulator memory, with
\ four ignored address bits.  This means that there are 16 ways to address each
\ actual byte.  For testing we avoid tests that address the same actual location
\ with distinct 16 bit synonyms.  Also note that the reset vector at $fffe-f
\ will always map to the last two bytes of emulator memory.

12      constant MBITS      \ simulator memory is 1 << MBITS bytes, with upper address bits ignored
$FFFE   constant RESET      \ maps to the last two emulator bytes since high bits are ignored

\ create emulator address mask and reserve its memory
1 MBITS lshift 1-       constant MMASK
here MMASK 1+ allot     constant MBASE

\ helper functions for reading and writing emulator memory
: &M    ( adr -- adr' ) MMASK AND MBASE + ;     \ emulator address to host address
: M     ( adr -- v )    &M C@ ;
: >M    ( adr v -- )    SWAP &M C! ;

\ We need to be careful with double byte access, since they need to wrap at
\ 8 or 16 bits.  So we can't just use @ to fetch two bytes since they aren't
\ always contiguous in memory.

: LSB   ( v -- byte )   $FF AND ;
: W>B   ( w -- lo hi )  DUP LSB SWAP 8 RSHIFT ;
: B>W   ( lo hi -- w )  8 LSHIFT OR ;

: MM    ( adr -- adr )  DUP M SWAP 1+ M B>W ;       \ read word from two bytes
: MMZ   ( zp -- adr )   DUP M SWAP 1+ LSB M B>W ;   \ read word from two bytes in ZP

\ Define the 16 bit program counter and some helpers

0 value PC
: >PC   ( adr -- )      TO PC ;
: PC+   ( -- adr )      PC PC 1+ >PC ;
: PC++  ( -- adr )      PC PC 1+ 1+ >PC ;

\ Reserve table of opcode XTs, with helpers to populate and emulate them

here $100 CELLS ALLOT constant OPS
: op,   ( p xt -- p' )  OVER ! 1+ 1+ ;      \ helper to write XT and inc slot
: op    ( -- )          PC+ M CELLS OPS + @  EXECUTE ;  \ emulate current opcode

\ Reserve register memory and set up access helpers

here 5 allot constant REGISTERS
0 constant #A
1 constant #X
2 constant #Y
3 constant #S               \ stack pointer (8 bit)
4 constant #P               \ flag register

: &R    ( # -- adr )    REGISTERS + ;
: R     ( # -- v )      &R C@ ;
: R!    ( v # -- )      &R C! ;
: >R    ( # v -- )      SWAP R! ;

: A     ( -- v )    #A R ;      : >A    ( v -- )    #A R! ;
: X     ( -- v )    #X R ;      : >X    ( v -- )    #X R! ;
: Y     ( -- v )    #Y R ;      : >Y    ( v -- )    #Y R! ;
: S     ( -- v )    #S R ;      : >S    ( v -- )    #S R! ;
: P     ( -- v )    #P R ;      : >P    ( v -- )    #P R! ;

\ Stack management

: PUSH  ( v -- )        S $100 + SWAP >M  S 1- >S ;
: POP   ( -- v )        S 1+ >S  S $100 + M ;
\ push/pull address bytes individually since stack can wrap
: PUSH2 ( vv -- )       W>B PUSH PUSH ;
: POP2  ( -- vv )       POP POP B>W ;

\ Flag bits

%00000001 dup   constant ^0 constant ^C	    \ Carry
%00000010 dup   constant ^1 constant ^Z	    \ Zero
%00000100 dup   constant ^2 constant ^I	    \ Interrupt
%00001000 dup   constant ^3 constant ^D	    \ Decimal
%00010000 dup   constant ^4 constant ^B	    \ Break
%00100000 dup   constant ^5 constant ^G     \ Ignored (always set)
%01000000 dup   constant ^6 constant ^V	    \ Overflow
%10000000 dup   constant ^7 constant ^N	    \ Negative

%100000000  constant ^8

: BIT?  ( v mask -- f )  AND 0<> ;           \ test if bit number is set

\ helpers to set or query flags

: F>P   ( f mask -- )   DUP INVERT P AND -ROT AND OR >P ;   \ set or clear bit number based on flag

: >Z    ( v -- )        0=      ^Z F>P ;        \ set Z by testing value for zero
: >N    ( v -- )        ^7 BIT? ^N F>P ;        \ set N by testing sign of value
: >Z>N> ( v -- v )      DUP >Z DUP >N ;         \ shortcut for both, keeping value
: >C    ( f -- )        ^C F>P ;                \ set other flags with true/false
: >V    ( f -- )        ^V F>P ;
: C?    ( -- f )        P ^C BIT? ;             \ carry flag as true/false

\ define T and >T so generic instructions can operate on either register or memory
DEFER &T    ( adr -- adr' )
: T     ( adr -- v )    &T C@ ;
: >T    ( adr v -- )    SWAP &T C! ;

: TM   ['] &M IS &T ;  \ target memory
: TR   ['] &R IS &T ;  \ target register

: SEXT  ( rel -- v )    DUP ^7 BIT? $ff00 AND OR ;  \ sign extend an 8-bit value

\ addressing modes that target a memory location or register index

: @IMPLA ( -- r )   #A                                  TR ;   \ implied A
: @IMPLX ( -- r )   #X                                  TR ;   \ implied X
: @IMPLY ( -- r )   #Y                                  TR ;   \ implied Y
: @IMM   ( -- adr ) PC+                                 TM ;   \ #dd
: @ZP    ( -- adr ) PC+     M                           TM ;   \ zp
: @ZPX   ( -- adr ) PC+     M       X + LSB             TM ;   \ zp,X
: @ZPY   ( -- adr ) PC+     M       Y + LSB             TM ;   \ zp,Y
: @ZPI   ( -- adr ) PC+     M                 MMZ       TM ;   \ (zp)
: @ZPXI  ( -- adr ) PC+     M       X + LSB   MMZ       TM ;   \ (zp,X)
: @ZPIY  ( -- adr ) PC+     M                 MMZ   Y + TM ;   \ (zp),Y
: @ABS   ( -- adr ) PC++    MM                          TM ;   \ llhh
: @ABSX  ( -- adr ) PC++    MM                      X + TM ;   \ llhh,X
: @ABSY  ( -- adr ) PC++    MM                      Y + TM ;   \ llhh,Y
: @ABSI  ( -- adr ) PC++    MM                MM        TM ;   \ (llhh)
: @ABSXI ( -- adr ) PC++    MM      X +       MM        TM ;   \ (llhh,X)
: @REL   ( -- adr ) PC+     M  SEXT PC +                TM ;   \ rr
: @ZPREL ( -- a a ) PC+     M  @REL                     TM ;   \ zp,rr

\ opcode implementations that act on a register or memory location

: %STR  ( adr r# -- )   R >T ;
: %STZ  ( adr -- )      0 >T ;

: %LDR  ( adr #r -- )   SWAP T >Z>N> >R ;
\ most transfers are expressed as load from implied register with special case for S
: %TSX  ( -- )          S  >Z>N>  >X ;
: %TXS  ( -- )          X  >S ; \ NB no flags are affected

: %INC  ( adr -- )      DUP T 1+ LSB  >Z>N>  >T ;
: %DEC  ( adr -- )      DUP T 1- LSB  >Z>N>  >T ;

: %ORA  ( adr -- )      T A OR   >Z>N>  >A ;
: %AND  ( adr -- )      T A AND  >Z>N>  >A ;
: %EOR  ( adr -- )      T A XOR  >Z>N>  >A ;

: %SMB  ( adr bit -- )  OVER T OR >T ;
: %RMB  ( adr bit -- )  INVERT OVER T AND >T ;

: %BIT  ( adr -- )      T DUP A AND >Z DUP >N DUP ^6 BIT? >V   ;
\ BIT #dd is a special case where only Z is updated
: %BIT' ( adr -- )      T A AND >Z ;
: %TRB  ( adr -- )      DUP T A 2DUP AND >Z INVERT AND >T ;
: %TSB  ( adr -- )      DUP T A 2DUP AND >Z OR >T ;

: %LSR  ( adr -- )      DUP T DUP ^0 BIT? >C 2/      >Z>N>  >T ;
: %ASL  ( adr -- )      DUP T DUP ^7 BIT? >C 2* LSB  >Z>N>  >T ;
: %ROR  ( adr -- )      C? ^7 AND OVER T DUP ^0 BIT? >C 2/ OR      >Z>N>  >T ;
: %ROL  ( adr -- )      C? ^0 AND OVER T DUP ^7 BIT? >C 2* OR LSB  >Z>N>  >T ;

: %CPR  ( adr #r -- )   R SWAP T - >Z>N> 0< INVERT >C ;

\ Note: Decimal mode not implemented for ADC/SBC

\ Use XOR trick for add/subtract to infer the carry bits from the sum S
\ Start with S = A^M^CS so CS = S^A^M and we can calculate the 6502 flags
\ via C = C8 (the addition has a carry if carry out of bit 7 is set)
\ and V = C8^C7 (overflow if the carries in and out of bit 7 differ)
: A+    ( v -- v )
    A 2DUP XOR -ROT
    ( A^B  B  A )
    + C? -              \ calculate S=A+B+C, noting C? is true == -1 when set
    TUCK XOR            \ calculate A^B^S to get C and V flags
    ( S  A^B^S )
    DUP ^8 BIT? DUP >C SWAP ^7 BIT? XOR >V
    ( S )
    LSB  >Z>N>          \ set Z and N leaving byte result
    ;
: %ADC  ( adr -- )      T A+ >A ;
\ Reframe subtraction as an addition using twos complement:
\   -M = 256 - M = 1 + 255 - M = 1 + ~M  where ~M is the inverse of M
\ Since borrow = 1 - carry we get:
\   A - M - borrow = A + ~M + 1 - borrow = A + ~M + C
: %SBC  ( adr -- )      T INVERT A+ >A ;

: %PHT  ( r -- )        T PUSH ;
: %PLT  ( r -- )        POP >Z>N> >T ;
\ the status register has special behavior
: %PHP  ( -- )          P ^B OR PUSH ;                      \ set BRK
: %PLP  ( -- )          POP >Z>N> ^G OR ^B INVERT AND >P ;  \ set IGN clr BRK

: %CLF  ( mask -- )     FALSE SWAP F>P ;
: %SEF  ( mask -- )     TRUE  SWAP F>P ;

: %JMP  ( adr -- )      >PC ;
: %JSR  ( adr -- )      PC 1- PUSH2 >PC ;
: %RTS  ( -- )          POP2 1+ >PC ;
: %RTI  ( -- )          %PLP POP2 >PC ;
: %BRK  ( adr -- )      DROP PC PUSH2 %PHP ^I %SEF RESET MM >PC ;

: ?JMP  ( adr f -- )    IF >PC ELSE DROP THEN ;

: %BFS  ( adr mask -- ) P AND    ?JMP ;
: %BFC  ( adr mask -- ) P AND 0= ?JMP ;

: %BBR  ( adr dst bit -- )  ROT T AND 0= ?JMP ;
: %BBS  ( adr dst bit -- )  ROT T AND    ?JMP ;

: %NOP  ( adr -- )      DROP ;          \ Support variable length NOPs

\ Build the opcode table, usually pairing a memory mode with an operator

OPS

:noname     @IMM        %BRK    ; op,   \ 00 BRK            ---I--
:noname	    @ZPXI	    %ORA    ; op,   \ 01 ORA (zp,X)     NZ----
:noname		@ZPI	    %NOP    ; op,   \ 02 NOP (zp)       ------
:noname     @IMPLA      %NOP    ; op,   \ 03 NOP            ------
:noname		@ZP	        %TSB    ; op,   \ 04 TSB zp         -Z----
:noname		@ZP	        %ORA    ; op,   \ 05 ORA zp         NZ----
:noname		@ZP	        %ASL    ; op,   \ 06 ASL zp         NZC---
:noname     @ZP     ^0  %RMB    ; op,   \ 07 RMB0 zp        ------
:noname                 %PHP    ; op,   \ 08 PHP            ------
:noname		@IMM	    %ORA    ; op,   \ 09 ORA #dd        NZ----
:noname     @IMPLA      %ASL    ; op,   \ 0A ASL A          NZC---
:noname     @IMPLA      %NOP    ; op,   \ 0B NOP            ------
:noname		@ABS	    %TSB    ; op,   \ 0C TSB llhh       -Z----
:noname		@ABS	    %ORA    ; op,   \ 0D ORA llhh       NZ----
:noname		@ABS	    %ASL    ; op,   \ 0E ASL llhh       NZC---
:noname     @ZPREL  ^0  %BBR    ; op,   \ 0F BBR0 zp,rr     ------
:noname		@REL    ^N  %BFC    ; op,   \ 10 BPL rr         ------
:noname		@ZPIY	    %ORA    ; op,   \ 11 ORA (zp),Y     NZ----
:noname		@ZPI	    %ORA    ; op,   \ 12 ORA (zp)       NZ----
:noname     @IMPLA      %NOP    ; op,   \ 13 NOP            ------
:noname		@ZP	        %TRB    ; op,   \ 14 TRB zp         -Z----
:noname		@ZPX	    %ORA    ; op,   \ 15 ORA zp,X       NZ----
:noname		@ZPX	    %ASL    ; op,   \ 16 ASL zp,X       NZC---
:noname     @ZP     ^1  %RMB    ; op,   \ 17 RMB1 zp        ------
:noname             ^C  %CLF    ; op,   \ 18 CLC            --C---
:noname		@ABSY	    %ORA    ; op,   \ 19 ORA llhh,Y     NZ----
:noname     @IMPLA      %INC    ; op,   \ 1A INC A          NZ----
:noname     @IMPLA      %NOP    ; op,   \ 1B NOP            ------
:noname		@ABS	    %TRB    ; op,   \ 1C TRB llhh       -Z----
:noname		@ABSX	    %ORA    ; op,   \ 1D ORA llhh,X     NZ----
:noname		@ABSX	    %ASL    ; op,   \ 1E ASL llhh,X     NZC---
:noname     @ZPREL  ^1  %BBR    ; op,   \ 1F BBR1 zp,rr     ------
:noname		@ABS	    %JSR    ; op,   \ 20 JSR x16        ------
:noname	    @ZPXI	    %AND    ; op,   \ 21 AND (zp,X)     NZ----
:noname		@ZPI	    %NOP    ; op,   \ 22 NOP (zp)       ------
:noname     @IMPLA      %NOP    ; op,   \ 23 NOP            ------
:noname		@ZP	        %BIT    ; op,   \ 24 BIT zp         NZ---V
:noname		@ZP	        %AND    ; op,   \ 25 AND zp         NZ----
:noname		@ZP	        %ROL    ; op,   \ 26 ROL zp         NZC---
:noname     @ZP     ^2  %RMB    ; op,   \ 27 RMB2 zp        ------
:noname                 %PLP    ; op,   \ 28 PLP            NZCIDV
:noname		@IMM	    %AND    ; op,   \ 29 AND #dd        NZ----
:noname     @IMPLA      %ROL    ; op,   \ 2A ROL A          NZC---
:noname     @IMPLA      %NOP    ; op,   \ 2B NOP            ------
:noname		@ABS	    %BIT    ; op,   \ 2C BIT llhh       NZ---V
:noname		@ABS	    %AND    ; op,   \ 2D AND llhh       NZ----
:noname		@ABS	    %ROL    ; op,   \ 2E ROL llhh       NZC---
:noname     @ZPREL  ^2  %BBR    ; op,   \ 2F BBR2 zp,rr     ------
:noname		@REL    ^N  %BFS    ; op,   \ 30 BMI rr         ------
:noname		@ZPIY	    %AND    ; op,   \ 31 AND (zp),Y     NZ----
:noname		@ZPI	    %AND    ; op,   \ 32 AND (zp)       NZ----
:noname     @IMPLA      %NOP    ; op,   \ 33 NOP            ------
:noname		@ZPX	    %BIT    ; op,   \ 34 BIT zp,X       NZ---V
:noname		@ZPX	    %AND    ; op,   \ 35 AND zp,X       NZ----
:noname		@ZPX	    %ROL    ; op,   \ 36 ROL zp,X       NZC---
:noname     @ZP     ^3  %RMB    ; op,   \ 37 RMB3 zp        ------
:noname             ^C  %SEF    ; op,   \ 38 SEC            --C---
:noname		@ABSY	    %AND    ; op,   \ 39 AND llhh,Y     NZ----
:noname     @IMPLA      %DEC    ; op,   \ 3A DEC A          NZ----
:noname     @IMPLA      %NOP    ; op,   \ 3B NOP            ------
:noname		@ABSX	    %BIT    ; op,   \ 3C BIT llhh,X     NZ---V
:noname		@ABSX	    %AND    ; op,   \ 3D AND llhh,X     NZ----
:noname		@ABSX	    %ROL    ; op,   \ 3E ROL llhh,X     NZC---
:noname     @ZPREL  ^3  %BBR    ; op,   \ 3F BBR3 zp,rr     ------
:noname                 %RTI    ; op,   \ 40 RTI            NZCIDV
:noname	    @ZPXI	    %EOR    ; op,   \ 41 EOR (zp,X)     NZ----
:noname		@ZPI	    %NOP    ; op,   \ 42 NOP (zp)       ------
:noname     @IMPLA      %NOP    ; op,   \ 43 NOP            ------
:noname		@ZP	        %NOP    ; op,   \ 44 NOP zp         ------
:noname		@ZP	        %EOR    ; op,   \ 45 EOR zp         NZ----
:noname		@ZP	        %LSR    ; op,   \ 46 LSR zp         NZC---
:noname     @ZP     ^4  %RMB    ; op,   \ 47 RMB4 zp        ------
:noname     @IMPLA      %PHT    ; op,   \ 48 PHA            ------
:noname		@IMM	    %EOR    ; op,   \ 49 EOR #dd        NZ----
:noname     @IMPLA      %LSR    ; op,   \ 4A LSR A          NZC---
:noname     @IMPLA      %NOP    ; op,   \ 4B NOP            ------
:noname		@ABS	    %JMP    ; op,   \ 4C JMP x16        ------
:noname		@ABS	    %EOR    ; op,   \ 4D EOR llhh       NZ----
:noname		@ABS	    %LSR    ; op,   \ 4E LSR llhh       NZC---
:noname     @ZPREL  ^4  %BBR    ; op,   \ 4F BBR4 zp,rr     ------
:noname		@REL    ^V  %BFC    ; op,   \ 50 BVC rr         ------
:noname		@ZPIY	    %EOR    ; op,   \ 51 EOR (zp),Y     NZ----
:noname		@ZPI	    %EOR    ; op,   \ 52 EOR (zp)       NZ----
:noname     @IMPLA      %NOP    ; op,   \ 53 NOP            ------
:noname		@ZPX	    %NOP    ; op,   \ 54 NOP zp,X       ------
:noname		@ZPX	    %EOR    ; op,   \ 55 EOR zp,X       NZ----
:noname		@ZPX	    %LSR    ; op,   \ 56 LSR zp,X       NZC---
:noname     @ZP     ^5  %RMB    ; op,   \ 57 RMB5 zp        ------
:noname             ^I  %CLF    ; op,   \ 58 CLI            ---I--
:noname		@ABSY	    %EOR    ; op,   \ 59 EOR llhh,Y     NZ----
:noname     @IMPLY      %PHT    ; op,   \ 5A PHY            ------
:noname     @IMPLA      %NOP    ; op,   \ 5B NOP            ------
:noname		@ABSX	    %NOP    ; op,   \ 5C NOP llhh,X     ------
:noname		@ABSX	    %EOR    ; op,   \ 5D EOR llhh,X     NZ----
:noname		@ABSX	    %LSR    ; op,   \ 5E LSR llhh,X     NZC---
:noname     @ZPREL  ^5  %BBR    ; op,   \ 5F BBR5 zp,rr     ------
:noname                 %RTS    ; op,   \ 60 RTS            ------
:noname	    @ZPXI	    %ADC    ; op,   \ 61 ADC (zp,X)     NZC--V
:noname		@ZPI	    %NOP    ; op,   \ 62 NOP (zp)       ------
:noname     @IMPLA      %NOP    ; op,   \ 63 NOP            ------
:noname		@ZP	        %STZ    ; op,   \ 64 STZ zp         ------
:noname		@ZP	        %ADC    ; op,   \ 65 ADC zp         NZC--V  (no decimal mode)
:noname		@ZP	        %ROR    ; op,   \ 66 ROR zp         NZC---
:noname     @ZP     ^6  %RMB    ; op,   \ 67 RMB6 zp        ------
:noname     @IMPLA      %PLT    ; op,   \ 68 PLA            NZ----
:noname		@IMM	    %ADC    ; op,   \ 69 ADC #dd        NZC--V
:noname     @IMPLA      %ROR    ; op,   \ 6A ROR A          NZC---
:noname     @IMPLA      %NOP    ; op,   \ 6B NOP            ------
:noname		@ABSI	    %JMP    ; op,   \ 6C JMP (llhh)     ------
:noname		@ABS	    %ADC    ; op,   \ 6D ADC llhh       NZC--V
:noname		@ABS	    %ROR    ; op,   \ 6E ROR llhh       NZC---
:noname     @ZPREL  ^6  %BBR    ; op,   \ 6F BBR6 zp,rr     ------
:noname		@REL    ^V  %BFS    ; op,   \ 70 BVS rr         ------
:noname		@ZPIY	    %ADC    ; op,   \ 71 ADC (zp),Y     NZC--V
:noname		@ZPI	    %ADC    ; op,   \ 72 ADC (zp)       NZC--V
:noname     @IMPLA      %NOP    ; op,   \ 73 NOP            ------
:noname		@ZPX	    %STZ    ; op,   \ 74 STZ zp,X       ------
:noname		@ZPX	    %ADC    ; op,   \ 75 ADC zp,X       NZC--V
:noname		@ZPX	    %ROR    ; op,   \ 76 ROR zp,X       NZC---
:noname     @ZP     ^7  %RMB    ; op,   \ 77 RMB7 zp        ------
:noname             ^I  %SEF    ; op,   \ 78 SEI            ---I--
:noname		@ABSY	    %ADC    ; op,   \ 79 ADC llhh,Y     NZC--V
:noname     @iMPLY      %PLT    ; op,   \ 7A PLY            NZ----
:noname     @IMPLA      %NOP    ; op,   \ 7B NOP            ------
:noname		@ABSXI	    %JMP    ; op,   \ 7C JMP (llhh,X)   ------
:noname		@ABSX	    %ADC    ; op,   \ 7D ADC llhh,X     NZC--V
:noname		@ABSX	    %ROR    ; op,   \ 7E ROR llhh,X     NZC---
:noname     @ZPREL  ^7  %BBR    ; op,   \ 7F BBR7 zp,rr     ------
:noname		@REL	    %JMP    ; op,   \ 80 BRA rr         ------
:noname		@ZPXI	#A  %STR    ; op,   \ 81 STA (zp,X)     ------
:noname		@ZPI	    %NOP    ; op,   \ 82 NOP (zp)       ------
:noname     @IMPLA      %NOP    ; op,   \ 83 NOP            ------
:noname		@ZP	    #Y  %STR    ; op,   \ 84 STY zp         ------
:noname		@ZP	    #A  %STR    ; op,   \ 85 STA zp         ------
:noname		@ZP	    #X  %STR    ; op,   \ 86 STX zp         ------
:noname     @ZP     ^0  %SMB    ; op,   \ 87 SMB0 zp        ------
:noname     @IMPLY      %DEC    ; op,   \ 88 DEY            NZ----
:noname		@IMM	    %BIT'   ; op,   \ 89 BIT #dd        -Z----  (special case)
:noname     @IMPLX  #A  %LDR    ; op,   \ 8A TXA            NZ----
:noname     @IMPLA      %NOP    ; op,   \ 8B NOP            ------
:noname		@ABS	#Y  %STR    ; op,   \ 8C STY llhh       ------
:noname		@ABS	#A  %STR    ; op,   \ 8D STA llhh       ------
:noname		@ABS	#X  %STR    ; op,   \ 8E STX llhh       ------
:noname     @ZPREL  ^0  %BBS    ; op,   \ 8F BBS0 zp,rr     ------
:noname		@REL    ^C  %BFC    ; op,   \ 90 BCC rr         ------
:noname		@ZPIY	#A  %STR    ; op,   \ 91 STA (zp),Y     ------
:noname		@ZPI	#A  %STR    ; op,   \ 92 STA (zp)       ------
:noname     @IMPLA      %NOP    ; op,   \ 93 NOP            ------
:noname		@ZPX	#Y  %STR    ; op,   \ 94 STY zp,X       ------
:noname		@ZPX	#A  %STR    ; op,   \ 95 STA zp,X       ------
:noname		@ZPY	#X  %STR    ; op,   \ 96 STX zp,Y       ------
:noname     @ZP     ^1  %SMB    ; op,   \ 97 SMB1 zp        ------
:noname     @IMPLY  #A  %LDR    ; op,   \ 98 TYA            NZ----
:noname		@ABSY	#A  %STR    ; op,   \ 99 STA llhh,Y     ------
:noname                 %TXS    ; op,   \ 9A TXS            ------
:noname     @IMPLA      %NOP    ; op,   \ 9B NOP            ------
:noname		@ABS	    %STZ    ; op,   \ 9C STZ llhh       ------
:noname		@ABSX	#A  %STR    ; op,   \ 9D STA llhh,X     ------
:noname		@ABSX	    %STZ    ; op,   \ 9E STZ llhh,X     ------
:noname     @ZPREL  ^1  %BBS    ; op,   \ 9F BBS1 zp,rr     ------
:noname		@IMM	#Y  %LDR    ; op,   \ A0 LDY #dd        NZ----
:noname		@ZPXI	#A  %LDR    ; op,   \ A1 LDA (zp,X)     NZ----
:noname		@IMM	#X  %LDR    ; op,   \ A2 LDX #dd        NZ----
:noname     @IMPLA      %NOP    ; op,   \ A3 NOP            ------
:noname		@ZP	    #Y  %LDR    ; op,   \ A4 LDY zp         NZ----
:noname		@ZP	    #A  %LDR    ; op,   \ A5 LDA zp         NZ----
:noname		@ZP	    #X  %LDR    ; op,   \ A6 LDX zp         NZ----
:noname     @ZP     ^2  %SMB    ; op,   \ A7 SMB2 zp        ------
:noname     @IMPLA  #Y  %LDR    ; op,   \ A8 TAY            NZ----
:noname		@IMM	#A  %LDR    ; op,   \ A9 LDA #dd        NZ----
:noname     @IMPLA  #X  %LDR    ; op,   \ AA TAX            NZ----
:noname     @IMPLA      %NOP    ; op,   \ AB NOP            ------
:noname		@ABS	#Y  %LDR    ; op,   \ AC LDY llhh       NZ----
:noname		@ABS	#A  %LDR    ; op,   \ AD LDA llhh       NZ----
:noname		@ABS	#X  %LDR    ; op,   \ AE LDX llhh       NZ----
:noname     @ZPREL  ^2  %BBS    ; op,   \ AF BBS2 zp,rr     ------
:noname		@REL    ^C  %BFS    ; op,   \ B0 BCS rr         ------
:noname		@ZPIY	#A  %LDR    ; op,   \ B1 LDA (zp),Y     NZ----
:noname		@ZPI	#A  %LDR    ; op,   \ B2 LDA (zp)       NZ----
:noname     @IMPLA      %NOP    ; op,   \ B3 NOP            ------
:noname		@ZPX	#Y  %LDR    ; op,   \ B4 LDY zp,X       NZ----
:noname		@ZPX	#A  %LDR    ; op,   \ B5 LDA zp,X       NZ----
:noname		@ZPY	#X  %LDR    ; op,   \ B6 LDX zp,Y       NZ----
:noname     @ZP     ^3  %SMB    ; op,   \ B7 SMB3 zp        ------
:noname             ^V  %CLF    ; op,   \ B8 CLV            -----V
:noname		@ABSY	#A  %LDR    ; op,   \ B9 LDA llhh,Y     NZ----
:noname                 %TSX    ; op,   \ BA TSX            NZ----
:noname     @IMPLA      %NOP    ; op,   \ BB NOP            ------
:noname		@ABSX	#Y  %LDR    ; op,   \ BC LDY llhh,X     NZ----
:noname		@ABSX	#A  %LDR    ; op,   \ BD LDA llhh,X     NZ----
:noname		@ABSY	#X  %LDR    ; op,   \ BE LDX llhh,Y     NZ----
:noname     @ZPREL  ^3  %BBS    ; op,   \ BF BBS3 zp,rr     ------
:noname		@IMM	#Y  %CPR    ; op,   \ C0 CPY #dd        NZC---
:noname	    @ZPXI	#A  %CPR    ; op,   \ C1 CMP (zp,X)     NZC---
:noname		@ZPI	    %NOP    ; op,   \ C2 NOP (zp)       ------
:noname     @IMPLA      %NOP    ; op,   \ C3 NOP            ------
:noname		@ZP	    #Y  %CPR    ; op,   \ C4 CPY zp         NZC---
:noname		@ZP	    #A  %CPR    ; op,   \ C5 CMP zp         NZC---
:noname		@ZP	        %DEC    ; op,   \ C6 DEC zp         NZ----
:noname     @ZP     ^4  %SMB    ; op,   \ C7 SMB4 zp        ------
:noname     @IMPLY      %INC    ; op,   \ C8 INY            NZ----
:noname		@IMM	#A  %CPR    ; op,   \ C9 CMP #dd        NZC---
:noname     @IMPLX      %DEC    ; op,   \ CA DEX            NZ----
:noname     @IMPLA      %NOP    ; op,   \ CB WAI            ------  (not implemented)
:noname		@ABS	#Y  %CPR    ; op,   \ CC CPY llhh       NZC---
:noname		@ABS	#A  %CPR    ; op,   \ CD CMP llhh       NZC---
:noname		@ABS	    %DEC    ; op,   \ CE DEC llhh       NZ----
:noname     @ZPREL  ^4  %BBS    ; op,   \ CF BBS4 zp,rr     ------
:noname		@REL    ^Z  %BFC    ; op,   \ D0 BNE rr         ------
:noname		@ZPIY	#A  %CPR    ; op,   \ D1 CMP (zp),Y     NZC---
:noname		@ZPI	#A  %CPR    ; op,   \ D2 CMP (zp)       NZC---
:noname     @IMPLA      %NOP    ; op,   \ D3 NOP            ------
:noname		@ZPX	    %NOP    ; op,   \ D4 NOP zp,X       ------
:noname		@ZPX	#A  %CPR    ; op,   \ D5 CMP zp,X       NZC---
:noname		@ZPX	    %DEC    ; op,   \ D6 DEC zp,X       NZ----
:noname     @ZP     ^5  %SMB    ; op,   \ D7 SMB5 zp        ------
:noname             ^D  %CLF    ; op,   \ D8 CLD            ----D-
:noname		@ABSY	#A  %CPR    ; op,   \ D9 CMP llhh,Y     NZC---
:noname     @IMPLX      %PHT    ; op,   \ DA PHX            ------
:noname     @IMPLA      %NOP    ; op,   \ DB STP            ------  (not implemented)
:noname		@ABS	    %NOP    ; op,   \ DC NOP llhh       ------
:noname		@ABSX	#A  %CPR    ; op,   \ DD CMP llhh,X     NZC---
:noname		@ABSX	    %DEC    ; op,   \ DE DEC llhh,X     NZ----
:noname     @ZPREL  ^5  %BBS    ; op,   \ DF BBS5 zp,rr     ------
:noname		@IMM	#X  %CPR    ; op,   \ E0 CPX #dd        NZC---
:noname	    @ZPXI	    %SBC    ; op,   \ E1 SBC (zp,X)     NZC--V  (no decimal mode)
:noname		@ZPI	    %NOP    ; op,   \ E2 NOP (zp)       ------
:noname     @IMPLA      %NOP    ; op,   \ E3 NOP            ------
:noname		@ZP	    #X  %CPR    ; op,   \ E4 CPX zp         NZC---
:noname		@ZP	        %SBC    ; op,   \ E5 SBC zp         NZC--V
:noname		@ZP	        %INC    ; op,   \ E6 INC zp         NZ----
:noname     @ZP     ^6  %SMB    ; op,   \ E7 SMB6 zp        ------
:noname     @IMPLX      %INC    ; op,   \ E8 INX            NZ----
:noname		@IMM	    %SBC    ; op,   \ E9 SBC #dd        NZC--V
:noname     @IMPLA      %NOP    ; op,   \ EA NOP            ------
:noname     @IMPLA      %NOP    ; op,   \ EB NOP            ------
:noname		@ABS    #X  %CPR    ; op,   \ EC CPX llhh       NZC---
:noname		@ABS	    %SBC    ; op,   \ ED SBC llhh       NZC--V
:noname		@ABS	    %INC    ; op,   \ EE INC llhh       NZ----
:noname     @ZPREL  ^6  %BBS    ; op,   \ EF BBS6 zp,rr     ------
:noname		@REL    ^Z  %BFS    ; op,   \ F0 BEQ rr         ------
:noname		@ZPIY	    %SBC    ; op,   \ F1 SBC (zp),Y     NZC--V
:noname		@ZPI	    %SBC    ; op,   \ F2 SBC (zp)       NZC--V
:noname     @IMPLA      %NOP    ; op,   \ F3 NOP            ------
:noname		@ZPX	    %NOP    ; op,   \ F4 NOP zp,X       ------
:noname		@ZPX	    %SBC    ; op,   \ F5 SBC zp,X       NZC--V
:noname		@ZPX	    %INC    ; op,   \ F6 INC zp,X       NZ----
:noname     @ZP     ^7  %SMB    ; op,   \ F7 SMB7 zp        ------
:noname             ^D  %SEF    ; op,   \ F8 SED            ----D-
:noname		@ABSY	    %SBC    ; op,   \ F9 SBC llhh,Y     NZC--V
:noname     @IMPLX      %PLT    ; op,   \ FA PLX            NZ----
:noname     @IMPLA      %NOP    ; op,   \ FB NOP            ------
:noname		@ABS	    %NOP    ; op,   \ FC NOP llhh       ------
:noname		@ABSX	    %SBC    ; op,   \ FD SBC llhh,X     NZC--V
:noname		@ABSX	    %INC    ; op,   \ FE INC llhh,X     NZ----
:noname     @ZPREL  ^7  %BBS    ; op,   \ FF BBS7 zp,rr     ------

drop    \ done with the OPS table pointer we've been incrementing

\ As a demo, let's emulate Tali's UM* which is position-independent

: demo ( -- )
    \ Copy the code (including the RTS) to emulator address $400
    \ The code is position-independent (no jumps) but there's
    \ an initial bounds check JSR that we'll avoid below
    ['] um* DUP INT>NAME WORDSIZE 1+ ( xt u )
    $400 &M SWAP MOVE

    \ Initalize the emulator PC (skip stack check) and stack pointers
    $403 >PC  $fc >X  $ff >S
    \ Set up the multiplication by placing args on the emulator's data stack
    2017 $fc &M !  2027 $fe &M !

    \ Emulate op codes until we see RTS
    ." emulating UM* code ... "
    BEGIN op PC M $60 = UNTIL
    \ Share the good news
    ." 2017 * 2027 = " X &M 2@ ud.
;
demo
