using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace ShipManager
{
    public class RealWorldMessage : MonoSingleton<RealWorldMessage>
    {
        /// <summary>
        /// 密钥 于高德开发者平台创建应用申请获得
        /// </summary>
        private const string key = "0c16a12aa6ec149344c36ed74eedf016";
        private string cityCode = "420100"; //武汉市的城市编码
        
        public TMP_Text dateText;
        public TMP_Text weatherText;
        public TMP_Text temperatureText;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            Get(cityCode, GetDataType.Lives, data => { });
        }

        private void Update()
        {
            if (dateText)
            {
                //获取当前时间
                System.DateTime currentTime = System.DateTime.Now;
                //格式化时间
                string formattedTime = "时间：" + currentTime.ToString("yyyy-MM-dd HH:mm:ss");
                //设置文本
                dateText.text = formattedTime;
            }
        }

        public enum GetDataType
        {
            /// <summary>
            /// 获取实况天气
            /// </summary>
            Lives,
            /// <summary>
            /// 获取预报天气
            /// </summary>
            Forecast
        }
        /// <summary>
        /// 获取天气数据
        /// </summary>
        /// <param name="city">城市编码</param>
        /// <param name="callback">回调函数</param>
        public void Get(string city, GetDataType type, Action<string> callback)
        {
            StartCoroutine(SendWebRequest(city, type, callback));
        }
        
        private IEnumerator SendWebRequest(string city, GetDataType type, Action<string> callback)
        {
            //url拼接
            string url = string.Format("https://restapi.amap.com/v3/weather/weatherInfo?key={0}&city={1}&extensions={2}", key, city, type == GetDataType.Lives ? "base" : "all");
            //GET方式调用API服务
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                DateTime beginTime = DateTime.Now;
                yield return request.SendWebRequest();
                DateTime endTime = DateTime.Now;
                if (request.result == UnityWebRequest.Result.Success)
                {
                    //Debug.Log($"{beginTime} 发起网络请求 于 {endTime} 收到响应：\r\n{request.downloadHandler.text}");
                    callback.Invoke(request.downloadHandler.text);

                    if (weatherText!=null)
                    {
                        //解析text数据，提取weather和temperature保存到string中
                        string weather = request.downloadHandler.text.Split(new string[] { "\"weather\":\"" }, StringSplitOptions.None)[1].Split('"')[0];
                        weatherText.text = "天气：" + weather;
                    }

                    if (temperatureText != null)
                    {
                        string temperature = request.downloadHandler.text.Split(new string[] { "\"temperature\":\"" }, StringSplitOptions.None)[1].Split('"')[0];
                        temperatureText.text = "气温：" + temperature + "°C";
                    }
                }
                else
                {
                    Debug.Log($"发起网络请求失败：{request.error}");
                }
            }
        }
    }
}
