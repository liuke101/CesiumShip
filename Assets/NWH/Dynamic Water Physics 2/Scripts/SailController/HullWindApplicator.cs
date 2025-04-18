using UnityEngine;

#if UNITY_EDITOR
using NWH.DWP2.NUI;
using NWH.DWP2.SailController;
using UnityEditor;
#endif

namespace NWH.DWP2.SailController
{
    /// <summary>
    /// Applies the drag force to the hull based on the wind speed.
    /// Does not take the waterline into account so dimensions should be for the part of the
    /// hull above the water.
    /// </summary>
    public class HullWindApplicator : MonoBehaviour
    {
        /// <summary>
        /// The dimensions of the object (x = width, y = height, z = length).
        /// </summary>
        [Tooltip("The dimensions of the object (x = width, y = height, z = length).")]
        [SerializeField]
        public Vector3 dimensions;

        /// <summary>
        /// The drag coefficient of the object.
        /// </summary>
        [Tooltip("The drag coefficient of the object.")]
        [SerializeField]
        public float dragCoefficient = 1.0f;

        
        private Rigidbody _rb;

        
        private void Start()
        {
            _rb = GetComponentInParent<Rigidbody>();
            Debug.Assert(_rb != null, "Rigidbody not found on self or parents.");
        }

        
        private void FixedUpdate()
        {
            if (WindGenerator.Instance == null) return;

            // Calculate the frontal area of the object facing the wind
            Vector3 windDirection = WindGenerator.Instance.CurrentWind.normalized;
            float projectedArea = Mathf.Abs(Vector3.Dot(dimensions, windDirection));

            // Calculate the wind force
            float windSpeed = WindGenerator.Instance.CurrentWind.magnitude;
            float windForceMagnitude = 0.5f * dragCoefficient * projectedArea * Mathf.Pow(windSpeed, 2);
            Vector3 windForce = windDirection * windForceMagnitude;

            // Apply the wind force to the Rigidbody
            _rb.AddForce(windForce);
        }
    }
}


#if UNITY_EDITOR
namespace NWH.DWP2.SailController
{
    [CustomEditor(typeof(HullWindApplicator))]
    [CanEditMultipleObjects]
    public class HullWindApplicatorEditor : DWP_NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            drawer.Field("dimensions");
            drawer.Field("dragCoefficient");

            drawer.EndEditor(this);
            return true;
        }
    }
}
#endif