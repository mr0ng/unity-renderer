using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Input.Utilities;
using UnityEngine;
using UnityEngine.UI;

public abstract class VRHUDHelper : MonoBehaviour
{
    [SerializeField]
    private int sortingOrder;
    
    protected Transform myTrans;

    private void Awake()
    {
        if (!CrossPlatformManager.IsVR)
        {
            enabled = false;
            return;
        }
        myTrans = transform;
    }

    private void Start()
    {
        ConvertUI();
        SetupHelper();
    }

    protected abstract void SetupHelper();

    private void ConvertUI()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();
        if (GetComponent<CanvasUtility>() == null)
            gameObject.AddComponent<CanvasUtility>();
        if (GetComponent<NearInteractionTouchableUnityUI>() == null)
            gameObject.AddComponent<NearInteractionTouchableUnityUI>();
    }

    private void OnEnable()
    {
        RunOnEnable();
    }
    
    protected abstract void RunOnEnable();

    private void OnDisable()
    {
        if (!CrossPlatformManager.IsVR) return;
        RunOnDisable();
    }
    protected abstract void RunOnDisable();

    public void Hide(Vector3 pos) => myTrans.position += pos;

    public void ResetHud()
    {
        myTrans.localPosition = Vector3.zero;
        myTrans.localRotation = Quaternion.identity;
    }
}