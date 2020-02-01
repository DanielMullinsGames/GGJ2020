using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// CanvasPrinter works like a printer that it creates the view objects based on the CanvasData.
    ///
    /// CanvasPrinter works with ScrollRect, If the scroll position is changed, it calculates the
    /// visible sections and add/remove view objects. It use reference count to manage all created
    /// view objects.
    ///
    /// The ItemAdded/ItemRemoved event will be triggered if new NodeItem is added/removed.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class CanvasPrinter : MonoBehaviour
    {
        public class NodeItem
        {
            public CanvasData.Node node;
            public RectTransform transform;
            public int referencedCount;

            public void Reset()
            {
                node = null;
                transform = null;
                referencedCount = 0;
            }
        }

        /// <summary>
        /// Helper class to delay releasing view objects.
        /// </summary>
        public class DelayedViewPool
        {
            private Dictionary<GameObject, Stack<GameObject>> poolDictionary;
            private Dictionary<GameObject, GameObject> instanceDictionary;
            private IViewPool viewPool;

            public DelayedViewPool(IViewPool viewPool)
            {
                if (viewPool == null)
                {
                    throw new ArgumentNullException("viewPool");
                }

                poolDictionary = new Dictionary<GameObject, Stack<GameObject>>();
                instanceDictionary = new Dictionary<GameObject, GameObject>();
                this.viewPool = viewPool;
            }

            public GameObject GetViewObject(GameObject prefab)
            {
                Stack<GameObject> freeObjects;
                if (!poolDictionary.TryGetValue(prefab, out freeObjects))
                {
                    // add it
                    freeObjects = new Stack<GameObject>();
                    poolDictionary.Add(prefab, freeObjects);
                }

                GameObject viewObject = null;

                if (freeObjects.Count > 0)
                {
                    // use pool
                    viewObject = freeObjects.Pop();
                }
                else
                {
                    // call internal view pool
                    viewObject = viewPool.GetViewObject(prefab);

                    if (!instanceDictionary.ContainsKey(viewObject))
                    {
                        // add to instance dictionary
                        instanceDictionary.Add(viewObject, prefab);
                    }
                }

                return viewObject;
            }

            public void ReleaseViewObject(GameObject viewObject)
            {
                // get prefab
                GameObject prefab;
                if (!instanceDictionary.TryGetValue(viewObject, out prefab))
                {
                    Debug.LogError("Unknown instance", viewObject);
                    return;
                }

                // add it to pool
                var freeObjects = poolDictionary[prefab];
                freeObjects.Push(viewObject);
            }

            public void Release()
            {
                foreach (var kvp in poolDictionary)
                {
                    var freeObjects = kvp.Value;

                    foreach (var view in freeObjects)
                    {
                        viewPool.ReleaseViewObject(view);
                    }

                    freeObjects.Clear();
                }
            }
        }

        public CanvasData data;
        public bool enableDebug;

        private DelayedViewPool viewPool;
        private ObjectPool<NodeItem> nodeItemPool;
        private ScrollRect scrollRect;
        private RectTransform contentRT;
        private IndexRange lastRange;

        private List<int> tempIndexList;
        private List<NodeItem> nodeItemList;
        private Dictionary<CanvasData.Node, NodeItem> nodeDictionary;

        private event Action<NodeItem> NodeItemAdded;
        private event Action<NodeItem> NodeItemRemoved;

        void Start()
        {
            // get scroll rect
            scrollRect = GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                Debug.LogError("Require ScrollRect Component", gameObject);
                return;
            }

            // create containers
            nodeDictionary = new Dictionary<CanvasData.Node, NodeItem>();
            nodeItemList = new List<NodeItem>();
            tempIndexList = new List<int>();

            // create view pool
            var localViewPool = LocalViewPool.Create(transform);
            viewPool = new DelayedViewPool(localViewPool);

            // create object pool
            nodeItemPool = new ObjectPool<NodeItem>(() => new NodeItem(), x => x.Reset());

            InitContent();

            UpdateView();

            // add listener
            scrollRect.onValueChanged.AddListener(OnScrollRectChanged);
        }

        void OnDestroy()
        {
            // remove listener
            scrollRect.onValueChanged.RemoveListener(OnScrollRectChanged);
        }

        private void InitContent()
        {
            // get content rect transform
            contentRT = scrollRect.content;

            // set pivot
            contentRT.pivot = new Vector2(0f, 1f);

            // set position and size
            contentRT.anchoredPosition = new Vector2(0f, 0f);
            contentRT.sizeDelta = new Vector2(data.width, data.height);

            lastRange = IndexRange.Empty;
        }

        private void OnScrollRectChanged(Vector2 pos)
        {
            UpdateView();
        }

        private IndexRange GetVisibleSectionRange()
        {
            // get viewport rect
            var viewportRect = scrollRect.viewport.rect;

            // check viewport rect
            if (viewportRect.width == 0f || viewportRect.height == 0f)
            {
                // fallback to current node
                viewportRect = ((RectTransform)transform).rect;

                if (viewportRect.width == 0f || viewportRect.height == 0f)
                {
                    // return empty if viewport is invalid
                    return IndexRange.Empty;
                }
            }

            int startIndex;
            int endIndex;

            var pos = contentRT.anchoredPosition;
            var offset = new Vector2(-pos.x, pos.y);
            var rect = new Rect(offset, viewportRect.size);

            if (data.layout == CanvasData.LayoutType.Horizontal)
            {
                startIndex = (int)(rect.xMin / data.sectionWidth);
                endIndex = (int)(rect.xMax / data.sectionWidth);
            }
            else
            {
                startIndex = (int)(rect.yMin / data.sectionHeight);
                endIndex = (int)(rect.yMax / data.sectionHeight);
            }

            // clamp index
            int count = data.sectionList.Count;
            startIndex = Mathf.Clamp(startIndex, 0, count - 1);
            endIndex = Mathf.Clamp(endIndex, 0, count - 1);

            var range = new IndexRange(startIndex, endIndex);
            return range;
        }

        private void Subtract(IndexRange lhs, IndexRange rhs, List<int> list)
        {
            list.Clear();

            if (lhs == IndexRange.Empty)
            {
                return;
            }

            if (rhs == IndexRange.Empty)
            {
                for (int index = lhs.Min; index <= lhs.Max; index++)
                {
                    list.Add(index);
                }

                return;
            }

            for (int index = lhs.Min; index <= lhs.Max; index++)
            {
                if (index > rhs.Max || index < rhs.Min)
                {
                    list.Add(index);
                }
            }
        }

        private NodeItem GetItem(CanvasData.Node node)
        {
            NodeItem value;
            if (nodeDictionary.TryGetValue(node, out value))
            {
                return value;
            }

            return null;
        }

        private NodeItem AddItem(CanvasData.Node node)
        {
            // create view
            var view = viewPool.GetViewObject(node.prefab);

            // set as child
            var rt = view.GetComponent<RectTransform>();
            rt.SetParent(contentRT, false);

            // set pos and size
            rt.anchoredPosition = node.anchoredPosition;
            rt.sizeDelta = node.sizeDelta;
            rt.rotation = Quaternion.Euler(0f, 0f, node.rotation);

            // active view if not
            if (!view.activeSelf)
            {
                view.SetActive(true);
            }

            if (!string.IsNullOrEmpty(node.metadata))
            {
                var serializableCanvasNode = view.GetComponent<ISerializableCanvasNode>();
                Assert.IsNotNull(serializableCanvasNode);

                // deserialize metadata
                serializableCanvasNode.Deserialize(node.metadata);
            }

            // create item
            var item = nodeItemPool.GetObject();

            item.node = node;
            item.transform = rt;

            // add it
            nodeDictionary.Add(node, item);
            nodeItemList.Add(item);

            if (NodeItemAdded != null)
            {
                // raise event
                NodeItemAdded.Invoke(item);
            }

            return item;
        }

        private void RemoveItem(CanvasData.Node node)
        {
            var item = nodeDictionary[node];

            // get view
            var view = item.transform.gameObject;

            // release view
            viewPool.ReleaseViewObject(view);

            // remove it
            nodeDictionary.Remove(node);
            nodeItemList.Remove(item);

            if (NodeItemRemoved != null)
            {
                // raise event
                NodeItemRemoved.Invoke(item);
            }

            // release item
            nodeItemPool.PutObject(item);
        }

        private void SortItemList()
        {
            // sort based on node index
            nodeItemList.Sort((x, y) => x.node.index.CompareTo(y.node.index));

            // set transform index
            int index = 0;
            foreach (var item in nodeItemList)
            {
                item.transform.SetSiblingIndex(index);
                index++;
            }
        }

        public void UpdateView()
        {
            var currentRange = GetVisibleSectionRange();

            // check if range changed
            if (currentRange == lastRange)
            {
                return;
            }

            // get removed
            Subtract(lastRange, currentRange, tempIndexList);
            foreach (var index in tempIndexList)
            {
                var section = data.sectionList[index];

                if (enableDebug)
                {
                    Debug.LogFormat("Remove section: {0}", section);
                }

                // iterate node list
                foreach (var node in section.nodeList)
                {
                    var item = GetItem(node);

                    // decrease reference count
                    item.referencedCount--;

                    if (item.referencedCount == 0)
                    {
                        // remove item if ref count is zero
                        RemoveItem(node);
                    }
                }
            }

            // get added 
            Subtract(currentRange, lastRange, tempIndexList);
            foreach (var index in tempIndexList)
            {
                var section = data.sectionList[index];

                if (enableDebug)
                {
                    Debug.LogFormat("Add section: {0}", section);
                }

                // iterate node list
                foreach (var node in section.nodeList)
                {
                    var item = GetItem(node);

                    if (item == null)
                    {
                        item = AddItem(node);
                    }

                    // increase reference count
                    item.referencedCount++;
                }
            }

            // do the real releasing
            viewPool.Release();

            // sort transform list
            SortItemList();

            // update last range
            lastRange = currentRange;
        }

        public void RegisterEvents(Action<NodeItem> nodeItemAddedAction, Action<NodeItem> nodeItemRemovedAction)
        {
            NodeItemAdded += nodeItemAddedAction;
            NodeItemRemoved += nodeItemRemovedAction;

            // process all added items
            foreach (var kvp in nodeDictionary)
            {
                nodeItemAddedAction(kvp.Value);
            }
        }

        public void UnregisterEvents(Action<NodeItem> nodeItemAddedAction, Action<NodeItem> nodeItemRemovedAction)
        {
            NodeItemAdded -= nodeItemAddedAction;
            NodeItemRemoved -= nodeItemRemovedAction;

            // process all added items
            foreach (var kvp in nodeDictionary)
            {
                nodeItemRemovedAction(kvp.Value);
            }
        }
    }
}
