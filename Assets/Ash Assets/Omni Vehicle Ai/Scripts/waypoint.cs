using System.Collections.Generic;
using UnityEngine;

public class waypoint : MonoBehaviour
{
    // Array of connections (waypoints that this waypoint is connected to)
    public List <waypoint> connections;

    public Vector3 Position => transform.position;

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(Position, 0.1f);

        // Draw lines to connected waypoints
        if (connections != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var connection in connections)
            {
                if (connection != null)
                {
                    Gizmos.DrawLine(Position, connection.Position);
                }
            }
        }
    }
}
