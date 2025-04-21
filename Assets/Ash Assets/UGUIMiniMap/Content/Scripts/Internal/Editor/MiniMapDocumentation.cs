using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UGUIMiniMap.TutorialWizard
{
    public class MiniMapDocumentation : TutorialWizard
    {
        //required//////////////////////////////////////////////////////
        private const string ImagesFolder = "ugui-minimap/editor/";
        private NetworkImages[] m_ServerImages = new NetworkImages[]
        {
        new NetworkImages{Name = "img-0.jpg", Image = null},
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.jpg", Image = null},
        new NetworkImages{Name = "img-9.jpg", Image = null},
        new NetworkImages{Name = "img-10.jpg", Image = null},
        new NetworkImages{Name = "img-11.jpg", Image = null},
        new NetworkImages{Name = "img-12.jpg", Image = null},
        new NetworkImages{Name = "img-13.jpg", Image = null},
        new NetworkImages{Name = "img-14.jpg", Image = null},
        new NetworkImages{Name = "img-15.png", Image = null},
        };
        private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "Get Started", StepsLenght = 1, DrawFunctionName = nameof(GetStarted) },
     new Steps { Name = "Add MiniMap", StepsLenght = 1, DrawFunctionName = nameof(AddMiniMap) },
     new Steps { Name = "SetUp MiniMap", StepsLenght = 3, DrawFunctionName = nameof(SetUpMiniMap),
     SubStepsNames = new string[]{ "Define Player", "Setup Layer", "Minimap Properties" } },
     new Steps { Name = "Picture Mode", StepsLenght = 4, DrawFunctionName = nameof(PictureMode) },
     new Steps { Name = "Third Person", StepsLenght = 1, DrawFunctionName = nameof(ThirdPersonDoc) },
     new Steps { Name = "Add Icon", StepsLenght = 1, DrawFunctionName = nameof(AddIcon) },
     new Steps { Name = "Player Icon", StepsLenght = 1, DrawFunctionName = nameof(PlayerIcon) },
     new Steps { Name = "Icon Text", StepsLenght = 1, DrawFunctionName = nameof(DrawIconText) },
     new Steps { Name = "Navigation Path", StepsLenght = 1, DrawFunctionName = nameof(NavPathDoc) },
     new Steps { Name = "Game Cameras", StepsLenght = 1, DrawFunctionName = nameof(GameCamerasDoc) },
     new Steps { Name = "No render object", StepsLenght = 1, DrawFunctionName = nameof(NoRenderObjectDoc) },
     new Steps { Name = "MiniMap Input", StepsLenght = 3, DrawFunctionName = nameof(MiniMapInputDoc),
     SubStepsNames = new string[] { "Change Keys", "Input System", "Drag"} },
     new Steps { Name = "Mobile", StepsLenght = 1, DrawFunctionName = nameof(MobileDoc) },
     new Steps { Name = "URP/HDRP", StepsLenght = 1, DrawFunctionName = nameof(URPDoc) },
     new Steps { Name = "Change Position", StepsLenght = 1, DrawFunctionName = nameof(ChangePositionDoc) },
     new Steps { Name = "Compass", StepsLenght = 1, DrawFunctionName = nameof(CompassDoc) },
     new Steps { Name = "Optimization", StepsLenght = 1, DrawFunctionName = nameof(OptimizationDoc) },
     new Steps { Name = "API", StepsLenght = 1, DrawFunctionName = nameof(APIDoc) },
    };

        private readonly GifData[] AnimatedImages = new GifData[]
   {
        new GifData{ Path = "umm-1.gif" },
        new GifData{ Path = "umm-2.gif" },

   };

        [MenuItem("Window/MiniMap/Documentation")]
        private static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(MiniMapDocumentation));
        }
        //final required////////////////////////////////////////////////

        public override void OnEnable()
        {
            base.OnEnable();
            base.Initizalized(m_ServerImages, AllSteps, ImagesFolder, AnimatedImages);
            SetHolderImage(Resources.Load<Texture2D>("place-holder-editor"));
            allowTextSuggestions = true;
            Style.highlightColor = "FF0045".ToUnityColor();
        }

        public override void WindowArea(int window)
        {
            GUI.skin.textArea.richText = true;
            AutoDrawWindows();
        }

        bool LayerApply = false;
        void GetStarted()
        {
            DrawText("After import the package in your Unity project you have to add a new <b>Layer</b>, for it if you have not do it yet, simply click in the button below.");
            DownArrow();
            if (!LayerExist("MiniMap"))
            {
                if (DrawButton("Add MiniMap Layer"))
                {
                    CreateLayer("MiniMap");
                    LayerApply = true;
                }
            }
            else { LayerApply = true; }

            if (LayerApply)
            {
                Space();
                DrawText("Done!, the layer is set up already, continue with the next step.");
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(EditorGUIUtility.IconContent("Collab").image);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }

        void AddMiniMap()
        {
            DrawText("In order to add a minimap in one of your maps/scenes, you simply have to drag one of the mini maps prefabs from the asset folder to the scene hierarchy\n                 \n  \n-The Mini Maps prefabs are located in <i>Assets ➔ UGUIMiniMap ➔ Content ➔ Prefabs ➔ Minimaps➔*</i>, you will see various example prefabs, each one has a different design but the same functionality, select the one that most fit to your needs and drag it to scene the hierarchy.");
            DrawServerImage("img-16.png");
            DownArrow();
            DrawText("Now you will see the Mini Map in the upper left corner in the Game View window, now you can start to set up it to you needs (see the next step)");
        }

        bool isSetupReady = false;
        void SetUpMiniMap()
        {
            if (subStep == 0)
            {
                DrawText("Now that you have the minimap in your map scene, you have to set up a few required parameters and if you want, you can tweak other optional settings to personalize the minimap.\n  \n<b><size=16>Define the Player</size></b>\n \nFirst, you have to set the <b>Player</b> object, in order to the minimap follow the player you have to let it know which object is the player in the scene, for it, simply assign the player object from the hierarchy window in the MiniMap ➔ bl_MiniMap ➔ General Settings ➔ <b>Target</b> field.");
                DrawServerImage("img-17.png");
                DrawHorizontalSeparator();
                DrawText("<b><size=16>Assign the player in runtime</size></b>");
                DrawText("There may be the case where the player is not in the scene by default and instead, it's instanced in runtime as usually happens with multiplayer games, for example\n \nFor these cases you can't assign the player in the inspector because you can't set the player prefab (from the Project Window), if is this your case then you have a few options to handle this scenario:\n \n1. Add the script <b>bl_MiniMapPlayer.cs</b> to the player prefab, as simple as that, attach that script in the player prefab that is instanced in runtime and it will automatically assign it as the minimap target when it's instanced.\n \n2. Assign by code, if you want to have more control over when and how to assign the target you can set the target to bl_MiniMap.cs with:");
                Space();
                DrawCodeText("bl_MiniMapUtils.GetMiniMap(0).Target = MyPlayerObjectReference;");
                Space();
                DrawText("You can put that code in the same script -> function where you instance the player");
            }
            else if (subStep == 1)
            {
                DrawText("<b><size=16>Assign the Minimap Layer</size></b>\n \nThe next thing that you have to set up is the MiniMap Layer.\n \nAs you may remember in the Get Started section you add a new Layer in the Project Settings, well, now you have to define that new layer in the Minimap inspector, simply go to the MiniMap ➔ bl_MiniMap ➔ General Settings ➔ MiniMap Layer ➔ Select the option called <b>MiniMap</b>, OR you can do this automatically by clicking in the button below:");

                if (DrawButton("Setup MiniMap Layer"))
                {
                    bl_MiniMap[] mn = FindObjectsByType<bl_MiniMap>(FindObjectsSortMode.None);
                    foreach (bl_MiniMap m in mn)
                    {
                        m.MiniMapLayer = LayerMask.NameToLayer("MiniMap");
                        EditorUtility.SetDirty(m);
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        Debug.Log("MiniMap layer set up!");
                    }
                    isSetupReady = true;
                }
                if (isSetupReady)
                {
                    DownArrow();
                    DrawText("Great!, All require settings are ready, but you have a bunch of optionals settings that you can play with to customize the MiniMap functionality, " +
                        "they will be cover in the next step so you can understand for what are they");
                }
            }
            else if (subStep == 2)
            {
                DrawText("Here I'll explain a little for what or what do each settings in bl_MiniMap.cs so you can understand they and you decide if you want use or not or customize to your needs.");
                Space();
                GUILayout.BeginVertical("General Settings", "window");
                DrawPropertieInfo("LevelName", "string", "used to show a custom map name in the full screen minimap UI");
                DrawPropertieInfo("MiniMapLayer", "LayerMask", "The layer of the minimap, that was set up in the previous step.");
                DrawPropertieInfo("Draw Mode", "Enum", "Tell the minimap how to render the map, Realtime: From a Camera or a Picture (Performed)");
                DrawPropertieInfo("Orthographic", "Bool", "use the MiniMap Camera in orthographic mode? useful for 2D games (for RealTime mode only)");
                DrawPropertieInfo("Map Mode", "Enum", "Local: follow the player, Global: static and render the whole map");
                DrawPropertieInfo("Map Render", "bl_MapRender", "when use Picture Mode, you have to assign the map render scriptable object.");
                DrawPropertieInfo("Is Mobile", "bool", "Define if the game is going to be a mobile game.");
                DrawPropertieInfo("Update Rate", "int", "Define the each how many frames the minimap is going to be updated, higher the value = better performance but less movement precision.");

                GUILayout.EndVertical();
                Space();
                GUILayout.BeginVertical("Zoom Settings", "window");
                DrawPropertieInfo("Default Zoom", "Float", "The default zoom of the MiniMap");
                DrawPropertieInfo("Zoom Range", "MinMax", "the minimum and maximum zoom allowed for the minimap");
                DrawPropertieInfo("Zoom Scroll Sensitivity", "Float", "Amount of change zoom with each mouse scroll");
                DrawPropertieInfo("Icon Size Multiplier", "Float", "Multiple the minimap icons size by this value");
                DrawPropertieInfo("Zoom Speed", "Float", "Zoom lerp speed");
                GUILayout.EndVertical();
                Space();
                GUILayout.BeginVertical("Position Settings", "window");
                DrawPropertieInfo("Smooth Position Tracking", "bool", "Should smooth the minimap target position tracking?");
                DrawPropertieInfo("Fullscreen Mode", "enum", "Define how the minimap is going to calculate the fullscreen size.");
                DrawPropertieInfo("FullScreenMapPosition", "Vector3", "The AnchoredPosition of the MiniMap when is in full screen mode");
                DrawPropertieInfo("FullScreenMapRotation", "Vector3", "The rotation of the minimap when is in full screen mode (and 3D draw mode)");
                DrawPropertieInfo("FullScreen Map Size", "Vector3", "The sizeDelta of the minimap anchor when is in full screen mode");
                GUILayout.EndVertical();
                Space();
                GUILayout.BeginVertical("Rotation Settings", "window");
                DrawPropertieInfo("Shape", "enum", "Define the shape of the minimap");
                DrawPropertieInfo("Circle Size", "Float", "The ratio of the circle where icons will rotate when are off screen");
                DrawPropertieInfo("Rotate with player", "Bool", "Rotate the minimap camera with the player or keep a static rotation?");
                DrawPropertieInfo("Icons Always Facing Up", "Bool", "force minimap icons to rotate and keep they original perspective");
                DrawPropertieInfo("Smooth Rotation", "Bool", "use lerp for rotate the minimap");
                GUILayout.EndVertical();
                Space();
                GUILayout.BeginVertical("Grip Settings", "window");
                DrawPropertieInfo("Show Grip", "Bool", "show a grip pattern over the minimap render?");
                DrawPropertieInfo("Row Grip Size", "Float", "The size of the grip pattern");
                GUILayout.EndVertical();
                Space();
                GUILayout.BeginVertical("Map Pointers Settings", "window");
                DrawPropertieInfo("Allow Map Pointers Grip", "Bool", "Players can create interest points by clicking the minimap?");
                DrawPropertieInfo("Allow multiples marks", "Bool", "Can create multiples interest points or just one per player?");
                GUILayout.EndVertical();
                Space();
                GUILayout.BeginVertical("Drag Settings", "window");
                DrawPropertieInfo("Active Drag MiniMap", "Bool", "Allow player to drag the mini map focus area");
                DrawPropertieInfo("Only on full screen", "Bool", "Allowed only in full screen mode");
                DrawPropertieInfo("Auto reset position", "Bool", "Reset the focus area when switch the minimap size");
                DrawPropertieInfo("Horiz / Vert Speed", "Vector2", "The drag sensitivity");
                DrawPropertieInfo("MinMax Horiz / Vert", "Vector2", "The drag area bounds limits");
                DrawPropertieInfo("Cursor X / Y offset", "Float", "offset the drag cursor image");
                GUILayout.EndVertical();
                Space();
                GUILayout.BeginVertical("Animations Settings", "window");
                DrawPropertieInfo("Show Level Name", "Bool", "Show the custom map name in the minimap UI when is in full screen");
                DrawPropertieInfo("Show Panel Info", "Bool", "Show a panel in the left side of the full screen minimap");
                DrawPropertieInfo("Fade on full screen", "Bool", "Fade effect when switch to full screen minimap");
                DrawPropertieInfo("FullscreenTransitionSpeed", "Float", "Speed of the transition of the minimap when switch size");
                DrawPropertieInfo("Damage effect speed", "Float", "Speed of the red flash effect when call the OnDamage function");
                GUILayout.EndVertical();
                Space();
                GUILayout.BeginVertical("Render Settings", "window");
                DrawPropertieInfo("Player Color", "Color", "The color of the Main Player icon in the minimap UI");
                DrawPropertieInfo("Tint Color", "Color", "Shader tint color of the map render (when use Picture mode)");
                DrawPropertieInfo("Specular Color", "Color", "Shader specular color of the map render (when use Picture mode)");
                DrawPropertieInfo("Emission Color", "Color", "Shader Emission color of the map render (when use Picture mode)");
                DrawPropertieInfo("Emission Amount", "Float", "Amount of the emission (brightness) of the map render (when use Picture mode)");
                GUILayout.EndVertical();
            }
        }

        void PictureMode()
        {
            if (subStep == 0)
            {
                DrawText("You can render the map in the mini map in two modes: <b>RealTime</b> and <b>Picture</b> mode, for the first as the name say it renders the map in real-time <i>(from the minimap camera)</i>, and with the <b>Picture Mode</b> you render just a texture <i>(screen shot)</i> of the map, this last one is performed much better than the real time since it just takes 1 draw call, this is indispensable for mobile platforms, that is the way that almost all games use for render mini maps.\n  \n-Now for use Real Time mode you simply need set the option in bl_MiniMap ➔ General Settings ➔ Render Mode ➔ <b>RealTime</b> and you are ready to go, but with picture mode you have to do a few extra steps");
                DownArrow();
                DrawText("- First you have to set up the Mini Map to use the Picture mode, so go to the MiniMap in your scene -> bl_MiniMap.cs inspector -> General Settings -> Render Mode -> Select <b>Picture</b>");
                DrawServerImage("img-26.png");
            }
            else if (subStep == 1)
            {
                DrawText("Now you have setup the map bounds, the visible area, for this you have to scale the <b>Map Boundary</b> transform in the hierarchy, to automatically select this object simply go to" +
                    " bl_MiniMap inspector -> General Settings -> Click on the button in the bottom <b>Set Bounds</b>");
                DrawServerImage("img-27.png");
                DownArrow();
                DrawText("After click the button in the Scene View you will see a plane gizmo selected with the Rect Tool automatically, you will have something like this:");
                DrawImage(GetServerImage(4));
                DrawText("Now you have to move and scale this transform to fit the map bounds.\n \nFirst, you can try to do this automatically by clicking on the inspector button of <b>bl_MiniMapBounds</b> > <b>Try to Automatically set Bounds</b>, this will calculate the bounds based on the meshes in your map scene");
                DrawServerImage("img-28.png");
                DrawText("Unfortunately, that method is not perfect and you still may need to adjust the bounds and position manually, using the Rect editor tool and the orthographic editor camera mode if necessary to set exactly the same size and floor position as the map in your scene");
                DrawAnimatedImage(0);
                DrawText("These bounds represent the area that the minimap is going to render so make sure it fits perfectly your map bounds, with no extra space or missing borders");
                DrawServerImage("img-29.png");
                DownArrow();
                DrawNote("Your map may not be exactly a square like in this example, but the logic is the same, set the bounds to the limit of each axis (x and z) of your map, <b>is highly recommended that even if your map doesn't have a squared shape your bounds should have the same size on the x and z axis</b>, so what you can do is fit the bounds of the x-axis and then set the same size for the z-axis even if it has extra space with respect to the limits of your map.");
            }
            else if (subStep == 2)
            {
                DrawText("Now, you have to bake your map render in an image or multiple images, for this, go to the MiniMap in your scene > bl_MiniMap > General Settings > click on the button <b>Render Map</b>");
                DrawServerImage("img-30.png");
                DrawText("After you click the button > the <i>Map Render</i> tool will be instantiated, and you should see in the <b>Game View</b> window a preview of the render area.");
                DrawServerImage("img-31.png");

                DrawText("By default, at the start the tool tries to automatically fit the whole map in the square area <i>(represented by the center border lines)</i> that will be rendered but this may not be perfect and you may need to adjust it manually to center and scale the render.\n \n- <b>To Move the render</b> simply move the <b>Map Render Tool</b> object in the hierarchy window");
                DrawAnimatedImage(1);
                DrawText("- <b>To Scale</b> simply modify the <b>Size</b> slider in the inspector of <b>Map Render Tool > bl_MiniMapRenderTool > Size</b>");
                DrawServerImage("img-32.png");
                DrawText("The objective is to put the map bounds inside the rendered borders perfectly centered");
                DrawServerImage("img-33.png");
                DrawText("Once you got it > select the <b>Map Render Tool</b> in the hierarchy > in the inspector of <b>bl_MiniMapRenderTool</b> > you have some properties that you can adjust, the important ones are the <b>Resolution</b> and the <b>Render Divisions</b>.\n \nThe <b>Resolution</b> is the pixel resolution of each map render, the higher the resolution = the better the image quality but larger the image file size.\n \nThe <b>Render Divisions</b> is in how many images the render will be divided, this is especially useful for big maps where even a single 4098x4098 single image render won't work because it will not capture all the map details as you may require, so what this does is to divide the render in multiple images and each image will render a different part of the map with the given resolution which after put them together will result in the same result as a higher resolution image, <i>e.g</i> a Render Division of 4x4 with 2048 Resolution will give the same result as an 8192x8192 (8K) image which otherwise you won't be able to use in Unity because the limit resolution is 4098 (4K)\n \nYou should use a sub-division render only if you really need to <i>(if a single image as the max resolution is not enough)</i>, otherwise, leave the property as <b>Single</b> to bake the render in a single image.");
                DrawText("To finally bake the Render simply click on the <b>Render Map</b> button");
                DrawServerImage("img-34.png");
                DrawText("After you click on the button it will prompt a window to select the folder to save the render, select the folder, and then the render will be baked and saved.");
                Space(20);
                DrawNote("After the bake finish, you can delete the Map Render Tool object from the scene hierarchy.");
            }
            else if (subStep == 3)
            {
                DrawText("Finally, all you have to do is assign the Render container in the MiniMap > <b>Map Render</b> field.\n \nThe Map Render container is located in the folder you select when baking the render which by default is <i>Assets ➔ UGUIMiniMap ➔ Content ➔ Art ➔ SnapShots➔*</i>, assign that render in your scene Minimap > bl_MiniMap > General Settings > <b>Map Render</b>");
                DrawServerImage("img-35.png");
                DrawNote("The map render container is the scriptable object, not the Texture image.");
            }
        }

        void ThirdPersonDoc()
        {
            DrawText("<size=18><b>Using Separate Targets for Minimap Position and Rotation</b></size>\n\n\nIn games where the camera rotates independently from the main character—such as third-person, top-down, or RTS games—you may want the minimap to follow the character’s position while using the camera’s rotation. By default, the minimap allows assigning only one target, which is used for both position and rotation reference. However, you can achieve the desired behavior with a few simple steps:\n\n<size=16><b>1. Attach the `<b>bl_MiniMapTarget</b>` Script</b></size>\n\n\nOn the GameObject assigned as the `<b>Target</b>` in <b>bl_MiniMap</b>, attach the `<b>bl_MiniMapTarget</b>` script.\n\n<size=16><b>2. Assign Position and Rotation Targets</b></size>\n\n\nIn the <b>Inspector</b> of `<b>bl_MiniMapTarget</b>`, you'll find two fields:\n• <b>Target</b> – Assign the transform of the main character (used for position tracking).\n\n• <b>Rotation Target</b> – Assign the transform of the camera (used for rotation tracking).\n\n\n<size=16><b>3. Done!</b></size>\n\n\nThe minimap will automatically recognize the `<b>bl_MiniMapTarget</b>` script and adjust its behavior accordingly. Now, the minimap will track the character’s position while using the camera’s rotation for orientation.");
            DrawServerImage("img-36.png");
        }

        void AddIcon()
        {
            DrawText("In order to show an icon in the minimap over an object in the 3D world, you simply have to attach the script <b>bl_MiniMapEntity.cs</b> in the object.\n \n" +
                "so select the object that you want add the icon and add the script bl_MiniMapEntity.cs");
            DrawServerImage("img-18.png");
            DrawText("After you attach the script you will have some parameters that you can use to customize the icon, but first, there's a field that you must assign and it's the <b>Icon Data</b> field.\n \nIn that field, you have to assign a scriptable object of <b>bl_MiniMapIconData</b>, you can click on the <b>Create New</b> button to create a new preset or you can assign an existing preset, by default the presets are located in the folder <i>Assets ➔ UGUIMiniMap ➔ Content ➔ Prefabs ➔ Presets➔*</i>");
            DrawServerImage("img-25.png");
            DownArrow();
            DrawText("Now you have in the added script you can customize some settings for reach what you want, the only required one is the <b>Icon</b>, below I'll describe for what is each or what do each one of the settings do.");
            DownArrow();
            DrawPropertieInfo("Target", "Transform", "The object to follow, you can leave this empty and the object where the script is attached will use as Target");
            DrawPropertieInfo("Icon", "Sprite", "The sprite icon for show in the minimap.");
            DrawPropertieInfo("Death Icon", "Sprite", "The sprite icon for show in the minimap when the object get destroyed.");
            DrawPropertieInfo("StartRenderDelay", "Float", "Delay seconds before the icon appear in the minimap after this is instanced");
            DrawPropertieInfo("Icon Color", "Color", "Color of the minimap icon");
            DrawPropertieInfo("Show Circle Area", "Bool", "Show a outline circle around this icon in the minimap");
            DrawPropertieInfo("Radius", "Float", "The Radius of the circle icon");
            DrawPropertieInfo("Border Offset", "Float", "Off set of the minimap border for this icon");
            DrawPropertieInfo("OffScreenIconSize", "Float", "Size of the icon when is off screen / out of the minimap focus area.");
            DrawPropertieInfo("is Interact able", "Bool", "This icon can detect when mouse down over it (like a button) and show a text when this happen?");
            DrawPropertieInfo("Text", "String", "Text to show when click over this icon");
            DrawPropertieInfo("Effect", "Enum", "UI loop effect of this icon in the minimap");
            DrawPropertieInfo("Destroy With Object", "Bool", "Destroy the minimap icon when the object where the script is attached get destroyed.");
            DrawPropertieInfo("is Placeholder", "Bool", "is this a icon created temporally?");
        }

        void PlayerIcon()
        {
            DrawText("As you will noticed the icon for main player is not set up like for other, you don't have to add the bl_MiniMapEntity.cs script to the main player, because the icon for this is already instanced," +
                " you only have to add the player in the Target field of bl_MiniMap.cs, so for change the icon for this you can assign it in <b>bl_MiniMap inspector -> Render Settings -> Player Icon.</b>");
            DrawImage(GetServerImage(12));
        }

        void DrawIconText()
        {
            DrawText("If you want to show some information when player hover or touch one of the icons in the minimap like this:");
            DrawImage(GetServerImage(13));
            DownArrow();
            DrawText("in the bl_MiniMapEntity.cs inspector <i>(of the minimap icon target)</i> toggle ON the field <b>isInteractable</b>, then set the text to display in the Text Area and select when the text will be displayed (OnHover or OnTouch the icon).");
            DrawImage(GetServerImage(14));
            DownArrow();
            DrawText("if you wanna change the text in runtime, first you have to have a reference of the bl_MiniMapEntity that you wanna edit, then just call: miniMapItem.SetIconText(\"My New Text\");");
        }

        void GameCamerasDoc()
        {
            DrawText("If you are using the Picture Mode or showing the Grids you may find the issue that your player can see the map render<i>(a giant plane mesh on the floor)</i>, this is because your camera render the <b>MiniMap</b> layer.\n \nTo fix this all you have to do is <b>remove the MiniMap layer from the Culling Mask of your game cameras</b>, you can do this manually for your camera's in prefabs and you can do this automatically for cameras instances in your map scenes:\n \nOpen your map scene and click in the button below to automatically remove the MiniMap layer from all the cameras in the opened scene.");

            if (DrawButton("Setup Cameras"))
            {
                SetupSceneCameras();
            }
        }

        void NoRenderObjectDoc()
        {
            DrawText("\"In the case that you don't want to render certain objects from your scene in the minimap, you can set this up easily.\n \n- <b>If you are using Picture mode</b> then all you have to do is hide/deactivate these objects while you take the map screenshot and then enable them again.\n \n- <b>If you are using Realtime mode</b>, all you have to do is assign the layers you don't want to render in the minimap, in the inspector of bl_MiniMap > <b>General Settings</b> tab > Exclude Layers.");
        }

        void MiniMapInputDoc()
        {
            if (subStep == 0)
            {
                DrawSuperText("By default the minimap inputs for Toggle the screen mode, Zoom In, and Zoom Out are '<b>M', '+', and '-'</b> respectively, you can change these keys in the preset located in:\n <i>Assets ➔ UGUIMiniMap ➔ Content ➔ Prefabs ➔ Presets ➔ <?link=asset:Assets/UGUIMiniMap/Content/Prefabs/Presets/MiniMap Input Legacy.asset>MiniMap Input Legacy</link></i>");
                DrawServerImage("img-21.png");
            }
            else if (subStep == 1)
            {
                DrawText("By default, the MiniMap only uses the Input System to detect 3 keys, for it, it <b>uses Unity's legacy input system</b>, a common issue I receive is about errors with Unity's new Input System <i>before UMM version 2.5.0</i>, after this version you won't get any errors but it still won't work with the new input system.\n \nBut it doesn't have to, <b>you can use both Input System</b> at the same time in your project, and setting it up is really simple, all you have to do is set the option <b>Active Input Handler</b> to <b>Both</b> in your <b>Player Settings > Other Settings</b>.");
            }
            else if (subStep == 2)
            {
                DrawText("A feature of the minimap system is that it allows you to move the minimap while you are in fullscreen mode, that way you can inspect other areas of the map that are out of the range of the current player view.\n \nYou can find all the parameters related to this feature in the inspector of bl_MiniMap > <b>Drag Settings</b> > *");
            }
        }

        void MobileDoc()
        {
            DrawText("This minimap works on all platforms including mobile platforms, but for low-end platforms such as mobile's where performance is more important you have to make sure to use the best settings for the max performance of the minimap.\n \n<b><size=16>Setup the Minimap for max performance</size></b>\n \n- First, if you are building for a mobile platform > turn on the <b>Is Mobile</b> toggle in bl_MiniMap > General Settings > <b>IsMobile</b>.\n \n- <b>Use Picture Mode</b>, real-time is easy to use but has a higher performance impact therefore <b>Picture mode</b> is the way to go for max performance, see the Picture mode section for the guide on how to set it up.\n \n- Increase the Update Rate, in bl_MiniMap > General Settings > <b>Update Rate</b>, increasing this value result in a lower performance cost per frame but with the disadvantage that it adds a jitter effect to the icons in the minimap, higher the value = more performance but more jitter movement, so find a value that works for you.");
            DrawServerImage("img-19.png");
        }

        void URPDoc()
        {
            DrawText("By default, the Minimap is set up for Unity's Built-In Render Pipeline. If you're using another render pipeline (URP or HDRP), you need to make one change to ensure compatibility and prevent the minimap from displaying a pink color.\n\n1. <b>Ensure Shader Graph is Installed:</b>\n\n  • Open the Unity Package Manager from the top navigation menu: `<b>Window > Package Manager</b>`.\n  • In the top left corner, select `<b>Packages: Unity Registry</b>`.\n  • Search for the <b>Shader Graph</b> package. If it's not installed, you'll see an install button in the bottom right corner. Click it to install.\n\n2. <b>Configure Minimap Settings:</b>\n\n  • Once Shader Graph is installed, navigate to the <b>Minimap Settings</b> in your project located in: <i>Assets/UGUIMiniMap/Resources/<b>MiniMapData</b></i>\n  • With the scriptable object selected, go to the Inspector window.\n  • In the Render Pipeline field, select the render pipeline you are using.\n\nThat's it! Your minimap should now be compatible with the URP or HDRP render pipeline.");
        }

        void ChangePositionDoc()
        {
            DrawText("If you want to change the position of the minimap which by default is set to the top left corner, you have to <b>make sure to adjust the anchor position too</b>, here is how you do it correctly:\n \n- The object that you have to move is the object <b>MiniMap UI</b> which is the first child of the MiniMap Canvas:");
            DrawServerImage("img-22.png");
            DrawSuperText("- Move this object where you want the new position to be, and after this adjust the anchor position of the same object too.\n \nIf you are not familiar with the UI Anchor, check this Unity official documentation in the Anchor section which explain it detailed:\n<?link=https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/UIBasicLayout.html>https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/UIBasicLayout.html</link>");
            DrawServerImage("img-23.png");
        }

        void CompassDoc()
        {
            DrawText("By default, the minimap prefabs have a cardinals compass UI, if you don't want to use it you can simply remove or deactivate the Compass UI from the MiniMap canvas, this object:");
            DrawServerImage("img-24.png");
        }

        void OptimizationDoc()
        {
            DrawText("<size=18><b><b>Optimization</b></b></size>\n\n\nYou can optimize the minimap further by trading a bit of visual quality for improved performance, reducing its impact on frame time.\n\n<size=16><b><b>1. Lower the Render Texture Resolution</b></b></size>\n\n\nBy default, the minimap uses a <b>1024x1024 Render Texture</b> for maximum clarity. However, if performance is a concern, reducing it to <b>512x512</b> or <b>256x256</b> can help <b>lower memory usage</b> and <b>reduce update costs</b>.\n\nTo adjust the resolution:\n• Open the <b>bl_MiniMap</b> inspector.\n\n• Navigate to the <b>Render Settings</b> tab.\n\n• Select <b>512x512</b> or <b>256x256</b> from the available options.\n\n• Click the <b>Set</b> button to apply the change.\n\n<size=16><b><b>2. Remove Unnecessary UI Elements</b></b></size>\n\n\nSome minimap prefabs include extra UI elements like vignette overlays, outlines, and decorative images. While these improve visual style, they <b>increase draw calls</b>, slightly impacting performance.\n\nTo optimize:\n• <b>Remove unused UI components</b> from the minimap canvas hierarchy.\n\n• <b>Combine multiple overlay images</b> into a single texture using an image editor (e.g., Photoshop) to <b>reduce draw calls</b> to one instead of multiple.");
            DrawText("<size=18><b><b>3. Limit Camera Update Rate</b></b></size>\n\n\nBy default, the minimap camera updates <b>every frame</b>, which, while efficient for smooth rendering, can still be <b>costly</b> in terms of performance. To optimize this, you can <b>enable Rate-Limited mode</b>, allowing the camera to update at a controlled interval instead of every frame.\n\n<size=16><b><b>How to Enable Rate-Limited Updates:</b></b></size>\n\n• Open the <b>bl_MiniMap</b> inspector.\n\n• Navigate to <b>Render Settings</b> tab.\n\n• Set the <b>Camera Update Mode</b> to <b>Rate Limited</b>.\n\n• Adjust the <b>Update Rate</b> value in the General Settings tab as needed.\n\n<size=16><b><b>Performance vs. Visual Quality Trade-off:</b></b></size>\n\n• <b>Higher Update Rate</b> → <b>Better performance</b>, but may introduce slight <b>jitter</b> in the minimap rendering.\n\n• <b>Lower Update Rate</b> → Smoother rendering, but <b>higher performance cost</b>.\n\nFind the right balance based on your needs to optimize both <b>performance and visual quality</b>");
        }

        void NavPathDoc()
        {
            DrawText("<size=18><b>Minimap Navigation Path</b></size>\n\n\nThe minimap navigation path is a useful feature in minimap systems that helps guide players to a specific location on the map. By selecting a target point on the minimap, a path is drawn from the player's current position to the destination, providing clear navigation. This feature is particularly beneficial in RPGs, open-world games, large maps, or any game where multiple routes exist to reach a location.\n\nThere are various ways to implement this system, such as manually placing navigation nodes, using custom A* pathfinding algorithms, or other navigation techniques. However, for simplicity, this system utilizes Unity’s <b>NavMesh</b> to calculate paths. To enable it, you only need to <b>bake the NavMesh</b> in your scene, allowing the system to generate paths automatically.");
            DrawHorizontalSeparator();
            DrawSuperText("<size=18><b>How to Use the Minimap Navigation Path</b></size>\n\n\nTo use this feature, you first need to <b>bake the NavMesh</b> in your map scene. The <b>NavMesh</b> is a navigable surface generated based on the static colliders in your scene. This mesh provides the necessary data for Unity’s <i><em>A</i> Pathfinding algorithm</em>* to calculate paths. If your scene is set up correctly, baking the NavMesh is a quick and simple process.\n\n<size=16><b>Baking the NavMesh</b></size>\n\n1. <b>Install the AI Navigation package</b> (if not already installed):\n\n  • Open <b>Window > Package Manager</b>\n\n  • Select <b>Unity Registry</b>\n\n  • Find and install <b>AI Navigation</b>\n\n2. <b>Create and bake the NavMesh:</b>\n\n  • Create an <b>empty GameObject</b> in your scene.\n\n  • Attach the <b>NavMeshSurface</b> component to it.\n\n  • Set <b>Use Geometry</b> to <b>All Colliders</b>.\n\n  • Click the <b>Bake</b> button.\n\nFor more detailed instructions, refer to the official documentation:\n<?link=https://docs.unity3d.com/Packages/com.unity.ai.navigation@1.1/manual/CreateNavMesh.html>https://docs.unity3d.com/Packages/com.unity.ai.navigation@1.1/manual/CreateNavMesh.html</link>\n\n<size=16><b>Enabling Minimap Navigation Path</b></size>\n\n\nOnce the NavMesh is baked:\n\n  • Ensure the feature is enabled in the minimap settings:  • Select the <b>MiniMap</b> in your scene.\n\n  • Go to <b>bl_MiniMap > Map Pointer Settings > Show Path Navigation</b>.\n\nThat's it! Now, when a player clicks on a point in the fullscreen minimap, a navigation path will be displayed, guiding them to the selected location.");
        }

        void APIDoc()
        {
            DrawText("If you need to reference the active minimap <i><b>(bl_MiniMap)</b></i> in the scene using a singleton, you can use:");
            DrawCodeText("bl_MiniMap.ActiveMiniMap");

            Space(10);
            DrawText("If you need to assign the minimap target at runtime—such as in multiplayer games where the player is not present in the scene by default but is instantiated after establishing a connection—simply add the following line of code in the script that instantiates the player. This will set the instantiated player's transform as the minimap target:");
            DrawCodeText(" bl_MiniMap.ActiveMiniMap.SetTarget(PLAYER_TRANSFORM);");
            Space(10);
            DrawText("To enable or disable the minimap, call <b>bl_MiniMap.SetActive(bool active, bool onlyUI)</b>. The first parameter controls whether the minimap is enabled or disabled, while the second parameter determines if only the <b>UI</b> should be affected (keeping the minimap logic running) or if the entire minimap system should be disabled.");
            DrawCodeText("bl_MiniMap.ActiveMiniMap.SetActive(true, false);");
            Space(10);
            DrawText("If you need to switch the active minimap at runtime—such as when your game has multiple minimap instances that change based on game logic—you can activate a different minimap while retaining the icons from the previous one. To do this, get a reference to the <b>bl_MiniMap</b> instance you want to activate and call the <b>SetAsActiveMiniMap()</b> function.");
            DrawCodeText("private bl_MiniMap miniMap;\n \n        void OnSwitchMiniMap()\n        {\n            miniMap.SetAsActiveMiniMap();\n        }");
            Space(10);
            DrawText("If you need to enable or disable a specific icon on the minimap based on your game logic, simply hold a reference to the <b>bl_MiniMapEntityBase</b> script in your game logic script (the script that controls when the icon should be shown or hidden). To toggle the icon's visibility, call the <b>SetActiveIcon(true/false)</b> method.");
            DrawCodeText("public bl_MiniMapEntityBase icon;\n \n        void OnEnableIcon()\n        {\n            icon.SetActiveIcon(true);\n        }\n \n        void OnDisableIcon()\n        {\n            icon.SetActiveIcon(false);\n        }");
            Space(10);
            DrawText("To create a waypoint or location marker on the minimap via code, simply call the <b>SetPointMark(POINT_WORLD_POSITION)</b> method from <b>bl_MiniMap</b> and pass the desired world position where you want the icon to appear.");
            DrawCodeText("bl_MiniMap.ActiveMiniMap.SetPointMark(POINT_WORLD_POSITION);");
        }

        public void CreateLayer(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new System.ArgumentNullException("name", "New layer name string is either null or empty.");

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layerProps = tagManager.FindProperty("layers");
            var propCount = layerProps.arraySize;

            SerializedProperty firstEmptyProp = null;

            for (var i = 0; i < propCount; i++)
            {
                var layerProp = layerProps.GetArrayElementAtIndex(i);

                var stringValue = layerProp.stringValue;

                if (stringValue == name) return;

                if (i < 8 || stringValue != string.Empty) continue;

                if (firstEmptyProp == null)
                    firstEmptyProp = layerProp;
            }

            if (firstEmptyProp == null)
            {
                UnityEngine.Debug.LogError("Maximum limit of " + propCount + " layers exceeded. Layer \"" + name + "\" not created.");
                return;
            }

            firstEmptyProp.stringValue = name;
            tagManager.ApplyModifiedProperties();
        }

        public static bool LayerExist(string layerName)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layerProps = tagManager.FindProperty("layers");
            var propCount = layerProps.arraySize;

            for (var i = 0; i < propCount; i++)
            {
                var layerProp = layerProps.GetArrayElementAtIndex(i);
                var stringValue = layerProp.stringValue;
                if (stringValue == layerName) return true;

                if (i < 8 || stringValue != string.Empty) continue;
            }

            return false;
        }

        [MenuItem("Window/MiniMap/Setup Scene Cameras")]
        static void SetupSceneCameras()
        {
            Camera[] all = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            int layerID = LayerMask.NameToLayer("MiniMap");
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].gameObject.name.Contains("MiniMap")) continue;
                int cm = all[i].cullingMask;
                int ncm = cm & ~(1 << layerID);
                all[i].cullingMask = ncm;
                EditorUtility.SetDirty(all[i]);
                Debug.Log("Camera: " + all[i].gameObject.name + " setup correctly");
            }
        }
    }
}