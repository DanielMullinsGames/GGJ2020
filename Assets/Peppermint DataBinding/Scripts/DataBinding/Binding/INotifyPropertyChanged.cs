using System;

namespace Peppermint.DataBinding
{
    public interface INotifyPropertyChanged
    {
        /// <summary>
        /// Property changed event. First parameter is object, second parameter is the name of property.
        /// </summary>
        event Action<object, string> PropertyChanged;
    }
}