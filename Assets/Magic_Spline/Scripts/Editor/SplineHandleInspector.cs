using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(SplineHandle))]
class SplineHandleInspector : Editor
{
    public override void OnInspectorGUI()
    {
        SplineHandle handle = (SplineHandle)target;

        if (handle.Opposite != null && GUILayout.Button("Snap to axis"))
        {
            handle.SnapOppositeToAxis();
        }

        if (GUI.changed) { EditorUtility.SetDirty(handle); }
    }
}
