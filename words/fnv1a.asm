fnv_hash = scratch
fnv_tmp = scratch+4

; ## FNV1A ( addr n -- dhash ) "Calculate the 32-bit FNV1A hash of a byte string"
; ## "fnv1a" tested Tali Forth
xt_fnv1a:
        jsr underflow_2
w_fnv1a:
        jsr fnv1a               ; calculate hash in fnv_hash (see cache.asm)

        lda fnv_hash            ; finally swap XINU to NUXI order
        sta 2,x
        lda fnv_hash+1
        sta 3,x
        lda fnv_hash+2
        sta 0,x
        lda fnv_hash+3
        sta 1,x

z_fnv1a:
        rts


fnv1a:  ; ( addr n -- addr+n 0 )
        ; see https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function"
        ;
        ; This is a 32-bit hash with good avalanche properties which is easy to calculate.
        ; The algorithm looks like this:
        ;
        ; hash_0 = $811c9dc5
        ; hash_i+1 = (hash_i ^ s_i ) * $01000193
        ;
        ; The multipier $01000193 can be written as 2^24 + 2^8 + $93 which
        ; simplifies the calculation.
        ;
        ; Tests:
        ;
        ; Two four byte "strings" that hash to 0:
        ;
        ;       here $cc c, $24 c, $31 c, $c4 c, 4 fnv1a ud. -> $00000000
        ;       here $e0 c, $4d c, $9f c, $cb c, 4 fnv1a ud. -> $00000000
        ;
        ; An empty string hashes to the basis value:
        ;
        ;       0 0 fnv1a ud. -> $811C9DC5

        ; Set the initial value to $811c9dc5.  We could copy from a static .dword
        ; but unrolling is faster and only a byte longer.
        lda #$c5
        sta fnv_hash
        lda #$9d
        sta fnv_hash+1
        lda #$1c
        sta fnv_hash+2
        lda #$81
        sta fnv_hash+3

        lda 0,x                 ; handle empty string
        ora 1,x
        beq _done

_loop:
        lda (2,x)               ; hash the current character into LSB
;    jsr emit_a  ;TODO

        eor fnv_hash
        sta fnv_hash

        ; now we need to calculate h << 24 + h << 8 + h * $93
        lda fnv_hash+3          ; copy h to fnv_tmp temp...
        sta fnv_tmp+3

        lda fnv_hash+2
        sta fnv_tmp+2           ; while shifting left to get h << 8
        sta fnv_hash+3

        lda fnv_hash+1
        sta fnv_tmp+1
        sta fnv_hash+2

        lda fnv_hash
        sta fnv_tmp
        sta fnv_hash+1

        clc
        adc fnv_hash+3          ; add LSB to MSB giving h << 24 + h << 8
        sta fnv_hash+3

        stz fnv_hash

        ; now we just need to add h*$93, with h in fnv_tmp
        ; set up an 8bit x 32bit => 32bit multiply loop
        lda #%1001_0011         ; $93
        sta tmpdsp
_mul93:
        lsr tmpdsp
        bcc _x2                 ; add this multiple?

        php                     ; we'll check Z later to see if we're done
        clc
        ; unrolling takes a few more bytes, but saves time and avoids fancy
        ; indexing which we'd need to avoid compare wrecking carry between iterations
        lda fnv_hash
        adc fnv_tmp
        sta fnv_hash
        lda fnv_hash+1
        adc fnv_tmp+1
        sta fnv_hash+1
        lda fnv_hash+2
        adc fnv_tmp+2
        sta fnv_hash+2
        lda fnv_hash+3
        adc fnv_tmp+3
        sta fnv_hash+3
        plp                     ; recover tmpdsp = 0 result
_x2:
        beq _nxtchr             ; if tmpdsp is zero we're done

        asl fnv_tmp             ; multiplicand *= 2
        rol fnv_tmp+1
        rol fnv_tmp+2
        rol fnv_tmp+3
        bra _mul93              ; another bit of multiplier

_nxtchr:
        lda 0,x                 ; n--
        bne +
        dec 1,x
+
        dea
        sta 0,x
        ora 1,x
        beq _done

        inc 2,x                 ; addr++
        bne +
        inc 3,x
+
        bra _loop

_done:
;    lda #AscSP  ;TODO
;    jsr emit_a
        rts
