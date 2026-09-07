# ProDOS filesystem for Tali Forth 2

## TL;DR (current state)

A minimal **read-only** ProDOS filesystem reader in Forth (`prodos.fs`), plus
a block-device loader (`p8ldr.fs`) and device preamble (`p8pre.fs`) for
booting Tali from a ProDOS `.po` disk image on the
[c65](https://github.com/patricksurry/c65) simulator. The companion Python
tool [`../pyprodos`](../../../../pyprodos) creates and inspects the `.po`
images used here.

**Working:** directory traversal (`dir.`, `ls`), path resolution
(`path-find`, `cd`), sequential file reads across sparse/index/master-index
blocks (`fopen`/`fread`), and loading Forth source straight off the volume
(`load-file`, `include`).

**Not yet done / WIP:**
- No write support (create/delete/append) — read-only by design so far.
- `p8ldr.fs` and `p8pre.fs` are two alternate, untested boot-loader sketches
  for getting Tali to find and `evaluate*` the loader block from block 0;
  neither is wired into a platform build yet.
- The scratch session log below was run against
  `../pyprodos/images/GSOSv6.0.1.po` to try subdirectories, `FTYPE.APPLE`
  lookup, and reading `/PRODOS`. Some commands there predate the current
  word names (`dir-find`, `path-find`, `read-file` were renamed from
  `dirfind`, `pathfind`, `readfile`) and haven't been re-verified since.

## Session notes / scratch log

```
prodos create ../tali/test.po --size 1024 --loader ../tali/examples/forth/prodos/p8ldr.fs --force
prodos import ../tali/test.po ../tali/examples/forth/prodos/prodos.fs /
prodos ls ../tali/test.po

PRODOS.FS              2671 2/FF RW-BND 25-10-13T15:47 25-10-13T15:47 7 @ 7
    1 files in PYP8 F RW-BND 25-10-13T15:47


../c65/c65 -qq -r taliforth-c65.bin -b test.po

\ import prequel for p8init and p8read

blkbuffer 0 p8read          \ read boot sector (should also verify)
blkbuffer $200 evaluate*    \ get xt for loader
.s <1> 9B8  ok
execute                     \ run the loader
.s <0>  ok

13 roman                    \ file imported OK

\ ../c65/c65 -qq -r taliforth-c65.bin -b ../pyprodos/images/GSOSv6.0.1.po





volblk s" ICONS" dirfind dup $20 dump
entry@ dup dirhdr dir.
s" FTYPE.APPLE" dirfind
$2000 swap readfile


$2000 2000 erase
s" /PRODOS" pathfind
$2000 swap read-file


$2000 2000 erase
s" /PRODOS" pathfind
fopen $2000 68 1600. fread



s" /ICONS/FINDER.DATA" pathfind



s" /ICONS/FINDER.DATA" pathfind


: tr ( addr u oldch newch -- )
    \ replace oldch with newch in string
    swap 2swap bounds do
        ( newch oldch )
        i c@ over = if over i c! then
    loop
    2drop
;


load README 2dup $0d $0a tr cr type
```


