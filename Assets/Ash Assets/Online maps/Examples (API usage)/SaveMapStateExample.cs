/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of runtime saving map state.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/SaveMapStateExample")]
    public class SaveMapStateExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        /// <summary>
        /// List of marker textures.
        /// </summary>
        public List<Texture2D> markerTextures;
        
        /// <summary>
        /// List of marker 3D prefabs.
        /// </summary>
        public List<GameObject> marker3DPrefabs;

        /// <summary>
        /// Key for PlayerPrefs.
        /// </summary>
        private string key = "MapSettings";

        private void Start()
        {
            // If the map is not specified, then find the current instance.
            if (map == null) map = OnlineMaps.instance;
            
            LoadState();
        }

        /// <summary>
        /// Load 2D markers from JSON.
        /// </summary>
        /// <param name="manager">Marker manager</param>
        /// <param name="json">JSON</param>
        private void LoadMarkerManager(OnlineMapsMarkerManager manager, OnlineMapsJSONItem json)
        {
            if (json == null) return;
            
            manager.RemoveAll();
            foreach (OnlineMapsJSONItem jitem in json)
            {
                double mx = jitem.V<double>("longitude");
                double my = jitem.V<double>("latitude");
                int textureIndex = jitem.V<int>("texture");
                Texture2D texture = null;
                if (textureIndex > -1 && textureIndex < markerTextures.Count) texture = markerTextures[textureIndex];
                string label = jitem.V<string>("label");

                OnlineMapsMarker marker = manager.Create(mx, my, texture, label);
                
                marker.range = jitem.V<OnlineMapsRange>("range");
                marker.align = (OnlineMapsAlign)jitem.V<int>("align");
                marker.rotation = jitem.V<float>("rotation");
                marker.enabled = jitem.V<bool>("enabled");
            }
        }

        /// <summary>
        /// Load 3D markers from JSON.
        /// </summary>
        /// <param name="manager">Marker manager</param>
        /// <param name="json">JSON</param>
        private void LoadMarker3DManager(OnlineMapsMarker3DManager manager, OnlineMapsJSONItem json)
        {
            if (manager == null || json == null) return;
            
            manager.RemoveAll();
            foreach (OnlineMapsJSONItem jitem in json)
            {
                double mx = jitem.V<double>("longitude");
                double my = jitem.V<double>("latitude");
                int prefabIndex = jitem.V<int>("prefab");
                GameObject prefab = null;
                if (prefabIndex > -1 && prefabIndex < marker3DPrefabs.Count) prefab = marker3DPrefabs[prefabIndex];

                OnlineMapsMarker3D marker = manager.Create(mx, my, prefab);

                marker.range = jitem.V<OnlineMapsRange>("range");
                marker.label = jitem.V<string>("label");
                marker.rotationY = jitem.V<float>("rotationY");
                marker.scale = jitem.V<float>("scale");
                marker.enabled = jitem.V<bool>("enabled");
                marker.sizeType = (OnlineMapsMarker3D.SizeType)jitem.V<int>("sizeType");
            }
        }

        /// <summary>
        /// Loading saved state.
        /// </summary>
        private void LoadState()
        {
            if (!PlayerPrefs.HasKey(key)) return;

            // Load map position and zoom
            string settings = PlayerPrefs.GetString(key);
            OnlineMapsJSONItem json = OnlineMapsJSON.Parse(settings);
            OnlineMapsJSONItem jpos = json["Map/Coordinates"];
            map.position = jpos.Deserialize<Vector2>();
            map.floatZoom = json["Map/Zoom"].V<float>();

            // Load 2D and 3D markers
            LoadMarkerManager(map.markerManager, json["Markers"]);
            LoadMarker3DManager(map.marker3DManager, json["Markers3D"]);
        }

        private void OnGUI()
        {
            // By clicking on the button to save the current state.
            if (GUI.Button(new Rect(5, 5, 150, 30), "Save State")) SaveState();
        }

        private void SaveState()
        {
            OnlineMapsJSONObject json = new OnlineMapsJSONObject();

            // Save position and zoom
            OnlineMapsJSONObject jmap = new OnlineMapsJSONObject();
            json.Add("Map", jmap);
            jmap.Add("Coordinates", map.position);
            jmap.Add("Zoom", map.floatZoom);

            // Save 2D markers
            OnlineMapsJSONArray jmarkers = new OnlineMapsJSONArray();
            foreach (OnlineMapsMarker marker in map.markerManager)
            {
                OnlineMapsJSONObject jmarker = marker.ToJSON() as OnlineMapsJSONObject;
                jmarker.Add("texture", markerTextures.IndexOf(marker.texture));
                jmarkers.Add(jmarker);
            }
            json.Add("Markers", jmarkers);

            // Save 3D markers
            if (map.marker3DManager != null)
            {
                OnlineMapsJSONArray jmarkers3d = new OnlineMapsJSONArray();
                foreach (OnlineMapsMarker3D marker in OnlineMapsMarker3DManager.instance)
                {
                    OnlineMapsJSONObject jmarker = marker.ToJSON() as OnlineMapsJSONObject;
                    jmarker.Add("prefab", marker3DPrefabs.IndexOf(marker.prefab));
                    jmarkers3d.Add(jmarker);
                }
                json.Add("Markers3D", jmarkers3d);
            }

            Debug.Log(json.ToString());
            
            // Save settings to PlayerPrefs
            PlayerPrefs.SetString(key, json.ToString());
        }
    }
}