using UnityEngine;
using System.Collections;

namespace Kilt.EasyRopes2D.Examples
{
    public class BlockGlass : BlockMaterial
    {
        #region Unity Functions

        protected override void Awake()
        {
            base.Awake();
            Defense = 0.6f;
            Attack = 0.7f;
        }

        #endregion
    }
}
