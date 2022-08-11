using System;
using DCL.VR;
using UnityEngine;

public class VRCharacterController : MonoBehaviour
{
    [SerializeField]
    private Transform cameraParent;
    
#if (UNITY_ANDROID && !UNITY_EDITOR)
    private readonly Vector3 offset = new Vector3(0f, 0.55f, 0f);
#else
    // private readonly Vector3 offset = new Vector3(0f, -0.55f, 0f);
    private readonly Vector3 offset = new Vector3(0f, 0.55f, 0f);
#endif
    private Transform mixedRealityPlayspace;

    private void Start()
    {
        mixedRealityPlayspace = VRPlaySpace.i.transform;
        PlaceCamera();
    }
    
    private void PlaceCamera()
    {
        mixedRealityPlayspace.parent = cameraParent;
        mixedRealityPlayspace.localPosition = offset;
        var canvas = GameObject.Find("Canvas");
        #if UNITY_ANDROID && !UNITY_EDITOR
        canvas.transform.localPosition += Vector3.down;
        #endif
        //var mainCamera = mixedRealityPlayspace.GetComponentInChildren<Camera>();
        // mainCamera.transform.localPosition = new Vector3(0, 1.5f, 0);
        mixedRealityPlayspace.localRotation = Quaternion.identity;
    }
}
