using System;
using System.Reflection;
using Tests;

namespace Tests.Nodes
{
    public class MethodNode : Node
    {
        private MethodTestSettings testSettings;
        public MethodTestSettings TestSettings { get { return testSettings; } }

        //public MethodTestResult TestResult;

        public MethodNode(MethodTestSettings testSettings, Node parentNode = null) : base(testSettings.Method.Name, parentNode)
        {
            this.testSettings = testSettings;
            MethodInfo methodInfo = testSettings.Method;
            //FullName = methodInfo.DeclaringType.FullName + "." + methodInfo.Name;
        }

        public override bool Contains(string pattern)
        {
            return /*FullName*/Name.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) > -1;
        }
    }
}