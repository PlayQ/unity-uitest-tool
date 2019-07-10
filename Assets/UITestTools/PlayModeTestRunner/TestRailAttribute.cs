using System;
using NUnit.Framework.Constraints;

namespace PlayQ.UITestTools
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TestRailAttribute : Attribute
    {
        public string TestRailURL
        {
            get
            {
                return "https://" +
                       subdomen +
                       ".testrail.net/index.php?/cases/view/" +
                       testId; 
            }
        }

        private readonly string subdomen;
        private readonly int testId;
        public readonly string Description;
        
        public TestRailAttribute(string subdomen, int testId, string description)
        {
            this.subdomen = subdomen;
            this.testId = testId;
            this.Description = description;
        }
        
        public TestRailAttribute(string subdomen, int testId)
        {
            this.subdomen = subdomen;
            this.testId = testId;
        }
    }
}