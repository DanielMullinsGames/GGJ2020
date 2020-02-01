using System;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// CustomSetter can be used to set any property of Unity Object.
    ///
    /// The configuration is similar to StringSelector. If the value matches the given config, the
    /// style is applied to target object. You should specify the target first. The style contains a
    /// group of StyleSetter, and in each StyleSetter you can choose the target path from the popup
    /// menu. The value type is automatically selected based on the property type, you only need to
    /// specify the value. If the target is null, the target path will be shown as string field and
    /// the value type must be manually setup.
    /// </summary>
    [Binder]
    [AddComponentMenu("Peppermint/Data Binding/Binder/Setter/Custom Setter")]
    public class CustomSetter : MonoBehaviour
    {
        [Serializable]
        public class Style
        {
            public StyleSetter[] setters;
        }

        [Serializable]
        public class Config
        {
            public string value;
            public Style style;
        }

        public string path;
        public UnityEngine.Object target;
        public Style defaultStyle;
        public Config[] configs;

        private Type targetType;
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
                    // use default style
                    ApplyStyle(defaultStyle);
                }
                else
                {
                    // apply style
                    ApplyStyle(config.style);
                }
            }
        }

        void Start()
        {
            if (target == null)
            {
                Debug.LogError("target is null", gameObject);
                return;
            }

            // get target type
            targetType = target.GetType();

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

        private void ApplyStyle(Style style)
        {
            foreach (var item in style.setters)
            {
                var pa = TypeCache.Instance.GetPropertyAccessor(targetType, item.targetPath);

                // check property path
                if (pa == null)
                {
                    Debug.LogErrorFormat("Invalid propery {0}, targetType={1}", item.targetPath, targetType.Name);
                    continue;
                }

                // check value type
                if (item.value.valueType == UnionObject.ValueType.None)
                {
                    Debug.LogErrorFormat("Invalid value type {0}, targetType={1}", item.value.valueType, targetType.Name);
                    continue;
                }

                // get value from setter
                var targetValue = item.value.GetBoxedValue();

                // set value
                pa.SetValue(target, targetValue);
            }
        }
    }
}
