using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PlayQ.UITestTools
{
    public static class PermittedErrors
    {
        private static readonly List<PermittedErrorInfo> errorsInfos = new List<PermittedErrorInfo>();

        public static PermittedErrorInfo Add(string message, string stacktrace = "", bool regExp = false)
        {
            PermittedErrorInfo errorInfo = new PermittedErrorInfo(message, regExp, stacktrace);
            errorsInfos.Add(errorInfo);
            return errorInfo;
        }

        public static void ClearAll()
        {
            errorsInfos.Clear();
        }
        
        public static void Remove(PermittedErrorInfo error)
        {
            errorsInfos.Remove(error);
        }
        
        private static bool IsPermittedError(List<PermittedErrorInfo> errorInfos, string message, string stacktrace)
        {
            foreach (var errorInfo in errorInfos)
            {
                var messageCheck = false;
                if (errorInfo.IsRegExpMessage)
                {
                    var regexp = new Regex(errorInfo.Message);
                    messageCheck = regexp.IsMatch(message);
                }
                else
                {
                    messageCheck = message.IndexOf(errorInfo.Message, StringComparison.Ordinal) != -1;
                }

                var stacktraceCheck = true;
                if (string.IsNullOrEmpty(errorInfo.Stacktrace))
                {
                    stacktraceCheck = stacktrace.IndexOf(errorInfo.Stacktrace, StringComparison.Ordinal) != -1;
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
            return IsPermittedError(errorsInfos, message, stacktrace);
        }
    }
}