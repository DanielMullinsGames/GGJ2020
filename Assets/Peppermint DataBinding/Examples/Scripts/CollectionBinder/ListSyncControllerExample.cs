using System.Collections;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to use the ListSyncController.
    ///
    /// ListSyncController can be used to synchronize content from source ObservableList to target
    /// ObservableList. In this demo, it contains one model list, and two different viewModel lists.
    /// In the code, we only need to modify the model list, such as add, remove, sort, etc. We do not
    /// need to duplicate these modifications in its viewModel lists, the ListSyncController will
    /// handle the hard work for us.
    ///
    /// For example, if we add a new item to the model list, the ListSyncController will handle this
    /// event and call the "createTarget" delegate to create the associated viewModel. If a model is
    /// removed from the model list, the ListSyncController will call the "destroyTarget" delegate to
    /// destroy its associated viewModel. If the "destroyTarget" is null, the call is ignored.
    /// </summary>
    public class ListSyncControllerExample : BindableMonoBehaviour
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

        /// <summary>
        /// ViewModelA only support selection.
        /// </summary>
        public class ItemViewModelA : BindableObject
        {
            private ItemModel model;
            private bool selected;
            private ICommand selectCommand;

            #region Bindable Properties

            public ColorTag Tag
            {
                get { return model.Tag; }
            }

            public string Name
            {
                get { return model.Name; }
            }

            public bool Selected
            {
                get { return selected; }
                set { SetProperty(ref selected, value, "Selected"); }
            }

            public ICommand SelectCommand
            {
                get { return selectCommand; }
                set { SetProperty(ref selectCommand, value, "SelectCommand"); }
            }

            #endregion

            public ItemViewModelA(ItemModel model)
            {
                this.model = model;

                // register event
                model.PropertyChanged += OnModelPropertyChanged;
            }

            public void OnDestroy()
            {
                // unregister event
                model.PropertyChanged -= OnModelPropertyChanged;
            }

            private void OnModelPropertyChanged(object sender, string propertyName)
            {
                if (propertyName == "Name" || propertyName == "Tag")
                {
                    // redirect event
                    NotifyPropertyChanged(propertyName);
                }
            }
        }

        /// <summary>
        /// ItemViewModelB supports selection and removing.
        /// </summary>
        public class ItemViewModelB : BindableObject
        {
            private ItemModel model;
            private bool selected;
            private ICommand removeCommand;
            private ICommand selectCommand;

            // expose internal model for data binding
            public ItemModel Model
            {
                get { return model; }
            }

            #region Bindable Properties

            public bool Selected
            {
                get { return selected; }
                set { SetProperty(ref selected, value, "Selected"); }
            }

            public ICommand RemoveCommand
            {
                get { return removeCommand; }
                set { SetProperty(ref removeCommand, value, "RemoveCommand"); }
            }

            public ICommand SelectCommand
            {
                get { return selectCommand; }
                set { SetProperty(ref selectCommand, value, "SelectCommand"); }
            }

            #endregion

            public ItemViewModelB(ItemModel model)
            {
                this.model = model;
            }
        }

        public int initCount = 20;

        private int nextIndex;
        private ObservableList<ItemModel> modelList;
        private ObservableList<ItemViewModelA> viewModelListA;
        private ObservableList<ItemViewModelB> viewModelListB;
        private int selectedCountA;
        private int selectedCountB;
        private bool hideListA;
        private ICommand resetCommand;
        private ICommand addItemCommand;
        private ICommand clearCommand;
        private ICommand shuffleListCommand;
        private ICommand sortListCommand;
        private ICommand changeColorToRedCommand;
        private ICommand changeColorToGreenCommand;
        private ICommand changeColorToBlueCommand;

        private ListSyncController<ItemModel, ItemViewModelA> syncControllerA;
        private ListSyncController<ItemModel, ItemViewModelB> syncControllerB;

        #region Bindable Properties

        public int SelectedCountA
        {
            get { return selectedCountA; }
            set { SetProperty(ref selectedCountA, value, "SelectedCountA"); }
        }

        public int SelectedCountB
        {
            get { return selectedCountB; }
            set { SetProperty(ref selectedCountB, value, "SelectedCountB"); }
        }

        public bool HideListA
        {
            get { return hideListA; }
            set { SetProperty(ref hideListA, value, "HideListA"); }
        }

        public ObservableList<ItemViewModelA> ViewModelListA
        {
            get { return viewModelListA; }
            set { SetProperty(ref viewModelListA, value, "ViewModelListA"); }
        }

        public ObservableList<ItemViewModelB> ViewModelListB
        {
            get { return viewModelListB; }
            set { SetProperty(ref viewModelListB, value, "ViewModelListB"); }
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

        IEnumerator Start()
        {
            // create containers
            modelList = new ObservableList<ItemModel>();
            viewModelListA = new ObservableList<ItemViewModelA>();
            viewModelListB = new ObservableList<ItemViewModelB>();

            ResetItems();

            // create sync controller
            syncControllerA = new ListSyncController<ItemModel, ItemViewModelA>(modelList, viewModelListA, CreateViewModelA, DestroyViewModelA);
            syncControllerB = new ListSyncController<ItemModel, ItemViewModelB>(modelList, ViewModelListB, CreateViewModelB);

            // begin sync
            syncControllerA.BeginSync();
            syncControllerB.BeginSync();

            // create commands
            resetCommand = new DelegateCommand(ResetItems);
            clearCommand = new DelegateCommand(ClearItems);
            addItemCommand = new DelegateCommand(AddNewItem);
            sortListCommand = new DelegateCommand(SortList);
            shuffleListCommand = new DelegateCommand(ShuffleList);
            changeColorToRedCommand = new DelegateCommand(() => ChangeColorTag(ColorTag.Red));
            changeColorToGreenCommand = new DelegateCommand(() => ChangeColorTag(ColorTag.Green));
            changeColorToBlueCommand = new DelegateCommand(() => ChangeColorTag(ColorTag.Blue));

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // wait one frame for grid view
                yield return null;
            }

            BindingManager.Instance.AddSource(this, typeof(ListSyncControllerExample).Name);
        }

        void OnDestroy()
        {
            // end sync
            syncControllerA.EndSync();
            syncControllerB.EndSync();

            BindingManager.Instance.RemoveSource(this);
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
            modelList.Clear();

            SelectedCountA = 0;
            SelectedCountB = 0;
        }

        public void AddNewItem()
        {
            // create new item
            var newItem = new ItemModel();

            // set values
            newItem.Index = nextIndex++;
            newItem.Tag = (ColorTag)(newItem.Index % 3);
            newItem.Name = string.Format("Item {0:D2}", newItem.Index);

            // add it
            modelList.Add(newItem);
        }

        public void SortList()
        {
            Debug.LogFormat("SortList");

            modelList.Sort((x, y) => x.Index.CompareTo(y.Index));

            RefreshGridView();
        }

        public void ShuffleList()
        {
            Debug.LogFormat("ShuffleList");

            modelList.Shuffle();

            RefreshGridView();
        }

        public void ChangeColorTag(ColorTag newTag)
        {
            Debug.LogFormat("ChangeColorTag {0}", newTag);

            foreach (var item in modelList)
            {
                item.Tag = newTag;
            }
        }

        public void RemoveItem(ItemModel item)
        {
            Debug.LogFormat("RemoveItem {0}", item.Name);

            // remove it
            modelList.Remove(item);

            UpdateSelectedCountA();
            UpdateSelectedCountB();
        }

        public void SelectA(ItemViewModelA vm)
        {
            Debug.LogFormat("SelectA {0}", (vm != null) ? vm.Name : "null");

            if (vm != null)
            {
                vm.Selected = !vm.Selected;
            }

            UpdateSelectedCountA();
        }

        public void SelectB(ItemViewModelB vm)
        {
            Debug.LogFormat("SelectB {0}", (vm != null) ? vm.Model.Name : "null");

            foreach (var item in viewModelListB)
            {
                item.Selected = (item == vm);
            }

            UpdateSelectedCountB();
        }

        private void UpdateSelectedCountA()
        {
            int count = 0;
            foreach (var item in viewModelListA)
            {
                if (item.Selected)
                {
                    count++;
                }
            }

            // update selected count
            SelectedCountA = count;
        }

        private void RefreshGridView()
        {
            // force refresh grid view
            HideListA = true;
            HideListA = false;
        }

        private void UpdateSelectedCountB()
        {
            int count = 0;
            foreach (var item in viewModelListB)
            {
                if (item.Selected)
                {
                    count++;
                }
            }

            // update selected count
            SelectedCountB = count;
        }

        private ItemViewModelA CreateViewModelA(ItemModel model)
        {
            // create viewModel from model
            var vm = new ItemViewModelA(model);

            // create commands
            vm.SelectCommand = new DelegateCommand(() => SelectA(vm));

            return vm;
        }

        private void DestroyViewModelA(ItemViewModelA vm)
        {
            // clean up
            vm.OnDestroy();
        }

        private ItemViewModelB CreateViewModelB(ItemModel model)
        {
            // create viewModel from model
            var vm = new ItemViewModelB(model);

            // create commands
            vm.SelectCommand = new DelegateCommand(() => SelectB(vm));
            vm.RemoveCommand = new DelegateCommand(() => RemoveItem(model));

            return vm;
        }
    }
}