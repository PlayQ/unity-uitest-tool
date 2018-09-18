#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using PlayQ.UITestTools;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;


public class TestToolBuildScript
{
    //You may call it from PlayQ.Build.Builder class
    [UsedImplicitly]
    public static void PrepareTestBuild()
    {
        AddTestSceneToBuild();
        ResolveTestMode();
        PlayModeTestRunner.AdjustSelectedTestsForBuild();
        AddScreenshotsAndEventsToBuild();
    }

    //For executing from command line
    [UsedImplicitly]
    private static void TestBuild()
    {
        SetTimeScaleFromCommandLineArgument();
        PrepareTestBuild();
        
        string[] list = EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray();
        BuildPipeline.BuildPlayer(list,
            ConsoleArgumentHelper.GetArg("-testBuildPath"), BuildTarget.Android,
            BuildOptions.None);
    }

    //for executing from connamd line
    [UsedImplicitly]
    public static void RunPlayModeTests()
    {
        SetTimeScaleFromCommandLineArgument();

        PlayerPrefs.SetString("PackageUpdaterLastChecked68207", DateTime.Now.ToString(CultureInfo.InvariantCulture));

        PlayModeTestRunner.QuitAppAfterCompleteTests = true;
        ResolveTestMode();
        PlayModeTestRunner.Run();
    }

    private static void SetTimeScaleFromCommandLineArgument()
    {
        string timeScaleString = ConsoleArgumentHelper.GetArg("-timeScale");

        int timeScale = 0;
        if (timeScaleString != null)
        {
            int.TryParse(timeScaleString, out timeScale);
        }
        if (timeScale <= 0)
        {
            timeScale = 1;
        }
        PlayModeTestRunner.DefaultTimescale = timeScale;
    }

    private static void AddTestSceneToBuild()
    {
        string scenePath = PlayModeTestRunner.GetTestScenePath();
        if (string.IsNullOrEmpty(scenePath))
        {
            Debug.LogError("Cant find test scene");
            return;
        }
        
        var scenes = EditorBuildSettings.scenes.ToList();
        var allScenes = scenes.Aggregate("", (concated, scene) => scene.path + "; " + concated);
        
        Debug.LogError("----------- test scene path: " + scenePath + 
                       " all scenes amount: " + scenes.Count +
                       " all scene names " + allScenes);
        
        if (scenes.Count == 0 || scenes[0].path != scenePath || scenes[0].enabled == false)
        {
            if (scenes.Count > 0)
            {
                Debug.LogError("----------- scenes[0].path: " + scenes[0].path + 
                               " scenes[0].enabled: " + scenes[0].enabled);
            }
            var index = scenes.FindIndex(s => s.path.Contains(PlayModeTestRunner.TEST_NAME_SCENE));
            if (index != -1)
            {
                scenes.RemoveAt(index);
            }
            Debug.LogError("----------- test scene index " + index);
            
            scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            
            Debug.LogError("------- EditorBuildSettings.scenes "
                           + EditorBuildSettings.scenes.Aggregate("", (concated, scene) => scene.path + "; " + concated));
        }
    }

    private static void ResolveTestMode()
    {
        bool runOnlySelected = ConsoleArgumentHelper.IsArgExist("-runOnlySelectedTests");
        bool runOnlySmoke = ConsoleArgumentHelper.IsArgExist("-runOnlySmokeTests");

        if (runOnlySelected && runOnlySmoke)
        {
            Debug.LogError("You trying to launck selected and smoke tests together.");
        }
        if (runOnlySelected)
        {
            RunTestsMode.SelectOnlySelectedTests();
        }
        else
        {
            if (runOnlySmoke)
            {
                RunTestsMode.SelectOnlySmokeTests();
            }
            else
            {
                RunTestsMode.SelectAllTests();
            }
        }
    }


    private const string PATH_EDITOR_RESOURCES = "/Tests/Editor/Resources";
    private const string PATH_MOBILE_RESOURCES = "/Tests/Resources";
    private const string SCREENSHOT_FOLDER = "/ReferenceScreenshots";
    private const string EVENTS = "/ArchivedEvents.asset";

    private static void AddScreenshotsAndEventsToBuild()
    {
        if (Directory.Exists(Application.dataPath + PATH_EDITOR_RESOURCES + SCREENSHOT_FOLDER))
        {
            if (Directory.Exists(Application.dataPath + PATH_MOBILE_RESOURCES + SCREENSHOT_FOLDER))
            {
                Directory.Delete(Application.dataPath + PATH_MOBILE_RESOURCES + SCREENSHOT_FOLDER, true);
            }
            if (!Directory.Exists(Application.dataPath + PATH_MOBILE_RESOURCES))
            {
                Directory.CreateDirectory(Application.dataPath + PATH_MOBILE_RESOURCES);
            }
            Directory.Move(Application.dataPath + PATH_EDITOR_RESOURCES + SCREENSHOT_FOLDER,
                Application.dataPath + PATH_MOBILE_RESOURCES + SCREENSHOT_FOLDER);
        }

        var editorEventsPath = Application.dataPath + PATH_EDITOR_RESOURCES + EVENTS;
        if (File.Exists(editorEventsPath))
        {
            var mobileEventsPath = Application.dataPath + PATH_MOBILE_RESOURCES + EVENTS;
            if (File.Exists(mobileEventsPath))
            {
                File.Delete(mobileEventsPath);
            }

            var mobileResourcesPath = Application.dataPath + PATH_MOBILE_RESOURCES;
            if (Directory.Exists(mobileResourcesPath))
            {
                Directory.CreateDirectory(mobileResourcesPath);
            }

            Directory.Move(editorEventsPath, mobileEventsPath);
        }
    }

    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log("OnPostprocessBuild: move Reference Screensots from Resources back to editor folder.");

        if (!Directory.Exists(Application.dataPath + PATH_EDITOR_RESOURCES))
        {
            Directory.CreateDirectory(Application.dataPath + PATH_EDITOR_RESOURCES);
        }

        if (Directory.Exists(Application.dataPath + PATH_MOBILE_RESOURCES + SCREENSHOT_FOLDER))
        {
            Directory.Move(Application.dataPath + PATH_MOBILE_RESOURCES + SCREENSHOT_FOLDER,
                Application.dataPath + PATH_EDITOR_RESOURCES + SCREENSHOT_FOLDER);
        }

        var mobileEventsPath = Application.dataPath + PATH_MOBILE_RESOURCES + EVENTS;
        if (File.Exists(mobileEventsPath))
        {
            var editorEventsPath = Application.dataPath + PATH_EDITOR_RESOURCES + EVENTS;
            Directory.Move(mobileEventsPath, editorEventsPath);
        }

        if (Directory.Exists(Application.dataPath + PATH_MOBILE_RESOURCES))
        {
            Directory.Delete(Application.dataPath + PATH_MOBILE_RESOURCES, true);
        }
    }
    
    [MenuItem("Window/UI Test Tools/Prepare For Device Build")]
    public static void MenuPreBuild()
    {
        AddTestSceneToBuild();
        PlayModeTestRunner.AdjustSelectedTestsForBuild();
        AddScreenshotsAndEventsToBuild();
    }
}


#endif