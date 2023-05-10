using System;
using System.Runtime.CompilerServices;

namespace RefconGatewayBase.Logging;

internal class LogConsole : ILog
{
    public bool IsConfigured()
    {
        return true;
    }

    public void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        WriteToConsole(message, ConsoleColor.DarkYellow);
    }

    public void Error(string message, Exception e = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        WriteToConsole($"{message} {e}", ConsoleColor.Red);
    }

    //$"{logEvent.Message} {logEvent?.ObjectValue?.ToString()}{logEvent?.Exception}"

    public void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        WriteToConsole(message, ConsoleColor.White);
    }

    public void Property(string message, object _obj, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        WriteToConsole($"{message} {_obj}", ConsoleColor.Green);
    }

    public void Warning(string message, Exception e = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        WriteToConsole(message, ConsoleColor.Yellow);
    }

    public void ShutdownLogger([CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        WriteToConsole("Shutting down logger", ConsoleColor.DarkGray);
    }

    private void WriteToConsole(string message, ConsoleColor consoleColor)
    {
        Console.ForegroundColor = consoleColor;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}