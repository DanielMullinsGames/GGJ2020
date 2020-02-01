using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// Demonstrate how to use DataContextBinder for nested source.
    ///
    /// The DataContextBinderExample class contains a complex item hierarchy that is difficult to
    /// setup with nested property bindings. In this example, each Item contains two text binders
    /// that are bound to the "Name" and "Counter" properties. For example, to bind to the "Name" of
    /// "ItemC", you need to specify the source path to "NestedItem.NestedItem.NestedItem.Name". You
    /// also need to add this long-nested property prefix to "Counter" and all child binders, which
    /// is error-prone and difficult to maintain.
    ///
    /// You can simplify the UI setup with nested DataContext and DataContextBinder. You only need to
    /// specify the nested source path in each DataContextBinder, and all child binders can simply
    /// use property name as source path.
    ///
    /// The DataContextBinder can also handle nested source changes automatically and you do not need
    /// to handle it manually.
    /// </summary>
    public class DataContextBinderExample : BindableMonoBehaviour
    {
        public class Item : BindableObject
        {
            private string name;
            private int counter;
            private Item nestedItem;

            #region Bindable Properties

            public string Name
            {
                get { return name; }
                set { SetProperty(ref name, value, "Name"); }
            }

            public int Counter
            {
                get { return counter; }
                set { SetProperty(ref counter, value, "Counter"); }
            }

            public Item NestedItem
            {
                get { return nestedItem; }
                set { SetProperty(ref nestedItem, value, "NestedItem"); }
            }

            #endregion

            public Item(string name)
            {
                this.name = name;
            }
        }

        private Item nestedItem;
        private ICommand updateCounterCommand;
        private ICommand changeItemACommand;
        private ICommand changeItemBCommand;
        private ICommand changeItemCCommand;

        #region Bindable Properties

        public Item NestedItem
        {
            get { return nestedItem; }
            set { SetProperty(ref nestedItem, value, "NestedItem"); }
        }

        public ICommand UpdateCounterCommand
        {
            get { return updateCounterCommand; }
            set { SetProperty(ref updateCounterCommand, value, "UpdateCounterCommand"); }
        }

        public ICommand ChangeItemACommand
        {
            get { return changeItemACommand; }
            set { SetProperty(ref changeItemACommand, value, "ChangeItemACommand"); }
        }

        public ICommand ChangeItemBCommand
        {
            get { return changeItemBCommand; }
            set { SetProperty(ref changeItemBCommand, value, "ChangeItemBCommand"); }
        }

        public ICommand ChangeItemCCommand
        {
            get { return changeItemCCommand; }
            set { SetProperty(ref changeItemCCommand, value, "ChangeItemCCommand"); }
        }

        #endregion

        void Start()
        {
            // create items
            var itemA = new Item("ItemA");
            var itemB = new Item("ItemB");
            var itemC = new Item("ItemC");

            // setup hierarchy
            itemB.NestedItem = itemC;
            itemA.NestedItem = itemB;
            NestedItem = itemA;

            // create commands
            updateCounterCommand = new DelegateCommand(UpdateCounter);
            changeItemACommand = new DelegateCommand(ChangeItemA);
            changeItemBCommand = new DelegateCommand(ChangeItemB);
            changeItemCCommand = new DelegateCommand(ChangeItemC);

            BindingManager.Instance.AddSource(this, typeof(DataContextBinderExample).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        void UpdateCounter()
        {
            NestedItem.Counter++;
            NestedItem.NestedItem.Counter += 10;
            nestedItem.NestedItem.NestedItem.Counter += 100;
        }

        void ChangeItemA()
        {
            var newItem = new Item("ItemA");
            newItem.NestedItem = new Item("ItemB");
            newItem.NestedItem.NestedItem = new Item("ItemC");

            NestedItem = newItem;
        }

        void ChangeItemB()
        {
            var newItem = new Item("ItemB");
            newItem.NestedItem = new Item("ItemC");

            NestedItem.NestedItem = newItem;
        }

        void ChangeItemC()
        {
            var newItem = new Item("ItemC");

            NestedItem.NestedItem.NestedItem = newItem;
        }
    }
}
