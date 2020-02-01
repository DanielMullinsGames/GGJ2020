using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Kilt.Extensions;

namespace Kilt.EasyRopes2D
{
    /// <summary>
    /// Based in NGUI UICamera
    /// 
    /// - OnHover (isOver) is sent when the mouse hovers over a collider or moves away.
    /// - OnPress (isDown) is sent when a mouse button gets pressed on the collider.
    /// - OnSelect (selected) is sent when a mouse button is released on the same object as it was pressed on.
    /// - OnClick () is sent with the same conditions as OnSelect, with the added check to see if the mouse has not moved much. UICamera.currentTouchID tells you which button was clicked.
    /// - OnDoubleClick () is sent when the click happens twice within a fourth of a second. UICamera.currentTouchID tells you which button was clicked.
    /// - OnDrag (delta) is sent when a mouse or touch gets pressed on a collider and starts dragging it.
    /// - OnDrop (gameObject) is sent when the mouse or touch get released on a different collider than the one that was being dragged.
    /// - OnInput (text) is sent when typing (after selecting a collider by clicking on it).
    /// - OnTooltip (show) is sent when the mouse hovers over a collider for some time without moving.
    /// - OnScroll (float delta) is sent out when the mouse scroll wheel is moved.
    /// - OnKey (KeyCode key) is sent when keyboard or controller input is used.
    /// </summary>

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class CameraInputController : MonoBehaviour
    {
        #region Events

        //On global press is called before event OnPress and can be registered by other controls to know when OnPress will start
        public static event System.Action<bool> OnGlobalPress;
        public static event System.Action<Vector2> OnGlobalDrag;
        public static event System.Action<GameObject> OnGlobalDrop;

        #endregion

        #region Helper Enums

        [System.Flags]
        public enum DrawTypeEnum { NGUIType = 1, MeshRendererType = 2, SpriteRendererType = 4 }

        [System.Flags]
        public enum ColliderTypeEnum { Raycast2D = 1, Raycast3D = 2 }

        #endregion

        #region Helper UICamera Classes/Enums

        /// <summary>
        /// Whether the touch event will be sending out the OnClick notification at the end.
        /// </summary>

        public enum ClickNotification
        {
            None,
            Always,
            BasedOnDelta
        }

        /// <summary>
        /// Ambiguous mouse, touch, or controller event.
        /// </summary>

        public class MouseOrTouch
        {
            public Vector2 pos;             // Current position of the mouse or touch event
            public Vector2 delta;           // Delta since last update
            public Vector2 totalDelta;      // Delta since the event started being tracked

            public Camera pressedCam;       // Camera that the OnPress(true) was fired with

            public GameObject current;      // The current game object under the touch or mouse
            public GameObject pressed;      // The last game object to receive OnPress
            public GameObject dragged;      // The last game object to receive OnDrag

            public float clickTime = 0f;    // The last time a click event was sent out

            public ClickNotification clickNotification = ClickNotification.Always;
            public bool touchBegan = true;
            public bool pressStarted = false;
            public bool dragStarted = false;
        }

        class Highlighted
        {
            public GameObject go;
            public int counter = 0;
        }

        #endregion

        #region UICamera Variables and Properties 

        /// <summary>
        /// If 'true', currently hovered object will be shown in the top left corner.
        /// </summary>

        public bool debug = false;

        /// <summary>
        /// Whether the mouse input is used.
        /// </summary>

        public bool useMouse = true;

        /// <summary>
        /// Whether the touch-based input is used.
        /// </summary>

        public bool useTouch = true;

        /// <summary>
        /// Whether multi-touch is allowed.
        /// </summary>

        public bool allowMultiTouch = true;

        /// <summary>
        /// Whether the keyboard events will be processed.
        /// </summary>

        public bool useKeyboard = true;

        /// <summary>
        /// Whether the joystick and controller events will be processed.
        /// </summary>

        public bool useController = true;

        /// <summary>
        /// If 'true', once a press event is started on some object, that object will be the only one that will be
        /// receiving future events until the press event is finally released, regardless of where that happens.
        /// If 'false', the press event won't be locked to the original object, and other objects will be receiving
        /// OnPress(true) and OnPress(false) events as the touch enters and leaves their area.
        /// </summary>

        public bool stickyPress = true;

        /// <summary>
        /// Which layers will receive events.
        /// </summary>

        public LayerMask eventReceiverMask = -1;

        /// <summary>
        /// Whether raycast events will be clipped just like widgets. This essentially means that clicking on a collider that
        /// happens to have been clipped will not produce a hit. Note that having this enabled will slightly reduce performance.
        /// </summary>

        public bool clipRaycasts = true;

        /// <summary>
        /// How long of a delay to expect before showing the tooltip.
        /// </summary>

        public float tooltipDelay = 1f;

        /// <summary>
        /// Whether the tooltip will disappear as soon as the mouse moves (false) or only if the mouse moves outside of the widget's area (true).
        /// </summary>

        public bool stickyTooltip = true;

        /// <summary>
        /// How much the mouse has to be moved after pressing a button before it starts to send out drag events.
        /// </summary>

        public float mouseDragThreshold = 4f;

        /// <summary>
        /// How far the mouse is allowed to move in pixels before it's no longer considered for click events, if the click notification is based on delta.
        /// </summary>

        public float mouseClickThreshold = 10f;

        /// <summary>
        /// How much the mouse has to be moved after pressing a button before it starts to send out drag events.
        /// </summary>

        public float touchDragThreshold = 40f;

        /// <summary>
        /// How far the touch is allowed to move in pixels before it's no longer considered for click events, if the click notification is based on delta.
        /// </summary>

        public float touchClickThreshold = 40f;

        /// <summary>
        /// Raycast range distance. By default it's as far as the camera can see.
        /// </summary>

        public float rangeDistance = -1f;

        /// <summary>
        /// Name of the axis used for scrolling.
        /// </summary>

        public string scrollAxisName = "Mouse ScrollWheel";

        /// <summary>
        /// Name of the axis used to send up and down key events.
        /// </summary>

        public string verticalAxisName = "Vertical";

        /// <summary>
        /// Name of the axis used to send left and right key events.
        /// </summary>

        public string horizontalAxisName = "Horizontal";

        /// <summary>
        /// Various keys used by the camera.
        /// </summary>

        public KeyCode submitKey0 = KeyCode.Return;
        public KeyCode submitKey1 = KeyCode.JoystickButton0;
        public KeyCode cancelKey0 = KeyCode.Escape;
        public KeyCode cancelKey1 = KeyCode.JoystickButton1;

        public delegate void OnCustomInput();

        /// <summary>
        /// Custom input processing logic, if desired. For example: WP7 touches.
        /// Use UICamera.current to get the current camera.
        /// </summary>

        static public OnCustomInput onCustomInput;

        /// <summary>
        /// Whether tooltips will be shown or not.
        /// </summary>

        static public bool showTooltips = true;

        /// <summary>
        /// Position of the last touch (or mouse) event.
        /// </summary>

        static public Vector2 lastTouchPosition = Vector2.zero;

        /// <summary>
        /// Last raycast hit prior to sending out the event. This is useful if you want detailed information
        /// about what was actually hit in your OnClick, OnHover, and other event functions.
        /// </summary>

        static public RaycastHit lastHit;

        /// <summary>
        /// UICamera that sent out the event.
        /// </summary>

        static public CameraInputController current = null;

        /// <summary>
        /// Last camera active prior to sending out the event. This will always be the camera that actually sent out the event.
        /// </summary>

        static public Camera currentCamera = null;

        /// <summary>
        /// ID of the touch or mouse operation prior to sending out the event. Mouse ID is '-1' for left, '-2' for right mouse button, '-3' for middle.
        /// </summary>

        static public int currentTouchID = -1;

        /// <summary>
        /// Current touch, set before any event function gets called.
        /// </summary>

        static public MouseOrTouch currentTouch = null;

        /// <summary>
        /// Whether an input field currently has focus.
        /// </summary>

        static public bool inputHasFocus = false;

        /// <summary>
        /// If set, this game object will receive all events regardless of whether they were handled or not.
        /// </summary>

        static public GameObject genericEventHandler;

        /// <summary>
        /// If events don't get handled, they will be forwarded to this game object.
        /// </summary>

        static public GameObject fallThrough;

        // List of currently highlighted items
        static List<Highlighted> mHighlighted = new List<Highlighted>();

        // Selected widget (for input)
        static GameObject mSel = null;

        // Mouse events
        static MouseOrTouch[] mMouse = new MouseOrTouch[] { new MouseOrTouch(), new MouseOrTouch(), new MouseOrTouch() };

        // The last object to receive OnHover
        static GameObject mHover;

        // Joystick/controller/keyboard event
        static MouseOrTouch mController = new MouseOrTouch();

        // Used to ensure that joystick-based controls don't trigger that often
        static float mNextEvent = 0f;

        // List of currently active touches
        static Dictionary<int, MouseOrTouch> mTouches = new Dictionary<int, MouseOrTouch>();

        // Tooltip widget (mouse only)
        GameObject mTooltip = null;

        // Mouse input is turned off on iOS
        Camera mCam = null;
        LayerMask mLayerMask;
        float mTooltipTime = 0f;
        bool mIsEditor = false;


        /// <summary>
        /// Caching is always preferable for performance.
        /// </summary>

        public Camera cachedCamera { get { if (mCam == null) mCam = GetComponent<Camera>(); return mCam; } }

        /// <summary>
        /// Set to 'true' just before OnDrag-related events are sent (this includes OnPress events that resulted from dragging).
        /// </summary>

        static public bool isDragging = false;

        /// <summary>
        /// The object hit by the last Raycast that was the result of a mouse or touch event.
        /// </summary>

        static public GameObject hoveredObject;

        /// <summary>
        /// Option to manually set the selected game object.
        /// </summary>

        static public GameObject selectedObject
        {
            get
            {
                return mSel;
            }
            set
            {
                if (mSel != value)
                {
                    if (mSel != null)
                    {
                        CameraInputController uicam = FindInputControllerForLayer(mSel.layer);

                        if (uicam != null)
                        {
                            current = uicam;
                            currentCamera = uicam.cachedCamera;
                            Notify(mSel, "OnSelect", false, true);
                            if (uicam.useController || uicam.useKeyboard) Highlight(mSel, false);
                            current = null;
                        }
                    }

                    mSel = value;

                    if (mSel != null)
                    {
                        CameraInputController uicam = FindInputControllerForLayer(mSel.layer);

                        if (uicam != null)
                        {
                            current = uicam;
                            currentCamera = uicam.cachedCamera;
                            if (uicam.useController || uicam.useKeyboard) Highlight(mSel, true);
                            Notify(mSel, "OnSelect", true, true);
                            current = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Number of active touches from all sources.
        /// </summary>

        static public int touchCount
        {
            get
            {
                int count = 0;

                foreach (KeyValuePair<int, MouseOrTouch> touch in mTouches)
                    if (touch.Value.pressed != null)
                        ++count;

                for (int i = 0; i < mMouse.Length; ++i)
                    if (mMouse[i].pressed != null)
                        ++count;

                if (mController.pressed != null)
                    ++count;

                return count;
            }
        }

        /// <summary>
        /// Number of active drag events from all sources.
        /// </summary>

        static public int dragCount
        {
            get
            {
                int count = 0;

                foreach (KeyValuePair<int, MouseOrTouch> touch in mTouches)
                    if (touch.Value.dragged != null)
                        ++count;

                for (int i = 0; i < mMouse.Length; ++i)
                    if (mMouse[i].dragged != null)
                        ++count;

                if (mController.dragged != null)
                    ++count;

                return count;
            }
        }

        /// <summary>
        /// Convenience function that returns the main HUD camera.
        /// </summary>

        static public Camera mainCamera
        {
            get
            {
                CameraInputController mouse = eventHandler;
                return (mouse != null) ? mouse.cachedCamera : null;
            }
        }

        /// <summary>
        /// Event handler for all types of events.
        /// </summary>

        static public CameraInputController eventHandler
        {
            get
            {
                for (int i = 0; i < AllCameraInputControllers.Count; ++i)
                {
                    // Invalid or inactive entry -- keep going
                    CameraInputController cam = AllCameraInputControllers[i];
                    if (cam == null || !cam.enabled || !GetActive(cam.gameObject)) continue;
                    return cam;
                }
                return null;
            }
        }

        #endregion

        #region New Properties/Variables

        [SerializeField, MaskEnumAttribute]
        ColliderTypeEnum m_collisionType = ColliderTypeEnum.Raycast2D | ColliderTypeEnum.Raycast3D;

        public ColliderTypeEnum CollisionType
        {
            get
            {
                return m_collisionType;
            }
            set
            {
                if (m_collisionType == value)
                    return;
                m_collisionType = value;
            }
        }

        //Used To extend collision system to 2D
        static public RaycastHit2D lastHit2D;

        //Used To extend collision system to UGUI
        static public RaycastResult lastHitUGUI;

        #endregion

        #region Private Variables from CameraInputController Getters/Setters

        static List<CameraInputController> s_allCameraInputControllers = new List<CameraInputController>();
        protected static List<CameraInputController> AllCameraInputControllers
        {
            get
            {
                if (s_allCameraInputControllers == null)
                    s_allCameraInputControllers = new List<CameraInputController>();
                return s_allCameraInputControllers;
            }
            set
            {
                if (s_allCameraInputControllers == value)
                    return;
                s_allCameraInputControllers = value;
            }
        }

        // Selected widget (for input)
        //static GameObject mSel = null;
#if NGUI_DLL
	static System.Reflection.FieldInfo _selInfo = null;
#endif
        protected static GameObject Sel
        {
            get
            {
                return selectedObject;
            }
            set
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_selInfo == null)
				_selInfo = v_type.GetField("mSel", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Static);
			System.Reflection.FieldInfo v_fieldInfo = _selInfo;
			object v_currentValue = v_fieldInfo.GetValue(null);
			
			if(AOTHelper.Equals(v_currentValue, value))
				return;
			v_fieldInfo.SetValue(null, value);
			
#else
                if (mSel == value)
                    return;
                mSel = value;

#endif
            }
        }



        // Mouse events
#if NGUI_DLL
	static System.Reflection.FieldInfo _mouseInfo = null;
#endif
        protected static MouseOrTouch[] Mouse
        {
            get
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_mouseInfo == null)
				_mouseInfo = v_type.GetField("mMouse", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Static);
			System.Reflection.FieldInfo v_fieldInfo = _mouseInfo;
			object v_currentValue = v_fieldInfo.GetValue(null);
			return v_currentValue as MouseOrTouch[];
#else
                return mMouse;
#endif
            }
            set
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_mouseInfo == null)
				_mouseInfo = v_type.GetField("mMouse", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Static);
			System.Reflection.FieldInfo v_fieldInfo = _mouseInfo;
			object v_currentValue = v_fieldInfo.GetValue(null);
			
			if(AOTHelper.Equals(v_currentValue, value))
				return;
			v_fieldInfo.SetValue(null, value);
			
#else
                if (mMouse == value)
                    return;
                mMouse = value;

#endif
            }
        }

