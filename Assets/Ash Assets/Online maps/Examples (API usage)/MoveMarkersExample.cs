/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of an animated marker moving between locations.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/MoveMarkersExample")]
    public class MoveMarkersExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        /// <summary>
        /// Time of movement between locations.
        /// </summary>
        public float time = 10;

        private OnlineMapsMarker marker;

        private Vector2 fromPosition;
        private Vector2 toPosition;

        /// <summary>
        /// Relative position (0-1) between from and to
        /// </summary>
        private float angle = 0.5f;

        /// <summary>
        /// Move direction
        /// </summary>
        private int direction = 1;

        private void Start()
        {
            // If the map is not specified, get the current instance.
            if (map == null) map = OnlineMaps.instance;
            
            // Create a new marker.
            marker = map.markerManager.Create(map.position);
            fromPosition = map.topLeftPosition;
            toPosition = map.bottomRightPosition;
        }

        private void Update()
        {
            angle += Time.deltaTime / time * direction;
            if (angle > 1)
            {
                angle = 2 - angle;
                direction = -1;
            }
            else if (angle < 0)
            {
                angle *= -1;
                direction = 1;
            }

            // Set new position
            marker.position = Vector2.Lerp(fromPosition, toPosition, angle);

            // Marks the map should be redrawn.
            // Map is not redrawn immediately. It will take some time.
            map.Redraw();
        }
    }
}