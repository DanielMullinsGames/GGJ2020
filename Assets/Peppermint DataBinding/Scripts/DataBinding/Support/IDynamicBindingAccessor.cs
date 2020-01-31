
namespace Peppermint.DataBinding
{
    /// <summary>
    /// Accessor for ListDynamicBinding.
    /// </summary>
    public interface IDynamicBindingAccessor
    {
        bool IsBound { get; }

        int ItemCount { get; }

        object GetItem(int index);

        void UpdateDynamicList();
    }
}
