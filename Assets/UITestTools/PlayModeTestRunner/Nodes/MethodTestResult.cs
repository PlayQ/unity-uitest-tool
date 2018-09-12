using System;
using System.Collections.Generic;

namespace Tests
{
    [Serializable]
    public class MethodTestResult
    {
        public TestState State;
        public List<string> Logs;
    }
}