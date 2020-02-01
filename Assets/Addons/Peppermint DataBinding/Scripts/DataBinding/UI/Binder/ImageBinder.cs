using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Bind the sprite property of Image to a string property.
    ///
    /// ImageBinder must specify a converter, which convert a string to a sprite object. See
    /// SpriteNameConverter for more information.
    /// </summary>
    [Binder]
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("Peppermint/Data Binding/Binder/UI/Image Binder")]
    public class ImageBinder : MonoBehaviour
    {
        public string path;
        public string converterName;

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
            IValueConverter vc = ValueConverterProvider.Instance.GetNamedConverter(converterName);
            binding = new Binding(path, target, "sprite", Binding.BindingMode.OneWay, Binding.ConversionMode.Parameter, vc);

            BindingUtility.AddBinding(binding, transform, out dataContext);
        }
    }
}