        // The last object to receive OnHover
#if NGUI_DLL
	static System.Reflection.FieldInfo _hoverInfo = null;
#endif
        protected static GameObject Hover
        {
            get
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_hoverInfo == null)
				_hoverInfo = v_type.GetField("mHover", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Static);
			System.Reflection.FieldInfo v_fieldInfo = _hoverInfo;
			object v_currentValue = v_fieldInfo.GetValue(null);
			return v_currentValue as GameObject;
#else
                return mHover;
#endif
            }
            set
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_hoverInfo == null)
				_hoverInfo = v_type.GetField("mHover", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Static);
			System.Reflection.FieldInfo v_fieldInfo = _hoverInfo;
			object v_currentValue = v_fieldInfo.GetValue(null);
			
			if(AOTHelper.Equals(v_currentValue, value))
				return;
			v_fieldInfo.SetValue(null, value);
			
#else
                if (mHover == value)
                    return;
                mHover = value;

#endif
            }
        }

        // Joystick/controller/keyboard event
#if NGUI_DLL
	static System.Reflection.FieldInfo _controllerInfo = null;
#endif
        protected static MouseOrTouch Controller
        {
            get
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_controllerInfo == null)
				_controllerInfo = v_type.GetField("mController", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Static);
			System.Reflection.FieldInfo v_fieldInfo = _controllerInfo;
			object v_currentValue = v_fieldInfo.GetValue(null);
			return v_currentValue as MouseOrTouch;
