using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Camera
{
    public class VRCameraController : MonoBehaviour
    {
        [SerializeField]
        private CameraController controller;
        
        void Start()
        {
            CommonScriptableObjects.cameraBlocked.OnChange += CameraBlockedOnchange;
            controller.SetCameraMode(CameraMode.ModeId.FirstPerson);
            CommonScriptableObjects.cameraBlocked.Set(true);
        }

        private void CameraBlockedOnchange(bool current, bool previous)
        {
            if (current == false)
                CommonScriptableObjects.cameraBlocked.Set(true);
        }
    }
}
