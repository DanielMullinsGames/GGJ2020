using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kilt.Extensions;

namespace Kilt.EasyRopes2D
{
    [ExecuteInEditMode]
    public class TacksInventory : MonoBehaviour
    {
        #region Private Variables

        List<Tack> m_tacksInObject = new List<Tack>();

        bool _needCheckKinematic = true;
        bool _needFillWithChildrenTacks = true;

        #endregion

        #region Public Properties

        public List<Tack> TacksInObject
        {
            get
            {
                if (m_tacksInObject == null)
                    m_tacksInObject = new List<Tack>();
                return m_tacksInObject;
            }
            protected set
            {
                if (m_tacksInObject == value)
                    return;
                m_tacksInObject = value;
            }
        }

        #endregion

        #region Unity Functions

        protected virtual void Awake()
        {
            FillWithChildrenTacks(true);
        }

        protected virtual void Start()
        {
            FillWithChildrenTacks(false);
            CheckKinematic(false);
        }

        protected virtual void Update()
        {
            FillWithChildrenTacks(false);
            CheckKinematic(false);
        }

        #endregion

        #region Add/Remove

        public void AddTackInList(Tack p_tack)
        {
            if (p_tack != null)
            {
                TacksInObject.AddChecking(p_tack);
                _needCheckKinematic = true;
            }
        }

        public void RemoveTackFromList(Tack p_tack)
        {
            if (p_tack != null)
            {
                TacksInObject.RemoveChecking(p_tack);
                _needCheckKinematic = true;
            }
        }

        public void FillWithChildrenTacks(bool p_force = false)
        {
            if (_needFillWithChildrenTacks || p_force)
            {
                _needFillWithChildrenTacks = false;
                Tack[] v_newTacksInObject = GetComponentsInChildren<Tack>();
                TacksInObject = new List<Tack>(v_newTacksInObject);
                _needCheckKinematic = true;
            }
        }

        #endregion

        #region Helper Functions

        public void MarkToCheckKinematic()
        {
            _needCheckKinematic = true;
        }

        public void CheckKinematic(bool p_force = false)
        {
            if (_needCheckKinematic || p_force)
            {
                _needCheckKinematic = false;
                bool v_setToKinematic = false;
                Rigidbody2D v_body = this.GetNonMarkedComponentInChildren<Rigidbody2D>();
                if (v_body == null)
                    v_body = this.GetNonMarkedComponentInParent<Rigidbody2D>();
                foreach (Tack v_tack in TacksInObject)
                {
                    if (v_tack != null)
                    {
                        if (v_tack.IsKinematic)
                        {
                            v_setToKinematic = true;
                            if (v_tack.GetComponent<Rigidbody2D>() != null)
                                v_tack.GetComponent<Rigidbody2D>().isKinematic = true;
                        }
                    }
                    else
                        _needFillWithChildrenTacks = true;
                }
                if (v_body != null)
                    v_body.isKinematic = v_setToKinematic;
            }

        }

        public GameObject GetTackObjectWithIndex(int p_index, bool p_returnDefaultWhenOutOfBounds = true)
        {
            Tack v_tack = GetTackWithIndex(p_index, p_returnDefaultWhenOutOfBounds);
            GameObject v_tackObject = v_tack != null ? v_tack.gameObject : null;
            if (v_tackObject == null)
                v_tackObject = this.gameObject;
            return v_tackObject;
        }

        public Tack GetTackWithIndex(int p_index, bool p_returnDefaultWhenOutOfBounds = true)
        {
            Tack v_tack = null;
            if (p_index >= 0 && p_index < TacksInObject.Count)
            {
                v_tack = TacksInObject[p_index];
            }
            if (v_tack == null && p_returnDefaultWhenOutOfBounds)
            {
                foreach (Tack v_tackInList in TacksInObject)
                {
                    v_tack = v_tackInList;
                    if (v_tack != null)
                        break;
                }
            }
            return v_tack;
        }

        public int GetIndexOfTack(Tack p_tack, bool p_addThisTackIfNotAdded = true)
        {
            int v_returnIndex = -1;
            if (p_tack != null)
            {
                for (int i = 0; i < TacksInObject.Count; i++)
                {
                    Tack v_tackInObject = TacksInObject[i];
                    if (p_tack == v_tackInObject)
                    {
                        v_returnIndex = i;
                        break;
                    }
                }
                if (v_returnIndex == -1 && p_addThisTackIfNotAdded)
                {
                    TacksInObject.Add(p_tack);
                    v_returnIndex = TacksInObject.Count - 1;
                }
            }
            return v_returnIndex;
        }

        #endregion
    }
}
