/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of the conversion of screen coordinates into geographic coordinates.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/ScreenToGeoExample")]
    public class ScreenToGeoExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map control. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMapsControlBase control;

        private void Start()
        {
            // If the control is not specified, get the current instance.
            if (control == null) control = OnlineMapsControlBase.instance;
        }
        
        private void Update()
        {
            // Screen coordinate of the cursor.
            Vector3 mousePosition = OnlineMapsInput.mousePosition;

            // Converts the screen coordinates to geographic.
            Vector3 mouseGeoLocation = control.GetCoords(mousePosition);

            // Showing geographical coordinates in the console.
            Debug.Log(mouseGeoLocation);
        }
    }
}