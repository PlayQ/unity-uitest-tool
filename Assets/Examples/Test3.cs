using UnityEngine;

namespace PlayQ.UITestTools
{
    public class Test3 : ITestRunnerCallbackReceiver
    {
        public void OnBeforeTestsRunEvent()
        {
            Debug.LogWarning("Test3: OnBeforeTestsRunEvent()");
        }

        public void OnAfterTestsRunEvent()
        {
            Debug.LogWarning("Test3: OnAfterTestsRunEvent()");
        }
    }
}