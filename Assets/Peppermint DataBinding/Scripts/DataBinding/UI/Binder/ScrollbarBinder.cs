using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Bind Scrollbar to a float property.
    ///
    /// ScrollbarBinder is a two-way binder, it get the value from the source (float type), and
    /// update the source if Scrollbar's value changes.
    /// </summary>
    [Binder]
    [RequireComponent(typeof(Scrollbar))]
    [AddComponentMenu("Peppermint/Data Binding/Binder/UI/Scrollbar Binder")]
    public class ScrollbarBinder : MonoBehaviour
    {
        public string path;

        private Scrollbar target;
        private Binding binding;
        private IDataContext dataContext;

        void Start()
        {
            target = GetComponent<Scrollbar>();
            if (target == null)
            {
                Debug.LogError("Require Scrollbar Component", gameObject);
                return;
            }

            CreateBinding();

            // add listener
            target.onValueChanged.AddListener(OnScrollbarValueChanged);
        }

        void OnDestroy()
        {
            // remove listener
            target.onValueChanged.RemoveListener(OnScrollbarValueChanged);

            BindingUtility.RemoveBinding(binding, dataContext);
        }

        private void CreateBinding()
        {
            // create binding
            binding = new Binding(path, target, "value");

            BindingUtility.AddBinding(binding, transform, out dataContext);
        }

        private void OnScrollbarValueChanged(float value)
        {
            if (binding.IsBound)
            {
                binding.UpdateSource();
            }
        }
    }
}
