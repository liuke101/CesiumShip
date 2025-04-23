/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of automatic versioning of Google Satellite.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/Auto Version Google Satellite")]
    public class AutoVersionGoogleSatellite : MonoBehaviour
    {
        /// <summary>
        /// Key for storing the last version of Google Satellite.
        /// </summary>
        private const string VERSION_KEY = "LastGoogleSatelliteVersion";
        
        /// <summary>
        /// Key for storing the last date of checking the version of Google Satellite.
        /// </summary>
        private const string LAST_CHECK_KEY = "LastGoogleSatelliteVersionCheckDate";
        
        /// <summary>
        /// Reference to the map.
        /// </summary>
        public OnlineMaps map;

        private void Start()
        {
            // If the map is not specified, then find the map.
            if (map == null) map = OnlineMaps.instance;
            
            // If the map type is Google Satellite, then get the version number.
            if (map.mapType == "google.satellite")
            {
                // Load the last known version number.
                string version = PlayerPrefs.GetString(VERSION_KEY, "953");
                
                // Set the version number to the map.
                map.activeType["version"] = version;
                
                // Get the date of the last check.
                string lastCheckDate = PlayerPrefs.GetString(LAST_CHECK_KEY, "2023-07-26");
                DateTime lastCheckDateTime = DateTime.ParseExact(lastCheckDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime currentDate = DateTime.Now;
                TimeSpan difference = currentDate - lastCheckDateTime;
                
                // If the last check was more than 3 days ago, then check the version number.
                if (difference.TotalDays > 3)
                {
                    // Send a request to the Google Maps API.
                    OnlineMapsWWW www = new OnlineMapsWWW("http://maps.googleapis.com/maps/api/js");
                    www.OnComplete += OnRequestComplete;
                }
            }
        }

        /// <summary>
        /// Event that occurs when the request is completed.
        /// </summary>
        /// <param name="www">Reference to the request.</param>
        private void OnRequestComplete(OnlineMapsWWW www)
        {
            // If there was an error, then exit the method.
            if (www.hasError) return;
            
            // Get the response text.
            string response = www.text;
            
            // Find the version number in the response text.
            Match match = Regex.Match(response, @"kh\?v=(\d+)");
            
            // If the version number was not found, then exit the method.
            if (!match.Success) return;
            
            // Get the version number.
            string version = match.Groups[1].Value;
            
            // Save the version number and the date of the last check.
            PlayerPrefs.SetString(VERSION_KEY, version);
            PlayerPrefs.SetString(LAST_CHECK_KEY, DateTime.Now.ToString("yyyy-MM-dd"));
            
            // Set the version number to the map.
            map.activeType["version"] = version;
        }
    }
}