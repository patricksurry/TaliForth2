; Default kernel file for Tali Forth 2
; Scot W. Stevenson <scot.stevenson@gmail.com>
; Sam Colwell
; First version: 19. Jan 2014
; This version: 04. Dec 2022

; The main file of Tali got us to about $e000. However, py65mon by default puts
; the basic I/O routines at the beginning of $f000. We don't want to change
; that because it would make using it out of the box harder, so we just
; advance past the virtual hardware addresses.

io_start = $f000

* = io_start

; Define the c65 / py65mon magic IO addresses relative to $f000
                .byte ?
io_putc:        .byte ?     ; $f001     write byte to stdout
                .word ?
io_getc:        .byte ?     ; $f004     read byte from stdin
io_peekc:       .byte ?     ; $f005     non-blocking input
                            ;           bit7=0 if no input
                            ;           bit7=1 with 7bit chr if input
io_clk_start:   .byte ?     ; $f006     *read* to start cycle counter
io_clk_stop:    .byte ?     ; $f007     *read* to stop the cycle counter
io_clk_cycles:  .word ?,?   ; $f008-b   32-bit cycle count in NUXI order
                .word ?,?

; these magic block IO addresses are only implemented by c65 (not py65mon)
; see c65/README.md for more detail

io_blk_action:  .byte ?     ; $f010     Write to act (status:0 read:1 write:2 quit:ff)
io_blk_status:  .byte ?     ; $f011     Read action result (0 = OK)
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

kernel_getc:
        ; """Get a single character from the keyboard. By default, py65mon
        ; is set to $f004, which we just keep. Note that py65mon's getc routine
        ; is non-blocking, so it will return '00' even if no key has been
        ; pressed. We turn this into a blocking version by waiting for a
        ; non-zero character.
        ; """
_loop:
                lda io_getc
                beq _loop
                rts


kernel_putc:
        ; """Print a single character to the console. By default, py65mon
        ; is set to $f001, which we just keep.
        ; """
                sta io_putc
                rts

kernel_bye:
platform_bye:
        ; """Forth shutdown called from BYE"""
                brk
