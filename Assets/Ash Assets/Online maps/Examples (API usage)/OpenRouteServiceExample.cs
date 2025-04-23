/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of a request to Open Route Service.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/OpenRouteServiceExample")]
    public class OpenRouteServiceExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        private void Start()
        {
            // If the map is not specified, get the current instance.
            if (map == null) map = OnlineMaps.instance;
            
            // Looking for pedestrian route between the coordinates.
            OnlineMapsOpenRouteServiceDirections.Find(
                new double[]
                {
                    // Coordinates
                    8.6817521f, 49.4173462f,
                    8.6828883f, 49.4067577f
                },
                new OnlineMapsOpenRouteServiceDirections.Params
                {
                    // Extra params
                    language = "ru",
                    profile = OnlineMapsOpenRouteServiceDirections.Profile.footWalking
                }).OnComplete += OnRequestComplete;
        }

        /// <summary>
        /// This method is called when a response is received.
        /// </summary>
        /// <param name="response">Response string</param>
        private void OnRequestComplete(string response)
        {
            Debug.Log(response);

            OnlineMapsOpenRouteServiceDirectionResult result = OnlineMapsOpenRouteServiceDirections.GetResults(response);
            if (result == null || result.routes.Length == 0)
            {
                Debug.Log("Open Route Service Directions failed.");
                return;
            }

            // Get the points of the first route.
            List<OnlineMapsVector2d> points = result.routes[0].points;

            // Draw the route.
            OnlineMapsDrawingLine line = new OnlineMapsDrawingLine(points, Color.red);
            map.drawingElementManager.Add(line);

            // Set the map position to the first point of route.
            map.position = points[0];
        }
    }
}