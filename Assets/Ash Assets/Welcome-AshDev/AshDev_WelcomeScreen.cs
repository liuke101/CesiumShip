using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AshDev.WelcomeScreen
{
#if UNITY_EDITOR

    [InitializeOnLoad]
    public class AshDev_WelcomeScreen : EditorWindow
    {
        static AshDev_WelcomeScreen()
        {
            EditorApplication.delayCall += () =>
            {
                if (SessionState.GetBool("AshDev_WelcomeScreen_Shown", false))
                {
                    return;
                }

                SessionState.SetBool("AshDev_WelcomeScreen_Shown", true);

                var importer = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath("bf3f9980c43241248935d8a59a323fa6"));
                if (importer != null && importer.userData == "dontshowagain")
                {
                    return;
                }

                Open();
            };
        }

        [MenuItem("Tools/AshDev - Welcome Screen")]
        static void Open()
        {
            ResetWelcomeScreenFlag();

            var window = GetWindow<AshDev_WelcomeScreen>(true, "Welcome to AshDev", true);
            window.minSize = new Vector2(640, 360);
            window.maxSize = new Vector2(640, 360);
        }

        // Method to reset the "Don't Show Again" flag
        static void ResetWelcomeScreenFlag()
        {
            var importer = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath("bf3f9980c43241248935d8a59a323fa6"));
            if (importer != null)
            {
                importer.userData = "";
                importer.SaveAndReimport();
            }
        }

        [System.Obsolete]
        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.height = new StyleLength(new Length(100, LengthUnit.Percent));

            // UXML
            var uxmlDocument = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("bf3f9980c43241248935d8a59a323fa6"));
            root.Add(uxmlDocument.Instantiate());

            // uss
            var ussDocument = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("14702adc3445aae46b0fda5e219da6ad"));
            root.styleSheets.Add(ussDocument);

            // Background image
            root.style.backgroundImage = new StyleBackground(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("7dad11779d6bc3d40bc4ba28422834d4")));
            root.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;


            // Buttons
            root.Q<Label>("freesetup").AddManipulator(new Clickable(evt => { Application.OpenURL("https://discord.gg/yU82FbNHcu"); }));
            root.Q<Label>("freelance").AddManipulator(new Clickable(evt => { Application.OpenURL("mailto:ashdevbiz@gmail.com"); }));
            root.Q<Label>("discord").AddManipulator(new Clickable(evt => { Application.OpenURL("https://discord.gg/yU82FbNHcu"); }));
            root.Q<Label>("youtube").AddManipulator(new Clickable(evt => { Application.OpenURL("https://www.youtube.com/@ashdev"); }));

            // Don't show again
            root.Q<Label>("dontshowagain").AddManipulator(new Clickable(evt => 
            {
                this.Close();
                var importer = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath("bf3f9980c43241248935d8a59a323fa6"));
                importer.userData = "dontshowagain";
                importer.SaveAndReimport();

            }));

            root.Q<Label>("close").AddManipulator(new Clickable(evt => { Close(); }));

        }

        

    }

#endif
}
