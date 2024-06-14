\    $f006   start   Reading here starts the cycle counter
\    $f007   stop    Reading here stops the cycle counter
\    $f008-b cycles  Current 32 bit cycle count in NUXI order

$f006 constant t_start
$f007 constant t_stop
$f008 constant t_cycles

2variable t_overhead
2variable total-cycles

true strip-underflow !

: cycles ( xt -- ud )
    t_start @ drop execute t_stop @ drop
    t_cycles 2@ t_overhead 2@ d-
;

: show-cycles ( xt -- )
    cycles
    2dup total-cycles 2@ d+ total-cycles 2!
    ud. ." cycles." cr
;

0. total-cycles 2!
0. t_overhead 2! ' align cycles t_overhead 2!

\ benchmarks from https://theultimatebenchmark.org/#sec-13
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

cr .( ddbench: )    ' ddbench show-cycles
cr .( intcalcs: )   ' intcalcs show-cycles
cr .( fib2-bench: ) ' fib2-bench show-cycles
cr .( nesting: )    ' 20th show-cycles
cr .( sieve-bench: ) ' sieve-bench show-cycles
cr .( gcd1-bench: ) ' gcd1-bench show-cycles
cr .( pal-bench: )  ' pal-bench show-cycles
cr .( coll-bench: ) ' coll-bench show-cycles
cr .( Complete: ) total-cycles 2@ ud. .( cycles )
cr
bye

