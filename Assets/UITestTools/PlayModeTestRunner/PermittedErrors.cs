using System;
using System.Collections.Generic;

namespace PlayQ.UITestTools
{
    public static class PermittedErrors
    {
        struct ErrorInfo
        {
            public string Message;
            public bool IsExactMessage;
            public string Stacktrace;
            public bool IsExactStacktrace;
        }
        
        private static readonly List<ErrorInfo> erroInfosPermanent = new List<ErrorInfo>();
        
        private static List<ErrorInfo> erroInfos = new List<ErrorInfo>();

        public static void AddPermittedError(string message, string stacktrace = "", 
            bool isExactMessage = false, bool isExactStacktrace = false)
        {
            erroInfos.Add(new ErrorInfo
            {
                Message = message,
                Stacktrace = stacktrace,
                IsExactMessage = isExactMessage,
                IsExactStacktrace = isExactStacktrace
            });
        }

        public static void Clear()
        {
            erroInfos.Clear();
        }

        private static bool IsPermittedError(List<ErrorInfo> errorInfos, string message, string stacktrace)
        {
            foreach (var errorInfo in errorInfos)
            {
                var messageCheck = false;
                if (errorInfo.IsExactMessage)
                {
                    messageCheck = message.Equals(errorInfo.Message, StringComparison.Ordinal);
                }
                else
                {
                    messageCheck = message.IndexOf(errorInfo.Message, StringComparison.Ordinal) != -1;
                }
                
                var stacktraceCheck = true;
                if (errorInfo.IsExactStacktrace)
                {
                    if (errorInfo.Stacktrace != null)
                    {
                        stacktraceCheck = stacktrace.Equals(errorInfo.Stacktrace, StringComparison.Ordinal);   
                    }
                }
                else
                {
                    if (errorInfo.Stacktrace != null)
                    {
                        stacktraceCheck = stacktrace.IndexOf(errorInfo.Stacktrace, StringComparison.Ordinal) != -1;
                    }
                }

                if (messageCheck && stacktraceCheck)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsPermittedError(string message, string stacktrace)
        {
            var isPermittedPermanent = IsPermittedError(erroInfosPermanent, message, stacktrace);
            if (isPermittedPermanent)
            {
                return true;
            }
            var isPermitted = IsPermittedError(erroInfos, message, stacktrace);    
            return isPermitted;
        }
    }
}