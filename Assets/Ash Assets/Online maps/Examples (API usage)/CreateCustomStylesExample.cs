/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to dynamically create custom styles
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/CreateCustomStylesExample")]
    public class CreateCustomStylesExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        /// <summary>
        /// URL of the first style.
        /// </summary>
        public string style1 = "https://a.tiles.mapbox.com/v4/mapbox.satellite/{zoom}/{x}/{y}.png?access_token=";
        
        /// <summary>
        /// URL of the second style.
        /// </summary>
        public string style2 = "https://a.tiles.mapbox.com/v4/mapbox.streets/{zoom}/{x}/{y}.png?access_token=";
        
        /// <summary>
        /// Mapbox Access Token
        /// </summary>
        public string mapboxAccessToken;

        /// <summary>
        /// Indicates which style is currently used.
        /// </summary>
        private bool useFirstStyle = true;

        private void Start()
        {
            // If map is not specified, use the current instance.
            if (map == null) map = OnlineMaps.instance;
            
            // Create a new provider
            OnlineMapsProvider.Create("myprovider").AppendTypes(
                // Create a new map types
                new OnlineMapsProvider.MapType("style1") { urlWithLabels = style1 + mapboxAccessToken }
            );
            
            // Another way to create a map type
            OnlineMapsProvider.CreateMapType("myprovider.style2", style2 + mapboxAccessToken);
            
            // Get a provider
            OnlineMapsProvider provider = OnlineMapsProvider.Get("myprovider");
            Debug.Log($"Provider: {provider.title}, count types: {provider.types.Length}");

            // Select map type
            map.mapType = "myprovider.style1";
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Change Style"))
            {
                useFirstStyle = !useFirstStyle;
                
                // Switch map type
                map.mapType = "myprovider.style" + (useFirstStyle ? "1" : "2");
            }
        }
    }
}