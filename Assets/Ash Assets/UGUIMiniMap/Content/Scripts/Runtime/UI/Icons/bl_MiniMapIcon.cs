using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lovatto.MiniMap
{
    /// <summary>
    /// Default minimap icon handler
    /// This script setup the image icon in the canvas
    /// If you want to use your custom script simply inherit your script from bl_MiniMapIconBase
    /// </summary>
    public sealed class bl_MiniMapIcon : bl_MiniMapIconBase, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {

        #region Public members
        [Header("SETTINGS")]
        public float DestroyIn = 5f;
        [Header("REFERENCES")]
        public Image TargetGraphic;
        [SerializeField] private RectTransform CircleAreaRect = null;
        public Sprite DeathIcon = null;
        public GameObject textAreaBackground;
        [SerializeField] private Text InfoText = null;
        public CanvasGroup m_CanvasGroup;
        public CanvasGroup infoAlpha;
        #endregion

        #region Private members
        private Animator Anim;
        private float delay = 0.1f;
        private bl_MiniMapMaskHandler MaskHelper = null;
        private bl_MiniMapEntityBase miniMapItem;
        private bool isTextOpen = false;
        private float maxOpacity = 1;
        private float timeSinceInstantiated;
        #endregion

        #region Public properties
        public RectTransform textRect { get; private set; }
        public override Image GetImage => TargetGraphic;

        public override float Opacity
        {
            get => m_CanvasGroup.alpha; set
            {
                m_CanvasGroup.alpha = value * maxOpacity;
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            //Get the canvas group or add one if nt have.
            if (m_CanvasGroup == null)
            {
                m_CanvasGroup = GetComponent<CanvasGroup>();
            }
            if (GetComponent<Animator>() != null)
            {
                Anim = GetComponent<Animator>();
            }
            if (Anim != null) { Anim.enabled = false; }
            if (textAreaBackground != null) { textRect = textAreaBackground.GetComponent<RectTransform>(); textAreaBackground.SetActive(false); }
            m_CanvasGroup.alpha = 0;
            if (CircleAreaRect != null) { CircleAreaRect.gameObject.SetActive(false); }

            timeSinceInstantiated = Time.time;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public override void SetUp(bl_MiniMapEntityBase item)
        {
            miniMapItem = item;
            m_CanvasGroup.interactable = miniMapItem.IsInteractable;
        }

        /// <summary>
        /// When player or the target die,desactive,remove,etc..
        /// call this for remove the item UI from Map
        /// for change to other icon and desactive in certain time
        /// or destroy immediate
        /// </summary>
        /// <param name="inmediate"></param>
        /// <param name="death"></param>
        public override void DestroyIcon(bool inmediate, Sprite death)
        {
            if (inmediate)
            {
                Destroy(gameObject);
            }
            else
            {
                //Change the sprite to icon death
                TargetGraphic.sprite = death == null ? DeathIcon : death;
                //destroy in 5 seconds
                Destroy(gameObject, DestroyIn);
            }
        }
        /// <summary>
        /// Get info to display
        /// </summary>
        /// <param name="info"></param>
        public override void SetText(string info)
        {
            if (InfoText == null)
                return;

            InfoText.text = info;
            if (textAreaBackground != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(textRect);
                LayoutRebuilder.ForceRebuildLayoutImmediate(InfoText.GetComponent<RectTransform>());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ico"></param>
        public override void SetIcon(Sprite ico)
        {
            TargetGraphic.sprite = ico;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newColor"></param>
        public override void SetColor(Color newColor)
        {
            TargetGraphic.color = newColor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public override void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        /// <summary>
        /// Show a visible circle area in the minimap with this
        /// item as center
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="AreaColor"></param>
        public override RectTransform SetCircleArea(float radius, Color AreaColor)
        {
            if (CircleAreaRect == null) { return null; }

            var miniMap = bl_MiniMapUtils.GetMiniMap();
            MaskHelper = miniMap.MiniMapUI.minimapMaskManager;
            MaskHelper.SetMaskedIcon(CircleAreaRect);
            radius *= 10;
            radius *= miniMap.IconMultiplier;
            Vector2 r = new Vector2(radius, radius);
            CircleAreaRect.sizeDelta = r;
            CircleAreaRect.GetComponent<Image>().CrossFadeColor(AreaColor, 1, true, true);
            CircleAreaRect.gameObject.SetActive(true);

            return CircleAreaRect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public override void SetActiveCircleArea(bool active)
        {
            if (active)
            {
                MaskHelper.SetMaskedIcon(CircleAreaRect);
                CircleAreaRect.gameObject.SetActive(true);
            }
            else
            {
                CircleAreaRect.SetParent(transform);
                CircleAreaRect.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opacity"></param>
        public override void SetOpacity(float opacity)
        {
            if (m_CanvasGroup == null) return;

            maxOpacity = opacity;
            m_CanvasGroup.alpha = opacity;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void ForceFaceUp()
        {
            if (textAreaBackground != null)
            {
                textRect.up = Vector3.up;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator FadeIcon()
        {
            yield return new WaitForSeconds(delay);
            float d = 0;
            while (d < 1)
            {
                d += Time.deltaTime * 2;
                m_CanvasGroup.alpha = maxOpacity * d;
                yield return null;
            }
            if (Anim != null) { Anim.enabled = true; }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Time.time - timeSinceInstantiated < 0.5f) return; // prevent click on spawn (0.5s delay)
            if (miniMapItem == null) return;
            if (!miniMapItem.IsInteractable || miniMapItem.InteractAction != bl_MiniMapEntityBase.InteracableAction.OnHover) return;
            OnInteract(true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Time.time - timeSinceInstantiated < 0.5f) return; // prevent click on spawn (0.5s delay)
            if (miniMapItem == null) return;
            if (!miniMapItem.IsInteractable || miniMapItem.InteractAction != bl_MiniMapEntityBase.InteracableAction.OnTouch) return;
            OnInteract();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (miniMapItem == null) return;
            if (!miniMapItem.IsInteractable || miniMapItem.InteractAction != bl_MiniMapEntityBase.InteracableAction.OnHover) return;
            OnInteract(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnInteract()
        {
            isTextOpen = !isTextOpen;
            OnInteract(isTextOpen);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnInteract(bool open)
        {
            // if there is a custom event, invoke it and return
            if (miniMapItem.InvokeOnClickEvent()) return;

            StopCoroutine(nameof(FadeInfo));
            StartCoroutine(nameof(FadeInfo), !open);
        }

        /// <summary>
        /// 
        /// </summary>
        IEnumerator FadeInfo(bool fadeOut)
        {
            if (!fadeOut) { textAreaBackground.SetActive(true); }
            float d = 0;
            while (d < 1)
            {
                d += Time.deltaTime * 4;
                infoAlpha.alpha = fadeOut ? Mathf.Lerp(1, 0, d) : Mathf.Lerp(0, 1, d);
                yield return null;
            }
            if (fadeOut) { textAreaBackground.SetActive(false); }
        }

        public override void SpawnedDelayed(float v) { delay = v; StartCoroutine(FadeIcon()); }
    }
}