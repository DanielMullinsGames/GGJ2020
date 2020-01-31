using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to use the DictionarySyncController.
    ///
    /// DictionarySyncController can be used to synchronize ObservableDictionary to ObservableList.
    /// When creating the controller, you need to specify the "sortTarget" delegate, because the
    /// items in the dictionary have no determined order. If TSource and TTarget types are the
    /// same, you can ignore the "createTarget" delegate.
    ///
    /// In peppermint data binding, collection bindings do not support associated container, such as
    /// dictionary, etc. If you need to bind to dictionary, you need to create a helper method/class
    /// to synchronize the content from dictionary to bindable list. The DictionarySyncController is
    /// a simple synchronization controller, you can use it as an adaptor to make your dictionary
    /// support data binding.
    /// </summary>
    public class DictionarySyncControllerExample : BindableMonoBehaviour
    {
        // Item model
        public class ItemModel : BindableObject
        {
            private int index;
            private ColorTag tag;
            private string name;

            #region Bindable Properties

            public int Index
            {
                get { return index; }
                set { SetProperty(ref index, value, "Index"); }
            }

            public ColorTag Tag
            {
                get { return tag; }
                set { SetProperty(ref tag, value, "Tag"); }
            }

            public string Name
            {
                get { return name; }
                set { SetProperty(ref name, value, "Name"); }
            }

            #endregion
        }

        // Item view model
        public class ItemViewModel : BindableObject
        {
            private ItemModel model;
            private ICommand removeCommand;

            // expose internal model for data binding
            public ItemModel Model { get { return model; } }

            #region Bindable Properties

            public ICommand RemoveCommand
            {
                get { return removeCommand; }
                set { SetProperty(ref removeCommand, value, "RemoveCommand"); }
            }

            #endregion

            public ItemViewModel(ItemModel model)
            {
                this.model = model;
            }
        }

        public int initCount = 20;

        private int nextIndex;
        private ObservableDictionary<int, ItemModel> modelDictionary;
        private ObservableList<ItemViewModel> viewModelList;
        private DictionarySyncController<int, ItemModel, ItemViewModel> syncController;

        private ICommand resetCommand;
        private ICommand addItemCommand;
        private ICommand clearCommand;

        #region Bindable Properties

        public ObservableList<ItemViewModel> ViewModelList
        {
            get { return viewModelList; }
            set { SetProperty(ref viewModelList, value, "ViewModelList"); }
        }

        public ICommand ResetCommand
        {
            get { return resetCommand; }
            set { SetProperty(ref resetCommand, value, "ResetCommand"); }
        }

        public ICommand AddItemCommand
        {
            get { return addItemCommand; }
            set { SetProperty(ref addItemCommand, value, "AddItemCommand"); }
        }

        public ICommand ClearCommand
        {
            get { return clearCommand; }
            set { SetProperty(ref clearCommand, value, "ClearCommand"); }
        }

        #endregion

        void Start()
        {
            // create containers
            modelDictionary = new ObservableDictionary<int, ItemModel>();
            viewModelList = new ObservableList<ItemViewModel>();

            ResetItems();

            // create sync controller
            syncController = new DictionarySyncController<int, ItemModel, ItemViewModel>(modelDictionary, viewModelList,
                SortViewModel, CreateViewModel);

            // begin sync
            syncController.BeginSync();

            // create commands
            resetCommand = new DelegateCommand(ResetItems);
            clearCommand = new DelegateCommand(ClearItems);
            addItemCommand = new DelegateCommand(AddNewItem);

            // add source
            BindingManager.Instance.AddSource(this, typeof(DictionarySyncControllerExample).Name);
        }

        void OnDestroy()
        {
            // end sync
            syncController.EndSync();

            // remove source
            BindingManager.Instance.RemoveSource(this);
        }

        private int SortViewModel(ItemViewModel lhs, ItemViewModel rhs)
        {
            // sort item by index
            return lhs.Model.Index.CompareTo(rhs.Model.Index);
        }

        private ItemViewModel CreateViewModel(ItemModel model)
        {
            // create viewModel from model
            var vm = new ItemViewModel(model);

            // create commands
            vm.RemoveCommand = new DelegateCommand(() => RemoveItem(model));

            return vm;
        }

        public void ResetItems()
        {
            Debug.LogFormat("ResetItems");

            ClearItems();

            for (int i = 0; i < initCount; i++)
            {
                AddNewItem();
            }
        }

        public void ClearItems()
        {
            Debug.LogFormat("ClearItems");

            nextIndex = 0;
            modelDictionary.Clear();
        }

        public void AddNewItem()
        {
            // create new item
            var newItem = new ItemModel();

            // set values
            newItem.Index = nextIndex++;
            newItem.Tag = (ColorTag)(newItem.Index % 3);
            newItem.Name = string.Format("Item {0:D2}", newItem.Index);

            // add to dictionary
            modelDictionary.Add(newItem.Index, newItem);
        }

        public void RemoveItem(ItemModel item)
        {
            Debug.LogFormat("RemoveItem {0}", item.Name);

            // remove from dictionary
            modelDictionary.Remove(item.Index);
        }
    }
}