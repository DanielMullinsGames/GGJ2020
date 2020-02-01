using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Binding for collection object.
    ///
    /// CollectionBinding can handle duplicate objects in collection. If an object appears multiple
    /// times, only the first instance appears in the view.
    /// </summary>
    public class CollectionBinding : IBinding
    {
        // Accessor
        public class Accessor : ICollectionBindingAccessor
        {
            private CollectionBinding owner;

            #region ICollectionBindingAccessor

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
                        return owner.bindingDictionary.Count;
                    }
                    else
                    {
                        return owner.collectionView.Count;
                    }
                }
            }

            #endregion

            public Accessor(CollectionBinding owner)
            {
                this.owner = owner;
            }
        }

        protected object source;
        protected string sourcePath;
        protected string sourcePropertyName;

        protected IEnumerable itemsSource;
        protected IViewFactory factory;
        protected CollectionView collectionView;
        protected Dictionary<object, GameObject> bindingDictionary;
        protected Dictionary<GameObject, int> viewReferenceCountDictionary;
        protected ICollectionBindingAccessor collectionBindingAccessor;

        public bool IsBound
        {
            get { return itemsSource != null; }
        }

        public object Source
        {
            get { return source; }
        }

        public IEnumerable ItemsSource
        {
            get { return itemsSource; }
        }

        public Dictionary<object, GameObject> BindingDictionary
        {
            get { return bindingDictionary; }
        }

        public ICollectionView CollectionView
        {
            get { return collectionView; }
        }

        public ICollectionBindingAccessor CollectionBindingAccessor
        {
            get
            {
                if (collectionBindingAccessor == null)
                {
                    // lazy initialize accessor
                    collectionBindingAccessor = new Accessor(this);
                }

                return collectionBindingAccessor;
            }
        }

        public CollectionBinding(string sourcePath, IViewFactory factory)
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

            // check if collection object is IEnumerable
            IEnumerable collectionSource = collectionObject as IEnumerable;
            if (collectionSource == null)
            {
                var msg = string.Format("Binding failed, collection source is not IEnumerable object. source={0}, sourcePath={1}, collectionSource={2}",
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

            AddItems(itemsSource);

            UpdateCollectionView();
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

            UpdateCollectionView();
            UpdateView();

            // clear source
            source = null;
            itemsSource = null;
        }

        public void HandleSourcePropertyChanged(object sender, string propertyName)
        {
            OnSourcePropertyChanged(sender, propertyName);
        }

        public void CreateCollectionView()
        {
            // create view
            collectionView = new CollectionView(this);
        }

        public void UpdateView()
        {
            factory.UpdateView();
        }

        public IDataContext GetDataContext(object item)
        {
            // get view
            GameObject view;
            if (!bindingDictionary.TryGetValue(item, out view))
            {
                return null;
            }

            var dc = view.GetComponent<IDataContext>();
            return dc;
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
            IEnumerable newItemsSource = collectionObject as IEnumerable;
            if (newItemsSource == null)
            {
                Debug.LogError("OnPropertyChanged failed, collectionObject is not IEnumerable", factory as UnityEngine.Object);
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

            UpdateCollectionView();
            UpdateView();
        }

        protected void ChangeItemsSource(IEnumerable newItemsSource)
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

            AddItems(itemsSource);
        }

        protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItems(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveItems(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    ReplaceItems(e.OldItems, e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Move:
                    MoveItems(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    ClearItems();
                    break;

                default:
                    throw new ArgumentException("Unhandled action " + e.Action);
            }

            UpdateCollectionView();
            UpdateView();
        }

        protected void InitViewReferenceCount()
        {
            viewReferenceCountDictionary = new Dictionary<GameObject, int>();

            // set all reference count to 1
            foreach (var item in bindingDictionary)
            {
                viewReferenceCountDictionary.Add(item.Value, 1);
            }
        }

        protected void AddItems(IEnumerable items)
        {
            var collection = new ObjectCollection(items);
            AddItems(collection);
        }

        protected void AddItems(ObjectCollection collection)
        {
            // create view items
            foreach (var item in collection)
            {
                GameObject view;
                if (bindingDictionary.TryGetValue(item, out view))
                {
                    if (viewReferenceCountDictionary == null)
                    {
                        // lazy initialization
                        InitViewReferenceCount();
                    }

                    // increase reference count
                    viewReferenceCountDictionary[view]++;

                    continue;
                }

                // create view
                view = factory.CreateItemView(item);

                // set source
                var dataContext = view.GetComponent<IDataContext>();
                dataContext.Source = item;

                // add to dictionary
                bindingDictionary.Add(item, view);

                if (viewReferenceCountDictionary != null)
                {
                    // set reference count to 1
                    viewReferenceCountDictionary.Add(view, 1);
                }
            }

            if (viewReferenceCountDictionary != null)
            {
                Assert.AreEqual(bindingDictionary.Count, viewReferenceCountDictionary.Count);
            }
        }

        protected void RemoveItems(ObjectCollection collection)
        {
            foreach (var item in collection)
            {
                // get view
                GameObject view;
                if (!bindingDictionary.TryGetValue(item, out view))
                {
                    Debug.LogErrorFormat("Unknown item {0}", item);
                    continue;
                }

                bool removeFlag = true;

                if (viewReferenceCountDictionary != null)
                {
                    int count = 0;
                    if (!viewReferenceCountDictionary.TryGetValue(view, out count))
                    {
                        Debug.LogErrorFormat("Unknown view {0}", view);
                        continue;
                    }

                    // decrease ref count
                    count--;
                    viewReferenceCountDictionary[view] = count;

                    // check if count is zero
                    removeFlag = (count <= 0);
                }

                if (!removeFlag)
                {
                    continue;
                }

                // remove view

                // unbind source
                var dataContext = view.GetComponent<IDataContext>();
                dataContext.Source = null;

                // destroy view
                factory.ReleaseItemView(view);

                // remove from dictionary
                bindingDictionary.Remove(item);

                if (viewReferenceCountDictionary != null)
                {
                    viewReferenceCountDictionary.Remove(view);
                }
            }

            if (viewReferenceCountDictionary != null)
            {
                Assert.AreEqual(bindingDictionary.Count, viewReferenceCountDictionary.Count);
            }
        }

        protected void ClearItems()
        {
            foreach (var item in bindingDictionary)
            {
                // get view
                GameObject view = item.Value;

                // unbind source
                var dataContext = view.GetComponent<IDataContext>();
                dataContext.Source = null;

                // destroy item
                factory.ReleaseItemView(view);
            }

            bindingDictionary.Clear();

            if (viewReferenceCountDictionary != null)
            {
                viewReferenceCountDictionary.Clear();
            }
        }

        protected void ResetItems()
        {
            ClearItems();
            AddItems(itemsSource);
        }

        protected void ReplaceItems(ObjectCollection oldItems, ObjectCollection newItems)
        {
            RemoveItems(oldItems);
            AddItems(newItems);

            ResetViewOrder(itemsSource);
        }

        protected void MoveItems(ObjectCollection collection)
        {
            ResetViewOrder(collection);
        }

        protected void ResetViewOrder(IEnumerable items)
        {
            var collection = new ObjectCollection(items);
            ResetViewOrder(collection);
        }

        protected void ResetViewOrder(ObjectCollection collection)
        {
            int index = 0;
            foreach (var item in collection)
            {
                // get view
                GameObject view;
                if (!bindingDictionary.TryGetValue(item, out view))
                {
                    Debug.LogErrorFormat("Unknown item {0}", item);
                    continue;
                }

                view.transform.SetSiblingIndex(index++);
            }
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