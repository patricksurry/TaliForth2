\ simulator will use memory from M0 .. M0 + 1<<MSZ-1
\ address bits higher than MSZ are ignored, e.g. reset vectors
\ will always map to the last bytes of memory range

$4000   constant M0         \ base memory location
12      constant MSZ        \ simulator memory is 1 << MSZ bits (upper address bits ignored)

0 VALUE A
0 VALUE X
0 VALUE Y
0 VALUE S                   \ stack pointer
0 VALUE P                   \ flag register

0 VALUE PC                  \ 16 bit program counter

: BIT   ( bit -- mask )     1 SWAP LSHIFT ;

\ Flag bits

7 constant #N	    \ Negative
6 constant #V	    \ Overflow
5 constant #G       \ Ignored (though see PHP/PLP)
4 constant #B	    \ Break
3 constant #D	    \ Decimal
2 constant #I	    \ Interrupt
1 constant #Z	    \ Zero
0 constant #C	    \ Carry

: byte  ( v -- byte )   $ff and ;
: >A    ( v -- )        byte TO A ;
: >X    ( v -- )        byte TO X ;
: >Y    ( v -- )        byte TO Y ;
: >S    ( v -- )        byte TO S ;
: >P    ( v -- )        byte TO P ;
: >PC   ( adr -- )      TO PC ;
: >#P   ( f bit -- )    BIT dup invert P and -rot and or TO P ;
: BIT?  ( v bit -- f )  BIT AND 0<> ;
: ^Z    ( v -- v )      DUP 0=      #Z >#P ;
: ^N    ( v -- v )      DUP 7 BIT?  #N >#P ;
: >C    ( f -- )        #C >#P ;
: >V    ( f -- )        #V >#P ;
: >I    ( f -- )        #I >#P ;
: >D    ( f -- )        #D >#P ;
: C?    ( -- f )        P #C BIT? ;
: N?    ( -- f )        P #N BIT? ;
: V?    ( -- f )        P #V BIT? ;
: Z?    ( -- f )        P #Z BIT? ;
: PC+   ( -- adr )      PC PC 1+ >PC ;
: PC++  ( -- adr )      PC PC 1+ 1+ >PC ;

: sext  ( rel -- v )    DUP 7 BIT? $ff00 AND OR ;  \ sign extend an 8-bit value

1 MSZ lshift 1- constant MMASK

