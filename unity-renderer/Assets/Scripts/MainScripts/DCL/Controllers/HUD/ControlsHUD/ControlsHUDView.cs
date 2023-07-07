using UnityEngine;
using System;

public class ControlsHUDView : MonoBehaviour
{
    [SerializeField] internal InputAction_Trigger closeAction;
    [SerializeField] internal ShowHideAnimator showHideAnimator;
    [SerializeField] internal Button_OnPointerDown closeButton;
    [SerializeField] internal GameObject voiceChatButton;

    public event Action<bool> onCloseActionTriggered;

    private void Awake()
    {
        #if DCL_VR
        OnCloseActionTriggered(DCLAction_Trigger.CloseWindow);
#else
        closeAction.OnTriggered += OnCloseActionTriggered;
        closeButton.onPointerDown += () => Close(true);
        #endif
    }

    private void OnDestroy() { closeAction.OnTriggered -= OnCloseActionTriggered; }

    private void OnCloseActionTriggered(DCLAction_Trigger action) { Close(false); }

    private void Close(bool closedByButtonPress) { onCloseActionTriggered?.Invoke(closedByButtonPress); }
}
