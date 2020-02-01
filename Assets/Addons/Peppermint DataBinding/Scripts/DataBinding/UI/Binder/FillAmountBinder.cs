using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Bind the fillAmount property of Image to a float property.
    /// </summary>
    [Binder]
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("Peppermint/Data Binding/Binder/UI/FillAmount Binder")]
    public class FillAmountBinder : MonoBehaviour
    {
        public string path;

        private IBinding binding;
        private IDataContext dataContext;

        void Start()
        {
            CreateBinding();
        }

        void OnDestroy()
        {
            BindingUtility.RemoveBinding(binding, dataContext);
        }

        private void CreateBinding()
        {
            var target = GetComponent<Image>();
            if (target == null)
            {
                Debug.LogError("Require Image Component", gameObject);
                return;
            }

            // create binding for sprite
            binding = new Binding(path, target, "fillAmount");

            BindingUtility.AddBinding(binding, transform, out dataContext);
        }
    }
}