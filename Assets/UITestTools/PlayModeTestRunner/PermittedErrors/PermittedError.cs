namespace PlayQ.UITestTools
{
    public class PermittedErrorInfo
    {
        public readonly string Message;
        public readonly bool IsRegExpMessage;
        public readonly string Stacktrace;

        public PermittedErrorInfo(string message, bool isRegExpMessage, string stacktrace)
        {
            Message = message;
            IsRegExpMessage = isRegExpMessage;
            Stacktrace = stacktrace;
        }

        public void Remove()
        {
            PermittedErrors.Remove(this);
        }
    }
}