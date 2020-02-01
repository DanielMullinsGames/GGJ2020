#if UNITY_5_3 || UNITY_5_2 || UNITY_5_1 || UNITY_5_0 || UNITY_4
#define UNITY_5_3_OR_BELOW
#endif

using UnityEngine;
using System.Collections;

namespace Kilt.EasyRopes2D.Examples
{

    public enum AreaTypeEnum { Rectangle, Triangle, Ellipse }

    [ExecuteInEditMode]
    public class MassCalculator : MonoBehaviour
    {

        #region Private Variables
        [SerializeField]
        Vector2 _size = new Vector2(100, 100);
        [SerializeField]
        float _density = 0.0001f;
        [SerializeField]
        float _resistence = 0.001f;
        [SerializeField]
        AreaTypeEnum _areaType = AreaTypeEnum.Rectangle;

        Vector2 m_size = new Vector2(100, 100);
        float m_density = 0.0001f;
        float m_resistence = 0.001f;
        AreaTypeEnum m_areaType = AreaTypeEnum.Rectangle;

        #endregion

        #region Public Properties

        public Vector2 Size
        {
            get { return _size; }
            set
            {
                if (m_size == value && _size == value)
                    return;
                m_size = value;
                _size = m_size;
                UpdateRigidBody2D();
            }
        }

        public float Density
        {
            get { return _density; }
            set
            {
                if (m_density == value && _density == value)
                    return;
                m_density = value;
                _density = m_density;
                UpdateRigidBody2D();
            }
        }

        public float Resistence
        {
            get { return _resistence; }
            set
            {
                if (m_resistence == value && _resistence == value)
                    return;
                m_resistence = value;
                _resistence = m_resistence;
                UpdateResistence();
            }
        }

        public AreaTypeEnum AreaType
        {
            get { return _areaType; }
            set
            {
                if (m_areaType == value && _areaType == value)
                    return;
                m_areaType = value;
                _areaType = m_areaType;
                UpdateRigidBody2D();
            }
        }

        #endregion

        #region Unity Functions

        bool _loaded = false;
#if UNITY_5_3_OR_BELOW
        protected virtual void OnLevelWasLoaded()
	    {
		    if(!_loaded)
		    {
			    _loaded = true;
			    CorrectValues(true);
			    UpdateRigidBody2D();
			    UpdateResistence();
		    }
	    }
#else
        protected virtual void OnSceneLoaded(UnityEngine.SceneManagement.Scene p_scene, UnityEngine.SceneManagement.LoadSceneMode p_mode)
        {
            if (!_loaded)
            {
                _loaded = true;
                CorrectValues(true);
                UpdateRigidBody2D();
                UpdateResistence();
            }
        }
#endif

        protected virtual void Awake()
        {
            if (!_loaded)
            {
                _loaded = true;
                CorrectValues(true);
                UpdateRigidBody2D();
                UpdateResistence();
            }
        }

        protected virtual void OnEnable()
        {
            RegisterLevelEvents();
        }

        protected virtual void OnDisable()
        {
            UnregisterLevelEvents();
        }

        protected virtual void Update()
        {
            CorrectValues();
        }

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

        public virtual void CorrectValues(bool p_force = false)
        {
            if (p_force || Application.isEditor)
            {
                if (m_size != _size)
                    Size = _size;
                if (m_density != _density)
                    Density = _density;
                if (m_resistence != _resistence)
                    Resistence = _resistence;
                if (m_areaType != _areaType)
                    AreaType = _areaType;
            }
        }

        public virtual float GetArea()
        {
            float v_area = 0;
            if (AreaType == AreaTypeEnum.Ellipse)
                v_area = 3.14f * (Size.x / 2.0f * Size.y / 2.0f);
            else if (AreaType == AreaTypeEnum.Triangle)
                v_area = (Size.x * Size.y) / 2.0f;
            else
                v_area = Size.x * Size.y;
            return v_area;
        }

        public virtual void UpdateRigidBody2D()
        {
            if (GetComponent<Rigidbody2D>() != null)
            {
                GetComponent<Rigidbody2D>().mass = GetArea() * Density;
            }
        }

        public virtual void UpdateResistence()
        {
            DamageableBlock v_block = GetComponent<DamageableBlock>();
            if (v_block != null)
            {
                v_block.CurrentLife = v_block.CurrentLife <= 0 ? v_block.MaxLife : Mathf.Clamp(v_block.CurrentLife, 0, v_block.MaxLife);
                float v_percent = v_block.MaxLife != 0 ? Mathf.Clamp(v_block.CurrentLife / v_block.MaxLife, 0, 1) : 0;
                v_block.MaxLife = GetArea() * Resistence;
                v_block.CurrentLife = v_percent * v_block.MaxLife;
            }
        }

        #endregion
    }
}
