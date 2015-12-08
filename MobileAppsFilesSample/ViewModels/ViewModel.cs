using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace MobileAppsFilesSample
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var temp = PropertyChanged;
            if (temp != null)
            {
                temp(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
