using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tests
{
    public class MethodTestSettings
    {
        public readonly MethodInfo Method;
        public readonly float TimeOut;
        public readonly bool IsIgnored;
        public readonly string IgnoreReason;
        private List<CustomResolution> targetResolutions;
        public readonly CustomResolution EditorTargetResolution;
        public readonly bool IsSmoke;

        public MethodTestSettings(MethodInfo methodInfo,
                                  float timeOut,
                                  bool isIgnored,
                                  string ignoreReason,
                                  bool isSmoke,
                                  List<CustomResolution> targetResolutions,
                                  CustomResolution editorTargetResolution)
        {
            Method = methodInfo;
            TimeOut = timeOut;
            IsIgnored = isIgnored;
            IgnoreReason = ignoreReason;
            this.targetResolutions = targetResolutions;
            IsSmoke = isSmoke;
        }

        public bool ContainsTargetResolution(int width, int height)
        {
            if (targetResolutions.Count == 0)
            {
                return true;
            }

            return targetResolutions.FirstOrDefault(item => item.Width == width && item.Height == height) != null;
        }

        public CustomResolution DefaultTargetResolution
        {
            get
            {
                return targetResolutions[0];
            }
        }
    }
}