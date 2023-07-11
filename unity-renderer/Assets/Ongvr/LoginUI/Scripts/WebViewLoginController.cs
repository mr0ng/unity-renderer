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
    private IWebView webView;


    [SerializeField] private BaseWebViewPrefab webViewPrefab;
    [SerializeField]private RawImage webviewImage;
    // private string[] buttonClickCodes = new string[]
    // {
    //     "document.querySelector('.LoginWalletItem .ui.huge.primary.button').click();",
    //     "document.querySelector('.LoginGuestItem .ui.huge.primary.button').click();"
    // };
    [SerializeField] private Button guestLoginButton;
    [SerializeField] private Button walletLoginButton;

    [SerializeField] private GameObject WalletLoginPanel;
    [SerializeField] private Button FortmaticLoginButton;
    [SerializeField] private Button WalletConnectLoginButton;

    [SerializeField] private GameObject FortmaticLoginPanel;

    [SerializeField] private GameObject WalletConnectLoginPanel;
    [SerializeField] private Image QRImage;

    //[SerializeField] private Button togglePasswordButton;

   // [SerializeField] private TMP_InputField userName;
    //[SerializeField] private TMP_InputField password;




    //[SerializeField] private TMP_Text LoginWarningText;

    public static string StudentName = "";
    // Start is called before the first frame update
    void Start()
    {
        if (I != null) return;
        I = this;

        webView = webViewPrefab.WebView;
        CloseWalletLoginPanel();
        //LoginWarningText.gameObject.SetActive(false);

        // Attach the functions to the buttons' onClick event
        guestLoginButton.onClick.AddListener(GuestClickButton);
        walletLoginButton.onClick.AddListener(WalletClickButton);
        FortmaticLoginButton.onClick.AddListener(FortmaticLoginClickButton);
        WalletConnectLoginButton.onClick.AddListener(WalletConnectLoginClickButton);

    }

    // Update is called once per frame
    void Update()
    {
        // Disable the button if either username or password is empty

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
        WalletLoginPanel.SetActive(true);
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
        FortmaticLoginPanel.SetActive(true);

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
            WalletConnectLoginPanel.SetActive(true);
            QRImage.material = webviewImage.material;
        });
    }

    public void CloseWalletLoginPanel()
    {
        // userName.text = "";
        // password.text = "";
        WalletLoginPanel.SetActive(false);
    }






}

