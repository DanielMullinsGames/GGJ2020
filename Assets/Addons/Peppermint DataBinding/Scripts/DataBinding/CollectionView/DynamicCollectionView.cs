using System;
using System.Collections.Generic;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// CollectionView for dynamic binding
    /// </summary>
    public class DynamicCollectionView : ICollectionView
    {
        private Predicate<object> filter;
        private Func<object, object, int> sort;

        private ListDynamicBinding owner;
        private List<object> modifiedList;

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
            get { return modifiedList.Count; }
        }

        public List<object> ModifiedList
        {
            get { return modifiedList; }
        }

        public event Action ViewChanged;

        public DynamicCollectionView(ListDynamicBinding owner)
        {
            this.owner = owner;

            modifiedList = new List<object>();
        }

        private void ApplyFilter()
        {
            foreach (var item in owner.ItemsSource)
            {
                bool include = true;

                // invoke filter
                if (filter != null)
                {
                    // call delegate
                    include = filter.Invoke(item);
                }

                if (include)
                {
                    modifiedList.Add(item);
                }
            }
        }

        private void ApplySort()
        {
            if (sort == null)
            {
                return;
            }

            // sort list
            modifiedList.Sort(CompareTo);
        }

        private int CompareTo(object lhs, object rhs)
        {
            // call delegate
            int result = sort.Invoke(lhs, rhs);
            return result;
        }

        public void Apply()
        {
            // clear list
            modifiedList.Clear();

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

            owner.UpdateDynamicList();
            owner.UpdateView();
        }
    }
}
