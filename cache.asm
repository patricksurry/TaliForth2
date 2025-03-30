bloom_bits = address($7b00)              ; TODO

bloom_init:
        ldy #0
        tya
-
        sta bloom_bits,y
        iny
        bne -
        rts


bloom_add_wordlist:
    ; given first NT in tmp1, add all the words in that list

_loop:
        lda tmp1+1
        beq _done

        ldy #1
        lda (tmp1),y            ; name length
        sta djb_len

        lda (tmp1)              ; grab status flags for header length
        and #DC+LC+FP           ; mask length bits
        lsr
        adc #4                  ; calculate header length, leaving C=0

        adc tmp1                ; name starts at tmp1 + header length
        sta djb_sptr
        lda #0
        adc tmp1+1
        sta djb_sptr+1

        jsr bloom_set
        jsr nt_to_nt            ; update tmp1 to prev NT
        bra _loop
_done:

;TODO temporarily count and show number of bits set
        jsr w_zero
        ldy #0
_count:
        lda bloom_bits,y
-
        lsr
        bcc _zero
        jsr w_one_plus
+
_zero:  cmp #0
        bne -
        iny
        bne _count

        jsr w_dot
        jsr w_cr
;TODO end

        rts


demux3to8:
    .byte %0000_0001
    .byte %0000_0010
    .byte %0000_0100
    .byte %0000_1000
    .byte %0001_0000
    .byte %0010_0000
    .byte %0100_0000
    .byte %1000_0000


bloom_set:      ; djb_sptr, djb_len -> djb_hash
        jsr djb2d               ; calculate 32 bit hash in djb_hash
bloom_set2:     ; alternate entrypoint if hash is already calculated
        bit demux3to8 + 6       ; set overflow flag
        bra bloom_common

bloom_test:     ; djb_sptr, djb_len -> djb_hash, A =?= 0
    ; returns A=0 if might be present, A>0 if definitely not present (with failing hash index)
        jsr djb2d               ; calculate 32 bit hash in djb_hash
        clv

    ; We'll use the 32 bit hash plus the parity bit of the first character
    ; to get a 33 bit hash which we split into three indepednent 11 bit hashes.
    ; Each 11 bit hash gives us an 8 bit index to the bloom_bits table
    ; plus a 3 bit index within the corresponding byte.
bloom_common:
        phx                     ; save Forth stack pointer
        ldx #3                  ; test three 11 bit hashes

        lda (djb_sptr)
        ror                     ; C = parity of first character

        lda djb_hash            ; copy first hash byte
        sta tmpdsp
_loop:
        and #7
        tay                     ; next 3 LSB bits of hash byte give bit index 0-7

        lda demux3to8,y         ; convert index bit mask
        ldy djb_hash,x          ; use the k-th byte of hash as index to bloom filter

        bvc _test               ; set or test?

        ora bloom_bits,y        ; set the target bit
        sta bloom_bits,y
        bra _cont
_test:
        and bloom_bits,y        ; test whether target bit is set
        beq _absent             ; this string can't be in the bloom filter
_cont:
        dex
        beq _done

        lda tmpdsp
        ror a                   ; take next 3 bits from first hash byte, rolling in parity bit on first pass
        ror a
        ror a
        sta tmpdsp

        bra _loop
_done:
_absent:
        txa                     ; returns A<>0 for failed test
        plx                     ; restore Forth stack pointer

        rts



; with fnv1a + bloom
; python3 talitest_c65.py -t core_a core_b core_c
; bye c65: PC=f016 A=98 X=78 Y=98 S=f6 FLAGS=<N0 V0 B1 D0 I1 Z0 C1> ticks=72142959

; with djb32d + bloom
; bye c65: PC=f016 A=98 X=78 Y=98 S=f6 FLAGS=<N0 V0 B1 D0 I1 Z0 C1> ticks=66468642

; master
; bye c65: PC=f016 A=98 X=78 Y=98 S=f6 FLAGS=<N0 V0 B0 D0 I1 Z0 C1> ticks=76380293