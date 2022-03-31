using System;
using Microsoft.MixedReality.Toolkit.Input.Utilities;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEditor;
using UnityEngine.UI;

public class ConvertUIForVR : MonoBehaviour
{
    internal enum UIBehavior
    {
        Loading,
        Stationary,
        Overlay,
        Interactive
    }

    [SerializeField]
    internal UIBehavior behavior;
    [SerializeField]
    private Vector3 scale;
    
    [SerializeField]

    private Transform cameraTrans;
    private GameObject loadingCamera;
    
    private void Awake()
    {
        if (CrossPlatformManager.IsVRPlatform())
            ConvertUI();
    }
    
    private void ConvertUI()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        transform.localScale = scale;
        if (!TryGetComponent<GraphicRaycaster>(out var caster))
            gameObject.AddComponent<GraphicRaycaster>();
        if (!TryGetComponent<CanvasUtility>(out var utility))
            gameObject.AddComponent<CanvasUtility>();

        switch (behavior)
        {
            case UIBehavior.Loading:
                if (cameraTrans == null)
                {
                    loadingCamera = Instantiate(Resources.Load<GameObject>("LoadingCamera"));
                    cameraTrans = loadingCamera.transform;
                }
                break;
            case UIBehavior.Interactive :
                var orbital = gameObject.AddComponent<Orbital>();
                break;
        }
    }

    private void RunBehavior()
    {
        switch (behavior)
        {
            case UIBehavior.Loading :
                loadingCamera.SetActive(true);
                transform.position = new Vector3(0f, 1.5f, 1f);
                break;
        }
    }

    private void OnEnable()
    {
        Debug.LogWarning("Set Camera active");
        if (CrossPlatformManager.IsVRPlatform())
            RunBehavior();
    }

    private void OnDisable()
    {
        Debug.LogWarning("set camera inactive");
        loadingCamera.SetActive(false);
    }
}
