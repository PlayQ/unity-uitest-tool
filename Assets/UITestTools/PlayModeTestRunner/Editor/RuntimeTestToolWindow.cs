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

        private SelectedTestsSerializable selectedTests;
        [Serializable]
        private class OpenClassInfo
        {
            public string ClassName;
            public bool State;
        }
        
        [Serializable]
        private class SelectedClassInfo
        {
            public SelectedClassInfo(string className,  bool isClassSelected, bool isAnyMethodSelected)
            {
                ClassName = className;
                this.isAnyMethodSelected = isAnyMethodSelected;
                this.isClassSelected = isClassSelected;

                previousIsAnyMethodSelected = this.isAnyMethodSelected;
                previousIsClassSelected = this.isClassSelected;
            }
            
            private bool previousIsClassSelected;
            private bool previousIsAnyMethodSelected;
            public string ClassName;
            

            private bool isClassSelected;
            public bool IsClassSelected
            {
                set
                {
                    previousIsClassSelected = isClassSelected;
                    isClassSelected = value;
                }
                get { return isClassSelected; }
            }
            
            private bool isAnyMethodSelected;
            public bool IsAnyMethodSelected
            {
                set
                {
                    previousIsAnyMethodSelected = isAnyMethodSelected;
                    isAnyMethodSelected = value;
                }
                get { return isAnyMethodSelected; }
            }

            public bool IsMethodSelectionChanged
            {
                get { return isAnyMethodSelected != previousIsAnyMethodSelected; }
            }
            
            public bool IsClassSelectionChanged
            {
                get { return isClassSelected != previousIsClassSelected; }
            }
        }

        [MenuItem("Window/Runtime Test Runner")]
        private static void ShowWindow()
        {
            GetWindow(typeof(RuntimeTestToolWindow), false, "Test Runner").Show();
        }

        [SerializeField] private bool isAllSelected;
        private List<UnitTestClass> testClasses;
        private Dictionary<MethodInfo, TestState> testInfo = new Dictionary<MethodInfo, TestState>();
        private Dictionary<string, bool> openedClasses = new Dictionary<string, bool>();
        private List<OpenClassInfo> openedClassesList = new List<OpenClassInfo>();
        private Dictionary<string, SelectedClassInfo> selectedClassInfos = 
            new Dictionary<string, SelectedClassInfo>();

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
            selectedTests = SelectedTestsSerializable.CreateOrLoad();

            FillSelectedTests();
            RestoreOpenedClasses();
            CreateStyleClasses();
            SubscribeToEvents();
            LoadImgs();
        }

        private void FillSelectedTests()
        {
            isAllSelected = true;
            bool isDirty = false;
            
            var testInfoToMethodName = selectedTests.TestInfoToMethodName;
            var allMethodNames = new HashSet<string>();
            foreach (var testClass in testClasses)
            {
                int selectedMethodsCount = 0;
                int totalMethodsCount = testClass.TestMethods.Count;
                foreach (var testMethod in testClass.TestMethods)
                {
                    allMethodNames.Add(testMethod.FullName);
                    if (!testInfoToMethodName.ContainsKey(testMethod.FullName))
                    {
                        isDirty = true;
                        selectedMethodsCount++;
                        testInfoToMethodName[testMethod.FullName] =
                            new SelectedTestsSerializable.TestInfo
                            {
                                ClassName = testClass.Type.FullName,
                                IsSelected = true,
                                MethodName = testMethod.FullName
                            };
                    }
                    else
                    {
                        if (testInfoToMethodName[testMethod.FullName].IsSelected)
                        {
                            selectedMethodsCount++;
                        }
                    }
                }
                
                selectedClassInfos.Add(testClass.Type.FullName, 
                    new SelectedClassInfo(testClass.Type.FullName,
                        selectedMethodsCount > 0, selectedMethodsCount > 0));

                isAllSelected = isAllSelected && selectedMethodsCount == totalMethodsCount;
            }

            var keysToDelete = new List<string>();
            foreach (var testInfo in testInfoToMethodName.Keys)
            {
                if (!allMethodNames.Contains(testInfo))
                {
                    keysToDelete.Add(testInfo);
                }
            }
            foreach (var key in keysToDelete)
            {
                isDirty = true;
                testInfoToMethodName.Remove(key);
            }

            if (isDirty)
            {
                selectedTests.SerializeData();   
            }
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

        private void CustomRepaint()
        {
            Repaint();
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
        
        private const BindingFlags staticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private object CallStaticMethod(Type targetType, string methodName, params object[] methodParams)
        {
            var method = targetType.GetMethod(methodName, staticFlags);
            return method.Invoke(null, methodParams);
        }
        
        private void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            DrawHeader();
            DrawStatistics();
            DrawClasses();
            GUILayout.EndScrollView();
            
            var isDirty = selectedTests && (bool)CallStaticMethod(typeof(EditorUtility), "IsDirty", selectedTests.GetInstanceID());
            if (isDirty)
            {
                var style = new GUIStyle(EditorStyles.boldLabel);
                style.normal.textColor = Color.red;
                GUILayout.Label("You have change scriptable object:", style);
                GUILayout.Label(SelectedTestsSerializable.path, style);
                GUILayout.Label("Please save that asset", style);
            }
            else
            {
                EditorApplication.update -= CustomRepaint;
            }
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
        private float GetContentLenght(GUIContent content)
        {
            GUIStyle style = GUI.skin.label;
            style.alignment = TextAnchor.MiddleCenter;
            Vector2 size = style.CalcSize(content);
            return size.x;
        }

        private void DrawClass(UnitTestClass testClass)
        {
            GUILayout.BeginHorizontal();
            var isOpened = openedClasses.ContainsKey(testClass.Type.FullName) &&
                           openedClasses[testClass.Type.FullName];
            var content = new GUIContent(testClass.Type.FullName, GetClassIcon(testClass));
            
            var oldValue = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = GetContentLenght(content) + 40;
            isOpened = EditorGUILayout.Foldout(isOpened, content, true, defaultFoldoutStyle);
            EditorGUIUtility.labelWidth = oldValue;
            openedClasses[testClass.Type.FullName] = isOpened;
            GUILayout.Space(GetContentLenght(content) - 40);

            bool isDirty = false;
            var info = selectedClassInfos[testClass.Type.FullName];

            
            if (info.IsMethodSelectionChanged)
            {
                info.IsClassSelected = EditorGUILayout.Toggle(info.IsAnyMethodSelected);
            }
            else
            {
                info.IsClassSelected = EditorGUILayout.Toggle(info.IsClassSelected);
            }

            
            if (info.IsClassSelectionChanged && !info.IsMethodSelectionChanged)
            {
                foreach (var method in testClass.TestMethods)
                {
                    selectedTests.TestInfoToMethodName[method.FullName].IsSelected = info.IsClassSelected;
                }   
            }
           GUILayout.FlexibleSpace();

           GUILayout.EndHorizontal();

            if (isOpened)
            {
                EditorGUILayout.BeginVertical(offsetStyle);
                foreach (var mthd in testClass.TestMethods)
                {
                    var foldedRect = EditorGUILayout.BeginHorizontal();
                    DrawMethod(mthd, foldedRect, ref isDirty);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            if (!selectedTests)
            {
                 selectedTests = SelectedTestsSerializable.CreateOrLoad();
            }
            info.IsAnyMethodSelected = selectedTests.IsAnyMethodSelected(testClass.Type.FullName);

            isDirty = isDirty || info.IsClassSelectionChanged;
            if (isDirty)
            {
                EditorUtility.SetDirty(selectedTests);
                EditorApplication.update += CustomRepaint;
                selectedTests.SerializeData();
                if (!selectedTests.TestInfoToMethodName.All(selectedClassInfo => 
                    selectedClassInfo.Value.IsSelected))
                {
                    isAllSelected = false;
                }
                else
                {
                    isAllSelected = true;
                }
            }
        }

        private void DrawMethod(UnitTestMethod testMethod, Rect rect, ref bool isDirty)
        {
            var content = testInfo.ContainsKey(testMethod.Method)
                ? new GUIContent(testMethod.Method.Name, GetMethodIcon(testInfo[testMethod.Method]))
                : new GUIContent(testMethod.Method.Name, defaultIcon);
            EditorGUILayout.LabelField(content);
            rect.x = GetContentLenght(content) + 30;

            if (!selectedTests.TestInfoToMethodName.ContainsKey(testMethod.FullName))
            {
                return;
            }
            
            var prevValue = selectedTests.TestInfoToMethodName[testMethod.FullName].IsSelected;
            selectedTests.TestInfoToMethodName[testMethod.FullName].IsSelected =
                GUI.Toggle(rect, selectedTests.TestInfoToMethodName[testMethod.FullName].IsSelected, "");

            isDirty =  isDirty || prevValue != selectedTests.TestInfoToMethodName[testMethod.FullName].IsSelected;
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
            
            if (GUILayout.Button(isAllSelected ? "Deselect all" : "Select all", GUILayout.Width(120)/*EditorStyles.miniButton*/))
            {
                isAllSelected = !isAllSelected;
                
                foreach (var testInfo in selectedTests.TestInfoToMethodName)
                {
                    testInfo.Value.IsSelected = isAllSelected;
                }
                selectedTests.SerializeData();
            }
            
            if (GUILayout.Button("Run", GUILayout.Width(60)/*EditorStyles.miniButton*/))
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