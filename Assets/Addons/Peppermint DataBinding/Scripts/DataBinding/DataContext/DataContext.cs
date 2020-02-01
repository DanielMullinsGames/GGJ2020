using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// DataContext component is the place where the binding source object being specified. It
    /// contains a list of binding objects which binds to the same source. Every binder need a data
    /// context by searching its parent, if the data context is not found, it continues searching on
    /// its ancestor. If a bunch of binders bind to the same object, just put a data context on these
    /// binders' common ancestor.
    ///
    /// There are two type of data context based on its usage, a data context and a registered data
    /// context. The basic data context is not known by BindingManager, its source is manually
    /// controlled. e.g. the data context inside view template of CollectionBinder, which is
    /// controlled by CollectionBinding. Registered data context will get source automatically when
    /// the required source is added.
    /// </summary>
    [AddComponentMenu("Peppermint/Data Binding/Data Context/Data Context")]
    public class DataContext : MonoBehaviour, IDataContext
    {
        private object source;
        private List<IBinding> bindingList = new List<IBinding>();

        public object Source
        {
            get { return source; }

            set
            {
                if (value != null)
                {
                    BindSource(value);
                }
                else
                {
                    UnbindSource();
                }
            }
        }

        public bool IsBound
        {
            get { return source != null; }
        }

        public List<IBinding> BindingList
        {
            get { return bindingList; }
        }

        public void AddBinding(IBinding binding)
        {
            if (binding == null)
            {
                return;
            }

            if (bindingList.Contains(binding))
            {
                Debug.LogWarningFormat("Binding {0} is already added.", binding);
                return;
            }

            bindingList.Add(binding);

            if (IsBound)
            {
                binding.Bind(source);
            }
        }

        public void AddBinding(IEnumerable<IBinding> bindings)
        {
            if (bindings == null)
            {
                return;
            }

            foreach (var item in bindings)
            {
                AddBinding(item);
            }
        }

        public void RemoveBinding(IBinding binding)
        {
            if (binding == null)
            {
                return;
            }

            if (!bindingList.Contains(binding))
            {
                Debug.LogWarningFormat("Unknown binding {0}.", binding);
                return;
            }

            bindingList.Remove(binding);

            if (IsBound)
            {
                binding.Unbind();    
            }
        }

        public void RemoveBinding(IEnumerable<IBinding> bindings)
        {
            if (bindings == null)
            {
                return;
            }

            foreach (var item in bindings)
            {
                RemoveBinding(item);
            }
        }

        private void BindSource(object source)
        {
            if (IsBound)
            {
                UnbindSource();
            }

            this.source = source;

            foreach (var item in bindingList)
            {
                item.Bind(source);
            }
        }

        private void UnbindSource()
        {
            if (!IsBound)
            {
                return;
            }

            this.source = null;

            foreach (var item in bindingList)
            {
                item.Unbind();
            }
        }

    }
}
