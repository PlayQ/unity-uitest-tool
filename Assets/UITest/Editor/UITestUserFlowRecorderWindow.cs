using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace PlayQ.UITestTools
{
    public class UITestUserFlowRecorderWindow : EditorWindow
    {
        private Texture normalIconTexture;
        private GUIContent normalIconContent;
        private Texture grayscaleIconTexture;
        private GUIContent grayscaleIconContent;

        private UITestUserFlowRecorderController controller;

        private Vector2 scrollPosition;
        private Vector2 codeScrollPosition;

        private ReorderableList reorderableList;
        private List<float> heights = new List<float>();



        [UnityEditor.MenuItem("Window/UI Test Tools/Flow recorder %t")]
        static void Init()
        {
            UITestUserFlowRecorderWindow window =
                (UITestUserFlowRecorderWindow) GetWindow(typeof(UITestUserFlowRecorderWindow), false, "Recorder");
            window.Show();
        }

        private void OnEnable()
        {
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
        
        private void CreateFakeActions()
        {
            var bigParams = new List<object> {5, 5, false, 120f};
            var smallParams = new List<object> { };
            for (int i = 0; i < 5; i++)
            {
                controller.UserActions.Add(new UserActionInfo
                {
                    MethodsInfoExtended = new List<MethodInfoExtended>
                    {
                        new MethodInfoExtended
                        {
                            PathIsPresent = true,
                            GameObjectFullPath = "FAKE " + i,
                            AssertationMethodDescription = (i % 2 == 0 ? "big " : "small ") + i,
                            ParametersValues = i % 2 == 0 ? bigParams : smallParams
                        },
                        new MethodInfoExtended
                        {
                            PathIsPresent = true,
                            GameObjectFullPath = "FAKE " + i,
                            AssertationMethodDescription = (i % 2 == 0 ? "small " : "big ") + i,
                            ParametersValues = i % 2 == 0 ? smallParams : bigParams
                        }
                    }
                });
            }
        }

        private void OnDisable()
        {
            EditorApplication.update -= DoUpdate;
        }

        private bool switchedByHotKey;
        private void DoUpdate()
        {
            if (controller.IsRecording && Input.GetKey(KeyCode.C) && Input.GetKey(KeyCode.LeftAlt) &&
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
        }

        void OnGUI()
        {
            if (!switchedByHotKey && controller.IsRecording&&Event.current != null &&Event.current.type == EventType.KeyDown &&Event.current.alt && (Event.current.command ||Event.current.command) && Event.current.keyCode==KeyCode.C)
            {
                controller.ChagneAssertation();
                Repaint();
            }            

            if (controller.IsAssertationMode)
            {
                GUI.backgroundColor = new Color(199f / 255, 255f / 255, 213f / 255);
            }
            
            EditorGUILayout.BeginVertical();
            DrawHeader();
//            color = EditorGUILayout.ColorField(color);

            if (EditorApplication.isPaused && controller.IsRecording)
            {
                var style = new GUIStyle(GUI.skin.label) {fontStyle = FontStyle.Bold};
                EditorGUILayout.LabelField("Can't record when editor pause enabled!", style);
                EditorGUILayout.LabelField("Use 'Pause mode' button instead.", style);
            }
            
            DrawListOfUserAction();

            if (controller.UserActions.Count == 0)
            {
                GUILayout.FlexibleSpace();   
            }
            DrawBottomPanel();
            EditorGUILayout.EndVertical(); 
        }


        private bool collapsed;
        private bool isGenerateDebugStrings;
        private void DrawBottomPanel()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();
            float width = position.width - 15;
            GUI.enabled = controller.UserActions.Count > 0;
            if (GUILayout.Button("Generate Code and Copy", GUILayout.Height(30), GUILayout.Width(width-40-40-8)))
            {
                controller.GenerateCode(isGenerateDebugStrings);
            }
            bool codeGenerated = !String.IsNullOrEmpty(controller.GeneratedCode);
            GUI.enabled = codeGenerated;
            if (GUILayout.Button("Copy", GUILayout.Height(30),GUILayout.Width(40)))
            {
                controller.CopyToBuffer();
            }
            if (!codeGenerated)
            {
                collapsed = false;                
            }
            if (GUILayout.Button(collapsed ? "-" : "+", GUILayout.Height(30), GUILayout.Width(40)))
            {
                collapsed = !collapsed;
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            GUI.enabled = controller.UserActions.Count > 0;
            isGenerateDebugStrings = EditorGUILayout.Toggle("Generate debug logs:", isGenerateDebugStrings);
            GUI.enabled = true;
            if (collapsed)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                codeScrollPosition = EditorGUILayout.BeginScrollView(codeScrollPosition,
                    GUILayout.Width(width-6),
                    GUILayout.Height(500));
                controller.GeneratedCode = EditorGUILayout.TextArea(controller.GeneratedCode, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }
        
        void AddMenuItem(GenericMenu menu, string menuPath, UserActionInfo action, int index)
        {
            menu.AddItem(new GUIContent(menuPath), false, () =>
            {
                action.SelectedAssertationIndex = index;
            });
        }
        
        int indexToCopy = -1;
        UserActionInfo toRemove;
        private void DrawUserClickInfo(Rect rect, int index, bool isActive, bool isFocused)
        {
            float topLineHeight = EditorGUIUtility.singleLineHeight;
            var rectToDrawWith = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            
            var userAction = (UserActionInfo)reorderableList.list[index];
            var parametersType = userAction.SelectedAssertation.ParametersValuesToSave;

            var additionalFields = userAction.SelectedAssertation.PathIsPresent ? 4 : 3;
            var shiftY = (heights[index] - EditorGUIUtility.singleLineHeight) / (parametersType.Count + additionalFields);
            rectToDrawWith.y += EditorGUIUtility.singleLineHeight * 0.5f;

            rectToDrawWith.width -= 64;

            rectToDrawWith.width -= 62;
            rectToDrawWith.x += 30;

            var style = new GUIStyle(GUI.skin.label) {fontStyle = FontStyle.Bold};
            EditorGUI.LabelField(new Rect(rectToDrawWith.x - 30, rectToDrawWith.y, 30, rectToDrawWith.height), (index+1).ToString(), style);

            var labelForButton = userAction.SelectedAssertation.AssertationMethodDescription;
            labelForButton = labelForButton.Replace('/', ' ');
            if (GUI.Button(rectToDrawWith, labelForButton))
            {
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < userAction.MethodsInfoExtended.Count; i++)
                {
                    AddMenuItem(menu, userAction.MethodsInfoExtended[i].AssertationMethodDescription, userAction, i);    
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
            rectToDrawWith.width += 64;

            rectToDrawWith.y += shiftY;
            userAction.Description = EditorGUI.TextField(rectToDrawWith, "Description: ", userAction.Description);            
            
            if (userAction.SelectedAssertation.PathIsPresent)
            {
                rectToDrawWith.y += shiftY;
                userAction.SelectedAssertation.GameObjectFullPath = EditorGUI.TextField(rectToDrawWith, "Path: ", userAction.SelectedAssertation.GameObjectFullPath);
            }


            foreach (var paramInfo in parametersType)
            {
                rectToDrawWith.y += shiftY;

                object paramValue = null;
                try
                {
                    paramValue = paramInfo.Get();
                    if (paramValue == null)
                    {
                        Debug.LogError("values is null for: type " + paramInfo.Type + " and string value " + paramInfo.Value);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                    continue;
                }

                switch (paramInfo.Type)
                {
                    case "System.Int32":
                        paramInfo.Value = EditorGUI.IntField(rectToDrawWith, paramInfo.Name, (int)paramValue).ToString();
                        
                        break;
                    case "System.Single":
                        paramInfo.Value = EditorGUI.FloatField(rectToDrawWith, paramInfo.Name, (float)paramValue).ToString();
                        break;
                    case "System.Boolean":
                        paramInfo.Value = EditorGUI.Toggle(rectToDrawWith, paramInfo.Name, (bool)paramValue) ? "true" : "false";
                        break;
                    case "System.String":
                        paramInfo.Value = EditorGUI.TextField(rectToDrawWith, paramInfo.Name, paramValue.ToString());
                       break;
                    default:
                        if (paramInfo.isEnum)
                        {
                            var enumValue = (Enum) paramValue;
                            paramInfo.Value = EditorGUI.EnumPopup(rectToDrawWith, enumValue.GetType().Name, enumValue).ToString();
                        }
                        else
                        {
                            paramInfo.Value = EditorGUI.TextField(rectToDrawWith, paramInfo.Name, paramValue.ToString());    
                        }
                        break;    
                }
            }
            
        }

        private void CalculateListItemsHeight()
        {
            heights.Clear();
            foreach (var action in controller.UserActions)
            {
                var additionalFieldsAmount = action.SelectedAssertation.PathIsPresent ? 4 : 3;
                var height = EditorGUIUtility.singleLineHeight * 1.2f *
                             (action.SelectedAssertation.ParametersValuesToSave.Count + additionalFieldsAmount)
                             + EditorGUIUtility.singleLineHeight;
                heights.Add(height);    
            }   
        }

        private void DrawListOfUserAction()
        {
            if (Event.current.type != EventType.Repaint)
            {
                if (indexToCopy != -1)
                {
                    controller.UserActions.Insert(indexToCopy + 1,
                        controller.UserActions[indexToCopy].Clone());
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
                reorderableList.elementHeightCallback = index =>
                {
                    return heights[index];
                };
                reorderableList.drawElementBackgroundCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    if (Event.current.type == EventType.Layout || index == -1)
                    {
                        return;
                    }
                    Color c = GUI.backgroundColor;
                    if (isActive)
                    {
                        GUI.backgroundColor = new Color(204f/255f,204f/255f,204f/255f, 1);
                    }
                    rect.y += 1;
                    rect.height = heights[index] - 2;
                    rect.x = 3;
                    rect.width -= 6;
                    GUI.Box(rect, "");
                    GUI.backgroundColor = c;
                };

                reorderableList.onReorderCallback = (ReorderableList list) =>
                {
                    Debug.Log("----------------- REORDER");
                    CalculateListItemsHeight();
                };

                reorderableList.onAddCallback = (ReorderableList list) =>
                {
                    controller.CreateNoGameobjectAction();
                };
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            reorderableList.DoLayoutList();
            EditorGUILayout.EndScrollView();
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
            if (GUILayout.Button(controller.IsAssertationMode ? "Cancel\nCheck\nAlt+Ctrl+C" : "Check\nAlt+Ctrl+C", GUILayout.Width(70),
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