using System;
using UnityEngine;

namespace Peppermint.DataBinding.Editor
{
    [Serializable]
    public class PropertyCodeSettings : ScriptableObject
    {
        public string outputDirectory;
        public string templatePath;
    }
}
