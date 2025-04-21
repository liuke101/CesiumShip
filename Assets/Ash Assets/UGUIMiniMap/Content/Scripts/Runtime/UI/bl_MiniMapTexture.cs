using UnityEngine;
using UnityEngine.EventSystems;

namespace Lovatto.MiniMap
{
    public class bl_MiniMapTexture : MonoBehaviour, IPointerClickHandler
    {
        private RectTransform Area;
        private bl_MiniMap MiniMap;

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            Area = GetComponent<RectTransform>();
            MiniMap = transform.root.GetComponentInChildren<bl_MiniMap>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!MiniMap.AllowMapMarks) return;
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Area, eventData.pressPosition, eventData.pressEventCamera, out Vector2 localPoint))
            {
                localPoint.x /= Area.rect.width;
                localPoint.y /= Area.rect.height;

                Vector2 absolutePosition = new Vector3(localPoint.x * 2, localPoint.y * 2);

                Vector2 ms = MiniMap.miniMapCamera.pixelRect.size * 0.5f;
                Vector3 RelativeScreen = new Vector2((absolutePosition.x * ms.x) + ms.x, (absolutePosition.y * ms.y) + ms.y);
                SpawnPointer(RelativeScreen);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        void SpawnPointer(Vector2 position)
        {
            Ray ray = MiniMap.miniMapCamera.ScreenPointToRay(position);
            if (Physics.Raycast(ray, out RaycastHit raycast))
            {
                MiniMap.SetPointMark(raycast.point);
                bl_MiniMapEvents.onSelectPosition?.Invoke(raycast.point);
            }
        }

        public Vector2 GetRectSize()
        {
            return Area.rect.size;
        }
    }
}