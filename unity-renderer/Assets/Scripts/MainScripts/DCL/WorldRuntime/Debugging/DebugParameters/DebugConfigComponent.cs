using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using DCL.Components;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using TMPro;
using UnityEngine.UI;

#if UNITY_ANDROID
using Vuplex.WebView;

#endif
namespace DCL
{
    public class DebugConfigComponent : MonoBehaviour
    {
        private static DebugConfigComponent sharedInstance;
//#if (UNITY_ANDROID || UNITY_STANDALONE)
        [SerializeField] private CanvasWebViewPrefab DCLWebview;
        [SerializeField] private CanvasKeyboard keyboardDCL;
        [SerializeField] private CanvasWebViewPrefab optionsWeview;
        [SerializeField] public CanvasKeyboard keyboardOptions;
        [SerializeField] private TMP_InputField urlInput;
        [SerializeField] private Button reload;
        [SerializeField] private Button swapTabs; 
        private string webViewURL = "";
        private bool isMainTab = true;
        
        //public NonNativeKeyboard keyboard;
//#endif
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
            LOCAL_HOST,
            CUSTOM,
        }

        public enum Environment
        {
            USE_DEFAULT_FROM_URL,
            LOCAL,
            ZONE,
            TODAY,
            ORG
        }

        private const string ENGINE_DEBUG_PANEL = "ENGINE_DEBUG_PANEL";
        private const string SCENE_DEBUG_PANEL = "SCENE_DEBUG_PANEL";

        [Header("General Settings")]
        public bool openBrowserWhenStart;

        


        public bool webSocketSSL = false;

        [Header("Kernel General Settings")] public string kernelVersion;
        public bool useCustomContentServer = false;

        public string customContentServerUrl = "http://localhost:1338/";

        [Space(10)] public BaseUrl baseUrlMode;

        public string baseUrlCustom;

        [Space(10)] public Environment environment;

        [Tooltip(
            "Set this field to force the realm (server). On the latin-american zone, recommended realms are fenrir-amber, baldr-amber and thor. Other realms can give problems to debug from Unity editor due to request certificate issues.\n\nFor auto selection leave this field blank.\n\nCheck out all the realms at https://catalyst-monitor.vercel.app/?includeDevServers")]
        public string realm;

        public Vector2 startInCoords = new Vector2(-99, 109);

        [Header("Kernel Misc Settings")] public bool forceLocalComms = true;

        public bool allWearables = false;
        public bool testWearables = false;
        public bool enableTutorial = false;
        public bool builderInWorld = false;
        //public bool soloScene = true;
        public int parcelRadiusToLoad = 4;
        public bool disableAssetBundles = true;
        public bool multithreaded = false;
        public DebugPanel debugPanelMode = DebugPanel.Off;

