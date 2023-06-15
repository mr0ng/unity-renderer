using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerHelper : MonoBehaviour
{
    public static PointerHelper Instance { get; private set; }
    [SerializeField]
    private Vector3 offset;
    private RectTransform referenceTrans;
    private readonly Vector3[] corners = new Vector3[4];
    private Vector3 origin;
    private bool isVR;

    private void Awake()
    {
        isVR = CrossPlatformManager.IsVRPlatform();
        Instance = this;
    }

    public Vector3 GetPointerPos()
    {
        if (!isVR)
            return Input.mousePosition;
        UpdateOrigin();
        Vector3 localPointerPos = default;
        var hit = CoreServices.FocusProvider?.PrimaryPointer.Result.Details.Point;

        Vector3 point = ToLocalSpace(hit ?? Vector3.zero);
        localPointerPos = point - origin;

        return localPointerPos + offset;
    }
    private Vector3 ToLocalSpace(Vector3 hitpoint)
    {
        if (referenceTrans == null) return Vector3.zero;
        return referenceTrans.worldToLocalMatrix.MultiplyPoint(hitpoint);
    }
    private void UpdateOrigin()
    {
        if (referenceTrans != null)
            referenceTrans.GetLocalCorners(corners);
        origin = corners[0];
    }

    public void UpdateCorners(Vector3[] refCorners, RectTransform rectTrans, ref Vector3 worldCoordsOriginInMap)
    {
        rectTrans.GetWorldCorners(corners);
        rectTrans.GetWorldCorners(refCorners);
        if (isVR)
            worldCoordsOriginInMap = rectTrans.worldToLocalMatrix.MultiplyPoint(refCorners[0]);
        else
            worldCoordsOriginInMap = refCorners[0];
        if (referenceTrans == null) referenceTrans = rectTrans;
    }

    public bool IsCursorOverMapChunk(int layer)
    {
        if (!isVR)
            return IsMouseOverUI(layer);
        return IsPointerOnUI(layer);
    }

    private bool IsPointerOnUI(int layer)
    {
        var target = CoreServices.FocusProvider?.PrimaryPointer.Result.CurrentPointerTarget;
        return target != null && target.layer == layer;
    }

    private bool IsMouseOverUI(int layer)
    {
        PointerEventData uiRaycastPointerEventData = new PointerEventData(EventSystem.current);
        uiRaycastPointerEventData.position = Input.mousePosition;
        List<RaycastResult> uiRaycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(uiRaycastPointerEventData, uiRaycastResults);

        return uiRaycastResults.Count > 0 && uiRaycastResults[0].gameObject.layer == layer;
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData) { throw new NotImplementedException(); }
    public void OnPointerDragged(MixedRealityPointerEventData eventData) { throw new NotImplementedException(); }
    public void OnPointerUp(MixedRealityPointerEventData eventData) { throw new NotImplementedException(); }
    public void OnPointerClicked(MixedRealityPointerEventData eventData) { throw new NotImplementedException(); }
}
