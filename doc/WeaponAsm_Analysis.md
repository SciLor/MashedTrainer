# Weapon ASM injection — how it works, verified against MFL.exe

The trainer doesn't hook anything; it allocates two tiny hand-written x86
stubs (`Asm/ChangeWeapon.bin`, `Asm/DropWeapon.bin`, assembled from the
`.asm` sources next to them via FASM) inside the target process with
`Process.Memory.Allocate`, then calls them with `RemoteAllocation.Execute`
(`Types/Game.cs:137-166,300-306`). Each stub is a small wrapper around a
real game function, with validation the game itself doesn't do.

Addresses in the `.asm`/`.cs` are given relative to `MFL.exe`'s image base
`0x400000` (`PROCESS_BASE` in `BaseMemorySharp.cs:13`); I verified everything
below against the real `MFL.exe` (file offset == VA − 0x400000 for `.text`
and `.data`, since both sections are stored 1:1 — confirmed from the PE
section table).

## Player struct array

- Base `0x8BED20`, stride `0xB4`, 4 entries (ends at `0x8BEFF0`).
- `+0xA8`: pointer to the equipped weapon's "weapon system" descriptor (null
  = no weapon).
- `+0xAC`: return value of that descriptor's "equip" call — in practice also
  acts as a non-zero/zero flag in lockstep with `+0xA8`.

Confirmed by disassembling `FUN_00467d90`/`FUN_00467df0` (both loop
`puVar1 = 0x8BED20; ...; puVar1 += 0xB4; while(puVar1 < 0x8BEFF0)` and check
`*(puVar1+0xA8) != 0`) and matches `Player.cs:22` (`BASE_WEAPON_ADDRESS =
0x8BEDCC` = `0x8BED20 + 0xAC`) and `PLAYER_WEAPON_DISTANCE = 0xB4`.

## `0x467980` — real "unequip weapon" function

```
mov eax, [esi+0xa8]   ; esi = player struct pointer (register param, NOT stack!)
push esi
call [eax+0x10]       ; weapon_system->vtbl[4](player)  -- "deactivate"
xor eax, eax
add esp, 4
mov [esi+0xa8], eax
mov [esi+0xac], eax
ret                    ; plain ret — no stack args to clean up
```

Bug in the *game*: if `[esi+0xa8]` is already 0 (no weapon), `eax` is 0 and
`call [eax+0x10]` reads a function pointer from address `0x10` and jumps
there — an access violation. Every real caller in the game (`FUN_00467a60`,
`FUN_00467d90`, `FUN_00467df0`, `FUN_00467e60` itself) guards this with
`if (*(player+0xA8) != 0) call FUN_00467980();` before calling it.

`DropWeapon.asm` reproduces that guard, just checking `+0xAC` instead of
`+0xA8` (`cmp byte [eax+0xAC], 0 / je exit_function`) — functionally
equivalent since the two fields are always set/cleared together — then sets
`esi = player_struct_ptr` and calls `0x467980` with **no stack args**,
matching the real register-based ABI exactly. `ret 4` at the end only pops
the stub's own 1 parameter (`player_id`) for its C#-side stdcall caller;
unrelated to the inner call. **Correct.**

## `0x467e60` — real "equip weapon" function

```
push esi
mov esi, [esp+0xc]     ; = pre-push [esp+8]: 2nd arg  = weapon type id (lookup key)
call 0x467960           ; linear search of a 9-entry, 0x40-byte-stride table
                         ;   at 0x60DC20 for a slot whose first dword == esi;
                         ;   returns that slot's address, or 0 if not found
mov esi, [esp+0x8]      ; = pre-push [esp+4]: 1st arg  = player struct pointer
push esi
mov [esi+0xa8], eax      ; player.weaponSystem = found descriptor (or NULL!)
call [eax+0x4]            ; descriptor->vtbl[1](player)  -- "activate"
add esp, 4
mov [esi+0xac], eax
pop esi
ret                       ; plain ret, caller cleans up 2 pushed dwords (cdecl)
```

So the real signature is `cdecl void SetWeapon(void* player, int weaponTypeId)`,
first arg pushed last (closest to return address), stack cleaned by the
*caller*.

Bug in the *game*: if `weaponTypeId` doesn't match any of the 9 table
entries, `FUN_00467960` returns 0, which gets stored at `+0xa8` **and then
immediately dereferenced** (`call [eax+0x4]` = `call [0x4]`) — guaranteed
crash for any invalid weapon id. Ghidra's decompiler actually mis-detects
this function as taking a single parameter (because of the `push esi`
prologue shifting the stack-slot numbering it tracks); the raw disassembly
above is ground truth and is what the trainer's ASM was written against.

I pulled the real table (`0x60DC20`, count at `0x60DE60`) straight out of
the binary:

```
count = 9
ids = {0x9, 0xA, 0xB, 0xC, 0x10, 0x11, 0x12, 0x13, 0x7}
```

`ChangeWeapon.asm`'s weapon-id whitelist loop (9 bytes: `0x07,0x09,0x0A,
0x0B,0x0C,0x10,0x11,0x12,0x13`) is a byte-for-byte match of this table — it
exists specifically to stop an invalid id from reaching `0x467E60` and
crashing the game, and it's correct. `Weapons/Weapon.cs`'s `WeaponId` enum
uses the same 9 values. The stack layout the stub builds
(`push ecx /*weapon*/; push eax /*player*/; call 0x467E60; add esp, 8`) also
matches the real ABI exactly (arg1=player at `[esp+4]`, arg2=weaponId at
`[esp+8]`, caller-cleaned). **Correct**, despite looking wrong at first
glance from the Ghidra decompilation alone.

The "already equipped?" guard (`cmp byte [eax+0xAC], 0 / jne exit_function`)
mirrors why `Game.EquipWeapon()` (`Types/Game.cs:300-303`) always calls
`DropWeapon()` before `funcChangeWeapon` — the stub refuses to equip over an
existing weapon, so the caller must drop first.

## Bug found: stale immediate in `ChangeWeapon.asm`

`Asm/ChangeWeapon.asm:57` reads:

```
mov ebx, 0x467E6 ;workaround
```

— missing the trailing `0` (should be `0x467E60`). The checked-in
`ChangeWeapon.bin` is **not** stale — its bytes (`bb 60 7e 46 00` = `mov
ebx, 0x00467E60`) are correct, so the shipped trainer works. But the `.asm`
source and the `.bin` it's supposed to produce are out of sync: reassembling
`ChangeWeapon.asm` as-is with FASM today would silently produce a `mov ebx,
0x467E6`, i.e. jump to garbage code at `0x467E6` instead of `0x467E60`, and
corrupt the game's call stack / crash. `DropWeapon.asm`'s equivalent line
(`mov eax, 0x467980`) has no such typo and matches its `.bin`.

**Fix:** change line 57 to `mov ebx, 0x467E60`.
