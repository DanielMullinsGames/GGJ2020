using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding.Example.Mvc
{
    /// <summary>
    /// Inventory UI controller.
    /// </summary>
    public class InventoryController : MonoBehaviour
    {
        public GameObject viewPrefab;
        public RectTransform container;
        public Text countText;
        public Button useItemButton;
        public Button sortListButton;
        public Button shuffleListButton;

        private Dictionary<InventoryModel.ItemModel, ItemControl> controlDictionary;
        private List<InventoryModel.ItemModel> pendingList;

        private InventoryModel.ItemModel selectedItem;
        private InventoryModel inventory;

        void Start()
        {
            // keep reference
            inventory = GameState.Instance.inventory;

            // add button listener
            useItemButton.onClick.AddListener(UseItem);
            sortListButton.onClick.AddListener(SortList);
            shuffleListButton.onClick.AddListener(ShuffleList);

            // create containers
            controlDictionary = new Dictionary<InventoryModel.ItemModel, ItemControl>();
            pendingList = new List<InventoryModel.ItemModel>();

            RefreshView();

            // add property changed handlers
            inventory.PropertyChanged += OnInventoryPropertyChanged;
        }

        void OnDestroy()
        {
            // remove property changed handlers
            inventory.PropertyChanged -= OnInventoryPropertyChanged;

            // remove button listener
            useItemButton.onClick.RemoveListener(UseItem);
            sortListButton.onClick.RemoveListener(SortList);
            shuffleListButton.onClick.RemoveListener(ShuffleList);
        }

        void RefreshList()
        {
            // sync collection
            BindingUtility.SyncListToDictionary(inventory.ItemList, controlDictionary, pendingList, AddItemView, RemoveItemView);

            // update order
            BindingUtility.SyncViewOrder(inventory.ItemList, controlDictionary, pendingList);
            
            // workaround for update grid view
            LayoutRebuilder.MarkLayoutForRebuild(container);

            if (selectedItem != null)
            {
                // check if selected item is removed
                if (!controlDictionary.ContainsKey(selectedItem))
                {
                    SelectItem(null);
                }
            }
        }

        void RefreshView()
        {
            RefreshList();

            // update UI
            countText.text = inventory.ItemList.Count.ToString();

            // set interactable
            useItemButton.interactable = (selectedItem != null);
        }

        void AddItemView(InventoryModel.ItemModel item)
        {
            // create view
            var view = Instantiate<GameObject>(viewPrefab);

            // attach to container
            view.transform.SetParent(container, false);
            view.transform.SetAsLastSibling();

            // activate view
            if (!view.activeSelf)
            {
                view.SetActive(true);
            }

            // initialize control
            var control = view.GetComponent<ItemControl>();
            control.Init(item);

            // add button listener
            control.clickButton.onClick.AddListener(() => SelectItem(item));

            // add to dictionary
            controlDictionary.Add(item, control);
        }

        void RemoveItemView(InventoryModel.ItemModel item)
        {
            // get control
            var control = controlDictionary[item];

            // remove button listener
            control.clickButton.onClick.RemoveAllListeners();

            // destroy view
            Destroy(control.gameObject);

            // remove from dictionary
            controlDictionary.Remove(item);
        }

        private void OnInventoryPropertyChanged(object sender, string propertyName)
        {
            if (propertyName == "ItemList")
            {
                // force refresh the whole scroll view
                RefreshList();
            }
            else if (propertyName == "ItemList.Count")
            {
                countText.text = inventory.ItemList.Count.ToString();
            }
        }

        public void SelectItem(InventoryModel.ItemModel item)
        {
            Debug.LogFormat("SelectItem, item={0}", (item != null) ? item.Name : "null");

            selectedItem = item;

            // disable all
            foreach (var kvp in controlDictionary)
            {
                var control = kvp.Value;
                control.Selected = false;
            }

            if (selectedItem != null)
            {
                var selectedControl = controlDictionary[item];
                selectedControl.Selected = true;
            }

            // update UI
            useItemButton.interactable = (selectedItem != null);
        }

        public void UseItem()
        {
            Debug.LogFormat("UseItem, item={0}", selectedItem.Name);

            // update amount
            selectedItem.Amount--;

            if (selectedItem.Amount <= 0)
            {
                // remove item if amount is zero
                inventory.RemoveItem(selectedItem);
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

            RefreshList();
        }

        public void ShuffleList()
        {
            Debug.LogFormat("ShuffleList");

            inventory.ItemList.Shuffle();

            RefreshList();
        }
    }
}
