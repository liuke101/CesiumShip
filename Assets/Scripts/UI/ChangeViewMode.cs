using System;
using NWH.Common.Cameras;
using ShipAI;
using ShipManager;
using TMPro;
using UnityEngine;

namespace ShipUI
{
    /// <summary>
    /// 改变视角模式
    /// </summary>
    public class ChangeViewMode : MonoBehaviour
    {
        private TMP_Dropdown dropDown;
        private CameraChanger cameraChanger;

        private void Awake()
        {
            dropDown = GetComponent<TMP_Dropdown>();
            dropDown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        private void OnDropdownValueChanged(int index)
        {
            cameraChanger = SceneManager.Instance.activeShip.gameObject.GetComponentInChildren<CameraChanger>();
            switch (index)
            {
                // 0: 自由视角
                case 0:
                    cameraChanger.ChangeCamera(0);
                    break;
                // 1: 船尾视角
                case 1:
                    cameraChanger.ChangeCamera(1);
                    break;
                // 2: 船内视角
                case 2:
                    cameraChanger.ChangeCamera(2);
                    break;
                // 3: 上帝视角
                case 3:
                    cameraChanger.ChangeCamera(3);
                    break;
                default:
                    break;
            }
        }
    }
}