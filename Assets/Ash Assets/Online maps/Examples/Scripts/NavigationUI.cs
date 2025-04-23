using UnityEngine;
using UnityEngine.UI;

namespace InfinityCode.OnlineMapsDemos
{
    public class NavigationUI : MonoBehaviour
    {
        /// <summary>
        /// Reference to destination input field.
        /// </summary>
        public InputField destinationInput;

        /// <summary>
        /// Reference to route confirmation UI.
        /// </summary>
        public GameObject confirmationUI;

        /// <summary>
        /// Reference to navigation UI.
        /// </summary>
        public GameObject navigationUI;

        /// <summary>
        /// Reference to finish UI.
        /// </summary>
        public GameObject finishUI;

        /// <summary>
        /// Reference to the field for displaying the total distance of the route.
        /// </summary>
        public Text totalDistanceText;

        /// <summary>
        /// Reference to the field for displaying the total duration of the route.
        /// </summary>
        public Text totalDurationText;

        /// <summary>
        /// Reference to the field for displaying the remain distance of the route.
        /// </summary>
        public Text remainDistanceText;

        /// <summary>
        /// Reference to the field for displaying the remain duration of the route.
        /// </summary>
        public Text remainDurationText;

        /// <summary>
        /// Reference to the field for displaying the instruction of the step.
        /// </summary>
        public Text instructionText;

        /// <summary>
        /// Reference to the field for displaying the total distance of the route in confirmation UI.
        /// </summary>
        public Text confirmationTotalDistanceText;

        /// <summary>
        /// Reference to the field for displaying the total duration of the route in confirmation UI.
        /// </summary>
        public Text confirmationTotalDurationText;

        private Navigation navigation;

        /// <summary>
        /// Cancels navigation from confirmation UI.
        /// </summary>
        public void CancelNavigation()
        {
            confirmationUI.SetActive(false);
            navigation.CancelNavigation();
        }

        /// <summary>
        /// Closes finish UI.
        /// </summary>
        public void CloseFinishUI()
        {
            // Hide UI
            finishUI.SetActive(false);

            navigation.routeDrawer.RemoveLines();
        }

        /// <summary>
        /// Converts the distance in meters to a human readable string.
        /// </summary>
        /// <param name="distance">Distance in meters</param>
        /// <returns>Human readable distance string</returns>
        private string GetDistanceString(int distance)
        {
            if (distance < 1000) return distance + "m";
            if (distance < 10000) return (distance / 1000f).ToString("F2") + "km";
            return distance / 1000 + "km";
        }

        /// <summary>
        /// Converts the duration in seconds to a human readable string.
        /// </summary>
        /// <param name="duration">Duration in seconds</param>
        /// <returns>Human readable duration string</returns>
        public string GetDurationString(int duration)
        {
            if (duration > 3600) return duration / 3600 + "h " + duration % 3600 / 60 + "m";
            return duration / 60 + "m";
        }

        /// <summary>
        /// Initial search for a route to a destination by clicking on the search button.
        /// </summary>
        public void FindRoute()
        {
            navigation.FindRoute();

            // Hide UI
            confirmationUI.SetActive(false);
            navigationUI.SetActive(false);
            finishUI.SetActive(false);
        }

        public void Finish()
        {
            navigationUI.SetActive(false);
            finishUI.SetActive(true);
        }

        private void OnEnable()
        {
            navigation = GetComponent<Navigation>();
        }

        public void ShowConfirmation()
        {
            confirmationUI.SetActive(true);
        }

        public void SetInstruction(string text)
        {
            instructionText.text = text;
        }

        public void SetDistance(int distance)
        {
            confirmationTotalDistanceText.text = totalDistanceText.text = "Total Distance: " + GetDistanceString(distance);
        }

        public void SetDuration(int duration)
        {
            confirmationTotalDurationText.text = totalDurationText.text = "Total Duration: " + GetDurationString(duration);
        }

        public void SetRemainDistance(int distance)
        {
            remainDistanceText.text = "Remain Distance: " + GetDistanceString(distance);
        }

        public void SetRemainDuration(int duration)
        {
            remainDurationText.text = "Remain Duration: " + GetDurationString(duration);
        }

        private void Start()
        {
            // Hide UI
            confirmationUI.SetActive(false);
            navigationUI.SetActive(false);
            finishUI.SetActive(false);
        }

        /// <summary>
        /// Starts navigation from confirmation UI
        /// </summary>
        public void StartNavigation()
        {
            // Hide confirmation UI and show navigation UI
            confirmationUI.SetActive(false);
            navigationUI.SetActive(true);

            navigation.StartNavigation();
        }

        /// <summary>
        /// Update the distance and duration on UI
        /// </summary>
        public void UpdateRemainDistanceAndDuration()
        {
            

            // Set remain distance and duration to UI
            remainDistanceText.text = "Remain Distance: " + GetDistanceString(navigation.totalDistance - navigation.coveredDistance);
            remainDurationText.text = "Remain Duration: " + GetDurationString(navigation.totalDuration - navigation.coveredDuration);
        }
    }
}