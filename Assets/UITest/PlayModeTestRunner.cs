﻿using System;
using System.Collections;
using System.Collections.Generic;
 using System.Linq;
 using System.Reflection;
using NUnit.Framework;
using UnityEditor;
 using UnityEditorInternal;
 using UnityEngine;
 using UnityEngine.TestTools;

namespace PlayQ.UITestTools
{
    public class PlayModeTestRunner : MonoBehaviour
    {
        private const string MONOBEHAVIOUR_NAME = "TestRunner";
        private const float DEFAULT_TIME_OUT = 30000f;
        private const string TC_DATE_FORMAT = "yyyy-MM-ddThh:mm:ss.fff";

        private const string DELIMETER =
            "===================================================================================";

        private class TestedClass
        {
            public class TestedMethod
            {
                public readonly string FullName;
                public readonly bool Sync;
                public readonly MethodInfo Method;
                public readonly float TimeOut = DEFAULT_TIME_OUT;
                public readonly bool IsIgnored;

                public TestedMethod(MethodInfo methodInfo, bool sync)
                {
                    Method = methodInfo;
                    Sync = sync;
                    FullName = methodInfo.DeclaringType.Namespace + methodInfo.Name;

                    var attrs = methodInfo.GetCustomAttributes(false);
                    foreach (var attr in attrs)
                    {
                        var timeOut = attr as TimeoutAttribute;
                        if (timeOut != null)
                        {
                            TimeOut = (int) timeOut.Properties.Get("Timeout");
                        }
                        var ignore = attr as IgnoreAttribute;
                        if (ignore != null)
                        {
                            IsIgnored = true;
                        }
                    }
                }
            }

            public List<TestedMethod> TestMethods;
            public readonly List<MethodInfo> SetUpMethods;
            public readonly List<MethodInfo> TearDownMethods;
            public readonly Type Target;

            public TestedClass(Type t)
            {
                Target = t;
                SetUpMethods = GetMethodsWithCustomAttribute<SetUpAttribute>(Target);
                TearDownMethods = GetMethodsWithCustomAttribute<TearDownAttribute>(Target);
                FillTestMethods();
            }

            private void FillTestMethods()
            {
                TestMethods = new List<TestedMethod>();
                var asyncTests = GetMethodsWithCustomAttribute<UnityTestAttribute>(Target);
                foreach (var asyncTest in asyncTests)
                {
                    TestMethods.Add(new TestedMethod(asyncTest, false));
                }

                var syncTests = GetMethodsWithCustomAttribute<TestAttribute>(Target);
                foreach (var syncTest in syncTests)
                {
                    TestMethods.Add(new TestedMethod(syncTest, true));
                }
            }
        }

        private static bool currentTestFailed;
        private static bool currentTestSuccess;
        private static string currentMethodName;
        private static PlayModeTestRunner sceneMonobehaviour;
        private static Coroutine cachedCoroutine;

        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
#if UNITY_EDITOR
            CleanUp();
            StartProcessingTests();
#endif
        }

