using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ShipAI
{
    public class ShipInputProvider : MonoBehaviour
    {
        public AIShipController aiShipController;

        public enum InputType {  Ai, Player };
        public InputType inputType;

        public float AccelerationInput { get; private set; }
        public float SteerInput { get; private set; }
        public float HandbrakeInput { get; private set; }
        
        public Slider ThrottleSlider;
        public Slider RudderSlider;

        private void Update()
        {
            if (inputType == InputType.Player)
            {
                //无操作
            }
            else if(inputType == InputType.Ai)
            {
                ProvideAiInput();
            }
        }
            
        //控制逻辑
        private void ProvideAiInput()
        {
            // Get inputs
            SteerInput = aiShipController.GetSteerInput();
            AccelerationInput = aiShipController.GetAccelerationInput();
            HandbrakeInput = aiShipController.GetHandBrakeInput();

            // set inputs
            ApplySteering();
            ApplyAcceleration();
            ApplyHandbrake();
        }
        
        //转向
        void ApplySteering()
        {
            RudderSlider.value = SteerInput;
        }
        
        //加速
        void ApplyAcceleration()
        {
            ThrottleSlider.value = AccelerationInput;
        }

        void ApplyHandbrake()
        {
            if (HandbrakeInput > 0f)
            {
                //刹车
            }
            else
            {
            }
        }
    }
}