using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Dynamic controller for HorizontalLayoutGroup.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class HorizontalLayoutGroupController : MonoBehaviour, IDynamicController
    {
        public HorizontalLayoutGroup layoutGroup;

        private float itemWidth;
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

            // set pivot to left
            contentRT.pivot = new Vector2(0f, 0.5f);

            // add listener
            scrollRect.onValueChanged.AddListener(OnScrollRectChanged);
        }

        void OnDestroy()
        {
            // remove listener
            scrollRect.onValueChanged.RemoveListener(OnScrollRectChanged);
        }
        
        private int GetItemIndexH(float x)
        {
            int index = 0;
            var padding = layoutGroup.padding;

            if (x < padding.left + itemWidth)
            {
                index = 0;
            }
            else
            {
                float delta = x - (padding.left + itemWidth);
                index = (int)(delta / (layoutGroup.spacing + itemWidth)) + 1;
            }

            return index;
        }

        private IndexRange GetVisibleRange()
        {
            // get viewport width
            var viewportWidth = scrollRect.viewport.rect.width;

            // check width
            if (viewportWidth == 0.0f)
            {
                // use width of current rect
                var rt = (RectTransform)transform;
                viewportWidth = rt.rect.width;

                if (viewportWidth == 0.0f)
                {
                    // return empty if width is invalid
                    return IndexRange.Empty;
                }
            }

            // calculate visible range
            float left = -contentRT.anchoredPosition.x;
            float right = -contentRT.anchoredPosition.x + viewportWidth;

            int startIndex = GetItemIndexH(left);
            int endIndex = GetItemIndexH(right);

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
            float width = padding.left + padding.right + Mathf.Max(0, count - 1) * layoutGroup.spacing + itemWidth * count;
            var newContentSize = new Vector2(width, 0f);

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

            // get width
            itemWidth = layoutElement.preferredWidth;
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
            float left = -contentRT.anchoredPosition.x;
            int startIndex = GetItemIndexH(left);

            float posX = 0f;
            if (startIndex > 0)
            {
                posX = startIndex * (itemWidth + layoutGroup.spacing);
            }

            // set layout group position
            layoutGroupRT.anchoredPosition = new Vector2(posX, 0f);
        }

        #endregion
    }
}