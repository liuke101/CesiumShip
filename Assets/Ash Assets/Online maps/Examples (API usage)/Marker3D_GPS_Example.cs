/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to dynamically create a 3D marker in the GPS locations of user.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/Marker3D_GPS_Example")]
    public class Marker3D_GPS_Example : MonoBehaviour
    {
        /// <summary>
        /// Reference to the 3D control (Texture or Tileset). If not specified, the current instance will be used.
        /// </summary>
        public OnlineMapsControlBase3D control;
        
        /// <summary>
        /// Prefab of 3D marker
        /// </summary>
        public GameObject prefab;

        private OnlineMapsMarker3D locationMarker;

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

            //Create a marker to show the current GPS coordinates.
            locationMarker = control.marker3DManager.Create(Vector2.zero, prefab);

            //Hide handle until the coordinates are not received.
            locationMarker.enabled = false;

            // Gets Location Service Component.
            OnlineMapsLocationService ls = OnlineMapsLocationService.instance;

            if (ls == null)
            {
                Debug.LogError(
                    "Location Service not found.\nAdd Location Service component (Component / Infinity Code / Online Maps / Plugins / Location Service).");
                return;
            }

            //Subscribe to the GPS coordinates change
            ls.OnLocationChanged += OnLocationChanged;
            ls.OnCompassChanged += OnCompassChanged;

            //Subscribe to zoom change
            control.map.OnChangeZoom += OnChangeZoom;
        }

        private void OnChangeZoom()
        {
            //Example of scaling object
            int zoom = control.map.zoom;

            if (zoom >= 5 && zoom < 10)
            {
                float s = 10f / (2 << (zoom - 5));
                Transform markerTransform = locationMarker.transform;
                if (markerTransform != null) markerTransform.localScale = new Vector3(s, s, s);

                // show marker
                locationMarker.enabled = true;
            }
            else
            {
                // Hide marker
                locationMarker.enabled = false;
            }
        }

        private void OnCompassChanged(float f)
        {
            //Set marker rotation
            Transform markerTransform = locationMarker.transform;
            if (markerTransform != null) markerTransform.rotation = Quaternion.Euler(0, f * 360, 0);
        }

        //This event occurs at each change of GPS coordinates
        private void OnLocationChanged(Vector2 position)
        {
            //Change the position of the marker to GPS coordinates
            locationMarker.position = position;

            //If the marker is hidden, show it
            if (!locationMarker.enabled) locationMarker.enabled = true;
        }
    }
}