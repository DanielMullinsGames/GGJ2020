using System;
using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Base class for bindable MonoBehaviour
    /// </summary>
    public abstract class BindableMonoBehaviour : MonoBehaviour, INotifyPropertyChanged
    {
        private BindingEvent bindingEvent = new BindingEvent();

        public event Action<object, string> PropertyChanged
        {
            add
            {
                bindingEvent.Add(value);
            }

            remove
            {
                bindingEvent.Remove(value);
            }
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            bindingEvent.Invoke(this, propertyName);
        }

        protected void NotifyAllPropertiesChanged()
        {
            NotifyPropertyChanged(null);
        }

#if NET_4_6
        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
#else
        protected bool SetProperty<T>(ref T storage, T value, string propertyName)
#endif
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            // set new value
            storage = value;

            NotifyPropertyChanged(propertyName);
            return true;
        }

#if NET_4_6
        protected bool SetProperty<T>(T storage, T value, Action<T> updateAction, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
#else
        protected bool SetProperty<T>(T storage, T value, Action<T> updateAction, string propertyName)
#endif
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            // call action to set new value
            updateAction.Invoke(value);

            NotifyPropertyChanged(propertyName);
            return true;
        }
    }
}
