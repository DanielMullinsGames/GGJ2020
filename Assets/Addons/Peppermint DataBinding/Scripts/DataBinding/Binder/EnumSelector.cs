using System;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Activate targets by comparing the string value of the source property.
    ///
    /// EnumSelector is a custom selector for enum type. To setup this selector, you need to specify
    /// the enumType field first, which is the type name of the enum. If the type name is valid, the
    /// editor will use enum popup for the value field, otherwise it will use string field instead.
    ///
    /// The enumType field supports type name, partial type name, and namespace qualified type name,
    /// e.g. "EnumA", "FooClass+EnumA", "Namespace.FooClass+EnumA". If the type name conflict, you
    /// can try partial type name, and namespace qualified type name respectively.
    ///
    /// The editor will search all script assemblies and all “UnityEngine” assemblies.
    /// </summary>
    [Binder]
    [AddComponentMenu("Peppermint/Data Binding/Binder/Selector/Enum Selector")]
    public class EnumSelector : MonoBehaviour
    {
        [Serializable]
        public class Config
        {
            [EnumNamePopup("enumType")]
            public string value;
            public GameObject[] targets;
        }

        public string path;
        public string enumType;
        public Config[] configs;

        private ActiveStateModifier asm;

        private IBinding binding;
        private IDataContext dataContext;

        public string StringValue
        {
            set
            {
                // deactivate all
                asm.SetAll(false);

                // find matched config
                var config = Array.Find(configs, x => x.value == value);

                if (config != null)
                {
                    // activate all
                    asm.SetRange(config.targets, true);
                }

                asm.Apply();
            }
        }

        void Start()
        {
            // create active state modifier
            asm = new ActiveStateModifier();
            foreach (var item in configs)
            {
                asm.AddRange(item.targets);
            }
            asm.Init();

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
