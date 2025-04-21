using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Lovatto.MiniMap
{
    public class bl_MiniMapUI : MonoBehaviour
    {
        public RectTransform root;
        public CanvasGroup rootAlpha;
        public bl_MiniMapPlayerIcon playerIcon;
        public RectTransform iconsPanel;
        public bl_MiniMapTexture mapTextureRender = null;
        public bl_MiniMapMaskHandler minimapMaskManager;
        public Animator hitAnimator;
        public float hitEffectSpeed = 1.5f;
        [SerializeField] private Graphic[] backgroundGraphics = null;


        private bl_MiniMapSize _miniMapSize = null;
        public bl_MiniMapSize MiniMapSize
        {
            get
            {
                if (_miniMapSize == null)
                {
                    _miniMapSize = GetComponentInChildren<bl_MiniMapSize>(true);
                }
                return _miniMapSize;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="miniMap"></param>
        public void Setup(bl_MiniMap miniMap)
        {
            if (playerIcon != null) playerIcon.ApplyMiniMapSettings(miniMap);
            SetBackgroundOpacity(Minimap.backgroundOpacity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            root.gameObject.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        public void DoStartFade(float delay, Action callback)
        {
            if (rootAlpha == null) return;

            StopAllCoroutines();
            StartCoroutine(Fade());

            IEnumerator Fade()
            {
                rootAlpha.alpha = 0;
                yield return new WaitForSeconds(delay);
                while (rootAlpha.alpha < Minimap.overallOpacity)
                {
                    rootAlpha.alpha += Time.deltaTime;
                    yield return null;
                }
                rootAlpha.alpha = Minimap.overallOpacity;
                callback?.Invoke();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetIcon"></param>
        public void ConfigureWorldTarget(bl_MiniMapEntityBase targetIcon)
        {
            if (playerIcon == null)
                return;

            MiniMapIconSettings settings = playerIcon.GetIconSettings();
            settings.Target = targetIcon.transform;
            settings.ItemEffect = ItemEffect.None;
            settings.Interactable = false;

            targetIcon.SetIconSettings(settings);
        }

        /// <summary>
        /// 
        /// </summary>
        public void DoHitEffect()
        {
            if (hitAnimator == null) return;

            hitAnimator.speed = hitEffectSpeed;
            hitAnimator.Play("HitEffect", 0, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnZoomClick(bool zoomIn)
        {
            Minimap.ChangeZoom(zoomIn);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnCentralizeClick()
        {
            Minimap.GoToTarget();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opacity"></param>
        public void SetBackgroundOpacity(float opacity)
        {
            foreach (var g in backgroundGraphics)
            {
                if (g == null) continue;
                g.canvasRenderer.SetAlpha(opacity);
            }
        }

        private bl_MiniMap _minimap;
        public bl_MiniMap Minimap
        {
            get
            {
                if (_minimap == null)
                {
                    _minimap = GetComponentInParent<bl_MiniMap>();
                }
                return _minimap;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public RectTransform PlayerIconTransform
        {
            get
            {
                return playerIcon != null ? playerIcon.IconTransform : null;
            }
        }
    }
}