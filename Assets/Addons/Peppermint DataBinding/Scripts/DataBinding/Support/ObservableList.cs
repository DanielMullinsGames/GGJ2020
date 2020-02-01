using System;
using System.Collections;
using System.Collections.Generic;

namespace Peppermint.DataBinding
{
    public class ObservableList<T> : IList, IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private const string CountString = "Count";

        private List<T> list;

        public bool IsNotifying { get; set; }

        public ObservableList()
        {
            IsNotifying = true;
            list = new List<T>();
        }

        public ObservableList(int capacity)
        {
            IsNotifying = true;
            list = new List<T>(capacity);
        }

        public ObservableList(IEnumerable<T> collection)
        {
            IsNotifying = true;
            list = new List<T>(collection);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            list.AddRange(collection);

            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection);
            OnCollectionChanged(e);
        }

        public void RemoveRange(int index, int count)
        {
            List<T> oldItems = list.GetRange(index, count);
            list.RemoveRange(index, count);

            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems);
            OnCollectionChanged(e);
        }

        public List<T> GetRange(int index, int count)
        {
            return list.GetRange(index, count);
        }

        public List<T>.Enumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public void NotifyCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(e);
        }

        #region IList

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection)list).SyncRoot;
            }
        }

        int IList.Add(object value)
        {
            try
            {
                Add((T)value);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException();
            }

            return Count - 1;
        }

        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
            {
                return Contains((T)value);
            }

            return false;
        }

        int IList.IndexOf(object value)
        {
            if (IsCompatibleObject(value))
            {
                return IndexOf((T)value);
            }

            return -1;
        }

        void IList.Insert(int index, object value)
        {
            try
            {
                Insert(index, (T)value);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException();
            }
        }

        void IList.Remove(object value)
        {
            if (IsCompatibleObject(value))
            {
                Remove((T)value);
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            T[] tArray = array as T[];
            if (tArray != null)
            {
                list.CopyTo(tArray, index);
            }
            else
            {
                Type targetType = array.GetType().GetElementType();
                Type sourceType = typeof(T);
                if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType)))
                {
                    throw new ArgumentException("Invalid array type");
                }

                object[] objects = array as object[];
                if (objects == null)
                {
                    throw new ArgumentException("Invalid array type");
                }

                int count = list.Count;
                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        objects[index++] = list[i];
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException("Invalid array type");
                }
            }
        }

        object IList.this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                try
                {
                    this[index] = (T)value;
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException();
                }
            }
        }

        #endregion

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            list.Insert(index, item);

            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item);
            OnCollectionChanged(e);
        }

        public void RemoveAt(int index)
        {
            T oldItem = list[index];
            list.RemoveAt(index);

            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem);
            OnCollectionChanged(e);
        }

        public T this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                T oldItem = list[index];
                list[index] = value;

                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem);
                OnCollectionChanged(e);
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            list.Add(item);

            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item);
            OnCollectionChanged(e);
        }

        public void Clear()
        {
            list.Clear();

            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(e);
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return list.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((IList<T>)list).IsReadOnly; }
        }

        public bool Remove(T item)
        {
            bool result = list.Remove(item);

            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item);
            OnCollectionChanged(e);
            return result;
        }

        public void Sort()
        {
            list.Sort();

            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, list);
            OnCollectionChanged(e);
        }

        public void Sort(Comparison<T> comparison)
        {
            list.Sort(comparison);

            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, list);
            OnCollectionChanged(e);
        }

        public void Sort(IComparer<T> comparer)
        {
            list.Sort(comparer);

            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, list);
            OnCollectionChanged(e);
        }

        #endregion

        #region INotifyCollectionChanged Members

        public event Action<object, NotifyCollectionChangedEventArgs> CollectionChanged;

        #endregion

        #region INotifyPropertyChanged Members

        public event Action<object, string> PropertyChanged;

        #endregion

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (IsNotifying && CollectionChanged != null)
            {
                CollectionChanged.Invoke(this, e);
            }

            // notify count changed
            OnPropertyChanged(CountString);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, propertyName);
            }
        }

        private static bool IsCompatibleObject(object value)
        {
            // Non-null values are fine. Only accept nulls if T is a class or Nullable<U>.
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
            return ((value is T) || (value == null && default(T) == null));
        }
    }
}