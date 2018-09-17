using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Screen = UnityEngine.Screen;
using Tests.Nodes;
using Tests;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlayQ.UITestTools
{
    public class PlayModeTestRunner : TestCoroutiner
    {
        public enum SpecificTestType { Class, Method }

        public static bool IsTestUIEnabled = true;

        public static event Action<string, List<string>> OnTestPassed;
        public static event Action<string, List<string>> OnTestIgnored;
        public static event Action<string, List<string>> OnTestFailed;

        public static bool IsRunning
        {
            get { return isRunning && Application.isPlaying; }
        }

        public const string TEST_NAME_SCENE = "RuntimeTestScene.unity";
        private const string LOG_FILE_NAME = "runtimeTests.txt";

        private static Node testsRootNode;
        public static Node TestsRootNode
        {
            get
            {
                if (testsRootNode == null)
                    RefreshTestsRootNode();

                return testsRootNode;
            }
        }

        public static int TestsAmount = -1;

        private static List<MethodNode> methodsToTestQueue = new List<MethodNode>();

        public static void RefreshTestsRootNode()
        {
            testsRootNode = NodeFactory.Build();

            TestsAmount = ((IAllMethodsEnumerable)testsRootNode).AllMethods.Count();
        }

        private static TestInfoData testInfoData;

        public enum TestState
        {
            NotInited,
            InProgress,
            Failed,
            Success,
            Ignored
        }


        public static MethodInfo CurrentMethodInfo
        {
            get { return currentMethodInfo; }
        }
        public static TestState CurrentTestState
        {
            get { return currentTestState; }
        }

        private static MethodInfo currentMethodInfo;
        private static TestState currentTestState;
        private PlayModeLogger playModeLogger;
        private LogSaver logSaver;
        private PlayModeTestRunnerGUI screenGUIDrawer;
        private static bool isRunning;


        private static SelectedTestsSerializable serializedTests;
        public static SelectedTestsSerializable SerializedTests
        {
            get
            {
#if UNITY_EDITOR
                return SelectedTestsSerializable.CreateOrLoad();
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
            Debug.Log("--------Playmode test runner awakes");
            DontDestroyOnLoad(gameObject);
            playModeLogger = new PlayModeLogger();
            screenGUIDrawer = new PlayModeTestRunnerGUI();
            logSaver = new LogSaver(Path.Combine(Application.persistentDataPath, LOG_FILE_NAME));
            serializedTests = SelectedTestsSerializable.Load();
            testInfoData = new TestInfoData();

            if (testsRootNode == null)
                RefreshTestsRootNode();

            AddSelectedMethodsToTestQueue();
            StartProcessingTests();
        }

        public static bool QuitAppAfterCompleteTests
        {
            set
            {
                SerializedTests.QuitAferComplete = value;
            }
            get { return SerializedTests.QuitAferComplete; }
        }

        public static float DefaultTimescale
        {
            set
            {
                SerializedTests.DefaultTimescale = value;
            }
            get
            {
                return SerializedTests.DefaultTimescale;
            }
        }

        public static int RunTestsGivenAmountOfTimes
        {
            get { return RunTestsMode.RunTestsGivenAmountOfTimes; }
            set { RunTestsMode.RunTestsGivenAmountOfTimes = value; }
        }

#if UNITY_EDITOR


        public static void AdjustSelectedTestsForBuild()
        {
            var selectedTests = SelectedTestsSerializable.CreateOrLoad();

            var runMode = RunTestsMode.Mode;

            var methodsToTest = new Dictionary<string, MethodNode>();

            foreach (ClassNode classNode in ((IAllClassesEnumerable)TestsRootNode).AllClasses)
            {
                foreach (MethodNode methodNode in ((IAllMethodsEnumerable)classNode).AllMethods)
                {
                    methodsToTest.Add(methodNode.FullName, methodNode);

                    if (runMode == RunTestsMode.RunTestsModeEnum.All
                        || (runMode == RunTestsMode.RunTestsModeEnum.Smoke && methodNode.TestSettings.IsSmoke))
                    {
                        var testInfo = selectedTests.GetTest(methodNode.FullName);

                        if (testInfo != null)
                        {
                            testInfo.IsSelected = true;
                        }
                        else
                        {
                            selectedTests.AddTest(methodNode.FullName,
                                                  new SelectedTestsSerializable.TestInfo(methodNode.FullName, classNode.FullName)
                                                  {
                                                      ClassName = classNode.FullName,
                                                      IsSelected = true
                                                  });
                        }
                    }
                }
            }

            var testsToRemove = new List<string>();

            foreach (var methodName in selectedTests.TestInfoKeys)
            {
                if (!methodsToTest.ContainsKey(methodName))
                {
                    testsToRemove.Add(methodName);
                }
            }

            foreach (var key in testsToRemove)
            {
                selectedTests.RemoveTest(key);
            }

            AssetDatabase.SaveAssets();
        }

#endif

        private void AddSelectedMethodsToTestQueue()
        {
            methodsToTestQueue.Clear();

            var mode = RunTestsMode.Mode;

            int testRunsAmount = RunTestsMode.RunTestsGivenAmountOfTimes;

            Debug.Log("QA: selectedTests: " + SerializedTests.TestInfoKeys.Count);

            Node newTestsRootNode = new HierarchicalNode(string.Empty, null);

            for (int i = 0; i < testRunsAmount; i++)
            {
                foreach (MethodNode methodNode in ((IAllMethodsEnumerable)testsRootNode).AllMethods)
                {
                    ClassNode classNode = (ClassNode)methodNode.Parent;

                    var isMethodSmoke = methodNode.TestSettings.IsSmoke;

                    var isMethodSelected = SerializedTests.ContainsTest(methodNode.FullName)
                                           && SerializedTests.GetTest(methodNode.FullName).IsSelected;

                    switch (mode)
                    {
                        case RunTestsMode.RunTestsModeEnum.All:

                            methodsToTestQueue.Add(methodNode);

                            break;

                        case RunTestsMode.RunTestsModeEnum.Smoke:

                            if (isMethodSmoke)
                            {
                                methodsToTestQueue.Add(methodNode);
                            }

                            break;

                        case RunTestsMode.RunTestsModeEnum.Selected:

                            if (isMethodSelected)
                            {
                                methodsToTestQueue.Add(methodNode);
                            }

                            break;

                        case RunTestsMode.RunTestsModeEnum.DoubleClick:

                            string specificTestOrClassName = RunTestsMode.SpecificTestOrClassName;

                            switch (RunTestsMode.RunSpecificTestType)
                            {
                                case SpecificTestType.Class:

                                    if (classNode.Type.FullName.Contains(specificTestOrClassName))
                                    {
                                        methodsToTestQueue.Add(methodNode);
                                    }
                                    else
                                    {
                                        break;
                                    }

                                    break;

                                case SpecificTestType.Method:

                                    if (methodNode.FullName == specificTestOrClassName)
                                    {
                                        methodsToTestQueue.Add(methodNode);
                                    }
                                    else
                                    {
                                        break;
                                    }

                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        void RunMethod(object instance, MethodInfo info, Action callback)
        {
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
            currentTestState = TestState.InProgress;

            bool testFinished = false;

            RunMethod(instance, methodNode.TestSettings.Method, () =>
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
                    Debug.LogErrorFormat("Method \"{0}\" failed by timeout.", methodNode.FullName);

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
            logSaver.Write(condition, stackTrace);
            if ((logType == LogType.Exception || logType == LogType.Error))
            {
                if (logType == LogType.Error &&
                    (stackTrace == "UnityEditor.AsyncHTTPClient:Done(State, Int32)\n"
                     || PermittedErrors.IsPermitted(condition)))
                {
                    return;
                }
                StopTestCoroutine();
                currentTestState = TestState.Failed;
                testInfoData.AddFailed(CurrentMethodInfo.DeclaringType.FullName +
                                       "." + CurrentMethodInfo.Name,
                    condition + "   " + stackTrace);
            }
        }

        private IEnumerator ProcessTestQueue()
        {

            Debug.Log("QA: Process classesForTest #: " + testsRootNode.Children.Count());

            ClassNode previousClassNode = null;

            ITestRunnerCallbackReceiver[] callbackables = TriggerBeforeTestRunEvent(Callbackables);

            //foreach (MethodNode methodNode in methodsToTestQueue)
            for (int i = 0; i < methodsToTestQueue.Count; i++)
            {
                MethodNode methodNode = methodsToTestQueue[i];

                ClassNode classNode = (ClassNode)methodNode.Parent;

                Debug.Log("QA: Process " + classNode.Type + " testClass.TestMethods #: " + classNode.Children.Count());

                playModeLogger.Logs.Clear();

                testInfoData.Total++;

                currentTestState = TestState.NotInited;

                if (methodNode.TestSettings.IsIgnored)
                {
                    ProcessIgnore(methodNode);

                    continue;
                }

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
                        Debug.LogWarning(String.Format("PlayModeTestRunner: TARGET RESOLUTION {0}:{1}", methodNode.TestSettings.DefaultTargetResolution.Width, methodNode.TestSettings.DefaultTargetResolution.Height));

                        GameViewResizer.SetResolution(methodNode.TestSettings.DefaultTargetResolution.Width,
                                                      methodNode.TestSettings.DefaultTargetResolution.Height);
                    }
#else
                        ProcessIgnore(method);
                        continue;
#endif
                }

                object testInstance = null;

                currentMethodInfo = methodNode.TestSettings.Method;

                screenGUIDrawer.SetCurrentTest(methodNode.FullName);

                playModeLogger.StartLog(methodNode.FullName);

                try
                {
                    testInstance = Activator.CreateInstance(classNode.Type);
                }
                catch (Exception ex)
                {
                    Debug.LogErrorFormat("Can't instantiate class \"{0}\". Exception: \"{1}\"",
                                         classNode.Type.FullName, ex.Message);

                    ProcessTestFail(methodNode);

                    continue;
                }

#if UNITY_EDITOR
                Time.timeScale = PlayModeTestRunner.DefaultTimescale;
#endif
                Debug.Log("QA: Running testClass: " + classNode.Type);
                currentTestState = TestState.InProgress;

                if (classNode != previousClassNode)
                {
                    previousClassNode = classNode;

                    yield return RunMethods(classNode.OneTimeSetUpMethods, testInstance, true);

                    if (currentTestState == TestState.Failed)
                    {
                        Debug.LogError("Fail during executing set up methods.");

                        ProcessTestFail(methodNode);

                        continue;
                    }
                }

                yield return RunMethods(classNode.SetUpMethods, testInstance, true);

                if (currentTestState == TestState.Failed)
                {
                    Debug.LogError("Fail during executing set up methods.");

                    ProcessTestFail(methodNode);

                    continue;
                }

                yield return InvokeTestMethod(testInstance, methodNode);
                var previousState = currentTestState;

                yield return RunMethods(classNode.TearDownMethods, testInstance, false);

                if (i + 1 == methodsToTestQueue.Count || methodsToTestQueue[i + 1].Parent != classNode)
                {
                    yield return RunMethods(classNode.OneTimeTearDownMethods, testInstance, true);
                }

                if (currentTestState != previousState && currentTestState == TestState.Failed)
                {
                    Debug.LogError("Fail during executing tear down methods.");

                    ProcessTestFail(methodNode);

                    continue;
                }

                if (currentTestState == TestState.InProgress)
                {
                    currentTestState = TestState.Success;
                    testInfoData.AddSuccess(methodNode.FullName);

                    playModeLogger.SuccessLog(methodNode.FullName);

                    if (OnTestPassed != null)
                    {
                        OnTestPassed(methodNode.FullName, playModeLogger.LogsCopy);
                    }
                }
                else
                {
                    ProcessTestFail(methodNode);
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

                return AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => p != interfaceType && interfaceType.IsAssignableFrom(p));
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

            currentTestState = TestState.Ignored;

            playModeLogger.IgnoreLog(method.FullName, method.TestSettings.IgnoreReason ?? "ignored by ignore attribute");

            if (OnTestIgnored != null)
            {
                OnTestIgnored(method.FullName, playModeLogger.LogsCopy);
            }
            
        }

        private void ProcessTestFail(MethodNode method)
        {
            playModeLogger.FailLog(method.FullName);

            screenGUIDrawer.AddFailedTest(method.FullName);

            if (OnTestFailed != null)
            {
                OnTestFailed(method.FullName, playModeLogger.LogsCopy);
            }
        }

        private void FinalizeTest()
        {
            logSaver.SaveTestInfo(testInfoData);

            logSaver.Close();

            isRunning = false;
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            
            if(QuitAppAfterCompleteTests)
            {
                EditorApplication.Exit(0);
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

                if (checkFailedTests && currentTestState == TestState.Failed)
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

        public static void RunTestByDoubleClick(SpecificTestType type, string testFullName)
        {
            RunTestsMode.RunTestByDoubleClick(type, testFullName);
            Run();
        }

        
        public static void Run()
        {
            var scenePath = PlayModeTestRunner.GetTestScenePath();
            if (scenePath == null)
            {
                Debug.LogError("Cant find test scene");
                return;
            }
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            EditorApplication.isPlaying = true;
        }
#endif
    }

    public class TestInfoData
    {
        public string Build;
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

    public class TestResultMessage
    {
        public string Name;
        public List<string> ErrorMessages = new List<string>();
    }
}