using DCL.Helpers;
using DCL.Providers;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class PromoteChannelsToastPlugin : IPlugin
    {
        private readonly CancellationTokenSource cts = new ();

        private PromoteChannelsToastComponentController promoteChannelsToastController;

        public PromoteChannelsToastPlugin()
        {
            Initialize(cts.Token);
        }

        private async void Initialize(CancellationToken ct)
        {
            #if DCL_VR
            var view = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                        .Instantiate<PromoteChannelsToastComponentView>("PromoteChannelsHUDVR", cancellationToken: ct);
            #else
            var view = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                        .Instantiate<PromoteChannelsToastComponentView>("PromoteChannelsHUD", cancellationToken: ct);
#endif

            promoteChannelsToastController = new PromoteChannelsToastComponentController(
                view, new DefaultPlayerPrefs(), DataStore.i, CommonScriptableObjects.rendererState);
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
            promoteChannelsToastController.Dispose();
        }
    }
}
