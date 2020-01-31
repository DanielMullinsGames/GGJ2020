using System.Collections.Generic;

namespace Peppermint.DataBinding
{
    public interface IDynamicViewFactory : IViewFactory
    {
        // Gets the list of items to be bound.
        void GetDynamicItems(List<object> itemList);
    }
}