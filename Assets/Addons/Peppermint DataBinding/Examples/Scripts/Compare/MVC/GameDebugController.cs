using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding.Example.Mvc
{
    /// <summary>
    /// GameDebug controller.
    /// </summary>
    public class GameDebugController : MonoBehaviour
    {
        public int hpDelta;
        public int mpDelta;
        public float upgradeScale;
        public int defaultAmount;
        public InventoryController inventoryController;

        public GameObject miniInfoPanel;
        public GameObject detailInfoPanel;
        public GameObject inventoryPanel;

        public Button hpIncButton;
        public Button hpDecButton;
        public Button mpIncButton;
        public Button mpDecButton;
        public Button addItemButton;
        public Button removeItemButton;
        public Button upgradeButton;
        public Button resetButton;
        public Button toggleMiniInfoButton;
        public Button toggleDetailInfoButton;
        public Button toggleInventoryButton;

        private PlayerModel player;
        private InventoryModel inventory;

        private int itemIndex;

        void Awake()
        {
            // keep reference
            player = GameState.Instance.player;
            inventory = GameState.Instance.inventory;

            ResetState();

            // add button listeners
            hpIncButton.onClick.AddListener(() => UpdateHP(hpDelta));
            hpDecButton.onClick.AddListener(() => UpdateHP(-hpDelta));
            mpIncButton.onClick.AddListener(() => UpdateMP(mpDelta));
            mpDecButton.onClick.AddListener(() => UpdateMP(-mpDelta));
            addItemButton.onClick.AddListener(AddItem);
            removeItemButton.onClick.AddListener(RemoveItem);
            upgradeButton.onClick.AddListener(Upgrade);
            resetButton.onClick.AddListener(ResetState);
            toggleMiniInfoButton.onClick.AddListener(ToggleMiniInfo);
            toggleDetailInfoButton.onClick.AddListener(ToggleDetailInfo);
            toggleInventoryButton.onClick.AddListener(ToggleInventory);
        }

        void OnDestroy()
        {
            // remove button listeners
            hpIncButton.onClick.RemoveAllListeners();
            hpDecButton.onClick.RemoveAllListeners();
            mpIncButton.onClick.RemoveAllListeners();
            mpDecButton.onClick.RemoveAllListeners();
            addItemButton.onClick.RemoveListener(AddItem);
            removeItemButton.onClick.RemoveListener(RemoveItem);
            upgradeButton.onClick.RemoveListener(Upgrade);
            resetButton.onClick.RemoveListener(ResetState);
            toggleMiniInfoButton.onClick.RemoveListener(ToggleMiniInfo);
            toggleDetailInfoButton.onClick.RemoveListener(ToggleDetailInfo);
            toggleInventoryButton.onClick.RemoveListener(ToggleInventory);
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

            // call wrapper method
            inventory.AddItem(item);
        }

        public void RemoveItem()
        {
            // remove last one
            var item = inventory.ItemList.LastOrDefault();
            if (item == null)
            {
                return;
            }

            // call wrapper method
            inventory.RemoveItem(item);
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
            inventory.ClearItems();
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
            var newState = !miniInfoPanel.activeSelf;
            miniInfoPanel.SetActive(newState);
        }

        public void ToggleDetailInfo()
        {
            var newState = !detailInfoPanel.activeSelf;
            detailInfoPanel.SetActive(newState);
        }

        public void ToggleInventory()
        {
            var newState = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(newState);

            if (!newState)
            {
                // clear selected item
                inventoryController.SelectItem(null);
            }
        }
    }
}
