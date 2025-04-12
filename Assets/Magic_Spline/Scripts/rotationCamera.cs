using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotationCamera : MonoBehaviour {

    Vector3 startRotation;

    [Header("Down bound angle X")]
    [Range(-180, 180)]
    public float MinX = 0f;

    [Header("Upper bound angle X")]
    [Range(-180, 180)]
    public float MaxX = 80f;

    float rotX;
    float rotY;
    public float Speed = 1;

    void Start()
    {
        startRotation = transform.eulerAngles;
        rotX = startRotation.y;
        rotY = startRotation.x;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            rotX += Input.GetAxis("Mouse X") * Speed;
            if(rotY > MinX && rotY < MaxX)
            {
                rotY += Input.GetAxis("Mouse Y") * -Speed;
            }
            else
            {
                if(rotY <= MinX)
                {
                    if(Input.GetAxis("Mouse Y") < 0)
                    {
                        rotY += Input.GetAxis("Mouse Y") * -Speed;
                    }
                }
                if (rotY >= MaxX)
                {
                    if (Input.GetAxis("Mouse Y") > 0)
                    {
                        rotY += Input.GetAxis("Mouse Y") * -Speed;
                    }
                }
            }
         
            
        }
 

        transform.eulerAngles = new Vector3(rotY, rotX, 0);




    }
}
