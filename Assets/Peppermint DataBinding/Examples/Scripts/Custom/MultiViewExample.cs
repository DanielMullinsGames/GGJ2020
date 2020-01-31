using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to switch multiple views.
    ///
    /// This example is a simplified item inspector. The UI consists of two parts: The left part is a
    /// scroll view which contains all items, and the right part is a single panel that shows the
    /// details of the currently selected item. Because different item types have different
    /// properties, so we need to create different views.
    ///
    /// First, we create a collection that contains different types of items, such as WeaponItem,
    /// CharacterItem, etc. We can use the CollectionMultiViewBinder and specify different view
    /// templates based on item type. Note that we did not specify the collectionViewPath in
    /// CollectionMultiViewBinder, so the binder will use built-in type name matching to choose the
    /// view template.
    ///
    /// Next, we need to implement the item selection. We can add the selected item as a binding
    /// source and set up a view to bind to it. You can try adding a DataContextRegister to each
    /// detail view and using the selector to switch views, but it doesn't work. Because only the
    /// compatible view can bind to the source and all other incompatible views must unbind from this
    /// source. For example, the weapon detail panel has a text binder which bind to the attack
    /// property, if the current selected item is a Character, the binding will fail.
    ///
    /// To solve this problem, we can create a custom selector which can switch views and also handle
    /// data context switching. When we update the selected item, we need to remove the old source
    /// first, then we update SelectedItemTypeName property to notify the selector to switch the view
    /// and its data context, and then we can add the new source.
    /// </summary>
    public class MultiViewExample : BindableMonoBehaviour
    {
        public int weaponCount = 5;
        public int characterCount = 3;

        /// <summary>
        /// Base viewModel class
        /// </summary>
        public class BaseItem : BindableObject
        {
            private string name;
            private bool isSelected;
            private ICommand selectCommand;

            #region Bindable Properties

            public string Name
            {
                get { return name; }
                set { SetProperty(ref name, value, "Name"); }
            }

            public bool IsSelected
            {
                get { return isSelected; }
                set { SetProperty(ref isSelected, value, "IsSelected"); }
            }

            public ICommand SelectCommand
            {
                get { return selectCommand; }
                set { SetProperty(ref selectCommand, value, "SelectCommand"); }
            }

            #endregion
        }

        /// <summary>
        /// ViewModel class for weapon
        /// </summary>
        public class WeaponItem : BaseItem
        {
            private float attack;
            private float defense;

            #region Bindable Properties

            public float Attack
            {
                get { return attack; }
                set { SetProperty(ref attack, value, "Attack"); }
            }

            public float Defense
            {
                get { return defense; }
                set { SetProperty(ref defense, value, "Defense"); }
            }

            #endregion
        }

        /// <summary>
        /// ViewModel class for character
        /// </summary>
        public class CharacterItem : BaseItem
        {
            private float hp;
            private float mp;
            private int level;

            #region Bindable Properties

            public float Hp
            {
                get { return hp; }
                set { SetProperty(ref hp, value, "Hp"); }
            }

            public float Mp
            {
                get { return mp; }
                set { SetProperty(ref mp, value, "Mp"); }
            }

            public int Level
            {
                get { return level; }
                set { SetProperty(ref level, value, "Level"); }
            }

            #endregion
        }

        private BaseItem selectedItem;
        private string selectedItemTypeName;
        private ObservableList<BaseItem> itemList;
        private ICommand clearSelectionCommand;

        #region Bindable Properties

        public ICommand ClearSelectionCommand
        {
            get { return clearSelectionCommand; }
            set { SetProperty(ref clearSelectionCommand, value, "ClearSelectionCommand"); }
        }

        public string SelectedItemTypeName
        {
            get { return selectedItemTypeName; }
            set { SetProperty(ref selectedItemTypeName, value, "SelectedItemTypeName"); }
        }

        public ObservableList<BaseItem> ItemList
        {
            get { return itemList; }
            set { SetProperty(ref itemList, value, "ItemList"); }
        }

        #endregion

        void Start()
        {
            Init();

            BindingManager.Instance.AddSource(this, typeof(MultiViewExample).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        private void Init()
        {
            itemList = new ObservableList<BaseItem>();

            // add weapons
            for (int index = 0; index < weaponCount; index++)
            {
                var item = new WeaponItem()
                {
                    Name = string.Format("Weapon {0}", (char)(index + 'A')),
                    Attack = 10 * (index + 1),
                    Defense = 20 * (index + 1),
                };

                itemList.Add(item);
            }

            // add characters
            for (int index = 0; index < characterCount; index++)
            {
                var item = new CharacterItem()
                {
                    Name = string.Format("Character {0}", (char)(index + 'A')),
                    Hp = 100 * (index + 1),
                    Mp = 50 * (index + 1),
                    Level = index + 1,
                };

                itemList.Add(item);
            }

            // create command
            foreach (var item in itemList)
            {
                var currentItem = item;
                item.SelectCommand = new DelegateCommand(() => SelectItem(currentItem));
            }

            clearSelectionCommand = new DelegateCommand(() => SelectItem(null));
        }

        public void SelectItem(BaseItem target)
        {
            if (selectedItem == target)
            {
                return;
            }

            Debug.Log(string.Format("SelectItem {0}", (target != null) ? target.Name : "null"), gameObject);

            // set flag
            foreach (var item in itemList)
            {
                item.IsSelected = (item == target);
            }

            if (selectedItem != null)
            {
                // remove old source
                BindingManager.Instance.RemoveSource(selectedItem);
            }

            // update selected item
            selectedItem = target;

            if (selectedItem != null)
            {
                // set type name, so the ViewSelector will choose the correct view
                SelectedItemTypeName = target.GetType().Name;

                // add new source
                BindingManager.Instance.AddSource(selectedItem, "SelectedItem");
            }
            else
            {
                // set type name to null
                SelectedItemTypeName = null;
            }
        }
    }
}
