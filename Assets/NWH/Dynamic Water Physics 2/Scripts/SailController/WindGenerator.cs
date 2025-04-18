using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using NWH.DWP2.NUI;
using NWH.DWP2.SailController;
using UnityEditor;
#endif


namespace NWH.DWP2.SailController
{
    /// <summary>
    /// Generates random wind.
    /// </summary>
    public class WindGenerator : MonoBehaviour
    {
        public static WindGenerator Instance;

        /// <summary>
        /// Base wind speed in m/s.
        /// </summary>
        [UnityEngine.Tooltip("Base wind speed in m/s.")]
        public float baseSpeed = 10.0f;
        
        /// <summary>
        /// Base wind direction in degrees with 0 degrees indicating Z-forward ('north').
        /// </summary>
        [UnityEngine.Tooltip("Base wind direction in degrees with 0 degrees indicating Z-forward ('north').")]
        public float baseDirection;
        
        /// <summary>
        /// Maximum possible variation/deviation of the wind from the baseSpeed.
        /// </summary>
        [UnityEngine.Tooltip("Maximum possible variation/deviation of the wind from the baseSpeed.")]
        public float maxSpeedVariation = 5f;
        
        /// <summary>
        /// Maximum possible variation of the direction in degrees from the
        /// baseDirection.
        /// </summary>
        [UnityEngine.Tooltip("Maximum possible variation of the direction in degrees from the\r\nbaseDirection.")]
        public float maxDirectionVariation = 30f;
        
        /// <summary>
        /// Minimum interval between the wind variations / changes.
        /// </summary>
        [UnityEngine.Tooltip("Minimum interval between the wind variations / changes.")]
        public float minVariationInterval = 2.0f;
        
        /// <summary>
        /// Maximum interval between the wind variations / changes.
        /// </summary>
        [UnityEngine.Tooltip("Maximum interval between the wind variations / changes.")]
        public float maxVariationInterval = 6.0f;
        
        /// <summary>
        /// Current wind value as calculated from the settings.
        /// </summary>
        public Vector3 CurrentWind { get; private set; }
        
        /// <summary>
        /// Current wind direction in degrees, with 0 being Z-forward.
        /// </summary>
        public float CurrentDirection { get; private set; }
        
        /// <summary>
        /// Current wind speed in m/s.
        /// </summary>
        public float CurrentSpeed { get; private set; }
        
        
        private float _currentInterval = 1f;
        private float _targetSpeed;
        private float _targetDirection;
        private float _smoothingSpeedVelocity;
        private float _smoothingDirectionVelocity;

        
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("The scene has more than one WindGenerator. The previous one(s) will be ignored.");
            }
            Instance = this;
        }


        private void Start()
        {
            StartCoroutine(GustCoroutine());
        }


        private IEnumerator GustCoroutine()
        {
            while (true)
            {
                _targetSpeed = baseSpeed + Random.Range(-maxSpeedVariation, maxSpeedVariation);
                _targetDirection = baseDirection + Random.Range(-maxDirectionVariation, maxDirectionVariation);
                _currentInterval = Random.Range(minVariationInterval, maxVariationInterval);
                yield return new WaitForSeconds(_currentInterval);
            }
        }
        

        private void FixedUpdate()
        {
            CurrentDirection = Mathf.SmoothDamp(CurrentDirection, _targetDirection,
                ref _smoothingDirectionVelocity, _currentInterval);
            CurrentSpeed = Mathf.SmoothDamp(CurrentSpeed, _targetSpeed,
                ref _smoothingSpeedVelocity, _currentInterval);

            CurrentWind = Quaternion.AngleAxis(CurrentDirection, Vector3.up) * Vector3.forward * CurrentSpeed;
        }
    }
}


#if UNITY_EDITOR
namespace NWH.DWP2.SailController
{
    [CustomEditor(typeof(WindGenerator))]
    [CanEditMultipleObjects]
    public class WindGeneratorEditor : DWP_NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            WindGenerator windGenerator = (WindGenerator) target;

            if (Application.isPlaying)
            {
                drawer.Label($"Current Wind: {windGenerator.CurrentSpeed:0.0} from {windGenerator.CurrentDirection:0}");
            }

            drawer.BeginSubsection("Base");
            drawer.Field("baseSpeed");
            drawer.Field("baseDirection");
            drawer.EndSubsection();
            
            drawer.BeginSubsection("Variation");
            drawer.Field("maxSpeedVariation");
            drawer.Field("maxDirectionVariation");
            drawer.Field("minVariationInterval");
            drawer.Field("maxVariationInterval");
            drawer.EndSubsection();

            drawer.EndEditor(this);
            return true;
        }
    }
}
#endif