#!/bin/sh
# Rebuilds the flat binaries (*.bin) from the *.asm files with the bundled flat assembler (fasm/, v1.73.35).
set -e
cd "$(dirname "$0")"
for f in *.asm; do
  "${FASM:-./fasm/fasm}" "$f" "${f%.asm}.bin"
done
