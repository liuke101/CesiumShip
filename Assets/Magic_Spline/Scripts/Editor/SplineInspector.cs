using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MagicSpline))]
public class SplineInspector : Editor
{
    public override void OnInspectorGUI()
    {
        MagicSpline spline = (MagicSpline) target;

        spline.DraggablePrefab = (GameObject)EditorGUILayout.ObjectField(nameof(spline.DraggablePrefab), spline.DraggablePrefab, typeof(GameObject), true);
        spline.NewNodeOffset = EditorGUILayout.FloatField(nameof(spline.NewNodeOffset), spline.NewNodeOffset);
        spline.MoveSpeed = EditorGUILayout.FloatField(nameof(spline.MoveSpeed), spline.MoveSpeed);
        spline.SpawnStartDelay = EditorGUILayout.FloatField(nameof(spline.SpawnStartDelay), spline.SpawnStartDelay);
        spline.SpawnDelay = EditorGUILayout.FloatField(nameof(spline.SpawnDelay), spline.SpawnDelay);
        spline.SpawnCount = EditorGUILayout.IntField(nameof(spline.SpawnCount), spline.SpawnCount);
        spline.DraggableSplineEndAction = (DraggableSplineEndAction)EditorGUILayout.EnumPopup(nameof(spline.DraggableSplineEndAction), spline.DraggableSplineEndAction);
        spline.PreWarmTime = EditorGUILayout.FloatField(nameof(spline.PreWarmTime), spline.PreWarmTime);

        spline.SpawnDelay = Mathf.Max(spline.SpawnDelay, 0.1f);

        if (GUILayout.Button("Add point"))
        {
            spline.AddNode();
        }

        if (spline.Segments.Count > 0 && GUILayout.Button("Close"))
        {
            spline.Close();
        }

        if (GUI.changed) { EditorUtility.SetDirty(spline); }
    }
}
