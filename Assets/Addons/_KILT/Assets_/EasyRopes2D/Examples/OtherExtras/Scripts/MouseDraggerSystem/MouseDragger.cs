using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kilt.Extensions;

namespace Kilt.EasyRopes2D.Examples
{
    public enum MouseButtonTrigger { ButtonLeft = 0, ButtonRight = 1, ButtonMiddle = 2, DontTrigger = 3 }

    //If this class is active, all objects in scene can be dragged by mouse click
    public class MouseDragger : DragController
    {
        #region Singleton

        private static MouseDragger m_instance = null;

        public static MouseDragger Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = GameObject.FindObjectOfType(typeof(MouseDragger)) as MouseDragger;
                }

                return m_instance;
            }
            protected set
            {
                m_instance = value;
            }
        }

        #endregion

        #region Unity Functions

        protected virtual void OnLevelLoaded()
        {
            if (MouseDragger.Instance != null && MouseDragger.Instance != this)
                DestroyUtils.Destroy(MouseDragger.Instance.gameObject);
            MouseDragger.Instance = this;
        }

        protected virtual void Update()
        {
            Move();
        }

        #endregion

        #region Helper Functions

        protected override void PressedTrueEffect()
        {
            SetAllBodysToKinematic(ObjectPressed);
        }

        protected override void PressedFalseEffect()
        {
            RevertAllBodysToOldValues();
        }

        //Vector2 _oldPosition = Vector2.zero;
        protected virtual void Move()
        {
            if (Pressed && ObjectPressed != null)
            {
                Vector3 v_oldPos = ObjectPressed.transform.position;
                Vector2 v_worldMousePosition = GetWorldMousePosition(ObjectPressed);
                v_oldPos.x = v_worldMousePosition.x + DeltaClick.x;
                v_oldPos.y = v_worldMousePosition.y + DeltaClick.y;
                ObjectPressed.transform.position = v_oldPos;
                //_oldPosition = _objectToMove.transform.position;
            }
        }

        private List<Rigidbody2D> GetRigidBodys(GameObject p_object)
        {
            List<Rigidbody2D> v_list = new List<Rigidbody2D>();
            if (p_object != null)
            {
                v_list.MergeList(new List<Rigidbody2D>(p_object.GetComponents<Rigidbody2D>()));
                v_list.MergeList(new List<Rigidbody2D>(p_object.GetComponentsInChildren<Rigidbody2D>()));
            }
            return v_list;
        }


        List<Rigidbody2D> _oldKinematicBodys = new List<Rigidbody2D>();
        List<Rigidbody2D> _oldNonKinematicBodys = new List<Rigidbody2D>();
        private void SetAllBodysToKinematic(GameObject p_object)
        {
            RevertAllBodysToOldValues();
            List<Rigidbody2D> v_bodys = GetRigidBodys(p_object);
            foreach (Rigidbody2D v_body in v_bodys)
            {
                if (v_body != null)
                {
                    if (!v_body.isKinematic)
                        _oldNonKinematicBodys.AddChecking(v_body);
                    else
                    {
                        if (!_oldNonKinematicBodys.Contains(v_body))
                            _oldKinematicBodys.AddChecking(v_body);
                    }

                    v_body.isKinematic = true;
                    v_body.freezeRotation = true;
                    v_body.velocity = Vector2.zero;
                    v_body.angularVelocity = 0;
                }
            }
        }

        private void RevertAllBodysToOldValues()
        {
            foreach (Rigidbody2D v_body in _oldKinematicBodys)
            {
                if (v_body != null)
                {
                    v_body.isKinematic = true;
#if UNITY_5_3_OR_NEWER
                    v_body.freezeRotation = true;
                    v_body.velocity = Vector2.zero;
                    v_body.angularVelocity = 0;
#endif
                }
            }
            foreach (Rigidbody2D v_body in _oldNonKinematicBodys)
            {
                if (v_body != null)
                {
                    v_body.isKinematic = false;
#if UNITY_5_3_OR_NEWER
                    v_body.freezeRotation = false;
                    v_body.velocity = Vector2.zero;
                    v_body.angularVelocity = 0;
#endif
                }
            }
            _oldKinematicBodys.Clear();
            _oldNonKinematicBodys.Clear();
        }

        #endregion
    }
}
