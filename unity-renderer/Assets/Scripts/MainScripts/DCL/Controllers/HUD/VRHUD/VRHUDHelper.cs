using System;
using DCL;
using DCL.Huds;
using Microsoft.MixedReality.Toolkit.Input.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine.UI;

public class VRHUDHelper : MonoBehaviour
{
    private enum FollowBehavior
    {
        Stationary,
        Toolbelt,
        PalmUp,
        Radial,
        Orbital
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
    private HUDElementID hudType;
    [SerializeField]
    private FollowBehavior followBehavior;
    [SerializeField]
    private InterationBehavior interactionBehavior;
    [SerializeField]
    private Vector3 scale = new Vector3(0.001f, 0.001f, 0.001f);

    [SerializeField]
    private GameObject visuals;
    
    private Transform cameraTrans;
    private GameObject loadingCamera;

    private void Awake()
    {
        if (!CrossPlatformManager.IsVRPlatform())
        {
            enabled = false;
            RemoveDependencies();
            return;
        }
        ConvertUI();
        VRHUDController.I.Register(hudType, this);
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
        transform.localScale = scale;
        if (GetComponent<GraphicRaycaster>() != null)
            gameObject.AddComponent<GraphicRaycaster>();
        if (GetComponent<CanvasUtility>() != null)
            gameObject.AddComponent<CanvasUtility>();

        SetUpFollowBehavior();
        SetUpInteractionBehavtior();
    }
    
    private void SetUpInteractionBehavtior()
    {
        switch (interactionBehavior)
        {
            case InterationBehavior.Loading:
                if (cameraTrans == null)
                {
                    loadingCamera = Instantiate(Resources.Load<GameObject>("LoadingCamera"));
                    cameraTrans = loadingCamera.transform;
                }
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
                        button.onClick?.Invoke();
                    });
                }
                break;
            case InterationBehavior.Near:
            case InterationBehavior.Both:
            case InterationBehavior.None:
            default: break;
        }
    }
    
    private void SetUpFollowBehavior()
    {
        switch (followBehavior)
        {
            case FollowBehavior.Orbital :
                Orbital orbital = gameObject.AddComponent<Orbital>();
                break;
            case FollowBehavior.PalmUp :
                SolverHandler solver = gameObject.AddComponent<SolverHandler>();
                solver.TrackedTargetType = TrackedObjectType.HandJoint;
                solver.TrackedHandJoint = TrackedHandJoint.Wrist;
                HandConstraintPalmUp palmUp = gameObject.AddComponent<HandConstraintPalmUp>();
                palmUp.OnHandActivate.AddListener(ActivateVisuals);
                palmUp.OnHandDeactivate.AddListener(DeactivateVisuals);
                break;
            case FollowBehavior.Toolbelt:
            case FollowBehavior.Radial:
            case FollowBehavior.Stationary:
            default:
                gameObject.SetActive(false);
                break;
        }
    }

    private void RunBehavior()
    {
        switch (followBehavior)
        {
            case FollowBehavior.Orbital : 
                break;
            case FollowBehavior.Toolbelt:
                break;
            case FollowBehavior.PalmUp:
                break;
            case FollowBehavior.Radial:
                break;
            case FollowBehavior.Stationary:
            default: break;
        }
        switch (interactionBehavior)
        {
            case InterationBehavior.Loading:
                if (Camera.main != null) Camera.main.enabled = false;
                loadingCamera.SetActive(true);
                transform.position = new Vector3(0f, 1.5f, 1f);
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
                if (Camera.main != null)
                    Camera.main.enabled = true;
                loadingCamera.SetActive(false);
                break;
            case InterationBehavior.Far:
                break;
            case InterationBehavior.Near:
            case InterationBehavior.Both:
            case InterationBehavior.None:
            default: break;
        }
    }

    private void ActivateVisuals() => visuals.SetActive(true);
    private void DeactivateVisuals() => visuals.SetActive(false);

    public void ActivateHud()
    {
        gameObject.SetActive(true);
    }
}
