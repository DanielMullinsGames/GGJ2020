#if PEPPERMINT_UNIT_TEST

using System.Collections.Generic;

namespace Peppermint.DataBinding.UnitTest
{
    public static class TestUtility
    {
        public static void Shuffle<T>(IList<T> list)
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

#endif