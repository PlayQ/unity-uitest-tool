public static class UITestToolsVersionInfo
{
    public const int MAJOR = 0;
    public const int MINOR = 2;
    public const int PATCH = 0;

    public static string StringVersion
    {
        get { return "v." + MAJOR + "." + MINOR + "." + PATCH; }
    }
}