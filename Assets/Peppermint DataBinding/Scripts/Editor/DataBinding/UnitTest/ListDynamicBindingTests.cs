#if PEPPERMINT_UNIT_TEST

using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding.UnitTest
{
    [TestFixture]
    public class ListDynamicBindingTests
    {
        public class ModelA : BindableObject
        {
            private int intValue;

            public int IntValue
            {
                get { return intValue; }
                set { SetProperty(ref intValue, value, "IntValue"); }
            }
        }

        public class ModelB
        {
            public ObservableList<ModelA> List { get; set; }

            public ModelB()
            {
                List = new ObservableList<ModelA>();
            }
        }

        public class TestViewFactory : IDynamicViewFactory
        {
            private Func<List<object>> getDynamicItemsFun;
            private List<GameObject> viewList;
            private Transform parentNode;

            public int ViewCount { get { return viewList.Count; } }

            public Func<List<object>> GetDynamicItemsFun
            {
                get { return getDynamicItemsFun; }
                set { getDynamicItemsFun = value; }
            }

            public TestViewFactory()
            {
                viewList = new List<GameObject>();

                // create container node
                var go = new GameObject("Container");
                go.AddComponent<RectTransform>();
                parentNode = go.transform;
            }

            public GameObject CreateItemView(object item)
            {
                var go = new GameObject();
                go.AddComponent<RectTransform>();
                go.AddComponent<DataContext>();

                viewList.Add(go);

                // add as child
                go.transform.SetParent(parentNode, false);

                return go;
            }

            public void ReleaseItemView(GameObject view)
            {
                // remove view
                viewList.Remove(view);

                GameObject.DestroyImmediate(view);
            }

            public void Destroy()
            {
                foreach (var item in viewList)
                {
                    GameObject.DestroyImmediate(item);
                }

                GameObject.DestroyImmediate(parentNode.gameObject);
            }

            public void GetDynamicItems(List<object> itemList)
            {
                itemList.Clear();

                if (getDynamicItemsFun != null)
                {
                    var list = getDynamicItemsFun.Invoke();
                    itemList.AddRange(list);
                }
            }

            public void UpdateView()
            {
                
            }
        }


        [Test]
        public void AddAndRemoveTest()
        {
            var source = new ModelB();
            var target = new TestViewFactory();

            // bind to collection
            var binding = new ListDynamicBinding("List", target);
            binding.Bind(source);

            // setup function
            target.GetDynamicItemsFun = () =>
            {
                var list = new List<object>();
                foreach (var item in source.List)
                {
                    list.Add(item);    
                }
                return list;
            };

            Assert.AreEqual(0, target.ViewCount);

            // add item
            source.List.Add(new ModelA());
            Assert.AreEqual(1, target.ViewCount);

            // add item
            source.List.Add(new ModelA());
            Assert.AreEqual(2, target.ViewCount);

            // remove item
            source.List.RemoveAt(0);
            Assert.AreEqual(1, target.ViewCount);

            // add item
            source.List.Add(new ModelA());
            Assert.AreEqual(2, target.ViewCount);

            // clear
            source.List.Clear();
            Assert.AreEqual(0, target.ViewCount);

            target.Destroy();
        }

        [Test]
        public void UpdateItemsTest()
        {
            var source = new ModelB();
            var target = new TestViewFactory();

            // bind to collection
            var binding = new ListDynamicBinding("List", target);
            binding.Bind(source);

            Assert.AreEqual(0, target.ViewCount);

            // add item
            source.List.Add(new ModelA());
            source.List.Add(new ModelA());
            Assert.AreEqual(0, target.ViewCount);

            target.GetDynamicItemsFun = () =>
            {
                var list = new List<object>();
                list.Add(source.List[0]);
                return list;
            };

            // update binding
            binding.UpdateDynamicList();
            Assert.AreEqual(1, target.ViewCount);

            target.GetDynamicItemsFun = () =>
            {
                var list = new List<object>();
                list.Add(source.List[0]);
                list.Add(source.List[1]);
                return list;
            };

            // update binding
            binding.UpdateDynamicList();
            Assert.AreEqual(2, target.ViewCount);

            target.Destroy();
        }

        [Test]
        public void OrderTest()
        {
            var source = new ModelB();
            var target = new TestViewFactory();

            // setup function
            target.GetDynamicItemsFun = () =>
            {
                var list = new List<object>();
                foreach (var item in source.List)
                {
                    list.Add(item);
                }
                return list;
            };

            // bind to collection
            var binding = new ListDynamicBinding("List", target);
            binding.Bind(source);

            Assert.AreEqual(0, target.ViewCount);

            // add item
            source.List.Add(new ModelA());
            source.List.Add(new ModelA());

            var sourceA = source.List[0];
            var sourceB = source.List[1];

            var viewA = binding.BindingDictionary[sourceA];
            Assert.AreEqual(0, viewA.transform.GetSiblingIndex());

            var viewB = binding.BindingDictionary[sourceB];
            Assert.AreEqual(1, viewB.transform.GetSiblingIndex());

            // switch items
            source.List[0] = sourceB;
            source.List[1] = sourceA;

            viewA = binding.BindingDictionary[sourceA];
            Assert.AreEqual(1, viewA.transform.GetSiblingIndex());

            viewB = binding.BindingDictionary[sourceB];
            Assert.AreEqual(0, viewB.transform.GetSiblingIndex());

            target.Destroy();
        }

        [Test]
        public void DuplicateItemTest()
        {
            var source = new ModelB();
            var target = new TestViewFactory();

            // setup function
            target.GetDynamicItemsFun = () =>
            {
                var list = new List<object>();
                foreach (var item in source.List)
                {
                    list.Add(item);
                }
                return list;
            };

            // bind to collection
            var binding = new ListDynamicBinding("List", target);
            binding.Bind(source);

            // check initial state
            {
                Assert.AreEqual(0, target.ViewCount);
            }

            var model = new ModelA();

            // add item
            {
                source.List.Add(model);
                source.List.Add(model);

                // verify view count
                Assert.AreEqual(1, binding.BindingDictionary.Count);
            }

            // remove item
            {
                source.List.RemoveAt(0);
                Assert.AreEqual(1, binding.BindingDictionary.Count);
            }

            // remove item
            {
                source.List.RemoveAt(0);
                Assert.AreEqual(0, binding.BindingDictionary.Count);
            }

            target.Destroy();
        }

        [Test]
        public void ListMethodTest()
        {
            var source = new ModelB();
            var target = new TestViewFactory();

            // setup function
            target.GetDynamicItemsFun = () =>
            {
                var list = new List<object>();
                foreach (var item in source.List)
                {
                    list.Add(item);
                }
                return list;
            };

            // bind to collection
            var binding = new ListDynamicBinding("List", target);
            binding.Bind(source);

            Assert.AreEqual(0, target.ViewCount);

            // add item
            source.List.Add(new ModelA() { IntValue = 1 });
            source.List.Add(new ModelA() { IntValue = 2 });
            source.List.Add(new ModelA() { IntValue = 3 });

            // do shuffle
            TestUtility.Shuffle(source.List);

            // do sort
            source.List.Sort((x, y) => x.IntValue.CompareTo(y.IntValue));

            target.Destroy();
        }

        [Test]
        public void UpdateItemValueTest()
        {
            var source = new ModelB();
            var target = new TestViewFactory();

            // setup function
            target.GetDynamicItemsFun = () =>
            {
                var list = new List<object>();
                foreach (var item in source.List)
                {
                    list.Add(item);
                }
                return list;
            };

            // bind to collection
            var binding = new ListDynamicBinding("List", target);
            binding.Bind(source);

            Assert.AreEqual(0, target.ViewCount);

            // add item
            var itemA = new ModelA();
            source.List.Add(itemA);

            // add item
            var itemB = new ModelA();
            source.List.Add(itemB);

            // add item view A
            var viewA = new ModelA();
            var bindingA = new Binding("IntValue", viewA, "IntValue");
            var dcA = binding.BindingDictionary[itemA].GetComponent<IDataContext>();
            dcA.AddBinding(bindingA);

            // add item view B
            var viewB = new ModelA();
            var bindingB = new Binding("IntValue", viewB, "IntValue");
            var dcB = binding.BindingDictionary[itemB].GetComponent<IDataContext>();
            dcB.AddBinding(bindingB);

            // verify state
            Assert.IsTrue(bindingA.IsBound);
            Assert.IsTrue(bindingB.IsBound);

            Assert.AreEqual(1, dcA.BindingList.Count);
            Assert.AreEqual(1, dcB.BindingList.Count);

            Assert.AreEqual(0, viewA.IntValue);
            Assert.AreEqual(0, viewB.IntValue);

            // update value
            itemA.IntValue = 1;
            itemB.IntValue = 2;

            Assert.AreEqual(1, viewA.IntValue);
            Assert.AreEqual(2, viewB.IntValue);

            target.Destroy();
        }
    }
}

#endif