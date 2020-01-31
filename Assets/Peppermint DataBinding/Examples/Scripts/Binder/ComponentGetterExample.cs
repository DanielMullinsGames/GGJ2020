using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to use ComponentGetter.
    ///
    /// Generally, if you want reference view component, you need to declare a public field and set
    /// the reference in Unity editor. In MVVM pattern, the view model is unaware of the view, so to
    /// get access to the view is difficult. With the help of ComponentGetter you can get any
    /// component from the view.
    ///
    /// In this demo, two RectTransform references are set from the view. The set accessor of
    /// ArrowTransform and TargetTransform are called by the ComponnetGetter. The reference is
    /// assigned when binding, and is reset to null when unbinding.
    ///
    /// ComponentGetter will make your model/view model dependent on the view, so use it wisely.
    /// </summary>
    public class ComponentGetterExample : BindableMonoBehaviour
    {
        private RectTransform arrowTransform;
        private RectTransform targetTransform;

        #region Bindable Properties

        public RectTransform ArrowTransform
        {
            set
            {
                Debug.LogFormat("ArrowTransform's set accessor is called, value={0}", value);
                arrowTransform = value;
            }
        }

        public RectTransform TargetTransform
        {
            set
            {
                Debug.LogFormat("TargetTransform's set accessor is called, value={0}", value);
                targetTransform = value;
            }
        }

        #endregion

        void Start()
        {
            BindingManager.Instance.AddSource(this, typeof(ComponentGetterExample).Name);
        }

        void LateUpdate()
        {
            if (arrowTransform == null || targetTransform == null)
            {
                return;
            }

            // look target
            arrowTransform.LookAt(targetTransform, Vector3.back);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }
    }
}
