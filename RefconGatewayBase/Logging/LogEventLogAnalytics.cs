using System;
using System.Linq;
using Newtonsoft.Json;

namespace RefconGatewayBase.Logging;

internal class LogEventLogAnalytics
{
    public string File;
    public int LineNumber;
    public string MemberName;
    public string Message;

    public LogEventLogAnalytics(Log.LogType logType, string message, Exception exception, string memberName, string filePath, int lineNumber, object obj = null)
    {
        Type = logType;
        Message = message;
        MemberName = memberName;
        File = filePath.Split('\\').LastOrDefault();
        LineNumber = lineNumber;
        this.exception = exception;
        ObjectValue = obj;
        Created = DateTime.UtcNow;
    }

    public string System { get; set; }

    [JsonIgnore] public DateTime Created { get; set; }

    [JsonIgnore] public Log.LogType Type { get; }

    private Exception exception { get; }

    public string Exception
    {
        get
        {
            if (exception != null) { return exception.ToString(); }

            return null;
        }
    }

    public string ExceptionMessage
    {
        get
        {
            if (exception != null) { return exception.Message; }

            return null;
        }
    }

    public object ObjectValue { get; set; }

    public string ObjectType
    {
        get
        {
            if (ObjectValue != null) { return ObjectValue.GetType().ToString(); }

            return null;
        }
    }
}