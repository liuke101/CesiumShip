using UnityEngine;

namespace OmniVehicleAi
{
    public class TurnRadiusAssistant : MonoBehaviour
    {
        [Header("References")]
        public AIVehicleController aiVehicleController; // Reference to your AI vehicle controller
        public bool overrideInput = false;

        [Range(0, 1)]
        public float accelerationInput = 0.1f;

        private Transform VehicleTransform;
        public float turnRadius;

        // Number of frames to wait between position samples
        public int framesBetweenSamples = 25; // You can set this in the Inspector

        private int framesElapsed = 0;
        private int step = 0;

        // Positions
        private Vector3 prevPrevPos;
        private Vector3 prevPos;
        private Vector3 currPos;

        void Start()
        {
            VehicleTransform = aiVehicleController.vehicleTransform;

            // Initialize positions
            prevPrevPos = VehicleTransform.position;

            // Initialize frame counter and step
            framesElapsed = 0;
            step = 0;
        }

        private void Update()
        {
            if (overrideInput) { aiVehicleController.OverrideInput(accelerationInput, 1, 0); }
        }

        void FixedUpdate()
        {
            framesElapsed++;

            if (framesElapsed >= framesBetweenSamples)
            {
                framesElapsed = 0;
                step++;

                if (step == 1)
                {
                    // First sample after starting or after calculation
                    prevPos = VehicleTransform.position;
                }
                else if (step == 2)
                {
                    // Second sample
                    currPos = VehicleTransform.position;

                    // Calculate turn radius after collecting three positions
                    turnRadius = CalculateTurnRadius(prevPrevPos, prevPos, currPos);

                    // Optionally, display the turn radius
                    // Debug.Log("Turn Radius: " + turnRadius);

                    // In the same frame, reset prevPrevPos for the next calculation cycle
                    prevPrevPos = VehicleTransform.position;

                    // Reset step to begin the next cycle
                    step = 0;
                }
            }
        }

        float CalculateTurnRadius(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            // Extract X and Z components (assuming movement on XZ-plane)
            float x1 = p1.x;
            float z1 = p1.z;
            float x2 = p2.x;
            float z2 = p2.z;
            float x3 = p3.x;
            float z3 = p3.z;

            // Calculate the determinant (D)
            float D = 2 * (x1 * (z2 - z3) + x2 * (z3 - z1) + x3 * (z1 - z2));

            // Check for colinear points
            if (Mathf.Abs(D) < 0.0001f)
            {
                // Points are nearly colinear; return infinity as radius
                return Mathf.Infinity;
            }

            // Calculate the circle's center (Ux, Uz)
            float x1SqPlusZ1Sq = x1 * x1 + z1 * z1;
            float x2SqPlusZ2Sq = x2 * x2 + z2 * z2;
            float x3SqPlusZ3Sq = x3 * x3 + z3 * z3;

            float Ux = (x1SqPlusZ1Sq * (z2 - z3) + x2SqPlusZ2Sq * (z3 - z1) + x3SqPlusZ3Sq * (z1 - z2)) / D;
            float Uz = (x1SqPlusZ1Sq * (x3 - x2) + x2SqPlusZ2Sq * (x1 - x3) + x3SqPlusZ3Sq * (x2 - x1)) / D;

            // Calculate the radius
            float dx = x1 - Ux;
            float dz = z1 - Uz;
            float radius = Mathf.Sqrt(dx * dx + dz * dz);

            return radius;
        }

    }
}
