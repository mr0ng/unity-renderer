using System;
using DCL.CameraTool;
using UnityEngine;

namespace DCL.Camera
{
    public class VRCameraController : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.Camera cam;
        [SerializeField]
        private CameraController controller;
        [SerializeField]
        private Transform targetToFollow;
        [SerializeField]
        private float secondsBetweenTurns = .5f;
        [SerializeField]
        private float rotation = 45f;

        [SerializeField]
        private InputAction_Trigger cameraChangeAction;
        [SerializeField]
        private InputAction_Measurable cameraX;
        [SerializeField]
        private InputAction_Measurable characterX;
        [SerializeField]
        private InputAction_Measurable characterY;

        private DateTime lastTurned;

        private readonly Vector3 offset = new Vector3(0f, -1f, 0f);
        private Transform myTrans;
        private Transform camTrans;
        private Vector3 cameraDifferenceRot;
        private Vector3 lastCameraRot;
        private bool smoothRotate = true;

        [SerializeField]
        private float rotationThreshold = 0.001f;
        void Start()
        {
            #if !DCL_VR
                return;
            #endif
            myTrans = transform;
            camTrans = UnityEngine.Camera.main.transform;
            lastCameraRot = camTrans.localEulerAngles;
            CommonScriptableObjects.cameraBlocked.OnChange += CameraBlockedOnchange;
            controller.SetCameraMode(CameraMode.ModeId.FirstPerson);
            cameraChangeAction.isTriggerBlocked = CommonScriptableObjects.cameraBlocked;
            CommonScriptableObjects.cameraBlocked.Set(true);

            DCLCharacterController.i.OnUpdateFinish += FollowCharacter;
            cameraX.OnValueChanged += RotateCamera;
        }

        private void Update()
        {
#if !DCL_VR
                return;
#endif
            DisableCamera();
            if (!camTrans.hasChanged)
                return;
            var localEulerAngles = camTrans.localEulerAngles;
            cameraDifferenceRot = localEulerAngles-lastCameraRot  ;
            //remove off angle from camera parent
            camTrans.parent.localEulerAngles = new Vector3(0,  -localEulerAngles.y, 0);
            //add to CharacterController
            myTrans.eulerAngles += new Vector3(0f, cameraDifferenceRot.y, 0f);
            lastCameraRot = localEulerAngles;

            CommonScriptableObjects.cameraForward.Set(camTrans.forward);
            CommonScriptableObjects.cameraRight.Set(camTrans.right);
            CommonScriptableObjects.cameraPosition.Set(camTrans.position);
        }

        private void DisableCamera()
        {
            if (!cam.enabled)
                return;
            cam.enabled = false;
        }

        private void RotateCamera(DCLAction_Measurable action, float value)
        {
            if (smoothRotate)
            {
                SmoothRotate(value);
                return;
            }
            SnapRotate(value);
        }

        private void SnapRotate(float value)
        {
            if ((DateTime.Now - lastTurned).TotalSeconds < secondsBetweenTurns) return;
            var yValue = value >= 0 ? rotation : -rotation;
            myTrans.eulerAngles += new Vector3(0f, yValue, 0f);
            lastTurned = DateTime.Now;
        }

        public void SmoothRotate(float value)
        {
            myTrans.eulerAngles += new Vector3(0f, value * 1.3f * rotation * Time.deltaTime, 0f);
        }

        private void FollowCharacter(float deltaTime)
        {
            myTrans.position = targetToFollow.position;// + offset;
        }

        private void CameraBlockedOnchange(bool current, bool previous)
        {
            if (current == false)
                CommonScriptableObjects.cameraBlocked.Set(true);
        }

        public void SetSmoothRotate(bool value) => smoothRotate = value;
    }
}
