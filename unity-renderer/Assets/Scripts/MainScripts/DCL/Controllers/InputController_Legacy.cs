using System;
using System.Collections.Generic;
using DCL.Configuration;
using DCL.Interface;
using UnityEngine;
using UnityEngine.XR;

namespace DCL
{
    public class InputController_Legacy
    {
        public delegate void ButtonListenerCallback(WebInterface.ACTION_BUTTON buttonId, EVENT eventType, bool useRaycast, bool enablePointerEvent);
        
        private static bool renderingEnabled => CommonScriptableObjects.rendererState.Get();
        private static InputController_Legacy instance = null;

        public static InputController_Legacy i
        {
            get
            {
                if (instance == null)
                    instance = new InputController_Legacy();

                return instance;
            }
        }

        public enum EVENT
        {
            BUTTON_DOWN,
            BUTTON_UP
        }

        private enum BUTTON_TYPE
        {
            MOUSE,
            KEYBOARD
        }

        private struct BUTTON_MAP
        {
            public BUTTON_TYPE type;
            public int buttonNum;
            public WebInterface.ACTION_BUTTON buttonId;
            public bool useRaycast;
            public bool enablePointerEvent;
        }

        private bool buttonDown;
        private bool primeDown;
        private bool secondDown;
        private Dictionary<WebInterface.ACTION_BUTTON, List<ButtonListenerCallback>> listeners = new Dictionary<WebInterface.ACTION_BUTTON, List<ButtonListenerCallback>>();
        private List<BUTTON_MAP> buttonsMap = new List<BUTTON_MAP>();

        private InputDevice leftHand;
        private InputDevice rightHand;

