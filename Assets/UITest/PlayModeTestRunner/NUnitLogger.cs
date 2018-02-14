using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public class NUnitLogger : ITestLogger
    {
        private const string TC_DATE_FORMAT = "yyyy-MM-ddThh:mm:ss.fff";

        public void LogTestStart()
        {
            Debug.Log(string.Format("##teamcity[testStarted timestamp='{0}' name='{1}']",
                DateTime.UtcNow.ToString(TC_DATE_FORMAT), TestContext.CurrentContext.Test.FullName));
        }

        public void LogTestEnd()
        {
            if (TestContext.CurrentContext.Result.Outcome == ResultState.Ignored)
            {
                Debug.Log(string.Format("##teamcity[testIgnored timestamp='{0}' name='{1}']",
                    DateTime.UtcNow.ToString(TC_DATE_FORMAT),
                    TestContext.CurrentContext.Test.FullName));
                return;
            }
            if (TestContext.CurrentContext.Result.Outcome == ResultState.Success)
            {
                Debug.Log(string.Format("##teamcity[testFinished timestamp='{0}' name='{1}']",
                    DateTime.UtcNow.ToString(TC_DATE_FORMAT),
                    TestContext.CurrentContext.Test.FullName));
                return;
            }
            Debug.Log(string.Format("##teamcity[testFailed timestamp='{0}' name='{1}']",
                DateTime.UtcNow.ToString(TC_DATE_FORMAT),
                TestContext.CurrentContext.Test.FullName));
        }
    }
}