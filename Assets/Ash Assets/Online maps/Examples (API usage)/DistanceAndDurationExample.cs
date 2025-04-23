/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Linq;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example how to calculate the distance and the duration of the route
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/DistanceAndDurationExample")]
    public class DistanceAndDurationExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        /// <summary>
        /// Google API Key
        /// </summary>
        public string googleAPIKey;

        private void Start()
        {
            if (map == null) map = OnlineMaps.instance;

            if (string.IsNullOrEmpty(googleAPIKey))
            {
                Debug.LogWarning("Please specify Google API Key");
                return;
            }

            // Find route using Google Directions API
            // Origin and destination can be specified as coordinates or addresses
            string origin = "Los Angeles";
            Vector2 destination = new Vector2(-118.178960f, 35.063995f);
            OnlineMapsGoogleDirections request = new OnlineMapsGoogleDirections(googleAPIKey, origin, destination);
            request.OnComplete += OnGoogleDirectionsComplete;
            request.Send();
        }

        /// <summary>
        /// This method is called when the response from Google Directions API is received
        /// </summary>
        /// <param name="response">Response from Google Direction API</param>
        private void OnGoogleDirectionsComplete(string response)
        {
            Debug.Log(response);

            // Try load result
            OnlineMapsGoogleDirectionsResult result = OnlineMapsGoogleDirections.GetResult(response);
            if (result == null || result.routes.Length == 0) return;

            // Get the first route
            OnlineMapsGoogleDirectionsResult.Route route = result.routes[0];

            // Draw route on the map
            OnlineMapsDrawingLine line = new OnlineMapsDrawingLine(route.overview_polyline, Color.red, 3);
            map.drawingElementManager.Add(line);

            // Calculate the distance
            int distance = route.legs.Sum(l => l.distance.value); // meters

            // Calculate the duration
            int duration = route.legs.Sum(l => l.duration.value); // seconds

            // Log distane and duration
            Debug.Log("Distance: " + distance + " meters, or " + (distance / 1000f).ToString("F2") + " km");
            Debug.Log("Duration: " + duration + " sec, or " + (duration / 60f).ToString("F1") + " min, or " + (duration / 3600f).ToString("F1") + " hours");
        }
    }
}
