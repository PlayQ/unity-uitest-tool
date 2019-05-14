#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public class UITestToolWindowFooter
    {
        public const int HEIGHT = 22;
        private void DrawStatusBar(Rect position, Color color, string labelText)
        {
            Rect window = position;

            const int panelHeight = 45;

            Rect panel = window;

            panel.y = panel.height - panelHeight;

            panel.height = panelHeight;

            panel.x = 0;

            const int labelHeight = 20;

            window = new Rect(0, panel.y + labelHeight, panel.width, labelHeight);

            const int statusBarOffset = 2;

            Rect statusBarRect = new Rect(statusBarOffset, window.y, window.width - 2 * statusBarOffset, HEIGHT);

            GUI.backgroundColor = color;

            GUI.Box(statusBarRect, "", EditorStyles.helpBox);

            GUI.Label(new Rect(0, statusBarRect.y + 3, statusBarRect.width - 5, 15),
                UITestToolsVersionInfo.StringVersion,
                new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight });
            
            GUI.Label(new Rect(5, statusBarRect.y + 3, statusBarRect.width, 15),
                labelText,
                new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft });

            GUI.backgroundColor = Color.white;
        }

        public void Draw(Rect position, bool isDirty = false)
        {
            string label;
            Color color;
            if (isDirty)
            {
                color = Color.red;
#if UNITY_EDITOR_OSX
                label = "Press Cmd+S to serialize the asset to disk";
#else
                label = "Press Ctrl+S to serialize the asset to disk";
#endif
            }
            else
            {
                color = Color.white;
                label = "Status OK.";
            }
            DrawStatusBar(position, color, label);
        }

    }
}
#endif