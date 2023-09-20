using Microsoft.MixedReality.Toolkit.Experimental.UI;
using TMPro;
using UnityEngine;
using System;
using System.Collections;

namespace DCL.Interface
{
    public class KeyboardManager : MonoBehaviour
    {
        public static KeyboardManager Instance;

        private static GameObject canvasKeyboard;
        public static NonNativeKeyboard keyboard;
        private static Transform _keyboardTrans;

        private static TMP_InputField activeInputField;
        static int previousActiveInputCaretPosition = 0;
        static int previousKeyboardInputCaretPosition = 0;
        private static bool isFocused = false;
        private void Awake()
        {
            // Setup singleton
            if (Instance == null)
            {
                Instance = this;
                // DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            Create();
            activeInputField = null;
        }

        private static void Create()
        {
#if !DCL_VR
            return;
#endif
            canvasKeyboard = Instantiate((GameObject)Resources.Load("Prefabs/KeyboardCanvas"));
            keyboard = NonNativeKeyboard.Instance;
            keyboard.CloseOnInactivity = false;
            _keyboardTrans = canvasKeyboard.transform;
            Canvas canvas = keyboard.transform.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1005;
            canvas.sortingLayerName = "menu";
            // Set the layer of the keyboard and all its children to UI.
            SetLayerRecursively(canvasKeyboard, LayerMask.NameToLayer("UI"));
            SetLayerRecursively(keyboard.gameObject, LayerMask.NameToLayer("UI"));
            keyboard.OnClosed += CloseKeyboard;
        }

        private static void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (null == obj)
            {
                return;
            }

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                if (null == child)
                {
                    continue;
                }
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        public void OpenKeyboard(TMP_InputField inputField)
        {
            // keyboard.ShowAlphaKeyboard();
            // keyboard.AlphaSubKeys.enabled = true;
            // keyboard.PresentKeyboard();
            // keyboard.InputField.gameObject.SetActive(false);
            if (canvasKeyboard == null || keyboard == null || _keyboardTrans == null)
                Create();

            if (keyboard.isActiveAndEnabled && activeInputField == inputField) // If keyboard is already open for the same field
            {

                keyboard.InputField.caretPosition = activeInputField.caretPosition;
                keyboard.InputField.stringPosition = activeInputField.stringPosition;
                previousActiveInputCaretPosition = activeInputField.caretPosition;
                previousKeyboardInputCaretPosition = activeInputField.caretPosition;
                return;
            }

            activeInputField = inputField;
            CleanUpEvents();
            keyboard.InputField.text = activeInputField.text;
            int cursorPos = inputField.caretPosition;  // or calculate based on click position if needed
            keyboard.InputField.caretPosition = cursorPos;
            keyboard.InputField.stringPosition = cursorPos;
            previousActiveInputCaretPosition = cursorPos;
             previousKeyboardInputCaretPosition = cursorPos;
            SetupEvents();
            keyboard.PresentKeyboard(NonNativeKeyboard.LayoutType.Alpha);
            // Position and orientation code
            var rawForward = CommonScriptableObjects.cameraForward.Get();
            _keyboardTrans.position = CommonScriptableObjects.cameraPosition.Get() + (.7f * rawForward) + new UnityEngine.Vector3(0, -0.38f, 0);

            // Compute the rotation that points the keyboard forward
            Quaternion forwardRotation = Quaternion.LookRotation(new UnityEngine.Vector3(rawForward.x, 0, rawForward.z));

            // Apply a 30-degree tilt upwards
            Quaternion tiltUpRotation = Quaternion.Euler(30f, 0f, 0f);

            // Combine the forward rotation with the tilt
            _keyboardTrans.rotation = forwardRotation * tiltUpRotation;

            canvasKeyboard.SetActive(true);
            StartCoroutine(TrackCursorChangeCoroutine());
        }

        private void HandleSubmit(object sender, EventArgs e)
        {
            if (activeInputField != null)
            {
                activeInputField.text = keyboard.InputField.text;
                activeInputField.onSubmit.Invoke(activeInputField.text);
            }
            // CloseKeyboard();
        }


        public static void CloseKeyboard(object sender, EventArgs eventArgs)
        {
            KeyboardManager.Instance.StopCoroutine(TrackCursorChangeCoroutine());
            canvasKeyboard.SetActive(false);
            KeyboardManager.Instance.CleanUpEvents();
            activeInputField = null;
            Destroy(canvasKeyboard);
        }

        public void UpdateKeyboardText(string newText)
        {
            keyboard.InputField.text = newText;
        }

        private void HandleTextUpdated(string s)
        {
            if (activeInputField != null && s != "")
            {
                int selectionStart = Mathf.Min(keyboard.InputField.selectionAnchorPosition, keyboard.InputField.selectionFocusPosition);
                int selectionEnd = Mathf.Max(keyboard.InputField.selectionAnchorPosition, keyboard.InputField.selectionFocusPosition);

                // Check if there's a range selected
                if (selectionStart != selectionEnd)
                {
                    string leftPart = activeInputField.text.Substring(0, selectionStart);
                    string rightPart = activeInputField.text.Substring(selectionEnd);

                    // Deduce the new character from the difference in s and activeInputField.text
                    string difference = FindDifference(activeInputField.text, s);

                    activeInputField.text = leftPart + difference + rightPart;
                    activeInputField.caretPosition = selectionStart + difference.Length;
                    activeInputField.selectionFocusPosition = activeInputField.caretPosition;
                    keyboard.InputField.selectionAnchorPosition = activeInputField.caretPosition;
                    keyboard.InputField.selectionFocusPosition = activeInputField.caretPosition;
                    keyboard.InputField.text = activeInputField.text;
                }
                else
                {
                    activeInputField.text = keyboard.InputField.text;
                    activeInputField.caretPosition = keyboard.InputField.caretPosition;
                    activeInputField.stringPosition = keyboard.InputField.stringPosition;
                }
            }
        }

private string FindDifference(string oldText, string newText)
{
    int minLength = Mathf.Min(oldText.Length, newText.Length);

    int startDifference = 0;
    while (startDifference < minLength && oldText[startDifference] == newText[startDifference])
    {
        startDifference++;
    }

    int endDifferenceOld = oldText.Length - 1;
    int endDifferenceNew = newText.Length - 1;
    while (endDifferenceOld >= startDifference && endDifferenceNew >= startDifference && oldText[endDifferenceOld] == newText[endDifferenceNew])
    {
        endDifferenceOld--;
        endDifferenceNew--;
    }

    if (endDifferenceNew >= startDifference)
        return newText.Substring(startDifference, endDifferenceNew - startDifference + 1);
    else
        return string.Empty;
}



        private void SetupEvents()
        {
            activeInputField.onSelect.AddListener(OnInputFieldSelect);
            activeInputField.onDeselect.AddListener(OnInputFieldDeselect);
            keyboard.OnTextSubmitted += HandleSubmit;
            keyboard.OnTextUpdated += HandleTextUpdated;
        }

        void OnInputFieldSelect(string text)
        {
            isFocused = true;
        }

        void OnInputFieldDeselect(string text)
        {
            isFocused = false;
        }

        private void CleanUpEvents()
        {
            activeInputField.onSelect.RemoveListener(OnInputFieldSelect);
            activeInputField.onDeselect.RemoveListener(OnInputFieldDeselect);
            keyboard.OnTextSubmitted -= HandleSubmit;
            keyboard.OnTextUpdated -= HandleTextUpdated;
        }
        public bool IsKeyboardOpenFor(TMP_InputField inputField)
        {
            if (canvasKeyboard == null) return false;
            return (canvasKeyboard.activeSelf && activeInputField == inputField);
        }

        private static IEnumerator TrackCursorChangeCoroutine()
        {


            while (keyboard != null && activeInputField != null && keyboard.isActiveAndEnabled)
            {
                // Check if the caret position of the active input field has changed (indicating a click)


                // Check if the caret position of the keyboard input field has changed
                 if (previousKeyboardInputCaretPosition != keyboard.InputField.caretPosition && !Input.anyKeyDown)
                {
                    activeInputField.caretPosition = keyboard.InputField.caretPosition;
                    activeInputField.stringPosition = keyboard.InputField.stringPosition;
                }
                 else if (isFocused && previousActiveInputCaretPosition != activeInputField.caretPosition && !Input.anyKeyDown)
                {
                    keyboard.InputField.caretPosition = activeInputField.caretPosition;
                    keyboard.InputField.stringPosition = activeInputField.stringPosition;

                }
                // Update the tracked caret positions
                previousActiveInputCaretPosition = activeInputField.caretPosition;
                previousKeyboardInputCaretPosition = keyboard.InputField.caretPosition;

                yield return new WaitForSeconds(0.01f);
            }
        }

    }

}
