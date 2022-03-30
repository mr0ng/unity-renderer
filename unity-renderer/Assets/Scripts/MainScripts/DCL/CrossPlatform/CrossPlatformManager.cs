using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class CrossPlatformManager
{
    public static string GetControllerName()
    {
        PlatformSettings settings = Resources.Load<PlatformSettings>($"PlatformSettings");
        var contorllerName = "";
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Camera, devices);
        Debug.LogWarning(devices.Any());
        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            contorllerName = settings.NonVRController;
        }
        else
        {
            contorllerName = settings.VRController;
        }

        return contorllerName;
    }

    public static bool IsVRPlatform()
    {
        return XRGeneralSettings.Instance.Manager.activeLoader != null;
    }
}
