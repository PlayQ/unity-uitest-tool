using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public static partial class AsyncWait
    {
        [ShowInEditor(typeof(StartWaitingForLogClass), "Async Wait/Start Waiting For Log", false)]
        public static AbstractAsyncWaiter StartWaitingForLog(string message, bool isRegExp,
            float timeout = 10)
        {
            return new LogWaiter(message, isRegExp, timeout);
        }
        
        [ShowInEditor(typeof(StopWaitingForLogClass), "Async Wait/Stop Waiting For Log", false)]
        public static void StopWaitingForGameLog()
        {
            throw new Exception("StopWaitingForGameLog method is only for code generation, don't use it");
        }
        
        public class LogWaiter : AbstractAsyncWaiter
        {
            protected override string errorMessage
            {
                get { return "can't wait for log: " + message; }
            }

            private string message;
            private bool isComplete;
            private IStringComparator stringComparator;

            public LogWaiter(string message, bool isRegExp, float timeout)
            {
                Application.logMessageReceived += ApplicationOnLogMessageReceived;
                OnCompleteCallback += OnComplete;
                stringComparator = UITestUtils.GetStringComparator(message, isRegExp);
                this.message = message;
                StartWait(timeout);
            }

            private void OnComplete()
            {
                OnCompleteCallback -= OnComplete;
                Application.logMessageReceived -= ApplicationOnLogMessageReceived;
            }

            private void ApplicationOnLogMessageReceived(string condition, 
                string stackTrace, LogType logType)
            {
                if (logType == LogType.Error)
                {
                    return;
                }

                isComplete = stringComparator.TextEquals(condition);

                if (isComplete)
                {
                    Application.logMessageReceived -= ApplicationOnLogMessageReceived;
                }
            }
                

            protected override bool Check()
            {
                return isComplete;
            }
        }

        private class StartWaitingForLogClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return
                    new CreateWaitVariable<LogWaiter>()
                        .Append(new MethodName())
                        .String("empty log")
                        .Bool(false)
                        .Float(10);
            }
        }
        
        private class StopWaitingForLogClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return
                    new WaitForVariable<LogWaiter>();
            }
        }
        
    }
}