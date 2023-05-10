using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RefconGatewayBase.Configuration;

namespace RefconGatewayBase.Logging;

public static class Log
{
    public enum LogType
    {
        Debug,
        Info,
        Warning,
        Error,
        Property
    }

    private static List<ILog> _logInterfaces;

    private static List<ILog> logInterfaces
    {
        get
        {
            if (_logInterfaces == null) { _logInterfaces = SelectLoginterfaces(); }

            return _logInterfaces;
        }
    }

    #region Private methods

    private static List<ILog> SelectLoginterfaces()
    {
        var loggers = new List<ILog>();
        if (Config.LogAnalyticsEnabled) { loggers.Add(new LogAnalytics()); }

        // If we have a console available, use it
        if (OperatingSystem.IsWindows() && Environment.UserInteractive && Console.Title.Length > 0 && Config.ConsoleLogEnabled) { loggers.Add(new LogConsole()); }

        return loggers;
    }

    #endregion Private methods

    #region Public methods

    public static bool LoggersConfigured()
    {
        if (_logInterfaces == null) { return false; }

        return !_logInterfaces.Any(x => !x.IsConfigured());
    }

    public static void AddLogger(ILog logger)
    {
        if (!logInterfaces.Any(x => x.GetType() == logger.GetType())) { _logInterfaces.Add(logger); }
    }

    /// <summary>
    /// Logs information
    /// </summary>
    /// <param name="message">The message.</param>
    public static void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        logInterfaces.ForEach(x => x.Debug(message, memberName, filePath, lineNumber));
    }

    /// <summary>
    /// Logs information
    /// </summary>
    /// <param name="message">The message.</param>
    public static void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        logInterfaces.ForEach(x => x.Info(message, memberName, filePath, lineNumber));
    }

    /// <summary>
    /// Logs warning
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="e">The exception</param>
    public static void Warning(string message, Exception e = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        logInterfaces.ForEach(x => x.Warning(message, e, memberName, filePath, lineNumber));
    }

    /// <summary>
    /// Logs error
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="e">The exception</param>
    public static void Error(string message, Exception e = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        logInterfaces.ForEach(x => x.Error(message, e, memberName, filePath, lineNumber));
    }

    public static void Property(string message, object _obj, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        logInterfaces.ForEach(x => x.Property(message, _obj, memberName, filePath, lineNumber));
    }

    public static void ShutdownLogger([CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        logInterfaces.ForEach(x => x.ShutdownLogger(memberName, filePath, lineNumber));
    }

    #endregion Public methods
}