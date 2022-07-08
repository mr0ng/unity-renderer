using DCL.Huds;
using UnityEngine;

class LoadingHudHelper : VRHUDHelper
{
    [SerializeField]
    private LayerMask loadingMask;
    [SerializeField]
    private ShowHideAnimator animator;

    protected override void SetupHelper()
    {
        myTrans.localScale = 0.00075f * Vector3.one;
        VRHUDController.I.SetupLoading(animator);
        VRHUDController.LoadingStart += () =>
        {
            CrossPlatformManager.SetCameraForLoading(loadingMask);
            var forward = VRHUDController.I.GetForward();
            myTrans.position = Camera.main.transform.position + forward;
            myTrans.forward = forward;
        };
        VRHUDController.LoadingEnd += CrossPlatformManager.SetCameraForGame;
    }

    protected override void RunOnEnable() {  }
    protected override void RunOnDisable() { }
}
