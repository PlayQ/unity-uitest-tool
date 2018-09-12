using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public class LogSaver
    {
        private readonly string path;
        private StreamWriter writer;

        public void SaveTestInfo(TestInfoData data)
        {
            var json = JObject.FromObject(data).ToString();
            var folderPath = Application.persistentDataPath + Path.DirectorySeparatorChar + "Metrics";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            File.WriteAllText(folderPath + Path.DirectorySeparatorChar + "tests.json", json);
        }

        public LogSaver(string path)
        {
            this.path = path;
        }

        public void StartWrite()
        {
            writer = new StreamWriter(File.Create(path));
        }

        public void Write(string log, string stacktrace)
        {
            writer.WriteLine(DateTime.UtcNow.ToString(PlayModeLogger.TC_DATE_FORMAT) + ": " + log);
            writer.WriteLine(stacktrace);
        }

        public void Close()
        {
            writer.Close();
        }
    }
}