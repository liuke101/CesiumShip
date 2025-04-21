using Lovatto.MiniMap;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Default minimap entity handler
/// This script is attached to the game objects that we wanna show in the minimap
/// If you want to use your custom script simply inherit your script from bl_MiniMapEntityBase and use this as reference.
/// </summary>
public class bl_MiniMapEntity : bl_MiniMapEntityBase
{

    [Serializable]
    public enum IconFaceDirection
    {
        UseGlobalSetting,
        AlwaysUp,
        TargetDirection
    }

    #region Public members
    [Header("TARGET")]
    [Tooltip("Transform to UI Icon will be follow")]
    public Transform Target = null;
    [Tooltip("Custom Position from target position")]
    public Vector3 OffSet = Vector3.zero;

    [Header("ICON")]
    public bl_MiniMapIconData iconData;

    [Header("CIRCLE AREA")]
    public bool ShowCircleArea = false;
    [Range(1, 100)] public float CircleAreaRadius = 10;
    public Color CircleAreaColor = new Color(1, 1, 1, 0.9f);

    [Header("ICON BUTTON")]
    [Tooltip("UI can interact when press it?")]
    public bool isInteractable = true;
    public InteracableAction interacableAction = InteracableAction.OnHover;
    [TextArea(2, 2)] public string InfoItem = "Info Icon here";
    public bool keepTextAlwaysFacingUp = false;

    [Serializable] public class UEvent : UnityEvent { }
    public UEvent onClick = new UEvent();

    [Header("SETTINGS")]
    [Tooltip("Can Icon show when is off screen?")]
    public bool OffScreen = true;
    public bool opacityBasedDistance = false;
    public float maxDistance = 250;
    public IconFaceDirection iconFaceDirection = IconFaceDirection.UseGlobalSetting;
    public bool enableDisableWithTarget = true;
    public bool DestroyWithObject = true;
    [Range(0, 5)] public float BorderOffScreen = 0.01f;
    [Tooltip("Time before render/show item in minimap after instance")]
    [Range(0, 3)] public float RenderDelay = 0.3f;
    public ItemEffect m_Effect = ItemEffect.None;
    public GameObject CustomIconPrefab;
    #endregion

    #region Public properties
    public bl_MiniMapIconBase IconInstance { get; private set; }

    public override bool IsInteractable
    {
        get => isInteractable;
        set => isInteractable = value;
    }

    public override InteracableAction InteractAction
    {
        get => interacableAction;
        set => interacableAction = value;
    }

    public override Transform GetTarget
    {
        get => Target;
    }
    #endregion

    #region Private members
    private Image Graphic = null;
    private RectTransform GraphicRect;
    private RectTransform RectRoot;
    private GameObject cacheItem = null;
    private RectTransform CircleAreaRect = null;
    private Vector2 position;
    private bool clampedY, clampedX = false;
    private Vector2 viewPortSize, viewPortFullSize, borderRect;
    private Vector2 borderMarging = Vector2.zero;
    private Vector2 targetSize = Vector2.one;
    private bl_MiniMapIconData iconDataInstance;
    private float targetOpacity;
    private float sizeRatio = 1;
    #endregion

    /// <summary>
    /// Get all required component in start
    /// </summary>
    void Start()
    {
        if (MiniMapOwner != null)
        {
            CreateIcon();
            MiniMapOwner.RegisterItem(this);
        }
        else { Debug.Log("You need a MiniMap in scene for use MiniMap Items."); }
    }

    private void OnEnable()
    {
        if (cacheItem != null && enableDisableWithTarget) SetActiveIcon(true);
    }

    private void OnDisable()
    {
        if (cacheItem != null && enableDisableWithTarget) SetActiveIcon(false);
    }

