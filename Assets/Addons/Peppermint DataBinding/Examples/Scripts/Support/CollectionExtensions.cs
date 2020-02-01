using System.Collections.Generic;

namespace Peppermint.DataBinding.Example
{
    public static class CollectionExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            var observableList = list as ObservableList<T>;

            if (observableList != null)
            {
                // disable notifying
                observableList.IsNotifying = false;
            }

            DoShuffle<T>(list);

            if (observableList != null)
            {
                // enable notify and notify move event
                observableList.IsNotifying = true;
                observableList.NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, observableList));
            }
        }

        private static void DoShuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);

                // swap
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
