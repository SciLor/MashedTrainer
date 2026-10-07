# Trainer cross-check against MFL.exe

Everything below was checked against the real `MFL.exe` (objdump + the Ghidra project `mfl`).
Names/comments are in `ghidra/names/MFL_names.csv` (applied to the project), decompilation and
listing of the relevant functions in `ghidra/output/named/MFL_trainer.{c,asm}`.
The weapon ASM stubs are covered in `WeaponAsm_Analysis.md`. **Nothing here was run in the game**
(no Windows/.NET toolchain on the analysis machine): the C# changes are uncompiled and untested.

## Verified OK
| Trainer | MFL.exe |
|---|---|
| Player weapon slots `0x8BED20`, stride `0xB4`, `+0xA8/+0xAC` | confirmed (`FUN_00467d90`, `player_equip_weapon`) |
| Weapon id whitelist (9 ids) | = descriptor table `0x60DC20` (9 entries) |
| Weapon state arrays (Drum, MG, Rocket, Mines, Flame, Shotgun, Flash, Oil, Mortar: base + stride) | all 9 match the equip functions (`weapon_*_equip`) |
| `BASE_WEAPON_ADDRESS 0x8BEDCC` = slot `+0xAC` = pointer into the state array | confirmed; `Player.WeaponPointer` lookup by exact element works. Simpler/robust alternative: `[ [slot+0xA8] ]` is the weapon type id |
| Player car struct `0x8B06E0` stride `0xD04`, matrix at `+0x928` | confirmed (RwMatrix, `+0x30` pos, RW is Y-up so `+0x34` is height) |
| Points `0x8D8B40`, visual `+0x20`, change `+0x40`, player count `0x8D8B30`, distance array `0x8C7E40`, damage `0x65A9E8` stride `0x28` | found with the expected stride/writers |
| Camera operand sites `0x450BC0` (divider 5.0) / `0x450BC6` (add 1.0) | correct (the old Cheat Engine labels for `0x450BB8/BE/C4` are instruction starts, operands are +2) |
| `0x41340D`, `0x423FEC`, `0x423FFA` | correct operand/immediate sites |

## Bugs found (fixed in this change unless noted)
1. **Position buffer**: the car pose is double buffered. `0x8B06E0+0x928` and `+0x968` alternate; the live one is
   `[car+0x9AC]` (`cur = next; next = (next-1)&1` each tick, `0x47F2C1`). The trainer always used buffer 0, so
   reads/teleports/freeze were wrong about half the time. `Player` now reads the current buffer and writes both.
2. **Shared constants patched globally**: `CameraTiltMultiplicator` wrote the global `0x5DE290` (50.0, 20 users: HUD
   layout, particles, ...), `DistanceWarningThreshold` wrote `0x5DDB14` (7.0, 22 users incl. HUD). Now every feature
   redirects only its own instruction operands (`PatchedFloat` in `Game.cs`) and restores them on exit
   (the max-damage immediate was never restored before).
3. `ChangeWeapon.asm:57` typo (`0x467E6`), fixed earlier; `.bin` was fine.
4. **Max distance (NOT fixed, unverified)**: the patch at `0x41340D` sits in `game_mode_end_check` and does an
   *equality* compare of `[0x8C7E00]` with the value. `0x8C7E00` has no static writer (BSS, always 0.0), so this patch
   is probably dead. The real "too far away" logic is zoom/distance based (`distances_update`, `player_distance_warning`,
   camera zoom clamp). Needs a runtime check (hardware breakpoint on `0x8C7E00`) before relying on it.
5. `GAME_ACTIVE 0x6AE110` is the *race audio streams exist* flag (set in `soundengine_create_race_streams`), not a game
   state. Works as an in-race/not-paused proxy; menu/loading states are not distinguished.
6. `Game.MAXIMUM_POINTS 0x658DE4` is mode specific; `0x659338` is the same value written for another mode (`0x41FC10`).
   The trainer reads only the first.
7. Minor: `WeaponHelper.InitWeapon` tests `Drum` twice; `Game` finalizer runs `DoDispose` on a possibly finalized `Process`.

## New: camera
Camera function `camera_update` (`0x44FA30`), `zoom` (0..10, grows when players spread) :
- pitch = `tilt(50) * zoom / 10 - 5` (clamped by `[0x5DF474]`, so a big tilt needs that cap too)
- distance multiplier = `zoom / 0.8 / divider(5) + add(1)`  -> new knob **Height Distance factor** (`0x450BBA`)
- zoom is hard-clamped to **10.0** (3 compares + 4 `mov [ebp-x], 0x41200000` immediates) and normalised by 10.0 in
  25 places inside the function. A larger "max distance" is useless unless the clamp moves too -> new knob
  **Zoom limit** patches all 25 operands + 4 immediates together (so the `(10 - zoom)/10` smoothing terms stay >= 0).
Camera smoothing also multiplies by 0.8 (`0x5DDB18`) in ~12 other places; left alone.

## New: flip / turn
Car pose = RwMatrix `{right, up, at, pos}` (0x40 bytes) in the double buffer above; velocity at `car+0x144` (assumed world
space). `Player.Flip()` negates right and up (180 deg around forward) and lifts 1.5 units; `Player.Turn(deg)` rotates
right/at around up and rotates the velocity the same way. The game re-orthonormalises every tick
(`MatrixOrthoNormalize`), so exact orthonormal values are stable. Buttons "Flip" / "Turn 180" are in the player panel.
`IsOnRoof` = `up.y < 0`; Drive-over-revive now flips a revived car that lies on its roof.
Untested in game: axis/sign conventions (forward = `at`), the 1.5 lift, and whether wheel contact state needs a reset
after a flip.
