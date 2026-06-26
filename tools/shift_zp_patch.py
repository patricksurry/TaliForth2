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
            
    # Mirror platform specific files dynamically
    dest_platform_path = os.path.join(build_dir, "platform", args.platform)
    for filename in os.listdir(platform_dir):
        src_file = os.path.join(platform_dir, filename)
        if os.path.isfile(src_file) and filename.endswith((".asm", ".asc")):
            shutil.copy2(src_file, os.path.join(dest_platform_path, filename))
        elif os.path.isdir(src_file) and filename != f"build_{args.platform}":
            shutil.copytree(src_file, os.path.join(dest_platform_path, filename))

    print(f"--- Patching zero-page offsets using prefix '{args.prefix}' ---")
    # Regular expressions to safely capture and shift zero-page references
    regex_std = re.compile(r'\b([0-6]),([xX])\b')
    regex_ind = re.compile(r'\(([0-6]),([xX])\)')
    regex_hex = re.compile(r'\$([fF][fF]),([xX])')

    # Target staging files to mutate
    words_dir = os.path.join(build_dir, "words")
    files_to_patch = [os.path.join(words_dir, f) for f in os.listdir(words_dir) if f.endswith(".asm")]
    files_to_patch.append(os.path.join(build_dir, "taliforth.asm"))

    for filepath in files_to_patch:
        with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
            content = f.read()

        content = regex_std.sub(f'{args.prefix}\\1,\\2', content)
        content = regex_ind.sub(f'({args.prefix}\\1,\\2)', content)
        content = regex_hex.sub(f'{args.prefix}$\\1,\\2', content)

        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)

    print("--- Local staging workspace is ready for 64tass ---")

if __name__ == "__main__":
    main()