#else
                return mController;
#endif
            }
            set
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_controllerInfo == null)
				_controllerInfo = v_type.GetField("mController", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Static);
			System.Reflection.FieldInfo v_fieldInfo = _controllerInfo;
			object v_currentValue = v_fieldInfo.GetValue(null);
			
			if(AOTHelper.Equals(v_currentValue, value))
				return;
			v_fieldInfo.SetValue(null, value);
			
#else
                if (mController == value)
                    return;
                mController = value;

#endif
            }
        }

        // Used to ensure that joystick-based controls don't trigger that often
#if NGUI_DLL
	static System.Reflection.FieldInfo _nextEventInfo = null;
#endif
        protected static float NextEvent
        {
            get
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_nextEventInfo == null)
				_nextEventInfo = v_type.GetField("mNextEvent", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Static);
			System.Reflection.FieldInfo v_fieldInfo = _nextEventInfo;
			object v_currentValue = v_fieldInfo.GetValue(null);
			return (float)v_currentValue;
#else
                return mNextEvent;
#endif
            }
            set
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_nextEventInfo == null)
				_nextEventInfo = v_type.GetField("mNextEvent", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Static);
			System.Reflection.FieldInfo v_fieldInfo = _nextEventInfo;
			object v_currentValue = v_fieldInfo.GetValue(null);
			
			if(AOTHelper.Equals(v_currentValue, value))
				return;
			v_fieldInfo.SetValue(null, value);
			
#else
                if (mNextEvent == value)
                    return;
                mNextEvent = value;

#endif
            }
        }

        // List of currently active touches
#if NGUI_DLL
	static System.Reflection.FieldInfo _touchesInfo = null;
#endif
        protected static Dictionary<int, MouseOrTouch> Touches
        {
            get
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_touchesInfo == null)
				_touchesInfo = v_type.GetField("mTouches", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Static);
			System.Reflection.FieldInfo v_fieldInfo = _touchesInfo;
			object v_currentValue = v_fieldInfo.GetValue(null);
			return v_currentValue as Dictionary<int, MouseOrTouch>;
#else
                return mTouches;
#endif
            }
            set
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_touchesInfo == null)
				_touchesInfo = v_type.GetField("mTouches", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Static);
			System.Reflection.FieldInfo v_fieldInfo = _touchesInfo;
			object v_currentValue = v_fieldInfo.GetValue(null);
			
			if(AOTHelper.Equals(v_currentValue, value))
				return;
			v_fieldInfo.SetValue(null, value);
			
#else
                if (mTouches == value)
                    return;
                mTouches = value;

#endif
            }
        }

        // Tooltip widget (mouse only)
#if NGUI_DLL
	System.Reflection.FieldInfo _tooltipInfo = null;
#endif
        protected GameObject Tooltip
        {
            get
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_tooltipInfo == null)
				_tooltipInfo = v_type.GetField("mTooltip", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Instance);
			System.Reflection.FieldInfo v_fieldInfo = _tooltipInfo;
			object v_currentValue = v_fieldInfo.GetValue(this);
			return v_currentValue as GameObject;
#else
                return mTooltip;
#endif
            }
            set
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_tooltipInfo == null)
				_tooltipInfo = v_type.GetField("mTooltip", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Instance);
			System.Reflection.FieldInfo v_fieldInfo = _tooltipInfo;			
			object v_currentValue = v_fieldInfo.GetValue(this);
			
			if(AOTHelper.Equals(v_currentValue, value))
				return;
			v_fieldInfo.SetValue(this, value);
			
#else
                if (mTooltip == value)
                    return;
                mTooltip = value;

#endif
            }
        }

#if NGUI_DLL
	System.Reflection.FieldInfo _camInfo = null;
#endif
        protected Camera Cam
        {
            get
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_camInfo == null)
				_camInfo = v_type.GetField("mCam", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Instance);
			System.Reflection.FieldInfo v_fieldInfo = _camInfo;
			object v_currentValue = v_fieldInfo.GetValue(this);
			return v_currentValue as Camera;
#else
                return mCam;
#endif
            }
            set
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_camInfo == null)
				_camInfo = v_type.GetField("mCam", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Instance);
			System.Reflection.FieldInfo v_fieldInfo = _camInfo;			
			object v_currentValue = v_fieldInfo.GetValue(this);
			
			if(AOTHelper.Equals(v_currentValue, value))
				return;
			v_fieldInfo.SetValue(this, value);
			
#else
                if (mCam == value)
                    return;
                mCam = value;

#endif
            }
        }

#if NGUI_DLL
	System.Reflection.FieldInfo _layerMaskInfo = null;
#endif
        protected LayerMask LayMask
        {
            get
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_layerMaskInfo == null)
				_layerMaskInfo = v_type.GetField("mLayerMask", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Instance);
			System.Reflection.FieldInfo v_fieldInfo = _layerMaskInfo;
			object v_currentValue = v_fieldInfo.GetValue(this);
			return (LayerMask)v_currentValue;
#else
                return mLayerMask;
#endif
            }
            set
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_layerMaskInfo == null)
				_layerMaskInfo = v_type.GetField("mLayerMask", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Instance);
			System.Reflection.FieldInfo v_fieldInfo = _layerMaskInfo;			
			object v_currentValue = v_fieldInfo.GetValue(this);
			
			if(AOTHelper.Equals(v_currentValue, value))
				return;
			v_fieldInfo.SetValue(this, value);
			
#else
                if (mLayerMask == value)
                    return;
                mLayerMask = value;

#endif
            }
        }

#if NGUI_DLL
	System.Reflection.FieldInfo _tooltipTimeInfo = null;
#endif
        protected float TooltipTime
        {
            get
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_tooltipTimeInfo == null)
				_tooltipTimeInfo = v_type.GetField("mTooltipTime", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Instance);
			System.Reflection.FieldInfo v_fieldInfo = _tooltipTimeInfo;
			object v_currentValue = v_fieldInfo.GetValue(this);
			return (float)v_currentValue;
#else
                return mTooltipTime;
#endif
            }
            set
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_tooltipTimeInfo == null)
				_tooltipTimeInfo = v_type.GetField("mTooltipTime", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Instance);
			System.Reflection.FieldInfo v_fieldInfo = _tooltipTimeInfo;			
			object v_currentValue = v_fieldInfo.GetValue(this);
			
			if(AOTHelper.Equals(v_currentValue, value))
				return;
			v_fieldInfo.SetValue(this, value);
			
#else
                if (mTooltipTime == value)
                    return;
                mTooltipTime = value;

#endif
            }
        }

#if NGUI_DLL
	System.Reflection.FieldInfo _isEditorInfo = null;
#endif
        protected bool IsEditor
        {
            get
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_isEditorInfo == null)
				_isEditorInfo = v_type.GetField("mIsEditor", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Instance);
			System.Reflection.FieldInfo v_fieldInfo = _isEditorInfo;
			object v_currentValue = v_fieldInfo.GetValue(this);
			return (bool)v_currentValue;
#else
                return mIsEditor;
