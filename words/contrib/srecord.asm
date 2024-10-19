.comment

Stnnhhll<dd x n-3>cc
nn: count including address (llhh), data and checksum
cc: checksum of count, address, data = (sum(vals) & $ff) ^ $ff

S0: header
S1: data (16 bit address)
S5: 'addr' is 16 bit record count (# of S1 records)
S9: termination, with entrypoint

parse-s19 S0... S1... S1... S5... S9... ( -- xt )

d65c:

S00600004844521B        ; S0: 6 bytes, addr=0, data=HDR, chk=1B
S1230200B200A274DD4F03F006E810F8291FAA8A4AAABDAC0390044A4A4A4AA0018405B883
S1230220290FF00A3A4A2605AA6979BDBC03850608A400A50120EB02A2FBA00220CF02882F
S123024010FAE8F013C60530F1B2009506E600D002E60120EF0280E4A502A20EDDCA03D087
S123026005BDD703802BCA10F3A208BD03032502DD0B03F003CAD0F3A5028607E008B00B90
S1230280E005100B46076A46076A186A4A4A2CA9440AAABD14038507BD15038508A203A9F9
S12302A00A060726082A90F920D102CAD0F120CF0228A506F016A20706069006BD9403203B
S12302C0D102E005D00320D402CA10ECA90A2CA9204C58064605A504A403B00F9850100827
S12302E06500A8A50169002810013A20EF0298484A4A4A4A20FA0268290F0930C93A30D159
S1230300690680CD07031F1FD7031F0704021A12D4030200827C880BE42B0609029D026119
S1230320601B8698C4A46812E073349D329D3261321C301CD80CD893E464E493309D3061B2
S12303404621864B864B46213282328326A6F0A430823083961420821814061BE454208387
S123036052134699129502828615121B26950283A615529982147221C61042A6326172A0D0
S1230380E6102C1B321CB24B8A13081B301CB04B62114899592C29582C242823E07352B878
S12303A0209D702102A670A0605384A4B303111130002222DF0955158000666620A0382353
S12303C0647C672040608096B6BE6C147C1C9C4C898A9EA2AADBEACACB4A4B0B4A09480BEB
S10F03E015494644474502F077A8C122A5
S12306006400A9028501200002A501C903D0F700006402A9108501A5028500920048890FA3
S1230620D00320CC0268200002E602D0E620CC020000A9608DB1026400A9072400D003206B
S11F0640CC02A500A0084A260288D0FA205802E600D0E620CC0200008D01F060D9
S5030013E9
S9030200FA


.endcomment


s19chk = tmp2
s19mode = tmp2+1                ; %wfxx_xxxx with w=1 for data-write (S1) and f=1 for last (S9)
s19len = tmp2+1
hinbl = tmpdsp

#nt_header to_srec, ">srec"
xt_to_srec:     ; ( addr u -- )
        jsr underflow_2
w_to_srec:
        jsr w_cr
        jsr w_two_dup           ; keep an original copy and one to adjust
        ; ( addr u addr u )

        stz tmp1
        stz tmp1+1

        ldy #0
        stz s19len
        jsr _s19record

        stz 4,x                 ; count number of data records
        stz 5,x
        ; ( addr n addr u )

_data:
        lda 1,x                 ; decide how many bytes to write
        bne _max
        lda 0,x
        beq _footer             ; no bytes left?
        cmp #$20                ; at most 32 bytes
        bcc +
_max:
        lda #$20
+
        ldy 2,x                 ; copy current data pointer
        sty tmp1
        ldy 3,x
        sty tmp1+1

        jsr w_zero
        sta 0,x
        ; ( addr n addr' u' k )
        sta s19len
        jsr w_slash_string      ; consume len bytes
        ; ( addr n addr'' u'' )

        ldy #1                  ; write a data record
        jsr _s19record

        inc 4,x                 ; count the records
        bne _data
        inc 5,x
        bra _data

_footer:
        jsr w_two_drop          ; drop consumed string

        lda #%1001_0101         ; S5 and then S9
        sta tmpdsp
        ; ( addr n )
_loop:
        lda tmpdsp
        beq _done

        tay
        lsr
        lsr
        lsr
        lsr
        sta tmpdsp

        ; copy TOS to the data pointer
        ; on first pass this is record count for S5,
        ; then the original address for S9
        lda 0,x
        sta tmp1
        lda 1,x
        sta tmp1+1
        jsr w_drop

        stz s19len
        jsr _s19record
        bra _loop

_done:
        rts

_s19record:
        lda #'S'
        jsr emit_a
        tya
        jsr nibble_to_ascii

        stz s19chk              ; initialize the checksum, starting from count byte

        lda s19len              ; write record length
        clc
        adc #3                  ; data length + 2 address bytes + 1 checksum byte
        jsr _s19emit

        ldy #1                  ; write two address bytes (big-endian order)
-
        lda tmp1,y
        jsr _s19emit
        dey
        bpl -

        ldy #0                  ; write any data bytes
-
        dec s19len
        bmi +
        lda (tmp1),y
        jsr _s19emit
        iny
        bra -
+
        eor #$ff                ; compute and write the checksum
        jsr byte_to_ascii
        jsr w_cr
        rts

_s19emit:
        pha
        jsr byte_to_ascii
        pla
        clc
        adc s19chk
        sta s19chk
z_to_srec:
        rts


#nt_header srec_from, "srec>"
xt_srec_from:   ; ( -- xt f )
w_srec_from:
        jsr w_parse_name        ; ( addr n )
        lda 0,x
        ora 1,x
        bne +                   ; non-zero length?

        inx                     ; drop string
        inx
        inx
        inx

        jsr w_refill            ; fetch another line
        inx                     ; drop flag either way
        inx
        lda $fe,x               ; bail out if refill failed
        beq _fail
        bra w_srec_from         ; otherwise get next word for SREC string
+
        lda 2,x                 ; stash the string pointer for easy access
        sta tmp1                ; and so we can write SREC addresses here
        lda 3,x
        sta tmp1+1

        lda 1,x
        bne _fail               ; max 256 char input
        lsr 0,x                 ; we'll count pairs of characters
        bcs _fail               ; but must start with an even number of them

        lda (tmp1)
        cmp #'S'
        bne _fail

        stz s19mode
        stz s19chk

        ldy #1
        jsr chr2nbl             ; we'll handle S0,S1,S5 and S9: 0, %1, %101, %1001
        bcs _fail               ; invalid nibble
        beq _s0                 ; otherwise check for S0 (with C=0 from above)
        lsr                     ; others are odd, so check lsb leaving %0, %10, or %100
        bcc _fail
        beq _s1                 ; S1 (with C=1 from above)
        lsr                     ; otherwise check zero lsb again leaving %1 or %10
        bcs _fail
        dea
        beq _s5                 ; S5 (with C=0 from above)
        dea
        bne _fail
        dec s19mode             ; $ff

        ; otherwise fall through for S9 (with C=0 from above)
        ; we'll treat all modes the same, except that S1 will write
        ; data through to the address, others will just checksum it
_s0:
_s5:
_s9:
_s1:
        ror s19mode             ; S1 sets N=1 (via C=1), S9 has V=1
        lsr s19mode             ; temporarily disable data write (shift to %0wfx_xxxx)

        dec 0,x                 ; we consumed the 'S#' pair

        jsr _s19byte            ; read number of encoded bytes, n
        cmp 0,x                 ; should match remaining pairs characters
        bne _fail

        jsr _s19byte            ; read and store S19 address at NOS, ,
        sta 3,x                 ; note: big endian order
        jsr _s19byte
        sta 2,x
        asl s19mode             ; restore write mode

-
        lda 0,x                 ; stop when we get to checksum byte
        dea
        beq +
        jsr _s19byte            ; read, checksum and maybe write reamining data bytes
        bra -
+
        jsr str2byte            ; read the checksum byte
        bcs _fail
        eor #$ff
        cmp s19chk
        bne _fail

        bit s19mode
        lda #$ff                ; in case we're done
        bvs +                   ; S9 so we're done
        inx
        inx
        inx
        inx
        jmp w_srec_from

_fail:
        lda #0
+
        sta 0,x
        sta 1,x
        rts


_s19byte:
    ; parse two characters as a byte, dec count of pairs,
    ; and write value to addr++ if mode bit set.
    ; Accumulate result in running checksum.  Returns A=value
    ; Failure breaks out to top-level failure.
        jsr str2byte
        bcc _ok
        pla                     ; pop this return and fail at top level
        pla
        bra _fail
_ok:
        bit s19mode             ; write data if bit 7 set
        bpl +
        sta (2,x)
        inc 2,x
        bne +
        inc 3,x
+
        dec 0,x                 ; consumed a pair
        pha
        adc s19chk
        sta s19chk              ; update and return checksum
        pla
z_srec_from:
        rts

str2byte:
        ; convert two ascii chars from (tmp1),y to a hex byte
        ; incrementing y by two.
        ; return C=0 on success with A=value, else C=1
        jsr chr2nbl
        bcs chr2nbl_done

        asl
        asl
        asl
        asl
        sta hinbl               ; save high nibble
        ; fall thru, skipping stz zp
        .byte $2c

chr2nbl:
        ; convert A from ascii 0-9A-Fa-f to hex $0-f
        ; with C=0 on success, C=1 on invalid char
        stz hinbl
        lda (tmp1),y

        cmp #$40
        bcs _hi

        sbc #$2F                ; < '0' => large
        cmp #10                 ; C=1 if >= 10 (error)
        bra _done

_hi:
        and #$1f                ; mask to accept uc & lc
        beq _done               ; @ or ` is error (C=1 from above)
        adc #8                  ; A=1 => 1+8+1=10
        cmp #$10                ; set C=1 if not $a-$f
_done:
chr2nbl_done:
        iny
        ora hinbl               ; combine high nibble
        rts

