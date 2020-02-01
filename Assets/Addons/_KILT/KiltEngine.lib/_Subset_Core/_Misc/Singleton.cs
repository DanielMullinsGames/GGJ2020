using UnityEngine;
using System.Collections;
using Kilt.Extensions;
using Kilt.Reflection;

namespace Kilt
{
    public abstract class Singleton<T> : BaseSingleton where T : BaseSingleton
    {
        #region Static Functions

        protected static T s_instance;

        public static T Instance
        {
            get
            {
                if (s_instance == null && EnableSingletonCreation)
                {
                    System.Type v_type = typeof(T);
                    s_instance = GameObject.FindObjectOfType(v_type) as T;

                    try
                    {
                        if (s_instance == null && v_type != null && !typeof(T).IsAbstract())
                        {
                            string v_name = "(singleton) " + typeof(T).ToString();
                            T v_object = new GameObject(v_name).AddComponent<T>();
                            v_object.gameObject.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
                            s_instance = v_object;
                            DestroyUtils.Destroy(v_object.gameObject, true);
                        }
                    }
                    catch { }
                }

                return s_instance;
            }
            set
            {
                if (s_instance == value)
                    return;
                s_instance = value;
            }
        }

        public static bool InstanceExists()
        {
            return GetInstance(false) == null ? false : true;
        }

        public static T GetInstance(bool p_canCreateANewOne = false)
        {
            T v_instance = null;
            if (p_canCreateANewOne)
                v_instance = Instance;
            else
            {
                if (s_instance == null)
                    s_instance = GameObject.FindObjectOfType(typeof(T)) as T;
                v_instance = s_instance;
            }
            return v_instance;
        }

        #endregion

        # region Internal Properties

        protected bool _keepNewInstanceIfDuplicated = false;
        protected virtual bool KeepNewInstanceIfDuplicated
        {
            get

            {
                return _keepNewInstanceIfDuplicated;
            }
            set

            {
                if (_keepNewInstanceIfDuplicated == value)
                    return;
                _keepNewInstanceIfDuplicated = value;
            }
        }

        #endregion

        #region Unity Functions

        protected virtual void Awake()
        {
            CheckInstance();
        }

        #endregion

        #region Helper Functions

        protected virtual void CheckInstance()
        {
            if (KeepNewInstanceIfDuplicated)
            {
                if (s_instance != this && s_instance != null)
                    Kilt.DestroyUtils.Destroy(s_instance.gameObject);
                s_instance = this as T;
            }
            else
            {

                if (s_instance != this && s_instance != null && !s_instance.IsMarkedToDestroy(true))
                    DestroyUtils.Destroy(this.gameObject);
                else
                {
                    if (s_instance == null || (s_instance.IsMarkedToDestroy(true) && !this.IsMarkedToDestroy(true)))
                        Instance = this as T;
                }
            }
        }

        #endregion
    }
}
