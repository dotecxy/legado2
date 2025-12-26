using Legado.Core;
using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legado.Shared
{
    [SingletonDependency]
    public class AppStates : INotifyPropertyChanged
    {
        private IndexState _MainState;

        public IndexState IndexState
        {
            get { return _MainState; }
            set
            {
                _MainState = value;
                RaisePropertyChanged(nameof(IndexState));
            }
        } 

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }



    public enum IndexState
    {
        Bookshelf, Discover, User
    }
}
