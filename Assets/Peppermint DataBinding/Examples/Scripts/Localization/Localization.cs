using System;
using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    public enum Language
    {
        ENU,
        FRA,
        JPN,
        CHS,
        CHT,
    }

    /// <summary>
    /// A simple localization manager.
    ///
    /// This class contains a LanguageChanged event which will triggered when the language changes.
    /// You can call Get() method to lookup localized string by string based key, and set the
    /// Language property to change current language.
    ///
    /// It also manges localization asset files loading and unloading. In this demo, we use
    /// ScriptableObject to store the data, the data is just a string dictionary. In production code,
    /// you can use JSON, CSV or any custom format to store the data.
    /// </summary>
    public class Localization
    {
        #region Singleton

        public static Localization Instance { get { return Nested.instance; } }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly Localization instance = new Localization();
        }

        #endregion

        public event Action LanguageChanged
        {
            add
            {
                actionList.Add(value);
            }

            remove
            {
                actionList.Remove(value);
            }
        }

        private List<Action> actionList;
        private Dictionary<string, string> textDictionary;
        private Language language;

        private const string LanguagePrefsKey = "Peppermint.DataBinding.Example.Language";

        public Language Language
        {
            get
            {
                return language;
            }

            set
            {
                SetLanguage(value);
            }
        }

        private Localization()
        {
            InitLanguage();

            LoadData();
        }

        private void InitLanguage()
        {
            if (PlayerPrefs.HasKey(LanguagePrefsKey))
            {
                // load saved value
                language = (Language)PlayerPrefs.GetInt(LanguagePrefsKey);
            }
            else
            {
                // choose language based on system language
                var sysLanguage = Application.systemLanguage;

                if (sysLanguage == SystemLanguage.Chinese || sysLanguage == SystemLanguage.ChineseSimplified)
                {
                    language = Language.CHS;
                }
                else if (sysLanguage == SystemLanguage.ChineseTraditional)
                {
                    language = Language.CHT;
                }
                else if (sysLanguage == SystemLanguage.French)
                {
                    language = Language.FRA;
                }
                else if (sysLanguage == SystemLanguage.Japanese)
                {
                    language = Language.JPN;
                }
                else
                {
                    // fall back to English
                    language = Language.ENU;
                }

                // save it
                PlayerPrefs.SetInt(LanguagePrefsKey, (int)language);
            }

            // create action list for better performance
            actionList = new List<Action>();
        }

        private void LoadData()
        {
            string path = string.Format("Localization/LocalizationData_{0}", language);

            // load data
            var data = Resources.Load<LocalizationData>(path);

            textDictionary = new Dictionary<string, string>();
            for (int i = 0; i < data.keys.Length; i++)
            {
                var key = data.keys[i];
                var text = data.values[i];

                textDictionary.Add(key, text);
            }

            // unload asset
            Resources.UnloadAsset(data);
        }

        private void SetLanguage(Language value)
        {
            language = value;

            // save language
            PlayerPrefs.SetInt(LanguagePrefsKey, (int)language);

            LoadData();

            // raise event
            NotifyLanguageChanged();
        }

        private void NotifyLanguageChanged()
        {
            int n = actionList.Count;
            for (int i = 0; i < n; i++)
            {
                actionList[i].Invoke();
            }
        }

        /// <summary>
        /// Get localized string by key.
        /// </summary>
        /// <param name="key">Translation key</param>
        /// <returns>Return localized string if key is valid, otherwise return null.</returns>
        public string Get(string key)
        {
            if (key == null)
            {
                return null;
            }

            string value = null;
            if (!textDictionary.TryGetValue(key, out value))
            {
                return null;
            }

            return value;
        }
    }
}
