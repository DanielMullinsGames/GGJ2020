using UnityEngine;

namespace Peppermint.DataBinding
{
    public interface IViewPool
    {
        // Get view instance from ViewPool
        GameObject GetViewObject(GameObject viewPrefab);

        // Release view instance to ViewPool
        void ReleaseViewObject(GameObject view);
    }
}
