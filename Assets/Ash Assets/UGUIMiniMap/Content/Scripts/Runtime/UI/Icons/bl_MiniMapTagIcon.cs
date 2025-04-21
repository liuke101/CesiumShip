using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lovatto.MiniMap
{
    public sealed class bl_MiniMapTagIcon : bl_MiniMapIconBase
    {
        [SerializeField] private Image tagImg = null;
        [SerializeField] private Text tagText = null;
        [SerializeField] private CanvasGroup rootGroup = null;

        private float maxOpacity = 1;
        private float delay = 0;

        /// <summary>
        /// 
        /// </summary>
        public override Image GetImage => tagImg;

        /// <summary>
        /// 
        /// </summary>
        public override float Opacity
        {
            get => rootGroup.alpha; set
            {
                rootGroup.alpha = value * maxOpacity;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        public override void SetUp(bl_MiniMapEntityBase entity)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public override void SetActive(bool active)
        {
           gameObject.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opacity"></param>
        public override void SetOpacity(float opacity)
        {
            rootGroup.alpha = opacity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public override void SetText(string text)
        {
            tagText.text = text;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delay"></param>
        public override void SpawnedDelayed(float wdelay)
        {
            delay = wdelay;
            StartCoroutine(FadeIcon());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator FadeIcon()
        {
            yield return new WaitForSeconds(delay);
            float d = 0;
            while (d < 1)
            {
                d += Time.deltaTime * 2;
                rootGroup.alpha = maxOpacity * d;
                yield return null;
            }
        }
    }
}