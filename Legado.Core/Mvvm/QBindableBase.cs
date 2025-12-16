using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Legado.Core.Mvvm
{
    public abstract class QBindableBase : ObservableObject
    {

        public void NotifyPropertyChanged(string propertyName)
        { 
            base.OnPropertyChanged(propertyName);
        } 

        protected virtual bool Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (!object.Equals(storage, value))
            {
                storage = value;
                NotifyPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        protected virtual bool SetObjectPropertyValue<T>(object storageObj, string storagePropertyName, T value, [CallerMemberName] string propertyName = null)
        {
            Type srcType = storageObj.GetType();
            PropertyInfo pInfo = srcType.GetProperty(storagePropertyName);
            object oldVal = pInfo.GetValue(storageObj);
            if (!object.Equals(oldVal, value))
            {
                pInfo.SetValue(storageObj, value);
                NotifyPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        protected virtual bool SetObjectPropertyValue<T>(object storageObj, T value, [CallerMemberName] string propertyName = null)
        {
            return SetObjectPropertyValue(storageObj, propertyName, value, propertyName);
        }
    }

}