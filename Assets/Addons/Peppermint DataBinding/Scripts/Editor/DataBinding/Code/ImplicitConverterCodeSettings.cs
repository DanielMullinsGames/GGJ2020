using System;
using UnityEngine;

namespace Peppermint.DataBinding.Editor
{
    [Serializable]
    public class ImplicitConverterCodeSettings : ScriptableObject
    {
        public string outputDirectory;
        public string templatePath;
    }
}