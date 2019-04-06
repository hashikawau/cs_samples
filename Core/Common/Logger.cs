using System;
using System.IO;

namespace FileTransfer.Core.Common
{
    class Logger
    {
        private static string _dateFormat = "yyyy/MM/dd HH:mm:ss.fff";

        private static string _filePath = Path.Combine(Path.GetTempPath(), "app.log");

        private string _tag;

        public Logger(Type clazz)
        {
            _tag = clazz.Name;
        }

        public Logger(string tag)
        {
            _tag = tag;
        }

        internal void Error(string format, params object[] args)
        {
            WriteLog("ERROR", _tag, format, args);
        }

        internal void Error(Exception e)
        {
            WriteLog("ERROR", _tag, "{0}: {1}\n{2}", e.Source, e.Message, e.StackTrace);
        }

        internal void Warn(string format, params object[] args)
        {
            WriteLog("WARN ", _tag, format, args);
        }

        internal void Info(string format, params object[] args)
        {
            WriteLog("INFO ", _tag, format, args);
        }

        internal void Debug(string format, params object[] args)
        {
            WriteLog("DEBUG", _tag, format, args);
        }

        private static void WriteLog(string logLevel, string tag, string format, params object[] args)
        {
            string header = string.Format("[{0}] [{1}] [{2}]: ", DateTime.Now.ToString(_dateFormat), logLevel, tag);
            string message = string.Format(format, args);
            //System.Diagnostics.Debug.Write(header);
            //System.Diagnostics.Debug.WriteLine(message);
            lock (_filePath)
            {
                using (StreamWriter ostream = new StreamWriter(new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read)))
                {
                    ostream.BaseStream.Seek(0, SeekOrigin.End);
                    ostream.Write(header);
                    ostream.WriteLine(message);
                }
            }

            //Console.Write(string.Format("[{0}] [{1}] [{2}]: ", DateTime.Now.ToString(_dateFormat), logLevel, tag));
            //Console.WriteLine(string.Format(format, args));
        }
    }
}
