using UnityEditor;
using UnityEngine;

namespace PluginSmith.SceneMaster
{
    public static class SceneDataStorage
    {
        private const string DATA_KEY = "SceneMaster_Data";

        public static void Save(SceneMasterData data)
        {
            string json = JsonUtility.ToJson(data);
            EditorPrefs.SetString(DATA_KEY, json);
        }

        public static SceneMasterData Load()
        {
            string json = EditorPrefs.GetString(DATA_KEY, "{}");
            return JsonUtility.FromJson<SceneMasterData>(json) ?? new SceneMasterData();
        }
    }
}