using System.Collections.Generic;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public class PlayModeTestRunnerGUI
    {
        private string currentTest;

        private List<string> failedTests = new List<string>();
        private Vector2 scrollState;

        public void SetCurrentTest(string text)
        {
            currentTest = "Current: \"" + text + "\"";
        }

        public void AddFailedTest(string failedTest)
        {
            failedTests.Add(failedTest);
        }

        public void Draw()
        {
            GUI.color = Color.black;
            GUILayout.BeginVertical(GUI.skin.box);
            scrollState = GUILayout.BeginScrollView(scrollState);
           
            if (currentTest != null)
            {
                GUI.color = Color.white;
                GUILayout.Label(currentTest);
                GUI.color = Color.black;
            }

            if (failedTests.Count > 0)
            {
                GUI.color = Color.white;
                GUILayout.Label("Failed: ");
                foreach (var test in failedTests)
                {
                    GUILayout.Label(test);
                }
                GUI.color = Color.black;
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUI.color = Color.white;
        }
    }
}