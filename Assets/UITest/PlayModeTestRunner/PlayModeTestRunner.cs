﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PlayQ.UITestTools
{
    public class PlayModeTestRunner : MonoBehaviour
    {
        public const float DEFAULT_TIME_OUT = 30000f;
        private const string LOG_FILE_NAME = "runtimeTests.txt";

        private bool currentTestFailed;
        private bool currentTestSuccess;
        private Coroutine cachedCoroutine;
        private PlayModeLogger logger;
        private LogSaver logSaver;

        // Mono behaviour 
        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
            logger = new PlayModeLogger();
            logSaver = new LogSaver(Path.Combine(Application.persistentDataPath, LOG_FILE_NAME));

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
            foreach (var testClass in GetTestClasses())
            {
                foreach (var method in testClass.TestMethods)
                {
                    if (method.IsIgnored)
                    {
                        logger.IgnoreLog(method.FullName);
                        continue;
                    }

                    object testInstance = null;
                    logger.StartLog(method.FullName);

                    try
                    {
                        testInstance = Activator.CreateInstance(testClass.Target);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogErrorFormat("Can't instantiate class \"{0}\". Exception: \"{1}\"",
                            testClass.Target.FullName, ex.Message);
                        logger.FailLog(method.FullName);
                        continue;
                    }

                    try
                    {
                        RunMethods(testClass.SetUpMethods, testInstance);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogErrorFormat("Fail during executing set up methods: \"{0}\"", ex.Message);
                        logger.FailLog(method.FullName);
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
                        logger.FailLog(method.FullName);
                        continue;
                    }

                    if (!currentTestFailed)
                    {
                        logger.SuccessLog(method.FullName);
                    }
                    else
                    {
                        logger.FailLog(method.FullName);
                    }
                }
            }
            FinalizeTest();
        }

        private void FinalizeTest()
        {
            logSaver.Close();
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