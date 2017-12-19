#if UNITY_EDITOR

using UnityEditor;
using System;
using System.Reflection;

namespace PlayQ.UITestTools
{
	public static class GameViewResizer
	{
		static GameViewResizer()
		{
			GameViewType = Type.GetType("UnityEditor.GameView, UnityEditor.dll");
			GameViewSizeType = Type.GetType("UnityEditor.GameViewSize, UnityEditor.dll");
			GameViewSizeEnum = Type.GetType("UnityEditor.GameViewSizeType, UnityEditor.dll");


			var gameViewSizesType = Type.GetType("UnityEditor.GameViewSizes, UnityEditor.dll");
			var gameViewSizeGroupType = Type.GetType("UnityEditor.GameViewSizeGroup, UnityEditor.dll");
			var singletonOfGameViewSizes = Type.GetType("UnityEditor.ScriptableSingleton`1, UnityEditor.dll")
				.MakeGenericType(gameViewSizesType);

			GetMainGameViewMethod = FindMethod(GameViewType, "GetMainGameView");
			SetSelectionMethod = FindMethod(GameViewType, "SizeSelectionCallback");
			AddCustomSizeMethod = FindMethod(gameViewSizeGroupType, "AddCustomSize");
			GetTotalGroupCountMethod = FindMethod(gameViewSizeGroupType, "GetTotalCount");

			SingletonOfGameViewSizesInstance = FindProperty(singletonOfGameViewSizes, "instance", gameViewSizesType);
			CurrentGroupProperty = FindProperty(gameViewSizesType, "currentGroup", gameViewSizeGroupType);
		}

		private static readonly Type GameViewType;
		private static readonly Type GameViewSizeType;
		private static readonly Type GameViewSizeEnum;

		private static readonly MethodInfo GetMainGameViewMethod;
		private static readonly MethodInfo SetSelectionMethod;
		private static readonly MethodInfo AddCustomSizeMethod;
		private static readonly MethodInfo GetTotalGroupCountMethod;

		private static readonly PropertyInfo SingletonOfGameViewSizesInstance;
		private static readonly PropertyInfo CurrentGroupProperty;

		private static PropertyInfo FindProperty(Type defineType, string name, Type returnType)
		{
			return defineType.GetProperty(name,
				BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
				returnType, new Type[0], null);
		}

		private static MethodInfo FindMethod(Type defineType, string name)
		{
			return defineType.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public static void SetResolution(int width, int height)
		{
			var window = GetWindow();

			var instance = SingletonOfGameViewSizesInstance.GetValue(null, null);
			var currentGroup = CurrentGroupProperty.GetValue(instance, null);
			var sizeTypeObject = Enum.ToObject(GameViewSizeEnum, 1);
			var targetSize = Activator.CreateInstance(GameViewSizeType, sizeTypeObject, width, height, "Test size");
			AddCustomSizeMethod.Invoke(currentGroup, new object[] {targetSize});
			var count = (int) GetTotalGroupCountMethod.Invoke(currentGroup, new object[0]);
			SetSelectionMethod.Invoke(window, new object[] {count - 1, null});
		}

		private static EditorWindow GetWindow()
		{
			var currentWindow = EditorWindow.mouseOverWindow;
			if (currentWindow != null && GameViewType == currentWindow.GetType())
			{
				return currentWindow;
			}

			currentWindow = EditorWindow.focusedWindow;
			if (currentWindow != null && GameViewType == currentWindow.GetType())
			{
				return currentWindow;
			}

			return GetMainGameViewMethod.Invoke(null, null) as EditorWindow;
		}
	}
}
#endif