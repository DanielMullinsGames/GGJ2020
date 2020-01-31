using System;
using System.Collections.Generic;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Sync controller for ObservableList.
    /// </summary>
    /// <typeparam name="TSource">The type of source ObservableList item</typeparam>
    /// <typeparam name="TTarget">The type of target ObservableList item</typeparam>
    public class ListSyncController<TSource, TTarget> : BaseSyncController<TSource, TTarget, ObservableList<TSource>, ObservableList<TTarget>>
        where TSource : class
        where TTarget : class
    {
        protected List<TSource> tempList;

        /// <summary>
        /// ListSyncController constructor.
        /// </summary>
        /// <param name="sourceList">The source ObservableList for synchronizing</param>
        /// <param name="targetList">The target ObservableList for synchronizing</param>
        /// <param name="createTarget">
        /// The delegate to be called when a source item is added. If the delegate is null, the
        /// controller will use type casting.
        /// </param>
        /// <param name="destroyTarget">
        /// The delegate to be called when a source item is removed. If the delegate is null, the
        /// controller will not call it.
        /// </param>
        public ListSyncController(ObservableList<TSource> sourceList, ObservableList<TTarget> targetList,
            Func<TSource, TTarget> createTarget, Action<TTarget> destroyTarget = null)
            : base(sourceList, targetList, createTarget, destroyTarget)
        {
            // create temp list
            tempList = new List<TSource>();
        }

        protected override TSource GetSource(object o)
        {
            // unbox source
            var source = (TSource)o;
            return source;
        }

        protected override void ReplaceItems(ObjectCollection oldItems, ObjectCollection newItems)
        {
            // call base
            base.ReplaceItems(oldItems, newItems);

            UpdateTargetOrder();
        }

        protected override void MoveItems(ObjectCollection collection)
        {
            // call base
            base.MoveItems(collection);

            UpdateTargetOrder();
        }

        protected void UpdateTargetOrder()
        {
            BindingUtility.SyncListOrder(sourceCollection, targetCollection, sourceToTargetMap, tempList);
        }
    }
}
