using System;
using System.Collections;
using System.Collections.Generic;

namespace Peppermint.DataBinding
{
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private const string CountString = "Count";

        private Dictionary<TKey, TValue> dictionary;

        public bool IsNotifying { get; set; }

        public ObservableDictionary()
        {
            IsNotifying = true;
            dictionary = new Dictionary<TKey, TValue>();
        }

        public void AddRange(IDictionary<TKey, TValue> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (items.Count == 0)
            {
                return;
            }

            if (dictionary.Count > 0)
            {
                foreach (var k in items.Keys)
                {
                    if (dictionary.ContainsKey(k))
                    {
                        throw new ArgumentException("An item with the same key has already been added.");
                    }
                }
            }

            // add all items
            foreach (var kvp in items)
            {
                dictionary.Add(kvp.Key, kvp.Value);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items));
        }

        public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        public void NotifyCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(e);
        }

        #region IDictionary<TKey, TValue> Members

        public ICollection<TKey> Keys
        {
            get { return dictionary.Keys; }
        }

        public ICollection<TValue> Values
        {
            get { return dictionary.Values; }
        }

        public TValue this[TKey key]
        {
            get
            {
                return dictionary[key];
            }
            set
            {
                Insert(key, value, false);
            }
        }

        public void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            TValue value;
            dictionary.TryGetValue(key, out value);

            var removed = dictionary.Remove(key);
            if (removed)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value)));
            }

            return removed;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        #endregion

        #region ICollection<KeyValuePair<TKey, TValue>> Members

        public int Count
        {
            get { return dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((IDictionary<TKey, TValue>)dictionary).IsReadOnly; }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Insert(item.Key, item.Value, true);
        }

        public void Clear()
        {
            if (dictionary.Count > 0)
            {
                dictionary.Clear();
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((IDictionary<TKey, TValue>)dictionary).Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary<TKey, TValue>)dictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return dictionary.Remove(item.Key);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey, TValue>> Members

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        #endregion

        #region INotifyCollectionChanged Members

        public event Action<object, NotifyCollectionChangedEventArgs> CollectionChanged;

        #endregion

        #region INotifyPropertyChanged Members

        public event Action<object, string> PropertyChanged;

        #endregion

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (IsNotifying && CollectionChanged != null)
            {
                CollectionChanged.Invoke(this, e);
            }

            OnPropertyChanged();
        }

        private void OnPropertyChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, CountString);
            }
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            TValue item;
            if (dictionary.TryGetValue(key, out item))
            {
                if (add)
                {
                    throw new ArgumentException("An item with the same key has already been added.");
                }

                if (Equals(item, value))
                {
                    return;
                }

                dictionary[key] = value;

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(key, value), new KeyValuePair<TKey, TValue>(key, item)));
            }
            else
            {
                dictionary[key] = value;

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));
            }
        }
    }
}
