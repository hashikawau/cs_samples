using System;

namespace CSSamples.Common.Logger
{
    public class Logger
    {
        public static LogLevel GlobalLevel { get; set; }
#if DEBUG
            = LogLevel.DEBUG;
#else
            = LogLevel.INFO;
#endif

        public static event EventHandler<LogAppendingEventArgs> Appenders;

        public static Logger GetLogger(Type clazz)
            => new Logger(clazz);

        private string _tag;

        public Logger(Type clazz)
        {
            _tag = clazz.Name;
        }

        private void WriteToAppenders(LogLevel level, string message)
        {
            if (level <= GlobalLevel)
                Appenders?.Invoke(this, new LogAppendingEventArgs(level, _tag, message));
        }

        public void Error(Exception exception)
            => WriteToAppenders(LogLevel.ERROR, string.Format("{0}\n{1}", exception.Message, exception.StackTrace));

        public void Error(string format, params object[] args)
            => WriteToAppenders(LogLevel.ERROR, string.Format(format, args));

        public void Warn(string format, params object[] args)
            => WriteToAppenders(LogLevel.WARN, string.Format(format, args));

        public void Info(string format, params object[] args)
            => WriteToAppenders(LogLevel.INFO, string.Format(format, args));

        public void Debug(string format, params object[] args)
            => WriteToAppenders(LogLevel.DEBUG, string.Format(format, args));
    }

    public enum LogLevel
    {
        ERROR,
        WARN,
        INFO,
        DEBUG
    }

    public class LogAppendingEventArgs
    {
        public string Time { get; private set; }
        public LogLevel Level { get; private set; }
        public string Tag { get; private set; }
        public string Message { get; private set; }

        public LogAppendingEventArgs(LogLevel level, string tag, string message)
        {
            Time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            Level = level;
            Tag = tag;
            Message = message;
        }
    }
}
