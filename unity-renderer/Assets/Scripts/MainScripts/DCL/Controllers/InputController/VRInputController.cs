using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

public class VRInputController : MonoBehaviour
{
    [SerializeField]
    private Transform cameraParent;
    [SerializeField]
    private float speed = 15f;
    private InputDevice rightController;

    private void Start()
    {
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private bool GetAxis(out Vector2 value) => rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out value);

    private void Update() { CheckForRotation(); }
    
    private void CheckForRotation()
    {
        if (!rightController.isValid)
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (GetAxis(out Vector2 value))
        {
            cameraParent.eulerAngles += Time.deltaTime * speed * new Vector3(0f, value.x, 0f);
        }
    }
}
