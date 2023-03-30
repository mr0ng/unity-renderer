using DCL;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DCL.Configuration;
using System.Collections.Generic;

/// <summary>
/// Mapping for Trigger actions
/// </summary>
public enum DCLAction_Trigger
{
    //Remember to explicitly assign the value to each entry so we minimize issues with serialization + conflicts
    CameraChange = 100,
    CursorUnlock = 101,

    ToggleNavMap = 110,
    ToggleFriends = 120,
    CloseWindow = 121,
    ToggleWorldChat = 122,
    ToggleUIVisibility = 123,
    ToggleControlsHud = 124,
    ToggleSettings = 125,
    ToggleStartMenu = 126,
    ToggleVoiceChatRecording = 127,
    ToggleAvatarEditorHud = 128,
    ToggleQuestsPanelHud = 129,
    ToggleAvatarNamesHud = 130,
    TogglePlacesAndEventsHud = 131,
    ToggleShortcut0 = 132,
    ToggleShortcut1 = 133,
    ToggleShortcut2 = 134,
    ToggleShortcut3 = 135,
    ToggleShortcut4 = 136,
    ToggleShortcut5 = 137,
    ToggleShortcut6 = 138,
    ToggleShortcut7 = 139,
    ToggleShortcut8 = 140,
    ToggleShortcut9 = 141,
    ToggleEmoteShortcut0 = 142,
    ToggleEmoteShortcut1 = 143,
    ToggleEmoteShortcut2 = 144,
    ToggleEmoteShortcut3 = 145,
    ToggleEmoteShortcut4 = 146,
    ToggleEmoteShortcut5 = 147,
    ToggleEmoteShortcut6 = 148,
    ToggleEmoteShortcut7 = 149,
    ToggleEmoteShortcut8 = 150,
    ToggleEmoteShortcut9 = 151,
    ChatPreviousInHistory = 152,
    ChatNextInHistory = 153,

    Expression_Wave = 201,
    Expression_FistPump = 202,
    Expression_Robot = 203,
    Expression_RaiseHand = 204,
    Expression_Clap = 205,
    Expression_ThrowMoney = 206,
    Expression_SendKiss = 207,
    Expression_Dance = 208,
    Expression_Hohoho = 209,
    Expression_Snowfall = 210,
}

/// <summary>
/// Mapping for hold actions
/// </summary>
public enum DCLAction_Hold
{
    //Remember to explicitly assign the value to each entry so we minimize issues with serialization + conflicts
    Sprint = 1,
    Jump = 2,
    ZoomIn = 3,
    ZoomOut = 4,
    FreeCameraMode = 101,
    VoiceChatRecording = 102,
    DefaultConfirmAction = 300,
    DefaultCancelAction = 301,
    OpenExpressions = 447
}

/// <summary>
/// Mapping for measurable actions
/// </summary>
public enum DCLAction_Measurable
{
    //Remember to explicitly assign the value to each entry so we minimize issues with serialization + conflicts
    CharacterXAxis = 1,
    CharacterYAxis = 2,
    CameraXAxis = 3,
    CameraYAxis = 4,
    MouseWheel = 5
}

/// <summary>
/// Input Controller will map inputs(keys/mouse/axis) to DCL actions, check if they can be triggered (modifiers) and raise the events
/// </summary>
public class InputController : MonoBehaviour
{
    public static bool ENABLE_THIRD_PERSON_CAMERA = true;

    [Header("General Input")]
    public InputAction_Trigger[] triggerTimeActions;
    public InputAction_Hold[] holdActions;
    public InputAction_Measurable[] measurableActions;

    bool renderingEnabled => CommonScriptableObjects.rendererState.Get();
    bool allUIHidden => CommonScriptableObjects.allUIHidden.Get();

    private DCLPlayerInput playerInput;
    private static DCLPlayerInput.PlayerActions player;

    private Vector3 lastPos;
    private Vector3 currentPos;
    private Vector3 lastRot;
    private Vector3 currentRot;
    
    private void Awake()
    {
        playerInput = new DCLPlayerInput();
        player = playerInput.Player;
    }

