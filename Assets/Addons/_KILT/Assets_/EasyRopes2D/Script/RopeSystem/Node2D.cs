using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kilt.Extensions;

namespace Kilt.EasyRopes2D
{
    public class Node2D : MonoBehaviour
    {
        #region Public Properties

        Rope2D _ropeParent = null;
        public Rope2D RopeParent
        {
            get
            {
                if (_ropeParent == null)
                    _ropeParent = GetComponentInParent<Rope2D>();
                return _ropeParent;
            }
        }

        #endregion

        #region Protected Properties

        bool _needApplyIgnore = true;
        protected virtual bool NeedApplyIgnore
        {
            get
            {
                return _needApplyIgnore;
            }
            set
            {
                if (_needApplyIgnore == value)
                    return;
                _needApplyIgnore = value;
            }
        }

        bool _started = false;
        protected virtual bool Started
        {
            get
            {
                return _started;
            }
            set
            {
                if (_started == value)
                    return;
                _started = value;
            }
        }

        #endregion

        #region Unity Functions


        protected virtual void OnEnable()
        {
            if (Started)
                ApplyIgnoreRopeCollision();
        }

        protected virtual void Start()
        {
            if (!Started)
            {
                Started = true;
                ApplyIgnoreRopeCollision();
            }
        }

        protected virtual void OnDisable()
        {
            NeedApplyIgnore = true;
        }

        #endregion

        #region Helper Functions

        public virtual void ApplyIgnoreRopeCollision(bool p_force = false)
        {
            StopCoroutine("RequestApplyIgnoreRopeCollision");
            StartCoroutine(RequestApplyIgnoreRopeCollision(p_force));
        }

        protected virtual IEnumerator RequestApplyIgnoreRopeCollision(bool p_force)
        {
            if (RopeParent != null && (NeedApplyIgnore || p_force))
            {
                NeedApplyIgnore = false;
                List<Collider2D> v_selfColliders = new List<Collider2D>(this.GetNonMarkedComponentsInChildren<Collider2D>());
                v_selfColliders.MergeList(new List<Collider2D>(this.GetNonMarkedComponentsInParent<Collider2D>(false, false)));
                List<GameObject> v_nodes = new List<GameObject>(RopeParent.Nodes);
                v_nodes.MergeList(RopeParent.GetPluggedObjectsInRope());
                foreach (GameObject v_object in v_nodes)
                {
                    if (v_object != null)
                    {
                        yield return null; //Prevent Lags waiting one cycle to apply
                        List<Collider2D> v_otherColliders = new List<Collider2D>(v_object.GetNonMarkedComponentsInChildren<Collider2D>());
                        v_otherColliders.MergeList(new List<Collider2D>(v_object.GetNonMarkedComponentsInParent<Collider2D>(false, false)));
                        foreach (Collider2D v_selfCollider in v_selfColliders)
                        {
                            if (v_selfCollider != null && v_selfCollider.GetInstanceID() != 0)
                            {
                                foreach (Collider2D v_otherCollider in v_otherColliders)
                                {
                                    if (v_otherCollider != null && v_otherCollider.GetInstanceID() != 0)
                                        Physics2D.IgnoreCollision(v_selfCollider, v_otherCollider);
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool Cut()
        {
            if (RopeParent != null)
            {
                return RopeParent.CutNode(this.gameObject);
            }
            else
                DestroyUtils.DestroyImmediate(this.gameObject);

            return true;
        }

        #endregion
    }
}
