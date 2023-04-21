using Cysharp.Threading.Tasks;
using DCL;
using DCL.Browser;
using DCL.Chat;
using DCL.Chat.HUD;
using DCL.HelpAndSupportHUD;
using DCL.Huds.QuestsPanel;
using DCL.Huds.QuestsTracker;
using DCL.ProfanityFiltering;
using DCL.Providers;
using DCL.SettingsCommon;
using DCL.SettingsPanelHUD;
using DCL.Social.Chat.Mentions;
using DCL.Social.Friends;
using DCLServices.WearablesCatalogService;
using SignupHUD;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;
using static MainScripts.DCL.Controllers.HUD.HUDAssetPath;
using Environment = DCL.Environment;

public class HUDFactory : IHUDFactory
{
    private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;
    private readonly List<IDisposable> disposableViews;

    private Service<IAddressableResourceProvider> assetsProviderService;
    private IAddressableResourceProvider assetsProviderRef;

    private IAddressableResourceProvider assetsProvider => assetsProviderRef ??= assetsProviderService.Ref;
    public static event Action<IHUD> OnViewCreated;
    protected HUDFactory()
    {
        disposableViews = new List<IDisposable>();
    }

    public HUDFactory(IAddressableResourceProvider assetsProvider)
    {
        disposableViews = new List<IDisposable>();
        assetsProviderRef = assetsProvider;
    }

    public void Initialize() { }

    public void Dispose()
    {
        foreach (IDisposable view in disposableViews)
            view.Dispose();
    }

