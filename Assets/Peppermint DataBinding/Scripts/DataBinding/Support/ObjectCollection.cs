using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Object collection supports single element and enumerable elements. It also contains an
    /// optimized enumerator to avoid GC.
    /// </summary>
    public struct ObjectCollection
    {
        private IEnumerable items;
        private object item;

        // ctor for enumerable
        public ObjectCollection(IEnumerable items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            this.items = items;
            this.item = null;
        }

        // ctor for single item
        public ObjectCollection(object item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            this.item = item;
            this.items = null;
        }

        public Enumerator GetEnumerator()
        {
            if (items != null)
            {
                var itemList = items as IList;
                if (itemList != null)
                {
                    // list
                    return new Enumerator(itemList);
                }
                else
                {
                    // enumerable
                    return new Enumerator(items);
                }
            }
            else if (item != null)
            {
                // single item
                return new Enumerator(item);
            }
            else
            {
                // empty
                return new Enumerator();
            }
        }

        #region Enumerator

        /// <summary>
        /// If the source type is IList, it uses index to get the element. If the source type is
        /// object, it just return it. Otherwise it will use the default enumerator.
        /// </summary>
        public struct Enumerator : IEnumerator, IDisposable
        {
            public enum Mode
            {
                None = 0,
                SingleItem,
                ItemList,
                ItemEnumerator,
            }

            private Mode mode;
            private object item;
            private IEnumerator itemEnumerator;
            private IList itemList;
            private int index;

            public object Current
            {
                get
                {
                    if (mode == Mode.SingleItem)
                    {
                        if (index != 0)
                        {
                            throw new InvalidOperationException();
                        }

                        return item;
                    }
                    else if (mode == Mode.ItemList)
                    {
                        if (index < 0 || index > (itemList.Count - 1))
                        {
                            throw new InvalidOperationException();
                        }

                        return itemList[index];
                    }
                    else if (mode == Mode.ItemEnumerator)
                    {
                        return itemEnumerator.Current;
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            // ctor for single item
            public Enumerator(object item) : this()
            {
                mode = Mode.SingleItem;

                this.item = item;
                index = -1;
            }

            // ctor for list
            public Enumerator(IList items) : this()
            {
                mode = Mode.ItemList;

                this.itemList = items;
                index = -1;
            }

            // ctor for enumerable
            public Enumerator(IEnumerable items) : this()
            {
                mode = Mode.ItemEnumerator;

                itemEnumerator = items.GetEnumerator();
            }

            public bool MoveNext()
            {
                if (mode == Mode.SingleItem)
                {
                    index++;

                    return index < 1;
                }
                else if (mode == Mode.ItemList)
                {
                    index++;

                    return index < itemList.Count;
                }
                else if (mode == Mode.ItemEnumerator)
                {
                    return itemEnumerator.MoveNext();
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                if (mode == Mode.SingleItem || mode == Mode.ItemList)
                {
                    index = -1;
                }
                else if (mode == Mode.ItemEnumerator)
                {
                    itemEnumerator.Reset();
                }
            }

            public void Dispose()
            {
                if (itemEnumerator != null)
                {
                    // dispose internal enumerator
                    var disposable = itemEnumerator as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }

        #endregion
    }
}
