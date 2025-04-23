/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of a request to Bing Maps Location API, and get the result.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/BingMapsLocationAPIExample")]
    public class BingMapsLocationAPIExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;

        /// <summary>
        /// Search query
        /// </summary>
        public string query = "New York";

        private void Start()
        {
            if (!OnlineMapsKeyManager.hasBingMaps)
            {
                Debug.LogError("Bing Maps API Key is missing. Specify the key in Key Manager.");
                return;
            }
            
            // If the map is not specified, then find the map.
            if (map == null) map = OnlineMaps.instance;
            
            // Looking for a location by name.
            OnlineMapsBingMapsLocation.FindByQuery(query, OnlineMapsKeyManager.BingMaps()).OnComplete += OnRequestComplete;

            // Subscribe to map click event.
            map.control.OnMapClick += OnMapClick;
        }

        /// <summary>
        /// This method is called when click on map.
        /// </summary>
        private void OnMapClick()
        {
            // Looking for a location by coordinates.
            OnlineMapsBingMapsLocation.FindByPoint(map.position, OnlineMapsKeyManager.BingMaps()).OnComplete += OnRequestComplete;
        }

        /// <summary>
        /// This method is called when a response is received.
        /// </summary>
        /// <param name="response">Response string</param>
        private static void OnRequestComplete(string response)
        {
            Debug.Log(response);

            // Get an array of results.
            OnlineMapsBingMapsLocationResult[] results = OnlineMapsBingMapsLocation.GetResults(response);
            if (results == null)
            {
                Debug.Log("No results");
                return;
            }

            // Log results info.
            Debug.Log(results.Length);
            foreach (OnlineMapsBingMapsLocationResult result in results)
            {
                Debug.Log(result.name);
                Debug.Log(result.formattedAddress);
                foreach (KeyValuePair<string, string> pair in result.address)
                {
                    Debug.Log(pair.Key + ": " + pair.Value);
                }
                Debug.Log("------------------------------");
            }
        }
    }
}