using UnityEngine;
using UnityEngine.UI;

namespace Lovatto.MiniMap
{
    public abstract class bl_MiniMapIconBase : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract Image GetImage
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract float Opacity
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract void SetUp(bl_MiniMapEntityBase entity);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newIcon"></param>
        public virtual void SetIcon(Sprite newIcon)
        {
            GetImage.sprite = newIcon;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public abstract void SetText(string text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newColor"></param>
        public virtual void SetColor(Color newColor)
        {
            GetImage.color = newColor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opacity"></param>
        public abstract void SetOpacity(float opacity);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="AreaColor"></param>
        /// <returns></returns>
        public virtual RectTransform SetCircleArea(float radius, Color AreaColor) { return null; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public abstract void SetActive(bool active);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public virtual void SetActiveCircleArea(bool active) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inmediate"></param>
        public virtual void DestroyIcon(bool inmediate, Sprite overrideDeathIcon = null)
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delay"></param>
        public abstract void SpawnedDelayed(float delay);

        /// <summary>
        /// Force the icon always face up no matter the minimap rotation.
        /// </summary>
        public virtual void ForceFaceUp() { }
    }
}