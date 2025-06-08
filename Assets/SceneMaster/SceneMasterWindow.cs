using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PluginSmith.SceneMaster
{
    public class SceneMasterWindow : EditorWindow
    {
        private SceneMasterData data = new SceneMasterData();
        private HashSet<string> favoriteScenes = new HashSet<string>();
        private Dictionary<string, string> sceneTags = new Dictionary<string, string>();
        private string searchQuery = "";
        private Texture2D logo;

        [MenuItem("Tools/SceneMaster")]
        public static void ShowWindow()
        {
            var window = GetWindow<SceneMasterWindow>("SceneMaster");
            window.minSize = new Vector2(300, 400);
        }

        private void OnEnable()
        {
            data = SceneDataStorage.Load();
            favoriteScenes = new HashSet<string>(data.favoriteScenePaths);
            sceneTags = data.taggedScenes.ToDictionary(t => t.path, t => t.tag);
            logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/SceneMaster/logo.png");
        }

        private void OnGUI()
        {
            DrawHeader();

            GUILayout.Space(10);
            GUILayout.Label("🔍 Search", EditorStyles.boldLabel);
            searchQuery = GUILayout.TextField(searchQuery);

            GUILayout.Space(10);
            GUILayout.Label("⭐ Favorites", EditorStyles.boldLabel);
            SceneDrawer.DrawGroupedScenes(true, searchQuery, favoriteScenes, sceneTags, SaveData);

            GUILayout.Space(10);
            GUILayout.Label("📁 All Scenes", EditorStyles.boldLabel);
            SceneDrawer.DrawGroupedScenes(false, searchQuery, favoriteScenes, sceneTags, SaveData);
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            if (logo != null)
            {
                GUILayout.Label(logo, GUILayout.Width(64), GUILayout.Height(64));
            }

            GUILayout.BeginVertical();

            GUILayout.Label("SceneMaster", EditorStyles.boldLabel);
            GUILayout.Label("Scene switching & favorites tool", EditorStyles.miniLabel);

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reset", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("Reset SceneMaster", "Clear all favorites and tags?", "Yes", "Cancel"))
                {
                    favoriteScenes.Clear();
                    sceneTags.Clear();
                    SaveData();
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

            SceneDataStorage.Save(data);
        }
    }
}
