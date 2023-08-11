using DCL.Components;

using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Utils = DCL.Helpers.Utils;

//VR additions
using TMPro;
using DCL.Interface;
using DCL.SettingsCommon;
using System.Collections.Generic;
using UnityEngine.UI;
using Vuplex.WebView;

using QualitySettings = UnityEngine.QualitySettings;
//end VR additions
namespace DCL
{
    public class DebugConfigComponent : MonoBehaviour
    {
        private Stopwatch loadingStopwatch;
        private static DebugConfigComponent sharedInstance;
//VR
 		[SerializeField] private GameObject startMenu;
 		[SerializeField] private GameObject startSceneObjects;
        [SerializeField] private GameObject browserOptionsButton;

        [SerializeField] private TMP_Text popupMessage;
        [SerializeField] private GameObject popupMessageObj;
        [SerializeField] private CanvasWebViewPrefab DCLWebview;
        [SerializeField] private Button reload;
        [SerializeField] private Button swapTabs;
        [SerializeField] private Toggle useInternalBrowser;
        private string webViewURL = "";
        private bool isMainTab = true;
		public bool openInternalBrowser;
        private GeneralSettings currentSettings;
        //end VR
        private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;

        public static DebugConfigComponent i
        {
            get
            {
                if (sharedInstance == null)
                    sharedInstance = FindObjectOfType<DebugConfigComponent>();

                return sharedInstance;
            }

            private set => sharedInstance = value;
        }

        public DebugConfig debugConfig;

        public enum DebugPanel
        {
            Off,
            Scene,
            Engine
        }

        public enum BaseUrl
        {
            ZONE,
            ORG,
            LOCAL_HOST,
            CUSTOM,
        }

        public enum Network
        {
            MAINNET,
            SEPOLIA,
        }

        [Header("General Settings")] public bool OpenBrowserOnStart = true;
        public bool webSocketSSL = false;

        [Header("Kernel General Settings")] public string kernelVersion;
        public bool useCustomContentServer = false;

        public string customContentServerUrl = "http://localhost:1338/";

        [Space(10)] public BaseUrl baseUrlMode = BaseUrl.ZONE;
        [DrawIf("baseUrlMode", BaseUrl.CUSTOM)]
        public string customURL = "https://play.decentraland.zone/?";

        [Space(10)] public Network network;

        [Tooltip(
            "Set this field to force the realm (server). On the latin-american zone, recommended realms are fenrir-amber, baldr-amber and thor. Other realms can give problems to debug from Unity editor due to request certificate issues.\n\nFor auto selection leave this field blank.\n\nCheck out all the realms at https://catalyst-monitor.vercel.app/?includeDevServers")]
        public string realm;

        public Vector2 startInCoords = new Vector2(-99, 109);

        [Tooltip("Set this value to load the catalog from another wallet for debug purposes")]
        public string overrideUserID = "";

        [Header("Kernel Misc Settings")] public bool forceLocalComms = true;

        public bool enableTutorial = false;
        public bool builderInWorld = false;
        public bool soloScene = true;
        public bool disableAssetBundles = false;
        public bool enableDebugMode = false;
        public DebugPanel debugPanelMode = DebugPanel.Off;

        [Header("Performance")]
        public bool disableGLTFDownloadThrottle = false;
        public bool multithreaded = false;
        public bool runPerformanceMeterToolDuringLoading = false;
        private PerformanceMeterController performanceMeterController;

