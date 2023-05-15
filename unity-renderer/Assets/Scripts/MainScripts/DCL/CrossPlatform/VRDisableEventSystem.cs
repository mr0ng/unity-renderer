using UnityEngine;

public class VRDisableEventSystem : MonoBehaviour
{
    private void Awake()
    {
        #if DCL_VR
        gameObject.SetActive(false);
        #endif
    }
}
