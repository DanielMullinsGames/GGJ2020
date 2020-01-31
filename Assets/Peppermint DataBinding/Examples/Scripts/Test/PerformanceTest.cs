using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// A simple performance test.
    /// </summary>
    public class PerformanceTest : MonoBehaviour
    {
        public class ModelA : BindableObject
        {
            private int intValue;
            private string stringValue;

            #region Bindable Properties

            public int IntValue
            {
                get { return intValue; }
                set { SetProperty(ref intValue, value, "IntValue"); }
            }

            public string StringValue
            {
                get { return stringValue; }
                set { SetProperty(ref stringValue, value, "StringValue"); }
            }

            #endregion
        }

        public class ViewA
        {
            public int IntValue { get; set; }
            public string StringValue { get; set; }
        }

        public class ModelB : BindableObject
        {
            private int intValue;
            private float floatValue;

            #region Bindable Properties

            public int IntValue
            {
                get { return intValue; }
                set { SetProperty(ref intValue, value, "IntValue"); }
            }

            public float FloatValue
            {
                get { return floatValue; }
                set { SetProperty(ref floatValue, value, "FloatValue"); }
            }

            #endregion
        }

        public class ViewB
        {
            public string TextA { get; set; }
            public string TextB { get; set; }
        }

        public Button button;
        public Text logText;
        public int bindingTestCount = 10000;
        public int propertyTestCount = 10000;
        public int loopTestCount = 1000;
        public int innerLoopCount = 1000;
        public int collectionTestCount = 10000;

        private StringBuilder buffer;

        void Awake()
        {
            buffer = new StringBuilder();
            Application.logMessageReceived += OnLogMessageReceived;

            button.onClick.AddListener(RunTest);
        }

        void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        private void ResetLogInfo()
        {
            // reset log info
            logText.text = string.Empty;
            buffer.Length = 0;
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            buffer.AppendLine(condition);
            logText.text = buffer.ToString();
        }

        private void RunTest()
        {
            ResetLogInfo();

            StartCoroutine(RunTestAsync());
        }

        private IEnumerator RunTestAsync()
        {
            yield return StartCoroutine(StartupTest());

            if (bindingTestCount > 0)
            {
                yield return StartCoroutine(BindAndUnbindTest());
                yield return StartCoroutine(NotifyTestA1());
                yield return StartCoroutine(NotifyTestA2());
                yield return StartCoroutine(NotifyTestB1());
                yield return StartCoroutine(NotifyTestB2());
            }

            if (propertyTestCount > 0)
            {
                yield return StartCoroutine(PropertyTestA());
                yield return StartCoroutine(PropertyTestB());
            }

            if (loopTestCount > 0)
            {
                yield return StartCoroutine(LoopTestA1());
                yield return StartCoroutine(LoopTestA2());
            }

            if (collectionTestCount > 0)
            {
                yield return StartCoroutine(CollectionTestA1());
            }
        }

        private IEnumerator StartupTest()
        {
            var test = RunActionAsync("StartupTest", 1, () =>
            {
#pragma warning disable 168
                var bm = BindingManager.Instance;
                var cache = TypeCache.Instance;
                var vcp = ValueConverterProvider.Instance;
#pragma warning restore 168
            });

            yield return StartCoroutine(test);
        }

        private IEnumerator BindAndUnbindTest()
        {
            var model = new ModelA();
            var view = new ViewA();
            var bindingList = new List<Binding>();

            var test = RunActionAsync("BindAndUnbindTest", bindingTestCount, () =>
            {
                // bind
                for (int i = 0; i < bindingTestCount; i++)
                {
                    var binding = new Binding("IntValue", view, "IntValue");
                    binding.Bind(model);
                    bindingList.Add(binding);

                    binding = new Binding("StringValue", view, "StringValue");
                    binding.Bind(model);
                    bindingList.Add(binding);
                }

                // unbind
                foreach (var item in bindingList)
                {
                    item.Unbind();
                }
            });

            yield return StartCoroutine(test);
        }

        private IEnumerator NotifyTestA1()
        {
            var model = new ModelA();
            var bindingList = new List<Binding>();

            for (int i = 0; i < bindingTestCount; i++)
            {
                var view = new ViewA();

                var binding = new Binding("IntValue", view, "IntValue");
                binding.Bind(model);
                bindingList.Add(binding);

                binding = new Binding("StringValue", view, "StringValue");
                binding.Bind(model);
                bindingList.Add(binding);
            }

            var test = RunActionAsync("NotifyTestA1", bindingTestCount, () =>
            {
                // update value
                model.IntValue = 1;
                model.StringValue = "A";
            });

            yield return StartCoroutine(test);
        }

        private IEnumerator NotifyTestA2()
        {
            var modelList = new List<ModelA>();
            var bindingList = new List<Binding>();

            for (int i = 0; i < bindingTestCount; i++)
            {
                var model = new ModelA();
                modelList.Add(model);

                var view = new ViewA();

                var binding = new Binding("IntValue", view, "IntValue");
                binding.Bind(model);
                bindingList.Add(binding);

                binding = new Binding("StringValue", view, "StringValue");
                binding.Bind(model);
                bindingList.Add(binding);
            }

            var test = RunActionAsync("NotifyTestA2", bindingTestCount, () =>
            {
                // update value
                foreach (var item in modelList)
                {
                    item.IntValue = 1;
                    item.StringValue = "A";
                }
            });

            yield return StartCoroutine(test);
        }

        private IEnumerator LoopTestA1()
        {
            var model = new ModelA();
            var view = new ViewA();

            var binding = new Binding("IntValue", view, "IntValue");
            binding.Bind(model);

            var test = RunActionLoopAsync("LoopTestA1", loopTestCount, () =>
            {
                for (int i = 0; i < innerLoopCount; i++)
                {
                    // update value
                    model.IntValue = i;

                    Assert.AreEqual(i, view.IntValue);
                }
            });

            yield return StartCoroutine(test);
        }

        private IEnumerator LoopTestA2()
        {
            var model = new ModelA();
            var view = new ViewA();

            var binding = new Binding("StringValue", view, "StringValue");
            binding.Bind(model);

            string[] stringTable = new string[]
            {
                "A",
                "B",
                "C",
            };

            var test = RunActionLoopAsync("LoopTestA2", loopTestCount, () =>
            {
                for (int i = 0; i < innerLoopCount; i++)
                {
                    var stringIndex = i % stringTable.Length;
                    var str = stringTable[stringIndex];

                    // update value
                    model.StringValue = str;

                    Assert.AreEqual(str, view.StringValue);
                }
            });

            yield return StartCoroutine(test);
        }


        private IEnumerator NotifyTestB1()
        {
            var model = new ModelB();
            var bindingList = new List<Binding>();

            for (int i = 0; i < bindingTestCount; i++)
            {
                var view = new ViewB();

                var binding = new Binding("IntValue", view, "TextA");
                binding.Bind(model);
                bindingList.Add(binding);

                binding = new Binding("FloatValue", view, "TextB");
                binding.Bind(model);
                bindingList.Add(binding);
            }

            var test = RunActionAsync("NotifyTestB1", bindingTestCount, () =>
            {
                // update value
                model.IntValue = 1;
                model.FloatValue = 2f;
            });

            yield return StartCoroutine(test);
        }

        private IEnumerator NotifyTestB2()
        {
            // wait one frame
            yield return null;

            var modelList = new List<ModelB>();
            var bindingList = new List<Binding>();

            for (int i = 0; i < bindingTestCount; i++)
            {
                var model = new ModelB();
                modelList.Add(model);

                var view = new ViewB();

                var binding = new Binding("IntValue", view, "TextA");
                binding.Bind(model);
                bindingList.Add(binding);

                binding = new Binding("FloatValue", view, "TextB");
                binding.Bind(model);
                bindingList.Add(binding);
            }

            var test = RunActionAsync("NotifyTestB2", bindingTestCount, () =>
            {
                // update value
                foreach (var item in modelList)
                {
                    item.IntValue = 1;
                    item.FloatValue = 2f;
                }
            });

            yield return StartCoroutine(test);
        }

        private IEnumerator PropertyTestA()
        {
            var model = new ViewA()
            {
                StringValue = "Test",
            };

            var intProperty = typeof(ViewA).GetProperty("IntValue");
            var stringProperty = typeof(ViewA).GetProperty("StringValue");

            var test = RunActionAsync("PropertyTestA", propertyTestCount, () =>
            {
                for (int i = 0; i < propertyTestCount; i++)
                {
                    // update int value
                    int count = (int)intProperty.GetValue(model, null) + 1;
                    intProperty.SetValue(model, count, null);

                    string stringValue = (string)stringProperty.GetValue(model, null);
                    stringProperty.SetValue(model, stringValue, null);
                }

                Assert.AreEqual(propertyTestCount, model.IntValue);
            });

            yield return StartCoroutine(test);
        }

        private IEnumerator PropertyTestB()
        {
            // wait one frame
            yield return null;

            var model = new ViewA();
            var intProperty = typeof(ViewA).GetProperty("IntValue");
            var stringProperty = typeof(ViewA).GetProperty("StringValue");

            var intPropertyAccessor = TypeCache.Instance.GetPropertyAccessor(intProperty);
            var stringPropertyAccessor = TypeCache.Instance.GetPropertyAccessor(stringProperty);

            var test = RunActionAsync("PropertyTestB", propertyTestCount, () =>
            {
                for (int i = 0; i < propertyTestCount; i++)
                {
                    int count = (int)intPropertyAccessor.GetValue(model) + 1;
                    intPropertyAccessor.SetValue(model, count);

                    string stringValue = (string)stringPropertyAccessor.GetValue(model);
                    stringPropertyAccessor.SetValue(model, stringValue);
                }

                Assert.AreEqual(propertyTestCount, model.IntValue);
            });

            yield return StartCoroutine(test);
        }

        private IEnumerator CollectionTestA1()
        {
            var listA = new List<object>(collectionTestCount);
            var listB = new List<object>(collectionTestCount);
            for (int i = 0; i < collectionTestCount; i++)
            {
                listA.Add(i);
                listB.Add(2 * i);
            }

            var collection = new ObservableList<object>(collectionTestCount);
            collection.CollectionChanged += (sender, arg) => { };

            var test = RunMemoryTestAsync("CollectionTestA1", () =>
            {
                // add
                for (int i = 0; i < collectionTestCount; i++)
                {
                    collection.Add(listA[i]);
                }

                // indexer
                for (int i = 0; i < collectionTestCount; i++)
                {
                    collection[i] = listB[i];
                }

                // remove
                for (int i = 0; i < collectionTestCount; i++)
                {
                    collection.RemoveAt(0);
                }

                // range
                for (int i = 0; i < collectionTestCount; i++)
                {
                    collection.AddRange(listA);
                    collection.Clear();
                }
            });

            yield return StartCoroutine(test);
        }

        private IEnumerator RunActionAsync(string testName, int count, Action run, Action postRun = null)
        {
            yield return null;

            // run action
            var sw = System.Diagnostics.Stopwatch.StartNew();
            run();
            sw.Stop();
            yield return null;

            // post run
            if (postRun != null)
            {
                postRun();
                yield return null;
            }

            // calculate time
            var total = sw.Elapsed.TotalMilliseconds;
            var avg = total / count;

            // log time
            var info = string.Format("Run {0}, total: {1:F6} ms, avg: {2:F6} ms", testName, total, avg);
            Debug.Log(info);
            yield return null;
        }

        private IEnumerator RunActionLoopAsync(string testName, int count, Action run, Action postRun = null)
        {
            yield return null;

            // run action
            for (int i = 0; i < count; i++)
            {
                run();
                yield return null;
            }

            // post run
            if (postRun != null)
            {
                postRun();
                yield return null;
            }

            // log info
            var info = string.Format("Run {0}, loop={1}", testName, count);
            Debug.Log(info);
            yield return null;
        }

        private IEnumerator RunMemoryTestAsync(string testName, Action run)
        {
            yield return null;

            GC.Collect();
            var sizeBefore = GC.GetTotalMemory(false);
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // run action
            run();

            sw.Stop();
            var sizeAfter = GC.GetTotalMemory(false);
            var usedSize = sizeAfter - sizeBefore;

            yield return null;

            // calculate time
            var total = sw.Elapsed.TotalMilliseconds;
            
            // log time
            var info = string.Format("Run {0}, total: {1:F6} ms, size: {2} bytes", testName, total, usedSize);
            Debug.Log(info);
            yield return null;
        }
    }
}