: &M    ( adr -- adr' ) MMASK AND M0 + ;
: M     ( adr -- v )    &M C@ ;
: MM    ( adr -- adr )  &M @ ;
: >M    ( adr v -- )    swap &M C! ;
: >MM   ( adr vv -- )   swap &M ! ;

: D     ( adr' -- v )   C@ ;
: >D    ( adr' v -- )   C! ;

\ addressing modes
\ each consumes operands from PC and resolves target address
: @IMM   ( -- adr )  PC+                                 ;   \ #dd
: @ZP    ( -- adr )  PC+     M                           ;   \ zp
: @ZPX   ( -- adr )  PC+     M       X + byte            ;   \ zp,X
: @ZPY   ( -- adr )  PC+     M       Y + byte            ;   \ zp,Y
: @ZPI   ( -- adr )  PC+     M                 MM        ;   \ (zp)
: @ZPXI  ( -- adr )  PC+     M       X + byte  MM        ;   \ (zp,X)
: @ZPIY  ( -- adr )  PC+     M                 MM    Y + ;   \ (zp),Y
: @ABS   ( -- adr )  PC++    MM                          ;   \ llhh
: @ABSX  ( -- adr )  PC++    MM                      X + ;   \ llhh,X
: @ABSY  ( -- adr )  PC++    MM                      Y + ;   \ llhh,Y
: @ABSI  ( -- adr )  PC++    MM                MM        ;   \ (llhh)
: @ABSXI ( -- adr )  PC++    MM      X +       MM        ;   \ (llhh,X)
: @REL   ( -- adr )  PC+     M sext  PC +                ;   \ rr

\ TODO
\ to unify immediate mode instructions we could write
\ variable &A  : A &A c@ ; : >A &A c! ; \ &A is the address of a register
\ 0 value &D   : D &D c@ ; : >D &D c! ; \ &D is an address shadowing memory or register
\ : &M ( adr -- ) MMASK AND M0 + ;
\ then  &A to &D  or  &M to &D  and we can write generic ops using D

: %ORA  ( adr -- )  M A OR   ^Z ^N  >A ;
: %AND  ( adr -- )  M A AND  ^Z ^N  >A ;
: %EOR  ( adr -- )  M A XOR  ^Z ^N  >A ;

: %BIT  ( adr -- )  M ^N DUP 6 BIT? >V A AND ^Z DROP ;
: %BIT' ( adr -- )  M A AND ^Z DROP ;       \ BIT #dd special case
: %TRB  ( adr -- )  DUP M A 2DUP AND ^Z DROP INVERT AND >M ;
: %TSB  ( adr -- )  DUP M A 2DUP AND ^Z DROP OR >M ;


: %INC  ( adr -- )  DUP M 1+ ^Z ^N  >M ;
: %DEC  ( adr -- )  DUP M 1- ^Z ^N  >M ;
: %INA  ( -- )      A 1+ ^Z ^N >A ;
: %DEA  ( -- )      A 1- ^Z ^N >A ;
: %INX  ( -- )      X 1+ ^Z ^N >X ;
: %DEX  ( -- )      X 1- ^Z ^N >X ;
: %INY  ( -- )      Y 1+ ^Z ^N >Y ;
: %DEY  ( -- )      Y 1- ^Z ^N >Y ;

: %LDA  ( adr -- )  M  ^Z ^N  >A ;
: %STA  ( adr -- )  A >M ;
: %LDX  ( adr -- )  M  ^Z ^N  >X ;
: %STX  ( adr -- )  X >M ;
: %LDY  ( adr -- )  M  ^Z ^N  >Y ;
: %STY  ( adr -- )  Y >M ;
: %STZ  ( adr -- )  0 >M ;

: %LSRA ( -- )      A DUP 0 BIT? >C 2/ ^Z ^N >A ;
: %ASLA ( -- )      A DUP 7 BIT? >C 2* ^Z ^N >A ;
: %RORA ( -- )      C? 7 BIT AND A DUP 0 BIT? >C 2/ OR ^Z ^N >A ;
: %ROLA ( -- )      C? 0 BIT AND A DUP 7 BIT? >C 2* OR ^Z ^N >A ;
: %LSR  ( adr -- )  DUP M DUP 0 BIT? >C 2/ ^Z ^N >M ;
: %ASL  ( adr -- )  DUP M DUP 7 BIT? >C 2* ^Z ^N >M ;
: %ROR  ( adr -- )  C? 7 BIT AND OVER M DUP 0 BIT? >C 2/ OR ^Z ^N >M ;
: %ROL  ( adr -- )  C? 0 BIT AND OVER M DUP 7 BIT? >C 2* OR ^Z ^N >M ;

: %CMP  ( adr -- )  A SWAP M - ^Z ^N 0< INVERT >C ;
: %CPX  ( adr -- )  X SWAP M - ^Z ^N 0< INVERT >C ;
: %CPY  ( adr -- )  Y SWAP M - ^Z ^N 0< INVERT >C ;
\ For addition, S = A^M^Cs, so Cs = S^A^M, where V = C8^C7, C=C8
: A+    ( v -- )
    A 2DUP XOR -ROT + C? - ^Z ^N DUP >A
    XOR ( A^B^S ) DUP 8 BIT? DUP >C SWAP 7 BIT? XOR >V ;
: %ADC  ( adr -- )  M A+ ;
\ Reframe subtraction as addition of twos complement
\ using -M = 256 - M = 1 + 255 - M = 1 + ~M and borrow = 1-C
\ A - M - borrow = A + ~M + 1 - borrow = A + ~M + C
: %SBC  ( adr -- )  M INVERT A+ ;

: PUSH  ( v -- )    S $100 + SWAP >M  S 1- >S ;
: POP   ( -- v )    S 1+ >S  S $100 + M  ^Z ^N ;
: PUSH2 ( adr -- )  S $FF + SWAP >MM  S 1- 1- >S ;
: POP2  ( -- adr )  S 1+ 1+ >S  S $FF + MM ;   \ no flags

: %PHA  ( -- )      A PUSH ;
: %PLA  ( -- )      POP >A ;
: %PHX  ( -- )      X PUSH ;
: %PLX  ( -- )      POP >X ;
: %PHY  ( -- )      Y PUSH ;
: %PLY  ( -- )      POP >Y ;
: %PHP  ( -- )      P #G BIT OR #B BIT OR PUSH ;
: %PLP  ( -- )      POP #G BIT #B BIT OR INVERT AND >P ;

: %TAX  ( -- )      A ^Z ^N >X ;
: %TXA  ( -- )      X ^Z ^N >A ;
: %TAY  ( -- )      A ^Z ^N >Y ;
: %TYA  ( -- )      Y ^Z ^N >A ;
: %TSX  ( -- )      S ^Z ^N >X ;
: %TXS  ( -- )      X ^Z ^N >S ;

: %CLC  ( -- )      false >C ;
: %SEC  ( -- )      true  >C ;
: %CLD  ( -- )      false >D ;
: %SED  ( -- )      true  >D ;
: %CLI  ( -- )      false >I ;
: %SEI  ( -- )      true  >I ;
: %CLV  ( -- )      false >V ;

: %JMP  ( adr -- )  >PC ;
: %JSR  ( adr -- )  PC 1- PUSH2 >PC ;
: %RTS  ( -- )      POP2 1+ >PC ;
: %RTI  ( -- )      POP >P POP2 >PC ;
: %BRK  ( adr -- )  DROP PC PUSH2 %PHP $FFFE MM >PC ;

: ?JMP  ( adr f -- ) IF >PC ELSE DROP THEN ;
: %BPL  ( adr -- )  N? INVERT ?JMP ;
: %BMI  ( adr -- )  N? ?JMP ;
: %BVC  ( adr -- )  V? INVERT ?JMP ;
: %BVS  ( adr -- )  V? ?JMP ;
: %BCC  ( adr -- )  C? INVERT ?JMP ;
: %BCS  ( adr -- )  C? ?JMP ;
: %BNE  ( adr -- )  Z? INVERT ?JMP ;
: %BEQ  ( adr -- )  Z? ?JMP ;

: NOOP ;
: %NOP  ( adr -- )  drop ;


\ reserve space for a table of 256 xts, one for each opcode
here $100 CELLS ALLOT constant OPTBL

\ helper to write an xt to the next free slot
: op, ( p xt -- p' ) over ! 1+ 1+ ;

\ helper to execute the current opcode
: op    PC+ M CELLS OPTBL + @ ?DUP IF EXECUTE THEN ;


\ not implemented: WAI, STP, RMB#, SMB#, BBR#, BBS# ($.7 and $.f)


OPTBL

\ define each opcode, combining a memory mode and operator
:noname     @IMM    %BRK    ; op,   \ 00 BRK            ---I--
:noname	    @ZPXI	%ORA    ; op,   \ 01 ORA (zp,X)     NZ----
:noname		@ZPI	%NOP    ; op,   \ 02 NOP (zp)       ------
:noname             NOOP    ; op,   \ 03 NOP            ------
:noname		@ZP	    %TSB    ; op,   \ 04 TSB zp         -Z----
:noname		@ZP	    %ORA    ; op,   \ 05 ORA zp         NZ----
:noname		@ZP	    %ASL    ; op,   \ 06 ASL zp         NZC---
0 op,                               \ 07 RMB0 zp        ------
:noname             %PHP    ; op,   \ 08 PHP            ------
:noname		@IMM	%ORA    ; op,   \ 09 ORA #dd        NZ----
:noname             %ASLA   ; op,   \ 0A ASL A          NZC---
:noname             NOOP    ; op,   \ 0B NOP            ------
:noname		@ABS	%TSB    ; op,   \ 0C TSB llhh       -Z----
:noname		@ABS	%ORA    ; op,   \ 0D ORA llhh       NZ----
:noname		@ABS	%ASL    ; op,   \ 0E ASL llhh       NZC---
0 op,                               \ 0F BBR0 zp,rr     ------
:noname		@REL	%BPL    ; op,   \ 10 BPL rr         ------
:noname		@ZPIY	%ORA    ; op,   \ 11 ORA (zp),Y     NZ----
:noname		@ZPI	%ORA    ; op,   \ 12 ORA (zp)       NZ----
:noname             NOOP    ; op,   \ 13 NOP            ------
:noname		@ZP	    %TRB    ; op,   \ 14 TRB zp         -Z----
:noname		@ZPX	%ORA    ; op,   \ 15 ORA zp,X       NZ----
:noname		@ZPX	%ASL    ; op,   \ 16 ASL zp,X       NZC---
0 op,                               \ 17 RMB1 zp        ------
:noname             %CLC    ; op,   \ 18 CLC            --C---
:noname		@ABSY	%ORA    ; op,   \ 19 ORA llhh,Y     NZ----
:noname             %INA    ; op,   \ 1A INC A          NZ----
:noname             NOOP    ; op,   \ 1B NOP            ------
:noname		@ABS	%TRB    ; op,   \ 1C TRB llhh       -Z----
:noname		@ABSX	%ORA    ; op,   \ 1D ORA llhh,X     NZ----
:noname		@ABSX	%ASL    ; op,   \ 1E ASL llhh,X     NZC---
0 op,                               \ 1F BBR1 zp,rr     ------
:noname		@ABS	%JSR    ; op,   \ 20 JSR x16        ------
:noname	    @ZPXI	%AND    ; op,   \ 21 AND (zp,X)     NZ----
:noname		@ZPI	%NOP    ; op,   \ 22 NOP (zp)       ------
:noname             NOOP    ; op,   \ 23 NOP            ------
:noname		@ZP	    %BIT    ; op,   \ 24 BIT zp         NZ---V
:noname		@ZP	    %AND    ; op,   \ 25 AND zp         NZ----
:noname		@ZP	    %ROL    ; op,   \ 26 ROL zp         NZC---
0 op,                               \ 27 RMB2 zp        ------
:noname             %PLP    ; op,   \ 28 PLP            NZCIDV
:noname		@IMM	%AND    ; op,   \ 29 AND #dd        NZ----
:noname             %ROLA   ; op,   \ 2A ROL A          NZC---
:noname             NOOP    ; op,   \ 2B NOP            ------
:noname		@ABS	%BIT    ; op,   \ 2C BIT llhh       NZ---V
:noname		@ABS	%AND    ; op,   \ 2D AND llhh       NZ----
:noname		@ABS	%ROL    ; op,   \ 2E ROL llhh       NZC---
0 op,                               \ 2F BBR2 zp,rr     ------
:noname		@REL	%BMI    ; op,   \ 30 BMI rr         ------
:noname		@ZPIY	%AND    ; op,   \ 31 AND (zp),Y     NZ----
:noname		@ZPI	%AND    ; op,   \ 32 AND (zp)       NZ----
:noname             NOOP    ; op,   \ 33 NOP            ------
:noname		@ZPX	%BIT    ; op,   \ 34 BIT zp,X       NZ---V
:noname		@ZPX	%AND    ; op,   \ 35 AND zp,X       NZ----
:noname		@ZPX	%ROL    ; op,   \ 36 ROL zp,X       NZC---
0 op,                               \ 37 RMB3 zp        ------
:noname             %SEC    ; op,   \ 38 SEC            --C---
:noname		@ABSY	%AND    ; op,   \ 39 AND llhh,Y     NZ----
:noname             %DEA    ; op,   \ 3A DEC A          NZ----
:noname             NOOP    ; op,   \ 3B NOP            ------
:noname		@ABSX	%BIT    ; op,   \ 3C BIT llhh,X     NZ---V
:noname		@ABSX	%AND    ; op,   \ 3D AND llhh,X     NZ----
:noname		@ABSX	%ROL    ; op,   \ 3E ROL llhh,X     NZC---
0 op,                               \ 3F BBR3 zp,rr     ------
:noname             %RTI    ; op,   \ 40 RTI            NZCIDV
:noname	    @ZPXI	%EOR    ; op,   \ 41 EOR (zp,X)     NZ----
:noname		@ZPI	%NOP    ; op,   \ 42 NOP (zp)       ------
:noname             NOOP    ; op,   \ 43 NOP            ------
:noname		@ZP	    %NOP    ; op,   \ 44 NOP zp         ------
:noname		@ZP	    %EOR    ; op,   \ 45 EOR zp         NZ----
:noname		@ZP	    %LSR    ; op,   \ 46 LSR zp         NZC---
0 op,                               \ 47 RMB4 zp        ------
:noname             %PHA    ; op,   \ 48 PHA            ------
:noname		@IMM	%EOR    ; op,   \ 49 EOR #dd        NZ----
:noname             %LSRA   ; op,   \ 4A LSR A          NZC---
:noname             NOOP    ; op,   \ 4B NOP            ------
:noname		@ABS	%JMP    ; op,   \ 4C JMP x16        ------
:noname		@ABS	%EOR    ; op,   \ 4D EOR llhh       NZ----
:noname		@ABS	%LSR    ; op,   \ 4E LSR llhh       NZC---
0 op,                               \ 4F BBR4 zp,rr     ------
:noname		@REL	%BVC    ; op,   \ 50 BVC rr         ------
:noname		@ZPIY	%EOR    ; op,   \ 51 EOR (zp),Y     NZ----
:noname		@ZPI	%EOR    ; op,   \ 52 EOR (zp)       NZ----
:noname             NOOP    ; op,   \ 53 NOP            ------
:noname		@ZPX	%NOP    ; op,   \ 54 NOP zp,X       ------
:noname		@ZPX	%EOR    ; op,   \ 55 EOR zp,X       NZ----
:noname		@ZPX	%LSR    ; op,   \ 56 LSR zp,X       NZC---
0 op,                               \ 57 RMB5 zp        ------
:noname             %CLI    ; op,   \ 58 CLI            ---I--
:noname		@ABSY	%EOR    ; op,   \ 59 EOR llhh,Y     NZ----
:noname             %PHY    ; op,   \ 5A PHY            ------
:noname             NOOP    ; op,   \ 5B NOP            ------
:noname		@ABSX	%NOP    ; op,   \ 5C NOP llhh,X     ------
:noname		@ABSX	%EOR    ; op,   \ 5D EOR llhh,X     NZ----
:noname		@ABSX	%LSR    ; op,   \ 5E LSR llhh,X     NZC---
0 op,                               \ 5F BBR5 zp,rr     ------
:noname             %RTS    ; op,   \ 60 RTS            ------
:noname	    @ZPXI	%ADC    ; op,   \ 61 ADC (zp,X)     NZC--V
:noname		@ZPI	%NOP    ; op,   \ 62 NOP (zp)       ------
:noname             NOOP    ; op,   \ 63 NOP            ------
:noname		@ZP	    %STZ    ; op,   \ 64 STZ zp         ------
:noname		@ZP	    %ADC    ; op,   \ 65 ADC zp         NZC--V
:noname		@ZP	    %ROR    ; op,   \ 66 ROR zp         NZC---
0 op,                               \ 67 RMB6 zp        ------
:noname             %PLA    ; op,   \ 68 PLA            NZ----
:noname		@IMM	%ADC    ; op,   \ 69 ADC #dd        NZC--V
:noname             %RORA   ; op,   \ 6A ROR A          NZC---
:noname             NOOP    ; op,   \ 6B NOP            ------
:noname		@ABSI	%JMP    ; op,   \ 6C JMP (llhh)     ------
:noname		@ABS	%ADC    ; op,   \ 6D ADC llhh       NZC--V
:noname		@ABS	%ROR    ; op,   \ 6E ROR llhh       NZC---
0 op,                               \ 6F BBR6 zp,rr     ------
:noname		@REL	%BVS    ; op,   \ 70 BVS rr         ------
:noname		@ZPIY	%ADC    ; op,   \ 71 ADC (zp),Y     NZC--V
:noname		@ZPI	%ADC    ; op,   \ 72 ADC (zp)       NZC--V
:noname             NOOP    ; op,   \ 73 NOP            ------
:noname		@ZPX	%STZ    ; op,   \ 74 STZ zp,X       ------
:noname		@ZPX	%ADC    ; op,   \ 75 ADC zp,X       NZC--V
:noname		@ZPX	%ROR    ; op,   \ 76 ROR zp,X       NZC---
0 op,                               \ 77 RMB7 zp        ------
:noname             %SEI    ; op,   \ 78 SEI            ---I--
:noname		@ABSY	%ADC    ; op,   \ 79 ADC llhh,Y     NZC--V
:noname             %PLY    ; op,   \ 7A PLY            NZ----
:noname             NOOP    ; op,   \ 7B NOP            ------
:noname		@ABSXI	%JMP    ; op,   \ 7C JMP (llhh,X)   ------
:noname		@ABSX	%ADC    ; op,   \ 7D ADC llhh,X     NZC--V
:noname		@ABSX	%ROR    ; op,   \ 7E ROR llhh,X     NZC---
0 op,                               \ 7F BBR7 zp,rr     ------
:noname		@REL	%JMP    ; op,   \ 80 BRA rr         ------
:noname		@ZPXI	%STA    ; op,   \ 81 STA (zp,X)     ------
:noname		@ZPI	%NOP    ; op,   \ 82 NOP (zp)       ------
:noname             NOOP    ; op,   \ 83 NOP            ------
:noname		@ZP	    %STY    ; op,   \ 84 STY zp         ------
:noname		@ZP	    %STA    ; op,   \ 85 STA zp         ------
:noname		@ZP	    %STX    ; op,   \ 86 STX zp         ------
0 op,                               \ 87 SMB0 zp        ------
:noname             %DEY    ; op,   \ 88 DEY            NZ----
:noname		@IMM	%BIT'   ; op,   \ 89 BIT #dd        -Z----  (special case)
:noname             %TXA    ; op,   \ 8A TXA            NZ----
:noname             NOOP    ; op,   \ 8B NOP            ------
:noname		@ABS	%STY    ; op,   \ 8C STY llhh       ------
:noname		@ABS	%STA    ; op,   \ 8D STA llhh       ------
:noname		@ABS	%STX    ; op,   \ 8E STX llhh       ------
0 op,                               \ 8F BBS0 zp,rr     ------
:noname		@REL	%BCC    ; op,   \ 90 BCC rr         ------
:noname		@ZPIY	%STA    ; op,   \ 91 STA (zp),Y     ------
:noname		@ZPI	%STA    ; op,   \ 92 STA (zp)       ------
:noname             NOOP    ; op,   \ 93 NOP            ------
:noname		@ZPX	%STY    ; op,   \ 94 STY zp,X       ------
:noname		@ZPX	%STA    ; op,   \ 95 STA zp,X       ------
:noname		@ZPY	%STX    ; op,   \ 96 STX zp,Y       ------
0 op,                               \ 97 SMB1 zp        ------
:noname             %TYA    ; op,   \ 98 TYA            NZ----
:noname		@ABSY	%STA    ; op,   \ 99 STA llhh,Y     ------
:noname             %TXS    ; op,   \ 9A TXS            ------
:noname             NOOP    ; op,   \ 9B NOP            ------
:noname		@ABS	%STZ    ; op,   \ 9C STZ llhh       ------
:noname		@ABSX	%STA    ; op,   \ 9D STA llhh,X     ------
:noname		@ABSX	%STZ    ; op,   \ 9E STZ llhh,X     ------
0 op,                               \ 9F BBS1 zp,rr     ------
:noname		@IMM	%LDY    ; op,   \ A0 LDY #dd        NZ----
:noname		@ZPXI	%LDA    ; op,   \ A1 LDA (zp,X)     NZ----
:noname		@IMM	%LDX    ; op,   \ A2 LDX #dd        NZ----
:noname             NOOP    ; op,   \ A3 NOP            ------
:noname		@ZP	    %LDY    ; op,   \ A4 LDY zp         NZ----
:noname		@ZP	    %LDA    ; op,   \ A5 LDA zp         NZ----
:noname		@ZP	    %LDX    ; op,   \ A6 LDX zp         NZ----
0 op,                               \ A7 SMB2 zp        ------
:noname             %TAY    ; op,   \ A8 TAY            NZ----
:noname		@IMM	%LDA    ; op,   \ A9 LDA #dd        NZ----
:noname             %TAX    ; op,   \ AA TAX            NZ----
:noname             NOOP    ; op,   \ AB NOP            ------
:noname		@ABS	%LDY    ; op,   \ AC LDY llhh       NZ----
:noname		@ABS	%LDA    ; op,   \ AD LDA llhh       NZ----
:noname		@ABS	%LDX    ; op,   \ AE LDX llhh       NZ----
0 op,                               \ AF BBS2 zp,rr     ------
:noname		@REL	%BCS    ; op,   \ B0 BCS rr         ------
:noname		@ZPIY	%LDA    ; op,   \ B1 LDA (zp),Y     NZ----
:noname		@ZPI	%LDA    ; op,   \ B2 LDA (zp)       NZ----
:noname             NOOP    ; op,   \ B3 NOP            ------
:noname		@ZPX	%LDY    ; op,   \ B4 LDY zp,X       NZ----
:noname		@ZPX	%LDA    ; op,   \ B5 LDA zp,X       NZ----
:noname		@ZPY	%LDX    ; op,   \ B6 LDX zp,Y       NZ----
0 op,                               \ B7 SMB3 zp        ------
:noname             %CLV    ; op,   \ B8 CLV            -----V
:noname		@ABSY	%LDA    ; op,   \ B9 LDA llhh,Y     NZ----
:noname             %TSX    ; op,   \ BA TSX            NZ----
:noname             NOOP    ; op,   \ BB NOP            ------
:noname		@ABSX	%LDY    ; op,   \ BC LDY llhh,X     NZ----
:noname		@ABSX	%LDA    ; op,   \ BD LDA llhh,X     NZ----
:noname		@ABSY	%LDX    ; op,   \ BE LDX llhh,Y     NZ----
0 op,                               \ BF BBS3 zp,rr     ------
:noname		@IMM	%CPY    ; op,   \ C0 CPY #dd        NZC---
:noname	    @ZPXI	%CMP    ; op,   \ C1 CMP (zp,X)     NZC---
:noname		@ZPI	%NOP    ; op,   \ C2 NOP (zp)       ------
:noname             NOOP    ; op,   \ C3 NOP            ------
:noname		@ZP	    %CPY    ; op,   \ C4 CPY zp         NZC---
:noname		@ZP	    %CMP    ; op,   \ C5 CMP zp         NZC---
:noname		@ZP	    %DEC    ; op,   \ C6 DEC zp         NZ----
0 op,                               \ C7 SMB4 zp        ------
:noname             %INY    ; op,   \ C8 INY            NZ----
:noname		@IMM	%CMP    ; op,   \ C9 CMP #dd        NZC---
:noname             %DEX    ; op,   \ CA DEX            NZ----
0 op,                               \ CB WAI            ------
:noname		@ABS	%CPY    ; op,   \ CC CPY llhh       NZC---
:noname		@ABS	%CMP    ; op,   \ CD CMP llhh       NZC---
:noname		@ABS	%DEC    ; op,   \ CE DEC llhh       NZ----
0 op,                               \ CF BBS4 zp,rr     ------
:noname		@REL	%BNE    ; op,   \ D0 BNE rr         ------
:noname		@ZPIY	%CMP    ; op,   \ D1 CMP (zp),Y     NZC---
:noname		@ZPI	%CMP    ; op,   \ D2 CMP (zp)       NZC---
:noname             NOOP    ; op,   \ D3 NOP            ------
:noname		@ZPX	%NOP    ; op,   \ D4 NOP zp,X       ------
:noname		@ZPX	%CMP    ; op,   \ D5 CMP zp,X       NZC---
:noname		@ZPX	%DEC    ; op,   \ D6 DEC zp,X       NZ----
0 op,                               \ D7 SMB5 zp        ------
:noname             %CLD    ; op,   \ D8 CLD            ----D-
:noname		@ABSY	%CMP    ; op,   \ D9 CMP llhh,Y     NZC---
:noname             %PHX    ; op,   \ DA PHX            ------
0 op,                               \ DB STP            ------
:noname		@ABS	%NOP    ; op,   \ DC NOP llhh       ------
:noname		@ABSX	%CMP    ; op,   \ DD CMP llhh,X     NZC---
:noname		@ABSX	%DEC    ; op,   \ DE DEC llhh,X     NZ----
0 op,                               \ DF BBS5 zp,rr     ------
:noname		@IMM	%CPX    ; op,   \ E0 CPX #dd        NZC---
:noname	    @ZPXI	%SBC    ; op,   \ E1 SBC (zp,X)     NZC--V
:noname		@ZPI	%NOP    ; op,   \ E2 NOP (zp)       ------
:noname             NOOP    ; op,   \ E3 NOP            ------
:noname		@ZP	    %CPX    ; op,   \ E4 CPX zp         NZC---
:noname		@ZP	    %SBC    ; op,   \ E5 SBC zp         NZC--V
:noname		@ZP	    %INC    ; op,   \ E6 INC zp         NZ----
0 op,                               \ E7 SMB6 zp        ------
:noname             %INX    ; op,   \ E8 INX            NZ----
:noname		@IMM	%SBC    ; op,   \ E9 SBC #dd        NZC--V
:noname             NOOP    ; op,   \ EA NOP            ------
:noname             NOOP    ; op,   \ EB NOP            ------
:noname		@ABS	%CPX    ; op,   \ EC CPX llhh       NZC---
:noname		@ABS	%SBC    ; op,   \ ED SBC llhh       NZC--V
:noname		@ABS	%INC    ; op,   \ EE INC llhh       NZ----
0 op,                               \ EF BBS6 zp,rr     ------
:noname		@REL	%BEQ    ; op,   \ F0 BEQ rr         ------
:noname		@ZPIY	%SBC    ; op,   \ F1 SBC (zp),Y     NZC--V
:noname		@ZPI	%SBC    ; op,   \ F2 SBC (zp)       NZC--V
:noname             NOOP    ; op,   \ F3 NOP            ------
:noname		@ZPX	%NOP    ; op,   \ F4 NOP zp,X       ------
:noname		@ZPX	%SBC    ; op,   \ F5 SBC zp,X       NZC--V
:noname		@ZPX	%INC    ; op,   \ F6 INC zp,X       NZ----
0 op,                               \ F7 SMB7 zp        ------
:noname             %SED    ; op,   \ F8 SED            ----D-
:noname		@ABSY	%SBC    ; op,   \ F9 SBC llhh,Y     NZC--V
:noname             %PLX    ; op,   \ FA PLX            NZ----
:noname             NOOP    ; op,   \ FB NOP            ------
:noname		@ABS	%NOP    ; op,   \ FC NOP llhh       ------
:noname		@ABSX	%SBC    ; op,   \ FD SBC llhh,X     NZC--V
:noname		@ABSX	%INC    ; op,   \ FE INC llhh,X     NZ----
0 op,                               \ FF BBS7 zp,rr     ------


OPTBL - .  \ should be $200
.s \ should be empty


\ test code borrows from taliforth

hex

variable actual-depth   \ stack record
create actual-results  20 cells allot


\ Empty stack: handles underflowed stack too
: empty-stack ( ... -- )
   depth ?dup if
      dup 0< if
         negate 0 do 0 loop
      else
         0 do drop loop
      then
   then ;

\ Print the previous test's actual results. Added by SamCo 2018-05
: show-results ( -- )
   cr s"  ACTUAL RESULT: { " type
   actual-depth @ 0 ?do
      actual-results
      actual-depth @ i - 1- \ Print them in reverse order to match test.
      cells + @ .
   loop
   s" }" type ;

\ Display an error message followed by the line that had the error
: error  \ ( C-ADDR U -- )
   type source type \ display line corresponding to error
   empty-stack      \ throw away every thing else
   show-results ;   \ added by SamCo to show what actually happened

\ Syntactic sugar
: T{  ( -- ) ;

\ Record depth and content of stack
: ->  ( ... -- )
   depth dup actual-depth !  \ record depth
   ?dup if                   \ if there is something on stack ...
      0 do
         actual-results i cells + !
      loop                   \ ... save it
   then ;

\ Compare stack (expected) contents with saved (actual) contents
: }T  ( ... -- )
   depth actual-depth @ = if     \ if depths match
      depth ?dup if              \ if there is something on the stack
         0 do                    \ for each stack item
            actual-results i cells + @  \ compare actual with expected
            <> if
               cr s" INCORRECT RESULT: " error leave
            then
         loop
      then
   else                          \ depth mismatch
      cr s" WRONG NUMBER OF RESULTS: " error
   then ;


\ single test case

$2113 >PC $bf >S $21 >A $0b >X $4c >Y $ea >P
$2113 $09 >M $2114 $4b >M $2115 $c2 >M
T{ op -> }T
T{ PC S A X Y P -> $2115 $bf $6b $0b $4c $68 }T
T{ $2113 M $2114 M $2115 M -> $09 $4b $c2 }T




