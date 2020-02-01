using System;
using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// ViewSelector is a custom selector which activates targets by comparing the string value of
    /// the source property.
    ///
    /// Unlike other selectors, it also handles data context adding/removing when switching views.
    /// </summary>
    [Binder]
    public class ViewSelector : MonoBehaviour
    {
        [Serializable]
        public class Config
        {
            public string name;
            public GameObject view;
        }

        public string path;
        public string requiredSource;
        public Config[] configs;
        public GameObject defaultView;

        private IBinding binding;
        private IDataContext dataContext;
        private GameObject currentView;

        public string StringValue
        {
            set
            {
                // reset
                ResetCurrentView();

                if (value == null)
                {
                    // just return if value is null
                    return;
                }

                // get view
                GameObject view = null;
                var config = Array.Find(configs, x => x.name == value);

                if (config == null)
                {
                    // use default view
                    view = defaultView;
                }
                else
                {
                    view = config.view;
                }

                if (view == null)
                {
                    Debug.LogError(string.Format("No matched view for {0}", value), gameObject);
                    return;
                }

                // activate
                view.SetActive(true);

                // add data context
                var dc = view.GetComponent<IDataContext>();
                BindingManager.Instance.AddDataContext(dc, requiredSource);

                // set current
                currentView = view;
            }
        }

        void Start()
        {
            CreateBinding();
        }

        void OnDestroy()
        {
            ResetCurrentView();

            BindingUtility.RemoveBinding(binding, dataContext);
        }

        private void CreateBinding()
        {
            binding = new Binding(path, this, "StringValue");

            BindingUtility.AddBinding(binding, transform, out dataContext);
        }

        private void ResetCurrentView()
        {
            if (currentView == null)
            {
                return;
            }

            // deactivate
            currentView.SetActive(false);

            // remove data context
            var dc = currentView.GetComponent<IDataContext>();
            BindingManager.Instance.RemoveDataContext(dc);

            // reset
            currentView = null;
        }
    }
}
