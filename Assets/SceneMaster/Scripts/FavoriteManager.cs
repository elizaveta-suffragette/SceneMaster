using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneMasterWindow : EditorWindow
{
    private SceneMasterData data = new SceneMasterData();
    private HashSet<string> favoriteScenes = new HashSet<string>();
    private Dictionary<string, string> sceneTags = new Dictionary<string, string>();
    private const string DATA_KEY = "SceneMaster_Data";

    [MenuItem("Tools/SceneMaster")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneMasterWindow>("SceneMaster");
        window.minSize = new Vector2(300, 400);
    }

    private void OnEnable()
    {
        LoadData();
    }

    private void OnGUI()
    {
        GUILayout.Label("⭐ Favorites", EditorStyles.boldLabel);
        DrawGroupedScenes(onlyFavorites: true);

        GUILayout.Space(10);
        GUILayout.Label("📁 All Scenes", EditorStyles.boldLabel);
        DrawGroupedScenes(onlyFavorites: false);
    }

    private void DrawGroupedScenes(bool onlyFavorites)
    {
        string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });

        var grouped = sceneGUIDs
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Where(path => !onlyFavorites || favoriteScenes.Contains(path))
            .GroupBy(path => sceneTags.ContainsKey(path) ? sceneTags[path] : "Untagged")
            .OrderBy(group => group.Key);

        foreach (var group in grouped)
        {
            GUILayout.Label($"📂 {group.Key}", EditorStyles.boldLabel);
            foreach (var path in group)
            {
                DrawSceneRow(path);
            }

            GUILayout.Space(5);
        }
    }

    private void DrawSceneRow(string path)
    {
        string sceneName = Path.GetFileNameWithoutExtension(path);
        bool isFavorite = favoriteScenes.Contains(path);
        string currentTag = sceneTags.ContainsKey(path) ? sceneTags[path] : "";

        GUILayout.BeginHorizontal();

        // Favorite toggle button
        if (GUILayout.Button(isFavorite ? "★" : "☆", GUILayout.Width(25)))
        {
            if (isFavorite)
                favoriteScenes.Remove(path);
            else
                favoriteScenes.Add(path);

            SaveData();
        }

        // Scene name
        GUILayout.Label(sceneName, GUILayout.Width(120));

        // Editable tag field
        string newTag = GUILayout.TextField(currentTag, GUILayout.Width(100));
        if (newTag != currentTag)
        {
            if (string.IsNullOrWhiteSpace(newTag))
                sceneTags.Remove(path);
            else
                sceneTags[path] = newTag;

            SaveData();
        }

        // Open button
        if (GUILayout.Button("Open", GUILayout.Width(60)))
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[SceneMaster] Opening scene: " + path);
                EditorSceneManager.OpenScene(path);
            }
        }

        GUILayout.EndHorizontal();
    }

    private void SaveData()
    {
        data.favoriteScenePaths = favoriteScenes.ToList();
        data.taggedScenes = sceneTags
            .Select(t => new SceneTagData { path = t.Key, tag = t.Value })
            .ToList();

        string json = JsonUtility.ToJson(data);
        EditorPrefs.SetString(DATA_KEY, json);
    }

    private void LoadData()
    {
        string json = EditorPrefs.GetString(DATA_KEY, "{}");
        data = JsonUtility.FromJson<SceneMasterData>(json) ?? new SceneMasterData();
        favoriteScenes = new HashSet<string>(data.favoriteScenePaths);
        sceneTags = data.taggedScenes.ToDictionary(t => t.path, t => t.tag);
    }
}

[System.Serializable]
public class SceneTagData
{
    public string path;
    public string tag;
}

[System.Serializable]
public class SceneMasterData
{
    public List<string> favoriteScenePaths = new List<string>();
    public List<SceneTagData> taggedScenes = new List<SceneTagData>();
}
