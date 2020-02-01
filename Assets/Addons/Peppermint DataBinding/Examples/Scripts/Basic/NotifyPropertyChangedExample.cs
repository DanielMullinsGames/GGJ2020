
namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// Demonstrate how to notify the property changed event.
    ///
    /// In peppermint data binding, there are two ways to notify these events:
    /// -Implement the INotifyPropertyChanged interface (preferred).
    /// -Call BindingManager.NotifyPropertyChanged method.
    ///
    /// The first approach is recommended, the source type only need to inherit from "BindableClass",
    /// and call the SetProperty method in its property's set accessor. When you set a new value to
    /// its property, the PropertyChanged event is invoked.
    ///
    /// If your source type does not implement INotifyPropertyChanged interface, you can't invoke the
    /// PropertyChanged event. Instead, you call BindingManager.NotifyPropertyChanged method if the
    /// source property is changed.
    ///
    /// In this example, we demonstrate both ways. The ClassA is a bindable class, it uses the
    /// PropertyChanged event to notify property changed. The ClassB is a normal class, it calls the
    /// NotifyPropertyChanged method to notify property changed event.
    ///
    /// BindingManager has 4 different methods for property notification. NotifyPropertyChanged for
    /// normal/nested property. NotifyItemPropertyChanged for normal/nested property of specified
    /// collection item.
    /// </summary>
    public class NotifyPropertyChangedExample : BindableMonoBehaviour
    {
        // ClassA is a bindable class, update the source property will notify the property changed
        // event to binding objects, and the UI will get updated automatically.
        public class ClassA : BindableObject
        {
            private int counter;

            #region Bindable Properties

            public int Counter
            {
                get { return counter; }
                set { SetProperty(ref counter, value, "Counter"); }
            }

            #endregion
        }

        // ClassB is a normal class, it does not implement the INotifyPropertyChanged interface. If
        // you want to use it as binding source, you need to do the property changed notification
        // manually, otherwise the UI will not get updated.
        public class ClassB
        {
            public int Counter { get; set; }
        }

        private ClassA objectA;
        private ClassB objectB;

        private ICommand updateValueCommand;

        public ClassA ObjectA
        {
            get { return objectA; }
        }

        public ClassB ObjectB
        {
            get { return objectB; }
        }

        #region Bindable Properties

        public ICommand UpdateValueCommand
        {
            get { return updateValueCommand; }
            set { SetProperty(ref updateValueCommand, value, "UpdateValueCommand"); }
        }

        #endregion

        void Start()
        {
            // create binding source objects
            objectA = new ClassA();
            objectB = new ClassB();

            // create command
            updateValueCommand = new DelegateCommand(UpdateValue);

            BindingManager.Instance.AddSource(this, typeof(NotifyPropertyChangedExample).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        private void UpdateValue()
        {
            objectA.Counter++;

            objectB.Counter++;

            // notify "ObjectB.Counter" is changed manually. "ObjectB.Counter" is a nested property,
            // we use the overloaded version for nested property.
            BindingManager.Instance.NotifyPropertyChanged(this, objectB, "Counter");
        }
    }
}