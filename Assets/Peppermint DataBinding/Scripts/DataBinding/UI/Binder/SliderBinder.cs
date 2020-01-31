using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Bind Slider to a float property.
    ///
    /// SliderBinder is a two-way binder, it get the value from the source (float type), and update
    /// the source if Slider's value changes. You must specify value path, which is the source path
    /// of Slider's value. The min value path and max value path is optional. If specified, it
    /// control the min and max value of the Slider.
    /// </summary>
    [Binder]
    [RequireComponent(typeof(Slider))]
    [AddComponentMenu("Peppermint/Data Binding/Binder/UI/Slider Binder")]
    public class SliderBinder : MonoBehaviour
    {
        public string valuePath;
        public string minValuePath;
        public string maxValuePath;

        private Slider target;
        private Binding valueBinding;
        private List<IBinding> bindingList;
        private IDataContext dataContext;

        void Start()
        {
            target = GetComponent<Slider>();
            if (target == null)
            {
                Debug.LogError("Require Slider Component", gameObject);
                return;
            }

            CreateBinding();

            // add listener
            target.onValueChanged.AddListener(OnSliderValueChanged);
        }

        void OnDestroy()
        {
            // remove listener
            target.onValueChanged.RemoveListener(OnSliderValueChanged);

            BindingUtility.RemoveBinding(bindingList, dataContext);
        }

        private void CreateBinding()
        {
            bindingList = new List<IBinding>();

            if (!string.IsNullOrEmpty(minValuePath))
            {
                // minValue
                var binding = new Binding(minValuePath, target, "minValue");
                bindingList.Add(binding);
            }

            if (!string.IsNullOrEmpty(maxValuePath))
            {
                // maxValue
                var binding = new Binding(maxValuePath, target, "maxValue");
                bindingList.Add(binding);
            }

            // create value binding
            valueBinding = new Binding(valuePath, target, "value");
            bindingList.Add(valueBinding);

            BindingUtility.AddBinding(bindingList, transform, out dataContext);
        }

        private void OnSliderValueChanged(float value)
        {
            if (valueBinding.IsBound)
            {
                valueBinding.UpdateSource();    
            }
        }
    }
}
