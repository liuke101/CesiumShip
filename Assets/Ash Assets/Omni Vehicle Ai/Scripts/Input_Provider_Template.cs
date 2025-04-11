using UnityEngine;

namespace OmniVehicleAi
{
    public class Input_Provider_Template : MonoBehaviour
    {
        // Reference to the user's custom vehicle controller
        public MonoBehaviour vehicleController; // Change this to the actual vehicle controller type in your project
        public AIVehicleController aiVehicleController; // AI input provider

        // Enum to switch between Player and AI input modes
        public enum InputType { Player, Ai };
        public InputType inputType; // Control mode (Player or AI)

        // Variables to hold input values
        public float AccelerationInput { get; private set; }
        public float SteerInput { get; private set; }
        public float HandbrakeInput { get; private set; }

        private void Update()
        {
            // Switch between input types based on the selected input type
            if (inputType == InputType.Player)
            {
                ProvidePlayerInput(); // If player-controlled, get player inputs
            }
            else
            {
                ProvideAiInput(); // If AI-controlled, get AI inputs
            }
        }

        // Function to provide AI inputs to the vehicle controller
        private void ProvideAiInput()
        {
            // Get AI inputs from AI vehicle controller
            SteerInput = aiVehicleController.GetSteerInput();
            AccelerationInput = aiVehicleController.GetAccelerationInput();
            HandbrakeInput = aiVehicleController.GetHandBrakeInput();

            // Provide these inputs to the user's custom vehicle controller
            // This assumes the user's vehicle controller has its own method to handle inputs
            // Replace the following line with the appropriate method from your vehicle controller
            // Example:
            // vehicleController.ProvideInputs(AccelerationInput, SteerInput, HandbrakeInput);
        }

        // Function to provide Player inputs to the vehicle controller
        private void ProvidePlayerInput()
        {
            // Get player inputs
            //example :-
            //AccelerationInput = Input.GetAxis("Vertical"); // Forward/backward movement
            //SteerInput = Input.GetAxis("Horizontal"); // Left/right steering
            //HandbrakeInput = Input.GetButton("Jump") ? 1f : 0f; // Handbrake input

            // Provide these inputs to the user's custom vehicle controller
            // Replace the following line with the appropriate method from your vehicle controller
            // Example:
            // vehicleController.ProvideInputs(AccelerationInput, SteerInput, HandbrakeInput);
        }
    }
}
