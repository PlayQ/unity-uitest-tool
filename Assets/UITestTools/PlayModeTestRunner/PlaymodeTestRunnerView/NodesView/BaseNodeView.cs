#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Utilities;
using Tests.Nodes;
using UnityEditor;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public abstract class BaseNodeView
    {
        protected const int METHOD_MARGIN = -1;
        protected const int SPACE_BETWEEN_TOGGLE_AND_BUTTON = 5;
        
        protected static Texture2D successIcon;
        protected static Texture2D failIcon;
        protected static Texture2D ignoreIcon;
        protected static Texture2D defaultIcon;

        private Node node;
        protected SelectedNode selectedNode;
        protected DateTime clickInterval;

        public bool? IsOpenFiltered { get; protected set; }
        protected virtual bool IsNodeOpened
        {
            get
            {
                if (IsOpenFiltered.HasValue)
                {
                    return IsOpenFiltered.Value;
                }

                return node.IsOpened;
            }
        }

        public void SetOpenFiltered(bool? isForecedOpen)
        {
            IsOpenFiltered = isForecedOpen;
        }
        
        
        public BaseNodeView(Node node, SelectedNode selectedNode)
        {
            this.node = node;
            this.selectedNode = selectedNode;
            LoadImgs();
        }
        
        private void LoadImgs()
        {
            if (successIcon == null)
            {
                successIcon = Resources.Load<Texture2D>("passed");
                failIcon = Resources.Load<Texture2D>("failed");
                ignoreIcon = Resources.Load<Texture2D>("ignored");
                defaultIcon = Resources.Load<Texture2D>("normal");
            }
        }
        
        protected Texture2D GetTestIcon(TestState state)
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
        
        protected void DrawCurrentTestBackground(Rect rect)
        {
            DrawBackground(rect, new Color(1, 1, 196f / 255));
        }

        protected void DrawBackground(Rect rect, Color color)
        {
            rect.min = new Vector2(0, rect.min.y);
            Color currentBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUI.Box(rect, GUIContent.none);
            GUI.backgroundColor = currentBackgroundColor;
        }

        protected void DrawBackgroundIfSelected(Rect rect)
        {
            if (selectedNode.IsGivenNodeSelected(node))
            {
                DrawBackground(rect, new Color(0.098f, 0.71f, 0.996f));
            }
        }

        public abstract bool Draw();

        protected void HandleKeyboardEvent(Action OnReturnKeyPress)
        {
            if (selectedNode.IsGivenNodeSelected(node)
                && Event.current.type == EventType.KeyDown)
            {
                if (GUI.GetNameOfFocusedControl() != "")
                {
                    return;
                }
                
                if ((Event.current.control || Event.current.command) &&
                    Event.current.keyCode == KeyCode.C)
                {
                    CopyName();
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

                        if (selectedNode.Node != null 
                            && selectedNode.Node.Parent != null 
                            && !selectedNode.Node.Parent.IsRoot)
                        {
                            Node upperChild = null;
                            foreach (var child in selectedNode.Node.Parent.Children)
                            {
                                if (!child.IsHided)
                                {
                                    if (child != selectedNode.Node)
                                    {
                                        upperChild = child;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }       
                            }
                            
                            if (upperChild != null)
                            {
                                upperChild = GetLastOpenChild(upperChild);
                                selectedNode.UpdateSelectedNode(upperChild);
                                
                            }
                            else
                            {
                                selectedNode.UpdateSelectedNode(selectedNode.Node.Parent);
                            }
                        }

                        Event.current.Use();

                        return;

                    case KeyCode.DownArrow:

                        if (selectedNode.Node != null && selectedNode.Node.ChildrenCount > 0 && 
                            IsNodeOpened)
                        {
                            Node upperChild = null;
                            foreach (var child in selectedNode.Node.Children)
                            {
                                if (!child.IsHided)
                                {
                                    selectedNode.UpdateSelectedNode(child);
                                    Event.current.Use();
                                    return;
                                }       
                            }
                        }

                        TrySelectElementBelow(node);

                        Event.current.Use();

                        return;

                    case KeyCode.RightArrow:

                        if (selectedNode.Node != null && selectedNode.Node.ChildrenCount > 0)
                        {
                            if (!IsNodeOpened)
                            {
                                if (IsOpenFiltered.HasValue)
                                {
                                    IsOpenFiltered = true;  
                                } 
                                node.SetOpened(true);
                                Event.current.Use();
                                return;
                            }

                            selectedNode.UpdateSelectedNode(selectedNode.Node.ChildAt(0));
                        }

                        Event.current.Use();

                        return;

                    case KeyCode.LeftArrow:

                        if (IsNodeOpened)
                        {
                            //IsOpenFiltered = false;
                            if (IsOpenFiltered.HasValue)
                            {
                                IsOpenFiltered = false;  
                            } 
                            node.SetOpened(false);
                            Event.current.Use();
                            return;
                        }

                        if (selectedNode.Node != null && selectedNode.Node.Parent != null && !selectedNode.Node.Parent.IsRoot)
                        {
                            selectedNode.UpdateSelectedNode(selectedNode.Node.Parent);
                        }

                        Event.current.Use();
                        return;
                }
            }
        }

        
        private void TrySelectElementBelow(Node node)
        {
            if (node.Parent != null && !node.Parent.IsRoot)
            {
                foreach (var child in node.Parent.Children)
                {
                    if (!child.IsHided && child.Index > node.Index)
                    {
                        selectedNode.UpdateSelectedNode(child);
                        Event.current.Use();
                        return;
                    }       
                }
                
                TrySelectElementBelow(node.Parent);
            }
        }
        
        private Node GetLastOpenChild(Node node)
        {
            if (node.View.IsNodeOpened)
            {
                var openedChildren = node.Children.Where(c => !c.IsHided).ToList();
                if (openedChildren.Count > 0)
                {
                    var targetNode = openedChildren.Last();
                    return GetLastOpenChild(targetNode);
                }
            }
            return node;
        }
        
        protected bool DrawFoldout()
        {
            EditorGUIUtility.fieldWidth = 12;    
            return EditorGUILayout.Foldout(IsNodeOpened, "");
        }

        protected void CopyName()
        {
            if (selectedNode.Node != null)
            {
                EditorGUIUtility.systemCopyBuffer = selectedNode.Node.FullName;
            }
        }

        protected void DrawTestLine(Rect rect,
            Action OnClick,
            Action OnDoubleClick,
            Action OnRightClick,
            string namePostfix)
        {    
            if (OnRightClick != null)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                {
                    Vector2 mousePos = Event.current.mousePosition;
                    if (rect.Contains(mousePos))
                    {
                        OnRightClick();
                    }
                }
            }
            
            Color oldColor = GUI.backgroundColor;
            if (node.IsSemiSelected && !node.IsSelected)
            {
                GUI.backgroundColor = new Color(174f / 255f, 208f / 255f, 1, 1);
            }

            GUI.SetNextControlName("TestItem");

            var fullSelected = 
                GUILayout.Toggle(node.IsSelected, "",
                    GUILayout.Width(10 + SPACE_BETWEEN_TOGGLE_AND_BUTTON));
            node.SetSelected(fullSelected);
            

            GUI.backgroundColor = oldColor;

            var content = new GUIContent(node.Name + namePostfix,
                GetTestIcon(node.State));

            var guiStyle = new GUIStyle(EditorStyles.label);

            GUILayout.Space(-SPACE_BETWEEN_TOGGLE_AND_BUTTON);

            var click = GUILayout.Button(content,
                guiStyle,
                GUILayout.ExpandWidth(true),
                GUILayout.Width(EditorStyles.label.CalcSize(content).x + 10));

            if (click)
            {
                GUI.FocusControl(null);
                DateTime currentClickTime = DateTime.Now;
                if ((currentClickTime - clickInterval).TotalMilliseconds < 300 && !Application.isPlaying)
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

                clickInterval = currentClickTime;
            }
        }
    }

}

#endif