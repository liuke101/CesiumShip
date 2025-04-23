/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityCode.OnlineMapsDemos
{
    public class Navigation : MonoBehaviour
    {
        #region FIELDS

        /// <summary>
        /// Reference to Control. If missed, the singleton value will be used.
        /// </summary>
        public OnlineMapsTileSetControl control;

        /// <summary>
        /// The distance (km) from the user's location to the nearest point on the route to consider that he left the route.
        /// </summary>
        public float updateRouteAfterKm = 0.05f;

        /// <summary>
        /// Delay to find a new route.
        /// </summary>
        public float updateRouteDelay = 10;

        /// <summary>
        /// If TRUE the user's GPS position will be used, if FALSE the marker position will be used, which you can drag.
        /// </summary>
        public bool useLocationServicePosition;

        /// <summary>
        /// Should the compass value be smoothed.
        /// </summary>
        public bool lerpCompassValue = true;

        /// <summary>
        /// Prefab of 3D marker.
        /// </summary>
        public GameObject markerPrefab;

        private OnlineMaps map;
        private OnlineMapsMarker3D marker;
        private OnlineMapsVector2d[] routePoints;
        private OnlineMapsVector2d lastPosition;
        private bool followRoute;
        private float speed = 0;
        private LastPositionItem lastKnownLocation;
        private OnlineMapsVector2d currentLocation;
        private OnlineMapsVector2d correction;
        private float correctionProgress;
        private int maxPositionCount = 3;
        private List<LastPositionItem> lastPositions;
        private float timeToUpdateRoute = float.MinValue;
        private OnlineMapsVector2d destinationPoint;
        private float compass;
        private float smoothedCompass;
        private float correctionTime = 2f;

        #endregion

        #region PROPERTIES

        public int currentStepIndex { get; private set; }
        public int coveredDistance { get; private set; }
        public int coveredDuration { get; private set; }
        public OnlineMapsVector2d lastPointOnRoute { get; private set; }
        public NavigationUI navigationUI { get; private set; }
        public int pointIndex { get; private set; }
        public OnlineMapsGoogleDirectionsResult.Step[] steps { get; private set; }

        public int remainDistance
        {
            get { return totalDistance - coveredDistance; }
        }

        public int remainDuration
        {
            get { return totalDuration - coveredDuration; }
        }

        public NavigationRouteDrawer routeDrawer { get; private set; }

        public int totalDistance { get; private set; }
        public int totalDuration { get; private set; }

        #endregion

        public void CancelNavigation()
        {
            routeDrawer.RemoveLines();

            routePoints = null;
            steps = null;
            followRoute = false;
        }

        /// <summary>
        /// Checks if the user has reached the destination.
        /// </summary>
        /// <param name="position">User's position</param>
        /// <returns>Whether the user has reached the destination</returns>
        private bool CheckFinished(Vector2 position)
        {
            if (currentStepIndex != steps.Length - 1) return false;

            // Get distance between user and destination
            double d = OnlineMapsUtils.DistanceBetweenPoints(position.x, position.y, 0, destinationPoint.x, destinationPoint.y, 0);

            // If the distance is less than the threshold, the user has reached the destination
            if (d < 0.02) return false;

            // Stop navigation and show finish UI
            followRoute = false;
            navigationUI.Finish();

            Debug.Log("Finished");

            return true;
        }

        public void FindRoute()
        {
            // Check for Google Maps API key
            if (!OnlineMapsKeyManager.hasGoogleMaps)
            {
                Debug.LogWarning("Please enter Map / Key Manager / Google Maps");
                return;
            }

            // Send request to Google Directions API
            OnlineMapsGoogleDirections request = OnlineMapsGoogleDirections.Find(
                new OnlineMapsGoogleDirections.Params(
                    GetUserLocation(),
                    navigationUI.destinationInput.text));

            // When the request is complete call the OnRequestComplete method.
            request.OnComplete += OnRequestComplete;
        }

        private OnlineMapsVector2d GetExpectedPosition()
        {
            if (!useLocationServicePosition) return marker.position;
            if (Math.Abs(speed) < float.Epsilon) return new OnlineMapsVector2d(lastKnownLocation.lng, lastKnownLocation.lat);

            double coveredDistance = (Time.time - lastKnownLocation.timestamp) * speed / 3600f;
            double lng, lat;
            OnlineMapsUtils.GetCoordinateInDistance(lastKnownLocation.lng, lastKnownLocation.lat, (float)coveredDistance, compass, out lng, out lat);

            OnlineMapsVector2d position = new OnlineMapsVector2d(lng, lat);

            if (correctionProgress < 1 && correction.SqrMagnitude() > 0)
            {
                float nextCorrectionProgress = correctionProgress + Time.deltaTime / correctionTime;
                if (nextCorrectionProgress > 1) nextCorrectionProgress = 1;

                float correctionDelta = nextCorrectionProgress - correctionProgress;
                currentLocation.x += correction.x * correctionDelta;
                currentLocation.y += correction.y * correctionDelta;

                correctionProgress = nextCorrectionProgress;
            }
            return position;
        }

        /// <summary>
        /// Finds the nearest point on the route and checks if the user has left the route.
        /// </summary>
        /// <param name="position">User location.</param>
        /// <param name="positionOnRoute">Returns the nearest point on the route.</param>
        /// <param name="pointChanged">Returns whether the number of the route point in use has changed.</param>
        /// <returns>Returns whether the user is following the route.</returns>
        private bool GetPointOnRoute(Vector2 position, out bool pointChanged)
        {
            pointChanged = false;
            var step = steps[currentStepIndex];
            OnlineMapsVector2d p1 = step.polylineD[pointIndex];
            OnlineMapsVector2d p2 = step.polylineD[pointIndex + 1];
            OnlineMapsVector2d p;
            double dist;

            if (p1 != p2)
            {
                // Check if the user is on the same route point.
                OnlineMapsUtils.NearestPointStrict(position.x, position.y, p1.x, p1.y, p2.x, p2.y, out p.x, out p.y);
                if (p != p2)
                {
                    dist = OnlineMapsUtils.DistanceBetweenPoints(p.x, p.y, 0, position.x, position.y, 0);

                    if (dist < updateRouteAfterKm)
                    {
                        timeToUpdateRoute = float.MinValue;
                        lastPointOnRoute = p;
                        return true;
                    }
                }
            }

            // Checking what step and point the user is on
            for (int i = currentStepIndex; i < steps.Length; i++)
            {
                step = steps[i];
                OnlineMapsVector2d[] polyline = step.polylineD;

                for (int j = pointIndex; j < polyline.Length - 1; j++)
                {
                    p1 = polyline[j];
                    p2 = polyline[j + 1];
                    OnlineMapsUtils.NearestPointStrict(position.x, position.y, p1.x, p1.y, p2.x, p2.y, out p.x, out p.y);
                    if (p == p2) continue;

                    dist = OnlineMapsUtils.DistanceBetweenPoints(p.x, p.y, 0, position.x, position.y, 0);
                    if (dist < updateRouteAfterKm)
                    {
                        // Update the step instruction and save the index of step and point.
                        navigationUI.SetInstruction(step.string_instructions);
                        currentStepIndex = i;
                        pointChanged = true;
                        pointIndex = j;
                        timeToUpdateRoute = float.MinValue;
                        lastPointOnRoute = p;
                        if (!useLocationServicePosition) marker.LookToCoordinates(polyline[polyline.Length - 1]);
                        return true;
                    }
                }

                pointIndex = 0;
            }

            // The user has left the route. If the countdown to the search for a new route has not started, we start it.
            if (timeToUpdateRoute < -999) timeToUpdateRoute = updateRouteDelay;

            return false;
        }

        /// <summary>
        /// Gets the user's location.
        /// </summary>
        /// <returns>User's location</returns>
        private Vector2 GetUserLocation()
        {
            if (useLocationServicePosition) return OnlineMapsLocationService.instance.position;
            return marker.position;
        }

        /// <summary>
        /// Called when the compass value has been changed.
        /// </summary>
        /// <param name="rotation">Compass true heading (0-1)</param>
        private void OnCompassChanged(float rotation)
        {
            // Set the rotation of the marker.
            // Update compass value
            compass = rotation * 360;

            // If the marker rotation should not smooth, update the rotation
            if (!lerpCompassValue && marker != null) marker.rotationY = rotation;
        }

        private void OnEnable()
        {
            navigationUI = GetComponent<NavigationUI>();
            routeDrawer = GetComponent<NavigationRouteDrawer>();
        }

        /// <summary>
        /// Called when the user's GPS position has changed.
        /// </summary>
        /// <param name="position">User's GPS position</param>
        private void OnLocationChanged(Vector2 position)
        {
            // Save a new location
            lastKnownLocation = new LastPositionItem(position.x, position.y, Time.time);

            // Calculating the correction vector
            correction = (OnlineMapsVector2d)position - currentLocation;

            // Update current speed
            UpdateSpeed();

            // Calculate a distance between new and old locations
            double dx, dy;
            OnlineMapsUtils.DistanceBetweenPoints(position.x, position.y, currentLocation.x, currentLocation.y, out dx, out dy);
            double d = Math.Sqrt(dx * dx + dy * dy);

            // If the distance is too long or the speed is too low, update the location
            if (d > 0.01 || speed < 1)
            {
                currentLocation = position;
                correction = OnlineMapsVector2d.zero;
            }

            // Reset correction progress
            correctionProgress = 0;
        }

        /// <summary>
        /// This method is called when the Google Directions API returned a response.
        /// </summary>
        /// <param name="response">Response from the service</param>
        private void OnRequestComplete(string response)
        {
            // Parse a response
            OnlineMapsGoogleDirectionsResult result = OnlineMapsGoogleDirections.GetResult(response);

            // If there are no routes, return
            if (result.routes.Length == 0)
            {
                Debug.Log("Can't find route");
                return;
            }

            OnlineMapsGoogleDirectionsResult.Route route = result.routes[0];
            if (route == null)
            {
                Debug.Log("Can't find route");
                return;
            }

            // Reset step and point indices
            currentStepIndex = 0;
            pointIndex = 0;

            // Get steps from the route
            steps = route.legs.SelectMany(l => l.steps).ToArray();

            // Get route points from steps
            routePoints = steps.SelectMany(s => s.polylineD).ToArray();

            // The remaining points are the entire route
            routeDrawer.SetRemainPoints(routePoints.ToList());

            // The destination is the last point
            destinationPoint = routePoints.Last();

            // Calculate total distance and duration
            totalDistance = route.legs.Sum(l => l.distance.value);
            totalDuration = route.legs.Sum(l => l.duration.value);

            // Set distance, duration and first instruction on UI
            navigationUI.SetDistance(totalDistance);
            navigationUI.SetDuration(totalDuration);
            navigationUI.SetRemainDistance(totalDistance);
            navigationUI.SetRemainDuration(totalDuration);
            navigationUI.SetInstruction(steps[0].string_instructions);

            // Show the whole route
            OnlineMapsGPXObject.Bounds b = route.bounds;

            Vector2[] bounds =
            {
                new Vector2((float) b.minlon, (float) b.maxlat),
                new Vector2((float) b.maxlon, (float) b.minlat),
            };

            Vector2 center;
            int zoom;
            OnlineMapsUtils.GetCenterPointAndZoom(bounds, out center, out zoom);

            map.SetPositionAndZoom(center.x, center.y, zoom);
            lastPosition = marker.position;

            // If a marker position is used, turn it towards the second point
            if (!useLocationServicePosition) marker.LookToCoordinates(routePoints[1]);

            // Show confirmation UI
            navigationUI.ShowConfirmation();
        }

        /// <summary>
        /// This method is called when Google Directions API returned updated route.
        /// </summary>
        /// <param name="response"></param>
        private void OnUpdateRouteComplete(string response)
        {
            // Parse a response
            OnlineMapsGoogleDirectionsResult result = OnlineMapsGoogleDirections.GetResult(response);

            // If there are no routes, return
            if (result.routes.Length == 0)
            {
                Debug.Log("Can't find route");
                return;
            }

            OnlineMapsGoogleDirectionsResult.Route route = result.routes[0];
            if (route == null)
            {
                Debug.Log("Can't find route");
                return;
            }

            // Get steps from route
            steps = route.legs.SelectMany(l => l.steps).ToArray();

            // Get route points from steps
            routePoints = steps.SelectMany(s => s.polylineD).ToArray();
            destinationPoint = routePoints.Last();

            // Calculate total distance and duration
            totalDistance = route.legs.Sum(l => l.distance.value);
            totalDuration = route.legs.Sum(l => l.duration.value);

            // Set distance, duration and first instruction on UI
            navigationUI.SetDistance(totalDistance);
            navigationUI.SetDuration(totalDuration);
            navigationUI.SetRemainDistance(totalDistance);
            navigationUI.SetRemainDuration(totalDuration);
            navigationUI.SetInstruction(steps[0].string_instructions);

            // Reset step and point indices
            currentStepIndex = 0;
            pointIndex = 0;

            // Update covered and remain lines
            routeDrawer.UpdateLines();
        }

        /// <summary>
        /// Requests an updated route
        /// </summary>
        private void RequestUpdateRoute()
        {
            if (!OnlineMapsKeyManager.hasGoogleMaps)
            {
                Debug.LogWarning("Please enter Map / Key Manager / Google Maps");
            }

            // Send request to Google Directions API
            OnlineMapsGoogleDirections request = OnlineMapsGoogleDirections.Find(
                new OnlineMapsGoogleDirections.Params(
                    GetUserLocation(),
                    navigationUI.destinationInput.text));

            // When the request is complete call OnUpdateRouteComplete method.
            request.OnComplete += OnUpdateRouteComplete;
        }

        private void SmoothCompass()
        {
            if (lerpCompassValue)
            {
                if (compass - smoothedCompass > 180) smoothedCompass += 360;
                else if (compass - smoothedCompass < -180) smoothedCompass -= 360;

                if (Math.Abs(compass - smoothedCompass) >= float.Epsilon)
                {
                    if (Mathf.Abs(compass - smoothedCompass) < 0.003f) smoothedCompass = compass;
                    else smoothedCompass = Mathf.Lerp(smoothedCompass, compass, 0.02f);

                    marker.rotationY = smoothedCompass;
                }
            }
        }

        private void Start()
        {
            // Get map and control instances
            if (control == null) control = OnlineMapsTileSetControl.instance;
            map = control.map;

            // Create a new marker in the center of the map
            double longitude, latitude;
            map.GetPosition(out longitude, out latitude);
            marker = control.marker3DManager.Create(longitude, latitude, markerPrefab);

            currentStepIndex = pointIndex = -1;

            // If use location service, subscribe to events
            // Else make a marker draggable
            if (useLocationServicePosition)
            {
                OnlineMapsLocationService.instance.OnLocationChanged += OnLocationChanged;
                OnlineMapsLocationService.instance.OnCompassChanged += OnCompassChanged;
            }
            else marker.isDraggable = true;
        }

        public void StartNavigation()
        {
            // Zoom in on the map at the first route point
            map.SetPositionAndZoom(routePoints[0].x, routePoints[0].y, 19);

            // Create covered line
            routeDrawer.InitCoveredPoints();

            // Start navigation and reset indices
            followRoute = true;
            currentStepIndex = 0;
            pointIndex = 0;
        }

        /// <summary>
        /// Called every frame
        /// </summary>
        private void Update()
        {
            SmoothCompass();

            // If navigation is not started, return
            if (!followRoute) return;

            // If the user has left the route, wait for a delay and request a new route
            if (timeToUpdateRoute > 0)
            {
                timeToUpdateRoute -= Time.deltaTime;
                if (timeToUpdateRoute <= 0)
                {
                    timeToUpdateRoute = float.MinValue;
                    RequestUpdateRoute();
                }
            }

            // Get the position of the marker, and if it hasn't changed, return
            OnlineMapsVector2d currentPosition = GetExpectedPosition();
            //Debug.Log(currentPosition.x + "   " + currentPosition.y);
            if (currentPosition == lastPosition) return;

            lastPosition = currentPosition;
            bool pointChanged;

            // Check if the user has reached the destination
            if (CheckFinished(currentPosition))
            {
                marker.position = currentPosition;
            }
            // Get the nearest point on a route
            else if (GetPointOnRoute(currentPosition, out pointChanged))
            {
                if (useLocationServicePosition) marker.SetPosition(lastPointOnRoute.x, lastPointOnRoute.y);
                else marker.position = currentPosition;

                UpdateCoveredValues();

                // Update covered and remain lines
                routeDrawer.UpdateLines();

                // If the point index has changed, update the distance and duration on UI
                if (pointChanged) navigationUI.UpdateRemainDistanceAndDuration();

                // Redraw the map
                map.Redraw();
            }
            else
            {
                marker.position = currentPosition;

                // The user has left the route
                Debug.Log("The user has left the route");
            }
        }

        private void UpdateCoveredValues()
        {
            coveredDistance = 0;
            coveredDuration = 0;

            OnlineMapsGoogleDirectionsResult.Step s;

            // Sum the distances and the duration of covered steps
            for (int i = 0; i < currentStepIndex; i++)
            {
                s = steps[i];
                coveredDistance += s.distance.value;
                coveredDuration += s.duration.value;
            }

            s = steps[currentStepIndex];
            OnlineMapsVector2d[] polyline = s.polylineD;
            double stepDistance = 0;

            // Sum the distance between covered points on current step
            for (int i = 0; i < pointIndex - 1; i++)
            {
                OnlineMapsVector2d p1 = polyline[i];
                OnlineMapsVector2d p2 = polyline[i + 1];
                stepDistance += OnlineMapsUtils.DistanceBetweenPoints(p1.x, p1.y, 0, p2.x, p2.y, 0) * 1000;
            }

            // Add the progress of the current step to the covered distance and duration.
            if (stepDistance > s.distance.value) stepDistance = s.distance.value;
            coveredDistance += (int)stepDistance;
            coveredDuration += (int)(stepDistance / s.distance.value * s.duration.value);
        }

        /// <summary>
        /// Update speed
        /// </summary>
        public void UpdateSpeed()
        {
            if (lastPositions == null) lastPositions = new List<LastPositionItem>();

            lastPositions.Add(lastKnownLocation);
            while (lastPositions.Count > maxPositionCount) lastPositions.RemoveAt(0);

            if (lastPositions.Count < 2)
            {
                speed = 0;
                return;
            }

            LastPositionItem p1 = lastPositions[0];
            LastPositionItem p2 = lastPositions[lastPositions.Count - 1];

            double dx, dy;
            OnlineMapsUtils.DistanceBetweenPoints(p1.lng, p1.lat, p2.lng, p2.lat, out dx, out dy);
            double distance = Math.Sqrt(dx * dx + dy * dy);
            double time = (p2.timestamp - p1.timestamp) / 3600;
            speed = Mathf.Abs((float)(distance / time));
        }

        internal struct LastPositionItem
        {
            public float lat;
            public float lng;
            public double timestamp;

            public LastPositionItem(float longitude, float latitude, double timestamp)
            {
                lng = longitude;
                lat = latitude;
                this.timestamp = timestamp;
            }
        }
    }
}