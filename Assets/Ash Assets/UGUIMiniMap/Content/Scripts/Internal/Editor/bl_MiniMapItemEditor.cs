using Lovatto.MiniMap;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

[CustomEditor(typeof(bl_MiniMapEntity))]
public class bl_MiniMapEntityEditor : Editor
{

    SerializedProperty iconProp;
    AnimBool iconAnim;

    SerializedProperty circleIconProp;
    AnimBool circleIconAnim;

    SerializedProperty settingsProp;
    AnimBool settingsAnim;

    SerializedProperty onClickProp;
    AnimBool onClickAnim;

    private Dictionary<ScriptableObject, Editor> cachedEditors = new Dictionary<ScriptableObject, Editor>();
    bl_MiniMapEntity script;

    private void OnEnable()
    {
        script = (bl_MiniMapEntity)target;
        iconProp = serializedObject.FindProperty("iconData");
        InitAnim(ref iconAnim, iconProp);

        circleIconProp = serializedObject.FindProperty("ShowCircleArea");
        InitAnim(ref circleIconAnim, circleIconProp);

        settingsProp = serializedObject.FindProperty("OffScreen");
        InitAnim(ref settingsAnim, settingsProp);

        onClickProp = serializedObject.FindProperty("isInteractable");
        InitAnim(ref onClickAnim, onClickProp);
    }

