using System;

namespace PlayQ.UITestTools
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TestRailAttribute : Attribute
    {
        public string TestRailURL
        {
            get
            {
                return "https://" +
                       subdomen +
                       ".testrail.net/index.php?/cases/view/" +
                       testID; 
            }
        }
        private readonly string subdomen;
        private  readonly int testID;
        public TestRailAttribute(string subdomen, int testID)
        {
            this.subdomen = subdomen;
            this.testID = testID;
        }
    }
}