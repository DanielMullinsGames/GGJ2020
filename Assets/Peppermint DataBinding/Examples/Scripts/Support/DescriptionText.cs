using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    public class DescriptionText : MonoBehaviour
    {
        [Multiline]
        public string text;

        public string Text
        {
            get { return text; }
        }
    }
}
