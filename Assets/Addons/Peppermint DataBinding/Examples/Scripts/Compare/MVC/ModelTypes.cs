using System;
using System.Collections.Generic;

namespace Peppermint.DataBinding.Example.Mvc
{
    public abstract class BaseModel
    {
        public event Action<object, string> PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, propertyName);
            }
        }

        protected bool SetProperty<T>(ref T storage, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            // set new value
            storage = value;

            NotifyPropertyChanged(propertyName);
            return true;
        }
    }

    public class PlayerModel : BaseModel
    {
        private int hp;
        private int mp;
        private int hpMax;
        private int mpMax;

        private int level;
        private long gold;
        private long exp;

        #region Properties

        public int Hp
        {
            get { return hp; }
            set { SetProperty(ref hp, value, "Hp"); }
        }

        public int Mp
        {
            get { return mp; }
            set { SetProperty(ref mp, value, "Mp"); }
        }

        public int HpMax
        {
            get { return hpMax; }
            set { SetProperty(ref hpMax, value, "HpMax"); }
        }

        public int MpMax
        {
            get { return mpMax; }
            set { SetProperty(ref mpMax, value, "MpMax"); }
        }

        public int Level
        {
            get { return level; }
            set { SetProperty(ref level, value, "Level"); }
        }

        public long Gold
        {
            get { return gold; }
            set { SetProperty(ref gold, value, "Gold"); }
        }

        public long Exp
        {
            get { return exp; }
            set { SetProperty(ref exp, value, "Exp"); }
        }

        #endregion
    }

    public class InventoryModel : BaseModel
    {
        public class ItemModel : BaseModel
        {
            private int itemType;
            private string name;
            private int amount;

            #region Properties

            public int ItemType
            {
                get { return itemType; }
            }

            public string Name
            {
                get { return name; }
            }

            public int Amount
            {
                get { return amount; }
                set { SetProperty(ref amount, value, "Amount"); }
            }

            #endregion

            public ItemModel(int itemType, string name, int amount)
            {
                this.itemType = itemType;
                this.name = name;
                this.amount = amount;
            }
        }

        private List<ItemModel> itemList = new List<ItemModel>();

        public List<ItemModel> ItemList
        {
            get { return itemList; }
        }

        #region List operations

        public void AddItem(ItemModel item)
        {
            itemList.Add(item);

            // raise event
            NotifyPropertyChanged("ItemList");
            NotifyPropertyChanged("ItemList.Count");
        }

        public void RemoveItem(ItemModel item)
        {
            itemList.Remove(item);

            // raise event
            NotifyPropertyChanged("ItemList");
            NotifyPropertyChanged("ItemList.Count");
        }

        public void ClearItems()
        {
            itemList.Clear();

            // raise event
            NotifyPropertyChanged("ItemList");
            NotifyPropertyChanged("ItemList.Count");
        }

        #endregion
    }

    public class GameState
    {
        public PlayerModel player;
        public InventoryModel inventory;

        #region Singleton

        public static GameState Instance { get { return Nested.instance; } }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly GameState instance = new GameState();
        }

        #endregion

        public GameState()
        {
            player = new PlayerModel();
            inventory = new InventoryModel();
        }
    }
}
