/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to make the overlay for the tileset.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/TilesetOverlayExample")]
    public class TilesetOverlayExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the tileset control. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMapsTileSetControl control;
        
        /// <summary>
        /// Overlay material.
        /// If missed, a new material with Transparent/Diffuse shader will be used.
        /// If present, the texture field will be ignored and you must specify the texture directly in the material.
        /// </summary>
        public Material material;
        
        /// <summary>
        /// Overlay texture in mercator projection
        /// </summary>
        public Texture texture;
        
        /// <summary>
        /// Overlay transparency
        /// </summary>
        [Range(0, 1)] public float alpha = 1;
        
        private Mesh overlayMesh;
        private OnlineMaps map;

        private void Start()
        {
            // If the control is not specified, get the current instance.
            if (control == null) control = OnlineMapsTileSetControl.instance;
            
            // Create overlay container
            GameObject overlayContainer = new GameObject("OverlayContainer");
            overlayContainer.transform.parent = transform;

            // Init overlay material
            MeshRenderer meshRenderer = overlayContainer.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = overlayContainer.AddComponent<MeshFilter>();
            if (material == null)
            {
                material = new Material(Shader.Find("Transparent/Diffuse"));
                material.mainTexture = texture;
            }
            
            meshRenderer.sharedMaterial = material;

            overlayMesh = meshFilter.sharedMesh = new Mesh();
            overlayMesh.name = "Overlay Mesh";
            overlayMesh.MarkDynamic();
            overlayMesh.vertices = new Vector3[4];

            if (control == null) control = OnlineMapsTileSetControl.instance;

            // Subscribe to events
            map = control.map;
            map.OnChangePosition += UpdateMesh;
            map.OnChangeZoom += UpdateMesh;

            // Init mesh
            UpdateMesh();
        }

        private void UpdateMesh()
        {
            // Clear overlay mesh
            overlayMesh.Clear(true);

            // Init vertices and normals
            Vector2 size = control.sizeInScene;
            Vector3 p1 = new Vector3(-size.x, 0, 0);
            Vector3 p2 = new Vector3(0, 0, size.y);
            float y = 0.5f;
            overlayMesh.vertices = new[]
            {
                new Vector3(p1.x, y, p1.z),
                new Vector3(p1.x, y, p2.z),
                new Vector3(p2.x, y, p2.z),
                new Vector3(p2.x, y, p1.z)
            };

            overlayMesh.normals = new[]
            {
                Vector3.up,
                Vector3.up,
                Vector3.up,
                Vector3.up
            };

            // Init overlay UV
            double tlx, tly, brx, bry;
            map.GetTileCorners(out tlx, out tly, out brx, out bry);

            int maxTileCount = 1 << map.zoom;

            float uvX1 = (float)(tlx / maxTileCount);
            float uvX2 = (float)(brx / maxTileCount);

            if (uvX1 > uvX2) uvX2 += 1;

            float uvY1 = (float)(1 - tly / maxTileCount);
            float uvY2 = (float)(1 - bry / maxTileCount);

            overlayMesh.uv = new[]
            {
                new Vector2(uvX2, uvY1),
                new Vector2(uvX2, uvY2),
                new Vector2(uvX1, uvY2),
                new Vector2(uvX1, uvY1)
            };

            // Init triangles
            overlayMesh.SetTriangles(new[]
            {
                0, 1, 2,
                0, 2, 3
            }, 0);

            overlayMesh.RecalculateBounds();
            overlayMesh.RecalculateNormals();
        }

        private void Update()
        {
            if (Math.Abs(material.color.a - alpha) > float.Epsilon)
            {
                Color color = material.color;
                color.a = alpha;
                material.color = color;
            }
        }
    }
}