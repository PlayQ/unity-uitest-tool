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
        private UserActionInfo userAction;
        private List<string> generatedCodes;
        private float maxWidthForAssertationName;
        private float maxWidthForAudioName;
        IEnumerable<KeyValuePair<GameObject, string>> audioClipNameToGameObject;

        [MenuItem("Window/UI Test Tools/Helper")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            UITestToolsHelperWindow window = (UITestToolsHelperWindow) GetWindow(typeof(UITestToolsHelperWindow), false, "Test Helper");
            window.Show();
        }
        [MenuItem("GameObject/Copy path", false, 0)]
        private static void CopyPath()
        {
            EditorGUIUtility.systemCopyBuffer = UITestUtils.GetGameObjectFullPath((GameObject)Selection.activeObject);
        }
//        [MenuItem("GameObject/kek/Copy path", true)]
//        private static bool CopyPathValidation()
//        {
//            return Selection.activeObject && Selection.activeObject.GetType() == typeof(GameObject);
//        }

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
            Selection.selectionChanged += Repaint;
            userFlowModel = new UserFlowModel();
            generatedCodes = new List<string>();
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
                var newAction = userFlowModel.HandleGameObject(go);
                if (userAction == null || userAction.MethodsInfoExtended == null ||
                    userAction.MethodsInfoExtended.Count == 0)
                {
                    generatedCodes.Clear();
                    userAction = newAction;
                }
                if (userAction.SelectedAssertation.GameObjectFullPath !=
                    newAction.SelectedAssertation.GameObjectFullPath)
                {
                    generatedCodes.Clear();
                    userAction = newAction;
                }
                DrawUserClickInfo(userAction);
            }            
            DrawPlayingSounds();
            EditorGUILayout.EndScrollView();
            
        }

        private void DrawPlayingSounds()
        {
            if (GUILayout.Button("Find playing sounds", GUILayout.ExpandWidth(true)))
            {
                maxWidthForAudioName = 0;
                audioClipNameToGameObject = FindObjectsOfType<AudioSource>()
                    .Where(source => source.isPlaying)
                    .Select(source =>
                    {
                        var currentWidth = GetContentLenght(source.clip.name);
                        if (maxWidthForAudioName < currentWidth)
                        {
                            maxWidthForAudioName = currentWidth;
                        } 
                        var kvp = new KeyValuePair<GameObject, string>(
                            source.gameObject,
                            source.clip.name);
                        return kvp;
                    }).ToList();
            }

            if (audioClipNameToGameObject != null)
            {
                foreach (var kvp in audioClipNameToGameObject)
                {
                    var rect = EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(kvp.Value);
                    rect.x = maxWidthForAudioName + 5;
                    rect.width = rect.width - 130 - maxWidthForAudioName;
                    if (kvp.Key != null)
                    {
                        EditorGUI.TextField(rect, UITestUtils.GetGameObjectFullPath(kvp.Key));    
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Copy", GUILayout.Width(60)))
                    {
                        EditorGUIUtility.systemCopyBuffer = kvp.Value;
                    }
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = kvp.Key;
                    }
                    EditorGUILayout.EndHorizontal();
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
        
         private void DrawUserClickInfo(UserActionInfo userAction)
         {
             if (userAction.MethodsInfoExtended.Count == 0)
             {
                 return;
             }
            
             EditorGUILayout.BeginHorizontal();
             EditorGUILayout.TextField("Path", userAction.MethodsInfoExtended[0].GameObjectFullPath);
             if (GUILayout.Button("Copy", GUILayout.Width(60)))
             {
                 EditorGUIUtility.systemCopyBuffer = userAction.MethodsInfoExtended[0].GameObjectFullPath;
             }
             
             EditorGUILayout.EndHorizontal();

             if (generatedCodes.Count == 0)
             {
                 if (userAction.MethodsInfoExtended.Any())
                 {
                     var label = userAction.MethodsInfoExtended[0].AssertationMethodDescription + ": ";
                     maxWidthForAssertationName = GetContentLenght(label);
                 }
                 foreach(var assertation in userAction.MethodsInfoExtended)
                 {
                     generatedCodes.Add(assertation.GenerateMethodCode());
                     var label = assertation.AssertationMethodDescription + ": ";
                     var currentWidth = GetContentLenght(label);
                     if (maxWidthForAssertationName < currentWidth)
                     {
                         maxWidthForAssertationName = currentWidth;
                     } 
                 }    
             }
             
             for(var i = 0; i < userAction.MethodsInfoExtended.Count; i++)
             {
                 var rect = EditorGUILayout.BeginHorizontal();
                 var assertation = userAction.MethodsInfoExtended[i];
                 EditorGUILayout.LabelField(assertation.AssertationMethodDescription + ": ");
                 rect.x = maxWidthForAssertationName;
                 rect.width = rect.width - 65 - maxWidthForAssertationName;  
                 EditorGUI.TextField(rect, generatedCodes[i]);
                 GUILayout.FlexibleSpace();
                 if (GUILayout.Button("Copy", GUILayout.Width(60)))
                 {
                     EditorGUIUtility.systemCopyBuffer = assertation.GenerateMethodCode();
                 }
                 EditorGUILayout.EndHorizontal();
             } 
        }
    }
}