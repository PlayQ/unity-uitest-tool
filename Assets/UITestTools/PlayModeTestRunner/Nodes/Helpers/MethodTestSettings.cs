using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Tests
{
    public class MethodTestSettings
    {
        [JsonProperty] private string assemblySerialized;
        [JsonProperty] private string typeSerialized;
        [JsonProperty] private string methodName;
        
        private Type ownerType;
        private MethodInfo methodInfo;
        
        [JsonIgnore]
        public MethodInfo MethodInfo
        {
            get
            {
                if (methodInfo == null)
                {
                    if (ownerType == null)
                    {
                        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        var assembly = assemblies.FirstOrDefault(a => a.FullName == assemblySerialized);
                        if (assembly != null)
                        {
                            ownerType = assembly.GetType(typeSerialized);
                        }
                    }

                    if (ownerType != null)
                    {
                        methodInfo = ownerType.GetMethod(methodName);
                    }
                }

                return methodInfo;
            }
            set
            {
                ownerType = value.DeclaringType;
                typeSerialized = ownerType.FullName;
                assemblySerialized = ownerType.Assembly.FullName;
                methodName = value.Name;
                methodInfo = value;
            }
        }
        
        public readonly float TimeOut = 400000;
        public readonly bool IsIgnored;
        public readonly string IgnoreReason;
        [JsonProperty]
        private List<CustomResolution> targetResolutions;
        public readonly CustomResolution EditorTargetResolution;
        public readonly bool IsSmoke;
        public readonly string TestRailURL;

        
        [JsonConstructor]
        public MethodTestSettings(
            float timeOut,
            bool isIgnored,
            string ignoreReason,
            bool isSmoke,
            string testRailUrl,
            List<CustomResolution> targetResolutions,
            CustomResolution editorTargetResolution)
        {
            TestRailURL = testRailUrl;
            TimeOut = timeOut;
            IsIgnored = isIgnored;
            IgnoreReason = ignoreReason;
            this.targetResolutions = targetResolutions;
            IsSmoke = isSmoke;
        }
        
        public MethodTestSettings(MethodInfo methodInfo,
                                  float timeOut,
                                  bool isIgnored,
                                  string ignoreReason,
                                  bool isSmoke,
                                  string testRailUrl,
                                  List<CustomResolution> targetResolutions,
                                  CustomResolution editorTargetResolution)
        {
            TestRailURL = testRailUrl;
            MethodInfo = methodInfo;
            TimeOut = timeOut;
            IsIgnored = isIgnored;
            IgnoreReason = ignoreReason;
            this.targetResolutions = targetResolutions;
            IsSmoke = isSmoke;
        }

        public bool ContainsTargetResolution(int width, int height)
        {
            if (targetResolutions == null || targetResolutions.Count == 0)
            {
                return true;
            }

            return targetResolutions.FirstOrDefault(item => item.Width == width && item.Height == height) != null;
        }

        [JsonIgnore]
        public CustomResolution DefaultTargetResolution
        {
            get
            {
                if (targetResolutions == null || targetResolutions.Count == 0)
                {
                    return new CustomResolution(800, 600); 
                }
                return targetResolutions[0];
            }
        }
    }
}