using System;
using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// CollectionMultiViewBinder is an extended version of CollectionBinder. It supports multiple
    /// view templates.
    ///
    /// "View Selector Path" is the source path for the view selector. It's a delegate that will be
    /// called when the view is created. You can set different view templates in the configs. In each
    /// config, you must specify a view template, and set a name tag (optional). When the delegate is
    /// called, you can decide which view template to be used based on the item object. If the "View
    /// Selector Path" is empty, it will use default type name matching.
    ///
    /// You can also specify "Default ViewTemplate", it will be used if there is no matching view template.
    /// </summary>
    [Binder]
    [AddComponentMenu("Peppermint/Data Binding/Binder/Collection/Collection MultiView Binder")]
    public class CollectionMultiViewBinder : MonoBehaviour, IViewFactory
    {
        public string collectionPath;
        public string viewSelectorPath;
        public ViewTemplateConfig[] configs;
        public GameObject defaultViewTemplate;
        public string collectionViewPath;
        public Transform container;
        public bool usePool;

        protected SelectViewTemplateDelegate viewSelector;
        protected CollectionBinding collectionBinding;
        protected List<IBinding> bindingList;
        protected IDataContext dataContext;
        protected IViewPool viewPool;
        protected Dictionary<string, GameObject> viewNameDictionary;

        public ICollectionView CollectionView
        {
            get
            {
                return collectionBinding.CollectionView;
            }
        }

        public Delegate ViewSelector
        {
            set
            {
                if (value == null)
                {
                    viewSelector = null;
                    return;
                }

                if (value is SelectViewTemplateDelegate)
                {
                    // cast to SelectViewTemplateDelegate
                    viewSelector = (SelectViewTemplateDelegate)value;
                }
                else
                {
                    // convert to SelectViewTemplateDelegate
                    viewSelector = (SelectViewTemplateDelegate)Delegate.CreateDelegate(typeof(SelectViewTemplateDelegate), value.Target, value.Method, false);

                    if (viewSelector == null)
                    {
                        Debug.LogErrorFormat("Convert to SelectViewTemplateDelegate failed. delegate={0}", value);
                    }
                }
            }
        }

        protected bool UseDefaultViewSelection
        {
            get { return string.IsNullOrEmpty(viewSelectorPath); }
        }

        void Start()
        {
            if (!CheckConfig())
            {
                return;
            }

            if (UseDefaultViewSelection)
            {
                // create name dictionary
                viewNameDictionary = new Dictionary<string, GameObject>();

                foreach (var item in configs)
                {
                    viewNameDictionary.Add(item.name, item.viewTemplate);
                }
            }

            if (usePool)
            {
                var templates = new List<GameObject>();
                foreach (var config in configs)
                {
                    // add to list
                    templates.Add(config.viewTemplate);
                }

                // get pool
                viewPool = ViewPoolManager.Instance.CreateDynamicPool(templates);
            }

            CreateBinding();

            BindingUtility.AddBinding(bindingList, transform, out dataContext);
        }

        void OnDestroy()
        {
            BindingUtility.RemoveBinding(bindingList, dataContext);
        }

        protected bool CheckConfig()
        {
            if (configs.Length == 0)
            {
                Debug.LogError("configs is empty");
                return false;
            }

            foreach (var config in configs)
            {
                var viewTemplate = config.viewTemplate;
                if (viewTemplate == null)
                {
                    Debug.LogError("viewTemplate is null");
                    return false;
                }

                // check if view template contains DataContext component
                if (viewTemplate.GetComponent<IDataContext>() == null)
                {
                    Debug.LogError("ViewTemplate need IDataContext component", gameObject);
                    return false;
                }
            }

            return true;
        }

        protected virtual void CreateBinding()
        {
            bindingList = new List<IBinding>();

            // create selector binding
            if (!UseDefaultViewSelection)
            {
                var binding = new Binding(viewSelectorPath, this, "ViewSelector", Binding.BindingMode.OneWay, Binding.ConversionMode.None, null);
                binding.SetFlags(Binding.ControlFlags.ResetTargetValue);

                bindingList.Add(binding);
            }

            // create collection binding
            collectionBinding = new CollectionBinding(collectionPath, this);
            bindingList.Add(collectionBinding);

            if (!string.IsNullOrEmpty(collectionViewPath))
            {
                // create collection view
                collectionBinding.CreateCollectionView();

                // create collection view binding
                var binding = new Binding(collectionViewPath, this, "CollectionView", Binding.BindingMode.OneWayToSource);
                binding.SetFlags(Binding.ControlFlags.ResetSourceValue);

                bindingList.Add(binding);
            }
        }

        private GameObject GetViewTemplate(object item)
        {
            var type = item.GetType();
            var typeName = type.Name;

            GameObject result = null;
            if (!viewNameDictionary.TryGetValue(typeName, out result))
            {
                // return null if name not match
                return null;
            }

            return result;
        }

        #region IViewFactory

        public GameObject CreateItemView(object item)
        {
            GameObject viewTemplate = null;

            if (UseDefaultViewSelection)
            {
                // get view template by type name
                viewTemplate = GetViewTemplate(item);
            }
            else
            {
                // invoke delegate
                viewTemplate = viewSelector.Invoke(item, configs);
            }

            if (viewTemplate == null)
            {
                // use default view template
                viewTemplate = defaultViewTemplate;
            }

            GameObject view;
            if (viewPool != null)
            {
                // use pool
                view = viewPool.GetViewObject(viewTemplate);
            }
            else
            {
                // create new view
                view = GameObject.Instantiate<GameObject>(viewTemplate);
            }

            // add it to container
            view.transform.SetParent(container, false);

            // active view if not
            if (!view.activeSelf)
            {
                view.SetActive(true);
            }

            return view;
        }

        public void ReleaseItemView(GameObject view)
        {
            if (viewPool != null)
            {
                viewPool.ReleaseViewObject(view);
            }
            else
            {
                // just destroy it
                GameObject.Destroy(view);
            }
        }

        public virtual void UpdateView()
        {
            
        }

        #endregion
    }
}
