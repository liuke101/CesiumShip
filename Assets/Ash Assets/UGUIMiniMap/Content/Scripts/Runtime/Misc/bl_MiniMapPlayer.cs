using UnityEngine;

namespace Lovatto.MiniMap
{
    /// <summary>
    /// Attach this script to the game player only
    /// This will automatically assign the player to the minimap
    /// </summary>
    public class bl_MiniMapPlayer : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            AssignAsTarget();
        }

        /// <summary>
        /// 
        /// </summary>
        public void AssignAsTarget()
        {
            var miniMap = bl_MiniMap.ActiveMiniMap;
            if (miniMap == null) return;

            miniMap.SetTarget(gameObject);
        }
    }
}