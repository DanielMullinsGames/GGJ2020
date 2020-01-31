using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to use ButtonBinder.
    ///
    /// The ButtonBinder binds to ICommand object, the ICommand interface has two methods, the
    /// CanExecute method control the interactable of the button, and the Execute method is used to
    /// handle the button's onClick event.
    ///
    /// You can use DelegateCommand to create ICommand objects. The constructor has two parameters,
    /// the first parameter is compulsory. It is used for the execute method. The second parameter is
    /// optional, it is a delegate for the CanExecute method.
    ///
    /// Notice that the Execute method has no parameter, you can use closure to capture any number of
    /// parameters.
    /// </summary>
    public class ButtonBinderExample : BindableMonoBehaviour
    {
        private ICommand commandA;
        private ICommand commandB;
        private ICommand commandC;

        #region Bindable Properties

        public ICommand CommandA
        {
            get { return commandA; }
            set { SetProperty(ref commandA, value, "CommandA"); }
        }

        public ICommand CommandB
        {
            get { return commandB; }
            set { SetProperty(ref commandB, value, "CommandB"); }
        }

        public ICommand CommandC
        {
            get { return commandC; }
            set { SetProperty(ref commandC, value, "CommandC"); }
        }

        #endregion

        void Start()
        {
            // no parameter
            commandA = new DelegateCommand(MethodA);

            // one parameter
            commandB = new DelegateCommand(() => MethodB(0));

            // set a delegate for CanExecute.
            commandC = new DelegateCommand(MethodC, CanExecuteMethodC);

            BindingManager.Instance.AddSource(this, typeof(ButtonBinderExample).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        public void MethodA()
        {
            LogInfo("MethodA");
        }

        public void MethodB(int value)
        {
            LogInfo(string.Format("MethodB, value={0}", value));

            // create new command
            CommandB = new DelegateCommand(() => MethodB(value + 1));
        }

        public void MethodC()
        {
            LogInfo("MethodC");
        }

        public bool CanExecuteMethodC()
        {
            var t = (int)Time.realtimeSinceStartup;
            return ((t % 2) == 0);
        }

        private void LogInfo(string text)
        {
            MessagePopup.Instance.Show(text);
            Debug.Log(text);
        }
    }
}
