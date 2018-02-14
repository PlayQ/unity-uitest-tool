using System;
using System.IO;

namespace PlayQ.UITestTools
{
    public class LogSaver
    {
        private readonly string path;
        private StreamWriter writer;

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