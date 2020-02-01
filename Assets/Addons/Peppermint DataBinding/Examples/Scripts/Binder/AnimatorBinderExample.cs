
namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to use AnimatorBinder.
    ///
    /// Using bool, int and float parameter type is easy, just set the parameter type and the
    /// parameter name in AnimatorBinder. In the code, you just set the value, and the AnimatorBinder
    /// will update the parameter value for you.
    ///
    /// To use trigger parameter type, you need to create the AnimatorTrigger object before the
    /// source is bound. You call AnimatorTrigger's SetTrigger/ResetTrigger method to set/reset a
    /// trigger parameter.
    ///
    /// Notice that the Animator will reset its parameters after deactivated, the AnimatorBinder will
    /// handle parameter restore with some restrictions. See AnimatorBinder for more information.
    /// </summary>
    public class AnimatorBinderExample : BindableMonoBehaviour
    {
        private bool hide;
        private bool isVisible;
        private bool rotate;
        private float speed;
        private AnimatorTrigger selectTrigger;
        private AnimatorTrigger activeTrigger;
        private ICommand selectCommand;
        private ICommand toggleCommand;

        #region Bindable Properties

        public bool Hide
        {
            get { return hide; }
            set { SetProperty(ref hide, value, "Hide"); }
        }

        public bool IsVisible
        {
            get { return isVisible; }
            set { SetProperty(ref isVisible, value, "IsVisible"); }
        }

        public bool Rotate
        {
            get { return rotate; }
            set { SetProperty(ref rotate, value, "Rotate"); }
        }

        public float Speed
        {
            get { return speed; }
            set { SetProperty(ref speed, value, "Speed"); }
        }

        public AnimatorTrigger SelectTrigger
        {
            get { return selectTrigger; }
            set { SetProperty(ref selectTrigger, value, "SelectTrigger"); }
        }

        public AnimatorTrigger ActiveTrigger
        {
            get { return activeTrigger; }
            set { SetProperty(ref activeTrigger, value, "ActiveTrigger"); }
        }

        public ICommand SelectCommand
        {
            get { return selectCommand; }
            set { SetProperty(ref selectCommand, value, "SelectCommand"); }
        }

        public ICommand ToggleCommand
        {
            get { return toggleCommand; }
            set { SetProperty(ref toggleCommand, value, "ToggleCommand"); }
        }

        #endregion

        void Start()
        {
            Init();

            BindingManager.Instance.AddSource(this, typeof(AnimatorBinderExample).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        private void Init()
        {
            selectTrigger = new AnimatorTrigger();
            activeTrigger = new AnimatorTrigger();
            speed = 0.0f;
            isVisible = true;
            rotate = true;

            selectCommand = new DelegateCommand(SelectIcon);
            toggleCommand = new DelegateCommand(ToggleIcon);
        }

        public void SelectIcon()
        {
            // set trigger
            selectTrigger.SetTrigger();
        }

        public void ToggleIcon()
        {
            if (IsVisible)
            {
                // hide icon
                IsVisible = false;
            }
            else
            {
                // notice that the trigger is set before the GO is activated, so the AnimatorBinder
                // will restore the trigger when the animator is activated.
                activeTrigger.SetTrigger();

                // activate GO
                IsVisible = true;
            }
        }
    }

}