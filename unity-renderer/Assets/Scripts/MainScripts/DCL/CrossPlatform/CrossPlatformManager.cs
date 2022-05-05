using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Physics;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public static class CrossPlatformManager
{
    private static bool isVR;
    public static bool IsVR
    {
        get => isVR;
        set => isVR = value;
    }
    
    private static LayerMask layerMask;
    public static string GetControllerName()
    {
        PlatformSettings settings = Resources.Load<PlatformSettings>($"PlatformSettings");
        string contorllerName;
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Camera, devices);
        IsVR = XRGeneralSettings.Instance.Manager.activeLoader != null;
        if (!IsVR)
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
    
    public static Ray GetRay()
    {
        var pos = CoreServices.FocusProvider?.PrimaryPointer.Result.StartPoint;
        var index = CoreServices.FocusProvider?.PrimaryPointer.Result.RayStepIndex;
        if (!index.HasValue)
            return default;
        var rayStep = CoreServices.FocusProvider?.PrimaryPointer.Rays[index.Value];
        var rayStepValue = rayStep.Value;
        return new Ray(rayStepValue.Origin, rayStepValue.Direction);
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
    
    public static Vector3 GetPoint()
    {
        var point = CoreServices.FocusProvider?.PrimaryPointer?.Result?.Details.Point;

        return point ?? Vector3.zero;
    }
    // public static bool PointerPressed(WebInterface.ACTION_BUTTON getActionButton)
    // {
    //     //if (IsVR) 
    // }
}
