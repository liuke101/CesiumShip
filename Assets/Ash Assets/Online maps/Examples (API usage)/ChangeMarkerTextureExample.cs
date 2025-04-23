/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of a dynamic change texture of 2D marker.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/ChangeMarkerTextureExample")]
    public class ChangeMarkerTextureExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        /// <summary>
        /// New texture for markers. Must have "Read / Write Enabled - ON".
        /// </summary>
        public Texture2D newMarkerTexture;

        private void Start()
        {
            // If map is not specified, use the current instance.
            if (map == null) map = OnlineMaps.instance;
        }

        private void OnGUI()
        {
            // When you click on ...
            if (GUI.Button(new Rect(10, 10, 100, 20), "Change markers"))
            {
                // ... all markers will change the texture.
                foreach (OnlineMapsMarker marker in map.markerManager)
                {
                    marker.texture = newMarkerTexture;
                    marker.Init();
                }

                // Redraw map
                map.Redraw();
            }
        }
    }
}