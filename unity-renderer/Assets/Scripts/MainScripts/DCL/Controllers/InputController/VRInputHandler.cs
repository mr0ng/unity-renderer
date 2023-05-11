// using UnityEngine;
// using UnityEngine.InputSystem;
//
// public class VRInputHandler : MonoBehaviour
// {
//     public InputActionAsset vrInputActions;
//
//     private InputAction characterXAxis;
//     private InputAction characterYAxis;
//     private InputAction cameraXAxis;
//     private InputAction cameraYAxis;
//     private InputAction mouseWheel;
//
//     private void Awake()
//     {
//         // Set up the input actions
//         characterXAxis = vrInputActions.FindAction("CharacterXAxis");
//         characterYAxis = vrInputActions.FindAction("CharacterYAxis");
//         cameraXAxis = vrInputActions.FindAction("CameraXAxis");
//         cameraYAxis = vrInputActions.FindAction("CameraYAxis");
//         mouseWheel = vrInputActions.FindAction("MouseWheel");
//     }
//
//     private void OnEnable()
//     {
//         characterXAxis.Enable();
//         characterYAxis.Enable();
//         cameraXAxis.Enable();
//         cameraYAxis.Enable();
//         mouseWheel.Enable();
//     }
//
//     private void OnDisable()
//     {
//         characterXAxis.Disable();
//         characterYAxis.Disable();
//         cameraXAxis.Disable();
//         cameraYAxis.Disable();
//         mouseWheel.Disable();
//     }
//
//     public float GetCharacterXAxisValue()
//     {
//         return characterXAxis.ReadValue<float>();
//     }
//
//     public float GetCharacterYAxisValue()
//     {
//         return characterYAxis.ReadValue<float>();
//     }
//
//     public float GetCameraXAxisValue()
//     {
//         return cameraXAxis.ReadValue<float>();
//     }
//
//     public float GetCameraYAxisValue()
//     {
//         return cameraYAxis.ReadValue<float>();
//     }
//
//     public float GetMouseWheelValue()
//     {
//         return mouseWheel.ReadValue<float>();
//     }
// }
