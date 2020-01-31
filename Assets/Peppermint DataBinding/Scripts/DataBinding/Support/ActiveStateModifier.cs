using System;
using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding
{
    public class ActiveStateModifier
    {
        private BitArraySlim states;
        private Dictionary<GameObject, int> dictionary;

        public ActiveStateModifier()
        {
            dictionary = new Dictionary<GameObject, int>();
        }

        public bool this[GameObject go]
        {
            get
            {
                return Get(go);
            }

            set
            {
                Set(go, value);
            }
        }

        public void Add(GameObject target)
        {
            if (states != null)
            {
                throw new InvalidOperationException("ActiveStateModifier is initialized");
            }

            if (dictionary.ContainsKey(target))
            {
                // ignore duplicate
                return;
            }

            // add it
            int index = dictionary.Count;
            dictionary.Add(target, index);
        }

        public void AddRange(IEnumerable<GameObject> targets)
        {
            if (states != null)
            {
                throw new InvalidOperationException("ActiveStateModifier is initialized");
            }

            foreach (var target in targets)
            {
                if (dictionary.ContainsKey(target))
                {
                    // ignore duplicate
                    continue;
                }

                // add it
                int index = dictionary.Count;
                dictionary.Add(target, index);
            }
        }

        public void Init()
        {
            // create bit array
            states = new BitArraySlim(dictionary.Count);
        }

        public void SetAll(bool value)
        {
            // set all
            states.SetAll(value);
        }

        public void Apply()
        {
            foreach (var item in dictionary)
            {
                var go = item.Key;
                bool value = states[item.Value];

                if (go.activeSelf != value)
                {
                    go.SetActive(value);
                }
            }
        }

        public bool Get(GameObject go)
        {
            int index;
            if (!dictionary.TryGetValue(go, out index))
            {
                Debug.LogWarning("Invalid go", go);
                return false;
            }

            // get bit value
            bool value = states[index];
            return value;
        }

        public void Set(GameObject go, bool value)
        {
            int index;
            if (!dictionary.TryGetValue(go, out index))
            {
                Debug.LogWarning("Invalid go", go);
                return;
            }

            // set bit value
            states[index] = value;
        }

        public void SetRange(IEnumerable<GameObject> targets, bool value)
        {
            foreach (var target in targets)
            {
                int index;
                if (!dictionary.TryGetValue(target, out index))
                {
                    Debug.LogWarning("Invalid go", target);
                    continue;
                }

                // set bit value
                states[index] = value;
            }
        }
    }
}