        private void Awake()
        {

		#if DCL_VR

#if UNITY_ANDROID && !UNITY_EDITOR
            useInternalBrowser.transform.parent.gameObject.SetActive((false));
#else
            browserOptionsButton.SetActive(false);
#endif
#endif
            if (sharedInstance == null)
                sharedInstance = this;
            ShowWebviewScreen();
            DataStore.i.debugConfig.soloScene = debugConfig.soloScene;
            DataStore.i.debugConfig.soloSceneCoords = debugConfig.soloSceneCoords;
            DataStore.i.debugConfig.ignoreGlobalScenes = debugConfig.ignoreGlobalScenes;
            DataStore.i.debugConfig.msgStepByStep = debugConfig.msgStepByStep;
            DataStore.i.debugConfig.overrideUserID = overrideUserID;
            DataStore.i.performance.multithreading.Set(multithreaded);
            if (disableGLTFDownloadThrottle) DataStore.i.performance.maxDownloads.Set(999);
            Texture.allowThreadedTextureCreation = multithreaded;
#if DCL_VR
            useInternalBrowser.onValueChanged.AddListener(UpdateBrowserType);
            // useInternalBrowser.isOn = currentSettings.useInternalBrowser;
#if (UNITY_EDITOR  || UNITY_STANDALONE)
            StandaloneWebView.GloballySetUserAgent("Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
            StandaloneWebView.SetIgnoreCertificateErrors(true);
            StandaloneWebView.GloballySetUserAgent(false);
            StandaloneWebView.SetCameraAndMicrophoneEnabled(true);
            StandaloneWebView.SetAutoplayEnabled(true);
            StandaloneWebView.SetTargetFrameRate(72);
            StandaloneWebView.SetCommandLineArguments("--disable-web-security");

#elif UNITY_ANDROID
            AndroidGeckoWebView.GloballySetUserAgent("Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
            Web.SetUserAgent("Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
            Web.SetStorageEnabled(true);
            Web.SetIgnoreCertificateErrors(true);
            Web.SetAutoplayEnabled(true);
            AndroidGeckoWebView.SetIgnoreCertificateErrors(true);
            AndroidGeckoWebView.SetCameraAndMicrophoneEnabled(true);
            AndroidGeckoWebView.SetAutoplayEnabled(true);
            AndroidGeckoWebView.SetStorageEnabled(true);
            AndroidGeckoWebView.SetEnterpriseRootsEnabled(true);
            AndroidGeckoWebView.SetDrmEnabled(true);
            AndroidGeckoWebView.SetPreferences(new Dictionary<string, string> {
                ["network.websocket.allowInsecureFromHTTPS"] = "true",
                ["dom.security.https_only_check_path_upgrade_downgrade_endless_loop"] = "false",
                ["dom.security.https_only_mode_break_upgrade_downgrade_endless_loop"] = "false",
                ["security.csp.enable"] = "false",
                ["dom.webnotifications.allowcrossoriginiframe"] = "true",
                ["dom.webnotifications.allowinsecure"] = "true",
                ["network.auth.subresource-img-cross-origin-http-auth-allow"] = "true",
                ["network.http.referer.XOriginPolicy"] = "1"
            });

#endif
#endif
        }

        private void Start()
        {
#if DCL_VR
            string existingValue = VRSettingsManager.I.GetSetting("openInternalBrowser");

            if (existingValue == null || existingValue == "")
            {
                VRSettingsManager.I.SetSetting("openInternalBrowser", openInternalBrowser.ToString());
            }
            else
            {
                openInternalBrowser = bool.Parse(existingValue);
            }
            useInternalBrowser.isOn = openInternalBrowser;
			if (!Debug.isDebugBuild)
            {
                startMenu.gameObject.SetActive(true);



#if UNITY_ANDROID && !UNITY_EDITOR
//don't have a method of using external browser on quest2.
                openInternalBrowser = true;
                useInternalBrowser.transform.parent.gameObject.SetActive(false);

                // CommonScriptableObjects.useInternalBrowser.Set(true);
                webSocketSSL = false;
                baseUrlMode = BaseUrl.CUSTOM;



#else
                useInternalBrowser.transform.parent.gameObject.SetActive(true);
                webSocketSSL = true;
                baseUrlMode = BaseUrl.ORG;

#endif

            }

            if (openInternalBrowser)
            {

                DCLWebview.gameObject.SetActive((true));
            }
            else
            {
                DCLWebview.gameObject.SetActive((false));

            }

#endif
            lock (DataStore.i.wsCommunication.communicationReady)
            {
                if (DataStore.i.wsCommunication.communicationReady.Get()) { InitConfig(); }
                else { DataStore.i.wsCommunication.communicationReady.OnChange += OnCommunicationReadyChangedValue; }
            }
        }

        private void UpdateBrowserType(bool current)
        {
            openInternalBrowser = current;
            VRSettingsManager.I.SetSetting("openInternalBrowser",openInternalBrowser.ToString());
        }
        private void OnCommunicationReadyChangedValue(bool newState, bool prevState)
        {
            Debug.Log("todo: Clint: before Init after OnCommunicationReadyChangedValue.");

            if (newState && !prevState)
            {
                InitConfig();
                Debug.Log("todo: Clint: before Init after OnCommunicationReadyChangedValue.");
            }

            DataStore.i.wsCommunication.communicationReady.OnChange -= OnCommunicationReadyChangedValue;
        }

        private void InitConfig()
        {
            Debug.Log($"Todo: Clint: InitConfig started");
            if (useCustomContentServer)
            {
                RendereableAssetLoadHelper.useCustomContentServerUrl = true;
                RendereableAssetLoadHelper.customContentServerUrl = customContentServerUrl;
            }

            if (OpenBrowserOnStart)
            {
                OpenWebBrowser();
                Debug.Log($"Clint: InitConfig started. Open Browser");
            }

            if (runPerformanceMeterToolDuringLoading)
            {
                CommonScriptableObjects.forcePerformanceMeter.Set(true);
                performanceMeterController = new PerformanceMeterController();

                StartSampling();
                CommonScriptableObjects.rendererState.OnChange += EndSampling;
            }
        }

        private void StartSampling()
        {
            loadingStopwatch = new Stopwatch();
            loadingStopwatch.Start();
            performanceMeterController.StartSampling(999);
        }

        private void EndSampling(bool current, bool previous)
        {
            if (current)
            {
                loadingStopwatch.Stop();
                CommonScriptableObjects.rendererState.OnChange -= EndSampling;
                performanceMeterController.StopSampling();
                Debug.Log($"Loading time: {loadingStopwatch.Elapsed.Seconds} seconds");
            }
        }

        public void OpenWebBrowser()
        {

            string existingValue = VRSettingsManager.I.GetSetting("openInternalBrowser");

            if (existingValue == null || existingValue == "")
            {
                VRSettingsManager.I.SetSetting("openInternalBrowser", openInternalBrowser.ToString());
            }
            else
            {
                openInternalBrowser = bool.Parse(existingValue);
            }
            useInternalBrowser.isOn = openInternalBrowser;

            Debug.Log($"Clint: OpenBrowser started");
            string baseUrl = "";
            string debugString = "";

            if (baseUrlMode.Equals(BaseUrl.CUSTOM))
            {
                baseUrl = this.customURL;
                if (string.IsNullOrEmpty(this.customURL))
                {
                    Debug.LogError("Custom url cannot be empty");
                    QuitGame();
                    return;
                }
            }
            else if (baseUrlMode.Equals(BaseUrl.LOCAL_HOST))
            {
                baseUrl = "http://localhost:8080/?";
            }
            else if (baseUrlMode.Equals(BaseUrl.ORG))
            {
                baseUrl = "http://play.decentraland.org/?";
                if (!webSocketSSL)
                {
                    Debug.LogError(
                        "play.decentraland.org only works with WebSocket SSL, please change the base URL to play.decentraland.zone");
                    QuitGame();
                    return;
                }
            }
            else
            {
                baseUrl = "http://play.decentraland.zone/?";
            }
            #if DCL_VR
            #if UNITY_STANDALONE || UNITY_EDITOR
                baseUrl += "dcl_renderer_type=vr_desktop&";
            #else
                baseUrl += "dcl_renderer_type=vr_android&";
            #endif
            #endif
            switch (network)
            {
                case Network.SEPOLIA:
                    debugString = "NETWORK=sepolia&";
                    break;
                case Network.MAINNET:
                    debugString = "NETWORK=mainnet&";
                    break;
            }

            if (!string.IsNullOrEmpty(kernelVersion))
            {
                debugString += $"kernel-version={kernelVersion}&";
            }

            if (forceLocalComms)
            {
                debugString += "LOCAL_COMMS&";
            }

            if (enableTutorial)
            {
                debugString += "RESET_TUTORIAL&";
            }

            if (soloScene)
            {
                debugString += "LOS=0&";
            }

            if (builderInWorld)
            {
                debugString += "ENABLE_BUILDER_IN_WORLD&";
            }

            if (disableAssetBundles)
            {
                debugString += "DISABLE_ASSET_BUNDLES&DISABLE_WEARABLE_ASSET_BUNDLES&";
            }

            if (enableDebugMode)
            {
                debugString += "DEBUG_MODE&";
            }

            if (!string.IsNullOrEmpty(realm))
            {
                debugString += $"realm={realm}&";
            }
            string debugPanelString = "";

            if (debugPanelMode == DebugPanel.Engine)
            {
                debugPanelString = "ENGINE_DEBUG_PANEL&";
            }
            else if (debugPanelMode == DebugPanel.Scene)
            {
                debugPanelString = "SCENE_DEBUG_PANEL&";
            }

            if (webSocketSSL)
            {
                Debug.Log(
                    "[REMINDER] To be able to connect with SSL you should start Chrome with the --ignore-certificate-errors argument specified (or enabling the following option: chrome://flags/#allow-insecure-localhost). In Firefox set the configuration option `network.websocket.allowInsecureFromHTTPS` to true, then use the ws:// rather than the wss:// address.");
            }
            Debug.Log($"URL: {baseUrl}{debugString}{debugPanelString}position={startInCoords.x}%2C{startInCoords.y}&ws={DataStore.i.wsCommunication.url}");
            #if DCL_VR
			 webViewURL =  $"{baseUrl}{debugString}{debugPanelString}position={startInCoords.x}%2C{startInCoords.y}&ws={DataStore.i.wsCommunication.url}";

             var canvas = GameObject.Find("Canvas");
#if UNITY_ANDROID && !UNITY_EDITOR
            //don't have a method of using external browser on quest2.
            openInternalBrowser = true;
#endif
            Debug.Log($"Clint: OpenBrowser Middle: internal - {openInternalBrowser}");
            if (openInternalBrowser)
            {
               // browserMessage.text = "Browser Loading";
                WebViewOptions opt = new WebViewOptions();
#if ( UNITY_EDITOR || UNITY_STANDALONE)
                opt.preferredPlugins  = new WebPluginType[] { WebPluginType.AndroidGecko, WebPluginType.iOS, WebPluginType.Windows, WebPluginType.UniversalWindowsPlatform };
#elif UNITY_ANDROID
            opt.preferredPlugins  = new WebPluginType[] { WebPluginType.AndroidGecko};
#endif
                DCLWebview.InitialUrl = webViewURL;
                if (DCLWebview.WebView!= null && DCLWebview.WebView.IsInitialized) {


                    DCLWebview.gameObject.SetActive((true));
                    Debug.Log($"main webview loading {webViewURL}");
                    DCLWebview.WebView.LoadUrl(webViewURL);
                    DCLWebview.WebView.LoadProgressChanged += ( sender,  args) => {
                        //browserMessage.transform.parent.gameObject.SetActive((false));
                        Debug.Log($"WebView Status LoadProgressChanged {args.Type.ToString()}, {sender.ToString()}");
                    };
                    DCLWebview.WebView.PageLoadFailed += ( sender,  args) => { Debug.Log($"WebView Status PageLoadFailed {args.ToString()}, {sender.ToString()}"); };
                    DCLWebview.WebView.CloseRequested += ( sender,  args) => { Debug.Log($"WebView Status CloseRequested {args.ToString()}, {sender.ToString()}"); };}
                else
                {
                    DCLWebview.Initialized += (sender, eventArgs) =>
                    {
                        ShowWebviewScreen();
                        DCLWebview.gameObject.SetActive((true));
                        Debug.Log($"main webview loading {webViewURL}");
                        DCLWebview.WebView.LoadUrl(webViewURL);
                        DCLWebview.WebView.LoadProgressChanged += ( sender,  args) => {
                            //browserMessage.transform.parent.gameObject.SetActive((false));
                            Debug.Log($"WebView Status LoadProgressChanged {args.Type.ToString()}, {sender.ToString()}");
                        };
                        DCLWebview.WebView.PageLoadFailed += ( sender,  args) => { Debug.Log($"WebView Status PageLoadFailed {args.ToString()}, {sender.ToString()}"); };
                        DCLWebview.WebView.CloseRequested += ( sender,  args) => { Debug.Log($"WebView Status CloseRequested {args.ToString()}, {sender.ToString()}"); };
                    };
                }
                DCLWebview.Resolution = 450;
            }
            else
            {
                //browserMessage.text = "Use External Browser";
                Application.OpenURL(webViewURL);
            }
            //#endif
			#else



			Application.OpenURL(
                $"{baseUrl}{debugString}{debugPanelString}position={startInCoords.x}%2C{startInCoords.y}&ws={DataStore.i.wsCommunication.url}");
#endif

        }
#if DCL_VR
//         private void _positionPrefabs() {
// #if (UNITY_ANDROID || UNITY_STANDALONE || UNITY_EDITOR)
//             var rectTransform = DCLWebview.transform as RectTransform;
//
//
//             //rectTransform.anchoredPosition3D = Vector3.zero;
//             rectTransform.offsetMin = new Vector2(-0.191f, -0.25f);
//             rectTransform.offsetMax = new Vector2(-0.809f, -0.75f);
//             rectTransform.pivot = new Vector2(0.5f, 1);
//             rectTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
//
// #endif
//         }
        public  void HideWebViewScreens()
        {
            Debug.Log("WebView Connected, hiding web browsers.");
            // _canvasWebViewPrefab.Visible = false;
            //DCLWebview.transform.localPosition += new Vector3(0, 1, 0);
            //_keyboard.WebViewPrefab.transform.localPosition -= new Vector3(0, 10, 0);
            startMenu.SetActive((false));
             Destroy(startSceneObjects);
            //keyboardOptions.gameObject.SetActive(false);
            //keyboardDCL.gameObject.SetActive((false));
            //optionsWeview.gameObject.SetActive(false);
            reload.gameObject.SetActive((false));
            swapTabs.gameObject.SetActive((false));
            DCLWebview.gameObject.SetActive(false);


        }
        public void ShowWebviewScreen()
        {
            startMenu.SetActive((true));
            // OpenWebBrowser();
            // popupMessageObj.SetActive(true);
            // popupMessage.text = "Network Communication Lost.\r\nRestart Application.\r\n Reduced Loading Radius Recommended In This Area";
            // ReloadPage();

            //DCLWebview.gameObject.SetActive(true);
            //keyboardDCL.gameObject.SetActive((true));
            //urlInput.gameObject.SetActive(false);
            //reload.gameObject.SetActive((true));


        }
        public void SwapBrowserTabs()
        {
            if (isMainTab)
            {
                DCLWebview.gameObject.SetActive((false));
                //keyboardDCL.gameObject.SetActive(false);
                //optionsWeview.gameObject.SetActive((true));
                //keyboardOptions.gameObject.SetActive(true);
                isMainTab = false;
            }
            else
            {
                DCLWebview.gameObject.SetActive((true));
                //keyboardDCL.gameObject.SetActive(true);
                //optionsWeview.gameObject.SetActive((false));
                //keyboardOptions.gameObject.SetActive(false);
                isMainTab = true;
            }

        }
        public void ReloadPage()
        {
            //TODO: ensure websocket is restarted and listening, set start location to current parcel, ensure startMenu is open, reload url. Use to correct dropped connections in future.
            //Set

            //if(_canvasWebViewPrefab!=null) _canvasWebViewPrefab.Destroy();
            //if(_keyboard!= null)  Destroy(_keyboard.gameObject);
            // if(openInternalBrowser)
            //     DCLWebview.WebView.Reload();
            // else
            Start();
            OpenWebBrowser();

        }
        public void PauseWebview()
        {
            //DCLWebview.WebView.Dispose();
        }


        public void ToggleUseInternalBrowser()
        {

            openInternalBrowser = useInternalBrowser.isOn;
            // WebInterface.openURLInternal = openInternalBrowser;
            // if (useInternalBrowser.isOn)
            // {
            //     DCLWebview.gameObject.SetActive(true);
            //     keyboardDCL.gameObject.SetActive(true);
            //     optionsWeview.gameObject.SetActive(false);
            //     keyboardOptions.gameObject.SetActive(false);
            //     OpenWebBrowser();
            // }
            // else
            // {
            //     if (DCLWebview != null && DCLWebview.WebView != null)
            //     {
            //         DCLWebview.WebView.LoadUrl("https://www.google.com/");
            //         //DCLWebview.WebView.StopLoad();
            //         //DCLWebview.WebView.Dispose();
            //         DCLWebview.gameObject.SetActive((false));
            //     }
            //     DCLWebview.gameObject.SetActive(false);
            //     keyboardDCL.gameObject.SetActive(false);
            //     optionsWeview.gameObject.SetActive(false);
            //     keyboardOptions.gameObject.SetActive(false);
            //     OpenWebBrowser();
            // }

        }
#endif

        private void OnDestroy()
        {
            DataStore.i.wsCommunication.communicationReady.OnChange -= OnCommunicationReadyChangedValue;
        }

        private void QuitGame()
        {
            Utils.QuitApplication();
        }
    }
}
