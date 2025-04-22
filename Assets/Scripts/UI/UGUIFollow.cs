using System;
using NWH.Common.Cameras;
using NWH.Common.SceneManagement;
using NWH.Common.Vehicles;
using NWH.DWP2.ShipController;
using ShipManager;
using TMPro;
using UnityEngine;

namespace ShipUI
{
    /// <summary>
    /// UGUI跟随船舶
    /// </summary>
    public class UGUIFollow : MonoBehaviour
    {
        public Canvas canvas;
        public AdvancedShipController FollowShip; //船舶
        public Vector3 offset; //偏移量
        public TMP_Text speedText; //船速文本

        private void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
            }
        }

        private void Update()
        {
            if (canvas != null)
            {
                canvas.worldCamera = SceneManager.Instance.activeCamera;
            }
            
            if (SceneManager.Instance.activeShip != null)
            {
                float speed =FollowShip.SpeedKnots;
                speedText.text = "航速：" + $"{speed:0.0}" + "kts";
            }
            
            // UI跟随目标
            transform.position = FollowShip.transform.position + offset;
            
            // UI正对相机
            transform.rotation = Quaternion.LookRotation(transform.position - SceneManager.Instance.activeCamera.transform.position);
        }
    }
}