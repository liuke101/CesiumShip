using NWH.Common.Input;
using NWH.DWP2.ShipController;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NWH.Common.Vehicles;
using TMPro;

namespace ShipUI
{
    public class ShipGUIHandler : MonoBehaviour
    {
        public TMP_Text  speedText;
        public TMP_Text  rudderText;
        public TMP_Text PitchText;
        public TMP_Text YawText;
        public TMP_Text RollText;

        private AdvancedShipController activeShip;

        private void Update()
        {
            activeShip = Vehicle.ActiveVehicle as AdvancedShipController;
            if (activeShip != null)
            {
                float speed = activeShip.SpeedKnots;
                speedText.text = "航速：" + $"{speed:0.0}" + "kts";

                if (activeShip.rudders.Count > 0)
                {
                    float rudderAngle = activeShip.rudders[0].Angle;
                    rudderText.text = "舵角：" + $"{rudderAngle:0.0}" + "°";
                }
                
                float pitch = activeShip.transform.localEulerAngles.x;
                float yaw = activeShip.transform.localEulerAngles.y;
                float roll = activeShip.transform.localEulerAngles.z;

                PitchText.text = "俯仰角：" + $"{pitch:0.0}" + "°";
                YawText.text = "偏航角：" + $"{yaw:0.0}" + "°";
                RollText.text = "翻滚角：" + $"{roll:0.0}" + "°";
            }
        }
    }
}