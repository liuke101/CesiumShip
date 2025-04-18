#if UNITY_EDITOR
using NWH.DWP2.NUI;
using NWH.DWP2.ShipController;
using UnityEditor;
using UnityEngine;

namespace NWH.DWP2.WaterObjects
{
    [CustomEditor(typeof(Anchor))]
    [CanEditMultipleObjects]
    public class AnchorEditor : DWP_NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            Anchor anchor = (Anchor)target;

            if (Application.isPlaying)
            {
                drawer.Label($"Dropped: {anchor.Dropped}");
            }

            
            drawer.Field("dropOnStart");
            drawer.Field("forceCoefficient");
            drawer.Field("zeroForceRadius");
            drawer.Field("dragForce");
            drawer.Field("localAnchorPoint");

            drawer.EndEditor(this);
            return true;
        }
    }
}

#endif
