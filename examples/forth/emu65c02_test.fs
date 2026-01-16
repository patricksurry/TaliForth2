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

\ bulk tests follow

