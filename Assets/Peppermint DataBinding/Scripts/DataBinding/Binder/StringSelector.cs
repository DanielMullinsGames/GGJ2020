using System;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Activate targets by comparing the string value of the source property.
    ///
    /// The configuration is similar to IntSelector and the testing function is string equality.
    /// StringSelector is a general selector, the source type can be any type, and the default
    /// converter will handle the convention for you.
    ///
    /// You can bind to Enum type, just set the value to the name of enum constant. You can also bind
    /// to Boolean type and set value to True or False.
    /// </summary>
    [Binder]
    [AddComponentMenu("Peppermint/Data Binding/Binder/Selector/String Selector")]
    public class StringSelector : MonoBehaviour
    {
        [Serializable]
        public class Config
        {
            public string value;
            public GameObject[] targets;
        }

        public string path;
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
