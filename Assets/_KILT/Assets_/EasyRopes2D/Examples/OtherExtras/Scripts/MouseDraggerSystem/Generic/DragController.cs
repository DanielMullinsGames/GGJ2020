using UnityEngine;
using System.Collections;

namespace Kilt.EasyRopes2D.Examples
{
    public abstract class DragController : MonoBehaviour
    {
        #region Private Variables

        [SerializeField]
        MouseButtonTrigger m_triggerButton = MouseButtonTrigger.ButtonRight;

        bool _pressed = false;
        GameObject _objectPressed = null;
        Vector2 _deltaClick = Vector2.zero;

        #endregion

        #region Protected Properties

        protected bool Pressed
        {
            get
            {
                return _pressed;
            }
            set
            {
                if (_pressed == value)
                    return;
                _pressed = value;
            }
        }

        protected GameObject ObjectPressed
        {
            get
            {
                return _objectPressed;
            }
            set
            {
                if (_objectPressed == value)
                    return;
                _objectPressed = value;
            }
        }

        protected Vector2 DeltaClick
        {
            get
            {
                return _deltaClick;
            }
            set
            {
                if (_deltaClick == value)
                    return;
                _deltaClick = value;
            }
        }

        #endregion

        #region Public Properties

        public virtual MouseButtonTrigger TriggerButton
        {
            get
            {
                return m_triggerButton;
            }
            set
            {
                if (m_triggerButton == value)
                    return;
                m_triggerButton = value;
            }
        }

        #endregion

        #region Unity Functions

        protected virtual void Start()
        {
            ClickEffectController.OnGlobalPress += HandleOnGlobalPress;
        }

        protected virtual void OnDestroy()
        {
            PressedFalseEffect();
            ClickEffectController.OnGlobalPress -= HandleOnGlobalPress;
        }

        #endregion

        #region Events Registration

        protected void HandleOnGlobalPress(bool p_pressed)
        {
            if (TriggerButton != MouseButtonTrigger.DontTrigger &&
               Input.GetMouseButton((int)TriggerButton) &&
               CameraInputController.currentTouch != null &&
               CameraInputController.currentTouch.pressed != null &&
               enabled &&
               gameObject.activeSelf &&
               gameObject.activeInHierarchy)
            {
                Pressed = p_pressed;
                if (Pressed)
                {
                    ApplyPressedTrue();
                }
                else
                {
                    ApplyPressedFalse();
                }
            }
            else
            {
                ApplyPressedFalse();
            }
        }

        #endregion

        #region Helper Functions

        protected Vector2 GetWorldMousePosition(GameObject p_selectedObject)
        {
            Vector2 v_worldMousePosition = Vector2.zero;
            if (p_selectedObject != null)
            {
                Camera v_camera = null;
                v_camera = RopeInternalUtils.GetCameraThatDrawLayer(p_selectedObject.layer);
                if (v_camera != null)
                    v_worldMousePosition = v_camera.ScreenToWorldPoint(Input.mousePosition);
            }
            return v_worldMousePosition;
        }

        protected void ApplyPressedTrue()
        {
            ObjectPressed = CameraInputController.currentTouch.pressed;
            if (ObjectPressed != null)
            {
                Vector2 v_worldMousePosition = GetWorldMousePosition(ObjectPressed);
                DeltaClick = new Vector2(ObjectPressed.transform.position.x - v_worldMousePosition.x, ObjectPressed.transform.position.y - v_worldMousePosition.y);
                //_oldPosition = _objectToMove.transform.position;
            }
            PressedTrueEffect();
        }

        protected void ApplyPressedFalse()
        {
            PressedFalseEffect();
            ObjectPressed = null;
            DeltaClick = Vector2.zero;
        }

        protected virtual void PressedTrueEffect()
        {
        }

        protected virtual void PressedFalseEffect()
        {
        }

        #endregion
    }
}
