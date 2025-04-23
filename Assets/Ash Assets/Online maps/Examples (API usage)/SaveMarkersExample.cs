/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of runtime saving 2D markers to PlayerPrefs, and loading of 2D markers from PlayerPrefs.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/SaveMarkersExample")]
    public class SaveMarkersExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map control. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMapsControlBase control;
        
        /// <summary>
        /// Key in PlayerPrefs
        /// </summary>
        private static string prefsKey = "markers";

        /// <summary>
        /// Use this for initialization
        /// </summary>
        private void Start()
        {
            // If the control is not specified, get the current instance.
            if (control == null) control = OnlineMapsControlBase.instance;
            
            // Try load markers
            TryLoadMarkers();
        }

        /// <summary>
        /// Saves markers to PlayerPrefs as xml string
        /// </summary>
        public void SaveMarkers()
        {
            // Create XMLDocument and first child
            OnlineMapsXML xml = new OnlineMapsXML("Markers");

            // Save markers data
            foreach (OnlineMapsMarker marker in control.markerManager)
            {
                // Create marker node
                OnlineMapsXML markerNode = xml.Create("Marker");
                markerNode.Create("Position", marker.position);
                markerNode.Create("Label", marker.label);
            }

            // Save xml string
            PlayerPrefs.SetString(prefsKey, xml.outerXml);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Try load markers from PlayerPrefs
        /// </summary>
        private void TryLoadMarkers()
        {
            // If the key does not exist, returns.
            if (!PlayerPrefs.HasKey(prefsKey)) return;

            // Load xml string from PlayerPrefs
            string xmlData = PlayerPrefs.GetString(prefsKey);

            // Load xml document
            OnlineMapsXML xml = OnlineMapsXML.Load(xmlData);

            // Load markers
            foreach (OnlineMapsXML node in xml)
            {
                // Gets coordinates and label
                Vector2 position = node.Get<Vector2>("Position");
                string label = node.Get<string>("Label");

                // Create marker
                control.markerManager.Create(position, label);
            }
        }
    }
}