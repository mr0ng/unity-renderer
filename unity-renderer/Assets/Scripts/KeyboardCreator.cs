using System;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using TMPro;
using UnityEngine;

namespace DCL.Interface
{
    public class KeyboardCreator : MonoBehaviour
    {
        private static GameObject canvasKeyboard;
        private static NonNativeKeyboard keyboard;
        private static Transform _keyboardTrans;
        private static Transform keyboardTrans
        {
            get
            {
                if (_keyboardTrans != null)
                    return _keyboardTrans;
                Create();
                return _keyboardTrans;
            }
        }
        // Add static field for active KeyboardCreator
        private static KeyboardCreator activeCreator;

        private static void Create()
        {
            #if !DCL_VR
            return;
            #endif
            canvasKeyboard = Instantiate((GameObject)Resources.Load("Prefabs/KeyboardCanvas"));
            keyboard = NonNativeKeyboard.Instance;
            _keyboardTrans = canvasKeyboard.transform;
            Canvas canvas = keyboard.transform.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 105;
            canvas.sortingLayerName = "menu";
            // Set the layer of the keyboard and all its children to UI.
            SetLayerRecursively(canvasKeyboard, LayerMask.NameToLayer("UI"));
            SetLayerRecursively(keyboard.gameObject, LayerMask.NameToLayer("UI"));
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

        private TMP_InputField tmpInputField;

        private void Awake()
        {
#if !DCL_VR
            return;
#endif
            tmpInputField = GetComponent<TMP_InputField>();
            tmpInputField.onSelect.AddListener(OpenKeyboard);
        }

        private void Close(string arg0)
        {
            // Check if this was the active input field
            if (activeCreator == this)
            {
                // Clear the active field
                activeCreator = null;
            }

            CleanUpEvents();
        }

        private void OpenKeyboard(string arg0)
        {
            if (keyboardTrans == null)
                return;
            CleanUpEvents();
            // Set this KeyboardCreator as the active one
            activeCreator = this;

            SetupEvents();
            keyboard.PresentKeyboard(NonNativeKeyboard.LayoutType.URL);
            var rawForward = CommonScriptableObjects.cameraForward.Get();
            keyboardTrans.position = CommonScriptableObjects.cameraPosition.Get() + (.7f * rawForward) + new UnityEngine.Vector3(0, -0.38f, 0);
            // Tilt the keyboard upwards by adjusting the x rotation
            keyboardTrans.rotation = Quaternion.Euler(-30f, keyboardTrans.rotation.eulerAngles.y, keyboardTrans.rotation.eulerAngles.z);

            keyboardTrans.forward = new UnityEngine.Vector3(rawForward.x, 0, rawForward.z);
            canvasKeyboard.SetActive(true);
        }

        private void HandleSubmit(object sender, EventArgs e)
        {
            // Only set the text if this is the active input field
            if (activeCreator == this)
            {
                tmpInputField.text = keyboard.InputField.text;
                tmpInputField.onSubmit.Invoke(tmpInputField.text);
            }
        }

        private void CleanUpEvents() { keyboard.OnTextSubmitted -= HandleSubmit; }

        private void SetupEvents() { keyboard.OnTextSubmitted += HandleSubmit; }
    }

}
