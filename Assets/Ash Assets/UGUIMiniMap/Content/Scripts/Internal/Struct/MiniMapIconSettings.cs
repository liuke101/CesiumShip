using UnityEngine;

namespace Lovatto.MiniMap
{
    /// <summary>
    /// 
    /// </summary>
    public class MiniMapIconSettings
    {
        public Transform Target;
        public Vector3 Position;
        public Sprite Icon;
        public Color Color;
        public float Size;
        public bool Interactable;
        public ItemEffect ItemEffect;

        public MiniMapIconSettings() { }

        public MiniMapIconSettings(Vector3 position) => Position = position;

        public MiniMapIconSettings(Transform target) => Target = target;
    }
}