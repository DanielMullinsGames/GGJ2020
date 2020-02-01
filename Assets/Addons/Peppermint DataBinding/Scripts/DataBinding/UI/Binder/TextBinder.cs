using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Bind the text property to a property of any type.
    ///
    /// If the format string is empty, the value's ToString method is called, otherwise the specified
    /// format string is used. The source type can be any type.
    /// </summary>
    [Binder]
    [RequireComponent(typeof(Text))]
    [AddComponentMenu("Peppermint/Data Binding/Binder/UI/Text Binder")]
    public class TextBinder : MonoBehaviour
    {
        public string path;
        public string formatString;

        private Text target;
        private IBinding binding;
        private IDataContext dataContext;

        public object Text
        {
            set
            {
                SetText(value);
            }
        }

        void Start()
        {
            target = GetComponent<Text>();
            if (target == null)
            {
                Debug.LogError("Require Text Component", gameObject);
                return;
            }

            CreateBinding();
        }

        void OnDestroy()
        {
            BindingUtility.RemoveBinding(binding, dataContext);
        }

        private void CreateBinding()
        {
            // create binding for text
            binding = new Binding(path, this, "Text");

            BindingUtility.AddBinding(binding, transform, out dataContext);
        }

        void SetText(object value)
        {
            if (string.IsNullOrEmpty(formatString))
            {
                // check null
                if (value == null)
                {
                    target.text = null;
                }
                else
                {
                    target.text = value.ToString();
                }
            }
            else
            {
                // use format string
                target.text = string.Format(formatString, value);
            }
        }

    }
}