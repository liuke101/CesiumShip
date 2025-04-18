using NWH.DWP2.ShipController;
using UnityEngine;

#if UNITY_EDITOR
using NWH.DWP2.NUI;
using NWH.DWP2.SailController;
using UnityEditor;
#endif

namespace NWH.DWP2.SailController
{
    public class SailRotator : MonoBehaviour
    {
        /// <summary>
        /// Rotation speed of this transform in deg/s.
        /// Multiplied by the rotationAxis to get the final rotation.
        /// </summary>
        [UnityEngine.Tooltip("Rotation speed of this transform in deg/s.\r\nMultiplied by the rotationAxis to get the final rotation.")]
        public float rotationSpeed = 50f;
        
        
        /// <summary>
        /// Rotation axis of this transform.
        /// Use -1 to reverse the rotation.
        /// </summary>
        [UnityEngine.Tooltip("Rotation axis of this transform.\r\nUse -1 to reverse the rotation.")]
        public Vector3 rotationAxis = new Vector3(0, 1, 0);

        
        private AdvancedShipController _shipController;
        
        
        private void Awake()
        {
            _shipController = GetComponentInParent<AdvancedShipController>();
            Debug.Assert(_shipController != null, "SailController requires the AdvancedShipController to" +
                                                  " be attached to one of the parents (does not have to be direct parent).");
        }
        
        
        private void Update()
        {
            float rotationAngle = _shipController.input.RotateSail * rotationSpeed * Time.deltaTime;
            transform.Rotate(rotationAngle * rotationAxis);
        }
    }
}


#if UNITY_EDITOR
namespace NWH.DWP2.SailController
{
    [CustomEditor(typeof(SailRotator))]
    [CanEditMultipleObjects]
    public class SailRotatorEditor : DWP_NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            drawer.Field("rotationSpeed");
            drawer.Field("rotationAxis");
            
            drawer.EndEditor(this);
            return true;
        }
    }
}
#endif