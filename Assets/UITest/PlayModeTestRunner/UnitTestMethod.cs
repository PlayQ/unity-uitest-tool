using System.Reflection;
using NUnit.Framework;

namespace PlayQ.UITestTools
{
    public class UnitTestMethod
    {
        public readonly string FullName;
        public readonly bool Sync;
        public readonly MethodInfo Method;
        public readonly float TimeOut = PlayModeTestRunner.DEFAULT_TIME_OUT;
        public readonly bool IsIgnored;

        public UnitTestMethod(MethodInfo methodInfo, bool sync)
        {
            Method = methodInfo;
            Sync = sync;
            FullName = methodInfo.DeclaringType.Namespace + methodInfo.Name;

            var attrs = methodInfo.GetCustomAttributes(false);
            foreach (var attr in attrs)
            {
                var timeOut = attr as TimeoutAttribute;
                if (timeOut != null)
                {
                    TimeOut = (int) timeOut.Properties.Get("Timeout");
                }
                var ignore = attr as IgnoreAttribute;
                if (ignore != null)
                {
                    IsIgnored = true;
                }
            }

        }
    }
}