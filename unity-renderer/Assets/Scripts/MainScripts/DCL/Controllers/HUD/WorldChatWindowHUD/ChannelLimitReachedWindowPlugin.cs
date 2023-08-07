using DCL.Social.Chat;
using DCL.Providers;
using System.Threading;

namespace DCL.Social.Chat
{
    public class ChannelLimitReachedWindowPlugin : IPlugin
    {
        private readonly CancellationTokenSource cts = new ();

        private ChannelLimitReachedWindowController channelLimitReachedWindow;

        public ChannelLimitReachedWindowPlugin()
        {
            Initialize(cts.Token);
        }

        private async void Initialize(CancellationToken ct)
        {
            #if DCL_VR
            var view = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                        .Instantiate<ChannelLimitReachedWindowComponentView>("ChannelLimitReachedModalVR", cancellationToken: ct);
            #else
            var view = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                    .Instantiate<ChannelLimitReachedWindowComponentView>("ChannelLimitReachedModal", cancellationToken: ct);
#endif
            view.name = "ChannelLimitReachedModal";
            channelLimitReachedWindow = new ChannelLimitReachedWindowController(view, DataStore.i);
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
            channelLimitReachedWindow.Dispose();
        }
    }
}
