using DCL;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VRSettingsUIManager : MonoBehaviour
{
    [SerializeField] private Toggle openInternalBrowserToggle;
    [SerializeField] private Toggle useNewUIToggle;
    [SerializeField] private TMP_Dropdown baseUrlModeDropdown;
    [SerializeField] private TMP_InputField startInCoordsInputFieldx;
    [SerializeField] private TMP_InputField startInCoordsInputFieldy;
    [SerializeField] private Toggle disableGLTFDownloadThrottleToggle;
    [SerializeField] private Toggle multithreadedToggle;
    [SerializeField] private TMP_Dropdown networkDropdown;
    [SerializeField] private Toggle OpenBrowserOnStartToggle;
    [SerializeField] private Toggle webSocketSSLToggle;
    [SerializeField] private TMP_InputField kernelVersionInputField;
    [SerializeField] private Toggle useCustomContentServerToggle;
    [SerializeField] private TMP_InputField customContentServerUrlInputField;
    [SerializeField] private TMP_InputField realmInputField;
    [SerializeField] private TMP_InputField catalystInputField;

    // public Toggle enableTutorialToggle;
    // public Toggle builderInWorldToggle;
    [SerializeField] private Toggle soloSceneToggle;
    [SerializeField] private Toggle disableAssetBundlesToggle;
    [SerializeField] private Toggle enableDebugModeToggle;
    [SerializeField] private TMP_Dropdown debugPanelModeDropdown;
    [SerializeField] private Button resetSettingsButton;
    [SerializeField] private Button restartAppButton;

    void Start()
    {
        InitializeDropdownWithEnum<DebugConfigComponent.BaseUrl>(baseUrlModeDropdown);
        InitializeDropdownWithEnum<DebugConfigComponent.Network>(networkDropdown);
        InitializeDropdownWithEnum<DebugConfigComponent.DebugPanel>(debugPanelModeDropdown);

        StartCoroutine(SetUIFromSettings());

        // Set up listeners
        openInternalBrowserToggle.onValueChanged.AddListener(value => { VRSettingsManager.I.SetSetting("openInternalBrowser", value.ToString()); });
        useNewUIToggle.onValueChanged.AddListener(value => { VRSettingsManager.I.SetSetting("useNewUI", value.ToString()); });
        baseUrlModeDropdown.onValueChanged.AddListener(value => { VRSettingsManager.I.SetSetting("baseUrlMode", Enum.GetName(typeof(DebugConfigComponent.BaseUrl), value)); });
        startInCoordsInputFieldx.onEndEdit.AddListener(value => { VRSettingsManager.I.SetSetting("startInCoords", $"{startInCoordsInputFieldx.text},{startInCoordsInputFieldy.text}"); });
        startInCoordsInputFieldy.onEndEdit.AddListener(value => { VRSettingsManager.I.SetSetting("startInCoords", $"{startInCoordsInputFieldx.text},{startInCoordsInputFieldy.text}"); });
        disableGLTFDownloadThrottleToggle.onValueChanged.AddListener(value => { VRSettingsManager.I.SetSetting("disableGLTFDownloadThrottle", value.ToString()); });
        multithreadedToggle.onValueChanged.AddListener(value => { VRSettingsManager.I.SetSetting("multithreaded", value.ToString()); });
        networkDropdown.onValueChanged.AddListener(value => { VRSettingsManager.I.SetSetting("network", Enum.GetName(typeof(DebugConfigComponent.Network), value)); });
        OpenBrowserOnStartToggle.onValueChanged.AddListener(value => { VRSettingsManager.I.SetSetting("OpenBrowserOnStart", value.ToString()); });
        webSocketSSLToggle.onValueChanged.AddListener(value => { VRSettingsManager.I.SetSetting("webSocketSSL", value.ToString()); });
        useCustomContentServerToggle.onValueChanged.AddListener(value => { VRSettingsManager.I.SetSetting("useCustomContentServer", value.ToString()); });
        customContentServerUrlInputField.onEndEdit.AddListener(value => { VRSettingsManager.I.SetSetting("customContentServerUrl", value); });
        realmInputField.onEndEdit.AddListener(value => { VRSettingsManager.I.SetSetting("realm", value); });
        catalystInputField.onEndEdit.AddListener(value => { VRSettingsManager.I.SetSetting("catalyst", value); });

        // enableTutorialToggle.onValueChanged.AddListener(value => { VRSettingsManager.I.SetSetting("enableTutorial", value.ToString()); });
        // builderInWorldToggle.onValueChanged.AddListener(value => { VRSettingsManager.I.SetSetting("builderInWorld", value.ToString()); });
        soloSceneToggle.onValueChanged.AddListener(value => { VRSettingsManager.I.SetSetting("soloScene", value.ToString()); });
        disableAssetBundlesToggle.onValueChanged.AddListener(value => { VRSettingsManager.I.SetSetting("disableAssetBundles", value.ToString()); });
        enableDebugModeToggle.onValueChanged.AddListener(value => { VRSettingsManager.I.SetSetting("enableDebugMode", value.ToString()); });
        debugPanelModeDropdown.onValueChanged.AddListener(value => { VRSettingsManager.I.SetSetting("debugPanelMode", Enum.GetName(typeof(DebugConfigComponent.DebugPanel), value)); });
        resetSettingsButton.onClick.AddListener(() => {
            VRSettingsManager.I.ClearSettings();
            StartCoroutine(SetUIFromSettings());
        });
        restartAppButton.onClick.AddListener(() => {
            Application.Quit();
        });
        // ... setup other listeners here
    }

    public IEnumerator SetUIFromSettings()
    {
        while (!DebugConfigComponent.i.vrSettingsLoaded)
            yield return null;

        // Boolean toggles
        openInternalBrowserToggle.isOn = DebugConfigComponent.i.openInternalBrowser;
        useNewUIToggle.isOn = VRSettingsManager.I.GetSetting("useNewUI", useNewUIToggle.isOn);
        disableGLTFDownloadThrottleToggle.isOn = DebugConfigComponent.i.disableGLTFDownloadThrottle;
        multithreadedToggle.isOn = DebugConfigComponent.i.multithreaded;
        OpenBrowserOnStartToggle.isOn = DebugConfigComponent.i.OpenBrowserOnStart;
        webSocketSSLToggle.isOn = DebugConfigComponent.i.webSocketSSL;
        useCustomContentServerToggle.isOn = DebugConfigComponent.i.useCustomContentServer;

        // enableTutorialToggle.isOn = DebugConfigComponent.i.enableTutorial;
        // builderInWorldToggle.isOn = DebugConfigComponent.i.builderInWorld;
        soloSceneToggle.isOn = DebugConfigComponent.i.soloScene;
        disableAssetBundlesToggle.isOn = DebugConfigComponent.i.disableAssetBundles;
        enableDebugModeToggle.isOn = DebugConfigComponent.i.enableDebugMode;

        // Enum dropdowns
        baseUrlModeDropdown.value = (int)DebugConfigComponent.i.baseUrlMode; // Assuming baseUrlMode is an enum
        networkDropdown.value = (int)DebugConfigComponent.i.network; // Assuming network is an enum
        debugPanelModeDropdown.value = (int)DebugConfigComponent.i.debugPanelMode; // Assuming debugPanelMode is an enum

        // Vector2 input fields
        startInCoordsInputFieldx.text = DebugConfigComponent.i.startInCoords.x.ToString();
        startInCoordsInputFieldy.text = DebugConfigComponent.i.startInCoords.y.ToString();

        // String input fields
        kernelVersionInputField.text = DebugConfigComponent.i.kernelVersion;
        customContentServerUrlInputField.text = DebugConfigComponent.i.customContentServerUrl;
        realmInputField.text = DebugConfigComponent.i.realm;
        catalystInputField.text = DebugConfigComponent.i.catalyst;
    }

    // Update is called once per frame

    public static void InitializeDropdownWithEnum<T>(TMP_Dropdown dropdown)
    {
        string[] enumNames = Enum.GetNames(typeof(T));
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        foreach (string name in enumNames) { options.Add(new TMP_Dropdown.OptionData(name)); }

        dropdown.options.Clear();
        dropdown.options.AddRange(options);
        dropdown.RefreshShownValue();
    }
}
