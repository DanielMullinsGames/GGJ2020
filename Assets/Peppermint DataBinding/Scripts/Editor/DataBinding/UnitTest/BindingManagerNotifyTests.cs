#if PEPPERMINT_UNIT_TEST

using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding.UnitTest
{
    [TestFixture]
    public class BindingManagerNotifyTests
    {
        public class ModelA
        {
            public int IntValue { get; set; }
        }

        public class ModelB
        {
            public ObservableList<ModelA> List { get; set; }

            public ModelB()
            {
                List = new ObservableList<ModelA>();
            }
        }

        public class ModelC
        {
            public class Nested
            {
                public int IntValue { get; set; }
            }

            public int IntValue { get; set; }
            public Nested NestedValue { get; set; }

            public ModelC()
            {
                NestedValue = new Nested();
            }
        }

        public class ModelD
        {
            public ObservableList<ModelC> List { get; set; }

            public ModelD()
            {
                List = new ObservableList<ModelC>();
            }
        }

        public class TestViewFactory : IViewFactory
        {
            public GameObject viewTemplate;
            public int nextIndex;
            public List<GameObject> viewList = new List<GameObject>();

            public TestViewFactory()
            {
                // create default view template
                viewTemplate = new GameObject("ItemViewTemplate", typeof(RectTransform), typeof(DataContext));
            }

            public GameObject CreateItemView(object item)
            {
                var go = GameObject.Instantiate(viewTemplate);

                viewList.Add(go);
                nextIndex++;
                return go;
            }

            public void ReleaseItemView(GameObject view)
            {
                viewList.Remove(view);
                GameObject.DestroyImmediate(view);
            }

            public void UpdateView()
            {

            }

            public void Clear()
            {
                foreach (var item in viewList)
                {
                    GameObject.DestroyImmediate(item);
                }

                GameObject.DestroyImmediate(viewTemplate);
            }

        }

        [Test]
        public void BindToProperty()
        {
            var source = new ModelA();
            var target = new ModelA();

            var binding = new Binding("IntValue", target, "IntValue");
            binding.Bind(source);

            source.IntValue = 2;
            Assert.AreEqual(0, target.IntValue);

            // notify property changed
            binding.HandleSourcePropertyChanged(source, "IntValue");
            Assert.AreEqual(2, target.IntValue);

            source.IntValue = 3;

            // notify with null property name
            binding.HandleSourcePropertyChanged(source, null);
            Assert.AreEqual(3, target.IntValue);
        }

        [Test]
        public void BindToNestedProperty()
        {
            var source = new ModelC();
            var target = new ModelA();

            // bind to nested property
            var binding = new Binding("NestedValue.IntValue", target, "IntValue");
            binding.Bind(source);

            source.NestedValue.IntValue = 2;
            Assert.AreEqual(0, target.IntValue);

            // notify nested property changed
            binding.HandleSourcePropertyChanged(source.NestedValue, "IntValue");
            Assert.AreEqual(2, target.IntValue);

            source.NestedValue.IntValue = 3;

            // notify with null property name
            binding.HandleSourcePropertyChanged(source.NestedValue, null);
            Assert.AreEqual(3, target.IntValue);
        }


        [Test]
        public void BindToCollection()
        {
            var source = new ModelB();
            var target = new TestViewFactory();

            // bind to collection
            var binding = new CollectionBinding("List", target);
            binding.Bind(source);

            Assert.AreEqual(0, target.viewList.Count);

            // add item
            source.List.Add(new ModelA());
            Assert.AreEqual(1, target.viewList.Count);
            Assert.AreEqual(1, target.nextIndex);

            // notify list changed
            binding.HandleSourcePropertyChanged(source, "List");

            Assert.AreEqual(2, target.nextIndex);
            Assert.AreEqual(1, target.viewList.Count);

            // notify with null property name
            binding.HandleSourcePropertyChanged(source, null);

            Assert.AreEqual(3, target.nextIndex);
            Assert.AreEqual(1, target.viewList.Count);

            target.Clear();
        }

        [Test]
        public void TestPropertyChanged()
        {
            var source = new ModelA();
            var target = new ModelA();

            var go = new GameObject();
            var dc = go.AddComponent<DataContext>();

            var binding = new Binding("IntValue", target, "IntValue");
            dc.AddBinding(binding);

            BindingManager.Instance.AddSource(source, "Test");
            BindingManager.Instance.AddDataContext(dc, "Test");

            // set value
            source.IntValue = 2;
            Assert.AreEqual(0, target.IntValue);

            BindingManager.Instance.NotifyPropertyChanged(source, "IntValue");

            // verify value changed
            Assert.AreEqual(2, target.IntValue);

            source.IntValue = 3;

            // notify with null path
            BindingManager.Instance.NotifyPropertyChanged(source, null);
            Assert.AreEqual(3, target.IntValue);

            GameObject.DestroyImmediate(go);
            BindingManager.Instance.Clear();
        }

        [Test]
        public void TestNestedPropertyChanged()
        {
            var source = new ModelC();
            var target = new ModelA();

            var go = new GameObject();
            var dc = go.AddComponent<DataContext>();

            var binding = new Binding("NestedValue.IntValue", target, "IntValue");
            dc.AddBinding(binding);

            BindingManager.Instance.AddSource(source, "Test");
            BindingManager.Instance.AddDataContext(dc, "Test");

            // set value
            source.NestedValue.IntValue = 2;
            Assert.AreEqual(0, target.IntValue);

            // notify source
            BindingManager.Instance.NotifyPropertyChanged(source, "IntValue");
            Assert.AreEqual(0, target.IntValue);

            // notify nested
            BindingManager.Instance.NotifyPropertyChanged(source, source.NestedValue, "IntValue");
            Assert.AreEqual(2, target.IntValue);

            source.NestedValue.IntValue = 3;

            // notify source with null path
            BindingManager.Instance.NotifyPropertyChanged(source, null);
            Assert.AreEqual(2, target.IntValue);

            // notify nested with null path
            BindingManager.Instance.NotifyPropertyChanged(source, source.NestedValue, null);
            Assert.AreEqual(3, target.IntValue);

            GameObject.DestroyImmediate(go);
            BindingManager.Instance.Clear();
        }

        [Test]
        public void TestPropertyChangedBoth()
        {
            var source = new ModelC();
            var target = new ModelC();

            var go = new GameObject();
            var dc = go.AddComponent<DataContext>();

            var bindingA = new Binding("IntValue", target, "IntValue");
            var bindingB = new Binding("NestedValue.IntValue", target, "NestedValue.IntValue");
            dc.AddBinding(bindingA);
            dc.AddBinding(bindingB);

            BindingManager.Instance.AddSource(source, "Test");
            BindingManager.Instance.AddDataContext(dc, "Test");

            // set value
            source.IntValue = 20;
            source.NestedValue.IntValue = 2;
            Assert.AreEqual(0, target.IntValue);
            Assert.AreEqual(0, target.NestedValue.IntValue);

            // notify source
            BindingManager.Instance.NotifyPropertyChanged(source, "IntValue");
            Assert.AreEqual(20, target.IntValue);
            Assert.AreEqual(0, target.NestedValue.IntValue);

            // notify nested
            source.IntValue = 30;
            source.NestedValue.IntValue = 3;
            BindingManager.Instance.NotifyPropertyChanged(source, source.NestedValue, "IntValue");
            Assert.AreEqual(20, target.IntValue);
            Assert.AreEqual(3, target.NestedValue.IntValue);

            // notify source with null path
            source.IntValue = 40;
            source.NestedValue.IntValue = 4;
            BindingManager.Instance.NotifyPropertyChanged(source, null);
            Assert.AreEqual(40, target.IntValue);
            Assert.AreEqual(3, target.NestedValue.IntValue);

            // notify nested with null path
            source.IntValue = 50;
            source.NestedValue.IntValue = 5;
            BindingManager.Instance.NotifyPropertyChanged(source, source.NestedValue, null);
            Assert.AreEqual(40, target.IntValue);
            Assert.AreEqual(5, target.NestedValue.IntValue);

            GameObject.DestroyImmediate(go);
            BindingManager.Instance.Clear();
        }

        [Test]
        public void TestItemPropertyChanged()
        {
            var source = new ModelB();
            var target = new TestViewFactory();

            var go = new GameObject("Test");
            var dc = go.AddComponent<DataContext>();

            // create collection binding
            var binding = new CollectionBinding("List", target);
            dc.AddBinding(binding);

            BindingManager.Instance.AddSource(source, "Test");
            BindingManager.Instance.AddDataContext(dc, "Test");

            // add item
            var item = new ModelA();
            item.IntValue = 2;
            source.List.Add(item);

            var itemView = binding.BindingDictionary[item];
            var itemDataContext = itemView.GetComponent<IDataContext>();

            // add item binding
            var itemTarget = new ModelA();
            var itemBinding = new Binding("IntValue", itemTarget, "IntValue");
            itemDataContext.AddBinding(itemBinding);

            // check item value
            Assert.AreEqual(2, itemTarget.IntValue);

            // update value
            item.IntValue = 3;
            Assert.AreEqual(2, itemTarget.IntValue);

            // notify
            BindingManager.Instance.NotifyItemPropertyChanged(source, source.List, item, "IntValue");
            Assert.AreEqual(3, itemTarget.IntValue);

            // update value
            item.IntValue = 4;
            Assert.AreEqual(3, itemTarget.IntValue);

            // notify with null
            BindingManager.Instance.NotifyItemPropertyChanged(source, source.List, item, null);
            Assert.AreEqual(4, itemTarget.IntValue);

            BindingManager.Instance.Clear();
            target.Clear();
            GameObject.DestroyImmediate(go);
        }

        [Test]
        public void TestItemNestedPropertyChanged()
        {
            var source = new ModelD();
            var target = new TestViewFactory();

            var go = new GameObject("Test");
            var dc = go.AddComponent<DataContext>();

            // create collection binding
            var binding = new CollectionBinding("List", target);
            dc.AddBinding(binding);

            BindingManager.Instance.AddSource(source, "Test");
            BindingManager.Instance.AddDataContext(dc, "Test");

            // add item
            var item = new ModelC();
            item.NestedValue.IntValue = 2;
            source.List.Add(item);

            var itemView = binding.BindingDictionary[item];
            var itemDataContext = itemView.GetComponent<IDataContext>();

            // add item binding
            var itemTarget = new ModelA();
            var itemBinding = new Binding("NestedValue.IntValue", itemTarget, "IntValue");
            itemDataContext.AddBinding(itemBinding);

            // check item value
            Assert.AreEqual(2, itemTarget.IntValue);

            // update value
            item.NestedValue.IntValue = 3;
            Assert.AreEqual(2, itemTarget.IntValue);

            // notify
            BindingManager.Instance.NotifyItemPropertyChanged(source, source, source.List, item, item.NestedValue, "IntValue");
            Assert.AreEqual(3, itemTarget.IntValue);

            // update value
            item.NestedValue.IntValue = 4;
            Assert.AreEqual(3, itemTarget.IntValue);

            // notify with null
            BindingManager.Instance.NotifyItemPropertyChanged(source, source, source.List, item, item.NestedValue, null);
            Assert.AreEqual(4, itemTarget.IntValue);

            BindingManager.Instance.Clear();
            target.Clear();
            GameObject.DestroyImmediate(go);
        }
    }
}

#endif