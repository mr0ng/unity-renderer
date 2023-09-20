using DCL;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Vuplex.WebView;

public class WebViewLoginController2 : MonoBehaviour
{
    public static WebViewLoginController2 I;
    protected IWebView webView;
    [SerializeField] private GameObject loginElements;
    [SerializeField] protected BaseWebViewPrefab webViewPrefab;
    [SerializeField] private RawImage webviewImage;
    [SerializeField] private CanvasKeyboard keyboard;
    [SerializeField] private GameObject backgroundObjects;
    [SerializeField] private Button guestLoginButton;
    [SerializeField] private Button walletLoginButton;
    [SerializeField] private Button walletCloseButton;
    [SerializeField] private Button fortmaticLoginButton;
    [SerializeField] private Button walletConnectLoginButton;
    [SerializeField] private Button walletConnectCloseButton;
    [SerializeField] private Button coinbaseLoginButton;

    [SerializeField] private GameObject walletLoginPanel;
    [SerializeField] private GameObject fortmaticLoginPanel;
    [SerializeField] private Button fortmaticCloseButton;
    [SerializeField] private GameObject walletConnectLoginPanel;
    [SerializeField] private GameObject coinbaseLoginPanel;
    [SerializeField] private Button coinbaseCloseButton;

    [SerializeField] private Image walletConnectQRImage;
    [SerializeField] private Image walletConnectQRImage2;
    [SerializeField] private Image coinbaseQRImage;
    [SerializeField] private Image coinbaseQRImage2;

    [SerializeField] private Toggle useNewUIToggle;
    [SerializeField] private bool useNewUI = true;
    [SerializeField] private TMP_Text browserMessage;
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject settingsPanel;

    private const string GUEST_CLICK = "guestClick";
    private const string WALLET_CLICK = "walletClick";
    private const string WALLET_CLOSE_CLICK = "walletCloseClick";
    private const string FORTMATIC_CLICK = "fortmaticClick";
    private const string WALLET_CONNECT_CLICK = "walletConnectClick";
    private const string WALLET_CONNECT_CLOSE_CLICK = "walletConnectCloseClick";
    private const string COINBASE_WALLET_CLICK = "coinbaseWalletClick";
    private const string COINBASE_CLOSE_CLICK = "coinbaseCloseClick";
    private const string FORTMATIC_CLOSE_CLICK = "fortmaticCloseClick";
    private const string FORTMATIC_CONTINUE_CLICK = "fortmaticContinueClick";

    private const string GUEST_LOGIN_BUTTON = "guestLoginButton";
    private const string WALLET_LOGIN_BUTTON = "walletLoginButton";
    private const string WALLET_CLOSE_BUTTON = "walletCloseButton";
    private const string FORTMATIC_LOGIN_BUTTON = "fortmaticLoginButton";
    private const string WALLET_CONNECT_LOGIN_BUTTON = "walletConnectLoginButton";
    private const string COINBASE_WALLET_LOGIN_BUTTON = "coinbaseWalletLoginButton";
    private const string WALLET_CONNECT_CLOSE_BUTTON = "walletConnectCloseButton";
    private const string COINBASE_CLOSE_BUTTON = "coinbaseCloseButton";
    private const string FORTMATIC_CLOSE_BUTTON = "fortmaticCloseButton";

    private const string WALLET_LOGIN_PANEL = "walletLoginPanel";
    private const string FORTMATIC_LOGIN_PANEL = "fortmaticLoginPanel";
    private const string WALLET_CONNECT_LOGIN_PANEL = "walletConnectLoginPanel";
    private const string COINBASE_LOGIN_PANEL = "coinbaseLoginPanel";






    private Dictionary<string, string> jsQueries = new Dictionary<string, string>
    {
        { "guestClick", "document.querySelector('.LoginGuestItem .ui.huge.primary.button').click();" },
        { "walletClick", "document.querySelector('.LoginWalletItem .ui.huge.primary.button').click();" },
        { "walletCloseClick", "document.querySelector('body > div.ui.page.modals.dimmer.transition.visible.active > div > div.dcl.modal-navigation > div.dcl.modal-navigation-button.modal-navigation-close').click();" },
        { "fortmaticClick", "document.querySelector('.dcl.option.fortmatic').click();" },
        { "walletConnectClick", "document.querySelector('.dcl.option.wallet-connect').click();" },
        { "walletConnectCloseClick", "document.querySelector('body > wcm-modal').shadowRoot.querySelector('#wcm-modal > div > wcm-modal-backcard').shadowRoot.querySelector('div.wcm-toolbar > button').click();" },
        { "coinbaseWalletClick", "document.querySelector('.dcl.option.wallet-link').click();" },
        { "coinbaseCloseClick", "document.querySelector('html > div > div.-cbwsdk-link-flow-root > div > div.-cbwsdk-extension-dialog > div > button').click();" },
        { "fortmaticCloseClick", "document.querySelector('.close-button').click();" },
        { "fortmaticContinueClick", "document.querySelector('#root > div > div > div > div > div > div.page-container > div > div > div > div > div > div > div > div.CTAButton-component.is-primary').click();" },
    };

