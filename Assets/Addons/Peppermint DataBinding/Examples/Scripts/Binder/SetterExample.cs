using System;
using System.Linq;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to use setter.
    ///
    /// In this demo, one ColorSetter is used to control the color of the BG image, and another
    /// CustomSetter is used to control the font size of the Text. Setter is useful if you want to
    /// tweak the view without adding extra data to the model.
    ///
    /// For example, If you want specify different colors in the view for different rarity values,
    /// you can add a color property in the model and use model to control the view. Or you can use
    /// setter to specify the color directly in the view base on the rarity value.
    ///
    /// Note that if the view is totally different for different rarity values, you should make
    /// different views and use selector to choose which view to be activated.
    /// </summary>
    public class SetterExample : BindableMonoBehaviour
    {
        public enum Rarity
        {
            Common,
            Rare,
            Epic,
            Legendary,
        }

        private Rarity currentValue;
        private int minValue;
        private int maxValue;

        private ICommand increaseCommand;
        private ICommand decreaseCommand;

        #region Bindable Properties

        public Rarity CurrentValue
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

        void Start()
        {
            // create commands
            increaseCommand = new DelegateCommand(Increase, () => currentValue != Rarity.Legendary);
            decreaseCommand = new DelegateCommand(Decrease, () => currentValue != Rarity.Common);

            // get enum bound
            var values = (Rarity[])Enum.GetValues(typeof(Rarity));
            minValue = (int)values.First();
            maxValue = (int)values.Last();

            BindingManager.Instance.AddSource(this, typeof(SetterExample).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        public void Decrease()
        {
            int newValue = (int)currentValue - 1;

            // clamp value
            newValue = Mathf.Max(minValue, newValue);

            // update
            CurrentValue = (Rarity)newValue;
        }

        public void Increase()
        {
            int newValue = (int)currentValue + 1;

            // clamp value
            newValue = Mathf.Min(maxValue, newValue);

            // update
            CurrentValue = (Rarity)newValue;
        }
    }
}
