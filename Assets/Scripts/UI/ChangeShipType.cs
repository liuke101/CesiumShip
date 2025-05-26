using System;
using NWH.Common.Cameras;
using NWH.Common.SceneManagement;
using ShipAI;
using ShipManager;
using TMPro;
using UnityEngine;

namespace ShipUI
{
    /// <summary>
    /// 改变船舶类型
    /// </summary>
    public class ChangeShipType : MonoBehaviour
    {
        private TMP_Dropdown dropDown;

        private void Awake()
        {
            dropDown = GetComponent<TMP_Dropdown>();
            dropDown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        private void OnDropdownValueChanged(int index)
        {
            switch (index)
            {
                case 0:
                    VehicleChanger.Instance.ChangeVehicle(0);
                    break;
                case 1:
                    VehicleChanger.Instance.ChangeVehicle(1);
                    break;
                case 2:
                    VehicleChanger.Instance.ChangeVehicle(2);
                    break;
                case 3:
                    VehicleChanger.Instance.ChangeVehicle(3);
                    break;
                default:
                    break;
            }
        }
    }
}