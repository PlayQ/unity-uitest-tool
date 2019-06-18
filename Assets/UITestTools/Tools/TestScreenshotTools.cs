using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlayQ.UITestTools
{
	public static class TestScreenshotTools
	{
		public const string REFERENCE_SCREENSHOT_DIRECTORY = "ReferenceScreenshots";
		public const string SCREENSHOT_DIRECTORY = "Screenshots";

		public static string GenerateScreenshotNameWithTime(string name)
		{
			var adjustedName = new StringBuilder()
				.Append(name)
				.Append('-')
				.Append(DateTime.UtcNow.ToString(PlayModeLogger.FILE_DATE_FORMAT))
				.Append(".png").ToString();

			return adjustedName;
		}

		public static string GenerateReferenceScreenshotNameToSave(string name)
		{
			var adjustedName = new StringBuilder()
				.Append(name)
				.Append(".png").ToString();

			return adjustedName;
		}

		public static string ScreenshotDirectoryToCaptureByUnity
		{
			get
			{
				string path;
#if UNITY_EDITOR
				path = Application.persistentDataPath + '/' + SCREENSHOT_DIRECTORY;
#else
				//On mobile platforms the filename is appended to the persistent data path.
		        path = '/' + SCREENSHOT_DIRECTORY;
#endif

				return path + '/' + SubDirectoriesForCurrentTest;
			}
		}
		
		public static string ScreenshotDirectoryToLoadByFileSystem
		{
			get
			{
				var path = Application.persistentDataPath + '/' + 
				           SCREENSHOT_DIRECTORY + '/' + 
				           SubDirectoriesForCurrentTest;

				return path;
			}
		}
		
#if UNITY_EDITOR
		public static string ReferenceScreenshotDirectoryToSaveByFileSystem
		{
			get
			{
				var editorResources = SelectedTestsSerializable.EditorResourceDirectory;
				var path = editorResources + "/" + REFERENCE_SCREENSHOT_DIRECTORY;
				if (string.IsNullOrEmpty(path))
				{
					return null;
				}
				return path + '/' + SubDirectoriesForCurrentTest;
			}
		}
#endif
		public static string ReferenceScreenshotDirectoryToLoadFromResources
		{
			get
			{
				return REFERENCE_SCREENSHOT_DIRECTORY + '/' + SubDirectoriesForCurrentTest;
			}
		}

		public static string SubDirectoriesForCurrentTest
		{
			get
			{
				var camera = UITestUtils.FindAnyGameObject<Camera>();

				var pathBuilder = new StringBuilder();
				pathBuilder.Append("resolution_")
					.Append(camera.pixelWidth)
					.Append("_")
					.Append(camera.pixelHeight);

				var testMethodName = CurrentTestInfo.TestMethodName;
				if (!string.IsNullOrEmpty(testMethodName))
				{
					pathBuilder.Append(Path.DirectorySeparatorChar);
					pathBuilder.Append(testMethodName);
				}

				var result = pathBuilder.ToString();
				if (!Directory.Exists(result))
				{
					Directory.CreateDirectory(result);
				}

				return result;
			}
		}
		
        
#if UNITY_EDITOR        
        public static void ClearScreenshotsEmptyFolders()
        {
	        var screenshotDirectory = Application.persistentDataPath + '/' + SCREENSHOT_DIRECTORY;
            var directories = Directory.GetDirectories(screenshotDirectory);
            if (directories.Any())
            {
                foreach (var dirName in directories)
                {
                    ClearScreenshotsEmptyFoldersRec(dirName);
                }
            }
        }
        
        private static void ClearScreenshotsEmptyFoldersRec(string targetPath)
        {
            var files = Directory.GetFiles(targetPath, "*.png", SearchOption.AllDirectories);
            if (!files.Any())
            {
                Directory.Delete(targetPath, true);
            }
            else
            {
                var directories = Directory.GetDirectories(targetPath);
                if (directories.Any())
                {
                    foreach (var dirName in directories)
                    {
                        ClearScreenshotsEmptyFoldersRec(dirName);
                    }
                }
            }
        }
#endif

	}
}