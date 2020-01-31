using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// ListDynamicBinder is a collection binder which binds to IList object.
    ///
    /// Unlike the CollectionBinder, it only creates and binds the visible items of the list. It
    /// requires a dynamic controller to calculate the visible items and handle the layout. It's very
    /// useful for scroll views with a large amount of items, e.g. leaderboard.
    /// </summary>
    [Binder]
    [AddComponentMenu("Peppermint/Data Binding/Binder/Collection/List Dynamic Binder")]
    public class ListDynamicBinder : MonoBehaviour, IDynamicViewFactory
    {
        public string collectionPath;
        public string collectionViewPath;
        public GameObject viewTemplate;
        public Transform container;
        public bool usePool;

        protected ListDynamicBinding listDynamicBinding;
        protected List<IBinding> bindingList;
        protected IDataContext dataContext;
        protected IDynamicController dynamicController;
        protected IViewPool viewPool;
        protected int viewIndex;

        public ICollectionView CollectionView
        {
            get
            {
                return listDynamicBinding.CollectionView;
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

            // get dynamic controller
            dynamicController = GetComponent<IDynamicController>();
            if (dynamicController == null)
            {
                Debug.LogError("Need DynamicController component", gameObject);
                return;
            }

            if (usePool)
            {
                // get pool
                viewPool = ViewPoolManager.Instance.GetViewPool(viewTemplate);
            }
            else
            {
                // create local view pool
                viewPool = LocalViewPool.Create(transform);
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

            // create list dynamic binding
            listDynamicBinding = new ListDynamicBinding(collectionPath, this);
            bindingList.Add(listDynamicBinding);

            if (!string.IsNullOrEmpty(collectionViewPath))
            {
                // create collection view
                listDynamicBinding.CreateCollectionView();

                // create collection view binding
                var binding = new Binding(collectionViewPath, this, "CollectionView", Binding.BindingMode.OneWayToSource);
                binding.SetFlags(Binding.ControlFlags.ResetSourceValue);

                bindingList.Add(binding);
            }

            InitDynamicController();
        }

        protected virtual void InitDynamicController()
        {
            // initialize controller with viewTemplate
            dynamicController.Init(listDynamicBinding.DynamicBindingAccessor, viewTemplate);
        }

        #region IDynamicViewFactory

        public GameObject CreateItemView(object item)
        {
            GameObject view = viewPool.GetViewObject(viewTemplate);

            if (Application.isEditor)
            {
                // set unique name
                view.name = string.Format("{0} {1}", viewTemplate.name, viewIndex++);
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
            viewPool.ReleaseViewObject(view);
        }

        public void UpdateView()
        {
            // redirect to controller
            dynamicController.UpdateView();
        }

        public void GetDynamicItems(List<object> dynamicList)
        {
            // clear list
            dynamicList.Clear();

            // redirect to controller
            dynamicController.GetDynamicItems(dynamicList);
        }

        #endregion
    }
}
