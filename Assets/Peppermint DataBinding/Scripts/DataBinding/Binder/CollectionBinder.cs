using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// CollectionBinder handles collection binding.
    ///
    /// View template is the UI template for collection item, it must contains a DataContext
    /// component to work with data binding. Collection path is the source path of collection
    /// binding. The container is the parent of all created view, CollectionBinding does not handle
    /// any layout calculation, so it can be used with any layout group. Collection view path is the
    /// source path of the created CollectionView, if you do not use it just leave it empty. Enable
    /// use pool can improve the performance, see ViewPoolManager for more information.
    ///
    /// ObservableList is the best collection type for collection binding. Although the source type
    /// can be any IEnumerable type, such as T[], List, these types do not implement
    /// INotifyCollectionChanged interface, so the view will not get updated if new item is added or
    /// deleted. If you know the collection is not changed after binding, you can bind to these
    /// types.
    /// </summary>
    [Binder]
    [AddComponentMenu("Peppermint/Data Binding/Binder/Collection/Collection Binder")]
    public class CollectionBinder : MonoBehaviour, IViewFactory
    {
        public string collectionPath;
        public string collectionViewPath;
        public GameObject viewTemplate;
        public Transform container;
        public bool usePool;

        protected CollectionBinding collectionBinding;
        protected List<IBinding> bindingList;
        protected IDataContext dataContext;
        protected IViewPool viewPool;

        public ICollectionView CollectionView
        {
            get
            {
                return collectionBinding.CollectionView;
            }
        }

        void Start()
        {
            // check if view template contains DataContext component
            if (viewTemplate.GetComponent<IDataContext>() == null)
            {
                Debug.LogError("ViewTemplate need IDataContext component", gameObject);
                return;
            }

            if (usePool)
            {
                // get pool
                viewPool = ViewPoolManager.Instance.GetViewPool(viewTemplate);
            }

            CreateBinding();

            BindingUtility.AddBinding(bindingList, transform, out dataContext);
        }

        void OnDestroy()
        {
            BindingUtility.RemoveBinding(bindingList, dataContext);
        }

        protected virtual void CreateBinding()
        {
            bindingList = new List<IBinding>();

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

        #region IViewFactory

        public GameObject CreateItemView(object item)
        {
            GameObject view = null;

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
            view.transform.SetAsLastSibling();

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
