using UnityEngine;

namespace PluginSmith.SceneMaster.UI
{
    public class OpenSceneMasterButton : MonoBehaviour
    {
        public void OpenSceneMasterWindow()
        {
#if UNITY_EDITOR
            PluginSmith.SceneMaster.SceneMasterWindow.ShowWindow();
#else
            Debug.Log("SceneMaster is only available in the Editor.");
#endif
        }
    }
}
