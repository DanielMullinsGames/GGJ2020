using System;
using System.Linq;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates a simple in game store.
    /// </summary>
    public class ShopDemo : BindableMonoBehaviour
    {
        public class ShopItem : BindableObject
        {
            private ShopData.Item data;
            private ICommand buyCommand;

            #region Bindable Properties

            public ShopItemType ItemType
            {
                get { return data.itemType; }
            }

            public int Amount
            {
                get { return data.amount; }
            }

            public string Name
            {
                get { return data.name; }
            }

            public string IconName
            {
                get { return data.iconName; }
            }

            public float Price
            {
                get { return data.price; }
            }

            public ICommand BuyCommand
            {
                get { return buyCommand; }
                set { SetProperty(ref buyCommand, value, "BuyCommand"); }
            }

            #endregion

            public ShopItem(ShopData.Item data)
            {
                this.data = data;
            }
        }

        public class PlayerState : BindableObject
        {
            private long gold;
            private long gems;

            #region Bindable Properties

            public long Gold
            {
                get { return gold; }
                set { SetProperty(ref gold, value, "Gold"); }
            }

            public long Gems
            {
                get { return gems; }
                set { SetProperty(ref gems, value, "Gems"); }
            }

            #endregion
        }

        public ShopData shopData;

        private PlayerState playerState;
        private ShopItem pendingItem;

        private ShopItemType currentCategory;
        private ObservableList<ShopItem> itemListA;
        private ObservableList<ShopItem> itemListB;
        private ICommand setCategoryACommand;
        private ICommand setCategoryBCommand;

        #region Bindable Properties

        public ShopItemType CurrentCategory
        {
            get { return currentCategory; }
            set { SetProperty(ref currentCategory, value, "CurrentCategory"); }
        }

        public ObservableList<ShopItem> ItemListA
        {
            get { return itemListA; }
            set { SetProperty(ref itemListA, value, "ItemListA"); }
        }

        public ObservableList<ShopItem> ItemListB
        {
            get { return itemListB; }
            set { SetProperty(ref itemListB, value, "ItemListB"); }
        }

        public ICommand SetCategoryACommand
        {
            get { return setCategoryACommand; }
            set { SetProperty(ref setCategoryACommand, value, "SetCategoryACommand"); }
        }

        public ICommand SetCategoryBCommand
        {
            get { return setCategoryBCommand; }
            set { SetProperty(ref setCategoryBCommand, value, "SetCategoryBCommand"); }
        }

        #endregion

        void Start()
        {
            // load shop data
            LoadData();

            setCategoryACommand = new DelegateCommand(() => SetCategory(ShopItemType.Gold));
            setCategoryBCommand = new DelegateCommand(() => SetCategory(ShopItemType.Gem));

            // create player state
            playerState = new PlayerState();

            BindingManager.Instance.AddSource(this, typeof(ShopDemo).Name);
            BindingManager.Instance.AddSource(playerState, typeof(PlayerState).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
            BindingManager.Instance.RemoveSource(playerState);
        }

        private void LoadData()
        {
            // create gold item list
            itemListA = CreateShopItemList(ShopItemType.Gold);

            // create gem item list
            itemListB = CreateShopItemList(ShopItemType.Gem);
        }

        private ObservableList<ShopItem> CreateShopItemList(ShopItemType itemType)
        {
            var itemList = new ObservableList<ShopItem>();

            var list = shopData.itemList.Where(x => x.itemType == itemType);
            foreach (var item in list)
            {
                // create shop item
                var shopItem = new ShopItem(item);

                // set command
                shopItem.BuyCommand = new DelegateCommand(() => BuyItem(shopItem));

                // add to list
                itemList.Add(shopItem);
            }

            return itemList;
        }

        public void SetCategory(ShopItemType category)
        {
            if (CurrentCategory == category)
            {
                return;
            }

            Debug.LogFormat("ChangeCategory {0}", category);
            CurrentCategory = category;
        }

        public void BuyItem(ShopItem item)
        {
            // setup message box arguments
            var args = new MessageBox.MessageBoxArgs();
            args.messageText = string.Format("Do you want to buy \"{0}\" for ${1:F2}?", item.Name, item.Price);
            args.callback = OnMessageBox;
            args.yesText = "Buy";
            args.noText = "Cancel";

            // save current item
            pendingItem = item;

            // open message box and let the player to confirm purchase
            MessageBox.Instance.Open(args);
        }

        private void OnMessageBox(MessageBox.MessageBoxResult result)
        {
            if (result == MessageBox.MessageBoxResult.Yes)
            {
                LogInfo(String.Format("Buy item \"{0}\", price={1}", pendingItem.Name, pendingItem.Price));

                // handle purchase success. In this demo, we just add currency to the playerState object.
                if (pendingItem.ItemType == ShopItemType.Gold)
                {
                    playerState.Gold += pendingItem.Amount;
                }
                else if (pendingItem.ItemType == ShopItemType.Gem)
                {
                    playerState.Gems += pendingItem.Amount;
                }
            }
            else
            {
                LogInfo(String.Format("Cancel item \"{0}\"", pendingItem.Name));
            }

            // clear pending item
            pendingItem = null;
        }

        private void LogInfo(string text)
        {
            MessagePopup.Instance.Show(text);
            Debug.Log(text);
        }
    }
}