#endif
            }
            set
            {
#if NGUI_DLL
			System.Type v_type = typeof(UICamera);
			if(_isEditorInfo == null)
				_isEditorInfo = v_type.GetField("mIsEditor", System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Instance);
			System.Reflection.FieldInfo v_fieldInfo = _isEditorInfo;			
			object v_currentValue = v_fieldInfo.GetValue(this);
			
			if(AOTHelper.Equals(v_currentValue, value))
				return;
			v_fieldInfo.SetValue(this, value);
			
#else
                if (mIsEditor == value)
                    return;
                mIsEditor = value;

#endif
            }
        }

        /// <summary>
        /// Helper function that determines if this script should be handling the events.
        /// </summary>
        protected bool handlesEvents { get { return eventHandler == this; } }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Static comparison function used for sorting.
        /// </summary>

        static int CompareFunc(CameraInputController a, CameraInputController b)
        {
            if (a.cachedCamera.depth < b.cachedCamera.depth) return 1;
            if (a.cachedCamera.depth > b.cachedCamera.depth) return -1;
            return 0;
        }

        protected void UpdateHoveredObject()
        {
            UpdateHoveredObject(Input.mousePosition);
        }

        protected virtual void UpdateHoveredObject(Vector2 p_position)
        {
            bool v_sucess = false;
            if (EventSystem.current != null)
                hoveredObject = (v_sucess = Raycast(p_position, out lastHitUGUI)) ? lastHitUGUI.gameObject : fallThrough;
            //Take care, new UIEvent System will be called, in all layers, before we can call camera methods
            if (!v_sucess)
            {
#if NGUI_DLL
			foreach(UICamera v_uiCamera in UICamList)
#else
                foreach (CameraInputController v_uiCamera in AllCameraInputControllers)
#endif
                {
                    if (v_uiCamera != null)
                    {
                        if (!v_sucess)
                            hoveredObject = (v_sucess = RaycastToCamera(v_uiCamera, p_position, out lastHit2D)) ? lastHit2D.collider.gameObject : fallThrough;
                        if (!v_sucess)
                            hoveredObject = (v_sucess = RaycastToCamera(v_uiCamera, p_position, out lastHit)) ? lastHit.collider.gameObject : fallThrough;
                        if (v_sucess)
                            break;
                    }
                }
            }

            if (hoveredObject == null) hoveredObject = genericEventHandler;
        }

        #region RayCast

        /// <summary>
        /// Returns the object under the specified position.
        /// </summary>
        static RaycastHit2D mEmpty2D = new RaycastHit2D();
        static public bool RaycastToCamera(CameraInputController p_camera, Vector3 inPos, out RaycastHit2D hit)
        {
            CameraInputController cam = p_camera;
            bool v_canUseThisRayCast = cam != null && cam.CollisionType.ContainsFlag(ColliderTypeEnum.Raycast2D);
            // Skip inactive scripts
            if (cam != null && cam.enabled && GetActive(cam.gameObject) && v_canUseThisRayCast)
            {
                // Convert to view space
                currentCamera = cam.cachedCamera;
                Vector3 pos = currentCamera.ScreenToViewportPoint(inPos);
                if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y))
                {
                    // If it's outside the camera's viewport, do nothing
                    if (pos.x >= 0f && pos.x <= 1f && pos.y >= 0f && pos.y <= 1f)
                    {
                        // Cast a ray into the screen
                        Ray ray = currentCamera.ScreenPointToRay(inPos);

                        // Raycast into the screen
                        int mask = currentCamera.cullingMask & (int)cam.eventReceiverMask;
                        float dist = (cam.rangeDistance > 0f) ? cam.rangeDistance : currentCamera.farClipPlane - currentCamera.nearClipPlane;

                        // If raycasts should be clipped by panels, we need to find a panel for each hit
                        if (cam.clipRaycasts)
                        {
                            RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, dist, mask);
                            if (hits.Length > 1)
                            {
                                //Sort By Depth
                                System.Array.Sort(hits,
                                                  delegate (RaycastHit2D r1, RaycastHit2D r2)
                                                  {
                                                      int v_depth1 = 0;
                                                      int v_depth2 = 0;
                                                      DrawTypeEnum v_sucess1 = GetDrawOrderHighestDepth(r1.transform, out v_depth1);
                                                      DrawTypeEnum v_sucess2 = GetDrawOrderHighestDepth(r2.transform, out v_depth2);
                                                      if (v_sucess1 != (DrawTypeEnum)0 && v_sucess2 != (DrawTypeEnum)0)
                                                      {
                                                          if (v_depth1 == v_depth2)
                                                          {
                                                              if (v_sucess1.ContainsFlag(DrawTypeEnum.MeshRendererType) && v_sucess2.ContainsFlag(DrawTypeEnum.MeshRendererType))
                                                                  return r1.fraction.CompareTo(r2.fraction); //Compare two Meshs with same depth by fraction
                                                          else
                                                                  return -v_depth1.CompareTo(v_depth2);//We Must Reverse Values to make Highest Depths Come First
                                                      }
                                                          else
                                                              return -v_depth1.CompareTo(v_depth2);//We Must Reverse Values to make Highest Depths Come First
                                                  }

                                                  //Use Default Method if UIWidgets or Renderer not found
                                                  return r1.fraction.CompareTo(r2.fraction);
                                                  }
                                );

                                if (hits.Length > 0)
                                    hit = hits[0];
                            }
                            else if (hits.Length == 1)
                            {
                                hit = hits[0];
                                return true;
                            }
                        }
                        else
                        {
                            hit = Physics2D.Raycast(ray.origin, ray.direction, dist, mask);
                            if (hit.collider != null)
                                return true;
                        }
                    }
                }
            }
            hit = mEmpty2D;
            return false;
        }

        static RaycastHit mEmpty3D = new RaycastHit();
        static public bool RaycastToCamera(CameraInputController p_camera, Vector3 inPos, out RaycastHit hit)
        {
            CameraInputController cam = p_camera;
            bool v_canUseThisRayCast = cam != null && cam.CollisionType.ContainsFlag(ColliderTypeEnum.Raycast3D);
            // Skip inactive scripts
            if (cam != null && cam.enabled && GetActive(cam.gameObject) && v_canUseThisRayCast)
            {
                // Convert to view space
                currentCamera = cam.cachedCamera;
                Vector3 pos = currentCamera.ScreenToViewportPoint(inPos);
                if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y))
                {
                    // If it's outside the camera's viewport, do nothing
                    if (pos.x >= 0f && pos.x <= 1f && pos.y >= 0f && pos.y <= 1f)
                    {
                        // Cast a ray into the screen
                        Ray ray = currentCamera.ScreenPointToRay(inPos);

                        // Raycast into the screen
                        int mask = currentCamera.cullingMask & (int)cam.eventReceiverMask;
                        float dist = (cam.rangeDistance > 0f) ? cam.rangeDistance : currentCamera.farClipPlane - currentCamera.nearClipPlane;

                        // If raycasts should be clipped by panels, we need to find a panel for each hit
                        if (cam.clipRaycasts)
                        {
                            RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, dist, mask);
                            if (hits.Length > 1)
                            {
                                //Sort By Depth
                                System.Array.Sort(hits,
                                                  delegate (RaycastHit r1, RaycastHit r2)
                                                  {

                                                      int v_depth1 = 0;
                                                      int v_depth2 = 0;
                                                      DrawTypeEnum v_sucess1 = GetDrawOrderHighestDepth(r1.transform, out v_depth1);
                                                      DrawTypeEnum v_sucess2 = GetDrawOrderHighestDepth(r2.transform, out v_depth2);
                                                      if (v_sucess1 != (DrawTypeEnum)0 && v_sucess2 != (DrawTypeEnum)0)
                                                      {
                                                          if (v_depth1 == v_depth2)
                                                          {
                                                              if (v_sucess1.ContainsFlag(DrawTypeEnum.MeshRendererType) && v_sucess2.ContainsFlag(DrawTypeEnum.MeshRendererType))
                                                                  return r1.distance.CompareTo(r2.distance); //Compare two Meshs with same depth by fraction
                                                          else
                                                                  return -v_depth1.CompareTo(v_depth2);//We Must Reverse Values to make Highest Depths Come First
                                                      }
                                                          else
                                                              return -v_depth1.CompareTo(v_depth2);//We Must Reverse Values to make Highest Depths Come First
                                                  }

                                                  //Use Default Method if UIWidgets or Renderer not found
                                                  return r1.distance.CompareTo(r2.distance);
                                                  }
                                );

                                if (hits.Length > 0)
                                    hit = hits[0];
                            }
                            else if (hits.Length == 1)
                            {
                                hit = hits[0];
                                return true;
                            }
                        }
                        else
                        {
                            Physics.Raycast(ray.origin, ray.direction, out hit, dist, mask);
                            if (hit.collider != null)
                                return true;
                        }
                    }
                }
            }
            hit = mEmpty3D;
            return false;
        }

        static RaycastResult mEmptyUGUI = new RaycastResult();
        static public bool RaycastToCamera(CameraInputController p_camera, Vector3 inPos, out RaycastResult hit)
        {
            if (p_camera != null && p_camera.enabled && GetActive(p_camera.gameObject))
            {
                hit = mEmptyUGUI;
                PointerEventData v_pointer = new PointerEventData(EventSystem.current);
                v_pointer.position = inPos;
                List<RaycastResult> v_raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(v_pointer, v_raycastResults);
                FilterResultByLayer(p_camera.eventReceiverMask, ref v_raycastResults);
                if (v_raycastResults.Count > 0)
                {
                    bool v_foundEventHandler = false;
                    for (int i = 0; i < v_raycastResults.Count; i++)
                    {
                        RaycastResult v_raycastResult = v_raycastResults[i];
                        v_foundEventHandler = UpdateRayCastWithType<IEventSystemHandler>(ref v_raycastResult);
                        if (v_foundEventHandler)
                        {
                            hit = v_raycastResult;
                            break;
                        }
                    }
                    if (!v_foundEventHandler)
                        hit = v_raycastResults.GetFirst();
                    return true;
                }
            }
            hit = mEmptyUGUI;
            return false;
        }

        static public bool Raycast(Vector3 inPos, out RaycastHit2D hit)
        {
            for (int i = 0; i < AllCameraInputControllers.Count; ++i)
            {
                CameraInputController cam = AllCameraInputControllers[i];
                bool v_sucess = RaycastToCamera(cam, inPos, out hit);
                if (v_sucess)
                    return v_sucess;
            }
            hit = mEmpty2D;
            return false;
        }

        static public bool Raycast(Vector3 inPos, out RaycastHit hit)
        {
            for (int i = 0; i < AllCameraInputControllers.Count; ++i)
            {
                CameraInputController cam = AllCameraInputControllers[i];
                bool v_sucess = RaycastToCamera(cam, inPos, out hit);
                if (v_sucess)
                    return v_sucess;
            }
            hit = mEmpty3D;
            return false;
        }

        static public bool Raycast(Vector3 inPos, out RaycastResult hit)
        {
            hit = mEmptyUGUI;
            PointerEventData v_pointer = new PointerEventData(EventSystem.current);
            v_pointer.position = inPos;
            List<RaycastResult> v_raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(v_pointer, v_raycastResults);
            if (v_raycastResults.Count > 0)
            {
                bool v_foundEventHandler = false;
                for (int i = 0; i < v_raycastResults.Count; i++)
                {
                    RaycastResult v_raycastResult = v_raycastResults[i];
                    v_foundEventHandler = UpdateRayCastWithType<IEventSystemHandler>(ref v_raycastResult);
                    if (v_foundEventHandler)
                    {
                        hit = v_raycastResult;
                        break;
                    }
                }
                if (!v_foundEventHandler)
                    hit = v_raycastResults.GetFirst();
                return true;
            }
            hit = mEmptyUGUI;
            return false;
        }

        #endregion

        static protected bool UpdateRayCastWithType<T>(ref RaycastResult p_ray)
        {
            if (p_ray.gameObject != null)
            {
                List<Component> v_components = new List<Component>();
                v_components.AddChecking(p_ray.gameObject.GetComponent(typeof(T)));
                v_components.MergeList(new List<Component>(p_ray.gameObject.GetComponentsInParent(typeof(T))));
                foreach (Component v_component in v_components)
                {
                    if (!(v_component is MonoBehaviour) || (v_component as MonoBehaviour).enabled)
                    {
                        p_ray.gameObject = v_component.gameObject;
                        return true;
                    }
                }
            }
            return false;
        }

        static DrawTypeEnum GetDrawOrderHighestDepth(Transform p_root, out int p_depth)
        {
            DrawTypeEnum v_sucess = (DrawTypeEnum)0;
            p_depth = 0;
            Renderer v_renderer = GetRendererWithHighestDepthInTransform(p_root);
            if (v_sucess == (DrawTypeEnum)0 && v_renderer != null)
            {
                v_sucess = v_renderer is MeshRenderer ? DrawTypeEnum.MeshRendererType : (v_renderer is SpriteRenderer ? DrawTypeEnum.SpriteRendererType : (DrawTypeEnum)0);
                p_depth = v_renderer.sortingOrder;
            }

            return v_sucess;
        }

        static Renderer GetRendererWithHighestDepthInTransform(Transform p_root)
        {
            Renderer v_renderer = null;
            if (p_root != null)
            {
                v_renderer = p_root.GetComponent<Renderer>();
                if (v_renderer == null)
                {
                    Renderer[] v_renderersInChildrens = p_root.GetComponentsInChildren<Renderer>();
                    foreach (Renderer v_rendererChild in v_renderersInChildrens)
                    {
                        if (v_renderer == null)
                            v_renderer = v_rendererChild;
                        else
                        {
                            if (v_rendererChild.sortingOrder > v_renderer.sortingOrder)
                                v_renderer = v_rendererChild;
                        }
                    }
                }
            }
            return v_renderer;
        }

        static public CameraInputController FindInputControllerForLayer(int layer)
        {
            int layerMask = 1 << layer;

            for (int i = 0; i < AllCameraInputControllers.Count; ++i)
            {
                CameraInputController cam = AllCameraInputControllers[i];
                Camera uc = cam.cachedCamera;
                if ((uc != null) && (uc.cullingMask & layerMask) != 0) return cam;
            }
            return null;
        }

        /// <summary>
        /// Using the keyboard will result in 1 or -1, depending on whether up or down keys have been pressed.
        /// </summary>

        static int GetDirection(KeyCode up, KeyCode down)
        {
            if (Input.GetKeyDown(up)) return 1;
            if (Input.GetKeyDown(down)) return -1;
            return 0;
        }

        /// <summary>
        /// Using the keyboard will result in 1 or -1, depending on whether up or down keys have been pressed.
        /// </summary>

        static int GetDirection(KeyCode up0, KeyCode up1, KeyCode down0, KeyCode down1)
        {
            if (Input.GetKeyDown(up0) || Input.GetKeyDown(up1)) return 1;
            if (Input.GetKeyDown(down0) || Input.GetKeyDown(down1)) return -1;
            return 0;
        }

        /// <summary>
        /// Using the joystick to move the UI results in 1 or -1 if the threshold has been passed, mimicking up/down keys.
        /// </summary>

        static int GetDirection(string axis)
        {
            float time = Time.realtimeSinceStartup;

            if (NextEvent < time)
            {
                float val = Input.GetAxis(axis);

                if (val > 0.75f)
                {
                    NextEvent = time + 0.25f;
                    return 1;
                }

                if (val < -0.75f)
                {
                    NextEvent = time + 0.25f;
                    return -1;
                }
            }
            return 0;
        }

