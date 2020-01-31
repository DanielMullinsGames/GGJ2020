using UnityEngine;
using System.Collections;

namespace Kilt.EasyRopes2D.Examples
{
    public class BlockMaterial : DamageableBlock
    {
        #region Private Variables

        [SerializeField]
        bool m_canDestroyOnClick = true;

        #endregion

        #region Public Properties

        public bool CanDestroyOnClick
        {
            get { return m_canDestroyOnClick; }
            set { m_canDestroyOnClick = value; }
        }

        #endregion

        #region Event Implementations

        public override void OnUserClick()
        {
            if (CanDestroyOnClick)
            {
                Destroy();
            }
        }

        #endregion
    }
}
