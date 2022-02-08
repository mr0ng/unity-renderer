using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ongvrsystem : InputSystemGlobalHandlerListener, IMixedRealityInputHandler<Vector2>, IMixedRealityInputActionHandler
{

    public MixedRealityInputAction moveAction;
    public float speed = 0.1f;
    public float CameraFollowSpeed = 100f;
    public float MaxDistCameraPlayer = 20;
    private Transform characterController;

    // Start is called before the first frame update
    void Start()
    {
        //CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
        //CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler<Vector2>>(this);
        StartCoroutine(PlaceMainCameraAtAvatar());
    }

    void OnDisable()
    {
        //CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
        //CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputHandler<Vector2>>(this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator PlaceMainCameraAtAvatar()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);

            //See if the avatar's eyes have loaded yet - that's where we'd want to put the camera
            //Make sure it's when the avatar is put up at the high area (TODO: Find out if there's a callback we can listen for instead)
            if (GameObject.Find("AvatarRenderer") != null && GameObject.Find("AvatarRenderer").transform.position.y>50f)
            {


                GameObject mixedRealityPlayspace = GameObject.Find("MixedRealityPlayspace");


                //We don't want to move the camera itself, but rather the parent of the camera to where the eyes are
                characterController = GameObject.Find("CharacterController").transform;
                transform.parent.position = GameObject.Find("AvatarRenderer").transform.position;
                transform.parent.position = new Vector3(transform.parent.position.x, transform.parent.position.y+1f, transform.parent.position.z);
                Debug.Log("Moved camera parent to AvatarRenderer Location");

                //Turn off MRTK Camera. Bad programming - feel free to improve.
                mixedRealityPlayspace.transform.Find("Main Camera").gameObject.SetActive(false);

                //Move and Reparent the MRplayspace to the characterController so that we always have our controllers with us
                mixedRealityPlayspace.transform.position = transform.parent.position;
                mixedRealityPlayspace.transform.parent = transform.parent;


                //Once the avatar is found and the camera is moved, then break out of the while loop
                break;
            }
        }

        //StartCoroutine(MoveAvatarWithCamera());
        StartCoroutine(VRLocomotion());
    }

    private IEnumerator MoveAvatarWithCamera()
    {
        yield return null;

        

        Debug.Log("Found Camera Controller: " + characterController);

        while (true)
        {
            characterController.transform.position = transform.position - transform.forward*0.2f;
            characterController.transform.rotation = Quaternion.Euler(characterController.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, characterController.transform.rotation.eulerAngles.z);
            yield return null;
        }
    }

    private IEnumerator VRLocomotion()
    {
        yield return null;



        while(true)
        {
            yield return null;

            float axisX = -Input.GetAxis("AXIS_4");
            float axisY = -Input.GetAxis("AXIS_5");
            if (Math.Abs(axisX) > 0.1f || Math.Abs(axisY) > 0.1f)
            {
                //transform.parent.position = transform.parent.position + new Vector3(transform.forward.x, 0f, transform.forward.z) * axisY * speed;
                //transform.parent.position = transform.parent.position + Vector3.Cross(transform.forward, Vector3.up) * axisX * speed;

                characterController.position = characterController.position + new Vector3(transform.forward.x, 0f, transform.forward.z) * axisY * speed;
                characterController.position = characterController.position + Vector3.Cross(transform.forward, Vector3.up) * axisX * speed;



            }

            characterController.transform.rotation = Quaternion.Euler(characterController.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, characterController.transform.rotation.eulerAngles.z);

            //If the camera is near the avatar, smoothly follow it. Otherwise, teleport to it
            Debug.Log("Distance between camera and player: " + Vector3.Distance(transform.parent.position, characterController.position));
            if (Vector3.Distance(transform.parent.position, characterController.position) < 20)
            {
                transform.parent.position = Vector3.MoveTowards(transform.parent.position, characterController.position, Time.deltaTime * CameraFollowSpeed);
            }
            else
            {
                transform.parent.position = characterController.position;
            }
        }

    }

    public void OnInputChanged(InputEventData<Vector2> eventData)
    {
        Debug.Log("ACTION Thumbstick was pressed.");

        //if (eventData.MixedRealityInputAction == moveAction)
        //{
        //    Vector3 localDelta = speed * (Vector3)eventData.InputData;
        //    transform.parent.position = transform.parent.position + transform.rotation * localDelta;
        //}
    }

    public void OnActionStarted(BaseInputEventData eventData)
    {
        Debug.Log("Thumbstick was pressed.");

        //if (eventData.MixedRealityInputAction == moveAction)
        //{            
        //    Vector3 localDelta = speed * (Vector3)eventData.InputData;
        //    transform.parent.position = transform.parent.position + transform.rotation * localDelta;
        //}
    }

    public void OnActionEnded(BaseInputEventData eventData)
    {
        
    }

    protected override void RegisterHandlers()
    {
        //CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
        //CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler<Vector2>>(this);
    }

    protected override void UnregisterHandlers()
    {
        //CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
        //CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputHandler<Vector2>>(this);
    }
}
