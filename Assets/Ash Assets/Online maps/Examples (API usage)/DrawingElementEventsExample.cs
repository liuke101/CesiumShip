/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of using the events of DrawingElement.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/DrawingElementEventsExample")]
    public class DrawingElementEventsExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;

        private void Start()
        {
            if (map == null) map = OnlineMaps.instance;
            
            // Create a new rect element.
            OnlineMapsDrawingRect rect = new OnlineMapsDrawingRect(-119.0807f, 34.58658f, 3, 3, Color.black, 1f,
                Color.blue);

            // Subscribe to events.
            rect.OnClick += OnClick;
            rect.OnPress += OnPress;
            rect.OnRelease += OnRelease;
            rect.OnDoubleClick += OnDoubleClick;
            
            // Add element to map.
            map.drawingElementManager.Add(rect);

            // Create a new poly element.
            List<Vector2> polyPoints = new List<Vector2>
            {
                //Geographic coordinates
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(2, 2),
                new Vector2(0, 1)
            };
            
            OnlineMapsDrawingPoly poly = new OnlineMapsDrawingPoly(polyPoints, Color.red, 1f);
            
            // Create tooltip for poly.
            poly.tooltip = "Drawing Element";

            // Subscribe to events.
            poly.OnClick += OnClick;
            poly.OnPress += OnPress;
            poly.OnRelease += OnRelease;
            poly.OnDoubleClick += OnDoubleClick;
            
            // Add element to map.
            map.drawingElementManager.Add(poly);
        }

        private void OnDoubleClick(OnlineMapsDrawingElement drawingElement)
        {
            Debug.Log("OnDoubleClick");
        }

        private void OnRelease(OnlineMapsDrawingElement drawingElement)
        {
            Debug.Log("OnRelease");
        }

        private void OnPress(OnlineMapsDrawingElement drawingElement)
        {
            Debug.Log("OnPress");
        }

        private void OnClick(OnlineMapsDrawingElement drawingElement)
        {
            Debug.Log("OnClick");
        }
    }
}