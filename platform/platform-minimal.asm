        ; 65C02 processor (Tali will not compile on older 6502)
        .cpu "65c02"
        ; No special text encoding (eg. ASCII)
        .enc "none"

; This illustrates a minimal configuration of TaliForth requiring a little under 12K of ROM.
; The 16K image leaves $f000-$ffff empty other than py65mon I/O and the interrupt vectors.
; Build and run in the py65mon simulator like:

;       make taliforth-minimal.bin
;       py65mon -m 65c02 -r taliforth-py65mon.bin

; If you want to customize this to run on your own hardware or simulator,
; simply rewrite the kernel_getc and kernel_putc routines below so that they
; respectively fetch a key and display a character within your environment.

; Where to start Tali Forth 2 in ROM (or RAM if loading it)
        * = $c000

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
;    $bc00  +-------------------+  hist_buff, cp_end
;           |   Input History   |
;           |    for ACCEPT     |
;           |  8x128B buffers   |
;    $bfff  +-------------------+  ram_end


; HARD PHYSICAL ADDRESSES

; Some of these are somewhat silly for the 65c02, where for example
; the location of the Zero Page is fixed by hardware. However, we keep
; these for easier comparisons with Liara Forth's structure and to
; help people new to these things.

ram_start = $0000          ; start of installed 32 KiB of RAM
ram_end   = $c000-1        ; end of installed RAM
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
buffer0   = stack0+$100      ; input buffer ($0200-$02ff)
cp0       = buffer0+bsize+1  ; Dictionary starts after last buffer
                             ; The RAM System Variables and BLOCK buffer are
                             ; placed right at the beginning of the dictionary.
cp_end    = hist_buff        ; Last RAM byte available for code
padoffset = $ff              ; offset from CP to PAD (holds number strings)


; OPTIONAL WORDSETS

; For our minimal build, we'll drop all the optional words

; TALI_OPTIONAL_WORDS := [ "ed", "editor", "ramdrive", "block", "environment?", "assembler", "disassembler", "wordlist" ]
TALI_OPTIONAL_WORDS := [ ]

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
TALI_OPTION_HISTORY := 0
;TALI_OPTION_HISTORY := 1

; The terse option strips or shortens various strings to reduce the memory
; footprint when set to 1 (~0.5K)
;TALI_OPTION_TERSE := 0
TALI_OPTION_TERSE := 1

; =====================================================================
; FINALLY

; Make sure the above options are set BEFORE this include.

.include "../taliforth.asm" ; zero page variables, definitions

; The minimal config just sneaks under $f000 where the default py65mon IO lives
; leaving almost 4K empty (from $f016 thru $fffa) for whatever you need

; By default Tali is set up for I/O with the py65mon or c65 simulator
; (see docs/MANUAL.md for details).  You can configure Tali for your
; own hardware setup by defining your own kernel routines as follows:
;
;       kernel_init - Initialize the low-level hardware
;       kernel_getc - Get single character in A from the keyboard (blocks)
;       kernel_putc - Prints the character in A to the screen
;       kernel_bye  - Exit forth, e.g. to a monitor program or just `brk`
;       s_kernel_id - The zero-terminated string printed at boot;

.include "simulator.asm"

; Leave the following string as the last entry in the kernel routine so it
; is easier to see where the kernel ends in hex dumps. This string is
; displayed after a successful boot

s_kernel_id:
        .text "TF2", AscLF, 0

; Define the interrupt vectors.  For the simulator we redirect them all
; to the kernel_init routine and restart the system hard.  If you want to
; use them on actual hardware, you'll likely have to redefine them.

* = $fffa

v_nmi   .word kernel_init
v_reset .word kernel_init
v_irq   .word kernel_init

; END
