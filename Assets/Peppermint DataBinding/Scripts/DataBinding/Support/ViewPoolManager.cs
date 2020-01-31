using System;
using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// ViewPoolManager contains a list of ViewPool objects. You can retrieve a ViewPool with
    /// specified prefab.
    /// </summary>
    public class ViewPoolManager
    {
        private Dictionary<int, IViewPool> poolDictionary;

        #region Singleton

        public static ViewPoolManager Instance { get { return Nested.instance; } }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly ViewPoolManager instance = new ViewPoolManager();
        }

        #endregion

        private ViewPoolManager()
        {
            poolDictionary = new Dictionary<int, IViewPool>();
        }

        public IViewPool GetViewPool(GameObject prefab)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException("viewPrefab");
            }

            IViewPool pool;
            int instanceID = prefab.GetInstanceID();

            if (!poolDictionary.TryGetValue(instanceID, out pool))
            {
                Debug.LogWarningFormat("Found no viewPool for prefab {0}", prefab.name);
                return null;
            }

            return pool;
        }

        public void AddViewPool(IViewPool viewPool, GameObject viewPrefab)
        {
            if (viewPool == null)
            {
                throw new ArgumentNullException("viewPool");
            }

            if (viewPrefab == null)
            {
                throw new ArgumentNullException("viewPrefab");
            }

            int instanceID = viewPrefab.GetInstanceID();

            // check if prefab is already added
            if (poolDictionary.ContainsKey(instanceID))
            {
                Debug.LogWarningFormat("ViewPool for viewPrefab {0} is already added.", viewPrefab.name);
                return;
            }

            // add pool
            poolDictionary.Add(instanceID, viewPool);
        }

        public void RemoveViewPool(IViewPool viewPool, GameObject viewPrefab)
        {
            if (viewPool == null)
            {
                throw new ArgumentNullException("viewPool");
            }

            int instanceID = 0;
            if (viewPrefab == null)
            {
                // try search instanceID
                foreach (var kvp in poolDictionary)
                {
                    if (kvp.Value == viewPool)
                    {
                        instanceID = kvp.Key;
                        break;
                    }
                }

                if (instanceID == 0)
                {
                    Debug.LogWarning("ViewPrefab is null");
                    return;
                }
            }
            else
            {
                instanceID = viewPrefab.GetInstanceID();
            }

            // check if prefab is valid
            if (!poolDictionary.ContainsKey(instanceID))
            {
                Debug.LogWarningFormat("Unknown ViewPool for viewPrefab {0}", viewPrefab.name);
                return;
            }

            // remove it
            poolDictionary.Remove(instanceID);
        }

        /// <summary>
        /// Create a new dynamic pool which handles object pooling for specified view templates.
        /// </summary>
        /// <param name="viewTemplates">The target view templates</param>
        /// <returns>The created dynamic view pool</returns>
        public IViewPool CreateDynamicPool(IEnumerable<GameObject> viewTemplates)
        {
            var dictionary = new Dictionary<int, IViewPool>();

            // check view templates
            foreach (var viewPrefab in viewTemplates)
            {
                int instanceID = viewPrefab.GetInstanceID();
                IViewPool viewPool;

                if (!poolDictionary.TryGetValue(instanceID, out viewPool))
                {
                    Debug.LogWarningFormat("No viewPool for prefab {0}", viewPrefab.name);
                    return null;
                }

                // add to dictionary
                dictionary.Add(instanceID, viewPool);
            }

            // create dynamic pool
            var pool = new DynamicPool(dictionary);
            return pool;
        }

        public void Clear()
        {
            poolDictionary.Clear();
        }
    }
}
