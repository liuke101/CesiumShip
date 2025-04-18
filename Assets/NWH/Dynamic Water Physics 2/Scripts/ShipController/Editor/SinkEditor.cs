#if UNITY_EDITOR
using NWH.DWP2.NUI;
using NWH.DWP2.ShipController;
using UnityEditor;

namespace NWH.DWP2.WaterObjects
{
    [CustomEditor(typeof(Sink))]
    [CanEditMultipleObjects]
    public class SinkEditor : DWP_NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            Sink sink = (Sink)target;
            
            drawer.Info("Position of the Transform to which this component is attached to " +
                        "roughly represents the sink direction / water ingress point.");
            drawer.Field("addedMassPercentPerSecond");
            drawer.Field("maxAdditionalMass");

            if (drawer.Button("Start Sinking"))
            {
                sink.StartSinking();
            }

            if (drawer.Button("Stop Sinking"))
            {
                sink.StopSinking();
            }

            if (drawer.Button("Reset"))
            {
                sink.ResetMass();
            }

            drawer.EndEditor(this);
            return true;
        }
    }
}

#endif
