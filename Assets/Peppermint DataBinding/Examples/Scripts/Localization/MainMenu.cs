using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// ViewModel for main menu.
    ///
    /// This viewModel does not contains any text, so it do not need to handle the LanguageChanged
    /// event. In the QuitGame method, it will popup a message box. The message box argument contains
    /// localization text, so we can simplely call the Get() method of Localization to get localized text.
    /// </summary>
    public class MainMenu : BindableMonoBehaviour
    {
        private ICommand startCommand;
        private ICommand quitCommand;
        private ICommand enterOptionMenuCommand;

        #region Bindable Properties

        public ICommand StartCommand
        {
            get { return startCommand; }
            set { SetProperty(ref startCommand, value, "StartCommand"); }
        }

        public ICommand QuitCommand
        {
            get { return quitCommand; }
            set { SetProperty(ref quitCommand, value, "QuitCommand"); }
        }

        public ICommand EnterOptionMenuCommand
        {
            get { return enterOptionMenuCommand; }
            set { SetProperty(ref enterOptionMenuCommand, value, "EnterOptionMenuCommand"); }
        }

        #endregion

        void Start()
        {
            // create commands
            startCommand = new DelegateCommand(() => LogInfo("Start Game"));
            enterOptionMenuCommand = new DelegateCommand(EnterOptionMenu);
            quitCommand = new DelegateCommand(QuitGame);

            BindingManager.Instance.AddSource(this, typeof(MainMenu).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        private void OnMessageBox(MessageBox.MessageBoxResult result)
        {
            if (result == MessageBox.MessageBoxResult.Yes)
            {
                LogInfo("Quit game");
            }
        }

        private void LogInfo(string text)
        {
            MessagePopup.Instance.Show(text);
            Debug.Log(text);
        }

        public void EnterOptionMenu()
        {
            MenuManager.Instance.PushMenu("OptionMenu");
        }

        public void QuitGame()
        {
            // setup message box arguments
            var args = new MessageBox.MessageBoxArgs();
            args.callback = OnMessageBox;

            // get localized text
            args.messageText = Localization.Instance.Get("MessageBox_QuitGame");
            args.yesText = Localization.Instance.Get("OK");
            args.noText = Localization.Instance.Get("Cancel");

            // open message box
            MessageBox.Instance.Open(args);
        }
    }
}