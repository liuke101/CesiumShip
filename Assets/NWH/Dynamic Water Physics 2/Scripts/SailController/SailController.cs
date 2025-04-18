using UnityEngine;
#if UNITY_EDITOR
using NWH.DWP2.NUI;
using UnityEditor;
#endif

namespace NWH.DWP2.SailController
{
    /// <summary>
    /// Calculates and applies the forces based on the sail position and wind speed.
    /// For sail rotation use SailRotator in combination with SailController.
    /// Transforms a, b, c and d denote the four corners of the sail, starting with bottom left and
    /// continuing in clockwise order. There is no limit to what these have to be parented to,
    /// so e.g. b and c (top points) and a and d (bottom points) can be attached to two different parents
    /// so that the sail can be furled (retracted/rolled/etc.). This method can also be used to generate
    /// two part sails. 
    /// </summary>
    public class SailController : MonoBehaviour
    {
        #region UserSettings

        /// <summary>
        /// Bottom left sail corner if square sail.
        /// Otherwise bottom front.
        /// </summary>
        [Tooltip("Bottom left sail corner if square sail.\r\nOtherwise bottom front.")]
        public Transform a;

        /// <summary>
        /// Top left sail corner if square sail.
        /// Otherwise top front.
        /// </summary>
        [Tooltip("Top left sail corner if square sail.\r\nOtherwise top front.")]
        public Transform b;

        /// <summary>
        /// Top right sail corner if square sail.
        /// Otherwise top rear.
        /// </summary>
        [Tooltip("Top right sail corner if square sail.\r\nOtherwise top rear.")]
        public Transform c;

        /// <summary>
        /// Bottom right sail corner if square sail.
        /// Otherwise bottom rear.
        /// </summary>
        [Tooltip("Bottom right sail corner if square sail.\r\nOtherwise bottom rear.")]
        public Transform d;

        public SailPreset sailPreset;

        /// <summary>
        /// The air density. Can also be used
        /// as a force coefficient as this affects both lift and drag forces equally.
        /// </summary>
        [Tooltip(
            "The air density. Can also be used\r\nas a force coefficient as this affects both lift and drag forces equally.")]
        public float airDensity = 1.225f;

        #endregion


        #region SailCalculated

        /// <summary>
        /// Surface-weighted center of the sail.
        /// Represents point at which the forces will be applied.
        /// </summary>
        public Vector3 SailCenter { get; private set; }

        /// <summary>
        /// Total area of the sail calculated from a, b, c, and d points.
        /// </summary>
        public float SailArea { get; private set; }

        /// <summary>
        /// Forward direction of the sail in world coordinates.
        /// </summary>
        public Vector3 SailForward { get; private set; }

        /// <summary>
        /// Up direction of the sail in world coordinates.
        /// </summary>
        public Vector3 SailUp { get; private set; }

        /// <summary>
        /// Right direction of the sail in world coordinates.
        /// </summary>
        public Vector3 SailRight { get; private set; }

        private Vector3 _liftForceDirection;

        private Vector3 _dragForceDirection;

        private float _liftForce;

        private float _dragForce;

        #endregion


        #region WindCalculated

        /// <summary>
        /// True wind direction, ignoring the speed of the ship.
        /// </summary>
        public Vector3 TrueWind { get; private set; }

        /// <summary>
        /// Velocity of the ship Rigidbody.
        /// </summary>
        public Vector3 ShipVelocity { get; private set; }

        /// <summary>
        /// Force applied to the sail, at the sail center point (weighted).
        /// </summary>
        public Vector3 SailForce { get; private set; }

        /// <summary>
        /// Apparent wind is the wind experienced on a moving sailboat.
        /// It combines the trueWind and the shipVelocity,
        /// resulting in a combined wind direction and speed.
        /// </summary>
        public Vector3 ApparentWind { get; private set; }

        /// <summary>
        /// Angle between the sail forward direction and the apparent wind.
        /// The forward direction is determined from (d-a).normalized.
        /// </summary>
        public float AngleOfAttack { get; private set; }

