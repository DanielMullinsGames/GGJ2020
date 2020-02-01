using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding.Example.Mvc
{
    /// <summary>
    /// MiniInfo UI control
    /// </summary>
    public class MiniInfoControl : MonoBehaviour
    {
        public Text hpText;
        public Text mpText;
        public Image hpGauge;
        public Image mpGauge;

        void Start()
        {
            RefreshView();

            // add property changed handlers
            GameState.Instance.player.PropertyChanged += OnPlayerPropertyChanged;
        }

        void OnDestroy()
        {
            // remove property changed handlers
            GameState.Instance.player.PropertyChanged -= OnPlayerPropertyChanged;
        }

        void RefreshView()
        {
            var player = GameState.Instance.player;

            hpText.text = player.Hp.ToString();
            mpText.text = player.Mp.ToString();

            // calculate ratio
            var ratio = (float)player.Hp / player.HpMax;
            hpGauge.transform.localScale = new Vector3(ratio, 1f, 1f);
            ratio = (float)player.Mp / player.MpMax;
            mpGauge.transform.localScale = new Vector3(ratio, 1f, 1f);
        }

        void OnPlayerPropertyChanged(object sender, string propertyName)
        {
            var player = (PlayerModel)sender;

            if (propertyName == "Hp")
            {
                hpText.text = player.Hp.ToString();
            }
            else if (propertyName == "Mp")
            {
                mpText.text = player.Mp.ToString();
            }

            if (propertyName == "Hp" || propertyName == "HpMax")
            {
                var ratio = (float)player.Hp / player.HpMax;
                hpGauge.transform.localScale = new Vector3(ratio, 1f, 1f);
            }
            else if (propertyName == "Mp" || propertyName == "MpMax")
            {
                var ratio = (float)player.Mp / player.MpMax;
                mpGauge.transform.localScale = new Vector3(ratio, 1f, 1f);
            }
        }
    }
}
