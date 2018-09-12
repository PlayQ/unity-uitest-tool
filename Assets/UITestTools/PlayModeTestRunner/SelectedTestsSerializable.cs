using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayQ.UITestTools;
using UnityEngine;
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
    public float DefaultTimescale;
    
    public const string path = "Assets/Tests/UITestTools/PlayModeTestRunner/Resources/SelectedTests.asset";
    
    [SerializeField] private List<TestInfo> testsInfo = new List<TestInfo>();

    private Dictionary<string, TestInfo> testInfoToMethodName;

    private void RestoreDictionary()
    {
        testInfoToMethodName = new Dictionary<string, TestInfo>();
        foreach (var testInfo in testsInfo)
        {
            testInfoToMethodName.Add(testInfo.MethodName, testInfo);
        } 
    }
    
    public void AddTest(string key, TestInfo testInfo)
    {
        if (testInfoToMethodName == null)
        {
            RestoreDictionary();
        }
        testInfoToMethodName.Add(key, testInfo);
        testsInfo.Add(testInfo);
    }
    
    public bool ContainsTest(string key)
    {
        if (testInfoToMethodName == null)
        {
            RestoreDictionary();
        }

        return testInfoToMethodName.ContainsKey(key);
    }
    
    public bool RemoveTest(string key)
    {
        if (testInfoToMethodName == null)
        {
            RestoreDictionary();
        }

        var index = testsInfo.FindIndex(test => test.MethodName == key);
        if (index != -1)
        {
            testsInfo.RemoveAt(index);
        }
        return testInfoToMethodName.Remove(key);
    }
    
    public TestInfo GetTest(string key)
    {
        if (testInfoToMethodName == null)
        {
            RestoreDictionary();
        }

        TestInfo result;
        testInfoToMethodName.TryGetValue(key, out result);
        return result;
    }

   
    public ICollection<string> TestInfoKeys
    {
        get
        {
            if (testInfoToMethodName == null)
            {
                RestoreDictionary();
            }

            return testInfoToMethodName.Keys;
        }

    }

    public ICollection<TestInfo> SelectedTestsInfos
    {
        get
        {
            return testsInfo.Where(item => item.IsSelected).ToArray();
        }
    }

#if UNITY_EDITOR
    public static SelectedTestsSerializable CreateOrLoad()
    {
        var asset = AssetDatabase.LoadAssetAtPath<SelectedTestsSerializable>(path);
        if (!asset)
        {
            asset = CreateInstance<SelectedTestsSerializable>();

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
        }
        return asset;
    }
#endif

    
    public static SelectedTestsSerializable Load()
    {
        return Resources.Load<SelectedTestsSerializable>("SelectedTests");
    }

    [Serializable]
    public class TestInfo
    {
        public bool IsSelected;
        public string MethodName;
        public string ClassName;

        public TestInfo(string methodName, string className)
        {
            MethodName = methodName;
            ClassName = className;
        }
    }

}
