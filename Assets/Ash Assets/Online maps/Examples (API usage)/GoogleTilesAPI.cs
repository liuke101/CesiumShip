/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of using Google Tiles API.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/GoogleTilesAPI")]
    public class GoogleTilesAPI : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        /// <summary>
        /// Google API Key
        /// </summary>
        public string apiKey;
        
        /// <summary>
        /// Map type
        /// </summary>
        public string mapType = "roadmap";
        
        /// <summary>
        /// Language
        /// </summary>
        public string language = "ja-JP";
        
        /// <summary>
        /// Region
        /// </summary>
        public string region = "region";

        /// <summary>
        /// URL of the request
        /// </summary>
        private string url = "https://www.googleapis.com/tile/v1/tiles/{zoom}/{x}/{y}?session={sessiontoken}&key={apikey}";
        
        /// <summary>
        /// Session token
        /// </summary>
        private string sessiontoken;

        private void Start()
        {
            // If the map is not specified, then find the map.
            if (map == null) map = OnlineMaps.instance;
            
            // Create a new map type
            OnlineMapsProvider.CreateMapType("google.tilesapi", url);
            
            // Handle events to replace tokens and start downloading tiles
            OnlineMapsTile.OnReplaceURLToken += OnReplaceUrlToken;
            OnlineMapsTileManager.OnStartDownloadTile += OnStartDownloadTile;

            // Select map type
            OnlineMaps.instance.mapType = "google.tilesapi";

            // Start getting session token
            StartCoroutine(GetSessionToken());
        }

        /// <summary>
        /// Gets session token
        /// </summary>
        private IEnumerator GetSessionToken()
        {
            // Create json parameters for request
            OnlineMapsJSONObject jReq = new OnlineMapsJSONObject();
            jReq.Add("mapType", mapType);
            jReq.Add("language", language);
            jReq.Add("region", region);
            
            // Convert json to bytes
            byte[] requestBytes = Encoding.UTF8.GetBytes(jReq.ToString());

            // Create request
            UnityWebRequest www = new UnityWebRequest("https://tile.googleapis.com/tile/v1/createSession?key=" + apiKey, "POST");
            www.SetRequestHeader("Content-Type", "application/json");
            www.uploadHandler = new UploadHandlerRaw(requestBytes);
            www.downloadHandler = new DownloadHandlerBuffer();

            // Send request
            yield return www.SendWebRequest();

            // If there was an error, then exit the method.
#if UNITY_2020_1_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                Debug.Log(www.error + "\n" + Encoding.UTF8.GetString(www.downloadHandler.data));
                yield break;
            }

            // Get session token from response
            string response = Encoding.UTF8.GetString(www.downloadHandler.data);
            OnlineMapsJSONItem json = OnlineMapsJSON.Parse(response);
            sessiontoken = json.V<string>("session");

            // Iterate through all tiles to restart downloading
            map.tileManager.tiles.ForEach(t =>
            {
                if (t.status == OnlineMapsTileStatus.error) t.status = OnlineMapsTileStatus.none;
            });

            // Redraw map
            map.Redraw();
        }

        /// <summary>
        /// Replaces tokens in the URL
        /// </summary>
        /// <param name="tile">Tile</param>
        /// <param name="token">Token</param>
        /// <returns>Vale of token</returns>
        private string OnReplaceUrlToken(OnlineMapsTile tile, string token)
        {
            if (token == "sessiontoken") return sessiontoken;
            if (token == "apikey") return apiKey;
            return null;
        }

        /// <summary>
        /// Occurs when the tile starts downloading.
        /// </summary>
        /// <param name="tile">Tile</param>
        private void OnStartDownloadTile(OnlineMapsTile tile)
        {
            // If there is no session token, mark the tile as an error and exit the method.
            if (string.IsNullOrEmpty(sessiontoken))
            {
                tile.status = OnlineMapsTileStatus.error;
                return;
            }

            // Pass the tile to built-in tile downloader
            OnlineMapsTileManager.StartDownloadTile(tile);
        }
    }
}