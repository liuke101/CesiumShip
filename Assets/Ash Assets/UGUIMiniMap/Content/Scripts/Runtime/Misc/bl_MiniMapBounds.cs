using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lovatto.MiniMap
{
    public class bl_MiniMapBounds : MonoBehaviour
    {

        [Header("Use UI editor Tool for scale the wordSpace")]
        public Color GizmoColor = new Color(1, 1, 1, 0.75f);
        public bool alwaysShow = false;

        private RectTransform m_rectTransform;
        public RectTransform BoundTransform
        {
            get
            {
                if (m_rectTransform == null) { m_rectTransform = GetComponent<RectTransform>(); }
                return m_rectTransform;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector3 position
        {
            get => BoundTransform.position;
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector3 size
        {
            get => BoundTransform.sizeDelta;
            set => BoundTransform.sizeDelta = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public Quaternion rotation
        {
            get => BoundTransform.rotation;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            Vector2 originalSize = size;
            if (size.x > size.y)
            {
                originalSize.y = size.x;
            }
            else if (size.x < size.y)
            {
                originalSize.x = size.y;
            }
            size = originalSize;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!alwaysShow) return;

            Draw();
        }

        /// <summary>
        /// Debuging world space of map
        /// </summary>
        void OnDrawGizmosSelected()
        {
            if (alwaysShow) return;

            Draw();
        }

        /// <summary>
        /// 
        /// </summary>
        void Draw()
        {
            if (m_rectTransform == null) m_rectTransform = this.GetComponent<RectTransform>();

            Vector3 v = m_rectTransform.sizeDelta;

            var matrix = Gizmos.matrix;

            Gizmos.matrix = Matrix4x4.TRS(m_rectTransform.position, m_rectTransform.rotation, Vector3.one);

            Gizmos.color = GizmoColor;
            Gizmos.DrawCube(Vector3.zero, new Vector3(v.x, v.y, 2));
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(v.x, v.y, 2));

            Gizmos.matrix = matrix;
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(bl_MiniMapBounds))]
    public class bl_MiniMapBoundsEditor : Editor
    {
        private Tool beforeTool = Tool.Move;
        bl_MiniMapBounds script;

        private void OnEnable()
        {
            beforeTool = Tools.current;
            script = (bl_MiniMapBounds)target;
        }

        private void OnDisable()
        {
            Tools.current = beforeTool;
            Tools.current = Tool.Rect;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(10);
            if (GUILayout.Button("Try to automatically set bounds"))
            {
                CalculateBounds();
            }
        }

        public void CalculateBounds()
        {
            var allMeshes = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
            var allSNMeshes = FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None);

            var renderers = new List<Renderer>();
            foreach (var item in allMeshes)
            {
                if (item.GetComponent<ParticleSystem>() != null) continue;
                renderers.Add(item);
            }
            renderers.AddRange(allSNMeshes);

            Bounds bounds = renderers.Count > 0 ? renderers[0].bounds : new Bounds();
            foreach (var item in renderers)
            {
                bounds.Encapsulate(item.bounds);
            }

            Undo.RecordObject(script.transform, "MiniMap Bounds");

            Vector3 pos = bounds.center;
            pos.y -= bounds.extents.y;
            script.transform.position = pos;

            Vector2 size = Vector3.zero;
            size.x = bounds.size.x;
            size.y = bounds.size.z;
            ((RectTransform)script.transform).sizeDelta = size;
        }

        void OnSceneGUI()
        {
            // get the chosen game object
            bl_MiniMapBounds t = target as bl_MiniMapBounds;
            if (t == null)
                return;

            Tools.current = Tool.Rect;
            t.transform.position = Handles.DoPositionHandle(t.transform.position, t.transform.rotation);
        }
    }
#endif
}