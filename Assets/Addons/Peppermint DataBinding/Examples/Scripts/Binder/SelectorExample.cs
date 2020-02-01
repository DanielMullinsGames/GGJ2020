using System;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to use selector.
    ///
    /// Selector activates targets by comparing the source value with configured value. In this demo,
    /// the BoolOption uses BoolSelector, IntOption uses IntSelector, StringOption use
    /// StringSelector, and the EnumOption use EnumSelector.
    ///
    /// StringSelector can be used as a general selector, you can specify the string value of source
    /// type, and the converter will handle the type conversion for you. For example, you can bind to
    /// Enum type, just set the value to the name of enum constant. You can also bind to Boolean type
    /// and set value to True or False.
    /// </summary>
    public class SelectorExample : MonoBehaviour
    {
        [Serializable]
        public class BoolOption : BindableObject
        {
            public bool currentValue;

            #region Bindable Properties

            public bool CurrentValue
            {
                get { return currentValue; }
                set { SetProperty(ref currentValue, value, "CurrentValue"); }
            }

            #endregion
        }

        [Serializable]
        public class IntOption : BindableObject
        {
            public int currentValue;
            public int minValue;
            public int maxValue;

            private ICommand increaseCommand;
            private ICommand decreaseCommand;

            #region Bindable Properties

            public int CurrentValue
            {
                get { return currentValue; }
                set { SetProperty(ref currentValue, value, "CurrentValue"); }
            }

            public ICommand IncreaseCommand
            {
                get { return increaseCommand; }
                set { SetProperty(ref increaseCommand, value, "IncreaseCommand"); }
            }

            public ICommand DecreaseCommand
            {
                get { return decreaseCommand; }
                set { SetProperty(ref decreaseCommand, value, "DecreaseCommand"); }
            }

            #endregion

            public IntOption()
            {
                // create commands
                increaseCommand = new DelegateCommand(Increase, () => currentValue != maxValue);
                decreaseCommand = new DelegateCommand(Decrease, () => currentValue != minValue);
            }

            public void Decrease()
            {
                int newValue = currentValue - 1;

                // update value
                CurrentValue = Mathf.Max(minValue, newValue);
            }

            public void Increase()
            {
                int newValue = currentValue + 1;

                // update value
                CurrentValue = Mathf.Min(maxValue, newValue);
            }
        }

        [Serializable]
        public class StringOption : BindableObject
        {
            public string currentValue;
            public string[] options;

            private int valueIndex;
            private ICommand increaseCommand;
            private ICommand decreaseCommand;

            #region Bindable Properties

            public string CurrentValue
            {
                get { return currentValue; }
                set { SetProperty(ref currentValue, value, "CurrentValue"); }
            }

            public ICommand IncreaseCommand
            {
                get { return increaseCommand; }
                set { SetProperty(ref increaseCommand, value, "IncreaseCommand"); }
            }

            public ICommand DecreaseCommand
            {
                get { return decreaseCommand; }
                set { SetProperty(ref decreaseCommand, value, "DecreaseCommand"); }
            }

            #endregion

            public StringOption()
            {
                // create commands
                increaseCommand = new DelegateCommand(Increase, () => valueIndex != options.Length - 1);
                decreaseCommand = new DelegateCommand(Decrease, () => valueIndex != 0);
            }

            public void Decrease()
            {
                // clamp value
                valueIndex = Mathf.Max(0, valueIndex - 1);

                // update
                CurrentValue = options[valueIndex];
            }

            public void Increase()
            {
                // clamp value
                valueIndex = Mathf.Min(options.Length - 1, valueIndex + 1);

                // update
                CurrentValue = options[valueIndex];
            }
        }

        [Serializable]
        public class EnumOption : BindableObject
        {
            /// <summary>
            /// You can specify the following type names in EnumSelector:
            /// 1. "Preset" (Type name)
            /// 2. "SelectorExample+EnumOption+Preset" (Partial type name)
            /// 3. "Peppermint.DataBinding.Example.SelectorExample+EnumOption+Preset" (Namespace qualified type name)
            /// </summary>
            public enum Preset
            {
                Mute,
                Low,
                Normal,
                High
            }

            public Preset currentValue;

            private int minValue;
            private int maxValue;
            private ICommand increaseCommand;
            private ICommand decreaseCommand;

            #region Bindable Properties

            public Preset CurrentValue
            {
                get { return currentValue; }
                set { SetProperty(ref currentValue, value, "CurrentValue"); }
            }

            public ICommand IncreaseCommand
            {
                get { return increaseCommand; }
                set { SetProperty(ref increaseCommand, value, "IncreaseCommand"); }
            }

            public ICommand DecreaseCommand
            {
                get { return decreaseCommand; }
                set { SetProperty(ref decreaseCommand, value, "DecreaseCommand"); }
            }

            #endregion

            public EnumOption()
            {
                // create commands
                increaseCommand = new DelegateCommand(Increase, () => currentValue != Preset.High);
                decreaseCommand = new DelegateCommand(Decrease, () => currentValue != Preset.Mute);

                // set enum bound
                minValue = (int)Preset.Mute;
                maxValue = (int)Preset.High;
            }

            public void Decrease()
            {
                int newValue = (int)currentValue - 1;

                // clamp value
                newValue = Mathf.Max(minValue, newValue);

                // update
                CurrentValue = (Preset)newValue;
            }

            public void Increase()
            {
                int newValue = (int)currentValue + 1;

                // clamp value
                newValue = Mathf.Min(maxValue, newValue);

                // update
                CurrentValue = (Preset)newValue;
            }
        }

        public BoolOption booleanOption;
        public IntOption intOption;
        public StringOption stringOption;
        public EnumOption enumOption;

        void Start()
        {
            // add source
            BindingManager.Instance.AddSource(booleanOption, typeof(BoolOption).Name);
            BindingManager.Instance.AddSource(intOption, typeof(IntOption).Name);
            BindingManager.Instance.AddSource(stringOption, typeof(StringOption).Name);
            BindingManager.Instance.AddSource(enumOption, typeof(EnumOption).Name);
        }

        void OnDestroy()
        {
            // remove source
            BindingManager.Instance.RemoveSource(booleanOption);
            BindingManager.Instance.RemoveSource(intOption);
            BindingManager.Instance.RemoveSource(stringOption);
            BindingManager.Instance.RemoveSource(enumOption);
        }
    }
}
