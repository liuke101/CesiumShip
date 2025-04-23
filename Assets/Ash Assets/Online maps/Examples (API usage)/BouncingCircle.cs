/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example how to draw bouncing circle on the map.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/BouncingCircle")]
    public class BouncingCircle : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not set, the current instance will be used.
        /// </summary>
        public OnlineMaps map;

        /// <summary>
        /// Animation curve
        /// </summary>
        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        /// <summary>
        /// Duration of animation
        /// </summary>
        public float duration = 3;

        /// <summary>
        /// Radius of the circle
        /// </summary>
        public float radiusKM = 0.1f;

        /// <summary>
        /// Number of segments
        /// </summary>
        public int segments = 32;

        private List<BoundingItem> items;

        /// <summary>
        /// This method is called when the script starts
        /// </summary>
        private void Start()
        {
            if (map == null) map = OnlineMaps.instance;
            
            // Subscribe to click on map event
            map.control.OnMapClick += OnMapClick;
        }

        /// <summary>
        /// This method is called when a user clicks on a map
        /// </summary>
        private void OnMapClick()
        {
            // Get the coordinates under cursor
            double lng, lat;
            map.control.GetCoords(out lng, out lat);
            OnlineMapsVector2d center = new OnlineMapsVector2d(lng, lat);

            // Create a new marker under cursor
            map.markerManager.Create(lng, lat, "Marker " + map.markerManager.Count);

            // Get the coordinate at the desired distance
            double nlng, nlat;
            OnlineMapsUtils.GetCoordinateInDistance(lng, lat, radiusKM, 90, out nlng, out nlat);

            double tx1, ty1, tx2, ty2;

            // Convert the coordinate under cursor to tile position
            map.projection.CoordinatesToTile(lng, lat, 20, out tx1, out ty1);

            // Convert remote coordinate to tile position
            map.projection.CoordinatesToTile(nlng, nlat, 20, out tx2, out ty2);

            // Calculate radius in tiles
            double r = tx2 - tx1;

            // Create a new array for points
            OnlineMapsVector2d[] points = new OnlineMapsVector2d[segments];

            // Calculate a step
            double step = 360d / segments;

            // Calculate each point of circle
            for (int i = 0; i < segments; i++)
            {
                double px = tx1 + Math.Cos(step * i * OnlineMapsUtils.Deg2Rad) * r;
                double py = ty1 + Math.Sin(step * i * OnlineMapsUtils.Deg2Rad) * r;
                map.projection.TileToCoordinates(px, py, 20, out lng, out lat);
                points[i] = new OnlineMapsVector2d(lng, lat);
            }

            // Create a new polygon to draw a circle
            OnlineMapsDrawingPoly poly = new OnlineMapsDrawingPoly(points, Color.red, 3);
            map.drawingElementManager.Add(poly);

            if (items == null) items = new List<BoundingItem>();

            items.Add(new BoundingItem (this)
            {
                points = points,
                center = center
            });
        }
        
        private void Update()
        {
            // If there are no items, we do not need to update them
            if (items == null || items.Count == 0) return;

            // Update items and remove completed
            items.RemoveAll(i => i.Update());

            // Redraw map
            map.Redraw();
        }

        /// <summary>
        /// Class that stores information about the circle
        /// </summary>
        public class BoundingItem
        {
            /// <summary>
            /// Center of the circle
            /// </summary>
            public OnlineMapsVector2d center;

            /// <summary>
            /// Is the animation completed?
            /// </summary>
            public bool finished;

            /// <summary>
            /// Array of points of the circle
            /// </summary>
            public OnlineMapsVector2d[] points;

            /// <summary>
            /// Reference to the BouncingCircle instance
            /// </summary>
            private BouncingCircle instance;

            /// <summary>
            /// Animation progress
            /// </summary>
            private float progress;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="instance">Reference to the BouncingCircle instance</param>
            public BoundingItem(BouncingCircle instance)
            {
                this.instance = instance;
            }

            /// <summary>
            /// Update circle
            /// </summary>
            /// <returns>True - animation is completed, False - otherwise</returns>
            public bool Update()
            {
                // Update animation progress
                progress += Time.deltaTime / instance.duration;
                if (progress >= 1)
                {
                    progress = 1;
                    finished = true;
                }

                // Calculate radius
                float radius = instance.radiusKM * instance.curve.Evaluate(progress);

                // Find the coordinate at the desired distance
                double nlng, nlat;
                OnlineMapsUtils.GetCoordinateInDistance(center.x, center.y, radius, 90, out nlng, out nlat);

                double tx1, ty1, tx2, ty2;

                // Convert the coordinate under cursor to tile position
                instance.map.projection.CoordinatesToTile(center.x, center.y, 20, out tx1, out ty1);

                // Convert remote coordinate to tile position
                instance.map.projection.CoordinatesToTile(nlng, nlat, 20, out tx2, out ty2);

                // Calculate radius in tiles
                double r = tx2 - tx1;

                int segments = points.Length;

                // Calculate a step
                double step = 360d / segments;

                double lng, lat;

                // Calculate each point of circle
                for (int i = 0; i < segments; i++)
                {
                    double px = tx1 + Math.Cos(step * i * OnlineMapsUtils.Deg2Rad) * r;
                    double py = ty1 + Math.Sin(step * i * OnlineMapsUtils.Deg2Rad) * r;
                    instance.map.projection.TileToCoordinates(px, py, 20, out lng, out lat);
                    points[i] = new OnlineMapsVector2d(lng, lat);
                }

                return finished;
            }
        }
    }
}