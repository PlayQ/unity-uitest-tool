using System;
using System.Collections.Generic;
using PlayQ.UITestTools;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SelectedTestsSerializable : ScriptableObject
{
#if UNITY_EDITOR
    private static string editorResourceDirectory;
    public static string EditorResourceDirectory
    {
        get
        {
            if (string.IsNullOrEmpty(editorResourceDirectory))
            {
                string path = FindPath("PlayModeTestRunner", Application.dataPath);
                if (!string.IsNullOrEmpty(path))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                    path += "/Editor/Resources";
                    editorResourceDirectory = path;
                    
                    if (!Directory.Exists(editorResourceDirectory))
                    {
                        Directory.CreateDirectory(editorResourceDirectory);
                    }
                }
                else
                {
                    throw new Exception("SelectedTestsSerializable: UNABLE TO FIND FOLDER NAMED \"PlayModeTestRunner\"");
                }
            }

            return editorResourceDirectory;
        }
    }
    private static string buildResourceDirectory;
    public static string BuildResourceDirectory
    {
        get
        {
            if (string.IsNullOrEmpty(buildResourceDirectory))
            {
                string path = FindPath("PlayModeTestRunner", Application.dataPath);
                if (!string.IsNullOrEmpty(path))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                    path += "/Resources";
                    buildResourceDirectory = path;
                    if (!Directory.Exists(buildResourceDirectory))
                    {
                        Directory.CreateDirectory(buildResourceDirectory);
                    }
                }
                else
                {
                    throw new Exception("SelectedTestsSerializable: UNABLE TO FIND FOLDER NAMED \"PlayModeTestRunner\"");
                }
            }

            return buildResourceDirectory;
        }
    }
#endif
    
    public RunTestsMode.RunTestsModeEnum TestRunMode;
    public PlayModeTestRunner.SpecificTestType RunSpecificTestType;
    public int RepeatTestsNTimes;
    public string SpecificTestOrClassName;
    public bool QuitAferComplete;
    public float DefaultTimescale = 1;
    public string SerializedTestsData;
    public string SelectedNodeFullName;
    public string[] Namespaces;
    public bool ForceMakeReferenceScreenshot;
    public List<string> BaseTypes = new List<string>();


#if UNITY_EDITOR
    public static SelectedTestsSerializable CreateOrLoad()
    {
        var asset = Resources.Load<SelectedTestsSerializable>("SelectedTests");

        if (!asset)
        {
            asset = CreateInstance<SelectedTestsSerializable>();
            var path = EditorResourceDirectory + '/' + "SelectedTests.asset";
            
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
        }

        return asset;
    }

    //Breadth-first search
    public static string FindPath(string targetDirectory, string directoryPath)
    {
        var directories = Directory.GetDirectories(directoryPath, targetDirectory, SearchOption.AllDirectories);
        return directories.Length == 0 ? null : directories[0];
    }
#endif


    public static SelectedTestsSerializable Load()
    {
        return Resources.Load<SelectedTestsSerializable>("SelectedTests");
    }

}
