using DCL.Huds;
using System.Collections;
using UnityEngine;

public class UIFaceUser : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float maxViewAngle = 90.0f;
    [SerializeField] private float rotationSpeed = 1.0f;
    [SerializeField] private float checkInterval = 0.2f;  // Check every 200 milliseconds
    private WaitForSeconds checkIntervalWT;
    private void Start()
    {
        checkIntervalWT = new WaitForSeconds(checkInterval);
        StartCoroutine(CheckViewAngle());
    }

    private IEnumerator CheckViewAngle()
    {
        while (true)
        {
            Vector3 toObject = transform.position - mainCamera.transform.position;
            toObject.y = 0; // Ignore vertical difference

            Vector3 cameraForwardHorizontal = mainCamera.transform.forward;
            cameraForwardHorizontal.y = 0; // Ignore vertical difference

            float viewAngle = Vector3.Angle(cameraForwardHorizontal, toObject);

            if (viewAngle > maxViewAngle)
            {
                Position();
                // Vector3 toCamera = mainCamera.transform.position - transform.position;
                // toCamera.y = 0; // Ignore vertical difference
                //
                // Quaternion targetRotation = Quaternion.LookRotation(-toCamera);
                // transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }


            yield return checkIntervalWT;
        }
    }

    private void Position()
    {
        transform.localPosition = Vector3.zero;

        var forward = VRHUDController.I.GetForward();
        if (Camera.main != null)
            transform.position = Camera.main.transform.position+ 1.9f*forward + 1.4f * Vector3.down ;

        transform.forward = forward;

    }
}
