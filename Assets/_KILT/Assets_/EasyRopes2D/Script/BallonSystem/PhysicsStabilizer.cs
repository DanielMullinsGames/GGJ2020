using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kilt.EasyRopes2D
{
    /// <summary>
    /// Add this to scene if you want to stabilize physics after load (Unity Apply some bizarre forces when loading scenes and that forces can affect ballon/rope counter-pulley system)
    /// </summary>
    public class PhysicsStabilizer : Singleton<PhysicsStabilizer>
    {
        bool _firstUpdate = true;
        protected virtual void OnEnable()
        {
            if(s_instance == this)
                _firstUpdate = true;
        }

        protected virtual void LateUpdate()
        {
            if (_firstUpdate && s_instance == this)
            {
                _firstUpdate = false;
                var v_bodies = Object.FindObjectsOfType<Rigidbody2D>();
                foreach (var v_body in v_bodies)
                {
                    if (v_body != null)
                        RopeInternalUtils.TryClearRigidBody2D(v_body);
                }
            }
        }

    }
}
