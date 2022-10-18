using DCL;
using DCL.Huds;
using GotoPanel;
using SignupHUD;
using UnityEngine;

public class TeleportPromptHudHelper : VRHUDHelper
{
    [SerializeField]
    private TeleportPromptHUDView view;
    private BaseVariable<bool> dataStoreIsOpen = DataStore.i.exploreV2.isOpen;
    protected override void SetupHelper()
    {
        transform.localScale = 0.0033f * Vector3.one;
        view.OnSetVisibility += OnVisiblityChange;
    }
    private void OnVisiblityChange(bool visible)
    {
        if (!visible) { 
            myTrans.position += 10 * Vector3.down;
            myTrans.localRotation = Quaternion.identity;
        }
        else 
            Position();
    }
    
    private void Position()
    {
        
        var forward = VRHUDController.I.GetForward();
        if (Camera.main != null)
#if UNITY_ANDROID && !UNITY_EDITOR
            myTrans.position = Camera.main.transform.position+ 1.9f*forward;
#else
            myTrans.position = Camera.main.transform.position + 1.9f*forward;// + 2.3f*Vector3.up + 3.75f*Vector3.forward;
        
        #endif
        myTrans.forward = forward;
    }
    
}
