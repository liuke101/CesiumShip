using Lovatto.MiniMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class bl_MiniMap : MonoBehaviour
{
    #region Public members
    public GameObject m_Target;
    public int MiniMapLayer = 10;
    public LayerMask excludeLayers;
    public Camera miniMapCamera = null;
    public MiniMapRenderType renderType = MiniMapRenderType.Picture;
    public MiniMapRenderMode canvasRenderMode = MiniMapRenderMode.Mode2D;
    public MiniMapMapType mapMode = MiniMapMapType.Local;
    public MiniMapCameraUpdateMode cameraUpdateMode = MiniMapCameraUpdateMode.EveryFrame;
    public bool Ortographic2D = false;
    public bl_MapRender mapRender = null;
    public bool isMobile = false;
    public int UpdateRate = 5;
    public float playerIconSize = 8;
    [Range(0.05f, 2)] public float IconMultiplier = 1;
    [Range(1, 10)] public int scrollSensitivity = 3;
    //Default height to view from, if you need have a static height, just edit this.
    public float DefaultHeight = 30;
    public bool saveZoomInRuntime = false;
    public float MaxZoom = 80;
    public float MinZoom = 5;
    public float LerpHeight = 8;
    public bool iconsSizeRelativeToZoom = true;
    public Sprite PlayerIconSprite;
    public MiniMapMapShape mapShape = MiniMapMapShape.Rectangle;
    public float CompassSize = 175f;
    public bool iconsAlwaysFacingUp = true;
    public bool DynamicRotation = true;
    public bool SmoothRotation = true;
    public float LerpRotation = 8;
    public float mapRotationOffset = 0;
    public bool AllowMapMarks = true;
    public GameObject MapPointerPrefab;
    public bool AllowMultipleMarks = false;
    public bool showPathNav = true;
    public bool ShowAreaGrid = true;
    [Range(1, 20)] public float AreasSize = 4;
    public float gridOpacity = 0.7f;
    public float overallOpacity = 1;
    public float backgroundOpacity = 1f;
    public float navPathWidth = 2.5f;

    public MiniMapFullScreenMode fullScreenMode = MiniMapFullScreenMode.ScreenArea;
    public bool FadeOnFullScreen = false;
    public float fullScreenMargin = 10;
    public float sizeTransitionDuration = 0.5f;
    public AnimationCurve sizeTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool showCursorOnFullscreen = true;

    public bool lerpTrackingPosition = false;
    public Vector3 FullMapPosition = Vector2.zero;
    public Vector3 FullMapRotation = Vector3.zero;
    public Vector2 FullMapSize = Vector2.zero;
    public bool CanDragMiniMap = true;
    public bool DragOnlyOnFullScreen = true;
    public bool ResetOffSetOnChange = true;
    public Vector2 DragMovementSpeed = new Vector2(0.5f, 0.35f);
    public Vector2 MaxOffSetPosition = new Vector2(1000, 1000);
    public Texture2D DragCursorIcon;
    public Vector2 HotSpot = Vector2.zero;
    public float planeSaturation = 1.4f;
    public bl_MiniMapBounds mapBounds;
    public Canvas m_Canvas = null;
    public GameObject ItemPrefabSimple = null;
    public Transform minimapRig;
    public MiniMapRTSize _rtSize = MiniMapRTSize._1024;
    public Color playerColor = Color.white;
    public Color emptySpaceColor = new Color(0, 0, 0, 0.25f);
    public Color navPathColor = Color.green;
    #endregion

    #region Public properties
    public bool IsFullScreen { get; set; }
    public bool hasError { get; set; }

    /// <summary>
    /// Current Minimap zoom level
    /// </summary>
    public float Zoom { get; set; }

    /// <summary>
    /// Does the minimap require high precision at the moment?
    /// </summary>
    public bool HighPrecisionMode
    {
        get;
        set;
    } = false;

    /// <summary>
    /// Current active minimap
    /// </summary>
    public static bl_MiniMap ActiveMiniMap { get; private set; }

    private bl_MiniMapUI _minimapUI = null;
    public bl_MiniMapUI MiniMapUI
    {
        get
        {
            if (_minimapUI == null)
            {
                _minimapUI = transform.parent != null
                    ? transform.parent.GetComponentInChildren<bl_MiniMapUI>(true)
                    : GetComponentInChildren<bl_MiniMapUI>(true);
            }
            return _minimapUI;
        }
    }
    #endregion

    #region Private members
    private GameObject mapPointer;
    [HideInInspector] public Vector3 MiniMapPosition = Vector2.zero;
    [HideInInspector] public Vector3 MiniMapRotation = Vector3.zero;
    [HideInInspector] public Vector2 MiniMapSize = Vector2.zero;
    private Vector3 DragOffset = Vector3.zero;
    private bool DefaultRotationMode = false;
    private Vector3 DeafultMapRot = Vector3.zero;
    private MiniMapMapShape defaultShape;
    public const string MMHeightKey = "MinimapCameraHeight";
    private bool isAlphaComplete = false;
    private bool isPlanedCreated = false;
    private readonly List<bl_MiniMapEntityBase> miniMapItems = new List<bl_MiniMapEntityBase>();
    private Vector3 playerPosition, targetPosition;
    private Vector3 playerRotation;
    private bool isUpdateFrame = false;
    private bl_MiniMapPlaneBase miniMapPlane;
    [HideInInspector] public bool _isPreviewFullscreen = false;
    private bool m_initialized = false;
    private bl_MiniMapInputBase inputHandler;
    private bool wasCursorVisible = false;
    private CursorLockMode wasCursorMode = CursorLockMode.None;
    private Vector3 m_mapRotationOffsetVector = Vector3.zero;
    private float minimapZoom = 0;
    private bl_MiniMapPathNav pathNav;
    private bl_MiniMapTarget targetScript;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        if (!m_initialized)
        {
            inputHandler = bl_MiniMapData.Instance.inputHandler;
            if (inputHandler != null) inputHandler.Init();

            MiniMapUI?.Setup(this);
            MiniMapUI.MiniMapSize?.Init(this);
            GetMiniMapSize();
            DefaultRotationMode = DynamicRotation;
            DeafultMapRot = minimapRig.eulerAngles;
            defaultShape = mapShape;
            m_mapRotationOffsetVector.Set(0, mapRotationOffset, 0);
            if (m_Target != null) m_Target.TryGetComponent(out targetScript);

            if (hasError) return;

            mapBounds?.Init();
            SetupMiniMapCamera();
            CreateMapPlane(renderType == MiniMapRenderType.RealTime);

            if (mapMode == MiniMapMapType.Local)
            {
                //Get Save Height
                Zoom = saveZoomInRuntime ? PlayerPrefs.GetFloat(MMHeightKey, DefaultHeight) : DefaultHeight;
            }
            else
            {
                ConfigureWorldTarget();
                Zoom = DefaultHeight;
            }
            minimapZoom = Zoom;
        }

        MiniMapUI.DoStartFade(0, () => { isAlphaComplete = true; });
        m_initialized = true;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        if (ActiveMiniMap == null) ActiveMiniMap = this;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        if (!isAlphaComplete) MiniMapUI.DoStartFade(0, () => { isAlphaComplete = true; });
    }

    /// <summary>
    /// Create a Plane with Map Texture
    /// MiniMap Camera will be renderer only this plane.
    /// This is more optimizing that RealTime type.
    /// </summary>
    void CreateMapPlane(bool realTime)
    {
        if (isPlanedCreated) return;
        if (mapRender == null && !realTime)
        {
            Debug.LogError("Map Render has not been assigned.");
            return;
        }
        if (!realTime || ShowAreaGrid)
        {
            GameObject plane = Instantiate(bl_MiniMapData.GetMapPlanePrefab().gameObject) as GameObject;
            miniMapPlane = plane.GetComponent<bl_MiniMapPlaneBase>();
            miniMapPlane.Setup(this);
        }
        isPlanedCreated = true;
    }

    /// <summary>
    /// Avoid to UI world space collision with other objects in scene.
    /// </summary>
    private void SetupMiniMapCamera()
    {
        //Verify is MiniMap Layer Exist in Layer Mask List.
        string layer = LayerMask.LayerToName(MiniMapLayer);
        //If not exist.
        if (string.IsNullOrEmpty(layer))
        {
            int tryID = LayerMask.NameToLayer("MiniMap");
            if (tryID == -1)
            {
                Debug.LogError($"MiniMap Layer '{tryID}' is null, please assign it in the inspector.", gameObject);
                MiniMapUI.SetActive(false);
                hasError = true;
                enabled = false;
                return;
            }
            else
            {
                MiniMapLayer = tryID;
            }
        }

        if (canvasRenderMode == MiniMapRenderMode.Mode3D)
        {
            Camera cam = (Camera.main != null) ? Camera.main : Camera.current;
            if (cam == null)
            {
                Debug.LogWarning("Main camera couldn't be found in the scene.");
                return;
            }
            m_Canvas.worldCamera = cam;
            //Avoid to 3D UI transferred other objects in the scene.
            cam.nearClipPlane = 0.015f;
            m_Canvas.planeDistance = 0.1f;
        }

        if (renderType == MiniMapRenderType.Picture)
        {
            miniMapCamera.cullingMask = 1 << MiniMapLayer;
        }

        if (excludeLayers.value != 0)
        {
            miniMapCamera.cullingMask &= ~excludeLayers.value;
        }

        Color bc = emptySpaceColor;
        bc.a *= backgroundOpacity;
        miniMapCamera.backgroundColor = bc;

        miniMapCamera.allowHDR = false;
        miniMapCamera.allowMSAA = false;

        miniMapCamera.enabled = cameraUpdateMode == MiniMapCameraUpdateMode.EveryFrame;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ConfigureWorldTarget()
    {
        if (m_Target == null)
            return;

        if (!m_Target.TryGetComponent<bl_MiniMapEntity>(out var mmi))
        {
            mmi = m_Target.AddComponent<bl_MiniMapEntity>();
        }
        MiniMapUI.ConfigureWorldTarget(mmi);
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (hasError || m_Target == null || miniMapCamera == null)
            return;
        isUpdateFrame = (Time.frameCount % UpdateRate) == 0;

        if (cameraUpdateMode == MiniMapCameraUpdateMode.RateLimited) miniMapCamera.Render();

        //Controlled inputs key for minimap
        if (!isMobile) { Inputs(); }
        //controlled that minimap follow the target
        PositionControl();
        //Apply rotation settings
        RotationControl();
        //for minimap and world map control
        MapZoomControl();
        //update all items (icons)
        UpdateItems();
    }

    /// <summary>
    /// Minimap follow the target.
    /// </summary>
    void PositionControl()
    {
        if (mapMode == MiniMapMapType.Local)
        {
            if (isUpdateFrame)
            {
                playerPosition = minimapRig.position;
                targetPosition = Target.position;
                // Update the transformation of the camera as per the target's position.
                playerPosition.x = targetPosition.x;
                if (!Ortographic2D) playerPosition.z = targetPosition.z;
                else playerPosition.y = targetPosition.y;

                playerPosition += DragOffset;

                //Calculate player position
                if (Target != null && MiniMapUI.PlayerIconTransform != null)
                {
                    Vector3 pp = miniMapCamera.WorldToViewportPoint(targetPosition);
                    MiniMapUI.PlayerIconTransform.anchoredPosition = bl_MiniMapUtils.CalculateMiniMapPosition(pp, MiniMapUI.root);
                }

                // For this, we add the predefined (but variable, see below) height var.
                if (!Ortographic2D)
                {
                    playerPosition.y = Target.TransformPoint(Vector3.up * 200).y;
                }
                else
                {
                    playerPosition.z = (targetPosition.z * 2) - (MaxZoom + (MinZoom * 0.5f));
                }
            }

            //Camera follow the target
            minimapRig.position = lerpTrackingPosition ? Vector3.Lerp(minimapRig.position, playerPosition, Time.deltaTime * 10) : playerPosition;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void RotationControl()
    {
        // If the minimap should rotate as the target does, the rotateWithTarget var should be true.
        // An extra catch because rotation with the full screen map is a bit weird.       
        if (DynamicRotation && mapMode != MiniMapMapType.Global)
        {
            if (isUpdateFrame)
            {
                //get local reference.
                playerRotation = minimapRig.eulerAngles;
                playerRotation.y = TargetRotation.y;
            }
            if (SmoothRotation)
            {
                if (isUpdateFrame)
                {
                    if (canvasRenderMode == MiniMapRenderMode.Mode2D)
                    {
                        //For 2D Mode
                        MiniMapUI.PlayerIconTransform.eulerAngles = Vector3.zero;
                    }
                    else
                    {
                        //For 3D Mode
                        MiniMapUI.PlayerIconTransform.localEulerAngles = Vector3.zero;
                    }
                }

                // Lerp rotation of map
                minimapRig.rotation = Quaternion.Slerp(minimapRig.rotation, Quaternion.Euler(playerRotation), Time.smoothDeltaTime * LerpRotation);
            }
            else
            {
                minimapRig.eulerAngles = playerRotation;
            }
        }
        else
        {
            m_mapRotationOffsetVector.y = mapRotationOffset;
            minimapRig.eulerAngles = DeafultMapRot + m_mapRotationOffsetVector;
            if (canvasRenderMode == MiniMapRenderMode.Mode2D)
            {
                //When map rotation is static, only rotate the player icon
                Vector3 e = Vector3.zero;
                //get and fix the correct angle rotation of target
                e.z = -TargetRotation.y + mapRotationOffset;
                MiniMapUI.PlayerIconTransform.eulerAngles = e;
            }
            else
            {
                //Use local rotation in 3D mode.
                Vector3 tr = RotationTarget.localEulerAngles;
                Vector3 r = Vector3.zero;
                r.z = -tr.y;
                MiniMapUI.PlayerIconTransform.localEulerAngles = r;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void UpdateItems()
    {
        if (!isUpdateFrame) return;
        if (miniMapItems == null || miniMapItems.Count <= 0) return;

        for (int i = miniMapItems.Count - 1; i >= 0; i--)
        {
            if (miniMapItems[i] == null) { miniMapItems.RemoveAt(i); continue; }
            miniMapItems[i].OnUpdateItem();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Inputs()
    {
        if (inputHandler == null) return;

        // If the minimap button is pressed then toggle the map state.
        if (inputHandler.IsInputDown(bl_MiniMapInputBase.MiniMapInput.ScreenMode))
        {
            ToggleSize();
        }
        if (inputHandler.IsInputDown(bl_MiniMapInputBase.MiniMapInput.ZoomOut))
        {
            ChangeZoom(true);
        }
        if (inputHandler.IsInputDown(bl_MiniMapInputBase.MiniMapInput.ZoomIn))
        {
            ChangeZoom(false);
        }
    }

    /// <summary>
    /// Map FullScreen or MiniMap
    /// Lerp all transition for smooth effect.
    /// </summary>
    void MapZoomControl()
    {
        float zoom = Mathf.Lerp(miniMapCamera.orthographicSize, Zoom, Time.deltaTime * LerpHeight);
        zoom = Mathf.Max(1, zoom);
        miniMapCamera.orthographicSize = zoom;
    }

    /// <summary>
    /// This called one time when press the toggle key
    /// </summary>
    void ToggleSize()
    {
        IsFullScreen = !IsFullScreen;
        if (IsFullScreen) SetToFullscreenSize();
        else SetToMiniMapSize();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetToMiniMapSize()
    {
        IsFullScreen = false;
        if (FadeOnFullScreen) { MiniMapUI.DoStartFade(0.35f, null); }
        if (mapMode != MiniMapMapType.Global)
        {
            //when return of full screen, return to current height
            Zoom = minimapZoom;
        }
        if (mapShape != defaultShape) { mapShape = defaultShape; }
        MiniMapUI.minimapMaskManager?.ChangeMaskType(false);
        if (DynamicRotation != DefaultRotationMode) { DynamicRotation = DefaultRotationMode; }

        if (showCursorOnFullscreen)
        {
            Cursor.visible = wasCursorVisible;
            Cursor.lockState = wasCursorMode;
        }

        bl_MiniMapOverlay.Instance?.SetActive(IsFullScreen);
        //reset offset position 
        if (ResetOffSetOnChange) { GoToTarget(); }

        MiniMapUI.MiniMapSize?.DoTransition();
        if (pathNav != null) pathNav.UpdateSize(this);

        float ratio = GetViewportRatio();
        foreach (var item in miniMapItems)
        {
            if (item == null) continue;
            item.OnViewportChanged(ratio);
        }
        if (iconsSizeRelativeToZoom && MiniMapUI.playerIcon != null) MiniMapUI.playerIcon.SetSize(playerIconSize * ratio);

    }

    /// <summary>
    /// 
    /// </summary>
    public void SetToFullscreenSize()
    {
        IsFullScreen = true;
        if (FadeOnFullScreen) { MiniMapUI.DoStartFade(0.35f, null); }
        if (mapMode != MiniMapMapType.Global)
        {
            //when change to full screen, the height is the max
            Zoom = MaxZoom;
        }
        mapShape = MiniMapMapShape.Rectangle;
        MiniMapUI.minimapMaskManager?.ChangeMaskType(true);
        if (DynamicRotation) { DynamicRotation = false; ResetMapRotation(); }

        if (showCursorOnFullscreen)
        {
            wasCursorVisible = Cursor.visible;
            wasCursorMode = Cursor.lockState;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        bl_MiniMapOverlay.Instance?.SetActive(IsFullScreen);
        //reset offset position 
        if (ResetOffSetOnChange) { GoToTarget(); }

        MiniMapUI.MiniMapSize?.DoTransition();
        if (pathNav != null) pathNav.UpdateSize(this);

        float ratio = GetViewportRatio();
        foreach (var item in miniMapItems)
        {
            if (item == null) continue;
            item.OnViewportChanged(ratio);
        }

        if (iconsSizeRelativeToZoom && MiniMapUI.playerIcon != null) MiniMapUI.playerIcon.SetSize(playerIconSize * ratio);
    }

    /// <summary>
    /// Make this minimap the active one
    /// If previously in the same scene another minimap was active
    /// This will transfer the icons from that minimap to this one.
    /// </summary>
    public void SetAsActiveMiniMap()
    {
        if (ActiveMiniMap == this) return;

        var othersMinimaps = FindObjectsByType<bl_MiniMap>(FindObjectsSortMode.None);
        for (int i = 0; i < othersMinimaps.Length; i++)
        {
            othersMinimaps[i].SetActive(false);
        }

        SetActive(true);
        if (ActiveMiniMap != null)
        {
            ActiveMiniMap.TransferIconsTo(this);
        }
        ActiveMiniMap = this;
        bl_MiniMapEvents.onActiveMiniMapChanged?.Invoke(this);
    }

    /// <summary>
    /// Transfer the instanced icons in this minimap
    /// to the given one
    /// </summary>
    /// <param name="otherMinimap">Minimap to transfer the icons</param>
    public void TransferIconsTo(bl_MiniMap otherMinimap)
    {
        foreach (var item in miniMapItems)
        {
            if (item == null) continue;

            item.ChangeMiniMapOwner(otherMinimap);

            if (!otherMinimap.miniMapItems.Contains(item))
            {
                otherMinimap.miniMapItems.Add(item);
            }
        }
    }

    /// <summary>
    /// Enable/Disable this minimap
    /// </summary>
    /// <param name="active"></param>
    /// <param name="onlyUI">Only affect the UI but not the MiniMap component</param>
    public void SetActive(bool active, bool onlyUI = false)
    {
        if (!onlyUI)
        {
            gameObject.SetActive(active);
            if (miniMapPlane != null) miniMapPlane.SetActive(active);
        }
        else MiniMapUI.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    public void SetDragPosition(Vector3 pos)
    {
        if (DragOnlyOnFullScreen)
        {
            if (!IsFullScreen)
                return;
        }

        DragOffset.x += ((-pos.x) * DragMovementSpeed.x);
        DragOffset.z += ((-pos.y) * DragMovementSpeed.y);

        DragOffset.x = Mathf.Clamp(DragOffset.x, -MaxOffSetPosition.x, MaxOffSetPosition.x);
        DragOffset.z = Mathf.Clamp(DragOffset.z, -MaxOffSetPosition.y, MaxOffSetPosition.y);
    }

    /// <summary>
    /// Create a point in the map from the given world position
    /// </summary>
    /// <param name="Position">world map position</param>
    public void SetPointMark(Vector3 Position)
    {
        if (!AllowMultipleMarks)
        {
            Destroy(mapPointer);
        }
        mapPointer = Instantiate(MapPointerPrefab, Position, Quaternion.identity) as GameObject;
        mapPointer.GetComponent<bl_MapPointerBase>().SetColor(playerColor);

        if (showPathNav)
        {
            if (pathNav == null)
            {
                var go = Instantiate(bl_MiniMapData.Instance.pathNavPrefab.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
                pathNav = go.GetComponent<bl_MiniMapPathNav>();
                go.layer = LayerMask.NameToLayer("MiniMap");
                pathNav.SetColor(navPathColor);
                pathNav.SetWidth(navPathWidth);
            }

            pathNav.TrackTarget(Target, Position, this);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void GoToTarget()
    {
        StopCoroutine(nameof(ResetOffset));
        StartCoroutine(nameof(ResetOffset));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator ResetOffset()
    {
        while (Vector3.Distance(DragOffset, Vector3.zero) > 0.2f)
        {
            DragOffset = Vector3.Lerp(DragOffset, Vector3.zero, Time.deltaTime * 12);
            yield return null;
        }
        DragOffset = Vector3.zero;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="b"></param>
    public void ChangeZoom(bool zoomIn)
    {
        if (mapMode == MiniMapMapType.Global)
            return;

        if (zoomIn) Zoom += scrollSensitivity;
        else Zoom -= scrollSensitivity;

        Zoom = Mathf.Clamp(Zoom, MinZoom, MaxZoom);
        minimapZoom = Zoom;
        if (saveZoomInRuntime) PlayerPrefs.SetFloat(MMHeightKey, Zoom);

        float ratio = GetViewportRatio();
        foreach (var item in miniMapItems)
        {
            if (item == null) continue;

            item.OnViewportChanged(ratio);
        }
        if (pathNav != null) pathNav.UpdateSize(this);
        if (iconsSizeRelativeToZoom && MiniMapUI.playerIcon != null) MiniMapUI.playerIcon.SetSize(playerIconSize * ratio);

    }

    /// <summary>
    /// Call this when player / target receive damage
    /// for play a 'Hit effect' in minimap.
    /// </summary>
    public void DoHitEffect() => MiniMapUI?.DoHitEffect();

    /// <summary>
    /// Create a new icon without reference in runtime.
    /// see all structure options in bl_MMItemInfo.
    /// </summary>
    public bl_MiniMapEntityBase CreateNewItem(MiniMapIconSettings item)
    {
        if (hasError) return null;

        GameObject newItem = Instantiate(ItemPrefabSimple, item.Position, Quaternion.identity) as GameObject;
        var mmItem = newItem.GetComponent<bl_MiniMapEntityBase>();

        mmItem.SetIconSettings(item);

        return mmItem;
    }

    /// <summary>
    /// Reset this transform rotation helper.
    /// </summary>
    void ResetMapRotation() { minimapRig.eulerAngles = new Vector3(90, 0, 0); }

    /// <summary>
    /// Change the size of Map full screen or mini
    /// </summary>
    /// <param name="fullscreen">is full screen?</param>
    public void ChangeMapSize(bool fullscreen)
    {
        IsFullScreen = fullscreen;
    }

    /// <summary>
    /// Set target in runtime
    /// </summary>
    /// <param name="t"></param>
    public void SetTarget(GameObject t)
    {
        m_Target = t;
    }

    /// <summary>
    /// Set target in runtime
    /// </summary>
    public void SetTarget(bl_MiniMapTarget newTarget)
    {
        targetScript = newTarget;
    }

    /// <summary>
    /// Set Map Texture in Runtime
    /// </summary>
    /// <param name="t"></param>
    public void SetMapTexture(Texture2D newTexture)
    {
        if (renderType != MiniMapRenderType.Picture)
        {
            Debug.LogWarning("You only can set texture in Picture Mode");
            return;
        }
        miniMapPlane.SetMapTexture(newTexture);
    }

#if UNITY_EDITOR
    public void OnValidate()
    {
        if (miniMapCamera != null)
        {
            miniMapCamera.orthographicSize = DefaultHeight;
        }
        if (MiniMapUI != null && MiniMapUI.playerIcon != null)
        {
            MiniMapUI.playerIcon.SetIcon(PlayerIconSprite, true);
            MiniMapUI.playerIcon.SetColor(playerColor);

            EditorUtility.SetDirty(MiniMapUI.playerIcon);
        }

        if (MiniMapUI != null)
        {
            if (MiniMapUI.rootAlpha != null) MiniMapUI.rootAlpha.alpha = overallOpacity;
        }
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    public void SetGridSize(float value)
    {
        if (miniMapPlane == null) return;

        miniMapPlane.SetGridSize(value);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetActiveGrid(bool active)
    {
        if (miniMapPlane == null) return;

        miniMapPlane.SetActiveGrid(active);
    }

    /// <summary>
    /// Call this to change the mode of rotation of map
    /// Static or dynamic
    /// </summary>
    public void SetMapRotationMode(bool dynamic)
    {
        if (IsFullScreen) return;

        DynamicRotation = dynamic;
        DefaultRotationMode = dynamic;
    }

    /// <summary>
    /// 
    /// </summary>
    public void GetMiniMapSize()
    {
        var root = MiniMapUI.root;
        MiniMapSize = root.sizeDelta;
        MiniMapPosition = root.anchoredPosition;
        MiniMapRotation = root.eulerAngles;
    }

    /// <summary>
    /// 
    /// </summary>
    public void GetFullMapSize()
    {
        var root = MiniMapUI.root;
        FullMapSize = root.sizeDelta;
        FullMapPosition = root.anchoredPosition;
        FullMapRotation = root.eulerAngles;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    public void RegisterItem(bl_MiniMapEntityBase item)
    {
        if (miniMapItems.Contains(item)) return;

        miniMapItems.Add(item);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    public void RemoveItem(bl_MiniMapEntityBase item)
    {
        miniMapItems.Remove(item);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float GetZoomRatio()
    {
        return DefaultHeight / Mathf.Max(Zoom, 1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float GetViewportRatio()
    {
        return MiniMapUI.MiniMapSize.GetSizeRatio() * GetZoomRatio();
    }

    /// <summary>
    /// 
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnSceneLoad()
    {
        ActiveMiniMap = null;
    }

    /// <summary>
    /// 
    /// </summary>
    public Transform Target
    {
        get
        {
            if (targetScript != null)
            {
                return targetScript.GetTarget();
            }
            return m_Target != null ? m_Target.transform : transform;
        }
        set
        {
            m_Target = value.gameObject;
        }
    }

    public Transform RotationTarget
    {
        get
        {
            if (targetScript != null)
            {
                return targetScript.GetRotationTarget();
            }
            return m_Target != null ? m_Target.transform : transform;
        }
    }

    public Vector3 TargetRotation
    {
        get
        {
            if (targetScript != null) return targetScript.GetRotationTarget().eulerAngles;
            return m_Target != null ? m_Target.transform.eulerAngles : Vector3.zero;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool HasTarget() => m_Target != null;
}