.include "simulator.asm"

; py65mon doesn't have kbhit so we'll roll our own using io_kbhit as a one character buffer
io_bufc = io_kbhit

kernel_getc:
        ; """Get a single character from the keyboard.
        ; py65mon's io_getc is non-blocking, returning 0 when no key is pressed.
        ; We'll convert to blocking by waiting for a non-zero result.
        ; We also check the single character io_bufc buffer used by kbhit
        ;
        ; Note this routine must preserve X and Y but that's easy here.
        ; If your code is more complex, wrap it with PHX, PHY ... PLY, PHX
        ; """
                lda io_bufc             ; first check the buffer
                stz io_bufc
                bne _done
_loop:                                  ; otherwise wait for a character
                lda io_getc
                beq _loop
_done:
                rts

kernel_kbhit:
        ; """Check if a character is available.  py65mon doesn't have a native kbhit
        ; so we buffer the result of the non-blocking io_getc instead
        ; This routine is only required if you use the KEY? word.
        ; """
                lda io_bufc             ; do we already have a character?
                bne _done

                lda io_getc             ; otherwise check and buffer the result
                sta io_bufc
_done:
                rts


; Leave the following string as the last entry in the kernel routine so it
; is easier to see where the kernel ends in hex dumps. This string is
; displayed after a successful boot

s_kernel_id:
        .text "Tali Forth 2 default kernel for py65mon (04. Dec 2022)", AscLF, 0

; Define the interrupt vectors.  For the simulator we redirect them all
; to the kernel_init routine and restart the system hard.  If you want to
; use them on actual hardware, you'll likely have to redefine them.

* = $fffa

v_nmi   .word kernel_init
v_reset .word kernel_init
v_irq   .word kernel_init

; END
