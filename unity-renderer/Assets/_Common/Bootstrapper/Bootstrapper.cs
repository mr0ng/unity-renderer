using UnityEditor;
using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject webGLPrefab;
    [SerializeField] private GameObject desktopPrefab;
    [SerializeField] private GameObject VRPrefab;


//#if UNITY_EDITOR
    public enum Platform
    {
        WebGL,
        Desktop,
        VR
    }

    [SerializeField]
    private Platform _currentPlatform;
    public Platform currentPlatform
    {
        get => _currentPlatform;
        set
        {
            _currentPlatform = value;
            SetDCLVRScriptingSymbol(_currentPlatform == Platform.VR);

            #if DCL_VR
                    Debug.Log("VR Define Set");
            #endif
        }
    }
    private void SetDCLVRScriptingSymbol(bool enableDCL_VR)
    {
#if UNITY_EDITOR
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        if (enableDCL_VR && !currentSymbols.Contains("DCL_VR"))
        {
            currentSymbols += ";DCL_VR";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, currentSymbols);
        }
        else if (!enableDCL_VR && currentSymbols.Contains("DCL_VR"))
        {
            currentSymbols = currentSymbols.Replace(";DCL_VR", "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, currentSymbols);
        }
        #endif
    }
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

//#else
//     private void Awake()
//     {
// #if UNITY_WEBGL
//         Instantiate(webGLPrefab);
// #elif ENABLE_VR
//         Instantiate(VRPrefab);
// #else
//         Instantiate(desktopPrefab);
// #endif
//     }
//#endif
}
