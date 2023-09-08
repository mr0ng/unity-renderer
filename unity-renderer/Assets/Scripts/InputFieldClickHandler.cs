using DCL.Interface;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Microsoft.MixedReality.Toolkit.Input;

public class InputFieldClickHandler : MonoBehaviour
{
    public InputActionAsset inputActions; // Reference to your Input Action Asset
    private InputAction selectAction; // The specific action you want to listen to

    private void Awake()
    {
        // Find the "Select" action
        selectAction = inputActions.FindAction("Select");

        // Register the callback
        selectAction.performed += context => CheckForInputField();
    }

    private void OnEnable()
    {
        // Enable the action when the object is enabled
        selectAction.Enable();
    }

    private void OnDisable()
    {
        // Disable the action when the object is disabled
        selectAction.Disable();
    }

    public void CheckForInputField()
    {
        if (CoreServices.FocusProvider.PrimaryPointer == null || CoreServices.FocusProvider.PrimaryPointer.Result.Details.Object == null)
        {
            return; // No hit, exit early
        }

        // Get the hit GameObject
        GameObject hitObject = CoreServices.FocusProvider.PrimaryPointer.Result.Details.Object;

        TMP_InputField hitInputField = hitObject.GetComponentInParent<TMP_InputField>();

        if (hitInputField != null && hitInputField == KeyboardManager.keyboard.InputField)
        {
            return;
        }
        // Check for TMP_InputField on the hit object or its parents
        TMP_InputField inputField = hitObject.GetComponentInParent<TMP_InputField>();

        if (inputField != null)
        {
            if (KeyboardManager.Instance.IsKeyboardOpenFor(inputField))
            {
                KeyboardManager.Instance.UpdateKeyboardText(inputField.text);
            }
            else
            {
                KeyboardManager.Instance.OpenKeyboard(inputField);
            }
        }
    }
    
}
