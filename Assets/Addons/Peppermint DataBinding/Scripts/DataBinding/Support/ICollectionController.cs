
namespace Peppermint.DataBinding
{
    /// <summary>
    /// View controller for CollectionBinding.
    /// </summary>
    public interface ICollectionController
    {
        // Initialize collection controller
        void Init(ICollectionBindingAccessor accessor, object args);

        // Update view
        void UpdateView();
    }
}