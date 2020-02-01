using UnityEngine;
using System.Collections;
using Kilt.Extensions;

namespace Kilt.EasyRopes2D
{
    [SelectionBase]
    public class TackRope : Rope2D
    {
        #region Private Variables

        [SerializeField]
        int m_tackAIndex = 0;
        [SerializeField]
        int m_tackBIndex = 0;

        #endregion

        #region Public Properties
        bool _tackUpdated = true;

        public int TackAIndex
        {
            get
            {
                return m_tackAIndex;
            }
            set
            {
                if (m_tackAIndex == value)
                    return;
                RecordObject();
                m_tackAIndex = value;
                _tackUpdated = true;
                NeedUpdateRope = true;
            }
        }

        public int TackBIndex
        {
            get
            {
                return m_tackBIndex;
            }
            set
            {
                if (m_tackBIndex == value)
                    return;
                RecordObject();
                m_tackBIndex = value;
                _tackUpdated = true;
                NeedUpdateRope = true;
            }
        }

        #endregion

        #region Overriden Properties

        public override GameObject ObjectA
        {
            get { return base.ObjectA; }
            set
            {
                base.ObjectA = value;
                if (value != null)
                {
                    Tack v_tack = value.GetComponent<Tack>();
                    if (v_tack != null)
                        TackAIndex = v_tack.GetTackIndex();
                }
                _tackUpdated = true;
            }
        }

        public override GameObject ObjectB
        {
            get { return base.ObjectB; }
            set
            {
                base.ObjectB = value;
                if (value != null)
                {
                    Tack v_tack = value.GetComponent<Tack>();
                    if (v_tack != null)
                        TackBIndex = v_tack.GetTackIndex();
                }
                _tackUpdated = true;
            }
        }

        #endregion

        #region Overriden Functions

        protected override void CorrectValues()
        {
            base.CorrectValues();
            if (_tackUpdated)
                UpdateTackObjectsBeforeRopeCreated();
        }

        public override void CreateRope(bool p_clearNodes = true)
        {
            UpdateTackObjectsBeforeRopeCreated();
            base.CreateRope(p_clearNodes);
        }

        #endregion

        #region Helper Functions

        protected virtual void UpdateTackObjectsBeforeRopeCreated()
        {
            GameObject v_attachedObjectA = GetAttachedObjectA();
            GameObject v_attachedObjectB = GetAttachedObjectB();

            if (v_attachedObjectA != null)
            {
                TacksInventory v_tacksControllerA = v_attachedObjectA.GetNonMarkedComponentInChildren<TacksInventory>();
                if (v_tacksControllerA != null)
                {
                    ObjectA = v_tacksControllerA.GetTackObjectWithIndex(TackAIndex);
                }
            }
            if (v_attachedObjectB != null)
            {
                TacksInventory v_tacksControllerB = v_attachedObjectB.GetNonMarkedComponentInChildren<TacksInventory>();
                if (v_tacksControllerB != null)
                {
                    ObjectB = v_tacksControllerB.GetTackObjectWithIndex(TackBIndex);
                }
            }
            _tackUpdated = false;
        }

        #endregion
    }
}
