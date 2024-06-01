; Default kernel file for Tali Forth 2 shared by py65mon and c65 platforms
; Scot W. Stevenson <scot.stevenson@gmail.com>
; Sam Colwell
; First version: 19. Jan 2014
; This version: 04. Dec 2022

        ; 65C02 processor (Tali will not compile on older 6502)
        .cpu "65c02"
        ; No special text encoding (eg. ASCII)
        .enc "none"

        ; Where to start Tali Forth 2 in ROM (or RAM if loading it)
        * = $8000

; I/O facilities and memory layout are handled in these separate platform files
; to isolate hardware dependencies. See docs/memorymap.txt for a discussion of Tali's
; memory layout.

; MEMORY MAP OF RAM

; Drawing is not only very ugly, but also not to scale. See the manual for
; details on the memory map. Note that some of the values are hard-coded in
; the testing routines, especially the size of the input history buffer, the
; offset for PAD, and the total RAM size. If these are changed, the tests will
; have to be changed as well


;    $0000  +-------------------+  ram_start, zpage, user0
;           |   Tali zp vars    |
;           +-------------------+
;           |                   |
;           |                   |
;           +~~~~~~~~~~~~~~~~~~~+  <-- dsp
;           |                   |
;           |  ^  Data Stack    |
;           |  |                |
;    $0078  +-------------------+  dsp0, stack
;           |    flood plain    |
;    $007F  +-------------------+
;           |                   |
;           |   (free space)    |
;           |                   |
;    $0100  +-------------------+
;           |                   |
;           |  ^  Return Stack  |  <-- rsp
;           |  |                |
;    $0200  +-------------------+  rsp0, buffer, buffer0
;           |    Input Buffer   |
;    $0300  +-------------------+
;           | Native forth vars |
;    $0400  +-------------------+
;           |  1K block buffer  |
;    $0800  +-------------------+  cp0
;           |  |                |
;           |  v  Dictionary    |
;           |       (RAM)       |
;           |                   |
;   (...)   ~~~~~~~~~~~~~~~~~~~~~  <-- cp aka HERE
;           |                   |
;           |                   |
;           |                   |
;           |                   |
;           |                   |
;           |                   |
;    $7C00  +-------------------+  hist_buff, cp_end
;           |   Input History   |
;           |    for ACCEPT     |
;           |  8x128B buffers   |
;    $7fff  +-------------------+  ram_end


; HARD PHYSICAL ADDRESSES

; Some of these are somewhat silly for the 65c02, where for example
; the location of the Zero Page is fixed by hardware. However, we keep
; these for easier comparisons with Liara Forth's structure and to
; help people new to these things.

ram_start = $0000          ; start of installed 32 KiB of RAM
ram_end   = $8000-1        ; end of installed RAM
zpage     = ram_start      ; begin of Zero Page ($0000-$00ff)
zpage_end = $7F            ; end of Zero Page used ($0000-$007f)
stack0    = $0100          ; begin of Return Stack ($0100-$01ff)
hist_buff = ram_end-$03ff  ; begin of history buffers


; SOFT PHYSICAL ADDRESSES

; Tali currently doesn't have separate user variables for multitasking. To
; prepare for this, though, we've already named the location of the user's
; Zero-Page System Variables user0. Note cp0 starts one byte further down so
; that it currently has the address $300 and not $2FF. This avoids crossing
; the page boundry when accessing the RAM System Variables table, which would
; cost an extra cycle.

user0     = zpage            ; TaliForth2 system variables
rsp0      = $ff              ; initial Return Stack Pointer (65c02 stack)
bsize     = $ff              ; size of input/output buffers
buffer0   = stack0+$100      ; input buffer ($0200-$027f)
cp0       = buffer0+bsize+1  ; Dictionary starts after last buffer
                             ; The RAM System Variables and BLOCK buffer are
                             ; placed right at the beginning of the dictionary.
cp_end    = hist_buff        ; Last RAM byte available for code
padoffset = $ff              ; offset from CP to PAD (holds number strings)


; OPTIONAL WORDSETS

; Tali Forth 2 is a bit of a beast, expecting about 24K of ROM space.
; For some applications, the user might not need certain words and would
; prefer to have the memory back instead.  Remove any of the items in
; TALI_OPTIONAL_WORDS to remove the associated words when Tali is
; assembled.  If TALI_OPTIONAL_WORDS is not defined in your platform file,
; you will get all of the words.

TALI_OPTIONAL_WORDS := [ "ed", "editor", "ramdrive", "block", "environment?", "assembler", "disassembler", "wordlist" ]

; "ed" is a string editor. (~1.5K)
; "editor" is a block editor. (~0.25K)
;     The EDITOR-WORDLIST will also be removed.
; "ramdrive" is for testing block words without a block device. (~0.3K)
; "block" is the optional BLOCK words. (~1.4K)
; "environment?" is the ENVIRONMENT? word.  While this is a core word
;     for ANS-2012, it uses a lot of strings and therefore takes up a lot
;     of memory. (~0.2K)
; "assembler" is an assembler. (~3.2K)
;     The ASSEMBLER-WORDLIST will also be removed if the assembler is removed.
; "disassembler" is the disassembler word DISASM. (~0.6K)
;     If both the assembler and dissasembler are removed, the tables
;     (used for both assembling and disassembling) will be removed
;     for additional memory savings. (extra ~1.6K)
; "wordlist" is for the optional SEARCH-ORDER words (eg. wordlists)
;     Note: Without "wordlist", you will not be able to use any words from
;     the EDITOR or ASSEMBLER wordlists (they should probably be disabled
;     by also removing "editor" and "assembler"), and all new words will
;     be compiled into the FORTH wordlist. (~0.9K)


