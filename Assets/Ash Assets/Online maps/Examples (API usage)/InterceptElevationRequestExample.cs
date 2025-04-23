/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to intercept the request to the elevation data, and send elevation value to Online Maps.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/InterceptElevationRequestExample")]
    public class InterceptElevationRequestExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the Bing Maps Elevation Manager. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMapsBingMapsElevationManager elevationManager;

        private void Start()
        {
            // If the elevation manager is not specified, get the current instance.
            if (elevationManager == null) elevationManager = OnlineMapsBingMapsElevationManager.instance;

            // Intercept elevation request
            elevationManager.OnGetElevation += OnGetElevation;
        }

        private void OnGetElevation(double leftLongitude, double topLatitude, double rightLongitude, double bottomLatitude)
        {
            // Elevation map must be 32x32
            short[,] elevation = new short[32, 32];

            // Here you get the elevation from own sources.

            // Set elevation map
            elevationManager.SetElevationData(elevation);
        }
    }
}