using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Bind ScrollRect to a Vector2 property.
    ///
    /// ScrollRectBinder is a two-way binder, it get the value from the source (Vector2 type), and
    /// update the source if ScrollRect's value changes.
    /// </summary>
    [Binder]
    [RequireComponent(typeof(ScrollRect))]
    [AddComponentMenu("Peppermint/Data Binding/Binder/UI/ScrollRect Binder")]
    public class ScrollRectBinder : MonoBehaviour
    {
        public string path;

        private ScrollRect target;
        private Binding binding;
        private IDataContext dataContext;

        void Start()
        {
            target = GetComponent<ScrollRect>();
            if (target == null)
            {
                Debug.LogError("Require ScrollRect Component", gameObject);
                return;
            }

            CreateBinding();

            // add listener
            target.onValueChanged.AddListener(OnScrollRectValueChanged);
        }

        void OnDestroy()
        {
            // remove listener
            target.onValueChanged.RemoveListener(OnScrollRectValueChanged);

            BindingUtility.RemoveBinding(binding, dataContext);
        }

        private void CreateBinding()
        {
            // create binding
            binding = new Binding(path, target, "normalizedPosition");

            BindingUtility.AddBinding(binding, transform, out dataContext);
        }

        private void OnScrollRectValueChanged(Vector2 value)
        {
            if (binding.IsBound)
            {
                binding.UpdateSource();    
            }
        }
    }
}
