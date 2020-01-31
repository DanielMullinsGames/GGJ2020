using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to implement localization.
    ///
    /// Localization is a bit complex, you need to handle localization for text, image, cultureInfo,
    /// etc. This example shows how to handle the text localization with MVVM as simple as possible.
    /// The basic principle is the same, you can create your own localization to handle image
    /// localization and other stuff. Check Localization.cs for more information.
    ///
    /// Basically, localized text can be divided into two categories: unreferenced text and
    /// referenced text. The unreferenced text is just normal text on the UI, and you don't need to
    /// manipulate the text in your code. Such as text label: "Volume", "OK", "Cancel", etc. The
    /// referenced text is the text referenced in your code. You need to manipulate or generate these
    /// texts in your view model, such as "PlayerName", "NetworkStatus", etc.
    ///
    /// To handle unreferenced text localization, check TextLocalize for more information.
    ///
    /// This OptionMenu handles the LanguageChanged event in its OnLocalize method. This method will
    /// be called when the language changes. In this method it populates the "LanguageNames" array with
    /// localized strings, so the dropdown control will update its options. It also notifies the
    /// "NetworkStatus" changes, so the get method of NetworkStatus will be called. The NetworkStatus string
    /// is dynamically changing, so we have to generate it on the fly.
    /// </summary>
    public class OptionMenu : BindableMonoBehaviour
    {
        public float updateInterval = 1.0f;
        public Language[] supportedLanguages;
        
        private bool enableMusic;
        private bool enableSfx;
        private int languageIndex;
        private string[] languageNames;
        private ICommand backCommand;
        private NetworkReachability currentStatus;

        private const string EnableMusicPrefsKey = "Peppermint.DataBinding.Example.EnableMusic";
        private const string EnableSfxPrefsKey = "Peppermint.DataBinding.Example.EnableSfx";

        #region Bindable Properties

        public bool EnableMusic
        {
            get { return enableMusic; }
            set
            {
                if (SetProperty(ref enableMusic, value, "EnableMusic"))
                {
                    PlayerPrefs.SetInt(EnableMusicPrefsKey, value ? 1 : 0);
                }
            }
        }

        public bool EnableSfx
        {
            get { return enableSfx; }
            set
            {
                if (SetProperty(ref enableSfx, value, "EnableSfx"))
                {
                    PlayerPrefs.SetInt(EnableSfxPrefsKey, value ? 1 : 0);
                }
            }
        }

        public int LanguageIndex
        {
            get { return languageIndex; }
            set
            {
                if (SetProperty(ref languageIndex, value, "LanguageIndex"))
                {
                    // convert index to enum
                    var newLanguage = supportedLanguages[languageIndex];

                    // call localization
                    Localization.Instance.Language = newLanguage;
                }
            }
        }

        public String[] LanguageNames
        {
            get { return languageNames; }
            set { SetProperty(ref languageNames, value, "LanguageNames"); }
        }

        public ICommand BackCommand
        {
            get { return backCommand; }
            set { SetProperty(ref backCommand, value, "BackCommand"); }
        }

        public string NetworkStatus
        {
            get
            {
                var label = Localization.Instance.Get("NetworkStatus");

                // get localized enum text
                var enumKey = string.Format("NetworkReachability.{0}", Application.internetReachability);
                var statusText = Localization.Instance.Get(enumKey);

                // combine
                var text = string.Format("{0}: {1}", label, statusText);
                return text;
            }
        }

        #endregion

        void Start()
        {
            // get saved values
            enableMusic = PlayerPrefs.GetInt(EnableMusicPrefsKey, 1) != 0;
            enableSfx = PlayerPrefs.GetInt(EnableSfxPrefsKey, 1) != 0;

            SetLanguageNames();

            // get language index
            languageIndex = Array.IndexOf(supportedLanguages, Localization.Instance.Language);

            // create commands
            backCommand = new DelegateCommand(BackToMainMenu);

            // start update
            currentStatus = Application.internetReachability;
            StartCoroutine(UpdateAsync());

            // add event handler
            Localization.Instance.LanguageChanged += OnLocalize;

            BindingManager.Instance.AddSource(this, typeof(OptionMenu).Name);
        }

        void OnDestroy()
        {
            // remove event handler
            Localization.Instance.LanguageChanged -= OnLocalize;

            BindingManager.Instance.RemoveSource(this);
        }

        private IEnumerator UpdateAsync()
        {
            while (true)
            {
                yield return new WaitForSeconds(updateInterval);

                // get current
                var status = Application.internetReachability;

                if (status != currentStatus)
                {
                    currentStatus = status;
                    NotifyPropertyChanged("NetworkStatus");
                }
            }
        }

        private void SetLanguageNames()
        {
            // get localized text
            var nameList = new List<string>();
            foreach (var item in supportedLanguages)
            {
                var key = string.Format("Language.{0}", item);
                var text = Localization.Instance.Get(key);

                nameList.Add(text);
            }

            // update language names
            LanguageNames = nameList.ToArray();
        }

        private void OnLocalize()
        {
            SetLanguageNames();

            // notify "NetworkStatus"
            NotifyPropertyChanged("NetworkStatus");
        }

        public void BackToMainMenu()
        {
            MenuManager.Instance.PopMenu();
        }
    }
}