    public override void OnInspectorGUI()
    {
        bool allowSceneObjects = !EditorUtility.IsPersistent(target);
        EditorGUI.BeginChangeCheck();

        GUILayout.BeginVertical("box");
        script.Target = EditorGUILayout.ObjectField("Target", script.Target, typeof(Transform), allowSceneObjects) as Transform;
        script.OffSet = EditorGUILayout.Vector3Field("Position Offset", script.OffSet);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        if (GUILayout.Button("Icon Settings", EditorStyles.toolbarPopup)) { iconProp.isExpanded = !iconProp.isExpanded; iconAnim.target = iconProp.isExpanded; }

        if (EditorGUILayout.BeginFadeGroup(iconAnim.faded))
        {
            GUILayout.Space(4);
            if (script.iconData == null)
            {
                EditorGUILayout.BeginHorizontal();
                script.iconData = EditorGUILayout.ObjectField("Icon Preset", script.iconData, typeof(bl_MiniMapIconData), false) as bl_MiniMapIconData;
                if (GUILayout.Button("Create New", GUILayout.Width(80)))
                {
                    string path = AssetDatabase.GetAssetPath(bl_MiniMapData.Instance);
                    path = Path.GetDirectoryName(path);
                    path = $"{Directory.GetParent(path).ToString()}\\Content\\Prefabs\\Presets\\";

                    if (!Directory.Exists(path))
                    {
                        path = EditorUtility.OpenFolderPanel("Save Folder", "Assets", "Assets");
                    }

                    if (string.IsNullOrEmpty(path)) return;

                    path += $"Icon Data.asset";
                    path = AssetDatabase.GenerateUniqueAssetPath(path);

                    var instance = ScriptableObject.CreateInstance<bl_MiniMapIconData>();
                    AssetDatabase.CreateAsset(instance, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    var prefab = AssetDatabase.LoadAssetAtPath(path, typeof(bl_MiniMapIconData)) as bl_MiniMapIconData;
                    script.iconData = prefab;
                    EditorUtility.SetDirty(target);
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                script.iconData = EditorGUILayout.ObjectField("Icon Preset", script.iconData, typeof(bl_MiniMapIconData), false) as bl_MiniMapIconData;
                DrawEditorOf(script.iconData);
            }
            script.RenderDelay = EditorGUILayout.Slider("Start render delay", script.RenderDelay, 0, 3);
        }
        EditorGUILayout.EndFadeGroup();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        if (GUILayout.Button("Circle Area Settings", EditorStyles.toolbarPopup)) { circleIconProp.isExpanded = !circleIconProp.isExpanded; circleIconAnim.target = circleIconProp.isExpanded; }
        if (EditorGUILayout.BeginFadeGroup(circleIconAnim.faded))
        {
            script.ShowCircleArea = EditorGUILayout.ToggleLeft("Show Circle Area", script.ShowCircleArea, EditorStyles.toolbarButton);
            if (script.ShowCircleArea)
            {
                script.CircleAreaRadius = EditorGUILayout.Slider("Radius", script.CircleAreaRadius, 1, 100);
                script.CircleAreaColor = EditorGUILayout.ColorField("Circle Color", script.CircleAreaColor);
            }
        }
        EditorGUILayout.EndFadeGroup();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        if (GUILayout.Button("Settings", EditorStyles.toolbarPopup)) { settingsProp.isExpanded = !settingsProp.isExpanded; settingsAnim.target = settingsProp.isExpanded; }

        if (EditorGUILayout.BeginFadeGroup(settingsAnim.faded))
        {
            script.enableDisableWithTarget = EditorGUILayout.ToggleLeft("Enable/Disable with Target", script.enableDisableWithTarget, EditorStyles.toolbarButton);
            script.DestroyWithObject = EditorGUILayout.ToggleLeft("Destroy with Object", script.DestroyWithObject, EditorStyles.toolbarButton);
            GUILayout.Space(2);
            script.OffScreen = EditorGUILayout.ToggleLeft("Show icon off screen", script.OffScreen, EditorStyles.toolbarButton);
            if (script.OffScreen)
            {
                script.BorderOffScreen = EditorGUILayout.Slider("Border Margin", script.BorderOffScreen, 0, 5);
            }
            GUILayout.Space(5);
            script.opacityBasedDistance = EditorGUILayout.ToggleLeft("Opacity Based On Distance", script.opacityBasedDistance, EditorStyles.toolbarButton);
            if (script.opacityBasedDistance)
            {
                script.maxDistance = EditorGUILayout.FloatField("Max Render Distance", script.maxDistance);
            }
            GUILayout.Space(2);
            GUILayout.Label("Text");
            script.InfoItem = EditorGUILayout.TextArea(script.InfoItem, GUILayout.Height(35));
            script.iconFaceDirection = (bl_MiniMapEntity.IconFaceDirection)EditorGUILayout.EnumPopup("Icon Face Direction", script.iconFaceDirection);

            script.m_Effect = (ItemEffect)EditorGUILayout.EnumPopup("Loop Effect", script.m_Effect);
            script.CustomIconPrefab = EditorGUILayout.ObjectField("Override Global Icon Prefab", script.CustomIconPrefab, typeof(GameObject), false) as GameObject;
        }
        EditorGUILayout.EndFadeGroup();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        if (GUILayout.Button("On Click", EditorStyles.toolbarPopup)) { onClickProp.isExpanded = !onClickProp.isExpanded; onClickAnim.target = onClickProp.isExpanded; }

        if (EditorGUILayout.BeginFadeGroup(onClickAnim.faded))
        {
            script.isInteractable = EditorGUILayout.ToggleLeft("Is Interactable", script.isInteractable, EditorStyles.toolbarButton);
            GUILayout.Space(2);
            if (script.isInteractable)
            {
                GUILayout.Space(2);
                script.interacableAction = (bl_MiniMapEntityBase.InteracableAction)EditorGUILayout.EnumPopup("Interactable Trigger", script.interacableAction);
                GUILayout.Space(2);
                script.keepTextAlwaysFacingUp = EditorGUILayout.ToggleLeft("Keep Text Always Facing Up", script.keepTextAlwaysFacingUp, EditorStyles.toolbarButton);
                GUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onClick"), true);
            }
        }
        EditorGUILayout.EndFadeGroup();
        GUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(script);
        }
    }

    void DrawEditorOf(ScriptableObject scriptable)
    {
        if (scriptable == null) return;

        Editor editor;
        if (cachedEditors.ContainsKey(scriptable))
        {
            editor = cachedEditors[scriptable];
        }
        else
        {
            editor = Editor.CreateEditor(scriptable);
            cachedEditors.Add(scriptable, editor);
        }

        if (editor == null) return;

        Rect r = EditorGUILayout.BeginVertical();
        EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.02f));
        EditorGUI.BeginChangeCheck();
        editor.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(editor);
        }
        EditorGUILayout.EndVertical();
    }

    private void InitAnim(ref AnimBool anim, SerializedProperty prop)
    {
        anim = new AnimBool(prop.isExpanded);
        anim.valueChanged.AddListener(Repaint);
    }
}