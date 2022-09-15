using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using DCL.SettingsCommon.SettingsControllers.SpecificControllers;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Logger = UnityEngine.Logger;


public class PerformanceController : MonoBehaviour
{
   
    private Camera camera;
    private SettingsControlController renderingScaleSettingController;
    private SettingsControlController sceneLoadRadiusSettingController;
    private long totalAllocatedMemoryLong;
    private long monoUsedSizeLong;
    private long allocatedMemoryForGraphicsDriver;
    private long totalUnusedReservedMemoryLong;
    private long totalMemory;
    private int frameCount = 100;
    #if UNITY_ANDROID && !UNITY_EDITOR
    private long MaxMemoryAllowed = 1800000000;
    #else
    private long MaxMemoryAllowed = 5800000000;
    #endif
    // Start is called before the first frame update
   
    private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;
    void Start()
    {
       
        renderingScaleSettingController = ScriptableObject.CreateInstance<RenderingScaleControlController>();
        renderingScaleSettingController.Initialize();
        sceneLoadRadiusSettingController = ScriptableObject.CreateInstance<ScenesLoadRadiusControlController>();
        sceneLoadRadiusSettingController.Initialize();
        StartCoroutine(CheckPerformace());
        Application.lowMemory += OnLowMemory;
    }
   

    // Update is called once per frame
    void Update()
    {
        
    }
    private WaitForSeconds waitTimeCheck = new WaitForSeconds(1f);
    private IEnumerator CheckPerformace()
    {
        while (true)
        {
            yield return waitTimeCheck;
            DateTime startTime = DateTime.Now;
            for (int i = 0; i < frameCount; i++)
            {
                yield return null;
                
            }
            
            TimeSpan frameSpan = (DateTime.Now - startTime);

            double fps =  1 / frameSpan.TotalSeconds*frameCount ;
            //Debug.Log($"frameTime Span {frameSpan.TotalMilliseconds/5}ms, fps {fps} , memory {System.GC.GetTotalMemory(false)} of max {Profiler.GetMonoHeapSize()}");
            totalAllocatedMemoryLong = Profiler.GetTotalAllocatedMemoryLong();
            monoUsedSizeLong = Profiler.GetMonoUsedSizeLong(); 
            allocatedMemoryForGraphicsDriver = Profiler.GetAllocatedMemoryForGraphicsDriver();
            totalUnusedReservedMemoryLong = Profiler.GetTotalUnusedReservedMemoryLong();
            totalMemory = totalAllocatedMemoryLong+monoUsedSizeLong+allocatedMemoryForGraphicsDriver;
            Debug.Log($"Performance: fps{fps}, Memory tot{totalMemory/1000000} ,alloc{totalAllocatedMemoryLong/1000000},mono{monoUsedSizeLong/1000000},gfx{allocatedMemoryForGraphicsDriver/1000000}, unusedres{totalUnusedReservedMemoryLong/1000000}");
            if ( totalMemory > MaxMemoryAllowed) OnLowMemory();
            if (totalMemory < (.73 * MaxMemoryAllowed))
            {
                RestoreSettings();
            }
            if(fps < 65) OnLowFrameRate();
            else if (fps > 72) GoodFrameRate();
        }
    }
    
    
    private void RestoreSettings() { 
        camera = Camera.main;
        
        //Move clipping plane further
        // camera.farClipPlane = 300;
        //
        // float newValue = 1.0f;
        // renderingScaleSettingController.UpdateSetting(newValue);
        
        
        object newLOS = sceneLoadRadiusSettingController.GetStoredValue();
        float.TryParse(newLOS.ToString(), out float newVal);
        if (newVal <= 3){
            sceneLoadRadiusSettingController.UpdateSetting((newVal +1));
        }
        Debug.Log($"memory good.  Changed paramaters: farclipplane {camera.farClipPlane}, LOS {newVal+1}, ");
    }
    private void OnLowMemory()
    {
        camera = Camera.main;
        //TODO: drop LOS, and immediately destroy further parcels.
         
        sceneLoadRadiusSettingController.UpdateSetting(1.0f);
       
        Resources.UnloadUnusedAssets();
        GC.Collect();
        Debug.Log($"Low memory reached.  Changed paramaters: farclipplane {camera.farClipPlane}, LOS 1, ");
    }
    
    private void OnLowFrameRate()
    {
        if (camera.farClipPlane > 30)
            camera.farClipPlane = camera.farClipPlane * .9f;
        object newValue = renderingScaleSettingController.GetStoredValue();
        float.TryParse(newValue.ToString(), out float newFloat);
        if (newFloat > 0.4f)
        {
            renderingScaleSettingController.UpdateSetting(newFloat*0.9f);
        }
        Debug.Log($"Low FrameRate.  Changed paramaters: farclipplane {camera.farClipPlane}, renderscale {newFloat*1.1f} ");

    }
    private void GoodFrameRate()
    {
        if (camera.farClipPlane < 500)
            camera.farClipPlane = camera.farClipPlane * 1.15f;
        object newValue = renderingScaleSettingController.GetStoredValue();
        float.TryParse(newValue.ToString(), out float newFloat);
        if (newFloat < 1f)
        {
            renderingScaleSettingController.UpdateSetting(newFloat*1.1f);
        }
        Debug.Log($"FrameRate Good.  Changed paramaters: farclipplane {camera.farClipPlane}, renderscale {newFloat*1.1f}, ");
       
    }
}

