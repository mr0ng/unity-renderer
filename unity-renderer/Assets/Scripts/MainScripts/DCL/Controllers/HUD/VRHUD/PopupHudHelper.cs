using UnityEngine;

public class PopupHudHelper : VRHUDHelper
{
    [SerializeField]
    protected GameObject objectToHide;
    
    protected override void SetupHelper()
    {
        myTrans.localScale = 0.0025f * Vector3.one;
        objectToHide.SetActive(false);
    }

    protected override void RunOnEnable()
    { 
        CrossPlatformManager.GetSurfacePoint(out var point, out var normal);
        myTrans.position = point + normal * .25f;
        myTrans.forward = -normal;
    }
    protected override void RunOnDisable()
    {
        
    }
}
