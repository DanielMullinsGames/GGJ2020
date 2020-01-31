using UnityEngine;

namespace Peppermint.DataBinding.Example.Mvvm
{
    public class MiniInfoViewModel : BindableMonoBehaviour
    {
        private Vector3 hpScale;
        private Vector3 mpScale;

        #region Bindable Properties

        public Vector3 HpScale
        {
            get { return hpScale; }
            private set { SetProperty(ref hpScale, value, "HpScale"); }
        }

        public Vector3 MpScale
        {
            get { return mpScale; }
            private set { SetProperty(ref mpScale, value, "MpScale"); }
        }

        #endregion

        void Start()
        {
            var player = GameState.Instance.player;

            // calculate scale
            var ratio = (float)player.Hp / player.HpMax;
            HpScale = new Vector3(ratio, 1f, 1f);
            ratio = (float)player.Mp / player.MpMax;
            MpScale = new Vector3(ratio, 1f, 1f);

            // register handler
            GameState.Instance.player.PropertyChanged += OnPlayerPropertyChanged;

            // add source
            BindingManager.Instance.AddSource(this, typeof(MiniInfoViewModel).Name);
        }

        void OnDestroy()
        {
            // remove source
            BindingManager.Instance.RemoveSource(this);

            // unregister handler
            GameState.Instance.player.PropertyChanged += OnPlayerPropertyChanged;
        }

        private void OnPlayerPropertyChanged(object sender, string propertyName)
        {
            var player = (PlayerModel)sender;

            if (propertyName == "Hp" || propertyName == "HpMax")
            {
                var ratio = (float)player.Hp / player.HpMax;
                HpScale = new Vector3(ratio, 1f, 1f);
            }
            else if (propertyName == "Mp" || propertyName == "MpMax")
            {
                var ratio = (float)player.Mp / player.MpMax;
                MpScale = new Vector3(ratio, 1f, 1f);
            }
        }
    }
}
