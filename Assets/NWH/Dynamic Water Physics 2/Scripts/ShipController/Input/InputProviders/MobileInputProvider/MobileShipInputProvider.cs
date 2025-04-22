using NWH.Common.Input;
using UnityEngine.UI;

#if UNITY_EDITOR
using NWH.DWP2.NUI;
using NWH.DWP2.ShipController;
using UnityEditor;
#endif

namespace NWH.DWP2.ShipController
{
    /// <summary>
    ///     Class for handling mobile user input via touch screen and sensors.
    /// </summary>
    public class MobileShipInputProvider : ShipInputProvider
    {
        // Ship
        public Slider            steeringSlider;
        public Slider            throttleSlider;
        public MobileInputButton engineStartStopButton;
        public MobileInputButton anchorButton;

        // Camera
        public MobileInputButton changeCameraButton;

        // Scene
        public MobileInputButton changeShipButton;


        public override float Steering()
        {
            if (steeringSlider != null)
            {
                return steeringSlider.value;
            }

            return 0;
        }


        public override float Throttle()
        {
            if (throttleSlider != null)
            {
                return throttleSlider.value;
            }

            return 0;
        }

        public override bool EngineStartStop()
        {
            if (engineStartStopButton != null)
            {
                return engineStartStopButton.hasBeenClicked;
            }

            return false;
        }


        public override bool Anchor()
        {
            if (anchorButton != null)
            {
                return anchorButton.hasBeenClicked;
            }

            return false;
        }
    }
}


#if UNITY_EDITOR
namespace NWH.DWP2.WaterObjects
{
    /// <summary>
    ///     Editor for MobileInputProvider.
    /// </summary>
    [CustomEditor(typeof(MobileShipInputProvider))]
    public class MobileShipInputProviderEditor : DWP_NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            drawer.Info("None of the buttons are mandatory. If you do not wish to use an input leave the field empty.");

            MobileShipInputProvider mip = target as MobileShipInputProvider;
            if (mip == null)
            {
                drawer.EndEditor(this);
                return false;
            }

            drawer.BeginSubsection("Vehicle");
            drawer.Field("steeringSlider");
            drawer.Field("throttleSlider");
            drawer.Field("engineStartStopButton");
            drawer.Field("anchorButton");
            drawer.EndSubsection();

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
