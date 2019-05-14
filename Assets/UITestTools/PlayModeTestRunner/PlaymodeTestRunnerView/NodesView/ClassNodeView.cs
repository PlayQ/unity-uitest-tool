#if UNITY_EDITOR
using Tests.Nodes;
using UnityEditor;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public class RootView : BaseNodeView
    {
        private ClassNode classNode;
        public RootView(ClassNode node, SelectedNode selectedNode) 
            : base(node, selectedNode)
        {
            classNode = node;
        }

        public override bool Draw()
        {
            bool isDirty = false;
            EditorGUILayout.BeginVertical();

            foreach (var childNode in classNode.Children)
            {
                isDirty |= childNode.View.Draw();
            }
                
            EditorGUILayout.EndVertical();
            
            return isDirty;
        }
    }
    public class ClassNodeView : BaseNodeView
    {
        private const int CLASS_MARGIN = 3;
        private const int PARENT_MARGIN = 1;
        
        private GUIStyle offsetStyle;
        private ClassNode classNode;
        private PlayModeTestRunner.PlayingMethodNode currentPlayingTest;
        
        private void OpenSource()
        {
            if (!string.IsNullOrEmpty(classNode.FilePath))
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>
                    (classNode.FilePath);
                if (asset != null)
                {
                    AssetDatabase.OpenAsset(asset);
                }
            }
        }

        private void RunClass()
        {
            foreach (var method in classNode.GetChildrenOfType<MethodNode>())
            {
                method.ResetTestState();
            }

            PlayModeTestRunner.RunTestByDoubleClick(
                PlayModeTestRunner.SpecificTestType.Class,
                classNode);   
        }

        private bool drawContextMenu;
        private void PreDraw()
        {
            if (drawContextMenu)
            {
                drawContextMenu = false;
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Open source"), false, OpenSource);
                menu.AddItem(new GUIContent("Copy Name"), false, CopyName);
                menu.AddItem(new GUIContent("Run"), false, RunClass);
                menu.ShowAsContext();
            }
        }
        
        public override bool Draw()
        {
            PreDraw();
            
            if (classNode.IsHided)
            {
                return false;
            }
            
            GUILayout.Space(CLASS_MARGIN);

            Rect rect = EditorGUILayout.BeginHorizontal();
            
            if (!IsNodeOpened && currentPlayingTest.Node != null && 
                currentPlayingTest.Node.IsChildOf(classNode))
            {
                DrawCurrentTestBackground(rect);
            }
            else
            {
                DrawBackgroundIfSelected(rect);
            }

 
            var isOpenedOld = IsNodeOpened;
            var isOpenedNew = DrawFoldout();
            if (IsOpenFiltered.HasValue)
            {
                IsOpenFiltered = isOpenedNew;
            }
            else
            {
                classNode.SetOpened(isOpenedNew);
            }

            HandleKeyboardEvent(RunClass);

            var prevSelection = classNode.IsSelected;

            DrawTestLine(rect,
                () => { selectedNode.UpdateSelectedNode(classNode); },
                () =>
                {
                    foreach (var method in classNode.GetChildrenOfType<MethodNode>())
                    {
                        method.ResetTestState();
                    }

                    PlayModeTestRunner.RunTestByDoubleClick(
                        PlayModeTestRunner.SpecificTestType.Class,
                        classNode);
                },
                () =>
                {
                    selectedNode.UpdateSelectedNode(classNode);
                    drawContextMenu = true;
                },
                string.Empty);

            var isDirty = classNode.IsOpened != isOpenedOld;
            isDirty |= prevSelection != classNode.IsSelected;
            
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (IsNodeOpened)
            {
                GUILayout.Space(PARENT_MARGIN);
                EditorGUILayout.BeginVertical(offsetStyle);

                foreach (var childNode in classNode.Children)
                {
                    isDirty |= childNode.View.Draw();
                }

                
                EditorGUILayout.EndVertical();
            }
            return isDirty;
        }

        public ClassNodeView(ClassNode node, SelectedNode selectedNode,
            PlayModeTestRunner.PlayingMethodNode currentPlayingTest) 
            : base(node, selectedNode)
        {
            this.currentPlayingTest = currentPlayingTest;
            classNode = node;
            offsetStyle = new GUIStyle {margin = {left = 14, right = 0}};
        }
    }

}
#endif