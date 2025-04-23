/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of calculating the size of area.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/CalcAreaExample")]
    public class CalcAreaExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        /// <summary>
        /// Texture of marker
        /// </summary>
        public Texture2D markerTexture;

        /// <summary>
        /// Line width.
        /// </summary>
        public float borderWidth = 1;
        
        private float _borderWidth;
        private bool changed = false;
        private OnlineMapsControlBase control;
        private OnlineMapsDrawingElementManager drawingElementManager;
        private List<OnlineMapsMarker> markers = new List<OnlineMapsMarker>();
        private OnlineMapsMarkerManager markerManager;
        private List<Vector2> markerPositions = new List<Vector2>();
        private OnlineMapsDrawingPoly polygon;


        private void Start()
        {
            // If the map is not specified, use the current instance.
            if (map == null) map = OnlineMaps.instance;
            
            // Get references to the control, drawingElementManager and markerManager.
            control = map.control;
            drawingElementManager = control.drawingElementManager;
            markerManager = control.markerManager;

            _borderWidth = borderWidth;
        }

        public void Clear()
        {
            if (polygon != null)
            {
                drawingElementManager.Remove(polygon);
                polygon = null;
            }

            foreach (OnlineMapsMarker marker in markers) markerManager.Remove(marker);
            markers.Clear();

            markerPositions.Clear();
            changed = true;
        }

        private void CheckMarkerPositions()
        {
            // Check the position of each marker.
            for (int i = 0; i < markers.Count; i++)
            {
                if (markerPositions[i] != markers[i].position)
                {
                    // If the position marker changed, then change the value in markerPositions. 
                    // In the polygon value changes automatically.
                    markerPositions[i] = markers[i].position;
                    changed = true;
                }
            }
        }

        private void OnMouseUp()
        {
            if (OnlineMapsInput.GetKey(KeyCode.LeftShift))
            {
                // Get the geographical coordinates of the cursor.
                Vector2 cursorCoords = control.GetCoords();

                // Create a new marker at the specified coordinates.
                OnlineMapsMarker marker = markerManager.Create(cursorCoords, markerTexture, "Marker " + (markerManager.Count + 1));

                // Save marker and coordinates.
                markerPositions.Add(cursorCoords);
                markers.Add(marker);

                // Mark that markers changed.
                changed = true;
            }
        }

        private void Update()
        {
            if (Math.Abs(_borderWidth - borderWidth) > float.Epsilon)
            {
                _borderWidth = borderWidth;
                if (polygon != null)
                {
                    polygon.borderWidth = borderWidth;
                    map.Redraw();
                }
            }

            // Check the position of the markers.
            CheckMarkerPositions();

            // If nothing happens, then return.
            if (!changed) return;
            changed = false;

            // If the number of points is less than 3, then return.
            if (markers.Count < 3)
            {
                map.Redraw();
                return;
            }

            // If the polygon is not created, then create.
            if (polygon == null)
            {
                // For points, reference to markerPositions. 
                // If you change the values in markerPositions, value in the polygon will be adjusted automatically.
                polygon = new OnlineMapsDrawingPoly(markerPositions, Color.black, borderWidth, new Color(1, 1, 1, 0.3f));

                // Add an element to the map.
                drawingElementManager.Add(polygon);
            }

            // Calculates area of the polygon.
            // Important: this algorithm works correctly only if the lines do not intersect.
            float area = 0;

            // Triangulate points.
            int[] indexes = OnlineMapsUtils.Triangulate(markerPositions).ToArray();

            // Calculate the area of each triangle.
            for (int i = 0; i < indexes.Length / 3; i++)
            {
                // Get the points of the triangle.
                Vector2 p1 = markerPositions[indexes[i * 3]];
                Vector2 p2 = markerPositions[indexes[i * 3 + 1]];
                Vector2 p3 = markerPositions[indexes[i * 3 + 2]];

                // Calculate the distance between points.
                float d1 = OnlineMapsUtils.DistanceBetweenPoints(p1, p2).magnitude;
                float d2 = OnlineMapsUtils.DistanceBetweenPoints(p2, p3).magnitude;
                float d3 = OnlineMapsUtils.DistanceBetweenPoints(p3, p1).magnitude;

                // Calculate the area.
                float p = (d1 + d2 + d3) / 2;
                area += Mathf.Sqrt(p * (p - d1) * (p - d2) * (p - d3));
            }

            Debug.Log("Area: " + area + " km^2");

            map.Redraw();
        }
    }
}