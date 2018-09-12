using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public readonly string IgnoreReason;
        public readonly List<CustomResolution> TargetResolutions = new List<CustomResolution>();
        public readonly CustomResolution EditorTargetResolution;

        public bool IsSmoke
        {
            get
            {
                var attributes = Method.GetCustomAttributes(false);
                return attributes.Any(attr => attr.GetType() == typeof(SmokeTestAttribute));
            }
        }

        public UnitTestMethod(MethodInfo methodInfo, bool sync)
        {
            Method = methodInfo;
            Sync = sync;
            FullName = methodInfo.DeclaringType.FullName + "." + methodInfo.Name;


            var attrs = methodInfo.GetCustomAttributes(false);

            //Attributes are getting added in a reverse order that they're written by c#
            //i.e. if you have [A] above [B] above target method in your source code, 
            //this list will be in order B: 0, A: 1
            //If you have order-sensitive attributes then you need this list reversed against its initial state
            //Ordering is needed to set correct resolution from target resolution attribute in editor
            attrs.Reverse();

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
                    IgnoreReason = ignore.Reason; 
                    continue;
                }
                var targetResolution = attr as TargetResolutionAttribute;
                if (targetResolution != null)
                {
                    TargetResolutions.Add(new CustomResolution(targetResolution.Width, targetResolution.Height));

                    continue;
                }
                var editorTargetResolution = attr as EditorResolutionAttribute;
                if (editorTargetResolution != null)
                {
                    EditorTargetResolution = new CustomResolution(editorTargetResolution.Width, editorTargetResolution.Height);
                    continue;
                }
            }
        }

        public bool Filter(string s)
        {
            if (s == null)
            {
                s = string.Empty;
            }
            if (!String.IsNullOrEmpty(FullName))
            {
                return FullName.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return false;
        }

        public bool ContainsTargetResolution(int width, int height)
        {
            if (TargetResolutions.Count == 0)
            {
                return true;
            }
            return TargetResolutions.FirstOrDefault(item => item.Width == width && item.Height == height) != null;
        }
    }
}