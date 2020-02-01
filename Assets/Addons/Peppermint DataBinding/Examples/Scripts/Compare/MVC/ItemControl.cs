using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding.Example.Mvc
{
    /// <summary>
    /// UI control for InventoryModel.ItemModel
    /// </summary>
    public class ItemControl : MonoBehaviour
    {
        public Text nameText;
        public Text amountText;
        public Button clickButton;
        public Image backgroundImage;

        public Color selectedColor;
        public Color normalColor;

        private InventoryModel.ItemModel item;
        private bool selected;

        public bool Selected
        {
            get { return selected; }

            set
            {
                selected = value;
                UpdateBackgroundColor();
            }
        }

        void Start()
        {

        }

        void OnDestroy()
        {
            // remove property changed handlers
            item.PropertyChanged -= OnItemModelPropertyChanged;
        }

        void RefreshView()
        {
            nameText.text = item.Name;
            amountText.text = item.Amount.ToString();

            UpdateBackgroundColor();
        }

        void UpdateBackgroundColor()
        {
            if (selected)
            {
                backgroundImage.color = selectedColor;
            }
            else
            {
                backgroundImage.color = normalColor;
            }
        }

        void OnItemModelPropertyChanged(object sender, string propertyName)
        {
            if (propertyName == "Amount")
            {
                amountText.text = item.Amount.ToString();
            }
        }

        public void Init(InventoryModel.ItemModel item)
        {
            this.item = item;

            RefreshView();

            // add property changed handlers
            item.PropertyChanged += OnItemModelPropertyChanged;
        }
    }
}
