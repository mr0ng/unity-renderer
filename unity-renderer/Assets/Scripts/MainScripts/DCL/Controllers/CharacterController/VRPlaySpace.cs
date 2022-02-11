using UnityEngine;

namespace DCL.VR
{
    public class VRPlaySpace : MonoBehaviour
    {
        public static VRPlaySpace i;

        [SerializeField]
        private GameObject cameraObject;

        private void Awake()
        {
            i = this;
        }

        public void SetCameraInactive()
        {
            cameraObject.SetActive(false);
        }
    }
}
