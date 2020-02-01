using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Bind the color property of UI.Graphic to a color property. The source type must be Color.
    /// </summary>
    [Binder]
    [AddComponentMenu("Peppermint/Data Binding/Binder/UI/Color Binder")]
    public class ColorBinder : MonoBehaviour
    {
        public string path;

        private IBinding binding;
        private IDataContext dataContext;

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
            var target = GetComponent<UnityEngine.UI.Graphic>();
            if (target == null)
            {
                Debug.LogError("Require UI.Graphic Component", gameObject);
                return;
            }

            // create binding for color
            binding = new Binding(path, target, "color");

            BindingUtility.AddBinding(binding, transform, out dataContext);
        }
    }

}
