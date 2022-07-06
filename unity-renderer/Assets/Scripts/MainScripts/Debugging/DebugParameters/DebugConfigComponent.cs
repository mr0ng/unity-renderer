using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using UnityEditor;
using UnityEngine;
#if UNITY_ANDROID
using Vuplex.WebView;
#endif
namespace DCL
{
    public class DebugConfigComponent : MonoBehaviour
    {
        private static DebugConfigComponent sharedInstance;
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

        [Header("Kernel General Settings")]
        public string kernelVersion;
        public bool useCustomContentServer = false;

        public string customContentServerUrl = "http://localhost:1338/";

        [Space(10)]
        public BaseUrl baseUrlMode;

        public string baseUrlCustom;

        [Space(10)]
        public Environment environment;

        [Tooltip("Set this field to force the realm (server). On the latin-american zone, recommended realms are fenrir-amber, baldr-amber and thor. Other realms can give problems to debug from Unity editor due to request certificate issues.\n\nFor auto selection leave this field blank.\n\nCheck out all the realms at https://catalyst-monitor.vercel.app/?includeDevServers")]
        public string realm;

        public Vector2 startInCoords = new Vector2(-99, 109);

        [Header("Kernel Misc Settings")]
        public bool forceLocalComms = true;

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
            Web.SetUserAgent(false);
            Web.SetStorageEnabled(true); 
            Web.SetIgnoreCertificateErrors(true);
            Web.EnableRemoteDebugging();
            
            
            
        }

