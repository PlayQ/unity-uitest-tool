using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Tests;
using Tests.Nodes;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Screen = UnityEngine.Screen;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlayQ.UITestTools
{
    public class PlayModeTestRunner : TestCoroutiner
    {
        public enum SpecificTestType { Class, Method }

        public static bool IsTestUIEnabled = true;

        public static bool IsRunning
        {
            get { return isRunning && Application.isPlaying; }
        }

        public static string[] Namespaces
        {
            set
            {
                SerializedTests.Namespaces = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(SerializedTests);
#endif
            }
            get { return SerializedTests.Namespaces; }
        }

        public const string TEST_NAME_SCENE = "RuntimeTestScene.unity";
        private const string LOG_FILE_NAME = "runtimeTests.txt";

        private static ClassNode testsRootNode;

        public static event Action OnMethodStateUpdated;

        public static void SaveTestsData()
        {
            string testsStatesStr = JsonConvert.SerializeObject(TestsRootNode,
                new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Include
                });
            
            if (SerializedTests != null)
            {
                SerializedTests.SerializedTestsData = testsStatesStr;
#if UNITY_EDITOR
                EditorUtility.SetDirty(SerializedTests);
#endif
            }
        }

        public static void ResetTestRootNode()
        {
            testsRootNode = null;
            testsRootNode = NodeFactory.Build();
        }
        
        public static ClassNode TestsRootNode
        {
            get
            {
                if (testsRootNode == null)
                {
                    #if UNITY_EDITOR
                    if (UpdateTestsOnEveryCompilation)
                    {
                        testsRootNode = NodeFactory.Build();      
                    }
                    #else
                        testsRootNode = NodeFactory.Build();
                    #endif
                    
                    if (SerializedTests != null && 
                        !string.IsNullOrEmpty(SerializedTests.SerializedTestsData))
                    {
                        try
                        {
                            var oldData = JsonConvert.DeserializeObject<ClassNode>(
                                SerializedTests.SerializedTestsData,
                                new JsonSerializerSettings
                                {
                                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                                    TypeNameHandling = TypeNameHandling.Auto,
                                    NullValueHandling = NullValueHandling.Include,
                                });
                            
                            if (oldData != null)
                            {
                                if (testsRootNode != null)
                                {
                                    testsRootNode.MergeWithRoot(oldData);    
                                }
                                else
                                {
                                    testsRootNode = oldData;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Log(ex);
                        }
                    }
                }
                return testsRootNode;
            }
        }

        
        private static TestInfoData testInfoData;


        public class PlayingMethodNode
        {
            private MethodNode node;
            public MethodNode Node
            {
                get { return node; }
            }

            public void UpdateCurrentPlayingNode(MethodNode node)
            {
                this.node = node;
            }
        }

        public static PlayingMethodNode CurrentPlayingMethodNode = new PlayingMethodNode();
        private bool exceptionThrown;
        
        private PlayModeLogger playModeLogger;
        private LogSaver logSaver;
        private PlayModeTestRunnerGUI screenGUIDrawer;
        private static bool isRunning;

        #if UNITY_EDITOR
        private class Postprocessor : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, 
                                               string[] movedAssets, string[] movedFromAssetPaths)
            {
                foreach (string str in deletedAssets)
                {
                    if (str.EndsWith("SelectedTests.asset"))
                    {
                        serializedTests = null;
                    }
                }
            }
        }
        #endif

        private static SelectedTestsSerializable serializedTests;

        public static SelectedTestsSerializable SerializedTests
        {
            get
            {
#if UNITY_EDITOR
                if (!serializedTests)
                {
                    serializedTests = SelectedTestsSerializable.CreateOrLoad();
                }

                return serializedTests;
#else
                if (!serializedTests)
                {
                    serializedTests = SelectedTestsSerializable.Load(); 
                }
                if (serializedTests == null)
                {
                    throw new NullReferenceException("SelectedTestsSerializable not found");
                }
                return serializedTests;
#endif
            }
        }

        private void Awake()
        {
            Debug.Log("TestTools: PlayMode test runner awakes");
            DontDestroyOnLoad(gameObject);
            playModeLogger = new PlayModeLogger();
            screenGUIDrawer = new PlayModeTestRunnerGUI();
            logSaver = new LogSaver(Path.Combine(Application.persistentDataPath, LOG_FILE_NAME));
            testInfoData = new TestInfoData();

            StartProcessingTests();
        }

        
        public static bool UpdateTestsOnEveryCompilation
        {
            set
            {
                SerializedTests.UpdateTestsOnEveryCompilation = value;
            }
            get { return SerializedTests.UpdateTestsOnEveryCompilation; }
        }
        
        public static bool QuitAppAfterCompleteTests
        {
            set
            {
                SerializedTests.QuitAferComplete = value;
            }
            get { return SerializedTests.QuitAferComplete; }
        }

        public static List<string> BaseTypes
        {
            get
            {
                return SerializedTests.BaseTypes;
            }
        }

        public static float DefaultTimescale
        {
            set
            {
                SerializedTests.DefaultTimescale = value;
                if (isRunning)
                {
                    Time.timeScale = value;
                }
            }
            get
            {
                return SerializedTests.DefaultTimescale;
            }
        }
        
        public static bool ForceMakeReferenceScreenshot
        {
            set
            {
                SerializedTests.ForceMakeReferenceScreenshot = value;
            }
            get
            {
                return SerializedTests.ForceMakeReferenceScreenshot;
            }
        }

        public static int RunTestsGivenAmountOfTimes
        {
            get { return RunTestsMode.RunTestsGivenAmountOfTimes; }
            set { RunTestsMode.RunTestsGivenAmountOfTimes = value; }
        }
        
        void RunMethod(object instance, MethodInfo info, Action callback)
        {
            if (info == null)
            {
                Debug.LogError("Can't run method, because method info is null, probably is not exist anymore");
                callback();
                return;
            }
            var returnType = info.ReturnType;
            object result;

            try
            {
                result = info.Invoke(info.IsStatic ? null : instance, null);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Fail during RunMethod: \"{0}\"", ex);

                callback();

                return;
            }

            if (returnType != typeof(IEnumerable) && returnType != typeof(IEnumerator))
            {
                callback();

                return;
            }

            var enumerable = result as IEnumerable;

            if (enumerable != null)
            {
                StartTestCoroutine(enumerable, instance.GetType().Name, info.Name, callback);

                return;
            }

            var enumerator = result as IEnumerator;
            if (enumerator != null)
            {
                StartTestCoroutine(enumerator, instance.GetType().Name, info.Name, callback);
            }
        }

        private IEnumerator InvokeTestMethod(object instance, MethodNode methodNode)
        {
            bool testFinished = false;

            RunMethod(instance, methodNode.TestSettings.MethodInfo, () =>
            {
                testFinished = true;
            });

            var currentTimer = 0f;

            var timeOut = methodNode.TestSettings.TimeOut / 1000;

            do
            {
                yield return null;

                currentTimer += Time.unscaledDeltaTime;

                if (currentTimer >= timeOut)
                {
                    Debug.LogErrorFormat("Method \"{0}\" failed by timeout.",
                        methodNode.FullName);

                    yield break;
                }
                // wait for done
            } while (!testFinished);

            yield return null; //we need to skip one frame to make sure error pause is executed
        }

        private void StartProcessingTests()
        {
            isRunning = true;

            logSaver.StartWrite();

            Application.logMessageReceived += ApplicationOnLogMessageReceived;

            StartCoroutine(ProcessTestQueue());
        }

        private void ApplicationOnLogMessageReceived(string condition, string stackTrace, LogType logType)
        {
            if (!isRunning)
            {
                return;
            }
            logSaver.Write(condition, stackTrace);
            if (logType == LogType.Exception || logType == LogType.Error)
            {
                if (PermittedErrors.IsPermittedError(condition, stackTrace))
                {
                    return;
                }
                
                StopTestCoroutine();
                if (CurrentPlayingMethodNode.Node != null)
                {
                    exceptionThrown = true;
                    testInfoData.AddFailed(CurrentPlayingMethodNode.Node.FullName,
                        condition + "   " + stackTrace);
                }
            }
        }

        public static void SavePreloadingTime(float time)
        {
            testInfoData.PreloadingTime = time;
        }
        
        private IEnumerator ProcessTestQueue()
        {
            Debug.LogFormat("[PlayModeTestRunner] ProcessTestQueueProcess: Mode {0}", RunTestsMode.Mode);

            ClassNode previousClassNode = null;

            ITestRunnerCallbackReceiver[] callbackables = TriggerBeforeTestRunEvent(Callbackables);

            List<MethodNode> methods = null;
            
            if (RunTestsMode.Mode == RunTestsMode.RunTestsModeEnum.DoubleClick)
            {
                if (!string.IsNullOrEmpty(RunTestsMode.NodeToRunFullName))
                {
                    var targetNodes = 
                        TestsRootNode.GetChildrenOfType<Node>(true, false,
                        node =>
                        {
                            return node.FullName.Equals(RunTestsMode.NodeToRunFullName, StringComparison.Ordinal);
                        });

                    if (targetNodes.Count > 0)
                    {
                        var targetNode = targetNodes[0];
                        if (targetNode is MethodNode)
                        {
                            methods = new List<MethodNode>
                            {
                                (MethodNode) targetNode
                            };       
                        }
                        else
                        {
                            methods = targetNode.GetChildrenOfType<MethodNode>();        
                        }
                    }
                }
            }
            
            if(methods == null || methods.Count == 0)
            {
                methods = TestsRootNode.GetChildrenOfType<MethodNode>();    
            }
            
            Debug.LogFormat("[PlayModeTestRunner] NonFilteredMethods: {0}", methods.Count);

            methods = methods.Where(methodNode =>
            {
                Debug.LogFormat("method data: IsSmoke: {0}  isSelected: {1}",
                    methodNode.TestSettings.IsSmoke,
                    methodNode.IsSelected);
                
                if (RunTestsMode.Mode == RunTestsMode.RunTestsModeEnum.Smoke
                    && !methodNode.TestSettings.IsSmoke)
                {
                    return false;
                }
                
                if (RunTestsMode.Mode == RunTestsMode.RunTestsModeEnum.Selected
                    && !methodNode.IsSelected)
                {
                    return false;
                }

                if (Namespaces != null && Namespaces.Length > 0)
                {
                    var result = false;
                    
                    for (int i = 0; i < Namespaces.Length; i++)
                    {
                        var contains = methodNode.FullName.IndexOf(Namespaces[i],
                                           StringComparison.OrdinalIgnoreCase) != -1;
                        result |= contains;
                    }

                    return result;
                }
                
                return true;
            }).ToList();
            
            Debug.LogFormat("[PlayModeTestRunner] FilteredMethods: {0}", methods.Count);
            
            testInfoData.Total = methods.Count;
            for (int i = 0; i <  methods.Count; i++)
            {
                var methodNode = methods[i];
                TestStep.Reset();
                ClassNode classNode = (ClassNode)methodNode.Parent;
                
                Debug.Log("QA: Process " + classNode.Type + " testClass.TestMethods #: " + classNode.Children.Count());
                CurrentPlayingMethodNode.UpdateCurrentPlayingNode(methodNode);
                playModeLogger.Logs.Clear();
                if (methodNode.TestSettings.IsIgnored)
                {
                    ProcessIgnore(methodNode);
                    continue;
                }

                for (int repeatTime = 0; repeatTime < RunTestsGivenAmountOfTimes; repeatTime++)
                {
                    exceptionThrown = false;
#if UNITY_EDITOR
                    if (methodNode.TestSettings.EditorTargetResolution != null)
                    {
                        CustomResolution resolution = methodNode.TestSettings.EditorTargetResolution;

                        if (resolution.Width != Screen.width ||
                            resolution.Height != Screen.height)
                        {
                            GameViewResizer.SetResolution(resolution.Width, resolution.Height);
                        }
                    }
#endif

                    if (!methodNode.TestSettings.ContainsTargetResolution(Screen.width, Screen.height))
                    {
#if UNITY_EDITOR
                        if (methodNode.TestSettings.EditorTargetResolution == null)
                        {
                            Debug.LogWarning(String.Format("PlayModeTestRunner: TARGET RESOLUTION {0}:{1}",
                                methodNode.TestSettings.DefaultTargetResolution.Width,
                                methodNode.TestSettings.DefaultTargetResolution.Height));

                            GameViewResizer.SetResolution(methodNode.TestSettings.DefaultTargetResolution.Width,
                                methodNode.TestSettings.DefaultTargetResolution.Height);
                        }
#else
                        ProcessIgnore(methodNode);
                        continue;
#endif
                    }

                    object testInstance = null;
                    screenGUIDrawer.SetCurrentTest(methodNode.FullName);
                    playModeLogger.StartLog(methodNode.FullName);

                    try
                    {
                        testInstance = Activator.CreateInstance(classNode.Type);
                    }
                    catch (Exception ex)
                    {
                        if (classNode.Type == null)
                        {
                            Debug.LogErrorFormat("Can't instantiate class - class type is null." +
                                                 " class probably was deleted ");
                        }
                        else
                        {
                            Debug.LogErrorFormat("Can't instantiate class \"{0}\". Exception: \"{1}\"",
                                classNode.Type.FullName, ex.Message);    
                        }
                        
                        ProcessTestFail(methodNode);
                        continue;
                    }

                    Time.timeScale = DefaultTimescale;
                    Debug.Log("QA: Running testClass: " + classNode.Type);

                    if (classNode != previousClassNode)
                    {
                        previousClassNode = classNode;

                        yield return RunMethods(classNode.OneTimeSetUpMethods, testInstance, true);

                        if (exceptionThrown)
                        {
                            Debug.LogError("Fail during executing set up methods.");
                            ProcessTestFail(methodNode);
                            continue;
                        }
                    }

                    yield return RunMethods(classNode.SetUpMethods, testInstance, true);

                    if (exceptionThrown)
                    {
                        Debug.LogError("Fail during executing set up methods.");
                        ProcessTestFail(methodNode);
                        continue;
                    }

                    yield return InvokeTestMethod(testInstance, methodNode);
                    var exceptionDuringTestMethod = exceptionThrown;
                    exceptionThrown = false;

                    yield return RunMethods(classNode.TearDownMethods, testInstance, false);

                    if (i + 1 == methods.Count ||
                        methods[i + 1].Parent != classNode)
                    {
                        yield return RunMethods(classNode.OneTimeTearDownMethods, testInstance, true);
                    }

                    if (!exceptionDuringTestMethod && exceptionThrown)
                    {
                        Debug.LogError("Fail during executing tear down methods.");
                        ProcessTestFail(methodNode);
                        continue;
                    }

                    if (exceptionDuringTestMethod)
                    {
                        ProcessTestFail(methodNode);
                        continue;
                    }

                    testInfoData.AddSuccess(methodNode.FullName);
                    playModeLogger.EndLog(methodNode.FullName);
                    methodNode.SetTestState(TestState.Passed);
                    methodNode.Logs = playModeLogger.LogsCopy;
                    methodNode.SetStep(TestStep.CurrentIndex);
                    
                    if (OnMethodStateUpdated != null)
                    {
                        OnMethodStateUpdated();
                    }
                }
            }
            
            TriggerAfterTestRunEvent(callbackables);

            FinalizeTest();
        }

        private IEnumerable<Type> Callbackables
        {
            get
            {
                var interfaceType = typeof(ITestRunnerCallbackReceiver);

                return Assembly.GetAssembly(typeof(ITestRunnerCallbackReceiver)).GetTypes().Where(p => p != interfaceType && interfaceType.IsAssignableFrom(p));
            }
        }

        private ITestRunnerCallbackReceiver[] TriggerBeforeTestRunEvent(IEnumerable<Type> classTypes)
        {
            List<ITestRunnerCallbackReceiver> result = new List<ITestRunnerCallbackReceiver>();

            foreach (var classType in classTypes)
            {
                ITestRunnerCallbackReceiver callbackableInstance = null;

                try
                {
                    callbackableInstance = (ITestRunnerCallbackReceiver)Activator.CreateInstance(classType);
                }
                catch (Exception ex)
                {
                    Debug.LogErrorFormat("Can't instantiate class \"{0}\". Exception: \"{1}\"",
                                         classType.FullName, ex.Message);

                    continue;
                }

                result.Add(callbackableInstance);

                callbackableInstance.OnBeforeTestsRunEvent();
            }

            return result.ToArray();
        }

        private void TriggerAfterTestRunEvent(ITestRunnerCallbackReceiver[] callbackableInstances)
        {
            foreach (ITestRunnerCallbackReceiver callbackableInstance in callbackableInstances)
            {
                callbackableInstance.OnAfterTestsRunEvent();
            }
        }

        private void ProcessIgnore(MethodNode method)
        {
            testInfoData.AddIgnored(method.FullName);
            playModeLogger.IgnoreLog(method.FullName,
                method.TestSettings.IgnoreReason ?? "ignored by ignore attribute");

            method.SetTestState(TestState.Ignored);
            method.Logs = playModeLogger.LogsCopy;
            method.SetStep(0);
            if (OnMethodStateUpdated != null)
            {
                OnMethodStateUpdated();
            }
        }

        private void ProcessTestFail(MethodNode method)
        {
            playModeLogger.FailLog(method.FullName);

            screenGUIDrawer.AddFailedTest(method.FullName);

            method.SetTestState(TestState.Failed);
            method.Logs = playModeLogger.LogsCopy;
            method.SetStep(0);
            if (OnMethodStateUpdated != null)
            {
                OnMethodStateUpdated();
            }
        }

        private void OnDisable()
        {
            Debug.LogWarning("Disabling playmodetestrunner: " + Environment.StackTrace);
            FinalizeTest();
        }

        private void FinalizeTest()
        {
            if (!IsRunning)
            {
                return;
            }
            
            Debug.LogWarning("TestsTools: Tests are done!");
            logSaver.SaveTestInfo(testInfoData);

            logSaver.Close();

            isRunning = false;
            
#if UNITY_EDITOR
            SaveTestsData();            
            EditorApplication.isPlaying = false;
            CurrentPlayingMethodNode.UpdateCurrentPlayingNode(null);
            
            if(QuitAppAfterCompleteTests)
            {
                Debug.Log("TestsTools: Exiting application!");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.Log("TestsTools: Exiting application not needed!");
            }
#else
            Debug.Log("QA: tests finished.");
            Application.Quit();
#endif 
        }

        private IEnumerator RunMethods(IEnumerable<MethodInfo> methods, object testInstance, bool checkFailedTests)
        {
            foreach (var method in methods)
            {
                bool done = false;

                RunMethod(testInstance, method, () =>
                {
                    done = true;
                });
                while (!done)
                {
                    yield return null;
                }

                if (checkFailedTests && exceptionThrown)
                {
                    break;
                }
            }
        }

        public static string GetTestScenePath()
        {
            var pathes = Directory.GetFiles(Application.dataPath, TEST_NAME_SCENE, SearchOption.AllDirectories);
            if (pathes.Length == 0)
            {
                Debug.LogError("Can't find test scene");
                return null;
            }

            var testScenePath = pathes[0];
            var assetsIndex = testScenePath.IndexOf("Assets", StringComparison.Ordinal);
            return testScenePath.Remove(0, assetsIndex);
        }

        private void OnGUI()
        {
            if (IsTestUIEnabled && screenGUIDrawer!=null)
            {
                screenGUIDrawer.Draw();   
            }
        }

