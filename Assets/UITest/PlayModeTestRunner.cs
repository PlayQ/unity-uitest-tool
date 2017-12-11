﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
 using UnityEngine.TestTools;

public class PlayModeTestRunner : MonoBehaviour
{
    private const string MONOBEHAVIOUR_NAME = "TestRunner";
    private const string TC_DATE_FORMAT = "yyyy-MM-ddThh:mm:ss.fff";

    public class TestedClass
    {
        public readonly List<MethodInfo> TestMethods;
        public readonly List<MethodInfo> SetUpMethods;
        public readonly List<MethodInfo> TearDownMethods;
        public readonly Type Target;

        public TestedClass(Type t)
        {
            Target = t;
            TestMethods = GetMethodsWithCustomAttribute<UnityTestAttribute>(Target);
            SetUpMethods = GetMethodsWithCustomAttribute<SetUpAttribute>(Target);
            TearDownMethods = GetMethodsWithCustomAttribute<TearDownAttribute>(Target);
        }
    }

    private static bool currentTestFailed;
    private static string currentMethodName;
    private static PlayModeTestRunner runnerBehaviour;

    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
#if UNITY_EDITOR
        CleanUp();
        StartProcessingTests();
#endif
    }

    public static IEnumerator RunThrowingIterator(IEnumerator enumerator)
    {
        while (!currentTestFailed)
        {
            object current;
            try
            {
                if (enumerator.MoveNext() == false)
                {
                    break;
                }
                current = enumerator.Current;
            }
            catch (Exception)
            {
                currentTestFailed = true;
                yield break;
            }
            yield return current;
        }
    }

    private static IEnumerator InvokeTestMethod(object instance, MethodInfo method)
    {
        var enumerator = (IEnumerator) method.Invoke(method.IsStatic ? null : instance, null);
        if (enumerator != null)
        {
            yield return RunThrowingIterator(enumerator);
        }
        else
        {
            Debug.LogErrorFormat("Can't process method \"{0}\". It need to return IEnumerator", method.Name);
        }
    }

    private static void StartProcessingTests()
    {
        var newGameObject = new GameObject(MONOBEHAVIOUR_NAME);
        runnerBehaviour = newGameObject.AddComponent<PlayModeTestRunner>();
        DontDestroyOnLoad(newGameObject);

        Application.logMessageReceived += ApplicationOnLogMessageReceived;
        runnerBehaviour.StartCoroutine(TestsCoroutine());
    }

    private static void ApplicationOnLogMessageReceived(string condition, string stackTrace, LogType logType)
    {
        if (logType == LogType.Exception || logType == LogType.Error)
        {
            currentTestFailed = true;
        }
    }

    private static IEnumerator TestsCoroutine()
    {
        foreach (var testClass in GetTestClasses())
        {
            foreach (var method in testClass.TestMethods)
            {
                object testInstance = null;
                currentTestFailed = false;
                currentMethodName = testClass.Target.FullName + "." + method.Name;
                StartTeamCityLog();

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

    private static void StartTeamCityLog()
    {
        Debug.Log(string.Format("##teamcity[testStarted timestamp='{0}' name='{1}']",
            DateTime.UtcNow.ToString(TC_DATE_FORMAT), currentMethodName));
    }

    private static void SuccessTeamCityLog()
    {
        Debug.Log(string.Format("##teamcity[testFinished timestamp='{0}' name='{1}']",
            DateTime.UtcNow.ToString(TC_DATE_FORMAT), currentMethodName));
    }

    private static void FailTeamCityLog()
    {
        Debug.Log(string.Format("##teamcity[testFailed timestamp='{0}' name='{1}']",
            DateTime.UtcNow.ToString(TC_DATE_FORMAT), currentMethodName));
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

    private static List<MethodInfo> GetStartUpMethods(Type targetType)
    {
        return GetMethodsWithCustomAttribute<SetUpAttribute>(targetType);
    }

    private static List<MethodInfo> GetTearsDownMethods(Type targetType)
    {
        return GetMethodsWithCustomAttribute<TearDownAttribute>(targetType);
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