using UnityEngine;
using UnityEngine.EventSystems;

namespace Lovatto.MiniMap
{
    public class bl_MiniMapDragArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IScrollHandler
    {
        private bl_MiniMap MiniMap;
        private Vector2 origin;
        private Vector2 direction;
        private bool touched;
        private int pointerID;

        private Texture2D cursorIcon;

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            MiniMap = transform.root.GetComponentInChildren<bl_MiniMap>();
            direction = Vector2.zero;
            touched = false;
            cursorIcon = MiniMap.DragCursorIcon;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void OnPointerDown(PointerEventData data)
        {
            if (MiniMap == null || !MiniMap.CanDragMiniMap)
                return;
            if (!MiniMap.isMobile && data.button != PointerEventData.InputButton.Right)
            {
                return;
            }

            if (!touched)
            {
                touched = true;
                pointerID = data.pointerId;
                origin = data.position;
                Cursor.SetCursor(cursorIcon, MiniMap.HotSpot, CursorMode.ForceSoftware);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void OnDrag(PointerEventData data)
        {
            if (MiniMap == null || !MiniMap.CanDragMiniMap)
                return;
            if (!MiniMap.isMobile && data.button != PointerEventData.InputButton.Right)
            {
                return;
            }

            if (data.pointerId == pointerID)
            {
                Vector2 currentPosition = data.position;
                Vector2 directionRaw = currentPosition - origin;
                direction = (directionRaw * Time.deltaTime);

                MiniMap.SetDragPosition(direction);
                origin = data.position;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void OnPointerUp(PointerEventData data)
        {
            if (MiniMap == null || !MiniMap.CanDragMiniMap)
                return;
            if (!MiniMap.isMobile && data.button != PointerEventData.InputButton.Right)
            {
                return;
            }

            if (data.pointerId == pointerID)
            {
                direction = Vector2.zero;
                touched = false;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public void OnScroll(PointerEventData eventData)
        {
            if (MiniMap == null) return;

            if (eventData.scrollDelta.y > 0)
            {
                MiniMap.ChangeZoom(false);
            }
            else if (eventData.scrollDelta.y < 0)
            {
                MiniMap.ChangeZoom(true);
            }
        }
    }
}