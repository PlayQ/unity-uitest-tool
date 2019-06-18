namespace PlayQ.UITestTools
{
    public static class UITestToolsVersionInfo
    {
        public const int MAJOR = 1;
        public const int MINOR = 0;
        public const int PATCH = 1;

        public static string StringVersion
        {
            get { return "v." + MAJOR + "." + MINOR + "." + PATCH; }
        }
    }
}