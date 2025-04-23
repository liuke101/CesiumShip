/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to use a texture from Real World Terrain for tiles.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/Texture From Real World Terrain")]
    public class TextureFromRWT : MonoBehaviour
    {
        /// <summary>
        /// Reference to the Tileset Control.
        /// </summary>
        public OnlineMapsTileSetControl control;

        /// <summary>
        /// Top latitude of the texture.
        /// </summary>
        public double top;

        /// <summary>
        /// Left longitude of the texture.
        /// </summary>
        public double left;

        /// <summary>
        /// Bottom latitude of the texture.
        /// </summary>
        public double bottom;

        /// <summary>
        /// Right longitude of the texture.
        /// </summary>
        public double right;

        /// <summary>
        /// Texture.
        /// </summary>
        public Texture2D texture;

        /// <summary>
        /// Texture for empty tiles.
        /// </summary>
        public Texture2D emptyTexture;

        /// <summary>
        /// Mercator coordinates of the texture.
        /// </summary>
        private double mx1, my1, mx2, my2;

        private void Start()
        {
            // If the control is not specified, get the current instance.
            if (control == null) control = OnlineMapsTileSetControl.instance;
            
            // Convert coordinates to Mercator.
            mx1 = left;
            my1 = top;
            mx2 = right;
            my2 = bottom;
            OnlineMapsUtils.LatLongToMercat(ref mx1, ref my1);
            OnlineMapsUtils.LatLongToMercat(ref mx2, ref my2);
            
            // Handle the OnDrawTile event to draw the texture.
            control.OnDrawTile += OnDrawTile;
            
            OnlineMapsTileManager.OnStartDownloadTile += OnStartDownloadTile;
        }

        /// <summary>
        /// This event occurs when the map draws a tile.
        /// </summary>
        /// <param name="tile">Tile</param>
        /// <param name="material">Material</param>
        private void OnDrawTile(OnlineMapsTile tile, Material material)
        {
            // Get coordinates of the tile.
            OnlineMapsVector2d topLeft = tile.topLeft;
            OnlineMapsVector2d bottomRight = tile.bottomRight;

            // Convert coordinates to Mercator.
            double tmx1 = topLeft.x;
            double tmy1 = topLeft.y;
            double tmx2 = bottomRight.x;
            double tmy2 = bottomRight.y;
            OnlineMapsUtils.LatLongToMercat(ref tmx1, ref tmy1);
            OnlineMapsUtils.LatLongToMercat(ref tmx2, ref tmy2);

            // If the tile is outside the texture, then assign an empty texture.
            if (mx1 > tmx2 || mx2 < tmx1 || my1 > tmy1 || my2 < tmy1)
            {
                material.mainTexture = emptyTexture;
                material.mainTextureOffset = Vector2.zero;
                material.mainTextureScale = Vector2.one;
                return;
            }

            // Assign the texture.
            material.mainTexture = texture;
            
            // Calculate the offset and scale of the texture.
            double ox = (tmx1 - mx1) / (mx2 - mx1);
            double oy = (my2 - tmy2) / (my2 - my1);
            double sx = (tmx2 - tmx1) / (mx2 - mx1);
            double sy = (tmy2 - tmy1) / (my2 - my1);
            material.mainTextureOffset = new Vector2((float)ox, (float)oy);
            material.mainTextureScale = new Vector2((float)sx, (float)sy);
        }

        /// <summary>
        /// This event occurs when the tile starts loading.
        /// </summary>
        /// <param name="tile">Tile</param>
        private void OnStartDownloadTile(OnlineMapsTile tile)
        {
            // Mark the tile as loaded and assign the texture.
            tile.status = OnlineMapsTileStatus.loaded;
            tile.texture = emptyTexture;
        }
    }
}