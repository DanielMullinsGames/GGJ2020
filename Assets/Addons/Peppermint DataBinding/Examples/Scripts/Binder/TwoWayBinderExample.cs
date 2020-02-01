using System;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to setup two-way binders for unity built-in controls.
    ///
    /// Usually the one-way binder only need the source property to declare a get accessor, and the
    /// set accessor is used for property changed notification. The two-way binder need the source
    /// property to declare both accessors (read-write property). The two-way binder get its value
    /// from the get accessor, and listen for UI changes. If the UI changes, new value is set to
    /// model by calling the set accessor.
    ///
    /// Note that the set accessor must check the new value, and only raise event if the value really
    /// changes, otherwise it will cause an infinite loop. The SetProperty method from the base class
    /// already handle the check for you.
    /// </summary>
    public class TwoWayBinderExample : BindableMonoBehaviour
    {
        public string playerName;
        public bool enableMusic;
        public float volume;
        public float scrollbarPosition;
        public Vector2 scrollRectPosition;
        public int languageIndex;
        public string[] languageOptions;

        private Func<string, int, char, char> playerNameValidate;

        #region Bindable Properties

        public string PlayerName
        {
            get { return playerName; }
            set { SetProperty(ref playerName, value, "PlayerName"); }
        }

        public bool EnableMusic
        {
            get { return enableMusic; }
            set { SetProperty(ref enableMusic, value, "EnableMusic"); }
        }

        public float Volume
        {
            get { return volume; }
            set { SetProperty(ref volume, value, "Volume"); }
        }

        public float ScrollbarPosition
        {
            get { return scrollbarPosition; }
            set { SetProperty(ref scrollbarPosition, value, "ScrollbarPosition"); }
        }

        public Vector2 ScrollRectPosition
        {
            get { return scrollRectPosition; }
            set { SetProperty(ref scrollRectPosition, value, "ScrollRectPosition"); }
        }

        public int LanguageIndex
        {
            get { return languageIndex; }
            set { SetProperty(ref languageIndex, value, "LanguageIndex"); }
        }

        public string[] LanguageOptions
        {
            get { return languageOptions; }
            set { SetProperty(ref languageOptions, value, "LanguageOptions"); }
        }

        public Func<string, int, char, char> PlayerNameValidate
        {
            get { return playerNameValidate; }
            set { SetProperty(ref playerNameValidate, value, "PlayerNameValidate"); }
        }

        #endregion

        void Start()
        {
            // create validate function
            playerNameValidate = (x, y, z) => ValidateName(z);

            BindingManager.Instance.AddSource(this, typeof(TwoWayBinderExample).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        public char ValidateName(char addedChar)
        {
            Debug.LogFormat("ValidateName char={0}", addedChar);

            if (addedChar == '$')
            {
                return '\0';
            }

            return addedChar;
        }

#if UNITY_EDITOR

        void Reset()
        {
            playerName = "Player";
            volume = 0.5f;
            languageIndex = 0;
            languageOptions = new string[]
            {
                SystemLanguage.English.ToString(),
                SystemLanguage.French.ToString(),
                SystemLanguage.Chinese.ToString(),
            };
            scrollbarPosition = 1f;
            scrollRectPosition = new Vector2(0f, 1f);
        }

#endif

    }
}
