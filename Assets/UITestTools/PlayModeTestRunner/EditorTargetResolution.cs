using System;
using UnityEngine;

namespace PlayQ.UITestTools
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EditorResolutionAttribute : Attribute
    {
        public readonly int Width;
        public readonly int Height;

        public EditorResolutionAttribute(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}