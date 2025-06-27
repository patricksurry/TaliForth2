; platform_words.asm
; Put any words (written in assembly) that you would like to add for your
; platform here.  You need to create a header (with the #nt_header macro)
; and you need to mark the entry with an "xt_wordname:" label and the
; exit (just before the rts at the end) with a "z_wordname:" label where
; wordname is replaced with the name of your word.

; You will want to refer to words/headers.asm for a description of how to
; use the #nt_header macro and the Developer Guide section of the manual.

; Here is an example word that just puts 5 onto the Forth data stack.

;#nt_header
