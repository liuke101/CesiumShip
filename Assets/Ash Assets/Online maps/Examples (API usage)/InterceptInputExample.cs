/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to override the input, and use the center of screen as the cursor, and Z key as a left mouse button.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/InterceptInputExample")]
    public class InterceptInputExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map control. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMapsControlBase control;
        
        private void Start()
        {
            // If the control is not specified, get the current instance.
            if (control == null) control = OnlineMapsControlBase.instance;
            
            // Intercepts getting the cursor coordinates.
            control.OnGetInputPosition += OnGetInputPosition;

            // Intercepts getting the number of touches.
            control.OnGetTouchCount += OnGetTouchCount;
        }

        private Vector2 OnGetInputPosition()
        {

#if !UNITY_EDITOR
            // On the device returns center of screen.
            return Camera.main.ViewportToScreenPoint(new Vector3(0.5F, 0.5F, 0));
 #else
            // In the editor returns the coordinates of the mouse cursor.
            return OnlineMapsInput.mousePosition;
#endif
        }

        private int OnGetTouchCount()
        {
            // If pressed Z, then it will work like the left mouse button.
            return OnlineMapsInput.GetKey(KeyCode.Z) ? 1 : 0;
        }
    }
}