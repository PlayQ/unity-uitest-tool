namespace PlayQ.UITestTools
{
    public static class UITestToolsVersionInfo
    {
        public const int MAJOR = 1;
        public const int MINOR = 1;
        public const int PATCH = 0;

        public static string StringVersion
        {
            get { return "v." + MAJOR + "." + MINOR + "." + PATCH; }
        }
    }
}