    /// <summary>
    /// 
    /// </summary>
    void CreateIcon()
    {
        if (MiniMapOwner.hasError || !this.enabled) return;
        //Instantiate UI in canvas
        GameObject g = bl_MiniMapData.Instance.IconPrefab;
        if (CustomIconPrefab != null) { g = CustomIconPrefab; }
        cacheItem = Instantiate(g) as GameObject;
        cacheItem.name = $"Icon ({gameObject.name})";
        if (MiniMapOwner.iconsSizeRelativeToZoom) sizeRatio = MiniMapOwner.GetViewportRatio();
        RectRoot = OffScreen ? MiniMapOwner.MiniMapUI.root : MiniMapOwner.MiniMapUI.iconsPanel;
        //SetUp Icon UI
        IconInstance = cacheItem.GetComponent<bl_MiniMapIconBase>();
        IconInstance.SetUp(this);
        Graphic = IconInstance.GetImage;
        GraphicRect = Graphic.GetComponent<RectTransform>();
        if (Icon != null) { Graphic.sprite = Icon; Graphic.color = IconColor; }
        cacheItem.transform.SetParent(RectRoot.transform, false);
        GraphicRect.anchoredPosition = Vector2.zero;
        if (Target == null) { Target = transform; }
        StartEffect();
        IconInstance.SpawnedDelayed(RenderDelay);
        IconInstance.SetText(InfoItem);
        viewPortFullSize = MiniMapOwner.MiniMapUI.mapTextureRender.GetRectSize();
        viewPortSize = viewPortFullSize * 0.5f;
        borderRect = RectRoot.rect.size * 0.5f;
        borderMarging = Vector2.one * BorderOffScreen;
        if (ShowCircleArea)
        {
            CircleAreaRect = IconInstance.SetCircleArea(CircleAreaRadius * sizeRatio, CircleAreaColor);
        }
    }

