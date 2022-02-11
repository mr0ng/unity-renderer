using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;

public class CrossPlatformManager
{
    public static string GetControllerName()
    {
        PlatformSettings settings = Resources.Load<PlatformSettings>($"PlatformSettings");
        var contorllerName = "";
        if (XRGeneralSettings.Instance.Manager.activeLoader != null)
        {
            contorllerName = settings.NonVRController;
        }
        else
        {
            contorllerName = settings.VRController;
        }

        return contorllerName;
    }
}
