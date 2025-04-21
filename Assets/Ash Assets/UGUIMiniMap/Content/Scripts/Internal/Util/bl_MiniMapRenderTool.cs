using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lovatto.MiniMap
{
    [RequireComponent(typeof(Camera)), ExecuteInEditMode]
    public class bl_MiniMapRenderTool : MonoBehaviour
    {
        /// <summary>
        /// The applied MSAA, possible values are 1,2,4 nad 8
        /// </summary>
        public int msaa = 1;
        public bl_MapRender.RendersDivisions renderDivisions = bl_MapRender.RendersDivisions.Single;
        public string[] Resolutions = new string[] { "4096", "2048", "1024", "512", "256" };
        public int CurrentResolution = 1;
        public bool backgroundTransparent = true;
        public bool previewBorders = true;
        public bool blackBorders = true;
        public bl_MiniMap miniMap;

        /// <summary>
        /// 
        /// </summary>
        private void OnGUI()
        {
            if (previewBorders)
            {
                if (blackBorders) GUI.color = Color.black;
                Rect ScreenShotRect = new Rect(0, 0, Screen.height, Screen.height);
                Vector2 Center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                Vector2 HalfSize = new Vector2(ScreenShotRect.height * 0.5f, ScreenShotRect.height * 0.5f);

                GUI.DrawTexture(new Rect(Center.x - HalfSize.x, Center.y - HalfSize.y, 2, ScreenShotRect.height), Texture2D.whiteTexture, ScaleMode.StretchToFill);
                GUI.DrawTexture(new Rect(Center.x + HalfSize.x, Center.y - HalfSize.y, 2, ScreenShotRect.height), Texture2D.whiteTexture, ScaleMode.StretchToFill);
                GUI.DrawTexture(new Rect(Center.x - HalfSize.x, (Center.y - HalfSize.y), ScreenShotRect.width, 2), Texture2D.whiteTexture, ScaleMode.StretchToFill);
                GUI.DrawTexture(new Rect(Center.x - HalfSize.x, (Center.y + HalfSize.y), ScreenShotRect.width + 2, 2), Texture2D.whiteTexture, ScaleMode.StretchToFill);

                GUI.color = blackBorders ? new Color(0, 0, 0, 0.3f) : new Color(1, 1, 1, 0.3f);
                GUI.DrawTexture(new Rect(Center.x - 1, Center.y - HalfSize.y, 2, ScreenShotRect.height), Texture2D.whiteTexture, ScaleMode.StretchToFill);
                GUI.DrawTexture(new Rect(Center.x - HalfSize.x, Center.y - 1, ScreenShotRect.width, 2), Texture2D.whiteTexture, ScaleMode.StretchToFill);

                GUI.color = Color.white;
            }
        }

        public void SetMiniMap(bl_MiniMap mm)
        {
            miniMap = mm;
            CenterBounds();
        }

        public void CenterBounds()
        {
            Vector3 v = transform.position;
            v.x = miniMap.mapBounds.position.x;
            v.z = miniMap.mapBounds.position.z;
            transform.position = v;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(bl_MiniMapRenderTool))]
    public class bl_MiniMapScreenShotEditor : Editor
    {
        bl_MiniMapRenderTool script;
        Camera m_Camera;
        public bl_MiniMapBounds mapBounds;
        public Color gizmoColor = new Color(0, 1, 0, 0.25f);
        public bool init = false;


        private void OnEnable()
        {
            script = (bl_MiniMapRenderTool)target;
            m_Camera = script.GetComponent<Camera>();

            if (!init)
            {
                if (mapBounds == null && FindAnyObjectByType<bl_MiniMapBounds>() != null)
                {
                    mapBounds = FindAnyObjectByType<bl_MiniMapBounds>();
                }

                if (mapBounds != null)
                {
                    m_Camera.orthographicSize = mapBounds.BoundTransform.sizeDelta.x * 0.5f;
                    gizmoColor = mapBounds.GizmoColor;
                    gizmoColor.a = 0.25f;
                }
                script.CenterBounds();
                init = true;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("Settings", "box");
            script.CurrentResolution = EditorGUILayout.Popup("Resolution", script.CurrentResolution, script.Resolutions);
            script.renderDivisions = (bl_MapRender.RendersDivisions)EditorGUILayout.EnumPopup("Render Divisions", script.renderDivisions);
            script.msaa = EditorGUILayout.IntSlider("MSAA", script.msaa, 1, 4);
            m_Camera.orthographicSize = EditorGUILayout.Slider("Size", m_Camera.orthographicSize, 1f, 1000f);
            script.backgroundTransparent = EditorGUILayout.ToggleLeft("Transparent Background", script.backgroundTransparent);
            script.previewBorders = EditorGUILayout.ToggleLeft("Preview Borders", script.previewBorders);
            if (script.previewBorders)
            {
                script.blackBorders = EditorGUILayout.ToggleLeft("Black Borders", script.blackBorders);
            }
            if (mapBounds == null)
            {
                EditorGUILayout.HelpBox("The Minimap bounds has not been assigned!\nMake sure you have a minimap prefab in the scene and active it while take the render.", MessageType.Warning);
            }
            mapBounds = EditorGUILayout.ObjectField("Map Bounds", mapBounds, typeof(bl_MiniMapBounds), true) as bl_MiniMapBounds;
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            GUI.enabled = mapBounds != null;
            if (GUILayout.Button("Fit Size to Bounds"))
            {
                m_Camera.orthographicSize = mapBounds.BoundTransform.sizeDelta.x * 0.5f;
            }
            GUI.enabled = true;

            if (GUILayout.Button("Center Bounds"))
            {
                script.CenterBounds();
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Render Map", GUILayout.Height(40)))
            {
                TakeSnapshot();
            }
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }

        /// <summary>
        /// Takes a map snapshot and saves it
        /// </summary>
        public void TakeSnapshot()
        {
            Vector2 res = GetResolution();
            int w = Mathf.FloorToInt(res.x);
            int h = Mathf.FloorToInt(res.y);

            string path = EditorUtility.SaveFolderPanel("Save Screen Shot", "Assets/UGUIMiniMap/Content/Art/SnapShots/", "");
            if (string.IsNullOrEmpty(path)) return;

            var renderContainer = ScriptableObject.CreateInstance<bl_MapRender>();
            renderContainer.renderDivisions = script.renderDivisions;
            renderContainer.snapshots = new List<Texture2D>();

            if (script.renderDivisions == bl_MapRender.RendersDivisions.Single)
            {
                var snapshot = Render(w, h);
                string savePath = SaveImage(path, "", snapshot);
                AssetDatabase.Refresh();
                string relativepath = "Assets" + savePath.Substring(Application.dataPath.Length);
                SetupTextureAsset(relativepath);

                Texture2D textAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(relativepath);
                renderContainer.snapshots.Add(textAsset);

                string containerPath2 = $"Assets{path.Substring(Application.dataPath.Length)}/{SceneManager.GetActiveScene().name}_Render_{script.renderDivisions.ToString().Replace("_", "")}.asset";
                var container2 = SaveRenderContainer(renderContainer, containerPath2);
                EditorGUIUtility.PingObject(container2);

                return;
            }

            Transform transform = script.transform;
            int divisions = (int)script.renderDivisions;

            var renders = new Texture2D[divisions * divisions];

            Vector3 defaultPosition = transform.position;
            float defaultSize = m_Camera.orthographicSize;

            float divSize = defaultSize / divisions;
            float halfDiv = (float)divisions / 2f;

            Vector3 renderPos = defaultPosition;
            renderPos.x -= ((divSize * 2) * halfDiv) - divSize;
            renderPos.z -= ((divSize * 2) * halfDiv) - divSize;
            float initX = renderPos.x;

            transform.position = renderPos;
            m_Camera.orthographicSize = divSize;

            int renderId = 0;
            for (int i = 0; i < divisions; i++)
            {
                for (int e = 0; e < divisions; e++)
                {
                    renders[renderId] = Render(w, h);
                    // Debug.Log($"render {renderId} pos: {renderPos}, render size: {renderSize} row size: {rowSize}");

                    renderPos.x += divSize * 2;
                    transform.position = renderPos;
                    renderId++;
                }
                renderPos.x = initX;
                renderPos.z += divSize * 2;
                transform.position = renderPos;
            }

            transform.position = defaultPosition;
            m_Camera.orthographicSize = defaultSize;

            // save shots
            renderId = 0;
            string[] paths = new string[renders.Length];
            foreach (var render in renders)
            {
                if (render == null) continue;

                string endPoint = $"_({renderId})";
                paths[renderId] = SaveImage(path, $"{SnapshotName(w, h, endPoint)}", render, false);
                paths[renderId] = "Assets" + paths[renderId].Substring(Application.dataPath.Length);
                renderId++;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            foreach (var p in paths)
            {
                Texture2D t = AssetDatabase.LoadAssetAtPath(p, typeof(Texture2D)) as Texture2D;
                renderContainer.snapshots.Add(t);

                SetupTextureAsset(p);
            }

            string containerPath = $"Assets{path.Substring(Application.dataPath.Length)}/{SceneManager.GetActiveScene().name}_Render_{script.renderDivisions.ToString().Replace("_", "")}.asset";
            var container = SaveRenderContainer(renderContainer, containerPath);
            EditorGUIUtility.PingObject(container);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        private void SetupTextureAsset(string path)
        {
            TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(path);
            if (ti != null)
            {
                ti.alphaIsTransparency = true;
                ti.wrapMode = TextureWrapMode.Clamp;
                ti.filterMode = FilterMode.Point;
                ti.mipmapEnabled = false;//performance
                ti.isReadable = false;//performance
                EditorUtility.SetDirty(ti);
                ti.SaveAndReimport();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Texture2D Render(int width, int height)
        {
            var camera = m_Camera;
            //setup rendertexture
            RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = script.msaa;
            rt.filterMode = FilterMode.Bilinear;
            camera.targetTexture = rt;

            var format = script.backgroundTransparent ? TextureFormat.ARGB32 : TextureFormat.RGB24;
            //render the texture
            Texture2D snapshot = new Texture2D(width, height, format, false);
            camera.Render();
            RenderTexture.active = rt;
            snapshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null;
            snapshot.alphaIsTransparency = true;
            // only require to preview
            snapshot.Apply();

            DestroyImmediate(rt);
            return snapshot;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        /// <param name="image"></param>
        /// <param name="cleanAfter"></param>
        /// <returns></returns>
        private string SaveImage(string folderPath, string fileName, Texture2D image, bool cleanAfter = true)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = SnapshotName(image.width, image.height);
            }

            string savePath = Path.Combine(folderPath, fileName);
            // savePath = AssetDatabase.GenerateUniqueAssetPath(savePath);
            byte[] buffer = image.EncodeToPNG();
            File.WriteAllBytes(savePath, buffer);

            if (cleanAfter) DestroyImmediate(image);

            return savePath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private bl_MapRender SaveRenderContainer(bl_MapRender instance, string path)
        {
            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var container = AssetDatabase.LoadAssetAtPath<bl_MapRender>(path);
            return container;
        }

        private void OnSceneGUI()
        {
            Transform transform = script.transform;

            Vector3 heightPosition = transform.position;
            float size = m_Camera.orthographicSize;

            Handles.color = gizmoColor;
            DrawFilledRect(heightPosition, Quaternion.LookRotation(Vector3.up), size * 2);
            Handles.RectangleHandleCap(0, heightPosition, Quaternion.LookRotation(Vector3.up), size, EventType.Repaint);

            if (mapBounds != null)
            {
                Vector3 floorPos = heightPosition;
                floorPos.y = mapBounds.BoundTransform.position.y;

                DrawFilledRect(floorPos, Quaternion.LookRotation(Vector3.up), size * 2);
                Handles.RectangleHandleCap(0, floorPos, Quaternion.LookRotation(Vector3.up), size, EventType.Repaint);

                // top right
                Vector3 top = heightPosition;
                top.x += size;
                top.z += size;
                DrawVerticalDottedLine(top, floorPos.y);

                // top left
                top = heightPosition;
                top.x -= size;
                top.z += size;
                DrawVerticalDottedLine(top, floorPos.y);

                // bottom right
                top = heightPosition;
                top.x += size;
                top.z -= size;
                DrawVerticalDottedLine(top, floorPos.y);

                // bottom left
                top = heightPosition;
                top.x -= size;
                top.z -= size;
                DrawVerticalDottedLine(top, floorPos.y);
            }

            Handles.color = Color.white;

            if (script.renderDivisions == bl_MapRender.RendersDivisions.Single) return;

            float div = (float)script.renderDivisions;
            float divSize = size / div;
            float halfDiv = div / 2;

            Vector3 divPos = heightPosition;
            divPos.x -= ((divSize * 2) * halfDiv) - divSize;
            divPos.z -= ((divSize * 2) * halfDiv) - divSize;
            float initX = divPos.x;

            Handles.color = new Color(1, 1, 1, 0.2f);
            for (int i = 0; i < div; i++)
            {
                for (int x = 0; x < div; x++)
                {
                    Handles.RectangleHandleCap(0, divPos, Quaternion.LookRotation(Vector3.up), divSize, EventType.Repaint);
                    divPos.x += divSize * 2;
                }
                divPos.z += divSize * 2;
                divPos.x = initX;
            }
        }

        private void DrawVerticalDottedLine(Vector3 topPos, float bottomPos)
        {
            Vector3 bottom = topPos;
            bottom.y = bottomPos;

            Handles.DrawDottedLine(topPos, bottom, 2);
        }

        private void DrawFilledRect(Vector3 position, Quaternion rotation, float size)
        {
            var matrix = Handles.matrix;
            Handles.matrix = Matrix4x4.TRS(position, Quaternion.identity, new Vector3(1, 0.001f, 1));
            Handles.CubeHandleCap(0, Vector3.zero, rotation, size, EventType.Repaint);
            Handles.matrix = matrix;
        }

        private Vector2 GetResolution()
        {
            switch (script.CurrentResolution)
            {
                case 0:
                    return new Vector2(4096, 4096);
                case 1:
                default:
                    return new Vector2(2048, 2048);
                case 2:
                    return new Vector2(1024, 1024);
                case 3:
                    return new Vector2(512, 512);
                case 4:
                    return new Vector2(256, 256);
            }
        }

        private string SnapshotName(int width, int height, string endPoint = "")
        {
            string levelName = SceneManager.GetActiveScene().name;

            //if in editor, we have to get the name through editor
            if (!Application.isPlaying)
            {
                string[] path = SceneManager.GetActiveScene().path.Split(char.Parse("/"));
                string[] fileName = path[path.Length - 1].Split(char.Parse("."));
                levelName = fileName[0];
            }

            return $"MiniMap-{levelName}-{width}x{height}{endPoint}.png";
        }
    }
#endif
}