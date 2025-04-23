/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsDemos
{
    [AddComponentMenu("Infinity Code/Online Maps/Demos/DriveUsingKeyboard")]
    public class DriveUsingKeyboard : MonoBehaviour
    {
        public GameObject prefab;
        public float markerScale = 5f;
        public float speed;
        public float maxSpeed = 160;
        public float rotation;
        public bool rotateCamera = true;
        public bool centerOnMarker = true;

        private OnlineMaps map;
        private OnlineMapsMarker3D marker;
        private double lng, lat;

        private void Start()
        {
            map = OnlineMaps.instance;

            map.GetPosition(out lng, out lat);

            marker = OnlineMapsMarker3DManager.CreateItem(lng, lat, prefab);
            marker.scale = markerScale;
            marker.rotationY = rotation;
        }

        private void Update()
        {
            float acc = 0;

            if (OnlineMapsInput.GetKey(KeyCode.W) || OnlineMapsInput.GetKey(KeyCode.UpArrow)) acc = 1;
            else if (OnlineMapsInput.GetKey(KeyCode.S) || OnlineMapsInput.GetKey(KeyCode.DownArrow)) acc = -1;
            
            if (Mathf.Abs(acc) > 0) speed = Mathf.Lerp(speed, maxSpeed * Mathf.Sign(acc), Time.deltaTime * Mathf.Abs(acc));
            else speed = Mathf.Lerp(speed, 0, Time.deltaTime * 0.1f);

            if (Mathf.Abs(speed) < 0.1) return;

            float r = 0;
            
            if (OnlineMapsInput.GetKey(KeyCode.A) || OnlineMapsInput.GetKey(KeyCode.LeftArrow)) r = -1;
            else if (OnlineMapsInput.GetKey(KeyCode.D) || OnlineMapsInput.GetKey(KeyCode.RightArrow)) r = 1;
            
            rotation += r * Time.deltaTime * speed;
            OnlineMapsUtils.GetCoordinateInDistance(lng, lat, speed * Time.deltaTime / 3600, rotation + 180, out lng, out lat);

            marker.rotationY = rotation;
            marker.SetPosition(lng, lat);
            if (centerOnMarker) map.SetPosition(lng, lat);
            if (rotateCamera) OnlineMapsCameraOrbit.instance.rotation = new Vector2(OnlineMapsCameraOrbit.instance.rotation.x, rotation + 180);
        }
    }
}