        #endregion


        #region Cached

        private Vector3   _pA;
        private Vector3   _pB;
        private Vector3   _pC;
        private Vector3   _pD;
        private Rigidbody _rb;

        #endregion


        private void Awake()
        {
            _rb = GetComponentInParent<Rigidbody>();
            Debug.Assert(_rb != null, "Rigidbody not found on the sail or any of the parent GameObjects.");
        }


        void Start()
        {
            if (WindGenerator.Instance == null)
            {
                Debug.LogError("WindGenerator not found in the scene. SailController will not work.");
            }

            if (sailPreset == null)
            {
                Debug.LogError($"SailPreset not assigned to SailController {name}");
            }
        }


        private void FixedUpdate()
        {
            Debug.Assert(a != null, $"{name}: Transform 'a' is not assigned.");
            Debug.Assert(b != null, $"{name}: Transform 'b' is not assigned.");
            Debug.Assert(c != null, $"{name}: Transform 'c' is not assigned.");
            Debug.Assert(d != null, $"{name}: Transform 'd' is not assigned.");

            // Update the sail positions, directions and geometry
            UpdateCachedPositions();
            SailCenter  = CalculateSurfaceWeightedCenter();
            SailArea    = CalculateSailArea();
            SailForward = CalculateSailForward();
            SailUp      = CalculateSailUp();
            SailRight   = CalculateSailRight(SailUp, SailForward);

            // Calculate velocities
            ShipVelocity  = _rb.velocity;
            TrueWind      = WindGenerator.Instance.CurrentWind;
            ApparentWind  = CalculateApparentWind(ShipVelocity, TrueWind);
            AngleOfAttack = CalculateAngleOfAttack(ApparentWind, SailForward);

            // Calculate and apply force
            SailForce = CalculateSailForce();
            _rb.AddForceAtPosition(SailForce, SailCenter);
        }


        private void UpdateCachedPositions()
        {
            _pA = a.position;
            _pB = b.position;
            _pC = c.position;
            _pD = d.position;
        }


        private Vector3 CalculateSurfaceWeightedCenter()
        {
            return GeometryUtils.CalculateSurfaceWeightedCenter(_pA, _pB, _pC, _pD);
        }


        private float CalculateSailArea()
        {
            return GeometryUtils.CalculateQuadrilateralArea(_pA, _pB, _pC, _pD);
        }


        private Vector3 CalculateSailForward()
        {
            return (_pA - _pD).normalized;
        }


        private Vector3 CalculateSailUp()
        {
            return ((_pB + _pC) * 0.5f - (_pA + _pD) * 0.5f).normalized;
        }


        private Vector3 CalculateSailRight(Vector3 sailUp, Vector3 sailForward)
        {
            return Vector3.Cross(sailUp, sailForward).normalized;
        }


        private Vector3 CalculateApparentWind(Vector3 boatVelocity, Vector3 trueWind)
        {
            return trueWind - boatVelocity;
        }


        private float CalculateAngleOfAttack(Vector3 apparentWind, Vector3 sailForward)
        {
            return Vector3.SignedAngle(sailForward, apparentWind, Vector3.up);
        }


        private Vector3 CalculateSailForce()
        {
            float apparentWindSpeed = ApparentWind.magnitude;
            float dynamicPressure   = 0.5f * airDensity * apparentWindSpeed * apparentWindSpeed;

            float liftCoefficient = sailPreset.liftCoefficientVsAoACurve.Evaluate(AngleOfAttack) * sailPreset.liftScale;
            _liftForce       = liftCoefficient * dynamicPressure * SailArea;

            float dragCoefficient = sailPreset.dragCoefficientVsAoACurve.Evaluate(AngleOfAttack) * sailPreset.dragScale;
            _dragForce       = dragCoefficient * dynamicPressure * SailArea;

            _liftForceDirection = SailRight * Mathf.Sign(Vector3.Dot(ApparentWind, SailRight));
            _dragForceDirection = ApparentWind.normalized;

            Vector3 totalForce = _liftForceDirection * _liftForce + _dragForceDirection * _dragForce;

            // Compensate for the lean
            totalForce *= Vector3.Dot(SailUp, Vector3.up);

            return totalForce;
        }


