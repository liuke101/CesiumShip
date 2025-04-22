using System;
using ShipAI;
using ShipManager;
using TMPro;
using UnityEngine;

namespace ShipUI
{
    /// <summary>
    /// 改变控制模式
    /// </summary>
    public class ChangeControlMode : MonoBehaviour
    {
        private TMP_Dropdown dropDown;

        private void Awake()
        {
            dropDown = GetComponent<TMP_Dropdown>();
            dropDown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        private void OnDropdownValueChanged(int index)
        {
            ShipInputProvider shipInputProvider =
                SceneManager.Instance.activeShip.gameObject.GetComponentInChildren<ShipInputProvider>();
            if (shipInputProvider == null) return;
            
            switch (index)
            {
                case 0:
                    shipInputProvider.inputType = ShipInputProvider.InputType.Ai;
                    break;
                case 1:
                    shipInputProvider.inputType = ShipInputProvider.InputType.Player;
                    break;
                default:
                    break;
            }
        }
    }
}