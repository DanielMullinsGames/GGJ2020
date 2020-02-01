using UnityEngine;
using System.Collections;

namespace Kilt.EasyRopes2D.Examples
{
    public class BlockWood : BlockMaterial
    {
        #region Unity Functions

        protected override void Awake()
        {
            base.Awake();
            Defense = 0.7f;
            Attack = 1f;
        }

        #endregion
    }
}
