using UnityEngine;

namespace OmniVehicleAi
{
    public class Demo_Input_Provider_Generated : MonoBehaviour
    {
        public DemoVehicleController vehicleController;
        public AIVehicleController aiVehicleController;

        public enum InputType { Player, Ai };
        public InputType inputType;

        public float AccelerationInput { get; private set; }
        public float SteerInput { get; private set; }
        public float HandbrakeInput { get; private set; }

        private void Update()
        {
            if (inputType == InputType.Player)
            {
                ProvidePlayerInput();
            }
            else
            {
                ProvideAiInput();
            }
        }

        private void ProvideAiInput()
        {
            SteerInput = aiVehicleController.GetSteerInput();
            AccelerationInput = aiVehicleController.GetAccelerationInput();
            HandbrakeInput = aiVehicleController.GetHandBrakeInput();
            vehicleController.ProvideInputs(
                AccelerationInput,
                SteerInput,
                HandbrakeInput
            );
        }
        private void ProvidePlayerInput()
        {
            // Example Player inputs:
            // AccelerationInput = Input.GetAxis("Vertical");
            // SteerInput = Input.GetAxis("Horizontal");
            // HandbrakeInput = Input.GetButton("Jump") ? 1f : 0f;
        }
    }
}
