using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoNotifyPropertyChanged
{
    public abstract class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string PropertyName)
        {
            var handler = PropertyChanged; 
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(PropertyName));
            }
        }
    }
}
