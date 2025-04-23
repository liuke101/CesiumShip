/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of scaling markers when changing zoom.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/MarkerScaleByZoomExample")]
    public class MarkerScaleByZoomExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        /// <summary>
        /// Zoom, when the scale = 1.
        /// </summary>
        public int defaultZoom = 15;

        /// <summary>
        /// Instance of marker.
        /// </summary>
        private OnlineMapsMarkerBase marker;

        /// <summary>
        /// Init.
        /// </summary>
        private void Start()
        {
            // If the map is not specified, get the current instance.
            if (map == null) map = OnlineMaps.instance;
            
            // Create a new marker.
            marker = map.markerManager.Create(15, 15);

            // Subscribe to change zoom.
            map.OnChangeZoom += OnChangeZoom;

            // Initial rescale marker.
            OnChangeZoom();
        }

        /// <summary>
        /// On change zoom.
        /// </summary>
        private void OnChangeZoom()
        {
            float originalScale = 1 << defaultZoom;
            float currentScale = 1 << map.zoom;

            marker.scale = currentScale / originalScale;
        }
    }
}