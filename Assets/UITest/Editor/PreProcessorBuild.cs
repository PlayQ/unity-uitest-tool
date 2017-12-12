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
        private const string TestSceneName = "RuntimeTestScene.unity";

        public int callbackOrder
        {
            get { return 0; }
        }

        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            var pathes = Directory.GetFiles(Application.dataPath, TestSceneName, SearchOption.AllDirectories);
            if (pathes.Length == 0)
            {
                Debug.LogError("Can't find test scene");
                return;
            }

            var testScenePath = pathes[0];
            var assetsIndex = testScenePath.IndexOf("Assets", StringComparison.Ordinal);
            testScenePath = testScenePath.Remove(0, assetsIndex);

            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Count == 0 || scenes[0].path != testScenePath || scenes[0].enabled == false)
            {
                var index = scenes.FindIndex(s => s.path.Contains(TestSceneName));
                if (index != -1)
                {
                    scenes.RemoveAt(index);
                }

                scenes.Insert(0, new EditorBuildSettingsScene(testScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}

#endif