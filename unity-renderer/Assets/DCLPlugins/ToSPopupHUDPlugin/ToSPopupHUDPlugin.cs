using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using MainScripts.DCL.Controllers.HUD.ToSPopupHUD;

namespace DCLPlugins.ToSPopupHUDPlugin
{
    public class ToSPopupHUDPlugin : IPlugin
    {
        private ToSPopupController controller;

        public ToSPopupHUDPlugin()
        {
            Initialize().Forget();
        }

        private async UniTaskVoid Initialize()
        {
            await Environment.WaitUntilInitialized();
            var assetsProvider = Environment.i.platform.serviceLocator.Get<IAddressableResourceProvider>();
            var hudsDataStore = DataStore.i.HUDs;
            #if DCL_VR
            var view = await assetsProvider.Instantiate<IToSPopupView>("ToSPopupHUDVR", "_ToSPopupHUDVR");
            #else
            var view = await assetsProvider.Instantiate<IToSPopupView>("ToSPopupHUD", "_ToSPopupHUD");
            #endif
            controller = new ToSPopupController(view, hudsDataStore.tosPopupVisible, new ToSPopupHandler(hudsDataStore.tosPopupVisible));
        }

        public void Dispose()
        {
            controller?.Dispose();
        }
    }
}
