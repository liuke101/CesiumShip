using UnityEngine;

namespace Lovatto.MiniMap
{
    public class bl_MiniMapOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject content = null;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            SetActive(false);
            transform.SetAsFirstSibling();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            instance = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            if (content != null)
            {
                content.SetActive(active);
            }
        }

        private static bl_MiniMapOverlay instance;
        public static bl_MiniMapOverlay Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<bl_MiniMapOverlay>();
                }
                return instance;
            }
        }
    }
}