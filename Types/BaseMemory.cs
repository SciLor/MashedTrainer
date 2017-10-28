using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace SciLors_Mashed_Trainer.Types {
    public abstract class BaseMemory : INotifyPropertyChanged {
        
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hObject);

        [Flags]
        public enum ProcessAccessType {
            PROCESS_TERMINATE = (0x0001),
            PROCESS_CREATE_THREAD = (0x0002),
            PROCESS_SET_SESSIONID = (0x0004),
            PROCESS_VM_OPERATION = (0x0008),
            PROCESS_VM_READ = (0x0010),
            PROCESS_VM_WRITE = (0x0020),
            PROCESS_DUP_HANDLE = (0x0040),
            PROCESS_CREATE_PROCESS = (0x0080),
            PROCESS_SET_QUOTA = (0x0100),
            PROCESS_SET_INFORMATION = (0x0200),
            PROCESS_QUERY_INFORMATION = (0x0400)
        }

        [Flags]
        public enum AllocationType {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }
        [Flags]
        public enum MemoryProtection {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }
        
        //SeDebugPrivilege

        [DllImport("kernel32.dll")]
        private static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern Int32 WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll")]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

        public IntPtr hProcess;
        public BaseMemory(BaseMemory child) : this(child.hProcess) { }
        public BaseMemory(IntPtr hProcess) {
            this.hProcess = hProcess;
        }
        private Int32 ReadMemory(int address, byte[] buffer) {
            IntPtr bytesRead = IntPtr.Zero;
            return ReadProcessMemory(hProcess, new IntPtr(address), buffer, (uint)buffer.Length, out bytesRead);
        }
        private Int32 WriteMemory(int address, byte[] buffer) {
            IntPtr bytesWritten = IntPtr.Zero;
            return WriteProcessMemory(hProcess, new IntPtr(address), buffer, (uint)buffer.Length, out bytesWritten);
        }
        
        protected bool ReadBoolean(int address) {
            byte buffer = ReadByte(address);
            if (buffer == 1)
                return true;
            return false;
        }
        protected byte ReadByte(int address) {
            byte[] buffer = new byte[1];
            ReadMemory(address, buffer);
            return buffer[0];
        }
        protected short ReadShort(int address) {
            byte[] buffer = new byte[2];
            ReadMemory(address, buffer);
            return BitConverter.ToInt16(buffer, 0);
        }
        protected int ReadInt(int address) {
            byte[] buffer = new byte[4];
            ReadMemory(address, buffer);
            return BitConverter.ToInt32(buffer, 0);
        }
        protected long ReadLong(int address) {
            byte[] buffer = new byte[8];
            ReadMemory(address, buffer);
            return BitConverter.ToInt64(buffer, 0);
        }
        protected float ReadFloat(int address) {
            byte[] buffer = new byte[4];
            ReadMemory(address, buffer);
            return BitConverter.ToSingle(buffer, 0);
        }
        protected double ReadDouble(int address) {
            byte[] buffer = new byte[8];
            ReadMemory(address, buffer);
            return BitConverter.ToDouble(buffer, 0);
        }

        /*
        protected void Write(IntPtr address, object value) {
            //Dirty!
            Write(address, BitConverter.GetBytes((dynamic)value));
        }*/
        protected void Write(int address, bool value) {
            if (value) {
                Write(address, (byte)0x01);
            } else {
                Write(address, (byte)0x00);
            }
        }
        protected void Write(int address, byte value) {
            Write(address, BitConverter.GetBytes(value));
        }
        protected void Write(int address, short value) {
            Write(address, BitConverter.GetBytes(value));
        }
        protected void Write(int address, int value) {
            Write(address, BitConverter.GetBytes(value));
        }
        protected void Write(int address, long value) {
            Write(address, BitConverter.GetBytes(value));
        }
        protected void Write(int address, float value) {
            Write(address, BitConverter.GetBytes(value));
        }
        protected void Write(int address, double value) {
            Write(address, BitConverter.GetBytes(value));
        }
        private void Write(int address, byte[] buffer) {
            WriteMemory(address, buffer);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged() {
            foreach (var prop in GetType().GetProperties()) {
                PropertyChanged(this, new PropertyChangedEventArgs(prop.Name));
            }
        }
    }
}
