using UnityEngine;
using UnityEngine.Serialization;

namespace Data
{
    [CreateAssetMenu (fileName = "WeatherData", menuName = "ScriptableObjects/WeatherData", order = 0)]
    public class WeatherData : ScriptableObject
    {
        //Cloudy, windy, sunny, foggy, snowy, overcast, rainy and hail
        public Sprite Sunny; 
        public Sprite Cloudy; 
        public Sprite Overcast; //阴天
        public Sprite Windy;
        public Sprite Rainy;
        public Sprite Snowy;
        public Sprite Foggy;
        public Sprite Hail; //冰雹
    }
}