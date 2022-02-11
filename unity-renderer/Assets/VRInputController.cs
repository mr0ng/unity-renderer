using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using UnityEngine;
using DCL;

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
    private InputAction_Measurable characterXAxis;
    [SerializeField]
    private InputAction_Measurable characterYAxis;
    [SerializeField]
    private float MaxDistCameraPlayer = 20;
    private DCLCharacterController characterController;
    private Vector3 position;


    private void Awake()
    {
        CommonScriptableObjects.worldOffset.OnChange += OnWorldReposition;
    }

    private void OnValidate()
    {
        if (moveAction.AxisConstraint != AxisType.DualAxis)
        {
            Debug.LogError($"Move Action must be of DualAxis type, {moveAction.Description} is of {moveAction.AxisConstraint} type");
            moveAction = MixedRealityInputAction.None;
        }
        if (rotateAction.AxisConstraint != AxisType.DualAxis)
        {
            Debug.LogError($"Rotate Action must be of DualAxis type, {rotateAction.Description} is of {rotateAction.AxisConstraint} type");
            rotateAction = MixedRealityInputAction.None;
        }
        if (jumpAction.AxisConstraint != AxisType.Digital)
        {
            Debug.LogError($"Jump Action must be of Digital type, {jumpAction.Description} is of {jumpAction.AxisConstraint} type");
            jumpAction = MixedRealityInputAction.None;
        }
    }

    private void OnWorldReposition(Vector3 current, Vector3 previous)
    {
        GameObject mixedRealityPlayspace = GameObject.Find("MixedRealityPlayspace");


        //We don't want to move the camera itself, but rather the parent of the camera to where the eyes are
        characterController = DCLCharacterController.i;
        transform.parent.position = GameObject.Find("AvatarRenderer").transform.position;
        transform.parent.position = new Vector3(transform.parent.position.x, transform.parent.position.y + 1f, transform.parent.position.z);
        Debug.Log("Moved camera parent to AvatarRenderer Location");

        //Turn off MRTK Camera. Bad programming - feel free to improve.
        mixedRealityPlayspace.transform.Find("Main Camera").gameObject.SetActive(false);

        //Move and Reparent the MRplayspace to the characterController so that we always have our controllers with us
        mixedRealityPlayspace.transform.position = transform.parent.position;
        mixedRealityPlayspace.transform.parent = transform.parent;

        //StartCoroutine(VRLocomotion());
    }

    protected override void Start()
    {
        //CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
        //CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler<Vector2>>(this);
    }

    protected override void OnDisable()
    {
        //CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
        //CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputHandler<Vector2>>(this);
    }

    void Update()
    {

    }

    private void OnDestroy()
    {
        CommonScriptableObjects.worldOffset.OnChange -= OnWorldReposition;
    }

    private IEnumerator VRLocomotion()
    {
        yield return null;



        while(true)
        {
            yield return null;
            position = Vector3.zero;
            float axisX = -Input.GetAxis("AXIS_4");
            float axisY = -Input.GetAxis("AXIS_5");
            if (Math.Abs(axisX) > 0.1f || Math.Abs(axisY) > 0.1f)
            {
                position = position + new Vector3(transform.forward.x, 0f, transform.forward.z) * axisY * speed;
                position = position + Vector3.Cross(transform.forward, Vector3.up) * axisX * speed;

            }

            characterController.transform.rotation = Quaternion.Euler(characterController.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, characterController.transform.rotation.eulerAngles.z);

            //If the camera is near the avatar, smoothly follow it. Otherwise, teleport to it
            Debug.Log("Distance between camera and player: " + Vector3.Distance(transform.parent.position, position));
            if (Vector3.Distance(transform.parent.position, position) < 20)
            {
                transform.parent.position = Vector3.MoveTowards(transform.parent.position, position, Time.deltaTime * CameraFollowSpeed);
            }
            else
            {
                transform.parent.position = position;
            }
        }
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
        characterXAxis.GetDCLAction();
    }

    private void MovePlayer(Vector2 inputData)
    {
        Debug.Log($"left hand input {inputData}");

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
