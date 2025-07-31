#if UNITY_EDITOR_WIN
using UnityEditor;
using UnityEngine;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

public class PlayerPrefsManager : EditorWindow
{
    private Vector2 scrollPos;
    private readonly Dictionary<string, object> playerPrefsData = new();
    private string searchTerm = "";
    private string editValue = "";
    private string editingKey = null;
    private string editingType = null;

    private static readonly string[] defaultUnityKeys =
    {
        "Screenmanager Resolution Width",
        "Screenmanager Resolution Height",
        "Screenmanager Is Fullscreen mode",
        "UnityGraphicsQuality",
        "UnitySelectMonitor",
        "UnityVSyncCount",
        "UnityAnalytics",
        "UnityCloud",
        "UnityConnect",
        "unity.",
        "AddressablesRuntimeDataPath",
    };

    [MenuItem("Tools/PlayerPrefs Manager")]
    public static void ShowWindow()
    {
        var window = GetWindow<PlayerPrefsManager>("PlayerPrefs Manager");
        window.minSize = new Vector2(600, 400);
        window.maxSize = new Vector2(1200, 900);
    }

    private void OnEnable()
    {
        LoadPlayerPrefsFromRegistry();
    }

    private void LoadPlayerPrefsFromRegistry()
    {
        playerPrefsData.Clear();
        string companyName = PlayerSettings.companyName;
        string productName = PlayerSettings.productName;
        string regPath = $@"Software\Unity\UnityEditor\{companyName}\{productName}";

        try
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(regPath, true);
            if (key != null)
            {
                foreach (string valueName in key.GetValueNames())
                {
                    if (!defaultUnityKeys.Any(k => valueName.StartsWith(k, StringComparison.OrdinalIgnoreCase)))
                    {
                        playerPrefsData[valueName.Split('_').First()] = key.GetValue(valueName);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading PlayerPrefs: {ex.Message}");
        }

        editingKey = null;
        editingType = null;
        editValue = "";
        Repaint();
    }

    private void OnGUI()
    {
        GUIStyle headerStyle = new(EditorStyles.boldLabel) { fontSize = 16, padding = new RectOffset(10, 10, 5, 5) };
        GUIStyle buttonStyle = new(EditorStyles.miniButton) { fixedHeight = 22, fontSize = 11 };
        GUIStyle keyStyle = new(EditorStyles.label) { wordWrap = true, padding = new RectOffset(5, 5, 2, 2) };
        GUIStyle typeStyle = new(EditorStyles.label) { wordWrap = false, padding = new RectOffset(5, 5, 2, 2), alignment = TextAnchor.MiddleLeft };

        GUILayout.Space(10);
        GUILayout.Label("🎮 PlayerPrefs Manager", headerStyle);
        GUILayout.Space(8);

        GUILayout.BeginHorizontal();
        GUILayout.Label("🔍 Search:", EditorStyles.boldLabel, GUILayout.Width(60));
        searchTerm = EditorGUILayout.TextField(searchTerm, GUILayout.Height(20));
        if (GUILayout.Button("🔄 Refresh", buttonStyle, GUILayout.Width(80)))
        {
            LoadPlayerPrefsFromRegistry();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        float totalWidth = position.width - 20;
        float typeWidth = totalWidth * 0.15f;
        float keyWidth = totalWidth * 0.30f;
        float valueWidth = totalWidth * 0.35f;
        float actionsWidth = totalWidth * 0.20f;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Type", EditorStyles.boldLabel, GUILayout.Width(typeWidth));
        GUILayout.Label("Key", EditorStyles.boldLabel, GUILayout.Width(keyWidth));
        GUILayout.Label("Value", EditorStyles.boldLabel, GUILayout.Width(valueWidth));
        GUILayout.Label("Actions", EditorStyles.boldLabel, GUILayout.Width(actionsWidth));
        GUILayout.EndHorizontal();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height - 100));

        if (playerPrefsData.Count == 0)
        {
            GUILayout.Label("No custom PlayerPrefs found.", EditorStyles.centeredGreyMiniLabel);
        }
        else
        {
            var keys = playerPrefsData.Keys.OrderBy(k => k).ToList();
            foreach (var key in keys)
            {
                if (!string.IsNullOrEmpty(searchTerm) && !key.ToLower().Contains(searchTerm.ToLower()))
                    continue;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();

                GUILayout.Label(GetPrefType(playerPrefsData[key]), typeStyle, GUILayout.Width(typeWidth));
                GUILayout.Label("🔑 " + key, keyStyle, GUILayout.Width(keyWidth));

                if (editingKey == key)
                {
                    editValue = EditorGUILayout.TextField(editValue, GUILayout.Width(valueWidth));
                    InitEditState(editValue,playerPrefsData[key]);

                    GUILayout.BeginHorizontal(GUILayout.Width(actionsWidth));
                    GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
                    if (GUILayout.Button("✔ Save", buttonStyle, GUILayout.Width(actionsWidth / 2 - 5)))
                    {
                        PlayerPrefs.DeleteKey(key);
                        SaveValueInternal(key, editValue, editingType);
                    }
                    GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f);
                    if (GUILayout.Button("✖ Cancel", buttonStyle, GUILayout.Width(actionsWidth / 2 - 5)))
                    {
                        editingKey = null;
                        editingType = null;
                        editValue = "";
                    }
                    GUI.backgroundColor = Color.white;
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Label(GetValueDisplay(playerPrefsData[key]), keyStyle, GUILayout.Width(valueWidth));

                    GUILayout.BeginHorizontal(GUILayout.Width(actionsWidth));
                    GUI.backgroundColor = new Color(1f, 0.8f, 0.2f);
                    if (GUILayout.Button("✎ Edit", buttonStyle, GUILayout.Width(actionsWidth / 2 - 5)))
                    {
                        editingKey = key;
                        editingType = null;
                        editValue = "";
                    }
                    GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);
                    if (GUILayout.Button("🗑 Delete", buttonStyle, GUILayout.Width(actionsWidth / 2 - 5)))
                    {
                        // if (EditorUtility.DisplayDialog("Confirm Delete", $"Delete key '{key}'?", "Yes", "No"))
                        // {
                        //     PlayerPrefs.DeleteKey(key);
                        //     LoadPlayerPrefsFromRegistry();
                        // }
                        
                        PlayerPrefs.DeleteKey(key);
                        LoadPlayerPrefsFromRegistry();
                    }
                    GUI.backgroundColor = Color.white;
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUILayout.Space(4);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void InitEditState(string value,object type)
    {
        if (!string.IsNullOrEmpty(editingType)) return;

        switch (type)
        {
            case int or long:
                editingType = "int";
                editValue = value.ToString();
                break;
            case float:
                editingType = "float";
                editValue = value.ToString();
                break;
            case byte[] bytes:
                editingType = "string";
                try
                {
                    editValue = Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                    editValue = BitConverter.ToString(bytes).Replace("-", " ");
                }
                break;
            default:
                editingType = "string";
                editValue = value?.ToString() ?? "";
                break;
        }
    }

    private string GetValueDisplay(object value)
    {
        if (value is byte[] v)
        {
            try
            {
                return Encoding.UTF8.GetString(v);
            }
            catch
            {
                return BitConverter.ToString(v).Replace("-", " ");
            }
        }
        return value?.ToString() ?? "null";
    }

    private string GetPrefType(object value)
    {
        return value switch
        {
            int or long => "int",
            float => "float",
            byte[] => "string",
            _ => "string"
        };
    }

    private void SaveValueInternal(string key, string newValue, string type)
    {
        string companyName = PlayerSettings.companyName;
        string productName = PlayerSettings.productName;
        string regPath = $@"Software\Unity\UnityEditor\{companyName}\{productName}";

        try
        {
            using RegistryKey keyReg = Registry.CurrentUser.OpenSubKey(regPath, true) ?? Registry.CurrentUser.CreateSubKey(regPath);
            if (type == "int")
            {
                if (int.TryParse(newValue, out int intVal))
                {
                    PlayerPrefs.SetInt(key, intVal);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Input", "Value must be a valid integer (e.g., 123)!", "OK");
                    return;
                }
            }
            else if (type == "float")
            {
                if (float.TryParse(newValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatVal))
                {
                    PlayerPrefs.SetFloat(key, floatVal);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Input", "Value must be a valid float (e.g., 12.3)!", "OK");
                    return;
                }
            }
            else
            {
                PlayerPrefs.SetString(key, newValue);
            }

            PlayerPrefs.Save();
            Debug.Log($"Saved key: {key} with value: {newValue} (type: {type})");
            LoadPlayerPrefsFromRegistry();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving PlayerPrefs: {ex.Message}");
        }
    }
}
#endif