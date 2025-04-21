using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lovatto.MiniMap
{
    [CreateAssetMenu(fileName = "Map Render", menuName = "MiniMap/Render")]
    public class bl_MapRender : ScriptableObject
    {
        /// <summary>
        /// 
        /// </summary>
        public enum RendersDivisions
        {
            Single = 1,
            _2x2 = 2,
            _3x3 = 3,
            _4x4 = 4,
            _5x5 = 5,
        }

        public RendersDivisions renderDivisions = RendersDivisions.Single;
        public List<Texture2D> snapshots;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Texture2D GetSingle()
        {
            return snapshots[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Texture2D GetSnapshot(int index)
        {
            return snapshots[index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsSingle()
        {
            return renderDivisions == RendersDivisions.Single;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfDivisions()
        {
            return (int)renderDivisions;
        }

        /// <summary>
        /// 
        /// </summary>
        public void DrawOnGUI(Rect fullArea)
        {
#if UNITY_EDITOR
            EditorGUI.DrawRect(fullArea, new Color(1,0,0,0.33f));
            
            if (renderDivisions == RendersDivisions.Single)
            {
                GUI.DrawTexture(fullArea, snapshots[0]);
                return;
            }

            int divisions = (int)renderDivisions;
            Rect area = fullArea;
            float row = Mathf.Ceil(fullArea.width / (float)divisions);
            area.width = area.height = row;
            area.y = fullArea.y + (fullArea.height - row);

            int snapId = 0;
            for (int y = 0; y < divisions; y++)
            {
                for (int x = 0; x < divisions; x++)
                {
                    GUI.DrawTexture(area, snapshots[snapId], ScaleMode.ScaleToFit);
                    area.x += row;
                    snapId++;
                }
                area.y -= row;
                area.x = fullArea.x;
            }
#endif
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(bl_MapRender))]
    public class bl_MapRenderEditor : Editor
    {
        bl_MapRender script;

        private void OnEnable()
        {
            script = (bl_MapRender)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var drawArea = GUILayoutUtility.GetRect(256, 256);
            script.DrawOnGUI(drawArea);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}