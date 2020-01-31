using System;
using UnityEngine;

namespace Peppermint.DataBinding.Editor
{
    [Serializable]
    public class AotCodeSettings : ScriptableObject
    {
        public string outputDirectory;
        public string templatePath;
        public string className;
    }
}
