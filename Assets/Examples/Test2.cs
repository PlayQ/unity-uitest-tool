using UnityEngine;

namespace PlayQ.UITestTools
{
    public class Test2 : ITestRunnerCallbackReceiver
    {
        public void OnBeforeTestsRunEvent()
        {
            Debug.LogWarning("Test2: OnBeforeTestsRunEvent()");
        }

        public void OnAfterTestsRunEvent()
        {
            Debug.LogWarning("Test2: OnAfterTestsRunEvent()");
        }
    }
}