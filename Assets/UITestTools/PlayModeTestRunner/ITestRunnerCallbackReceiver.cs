namespace PlayQ.UITestTools
{
    public interface ITestRunnerCallbackReceiver
    {
        void OnBeforeTestsRunEvent();
        void OnAfterTestsRunEvent();
	}
}