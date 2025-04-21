using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lovatto.MiniMap
{
    public class bl_DamageBoxDemo : MonoBehaviour
    {

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                bl_MiniMap.ActiveMiniMap.DoHitEffect();
            }
        }
    }
}