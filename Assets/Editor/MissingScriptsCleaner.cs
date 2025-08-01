using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class MissingScriptsCleaner : EditorWindow
{
    private string _log = "";
    private Vector2 _mainScrollPosition;
    private Vector2 _sceneScrollPosition;
    private Vector2 _prefabScrollPosition;
    private Vector2 _logScrollPosition;
    private string _targetFolder;
    private bool _includeAllFolders;
    private bool _showDescription;
    private bool _targetFolderMode;
    private bool _targetSceneMode;
    private bool _showLog;
    private string[] _allScenePaths = new string[0];
    private bool[] _sceneSelectionStates = new bool[0];
    private string[] _allPrefabPaths = new string[0];
    private bool[] _prefabSelectionStates = new bool[0];
    private string _logFilePath = "Assets/MissingScriptsLog.txt";
    private string _sceneSearchQuery = "";
    private string _prefabSearchQuery = "";
    private bool _selectAllPrefabs;
    private bool _selectAllScenes;

    private static class Styles
    {
        public static readonly GUIStyle HeaderStyle = new(EditorStyles.boldLabel)
        {
            fontSize = 16,
            margin = new RectOffset(10, 10, 5, 5)
        };
        public static readonly GUIStyle SectionStyle = new (EditorStyles.helpBox)
        {
            margin = new RectOffset(5, 5, 5, 5),
            padding = new RectOffset(10, 10, 10, 10)
        };
        public static readonly GUIStyle ButtonStyle = new (GUI.skin.button)
        {
            fontSize = 14,
            padding = new RectOffset(10, 10, 8, 8),
            normal = { textColor = Color.white },
            hover = { textColor = new Color(0.9f, 0.9f, 0.9f) }
        };
        public static readonly GUIStyle SearchFieldStyle = new (EditorStyles.textField)
        {
            fontSize = 12,
            margin = new RectOffset(5, 5, 5, 5)
        };
        public static readonly Color FoldoutBgColor = new (0.15f, 0.15f, 0.15f, 0.5f);
    }

    [MenuItem("Tools/Missing Scripts Cleaner")]
    public static void ShowWindow()
    {
        var window = GetWindow<MissingScriptsCleaner>("🧹 Missing Scripts Cleaner");
        window.minSize = new Vector2(600, 500);
    }

    private void OnEnable()
    {
        LoadAllScenes();
    }

    private void LoadAllScenes()
    {
        var guids = AssetDatabase.FindAssets("t:Scene");
        _allScenePaths = guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();
        _sceneSelectionStates = new bool[_allScenePaths.Length];
    }

    private void LoadPrefabsInFolder(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            _allPrefabPaths = new string[0];
            _prefabSelectionStates = new bool[0];
            return;
        }

        var folders = _includeAllFolders
            ? AssetDatabase.GetSubFolders(folderPath).Prepend(folderPath).ToArray()
            : new[] { folderPath };

        var filter = string.IsNullOrEmpty(_prefabSearchQuery) ? "t:GameObject" : $"t:GameObject {_prefabSearchQuery}";
        var guids = AssetDatabase.FindAssets(filter, folders);
        _allPrefabPaths = guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();
        _prefabSelectionStates = new bool[_allPrefabPaths.Length];

        if (!_includeAllFolders)
        {
            _allPrefabPaths = _allPrefabPaths.Where(path => path.StartsWith(folderPath + "/") && path.LastIndexOf('/') == folderPath.Length).ToArray();
            _prefabSelectionStates = new bool[_allPrefabPaths.Length];
        }

        for (int i = 0; i < _prefabSelectionStates.Length; i++)
            _prefabSelectionStates[i] = _selectAllPrefabs;
    }

    private void OnGUI()
    {
        EditorGUIUtility.labelWidth = 160;
        _mainScrollPosition = EditorGUILayout.BeginScrollView(_mainScrollPosition, GUILayout.ExpandHeight(true));

        GUILayout.Space(10);
        DrawHeader();
        GUILayout.Space(15);
        DrawFolderSection();
        GUILayout.Space(15);
        DrawSceneSection();
        GUILayout.Space(15);
        DrawLogSection();
        GUILayout.Space(10);

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        using (new EditorGUILayout.VerticalScope(Styles.SectionStyle))
        {
            EditorGUILayout.LabelField("🧹 Missing Scripts Cleaner", Styles.HeaderStyle);
            GUILayout.Space(5);
            GUI.color = Color.cyan;
            _showDescription = EditorGUILayout.Foldout(_showDescription, "ℹ️ About This Tool", true);
            GUI.color = Color.white;
            if (_showDescription)
            {
                EditorGUILayout.LabelField(
                    "Effortlessly remove missing script references from:\n\n" +
                    "• Prefabs in specified folders (including subfolders)\n" +
                    "• Scenes (selected or all in project)\n\n" +
                    "All actions are logged and saved automatically to a text file.",
                    EditorStyles.wordWrappedLabel);
            }
        }
    }

    private void DrawFolderSection()
    {
        using (new EditorGUILayout.VerticalScope(Styles.SectionStyle))
        {
            EditorGUILayout.LabelField("📁 Clean Prefabs in Folders", EditorStyles.boldLabel);
            GUILayout.Space(8);

            bool prevTargetFolderMode = _targetFolderMode;
            _targetFolderMode = EditorGUILayout.ToggleLeft("Clean Specific Folder", _targetFolderMode, EditorStyles.boldLabel);

            if (_targetFolderMode)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUI.color = Color.cyan;
                    string newFolder = EditorGUILayout.TextField("Target Folder", _targetFolder);
                    GUI.color = Color.white;
                    if (GUILayout.Button("Browse", GUILayout.Width(80)))
                    {
                        var selected = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
                        if (!string.IsNullOrEmpty(selected) && selected.StartsWith(Application.dataPath))
                            newFolder = "Assets" + selected[Application.dataPath.Length..];
                    }
                    if (newFolder != _targetFolder)
                    {
                        _targetFolder = newFolder;
                        _selectAllPrefabs = false;
                        LoadPrefabsInFolder(_targetFolder);
                    }
                }

                bool prevIncludeAllFolders = _includeAllFolders;
                _includeAllFolders = EditorGUILayout.Toggle("Include Subfolders", _includeAllFolders);
                if (prevIncludeAllFolders != _includeAllFolders || prevTargetFolderMode != _targetFolderMode)
                {
                    _selectAllPrefabs = false;
                    LoadPrefabsInFolder(_targetFolder);
                }
                GUILayout.Space(5);

                if (!string.IsNullOrEmpty(_targetFolder))
                {
                    GUI.color = Color.cyan;
                    string newSearchQuery = EditorGUILayout.TextField("Search Prefabs", _prefabSearchQuery);
                    GUI.color = Color.white;

                    if (newSearchQuery != _prefabSearchQuery)
                    {
                        _prefabSearchQuery = newSearchQuery;
                        _selectAllPrefabs = false;
                        LoadPrefabsInFolder(_targetFolder);
                    }
                    GUILayout.Space(5);

                    if (_allPrefabPaths.Length > 0)
                    {
                        bool prevSelectAllPrefabs = _selectAllPrefabs;
                        _selectAllPrefabs = EditorGUILayout.ToggleLeft("Select All", _selectAllPrefabs);
                        if (prevSelectAllPrefabs != _selectAllPrefabs)
                        {
                            for (int i = 0; i < _prefabSelectionStates.Length; i++)
                                _prefabSelectionStates[i] = _selectAllPrefabs;
                        }

                        _prefabScrollPosition = EditorGUILayout.BeginScrollView(_prefabScrollPosition, GUILayout.Height(100));
                        for (int i = 0; i < _allPrefabPaths.Length; i++)
                        {
                            bool prevState = _prefabSelectionStates[i];
                            _prefabSelectionStates[i] = EditorGUILayout.ToggleLeft(_allPrefabPaths[i], _prefabSelectionStates[i]);
                            if (prevState != _prefabSelectionStates[i])
                            {
                                _selectAllPrefabs = _prefabSelectionStates.All(state => state);
                            }
                        }
                        EditorGUILayout.EndScrollView();
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No prefabs found matching the search query.", EditorStyles.wordWrappedLabel);
                    }

                    GUILayout.Space(10);

                    GUI.enabled = _prefabSelectionStates.Any(state => state) && !string.IsNullOrEmpty(_targetFolder);
                    if (GUILayout.Button("🧼 Clean Selected Prefabs", Styles.ButtonStyle))
                    {
                        _log = "";
                        var selectedPrefabs = _allPrefabPaths.Where((prefab, index) => _prefabSelectionStates[index]).ToArray();
                        var folders = _includeAllFolders
                            ? AssetDatabase.GetSubFolders(_targetFolder).Prepend(_targetFolder)
                            : new[] { _targetFolder };

                        foreach (var folder in folders)
                            CleanFolder(folder, selectedPrefabs);

                        SaveLogToFile();
                    }
                    GUI.enabled = true;
                }
                else
                {
                    EditorGUILayout.LabelField("Please select a folder to display prefabs.", EditorStyles.wordWrappedLabel);
                }
            }
            else
            {
                if (GUILayout.Button("🧼 Clean All Folders in Assets", Styles.ButtonStyle))
                {
                    _log = "";
                    foreach (var folder in AssetDatabase.GetSubFolders("Assets"))
                        CleanFolder(folder, null);

                    SaveLogToFile();
                }
            }
        }
    }

    private void DrawSceneSection()
    {
        using (new EditorGUILayout.VerticalScope(Styles.SectionStyle))
        {
            EditorGUILayout.LabelField("🎬 Clean Scenes", EditorStyles.boldLabel);
            GUILayout.Space(8);

            _targetSceneMode = EditorGUILayout.ToggleLeft("Clean Specific Scene(s)", _targetSceneMode, EditorStyles.boldLabel);

            if (_targetSceneMode)
            {
                GUI.color = Color.cyan;
                _sceneSearchQuery = EditorGUILayout.TextField("Search Scenes", _sceneSearchQuery, Styles.SearchFieldStyle);
                GUI.color = Color.white;
                GUILayout.Space(5);

                bool prevSelectAllScenes = _selectAllScenes;
                _selectAllScenes = EditorGUILayout.ToggleLeft("Select All", _selectAllScenes);
                if (prevSelectAllScenes != _selectAllScenes)
                {
                    for (int i = 0; i < _sceneSelectionStates.Length; i++)
                        _sceneSelectionStates[i] = _selectAllScenes;
                }

                _sceneScrollPosition = EditorGUILayout.BeginScrollView(_sceneScrollPosition, GUILayout.Height(150), GUILayout.ExpandWidth(true));
                for (int i = 0; i < _allScenePaths.Length; i++)
                {
                    if (string.IsNullOrEmpty(_sceneSearchQuery) || _allScenePaths[i].ToLower().Contains(_sceneSearchQuery.ToLower()))
                    {
                        bool prevState = _sceneSelectionStates[i];
                        _sceneSelectionStates[i] = EditorGUILayout.ToggleLeft(_allScenePaths[i], _sceneSelectionStates[i]);
                        if (prevState != _sceneSelectionStates[i])
                        {
                            _selectAllScenes = _sceneSelectionStates.All(state => state);
                        }
                    }
                }
                EditorGUILayout.EndScrollView();

                GUILayout.Space(10);

                GUI.enabled = _sceneSelectionStates.Any(state => state);
                if (GUILayout.Button("🧼 Clean Selected Scenes", Styles.ButtonStyle))
                {
                    var selectedScenes = _allScenePaths.Where((scene, index) => _sceneSelectionStates[index]).ToArray();
                    _log = "";
                    foreach (var scenePath in selectedScenes)
                        CleanScenePath(scenePath);

                    SaveLogToFile();
                }
                GUI.enabled = true;
            }
            else
            {
                if (GUILayout.Button("🧼 Clean All Scenes", Styles.ButtonStyle))
                {
                    if (EditorUtility.DisplayDialog("Are you sure?", "This will open and save all scenes in the project. Continue?", "Yes", "Cancel"))
                    {
                        _log = "";
                        foreach (var scenePath in _allScenePaths)
                            CleanScenePath(scenePath);

                        SaveLogToFile();
                        EditorUtility.DisplayDialog("Done", "All scenes cleaned.", "OK");
                    }
                }
            }
        }
    }

    private void DrawLogSection()
    {
        Rect foldoutRect = EditorGUILayout.GetControlRect(false, 22);
        EditorGUI.DrawRect(foldoutRect, Styles.FoldoutBgColor);
        GUI.color = Color.cyan;
        _showLog = EditorGUI.Foldout(foldoutRect, _showLog, "📝 Log Output", true);
        GUI.color = Color.white;
        if (_showLog)
        {
            using (new EditorGUILayout.VerticalScope(Styles.SectionStyle))
            {
                _logScrollPosition = EditorGUILayout.BeginScrollView(_logScrollPosition, GUILayout.Height(150));
                EditorGUILayout.TextArea(_log, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();

                GUILayout.Space(5);
                EditorGUILayout.LabelField($"Log saved to: {_logFilePath}", EditorStyles.wordWrappedLabel);
            }
        }
    }

    private void CleanFolder(string folderPath, string[] selectedPrefabs)
    {
        if (selectedPrefabs == null)
        {
            var filter = string.IsNullOrEmpty(_prefabSearchQuery) ? "t:GameObject" : $"t:GameObject {_prefabSearchQuery}";
            var folders = _includeAllFolders
                ? AssetDatabase.GetSubFolders(folderPath).Prepend(folderPath).ToArray()
                : new[] { folderPath };
            var guids = AssetDatabase.FindAssets(filter, folders);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!_includeAllFolders && !(path.StartsWith(folderPath + "/") && path.LastIndexOf('/') == folderPath.Length))
                    continue;

                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (obj == null) continue;

                int removed = CleanGameObject(obj);
                if (removed > 0)
                    AppendLog($"✅ Prefab: {path} - Removed {removed} missing scripts.\n");
            }
        }
        else
        {
            foreach (var path in selectedPrefabs.Where(p => p.StartsWith(folderPath)))
            {
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (obj == null) continue;

                int removed = CleanGameObject(obj);
                if (removed > 0)
                    AppendLog($"✅ Prefab: {path} - Removed {removed} missing scripts.\n");
            }
        }

        if (string.IsNullOrEmpty(_log))
            AppendLog($"ℹ️ No missing scripts in: {folderPath}\n");
    }

    private void CleanScenePath(string scenePath)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        int removed = scene.GetRootGameObjects().Sum(CleanGameObject);

        if (removed > 0)
        {
            EditorSceneManager.SaveScene(scene);
            AppendLog($"✅ Scene: {scenePath} - Removed {removed} missing scripts.\n");
        }
        else
        {
            AppendLog($"ℹ️ Scene: {scenePath} - No missing scripts.\n");
        }
    }

    private int CleanGameObject(GameObject obj)
    {
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
        foreach (Transform child in obj.transform)
            removed += CleanGameObject(child.gameObject);
        return removed;
    }

    private void AppendLog(string text)
    {
        _log += text;
        SaveLogToFile();
    }

    private void SaveLogToFile()
    {
        try
        {
            File.WriteAllText(_logFilePath, _log);
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save log file: " + e.Message);
        }
    }
}