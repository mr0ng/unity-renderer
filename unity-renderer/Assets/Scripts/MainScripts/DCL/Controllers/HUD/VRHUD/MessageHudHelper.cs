using DCL.Huds;
using UnityEngine;

public class MessageHudHelper : VRHUDHelper
{
    protected override void SetupHelper()
    {
        VRHUDController.LoadingStart += HideMessage;
    }
    
    protected override void RunOnEnable() { }
    protected override void RunOnDisable() { }

    private void HideMessage()
    {
        gameObject.SetActive(false);
        VRHUDController.LoadingStart -= HideMessage;
    }
}
