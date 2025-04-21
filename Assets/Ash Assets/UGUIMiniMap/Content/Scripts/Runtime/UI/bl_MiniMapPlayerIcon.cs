using UnityEngine;
using UnityEngine.UI;

namespace Lovatto.MiniMap
{
    public class bl_MiniMapPlayerIcon : MonoBehaviour
    {
        [SerializeField] private RectTransform root = null;
        [SerializeField] private Image iconImage = null;
        [SerializeField] private GameObject viewAreaUI = null;

        private Sprite defaultIcon;

        /// <summary>
        /// Set active the player icon
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            if (root == null) return;

            root.gameObject.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void SetActiveVisibleArea(bool active)
        {
            if (viewAreaUI == null) return;

            viewAreaUI.SetActive(active);
        }

        /// <summary>
        /// Set the tint color for the player icon
        /// </summary>
        /// <param name="color"></param>
        public void SetColor(Color color)
        {
            if (iconImage == null) return;

            iconImage.color = color;
        }

        /// <summary>
        /// Change the player icon sprite
        /// Set null to set back to the default icon
        /// </summary>
        /// <param name="newIcon"></param>
        public void SetIcon(Sprite newIcon, bool overrideDefault = false)
        {
            if (iconImage == null) return;

            if (newIcon == null)
            {
                if (defaultIcon == null && iconImage.sprite != null) return;

                iconImage.sprite = defaultIcon;
                return;
            }

            if (defaultIcon == null && !overrideDefault) defaultIcon = iconImage.sprite;
            iconImage.sprite = newIcon;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        public void SetSize(float size)
        {
            if (root == null) return;

            size = Mathf.Max(5, size);
            Vector2 newSize = Vector2.one * size;
            root.sizeDelta = newSize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="miniMap"></param>
        public void ApplyMiniMapSettings(bl_MiniMap miniMap)
        {
            SetIcon(miniMap.PlayerIconSprite, true);
            SetColor(miniMap.playerColor);
            SetActive(miniMap.mapMode == MiniMapMapType.Local);
        }

        /// <summary>
        /// 
        /// </summary>
        public RectTransform IconTransform
        {
            get => root;
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector2 Position
        {
            get => root.anchoredPosition;
            set => root.anchoredPosition = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MiniMapIconSettings GetIconSettings()
        {
            return new MiniMapIconSettings()
            {
                Icon = iconImage.sprite,
                Color = iconImage.color,
                Size = root.sizeDelta.x + 2,
            };
        }
    }
}