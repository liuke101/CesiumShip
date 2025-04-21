using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lovatto.MiniMap
{

    public abstract class bl_MiniMapInputBase : ScriptableObject
    {
        /// <summary>
        /// 
        /// </summary>
        public enum MiniMapInput
        {
            ZoomIn,
            ZoomOut,
            ScreenMode,
        }
        
        /// <summary>
        /// Initialize the input handler
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract bool IsInputDown(MiniMapInput key);

    }
}