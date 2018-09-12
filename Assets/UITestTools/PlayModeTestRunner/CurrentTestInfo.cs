using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public static class CurrentTestInfo
    {
        public static Vector2[] TestMethodTargetResolution
        {
            get
            {
                MethodInfo testMethodInfo = null;

                if (PlayModeTestRunner.CurrentMethodInfo != null)
                {
                    testMethodInfo = PlayModeTestRunner.CurrentMethodInfo;
                }

                if (testMethodInfo == null)
                {
                    return new Vector2[] { };
                }
                
                var customAttributes = testMethodInfo.GetCustomAttributes(true);
                if (customAttributes.Any())
                {
                    var targetResolutions = customAttributes
                        .Where(attribute =>
                        {
                            return attribute.GetType() == typeof(TargetResolutionAttribute);
                        });
                    if (targetResolutions.Any())
                    {
                        return targetResolutions.Select(attribute =>
                        {
                            var targetResolution = (TargetResolutionAttribute) attribute;
                            return new Vector2(targetResolution.Width, targetResolution.Height);
                        }).ToArray();
                    }
                }
                
                return new Vector2[] { };
            }
        }
        
        
        public static string TestSuitName
        {
            get
            {
                if (PlayModeTestRunner.IsRunning)
                {
                    return PlayModeTestRunner.CurrentMethodInfo != null
                        ? PlayModeTestRunner.CurrentMethodInfo.DeclaringType.FullName
                        : null;
                }
                return null;
            }
        }
        
        public static string TestMethodName
        {
            get
            {
                if (PlayModeTestRunner.IsRunning)
                {
                    return PlayModeTestRunner.CurrentMethodInfo != null
                        ? PlayModeTestRunner.CurrentMethodInfo.Name
                        : null;
                }
                return null;
            }
        }
    }
}