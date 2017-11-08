using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Binarysharp.Assemblers.Fasm;
using System.IO;
using Binarysharp.MemoryManagement.Memory;
using Binarysharp.MemoryManagement.Native;
using Binarysharp.MemoryManagement.Assembly.CallingConvention;
using static SciLors_Mashed_Trainer.Types.Player;
using static SciLors_Mashed_Trainer.Types.Weapons.Weapon;
using SciLors_Mashed_Trainer.Types.Weapons;

namespace SciLors_Mashed_Trainer.Types {
    public class Game : BaseMemorySharp {
        private IntPtr PLAYER_COUNT = new IntPtr(0x8D8B30 - PROCESS_BASE);
        private IntPtr DISTANCE_THRESHOLD = new IntPtr(0x5DDB14 - PROCESS_BASE);

        private RemoteAllocation funcChangeWeapon;
        private RemoteAllocation funcDropWeapon;

        public int PlayerCount {
            get {
                //return Memory.Read<int>(PLAYER_COUNT);
                return Memory[PLAYER_COUNT].Read<int>();
            }
        }

        public float MaximumDistance
        {
            get { return 10; }
        }
        public float DistanceWarningThreshold
        {
            get { return Memory[DISTANCE_THRESHOLD].Read<float>(); }
            set { Memory[DISTANCE_THRESHOLD].Write<float>(value); }
        }

        public bool IsActive
        {
            get { return true; }
        }

        public WeaponHelper WeaponHelper;

        public Game(Process process) : base(process) {
            readAndInjectAsmFunctions();
            this.WeaponHelper = new WeaponHelper(this);
        }
        
        private void readAndInjectAsmFunctions() {
            byte[] asmBytes = File.ReadAllBytes("Asm\\ChangeWeapon.bin");
            funcChangeWeapon = Memory.Memory.Allocate(asmBytes.Length, MemoryProtectionFlags.ExecuteReadWrite);
            funcChangeWeapon.Write<byte>(asmBytes);

            asmBytes = File.ReadAllBytes("Asm\\DropWeapon.bin");
            funcDropWeapon = Memory.Memory.Allocate(asmBytes.Length, MemoryProtectionFlags.ExecuteReadWrite);
            funcDropWeapon.Write<byte>(asmBytes);
        }

        public void EquipWeapon(PlayerId playerId, WeaponId weaponId) {
            DropWeapon(playerId);
            funcChangeWeapon.Execute(CallingConventions.Stdcall, (int)playerId, (int)weaponId);
        }
        public void DropWeapon(PlayerId playerId) {
            funcDropWeapon.Execute(CallingConventions.Stdcall, (int)playerId);
        }

    }
}
