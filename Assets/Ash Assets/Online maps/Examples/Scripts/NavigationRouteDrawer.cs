using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.OnlineMapsDemos
{
    public class NavigationRouteDrawer : MonoBehaviour
    {
        private Navigation navigation;

        private List<OnlineMapsVector2d> remainPoints;
        private List<OnlineMapsVector2d> coveredPoints;
        private OnlineMapsDrawingLine routeLine;
        private OnlineMapsDrawingLine coveredLine;

        private OnlineMapsDrawingElementManager drawingElementManager
        {
            get { return navigation.control.drawingElementManager; }
        }

        public void InitCoveredPoints()
        {
            coveredPoints = new List<OnlineMapsVector2d>(remainPoints.Count);
            coveredLine = new OnlineMapsDrawingLine(coveredPoints, Color.gray, 3);
            drawingElementManager.Add(coveredLine);
        }

        private void OnEnable()
        {
            navigation = GetComponent<Navigation>();
        }

        public void RemoveLines()
        {
            // Remove covered and remain lines.
            drawingElementManager.Remove(routeLine);
            routeLine = null;

            if (coveredLine != null)
            {
                drawingElementManager.Remove(coveredLine);
                coveredLine = null;
            }
        }

        public void SetRemainPoints(List<OnlineMapsVector2d> points)
        {
            remainPoints = points;

            // Create a line and add it to the map
            if (routeLine == null)
            {
                routeLine = new OnlineMapsDrawingLine(remainPoints, Color.green, 3);
                drawingElementManager.Add(routeLine);
            }
            else routeLine.points = remainPoints;
        }

        /// <summary>
        /// Updates covered and remain lines
        /// </summary>
        public void UpdateLines()
        {
            // Clears line points.
            // It doesn't make sense to create new lines here, because drawing elements keeps a reference to the lists.
            coveredPoints.Clear();
            remainPoints.Clear();

            // Iterate all steps.
            OnlineMapsGoogleDirectionsResult.Step[] steps = navigation.steps;
            int currentStepIndex = navigation.currentStepIndex;

            for (int i = 0; i < steps.Length; i++)
            {
                // Get a polyline
                var step = steps[i];
                OnlineMapsVector2d[] polyline = step.polylineD;

                // Iterate all points of polyline
                for (int j = 0; j < polyline.Length; j++)
                {
                    OnlineMapsVector2d p = polyline[j];

                    // If index of step less than current step, add to covered list
                    // If index of step greater than current step, add to remain list
                    // If this is current step, points than less current point add to covered list, otherwise add to remain list
                    if (i < currentStepIndex)
                    {
                        coveredPoints.Add(p);
                    }
                    else if (i > currentStepIndex)
                    {
                        remainPoints.Add(p);
                    }
                    else
                    {
                        if (j < navigation.pointIndex)
                        {
                            coveredPoints.Add(p);
                        }
                        else if (j > navigation.pointIndex)
                        {
                            remainPoints.Add(p);
                        }
                        else
                        {
                            coveredPoints.Add(p);
                            coveredPoints.Add(navigation.lastPointOnRoute);
                            remainPoints.Add(navigation.lastPointOnRoute);
                        }
                    }
                }
            }
        }
    }
}