using UnityEngine;

#if UNITY_EDITOR
using NWH.DWP2.NUI;
using NWH.DWP2.ShipController;
using UnityEditor;
#endif


namespace NWH.DWP2.ShipController
{
    /// <summary>
    ///     Class for handling input through new InputSystem
    /// </summary>
    public class InputSystemShipInputProvider : ShipInputProvider
    {
        public ShipInputActions shipInputActions;

        private float _steering;
        private float _throttle;
        private float _rotateSail;
        

        public new void Awake()
        {
            base.Awake();
            shipInputActions = new ShipInputActions();
            shipInputActions.Enable();
        }


        public void Update()
        {
            _steering       = shipInputActions.ShipControls.Steering.ReadValue<float>();
            _throttle       = shipInputActions.ShipControls.Throttle.ReadValue<float>();
            _rotateSail   = shipInputActions.ShipControls.RotateSail.ReadValue<float>();
        }


        // Ship bindings
        public override float Throttle()
        {
            return _throttle;
        }
        
        public override float Steering()
        {
            return _steering;
        }

        public override bool EngineStartStop()
        {
            return shipInputActions.ShipControls.EngineStartStop.triggered;
        }


        public override bool Anchor()
        {
            return shipInputActions.ShipControls.Anchor.triggered;
        }


        public override float RotateSail()
        {
            return _rotateSail;
        }


        public override Vector2 DragObjectPosition()
        {
            return Vector2.zero;
        }


        public override bool DragObjectModifier()
        {
            return false;
        }
    }
}


#if UNITY_EDITOR
namespace NWH.DWP2.WaterObjects
{
    [CustomEditor(typeof(InputSystemShipInputProvider))]
    public class InputSystemShipInputProviderEditor : DWP_NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            drawer.Info("Input settings for Unity's new input system can be changed by modifying 'ShipInputActions' " +
                        "file (double click on it to open).");

            drawer.EndEditor(this);
            return true;
        }


        public override bool UseDefaultMargins()
        {
            return false;
        }
    }
}

#endif
