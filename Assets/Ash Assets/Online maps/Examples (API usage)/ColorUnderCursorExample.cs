/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example how to get the color under the cursor
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/ColorUnderCursorExample")]
    public class ColorUnderCursorExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        private void Start()
        {
            // If map is not specified, use the current instance.
            if (map == null) map = OnlineMaps.instance;
            
            // Subscribe to OnMapClick
            map.control.OnMapClick += OnMapClick;
        }

        /// <summary>
        /// This method is called when the map is clicked.
        /// </summary>
        private void OnMapClick()
        {
            // Get the coordinates under the cursor.
            double lng, lat;
            map.control.GetCoords(out lng, out lat);

            // Convert coordinates to tile position
            double tx, ty;
            map.projection.CoordinatesToTile(lng, lat, map.zoom, out tx, out ty);

            // Get tile index
            int itx = (int)tx;
            int ity = (int)ty;

            // Get tile
            OnlineMapsTile tile = map.tileManager.GetTile(map.zoom, itx, ity);

            // If the tile exists, but is not yet loaded, take the parent
            while (tile != null && tile.status != OnlineMapsTileStatus.loaded)
            {
                tile = tile.parent;
                tx /= 2;
                ty /= 2;
            }

            // If the tile does not exist
            if (tile == null)
            {
                Debug.Log("No loaded tiles under cursor");
                return;
            }

            // Calculate the relative position
            double rx = tx - (int)tx;
            double ry = ty - (int)ty;

            // For Target - Tileset
            if (!map.control.resultIsTexture)
            {
                Color color = tile.texture.GetPixelBilinear((float)rx, 1 - (float)ry);
                Debug.Log(color);
            }
            // For Target - Texture
            else
            {
                int row = (int)((1 - ry) * OnlineMapsUtils.tileSize);
                Color color = (tile as OnlineMapsRasterTile).colors[(int)((row + rx) * OnlineMapsUtils.tileSize)];
                Debug.Log(color);
            }
        }
    }
}