        private InputController_Legacy()
        {
            SetInputDevices();
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.MOUSE, buttonNum = 0, buttonId = WebInterface.ACTION_BUTTON.POINTER, useRaycast = true, enablePointerEvent = true});
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.PrimaryButtonKeyCode, buttonId = WebInterface.ACTION_BUTTON.PRIMARY, useRaycast = true, enablePointerEvent = true });
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.SecondaryButtonKeyCode, buttonId = WebInterface.ACTION_BUTTON.SECONDARY, useRaycast = true, enablePointerEvent = true });
            
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.ForwardButtonKeyCode, buttonId = WebInterface.ACTION_BUTTON.FORWARD, useRaycast = false, enablePointerEvent = false });
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.ForwardButtonKeyCodeAlt, buttonId = WebInterface.ACTION_BUTTON.FORWARD, useRaycast = false, enablePointerEvent = false });
            
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.BackwardButtonKeyCode, buttonId = WebInterface.ACTION_BUTTON.BACKWARD, useRaycast = false, enablePointerEvent = false });
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.BackwardButtonKeyCodeAlt, buttonId = WebInterface.ACTION_BUTTON.BACKWARD, useRaycast = false, enablePointerEvent = false });
            
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.RightButtonKeyCode, buttonId = WebInterface.ACTION_BUTTON.RIGHT, useRaycast = false, enablePointerEvent = false });
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.RightButtonKeyCodeAlt, buttonId = WebInterface.ACTION_BUTTON.RIGHT, useRaycast = false, enablePointerEvent = false });
            
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.LeftButtonKeyCode, buttonId = WebInterface.ACTION_BUTTON.LEFT, useRaycast = false, enablePointerEvent = false });
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.LeftButtonKeyCodeAlt, buttonId = WebInterface.ACTION_BUTTON.LEFT, useRaycast = false, enablePointerEvent = false });
            
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.WalkButtonKeyCode, buttonId = WebInterface.ACTION_BUTTON.WALK, useRaycast = false, enablePointerEvent = false });
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.JumpButtonKeyCode, buttonId = WebInterface.ACTION_BUTTON.JUMP, useRaycast = false, enablePointerEvent = false });
            
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.ActionButton3Keycode, buttonId = WebInterface.ACTION_BUTTON.ACTION_3, useRaycast = true, enablePointerEvent = false });
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.ActionButton4Keycode, buttonId = WebInterface.ACTION_BUTTON.ACTION_4, useRaycast = true, enablePointerEvent = false });
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.ActionButton5Keycode, buttonId = WebInterface.ACTION_BUTTON.ACTION_5, useRaycast = true, enablePointerEvent = false });
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.ActionButton6Keycode, buttonId = WebInterface.ACTION_BUTTON.ACTION_6, useRaycast = true, enablePointerEvent = false });
        }
        private void SetInputDevices()
        {
            leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            
            if (!leftHand.isValid || !rightHand.isValid)
            {
                Debug.Log($"left is valid {leftHand.isValid}, right is valid {rightHand.isValid}");
                InputDevices.deviceConnected += OnDeviceConnected;
            }
        }
        private void OnDeviceConnected(InputDevice device)
        {
            InputDevices.deviceConnected -= OnDeviceConnected;
            SetInputDevices();
        }

        public void AddListener(WebInterface.ACTION_BUTTON buttonId, ButtonListenerCallback callback)
        {
            if (!listeners.ContainsKey(buttonId))
                listeners.Add(buttonId, new List<ButtonListenerCallback>());

            if (!listeners[buttonId].Contains(callback))
                listeners[buttonId].Add(callback);
        }

        public void RemoveListener(WebInterface.ACTION_BUTTON buttonId, ButtonListenerCallback callback)
        {
            if (listeners.ContainsKey(buttonId))
            {
                if (listeners[buttonId].Contains(callback))
                {
                    listeners[buttonId].Remove(callback);

                    if (listeners[buttonId].Count == 0)
                        listeners.Remove(buttonId);
                }
            }
        }

        // Note (Zak): it is public for testing purposes only
        public void RaiseEvent(WebInterface.ACTION_BUTTON buttonId, EVENT evt, bool useRaycast, bool enablePointerEvent)
        {
            if (!listeners.ContainsKey(buttonId))
                return;

            List<ButtonListenerCallback> callbacks = listeners[buttonId];
            int count = callbacks.Count;

            for (int i = 0; i < count; i++)
            {
                callbacks[i].Invoke(buttonId, evt, useRaycast, enablePointerEvent);
            }
        }

        public void Update()
        {
            if (!renderingEnabled)
                return;
            int count = buttonsMap.Count;

            for (int i = 0; i < count; i++)
            {
                BUTTON_MAP btnMap = buttonsMap[i];

                if (CommonScriptableObjects.allUIHidden.Get())
                    break;
                
                switch (btnMap.type)
                {
                    case BUTTON_TYPE.MOUSE:
                        if (GetButtonDown(btnMap))
                            RaiseEvent(btnMap.buttonId, EVENT.BUTTON_DOWN, btnMap.useRaycast, btnMap.enablePointerEvent);
                        else if (GetButtonUp(btnMap))
                            RaiseEvent(btnMap.buttonId, EVENT.BUTTON_UP, btnMap.useRaycast, btnMap.enablePointerEvent);
                        break;
                    case BUTTON_TYPE.KEYBOARD:
                        if (GetKeyDown(btnMap))
                            RaiseEvent(btnMap.buttonId, EVENT.BUTTON_DOWN, btnMap.useRaycast, btnMap.enablePointerEvent);
                        else if (GetKeyUp(btnMap))
                            RaiseEvent(btnMap.buttonId, EVENT.BUTTON_UP, btnMap.useRaycast, btnMap.enablePointerEvent);
                        break;
                }
            }
        }
        private bool GetKeyUp(BUTTON_MAP btnMap)
        {
            var code = (KeyCode) btnMap.buttonNum;
            if (!CrossPlatformManager.IsVR)
                return Input.GetKeyUp(code);

            var button = GetButton(code, out var thisFrame);

            return !button && thisFrame;

        }
        private bool GetButton(KeyCode code, out bool thisFrame)
        {
            bool button = default;
            thisFrame = false;
            switch (code)
            {
                case KeyCode.E :
                    leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out button);
                    if (primeDown != button)
                    {
                        primeDown = button;
                        thisFrame = true;
                    }
                    break;
                case KeyCode.F :
                    leftHand.TryGetFeatureValue(CommonUsages.secondaryButton, out button);
                    if (secondDown != button)
                    {
                        secondDown = button;
                        thisFrame = true;
                    }
                    break;
            }
            return button;
        }
        private bool GetKeyDown(BUTTON_MAP btnMap)
        {
            var code = (KeyCode) btnMap.buttonNum;
            if (!CrossPlatformManager.IsVR)
                return Input.GetKeyDown(code);

            var button = GetButton(code, out var thisFrame);

            return button && thisFrame;
        }
        private bool GetButtonUp(BUTTON_MAP btnMap)
        {
            if (!CrossPlatformManager.IsVR)
                return Input.GetMouseButtonUp(btnMap.buttonNum);
            
            rightHand.TryGetFeatureValue(CommonUsages.trigger, out float value);

            if (!buttonDown || value > .25f)
                return false;
            buttonDown = false;
            return true;

        }
        private bool GetButtonDown(BUTTON_MAP btnMap)
        {
            if (!CrossPlatformManager.IsVR)
                return Input.GetMouseButtonDown(btnMap.buttonNum);
            
            rightHand.TryGetFeatureValue(CommonUsages.trigger, out float value);

            if (buttonDown || value < .75f)
                return false;
            buttonDown = true;
            return true;
        }

        public bool IsPressed(WebInterface.ACTION_BUTTON button)
        {
            rightHand.TryGetFeatureValue(CommonUsages.trigger, out float r);
            switch (button)
            {
                case WebInterface.ACTION_BUTTON.POINTER:
                    if (!CrossPlatformManager.IsVR)
                        return Input.GetMouseButton(0);
                    return r > .7f;
                case WebInterface.ACTION_BUTTON.PRIMARY:
                    if (!CrossPlatformManager.IsVR)
                        return Input.GetKey(InputSettings.PrimaryButtonKeyCode);
                    leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool prime);
                    return prime;
                case WebInterface.ACTION_BUTTON.SECONDARY:
                    if (!CrossPlatformManager.IsVR)
                        return Input.GetKey(InputSettings.SecondaryButtonKeyCode);
                    leftHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bool second);
                    return second;
                default: // ANY
                    if (!CrossPlatformManager.IsVR) return Input.GetMouseButton(0) ||
                                                          Input.GetKey(InputSettings.PrimaryButtonKeyCode) ||
                                                          Input.GetKey(InputSettings.SecondaryButtonKeyCode);
                    return r > .7f;
            }
        }
    }
}