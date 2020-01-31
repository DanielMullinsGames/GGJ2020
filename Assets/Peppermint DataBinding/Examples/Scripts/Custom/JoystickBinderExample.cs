using System;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// JoystickBinder Test ViewModel.
    ///
    /// Notice that InputHorizontal, InputVertical, HorizontalScale and VerticalScale, there are all
    /// related to the InputValue property. So if the InputValue changes, all related properties need
    /// notify property changed event.
    /// </summary>
    public class JoystickBinderExample : BindableMonoBehaviour
    {
        private Vector2 inputValue;
        private Action<Vector2> inputValueChanged;
        private Vector3 horizontalScale;
        private Vector3 verticalScale;

        #region Bindable Properties

        public Vector2 InputValue
        {
            get { return inputValue; }
            set
            {
                if (!SetProperty(ref inputValue, value, "InputValue"))
                {
                    return;
                }

                // notify the associated properties
                NotifyPropertyChanged("InputHorizontal");
                NotifyPropertyChanged("InputVertical");

                // calculate scale
                SetupScale();
            }
        }

        public Action<Vector2> InputValueChanged
        {
            get { return inputValueChanged; }
        }

        public float InputHorizontal
        {
            get { return inputValue.x; }
        }

        public float InputVertical
        {
            get { return inputValue.y; }
        }

        public Vector3 HorizontalScale
        {
            get { return horizontalScale; }
            set { SetProperty(ref horizontalScale, value, "HorizontalScale"); }
        }

        public Vector3 VerticalScale
        {
            get { return verticalScale; }
            set { SetProperty(ref verticalScale, value, "VerticalScale"); }
        }

        #endregion

        void Start()
        {
            // create action
            inputValueChanged = OnValueChanged;
            SetupScale();

            BindingManager.Instance.AddSource(this, typeof(JoystickBinderExample).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        private void OnValueChanged(Vector2 value)
        {
            // set new value
            InputValue = value;
        }

        private void SetupScale()
        {
            float scaleH = Map(inputValue.x, -1f, 1f, 0f, 1f);
            HorizontalScale = new Vector3(scaleH, 1f, 1f);

            float scaleV = Map(inputValue.y, -1f, 1f, 0f, 1f);
            VerticalScale = new Vector3(scaleV, 1f, 1f);
        }

        private float Map(float x, float inMin, float inMax, float outMin, float outMax)
        {
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }
    }
}
