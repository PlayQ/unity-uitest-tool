using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public class CustomResolution
    {
        public CustomResolution(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public readonly int Width;
        public readonly int Height;
    }
    
    public class UnitTestMethod
    {
        private const float DEFAULT_TIME_OUT = 30000f;

        public readonly string FullName;
        public readonly bool Sync;
        public readonly MethodInfo Method;
        public readonly float TimeOut = DEFAULT_TIME_OUT;
        public readonly bool IsIgnored;
        public readonly CustomResolution TargetResolution;

        public UnitTestMethod(MethodInfo methodInfo, bool sync)
        {
            Method = methodInfo;
            Sync = sync;
            FullName = methodInfo.DeclaringType.FullName + "." + methodInfo.Name;

            var attrs = methodInfo.GetCustomAttributes(false);
            foreach (var attr in attrs)
            {
                var timeOut = attr as TimeoutAttribute;
                if (timeOut != null)
                {
                    TimeOut = (int) timeOut.Properties.Get("Timeout");
                    continue;
                }
                var ignore = attr as IgnoreAttribute;
                if (ignore != null)
                {
                    IsIgnored = true;
                    continue;
                }
                var targetResolution = attr as TargetResolutionAttribute;
                if (targetResolution != null)
                {
                    TargetResolution = new CustomResolution(targetResolution.Width, targetResolution.Height);
                    continue;
                }
            }
        }
    }
}