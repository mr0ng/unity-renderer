using DCL.SettingsPanelHUD;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HUD.SettingsPanelHUDDesktop.Scripts
{
    public class SettingsPanelHUDViewDesktop : SettingsPanelHUDView
    {
        //#if DCL_VR
        //private const string DESKTOP_PATH = "SettingsPanelHUDDesktopVR";
        //#else
        private const string DESKTOP_PATH = "SettingsPanelHUDDesktop";
        //#endif
        internal static SettingsPanelHUDView Create()
        {
            SettingsPanelHUDView view = Instantiate(Resources.Load<GameObject>(DESKTOP_PATH)).GetComponent<SettingsPanelHUDView>();
            view.name = "_SettingsPanelHUDDesktop";
            return view;
        }
    }
}
