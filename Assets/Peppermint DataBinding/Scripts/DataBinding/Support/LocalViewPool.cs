using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Local view pool that support multiple prefabs.
    /// </summary>
    public class LocalViewPool : MonoBehaviour, IViewPool
    {
        private Dictionary<GameObject, Stack<GameObject>> poolDictionary;
        private Dictionary<GameObject, GameObject> instanceDictionary;

        void Awake()
        {
            poolDictionary = new Dictionary<GameObject, Stack<GameObject>>();
            instanceDictionary = new Dictionary<GameObject, GameObject>();
        }

        void OnDestroy()
        {
            foreach (var kvp in poolDictionary)
            {
                var stack = kvp.Value;
                foreach (var item in stack)
                {
                    GameObject.Destroy(item);
                }
            }

            poolDictionary.Clear();
            instanceDictionary.Clear();
        }

        public GameObject GetViewObject(GameObject prefab)
        {
            Stack<GameObject> freeObjects;
            if (!poolDictionary.TryGetValue(prefab, out freeObjects))
            {
                // add it
                freeObjects = new Stack<GameObject>();
                poolDictionary.Add(prefab, freeObjects);
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
                viewObject = GameObject.Instantiate<GameObject>(prefab);

                // add to instance dictionary
                instanceDictionary.Add(viewObject, prefab);
            }

            SetInstanceName(prefab, viewObject);

            return viewObject;
        }

        public void ReleaseViewObject(GameObject viewObject)
        {
            // get prefab
            GameObject prefab;
            if (!instanceDictionary.TryGetValue(viewObject, out prefab))
            {
                Debug.LogError("Unknown instance", viewObject);
                return;
            }

            // attach as child
            viewObject.SetActive(false);
            viewObject.transform.SetParent(transform, false);

            ResetName(prefab, viewObject);

            // add it to pool
            var freeObjects = poolDictionary[prefab];
            freeObjects.Push(viewObject);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void SetInstanceName(GameObject viewPrefab, GameObject view)
        {
            view.name = viewPrefab.name;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void ResetName(GameObject viewPrefab, GameObject view)
        {
            view.name = string.Format("{0}(Cached)", viewPrefab.name);
        }

        public static IViewPool Create(Transform parent)
        {
            // create new GO
            var go = new GameObject("LocalViewPool");

            // add component
            var component = go.AddComponent<LocalViewPool>();

            // attach as child
            go.transform.SetParent(parent, false);

            return component;
        }
    }
}
