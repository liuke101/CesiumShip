/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of detection of water by texture.
    /// This example requires a texture:
    /// http://infinity-code.com/atlas/online-maps/images/mapForDetectWaterBW4.jpg
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/DetectWaterByTextureExample")]
    public class DetectWaterByTextureExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        /// <summary>
        /// Color of water on the texture.
        /// </summary>
        public Color32 waterColor = Color.black;

        // Set map 2048x2048, with Read / Write Enabled
        public Texture2D mapForDetectWater;

        private void Start()
        {
            if (map == null) map = OnlineMaps.instance;
        }

        private void Update()
        {
            // Check if the P key is released
            if (OnlineMapsInput.GetKeyUp(KeyCode.P))
            {
                // Get the coordinates under the cursor.
                Vector2 mouseCoords = map.control.GetCoords();
                
                // Check if there is water at this point
                bool hasWater = HasWater(mouseCoords.x, mouseCoords.y);
                Debug.Log(hasWater ? "Has Water" : "No Water");
            }
        }

        private bool HasWater(float lng, float lat)
        {
            // Convert geo coordinates to tile coordinates
            double tx, ty;
            map.projection.CoordinatesToTile(lng, lat, 3, out tx, out ty);

            const int countTileRowCol = 8;

            // Convert tile coordinates to texture coordinates (UV)
            tx /= countTileRowCol;
            ty /= countTileRowCol;

            // Check pixel color
            Color color = mapForDetectWater.GetPixelBilinear((float)tx, (float)(1 - ty));
            Debug.Log(tx + "   " + ty);

            return color == waterColor;
        }
    }
}