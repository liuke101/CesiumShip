/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public abstract class OnlineMapsMarkerManagerBaseEditor<T, U> : Editor 
    where T: OnlineMapsMarkerManagerBase<T, U>
    where U: OnlineMapsMarkerBase
{
    const int ITEMS_PER_PAGE = 10;
    
    protected int countItems;
    protected int[] displayItems;
    protected string filter;
    protected bool isDirty;
    protected SerializedProperty items;
    protected T manager;
    protected OnlineMaps map;
    protected int page = 1;

    protected virtual void AddMarker()
    {
        filter = null;
        serializedObject.Update();
        page = Mathf.CeilToInt(items.arraySize / (float)ITEMS_PER_PAGE);
        UseAllItems();
    }

    protected virtual void DrawItem(int i, ref int removedIndex)
    {
        if (i < 0 || i >= items.arraySize) return;
        
        EditorGUILayout.BeginVertical(GUI.skin.box);

        OnlineMapsMarkerBasePropertyDrawer.isRemoved = false;
        OnlineMapsMarkerBasePropertyDrawer.isEnabledChanged = null;
        EditorGUILayout.PropertyField(items.GetArrayElementAtIndex(i), new GUIContent("Marker " + (i + 1)));
        if (OnlineMapsMarkerBasePropertyDrawer.isRemoved) removedIndex = i;
        if (OnlineMapsMarkerBasePropertyDrawer.isEnabledChanged.HasValue) manager[i].enabled = OnlineMapsMarkerBasePropertyDrawer.isEnabledChanged.Value;

        EditorGUILayout.EndVertical();
    }

    private void DrawItemHeader(bool usePagination, int start, int end, int countPages)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (!usePagination)
        {
            string headerLabel = "Total: " + displayItems.Length;
            GUILayout.Label(headerLabel);
        }
        else
        {
            string headerLabel = "Total: " + displayItems.Length;
            headerLabel += ", Displayed: " + (start + 1) + "-" + end;
            GUILayout.Label(headerLabel);

            if (GUILayout.Button("<", GUILayout.ExpandWidth(false)))
            {
                if (--page < 1) page = countPages;
            }

            GUIContent pageLabel = new GUIContent(page + "/" + countPages);
            GUIStyle pageStyle = EditorStyles.toolbarTextField;
            Vector2 size = pageStyle.CalcSize(pageLabel);
            EditorGUI.BeginChangeCheck();
            page = EditorGUILayout.IntField(page, pageStyle, GUILayout.Width(size.x + 10));
            if (EditorGUI.EndChangeCheck())
            {
                if (page < 1) page = 1;
                else if (page > countPages) page = countPages;
            }

            if (GUILayout.Button(">", GUILayout.ExpandWidth(false)))
            {
                if (++page > countPages) page = 1;
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawItems()
    {
        if (countItems != items.arraySize) UpdateFilteredItems();
        
        int removedIndex = -1;

        bool usePagination = displayItems.Length > ITEMS_PER_PAGE;
        int start = 0;
        int end = displayItems.Length;
        int countPages = Mathf.CeilToInt(displayItems.Length / (float)ITEMS_PER_PAGE);
        if (usePagination)
        {
            start = (page - 1) * ITEMS_PER_PAGE;
            end = Mathf.Min(start + ITEMS_PER_PAGE, displayItems.Length);
        }

        DrawItemHeader(usePagination, start, end, countPages);

        EditorGUI.BeginChangeCheck();
        for (int i = start; i < end; i++)
        {
            try
            {
                int index = displayItems[i];
                DrawItem(index, ref removedIndex);
            }
            catch
            {
            }
        }

        if (EditorGUI.EndChangeCheck()) isDirty = true;

        if (removedIndex != -1)
        {
            manager.RemoveAt(removedIndex);
            isDirty = true;
            serializedObject.Update();
            UpdateFilteredItems();
            countPages = Mathf.CeilToInt(displayItems.Length / (float)ITEMS_PER_PAGE);
            if (page > countPages) page = countPages;
        }

        EditorGUILayout.Space();
    }

    protected virtual void DrawSettings()
    {
    }

    private void OnEnable()
    {
        manager = target as T;
        map = manager.GetComponent<OnlineMaps>();
        items = serializedObject.FindProperty("_items");
        serializedObject.Update();
        page = 1;
        filter = null;
        countItems = items.arraySize;
        UseAllItems();
        OnEnableLate();

        serializedObject.ApplyModifiedProperties();
    }

    protected virtual void OnEnableLate()
    {
    }

    public override void OnInspectorGUI()
    {
        isDirty = false;
        
        DrawSettings();

        EditorGUI.BeginChangeCheck();
        filter = EditorGUILayout.TextField("Filter By Label", filter);
        if (EditorGUI.EndChangeCheck())
        {
            page = 1;
            UpdateFilteredItems();
        }

        DrawItems();

        if (GUILayout.Button("Add Marker"))
        {
            AddMarker();
            isDirty = true;
        }

        serializedObject.ApplyModifiedProperties();

        if (isDirty)
        {
            EditorUtility.SetDirty(target);
            if (!OnlineMaps.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
            else map.Redraw();
        }
    }

    private void UpdateFilteredItems()
    {
        if (string.IsNullOrEmpty(filter))
        {
            UseAllItems();
            return;
        }
        
        string f = filter.ToLowerInvariant();
        List<int> temp = new List<int>();

        for (int i = 0; i < items.arraySize; i++)
        {
            SerializedProperty sp = items.GetArrayElementAtIndex(i);
            SerializedProperty labelProp = sp.FindPropertyRelative("label");
            if (labelProp.stringValue == null || !labelProp.stringValue.ToLowerInvariant().Contains(f)) continue;
            temp.Add(i);
        }
        
        displayItems = temp.ToArray();
        int countPages = Mathf.CeilToInt(displayItems.Length / (float)ITEMS_PER_PAGE);
        if (page > countPages) page = countPages;
    }

    protected void UseAllItems()
    {
        displayItems = new int[items.arraySize];
        for (int i = 0; i < items.arraySize; i++)
        {
            displayItems[i] = i;
        }
    }
}