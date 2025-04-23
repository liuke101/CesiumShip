/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Net;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to use IGN maps.
    /// IGN requires a Referer header to be specified, and this example shows how to add it.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/IGNHook")]
    public class IGNHook: MonoBehaviour
    {
        /// <summary>
        /// Referer header.
        /// </summary>
        public string referer = "https://macarte.ign.fr/carte/";
        
        private void Start()
        {
            // Subscribe to the event of starting downloading a tile.
            OnlineMapsTileManager.OnStartDownloadTile += OnStartDownloadTile;
        }

        /// <summary>
        /// This method is called when the tile starts downloading.
        /// </summary>
        /// <param name="tile">Tile</param>
        private void OnStartDownloadTile(OnlineMapsTile tile)
        {
            string url = tile.url;
            tile.status = OnlineMapsTileStatus.loading;
            
            // Create a new WebClient and add the Referer header.
            WebClient client = new WebClient();
            client.Headers = new WebHeaderCollection()
            {
                { HttpRequestHeader.Referer, referer},
            };
            client.DownloadDataCompleted += (sender, e) => { OnTileDownloaded(tile, e); };
            client.DownloadDataAsync(new Uri(url));
        }

        /// <summary>
        /// This method is called when the tile is downloaded.
        /// </summary>
        /// <param name="tile">Tile</param>
        /// <param name="e">Event data</param>
        private static void OnTileDownloaded(OnlineMapsTile tile, DownloadDataCompletedEventArgs e)
        {
            // If the map is not specified, we do nothing.
            if (tile.map == null) return;
            if (e.Error != null)
            {
                tile.MarkError();
                Debug.Log(e.Error.Message);
                return;
            }

            // Create a new texture and load the downloaded data into it.
            Texture2D t = new Texture2D(1, 1);
            t.LoadImage(e.Result);
            t.wrapMode = TextureWrapMode.Clamp;
            tile.texture = t;
            tile.status = OnlineMapsTileStatus.loaded;
            tile.map.Redraw();
        }
    }
}
