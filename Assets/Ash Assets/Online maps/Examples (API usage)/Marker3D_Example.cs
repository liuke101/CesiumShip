/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of dynamic creating 3d marker, and change the position of 3D marker.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/Marker3D_Example")]
    public class Marker3D_Example : MonoBehaviour
    {
        /// <summary>
        /// Reference to the 3D control (Texture or Tileset). If not specified, the current instance will be used.
        /// </summary>
        public OnlineMapsControlBase3D control;
        
        /// <summary>
        /// Prefab of 3D marker
        /// </summary>
        public GameObject markerPrefab;

        private OnlineMapsMarker3D marker3D;

        private void Start()
        {
            // If the control is not specified, get the current instance.
            if (control == null) control = OnlineMapsControlBase3D.instance;

            // Check if the control is 3D.
            if (control == null)
            {
                Debug.LogError("You must use the 3D control (Texture or Tileset).");
                return;
            }

            // Marker position. Geographic coordinates.
            Vector2 markerPosition = new Vector2(0, 0);

            // Create 3D marker
            marker3D = control.marker3DManager.Create(markerPosition, markerPrefab);

            // Specifies that marker should be shown only when zoom from 1 to 10.
            marker3D.range = new OnlineMapsRange(1, 10);
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(5, 5, 100, 20), "Move Left"))
            {
                // Change the marker coordinates.
                Vector2 mPos = marker3D.position;
                mPos.x += 0.1f;
                marker3D.position = mPos;
            }
        }
    }
}