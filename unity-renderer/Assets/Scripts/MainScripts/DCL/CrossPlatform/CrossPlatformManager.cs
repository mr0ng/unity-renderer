using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public static class CrossPlatformManager
{
    private static LayerMask layerMask;
    public static string GetControllerName()
    {
        PlatformSettings settings = Resources.Load<PlatformSettings>($"PlatformSettings");
        string contorllerName;
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Camera, devices);
        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            contorllerName = settings.NonVRController;
        }
        else
        {
            SetUpForVR();
            contorllerName = settings.VRController;
        }

        return contorllerName;
    }

    public static bool IsVRPlatform()
    {
        return XRGeneralSettings.Instance.Manager.activeLoader != null;
    }

    private static void SetUpForVR()
    {
        DCL.Helpers.Utils.LockCursor();
        DCL.Helpers.Utils.OnCursorLockChanged += LockCursor;
    }
    
    private static void LockCursor(bool state)
    {
        if (!state) DCL.Helpers.Utils.LockCursor();
    }

    public static void SetCameraForLoading(LayerMask mask)
    {
        var mainCam = Camera.main;
        layerMask = mainCam.cullingMask;
        mainCam.cullingMask = mask;
        mainCam.clearFlags = CameraClearFlags.Color;
        mainCam.backgroundColor = Color.black;
    }

    public static void SetCameraForGame()
    {
        
        var mainCam = Camera.main;
        mainCam.cullingMask =layerMask;
        mainCam.clearFlags = CameraClearFlags.Skybox;
    }
}
