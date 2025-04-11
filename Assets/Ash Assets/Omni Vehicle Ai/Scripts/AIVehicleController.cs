using System;
using UnityEngine;
using UnityEngine.Splines;

namespace OmniVehicleAi
{
    public class AIVehicleController : MonoBehaviour
    {
        #region Variables

        public enum Ai_Mode { TargetFollow, PathFollow };

        public Ai_Mode AiMode;
        [Tooltip("Speed at which AI input is interpolated.")]
        public float InputLerpSpeed = 5f;

        [Header("PathFollow Settings")]
        [HideInInspector] public Vector3 A, B, C, D;

        [Header("Vehicle References")]
        public Rigidbody vehicleRigidbody;
        public Transform vehicleTransform;
        public Transform target;

        [Header("Target Follow Settings")]
        [Tooltip("Minimum distance to the target before stopping.")]
        public float stoppingDistance = 1f;
        //[Tooltip("Distance at which the vehicle begins to slow down.")]
        //public float slowDownDistance = 25f;
        //[Tooltip("Distance at which braking begins.")]
        //public float brakingDistance = 15f;
        [Tooltip("Distance at which reversing is triggered.")]
        public float reverseDistance = 15f;

        [Header("Reversing Settings")]
        [Tooltip("Time before the vehicle is considered stuck.")]
        public float stuckTime = 1.5f;
        [Tooltip("Time spent reversing after getting stuck.")]
        public float reversingTime = 1.5f;

        private float stuckTimer = 0f;
        private float reversingTimer = 0f;

        [Header("AI Input Settings")]
        [Tooltip("AI-controlled acceleration input.")]
        private float accelerationInputAi = 0f;
        [Tooltip("AI-controlled steering input.")]
        private float steerInputAi = 0f;
        [Tooltip("AI-controlled handbrake input.")]
        private float handBrakeInputAi = 0f;

        [Header("Speed Settings")]
        //[Tooltip("Maximum steering angle for the vehicle.")]
        //public float maxSteerAngle = 30f;
        //[Tooltip("Speed at which the vehicle slows down when approaching a turn.")]
        //public float slowDownSpeed = 20f;
        [Tooltip("Speed threshold for slowing down during a turn.")]
        public float turnSlowDownSpeed = 10f;
        [Tooltip("Rate at which the vehicle decelerates.")]
        public float decelerationRate = 10f;
        [Tooltip("Buffer speed to avoid abrupt changes in acceleration.")]
        public float bufferSpeed = 5f;
        [Tooltip("Determines if the vehicle should brake based on its speed to completely stop at the destination.")]
        public bool useSpeedBasedBraking = true;

        [Header("Turn Settings")]
        [Tooltip("Angle threshold for slowing down during a turn.")]
        public float turnSlowDownAngle = 15f;
        [Tooltip("Buffer angle to avoid abrupt changes in steering.")]
        public float bufferAngle = 5f;
        [Tooltip("Calculated turning radius based on vehicle dimensions and steering angle.")]
        public float TurnRadius = 5f;
        [Tooltip("Turn radius offset in forward direction of the vehicle for gizmos")]
        public float TurnRadiusOffset = 0f;


        [Header("Obstacle Avoidance Settings")]
        [Tooltip("Speed at which the vehicle slows down when an obstacle is detected.")]
        public float ObstacleSlowDownSpeed = 30f;
        [Tooltip("Distance at which the vehicle starts turning to avoid an obstacle.")]
        public float ObstacleTurnDistance = 25f;

        [Header("Vehicle Input")]
        private float accelerationInput;
        private float steerInput;
        private float handBrakeInput;


        [Header("Sensor Settings")]
        public Sensor[] frontSensors;
        public Sensor[] sideSensors;
        [Tooltip("Length of the sensors for detecting obstacles.")]
        public float sensorLength;
        [Tooltip("Layer mask for detecting obstacles.")]
        public LayerMask ObstacleLayers;
        [Tooltip("Width of the road for calculating off-track conditions.")]
        public float roadWidth;

        [HideInInspector] public Vector3 LocalVehiclevelocity;
        [HideInInspector] public float turnmultiplyer;
        [HideInInspector] public float SensorTurnAmount;
        [HideInInspector] public bool obstacleInPath;
        [HideInInspector] public float ObstacleAngle;
        private float obstacleDistance;
        [HideInInspector] public Transform progressTransform { get; private set; }
        [HideInInspector] public Transform pathTarget { get; private set; }

