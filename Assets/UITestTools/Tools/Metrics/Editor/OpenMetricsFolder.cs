using System.IO;
using UnityEditor;

namespace PlayQ.UITestTools.Editor
{
    public class OpenMetricsFolder
    {
        [MenuItem("Window/UI Test Tools/Open Tests Metrics")]
        static void OpenTestsMetrics()
        {
            if (Directory.Exists(TestMetrics.MetricsFolder))
            {
                EditorUtility.RevealInFinder(TestMetrics.MetricsFolder);                
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Metrics folder doesn't exist yet.", "Ok");
            }
        }
    }
}