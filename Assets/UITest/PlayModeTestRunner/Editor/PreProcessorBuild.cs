#if RUNTIME_TESTS_ENABLED

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public class PreProcessorBuild : IPreprocessBuild
    {
        public int callbackOrder
        {
            get { return 0; }
        }

        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            var scenePath = PlayModeTestRunner.GetTestScenePath();
            if (scenePath == null)
            {
                Debug.LogError("Cant find test scene");
                return;
            }
            
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Count == 0 || scenes[0].path != scenePath || scenes[0].enabled == false)
            {
                var index = scenes.FindIndex(s => s.path.Contains(PlayModeTestRunner.TEST_NAME_SCENE));
                if (index != -1)
                {
                    scenes.RemoveAt(index);
                }

                scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}

#endif