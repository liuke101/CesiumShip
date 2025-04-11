using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace OmniVehicleAi
{
    public class PathFinding : MonoBehaviour
    {
        public waypointSystem waypointSystem;
        public AIVehicleController AIVehicleController;
        public PathProgressTracker pathProgressTracker;

        public List<waypoint> waypoints;

        public void FindPath(Vector3 destination)
        {
            waypoint startWaypoint = waypointSystem.GetNearestWaypoint(AIVehicleController.vehicleTransform.position);
            waypoint endWaypoint = waypointSystem.GetNearestWaypoint(destination);

            List<waypoint> shortestPath = waypointSystem.GetShortestPath(startWaypoint, endWaypoint); //shortest path list of waypoints
            waypoints = shortestPath;

            Spline spline = waypointSystem.CreateSplineFromPath(shortestPath); //create spline from list of waypoints

            SetPath(spline);
        }


        private void SetPath(Spline spline)
        {
            if (pathProgressTracker.splineContainer == null)
            {
                Debug.LogError("SplineContainer not assigned.");
                return;
            }

            // Clear the current spline points
            Spline pathSpline =  pathProgressTracker.splineContainer.Spline;
            pathSpline.Clear();

            Vector3 pathSplineOffset = pathProgressTracker.splineContainer.transform.position;

            // Manually add knots from the provided spline
            for (int i = 0; i < spline.Count; i++)
            {
                BezierKnot knot = spline[i] - pathSplineOffset; // Get each knot from the provided spline
                pathSpline.Add(knot); // Add each knot to the circuitProgressTracker spline container
            }

            // Optionally, set the tangent mode again for smooth curves
            for (int i = 0; i < pathSpline.Count; i++)
            {
                pathSpline.SetTangentMode(i, TangentMode.AutoSmooth); // Ensure smooth tangents
            }
        }


        private void OnDrawGizmosSelected()
        {
            if (waypoints != null)
            {
                Gizmos.color = Color.green;
                foreach (waypoint waypoint in waypoints)
                {
                    Gizmos.DrawSphere(waypoint.Position, 1f);
                }
            }
        }

    }
}
