using System;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// A simple message box
    /// </summary>
    public class MessageBox : BindableMonoBehaviour
    {
        public enum MessageBoxResult
        {
            Yes,
            No,
        }

        public class MessageBoxArgs
        {
            public string messageText;
            public string yesText;
            public string noText;
            public Action<MessageBoxResult> callback;
        }

        private Action<MessageBoxResult> currentCallback;
        private string messageText;
        private string yesText;
        private string noText;
        private bool isVisible;

        private ICommand yesCommand;
        private ICommand noCommand;

        #region Bindable Properties

        public string MessageText
        {
            get { return messageText; }
            set { SetProperty(ref messageText, value, "MessageText"); }
        }

        public string YesText
        {
            get { return yesText; }
            set { SetProperty(ref yesText, value, "YesText"); }
        }

        public string NoText
        {
            get { return noText; }
            set { SetProperty(ref noText, value, "NoText"); }
        }

        public bool IsVisible
        {
            get { return isVisible; }
            set { SetProperty(ref isVisible, value, "IsVisible"); }
        }

        public ICommand YesCommand
        {
            get { return yesCommand; }
            set { SetProperty(ref yesCommand, value, "YesCommand"); }
        }

        public ICommand NoCommand
        {
            get { return noCommand; }
            set { SetProperty(ref noCommand, value, "NoCommand"); }
        }

        #endregion

        #region Singleton

        private static MessageBox instance;

        public static MessageBox Instance
        {
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType(typeof(MessageBox)) as MessageBox;

                    if (!instance)
                    {
                        Debug.Log("Can not find MessageBoxController instance");
                    }
                }

                return instance;
            }
        }

        #endregion

        void Awake()
        {
            instance = this;
        }

        void OnDisable()
        {
            instance = null;
        }

        void Start()
        {
            // create commands
            yesCommand = new DelegateCommand(() => OnMessageBox(MessageBoxResult.Yes));
            noCommand = new DelegateCommand(() => OnMessageBox(MessageBoxResult.No));

            BindingManager.Instance.AddSource(this, typeof(MessageBox).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        public void Open(MessageBoxArgs args)
        {
            // update message box
            MessageText = args.messageText;
            YesText = args.yesText;
            NoText = args.noText;

            // save callback
            currentCallback = args.callback;

            // show message box
            IsVisible = true;
        }

        private void OnMessageBox(MessageBoxResult result)
        {
            if (currentCallback != null)
            {
                currentCallback.Invoke(result);
                currentCallback = null;    
            }
            
            // hide message box
            IsVisible = false;
        }
    }
}
