using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Bind Dropdown to an int property.
    ///
    /// DropdownBinder is a two-way binder, it get the value from the source (int type), and update
    /// the source if Dropdown's value changes. The value path must specified, it's the index of
    /// currently selected option. String options path is optional, if specified, a list of string
    /// will be used as options.
    /// </summary>
    [Binder]
    [RequireComponent(typeof(Dropdown))]
    [AddComponentMenu("Peppermint/Data Binding/Binder/UI/Dropdown Binder")]
    public class DropdownBinder : MonoBehaviour
    {
        public string valuePath;
        public string stringOptionsPath;

        private Dropdown target;
        private List<IBinding> bindingList;
        private Binding valueBinding;
        private IDataContext dataContext;

        public IEnumerable<string> StringOptions
        {
            set
            {
#if UNITY_5_3_OR_NEWER
                target.ClearOptions();

                if (value != null)
                {
                    var options = new List<string>(value);
                    target.AddOptions(options);
                }

                target.RefreshShownValue();
#else
                if (value == null)
                {
                    // set empty list
                    target.options = new List<Dropdown.OptionData>();
                }
                else
                {
                    var options = new List<Dropdown.OptionData>();
                    foreach (var item in value)
                    {
                        options.Add(new Dropdown.OptionData(item));
                    }

                    // update target
                    target.options = options;
                }
#endif
            }
        }


        void Start()
        {
            target = GetComponent<Dropdown>();
            if (target == null)
            {
                Debug.LogError("Require Dropdown Component", gameObject);
                return;
            }

            CreateBinding();

            // add listener
            target.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        void OnDestroy()
        {
            // remove listener
            target.onValueChanged.RemoveListener(OnDropdownValueChanged);

            BindingUtility.RemoveBinding(bindingList, dataContext);
        }

        private void CreateBinding()
        {
            bindingList = new List<IBinding>();

            // create binding
            if (!string.IsNullOrEmpty(stringOptionsPath))
            {
                var optionBinding = new Binding(stringOptionsPath, this, "StringOptions");
                bindingList.Add(optionBinding);
            }

            valueBinding = new Binding(valuePath, target, "value");
            bindingList.Add(valueBinding);

            BindingUtility.AddBinding(bindingList, transform, out dataContext);
        }

        private void OnDropdownValueChanged(int value)
        {
            if (valueBinding.IsBound)
            {
                valueBinding.UpdateSource();
            }
        }
    }
}