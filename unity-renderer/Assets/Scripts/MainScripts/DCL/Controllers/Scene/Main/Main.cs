using System;
using System.Collections.Generic;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.SettingsCommon;
using DCL.VR;
using RPC;
using UnityEngine;
using UnityEngine.Serialization;

namespace DCL
{
    /// <summary>
    /// This is the InitialScene entry point.
    /// Most of the application subsystems should be initialized from this class Awake() event.
    /// </summary>
    public class Main : MonoBehaviour
    {
        [SerializeField] private bool disableSceneDependencies;
        public static Main i { get; private set; }

        public PoolableComponentFactory componentFactory;

        private PerformanceMetricsController performanceMetricsController;
        public IKernelCommunication kernelCommunication;
        public  WebSocketCommunication webSocketCommunication;
        protected PluginSystem pluginSystem;
        private Transform mixedRealityPlayspace;
        private Transform cameraParent;
        private bool isDisposed;
        protected virtual void Awake()
        {
            UnityThread.initUnityThread(true);
            if (i != null)
            {
                Utils.SafeDestroy(this);
                return;
            }

            i = this;
            mixedRealityPlayspace = VRPlaySpace.i.transform;
            mixedRealityPlayspace.parent = cameraParent;
            mixedRealityPlayspace.localPosition = new Vector3(0f, -0.85f, 0f);;
            if (!disableSceneDependencies)
                InitializeSceneDependencies();

            Settings.CreateSharedInstance(new DefaultSettingsFactory());

            if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
            {
                performanceMetricsController = new PerformanceMetricsController();
          
                SetupServices();

                DataStore.i.HUDs.loadingHUD.visible.OnChange += OnLoadingScreenVisibleStateChange;
            }
            
#if UNITY_STANDALONE || UNITY_EDITOR
            Application.quitting += () => DataStore.i.common.isApplicationQuitting.Set(true);
#endif

            InitializeDataStore();
            SetupPlugins();
            InitializeCommunication();
        }

        protected virtual void InitializeDataStore()
        {
// <<<<<<< HEAD
// #if UNITY_ANDROID && !UNITY_EDITOR
            // DataStore.i.textureConfig.gltfMaxSize.Set(1024);
            // DataStore.i.textureConfig.generalMaxSize.Set(2048);
            // DataStore.i.avatarConfig.useHologramAvatar.Set(true); 
// #else
            // DataStore.i.textureConfig.gltfMaxSize.Set(2048);
            // DataStore.i.textureConfig.generalMaxSize.Set(2048);
// =======
            DataStore.i.textureConfig.gltfMaxSize.Set(TextureCompressionSettings.GLTF_TEX_MAX_SIZE_WEB);
            DataStore.i.textureConfig.generalMaxSize.Set(TextureCompressionSettings.GENERAL_TEX_MAX_SIZE_WEB);
// >>>>>>> upstream/dev
            DataStore.i.avatarConfig.useHologramAvatar.Set(true);
// #endif
        }

        protected virtual void InitializeCommunication()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("DCL Unity Build Version: " + DCL.Configuration.ApplicationSettings.version);
            Debug.unityLogger.logEnabled = true;
            Debug.Log($"Main: starting NativeBridgeCommunication");
            kernelCommunication = new NativeBridgeCommunication(Environment.i.world.sceneController);
#else
            // TODO(Brian): Remove this branching once we finish migrating all tests out of the
            //              IntegrationTestSuite_Legacy base class.
            // if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
            // {
            Debug.Log($"Main: starting WebSockeSSL");
            kernelCommunication = new WebSocketCommunication(DebugConfigComponent.i.webSocketSSL);
            // WebSocketCommunication kc = kernelCommunication as WebSocketCommunication;
            // WebSocketCommunication.service.OnCloseEvent += RestartSocketServer;
            // }
#endif
            RPCServerBuilder.BuildDefaultServer();
        }

        void OnLoadingScreenVisibleStateChange(bool newVisibleValue, bool previousVisibleValue)
        {
            if (newVisibleValue)
            {
                // Prewarm shader variants
                Resources.Load<ShaderVariantCollection>("ShaderVariantCollections/shaderVariants-selected").WarmUp();
                DataStore.i.HUDs.loadingHUD.visible.OnChange -= OnLoadingScreenVisibleStateChange;
            }
        }

        protected virtual void SetupPlugins()
        {
            pluginSystem = PluginSystemFactory.Create();
            pluginSystem.Initialize();
        }

        protected virtual void SetupServices()
        {
            Environment.Setup(ServiceLocatorFactory.CreateDefault());
        }

        protected virtual void Start()
        {
            // this event should be the last one to be executed after initialization
            // it is used by the kernel to signal "EngineReady" or something like that
            // to prevent race conditions like "SceneController is not an object",
            // aka sending events before unity is ready
            DCL.Interface.WebInterface.SendSystemInfoReport();

            // We trigger the Decentraland logic once everything is initialized.
            DCL.Interface.WebInterface.StartDecentraland();
        }

        protected virtual void Update()
        {
            performanceMetricsController?.Update();
        }
        
        [RuntimeInitializeOnLoadMethod]
        static void RunOnStart()
        {
            Application.wantsToQuit += ApplicationWantsToQuit;
        }
        private static bool ApplicationWantsToQuit()
        {
            if (i != null)
                i.Dispose();
    
            return true;
        }

        protected virtual void Dispose()
        {
            isDisposed = true;
            DataStore.i.HUDs.loadingHUD.visible.OnChange -= OnLoadingScreenVisibleStateChange;

            DataStore.i.common.isApplicationQuitting.Set(true);

            pluginSystem?.Dispose();

            if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
                Environment.Dispose();
            
            kernelCommunication?.Dispose();
        }
        
        protected virtual void InitializeSceneDependencies()
        {
            gameObject.AddComponent<UserProfileController>();
            gameObject.AddComponent<RenderingController>();
            gameObject.AddComponent<CatalogController>();
            gameObject.AddComponent<MinimapMetadataController>();
            //TODO: handle HUDS that need to be converted to VR
            gameObject.AddComponent<ChatController>();
            //TODO: handle HUDS that need to be converted to VR
            gameObject.AddComponent<FriendsController>();
            
            gameObject.AddComponent<HotScenesController>();
            gameObject.AddComponent<GIFProcessingBridge>();
            gameObject.AddComponent<RenderProfileBridge>();
            gameObject.AddComponent<AssetCatalogBridge>();
            //gameObject.AddComponent<ScreenSizeWatcher>();//Test VR not needed.
            gameObject.AddComponent<SceneControllerBridge>();

            MainSceneFactory.CreateBuilderInWorldBridge(gameObject);
            MainSceneFactory.CreateBridges();
            //TODO: handle HUDS that need to be converted to VR
            //MainSceneFactory.CreateMouseCatcher();
            MainSceneFactory.CreatePlayerSystems();
            CreateEnvironment();
            MainSceneFactory.CreateAudioHandler();
            MainSceneFactory.CreateHudController();
            MainSceneFactory.CreateNavMap();
            MainSceneFactory.CreateEventSystem();
        }

        protected virtual void CreateEnvironment() => MainSceneFactory.CreateEnvironment();
        public void RestartSocketServer()
        {
             if (isDisposed)
                            return;
            //DebugConfigComponent.i.ReloadPage();
            //kernelCommunication.Dispose();
            //InitializeCommunication();
            DebugConfigComponent.i.ShowWebviewScreen();
        }
    }
}