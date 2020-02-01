#if PEPPERMINT_UNIT_TEST

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Peppermint.DataBinding.UnitTest
{
    [TestFixture]
    public class ObjectCollectionTests
    {
        private void CheckMode(ObjectCollection collection, ObjectCollection.Enumerator.Mode mode)
        {
            var fieldInfo = typeof(ObjectCollection.Enumerator).GetField("mode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var enumerator = collection.GetEnumerator();
            var value = fieldInfo.GetValue(enumerator);

            Assert.AreEqual(mode, value);
        }

        [Test]
        public void TestEmpty()
        {
            var collection = new ObjectCollection();
            CheckMode(collection, ObjectCollection.Enumerator.Mode.None);

#pragma warning disable 168
            int index = 0;
            foreach (var item in collection)
            {
                index++;
            }
#pragma warning restore 168

            Assert.AreEqual(0, index);
        }

        [Test]
        public void TestSingleItem()
        {
            object o = 99;
            var list = new List<object>(new object[] { o });

            var collection = new ObjectCollection(o);
            CheckMode(collection, ObjectCollection.Enumerator.Mode.SingleItem);

            int index = 0;
            foreach (var item in collection)
            {
                var v = list[index++];
                Assert.AreEqual(v, item);
            }
            Assert.AreEqual(1, index);
        }

        [Test]
        public void TestList()
        {
            var list = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                var v = string.Format("Item {0}", i);
                list.Add(v);
            }

            var collection = new ObjectCollection(list);
            CheckMode(collection, ObjectCollection.Enumerator.Mode.ItemList);

            int index = 0;
            foreach (var item in collection)
            {
                var v = list[index++];
                Assert.AreEqual(v, item);
            }
            Assert.AreEqual(list.Count, index);
        }

        [Test]
        public void TestDictionary()
        {
            var dic = new Dictionary<int, string>();
            for (int i = 0; i < 10; i++)
            {
                var v = string.Format("Item {0}", i);
                dic.Add(i, v);
            }

            var collection = new ObjectCollection(dic);
            CheckMode(collection, ObjectCollection.Enumerator.Mode.ItemEnumerator);

            int index = 0;
            foreach (var item in collection)
            {
                var k = index;
                var v = dic[index++];
                var kvp = new KeyValuePair<int, string>(k, v);

                Assert.AreEqual(kvp, item);
            }
            Assert.AreEqual(dic.Count, index);
        }

        [Test]
        public void TestEnumerable()
        {
            var list = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(i);
            }

            var linq = list.Where(x => x % 2 == 0);
            var resultList = linq.ToList();
            var collection = new ObjectCollection(linq);
            CheckMode(collection, ObjectCollection.Enumerator.Mode.ItemEnumerator);

            int index = 0;
            foreach (var item in collection)
            {
                var v = resultList[index++];
                Assert.AreEqual(v, item);
            }
            Assert.AreEqual(5, index);
        }

        [Test]
        public void TestEnumerator()
        {
            var list = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(i);
            }

            var collection = new ObjectCollection(list);

#pragma warning disable 168
            using (var e = collection.GetEnumerator())
            {
                Assert.Throws<InvalidOperationException>(() => { var v = e.Current; });

                int index = 0;
                while (e.MoveNext())
                {
                    var a = e.Current;
                    var b = list[index++];

                    Assert.AreEqual(b, a);
                }
                Assert.AreEqual(list.Count, index);

                Assert.Throws<InvalidOperationException>(() => { var v = e.Current; });

                // reset
                index = 0;
                e.Reset();

                // iterate again
                while (e.MoveNext())
                {
                    var a = e.Current;
                    var b = list[index++];

                    Assert.AreEqual(b, a);
                }
                Assert.AreEqual(list.Count, index);
            }
#pragma warning restore 168
        }
    }
}

#endif