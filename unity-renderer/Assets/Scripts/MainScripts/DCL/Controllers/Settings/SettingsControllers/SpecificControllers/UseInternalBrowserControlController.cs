using DCL.Interface;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Use Internal Browser", fileName = "ControlController")]
    public class UseInternalBrowserControlController : ToggleSettingsControlController
    {
        private GeneralSettings currentSettings;
        public override void Initialize()
        {
            base.Initialize();
            CommonScriptableObjects.useInternalBrowser.OnChange += UseInternalBrowserChanged;

            UseInternalBrowserChanged(currentGeneralSettings.useInternalBrowser);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            CommonScriptableObjects.useInternalBrowser.OnChange -= UseInternalBrowserChanged;
        }

        public override object GetStoredValue() =>
            currentGeneralSettings.useInternalBrowser;

        public override void UpdateSetting(object newValue) =>
            CommonScriptableObjects.useInternalBrowser.Set((bool)newValue);

        private void UseInternalBrowserChanged(bool current, bool _ = false)
        {
            currentGeneralSettings.useInternalBrowser = current;
            // WebInterface.openURLInternal = current;
            ApplySettings();
        }
    }
}