; TALI_OPTION_CR_EOL sets the character(s) that are printed by the word
; CR in order to move the cursor to the next line.  The default is "lf"
; for a line feed character (#10).  "cr" will use a carriage return (#13).
; Having both will use a carriage return followed by a line feed.  This
; only affects output.  Either CR or LF can be used to terminate lines
; on the input.

TALI_OPTION_CR_EOL := [ "lf" ]
;TALI_OPTION_CR_EOL := [ "cr" ]
;TALI_OPTION_CR_EOL := [ "cr", "lf" ]

; The history option enables editable input history buffers via ctrl-n/ctrl-p
; These buffers are disabled when set to 0 (~0.2K Tali, 1K RAM)
;TALI_OPTION_HISTORY := 0
TALI_OPTION_HISTORY := 1

; The terse option strips or shortens various strings to reduce the memory
; footprint when set to 1 (~0.5K)
TALI_OPTION_TERSE := 0
;TALI_OPTION_TERSE := 1

; =====================================================================
; FINALLY

; Make sure the above options are set BEFORE this include.

.include "../taliforth.asm" ; zero page variables, definitions

; Of the 32 KiB we use, 24 KiB are reserved for Tali (from $8000 to $DFFF)
; and the last eight (from $E000 to $FFFF) are left for whatever the user
; wants to use them for.

; By default Tali is set up for I/O with the py65mon or c65 simulator
; (see docs/MANUAL.md for details).  You can configure Tali for your
; own hardware setup by defining your own kernel routines as follows:
;
;       kernel_init - Initialize the low-level hardware
;       kernel_getc - Get single character in A from the keyboard (blocks)
;       kernel_putc - Prints the character in A to the screen
;       kernel_kbhit - Return non-zero if a key is ready for kernel_getc
;       kernel_bye  - Exit forth, e.g. to a monitor program or just `brk`
;       s_kernel_id - The zero-terminated string printed at boot;

; The main file of Tali got us to about $e000. However, py65mon by default puts
; the basic I/O routines at the beginning of $f000. We don't want to change
; that because it would make using it out of the box harder, so we just
; advance past the virtual hardware addresses.

io_start = $f000

* = io_start

; Define the c65 / py65mon magic IO addresses relative to $f000
                .byte ?
io_putc:        .byte ?         ; $f001     write byte to stdout
                .byte ?
io_kbhit:       .byte ?         ; $f003     read non-zero on key ready (c65 only)
io_getc:        .byte ?         ; $f004     non-blocking read input character (0 if no key)
io_clk_start:   .byte ?         ; $f006     *read* to start cycle counter
io_clk_stop:    .byte ?         ; $f007     *read* to stop the cycle counter
io_clk_cycles:  .word ?,?       ; $f008-b   32-bit cycle count in NUXI order
                .word ?,?

; these magic block IO addresses are only implemented by c65 (not py65mon)
; see c65/README.md for more detail

io_blk_action:  .byte ?     ; $f010     Write to act (status=0 read=1 write=2)
io_blk_status:  .byte ?     ; $f011     Read action result (OK=0)
io_blk_number:  .word ?     ; $f012     Little endian block number 0-ffff
io_blk_buffer:  .word ?     ; $f014     Little endian memory address

io_end:


kernel_init:
        ; """Initialize the hardware. This is called with a JMP and not
        ; a JSR because we don't have anything set up for that yet. With
        ; py65mon, of course, this is really easy. -- At the end, we JMP
        ; back to the label forth to start the Forth system.
        ; """
                ; Since the default case for Tali is the py65mon emulator, we
                ; have no use for interrupts. If you are going to include
                ; them in your system in any way, you're going to have to
                ; do it from scratch. Sorry.
                sei             ; Disable interrupts

                ; We've successfully set everything up, so print the kernel
                ; string
                ldx #0
-               lda s_kernel_id,x
                beq _done
                jsr kernel_putc
                inx
                bra -
_done:
                jmp forth

kernel_bye:
        ; """Forth shutdown called from BYE"""
                brk

kernel_putc:
        ; """Print a single character to the console.
        ;
        ; Note this routine must preserve X, Y but that's easy here.
        ; If your code is more complex, wrap it with PHX, PHY ... PLY, PHX
        ; """
                sta io_putc
                rts

; c65 and py65mon have different implementations of kernel_getc and kernel_kbhit

; kernel_getc should wait until a key is available from the keyboard and then
; return it in the accumulator.  It should preserve the X and Y registers.

; kernel_kbhit should return non-zero if a key is available to read,
; but doesn't actually return the character.  It should preserve the X and Y registers.
; It's only required if you use the KEY? word.
; If your hardware requires you to read the character while checking if one is
; ready, you should buffer it and modify kernel_getc appropriately.
; See the platform-py65mon.asm as an example.
; If your hardware doesn't support kbhit at all, you can use this dummy
; implementation which makes KEY? behave like the blocking KEY:
;
; kernel_kbhit:
;               lda #$ff
;               rts

