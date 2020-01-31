using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// SyncController can automatically synchronize the content from the source collection to the
    /// target collection.
    ///
    /// It can handle duplicate objects in the source collection. If an source item appears multiple
    /// times, only the first instance will be synchronized to the target collection.
    /// </summary>
    /// <typeparam name="TSource">The type of source collection item</typeparam>
    /// <typeparam name="TTarget">The type of target collection item</typeparam>
    /// <typeparam name="TSourceCollection">The type of source collection</typeparam>
    /// <typeparam name="TTargetCollection">The type of target collection</typeparam>
    public abstract class BaseSyncController<TSource, TTarget, TSourceCollection, TTargetCollection>
        where TSource : class
        where TTarget : class
        where TSourceCollection : INotifyCollectionChanged, IEnumerable
        where TTargetCollection : ICollection<TTarget>
    {
        protected TSourceCollection sourceCollection;
        protected TTargetCollection targetCollection;
        protected Func<TSource, TTarget> createTarget;
        protected Action<TTarget> destroyTarget;

        protected bool isSyncing;
        protected Dictionary<TSource, TTarget> sourceToTargetMap;
        protected Dictionary<TTarget, int> referenceCountDictionary;

        public bool IsSyncing { get { return isSyncing; } }

        /// <summary>
        /// BaseSyncController constructor.
        /// </summary>
        /// <param name="sourceCollection">The source collection for synchronizing</param>
        /// <param name="targetCollection">The target collection for synchronizing</param>
        /// <param name="createTarget">
        /// The delegate to be called when a source item is added. If the delegate is null, the
        /// controller will use type casting.
        /// </param>
        /// <param name="destroyTarget">
        /// The delegate to be called when a source item is removed. If the delegate is null, the
        /// controller will not call it.
        /// </param>
        public BaseSyncController(TSourceCollection sourceCollection, TTargetCollection targetCollection,
            Func<TSource, TTarget> createTarget, Action<TTarget> destroyTarget)
        {
            if (sourceCollection == null)
            {
                throw new ArgumentNullException("sourceCollection");
            }

            if (targetCollection == null)
            {
                throw new ArgumentNullException("targetCollection");
            }

            this.sourceCollection = sourceCollection;
            this.targetCollection = targetCollection;
            this.createTarget = createTarget;
            this.destroyTarget = destroyTarget;

            // create internal container
            sourceToTargetMap = new Dictionary<TSource, TTarget>();
        }

        /// <summary>
        /// Begins synchronizing.
        /// </summary>
        public void BeginSync()
        {
            if (isSyncing)
            {
                // already started
                return;
            }

            // register event
            sourceCollection.CollectionChanged += OnSourceCollectionChanged;

            // clear targets
            ClearItems();

            // add all
            var items = new ObjectCollection(sourceCollection);
            AddItems(items);

            // set flag
            isSyncing = true;
        }

        /// <summary>
        /// Ends synchronizing.
        /// </summary>
        /// <param name="clear">
        /// If true, all target items are cleared. Otherwise it will keep all target items.
        /// </param>
        public void EndSync(bool clear = true)
        {
            if (!isSyncing)
            {
                // already stopped
                return;
            }

            if (clear)
            {
                // clear target list
                ClearItems();
            }
            else
            {
                // clear internal container
                sourceToTargetMap.Clear();
                if (referenceCountDictionary != null)
                {
                    referenceCountDictionary.Clear();
                }
            }

            // unregister event
            sourceCollection.CollectionChanged -= OnSourceCollectionChanged;

            // reset flag
            isSyncing = false;
        }

        #region Handle collection events

        protected virtual void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
        }

        protected virtual void AddItems(ObjectCollection collection)
        {
            foreach (var item in collection)
            {
                // get TSource
                var source = GetSource(item);

                TTarget target;
                if (sourceToTargetMap.TryGetValue(source, out target))
                {
                    if (referenceCountDictionary == null)
                    {
                        // lazy initialization
                        InitReferenceCount();
                    }

                    // increase reference count
                    referenceCountDictionary[target]++;
                    continue;
                }

                // call create
                target = CreateTarget(source);

                // add it
                targetCollection.Add(target);
                sourceToTargetMap.Add(source, target);

                if (referenceCountDictionary != null)
                {
                    // set reference count to 1
                    referenceCountDictionary.Add(target, 1);
                }
            }

            if (referenceCountDictionary != null)
            {
                Assert.AreEqual(sourceToTargetMap.Count, referenceCountDictionary.Count);
            }
        }

        protected virtual void RemoveItems(ObjectCollection collection)
        {
            foreach (var item in collection)
            {
                // get TSource
                var source = GetSource(item);

                TTarget target;
                if (!sourceToTargetMap.TryGetValue(source, out target))
                {
                    Debug.LogErrorFormat("Unknown source {0}", source);
                    continue;
                }

                bool removeFlag = true;

                if (referenceCountDictionary != null)
                {
                    int count = 0;
                    if (!referenceCountDictionary.TryGetValue(target, out count))
                    {
                        Debug.LogErrorFormat("Unknown target {0}", target);
                        continue;
                    }

                    // decrease ref count
                    count--;
                    referenceCountDictionary[target] = count;

                    // check count
                    removeFlag = (count <= 0);
                }

                if (!removeFlag)
                {
                    continue;
                }

                // call destroy
                DestroyTarget(target);

                // remove it
                targetCollection.Remove(target);
                sourceToTargetMap.Remove(source);

                if (referenceCountDictionary != null)
                {
                    referenceCountDictionary.Remove(target);
                }
            }

            if (referenceCountDictionary != null)
            {
                Assert.AreEqual(sourceToTargetMap.Count, referenceCountDictionary.Count);
            }
        }

        protected virtual void ClearItems()
        {
            foreach (var item in targetCollection)
            {
                // call destroy
                DestroyTarget(item);
            }

            targetCollection.Clear();
            sourceToTargetMap.Clear();

            if (referenceCountDictionary != null)
            {
                referenceCountDictionary.Clear();
            }
        }

        protected virtual void ReplaceItems(ObjectCollection oldItems, ObjectCollection newItems)
        {
            RemoveItems(oldItems);
            AddItems(newItems);
        }

        protected virtual void MoveItems(ObjectCollection collection)
        {
            // do nothing
        }

        #endregion

        protected void InitReferenceCount()
        {
            referenceCountDictionary = new Dictionary<TTarget, int>();

            // set all reference count to 1
            foreach (var item in sourceToTargetMap)
            {
                referenceCountDictionary.Add(item.Value, 1);
            }
        }

        protected TTarget CreateTarget(TSource source)
        {
            TTarget target = null;

            if (createTarget != null)
            {
                // call func
                target = createTarget.Invoke(source);
            }
            else
            {
                // type casting
                target = (TTarget)((object)source);
            }

            return target;
        }

        protected void DestroyTarget(TTarget target)
        {
            if (destroyTarget != null)
            {
                // call action
                destroyTarget.Invoke(target);
            }
        }

        // Unbox TSource from collection item
        protected abstract TSource GetSource(object o);
    }
}