    public virtual async UniTask<IHUD> CreateHUD(HUDElementID hudElementId, CancellationToken cancellationToken = default)
    {
        IHUD hud = null;
        switch (hudElementId)
        {

            case HUDElementID.NONE:
                break;
            case HUDElementID.MINIMAP:
                hud = new MinimapHUDController(MinimapMetadataController.i, new WebInterfaceHomeLocationController(), Environment.i);

                break;
            case HUDElementID.PROFILE_HUD:
                hud = new ProfileHUDController(new UserProfileWebInterfaceBridge(),
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    DataStore.i);

                break;
            case HUDElementID.NOTIFICATION:
                hud =  new NotificationHUDController();
                break;
            case HUDElementID.AVATAR_EDITOR:
                hud = new AvatarEditorHUDController(
                    DataStore.i.featureFlags,
                    Environment.i.platform.serviceProviders.analytics,
                    Environment.i.serviceLocator.Get<IWearablesCatalogService>());

                break;
            case HUDElementID.SETTINGS_PANEL:
                hud =  new SettingsPanelHUDController();
                break;
            case HUDElementID.TERMS_OF_SERVICE:
                hud =  new TermsOfServiceHUDController();
                break;
            case HUDElementID.FRIENDS:
                hud =  new FriendsHUDController(DataStore.i,
                    FriendsController.i,
                    new UserProfileWebInterfaceBridge(),
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    Environment.i.serviceLocator.Get<IChatController>(),
                    SceneReferences.i.mouseCatcher);

                break;
            case HUDElementID.WORLD_CHAT_WINDOW:
                hud = new WorldChatWindowController(
                    new UserProfileWebInterfaceBridge(),
                    FriendsController.i,
                    Environment.i.serviceLocator.Get<IChatController>(),
                    DataStore.i,
                    SceneReferences.i.mouseCatcher,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    Environment.i.serviceLocator.Get<IChannelsFeatureFlagService>(),
                    new WebInterfaceBrowserBridge(),
                    CommonScriptableObjects.rendererState,
                    DataStore.i.mentions);

                break;
            case HUDElementID.PRIVATE_CHAT_WINDOW:
                hud = new PrivateChatWindowController(
                    DataStore.i,
                    new UserProfileWebInterfaceBridge(),
                    Environment.i.serviceLocator.Get<IChatController>(),
                    FriendsController.i,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    SceneReferences.i.mouseCatcher,
                    new MemoryChatMentionSuggestionProvider(UserProfileController.i, DataStore.i));

                break;
            case HUDElementID.PUBLIC_CHAT:
                hud = new PublicChatWindowController(
                    Environment.i.serviceLocator.Get<IChatController>(),
                    new UserProfileWebInterfaceBridge(),
                    DataStore.i,
                    Environment.i.serviceLocator.Get<IProfanityFilter>(),
                    SceneReferences.i.mouseCatcher,
                    new MemoryChatMentionSuggestionProvider(UserProfileController.i, DataStore.i),
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()));

                break;
            case HUDElementID.CHANNELS_CHAT:
                hud = new ChatChannelHUDController(
                    DataStore.i,
                    new UserProfileWebInterfaceBridge(),
                    Environment.i.serviceLocator.Get<IChatController>(),
                    SceneReferences.i.mouseCatcher,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    Environment.i.serviceLocator.Get<IProfanityFilter>(),
                    new MemoryChatMentionSuggestionProvider(UserProfileController.i, DataStore.i));

                break;
            case HUDElementID.CHANNELS_SEARCH:
                hud = new SearchChannelsWindowController(
                    Environment.i.serviceLocator.Get<IChatController>(),
                    SceneReferences.i.mouseCatcher,
                    DataStore.i,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    Environment.i.serviceLocator.Get<IChannelsFeatureFlagService>());

                break;
            case HUDElementID.CHANNELS_CREATE:
                hud = new CreateChannelWindowController(Environment.i.serviceLocator.Get<IChatController>(), DataStore.i);
                break;
            case HUDElementID.CHANNELS_LEAVE_CONFIRMATION:
                hud = new LeaveChannelConfirmationWindowController(Environment.i.serviceLocator.Get<IChatController>());
                break;
            case HUDElementID.TASKBAR:
                hud = new TaskbarHUDController(Environment.i.serviceLocator.Get<IChatController>(), FriendsController.i);
                break;
            case HUDElementID.OPEN_EXTERNAL_URL_PROMPT:
                hud = new ExternalUrlPromptHUDController();
                break;
            case HUDElementID.NFT_INFO_DIALOG:
                hud = new NFTPromptHUDController();
                break;
            case HUDElementID.TELEPORT_DIALOG:
                hud = new TeleportPromptHUDController();
                break;
            case HUDElementID.CONTROLS_HUD:
                hud = new ControlsHUDController();
                break;
            case HUDElementID.HELP_AND_SUPPORT_HUD:
                hud = new HelpAndSupportHUDController(await CreateHUDView<IHelpAndSupportHUDView>(HELP_AND_SUPPORT_HUD, cancellationToken));
                break;
            case HUDElementID.USERS_AROUND_LIST_HUD:
                hud = new VoiceChatWindowController(
                    new UserProfileWebInterfaceBridge(),
                    FriendsController.i,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    DataStore.i,
                    Settings.i,
                    SceneReferences.i.mouseCatcher);
                break;
            case HUDElementID.GRAPHIC_CARD_WARNING:
                hud = new GraphicCardWarningHUDController();
                break;
            case HUDElementID.QUESTS_PANEL:
                hud = new QuestsPanelHUDController();
                break;
            case HUDElementID.QUESTS_TRACKER:
                hud = new QuestsTrackerHUDController(await CreateHUDView<IQuestsTrackerHUDView>(QUESTS_TRACKER_HUD, cancellationToken));
                break;
            case HUDElementID.SIGNUP:
                hud = new SignupHUDController(Environment.i.platform.serviceProviders.analytics, await CreateSignupHUDView(cancellationToken), dataStoreLoadingScreen.Ref);
                break;
        }
// #if DCL_VR
//         if (hud != null)
//         {
//             try
//             {
//                 OnViewCreated?.Invoke(hud);
//                 NotifyVRUIManager(hud);
//             }
//             catch(Exception ex)
//             {
//                 Debug.LogError(ex.Message);
//             }
//         }
//         else
//         {
//             OnViewCreated?.Invoke(hud);
//         }
// #endif

        return hud;
    }
    private void NotifyVRUIManager(IHUD hud)
    {
        PropertyInfo viewProperty = hud.GetType().GetProperty("view");
        if (viewProperty != null)
        {
            object view = viewProperty.GetValue(hud);
            if (view is Component viewComponent)
            {
                VRUIManager.Instance.OnUIElementCreated(viewComponent.gameObject);
            }
            else
                OnViewCreated?.Invoke(hud);
        }
        else
            OnViewCreated?.Invoke(hud);
    }
    public async UniTask<ISignupHUDView> CreateSignupHUDView(CancellationToken cancellationToken = default) =>
        await CreateHUDView<ISignupHUDView>(SIGNUP_HUD, cancellationToken);

    protected async UniTask<T> CreateHUDView<T>(string assetAddress, CancellationToken cancellationToken = default) where T:IDisposable
    {
        var view = await assetsProvider.Instantiate<T>(assetAddress, $"_{assetAddress}", cancellationToken);
        disposableViews.Add(view);

        return view;
    }
}
