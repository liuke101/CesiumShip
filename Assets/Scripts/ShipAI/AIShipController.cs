using System;
using NWH.Common.SceneManagement;
using OmniVehicleAi;
using UnityEngine;
using UnityEngine.Splines;

namespace ShipAI
{
    //https://soft-pilot-e91.notion.site/AIVehicleController-11d54a67048d8007a824c16982ae87e4
    public class AIShipController : MonoBehaviour
    {
        #region Variables
        public enum Ai_Mode { TargetFollow};

        public Ai_Mode AiMode;
        [Tooltip("Speed at which AI input is interpolated.")]
        public float InputLerpSpeed = 5f;

        [Header("Vehicle References")]
        public Rigidbody vehicleRigidbody;
        public Transform vehicleTransform;
        public Transform target;

        [Header("目标跟随设置")]
        [Tooltip("停止前到目标的最小距离")]
        public float stoppingDistance = 1f;
        [Tooltip("触发倒车的距离")]
        public float reverseDistance = 15f;

        [Header("倒车设置")]
        [Tooltip("车辆被认为卡住前的时间")]
        public float stuckTime = 1.5f;
        [Tooltip("卡住后倒车的时间")]
        public float reversingTime = 1.5f;

        private float stuckTimer = 0f;
        private float reversingTimer = 0f;
        
        [Header("AI输入设置")]
        private float accelerationInput;
        private float steerInput;
        private float handBrakeInput;
        [Tooltip("AI-controlled acceleration input.")]
        private float accelerationInputAi = 0f;
        [Tooltip("AI-controlled steering input.")]
        private float steerInputAi = 0f;
        [Tooltip("AI-controlled handbrake input.")]
        private float handBrakeInputAi = 0f;

        [Header("速度设置")]
        [Tooltip("减速速度")]
        public float decelerationRate = 10f;
        [Tooltip("决定车辆是否应该根据其速度刹车以完全停在目的地")]
        public bool useSpeedBasedBraking = true;

        [Header("转向设置")]
        [Tooltip("根据车辆尺寸和转向角度计算转弯半径。")]
        public float TurnRadius = 5f;
        [Tooltip("Turn radius offset in forward direction of the vehicle for gizmos")]
        public float TurnRadiusOffset = 0f;


        [Header("障碍躲避设置")]
        [Tooltip("当检测到障碍物时车辆减速的速度")]
        public float ObstacleSlowDownSpeed = 30f;
        [Tooltip("车辆为避开障碍物而开始转弯的距离")]
        public float ObstacleTurnDistance = 25f;

        [Header("传感器设置")]
        public Sensor[] frontSensors;
        public Sensor[] sideSensors;
        [Tooltip("用于探测障碍物的传感器长度")]
        public float sensorLength;
        [Tooltip("障碍物的Layer mask")]
        public LayerMask ObstacleLayers;
        [Tooltip("用于计算偏离轨道情况的道路宽度")]
        public float roadWidth;

        [HideInInspector] public Vector3 LocalVehiclevelocity;
        [HideInInspector] public float turnmultiplyer;
        [HideInInspector] public float SensorTurnAmount;
        [HideInInspector] public bool obstacleInPath;
        [HideInInspector] public float ObstacleAngle;
        private float obstacleDistance;
        [HideInInspector] public Transform progressTransform { get; private set; }
        [HideInInspector] public Transform pathTarget { get; private set; }

        private bool FL, FR, RL, RR, IL, IR;
        private bool isTakingReverse = false;
        private bool stopVehicle = false, canSteer = true;

        private bool isFrontSensorDetected = false;
        private bool useReverseIfStuck = true;

        private bool isTargetInVision = false;


        [Serializable]
        public class Sensor
        {
            [Tooltip("传感器对转向的影响权重")]
            [HideInInspector] public float weight;
            public Transform sensorPoint;
            [Tooltip("传感器方向")]
            [HideInInspector] public float direction;
            [HideInInspector] public RaycastHit hit;
        }

        #endregion

