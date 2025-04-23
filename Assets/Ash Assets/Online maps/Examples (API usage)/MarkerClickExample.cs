/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to create a click event for dynamic markers and markers created by the inspector.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/MarkerClickExample")]
    public class MarkerClickExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        private void Start()
        {
            // If the map is not specified, get the current instance.
            if (map == null) map = OnlineMaps.instance;

            // Add OnClick events to static markers
            foreach (OnlineMapsMarker marker in map.markerManager)
            {
                marker.OnClick += OnMarkerClick;
            }

            // Add OnClick events to dynamic markers
            OnlineMapsMarker dynamicMarker = map.markerManager.Create(Vector2.zero, null, "Dynamic marker");
            dynamicMarker.OnClick += OnMarkerClick;
        }

        private void OnMarkerClick(OnlineMapsMarkerBase marker)
        {
            // Show in console marker label.
            Debug.Log(marker.label);
        }
    }
}