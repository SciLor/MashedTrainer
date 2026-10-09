#!/usr/bin/env python3
"""Exports the game's own icons to Img/reference/ for SHOWCASE.md (needs Pillow for the loose BMPs):
  reference/ingame/<name>.png  from POWERUPICONS.TXD   (what the pickup cubes show; source of the 'ingame' pack)
  reference/classic/<name>.png from POWERUPS/textures/ (older loose 32x32 textures; source of the 'classic' pack); alpha dropped (the game draws them 43-65% transparent)
Local reference only (copyrighted, gitignored): the trainer ships the SVG based packs, never these files.
Usage: export_originals.py [game dir, default ../../MashedGame/FullyLoaded]"""
import sys
from pathlib import Path
from PIL import Image

here = Path(__file__).resolve().parent
sys.path.insert(0, str(here.parents[1] / "MashedFormats"))
import pitexd

game = Path(sys.argv[1]) if len(sys.argv) > 1 else here.parents[1] / "MashedGame/FullyLoaded"
pu = "App_Executables/TOASTART/Common/POWERUPS"
for sub in ("ingame", "classic"):
    (here / "reference" / sub).mkdir(parents=True, exist_ok=True)
for t in pitexd.load(game / "extracted" / pu / "Powerups/POWERUPICONS.TXD").textures:
    (here / "reference/ingame" / f"{t.name}.png").write_bytes(t.images[0].png())
for f in (game / "installed" / pu / "textures").iterdir():
    if f.suffix.lower() in (".png", ".bmp"):
        Image.open(f).convert("RGB").save(here / "reference/classic" / f"{f.stem}.png")
print(f"exported to {here / 'reference'}")
