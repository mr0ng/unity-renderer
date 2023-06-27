using DCL;
using DCL.Social.Passports;
using System;
using UnityEngine;

public class PlayerPassportHudHelper : VRHUDHelper
{
    [SerializeField]
    private PlayerPassportHUDView view;
    private BaseVariable<bool> dataStoreIsOpen = DataStore.i.exploreV2.isOpen;
    protected override void SetupHelper()
    {
        //Position();
        view.OnSetVisibility += OnVisiblityChange;
    }
    private void OnVisiblityChange(bool visible)
    {
        Position();
    }

    public void Position()
    {
        myTrans.localScale = 0.0024f*Vector3.one;
        var rawForward = CommonScriptableObjects.cameraForward.Get();
        var forward = new Vector3(rawForward.x, 0, rawForward.z).normalized;
        myTrans.position = Camera.main.transform.position + 3 * forward;// + 1.0f * Vector3.up;
        myTrans.forward =  forward;

    }

    private void OnDestroy()
    {
        view.OnSetVisibility -= OnVisiblityChange;
    }
}
