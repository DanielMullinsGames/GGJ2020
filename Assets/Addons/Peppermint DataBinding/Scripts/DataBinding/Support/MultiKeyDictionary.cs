using System.Collections.Generic;

namespace Peppermint.DataBinding
{
    // Dictionary that supports multi keys
    public class MultiKeyDictionary<T1, T2, T3> : Dictionary<T1, Dictionary<T2, T3>>
    {
        public bool ContainsKey(T1 key1, T2 key2)
        {
            Dictionary<T2, T3> dictionary;
            if (!TryGetValue(key1, out dictionary))
            {
                return false;
            }

            var result = dictionary.ContainsKey(key2);
            return result;
        }

        public bool TryGetValue(T1 key1, T2 key2, out T3 value)
        {
            Dictionary<T2, T3> dictionary;
            if (!TryGetValue(key1, out dictionary))
            {
                value = default(T3);
                return false;
            }

            var result = dictionary.TryGetValue(key2, out value);
            return result;
        }

        public void Add(T1 key1, T2 key2, T3 value)
        {
            Dictionary<T2, T3> dictionary;
            if (!TryGetValue(key1, out dictionary))
            {
                // add new item
                dictionary = new Dictionary<T2, T3>();
                Add(key1, dictionary);
            }

            // add to dictionary
            dictionary.Add(key2, value);
        }

        public bool Remove(T1 key1, T2 key2)
        {
            Dictionary<T2, T3> dictionary;
            if (!TryGetValue(key1, out dictionary))
            {
                // key1 not found
                return false;
            }

            var result = dictionary.Remove(key2);
            return result;
        }
    }
}
