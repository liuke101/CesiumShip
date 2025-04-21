using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lovatto.MiniMap
{
    public abstract class bl_MapPointerBase : MonoBehaviour
    {
        /// <summary>
        /// Apply a custom color to the pointer
        /// </summary>
        /// <param name="color"></param>
        public abstract void SetColor(Color color);
    }
}