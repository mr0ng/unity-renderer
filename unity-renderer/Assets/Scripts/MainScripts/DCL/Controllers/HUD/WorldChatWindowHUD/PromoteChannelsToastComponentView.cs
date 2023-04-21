using System;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class PromoteChannelsToastComponentView : BaseComponentView, IPromoteChannelsToastComponentView
    {
        [SerializeField] internal ButtonComponentView closeButton;

        public event Action OnClose;

        public override void Awake()
        {
            base.Awake();

            closeButton.onClick.AddListener(() => OnClose?.Invoke());
        }

        public override void RefreshControl()
        {
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        public static PromoteChannelsToastComponentView Create()
        {
            #if DCL_VR
            return Instantiate(
                Resources.Load<PromoteChannelsToastComponentView>("SocialBarV1/PromoteChannelsHUDVR"));
            #else
            return Instantiate(
                Resources.Load<PromoteChannelsToastComponentView>("SocialBarV1/PromoteChannelsHUD"));
            #endif
        }
    }
}
