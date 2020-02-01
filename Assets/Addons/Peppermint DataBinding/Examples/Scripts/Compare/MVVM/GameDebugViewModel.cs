using System.Linq;
using UnityEngine;

namespace Peppermint.DataBinding.Example.Mvvm
{
    public class GameDebugViewModel : BindableMonoBehaviour
    {
        public int hpDelta;
        public int mpDelta;
        public float upgradeScale;
        public int defaultAmount;
        public InventoryViewModel inventoryVM;

        private bool showMiniInfo;
        private bool showDetailInfo;
        private bool showInventory;

        private ICommand increaseHPCommand;
        private ICommand decreaseHPCommand;
        private ICommand increaseMPCommand;
        private ICommand decreaseMPCommand;
        private ICommand addItemCommand;
        private ICommand removeItemCommand;
        private ICommand resetCommand;
        private ICommand upgradeCommand;
        private ICommand toggleMiniInfoCommand;
        private ICommand toggleDetailInfoCommand;
        private ICommand toggleInventoryCommand;

        private PlayerModel player;
        private InventoryModel inventory;
        private int itemIndex;

        #region Bindable Properties

        public bool ShowMiniInfo
        {
            get { return showMiniInfo; }
            set { SetProperty(ref showMiniInfo, value, "ShowMiniInfo"); }
        }

        public bool ShowDetailInfo
        {
            get { return showDetailInfo; }
            set { SetProperty(ref showDetailInfo, value, "ShowDetailInfo"); }
        }

        public bool ShowInventory
        {
            get { return showInventory; }
            set { SetProperty(ref showInventory, value, "ShowInventory"); }
        }

        public ICommand IncreaseHPCommand
        {
            get { return increaseHPCommand; }
            set { SetProperty(ref increaseHPCommand, value, "IncreaseHPCommand"); }
        }

        public ICommand DecreaseHPCommand
        {
            get { return decreaseHPCommand; }
            set { SetProperty(ref decreaseHPCommand, value, "DecreaseHPCommand"); }
        }

        public ICommand IncreaseMPCommand
        {
            get { return increaseMPCommand; }
            set { SetProperty(ref increaseMPCommand, value, "IncreaseMPCommand"); }
        }

        public ICommand DecreaseMPCommand
        {
            get { return decreaseMPCommand; }
            set { SetProperty(ref decreaseMPCommand, value, "DecreaseMPCommand"); }
        }

        public ICommand AddItemCommand
        {
            get { return addItemCommand; }
            set { SetProperty(ref addItemCommand, value, "AddItemCommand"); }
        }

        public ICommand RemoveItemCommand
        {
            get { return removeItemCommand; }
            set { SetProperty(ref removeItemCommand, value, "RemoveItemCommand"); }
        }

        public ICommand ResetCommand
        {
            get { return resetCommand; }
            set { SetProperty(ref resetCommand, value, "ResetCommand"); }
        }

        public ICommand UpgradeCommand
        {
            get { return upgradeCommand; }
            set { SetProperty(ref upgradeCommand, value, "UpgradeCommand"); }
        }

        public ICommand ToggleMiniInfoCommand
        {
            get { return toggleMiniInfoCommand; }
            set { SetProperty(ref toggleMiniInfoCommand, value, "ToggleMiniInfoCommand"); }
        }

        public ICommand ToggleDetailInfoCommand
        {
            get { return toggleDetailInfoCommand; }
            set { SetProperty(ref toggleDetailInfoCommand, value, "ToggleDetailInfoCommand"); }
        }

        public ICommand ToggleInventoryCommand
        {
            get { return toggleInventoryCommand; }
            set { SetProperty(ref toggleInventoryCommand, value, "ToggleInventoryCommand"); }
        }

        #endregion

        void Awake()
        {
            // keep reference
            player = GameState.Instance.player;
            inventory = GameState.Instance.inventory;

            ResetState();
        }

        void Start()
        {
            // create commands
            increaseHPCommand = new DelegateCommand(() => UpdateHP(hpDelta));
            decreaseHPCommand = new DelegateCommand(() => UpdateHP(-hpDelta));
            increaseMPCommand = new DelegateCommand(() => UpdateMP(mpDelta));
            decreaseMPCommand = new DelegateCommand(() => UpdateMP(-mpDelta));
            addItemCommand = new DelegateCommand(AddItem);
            removeItemCommand = new DelegateCommand(RemoveItem);
            upgradeCommand = new DelegateCommand(Upgrade);
            resetCommand = new DelegateCommand(ResetState);
            toggleMiniInfoCommand = new DelegateCommand(ToggleMiniInfo);
            toggleDetailInfoCommand = new DelegateCommand(ToggleDetailInfo);
            toggleInventoryCommand = new DelegateCommand(ToggleInventory);

            // add current as source
            BindingManager.Instance.AddSource(this, typeof(GameDebugViewModel).Name);

            // add player and inventory as source
            BindingManager.Instance.AddSource(player, typeof(PlayerModel).Name);
            BindingManager.Instance.AddSource(inventory, typeof(InventoryModel).Name);
        }

        void OnDestroy()
        {
            // remove source
            BindingManager.Instance.RemoveSource(this);
            BindingManager.Instance.RemoveSource(player);
            BindingManager.Instance.RemoveSource(inventory);
        }

        public void UpdateHP(int amount)
        {
            var newValue = player.Hp + amount;
            player.Hp = Mathf.Clamp(newValue, 0, player.HpMax);
        }

        public void UpdateMP(int amount)
        {
            var newValue = player.Mp + amount;
            player.Mp = Mathf.Clamp(newValue, 0, player.MpMax);
        }

        public void AddItem()
        {
            // create item model
            var name = string.Format("{0:D02}", itemIndex++);
            var item = new InventoryModel.ItemModel(0, name, defaultAmount);

            inventory.ItemList.Add(item);
        }

        public void RemoveItem()
        {
            // remove last one
            var item = inventory.ItemList.LastOrDefault();
            if (item == null)
            {
                return;
            }

            inventory.ItemList.Remove(item);
        }

        public void ResetState()
        {
            player.Hp = 100;
            player.HpMax = 100;
            player.Mp = 200;
            player.MpMax = 200;
            player.Gold = 999;
            player.Exp = 1000;
            player.Level = 1;

            itemIndex = 0;
            inventory.ItemList.Clear();
        }

        public void Upgrade()
        {
            player.HpMax += (int)(player.HpMax * upgradeScale);
            player.MpMax += (int)(player.MpMax * upgradeScale);
            player.Gold += (int)(player.Gold * upgradeScale);
            player.Exp += (int)(player.Exp * upgradeScale);
            player.Level++;

            player.Hp = player.HpMax;
            player.Mp = player.MpMax;
        }

        public void ToggleMiniInfo()
        {
            ShowMiniInfo = !ShowMiniInfo;
        }

        public void ToggleDetailInfo()
        {
            ShowDetailInfo = !ShowDetailInfo;
        }

        public void ToggleInventory()
        {
            ShowInventory = !ShowInventory;

            if (!ShowInventory)
            {
                // clear selected item
                inventoryVM.SelectItem(null);
            }
        }
    }
}
