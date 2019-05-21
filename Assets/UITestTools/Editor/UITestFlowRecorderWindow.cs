using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using EditorUITools;

namespace PlayQ.UITestTools
{
    public class UITestFlowRecorderWindow : EditorWindow
    {
        private Texture normalIconTexture;
        private GUIContent normalIconContent;
        private Texture grayscaleIconTexture;
        private GUIContent grayscaleIconContent;

        private UITestUserFlowRecorderController controller;

        private Vector2 scrollPosition;
        private Vector2 codeScrollPosition;

        private UnityEditorInternal.ReorderableList reorderableList;
        private List<float> heights = new List<float>();

        private static GameObject goSelectedForNewAssertation;
        private static bool isOpen;
        private ParameterDrawersFactory drawersFactory;

        private TwoSectionWithSliderDrawer twoSectionDrawer;
        private UITestToolWindowFooter footer;

        [MenuItem("Window/UI Test Tools/Flow Recorder %t")]
        static void Init()
        {
            UITestFlowRecorderWindow window =
                (UITestFlowRecorderWindow) GetWindow(typeof(UITestFlowRecorderWindow), false, "Recorder");
            window.Show();
        }
        
        
        [MenuItem("GameObject/Create Assertation", false, 0)]
        private static void CreateAssertation()
        {
            if (!isOpen)
            {
                UITestFlowRecorderWindow window =
                    (UITestFlowRecorderWindow) GetWindow(typeof(UITestFlowRecorderWindow), false, "Recorder");
                window.Show();
            }

            var go = Selection.activeObject as GameObject;
            goSelectedForNewAssertation = go;
        }

        private void OnEnable()
        {
            drawersFactory = new ParameterDrawersFactory();
            footer = new UITestToolWindowFooter();
            isOpen = true;
            normalIconTexture = Resources.Load("test_record_color") as Texture;
            normalIconContent = new GUIContent("Recorder", normalIconTexture);
            grayscaleIconTexture = Resources.Load("test_record_grayscale") as Texture;
            grayscaleIconContent = new GUIContent("Recorder", grayscaleIconTexture);
            EditorApplication.update += DoUpdate;
            if (controller == null)
            {
                controller = new UITestUserFlowRecorderController();
            }
            //            CreateFakeActions();

            if (twoSectionDrawer == null)
            {
                twoSectionDrawer = new TwoSectionWithSliderDrawer(
                    70,
                    50,
                    int.MaxValue
                );
            }
        }

