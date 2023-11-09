using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using DCL;
using System.IO;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class WebSocketCommunication : IKernelCommunication
{
    public static DCLWebSocketService service;

    [System.NonSerialized]
    public static Queue<DCLWebSocketService.Message> queuedMessages = new Queue<DCLWebSocketService.Message>();

    [System.NonSerialized]
    public static volatile bool queuedMessagesDirty;

    private Dictionary<string, GameObject> bridgeGameObjects = new Dictionary<string, GameObject>();

    public Dictionary<string, string> messageTypeToBridgeName = new Dictionary<string, string>(); // Public to be able to modify it from `explorer-desktop`
    private bool requestStop = false;
    private Coroutine updateCoroutine;
    private int currentPort = 0;
    WebSocketServer ws;
    public static event Action<string> OnProfileLoading;

    public WebSocketCommunication(bool withSSL = false, int startPort = 7666, int endPort = 7800)
    {
        if (currentPort != 0) startPort = currentPort + 1;
        InitMessageTypeToBridgeName();

        DCL.DataStore.i.debugConfig.isWssDebugMode = true;

        string url = StartServer(startPort, endPort, withSSL);

        Debug.Log("WebSocket Server URL: " + url);

        DataStore.i.wsCommunication.url = url;

        DataStore.i.wsCommunication.communicationReady.Set(true);

        updateCoroutine = CoroutineStarter.Start(ProcessMessages());
    }

    public bool isServerReady => ws.IsListening;

    public void Dispose()
    {
        if (ws != null)
            ws.Stop();
    }

    public static event Action<DCLWebSocketService> OnWebSocketServiceAdded;
#if UNITY_ANDROID && !UNITY_EDITOR

    void MakeSecureRequest() {
    if (Application.platform == RuntimePlatform.Android) {
        Debug.Log("About to make secure request...");

        AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        Debug.Log($"Activity object: {activity}");
        AndroidJavaClass javaClass = new AndroidJavaClass("com.example.certhandler.MyCertificateHandler");
        bool success = javaClass.CallStatic<bool>("handleCertificate", activity);

        if (success) {
            Debug.Log("Android Certificate Granted");
        } else {
            Debug.Log("Android Certificate Failed");
        }
    }
}

#endif
    private string StartServer(int port, int maxPort, bool withSSL, bool verbose = false)
    {
        currentPort = port;
        Debug.Log($"WebSocketCommunication: StartServer 1");

        if (port > maxPort) { throw new SocketException((int)SocketError.AddressAlreadyInUse); }

        string wssServerUrl;
        string wssServiceId = "dcl";

        try
        {
            if (withSSL)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                MakeSecureRequest();
                wssServerUrl = $"wss://127.0.0.1:{port}/";
#else
                wssServerUrl = $"wss://localhost:{port}/";
#endif
                Debug.Log($"WebSocketCommunication: StartServer 2");
                ws = new WebSocketServer(wssServerUrl)
                {
                    SslConfiguration =
                    {
                        ServerCertificate = loadSelfSignedServerCertificate(),
                        ClientCertificateRequired = false,
                        CheckCertificateRevocation = false,
                        ClientCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                        EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
                    },
                    KeepClean = false
                };
                Debug.Log($"WebSocketCommunication: StartServer 3");
            }
            else
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                wssServerUrl = $"ws://127.0.0.1:{port}/";
#else
                wssServerUrl = $"ws://localhost:{port}/";
#endif
                Debug.Log($"WebSocketCommunication: StartServer 4");
                ws = new WebSocketServer(wssServerUrl);
            }
            Debug.Log($"WebSocketCommunication: StartServer 5");
            ws.AddWebSocketService("/" + wssServiceId, () =>
            {
                service = new DCLWebSocketService();
                Debug.Log($"WebSocketCommunication: StartServer 6");
                service.OnCloseEvent += () =>
                {
                    // DataStore.i.wsCommunication.communicationReady.Set(false);

                    ws.Stop();
                    ws.WebSocketServices.Clear();

                    //ws.WebSocketServices.Clear();

                    queuedMessages.Clear();

                    //service.Context.WebSocket.Connect();

                    service = null;
                    ws = null;
                    requestStop = false;
                    queuedMessagesDirty = false;

                    UnityThread.executeCoroutine(
                        RestartCommunication(port, maxPort, withSSL)
                    );
                };
                Debug.Log($"WebSocketCommunication: StartServer 7");
                OnWebSocketServiceAdded?.Invoke(service);
                Debug.Log($"WebSocketCommunication: StartServer 8");
                return service;
            });
            Debug.Log($"WebSocketCommunication: StartServer 9");
            if (verbose)
            {
                ws.Log.Level = LogLevel.Debug;
                ws.Log.Output += OnWebSocketLog;
            }
            Debug.Log($"WebSocketCommunication: StartServer 10");
            ws.Start();
            Debug.Log($"WebSocketCommunication: StartServer 11");
        }
        catch (InvalidOperationException e)
        {
            Debug.Log($"WebSocketCommunication: StartServer 12");
            ws.Stop();

            if (withSSL) // Search for available ports only if we're using SSL
            {
                SocketException se = (SocketException)e.InnerException;
                Debug.Log($"WebSocketCommunication: StartServer 13");
                if (se is { SocketErrorCode: SocketError.AddressAlreadyInUse }) { return StartServer(port + 1, maxPort, withSSL); }
            }

            throw new InvalidOperationException(e.Message, e.InnerException);
        }
        Debug.Log($"WebSocketCommunication: StartServer 14");
        string wssUrl = wssServerUrl + wssServiceId;
        Debug.Log($"WebSocketCommunication: StartServer 15 {wssUrl}");
        return wssUrl;
    }

    private IEnumerator RestartCommunication(int port, int maxPort, bool withSSL)
    {
        yield return new WaitForSeconds(3);
        DataStore.i.wsCommunication.communicationReady.Set(false);
        DataStore.i.common.isApplicationQuitting.Set(false);

        //
        // yield return new WaitForSeconds(3);
        // InitMessageTypeToBridgeName();
        //
        // DCL.DataStore.i.debugConfig.isWssDebugMode = true;
        // string url = StartServer(port + 1, maxPort, withSSL);
        // Debug.Log("WebSocket Server URL: " + url);
        //
        // DataStore.i.wsCommunication.url = url;
        //
        // DataStore.i.wsCommunication.communicationReady.Set(true);
        // if(updateCoroutine == null);
        // updateCoroutine = CoroutineStarter.Start(ProcessMessages());
        //
        yield return new WaitForSeconds(1);
        #if DCL_VR
        DebugConfigComponent.i.ShowWebviewScreen();
        #endif
    }

    private X509Certificate2 loadSelfSignedServerCertificate()
    {
        byte[] rawData = Convert.FromBase64String(SelfCertificateData.data);
        X509Certificate2 cert = new X509Certificate2(rawData, "cert");

        // // Export the certificate to a byte array in DER format (.cer)
        // byte[] exportedCert = cert.Export(X509ContentType.Cert);
        //
        // // Or, for PEM format, use X509ContentType.Pfx or X509ContentType.Pkcs12
        //
        // // Save the byte array to a .cer file
        // string filePath = Path.Combine(Application.persistentDataPath, "myCertificate.cer");
        // File.WriteAllBytes(filePath, exportedCert);
        //
        // byte[] exportedCert2 = cert.Export(X509ContentType.Pfx);
        //
        // // Or, for PEM format, use X509ContentType.Pfx or X509ContentType.Pkcs12
        //
        // // Save the byte array to a .cer file
        // string filePath2 = Path.Combine(Application.persistentDataPath, "myCertificate.pfx");
        // File.WriteAllBytes(filePath2, exportedCert2);
        //
        // byte[] exportedCert3 = cert.Export(X509ContentType.Pkcs12);
        //
        // // Or, for PEM format, use X509ContentType.Pfx or X509ContentType.Pkcs12
        //
        // // Save the byte array to a .cer file
        // string filePath3 = Path.Combine(Application.persistentDataPath, "myCertificate.Pkcs12");
        // File.WriteAllBytes(filePath3, exportedCert3);

        // Certificate validation logic for Android
        // #if UNITY_ANDROID && !UNITY_EDITOR
        //     // Remove existing callbacks to avoid multiple calls
        //     ServicePointManager.ServerCertificateValidationCallback = null;
        //
        //     // Add a new callback
        //     ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => {
        //         // Compare with your known certificate
        //         return certificate.Equals(cert);
        //     };
        // #endif
        return cert;
    }

    private void OnWebSocketLog(LogData logData, string message)
    {
        switch (logData.Level)
        {
            case LogLevel.Debug:
                Debug.Log($"[WebSocket] {logData.Message}");
                break;
            case LogLevel.Warn:
                Debug.LogWarning($"[WebSocket] {logData.Message}");
                break;
            case LogLevel.Error:
                Debug.LogError($"[WebSocket] {logData.Message}");
                break;
            case LogLevel.Fatal:
                Debug.LogError($"[WebSocket] {logData.Message}");
                break;
        }
    }

    private void InitMessageTypeToBridgeName()
    {
        // Please, use `Bridges` as a bridge name, avoid adding messages here. The system will use `Bridges` as the default bridge name.
        messageTypeToBridgeName["SetDebug"] = "Main";
        messageTypeToBridgeName["SetSceneDebugPanel"] = "Main";
        messageTypeToBridgeName["SetMemoryUsage"] = "Main";
        messageTypeToBridgeName["ShowFPSPanel"] = "Main";
        messageTypeToBridgeName["HideFPSPanel"] = "Main";
        messageTypeToBridgeName["SetEngineDebugPanel"] = "Main";
        messageTypeToBridgeName["SendSceneMessage"] = "Main";
        messageTypeToBridgeName["LoadParcelScenes"] = "Main";
        messageTypeToBridgeName["UnloadScene"] = "Main";
        messageTypeToBridgeName["UnloadSceneV2"] = "Main";
        messageTypeToBridgeName["Reset"] = "Main";
        messageTypeToBridgeName["CreateGlobalScene"] = "Main";
        messageTypeToBridgeName["BuilderReady"] = "Main";
        messageTypeToBridgeName["UpdateParcelScenes"] = "Main";
        messageTypeToBridgeName["LoadProfile"] = "Main";
        messageTypeToBridgeName["AddUserProfileToCatalog"] = "Main";
        messageTypeToBridgeName["AddUserProfilesToCatalog"] = "Main";
        messageTypeToBridgeName["RemoveUserProfilesFromCatalog"] = "Main";
        messageTypeToBridgeName["ActivateRendering"] = "Main";
        messageTypeToBridgeName["DeactivateRendering"] = "Main";
        messageTypeToBridgeName["ForceActivateRendering"] = "Main";
        messageTypeToBridgeName["AddWearablesToCatalog"] = "Main";
        messageTypeToBridgeName["WearablesRequestFailed"] = "Main";
        messageTypeToBridgeName["RemoveWearablesFromCatalog"] = "Main";
        messageTypeToBridgeName["ClearWearableCatalog"] = "Main";
        messageTypeToBridgeName["InitializeFriends"] = "Main";
        messageTypeToBridgeName["UpdateFriendshipStatus"] = "Main";
        messageTypeToBridgeName["UpdateUserPresence"] = "Main";
        messageTypeToBridgeName["FriendNotFound"] = "Main";
        messageTypeToBridgeName["UpdateMinimapSceneInformation"] = "Main";
        messageTypeToBridgeName["UpdateHotScenesList"] = "Main";
        messageTypeToBridgeName["SetRenderProfile"] = "Main";
        messageTypeToBridgeName["CrashPayloadRequest"] = "Main";
        messageTypeToBridgeName["SetDisableAssetBundles"] = "Main";
        messageTypeToBridgeName["DumpRendererLockersInfo"] = "Main";
        messageTypeToBridgeName["PublishSceneResult"] = "Main";
        messageTypeToBridgeName["BuilderProjectInfo"] = "Main";
        messageTypeToBridgeName["BuilderInWorldCatalogHeaders"] = "Main";
        messageTypeToBridgeName["RequestedHeaders"] = "Main";
        messageTypeToBridgeName["AddAssets"] = "Main";
        messageTypeToBridgeName["RunPerformanceMeterTool"] = "Main";
        messageTypeToBridgeName["InstantiateBotsAtWorldPos"] = "Main";
        messageTypeToBridgeName["InstantiateBotsAtCoords"] = "Main";
        messageTypeToBridgeName["StartBotsRandomizedMovement"] = "Main";
        messageTypeToBridgeName["StopBotsMovement"] = "Main";
        messageTypeToBridgeName["RemoveBot"] = "Main";
        messageTypeToBridgeName["ClearBots"] = "Main";
        messageTypeToBridgeName["ToggleSceneBoundingBoxes"] = "Main";
        messageTypeToBridgeName["TogglePreviewMenu"] = "Main";
        messageTypeToBridgeName["ToggleSceneSpawnPoints"] = "Main";
        messageTypeToBridgeName["AddFriendsWithDirectMessages"] = "Main";
        messageTypeToBridgeName["AddFriends"] = "Main";
        messageTypeToBridgeName["AddFriendRequests"] = "Main";
        messageTypeToBridgeName["UpdateTotalFriendRequests"] = "Main";
        messageTypeToBridgeName["UpdateTotalFriends"] = "Main";
        messageTypeToBridgeName["UpdateHomeScene"] = "Main";

        messageTypeToBridgeName["Teleport"] = "CharacterController";

        messageTypeToBridgeName["SetRotation"] = "CameraController";

        messageTypeToBridgeName["ShowNotificationFromJson"] = "HUDController";
        messageTypeToBridgeName["ConfigureHUDElement"] = "HUDController";
        messageTypeToBridgeName["ShowTermsOfServices"] = "HUDController";
        messageTypeToBridgeName["RequestTeleport"] = "HUDController";
        messageTypeToBridgeName["ShowAvatarEditorInSignUp"] = "HUDController";
        messageTypeToBridgeName["SetUserTalking"] = "HUDController";
        messageTypeToBridgeName["SetUsersMuted"] = "HUDController";
        messageTypeToBridgeName["ShowWelcomeNotification"] = "HUDController";
        messageTypeToBridgeName["UpdateBalanceOfMANA"] = "HUDController";
        messageTypeToBridgeName["SetPlayerTalking"] = "HUDController";
        messageTypeToBridgeName["SetVoiceChatEnabledByScene"] = "HUDController";
        messageTypeToBridgeName["TriggerSelfUserExpression"] = "HUDController";

        messageTypeToBridgeName["GetMousePosition"] = "BuilderController";
        messageTypeToBridgeName["SelectGizmo"] = "BuilderController";
        messageTypeToBridgeName["ResetObject"] = "BuilderController";
        messageTypeToBridgeName["ZoomDelta"] = "BuilderController";
        messageTypeToBridgeName["SetPlayMode"] = "BuilderController";
        messageTypeToBridgeName["TakeScreenshot"] = "BuilderController";
        messageTypeToBridgeName["ResetBuilderScene"] = "BuilderController";
        messageTypeToBridgeName["SetBuilderCameraPosition"] = "BuilderController";
        messageTypeToBridgeName["SetBuilderCameraRotation"] = "BuilderController";
        messageTypeToBridgeName["ResetBuilderCameraZoom"] = "BuilderController";
        messageTypeToBridgeName["SetGridResolution"] = "BuilderController";
        messageTypeToBridgeName["OnBuilderKeyDown"] = "BuilderController";
        messageTypeToBridgeName["UnloadBuilderScene"] = "BuilderController";
        messageTypeToBridgeName["SetSelectedEntities"] = "BuilderController";
        messageTypeToBridgeName["GetCameraTargetBuilder"] = "BuilderController";
        messageTypeToBridgeName["PreloadFile"] = "BuilderController";
        messageTypeToBridgeName["SetBuilderConfiguration"] = "BuilderController";

        messageTypeToBridgeName["SetTutorialEnabled"] = "TutorialController";
        messageTypeToBridgeName["SetTutorialEnabledForUsersThatAlreadyDidTheTutorial"] = "TutorialController";

        messageTypeToBridgeName["VoiceChatStatus"] = "VoiceChatController";
    }

    IEnumerator ProcessMessages()
    {
        var hudControllerGO = GameObject.Find("HUDController");
        var mainGO = GameObject.Find("Main");

        while (!requestStop)
        {
            lock (queuedMessages)
            {
                if (queuedMessagesDirty)
                {
                    while (queuedMessages.Count > 0)
                    {
                        DCLWebSocketService.Message msg = queuedMessages.Dequeue();

                        switch (msg.type)
                        {
                            // Add to this list the messages that are used a lot and you want better performance
                            case "SendSceneMessage":
                                DCL.Environment.i.world.sceneController.SendSceneMessage(msg.payload);
                                break;
                            case "Reset":
                                DCL.Environment.i.world.sceneController.UnloadAllScenesQueued();
                                break;
                            case "SetVoiceChatEnabledByScene":
                                if (int.TryParse(msg.payload, out int value)) // The payload should be `string`, this will be changed in a `renderer-protocol` refactor
                                {
                                    hudControllerGO.SendMessage(msg.type, value);
                                }

                                break;
                            case "RunPerformanceMeterTool":
                                if (float.TryParse(msg.payload, out float durationInSeconds)) // The payload should be `string`, this will be changed in a `renderer-protocol` refactor
                                {
                                    mainGO.SendMessage(msg.type, durationInSeconds);
                                }

                                break;
                            default:
#if DCL_VR
                                if (msg.type == "LoadProfile")
                                    OnProfileLoading?.Invoke(msg.payload);
#endif
                                if (!messageTypeToBridgeName.TryGetValue(msg.type, out string bridgeName))
                                {
                                    bridgeName = "Bridges"; // Default bridge
                                }

                                if (bridgeGameObjects.TryGetValue(bridgeName, out GameObject bridgeObject) == false)
                                {
                                    bridgeObject = GameObject.Find(bridgeName);
                                    bridgeGameObjects.Add(bridgeName, bridgeObject);
                                }

                                if (bridgeObject != null) { bridgeObject.SendMessage(msg.type, msg.payload); }

                                break;
                        }

                        if (DCLWebSocketService.VERBOSE)
                        {
                            Debug.Log(
                                "<b><color=#0000FF>WebSocketCommunication</color></b> >>> Got it! passing message of type " +
                                msg.type);
                        }
                    }
                }
            }

            yield return null;
        }
    }
}
