using System.Collections.Generic;

namespace PluginSmith.SceneMaster
{

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
}
