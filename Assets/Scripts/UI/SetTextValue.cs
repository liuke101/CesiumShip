using System;
using TMPro;
using UnityEngine;

namespace CesiumShip
{
    public class SetTextValue : MonoBehaviour
    {
        private TMP_Text Text;

        private void Awake()
        {
            Text = GetComponent<TMP_Text>();
        }
        
        public void SetText(float value)
        {
            if (Text != null)
            {
                Text.text = value.ToString("F2");
            }
            else
            {
                Debug.LogError("TMP_Text component not found!");
            }
        }
    }
}