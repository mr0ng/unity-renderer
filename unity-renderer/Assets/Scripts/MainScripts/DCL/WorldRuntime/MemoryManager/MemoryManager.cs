using System;
using System.Collections;
using System.Collections.Generic;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using DCL.SettingsCommon.SettingsControllers.SpecificControllers;
using UnityEngine;
using UnityEngine.Profiling;

namespace DCL
{
    public class MemoryManager : IMemoryManager
    {

        private const long MAX_USED_MEMORY = 1600 * 1024 * 1024; // 1.6GB
        #if UNITY_ANDROID && !UNITY_EDITOR
        private const long MAX_GFX_MEMORY = 600 * 1024 * 1024;
        #else
        private const long MAX_GFX_MEMORY = 2500000;
#endif
        private const float TIME_FOR_NEW_MEMORY_CHECK = 60.0f;
        private SettingsControlController sceneLoadRadiusSettingController;
        private Coroutine autoCleanupCoroutine;

        private long memoryThresholdForCleanup = MAX_USED_MEMORY;
        private float cleanupInterval;
        private bool memoryIssue = false;
        private long totalAllocatedMemoryLong;
        private long monoUsedSizeLong;
        private long allocatedMemoryForGraphicsDriver;
        private long totalUnusedReservedMemoryLong;
        private long totalMemory;
        public event System.Action OnCriticalMemory;

        public MemoryManager(long memoryThresholdForCleanup, float cleanupInterval)
        {
            this.memoryThresholdForCleanup = memoryThresholdForCleanup;
            this.cleanupInterval = cleanupInterval;
            sceneLoadRadiusSettingController = ScriptableObject.CreateInstance<ScenesLoadRadiusControlController>();
            sceneLoadRadiusSettingController.Initialize();
            autoCleanupCoroutine = CoroutineStarter.Start(AutoCleanup());
        }

        public MemoryManager()
        {
            this.memoryThresholdForCleanup = MAX_USED_MEMORY;
           
            this.cleanupInterval = TIME_FOR_NEW_MEMORY_CHECK;
            sceneLoadRadiusSettingController = ScriptableObject.CreateInstance<ScenesLoadRadiusControlController>();
            sceneLoadRadiusSettingController.Initialize();
            autoCleanupCoroutine = CoroutineStarter.Start(AutoCleanup());
           
        }

        public void Dispose()
        {
            if (autoCleanupCoroutine != null)
                CoroutineStarter.Stop(autoCleanupCoroutine);

            autoCleanupCoroutine = null;
        }

        public void Initialize()      
        {
            Application.lowMemory += OnLowMemory;
        }

        bool NeedsMemoryCleanup()
        {
            totalAllocatedMemoryLong = Profiler.GetTotalAllocatedMemoryLong();
            monoUsedSizeLong = Profiler.GetMonoUsedSizeLong(); 
            allocatedMemoryForGraphicsDriver = Profiler.GetAllocatedMemoryForGraphicsDriver();
            totalUnusedReservedMemoryLong = Profiler.GetTotalUnusedReservedMemoryLong();
            totalMemory = totalAllocatedMemoryLong+monoUsedSizeLong+allocatedMemoryForGraphicsDriver;
            Debug.Log($"Performance:  Memory tot{totalMemory/1000000} ,alloc{totalAllocatedMemoryLong/1000000},mono{monoUsedSizeLong/1000000},gfx{allocatedMemoryForGraphicsDriver/1000000}, unusedreserved{totalUnusedReservedMemoryLong/1000000}");
            if (memoryIssue &&  totalMemory < 0.7 * MAX_USED_MEMORY)// && allocatedMemoryForGraphicsDriver < 0.7 * MAX_GFX_MEMORY)
            {
                OnMemoryRestored();
            }
            return (totalMemory >= this.memoryThresholdForCleanup); // || allocatedMemoryForGraphicsDriver >= MAX_GFX_MEMORY) ;

        }

        IEnumerator AutoCleanup()
        {
            while (true)
            {
                if (NeedsMemoryCleanup() && !isCleaning)
                {
                    //object newLOS = sceneLoadRadiusSettingController.GetStoredValue();
                    //float.TryParse(newLOS.ToString(), out float newVal);
                    //if(newVal >=2)
                    // sceneLoadRadiusSettingController.UpdateSetting(newVal-1);
                    memoryIssue = true;
                    OnCriticalMemory?.Invoke();
                    OnLowMemory();
                    
                   //Resources.UnloadUnusedAssets();
                }
                yield return new WaitForSecondsRealtime(this.cleanupInterval);
                
            }
        }
        private bool isCleaning = false;

        private void OnLowMemory()
        {
            // object newLOS = sceneLoadRadiusSettingController.GetStoredValue();
            // float.TryParse(newLOS.ToString(), out float newVal);
            // if (newVal > 1)
            //     sceneLoadRadiusSettingController.UpdateSetting(newVal - 1);
            //
            // DataStore.i.textureConfig.generalMaxSize.Set(64);
            // DataStore.i.textureConfig.gltfMaxSize.Set(64);
            // //stop loading tasks.
            // Debug.LogError("LowMemory hit on device");
            
            CoroutineStarter.Start(CleanPoolManager());
        }
        private void OnMemoryRestored()
        {
            // DataStore.i.textureConfig.generalMaxSize.Set(256);
            // DataStore.i.textureConfig.gltfMaxSize.Set(256);
            // object newLOS = sceneLoadRadiusSettingController.GetStoredValue();
            // float.TryParse(newLOS.ToString(), out float newVal);
            // if (newVal < 4)
            //     sceneLoadRadiusSettingController.UpdateSetting(newVal + 1);
            memoryIssue = false;
        }
        public IEnumerator CleanPoolManager(bool forceCleanup = false, bool immediate = false)
        {
            bool unusedOnly = true;
            bool nonPersistentOnly = true;
            isCleaning = true;
            if (forceCleanup)
            {
                unusedOnly = false;
                nonPersistentOnly = false;
            }

            if (immediate)
            {
                PoolManager.i.Cleanup(unusedOnly, nonPersistentOnly);
            }
            else
            {
                yield return PoolManager.i.CleanupAsync(unusedOnly, nonPersistentOnly, false);
            }
            Resources.UnloadUnusedAssets();
            isCleaning = false;
        }
    }
}