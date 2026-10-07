using System;
using System.Collections.Generic;
using System.ComponentModel;
using SciLors_Mashed_Trainer.Core;

namespace SciLors_Mashed_Trainer.Types {
    public abstract class BaseMemorySharp : INotifyPropertyChanged {
        protected const int PROCESS_BASE = 0x400000;

        public IMemoryTarget Target { get; protected set; }

        public BaseMemorySharp(BaseMemorySharp child) : this(child.Target) { }
        public BaseMemorySharp(IMemoryTarget target) {
            this.Target = target;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged() {
            foreach (var prop in GetType().GetProperties()) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop.Name));
            }
        }
        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
