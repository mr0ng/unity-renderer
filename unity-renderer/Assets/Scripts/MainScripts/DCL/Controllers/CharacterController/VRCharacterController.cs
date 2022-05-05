using System;
using DCL.VR;
using UnityEngine;

public class VRCharacterController : MonoBehaviour
{
    [SerializeField]
    private Transform cameraParent;
    private readonly Vector3 offset = new Vector3(0f, -0.475f, 0f);

    private Transform mixedRealityPlayspace;

    private void Start()
    {
        PlaceCamera();
    }
    
    private void PlaceCamera()
    {
        VRPlaySpace playSpace = VRPlaySpace.i;
        mixedRealityPlayspace = playSpace.transform;

        mixedRealityPlayspace.position = cameraParent.position + offset;
        mixedRealityPlayspace.parent = cameraParent;
    }
}
