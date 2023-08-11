using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using System.Reflection;

namespace DCL.EmotesWheel
{
    public class VREmoteWheelController : MonoBehaviour
    {
        private EmotesWheelController emotesWheelCtrl;
        private EmotesWheelView emotesWheelView;
        // [SerializeField] private EmoteWheelSlot[] slots;
        [SerializeField] private InputActionAsset inputActions; // Reference to your Input Action Asset
        private InputAction emoteAction;
        private InputAction emoteSelect;
        private InputAction moveAction;
        private int lastSelectedSlot = -1;
        private Canvas canvas;
        [SerializeField] CharacterPreviewController characterPreview;
        private DataStore_EmotesCustomization emotesCustomizationDataStore => DataStore.i.emotesCustomization;

        int selectedSlot = 0;
        private Transform previewTransformParent;

        void Awake()
        {

            canvas = GetComponent<Canvas>();
            canvas.gameObject.SetActive(true);
            emoteAction = inputActions.FindAction("Emotes");
            emoteSelect = inputActions.FindAction("SelectEmote");
            moveAction = inputActions.FindAction("Move");


            emoteAction.performed += OnEmote;
            moveAction.performed += OnMove;
            emoteSelect.performed += OnEmoteSelect;


            emoteAction.Enable();
            moveAction.Enable();
            emotesCustomizationDataStore.equippedEmotes.OnSet += OnEquippedEmotesSet;
        }
        private void OnEquippedEmotesSet(IEnumerable<EquippedEmoteData> equippedEmotes) { emotesWheelCtrl.UpdateEmoteSlots(); }

        private void InitializeCharacterPreview()
        {
            emotesWheelCtrl = new EmotesWheelController(
                UserProfile.GetOwnUserProfile(),
                Environment.i.serviceLocator.Get<IEmotesCatalogService>(),
                Environment.i.serviceLocator.Get<IWearablesCatalogService>());

            emotesWheelView = emotesWheelCtrl.view;
            emotesWheelView.onEmoteClicked += OnEmoteClicked;


            var helper = (MonoBehaviour)emotesWheelView.gameObject.GetComponent("MenuHudHelper");

            if (helper != null)
            {
                helper.enabled = false;
            }
            emotesWheelView.transform.parent = this.transform;

            emotesWheelView.gameObject.SetActive(true);

            ICharacterPreviewFactory fact = Environment.i.serviceLocator.Get<ICharacterPreviewFactory>();
            characterPreview = (CharacterPreviewController)fact.Create(
                loadingMode: CharacterPreviewMode.WithoutHologram,
                renderTexture: (RenderTexture)new RenderTexture(320,320,16),
                isVisible: true,
                previewCameraFocus: PreviewCameraFocus.DefaultEditing,
                isAvatarShadowActive: true);

            // Setting the characterPreview GameObject's layer to Default.
            SetLayerRecursively(characterPreview.gameObject, LayerMask.NameToLayer("UI"));
            // Assuming you've already added ObjectManipulator to the gameObject
            ObjectManipulator manipulator = characterPreview.gameObject.AddComponent<ObjectManipulator>();

            // Add RotationAxisConstraint to the GameObject and configure it
            RotationAxisConstraint rotationConstraint = characterPreview.gameObject.AddComponent<RotationAxisConstraint>();
            rotationConstraint.ConstraintOnRotation = AxisFlags.XAxis | AxisFlags.ZAxis;

            // Add MoveAxisConstraint to the GameObject and configure it
            MoveAxisConstraint moveAxisConstraint = characterPreview.gameObject.AddComponent<MoveAxisConstraint>();
            moveAxisConstraint.ConstraintOnMovement = AxisFlags.XAxis | AxisFlags.YAxis | AxisFlags.ZAxis;

            // Ensure the ObjectManipulator's ConstraintsManager is aware of these constraints
            if (manipulator.ConstraintsManager == null)
            {
                manipulator.ConstraintsManager = new ConstraintManager();
            }
            manipulator.ConstraintsManager.AddConstraintToManualSelection(rotationConstraint);
            manipulator.ConstraintsManager.AddConstraintToManualSelection(moveAxisConstraint);


            characterPreview.transform.parent = emotesWheelView.transform;
            characterPreview.transform.localScale = 250 * Vector3.one;
            characterPreview.transform.localPosition = new Vector3(-366, -181, 0);
            characterPreview.transform.localEulerAngles = new Vector3(0, 143, 0);

            // Disable child GameObject named "Ground"
            Transform groundTransform = characterPreview.transform.Find("Ground");
            if (groundTransform != null) // Safety check
            {
                groundTransform.gameObject.SetActive(false);
            }

            // Add a Box Collider to characterPreview and set its properties
            BoxCollider boxCollider = characterPreview.gameObject.AddComponent<BoxCollider>();
            boxCollider.center = new Vector3(-0.02876255f, 0.6379074f, 0);
            boxCollider.size = new Vector3(0.6459f, 1.82f, 1f);

            emotesWheelView.transform.localScale =  Vector3.one;
            emotesWheelView.transform.localPosition = new Vector3(65, -130, 0);
            emotesWheelView.transform.localEulerAngles = new Vector3(0, 17.8f, 0);

            AvatarModel avatarModel =  UserProfile.GetOwnUserProfile().avatar;
            foreach (string emoteId in emotesCustomizationDataStore.currentLoadedEmotes.Get())
            {
                avatarModel.emotes.Add(new AvatarModel.AvatarEmoteEntry() { urn = emoteId });
            }
            characterPreview.TryUpdateModelAsync(avatarModel);

        }

