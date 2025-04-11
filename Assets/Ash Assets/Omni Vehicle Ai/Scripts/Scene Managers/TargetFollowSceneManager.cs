using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OmniVehicleAi
{
    public class TargetFollowSceneManager : MonoBehaviour
    {
        public AIVehicleController AIVehicleController;

        public Vector3 areaSize = new Vector3(100, 1, 100);

        [Range(0f, 3f)]
        public float delay = 2f;  // Time in seconds before target replacement

        private float timer = 0f; // Timer to track delay

        private void Update()
        {
            Vector3 targetPosition = AIVehicleController.target.position;
            Vector3 vehiclePosition = AIVehicleController.vehicleTransform.position;
            float distance = (targetPosition - vehiclePosition).magnitude;

            // Check if the vehicle is close enough and if the delay has passed
            if (distance < 5f)
            {
                timer += Time.deltaTime;
                if(timer >= delay)
                {
                    float x = Random.Range(-areaSize.x/2, areaSize.x/2);
                    float z = Random.Range(-areaSize.z/2, areaSize.z/2);
                    AIVehicleController.target.position = new Vector3(x, targetPosition.y, z);

                    timer = 0f;
                }
            }
            else
            {
                timer = 0f;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, areaSize);
        }
    }
}
