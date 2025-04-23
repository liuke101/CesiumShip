/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of use Online Maps Drawing API.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/DrawingAPI Example")]
    public class DrawingAPI_Example : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        private void Start()
        {
            // If map is not specified, use the current instance.
            if (map == null) map = OnlineMaps.instance;
            
            List<Vector2> linePoints = new List<Vector2>
            {
                //Geographic coordinates
                new Vector2(3, 3),
                new Vector2(5, 3),
                new Vector2(4, 4),
                new Vector2(9.3f, 6.5f)
            };

            List<Vector2> polyPoints = new List<Vector2>
            {
                //Geographic coordinates
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(2, 2),
                new Vector2(0, 1)
            };

            // Draw line
            OnlineMapsDrawingLine line = new OnlineMapsDrawingLine(linePoints, Color.green, 5);
            map.drawingElementManager.Add(line);

            // Draw filled transparent poly
            OnlineMapsDrawingPoly poly = new OnlineMapsDrawingPoly(polyPoints, Color.red, 1, new Color(1, 1, 1, 0.5f));
            map.drawingElementManager.Add(poly);

            // Draw filled rectangle
            // (position, size, borderColor, borderWidth, backgroundColor)
            OnlineMapsDrawingRect rect = new OnlineMapsDrawingRect(new Vector2(2, 2), new Vector2(1, 1), Color.green, 1, Color.blue);
            map.drawingElementManager.Add(rect);
        }
    }
}