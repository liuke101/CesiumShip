using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

namespace OmniVehicleAi
{
    public class PathProgressTracker : MonoBehaviour
    {
        public AIVehicleController aiVehicleController;
        public SplineContainer splineContainer; // The spline container holding the spline

        [Header("Offsets")]
        public float offset_A = 15f;
        public float offset_AB = 10f;
        public float offset_BC = 10f;

        public Vector3 progressPoint { get; private set; }
        public Vector3 progressTangent { get; private set; }
        public Vector3 progressRight { get; private set; }

        public float progressDistance { get; private set; } // The progress along the spline.

        [HideInInspector] public Vector3 A { get; private set; }
        [HideInInspector] public Vector3 B { get; private set; }
        [HideInInspector] public Vector3 C { get; private set; }
        [HideInInspector] public Vector3 D { get; private set; }
        private float speed;
        

        [Header("Lap Settings")]
        public int totalLaps = 3; // Total number of laps for the race
        public int currentLap = 1; // Current lap count
        public bool loopCircuit = true; // If true, the circuit loops for multiple laps

        private Spline spline;
        private float splineLength;
        
        public UnityEvent OnLapCompleted; // Event for lap completion
        public UnityEvent OnAllLapsCompleted; // Event for all laps completion

        private void Start()
        {
            if (aiVehicleController == null)
            {
                Debug.LogError("AIVehicleController component is missing on this GameObject.");
            }

            if (splineContainer == null)
            {
                Debug.LogError("SplineContainer not assigned and none found in the scene.");
                enabled = false;
                return;
            }

            spline = splineContainer.Splines[0];
            if (spline == null)
            {
                Debug.LogError("SplineContainer does not contain any splines.");
                enabled = false;
                return;
            }

            splineLength = spline.GetLength();
        
            ResetProgress();
        }

        
        public void ResetProgress()
        {
            progressDistance = 0f;
            currentLap = 1; // Reset the lap count
        }

        private void Update()
        {
            if (aiVehicleController == null || spline == null)
            {
                return;
            }

            speed = aiVehicleController.LocalVehiclevelocity.magnitude;

            // If loopCircuit is true, the positions can wrap around the spline.
            if (loopCircuit)
            {
                A = GetSplinePoint((progressDistance + offset_A) % splineLength);
                B = GetSplinePoint((progressDistance + offset_A + offset_AB) % splineLength);
                C = GetSplinePoint((progressDistance + offset_A + offset_AB + offset_BC) % splineLength);
                D = GetSplinePoint((progressDistance + offset_A + offset_AB + offset_BC + speed) % splineLength);
            }
            else
            {
                // Clamp the positions at the end of the spline when it's not looping.
                A = GetSplinePoint(Mathf.Min(progressDistance + offset_A, splineLength));
                B = GetSplinePoint(Mathf.Min(progressDistance + offset_A + offset_AB, splineLength));
                C = GetSplinePoint(Mathf.Min(progressDistance + offset_A + offset_AB + offset_BC, splineLength));
                D = GetSplinePoint(Mathf.Min(progressDistance + offset_A + offset_AB + offset_BC + speed, splineLength));
            }

            // Get the current progress point and tangent
            progressPoint = GetSplinePoint(progressDistance % splineLength);
            progressTangent = GetSplineTangent(progressDistance % splineLength);
            progressRight = Vector3.Cross(progressTangent, Vector3.up).normalized;

            Vector3 progressDelta = progressPoint - transform.position;

            // Adjust progress distance based on direction
            if (Vector3.Dot(progressDelta, progressTangent) < 0)
            {
                progressDistance += progressDelta.magnitude * 0.5f;
            }

            // Check if we've completed a lap
            if (progressDistance >= splineLength)
            {
                CompleteLap();
            }
        }

        private void CompleteLap()
        {
            if (loopCircuit)
            {
                progressDistance -= splineLength;// Continue with remaining distance past the spline length
            }

            currentLap++;

            OnLapCompleted?.Invoke();

            if(currentLap > totalLaps)
            {
                OnAllLapsCompleted?.Invoke();
            }

            // Handle looping logic if enabled
            if (loopCircuit && currentLap > totalLaps)
            {
                currentLap = 1; // Restart from the first lap
            }
        }

        public Vector3 GetClosestPointOnCircuit(Vector3 point)
        {
            if (spline == null)
            {
                Debug.LogError("Spline is not initialized.");
                return Vector3.zero;
            }

            float closestDistance = float.MaxValue;
            Vector3 closestPoint = Vector3.zero;

            for (int i = 0; i < spline.Count - 1; i++)
            {
                float t0 = (float)i / (spline.Count - 1);
                float t1 = (float)(i + 1) / (spline.Count - 1);
                int steps = 10;

                for (int j = 0; j <= steps; j++)
                {
                    float t = Mathf.Lerp(t0, t1, j / (float)steps);
                    Vector3 splinePoint = spline.EvaluatePosition(t);
                    float distance = Vector3.Distance(point, splinePoint);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPoint = splinePoint;
                    }
                }
            }

            return closestPoint;
        }

        public Vector3 GetSplinePoint(float distance)
        {
            if (splineContainer == null)
            {
                Debug.LogError("SplineContainer is not assigned.");
                return Vector3.zero;
            }
            float normalizedDistance = distance / splineLength;
            return splineContainer.EvaluatePosition(normalizedDistance);
        }

        public Vector3 GetSplineTangent(float distance)
        {
            if (splineContainer == null)
            {
                Debug.LogError("SplineContainer is not assigned.");
                return Vector3.forward;
            }
            float normalizedDistance = distance / splineLength;

            Vector3 splineTangent = splineContainer.EvaluateTangent(normalizedDistance);

            return splineTangent.normalized;
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if(aiVehicleController.AiMode != AIVehicleController.Ai_Mode.PathFollow) { return; }

            if (Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, A);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(progressPoint, 0.2f);
                Gizmos.DrawLine(transform.position, progressPoint);
                Gizmos.color = new Color(0.8f, 0.1f, 0, 0.8f);
                Gizmos.DrawSphere(A, 1);
                Gizmos.DrawSphere(B, 1);
                Gizmos.DrawSphere(C, 1);
                Gizmos.DrawSphere(D, 1);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(A, B);
                Gizmos.DrawLine(B, C);
                Gizmos.DrawLine(C, D);
            }
            else
            {
                Vector3 tempA = transform.position + transform.forward * offset_A;
                Vector3 tempB = transform.position + transform.forward * (offset_A + offset_AB);
                Vector3 tempC = transform.position + transform.forward * (offset_A + offset_AB + offset_BC);

                Gizmos.color = new Color(0.8f, 0.1f, 0, 0.8f);
                Gizmos.DrawSphere(tempA, 1);
                Gizmos.DrawSphere(tempB, 1);
                Gizmos.DrawSphere(tempC, 1);
            }
        }

#endif
    }
}
