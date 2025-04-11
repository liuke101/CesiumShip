using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.Generic;

public class InputProviderGeneratorWindow : EditorWindow
{
    private UnityEngine.Object vehicleControllerReference; // Can be MonoScript or MonoBehaviour
    private string scriptName = "Input_Provider_Generated";
    private Vector2 previewScrollPos;
    private string previewScriptText = "";

    private enum InputSettingMode { Direct, SeparateMethods, SingleMethod }
    private InputSettingMode inputMode = InputSettingMode.Direct;

    private string[] fieldNames, methodNames;
    private int selectedAccelerationField, selectedSteerField, selectedHandbrakeField;
    private int selectedAccelerationMethod, selectedSteerMethod, selectedHandbrakeMethod;
    private int selectedSingleInputMethod;

    // Order settings for single method mode using ReorderableList
    private List<string> inputOrderList = new List<string> { "AccelerationInput", "SteerInput", "HandbrakeInput" };
    private ReorderableList reorderableList;

    private FieldInfo[] fields;
    private MethodInfo[] methods;
    private Type controllerType;

    [MenuItem("Tools/Ash Assets/Omni Vehicle Ai/Input Provider Generator")]
    private static void ShowWindow()
    {
        GetWindow<InputProviderGeneratorWindow>("Input Provider Generator");
    }

