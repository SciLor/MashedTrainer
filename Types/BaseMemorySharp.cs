using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Binarysharp.MemoryManagement;
using System.Diagnostics;

namespace SciLors_Mashed_Trainer.Types {
    public abstract class BaseMemorySharp : INotifyPropertyChanged
    {
        protected const int PROCESS_BASE = 0x400000;

        public MemorySharp Process;
        public BaseMemorySharp(BaseMemorySharp child) : this(child.Process) { }
        public BaseMemorySharp(MemorySharp process) {
            this.Process = process;
        }
        public BaseMemorySharp(Process process) {
            this.Process = new MemorySharp(process);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged() {
            foreach (var prop in GetType().GetProperties()) {
                PropertyChanged(this, new PropertyChangedEventArgs(prop.Name));
            }
        }
        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, string propertyName) {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