        private void Start()
        {
           
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
//  #if (UNITY_EDITOR  || UNITY_STANDALONE)
//             
//
//              if (baseUrlMode == BaseUrl.CUSTOM)
//                  baseUrl = baseUrlCustom;
//              else
//                  baseUrl = "http://localhost:3000/?";
//
//              switch (environment)
//              {
//                  case Environment.USE_DEFAULT_FROM_URL:
//                      break;
//                  case Environment.LOCAL:
//                      debugString = "DEBUG_MODE&";
//                      break;
//                  case Environment.ZONE:
//                      debugString = "NETWORK=ropsten&";
//                      break;
//                  case Environment.TODAY:
//                      debugString = "NETWORK=mainnet&";
//                      break;
//                  case Environment.ORG:
//                      debugString = "NETWORK=mainnet&";
//                      break;
//              }
//
//              if (!string.IsNullOrEmpty(kernelVersion))
//              {
//                  debugString += $"kernel-version={kernelVersion}&";
//              }
//
//              if (forceLocalComms)
//              {
//                  debugString += "LOCAL_COMMS&";
//              }
//
//              if (allWearables)
//              {
//                  debugString += "ALL_WEARABLES&";
//              }
//
//              if (testWearables)
//              {
//                  debugString += "TEST_WEARABLES&";
//              }
//
//              if (enableTutorial)
//              {
//                  debugString += "RESET_TUTORIAL&";
//              }
//
//              if (soloScene)
//              {
//                  debugString += "LOS=0&";
//              }
//
//              if (builderInWorld)
//              {
//                  debugString += "ENABLE_BUILDER_IN_WORLD&";
//              }
//
//              if (!string.IsNullOrEmpty(realm))
//              {
//                  debugString += $"realm={realm}&";
//              }
//
//              
//
//              if (debugPanelMode == DebugPanel.Engine)
//              {
//                  debugPanelString = ENGINE_DEBUG_PANEL + "&";
//              }
//              else if (debugPanelMode == DebugPanel.Scene)
//              {
//                  debugPanelString = SCENE_DEBUG_PANEL + "&";
//              }
//
//              if (!webSocketSSL)
//              {
//                  if (baseUrl.Contains("play.decentraland.org"))
//                  {
//                      Debug.LogError("play.decentraland.org only works with WebSocket SSL, please change the base URL to play.decentraland.zone");
//                      QuitGame();
//                      return;
//                  }
//              }
//              else
//              {
//                  Debug.Log("[REMINDER] To be able to connect with SSL you should start Chrome with the --ignore-certificate-errors argument specified (or enabling the following option: chrome://flags/#allow-insecure-localhost). In Firefox set the configuration option `network.websocket.allowInsecureFromHTTPS` to true, then use the ws:// rather than the wss:// address.");                
//              }
//
//              Application.OpenURL(
//                  $"{baseUrl}{debugString}{debugPanelString}position={startInCoords.x}%2C{startInCoords.y}&ws={DataStore.i.wsCommunication.url}");
// #elif  UNITY_ANDROID
           

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
                    //QuitGame();
                    //return;
                }
            }
            else
            {
                Debug.Log("[REMINDER] To be able to connect with SSL you should start Chrome with the --ignore-certificate-errors argument specified (or enabling the following option: chrome://flags/#allow-insecure-localhost). In Firefox set the configuration option `network.websocket.allowInsecureFromHTTPS` to true, then use the ws:// rather than the wss:// address.");                
            }
            // _webViewPrefab = WebViewPrefab.Instantiate(0.9f, 0.6f);
            // _webViewPrefab.transform.parent = transform;
            // _webViewPrefab.transform.localPosition = new Vector3(0, 0f, 0.6f);
            // _webViewPrefab.transform.LookAt(transform);
            // _webViewPrefab.Initialized += (sender, e) => {
            //     _webViewPrefab.WebView.LoadUrl($"{baseUrl}{debugString}{debugPanelString}position={startInCoords.x}%2C{startInCoords.y}&ws={DataStore.i.wsCommunication.url}");
            // };

            // Add the keyboard under the main webview.
            // _keyboard = Keyboard.Instantiate();
            // _keyboard.transform.parent = _webViewPrefab.transform;
            // _keyboard.transform.localPosition = new Vector3(0, -0.41f, 0);
            // _keyboard.transform.localEulerAngles = new Vector3(0, 0, 0);
            // // Hook up the keyboard so that characters are routed to the main webview.
            // _keyboard.InputReceived += (sender, e) => _webViewPrefab.WebView.HandleKeyboardInput(e.Value);
            
            
            var canvas = GameObject.Find("Canvas");
            DontDestroyOnLoad(canvas);
            WebViewOptions opt = new WebViewOptions();
            opt.preferredPlugins  = new WebPluginType[] { WebPluginType.AndroidGecko};
            _canvasWebViewPrefab = CanvasWebViewPrefab.Instantiate(opt);
            
            _canvasWebViewPrefab.InitialResolution = 400;
            _canvasWebViewPrefab.RemoteDebuggingEnabled = true;
            _canvasWebViewPrefab.LogConsoleMessages = true;
            _canvasWebViewPrefab.NativeOnScreenKeyboardEnabled = false;
            _canvasWebViewPrefab.Native2DModeEnabled = false;
            _canvasWebViewPrefab.transform.SetParent(canvas.transform, false);
            _canvasWebViewPrefab.Initialized += (sender, eventArgs) => {
                _canvasWebViewPrefab.WebView.LoadUrl($"{baseUrl}{debugString}{debugPanelString}position={startInCoords.x}%2C{startInCoords.y}&ws={DataStore.i.wsCommunication.url}");
            };
            
            // Create a CanvasKeyboard
            // https://developer.vuplex.com/webview/CanvasKeyboard
            _keyboard = CanvasKeyboard.Instantiate();
            _keyboard.InitialResolution = 400;
            _keyboard.transform.SetParent(canvas.transform, false);
            // Hook up the keyboard so that characters are routed to the CanvasWebViewPrefab.
            _keyboard.InputReceived += (sender, eventArgs) => {
                _canvasWebViewPrefab.WebView.HandleKeyboardInput(eventArgs.Value);
            };
            Debug.Log("Created WebView objects");
            _positionPrefabs();
            Debug.Log("finished positioning webview objects");
           

//#endif
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
            keyboardTransform.offsetMin = new Vector2(0.5f, -1.5f);
            keyboardTransform.offsetMax = new Vector2(0.5f, 0);
            keyboardTransform.pivot = new Vector2(0.5f, 0);
            keyboardTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 690/150);
            keyboardTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 162/150);
        }

        private void OnDestroy() { DataStore.i.wsCommunication.communicationReady.OnChange -= OnCommunicationReadyChangedValue; }
        
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
    }
}