        private PathProgressTracker pathProgressTracker;
        private bool FL, FR, RL, RR, IL, IR;
        private bool isTakingReverse = false;
        private bool stopVehicle = false, canSteer = true;

        private float overrideAccelerationInput = 0f;
        private float overrideSteerInput = 0f;
        private float overrideHandBrakeInput = 0f;
        private bool isOverrideActive = false;
        private bool isFrontSensorDetected = false;
        private bool useReverseIfStuck = true;
        //private bool useSensors = true;

        private bool isTargetInVision = false;


        [Serializable]
        public class Sensor
        {
            [Tooltip("Weight of the sensor's impact on steering.")]
            [HideInInspector] public float weight;
            public Transform sensorPoint;
            [Tooltip("Direction of the sensor.")]
            [HideInInspector] public float direction;
            [HideInInspector] public RaycastHit hit;
        }

        #endregion

        #region Unity Methods

        private void Start()
        {
            calculateSensorDirection();

            if (GetComponent<PathProgressTracker>() != null)
            {
                pathProgressTracker = GetComponent<PathProgressTracker>();
            }

            progressTransform = new GameObject("progressTransform").transform;
            progressTransform.parent = vehicleTransform;

            pathTarget = new GameObject("pathTarget").transform;
            pathTarget.parent = vehicleTransform;

        }

        private void Update()
        {
            LocalVehiclevelocity = vehicleTransform.InverseTransformDirection(vehicleRigidbody.velocity);

            SensorLogic();

            if (AiMode == Ai_Mode.TargetFollow)
            {
                TargetFollowLogic();
            }
            if (AiMode == Ai_Mode.PathFollow)
            {
                PathFollowLogic();
            }

            if (useReverseIfStuck)
            {
                //taking Reverse Logic
                TakingReverseIfStuckLogic();
            }

            HandleStartAndStop();
        }

        private void LateUpdate()
        {
            // If override is active, apply the override inputs
            if (isOverrideActive)
            {
                accelerationInputAi = overrideAccelerationInput;
                steerInputAi = overrideSteerInput;
                handBrakeInputAi = overrideHandBrakeInput;
            }

            // Apply the AI inputs
            if (accelerationInputAi > 0.1f)
            {
                accelerationInput = Mathf.Lerp(accelerationInput, accelerationInputAi, Time.deltaTime * InputLerpSpeed);
            }
            else
            {
                accelerationInput = accelerationInputAi;
            }
            steerInput = steerInputAi;
            handBrakeInput = handBrakeInputAi;
        }

        #endregion

        #region Input Methods

        // Method to override inputs
        public void OverrideInput(float _accelerationInput, float _steerInput, float _handBrakeInput)
        {
            overrideAccelerationInput = _accelerationInput;
            overrideHandBrakeInput = _handBrakeInput;
            overrideSteerInput = _steerInput;
            isOverrideActive = true;
        }

        // Method to reset input override
        public void ResetInputOverride()
        {
            overrideAccelerationInput = 0;
            overrideHandBrakeInput = 0;
            overrideSteerInput = 0;
            isOverrideActive = false;
        }

        public float GetAccelerationInput()
        {
            return accelerationInput;
        }

        public float GetSteerInput()
        {
            return steerInput;
        }

        public float GetHandBrakeInput()
        {
            return handBrakeInput;
        }

        #endregion

        #region Taking Reverse Logic

        bool stuckTimerExceeded = false;
        private void TakingReverseIfStuckLogic()
        {
            if (LocalVehiclevelocity.magnitude < 1f)
            {
                stuckTimer += Time.deltaTime;
            }
            else
            {
                stuckTimer = 0f;
            }

            if (stuckTimer > stuckTime)
            {
                stuckTimerExceeded = true;
            }

            if (stuckTimerExceeded)
            {
                reversingTimer += Time.deltaTime;
                if (reversingTimer < reversingTime)
                {
                    isTakingReverse = true;
                    accelerationInputAi = -1;
                    steerInputAi = 0;
                }
                else
                {
                    if (isTakingReverse)
                    {
                        if (LocalVehiclevelocity.z < 0)
                        {
                            accelerationInputAi = 1;
                            steerInputAi = 0;
                        }
                        else
                        {
                            reversingTimer = 0f;
                            stuckTimer = 0f;
                            isTakingReverse = false;
                            stuckTimerExceeded = false;
                        }
                    }
                }
            }
        }

