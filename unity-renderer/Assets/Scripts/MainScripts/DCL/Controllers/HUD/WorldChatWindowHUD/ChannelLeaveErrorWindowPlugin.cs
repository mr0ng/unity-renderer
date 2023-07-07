using DCL.Providers;
using System.Threading;

namespace DCL.Chat.HUD
{
    public class ChannelLeaveErrorWindowPlugin : IPlugin
    {
        private readonly CancellationTokenSource cts = new ();

        private ChannelLeaveErrorWindowController controller;

        public ChannelLeaveErrorWindowPlugin()
        {
            Initialize(cts.Token);
        }

        private async void Initialize(CancellationToken ct)
        {
            #if DCL_VR
            var view = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                        .Instantiate<ChannelLeaveErrorWindowComponentView>("ChannelLeaveErrorModalVR", cancellationToken: ct);
            #else
            var view = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                        .Instantiate<ChannelLeaveErrorWindowComponentView>("ChannelLeaveErrorModal", cancellationToken: ct);
#endif
            controller = new ChannelLeaveErrorWindowController(view, Environment.i.serviceLocator.Get<IChatController>(), DataStore.i);
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
            controller.Dispose();
        }
    }
}
