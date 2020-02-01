using System.Collections;
using System.Linq;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to work with multiple binding source objects.
    ///
    /// In this demo, one binding source is the viewer itself, and the other is an "information
    /// panel". In MVVM pattern, you focus on the Model or ViewModel, manipulate the value of
    /// objects. The data binding will handle the view updates for you.
    /// </summary>
    public class DocumentViewer : BindableMonoBehaviour
    {
        /// <summary>
        /// Item has a nested POCO data and additional fields which are used in view model. You need
        /// to create properties for POCO to make it support data binding.
        /// </summary>
        public class Item : BindableObject
        {
            private DocumentData.Item data;
            private string description;
            private int index;
            private bool selected;
            private ICommand selectCommand;

            public Item(DocumentData.Item data)
            {
                this.data = data;

                // replace <br> with new line
                description = data.description.Replace("<br>", "\n");
            }

            #region Bindable Properties

            public ComponentCategory Category
            {
                get { return data.category; }
            }

            public string Name
            {
                get { return data.name; }
            }

            public string IconName
            {
                get { return data.iconName; }
            }

            public string Description
            {
                get { return description; }
            }

            public int Index
            {
                get { return index; }
                set { SetProperty(ref index, value, "Index"); }
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
        }

        /// <summary>
        /// ItemInformation is a binding source, it controls the information panel.
        /// </summary>
        public class ItemInformation : BindableObject
        {
            private AnimatorTrigger switchTrigger = new AnimatorTrigger();
            private string name;
            private string description;
            private ComponentCategory? category;

            #region Bindable Properties

            public AnimatorTrigger SwitchTrigger
            {
                get { return switchTrigger; }
                set { SetProperty(ref switchTrigger, value, "SwitchTrigger"); }
            }

            public string Name
            {
                get { return name; }
                set { SetProperty(ref name, value, "Name"); }
            }

            public string Description
            {
                get { return description; }
                set { SetProperty(ref description, value, "Description"); }
            }

            public ComponentCategory? Category
            {
                get { return category; }
                set { SetProperty(ref category, value, "Category"); }
            }

            #endregion

            public void UpdateInfo(Item selectedItem)
            {
                Name = selectedItem.Name;
                Description = selectedItem.Description;
                Category = selectedItem.Category;
            }
        }

        public DocumentData documentData;
        public float fadeOutDuration;

        private ObservableList<Item> itemList;
        private ItemInformation itemInformation;
        private Item selectedItem;

        #region Bindable Properties

        public ObservableList<Item> ItemList
        {
            get { return itemList; }
            set { SetProperty(ref itemList, value, "ItemList"); }
        }

        #endregion

        void Start()
        {
            LoadData();

            BindingManager.Instance.AddSource(itemInformation, typeof(ItemInformation).Name);
            BindingManager.Instance.AddSource(this, typeof(DocumentViewer).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(itemInformation);
            BindingManager.Instance.RemoveSource(this);
        }

        private void LoadData()
        {
            itemList = new ObservableList<Item>();
            itemInformation = new ItemInformation();

            int index = 0;
            foreach (var item in documentData.list)
            {
                // create model
                var model = new Item(item);
                model.Index = index++;
                
                // set command
                model.SelectCommand = new DelegateCommand(() => SelectItem(model));

                // add to list
                itemList.Add(model);
            }

            // select first one
            SelectItem(itemList.First(), false);
        }

        public void SelectItem(Item target, bool enableAnimation = true)
        {
            if (selectedItem == target)
            {
                return;
            }

            // update selected
            foreach (var item in itemList)
            {
                if (item == target)
                {
                    item.Selected = true;
                }
                else
                {
                    item.Selected = false;
                }
            }

            // save current selected
            selectedItem = target;

            if (enableAnimation)
            {
                StartCoroutine(SwitchTargetAsync());
            }
            else
            {
                itemInformation.UpdateInfo(selectedItem);
            }
        }

        private IEnumerator SwitchTargetAsync()
        {
            // trigger animation
            itemInformation.SwitchTrigger.SetTrigger();

            // wait for completely fade out
            yield return new WaitForSeconds(fadeOutDuration);

            // update info
            itemInformation.UpdateInfo(selectedItem);
        }
    }
}
