using System;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using NWH.DWP2.NUI;
using NWH.DWP2.ShipController;
using UnityEditor;
#endif

namespace NWH.DWP2.ShipController
{
    /// <summary>
    ///     Struct for storing ship input
    /// </summary>
    [Serializable]
    public struct ShipInputStates
    {
        [Range(-1, 1)]
        public float steering;

        [Range(-1, 1)]
        public float throttle;
        
        [Range(-1, 1)]
        public float throttle2;
        
        [Range(-1, 1)]
        public float throttle3;
        
        [Range(-1, 1)]
        public float throttle4;

        [Range(-1, 1)]
        public float sternThruster;

        [Range(-1, 1)]
        public float bowThruster;

        [Range(0, 1)]
        public float submarineDepth;

        [Range(-1, 1)]
        public float rotateSail;

        public bool engineStartStop;
        public bool anchor;

        public bool changeShip;
        public bool changeCamera;


        public void Reset()
        {
            throttle        = 0;
            throttle2        = 0;
            throttle3        = 0;
            throttle4        = 0;
            sternThruster   = 0;
            bowThruster     = 0;
            submarineDepth  = 0;
            engineStartStop = false;
            anchor          = false;
        }
    }
}


#if UNITY_EDITOR
namespace NWH.DWP2.WaterObjects
{
    /// <summary>
    ///     Property drawer for InputStates.
    /// </summary>
    [CustomPropertyDrawer(typeof(ShipInputStates))]
    public class ShipInputStatesDrawer : DWP_NUIPropertyDrawer
    {
        public override bool OnNUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!base.OnNUI(position, property, label))
            {
                return false;
            }

            drawer.Field("steering");
            drawer.Field("throttle");
            drawer.Field("throttle2");
            drawer.Field("throttle3");
            drawer.Field("throttle4");
            drawer.Field("bowThruster");
            drawer.Field("sternThruster");
            drawer.Field("submarineDepth");
            drawer.Field("rotateSail");
            drawer.Field("engineStartStop");
            drawer.Field("anchor");
            EditorGUI.EndDisabledGroup();

            drawer.EndProperty();
            return true;
        }
    }
}

#endif
