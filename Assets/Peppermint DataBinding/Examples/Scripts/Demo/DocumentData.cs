using System;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    public enum ComponentCategory
    {
        Binder,
        TwoWayBinder,
        Setter,
        Getter,
        Selector,
    }

    public class DocumentData : ScriptableObject
    {
        [Serializable]
        public class Item
        {
            public string name;
            public ComponentCategory category;
            public string description;
            public string iconName;
        }

        public Item[] list;
    }

}