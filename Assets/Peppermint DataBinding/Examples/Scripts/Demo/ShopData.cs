using System;
using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    public enum ShopItemType
    {
        Gold,
        Gem,
    }

    public class ShopData : ScriptableObject
    {
        [Serializable]
        public class Item
        {
            public ShopItemType itemType;
            public int amount;
            public string name;
            public string iconName;
            public float price;
        }

        public List<Item> itemList;
    }
}
