#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using PlayQ.UITestTools;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class TestToolBuildScript
{   
    private static void PrepareBuild()
    {
        AddTestSceneToBuild();
        ResolveTestMode();
        AddScreenshotsToBuild();
    }

    //For executing from command line
    [UsedImplicitly]
    private static void TestBuild()
    {
        SetTimeScaleFromCommandLineArgument();
        PrepareBuild();
        
        string[] list = EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray();
        BuildPipeline.BuildPlayer(list,
            ConsoleArgumentHelper.GetArg("-testBuildPath"), BuildTarget.Android,
            BuildOptions.None);
    }

    //for executing from command line
    [UsedImplicitly]
    public static void RunPlayModeTests()
    {
        SetTimeScaleFromCommandLineArgument();
        SetTestsNamespacesFromCommandLineArgument();

        PlayerPrefs.SetString("PackageUpdaterLastChecked68207", DateTime.Now.ToString(CultureInfo.InvariantCulture));
        
        PlayModeTestRunner.QuitAppAfterCompleteTests = true;
        ResolveTestMode();
        PlayModeTestRunner.Run();
    }

    private static void SetTestsNamespacesFromCommandLineArgument()
    {
        string namespaceString = ConsoleArgumentHelper.GetArg("-testNamespaces");

        int timeScale = 0;
        if (!string.IsNullOrEmpty(namespaceString))
        {
            namespaceString = namespaceString.Replace(" ", String.Empty);
            string[] strings = namespaceString.Split(';');

            if (strings.Length > 0)
            {
                PlayModeTestRunner.Namespaces = strings.Where(s => !string.IsNullOrEmpty(s)).ToArray();
                if (PlayModeTestRunner.Namespaces.Length > 0)
                {
                    Debug.LogFormat("TestTools: namespace string {0}  Test Namespaces count: {1}",
                        namespaceString,
                        PlayModeTestRunner.Namespaces.Length);
                    
                    foreach (var name in PlayModeTestRunner.Namespaces)
                    {
                        Debug.Log("namespace: " + name);
                    }
                    return;
                }
            }
        }
        
        Debug.Log("TestTools: No Test Namespaces:");
        PlayModeTestRunner.Namespaces = null;
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
        
        Debug.Log("----------- test scene path: " + scenePath + 
                       " all scenes amount: " + scenes.Count +
                       " all scene names " + allScenes);
        
        if (scenes.Count == 0 || scenes[0].path != scenePath || scenes[0].enabled == false)
        {
            if (scenes.Count > 0)
            {
                Debug.Log("----------- scenes[0].path: " + scenes[0].path + 
                               " scenes[0].enabled: " + scenes[0].enabled);
            }
            var index = scenes.FindIndex(s => s.path.Contains(PlayModeTestRunner.TEST_NAME_SCENE));
            if (index != -1)
            {
                scenes.RemoveAt(index);
            }
            Debug.Log("----------- test scene index " + index);
            
            scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            
            Debug.Log("------- EditorBuildSettings.scenes "
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

    private static void MoveContentFromFolderToFolder(string folderSrc, string folderDst)
    {
        if (!Directory.Exists(folderSrc) ||
            !Directory.Exists(folderDst))
        {
            return;
        }

        var buildFiles = Directory.GetFiles(folderDst);
        foreach (var file in buildFiles)
        {
            File.Delete(file);
        }
        
        var buildFolders = Directory.GetDirectories(folderDst);
        foreach (var folder in buildFolders)
        {
            Directory.Delete(folder, true);
        }
        
        var editorFiles = Directory.GetFiles(folderSrc);
        foreach (var file in editorFiles)
        {
            var fileToMove = new FileInfo(file);
            File.Move(file,folderDst + '/' + fileToMove.Name);
        }
        var editorFolders = Directory.GetDirectories(folderSrc);
        foreach (var folder in editorFolders)
        {
            var folderToMove = new DirectoryInfo(folder);
            Directory.Move(folder,folderDst + '/' + folderToMove.Name);
        }
    }

    private static void AddScreenshotsToBuild()
    {
        var editorTestResourcesFolder = PlayModeTestRunner.EditorTestResourcesFolder;
        var buildTestResourcesFolder = PlayModeTestRunner.BuildTestResourcesFolder;
        
        if (string.IsNullOrEmpty(editorTestResourcesFolder) || string.IsNullOrEmpty(buildTestResourcesFolder))
        {
            return;
        }
        
        MoveContentFromFolderToFolder(editorTestResourcesFolder, buildTestResourcesFolder);
    }
    
    
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log("OnPostprocessBuild: move Reference Screensots from Resources back to editor folder.");
        
        var editorTestResourcesFolder = PlayModeTestRunner.EditorTestResourcesFolder;
        var buildTestResourcesFolder = PlayModeTestRunner.BuildTestResourcesFolder;
        
        if (string.IsNullOrEmpty(editorTestResourcesFolder) || string.IsNullOrEmpty(buildTestResourcesFolder))
        {
            return;
        }
        
        MoveContentFromFolderToFolder(buildTestResourcesFolder, editorTestResourcesFolder);
    }
    
    [MenuItem("Window/UI Test Tools/Prepare For Device Build")]
    public static void MenuPreBuild()
    {
        AddTestSceneToBuild();
        AddScreenshotsToBuild();
    }
}
#endif