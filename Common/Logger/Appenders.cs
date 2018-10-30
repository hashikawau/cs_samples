using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CSSamples.Common.Logger
{
    public class ConsoleAppender
    {
        public void Append(object sender, LogAppendingEventArgs args)
        {
            Console.Write($"[{args.Time}] [{args.Level,-5}] [{args.Tag}] ");
            Console.WriteLine(args.Message);
        }
    }

    public class DebugLogAppender
    {
        public void Append(object sender, LogAppendingEventArgs args)
        {
            Debug.Write($"[{args.Time}] [{args.Level,-5}] [{args.Tag}] ");
            Debug.WriteLine(args.Message);
        }
    }

    public class FileAppender
    {
        public string FilePath { get; private set; }

        public FileAppender(string filePath)
        {
            FilePath = filePath;
            if (Directory.Exists(FilePath))
                throw new Exception(string.Format("file path is directory: FilePath={0}", FilePath));
            string directory = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public void Append(object sender, LogAppendingEventArgs args)
        {
            lock (FilePath)
            {
                using (var writer = new StreamWriter(
                    new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite),
                    Encoding.UTF8))
                {
                    writer.Write($"[{args.Time}] [{args.Level,-5}] [{args.Tag}] ");
                    writer.WriteLine(args.Message);
                }
            }
        }
    }
}