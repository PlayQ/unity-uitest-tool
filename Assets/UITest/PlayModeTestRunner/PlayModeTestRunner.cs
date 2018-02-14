﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
 using NUnit.Framework;
 using UnityEngine;
 using UnityEngine.TestTools;
 using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlayQ.UITestTools
{   
    public class PlayModeTestRunner : MonoBehaviour
    {
        public const string PLAY_ONLY_SELECTED_TESTS = "playOnlySelectedTests";
        public const string QUIT_APP_AFTER_TESTS = "QuitAferComplete";
        
        public static Action<MethodInfo> OnTestPassed;
        public static Action<MethodInfo> OnTestIgnored;
        public static Action<MethodInfo> OnTestFailed;
        public static Action OnStartProcessingTests;
        
        public static bool IsRunning
        {
            get { return isRunning; }
        }
        
        public const string TEST_NAME_SCENE = "RuntimeTestScene.unity";
        private const string LOG_FILE_NAME = "runtimeTests.txt";

        private static List<UnitTestClass> classesForTest;

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
        private Coroutine currentTestCoroutine;
        private PlayModeLogger playModeLogger;
        private LogSaver logSaver;
        private PlayModeTestRunnerGUI screenGUIDrawer;
        private static bool isRunning;
        private SelectedTestsSerializable selectedTests;
        
        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
            playModeLogger = new PlayModeLogger();
            screenGUIDrawer = new PlayModeTestRunnerGUI();
            logSaver = new LogSaver(Path.Combine(Application.persistentDataPath, LOG_FILE_NAME));
            
            selectedTests = SelectedTestsSerializable.Load();
            classesForTest = GetTestClasses();
            RemoveUnselectedTests();
            StartProcessingTests();
        }

        private void RemoveUnselectedTests()
        {
            if (!selectedTests || (PlayerPrefs.HasKey(PLAY_ONLY_SELECTED_TESTS) && 
                PlayerPrefs.GetInt(PLAY_ONLY_SELECTED_TESTS) == 0))
            {
                PlayerPrefs.DeleteKey(PLAY_ONLY_SELECTED_TESTS);
                return;
            }
            var filteredClassesForTests = new List<UnitTestClass>();
            foreach(var classForTest in classesForTest)
            {
                var newClassForTest = new UnitTestClass(classForTest.Type);
                newClassForTest.SetUpMethods = classForTest.SetUpMethods;
                newClassForTest.TearDownMethods = classForTest.TearDownMethods;
                newClassForTest.TestMethods = new List<UnitTestMethod>();
                foreach (var methodForTest in classForTest.TestMethods)
                {
                    if (!selectedTests.TestInfoToMethodName.ContainsKey(methodForTest.FullName))
                    {
                        newClassForTest.TestMethods.Add(methodForTest);
                    }
                    else if (selectedTests.TestInfoToMethodName[methodForTest.FullName].IsSelected)
                    {
                        newClassForTest.TestMethods.Add(methodForTest);
                    }
                }
                if (newClassForTest.TestMethods.Count > 0)
                {
                    filteredClassesForTests.Add(newClassForTest);
                }
            }
            classesForTest = filteredClassesForTests;
        }
        
        private IEnumerator InvokeMethod(object instance, UnitTestMethod method)
        {
            var returnedType = method.Method.ReturnType;

            if (method.Sync)
            {
                if (returnedType != typeof(void))
                {
                    Debug.LogErrorFormat("Method \"{0}\" with attribute [Test] must return void.", method.FullName);
                    yield break;
                }
                method.Method.Invoke(method.Method.IsStatic ? null : instance, null);
                currentTestState = TestState.Success;
                yield break;
            }

            if (returnedType != typeof(IEnumerable) && returnedType != typeof(IEnumerator))
            {
                Debug.LogErrorFormat(
                    "Method \"{0}\" with attribute [UnityTest] must return IEnumerable or IEnumerator.",
                    method.FullName);
                yield break;
            }

            var returnedObject = method.Method.Invoke(method.Method.IsStatic ? null : instance, null);
            var enumerable = returnedObject as IEnumerable;
            if (enumerable != null)
            {
                foreach (YieldInstruction yieldInstruction in enumerable)
                {
                    yield return yieldInstruction;
                }
                currentTestState = TestState.Success;
                yield break;
            }

            var enumerator = returnedObject as IEnumerator;
            if (enumerator != null)
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
                currentTestState = TestState.Success;
                yield break;
            }
        }

        private IEnumerator InvokeTestMethod(object instance, UnitTestMethod method)
        {
            currentTestState = TestState.InProgress;
            currentTestCoroutine = StartCoroutine(InvokeMethod(instance, method));

            var currentTimer = 0f;
            var timeOut = method.TimeOut / 1000;
            do
            {
                yield return null;
                currentTimer += Time.unscaledDeltaTime;

                if (currentTimer >= timeOut)
                {
                    Debug.LogErrorFormat("Method \"{0}\" failed by timeout.", method.FullName);
                    yield break;
                }
                // wait for done
            } while (currentTestState == TestState.InProgress);
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
            if (logType == LogType.Exception || logType == LogType.Error)
            {
                if (currentTestCoroutine != null)
                {
                    StopCoroutine(currentTestCoroutine);
                }
                currentTestState = TestState.Failed;
            }
        }

        private IEnumerator ProcessTestQueue()
        {
            if (OnStartProcessingTests != null)
            {
                OnStartProcessingTests();
            }

            foreach (var testClass in classesForTest)
            {
                foreach (var method in testClass.TestMethods)
                {
                    currentTestState = TestState.NotInited;
                    if (method.IsIgnored)
                    {
                        currentTestState = TestState.Ignored;
                        playModeLogger.IgnoreLog(method.FullName);
                        if (OnTestIgnored != null)
                        {
                            OnTestIgnored(method.Method);
                        }
                        continue;
                    }

                    if (method.TargetResolution != null)
                    {
                        if (method.TargetResolution.Width != Screen.width ||
                            method.TargetResolution.Height != Screen.height)
                        {
#if UNITY_EDITOR
                            GameViewResizer.SetResolution(method.TargetResolution.Width,
                                method.TargetResolution.Height);
#else
                            playModeLogger.IgnoreLog(method.FullName);
                            if (OnTestIgnored != null)
                            {
                                OnTestIgnored(method.Method);
                            }
                            continue;
#endif
                        }
                    }

                    object testInstance = null;
                    currentMethodInfo = method.Method;
                    screenGUIDrawer.SetCurrentTest(method.FullName);
                    playModeLogger.StartLog(method.FullName);

                    try
                    {
                        testInstance = Activator.CreateInstance(testClass.Type);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogErrorFormat("Can't instantiate class \"{0}\". Exception: \"{1}\"",
                            testClass.Type.FullName, ex.Message);
                        ProcessTestFail(method);
                        continue;
                    }

                    try
                    {
                        RunMethods(testClass.SetUpMethods, testInstance);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogErrorFormat("Fail during executing set up methods: \"{0}\"", ex.Message);
                        ProcessTestFail(method);
                        continue;
                    }

                    yield return InvokeTestMethod(testInstance, method);

                    try
                    {
                        RunMethods(testClass.TearDownMethods, testInstance);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogErrorFormat("Fail during executing tear down methods: \"{0}\"", ex);
                        ProcessTestFail(method);
                        continue;
                    }

                    if (currentTestState == TestState.Success)
                    {
                        playModeLogger.SuccessLog(method.FullName);
                        if (OnTestPassed != null)
                        {
                            OnTestPassed(method.Method);
                        }
                    }
                    else
                    {
                        ProcessTestFail(method);
                    }
                }
            }
            FinalizeTest();
        }

        private void ProcessTestFail(UnitTestMethod method)
        {
            playModeLogger.FailLog(method.FullName);
            screenGUIDrawer.AddFailedTest(method.FullName);
            if (OnTestFailed != null)
            {
                OnTestFailed(method.Method);
            }
        }

        private void FinalizeTest()
        {
            logSaver.Close();
            isRunning = false;
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            if(PlayerPrefs.HasKey(QUIT_APP_AFTER_TESTS) && PlayerPrefs.GetInt(QUIT_APP_AFTER_TESTS) == 1)
            {
                PlayerPrefs.DeleteKey(QUIT_APP_AFTER_TESTS);
                EditorApplication.Exit(0);
            }
#else
            Application.Quit();
            #endif
        }

        private void RunMethods(List<MethodInfo> methods, object testInstance)
        {
            foreach (var method in methods)
            {
                method.Invoke(method.IsStatic ? null : testInstance, null);
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
            screenGUIDrawer.Draw();
        }

        public static List<UnitTestClass> GetTestClasses()
        {
            var findedClasses = new List<UnitTestClass>();
            var targetAssembly = Assembly.GetAssembly(typeof(PlayModeTestRunner));
            var types = targetAssembly.GetTypes();

            foreach (var type in types)
            {
                var methods = GetTestMethods(type);
                if (methods.Count == 0)
                {
                    continue;
                }
                var unitTestClass = new UnitTestClass(type)
                {
                    TestMethods = methods,
                    SetUpMethods = GetMethodsWithCustomAttribute<SetUpAttribute>(type),
                    TearDownMethods = GetMethodsWithCustomAttribute<TearDownAttribute>(type)
                };
                findedClasses.Add(unitTestClass);
            }

            return findedClasses;
        }

        private static List<UnitTestMethod> GetTestMethods(Type t)
        {
            var list = new List<UnitTestMethod>();
            var asyncTests = GetMethodsWithCustomAttribute<UnityTestAttribute>(t);
            foreach (var asyncTest in asyncTests)
            {
                list.Add(new UnitTestMethod(asyncTest, false));
            }

            var syncTests = GetMethodsWithCustomAttribute<TestAttribute>(t);
            foreach (var syncTest in syncTests)
            {
                list.Add(new UnitTestMethod(syncTest, true));
            }
            return list;
        }

        private static List<MethodInfo> GetMethodsWithCustomAttribute<T>(Type targetType)
        {
            var findedMethods = new List<MethodInfo>();
            var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                                BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                var attrs = method.GetCustomAttributes(false);
                foreach (var attr in attrs)
                {
                    var isTestMethod = attr.GetType() == typeof(T);
                    if (isTestMethod)
                    {
                        findedMethods.Add(method);
                        break;
                    }
                }
            }
            return findedMethods;
        }
    }
}