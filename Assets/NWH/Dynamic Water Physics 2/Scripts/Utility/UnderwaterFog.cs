using UnityEngine;

namespace NWH.DWP2
{
    public class UnderwaterFog : MonoBehaviour
    {
        public Transform waterTransform;
        
        private void Update()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;

            if (mainCamera.transform.position.y < waterTransform.position.y + 0.001f)
            {
                RenderSettings.fog = true;
            }
            else
            {
                RenderSettings.fog = false;
            }
        }
    }
}

