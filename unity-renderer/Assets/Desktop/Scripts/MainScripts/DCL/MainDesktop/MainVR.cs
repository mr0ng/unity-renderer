using System;
using DCL.SettingsCommon;
using DCL.Components;
using DCL.Configuration;
using DCL.Interface;
using DCL.Providers;
using DCL.VR;
using MainScripts.DCL.Controllers.HUD.Preloading;

using MainScripts.DCL.Controllers.SettingsDesktop;
using UnityEditor;
using UnityEngine;
using UnityEngine.Diagnostics;
using DCL.Helpers;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using PlayerSettings = DCL.Configuration.PlayerSettings;

namespace DCL
{
    /// <summary>
    /// This is the MainDesktop entry point.
    /// Most of the application subsystems should be initialized from this class Awake() event.
    /// </summary>
    public class MainVR : Main
    {
        [SerializeField] private bool logWs = false;
        //private PreloadingController preloadingController;
        private bool isConnectionLost;
        private readonly DataStoreRef<DataStore_LoadingScreen> loadingScreenRef;

        private BaseVariable<FeatureFlag> featureFlags => DataStore.i.featureFlags.flags;
        private Transform mixedRealityPlayspace;
        private Transform cameraParent;
        protected override void Awake()
        {
            #if !DCL_VR
            Debug.LogError("DCL_VR not added in Player Compiler Defines. Please add DCL_VR under player setting Defines to enable full VR capabilities");

#endif
            UnityThread.initUnityThread(true);
            CommandLineParserUtils.ParseArguments();
            isConnectionLost = false;

            DCLVideoTexture.videoPluginWrapperBuilder = VideoProviderFactory.CreateVideoProvider;

            InitializeSettings();
            DataStore.i.wsCommunication.communicationReady.OnChange += RestartSocketServer;
            base.Awake();

            DataStore.i.wsCommunication.communicationEstablished.OnChange += OnCommunicationEstablished;
            DataStore.i.performance.multithreading.Set(true);
            DataStore.i.performance.maxDownloads.Set(50);
            Texture.allowThreadedTextureCreation = true;

            //TODO: Integrate preloading controller to LoadingScreenPlugin. Currently not visible
            //preloadingController = new PreloadingController(Environment.i.serviceLocator.Get<IAddressableResourceProvider>());
            // loadingFlowController = new LoadingFlowController(
            //     loadingScreenRef.Ref.decoupledLoadingHUD.visible,
            //     CommonScriptableObjects.rendererState,
            //     DataStore.i.wsCommunication.communicationEstablished);
        }

        protected override void Start()
        {
            WebInterface.SendSystemInfoReport();

            // We trigger the Decentraland logic once everything is initialized.
            WebInterface.StartDecentraland();
            // WebSocketCommunication.OnProfileLoading += OnProfileLoading;
            // mixedRealityPlayspace = VRPlaySpace.i.transform;
            // cameraParent = Camera.main.transform.parent;
            // mixedRealityPlayspace.parent = cameraParent;
            // mixedRealityPlayspace.localPosition = new Vector3(0f, -0.85f, 0f);;
            // Add event handler for communicationEstablished.OnChange

        }
        protected override void InitializeDataStore()
        {
            DataStore.i.textureConfig.gltfMaxSize.Set(TextureCompressionSettingsDesktop.GLTF_TEX_MAX_SIZE_DESKTOP);
            DataStore.i.textureConfig.generalMaxSize.Set(TextureCompressionSettingsDesktop.GENERAL_TEX_MAX_SIZE_DESKTOP);
            DataStore.i.avatarConfig.useHologramAvatar.Set(true);
        }

        protected override void InitializeCommunication()
        {
            DataStore.i.debugConfig.logWs = logWs;
            Debug.Log($"Clint: OpenBrowser started");
            // TODO(Brian): Remove this branching once we finish migrating all tests out of the
            //              IntegrationTestSuite_Legacy base class.
            if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
            {
                var withSSL = true;
                int startPort = CommandLineParserUtils.startPort;

// #if UNITY_EDITOR
                withSSL = DebugConfigComponent.i.webSocketSSL;
                startPort = 7666;
//  #else
//                 withSSL = CommandLineParserUtils.withSSL;
// #endif

                int endPort = startPort + 100;
                kernelCommunication = new WebSocketCommunication(withSSL, startPort, endPort);
            }
        }

        protected override void SetupPlugins()
        {
            pluginSystem = PluginSystemFactoryDesktop.Create();
            pluginSystem.Initialize();
        }

        private void InitializeSettings()
        {
            Settings.CreateSharedInstance(new DefaultSettingsFactory()
               .WithGraphicsQualitySettingsPresetPath("DesktopGraphicsQualityPresets"));
        }

        protected override void Dispose()
        {
            SettingsDesktop.i.displaySettings.Save();

            try
            {
                DataStore.i.wsCommunication.communicationEstablished.OnChange -= OnCommunicationEstablished;

                base.Dispose();
                VRDestroy();
            }
            catch (Exception e) { Debug.LogException(e); }
        }

        private void VRDestroy()
        {
            //preloadingController.Dispose();
#if !AV_PRO_PRESENT && !UNITY_ANDROID
            DCLVideoPlayer.StopAllThreads();
#endif
        }

        void OnCommunicationEstablished(bool current, bool previous)
        {
            if (current == false && previous) { isConnectionLost = true; }
        }

        protected override void Update()
        {
            base.Update();

            if (isConnectionLost) {  Helpers.Utils.QuitApplication(); }

            // TODO: Remove this after we refactor InputController to support overrides from desktop or to use the latest Unity Input System
            // This shortcut will help some users to fix the small resolution bugs that may happen if the player prefs are manipulated
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                && Input.GetKeyDown(KeyCode.F11))
            {
                DisplaySettings newDisplaySettings = new DisplaySettings { windowMode = WindowMode.Borderless };
                SettingsDesktop.i.displaySettings.Apply(newDisplaySettings);
                SettingsDesktop.i.displaySettings.Save();
            }
        }

        protected override void SetupServices()
        {
            Environment.Setup(ServiceLocatorDesktopFactory.CreateDefault());
        }
        public void RestartSocketServer(bool current, bool previous)
        {
            if (current)
                return;
            kernelCommunication.Dispose();
            // SetupPlugins();


            InitializeCommunication();
            // DebugConfigComponent.i.ShowWebviewScreen();
            DCL.Interface.WebInterface.SendSystemInfoReport();
            // SetupServices();
            // InitializeSceneDependencies();
            // InitializeDataStore();
            // DebugConfigComponent.i.ReloadPage();
            // We trigger the Decentraland logic once everything is initialized.
            DCL.Interface.WebInterface.StartDecentraland();
#if DCL_VR
            DebugConfigComponent.i.ShowWebviewScreen();
#endif
        }
    }
}