        #endregion

        #region Start and Stop Logic

        private void HandleStartAndStop()
        {
            if (stopVehicle && canSteer)
            {
                if (LocalVehiclevelocity.z > 1)
                {
                    accelerationInputAi = -1;
                    handBrakeInputAi = 0;
                }
                else if (LocalVehiclevelocity.z < 1)
                {
                    accelerationInputAi = 0;
                    handBrakeInputAi = 1;
                }
                else
                {
                    accelerationInputAi = 0;
                    handBrakeInputAi = 1;
                }
                return;
            }
            else if (stopVehicle && !canSteer)
            {
                if (LocalVehiclevelocity.z > 1)
                {
                    accelerationInputAi = -1;
                    handBrakeInputAi = 0;
                    steerInputAi = 0;
                }
                else
                {
                    accelerationInputAi = 0;
                    handBrakeInputAi = 1;
                    steerInputAi = 0;
                }
                return;
            }
        }

        #endregion

        #region Target Follow Logic

        public void TargetFollowLogic()
        {
            // null check
            if (target == null)
            {
                Debug.LogError("target is missing on vehicle.");

                // reset input
                accelerationInputAi = 0;
                steerInputAi = 0;
                handBrakeInputAi = 0;

                return;
            }

            float targetDistance = Vector3.Distance(vehicleTransform.position, target.position);
            Vector3 targetDirection = (target.position - vehicleTransform.position).normalized;
            Vector3 relative_direction = vehicleTransform.InverseTransformDirection(targetDirection);

            IR = Vector3.ProjectOnPlane((target.position - (vehicleTransform.position + vehicleTransform.right * TurnRadius)), Vector3.up).magnitude < TurnRadius;
            IL = Vector3.ProjectOnPlane((target.position - (vehicleTransform.position - vehicleTransform.right * TurnRadius)), Vector3.up).magnitude < TurnRadius;
            FR = relative_direction.z > 0 && relative_direction.x > 0 && !IR && !IL;
            FL = relative_direction.z > 0 && relative_direction.x < 0 && !IR && !IL;
            RR = relative_direction.z < 0 && relative_direction.x > 0 && !IR && !IL;
            RL = relative_direction.z < 0 && relative_direction.x < 0 && !IR && !IL;

            float steerAmount = Mathf.Abs(Vector3.Cross(targetDirection, vehicleTransform.forward).y);


            if (targetDistance > stoppingDistance)
            {
                useReverseIfStuck = true;

                if (IR) { accelerationInputAi = -1; steerInputAi = -1; }
                if (IL) { accelerationInputAi = -1; steerInputAi = 1; }
                if (FR) { accelerationInputAi = 1; if (LocalVehiclevelocity.z > 0) { steerInputAi = steerAmount; } }
                if (FL) { accelerationInputAi = 1; if (LocalVehiclevelocity.z > 0) { steerInputAi = -steerAmount; } }
                if (RR) { accelerationInputAi = -1; if (LocalVehiclevelocity.z < 0) { steerInputAi = -1; } else { steerInputAi = 0; } }
                if (RL) { accelerationInputAi = -1; if (LocalVehiclevelocity.z < 0) { steerInputAi = 1; } else { steerInputAi = 0; } }

                CheckTargetInVision();

                handBrakeInputAi = 0;

                if (targetDistance < reverseDistance)
                {
                    if (RR) { accelerationInputAi = -1; steerInputAi = steerAmount; }
                    if (RL) { accelerationInputAi = -1; steerInputAi = -steerAmount; }
                }

                //if (targetDistance < slowDownDistance)
                //{
                //    accelerationInputAi = Mathf.Lerp(accelerationInputAi, 0, Time.deltaTime * 2);
                //}
                //
                //if (targetDistance < brakingDistance)
                //{
                //    handBrakeInputAi = Mathf.Lerp(handBrakeInputAi, 1, Time.deltaTime * 2);
                //    accelerationInputAi = 0;
                //}

                if (Vector3.Angle(vehicleTransform.forward, targetDirection) > 20)
                {
                    accelerationInputAi = Mathf.Lerp(accelerationInputAi, 0, Time.deltaTime * 2);
                }

                if (useSpeedBasedBraking)
                {
                    float decelerationBuffer = 2.0f; // Buffer around the deceleration rate

                    // Calculate deceleration to avoid overshooting with buffer
                    float deceleration = (LocalVehiclevelocity.z * LocalVehiclevelocity.z) / (2 * targetDistance);
                    if (deceleration > decelerationRate + decelerationBuffer)
                    {
                        accelerationInputAi = -1f;
                    }
                    else if (deceleration > decelerationRate - decelerationBuffer && deceleration < decelerationRate + decelerationBuffer)
                    {
                        accelerationInputAi = -0.5f;
                    }
                }


                // Sensor avoidance logic
                if (obstacleInPath && !isTargetInVision)
                {
                    if (RR) { accelerationInputAi = 1; steerInputAi = steerAmount; }
                    if (RL) { accelerationInputAi = 1; steerInputAi = -steerAmount; }

                    if (obstacleDistance < ObstacleTurnDistance && obstacleDistance < targetDistance)
                    {
                        Vector3 closestHitPoint = GetClosestSensorHitPointToCarProgress();
                        float obstacleSign = Mathf.Sign(Vector3.Dot((closestHitPoint - vehicleTransform.position).normalized, vehicleTransform.right));
                        float targetTurnSign = Mathf.Sign(Vector3.Dot(targetDirection, vehicleTransform.right));

                        if (isFrontSensorDetected && LocalVehiclevelocity.z > 0)
                        {
                            steerInputAi = -turnmultiplyer;
                        }
                        else if (LocalVehiclevelocity.z > 0 && Vector3.Dot(targetDirection, vehicleTransform.forward) > 0 && obstacleSign == targetTurnSign)
                        {
                            steerInputAi = -turnmultiplyer;
                        }
                    }

                    if (LocalVehiclevelocity.z > ObstacleSlowDownSpeed)
                    {
                        accelerationInputAi = -1;
                    }
                }

            }
            else
            {
                useReverseIfStuck = false;
                if (LocalVehiclevelocity.z > 1)
                {
                    accelerationInputAi = -1;
                }
                else if (LocalVehiclevelocity.z < -1)
                {
                    accelerationInputAi = 1;
                }
                else
                {
                    accelerationInputAi = 0f;
                }

                steerInputAi = 0f;
                handBrakeInputAi = 1f;
            }
        }