        private static IEnumerator InvokeMethod(object instance, TestedClass.TestedMethod method)
        {
            var returnedType = method.Method.ReturnType;

            if (method.Sync)
            {
                if (returnedType != typeof(void))
                {
                    Debug.LogErrorFormat("Method \"{0}\" with attribute [Test] must return void.", currentMethodName);
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
                    currentMethodName);
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

        private static IEnumerator InvokeTestMethod(object instance, TestedClass.TestedMethod method)
        {
            currentTestFailed = false;
            currentTestSuccess = false;
            cachedCoroutine = sceneMonobehaviour.StartCoroutine(InvokeMethod(instance, method));

            var currentTimer = 0f;
            var timeOut = method.TimeOut / 1000;
            do
            {
                yield return null;
                currentTimer += Time.unscaledDeltaTime;

                if (currentTimer >= timeOut)
                {
                    Debug.LogErrorFormat("Method \"{0}\" failed by timeout.", currentMethodName);
                    yield break;
                }
                // wait for done
            } while (!currentTestFailed && !currentTestSuccess);
        }

        private static void StartProcessingTests()
        {
            var newGameObject = new GameObject(MONOBEHAVIOUR_NAME);
            sceneMonobehaviour = newGameObject.AddComponent<PlayModeTestRunner>();
            DontDestroyOnLoad(newGameObject);

            Application.logMessageReceived += ApplicationOnLogMessageReceived;
            sceneMonobehaviour.StartCoroutine(ProcessTestQueue());
        }

        private static void TerminateTest()
        {
            if (cachedCoroutine != null)
            {
                sceneMonobehaviour.StopCoroutine(cachedCoroutine);
            }
            currentTestFailed = true;
        }

        private static void ApplicationOnLogMessageReceived(string condition, string stackTrace, LogType logType)
        {
            if (logType == LogType.Exception || logType == LogType.Error)
            {
                TerminateTest();
            }
        }

        private static IEnumerator ProcessTestQueue()
        {
            foreach (var testClass in GetTestClasses())
            {
                foreach (var method in testClass.TestMethods)
                {
                    if (method.IsIgnored)
                    {
                        IgnoreLog();
                        continue;
                    }

                    object testInstance = null;
                    currentMethodName = testClass.Target.FullName + "." + method.FullName;
                    StartLog();

                    try
                    {
                        testInstance = Activator.CreateInstance(testClass.Target);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        FailTeamCityLog();
                        continue;
                    }

                    try
                    {
                        RunMethods(testClass.SetUpMethods, testInstance);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogErrorFormat("Fail during executing set up methods: \"{0}\"", ex);
                        FailTeamCityLog();
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
                        FailTeamCityLog();
                        continue;
                    }

                    if (!currentTestFailed)
                    {
                        SuccessTeamCityLog();
                    }
                    else
                    {
                        FailTeamCityLog();
                    }
                }
            }

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        private static void RunMethods(List<MethodInfo> methods, object testInstance)
        {
            foreach (var method in methods)
            {
                method.Invoke(method.IsStatic ? null : testInstance, null);
            }
        }

        private static void IgnoreLog()
        {
            Debug.Log(DELIMETER);
            Debug.LogFormat("\"{0}\" ignored.", currentMethodName);
            Debug.LogFormat("##teamcity[testIgnored timestamp='{0}' name='{1}']",
                DateTime.UtcNow.ToString(TC_DATE_FORMAT), currentMethodName);
            Debug.Log(DELIMETER);
        }

        private static void StartLog()
        {
            Debug.Log(DELIMETER);
            Debug.LogFormat("\"{0}\" setup", currentMethodName);
            Debug.LogFormat("##teamcity[testStarted timestamp='{0}' name='{1}']",
                DateTime.UtcNow.ToString(TC_DATE_FORMAT), currentMethodName);
        }

        private static void SuccessTeamCityLog()
        {
            Debug.LogFormat("##teamcity[testFinished timestamp='{0}' name='{1}']",
                DateTime.UtcNow.ToString(TC_DATE_FORMAT), currentMethodName);
            Debug.LogFormat("\"{0}\" success", currentMethodName);
            Debug.Log(DELIMETER);
        }

        private static void FailTeamCityLog()
        {
            Debug.LogFormat("##teamcity[testFailed timestamp='{0}' name='{1}']",
                DateTime.UtcNow.ToString(TC_DATE_FORMAT),
                currentMethodName);
            Debug.LogFormat("\"{0}\" fail", currentMethodName);
            Debug.Log(DELIMETER);
        }

        private static void CleanUp()
        {
            var objects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var gameObject in objects)
            {
                if (gameObject.name != MONOBEHAVIOUR_NAME)
                {
                    GameObject.DestroyImmediate(gameObject);
                }
            }
        }

        private static List<MethodInfo> GetMethodsWithCustomAttribute<T>(Type targetType)
        {
            var findedMethods = new List<MethodInfo>();
            var methods = targetType.GetMethods();
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

        private static List<TestedClass> GetTestClasses()
        {
            var findedClasses = new List<TestedClass>();
            var targetAssembly = Assembly.GetAssembly(typeof(PlayModeTestRunner));
            var types = targetAssembly.GetTypes();

            foreach (var type in types)
            {
                findedClasses.Add(new TestedClass(type));
            }

            return findedClasses;
        }
    }
}