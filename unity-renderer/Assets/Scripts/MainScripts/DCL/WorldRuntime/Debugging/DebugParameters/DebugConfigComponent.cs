using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using System.Timers;
using TMPro;
#if UNITY_ANDROID
using Vuplex.WebView;

#endif
namespace DCL
{
    public class DebugConfigComponent : MonoBehaviour
    {
        private static DebugConfigComponent sharedInstance;
        [SerializeField] private CanvasWebViewPrefab options;
        [SerializeField] private CanvasKeyboard keyboard;
        [SerializeField] private TMP_InputField urlInput;
            private string webViewURL = "";
        UnityEngine.TouchScreenKeyboard keyboardNative;
        public static string keyboardText = "";
        //Timer _buttonRefreshTimer = new Timer();
        //WebViewPrefab _controlsWebViewPrefab;
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
        CanvasWebViewPrefab _canvasWebViewPrefab;
        CanvasKeyboard _keyboard;


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
        public bool soloScene = true;
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
            Web.SetUserAgent("Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
            Web.SetStorageEnabled(true); 
            Web.SetIgnoreCertificateErrors(true);
            Web.EnableRemoteDebugging();
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidGeckoWebView.SetIgnoreCertificateErrors(true);
            AndroidGeckoWebView.GloballySetUserAgent("Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
            AndroidGeckoWebView.SetCameraAndMicrophoneEnabled(true);
            AndroidGeckoWebView.SetPreferences(new Dictionary<string, string> {
                ["security.fileuri.strict_origin_policy"] = "false",
                ["network.websocket.allowInsecureFromHTTPS"] = "true",
                ["security.csp.enable"] = "false",
                ["network.cors_preflight.allow_client_cert"] = "true",
                ["dom.cross_origin_iframes_loaded_in_background"] = "true",
                ["dom.webnotifications.allowcrossoriginiframe"] = "true",
                ["network.auth.subresource-img-cross-origin-http-auth-allow"] = "true",
                ["network.http.referer.XOriginPolicy"] = "true",
                ["network.http.referer.disallowCrossSiteRelaxingDefault.pbmode"] = "false",
                ["network.http.referer.XOriginPolicy"] = "1",
                ["network.websocket.auto-follow-http-redirects"] = "true",
                ["security.csp.enableNavigateTo"] = "true",
                ["security.mixed_content.block_active_content"] = "	false",
                ["security.mixed_content.upgrade_display_content"] = "true"
            });
            
#endif
            
            
        }

        private void Start()
        {
            keyboard.InputReceived += (sender, eventArgs) =>
            {
                options.WebView.HandleKeyboardInput(eventArgs.Value);
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

        private async void OpenWebBrowser()
        {
            string baseUrl = "";
            string debugString = "";
            string debugPanelString = "";
 #if (UNITY_EDITOR  || UNITY_STANDALONE)
            

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

             if (soloScene)
             {
                 debugString += "LOS=0&";
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

             Application.OpenURL(
                 $"{baseUrl}{debugString}{debugPanelString}position={startInCoords.x}%2C{startInCoords.y}&ws={DataStore.i.wsCommunication.url}");
#elif  UNITY_ANDROID
           

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

            if (soloScene)
            {
                debugString += "LOS=0&";
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
                Debug.Log(
                    "[REMINDER] To be able to connect with SSL you should start Chrome with the --ignore-certificate-errors argument specified (or enabling the following option: chrome://flags/#allow-insecure-localhost). In Firefox set the configuration option `network.websocket.allowInsecureFromHTTPS` to true, then use the ws:// rather than the wss:// address.");
            }
         

            webViewURL = $"{baseUrl}{debugString}{debugPanelString}position={startInCoords.x}%2C{startInCoords.y}&ws={DataStore.i.wsCommunication.url}";
            urlInput.text = webViewURL;
            var canvas = GameObject.Find("Canvas");
            //DontDestroyOnLoad(canvas);
            WebViewOptions opt = new WebViewOptions();
            opt.preferredPlugins  = new WebPluginType[] { WebPluginType.Android};
            
            AndroidGeckoWebView.GloballySetUserAgent("Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
            
            _canvasWebViewPrefab = CanvasWebViewPrefab.Instantiate(opt);
            _canvasWebViewPrefab.transform.SetParent(canvas.transform, false);
            _canvasWebViewPrefab.InitialResolution = 400;

            _canvasWebViewPrefab.RemoteDebuggingEnabled = true;
            _canvasWebViewPrefab.LogConsoleMessages = true;
            _canvasWebViewPrefab.NativeOnScreenKeyboardEnabled = false;
            _canvasWebViewPrefab.Native2DModeEnabled = false;
            
            _canvasWebViewPrefab.Initialized += (sender, eventArgs) => {
                _canvasWebViewPrefab.WebView.LoadUrl(webViewURL);
            };
            
            // Create a second webview above the first to show a UI that
            // displays the current URL and provides back / forward navigation buttons.
            //_controlsWebViewPrefab = WebViewPrefab.Instantiate(0.6f, 0.05f);
            //_controlsWebViewPrefab.transform.parent = _canvasWebViewPrefab.transform;
            //_controlsWebViewPrefab.transform.localPosition = new Vector3(0, 0.06f, 0);
            //_controlsWebViewPrefab.transform.localEulerAngles = Vector3.zero;
            //_controlsWebViewPrefab.InitialResolution = 400;

            // Set up a timer to allow the state of the back / forward buttons to be
            // refreshed one second after a URL change occurs.
            //_buttonRefreshTimer.AutoReset = false;
            //_buttonRefreshTimer.Interval = 1000;
            //_buttonRefreshTimer.Elapsed += ButtonRefreshTimer_Elapsed;
            // Create a CanvasKeyboard
            // https://developer.vuplex.com/webview/CanvasKeyboard
            _keyboard = CanvasKeyboard.Instantiate();
            _keyboard.InitialResolution = 400;
            _keyboard.transform.SetParent(canvas.transform, false);
            // Hook up the keyboard so that characters are routed to the CanvasWebViewPrefab.
            _keyboard.InputReceived += (sender, eventArgs) => {
                _canvasWebViewPrefab.WebView.HandleKeyboardInput(eventArgs.Value);
                Web.SetUserAgent(false);
                urlInput.text.Insert(urlInput.caretPosition,eventArgs.Value);
            };
            urlInput.keyboardType = TouchScreenKeyboardType.URL;
            urlInput.contentType = TMP_InputField.ContentType.Alphanumeric;
            
            //await Task.WhenAll(new Task[] {
           //     _canvasWebViewPrefab.WaitUntilInitialized(),
            //    _controlsWebViewPrefab.WaitUntilInitialized()
           // });
           // _controlsWebViewPrefab.WebView.MessageEmitted += Controls_MessageEmitted;
           // _controlsWebViewPrefab.WebView.LoadHtml(CONTROLS_HTML);

            // Android Gecko and UWP w/ XR enabled don't support transparent webviews, so set the cutout
            // rect to the entire view so that the shader makes its black background pixels transparent.
            //var pluginType = _controlsWebViewPrefab.WebView.PluginType;
           // if (pluginType == WebPluginType.AndroidGecko || pluginType == WebPluginType.UniversalWindowsPlatform) {
          //      _controlsWebViewPrefab.SetCutoutRect(new Rect(0, 0, 1, 1));
           // }
            Debug.Log("Created WebView objects");
            _positionPrefabs();
            Debug.Log("finished positioning webview objects");
           

#endif
        }
        private void _positionPrefabs() {
        
            var rectTransform = _canvasWebViewPrefab.transform as RectTransform;
            rectTransform.anchoredPosition3D = Vector3.zero;
            rectTransform.offsetMin = new Vector2(0, 1.5f);
            rectTransform.offsetMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 520/150);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 520/150);
            _canvasWebViewPrefab.transform.localScale = Vector3.one;
        
            var keyboardTransform = _keyboard.transform as RectTransform;
            keyboardTransform.anchoredPosition3D = Vector3.zero;
            keyboardTransform.offsetMin = new Vector2(0.5f, -1.8f);
            keyboardTransform.offsetMax = new Vector2(0.5f, -0.3f);
            keyboardTransform.pivot = new Vector2(0.5f, 0);
            keyboardTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 690/150);
            keyboardTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 162/150);
        }
        public void ReloadPage()
        {
            Debug.Log($"url set is : {_canvasWebViewPrefab.WebView.Url}, reloading");
            Web.SetUserAgent(false);
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidGeckoWebView.GloballySetUserAgent(false);
            _canvasWebViewPrefab.Destroy();
            Destroy(_keyboard.gameObject);
            OpenWebBrowser();
#endif  
            //_canvasWebViewPrefab.WebView.LoadUrl(urlInput.text);
            
            Web.SetUserAgent(false);
            _canvasWebViewPrefab.WebView.Reload();
         
        }
        // void ButtonRefreshTimer_Elapsed(object sender, ElapsedEventArgs eventArgs) {
        //
        //     // Get the main webview's back / forward state and then post a message
        //     // to the controls UI to update its buttons' state.
        //     Vuplex.WebView.Internal.Dispatcher.RunOnMainThread(async () => {
        //         var canGoBack = await _canvasWebViewPrefab.WebView.CanGoBack();
        //         var canGoForward  = await _canvasWebViewPrefab.WebView.CanGoForward();
        //         var serializedMessage = $"{{ \"type\": \"SET_BUTTONS\", \"canGoBack\": {canGoBack.ToString().ToLower()}, \"canGoForward\": {canGoForward.ToString().ToLower()} }}";
        //         _controlsWebViewPrefab.WebView.PostMessage(serializedMessage);
        //     });
        // }

       

       

        
        
        
        // void Controls_MessageEmitted(object sender, EventArgs<string> eventArgs) {
        //
        //     if (eventArgs.Value == "CONTROLS_INITIALIZED") {
        //         // The controls UI won't be initialized in time to receive the first UrlChanged event,
        //         // so explicitly set the initial URL after the controls UI indicates it's ready.
        //         _setDisplayedUrl(_canvasWebViewPrefab.WebView.Url);
        //         return;
        //     }
        //     var message = eventArgs.Value;
        //     if (message == "GO_BACK") {
        //         _canvasWebViewPrefab.WebView.GoBack();
        //     } else if (message == "GO_FORWARD") {
        //         _canvasWebViewPrefab.WebView.GoForward();
        //     }
        // }
        //
        // void MainWebView_UrlChanged(object sender, UrlChangedEventArgs eventArgs) {
        //
        //     _setDisplayedUrl(eventArgs.Url);
        //     _buttonRefreshTimer.Start();
        // }
        //
        // void _setDisplayedUrl(string url) {
        //
        //     if (_controlsWebViewPrefab.WebView != null) {
        //         var serializedMessage = $"{{ \"type\": \"SET_URL\", \"url\": \"{url}\" }}";
        //         _controlsWebViewPrefab.WebView.PostMessage(serializedMessage);
        //     }
        // }

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
//         const string CONTROLS_HTML = @"
//             <!DOCTYPE html>
//             <html>
//                 <head>
//                     <!-- This transparent meta tag instructs 3D WebView to allow the page to be transparent. -->
//                     <meta name='transparent' content='true'>
//                     <meta charset='UTF-8'>
//                     <style>
//                         body {
//                             font-family: Helvetica, Arial, Sans-Serif;
//                             margin: 0;
//                             height: 100vh;
//                             color: white;
//                         }
//                         .controls {
//                             display: flex;
//                             justify-content: space-between;
//                             align-items: center;
//                             height: 100%;
//                         }
//                         .controls > div {
//                             background-color: #283237;
//                             border-radius: 8px;
//                             height: 100%;
//                         }
//                         .url-display {
//                             flex: 0 0 75%;
//                             width: 75%;
//                             display: flex;
//                             align-items: center;
//                             overflow: hidden;
//                         }
//                         #url {
//                             width: 100%;
//                             white-space: nowrap;
//                             overflow: hidden;
//                             text-overflow: ellipsis;
//                             padding: 0 15px;
//                             font-size: 18px;
//                         }
//                         .buttons {
//                             flex: 0 0 20%;
//                             width: 20%;
//                             display: flex;
//                             justify-content: space-around;
//                             align-items: center;
//                         }
//                         .buttons > button {
//                             font-size: 40px;
//                             background: none;
//                             border: none;
//                             outline: none;
//                             color: white;
//                             margin: 0;
//                             padding: 0;
//                         }
//                         .buttons > button:disabled {
//                             color: rgba(255, 255, 255, 0.3);
//                         }
//                         .buttons > button:last-child {
//                             transform: scaleX(-1);
//                         }
//                         /* For Gecko only, set the background color
//                         to black so that the shader's cutout rect
//                         can translate the black pixels to transparent.*/
//                         @supports (-moz-appearance:none) {
//                             body {
//                                 background-color: black;
//                             }
//                         }
//                     </style>
//                 </head>
//                 <body>
//                     <div class='controls'>
//                         <div class='url-display'>
//                             <div id='url'></div>
//                         </div>
//                         <div class='buttons'>
//                             <button id='back-button' disabled='true' onclick='vuplex.postMessage(""GO_BACK"")'>←</button>
//                             <button id='forward-button' disabled='true' onclick='vuplex.postMessage(""GO_FORWARD"")'>←</button>
//                         </div>
//                     </div>
//                     <script>
//                         // Handle messages sent from C#
//                         function handleMessage(message) {
//                             var data = JSON.parse(message.data);
//                             if (data.type === 'SET_URL') {
//                                 document.getElementById('url').innerText = data.url;
//                             } else if (data.type === 'SET_BUTTONS') {
//                                 document.getElementById('back-button').disabled = !data.canGoBack;
//                                 document.getElementById('forward-button').disabled = !data.canGoForward;
//                             }
//                         }
//
//                         function attachMessageListener() {
//                             window.vuplex.addEventListener('message', handleMessage);
//                             window.vuplex.postMessage('CONTROLS_INITIALIZED');
//                         }
//
//                         if (window.vuplex) {
//                             attachMessageListener();
//                         } else {
//                             window.addEventListener('vuplexready', attachMessageListener);
//                         }
//                     </script>
//                 </body>
//             </html>
//         ";
    }
    
}