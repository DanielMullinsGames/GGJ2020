using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to setup DataContext.
    ///
    /// Usually, if multiple binders bind to the same object, you only need to add one DataContext
    /// and one DataContextRegister to their common ancestor. All binders under this data context
    /// node share the same data context, so you don't need to setup data context for each binder. If
    /// a binder in its child node need to bind to other source, you need to add a new data context,
    /// and specify the required source name.
    ///
    /// The binder finds the DataContext by searching the transform hierarchy upwards. It will use
    /// the first found DataContext.
    ///
    /// In this demo, the "CombinedInfo" panel has 4 text controls. The "Name", "Gold" and "Gems"
    /// bind to PlayerInfo, and the "ItemCount" binds to InventoryInfo. We only need to add a data
    /// context "PlayerInfo" to the "CombinedInfo" node, and add another data context "InventoryInfo"
    /// to "ItemCount" node.
    /// </summary>
    public class DataContextExample : MonoBehaviour
    {
        public class InventoryInfo : BindableObject
        {
            private int itemCount;
            private int capacity;

            #region Bindable Properties

            public int ItemCount
            {
                get { return itemCount; }
                set { SetProperty(ref itemCount, value, "ItemCount"); }
            }

            public int Capacity
            {
                get { return capacity; }
                set { SetProperty(ref capacity, value, "Capacity"); }
            }

            #endregion
        }

        public class PlayerInfo : BindableObject
        {
            private string name;
            private long gold;
            private long gems;

            #region Bindable Properties

            public string Name
            {
                get { return name; }
                set { SetProperty(ref name, value, "Name"); }
            }

            public long Gold
            {
                get { return gold; }
                set { SetProperty(ref gold, value, "Gold"); }
            }

            public long Gems
            {
                get { return gems; }
                set { SetProperty(ref gems, value, "Gems"); }
            }

            #endregion
        }

        private InventoryInfo inventoryInfo;
        private PlayerInfo playerInfo;

        void Start()
        {
            // create models
            inventoryInfo = new InventoryInfo()
            {
                Capacity = 20,
                ItemCount = 12,
            };

            playerInfo = new PlayerInfo()
            {
                Name = "TestPlayer",
                Gold = 9999,
                Gems = 99,
            };

            // add source
            BindingManager.Instance.AddSource(inventoryInfo, typeof(InventoryInfo).Name);
            BindingManager.Instance.AddSource(playerInfo, typeof(PlayerInfo).Name);
        }

        void OnDestroy()
        {
            // remove source
            BindingManager.Instance.RemoveSource(inventoryInfo);
            BindingManager.Instance.RemoveSource(playerInfo);
        }
    }
}
