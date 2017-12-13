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
        public static Action<MethodInfo> OnTestPassed;
        public static Action<MethodInfo> OnTestIgnored;
        public static Action<MethodInfo> OnTestFailed;
        public static Action OnStartProcessingTests;

        public const string TEST_NAME_SCENE = "RuntimeTestScene.unity";
        public const float DEFAULT_TIME_OUT = 30000f;
        private const string LOG_FILE_NAME = "runtimeTests.txt";

        private static List<UnitTestClass> classesForTest;

        private bool currentTestFailed;
        private bool currentTestSuccess;
        private Coroutine cachedCoroutine;
        private PlayModeLogger logger;
        private LogSaver logSaver;
        private PlayModeTestRunnerGUI guiDrawer;
        
        // Mono behaviour 
        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
            logger = new PlayModeLogger();
            guiDrawer = new PlayModeTestRunnerGUI();
            logSaver = new LogSaver(Path.Combine(Application.persistentDataPath, LOG_FILE_NAME));
            classesForTest = GetTestClasses();

            StartProcessingTests();
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
                currentTestSuccess = true;
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
                currentTestSuccess = true;
                yield break;
            }

            var enumerator = returnedObject as IEnumerator;
            if (enumerator != null)
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
                currentTestSuccess = true;
                yield break;
            }
        }

        private IEnumerator InvokeTestMethod(object instance, UnitTestMethod method)
        {
            currentTestFailed = false;
            currentTestSuccess = false;
            cachedCoroutine = StartCoroutine(InvokeMethod(instance, method));

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
            } while (!currentTestFailed && !currentTestSuccess);
        }

        private void StartProcessingTests()
        {
            logSaver.StartWrite();
            Application.logMessageReceived += ApplicationOnLogMessageReceived;
            StartCoroutine(ProcessTestQueue());
        }

        private void ApplicationOnLogMessageReceived(string condition, string stackTrace, LogType logType)
        {
            logSaver.Write(condition, stackTrace);
            if (logType == LogType.Exception || logType == LogType.Error)
            {
                if (cachedCoroutine != null)
                {
                    StopCoroutine(cachedCoroutine);
                }
                currentTestFailed = true;
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
                    if (method.IsIgnored)
                    {
                        logger.IgnoreLog(method.FullName);
                        if (OnTestIgnored != null)
                        {
                            OnTestIgnored(method.Method);
                        }
                        continue;
                    }

                    object testInstance = null;
                    guiDrawer.SetCurrentTest(method.FullName);
                    logger.StartLog(method.FullName);

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

                    if (!currentTestFailed)
                    {
                        logger.SuccessLog(method.FullName);
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
            logger.FailLog(method.FullName);
            guiDrawer.AddFailedTest(method.FullName);
            if (OnTestFailed != null)
            {
                OnTestFailed(method.Method);
            }
        }

        private void FinalizeTest()
        {
            logSaver.Close();

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
            #endif
        }

        private void Quit()
        {
#if !UNITY_EDITOR
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
        
        // GUI

        private void OnGUI()
        {
            guiDrawer.Draw();
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