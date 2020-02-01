using System;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to use CollectionView.
    ///
    /// To enable CollectionView, create a writable property and set the property name to
    /// collectionViewPath in CollectionBinder. Once the source is bound the created CollectionView
    /// is assigned through the set accessor. You can specify the filter and sort delegate to
    /// CollectionView. If filter is null, all items are included. If the sort is null, no sorting is
    /// applied. Call ICollectionView.Refresh() method if you need to refresh the view.
    ///
    /// In this demo, the CollectionBinder for the Source List is used for debugging.
    /// </summary>
    public class CollectionViewExample : BindableMonoBehaviour
    {
        public int initCount = 20;
        public bool showSourceList;

        private int nextIndex;
        private ObservableList<CollectionItem> itemList;
        private string nameFilter;
        private bool ascending;
        private int collectionViewItemCount;
        private ICommand addCommand;
        private ICommand clearCommand;
        private ICommand resetCommand;
        private ICommand shuffleCommand;
        private ICommand sortCommand;
        private ICollectionView collectionView;

        #region Bindable Properties

        public bool ShowSourceList
        {
            get { return showSourceList; }
            set { SetProperty(ref showSourceList, value, "ShowSourceList"); }
        }

        public ObservableList<CollectionItem> ItemList
        {
            get { return itemList; }
            set { SetProperty(ref itemList, value, "ItemList"); }
        }

        public string NameFilter
        {
            get { return nameFilter; }
            set
            {
                if (SetProperty(ref nameFilter, value, "NameFilter"))
                {
                    RefreshView();
                }
            }
        }

        public bool Ascending
        {
            get { return ascending; }
            set
            {
                if (SetProperty(ref ascending, value, "Ascending"))
                {
                    RefreshView();
                }
            }
        }

        public int CollectionViewItemCount
        {
            get { return collectionViewItemCount; }
            set { SetProperty(ref collectionViewItemCount, value, "CollectionViewItemCount"); }
        }

        public ICommand AddCommand
        {
            get { return addCommand; }
            set { SetProperty(ref addCommand, value, "AddCommand"); }
        }

        public ICommand ClearCommand
        {
            get { return clearCommand; }
            set { SetProperty(ref clearCommand, value, "ClearCommand"); }
        }

        public ICommand ResetCommand
        {
            get { return resetCommand; }
            set { SetProperty(ref resetCommand, value, "ResetCommand"); }
        }

        public ICommand ShuffleCommand
        {
            get { return shuffleCommand; }
            set { SetProperty(ref shuffleCommand, value, "ShuffleCommand"); }
        }

        public ICommand SortCommand
        {
            get { return sortCommand; }
            set { SetProperty(ref sortCommand, value, "SortCommand"); }
        }

        public ICollectionView CollectionView
        {
            set
            {
                SetCollectionView(value);
            }
        }

        #endregion

        void Start()
        {
            itemList = new ObservableList<CollectionItem>();
            ResetList();

            AddCommand = new DelegateCommand(AddItem);
            ResetCommand = new DelegateCommand(ResetList);
            ClearCommand = new DelegateCommand(ClearList);
            ShuffleCommand = new DelegateCommand(ShuffleList);
            SortCommand = new DelegateCommand(SortList);

            BindingManager.Instance.AddSource(this, typeof(CollectionViewExample).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        public void ResetList()
        {
            ClearList();

            // disable notify
            itemList.IsNotifying = false;

            for (int i = 0; i < initCount; i++)
            {
                AddItem();
            }

            // enable notify and notify add event
            itemList.IsNotifying = true;
            itemList.NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, itemList));

            Ascending = true;
            NameFilter = null;
        }

        public void AddItem()
        {
            var item = new CollectionItem();

            // set values
            item.Index = nextIndex++;
            item.Tag = (ColorTag)(item.Index % 3);

            // set name
            int letterIndex = item.Index % 26;
            char letter = (char)((int)'A' + letterIndex);
            item.Name = string.Format("{0} {1:D04}", letter, item.Index);

            item.RemoveCommand = new DelegateCommand(() => RemoveItem(item));

            itemList.Add(item);
        }

        public void ShuffleList()
        {
            itemList.Shuffle();
        }

        public void SortList()
        {
            itemList.Sort((x, y) => x.Index.CompareTo(y.Index));
        }

        public void ClearList()
        {
            itemList.Clear();
            nextIndex = 0;
        }

        public void RemoveItem(CollectionItem item)
        {
            itemList.Remove(item);
        }

        private void RefreshView()
        {
            if (collectionView != null)
            {
                collectionView.Refresh();
            }
        }

        private bool FilterItem(object o)
        {
            if (string.IsNullOrEmpty(nameFilter))
            {
                return true;
            }

            var item = o as CollectionItem;

            return item.Name.IndexOf(nameFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        private int SortItem(object lhs, object rhs)
        {
            var lhsItem = lhs as CollectionItem;
            var rhsItem = rhs as CollectionItem;

            int result = lhsItem.Index.CompareTo(rhsItem.Index);

            if (Ascending)
            {
                return result;
            }
            else
            {
                return -result;
            }
        }

        private void SetCollectionView(ICollectionView value)
        {
            Debug.LogFormat("SetCollectionView, value={0}", value);

            if (collectionView != null)
            {
                // clear
                collectionView.ViewChanged -= collectionView_ViewChanged;
                collectionView.Filter = null;
                collectionView.Sort = null;
                CollectionViewItemCount = 0;
            }

            collectionView = value;

            if (collectionView != null)
            {
                // setup
                collectionView.ViewChanged += collectionView_ViewChanged;
                collectionView.Filter = FilterItem;
                collectionView.Sort = SortItem;
                CollectionViewItemCount = collectionView.Count;
            }
        }

        void collectionView_ViewChanged()
        {
            // get item count from collection view
            CollectionViewItemCount = collectionView.Count;
        }
    }
}