        #endregion

        #region Path Follow Logic

        public void PathFollowLogic()
        {
            // null check
            if (pathProgressTracker == null)
            { 
                Debug.LogError("pathProgressTracker component is missing on vehicle");

                // reset input
                accelerationInputAi = 0;
                steerInputAi = 0;
                handBrakeInputAi = 0;

                return;
            }

            A = pathProgressTracker.A;
            B = pathProgressTracker.B;
            C = pathProgressTracker.C;
            D = pathProgressTracker.D;


            float progressDistance = pathProgressTracker.progressDistance;
            float splineLength = pathProgressTracker.splineContainer.Splines[0].GetLength();

            // target placement
            pathTarget.position = A;
            Vector3 targetForward = pathProgressTracker.GetSplineTangent((progressDistance + pathProgressTracker.offset_A));
            Vector3 targetUp = Vector3.up;
            pathTarget.rotation = Quaternion.LookRotation(targetForward, targetUp);

            // progress transform placement
            progressTransform.position = pathProgressTracker.progressPoint;
            Vector3 progressForward = pathProgressTracker.GetSplineTangent(progressDistance);
            Vector3 progressUp = Vector3.up;
            progressTransform.rotation = Quaternion.LookRotation(progressForward, progressUp);


            float targetDistance = Vector3.Distance(vehicleTransform.position, pathTarget.position);
            Vector3 targetDirection = (pathTarget.position - vehicleTransform.position).normalized;
            Vector3 relative_direction = vehicleTransform.InverseTransformDirection(targetDirection);

            IR = Vector3.ProjectOnPlane((pathTarget.position - (vehicleTransform.position + vehicleTransform.right * TurnRadius)), Vector3.up).magnitude < TurnRadius;
            IL = Vector3.ProjectOnPlane((pathTarget.position - (vehicleTransform.position - vehicleTransform.right * TurnRadius)), Vector3.up).magnitude < TurnRadius;
            FR = relative_direction.z > 0 && relative_direction.x > 0 && !IR && !IL;
            FL = relative_direction.z > 0 && relative_direction.x < 0 && !IR && !IL;
            RR = relative_direction.z < 0 && relative_direction.x > 0 && !IR && !IL;
            RL = relative_direction.z < 0 && relative_direction.x < 0 && !IR && !IL;

            float steerAmount = Mathf.Abs(Vector3.Cross(targetDirection, vehicleTransform.forward).y);

            if (targetDistance > 0)
            {
                if (IR) { accelerationInputAi = -1; steerInputAi = -1; }
                if (IL) { accelerationInputAi = -1; steerInputAi = 1; }
                if (FR) { accelerationInputAi = 1; if (LocalVehiclevelocity.z > 0) { steerInputAi = steerAmount; } else { steerInputAi = 0; } }
                if (FL) { accelerationInputAi = 1; if (LocalVehiclevelocity.z > 0) { steerInputAi = -steerAmount; } else { steerInputAi = 0; } }
                if (RR) { accelerationInputAi = -1; if (LocalVehiclevelocity.z < 0) { steerInputAi = -1; } else { steerInputAi = 0; } }
                if (RL) { accelerationInputAi = -1; if (LocalVehiclevelocity.z < 0) { steerInputAi = 1; } else { steerInputAi = 0; } }

                handBrakeInputAi = 0;

                // Speed-based predictive braking for upcoming turns
                Vector3 dirVF = vehicleTransform.forward;
                Vector3 dirVA = (A - vehicleTransform.position).normalized;
                Vector3 dirAB = (B - A).normalized;
                Vector3 dirBC = (C - B).normalized;
                Vector3 dirCD = (D - C).normalized;

                float angleVF_VA = Vector3.Angle(dirVF, dirVA);
                float angleAB_BC = Vector3.Angle(dirAB, dirBC);
                float angleBC_CD = Vector3.Angle(dirBC, dirCD);


                // Estimate time to reach each segment's turn
                float distanceToBC = Vector3.Distance(vehicleTransform.position, B);
                float distanceToCD = Vector3.Distance(vehicleTransform.position, C);
                float timeToBC = distanceToBC / LocalVehiclevelocity.z;
                float timeToCD = distanceToCD / LocalVehiclevelocity.z;

                // Predictive braking logic
                float brakingThresholdTime = 1.5f; // Time threshold to start braking earlier

                if (FR || FL)
                {
                    if ((//(angleVF_VA > turnSlowDownAngle + bufferAngle && timeToBC < brakingThresholdTime) ||
                        (angleAB_BC > turnSlowDownAngle + bufferAngle && timeToBC < brakingThresholdTime) ||
                        (angleBC_CD > turnSlowDownAngle + bufferAngle && timeToCD < brakingThresholdTime)) &&
                        LocalVehiclevelocity.z > turnSlowDownSpeed + bufferSpeed)
                    {
                        // A sharp turn is coming soon, and the vehicle is approaching it quickly
                        accelerationInputAi = -1;
                    }
                    else if ((//(angleVF_VA > turnSlowDownAngle && angleVF_VA < turnSlowDownAngle + bufferAngle) ||
                        (angleAB_BC > turnSlowDownAngle && angleAB_BC < turnSlowDownAngle + bufferAngle) ||
                        (angleBC_CD > turnSlowDownAngle && angleBC_CD < turnSlowDownAngle + bufferAngle)) &&
                        (LocalVehiclevelocity.z > turnSlowDownSpeed && LocalVehiclevelocity.z < turnSlowDownSpeed + bufferSpeed))
                    {
                        accelerationInputAi = 0;
                    }


                    // for drifting
                    if (angleVF_VA > turnSlowDownAngle && LocalVehiclevelocity.z > turnSlowDownSpeed)
                    {
                        handBrakeInputAi = 1;
                    }
                }

                // Calculate deceleration to avoid overshooting with buffer

                if (useSpeedBasedBraking)
                {
                    float decelerationBuffer = 2.0f; // Buffer around the deceleration rate
                    float D_distance = Vector3.Distance(vehicleTransform.position, D);
                    float deceleration = (LocalVehiclevelocity.z * LocalVehiclevelocity.z) / (2 * D_distance);

                    if (deceleration > decelerationRate + decelerationBuffer)
                    {
                        accelerationInputAi = -1f;
                    }
                    else if (deceleration > decelerationRate - decelerationBuffer && deceleration < decelerationRate + decelerationBuffer)
                    {
                        accelerationInputAi = -0.5f;
                    }
                }


                if (obstacleInPath && LocalVehiclevelocity.z > ObstacleSlowDownSpeed)
                {
                    accelerationInputAi = -1;
                }

                if (obstacleInPath && obstacleDistance < ObstacleTurnDistance && LocalVehiclevelocity.z > 0) // use variable instead of this 25
                {
                    Vector3 closestHitPoint = GetClosestSensorHitPointToCarProgress();
                    float distFromProgress = Mathf.Abs(Vector3.Dot(closestHitPoint - pathProgressTracker.progressPoint, pathProgressTracker.progressRight));
                    float distFromTarget = Mathf.Abs(Vector3.Dot(closestHitPoint - pathTarget.position, pathTarget.right));
                    float carOffsetFromCircuit = Mathf.Abs(Vector3.Dot(pathProgressTracker.progressPoint - vehicleTransform.position, pathProgressTracker.progressRight));

                    if (carOffsetFromCircuit > roadWidth / 2)
                    {
                        //this is not working properly
                        pathTarget.position = pathProgressTracker.progressPoint;
                    }

                    float obstacleSign = Mathf.Sign(Vector3.Dot((closestHitPoint - vehicleTransform.position).normalized, vehicleTransform.right));
                    float targetTurnSign = Mathf.Sign(Vector3.Dot(targetDirection, vehicleTransform.right));

                    if (distFromProgress < roadWidth / 2 || distFromTarget < roadWidth / 2)
                    {
                        //steerInputAi = -turnmultiplyer;

                        if (isFrontSensorDetected && LocalVehiclevelocity.z > 0)
                        {
                            steerInputAi = -turnmultiplyer;
                        }
                        else if (LocalVehiclevelocity.z > 0 && Vector3.Dot(targetDirection, vehicleTransform.forward) > 0 && obstacleSign == targetTurnSign)
                        {
                            steerInputAi = -turnmultiplyer;
                        }
                    }
                    else if (Mathf.Abs(carOffsetFromCircuit - distFromProgress) < roadWidth / 2 || carOffsetFromCircuit > roadWidth / 2)
                    {
                        //steerInputAi = -turnmultiplyer;

                        if (isFrontSensorDetected && LocalVehiclevelocity.z > 0)
                        {
                            steerInputAi = -turnmultiplyer;
                        }
                        else if (LocalVehiclevelocity.z > 0 && Vector3.Dot(targetDirection, vehicleTransform.forward) > 0 && obstacleSign == targetTurnSign)
                        {
                            steerInputAi = -turnmultiplyer;
                        }
                    }

                }

            }
            else
            {
                if (LocalVehiclevelocity.z > 1)
                {
                    accelerationInputAi = -1;
                }
                else if (LocalVehiclevelocity.z < -1)
                {
                    accelerationInputAi = 1;
                }
                else
                {
                    accelerationInputAi = 0f;
                }

                steerInputAi = 0f;
                handBrakeInputAi = 1f;
            }
        }


