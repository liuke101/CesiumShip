/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to change the sort order of the markers.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/TilesetMarkerDepthExample")]
    public class TilesetMarkerDepthExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the control. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMapsTileSetControl control;
        
        private void Start()
        {
            if (control == null) control = OnlineMapsTileSetControl.instance;

            // Create markers.
            control.markerManager.Create(0, 0);
            control.markerManager.Create(0, 0.01f);
            control.markerManager.Create(0, -0.01f);

            // Sets a new comparer.
            OnlineMapsMarkerFlatDrawer drawer = control.markerDrawer as OnlineMapsMarkerFlatDrawer;
            if (drawer != null) drawer.markerComparer = new MarkerComparer();

            // Get the center point and zoom the best for all markers.
            Vector2 center;
            int zoom;
            OnlineMapsUtils.GetCenterPointAndZoom(control.markerManager.ToArray(), out center, out zoom);

            // Change the position and zoom of the map.
            control.map.position = center;
            control.map.zoom = zoom;
        }

        /// <summary>
        /// Defines a new comparer.
        /// </summary>
        public class MarkerComparer : IComparer<OnlineMapsMarker>
        {
            public int Compare(OnlineMapsMarker m1, OnlineMapsMarker m2)
            {
                if (m1.position.y > m2.position.y) return -1;
                if (m1.position.y < m2.position.y) return 1;
                return 0;
            }
        }
    }
}