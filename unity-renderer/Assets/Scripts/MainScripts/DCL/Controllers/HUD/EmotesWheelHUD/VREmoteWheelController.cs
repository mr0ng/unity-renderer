using DCL.EmotesWheel;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace DCL.EmotesWheel
{
    public class VREmoteWheelController : MonoBehaviour
    {

        [FormerlySerializedAs("emoteWheelView")] public EmotesWheelView emotesWheelView;
        [SerializeField] private EmoteWheelSlot[] slots;
        [SerializeField] private InputActionAsset inputActions; // Reference to your Input Action Asset
        private InputAction emoteAction;
        private InputAction moveAction;
        private int lastSelectedSlot = -1;
        private Canvas canvas;
        [SerializeField] CharacterPreviewController characterPreview;

        int selectedSlot = 0;
        private Transform previewTransformParent;

        void Awake()
        {

            canvas = GetComponent<Canvas>();
            canvas.gameObject.SetActive(false);
            emoteAction = inputActions.FindAction("Emotes");
            moveAction = inputActions.FindAction("Move");

            emoteAction.performed += OnEmote;
            moveAction.performed += OnMove;

            emoteAction.Enable();
            moveAction.Enable();
        }

        void OnEmote(InputAction.CallbackContext ctx)
        {
            if (characterPreview == null)
            {
                characterPreview = FindObjectOfType<CharacterPreviewController>().GetComponent<CharacterPreviewController>();
                previewTransformParent = characterPreview.transform.parent;
            }
            if (emotesWheelView.gameObject.activeSelf)
            {
                canvas.gameObject.SetActive(false);
                characterPreview.transform.SetParent(previewTransformParent);
                Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("CharacterPreview"));
            }
            else
            {
                characterPreview.transform.SetParent(transform);
                characterPreview.transform.localPosition = new Vector3(-343, 135, -52);

                characterPreview.transform.eulerAngles = new Vector3(0, 180, 0);

                characterPreview.transform.localScale = new Vector3(250, 250, 250);

                Camera.main.cullingMask |= 1 << LayerMask.NameToLayer("CharacterPreview");
                var rawForward = CommonScriptableObjects.cameraForward.Get();
                transform.position = CommonScriptableObjects.cameraPosition.Get() + (.7f * rawForward) + new UnityEngine.Vector3(0, -0.38f, 0);

                // Compute the rotation that points the keyboard forward
                Quaternion forwardRotation = Quaternion.LookRotation(new UnityEngine.Vector3(rawForward.x, 0, rawForward.z));

                // Center joystick to deselect any slots
                canvas.gameObject.SetActive(true);
                selectedSlot = 0;
                UpdateSlotSelection();
            }
        }

        void OnMove(InputAction.CallbackContext ctx)
        {
            if (!emotesWheelView.gameObject.activeSelf) return;

            Vector2 direction = ctx.ReadValue<Vector2>();

            if (direction.magnitude > 0.5f)
            {
                // Calculate selected slot based on joystick direction
                selectedSlot = CalculateSlotFromDirection(direction);
            }
            else
            {
                // Joystick released, select current slot
                SelectSlot(selectedSlot);
                selectedSlot = 0;
            }

            UpdateSlotSelection();
        }

        public void UpdateSlotSelection() {

            for (int i = 0; i < slots.Length; i++) {

                if (i == selectedSlot) {
                    slots[i].animator.SetTrigger("Highlighted");
                } else {
                    slots[i].animator.SetTrigger("Normal");
                }

            }

        }

        int CalculateSlotFromDirection(Vector2 direction)
        {
            // Convert direction to angle
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Map angle range (-180 to 180) to slot index (0 to 9)
            int slotIndex = Mathf.RoundToInt(angle / 360f * slots.Length);

            return slotIndex;
        }

        void SelectSlot(int slotIndex)
        {
            // Slot selected, take action
            string emoteId = slots[slotIndex].emoteId;

            // Play emote...
        }

    }
}
