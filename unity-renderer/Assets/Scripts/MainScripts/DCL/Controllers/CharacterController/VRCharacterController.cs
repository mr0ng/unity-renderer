using System;
using DCL.VR;
using UnityEngine;

public class VRCharacterController : MonoBehaviour
{
    [SerializeField]
    private Transform cameraParent;
    private readonly Vector3 offset = new Vector3(0f, -0.55f, 0f);

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
