use32 ;Initialize as 32bit

_Change_Weapon: ;/* change_weapon(int player_id, int weapon_id)  */
  push ebp ;Save old Extended Base Pointer to Stack
  mov ebp, esp ;Read current Stack Pointer to Base Pointer
  sub esp, 0x0C ;Initialize local variables (12 byte = 0x0C)

  push eax
  push ebx
  push ecx
  push esi

  mov dword [ebp-0x04], 0x07090A0B ;Mortar, Machine Gun, Drum, Rocket,
  mov dword [ebp-0x08], 0x0C101112 ;Mines, Fire, Shotgun, Blind,
  mov dword [ebp-0x0C], 0x13000000 ;Oil

  mov esi, [ebp+08] ;Move player_id to esi

  ;/* Check if player is valid */
  cmp esi, 0 ;min playerid = 0
  jl exit_function
  cmp esi, 3 ;max playerid = 3
  jg exit_function

  ;/* Check if weapon is valid */
  mov eax, ebp ;Save known weapon pointer to eax
  sub eax, 9 ;Adjust local variable pointer to last entry of weaponlist

  xor ebx, ebx ;Clear ebx - loop index
  xor ecx, ecx ;Clear ecx - weapon_id
  mov cl, [ebp+0x0C] ;Load weapon_id of function param into cl of ecx

  weapon_loop:
  cmp [eax+ebx], cl ;weapon_id == known_weapon_id
  je exit_weapon_loop
  inc ebx ;index++
  cmp ebx, 9 ;index < 9?
  jl weapon_loop ;continue loop if valid index
  jmp exit_function ;Exit if weapon ID is not valid

  exit_weapon_loop:
  ;/* Calculate player structure pointer  */
  mov eax, esi ;player_id to eax
  mov ebx, 0xB4 ;Player Structure offset
  mul bl ;player_id * offsett
  add eax, 0x8BED20 ;Add offset to base address of player structure

  ;/* Check if there is already weapon on the car */
  cmp byte [eax+0xAC], 0 ;Weapon equiped (check on Player Structure in eax)?
  jne exit_function ;Jump to exit if there is already one

  ;/* Equip new weapon */
  push ecx ;Param 1: Weapon
  push eax ;Param 2: Player dependend Structure
  ;esi = player_id

  mov ebx, 0x467E6 ;workaround
  call ebx ;call 0x467E60 ;Call original Set-Weapon Function MFL.exe+67E60 - MFL.exe = 400000

  add esp, 8 ;Remove 2 full entries from Stack

  exit_function:
  pop esi
  pop ecx
  pop ebx
  pop eax
  mov esp, ebp ;Remove local variables (12 byte = 0x0C)
  pop ebp ;Restore ebp
  ret 8 ;Throw away local variables and 2 parameters (2 params x 4 byte), so the caller doesn't need to cleanup with add esp, 8