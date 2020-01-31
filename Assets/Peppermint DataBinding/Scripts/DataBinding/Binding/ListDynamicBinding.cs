using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Binding for list object.
    ///
    /// The ListDynamicBinding binds to IList object. It will not bind all item objects, it will
    /// dynamically select the objects need to be bound. It can reduce memory footprint and
    /// improve performance for large list.
    /// </summary>
    public class ListDynamicBinding : IBinding
    {
        // Accessor
        public class Accessor : IDynamicBindingAccessor
        {
            private ListDynamicBinding owner;

            #region IDynamicAccessor

            public bool IsBound
            {
                get { return owner.IsBound; }
            }

            public int ItemCount
            {
                get
                {
                    if (owner.collectionView == null)
                    {
                        return owner.itemsSource.Count;
                    }
                    else
                    {
                        return owner.collectionView.ModifiedList.Count;
                    }
                }
            }

            public object GetItem(int index)
            {
                if (owner.collectionView == null)
                {
                    return owner.itemsSource[index];
                }
                else
                {
                    return owner.collectionView.ModifiedList[index];
                }
            }

            public void UpdateDynamicList()
            {
                owner.UpdateDynamicList();
            }

            #endregion

            public Accessor(ListDynamicBinding owner)
            {
                this.owner = owner;
            }
        }

        protected object source;
        protected string sourcePath;
        protected string sourcePropertyName;

        protected IList itemsSource;
        protected IDynamicViewFactory factory;
        protected IDynamicBindingAccessor dynamicBindingAccessor;
        protected DynamicCollectionView collectionView;
        protected Dictionary<object, GameObject> bindingDictionary;
        protected List<object> pendingList;
        protected List<object> dynamicItemList;
        
        public bool IsBound
        {
            get { return itemsSource != null; }
        }

        public object Source
        {
            get { return source; }
        }

        public IList ItemsSource
        {
            get { return itemsSource; }
        }

        public Dictionary<object, GameObject> BindingDictionary
        {
            get { return bindingDictionary; }
        }

        public IDynamicBindingAccessor DynamicBindingAccessor
        {
            get
            {
                if (dynamicBindingAccessor == null)
                {
                    // lazy initialize accessor
                    dynamicBindingAccessor = new Accessor(this);
                }

                return dynamicBindingAccessor;
            }
        }

        public ICollectionView CollectionView
        {
            get { return collectionView; }
        }

        public ListDynamicBinding(string sourcePath, IDynamicViewFactory factory)
        {
            if (factory == null)
            {
                Debug.LogError("factory is null");
                return;
            }

            if (string.IsNullOrEmpty(sourcePath))
            {
                Debug.LogError("sourcePath is null", factory as UnityEngine.Object);
                return;
            }

            this.factory = factory;

            // setup source
            this.sourcePath = sourcePath;
            this.sourcePropertyName = BindingUtility.GetPropertyName(sourcePath);

            bindingDictionary = new Dictionary<object, GameObject>();
            pendingList = new List<object>();
            dynamicItemList = new List<object>();
        }

        public void Bind(object source)
        {
            if (source == null)
            {
                Debug.LogError("source is null", factory as UnityEngine.Object);
                return;
            }

            if (IsBound)
            {
                Unbind();
            }

            // handle nested property
            var bindingSource = BindingUtility.GetBindingObject(source, sourcePath);
            if (bindingSource == null)
            {
                Debug.LogError("bindingSource is null", factory as UnityEngine.Object);
                return;
            }

            // get collection object
            var collectionObject = BindingUtility.GetPropertyValue(bindingSource, sourcePropertyName);
            if (collectionObject == null)
            {
                Debug.LogError("Binding failed, collectionObject is null", factory as UnityEngine.Object);
                return;
            }

            // check if collection object is IList
            IList collectionSource = collectionObject as IList;
            if (collectionSource == null)
            {
                var msg = string.Format("Binding failed, collection source is not IList object. source={0}, sourcePath={1}, collectionSource={2}",
                    source, sourcePath, collectionObject.GetType());

                Debug.LogError(msg, factory as UnityEngine.Object);
                return;
            }

            // save source
            this.source = bindingSource;
            this.itemsSource = collectionSource;

            // register event
            var notifyCollection = itemsSource as INotifyCollectionChanged;
            if (notifyCollection != null)
            {
                notifyCollection.CollectionChanged += OnCollectionChanged;
            }

            // register property event
            var notifyInterface = this.source as INotifyPropertyChanged;
            if (notifyInterface != null)
            {
                notifyInterface.PropertyChanged += OnSourcePropertyChanged;
            }

            ResetItems();

            UpdateView();
        }

        public void Unbind()
        {
            if (!IsBound)
            {
                return;
            }

            // unregister event
            var notifyCollection = itemsSource as INotifyCollectionChanged;
            if (notifyCollection != null)
            {
                notifyCollection.CollectionChanged -= OnCollectionChanged;
            }

            // unregister property event
            var notifyInterface = source as INotifyPropertyChanged;
            if (notifyInterface != null)
            {
                notifyInterface.PropertyChanged -= OnSourcePropertyChanged;
            }

            ClearItems();

            UpdateView();

            // clear source
            source = null;
            itemsSource = null;
        }

        public void CreateCollectionView()
        {
            // create view
            collectionView = new DynamicCollectionView(this);
        }

        public void UpdateDynamicList()
        {
            if (!IsBound)
            {
                return;
            }

            // get dynamic list
            factory.GetDynamicItems(dynamicItemList);

            BindingUtility.SyncListToDictionary(dynamicItemList, bindingDictionary, pendingList, AddItem, RemoveItem);

            BindingUtility.SyncViewOrder(dynamicItemList, bindingDictionary, pendingList);
        }

        public void UpdateView()
        {
            factory.UpdateView();
        }

        public void HandleSourcePropertyChanged(object sender, string propertyName)
        {
            OnSourcePropertyChanged(sender, propertyName);
        }

        protected void OnSourcePropertyChanged(object sender, string propertyName)
        {
            Assert.IsTrue(IsBound);

            if (sender != source)
            {
                Debug.LogWarningFormat("Invalid sender {0}:{1}", sender, sender.GetHashCode());
                return;
            }

            if (propertyName != null && sourcePropertyName != propertyName)
            {
                // ignore invalid source path
                return;
            }

            // get collection object
            var collectionObject = BindingUtility.GetPropertyValue(source, sourcePropertyName);
            if (collectionObject == null)
            {
                Debug.LogError("OnPropertyChanged failed, collectionObject is null", factory as UnityEngine.Object);
                return;
            }

            // check if collection object is IEnumerable
            IList newItemsSource = collectionObject as IList;
            if (newItemsSource == null)
            {
                Debug.LogError("OnPropertyChanged failed, collectionObject is not IList", factory as UnityEngine.Object);
                return;
            }

            // check if itemsSource is changed
            if (itemsSource != newItemsSource)
            {
                ChangeItemsSource(newItemsSource);
            }
            else
            {
                // itemsSource is not changed, reset items
                ResetItems();
            }

            UpdateView();
        }

        protected void ChangeItemsSource(IList newItemsSource)
        {
            // unregister event
            var notifyCollection = itemsSource as INotifyCollectionChanged;
            if (notifyCollection != null)
            {
                notifyCollection.CollectionChanged -= OnCollectionChanged;
            }

            ClearItems();

            // update
            itemsSource = newItemsSource;

            // register event
            notifyCollection = itemsSource as INotifyCollectionChanged;
            if (notifyCollection != null)
            {
                notifyCollection.CollectionChanged += OnCollectionChanged;
            }

            CreateItems();
        }

        protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    break;

                case NotifyCollectionChangedAction.Remove:
                    break;

                case NotifyCollectionChangedAction.Replace:
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;

                default:
                    throw new ArgumentException("Unhandled action " + e.Action);
            }

            ResetItems();

            UpdateView();
        }

        protected void ClearItems()
        {
            // clear all
            foreach (var kvp in bindingDictionary)
            {
                var view = kvp.Value;

                // unbind source
                var dataContext = view.GetComponent<IDataContext>();
                dataContext.Source = null;

                // destroy view
                factory.ReleaseItemView(view);
            }

            bindingDictionary.Clear();
        }

        protected void CreateItems()
        {
            UpdateCollectionView();

            // get dynamic list
            factory.GetDynamicItems(dynamicItemList);

            foreach (var item in dynamicItemList)
            {
                AddItem(item);
            }
        }

        protected void ResetItems()
        {
            UpdateCollectionView();

            UpdateDynamicList();
        }

        protected void AddItem(object item)
        {
            GameObject view;
            if (bindingDictionary.TryGetValue(item, out view))
            {
                return;
            }

            // create view
            view = factory.CreateItemView(item);

            // bind source
            var dataContext = view.GetComponent<IDataContext>();
            dataContext.Source = item;

            // add to dictionary
            bindingDictionary.Add(item, view);
        }

        protected void RemoveItem(object item)
        {
            var view = bindingDictionary[item];

            // unbind source
            var dataContext = view.GetComponent<IDataContext>();
            dataContext.Source = null;

            // destroy view
            factory.ReleaseItemView(view);

            // remove from dictionary
            bindingDictionary.Remove(item);
        }

        protected void UpdateCollectionView()
        {
            if (collectionView != null)
            {
                collectionView.Apply();
            }
        }
    }
}