        #region Unity Methods
        private void Start()
        {
            // var InitialShip = VehicleChanger.Instance.vehicles[VehicleChanger.Instance.activeVehicleIndex];
            // if (InitialShip)
            // {
            //     vehicleRigidbody = InitialShip.GetComponent<Rigidbody>();
            //     vehicleTransform = InitialShip.transform;
            // }
            
            calculateSensorDirection();
            
            progressTransform = new GameObject("progressTransform").transform;
            progressTransform.parent = vehicleTransform;

            pathTarget = new GameObject("pathTarget").transform;
            pathTarget.parent = vehicleTransform;

        }

        private void Update()
        {
            LocalVehiclevelocity = vehicleTransform.InverseTransformDirection(vehicleRigidbody.velocity);
            
            //传感器
            SensorLogic();
            
            //目标跟随
            if (AiMode == Ai_Mode.TargetFollow)
            {
                TargetFollowLogic();
            }
            
            //倒车逻辑
            if (useReverseIfStuck)
            {
                TakingReverseIfStuckLogic();
            }
            
            //刹车
            HandleStartAndStop();
        }

        private void LateUpdate()
        {
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
            Vector3 relative_direction = vehicleTransform.InverseTransformDirection(targetDirection); //将目标方向转换为车辆的局部坐标系方向
            
            //根据目标相对于车辆的位置，判断目标是否在以下区域：
            // IR/IL: 目标在车辆右/左转弯半径内。
            // FR/FL: 目标在车辆前方右/左侧。
            // RR/RL: 目标在车辆后方右/左侧。
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
                float distance = Vector3.Distance(pathTarget.position, sensor.hit.point);
                if (distance < closestDistance && sensor.weight == 1)
                {
                    closestDistance = distance;
                    closestPoint = sensor.hit.point;
                }
            }

            foreach (Sensor sensor in sideSensors)
            {
                float distance = Vector3.Distance(pathTarget.position, sensor.hit.point);

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

        public void switchAiMode(Ai_Mode _aiMode)
        {
            AiMode = _aiMode;
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
                // 1. 绘制转弯半径的圆形
                // 右侧和左侧的转弯圆心
                Vector3 rightCircleCenter = vehicleTransform.position + vehicleTransform.right * TurnRadius - vehicleTransform.forward * TurnRadiusOffset;
                Vector3 leftCircleCenter = vehicleTransform.position - vehicleTransform.right * TurnRadius - vehicleTransform.forward * TurnRadiusOffset;
                // 绘制圆形, 连接车辆位置与圆心，并在圆心处绘制一个小球。
                UnityEditor.Handles.DrawWireDisc(rightCircleCenter, vehicleTransform.up, TurnRadius);
                Gizmos.DrawLine(vehicleTransform.position - vehicleTransform.forward * TurnRadiusOffset, rightCircleCenter);
                Gizmos.DrawSphere(rightCircleCenter, 0.2f);

                UnityEditor.Handles.DrawWireDisc(leftCircleCenter, vehicleTransform.up, TurnRadius);
                Gizmos.DrawLine(vehicleTransform.position - vehicleTransform.forward * TurnRadiusOffset, leftCircleCenter);
                Gizmos.DrawSphere(leftCircleCenter, 0.2f);
            }
            
            // 2. 目标跟随模式的可视
            if (AiMode == Ai_Mode.TargetFollow)
            {
                // 绘制目标位置的停止距离（红色圆）
                Gizmos.color = Color.red;
                if (target != null) { Gizmos.DrawWireSphere(target.position, stoppingDistance); }
                //Gizmos.color = Color.yellow;
                //Gizmos.DrawWireSphere(target.position, slowDownDistance);
                //Gizmos.color = Color.white;
                //Gizmos.DrawWireSphere(target.position, brakingDistance);
                
                // 绘制目标位置的倒车距离（半透明蓝色球体）
                Gizmos.color = new Color(0, 0, 1, 0.3f);
                if (target != null) { Gizmos.DrawSphere(target.position, reverseDistance); }
            }
            
            // 3. 传感器的可视化
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
        }

#endif

        #endregion

    }
}
