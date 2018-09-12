using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlayQ.UITestTools
{
	public static class TestScreenshotTools
	{
		public static string GenerateScreenshotName(string name)
		{
			var adjustedName = new StringBuilder()
				.Append(name)
				.Append('-')
				.Append(DateTime.UtcNow.ToString(PlayModeLogger.FILE_DATE_FORMAT))
				.Append(".png").ToString();

			return adjustedName;
		}

		public static string GenerateScreenshotSubfolderPath()
		{
			var camera = UITestUtils.FindAnyGameObject<Camera>();

			var pathBuilder = new StringBuilder();
			pathBuilder.Append("resolution_")
				.Append(camera.pixelWidth)
				.Append("_")
				.Append(camera.pixelHeight);

			var testSuitName = CurrentTestInfo.TestSuitName;
			if (!string.IsNullOrEmpty(testSuitName))
			{
				pathBuilder.Append(Path.DirectorySeparatorChar);
				pathBuilder.Append(testSuitName);
			}

			var testMethodName = CurrentTestInfo.TestMethodName;
			if (!string.IsNullOrEmpty(testMethodName))
			{
				pathBuilder.Append(Path.DirectorySeparatorChar);
				pathBuilder.Append(testMethodName);
			}

			var result = pathBuilder.ToString();
			return result;
		}
	}
}