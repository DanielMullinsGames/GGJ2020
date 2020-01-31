using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kilt
{
    public class DirtyBehaviour : MonoBehaviour
    {
        #region Unity Functions

        protected virtual void OnEnable()
        {
            if (_started)
                TryApply(true);
        }

        protected bool _started = false;
        protected virtual void Start()
        {
            _started = true;
            TryApply(true);
        }

        protected virtual void Update()
        {
            TryApply();
        }

        #endregion

        #region Helper Functions

        protected bool _isDirty = false;
        public virtual void SetDirty()
        {
            _isDirty = true;
        }

        public virtual void TryApply(bool p_force = false)
        {
            if (_isDirty || p_force)
            {
                _isDirty = false;
                Apply();
            }
        }

        protected virtual void Apply()
        {
        }

        #endregion
    }
}
