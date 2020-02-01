using System;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Activate targets by comparing the integer value of the source property.
    ///
    /// Set the size of configs array first. In each config, specify the value to be tested and the
    /// targets to be activated. The test function decides how the test is performed. When the value
    /// changes, the IntSelector will test the value with each value in configs, the passed targets
    /// will be activated, the failed targets will be deactivated.
    /// </summary>
    [Binder]
    [AddComponentMenu("Peppermint/Data Binding/Binder/Selector/Int Selector")]
    public class IntSelector : MonoBehaviour
    {
        [Serializable]
        public class Config
        {
            public int value;
            public GameObject[] targets;
        }

        public enum TestFunction
        {
            Never,
            Less,
            LessEqual,
            Greater,
            GreaterEqual,
            Equal,
            NotEqual,
            Always,
        }

        public string path;
        public TestFunction testFunction;
        public Config[] configs;

        private ActiveStateModifier asm;

        private IBinding binding;
        private IDataContext dataContext;

        public int IntValue
        {
            set
            {
                // deactivate all
                asm.SetAll(false);

                foreach (var config in configs)
                {
                    if (TestValue(config.value, value))
                    {
                        // activate all
                        asm.SetRange(config.targets, true);
                    }
                }

                asm.Apply();
            }
        }

        void Start()
        {
            // create active state modifier
            asm = new ActiveStateModifier();
            foreach (var item in configs)
            {
                asm.AddRange(item.targets);
            }
            asm.Init();

            CreateBinding();
        }

        void OnDestroy()
        {
            BindingUtility.RemoveBinding(binding, dataContext);
        }

        private void CreateBinding()
        {
            binding = new Binding(path, this, "IntValue");

            BindingUtility.AddBinding(binding, transform, out dataContext);
        }

        private bool TestValue(int lhs, int rhs)
        {
            switch (testFunction)
            {
                case TestFunction.Never:
                    return false;

                case TestFunction.Less:
                    return lhs < rhs;

                case TestFunction.LessEqual:
                    return lhs <= rhs;

                case TestFunction.Greater:
                    return lhs > rhs;

                case TestFunction.GreaterEqual:
                    return lhs >= rhs;

                case TestFunction.Equal:
                    return lhs == rhs;

                case TestFunction.NotEqual:
                    return lhs != rhs;

                case TestFunction.Always:
                    return true;

                default:
                    Debug.LogErrorFormat("Unsupported function {0}", testFunction);
                    return false;
            }
        }
    }
}
