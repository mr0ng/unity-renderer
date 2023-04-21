using System;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class ChannelLimitReachedWindowComponentView : BaseComponentView, IChannelLimitReachedWindowView
    {
        [SerializeField] internal ButtonComponentView[] acceptButton;

        public event Action OnClose;

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        public static ChannelLimitReachedWindowComponentView Create()
        {
            #if DCL_VR
            return Instantiate(
                Resources.Load<ChannelLimitReachedWindowComponentView>("SocialBarV1/ChannelLimitReachedModalVR"));
            #else
            return Instantiate(
                Resources.Load<ChannelLimitReachedWindowComponentView>("SocialBarV1/ChannelLimitReachedModal"));
            #endif
        }

        public override void Awake()
        {
            base.Awake();

            foreach (var button in acceptButton)
                button.onClick.AddListener(() => OnClose?.Invoke());
        }

        public override void RefreshControl()
        {
        }
    }
}
