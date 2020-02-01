
namespace Peppermint.DataBinding.Example
{
    public class HelloCollectionBinding : BindableMonoBehaviour
    {
        public class Item : BindableObject
        {
            private string name;
            private ICommand removeCommand;

            #region Bindable Properties

            public string Name
            {
                get { return name; }
                set { SetProperty(ref name, value, "Name"); }
            }

            public ICommand RemoveCommand
            {
                get { return removeCommand; }
                set { SetProperty(ref removeCommand, value, "RemoveCommand"); }
            }

            #endregion
        }

        public int initCount = 8;
        private int nextIndex;

        private ObservableList<Item> itemList;
        private ICommand clearCommand;
        private ICommand createCommand;

        #region Bindable Properties

        public ObservableList<Item> ItemList
        {
            get { return itemList; }
            set { SetProperty(ref itemList, value, "ItemList"); }
        }

        public ICommand ClearCommand
        {
            get { return clearCommand; }
            set { SetProperty(ref clearCommand, value, "ClearCommand"); }
        }

        public ICommand CreateCommand
        {
            get { return createCommand; }
            set { SetProperty(ref createCommand, value, "CreateCommand"); }
        }

        #endregion

        void Start()
        {
            itemList = new ObservableList<Item>();

            // create items
            for (int i = 0; i < initCount; i++)
            {
                AddNewItem();
            }

            // create commands
            clearCommand = new DelegateCommand(() => itemList.Clear());
            createCommand = new DelegateCommand(AddNewItem);

            // add current object as binding source
            BindingManager.Instance.AddSource(this, typeof(HelloCollectionBinding).Name);
        }

        void OnDestroy()
        {
            // remove source
            BindingManager.Instance.RemoveSource(this);
        }

        public void RemoveItem(Item item)
        {
            itemList.Remove(item);
        }

        private void AddNewItem()
        {
            // create new item
            var newItem = new Item();
            newItem.Name = string.Format("Item {0:D02}", nextIndex++);

            // set command
            newItem.RemoveCommand = new DelegateCommand(() => RemoveItem(newItem));

            // add to list
            itemList.Add(newItem);
        }
    }
}