    private void OnEnable()
    {
        reorderableList = new ReorderableList(inputOrderList, typeof(string), true, true, false, false);
        reorderableList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Set Input Order for Single Method");
        reorderableList.drawElementCallback = (rect, index, active, focused) =>
        {
            EditorGUI.LabelField(rect, inputOrderList[index]);
        };
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical("box");

        GUILayout.Label("Vehicle AI Input Provider Generator", EditorStyles.boldLabel);

        GUILayout.Space(10);

        // Script or Component selection
        vehicleControllerReference = EditorGUILayout.ObjectField("Vehicle Controller (Script or Component)", vehicleControllerReference, typeof(UnityEngine.Object), true);

        if (vehicleControllerReference != null)
        {
            // Determine if the reference is a MonoScript or MonoBehaviour and get the class type accordingly
            if (vehicleControllerReference is MonoScript monoScript)
            {
                controllerType = monoScript.GetClass();
            }
            else if (vehicleControllerReference is MonoBehaviour monoBehaviour)
            {
                controllerType = monoBehaviour.GetType();
            }
            else
            {
                controllerType = null;
            }

            // Check if the selected type is a valid MonoBehaviour
            if (controllerType == null || !typeof(MonoBehaviour).IsAssignableFrom(controllerType))
            {
                EditorGUILayout.HelpBox("Selected object is not a valid MonoBehaviour script or component.", MessageType.Error);
            }
        }

        GUILayout.Space(10);

        // Input setting mode
        inputMode = (InputSettingMode)EditorGUILayout.EnumPopup("Input Mode", inputMode);

        if (controllerType != null && typeof(MonoBehaviour).IsAssignableFrom(controllerType))
        {
            // Load relevant fields and methods based on the input mode
            LoadRelevantFieldsAndMethods();

            GUILayout.Space(10);

            // Display options based on the input mode
            switch (inputMode)
            {
                case InputSettingMode.Direct:
                    DisplayDirectFieldSelection();
                    break;
                case InputSettingMode.SeparateMethods:
                    DisplayMethodSelection();
                    break;
                case InputSettingMode.SingleMethod:
                    DisplaySingleMethodSelection();
                    break;
            }

            GUILayout.Space(10);

            // Script name and preview generation
            GUILayout.Label("Script Name", EditorStyles.boldLabel);
            scriptName = EditorGUILayout.TextField(scriptName);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Preview", GUILayout.Height(30)))
            {
                previewScriptText = GenerateScriptPreview();
            }

            if (GUILayout.Button("Create Script", GUILayout.Height(30)))
            {
                CreateScript();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Preview section
            GUILayout.Label("Script Preview", EditorStyles.boldLabel);
            previewScrollPos = GUILayout.BeginScrollView(previewScrollPos, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            GUILayout.TextArea(previewScriptText, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            GUILayout.EndScrollView();
        }
        else if (controllerType == null)
        {
            EditorGUILayout.HelpBox("Please select a valid MonoBehaviour script or component.", MessageType.Warning);
        }

        GUILayout.EndVertical();
    }

    private void LoadRelevantFieldsAndMethods()
    {
        fields = controllerType.GetFields(BindingFlags.Public | BindingFlags.Instance);

        if (inputMode == InputSettingMode.Direct)
        {
            fields = fields.Where(f => f.FieldType == typeof(float)).ToArray();
        }

        methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        if (inputMode == InputSettingMode.SeparateMethods)
        {
            methods = methods.Where(m => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(float)).ToArray();
        }
        else if (inputMode == InputSettingMode.SingleMethod)
        {
            methods = methods.Where(m => m.GetParameters().Length == 3 && m.GetParameters().All(p => p.ParameterType == typeof(float))).ToArray();
        }

        fieldNames = fields.Select(f => f.Name).ToArray();
        methodNames = methods.Select(m => m.Name).ToArray();
    }

    private void DisplayDirectFieldSelection()
    {
        if (fieldNames.Length == 0)
        {
            EditorGUILayout.HelpBox("No float fields found in the selected script or component.", MessageType.Warning);
            return;
        }

        selectedAccelerationField = EditorGUILayout.Popup("Acceleration Field", selectedAccelerationField, fieldNames);
        selectedSteerField = EditorGUILayout.Popup("Steer Field", selectedSteerField, fieldNames);
        selectedHandbrakeField = EditorGUILayout.Popup("Handbrake Field", selectedHandbrakeField, fieldNames);
    }

    private void DisplayMethodSelection()
    {
        if (methodNames.Length == 0)
        {
            EditorGUILayout.HelpBox("No methods with a single float parameter found in the selected script or component.", MessageType.Warning);
            return;
        }

        selectedAccelerationMethod = EditorGUILayout.Popup("Acceleration Method", selectedAccelerationMethod, methodNames);
        selectedSteerMethod = EditorGUILayout.Popup("Steer Method", selectedSteerMethod, methodNames);
        selectedHandbrakeMethod = EditorGUILayout.Popup("Handbrake Method", selectedHandbrakeMethod, methodNames);
    }

    private void DisplaySingleMethodSelection()
    {
        if (methodNames.Length == 0)
        {
            EditorGUILayout.HelpBox("No methods with exactly three float parameters found in the selected script or component.", MessageType.Warning);
            return;
        }

        selectedSingleInputMethod = EditorGUILayout.Popup("Single Input Method", selectedSingleInputMethod, methodNames);

        GUILayout.Space(10);
        reorderableList.DoLayoutList();
    }

    private string GenerateScriptPreview()
    {
        StringBuilder scriptBuilder = new StringBuilder();
        string controllerTypeName = controllerType.Name;

        scriptBuilder.AppendLine("using UnityEngine;");
        scriptBuilder.AppendLine();
        scriptBuilder.AppendLine("namespace OmniVehicleAi");
        scriptBuilder.AppendLine("{");
        scriptBuilder.AppendLine($"    public class {scriptName} : MonoBehaviour");
        scriptBuilder.AppendLine("    {");

        scriptBuilder.AppendLine($"        public {controllerTypeName} vehicleController;");
        scriptBuilder.AppendLine("        public AIVehicleController aiVehicleController;");
        scriptBuilder.AppendLine();
        scriptBuilder.AppendLine("        public enum InputType { Player, Ai };");
        scriptBuilder.AppendLine("        public InputType inputType;");
        scriptBuilder.AppendLine();
        scriptBuilder.AppendLine("        public float AccelerationInput { get; private set; }");
        scriptBuilder.AppendLine("        public float SteerInput { get; private set; }");
        scriptBuilder.AppendLine("        public float HandbrakeInput { get; private set; }");
        scriptBuilder.AppendLine();

        scriptBuilder.AppendLine("        private void Update()");
        scriptBuilder.AppendLine("        {");
        scriptBuilder.AppendLine("            if (inputType == InputType.Player)");
        scriptBuilder.AppendLine("            {");
        scriptBuilder.AppendLine("                ProvidePlayerInput();");
        scriptBuilder.AppendLine("            }");
        scriptBuilder.AppendLine("            else");
        scriptBuilder.AppendLine("            {");
        scriptBuilder.AppendLine("                ProvideAiInput();");
        scriptBuilder.AppendLine("            }");
        scriptBuilder.AppendLine("        }");
        scriptBuilder.AppendLine();

        scriptBuilder.AppendLine("        private void ProvideAiInput()");
        scriptBuilder.AppendLine("        {");
        scriptBuilder.AppendLine("            SteerInput = aiVehicleController.GetSteerInput();");
        scriptBuilder.AppendLine("            AccelerationInput = aiVehicleController.GetAccelerationInput();");
        scriptBuilder.AppendLine("            HandbrakeInput = aiVehicleController.GetHandBrakeInput();");

        if (inputMode == InputSettingMode.Direct)
        {
            scriptBuilder.AppendLine($"            vehicleController.{fieldNames[selectedAccelerationField]} = AccelerationInput;");
            scriptBuilder.AppendLine($"            vehicleController.{fieldNames[selectedSteerField]} = SteerInput;");
            scriptBuilder.AppendLine($"            vehicleController.{fieldNames[selectedHandbrakeField]} = HandbrakeInput;");
        }
        else if (inputMode == InputSettingMode.SeparateMethods)
        {
            scriptBuilder.AppendLine($"            vehicleController.{methodNames[selectedAccelerationMethod]}(AccelerationInput);");
            scriptBuilder.AppendLine($"            vehicleController.{methodNames[selectedSteerMethod]}(SteerInput);");
            scriptBuilder.AppendLine($"            vehicleController.{methodNames[selectedHandbrakeMethod]}(HandBrakeInput);");
        }
        else if (inputMode == InputSettingMode.SingleMethod)
        {
            scriptBuilder.AppendLine($"            vehicleController.{methodNames[selectedSingleInputMethod]}(");
            for (int i = 0; i < inputOrderList.Count; i++)
            {
                scriptBuilder.AppendLine($"                {inputOrderList[i]}" + (i < inputOrderList.Count - 1 ? "," : ""));
            }
            scriptBuilder.AppendLine("            );");
        }

        scriptBuilder.AppendLine("        }");

        scriptBuilder.AppendLine("        private void ProvidePlayerInput()");
        scriptBuilder.AppendLine("        {");
        scriptBuilder.AppendLine("            // Example Player inputs:");
        scriptBuilder.AppendLine("            // AccelerationInput = Input.GetAxis(\"Vertical\");");
        scriptBuilder.AppendLine("            // SteerInput = Input.GetAxis(\"Horizontal\");");
        scriptBuilder.AppendLine("            // HandbrakeInput = Input.GetButton(\"Jump\") ? 1f : 0f;");
        scriptBuilder.AppendLine("        }");

        scriptBuilder.AppendLine("    }");
        scriptBuilder.AppendLine("}");

        return scriptBuilder.ToString();
    }

    private void CreateScript()
    {
        // Open a save panel to choose the location and filename
        string path = EditorUtility.SaveFilePanelInProject("Save Input Provider Script", scriptName, "cs", "Please enter a file name to save the script.", "Assets/Ash Assets/Omni Vehicle Ai");

        if (!string.IsNullOrEmpty(path))
        {
            System.IO.File.WriteAllText(path, previewScriptText);
            AssetDatabase.Refresh();
        }
    }
}
