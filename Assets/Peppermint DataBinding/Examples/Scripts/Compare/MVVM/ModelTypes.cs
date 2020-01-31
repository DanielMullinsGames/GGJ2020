
namespace Peppermint.DataBinding.Example.Mvvm
{
    public class PlayerModel : BindableObject
    {
        private int hp;
        private int mp;
        private int hpMax;
        private int mpMax;

        private int level;
        private long gold;
        private long exp;

        #region Bindable Properties

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

    public class InventoryModel : BindableObject
    {
        public class ItemModel : BindableObject
        {
            private int itemType;
            private string name;
            private int amount;

            #region Bindable Properties

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

        private ObservableList<ItemModel> itemList = new ObservableList<ItemModel>();

        #region Bindable Properties

        public ObservableList<ItemModel> ItemList
        {
            get { return itemList; }
            set { SetProperty(ref itemList, value, "ItemList"); }
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