using UnityEngine;

namespace OmniVehicleAi
{
    public class DemoInputProvider : MonoBehaviour
    {
        public DemoVehicleController demoVehicleController; // reference of vehicle controller
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
            // Get inputs
            SteerInput = aiVehicleController.GetSteerInput();
            AccelerationInput = aiVehicleController.GetAccelerationInput();
            HandbrakeInput = aiVehicleController.GetHandBrakeInput();

            // set inputs
            demoVehicleController.ProvideInputs(AccelerationInput, SteerInput, HandbrakeInput);
        }

        private void ProvidePlayerInput()
        {
            // Get inputs
            AccelerationInput = Input.GetAxis("Vertical");
            SteerInput = Input.GetAxis("Horizontal");
            HandbrakeInput = Input.GetButton("Jump") ? 1f : 0f;

            // set inputs
            demoVehicleController.ProvideInputs(AccelerationInput, SteerInput, HandbrakeInput);
        }
    }
}
