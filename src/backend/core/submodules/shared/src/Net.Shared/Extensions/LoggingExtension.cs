using System;

using Microsoft.Extensions.Logging;

namespace Net.Shared.Extensions.Logging;

public static class LoggingExtension
{
    private static readonly string Pattern = "[{time:dd-MM HH:mm:ss}] - {message}";

    private static readonly Action<ILogger, DateTime, string, Exception?> ErrorShortAction = LoggerMessage.Define<DateTime, string>(LogLevel.Error, LogEvents.Error, Pattern);
    private static readonly Action<ILogger, DateTime, string, Exception?> ErrorCompactAction = LoggerMessage.Define<DateTime, string>(LogLevel.Error, LogEvents.Error, Pattern);
    private static readonly Action<ILogger, DateTime, Exception, Exception?> ErrorFullAction = LoggerMessage.Define<DateTime, Exception>(LogLevel.Error, LogEvents.Error, Pattern);
    private static readonly Action<ILogger, DateTime, string, Exception?> WarnAction = LoggerMessage.Define<DateTime, string>(LogLevel.Warning, LogEvents.Warning, Pattern);
    private static readonly Action<ILogger, DateTime, string, Exception?> InfoAction = LoggerMessage.Define<DateTime, string>(LogLevel.Information, LogEvents.Information, Pattern);
    private static readonly Action<ILogger, DateTime, string, Exception?> DebugAction = LoggerMessage.Define<DateTime, string>(LogLevel.Debug, LogEvents.Debug, Pattern);
    private static readonly Action<ILogger, DateTime, string, Exception?> TraceAction = LoggerMessage.Define<DateTime, string>(LogLevel.Trace, LogEvents.Trace, Pattern);

    public static void ErrorShort(this ILogger logger, Exception exception) => ErrorShortAction(logger, DateTime.UtcNow, exception.Message, null);
    public static void ErrorCompact(this ILogger logger, Exception exception)
    {
        var errorStackFirstLine = string.Empty;

        if (exception.StackTrace is not null)
        {
            var lines = exception.StackTrace.AsSpan();
            var endIndex = lines.IndexOf(Environment.NewLine);

            if (endIndex > 0)
            {
                var firstLine = lines[..endIndex];
                var inIndex = firstLine.IndexOf(" in ");
                errorStackFirstLine = inIndex > 0 ? firstLine[..inIndex].ToString().Trim() : string.Empty;
            }
        }

        var trimmedMessage = exception.Message.TrimEnd('.');
        var message = string.IsNullOrEmpty(errorStackFirstLine) ? trimmedMessage : $"{trimmedMessage} {errorStackFirstLine}";

        ErrorCompactAction(logger, DateTime.UtcNow, message, null);
    }
    public static void ErrorFull(this ILogger logger, Exception exception) => ErrorFullAction(logger, DateTime.UtcNow, exception, null);
    public static void Warn(this ILogger logger, string message) => WarnAction(logger, DateTime.UtcNow, message, null);
    public static void Info(this ILogger logger, string message) => InfoAction(logger, DateTime.UtcNow, message, null);
    public static void Debug(this ILogger logger, string message) => DebugAction(logger, DateTime.UtcNow, message, null);
    public static void Trace(this ILogger logger, string message) => TraceAction(logger, DateTime.UtcNow, message, null);

    private static class LogEvents
    {
        public static readonly EventId Error = new(1000, "error native log");
        public static readonly EventId Warning = new(1001, "warning native log");
        public static readonly EventId Information = new(1002, "information native log");
        public static readonly EventId Debug = new(1003, "debug native log");
        public static readonly EventId Trace = new(1004, "trace native log");
    }
}
