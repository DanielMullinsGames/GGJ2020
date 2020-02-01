using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to create a custom binder.
    ///
    /// JoystickBinder is a touch joystick, it handles the touch input and sets the analog value to
    /// its source property. Two update modes are supported in this binder. The first update mode
    /// uses binding object's UpdateSource method, which will set the value to source property using
    /// fast delegate (or reflection). The second mode uses the bound action, and update the value
    /// through the action. Note that the binding mode is different, the first one uses one-way to
    /// source, and the second one uses one-way.
    ///
    /// It also shows how to set the binding flags. When unbind() is called the source/target value
    /// will remain the latest value, it will not be reset to default value. You can set the flag to
    /// explicit tell the binding object to reset the value to default when unbinding.
    ///
    /// The Binder attribute mark this class as Binder, so the graph tool can recognize this class as
    /// a binder.
    ///
    /// If the Binder you created does not need to update the source/target value frequently. It's
    /// recommended to use property to update value. Most peppermint built-in binders use this
    /// approach. If the binder need to update the value frequently, consider using the action to
    /// update value which is more efficient.
    /// </summary>
    [Binder]
    public class JoystickBinder : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public enum UpdateMode
        {
            SetProperty,
            CallAction,
        }

        public UpdateMode updateMode;
        public string valuePath;
        public string valueChangedPath;

        public float minMovement;
        public float maxMovement;
        public RectTransform container;

        private Binding binding;
        private IDataContext dataContext;
        private Vector2 analogValue;
        private Action<Vector2> analogValueChanged;

        private RectTransform rectTransform;
        private Vector2 sourcePosition;
        private Vector2 dragStart;

        // AnalogValue need a get accessor, which is called when you call the binding.UpdateSource()
        public Vector2 AnalogValue
        {
            get
            {
                return analogValue;
            }
        }

        // AnalogValueChanged need a set accessor, which is called when binding and unbinding
        public Action<Vector2> AnalogValueChanged
        {
            set
            {
                analogValueChanged = value;
            }
        }

        void Start()
        {
            // save source position
            rectTransform = transform as RectTransform;
            sourcePosition = rectTransform.localPosition;

            CreateBinding();
        }

        void OnDestroy()
        {
            BindingUtility.RemoveBinding(binding, dataContext);
        }

        private void CreateBinding()
        {
            if (updateMode == UpdateMode.SetProperty)
            {
                // create one way to source binding
                binding = new Binding(valuePath, this, "AnalogValue", Binding.BindingMode.OneWayToSource);
                binding.SetFlags(Binding.ControlFlags.ResetSourceValue);
            }
            else if (updateMode == UpdateMode.CallAction)
            {
                // create one way binding
                binding = new Binding(valueChangedPath, this, "AnalogValueChanged");
                binding.SetFlags(Binding.ControlFlags.ResetTargetValue);
            }
            else
            {
                Debug.LogError("Unsupported update mode");
                return;
            }

            BindingUtility.AddBinding(binding, transform, out dataContext);
        }

        private void OnValueChanged(Vector2 value)
        {
            if (analogValue == value)
            {
                return;
            }

            // update value
            analogValue = value;

            if (!binding.IsBound)
            {
                // skip update if unbound
                return;
            }

            if (updateMode == UpdateMode.SetProperty)
            {
                // call UpdateSource, which internally calls the set accessor of source property.
                // Binding class use PropertyAccessor to set the value to source property, for value type
                // it will do the boxing and unboxing, calling this method frequently may cause GC issues.
                binding.UpdateSource();
            }
            else if (updateMode == UpdateMode.CallAction)
            {
                if (analogValueChanged != null)
                {
                    // call action which is fast and will not cause GC issues.
                    analogValueChanged.Invoke(value);
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // get start position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, eventData.position, eventData.pressEventCamera, out dragStart);

            OnValueChanged(Vector2.zero);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // reset input
            OnValueChanged(Vector2.zero);

            // restore handle
            rectTransform.localPosition = sourcePosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 currentPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, eventData.position, eventData.pressEventCamera, out currentPosition);

            var delta = currentPosition - dragStart;
            float distance = delta.magnitude;

            // check movement range
            if (distance < minMovement)
            {
                // move handle
                rectTransform.localPosition = currentPosition - dragStart;

                // inside dead zone
                OnValueChanged(Vector2.zero);
            }
            else
            {
                // clamp movement
                Vector2 movement = new Vector2();
                movement.x = Mathf.Clamp(delta.x, -maxMovement, maxMovement);
                movement.y = Mathf.Clamp(delta.y, -maxMovement, maxMovement);

                // move handle
                rectTransform.localPosition = movement;

                // calculate analog input
                var newValue = new Vector2(movement.x / maxMovement, movement.y / maxMovement);
                OnValueChanged(newValue);
            }
        }
    }
}
