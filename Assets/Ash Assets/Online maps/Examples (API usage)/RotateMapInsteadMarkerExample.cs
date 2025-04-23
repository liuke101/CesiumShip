/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of rotation of the camera together with a marker.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/RotateMapInsteadMarkerExample")]
    public class RotateMapInsteadMarkerExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        /// <summary>
        /// Reference to the camera orbit. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMapsCameraOrbit cameraOrbit;
        
        private OnlineMapsMarker marker;

        private void Start()
        {
            // If the map is not specified, get the current instance.
            if (map == null) map = OnlineMaps.instance;
            
            // If the camera orbit is not specified, get the current instance.
            if (cameraOrbit == null) cameraOrbit = OnlineMapsCameraOrbit.instance;

            // Create a new marker.
            marker = map.markerManager.Create(new Vector2(), "Player");

            // Subscribe to UpdateBefore event.
            map.OnUpdateBefore += OnUpdateBefore;
        }

        private void OnUpdateBefore()
        {
            // Update camera rotation
            cameraOrbit.rotation = new Vector2(30, marker.rotation * 360);
        }
    }
}