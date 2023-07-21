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
    [SerializeField] private Vector3 offset = new Vector3(3332, 1356, 0);
    [SerializeField] private float scaleFactor = 0.15f;
    private GameObject dockParent;
    private MixedRealityPointerEventData lastEventData = null;

    private RectTransform referenceTrans;
    public Vector3 localPointerPos;
    private Vector3 origin;
    public Dictionary<uint, bool> isCursorOverMap = new Dictionary<uint, bool>();
    private Dictionary<uint, bool> isDragging = new Dictionary<uint, bool>();
    private Dictionary<uint, bool> isTriggerDown = new Dictionary<uint, bool>();
    private Dictionary<uint,bool> triggerLastState = new Dictionary<uint, bool>();
    private Dictionary<uint,Vector2> initialPressPosition = new Dictionary<uint, Vector2>();
    private Dictionary<uint,float> initialPressTime = new Dictionary<uint, float>();
    private readonly BaseVariable<bool> navmapIsRendered = DataStore.i.HUDs.navmapIsRendered;

    [SerializeField] private MapRenderImage mapRenderImage;

    private float parcelSizeInMap;
    public Vector2Int cursorMapCoords;
    private int NAVMAP_CHUNK_LAYER;
    private Vector3 worldCoordsOriginInMap;

    [SerializeField] private double dragDistance = 10;
    [SerializeField] private double dragTime = 0.5f;
    private WaitForSeconds waitTime;

    [SerializeField] private Text xscale;
    [SerializeField] private Text xoffset;
    [SerializeField] private Text yoffset;
    [SerializeField] private Vector3 mapScale;
    [SerializeField] private Vector2 resolution;
    public static PointerHelper Instance { get; set; }
    private void Awake()
    {
        waitTime = new WaitForSeconds(0.01f);
        Instance = this;
        referenceTrans = GetComponent<RectTransform>();
        mapRenderImage = GetComponent<MapRenderImage>();
    }

    private void Start()
    {
        dockParent = GameObject.Find("Dock");

        #if UNITY_ANDROID && !UNITY_EDITOR
        offset = new Vector3(3004, 1712, 0);
        scaleFactor = .72f;
        #else
        offset = new Vector3(3682, 1716, 0);
        scaleFactor = 0.155f;
        #endif
        StartCoroutine(UpdateCoRoutine());
    }

    private IEnumerator UpdateCoRoutine()
    {
        while (true)
        {
            yield return waitTime;
            if (!navmapIsRendered.Get())
            {
                yield return null;
                continue;
            }

            UpdateScale();
            foreach (var pointerId in isTriggerDown.Keys)
            {
                if (IsCursorOverMapChunk(pointerId))
                {
                    if (isTriggerDown[pointerId] && !triggerLastState[pointerId] && !isDragging[pointerId])
                    {
                        var data = new PointerEventData(EventSystem.current)
                        {
                            position = cursorMapCoords,
                            dragging = isDragging[pointerId]
                        };

                        // mapRenderImage.OnPointerClick(data);
                        mapRenderImage.OnBeginDrag(data);
                        lastEventData.Pointer.IsFocusLocked = false;
                        Debug.Log($"update:OnBeginDrag: {cursorMapCoords}, {isDragging[pointerId]}");
                    }
                    else if (!isTriggerDown[pointerId] && triggerLastState[pointerId] && isDragging[pointerId])
                    {
                        mapRenderImage.OnEndDrag(new PointerEventData(EventSystem.current) { position = cursorMapCoords, dragging = isDragging[pointerId] });
                        isDragging[pointerId] = false;
                        Debug.Log($"update:OnEndDrag: {cursorMapCoords}, {isDragging[pointerId]}");
                    }
                    else
                    {
                        mapRenderImage.OnPointerMove(new PointerEventData(EventSystem.current) { position = cursorMapCoords, dragging = isDragging[pointerId] });
                        // Debug.Log($"update:OnPointerMove: {cursorMapCoords}, {isDragging[pointerId]}");
                    }

                    triggerLastState[pointerId] = isTriggerDown[pointerId];
                }
                else //dragged off of the map area
                {
                    if (isDragging[pointerId] && !isTriggerDown[pointerId])
                    {
                        mapRenderImage.OnEndDrag(new PointerEventData(EventSystem.current) { position = cursorMapCoords, dragging = isDragging[pointerId] });
                        Debug.Log($"update:OnEndDrag: off map {cursorMapCoords}, {isDragging[pointerId]}");
                        isDragging[pointerId] = false;
                    }
                }

                if (isTriggerDown[pointerId] && isDragging[pointerId])
                {
                    mapRenderImage.OnDrag(new PointerEventData(EventSystem.current) { position = cursorMapCoords, dragging = isDragging[pointerId] });
                    Debug.Log($"update:OnDrag: {cursorMapCoords}, {isDragging[pointerId]}");
                }
            }
        }
    }

    public bool IsCursorOverMapChunk(uint pointerId)
    {
        if (CoreServices.FocusProvider.PrimaryPointer == null || CoreServices.FocusProvider.PrimaryPointer.Result.Details.Object == null)
        {
            isCursorOverMap[pointerId] = false;
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
            isCursorOverMap[pointerId] = true;
            return true;
        }
        // cursorMapCoords.x = -50000;
        // cursorMapCoords.y = -50000;
        isCursorOverMap[pointerId] = false;
        return false;
    }

    private void UpdateScale()
    {
        if (mapScale == dockParent.transform.localScale && resolution.x == Screen.width && resolution.y == Screen.height) return;
        resolution = new Vector2(Screen.width, Screen.height);
        mapScale = dockParent.transform.localScale;
        scaleFactor = (204.4004788f * mapScale.x) + ((mapScale.x*0.496038287f) * resolution.y)- (0.1510055f*mapScale.x/0.00075f);
        xscale.text = scaleFactor.ToString("F3");
        // offset.x = 10.7458680858f * Mathf.Pow( mapScale.x,-0.8137811006f);
        // offset.x = 2.4498915534f * (1/mapScale.x) + 1340.39437145239f*( resolution.x/resolution.y) - 2796;
        float offsetRatio = 0.79540889f * (resolution.x / resolution.y)  - ((mapScale.x - 0.00075f) / 0.00075f * 0.0644f) + 0.24355365f ;

        offset.x = -0.0000084957f * (1/mapScale.x*(resolution.x/resolution.y))*(1/mapScale.x*(resolution.x/resolution.y)) + 1.067531384f * (1/mapScale.x*(resolution.x/resolution.y)) + 365.9912663f;
        xoffset.text = offset.x.ToString("F2");
        offset.y = offset.x/offsetRatio;
        yoffset.text = offset.y.ToString("F2");
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        uint pointerId = eventData.Pointer.PointerId;

        if (!isTriggerDown.ContainsKey(pointerId))
        {
            isTriggerDown[pointerId] = false;
            isDragging[pointerId] = false;
            triggerLastState[pointerId] = false;
        }

        initialPressPosition[pointerId] = cursorMapCoords;
        initialPressTime[pointerId] = Time.time;
        isTriggerDown[pointerId] = true;
        lastEventData = eventData;
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        uint pointerId = eventData.Pointer.PointerId;
        if (!navmapIsRendered.Get() || !isTriggerDown.ContainsKey(pointerId))
            return;

        float distance = Vector2.Distance(initialPressPosition[pointerId], cursorMapCoords);
        float time = Time.time - initialPressTime[pointerId];

        if (!isDragging[pointerId] && (distance > dragDistance || time > dragTime))
        {
            isDragging[pointerId] = true;
        }
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        uint pointerId = eventData.Pointer.PointerId;
        isTriggerDown[pointerId] = false;
        if(isDragging[pointerId])
        {
            mapRenderImage.OnEndDrag(new PointerEventData(EventSystem.current) { position = cursorMapCoords, dragging = isDragging[pointerId] });
            Debug.Log($"OnPointerUp:OnEndDrag: {cursorMapCoords}, {isDragging[pointerId]}");
            eventData.Pointer.IsFocusLocked = true;
            isDragging[pointerId] = false;
        }
        else
        {
            PointerEventData data = new PointerEventData(EventSystem.current)
            {
                position = cursorMapCoords,
                dragging = false,
            };
            mapRenderImage.OnPointerClick(data);
            Debug.Log($"OnPointerUp:OnPointerClick: {cursorMapCoords}, {isDragging[pointerId]}");
        }
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        uint pointerId = eventData.Pointer.PointerId;
        if (!navmapIsRendered.Get() || isDragging[pointerId])
            return;

        if (IsCursorOverMapChunk(pointerId))
        {
            PointerEventData data = new PointerEventData(EventSystem.current)
            {
                position = cursorMapCoords,
                dragging = isDragging[pointerId],
            };
            mapRenderImage.OnPointerClick(data);
            Debug.Log($"OnPointerClicked:OnPointerClick: {cursorMapCoords}, {isDragging[pointerId]}");
        }
    }

    public void OnPointerExited(MixedRealityPointerEventData eventData)
    {
        uint pointerId = eventData.Pointer.PointerId;
        if (isDragging[pointerId])
        {
            mapRenderImage.OnEndDrag(new PointerEventData(EventSystem.current) { position = cursorMapCoords, dragging = isDragging[pointerId] });
            Debug.Log($"OnPointerExited:OnEndDrag: {cursorMapCoords}, {isDragging[pointerId]}");
            isDragging[pointerId] = false;
            isTriggerDown[pointerId] = false;
        }
    }

    public void IncreaseScale()
    {
        scaleFactor += 0.005f;
        xscale.text = scaleFactor.ToString("F3");
        SaveAdjustments();
    }
    public void DecreaseScale()
    {
        scaleFactor -= 0.005f;
        xscale.text = scaleFactor.ToString("F3");
        SaveAdjustments();
    }

    public void IncreaseXOffset()
    {
        offset.x += 10f;
        xoffset.text = offset.x.ToString("F2");
        offset.y = 0.4113556452f * offset.x + 196.2841761183f;
        yoffset.text = offset.y.ToString("F2");
        SaveAdjustments();
    }
    public void DecreaseXOffset()
    {
        offset.x -= 10f;
        xoffset.text = offset.x.ToString("F2");
        offset.y = 0.4113556452f * offset.x + 196.2841761183f;
        yoffset.text = offset.y.ToString("F2");
        SaveAdjustments();
    }
    public void IncreaseYOffset()
    {
        offset.y += 10f;
        yoffset.text = offset.y.ToString("F2");
    }
    public void DecreaseYOffset()
    {
        offset.y -= 10f;
        yoffset.text = offset.y.ToString("F2");
    }
    public void SaveAdjustments()
    {
        PlayerPrefs.SetFloat("mapOffsetX", offset.x);
        PlayerPrefs.SetFloat("mapOffsetY", offset.y);
        PlayerPrefs.SetFloat("mapScale", scaleFactor);
        PlayerPrefs.Save();
        Debug.Log($"NavMapOffsets Saved; x {offset.x}, y {offset.y}, scale {scaleFactor}");
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
        Debug.Log($"PointerHelper Offsets for NavMap loaded: o{offset}, s{scaleFactor}");

    }
}



