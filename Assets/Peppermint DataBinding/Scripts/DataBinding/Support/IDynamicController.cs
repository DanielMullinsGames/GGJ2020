using System.Collections.Generic;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// View controller for ListDynamicBinding.
    /// </summary>
    public interface IDynamicController
    {
        // Initialize dynamic controller
        void Init(IDynamicBindingAccessor accessor, object args);

        // Get visible items
        void GetDynamicItems(List<object> itemList);

        // Update the view
        void UpdateView();
    }
}
