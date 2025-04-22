using System;
using ShipManager;
using TMPro;
using UnityEngine;
using UnityEngine.AzureSky;

namespace ShipUI
{
    //显示年月日时分秒
    public class RealWorldTime : MonoBehaviour
    {
        private TMP_Text dateText;
        
        private void Awake()
        {
            dateText = GetComponent<TMP_Text>();
        }
        
        private void Update()
        {
            //获取当前时间
            System.DateTime currentTime = System.DateTime.Now;
            //格式化时间
            string formattedTime = "时间：" + currentTime.ToString("yyyy-MM-dd HH:mm:ss");
            //设置文本
            dateText.text = formattedTime;
        }
    }
}