using UnityEngine;

namespace PlayQ.UITestTools
{
    public class Test1 : ITestRunnerCallbackReceiver
    {
        public void OnBeforeTestsRunEvent()
        {
            Debug.LogWarning("Test1: OnBeforeTestsRunEvent()");
        }

        public void OnAfterTestsRunEvent()
        {
            Debug.LogWarning("Test1: OnAfterTestsRunEvent()");
        }
    }
}