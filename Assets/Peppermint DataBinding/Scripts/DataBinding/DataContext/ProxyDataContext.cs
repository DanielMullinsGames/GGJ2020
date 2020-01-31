using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// ProxyDataContext is just a wrapper for the real DataContext.
    ///
    /// It's quite useful when you create a complex UI with multiple DataContext. For example, if you
    /// want to add a binder to a DataContext which is not the parent of current binder or in another
    /// node tree. You can add a ProxyDataContext component and specify the real data context, all
    /// child binders will be added to the target DataConext.
    /// </summary>
    [AddComponentMenu("Peppermint/Data Binding/Data Context/Proxy Data Context")]
    public class ProxyDataContext : MonoBehaviour, IDataContext
    {
        public DataContext realDataContext;

        public object Source
        {
            get { return realDataContext.Source; }
            set { realDataContext.Source = value; }
        }

        public bool IsBound
        {
            get { return realDataContext.IsBound; }
        }

        public List<IBinding> BindingList
        {
            get { return realDataContext.BindingList; }
        }

        public void AddBinding(IBinding binding)
        {
            realDataContext.AddBinding(binding);
        }

        public void AddBinding(IEnumerable<IBinding> bindings)
        {
            realDataContext.AddBinding(bindings);
        }

        public void RemoveBinding(IBinding binding)
        {
            realDataContext.RemoveBinding(binding);
        }

        public void RemoveBinding(IEnumerable<IBinding> bindings)
        {
            realDataContext.RemoveBinding(bindings);
        }
    }
}