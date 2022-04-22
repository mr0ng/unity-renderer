using System;
using DCL.Huds;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Input.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.UI;

public class VRHUDHelper : MonoBehaviour
{
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
    private Vector3 scale = new Vector3(0.001f, 0.001f, 0.001f);
    [SerializeField]
    private LayerMask loadingMask;

    private Transform myTrans;
    private readonly Vector3 hidenPos = new Vector3(0, 300, 0);

    private void Awake()
    {
        if (!CrossPlatformManager.IsVRPlatform())
        {
            enabled = false;
            return;
        }
        myTrans = transform;
        myTrans.position = hidenPos;
        ConvertUI();
    }

    private void Start()
    {
        switch (hudType)
        {
            case HudType.Menu:
                VRHUDController.I.Register(this);
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

    private void RemoveDependencies()
    {
        BoxCollider[] boxes = gameObject.GetComponentsInChildren<BoxCollider>();
        int count = boxes.Length;
        for (int i = 0; i < count; i++)
        {
            Destroy(boxes[i]);
        }
    }

    private void ConvertUI()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        myTrans.localScale = scale;
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
                ShowHud(Camera.main.transform);
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
    
    public void ShowHud(Transform mainCam)
    {
        var foward = mainCam.forward;
        var forward = new Vector3(foward.x, 0f , foward.z).normalized;
        myTrans.position = mainCam.position + forward;
        myTrans.forward = forward;
    }
    
    public void HidHud()
    {
        transform.position = hidenPos;
    }
}
