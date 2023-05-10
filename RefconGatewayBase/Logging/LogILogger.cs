using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace RefconGatewayBase.Logging;

public class LogILogger : ILog
{
    private static ILogger logger;

    #region Public methods

    public bool IsDebugEnabled => true;

    public LogILogger(ILogger _logger)
    {
        logger = _logger;
    }

    public bool IsConfigured()
    {
        return logger != null;
    }

    public void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        logger.LogDebug(message);
    }

    public void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        logger.LogInformation(message);
    }

    public void Warning(string message, Exception e = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        logger.LogWarning(message);
    }

    public void Error(string message, Exception e = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        logger.LogError(message, e);
    }

    public void Property(string message, object _obj, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        logger.LogInformation($"{message} {_obj}");
    }

    public void ShutdownLogger([CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        logger.LogInformation("Shutting down");
    }

    #endregion Public methods
}