        #endregion

        #region SensorLogic

        public void SensorLogic()
        {
            calculateSensorWeight();

            obstacleInPath = IsObstacleInPath();
            SensorTurnAmount = CalculateSensorValue();

            if (SensorTurnAmount == 0 && obstacleInPath)
            {
                turnmultiplyer = 0;
            }
            else
            {
                turnmultiplyer = Mathf.Sign(SensorTurnAmount);
            }
        }

        private void ProcessSensorWeights(Sensor[] sensors)
        {
            foreach (Sensor sensor in sensors)
            {
                if (Physics.Raycast(sensor.sensorPoint.position, sensor.sensorPoint.forward, out sensor.hit, sensorLength + LocalVehiclevelocity.magnitude, ObstacleLayers))
                {
                    sensor.weight = 1;
                    Debug.DrawLine(sensor.sensorPoint.position, sensor.hit.point, Color.red);
                }
                else
                {
                    sensor.weight = 0;
                }
            }
        }


        private void calculateSensorWeight()
        {
            ProcessSensorWeights(frontSensors);

            isFrontSensorDetected = false;
            foreach (Sensor sensor in frontSensors)
            {
                if (sensor.weight == 1)
                {
                    isFrontSensorDetected = true;
                }
            }

            ProcessSensorWeights(sideSensors);
        }


