using System;

namespace Peppermint.DataBinding
{
    public interface INotifyCollectionChanged
    {
        event Action<object, NotifyCollectionChangedEventArgs> CollectionChanged;
    }
}
