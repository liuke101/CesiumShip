using System;
using NWH.Common.Cameras;
using NWH.Common.SceneManagement;
using NWH.Common.Vehicles;
using NWH.DWP2.ShipController;
using ShipAI;
using UnityEngine;
using UnityEngine.AzureSky;

namespace ShipManager
{
    public class SceneManager : MonoSingleton<SceneManager>
    {
        public AzureTimeController azureTimeController;
        public AdvancedShipController activeShip; 
        public Camera activeCamera; 
        
        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            InitAzureTime();
            
            activeShip = VehicleChanger.Instance.vehicles[0] as AdvancedShipController;
            if (activeShip != null)
            {
                activeCamera = activeShip.gameObject.GetComponentInChildren<CameraChanger>().cameras[0]
                    .GetComponent<Camera>();
                azureTimeController.m_followTarget = activeCamera.transform;
            }
        }
        
        private void InitAzureTime()
        {
            if (azureTimeController)
            {
                float timeValue = DateTime.Now.Hour + DateTime.Now.Minute / 60f + DateTime.Now.Second / 3600f;
                azureTimeController.SetTimeline(timeValue);
            }
        }

        public void UpdateActiveShip(Vehicle vehicle)
        {
            if (vehicle is AdvancedShipController ship)
            {
                activeShip = ship;
                activeCamera = activeShip.gameObject.GetComponentInChildren<CameraChanger>().cameras[0]
                    .GetComponent<Camera>();
                azureTimeController.m_followTarget = activeCamera.transform;
            }
            else
            {
                Debug.LogError("Vehicle类型错误");
            }
        }

        public void UpdateActiveCamera(Camera cam)
        {
            if (cam != null)
            {
                activeCamera = cam;
                azureTimeController.m_followTarget = activeCamera.transform;
            }
        }
    }
}