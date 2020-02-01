using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Bind Selectable targets to a boolean property.
    ///
    /// The value control the interactable of specified targets.
    /// </summary>
    [Binder]
    [AddComponentMenu("Peppermint/Data Binding/Binder/UI/Interactable Binder")]
    public class InteractableBinder : MonoBehaviour
    {
        public string path;
        public Selectable[] targets;

        private IBinding binding;
        private IDataContext dataContext;

        public bool Interactable
        {
            set
            {
                // set all targets
                foreach (var item in targets)
                {
                    item.interactable = value;
                }
            }
        }

        void Start()
        {
            CreateBinding();
        }

        void OnDestroy()
        {
            BindingUtility.RemoveBinding(binding, dataContext);
        }

        private void CreateBinding()
        {
            binding = new Binding(path, this, "Interactable");

            BindingUtility.AddBinding(binding, transform, out dataContext);
        }
    }
}