using DCL.VR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRCharacterController : MonoBehaviour
{
    [SerializeField]
    private Transform avatarTrans;
    [SerializeField]
    private Transform cameraParent;
    [SerializeField]
    private DCLCharacterController controller;
    [SerializeField]
    private float speed = 0.1f;
    [SerializeField]
    private float CameraFollowSpeed = 100f;
    [SerializeField]
    private float MaxDistCameraPlayer = 20;
    private Transform cachedTrans;

    protected void Awake()
    {
        cachedTrans = transform;
        CommonScriptableObjects.worldOffset.OnChange += OnWorldReposition;
    }

    private IEnumerator Start()
    {
        // TODO: find callback for player repostion on start        
        while (cachedTrans.position.y < 50f)
            yield return null;

        PlaceCamera();
    }

    protected void OnDestroy()
    {
        CommonScriptableObjects.worldOffset.OnChange -= OnWorldReposition;
    }

    private void OnWorldReposition(Vector3 current, Vector3 previous)
    {
        PlaceCamera();
    }

    private void PlaceCamera()
    {
        Debug.Log("world Reposition");
        var playSpace = VRPlaySpace.i;
        Transform mixedRealityPlayspace = playSpace.transform;

        cameraParent.position = avatarTrans.position + Vector3.up;

        playSpace.SetCameraInactive();

        //Move and Reparent the MRplayspace to the characterController so that we always have our controllers with us
        mixedRealityPlayspace.position = cameraParent.position;
        mixedRealityPlayspace.parent = cameraParent;
    }
}
