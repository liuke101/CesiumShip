using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace OmniVehicleAi
{
    public class PathFindingSceneManager : MonoBehaviour
    {
        public AIVehicleController AiVehicleController;
        public Button SetDestinationButton;
        public Button CloseDestinationViewButton;

        public Cinemachine.CinemachineVirtualCamera VehicleCamera;
        public Cinemachine.CinemachineVirtualCamera SelectDectinationCamera;

        public Transform VisualTarget;
        
        bool destinationSelectionOpened = false;

        private void Start()
        {
            CloseDestinationSelectionView();

            SetDestinationButton.onClick.AddListener(OpenDestinationSelectionView);
            CloseDestinationViewButton.onClick.AddListener(CloseDestinationSelectionView);
        }

        public void OpenDestinationSelectionView()
        {
            SelectDectinationCamera.Priority = 10;
            VehicleCamera.Priority = 0;
            SetDestinationButton.gameObject.SetActive(false);
            CloseDestinationViewButton.gameObject.SetActive(true);


            destinationSelectionOpened = true;
        }

        public void CloseDestinationSelectionView()
        {
            SelectDectinationCamera.Priority = 0;
            VehicleCamera.Priority = 10;
            SetDestinationButton.gameObject.SetActive(true);
            CloseDestinationViewButton.gameObject.SetActive(false);

            destinationSelectionOpened = false;
        }

        private void Update()
        {
            if (!destinationSelectionOpened) return;

            // get mouse raycast hit point
            if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    AiVehicleController.DriveToDestination(hit.point);
                    VisualTarget.position = hit.point;
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(VisualTarget.position, 5f);
        }

    }

    #if UNITY_EDITOR

    // create a simple editor for the path finding scene manager
    [CustomEditor(typeof(PathFindingSceneManager))]
    public class PathFindingSceneManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // draw the default inspector
            DrawDefaultInspector();

            // add space
            EditorGUILayout.Space();

            // editing tutorial
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Instructions", EditorStyles.boldLabel);

            GUIStyle instructionStyle = new GUIStyle(EditorStyles.label);
            instructionStyle.wordWrap = true; // Ensures wrapping

            string text = "when in Set Destination mode, press left ctrl and left click on terrain to make Ai drive to Destination.";
            EditorGUILayout.LabelField(text, instructionStyle);

            GUILayout.EndVertical();

        }
    }

    #endif

}
