using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kilt.Extensions;

namespace Kilt.EasyRopes2D
{
    public class RopesAttached : MonoBehaviour
    {
        #region Events

        public System.Action<Rope2D> OnRopePlugged;
        public System.Action<Rope2D> OnRopeUnplugged;
        public System.Action<Rope2D> OnIndirectRopeDestroyed;
        public System.Action<Rope2D> OnIndirectRopeCreated;

        public System.Action<Rope2D> OnIndirectRopeBreak;
        public System.Action<Rope2D, int> OnRopeBreak;

        #endregion

        #region Private Variables

        List<Rope2D> m_pluggedRopes = new List<Rope2D>();
        List<Rope2D> m_indirectRopes = new List<Rope2D>();

        #endregion

        #region Public Variables

        public List<Rope2D> PluggedRopes
        {
            get
            {
                if (m_pluggedRopes == null)
                {
                    m_pluggedRopes = new List<Rope2D>();
                }
                return m_pluggedRopes;
            }
            protected set
            {
                m_pluggedRopes = value;
            }
        }

        public List<Rope2D> IndirectRopes
        {
            get
            {
                if (_needRecalcIndirectRopes)
                {
                    _needRecalcIndirectRopes = false;
                    m_indirectRopes = GetAllIndirectRopes(false);
                }
                if (m_indirectRopes == null)
                {
                    m_indirectRopes = new List<Rope2D>();
                }
                return m_indirectRopes;
            }
            protected set
            {
                m_indirectRopes = value;
            }
        }

        #endregion

        #region Unity Functions

        bool _needRecalcIndirectRopes = false;
        protected virtual void LateUpdate()
        {
            if (_needRecalcIndirectRopes)
            {
                _needRecalcIndirectRopes = false;
                IndirectRopes = GetAllIndirectRopes(false);
            }
        }

        #endregion

        #region Event Receivers

        protected virtual void HandleOnRopeDestroy(Rope2D p_rope)
        {
            RemovePluggedRopeFromList(p_rope);
        }

        protected virtual void HandleOnRopeBreak(Rope2D p_rope, int p_brokenIndex)
        {
            if (p_rope != null && p_rope.RopeBreakAction == RopeBreakActionEnum.DestroySelf)
                RemovePluggedRopeFromList(p_rope);

            if (OnRopeBreak != null)
                OnRopeBreak(p_rope, p_brokenIndex);

        }

        #endregion

        #region Object Finder

        public List<GameObject> GetAllOtherAttachedObjectsInPluggedRopes()
        {
            List<GameObject> v_othersPlugged = new List<GameObject>();
            foreach (Rope2D v_rope in PluggedRopes)
            {
                if (v_rope != null)
                {
                    GameObject v_objectA = v_rope.GetAttachedObjectA();
                    GameObject v_objectB = v_rope.GetAttachedObjectB();
                    if (v_objectA != null && v_objectA != this.gameObject)
                    {
                        v_othersPlugged.AddChecking(v_objectA);
                    }
                    else if (v_objectB != null && v_objectB != this.gameObject)
                    {
                        v_othersPlugged.AddChecking(v_objectB);
                    }
                }
            }
            return v_othersPlugged;
        }

        //Find ropes pluggued in objects atacched to one PluggedRope
        public List<Rope2D> GetAllIndirectRopes(bool p_includePluggedRopes = true)
        {
            List<Rope2D> v_indirectRopes = new List<Rope2D>(PluggedRopes);
            List<GameObject> v_indirectPluggedObjects = GetAllOtherAttachedObjectsInPluggedRopes();
            v_indirectPluggedObjects.AddChecking(this.gameObject);
            for (int i = 0; i < v_indirectPluggedObjects.Count; i++)
            {
                GameObject v_otherObject = v_indirectPluggedObjects[i];
                if (v_otherObject != null && v_otherObject != this.gameObject)
                {
                    RopesAttached v_attachedScript = v_otherObject.GetComponent<RopesAttached>();
                    if (v_attachedScript != null)
                    {
                        v_indirectRopes.MergeList(v_attachedScript.PluggedRopes);
                        v_indirectPluggedObjects.MergeList(v_attachedScript.GetAllOtherAttachedObjectsInPluggedRopes());
                    }
                }
            }
            if (!p_includePluggedRopes)
            {
                foreach (Rope2D v_rope in PluggedRopes)
                    v_indirectRopes.RemoveChecking(v_rope);
            }
            return v_indirectRopes;
        }

        public List<GameObject> GetAllIndirectObjects(bool p_includeOtherPluggedObjects = true)
        {
            /*List<GameObject> v_finalList = new List<GameObject>();
            foreach(RopeBehaviour2D v_rope in IndirectRopes)
            {
                if(v_rope != null)
                    v_finalList.MergeList(v_rope.GetPluggedObjectsInRope());
            }

            //Remove Or Add Plugged Objects
            foreach(RopeBehaviour2D v_rope in PluggedRopes)
            {
                if(v_rope != null)
                {
                    if(!p_includeOtherPluggedObjects)
                    {
                        foreach(GameObject v_object in v_rope.GetPluggedObjectsInRope())
                            v_finalList.RemoveChecking(v_object);
                    }
                    else
                        v_finalList.MergeList(v_rope.GetPluggedObjectsInRope());
                }
            }
            return v_finalList;*/


            List<GameObject> v_indirectPluggedObjects = GetAllOtherAttachedObjectsInPluggedRopes();
            List<GameObject> v_directObjects = v_indirectPluggedObjects.CloneList();
            v_indirectPluggedObjects.AddChecking(this.gameObject);
            for (int i = 0; i < v_indirectPluggedObjects.Count; i++)
            {
                GameObject v_otherObject = v_indirectPluggedObjects[i];
                if (v_otherObject != null && v_otherObject != this.gameObject)
                {
                    RopesAttached v_attachedScript = v_otherObject.GetComponent<RopesAttached>();
                    if (v_attachedScript != null)
                        v_indirectPluggedObjects.MergeList(v_attachedScript.GetAllOtherAttachedObjectsInPluggedRopes());
                }
            }
            if (!p_includeOtherPluggedObjects)
            {
                foreach (GameObject v_object in v_directObjects)
                    v_indirectPluggedObjects.RemoveChecking(v_object);
            }
            //Aways Remove Self
            v_indirectPluggedObjects.RemoveChecking(this.gameObject);
            return v_indirectPluggedObjects;
        }

