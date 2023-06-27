
using DCL;
using DCLServices.MapRendererV2.ConsumerUtils;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PointerHelper : MonoBehaviour, IMixedRealityPointerHandler
{

    #if UNITY_ANDROID && !Unity_EDITOR
    private Vector3 offset = new Vector3( 1273, 655, 0);
    private float scaleFactor = 1.445f;
    // private float heightFactor = 1.445f;
    #else
    private Vector3 offset = new Vector3( 1504, 651, 0);
     private float widthFactor = 0.22f;
    private float heightFactor = 0.22f;
#endif
    private RectTransform referenceTrans;

    public Vector3 localPointerPos;
    private Vector3 origin;
    public bool isCursorOverMap;
    private bool isDragging = false;
    private readonly BaseVariable<bool> navmapIsRendered = DataStore.i.HUDs.navmapIsRendered;

    [SerializeField] private MapRenderImage mapRenderImage;

    private float parcelSizeInMap;
    public Vector2Int cursorMapCoords;
    private int NAVMAP_CHUNK_LAYER;
    private Vector3 worldCoordsOriginInMap;
    private bool isTriggerDown = false;
    private bool triggerLastState = false;
    private Vector2 initialPressPosition;
    private float initialPressTime;
    [SerializeField] private double dragDistance = 10;
    [SerializeField] private double dragTime = 0.5f;
    private WaitForSeconds waitTime;

    [SerializeField] private Text xscale;
    [SerializeField] private Text yscale;
    [SerializeField] private Text xoffset;
    [SerializeField] private Text yoffset;
    public static PointerHelper Instance { get; set; }
    // void Update()
    // {
    //     if (!navmapIsRendered.Get())
    //         return;
    //
    //     if (IsCursorOverMapChunk())
    //     {
    //         if (isTriggerDown && !triggerLastState && !isDragging)  // When the trigger is first pressed
    //             mapRenderImage.OnBeginDrag(new PointerEventData(EventSystem.current) { position = cursorMapCoords, dragging = isDragging });
    //
    //         else if(isTriggerDown && isDragging) // While dragging
    //             mapRenderImage.OnDrag(new PointerEventData(EventSystem.current) { position = cursorMapCoords, dragging = isDragging });
    //
    //         else if(!isTriggerDown && triggerLastState && isDragging) // When the trigger is released
    //         {
    //             mapRenderImage.OnEndDrag(new PointerEventData(EventSystem.current) { position = cursorMapCoords, dragging = isDragging });
    //             isDragging = false;  // End dragging after firing the event
    //         }
    //         else //regular hover
    //             mapRenderImage.OnPointerMove(new PointerEventData(EventSystem.current) { position = cursorMapCoords, dragging = isDragging });
    //
    //     }
    //     triggerLastState = isTriggerDown;
    // }

    private void Awake()
    {
        LoadAdjustments();
        waitTime = new WaitForSeconds(0.1f);
        Instance = this;
        referenceTrans = GetComponent<RectTransform>();
        mapRenderImage = GetComponent<MapRenderImage>();
        // isVR = CrossPlatformManager.IsVRPlatform();
    }

    private void Start()
    {
        StartCoroutine(UpdateCoRoutine());
    }

    private IEnumerator UpdateCoRoutine()
    {
        while (true)
        {
            if (!navmapIsRendered.Get())
            {
                yield return waitTime;
                continue;
            }


            yield return waitTime;

            if (IsCursorOverMapChunk())
            {
                if (isTriggerDown && !triggerLastState && !isDragging) // When the trigger is first pressed
                    mapRenderImage.OnBeginDrag(new PointerEventData(EventSystem.current) { position = cursorMapCoords, dragging = isDragging });

                else if (isTriggerDown && isDragging) // While dragging
                    mapRenderImage.OnDrag(new PointerEventData(EventSystem.current) { position = cursorMapCoords, dragging = isDragging });

                else if (!isTriggerDown && triggerLastState && isDragging) // When the trigger is released
                {
                    mapRenderImage.OnEndDrag(new PointerEventData(EventSystem.current) { position = cursorMapCoords, dragging = isDragging });
                    isDragging = false; // End dragging after firing the event
                }
                else //regular hover
                    mapRenderImage.OnPointerMove(new PointerEventData(EventSystem.current) { position = cursorMapCoords, dragging = isDragging });
                triggerLastState = isTriggerDown;
            }


        }
    }
    public bool IsCursorOverMapChunk()
    {
        if (CoreServices.FocusProvider.PrimaryPointer == null || CoreServices.FocusProvider.PrimaryPointer.Result.Details.Object == null)
        {
            // cursorMapCoords.x = -50000;
            // cursorMapCoords.y = -50000;
            isCursorOverMap = false;
            return false;
        }
        // EventSystem.current.RaycastAll(uiRaycastPointerEventData, uiRaycastResults);

        localPointerPos = default;
        var hit = CoreServices.FocusProvider?.PrimaryPointer.Result.Details.Point;
        // convert the hit point from world space to local space of the canvas
        Vector3 point = referenceTrans.InverseTransformPoint(hit ?? Vector3.zero);

        localPointerPos = point - origin + offset;


        if (CoreServices.FocusProvider?.PrimaryPointer.Result.Details.Object.name == gameObject.name)
        {
            // Debug.Log($"Clint: raycast hit {hit.Value}, point {point}, local {localPointerPos},  {CoreServices.FocusProvider?.PrimaryPointer.Result.Details.Object.name}");
            cursorMapCoords.x = (int) (localPointerPos.x * scaleFactor);
            cursorMapCoords.y = (int) (localPointerPos.y * scaleFactor);
            isCursorOverMap = true;
            return true;
        }
        // cursorMapCoords.x = -50000;
        // cursorMapCoords.y = -50000;
        isCursorOverMap = false;
        return false;
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        initialPressPosition = cursorMapCoords;
        initialPressTime = Time.time;
        isTriggerDown = true;
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        if (!navmapIsRendered.Get())
            return;

        float distance = Vector2.Distance(initialPressPosition, cursorMapCoords);
        float time = Time.time - initialPressTime;

        if (!isDragging && (distance > dragDistance || time > dragTime))
        {
            isDragging = true; // set the flag when drag starts
        }
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        isTriggerDown = false;
        if(isDragging)
        {
            // Perform actions necessary when drag ends (if any)

            mapRenderImage.OnEndDrag(new PointerEventData(EventSystem.current) { position = cursorMapCoords, dragging = isDragging });
            isDragging = false; // reset the flag when drag ends
        }
        else
        {
            // Handle click event here
            PointerEventData data = new PointerEventData(EventSystem.current)
            {
                position = cursorMapCoords,
                dragging = false,
            };
            mapRenderImage.OnPointerClick(data);
        }
    }


    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        if (!navmapIsRendered.Get() || isDragging) // Also check if we're not dragging
            return;

        if (IsCursorOverMapChunk())
        {
            PointerEventData data = new PointerEventData(EventSystem.current)
            {
                position = cursorMapCoords,
                dragging = isDragging,
            };
            mapRenderImage.OnPointerClick(data);
        }
    }

    public void IncreaseScale()
    {
        scaleFactor += 0.005f;
        xscale.text = scaleFactor.ToString("F3");
        SaveAdjustments();
    }
    public void DecreaseXScale()
    {
        scaleFactor -= 0.005f;
        xscale.text = scaleFactor.ToString("F3");
        SaveAdjustments();
    }

    public void IncreaseXOffset()
    {
        offset.x += 5f;
        xoffset.text = offset.x.ToString("F2");
        SaveAdjustments();
    }
    public void DecreaseXOffset()
    {
        offset.x -= 5f;
        xoffset.text = offset.x.ToString("F2");
        SaveAdjustments();
    }
    public void IncreaseYOffset()
    {
        offset.y += 5f;
        yoffset.text = offset.y.ToString("F2");
    }
    public void DecreaseYOffset()
    {
        offset.y -= 5f;
        yoffset.text = offset.y.ToString("F2");
    }
    public void SaveAdjustments()
    {
        PlayerPrefs.SetFloat("mapOffsetX", offset.x);
        PlayerPrefs.SetFloat("mapOffsetY", offset.y);
        PlayerPrefs.SetFloat("mapScale", scaleFactor);
        // similarly save for other offsets or scaling factors.
        PlayerPrefs.Save();
    }

    public void LoadAdjustments()
    {
        if (PlayerPrefs.HasKey("mapOffsetX"))
        {
            Vector3 cursorPosition = offset;
            cursorPosition.x = PlayerPrefs.GetFloat("mapOffsetX");
            cursorPosition.y = PlayerPrefs.GetFloat("mapOffsetY");
           offset = cursorPosition;
        }
        if (PlayerPrefs.HasKey("mapScale"))
            scaleFactor = PlayerPrefs.GetFloat("mapScale");


    }
}
