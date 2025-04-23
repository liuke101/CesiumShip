/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;
using UnityEngine.UI;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to make a tooltip using uGUI for a single marker
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/uGUICustomTooltipExample")]
    public class uGUICustomTooltipExample : MonoBehaviour
    {
        /// <summary>
        /// Prefab of the tooltip
        /// </summary>
        public GameObject tooltipPrefab;

        /// <summary>
        /// Container for tooltip
        /// </summary>
        public Canvas container;

        public OnlineMapsRawImageTouchForwarder forwarder;

        private OnlineMapsMarker marker;
        private GameObject tooltip;

	    private void Start ()
        {
            marker = OnlineMapsMarkerManager.CreateItem(Vector2.zero, "Hello World");
            marker.OnDrawTooltip = delegate {  };

            OnlineMaps.instance.OnUpdateLate += OnUpdateLate;
        }

        public Vector2 GetScreenPosition(double lng, double lat, float yOffset)
        {
            double px, py;
            OnlineMaps map = marker.manager.map;
            OnlineMapsTileSetControl control = map.control as OnlineMapsTileSetControl;
            control.GetPosition(lng, lat, out px, out py);
            py -= yOffset;
            px /= map.buffer.renderState.width;
            py /= map.buffer.renderState.height;

            double cpx = -control.sizeInScene.x * px;
            double cpy = control.sizeInScene.y * py;

            double tlx, tly, brx, bry;
            map.GetCorners(out tlx, out tly, out brx, out bry);

            float elevation = 0;
            if (control.hasElevation)
            {
                float elevationScale = OnlineMapsElevationManagerBase.GetBestElevationYScale(control.elevationManager, tlx, tly, brx, bry);
                elevation = control.elevationManager.GetElevationValue(cpx, cpy, elevationScale, tlx, tly, brx, bry);
            }
            Vector3 worldPos = transform.position + transform.rotation * new Vector3((float)(cpx * transform.lossyScale.x), elevation * transform.lossyScale.y, (float)(cpy * transform.lossyScale.z));

            Camera cam = control.activeCamera != null ? control.activeCamera : Camera.main;
            return cam.WorldToScreenPoint(worldPos);
        }

        private void OnUpdateLate()
        {
            OnlineMapsMarkerBase tooltipMarker = OnlineMapsTooltipDrawerBase.tooltipMarker;
            if (tooltipMarker == marker)
            {
                if (tooltip == null)
                {
                    tooltip = Instantiate(tooltipPrefab) as GameObject;
                    (tooltip.transform as RectTransform).SetParent(container.transform);
                }
                Vector2 screenPosition = GetScreenPosition(marker.position.x, marker.position.y, marker.height + 10);
                if (forwarder != null) screenPosition = forwarder.MapToForwarderSpace(screenPosition);
                Vector2 point;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(container.transform as RectTransform, screenPosition, null, out point);
                (tooltip.transform as RectTransform).localPosition = point;
                tooltip.GetComponentInChildren<Text>().text = marker.label;

            }
            else if (tooltip != null)
            {
                OnlineMapsUtils.Destroy(tooltip);
                tooltip = null;
            }
        }
    }
}