
namespace Peppermint.DataBinding
{
    /// <summary>
    /// Accessor for CollectionBinding.
    /// </summary>
    public interface ICollectionBindingAccessor
    {
        bool IsBound { get; }

        int ItemCount { get; }
    }
}