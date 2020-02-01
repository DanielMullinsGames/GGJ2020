using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to setup CustomBinder.
    ///
    /// In this demo, two CustomBinders are used. One for binding the localRotation of Transform, and
    /// the other is used for binding the fontSize of UI.Text.
    ///
    /// Create a binder for some rarely used property is impractical. CustomBinder is a configurable
    /// binder, it can bind any property of Unity.Object. See CustomBinder for more information.
    /// </summary>
    public class CustomBinderExample : BindableMonoBehaviour
    {
        public float fontSize = 30f;
        public float minFontSize = 30f;
        public float maxFontSize = 72f;

        public float rotationZ = 0f;
        public float minRotationZ = 0f;
        public float maxRotationZ = 360f;

        private Quaternion rotation;

        #region Bindable Properties

        public float FontSize
        {
            get { return fontSize; }
            set { SetProperty(ref fontSize, value, "FontSize"); }
        }

        public float MinFontSize
        {
            get { return minFontSize; }
        }

        public float MaxFontSize
        {
            get { return maxFontSize; }
        }

        public float RotationZ
        {
            get { return rotationZ; }
            set
            {
                if (SetProperty(ref rotationZ, value, "RotationZ"))
                {
                    // set rotation
                    var newRotation = Quaternion.Euler(0f, 0f, rotationZ);
                    Rotation = newRotation;
                }
            }
        }

        public float MinRotationZ
        {
            get { return minRotationZ; }
        }

        public float MaxRotationZ
        {
            get { return maxRotationZ; }
        }

        public Quaternion Rotation
        {
            get { return rotation; }
            set { SetProperty(ref rotation, value, "Rotation"); }
        }

        #endregion

        void Start()
        {
            BindingManager.Instance.AddSource(this, typeof(CustomBinderExample).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }
    }
}
