\ ------------------------------------------------------------------------
testing block words: BLK SCR LOAD THRU BUFFER BLOCK UPDATE FLUSH \

marker block_tests
decimal
\ Bring in a 4-block ram drive
T{ 4 block-ramdrive-init -> }T

\ Put a string into the first block.
T{ : teststring s" Testing Blocks!" ; -> }T
T{ : teststringlength teststring swap drop ; -> }T
T{ teststring 0 block swap move update flush -> }T

\ See if it's in the ramdrive.
T{ ramdrive teststringlength teststring compare -> 0 }T

\ Bring in the editor wordlist.
editor-wordlist >order

T{ 0 erase-screen flush -> }T

\ Make sure the test string from before is gone by looking for space
\ at the beginning of the ramdrive.
T{ s"           " ramdrive 10 compare -> 0 }T

\ Enter screens for testing LOAD and THRU
1 enter-screen
( Test screen 1 )
( 1 ) variable testvalue
( 2 ) 5 testvalue !
( 3 ) variable blkinscreen
( 4 ) blk @ blkinscreen !
( 5 ) variable blkinstring
( 6 ) s" blk @ blkinstring !" evaluate
( 7 ) variable blkinscreenA
( 8 ) blk @ blkinscreenA !
\ ( 9 ) 20 testvalue +! \ Shouldn't run
( 10 ) \ Test comments in blocks 1 testvalue +!
( 11 ) 1 testvalue +! \ This shoud be ignored 1 testvalue +!
( 12 )                                                         \ ( 13 ) 1 testvalue +!
( 14 ) \ The above has a backslash as the very last character
( 15 ) \ on the line so no CR needed before giving line 13

\ Load screen 1 and then check for the expected side effects.
T{ 1 load -> }T

\ BLK should be 0 while in the string and 1 while in the rest of the block.
T{ blkinscreen @  -> 1 }T
T{ blkinstring @  -> 0 }T
T{ blkinscreenA @ -> 1 }T
T{ testvalue @    -> 7 }T

2 enter-screen
( Test screen 2 )
( 1 ) ( Test loading another screen )
( 2 ) variable shouldbe19 ( when "2 3 thru" is run)
( 3 ) 5 shouldbe19 !
( 4 ) variable blkinscreenB
( 5 ) blk @ blkinscreenB ! ( should be 2 )
( 6 ) 3 load
( 7 ) blk @ constant blkinscreenD ( try a different method )
( 8 )
( 9 )
( 10 )
( 11 )
( 12 )
( 13 )
( 14 ) \ Testing backslash at very end of a block.
( 15 )                                                         \

3 enter-screen
( Test screen 2 )
( 1 )
( 2 )
( 3 ) shouldbe19 @ 7 + shouldbe19 !
( 4 )
( 5 )
( 6 )
( 7 )
( 8 )
( 9 )
( 10 )
( 11 )
( 12 )
( 13 )
( 14 ) ( blkinscreen4 should be 3 because we are on screen 3 )
( 15 ) ( testing the last line )     blk @ constant blkinscreenC

T{ 2 list -> }T
T{ scr @ -> 2 }T \ Screen 2 is the last one we listed.

\ Load screens 2 and 3 with THRU and check for side effects.
\ shouldbe19 should be 5 + 7 + 7 because screen 2 loads screen 3.
T{ 2 3 thru -> }T

T{ blkinscreenB @ -> 2  }T \ Note: blkinscreen3 is a variable.
T{ blkinscreenC   -> 3  }T \ Note: blkinscreen4 is a constant.
T{ blkinscreenD   -> 2  }T \ Note: blkinscreen5 is a constant.
T{ shouldbe19 @   -> 19 }T

:noname
    s" block-c65-init" find-name ?dup if
        name>int execute if
            ." testing c65 block read/write"
            $c000 1 block-write     \ write a block from ROM
            $400 1 block-read       \ read it back to block buffer
            $c000 $400 $400 $400 compare    \ unchanged?
        else
            ." no c65 block device"
            0                       \ pass the null test
        then
    else
        ." this build doesn't include block-c65-init"
        0
    then
;
T{ execute -> 0 }T

\ Release all of the memory used.
block_tests
