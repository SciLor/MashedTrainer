#!/bin/sh
# Renders packs/<pack>/svg/*.svg to packs/<pack>/*.png (needs rsvg-convert).
# The PNGs are what the trainer embeds; the SVGs are the source (no game textures are shipped).
# Usage: ./build.sh [pack ...]   (default: all packs; packs: ingame = default, classic = legacy)
set -e
cd "$(dirname "$0")/packs"
for p in ${@:-*}; do
  for f in "$p"/svg/*.svg; do
    rsvg-convert -w 64 -h 64 "$f" -o "$p/$(basename "${f%.svg}").png"
  done
  echo "Rendered pack: $p"
done
