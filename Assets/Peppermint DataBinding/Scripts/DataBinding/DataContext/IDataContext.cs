using System.Collections.Generic;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// IDataContext interface
    /// </summary>
    public interface IDataContext
    {
        // binding source
        object Source { get; set; }

        bool IsBound { get; }

        List<IBinding> BindingList { get; }

        // add binding object
        void AddBinding(IBinding binding);

        void AddBinding(IEnumerable<IBinding> bindings);

        // remove binding object
        void RemoveBinding(IBinding binding);

        void RemoveBinding(IEnumerable<IBinding> bindings);
    }
}
