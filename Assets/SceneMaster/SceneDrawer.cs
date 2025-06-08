using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PluginSmith.SceneMaster
{
    public static class SceneDrawer
    {
        public static void DrawGroupedScenes(bool onlyFavorites, string searchQuery, HashSet<string> favorites, Dictionary<string, string> tags, System.Action onDataChanged)
        {
            string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });

            var grouped = sceneGUIDs
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path =>
                {
                    if (onlyFavorites && !favorites.Contains(path))
                        return false;

                    string name = Path.GetFileNameWithoutExtension(path).ToLower();
                    string tag = tags.ContainsKey(path) ? tags[path].ToLower() : "";

                    return string.IsNullOrEmpty(searchQuery) ||
                           name.Contains(searchQuery.ToLower()) ||
                           tag.Contains(searchQuery.ToLower());
                })
                .GroupBy(path => tags.ContainsKey(path) ? tags[path] : "Untagged")
                .OrderBy(group => group.Key);

            foreach (var group in grouped)
            {
                GUILayout.Space(4);
                GUILayout.Label($"📁 {group.Key}", EditorStyles.helpBox);
                foreach (var path in group)
                {
                    DrawSceneRow(path, favorites, tags, onDataChanged);
                }

                GUILayout.Space(5);
            }
        }

        private static void DrawSceneRow(string path, HashSet<string> favorites, Dictionary<string, string> tags, System.Action onDataChanged)
        {
            string sceneName = Path.GetFileNameWithoutExtension(path);
            bool isFavorite = favorites.Contains(path);
            string currentTag = tags.ContainsKey(path) ? tags[path] : "";

            // Get metadata
            FileInfo fileInfo = new FileInfo(path);
            long fileSizeBytes = fileInfo.Exists ? fileInfo.Length : 0;
            string sizeText = fileSizeBytes > 1024 * 1024
                ? $"{(fileSizeBytes / (1024f * 1024f)):0.0} MB"
                : $"{(fileSizeBytes / 1024f):0.0} KB";

            string modifiedText = fileInfo.Exists ? fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm") : "N/A";

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(isFavorite ? "★" : "☆", GUILayout.Width(25)))
            {
                if (isFavorite)
                    favorites.Remove(path);
                else
                    favorites.Add(path);
                onDataChanged?.Invoke();
            }

            GUILayout.Label(sceneName, GUILayout.Width(120));

            string newTag = GUILayout.TextField(currentTag, GUILayout.Width(100));
            if (newTag != currentTag)
            {
                if (string.IsNullOrWhiteSpace(newTag))
                    tags.Remove(path);
                else
                    tags[path] = newTag;
                onDataChanged?.Invoke();
            }

            if (GUILayout.Button("Open", GUILayout.Width(60)))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(path);
                }
            }

            GUILayout.EndHorizontal();

            // Metadata row
            GUILayout.BeginHorizontal();
            GUILayout.Label($"📁 {sizeText}   🕒 {modifiedText}", EditorStyles.miniLabel);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}
