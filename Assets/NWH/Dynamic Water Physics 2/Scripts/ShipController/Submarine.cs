using NWH.Common.Input;
using NWH.Common.Utility;
using NWH.Common.CoM;
using NWH.DWP2.WaterData;
using NWH.DWP2.WaterObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace NWH.DWP2.ShipController
{
    [RequireComponent(typeof(AdvancedShipController))]
    [RequireComponent(typeof(VariableCenterOfMass))]
    [DefaultExecutionOrder(500)]
    public class Submarine : MonoBehaviour, IMassAffector
    {
        public WaterObject ReferenceWaterObject;
        
        /// <summary>
        ///     Maximum additional mass that can be added (taking on water) to the base mass of the rigidbody to make submarine
        ///     sink.
        /// </summary>
        [FormerlySerializedAs("maxAdditionalMass")]
        [FormerlySerializedAs("maxMassFactor")]
        [Tooltip(
            "Maximum additional mass that can be added (taking on water) to the base mass of the rigidbody to make submarine sink.")]
        public float maxBallastMass = 200000f;

        /// <summary>
        /// Speed of change of the ballast mass, as a percentage of maxBallastMass.
        /// </summary>
        [UnityEngine.Tooltip("Speed of change of the ballast mass, as a percentage of maxBallastMass.")]
        public float ballastChangeSpeed = 0.05f;

        /// <summary>
        ///     If enabled submarine will try to keep horizontal by shifting the center of mass.
        /// </summary>
        [UnityEngine.Tooltip("    If enabled submarine will try to keep horizontal by shifting the center of mass.")]
        public bool keepHorizontal = false;

        /// <summary>
        ///     Sensitivity of calculation trying to keep the submarine horizontal. Higher number will mean faster reaction.
        /// </summary>
        [Tooltip(
            "Sensitivity of calculation trying to keep the submarine horizontal. Higher number will mean faster reaction.")]
        public float keepHorizontalSensitivity = 1f;

        /// <summary>
        ///     Maximum rigidbody center of mass offset that can be used to keep the submarine level.
        /// </summary>
        [UnityEngine.Tooltip("    Maximum rigidbody center of mass offset that can be used to keep the submarine level.")]
        public float maxMassOffset = 5f;
        
        private VariableCenterOfMass   _vcom;
        private float                  _zOffset;

        private float _mass;
        private Vector3 _centerOfMass;

        [HideInInspector] [SerializeField] private float depthInput;

        public float DepthInput
        {
            get { return depthInput; }
            set { depthInput = Mathf.Clamp(value, -1f, 1f); }
        }

        private void Awake()
        {
            if (ReferenceWaterObject == null)
            {
                ReferenceWaterObject = GetComponentInChildren<WaterObject>();
            }
        }

        private void Start()
        {
            _vcom         = GetComponentInParent<VariableCenterOfMass>();
            if (_vcom == null)
            {
                Debug.LogError($"VariableCenterOfMass script not found on object {name}. If updating from older versions" +
                               $"of DWP2 replace CenterOfMass [deprecated] script with VariableCenterOfMass [new] script.");
            }

            _vcom.useMassAffectors = true;
            _vcom.useDefaultMass = false;
            _vcom.useDefaultCenterOfMass = false;
            
            Debug.Assert(ReferenceWaterObject != null, "ReferenceWaterObject not set.");
        }


        private void FixedUpdate()
        {
            DepthInput     =  InputProvider.CombinedInput<ShipInputProvider>
                (i => i.SubmarineDepth());
            
            _mass -= DepthInput * maxBallastMass * ballastChangeSpeed * Time.fixedDeltaTime;
            _mass = Mathf.Clamp(_mass, 0f, Mathf.Infinity);

            if (keepHorizontal)
            {
                float angle = Vector3.SignedAngle(transform.up, Vector3.up, transform.right);
                _zOffset = Mathf.Clamp(Mathf.Sign(angle) * Mathf.Pow(angle * 0.2f, 2f) * keepHorizontalSensitivity,
                                      -maxMassOffset, maxMassOffset);
                Vector3 position = transform.position;
                _centerOfMass = new Vector3(position.x, position.y, position.z + _zOffset);
            }
        }


        public float GetMass()
        {
            return _mass;
        }


        public Vector3 GetWorldCenterOfMass()
        {
            return _centerOfMass;
        }


        public Transform GetTransform()
        {
            return transform;
        }
    }
}