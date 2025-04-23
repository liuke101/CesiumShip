/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to make the overlay from MapTiler tiles.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/TilesetMapTilerOverlayExample")]
    public class TilesetMapTilerOverlayExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        // Overlay transparency
        [Range(0, 1)]
        public float alpha = 1;

        private float _alpha = 1;

        private void Start()
        {
            // If the map is not specified, get the current instance.
            if (map == null) map = OnlineMaps.instance;
            
            if (OnlineMapsCache.instance != null)
            {
                // Subscribe to the cache events.
                OnlineMapsCache.instance.OnLoadedFromCache += LoadTileOverlay;
            }

            // Subscribe to the tile download event.
            OnlineMapsTileManager.OnStartDownloadTile += OnStartDownloadTile;
        }

        private static void LoadTileOverlay(OnlineMapsTile tile)
        {
            // Load overlay for tile from Resources.
            string path = string.Format("OnlineMapsOverlay/{0}/{1}/{2}", tile.zoom, tile.x, tile.y);
            Texture2D texture = Resources.Load<Texture2D>(path);
            if (texture != null)
            {
                tile.overlayBackTexture = Instantiate(texture);
                Resources.UnloadAsset(texture);
            }
        }

        private void OnStartDownloadTile(OnlineMapsTile tile)
        {
            // Load overlay for tile.
            LoadTileOverlay(tile);

            // Load the tile using a standard loader.
            OnlineMapsTileManager.StartDownloadTile(tile);
        }

        private void Update()
        {
            // Update the transparency of overlay.
            if (Math.Abs(_alpha - alpha) > float.Epsilon)
            {
                _alpha = alpha;
                lock (OnlineMapsTile.lockTiles)
                {
                    foreach (OnlineMapsTile tile in map.tileManager.tiles) tile.overlayBackAlpha = alpha;
                }
            }
        }
    }
}