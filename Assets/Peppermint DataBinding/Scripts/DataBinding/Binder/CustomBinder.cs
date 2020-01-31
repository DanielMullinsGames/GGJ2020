using System;
using System.Collections.Generic;
using UnityEngine;


namespace Peppermint.DataBinding
{
    /// <summary>
    /// CustomBinder can bind any property of Unity Object.
    ///
    /// Set the target object first, you can set it by dragging a unity object to target field. Then
    /// specify the number of binding in configs. In each config, the path is the source path of the
    /// binding. The target path will be shown as popup menu, you can choose the target property from
    /// the popup. If the target is null, the target path will be shown as string field. The
    /// converter name is optional.
    ///
    /// Create a binder for some rarely used property is impractical. CustomBinder is a configurable
    /// binder, it can bind any property of Unity.Object. The CustomBinder just do simple binding
    /// which update the target property if source property changes. If the type does not match it
    /// will use the converter you specified or the default converter. If you need more than just
    /// value copying, you should create your own binder.
    /// </summary>
    [Binder(SourcePathListMethod = "GetSourcePathList")]
    [AddComponentMenu("Peppermint/Data Binding/Binder/Custom/Custom Binder")]
    public class CustomBinder : MonoBehaviour
    {
        [Serializable]
        public class Config
        {
            public string path;

            [PropertyNamePopup("target")]
            public string targetPath;

            public string converterName;
        }

        public UnityEngine.Object target;
        public Config[] configs;

        private List<IBinding> bindingList;
        private IDataContext dataContext;

        void Start()
        {
            if (target == null)
            {
                Debug.LogError("target is null", gameObject);
                return;
            }

            CreateBinding();
        }

        void OnDestroy()
        {
            BindingUtility.RemoveBinding(bindingList, dataContext);
        }

        private void CreateBinding()
        {
            bindingList = new List<IBinding>();

            foreach (var item in configs)
            {
                // set converter mode
                var mode = Binding.ConversionMode.Automatic;
                IValueConverter vc = null;

                if (!string.IsNullOrEmpty(item.converterName))
                {
                    mode = Binding.ConversionMode.Parameter;
                    vc = ValueConverterProvider.Instance.GetNamedConverter(item.converterName);
                }
                
                var binding = new Binding(item.path, target, item.targetPath, Binding.BindingMode.OneWay, mode, vc);

                // add to list
                bindingList.Add(binding);
            }

            BindingUtility.AddBinding(bindingList, transform, out dataContext);
        }

#if UNITY_EDITOR

        public List<string> GetSourcePathList()
        {
            var list = new List<string>();

            foreach (var item in configs)
            {
                list.Add(item.path);
            }

            return list;
        }

#endif
    }

}

