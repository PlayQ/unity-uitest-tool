using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tests.Nodes
{
    public class MethodNode : Node
    {
        [JsonProperty]
        public int PassedAmount { get; private set; }
        [JsonProperty]
        public int IgnoredAmount { get; private set; }
        [JsonProperty]
        public int FailedAmount { get; private set; }

        public void ResetTestState()
        {
            PassedAmount = 0;
            IgnoredAmount = 0;
            FailedAmount = 0;
            UpdateParentTestState();
        }

        public void SetTestState(TestState state)
        {
            switch (state)
            {
                case TestState.Passed:
                    PassedAmount++;
                    break;
                case TestState.Ignored:
                    IgnoredAmount++;
                    break;
                case TestState.Failed:
                    FailedAmount++;
                    break;
            }
            UpdateParentTestState();
        }

        public List<string> Logs = new List<string>();
        
        [JsonProperty]
        private int step;
        
        [JsonIgnore]
        public int Step
        {
            get { return step; }
        }

        public void SetStep(int step)
        {
            this.step = step;
        }

        [JsonProperty]
        private string methodFullName;
        [JsonProperty]
        private ClassNode parentClass;
        
        [JsonIgnore]
        public ClassNode ParentClass
        {
            get { return parentClass; }
        }
        
        [JsonProperty]
        private MethodTestSettings testSettings;
        
        [JsonIgnore]
        public MethodTestSettings TestSettings { get { return testSettings; } }
        
        [JsonIgnore]
        public override string FullName
        {
            get
            {
                return methodFullName;
            }
        }

        [JsonIgnore]
        public override TestState State
        {
           get
            {
                if (FailedAmount > 0)
                {
                    return TestState.Failed;
                }

                if (IgnoredAmount > 0)
                {
                    return TestState.Ignored;
                }

                if (PassedAmount > 0)
                {
                    return TestState.Passed;
                }

                return TestState.Undefined;
            }
        }

        [JsonConstructor]
        private MethodNode()
        {
        }
    
        public MethodNode(MethodTestSettings testSettings, ClassNode parentNode)
            : base(testSettings.MethodInfo.Name, parentNode)
        {
            this.testSettings = testSettings;
            parentClass = parentNode;
            methodFullName = parentClass.FullName + "." + testSettings.MethodInfo.Name;
        }

        public override bool Contains(string pattern)
        {
            return Name.IndexOf(pattern, StringComparison.Ordinal) > -1;
        }

        protected override void SetDataFromNode(Node other)
        {
            base.SetDataFromNode(other);
            if (other is MethodNode)
            {
                var otherMethodNode = (MethodNode) other;
                step = otherMethodNode.Step;
                PassedAmount = otherMethodNode.PassedAmount;
                IgnoredAmount = otherMethodNode.IgnoredAmount;
                FailedAmount = otherMethodNode.FailedAmount;
                Logs = otherMethodNode.Logs;
            }
        }
    }
}