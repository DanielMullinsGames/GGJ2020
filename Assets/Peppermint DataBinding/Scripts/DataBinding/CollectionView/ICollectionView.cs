using System;

namespace Peppermint.DataBinding
{
    public interface ICollectionView
    {
        // Occurs when the view has changed.
        event Action ViewChanged;

        // Gets the number of records in the view.
        int Count { get; }

        // Gets or sets a method used to determine if an item is suitable for inclusion in the view.
        Predicate<object> Filter { get; set; }

        // Gets or sets a method used to sort the view.
        Func<object, object, int> Sort { get; set; }

        // Refresh view.
        void Refresh();
    }
}
