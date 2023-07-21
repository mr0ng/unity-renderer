using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Vuplex.WebView;

public class WebViewLoginController : MonoBehaviour
{
    public static WebViewLoginController I;
    protected IWebView webView;


    [SerializeField] protected BaseWebViewPrefab webViewPrefab;
    [SerializeField]private RawImage webviewImage;
    // private string[] buttonClickCodes = new string[]
    // {
    //     "document.querySelector('.LoginWalletItem .ui.huge.primary.button').click();",
    //     "document.querySelector('.LoginGuestItem .ui.huge.primary.button').click();"
    // };
    [SerializeField] protected Button guestLoginButton;
    [SerializeField] protected Button walletLoginButton;

    [SerializeField] private GameObject walletLoginPanel;
    [SerializeField] protected Button fortmaticLoginButton;
    [SerializeField] protected Button walletConnectLoginButton;
    [SerializeField] protected Button coinbaseLoginButton;
    [SerializeField] protected Button walletCloseButton;

    [SerializeField] private GameObject fortmaticLoginPanel;
    [SerializeField] private Button fortmaticPanelClose;
    [SerializeField] private GameObject walletConnectLoginPanel;
    [SerializeField] private Image walletConnectQRImage;
    [SerializeField] private Button walletConnectPanelClose;
    [SerializeField] private GameObject coinbaseLoginPanel;
    [SerializeField] private Image coinbaseQRImage;
    [SerializeField] private Button coinbasePanelClose;

    //[SerializeField] private TMP_Text LoginWarningText;

    public static string StudentName = "";
    // Start is called before the first frame update
    void Start()
    {
        if (I != null) return;
        I = this;
        CrossPlatformManager.SetCameraForGame();
        webView = webViewPrefab.WebView;
        CloseWalletLoginPanel();
        ShowMainLoginButtons();
        // Attach the functions to the buttons' onClick event
        guestLoginButton.onClick.AddListener(GuestClickButton);
        walletLoginButton.onClick.AddListener(WalletClickButton);
        walletCloseButton.onClick.AddListener(CloseWalletLoginPanel);
        fortmaticLoginButton.onClick.AddListener(FortmaticLoginClickButton);
        fortmaticPanelClose.onClick.AddListener(CloseFortmaticPanel);
        walletConnectLoginButton.onClick.AddListener(WalletConnectLoginClickButton);
        walletConnectPanelClose.onClick.AddListener(CloseWalletConnectPanel);
        coinbaseLoginButton.onClick.AddListener(CoinBaseLoginClickButton);
        coinbasePanelClose.onClick.AddListener(CloseCoinbaseLoginPanel);
    }

    public void GuestClickButton()
    {
        if (webView == null) webView = webViewPrefab.WebView;

        string jsCode = "document.querySelector('.LoginGuestItem .ui.huge.primary.button').click();";
        webView.ExecuteJavaScript(jsCode, (result) =>
        {
            // if (result == "Success")
            // {
            //     Debug.Log($"Guest Button click executed successfully");
            // }
            // else
            // {
            //     Debug.LogError($"Failed to execute button Guest click: {result}");
            // }
        });
        gameObject.SetActive(false);
    }

    public void WalletClickButton()
    {
        if (webView == null) webView = webViewPrefab.WebView;

        string jsCode = "document.querySelector('.LoginWalletItem .ui.huge.primary.button').click();";
        webView.ExecuteJavaScript(jsCode, (result) =>
        {
            // if (result == "Success")
            // {
            //     Debug.Log($"Button  click executed successfully");
            //
            // }
            // else
            // {
            //     Debug.LogError($"Failed to execute button  click: {result}");
            // }

        });
        walletLoginPanel.SetActive(true);
    }
    public void FortmaticLoginClickButton()
    {
        if (webView == null) webView = webViewPrefab.WebView;

        webView.ExecuteJavaScript($"document.querySelector('.dcl.option.fortmatic').click();", (result) =>
        {
            // if (result == "Success")
            // {
            //     Debug.Log($"Button  click executed successfully");
            //
            // }
            // else
            // {
            //     Debug.LogError($"Failed to execute button  click: {result}");
            // }

        });
        fortmaticLoginPanel.SetActive(true);

    }

