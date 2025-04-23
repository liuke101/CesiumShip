/*         INFINITY CODE         */
/*   https://infinity-code.com   */

#if !UNITY_WP_8_1 || UNITY_EDITOR

using System.IO;
using System.Text;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example how to make a runtime caching tiles.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/CacheTilesExample")]
    public class CacheTilesExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        private static StringBuilder builder = new StringBuilder();

        private void Start()
        {
            // If the map is not specified, use the current instance.
            if (map == null) map = OnlineMaps.instance;
            
            // Subscribe to the event of success download tile.
            OnlineMapsTile.OnTileDownloaded += OnTileDownloaded;

            // Intercepts requests to the download of the tile.
            OnlineMapsTileManager.OnStartDownloadTile += OnStartDownloadTile;
        }

        /// <summary>
        /// Gets the local path for tile.
        /// </summary>
        /// <param name="tile">Reference to tile</param>
        /// <returns>Local path for tile</returns>
        private static string GetTilePath(OnlineMapsTile tile)
        {
            OnlineMapsRasterTile rTile = tile as OnlineMapsRasterTile;
            
            builder.Length = 0;
            builder.Append(Application.persistentDataPath);
            builder.Append("/OnlineMapsTileCache/");
            builder.Append(rTile.mapType.provider.id);
            builder.Append("/");
            builder.Append(rTile.mapType.id);
            builder.Append("/");
            builder.Append(tile.zoom);
            builder.Append("/");
            builder.Append(tile.x);
            builder.Append("/");
            builder.Append(tile.y);
            builder.Append(".png");
            
            return builder.ToString();
        }

        /// <summary>
        /// This method is called when loading the tile.
        /// </summary>
        /// <param name="tile">Reference to tile</param>
        private void OnStartDownloadTile(OnlineMapsTile tile)
        {
            // Get local path.
            string path = GetTilePath(tile);

            // If the tile is cached.
            if (File.Exists(path))
            {
                // Load tile texture from cache.
                Texture2D tileTexture = new Texture2D(256, 256, TextureFormat.RGB24, false);
                tileTexture.LoadImage(File.ReadAllBytes(path));
                tileTexture.wrapMode = TextureWrapMode.Clamp;

                // Send texture to map.
                if (OnlineMapsControlBase.instance.resultIsTexture)
                {
                    (tile as OnlineMapsRasterTile).ApplyTexture(tileTexture);
                    OnlineMaps.instance.buffer.ApplyTile(tile);
                    OnlineMapsUtils.Destroy(tileTexture);
                }
                else
                {
                    tile.texture = tileTexture;
                    tile.status = OnlineMapsTileStatus.loaded;
                }

                // Redraw map.
                map.Redraw();
            }
            else
            {
                // If the tile is not cached, download tile with a standard loader.
                OnlineMapsTileManager.StartDownloadTile(tile);
            }
        }

        /// <summary>
        /// This method is called when tile is success downloaded.
        /// </summary>
        /// <param name="tile">Reference to tile.</param>
        private void OnTileDownloaded(OnlineMapsTile tile)
        {
            // Get local path.
            string path = GetTilePath(tile);

            // Cache tile.
            FileInfo fileInfo = new FileInfo(path);
            DirectoryInfo directory = fileInfo.Directory;
            if (!directory.Exists) directory.Create();

            File.WriteAllBytes(path, tile.www.bytes);
        }
    }
}

#endif