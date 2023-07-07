using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapHUDViewDesktop : MonoBehaviour
{
    internal static MinimapHUDView Create(MinimapHUDController controller)
    {
        #if DCL_VR
        var view = Object.Instantiate(Resources.Load<GameObject>("MinimapHUDVR")).GetComponent<MinimapHUDView>();
        view.Initialize(controller);
        #else
        var view = Object.Instantiate(Resources.Load<GameObject>("MinimapHUDDesktop")).GetComponent<MinimapHUDView>();
        view.Initialize(controller);
        #endif
        return view;
    }
}
