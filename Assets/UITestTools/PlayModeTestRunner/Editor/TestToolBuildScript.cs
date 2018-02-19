#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using PlayQ.UITestTools;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using Debug = UnityEngine.Debug;


	public class TestToolBuildScript : IPreprocessBuild
	{

		//don't user it it's only for test purpose
		private static void TestBuild()
		{
			List<string> scenePaths = new List<string>();

			foreach (var scene in EditorBuildSettings.scenes)
			{
				if (scene.enabled)
				{
					scenePaths.Add(scene.path);
				}
			}

			BuildPipeline.BuildPlayer(scenePaths.ToArray(),
				GetArgValue("-testBuildPath"), BuildTarget.Android, BuildOptions.None);
		}
		

		private static void PrepareBuild(out string scenePath)
		{
			scenePath = PlayModeTestRunner.GetTestScenePath();
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

			bool runOnlySelected = IsArgExist("-runOnlySelectedTests");
			bool runOnlySmoke = IsArgExist("-runOnlySmokeTests");
			
			
			PlayModeTestRunner.RunOnlySelectedTests = runOnlySelected;
			PlayModeTestRunner.RunOnlySmokeTests = runOnlySmoke;
		}
		
		public static void RunPlayModeTests()
		{

			string scenePath = null;
			PrepareBuild(out scenePath);

			if (!string.IsNullOrEmpty(scenePath))
			{
				EditorSceneManager.OpenScene(scenePath);
				PlayModeTestRunner.QuitAppAfterCompleteTests = true;
				EditorApplication.isPlaying = true;
			}
			else
			{
				EditorApplication.Exit(1);
			}
			
		}

		private static string GetArgValue(string name)
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
		
		private static bool IsArgExist(string name)
		{
			var args = System.Environment.GetCommandLineArgs();
			return args.Any(arg => arg == name);
		}

		public int callbackOrder
		{
			get { return 0; }
		}


		public void OnPreprocessBuild(BuildTarget target, string path)
		{
			if(IsArgExist("-makeTestBuild"))
			{
			
				string scenePath = null;
				PrepareBuild(out scenePath);
				PlayModeTestRunner.AdjustSelectedTestsForBuild();	
			}	
		}
	
	}


#endif