using System;
using UnityEngine;

namespace DCL.Camera
{
    public class VRCameraController : MonoBehaviour
    {
        [SerializeField]
        private CameraController controller;
        [SerializeField]
        private InputAction_Trigger cameraChangeAction;
        [SerializeField]
        private Transform targetToFollow;

        private Vector3 offset = new Vector3(0f, -0.5f, 0f);

        void Start()
        {
            CommonScriptableObjects.cameraBlocked.OnChange += CameraBlockedOnchange;
            controller.SetCameraMode(CameraMode.ModeId.FirstPerson);
            cameraChangeAction.isTriggerBlocked = CommonScriptableObjects.cameraBlocked;
            CommonScriptableObjects.cameraBlocked.Set(true);
        }

        private void CameraBlockedOnchange(bool current, bool previous)
        {
            if (current == false)
                CommonScriptableObjects.cameraBlocked.Set(true);
        }

        private void Update()
        {
            transform.position = targetToFollow.position + offset;
        }
    }
}
