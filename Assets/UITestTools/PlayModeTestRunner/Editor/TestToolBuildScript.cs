using System;
using System.Linq;
using PlayQ.UITestTools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;


	//Unity.exe -projectPath C:\WORK\CharmKing -executeMethod TestToolBuildScript.Build -playOnlySelectedTests false -buildTarget Android
	//-playOnlySelectedTests - optional, if not set all tests will be executed
	//-buildPath - optional, if set - android build will be created

	public static class TestToolBuildScript
	{

		public static void Build()
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

			var buildPath = GetArg("-buildPath");
			if (!string.IsNullOrEmpty(buildPath))
			{
				BuildPipeline.BuildPlayer(scenes.ToArray(), buildPath, BuildTarget.Android, BuildOptions.None);
				EditorApplication.Exit(0);
			}
			else
			{
				EditorSceneManager.OpenScene(scenePath);
				var ignoreUnselectedTests = GetArg("-playOnlySelectedTests");
				if (string.IsNullOrEmpty(ignoreUnselectedTests))
				{
					PlayerPrefs.SetInt(PlayModeTestRunner.PLAY_ONLY_SELECTED_TESTS, 0);
				}
				else
				{
					var result = ignoreUnselectedTests.Equals("true", StringComparison.OrdinalIgnoreCase);
					PlayerPrefs.SetInt(PlayModeTestRunner.PLAY_ONLY_SELECTED_TESTS, result ? 1 : 0);
				}

				PlayerPrefs.SetInt(PlayModeTestRunner.QUIT_APP_AFTER_TESTS, 1);
				EditorApplication.isPlaying = true;
			}
		}

		private static string GetArg(string name)
		{
			var args = System.Environment.GetCommandLineArgs();
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] == name && args.Length > i + 1)
				{
					return args[i + 1];
				}
			}
			return null;
		}

	}