#if !NGUI_DLL

        /// <summary>
        /// Returns whether the widget should be currently highlighted as far as the UICamera knows.
        /// </summary>

        static public bool IsHighlighted(GameObject go)
        {
            for (int i = mHighlighted.Count; i > 0;)
            {
                Highlighted hl = mHighlighted[--i];
                if (hl.go == go) return true;
            }
            return false;
        }

        /// <summary>
        /// Apply or remove highlighted (hovered) state from the specified object.
        /// </summary>

#endif

        protected static void Highlight(GameObject go, bool highlighted)
        {
#if !NGUI_DLL
            if (go != null)
            {
                for (int i = mHighlighted.Count; i > 0;)
                {
                    Highlighted hl = mHighlighted[--i];

                    if (hl == null || hl.go == null)
                    {
                        mHighlighted.RemoveAt(i);
                    }
                    else if (hl.go == go)
                    {
                        if (highlighted)
                        {
                            ++hl.counter;
                        }
                        else if (--hl.counter < 1)
                        {
                            mHighlighted.Remove(hl);
                            Notify(go, "OnHover", false);
                        }
                        return;
                    }
                }

                if (highlighted)
                {
                    Highlighted hl = new Highlighted();
                    hl.go = go;
                    hl.counter = 1;
                    mHighlighted.Add(hl);
                    Notify(go, "OnHover", true);
                }
            }
#else
		KiltUtils.CallStaticFunction(typeof(UICamera), "Highlight", go, highlighted);
#endif
        }

        /// <summary>
        /// Call OnGlobalPress
        /// </summary>

        static public void CallGlobalPress(bool p_pressed)
        {
            if (OnGlobalPress != null)
                OnGlobalPress(p_pressed);
        }

        /// <summary>
        /// Call OnGlobalDrag
        /// </summary>

        static public void CallGlobalDrag(Vector2 p_delta)
        {
            if (OnGlobalDrag != null)
                OnGlobalDrag(p_delta);
        }

        /// <summary>
        /// Call OnGlobalDrop
        /// </summary>

        static public void CallGlobalDrop(GameObject p_dropObject)
        {
            if (OnGlobalDrop != null)
                OnGlobalDrop(p_dropObject);
        }

        /// <summary>
        /// Generic notification function. Used in place of SendMessage to shorten the code and allow for more than one receiver.
        /// </summary>

        static public void Notify(GameObject go, string funcName, object obj, bool safeNotify = false)
        {
            if (safeNotify)
                SafeNotify(go, funcName, obj);
            else if (go != null)
            {
                go.SendMessage(funcName, obj, SendMessageOptions.DontRequireReceiver);
                if (genericEventHandler != null && genericEventHandler != go)
                {
                    genericEventHandler.SendMessage(funcName, obj, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        /// <summary>
        /// Used to prevent MissingMethodException when calling Notify (Slower then Normal Notify)
        /// </summary>
        static protected void SafeNotify(GameObject go, string funcName, object obj)
        {
            if (go != null)
            {
                List<MonoBehaviour> v_behavioursToSend = ComponentsToNotify(go);
                foreach (MonoBehaviour v_behaviour in v_behavioursToSend)
                {
                    if (v_behaviour != null)
                    {
                        FunctionUtils.CallFunction(v_behaviour, funcName, obj);
                    }
                }
            }
        }

        static protected List<MonoBehaviour> ComponentsToNotify(GameObject go)
        {
            List<MonoBehaviour> v_behaviours = new List<MonoBehaviour>();
            if (go != null)
                v_behaviours.MergeList(new List<MonoBehaviour>(go.GetComponents<MonoBehaviour>()));
            if (genericEventHandler != null && genericEventHandler != go)
                v_behaviours.MergeList(new List<MonoBehaviour>(genericEventHandler.GetComponents<MonoBehaviour>()));
            return v_behaviours;
        }

        /// <summary>
        /// Get or create a touch event.
        /// </summary>
        /// 
#if NGUI_DLL
	static new public MouseOrTouch GetTouch (int id)
#else
        static public MouseOrTouch GetTouch(int id)
#endif
        {
            MouseOrTouch touch = null;

            if (!Touches.TryGetValue(id, out touch))
            {
                touch = new MouseOrTouch();
                touch.touchBegan = true;
                Touches.Add(id, touch);
            }
            return touch;
        }

        /// <summary>
        /// Remove a touch event from the list.
        /// </summary>

#if NGUI_DLL
	static new public void RemoveTouch (int id) 
#else
        static public void RemoveTouch(int id)
#endif
        {
            Touches.Remove(id);
        }

        /// <summary>
        /// Add this camera to the list.
        /// </summary>

#if NGUI_DLL
	static public void AddInCorrectDepthPos(UICamera p_uiCamera)
	{
		//Remove Nulls and Previus added components of this object
		List<UICamera> v_list = new List<UICamera>(UICamList);
		v_list.RemoveChecking(p_uiCamera);
		UICamList = new List<UICamera>(v_list.ToArray());
		//We must add cameras with Highest Depth First
		if(p_uiCamera != null)
		{
			Camera v_camera = p_uiCamera.gameObject.GetComponent<Camera>();
			if(v_camera != null)
			{
				for(int i=0; i<UICamList.Count; i++)
				{
					if(UICamList[i] != null)
					{
						Camera v_checkingCamera = p_uiCamera.gameObject.GetComponent<Camera>();
						if(v_checkingCamera != null)
						{
							if(v_camera.depth >= v_checkingCamera.depth)
							{
								UICamList.Insert(i, p_uiCamera);
								return;
							}
						}
						else
						{
							UICamList.Insert(i, p_uiCamera);
							return;
						}
						
					}
					else
					{
						UICamList[i] = p_uiCamera;
						return;
					}
				}
			}
			//If not added, add.
			UICamList.Add(p_uiCamera);
		}
	}
#else
        public static void AddInCorrectDepthPos(CameraInputController p_uiCamera)
        {
            //Remove Nulls and Previus added components of this object
            List<CameraInputController> v_list = new List<CameraInputController>(AllCameraInputControllers);
            v_list.RemoveChecking(p_uiCamera);
            AllCameraInputControllers = new List<CameraInputController>(v_list.ToArray());
            //We must add cameras with Highest Depth First
            if (p_uiCamera != null)
            {
                Camera v_camera = p_uiCamera.gameObject.GetComponent<Camera>();
                if (v_camera != null)
                {
                    for (int i = 0; i < AllCameraInputControllers.Count; i++)
                    {
                        if (AllCameraInputControllers[i] != null)
                        {
                            Camera v_checkingCamera = p_uiCamera.gameObject.GetComponent<Camera>();
                            if (v_checkingCamera != null)
                            {
                                if (v_camera.depth >= v_checkingCamera.depth)
                                {
                                    AllCameraInputControllers.Insert(i, p_uiCamera);
                                    return;
                                }
                            }
                            else
                            {
                                AllCameraInputControllers.Insert(i, p_uiCamera);
                                return;
                            }

                        }
                        else
                        {
                            AllCameraInputControllers[i] = p_uiCamera;
                            return;
                        }
                    }
                }
                //If not added, add.
                AllCameraInputControllers.Add(p_uiCamera);
            }
        }
#endif

        public bool CameraCanInteractWithObject(GameObject p_object)
        {
            if (p_object != null)
            {
                if (eventReceiverMask == (eventReceiverMask | (1 << p_object.layer)))
                    return true;
            }
            return false;
        }

        #endregion

        #region Unity Functions

        protected virtual void OnApplicationQuit()
        {
#if !NGUI_DLL
            mHighlighted.Clear();
#else
		KiltUtils.CallFunction(this, "OnApplicationQuit", typeof(UICamera));
#endif
        }

        protected virtual void Awake()
        {
            AddInCorrectDepthPos(this);
            //UICamList.Add(this);


#if !UNITY_3_5 && !UNITY_4_0
            // We don't want the camera to send out any kind of mouse events
            cachedCamera.eventMask = 0;
#endif

            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.IPhonePlayer
                )
            {
                useMouse = false;
                useTouch = true;

                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    useKeyboard = false;
                    useController = false;
                }
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor ||
                     Application.platform == RuntimePlatform.OSXEditor)
            {
                IsEditor = true;
            }

            // Save the starting mouse position
            Mouse[0].pos.x = Input.mousePosition.x;
            Mouse[0].pos.y = Input.mousePosition.y;
            lastTouchPosition = Mouse[0].pos;

            // If no event receiver mask was specified, use the camera's mask
            if (eventReceiverMask == -1) eventReceiverMask = cachedCamera.cullingMask;
        }

        /// <summary>
        /// Remove this camera from the list.
        /// </summary>

        protected virtual void OnDestroy() { AllCameraInputControllers.Remove(this); }

        /// <summary>
        /// Sort the list when enabled.
        /// </summary>

        protected virtual void OnEnable() { AllCameraInputControllers.StableSort(CompareFunc); }

        /// <summary>
        /// Update the object under the mouse if we're not using touch-based input.
        /// </summary>

        protected virtual void FixedUpdate()
        {
            if (useMouse && Application.isPlaying && handlesEvents)
            {
                UpdateHoveredObject();
                for (int i = 0; i < 3; ++i) Mouse[i].current = hoveredObject;
            }
        }

        /// <summary>
        /// Check the input and send out appropriate events.
        /// </summary>

        protected virtual void Update()
        {
            // Only the first UI layer should be processing events
            if (!Application.isPlaying || !handlesEvents) return;

            current = this;

            // Update mouse input
            if (useMouse || (useTouch && IsEditor)) ProcessMouse();

            // Process touch input
            if (useTouch) ProcessTouches();

            // Custom input processing
            if (onCustomInput != null) onCustomInput();

            // Clear the selection on the cancel key, but only if mouse input is allowed
            if (useMouse && Sel != null && ((cancelKey0 != KeyCode.None && Input.GetKeyDown(cancelKey0)) ||
                                            (cancelKey1 != KeyCode.None && Input.GetKeyDown(cancelKey1)))) selectedObject = null;

            // Forward the input to the selected object
            if (Sel != null)
            {
                string input = Input.inputString;

                // Adding support for some macs only having the "Delete" key instead of "Backspace"
                if (useKeyboard && Input.GetKeyDown(KeyCode.Delete)) input += "\b";

                if (input.Length > 0)
                {
                    if (!stickyTooltip && Tooltip != null) ShowTooltip(false);
                    Notify(Sel, "OnInput", input);
                }
            }
            else inputHasFocus = false;

            // Update the keyboard and joystick events
            if (Sel != null) ProcessOthers();

            // If it's time to show a tooltip, inform the object we're hovering over
            if (useMouse && Hover != null)
            {
                float scroll = Input.GetAxis(scrollAxisName);
                if (scroll != 0f) Notify(Hover, "OnScroll", scroll);

                if (showTooltips && TooltipTime != 0f && (TooltipTime < Time.realtimeSinceStartup ||
                                                          Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    Tooltip = Hover;
                    ShowTooltip(true);
                }
            }
            current = null;
        }

#if UNITY_EDITOR
        void OnGUI()
        {
            if (debug)
            {
                if (lastHit.collider != null)
                    GUILayout.Label("Last Hit: " + GetHierarchy(lastHit.collider.gameObject).Replace("\"", ""));
                if (lastHit2D.collider != null)
                    GUILayout.Label("Last Hit 2D: " + GetHierarchy(lastHit2D.collider.gameObject).Replace("\"", ""));
                if (lastHitUGUI.gameObject != null)
                    GUILayout.Label("Last Hit UGUI: " + GetHierarchy(lastHitUGUI.gameObject).Replace("\"", ""));
            }
        }
#endif

        #endregion

        #region Process Touchs

        /// <summary>
        /// Update mouse input.
        /// </summary>
#if NGUI_DLL
	public new void ProcessMouse ()
#else
        public void ProcessMouse()
#endif
        {
            bool updateRaycast = (useMouse && Time.timeScale < 0.9f);

            if (!updateRaycast)
            {
                for (int i = 0; i < 3; ++i)
                {
                    if (Input.GetMouseButton(i) || Input.GetMouseButtonUp(i))
                    {
                        updateRaycast = true;
                        break;
                    }
                }
            }

            // Update the position and delta
            Mouse[0].pos = Input.mousePosition;
            Mouse[0].delta = Mouse[0].pos - lastTouchPosition;

            bool posChanged = (Mouse[0].pos != lastTouchPosition);
            lastTouchPosition = Mouse[0].pos;

            // Update the object under the mouse
            if (updateRaycast)
            {
                UpdateHoveredObject();
                Mouse[0].current = hoveredObject;
            }

            // Propagate the updates to the other mouse buttons
            for (int i = 1; i < 3; ++i)
            {
                Mouse[i].pos = Mouse[0].pos;
                Mouse[i].delta = Mouse[0].delta;
                Mouse[i].current = Mouse[0].current;
            }

            // Is any button currently pressed?
            bool isPressed = false;

            for (int i = 0; i < 3; ++i)
            {
                if (Input.GetMouseButton(i))
                {
                    isPressed = true;
                    break;
                }
            }

            if (isPressed)
            {
                // A button was pressed -- cancel the tooltip
                TooltipTime = 0f;
            }
            else if (useMouse && posChanged && (!stickyTooltip || Hover != Mouse[0].current))
            {
                if (TooltipTime != 0f)
                {
                    // Delay the tooltip
                    TooltipTime = Time.realtimeSinceStartup + tooltipDelay;
                }
                else if (Tooltip != null)
                {
                    // Hide the tooltip
                    ShowTooltip(false);
                }
            }

            // The button was released over a different object -- remove the highlight from the previous
            if (useMouse && !isPressed && Hover != null && Hover != Mouse[0].current)
            {
                if (Tooltip != null) ShowTooltip(false);
                Highlight(Hover, false);
                Hover = null;
            }

            // Process all 3 mouse buttons as individual touches
            if (useMouse)
            {
                for (int i = 0; i < 3; ++i)
                {
                    bool pressed = Input.GetMouseButtonDown(i);
                    bool unpressed = Input.GetMouseButtonUp(i);

                    currentTouch = Mouse[i];
                    currentTouchID = -1 - i;

                    // We don't want to update the last camera while there is a touch happening
                    if (pressed) currentTouch.pressedCam = currentCamera;
                    else if (currentTouch.pressed != null) currentCamera = currentTouch.pressedCam;

                    // Process the mouse events
                    ProcessTouch(pressed, unpressed);
                }
                currentTouch = null;
            }

            // If nothing is pressed and there is an object under the touch, highlight it
            if (useMouse && !isPressed && Hover != Mouse[0].current)
            {
                TooltipTime = Time.realtimeSinceStartup + tooltipDelay;
                Hover = Mouse[0].current;
                Highlight(Hover, true);
            }
        }

        /// <summary>
        /// Update touch-based events.
        /// </summary>

#if NGUI_DLL
	public new void ProcessTouches ()
#else
        public void ProcessTouches()
#endif
        {
            for (int i = 0; i < Input.touchCount; ++i)
            {
                Touch input = Input.GetTouch(i);

                currentTouchID = allowMultiTouch ? input.fingerId : 1;
                currentTouch = GetTouch(currentTouchID);

                bool pressed = (input.phase == TouchPhase.Began) || currentTouch.touchBegan;
                bool unpressed = (input.phase == TouchPhase.Canceled) || (input.phase == TouchPhase.Ended);
                currentTouch.touchBegan = false;

                if (pressed)
                {
                    currentTouch.delta = Vector2.zero;
                }
                else
                {
                    // Although input.deltaPosition can be used, calculating it manually is safer (just in case)
                    currentTouch.delta = input.position - currentTouch.pos;
                }

                currentTouch.pos = input.position;
                UpdateHoveredObject(currentTouch.pos);
                currentTouch.current = hoveredObject;
                lastTouchPosition = currentTouch.pos;

                // We don't want to update the last camera while there is a touch happening
                if (pressed) currentTouch.pressedCam = currentCamera;
                else if (currentTouch.pressed != null) currentCamera = currentTouch.pressedCam;

                // Double-tap support
                if (input.tapCount > 1) currentTouch.clickTime = Time.realtimeSinceStartup;

                // Process the events from this touch
                ProcessTouch(pressed, unpressed);

                // If the touch has ended, remove it from the list
                if (unpressed) RemoveTouch(currentTouchID);
                currentTouch = null;

                // Don't consider other touches
                if (!allowMultiTouch) break;
            }
        }

        /// <summary>
        /// Process keyboard and joystick events.
        /// </summary>
#if NGUI_DLL
	public new void ProcessOthers ()
#else
        public void ProcessOthers()
#endif
        {
            currentTouchID = -100;
            currentTouch = Controller;

            // If this is an input field, ignore WASD and Space key presses
            inputHasFocus = false;
#if NGUI_DLL
		inputHasFocus = (Sel != null && Sel.GetComponent<UIInput>() != null);
#endif

            bool submitKeyDown = (submitKey0 != KeyCode.None && Input.GetKeyDown(submitKey0)) || (submitKey1 != KeyCode.None && Input.GetKeyDown(submitKey1));
            bool submitKeyUp = (submitKey0 != KeyCode.None && Input.GetKeyUp(submitKey0)) || (submitKey1 != KeyCode.None && Input.GetKeyUp(submitKey1));

            if (submitKeyDown || submitKeyUp)
            {
                currentTouch.current = Sel;
                ProcessTouch(submitKeyDown, submitKeyUp);
                currentTouch.current = null;
            }

            int vertical = 0;
            int horizontal = 0;

            if (useKeyboard)
            {
                if (inputHasFocus)
                {
                    vertical += GetDirection(KeyCode.UpArrow, KeyCode.DownArrow);
                    horizontal += GetDirection(KeyCode.RightArrow, KeyCode.LeftArrow);
                }
                else
                {
                    vertical += GetDirection(KeyCode.W, KeyCode.UpArrow, KeyCode.S, KeyCode.DownArrow);
                    horizontal += GetDirection(KeyCode.D, KeyCode.RightArrow, KeyCode.A, KeyCode.LeftArrow);
                }
            }

            if (useController)
            {
                if (!string.IsNullOrEmpty(verticalAxisName)) vertical += GetDirection(verticalAxisName);
                if (!string.IsNullOrEmpty(horizontalAxisName)) horizontal += GetDirection(horizontalAxisName);
            }

            // Send out key notifications
            if (vertical != 0) Notify(Sel, "OnKey", vertical > 0 ? KeyCode.UpArrow : KeyCode.DownArrow);
            if (horizontal != 0) Notify(Sel, "OnKey", horizontal > 0 ? KeyCode.RightArrow : KeyCode.LeftArrow);
            if (useKeyboard && Input.GetKeyDown(KeyCode.Tab)) Notify(Sel, "OnKey", KeyCode.Tab);

            // Send out the cancel key notification
            if (cancelKey0 != KeyCode.None && Input.GetKeyDown(cancelKey0)) Notify(Sel, "OnKey", KeyCode.Escape);
            if (cancelKey1 != KeyCode.None && Input.GetKeyDown(cancelKey1)) Notify(Sel, "OnKey", KeyCode.Escape);

            currentTouch = null;
        }

        /// <summary>
        /// Process the events of the specified touch.
        /// </summary>

#if NGUI_DLL
	public new void ProcessTouch (bool pressed, bool unpressed)
#else
        public void ProcessTouch(bool pressed, bool unpressed)
#endif
        {
            // Whether we're using the mouse
            bool isMouse = (currentTouch == Mouse[0] || currentTouch == Mouse[1] || currentTouch == Mouse[2]);
            float drag = isMouse ? mouseDragThreshold : touchDragThreshold;
            float click = isMouse ? mouseClickThreshold : touchClickThreshold;

            // Send out the press message
            if (pressed)
            {
                if (Tooltip != null) ShowTooltip(false);

                currentTouch.pressStarted = true;
                CallGlobalPress(false);
                Notify(currentTouch.pressed, "OnPress", false);
                currentTouch.pressed = currentTouch.current;
                currentTouch.dragged = currentTouch.current;
                currentTouch.clickNotification = isMouse ? ClickNotification.BasedOnDelta : ClickNotification.Always;
                currentTouch.totalDelta = Vector2.zero;
                currentTouch.dragStarted = false;
                CallGlobalPress(true);
                Notify(currentTouch.pressed, "OnPress", true);

                // Clear the selection
                if (currentTouch.pressed != Sel)
                {
                    if (Tooltip != null) ShowTooltip(false);
                    selectedObject = null;
                }
            }
            else
            {
                // If the user is pressing down and has dragged the touch away from the original object,
                // unpress the original object and notify the new object that it is now being pressed on.
                if (!stickyPress && !unpressed && currentTouch.pressStarted && currentTouch.pressed != hoveredObject)
                {
                    isDragging = true;
                    CallGlobalPress(false);
                    Notify(currentTouch.pressed, "OnPress", false);
                    currentTouch.pressed = hoveredObject;
                    CallGlobalPress(true);
                    Notify(currentTouch.pressed, "OnPress", true);
                    isDragging = false;
                }

                if (currentTouch.pressStarted)
                {
                    float mag = currentTouch.delta.magnitude;

                    if (mag != 0f)
                    {
                        // Keep track of the total movement
                        currentTouch.totalDelta += currentTouch.delta;
                        mag = currentTouch.totalDelta.magnitude;

                        // If the drag event has not yet started, see if we've dragged the touch far enough to start it
                        if (!currentTouch.dragStarted && drag < mag)
                        {
                            currentTouch.dragStarted = true;
                            currentTouch.delta = currentTouch.totalDelta;
                        }

                        // If we're dragging the touch, send out drag events
                        if (currentTouch.dragStarted)
                        {
                            if (Tooltip != null) ShowTooltip(false);

                            isDragging = true;
                            bool isDisabled = (currentTouch.clickNotification == ClickNotification.None);
                            CallGlobalDrag(currentTouch.delta);
                            if (currentTouch.pressed != null)
                                Notify(currentTouch.dragged, "OnDrag", currentTouch.delta);
                            isDragging = false;

                            if (isDisabled)
                            {
                                // If the notification status has already been disabled, keep it as such
                                currentTouch.clickNotification = ClickNotification.None;
                            }
                            else if (currentTouch.clickNotification == ClickNotification.BasedOnDelta && click < mag)
                            {
                                // We've dragged far enough to cancel the click
                                currentTouch.clickNotification = ClickNotification.None;
                            }
                        }
                    }
                }
            }

            // Send out the unpress message
            if (unpressed)
            {
                currentTouch.pressStarted = false;
                if (Tooltip != null) ShowTooltip(false);

                if (currentTouch.pressed != null)
                {
                    CallGlobalPress(false);
                    Notify(currentTouch.pressed, "OnPress", false);

                    // Send a hover message to the object, but don't add it to the list of hovered items
                    // as it's already present. This happens when the mouse is released over the same button
                    // it was pressed on, and since it already had its 'OnHover' event, it never got
                    // Highlight(false), so we simply re-notify it so it can update the visible state.
                    if (useMouse && currentTouch.pressed == Hover) Notify(currentTouch.pressed, "OnHover", true);

                    // If the button/touch was released on the same object, consider it a click and select it
                    if (currentTouch.dragged == currentTouch.current ||
                        (currentTouch.clickNotification != ClickNotification.None &&
                     currentTouch.totalDelta.magnitude < drag))
                    {
                        if (currentTouch.pressed != Sel)
                        {
                            Sel = currentTouch.pressed;
                            Notify(currentTouch.pressed, "OnSelect", true, true);
                        }
                        else
                        {
                            Sel = currentTouch.pressed;
                        }

                        // If the touch should consider clicks, send out an OnClick notification
                        if (currentTouch.clickNotification != ClickNotification.None)
                        {
                            float time = Time.realtimeSinceStartup;

                            Notify(currentTouch.pressed, "OnClick", null);

                            if (currentTouch.clickTime + 0.35f > time)
                            {
                                Notify(currentTouch.pressed, "OnDoubleClick", null);
                            }
                            currentTouch.clickTime = time;
                        }
                    }
                    else // The button/touch was released on a different object
                    {
                        // Send a drop notification (for drag & drop)
                        CallGlobalDrop(currentTouch.dragged);
                        Notify(currentTouch.current, "OnDrop", currentTouch.dragged);
                    }
                }
                else
                {
                    CallGlobalDrop(null);
                    CallGlobalPress(false);
                }
                currentTouch.dragStarted = false;
                currentTouch.pressed = null;
                currentTouch.dragged = null;
            }
        }

        /// <summary>
        /// Show or hide the tooltip.
        /// </summary>

#if NGUI_DLL
	public new void ShowTooltip (bool val)
#else
        public void ShowTooltip(bool val)
#endif
        {
            TooltipTime = 0f;
            Notify(Tooltip, "OnTooltip", val);
            if (!val) Tooltip = null;
        }

        #endregion

        #region Tools

        static protected void FilterResultByLayer(LayerMask p_layerMask, ref List<RaycastResult> p_results)
        {
            if (p_results != null)
            {
                List<RaycastResult> v_newList = new List<RaycastResult>();
                foreach (RaycastResult v_result in p_results)
                {
                    if (v_result.gameObject != null && (p_layerMask == (p_layerMask | (1 << v_result.gameObject.layer))))
                        v_newList.Add(v_result);
                }
                p_results.Clear();
                foreach (RaycastResult v_result in v_newList)
                {
                    p_results.Add(v_result);
                }
            }
        }

        static protected string GetHierarchy(GameObject obj)
        {
            string path = obj.name;

            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = obj.name + "/" + path;
            }
            return "\"" + path + "\"";
        }

        static protected bool GetActive(GameObject go)
        {
#if UNITY_3_5
		return go && go.active;
#else
            return go && go.activeInHierarchy;
#endif
        }

        static protected T FindInParents<T>(GameObject go) where T : Component
        {
            if (go == null) return null;
            object comp = go.GetComponent<T>();

            if (comp == null)
            {
                Transform t = go.transform.parent;

                while (t != null && comp == null)
                {
                    comp = t.gameObject.GetComponent<T>();
                    t = t.parent;
                }
            }
            return (T)comp;
        }

        #endregion
    }
}
