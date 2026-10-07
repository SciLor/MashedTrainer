using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SciLors_Mashed_Trainer.Types {
    public class Position : INotifyPropertyChanged {
        private float x;
        public float X {
            get => x;
            set => SetField(ref x, value, nameof(X));
        }

        private float y;
        public float Y {
            get => y;
            set => SetField(ref y, value, nameof(Y));
        }

        private float z;
        public float Z {
            get => z;
            set => SetField(ref z, value, nameof(Z));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
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

        public float GetDistance(Position position) {
            return (float)Math.Sqrt(
                Math.Pow(X - position.X, 2) +
                Math.Pow(Y - position.Y, 2) +
                Math.Pow(Z - position.Z, 2)
            );
        }
    }
}
