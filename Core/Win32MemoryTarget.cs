using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SciLors_Mashed_Trainer.Core {
    public class Win32MemoryTarget : IMemoryTarget {
        private const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RESERVE = 0x2000;
        private const uint MEM_RELEASE = 0x8000;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);

        private readonly Process _process;
        private readonly IntPtr _hProcess;
        private bool _disposed;

        public bool IsRunning => !_process.HasExited;
        public int ProcessId => _process.Id;
        public IntPtr ProcessBase => _process.MainModule?.BaseAddress ?? new IntPtr(0x400000);

        public Win32MemoryTarget(Process process) {
            _process = process ?? throw new ArgumentNullException(nameof(process));
            _hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
            if (_hProcess == IntPtr.Zero) {
                throw new InvalidOperationException($"Failed to open process {process.Id} (Error {Marshal.GetLastWin32Error()})");
            }
        }

        public unsafe T Read<T>(IntPtr address) where T : unmanaged {
            int size = sizeof(T);
            byte[] buffer = new byte[size];
            if (!ReadProcessMemory(_hProcess, address, buffer, size, out _)) {
                return default;
            }
            fixed (byte* p = buffer) {
                return *(T*)p;
            }
        }

        public unsafe void Write<T>(IntPtr address, T value) where T : unmanaged {
            int size = sizeof(T);
            byte[] buffer = new byte[size];
            fixed (byte* p = buffer) {
                *(T*)p = value;
            }
            WriteProcessMemory(_hProcess, address, buffer, size, out _);
        }

        public byte[] ReadBytes(IntPtr address, int count) {
            byte[] buffer = new byte[count];
            ReadProcessMemory(_hProcess, address, buffer, count, out _);
            return buffer;
        }

        public void WriteBytes(IntPtr address, byte[] buffer) {
            if (buffer == null || buffer.Length == 0) return;
            WriteProcessMemory(_hProcess, address, buffer, buffer.Length, out _);
        }

        public IntPtr Allocate(int size) {
            return VirtualAllocEx(_hProcess, IntPtr.Zero, size, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
        }

        public void Free(IntPtr address) {
            if (address != IntPtr.Zero) {
                VirtualFreeEx(_hProcess, address, 0, MEM_RELEASE);
            }
        }

        public int ExecuteStdcall(IntPtr functionAddress, params int[] arguments) {
            // Build 32-bit x86 caller stub:
            // Push arguments in reverse order (stdcall)
            // Call functionAddress
            // Ret
            int argCount = arguments?.Length ?? 0;
            // 5 bytes per push imm32 (0x68 <val>)
            // 5 bytes call rel32 or 6 bytes call [imm32] or 5 bytes mov eax, addr; call eax (0xB8 <addr>; 0xFF 0xD0)
            // 1 byte ret (0xC3)
            int stubSize = (argCount * 5) + 7 + 1;
            IntPtr stubAddr = Allocate(stubSize);
            if (stubAddr == IntPtr.Zero) return -1;

            byte[] stub = new byte[stubSize];
            int offset = 0;

            // Push args reverse
            for (int i = argCount - 1; i >= 0; i--) {
                stub[offset++] = 0x68; // push imm32
                Array.Copy(BitConverter.GetBytes(arguments[i]), 0, stub, offset, 4);
                offset += 4;
            }

            // mov eax, functionAddress
            stub[offset++] = 0xB8;
            Array.Copy(BitConverter.GetBytes(functionAddress.ToInt32()), 0, stub, offset, 4);
            offset += 4;

            // call eax
            stub[offset++] = 0xFF;
            stub[offset++] = 0xD0;

            // ret
            stub[offset++] = 0xC3;

            WriteBytes(stubAddr, stub);

            IntPtr hThread = CreateRemoteThread(_hProcess, IntPtr.Zero, 0, stubAddr, IntPtr.Zero, 0, out _);
            int exitCode = 0;
            if (hThread != IntPtr.Zero) {
                WaitForSingleObject(hThread, 5000);
                GetExitCodeThread(hThread, out uint uExitCode);
                exitCode = (int)uExitCode;
                CloseHandle(hThread);
            }

            Free(stubAddr);
            return exitCode;
        }

        public void Dispose() {
            if (!_disposed) {
                if (_hProcess != IntPtr.Zero) {
                    CloseHandle(_hProcess);
                }
                _disposed = true;
            }
        }
    }
}

