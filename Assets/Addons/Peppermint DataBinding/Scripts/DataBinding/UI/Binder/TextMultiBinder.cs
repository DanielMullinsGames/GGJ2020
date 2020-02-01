using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Bind the text property to multi properties.
    ///
    /// If the format string is empty, string.Concat method is used to concatenate all string values,
    /// otherwise the specified format string is used. The source type can be any type.
    /// </summary>
    [Binder(SourcePathListMethod = "GetSourcePathList")]
    [RequireComponent(typeof(Text))]
    [AddComponentMenu("Peppermint/Data Binding/Binder/UI/Text MultiBinder")]
    public class TextMultiBinder : MonoBehaviour
    {
        [Serializable]
        public class Config
        {
            public string path;

            private object sourceValue;
            private TextMultiBinder owner;

            public object SourceValue
            {
                get { return sourceValue; }
                set
                {
                    sourceValue = value;

                    // notify owner
                    owner.UpdateText();
                }
            }

            public TextMultiBinder Owner
            {
                get { return owner; }
                set { owner = value; }
            }
        }

        public Config[] configs;
        public string formatString;

        private object[] sourceValues;
        private Text target;
        private List<IBinding> bindingList;
        private IDataContext dataContext;

        void Start()
        {
            target = GetComponent<Text>();
            if (target == null)
            {
                Debug.LogError("Require Text Component", gameObject);
                return;
            }

            // create values
            sourceValues = new object[configs.Length];

            CreateBinding();
        }

        void OnDestroy()
        {
            BindingUtility.RemoveBinding(bindingList, dataContext);
        }

        private void CreateBinding()
        {
            bindingList = new List<IBinding>();

            foreach (var config in configs)
            {
                // set owner
                config.Owner = this;

                // create binding
                var binding = new Binding(config.path, config, "SourceValue");

                // add it
                bindingList.Add(binding);
            }

            BindingUtility.AddBinding(bindingList, transform, out dataContext);
        }

        private void UpdateText()
        {
            // collect values
            for (int i = 0; i < configs.Length; i++)
            {
                sourceValues[i] = configs[i].SourceValue;
            }

            if (string.IsNullOrEmpty(formatString))
            {
                // concat values
                target.text = string.Concat(sourceValues);
            }
            else
            {
                // use format string
                target.text = string.Format(formatString, sourceValues);
            }
        }

#if UNITY_EDITOR

        public List<string> GetSourcePathList()
        {
            var list = new List<string>();

            foreach (var item in configs)
            {
                list.Add(item.path);
            }

            return list;
        }

#endif
    }
}