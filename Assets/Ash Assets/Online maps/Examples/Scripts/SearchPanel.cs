/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InfinityCode.OnlineMapsDemos
{
    [AddComponentMenu("Infinity Code/Online Maps/Demos/Search Panel")]
    public class SearchPanel:MonoBehaviour
    {
        /// <summary>
        /// (Optional) Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        /// <summary>
        /// Reference to the input field.
        /// </summary>
        public InputField inputField;
        
        /// <summary>
        /// Indicates whether to use autocomplete.
        /// </summary>
        public bool useAutocomplete = false;
        
        /// <summary>
        /// Reference to the autocomplete container.
        /// </summary>
        public RectTransform autocompleteContainer;
        
        /// <summary>
        /// Reference to the autocomplete item prefab.
        /// </summary>
        public GameObject autocompleteItemPrefab;
        
        /// <summary>
        /// Marker that shows the search result on the map.
        /// </summary>
        private OnlineMapsMarker marker;

        /// <summary>
        /// Hides the autocomplete container if the mouse is not over it.
        /// </summary>
        private void HideAutocomplete()
        {
            if (!useAutocomplete) return;
            if (autocompleteContainer == null) return;
            if (RectTransformUtility.RectangleContainsScreenPoint(autocompleteContainer, OnlineMapsInput.mousePosition)) return;

            if (autocompleteContainer != null) autocompleteContainer.gameObject.SetActive(false);
        }

        /// <summary>
        /// This method is called when the autocomplete request is completed.
        /// </summary>
        /// <param name="response">Response string</param>
        private void OnAutocompleteComplete(string response)
        {
            if (autocompleteContainer == null || autocompleteItemPrefab == null) return;
            
            OnlineMapsGooglePlacesAutocompleteResult[] results = OnlineMapsGooglePlacesAutocomplete.GetResults(response);
            if (results == null || results.Length == 0)
            {
                autocompleteContainer.gameObject.SetActive(false);
                return;
            }
            
            autocompleteContainer.gameObject.SetActive(true);
            foreach (Transform t in autocompleteContainer) Destroy(t.gameObject);
            
            float y = 0;
            
            foreach (OnlineMapsGooglePlacesAutocompleteResult result in results)
            {
                GameObject item = Instantiate(autocompleteItemPrefab);
                item.transform.SetParent(autocompleteContainer, false);
                item.GetComponentInChildren<Text>().text = result.description;
                item.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    inputField.text = result.description;
                    Search();
                    inputField.ActivateInputField();
                });
                

                RectTransform rectTransform = item.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(0, -y);
                y += rectTransform.rect.height;
            }
            
            RectTransform containerRectTransform = autocompleteContainer.GetComponent<RectTransform>();
            containerRectTransform.sizeDelta = new Vector2(containerRectTransform.sizeDelta.x, y);
        }

        /// <summary>
        /// This method is called when the geocoding request is completed.
        /// </summary>
        /// <param name="response">Response string</param>
        private void OnGeocodingComplete(string response)
        {
            OnlineMapsGoogleGeocodingResult[] results = OnlineMapsGoogleGeocoding.GetResults(response);
            if (results == null || results.Length == 0)
            {
                Debug.Log(response);
                return;
            }

            OnlineMapsGoogleGeocodingResult r = results[0];
            map.position = r.geometry_location;

            Vector2 center;
            int zoom;
            OnlineMapsUtils.GetCenterPointAndZoom(new[] { r.geometry_bounds_northeast, r.geometry_bounds_southwest }, out center, out zoom);
            map.zoom = zoom;

            if (marker == null) marker = OnlineMapsMarkerManager.CreateItem(r.geometry_location, r.formatted_address);
            else
            {
                marker.position = r.geometry_location;
                marker.label = r.formatted_address;
            }
        }

        /// <summary>
        /// This method is called when the input field text is changed.
        /// </summary>
        public void OnInputChanged()
        {
            if (!useAutocomplete) return;
            if (!OnlineMapsKeyManager.hasGoogleMaps) return;
            
            if (inputField.text.Length < 3)
            {
                if (autocompleteContainer != null) autocompleteContainer.gameObject.SetActive(false);
                return;
            }

            OnlineMapsGooglePlacesAutocomplete.Find(inputField.text).OnComplete += OnAutocompleteComplete;
        }

        /// <summary>
        /// This method is called when the search button is pressed.
        /// </summary>
        public void Search()
        {
            if (!OnlineMapsKeyManager.hasGoogleMaps)
            {
                Debug.LogWarning("Please enter Map / Key Manager / Google Maps");
                return;
            }

            if (inputField == null) return;
            if (inputField.text.Length < 3) return;

            string locationName = inputField.text;

            OnlineMapsGoogleGeocoding request = new OnlineMapsGoogleGeocoding(locationName, OnlineMapsKeyManager.GoogleMaps());
            request.OnComplete += OnGeocodingComplete;
            request.Send();
        }

        /// <summary>
        /// Shows the autocomplete container.
        /// </summary>
        private void ShowAutocomplete()
        {
            if (!useAutocomplete) return;
            if (autocompleteContainer == null) return;
            if (autocompleteContainer.transform.childCount == 0) return;
            
            autocompleteContainer.gameObject.SetActive(true);
        }

        private void Start()
        {
            if (map == null) map = OnlineMaps.instance;
            
            EventTrigger trigger = inputField.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry lostFocusEntry = new EventTrigger.Entry {eventID = EventTriggerType.Deselect};
            lostFocusEntry.callback.AddListener((data) => { HideAutocomplete(); });
            trigger.triggers.Add(lostFocusEntry);
            
            EventTrigger.Entry gainFocusEntry = new EventTrigger.Entry {eventID = EventTriggerType.Select};
            gainFocusEntry.callback.AddListener((data) => { ShowAutocomplete(); });
            trigger.triggers.Add(gainFocusEntry);
        }

        private void Update()
        {
            EventSystem eventSystem = EventSystem.current;
            if ((OnlineMapsInput.GetKeyUp(KeyCode.KeypadEnter) || OnlineMapsInput.GetKeyUp(KeyCode.Return)) && eventSystem.currentSelectedGameObject == inputField.gameObject)
            {
                Search();
            }
        }
    }
}
