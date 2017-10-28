using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Binarysharp.MemoryManagement;
using System.Diagnostics;

namespace SciLors_Mashed_Trainer.Types {
    public abstract class BaseMemorySharp : INotifyPropertyChanged {

        protected MemorySharp memory;
        public BaseMemorySharp(BaseMemorySharp child) : this(child.memory) { }
        public BaseMemorySharp(MemorySharp memory) {
            this.memory = memory;
        }
        public BaseMemorySharp(Process process) {
            memory = new MemorySharp(process);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged() {
            foreach (var prop in GetType().GetProperties()) {
                PropertyChanged(this, new PropertyChangedEventArgs(prop.Name));
            }
        }
    }
}
