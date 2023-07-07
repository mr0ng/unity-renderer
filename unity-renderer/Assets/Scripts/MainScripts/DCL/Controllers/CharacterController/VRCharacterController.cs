using DCL.VR;
using UnityEngine;
using DCL;

public class VRCharacterController : MonoBehaviour
{
    [SerializeField]
    private Transform cameraParent;
    [SerializeField]
    private BooleanVariable menuOpen;
    private Transform mainCamera;
    private Vector3 cameraDifferencePos;
    private Vector3 cameraDifferenceLocPos;
    private Vector3 lastCameraPos;
    private Vector3 lastCameraLocPos;
    //test variables for HMD motion.
    // public float one = -1;
    // public float two = -1;
    // public float three = -1;
    // public float four = 0;
    // public float five = 0;
    // public float six = 0;
    // public float seven = 0;
    // public float eight = 0;
    // public float nine = 0;

    [SerializeField]
    private float waitTimeSec = 0.05f;
    [SerializeField]
    private float distanceThreshold = 0.1f;

    private WaitForSeconds waitTime;
    private readonly DataStore_Player dataStorePlayer = DataStore.i.player;
 //#if (UNITY_ANDROID && !UNITY_EDITOR)
//     private readonly Vector3 offset = new Vector3(0f, 0.55f, 0f);
// #else
    private readonly Vector3 offset = new Vector3(0f, -0.85f, 0f);
// #endif
    private Transform mixedRealityPlayspace;

    private void Start()
    {
        #if !DCL_VR
        return;
        #endif
        mixedRealityPlayspace = VRPlaySpace.i.transform;
        menuOpen.OnChange += MenuOpened;
        PlaceCamera();
        mainCamera = Camera.main.transform;
        cameraDifferencePos = mainCamera.position;
        cameraDifferenceLocPos = mainCamera.localPosition;
        lastCameraPos = mainCamera.position;
        lastCameraLocPos = mainCamera.localPosition;
    }
    private void LateUpdate()
    {
#if !DCL_VR
        return;
#endif
        // while (true)
        // {
            waitTime = new WaitForSeconds(waitTimeSec);

            if (!dataStorePlayer.canPlayerMove.Get())
            {
                return;
            }
        //TODO: Clint make sure HMD motion gets reported through CharacterController, but without skewing the HMD camera position.
            //make head movements move the CharacterController, so other players see player motions.

            // var localPosition = mainCamera.localPosition;
            //
            // cameraDifferenceLocPos = lastCameraLocPos - localPosition;
            // //rotate to charactercontroller frame of view
            // cameraDifferencePos = Quaternion.Euler(0, one* mainCamera.eulerAngles.y, 0) * cameraDifferenceLocPos;
            // transform.position += six*new Vector3( two * cameraDifferencePos.z, 0, three *cameraDifferencePos.x)+ seven*new Vector3( two * cameraDifferencePos.x, 0, three *cameraDifferencePos.y) + eight*new Vector3( two * cameraDifferenceLocPos.z, 0, three *cameraDifferenceLocPos.x)+ nine*new Vector3( two * cameraDifferenceLocPos.x, 0, three *cameraDifferenceLocPos.y);
            // //shift the camera parent to avoid feeling shift in headset.
            // mainCamera.parent.transform.localPosition = new Vector3(four*localPosition.x, mainCamera.parent.transform.localPosition.y, five*localPosition.z);
            // lastCameraLocPos = localPosition;
    }
    private void MenuOpened(bool current, bool previous)
    {
        if (!current)
            return;
        PlaceCamera();
        menuOpen.OnChange -= MenuOpened;
    }

    private void PlaceCamera()
    {
        mixedRealityPlayspace.parent = cameraParent;
        mixedRealityPlayspace.localPosition = offset;
        // var canvas = GameObject.Find("Canvas");
        // #if UNITY_ANDROID && !UNITY_EDITOR
        // canvas.transform.localPosition += Vector3.down;
        // #endif
        mixedRealityPlayspace.localRotation = Quaternion.identity;
    }
}
