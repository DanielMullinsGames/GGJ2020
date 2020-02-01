#if UNITY_5_3 || UNITY_5_2 || UNITY_5_1 || UNITY_5_0 || UNITY_4
#define UNITY_5_3_OR_BELOW
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kilt.EasyRopes2D
{
    public class ClickEffectController : Singleton<ClickEffectController>
    {
        #region Events

        public static event System.Action<bool> OnGlobalPress;
        public static event System.Action<Vector2> OnGlobalDrag;
        public static event System.Action<GameObject> OnGlobalDrop;

        #endregion

        #region Private Functions

        [SerializeField]
        float m_pressRadius = 0.06f; // In Global Distance
        [SerializeField]
        GameObject m_clickEffect = null;
        [SerializeField]
        GameObject m_trailEffect = null;
        [SerializeField]
        Camera m_clickEffectCamera = null;

        GameObject _effectInScene;
        GameObject _trailEffectInScene;
        #endregion

        #region Public Functions

        public float PressRadius { get { return m_pressRadius; } set { m_pressRadius = value; } }
        public GameObject ClickEffect { get { return m_clickEffect; } set { m_clickEffect = value; } }
        public GameObject TrailEffect { get { return m_trailEffect; } set { m_trailEffect = value; } }
        public Camera ClickEffectCamera
        {
            get
            {
                if (m_clickEffectCamera == null)
                {
                    m_clickEffectCamera = GetComponentInChildren<Camera>();
                    if(m_clickEffectCamera == null)
                        m_clickEffectCamera = Camera.main;
                }
                return m_clickEffectCamera;
            }
        }
        #endregion

        #region Unity Functions

        protected override void Awake()
        {
            base.Awake();
            RegistedEvents();
        }

        protected virtual void OnEnable()
        {
            RegisterLevelEvents();
        }

        protected virtual void OnDisable()
        {
            UnregisterLevelEvents();
        }

        protected virtual void Start()
        {
            ShowTrailEffect(); //Force Create Trail Effect Before Any Click
        }

        protected virtual void OnDestroy()
        {
            UnregisterEvents();
        }

#if UNITY_5_3_OR_BELOW
    protected virtual void OnLevelWasLoaded()
	{
		if(m_instance != this && m_instance != null)
			Object.Destroy(this.gameObject);
		else
		{
			if(m_instance == null)
				Instance = this;
		}
	}
#else
        protected virtual void OnSceneLoaded(UnityEngine.SceneManagement.Scene p_scene, UnityEngine.SceneManagement.LoadSceneMode p_mode)
        {
            if (s_instance != this && s_instance != null)
                Object.Destroy(this.gameObject);
            else
            {
                if (s_instance == null)
                    Instance = this;
            }
        }
#endif

        #endregion

        #region Helper Functions

        protected void RegisterLevelEvents()
        {
            UnregisterLevelEvents();
#if !UNITY_5_3_OR_BELOW
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
#endif
        }

        protected void UnregisterLevelEvents()
        {
#if !UNITY_5_3_OR_BELOW
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
#endif
        }

        protected virtual void RegistedEvents()
        {
            UnregisterEvents();
            //#if NGUI_KILT_DLL
            CameraInputController.OnGlobalPress += HandleOnGlobalPress;
            CameraInputController.OnGlobalDrag += HandleOnGlobalDrag;
            CameraInputController.OnGlobalDrop += HandleOnGlobalDrop;
            //#endif
        }

        protected virtual void UnregisterEvents()
        {
            //#if NGUI_KILT_DLL
            CameraInputController.OnGlobalPress -= HandleOnGlobalPress;
            CameraInputController.OnGlobalDrag -= HandleOnGlobalDrag;
            CameraInputController.OnGlobalDrop -= HandleOnGlobalDrop;
            //#endif
        }

        protected virtual GameObject FindCloseObjectInRange(Vector2 p_globalPoint, float p_radius)
        {
            GameObject v_returnObject = null;
            if (PressRadius > 0)
            {
                float v_currentDistance = 9999999f;
                float v_angleStep = 10f;
                for (float j = 0; j < 360.0f; j += v_angleStep)
                {
                    Vector3 v_rotatedVector = RopeInternalUtils.RotateZ(new Vector2(1, 0), j);
                    RaycastHit2D v_hit = Physics2D.Raycast(p_globalPoint, v_rotatedVector, p_radius);
                    if (v_hit.transform != null && v_hit.transform.gameObject != null)
                    {
                        if (v_hit.distance < v_currentDistance)
                        {
                            v_returnObject = v_hit.transform.gameObject;
                            v_currentDistance = v_hit.distance;
                        }
                    }
                }
            }
            return v_returnObject;
        }

        protected void ShowEffect()
        {
            Vector2 v_globalPointInClickEffectCamera = ClickEffectCamera.ScreenToWorldPoint(CameraInputController.currentTouch != null ? CameraInputController.currentTouch.pos : (Vector2)Input.mousePosition);
            //ShowEffect In ClickEffectCamera ScreenSpace
            ShowEffect(v_globalPointInClickEffectCamera, PressRadius);
        }

        //In ClickCamera Space
        protected virtual void ShowEffect(Vector2 p_point, float p_radius)
        {
            GameObject v_object = null;
            if (ClickEffect != null)
            {
                v_object = GameObject.Instantiate(ClickEffect) as GameObject;
                v_object.transform.parent = ClickEffectCamera.transform;
                v_object.transform.position = p_point;
            }
            if (_effectInScene != null)
            {
                Object.Destroy(_effectInScene);
                _effectInScene = v_object;
            }
        }

        protected void ShowTrailEffect()
        {
            if (TrailEffect != null && _trailEffectInScene == null)
            {
                Vector2 v_globalPointInClickEffectCamera = ClickEffectCamera.ScreenToWorldPoint(Input.mousePosition);
                //ShowEffect In ClickEffectCamera ScreenSpace
                ShowTrailEffect(v_globalPointInClickEffectCamera);
            }
        }

        protected virtual void ShowTrailEffect(Vector2 p_point)
        {
            if (TrailEffect != null && _trailEffectInScene == null)
            {
                _trailEffectInScene = GameObject.Instantiate(TrailEffect) as GameObject;
                _trailEffectInScene.name = "Trail_Effect";
                _trailEffectInScene.transform.parent = ClickEffectCamera.transform;
                _trailEffectInScene.transform.position = new Vector3(p_point.x, p_point.y, TrailEffect.transform.position.z);
            }
        }

        //Get Radius based in ClickEffect Camera distances
        protected virtual float GetNewRadius(Camera p_newCamera, Vector2 p_screenPoint)
        {
            float v_newRadius = PressRadius;
            if (p_newCamera != null)
            {
                //Get global Point in NewCamera Space and In ClickEffectCamera Space 
                Vector2 v_globalPoint = p_newCamera.ScreenToWorldPoint(p_screenPoint);
                Vector2 v_globalPointInClickEffectCamera = ClickEffectCamera.ScreenToWorldPoint(p_screenPoint);
                //Pick one point in PressRadius distance in Global ClickEffectCamera Space
                Vector2 v_radiusDistancePointInClickEffect = new Vector2(v_globalPointInClickEffectCamera.x - PressRadius, v_globalPointInClickEffectCamera.y);
                //Send This distance point to ScreenSpace
                Vector2 v_screenDistancePoint = ClickEffectCamera.WorldToScreenPoint(v_radiusDistancePointInClickEffect);
                //Send ScreenDistance Point to newCamera World Space and get final value
                v_newRadius = Vector2.Distance(p_newCamera.ScreenToWorldPoint(v_screenDistancePoint), v_globalPoint);
            }
            return v_newRadius;
        }

        protected virtual void TryUpdateCurrentTouch(Camera p_pressCamera)
        {
            if (p_pressCamera != null && p_pressCamera != ClickEffectCamera)
            {
                if (CameraInputController.currentTouch != null && CameraInputController.currentTouch.pressed == null)
                {
                    Vector2 v_globalPoint = p_pressCamera.ScreenToWorldPoint(CameraInputController.currentTouch.pos);
                    float v_newRadius = GetNewRadius(p_pressCamera, CameraInputController.currentTouch.pos);
                    GameObject v_object = FindCloseObjectInRange(v_globalPoint, v_newRadius);
                    //if(v_object == null && _trailEffectInScene != null)
                    //	v_object = _trailEffectInScene;
                    CameraInputController.currentTouch.current = v_object;
                    CameraInputController.currentTouch.pressed = v_object;
                    CameraInputController.currentTouch.dragged = v_object;
                }
            }
        }

        #endregion

        #region Events Registration

        protected virtual void HandleOnGlobalPress(bool p_isPressed)
        {
            if (p_isPressed)
            {
                ShowTrailEffect();
                if (CameraInputController.currentTouch != null)
                {
                    ShowEffect();
                    foreach (Camera v_camera in Camera.allCameras)
                    {
                        TryUpdateCurrentTouch(v_camera);
                        if (CameraInputController.currentTouch.pressed != null)
                            break;
                    }
                    if (CameraInputController.currentTouch.pressedCam != null && CameraInputController.currentTouch.pressed == null)
                        TryUpdateCurrentTouch(CameraInputController.currentTouch.pressedCam);
                }
            }
            if (OnGlobalPress != null)
                OnGlobalPress(p_isPressed);
        }

        protected virtual void HandleOnGlobalDrag(Vector2 p_delta)
        {
            if (OnGlobalDrag != null)
                OnGlobalDrag(p_delta);
        }

        protected virtual void HandleOnGlobalDrop(GameObject p_dropObject)
        {
            if (OnGlobalDrop != null)
                OnGlobalDrop(p_dropObject);
        }

        #endregion
    }
}
