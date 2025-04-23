/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of calculating the amount of clicking on the marker.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/MarkerClickCountExample")]
    public class MarkerClickCountExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map control. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMapsControlBase3D control;
        
        /// <summary>
        /// Prefab of 3D marker
        /// </summary>
        public GameObject prefab;

        private void Start()
        {
            // If the control is not specified, get the current instance.
            if (control == null) control = OnlineMapsControlBase3D.instance;
            
            // Create a new markers.
            OnlineMapsMarker3D marker1 = control.marker3DManager.Create(0, 0, prefab);
            OnlineMapsMarker3D marker2 = control.marker3DManager.Create(10, 0, prefab);

            // Store click count in marker custom data.
            marker1["clickCount"] = 0;
            marker2["clickCount"] = 0;

            // Subscribe to click event.
            marker1.OnClick += OnMarkerClick;
            marker2.OnClick += OnMarkerClick;

            marker1.OnPress += OnPress;
        }

        private void OnPress(OnlineMapsMarkerBase onlineMapsMarkerBase)
        {
            Debug.Log("OnPress");
        }

        private void OnMarkerClick(OnlineMapsMarkerBase marker)
        {
            int clickCount = marker.Get<int>("clickCount");
            clickCount++;
            Debug.Log(clickCount);
            marker["clickCount"] = clickCount;
        }
    }    
}