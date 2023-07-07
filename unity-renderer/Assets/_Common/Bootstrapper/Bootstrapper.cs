using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject webGLPrefab;
    [SerializeField] private GameObject desktopPrefab;
    [SerializeField] private GameObject VRPrefab;


#if UNITY_EDITOR
    private enum Platform
    {
        WebGL,
        Desktop,
        VR
    }

    [SerializeField] private Platform currentPlatform;

    private void Awake()
    {
        switch (currentPlatform)
        {
            case Platform.WebGL:
                Instantiate(webGLPrefab);
                break;
            case Platform.Desktop:
                Instantiate(desktopPrefab);
                break;
            case Platform.VR:
                Instantiate(VRPrefab);
                break;
        }
    }

#else
    private void Awake()
    {
#if UNITY_WEBGL
        Instantiate(webGLPrefab);
#elif ENABLE_VR
        Instantiate(VRPrefab);
#else
        Instantiate(desktopPrefab);
#endif
    }
#endif
}
