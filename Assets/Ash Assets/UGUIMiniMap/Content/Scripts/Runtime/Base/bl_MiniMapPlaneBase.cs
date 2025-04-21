using UnityEngine;

namespace Lovatto.MiniMap
{
    public abstract class bl_MiniMapPlaneBase : MonoBehaviour
    {
        /// <summary>
        /// Setup the Minimap plane
        /// </summary>
        /// <param name="minimap"></param>
        public abstract void Setup(bl_MiniMap minimap);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public abstract void SetActive(bool active);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public virtual void SetActiveGrid(bool active) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newTexture"></param>
        public virtual void SetMapTexture(Texture2D newTexture) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        public virtual void SetGridSize(float size) { }
    }
}