        void OnEmote(InputAction.CallbackContext ctx)
        {
            bool shouldShow =false;

            if (characterPreview == null)
            {
                InitializeCharacterPreview();
                shouldShow = true;
            }
            else
            {
                shouldShow = !emotesWheelView.gameObject.activeSelf;
            }

            // Toggle visibility of the EmotesWheelView

            emotesWheelCtrl.SetVisibility(shouldShow);

            if (shouldShow)
            {
                DataStore.i.exploreV2.isOpen.Set(true);
                characterPreview.gameObject.SetActive(true);
                emotesWheelView.gameObject.SetActive(true);
                characterPreview.transform.localScale = 250 * Vector3.one;
                characterPreview.transform.localPosition = new Vector3(-366, -181, 0);
                characterPreview.transform.localEulerAngles = new Vector3(0, 143, 0);

                // Calculate position in front of the user
                Vector3 cameraPosition = CommonScriptableObjects.cameraPosition.Get();
                Vector3 rawForward = CommonScriptableObjects.cameraForward.Get();
                Vector3 forwardPosition = cameraPosition + (.7f * new Vector3(rawForward.x, 0, rawForward.z)); // Zeroing out the Y value from rawForward

                // Set Y position to be same height as camera and adjust the X and Z coordinates based on forwardPosition
                transform.position = new Vector3(forwardPosition.x, cameraPosition.y, forwardPosition.z);

                // Compute the direction from the character preview to the user (or camera).
                Vector3 directionToUser = (cameraPosition - characterPreview.transform.position).normalized;
                // Get the Y rotation only, keeping the character preview upright
                float yRotation = Quaternion.LookRotation(directionToUser).eulerAngles.y;
                transform.rotation = Quaternion.Euler(0, yRotation + 180, 0);

                // Reset joystick to deselect any slots
                selectedSlot = -2;
                UpdateSlotSelection();
            }
            else
            {
                DataStore.i.exploreV2.isOpen.Set(false);
                characterPreview.gameObject.SetActive(false);
                emotesWheelView.gameObject.SetActive(false);
            }
        }





        void OnMove(InputAction.CallbackContext ctx)
        {
            if (emotesWheelView == null || !emotesWheelView.gameObject.activeSelf) return;

            Vector2 direction = ctx.ReadValue<Vector2>();

            if (direction.magnitude > 0.7f) // Use this to show as highlighted, but not selected
            {
                // Calculate selected slot based on joystick direction
                selectedSlot = CalculateSlotFromDirection(direction);
                UpdateSlotSelection(); // Update the visual state of the emote wheel
            }

            else
            {
                // selectedSlot = -2;
            }
        }
        void OnEmoteSelect(InputAction.CallbackContext ctx)
        {
            // Check if the wheel is active before selecting an emote
            if (emotesWheelView != null && emotesWheelView.gameObject.activeSelf && selectedSlot > 0)
            {
                SelectSlot();
            }
        }
        public void UpdateSlotSelection() {

            for (int i = 0; i < emotesWheelView.emoteButtons.Length; i++) {
                if (emotesWheelView.emoteButtons[i].animator == null)
                {
                    emotesWheelView.emoteButtons[i].animator = emotesWheelView.emoteButtons[i].gameObject.GetComponent<Animator>();
                    continue;
                }
                if (i == selectedSlot) {
                    emotesWheelView.emoteButtons[i].animator.SetTrigger("Highlighted");
                } else {
                    emotesWheelView.emoteButtons[i].animator.SetTrigger("Normal");
                }

            }

        }

        int CalculateSlotFromDirection(Vector2 direction)
        {
            // Convert direction to angle, where 0/360 is up, 90 is right, 180 is down, 270 is left
            float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 360) % 360;

            if (angle > 337.5f || angle <= 22.5f)
                return 0; // up -> 1
            if (angle > 22.5f && angle <= 67.5f)
                return 1; // top-right -> 2
            if (angle > 67.5f && angle <= 112.5f)
                return 2; // right -> 3
            if (angle > 112.5f && angle <= 157.5f)
                return 3; // bottom-right -> 4
            if (angle > 157.5f && angle <= 202.5f)
                return 4; // down -> 5
            if (angle > 202.5f && angle <= 247.5f)
                return 5; // bottom-left -> 6
            if (angle > 247.5f && angle <= 292.5f)
                return 6; // left -> 7
            if (angle > 292.5f && angle <= 337.5f)
                return 7; // top-left -> 8

            return 0;  // Default to up -> 1
        }


        void SelectSlot()
        {
            if (selectedSlot < 0) return;
            // Slot selected, take action
            string emoteId = emotesWheelView.emoteButtons[selectedSlot].emoteId;

            emotesWheelView.emoteButtons[selectedSlot].button.onClick.Invoke();
            // Play emote...
            characterPreview.PlayEmote(emoteId, (long)Time.realtimeSinceStartup);
        }
        private void OnEmoteClicked(string emoteId)
        {
            characterPreview.PlayEmote(emoteId, (long)Time.realtimeSinceStartup);
        }
        public void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;

            var children = obj.GetComponentsInChildren<Transform>();
            foreach(var child in children)
            {
                child.gameObject.layer = layer;
            }
        }
        private void OnDestroy()
        {
            emoteAction.performed -= OnEmote;
            moveAction.performed -= OnMove;
            emoteSelect.performed -= OnEmoteSelect;
            emotesWheelView.onEmoteClicked -= OnEmoteClicked;
            emotesWheelCtrl.Dispose();

        }

    }
}