    private Dictionary<string, Button> buttons = new Dictionary<string, Button>();
    private Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (I != null) yield break;
        I = this;

        yield return new WaitUntil(() => VRSettingsManager.I != null);
        useNewUI = VRSettingsManager.I.GetSetting("useNewUI",useNewUI);

        CrossPlatformManager.SetCameraForGame();
        webView = webViewPrefab.WebView;
        fortmaticLoginPanel.transform.localPosition = new Vector3(-0.009f, -1000.0613f, 1000.24f);

        // Populate your dictionaries
        buttons.Add(GUEST_LOGIN_BUTTON, guestLoginButton);
        buttons.Add(WALLET_LOGIN_BUTTON, walletLoginButton);
        buttons.Add(WALLET_CLOSE_BUTTON, walletCloseButton);
        buttons.Add(FORTMATIC_LOGIN_BUTTON, fortmaticLoginButton);
        buttons.Add(WALLET_CONNECT_LOGIN_BUTTON, walletConnectLoginButton);
        buttons.Add(COINBASE_WALLET_LOGIN_BUTTON, coinbaseLoginButton);
        buttons.Add(WALLET_CONNECT_CLOSE_BUTTON, walletConnectCloseButton);
        buttons.Add(COINBASE_CLOSE_BUTTON, coinbaseCloseButton);
        buttons.Add(FORTMATIC_CLOSE_BUTTON, fortmaticCloseButton);

        panels.Add(WALLET_LOGIN_PANEL, walletLoginPanel);
        panels.Add(FORTMATIC_LOGIN_PANEL, fortmaticLoginPanel);
        panels.Add(WALLET_CONNECT_LOGIN_PANEL, walletConnectLoginPanel);
        panels.Add(COINBASE_LOGIN_PANEL, coinbaseLoginPanel);
        settingsButton.onClick.AddListener(() =>
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);

