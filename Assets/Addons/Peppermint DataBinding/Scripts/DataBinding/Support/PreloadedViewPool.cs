using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Preloaded view pool.
    ///
    /// Preloaded view pool creates a bunch of objects and caches them on awake. When an object is
    /// needed, it just return the cached object instead of instantiating it. When an object is no
    /// longer needed, it will recycle this object back to the pool rather than destroy it.
    ///
    /// The "preloadCount" decides the number of objects created on awake. The "poolCapacity" limit
    /// the number of objects in the pool, if the number exceed the capacity, it will be destroyed.
    ///
    /// Cached objects will be attached as child.
    /// </summary>
    public class PreloadedViewPool : MonoBehaviour, IViewPool
    {
        public GameObject viewPrefab;
        public int preloadCount;
        public int poolCapacity;

        private Stack<GameObject> freeObjects;

        public GameObject ViewPrefab
        {
            get { return viewPrefab; }
        }

        void Awake()
        {
            freeObjects = new Stack<GameObject>();

            for (int i = 0; i < preloadCount; i++)
            {
                var viewObject = Instantiate<GameObject>(viewPrefab);
                viewObject.SetActive(false);

                // attach as child
                viewObject.transform.SetParent(transform, false);

                ResetName(viewObject);

                // add to pool
                freeObjects.Push(viewObject);
            }

            // add it to manager
            ViewPoolManager.Instance.AddViewPool(this, viewPrefab);
        }

        void OnDestroy()
        {
            // remove it from manager
            ViewPoolManager.Instance.RemoveViewPool(this, viewPrefab);
        }

        public GameObject GetViewObject(GameObject prefab)
        {
            if (this == null)
            {
                // just return if current object is destroyed
                return null;
            }

            if (prefab != viewPrefab)
            {
                Debug.LogWarning("Invalid view prefab");
                return null;
            }

            GameObject viewObject = null;

            if (freeObjects.Count > 0)
            {
                // use pool
                viewObject = freeObjects.Pop();
                viewObject.transform.SetParent(null, false);
            }
            else
            {
                // create new view
                viewObject = GameObject.Instantiate<GameObject>(viewPrefab);
            }

            SetInstanceName(viewObject);

            return viewObject;
        }

        public void ReleaseViewObject(GameObject viewObject)
        {
            if (this == null)
            {
                // just return if current object is destroyed
                return;
            }

            if (freeObjects.Count >= poolCapacity)
            {
                // just destroy it
                GameObject.Destroy(viewObject);
                return;
            }

            ResetName(viewObject);

            // add it to pool
            viewObject.SetActive(false);
            viewObject.transform.SetParent(transform, false);
            freeObjects.Push(viewObject);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void SetInstanceName(GameObject view)
        {
            view.name = viewPrefab.name;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void ResetName(GameObject view)
        {
            view.name = string.Format("{0}(Cached)", viewPrefab.name);
        }
    }
}
