using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Activate targets by comparing the boolean value of the source property.
    ///
    /// If the value is true, targets are activated, otherwise inverseTarget are activated. Set
    /// inverse to true will negate the value.
    /// </summary>
    [Binder]
    [AddComponentMenu("Peppermint/Data Binding/Binder/Selector/Bool Selector")]
    public class BoolSelector : MonoBehaviour
    {
        public string path;
        public bool inverse;
        public GameObject[] targets;
        public GameObject[] inverseTargets;

        private ActiveStateModifier asm;

        private IBinding binding;
        private IDataContext dataContext;

        public bool IsEnabled
        {
            set
            {
                bool flag = inverse ? (!value) : value;

                // deactivate all
                asm.SetAll(false);

                if (flag)
                {
                    // activate targets
                    asm.SetRange(targets, true);
                }
                else
                {
                    // activate inverse targets
                    asm.SetRange(inverseTargets, true);
                }

                asm.Apply();
            }
        }

        void Start()
        {
            // create active state modifier
            asm = new ActiveStateModifier();
            asm.AddRange(targets);
            asm.AddRange(inverseTargets);
            asm.Init();

            CreateBinding();
        }

        void OnDestroy()
        {
            BindingUtility.RemoveBinding(binding, dataContext);
        }

        private void CreateBinding()
        {
            binding = new Binding(path, this, "IsEnabled");

            BindingUtility.AddBinding(binding, transform, out dataContext);
        }
    }
}