        private MethodInfo getGameWindowMethod;
        private void SetFocusToGameWindow()
        {
            if (getGameWindowMethod == null)
            {
                getGameWindowMethod = Type.GetType("UnityEditor.GameView, UnityEditor.dll").GetMethod("GetMainGameView",
                    BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
            var gameWindow = (EditorWindow) getGameWindowMethod.Invoke(null,null);
            if (gameWindow != null)
            {
                gameWindow.Focus();
            }
        }
        
        
        private void SetFocusToUIRecorder()
        {
             Focus();
        }
        
       
        private void OnDisable()
        {
            EditorApplication.update -= DoUpdate;
            isOpen = false;
        }

        private bool switchedByHotKey;
        private void DoUpdate()
        {
            if (controller != null && controller.IsRecording && Input.GetKey(KeyCode.C) && Input.GetKey(KeyCode.LeftAlt) &&
                (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl)))
            {
                if (!switchedByHotKey)
                {
                    controller.ChagneAssertation();
                    switchedByHotKey = true;
                    Repaint();
                }
            }
            else
            {
                switchedByHotKey = false;
            }

            titleContent = controller.IsRecording ? normalIconContent : grayscaleIconContent;
            if (controller.IsRecording)
            {
                if (controller.Update())
                {
                    Repaint();
                }
            }

            GenerateUserActionForGameObject();
        }

        private void GenerateUserActionForGameObject()
        {
            if (goSelectedForNewAssertation)
            {
                controller.HandleGameObject(goSelectedForNewAssertation);
                goSelectedForNewAssertation = null;
                Selection.activeObject = null;
                SetFocusToUIRecorder();
            }
        }

        private Color indexColor = new Color(0,0,0,32f/256f);
        private const int INDEX_SIZE = 19;

        void OnGUI()
        {
            if (!switchedByHotKey 
                && controller.IsRecording
                && Event.current != null 
                && Event.current.type == EventType.KeyDown 
                && Event.current.alt 
                && (Event.current.command ||Event.current.command) 
                && Event.current.keyCode==KeyCode.C)
            {
                controller.ChagneAssertation();

                Repaint();
            }            

            if (controller.IsAssertationMode)
            {
                GUI.backgroundColor = new Color(199f / 255, 255f / 255, 213f / 255);
            }

            DrawHeader();

            if (EditorApplication.isPaused && controller.IsRecording)
            {
                GUILayout.Space(5);

                string text = string.Join("\n", new string[] 
                {
                    "Can't record when editor pause enabled!",
                    "Use 'Pause mode' button instead."
                });

                EditorGUILayout.HelpBox(text, MessageType.Warning);
            }

            if (twoSectionDrawer.Draw(
                position,
                GUILayoutUtility.GetLastRect().max.y,
                UITestToolWindowFooter.HEIGHT,
                GUIStyle.none,
                DrawListOfUserAction,
                GUIStyle.none,
                DrawBottomPanel))
            {
                Repaint();
            }

            footer.Draw(position);
        }


        private bool collapsed;
        private void DrawBottomPanel()
        {
            EditorGUILayout.BeginHorizontal();
            float width = position.width - 15;
            GUI.enabled = controller.UserActions.Count > 0;
            if (GUILayout.Button("Generate Code and Copy", GUILayout.Height(30), GUILayout.ExpandWidth(true)))
            {
                controller.GenerateCode();
            }
            bool codeGenerated = !String.IsNullOrEmpty(controller.GeneratedCode);

            GUI.enabled = codeGenerated;

            if (GUILayout.Button("Copy", GUILayout.Height(30),GUILayout.Width(40)))
            {
                controller.CopyToBuffer();
            }

            collapsed = codeGenerated;

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            GUI.enabled = controller.UserActions.Count > 0;
            EditorGUIUtility.labelWidth = 120;
            GUI.enabled = true;

            if (collapsed)
            {
                codeScrollPosition = EditorGUILayout.BeginScrollView(codeScrollPosition);
                GUILayout.TextArea(controller.GeneratedCode, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        void AddMenuItem(GenericMenu menu, string menuPath, UserActionInfo action, int index)
        {
            menu.AddItem(new GUIContent(menuPath), false, () =>
            {
                action.SelectedIndex = index;
            });
        }
        
        int indexToCopy = -1;
        UserActionInfo toRemove;
        
        
        
        private void DrawUserClickInfo(Rect rect, int index, bool isActive, bool isFocused)
        {
            float topLineHeight = EditorGUIUtility.singleLineHeight;
            var rectToDrawWith = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);

            var userAction = (UserActionInfo)reorderableList.list[index];

            if (userAction.SelectedAssertation == null || userAction.SelectedAssertation.methodInfo == null)
            {
                Debug.LogError(String.Format("UITestFlowRecorderWindow: SELECTED ASSERTATION OR ITS METHOD INFO IS NULL FOR ELEMENT #{0}", index));

                toRemove = userAction;

                return;
            }

            AbstractParameter[] parameters = new AbstractParameter[0];
            if (userAction.SelectedCodeGenerator != null)
            {
                var generators = userAction.SelectedCodeGenerator.CalculateGeneratorSequence(); 
                parameters = generators.OfType<AbstractParameter>().ToArray();
            }
            
            var shiftY = (heights[index] - EditorGUIUtility.singleLineHeight) / (parameters.Length + 2);
            rectToDrawWith.y += EditorGUIUtility.singleLineHeight * 0.5f;

            rectToDrawWith.width -= 64;

            rectToDrawWith.width -= 62;
            rectToDrawWith.x += 30;

            var style = new GUIStyle(GUI.skin.label) {fontSize = INDEX_SIZE, normal = new GUIStyleState() {textColor = indexColor}, stretchWidth = true, fixedHeight = INDEX_SIZE + 10};
            EditorGUI.LabelField(new Rect(rectToDrawWith.x - 30, rectToDrawWith.y, 30, rectToDrawWith.height), (index+1).ToString(), style);

            if (userAction.SelectedAssertation == null)
            {
                return;
            }
            
            var labelForButton = userAction.SelectedAssertation.AssertationMethodDescription;
            labelForButton = labelForButton.Replace('/', ' ');
            if (GUI.Button(rectToDrawWith, labelForButton))
            {
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < userAction.AvailableAssertations.Count; i++)
                {
                    AddMenuItem(menu, userAction.AvailableAssertations[i].AssertationMethodDescription, userAction, i);    
                }
                
                menu.ShowAsContext();
            }
            
            rectToDrawWith.width += 62;    
            rectToDrawWith.x -= 30;


            Rect buttonRect = new Rect();
            buttonRect.x = rectToDrawWith.width + rectToDrawWith.x - 30; 
            buttonRect.y = rectToDrawWith.y;
            buttonRect.height = 16;
            buttonRect.width = 30;

            
            EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
            if (GUI.Button(buttonRect, "►"))
            {
                controller.ApplyAction(userAction);
            }

            buttonRect.x += 32;
            buttonRect.width = 37;
            EditorGUI.EndDisabledGroup();

            if (CopyShortcutPressed && isFocused)
            {
                indexToCopy = index;

                Repaint();
            }

            if (GUI.Button(buttonRect, "copy"))
            {
                indexToCopy = index;
            }
            buttonRect.x = rectToDrawWith.width + rectToDrawWith.x + 41; 
            buttonRect.y = rectToDrawWith.y;
            buttonRect.height = 16;
            buttonRect.width = 22;

            if (GUI.Button(buttonRect, "x"))
            {
                toRemove = (UserActionInfo)reorderableList.list[index];
            }
            rectToDrawWith.width += 64 - 28;

            rectToDrawWith.y += shiftY + 4;
            rectToDrawWith.x += 28;
            EditorGUIUtility.labelWidth = 100;

            userAction.Description = EditorGUI.TextField(rectToDrawWith, "Description: ", userAction.Description);

            foreach (var parameter in parameters)
            {
                rectToDrawWith.y += shiftY;
                var drawer = drawersFactory.GetDrawer(parameter);
                if (drawer != null)
                {
                    drawer.EditorDraw(parameter, rectToDrawWith,
                        userAction.SelectedAssertation.methodInfo);
                }
            }
            
        }

        private bool CopyShortcutPressed
        {
            get
            {
                Event currentEvent = Event.current;

                if (currentEvent.type != EventType.KeyDown)
                {
                    return false;
                }

#if UNITY_EDITOR_OSX
                bool commandKeyPressed = currentEvent.command;
#else
                bool commandKeyPressed = currentEvent.control;
#endif
                return commandKeyPressed && currentEvent.keyCode == KeyCode.D;
            }
        }

        private void CalculateListItemsHeight()
        {
            heights.Clear();
            foreach (var action in controller.UserActions)
            {
                if (action.SelectedCodeGenerator == null)
                {
                    heights.Add(0);  
                    continue;
                }

                var generators = action.SelectedCodeGenerator.CalculateGeneratorSequence();
                var height = EditorGUIUtility.singleLineHeight * 1.2f *
                             (generators.OfType<AbstractParameter>().Count() + 2)
                            + EditorGUIUtility.singleLineHeight;
                heights.Add(height);    
            }   
        }

        private List<int> m_NonDragTargetIndices;

        private void DrawListOfUserAction()
        {
            if (Event.current.type != EventType.Repaint)
            {
                if (indexToCopy != -1)
                {
                    var newUserAction = new UserActionInfo(controller.UserActions[indexToCopy]);

                    controller.UserActions.Insert(indexToCopy + 1, newUserAction);

                    indexToCopy = -1;
                }
                if (toRemove != null)
                {
                    controller.UserActions.Remove(toRemove);
                    toRemove = null;
                }
            }

            GUILayout.Space(2);
            CalculateListItemsHeight();

            if (reorderableList == null || reorderableList.count != controller.UserActions.Count)
            {
                
                reorderableList = new ReorderableList(controller.UserActions, null, true, false, true, false);
                
                reorderableList.drawElementCallback = DrawUserClickInfo;
                reorderableList.elementHeightCallback = index => heights[index];
                reorderableList.drawElementBackgroundCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    if (Event.current.type == EventType.Layout || index == -1)
                    {
                        return;
                    }

                    Color c = GUI.backgroundColor;

                    var userAction = (UserActionInfo)reorderableList.list[index];

                    if (userAction.SelectedAssertation == null || userAction.SelectedAssertation.methodInfo == null)
                    {
                        Debug.LogError(String.Format("UITestFlowRecorderWindow: SELECTED ASSERTATION OR ITS METHOD INFO IS NULL FOR ELEMENT #{0}", index));

                        toRemove = userAction;

                        return;
                    }

                    var declaringType = userAction.SelectedAssertation.methodInfo.DeclaringType.Name;

                    switch (declaringType)
                    {
                        case "Wait":
                        case "AsyncWait":
                            GUI.backgroundColor = new Color(0.827f, 0.859f, 0.827f);
                            break;
                        case "Check":
                        case "AsyncCheck":
                            GUI.backgroundColor =new Color(0.859f, 0.851f, 0.827f);
                            break;
                        case "Interact":
                            GUI.backgroundColor = new Color(0.827f, 0.831f, 0.85f);
                            break;
                    }
                    
                    if (isActive)
                    {
                        GUI.backgroundColor *= new Color(0.8f,0.8f,0.8f,1);
                    }

                    rect.y += 1;
                    rect.height = GetHeight(index, isActive) - 2;
                    rect.x = 3;
                    rect.width -= 6;
                    GUI.Box(rect, "");
                    GUI.backgroundColor = c;
                };

                reorderableList.onReorderCallback = (ReorderableList list) =>
                {
                    CalculateListItemsHeight();
                };

                reorderableList.onAddCallback = (ReorderableList list) =>
                {
                    controller.CreateNoGameobjectAction();
                };
            }

            m_NonDragTargetIndices = (List<int>)typeof(UnityEditorInternal.ReorderableList).GetField("m_NonDragTargetIndices", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(reorderableList);

            reorderableList.DoLayoutList();

            Repaint();
        }

        private float GetHeight(int index, bool isActive)
        {
            if (isActive)
                return heights[index];

            if (m_NonDragTargetIndices != null && m_NonDragTargetIndices.Count > 0)
            {
                int elementIndex = m_NonDragTargetIndices[index];

                if (elementIndex == -1)
                    return heights[index];

                return heights[elementIndex];
            }

            return heights[index];
        }

        private void DrawHeader()
        {
            const int panelHeight = 52;
            Rect fullRect = new Rect(0, 0, position.width, panelHeight);
            EditorGUI.DrawRect(fullRect, new Color(0.85f, 0.85f, 0.85f));
            EditorGUI.DrawRect(new Rect(0, panelHeight, position.width, 1), Color.black);

            GUILayout.Space(3);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(controller.IsRecording ? "Stop\nRecord" : "Start\nRecord", GUILayout.Width(70),
                GUILayout.Height(panelHeight - 5)))
            {
                SetFocusToGameWindow();
                if (!EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = true;
                }
                controller.StartOrStopRecording();
            }

            EditorGUI.BeginDisabledGroup(!controller.IsRecording);
            Color oldColor = GUI.color;
            GUI.color = new Color(0, 0.5f, 0);
            if (GUILayout.Button(controller.IsAssertationMode ? "Cancel\nCheck\nAlt+Ctrl+C" : "Check\nAlt+Ctrl+C", GUILayout.Width(71),
                GUILayout.Height(panelHeight - 5)))
            {
                controller.ChagneAssertation();
                SetFocusToGameWindow();
            }
            GUI.color = oldColor;
            EditorGUI.EndDisabledGroup();
            
            EditorGUI.BeginDisabledGroup(!controller.IsAssertationMode);
            if (GUILayout.Button(controller.IsForceRaycastEnabled ? "Disable\nforce raycast" : "Enable\nforce raycast", GUILayout.Width(90),
                GUILayout.Height(panelHeight - 5)))
            {
                controller.EnableOrDisableForceRaycast();
                SetFocusToGameWindow();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
            if (GUILayout.Button(controller.IsTimeScaleZero ? "Pause mode\nenabled" : "Pause mode\ndisabled", GUILayout.Width(90),
                GUILayout.Height(panelHeight - 5)))
            {
                controller.EnableOrDisableTimescalePause();
                SetFocusToGameWindow();
            }
            EditorGUI.EndDisabledGroup();
            
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clean", GUILayout.Width(70), GUILayout.Height(panelHeight - 5)))
            {
                controller.Clean();
            }
            
//                        EditorGUI.BeginDisabledGroup(isAssertationMode);
//                    
//                            isClickMode = isClickMode || isAssertationMode;
//                            if (GUILayout.Button(isClickMode ? "click mode" : "drag mode", GUILayout.Width(100)))
//                            {
//                                isClickMode = !isClickMode || isAssertationMode;
//                            }
//                        EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}