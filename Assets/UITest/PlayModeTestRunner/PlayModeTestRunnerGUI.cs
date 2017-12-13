using System.Collections.Generic;
using UnityEditor;
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
            GUI.color = Color.white;
            GUILayout.BeginVertical(EditorStyles.helpBox);
            scrollState = GUILayout.BeginScrollView(scrollState);
            GUI.color = Color.black;
            if (currentTest != null)
            {
                GUILayout.Label(currentTest);
            }

            if (failedTests.Count > 0)
            {
                GUILayout.Label("Failed: ");
                foreach (var test in failedTests)
                {
                    GUILayout.Label(test);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUI.color = Color.white;
        }
    }
}