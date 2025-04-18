using System;
using NWH.Common.Input;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using NWH.DWP2.NUI;
using NWH.DWP2.ShipController;
using UnityEditor;
#endif

namespace NWH.DWP2.ShipController
{
    /// <summary>
    ///     Manages vehicle input by retrieving it from the active InputProvider and filling in the InputStates with the
    ///     fetched data.
    /// </summary>
    [Serializable]
    public class ShipInputHandler
    {
        /// <summary>
        ///     When enabled input will be auto-retrieved from the InputProviders present in the scene.
        ///     Disable to manualy set the input through external scripts, i.e. AI controller.
        /// </summary>
        public bool autoSetInput = true;

        /// <summary>
        ///     All the input states of the vehicle. Can be used to set input through scripting or copy the inputs
        ///     over from other vehicle, such as truck to trailer.
        /// </summary>
        public ShipInputStates states;

        public UnityEvent modifyInputCallback = new UnityEvent();

        public float Throttle
        {
            get { return states.throttle; }
            set { states.throttle = Mathf.Clamp(value, -1f, 1f); }
        }
        
        public float Throttle2
        {
            get { return states.throttle2; }
            set { states.throttle2 = Mathf.Clamp(value, -1f, 1f); }
        }
        
        public float Throttle3
        {
            get { return states.throttle3; }
            set { states.throttle3 = Mathf.Clamp(value, -1f, 1f); }
        }
        
        public float Throttle4
        {
            get { return states.throttle4; }
            set { states.throttle4 = Mathf.Clamp(value, -1f, 1f); }
        }

        public float Steering
        {
            get { return states.steering; }
            set { states.steering = Mathf.Clamp(value, -1f, 1f); }
        }

        public float SternThruster
        {
            get { return states.sternThruster; }
            set { states.sternThruster = Mathf.Clamp(value, -1f, 1f); }
        }

        public float BowThruster
        {
            get { return states.bowThruster; }
            set { states.bowThruster = Mathf.Clamp(value, -1f, 1f); }
        }

        public float SubmarineDepth
        {
            get { return states.submarineDepth; }
            set { states.submarineDepth = Mathf.Clamp01(value); }
        }

        public bool Anchor
        {
            get { return states.anchor; }
            set { states.anchor = value; }
        }

        public bool EngineStartStop
        {
            get { return states.engineStartStop; }
            set { states.engineStartStop = value; }
        }


        public float RotateSail
        {
            get { return states.rotateSail; }
            set { states.rotateSail = value; }
        }


        public void Update()
        {
            if (!autoSetInput)
            {
                return;
            }

            Steering       = InputProvider.CombinedInput<ShipInputProvider>(i => i.Steering());
            Throttle       = InputProvider.CombinedInput<ShipInputProvider>(i => i.Throttle());
            Throttle2      = InputProvider.CombinedInput<ShipInputProvider>(i => i.Throttle2());
            Throttle3      = InputProvider.CombinedInput<ShipInputProvider>(i => i.Throttle3());
            Throttle4      = InputProvider.CombinedInput<ShipInputProvider>(i => i.Throttle4());
            BowThruster    = InputProvider.CombinedInput<ShipInputProvider>(i => i.BowThruster());
            SternThruster  = InputProvider.CombinedInput<ShipInputProvider>(i => i.SternThruster());
            SubmarineDepth = InputProvider.CombinedInput<ShipInputProvider>(i => i.SubmarineDepth());
            RotateSail       = InputProvider.CombinedInput<ShipInputProvider>(i => i.RotateSail());
            EngineStartStop |= InputProvider.CombinedInput<ShipInputProvider>(i => i.EngineStartStop());
            Anchor          |= InputProvider.CombinedInput<ShipInputProvider>(i => i.Anchor());
            
            modifyInputCallback.Invoke();
        }
    }
}


#if UNITY_EDITOR
namespace NWH.DWP2.WaterObjects
{
    /// <summary>
    ///     Property drawer for Input.
    /// </summary>
    [CustomPropertyDrawer(typeof(ShipInputHandler))]
    public class ShipInputHandlerDrawer : DWP_NUIPropertyDrawer
    {
        public override bool OnNUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!base.OnNUI(position, property, label))
            {
                return false;
            }

            drawer.Field("autoSetInput");
            drawer.Field("states");

            drawer.EndProperty();
            return true;
        }
    }
}

#endif
