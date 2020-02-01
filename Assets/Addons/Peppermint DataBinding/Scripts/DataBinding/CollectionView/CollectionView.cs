using System;
using System.Collections.Generic;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// CollectionView allows display the collection based on sort and filter, all without having to
    /// manipulate the underlying source collection itself.
    /// </summary>
    public class CollectionView : ICollectionView
    {
        private Predicate<object> filter;
        private Func<object, object, int> sort;

        private CollectionBinding owner;
        private List<object> sortingList;
        private int count;

        public Predicate<object> Filter
        {
            get { return filter; }
            set { filter = value; }
        }

        public Func<object, object, int> Sort
        {
            get { return sort; }
            set { sort = value; }
        }

        public int Count
        {
            get { return count; }
        }

        public event Action ViewChanged;

        public CollectionView(CollectionBinding owner)
        {
            this.owner = owner;

            // create list
            sortingList = new List<object>();
        }

        private void ApplyFilter()
        {
            int index = 0;

            foreach (var item in owner.BindingDictionary)
            {
                bool include = true;

                // invoke filter
                if (filter != null)
                {
                    // call delegate
                    include = filter.Invoke(item.Key);
                }

                // active view
                BindingUtility.SetGameObjectActive(item.Value, include);

                if (include)
                {
                    index++;
                }
            }

            // update count
            count = index;
        }

        private void ApplySort()
        {
            if (sort == null)
            {
                return;
            }

            // reset list
            sortingList.Clear();
            sortingList.AddRange(owner.BindingDictionary.Keys);

            // sort list
            sortingList.Sort(CompareTo);

            int index = 0;
            foreach (var item in sortingList)
            {
                // get view
                var view = owner.BindingDictionary[item];

                // set view index
                view.transform.SetSiblingIndex(index);

                index++;
            }

            sortingList.Clear();
        }

        private int CompareTo(object lhs, object rhs)
        {
            // call delegate
            int result = sort.Invoke(lhs, rhs);
            return result;
        }

        public void Apply()
        {
            ApplyFilter();
            ApplySort();

            if (ViewChanged != null)
            {
                // raise event
                ViewChanged.Invoke();
            }
        }

        public void Refresh()
        {
            Apply();

            owner.UpdateView();
        }
    }
}
