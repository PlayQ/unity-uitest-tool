#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace EditorUITools
{
    /// <summary>
    /// UI constant values
    /// </summary>
    public static class EditorUIConstants
    {
        /// <summary>
        /// Space between an element in Editor GUI Layout and layout's border
        /// </summary>
        public const int LAYOUT_PADDING = 3;

    }

    /// <summary>
    /// Draws two sections with a draggable slider between them
    /// </summary>
    [Serializable]
    public class TwoSectionWithSliderDrawer
    {
        //todo make it generic, use it in recorder
        /// <summary>
        /// Draws the empty box by a given rect and color
        /// </summary>
        /// <param name="rect">Box rect</param>
        /// <param name="color">Box color</param>
        private static void DrawEmptyBox(Rect rect, Color color)
        {
            Color currentBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUI.Box(rect, GUIContent.none);
            GUI.backgroundColor = currentBackgroundColor;
        }

        /// <summary>
        /// The upper border of the two sections
        /// </summary>
        [SerializeField]
        private int minBorder;

        /// <summary>
        /// The lower border of the two sections
        /// </summary>
        [SerializeField]
        private int maxBorder;

        /// <summary>
        /// Lower section drawer
        /// </summary>
        [SerializeField]
        private SectionDrawer lowerSection;

        /// <summary>
        /// Upper section drawer
        /// </summary>
        [SerializeField]
        private SectionDrawer upperSection;

        /// <summary>
        /// Draggable slider drawer
        /// </summary>
        [SerializeField]
        private SliderDrawer sliderDrawer;

        /// <summary>
        /// Initialize drawer
        /// </summary>
        /// <param name="upperSectionMinSize">Upper section's minimum size</param>
        /// <param name="upperSectionGUIStyle">Upper section's GUI style</param>
        /// <param name="lowerSectionMinSize">Lower section's minimum size</param>
        /// <param name="lowerSectionGUIStyle">Lower section's GUI style</param>
        /// <param name="sliderPos">Slider's default position</param>
        public TwoSectionWithSliderDrawer(int upperSectionMinSize,
                                             int lowerSectionMinSize,
                                             int sliderPos)
        {
            upperSection = new SectionDrawer(upperSectionMinSize, SectionDrawer.PositionTab.Upper);
            lowerSection = new SectionDrawer(lowerSectionMinSize, SectionDrawer.PositionTab.Lower);
            sliderDrawer = new SliderDrawer();

            sliderDrawer.SliderPos = sliderPos;
        }

        /// <summary>
        /// Draw the sections and a draggable slider
        /// </summary>
        /// <returns>Should we call Repaint() or not</returns>
        /// <param name="windowRect">Editor window rect</param>
        /// <param name="topOffset">Amount of height to exclude from total height bottom</param>
        /// <param name="bottomOffset">Amount of height to exclude from total height bottom</param>
        /// <param name="upperSectionDrawAction">Upper section content draw delegate</param>
        /// <param name="lowerSectionDrawAction">Lower section content draw delegate</param>
        public bool Draw(Rect windowRect,
                         float topOffset,
                         float bottomOffset,
            GUIStyle upperSectionGUIStyle, Action upperSectionDrawAction,
            GUIStyle lowerSectionGUIStyle, Action lowerSectionDrawAction)
        {
            if (upperSectionDrawAction == null || lowerSectionDrawAction == null)
            {
                throw new ArgumentNullException();
            }

            maxBorder = (int)windowRect.height - (int)bottomOffset - EditorUIConstants.LAYOUT_PADDING;

            if (topOffset > 1)
            {
                minBorder = (int)topOffset;
            }

            upperSection.FixedBorderPos = minBorder + EditorUIConstants.LAYOUT_PADDING;
            lowerSection.FixedBorderPos = maxBorder;

            bool needRepaint = sliderDrawer.Handle(windowRect);
            sliderDrawer.SliderPos = Mathf.Clamp(
                sliderDrawer.SliderPos,
                upperSection.FixedBorderPos + upperSection.MinSize,
                lowerSection.FixedBorderPos - lowerSection.MinSize);

            upperSection.Draw(upperSectionGUIStyle, upperSectionDrawAction, sliderDrawer, windowRect);
            sliderDrawer.Draw(windowRect);
            lowerSection.Draw(lowerSectionGUIStyle, lowerSectionDrawAction, sliderDrawer, windowRect);

            return needRepaint;
        }

        /// <summary>
        /// Section drawer
        /// </summary>
        [Serializable]
        private class SectionDrawer
        {
            /// <summary>
            /// The fixed border position
            /// </summary>
            [SerializeField]
            public int FixedBorderPos;

            /// <summary>
            /// The minimum size of the section
            /// </summary>
            [SerializeField]
            public int MinSize;

            /// <summary>
            /// The scroll position
            /// </summary>
            [SerializeField]
            private Vector2 scrollPosition;

            [SerializeField]
            private PositionTab position;

            public enum PositionTab
            {
                Upper,
                Lower
            }
            
            public SectionDrawer(int minSize, PositionTab position)
            {
                MinSize = minSize;
                this.position = position;
            }

            /// <summary>
            /// Draw the section
            /// </summary>
            /// <returns>The section's rect</returns>
            /// <param name="drawer"></param>
            /// <param name="sliderDrawer">Slider's drawer</param>
            public void Draw(GUIStyle style, Action drawer, SliderDrawer sliderDrawer, Rect windowRect)
            {
                Rect rect = GetRect(sliderDrawer, windowRect);

                GUILayout.BeginArea(rect);

                EditorGUILayout.BeginVertical(style);

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                drawer();

                GUILayout.EndScrollView();

                EditorGUILayout.EndVertical();

                GUILayout.EndArea();
            }

            private int CalculateHeight(SliderDrawer sliderDrawer)
            {
                switch (position)
                {
                    case PositionTab.Upper:
                        return sliderDrawer.SliderUpperBorderPos - FixedBorderPos;
                    case PositionTab.Lower:
                        return FixedBorderPos - sliderDrawer.SliderLowerBorderPos;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private Rect GetRect(SliderDrawer sliderDrawer, Rect windowRect)
            {
                Vector2 minimumCorner = position == PositionTab.Upper
                                                               ? new Vector2(0, FixedBorderPos)
                                                               : new Vector2(0, sliderDrawer.SliderLowerBorderPos);

                Vector2 size = new Vector2(windowRect.width, CalculateHeight(sliderDrawer));

                return new Rect(minimumCorner, size);
            }
        }

        /// <summary>
        /// Slider drawer
        /// </summary>
        [Serializable]
        private class SliderDrawer
        {
            /// <summary>
            /// Height of the resize control rect
            /// </summary>
            private const int RESIZE_RECT_HEIGHT = 10;

            /// <summary>
            /// Center line's thickness
            /// </summary>
            private const int RESIZE_LINE_THICKNESS = 2;

            /// <summary>
            /// Center line's color
            /// </summary>
            private static readonly Color lineColor = new Color(0.749f, 0.749f, 0.749f);

            /// <summary>
            /// Coordinate of slider center
            /// </summary>
            [SerializeField]
            public int SliderPos = 0;

            /// <summary>
            /// Offset between mouse position and slider center
            /// </summary>
            [SerializeField]
            private int mouseOffset;

            /// <summary>
            /// Slider's upper border coordinate
            /// </summary>
            public int SliderUpperBorderPos { get { return SliderPos - RESIZE_RECT_HEIGHT / 2; } }

            /// <summary>
            /// Slider's lower border coordinate
            /// </summary>
            public int SliderLowerBorderPos { get { return SliderPos + RESIZE_RECT_HEIGHT / 2; } }

            [SerializeField]
            private Rect sliderRect;

            /// <summary>
            /// Is slider currently being dragged
            /// </summary>
            [SerializeField]
            private bool isDragged;

            /// <summary>
            /// Handle mouse events
            /// </summary>
            /// <returns>Should we call Repaint() or not</returns>
            /// <param name="windowRect">Editor window rect</param>
            public bool Handle(Rect windowRect)
            {
                /*
                 * |__________________________________________| _
                 *                                               | <- RESIZE_PADDING_FROM_CLASSES
                 *  __________________________________________   |
                 * |           -RESIZE_RECT_HEIGHT-           | _|
                 * |__________________________________________|<- resizeRect
                 *  
                 *  __________________________________________
                 * |                                          |
                 */

                sliderRect = new Rect(
                    new Vector2(
                        0,
                        SliderPos - RESIZE_RECT_HEIGHT / 2),

                    new Vector2(
                        windowRect.width,
                        RESIZE_RECT_HEIGHT));

                bool needRepaint = HandleMouseDownEvent();

                needRepaint |= HandleMouseDragEvent();

                needRepaint |= HandleMouseUpEvent();

                needRepaint |= HandleMouseOutOfWindow();

                return needRepaint;
            }

            /// <summary>
            /// Draws the slider line and changes cursor image
            /// </summary>
            /// <param name="windowRect">Window rect.</param>
            public void Draw(Rect windowRect)
            {
                Rect lineRect = new Rect(
                    new Vector2(
                        0,
                        SliderPos - RESIZE_LINE_THICKNESS / 2),

                    new Vector2(
                        windowRect.width,
                        RESIZE_LINE_THICKNESS));

                DrawEmptyBox(lineRect, lineColor);

                EditorGUIUtility.AddCursorRect(sliderRect, MouseCursor.ResizeVertical);
            }

            /// <summary>
            /// Handle the mouse down event. If we click on control rect of the slider, we consider the slider being dragged. Otherwise - not
            /// </summary>
            /// <returns>Should we call Repaint() or not</returns>
            private bool HandleMouseDownEvent()
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    Vector2 mousePos = Event.current.mousePosition;

                    if (sliderRect.Contains(mousePos))
                    {
                        isDragged = true;

                        mouseOffset = (int)Event.current.mousePosition.y - SliderPos;

                        return true;
                    }

                    isDragged = false;

                }

                return false;
            }

            /// <summary>
            /// Handle the mouse drag event. If the slider is being dragged, we set sliderPos to mouse position (minus offset). If the position has changed, we call Repaint()
            /// </summary>
            /// <returns>Should we call Repaint() or not</returns>
            private bool HandleMouseDragEvent()
            {
                if (Event.current.type == EventType.MouseDrag && isDragged)
                {
                    int previousSliderPos = SliderPos;

                    SliderPos = (int)Event.current.mousePosition.y - mouseOffset;

                    if (SliderPos != previousSliderPos)
                    {
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Handle the mouse up event. Consider the slider as not being dragged
            /// </summary>
            /// <returns>Should we call Repaint() or not</returns>
            private bool HandleMouseUpEvent()
            {
                if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
                {
                    isDragged = false;

                    return true;
                }

                return false;
            }

            /// <summary>
            /// Handle the mouse out of window event
            /// </summary>
            /// <returns>Should we call Repaint() or not</returns>
            private bool HandleMouseOutOfWindow()
            {
                if (!isDragged)
                    return false;

                var mouseOverWindow = EditorWindow.mouseOverWindow;

                if (mouseOverWindow != null)
                {
                    string currentWindow = mouseOverWindow.ToString();

                    if (!string.IsNullOrEmpty(currentWindow))
                    {
                        if (currentWindow.Contains("RuntimeTestRunnerWindow"))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }
    }

    public static class UIHelper
    {
        public static bool SearchField(ref string text, Rect rect)
        {
            const int buttonSize = 14;
            Rect filterRect = rect;
            filterRect.width -= buttonSize; 
            var oldText = text; 
            text = EditorGUI.TextField(filterRect, "", text, "ToolbarSeachTextField");

            Rect buttonRect = filterRect;
            buttonRect.x += filterRect.width;
            buttonRect.width = buttonSize;
            if (GUI.Button(buttonRect, GUIContent.none,
                text.Length > 0 ? "ToolbarSeachCancelButton" : "ToolbarSeachCancelButtonEmpty"))
            {
                text = "";
                GUIUtility.keyboardControl = 0;
            }
            return oldText.Equals(text, StringComparison.Ordinal);
        }
        
        public static bool SearchField(ref string text, float width = -1)
        {
            EditorGUILayout.BeginHorizontal();
            var oldText = text;
            GUILayout.Space(width);
            if (width != -1)
            {
                text = EditorGUILayout.TextField("", text, "ToolbarSeachTextField",
                    GUILayout.Width(width));    
            }
            else
            {
                text = EditorGUILayout.TextField("", text, "ToolbarSeachTextField");
            }
            
            if (GUILayout.Button(GUIContent.none, text.Length > 0 ? "ToolbarSeachCancelButton"
                : "ToolbarSeachCancelButtonEmpty"))
            {
                text = "";
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();
            return oldText.Equals(text, StringComparison.Ordinal);
        }
    }
}
#endif