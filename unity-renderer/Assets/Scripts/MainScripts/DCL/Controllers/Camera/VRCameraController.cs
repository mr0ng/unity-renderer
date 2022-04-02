using UnityEngine;

namespace DCL.Camera
{
    public class VRCameraController : MonoBehaviour
    {
        [SerializeField]
        private CameraController controller;
        [SerializeField]
        private Transform targetToFollow;

        [SerializeField]
        private InputAction_Trigger cameraChangeAction;
        [SerializeField]
        private InputAction_Measurable cameraX;
        

        private Vector3 offset = new Vector3(0f, -0.5f, 0f);

        void Start()
        {
            CommonScriptableObjects.cameraBlocked.OnChange += CameraBlockedOnchange;
            controller.SetCameraMode(CameraMode.ModeId.FirstPerson);
            cameraChangeAction.isTriggerBlocked = CommonScriptableObjects.cameraBlocked;
            CommonScriptableObjects.cameraBlocked.Set(true);

            DCLCharacterController.i.OnUpdateFinish += FollowCharacter;
            cameraX.OnValueChanged += RotateCamera;

        }
        private void RotateCamera(DCLAction_Measurable action, float value)
        {
            transform.eulerAngles += Time.deltaTime * new Vector3(0f, value, 0f);
        }

        private void FollowCharacter(float deltaTime)
        {
            transform.position = targetToFollow.position + offset;
        }

        private void CameraBlockedOnchange(bool current, bool previous)
        {
            if (current == false)
                CommonScriptableObjects.cameraBlocked.Set(true);
        }
    }
}
