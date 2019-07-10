#if UNITY_EDITOR
using Tests.Nodes;
using UnityEditor;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public class MethodNodeView : BaseNodeView
    {
        private MethodNode methodNode;
        private PlayModeTestRunner.PlayingMethodNode currentPlayingTest;

        private void OpenSource()
        {
            if (!string.IsNullOrEmpty(methodNode.ParentClass.FilePath))
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>
                    (methodNode.ParentClass.FilePath);
                int lineCount = 1;

                if (asset != null)
                {
                    var content = asset.text;

                    var index = content.IndexOf("IEnumerator " + methodNode.Name);
                    for (int i = 0; i <= index; i++)
                    {
                        if (content[i] == '\n')
                        {
                            lineCount++;
                        }
                    }

                    AssetDatabase.OpenAsset(asset, lineCount);
                }
            }
        }

        private bool drawContextMenu;
        private void PreDraw()
        {
            if (drawContextMenu)
            {
                drawContextMenu = false;
                GenericMenu menu = new GenericMenu();
                if (methodNode.TestSettings.TestRailURLs != null)
                {
                    foreach (var testRail in methodNode.TestSettings.TestRailURLs)
                    {
                        menu.AddItem(new GUIContent("Open TestRail " + testRail.Description), false,
                            () => { Application.OpenURL(testRail.URL); });
                    }
                }

                menu.AddItem(new GUIContent("Run"), false, RunTest);
                menu.AddItem(new GUIContent("Open Source"), false, OpenSource);
                menu.AddItem(new GUIContent("Copy Name"), false, CopyName);
                menu.ShowAsContext();
            }
        }
        
        public override bool Draw()
        {
            PreDraw();
            
            if (methodNode.IsHided)
            {
                return false;
            }

            GUILayout.Space(METHOD_MARGIN);

            Rect rect = EditorGUILayout.BeginHorizontal();

            
            if (currentPlayingTest.Node == methodNode)
            {
                DrawCurrentTestBackground(rect);
            }
            else
            {
                DrawBackgroundIfSelected(rect);
            }

            GUILayout.Space(20);

            
            HandleKeyboardEvent(RunTest);

            var prevSelection = methodNode.IsSelected;
            
            DrawTestLine(rect,
                () =>
                {
                    selectedNode.UpdateSelectedNode(methodNode);
                },
                () =>
                {
                    methodNode.ResetTestState();
                    PlayModeTestRunner.RunTestByDoubleClick(
                        PlayModeTestRunner.SpecificTestType.Method,
                        methodNode);
                },
                () =>
                {
                    selectedNode.UpdateSelectedNode(methodNode);
                    drawContextMenu = true;
                },
                NamePostfix());

            bool isDirty = prevSelection != methodNode.IsSelected;

            EditorGUILayout.EndHorizontal();

            return isDirty;
        }

        private void RunTest()
        {
            methodNode.ResetTestState();
            PlayModeTestRunner.RunTestByDoubleClick(
                PlayModeTestRunner.SpecificTestType.Method,
                methodNode);
        }

        private string NamePostfix()
        {
            string namePostfix = "";
            if (methodNode.PassedAmount > 1 &&
                methodNode.FailedAmount == 0)
            {
                namePostfix = " [Passed: " + methodNode.PassedAmount + "]";
            }
            else if (methodNode.PassedAmount == 0 &&
                     methodNode.FailedAmount > 1)
            {
                namePostfix = " [Failed: " + methodNode.FailedAmount + "]";
            }
            else if (methodNode.PassedAmount +
                     methodNode.FailedAmount > 1)
            {
                namePostfix = " [Passed: " + methodNode.PassedAmount + " Failed: " +
                              methodNode.FailedAmount + "]";
            }

            return namePostfix;
        }

        protected override bool IsNodeOpened
        {
            get { return true; }
        }

        public MethodNodeView(MethodNode node, SelectedNode selectedNode,
            PlayModeTestRunner.PlayingMethodNode currentPlayingTest) : base(node, selectedNode)
        {
            methodNode = node;
            this.currentPlayingTest = currentPlayingTest;
        }
    }

}
#endif