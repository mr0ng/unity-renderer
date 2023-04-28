// using DCL;
// using Microsoft.MixedReality.Toolkit.Experimental.UI;
// using Microsoft.MixedReality.Toolkit.Input;
// using Microsoft.MixedReality.Toolkit.Input.Utilities;
// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
//
// public class VRUIManager : MonoBehaviour
// {
//     public Canvas vrCanvas; // Assign your MRTK Canvas in the Inspector
//     [SerializeField] private GameObject vrScreenCanvas;
//     [SerializeField] private NonNativeKeyboard mrtkKeyboard; // Add a reference to the MRTK Non-Native Keyboard
//     private Dictionary<int, RectTransform> uiElementCache = new Dictionary<int, RectTransform>();
//     public static VRUIManager Instance { get; private set; }
//
//     private void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }
//
//     private void Start()
//     {
//         if (vrCanvas == null)
//         {
//             Debug.LogError("VRUIManager: vrCanvas is not assigned.");
//         }
//         BaseComponentView.OnViewCreated += HandleViewCreated;
//     }
//
//     private void OnDestroy()
//     {
//         BaseComponentView.OnViewCreated -= HandleViewCreated;
//     }
//
//
//     public void OnUIElementCreated(GameObject uiElement)
//     {
//         if (uiElement != null && vrCanvas != null)
//         {
//             RectTransform rectTransform = uiElement.GetComponent<RectTransform>();
//
//             if (rectTransform != null)
//             {
//                 int instanceID = uiElement.GetInstanceID();
//
//                 // Remove the existing world space UI element if it exists
//                 if (uiElementCache.ContainsKey(instanceID))
//                 {
//                     Destroy(uiElementCache[instanceID].gameObject);
//                     uiElementCache.Remove(instanceID);
//                 }
//
//                 // Create a new world space UI element
//                 GameObject worldSpaceUIElement = Instantiate(uiElement, vrCanvas.transform);
//                 RectTransform worldSpaceRectTransform = worldSpaceUIElement.GetComponent<RectTransform>();
//                 worldSpaceRectTransform.localScale = rectTransform.localScale; // Adjust scale based on your Canvas settings
//                 worldSpaceRectTransform.localPosition = rectTransform.localPosition; // Adjust position based on your Canvas settings
//                 worldSpaceRectTransform.localRotation = rectTransform.localRotation;
//
//                 // Replace Unity UI components with MRTK components, if necessary
//                 // ...
//
//                 // Cache the new world space UI element
//                 uiElementCache.Add(instanceID, worldSpaceRectTransform);
//
//                 // Convert HUD element to VR
//                 Canvas hudCanvas = worldSpaceUIElement.GetComponentInChildren<Canvas>();
//                 if (hudCanvas != null)
//                 {
//                     ConvertHUDElementToVR(hudCanvas);
//                 }
//             }
//         }
//     }
//
//     // public void OnUIElementUpdated(GameObject uiElement)
//     // {
//     //     if (uiElement != null)
//     //     {
//     //         int instanceID = uiElement.GetInstanceID();
//     //
//     //         if (uiElementCache.ContainsKey(instanceID))
//     //         {
//     //             RectTransform rectTransform = uiElement.GetComponent<RectTransform>();
//     //             RectTransform worldSpaceRectTransform = uiElementCache[instanceID];
//     //
//     //             if (rectTransform != null && worldSpaceRectTransform != null)
//     //             {
//     //                 // Update world space UI element's RectTransform values
//     //                 worldSpaceRectTransform.localScale = rectTransform.localScale; // Adjust scale based on your Canvas settings
//     //                 worldSpaceRectTransform.localPosition = rectTransform.localPosition; // Adjust position based on your Canvas settings
//     //                 worldSpaceRectTransform.localRotation = rectTransform.localRotation;
//     //             }
//     //         }
//     //     }
//     // }
//     //
//     // public void OnUIElementRemoved(GameObject uiElement)
//     // {
//     //     if (uiElement != null)
//     //     {
//     //         int instanceID = uiElement.GetInstanceID();
//     //
//     //         if (uiElementCache.ContainsKey(instanceID))
//     //         {
//     //             // Remove the world space UI element and return it to the object pool, if applicable
//     //             Destroy(uiElementCache[instanceID].gameObject);
//     //             uiElementCache.Remove(instanceID);
//     //         }
//     //     }
//     // }
//     //
//     // // Utility method to replace Unity UI components with MRTK components
//     // private void ReplaceUnityUIComponentsWithMRTK(GameObject uiElement)
//     // {
//     //     // Implement the logic to replace Unity UI components with their MRTK counterparts
//     //     // ...
//     // }
//
//
//
//     private void HandleViewCreated(BaseComponentView viewInstance)
//     {
//         string name = viewInstance.name;
//         // if(name == "GotoPanelHudView")
//
//         Canvas hudCanvas = viewInstance.GetComponentInChildren<Canvas>();
//         if (hudCanvas != null)
//         {
//             ConvertHUDElementToVR(hudCanvas);
//         }
//         else
//             Debug.Log($"HandleViewCreated Failed: {name}");
//     }
//     // public void HandleHUDCreated(IHUD hudInstance)
//     // {
//     //     // Check if the HUD instance has a BaseComponentView
//     //     BaseComponentView baseComponentView = (hudInstance as BaseComponentView);
//     //
//     //     if (baseComponentView != null)
//     //     {
//     //         // Get the Canvas component from the BaseComponentView
//     //         Canvas hudCanvas = baseComponentView.GetComponentInChildren<Canvas>();
//     //
//     //         if (hudCanvas != null)
//     //         {
//     //             // Modify the HUD for VR
//     //             ConvertHUDElementToVR(hudCanvas);
//     //         }
//     //     }
//     //     else
//     //         Debug.Log($"HandleHudCreated Failed: {hudInstance.ToString()}");
//     // }
//
//     private void ConvertHUDElementToVR(Canvas hudCanvas)
// {
//     // Change the canvas render mode to World Space
//     hudCanvas.renderMode = RenderMode.WorldSpace;
//
//     // Update the sorting layer and order
//     hudCanvas.overrideSorting = true;
//     hudCanvas.sortingOrder = 100; // Change this value as needed
//     hudCanvas.sortingLayerName = "menu";
//
//     // Add GraphicRaycaster if it doesn't exist
//     if (hudCanvas.GetComponent<GraphicRaycaster>() == null)
//     {
//         hudCanvas.gameObject.AddComponent<GraphicRaycaster>();
//     }
//
//     // Add CanvasUtility if it doesn't exist
//     if (hudCanvas.GetComponent<CanvasUtility>() == null)
//     {
//         hudCanvas.gameObject.AddComponent<CanvasUtility>();
//     }
//
//     // Add NearInteractionTouchableUnityUI if it doesn't exist
//     if (hudCanvas.GetComponent<NearInteractionTouchableUnityUI>() == null)
//     {
//         var nearInteraction = hudCanvas.gameObject.AddComponent<NearInteractionTouchableUnityUI>();
//         nearInteraction.EventsToReceive = TouchableEventType.Pointer;
//     }
//
//     // Set the parent of the HUD element to the vrCanvas
//     hudCanvas.transform.SetParent(vrCanvas.transform);
//
//     // Set the desired position and orientation of the HUD elements in front of the player
//     Vector3 desiredPosition = Camera.main.transform.position + Camera.main.transform.forward * 2f; // 2 units in front of the player
//     Quaternion desiredRotation = Quaternion.LookRotation((desiredPosition - Camera.main.transform.position).normalized);
//
//     // Set the local position, rotation, and scale of the HUD element
//     hudCanvas.transform.position = desiredPosition;
//     hudCanvas.transform.rotation = desiredRotation;
//     hudCanvas.transform.localScale = 0.1f * Vector3.one; // Adjust this value to scale the elements down evenly
//
//     // Find all InputField elements in the HUD and add event triggers
//     InputField[] inputFields = hudCanvas.GetComponentsInChildren<InputField>(true);
//     foreach (InputField inputField in inputFields)
//     {
//         AddEventTriggerToInputField(inputField);
//     }
// }
//
//
//     private void AddEventTriggerToInputField(InputField inputField)
//     {
//         EventTrigger eventTrigger = inputField.gameObject.AddComponent<EventTrigger>();
//         EventTrigger.Entry entry = new EventTrigger.Entry
//         {
//             eventID = EventTriggerType.PointerClick
//         };
//         entry.callback.AddListener((eventData) => { OnInputFieldClicked(inputField); });
//         eventTrigger.triggers.Add(entry);
//     }
//
//     private void OnInputFieldClicked(InputField inputField)
//     {
//         mrtkKeyboard.gameObject.SetActive(true);
//         mrtkKeyboard.PresentKeyboard(inputField.text, NonNativeKeyboard.LayoutType.Alpha);
//         mrtkKeyboard.OnClosed += OnKeyboardClosed;
//         mrtkKeyboard.OnTextSubmitted += OnKeyboardTextSubmitted;
//     }
//
//     private void OnKeyboardClosed(object sender, System.EventArgs e)
//     {
//         mrtkKeyboard.OnClosed -= OnKeyboardClosed;
//         mrtkKeyboard.OnTextSubmitted -= OnKeyboardTextSubmitted;
//     }
//
//     private void OnKeyboardTextSubmitted(object sender, EventArgs eventArgs)
//     {
//         InputField inputField = sender as InputField;
//         if (inputField != null)
//         {
//             inputField.text = eventArgs.ToString();
//             inputField.onEndEdit.Invoke(eventArgs.ToString());
//         }
//         mrtkKeyboard.gameObject.SetActive(false);
//     }
// }
