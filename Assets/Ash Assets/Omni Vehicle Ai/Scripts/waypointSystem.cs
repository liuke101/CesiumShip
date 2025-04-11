using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class waypointSystem : MonoBehaviour
{
    public List<waypoint> waypoints = new List<waypoint>(); // List of all waypoints in the scene

    public bool editMode = false; // Toggle for enabling or disabling edit mode

    [HideInInspector] public float handleSize = 5f; // Adjustable handle size for waypoints (hidden in Inspector)

    public Spline CreateSplineFromPath(List<waypoint> waypointList)
    {
        Spline newSpline = new Spline(); // Create a new spline instance

        // Add points from the shortest path to the spline
        foreach (waypoint wp in waypointList)
        {
            Vector3 position = wp.Position;
            BezierKnot knot = new BezierKnot(position); // Create a new Bezier knot with the waypoint position
            newSpline.Add(knot); // Add the knot to the spline
        }

        // Set tangent mode to AutoSmooth for smooth curves
        for (int i = 0; i < newSpline.Count; i++)
        {
            newSpline.SetTangentMode(i, TangentMode.AutoSmooth); // Ensure smooth tangents
        }

        return newSpline; // Return the created spline
    }


    public waypoint GetNearestWaypoint(Vector3 position)
    {
        waypoint nearestWaypoint = null;
        float nearestDistance = Mathf.Infinity;

        foreach (waypoint waypoint in waypoints)
        {
            float distance = Vector3.Distance(waypoint.Position, position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestWaypoint = waypoint;
            }
        }

        return nearestWaypoint;
    }

    public List<waypoint> GetShortestPath(waypoint start, waypoint goal)
    {
        var openSet = new List<waypoint>();
        var cameFrom = new Dictionary<waypoint, waypoint>();
        var gScore = new Dictionary<waypoint, float>();

        foreach (waypoint waypoint in waypoints)
        {
            gScore[waypoint] = Mathf.Infinity;
        }

        gScore[start] = 0;
        openSet.Add(start);

        while (openSet.Count > 0)
        {
            openSet.Sort((a, b) => gScore[a].CompareTo(gScore[b]));
            waypoint current = openSet[0];

            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);

            foreach (waypoint neighbor in current.connections)
            {
                if (neighbor == null) continue;

                float tentativeGScore = gScore[current] + Vector3.Distance(current.Position, neighbor.Position);

                if (tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }

    private List<waypoint> ReconstructPath(Dictionary<waypoint, waypoint> cameFrom, waypoint current)
    {
        var totalPath = new List<waypoint> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }

        return totalPath;
    }

    public void AlignWaypointsToGround()
    {
        foreach (waypoint wp in waypoints)
        {
            Ray ray = new Ray(wp.Position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f))
            {
                wp.transform.position = hit.point;
            }
        }
    }

    #region Utility Functions

    public waypoint GetWaypointAtPosition(Vector3 position)
    {
        foreach (waypoint wp in waypoints)
        {
            if (Vector3.Distance(wp.transform.position, position) < handleSize)
            {
                return wp;
            }
        }
        return null;
    }

    public void RenameWaypoints()
    {
        for (int i = 0; i < waypoints.Count; i++)
        {
            waypoints[i].name = "Waypoint " + i;
        }
    }

    #endregion


}
