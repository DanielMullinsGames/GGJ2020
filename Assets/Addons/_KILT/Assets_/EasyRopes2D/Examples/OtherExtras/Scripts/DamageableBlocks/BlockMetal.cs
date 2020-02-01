using UnityEngine;
using System.Collections;

namespace Kilt.EasyRopes2D.Examples
{
    public class BlockMetal : BlockMaterial
    {
        #region Unity Functions

        protected override void Awake()
        {
            base.Awake();
            Defense = 0.05f; //TOUGH DEFENSE
            Attack = 0.8f;
        }

        #endregion

        #region Event Implementations

        public override void OnUserClick()
        {
        }

        #endregion
    }
}
