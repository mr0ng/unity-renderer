using DCL;
using UnityEngine;

public class AvatarEditorHudHelper : VRHUDHelper
{
    [SerializeField]
    private AvatarEditorHUDView view;
    private BaseVariable<bool> dataStoreIsOpen = DataStore.i.exploreV2.isOpen;
    protected override void SetupHelper()
    {
        view.OnSetVisibility += OnVisiblityChange;
    }
    private void OnVisiblityChange(bool visible)
    {
        if (dataStoreIsOpen.Get())
            myTrans.localRotation = Quaternion.identity;
        else if (visible) 
            Position();
    }
    
    private void Position()
    {
        
        var rawForward = CommonScriptableObjects.cameraForward.Get();
        var forward = new Vector3(rawForward.x, 0, rawForward.z).normalized;
        var pos = CommonScriptableObjects.cameraPosition.Get() + 2.5f*forward;
        myTrans.position = new Vector3(pos.x, 1.2f, pos.z);
        myTrans.forward = forward;
        
    }
}
