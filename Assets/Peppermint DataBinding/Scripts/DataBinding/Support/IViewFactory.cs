using UnityEngine;

namespace Peppermint.DataBinding
{
    public interface IViewFactory
    {
        // Creates a view GameObject for the specified item object.
        GameObject CreateItemView(object item);

        // Releases the view GameObject.
        void ReleaseItemView(GameObject view);

        // Update view
        void UpdateView();
    }
}
