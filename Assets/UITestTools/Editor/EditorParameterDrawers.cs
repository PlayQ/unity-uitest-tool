using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace PlayQ.UITestTools
{
    public abstract class AbstractParameterDrawer
    {
        public abstract Type ExpectedType { get; }

        public abstract void EditorDraw(AbstractParameter parameter,
            Rect rect, MethodInfo methodInfo);
    }

    public abstract class AbstractParameterDrawer<T> : AbstractParameterDrawer
        where T : AbstractParameter
    {
        public override Type ExpectedType
        {
            get { return typeof(T); }
        }

        protected abstract void EditorDrawInner(T parameter,
            Rect rect, MethodInfo methodInfo);

        public sealed override void EditorDraw(AbstractParameter parameter,
            Rect rect, MethodInfo methodInfo)
        {
            if (parameter.GetType() != typeof(T))
            {
                Debug.LogError("Parameter drawer of type: " + GetType() +
                               " can draw only instances of type: " + typeof(T) +
                               " but you pass instance of type: " + parameter.GetType());
                return;
            }

            EditorDrawInner((T) parameter, rect, methodInfo);
        }
    }

    public class ParameterIntDrawer : AbstractParameterDrawer<ParameterInt>
    {
        protected override void EditorDrawInner(ParameterInt parameter, Rect rect, MethodInfo methodInfo)
        {
            var index = parameter.CurrentParameterIndex();
            var parameters = methodInfo.GetParameters();
            if (parameters.Length <= index)
            {
                Debug.LogWarning("missmatch parameters amount for: " + methodInfo.Name);
                return;
            }
            var paramName = parameters[index].Name;
            if (parameter.ParameterValue != null)
            {
                parameter.ParameterValue =
                    EditorGUI.IntField(rect, new GUIContent(paramName.FirstCharToUpper() + ":", paramName.FirstCharToUpper()), parameter.ParameterValue);
            }
        }
    }

    public class ParameterFloatDrawer : AbstractParameterDrawer<ParameterFloat>
    {
        protected override void EditorDrawInner(ParameterFloat parameter, Rect rect, MethodInfo methodInfo)
        {
            var index = parameter.CurrentParameterIndex();
            var parameters = methodInfo.GetParameters();
            if (parameters.Length <= index)
            {
                Debug.LogWarning("missmatch parameters amount for: " + methodInfo.Name);
                return;
            }
            var paramName = parameters[index].Name;
            parameter.ParameterValue =
                EditorGUI.FloatField(rect, new GUIContent(paramName.FirstCharToUpper() + ":", paramName.FirstCharToUpper()), parameter.ParameterValue);
        }
    }

    public class ParameterDoubleDrawer : AbstractParameterDrawer<ParameterDouble>
    {
        protected override void EditorDrawInner(ParameterDouble parameter, Rect rect, MethodInfo methodInfo)
        {
            var index = parameter.CurrentParameterIndex();
            var parameters = methodInfo.GetParameters();
            if (parameters.Length <= index)
            {
                Debug.LogWarning("missmatch parameters amount for: " + methodInfo.Name);
                return;
            }
            var paramName = parameters[index].Name;
            if (parameter.ParameterValue != null)
            {
                parameter.ParameterValue =
                    EditorGUI.DoubleField(rect, new GUIContent(paramName.FirstCharToUpper() + ":", paramName.FirstCharToUpper()), parameter.ParameterValue);
            }
        }
    }

    public class ParameterBoolDrawer : AbstractParameterDrawer<ParameterBool>
    {
        protected override void EditorDrawInner(ParameterBool parameter, Rect rect, MethodInfo methodInfo)
        {
            var index = parameter.CurrentParameterIndex();
            var parameters = methodInfo.GetParameters();
            if (parameters.Length <= index)
            {
                Debug.LogWarning("missmatch parameters amount for: " + methodInfo.Name);
                return;
            }
            var paramName = parameters[index].Name;
            if (parameter.ParameterValue != null)
            {
                parameter.ParameterValue =
                    EditorGUI.Toggle(rect, new GUIContent(paramName.FirstCharToUpper() + ":", paramName.FirstCharToUpper()), parameter.ParameterValue);
            }
        }
    }

    public class ParameterStringDrawer : AbstractParameterDrawer<ParameterString>
    {

        protected override void EditorDrawInner(ParameterString parameter, Rect rect, MethodInfo methodInfo)
        {
            var index = parameter.CurrentParameterIndex();
            var parameters = methodInfo.GetParameters();
            if (parameters.Length <= index)
            {
                Debug.LogWarning("missmatch parameters amount for: " + methodInfo.Name);
                return;
            }
            var paramName = parameters[index].Name;
            if (parameter.ParameterValue != null)
            {
                parameter.ParameterValue = EditorGUI.TextField(rect, new GUIContent(paramName.FirstCharToUpper() + ":",paramName.FirstCharToUpper()), parameter.ParameterValue);
            }
        }
    }

    public class ParameterEnumDrawer : AbstractParameterDrawer<ParameterEnum>
    {
        protected override void EditorDrawInner(ParameterEnum parameter, Rect rect, MethodInfo methodInfo)
        {
            var index = parameter.CurrentParameterIndex();
            var parameters = methodInfo.GetParameters();
            if (parameters.Length <= index)
            {
                Debug.LogWarning("missmatch parameters amount for: " + methodInfo.Name);
                return;
            }
            var paramName = parameters[index].Name;
            if (parameter.NonSerializedEnum == null)
            {
                parameter.NonSerializedEnum = (Enum) Enum.Parse(parameter.EnumType, parameter.ParameterValue);
            }

            var newNonSerializedEnum = EditorGUI.EnumPopup(rect, new GUIContent(paramName.FirstCharToUpper() + ":", paramName.FirstCharToUpper()), parameter.NonSerializedEnum);
            if (newNonSerializedEnum != parameter.NonSerializedEnum)
            {
                parameter.ParameterValue = newNonSerializedEnum.ToString();
            }

            parameter.NonSerializedEnum = newNonSerializedEnum;
        }
    }

    public class ParameterPathToGameObjectDrawer : AbstractParameterDrawer<ParameterPathToGameObject>
    {
        private const int SELECT_BUTTON_WIDTH = 100;
        private const int SPACE = 2;

        protected override void EditorDrawInner(ParameterPathToGameObject parameter, Rect rect, MethodInfo methodInfo)
        {
            var index = parameter.CurrentParameterIndex();
            var parameters = methodInfo.GetParameters();
            if (parameters.Length <= index)
            {
                Debug.LogWarning("missmatch parameters amount for: " + methodInfo.Name);
                return;
            }
            var paramName = parameters[index].Name;
            if (parameter.ParameterValue != null)
            {
                Rect parameterRect = new Rect(rect.min, rect.size + new Vector2(-(SELECT_BUTTON_WIDTH + SPACE), 0f));
                Rect buttonRect = new Rect(new Vector2(parameterRect.max.x + SPACE, rect.min.y), new Vector2(SELECT_BUTTON_WIDTH, rect.size.y));

                parameter.ParameterValue =
                    EditorGUI.TextField(parameterRect, new GUIContent(paramName.FirstCharToUpper() + ":",paramName.FirstCharToUpper()), parameter.ParameterValue);

                if (GUI.Button(buttonRect, "Select"))
                {
                    GameObject target = UITestUtils.FindAnyGameObject(parameter.ParameterValue);

                    if (target != null)
                        Selection.activeGameObject = target;
                }
            }
        }
    }

    public static class StringUtils
    {
        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: 
                    throw new ArgumentNullException();
                case "":
                    return "";
                default:
                    return input.First().ToString().ToUpper() + input.Substring(1);
                    break;
            }
        }

    }
}