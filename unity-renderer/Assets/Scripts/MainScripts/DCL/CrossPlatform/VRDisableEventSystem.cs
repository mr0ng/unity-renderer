using UnityEngine;

public class VRDisableEventSystem : MonoBehaviour
{
    private void Awake()
    {
        if (CrossPlatformManager.IsVRPlatform()) gameObject.SetActive(false);
    }
}
