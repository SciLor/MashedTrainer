using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types {
    public class Position : INotifyPropertyChanged {

        private float x;
        public float X {
            get { return x; }
            set { SetField(ref x, value, "X"); }
        }
        private float y;
        public float Y {
            get { return y; }
            set { SetField(ref y, value, "Y"); }
        }
        private float z;
        public float Z {
            get { return z; }
            set { SetField(ref z, value, "Z"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
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

        public float GetDistance(Position position) {
            //a^2+b^2+c^2=d^2

            return (float)Math.Sqrt(
                Math.Pow(X - position.X, 2) +
                Math.Pow(Y - position.Y, 2) +
                Math.Pow(Z - position.Z, 2)
            );
        }
    }
}
