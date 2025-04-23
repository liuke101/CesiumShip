/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to make a map that will be the real world size.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/RealTilesetSizeExample")]
    public class RealTilesetSizeExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map control. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMapsTileSetControl control;
        
        private void Start()
        {
            // Get the reference to the map control.
            if (control == null) control = OnlineMapsTileSetControl.instance;
            
            // Initial resize
            UpdateSize();

            // Subscribe to change zoom
            control.map.OnChangeZoom += OnChangeZoom;
        }

        private void OnChangeZoom()
        {
            UpdateSize();
        }

        private void UpdateSize()
        {
            // Get distance (km) between corners of map
            Vector2 distance = OnlineMapsUtils.DistanceBetweenPoints(control.map.topLeftPosition,
                control.map.bottomRightPosition);

            // Set tileset size
            control.sizeInScene = distance * 1000;

            // Redraw map
            control.map.Redraw();
        }
    }
}