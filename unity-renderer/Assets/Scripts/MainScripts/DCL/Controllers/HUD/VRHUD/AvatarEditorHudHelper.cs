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
        // int defaultLayerIndex = 0;
        // int uiLayerIndex = LayerMask.NameToLayer("UI");
        // LayerMask combinedLayerMask = (1 << defaultLayerIndex) | (1 << uiLayerIndex);
        // CrossPlatformManager.SetCameraForLoading(combinedLayerMask);
        // if (dataStoreIsOpen.Get())
        //     myTrans.localRotation = Quaternion.identity;
        // else if (visible)
            Position();
    }

    private void Position()
    {
        myTrans.localScale = 0.0024f*Vector3.one;
        var rawForward = CommonScriptableObjects.cameraForward.Get();
        var forward = new Vector3(rawForward.x, 0, rawForward.z).normalized;
        myTrans.position = Camera.main.transform.position +2*forward + 1.0f * Vector3.up;
        myTrans.forward =  forward;

    }
}
