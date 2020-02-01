using UnityEngine;
using System.Collections;

namespace Kilt.EasyRopes2D.Examples
{
    public class BlockStone : BlockMaterial
    {
        #region Unity Functions

        protected override void Awake()
        {
            base.Awake();
            Defense = 0.8f;
            Attack = 0.7f;
        }

        #endregion
    }
}