    /// <summary>
    /// Transfer this icon to a new minimap rig
    /// </summary>
    /// <param name="newOwner"></param>
    public override void ChangeMiniMapOwner(bl_MiniMap newOwner)
    {
        _minimap = newOwner;
        RectRoot = OffScreen ? MiniMapOwner.MiniMapUI.root : MiniMapOwner.MiniMapUI.iconsPanel;
        cacheItem.transform.SetParent(RectRoot.transform, false);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdateItem()
    {
        //If a component missing, return for avoid bugs.
        if (Target == null || MiniMapOwner == null)
            return;
        if (Graphic == null)
            return;

        if (Time.frameCount % 30 == 0 || MiniMapOwner.HighPrecisionMode) OnFrameUpdate();
        IconControl();
        OpacityControl();
    }

    /// <summary>
    /// 
    /// </summary>
    void IconControl()
    {
        //Setting the modify position
        Vector3 CorrectPosition = TargetPosition + OffSet;
        //Convert the position of target in ViewPortPoint
        Vector2 wvp = MiniMapOwner.miniMapCamera.WorldToViewportPoint(CorrectPosition);
        //Calculate the position of target and convert into position of screen
        position.Set((wvp.x * viewPortFullSize.x) - viewPortSize.x, (wvp.y * viewPortFullSize.y) - viewPortSize.y);
        Vector2 UnClampPosition = position;

        //calculate the position of UI again, determine if off screen
        //if off screen reduce the size
        float Iconsize = Size;
        if (OffScreen)
        {
            if (MiniMapOwner.mapShape == MiniMapMapShape.Circle)
            {
                // Clamp the icon position inside the circle area
                if (position.sqrMagnitude > (MiniMapOwner.CompassSize * MiniMapOwner.CompassSize))
                {
                    position = position.normalized * MiniMapOwner.CompassSize;
                    Iconsize = IconOffScreenSize;
                }
                else
                {
                    Iconsize = Size * sizeRatio;
                }
            }
            else
            {
                Vector2 border = borderRect + borderMarging;
                position.x = bl_MiniMapUtils.ClampBorders(position.x, -border.x, border.x, out clampedX);
                position.y = bl_MiniMapUtils.ClampBorders(position.y, -border.y, border.y, out clampedY);
                //check if the icon is out of the minimap bounds
                Iconsize = clampedX || clampedY ? IconOffScreenSize : Size * sizeRatio;
            }
        }
        else
        {
            Iconsize *= sizeRatio;
        }

        //Apply position to the UI (for follow)
        GraphicRect.anchoredPosition = position;
        if (CircleAreaRect != null) { CircleAreaRect.anchoredPosition = UnClampPosition; }

        Iconsize = Mathf.Max(Iconsize, 5);
        //Change size with smooth transition
        targetSize = Vector2.one * (Iconsize * MiniMapOwner.IconMultiplier);
        GraphicRect.sizeDelta = Vector2.Lerp(GraphicRect.sizeDelta, targetSize, Time.deltaTime * 8);

        if (GetFaceDirection() == IconFaceDirection.AlwaysUp)
        {
            //with this the icon rotation always will facing up
            if (MiniMapOwner.canvasRenderMode == MiniMapRenderMode.Mode2D) { GraphicRect.up = Vector3.up; }
            else
            {
                Quaternion r = Quaternion.identity;
                r.x = Target.rotation.x;
                GraphicRect.localRotation = r;
            }
        }
        else
        {
            //with this the rotation icon will depend of target
            Vector3 re = MiniMapOwner.canvasRenderMode == MiniMapRenderMode.Mode2D ? Vector3.zero : GraphicRect.eulerAngles;
            //Fix player rotation for apply to el icon.
            re.z = ((-Target.rotation.eulerAngles.y) + MiniMapOwner.minimapRig.eulerAngles.y);
            GraphicRect.rotation = Quaternion.Euler(re);

            if (keepTextAlwaysFacingUp) IconInstance.ForceFaceUp();
        }
    }

    /// <summary>
    /// Called each target frame rate
    /// </summary>
    void OnFrameUpdate()
    {
        viewPortFullSize = MiniMapOwner.MiniMapUI.mapTextureRender.GetRectSize();
        viewPortSize = viewPortFullSize * 0.5f;
        borderRect = RectRoot.rect.size * 0.5f;

        if (opacityBasedDistance && MiniMapOwner != null && MiniMapOwner.HasTarget())
        {
            if (MiniMapOwner.IsFullScreen)
            {
                targetOpacity = 1;
            }
            else
            {
                float targetDistance = Vector3.Distance(TargetPosition, MiniMapOwner.Target.position);
                float percentage = targetDistance / maxDistance;
                percentage = Math.Min(1, percentage);
                if (percentage < 0.2f) percentage = 0;
                targetOpacity = 1 - percentage;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OpacityControl()
    {
        if (!opacityBasedDistance || IconInstance == null) return;

        IconInstance.Opacity = Mathf.Lerp(IconInstance.Opacity, targetOpacity, Time.deltaTime * 8);
    }

    /// <summary>
    /// 
    /// </summary>
    void StartEffect()
    {
        if (!Graphic.TryGetComponent<Animator>(out var a)) return;

        if (m_Effect == ItemEffect.Pulsing)
        {
            a.SetInteger("Type", 2);
        }
        else if (m_Effect == ItemEffect.Fade)
        {
            a.SetInteger("Type", 1);
        }
        else
        {
            Destroy(a);
        }
    }

    /// <summary>
    /// When player or the target die, disable,remove,etc..
    /// call this for remove the item UI from Map
    /// for change to other icon and disable in certain time
    /// or destroy immediate
    /// </summary>
    /// <param name="inmediate"></param>
    public override void DestroyIcon(bool inmediate)
    {
        if (IconInstance == null) return;

        if (DeathIcon == null || inmediate)
        {
            IconInstance.DestroyIcon(inmediate);
        }
        else
        {
            IconInstance.DestroyIcon(inmediate, DeathIcon);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newTarget"></param>
    public override void SetTarget(Transform newTarget)
    {
        Target = newTarget;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ico"></param>
    public override void SetIcon(Sprite newIcon)
    {
        if (IconInstance == null) return;

        IconInstance.SetIcon(newIcon);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newIconColor"></param>
    public override void SetIconColor(Color newIconColor)
    {
        IconColor = newIconColor;

        if (IconInstance == null) return;

        IconInstance.SetColor(newIconColor);
    }

    /// <summary>
    /// Show a visible circle area in the minimap with this
    /// item as center
    /// </summary>
    public override void SetCircleArea(float radius, Color AreaColor)
    {
        if (IconInstance == null) return;

        CircleAreaRect = IconInstance.SetCircleArea(radius, AreaColor);
    }

    /// <summary>
    /// 
    /// </summary>
    public void HideCircleArea()
    {
        if (IconInstance == null) return;

        IconInstance.SetActiveCircleArea(false);
        CircleAreaRect = null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public override void SetActiveIcon(bool active)
    {
        if (active) ShowItem();
        else HideItem();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public override void SetActiveCircleArea(bool active)
    {
        if (IconInstance == null) return;

        IconInstance.SetActiveCircleArea(active);
    }

    /// <summary>
    /// Call this for hide item in miniMap
    /// For show again just call "ShowItem()"
    /// NOTE: For destroy item call "DestroyItem(bool immediate)" instant this.
    /// </summary>
    public void HideItem()
    {
        if (cacheItem != null)
        {
            cacheItem.SetActive(false);
        }
        else
        {
            Debug.Log("There is no item to disable.");
        }
    }

    /// <summary>
    /// Call this for show again the item in miniMap when is hide
    /// </summary>
    public void ShowItem()
    {
        if (IconInstance != null)
        {
            cacheItem.SetActive(true);
            IconInstance.SetOpacity(1);
        }
        else
        {
            Debug.Log("There is no item to active.");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetIconText(string text)
    {
        if (IconInstance == null) return;
        IconInstance.SetText(text);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="iconSettings"></param>
    public override void SetIconSettings(MiniMapIconSettings iconSettings)
    {
        Icon = iconSettings.Icon;
        IconColor = iconSettings.Color;
        Target = iconSettings.Target;
        Size = iconSettings.Size;
        m_Effect = iconSettings.ItemEffect;
        isInteractable = iconSettings.Interactable;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnViewportChanged(float viewPortRatio)
    {
        if (!MiniMapOwner.iconsSizeRelativeToZoom) return;

        sizeRatio = viewPortRatio;
        if (ShowCircleArea)
        {
            CircleAreaRect = IconInstance.SetCircleArea(CircleAreaRadius * sizeRatio, CircleAreaColor);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool InvokeOnClickEvent()
    {
        if (onClick == null) { return false; }
        onClick.Invoke();
        return true;
    }

    /// <summary>
    /// If you need destroy icon when this gameObject is destroy.
    /// </summary>
    void OnDestroy()
    {
        if (MiniMapOwner != null) MiniMapOwner.RemoveItem(this);
        if (DestroyWithObject)
        {
            DestroyIcon(true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public Vector3 TargetPosition
    {
        get
        {
            return Target == null ? Vector3.zero : new Vector3(Target.position.x, 0, Target.position.z);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IconFaceDirection GetFaceDirection()
    {
        return iconFaceDirection == IconFaceDirection.UseGlobalSetting
            ? MiniMapOwner.iconsAlwaysFacingUp ? IconFaceDirection.AlwaysUp : IconFaceDirection.TargetDirection
            : iconFaceDirection;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Sprite Icon
    {
        get
        {
            return iconDataInstance != null ? iconDataInstance.Icon : iconData == null ? null : iconData.Icon;
        }
        set
        {
            if (iconData == null)
            {
                iconData = bl_MiniMapData.Instance.emptyIconData;
            }

            if (iconDataInstance == null) iconDataInstance = Instantiate(iconData);
            iconDataInstance.Icon = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Sprite DeathIcon
    {
        get
        {
            return iconDataInstance != null ? iconDataInstance.DeathIcon : iconData == null ? null : iconData.DeathIcon;
        }
        set
        {
            if (iconData == null)
            {
                iconData = bl_MiniMapData.Instance.emptyIconData;
            }

            if (iconDataInstance == null) iconDataInstance = Instantiate(iconData);
            iconDataInstance.DeathIcon = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Color IconColor
    {
        get
        {
            return iconDataInstance != null ? iconDataInstance.IconColor : iconData == null ? Color.white : iconData.IconColor;
        }
        set
        {
            if (iconData == null)
            {
                iconData = bl_MiniMapData.Instance.emptyIconData;
            }

            if (iconDataInstance == null) iconDataInstance = Instantiate(iconData);
            iconDataInstance.IconColor = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float Size
    {
        get
        {
            return iconDataInstance != null ? iconDataInstance.Size : iconData == null ? 0 : iconData.Size;
        }
        set
        {
            if (iconData == null)
            {
                iconData = bl_MiniMapData.Instance.emptyIconData;
            }

            if (iconDataInstance == null) iconDataInstance = Instantiate(iconData);
            iconDataInstance.Size = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float IconOffScreenSize
    {
        get
        {
            return iconDataInstance != null ? iconDataInstance.OffScreenSize : iconData == null ? 0 : iconData.OffScreenSize;
        }
        set
        {
            if (iconData == null)
            {
                iconData = bl_MiniMapData.Instance.emptyIconData;
            }

            if (iconDataInstance == null) iconDataInstance = Instantiate(iconData);
            iconDataInstance.OffScreenSize = value;
        }
    }

    private bl_MiniMap _minimap = null;
    private bl_MiniMap MiniMapOwner
    {
        get
        {
            if (_minimap == null)
            {
                _minimap = bl_MiniMapUtils.GetMiniMap();
            }
            return _minimap;
        }
    }
}