        #endregion

        #region Mass Helper

        public float GetSelfMass(bool p_includeChildrens = true, bool p_multiplyByGravityScale = true)
        {
            return RopeInternalUtils.GetObjectMass(this.gameObject, p_includeChildrens, p_multiplyByGravityScale);
        }

        public virtual float GetSumOfOtherObjectsMass(bool p_includeChildrens = true, bool p_multiplyByGravityScale = true)
        {
            float v_sumOfOtherObjectsMass = 0;
            List<GameObject> v_otherObjects = GetAllOtherAttachedObjectsInPluggedRopes();
            foreach (GameObject v_object in v_otherObjects)
            {
                v_sumOfOtherObjectsMass += RopeInternalUtils.GetObjectMass(v_object, p_includeChildrens, p_multiplyByGravityScale);
            }
            return v_sumOfOtherObjectsMass;
        }

        public float GetPluggedRopesMass(bool p_multiplyByGravityScale = true)
        {
            float v_ropesMassSum = 0;
            foreach (Rope2D v_rope in PluggedRopes)
            {
                if (v_rope != null)
                    v_ropesMassSum += v_rope.GetMassAttachedToObject(this.gameObject, p_multiplyByGravityScale);
            }
            return v_ropesMassSum;
        }

        //More Imprecise than GetPluggedRopesMass because detect only full rope mass
        public float GetIndirectRopesMass(bool p_includePluggedRopes = true, bool p_multiplyByGravityScale = true)
        {
            List<Rope2D> v_indirectRopesList = GetAllIndirectRopes(p_includePluggedRopes);
            float v_ropesMassSum = 0;
            foreach (Rope2D v_rope in v_indirectRopesList)
            {
                if (v_rope != null)
                    v_ropesMassSum += v_rope.GetRopeMass(p_multiplyByGravityScale);
            }
            return v_ropesMassSum;
        }

        public float GetSumOfIndirectObjectsMass(bool p_includeOtherPluggedObjects = true, bool p_includeChildrens = true, bool p_multiplyByGravityScale = true)
        {
            List<GameObject> v_indirectOtherObjectsList = GetAllIndirectObjects(p_includeOtherPluggedObjects);
            float v_massSum = 0;
            foreach (GameObject v_object in v_indirectOtherObjectsList)
            {
                v_massSum += RopeInternalUtils.GetObjectMass(v_object, p_includeChildrens, p_multiplyByGravityScale);
            }
            return v_massSum;
        }

        #endregion

        #region Message Receiver (From RopeBehaviour2D)

        protected void IndirectRopeBroke(Rope2D p_destroyedRope)
        {
            _needRecalcIndirectRopes = true;
            if (OnIndirectRopeBreak != null)
                OnIndirectRopeBreak(p_destroyedRope);
        }

        protected void IndirectRopeDestroyed(Rope2D p_destroyedRope)
        {
            _needRecalcIndirectRopes = true;
            if (OnIndirectRopeDestroyed != null)
                OnIndirectRopeDestroyed(p_destroyedRope);
        }

        protected void IndirectRopeCreated(Rope2D p_rope)
        {
            _needRecalcIndirectRopes = true;
            if (OnIndirectRopeCreated != null)
                OnIndirectRopeCreated(p_rope);
        }

        #endregion

        #region Add/Remove Rope

        public virtual void AddPluggedRopeInList(Rope2D p_rope)
        {
            if (p_rope != null && !PluggedRopes.Contains(p_rope))
            {
                PluggedRopes.Add(p_rope);
                p_rope.OnRopeDestroyed += HandleOnRopeDestroy;
                p_rope.OnRopeBreak += HandleOnRopeBreak;
                _needRecalcIndirectRopes = true;
                if (OnRopePlugged != null)
                    OnRopePlugged(p_rope);
            }
        }

        public virtual void RemovePluggedRopeFromList(Rope2D p_rope)
        {
            //Remove nulls too
            List<Rope2D> v_pluggedRopesAux = new List<Rope2D>();
            foreach (Rope2D v_rope in PluggedRopes)
            {
                if (v_rope != p_rope && v_rope != null)
                {
                    v_pluggedRopesAux.Add(v_rope);
                }
            }
            PluggedRopes = v_pluggedRopesAux;
            if (p_rope != null)
            {
                p_rope.OnRopeDestroyed -= HandleOnRopeDestroy;
                p_rope.OnRopeBreak -= HandleOnRopeBreak;
                _needRecalcIndirectRopes = true;
                if (OnRopeUnplugged != null)
                    OnRopeUnplugged(p_rope);
            }
        }

        #endregion
    }
}
