using System.Linq;

namespace PlayQ.UITestTools
{
    public class ConsoleArgumentHelper
    {
        public static string GetArg(string name)
        {
            var args = System.Environment.GetCommandLineArgs();
            if (args == null)
            {
                return null;
            }
            
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
            return null;
        }
		
        public static bool IsArgExist(string name)
        {
            var args = System.Environment.GetCommandLineArgs();
            if (args == null)
            {
                return false;
            }
            return args.Any(arg => arg == name);
        }
    }
}