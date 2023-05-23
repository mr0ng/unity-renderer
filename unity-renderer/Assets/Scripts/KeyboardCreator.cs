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

        private static void Create()
        {
            #if !DCL_VR
            return;
            #endif
            canvasKeyboard = Instantiate((GameObject)Resources.Load("Prefabs/KeyboardCanvas"));
            keyboard = NonNativeKeyboard.Instance;
            _keyboardTrans = canvasKeyboard.transform;
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

        private void Close(string arg0) { CleanUpEvents(); }

        private void OpenKeyboard(string arg0)
        {

            if (keyboardTrans == null)
                return;
            CleanUpEvents();
            SetupEvents();
            keyboard.PresentKeyboard(NonNativeKeyboard.LayoutType.URL);
            var rawForward = CommonScriptableObjects.cameraForward.Get();
            keyboardTrans.position = CommonScriptableObjects.cameraPosition.Get() + (.7f * rawForward) + new UnityEngine.Vector3(0, -0.35f, 0);
            // Tilt the keyboard upwards by adjusting the x rotation
            keyboardTrans.rotation = Quaternion.Euler(-10f, keyboardTrans.rotation.eulerAngles.y, keyboardTrans.rotation.eulerAngles.z);

            keyboardTrans.forward = new UnityEngine.Vector3(rawForward.x, 0, rawForward.z);
            canvasKeyboard.SetActive(true);
        }

        private void HandleSubmit(object sender, EventArgs e)
        {
            tmpInputField.text = keyboard.InputField.text;
            tmpInputField.onSubmit.Invoke(tmpInputField.text);
        }

        private void CleanUpEvents() { keyboard.OnTextSubmitted -= HandleSubmit; }

        private void SetupEvents() { keyboard.OnTextSubmitted += HandleSubmit; }
    }

}
