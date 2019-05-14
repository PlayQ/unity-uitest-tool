using System;
using PlayQ.UITestTools;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SelectedTestsSerializable : ScriptableObject
{ 
    public RunTestsMode.RunTestsModeEnum TestRunMode;
    public PlayModeTestRunner.SpecificTestType RunSpecificTestType;
    public int RepeatTestsNTimes;
    public string SpecificTestOrClassName;
    public bool QuitAferComplete;
    public float DefaultTimescale = 1;
    public string SerializedTestsData;
    public string SelectedNodeFullName;
    public string[] Namespaces;

#if UNITY_EDITOR
    public static SelectedTestsSerializable CreateOrLoad()
    {
        var asset = Resources.Load<SelectedTestsSerializable>("SelectedTests");

        if (!asset)
        {
            string path = FindPath("PlayModeTestRunner", Application.dataPath);

            if (!string.IsNullOrEmpty(path))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
                path = Path.Combine(path, "Resources");

                asset = CreateInstance<SelectedTestsSerializable>();

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = Path.Combine(path, "SelectedTests.asset");
                
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
            }
            else
                throw new Exception("SelectedTestsSerializable: UNABLE TO FIND FOLDER NAMED \"PlayModeTestRunner\"");
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
