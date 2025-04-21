using UnityEngine;

namespace Lovatto.MiniMap
{
    public sealed class bl_MapPointer : bl_MapPointerBase
    {

        public string PlayerTag = "Player";
        public AudioClip SpawnSound;
        public MeshRenderer m_Render;
        private AudioSource ASource;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if (SpawnSound != null)
            {
                ASource = GetComponent<AudioSource>();
                ASource.clip = SpawnSound;
                ASource.Play();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        public override void SetColor(Color c)
        {
            if (m_Render == null) return;

            GetComponent<bl_MiniMapEntityBase>().SetIconColor(c);
            c.a = 0.25f;
            m_Render.material.SetColor("_TintColor", c);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        void OnTriggerEnter(Collider c)
        {
            if (c.CompareTag(PlayerTag))
            {
                Destroy();
            }
        }

        /// <summary>
        /// Destroy the pointer
        /// </summary>
        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}