This is where all of Tali's native forth words are defined.

At the top level `taliforth.asm` includes `all.asm` here which defines
the triad COLD, ABORT, QUIT and then includes the remaining word groups.
Some words are conditionally included according to the platform
configuration of TALIFORTH_OPTIONAL_WORDS (see platform/README.md).
Dictionary entries (name tokens) for all words are defined in `headers.asm`.
Note there are several distinct wordlists.

The core words are organized into several functional groups for ease of understanding.
Looking at `core_juggle` and `core_math` gives a good way to learn the basics of how forth words are implemented in assembly.
Compilation and control flow in `core_compile` and `core_flow` are more complicated,
and understanding `core_meta` will reveal many of Forth's deepest mysteries.

- `core_compile.asm` - compilation words and native code generation
- `core_flow.asm` - flow control words like IF and LOOP
- `core_input.asm` - input management
- `core_juggle.asm` - stack juggling words like DUP and SWAP
- `core_math.asm` - 16-bit arithmetic and logical operations
- `core_mem.asm` - memory and string management
- `core_meta.asm` - words to create and manage words
- `core_misc.asm` - miscellanea like ENV, BASE, etc
- `core_output.asm` - output management

The words are organized following https://forth-standard.org/standard/words
along with some words specific to Tali Forth 2.  These extensions include:

- `tali.asm` low-level builtin helper words
- `assembler.asm` a 65c02 assembler
- `disasm.asm` a 65c02 disassembler
- `ed.asm` a line-based editor
- `editor.asm` a block-based editor


