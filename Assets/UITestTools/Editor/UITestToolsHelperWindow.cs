using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace PlayQ.UITestTools
{
    public class UITestToolsHelperWindow : EditorWindow
    {
        [SerializeField] private UserFlowModel userFlowModel;
        private List<string> generatedCodes;
        private List<string> generatedLabels;
        private GameObject selectedGameObject;
        private float maxWidthForAssertationName;
        IEnumerable<KeyValuePair<GameObject, string>> audioClipNameToGameObject;

        private UITestToolWindowFooter footer;
        
        [MenuItem("Window/UI Test Tools/Helper")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            UITestToolsHelperWindow window = (UITestToolsHelperWindow) GetWindow(typeof(UITestToolsHelperWindow), false, "Test Helper");
            window.Show();
        }

        [MenuItem("Window/UI Test Tools/Game Event Importer")]
        static void GameEventImporter()
        {
            // Get existing open window or if none, make a new one:
            GetWindow(typeof(GameEventImporter), false, "Game Event Importer").Show();
        }

        [MenuItem("GameObject/Copy Path", false, 0)]
        private static void CopyPath()
        {
            EditorGUIUtility.systemCopyBuffer = UITestUtils.GetGameObjectFullPath((GameObject)Selection.activeObject);
        }

        [MenuItem("Window/UI Test Tools/Open Screenshots")]
        static void OpenScreenshots()
        {
            if (Directory.Exists(Application.persistentDataPath + Path.DirectorySeparatorChar + "Screenshots" +
                                 Path.DirectorySeparatorChar))
            {
                EditorUtility.RevealInFinder(Application.persistentDataPath + Path.DirectorySeparatorChar + "Screenshots" + Path.DirectorySeparatorChar);                
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Screenshot folder doesn't exist yet.", "Ok");
            }
        }
        
        [MenuItem("Window/UI Test Tools/Open FPS metrics")]
        static void OpenFPSMetrics()
        {
            if (Directory.Exists(Interact.SaveFPSClass.FPSMettricsFolder))
            {
                EditorUtility.RevealInFinder(Interact.SaveFPSClass.FPSMettricsFolder);                
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "FPS metrics folder doesn't exist yet.", "Ok");
            }
        }

        private void OnEnable()
        {
            footer = new UITestToolWindowFooter();
            Selection.selectionChanged += Repaint;
            userFlowModel = new UserFlowModel();
            generatedCodes = new List<string>();
            generatedLabels = new List<string>();
            userFlowModel.FetchAssertationMethods();
        }

        private Vector2 scrollPosition;
        void OnGUI()
        {
            EditorGUILayout.Space();
            if (Selection.gameObjects.Length == 0)
            {
                EditorGUILayout.LabelField("No game objects selected", EditorStyles.boldLabel);
            }
            if (Selection.gameObjects.Length > 1)
            {
                EditorGUILayout.LabelField("Muliple game objects selection not supported.", EditorStyles.boldLabel);
            }
            GameObject go = Selection.gameObjects.Length == 1 ? Selection.gameObjects[0] : null;
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            if (go)
            {
                EditorGUILayout.LabelField("Selected object: ", go.name, EditorStyles.boldLabel);
                EditorGUILayout.Space();

                if (go != selectedGameObject)
                {
                    var newAction = userFlowModel.HandleGameObject(go);
                    selectedGameObject = go;
                    GenerateText(newAction);
                }
               
                DrawUserClickInfo();
            }            
            if (GUILayout.Button("Update Assertations", GUILayout.ExpandWidth(true)))
            {
                selectedGameObject = null;
            }
            DrawPlayingSounds();
            EditorGUILayout.EndScrollView();
            GUILayout.Space(UITestToolWindowFooter.HEIGHT + 5);
            footer.Draw(position);
        }

        private void DrawPlayingSounds()
        {
            if (GUILayout.Button("Find playing sounds", GUILayout.ExpandWidth(true)))
            {
                audioClipNameToGameObject = FindObjectsOfType<AudioSource>()
                    .Where(source => source.isPlaying && source.clip)
                    .Select(source =>
                    {
                        var kvp = new KeyValuePair<GameObject, string>(
                            source.gameObject,
                            source.clip.name);
                        return kvp;
                    }).ToList();
            }

            if (audioClipNameToGameObject != null)
            {
                GUILayout.Space(5);
                var indent = GetContentLenght(" GameObject path:");
                foreach (var kvp in audioClipNameToGameObject)
                {
                    
                    var firstRect = EditorGUILayout.BeginHorizontal();
                    
                    GUI.Box(new Rect(firstRect.x, firstRect.y - 3,
                        firstRect.width, firstRect.height * 2 + 9), "");
                    
                    EditorGUILayout.LabelField(" Audio name:");
                    firstRect.x = indent + 5;
                    firstRect.width = firstRect.width - 80 - indent;
                    
                    EditorGUI.TextField(firstRect, kvp.Value);
                    
                    if (GUILayout.Button("Copy", GUILayout.Width(60)))
                    {
                        EditorGUIUtility.systemCopyBuffer = kvp.Value;
                    }
                    GUILayout.Space(5);
                    EditorGUILayout.EndHorizontal();
                    
                    var secondRect = EditorGUILayout.BeginHorizontal();
                    secondRect.x = indent + 5;
                    secondRect.width = secondRect.width - 80 - indent;
                    if (kvp.Key != null)
                    {
                        EditorGUILayout.LabelField(" GameObject path:");
                        EditorGUI.TextField(secondRect, UITestUtils.GetGameObjectFullPath(kvp.Key));    
                    }
                    
                    GUILayout.FlexibleSpace();  
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = kvp.Key;
                    }
                    GUILayout.Space(5);
                    EditorGUILayout.EndHorizontal();
                    
                    GUILayout.Space(5);
                }
            }
        }
        
        
        private float GetContentLenght(string s)
        {
            GUIContent content = new GUIContent(s);
            GUIStyle style = GUI.skin.label;
            style.alignment = TextAnchor.MiddleCenter;
            Vector2 size = style.CalcSize(content);
            return size.x;
        }


        private void GenerateText(UserActionInfo userAction)
        {
            generatedLabels.Clear();
            generatedCodes.Clear();
            
            generatedLabels.Add("GameObject Path: ");
            generatedCodes.Add(UITestUtils.GetGameObjectFullPath(selectedGameObject));

            maxWidthForAssertationName = 0;

            WaitVariablesContainer.SilentMode = true;
            for (int i = 0; i < userAction.AvailableAssertations.Count; i++)
            {
                userAction.SelectedIndex = i;
                try
                {
                    var result = userAction.GenerateCode();
                    if (!string.IsNullOrEmpty(result))
                    {
                        generatedCodes.Add(result);
                    }
                    else
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
                
                var label = userAction.SelectedAssertation.AssertationMethodDescription + ": ";
                label = label.Replace("/", " ");
                generatedLabels.Add(label);
                var currentWidth = GetContentLenght(label);
                if (maxWidthForAssertationName < currentWidth)
                {
                    maxWidthForAssertationName = currentWidth;
                }   
            } 
            WaitVariablesContainer.SilentMode = false;
        }
        
         private void DrawUserClickInfo()
         {   
             var oldValue = EditorGUIUtility.labelWidth;
             EditorGUIUtility.labelWidth = maxWidthForAssertationName;
             
             for(var i = 0; i < generatedLabels.Count; i++)
             {
                 var rect = EditorGUILayout.BeginHorizontal();
                 EditorGUILayout.LabelField(generatedLabels[i]);
                 rect.x = maxWidthForAssertationName;
                 rect.width = rect.width - 65 - maxWidthForAssertationName;  
                 EditorGUI.TextField(rect, generatedCodes[i]);
                 GUILayout.FlexibleSpace();
                 if (GUILayout.Button("Copy", GUILayout.Width(60)))
                 {
                     EditorGUIUtility.systemCopyBuffer = generatedCodes[i];
                 }
                 EditorGUILayout.EndHorizontal();
             }
             EditorGUIUtility.labelWidth = oldValue;
         }
    }
}