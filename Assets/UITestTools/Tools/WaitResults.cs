namespace PlayQ.UITestTools.WaitResults
{
    public abstract class WaitResult
    {}

    public class WaitSuccess : WaitResult
    {
        public override string ToString()
        {
            return "Wait success";
        }
    }
        
    public class WaitFailed : WaitResult
    {
        private readonly string message;
        public WaitFailed(string message)
        {
            this.message = message;
        }

        public override string ToString()
        {
            return "Wait failed, reason: " + message;
        }
    }

}