        private void calculateSensorDirection()
        {
            foreach (Sensor sensor in frontSensors)
            {
                if (sensor.sensorPoint.localPosition.x == 0)
                {
                    sensor.direction = 0;
                }
                else
                {
                    sensor.direction = sensor.sensorPoint.localPosition.x / Mathf.Abs(sensor.sensorPoint.localPosition.x);
                }
            }

            foreach (Sensor sensor in sideSensors)
            {
                if (sensor.sensorPoint.localPosition.x == 0)
                {
                    sensor.direction = 0;
                }
                else
                {
                    sensor.direction = sensor.sensorPoint.localPosition.x / Mathf.Abs(sensor.sensorPoint.localPosition.x);
                }
            }
        }

        private bool IsObstacleInPath()
        {
            bool isObstacleDetected = false;
            float closestDistance = float.MaxValue;

            foreach (Sensor sensor in frontSensors)
            {
                if (sensor.weight == 1)
                {
                    float distance = sensor.hit.distance;
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                    }
                    isObstacleDetected = true;
                }
            }

            foreach (Sensor sensor in sideSensors)
            {
                if (sensor.weight == 1)
                {
                    float distance = sensor.hit.distance;
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                    }
                    isObstacleDetected = true;
                }
            }

            obstacleDistance = closestDistance;
            return isObstacleDetected;
        }


        private float CalculateSensorValue()
        {
            float sensorValue = 0f;
            Sensor middleSensor = new Sensor();

            // Front sensors impact
            foreach (Sensor sensor in frontSensors)
            {
                sensorValue += sensor.weight * sensor.direction;


                // get middle sensor

                if (sensor.direction == 0)
                {
                    middleSensor = sensor;
                }
            }

            if (sensorValue == 0 && middleSensor.weight == 1)
            {
                ObstacleAngle = Vector3.Dot(middleSensor.hit.normal, transform.right);

                sensorValue = (ObstacleAngle > 0) ? -1 : 1;
            }

            return sensorValue;
        }

        public Vector3 GetClosestSensorHitPointToCarProgress()
        {
            Vector3 closestPoint = Vector3.zero;
            float closestDistance = Mathf.Infinity;

            foreach (Sensor sensor in frontSensors)
            {
                float distance = Vector3.Distance(pathProgressTracker.progressPoint, sensor.hit.point);

                if (distance < closestDistance && sensor.weight == 1)
                {
                    closestDistance = distance;
                    closestPoint = sensor.hit.point;
                }
            }

            foreach (Sensor sensor in sideSensors)
            {
                float distance = Vector3.Distance(pathProgressTracker.progressPoint, sensor.hit.point);

                if (distance < closestDistance && sensor.weight == 1)
                {
                    closestDistance = distance;
                    closestPoint = sensor.hit.point;
                }
            }

            return closestPoint;
        }


        private void CheckTargetInVision()
        {
            // Calculate direction and angle to the target
            Vector3 directionToTarget = (target.position - vehicleTransform.position).normalized;
            float visionDistance = Vector3.Distance(vehicleTransform.position, target.position);

            // Check if within vision angle and distance
            if (Vector3.Distance(vehicleTransform.position, target.position) <= visionDistance)
            {
                // Perform a raycast to check if there are obstacles between the AI and the target
                if (!Physics.Raycast(vehicleTransform.position, directionToTarget, out RaycastHit hit, visionDistance, ObstacleLayers))
                {
                    isTargetInVision = true;
                    return;
                }
            }

            isTargetInVision = false;
        }

        #endregion

        #region Utility methods

        [ContextMenu("Start Vehicle Movement")]
        public void StartVehicleMovement()
        {
            stopVehicle = false;
            canSteer = true;
        }

        [ContextMenu("Stop Vehicle Movement")]
        public void StopVehicleAndDisableSteering()
        {
            stopVehicle = true;
            canSteer = false;
        }

        [ContextMenu("Stop Vehicle Movement But Allow Steering")]
        public void StopVehicleButAllowSteering()
        {
            stopVehicle = true;
            canSteer = true;
        }

        public void SetTarget(Transform _target)
        {
            switchAiMode(Ai_Mode.TargetFollow);
            target = _target;
        }

        public void SetPath(SplineContainer splineContainer)
        {
            switchAiMode(Ai_Mode.PathFollow);
            pathProgressTracker.splineContainer = splineContainer;

            //reset progress
            pathProgressTracker.ResetProgress();
        }

        public void switchAiMode(Ai_Mode _aiMode)
        {
            AiMode = _aiMode;
        }

        public void DriveToDestination(Vector3 destination)
        {
            if (GetComponent<PathFinding>() == null)
            {
                Debug.LogError("pathfinding component is missing on vehicle");
                return;
            }

            switchAiMode(Ai_Mode.PathFollow);

            PathFinding pathFinding = GetComponent<PathFinding>();
            pathFinding.FindPath(destination);

            pathProgressTracker.ResetProgress();

            StartVehicleMovement();


        }

        #endregion

        #region Gizmos

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            UnityEditor.Handles.color = Color.cyan;
            Gizmos.color = Color.blue;

            if(vehicleTransform != null)
            {
                Vector3 rightCircleCenter = vehicleTransform.position + vehicleTransform.right * TurnRadius - vehicleTransform.forward * TurnRadiusOffset;
                Vector3 leftCircleCenter = vehicleTransform.position - vehicleTransform.right * TurnRadius - vehicleTransform.forward * TurnRadiusOffset;

                UnityEditor.Handles.DrawWireDisc(rightCircleCenter, vehicleTransform.up, TurnRadius);
                Gizmos.DrawLine(vehicleTransform.position - vehicleTransform.forward * TurnRadiusOffset, rightCircleCenter);
                Gizmos.DrawSphere(rightCircleCenter, 0.2f);

                UnityEditor.Handles.DrawWireDisc(leftCircleCenter, vehicleTransform.up, TurnRadius);
                Gizmos.DrawLine(vehicleTransform.position - vehicleTransform.forward * TurnRadiusOffset, leftCircleCenter);
                Gizmos.DrawSphere(leftCircleCenter, 0.2f);
            }

            if (AiMode == Ai_Mode.TargetFollow)
            {
                Gizmos.color = Color.red;
                if (target != null) { Gizmos.DrawWireSphere(target.position, stoppingDistance); }
                //Gizmos.color = Color.yellow;
                //Gizmos.DrawWireSphere(target.position, slowDownDistance);
                //Gizmos.color = Color.white;
                //Gizmos.DrawWireSphere(target.position, brakingDistance);
                Gizmos.color = new Color(0, 0, 1, 0.3f);
                if (target != null) { Gizmos.DrawSphere(target.position, reverseDistance); }
            }

            if(frontSensors != null)
            {
                Gizmos.color = Color.green;
                foreach (Sensor sensor_ in frontSensors)
                {
                    if (sensor_.sensorPoint != null && sensor_.weight == 0)
                    {
                        Gizmos.DrawRay(sensor_.sensorPoint.position, sensor_.sensorPoint.forward * (sensorLength + LocalVehiclevelocity.magnitude));
                    }
                }
            }
            
            if(sideSensors != null)
            {
                foreach (Sensor sensor_ in sideSensors)
                {
                    if (sensor_.sensorPoint != null && sensor_.weight == 0)
                    {
                        Gizmos.DrawRay(sensor_.sensorPoint.position, sensor_.sensorPoint.forward * (sensorLength + LocalVehiclevelocity.magnitude));
                    }
                }
            }
            
            if (AiMode == Ai_Mode.PathFollow)
            {
                Gizmos.color = Color.magenta;
                if (Application.isPlaying)
                {
                    Gizmos.matrix = progressTransform.localToWorldMatrix;
                    Gizmos.DrawWireCube(Vector3.zero, new Vector3(roadWidth, 0.1f, roadWidth));
                }
                else
                {
                    if(vehicleTransform != null) Gizmos.DrawWireCube(vehicleTransform.position, (new Vector3(roadWidth, 0.1f, roadWidth)));
                }
            }

        }

#endif

        #endregion

    }
}
