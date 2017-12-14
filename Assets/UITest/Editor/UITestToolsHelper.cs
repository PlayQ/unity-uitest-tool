using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PlayQ.UITestTools
{
    public class UITestToolsHelper : EditorWindow
    {
        //for click - IPointerDownHandler, IPointerClickHandler, 
        //for drag - IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
        //for Set/AppentText - InputField
        //for Toggle - Toggle

        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/UI Test Tools Helper")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            UITestToolsHelper window = (UITestToolsHelper) EditorWindow.GetWindow(typeof(UITestToolsHelper), false, "UI tool");
            window.Show();
        }

        private void OnEnable()
        {
            Selection.selectionChanged += Repaint;
        }

        void OnGUI()
        {
            EditorGUILayout.Space();
            if (Selection.gameObjects.Length == 0)
            {
                EditorGUILayout.LabelField("No game objects selected", EditorStyles.boldLabel);
                return;
            }
            if (Selection.gameObjects.Length > 1)
            {
                EditorGUILayout.LabelField("Muliple game objects selection not supported.", EditorStyles.boldLabel);
                return;
            }
            GameObject go = Selection.gameObjects[0];

            EditorGUILayout.LabelField("Selected object: ", go.name, EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            string path = "\"" + GetPath(go.transform) + "\"";
            bool isClicable = IsClickable(go);
            bool isText = IsText(go);
            bool isToggle = IsToggle(go);
            bool isRectTransform = go.transform is RectTransform; 

            DrawLabel("Path:", path);

            //Interaction
            EditorGUILayout.Space();


            if (isRectTransform)
            {
                var rectTransform = go.transform as RectTransform;
                Vector3[] worldCoreners = new Vector3[4];
                rectTransform.GetWorldCorners(worldCoreners);
                
                var anyCamera = UITestTools.FindAnyGameObject<Camera>();
                if (anyCamera == null)
                {
                    EditorGUILayout.LabelField("No camera in scene - can't find screen coordinates",
                        EditorStyles.boldLabel);
                }
                else
                {
                    Canvas canvas = null;
                    var root = rectTransform;
                    
                    while (root)
                    {
                        canvas = root.GetComponent<Canvas>();
                        root = root.parent as RectTransform;
                    }

                    if (canvas != null)
                    {
                        if (Camera.main != null)
                        {
                            anyCamera = Camera.main;
                        }
                        if (canvas.worldCamera != null)
                        {
                            anyCamera = canvas.worldCamera;
                        }

                        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                        {
                            anyCamera = null;
                        }
                    
                        var screenCorners = worldCoreners.Select(worldPoint =>
                        {
                            return RectTransformUtility.WorldToScreenPoint(anyCamera, worldPoint);
                        
                        }).ToArray();

                        DrawLabel("Coordinates", "bottom-left point: x=" + screenCorners[0].x +
                                                 " y=" + screenCorners[0].y + " with=" +
                                                 (screenCorners[3].x - screenCorners[0].x) +
                                                 " height=" + (screenCorners[1].y - screenCorners[0].y));       
                    }
                }
            }
            
            if (isClicable)
            {
                DrawLabel("Click:", "Click(" + path + ");");
            }
            if (isText)
            {
                DrawLabel("SetText:", "SetText(" + path + ", \"Text\");");
            }
            DrawLabel("FindObject:", "FindObject(" + path + ");");
            //Wait
            EditorGUILayout.Space();

            DrawLabel("WaitForObject:", "yield return WaitForObject(" + path + ");");
            DrawLabel("WaitForDestroy:", "yield return WaitForDestroy(" + path + ");");
            DrawLabel("WaitForEnable:", "yield return WaitForEnable(" + path + ");");
            DrawLabel("WaitForDisable:", "yield return WaitForDisable(" + path + ");");
            //Assertation
            EditorGUILayout.Space();
            DrawLabel("IsEnable:", "Check.IsEnable(" + path + ");");
            DrawLabel("IsDisable:", "Check.IsDisable(" + path + ");");
            DrawLabel("IsExist:", "Check.IsExist(" + path + ");");
            DrawLabel("IsNotExist:", "Check.IsNotExist(" + path + ");");
            if (isText)
            {
                DrawLabel("IsTextEquals:", "Check.IsTextEquals(" + path + ");");
                DrawLabel("IsTextNotEquals:", "Check.IsTextNotEquals(" + path + ");");
            }
            if (isToggle)
            {
                DrawLabel("Toggle:", "Check.Toggle(" + path + ", \"True\");");
            }
        }

        private static void DrawLabel(string name, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(100));
            EditorGUILayout.TextField(value);
            if (GUILayout.Button("Copy", GUILayout.Width(40)))
            {
                EditorGUIUtility.systemCopyBuffer = value;
            }
            EditorGUILayout.EndHorizontal();
        }

        private bool IsText(GameObject go)
        {
            return go.GetComponent<InputField>() != null;
        }

        private bool IsToggle(GameObject go)
        {
            return go.GetComponent<Toggle>() != null;
        }

        private bool IsClickable(GameObject go)
        {
            if (go.GetComponent<IPointerDownHandler>() != null)
            {
                return true;
            }
            if (go.GetComponent<IPointerClickHandler>() != null)
            {
                return true;
            }
            return false;
        }

        private string GetPath(Transform go)
        {
            string path = go.name;
            if (go.parent != null)
            {
                path = GetPath(go.parent) + "/" + go.name;
            }
            return path;
        }
    }
}