    private void Start()
    {
        lastPos = player.MoveHMD.ReadValue<Vector3>();
        lastRot = player.RotateHMD.ReadValue<Vector3>();
    }

    public static void GetPlayerActions(ref DCLPlayerInput.PlayerActions actions) => actions = player;

    private void OnEnable() => playerInput.Enable();

    private void OnDisable() => playerInput.Disable();

    private void Update()
    {
        if (!renderingEnabled)
        {
            Stop_Measurable(measurableActions);
            return;
        }
        Update_Trigger(triggerTimeActions);
        Update_Hold(holdActions);
        Update_Measurable(measurableActions);
    }

    

    /// <summary>
    /// Map the trigger actions to inputs + modifiers and check if their events must be triggered
    /// </summary>
    private void Update_Trigger(InputAction_Trigger[] triggerTimeActions)
    {
        foreach (var action in triggerTimeActions)
        {
            if (action.isTriggerBlocked != null && action.isTriggerBlocked.Get())
                continue;

            switch (action.DCLAction)
            {
                case DCLAction_Trigger.CameraChange:
// <<<<<<< HEAD
                    // if (CommonScriptableObjects.cameraModeInputLocked.Get()) 
                        // break;

                    // //Disable until the fine-tuning is ready
                    // // if (ENABLE_THIRD_PERSON_CAMERA)
                    // //     InputProcessor.FromKey(action, KeyCode.V,
                    // //         modifiers: InputProcessor.Modifier.FocusNotInInput);
// =======
                    // Disable until the fine-tuning is ready
                    if (!CommonScriptableObjects.cameraModeInputLocked.Get() && ENABLE_THIRD_PERSON_CAMERA)
                        InputProcessor.FromKey(action, KeyCode.V, modifiers: InputProcessor.Modifier.FocusNotInInput);
// >>>>>>> upstream/release/20230227
                    break;
                case DCLAction_Trigger.CursorUnlock:
                    InputProcessor.FromMouseButtonUp(action, mouseButtonIdx: 1, modifiers: InputProcessor.Modifier.NeedsPointerLocked);
#if !WEB_PLATFORM
                    InputProcessor.FromKey(action, KeyCode.Escape, modifiers: InputProcessor.Modifier.NeedsPointerLocked);
#endif
                    break;
                case DCLAction_Trigger.ToggleNavMap:
                    InputProcessor.FromKey(action, KeyCode.M, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleFriends:
                    if (!allUIHidden)
                        InputProcessor.FromKey(action, KeyCode.L, modifiers: InputProcessor.Modifier.None);
                    break;
                case DCLAction_Trigger.ToggleWorldChat:
                    if (!allUIHidden)
                        InputProcessor.FromKey(action, KeyCode.Return, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleUIVisibility:
                    InputProcessor.FromKey(action, KeyCode.U, modifiers: InputProcessor.Modifier.None);
                    break;
                case DCLAction_Trigger.CloseWindow:
                    if (!allUIHidden && !DataStore.i.common.isSignUpFlow.Get())
                        InputProcessor.FromKey(action, KeyCode.Escape, modifiers: InputProcessor.Modifier.None);
                    break;
                case DCLAction_Trigger.ToggleControlsHud:
                    InputProcessor.FromKey(action, KeyCode.C, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleSettings:
                    InputProcessor.FromKey(action, KeyCode.P, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleStartMenu:
                    InputProcessor.FromKey(action, KeyCode.Tab, modifiers: InputProcessor.Modifier.None);
                    break;
                case DCLAction_Trigger.TogglePlacesAndEventsHud:
                    InputProcessor.FromKey(action, KeyCode.X, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut0:
                    InputProcessor.FromKey(action, KeyCode.Alpha0, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut1:
                    InputProcessor.FromKey(action, KeyCode.Alpha1, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut2:
                    InputProcessor.FromKey(action, KeyCode.Alpha2, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut3:
                    InputProcessor.FromKey(action, KeyCode.Alpha3, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut4:
                    InputProcessor.FromKey(action, KeyCode.Alpha4, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut5:
                    InputProcessor.FromKey(action, KeyCode.Alpha5, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut6:
                    InputProcessor.FromKey(action, KeyCode.Alpha6, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut7:
                    InputProcessor.FromKey(action, KeyCode.Alpha7, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut8:
                    InputProcessor.FromKey(action, KeyCode.Alpha8, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut9:
                    InputProcessor.FromKey(action, KeyCode.Alpha9, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut0:
                    InputProcessor.FromKey(action, KeyCode.Alpha0, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: new[] { KeyCode.B });
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut1:
                    InputProcessor.FromKey(action, KeyCode.Alpha1, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: new[] { KeyCode.B });
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut2:
                    InputProcessor.FromKey(action, KeyCode.Alpha2, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: new[] { KeyCode.B });
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut3:
                    InputProcessor.FromKey(action, KeyCode.Alpha3, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: new[] { KeyCode.B });
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut4:
                    InputProcessor.FromKey(action, KeyCode.Alpha4, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: new[] { KeyCode.B });
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut5:
                    InputProcessor.FromKey(action, KeyCode.Alpha5, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: new[] { KeyCode.B });
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut6:
                    InputProcessor.FromKey(action, KeyCode.Alpha6, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: new[] { KeyCode.B });
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut7:
                    InputProcessor.FromKey(action, KeyCode.Alpha7, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: new[] { KeyCode.B });
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut8:
                    InputProcessor.FromKey(action, KeyCode.Alpha8, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: new[] { KeyCode.B });
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut9:
                    InputProcessor.FromKey(action, KeyCode.Alpha9, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: new[] { KeyCode.B });
                    break;
                case DCLAction_Trigger.ChatNextInHistory:
                    InputProcessor.FromKey(action, KeyCode.UpArrow, modifiers: InputProcessor.Modifier.OnlyWithInputFocused);
                    break;
                case DCLAction_Trigger.ChatPreviousInHistory:
                    InputProcessor.FromKey(action, KeyCode.DownArrow, modifiers: InputProcessor.Modifier.OnlyWithInputFocused);
                    break;
                case DCLAction_Trigger.Expression_Wave:
                    InputProcessor.FromKey(action, KeyCode.Alpha1, modifiers: InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Trigger.Expression_FistPump:
                    InputProcessor.FromKey(action, KeyCode.Alpha2, modifiers: InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Trigger.Expression_Robot:
                    InputProcessor.FromKey(action, KeyCode.Alpha3, modifiers: InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Trigger.Expression_RaiseHand:
                    InputProcessor.FromKey(action, KeyCode.Alpha4, modifiers: InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Trigger.Expression_Clap:
                    InputProcessor.FromKey(action, KeyCode.Alpha5, modifiers: InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Trigger.Expression_ThrowMoney:
                    InputProcessor.FromKey(action, KeyCode.Alpha6, modifiers: InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Trigger.Expression_SendKiss:
                    InputProcessor.FromKey(action, KeyCode.Alpha7, modifiers: InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Trigger.ToggleVoiceChatRecording:
                    InputProcessor.FromKey(action, KeyCode.T, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: new[] { KeyCode.LeftAlt });
                    break;
                case DCLAction_Trigger.ToggleAvatarEditorHud:
                    InputProcessor.FromKey(action, KeyCode.I, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleQuestsPanelHud:
                    InputProcessor.FromKey(action, KeyCode.J, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleAvatarNamesHud:
                    InputProcessor.FromKey(action, KeyCode.N, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// Map the hold actions to inputs + modifiers and check if their events must be triggered
    /// </summary>
    private void Update_Hold(InputAction_Hold[] holdActions)
    {
        foreach (var action in holdActions)
        {
            if (action.isHoldBlocked != null && action.isHoldBlocked.Get())
            {
                Debug.Log($"{action.name} is hold blocked1");
                continue;

            switch (action.DCLAction)
            {
                case DCLAction_Hold.Sprint:
                    InputProcessor.FromKey(action, player.Walk.ReadValue<float>(),
                        InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Hold.Jump:
                    InputProcessor.FromKey(action, player.Jump.ReadValue<float>(),
                        InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Hold.ZoomIn:
                    InputProcessor.FromKey(action, KeyCode.KeypadPlus);
                    InputProcessor.FromKey(action, KeyCode.Plus);
                    break;
                case DCLAction_Hold.ZoomOut:
                    InputProcessor.FromKey(action, KeyCode.KeypadMinus);
                    InputProcessor.FromKey(action, KeyCode.Minus);
                    break;
                case DCLAction_Hold.FreeCameraMode:
                    // Disable until the fine-tuning is ready
                    if (ENABLE_THIRD_PERSON_CAMERA)
                        InputProcessor.FromKey(action, KeyCode.Y, InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_Hold.VoiceChatRecording:
                    // Push to talk functionality only triggers if no modifier key is pressed
                    InputProcessor.FromKey(action, KeyCode.T, InputProcessor.Modifier.FocusNotInInput, null);
                    break;
                case DCLAction_Hold.DefaultConfirmAction:
                    InputProcessor.FromKey(action, KeyCode.E);
                    break;
                case DCLAction_Hold.DefaultCancelAction:
                    InputProcessor.FromKey(action, KeyCode.F);
                    break;
                case DCLAction_Hold.OpenExpressions:
                    InputProcessor.FromKey(action, KeyCode.B, InputProcessor.Modifier.FocusNotInInput);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// Map the measurable actions to inputs + modifiers and check if their events must be triggered
    /// </summary>
    private void Update_Measurable(InputAction_Measurable[] measurableActions)
    {
        foreach (var action in measurableActions)
        {
            if (action.isMeasurableBlocked != null && action.isMeasurableBlocked.Get())
            {
                Debug.Log($"{action.name} is hold blocked2");
                continue;
// <<<<<<< HEAD  Possibly VR control
            // }
            
            // switch (action.GetDCLAction())
            // {
                // case DCLAction_Measurable.CharacterXAxis:
                    // InputProcessor.FromAxis(action, player.Move.ReadValue<Vector2>().x , 
                        // InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    
                    // break;
                // case DCLAction_Measurable.CharacterYAxis:
                    // InputProcessor.FromAxis(action, player.Move.ReadValue<Vector2>().y ,
                        // InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
// =======

            switch (action.DCLAction)
            {
                case DCLAction_Measurable.CharacterXAxis:
                    InputProcessor.FromAxis(action, "Horizontal", InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Measurable.CharacterYAxis:
                    InputProcessor.FromAxis(action, "Vertical", InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
// >>>>>>> upstream/release/20230227
                    break;
                case DCLAction_Measurable.CameraXAxis:
                    InputProcessor.FromAxis(action, player.Look.ReadValue<Vector2>().x,
                        InputProcessor.Modifier.NeedsPointerLocked | InputProcessor.Modifier.RequiresPointer);
                    break;
                case DCLAction_Measurable.CameraYAxis:
                    InputProcessor.FromAxis(action, player.Look.ReadValue<Vector2>().y,
                        InputProcessor.Modifier.NeedsPointerLocked | InputProcessor.Modifier.RequiresPointer);
                    break;
                case DCLAction_Measurable.MouseWheel:
                    InputProcessor.FromAxis(action, player.ScrollMouse.ReadValue<float>(), modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private static void Stop_Measurable(InputAction_Measurable[] measurableActions)
    {
        foreach (var action in measurableActions)
            action.RaiseOnValueChanged(0);
    }
}
// <<<<<<< HEAD

// /// <summary>
// /// Helper class that wraps the processing of inputs and modifiers to trigger actions events
// /// </summary>
// public static class InputProcessor
// {
    // private static readonly KeyCode[] MODIFIER_KEYS = new[] { KeyCode.LeftControl, KeyCode.LeftAlt, KeyCode.LeftShift, KeyCode.LeftCommand, KeyCode.B };

    // [Flags]
    // public enum Modifier
    // {
        // //Set the values as bit masks
        // None = 0b0000000, // No modifier needed
        // NeedsPointerLocked = 0b0000001, // The pointer must be locked to the game
        // FocusNotInInput = 0b0000010, // The game focus cannot be in an input field
        // NotInStartMenu = 0b0000100, // The game focus cannot be in full-screen start menu
        // OnlyWithInputFocused = 0b0001000, // The game focus must be in an input field
        // RequiresPointer = 0b0010000
    // }

    // /// <summary>
    // /// Check if the modifier keys are pressed
    // /// </summary>
    // /// <param name="modifierKeys"> Keycodes modifiers</param>
    // /// <returns></returns>
    // public static Boolean PassModifierKeys(KeyCode[] modifierKeys)
    // {
        // for (var i = 0; i < MODIFIER_KEYS.Length; i++)
        // {
            // var keyCode = MODIFIER_KEYS[i];
            // var pressed = Input.GetKey(keyCode);
            // if (modifierKeys == null)
            // {
                // if (pressed)
                    // return false;
            // }
            // else
            // {
                // if (modifierKeys.Contains(keyCode) != pressed)
                    // return false;
            // }
        // }

        // return true;
    // }

    // /// <summary>
    // /// Check if a miscellaneous modifiers are present. These modifiers are related to the meta-state of the application
    // /// they can be anything such as mouse pointer state, where the focus is, camera mode...
    // /// </summary>
    // /// <param name="modifiers"></param>
    // /// <returns></returns>
    // public static bool PassModifiers(Modifier modifiers)
    // {
        // bool hasPointer = IsModifierSet(modifiers, Modifier.RequiresPointer) && !CrossPlatformManager.IsVR;
        // if (hasPointer && IsModifierSet(modifiers, Modifier.NeedsPointerLocked) && !DCL.Helpers.Utils.IsCursorLocked)
            // return false;

        // var isInputFieldFocused = FocusIsInInputField();
        
        // if (IsModifierSet(modifiers, Modifier.FocusNotInInput) && isInputFieldFocused)
            // return false;
        
        // if (IsModifierSet(modifiers, Modifier.OnlyWithInputFocused) && !isInputFieldFocused)
            // return false;

        // if (IsModifierSet(modifiers, Modifier.NotInStartMenu) && IsStartMenuVisible())
            // return false;

        // return true;
    // }

    // private static bool IsStartMenuVisible() => DataStore.i.exploreV2.isOpen.Get();

    // /// <summary>
    // /// Process an input action mapped to a keyboard key.
    // /// </summary>
    // /// <param name="action">Trigger Action to perform</param>
    // /// <param name="key">KeyCode mapped to this action</param>
    // /// <param name="modifierKeys">KeyCodes required to perform the action</param>
    // /// <param name="modifiers">Miscellaneous modifiers required for this action</param>
    // public static void FromKey(InputAction_Trigger action, KeyCode key, KeyCode[] modifierKeys = null,
        // Modifier modifiers = Modifier.None)
    // {
        // if (!PassModifiers(modifiers))
            // return;

        // if (!PassModifierKeys(modifierKeys))
            // return;

        // if (Input.GetKeyDown(key))
            // action.RaiseOnTriggered();
    // }

    // /// <summary>
    // /// Process an input action mapped to a button.
    // /// </summary>
    // /// <param name="action">Trigger Action to perform</param>
    // /// <param name="mouseButtonIdx">Index of the mouse button mapped to this action</param>
    // /// <param name="modifiers">Miscellaneous modifiers required for this action</param>
    // public static void FromMouseButton(InputAction_Trigger action, int mouseButtonIdx,
        // Modifier modifiers = Modifier.None)
    // {
        // if (!PassModifiers(modifiers))
            // return;

        // if (Input.GetMouseButton(mouseButtonIdx))
            // action.RaiseOnTriggered();
    // }
    
    // public static void FromMouseButtonUp(InputAction_Trigger action, int mouseButtonIdx,
        // Modifier modifiers = Modifier.None)
    // {
        // if (!PassModifiers(modifiers))
            // return;

        // if (Input.GetMouseButtonUp(mouseButtonIdx))
            // action.RaiseOnTriggered();
    // }

    // /// <summary>
    // /// Process an input action mapped to a keyboard key
    // /// </summary>
    // /// <param name="action">Hold Action to perform</param>
    // /// <param name="key">KeyCode mapped to this action</param>
    // /// <param name="modifiers">Miscellaneous modifiers required for this action</param>
    // public static void FromKey(InputAction_Hold action, KeyCode key, Modifier modifiers = Modifier.None)
    // {
        // if (!PassModifiers(modifiers))
            // return;

        // if (Input.GetKeyDown(key))
            // action.RaiseOnStarted();
        // if (Input.GetKeyUp(key))
            // action.RaiseOnFinished();
    // }

    // public static void FromKey(InputAction_Hold action, float pressed, Modifier modifiers = Modifier.None)
    // {
        // if (!action.isOn && pressed > .5f)
            // action.RaiseOnStarted();
        // if (action.isOn && pressed < .5f)
            // action.RaiseOnFinished();
        
    // }

    // /// <summary>
    // /// Process an input action mapped to a keyboard key
    // /// </summary>
    // /// <param name="action">Hold Action to perform</param>
    // /// <param name="key">KeyCode mapped to this action</param>
    // /// <param name="modifiers">Miscellaneous modifiers required for this action</param>
    // /// <param name="modifierKeys">KeyCodes required to perform the action</param>
    // public static void FromKey(InputAction_Hold action, KeyCode key, Modifier modifiers, KeyCode[] modifierKeys)
    // {
        // if (!PassModifierKeys(modifierKeys))
            // return;

        // FromKey(action, key, modifiers);
    // }

    // /// <summary>
    // /// Process an input action mapped to a mouse button
    // /// </summary>
    // /// <param name="action">Hold Action to perform</param>
    // /// <param name="mouseButtonIdx">Index of the mouse button</param>
    // /// <param name="modifiers">Miscellaneous modifiers required for this action</param>
    // public static void FromMouse(InputAction_Hold action, int mouseButtonIdx, Modifier modifiers = Modifier.None)
    // {
        // if (!PassModifiers(modifiers))
            // return;

        // if (Input.GetMouseButtonDown(mouseButtonIdx))
            // action.RaiseOnStarted();
        // if (Input.GetMouseButtonUp(mouseButtonIdx))
            // action.RaiseOnFinished();
    // }

    // /// <summary>
    // /// Process an input action mapped to an axis
    // /// </summary>
    // /// <param name="action">Measurable Action to perform</param>
    // /// <param name="axisName">Axis name</param>
    // /// <param name="modifiers">Miscellaneous modifiers required for this action</param>
    // public static void FromAxis(InputAction_Measurable action, string axisName, Modifier modifiers = Modifier.None)
    // {
        // if (!PassModifiers(modifiers))
        // {
            // action.RaiseOnValueChanged(0);
            // return;
        // }

        // action.RaiseOnValueChanged(Input.GetAxis(axisName));
    // }

    // public static void FromAxis(InputAction_Measurable action, float value, Modifier modifiers = Modifier.None)
    // {
        // if (!PassModifiers(modifiers))
        // {
            // action.RaiseOnValueChanged(0);
            // return;
        // }

        // action.RaiseOnValueChanged(value);
    // }

    // /// <summary>
    // /// Bitwise check for the modifiers flags
    // /// </summary>
    // /// <param name="modifiers">Modifier to check</param>
    // /// <param name="value">Modifier mapped to a bit to check</param>
    // /// <returns></returns>
    // public static bool IsModifierSet(Modifier modifiers, Modifier value)
    // {
        // int flagsValue = (int)modifiers;
        // int flagValue = (int)value;

        // return (flagsValue & flagValue) != 0;
    // }

    // public static bool FocusIsInInputField()
    // {
        // if (EventSystem.current == null)
            // return false;

        // if (EventSystem.current.currentSelectedGameObject != null &&
            // (EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null ||
             // EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>() != null))
        // {
            // return true;
        // }

        // return false;
    // }
// }
// =======
// >>>>>>> upstream/release/20230227
