using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// Demonstrate how to handle nested source changes.
    ///
    /// In peppermint data binding, you can bind nested members in two different ways: nested
    /// property and encapsulated property. If the nested source does not change, you can simply
    /// follow the NestedClassExample. If the nested source will change, this example will show you
    /// how to handle nested source changes properly.
    ///
    /// For encapsulated property, you need to add/remove event handler and raise property changed
    /// events. This is the most efficient way, because the view does not need to rebind any binders.
    ///
    /// For nested property, you need to rebind the source if any nested source changes. The view
    /// will rebind all binders, which is only recommended for simple object. Alternatively, you can
    /// setup nested DataContext and DataContextBinder. When you change a nested source, the view
    /// will only rebind the associated binders, which is more efficient.
    ///
    /// In general, the view model class should encapsulate the underlying data structure and make it
    /// easy for data binding. Directly bind to complex internal data structure is not recommended.
    /// </summary>
    public class ChangeNestedSourceExample : BindableMonoBehaviour
    {
        public class Item : BindableObject
        {
            private int counter;

            #region Bindable Properties

            public int Counter
            {
                get { return counter; }
                set { SetProperty(ref counter, value, "Counter"); }
            }

            #endregion
        }

        public class NestedPropertyExample : BindableObject
        {
            private int counter;
            private Item nestedItem;

            #region Bindable Properties

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
        }

        public class EncapsulatedPropertyExample : BindableObject
        {
            private int counter;
            private Item nestedItem;

            #region Bindable Properties

            public int Counter
            {
                get { return counter; }
                set { SetProperty(ref counter, value, "Counter"); }
            }

            public int NestedItemCounter
            {
                get { return nestedItem.Counter; }
                set { nestedItem.Counter = value; }
            }

            #endregion

            public Item NestedItem
            {
                get { return nestedItem; }

                set
                {
                    if (nestedItem != null)
                    {
                        // remove event handler
                        nestedItem.PropertyChanged -= NestedItem_PropertyChanged;
                    }

                    nestedItem = value;

                    if (nestedItem != null)
                    {
                        // add event handler
                        nestedItem.PropertyChanged += NestedItem_PropertyChanged;
                    }

                    NotifyPropertyChanged("NestedItemCounter");
                }
            }

            private void NestedItem_PropertyChanged(object sender, string propertyName)
            {
                if (propertyName == "Counter")
                {
                    NotifyPropertyChanged("NestedItemCounter");
                }
            }
        }

        private NestedPropertyExample instanceA;
        private EncapsulatedPropertyExample instanceB;
        private NestedPropertyExample instanceC;

        private ICommand updateCounterCommand;
        private ICommand changeNestedItemCommand;

        #region Bindable Properties

        public ICommand UpdateCounterCommand
        {
            get { return updateCounterCommand; }
            set { SetProperty(ref updateCounterCommand, value, "UpdateCounterCommand"); }
        }

        public ICommand ChangeNestedItemCommand
        {
            get { return changeNestedItemCommand; }
            set { SetProperty(ref changeNestedItemCommand, value, "ChangeNestedItemCommand"); }
        }

        #endregion

        void Start()
        {
            // create instances
            instanceA = new NestedPropertyExample();
            instanceB = new EncapsulatedPropertyExample();
            instanceC = new NestedPropertyExample();
            instanceA.NestedItem = new Item();
            instanceB.NestedItem = new Item();
            instanceC.NestedItem = new Item();

            // create commands
            updateCounterCommand = new DelegateCommand(UpdateCounter);
            changeNestedItemCommand = new DelegateCommand(ChangeNestedItem);

            BindingManager.Instance.AddSource(instanceA, "InstanceA");
            BindingManager.Instance.AddSource(instanceB, "InstanceB");
            BindingManager.Instance.AddSource(instanceC, "InstanceC");
            BindingManager.Instance.AddSource(this, typeof(ChangeNestedSourceExample).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(instanceA);
            BindingManager.Instance.RemoveSource(instanceB);
            BindingManager.Instance.RemoveSource(instanceC);
            BindingManager.Instance.RemoveSource(this);
        }

        private void ChangeNestedItem()
        {
            // using nested property
            instanceA.NestedItem = new Item();
            RebindSource(instanceA, "InstanceA");

            // using encapsulated property
            instanceB.NestedItem = new Item();

            // using nested DataContext and DataContextBinder
            instanceC.NestedItem = new Item();
        }

        public void UpdateCounter()
        {
            instanceA.Counter++;
            instanceA.NestedItem.Counter += 10;

            instanceB.Counter++;
            instanceB.NestedItemCounter += 10;

            instanceC.Counter++;
            instanceC.NestedItem.Counter += 10;
        }

        private void RebindSource(object instance, string sourceName)
        {
            // rebind source
            BindingManager.Instance.RemoveSource(instance);
            BindingManager.Instance.AddSource(instance, sourceName);
        }
    }
}
