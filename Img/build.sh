#!/bin/sh
# Renders svg/*.svg to the fallback PNGs used by the trainer (needs rsvg-convert; rendered at 2x for crisp scaling in the GUI).
set -e
cd "$(dirname "$0")"
for f in svg/*.svg; do
  rsvg-convert -z 2 "$f" -o "$(basename "${f%.svg}").png"
done