    public void CloseFortmaticPanel()
    {
        webView.ExecuteJavaScript($"document.querySelector('#root > div > div > div > div > div > div.NavigationBar-component > div > div.right > div').click();", (result) =>
        {
            if (result == "Success")
            {
                Debug.Log($"Button  click executed successfully");

            }
            else
            {
                Debug.LogError($"Failed to execute button  click: {result}");
            }
        });
        fortmaticLoginPanel.SetActive(false);
    }
    public void CloseWalletConnectPanel()
    {
        webView.ExecuteJavaScript($"document.querySelector('body > wcm-modal:nth-child(19)').shadowRoot.querySelector('#wcm-modal > div > wcm-modal-backcard').shadowRoot.querySelector('div.wcm-toolbar > button').click();", (result) =>
        {
            if (result == "Success")
            {
                Debug.Log($"Button  click executed successfully");

            }
            else
            {
                Debug.LogError($"Failed to execute button  click: {result}");
            }
        });
        walletConnectLoginPanel.SetActive(false);
    }
    public void CloseCoinbaseLoginPanel()
    {
        webView.ExecuteJavaScript($"document.querySelector('html > div > div.-cbwsdk-link-flow-root > div > div.-cbwsdk-extension-dialog > div > button').click();", (result) =>
        {
            if (result == "Success")
            {
                Debug.Log($"Button  click executed successfully");

            }
            else
            {
                Debug.LogError($"Failed to execute button  click: {result}");
            }
        });
        coinbaseLoginPanel.SetActive(false);
    }
    public void WalletConnectLoginClickButton()
    {
        if (webView == null) webView = webViewPrefab.WebView;

        webView.ExecuteJavaScript($"document.querySelector('.dcl.option.wallet-connect').click();", (result) =>
        {
            if (result == "Success")
            {
                Debug.Log($"Button  click executed successfully");

            }
            else
            {
                Debug.LogError($"Failed to execute button  click: {result}");
            }
            walletConnectLoginPanel.SetActive(true);
            walletConnectQRImage.material = webviewImage.material;
        });
    }
    public void CoinBaseLoginClickButton()
    {
        if (webView == null) webView = webViewPrefab.WebView;

        webView.ExecuteJavaScript($"document.querySelector('.dcl.option.wallet-link').click();", (result) =>
        {
            if (result == "Success")
            {
                Debug.Log($"Button  click executed successfully");

            }
            else
            {
                Debug.LogError($"Failed to execute button  click: {result}");
            }
            walletConnectLoginPanel.SetActive(true);
            coinbaseQRImage.material = webviewImage.material;
        });
    }

    public void HideMainLoginButtons()
    {
        guestLoginButton.gameObject.SetActive(false);
        walletLoginButton.gameObject.SetActive(false);
    }
    public void ShowMainLoginButtons()
    {
        guestLoginButton.gameObject.SetActive(true);
        walletLoginButton.gameObject.SetActive(true);
    }

    public void CloseWalletLoginPanel()
    {
        if (webView == null) webView = webViewPrefab.WebView;
        if (webViewPrefab.WebView == null) return;
        // userName.text = "";
        // password.text = "";
        webView.ExecuteJavaScript($"document.querySelector('dcl modal-navigation-button modal-navigation-close').click();", (result) =>
        {
            if (result == "Success")
            {
                Debug.Log($"Button  click executed successfully");

            }
            else
            {
                Debug.LogError($"Failed to execute button  click: {result}");
            }
        });

        walletLoginPanel.SetActive(false);
    }






}

