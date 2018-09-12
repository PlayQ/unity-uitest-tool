using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlayQ.UITestTools;

namespace Tests
{
    public static class MethodTestSettingsFactory
    {
        private const float DEFAULT_TIME_OUT = 30000f;

        public static MethodTestSettings Build(MethodInfo methodInfo)
        {
            float timeOut = DEFAULT_TIME_OUT;
            bool isIgnored = false;
            bool isSmoke = false;
            string ignoreReason = string.Empty;
            List<CustomResolution> targetResolutions = new List<CustomResolution>();
            CustomResolution editorTargetResolution = null;

            var attributes = methodInfo.GetCustomAttributes(false);

            //Attributes are getting added in a reverse order that they're written by c#
            //i.e. if you have [A] above [B] above target method in your source code, 
            //this list will be in order B: 0, A: 1
            //If you have order-sensitive attributes then you need this list reversed against its initial state
            //Ordering is needed to set correct resolution from target resolution attribute in editor
            attributes.Reverse();

            foreach (var attribute in attributes)
            {
                var timeOutAttribute = attribute as TimeoutAttribute;
                if (timeOutAttribute != null)
                {
                    timeOut = (int)timeOutAttribute.Properties.Get("Timeout");
                    continue;
                }

                var ignoreAttribute = attribute as IgnoreAttribute;
                if (ignoreAttribute != null)
                {
                    isIgnored = true;
                    ignoreReason = ignoreAttribute.Reason;
                    continue;
                }

                var smokeTestAttribute = attribute as SmokeTestAttribute;
                if (smokeTestAttribute != null)
                {
                    isSmoke = true;
                    continue;
                }

                var targetResolutionAttribute = attribute as TargetResolutionAttribute;
                if (targetResolutionAttribute != null)
                {
                    targetResolutions.Add(new CustomResolution(targetResolutionAttribute.Width, targetResolutionAttribute.Height));

                    continue;
                }

                var editorTargetResolutionAttribute = attribute as EditorResolutionAttribute;
                if (editorTargetResolutionAttribute != null)
                {
                    editorTargetResolution = new CustomResolution(editorTargetResolutionAttribute.Width, editorTargetResolutionAttribute.Height);
                    continue;
                }
            }

            return new MethodTestSettings(methodInfo, timeOut, isIgnored, ignoreReason, isSmoke, targetResolutions, editorTargetResolution);
        }
    }
}