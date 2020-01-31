using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Bind Button to an ICommand property.
    ///
    /// The source type must be ICommand. The Execute method will be called if Button is clicked. The
    /// interactable of Button is controlled by the CanExecute method.
    ///
    /// The Execute method has no parameter, you can use closure to capture extra parameters when you
    /// create the DelegateCommand.
    /// </summary>
    [Binder]
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("Peppermint/Data Binding/Binder/UI/Button Binder")]
    public class ButtonBinder : MonoBehaviour
    {
        public string path;

        private Button button;
        private Binding binding;
        private IDataContext dataContext;
        private ICommand command;

        public ICommand Command
        {
            set
            {
                command = value;
            }
        }

        void Start()
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError("Require Button Component", gameObject);
                return;
            }

            CreateBinding();

            // add listener
            button.onClick.AddListener(OnButtonClick);
        }

        void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClick);    
            }
            
            BindingUtility.RemoveBinding(binding, dataContext);
        }

        void Update()
        {
            if (binding.IsBound && command != null)
            {
                var currentValue = button.interactable;
                var newValue = command.CanExecute();

                if (currentValue != newValue)
                {
                    // update if changed
                    button.interactable = newValue;
                }
            }
        }

        private void CreateBinding()
        {
            // create binding
            binding = new Binding(path, this, "Command", Binding.BindingMode.OneWay, Binding.ConversionMode.None, null);
            binding.SetFlags(Binding.ControlFlags.ResetTargetValue);

            BindingUtility.AddBinding(binding, transform, out dataContext);
        }

        private void OnButtonClick()
        {
            if (binding.IsBound && command != null)
            {
                if (command.CanExecute())
                {
                    command.Execute();
                }
            }
        }

    }
}