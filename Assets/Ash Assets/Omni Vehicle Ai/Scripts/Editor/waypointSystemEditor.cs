using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(waypointSystem))]
public class waypointSystemEditor : Editor
{
    waypointSystem systemTarget;
    waypoint selectedWaypoint;
    RaycastHit hit;

    private void OnEnable()
    {
        systemTarget = (waypointSystem)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Waypoint Editing", EditorStyles.boldLabel);

        // editing tutorial
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Instructions", EditorStyles.boldLabel);

        GUIStyle instructionStyle = new GUIStyle(EditorStyles.label);
        instructionStyle.wordWrap = true; // Ensures wrapping

        EditorGUILayout.LabelField(
            "1. Hold 'Left Shift' + Left Click to create a new waypoint.\n" +
            "2. Hold 'Left Shift' + Left Click on an existing waypoint to create or remove connections.\n" +
            "3. Use the move handles to adjust the position of the selected waypoint.\n" + 
            "4. 'CTRL' + 'Z' to undo any action",
            instructionStyle
        );

        GUILayout.EndVertical();


        // Adjustable handle size
        EditorGUI.BeginChangeCheck();
        float newHandleSize = EditorGUILayout.FloatField("Waypoint Handle Size", systemTarget.handleSize);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(systemTarget, "Change Handle Size");
            systemTarget.handleSize = newHandleSize;
        }

        if (GUILayout.Button("Align Waypoints To Ground", GUILayout.Height(30)))
        {
            Undo.RecordObject(systemTarget, "Align Waypoints To Ground");
            foreach (var wp in systemTarget.waypoints)
            {
                Undo.RecordObject(wp.transform, "Align Waypoint");
            }
            systemTarget.AlignWaypointsToGround();
        }

        if (GUILayout.Button("Rename Waypoints", GUILayout.Height(30)))
        {
            Undo.RecordObject(systemTarget, "Rename Waypoints");
            systemTarget.RenameWaypoints();
        }

        GUILayout.EndVertical();

        // Ensure the handles update in the Scene View when the handle size changes
        SceneView.RepaintAll();
    }

    private void OnSceneGUI()
    {
        Event e = Event.current;

        // Block selection of other objects when edit mode is enabled
        if (systemTarget.editMode)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            // Handle LeftShift + LeftClick to place a waypoint or create/remove connections
            if (e.shift && e.type == EventType.MouseDown && e.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out hit, 1000))
                {
                    waypoint clickedWaypoint = GetWaypointAtPosition(hit.point);
                    if (clickedWaypoint == null)
                    {
                        // Shift click on empty space: create a new waypoint
                        PlaceWaypointAt(hit.point);
                    }
                    else if (clickedWaypoint != selectedWaypoint)
                    {
                        // Shift click on another waypoint: create or remove a connection
                        CreateOrRemoveConnectionWithSelected(clickedWaypoint);
                    }
                    e.Use();
                }
            }

            // Draw handles for waypoint selection and show move handle for selected waypoint
            DrawWaypointHandles();
            DrawMoveHandleForSelectedWaypoint();
        }
    }

    private void PlaceWaypointAt(Vector3 position)
    {
        // Register the creation of the new GameObject for Undo
        GameObject newWaypoint = new GameObject("Waypoint");
        Undo.RegisterCreatedObjectUndo(newWaypoint, "Create Waypoint");

        newWaypoint.transform.position = position;
        newWaypoint.transform.parent = systemTarget.transform;

        // Record the addition of the new waypoint component
        waypoint wp = Undo.AddComponent<waypoint>(newWaypoint);
        wp.connections = new List<waypoint>(); // Ensure connections list is initialized

        // Record changes to the waypoints list
        Undo.RecordObject(systemTarget, "Add Waypoint");
        systemTarget.waypoints.Add(wp);

        // Connect the new waypoint to the currently selected waypoint
        if (selectedWaypoint != null)
        {
            Undo.RecordObject(selectedWaypoint, "Connect Waypoints");
            Undo.RecordObject(wp, "Connect Waypoints");
            selectedWaypoint.connections.Add(wp);
            wp.connections.Add(selectedWaypoint);
        }

        // Set the newly placed waypoint as the current one
        selectedWaypoint = wp;

        // Automatically rename all waypoints after adding a new one
        Undo.RecordObject(systemTarget, "Rename Waypoints");
        systemTarget.RenameWaypoints();
    }

    // Create or remove a connection between the selected waypoint and another waypoint
    private void CreateOrRemoveConnectionWithSelected(waypoint clickedWaypoint)
    {
        if (selectedWaypoint != null && clickedWaypoint != selectedWaypoint)
        {
            Undo.RecordObject(selectedWaypoint, "Modify Connections");
            Undo.RecordObject(clickedWaypoint, "Modify Connections");

            if (selectedWaypoint.connections.Contains(clickedWaypoint))
            {
                // Remove the bidirectional connection
                selectedWaypoint.connections.Remove(clickedWaypoint);
                clickedWaypoint.connections.Remove(selectedWaypoint);
            }
            else
            {
                // Add a bidirectional connection
                selectedWaypoint.connections.Add(clickedWaypoint);
                clickedWaypoint.connections.Add(selectedWaypoint);
            }
        }
    }

    // Get the waypoint at the clicked position
    private waypoint GetWaypointAtPosition(Vector3 position)
    {
        foreach (waypoint wp in systemTarget.waypoints)
        {
            if (Vector3.Distance(wp.transform.position, position) < systemTarget.handleSize) // Small tolerance to ensure selection
            {
                return wp;
            }
        }
        return null;
    }

    // This function draws handles to select waypoints
    private void DrawWaypointHandles()
    {
        for (int i = 0; i < systemTarget.waypoints.Count; i++)
        {
            waypoint wp = systemTarget.waypoints[i];

            // Reset the color to white before drawing each handle
            Handles.color = Color.white;

            // Create a handle button for each waypoint
            if (Handles.Button(wp.transform.position, Quaternion.identity, systemTarget.handleSize, systemTarget.handleSize, Handles.SphereHandleCap))
            {
                // Set this waypoint as the selected waypoint
                selectedWaypoint = wp;
            }

            // Highlight the currently selected waypoint
            if (wp == selectedWaypoint)
            {
                // Set color to green for the selected waypoint
                Handles.color = Color.green;

                // Draw a wire disc around the selected waypoint
                Handles.DrawWireDisc(wp.transform.position, Vector3.up, systemTarget.handleSize + 0.2f);
            }

            // Reset the color back to white after drawing
            Handles.color = Color.white;
        }

        // Ensure a scene repaint to update the handle highlights
        HandleUtility.Repaint();
    }


    // Draw a move handle for the selected waypoint
    private void DrawMoveHandleForSelectedWaypoint()
    {
        if (selectedWaypoint != null)
        {
            EditorGUI.BeginChangeCheck();

            // Show the move handle (PositionHandle) for the selected waypoint
            Vector3 newPosition = Handles.PositionHandle(selectedWaypoint.transform.position, Quaternion.identity);

            // If the user moved the waypoint, update its position
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(selectedWaypoint.transform, "Move Waypoint");
                selectedWaypoint.transform.position = newPosition;
            }
        }
    }
}
