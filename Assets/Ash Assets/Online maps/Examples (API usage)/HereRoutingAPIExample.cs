/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of a request to HERE Routing API.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/HereRoutingAPIExample")]
    public class HereRoutingAPIExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        private void Start()
        {
            // If the map is not specified, get the current instance.
            if (map == null) map = OnlineMaps.instance;
            
            // Looking for public transport route between the coordinates.
            OnlineMapsHereRoutingAPI.Find(
                new OnlineMapsHereRoutingAPI.Waypoint(37.38589, 55.90042), // Origin
                new OnlineMapsHereRoutingAPI.Waypoint(37.6853002, 55.8635228), // Destination
                new Dictionary<string, string>
                {
                    {"transportMode", "bus" },
                    {"lang",  "ru-ru"},
                    {"alternatives", "3" },
                    {"return", "polyline,actions,instructions" }
                }
                ).OnComplete += OnComplete;
        }

        /// <summary>
        /// This method is called when a response is received.
        /// </summary>
        /// <param name="response">Response string</param>
        private void OnComplete(string response)
        {
            Debug.Log(response);

            // Get result object
            OnlineMapsHereRoutingAPIResult result = OnlineMapsHereRoutingAPI.GetResult(response);

            if (result != null)
            {
                Color[] colors =
                {
                    Color.green,
                    Color.red,
                    Color.blue,
                    Color.magenta
                };
                int colorIndex = 0;

                // Draw all the routes in different colors.
                foreach (OnlineMapsHereRoutingAPIResult.Route route in result.routes)
                {
                    foreach (OnlineMapsHereRoutingAPIResult.Section section in route.sections)
                    {
                        if (section.polylinePoints2d != null)
                        {
                            OnlineMapsDrawingElement line = new OnlineMapsDrawingLine(section.polylinePoints2d, colors[colorIndex++]);
                            map.drawingElementManager.Add(line);
                        }
                    }
                }
            }
        }
    }
}