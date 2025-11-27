using UnityEditor;
using UnityEngine;
using UnityEditor.U2D;
using UnityEngine.U2D;
using System;
using System.Collections.Generic;


public class SpriteAtlasTool
{
    private UnityEngine.Object selectedObject;
    private string atlasName = "NewSpriteAtlas";
    private string savePath = "Assets/SpriteAtlases";

    private bool enableTightPacking = true;
    private bool enableRotation = false;
    private int padding = 4;
    private int blockOffset = 2;

    private bool readable = false;
    private bool generateMipMaps = false;
    private bool sRGB = true;
    private FilterMode filterMode = FilterMode.Trilinear;

    
    [Serializable]
    private class PlatformOverride
    {
        public BuildTargetGroup platform = BuildTargetGroup.Standalone;
        public bool overridden = false;
        public int maxTextureSize = 2048;
        public int compressionQuality = 50;
        public TextureImporterFormat format = TextureImporterFormat.Automatic;
    }
    private List<PlatformOverride> platformOverrides = new List<PlatformOverride>();
    private bool showPlatformSettings = true;

    
    public void DrawUI()
    {
        var origBg = GUI.backgroundColor;
        GUI.backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.1f,0.2f,0.3f) : new Color(0.8f,0.9f,1f);
        EditorGUILayout.BeginVertical("box");
        GUI.backgroundColor = origBg;

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Sprite Atlas Builder", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Select One GameObject or Sprite", EditorStyles.label);
        selectedObject = EditorGUILayout.ObjectField("Item", selectedObject, typeof(UnityEngine.Object), true);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);

        DrawAtlasSettingsGUI();
        EditorGUILayout.Space(5);

       

        showPlatformSettings = EditorGUILayout.Foldout(showPlatformSettings, "Platform Compression Overrides", true);
        if (showPlatformSettings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            for (int i = platformOverrides.Count - 1; i >= 0; i--)
            {
                var po = platformOverrides[i];
                EditorGUILayout.BeginHorizontal();
                po.overridden = EditorGUILayout.ToggleLeft("Override", po.overridden, GUILayout.Width(70));
                po.platform = (BuildTargetGroup)EditorGUILayout.EnumPopup(po.platform);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    platformOverrides.RemoveAt(i);
                    continue;
                }
                EditorGUILayout.EndHorizontal();

                if (po.overridden)
                {
                    EditorGUILayout.BeginVertical("box");
                    po.maxTextureSize = EditorGUILayout.IntPopup("Max Size", po.maxTextureSize,
                        new[] { "32","64","128","256","512","1024","2048","4096" },
                        new[] { 32,64,128,256,512,1024,2048,4096 });

                    po.format = (TextureImporterFormat)EditorGUILayout.EnumPopup("Format", po.format);

                    po.compressionQuality = EditorGUILayout.IntSlider("Quality", po.compressionQuality, 0, 100);
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.Space(3);
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Override", GUILayout.Height(22))) platformOverrides.Add(new PlatformOverride());
            if (GUILayout.Button("Clear Overrides", GUILayout.Height(22))) platformOverrides.Clear();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Save Path", EditorStyles.boldLabel);
        atlasName = EditorGUILayout.TextField("Atlas Name", atlasName);
        savePath = EditorGUILayout.TextField("Path", savePath);
        if (GUILayout.Button("Select Folder", GUILayout.Width(120)))
        {
            string selectedFolder = EditorUtility.OpenFolderPanel("Select Save Folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selectedFolder))
            {
                if (selectedFolder.StartsWith(Application.dataPath))
                {
                    savePath = "Assets" + selectedFolder.Substring(Application.dataPath.Length).Replace("\\", "/");
                }
                else
                {
                    Debug.LogWarning("Selected folder must be inside the Assets directory.");
                }
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);

        if (GUILayout.Button("Create Sprite Atlas", GUILayout.Height(30))) CreateSpriteAtlas();

        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }

    private void DrawAtlasSettingsGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Atlas Settings", EditorStyles.boldLabel);

        EditorGUILayout.Space(1);

        EditorGUILayout.LabelField("Packing Settings", EditorStyles.miniBoldLabel);
        enableTightPacking = EditorGUILayout.Toggle("Tight Packing", enableTightPacking);
        enableRotation = EditorGUILayout.Toggle("Allow Rotation", enableRotation);
        padding = EditorGUILayout.IntSlider("Padding", padding, 0, 32);
        blockOffset = EditorGUILayout.IntSlider("Block Offset", blockOffset, 0, 16);

        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Texture Settings", EditorStyles.miniBoldLabel);
        readable = EditorGUILayout.Toggle("Readable", readable);
        generateMipMaps = EditorGUILayout.Toggle("Generate MipMaps", generateMipMaps);
        sRGB = EditorGUILayout.Toggle("sRGB (Color Space)", sRGB);
        filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", filterMode);

        EditorGUILayout.EndVertical();
    }

    private void CreateSpriteAtlas()
    {
        if (selectedObject == null)
        {
            Debug.LogWarning("No object selected for atlas creation.");
            return;
        }

        var sprites = CollectSpritesFrom(selectedObject);
        if (sprites.Count == 0)
        {
            Debug.LogWarning("Selected item contains no sprites.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(savePath))
        {
            Debug.LogWarning($"Invalid save path: {savePath}. Creating default folder.");
            AssetDatabase.CreateFolder("Assets", "SpriteAtlases");
            savePath = "Assets/SpriteAtlases";
        }

        string path = $"{savePath}/{atlasName}.spriteatlas";
        var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path) ?? new SpriteAtlas();
        AssetDatabase.CreateAsset(atlas, path);

        atlas.SetPackingSettings(new SpriteAtlasPackingSettings
        {
            enableTightPacking = enableTightPacking,
            enableRotation = enableRotation,
            padding = padding,
            blockOffset = blockOffset
        });
        atlas.SetTextureSettings(new SpriteAtlasTextureSettings
        {
            readable = readable,
            generateMipMaps = generateMipMaps,
            sRGB = sRGB,
            filterMode = filterMode
        });

        foreach (var po in platformOverrides)
        {
            if (!po.overridden) continue;
            atlas.SetPlatformSettings(new TextureImporterPlatformSettings
            {
                name = po.platform.ToString(),
                overridden = true,
                maxTextureSize = po.maxTextureSize,
                format = po.format,
                compressionQuality = po.compressionQuality
            });
        }

        atlas.Remove(atlas.GetPackables());
        atlas.Add(sprites.ToArray());

        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(atlas);
        Debug.Log($"Sprite Atlas '{atlasName}' created at: {path}");
    }

    private List<Sprite> CollectSpritesFrom(UnityEngine.Object obj)
    {
        var list = new List<Sprite>();
        if (obj is GameObject go)
        {
            foreach (var sr in go.GetComponentsInChildren<SpriteRenderer>(true))
                if (sr.sprite != null && !list.Contains(sr.sprite)) list.Add(sr.sprite);
            foreach (var ui in go.GetComponentsInChildren<UnityEngine.UI.Image>(true))
                if (ui.sprite != null && !list.Contains(ui.sprite)) list.Add(ui.sprite);
        }
        else if (obj is Sprite sp)
        {
            list.Add(sp);
        }
        return list;
    }
}
