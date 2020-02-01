using System;
using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Dynamic pool that support multiple prefabs. It wraps the Get/ReleaseViewObject to its
    /// internal view pools.
    /// </summary>
    public class DynamicPool : IViewPool
    {
        private Dictionary<int, IViewPool> viewPoolDictionary;
        private Dictionary<int, IViewPool> instanceDictionary;

        public DynamicPool(Dictionary<int, IViewPool> viewPoolDictionary)
        {
            this.viewPoolDictionary = viewPoolDictionary;
            instanceDictionary = new Dictionary<int, IViewPool>();
        }

        public GameObject GetViewObject(GameObject viewPrefab)
        {
            if (viewPrefab == null)
            {
                throw new ArgumentNullException("viewPrefab");
            }

            int prefabInstanceID = viewPrefab.GetInstanceID();

            // get view pool
            IViewPool viewPool;
            if (!viewPoolDictionary.TryGetValue(prefabInstanceID, out viewPool))
            {
                return null;
            }

            // get view from specified view pool
            var view = viewPool.GetViewObject(viewPrefab);

            // add to dictionary
            int viewInstanceID = view.GetInstanceID();
            instanceDictionary.Add(viewInstanceID, viewPool);

            return view;
        }

        public void ReleaseViewObject(GameObject view)
        {
            int viewInstanceID = view.GetInstanceID();

            // get view pool
            IViewPool viewPool;
            if (!instanceDictionary.TryGetValue(viewInstanceID, out viewPool))
            {
                Debug.LogWarningFormat("Unknown view instance {0}", view.name);
                return;
            }

            // remove from dictionary
            instanceDictionary.Remove(viewInstanceID);

            // call view pool to release view
            viewPool.ReleaseViewObject(view);
        }
    }
}
