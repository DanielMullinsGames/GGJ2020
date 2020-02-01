using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// Text localization component.
    ///
    /// You need to specify the string key which is used to lookup the localized text. It handles the
    /// LanguageChanged event, so when the language changes, it automatically sets localized string
    /// to attached Text.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class TextLocalize : MonoBehaviour
    {
        public string key;

        private Text target;

        void Start()
        {
            // get target
            target = GetComponent<Text>();

            Localization.Instance.LanguageChanged += OnLocalize;

            OnLocalize();
        }

        void OnDestroy()
        {
            Localization.Instance.LanguageChanged -= OnLocalize;
        }

        void OnLocalize()
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("Key is empty", gameObject);
                return;
            }

            // get text
            var text = Localization.Instance.Get(key);
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogWarning(string.Format("Invalid key {0}", key), gameObject);
                return;
            }

            // set to target
            target.text = text;
        }
    }
}
