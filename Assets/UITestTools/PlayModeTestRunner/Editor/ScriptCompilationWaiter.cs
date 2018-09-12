using System;
using UnityEngine;

namespace UnityEditor
{
    public static class ScriptCompilationChecker
    {
        private static DateTime waitingForRestart;
        
        [Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded() 
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("===============================" +
                               "Compilation is started during Playmode. Tests will be restarted" +
                               "===============================");
                EditorApplication.isPlaying = false;
                waitingForRestart = DateTime.Now;
                EditorApplication.update += Update;
            }
        }


        private static void Update()
        {
            if (DateTime.Now - waitingForRestart > TimeSpan.FromSeconds(5))
            {
                EditorApplication.update -= Update;
                EditorApplication.isPlaying = true;
            }
        }
    }
}