#if UNITY_EDITOR

        public static void RunTestByDoubleClick(SpecificTestType type, Node node)
        {
            RunTestsMode.RunTestByDoubleClick(type, node);
            Run();
        }

        
        public static void Run()
        {
            Debug.LogFormat("[PlayModeTestRunner] Run: IsRunning {0}", IsRunning);
            if (IsRunning)
            {
                return;
            }
            
            var scenePath = GetTestScenePath();
            if (scenePath == null)
            {
                Debug.LogError("Cant find test scene");
                return;
            }
            SaveTestsData();
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            EditorApplication.isPlaying = true;
        }
#endif
        

    }
    
    public class TestInfoData
    {
        public class TestResultMessage
        {
            public string Name;
            public List<string> ErrorMessages = new List<string>();
        }
            
        public string Build;
        public float PreloadingTime;
        public int Total;
        public int Ignored;
        public int Failed;
        public int Success;
    
        public List<string> IgnoreTestsResults = new List<string>();
        public List<TestResultMessage> FailedTestsResults = new List<TestResultMessage>();
        public List<string> SuccessTestsResults = new List<string>();

        public TestInfoData()
        {
            Build = Application.version;
            var buildVersion = ConsoleArgumentHelper.GetArg("-buildNumber");
            if (!string.IsNullOrEmpty(buildVersion))
            {
                Build = Build + ":" + buildVersion;
            }
        }

        public void AddIgnored(string testName)
        {
            Ignored++;
            IgnoreTestsResults.Add(testName); 
        }
    
        public void AddSuccess(string testName)
        {
            Success++;
            SuccessTestsResults.Add(testName); 
        }
    
        public void AddFailed(string testName, string errorMessage)
        {
            var currentOne = FailedTestsResults.LastOrDefault();
            if (currentOne == null || currentOne.Name != testName)
            {
                Failed++;
                currentOne = new TestResultMessage();
                currentOne.Name = testName;
                FailedTestsResults.Add(currentOne);
            }
            currentOne.ErrorMessages.Add(errorMessage); 
        }
    }
}