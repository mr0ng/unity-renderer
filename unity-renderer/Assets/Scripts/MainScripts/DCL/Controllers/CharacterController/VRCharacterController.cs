using DCL.VR;
using UnityEngine;

public class VRCharacterController : MonoBehaviour
{
    [SerializeField]
    private Transform cameraParent;

    private void Start()
    {
        PlaceCamera();
    }
    
    private void PlaceCamera()
    {
        // this might be obsolete
        VRPlaySpace playSpace = VRPlaySpace.i;
        Transform mixedRealityPlayspace = playSpace.transform;

        //Move and Reparent the MRPlaySpace to the characterController so that we always have our controllers with us
        mixedRealityPlayspace.position = cameraParent.position;
        mixedRealityPlayspace.parent = cameraParent;
    }
}
