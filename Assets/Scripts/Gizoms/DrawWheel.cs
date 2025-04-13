using System;
using UnityEngine;


public class DrawWheel : MonoBehaviour
{
    private WheelCollider wheelCollider;
    
    private void Awake()
    {
        wheelCollider = GetComponent<WheelCollider>();
    }

    private void Update()
    {
    }

    //根据wheelCollider绘制车轮型Gizoms
    private void OnDrawGizmos()
    {
        
        if (wheelCollider != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(wheelCollider.transform.position, wheelCollider.radius);
            Gizmos.DrawCube(wheelCollider.transform.position, new Vector3(wheelCollider.radius, wheelCollider.radius, wheelCollider.radius));
        }
        
    }
}
