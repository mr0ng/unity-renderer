using DCL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

[Serializable]
public class SettingsData
{
    public Dictionary<string, string> settings = new Dictionary<string, string>();
    public Dictionary<string, string> defaultSettings = new Dictionary<string, string>(); // for default values
    public Dictionary<string, bool> encryptedKeys = new Dictionary<string, bool>(); // Keep track of encrypted keys
}

public class Crypto
{
    private static readonly byte[] key = Encoding.UTF8.GetBytes("k1#90LknL0z3IB87");
    private static readonly byte[] iv = Encoding.UTF8.GetBytes("m2Kor9gRbx90Pwi9");

    public static string Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            ICryptoTransform encryptor = aes.CreateEncryptor(key, iv);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs)) { sw.Write(plainText); }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }

    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText) || !IsBase64String(cipherText))
        {
            Debug.LogWarning("Invalid cipher text provided for decryption.");
            return null;
        }

        try
        {
            using (Aes aes = Aes.Create())
            {
                ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs)) { return sr.ReadToEnd(); }
                    }
                }
            }
        }
        catch (FormatException e)
        {
            Debug.LogError($"Error decrypting text: {e}");
            return null;
        }
    }

    public static bool IsBase64String(string s)
    {
        s = s.Trim();
        return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
    }
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

        if (settingsData == null)
        {
            settingsPath = Path.Combine(Application.persistentDataPath, "vrsettings1.2.dat");
            settingsData = LoadSettings();
        }

        // Initialize OnSettingChanged for each key
        foreach (var key in keys) { OnSettingChanged[key] = null; }
    }

    public bool IsSettingEncrypted(string key)
    {
        if (string.IsNullOrEmpty(key) || settingsData.encryptedKeys == null || !settingsData.encryptedKeys.ContainsKey(key))
        {
            return false; // Default to not encrypted
        }

        return settingsData.encryptedKeys[key];
    }

    public void SaveSettings(Dictionary<string, string> newSettings, bool encrypt = false)
    {
        // Create a copy of newSettings
        Dictionary<string, string> newSettingsCopy = new Dictionary<string, string>(newSettings);

        foreach (var setting in newSettingsCopy)
        {
            string value = setting.Value;

            if (encrypt)
            {
                value = Crypto.Encrypt(value);

                if (settingsData.encryptedKeys.ContainsKey(setting.Key)) { settingsData.encryptedKeys[setting.Key] = true; }
                else { settingsData.encryptedKeys.Add(setting.Key, true); }
            }

            if (settingsData.settings.ContainsKey(setting.Key)) { settingsData.settings[setting.Key] = value; }
            else { settingsData.settings.Add(setting.Key, value); }

            if (!settingsData.defaultSettings.ContainsKey(setting.Key))
            {
                settingsData.defaultSettings.Add(setting.Key, setting.Value); // New: Save default value
            }
        }

        string jsonData = JsonConvert.SerializeObject(settingsData);
        File.WriteAllText(settingsPath, jsonData);
    }

    public SettingsData LoadSettings()
    {
        try
        {
            if (File.Exists(settingsPath))
            {
                string jsonData = File.ReadAllText(settingsPath);
                SettingsData data = JsonConvert.DeserializeObject<SettingsData>(jsonData);

                // Ensure that encryptedKeys is initialized, even if missing from file
                if (data.encryptedKeys == null) { data.encryptedKeys = new Dictionary<string, bool>(); }

                foreach (var key in data.encryptedKeys.Keys)
                {
                    if (data.encryptedKeys[key])
                    {
                        string encryptedValue = data.settings[key];

                        try
                        {
                            string decryptedValue =
                                Crypto.Decrypt(encryptedValue); // Decrypt individual values if encrypted

                            data.settings[key] = encryptedValue;
                        }
                        catch (CryptographicException ex)
                        {
                            Debug.LogError($"Error decrypting value for key {key} : {ex.Message}");
                            data.settings[key] = Crypto.Encrypt("");
                        }
                    }
                }

                return data;
            }
            else
            {
                // Create default settings if settings file doesn't exist.
                settingsData = new SettingsData();
                for (int i = 0; i < keys.Count; i++)
                {
                    settingsData.settings.Add(keys[i], defaultValues[i]);
                    settingsData.defaultSettings.Add(keys[i], defaultValues[i]); // New: Save default value
                }

                SaveSettings(settingsData.settings);
                return settingsData;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading settings: {e}");
            settingsData = new SettingsData();

            // Save the default settings.
            SaveSettings(settingsData.settings);

            return settingsData;
        }
    }

    // Get setting value by key
    // Get setting value by key
    public string GetSetting(string key)
    {
        if (settingsData == null) { settingsData = LoadSettings(); }

        if (settingsData.settings.ContainsKey(key))
        {
            string value = settingsData.settings[key];

            if (IsSettingEncrypted(key))
            {
                value = Crypto.Decrypt(value); // Decrypt if it's encrypted
            }

            return value;
        }

        return "";
    }

    public T GetSetting<T>(string key, T defaultValue)
    {
        string stringValue = GetSetting(key); // Your existing GetSetting method

        if (string.IsNullOrEmpty(stringValue))
        {
            SetSetting(key, defaultValue.ToString());
            return defaultValue;
        }

        if (typeof(T).IsEnum)
        {
            if (Enum.TryParse(typeof(T), stringValue, out object parsedEnum)) { return (T)parsedEnum; }
            else
            {
                Debug.LogWarning($"Invalid enum value for {key}. Setting to default.");
                SetSetting(key, defaultValue.ToString());
                return defaultValue;
            }
        }
        else if (typeof(T) == typeof(bool))
        {
            if (bool.TryParse(stringValue, out bool parsedBool)) { return (T)(object)parsedBool; }
        }
        else if (typeof(T) == typeof(int))
        {
            if (int.TryParse(stringValue, out int parsedInt)) { return (T)(object)parsedInt; }
        }
        else if (typeof(T) == typeof(Vector2))
        {
            stringValue = stringValue.Trim('(', ')'); // Remove parentheses
            string[] coords = stringValue.Split(',');

            if (coords.Length != 2)
            {
                Debug.LogWarning($"Invalid Vector2 value for {key}. Setting to default.");
                SetSetting(key, defaultValue.ToString());
                return defaultValue;
            }
            else
            {
                if (float.TryParse(coords[0].Trim(), out float x) && float.TryParse(coords[1].Trim(), out float y)) { return (T)(object)new Vector2(x, y); }
                else
                {
                    Debug.LogWarning($"Failed to parse Vector2 values for {key}. Setting to default.");
                    SetSetting(key, defaultValue.ToString());
                    return defaultValue;
                }
            }
        }
        else if (typeof(T) == typeof(string)) { return (T)(object)stringValue; }

        // ... handle other types

        Debug.LogWarning($"Invalid value for {key}. Setting to default.");
        SetSetting(key, defaultValue.ToString());
        return defaultValue;
    }

    public void SetSetting(string key, string value, bool encrypt = false)
    {
        if (encrypt)
        {
            value = Crypto.Encrypt(value);

            if (settingsData.encryptedKeys.ContainsKey(key)) { settingsData.encryptedKeys[key] = true; }
            else { settingsData.encryptedKeys.Add(key, true); }
        }

        if (settingsData.settings.ContainsKey(key))
        {
            settingsData.settings[key] = value;

            if (!OnSettingChanged.ContainsKey(key))
            {
                OnSettingChanged.Add(key, null); // Add a null action for the new key
            }

            OnSettingChanged[key]?.Invoke(value); // Call the OnSettingChanged event for the given key
        }
        else
        {
            settingsData.settings.Add(key, value);

            if (!OnSettingChanged.ContainsKey(key))
            {
                OnSettingChanged.Add(key, null); // Add a null action for the new key
            }

            OnSettingChanged[key]?.Invoke(value); // Call the OnSettingChanged event for the given key
        }

        SaveSettings(settingsData.settings);
    }

    public void ClearSettings()
    {
        // Check if the file exists
        if (File.Exists(settingsPath))
        {
            // Delete the file
            File.Delete(settingsPath);
            Debug.Log($"Settings file deleted: {settingsPath}");
        }
        else { Debug.LogWarning($"Settings file not found: {settingsPath}"); }
        settingsData.settings = new Dictionary<string, string>(settingsData.defaultSettings);
        SaveSettings(settingsData.settings);
        DebugConfigComponent.i.LoadSettings();
    }
}
