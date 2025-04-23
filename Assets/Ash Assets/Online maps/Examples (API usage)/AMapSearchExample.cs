/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Search for a POIs, by using AMap search
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/AMapSearchExample")]
    public class AMapSearchExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        /// <summary>
        /// AMap API Key
        /// </summary>
        public string key;

        private void Start()
        {
            // If map is not specified, use the current instance.
            if (map == null) map = OnlineMaps.instance;
            
            // Start a new search
            OnlineMapsAMapSearch.Find(new OnlineMapsAMapSearch.TextParams(key)
            {
                // Params of request
                keywords = "北京大学",
                city = "beijing",

            }).OnComplete += OnRequestComplete; // Subscribe to OnComplete event
        }

        /// <summary>
        /// This method will be called when the search is completed.
        /// </summary>
        /// <param name="response">Response</param>
        private void OnRequestComplete(string response)
        {
            // Log response
            Debug.Log(response);

            // Load result object
            OnlineMapsAMapSearchResult result = OnlineMapsAMapSearch.GetResult(response);

            // Validate result and status
            if (result == null || result.status != 1) return;

            foreach (OnlineMapsAMapSearchResult.POI poi in result.pois)
            {
                // Get POI location
                double lng, lat;
                poi.GetLocation(out lng, out lat);

                // Create a new marker for each POI
                map.markerManager.Create(lng, lat, poi.name);
            }
        }
    }
}