/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Linq;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Calculates the center and the best  zoom for all markers on the map, and show it.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/GetCenterPointOfMarkersExample")]
    public class GetCenterPointOfMarkersExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;

        private void Start()
        {
            // If the map is not specified, get the current instance.
            if (map == null) map = OnlineMaps.instance;
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(5, 5, 100, 20), "Center"))
            {
                Vector2 center;
                int zoom;

                // Get the center point and zoom the best for all markers.
                OnlineMapsUtils.GetCenterPointAndZoom(map.markerManager.ToArray(), out center, out zoom);

                // Change the position and zoom of the map.
                map.position = center;
                map.zoom = zoom;
            }
        }
    }
}