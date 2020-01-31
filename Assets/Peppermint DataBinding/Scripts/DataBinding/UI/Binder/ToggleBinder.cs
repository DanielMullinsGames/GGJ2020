using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Bind Toggle to a boolean property.
    ///
    /// ToggleBinder is a two-way binder, it get the value from the source, and update the source if
    /// Toggle's value changes.
    /// </summary>
    [Binder]
    [RequireComponent(typeof(Toggle))]
    [AddComponentMenu("Peppermint/Data Binding/Binder/UI/Toggle Binder")]
    public class ToggleBinder : MonoBehaviour
    {
        public string path;

        private Toggle target;
        private Binding binding;
        private IDataContext dataContext;

        void Start()
        {
            target = GetComponent<Toggle>();
            if (target == null)
            {
                Debug.LogError("Require Toggle Component", gameObject);
                return;
            }

            CreateBinding();

            // add listener
            target.onValueChanged.AddListener(OnToggleValueChanged);
        }

        void OnDestroy()
        {
            // remove listener
            target.onValueChanged.RemoveListener(OnToggleValueChanged);

            BindingUtility.RemoveBinding(binding, dataContext);
        }

        private void CreateBinding()
        {
            // create binding
            binding = new Binding(path, target, "isOn");

            BindingUtility.AddBinding(binding, transform, out dataContext);
        }

        private void OnToggleValueChanged(bool value)
        {
            if (binding.IsBound)
            {
                binding.UpdateSource();    
            }
        }
    }
}
