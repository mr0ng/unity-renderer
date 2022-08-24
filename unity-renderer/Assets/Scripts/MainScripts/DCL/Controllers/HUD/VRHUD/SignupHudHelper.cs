using System;
using DCL;
using DCL.Huds;
using SignupHUD;
using UnityEngine;

public class SignupHudHelper : VRHUDHelper
{
    [SerializeField]
    private SignupHUDView view;
    private BaseVariable<bool> dataStoreIsOpen = DataStore.i.exploreV2.isOpen;
    protected override void SetupHelper()
    {
        myTrans.localScale = 0.002f * Vector3.one;
        view.OnSetVisibility += OnVisiblityChange;
        if (myTrans is RectTransform rect)
        {
            rect.sizeDelta = new Vector2(1920, 1080);
        }

    }
   
    private void OnVisiblityChange(bool visible)
    {
        Debug.Log($"SignupHudHelper OnVisiblityChange {visible}");
        if (dataStoreIsOpen.Get())
            myTrans.localRotation = Quaternion.identity;
        else if (visible) 
            Position();
    }
    
    public void Position()
    {
        var forward = VRHUDController.I.GetForward();
        myTrans.position = Camera.main.transform.position + forward + 0f*Vector3.up + 3.75f*Vector3.forward;
        myTrans.forward = forward;
    }
}
