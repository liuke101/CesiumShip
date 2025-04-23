/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to create a marker on click.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/CreateMarkerOnClick")]
    public class CreateMarkerOnClick:MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        private void Start()
        {
            // If map is not specified, use the current instance.
            if (map == null) map = OnlineMaps.instance;
            
            // Subscribe to the click event.
            map.control.OnMapClick += OnMapClick;
        }

        private void OnMapClick()
        {
            // Get the coordinates under the cursor.
            double lng, lat;
            map.control.GetCoords(out lng, out lat);

            // Create a label for the marker.
            string label = "Marker " + (map.markerManager.Count + 1);

            // Create a new marker.
            map.markerManager.Create(lng, lat, label);
        }
    }
}
