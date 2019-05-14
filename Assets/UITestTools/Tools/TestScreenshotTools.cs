using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlayQ.UITestTools
{
	public static class TestScreenshotTools
	{
		private static string GenerateScreenshotName(string name)
		{
			var adjustedName = new StringBuilder()
				.Append(name)
				.Append('-')
				.Append(DateTime.UtcNow.ToString(PlayModeLogger.FILE_DATE_FORMAT))
				.Append(".png").ToString();

			return adjustedName;
		}

        public static string GetFullPath(string name)
        {
            return GenerateScreenshotPathWithSubfolder(GenerateMainScreenshotDirectoryPath()) + Path.DirectorySeparatorChar + GenerateScreenshotName(name);
        }

		private static string GenerateScreenshotPathWithSubfolder(string path)
		{
			var camera = UITestUtils.FindAnyGameObject<Camera>();

			var pathBuilder = new StringBuilder();
            pathBuilder.Append(path);
            pathBuilder.Append(Path.DirectorySeparatorChar);
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
            if (!Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }
			return result;
		}
		
        private static string GenerateMainScreenshotDirectoryPath()
        {
            var pathBuilder = new StringBuilder();

            if (!Application.isMobilePlatform)
            {
                pathBuilder
                    .Append(Application.persistentDataPath)
                    .Append(Path.DirectorySeparatorChar);
            }

            pathBuilder.Append("Screenshots");
            var result = pathBuilder.ToString();
            if (!Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }
            return result;
        }
        
        public static string GenerateReferenceScreenshotPath()
        {
            var pathBuilder = new StringBuilder();
            pathBuilder
                .Append(Application.dataPath +
                        Path.DirectorySeparatorChar + "Tests" +
                        Path.DirectorySeparatorChar + "Editor" +
                        Path.DirectorySeparatorChar + "Resources")
                .Append(Path.DirectorySeparatorChar)
                .Append("ReferenceScreenshots");

            var subfolderPath = GenerateScreenshotPathWithSubfolder(pathBuilder.ToString());
            return subfolderPath;
        }
        
        public static string GenerateReferenceScreenshotName(string name)
        {
            var adjustedName = new StringBuilder()
                .Append(name)
                .Append(".png").ToString();

            return adjustedName;
        }
        
        public static void ClearScreenshotsEmptyFolders()
        {
            var directories = Directory.GetDirectories(GenerateMainScreenshotDirectoryPath());
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

	}
}