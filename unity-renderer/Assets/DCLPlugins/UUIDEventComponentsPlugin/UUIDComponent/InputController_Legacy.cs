using System;
using System.Collections.Generic;
using DCL.Configuration;
using DCL.Interface;
using UnityEngine;
using InputSettings = DCL.Configuration.InputSettings;

namespace DCL
{
    public class InputController_Legacy : IDisposable
    {
        public delegate void ButtonListenerCallback(WebInterface.ACTION_BUTTON buttonId, EVENT eventType,
            bool useRaycast, bool enablePointerEvent);

        private static bool renderingEnabled => CommonScriptableObjects.rendererState.Get();
		#if DCL_VR
        private static DCLPlayerInput.PlayerActions actions;
#endif
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
			#if DCL_VR
            public bool lastState;
			#endif
        }

        private Dictionary<WebInterface.ACTION_BUTTON, List<ButtonListenerCallback>> listeners =
            new Dictionary<WebInterface.ACTION_BUTTON, List<ButtonListenerCallback>>();

        private List<BUTTON_MAP> buttonsMap = new List<BUTTON_MAP>();


        public InputController_Legacy()
        {
		#if DCL_VR
            InputController.GetPlayerActions(ref actions);
			#endif
            buttonsMap.Add(new BUTTON_MAP()
            {
                type = BUTTON_TYPE.MOUSE, buttonNum = 0, buttonId = WebInterface.ACTION_BUTTON.POINTER,
                useRaycast = true, enablePointerEvent = true
            });
            buttonsMap.Add(new BUTTON_MAP()
            {
                type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.PrimaryButtonKeyCode,
                buttonId = WebInterface.ACTION_BUTTON.PRIMARY, useRaycast = true, enablePointerEvent = true
            });
            buttonsMap.Add(new BUTTON_MAP()
            {
                type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.SecondaryButtonKeyCode,
                buttonId = WebInterface.ACTION_BUTTON.SECONDARY, useRaycast = true, enablePointerEvent = true
            });

            buttonsMap.Add(new BUTTON_MAP()
            {
                type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.ForwardButtonKeyCode,
                buttonId = WebInterface.ACTION_BUTTON.FORWARD, useRaycast = false, enablePointerEvent = false
            });
            buttonsMap.Add(new BUTTON_MAP()
            {
                type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.ForwardButtonKeyCodeAlt,
                buttonId = WebInterface.ACTION_BUTTON.FORWARD, useRaycast = false, enablePointerEvent = false
            });

            buttonsMap.Add(new BUTTON_MAP()
            {
                type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.BackwardButtonKeyCode,
                buttonId = WebInterface.ACTION_BUTTON.BACKWARD, useRaycast = false, enablePointerEvent = false
            });
            buttonsMap.Add(new BUTTON_MAP()
            {
                type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.BackwardButtonKeyCodeAlt,
                buttonId = WebInterface.ACTION_BUTTON.BACKWARD, useRaycast = false, enablePointerEvent = false
            });

            buttonsMap.Add(new BUTTON_MAP()
            {
                type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.RightButtonKeyCode,
                buttonId = WebInterface.ACTION_BUTTON.RIGHT, useRaycast = false, enablePointerEvent = false
            });
            buttonsMap.Add(new BUTTON_MAP()
            {
                type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.RightButtonKeyCodeAlt,
                buttonId = WebInterface.ACTION_BUTTON.RIGHT, useRaycast = false, enablePointerEvent = false
            });

            buttonsMap.Add(new BUTTON_MAP()
            {
                type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.LeftButtonKeyCode,
                buttonId = WebInterface.ACTION_BUTTON.LEFT, useRaycast = false, enablePointerEvent = false
            });
            buttonsMap.Add(new BUTTON_MAP()
            {
                type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.LeftButtonKeyCodeAlt,
                buttonId = WebInterface.ACTION_BUTTON.LEFT, useRaycast = false, enablePointerEvent = false
            });

            buttonsMap.Add(new BUTTON_MAP()
            {
                type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.WalkButtonKeyCode,
                buttonId = WebInterface.ACTION_BUTTON.WALK, useRaycast = false, enablePointerEvent = false
            });
            buttonsMap.Add(new BUTTON_MAP()
            {
                type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.JumpButtonKeyCode,
                buttonId = WebInterface.ACTION_BUTTON.JUMP, useRaycast = false, enablePointerEvent = false
            });

            buttonsMap.Add(new BUTTON_MAP()
            {
                type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.ActionButton3Keycode,
                buttonId = WebInterface.ACTION_BUTTON.ACTION_3, useRaycast = true, enablePointerEvent = false
            });
            buttonsMap.Add(new BUTTON_MAP()
            {
                type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.ActionButton4Keycode,
                buttonId = WebInterface.ACTION_BUTTON.ACTION_4, useRaycast = true, enablePointerEvent = false
            });
            buttonsMap.Add(new BUTTON_MAP()
            {
                type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.ActionButton5Keycode,
                buttonId = WebInterface.ACTION_BUTTON.ACTION_5, useRaycast = true, enablePointerEvent = false
            });
            buttonsMap.Add(new BUTTON_MAP()
            {
                type = BUTTON_TYPE.KEYBOARD, buttonNum = (int) InputSettings.ActionButton6Keycode,
                buttonId = WebInterface.ACTION_BUTTON.ACTION_6, useRaycast = true, enablePointerEvent = false
            });

            Environment.i.platform.updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
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
        //private int updateSkip = 0;
        public void Update()
        {

            if (!renderingEnabled)
                return;
            // updateSkip = (updateSkip + 1 ) % 25;
            // if (updateSkip != 0)
            //     return;
            int count = buttonsMap.Count;

            for (int i = 0; i < count; i++)
            {

                BUTTON_MAP btnMap = buttonsMap[i];

                switch (btnMap.type)
                {
                    case BUTTON_TYPE.MOUSE:
                        if (CommonScriptableObjects.allUIHidden.Get())
                            break;
					#if DCL_VR
                        if (Input.GetMouseButtonDown(btnMap.buttonNum) && !btnMap.lastState)
                            RaiseEvent(btnMap.buttonId, EVENT.BUTTON_DOWN, btnMap.useRaycast,
                                btnMap.enablePointerEvent);

                        else if (Input.GetMouseButtonUp(btnMap.buttonNum) && btnMap.lastState)
                            RaiseEvent(btnMap.buttonId, EVENT.BUTTON_UP, btnMap.useRaycast, btnMap.enablePointerEvent);
					#else
						if (Input.GetMouseButtonDown(btnMap.buttonNum))
                            RaiseEvent(btnMap.buttonId, EVENT.BUTTON_DOWN, btnMap.useRaycast,
                                btnMap.enablePointerEvent);
                        else if (Input.GetMouseButtonUp(btnMap.buttonNum))
                            RaiseEvent(btnMap.buttonId, EVENT.BUTTON_UP, btnMap.useRaycast, btnMap.enablePointerEvent);
					#endif
                        break;
                    case BUTTON_TYPE.KEYBOARD:
                        if (CommonScriptableObjects.allUIHidden.Get())
                            break;
					#if DCL_VR
                        if (Input.GetKeyDown((KeyCode) btnMap.buttonNum) && !btnMap.lastState)
                            RaiseEvent(btnMap.buttonId, EVENT.BUTTON_DOWN, btnMap.useRaycast,
                                btnMap.enablePointerEvent);
                        else if (!Input.GetKeyDown((KeyCode) btnMap.buttonNum)&& btnMap.lastState)
                            RaiseEvent(btnMap.buttonId, EVENT.BUTTON_UP, btnMap.useRaycast, btnMap.enablePointerEvent);
					#else
						if (Input.GetKeyDown((KeyCode) btnMap.buttonNum))
                            RaiseEvent(btnMap.buttonId, EVENT.BUTTON_DOWN, btnMap.useRaycast,
                                btnMap.enablePointerEvent);
                        else if (Input.GetKeyUp((KeyCode) btnMap.buttonNum))
                            RaiseEvent(btnMap.buttonId, EVENT.BUTTON_UP, btnMap.useRaycast, btnMap.enablePointerEvent);
					#endif
                        break;
                }
            }
        }
        #if DCL_VR
        private static bool GetButtonUp(BUTTON_MAP btnMap)
        {

            return !actions.Select.triggered;
        }
        private static bool GetButtonDown(BUTTON_MAP btnMap)
        {
            return actions.Select.triggered;
        }

        private static bool GetKeyDown(KeyCode code)
        {
            switch (code)
            {
                case KeyCode.E:
                    return actions.PrimaryInteraction.triggered;
                case KeyCode.F:
                    return actions.SecondaryInteraction.triggered;
                default:
                    return default;
            }
        }
        #endif
        public bool IsPressed(WebInterface.ACTION_BUTTON button)
        {
            switch (button)
            {
			#if DCL_VR

			case WebInterface.ACTION_BUTTON.POINTER:
                    return actions.Select.triggered;
                case WebInterface.ACTION_BUTTON.PRIMARY:
                    return actions.PrimaryInteraction.triggered;
                case WebInterface.ACTION_BUTTON.SECONDARY:
                    return actions.SecondaryInteraction.triggered;
                default: // ANY
                    return actions.Select.triggered ||
                           actions.PrimaryInteraction.triggered ||
                           actions.SecondaryInteraction.triggered;
			#else

                case WebInterface.ACTION_BUTTON.POINTER:
                    return Input.GetMouseButton(0);
                case WebInterface.ACTION_BUTTON.PRIMARY:
                    return Input.GetKey(InputSettings.PrimaryButtonKeyCode);
                case WebInterface.ACTION_BUTTON.SECONDARY:
                    return Input.GetKey(InputSettings.SecondaryButtonKeyCode);
                default: // ANY
                    return Input.GetMouseButton(0) ||
                           Input.GetKey(InputSettings.PrimaryButtonKeyCode) ||
                           Input.GetKey(InputSettings.SecondaryButtonKeyCode);
			#endif
            }
        }

        public void Dispose()
        {
            Environment.i.platform.updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
        }
    }
}
