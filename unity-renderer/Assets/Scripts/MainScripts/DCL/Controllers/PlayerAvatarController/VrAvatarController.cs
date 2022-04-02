using UnityEngine;

namespace DCL
{
    public class VrAvatarController : MonoBehaviour
    {
        private void Start()
        {
            AvatarVisibility visibility = GetComponent<AvatarVisibility>();
            visibility.SetVisibility("VR_AVATAR_CONTROLLER", false);
        }
    }
}