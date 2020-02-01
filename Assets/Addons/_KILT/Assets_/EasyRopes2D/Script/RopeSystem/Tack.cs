using UnityEngine;
using System.Collections;

namespace Kilt.EasyRopes2D
{
    [SelectionBase]
    public class Tack : MonoBehaviour
    {
        #region Private Variables

        [SerializeField]
        bool _isKinematic = false;
        bool m_isKinematic = false;

        #endregion

        #region Public Properties

        public bool IsKinematic
        {
            get { return _isKinematic; }
            set
            {
                if (m_isKinematic == value && _isKinematic == value)
                    return;
                m_isKinematic = value;
                _isKinematic = m_isKinematic;
                int v_index = GetTackIndex();
                if (v_index >= 0)
                {
                    TacksInventory v_parent = GetComponentInParent<TacksInventory>();
                    if (v_parent != null)
                        v_parent.MarkToCheckKinematic();
                }
                if (GetComponent<Rigidbody2D>() != null) // Set Own RigidBody
                    GetComponent<Rigidbody2D>().isKinematic = true;
            }
        }

        #endregion

        #region Unity Functions

        protected virtual void Awake()
        {
            CorrectValues();
        }

        protected virtual void Update()
        {
            //if(Application.isEditor)
            CorrectValues();
        }


        #endregion

        #region Helper Methods

        protected virtual void CorrectValues()
        {
            if (m_isKinematic != _isKinematic)
                IsKinematic = _isKinematic;
        }

        public int GetTackIndex()
        {
            TacksInventory v_controller = GetComponentInParent<TacksInventory>();
            int v_index = -1;
            if (v_controller != null)
            {
                v_index = v_controller.GetIndexOfTack(this);
            }
            return v_index;
        }

        #endregion
    }
}
