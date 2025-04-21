using UnityEngine;

namespace Lovatto.MiniMap
{
    [CreateAssetMenu(menuName = "Lovatto/MiniMap/Input Handler Legacy")]
    public class bl_MiniMapInputLegacy : bl_MiniMapInputBase
    {
        public KeyCode screenModeKey = KeyCode.M;
        public KeyCode zoomInKey = KeyCode.KeypadPlus;
        public KeyCode zoomOutKey = KeyCode.KeypadMinus;
        
        /// <summary>
        /// 
        /// </summary>
        public override void Init()
        {
           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override bool IsInputDown(MiniMapInput key)
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            switch (key)
            {
                case MiniMapInput.ZoomIn:
                    return Input.GetKeyDown(zoomInKey);
                case MiniMapInput.ZoomOut:
                    return Input.GetKeyDown(zoomOutKey);
                case MiniMapInput.ScreenMode:
                    return Input.GetKeyDown(screenModeKey);
                default:
                    return false;
            }
#else
            return false;
#endif

        }
    }
}