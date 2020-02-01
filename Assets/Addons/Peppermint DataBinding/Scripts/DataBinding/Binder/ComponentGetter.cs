using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Gets component from the view.
    ///
    /// ComponentGetter creates a one-way to source binding. Once bound the source gets the value
    /// from the target, and the source value is set to null if unbound. You can set the target by
    /// dragging a component to target field.
    /// </summary>
    [Binder]
    [AddComponentMenu("Peppermint/Data Binding/Binder/Getter/Component Getter")]
    public class ComponentGetter : MonoBehaviour
    {
        public string path;
        public Component target;

        private Binding binding;
        private IDataContext dataContext;

        public Component Target
        {
            get
            {
                return target;
            }
        }

        void Start()
        {
            if (target == null)
            {
                Debug.LogError("target is null", gameObject);
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
            binding = new Binding(path, this, "Target", Binding.BindingMode.OneWayToSource, Binding.ConversionMode.None, null);
            binding.SetFlags(Binding.ControlFlags.ResetSourceValue);

            BindingUtility.AddBinding(binding, transform, out dataContext);
        }
    }

}
