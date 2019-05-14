using System;
using System.Collections.Generic;

namespace PlayQ.UITestTools
{
    public static class PermittedErrors
    {
        private static readonly List<string> permanent = new List<string>()
        {
            "RemoveEmptyFolders.cs", //AssetDatabase.DeleteAsset sometimes can log error (Unity bug)
            //"[TG] PersistenceManager:: Fail parsing string",
            //"[FB] Failed to link:\nPlayQ.TG.Core.Exceptions.UnhandledHttpException: Offline mode",
            "Exception when filling SpriteSharpDatabase with JSON data",
            "ArgumentException: An element with the same key already exists in the dictionary.",
            
            //"Can't get translation for term: COMMON_LEVEL_ID",
            //"_locDefaultText is null or empty. can't update region level button ui with correct localization",
            
            //"[RegionManager] Could not show region for ID: 1"
            "flow", "FLOW", "Flow",
            "_activeBubble not set"

        };

        struct ConditionToStacktrace
        {
            public string Condition;
            public string Stacktrace;
        }
        private static readonly List<ConditionToStacktrace> permanentExceptions = new List<ConditionToStacktrace>
        {
            new ConditionToStacktrace 
            {
                Condition = "The object of type 'GameObject' has been destroyed but you are still trying to access it.",
                Stacktrace = "BackgroundDarkener.SetBlockerAlpha (UnityEngine.GameObject obj, Single a) (at Assets/CharmKing/Scripts/UI/BackgroundDarkener.cs:502)"
            },
        };
        
        private static readonly List<ConditionToStacktrace> variableException = new List<ConditionToStacktrace>();

        private static readonly List<string> variable = new List<string>();

        public static void AddPermittedException(string condition, string stacktrace = "")
        {
            variableException.Add(new ConditionToStacktrace()
            {
                Condition = condition,
                Stacktrace = stacktrace
            });
        }
        
        public static void AddPermittedError(string errorText)
        {
            variable.Add(errorText);
        }

        public static void Clear()
        {
            variable.Clear();
            variableException.Clear();
        }

        public static bool IsPermittedException(string condition, string stacktrace)
        {
            foreach (var conditionToStack in permanentExceptions)
            {
                if (condition.Equals(conditionToStack.Condition, StringComparison.Ordinal) &&
                    stacktrace.Contains(conditionToStack.Stacktrace))
                {
                    return true;
                }
            }
            
            foreach (var conditionToStack in variableException)
            { 
                if (condition.Equals(conditionToStack.Condition, StringComparison.Ordinal) &&
                    stacktrace.Contains(conditionToStack.Stacktrace))
                {
                    return true;
                }
            }
            

            return false;
        }
        
        public static bool IsPermitted(string condition)
        {
            foreach (var error in permanent)
            {
                if (condition.Contains(error))
                {
                    return true;
                }
            }
            foreach (var error in variable)
            {
                if (condition.Contains(error))
                {
                    return true;
                }
            }
            return false;
        }
    }
}