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
        
        private static readonly List<ErrorInfo> erroInfos = new List<ErrorInfo>()
        {
            new ErrorInfo{Message = "RemoveEmptyFolders.cs"}, //AssetDatabase.DeleteAsset sometimes can log error (Unity bug)
            new ErrorInfo{Message = "Exception when filling SpriteSharpDatabase with JSON data"},
            new ErrorInfo{Message = "ArgumentException: An element with the same key already exists in the dictionary."},
            new ErrorInfo{Message = "_activeBubble not set"},
            new ErrorInfo{Message = "[ControlAssistant] _uiBlockingRequests is negative, which should never happen. Fix this! Clamping to zero for safety..."},
            new ErrorInfo{Message = "PurchaseSuccessQueueHandler == null"}, // caused by StoreIAP.OnPurchaseSuccessOrFail() it has TODO by CK teram. Any success or failed purchase
        };

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

        public static bool IsPermittedError(string message, string stacktrace)
        {
            foreach (var errorInfo in erroInfos)
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
                
                var stacktraceCheck = false;
                if (errorInfo.IsExactStacktrace)
                {
                    stacktraceCheck = stacktrace.Equals(errorInfo.Stacktrace, StringComparison.Ordinal);
                }
                else
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
    }
}