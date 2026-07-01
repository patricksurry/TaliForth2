#!/usr/bin/env python3
import os
import sys
import shutil
import re
import argparse

def main():
    parser = argparse.ArgumentParser(description="Universal TaliForth ZP/DP Addressing Offset Patcher")
    parser.add_argument("--platform", required=True, help="Platform name (e.g., pce)")
    parser.add_argument("--prefix", default="zpage+", help="Variable prefix to inject (e.g., zpage+)")
    parser.add_argument("--clean", action="store_true", help="Clean up the temporary workspace instead of building")
    args = parser.parse_args()

    base_dir = os.getcwd()
    platform_dir = os.path.join(base_dir, "platform", args.platform)
    build_dir = os.path.join(platform_dir, f"build_{args.platform}")
    
    # If the clean flag is given, just wipe the folder and exit
    if args.clean:
        if os.path.exists(build_dir):
            shutil.rmtree(build_dir)
        print(f"--- Staging workspace [platform/{args.platform}/build_{args.platform}] removed ---")
        return

    print(f"--- Preparing pristine temporary workspace inside platform/{args.platform} ---")
    if os.path.exists(build_dir):
        shutil.rmtree(build_dir)
    os.makedirs(os.path.join(build_dir, "platform", args.platform), exist_ok=True)
    
    print("--- Mirroring core source configuration to staging directory ---")
    for item in ["taliforth.asm", "definitions.asm", "stringtable.asm", "words"]:
        if os.path.isdir(item):
            shutil.copytree(item, os.path.join(build_dir, item))
        else:
            shutil.copy2(item, os.path.join(build_dir, item))

    # Mirror everything inside the specific platform source directory
    dest_platform_path = os.path.join(build_dir, "platform", args.platform)
    for filename in os.listdir(platform_dir):
        src_file = os.path.join(platform_dir, filename)
        if os.path.isfile(src_file) and filename.endswith((".asm", ".asc")):
            shutil.copy2(src_file, os.path.join(dest_platform_path, filename))
        elif os.path.isdir(src_file) and filename != f"build_{args.platform}":
            shutil.copytree(src_file, os.path.join(dest_platform_path, filename))

    print(f"--- Patching zero-page offsets using prefix '{args.prefix}' ---")
    
    # REGEX 1: Standard indexing (captures any preceding text, whitespace, or commas cleanly)
    # Group 1: Leading context boundary (\s+ or ,)
    # Group 2: Integer or Hex value (e.g., 0 or $ff)
    # Group 3: Indexing Register (x or X)
    regex_std = re.compile(r'(\s+|,)(?<!zpage\+)(?<!\+)(\d+|\$[0-9a-fA-F]+),([xX])\b')
    
    # REGEX 2: Indirect indexing layouts (e.g., "(0,x)" through "(15,x)")
    # Group 1: Integer offset
    # Group 2: Indexing Register (x or X)
    regex_ind = re.compile(r'(?<!zpage\+)(?<!\+)\((\d+),([xX])\)')

    # Build target file inventory
    words_dir = os.path.join(build_dir, "words")
    files_to_patch = [os.path.join(words_dir, f) for f in os.listdir(words_dir) if f.endswith(".asm")]
    files_to_patch.append(os.path.join(build_dir, "taliforth.asm"))

    for filepath in files_to_patch:
        with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
            lines = f.readlines()

        patched_lines = []
        for line in lines:
            # Separate pure comments from executable logic to safeguard inline documentation
            if ';' in line:
                code_part, comment_part = line.split(';', 1)
                comment_part = ';' + comment_part
            else:
                code_part = line
                comment_part = ''

            # Mutate active instructions exclusively
            if code_part.strip():
                # \1 keeps the exact leading whitespace/separator, avoiding accidental spaces
                code_part = regex_std.sub(r'\1' + f"{args.prefix}\\2,\\3", code_part)
                code_part = regex_ind.sub(f"({args.prefix}\\1,\\2)", code_part)
                
                # Intercept and fix the unique 65C02 multi-pass indirect vector jump in core.asm
                if 'jmp ($fffe,x)' in code_part:
                    code_part = code_part.replace('jmp ($fffe,x)', f'; jmp ($fffe,x)\n\tjmp (({args.prefix}$fffe) & $ffff, x)')

            patched_lines.append(code_part + comment_part)

        # Write the cleanly translated files out to the staging space
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write("".join(patched_lines))

    print("--- Staging workspace patching completed successfully ---")

if __name__ == "__main__":
    main()
