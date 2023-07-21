using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class SettingsData
{
    public Dictionary<string, string> settings = new Dictionary<string, string>();
}

public class VRSettingsManager : MonoBehaviour
{
    [SerializeField] private List<string> keys = new List<string>();
    [SerializeField] private List<string> defaultValues = new List<string>();
    public Dictionary<string, Action<string>> OnSettingChanged = new Dictionary<string, Action<string>>(); // Dictionary to hold the action for each key

    private string settingsPath;
    private SettingsData settingsData;
    public static VRSettingsManager I;

    private void Awake()
    {
        // Singleton pattern
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (I != this)
        {
            Destroy(gameObject);
            return;
        }
        settingsPath = Path.Combine(Application.persistentDataPath, "vrsettings.dat");
        settingsData = LoadSettings();

        // Initialize OnSettingChanged for each key
        foreach (var key in keys)
        {
            OnSettingChanged[key] = null;
        }
    }

    public void SaveSettings(Dictionary<string, string> newSettings)
    {
        // Create a copy of newSettings
        Dictionary<string, string> newSettingsCopy = new Dictionary<string, string>(newSettings);

        foreach (var setting in newSettingsCopy)
        {
            if (settingsData.settings.ContainsKey(setting.Key))
            {
                settingsData.settings[setting.Key] = setting.Value;
            }
            else
            {
                settingsData.settings.Add(setting.Key, setting.Value);
            }
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(settingsPath);
        bf.Serialize(file, settingsData);
        file.Close();
    }

    public SettingsData LoadSettings()
    {
        if (File.Exists(settingsPath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(settingsPath, FileMode.Open);
            SettingsData data = (SettingsData)bf.Deserialize(file);
            file.Close();

            return data;
        }
        else
        {
            // Create default settings if settings file doesn't exist.
            settingsData = new SettingsData();
            for (int i = 0; i < keys.Count; i++)
            {
                settingsData.settings.Add(keys[i], defaultValues[i]);
            }

            // Save the default settings.
            SaveSettings(settingsData.settings);

            return settingsData;
        }
    }

    // Get setting value by key
    // Get setting value by key
    public string GetSetting(string key)
    {
        if (settingsData.settings.ContainsKey(key))
        {
            return settingsData.settings[key];
        }

        return "";
    }

    public void SetSetting(string key, string value)
    {
        if (settingsData.settings.ContainsKey(key))
        {
            settingsData.settings[key] = value;
            if(!OnSettingChanged.ContainsKey(key))
            {
                OnSettingChanged.Add(key, null); // Add a null action for the new key
            }
            OnSettingChanged[key]?.Invoke(value); // Call the OnSettingChanged event for the given key
        }
        else
        {
            settingsData.settings.Add(key, value);
            if(!OnSettingChanged.ContainsKey(key))
            {
                OnSettingChanged.Add(key, null); // Add a null action for the new key
            }
            OnSettingChanged[key]?.Invoke(value); // Call the OnSettingChanged event for the given key
        }

        SaveSettings(settingsData.settings);
    }

}
