using DCL.Huds;
using UnityEngine;

public class MenuHudHelper : VRHUDHelper
{
    [SerializeField]
    private bool submenu;

    protected override void SetupHelper()
    {
        #if !DCL_VR
        return;
#endif
        VRHUDController.I.Register(this, submenu);
        VRHUDController.I.Reparent(myTrans);
    }
}
