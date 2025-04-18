#if UNITY_EDITOR
using NWH.DWP2.NUI;
using NWH.DWP2.ShipController;
using UnityEditor;

namespace NWH.DWP2.WaterObjects
{
    [CustomEditor(typeof(Submarine))]
    [CanEditMultipleObjects]
    public class SubmarineEditor : DWP_NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }
            
            drawer.Info("To make submarine surface faster lower the Rigidbody mass.\n " +
                        "To make submarine dive faster increase the 'maxMassFactor'.");
            drawer.Field("maxBallastMass");
            drawer.Field("ballastChangeSpeed");
            drawer.Info("Too low 'maxBallastMass' value will prevent the submarine from diving.");
            drawer.EndSubsection();

            drawer.BeginSubsection("Keep Horizontal");
            drawer.Field("keepHorizontal");
            drawer.Field("keepHorizontalSensitivity");
            drawer.Field("maxMassOffset");
            drawer.Info("Max Mass Offset [m] should not be larger than ~1/3 of the length of the submarine.");
            drawer.EndSubsection();

            drawer.EndEditor(this);
            return true;
        }
    }
}

#endif
