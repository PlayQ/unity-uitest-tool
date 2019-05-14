using System.IO;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public static class TestMetrics
    {
        public static FPSCounter FPSCounter = new FPSCounter();
        public static PreloaderTimeTracker PreloaderTimeTracker = new PreloaderTimeTracker();

        public static void CheckAndCreateFolder()
        {
            if (!Directory.Exists(MetricsFolder))
            {
                Directory.CreateDirectory(MetricsFolder);
            }
        }
        
        public static string PathTo(string file)
        {
            return MetricsFolder + Path.DirectorySeparatorChar + file;
        }
        
        public static string MetricsFolder
        {
            get
            {
                return Application.persistentDataPath +
                       Path.DirectorySeparatorChar + "Metrics";
            }
        }
    }
}