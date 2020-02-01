using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Bind the target data context to a nested source.
    ///
    /// DataContextBinder simplified the setup and usage for nested property bindings, which is
    /// useful for binding to complex type. It also handle nested source changes automatically.
    ///
    /// Note that the target data context must be different from its owner data context.
    /// </summary>
    [Binder]
    [AddComponentMenu("Peppermint/Data Binding/Binder/Component/Data Context Binder")]
    public class DataContextBinder : MonoBehaviour
    {
        public string path;
        public DataContext target;

        private Binding binding;
        private IDataContext dataContext;

        public object NestedSource
        {
            set
            {
                // set source
                target.Source = value;
            }
        }

        void Start()
        {
            if (target == null)
            {
                Debug.LogError("Target DataContext is null", this);
                return;
            }

            var owner = BindingUtility.FindDataContext(transform);
            if (ReferenceEquals(owner, target))
            {
                Debug.LogError("Invalid target");
                return;
            }

            CreateBinding();
        }

        void OnDestroy()
        {
            BindingUtility.RemoveBinding(binding, dataContext);
        }

        private void CreateBinding()
        {
            // create binding
            binding = new Binding(path, this, "NestedSource", Binding.BindingMode.OneWay, Binding.ConversionMode.None, null);
            binding.SetFlags(Binding.ControlFlags.ResetTargetValue);

            // add it
            BindingUtility.AddBinding(binding, transform, out dataContext);
        }
    }
}