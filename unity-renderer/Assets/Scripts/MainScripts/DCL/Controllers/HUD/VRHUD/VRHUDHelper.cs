using DCL.Huds;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Input.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class VRHUDHelper : MonoBehaviour
{
    private const string SortingLayer = "Menu";
    private const  int SortingID = 1;
    private enum HudType
    {
        Loading,
        Menu,
        Message,
        PopUp
    }
    
    [SerializeField]
    private HudType hudType;
    [SerializeField]
    private int sortingOrder;
    [SerializeField]
    private LayerMask loadingMask;
    [SerializeField]
    private bool submenu;
    [SerializeField]
    private GameObject objectToHide;
    
    private Transform myTrans;

    private void Awake()
    {
        if (!CrossPlatformManager.IsVR)
        {
            enabled = false;
            return;
        }
        myTrans = transform;
    }

    private void Start()
    {
        ConvertUI();
        switch (hudType)
        {
            case HudType.Menu:
                VRHUDController.I.Register(this, submenu);
                VRHUDController.I.Reparent(myTrans);
                break;
            case HudType.Message:
                VRHUDController.LoadingStart += Hide;
                break;
            case HudType.PopUp:
                objectToHide.SetActive(false);
                break;
        }
    }
    
    private void Hide()
    {
        gameObject.SetActive(false);
        VRHUDController.LoadingStart -= Hide;
    }

    private void ConvertUI()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();
        if (GetComponent<CanvasUtility>() == null)
            gameObject.AddComponent<CanvasUtility>();
        if (GetComponent<NearInteractionTouchableUnityUI>() == null)
            gameObject.AddComponent<NearInteractionTouchableUnityUI>();
        
        SetUpInteractionBehavtior();
    }
    
    private void SetUpInteractionBehavtior()
    {
        switch (hudType)
        {
            case HudType.Loading:
                myTrans.localScale = 0.00075f * Vector3.one;
                break;
            case HudType.PopUp:
                myTrans.localScale = 0.0025f * Vector3.one;
                break;
            default: break;
        }
    }

    private void RunBehavior()
    {
        switch (hudType)
        {
            case HudType.Loading:
                CrossPlatformManager.SetCameraForLoading(loadingMask);
                var forward = VRHUDController.I.GetForward();
                myTrans.position = Camera.main.transform.position + forward;
                myTrans.forward = forward;
                VRHUDController.RaiseLoadingStart();
                break;
            case HudType.PopUp:
                CrossPlatformManager.GetSurfacePoint(out var point, out var normal);
                myTrans.position = point + normal * .25f;
                myTrans.forward = -normal;
                break;
            default: break;
        }
    }

    private void OnEnable()
    {
        RunBehavior();
    }

    private void OnDisable()
    {
        if (CrossPlatformManager.IsVRPlatform()) 
            StopBehavior();
    }

    private void StopBehavior()
    {
        switch (hudType)
        {
            case HudType.Loading:
                CrossPlatformManager.SetCameraForGame();
                VRHUDController.RaiseLoadingEnd();
                break;
            default: break;
        }
    }

    public void Hide(Vector3 pos) => myTrans.position += pos;

    public void ResetHud()
    {
        myTrans.localPosition = Vector3.zero;
        myTrans.localRotation = Quaternion.identity;
    }
}
