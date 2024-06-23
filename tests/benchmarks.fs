
(
  These are some simple benchmarks adapted from https://theultimatebenchmark.org/#sec-13.
  They aren't part of the standard test suite since they take several seconds to complete.
  To benchmark a build, run commands like this:

  export TALIBIN=taliforth-c65.bin
  for limit in 0 20; for uf in 0 1; do
    export TALIVARS="$limit nc-limit \! $uf strip-underflow \! \n"
    echo "\n$TALIVARS"
    echo $TALIVARS | cat - tests/benchmarks.fs | c65/c65 -r $TALIBIN | grep 'cycles\. ok'
  done

This log records some historical results.  Each build date corresponds to commits linked below,
with each row giving the 65c02 cycle count for each test (and the cumulative total)
for `nc-limit` (NC) set to 0 or 20 bytes and `strip-underflow` (SUF) enabled or disabled.
Setting `0 nc-limit` generates smaller but slower code, and `0 strip-underflow` trades some
speed for run-time stack underflow checks.

Build     Test       0 NC 0 SUF   0 NC 1 SUF  20 NC 0 SUF  20 NC 1 SUF

20231101  ddbench      4751473      4751473      3965029      2916469
20240623  ddbench      5081308      4032748      2197724      1149164

20231101  intcalcs    28762819     28762819     25882671     22898462
20240623  intcalcs    27442769     25010657     24290570     21666430

20231101  fib2        33746816     33746816     28490708     21501780
20240623  fib2        35220166     28224838     20633701     13638373

20231101  nesting     25165800     25165800     25165800     25165800
20240623  nesting     25165800     25165800     25165800     25165800

20231101  sieve       13602239     13602239     11737511      9491287
20240623  sieve       13271375     11025135      9518125      7271885

20231101  gcd1        40939545     40939545     37438209     32771073
20240623  gcd1        37793577     30474953     25722994     16415754

20231101  pal         56252577     56252577     55211451     54105673
20240623  pal         29691810     28236978     27773260     26220729

20231101  coll        37286938     37286938     35333950     33462094
20240623  coll        34477726     32390862     29940867     27841333

20231101  Total      240508207    240508207    223225329    203504735
20240623  Total      208144531    184561971    165243041    139369468

Tested taliforth-c65.bin from these builds (20231101 used taliforth-py65mon.bin):

20231101 https://github.com/SamCoVT/TaliForth2/commit/e9fff574d0a2390e9a5cfb0a09538292bc13db7e
20240623 https://github.com/SamCoVT/TaliForth2/commit/032e914755f2ffb7f6a10fb4005d2ca9a1770edb

)

$f006 constant t_start    \ Read to start the cycle counter
$f007 constant t_stop     \ Read to stop the cycle counter
$f008 constant t_cycles   \ Double-word (32 bit) cycle count in NUXI order

2variable t_overhead
2variable total-cycles

: cycles ( xt -- ud )
    t_start @ drop execute t_stop @ drop
    t_cycles 2@ t_overhead 2@ d-
;

: show-cycles ( xt -- )
    cycles
    2dup total-cycles 2@ d+ total-cycles 2!
    ud. ." cycles."
;

0. total-cycles 2!
0. t_overhead 2! ' align cycles t_overhead 2!

: ddbench 1 32767 0 do dup drop loop drop ;

32000 constant bigint
variable result
: intcalcs
  1 dup result dup >r !
  begin
    dup bigint <
  while
    dup negate r@ +! 1+
    dup r@ +! 1+
    r@ @ over * r@ ! 1+
    r@ @ over / r@ ! 1+
  repeat
  r> drop drop
;

: fib2 ( n1 -- n2 )
   0 1 rot 0 do
      over + swap loop
   drop ;

: fib2-bench
    400 0 do i fib2 drop loop
 ;

: bottom ;              : 1st bottom bottom ;   : 2nd 1st 1st ;
: 3rd 2nd 2nd ;         : 4th 3rd 3rd ;         : 5th 4th 4th ;
: 6th 5th 5th ;         : 7th 6th 6th ;         : 8th 7th 7th ;
: 9th 8th 8th ;         : 10th 9th 9th ;        : 11th 10th 10th ;
: 12th 11th 11th ;      : 13th 12th 12th ;      : 14th 13th 13th ;
: 15th 14th 14th ;      : 16th 15th 15th ;      : 17th 16th 16th ;
: 18th 17th 17th ;      : 19th 18th 18th ;      : 20th 19th 19th ;

 8192 constant sieve-size
 variable sieve-flags
 0 sieve-flags !
 sieve-size allot

 : sieve-bench
   sieve-flags sieve-size 1 fill  ( set array )
   0 ( 0 count ) sieve-size 0
   do sieve-flags i + c@
     if i dup + 3 + dup i +
        begin dup sieve-size <
        while 0   over sieve-flags +  c!  over +  repeat
        drop drop 1+
     then
 loop
 drop
;


: gcd ( a b -- gcd )
   OVER IF
     BEGIN
       DUP WHILE
          2DUP U> IF SWAP THEN OVER -
     REPEAT DROP ELSE
     DUP IF NIP ELSE 2DROP 1 THEN
   THEN ;

: gcd1-bench 80 0 DO
      80 0 DO j i gcd drop loop
      loop ;

: num>str ( n -- addr bytecount ) 0 <# #S #> ;

: lasteqfirst? ( addr offsetlast -- flag )
  OVER + C@ SWAP C@ = ;

: ispalindrome? ( addr offsetlast -- flag )
  DUP 1 <              IF 2DROP 1 EXIT THEN
  2DUP lasteqfirst? 0= IF 2DROP 0 EXIT THEN
  2 - SWAP 1+ SWAP RECURSE ;

: pal-bench ( -- ) 10 BEGIN
    DUP   num>str  \ ( n addr len )
    2DUP 1-        \ ( n addr len addr len-1 )
    ispalindrome?  \ ( n addr len flag )
    drop 2drop     \ for output: if type space else 2drop then
    1+ DUP 4096 =
  UNTIL DROP ;


( Benchmark mit der Collatz-Funktion  V1.1 RG 2017-10-05   )
: cn+1 		( cn -- cm )
  2 /mod swap
  if dup 10922 < 	( kein ueberlauf ? )
    if 3 * 2 +
    else drop 0 then
  then
;
: coll. 	( cn -- )
  begin dup 1 > while
    cn+1 dup .
  repeat
  drop		( always 1 )
;
: ccnt 		( cn -- cnt)
  0 swap 	( cnt cn )
  begin dup 1 > while
  cn+1 dup
	if swap 1 + swap ( zaehlen )
	else drop 0
	then
  repeat
  drop
;
: cmax 		( k -- max )
  0 swap	( max k )
  begin dup 0 > while
    dup ccnt 	( max k cnt )
    rot  	( k cnt max )
    max		( k max )
    swap	( max k )
    1 -		( max k-1 )
  repeat
  drop
;
: coll-bench 384 cmax drop ;

cr .( ddbench:  )  ' ddbench show-cycles
cr .( intcalcs: )  ' intcalcs show-cycles
cr .( fib2:     )  ' fib2-bench show-cycles
cr .( nesting:  )  ' 20th show-cycles
cr .( sieve:    )  ' sieve-bench show-cycles
cr .( gcd1:     )  ' gcd1-bench show-cycles
cr .( pal:      )  ' pal-bench show-cycles
cr .( coll:     )  ' coll-bench show-cycles
cr .( Total:    )  total-cycles 2@ ud. .( cycles.)
cr
bye

