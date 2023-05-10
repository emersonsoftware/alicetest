using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using RefconGatewayBase.Configuration;

namespace RefconGatewayBase.Logging;

internal class LogAnalytics : ILog
{
    private static string WorkspaceId;
    private static string SharedKey;
    private static string SystemName;
    private static DateTime SystemNameUpdated;

    private HttpClient httpClient;
    private readonly object httpClientLock = new();

    #region Public methods

    public bool IsDebugEnabled => true;

    public bool IsConfigured()
    {
        return httpClient != null && WorkspaceId != null && SharedKey != null && SystemName != null;
    }

    public void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        AddEvent(new LogEventLogAnalytics(Log.LogType.Debug, message, null, memberName, filePath, lineNumber));
    }

    public void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        AddEvent(new LogEventLogAnalytics(Log.LogType.Info, message, null, memberName, filePath, lineNumber));
    }

    public void Warning(string message, Exception e = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        AddEvent(new LogEventLogAnalytics(Log.LogType.Warning, message, e, memberName, filePath, lineNumber));
    }

    public void Error(string message, Exception e = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        AddEvent(new LogEventLogAnalytics(Log.LogType.Error, message, e, memberName, filePath, lineNumber));
    }

    public void Property(string message, object obj, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        AddEvent(new LogEventLogAnalytics(Log.LogType.Property, message, null, memberName, filePath, lineNumber, obj));
    }

    public void ShutdownLogger([CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        httpClient.Dispose();
        httpClient = null;
    }

    #endregion Public methods

    #region Private methods

    private void AddEvent(LogEventLogAnalytics logEvent)
    {
        try
        {
            logEvent.System = GetSystemName();

            LogToAnalytics(logEvent);
        }
        catch
        {
            // Logging should never make the program fail
        }
    }

    private void LogToAnalytics(LogEventLogAnalytics logEvent)
    {
        var json = JsonConvert.SerializeObject(logEvent);
        // Create a hash for the API signature
        var datestring = logEvent.Created.ToString("r");
        var jsonBytes = Encoding.UTF8.GetBytes(json);
        var stringToHash = $"POST\n{jsonBytes.Length}\napplication/json\nx-ms-date:{datestring}\n/api/logs";
        var hashedString = BuildSignature(stringToHash, GetWorkspaceKey());
        var signature = "SharedKey " + GetWorkspaceId() + ":" + hashedString;

        PostData(logEvent.Type, signature, datestring, json, logEvent.Created);
    }

    private void PostData(Log.LogType logType, string signature, string date, string json, DateTime timeCreated)
    {
        try
        {
            var url = "https://" + GetWorkspaceId() + ".ods.opinsights.azure.com/api/logs?api-version=2016-04-01";

            if (httpClient == null) { httpClient = new HttpClient(); }

            lock (httpClientLock)
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("Log-Type", logType.ToString());
                httpClient.DefaultRequestHeaders.Add("Authorization", signature);
                httpClient.DefaultRequestHeaders.Add("x-ms-date", date);
                httpClient.DefaultRequestHeaders.Add("time-generated-field", timeCreated.ToString("YYYY-MM-DDThh:mm:ssZ", CultureInfo.InvariantCulture));

                var httpContent = new StringContent(json, Encoding.UTF8);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // We don't wait for the response since we want logging to be quick
                httpClient.PostAsync(new Uri(url), httpContent);
            }
        }
        catch (Exception excep) { Console.WriteLine("Logger Exception: " + excep.Message); }
    }

    // Build the API signature
    private static string BuildSignature(string message, string secret)
    {
        var encoding = new ASCIIEncoding();
        var keyByte = Convert.FromBase64String(secret);
        var messageBytes = encoding.GetBytes(message);
        using (var hmacsha256 = new HMACSHA256(keyByte))
        {
            var hash = hmacsha256.ComputeHash(messageBytes);
            return Convert.ToBase64String(hash);
        }
    }

    private static string GetSystemName()
    {
        if (string.IsNullOrEmpty(SystemName) || SystemNameUpdated.AddMinutes(5) < DateTime.UtcNow)
        {
            var value = Config.LogAnalyticsSystemName;

            if (string.IsNullOrEmpty(value)) { value = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"); }

            if (string.IsNullOrEmpty(value)) { value = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME"); }

            if (string.IsNullOrEmpty(value)) { value = $"localhost - {Environment.MachineName}"; }
            else if (value.Contains("localhost")) { value = $"{value} - {Environment.MachineName}"; }

            SystemName = value;
            SystemNameUpdated = DateTime.UtcNow;
        }

        return SystemName;
    }

    private static string GetWorkspaceId()
    {
        if (WorkspaceId == null) { WorkspaceId = Config.LogWorkpaceId; }

        return WorkspaceId;
    }

    private static string GetWorkspaceKey()
    {
        if (SharedKey == null) { SharedKey = Config.LogWorkspaceKey; }

        return SharedKey;
    }

    #endregion Private methods
}