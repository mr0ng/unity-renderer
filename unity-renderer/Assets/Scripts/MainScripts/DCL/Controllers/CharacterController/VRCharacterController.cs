using System;
using DCL.VR;
using UnityEngine;

public class VRCharacterController : MonoBehaviour
{
    [SerializeField]
    private Transform cameraParent;
    
#if (UNITY_ANDROID && !UNITY_EDITOR)
    private readonly Vector3 offset = new Vector3(0f, 0f, 0f);
#else
    private readonly Vector3 offset = new Vector3(0f, -0.55f, 0f);
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
        mixedRealityPlayspace.localRotation = Quaternion.identity;
    }
}
