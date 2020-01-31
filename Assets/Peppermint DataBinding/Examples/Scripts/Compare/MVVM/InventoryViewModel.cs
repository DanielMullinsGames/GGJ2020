using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding.Example.Mvvm
{
    public class InventoryViewModel : BindableMonoBehaviour
    {
        // ViewModel for inventory item
        public class ItemViewModel : BindableObject
        {
            private InventoryModel.ItemModel model;
            private ICommand clickCommand;
            private bool selected;

            // expose internal model for data binding
            public InventoryModel.ItemModel Model
            {
                get { return model; }
            }

            #region Bindable Properties

            public ICommand ClickCommand
            {
                get { return clickCommand; }
                set { SetProperty(ref clickCommand, value, "ClickCommand"); }
            }

            public bool Selected
            {
                get { return selected; }
                set { SetProperty(ref selected, value, "Selected"); }
            }

            #endregion

            public ItemViewModel(InventoryModel.ItemModel model)
            {
                this.model = model;
            }
        }

        private ObservableList<ItemViewModel> viewModelList;
        private ICommand useItemCommand;
        private ICommand sortListCommand;
        private ICommand shuffleListCommand;
        private bool showGridView = true;

        private Dictionary<InventoryModel.ItemModel, ItemViewModel> viewModelDictionary;
        private List<InventoryModel.ItemModel> pendingList;

        private InventoryModel.ItemModel selectedItem;
        private InventoryModel inventory;

        #region Bindable Properties

        public ObservableList<ItemViewModel> ViewModelList
        {
            get { return viewModelList; }
            set { SetProperty(ref viewModelList, value, "ViewModelList"); }
        }

        public ICommand UseItemCommand
        {
            get { return useItemCommand; }
            set { SetProperty(ref useItemCommand, value, "UseItemCommand"); }
        }

        public ICommand SortListCommand
        {
            get { return sortListCommand; }
            set { SetProperty(ref sortListCommand, value, "SortListCommand"); }
        }

        public ICommand ShuffleListCommand
        {
            get { return shuffleListCommand; }
            set { SetProperty(ref shuffleListCommand, value, "ShuffleListCommand"); }
        }

        public bool ShowGridView
        {
            get { return showGridView; }
            set { SetProperty(ref showGridView, value, "ShowGridView"); }
        }

        #endregion

        void Start()
        {
            // keep reference
            inventory = GameState.Instance.inventory;

            // create commands
            useItemCommand = new DelegateCommand(UseItem, () => selectedItem != null);
            sortListCommand = new DelegateCommand(SortList);
            shuffleListCommand = new DelegateCommand(ShuffleList);

            // create containers
            pendingList = new List<InventoryModel.ItemModel>();
            viewModelList = new ObservableList<ItemViewModel>();
            viewModelDictionary = new Dictionary<InventoryModel.ItemModel, ItemViewModel>();

            RefreshList();

            // register collection event
            inventory.ItemList.CollectionChanged += OnItemListCollectionChanged;

            // add source
            BindingManager.Instance.AddSource(this, typeof(InventoryViewModel).Name);
        }

        void OnDestroy()
        {
            // unregister collection event
            inventory.ItemList.CollectionChanged -= OnItemListCollectionChanged;

            // remove source
            BindingManager.Instance.RemoveSource(this);
        }
        
        private void RefreshList()
        {
            BindingUtility.SyncListToDictionary(inventory.ItemList, viewModelDictionary, pendingList, AddItem, RemoveItem);
            
            BindingUtility.SyncListOrder(inventory.ItemList, viewModelList, viewModelDictionary, pendingList);

            if (selectedItem != null)
            {
                // check if selected item is removed
                if (!viewModelDictionary.ContainsKey(selectedItem))
                {
                    SelectItem(null);
                }
            }
        }

        private void AddItem(InventoryModel.ItemModel model)
        {
            // create view model
            var vm = new ItemViewModel(model);

            // set command
            vm.ClickCommand = new DelegateCommand(() => SelectItem(model));

            // add it
            viewModelList.Add(vm);
            viewModelDictionary.Add(model, vm);
        }

        private void RemoveItem(InventoryModel.ItemModel model)
        {
            // get vm
            var vm = viewModelDictionary[model];

            // remove it
            viewModelList.Remove(vm);
            viewModelDictionary.Remove(model);
        }

        void OnItemListCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            RefreshList();
        }

        private void MarkContainerDirty()
        {
            // workaround to force update grid view
            ShowGridView = false;
            ShowGridView = true;
        }

        public void SelectItem(InventoryModel.ItemModel item)
        {
            Debug.LogFormat("SelectItem, item={0}", (item != null) ? item.Name : "null");

            selectedItem = item;

            // disable all
            foreach (var vm in viewModelList)
            {
                vm.Selected = false;
            }

            if (selectedItem != null)
            {
                var vm = viewModelDictionary[item];
                vm.Selected = true;
            }
        }

        public void UseItem()
        {
            Debug.LogFormat("UseItem, item={0}", selectedItem.Name);

            // update amount
            selectedItem.Amount--;

            if (selectedItem.Amount <= 0)
            {
                // remove item if amount is zero
                inventory.ItemList.Remove(selectedItem);

                SelectItem(null);
            }
        }

        public void SortList()
        {
            Debug.LogFormat("SortList");

            // sort list by amount
            inventory.ItemList.Sort((x, y) =>
            {
                // by amount
                int result = y.Amount.CompareTo(x.Amount);

                if (result == 0)
                {
                    // then by name
                    result = x.Name.CompareTo(y.Name);
                }

                return result;
            });

            MarkContainerDirty();
        }

        public void ShuffleList()
        {
            Debug.LogFormat("ShuffleList");

            inventory.ItemList.Shuffle();

            MarkContainerDirty();
        }
    }
}
