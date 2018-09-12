using System;
using System.Collections.Generic;
using UnityEngine;


namespace PlayQ.UITestTools
{
    /// <summary>
    /// https://confluence.jetbrains.com/display/TCD9/Build+Script+Interaction+with+TeamCity
    /// </summary>
    public class PlayModeLogger
    {
        public const string TC_DATE_FORMAT = "yyyy-MM-ddThh:mm:ss.fff";
        public const string FILE_DATE_FORMAT = "yyyy-MM-ddThh.mm.ss.fff";

        public List<string> Logs = new List<string>();

        private const string DELIMETER =
            "===================================================================================";

        public List<string> LogsCopy { get { return new List<string>(Logs); } }

        public void IgnoreLog(string methodName, string reason)
        {
            HookToDebugLogs();

            Debug.Log(DELIMETER);
            Debug.LogFormat("\"{0}\" ignored. Reason: {1}", methodName, reason);
            Debug.LogFormat("##teamcity[testIgnored timestamp='{0}' name='{1}' message='{2}']",
                DateTime.UtcNow.ToString(TC_DATE_FORMAT), methodName, reason);
            Debug.Log(DELIMETER);

            UnhookFromDebugLogs();
        }

        public void StartLog(string methodName)
        {
            Logs.Clear();
            HookToDebugLogs();

            Debug.Log(DELIMETER);
            Debug.LogFormat("\"{0}\" setup", methodName);
            Debug.LogFormat("##teamcity[testStarted timestamp='{0}' name='{1}']",
                DateTime.UtcNow.ToString(TC_DATE_FORMAT), methodName);
        }

        private void HookToDebugLogs()
        {
            Application.logMessageReceived += CaptureLog;
            //Application.logMessageReceivedThreaded += CaptureLogThread;
        }

        private void UnhookFromDebugLogs()
        {
            Application.logMessageReceived -= CaptureLog;
            //Application.logMessageReceivedThreaded -= CaptureLogThread;
        }

        void CaptureLog(string condition, string stacktrace, LogType type)
        {
            string log;
            if (type == LogType.Log)
            {
                log = condition;
            }
            else
            {
                log = String.Format("{2}: {0}\n{1}", condition, stacktrace, type.ToString().ToUpper());
            }

            Logs.Add(log);
        }

        void CaptureLogThread(string condition, string stacktrace, LogType type)
        {
            lock (Logs)
            {
                CaptureLog(condition, stacktrace, type);
            }
        }

        public void SuccessLog(string methodName)
        {
            Debug.LogFormat("##teamcity[testFinished timestamp='{0}' name='{1}']",
                DateTime.UtcNow.ToString(TC_DATE_FORMAT), methodName);
            Debug.LogFormat("\"{0}\" success", methodName);
            Debug.Log(DELIMETER);

            UnhookFromDebugLogs();
        }

        public void FailLog(string methodName)
        {
            Debug.LogFormat("##teamcity[testFailed timestamp='{0}' name='{1}']",
                DateTime.UtcNow.ToString(TC_DATE_FORMAT),
                methodName);
            Debug.LogFormat("\"{0}\" fail", methodName);
            Debug.Log(DELIMETER);

            UnhookFromDebugLogs();
        }

        public void LogTestStart()
        {
            StartLog(PlayModeTestRunner.CurrentMethodInfo.DeclaringType.Name + "." +
                     PlayModeTestRunner.CurrentMethodInfo.Name);
        }

        public void LogTestEnd()
        {
            var methodName = PlayModeTestRunner.CurrentMethodInfo.DeclaringType.Name + "." +
                             PlayModeTestRunner.CurrentMethodInfo.Name;
            var currentState = PlayModeTestRunner.CurrentTestState;

            switch (currentState)
            {
                case PlayModeTestRunner.TestState.Failed:
                    FailLog(methodName);
                    break;
                case PlayModeTestRunner.TestState.Success:
                    SuccessLog(methodName);
                    break;
                case PlayModeTestRunner.TestState.Ignored:
                    IgnoreLog(methodName, "");
                    break;
                default:
                    Debug.LogFormat("##teamcity["+ currentState +" timestamp='{0}' name='{1}']",
                        DateTime.UtcNow.ToString(TC_DATE_FORMAT),
                        methodName);
                    Debug.LogFormat("\"{0}\" fail", methodName);
                    Debug.Log(DELIMETER);
                    break;
            }
        }
    }
}