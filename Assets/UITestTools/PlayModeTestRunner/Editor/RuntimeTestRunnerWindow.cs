using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EditorUITools;
using Newtonsoft.Json.Linq;
using PlayQ.TG.Mediator.Utils;
using Tests.Nodes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PlayQ.UITestTools
{    
    public class RuntimeTestRunnerWindow : EditorWindow
    {
        private const int CLASS_MARGIN = 3;
        private const int METHOD_MARGIN = -1;
        private const int PARENT_MARGIN = 1;
        private const string TEST_STATE_KEY = "TestStates";

        private int runTestsGivenAmountOfTimes = -1;
        private int currentTimescale = -1;

        private const int SPACE_BETWEEN_TOGGLE_AND_BUTTON = 5;

        private const int PROGRESS_BAR_HEIGHT = 22;

        private SelectedNode selectedNode;

        private Dictionary<string, DateTime> clickIntervals = new Dictionary<string, DateTime>();

        [SerializeField]
        private TwoSectionWithSliderDrawer twoSectionDrawer;
        [SerializeField]
        private UITestToolWindowFooter footer;

        public enum TestState
        {
            Undefined,
            Passed,
            Ignored,
            Failed
        }

        private OpenedData openedData = new OpenedData();
        
        [Serializable]
        private class OpenedData
        {
            [Serializable]
            private class SerializableHashset
            {
                public readonly HashSet<string> HashSet = new HashSet<string>();
                private List<string> List = new List<string>();
        
                public void Restore()
                {
                    foreach (var value in List)
                    {
                        HashSet.Add(value);
                    }
                    List.Clear();
                }

                // for storing opened classes during entering to play mode
                public void Store()
                {
                    if (List == null)
                    {
                        List = new List<string>();
                    }
                    foreach (var openedClass in HashSet)
                    {
                        List.Add(openedClass);
                    }
                }
            }

            private SerializableHashset classes = new SerializableHashset();

            public HashSet<string> Classes
            {
                get { return classes.HashSet; }
            }
            
            private SerializableHashset namespaces = new SerializableHashset();

            public HashSet<string> Namespaces
            {
                get { return namespaces.HashSet; }
            }
            
            public void Restore()
            {
                classes.Restore();
                namespaces.Restore();
            }

            public void Store()
            {
                classes.Store();
                namespaces.Store();
            }
        }


        private SelectedTestsSerializable selectedTests;
        
        public bool ForceMakeReferenceScreenshot;
      
        [MenuItem("Window/UI Test Tools/Runtime Test Runner")]
        private static void ShowWindow()
        {
            GetWindow(typeof(RuntimeTestRunnerWindow), false, "Test Runner").Show();
        }


        [Serializable]
        public class TestInfoData
        {
            public string MethodFullName;
            public TestState State;
            public List<string> Logs;
        }

        private Dictionary<string, TestInfoData> testsState = new Dictionary<string, TestInfoData>();

        private Texture2D successIcon;
        private Texture2D failIcon;
        private Texture2D ignoreIcon;
        private Texture2D defaultIcon;

        private GUIStyle offsetStyle;
        private string filter;

        
        private void OnEnable()
        {
            PlayModeTestRunner.RefreshTestsRootNode();

            if (EditorPrefs.HasKey(TEST_STATE_KEY))
            {
                var testsStatesStr = EditorPrefs.GetString(TEST_STATE_KEY);
                JObject testsStatesJObject = JObject.Parse(testsStatesStr);
                testsState = testsStatesJObject.ToObject<Dictionary<string, TestInfoData>>();
            }

            selectedNode = new SelectedNode(testsState);

            CreateOrLoadSelected();

            if (EditorPrefs.HasKey("ForceMakeReferenceScreenshot"))
            {
                ForceMakeReferenceScreenshot = EditorPrefs.GetBool("ForceMakeReferenceScreenshot");
            }

            FillSelectedTests();
            footer = new UITestToolWindowFooter();
            if (twoSectionDrawer == null)
            {
                twoSectionDrawer = new TwoSectionWithSliderDrawer(
                    50,
                    50,
                    int.MaxValue
                );
            }

            CreateStyleClasses();
            openedData.Restore();
            SubscribeToEvents();
            LoadImgs();
        }

        private void FillSelectedTests()
        {
            var allMethodNames = new HashSet<string>();

            foreach (var testClass in ((IAllClassesEnumerable)PlayModeTestRunner.TestsRootNode).AllClasses)
            {
                foreach (var testMethod in ((IAllMethodsEnumerable)testClass).AllMethods)
                {
                    allMethodNames.Add(testMethod.FullName);

                    if (!selectedTests.ContainsTest(testMethod.FullName))
                    {
                        selectedTests.AddTest(testMethod.FullName,
                            new SelectedTestsSerializable.TestInfo(testMethod.FullName, testClass.FullName)
                            {
                                IsSelected = true
                            });
                    }
                }
            }

            var keysToDeleteForSelectedTests = new List<string>();
            foreach (var testInfoKey in selectedTests.TestInfoKeys)
            {
                if (!allMethodNames.Contains(testInfoKey))
                {
                    keysToDeleteForSelectedTests.Add(testInfoKey);
                }
            }
            foreach (var key in keysToDeleteForSelectedTests)
            {
                selectedTests.RemoveTest(key);
            }
            
            var keysToDeleteForTestsState = new List<string>();
            foreach (var testInfoKey in testsState.Keys)
            {
                if (!allMethodNames.Contains(testInfoKey))
                {
                    keysToDeleteForTestsState.Add(testInfoKey);
                }
            }
            foreach (var key in keysToDeleteForTestsState)
            {
                testsState.Remove(key);
            }
        }

        private void CreateStyleClasses()
        {
            offsetStyle = new GUIStyle {margin = {left = 14, right = 0}};
        }

        private void SubscribeToEvents()
        {
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
            string testsStatesStr = JObject.FromObject(testsState).ToString();
            EditorPrefs.SetString(TEST_STATE_KEY, testsStatesStr);
            openedData.Store();
            PlayModeTestRunner.OnTestFailed -= OnTestFailed;
            PlayModeTestRunner.OnTestPassed -= OnTestPassed;
            PlayModeTestRunner.OnTestIgnored -= OnTestIgnored;
        }

        private void CustomRepaint()
        {
            Repaint();
        }

        private void OnTestIgnored(string methodFullName, List<string> testLogs)
        {
            UpdateTestState(methodFullName, TestState.Ignored, testLogs);
        }

        private void OnTestPassed(string methodFullName, List<string> testLogs)
        {
            UpdateTestState(methodFullName, TestState.Passed, testLogs);
        }

        private void OnTestFailed(string methodFullName, List<string> testLogs)
        {
            UpdateTestState(methodFullName, TestState.Failed, testLogs);
        }

        private void UpdateTestState(string methodFullName, TestState state, List<string> testLogs)
        {
            if (testsState.ContainsKey(methodFullName))
            {
                testsState[methodFullName].State = state;
                testsState[methodFullName].Logs = testLogs;
            }
            else
            {
                testsState[methodFullName] = new TestInfoData()
                {
                    MethodFullName = methodFullName,
                    State = state,
                    Logs = testLogs
                };
            }

            Repaint();
        }

        private void OnGUI()
        {
            if (GUI.GetNameOfFocusedControl() != string.Empty)
            {
                selectedNode.Node = null;
            }

            selectedNode.MemorizePreviousNode();

            GUI.enabled = !Application.isPlaying;
            DrawHeader((int) position.width);
            GUI.enabled = true;
            DrawStatistics();
            DrawFilter();
            DrawClasses();
            DrawFooter();
        }
        
        

        private void DrawFilter()
        {
            GUI.SetNextControlName("Filter");
            filter = EditorGUILayout.TextField("Filter:", filter);
        }

        private Rect GetFooterRect()
        {
            Rect window = position;

            const int panelHeight = 45;

            Rect panel = window;

            panel.y = panel.height - panelHeight;

            panel.height = panelHeight;

            panel.x = 0;

            const int labelHeight = 20;

            window = new Rect(0, panel.y + labelHeight, panel.width, labelHeight);

            return window;
        }

        
        
        private void DrawFooter()
        {
            if (!IsDirty)
            {
                EditorApplication.update -= CustomRepaint;
            }
            footer.Draw(position, IsDirty);
            
            if (PlayModeTestRunner.IsRunning)
            {
                float positionY = position.height - (PROGRESS_BAR_HEIGHT + EditorUIConstants.LAYOUT_PADDING);

                if (IsDirty)
                {
                    positionY -= (UITestToolWindowFooter.HEIGHT + EditorUIConstants.LAYOUT_PADDING);
                }

                Rect r = new Rect(
                    new Vector2(0, positionY),
                    new Vector2(position.width, PROGRESS_BAR_HEIGHT));
                
                var currentStep = TestStep.GetCurrentStep();
                if (!PlayModeTestRunner.IsRunning || currentStep == null)
                {
                    GUI.enabled = false;
                    EditorGUI.ProgressBar(r, 0, "");
                    GUI.enabled = true;
                }
                else
                {
                    EditorGUI.ProgressBar(r, currentStep.Progress, currentStep.Text);
                    Repaint();
                }
            }
        }


        private bool IsDirty
        {
            get
            {
                var isDirty = selectedTests &&
                              (bool) Reflector.CallStaticMethod(typeof(EditorUtility), "IsDirty",
                                  selectedTests.GetInstanceID());
                return isDirty;
            }
        }

        private void DrawClasses()
        {
            float footer = 0;

            footer += UITestToolWindowFooter.HEIGHT + EditorUIConstants.LAYOUT_PADDING;

            if (PlayModeTestRunner.IsRunning)
            {
                footer += PROGRESS_BAR_HEIGHT + EditorUIConstants.LAYOUT_PADDING;
            }

            if (twoSectionDrawer.Draw(
                position,
                GUILayoutUtility.GetLastRect().max.y,
                footer, EditorStyles.helpBox, () =>
                {
                    ShowNode((HierarchicalNode)PlayModeTestRunner.TestsRootNode, string.Empty, true);
                },
                GUIStyle.none,
                () =>
                {
                    if (selectedNode.logs.Count > 0)
                    {
                        List<string> reverseLogs = new List<string>(selectedNode.logs);
                        reverseLogs.Reverse();

                        string totalLog = String.Join("\n", reverseLogs.ToArray());
                        totalLog = totalLog.Substring(0, Math.Min(totalLog.Length, 15000 - 4));

                        if (totalLog.Length == (15000 - 4))
                            totalLog = string.Concat(totalLog, "\n...");

                        GUILayout.TextArea(totalLog);
                    }
                }))
            {
                Repaint();
            }
        }


        private void ShowNode(HierarchicalNode node, string fullname, bool isRootNode = false)
        {
            if (node == null || (!string.IsNullOrEmpty(filter) && !node.Contains(filter)))
            {
                return;
            }

            bool isDirty = false;

            if (!isRootNode)
            {
                GUILayout.Space(CLASS_MARGIN);

                Rect rect = EditorGUILayout.BeginHorizontal();

                DrawBackgroundIfSelected(node, rect);

                bool isOpened = openedData.Namespaces.Contains(node.Name);

                bool newIsOpened = DrawFoldout(isOpened);

                HandleKeyboardEvent(node,
                                   () =>
                                   {
                                       testsState.Clear();

                                       PlayModeTestRunner.RunTestByDoubleClick(PlayModeTestRunner.SpecificTestType.Class,
                                           fullname);
                                   },
                                   ref newIsOpened);

                if (isOpened != newIsOpened)
                {
                    if (newIsOpened)
                    {
                        openedData.Namespaces.Add(node.Name);
                    }
                    else
                    {
                        openedData.Namespaces.Remove(node.Name);
                    }
                }

                if (fullname == String.Empty)
                {
                    fullname = node.Name;
                }
                else
                {
                    fullname += "." + node.Name;
                }

                bool selected = true;
                bool partialSelected = false;
                foreach (var methodName in selectedTests.TestInfoKeys)
                {
                    var testInfo = selectedTests.GetTest(methodName);
                    if (CustomStartsWith(fullname, methodName))
                    {
                        selected &= testInfo.IsSelected;
                        if (testInfo.IsSelected)
                        {
                            partialSelected = true;
                        }
                    }

                    if (!selected && partialSelected)
                    {
                        break;
                    }
                }

                bool oldSelected = selected;

                DrawTestLine(ref selected,
                             partialSelected,
                             ref newIsOpened,
                             node,
                             GetNameSpaceState(node),
                             true,
                             () =>
                             {
                                 selectedNode.Node = node;
                             },
                             () =>
                             {
                                 testsState.Clear();

                                 PlayModeTestRunner.RunTestByDoubleClick(PlayModeTestRunner.SpecificTestType.Class,
                                     fullname);
                             });

                if (oldSelected != selected)
                {
                    isDirty = true;

                    foreach (var methodName in selectedTests.TestInfoKeys)
                    {
                        var test = selectedTests.GetTest(methodName);

                        if (CustomStartsWith(fullname, methodName))
                        {
                            test.IsSelected = selected;
                        }
                    }
                }

                GUILayout.FlexibleSpace();

                EditorGUILayout.EndVertical();
            }
            if (isRootNode || openedData.Namespaces.Contains(node.Name))
            {
                if (!isRootNode)
                {
                    EditorGUILayout.BeginVertical(offsetStyle);
                }

                foreach (Node childNode in node.Children)
                {
                    ClassNode classNode = childNode as ClassNode;

                    if (classNode != null)
                        isDirty |= DrawClass(classNode);
                    else
                        ShowNode((HierarchicalNode)childNode, fullname);
                }

                if (!isRootNode)
                {
                    EditorGUILayout.EndVertical();
                }
            }

            if (selectedNode.WasAnotherNodeSelected)
                Repaint();

            if (isDirty)
            {
                EditorUtility.SetDirty(selectedTests);
                EditorApplication.update += CustomRepaint;
            }

        }

        private void DrawBackgroundIfSelected(Node node, Rect rect)
        {
            if (selectedNode.IsSelected(node))
            {
                rect.min = new Vector2(0, rect.min.y);

                Color currentBackgroundColor = GUI.backgroundColor;

                GUI.backgroundColor = new Color(0.098f, 0.71f, 0.996f);

                GUI.Box(rect, GUIContent.none);

                GUI.backgroundColor = currentBackgroundColor;
            }
        }

        public static bool CustomEndsWith(string a, string b)
        {
            int ap = a.Length - 1;
            int bp = b.Length - 1;

            while (ap >= 0 && bp >= 0 && a[ap] == b[bp])
            {
                ap--;
                bp--;
            }
            return (bp < 0 && a.Length >= b.Length) ||

                    (ap < 0 && b.Length >= a.Length);
        }

        public static bool CustomStartsWith(string a, string b)
        {
            int aLen = a.Length;
            int bLen = b.Length;
            int ap = 0; int bp = 0;

            while (ap < aLen && bp < bLen && a[ap] == b[bp])
            {
                ap++;
                bp++;
            }

            return (bp == bLen && aLen >= bLen) ||

                    (ap == aLen && bLen >= aLen);
        }

        private float GetContentLenght(GUIContent content)
        {
            GUIStyle style = GUI.skin.label;
//            style.alignment = TextAnchor.MiddleCenter;
            Vector2 size = style.CalcSize(content);
            return size.x;
        }

        private bool DrawFoldout(bool isOpened)
        {
            EditorGUIUtility.fieldWidth = 12;

            return EditorGUILayout.Foldout(isOpened, "");
        }
        
        private bool DrawClass(ClassNode testClass)
        {
            if (!string.IsNullOrEmpty(filter) && !testClass.Contains(filter))
            {
                return false;
            }

            GUILayout.Space(CLASS_MARGIN);

            Rect rect = EditorGUILayout.BeginHorizontal();

            DrawBackgroundIfSelected(testClass, rect);

            var isOpened = openedData.Classes.Contains(testClass.Type.FullName);
            
            var isOpenedNew = DrawFoldout(isOpened);

            HandleKeyboardEvent(testClass, 
                                () =>
                                {
                                    foreach (var method in ((IAllMethodsEnumerable)testClass).AllMethods)
                                    {
                                        testsState.Remove(method.FullName);
                                    }

                                    PlayModeTestRunner.RunTestByDoubleClick(PlayModeTestRunner.SpecificTestType.Class,
                                        testClass.Type.FullName);
                                },
                                ref isOpenedNew);

            bool isDirty = false;

            bool selected = true;
            bool partialSelected = false;
            foreach (var method in ((IAllMethodsEnumerable)testClass).AllMethods)
            {
                var test = selectedTests.GetTest(method.FullName);

                if (test == null)
                {
                    Debug.LogError(String.Format("RuntimeTestRunnerWindow: CANNOT FIND TEST {0} IN SELECTED TESTS", method.FullName));

                    break;
                }

                selected &= test.IsSelected;

                if (test.IsSelected)
                {
                    partialSelected = true;
                }
                if (!selected && partialSelected)
                {
                    break;
                }
            }

            bool oldSelected = selected;

            DrawTestLine(ref selected,
                         partialSelected,
                         ref isOpenedNew,
                         testClass,
                         GetClassState(testClass), 
                         true,
                         () =>
                         {
                             selectedNode.Node = testClass;
                         },
                         () =>
                         {
                             foreach (var method in ((IAllMethodsEnumerable)testClass).AllMethods)
                             {
                                 testsState.Remove(method.FullName);
                             }
                             
                             PlayModeTestRunner.RunTestByDoubleClick(PlayModeTestRunner.SpecificTestType.Class,
                                 testClass.Type.FullName);
                         });
            
            if (isOpenedNew != isOpened)
            {
                if (isOpenedNew)
                {
                    openedData.Classes.Add(testClass.Type.FullName);
                }
                else
                {
                    openedData.Classes.Remove(testClass.Type.FullName);
                }

                isOpened = isOpenedNew;
            }

            if (oldSelected != selected)
            {
                isDirty = true;

                foreach (var method in ((IAllMethodsEnumerable)testClass).AllMethods)
                {
                    selectedTests.GetTest(method.FullName).IsSelected = selected;                
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (isOpened)
            {
                GUILayout.Space(PARENT_MARGIN);
                EditorGUILayout.BeginVertical(offsetStyle);

                foreach (var childNode in testClass.Children)
                {
                    ClassNode classNode = childNode as ClassNode;

                    if (classNode != null)
                        isDirty |= DrawClass(classNode);

                    MethodNode methodNode = childNode as MethodNode;

                    if (methodNode != null)
                        isDirty |= DrawMethod(methodNode);
                }

                EditorGUILayout.EndVertical();
            }

            return isDirty;
        }

        private void CreateOrLoadSelected()
        {
            if (!selectedTests)
            {
                selectedTests = SelectedTestsSerializable.CreateOrLoad();
            }
        }
        
        private bool DrawMethod(MethodNode testMethod)
        {
            if (!string.IsNullOrEmpty(filter) && !testMethod.Contains(filter))
            {
                return false;
            }

            GUILayout.Space(METHOD_MARGIN);

            Rect rect = EditorGUILayout.BeginHorizontal();

            DrawBackgroundIfSelected(testMethod, rect);

            GUILayout.Space(20);

            var testInfo = selectedTests.GetTest(testMethod.FullName);

            var prevValue = testInfo.IsSelected;

            bool newIsOpened = false;

            HandleKeyboardEvent(testMethod,
                                () =>
                                {
                                    testsState.Remove(testMethod.FullName);

                                    PlayModeTestRunner.RunTestByDoubleClick(PlayModeTestRunner.SpecificTestType.Method, testMethod.FullName);
                                },
                                ref newIsOpened);

            DrawTestLine(ref testInfo.IsSelected,
                         false,
                         ref newIsOpened,
                         testMethod,
                         testsState.ContainsKey(testMethod.FullName)
                            ? testsState[testMethod.FullName].State
                            : TestState.Undefined,
                         false,
                         () =>
                         {
                            selectedNode.Node = testMethod;
                         },
                         () =>
                         {
                             testsState.Remove(testMethod.FullName);

                             PlayModeTestRunner.RunTestByDoubleClick(PlayModeTestRunner.SpecificTestType.Method, testMethod.FullName);
                         });

            bool isDirty = prevValue != testInfo.IsSelected;

            EditorGUILayout.EndHorizontal();

            return isDirty;
        }

        private void HandleKeyboardEvent(Node node, Action OnReturnKeyPress, ref bool collapsed)
        {
            if (selectedNode.IsSelected(node)
                && !selectedNode.WasAnotherNodeSelected
                && Event.current.type == EventType.KeyDown)
            {

                if (GUI.GetNameOfFocusedControl() != "")
                {
                    return;
                }

                switch (Event.current.keyCode)
                {
                    case KeyCode.Return:

                        if (Application.isPlaying)
                        {
                            return;
                        }

                        if (OnReturnKeyPress != null)
                        {
                            OnReturnKeyPress();
                        }

                        return;

                    case KeyCode.UpArrow:

                        if (selectedNode.Node.Parent != null && !selectedNode.Node.Parent.IsRoot)
                        {
                            if (selectedNode.Node.Index > 0)
                            {
                                Node targetNode = selectedNode.Node.Parent.ChildAt(selectedNode.Node.Index - 1);
                                bool isOpened = false;
                                if (targetNode is ClassNode)
                                {
                                    isOpened = openedData.Classes.Contains(targetNode.FullName);
                                }
                                if (targetNode is HierarchicalNode)
                                {
                                    isOpened = openedData.Namespaces.Contains(targetNode.FullName);
                                }

                                if (isOpened)
                                {
                                    targetNode = targetNode.ChildAt(targetNode.Children.Count() - 1);
                                }

                                selectedNode.Node = targetNode;
                            }
                            else
                            {
                                selectedNode.Node = selectedNode.Node.Parent;
                            }
                        }

                        Event.current.Use();

                        return;

                    case KeyCode.DownArrow:

                        if (selectedNode.Node.Children.Count() > 0 && collapsed)
                        {
                            selectedNode.Node = selectedNode.Node.ChildAt(0);

                            Event.current.Use();

                            return;
                        }

                        TrySelectElementBelow(selectedNode.Node);

                        Event.current.Use();

                        return;

                    case KeyCode.RightArrow:

                        if (selectedNode.Node.Children.Count() > 0)
                        {
                            if (!collapsed)
                            {
                                collapsed = true;

                                Event.current.Use();

                                return;
                            }

                            selectedNode.Node = selectedNode.Node.ChildAt(0);
                        }

                        Event.current.Use();

                        return;

                    case KeyCode.LeftArrow:

                        if (collapsed)
                        {
                            collapsed = false;

                            Event.current.Use();

                            return;
                        }

                        if (selectedNode.Node.Parent != null && !selectedNode.Node.Parent.IsRoot)
                        {
                            selectedNode.Node = selectedNode.Node.Parent;
                        }

                        Event.current.Use();

                        return;
                }

                return;
            }
        }

        private void TrySelectElementBelow(Node node)
        {
            if (node.Parent != null && !node.Parent.IsRoot)
            {
                if (node.Parent.Children.Count() == node.Index + 1)
                {
                    TrySelectElementBelow(node.Parent);
                }
                else
                    selectedNode.Node = node.Parent.ChildAt(node.Index + 1);
            }
        }

        private void DrawTestLine(ref bool fullSelected,
                                  bool partialSelected,
                                  ref bool isOpenedNew,
                                  Node node,
                                  TestState state,
                                  bool isNamespace, 
                                  Action OnClick,
                                  Action OnDoubleClick)
        {
            Color oldColor = GUI.backgroundColor;

            if (partialSelected && !fullSelected)
            {
                GUI.backgroundColor = new Color(174f / 255f, 208f / 255f, 1, 1);
            }

            GUI.SetNextControlName("TestItem");

            fullSelected = GUILayout.Toggle(fullSelected, "", GUILayout.Width(10 + SPACE_BETWEEN_TOGGLE_AND_BUTTON));

            GUI.backgroundColor = oldColor;

            var content = new GUIContent(node.Name, GetTestIcon(state));

            var guiStyle = new GUIStyle(EditorStyles.label);
            if (isNamespace)
            {
                guiStyle.fontStyle = FontStyle.Bold;
            }

            GUILayout.Space(-SPACE_BETWEEN_TOGGLE_AND_BUTTON);

            var click = GUILayout.Button(content,
                guiStyle,
                GUILayout.ExpandWidth(true),
                GUILayout.Width((isNamespace ? EditorStyles.boldLabel : EditorStyles.label)
                                .CalcSize(content).x+10));

            if (click)
            {
                GUI.FocusControl(null);
                DateTime currentClickTime = DateTime.Now;
                DateTime prevClick;
                clickIntervals.TryGetValue(node.Name, out prevClick);
                if ((currentClickTime - prevClick).TotalMilliseconds < 300 && !Application.isPlaying)
                {
                    if (OnDoubleClick != null)
                    {
                        OnDoubleClick();
                    }
                }
                else
                {
                    if (OnClick != null)
                    {
                        OnClick();
                    }
                }
                clickIntervals[node.Name] = currentClickTime;
            }
        }

        private Texture2D GetTextureByState(TestState state)
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
                    throw new ArgumentOutOfRangeException();
            }
        }

        private TestState GetClassState(ClassNode testClass)
        {
            var passedTests = 0;
            var ignoredTests = 0;
            var totalTests = 0;

            foreach (var testMethod in ((IAllMethodsEnumerable)testClass).AllMethods)
            {
                totalTests++;

                TestInfoData data;

                if (testsState.TryGetValue(testMethod.FullName, out data))
                {
                    switch (data.State)
                    {
                        case TestState.Passed:
                            passedTests++;
                            break;
                        case TestState.Ignored:
                            ignoredTests++;
                            break;
                        case TestState.Failed:
                            return TestState.Failed;
                    }
                }
            }

            if (passedTests == totalTests)
            {
                return TestState.Passed;
            }

            return ignoredTests > 0 ? TestState.Ignored : TestState.Undefined;
        }
        
        private TestState GetNameSpaceState(HierarchicalNode node)
        {
            bool ignored = false;
            bool undefined = false;

            foreach (var childNode in node.Children)
            {
                ClassNode classNode = childNode as ClassNode;

                if (classNode != null)
                {
                    var state = GetClassState(classNode);

                    switch (state)
                    {
                        case TestState.Undefined:

                            undefined = true;

                            break;

                        case TestState.Ignored:

                            ignored = true;

                            break;

                        case TestState.Failed:

                            return TestState.Failed;
                    }
                }
                else
                {
                    var state = GetNameSpaceState((HierarchicalNode)childNode);

                    switch (state)
                    {
                        case TestState.Undefined:

                            undefined = true;

                            break;

                        case TestState.Ignored:

                            ignored = true;

                            break;

                        case TestState.Failed:

                            return TestState.Failed;
                    }
                }
            }

            if (ignored)
            {
                return TestState.Ignored;
            }
            if (undefined)
            {
                return TestState.Undefined;
            }
            return TestState.Passed;
        }

        private Texture2D GetTestIcon(TestState state)
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
            EditorGUILayout.LabelField(new GUIContent("Passed:", successIcon), GUILayout.Width(100f));
            EditorGUILayout.LabelField(testsState.Values.Count(t => t.State == TestState.Passed).ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Failed:", failIcon), GUILayout.Width(100f));
            EditorGUILayout.LabelField(testsState.Values.Count(t => t.State == TestState.Failed).ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Ignored:", ignoreIcon), GUILayout.Width(100f));
            EditorGUILayout.LabelField(testsState.Values.Count(t => t.State == TestState.Ignored).ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Total:", defaultIcon), GUILayout.Width(100f));
            EditorGUILayout.LabelField(PlayModeTestRunner.TestsAmount.ToString());
            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }


        private void DrawHeader(int width)
        {
            const int buttonOffset = 3;

            width -= 10;
            
            GUILayout.Space(5f);

            EditorGUILayout.BeginHorizontal();

            int buttonSize = width/4 - buttonOffset;

            if (GUILayout.Button("Select all", GUILayout.Width(buttonSize)))
            {   
                foreach (var testName in selectedTests.TestInfoKeys)
                {
                    var testInfo = selectedTests.GetTest(testName);

                    testInfo.IsSelected = true;
                }
            }

            if (GUILayout.Button("Deselect all", GUILayout.Width(buttonSize)))
            {   
                foreach (var testName in selectedTests.TestInfoKeys)
                {
                    var testInfo = selectedTests.GetTest(testName);

                    testInfo.IsSelected = false;
                }
            }
            
            if (GUILayout.Button("Run all", GUILayout.Width(buttonSize)))
            {
                testsState.Clear();

                RunTestsMode.SelectAllTests();

                PlayModeTestRunner.Run();
            }
            
            if (GUILayout.Button("Run smoke", GUILayout.Width(buttonSize)))
            {
                ClearAllSmokeMethods();

                RunTestsMode.SelectOnlySmokeTests();

                PlayModeTestRunner.Run();
            }
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            
            
            if (GUILayout.Button("Run selected", GUILayout.Width(width * 0.25f - buttonOffset)))
            {
                foreach (var testInfo in selectedTests.SelectedTestsInfos)
                {
                    testsState.Remove(testInfo.MethodName);
                }

                RunTestsMode.SelectOnlySelectedTests();

                PlayModeTestRunner.Run();
            }

            if (runTestsGivenAmountOfTimes == -1)
            {
                runTestsGivenAmountOfTimes = PlayModeTestRunner.RunTestsGivenAmountOfTimes;
            }

            GUI.SetNextControlName("Repeating");

            int newTimes = EditorGUILayout.IntSlider("Repeat Tests N Times:", 
                runTestsGivenAmountOfTimes, 1,20);

            if (newTimes != runTestsGivenAmountOfTimes)
            {
                runTestsGivenAmountOfTimes = newTimes;

                PlayModeTestRunner.RunTestsGivenAmountOfTimes = newTimes;
            }

            EditorGUILayout.EndHorizontal();

            if (currentTimescale == -1)
            {
                currentTimescale = (int) PlayModeTestRunner.DefaultTimescale;
            }

            GUI.SetNextControlName("Timescale");

            int newTimescale = EditorGUILayout.IntSlider("Default Timescale:", currentTimescale, 1,20);

            if (newTimescale != currentTimescale)
            {
                currentTimescale = newTimescale;
                PlayModeTestRunner.DefaultTimescale = currentTimescale;
            }
            
            var text = "Make reference screenshot instead of comparing:";

            var content = new GUIContent(text);

            var oldValue = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = GetContentLenght(content);

            GUI.SetNextControlName("Reference");

            ForceMakeReferenceScreenshot = EditorGUILayout.Toggle(text, ForceMakeReferenceScreenshot);

            EditorPrefs.SetBool("ForceMakeReferenceScreenshot", ForceMakeReferenceScreenshot);

            EditorGUIUtility.labelWidth = oldValue;
            
            EditorGUILayout.LabelField("Playmode tests: ", EditorStyles.boldLabel);
        }

        private void ClearAllSmokeMethods()
        {
            List<string> smokeMethods = new List<string>();

            foreach (string key in testsState.Keys)
            {
                foreach (var testClass in ((IAllClassesEnumerable)PlayModeTestRunner.TestsRootNode).AllClasses)
                    foreach (var testMethod in ((IAllMethodsEnumerable)testClass).AllMethods)
                    if (testMethod.FullName == testsState[key].MethodFullName)
                    {
                        if (testMethod.TestSettings.IsSmoke)
                        {
                            smokeMethods.Add(key);
                        }
                    }
            }

            foreach (string key in smokeMethods)
            {
                testsState.Remove(key);
            }
        }
    }
}