using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

public class SeanTestScript : MonoBehaviour 
{

public float speed = 0.0005f;
    public bool toggleInput = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float axisX = -Input.GetAxis("AXIS_4");
        float axisY = -Input.GetAxis("AXIS_5");
        if (toggleInput && (Math.Abs(axisX)>0.1f || Math.Abs(axisY) > 0.1f))
        {
            //Debug.Log("X: " + axisX + " Y: " + axisY);
            //transform.parent.position = transform.parent.position + transform.forward * new Vector3(speed*axisX,0,-speed*axisY);
            transform.parent.position = transform.parent.position + new Vector3(transform.forward.x,0f, transform.forward.z) * axisY*speed;
            transform.parent.position = transform.parent.position + Vector3.Cross(transform.forward,Vector3.up) * axisX * speed;
        }
        
    }


}
