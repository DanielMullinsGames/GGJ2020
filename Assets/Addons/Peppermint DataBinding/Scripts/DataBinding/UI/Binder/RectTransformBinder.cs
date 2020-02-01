using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Bind the position properties of RectTransform to source properties.
    ///
    /// Specify the source path you want to bind, empty path will be ignored. For a non-stretching
    /// rect, you can use "Anchored Position Path" and "Size Delta Path". For a stretching rect, you
    /// can use "Offset Min Path" and "Offset Max Path".
    /// </summary>
    [Binder]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Peppermint/Data Binding/Binder/Component/RectTransform Binder")]
    public class RectTransformBinder : MonoBehaviour
    {
        public string offsetMinPath;
        public string offsetMaxPath;
        public string anchoredPositionPath;
        public string sizeDeltaPath;

        private List<IBinding> bindingList;
        private IDataContext dataContext;

        void Start()
        {
            CreateBinding();
        }

        void OnDestroy()
        {
            BindingUtility.RemoveBinding(bindingList, dataContext);
        }

        private void CreateBinding()
        {
            var target = GetComponent<RectTransform>();
            if (target == null)
            {
                Debug.LogError("Require RectTransform Component", gameObject);
                return;
            }

            bindingList = new List<IBinding>();

            if (!string.IsNullOrEmpty(offsetMinPath))
            {
                Binding binding = new Binding(offsetMinPath, target, "offsetMin");
                bindingList.Add(binding);
            }

            if (!string.IsNullOrEmpty(offsetMaxPath))
            {
                Binding binding = new Binding(offsetMaxPath, target, "offsetMax");
                bindingList.Add(binding);
            }

            if (!string.IsNullOrEmpty(anchoredPositionPath))
            {
                Binding binding = new Binding(anchoredPositionPath, target, "anchoredPosition");
                bindingList.Add(binding);
            }

            if (!string.IsNullOrEmpty(sizeDeltaPath))
            {
                Binding binding = new Binding(sizeDeltaPath, target, "sizeDelta");
                bindingList.Add(binding);
            }

            BindingUtility.AddBinding(bindingList, transform, out dataContext);
        }
    }
}