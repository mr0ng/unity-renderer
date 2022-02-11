using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using DCL;
using System;
using System.Collections;
using UnityEngine;

public class VRInputController : InputSystemGlobalHandlerListener, IMixedRealityInputHandler<Vector2>, IMixedRealityInputActionHandler
{
    [SerializeField]
    private MixedRealityInputAction moveAction;
    [SerializeField]
    private MixedRealityInputAction rotateAction;
    [SerializeField]
    private MixedRealityInputAction jumpAction;
    [SerializeField]
    private float speed = 0.1f;
    [SerializeField]
    private float CameraFollowSpeed = 100f;
    [SerializeField]
    private float MaxDistCameraPlayer = 20;
    [SerializeField]
    private InputAction_Measurable characterXAxis;
    [SerializeField]
    private InputAction_Measurable characterYAxis;
    [SerializeField]
    private InputAction_Hold jump;

    private void OnValidate()
    {
        CheckForValidAction(ref moveAction, AxisType.DualAxis);
        CheckForValidAction(ref rotateAction, AxisType.DualAxis);
        CheckForValidAction(ref jumpAction, AxisType.Digital);
    }

    private void CheckForValidAction(ref MixedRealityInputAction action, AxisType axisType)
    {
        if (action == MixedRealityInputAction.None)
        {
            Debug.LogWarning("Action has not been set", this);
        }
        else if (action.AxisConstraint != axisType)
        {
            Debug.LogError($"Move Action must be of DualAxis type, {moveAction.Description} is of {moveAction.AxisConstraint} type");
            action = MixedRealityInputAction.None;
        }
    }

    protected override void Start()
    {
        RegisterHandlers();
    }

    private void OnDestroy()
    {
        UnregisterHandlers();
    }

    public void OnInputChanged(InputEventData<Vector2> eventData)
    {
        if (eventData.MixedRealityInputAction.Equals(moveAction))
        {
            MovePlayer(eventData.InputData);
        }        
        else if (eventData.MixedRealityInputAction.Equals(rotateAction))
        {
            RotatePlayer(eventData.InputData);
        }
    }

    private void RotatePlayer(Vector2 inputData)
    {        
        Debug.Log($"right hand input {inputData}");
        characterXAxis.RaiseOnValueChanged(inputData.x);
        characterYAxis.RaiseOnValueChanged(inputData.y);
    }

    private void MovePlayer(Vector2 inputData)
    {
        Debug.Log($"left hand input {inputData}");
        characterXAxis.RaiseOnValueChanged(inputData.x);
        characterYAxis.RaiseOnValueChanged(inputData.y);
    }

    public void OnActionStarted(BaseInputEventData eventData)
    {
        if (!eventData.MixedRealityInputAction.Description.Equals("Thumb Press")) return;
        Debug.Log("Thumbstick was pressed.");
    }

    public void OnActionEnded(BaseInputEventData eventData)
    {
        if (!eventData.MixedRealityInputAction.Description.Equals("Thumb Press")) return;
        Debug.Log("Thumbstick was released.");
    }

    protected override void RegisterHandlers()
    {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler<Vector2>>(this);
    }

    protected override void UnregisterHandlers()
    {
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputHandler<Vector2>>(this);
    }
}
