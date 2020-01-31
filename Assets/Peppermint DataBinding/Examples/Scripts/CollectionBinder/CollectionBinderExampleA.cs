using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to bind to collection object (ObservableList).
    ///
    /// The type of the collection is ObservableList, which implements the INotifyCollectionChanged
    /// interface, any list operation, such as add, remove and sort will notify the underlying data
    /// binding to update the view.
    /// </summary>
    public class CollectionBinderExampleA : BindableMonoBehaviour
    {
        public int initCount = 20;

        private int nextIndex;
        private ObservableList<CollectionItem> itemList;

        private ICommand resetListCommand;
        private ICommand addItemCommand;
        private ICommand clearListCommand;
        private ICommand shuffleListCommand;
        private ICommand sortListCommand;
        private ICommand changeColorToRedCommand;
        private ICommand changeColorToGreenCommand;
        private ICommand changeColorToBlueCommand;

        #region Bindable Properties

        public ObservableList<CollectionItem> ItemList
        {
            get { return itemList; }
            set { SetProperty(ref itemList, value, "ItemList"); }
        }

        public ICommand ResetListCommand
        {
            get { return resetListCommand; }
            set { SetProperty(ref resetListCommand, value, "ResetListCommand"); }
        }

        public ICommand AddItemCommand
        {
            get { return addItemCommand; }
            set { SetProperty(ref addItemCommand, value, "AddItemCommand"); }
        }

        public ICommand ClearListCommand
        {
            get { return clearListCommand; }
            set { SetProperty(ref clearListCommand, value, "ClearListCommand"); }
        }

        public ICommand ShuffleListCommand
        {
            get { return shuffleListCommand; }
            set { SetProperty(ref shuffleListCommand, value, "ShuffleListCommand"); }
        }

        public ICommand SortListCommand
        {
            get { return sortListCommand; }
            set { SetProperty(ref sortListCommand, value, "SortListCommand"); }
        }

        public ICommand ChangeColorToRedCommand
        {
            get { return changeColorToRedCommand; }
            set { SetProperty(ref changeColorToRedCommand, value, "ChangeColorToRedCommand"); }
        }

        public ICommand ChangeColorToGreenCommand
        {
            get { return changeColorToGreenCommand; }
            set { SetProperty(ref changeColorToGreenCommand, value, "ChangeColorToGreenCommand"); }
        }

        public ICommand ChangeColorToBlueCommand
        {
            get { return changeColorToBlueCommand; }
            set { SetProperty(ref changeColorToBlueCommand, value, "ChangeColorToBlueCommand"); }
        }

        #endregion
	
        void Start()
        {
            itemList = new ObservableList<CollectionItem>();
            ResetList();

            // create commands
            resetListCommand = new DelegateCommand(ResetList);
            clearListCommand = new DelegateCommand(ClearList);
            shuffleListCommand = new DelegateCommand(ShuffleList);
            sortListCommand = new DelegateCommand(SortList);
            addItemCommand = new DelegateCommand(AddNewItem);

            changeColorToRedCommand = new DelegateCommand(() => ChangeColorTag(ColorTag.Red));
            changeColorToGreenCommand = new DelegateCommand(() => ChangeColorTag(ColorTag.Green));
            changeColorToBlueCommand = new DelegateCommand(() => ChangeColorTag(ColorTag.Blue));

            // add source
            BindingManager.Instance.AddSource(this, typeof(CollectionBinderExampleA).Name);
        }

        void OnDestroy()
        {
            // remove source
            BindingManager.Instance.RemoveSource(this);
        }

        public void ResetList()
        {
            Debug.LogFormat("ResetList");

            ClearList();

            // disable notify
            itemList.IsNotifying = false;

            for (int i = 0; i < initCount; i++)
            {
                AddNewItem();
            }

            // enable notify and notify add event
            itemList.IsNotifying = true;
            itemList.NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, itemList));
        }

        public void ClearList()
        {
            Debug.LogFormat("ClearList");

            nextIndex = 0;
            itemList.Clear();
        }

        public void SortList()
        {
            Debug.LogFormat("SortList");

            itemList.Sort((x, y) => x.Index.CompareTo(y.Index));
        }

        public void ShuffleList()
        {
            Debug.LogFormat("ShuffleList");

            itemList.Shuffle();
        }

        public void AddNewItem()
        {
            // create new item
            var newItem = new CollectionItem();

            // set values
            newItem.Index = nextIndex++;
            newItem.Tag = (ColorTag)(newItem.Index % 3);
            newItem.Name = string.Format("Item {0:D2}", newItem.Index);

            // use closure
            newItem.RemoveCommand = new DelegateCommand(() => RemoveItem(newItem));

            // add it
            itemList.Add(newItem);
        }

        public void RemoveItem(CollectionItem item)
        {
            Debug.LogFormat("RemoveItem {0}", item.Name);
            itemList.Remove(item);
        }

        public void ChangeColorTag(ColorTag newTag)
        {
            Debug.LogFormat("ChangeColorTag {0}", newTag);

            foreach (var item in itemList)
            {
                item.Tag = newTag;
            }
        }
    }
}
