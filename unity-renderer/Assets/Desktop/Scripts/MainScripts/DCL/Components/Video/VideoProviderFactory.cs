using DCL;
using DCL.Components.Video.Plugin;
using UnityEngine;

public class VideoProviderFactory
{
    public static IVideoPluginWrapper CreateVideoProvider()
    {
#if AV_PRO_PRESENT
        if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("use_avpro_player") && Application.platform != RuntimePlatform.LinuxPlayer)
        {
            Debug.Log($"VideoProvider using AVPro");
            return new VideoPluginWrapper_AVPro();
        }
#endif
#if !UNITY_ANDROID
        Debug.Log($"VideoProvider using Native");
        return new VideoPluginWrapper_Native();
#else
        Debug.Log($"VideoProvider using None- returned null");
        return null;
#endif
    }
}
