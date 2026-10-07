using System;
using System.Runtime.InteropServices;

namespace SciLors_Mashed_Trainer.Core {
    public class MockMemoryTarget : IMemoryTarget {
        private const int PROCESS_BASE_INT = 0x400000;
        private const int MEM_SIZE = 0x600000; // 6 MB covering up to 0xA00000

        private readonly byte[] _memory;
        private bool _disposed;
        private int _allocCounter = 0x980000;

        public bool IsRunning => !_disposed;
        public int ProcessId => 1337;
        public IntPtr ProcessBase => new IntPtr(PROCESS_BASE_INT);

        public MockMemoryTarget() {
            _memory = new byte[MEM_SIZE];
            InitializeMockState();
        }

        private void InitializeMockState() {
            // Player count: 4 (0x8D8B30)
            Write<int>(new IntPtr(0x8D8B30), 4);
            // Max points: 8 (0x658DE4)
            Write<int>(new IntPtr(0x658DE4), 8);
            // Game active: 1 (0x6AE110)
            Write<int>(new IntPtr(0x6AE110), 1);

            // Floats for patches
            Write<float>(new IntPtr(0x5DD620), 10.0f);
            Write<float>(new IntPtr(0x5DDB14), 7.0f);
            Write<float>(new IntPtr(0x5DE290), 50.0f);
            Write<float>(new IntPtr(0x5DDB18), 0.8f);
            Write<float>(new IntPtr(0x5DD41C), 5.0f);
            Write<float>(new IntPtr(0x5DD330), 1.0f);

            // Initialize 4 players
            for (int i = 0; i < 4; i++) {
                int pAddr = 0x8B06E0 + (i * 0xD04);
                int dmgAddr = 0x65A9E8 + (i * 0x28);
                int ptsAddr = 0x8D8B40 + (i * 4);

                // Alive: 1
                Write<int>(new IntPtr(pAddr + 0x004), 1);
                // Controls disabled: 0
                Write<int>(new IntPtr(pAddr + 0x010), 0);
                // Bot flag: P1 human (0), P2-4 bot (1)
                Write<int>(new IntPtr(pAddr + 0xD00), i > 0 ? 1 : 0);

                // Points
                Write<int>(new IntPtr(ptsAddr), 2 + i);

                // Matrix buffer index
                Write<int>(new IntPtr(pAddr + 0x9AC), 0);

                // Double buffer matrices (0x928 and 0x968)
                for (int buf = 0; buf < 2; buf++) {
                    int mAddr = pAddr + (buf == 0 ? 0x928 : 0x968);
                    // Right vector: (1, 0, 0)
                    Write<float>(new IntPtr(mAddr + 0x00), 1.0f);
                    Write<float>(new IntPtr(mAddr + 0x04), 0.0f);
                    Write<float>(new IntPtr(mAddr + 0x08), 0.0f);
                    // Up vector: (0, 1, 0)
                    Write<float>(new IntPtr(mAddr + 0x10), 0.0f);
                    Write<float>(new IntPtr(mAddr + 0x14), 1.0f);
                    Write<float>(new IntPtr(mAddr + 0x18), 0.0f);
                    // At vector: (0, 0, 1)
                    Write<float>(new IntPtr(mAddr + 0x20), 0.0f);
                    Write<float>(new IntPtr(mAddr + 0x24), 0.0f);
                    Write<float>(new IntPtr(mAddr + 0x28), 1.0f);
                    // Pos vector: (X, Y, Z)
                    Write<float>(new IntPtr(mAddr + 0x30), 10.0f * (i + 1));
                    Write<float>(new IntPtr(mAddr + 0x34), 1.0f);
                    Write<float>(new IntPtr(mAddr + 0x38), 20.0f * (i + 1));
                }

                // Distance
                Write<float>(new IntPtr(0x8C7E40 + (i * 4)), 5.0f * i);

                // Damage: Front 0%, Back 0%
                Write<float>(new IntPtr(dmgAddr + 0x00), 0.0f);
                Write<float>(new IntPtr(dmgAddr + 0x04), 0.0f);
            }

            // Mock equipped weapons: Player 1 has Rocket (0x6A5FF0), Player 2 has Flamethrower (0x6A9588)
            Write<int>(new IntPtr(0x8BEDCC), 0x6A5FF0);
            Write<int>(new IntPtr(0x8BEDCC + 0xB4), 0x6A9588);
        }

        private int GetIndex(IntPtr address) {
            int addr = address.ToInt32();
            if (addr < PROCESS_BASE_INT || addr >= (PROCESS_BASE_INT + MEM_SIZE)) {
                return -1;
            }
            return addr - PROCESS_BASE_INT;
        }

        public unsafe T Read<T>(IntPtr address) where T : unmanaged {
            int idx = GetIndex(address);
            if (idx < 0 || idx + sizeof(T) > MEM_SIZE) return default;
            fixed (byte* p = &_memory[idx]) {
                return *(T*)p;
            }
        }

        public unsafe void Write<T>(IntPtr address, T value) where T : unmanaged {
            int idx = GetIndex(address);
            if (idx < 0 || idx + sizeof(T) > MEM_SIZE) return;
            fixed (byte* p = &_memory[idx]) {
                *(T*)p = value;
            }
        }

        public byte[] ReadBytes(IntPtr address, int count) {
            int idx = GetIndex(address);
            if (idx < 0 || idx + count > MEM_SIZE) return new byte[count];
            byte[] buf = new byte[count];
            Array.Copy(_memory, idx, buf, 0, count);
            return buf;
        }

        public void WriteBytes(IntPtr address, byte[] buffer) {
            if (buffer == null || buffer.Length == 0) return;
            int idx = GetIndex(address);
            if (idx < 0 || idx + buffer.Length > MEM_SIZE) return;
            Array.Copy(buffer, 0, _memory, idx, buffer.Length);
        }

        public IntPtr Allocate(int size) {
            int addr = _allocCounter;
            _allocCounter += (size + 15) & ~15;
            return new IntPtr(addr);
        }

        public void Free(IntPtr address) {
            // No-op in mock
        }

        public int ExecuteStdcall(IntPtr functionAddress, params int[] arguments) {
            // Mock execution success
            return 0;
        }

        public void Dispose() {
            _disposed = true;
        }
    }
}

