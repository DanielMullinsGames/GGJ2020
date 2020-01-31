using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Dynamic controller for VerticalLayoutGroupController.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class VerticalLayoutGroupController : MonoBehaviour, IDynamicController
    {
        public VerticalLayoutGroup layoutGroup;

        private float itemHeight;
        private ScrollRect scrollRect;
        private RectTransform contentRT;
        private RectTransform layoutGroupRT;

        private IDynamicBindingAccessor accessor;
        private IndexRange lastRange;

        public bool IsAccessorReady
        {
            get
            {
                if (accessor == null || !accessor.IsBound)
                {
                    return false;
                }

                return true;
            }
        }

        void Awake()
        {
            if (layoutGroup == null)
            {
                Debug.LogError("layoutGroup is null", gameObject);
                return;
            }

            // get scroll rect
            scrollRect = GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                Debug.LogError("Require ScrollRect component", gameObject);
                return;
            }

            lastRange = IndexRange.Empty;

            // get rect transform
            contentRT = scrollRect.content;
            layoutGroupRT = layoutGroup.transform as RectTransform;

            // set pivot to top
            contentRT.pivot = new Vector2(0.5f, 1f);

            // add listener
            scrollRect.onValueChanged.AddListener(OnScrollRectChanged);
        }

        void OnDestroy()
        {
            // remove listener
            scrollRect.onValueChanged.RemoveListener(OnScrollRectChanged);
        }

        private int GetItemIndexV(float y)
        {
            int index = 0;
            var padding = layoutGroup.padding;

            if (y < padding.top + itemHeight)
            {
                index = 0;
            }
            else
            {
                float delta = y - (padding.top + itemHeight);
                index = (int)(delta / (layoutGroup.spacing + itemHeight)) + 1;
            }

            return index;
        }

        private IndexRange GetVisibleRange()
        {
            // get viewport height
            var viewportHeight = scrollRect.viewport.rect.height;

            // check height
            if (viewportHeight == 0.0f)
            {
                // use height of current rect
                var rt = (RectTransform)transform;
                viewportHeight = rt.rect.height;

                if (viewportHeight == 0.0f)
                {
                    // return empty if height is invalid
                    return IndexRange.Empty;
                }
            }

            // calculate visible range
            float top = contentRT.anchoredPosition.y;
            float bottom = contentRT.anchoredPosition.y + viewportHeight;

            int startIndex = GetItemIndexV(top);
            int endIndex = GetItemIndexV(bottom);

            int count = accessor.ItemCount;

            // check count
            if (count == 0 || startIndex >= count)
            {
                return IndexRange.Empty;
            }

            // clamp index
            startIndex = Mathf.Clamp(startIndex, 0, count - 1);
            endIndex = Mathf.Clamp(endIndex, 0, count - 1);

            var range = new IndexRange(startIndex, endIndex);
            return range;
        }

        private void UpdateContentSize()
        {
            // get items count
            int count = accessor.ItemCount;
            var padding = layoutGroup.padding;

            // calculate content size
            float height = padding.top + padding.bottom + Mathf.Max(0, count - 1) * layoutGroup.spacing + itemHeight * count;
            var newContentSize = new Vector2(0f, height);

            // update content size
            if (contentRT.sizeDelta != newContentSize)
            {
                contentRT.sizeDelta = newContentSize;
            }
        }

        void OnScrollRectChanged(Vector2 pos)
        {
            if (!IsAccessorReady)
            {
                return;
            }

            // get visible range
            var currentRange = GetVisibleRange();

            // check if range changed
            if (currentRange == lastRange)
            {
                return;
            }

            // update last range
            lastRange = currentRange;

            accessor.UpdateDynamicList();
            UpdateView();
        }

        #region IDynamicController

        public void Init(IDynamicBindingAccessor accessor, object args)
        {
            this.accessor = accessor;

            // get layout element
            var viewTemplate = (GameObject)args;
            var layoutElement = viewTemplate.GetComponent<ILayoutElement>();
            if (layoutElement == null)
            {
                Debug.LogError("ILayoutElement is null", gameObject);
                return;
            }

            // get height
            itemHeight = layoutElement.preferredHeight;
        }

        public void GetDynamicItems(List<object> dynamicList)
        {
            if (!IsAccessorReady)
            {
                return;
            }

            var visibleRange = GetVisibleRange();

            if (visibleRange == IndexRange.Empty)
            {
                // no item
                return;
            }

            // add items
            for (int index = visibleRange.Min; index <= visibleRange.Max; index++)
            {
                var item = accessor.GetItem(index);
                dynamicList.Add(item);
            }
        }

        public void UpdateView()
        {
            if (!IsAccessorReady)
            {
                return;
            }

            UpdateContentSize();

            // calculate layout group position
            float top = contentRT.anchoredPosition.y;
            int startIndex = GetItemIndexV(top);

            float posY = 0f;
            if (startIndex > 0)
            {
                posY = startIndex * (itemHeight + layoutGroup.spacing);
            }

            // set layout group position
            layoutGroupRT.anchoredPosition = new Vector2(0f, -posY);
        }

        #endregion
    }
}