/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to handle change of the position and zoom the map.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/ChangePositionAndZoomEventsExample")]
    public class ChangePositionAndZoomEventsExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;

        private void Start()
        {
            // If map is not specified, use the current instance.
            if (map == null) map = OnlineMaps.instance;
            
            // Subscribe to change position event.
            map.OnChangePosition += OnChangePosition;

            // Subscribe to change zoom event.
            map.OnChangeZoom += OnChangeZoom;
        }

        /// <summary>
        /// This method is called when the position of the map is changed.
        /// </summary>
        private void OnChangePosition()
        {
            // When the position changes you will see in the console new map coordinates.
            Debug.Log(map.position);
        }

        /// <summary>
        /// This method is called when the zoom of the map is changed.
        /// </summary>
        private void OnChangeZoom()
        {
            // When the zoom changes you will see in the console new zoom.
            Debug.Log(map.floatZoom);
        }
    }
}