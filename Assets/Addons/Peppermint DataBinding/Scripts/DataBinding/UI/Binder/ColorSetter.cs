using System;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// ColorSetter set the color of UI.Graphic object by comparing the string value of the source
    /// property.
    ///
    /// The configuration is similar to StringSelector. If the value matches the given config, the
    /// color from the matched config will be used, otherwise the default color is used.
    /// </summary>
    [Binder]
    [AddComponentMenu("Peppermint/Data Binding/Binder/Setter/Color Setter")]
    public class ColorSetter : MonoBehaviour
    {
        [Serializable]
        public class Config
        {
            public string value;
            public Color color;
        }

        public string path;
        public Color defaultColor;
        public Config[] configs;

        private UnityEngine.UI.Graphic target;
        private IBinding binding;
        private IDataContext dataContext;

        public string StringValue
        {
            set
            {
                // find matched config
                var config = Array.Find(configs, x => x.value == value);

                if (config == null)
                {
                    // use default color
                    target.color = defaultColor;
                }
                else
                {
                    // set color
                    target.color = config.color;
                }
            }
        }

        void Reset()
        {
            // get graphic target
            var target = GetComponent<UnityEngine.UI.Graphic>();
            
            if (target != null)
            {
                // get default color from target
                defaultColor = target.color;
            }
        }

        void Start()
        {
            // get graphic target
            target = GetComponent<UnityEngine.UI.Graphic>();
            if (target == null)
            {
                Debug.LogError("Require UI.Graphic Component", gameObject);
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
            binding = new Binding(path, this, "StringValue");

            BindingUtility.AddBinding(binding, transform, out dataContext);
        }
    }
}
