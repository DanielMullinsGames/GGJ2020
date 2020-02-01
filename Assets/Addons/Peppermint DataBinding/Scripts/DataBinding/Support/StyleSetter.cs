using System;

namespace Peppermint.DataBinding
{
    [Serializable]
    public class StyleSetter
    {
        [PropertyNamePopup("target")]
        public string targetPath;

        public UnionObject value;
    }
}
