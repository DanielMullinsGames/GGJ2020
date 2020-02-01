using UnityEngine;
using UnityEngine.EventSystems;

namespace Peppermint.DataBinding.Example
{
    public class DragPanel : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public RectTransform container;

        private Vector2 pointerOffset;
        private RectTransform rectTransform;

        void Awake()
        {
            rectTransform = transform as RectTransform;
            if (rectTransform == null)
            {
                Debug.LogError("Require RectTransform");
            }

            if (container == null)
            {
                Debug.LogError("Container is null");
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            rectTransform.SetAsLastSibling();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out pointerOffset);
        }

        public void OnDrag(PointerEventData eventData)
        {
            // clamp position
            Vector2 pointerPosition = ClampToWindow(eventData.position);

            Vector2 localPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(container, pointerPosition, eventData.pressEventCamera, out localPosition))
            {
                rectTransform.localPosition = localPosition - pointerOffset;
            }
        }

        private Vector2 ClampToWindow(Vector2 position)
        {
            Vector3[] canvasCorners = new Vector3[4];
            container.GetWorldCorners(canvasCorners);

            float clampedX = Mathf.Clamp(position.x, canvasCorners[0].x, canvasCorners[2].x);
            float clampedY = Mathf.Clamp(position.y, canvasCorners[0].y, canvasCorners[2].y);

            return new Vector2(clampedX, clampedY);
        }
    }

}
