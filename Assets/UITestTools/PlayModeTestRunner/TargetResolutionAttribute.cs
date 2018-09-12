using System;

namespace PlayQ.UITestTools
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TargetResolutionAttribute : Attribute
    {
        public readonly int Width;
        public readonly int Height;

        public TargetResolutionAttribute(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}