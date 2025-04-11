using UnityEngine;
using UnityEditor;

namespace OmniVehicleAi
{
    public class QuickSetupWindow : EditorWindow
    {
        // Two GameObjects: Configured Vehicle and Target Vehicle
        private GameObject configuredVehicle;
        private GameObject targetVehicle;
        private GameObject sensorsPrefab;

        private bool isConfiguredValid = true;
        private static string documentationURL = "https://soft-pilot-e91.notion.site/Documentation-11d54a67048d8072bbf4c40b5788f9a8?pvs=4";

        // Add menu item named "Quick Setup" to the Tools menu
        [MenuItem("Tools/Ash Assets/Omni Vehicle Ai/Quick Setup")]
        public static void ShowWindow()
        {
            // Show the window
            GetWindow<QuickSetupWindow>("Quick Setup Window");
        }

        // Open Documentation option
        [MenuItem("Tools/Ash Assets/Omni Vehicle Ai/Online Documentation")]
        private static void OpenDocumentation()
        {
            Application.OpenURL(documentationURL);
        }

        // GUI code for the window
        private void OnGUI()
        {
            GUILayout.Label("Omni Vehicle AI - Quick Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Configure the target vehicle by copying AI components.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            // Input fields for the two GameObjects
            configuredVehicle = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Configured Vehicle", "The vehicle with AI components to copy"), configuredVehicle, typeof(GameObject), true);
            targetVehicle = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Target Vehicle", "The vehicle to which the components will be copied"), targetVehicle, typeof(GameObject), true);
            sensorsPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Sensors Prefab", "The sensors prefab to be added to the target vehicle"), sensorsPrefab, typeof(GameObject), true);

            // Validate input fields
            isConfiguredValid = ValidateConfiguredVehicle();
            if (!isConfiguredValid)
            {
                EditorGUILayout.HelpBox("Configured Vehicle must contain AIVehicleController and PathProgressTracker components.", MessageType.Error);
            }

            // Add a button to trigger the copy action
            EditorGUI.BeginDisabledGroup(!isConfiguredValid || configuredVehicle == null || targetVehicle == null || sensorsPrefab == null);
            if (GUILayout.Button("Copy Components"))
            {
                ConfigureComponents();
            }
            EditorGUI.EndDisabledGroup();
        }

        // Method to validate that the configured vehicle has the required components
        private bool ValidateConfiguredVehicle()
        {
            if (configuredVehicle == null) return true;

            AIVehicleController aiVehicleController = configuredVehicle.GetComponent<AIVehicleController>();
            PathProgressTracker pathProgressTracker = configuredVehicle.GetComponent<PathProgressTracker>();

            return aiVehicleController != null && pathProgressTracker != null;
        }

        // Method to copy components (enhanced with checks)
        private void ConfigureComponents()
        {
            if (!ValidateConfiguredVehicle())
            {
                Debug.LogError("Configured Vehicle does not contain the required components.");
                return;
            }

            // Check if target vehicle is valid
            if (configuredVehicle == null || targetVehicle == null)
            {
                Debug.LogWarning("Both Configured Vehicle and Target Vehicle must be assigned.");
                return;
            }

            AIVehicleController aiVehicleController_configured = configuredVehicle.GetComponent<AIVehicleController>();
            PathProgressTracker pathProgressTracker_configured = configuredVehicle.GetComponent<PathProgressTracker>();

            // Add necessary components to the target vehicle
            AIVehicleController aiVehicleController_target = targetVehicle.AddComponent<AIVehicleController>();
            PathProgressTracker pathProgressTracker_target = targetVehicle.AddComponent<PathProgressTracker>();

            aiVehicleController_target.vehicleRigidbody = targetVehicle.GetComponentInChildren<Rigidbody>();
            aiVehicleController_target.vehicleTransform = targetVehicle.transform;
            pathProgressTracker_target.aiVehicleController = aiVehicleController_target;

            // Copy values
            copyAIVehicleControllerValues(aiVehicleController_configured, aiVehicleController_target);
            copyPathProgressTrackerValues(pathProgressTracker_configured, pathProgressTracker_target);
            ConfigureSensors(aiVehicleController_target);


            Debug.Log($"Successfully copied components from {configuredVehicle.name} to {targetVehicle.name}.");
        }

        private void copyAIVehicleControllerValues(AIVehicleController configuredVehicle, AIVehicleController targetVehicle)
        {
            // Copy all necessary properties
            targetVehicle.InputLerpSpeed = configuredVehicle.InputLerpSpeed;
            targetVehicle.stoppingDistance = configuredVehicle.stoppingDistance;
            targetVehicle.reverseDistance = configuredVehicle.reverseDistance;
            targetVehicle.stuckTime = configuredVehicle.stuckTime;
            targetVehicle.reversingTime = configuredVehicle.reversingTime;
            //targetVehicle.slowDownSpeed = configuredVehicle.slowDownSpeed;
            targetVehicle.turnSlowDownSpeed = configuredVehicle.turnSlowDownSpeed;
            targetVehicle.decelerationRate = configuredVehicle.decelerationRate;
            targetVehicle.bufferSpeed = configuredVehicle.bufferSpeed;
            targetVehicle.useSpeedBasedBraking = configuredVehicle.useSpeedBasedBraking;
            targetVehicle.turnSlowDownAngle = configuredVehicle.turnSlowDownAngle;
            targetVehicle.bufferAngle = configuredVehicle.bufferAngle;
            targetVehicle.TurnRadius = configuredVehicle.TurnRadius;
            targetVehicle.TurnRadiusOffset = configuredVehicle.TurnRadiusOffset;
            targetVehicle.ObstacleSlowDownSpeed = configuredVehicle.ObstacleSlowDownSpeed;
            targetVehicle.ObstacleTurnDistance = configuredVehicle.ObstacleTurnDistance;
            targetVehicle.sensorLength = configuredVehicle.sensorLength;
            targetVehicle.roadWidth = configuredVehicle.roadWidth;
        }

        private void copyPathProgressTrackerValues(PathProgressTracker configuredVehicle, PathProgressTracker targetVehicle)
        {
            // Copy all necessary properties
            targetVehicle.offset_A = configuredVehicle.offset_A;
            targetVehicle.offset_AB = configuredVehicle.offset_AB;
            targetVehicle.offset_BC = configuredVehicle.offset_BC;
            targetVehicle.totalLaps = configuredVehicle.totalLaps;
            targetVehicle.loopCircuit = configuredVehicle.loopCircuit;

        }

        private void ConfigureSensors(AIVehicleController targetVehicle)
        {
            if (sensorsPrefab == null)
            {
                Debug.LogError("Sensors Prefab is not assigned!");
                return;
            }

            AIVehicleController.Sensor[] frontsensors = new AIVehicleController.Sensor[3];
            AIVehicleController.Sensor[] sidesensors = new AIVehicleController.Sensor[2];

            for (int i = 0; i < frontsensors.Length; i++) frontsensors[i] = new AIVehicleController.Sensor();
            for (int i = 0; i < sidesensors.Length; i++) sidesensors[i] = new AIVehicleController.Sensor();

            // Instantiate sensors prefab
#if UNITY_EDITOR
            GameObject newSensors = (GameObject)PrefabUtility.InstantiatePrefab(sensorsPrefab, targetVehicle.transform);
#else
            GameObject newSensors = Instantiate(sensorsPrefab, targetVehicle.transform);
#endif

            if (newSensors == null)
            {
                Debug.LogError("Failed to instantiate Sensors Prefab!");
                return;
            }

            Transform frontSensors = newSensors.transform.Find("Front Sensors");
            Transform sideSensors = newSensors.transform.Find("Side Sensors");

            if (frontSensors == null || sideSensors == null)
            {
                Debug.LogError("Sensors not found in the prefab. Ensure prefab structure is correct.");
                return;
            }

            // Assign front sensors
            frontsensors[0].sensorPoint = frontSensors.Find("Front Left");
            frontsensors[1].sensorPoint = frontSensors.Find("Middle");
            frontsensors[2].sensorPoint = frontSensors.Find("Front Right");

            // Assign side sensors
            sidesensors[0].sensorPoint = sideSensors.Find("Side Left");
            sidesensors[1].sensorPoint = sideSensors.Find("Side Right");

            targetVehicle.frontSensors = frontsensors;
            targetVehicle.sideSensors = sidesensors;

            Debug.Log("Sensors successfully attached to the target vehicle.");
        }
    }
}
