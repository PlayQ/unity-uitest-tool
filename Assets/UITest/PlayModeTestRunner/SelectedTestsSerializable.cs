using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SelectedTestsSerializable : ScriptableObject
{
    public const string path = "Assets/Tests/UITestTools/PlayModeTestRunner/Resources/SelectedTests.asset";
    
    [SerializeField] private List<TestInfo> TestsInfo = new List<TestInfo>();
    
    private Dictionary<string, TestInfo> testInfoToMethodName = new Dictionary<string, TestInfo>();
    public Dictionary<string, TestInfo> TestInfoToMethodName
    {
        get
        {
            testInfoToMethodName.Clear();
            foreach (var testInfo in TestsInfo)
            {
                testInfoToMethodName[testInfo.MethodName] = testInfo;
            }
            return testInfoToMethodName;
        }
        set
        {
            testInfoToMethodName = value;
            SerializeData();
        }
    }

    public void SerializeData()
    {
        TestsInfo.Clear();
        foreach (var testToMethodName in testInfoToMethodName.Values)
        {
            TestsInfo.Add(testToMethodName);
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


    public bool IsAnyMethodSelected(string className)
    {
       var result = TestInfoToMethodName.Where(testInfo => 
           testInfo.Value.ClassName.Equals(className, StringComparison.Ordinal)
            && testInfo.Value.IsSelected);
        return result.Any();
    }
    
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
    }

}
