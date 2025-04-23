using System;
using NWH.DWP2.ShipController;
using NWH.DWP2.WaterObjects;
using ShipManager;
using UnityEngine;

namespace ShipUI
{
    public class ShipIconRotator : MonoBehaviour
    {
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
        private void Update()
        {
            AdvancedShipController ship = SceneManager.Instance.activeShip;
            if (rectTransform != null && ship!=null)
            {
                rectTransform.rotation = Quaternion.Euler(0, 0, -ship.transform.eulerAngles.y);
            }
        }
    }
}