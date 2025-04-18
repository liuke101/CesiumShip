#if UNITY_EDITOR
using NWH.DWP2.NUI;
using NWH.DWP2.WaterObjects;
using NWH.Common.Utility;
using UnityEditor;
using UnityEngine;

namespace NWH.DWP2
{
    [CustomEditor(typeof(WaterObject))]
    [CanEditMultipleObjects]
    [ExecuteAlways]
    public class WaterObjectEditor : DWP_NUIEditor
    {
        private Texture2D _originalMeshPreviewTexture;
        private Texture2D _simMeshPreviewTexture;
        private Texture2D _greyTexture;
        private bool      _editorHasWarnings;

        private WaterObject _waterObject;
        private float                   _previewHeight;


        private void OnEnable()
        {
            _greyTexture = new Texture2D(10, 10);
            FillInTexture(ref _greyTexture, new Color32(66, 66, 66, 255));
        }

        private static void FillInTexture(ref Texture2D tex, Color color)
        {
            Color[] fillColorArray = tex.GetPixels();

            for (int i = 0; i < fillColorArray.Length; i++)
            {
                fillColorArray[i] = color;
            }

            tex.SetPixels(fillColorArray);
            tex.Apply();
        }

        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }
            
            // Get water object and make sure it is initialized
            _waterObject = (WaterObject) target;

            // Draw logo texture
            Rect logoRect = drawer.positionRect;
            logoRect.height = 60f;
            drawer.DrawEditorTexture(logoRect, "Dynamic Water Physics 2/Logos/WaterObjectLogo");
            drawer.AdvancePosition(logoRect.height);

            if (_waterObject.originalMesh == null || _waterObject.serializedSimulationMesh == null || _waterObject.SimulationMesh == null)
            {
                drawer.Space(5);
                drawer.Info("Simulation mesh not generated. Click 'Update Simulation Mesh' button below to fix this.", MessageType.Error);
            }

            if (_waterObject.CurrentWaterDataProvider)
            {
                drawer.Info($"Current water data source: '{_waterObject.CurrentWaterDataProvider.GetType().Name}' " +
                            $"on object '{_waterObject.CurrentWaterDataProvider.name}'.");
            }

            if (Application.isPlaying)
            {
                drawer.Info($"Forces are being applied to '{_waterObject.targetRigidbody}'.");
            }
            
            drawer.BeginSubsection("Buoyancy");
            if (Application.isPlaying) drawer.Field("submergedVolume", false, "l");
            drawer.Field("buoyantForceCoefficient");
            drawer.Field("fluidDensity");
            drawer.EndSubsection();
            
            drawer.BeginSubsection("Hydrodynamics");
            drawer.Field("hydrodynamicForceCoefficient");
            drawer.Field("slamForceCoefficient");
            drawer.Field("suctionForceCoefficient");
            drawer.Field("skinDragCoefficient");
            drawer.Field("velocityDotPower");
            drawer.EndSubsection();

            drawer.BeginSubsection("Water");
            drawer.Field("calculateWaterHeights");
            drawer.Field("calculateWaterNormals");
            drawer.Field("calculateWaterFlows");
            
            drawer.Field("defaultWaterHeight");
            drawer.Field("defaultWaterNormal");
            drawer.Field("defaultWaterFlow");
            drawer.EndSubsection();
            
            // Simulation mesh
            drawer.BeginSubsection("Simulation Mesh Settings");
            if (drawer.Field("simplifyMesh").boolValue)
            {
                drawer.Field("targetTriangleCount");
            }

            drawer.Field("convexifyMesh");
            drawer.Field("weldColocatedVertices");

            if (drawer.Button("Update Simulation Mesh"))
            {
                UpdateSimulationMesh();
            }

            if (drawer.Button("Toggle In-Scene Preview"))
            {
                ToggleInScenePreview();
            }

            if (targets.Length == 1)
            {
                drawer.Label("Simulation Mesh Preview:");
                drawer.Space();
                if (Event.current.type == EventType.Repaint)
                {
                    DrawPreviewTexture(_waterObject, drawer.positionRect, out _previewHeight);
                }

                drawer.Space(_previewHeight);
            }

            drawer.EndSubsection();

            // Warnings
            drawer.BeginSubsection("Messages");
            DrawWarnings();
            drawer.EndSubsection();

