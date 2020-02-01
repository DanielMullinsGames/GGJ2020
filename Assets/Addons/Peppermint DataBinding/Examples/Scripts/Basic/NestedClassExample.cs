
namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to bind to nested members.
    ///
    /// You can declare a public get accessor for the nested object and make it available for data
    /// binding. Alternatively, you can create a new property for the member of nested object, and
    /// handle the property changed event redirection.
    ///
    /// In this demo, the left view binds to the nested property "NestedA.Counter", and the right
    /// view binds to the property "NestedCounter".
    ///
    /// In peppermint data binding, the source/target path of Binding, the source path of
    /// CollectionBinding, these paths support nested property. The notation for nested properties is
    /// the same as C#, e.g. "PropertyA.PropertyB.PropertyC".
    ///
    /// Notice that if you bind to a nested property, the source of the binding is the nested object,
    /// not the object you added as source.
    /// </summary>
    public class NestedClassExample : BindableMonoBehaviour
    {
        /// <summary>
        /// ClassA is a nested Class
        /// </summary>
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

        private int counter;
        private ClassA nestedA;
        private ICommand updateValueCommand;

        #region Bindable Properties

        public int Counter
        {
            get { return counter; }
            set { SetProperty(ref counter, value, "Counter"); }
        }

        public ICommand UpdateValueCommand
        {
            get { return updateValueCommand; }
            set { SetProperty(ref updateValueCommand, value, "UpdateValueCommand"); }
        }

        #endregion


        /// <summary>
        /// To bind to a nested property, you must specify the full source path in binder. The
        /// underlying binding object will find the right source object to bind.
        /// </summary>
        public ClassA NestedA
        {
            get { return nestedA; }
        }

        /// <summary>
        /// Getter for nested property.
        /// </summary>
        public int NestedCounter
        {
            get { return nestedA.Counter; }
        }

        void Start()
        {
            nestedA = new ClassA();

            // register event handler
            nestedA.PropertyChanged += nestedA_PropertyChanged;

            // create command
            updateValueCommand = new DelegateCommand(UpdateData);

            BindingManager.Instance.AddSource(this, typeof(NestedClassExample).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        public void UpdateData()
        {
            Counter += 10;
            nestedA.Counter++;
        }

        void nestedA_PropertyChanged(object sender, string propertyName)
        {
            // propertyName is the name of changed property in ClassA   
            if (propertyName == "Counter")
            {
                // redirect to property "NestedCounter"
                NotifyPropertyChanged("NestedCounter");
            }
        }
    }
}