using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding.Example.Mvc
{
    /// <summary>
    /// DetailInfo UI control
    /// </summary>
    public class DetailInfoControl : MonoBehaviour
    {
        public Text hpText;
        public Text mpText;
        public Text levelText;
        public Text goldText;
        public Text expText;
        public Text itemCountText;

        void Start()
        {
            RefreshView();

            // add property changed handlers
            GameState.Instance.player.PropertyChanged += OnPlayerPropertyChanged;
            GameState.Instance.inventory.PropertyChanged += OnInventoryPropertyChanged;
        }

        void OnDestroy()
        {
            // remove property changed handlers
            GameState.Instance.player.PropertyChanged -= OnPlayerPropertyChanged;
            GameState.Instance.inventory.PropertyChanged -= OnInventoryPropertyChanged;
        }

        void RefreshView()
        {
            var player = GameState.Instance.player;

            hpText.text = string.Format("{0}/{1}", player.Hp, player.HpMax);
            mpText.text = string.Format("{0}/{1}", player.Mp, player.MpMax);
            levelText.text = player.Level.ToString("D02");
            goldText.text = player.Gold.ToString();
            expText.text = player.Exp.ToString();

            var inventory = GameState.Instance.inventory;
            itemCountText.text = inventory.ItemList.Count.ToString();
        }

        void OnPlayerPropertyChanged(object sender, string propertyName)
        {
            var player = (PlayerModel)sender;

            if (propertyName == "Hp" || propertyName == "HpMax")
            {
                hpText.text = string.Format("{0}/{1}", player.Hp, player.HpMax);
            }
            else if (propertyName == "Mp" || propertyName == "MpMax")
            {
                mpText.text = string.Format("{0}/{1}", player.Mp, player.MpMax);
            }
            else if (propertyName == "Level")
            {
                levelText.text = player.Level.ToString("D02");
            }
            else if (propertyName == "Gold")
            {
                goldText.text = player.Gold.ToString();
            }
            else if (propertyName == "Exp")
            {
                expText.text = player.Exp.ToString();
            }
        }

        void OnInventoryPropertyChanged(object sender, string propertyName)
        {
            var inventory = (InventoryModel)sender;

            if (propertyName == "ItemList.Count")
            {
                itemCountText.text = inventory.ItemList.Count.ToString();
            }
        }
    }
}
