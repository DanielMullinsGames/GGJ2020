using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Bind InputField to a string property.
    ///
    /// InputFieldBinder is a two-way binder, it get the text value from the source (string type),
    /// and update the source if InputField's value changes.
    ///
    /// Text path is the source path of the text, and the update source trigger decides when to
    /// update the source. If you need to validate input characters, specify validate input path,
    /// otherwise leave it empty. The source type of validate input is a delegate, which must be
    /// compatible with the InputField.OnValidateInput delegate.
    /// </summary>
    [Binder]
    [RequireComponent(typeof(InputField))]
    [AddComponentMenu("Peppermint/Data Binding/Binder/UI/InputField Binder")]
    public class InputFieldBinder : MonoBehaviour
    {
        public enum UpdateSourceTrigger
        {
            ValueChanged,
            EditEnd,
        }

        public string textPath;
        public string validateInputPath;
        public UpdateSourceTrigger updateSourceTrigger;

        private InputField target;
        private Binding textBinding;
        private InputField.OnValidateInput validateInput;
        private List<IBinding> bindingList;
        private IDataContext dataContext;

        public string Text
        {
            get
            {
                return target.text;
            }

            set
            {
                if (value == null)
                {
                    target.text = string.Empty;
                }
                else
                {
                    target.text = value;
                }
            }
        }

        public Delegate ValidateInput
        {
            set
            {
                if (validateInput != null)
                {
                    target.onValidateInput -= validateInput;
                }

                if (value != null)
                {
                    if (value is InputField.OnValidateInput)
                    {
                        // cast to InputField.OnValidateInput
                        validateInput = (InputField.OnValidateInput)value;
                    }
                    else
                    {
                        // convert to InputField.OnValidateInput
                        validateInput = (InputField.OnValidateInput)Delegate.CreateDelegate(typeof(InputField.OnValidateInput), value.Target, value.Method, false);
                        if (validateInput == null)
                        {
                            Debug.LogErrorFormat("Convert to InputField.OnValidateInput failed. delegate={0}", value);
                        }
                    }
                }
                else
                {
                    validateInput = null;
                }

                if (validateInput != null)
                {
                    target.onValidateInput += validateInput;
                }
            }
        }

        void Start()
        {
            // add listener
            target = GetComponent<InputField>();
            if (target == null)
            {
                Debug.LogError("Require InputField Component", gameObject);
                return;
            }

            CreateBinding();

            // add listener
            if (updateSourceTrigger == UpdateSourceTrigger.EditEnd)
            {
                target.onEndEdit.AddListener(OnInputFieldEditEnd);
            }
            else
            {
#if UNITY_5_3_OR_NEWER
                target.onValueChanged.AddListener(OnInputFieldValueChanged);
#else
                target.onValueChange.AddListener(OnInputFieldValueChanged);
#endif
            }
        }

        void OnDestroy()
        {
            // remove listener
            if (updateSourceTrigger == UpdateSourceTrigger.EditEnd)
            {
                target.onEndEdit.RemoveListener(OnInputFieldEditEnd);
            }
            else
            {
#if UNITY_5_3_OR_NEWER
                target.onValueChanged.RemoveListener(OnInputFieldValueChanged);
#else
                target.onValueChange.RemoveListener(OnInputFieldValueChanged);
#endif
            }

            // remove validate
            if (validateInput != null)
            {
                target.onValidateInput -= validateInput;
            }

            BindingUtility.RemoveBinding(bindingList, dataContext);
        }

        private void CreateBinding()
        {
            bindingList = new List<IBinding>();

            // create binding
            textBinding = new Binding(textPath, this, "Text");
            bindingList.Add(textBinding);

            if (!string.IsNullOrEmpty(validateInputPath))
            {
                var binding = new Binding(validateInputPath, this, "ValidateInput");
                binding.SetFlags(Binding.ControlFlags.ResetTargetValue);

                bindingList.Add(binding);
            }

            BindingUtility.AddBinding(bindingList, transform, out dataContext);
        }

        private void OnInputFieldEditEnd(string value)
        {
            if (textBinding.IsBound)
            {
                textBinding.UpdateSource();
            }
        }

        private void OnInputFieldValueChanged(string value)
        {
            if (textBinding.IsBound)
            {
                textBinding.UpdateSource();
            }
        }
    }
}