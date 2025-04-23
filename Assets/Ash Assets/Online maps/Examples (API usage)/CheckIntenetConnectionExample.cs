/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to test the connection to the Internet.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/CheckIntenetConnectionExample")]
    public class CheckIntenetConnectionExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        private void Start()
        {
            // If map is not specified, use the current instance.
            if (map == null) map = OnlineMaps.instance;
            
            // Begin to check your Internet connection.
            map.CheckServerConnection(OnCheckConnectionComplete);
        }

        /// <summary>
        /// When the connection test is completed, this function will be called.
        /// </summary>
        /// <param name="status">Result of the test.</param>
        private void OnCheckConnectionComplete(bool status)
        {
            // If the test is successful, then allow the user to manipulate the map.
            map.control.allowUserControl = status;

            // Showing test result in console.
            Debug.Log(status ? "Has connection" : "No connection");
        }
    }
}