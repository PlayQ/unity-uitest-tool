using UnityEngine;
using PlayQ.UITestTools;

namespace UnityEditor
{
    public static class ScriptCompilationChecker
    {
#if UNITY_EDITOR
        [Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded() 
        {
            if (EditorApplication.isPlaying && GameObject.FindObjectOfType<PlayModeTestRunner>() != null)
            {
                Debug.LogError("===============================" +
                               "Compilation is started during Playmode. Tests will be restarted" +
                               "===============================");
                EditorApplication.update += Update;
            }
        }

        private static void Update()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }
            else
            {
                EditorApplication.update -= Update;
                EditorApplication.isPlaying = true;
            }
        }
#endif
    }
}