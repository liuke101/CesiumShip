/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(OnlineMapsMarker))]
public class OnlineMapsMarkerPropertyDrawer : OnlineMapsMarkerBasePropertyDrawer
{
    protected override int countFields
    {
        get { return OnlineMaps.isPlaying? 9: 8; }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        try
        {
            Rect rect = new Rect(position.x, position.y, position.width, 16);
            if (!DrawHeader(label, rect, property))
            {
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.BeginChangeCheck();
            SerializedProperty pLat = DrawProperty(property, "latitude", ref rect);
            if (EditorGUI.EndChangeCheck())
            {
                if (pLat.doubleValue < -90) pLat.doubleValue = -90;
                else if (pLat.doubleValue > 90) pLat.doubleValue = 90;
            }

            EditorGUI.BeginChangeCheck();
            SerializedProperty pLng = DrawProperty(property, "longitude", ref rect);
            if (EditorGUI.EndChangeCheck())
            {
                if (pLng.doubleValue < -180) pLng.doubleValue += 360;
                else if (pLng.doubleValue > 180) pLng.doubleValue -= 360;
            }

            DrawProperty(property, "range", ref rect, new GUIContent("Zooms"));

            EditorGUI.BeginChangeCheck();
            SerializedProperty pRot = DrawProperty(property, "_rotation", ref rect, new GUIContent("Rotation (0-1)"));
            if (EditorGUI.EndChangeCheck()) if (pRot.floatValue < 0 || pRot.floatValue > 1) pRot.floatValue = Mathf.Repeat(pRot.floatValue, 1);

            DrawProperty(property, "_scale", ref rect);
            DrawProperty(property, "label", ref rect);
            DrawProperty(property, "align", ref rect);

            EditorGUI.BeginChangeCheck();
            SerializedProperty pTexture = DrawProperty(property, "_texture", ref rect);
            if (EditorGUI.EndChangeCheck())
            {
                OnlineMapsEditorUtils.CheckMarkerTextureImporter(pTexture);

                if (EditorApplication.isPlaying)
                {
                    string displayName = property.displayName;
                    string indexStr = displayName.Substring(8);
                    int index;
                    if (int.TryParse(indexStr, out index))
                    {
                        OnlineMapsMarkerManager manager = property.serializedObject.targetObject as OnlineMapsMarkerManager;
                        if (manager != null)
                        {
                            OnlineMapsMarker marker = manager[index];
                            if (marker != null)
                            {
                                marker.texture = pTexture.objectReferenceValue as Texture2D;
                                manager.map.Redraw();
                            }
                        }
                    }
                }
            }

            DrawCenterButton(rect, pLng, pLat);
        }
        catch
        {
        }

        EditorGUI.EndProperty();
    }
}