using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public class RuntimeTestToolWindow : EditorWindow
    {
        public enum TestState
        {
            Undefined,
            Passed,
            Ignored,
            Failed
        }

        [Serializable]
        private class OpenClassInfo
        {
            public string ClassName;
            public bool State;
        }

        [MenuItem("Window/Runtime Test Runner")]
        private static void ShowWindow()
        {
            GetWindow(typeof(RuntimeTestToolWindow), false, "Test Runner").Show();
        }

        private List<UnitTestClass> testClasses;

        private Dictionary<MethodInfo, TestState> testInfo = new Dictionary<MethodInfo, TestState>();
        private Dictionary<string, bool> openedClasses = new Dictionary<string, bool>();
        private List<OpenClassInfo> openedClassesList = new List<OpenClassInfo>();

        private Texture2D successIcon;
        private Texture2D failIcon;
        private Texture2D ignoreIcon;
        private Texture2D defaultIcon;

        private GUIStyle offsetStyle;
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

        private void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
            testClasses = PlayModeTestRunner.GetTestClasses();

            RestoreOpenedClasses();
            CreateStyleClasses();
            SubscribeToEvents();
            LoadImgs();
        }

        private void RestoreOpenedClasses()
        {
            foreach (var value in openedClassesList)
            {
                openedClasses.Add(value.ClassName, value.State);
            }
            openedClassesList.Clear();
        }

        // for storing opened classes during entering to play mode
        private void StoreOpenedClasses()
        {
            foreach (var keyValue in openedClasses)
            {
                openedClassesList.Add(new OpenClassInfo {ClassName = keyValue.Key, State = keyValue.Value});
            }
        }

        private void CreateStyleClasses()
        {
            offsetStyle = new GUIStyle {margin = {left = 20}};
        }

        private void SubscribeToEvents()
        {
            PlayModeTestRunner.OnStartProcessingTests += OnStartProcessingTests;
            PlayModeTestRunner.OnTestFailed += OnTestFailed;
            PlayModeTestRunner.OnTestPassed += OnTestPassed;
            PlayModeTestRunner.OnTestIgnored += OnTestIgnored;
        }

        private void LoadImgs()
        {
            successIcon = Resources.Load<Texture2D>("passed");
            failIcon = Resources.Load<Texture2D>("failed");
            ignoreIcon = Resources.Load<Texture2D>("ignored");
            defaultIcon = Resources.Load<Texture2D>("normal");
        }

        private void OnDisable()
        {
            StoreOpenedClasses();

            PlayModeTestRunner.OnStartProcessingTests -= OnStartProcessingTests;
            PlayModeTestRunner.OnTestFailed -= OnTestFailed;
            PlayModeTestRunner.OnTestPassed -= OnTestPassed;
            PlayModeTestRunner.OnTestIgnored -= OnTestIgnored;
        }

        private void OnTestIgnored(MethodInfo methodInfo)
        {
            testInfo[methodInfo] = TestState.Ignored;
            Repaint();
        }

        private void OnTestPassed(MethodInfo methodInfo)
        {
            testInfo[methodInfo] = TestState.Passed;
            Repaint();
        }

        private void OnTestFailed(MethodInfo methodInfo)
        {
            testInfo[methodInfo] = TestState.Failed;
            Repaint();
        }

        private void OnStartProcessingTests()
        {
            testInfo.Clear();
            Repaint();
        }

        private void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            DrawHeader();
            DrawStatistics();
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
            var isOpened = openedClasses.ContainsKey(testClass.Type.FullName) &&
                           openedClasses[testClass.Type.FullName] == true;
            var content = new GUIContent(testClass.Type.FullName, GetClassIcon(testClass));
            isOpened = EditorGUILayout.Foldout(isOpened, content, true, defaultFoldoutStyle);
            openedClasses[testClass.Type.FullName] = isOpened;

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
            var content = testInfo.ContainsKey(method)
                ? new GUIContent(method.Name, GetMethodIcon(testInfo[method]))
                : new GUIContent(method.Name, defaultIcon);
            EditorGUILayout.LabelField(content);
        }

        private Texture2D GetClassIcon(UnitTestClass testClass)
        {
            var passedTests = 0;
            var ignoredTests = 0;
            foreach (var testMethod in testClass.TestMethods)
            {
                TestState state;
                if (testInfo.TryGetValue(testMethod.Method, out state))
                {
                    switch (state)
                    {
                        case TestState.Passed:
                            passedTests++;
                            break;
                        case TestState.Ignored:
                            ignoredTests++;
                            break;
                        case TestState.Failed:
                            return failIcon;
                    }
                }
            }
            if (passedTests == testClass.TestMethods.Count)
            {
                return successIcon;
            }
            return ignoredTests > 0 ? ignoreIcon : defaultIcon;
        }


        private Texture2D GetMethodIcon(TestState state)
        {
            switch (state)
            {
                case TestState.Undefined:
                    return defaultIcon;
                case TestState.Passed:
                    return successIcon;
                case TestState.Ignored:
                    return ignoreIcon;
                case TestState.Failed:
                    return failIcon;
                default:
                    throw new ArgumentOutOfRangeException("state", state, null);
            }
        }

        private void DrawStatistics()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Passed", successIcon), GUILayout.Width(300f));
            GUILayout.Label(testInfo.Values.Count(t => t == TestState.Passed).ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Failed", failIcon), GUILayout.Width(300f));
            GUILayout.Label(testInfo.Values.Count(t => t == TestState.Failed).ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Ignored", ignoreIcon), GUILayout.Width(300f));
            GUILayout.Label(testInfo.Values.Count(t => t == TestState.Ignored).ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Total", defaultIcon), GUILayout.Width(300f));
            GUILayout.Label(testClasses.SelectMany(c => c.TestMethods).Count().ToString());
            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawHeader()
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