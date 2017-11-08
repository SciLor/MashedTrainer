use32 ;Initialize as 32bit

_Drop_Weapon: ;/* change_weapon(int player_id)  */
  push ebp ;Save old Extended Base Pointer to Stack
  mov ebp, esp ;Read current Stack Pointer to Base Pointer
  sub esp, 0x00 ;Initialize local variables (0 byte = 0x00)

  push eax
  push ebx
  push esi

  mov esi, [ebp+08] ;Move player_id to esi

  ;/* Check if player is valid */
  cmp esi, 0 ;min playerid = 0
  jl exit_function
  cmp esi, 3 ;max playerid = 3
  jg exit_function

  ;/* Calculate player structure pointer  */
  mov eax, esi ;player_id to eax
  mov ebx, 0xB4 ;Player Structure offset
  mul bl ;player_id * offsett
  add eax, 0x8BED20 ;Add offset to base address of player structure

  ;/* Check if there is a weapon on the car */
  cmp byte [eax+0xAC], 0 ;Weapon equiped (check on Player Structure in eax)?
  je exit_function ;Jump to exit if there is noone

  mov esi, eax ;move player structure to esi (call param)

  mov eax, 0x467980 ;workaround
  call eax ;call 0x467980 ;Call original Remove-Weapon Function

  exit_function:
  pop esi
  pop ebx
  pop eax
  mov esp, ebp ;Remove local variables
  pop ebp ;Restore ebp
  ret 4 ;Throw away local variables and 2 parameters (2 params x 4 byte), so the caller doesn't need to cleanup with add esp, 8