        private void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            if (a == null || b == null || c == null || d == null) return;

            UpdateCachedPositions();

            Vector3 center = CalculateSurfaceWeightedCenter();

            // Draw sail directions
            if (!Application.isPlaying) // Calculate directions if out of play mode
            {
                SailForward = CalculateSailForward();
                SailUp      = CalculateSailUp();
                SailRight   = CalculateSailRight(SailUp, SailForward);
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(center, SailForward);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(center, SailUp);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(center, SailRight);


            // Draw sail shape
            if (a != null && b != null && c != null && d != null)
            {
                Gizmos.color = Color.white;

                Gizmos.DrawLine(_pA, _pB);
                Gizmos.DrawLine(_pB, _pC);
                Gizmos.DrawLine(_pC, _pD);
                Gizmos.DrawLine(_pD, _pA);
                Gizmos.DrawLine(_pA, _pC);
                Gizmos.DrawLine(_pB, _pD);

                Gizmos.DrawSphere(_pA, 0.1f);
                Gizmos.DrawSphere(_pB, 0.1f);
                Gizmos.DrawSphere(_pC, 0.1f);
                Gizmos.DrawSphere(_pD, 0.1f);

                Handles.Label(_pA, "A (bottom left)");
                Handles.Label(_pB, "B (top left)");
                Handles.Label(_pC, "C (top right)");
                Handles.Label(_pD, "D (bottom right");
            }


            // RUNTIME ONLY FROM THIS POINT FORWARD
            if (!Application.isPlaying) return;

            // Draw force point (center)
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(center, 0.05f);

            // Draw wind
            Gizmos.color = Color.green;
            Gizmos.DrawRay(center, TrueWind * 0.2f);
            Handles.Label(center + TrueWind * 0.2f, $"True Wind ({TrueWind.magnitude} m/s)");

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(center, ApparentWind * 0.2f);
            Handles.Label(center + ApparentWind * 0.2f, $"Apparent Wind ({ApparentWind.magnitude} m/s)");

            Gizmos.color = Color.red;
            Gizmos.DrawRay(center, SailForce * 0.001f);
            Handles.Label(center + SailForce * 0.001f, $"Force ({SailForce.magnitude} N)");

            Gizmos.color = Color.white;
            Gizmos.DrawRay(center, _liftForceDirection);
            Handles.Label(center + _liftForceDirection, "Lift Force Dir.");

            Gizmos.color = Color.gray;
            Gizmos.DrawRay(center, _dragForceDirection);
            Handles.Label(center + _dragForceDirection, "Drag Force Dir.");

            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(center - Vector3.up * 0.1f, ShipVelocity * 0.1f);
            Handles.Label(center - Vector3.up * 0.1f + ShipVelocity * 0.1f,
                          $"Ship Velocity ({ShipVelocity.magnitude} m/s)");
            #endif
        }
    }
}


#if UNITY_EDITOR
namespace NWH.DWP2.SailController
{
    [CustomEditor(typeof(SailController))]
    [CanEditMultipleObjects]
    public class SailControllerEditor : DWP_NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            SailController sailController = (SailController)target;

            if (Application.isPlaying)
            {
                drawer.BeginSubsection("Debug Info");
                drawer.Label($"AoA: {sailController.AngleOfAttack}");
                drawer.Label($"Force Mag.: {sailController.SailForce.magnitude}");
                drawer.Label($"Force: {sailController.SailForce}");
                drawer.EndSubsection();
            }

            drawer.BeginSubsection("Geometry");
            drawer.Field("a");
            drawer.Field("b");
            drawer.Field("c");
            drawer.Field("d");
            drawer.EndSubsection();

            drawer.BeginSubsection("Physics");
            drawer.Field("sailPreset");
            drawer.Field("airDensity");
            drawer.EndSubsection();

            drawer.EndEditor(this);
            return true;
        }
    }
}
#endif