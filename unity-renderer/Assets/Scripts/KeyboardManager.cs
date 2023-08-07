using Microsoft.MixedReality.Toolkit.Experimental.UI;
using TMPro;
using UnityEngine;
using System;

namespace DCL.Interface
{
    public class KeyboardManager : MonoBehaviour
    {
        public static KeyboardManager Instance;

        private static GameObject canvasKeyboard;
        private static NonNativeKeyboard keyboard;
        private static Transform _keyboardTrans;

        private TMP_InputField activeInputField;

        private void Awake()
        {
            // Setup singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
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
            _keyboardTrans = canvasKeyboard.transform;
            Canvas canvas = keyboard.transform.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1005;
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

        public void OpenKeyboard(TMP_InputField inputField)
        {
            if (_keyboardTrans == null)
                return;

            activeInputField = inputField;
            CleanUpEvents();
            keyboard.InputField.text = activeInputField.text;
            SetupEvents();
            keyboard.PresentKeyboard(NonNativeKeyboard.LayoutType.URL);
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
        }

        private void HandleSubmit(object sender, EventArgs e)
        {
            if (activeInputField != null)
            {
                activeInputField.text = keyboard.InputField.text;
                activeInputField.onSubmit.Invoke(activeInputField.text);
            }
        }

        private void CleanUpEvents() { keyboard.OnTextSubmitted -= HandleSubmit; }

        private void SetupEvents() { keyboard.OnTextSubmitted += HandleSubmit; }
    }
}
