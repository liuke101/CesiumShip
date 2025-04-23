/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to rotate the camera on a compass.
    /// Requires Tileset Control + Allow Camera Control - ON.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/RotateCameraByCompassExample")]
    public class RotateCameraByCompassExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the camera orbit. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMapsCameraOrbit cameraOrbit;

        private void Start()
        {
            // If the camera orbit is not specified, get the current instance.
            if (cameraOrbit == null) cameraOrbit = OnlineMapsCameraOrbit.instance;
            
            // Subscribe to compass event
            OnlineMapsLocationService.instance.OnCompassChanged += OnCompassChanged;
        }

        /// <summary>
        /// This method is called when the compass value is changed.
        /// </summary>
        /// <param name="f">New compass value (0-1)</param>
        private void OnCompassChanged(float f)
        {
            // Rotate the camera.
            cameraOrbit.rotation.y = f * 360;
        }
    }
}