using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PlayQ.UITestTools
{
    public class RuntimeTestToolWindow : EditorWindow
    {
        public enum TestState
        {
            Undefined,
            Passed,
            Skipped,
            Failed
        }

        [MenuItem("Window/Runtime Test Runner")]
        private static void ShowWindow()
        {
            GetWindow(typeof(RuntimeTestToolWindow), false, "Test Runner").Show();
        }

        private List<UnitTestClass> testClasses;

        private Dictionary<MethodInfo, TestState> testPassInfo = new Dictionary<MethodInfo, TestState>();
        private Dictionary<Type, bool> openedClasses = new Dictionary<Type, bool>();

        private GUIStyle offsetStyle;
        private GUIStyle passedStyle;
        private GUIStyle failedStyle;
        private GUIStyle unknownStyle;
        private GUIStyle skippedStyle;
        private Vector2 scrollPosition;

        private GUIStyle defaultFoldoutStyle
        {
            get
            {
                return new GUIStyle(EditorStyles.foldout)
                {
                    normal = {textColor = Color.black},
                    active = {textColor = Color.black},
                    hover = {textColor = Color.black},
                    onActive = {textColor = Color.black},
                    onFocused = {textColor = Color.black},
                    onHover = {textColor = Color.black},
                    onNormal = {textColor = Color.black},
                    focused = {textColor = Color.black}
                };
            }
        }

        private GUIStyle passedFoldoutStyle
        {
            get
            {
                return new GUIStyle(EditorStyles.foldout)
                {
                    normal = {textColor = new Color(0f, 0.7f, 0f)},
                    active = {textColor = new Color(0f, 0.7f, 0f)},
                    hover = {textColor = new Color(0f, 0.7f, 0f)},
                    onActive = {textColor = new Color(0f, 0.7f, 0f)},
                    onFocused = {textColor = new Color(0f, 0.7f, 0f)},
                    onHover = {textColor = new Color(0f, 0.7f, 0f)},
                    onNormal = {textColor = new Color(0f, 0.7f, 0f)},
                    focused = {textColor = new Color(0f, 0.7f, 0f)}
                };
            }
        }

        private GUIStyle failedFoldoutStyle
        {
            get
            {
                return new GUIStyle(EditorStyles.foldout)
                {
                    normal = {textColor = new Color(1f, 0f, 0f)},
                    active = {textColor = new Color(1f, 0f, 0f)},
                    hover = {textColor = new Color(1f, 0f, 0f)},
                    onActive = {textColor = new Color(1f, 0f, 0f)},
                    onFocused = {textColor = new Color(1f, 0f, 0f)},
                    onHover = {textColor = new Color(1f, 0f, 0f)},
                    onNormal = {textColor = new Color(1f, 0f, 0f)},
                    focused = {textColor = new Color(1f, 0f, 0f)}
                };
            }
        }


        private void OnEnable()
        {
            offsetStyle = new GUIStyle {margin = {left = 20}};
            passedStyle = new GUIStyle {normal = {textColor = new Color(0f, 0.7f, 0f)}};
            failedStyle = new GUIStyle {normal = {textColor = new Color(1f, 0f, 0f)}};
            skippedStyle = new GUIStyle {normal = {textColor = new Color(0.5f, 0.5f, 0.5f)}};
            unknownStyle = new GUIStyle {normal = {textColor = Color.black}};

            testClasses = PlayModeTestRunner.GetTestClasses();

            PlayModeTestRunner.OnStartProcessingTests += OnStartProcessingTests;
            PlayModeTestRunner.OnTestFailed += OnTestFailed;
            PlayModeTestRunner.OnTestPassed += OnTestPassed;
            PlayModeTestRunner.OnTestSkipped += OnTestSkipped;
        }

        private void OnDisable()
        {
            PlayModeTestRunner.OnStartProcessingTests -= OnStartProcessingTests;
            PlayModeTestRunner.OnTestFailed -= OnTestFailed;
            PlayModeTestRunner.OnTestPassed -= OnTestPassed;
            PlayModeTestRunner.OnTestSkipped -= OnTestSkipped;
        }

        private void OnTestSkipped(MethodInfo methodInfo)
        {
            testPassInfo[methodInfo] = TestState.Skipped;
            Repaint();
        }

        private void OnTestPassed(MethodInfo methodInfo)
        {
            testPassInfo[methodInfo] = TestState.Passed;
            Repaint();
        }

        private void OnTestFailed(MethodInfo methodInfo)
        {
            testPassInfo[methodInfo] = TestState.Failed;
            Repaint();
        }

        private void OnStartProcessingTests()
        {
            testPassInfo.Clear();
            Repaint();
        }

        private void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            DrawHeader();
            DrawClasses();
            GUILayout.EndScrollView();
        }

        private void DrawClasses()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            foreach (var testClass in testClasses)
            {
                DrawClass(testClass);
            }

            GUILayout.EndVertical();
        }

        private void DrawClass(UnitTestClass testClass)
        {
            var isOpened = openedClasses.ContainsKey(testClass.Type) && openedClasses[testClass.Type] == true;
            var style = GetClassState(testClass);
            isOpened = EditorGUILayout.Foldout(isOpened, testClass.Type.FullName, true, style);
            openedClasses[testClass.Type] = isOpened;

            if (isOpened)
            {
                GUILayout.BeginVertical(offsetStyle);
                foreach (var mthd in testClass.TestMethods)
                {
                    DrawMethod(mthd.Method, testClass.Type);
                }
                GUILayout.EndVertical();
            }
        }

        private void DrawMethod(MethodInfo method, Type type)
        {
            var style = testPassInfo.ContainsKey(method) ? GetMethodStyle(testPassInfo[method]) : unknownStyle;
            EditorGUILayout.LabelField(method.Name, style);
        }

        private GUIStyle GetClassState(UnitTestClass testClass)
        {
            var passedTests = 0;
            foreach (var testMethod in testClass.TestMethods)
            {
                TestState state;
                if (testPassInfo.TryGetValue(testMethod.Method, out state))
                {
                    switch (state)
                    {
                        case TestState.Passed:
                        case TestState.Skipped:
                            passedTests++;
                            break;
                        case TestState.Failed:
                            return failedFoldoutStyle;
                    }
                }
            }
            return passedTests == testClass.TestMethods.Count ? passedFoldoutStyle : defaultFoldoutStyle;
        }

        private GUIStyle GetMethodStyle(TestState state)
        {
            switch (state)
            {
                case TestState.Undefined:
                    return unknownStyle;
                case TestState.Passed:
                    return passedStyle;
                case TestState.Skipped:
                    return skippedStyle;
                case TestState.Failed:
                    return failedStyle;
                default:
                    throw new ArgumentOutOfRangeException("state", state, null);
            }
        }

        private static void DrawHeader()
        {
            GUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Playmode tests: ", EditorStyles.boldLabel);
            if (GUILayout.Button("Run all", EditorStyles.miniButton))
            {
                var scenePath = PlayModeTestRunner.GetTestScenePath();
                if (scenePath == null)
                {
                    Debug.LogError("Cant find test scene");
                    return;
                }
                EditorSceneManager.OpenScene(scenePath);
                EditorApplication.isPlaying = true;
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}