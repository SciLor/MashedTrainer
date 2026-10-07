using System;

namespace SciLors_Mashed_Trainer.Core {
    public interface IMemoryTarget : IDisposable {
        bool IsRunning { get; }
        int ProcessId { get; }
        IntPtr ProcessBase { get; }

        T Read<T>(IntPtr address) where T : unmanaged;
        void Write<T>(IntPtr address, T value) where T : unmanaged;

        byte[] ReadBytes(IntPtr address, int count);
        void WriteBytes(IntPtr address, byte[] buffer);

        IntPtr Allocate(int size);
        void Free(IntPtr address);

        int ExecuteStdcall(IntPtr functionAddress, params int[] arguments);
    }
}

