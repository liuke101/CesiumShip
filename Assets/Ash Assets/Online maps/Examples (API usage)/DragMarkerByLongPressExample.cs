/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to drag the markers by long press. It is convenient for mobile devices.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/DragMarkerByLongPressExample")]
    public class DragMarkerByLongPressExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        private void Start()
        {
            // If map is not specified, use the current instance.
            if (map == null) map = OnlineMaps.instance;
            
            // Create a new marker.
            OnlineMapsMarker marker = map.markerManager.Create(map.position, "My Marker");

            // Subscribe to OnLongPress event.
            marker.OnLongPress += OnMarkerLongPress;
        }

        private void OnMarkerLongPress(OnlineMapsMarkerBase marker)
        {
            // Starts moving the marker.
            map.control.dragMarker = marker;
            map.control.isMapDrag = false;
        }
    }
}