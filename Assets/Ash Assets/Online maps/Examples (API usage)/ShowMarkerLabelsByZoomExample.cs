/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to show marker labels, only in the zoom range.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/ShowMarkerLabelsByZoomExample")]
    public class ShowMarkerLabelsByZoomExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        private void Start()
        {
            // If the map is not specified, get the current instance.
            if (map == null) map = OnlineMaps.instance;

            // Create a new markers.
            OnlineMapsMarker marker1 = map.markerManager.Create(0, 0, null, "Marker 1");
            OnlineMapsMarker marker2 = map.markerManager.Create(10, 0, null, "Marker 2");

            // Store data about labels.
            marker1["data"] = new ShowMarkerLabelsByZoomItem(marker1.label, new OnlineMapsRange(3, 10));
            marker2["data"] = new ShowMarkerLabelsByZoomItem(marker2.label, new OnlineMapsRange(8, 15));

            // Sunscribe to ChangeZoom event.
            map.OnChangeZoom += OnChangeZoom;
            OnChangeZoom();
        }

        private void OnChangeZoom()
        {
            foreach (OnlineMapsMarker marker in map.markerManager)
            {
                ShowMarkerLabelsByZoomItem item = marker["data"] as ShowMarkerLabelsByZoomItem;
                if (item == null) continue;

                // Update marker labels.
                marker.label = item.zoomRange.InRange(map.zoom) ? item.label : "";
            }
        }

        public class ShowMarkerLabelsByZoomItem
        {
            /// <summary>
            /// Zoom range where you need to show the label.
            /// </summary>
            public OnlineMapsRange zoomRange;

            /// <summary>
            /// Label.
            /// </summary>
            public string label;

            public ShowMarkerLabelsByZoomItem(string label, OnlineMapsRange zoomRange)
            {
                this.label = label;
                this.zoomRange = zoomRange;
            }
        }
    }
}