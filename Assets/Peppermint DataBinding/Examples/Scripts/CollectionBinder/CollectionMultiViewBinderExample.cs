using System.Linq;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to setup CollectionMultiViewBinder.
    ///
    /// In this example, we created two different viewModel classes for the item and header. All
    /// created viewModel objects are added to the same objectList, so we need
    /// CollectionMultiViewBinder to select the correct view template based on the item type.
    ///
    /// CollectionMultiViewBinder is an extended CollectionBinder, so the basic settings are the
    /// same. In this example, two view templates are set in the binder. The first view template
    /// associated with name "Header", and the second one associated with name "Item". In
    /// OnSelectViewTemplate method, if the current object is Header, select template "Header",
    /// otherwise select template "Item".
    /// </summary>
    public class CollectionMultiViewBinderExample : BindableMonoBehaviour
    {
        // ViewModel for item
        public class Item : BindableObject
        {
            private string name;
            private float price;
            private bool isOwned;
            private ICommand buyCommand;

            #region Bindable Properties

            public string Name
            {
                get { return name; }
                set { SetProperty(ref name, value, "Name"); }
            }

            public float Price
            {
                get { return price; }
                set { SetProperty(ref price, value, "Price"); }
            }

            public bool IsOwned
            {
                get { return isOwned; }
                set { SetProperty(ref isOwned, value, "IsOwned"); }
            }

            public ICommand BuyCommand
            {
                get { return buyCommand; }
                set { SetProperty(ref buyCommand, value, "BuyCommand"); }
            }

            #endregion
        }

        // ViewModel for the header
        public class Header : BindableObject
        {
            private string name;

            #region Bindable Properties

            public string Name
            {
                get { return name; }
                set { SetProperty(ref name, value, "Name"); }
            }

            #endregion
        }

        public int groupCount = 4;
        public int itemCountPerGroup = 3;

        private ObservableList<object> objectList;
        private SelectViewTemplateDelegate selectViewTemplate;

        private ICommand resetListCommand;

        #region Bindable Properties

        public ObservableList<object> ObjectList
        {
            get { return objectList; }
            set { SetProperty(ref objectList, value, "ObjectList"); }
        }

        public SelectViewTemplateDelegate SelectViewTemplate
        {
            get { return selectViewTemplate; }
            set { SetProperty(ref selectViewTemplate, value, "SelectViewTemplate"); }
        }

        public ICommand ResetListCommand
        {
            get { return resetListCommand; }
            set { SetProperty(ref resetListCommand, value, "ResetListCommand"); }
        }

        #endregion

        void Start()
        {
            objectList = new ObservableList<object>();
            ResetList();

            // create commands
            resetListCommand = new DelegateCommand(ResetList);

            // set function
            selectViewTemplate = OnSelectViewTemplate;

            // add source
            BindingManager.Instance.AddSource(this, typeof(CollectionMultiViewBinderExample).Name);
        }

        void OnDestroy()
        {
            // remove source
            BindingManager.Instance.RemoveSource(this);
        }

        private char ConvertToLetter(int index)
        {
            int letterIndex = index % 26;
            char letter = (char)((int)'A' + letterIndex);
            return letter;
        }

        public void ResetList()
        {
            Debug.LogFormat("ResetList");

            objectList.Clear();

            // disable notify
            objectList.IsNotifying = false;

            for (int groupIndex = 0; groupIndex < groupCount; groupIndex++)
            {
                // create header
                var header = new Header();
                header.Name = string.Format("ItemGroup {0}", ConvertToLetter(groupIndex));
                objectList.Add(header);

                for (int itemIndex = 0; itemIndex < itemCountPerGroup; itemIndex++)
                {
                    // create item
                    var item = new Item();
                    item.Name = string.Format("Item {0}{1}", ConvertToLetter(groupIndex), itemIndex + 1);
                    item.Price = (itemIndex + 1) * 10f;
                    item.BuyCommand = new DelegateCommand(() => BuyItem(item), () => !item.IsOwned);

                    objectList.Add(item);
                }
            }

            // enable notify and notify add event
            objectList.IsNotifying = true;
            objectList.NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, objectList));
        }

        public void BuyItem(Item item)
        {
            Debug.LogFormat("BuyItem {0}", item.Name);

            item.IsOwned = true;
        }

        private GameObject OnSelectViewTemplate(object obj, ViewTemplateConfig[] configs)
        {
            if (obj is Header)
            {
                // use header prefab
                return configs.First(x => x.name == "Header").viewTemplate;
            }
            else
            {
                // use item prefab
                return configs.First(x => x.name == "Item").viewTemplate;
            }
        }
    }
}
