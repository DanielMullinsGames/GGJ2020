using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Peppermint.DataBinding.Editor
{
    public class SpriteSetBuilderSettings : ScriptableObject
    {
        [Serializable]
        public class Item
        {
            public bool recursive;
            public string directory;

            internal SpriteSet spriteSet;

            public string Name
            {
                get
                {
                    return Path.GetFileName(directory);
                }
            }
        }

        public string outputDirectory;
        public List<Item> itemList = new List<Item>();
    }
}