            drawer.EndEditor(this);
            return true;
        }


        private void DrawWarnings()
        {
            if (targets.Length == 1)
            {
                _editorHasWarnings = false;

                // Missing rigidbody
                if (_waterObject.targetRigidbody == null)
                {
                    _waterObject.targetRigidbody = _waterObject.transform.GetComponentInParent<Rigidbody>(true);
                    if (_waterObject.targetRigidbody == null)
                    {
                        drawer.Info("WaterObject requires a rigidbody attached to the object or its parent(s) to " +
                                    $"function. Add a rigidbody to object {_waterObject.name} or one of its parents.",
                                    MessageType.Error);
                        _editorHasWarnings = true;

                        if (drawer.Button("Add a Rigidbody"))
                        {
                            foreach (WaterObject wo in targets)
                            {
                                wo.gameObject.AddComponent<Rigidbody>();
                            }
                        }
                    }
                }

                // Collider count
                if (_waterObject.targetRigidbody != null)
                {
                    int colliderCount = _waterObject.targetRigidbody.transform.GetComponentsInChildren<Collider>()
                                                    .Length;
                    if (_waterObject.targetRigidbody.transform.GetComponentsInChildren<Collider>().Length == 0)
                    {
                        drawer.Info(
                            $"Found {colliderCount} colliders attached to rigidbody {_waterObject.targetRigidbody.name} " +
                            "and its children. At least one collider is required for a rigidbody to work properly.",
                            MessageType.Error);
                        _editorHasWarnings = true;

                        if (drawer.Button("Add a MeshCollider"))
                        {
                            foreach (WaterObject wo in targets)
                            {
                                MeshCollider mc = wo.gameObject.AddComponent<MeshCollider>();
                                mc.convex    = true;
                                mc.isTrigger = false;
                            }
                        }
                    }
                }

                // Excessive triangle count
                if (_waterObject.triangleCount > 150)
                {
                    drawer.Info($"Possible excessive number of triangles detected ({_waterObject.triangleCount})." +
                                " Use simplify mesh option to reduce the number of triangles, or if this is intentional ignore this message." +
                                " Recommended number is 16-128.", MessageType.Warning);
                }

                // Scale error
                if (_waterObject.transform.localScale.x <= 0
                    || _waterObject.transform.localScale.y <= 0
                    || _waterObject.transform.localScale.z <= 0)
                {
                    drawer.Info(
                        "Scale of this object is negative or zero on one or more of axes. Scale less than or equal to zero is not supported." +
                        " WaterObject will still be calculated but with unpredictable results. ", MessageType.Error);
                }

                if (!_editorHasWarnings)
                {
                    drawer.Info($"No warnings or errors for object {_waterObject.name}.");
                }
            }
        }
        

        private void UpdateSimulationMesh()
        {
            foreach (WaterObject wo in targets)
            {
                wo.StopSimMeshPreview();
                wo.GenerateSimMesh();
                Undo.RecordObject(wo, "Updated Simulation Mesh");
                EditorUtility.SetDirty(wo);
            }
        }


        private void ToggleInScenePreview()
        {
            foreach (WaterObject wo in targets)
            {
                if (wo.PreviewEnabled)
                {
                    wo.StopSimMeshPreview();
                }
                else
                {
                    wo.StartSimMeshPreview();
                }
            }
        }


        private void DrawPreviewTexture(WaterObject waterObject, Rect rect, out float previewHeight)
        {
            previewHeight = 0;

            if (waterObject == null) return;
            if (waterObject.originalMesh == null) return;
            if (waterObject.serializedSimulationMesh.vertices == null) return;
            if (waterObject.SimulationMesh == null) return;

            // Tri count
            int originalTriCount = waterObject.originalMesh == null ? 0 : waterObject.originalMesh.triangles.Length / 3;
            int originalVertCount = waterObject.originalMesh == null ? 0 : waterObject.originalMesh.vertices.Length;
            int simulationTriCount = waterObject.serializedSimulationMesh.triangles.Length / 3;
            int simulationVertCount = waterObject.serializedSimulationMesh.vertices.Length;

            _originalMeshPreviewTexture = AssetPreview.GetAssetPreview(waterObject.originalMesh);
            _simMeshPreviewTexture = waterObject.SimulationMesh == null
                                         ? AssetPreview.GetAssetPreview(waterObject.originalMesh)
                                         : AssetPreview.GetAssetPreview(waterObject.SimulationMesh);

            float startY          = rect.y;
            float previewWidth    = rect.width;
            float maxPreviewWidth = 480f;
            previewWidth = Mathf.Clamp(previewWidth, 240f, maxPreviewWidth);
            float margin    = (rect.width - previewWidth) * 0.5f;
            float halfWidth = previewWidth * 0.5f;

            Rect leftRect  = new Rect(rect.x + margin,             startY, halfWidth, halfWidth);
            Rect rightRect = new Rect(rect.x + halfWidth + margin, startY, halfWidth, halfWidth);

            Material previewMaterial = new Material(Shader.Find("UI/Default"));

            GUI.DrawTexture(leftRect, _originalMeshPreviewTexture == null ? _greyTexture : _originalMeshPreviewTexture);
            GUI.DrawTexture(rightRect, _simMeshPreviewTexture == null ? _greyTexture : _simMeshPreviewTexture);

            GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment        = TextAnchor.MiddleCenter;
            centeredStyle.normal.textColor = Color.white;

            Rect leftLabelRect = leftRect;
            leftLabelRect.height = 20f;
            GUI.Label(leftLabelRect, "ORIGINAL", centeredStyle);

            Rect rightLabelRect = rightRect;
            rightLabelRect.height = 20f;
            GUI.Label(rightLabelRect, "SIMULATION", centeredStyle);

            Rect leftBottomLabelRect = leftRect;
            leftBottomLabelRect.y      = leftRect.y + halfWidth - 20f;
            leftBottomLabelRect.height = 20f;
            GUI.Label(leftBottomLabelRect, $"{originalTriCount} tris, {originalVertCount} verts");

            Rect rightBottomLabelrect = rightRect;
            rightBottomLabelrect.y      = rightRect.y + halfWidth - 20f;
            rightBottomLabelrect.height = 20f;
            GUI.Label(rightBottomLabelrect, $"{simulationTriCount} tris, {simulationVertCount} verts");

            previewHeight = halfWidth;
        }
    }
}

#endif
