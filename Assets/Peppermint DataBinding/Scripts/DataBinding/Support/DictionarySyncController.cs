using System;
using System.Collections.Generic;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Sync controller for ObservableDictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of source ObservableDictionary's key</typeparam>
    /// <typeparam name="TSource">The type of source ObservableDictionary's value</typeparam>
    /// <typeparam name="TTarget">The type of target ObservableList item</typeparam>
    public class DictionarySyncController<TKey, TSource, TTarget> : BaseSyncController<TSource, TTarget, ObservableDictionary<TKey, TSource>, ObservableList<TTarget>>
        where TSource : class
        where TTarget : class
    {
        protected Func<TTarget, TTarget, int> sortTarget;

        /// <summary>
        /// DictionarySyncController constructor.
        /// </summary>
        /// <param name="dictionary">The source ObservableDictionary for synchronizing</param>
        /// <param name="targetList">The target ObservableList for synchronizing</param>
        /// <param name="sortTarget">The delegate to decide the order of target items.</param>
        /// <param name="createTarget">
        /// The delegate to be called when a source item is added. If the delegate is null, the
        /// controller will use type casting.
        /// </param>
        /// <param name="destroyTarget">
        /// The delegate to be called when a source item is removed. If the delegate is null, the
        /// controller will not call it.
        /// </param>
        public DictionarySyncController(ObservableDictionary<TKey, TSource> dictionary, ObservableList<TTarget> targetList,
            Func<TTarget, TTarget, int> sortTarget, Func<TSource, TTarget> createTarget, Action<TTarget> destroyTarget = null)
            : base(dictionary, targetList, createTarget, destroyTarget)
        {
            if (sortTarget == null)
            {
                throw new ArgumentNullException("sortTarget");
            }

            this.sortTarget = sortTarget;
        }

        protected override TSource GetSource(object o)
        {
            // unbox and return value
            var kvp = (KeyValuePair<TKey, TSource>)o;
            return kvp.Value;
        }

        protected override void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // call base
            base.OnSourceCollectionChanged(sender, e);

            // sort
            SortTargets();
        }

        private void SortTargets()
        {
            // sort list
            targetCollection.Sort(Compare);
        }

        private int Compare(TTarget lhs, TTarget rhs)
        {
            // call delegate
            int result = sortTarget.Invoke(lhs, rhs);
            return result;
        }
    }
}