using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to bind to general collection.
    ///
    /// Bind to collection type which does not implement the INotifyCollectionChanged interface is
    /// also supported. It's not recommended because the view will not get updated if the collection
    /// changes, such as add, clear, remove, etc.
    ///
    /// If your collection does not change or remain the same, you can bind to it. If the collection
    /// has changed, you have to call NotifyPropertyChanged to let the data binding knows the
    /// collection has changed. The underlying CollectionBinding will reset all items in order to
    /// update the view. So use ObservableList whenever you can.
    ///
    /// No matter what collection type you are binding to, the item in collection will get updated if
    /// the item's property has changed.
    /// </summary>
    public class CollectionBinderExampleB : BindableMonoBehaviour
    {
        public int initCount = 20;

        private int nextIndex;
        private bool needUpdate;
        private List<CollectionItem> itemList;
        private ICommand resetListCommand;
        private ICommand forceUpateCommand;
        private ICommand changeColorToRedCommand;
        private ICommand changeColorToGreenCommand;
        private ICommand changeColorToBlueCommand;

        #region Bindable Properties

        public bool NeedUpdate
        {
            get { return needUpdate; }
            set { SetProperty(ref needUpdate, value, "NeedUpdate"); }
        }

        public List<CollectionItem> ItemList
        {
            get { return itemList; }
            set { SetProperty(ref itemList, value, "ItemList"); }
        }

        public int ItemListCount
        {
            get { return itemList.Count; }
        }

        public ICommand ResetListCommand
        {
            get { return resetListCommand; }
            set { SetProperty(ref resetListCommand, value, "ResetListCommand"); }
        }

        public ICommand ForceUpateCommand
        {
            get { return forceUpateCommand; }
            set { SetProperty(ref forceUpateCommand, value, "ForceUpateCommand"); }
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
            itemList = new List<CollectionItem>();
            for (int i = 0; i < initCount; i++)
            {
                AddNewItem();
            }

            resetListCommand = new DelegateCommand(ResetList);
            forceUpateCommand = new DelegateCommand(ForceUpdateView);
            changeColorToRedCommand = new DelegateCommand(() => ChangeColorTag(ColorTag.Red));
            changeColorToGreenCommand = new DelegateCommand(() => ChangeColorTag(ColorTag.Green));
            changeColorToBlueCommand = new DelegateCommand(() => ChangeColorTag(ColorTag.Blue));

            // add source
            BindingManager.Instance.AddSource(this, typeof(CollectionBinderExampleB).Name);
        }

        void OnDestroy()
        {
            // remove source
            BindingManager.Instance.RemoveSource(this);
        }

        public void ForceUpdateView()
        {
            LogInfo("ForceUpdateView");

            // notify collection changed
            NotifyPropertyChanged("ItemList");

            // notify count is changed
            NotifyPropertyChanged("ItemListCount");

            NeedUpdate = false;
        }

        public void ResetList()
        {
            LogInfo("ResetList, view need manual update.");

            ClearList();

            for (int i = 0; i < initCount; i++)
            {
                AddNewItem();
            }
        }

        public void ClearList()
        {
            NeedUpdate = true;

            nextIndex = 0;
            itemList.Clear();
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
            LogInfo(string.Format("RemoveItem {0}, view need manual update.", item.Name));

            itemList.Remove(item);

            NeedUpdate = true;
        }

        public void ChangeColorTag(ColorTag newTag)
        {
            Debug.LogFormat("ChangeColorTag {0}", newTag);

            foreach (var item in itemList)
            {
                item.Tag = newTag;
            }
        }

        private void LogInfo(string text)
        {
            MessagePopup.Instance.Show(text);
            Debug.Log(text);
        }
    }
}