            if (settingsPanel.activeSelf)
            {
                loginElements.SetActive(false);
            }
            else
            {
                loginElements.SetActive(true);
            }
        });

        // Add more as required

        // Attach the functions to the buttons' onClick event
        buttons[GUEST_LOGIN_BUTTON]
           .onClick.AddListener(() =>
            {
                StartCoroutine(WaitAndExecuteGuestButtonClick());
            });

        buttons[WALLET_LOGIN_BUTTON].onClick.AddListener(() =>
            {
                StartCoroutine(WaitAndExecuteWalletButtonClick());
            });

        buttons[WALLET_CLOSE_BUTTON]
           .onClick.AddListener(() =>
            {
                StartCoroutine(ButtonClick(WALLET_CLOSE_CLICK, (bool success) =>
                {
                    if (success)
                    {
                        ShowPanel(WALLET_LOGIN_PANEL, false);
                        buttons[GUEST_LOGIN_BUTTON].gameObject.SetActive(true);
                        buttons[WALLET_LOGIN_BUTTON].gameObject.SetActive(true);

                    }
                }));
            });

        buttons[FORTMATIC_LOGIN_BUTTON]
           .onClick.AddListener(() =>
            {
                StartCoroutine(ButtonClick(FORTMATIC_CLICK, (bool success) =>
                {
                    if (success)
                    {
                        keyboard.gameObject.SetActive(true);
                        webViewPrefab.gameObject.SetActive(true);
                        fortmaticLoginPanel.transform.localPosition = new Vector3(-0.009f, -0.0613f, 0.24f);
                        ShowPanel(WALLET_LOGIN_PANEL, false);
                        ShowPanel(FORTMATIC_LOGIN_PANEL, true);

                    }
                }));
            });

        buttons[WALLET_CONNECT_LOGIN_BUTTON]
           .onClick.AddListener(() =>
            {
                StartCoroutine(ButtonClick(WALLET_CONNECT_CLICK, (bool success) =>
                {
                    if (success)
                    {
                        StartCoroutine(WaitForElement("body > wcm-modal", (bool elementExists) =>
                        {
                            if (elementExists)
                            {
                                ShowPanel(WALLET_LOGIN_PANEL, false);
                                ShowPanel(WALLET_CONNECT_LOGIN_PANEL, true);
                                walletConnectQRImage.material = webviewImage.material;

                                walletConnectQRImage2.material = webviewImage.material;
                                walletConnectQRImage2.transform.parent.gameObject.SetActive(true);

                            }
                        }));
                    }
                }));
            });

        buttons[COINBASE_WALLET_LOGIN_BUTTON].onClick.AddListener(() =>
            {
                StartCoroutine(ButtonClick(COINBASE_WALLET_CLICK, (bool success) =>
                {
                    if (success)
                    {
                        StartCoroutine(WaitForElement("html > div > div.-cbwsdk-link-flow-root > div > div.-cbwsdk-extension-dialog > div > div.-cbwsdk-extension-dialog-box-bottom > div.-cbwsdk-extension-dialog-box-bottom-qr-region > div", (bool elementExists) =>
                        {
                            if (elementExists)
                            {
                                ShowPanel(WALLET_LOGIN_PANEL, false);
                                ShowPanel(COINBASE_LOGIN_PANEL, true);
                                coinbaseQRImage.material = webviewImage.material;
                                coinbaseQRImage2.transform.parent.gameObject.SetActive(true);
                                coinbaseQRImage2.material = webviewImage.material;
                            }
                        }));
                    }
                }));
            });

        buttons[WALLET_CONNECT_CLOSE_BUTTON]
           .onClick.AddListener(() =>
            {
                StartCoroutine(ButtonClick(WALLET_CONNECT_CLOSE_CLICK, (bool success) =>
                {
                    if (success)
                    {
                        ResetScreen();
                        webView.Reload();
                    }
                }));
            });

        buttons[COINBASE_CLOSE_BUTTON]
           .onClick.AddListener(() =>
            {
                StartCoroutine(ButtonClick("coinbaseCloseClick", (bool success) =>
                {
                    if (success)
                    {
                        ResetScreen();
                        webView.Reload();
                    }
                }));
            });

        buttons[FORTMATIC_CLOSE_BUTTON]
           .onClick.AddListener(() =>
            {
                ResetScreen();
                webView.Reload();
            });


        ResetScreen();
        WebSocketCommunication.OnProfileLoading += (ani) =>
        {
            HideLoginElements();
        };

        useNewUIToggle.onValueChanged.AddListener(UpdateNewUserUIToggle);
        useNewUIToggle.isOn = useNewUI;
        if (useNewUI)
        {
            UpdateNewUserUIToggle(useNewUI);
        }
        if(!VRSettingsManager.I.OnSettingChanged.ContainsKey("openInternalBrowser"))
        {
            VRSettingsManager.I.OnSettingChanged.Add("openInternalBrowser", null); // Add a null action for the new key
        }
        VRSettingsManager.I.OnSettingChanged["openInternalBrowser"] += OnOpenInternalBrowserChanged;
        OnOpenInternalBrowserChanged( VRSettingsManager.I.GetSetting("openInternalBrowser"));

    }
    private IEnumerator WaitAndExecuteGuestButtonClick()
    {
        // Wait for the element to become available
        yield return WaitForElement(".LoginGuestItem .ui.huge.primary.button", (bool success) =>
        {
            if (success)
            {
                // Element is available, execute the rest of the logic
                fortmaticLoginPanel.transform.localPosition = new Vector3(-0.009f, -1000.0613f, 1000.24f);
                webViewPrefab.gameObject.SetActive(true);
                ShowPanel(FORTMATIC_LOGIN_PANEL, true);
                StartCoroutine(ButtonClick(GUEST_CLICK, (bool clickSuccess) =>
                {
                    if (clickSuccess)
                    {
                        buttons[GUEST_LOGIN_BUTTON].gameObject.SetActive(false);
                        buttons[WALLET_LOGIN_BUTTON].gameObject.SetActive(false);
                    }
                    else
                    {
                        // Handle failure here if needed
                    }
                }));
            }
            else
            {
                // Element was not found within the timeout period; handle this case as needed
            }
        });
    }

    private IEnumerator WaitAndExecuteWalletButtonClick()
    {
        // Wait for the element to become available
        yield return WaitForElement(".LoginGuestItem .ui.huge.primary.button", (bool success) =>
        {
            if (success)
            {
                // Element is available, execute the rest of the logic
                fortmaticLoginPanel.transform.localPosition = new Vector3(-0.009f, -1000.0613f, 1000.24f);
                webViewPrefab.gameObject.SetActive(true);
                ShowPanel(FORTMATIC_LOGIN_PANEL, true);

                StartCoroutine(ButtonClick(WALLET_CLICK, (bool clickSuccess) =>
                {
                    if (clickSuccess)
                    {
                        StartCoroutine(WaitForElement("body > div.ui.page.modals.dimmer.transition.visible.active > div > div.content > div.dcl.option.metamask", (bool elementExists) =>
                        {
                            if (elementExists)
                            {
                                CrossPlatformManager.SetCameraForGame();
                                buttons[GUEST_LOGIN_BUTTON].gameObject.SetActive(false);
                                buttons[WALLET_LOGIN_BUTTON].gameObject.SetActive(false);
                                ShowPanel(WALLET_LOGIN_PANEL, true);
                            }
                        }));
                    }
                    else
                    {
                        // Handle failure here if needed
                    }
                }));
            }
            else
            {
                // Element was not found within the timeout period; handle this case as needed
            }
        });
    }
    public IEnumerator ButtonClick(string key, Action<bool> callback)
    {
        if (webView == null) webView = webViewPrefab.WebView;

        if (!jsQueries.TryGetValue(key, out string jsCode))
        {
            callback(false);
            yield break;
        }

        // Remove '.click();' from the JavaScript code and then check if the element is null before clicking it.
        string queryWithoutClick = jsCode.Substring(0, jsCode.LastIndexOf(".click();"));

        string jsCheckCode = $@"
(function() {{
    var elem = {queryWithoutClick};
    if (elem !== null) {{
        elem.click();
        return 'success';
    }} else {{
        return 'failure';
    }}
}})();
";

        webView.ExecuteJavaScript(jsCheckCode, (result) =>
        {
            if (result == "failure")
            {
                Debug.LogWarning($"Failed to find and click on element with key: {key}");
                callback(false);
            }
            else if (string.IsNullOrEmpty(result))
            {
                Debug.LogError($"An error occurred when executing JavaScript");
                callback(false);
            }
            else { callback(true); }
        });

        yield return null;
    }

    private IEnumerator WaitForElement(string selector, Action<bool> callback, float timeout = 2)
    {
        bool elementExists = false;
        float startTime = Time.time;


        while (!elementExists && Time.time - startTime < timeout)
        {
            yield return new WaitForSeconds(0.1f); // check every 0.1 second

            webView.ExecuteJavaScript($"document.querySelector('{selector}') !== null", (result) =>
            {
                if (result == "true") { elementExists = true; }
            });
        }

        if (!elementExists)
        {
            buttons["guestLoginButton"].gameObject.SetActive(true);
            buttons["walletLoginButton"].gameObject.SetActive(true);
            ShowPanel("walletLoginPanel", false);
            ShowPanel("walletConnectLoginPanel", false);

            fortmaticLoginPanel.transform.localPosition = new Vector3(-0.009f, -1000.0613f, 1000.24f);
            webView.Reload();
            webViewPrefab.gameObject.SetActive(true);
        }
        callback(elementExists);

    }

    public void ShowPanel(string panelKey, bool show)
    {
        GameObject panel;
        if (!panels.TryGetValue(panelKey, out panel)) return;

        panel.SetActive(show);
    }

    private void ToggleLoginButtonsVisibility(bool show)
    {
        buttons["guestLoginButton"].gameObject.SetActive(show);
        buttons["walletLoginButton"].gameObject.SetActive(show);
    }

    public void ResetScreen()
    {
        buttons[GUEST_LOGIN_BUTTON].gameObject.SetActive(true);
        buttons[WALLET_LOGIN_BUTTON].gameObject.SetActive(true);
        fortmaticLoginPanel.transform.localPosition = new Vector3(-0.009f, -1000.0613f, 1000.24f);
        webViewPrefab.gameObject.SetActive(true);
        ShowPanel(FORTMATIC_LOGIN_PANEL, true);
        ShowPanel(WALLET_LOGIN_PANEL, false);
        ShowPanel(WALLET_CONNECT_LOGIN_PANEL, false);
        ShowPanel(COINBASE_LOGIN_PANEL, false);
        keyboard.gameObject.SetActive(false);
        coinbaseQRImage2.transform.parent.gameObject.SetActive(false);
        walletConnectQRImage2.transform.parent.gameObject.SetActive(false);
        coinbaseQRImage2.material = webviewImage.material;
        walletConnectQRImage2.material = webviewImage.material;
        backgroundObjects.SetActive(true);
        this.transform.parent.gameObject.SetActive(true);
        CrossPlatformManager.SetCameraForGame();

    }

    private IEnumerator CheckWebViewStatus()
    {
        bool notConnected = true;

        while (notConnected)
        {
            webView.ExecuteJavaScript(@"document.body.innerText.includes('Connected');", (result) => {
                if (result == "true")
                {
                    // result.Value should contain the result of JavaScript evaluation, which is a boolean indicating presence of 'Connected' in the text.
                    notConnected = true;
                    HideLoginElements();
                }
            });

            yield return new WaitForSeconds(0.3f);
        }
    }

    private void HideLoginElements()
    {
        buttons[GUEST_LOGIN_BUTTON].gameObject.SetActive(false);
        buttons[WALLET_LOGIN_BUTTON].gameObject.SetActive(false);
        fortmaticLoginPanel.transform.localPosition = new Vector3(-0.009f, -1000.0613f, 1000.24f);
        ShowPanel(WALLET_LOGIN_PANEL, false);
        ShowPanel(WALLET_CONNECT_LOGIN_PANEL, false);
        ShowPanel(COINBASE_LOGIN_PANEL, false);
        keyboard.gameObject.SetActive(false);
        coinbaseQRImage2.transform.parent.gameObject.SetActive(false);
        walletConnectQRImage2.transform.parent.gameObject.SetActive(false);
        backgroundObjects.SetActive(false);
        this.transform.localPosition = new Vector3(-0.009f, -1000.0613f, 1000.24f);

    }
    private void UpdateNewUserUIToggle(bool current)
    {
        useNewUI = current;
        VRSettingsManager.I.SetSetting("useNewUI",useNewUI.ToString());

        if (useNewUI)
        {
            ResetScreen();
            webViewPrefab.transform.parent = fortmaticLoginPanel.transform;
            webViewPrefab.transform.localPosition = new Vector3(0.1790009f,0.03620529f, -0.04f);
        }
        else
        {
            buttons[GUEST_LOGIN_BUTTON].gameObject.SetActive(false);
            buttons[WALLET_LOGIN_BUTTON].gameObject.SetActive(false);
            fortmaticLoginPanel.transform.localPosition = new Vector3(-0.009f, -1000.0613f, 1000.24f);
            ShowPanel(WALLET_LOGIN_PANEL, false);
            ShowPanel(WALLET_CONNECT_LOGIN_PANEL, false);
            ShowPanel(COINBASE_LOGIN_PANEL, false);
            keyboard.gameObject.SetActive(false);
            coinbaseQRImage2.transform.parent.gameObject.SetActive(false);
            walletConnectQRImage2.transform.parent.gameObject.SetActive(false);
            webViewPrefab.transform.parent = loginElements.transform.parent;
            webViewPrefab.transform.localPosition = new Vector3(0.171f,0f, -0.04f);
        }
    }
    void OnOpenInternalBrowserChanged(string newValue)
    {
        if (bool.Parse(newValue))
        {
            browserMessage.transform.parent.gameObject.SetActive(false);
            UpdateNewUserUIToggle(useNewUI);
        }
        else //hide webview selections
        {
            browserMessage.transform.parent.gameObject.SetActive(true);
            browserMessage.text = "Login using external browser";
            buttons[GUEST_LOGIN_BUTTON].gameObject.SetActive(false);
            buttons[WALLET_LOGIN_BUTTON].gameObject.SetActive(false);
            fortmaticLoginPanel.transform.localPosition = new Vector3(-0.009f, -1000.0613f, 1000.24f);
            webViewPrefab.transform.localPosition = new Vector3(-0.009f, -1000.0613f, 1000.24f);
            ShowPanel(WALLET_LOGIN_PANEL, false);
            ShowPanel(WALLET_CONNECT_LOGIN_PANEL, false);
            ShowPanel(COINBASE_LOGIN_PANEL, false);
            keyboard.gameObject.SetActive(false);
            coinbaseQRImage2.transform.parent.gameObject.SetActive(false);
            walletConnectQRImage2.transform.parent.gameObject.SetActive(false);
            backgroundObjects.SetActive(true);
            DebugConfigComponent.i.ShowWebviewScreen();
            //this.transform.localPosition = new Vector3(-0.009f, -1000.0613f, 1000.24f);
        }
    }
    void OnDestroy()
    {
        if (VRSettingsManager.I.OnSettingChanged.ContainsKey("openInternalBrowser"))
        {
            VRSettingsManager.I.OnSettingChanged["openInternalBrowser"] -= OnOpenInternalBrowserChanged;
        }
    }
}