        private void Awake()
        {
            if (sharedInstance == null)
                sharedInstance = this;

            DataStore.i.debugConfig.soloScene = debugConfig.soloScene;
            DataStore.i.debugConfig.soloSceneCoords = debugConfig.soloSceneCoords;
            DataStore.i.debugConfig.ignoreGlobalScenes = debugConfig.ignoreGlobalScenes;
            DataStore.i.debugConfig.msgStepByStep = debugConfig.msgStepByStep;
            DataStore.i.performance.multithreading.Set(multithreaded);
            Texture.allowThreadedTextureCreation = multithreaded;
            // options.Initialized += (sender, eventArgs) =>
            // {
            //     Debug.Log($"Secondary Webview loading {htmlServerTest}");
            //     options.WebView.LoadHtml(htmlServerTest);
            // };
           
#if (UNITY_EDITOR  || UNITY_STANDALONE)
            StandaloneWebView.GloballySetUserAgent("Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
            StandaloneWebView.SetIgnoreCertificateErrors(true);
            StandaloneWebView.GloballySetUserAgent(false);
            StandaloneWebView.SetCameraAndMicrophoneEnabled(true);
            optionsWeview.gameObject.SetActive(false);
            keyboardOptions.gameObject.SetActive(false);
#elif UNITY_ANDROID
            AndroidGeckoWebView.GloballySetUserAgent("Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
            Web.SetUserAgent("Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
            Web.SetStorageEnabled(true); 
            Web.SetIgnoreCertificateErrors(true);
            // Web.EnableRemoteDebugging();
            Web.SetAutoplayEnabled(true);
            AndroidGeckoWebView.SetIgnoreCertificateErrors(true);
            // AndroidGeckoWebView.GloballySetUserAgent(false);
            AndroidGeckoWebView.SetCameraAndMicrophoneEnabled(true);

            AndroidGeckoWebView.SetAutoplayEnabled(true);
            AndroidGeckoWebView.SetStorageEnabled(true);
            AndroidGeckoWebView.SetEnterpriseRootsEnabled(true);
            AndroidGeckoWebView.SetDrmEnabled(true);
            AndroidGeckoWebView.SetPreferences(new Dictionary<string, string> {
                ["network.websocket.allowInsecureFromHTTPS"] = "true",
                ["dom.security.https_only_check_path_upgrade_downgrade_endless_loop"] = "false",
                ["dom.security.https_only_mode_break_upgrade_downgrade_endless_loop"] = "false",
                ["security.csp.enable"] = "false"
                // ["dom.security.https_only_mode_send_http_background_request"] = "false",
                // ["dom.webnotifications.allowcrossoriginiframe"] = "true",
                // ["dom.webnotifications.allowinsecure"] = "true",
                // ["security.allow_unsafe_parent_loads"] = "true",
                // ["network.auth.subresource-img-cross-origin-http-auth-allow"] = "true",
                // //["network.dns.echconfig.fallback_to_origin_when_all_failed"] = "false",
                //
                // ["security.fileuri.strict_origin_policy"] = "false",
                // ["dom.cross_origin_iframes_loaded_in_background"] = "true",
                // ["security.csp.enableNavigateTo"] = "true",
                // ["security.mixed_content.block_active_content"] = "	false",
                // ["security.insecure_field_warning.ignore_local_ip_address"] = "false",
                // ["security.mixed_content.upgrade_display_content"] = "false",
                // ["network.websocket.auto-follow-http-redirects"] = "true",
                // ["network.http.referer.XOriginPolicy"] = "1",
                
            });
#endif

        }

        private void Start()
        {
            
            DCLWebview.gameObject.SetActive((true));
            optionsWeview.gameObject.SetActive(false);
            keyboardOptions.gameObject.SetActive(false);
            keyboardDCL.gameObject.SetActive(true);
            keyboardOptions.InputReceived += (sender, key) =>
            {
                optionsWeview.WebView.HandleKeyboardInput(key.Value);
            };
            keyboardDCL.InputReceived += (sender, key) =>
            {
                DCLWebview.WebView.HandleKeyboardInput(key.Value);
            };
        
            lock (DataStore.i.wsCommunication.communicationReady)
            {
                if (DataStore.i.wsCommunication.communicationReady.Get())
                {
                    Debug.Log("Debug Config Init");
                    InitConfig();
                }
                else
                {
                    Debug.Log("Debug Config starting listener for OnCommunicationReadyChangedValue");
                    DataStore.i.wsCommunication.communicationReady.OnChange += OnCommunicationReadyChangedValue;
                }
            }
        }

        // private void Update()
        // {
        //     transform.position = Camera.main.transform.position + new Vector3(0, 0.2f, 0);
        // }

        private void OnCommunicationReadyChangedValue(bool newState, bool prevState)
        {
            Debug.Log($"DebugConfig OnCommunicationReadyChangedValue {newState}");
            if (newState && !prevState)
                InitConfig();
            
            
            DataStore.i.wsCommunication.communicationReady.OnChange -= OnCommunicationReadyChangedValue;
        }

        private void InitConfig()
        {
            if (useCustomContentServer)
            {
                RendereableAssetLoadHelper.useCustomContentServerUrl = true;
                RendereableAssetLoadHelper.customContentServerUrl = customContentServerUrl;
            }

            if (openBrowserWhenStart)
                OpenWebBrowser();
        }

        private void OpenWebBrowser()
        {
            string baseUrl = "";
            string debugString = "";
            string debugPanelString = "";

            

             if (baseUrlMode == BaseUrl.CUSTOM)
                 baseUrl = baseUrlCustom;
             else
                 baseUrl = "http://localhost:3000/?";

             switch (environment)
             {
                 case Environment.USE_DEFAULT_FROM_URL:
                     break;
                 case Environment.LOCAL:
                     debugString = "DEBUG_MODE&";
                     break;
                 case Environment.ZONE:
                     debugString = "NETWORK=ropsten&";
                     break;
                 case Environment.TODAY:
                     debugString = "NETWORK=mainnet&";
                     break;
                 case Environment.ORG:
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

             if (allWearables)
             {
                 debugString += "ALL_WEARABLES&";
             }

             if (testWearables)
             {
                 debugString += "TEST_WEARABLES&";
             }

             if (enableTutorial)
             {
                 debugString += "RESET_TUTORIAL&";
             }

             if (parcelRadiusToLoad != 4)
             {
                 debugString += $"LOS={parcelRadiusToLoad}&";
             }
            if (disableAssetBundles)
            {
                debugString += "DISABLE_ASSET_BUNDLES&DISABLE_WEARABLE_ASSET_BUNDLES&";
            }

             if (builderInWorld)
             {
                 debugString += "ENABLE_BUILDER_IN_WORLD&";
             }

             if (!string.IsNullOrEmpty(realm))
             {
                 debugString += $"realm={realm}&";
             }

             

             if (debugPanelMode == DebugPanel.Engine)
             {
                 debugPanelString = ENGINE_DEBUG_PANEL + "&";
             }
             else if (debugPanelMode == DebugPanel.Scene)
             {
                 debugPanelString = SCENE_DEBUG_PANEL + "&";
             }

             if (!webSocketSSL)
             {
                 if (baseUrl.Contains("play.decentraland.org"))
                 {
                     Debug.LogError("play.decentraland.org only works with WebSocket SSL, please change the base URL to play.decentraland.zone");
                     QuitGame();
                     return;
                 }
             }
             else
             {
                 Debug.Log("[REMINDER] To be able to connect with SSL you should start Chrome with the --ignore-certificate-errors argument specified (or enabling the following option: chrome://flags/#allow-insecure-localhost). In Firefox set the configuration option `network.websocket.allowInsecureFromHTTPS` to true, then use the ws:// rather than the wss:// address.");                
             }


            webViewURL = $"{baseUrl}{debugString}{debugPanelString}position={startInCoords.x}%2C{startInCoords.y}&ws={DataStore.i.wsCommunication.url}";
            urlInput.text = webViewURL;
            var canvas = GameObject.Find("Canvas");
            //DontDestroyOnLoad(canvas);
            WebViewOptions opt = new WebViewOptions();
#if ( UNITY_EDITOR || UNITY_STANDALONE)
            opt.preferredPlugins  = new WebPluginType[] { WebPluginType.AndroidGecko,WebPluginType.iOS, WebPluginType.Windows, WebPluginType.UniversalWindowsPlatform};
#elif UNITY_ANDROID
            opt.preferredPlugins  = new WebPluginType[] { WebPluginType.AndroidGecko};
#endif
            
            DCLWebview.gameObject.SetActive(true);
            DCLWebview.InitialUrl = webViewURL;
            DCLWebview.Initialized += (sender, eventArgs) => {
                    Debug.Log($"main webview loading {webViewURL}");
                    DCLWebview.WebView.LoadUrl(webViewURL);
                };

            
            DCLWebview.transform.SetParent(canvas.transform, false);
            DCLWebview.InitialResolution = 350;

            DCLWebview.RemoteDebuggingEnabled = false;
            DCLWebview.LogConsoleMessages = false;
            DCLWebview.NativeOnScreenKeyboardEnabled = false;
            DCLWebview.Native2DModeEnabled = false;

            // Create a CanvasKeyboard
            // https://developer.vuplex.com/webview/CanvasKeyboard
            // _keyboard = CanvasKeyboard.Instantiate();
            // _keyboard.InitialResolution = 350;
            // _keyboard.transform.SetParent(canvas.transform, false);
            // Hook up the keyboard so that characters are routed to the CanvasWebViewPrefab.
            // _keyboard.InputReceived += (sender, eventArgs) => {
            //     _canvasWebViewPrefab.WebView.HandleKeyboardInput(eventArgs.Value);
            //     //Web.SetUserAgent(false);
            // };

           
            urlInput.keyboardType = TouchScreenKeyboardType.URL;
            urlInput.contentType = TMP_InputField.ContentType.Alphanumeric;
            
        
            Debug.Log("Created WebView objects");
            //_positionPrefabs();
            Debug.Log("finished positioning webview objects");
             // Application.OpenURL(
             //     $"{baseUrl}{debugString}{debugPanelString}position={startInCoords.x}%2C{startInCoords.y}&ws={DataStore.i.wsCommunication.url}")
             //#endif

        }
        private void _positionPrefabs() {
        #if (UNITY_ANDROID || UNITY_STANDALONE || UNITY_EDITOR)
            var rectTransform = DCLWebview.transform as RectTransform;
           
            
            //rectTransform.anchoredPosition3D = Vector3.zero;
            rectTransform.  offsetMin = new Vector2(-0.191f, -0.25f);
            rectTransform.offsetMax = new Vector2(-0.809f, -0.75f);
            rectTransform.pivot = new Vector2(0.5f, 1);
            //rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 520/150);
            //rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 520/150);

            rectTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
            // var keyboardTransform = _keyboard.transform as RectTransform;
            // keyboardTransform.anchoredPosition3D = Vector3.zero;
            // keyboardTransform.offsetMin = new Vector2(0.5f, -1.8f);
            // keyboardTransform.offsetMax = new Vector2(0.5f, -0.3f);
            // keyboardTransform.pivot = new Vector2(0.5f, 0);
            // keyboardTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 690/150);
            // keyboardTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 162/150);
#endif
        }

        
        public  void HideWebViewScreens()
        {
            Debug.Log("WebView Connected, hiding web browsers.");
            // _canvasWebViewPrefab.Visible = false;
             DCLWebview.transform.localPosition -= new Vector3(0, 10, 0);
            //_keyboard.WebViewPrefab.transform.localPosition -= new Vector3(0, 10, 0);
            keyboardOptions.gameObject.SetActive(false);
            keyboardDCL.gameObject.SetActive((false));
            optionsWeview.gameObject.SetActive(false);
             urlInput.gameObject.SetActive(false);
            reload.gameObject.SetActive((false));
            swapTabs.gameObject.SetActive((false));
            DCLWebview.gameObject.SetActive(false);
            //_keyboard.gameObject.SetActive(false);

        }
        public void SwapBrowserTabs()
        {
            if (isMainTab)
            {
                DCLWebview.gameObject.SetActive((false));
                keyboardDCL.gameObject.SetActive(false);
                optionsWeview.gameObject.SetActive((true));
                keyboardOptions.gameObject.SetActive(true);
                isMainTab = false;
            }
            else
            {
                DCLWebview.gameObject.SetActive((true));
                keyboardDCL.gameObject.SetActive(true);
                optionsWeview.gameObject.SetActive((false));
                keyboardOptions.gameObject.SetActive(false);
                isMainTab = true;
            }
                
        }
        // private void MainScreenKeyboard(object sender, EventArgs<string> e)
        // {
        //     DCLWebview.WebView.SendKey(e.Value);
        // }
        // private void OptionsScreenKeyboard(object sender, EventArgs<string> e)
        // {
        //     optionsWeview.WebView.SendKey(e.Value);
        // }

        public void ReloadPage()
        {
            //if(_canvasWebViewPrefab!=null) _canvasWebViewPrefab.Destroy();
            //if(_keyboard!= null)  Destroy(_keyboard.gameObject);
;           DCLWebview.WebView.Reload();
            OpenWebBrowser();

        }
    
       
        private void OnDestroy()
        {
            DataStore.i.wsCommunication.communicationReady.OnChange -= OnCommunicationReadyChangedValue;
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
   // private const string htmlServerTest = @"<!DOCTYPE HTML>
   // <html>
   // <head>
   // <script type = ""text/javascript"">
   //                function WebSocketTest() {
   //     if (""WebSocket"" in window) {
   //         document.getElementById(""sse"").innerHTML += ""\r\n  WebSocket is supported by your Browser!"";    
   //         // Let us open a web socket
   //         var ws = new WebSocket(""ws://localhost:5000/dcl"");
   //         ws.onopen = function() {
   //           
   //             // Web Socket is connected, send data using send()
   //             ws.send(""Message to send"");
   //             document.getElementById(""sse"").innerHTML += ""\r\n  CONNECTED: Message is sent..."";
   //         };
   //         
   //         ws.onmessage = function (evt) { 
   //             var received_msg = evt.data;
   //             document.getElementById(""sse"").innerHTML += ""\r\n  Message is received..."";
   //         };
   //         
   //         ws.onclose = function() { 
   //           
   //             // websocket is closed.
   //             //alert(""Connection is closed...""); 
   //             document.getElementById(""sse"").innerHTML += ""\r\n  Connection is closed..."";
   //         };
   //     } 
   //  else {
   //       
   //         // The browser doesn't support WebSocket
   //         document.getElementById(""sse"").innerHTML += ""\r\n  WebSocket NOT supported by your Browser!"";
   //     }
   // }
   // </script>
   //
   // </head>
   //
   // <body>
   // <div id = ""sse"">
   //     <a href = ""javascript:WebSocketTest()"">Run WebSocket</a>
   // </div>
   //
   // </body>
   // </html>";

    }
    
}