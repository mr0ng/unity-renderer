using DCL.Huds;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Input.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
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
        Message
    }
    
    private enum InterationBehavior
    {
        Loading = -1,
        None = 0,
        Far = 1,
        Near = 2,
        Both = 3
    }

    [SerializeField]
    private HudType hudType;
    [SerializeField]
    private InterationBehavior interactionBehavior;
    [SerializeField]
    private int sortingOrder;
    [SerializeField]
    private LayerMask loadingMask;

    private Transform myTrans;

    private void Awake()
    {
        if (!CrossPlatformManager.IsVRPlatform())
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
                VRHUDController.I.Register(this);
                VRHUDController.I.Reparent(myTrans);
                break;
            case HudType.Message:
                VRHUDController.LoadingStart += Hide;
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
        switch (interactionBehavior)
        {
            case InterationBehavior.Loading:
                myTrans.localScale = 0.00075f * Vector3.one;
                break;
            case InterationBehavior.Far:
                Button[] buttons = GetComponentsInChildren<Button>(true);
                foreach (Button button in buttons)
                {
                    Interactable interactable = button.gameObject.GetComponent<Interactable>();
                    if (interactable == null) continue;
                    interactable.OnClick.AddListener(() =>
                    {
                        Debug.LogWarning("Button clicked");
                    });
                }
                break;
            case InterationBehavior.Near:
            case InterationBehavior.Both:
            case InterationBehavior.None:
            default: break;
        }
    }

    private void RunBehavior()
    {
        switch (interactionBehavior)
        {
            case InterationBehavior.Loading:
                CrossPlatformManager.SetCameraForLoading(loadingMask);
                var forward = VRHUDController.I.GetForward();
                myTrans.position = Camera.main.transform.position + forward;
                myTrans.forward = forward;
                VRHUDController.RaiseLoadingStart();
                break;
            case InterationBehavior.Far:
                break;
            case InterationBehavior.Near:
            case InterationBehavior.Both:
            case InterationBehavior.None:
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
        switch (interactionBehavior)
        {
            case InterationBehavior.Loading:
                CrossPlatformManager.SetCameraForGame();
                VRHUDController.RaiseLoadingEnd();
                break;
            case InterationBehavior.Far:
                break;
            case InterationBehavior.Near:
            case InterationBehavior.Both:
            case InterationBehavior.None:
            default: break;
        }
    }

    public void ResetHud()
    {
        myTrans.localPosition = Vector3.zero;
        myTrans.localRotation = Quaternion.identity;

    }
}
