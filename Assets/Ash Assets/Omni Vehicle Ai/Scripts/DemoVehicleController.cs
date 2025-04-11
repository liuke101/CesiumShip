using UnityEngine;

namespace OmniVehicleAi
{
    public class DemoVehicleController : MonoBehaviour
    {
        public WheelCollider frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel;
        public Transform frontLeftTransform, frontRightTransform, rearLeftTransform, rearRightTransform;
        public float maxMotorTorque = 1500f;
        public float maxSteeringAngle = 30f;
        public float brakeTorque = 3000f;

        public float accelerationInput;
        public float steeringInput;
        public float handbrakeInput;

        void Update()
        {
            // Apply inputs to wheels
            ApplySteering();
            ApplyAcceleration();
            ApplyHandbrake();
            UpdateWheelVisuals();
        }

        public void ProvideInputs(float _accelerationInput, float _steerInput, float _handbrakeInput)
        {
            accelerationInput = _accelerationInput;
            steeringInput = _steerInput;
            handbrakeInput = _handbrakeInput;
        }

        public void SetAccelerationInput(float _accelerationInput)
        {
            accelerationInput = _accelerationInput;
        }

        public void SetSteeringInput(float _steeringInput)
        {
            steeringInput = _steeringInput;
        }

        public void SetHandbrakeInput(float _handbrakeInput)
        {
            handbrakeInput = _handbrakeInput;
        }


        void ApplySteering()
        {
            float steeringAngle = steeringInput * maxSteeringAngle;
            frontLeftWheel.steerAngle = steeringAngle;
            frontRightWheel.steerAngle = steeringAngle;
        }

        void ApplyAcceleration()
        {
            frontLeftWheel.motorTorque = accelerationInput * maxMotorTorque;
            frontRightWheel.motorTorque = accelerationInput * maxMotorTorque;
            rearLeftWheel.motorTorque = accelerationInput * maxMotorTorque;
            rearRightWheel.motorTorque = accelerationInput * maxMotorTorque;
        }

        void ApplyHandbrake()
        {
            if (handbrakeInput > 0f)
            {
                rearLeftWheel.brakeTorque = brakeTorque;
                rearRightWheel.brakeTorque = brakeTorque;
            }
            else
            {
                rearLeftWheel.brakeTorque = 0f;
                rearRightWheel.brakeTorque = 0f;
            }
        }

        void UpdateWheelVisuals()
        {
            UpdateWheelPose(frontLeftWheel, frontLeftTransform);
            UpdateWheelPose(frontRightWheel, frontRightTransform);
            UpdateWheelPose(rearLeftWheel, rearLeftTransform);
            UpdateWheelPose(rearRightWheel, rearRightTransform);
        }

        void UpdateWheelPose(WheelCollider wheelCollider, Transform wheelTransform)
        {
            Vector3 pos;
            Quaternion quat;
            wheelCollider.GetWorldPose(out pos, out quat);
            wheelTransform.position = pos;
            wheelTransform.rotation = quat;
        }
    }
}
